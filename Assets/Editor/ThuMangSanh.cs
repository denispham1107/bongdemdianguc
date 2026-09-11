using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU DUONG MANG: dang nhap - tao phong - vao phong - san sang - dem nguoc.
///
/// Chay THAT tren Firebase chu khong gia lap: dung dung tai khoan, dung dung
/// co so du lieu ma nguoi choi se dung. Vi vay no de lai rac neu dut giua
/// chung - buoc cuoi luon don phong da tao.
///
/// Phai vao Play mode vi tat ca deu la coroutine (UnityWebRequest khong chay
/// ngoai Play). Dong ho dung <c>Time.realtimeSinceStartup</c> chu khong phai
/// <c>Time.time</c>: o day dang do THOI GIAN MANG chu khong phai nhip game,
/// va may khung dau <c>Time.time</c> bi kep lai.
///
/// Ket qua ghi ra <c>PlayTestShots/mang_sanh.txt</c>.
/// </summary>
public static class ThuMangSanh
{
    // Hai tai khoan da tao san khi chay thu tren web
    // Doc tu file ngoai git - xem ThongTinChayThu
    static string EmailA { get { return ThongTinChayThu.EmailA; } }
    static string EmailB { get { return ThongTinChayThu.EmailB; } }
    static string MatKhau { get { return ThongTinChayThu.MatKhau; } }

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static string maPhongDaTao;

    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/26. Chay thu MANG - dang nhap va phong cho", false, 112)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        // Thieu thong tin tai khoan thi dung han o day, dung vao Play roi
        // moi hong - vao Play xong bao loi thi nhin het nhu loi mang.
        if (!ThongTinChayThu.DocHoacBao()) return;

        // Cat phien dang nhap dang luu - khong thi ManDangNhap tu dang nhap
        // lai chen giua phep thu va doi tai khoan. Xem ThuKhoaTaiKhoan.Chay.
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().name != "MainMenu")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; maPhongDaTao = null;
        daBatDau = false;

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static bool daBatDau;

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;

        // Can mot MonoBehaviour de chay coroutine trong Play mode
        var go = new GameObject("TAM_ThuMang");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuMang] " + s);
    }

    static IEnumerator ChayKichBan()
    {
        float t0 = Time.realtimeSinceStartup;
        Ghi("[ban 4] phong chet bi XOA han, phong song khong bi dung toi");

        // ---- 1. DANG NHAP ----
        FirebaseMang.Quen();
        bool ok = false; string e = null;
        float t = Time.realtimeSinceStartup;
        yield return FirebaseMang.DangNhap(EmailB, MatKhau, (o, err) => { ok = o; e = err; });
        Ghi(string.Format("dang nhap B: {0} ({1:F0} ms)",
                          ok ? "OK" : "LOI - " + e, (Time.realtimeSinceStartup - t) * 1000f));
        if (!ok) { loi++; Ket(); yield break; }

        // ---- 2. HO SO ----
        t = Time.realtimeSinceStartup;
        yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi(string.Format("tai ho so: {0}, ten = {1}, {2} thang / {3} tran ({4:F0} ms)",
                          ok ? "OK" : "LOI - " + e, HoSoMang.CuaToi.ten,
                          HoSoMang.CuaToi.soTranThang, HoSoMang.CuaToi.soTranChoi,
                          (Time.realtimeSinceStartup - t) * 1000f));
        if (!ok) { loi++; Ket(); yield break; }

        // ---- 3. DO DONG HO MAY CHU ----
        t = Time.realtimeSinceStartup;
        yield return PhongMang.DoDongHoMayChu();
        Ghi(string.Format("do dong ho may chu: {0:F0} ms", (Time.realtimeSinceStartup - t) * 1000f));

        // ---- 4. TAO PHONG ----
        t = Time.realtimeSinceStartup;
        yield return PhongMang.TaoPhong("Phong chay thu", "Act2", (o, err) => { ok = o; e = err; });
        Ghi(string.Format("tao phong: {0} ({1:F0} ms)",
                          ok ? "OK" : "LOI - " + e, (Time.realtimeSinceStartup - t) * 1000f));
        if (!ok) { loi++; Ket(); yield break; }

        maPhongDaTao = PhongMang.PhongHienTai.ma;
        Ghi("  ma phong = " + maPhongDaTao
            + ", man = " + PhongMang.PhongHienTai.manChoi
            + ", host = " + PhongMang.PhongHienTai.hostTen
            + ", la host = " + PhongMang.LaHost
            + ", so nguoi = " + PhongMang.PhongHienTai.soNguoi);

        if (!PhongMang.LaHost) { Ghi("[LOI] nguoi tao phong ma khong phai host"); loi++; }

        // ---- 5. DOC LAI DANH SACH: co thay phong minh vua tao khong ----
        t = Time.realtimeSinceStartup;
        System.Collections.Generic.List<PhongMang.Phong> ds = null;
        yield return PhongMang.LayDanhSach(k => ds = k);
        var thay = ds != null ? ds.Find(p => p.ma == maPhongDaTao) : null;
        Ghi(string.Format("doc danh sach: {0} phong, co phong vua tao = {1} ({2:F0} ms)",
                          ds == null ? 0 : ds.Count, thay != null,
                          (Time.realtimeSinceStartup - t) * 1000f));
        if (thay == null) { Ghi("[LOI] tao phong xong ma danh sach khong thay"); loi++; }

        // ---- 6. DOI MAN CHOI ----
        yield return PhongMang.DoiManChoi("Act1", null);
        yield return PhongMang.TaiLaiPhong(maPhongDaTao, (o, err) => { });
        Ghi("doi man sang Act1 -> doc lai duoc: " + PhongMang.PhongHienTai.manChoi);
        if (PhongMang.PhongHienTai.manChoi != "Act1") { Ghi("[LOI] doi man khong an"); loi++; }

        // ---- 7. BAT DAU DEM NGUOC ----
        t = Time.realtimeSinceStartup;
        yield return PhongMang.BatDauDemNguoc(null);
        yield return PhongMang.TaiLaiPhong(maPhongDaTao, (o, err) => { });

        double conLai = PhongMang.ConLaiGiay();
        Ghi(string.Format("bat dau dem nguoc: trang thai = {0}, con lai {1:F2} giay ({2:F0} ms)",
                          PhongMang.PhongHienTai.trangThai, conLai,
                          (Time.realtimeSinceStartup - t) * 1000f));

        if (PhongMang.PhongHienTai.trangThai != "demNguoc")
        { Ghi("[LOI] bam bat dau ma trang thai khong doi"); loi++; }

        // Dem nguoc dat 10 giay. Cho phep lech 1,5 giay cho duong truyen.
        if (conLai < PhongMang.GiayDemNguoc - 1.5 || conLai > PhongMang.GiayDemNguoc + 0.5)
        { Ghi("[LOI] dem nguoc lech qua nhieu so voi " + PhongMang.GiayDemNguoc + " giay"); loi++; }

        // ---- 8. DOI 3 GIAY ROI DO LAI: dong ho co chay deu khong ----
        double truocKhiDoi = PhongMang.ConLaiGiay();
        yield return new WaitForSecondsRealtime(3f);
        double sauKhiDoi = PhongMang.ConLaiGiay();
        double daChay = truocKhiDoi - sauKhiDoi;
        Ghi(string.Format("sau 3 giay thuc: dem nguoc tut {0:F2} giay (dung ra phai ~3,00)", daChay));
        if (daChay < 2.5 || daChay > 3.5) { Ghi("[LOI] dong ho dem nguoc chay sai nhip"); loi++; }

        // ---- 8a. PHONG MA CO BI LOC KHOI SANH KHONG ----
        // REST khong co onDisconnect, nen phong cua nguoi dong tab nam lai
        // vinh vien. Do o hai phia: phong vua dap nhip phai SONG, phong lang
        // qua nguong phai CHET - chi do mot phia thi khong phan biet duoc
        // "loc dung" voi "loc sach tron".
        yield return PhongMang.DapNhip();
        yield return PhongMang.TaiLaiPhong(maPhongDaTao, (o, err) => { });
        var phongMoi = PhongMang.PhongHienTai;

        double gio = PhongMang.GioMayChu();
        Ghi("phong vua dap nhip -> con song = " + PhongMang.PhongConSong(phongMoi, gio)
            + " (phai la True)");
        if (!PhongMang.PhongConSong(phongMoi, gio))
        { Ghi("[LOI] phong dang mo ma bi coi la chet"); loi++; }

        // Dich dong ho toi truoc nguong mot chut: phong y het the phai chet
        double gioSau = gio + (PhongMang.GiayCoiLaChet + 5.0) * 1000.0;
        Ghi("cung phong do, " + (PhongMang.GiayCoiLaChet + 5) + " giay sau -> con song = "
            + PhongMang.PhongConSong(phongMoi, gioSau) + " (phai la False)");
        if (PhongMang.PhongConSong(phongMoi, gioSau))
        { Ghi("[LOI] phong lang qua nguong ma van bi coi la song"); loi++; }

        // Va danh sach that: phong vua tao phai co mat, phong ma khong duoc co
        System.Collections.Generic.List<PhongMang.Phong> dsSong = null;
        yield return PhongMang.LayDanhSach(k => dsSong = k);
        int soMa = 0;
        double g2 = PhongMang.GioMayChu();
        if (dsSong != null)
            foreach (var q in dsSong)
                if (!PhongMang.PhongConSong(q, g2)) soMa++;
        Ghi("danh sach sanh: " + (dsSong == null ? 0 : dsSong.Count)
            + " phong, trong do phong ma = " + soMa + " (phai la 0)");
        if (soMa > 0) { Ghi("[LOI] phong ma van lot vao sanh"); loi++; }

        // ---- 8a-2. PHONG CHET CO BI XOA HAN KHONG ----
        // Loc khoi sanh moi la giau di; phai xoa han thi co so du lieu moi sach.
        // Dung mot phong gia da chet han: ghi capNhatLuc lui ve qua khu.
        string maGia = null;
        yield return FirebaseMang.Them("phong",
            "{\"ten\":\"Phong ma chay thu\",\"hostUid\":\"" + FirebaseMang.Uid + "\","
            + "\"hostTen\":\"" + FirebaseMang.TenHienThi + "\",\"manChoi\":\"Act2\","
            + "\"trangThai\":\"cho\",\"toiDa\":4,\"soNguoi\":1,"
            + "\"taoLuc\":" + (long)(PhongMang.GioMayChu() - 600000)
            + ",\"capNhatLuc\":" + (long)(PhongMang.GioMayChu() - 600000) + "}",
            (ma, e2) => { maGia = ma; });

        Ghi("dung mot phong ma (im lang 600 giay): " + (maGia ?? "KHONG TAO DUOC"));

        if (!string.IsNullOrEmpty(maGia))
        {
            // Doc danh sach - chinh viec doc phai keo theo don dep
            yield return PhongMang.LayDanhSach(k => { });

            string conLaiJson = null;
            yield return FirebaseMang.Doc("phong/" + maGia, s => conLaiJson = s);
            bool daXoa = string.IsNullOrEmpty(conLaiJson) || conLaiJson == "null";
            Ghi("sau khi doc sanh, phong ma con khong: " + (daXoa ? "da bi xoa" : "VAN CON"));
            if (!daXoa) { Ghi("[LOI] phong chet khong bi don"); loi++; }

            // Va phong THAT thi khong duoc dung toi - neu no cung bien mat thi
            // hoa ra dang xoa sach moi thu chu khong phai loc dung.
            string phongThat = null;
            yield return FirebaseMang.Doc("phong/" + maPhongDaTao, s => phongThat = s);
            bool conNguyen = !string.IsNullOrEmpty(phongThat) && phongThat != "null";
            Ghi("phong dang mo cua minh con nguyen: " + conNguyen + " (phai la True)");
            if (!conNguyen) { Ghi("[LOI] don nham ca phong dang song"); loi++; }
        }

        // ---- 8b. KHACH CO NHAY VAO TRAN DUOC KHONG ----
        // Loi that da gap: host vao tran, con nguoi choi kia ket lai o MainMenu.
        // Do khach chi vao khi trangThai == "demNguoc", ma host vua vao la ghi
        // de thanh "dangChoi". Doan nay dung DUNG tinh huong do tren Firebase
        // that roi hoi lai y het cach may khach hoi.
        var truoc = PhongMang.PhongHienTai;
        double conLaiBayGio = PhongMang.ConLaiGiay();
        Ghi(string.Format("con {0:F1} giay, dang dem nguoc -> khach vao tran = {1} (phai la False)",
                          conLaiBayGio, PhongMang.DenGioVaoTran(truoc, conLaiBayGio)));
        if (PhongMang.DenGioVaoTran(truoc, conLaiBayGio))
        { Ghi("[LOI] chua het gio ma da doi vao tran"); loi++; }

        // Het gio ma trang thai van la "demNguoc" thi phai vao
        if (!PhongMang.DenGioVaoTran(truoc, -0.01))
        { Ghi("[LOI] het gio dem nguoc ma khong vao tran"); loi++; }
        else Ghi("het gio dem nguoc -> khach vao tran = True");

        // Host vao truoc mot nhip: ghi that len Firebase roi doc lai
        yield return PhongMang.DanhDauDangChoi(null);
        yield return PhongMang.TaiLaiPhong(maPhongDaTao, (o, err) => { });
        var sau = PhongMang.PhongHienTai;

        Ghi("host da vao tran -> trang thai doc duoc = " + sau.trangThai
            + ", khach vao tran = " + PhongMang.DenGioVaoTran(sau, 999));

        if (sau.trangThai != "dangChoi")
        { Ghi("[LOI] host vao tran ma trang thai khong doi"); loi++; }

        if (!PhongMang.DenGioVaoTran(sau, 999))
        { Ghi("[LOI] host da vao ma khach van bi ket lai o sanh"); loi++; }

        // ---- 9. NGUOI THU HAI VAO PHONG ----
        // Dang nhap tai khoan A de bat chuoc nguoi choi khac. Phai luu lai
        // phien cua B truoc, khong thi buoc don dep phia sau khong con quyen.
        string uidB = FirebaseMang.Uid;
        string tokenB = FirebaseMang.IdToken;
        string tenB = FirebaseMang.TenHienThi;

        t = Time.realtimeSinceStartup;
        yield return FirebaseMang.DangNhap(EmailA, MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi(string.Format("dang nhap A: {0} ({1:F0} ms)",
                          ok ? "OK, ten = " + HoSoMang.CuaToi.ten : "LOI - " + e,
                          (Time.realtimeSinceStartup - t) * 1000f));

        if (ok)
        {
            // Phong dang o trang thai demNguoc nen VaoPhong se tu choi - dung
            // nhu thiet ke. Kiem tra dung dieu do.
            bool vaoDuoc = false; string loiVao = null;
            yield return PhongMang.VaoPhong(maPhongDaTao, (o, err) => { vaoDuoc = o; loiVao = err; });
            Ghi("vao phong dang dem nguoc: " + (vaoDuoc ? "VAO DUOC" : "bi tu choi - " + loiVao));
            if (vaoDuoc) { Ghi("[LOI] phong da bat dau ma van cho nguoi moi vao"); loi++; }
        }

        // ---- 10. DON DEP ----
        yield return FirebaseMang.DangNhap(EmailB, MatKhau, (o, err) => { ok = o; });
        if (ok && !string.IsNullOrEmpty(maPhongDaTao))
        {
            yield return FirebaseMang.Xoa("phong/" + maPhongDaTao, (o, err) => { });
            yield return FirebaseMang.Xoa("tran/" + maPhongDaTao, (o, err) => { });

            string con = null;
            yield return FirebaseMang.Doc("phong/" + maPhongDaTao, s => con = s);
            bool sach = string.IsNullOrEmpty(con) || con == "null";
            Ghi("don phong chay thu: " + (sach ? "sach" : "VAN CON"));
            if (!sach) loi++;
        }

        Ghi(string.Format("tong thoi gian: {0:F1} giay", Time.realtimeSinceStartup - t0));
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/mang_sanh.txt", bao.ToString());

        var rac = GameObject.Find("TAM_ThuMang");
        if (rac != null) Object.DestroyImmediate(rac);

        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraPhien;
    }

    static void TraPhien()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraPhien;
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc);
        else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
    }
}
