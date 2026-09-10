using UnityEngine;

/// <summary>
/// KY NANG 1 - BAN QUA CAU LUA.
/// Qua cau bay thang, keo theo duoi lua va khoi, cham vao dau la no ra
/// mot vung lua lam bong chay ke dich xung quanh.
/// </summary>
public class Fireball : MonoBehaviour
{
    [Header("Bay")]
    public float speed = 17f;
    public float lifetime = 4f;
    public float bodyRadius = 0.30f;

    [Header("Sat thuong")]
    public float impactDamage = 55f;
    public float blastRadius = 3.4f;
    public float burnSeconds = 3.5f;
    public LayerMask hitMask;
    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    Vector3 dir;
    float age;
    bool exploded;

    public static Fireball Spawn(Vector3 pos, Vector3 direction, LayerMask hitMask, LayerMask damageMask)
    {
        GameObject go;
        Fireball fb;

        var pf = GameAssets.I != null ? GameAssets.I.fireballPrefab : null;
        if (pf != null)
        {
            // Dung PREFAB co san trong Assets/Prefabs
            go = GameAssets.Make(pf, pos, Quaternion.LookRotation(direction), null);
            fb = go.GetComponent<Fireball>();
            if (fb == null) fb = go.AddComponent<Fireball>();
        }
        else
        {
            // Khong co prefab thi dung bang code nhu cu
            go = new GameObject("Fireball");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(direction);
            fb = go.AddComponent<Fireball>();
            VfxFactory.BuildFireballVisual(go.transform, fb.bodyRadius);
        }

        fb.dir = direction.normalized;
        fb.hitMask = hitMask;
        fb.damageMask = damageMask;
        return fb;
    }

    /// <summary>
    /// Ban MOT CHUM ba qua cau lua toe hinh quat ve phia truoc.
    ///
    /// Ba qua bay cung luc, lech nhau <paramref name="gocToe"/> do quanh truc
    /// DUNG - toe ngang chu khong toe len xuong, vi tran nha va mat dat khong
    /// lien quan gi den chuyen nham ngang cua nguoi choi.
    ///
    /// Quay quanh truc dung (Vector3.up) chu khong quanh truc ngang cua nguoi
    /// ban: neu nguoi choi nham chech len hay chech xuong, quay quanh truc cua
    /// ho se lam ba qua toe theo mot mat phang NGHIENG, hai qua bien mot cai
    /// chui xuong dat mot cai bay len troi.
    /// </summary>
    public static void SpawnChum(Vector3 pos, Vector3 direction, LayerMask hitMask,
                                 LayerMask damageMask, Damageable boQua = null,
                                 int soQua = 3, float gocToe = 11f)
    {
        Vector3 huong = direction.normalized;

        // Xep deu hai ben huong ngam: 3 qua -> -1, 0, +1 lan goc toe
        float giua = (soQua - 1) * 0.5f;
        for (int i = 0; i < soQua; i++)
        {
            float goc = (i - giua) * gocToe;
            Vector3 h = Quaternion.AngleAxis(goc, Vector3.up) * huong;
            var qua = Spawn(pos, h, hitMask, damageMask);
            if (qua != null)
            {
                qua.boQua = boQua;
                qua.tuaTruoc = BuTre.TuaTruocGiay;
            }
        }
    }

    /// <summary>
    /// Qua cau nay da "bay" bao lau o may ben kia truoc khi tin den day.
    ///
    /// Dat luc sinh, tieu dan trong nhung khung dau: qua cau duoc chay nhanh
    /// cho duoi kip cai no dang le da di duoc. Khong lam the thi don cua nguoi
    /// o xa luon toi cham hon cai ho nhin thay dung bang do tre duong truyen.
    /// </summary>
    public float tuaTruoc;

    void Update()
    {
        if (exploded) return;

        float dt = Time.deltaTime;

        // Tra no phan duong da mat vi duong truyen. Chia nho tung buoc chu
        // khong nhay mot phat: nhay thang thi no xuyen qua ca tuong lan nguoi.
        if (tuaTruoc > 0f)
        {
            float them = Mathf.Min(tuaTruoc, 1f / 60f);
            tuaTruoc -= them;
            dt += them;
        }
        age += dt;

        float step = speed * dt;
        Vector3 from = transform.position;

        RaycastHit hit;
        if (Physics.SphereCast(from, bodyRadius, dir, out hit, step + 0.05f, hitMask,
                               QueryTriggerInteraction.Collide))
        {
            transform.position = hit.point - dir * bodyRadius * 0.5f;
            Explode();
            return;
        }

        // CHAM AI THI NO NGAY - khong doi cham "vat can".
        //
        // Loi da vap trong tran doi khang: nguoi choi nam o lop Player, ma lop
        // ay khong co trong hitMask (hitMask chi co Enemy/Ground/Default), nen
        // qua cau BAY XUYEN THANG QUA NGUOI roi no o dau do phia sau. Do duoc:
        // ba qua bay ra, qua gan nhat chi cach nguoi 1,67 m, doi phuong mat
        // dung 0 mau.
        //
        // Khong sua bang cach nhet lop Player vao hitMask: qua cau sinh ra
        // ngay ben trong collider cua chinh nguoi tung, lam the thi no no tren
        // dau ho. Nen o day tu hoi: trong tam an don co ai KHONG PHAI nguoi
        // tung khong. Cau hoi nay dung cho ca quai lan nguoi.
        // Va phai QUET CA DOAN DUONG chu khong hoi mot diem: qua cau di
        // speed*dt moi khung, khung hinh tut xuong mot cai la no NHAY QUA nguoi
        // ma khong cham vao dau. Do duoc: cung phep thu ay, lan thi 46 mau lan
        // thi 0 - do dung la dau hieu cua viec nhay qua, khong phai do ngau
        // nhien cua chum ba qua.
        var cham = Physics.SphereCastAll(from, bodyRadius + 0.35f, dir, step + 0.05f,
                                         damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < cham.Length; i++)
        {
            var d = cham[i].collider.GetComponentInParent<Damageable>();
            if (d == null || d == boQua || d.IsDead) continue;

            // No o cho vua cham chu khong o cho da bay toi
            transform.position = cham[i].point != Vector3.zero
                               ? cham[i].point : from + dir * step;
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

        VfxFactory.FireExplosion(transform.position, blastRadius);
        CombatUtil.AreaDamage(transform.position, blastRadius, impactDamage, damageMask,
                              DamageType.Fire, burnSeconds, boQua);

        // Don no NGAY TREN MAT KHIENG thi tru mau khieng. AreaDamage o tren
        // khong lo duoc viec nay: no chi tim Damageable trong ban kinh, ma chu
        // khieng dung o TAM vom - xa hon ban kinh no. Xem Khieng.NoTrungKhieng.
        Khieng.NoTrungKhieng(transform.position, blastRadius, impactDamage, damageMask);

        CameraShake.Shake(0.25f, 0.16f);
        Destroy(gameObject);
    }
}
