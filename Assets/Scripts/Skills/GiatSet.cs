using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 7 - GIUT SET.
///
/// Mot tia set phong tu tay phu thuy sang ke dich gan nhat, roi LAN TIEP tu
/// con do sang con khac o quanh no: moi nhip nhay them mot tia moi. Khac han
/// Sam set - Sam set giang tia tu tren troi xuong nhung diem roi rac, con Giut
/// set la MOT mach dien chay xuyen qua ca dam quai.
///
/// Ba dieu lam no doc ra la "lan" chu khong phai "danh nhieu con cung luc":
///
///   1. MOI NHIP CACH NHAU 0,07 giay. Ve het mot luot trong cung mot khung
///      hinh thi nguoi choi thay mot mang set hien ra dong loat, khong doc duoc
///      thu tu lan.
///   2. Tia moi ve tu con VUA TRUNG chu khong tu tay phu thuy - do la thu noi
///      len mach dien di chuyen.
///   3. SAT THUONG GIAM DAN theo tung nhip, nen con cuoi mach chiu it hon con
///      dau. Khong giam thi ky nang nay manh hon moi ky nang khac trong game.
///
/// Nguoi dung 16/09/2026: tam 20 m (truoc 19,5), sat thuong ban dau 75 (truoc 30), phong CUNG LUC
/// toi da 4 tia neu co 4 ke dich phia truoc trong tam (truoc 3), moi tia van lan nhu cu; MOI CU
/// TRUNG (ca tia lan) 15% gay choang.
///
/// Nguoi dung 24/09/2026: VE LAI giong anh mau - loi trang manh, quang xanh bao quanh, nhieu soi dien
/// re nhanh, cum dien bung o tay va o cho trung (anh dung bang Blender MCP, xem LightningArc.anhBlender);
/// tia HIEN 0,6 GIAY (<see cref="GiayTiaHien"/>, truoc 0,3) va bam theo hai tay / than ke dich; phu thuy
/// day CA HAI TAY ra truoc (NguoiChoiHoatHinh.TuTheGiatSet). Luat choi khong doi.
/// </summary>
public class GiatSet : MonoBehaviour
{
    [Header("Tam va muc tieu")]
    [Tooltip("Tam ban cua tia dau tien, met")]
    // 20 m - nguoi dung chot 16/09/2026 (truoc 19,5 = 13 x 1,5). Khong co prefab: sua o day la an.
    // Sua thi sua ca PlayerController.TamGiatSet (vach ngam ve theo con so ay).
    public float range = TamNguoiChoi;

    /// <summary>
    /// Tam cua Giut set nguoi choi, met. 25/09/2026 nguoi dung: "cho tam danh bang voi tam danh cua skill Sam set" -
    /// chon GIAM 20 -> 12 m (Sam set: PlayerController.boltRange 12, so nam trong prefab Player_Sorceress). Menu 69
    /// so hai con so voi nhau.
    /// </summary>
    public const float TamNguoiChoi = 12f;
    /// <summary>Sat thuong ban dau (cap 1, nhip dau) cua Giut set nguoi choi.</summary>
    public const float SatThuongNguoiChoi = 75f;
    /// <summary>So tia phong cung luc toi da.</summary>
    public const int SoTiaNguoiChoi = 4;
    /// <summary>Xac suat choang moi cu trung (ca tia lan).</summary>
    public const float XacSuatChoangNguoiChoi = 0.15f;
    /// <summary>Choang bao lau (cap ky nang cao cong them 0,15 giay moi cap).</summary>
    public const float GiayChoangNguoiChoi = 1.5f;

    /// <summary>Moi tia hien bao lau, giay (nguoi dung chon 24/09/2026 - tia duy tri nhu anh mau).</summary>
    public const float GiayTiaHien = 0.6f;

    /// <summary>Be ngang dai anh cua tia dau (met) - nguoi dung 25/09/2026 xin manh hon (truoc 1,35).</summary>
    public const float BeNgangTia = 0.70f;

    /// <summary>
    /// Diem giua hai tay nguoi tung (NguoiChoiHoatHinh.DiemGiatSet). Co thi dau tia dau tien bam theo no;
    /// null (quai) thi dau tia dung yen.
    /// </summary>
    public Transform diemTay;

    [Tooltip("Ban kinh tim con tiep theo quanh con vua trung, met")]
    public float chainRadius = 5.5f;

    [Tooltip("So lan LAN THEM sau muc tieu dau tien")]
    public int maxChains = 5;

    [Tooltip("So MACH phong thang tu tay, moi mach di mot muc tieu rieng")]
    public int soTiaDau = SoTiaNguoiChoi;

    [Header("Sat thuong")]
    public float damage = SatThuongNguoiChoi;

    [Tooltip("Xac suat choang MOI CU TRUNG, ke ca tia lan")]
    [Range(0f, 1f)]
    public float xacSuatChoang = XacSuatChoangNguoiChoi;
    public float giayChoang = GiayChoangNguoiChoi;

    /// <summary>Dem cho phep thu: so cu trung / so cu gay choang, tach nhip dau va nhip lan.</summary>
    public static int SoTrungDau, SoChoangDau, SoTrungLan, SoChoangLan;

    [Tooltip("Moi nhip lan, sat thuong con lai bao nhieu phan")]
    [Range(0.3f, 1f)]
    public float damageFalloff = 0.85f;

    [Header("Nhip")]
    [Tooltip("Cach nhau bao lau giua hai nhip lan, giay")]
    public float hopDelay = 0.07f;

    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    /// <summary>Ke BI UOT (mua May giong) an them 50% - chi Giut set cua NGUOI CHOI; PhongCuaQuai tat.</summary>
    public bool tangKhiUot = true;

    [Header("Mau tia")]
    [Tooltip("Loi tia - sang gan nhu trang")]
    public Color mauLoi = Color.white;
    [Tooltip("Quang quanh tia - day moi la mau nguoi choi NHIN THAY")]
    // Xanh dam hon tia cu (0,45 0,70 1): anh mau quang xanh lam ro, chi loi moi trang
    // 25/09/2026 xanh DAM (0,30 0,52 -> 0,14 0,34): quang va hao quang day len ma kenh do/luc con cao thi tam cong
    // lai thanh TRANG (menu 69 muc H: phan trang ban dem 7,0 -> 9,6 diem anh voi mau 0,25 0,45) - chi loi moi duoc trang.
    public Color mauQuang = MauQuangNguoiChoi;

    /// <summary>Mau quang cua Giut set nguoi choi - Qua cau dien cung dung (KieuTia).</summary>
    public static readonly Color MauQuangNguoiChoi = new Color(0.14f, 0.34f, 1f, 1f);

    Vector3 origin, huong;

    /// <summary>So mach chua chay xong - het thi moi xoa vat the.</summary>
    int machConChay;

    static readonly Collider[] buffer = new Collider[64];

    /// <summary>
    /// Phong mot mach Giut set. <paramref name="origin"/> la diem xuat phat
    /// (tay phu thuy), <paramref name="huong"/> la huong nham.
    /// </summary>
    public static GiatSet Phong(Vector3 origin, Vector3 huong, LayerMask damageMask)
    {
        var go = new GameObject("GiatSet");
        go.transform.position = origin;

        var gs = go.AddComponent<GiatSet>();
        gs.origin = origin;
        gs.huong = huong.sqrMagnitude > 0.0001f ? huong.normalized : Vector3.forward;
        gs.damageMask = damageMask;
        return gs;
    }

    /// <summary>
    /// Ban cho QUAI dung: doi mau tia, sat thuong va tam ban.
    ///
    /// Mau nam o QUANG chu khong o loi. Loi tia luon gan nhu trang - do la cach
    /// tia set that trong: cai nguoi choi doc ra mau la vung sang toa quanh no.
    /// Nhuom ca loi thanh xanh la thi tia mat het ve chay bong, nhin ra mot
    /// coong nhua phat sang.
    /// </summary>
    public static GiatSet PhongCuaQuai(Vector3 origin, Vector3 huong, LayerMask damageMask,
                                       float satThuong, float tam, Color loi, Color quang)
    {
        var gs = Phong(origin, huong, damageMask);
        gs.damage = satThuong;
        gs.range = tam;
        gs.mauLoi = loi;
        gs.mauQuang = quang;

        // Quai chi danh MOT nguoi choi, khong co gi de lan sang
        gs.maxChains = 0;
        gs.soTiaDau = 1;
        // Choang 15% la cua KY NANG nguoi choi - don cua quai giu nguyen nhu cu
        gs.xacSuatChoang = 0f;
        gs.tangKhiUot = false;
        return gs;
    }

    void Start()
    {
        StartCoroutine(ChayTatCa());
    }

    /// <summary>
    /// Phong <see cref="soTiaDau"/> mach cung mot luc, moi mach di mot muc tieu
    /// rieng, roi tu do lan tiep nhu cu.
    ///
    /// Ba mach DUNG CHUNG mot danh sach "da trung": mot con quai chi an mot lan
    /// du no nam trong tam cua ca ba mach. Cho moi mach mot danh sach rieng thi
    /// dam quai dung sat nhau an ba lan sat thuong, va mach nay lan sang chinh
    /// con ma mach kia vua danh - nhin ra mot mo tia chong len nhau.
    /// </summary>
    IEnumerator ChayTatCa()
    {
        var daTrung = new HashSet<Damageable>();
        var dauTien = TimMayMucTieuDau(Mathf.Max(1, soTiaDau));

        if (dauTien.Count == 0)
        {
            BanVaoKhongKhi();
            Destroy(gameObject, 0.6f);
            yield break;
        }

        // Danh dau truoc TAT CA muc tieu dau: neu khong, mach thu nhat lan sang
        // dung con ma mach thu hai sap di toi, va mach hai thanh ra khong con
        // gi de danh.
        foreach (var d in dauTien) daTrung.Add(d);

        CameraShake.Shake(0.14f, 0.05f);

        machConChay = dauTien.Count;
        for (int i = 0; i < dauTien.Count; i++)
            StartCoroutine(MotMach(dauTien[i], daTrung));

        while (machConChay > 0) yield return null;
        Destroy(gameObject, 0.6f);
    }

    /// <summary>Mot mach: danh muc tieu dau roi lan tiep sang con quanh no.</summary>
    IEnumerator MotMach(Damageable muc, HashSet<Damageable> daTrung)
    {
        Vector3 tu = origin;
        Transform tuT = diemTay;
        float sat = damage;

        for (int nhip = 0; nhip <= maxChains && muc != null; nhip++)
        {
            Vector3 den = DiemTrung(muc);
            VeTia(tu, den, nhip == 0 ? 1.15f : 0.9f, tuT, muc.transform);
            tuT = muc.transform;
            VfxFactory.SetChayDen(den, 1.25f);
            VfxFactory.NamChuongNgai(den, 2.2f, VfxFactory.LopChuongNgai);

            // Khieng do tron don thi khong dinh choang (luat chung cua CombatUtil.AreaDamage)
            bool khiengDo = muc.khieng != null && muc.khieng.DangBat;
            muc.GhiKeDanh(boQua);
            muc.TakeDamage(tangKhiUot ? sat * BiUot.HeSo(muc) : sat, DamageType.Lightning, den);
            daTrung.Add(muc);

            // 15% CHOANG moi cu trung, gieo RIENG tung cu - ca tia dau lan tia lan.
            // Ban sao mang: StunnedEffect.Apply tu bo qua (may chu so huu gieo, bao qua bit CoChoang).
            if (xacSuatChoang > 0f && !muc.IsDead && !khiengDo)
            {
                if (nhip == 0) SoTrungDau++; else SoTrungLan++;
                if (Random.value < xacSuatChoang)
                {
                    if (nhip == 0) SoChoangDau++; else SoChoangLan++;
                    StunnedEffect.Apply(muc, giayChoang);
                }
            }

            tu = den;
            sat *= damageFalloff;
            muc = TimConTiepTheo(den, daTrung);

            // Danh dau NGAY khi chon: hai mach chay song song, khong danh dau
            // som thi ca hai cung nham vao mot con o nhip sau.
            if (muc != null)
            {
                daTrung.Add(muc);
                yield return new WaitForSeconds(hopDelay);
            }
        }

        machConChay--;
    }

    /// <summary>Khong co ai trong tam: van phong mot tia ra phia truoc roi tat.</summary>
    void BanVaoKhongKhi()
    {
        // Im lang khong lam gi thi nguoi choi tuong nut hong.
        //
        // Tia CHAM VAO DAU thi chay cho do: ban thang vao mot goc tuong hay mot
        // than cay ma khong de lai dau vet gi thi nhin ra tia set di xuyen qua
        // vat the.
        Vector3 den = origin + huong * range * 0.62f;

        RaycastHit hit;
        if (Physics.Raycast(origin, huong, out hit, range * 0.62f,
                            VfxFactory.LopChuongNgai, QueryTriggerInteraction.Ignore))
        {
            den = hit.point;
            VeTia(origin, den, 1f, diemTay, null);
            VfxFactory.NamDen(hit.point, hit.normal, Random.Range(0.40f, 0.70f));
            VfxFactory.SetChayDen(hit.point + hit.normal * 0.1f, 1.0f, 0.7f);
        }
        else VeTia(origin, den, 1f, diemTay, null);
    }

    /// <summary>Diem ngam tren than ke dich, khong phai duoi chan no.</summary>
    static Vector3 DiemTrung(Damageable d)
    {
        var col = d.GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.center;
        return d.transform.position + Vector3.up * 0.9f;
    }

    /// <summary>Dem cho phep thu: so tia da ve.</summary>
    public static int SoTiaDaVe;

    void VeTia(Vector3 tu, Vector3 den, float day, Transform bamDau, Transform bamCuoi)
    {
        SoTiaDaVe++;
        var arc = LightningArc.Create(tu, den, day, GiayTiaHien);
        arc.coreColor = mauLoi;
        arc.glowColor = mauQuang;
        KieuTia(arc, day, bamDau, bamCuoi);
    }

    /// <summary>
    /// Bien <paramref name="arc"/> thanh KIEU TIA GIUT SET: dai anh Blender (loi trang manh, vien xanh, soi re nhanh),
    /// cum dien bung hai dau, bam hai dau, duong gap khuc theo do dai. Khong dung toi mau va thoi gian song - noi goi
    /// tu dat. Qua cau dien cung goi ham nay (nguoi dung 25/09/2026: "tia dien danh ra cung co hieu ung nhu tia set
    /// cua Giut set") - sua kieu tia o DAY la ca hai ky nang cung doi.
    /// </summary>
    public static void KieuTia(LightningArc arc, float day, Transform bamDau, Transform bamCuoi)
    {
        // ANH VE TU BLENDER (nguoi dung 24/09/2026): loi, quang va soi re nhanh nam san trong anh nen duong
        // di chi can gap khuc LON, khuc nho da co trong anh. Thieu anh thi LightningArc tu quay ve cach cu.
        arc.anhBlender = true;
        // Nguoi dung 25/09/2026: tia MANH lai (1,35 -> 0,70 m), cum bung o tay nho lai - xem LightningArc.NapAnh
        arc.beNgang = BeNgangTia * day;
        arc.coBungDau = 0.45f;
        arc.coBungCuoi = 1.2f * day;
        arc.BamHaiDau(bamDau, bamCuoi);
        Vector3 tu = arc.start, den = arc.end;

        // HEP HON HAN cac tia set khac. Giut set phong RA TU TAY nguoi choi:
        // voi be ngang mac dinh (quang 1,05 -> dai rong 2,1 m) doan gan goc
        // trum kin ca dau lan mat nhan vat, con moi khuc gap thi quet ngang qua
        // than. Sam set giang tu troi xuong cach xa nguoi choi nen khong dinh.
        arc.coreWidth = 0.16f * day;
        arc.glowWidth = 0.42f * day;
        // Doan cang dai thi cang nhieu khuc gap, khong thi tia dai thanh mot
        // duong gan nhu thang con tia ngan thi gap khuc lung tung.
        float dai = Vector3.Distance(tu, den);
        arc.segments = Mathf.Clamp(Mathf.RoundToInt(dai * 1.1f), 5, 18);
        arc.jitter = Mathf.Clamp(dai * 0.05f, 0.15f, 0.42f);
        arc.branches = 2;
    }

    /// <summary>
    /// Lay toi da <paramref name="soLuong"/> muc tieu trong tam, xep theo diem.
    ///
    /// Cham diem theo CA hai thu - lech huong bao nhieu va o xa bao nhieu. Chi
    /// lay con gan nhat thi nham ve phia truoc van ban trung con dang dung sau
    /// lung.
    /// </summary>
    List<Damageable> TimMayMucTieuDau(int soLuong)
    {
        int n = Physics.OverlapSphereNonAlloc(origin, range, buffer, damageMask,
                                              QueryTriggerInteraction.Collide);
        ungVien.Clear();

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            // OverlapSphere tra ve TUNG COLLIDER: mot con quai co may collider
            // la no vao danh sach may lan, va ba tia se doi het vao mot con.
            bool trung = false;
            for (int j = 0; j < ungVien.Count; j++)
                if (ungVien[j].ai == d) { trung = true; break; }
            if (trung) continue;

            Vector3 toi = DiemTrung(d) - origin;
            toi.y = 0f;
            float xa = toi.magnitude;
            if (xa < 0.01f) continue;

            float hop = Vector3.Dot(toi / xa, huong);      // -1..1
            if (hop < -0.2f) continue;                     // sau lung han thi bo

            ungVien.Add(new Diem { ai = d, so = hop * 2.4f - xa / Mathf.Max(1f, range) });
        }

        ungVien.Sort(delegate (Diem a, Diem b) { return b.so.CompareTo(a.so); });

        var ra = new List<Damageable>();
        for (int i = 0; i < ungVien.Count && ra.Count < soLuong; i++)
            ra.Add(ungVien[i].ai);
        return ra;
    }

    struct Diem { public Damageable ai; public float so; }
    static readonly List<Diem> ungVien = new List<Diem>();

    /// <summary>Con gan nhat quanh <paramref name="tu"/> ma chua bi trung.</summary>
    Damageable TimConTiepTheo(Vector3 tu, HashSet<Damageable> daTrung)
    {
        int n = Physics.OverlapSphereNonAlloc(tu, chainRadius, buffer, damageMask,
                                              QueryTriggerInteraction.Collide);
        Damageable gan = null;
        float ganNhat = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || daTrung.Contains(d)) continue;
            if (boQua != null && d == boQua) continue;

            float xa = Vector3.Distance(DiemTrung(d), tu);
            if (xa < ganNhat) { ganNhat = xa; gan = d; }
        }
        return gan;
    }
}
