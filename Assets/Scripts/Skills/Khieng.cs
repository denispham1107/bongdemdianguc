using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 6 - KHIENG BAO VE.
///
/// Mot qua cau nang luong bao quanh nhan vat. Khieng lam BA viec, va ca ba deu
/// can thiet - thieu mot cai la no khong con la cai khien nua:
///
///   1. HAP THU SAT THUONG. Moi don danh trung nguoi choi deu tru vao mau khieng
///      truoc. Xem cho goi trong <see cref="Damageable.TakeDamage"/>.
///
///   2. CHAN QUAI LAI GAN. Qua cau co collider dac o lop rieng "Khieng"; quai
///      dung phai thi khong vao duoc ben trong, chi dam vao mat cau.
///
///   3. CHAN DAN TU XA. Cau lua cua mu phu thuy no ngay tren mat cau chu khong
///      bay xuyen qua - <see cref="EnemyAI"/> them lop Khieng vao mask cua dan.
///
/// VI SAO PHAI DUNG MOT LOP RIENG: neu de khieng o lop Default thi dan cua CHINH
/// NGUOI CHOI cung no vao no. Dan nguoi choi sinh ra o tay, tuc BEN TRONG qua
/// cau, nen no se dam vao mat trong va no ngay lap tuc - bat khieng len la khong
/// ban ra duoc phat nao. Lop rieng cho phep noi "dan dich thi chan, dan minh thi
/// cho qua".
///
/// DON PHA VO KHIENG KHONG TRAN SANG NGUOI CHOI. Khieng con 5 mau ma an mot don
/// 200 thi nguoi choi van khong mat mau nao - dung nhu yeu cau "khi mau khieng
/// mat het se no tung, va luc do nhan vat moi chiu sat thuong".
/// </summary>
public class Khieng : MonoBehaviour
{
    [Header("Mau khieng")]
    public float mauToiDa = 150f;

    /// <summary>
    /// Mau hien tai. MAC DINH BANG 0 - tuc "chua bat".
    ///
    /// Khong duoc de mac dinh 150. Component nay co the nam san tren nhan vat
    /// (bi them nham vao canh, hoac sot lai tu mot lan chay thu trong Editor),
    /// va neu mac dinh la 150 thi DangBat tra ve true ngay tu khi vao game -
    /// khieng tu bat len du nguoi choi khong bam nut nao. Da vap dung loi do.
    /// Chi <see cref="Bat"/> moi duoc nap mau vao.
    /// </summary>
    public float mau = 0f;

    [Header("Hinh dang")]
    public float banKinh = 2.2f;

    /// <summary>Con mau thi con chan duoc.</summary>
    public bool DangBat { get { return mau > 0f; } }

    /// <summary>Dem cho phep thu (menu 157).</summary>
    public static int SoLanChanHieuUng;

    /// <summary>
    /// DANG BAT KHIEN THI MIEN DICH MOI HIEU UNG (nguoi dung 01/11/2026): choang, dong bang, cham, nga, hat tung, chay, uot, chay den, loc cuon -
    /// goi o dau moi Apply. CHI mien dich, KHONG hoa giai: hieu ung da dinh truoc khi bat van chay tiep (khac TangHinh, thu xoa sach).
    /// </summary>
    public static bool ChanHieuUng(Component c)
    {
        if (c == null) return false;
        var k = c.GetComponent<Khieng>();
        if (k == null || !k.DangBat) return false;
        SoLanChanHieuUng++;
        return true;
    }

    /// <summary>Moc Time.time: truoc moc nay ApKhieng khong dung lai khieng vua bi pha tu goi tin cu tren duong (xem PhaVo).</summary>
    [System.NonSerialized] public float khongBatLaiTruoc;

    /// <summary>
    /// PHA VO NGAY, con bao nhieu mau cung vo (Bo xuong luot xuyen khien, 55% - EnemyAI). Goi ca tren ban sao: may tinh cu luot la chu phong, chua nhan
    /// goi tin cua chu khien; tam ngung ApKhieng 0,5 s de goi "con khien" dang bay khong dung lai khieng vua vo.
    /// </summary>
    public void PhaVo()
    {
        if (!DangBat) return;
        khongBatLaiTruoc = Time.time + 0.5f;
        mau = 0f;
        Vo();
    }

    public float Mau01 { get { return mauToiDa <= 0f ? 0f : Mathf.Clamp01(mau / mauToiDa); } }

    GameObject hinh;
    Material vatLieu;
    SphereCollider vachChan;
    float loe;                       // do loe len sau khi vua chan mot don

    static readonly int idLoe = Shader.PropertyToID("_Loe");
    static readonly int idYeu = Shader.PropertyToID("_Yeu");

    /// <summary>Lop rieng cua khieng - xem ghi chu tren dau lop.</summary>
    public const string TenLop = "Khieng";

    /// <summary>
    /// Moi cai khieng DANG BAT trong man. Giu san mot danh sach thay vi quet ca
    /// canh moi lan co vu no - mot tran co the co hang chuc vu no mot luc.
    /// </summary>
    static readonly List<Khieng> dangBat = new List<Khieng>();

    /// <summary>
    /// Mot vu no gan khieng thi TRU MAU KHIENG, du chu khieng dung ngoai tam no.
    ///
    /// VI SAO CAN HAM NAY: dan cua mu phu thuy no ngay TREN MAT VOM, ma ban
    /// kinh no (2,6 m) lai NHO HON ban kinh khieng (3,04 m) - nen nguoi choi
    /// dung giua khieng khong bao gio nam trong tam sat thuong. Ket qua la
    /// khieng chan duoc het nhung KHONG BAO GIO MAT MAU: bat tu.
    ///
    /// Tra ve so khieng bi tru mau.
    /// </summary>
    // boQua la nguoi tung phep. Khieng CUA HO khong bi tru: truoc day thieu cho
    // nay, cau lua cua minh no trong vong 6,4 m quanh minh la khieng cua minh
    // mat 55 mau - do duoc o menu 49.
    public static int NoTrungKhieng(Vector3 tamNo, float banKinhNo, float satThuong,
                                    LayerMask mask, Damageable boQua = null)
    {
        int trung = 0;
        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!k.DangBat) continue;

            // Chi tinh khieng cua nhung ai nam trong mask sat thuong cua don do
            if ((mask.value & (1 << k.gameObject.layer)) == 0) continue;

            // Khieng cua chinh nguoi tung phep
            if (boQua != null && (k.gameObject == boQua.gameObject || CheDoTran.LaDongDoi(boQua, k.GetComponent<Damageable>()))) continue;

            float d = Vector3.Distance(tamNo, k.transform.position);

            // Chu khieng DA nam trong tam no roi thi don da di qua duong
            // TakeDamage -> HapThu. Tinh them lan nua la tru mau hai lan.
            if (d <= banKinhNo) continue;

            // Vu no co cham toi mat vom khong
            if (d > banKinhNo + k.banKinh) continue;

            k.HapThu(satThuong);
            trung++;
        }
        return trung;
    }

    /// <summary>
    /// Mot vien dan bay tu <paramref name="tu"/> theo <paramref name="huong"/>
    /// (da chuan hoa) them <paramref name="quangDuong"/> met: co cham mat vom
    /// cua ai khong. Tra ve khoang cach toi cho cham GAN NHAT.
    ///
    /// VI SAO KHONG DUNG VA CHAM VAT LY (lop "Khieng" trong mask) nhu dan cua
    /// quai: cau lua cua NGUOI CHOI cung phai bi khieng cua nguoi choi KHAC
    /// chan - truoc day no bay xuyen thang vao trong vom va no cach tam 1,0 m
    /// (menu 49). Nhung ba luat sau phai giu, va hoi thang o day thi ro rang
    /// hon trong cay vao cach Unity xu ly vien cau xuat phat ben trong mot
    /// collider:
    ///   - Khieng CUA NGUOI TUNG thi bo qua (<paramref name="boQua"/>).
    ///   - Chi khieng cua nhung ai nam trong mask sat thuong cua vien dan.
    ///   - Vien dan xuat phat BEN TRONG vom thi vom khong chan no: dung trong
    ///     khieng cua nguoi khac ma ban ra thi van ra duoc.
    /// </summary>
    public static bool DanChamVom(Vector3 tu, Vector3 huong, float quangDuong, float banKinhDan,
                                  LayerMask mask, Damageable boQua, out float khoangCach)
    {
        khoangCach = float.MaxValue;
        bool cham = false;

        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!k.DangBat) continue;
            if ((mask.value & (1 << k.gameObject.layer)) == 0) continue;
            if (boQua != null && (k.gameObject == boQua.gameObject || CheDoTran.LaDongDoi(boQua, k.GetComponent<Damageable>()))) continue;

            // Tam vom o chan nhan vat (xem Dung). Cong ban kinh vien dan: no no
            // khi MAT NGOAI cua no cham vom, khong phai khi tam no cham.
            Vector3 c = k.transform.position;
            float R = k.banKinh + banKinhDan;

            Vector3 oc = tu - c;
            float c2 = oc.sqrMagnitude - R * R;
            if (c2 <= 0f) continue;                  // xuat phat trong vom

            float b = Vector3.Dot(oc, huong);
            if (b >= 0f) continue;                   // dang bay ra xa vom

            float delta = b * b - c2;
            if (delta < 0f) continue;                // bay truot ben canh

            float t = -b - Mathf.Sqrt(delta);
            if (t < 0f || t > quangDuong) continue;

            if (t < khoangCach) { khoangCach = t; cham = true; }
        }
        return cham;
    }

    // ================================================================
    // DON CHI CHAM MAT VOM, KHONG XUYEN VAO TRONG (16/10/2026, nguoi dung: "tat ca tia set, tia dien, bang, gio loc (tru Loc xoay)
    // danh vao nguoi choi dang bat khien thi chi cham vao khien, khong the xuyen vao ben trong"). Truoc day sat thuong da bi khien
    // nuot (HapThu) nhung HINH van di xuyen: tia Giut set / Qua cau dien / Quy cay cham NGUC nguoi ben trong, set Sam set / May
    // giong danh xuong DAT duoi chan, vien bang Mua bang roi vao trong vom, Gio loc bay xuyen qua. Cac ham duoi day tra loi
    // "duong di nay cham mat vom o dau" - noi goi dung o do va ve hieu ung tren mat vom.
    //
    // HINH VOM la ban cau KEO CAO theo VfxFactory.CaoVomKhieng (ngang banKinh, cao banKinh x 1,0957) dat o chan nhan vat - tinh
    // giao diem tren DUNG hinh ay (nen truc y ve hinh cau ban kinh banKinh, tham so doan thang giu nguyen vi phep nen tuyen tinh).

    /// <summary>Chu khien (nguoi dung trong vom).</summary>
    public Damageable ChuNhan { get { if (chuNhan == null) chuNhan = GetComponent<Damageable>(); return chuNhan; } }

    /// <summary>Chieu cao dinh vom (m) so voi chan.</summary>
    public float CaoVom { get { return banKinh * VfxFactory.CaoVomKhieng; } }

    /// <summary>Dem cho phep thu (menu 136): so lan mot don bi chan o mat vom (moi loai don dem o noi goi).</summary>
    public static int SoLanChanMatVom;

    /// <summary>
    /// Khien cua <paramref name="d"/> co CHAN don cua <paramref name="boQua"/> khong: dang bat, va d khong phai nguoi tung / dong doi.
    /// </summary>
    public static Khieng KhiengChan(Damageable d, Damageable boQua)
    {
        if (d == null || d.khieng == null || !d.khieng.DangBat) return null;
        if (CheDoTran.BoQua(boQua, d)) return null;
        return d.khieng;
    }

    /// <summary>
    /// Doan <paramref name="tu"/> -> <paramref name="den"/> di VAO mat vom o dau. <paramref name="s"/> = tham so 0..1 tren doan.
    /// Tra ve false khi: diem dau da o trong vom (dung trong vom ma danh ra thi ra duoc - cung luat voi DanChamVom), doan khong
    /// cham vom, hoac cham o nua DUOI mat dat (vom chi co nua tren).
    /// </summary>
    public bool CatVom(Vector3 tu, Vector3 den, out Vector3 diem, out float s)
    {
        diem = den; s = 1f;
        if (!DangBat) return false;
        Vector3 c = transform.position;
        float R = banKinh;
        float nen = 1f / VfxFactory.CaoVomKhieng;          // nen truc y: elipxoit -> cau ban kinh R
        Vector3 a = tu - c; a.y *= nen;
        Vector3 b = den - c; b.y *= nen;
        float C = a.sqrMagnitude - R * R;
        if (C <= 0f) return false;                          // xuat phat trong vom
        Vector3 d = b - a;
        float A = d.sqrMagnitude;
        if (A < 1e-8f) return false;
        float B = 2f * Vector3.Dot(a, d);
        float delta = B * B - 4f * A * C;
        if (delta < 0f) return false;                       // di truot ben canh
        float t = (-B - Mathf.Sqrt(delta)) / (2f * A);
        if (t < 0f || t > 1f) return false;
        if (a.y + d.y * t < -0.02f) return false;           // vao qua nua duoi (duoi dat)
        s = t;
        diem = Vector3.Lerp(tu, den, t);
        return true;
    }

    /// <summary>Huong tu tam vom toi <paramref name="p"/> trong khong gian DA NEN truc y (vecto don vi) - dung de bo tren mat vom.</summary>
    public Vector3 HuongNen(Vector3 p)
    {
        Vector3 a = p - transform.position;
        a.y /= VfxFactory.CaoVomKhieng;
        return a.sqrMagnitude > 1e-8f ? a.normalized : Vector3.up;
    }

    /// <summary>Diem tren mat vom theo huong da nen <paramref name="u"/> (xem HuongNen).</summary>
    public Vector3 DiemTheoHuong(Vector3 u)
    {
        u.Normalize();
        return transform.position + new Vector3(u.x * banKinh, u.y * CaoVom, u.z * banKinh);
    }

    /// <summary>Phap tuyen mat vom (huong ra ngoai) tai diem <paramref name="p"/> tren mat.</summary>
    public Vector3 PhapTuyen(Vector3 p)
    {
        Vector3 a = p - transform.position;
        float H = CaoVom;
        Vector3 n = new Vector3(a.x / (banKinh * banKinh), a.y / (H * H), a.z / (banKinh * banKinh));
        return n.sqrMagnitude > 1e-10f ? n.normalized : Vector3.up;
    }

    /// <summary>
    /// Doan <paramref name="tu"/> -> <paramref name="den"/> cham khien NAO dau tien (khien dang bat, chu nam trong
    /// <paramref name="mask"/>, khong phai cua nguoi tung / dong doi). Dung cho don KHONG nham vao mot nguoi cu the: set tu troi
    /// (Sam set, May giong), vien bang roi (Mua bang).
    /// </summary>
    public static Khieng DoanChamVom(Vector3 tu, Vector3 den, LayerMask mask, Damageable boQua, out Vector3 diem, out float s)
    {
        diem = den; s = 1f;
        Khieng gan = null;
        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!k.DangBat) continue;
            if ((mask.value & (1 << k.gameObject.layer)) == 0) continue;
            if (boQua != null && (k.gameObject == boQua.gameObject || CheDoTran.LaDongDoi(boQua, k.ChuNhan))) continue;
            Vector3 p; float t;
            if (k.CatVom(tu, den, out p, out t) && t < s) { s = t; diem = p; gan = k; }
        }
        return gan;
    }

    /// <summary>
    /// Diem <paramref name="p"/> (tren mat dat) nam TRONG vom cua khien nao (cung bo loc nhu DoanChamVom) - vom chay toi trum len
    /// mot vat dang roi (nguoi co khien chay vao duoi vien bang). <paramref name="diemMat"/> = diem tren mat vom theo huong tu tam ra p.
    /// </summary>
    public static Khieng TrongVom(Vector3 p, LayerMask mask, Damageable boQua, out Vector3 diemMat)
    {
        diemMat = p;
        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!k.DangBat) continue;
            if ((mask.value & (1 << k.gameObject.layer)) == 0) continue;
            if (boQua != null && (k.gameObject == boQua.gameObject || CheDoTran.LaDongDoi(boQua, k.ChuNhan))) continue;
            Vector3 a = p - k.transform.position;
            if (a.y < 0f) continue;
            a.y /= VfxFactory.CaoVomKhieng;
            if (a.sqrMagnitude >= k.banKinh * k.banKinh) continue;
            diemMat = k.DiemTheoHuong(k.HuongNen(p));
            return k;
        }
        return null;
    }

    static bool LocKhien(Khieng k, LayerMask mask, Damageable boQua)
    {
        if (k == null || !k.DangBat) return false;
        if ((mask.value & (1 << k.gameObject.layer)) == 0) return false;
        if (boQua != null && (k.gameObject == boQua.gameObject || CheDoTran.LaDongDoi(boQua, k.ChuNhan))) return false;
        return true;
    }

    /// <summary>
    /// GIO LOC di tu <paramref name="tu"/> toi <paramref name="den"/> (mat ngang), CHAN than loc ban kinh <paramref name="r"/>: cham vom
    /// khien nao dau tien - than loc cham vom khi tam loc cach tam vom banKinh + r (chan loc la cho rong nhat so voi vom: vom thon
    /// dan len tren nhanh hon than loc loe ra). Bo qua khien trong <paramref name="boRa"/> (vom da trum loc ngay luc sinh - dung trong
    /// vom ma tung ra thi ra duoc, cung luat DanChamVom). Tam loc da nam trong vong ay (vom chay toi trum len loc) = cham ngay o tu.
    /// </summary>
    public static Khieng LocChamVom(Vector3 tu, Vector3 den, float r, LayerMask mask, Damageable boQua,
                                    ICollection<Khieng> boRa, out Vector3 cho)
    {
        cho = den;
        Khieng gan = null;
        float sMin = 2f;
        Vector2 a = new Vector2(tu.x, tu.z), d = new Vector2(den.x - tu.x, den.z - tu.z);
        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!LocKhien(k, mask, boQua)) continue;
            if (boRa != null && boRa.Contains(k)) continue;
            Vector2 ac = a - new Vector2(k.transform.position.x, k.transform.position.z);
            float R = k.banKinh + r;
            float C = ac.sqrMagnitude - R * R;
            float t;
            if (C <= 0f) t = 0f;
            else
            {
                float A = d.sqrMagnitude;
                if (A < 1e-8f) continue;
                float B = 2f * Vector2.Dot(ac, d);
                float delta = B * B - 4f * A * C;
                if (delta < 0f) continue;
                t = (-B - Mathf.Sqrt(delta)) / (2f * A);
                if (t < 0f || t > 1f) continue;
            }
            if (t < sMin) { sMin = t; gan = k; }
        }
        if (gan != null) cho = Vector3.Lerp(tu, den, sMin);
        return gan;
    }

    /// <summary>Moi khien (qua bo loc) co vong ngang banKinh + r trum diem <paramref name="p"/> - them vao <paramref name="ra"/>.</summary>
    public static void VomTrumLoc(Vector3 p, float r, LayerMask mask, Damageable boQua, ICollection<Khieng> ra)
    {
        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (!LocKhien(k, mask, boQua)) continue;
            Vector3 v = p - k.transform.position; v.y = 0f;
            if (v.magnitude <= k.banKinh + r) ra.Add(k);
        }
    }

    /// <summary>
    /// Danh SAT THUONG cua mot don vao khien (don da cham mat vom): di qua TakeDamage cua chu khien -> HapThu nuot het, loe vom,
    /// keu "boong". Ghi ke danh truoc (luat chung, menu 61). Khong gay hieu ung gi - hieu ung khong lot qua vom.
    /// </summary>
    public void TrungMatVom(float satThuong, DamageType loai, Vector3 diem, Damageable boQua, HeSat? he = null)
    {
        var chu = ChuNhan;
        SoLanChanMatVom++;
        if (chu == null || chu.IsDead || satThuong <= 0f) { if (DangBat) HapThu(satThuong); return; }
        if (he.HasValue) chu.GhiKeDanh(boQua, he.Value); else chu.GhiKeDanh(boQua);
        chu.TakeDamage(satThuong, loai, diem);
    }

    // ================================================================

    /// <summary>
    /// Bat khieng len cho <paramref name="nguoi"/>. Dang co khieng thi NAP LAI
    /// day mau chu khong chong them mot cai nua.
    /// </summary>
    public static Khieng Bat(GameObject nguoi, float mauKhieng, float banKinh)
    {
        var k = nguoi.GetComponent<Khieng>();
        if (k == null) k = nguoi.AddComponent<Khieng>();

        k.mauToiDa = mauKhieng;
        k.mau = mauKhieng;
        k.banKinh = banKinh;
        k.enabled = true;
        k.Dung();
        TiengViTri.KhiengBat(nguoi.transform);     // tieng bat khien (16/10/2026) - ca ban sao (HieuUngQuaMang.ApKhieng goi Bat)
        return k;
    }

    void Dung()
    {
        DonBangVaCham();

        // Don sach VOM CU truoc khi dung vom moi.
        //
        // 'hinh' la bien private khong luu vao canh, nen sau khi vao Play mode
        // no luon bang null - trong khi GameObject con "Khieng" tu lan truoc
        // VAN CON do. Khong don thi moi lan bat lai la chong them mot vom nua,
        // va vom cu khong con ai tat duoc: nguoi choi thay khieng vo roi lai
        // hien ra nhu chua he vo.
        if (hinh == null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var con = transform.GetChild(i);
                if (con.name != "Khieng" && con.name != "VachChanKhieng") continue;
                if (Application.isPlaying) Destroy(con.gameObject);
                else DestroyImmediate(con.gameObject);
            }
            vachChan = null;
        }

        if (hinh == null)
        {
            hinh = VfxFactory.BuildKhieng(transform, banKinh);
            // Ban RIENG cho tung cai khieng: moi cai co muc mau va do loe khac
            // nhau, dung chung mot vat lieu thi chung dam len nhau.
            //
            // Ngoai Play mode thi KHONG duoc goi .material - no nhan ban vat lieu
            // va bo lai trong canh, Unity se keu "leak materials into the scene".
            var mr = hinh.GetComponentInChildren<MeshRenderer>();
            if (mr != null)
                vatLieu = Application.isPlaying ? mr.material : mr.sharedMaterial;
        }
        hinh.SetActive(true);

        if (!dangBat.Contains(this)) dangBat.Add(this);

        if (vachChan == null)
        {
            var go = new GameObject("VachChanKhieng");
            go.transform.SetParent(transform, false);
            go.layer = LayerMask.NameToLayer(TenLop);
            vachChan = go.AddComponent<SphereCollider>();
        }

        // Tam vach chan phai TRUNG voi tam qua cau nhin thay duoc, khong thi
        // quai bi chan o mot vong khong khop voi cho nguoi choi thay.
        //
        // Van dung hinh CAU du qua cau ve ra hoi det: SphereCollider khong bop
        // det duoc, ma lay ban kinh ngang thi vung chan hoi cao hon vom mot chut
        // - thua con hon thieu, quai khong loi vao duoc qua mep tren.
        // Tam o CHAN nhan vat, dung bang chan vom. Nua duoi hinh cau chim
        // xuong duoi dat nen khong anh huong gi - quai van bi chan dung o mep
        // vom theo phuong ngang.
        vachChan.center = Vector3.zero;
        vachChan.radius = banKinh;
        vachChan.enabled = true;

        CapNhatVatLieu();
    }

    /// <summary>
    /// Dat bang va cham cho lop Khieng. Phai lam bang code luc chay: bang trong
    /// Project Settings khong tu co dong nao cho mot lop vua duoc them vao.
    /// </summary>
    static bool daDonBang;
    static void DonBangVaCham()
    {
        if (daDonBang) return;
        daDonBang = true;

        int kh = LayerMask.NameToLayer(TenLop);
        if (kh < 0) { Debug.LogError("[Khieng] Chua co lop '" + TenLop + "'"); return; }

        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Enemy"), false);  // CHAN quai
        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Player"), true);
        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Ground"), true);
        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Spell"), true);
        Physics.IgnoreLayerCollision(kh, 0, true);
        Physics.IgnoreLayerCollision(kh, kh, true);
    }

    // ================================================================

    /// <summary>
    /// Khieng nuot bot sat thuong. Tra ve phan CON LAI danh vao nguoi choi.
    ///
    /// Luon tra ve 0 khi khieng con bat: don pha vo khieng khong tran sang
    /// nguoi choi.
    /// </summary>
    public float HapThu(float amount)
    {
        if (!DangBat || amount <= 0f) return amount;

        // Khieng cua BAN SAO van chan don (don khong di xuyen qua vom), nhung
        // KHONG tu tru mau: mau khieng that nam o may cua chu nhan, den day qua
        // goi tin. Tru o day nua thi khieng vo tren may nay trong khi ben kia
        // con nua mau - dung loi nguoi choi thay.
        if (chuNhan == null) chuNhan = GetComponent<Damageable>();
        KeuTrung();
        if (chuNhan != null && chuNhan.mauDoMayKhacQuyet) { loe = 1f; return 0f; }

        mau -= amount;
        loe = 1f;

        if (mau <= 0f)
        {
            mau = 0f;
            Vo();
        }
        else CapNhatVatLieu();

        return 0f;
    }

    // ---------------- TIENG "BOONG" khi don danh trung khien (16/10/2026, nguoi dung) ----------------
    /// <summary>Hai don cung khung (vd mot vu no + vien dan) chi mot tieng.</summary>
    const float GiayGiuaHaiTieng = 0.06f;
    /// <summary>Ban sao: khien tut mau theo goi tin ma may nay KHONG thay don nao vua dap vao (don o may kia) thi keu - neu tut it nhat chung nay mau
    /// (nhip chay / loc cuon ri rat nho thi khong keu "boong" lien tuc).</summary>
    const float MauTutDeKeu = 5f;
    float lucKeuTrung = -10f;
    public static int SoLanKeuTrung;

    void KeuTrung()
    {
        // Sat thuong RI (nhip chay, vung lua, loc cuon...) khong phai "don danh trung khien"
        if (Damageable.LaSatThuongRi) return;
        if (Time.time - lucKeuTrung < GiayGiuaHaiTieng) return;
        lucKeuTrung = Time.time;
        SoLanKeuTrung++;
        TiengViTri.KhiengTrung(transform);
    }

    void Update()
    {
        if (loe > 0f)
        {
            loe = Mathf.MoveTowards(loe, 0f, Time.deltaTime * 3.2f);
            if (vatLieu != null) vatLieu.SetFloat(idLoe, loe);
        }

        // Khieng tho nhe cho no khong dung im nhu mot khoi thuy tinh
        if (hinh != null && DangBat)
        {
            float n = 1f + Mathf.Sin(Time.time * 1.7f) * 0.018f;
            hinh.transform.localScale = Vector3.one * n;
        }
    }

    void CapNhatVatLieu()
    {
        if (vatLieu == null) return;
        vatLieu.SetFloat(idYeu, Mau01);
        vatLieu.SetFloat(idLoe, loe);
    }

    /// <summary>Het mau: no tung roi tat han.</summary>
    void Vo()
    {
        Vector3 tam = transform.position + Vector3.up * banKinh * 0.55f;
        VfxFactory.KhiengVo(tam, banKinh);
        TiengViTri.KhiengVo(tam);
        CameraShake.Shake(0.22f, 0.10f);
        NoGaySatThuong();

        Tat();
    }

    /// <summary>Dem cho phep thu (menu 157).</summary>
    public static int SoLanNo, SoKeTrungNo;

    static readonly Collider[] boDemNo = new Collider[64];
    static readonly HashSet<Damageable> daTrungNo = new HashSet<Damageable>();

    /// <summary>
    /// KHIEN VO TUNG GAY SAT THUONG (nguoi dung 02/11/2026): moi ke dich trong vom (banKinh) an DUNG mauToiDa = mau khien theo cap (225 cap 1 ... 393,5 cap 5),
    /// khong giam dan. Moi lan VO deu no (het mau + Bo xuong xuyen); Tat() thi khong. Chay tren MOI may co khien / ban sao khien vo: moi may chi tru that nhung
    /// nhan vat no quyet mau (quai o chu phong, nguoi choi o may ho) - y het phep phat lai - nen mauToiDa cua ban sao phai dung cap (DatMauToiDaTuMang).
    /// </summary>
    void NoGaySatThuong()
    {
        var chu = ChuNhan;
        if (chu == null || chu.IsDead) return;
        SoLanNo++;
        var pc = GetComponent<PlayerController>();
        int mat = pc != null ? pc.MatNaKeThu : LayerMask.GetMask("Enemy", "Player");
        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up, banKinh, boDemNo, mat, QueryTriggerInteraction.Collide);
        daTrungNo.Clear();
        for (int i = 0; i < n; i++)
        {
            var d = boDemNo[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || CheDoTran.BoQua(chu, d) || !daTrungNo.Add(d)) continue;
            SoKeTrungNo++;
            d.GhiKeDanh(chu);
            d.TakeDamage(mauToiDa, DamageType.Physical, d.transform.position + Vector3.up);
        }
    }

    /// <summary>Ban sao: dat mau toi da theo CAP cua nguoi bat (cap di theo goi phep), giu nguyen ti le mau dang co.</summary>
    public void DatMauToiDaTuMang(float toiDa)
    {
        float tiLe = Mau01;
        mauToiDa = toiDa;
        if (DangBat) mau = Mathf.Max(0.01f, tiLe * toiDa);
    }

    Damageable chuNhan;

    /// <summary>Dat mau khieng theo loi ke cua may chu nhan. Chi ban sao dung.</summary>
    public void DatMauTuMang(float mau01)
    {
        if (!DangBat) return;
        float moi = Mathf.Clamp01(mau01) * mauToiDa;
        if (moi < mau - 0.5f) loe = 1f;        // vua an don ben kia - loe len cho thay
        // tieng "boong": don dap vao o MAY KIA (may nay khong thay - HapThu chua keu trong 0,35 s qua), tut du nhieu (khong phai nhip ri)
        if (moi < mau - MauTutDeKeu && Time.time - lucKeuTrung > 0.35f) KeuTrung();
        mau = Mathf.Max(0.01f, moi);
        CapNhatVatLieu();
    }

    /// <summary>May chu nhan bao khieng da vo - vo theo, co hieu ung, chu khong
    /// tat im lang: nguoi xem phai thay no vo dung luc no vo that.</summary>
    public void VoTuMang()
    {
        if (!DangBat) return;
        mau = 0f;
        Vo();
    }

    public void Tat()
    {
        dangBat.Remove(this);
        mau = 0f;
        if (hinh != null) hinh.SetActive(false);
        if (vachChan != null) vachChan.enabled = false;
    }

    void OnDisable()
    {
        dangBat.Remove(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, banKinh);
    }
}
