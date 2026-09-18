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
/// toc do bay GIAM 25% (17 -> 12,75 m/s). Cung ngay nguoi dung chot lai: 10 m/s roi 8 m/s; than duoi loc to them 20% roi 30%, 40% (so ban goc).
/// Roi: tung ky nang chi ra MOT loc (khong con 3), loc tan sau 4,5 giay; va sua loi LOC TREO LEN MAI NHA (xem MatDatY).
/// Roi: trung doi thu nao thi mot tia set CHI HIEU UNG giat tu than loc sang no + chop / chay sem boc khoi nhu Sam set.
///
/// Khong co prefab trong GameAssets (them truong prefab la phai sua hai scene) - hinh dung bang code tu tai nguyen
/// Blender: <see cref="VfxFactory.BuildGioLoc"/>.
/// </summary>
public class GioLoc : MonoBehaviour
{
    public const float SatThuongGoc = 75f;
    public const float XacSuatHatTung = 0.55f;
    /// <summary>Vung trung 2,42 m (nguoi dung 17/09/2026: toan bo ban kinh loc to them 10% - ca hinh lan vung trung; truoc 2,2).</summary>
    public const float BanKinhTrung = 2.42f;
    /// <summary>Loc tu tan sau 4,5 giay (nguoi dung 17/09/2026, truoc do 3,5).</summary>
    public const float ThoiGianSong = 4.5f;

    /// <summary>Tung ky nang ra bao nhieu loc: 1 (nguoi dung 17/09/2026, truoc do 3).</summary>
    public const int SoLocMoiLan = 1;

    /// <summary>Ky nang CAP 5 (cap toi da): 2 loc SONG SONG, tam cach nhau 4 m, TON GAP DOI nang luong (nguoi dung 17/09/2026;
    /// chon: moi loc tinh rieng - dung giua bi ca hai quet la trung hai lan).</summary>
    public const int CapHaiLoc = 5;
    public const float KhoangCachHaiLoc = 4f;

    public static int SoLocTheoCap(int capKy) { return capKy >= CapHaiLoc ? 2 : SoLocMoiLan; }

    /// <summary>Nhan them vao nang luong (sau he so cap chung): so loc tung ra.</summary>
    public static float HeSoNangLuongTheoCap(int capKy) { return SoLocTheoCap(capKy); }

    /// <summary>CAP 5 chi ton 25 nang luong - con so CO DINH, khong nhan he so cap va khong nhan so loc
    /// (nguoi dung chot 18/09/2026). Truoc do cap 5 ton 20 x 1,4641 x 2 = 58,6.</summary>
    public const float NangLuongCap5 = 25f;

    /// <summary>Nang luong THAT SU phai tra cho mot lan tung, da tinh het moi he so.</summary>
    public static float NangLuongCan(int capKy, float nangLuongGoc, float heSoCapChung)
    {
        if (capKy >= CapHaiLoc) return NangLuongCap5;
        return nangLuongGoc * heSoCapChung * HeSoNangLuongTheoCap(capKy);
    }

    /// <summary>Moi lan danh trung mot ke dich thi nguoi tung hoi bang nay mana - CO DINH cho ca 5 cap
    /// (nguoi dung chot 18/09/2026).</summary>
    public const float ManaHoiMoiLanTrung = 10f;

    /// <summary>Dem cho phep thu (menu 71): tong mana da hoi lai nho danh trung.</summary>
    public static float ManaDaHoi;

    /// <summary>
    /// soLoc loc SONG SONG cung huong, xep ngang (vuong goc huong bay) cach nhau <paramref name="khoangCach"/>, doi xung quanh chan.
    /// </summary>
    public static void SpawnSongSong(Vector3 chan, Vector3 huong, LayerMask damageMask, Damageable boQua,
                                     int soLoc, float khoangCach, float heSoSatThuong = 1f, float themGiay = 0f)
    {
        huong.y = 0f;
        if (huong.sqrMagnitude < 0.001f) huong = Vector3.forward;
        huong.Normalize();
        Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;
        float giua = (soLoc - 1) * 0.5f;
        for (int i = 0; i < soLoc; i++)
        {
            var loc = Spawn(chan + ngang * ((i - giua) * khoangCach), huong, damageMask);
            loc.boQua = boQua;
            loc.lucTung = Time.time;
            loc.tuaTruoc = BuTre.TuaTruocGiay;
            loc.damage *= heSoSatThuong;
            loc.giayHatTung += themGiay;
        }
        CameraShake.Shake(0.14f, 0.05f);
    }

    /// <summary>Chieu cao hinh loc (luoi Blender) - nguoi dung chon ~5 m.</summary>
    public const float ChieuCao = 5f;

    /// <summary>Toc do bay 9,5 m/s (nguoi dung 17/09/2026 - truoc do 12,75 roi 10 roi 8).</summary>
    public const float TocDo = 9.5f;

    /// <summary>Nhip hai tia set trong long loc - bang nhip Loc xoay (Tornado.boltInterval 0,45).</summary>
    public const float NhipSetTrongLoc = 0.45f;

    /// <summary>Lo lua bi dap tat bao lau thi chay lai.</summary>
    public const float GiayLoChayLai = 30f;

    public float damage = SatThuongGoc;
    public float xacSuatHatTung = XacSuatHatTung;
    public float giayHatTung = BiHatTung.GiayMacDinh;
    public float banKinh = BanKinhTrung;

    public LayerMask damageMask;
    public Damageable boQua;

    /// <summary>Time.time luc NGUOI CHOI BAM ra con loc nay. Ky nang "Hoa Loc Xoay" (14) tim lan tung
    /// GAN NHAT cua nguoi ay theo con so nay - cap 5 ra hai loc thi ca hai mang cung mot moc.</summary>
    public float lucTung;
    public Vector3 dir = Vector3.forward;

    /// <summary>Bu tre mang: tua nhanh cho kip cho nguoi tung nhin thay (nhu QuaCauBang).</summary>
    public float tuaTruoc;

    float age, loTimer, setTimer;
    bool daTan;
    /// <summary>Hinh con loc (luoi Blender + he hat) - Hoa Loc Xoay can tat rieng khi doi thanh Loc xoay.</summary>
    public GameObject Hinh { get { return visual; } }

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
        pos.y = MatDatY(pos, pos.y);
        go.transform.position = pos;
        var g = go.AddComponent<GioLoc>();
        g.dir = huong;
        g.damageMask = damageMask;
        g.visual = VfxFactory.BuildGioLoc();
        g.visual.transform.SetParent(go.transform, false);
        return g;
    }

    /// <summary>Mot chum loc toe hinh quat quanh truc DUNG - xem QuaCauBang.SpawnChum. Nguoi choi tung <see cref="SoLocMoiLan"/> loc.</summary>
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

        // DI XUYEN moi vat can: khong hoi va cham gi, chi bam MAT DAT (lop Ground)
        Vector3 truoc = transform.position;
        Vector3 sau = truoc + dir * TocDo * dt;
        sau.y = MatDatY(sau, truoc.y);
        transform.position = sau;

        QuetTrung(truoc, sau);

        loTimer -= dt;
        if (loTimer <= 0f) { loTimer = 0.1f; DapTatLo(); }

        // Hai tia set tu dinh loc danh xuong trong long loc, lien tuc nhu Loc xoay - chi hinh
        setTimer -= dt;
        if (setTimer <= 0f) { setTimer = NhipSetTrongLoc; VfxFactory.GioLocSetTrongLoc(transform); }

        if (age >= ThoiGianSong) Tan();
    }

    static int lopMatDat = -1;

    /// <summary>
    /// Do cao MAT DAT duoi mot diem - CHI lop Ground (dia hinh Act2, dat WorldFactory Act1).
    ///
    /// LOI NGUOI DUNG BAO (17/09/2026): loc trung mot can nha nho thi "tu treo len mai nha". VfxFactory.GroundY chieu tia
    /// xuong ca lop Default (nha mo, bia, da, hang rao) nen di vao nha la no dung len MAI. Loc xuyen vat can thi phai
    /// bam dat ben duoi vat. Tia khong cham dat (ra ngoai ban do) thi giu do cao cu, khong roi ve y = 0.
    /// </summary>
    public static float MatDatY(Vector3 p, float neuKhongCo)
    {
        if (lopMatDat < 0) lopMatDat = LayerMask.GetMask("Ground");
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 30f, Vector3.down, out hit, 80f, lopMatDat, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return neuKhongCo;
    }

    /// <summary>Quet ca doan vua di (8 m/s, may yeu 10 khung/giay = 0,8 m moi khung) chu khong chi diem cuoi.</summary>
    /// <summary>
    /// Trung ke dich thi NGUOI TUNG hoi 10 mana (nguoi dung 18/09/2026).
    ///
    /// Chi cong tren MAY CUA CHINH NGUOI AY: con loc do may khac phat lai (ban sao mang) cung chay
    /// ham nay, nhung mana cua ban sao khong ai nhin thay - cong vao la hai may hien hai con so khac nhau.
    /// </summary>
    void HoiManaChoNguoiTung()
    {
        if (boQua == null) return;
        var pc = boQua.GetComponent<PlayerController>();
        if (pc == null || !pc.tuDocInput) return;
        float truoc = pc.mana;
        pc.mana = Mathf.Min(pc.maxMana, pc.mana + ManaHoiMoiLanTrung);
        ManaDaHoi += pc.mana - truoc;
    }

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

        // (17/09/2026 nguoi dung bo hieu ung tia set + chop + chay sem khi trung doi thu; thay bang 2 tia set trong long loc)
        d.GhiKeDanh(boQua);
        d.TakeDamage(damage, DamageType.Physical, nguc);
        HoiManaChoNguoiTung();

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
        readonly List<Vector3> coGoc = new List<Vector3>();

        void Start()
        {
            foreach (var r in GetComponentsInChildren<MeshRenderer>())
            {
                vo.Add(r.transform);
                coGoc.Add(r.transform.localScale);      // vo da nhan HeSoBanKinhGioLoc theo x/z - co tu co do
            }
        }

        void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(1f - t / thoiGian);
            for (int i = 0; i < vo.Count; i++)
                if (vo[i] != null) vo[i].localScale = coGoc[i] * k;
        }
    }
}
