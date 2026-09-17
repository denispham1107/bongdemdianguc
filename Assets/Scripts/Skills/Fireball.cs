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
    // 85 - nguoi dung chot 16/09/2026 (truoc 55). PREFAB Skill_QuaCauLua luu san gia tri va DE LEN so nay:
    // sua o day phai sua ca prefab (menu 70 doc thang prefab).
    public float impactDamage = 85f;
    public float blastRadius = 3.4f;
    public float burnSeconds = 3.5f;
    public LayerMask hitMask;
    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    [Header("Tu di muc tieu (Lua dia nguc) - 0 = bay thang nhu Qua cau lua")]
    public float tocQueo = 0f;
    public float tamTim = 20f;
    public float giayBatDauDi = 0f;
    public Damageable mucTieu;
    /// <summary>Danh dau qua cua Lua dia nguc (phep thu dem theo co nay). Mau / vu no giong het Qua cau lua.</summary>
    public bool diaNguc;

    /// <summary>Huong bay hien tai (phep thu doc).</summary>
    public Vector3 HuongBay { get { return dir; } }

    Vector3 dir;
    float age;
    bool exploded;
    static int lopDatDi = -1;

    /// <summary>
    /// Queo huong bay ve muc tieu (nguc ~1 m tren chan) toi da tocQueo do/giay; trong 4 m queo gap doi de khong quay vong quanh
    /// muc tieu (17 m/s va 360 do/giay la ban kinh queo 2,7 m). Muc tieu chet / mat thi tim ke con song gan qua nhat.
    /// </summary>
    /// <summary>Do cao "an toan" tren mat dat tai mot cho: mat dat + 1,3 m (thap hon thi qua cham suon doc luc uon cong).</summary>
    static float CaoAnToan(Vector3 cho, int lopDat)
    {
        RaycastHit h;
        if (Physics.Raycast(cho + Vector3.up * 20f, Vector3.down, out h, 60f, lopDat, QueryTriggerInteraction.Ignore))
            return h.point.y + 1.3f;
        return float.MinValue;
    }

    void DiMucTieu(float dt)
    {
        if (age < giayBatDauDi) return;
        if (mucTieu == null || mucTieu.IsDead || !mucTieu.gameObject.activeInHierarchy)
        {
            var ds = LuaDiaNguc.TimGanNhat(transform.position, damageMask, boQua, tamTim, 1);
            mucTieu = ds.Count > 0 ? ds[0] : null;
            if (mucTieu == null) return;
        }
        Vector3 p = transform.position;
        Vector3 dich = mucTieu.transform.position + Vector3.up * 1.0f;
        Vector3 ngang = new Vector3(dich.x - p.x, 0f, dich.z - p.z);
        float kcNgang = ngang.magnitude;
        // BAM DO CAO TREN MAT DAT (menu 72 do 17/09/2026: bay ngang o do cao luc phong, uon cong tren dat Act2 go ghe thi 7/8 qua
        // dam xuong dat cach 0,04-0,5 m). Con xa: giu cao >= 1,2 m tren mat dat phia truoc 1,5 m; gan (< 2,5 m) moi ha thang vao nguc.
        float yMuon = dich.y;
        if (kcNgang > 2.5f)
        {
            if (lopDatDi < 0) lopDatDi = LayerMask.GetMask("Ground");
            // Do dat o BA cho: ngay duoi qua, 1,5 m theo huong BAY hien tai, va 1,5 m ve phia muc tieu. Luc queo gap (vong ra
            // sau) hai huong ay khac han nhau - chi do mot cho thi qua van cham suon doc (menu 72 do 18/09/2026).
            yMuon = Mathf.Max(yMuon, CaoAnToan(p, lopDatDi));
            yMuon = Mathf.Max(yMuon, CaoAnToan(p + dir * 1.5f, lopDatDi));
            yMuon = Mathf.Max(yMuon, CaoAnToan(p + (kcNgang > 0.01f ? ngang / kcNgang : dir) * 1.5f, lopDatDi));
        }
        Vector3 toi = new Vector3(ngang.x, yMuon - p.y, ngang.z);
        float kc = toi.magnitude;
        if (kc < 0.01f) return;
        float queo = tocQueo * (kc < 4f ? 2f : 1f) * Mathf.Deg2Rad * dt;
        dir = Vector3.RotateTowards(dir, toi / kc, queo, 0f).normalized;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
    }

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

        // Vet lua moi (flipbook lua cuon + vet lua dai dung trong Blender) - thay anh tam giac cu
        VfxFactory.NangCapDuoiLua(go.transform, fb.bodyRadius);

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
    /// <param name="heSoSatThuong">Nhan them theo CAP ky nang cua nguoi tung (xem CapDo).</param>
    /// <param name="themGiayChay">Lua chay them bao nhieu giay - cung theo cap.</param>
    public static void SpawnChum(Vector3 pos, Vector3 direction, LayerMask hitMask,
                                 LayerMask damageMask, Damageable boQua = null,
                                 int soQua = 3, float gocToe = 11f,
                                 float heSoSatThuong = 1f, float themGiayChay = 0f)
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
                qua.impactDamage *= heSoSatThuong;
                qua.burnSeconds += themGiayChay;
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

        if (tocQueo > 0f) DiMucTieu(dt);

        float step = speed * dt;
        Vector3 from = transform.position;

        // Ba thu co the chan qua cau trong buoc nay: vat can, MAT KHIENG, va
        // nguoi/quai. Lay cai GAN NHAT - hoi lan luot roi no ngay o cai dau
        // tien thi mot bia mo NAM SAU vom khieng cung lam qua cau no o bia.
        float ganNhat = float.MaxValue;
        Vector3 choNo = Vector3.zero;

        RaycastHit hit;
        if (Physics.SphereCast(from, bodyRadius, dir, out hit, step + 0.05f, hitMask,
                               QueryTriggerInteraction.Collide))
        {
            ganNhat = hit.distance;
            choNo = hit.point - dir * bodyRadius * 0.5f;
        }

        // KHIENG CUA NGUOI KHAC CHAN QUA CAU. hitMask cua qua cau nguoi choi
        // khong co lop Khieng (xem PlayerController.obstacleMask), nen truoc day
        // no bay xuyen vao trong vom va no ngay tren nguoi chu khieng. Xem
        // Khieng.DanChamVom de biet vi sao hoi thang chu khong qua vat ly.
        float denVom;
        if (Khieng.DanChamVom(from, dir, step + 0.05f, bodyRadius, damageMask, boQua, out denVom)
            && denVom < ganNhat)
        {
            ganNhat = denVom;
            choNo = from + dir * denVom;
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
            if (cham[i].distance >= ganNhat) continue;

            // No o cho vua cham chu khong o cho da bay toi
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

        VfxFactory.ThaDuoiLua(transform);
        VfxFactory.FireExplosion(transform.position, blastRadius);
        CombatUtil.AreaDamage(transform.position, blastRadius, impactDamage, damageMask,
                              DamageType.Fire, burnSeconds, boQua);

        // Don no NGAY TREN MAT KHIENG thi tru mau khieng. AreaDamage o tren
        // khong lo duoc viec nay: no chi tim Damageable trong ban kinh, ma chu
        // khieng dung o TAM vom - xa hon ban kinh no. Xem Khieng.NoTrungKhieng.
        Khieng.NoTrungKhieng(transform.position, blastRadius, impactDamage, damageMask, boQua);

        CameraShake.Shake(0.25f, 0.16f);
        Destroy(gameObject);
    }
}
