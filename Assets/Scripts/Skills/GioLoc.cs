using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG GIO LOC (so hieu 10, them 16/09/2026).
///
/// Nguoi dung xin: phong ra 3 CON LOC NHO - hinh nhu Loc xoay nhung thap hon nua chieu cao, MAU NAU, van
/// co tia set ben trong; bay nhanh bang Qua cau bang (17 m/s), tu tan sau 3,5 giay; sat thuong goc 75,
/// hoi chieu 0,4 giay; 55% HAT TUNG doi thu 0,5 giay (khong cuon len troi), dang niem chieu thi bi ngat;
/// DI XUYEN moi vat can va nguoi choi khac.
///
/// Nhung cho nguoi dung chon khi toi hoi lai (16/09/2026):
///   - 75 MOT LAN moi muc tieu cho moi con loc (ba loc cung trung mot nguoi = toi da 225).
///   - 55% gieo RIENG moi con loc.
///   - Ton 20 nang luong; niem 0,38 s nhu Qua cau bang; toa quat 11 do nhu Qua cau bang.
///   - Vung trung ban kinh 2,2 m; bi hat cao 1,5 m.
///   - Ngat chieu ca nguoi choi lan quai.
///   - Loc nho DAP TAT LO LUA khi luot qua, lo chay lai sau 30 giay (khong cuon lo, khong cuon canh vat).
///
/// 17/09/2026 nguoi dung doi: loc cu "qua xau" -> DUNG LAI BANG BLENDER MCP (xoay mot chieu tu duoi len, khoi bui den cuon
/// len, mau xam trang nhu Loc xoay, cao ~5 m), BO TIA SET (ca hinh lan sat thuong 15 - nguoi dung chon chi con 75),
/// toc do bay GIAM 25% (17 -> 12,75 m/s). Cung ngay nguoi dung chot lai: 10 m/s; than duoi loc to them 20%.
///
/// Khong co prefab trong GameAssets (them truong prefab la phai sua hai scene) - hinh dung bang code tu tai nguyen
/// Blender: <see cref="VfxFactory.BuildGioLoc"/>.
/// </summary>
public class GioLoc : MonoBehaviour
{
    public const float SatThuongGoc = 75f;
    public const float XacSuatHatTung = 0.55f;
    public const float BanKinhTrung = 2.2f;
    public const float ThoiGianSong = 3.5f;

    /// <summary>Chieu cao hinh loc (luoi Blender) - nguoi dung chon ~5 m.</summary>
    public const float ChieuCao = 5f;

    /// <summary>Toc do bay 10 m/s (nguoi dung 17/09/2026 - truoc do 12,75 = Qua cau bang giam 25%).</summary>
    public const float TocDo = 10f;

    /// <summary>Lo lua bi dap tat bao lau thi chay lai.</summary>
    public const float GiayLoChayLai = 30f;

    public float damage = SatThuongGoc;
    public float xacSuatHatTung = XacSuatHatTung;
    public float giayHatTung = BiHatTung.GiayMacDinh;
    public float banKinh = BanKinhTrung;

    public LayerMask damageMask;
    public Damageable boQua;
    public Vector3 dir = Vector3.forward;

    /// <summary>Bu tre mang: tua nhanh cho kip cho nguoi tung nhin thay (nhu QuaCauBang).</summary>
    public float tuaTruoc;

    float age, loTimer;
    bool daTan;
    GameObject visual;
    readonly HashSet<Damageable> daTrung = new HashSet<Damageable>();

    static readonly Collider[] boDem = new Collider[64];

    // ---- Dem cho phep thu (menu 71) ----
    public static int SoLanTrung, SoLanHat, SoLoDapTat;

    public static GioLoc Spawn(Vector3 pos, Vector3 huong, LayerMask damageMask)
    {
        huong.y = 0f;
        if (huong.sqrMagnitude < 0.001f) huong = Vector3.forward;
        huong.Normalize();

        var go = new GameObject("GioLoc");
        pos.y = VfxFactory.GroundY(pos);
        go.transform.position = pos;
        var g = go.AddComponent<GioLoc>();
        g.dir = huong;
        g.damageMask = damageMask;
        g.visual = VfxFactory.BuildGioLoc();
        g.visual.transform.SetParent(go.transform, false);
        return g;
    }

    /// <summary>Ba con loc toe hinh quat quanh truc DUNG - xem QuaCauBang.SpawnChum.</summary>
    public static void SpawnChum(Vector3 chan, Vector3 huong, LayerMask damageMask, Damageable boQua,
                                 int soLoc = 3, float gocToe = 11f,
                                 float heSoSatThuong = 1f, float themGiay = 0f)
    {
        huong.y = 0f;
        if (huong.sqrMagnitude < 0.001f) huong = Vector3.forward;
        huong.Normalize();
        float giua = (soLoc - 1) * 0.5f;
        for (int i = 0; i < soLoc; i++)
        {
            Vector3 h = Quaternion.AngleAxis((i - giua) * gocToe, Vector3.up) * huong;
            var loc = Spawn(chan, h, damageMask);
            loc.boQua = boQua;
            loc.tuaTruoc = BuTre.TuaTruocGiay;
            loc.damage *= heSoSatThuong;
            loc.giayHatTung += themGiay;     // luat chung: hieu ung +0,15 s moi cap ky nang
        }
        CameraShake.Shake(0.14f, 0.05f);
    }

    void Update()
    {
        if (daTan) return;

        float dt = Time.deltaTime;
        if (tuaTruoc > 0f)
        {
            float them = Mathf.Min(tuaTruoc, 1f / 60f);
            tuaTruoc -= them;
            dt += them;
        }
        age += dt;

        // DI XUYEN moi vat can: khong hoi va cham gi, chi bam mat dat
        Vector3 truoc = transform.position;
        Vector3 sau = truoc + dir * TocDo * dt;
        sau.y = VfxFactory.GroundY(sau);
        transform.position = sau;

        QuetTrung(truoc, sau);

        loTimer -= dt;
        if (loTimer <= 0f) { loTimer = 0.1f; DapTatLo(); }

        if (age >= ThoiGianSong) Tan();
    }

    /// <summary>Quet ca doan vua di (10 m/s, may yeu 10 khung/giay = 1 m moi khung) chu khong chi diem cuoi.</summary>
    void QuetTrung(Vector3 tu, Vector3 den)
    {
        Vector3 a = tu + Vector3.up * 1.0f, b = den + Vector3.up * 1.0f;
        int n = Physics.OverlapCapsuleNonAlloc(a, b, banKinh, boDem, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = boDem[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || d == boQua || daTrung.Contains(d)) continue;
            daTrung.Add(d);
            TrungMot(d);
        }
    }

    void TrungMot(Damageable d)
    {
        SoLanTrung++;
        Vector3 nguc = d.transform.position + Vector3.up * 1.0f;

        // Khieng do tron don thi khong hat tung (luat chung cua CombatUtil.AreaDamage)
        bool khiengDo = d.khieng != null && d.khieng.DangBat;

        d.GhiKeDanh(boQua);
        d.TakeDamage(damage, DamageType.Physical, nguc);

        if (!d.IsDead && !khiengDo && Random.value < xacSuatHatTung)
        {
            if (BiHatTung.Apply(d, giayHatTung) != null) SoLanHat++;
        }
    }

    void DapTatLo()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 1.0f, banKinh, boDem,
                                              1 << 0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            var lo = boDem[i].GetComponentInParent<LoLuaDa>();
            if (lo != null && lo.DangChay) { lo.DapTatRoiChayLai(GiayLoChayLai); SoLoDapTat++; }
        }
    }

    /// <summary>Het 3,5 giay: ngung phun, than loc co lai trong 0,35 giay, hat da phun tu tan.</summary>
    void Tan()
    {
        daTan = true;
        if (visual == null) { Destroy(gameObject); return; }

        float conLauNhat = 0.35f;
        foreach (var ps in visual.GetComponentsInChildren<ParticleSystem>())
        {
            var em = ps.emission;
            em.enabled = false;
            conLauNhat = Mathf.Max(conLauNhat, ps.main.startLifetime.constantMax);
        }
        var co = visual.AddComponent<CoLaiRoiTat>();
        co.thoiGian = 0.35f;
        Destroy(gameObject, conLauNhat + 0.1f);
    }

    /// <summary>Co vo loc va dai xoan lai (hat khong gian the gioi van o yen cho tan).</summary>
    class CoLaiRoiTat : MonoBehaviour
    {
        public float thoiGian = 0.35f;
        float t;
        readonly List<Transform> vo = new List<Transform>();

        void Start()
        {
            foreach (var r in GetComponentsInChildren<MeshRenderer>())
                vo.Add(r.transform);
        }

        void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(1f - t / thoiGian);
            for (int i = 0; i < vo.Count; i++)
                if (vo[i] != null) vo[i].localScale = new Vector3(k, k, k);
        }
    }
}
