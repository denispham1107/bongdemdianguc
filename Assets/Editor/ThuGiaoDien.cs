using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: GIAO DIEN DANG NHAP - SANH CHO - TRONG PHONG - DEM NGUOC - CAI DAT.
///
/// Nguoi dung bao: vao game van hien menu choi don cu; giao dien so sai, nhieu
/// cho chu tran bi che; bang Cai dat mat dau tieng Viet ("CAI D T").
///
/// Phep thu nay di het cac man, o moi man:
///   - dem so lan chu KHONG VUA O (GiaoDien.SoLanCat: phai cat bot va them
///     "…") va so lan phai thu nho co chu - dem trong chinh ham ve, nen moi
///     nhan, moi nut deu duoc tinh, khong sot cai nao;
///   - hoi kieu chu dang dung co that la font Inter khong (font mac dinh thieu
///     chu co dau);
///   - chup anh de nhin.
/// Va kiem: da dang nhap san ma quay ve MainMenu thi vao THANG sanh, khong
/// hien menu choi don cu.
///
/// Anh ra <c>PlayTestShots/gd_*.png</c>, so do ra <c>PlayTestShots/giaodien.txt</c>.
/// Buoc cuoi xoa phong da tao, dang xuat, tra lai phien dang nhap cu.
/// </summary>
public static class ThuGiaoDien
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";

    [MenuItem("Diablo 2.5D/50. Chay thu GIAO DIEN dang nhap - sanh - phong", false, 137)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (!ThongTinChayThu.DocHoacBao()) return;

        canhCu = EditorSceneManager.GetActiveScene().path;
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien);           // de man dang nhap hien ra - xem ChupManMang
        PlayerPrefs.Save();

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] giao dien dang nhap - sanh - phong");
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuGiaoDien");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GiaoDien] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 60 && !File.Exists(duong); i++)
            yield return new WaitForEndOfFrame();
    }

    /// <summary>Dem chu tran trong vai khung hinh roi chup.</summary>
    static IEnumerator DoMan(string ten, string anh)
    {
        GiaoDien.DatLaiDem();
        for (int i = 0; i < 4; i++) yield return null;
        int cat = GiaoDien.SoLanCat, nho = GiaoDien.SoLanThuNho, ve = GiaoDien.SoLuotVe;
        string chuCat = GiaoDien.ChuBiCatCuoi;
        yield return Chup(anh);
        Ghi(string.Format("{0}: {5} luot ve, chu bi cat {1} lan{2}, chu phai thu nho {3} lan - anh {4}.png",
                          ten, cat, cat > 0 ? " (vd \"" + chuCat + "\")" : "", nho, anh, ve));
        // Khong ve gi thi hai so tren bang 0 ma chang chung minh duoc gi -
        // lan chay dau cua phep thu nay da bao "0 lan cat" cho mot sanh trong tron.
        if (GiaoDien.ChuBiThuNho.Count > 0)
            Ghi("      chu phai thu nho: " + string.Join(" | ", GiaoDien.ChuBiThuNho));
        Kiem(ve > 0, ten + ": KHONG CO GI DUOC VE");
        Kiem(cat == 0, ten + ": co chu khong vua o, phai cat bot");
    }

    /// <summary>
    /// Hoan nhip hoi lai cua sanh 30 giay - de du lieu gia dat vao khong bi ban
    /// tai tu may chu ghi de truoc luc chup. Lan chay dau, anh "phong du bon
    /// nguoi" van hien 1/4 vi mot nhip hoi lai den giua chung.
    /// </summary>
    static void HoanHoiLai(ManSanh sanh)
    {
        var f = typeof(ManSanh).GetField("hoiLanSau", BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null) f.SetValue(sanh, Time.unscaledTime + 30f);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("man hinh Game: " + Screen.width + "x" + Screen.height + ", ti le giao dien " + GiaoDien.TiLe.ToString("F2"));
        yield return new WaitForSecondsRealtime(0.8f);

        // ---- 1. Dang nhap ----
        Kiem(!FirebaseMang.DaDangNhap, "chua xoa duoc phien cu - khong co man dang nhap de do");
        yield return DoMan("1. man dang nhap", "gd_1_dangnhap");

        string fontChu = GiaoDien.KieuChu != null && GiaoDien.KieuChu.font != null ? GiaoDien.KieuChu.font.name : "(khong co)";
        string fontDam = GiaoDien.KieuNutMau != null && GiaoDien.KieuNutMau.font != null ? GiaoDien.KieuNutMau.font.name : "(khong co)";
        Ghi("   font dang dung: chu thuong = " + fontChu + ", nut = " + fontDam);
        Kiem(fontChu == "Inter-Regular" && fontDam == "Inter-SemiBold", "giao dien khong dung font Inter");

        var dn = Object.FindAnyObjectByType<ManDangNhap>();
        var fTrang = typeof(ManDangNhap).GetField("trang", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dn != null && fTrang != null)
        {
            fTrang.SetValue(dn, ManDangNhap.Trang.DangKy);
            yield return DoMan("2. tab tao tai khoan", "gd_2_dangky");
            fTrang.SetValue(dn, ManDangNhap.Trang.DangNhap);
        }

        // ---- 2. Sanh ----
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] dang nhap: " + e); loi++; Ket(); yield break; }
        if (dn != null && dn.daVao != null) dn.daVao();
        yield return new WaitForSecondsRealtime(2.5f);
        {
            var dsSanh = Object.FindObjectsByType<ManSanh>(FindObjectsInactive.Include);
            var dsMenu = Object.FindObjectsByType<MainMenuUI>(FindObjectsInactive.Include);
            var dsDn = Object.FindObjectsByType<ManDangNhap>(FindObjectsInactive.Include);
            string bat = "";
            foreach (var ms in dsSanh) bat += (ms.enabled ? "bat" : "tat") + (ms.isActiveAndEnabled ? "/chay " : "/khong ");
            Ghi("   chan doan: MainMenuUI " + dsMenu.Length + ", ManDangNhap " + dsDn.Length + ", ManSanh " + dsSanh.Length
                + " [" + bat.Trim() + "], da dang nhap = " + FirebaseMang.DaDangNhap
                + ", daVao cua dn = " + (dn != null && dn.daVao != null));
        }
        yield return DoMan("3. sanh cho", "gd_3_sanh");

        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh == null) { Ghi("[LOI] khong co sanh"); loi++; Ket(); yield break; }

        // ---- 3b. Danh sach co phong - du lieu gia CHI trong may, sanh hoi lai
        // moi 2 giay se ghi de, nen chup ngay. Mot phong ten dai het co, mot
        // phong da day - hai truong hop de tran chu nhat.
        {
            var fDs = typeof(ManSanh).GetField("danhSach", BindingFlags.NonPublic | BindingFlags.Instance);
            var ds = new System.Collections.Generic.List<PhongMang.Phong>();
            var p1 = new PhongMang.Phong { ma = "gia1", ten = "Nghĩa địa không lối thoát", hostUid = "x",
                                           hostTen = "KẻĐiSănĐêmKhuya99", manChoi = "Act2", trangThai = "cho", soNguoi = 2, toiDa = 4 };
            var p2 = new PhongMang.Phong { ma = "gia2", ten = "Đấu trường máu", hostUid = "y",
                                           hostTen = "Bóng Ma", manChoi = "Act1", trangThai = "cho", soNguoi = 4, toiDa = 4 };
            ds.Add(p1); ds.Add(p2);
            HoanHoiLai(sanh);
            yield return new WaitForSecondsRealtime(2.5f);     // cho luot hoi dang bay (neu co) ve het
            fDs.SetValue(sanh, ds);
            yield return DoMan("3b. sanh co hai phong (du lieu gia)", "gd_3b_sanh_coPhong");
        }

        sanh.MoCaiDat();
        yield return DoMan("4. bang cai dat", "gd_4_caidat");
        sanh.BamOKCaiDat();          // khong doi gi -> chi dong

        // ---- 3. Quay ve MainMenu khi DA dang nhap -> vao thang sanh ----
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        yield return new WaitForSecondsRealtime(1.5f);
        var sanhMoi = Object.FindAnyObjectByType<ManSanh>();
        bool vaoThang = sanhMoi != null && sanhMoi.enabled;
        Ghi("5. nap lai MainMenu khi dang dang nhap -> sanh hien ngay: " + (vaoThang ? "co" : "KHONG (hien menu choi don cu)"));
        Kiem(vaoThang, "quay ve MainMenu ma khong vao thang sanh");
        yield return DoMan("   sanh sau khi nap lai", "gd_5_vethang_sanh");
        sanh = sanhMoi;

        // ---- 4. Trong phong ----
        yield return PhongMang.TaoPhong("", "Act2", (o, err) => { ok = o; e = err; });
        Ghi("   tao phong: " + (ok ? "OK - " + PhongMang.PhongHienTai.ma + " \"" + PhongMang.PhongHienTai.ten + "\"" : "LOI - " + e));
        if (ok && sanh != null)
        {
            var f = typeof(ManSanh).GetField("dangO", BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(sanh, System.Enum.Parse(f.FieldType, "TrongPhong"));
            yield return new WaitForSecondsRealtime(2.0f);
            yield return DoMan("6. trong phong (chu phong)", "gd_6_trongphong");

            // 6b. Phong du bon nguoi - them ba nguoi GIA vao ban sao trong may
            {
                HoanHoiLai(sanh);
                yield return new WaitForSecondsRealtime(1.5f);
                var pp = PhongMang.PhongHienTai;
                pp.nguoiChoi.Add(new PhongMang.NguoiTrongPhong { uid = "g1", ten = "Nguyễn Thị Hằng Nga", sanSang = true, cho = 1 });
                pp.nguoiChoi.Add(new PhongMang.NguoiTrongPhong { uid = "g2", ten = "ĐồTểLàngMa", sanSang = false, cho = 2 });
                pp.nguoiChoi.Add(new PhongMang.NguoiTrongPhong { uid = "g3", ten = "Bóng Ma", sanSang = true, cho = 3 });
                yield return DoMan("6b. phong du bon nguoi (du lieu gia)", "gd_6b_phong_4nguoi");
            }

            // ---- 5. Dem nguoc - chi dat o ban sao trong may, KHONG ghi len
            // Firebase: dem nguoc that thi 10 giay sau nap Act2 va danh dau
            // phong "dang choi".
            var p = PhongMang.PhongHienTai;
            string cu = p.trangThai; double cuLuc = p.batDauLuc;
            p.trangThai = "demNguoc";
            p.batDauLuc = PhongMang.GioMayChu() + 7400.0;
            yield return DoMan("7. dem nguoc", "gd_7_demnguoc");
            p.trangThai = cu; p.batDauLuc = cuLuc;

            string ma = p.ma;
            yield return FirebaseMang.Xoa("phong/" + ma, (o2, e2) => { });
            yield return FirebaseMang.Xoa("tran/" + ma, (o2, e2) => { });
            PhongMang.PhongHienTai = null;
            Ghi("   don phong thu: xong");
        }

        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/giaodien.txt", bao.ToString());

        FirebaseMang.Quen();
        var rac = GameObject.Find("TAM_ThuGiaoDien");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;

        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc);
        else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);
        Debug.Log("[GiaoDien] da tra lai phien dang nhap va scene " + EditorSceneManager.GetActiveScene().path
                  + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
