using System.Collections;
using UnityEngine;

/// <summary>
/// MAN DANG NHAP / DANG KY - ve bang OnGUI ngay trong game.
///
/// Nguoi choi khong phai roi game de vao trang web nao ca: mo game len la
/// dang ky, dang nhap, roi vao thang sanh phong.
///
/// Trang web chi con mot viec duy nhat: cho admin quan ly tai khoan.
///
/// Ban Editor va ban PC deu chay duoc vi tat ca di qua REST API - xem
/// <see cref="FirebaseMang"/>.
///
/// Giao dien (khung, nut, font co dau tieng Viet) lay tu <see cref="GiaoDien"/>.
/// </summary>
public class ManDangNhap : MonoBehaviour
{
    public enum Trang { DangNhap, DangKy }

    Trang trang = Trang.DangNhap;
    string email = "", matKhau = "", ten = "";
    string bao = "";
    bool baoLaLoi;
    bool dangCho;
    bool daThuTuDangNhap;

    /// <summary>Goi khi dang nhap xong - ManSanh se nghe cai nay.</summary>
    public System.Action daVao;

    void Start()
    {
        // Con phien tu lan truoc thi vao thang, khong bat go lai mat khau
        if (FirebaseMang.CoTheTuDangNhapLai && !FirebaseMang.DaDangNhap)
            StartCoroutine(ThuTuDangNhap());
    }

    void Bao(string chu, bool loi) { bao = chu ?? ""; baoLaLoi = loi; }

    IEnumerator ThuTuDangNhap()
    {
        daThuTuDangNhap = true;
        dangCho = true;
        Bao("Đang khôi phục phiên trước...", false);

        bool ok = false; string loi = null;
        yield return FirebaseMang.TuDangNhapLai((o, e) => { ok = o; loi = e; });

        if (ok)
        {
            yield return HoSoMang.TaiHoacTao(null, (o2, e2) => { ok = o2; loi = e2; });
        }

        dangCho = false;
        if (ok) { Bao("", false); if (daVao != null) daVao(); }
        else { Bao(loi, true); FirebaseMang.Quen(); }
    }

    // ================================================================

    /// <summary>Bo cuc man dang nhap - ham THUAN de phep thu goi duoc.</summary>
    public struct BoCucDangNhap
    {
        public Rect tieuDe;        // anh ten game; width = 0 la khong ve
        public Rect khung;         // khung dang nhap / dang ky
        public Rect oNhapCuoi;     // o "Mat khau" - o de bi ban phim che nhat
        public bool hienTieuDe;
    }

    /// <summary>
    /// Day o mat khau nam cach dinh khung bao nhieu (don vi giao dien).
    ///
    /// Cong tu chinh thu tu ve trong <see cref="OnGUI"/>: 46 (le tren) + 50 (hai
    /// tab) + 26 + [100 neu co o Ten] + 100 (o Email) + 30 (nhan) + 52 (o nhap).
    /// </summary>
    public static float DayONhapCuoi(bool trangDangKy, float s)
    {
        return (304f + (trangDangKy ? 100f : 0f)) * s;
    }

    /// <summary>Phan khung duoc phep tran len khoi mep tren khi ban phim day len:
    /// dung bang hang tab, tuc cung lam la mat hai tab, khong bao gio mat o nhap.</summary>
    public static float TranLenToiDa(float s) { return 122f * s; }

    /// <summary>
    /// XEP MAN DANG NHAP, CO TINH DEN BAN PHIM AO.
    ///
    /// Tren dien thoai, ban phim che kin o nhap va nguoi choi go ma khong thay
    /// minh go gi (nguoi dung gap 12/09/2026). Vung con nhin thay do trang bao
    /// sang qua <see cref="BanPhimAo"/>.
    ///
    /// Ban phim chiem gan nua man hinh nen KHONG DU cho ca anh tieu de lan
    /// khung: ban phim len thi BO anh tieu de va day khung sao cho DAY khung
    /// nam ngay tren mep ban phim.
    ///
    /// Ham thuan (khong doc Screen, khong ve gi) de menu 50 goi thang voi hang
    /// chuc co man hinh va do cao ban phim khac nhau.
    /// </summary>
    public static BoCucDangNhap TinhBoCuc(float W, float H, float s, bool trangDangKy,
                                          float tiLeBanPhimChe, float tiLeAnhTieuDe)
    {
        var b = new BoCucDangNhap();

        float rong = Mathf.Min(W - 40f * s, 600f * s);
        float caoKhung = (trangDangKy ? 616f : 514f) * s;
        float rongTieuDe = Mathf.Min(W - 40f * s, 760f * s);
        float caoAnh = rongTieuDe / Mathf.Max(0.01f, tiLeAnhTieuDe);
        float caoTieuDe = caoAnh + 22f * s;

        float che = Mathf.Clamp01(tiLeBanPhimChe);
        bool coBanPhim = che > 0.05f;
        float nhinThay = H * (1f - che);

        b.hienTieuDe = !coBanPhim;
        float tong = (coBanPhim ? 0f : caoTieuDe) + caoKhung;

        float y = Mathf.Max(16f * s, (H - tong) * 0.5f - 10f * s);
        if (coBanPhim)
        {
            // Truoc het thu day ca khung len tren ban phim...
            y = Mathf.Min(y, nhinThay - caoKhung - 10f * s);

            // ...nhung trang "Tao tai khoan" cao 616 don vi, ban phim che quá
            // nua man hinh thi KHONG DU CHO ca khung. Luc ay uu tien O NHAP:
            // day tiep cho DAY O MAT KHAU nam tren mep ban phim, chap nhan hai
            // TAB tran len khoi mep tren (khong bao gio de mat o nhap).
            float canDay = nhinThay - 8f * s - DayONhapCuoi(trangDangKy, s);
            y = Mathf.Min(y, canDay);
            y = Mathf.Max(-TranLenToiDa(s), y);
        }
        float x = (W - rong) * 0.5f;

        b.tieuDe = coBanPhim ? new Rect(0f, 0f, 0f, 0f)
                             : new Rect((W - rongTieuDe) * 0.5f, y, rongTieuDe, caoAnh);
        if (!coBanPhim) y += caoAnh + 22f * s;
        b.khung = new Rect(x, y, rong, caoKhung);
        b.oNhapCuoi = new Rect(x + 36f * s, y + DayONhapCuoi(trangDangKy, s) - 52f * s,
                               rong - 72f * s, 52f * s);
        return b;
    }

    void OnGUI()
    {
        if (FirebaseMang.DaDangNhap) return;

        GiaoDien.ChuanBi();
        float s = GiaoDien.TiLe;
        // Khong con phu toi bon goc + suong do (VeNen): canh phia sau de nguyen

        var bc = TinhBoCuc(Screen.width, Screen.height, s, trang == Trang.DangKy,
                           BanPhimAo.TiLeChe, GiaoDien.TiLeTieuDe);

        // ---- Ten game (an di khi ban phim dang len - khong du cho) ----
        if (bc.hienTieuDe) GiaoDien.TieuDeGame(bc.tieuDe, s);

        // ---- Khung ----
        var khung = bc.khung;
        float x = khung.x, rong = khung.width, y = khung.y;
        GiaoDien.Khung(khung, s, true);

        float le = 36f * s;
        float xx = x + le, rr = rong - 2f * le;
        float yy = y + 46f * s;

        // Hai tab
        float rongTab = rr * 0.5f;
        GUI.enabled = !dangCho;
        if (GiaoDien.Tab(new Rect(xx, yy, rongTab, 50f * s), "ĐĂNG NHẬP", trang == Trang.DangNhap, s))
        { trang = Trang.DangNhap; Bao("", false); }
        if (GiaoDien.Tab(new Rect(xx + rongTab, yy, rongTab, 50f * s), "TẠO TÀI KHOẢN", trang == Trang.DangKy, s))
        { trang = Trang.DangKy; Bao("", false); }
        yy += 50f * s + 26f * s;

        if (trang == Trang.DangKy)
        {
            GiaoDien.NhanO(new Rect(xx, yy, rr, 26f * s), "Tên trong game (2–16 ký tự)");
            yy += 30f * s;
            ten = GiaoDien.ONhap(new Rect(xx, yy, rr, 52f * s), "o_ten", ten, 16, "Tên người khác sẽ thấy");
            yy += 52f * s + 18f * s;
        }

        GiaoDien.NhanO(new Rect(xx, yy, rr, 26f * s), "Email");
        yy += 30f * s;
        email = GiaoDien.ONhap(new Rect(xx, yy, rr, 52f * s), "o_email", email, 64, "ten@email.com");
        yy += 52f * s + 18f * s;

        GiaoDien.NhanO(new Rect(xx, yy, rr, 26f * s), "Mật khẩu (ít nhất 6 ký tự)");
        yy += 30f * s;
        matKhau = GiaoDien.ONhap(new Rect(xx, yy, rr, 52f * s), "o_matkhau", matKhau, 64, "", true);
        yy += 52f * s + 30f * s;

        string chuNut = dangCho
            ? "ĐANG CHỜ..."
            : (trang == Trang.DangKy ? "TẠO TÀI KHOẢN" : "VÀO GAME");

        if (GiaoDien.Nut(new Rect(xx, yy, rr, 62f * s), chuNut, GiaoDien.KieuNutMau) && !dangCho)
            BamNutChinh();
        GUI.enabled = true;
        yy += 62f * s + 16f * s;

        // Thong bao - nhieu dong, chu do khi la loi
        if (!string.IsNullOrEmpty(bao))
        {
            var k = GiaoDien.KieuChuMo;
            var mauCu = k.normal.textColor;
            var canh = k.alignment;
            k.normal.textColor = baoLaLoi ? GiaoDien.MauLoi : GiaoDien.MauMo;
            k.alignment = TextAnchor.UpperCenter;
            GiaoDien.ChuNhieuDong(new Rect(xx, yy, rr, khung.yMax - yy - 12f * s), bao, k);
            k.normal.textColor = mauCu;
            k.alignment = canh;
        }

        // Goi y o day man hinh
        var kd = GiaoDien.KieuChuNho;
        var cd = kd.alignment;
        var md = kd.normal.textColor;
        kd.alignment = TextAnchor.MiddleCenter;
        kd.normal.textColor = GiaoDien.MauToi;
        GiaoDien.Chu(new Rect(0f, Mathf.Min(Screen.height - 40f * s, khung.yMax + 18f * s), Screen.width, 28f * s),
                     "Nhấn Enter để xác nhận", kd);
        kd.alignment = cd;
        kd.normal.textColor = md;

        // Enter de gui - go xong mat khau la bam Enter, khong phai re chuot
        var e = Event.current;
        if (e.type == EventType.KeyDown
            && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            && !dangCho)
        {
            BamNutChinh();
            e.Use();
        }
    }

    void BamNutChinh()
    {
        if (trang == Trang.DangKy && ten.Trim().Length < 2)
        { Bao("Tên phải từ 2 ký tự trở lên.", true); return; }
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        { Bao("Email không hợp lệ.", true); return; }
        if (matKhau.Length < 6)
        { Bao("Mật khẩu phải từ 6 ký tự trở lên.", true); return; }

        StartCoroutine(trang == Trang.DangKy ? ChayDangKy() : ChayDangNhap());
    }

    IEnumerator ChayDangKy()
    {
        dangCho = true; Bao("Đang tạo tài khoản...", false);
        bool ok = false; string loi = null;

        yield return FirebaseMang.DangKy(email.Trim(), matKhau, (o, e) => { ok = o; loi = e; });

        if (ok)
            yield return HoSoMang.TaiHoacTao(ten.Trim(), (o, e) => { ok = o; loi = e; });

        dangCho = false;
        if (ok) { Bao("", false); if (daVao != null) daVao(); }
        else Bao(loi ?? "Không tạo được tài khoản.", true);
    }

    IEnumerator ChayDangNhap()
    {
        dangCho = true; Bao("Đang đăng nhập...", false);
        bool ok = false; string loi = null;

        yield return FirebaseMang.DangNhap(email.Trim(), matKhau, (o, e) => { ok = o; loi = e; });

        if (ok)
            yield return HoSoMang.TaiHoacTao(null, (o, e) => { ok = o; loi = e; });

        dangCho = false;
        if (ok) { Bao("", false); if (daVao != null) daVao(); }
        else
        {
            Bao(loi ?? "Không đăng nhập được.", true);
            // Ho so bao bi khoa thi phai bo phien di, khong giu lai
            if (FirebaseMang.LaLoiBiKhoa(bao)) FirebaseMang.Quen();
        }
    }
}
