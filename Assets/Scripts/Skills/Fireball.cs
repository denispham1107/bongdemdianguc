using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 1 - BAN QUA CAU LUA.
/// Qua cau bay thang, keo theo duoi lua va khoi, cham vao dau la no ra
/// mot vung lua lam bong chay ke dich xung quanh.
///
/// 19/09/2026 nguoi dung them hai thu, ca hai deu TAT theo mac dinh nen qua cau cua quai khong dinh gi:
///   - <see cref="ngaXacSuat"/>: Qua cau lua CAP 5 danh nga 30% (PlayerController bat theo cap nguoi tung);
///   - <see cref="xuyenVatNho"/>: qua cua Lua dia nguc bay XUYEN bia, mo, da - cay coi va nha van chan.
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

    /// <summary>
    /// BAY XUYEN VAT CAN NHO (bia, mo, da) - nguoi dung xin cho Lua dia nguc 19/09/2026;
    /// cay coi va nha van chan nhu cu. Qua cau lua thuong de TAT.
    /// </summary>
    public bool xuyenVatNho;

    /// <summary>
    /// QUA NAY (nguoi dung 26/09/2026, Qua cau lua + Qua cau bang): no ma TRUNG it nhat mot ke dich thi bat MOT qua moi bay sang
    /// ke dich GAN NHAT chua bi vu no ay trung, trong TamNay m quanh cho no - 100% sat thuong va hieu ung (chon: 1 lan, 100%, 6 m).
    /// Chi qua cua NGUOI CHOI (Qua cau lua: soLanNay = 1 o PlayerController); quai va Lua dia nguc = 0.
    /// </summary>
    public int soLanNay;
    public const float TamNay = 6f;
    /// <summary>Qua NAY: cac ke da bi vu no truoc trung - khong cham / khong nhan lai (con khong thi qua no ngay tren ke cu).</summary>
    public HashSet<Damageable> khongCham;
    /// <summary>Dem cho phep thu: so qua nay da bat ra.</summary>
    public static int SoLanNay;

    [Header("Danh nga (Qua cau lua cap 5)")]
    /// <summary>Xac suat danh nga moi ke trong vung no. 0 = TAT - mac dinh, va qua cau cua quai di duong nay.</summary>
    [Range(0f, 1f)] public float ngaXacSuat = 0f;
    public float ngaGiay = 1.5f;

    /// <summary>Qua cau lua CAP 5: 30% danh nga (nguoi dung 19/09/2026). Thoi gian nam lay thang cua
    /// Thien thach (ThienThach.NgaGiayNguoiChoi) vi nguoi dung xin "hieu ung giong nhu cua Thien thach".</summary>
    public const int CapDanhNga = 5;
    public const float NgaXacSuatCap5 = 0.30f;

    /// <summary>
    /// Vat can nay co phai VAT NHO khong - do bang KICH THUOC collider, khong theo ten: Act1 dung bang code,
    /// Act2 nhap tu Blender, ten khong lien quan gi den nhau.
    ///
    /// So do 19/09/2026 (cao / ngang, met):
    ///   Act2 nho:  452 bia mo 0,16-3,18 / toi 2,77   229 da 0,12-1,31   10 lo lua 2,33 / 1,00
    ///   Act2 to:   58 cay 6,54-17,47    5 nha mo 4,22-5,15 / 4,37-6,16   hang rao 4,77 / 107,60
    ///   Act1 to:   76 vach da 4,53-14,55
    /// Khe ho giua "bia cao nhat" 3,18 va "vat to thap nhat" 4,22 rong hon 1 m - nguong 4,00 nam giua.
    ///
    /// MAT DAT va NGUOI/QUAI khong bao gio la vat nho: dat thi khong the xuyen, con nguoi/quai thi phai NO.
    /// </summary>
    public const float CaoVatNho = 4f;
    public const float NgangVatNho = 4f;

    static int lopDatXuyen = -1;

    public static bool LaVatNho(Collider c)
    {
        if (c == null || c is TerrainCollider) return false;
        if (lopDatXuyen < 0) lopDatXuyen = LayerMask.NameToLayer("Ground");
        if (c.gameObject.layer == lopDatXuyen) return false;
        if (c.GetComponentInParent<Damageable>() != null) return false;
        // LO LUA DA (2,33 m) nho hon nguong nhung VAN CHAN - nguoi dung 25/09/2026: "cac vat can khac nhu nha,
        // cay coi, lo lua... van cho va cham no nhu binh thuong" (ap cho ca Lua dia nguc, nguoi dung chon).
        if (c.GetComponentInParent<LoLuaDa>() != null) return false;
        Vector3 co = c.bounds.size;
        return co.y < CaoVatNho && Mathf.Max(co.x, co.z) < NgangVatNho;
    }

    /// <summary>
    /// Vat can GAN NHAT chan duong bay trong buoc nay (cap nhat <paramref name="ganNhat"/> / <paramref name="choNo"/>
    /// neu gan hon). <paramref name="xuyen"/> = bo qua vat nho (<see cref="LaVatNho"/>). Dung chung cho Qua cau lua,
    /// Lua dia nguc va Qua cau bang.
    /// </summary>
    /// <param name="boRa">Qua NAY: bo qua than cac ke da trung - mat na vat can cua nguoi choi CO lop Enemy (than quai la "vat can"),
    /// qua nay sinh sat ke vua trung thi no ngay tren than ke ay (menu 84 26/09/2026).</param>
    public static void VatCanChan(Vector3 from, float banKinh, Vector3 huong, float dai, LayerMask mask, bool xuyen,
                                  ref float ganNhat, ref Vector3 choNo, HashSet<Damageable> boRa = null)
    {
        if (!xuyen && boRa == null)
        {
            RaycastHit hit;
            if (Physics.SphereCast(from, banKinh, huong, out hit, dai, mask, QueryTriggerInteraction.Collide)
                && hit.distance < ganNhat)
            {
                ganNhat = hit.distance;
                choNo = hit.point - huong * banKinh * 0.5f;
            }
            return;
        }
        // XUYEN VAT NHO: phai hoi CA DOAN chu khong lay cai gan nhat roi thoi - SphereCast chi tra ve MOT vat,
        // ma vat ay rat hay la cai bia dang dung chan truoc goc cay; bo rieng no di thi qua cau se xuyen luon
        // qua cay phia sau.
        var vc = Physics.SphereCastAll(from, banKinh, huong, dai, mask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < vc.Length; i++)
        {
            if (vc[i].distance >= ganNhat) continue;
            if (xuyen && LaVatNho(vc[i].collider)) continue;
            if (boRa != null)
            {
                var dv = vc[i].collider.GetComponentInParent<Damageable>();
                if (dv != null && boRa.Contains(dv)) continue;
            }
            ganNhat = vc[i].distance;
            // Sinh ra NGAY BEN TRONG vat can thi SphereCastAll tra distance 0 va point (0,0,0) -
            // lay thang diem ay la vu no nhay ve goc toa do ban do.
            Vector3 diem = vc[i].point;
            if (vc[i].distance <= 0.0001f && diem == Vector3.zero) diem = from;
            choNo = diem - huong * banKinh * 0.5f;
        }
    }

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
    public static float CaoAnToan(Vector3 cho, int lopDat)
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
                                 float heSoSatThuong = 1f, float themGiayChay = 0f,
                                 float ngaXacSuat = 0f, float ngaGiay = 1.5f, bool xuyenVatNho = false, int soLanNay = 0)
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
                qua.ngaXacSuat = ngaXacSuat;
                qua.ngaGiay = ngaGiay;
                qua.xuyenVatNho = xuyenVatNho;
                qua.soLanNay = soLanNay;
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

        VatCanChan(from, bodyRadius, dir, step + 0.05f, hitMask, xuyenVatNho, ref ganNhat, ref choNo, khongCham);

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
            if (khongCham != null && khongCham.Contains(d)) continue;
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
        // Ai dang trong vung no (TRUOC khi tru mau: ke chet vi vu no van tinh la "da trung") - de qua nay khong quay lai ho
        HashSet<Damageable> daTrungNo = soLanNay > 0 ? KeTrongVung(transform.position, blastRadius, damageMask, boQua) : null;
        VfxFactory.FireExplosion(transform.position, blastRadius);
        // Qua NAY: khongCham = ke da trung vu no truoc - khong an lai o vu no nay
        CombatUtil.AreaDamage(transform.position, blastRadius, impactDamage, damageMask,
                              DamageType.Fire, burnSeconds, boQua, khongCham);

        // DANH NGA (Qua cau lua cap 5). Gieo RIENG tung muc tieu bang chinh loi gieo cua Thien thach -
        // no da bo qua nguoi tung, ke da chet va ke dang con khien. Goi SAU sat thuong: ke chet vi vu no
        // thi khong nga nua.
        if (ngaXacSuat > 0f)
            ThienThach.GieoDanhNga(transform.position, blastRadius, damageMask, boQua, ngaXacSuat, ngaGiay, khongCham);

        // Don no NGAY TREN MAT KHIENG thi tru mau khieng. AreaDamage o tren
        // khong lo duoc viec nay: no chi tim Damageable trong ban kinh, ma chu
        // khieng dung o TAM vom - xa hon ban kinh no. Xem Khieng.NoTrungKhieng.
        Khieng.NoTrungKhieng(transform.position, blastRadius, impactDamage, damageMask, boQua);

        // QUA NAY: vu no trung it nhat mot ke dich -> bay sang ke gan nhat chua bi trung
        if (daTrungNo != null && daTrungNo.Count > 0)
        {
            if (khongCham != null) daTrungNo.UnionWith(khongCham);
            var ke = TimKeNay(transform.position, damageMask, boQua, daTrungNo);
            if (ke != null)
            {
                Vector3 den = ke.transform.position + Vector3.up;
                Vector3 h = den - transform.position; h = h.sqrMagnitude > 0.01f ? h.normalized : dir;
                // Xuat phat LECH khoi diem no 0,5 m ve phia ke moi: no vi cham vat can thi dat ngay trong vat ay - qua nay no luon
                Vector3 tu = transform.position + h * 0.5f + Vector3.up * 0.2f;
                var q = Spawn(tu, h, hitMask, damageMask);
                q.impactDamage = impactDamage; q.blastRadius = blastRadius; q.burnSeconds = burnSeconds;
                q.boQua = boQua; q.ngaXacSuat = ngaXacSuat; q.ngaGiay = ngaGiay; q.xuyenVatNho = xuyenVatNho;
                q.speed = speed; q.lifetime = 1.5f;
                // Dui theo ke dich (no chay thi van trung) - dung lai co che tu di cua Lua dia nguc
                q.mucTieu = ke; q.tocQueo = 540f; q.giayBatDauDi = 0f; q.tamTim = TamNay;
                q.soLanNay = soLanNay - 1;
                q.khongCham = daTrungNo;
                SoLanNay++;
            }
        }

        CameraShake.Shake(0.25f, 0.16f);
        Destroy(gameObject);
    }

    static readonly Collider[] boNay = new Collider[64];

    /// <summary>Moi Damageable con song (tru boQua) trong vung cau - dung chung cho Qua cau lua / Qua cau bang.</summary>
    public static HashSet<Damageable> KeTrongVung(Vector3 tam, float banKinh, LayerMask mask, Damageable boQua)
    {
        var ra = new HashSet<Damageable>();
        int n = Physics.OverlapSphereNonAlloc(tam, banKinh, boNay, mask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = boNay[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || d == boQua) continue;
            ra.Add(d);
        }
        return ra;
    }

    /// <summary>Ke dich con song GAN NHAT (mat ngang) trong TamNay m quanh <paramref name="tam"/>, khong nam trong <paramref name="boRa"/>.</summary>
    public static Damageable TimKeNay(Vector3 tam, LayerMask mask, Damageable boQua, HashSet<Damageable> boRa)
    {
        int n = Physics.OverlapSphereNonAlloc(tam, TamNay + 1.5f, boNay, mask, QueryTriggerInteraction.Collide);
        Damageable tot = null; float ganNhat = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            var d = boNay[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || d == boQua || (boRa != null && boRa.Contains(d))) continue;
            Vector3 v = d.transform.position - tam; v.y = 0f;
            float kc = v.magnitude;
            if (kc > TamNay || kc >= ganNhat) continue;
            ganNhat = kc; tot = d;
        }
        return tot;
    }
}
