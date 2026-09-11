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

        GiaoDien.VeNen();
        if (dangO == Cho.Sanh) VeSanh(s);
        else VeTrongPhong(s);

        // Bang cai dat ve SAU cung de nam tren. Sanh phia duoi da bi khoa
        // (GUI.enabled) trong luc bang mo - IMGUI trao cu bam cho nut nao ve
        // TRUOC, nen khong khoa thi bam vao bang lai trung nut cua sanh nam
        // ngay ben duoi.
        if (moCaiDat) VeCaiDat(s);
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
        public Rect tieuDePhongCho, nutCaiDat, nutVaoNhanh;
    }

    public static BoCucSanh TinhBoCucSanh(float W, float H, float s)
    {
        var b = new BoCucSanh();
        b.s = s;
        b.rong = RongKhungAo * s;
        b.x = (W - b.rong) * 0.5f;

        float y = 22f * s;
        b.dauTrang = new Rect(b.x, y, b.rong, 70f * s);
        y += 70f * s + 42f * s;

        b.khungTao = new Rect(b.x, y, b.rong, 150f * s);
        y += 150f * s + 34f * s;

        b.khungDanhSach = new Rect(b.x, y, b.rong, Mathf.Max(260f * s, H - y - 70f * s));

        float le = 30f * s;
        float yt = b.khungDanhSach.y + 24f * s;
        // Rong du cho "VAO PHONG NHANH" o co chu nho nhat (man hinh thap, ti le
        // 0,54): 270 don vi thi bi cat thanh "VAO PHONG NHA…" - anh menu 50.
        b.nutVaoNhanh = new Rect(b.khungDanhSach.xMax - le - 310f * s, yt, 310f * s, 52f * s);
        b.nutCaiDat = new Rect(b.nutVaoNhanh.x - 14f * s - 170f * s, yt, 170f * s, 52f * s);
        b.tieuDePhongCho = new Rect(b.khungDanhSach.x + le, yt, b.nutCaiDat.x - 14f * s - (b.khungDanhSach.x + le), 52f * s);
        return b;
    }

    /// <summary>Vi tri nut VAO PHONG NHANH tren man hinh hien tai.</summary>
    public static Rect ViTriNutVaoNhanh() { return TinhBoCucSanh(Screen.width, Screen.height, GiaoDien.TiLe).nutVaoNhanh; }

    /// <summary>Vi tri nut CAI DAT - ngay ben trai VAO PHONG NHANH, cung hang "Phong dang cho".</summary>
    public static Rect ViTriNutCaiDat() { return TinhBoCucSanh(Screen.width, Screen.height, GiaoDien.TiLe).nutCaiDat; }

    // ---------------- NGOAI SANH ----------------

    void VeSanh(float s)
    {
        var b = TinhBoCucSanh(Screen.width, Screen.height, s);

        // Bang cai dat dang mo thi ca sanh bi khoa - xem OnGUI
        bool khoa = moCaiDat;
        GUI.enabled = !khoa;

        // ---- Dau trang: ten nguoi choi, thanh tich, dang xuat ----
        var d = b.dauTrang;
        float rongNutXuat = 190f * s;
        GiaoDien.Chu(new Rect(d.x, d.y, d.width - rongNutXuat - 20f * s, 42f * s),
                     FirebaseMang.TenHienThi, GiaoDien.KieuTieuDe);
        GiaoDien.Chu(new Rect(d.x, d.y + 42f * s, d.width - rongNutXuat - 20f * s, 28f * s),
                     HoSoMang.CuaToi.soTranThang + " trận thắng  ·  " + HoSoMang.CuaToi.soTranChoi + " trận đã chơi",
                     GiaoDien.KieuChuMo);

        if (GiaoDien.Nut(new Rect(d.xMax - rongNutXuat, d.y + 10f * s, rongNutXuat, 50f * s),
                         "ĐĂNG XUẤT", GiaoDien.KieuNutDa))
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

        // Nut CAI DAT nam ngay ben trai VAO PHONG NHANH, cung hang
        if (GiaoDien.Nut(b.nutCaiDat, "CÀI ĐẶT", GiaoDien.KieuNutDa))
            MoCaiDat();

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

        // ---- Bon ghe ----
        int toiDa = p.toiDa <= 0 ? PhongMang.SoNguoiToiDa : p.toiDa;
        var kg = new Rect(x, y, rong, 330f * s);
        GiaoDien.Khung(kg, s, true);
        GiaoDien.Chu(new Rect(kg.x + le, kg.y + 44f * s, rong - 2f * le, 34f * s),
                     "NGƯỜI CHƠI  " + p.nguoiChoi.Count + "/" + toiDa, GiaoDien.KieuTieuDeNho);

        float khe = 16f * s;
        float rongThe = (rong - 2f * le - (toiDa - 1) * khe) / toiDa;
        float yThe = kg.y + 96f * s, caoThe = 200f * s;
        for (int i = 0; i < toiDa; i++)
        {
            var o = new Rect(kg.x + le + i * (rongThe + khe), yThe, rongThe, caoThe);
            VeGhe(o, i < p.nguoiChoi.Count ? p.nguoiChoi[i] : null, p, s);
        }
        y += 330f * s + 30f * s;

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

    /// <summary>Mot the ghe trong phong. <paramref name="n"/> null la ghe trong.</summary>
    void VeGhe(Rect o, PhongMang.NguoiTrongPhong n, PhongMang.Phong p, float s)
    {
        if (n == null)
        {
            GiaoDien.To(o, new Color(0.03f, 0.025f, 0.025f, 0.35f));
            GiaoDien.To(new Rect(o.x, o.y, o.width, 1f), new Color(0.3f, 0.15f, 0.12f, 0.6f));
            GiaoDien.To(new Rect(o.x, o.yMax - 1f, o.width, 1f), new Color(0.3f, 0.15f, 0.12f, 0.6f));
            var kt = GiaoDien.KieuChuNho;
            var c0 = kt.alignment; var m0 = kt.normal.textColor;
            kt.alignment = TextAnchor.MiddleCenter; kt.normal.textColor = GiaoDien.MauToi;
            GiaoDien.Chu(new Rect(o.x, o.center.y - 15f * s, o.width, 30f * s), "Ghế trống", kt);
            kt.alignment = c0; kt.normal.textColor = m0;
            return;
        }

        bool laChu = n.uid == p.hostUid;
        bool laToi = n.uid == FirebaseMang.Uid;
        GiaoDien.Hang(o, s, n.sanSang ? GiaoDien.MauXanh : (laChu ? GiaoDien.MauVang : GiaoDien.MauMau));

        float le = 18f * s;
        float rr = o.width - 2f * le;
        float y = o.y + 18f * s;

        // Nhan vai tro
        string vaiTro = laChu ? "CHỦ PHÒNG" : "NGƯỜI CHƠI";
        var kn = GiaoDien.KieuChuNho;
        var mc = kn.normal.textColor;
        kn.normal.textColor = laChu ? GiaoDien.MauVang : GiaoDien.MauMo;
        GiaoDien.Chu(new Rect(o.x + le, y, rr, 24f * s), vaiTro + (laToi ? "  ·  BẠN" : ""), kn);
        kn.normal.textColor = mc;
        y += 32f * s;

        GiaoDien.Chu(new Rect(o.x + le, y, rr, 38f * s), n.ten, GiaoDien.KieuTieuDeNho);
        y += 48f * s;

        var ks = GiaoDien.KieuChuMo;
        var ms = ks.normal.textColor;
        ks.normal.textColor = n.sanSang ? GiaoDien.MauXanh : GiaoDien.MauToi;
        GiaoDien.Chu(new Rect(o.x + le, y, rr, 28f * s), n.sanSang ? "SẴN SÀNG" : "Đang chờ…", ks);
        ks.normal.textColor = ms;

        if (PhongMang.LaHost && !laToi)
        {
            if (GiaoDien.Nut(new Rect(o.x + le, o.yMax - 18f * s - 44f * s, rr, 44f * s),
                             "ĐUỔI", GiaoDien.KieuNutDa))
                StartCoroutine(PhongMang.DuoiNguoi(n.uid, null));
        }
    }

    // ---------------- DEM NGUOC ----------------

    void VeDemNguoc(float s)
    {
        GiaoDien.VeNen(0.72f);

        double conLai = PhongMang.ConLaiGiay();
        int con = Mathf.Max(0, Mathf.CeilToInt((float)conLai));

        // Moi giay con so dap mot nhip: phong to roi co lai
        float phan = Mathf.Repeat((float)conLai, 1f);
        float phong = 1f + 0.22f * phan * phan;

        float giua = Screen.height * 0.5f;
        var k = GiaoDien.KieuTieuDeNho;
        var canh = k.alignment;
        k.alignment = TextAnchor.MiddleCenter;
        GiaoDien.Chu(new Rect(0, giua - 230f * s, Screen.width, 40f * s), "TRẬN ĐẤU BẮT ĐẦU SAU", k);
        k.alignment = canh;

        GiaoDien.DuongKe(new Rect(Screen.width * 0.5f - 260f * s, giua - 178f * s, 520f * s, Mathf.Max(1f, 2f * s)),
                         new Color(0.8f, 0.1f, 0.06f, 0.9f));

        var tam = new Vector2(Screen.width * 0.5f, giua);
        Matrix4x4 cu = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(phong, phong), tam);
        var ks = GiaoDien.KieuSoLon;
        var mau = ks.normal.textColor;
        ks.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
        GUI.Label(new Rect(0 + 6f * s, giua - 140f * s + 8f * s, Screen.width, 280f * s), con.ToString(), ks);
        ks.normal.textColor = new Color(0.95f, 0.10f, 0.05f, 0.12f);
        for (int i = 0; i < 8; i++)
        {
            float g = i * Mathf.PI / 4f;
            GUI.Label(new Rect(Mathf.Cos(g) * 7f * s, giua - 140f * s + Mathf.Sin(g) * 7f * s, Screen.width, 280f * s),
                      con.ToString(), ks);
        }
        ks.normal.textColor = mau;
        GUI.Label(new Rect(0, giua - 140f * s, Screen.width, 280f * s), con.ToString(), ks);
        GUI.matrix = cu;

        var km = GiaoDien.KieuTieuDe;
        var cm = km.alignment;
        km.alignment = TextAnchor.MiddleCenter;
        GiaoDien.Chu(new Rect(0, giua + 160f * s, Screen.width, 46f * s),
                     TenMan(PhongMang.PhongHienTai.manChoi).ToUpperInvariant(), km);
        km.alignment = cm;
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

    static readonly string[] MoTaMucDoHoa =
    {
        "100% độ phân giải · bóng mềm · khử răng cưa",
        "75% độ phân giải · bóng cứng · tắt khử răng cưa",
        "50% độ phân giải · tắt bóng · cho máy yếu",
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
        float cao = Mathf.Min(Screen.height - 30f * s, 640f * s);
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
        GiaoDien.Chu(new Rect(x, y, rong, 30f * s), "Độ phân giải và chất lượng hình", GiaoDien.KieuChuMo);
        y += 42f * s;

        float caoHang = 72f * s;
        float rongTen = 280f * s;
        for (int i = 0; i < 3; i++)
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

            y += caoHang + 10f * s;
        }
    }
}
