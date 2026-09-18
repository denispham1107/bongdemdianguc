using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SANH PHONG NGAY TRONG GAME - ve bang OnGUI.
///
/// Nguoi choi tao phong, xem danh sach phong nguoi khac tao, vao phong, bam
/// san sang. Host bam bat dau thi ca phong dem nguoc 10 giay roi cung nap man
/// choi. Host duoc bat dau KE CA khi chi co mot minh.
///
/// HOI LAI THEO NHIP chu khong co luong day ve: REST API khong co streaming.
/// Ngoai sanh hoi moi hai giay, trong phong hoi moi giay - nguoi dang cho thi
/// sot ruot hon. So do nay o <see cref="PhongMang.NhipHoiSanh"/>.
///
/// Giao dien (khung, nut, font co dau tieng Viet, chu tu co cho vua o) lay tu
/// <see cref="GiaoDien"/>. Moi thu duoc xep trong mot khung ao rong 1000 don
/// vi, nhan voi <see cref="GiaoDien.TiLe"/> - man hinh nao cung giu dung ti le.
/// </summary>
public class ManSanh : MonoBehaviour
{
    enum Cho { Sanh, TrongPhong }
    Cho dangO = Cho.Sanh;

    List<PhongMang.Phong> danhSach = new List<PhongMang.Phong>();
    string tenPhongMoi = "";
    string manChoiMoi = "Act2";
    string bao = "";
    bool dangCho;
    bool daVaoTran;
    Vector2 cuonDanhSach;

    float hoiLanSau;

    // ---- Bang CAI DAT ----
    // Nhieu tab, hien gio mot tab. Them tab thi them ten vao day va mot nhanh
    // trong VeCaiDat.
    static readonly string[] TenTabCaiDat = { "Giao diện" };
    bool moCaiDat;
    int tabCaiDat;
    MucDoHoa mucChon;

    /// <summary>Be rong khung ao cua sanh, tinh bang don vi truoc khi nhan ti le.</summary>
    public const float RongKhungAo = 1000f;

    void Update()
    {
        if (!FirebaseMang.DaDangNhap || dangO != Cho.Sanh)
        {
            // Sach phep xem truoc chi thuoc ve SANH: dang xuat, vao phong, dem nguoc
            // thi dong - CuaSoSachPhep la static, de mo thi vao tran no van phu kin man hinh
            if (CuaSoSachPhep.XemTruoc) CuaSoSachPhep.Dong();
        }
        else if (CuaSoSachPhep.XemTruoc)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CuaSoSachPhep.Dong();
            else CuaSoSachPhep.CapNhat(TiLeSachPhep);
        }

        // Cham ra NGOAI cua so thi sach dong ngay luc ngon tay DAT XUONG - con nut cua
        // sanh nam duoi cho ay lai nhan cu THA ra sau do (vd. VAO PHONG NHANH). Giu sanh
        // khoa toi khi nhac ngon tay.
        if (CuaSoSachPhep.XemTruoc) khoaSauSach = true;
        else if (khoaSauSach && Input.touchCount == 0 && !Input.GetMouseButton(0)) khoaSauSach = false;

        if (!FirebaseMang.DaDangNhap) return;

        // Dem nguoc xong thi vao tran - hoac host da vao truoc mot nhip
        // (xem PhongMang.DenGioVaoTran de biet vi sao phai xet ca hai)
        if (dangO == Cho.TrongPhong && !daVaoTran
            && PhongMang.DenGioVaoTran(PhongMang.PhongHienTai, PhongMang.ConLaiGiay()))
        {
            daVaoTran = true;
            StartCoroutine(VaoTran());
            return;
        }

        if (Time.unscaledTime < hoiLanSau) return;
        hoiLanSau = Time.unscaledTime
                  + (dangO == Cho.Sanh ? PhongMang.NhipHoiSanh : PhongMang.NhipHoiTrongPhong);
        StartCoroutine(dangO == Cho.Sanh ? HoiDanhSach() : HoiPhong());
    }

    IEnumerator HoiDanhSach()
    {
        yield return PhongMang.LayDanhSach(ds => danhSach = ds);
    }

    IEnumerator HoiPhong()
    {
        if (PhongMang.PhongHienTai == null) yield break;
        string ma = PhongMang.PhongHienTai.ma;

        // Chu phong dap nhip de sanh biet phong nay con song - xem
        // PhongMang.DapNhip. Nguoi khac khong dap: phong la cua chu phong.
        if (PhongMang.LaHost) yield return PhongMang.DapNhip();

        bool con = true;
        yield return PhongMang.TaiLaiPhong(ma, (ok, loi) => con = ok);

        if (!con)
        {
            // Host giai tan phong hoac mat mang
            dangO = Cho.Sanh;
            bao = "Phòng đã đóng.";
        }
    }

    IEnumerator VaoTran()
    {
        var p = PhongMang.PhongHienTai;
        if (p == null) yield break;
        string ma = p.ma;

        // DOC LAI PHONG MOT LAN NUA NGAY TRUOC KHI NAP MAN.
        //
        // Ban sao trong tay co the da mot giay tuoi (sanh hoi lai moi giay).
        // Mot giay la du de chu phong doi man ma may nay chua biet - va the la
        // hai nguoi nap hai man khac nhau. Doc lai ton them mot vong di-ve
        // (~50 ms tren Firebase Singapore), doi lai chac chan cung man.
        yield return PhongMang.TaiLaiPhong(ma, (ok, loi) => { });
        if (PhongMang.PhongHienTai != null) p = PhongMang.PhongHienTai;

        if (PhongMang.LaHost)
            yield return PhongMang.DanhDauDangChoi(null);

        // Man khong doc duoc thi ve man mac dinh chu khong goi LoadScene(null):
        // LoadScene(null) nem loi va nguoi choi ket lai o MainMenu khong hieu vi sao.
        string man = p.manChoi == "Act1" || p.manChoi == "Act2"
                   ? p.manChoi : PhongMang.ManMacDinh;

        // Giu lai thong tin phong de man choi biet minh dang o dau
        TranHienTai.MaPhong = ma;
        TranHienTai.ManChoi = man;
        TranHienTai.LaHost = PhongMang.LaHost;
        TranHienTai.DangChoiMang = true;

        SceneManager.LoadScene(man);
    }

    /// <summary>Ten man choi de hien cho nguoi choi doc.</summary>
    public static string TenMan(string man) { return man == "Act1" ? "Đấu trường" : "Nghĩa địa"; }

    // ================================================================

    void OnGUI()
    {
        if (!FirebaseMang.DaDangNhap) return;

        GiaoDien.ChuanBi();
        float s = GiaoDien.TiLe;

        // Dem nguoc phu kin man hinh
        if (dangO == Cho.TrongPhong && PhongMang.PhongHienTai != null
            && PhongMang.PhongHienTai.trangThai == "demNguoc")
        {
            VeDemNguoc(s);
            return;
        }

        if (dangO != Cho.Sanh) moCaiDat = false;
        bool moSach = dangO == Cho.Sanh && CuaSoSachPhep.XemTruoc;

        // Khong con phu toi bon goc + suong do: canh nghia dia phia sau de nguyen
        if (dangO == Cho.Sanh) VeSanh(s);
        else VeTrongPhong(s);

        // Bang cai dat ve SAU cung de nam tren. Sanh phia duoi da bi khoa
        // (GUI.enabled) trong luc bang mo - IMGUI trao cu bam cho nut nao ve
        // TRUOC, nen khong khoa thi bam vao bang lai trung nut cua sanh nam
        // ngay ben duoi.
        if (moCaiDat) VeCaiDat(s);

        // Sach phep xem truoc (nut KY NANG): cung ly do - ve sau cung, sanh da khoa
        if (moSach) CuaSoSachPhep.Ve(TiLeSachPhep, NhanVatMau(), IconXemTruoc());
    }

    void OnDestroy()
    {
        if (CuaSoSachPhep.XemTruoc) CuaSoSachPhep.Dong();
    }

    // ---------------- SACH PHEP XEM TRUOC ----------------
    //
    // Nguoi dung 14/09/2026: cho cu cua nut CAI DAT thanh nut KY NANG - mo NGUYEN cua
    // so Sach phep trong tran, hien du moi ky nang khong o khoa, keo tha san vao o.
    // Thu tu o luu cung cho voi trong tran (SachPhep -> localStorage) nen tat game mo
    // lai van con.

    /// <summary>Ti le cua Sach phep - DUNG cong thuc cua GameHUD (cao 1080), de o sanh
    /// va trong tran cua so to y het nhau.</summary>
    static float TiLeSachPhep { get { return Screen.height / 1080f; } }

    static Texture2D[] iconXemTruoc;

    static Texture2D[] IconXemTruoc()
    {
        if (iconXemTruoc == null || iconXemTruoc[0] == null)
            iconXemTruoc = new Texture2D[] {
                IconKyNang.Lua(), IconKyNang.Bang(), IconKyNang.Set(), IconKyNang.Loc(),
                IconKyNang.ThienThach(), IconKyNang.Khieng(), IconKyNang.GiatSet(),
                IconKyNang.BinhMau(), IconKyNang.BinhMana(), IconKyNang.CauBang(),
                IconKyNang.GioLoc(), IconKyNang.LuaDiaNguc(), IconKyNang.TangHinh(),
                IconKyNang.CauDien(), IconKyNang.HoaLocXoay() };
        return iconXemTruoc;
    }

    PlayerController nhanVatMau;
    bool khoaSauSach;

    /// <summary>
    /// Nhan vat de doc nang luong / hoi chieu / niem that (SachPhep.ThongSo). Man chinh
    /// co san mot Player_Sorceress dung lam canh (cung prefab voi trong tran, mot so
    /// component bi tat) - doc thong so tu no, khong chep tay con so vao day.
    /// </summary>
    PlayerController NhanVatMau()
    {
        if (nhanVatMau == null)
            nhanVatMau = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        return nhanVatMau;
    }

    /// <summary>Mo Sach phep xem truoc - nut KY NANG goi, phep thu (menu 59) cung goi.</summary>
    public void MoSachPhep()
    {
        moCaiDat = false;
        GUIUtility.keyboardControl = 0;
        CuaSoSachPhep.MoXemTruoc();
    }

    // ---------------- BO CUC ----------------

    /// <summary>
    /// Vi tri cac khoi cua sanh tren mot man hinh W x H. Dung chung cho VeSanh
    /// va phep thu (menu 48) - de phep thu hoi dung cai dang ve.
    /// </summary>
    public struct BoCucSanh
    {
        public float s, x, rong;
        public Rect dauTrang, khungTao, khungDanhSach;
        public Rect tieuDePhongCho, nutKyNang, nutVaoNhanh;
        public Rect nutDangXuat, nutCaiDat;     // dau trang, CAI DAT ngay ben trai DANG XUAT
    }

    public static BoCucSanh TinhBoCucSanh(float W, float H, float s)
    {
        var b = new BoCucSanh();
        b.s = s;
        b.rong = RongKhungAo * s;
        b.x = (W - b.rong) * 0.5f;

        float y = 22f * s;
        b.dauTrang = new Rect(b.x, y, b.rong, 70f * s);
        // Nguoi dung 14/09/2026: nut CAI DAT len dau trang, ngay ben trai DANG XUAT
        b.nutDangXuat = new Rect(b.dauTrang.xMax - 215f * s, b.dauTrang.y + 10f * s, 215f * s, 50f * s);
        b.nutCaiDat = new Rect(b.nutDangXuat.x - 14f * s - 170f * s, b.nutDangXuat.y, 170f * s, 50f * s);
        y += 70f * s + 42f * s;

        b.khungTao = new Rect(b.x, y, b.rong, 150f * s);
        y += 150f * s + 34f * s;

        b.khungDanhSach = new Rect(b.x, y, b.rong, Mathf.Max(260f * s, H - y - 70f * s));

        float le = 30f * s;
        float yt = b.khungDanhSach.y + 24f * s;
        // Rong du cho "VAO PHONG NHANH" o co chu nho nhat (man hinh thap, ti le
        // 0,54): 270 don vi thi bi cat thanh "VAO PHONG NHA…" - anh menu 50.
        b.nutVaoNhanh = new Rect(b.khungDanhSach.xMax - le - 310f * s, yt, 310f * s, 52f * s);
        // Cho cu cua CAI DAT: nut KY NANG (mo Sach phep xem truoc)
        b.nutKyNang = new Rect(b.nutVaoNhanh.x - 14f * s - 170f * s, yt, 170f * s, 52f * s);
        b.tieuDePhongCho = new Rect(b.khungDanhSach.x + le, yt, b.nutKyNang.x - 14f * s - (b.khungDanhSach.x + le), 52f * s);
        return b;
    }

    /// <summary>Vi tri nut VAO PHONG NHANH tren man hinh hien tai.</summary>
    public static Rect ViTriNutVaoNhanh() { return TinhBoCucSanh(Screen.width, Screen.height, GiaoDien.TiLe).nutVaoNhanh; }

    /// <summary>Vi tri nut CAI DAT - dau trang, ngay ben trai DANG XUAT.</summary>
    public static Rect ViTriNutCaiDat() { return TinhBoCucSanh(Screen.width, Screen.height, GiaoDien.TiLe).nutCaiDat; }

    /// <summary>Vi tri nut KY NANG - ngay ben trai VAO PHONG NHANH, cung hang "Phong dang cho".</summary>
    public static Rect ViTriNutKyNang() { return TinhBoCucSanh(Screen.width, Screen.height, GiaoDien.TiLe).nutKyNang; }

    // ---------------- NGOAI SANH ----------------

    void VeSanh(float s)
    {
        var b = TinhBoCucSanh(Screen.width, Screen.height, s);

        // Bang cai dat dang mo thi ca sanh bi khoa - xem OnGUI
        bool khoa = moCaiDat || CuaSoSachPhep.XemTruoc || khoaSauSach;
        GUI.enabled = !khoa;

        // ---- Dau trang: ten nguoi choi, thanh tich, dang xuat ----
        var d = b.dauTrang;
        float rongTen = b.nutCaiDat.x - 20f * s - d.x;     // chua cho CAI DAT + DANG XUAT
        GiaoDien.Chu(new Rect(d.x, d.y, rongTen, 42f * s),
                     FirebaseMang.TenHienThi, GiaoDien.KieuTieuDe);
        GiaoDien.Chu(new Rect(d.x, d.y + 42f * s, rongTen, 28f * s),
                     HoSoMang.CuaToi.soTranThang + " trận thắng  ·  " + HoSoMang.CuaToi.soTranChoi + " trận đã chơi",
                     GiaoDien.KieuChuMo);

        if (GiaoDien.Nut(b.nutCaiDat, "CÀI ĐẶT", GiaoDien.KieuNutDa))
            MoCaiDat();

        if (GiaoDien.Nut(b.nutDangXuat, "ĐĂNG XUẤT", GiaoDien.KieuNutDa))
        {
            FirebaseMang.Quen();
            danhSach.Clear();
            GUI.enabled = true;
            return;
        }

        // Vet mau chay ngang ngay duoi dau trang
        GiaoDien.DuongKe(new Rect(d.x, d.yMax + 14f * s, d.width, Mathf.Max(1f, 2f * s)),
                         new Color(0.72f, 0.08f, 0.05f, 0.9f));

        // ---- Tao phong ----
        var kt = b.khungTao;
        GiaoDien.Khung(kt, s);
        float le = 30f * s;
        float xx = kt.x + le, rr = kt.width - 2f * le;

        GiaoDien.Chu(new Rect(xx, kt.y + 18f * s, rr, 34f * s), "TẠO PHÒNG MỚI", GiaoDien.KieuTieuDeNho);

        float yh = kt.y + 70f * s, cao = 56f * s;
        float rongTao = 210f * s, rongMan = 190f * s, khe = 12f * s;
        float rongO = rr - rongTao - 2f * rongMan - 3f * khe;

        tenPhongMoi = GiaoDien.ONhap(new Rect(xx, yh, rongO, cao), "o_tenphong", tenPhongMoi, 24,
                                     "Tên phòng (không bắt buộc)");

        // Chon man: hai nut canh nhau, nut dang chon to mau do
        float xm = xx + rongO + khe;
        if (GiaoDien.Nut(new Rect(xm, yh, rongMan, cao), "Đấu trường",
                         manChoiMoi == "Act1" ? GiaoDien.KieuNutMau : GiaoDien.KieuNutDa))
            manChoiMoi = "Act1";
        if (GiaoDien.Nut(new Rect(xm + rongMan + khe, yh, rongMan, cao), "Nghĩa địa",
                         manChoiMoi == "Act2" ? GiaoDien.KieuNutMau : GiaoDien.KieuNutDa))
            manChoiMoi = "Act2";

        GUI.enabled = !dangCho && !khoa;
        if (GiaoDien.Nut(new Rect(kt.xMax - le - rongTao, yh, rongTao, cao), "TẠO PHÒNG", GiaoDien.KieuNutMau))
            StartCoroutine(ChayTaoPhong());
        GUI.enabled = !khoa;

        // ---- Danh sach phong ----
        var kd = b.khungDanhSach;
        GiaoDien.Khung(kd, s);
        GiaoDien.Chu(b.tieuDePhongCho, "PHÒNG ĐANG CHỜ", GiaoDien.KieuTieuDeNho);

        // Nut KY NANG nam ngay ben trai VAO PHONG NHANH, cung hang (cho cu cua CAI DAT)
        if (GiaoDien.Nut(b.nutKyNang, "KỸ NĂNG", GiaoDien.KieuNutDa))
            MoSachPhep();

        GUI.enabled = !dangCho && !khoa;
        if (GiaoDien.Nut(b.nutVaoNhanh, "VÀO PHÒNG NHANH", GiaoDien.KieuNutMau))
            StartCoroutine(ChayVaoNhanh());
        GUI.enabled = !khoa;

        float yds = b.nutVaoNhanh.yMax + 20f * s;
        GiaoDien.DuongKe(new Rect(kd.x + le, yds - 10f * s, kd.width - 2f * le, 1f), new Color(0.6f, 0.15f, 0.1f, 0.6f));
        var vung = new Rect(kd.x + le * 0.6f, yds, kd.width - le * 1.2f, kd.yMax - yds - 18f * s);

        var dangCho2 = danhSach.FindAll(p => p.DangCho);
        if (dangCho2.Count == 0)
        {
            var k = GiaoDien.KieuChuMo;
            var canh = k.alignment;
            k.alignment = TextAnchor.MiddleCenter;
            GiaoDien.Chu(new Rect(vung.x, vung.y + vung.height * 0.30f, vung.width, 34f * s),
                         "Chưa có phòng nào đang chờ.", k);
            GiaoDien.Chu(new Rect(vung.x, vung.y + vung.height * 0.30f + 36f * s, vung.width, 34f * s),
                         "Hãy tạo một phòng mới, hoặc bấm VÀO PHÒNG NHANH.", k);
            k.alignment = canh;
        }
        else
        {
            float caoHang = 84f * s, cach = 10f * s;
            float rongTrong = vung.width - 22f * s;
            var vungTrong = new Rect(0, 0, rongTrong, dangCho2.Count * (caoHang + cach));
            cuonDanhSach = GUI.BeginScrollView(vung, cuonDanhSach, vungTrong, false, false,
                                               GUIStyle.none, GUI.skin.verticalScrollbar);
            float y = 0f;
            foreach (var p in dangCho2)
            {
                VeHangPhong(new Rect(0, y, rongTrong, caoHang), p, s, khoa);
                y += caoHang + cach;
            }
            GUI.EndScrollView();
        }

        // ---- Thong bao ----
        if (!string.IsNullOrEmpty(bao))
        {
            var k = GiaoDien.KieuChuMo;
            var mauCu = k.normal.textColor;
            k.normal.textColor = GiaoDien.MauLoi;
            GiaoDien.Chu(new Rect(b.x, Screen.height - 50f * s, b.rong, 36f * s), bao, k);
            k.normal.textColor = mauCu;
        }

        GUI.enabled = true;
    }

    void VeHangPhong(Rect r, PhongMang.Phong p, float s, bool khoa)
    {
        int toiDa = p.toiDa <= 0 ? PhongMang.SoNguoiToiDa : p.toiDa;
        GiaoDien.Hang(r, s, p.ConCho ? GiaoDien.MauMau : new Color(0.3f, 0.25f, 0.25f));

        float rongNut = 150f * s;
        float rongCho = 150f * s;
        float xChu = r.x + 24f * s;
        float rongChu = r.width - 24f * s - rongNut - rongCho - 40f * s;

        GiaoDien.Chu(new Rect(xChu, r.y + 10f * s, rongChu, 36f * s), p.ten, GiaoDien.KieuTieuDeNho);
        GiaoDien.Chu(new Rect(xChu, r.y + 46f * s, rongChu, 28f * s),
                     TenMan(p.manChoi) + "  ·  Chủ phòng: " + p.hostTen, GiaoDien.KieuChuMo);

        // Bon o nguoi: o da co nguoi to do
        float xo = r.xMax - rongNut - 24f * s - rongCho;
        float kt = 16f * s, khe = 7f * s;
        float yo = r.y + r.height * 0.5f - kt * 0.5f - 10f * s;
        for (int i = 0; i < toiDa; i++)
        {
            var o = new Rect(xo + i * (kt + khe), yo, kt, kt);
            GiaoDien.To(o, new Color(0.35f, 0.12f, 0.10f));
            if (i < p.soNguoi) GiaoDien.To(new Rect(o.x + 2f, o.y + 2f, o.width - 4f, o.height - 4f), GiaoDien.MauMauSang);
            else GiaoDien.To(new Rect(o.x + 2f, o.y + 2f, o.width - 4f, o.height - 4f), new Color(0.05f, 0.04f, 0.04f));
        }
        GiaoDien.Chu(new Rect(xo, yo + kt + 4f * s, rongCho, 26f * s),
                     p.soNguoi + "/" + toiDa + " người", GiaoDien.KieuChuNho);

        GUI.enabled = !dangCho && p.ConCho && !khoa;
        if (GiaoDien.Nut(new Rect(r.xMax - rongNut - 16f * s, r.y + (r.height - 52f * s) * 0.5f, rongNut, 52f * s),
                         p.ConCho ? "VÀO" : "ĐẦY", p.ConCho ? GiaoDien.KieuNutMau : GiaoDien.KieuNutDa))
            StartCoroutine(ChayVaoPhong(p.ma));
        GUI.enabled = !khoa;
    }

    // ---------------- TRONG PHONG ----------------

    void VeTrongPhong(float s)
    {
        var p = PhongMang.PhongHienTai;
        if (p == null) { dangO = Cho.Sanh; return; }

        float rong = RongKhungAo * s;
        float x = (Screen.width - rong) * 0.5f;
        float y = 22f * s;

        // ---- Dau trang: ten phong, man, roi phong ----
        float rongNutRoi = 200f * s;
        GiaoDien.Chu(new Rect(x, y, rong - rongNutRoi - 20f * s, 44f * s), p.ten, GiaoDien.KieuTieuDe);
        GiaoDien.Chu(new Rect(x, y + 44f * s, rong - rongNutRoi - 20f * s, 28f * s),
                     "Màn chơi: " + TenMan(p.manChoi) + (PhongMang.LaHost ? "  ·  Bạn là chủ phòng" : ""),
                     GiaoDien.KieuChuMo);

        if (GiaoDien.Nut(new Rect(x + rong - rongNutRoi, y + 10f * s, rongNutRoi, 50f * s),
                         "RỜI PHÒNG", GiaoDien.KieuNutDa))
        { StartCoroutine(ChayRoiPhong()); return; }

        y += 72f * s + 14f * s;
        GiaoDien.DuongKe(new Rect(x, y, rong, Mathf.Max(1f, 2f * s)), new Color(0.72f, 0.08f, 0.05f, 0.9f));
        y += 30f * s;

        // ---- Chon man (chi chu phong) ----
        float le = 30f * s;
        if (PhongMang.LaHost)
        {
            var km = new Rect(x, y, rong, 110f * s);
            GiaoDien.Khung(km, s);
            GiaoDien.Chu(new Rect(km.x + le, km.y + 16f * s, 400f * s, 34f * s), "CHỌN MÀN CHƠI", GiaoDien.KieuTieuDeNho);
            GiaoDien.Chu(new Rect(km.x + le, km.y + 56f * s, 400f * s, 34f * s), "Chỉ chủ phòng đổi được", GiaoDien.KieuChuNho);
            float rongMan = 210f * s;
            float xm = km.xMax - le - 2f * rongMan - 12f * s;
            float ym = km.y + (km.height - 56f * s) * 0.5f;
            if (GiaoDien.Nut(new Rect(xm, ym, rongMan, 56f * s), "Đấu trường",
                             p.manChoi == "Act1" ? GiaoDien.KieuNutMau : GiaoDien.KieuNutDa) && p.manChoi != "Act1")
                StartCoroutine(PhongMang.DoiManChoi("Act1", null));
            if (GiaoDien.Nut(new Rect(xm + rongMan + 12f * s, ym, rongMan, 56f * s), "Nghĩa địa",
                             p.manChoi != "Act1" ? GiaoDien.KieuNutMau : GiaoDien.KieuNutDa) && p.manChoi == "Act1")
                StartCoroutine(PhongMang.DoiManChoi("Act2", null));
            y += 110f * s + 30f * s;
        }

        // ---- Bon ghe - MOI NGUOI MOT HANG ----
        // Truoc day bon the xep thanh bon cot; nguoi dung xin xep theo hang
        // (11/09/2026): moi hang rong ca khung, ten dai khong bi ep vao mot cot
        // hep 220 don vi nua.
        int toiDa = p.toiDa <= 0 ? PhongMang.SoNguoiToiDa : p.toiDa;
        const float CaoHangGhe = 64f, KheHangGhe = 8f;
        float caoKhungGhe = 96f + toiDa * CaoHangGhe + (toiDa - 1) * KheHangGhe + 24f;
        var kg = new Rect(x, y, rong, caoKhungGhe * s);
        GiaoDien.Khung(kg, s, true);
        GiaoDien.Chu(new Rect(kg.x + le, kg.y + 44f * s, rong - 2f * le, 34f * s),
                     "NGƯỜI CHƠI  " + p.nguoiChoi.Count + "/" + toiDa, GiaoDien.KieuTieuDeNho);

        float yHang = kg.y + 96f * s;
        for (int i = 0; i < toiDa; i++)
        {
            var o = new Rect(kg.x + le, yHang, rong - 2f * le, CaoHangGhe * s);
            VeGhe(o, i, i < p.nguoiChoi.Count ? p.nguoiChoi[i] : null, p, s);
            yHang += (CaoHangGhe + KheHangGhe) * s;
        }
        y += caoKhungGhe * s + 30f * s;

        // ---- San sang / bat dau ----
        var toi = p.nguoiChoi.Find(n => n.uid == FirebaseMang.Uid);
        bool dangSanSang = toi != null && toi.sanSang;

        float rongNut = PhongMang.LaHost ? (rong - 16f * s) * 0.5f : rong;
        if (GiaoDien.Nut(new Rect(x, y, rongNut, 64f * s),
                         dangSanSang ? "BỎ SẴN SÀNG" : "SẴN SÀNG",
                         PhongMang.LaHost || dangSanSang ? GiaoDien.KieuNutDa : GiaoDien.KieuNutMau))
            StartCoroutine(PhongMang.DatSanSang(!dangSanSang, null));

        if (PhongMang.LaHost)
        {
            if (GiaoDien.Nut(new Rect(x + rongNut + 16f * s, y, rongNut, 64f * s),
                             "BẮT ĐẦU TRẬN", GiaoDien.KieuNutMau))
                StartCoroutine(PhongMang.BatDauDemNguoc(null));
        }
        y += 64f * s + 16f * s;

        var k = GiaoDien.KieuChuMo;
        var canh = k.alignment;
        k.alignment = TextAnchor.MiddleCenter;
        GiaoDien.Chu(new Rect(x, y, rong, 30f * s),
                     PhongMang.LaHost
                       ? "Bạn là chủ phòng — bấm BẮT ĐẦU TRẬN lúc nào cũng được, kể cả khi chỉ có một mình."
                       : "Đang chờ chủ phòng bắt đầu trận…", k);
        k.alignment = canh;
    }

    /// <summary>
    /// Mot hang ghe trong phong: so ghe, ten (lon) va vai tro (nho) ben trai,
    /// trang thai san sang o giua phai, nut DUOI sat le phai (chi chu phong
    /// thay, va khong co o hang cua chinh minh). <paramref name="n"/> null la
    /// ghe trong.
    /// </summary>
    void VeGhe(Rect o, int thuTu, PhongMang.NguoiTrongPhong n, PhongMang.Phong p, float s)
    {
        float rongSo = 56f * s;
        float rongDuoi = 140f * s;
        float rongTrangThai = 190f * s;
        float xTrangThai = o.xMax - 16f * s - rongDuoi - 24f * s - rongTrangThai;
        float xTen = o.x + rongSo;
        float rongTen = xTrangThai - 16f * s - xTen;

        // So ghe o dau hang
        var kso = GiaoDien.KieuTieuDeNho;
        var cSo = kso.alignment; var mSo = kso.normal.textColor;
        kso.alignment = TextAnchor.MiddleCenter;
        kso.normal.textColor = n == null ? new Color(0.3f, 0.27f, 0.25f) : GiaoDien.MauToi;

        if (n == null)
        {
            GiaoDien.To(o, new Color(0.03f, 0.025f, 0.025f, 0.35f));
            GiaoDien.To(new Rect(o.x, o.yMax - 1f, o.width, 1f), new Color(0.3f, 0.15f, 0.12f, 0.6f));
            GiaoDien.Chu(new Rect(o.x, o.y, rongSo, o.height), (thuTu + 1).ToString(), kso);
            kso.alignment = cSo; kso.normal.textColor = mSo;

            var kt = GiaoDien.KieuChuMo;
            var m0 = kt.normal.textColor;
            kt.normal.textColor = GiaoDien.MauToi;
            GiaoDien.Chu(new Rect(xTen, o.y, rongTen, o.height), "Ghế trống", kt);
            kt.normal.textColor = m0;
            return;
        }

        bool laChu = n.uid == p.hostUid;
        bool laToi = n.uid == FirebaseMang.Uid;
        GiaoDien.Hang(o, s, n.sanSang ? GiaoDien.MauXanh : (laChu ? GiaoDien.MauVang : GiaoDien.MauMau));
        GiaoDien.Chu(new Rect(o.x, o.y, rongSo, o.height), (thuTu + 1).ToString(), kso);
        kso.alignment = cSo; kso.normal.textColor = mSo;

        // Ten (dong tren) va vai tro (dong duoi)
        GiaoDien.Chu(new Rect(xTen, o.y + 6f * s, rongTen, 32f * s), n.ten, GiaoDien.KieuTieuDeNho);

        string vaiTro = laChu ? "CHỦ PHÒNG" : "NGƯỜI CHƠI";
        var kn = GiaoDien.KieuChuNho;
        var mc = kn.normal.textColor;
        kn.normal.textColor = laChu ? GiaoDien.MauVang : GiaoDien.MauMo;
        GiaoDien.Chu(new Rect(xTen, o.y + 38f * s, rongTen, 22f * s), vaiTro + (laToi ? "  ·  BẠN" : ""), kn);
        kn.normal.textColor = mc;

        // Trang thai san sang
        var ks = GiaoDien.KieuTieuDeNho;
        var ms = ks.normal.textColor;
        ks.normal.textColor = n.sanSang ? GiaoDien.MauXanh : GiaoDien.MauToi;
        GiaoDien.Chu(new Rect(xTrangThai, o.y, rongTrangThai, o.height), n.sanSang ? "SẴN SÀNG" : "Đang chờ…", ks);
        ks.normal.textColor = ms;

        if (PhongMang.LaHost && !laToi)
        {
            if (GiaoDien.Nut(new Rect(o.xMax - 16f * s - rongDuoi, o.y + (o.height - 44f * s) * 0.5f, rongDuoi, 44f * s),
                             "ĐUỔI", GiaoDien.KieuNutDa))
                StartCoroutine(PhongMang.DuoiNguoi(n.uid, null));
        }
    }

    // ---------------- DEM NGUOC ----------------

    // Nguoi dung 13/09/2026 (anh chup man dem nguoc):
    //   - con so "qua don gian" -> ve lai cho rung ron: anh chu so ve san (CongCu/DemNguoc/
    //     sinh_so_dem_nguoc.py) CUNG ngon ngu hinh voi anh ten game - chu Gothic do mau, nut ne,
    //     mau nho giot; phia sau la vong phu chu xoay cham; moi giay con so DAP va RUNG, bon goc
    //     man hinh toi do lai theo nhip nhu tim dap;
    //   - KHONG hien ten man ("NGHIA DIA") nua;
    //   - dong "TRAN DAU BAT DAU SAU" va gach do dang de len nhan vat -> dua len sat mep tren.

    static Texture2D[] anhSo;
    static Texture2D anhVongPhuChu, anhToiGoc;

    static void NapAnhDemNguoc()
    {
        if (anhSo != null) return;
        anhSo = new Texture2D[10];
        for (int i = 0; i < 10; i++) anhSo[i] = Resources.Load<Texture2D>("GiaoDien/DemNguoc/So" + i);
        anhVongPhuChu = Resources.Load<Texture2D>("GiaoDien/DemNguoc/VongPhuChu");

        // Toi dan ra bon goc (giua trong suot) - to mau do sam luc ve
        const int n = 64;
        anhToiGoc = new Texture2D(n, n, TextureFormat.RGBA32, false);
        anhToiGoc.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x + 0.5f) / n * 2f - 1f, dy = (y + 0.5f) / n * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.4142f;
                float t = Mathf.Clamp01((d - 0.32f) / 0.68f);
                px[y * n + x] = new Color(1f, 1f, 1f, t * t * (3f - 2f * t));
            }
        anhToiGoc.SetPixels(px);
        anhToiGoc.Apply(false, false);
    }

    /// <summary>Khung dong chu "TRAN DAU BAT DAU SAU" - sat mep tren, tren dau nhan vat.</summary>
    public static Rect KhungTieuDeDemNguoc(float s)
    {
        return new Rect(0f, Mathf.Max(10f * s, Screen.height * 0.045f), Screen.width, 40f * s);
    }

    /// <summary>Gach do ngay duoi dong chu dem nguoc.</summary>
    public static Rect KhungGachDemNguoc(float s)
    {
        var t = KhungTieuDeDemNguoc(s);
        return new Rect(Screen.width * 0.5f - 260f * s, t.yMax + 8f * s, 520f * s, Mathf.Max(1f, 2f * s));
    }

    /// <summary>Chieu cao anh chu so luc dung yen (chua dap nhip). 306 = 360 x 0,85: nguoi dung xin
    /// con so va vong phu chu nho bot 15% (13/09/2026).</summary>
    public const float CaoSoDemNguoc = 306f;

    /// <summary>Canh vong phu chu luc dung yen. 400 = 470 x 0,85.</summary>
    public const float CoVongDemNguoc = 400f;

    void VeDemNguoc(float s)
    {
        // Khong phu toi / suong len canh phia sau (nguoi dung 12/09/2026) - chi toi DO o
        // bon goc theo nhip, giua man hinh van thay ro nhan vat va lo lua
        NapAnhDemNguoc();
        double conLai = PhongMang.ConLaiGiay();
        int con = Mathf.Clamp(Mathf.CeilToInt((float)conLai), 0, 99);

        // phan: vua sang giay moi = gan 1, cuoi giay = 0 -> nhip manh ngay luc doi so roi tat dan
        float phan = Mathf.Repeat((float)conLai, 1f);
        float nhip = phan * phan * phan;
        var mauCu = GUI.color;

        // ---- 1. Bon goc toi do dap theo nhip tim ----
        GUI.color = new Color(0.30f, 0f, 0f, 0.35f + 0.40f * nhip);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), anhToiGoc, ScaleMode.StretchToFill, true);

        // ---- 2. Dong chu + gach do: sat mep tren ----
        GUI.color = Color.white;
        var k = GiaoDien.KieuTieuDeNho;
        var canh = k.alignment;
        k.alignment = TextAnchor.MiddleCenter;
        GiaoDien.Chu(KhungTieuDeDemNguoc(s), "TRẬN ĐẤU BẮT ĐẦU SAU", k);
        k.alignment = canh;
        GiaoDien.DuongKe(KhungGachDemNguoc(s), new Color(0.8f, 0.1f, 0.06f, 0.9f));

        var tam = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Matrix4x4 cu = GUI.matrix;

        // ---- 3. Vong phu chu xoay cham phia sau con so ----
        if (anhVongPhuChu != null)
        {
            float coVong = CoVongDemNguoc * s * (1f + 0.07f * nhip);
            GUIUtility.RotateAroundPivot(Time.unscaledTime * 14f, tam);
            GUI.color = new Color(1f, 1f, 1f, 0.45f + 0.40f * nhip);
            GUI.DrawTexture(new Rect(tam.x - coVong * 0.5f, tam.y - coVong * 0.5f, coVong, coVong),
                            anhVongPhuChu, ScaleMode.StretchToFill, true);
            GUI.matrix = cu;
        }

        // ---- 4. Con so: dap to roi co lai, rung manh luc vua doi so ----
        string chu = con.ToString();
        var anh0 = anhSo[0];
        if (anh0 != null)
        {
            float cao = CaoSoDemNguoc * s * (1f + 0.25f * nhip);
            float rongMot = cao * anh0.width / (float)anh0.height;
            float buoc = rongMot * 0.58f;                         // chu so ghep sat nhau ("10")
            float tong = rongMot + buoc * (chu.Length - 1);
            float rung = 7f * s * nhip;
            float lx = Mathf.Sin(Time.unscaledTime * 71f) * rung, ly = Mathf.Cos(Time.unscaledTime * 53f) * rung;
            float x = tam.x - tong * 0.5f + lx;
            float y = tam.y - cao * 0.5f + ly;
            GUI.color = Color.white;
            for (int i = 0; i < chu.Length; i++)
            {
                var a = anhSo[chu[i] - '0'];
                if (a != null) GUI.DrawTexture(new Rect(x + i * buoc, y, rongMot, cao), a, ScaleMode.StretchToFill, true);
            }
        }
        else
        {
            // Khong nap duoc anh thi van phai co con so - ve bang chu nhu ban cu
            var ks = GiaoDien.KieuSoLon;
            GUI.Label(new Rect(0, tam.y - 140f * s, Screen.width, 280f * s), chu, ks);
        }
        GUI.color = mauCu;
    }

    // ---------------- HANH DONG ----------------

    IEnumerator ChayTaoPhong()
    {
        dangCho = true; bao = "";
        bool ok = false; string loi = null;
        yield return PhongMang.TaoPhong(tenPhongMoi.Trim(), manChoiMoi,
                                        (o, e) => { ok = o; loi = e; });
        dangCho = false;
        if (ok) { dangO = Cho.TrongPhong; daVaoTran = false; hoiLanSau = 0f; }
        else bao = loi ?? "Không tạo được phòng.";
    }

    IEnumerator ChayVaoPhong(string ma)
    {
        dangCho = true; bao = "";
        bool ok = false; string loi = null;
        yield return PhongMang.VaoPhong(ma, (o, e) => { ok = o; loi = e; });
        dangCho = false;
        if (ok) { dangO = Cho.TrongPhong; daVaoTran = false; hoiLanSau = 0f; }
        else bao = loi ?? "Không vào được phòng.";
    }

    IEnumerator ChayVaoNhanh()
    {
        dangCho = true; bao = "";
        bool ok = false; string loi = null;
        yield return PhongMang.VaoPhongNhanh((o, e) => { ok = o; loi = e; });
        dangCho = false;
        if (ok) { dangO = Cho.TrongPhong; daVaoTran = false; hoiLanSau = 0f; }
        else bao = loi ?? "Không vào được phòng nào.";
    }

    IEnumerator ChayRoiPhong()
    {
        dangCho = true;
        yield return PhongMang.RoiPhong(null);
        dangCho = false;
        dangO = Cho.Sanh;
        hoiLanSau = 0f;
    }

    // ---------------- CAI DAT ----------------

    string baoCaiDat = "";
    bool dangNapLai;

    // Chi CANH 3D doi do phan giai - chu, khung, nut luon du net nhu muc Cao
    static readonly string[] MoTaMucDoHoa =
    {
        "Cảnh 100% · bóng mềm · khử răng cưa",
        "Cảnh 75% · bóng cứng · tắt khử răng cưa",
        "Cảnh 62% · bóng cứng gần · giảm chi tiết xa",
        "Cảnh 50% · tắt bóng · cho máy rất yếu",
    };

    /// <summary>Mo bang cai dat - nut CAI DAT goi, phep thu (menu 48) cung goi.</summary>
    public void MoCaiDat()
    {
        moCaiDat = true;
        tabCaiDat = 0;
        mucChon = CaiDatDoHoa.Muc;
        baoCaiDat = "";
        // O ten phong dang giu ban phim thi van go chu vao duoc du da bi khoa
        GUIUtility.keyboardControl = 0;
    }

    public bool DangMoCaiDat { get { return moCaiDat; } }

    public void ChonMucDoHoa(MucDoHoa m) { mucChon = m; baoCaiDat = ""; }

    /// <summary>
    /// Nut OK. Khong doi gi thi chi dong bang - tai lai ca game chi de ve y
    /// het nhu cu la bat nguoi choi doi vo ich. Co doi thi luu roi tai lai.
    /// </summary>
    public void BamOKCaiDat()
    {
        if (mucChon == CaiDatDoHoa.Muc) { moCaiDat = false; return; }

        // Luu hong ma van tai lai thi game khoi dong voi muc CU - nguoi choi
        // thay minh bam OK ma khong co gi xay ra. Bao ro thay vi im lang.
        if (!CaiDatDoHoa.Luu(mucChon))
        {
            baoCaiDat = "Trình duyệt không cho lưu cài đặt (có thể đang ở chế độ ẩn danh).";
            return;
        }
        dangNapLai = true;
        CaiDatDoHoa.NapLaiGame();
    }

    void VeCaiDat(float s)
    {
        // Phu toi ca sanh
        GiaoDien.To(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.4f));

        float rong = Mathf.Min(Screen.width - 30f * s, 880f * s);
        float cao = Mathf.Min(Screen.height - 30f * s, 700f * s);   // bon hang muc do hoa
        float x = (Screen.width - rong) * 0.5f;
        float y = (Screen.height - cao) * 0.5f;
        float le = 34f * s;

        GiaoDien.Khung(new Rect(x, y, rong, cao), s, true, GiaoDien.DoDucBangNoi);

        GUI.enabled = !dangNapLai;

        GiaoDien.Chu(new Rect(x + le, y + 40f * s, rong - 2f * le, 44f * s), "CÀI ĐẶT", GiaoDien.KieuTieuDe);

        // ---- Hang tab ----
        float yt = y + 100f * s;
        float xt = x + le;
        for (int i = 0; i < TenTabCaiDat.Length; i++)
        {
            if (GiaoDien.Tab(new Rect(xt, yt, 220f * s, 48f * s), TenTabCaiDat[i], i == tabCaiDat, s))
                tabCaiDat = i;
            xt += 230f * s;
        }
        GiaoDien.To(new Rect(xt, yt + 47f * s, x + rong - le - xt, 1f), new Color(0.35f, 0.22f, 0.2f, 0.7f));

        float yn = yt + 70f * s;
        if (tabCaiDat == 0) VeTabGiaoDien(x + le, yn, rong - 2f * le, s);

        // ---- Nut duoi cung ----
        float yb = y + cao - 34f * s - 58f * s;
        string ghiChu = !string.IsNullOrEmpty(baoCaiDat) ? baoCaiDat
                      : dangNapLai ? "Đang tải lại game..."
                      : mucChon != CaiDatDoHoa.Muc ? "Bấm OK: game sẽ tải lại để áp dụng."
                      : "";
        if (ghiChu.Length > 0)
        {
            var k = GiaoDien.KieuChuMo;
            var mau = k.normal.textColor;
            k.normal.textColor = string.IsNullOrEmpty(baoCaiDat) ? GiaoDien.MauMo : GiaoDien.MauLoi;
            GiaoDien.Chu(new Rect(x + le, yb - 46f * s, rong - 2f * le, 34f * s), ghiChu, k);
            k.normal.textColor = mau;
        }

        float rongNut = 190f * s;
        if (GiaoDien.Nut(new Rect(x + rong - le - rongNut - 16f * s - rongNut, yb, rongNut, 58f * s),
                         "HỦY", GiaoDien.KieuNutDa))
            moCaiDat = false;

        if (GiaoDien.Nut(new Rect(x + rong - le - rongNut, yb, rongNut, 58f * s), "OK", GiaoDien.KieuNutMau))
            BamOKCaiDat();

        GUI.enabled = true;
    }

    void VeTabGiaoDien(float x, float y, float rong, float s)
    {
        GiaoDien.Chu(new Rect(x, y, rong, 30f * s), "Chất lượng cảnh 3D · chữ và nút luôn giữ nguyên độ nét", GiaoDien.KieuChuMo);
        y += 42f * s;

        float caoHang = 64f * s;
        float rongTen = 310f * s;          // du cho "Trung binh (hien gio)" khong phai thu nho
        for (int i = 0; i < CaiDatDoHoa.SoMuc; i++)
        {
            var m = (MucDoHoa)i;
            var o = new Rect(x, y, rong, caoHang);
            bool chon = m == mucChon;

            GiaoDien.Hang(o, s, chon ? GiaoDien.MauMauSang : new Color(0.3f, 0.2f, 0.18f));
            if (chon) GiaoDien.To(o, new Color(0.7f, 0.08f, 0.05f, 0.22f));

            // Ca hang la mot nut trong suot: bam vao dau cung chon
            if (GUI.Button(o, GUIContent.none, GUIStyle.none)) ChonMucDoHoa(m);

            string ten = CaiDatDoHoa.Ten[i] + (m == CaiDatDoHoa.Muc ? " (hiện giờ)" : "");
            var kt = GiaoDien.KieuTieuDeNho;
            var mt = kt.normal.textColor;
            kt.normal.textColor = chon ? Color.white : GiaoDien.MauGiay;
            GiaoDien.Chu(new Rect(o.x + 26f * s, o.y, rongTen - 26f * s, o.height), ten, kt);
            kt.normal.textColor = mt;

            GiaoDien.Chu(new Rect(o.x + rongTen, o.y, o.width - rongTen - 16f * s, o.height),
                         MoTaMucDoHoa[i], GiaoDien.KieuChuMo);

            y += caoHang + 8f * s;
        }
    }
}
