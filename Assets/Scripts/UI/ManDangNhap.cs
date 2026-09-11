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

    void OnGUI()
    {
        if (FirebaseMang.DaDangNhap) return;

        GiaoDien.ChuanBi();
        float s = GiaoDien.TiLe;
        GiaoDien.VeNen();

        // ---- Do cao ca khoi de dat giua man hinh ----
        float rong = Mathf.Min(Screen.width - 40f * s, 600f * s);
        float caoKhung = (trang == Trang.DangKy ? 616f : 514f) * s;
        float caoTieuDe = 236f * s;
        float tong = caoTieuDe + caoKhung;
        float y = Mathf.Max(16f * s, (Screen.height - tong) * 0.5f - 10f * s);
        float x = (Screen.width - rong) * 0.5f;

        // ---- Ten game ----
        GiaoDien.TieuDeGame(new Rect(0f, y, Screen.width, 130f * s), s);
        y += 168f * s;                       // chua cho vet mau chay duoi ten game

        // Dong phu giua hai duong ke
        float rongPhu = Mathf.Min(Screen.width - 40f * s, 620f * s);
        var kPhu = GiaoDien.KieuChuNho;
        var canhCu = kPhu.alignment;
        kPhu.alignment = TextAnchor.MiddleCenter;
        GiaoDien.Chu(new Rect((Screen.width - rongPhu) * 0.5f, y, rongPhu, 30f * s),
                     "KẺ SỐNG SÓT CUỐI CÙNG SẼ CHIẾN THẮNG", kPhu);
        kPhu.alignment = canhCu;
        GiaoDien.DuongKe(new Rect(Screen.width * 0.5f - 330f * s, y + 40f * s, 660f * s, Mathf.Max(1f, 1.5f * s)),
                         new Color(0.75f, 0.12f, 0.08f, 0.8f));
        y = y + caoTieuDe - 168f * s;

        // ---- Khung ----
        var khung = new Rect(x, y, rong, caoKhung);
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
