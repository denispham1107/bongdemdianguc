using UnityEngine;

/// <summary>
/// KY NANG "QUA CAU BANG" (so hieu 9, them 16/09/2026).
///
/// Nguoi dung: ba qua cau bang toe ra nhu Qua cau lua; sat thuong ban dau 65; hoi chieu
/// 0,55 giay; trung doi thu thi 40% lam CHAM 50% toc do trong 2 giay; cham la no nhu qua
/// cau lua va gay sat thuong moi ke dich trong vung no.
///
/// Duong bay va cach bat va cham CHEP DUNG Fireball.Update (vat can, mat khieng, quet ca
/// doan duong tim nguoi/quai, tua truoc bu tre mang) - moi cai bay ay da tung vap va co
/// ghi chu day du o Fireball.cs. Khac nhau o hinh anh (VfxQuaCauBang.cs) va vu no.
///
/// Lam cham di qua FrozenEffect.ApCham: ban sao mang KHONG tu gieo (may chu so huu la
/// trong tai, bit CoBang cua goi trang thai mang hieu ung sang may khac) - giong Mua bang.
/// </summary>
public class QuaCauBang : MonoBehaviour
{
    /// <summary>Sat thuong ban dau MOI QUA (cap ky nang 1) - nguoi dung chot 65.</summary>
    public const float SatThuongGoc = 65f;
    /// <summary>Xac suat lam cham moi muc tieu trung don.</summary>
    public const float XacSuatCham = 0.40f;
    /// <summary>Ti le giam toc do.</summary>
    public const float TiLeCham = 0.50f;
    /// <summary>Lam cham keo dai bao lau (cap ky nang cao cong them).</summary>
    public const float GiayCham = 2f;
    /// <summary>Ban kinh vu no - bang Qua cau lua (prefab Skill_QuaCauLua: 3,4 m).</summary>
    public const float BanKinhNo = 3.4f;
    /// <summary>Ban kinh HINH cum gai bang tu dat khi no (IceImpact) - chi la hinh, khong phai vung sat thuong.
    /// Mua bang dung CHUNG con so nay cho cum bang duoi dat (nguoi dung 16/09/2026: "cho bang tao ra tren mat
    /// dat cung to nhu bang cua Qua cau bang").</summary>
    public const float BanKinhHinhBang = BanKinhNo * 0.75f;

    public float speed = 17f;
    public float lifetime = 4f;
    public float bodyRadius = 0.30f;

    public float impactDamage = SatThuongGoc;
    public float blastRadius = BanKinhNo;
    public float giayCham = GiayCham;
    public LayerMask hitMask;
    public LayerMask damageMask;
    public Damageable boQua;

    /// <summary>Qua cau da bay bao lau o may ben kia truoc khi tin den - xem Fireball.tuaTruoc.</summary>
    public float tuaTruoc;

    Vector3 dir;
    float age;
    bool exploded;

    /// <summary>So lan no va so muc tieu bi lam cham tu khi vao Play - cho phep thu doc.</summary>
    public static int SoLanNo, SoLanTrung, SoLanCham;

    static readonly Collider[] buffer = new Collider[64];

    public static QuaCauBang Spawn(Vector3 pos, Vector3 direction, LayerMask hitMask, LayerMask damageMask)
    {
        var go = new GameObject("QuaCauBang");
        go.transform.position = pos;
        go.transform.rotation = Quaternion.LookRotation(direction);
        var qc = go.AddComponent<QuaCauBang>();
        VfxFactory.BuildQuaCauBangVisual(go.transform, qc.bodyRadius);
        qc.dir = direction.normalized;
        qc.hitMask = hitMask;
        qc.damageMask = damageMask;
        return qc;
    }

    /// <summary>Ban MOT CHUM qua cau bang toe hinh quat quanh truc DUNG - xem Fireball.SpawnChum.</summary>
    public static void SpawnChum(Vector3 pos, Vector3 direction, LayerMask hitMask, LayerMask damageMask,
                                 Damageable boQua = null, int soQua = 3, float gocToe = 11f,
                                 float heSoSatThuong = 1f, float themGiayCham = 0f)
    {
        Vector3 huong = direction.normalized;
        float giua = (soQua - 1) * 0.5f;
        for (int i = 0; i < soQua; i++)
        {
            Vector3 h = Quaternion.AngleAxis((i - giua) * gocToe, Vector3.up) * huong;
            var qua = Spawn(pos, h, hitMask, damageMask);
            qua.boQua = boQua;
            qua.tuaTruoc = BuTre.TuaTruocGiay;
            qua.impactDamage *= heSoSatThuong;
            qua.giayCham += themGiayCham;
        }
    }

    void Update()
    {
        if (exploded) return;

        float dt = Time.deltaTime;
        if (tuaTruoc > 0f)
        {
            float them = Mathf.Min(tuaTruoc, 1f / 60f);
            tuaTruoc -= them;
            dt += them;
        }
        age += dt;

        float step = speed * dt;
        Vector3 from = transform.position;
        float ganNhat = float.MaxValue;
        Vector3 choNo = Vector3.zero;

        RaycastHit hit;
        if (Physics.SphereCast(from, bodyRadius, dir, out hit, step + 0.05f, hitMask, QueryTriggerInteraction.Collide))
        {
            ganNhat = hit.distance;
            choNo = hit.point - dir * bodyRadius * 0.5f;
        }

        float denVom;
        if (Khieng.DanChamVom(from, dir, step + 0.05f, bodyRadius, damageMask, boQua, out denVom) && denVom < ganNhat)
        {
            ganNhat = denVom;
            choNo = from + dir * denVom;
        }

        var cham = Physics.SphereCastAll(from, bodyRadius + 0.35f, dir, step + 0.05f, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < cham.Length; i++)
        {
            var d = cham[i].collider.GetComponentInParent<Damageable>();
            if (d == null || d == boQua || d.IsDead) continue;
            if (cham[i].distance >= ganNhat) continue;
            ganNhat = cham[i].distance;
            choNo = cham[i].point != Vector3.zero ? cham[i].point : from + dir * step;
        }

        if (ganNhat < float.MaxValue)
        {
            transform.position = choNo;
            Explode();
            return;
        }

        transform.position = from + dir * step;
        if (age >= lifetime) Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;
        SoLanNo++;

        VfxFactory.ThaDuoiQuaCauBang(transform);
        VfxFactory.NoQuaCauBang(transform.position, blastRadius);
        NoBang(transform.position, blastRadius, impactDamage, damageMask, giayCham, boQua);
        Khieng.NoTrungKhieng(transform.position, blastRadius, impactDamage, damageMask, boQua);

        CameraShake.Shake(0.2f, 0.12f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Sat thuong BANG theo vung + 40% lam cham tung muc tieu (gieo RIENG moi muc tieu).
    /// Giam dan tu tam ra ria nhu CombatUtil.AreaDamage; GhiKeDanh truoc TakeDamage (menu 61);
    /// khieng do tron don thi khong dinh cham (luat chung cua AreaDamage).
    /// </summary>
    public static int NoBang(Vector3 center, float radius, float damage, LayerMask mask,
                             float giayCham, Damageable boQua)
    {
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, mask, QueryTriggerInteraction.Collide);
        int hits = 0;
        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            float dist = Vector3.Distance(center, d.transform.position);
            float falloff = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(dist / Mathf.Max(0.01f, radius)));
            bool khiengDo = d.khieng != null && d.khieng.DangBat;

            d.GhiKeDanh(boQua);
            d.TakeDamage(damage * falloff, DamageType.Ice, d.transform.position + Vector3.up * 1f);
            hits++;
            SoLanTrung++;

            if (d.IsDead || khiengDo) continue;
            if (Random.value < XacSuatCham)
            {
                // Ban sao mang: ApCham tu bo qua - ben kia gieo va bao qua goi trang thai
                if (!d.mauDoMayKhacQuyet) SoLanCham++;
                FrozenEffect.ApCham(d, TiLeCham, giayCham);
            }
        }
        return hits;
    }
}
