using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

/// <summary>
/// CHAY THU: NUT CAI DAT VA BON MUC DO HOA.
///
/// Nguoi dung xin: nut "Cai dat" o sanh (ben trai VAO PHONG NHANH), tab
/// "Giao dien" co cac muc Cao / Trung binh / Yeu / Rat yeu, bam OK thi game tai
/// lai va chay dung muc da chon. Tu 12/09/2026: muc thap CHI thu nho canh 3D
/// (KetXuatThuNho) - chu, khung, nut giu du net nhu muc Cao.
///
/// Do nhung gi:
///   A. Ngoai Play
///      1. Moi chu co dau trong ManSanh.cs deu co trong font cua OnGUI - quet
///         thang file nguon, khong chep lai danh sach chu (chep lai thi them
///         mot chu moi la phep thu khong biet).
///      2. Vi tri nut CAI DAT o nhieu kich co man hinh: khong de len tieu de
///         "Phong dang cho", khong de len VAO PHONG NHANH, chu vua trong nut.
///      3. Trang web KHONG con ha devicePixelRatio cua khung game (ha thi ca
///         giao dien nhoe theo) va khong doc khoa cai dat nua.
///      3b. Chuyen khoa cu (3 muc) sang khoa moi (4 muc): 2 cu = Rat yeu.
///   B. Trong Play (dang nhap that bang tai khoan chay thu)
///      4. Mo bang, chup anh.
///      5. OK ma khong doi gi: bang dong, game KHONG tai lai.
///      6. Lan luot Trung binh -> Yeu -> Rat yeu -> Cao: bam OK, doi game tai
///         lai, doc lai tu KHO LUU (khong tin bien trong bo nho), muc chat
///         luong Unity, bong, khu rang cua; roi vao Act2 do khoang bong, so vat
///         do bong, va anh dem canh 3D (kich thuoc, co phong len man hinh, go
///         ra ngoai luc ve de toa do OnGUI / chuot khong lech, dung sau bloom).
///
/// Cuoi cung tra lai: muc do hoa, phien dang nhap, muc chat luong cua Editor,
/// scene dang mo.
///
/// Ket qua ghi ra <c>PlayTestShots/caidat.txt</c>, anh <c>PlayTestShots/caidat_*.png</c>.
/// </summary>
public static class ThuCaiDat
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    // Tra lai sau khi do
    static bool coMucGoc; static int mucGoc;
    static bool coMucCu; static int mucCu;
    static bool coPhienGoc; static string phienGoc;
    static int chatLuongGoc;

    const string KhoaPhien = "diablo25d_refresh";

    [MenuItem("Diablo 2.5D/48. Chay thu CAI DAT do hoa", false, 135)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (!ThongTinChayThu.DocHoacBao()) return;

        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 2] nut CAI DAT va bon muc do hoa");

        // ---- A. Ngoai Play ----
        KiemFont();
        KiemViTri();
        KiemTrangWeb();
        KiemChuyenKhoaCu();

        // ---- B. Trong Play ----
        canhCu = EditorSceneManager.GetActiveScene().path;

        coMucGoc = PlayerPrefs.HasKey(CaiDatDoHoa.Khoa);
        mucGoc = PlayerPrefs.GetInt(CaiDatDoHoa.Khoa, 0);
        coMucCu = PlayerPrefs.HasKey(CaiDatDoHoa.KhoaCu);
        mucCu = PlayerPrefs.GetInt(CaiDatDoHoa.KhoaCu, 0);
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        chatLuongGoc = QualitySettings.GetQualityLevel();

        // Bat dau tu muc Cao nhu nguoi choi moi. Bo phien cu de man dang nhap
        // khong tu dang nhap chen ngang (xem ChupManMang).
        PlayerPrefs.SetInt(CaiDatDoHoa.Khoa, 0);
        PlayerPrefs.DeleteKey(CaiDatDoHoa.KhoaCu);
        PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuCaiDat");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CaiDat] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    // =============================================================
    // A1. FONT
    // =============================================================

    /// <summary>Cac file co chu hien len man dang nhap, sanh, phong, cai dat.</summary>
    public static readonly string[] TepGiaoDien =
    {
        "Assets/Scripts/UI/ManSanh.cs", "Assets/Scripts/UI/ManDangNhap.cs",
        "Assets/Scripts/UI/GiaoDien.cs", "Assets/Scripts/CaiDatDoHoa.cs",
        "Assets/Scripts/Mang/FirebaseMang.cs", "Assets/Scripts/Mang/PhongMang.cs",
        "Assets/Scripts/Mang/HoSoMang.cs", "Assets/Scripts/Mang/KhoiDongTranMang.cs",
    };

    static void KiemFont()
    {
        // DOC THANG BANG KY TU CUA FILE FONT se dong goi vao game - KHONG hoi
        // Font.HasCharacter. Ban cu cua phep thu nay hoi font mac dinh bang
        // HasCharacter va bao "thieu: khong"; len ban web thi "CAI DAT" hien
        // thanh "CAI D T" - trong Editor, Windows ve bu bang Arial va
        // HasCharacter tinh ca phan ay.
        string[] fonts = { "Assets/Resources/Fonts/Inter-Regular.ttf", "Assets/Resources/Fonts/Inter-SemiBold.ttf" };
        var bang = new System.Collections.Generic.List<System.Collections.Generic.HashSet<int>>();
        foreach (var f in fonts)
        {
            var b = BangKyTuFont.Doc(f);
            if (b == null) { Ghi("[LOI] khong doc duoc bang ky tu cua " + f); loi++; return; }
            bang.Add(b);
        }

        // Doi chung: mot font BIET LA thieu tieng Viet (Lato di kem Unity) phai
        // bi bo doc bao thieu - khong thi bo doc hong va cai gi cung "du".
        string lato = EditorApplication.applicationContentsPath
            + "/Resources/PackageManager/BuiltInPackages/com.unity.render-pipelines.universal/Samples~/URPPackageSamples/SharedAssets/Fonts/Lato-Regular.ttf";
        var bangLato = BangKyTuFont.Doc(lato);
        int latoThieu = 0;
        if (bangLato != null)
            foreach (char c in BangKyTuFont.ChuVietCoDau) if (!bangLato.Contains(c)) latoThieu++;

        // Du 134 chu co dau
        var thieuBangChu = new StringBuilder();
        foreach (char c in BangKyTuFont.ChuVietCoDau)
            foreach (var b in bang) if (!b.Contains(c)) { thieuBangChu.Append(c); break; }

        // Moi chu (ca chuoi lan ky tu don) trong cac file giao dien
        var daXet = new System.Collections.Generic.HashSet<char>();
        var thieu = new StringBuilder();
        int soChuoiCoDau = 0;
        foreach (var tep in TepGiaoDien)
        {
            string nguon = File.ReadAllText(tep);
            foreach (Match mt in Regex.Matches(nguon, "\"([^\"\\\\]|\\\\.)*\"|'([^'\\\\]|\\\\.)'"))
            {
                bool coDau = false;
                foreach (char c in mt.Value)
                {
                    if (c < 128) continue;
                    coDau = true;
                    if (!daXet.Add(c)) continue;
                    foreach (var b in bang) if (!b.Contains(c)) { thieu.Append(c).Append(' '); break; }
                }
                if (coDau) soChuoiCoDau++;
            }
        }

        Ghi("1. font Inter (doc thang bang ky tu trong file): Regular " + bang[0].Count + " ky tu, SemiBold "
            + bang[1].Count + " ky tu; 134 chu co dau tieng Viet thieu: "
            + (thieuBangChu.Length == 0 ? "khong" : thieuBangChu.ToString()));
        Ghi("   doi chung - Lato cua Unity: " + (bangLato == null ? "khong tim thay file" : "thieu " + latoThieu + "/134 chu (phai > 0)"));
        Ghi("   " + soChuoiCoDau + " chuoi co dau trong " + TepGiaoDien.Length + " file giao dien, "
            + daXet.Count + " ky tu khac nhau, font thieu: " + (thieu.Length == 0 ? "khong" : thieu.ToString()));
        Kiem(thieuBangChu.Length == 0, "font Inter thieu chu tieng Viet: " + thieuBangChu);
        Kiem(thieu.Length == 0, "font thieu ky tu dang dung: " + thieu);
        if (bangLato != null) Kiem(latoThieu > 0, "bo doc bang ky tu bao Lato du tieng Viet - bo doc hong");
    }

    // =============================================================
    // A2. VI TRI NUT
    // =============================================================

    static void KiemViTri()
    {
        var dam = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Inter-SemiBold.ttf");
        if (dam == null) { Ghi("[LOI] khong nap duoc font Inter-SemiBold"); loi++; return; }
        var kTieuDe = new GUIStyle { font = dam };
        var kNutDa = new GUIStyle { font = dam };
        var kNutMau = new GUIStyle { font = dam };

        int[,] manHinh =
        {
            { 1920, 1080 }, { 1366, 768 }, { 1280, 720 }, { 1024, 768 },
            { 2532, 1170 }, { 2400, 1080 }, { 2732, 2048 }, { 1080, 1920 }, { 1170, 2532 },
            { 1568, 581 },   // cua so Game trong Editor - cho menu 50 bat duoc chu bi cat
        };

        Ghi("2. vi tri nut CAI DAT o hang \"PHONG DANG CHO\" (co chu that cua Inter):");
        for (int i = 0; i < manHinh.GetLength(0); i++)
        {
            float W = manHinh[i, 0], H = manHinh[i, 1];
            float s = GiaoDien.TinhTiLe(W, H);
            var b = ManSanh.TinhBoCucSanh(W, H, s);

            // Co chu va dem hai ben lay dung nhu GiaoDien.ChuanBi
            kTieuDe.fontSize = Mathf.Max(10, Mathf.RoundToInt(25f * s));
            kNutDa.fontSize = Mathf.Max(10, Mathf.RoundToInt(20f * s));
            kNutMau.fontSize = Mathf.Max(10, Mathf.RoundToInt(23f * s));
            float dem = 2f * Mathf.RoundToInt(14f * s) + 4f;

            float rongTieuDe = kTieuDe.CalcSize(new GUIContent("PHÒNG ĐANG CHỜ")).x;
            float rongCaiDat = kNutDa.CalcSize(new GUIContent("CÀI ĐẶT")).x + dem;
            float rongNhanh = kNutMau.CalcSize(new GUIContent("VÀO PHÒNG NHANH")).x + dem;

            var cd = b.nutCaiDat; var vn = b.nutVaoNhanh; var td = b.tieuDePhongCho;
            bool deNhau = cd.Overlaps(vn);
            bool deTieuDe = td.x + rongTieuDe > cd.x;
            bool trongKhung = cd.x >= b.khungDanhSach.x && vn.xMax <= b.khungDanhSach.xMax
                              && b.khungDanhSach.x >= 0f && b.khungDanhSach.xMax <= W;
            bool vuaChu = rongCaiDat <= cd.width && rongNhanh <= vn.width;

            Ghi(string.Format("   {0}x{1}: ti le {2:F2}, tieu de het o {3:F0}, CAI DAT {4:F0}..{5:F0} (chu {6:F0}/{7:F0}), VAO NHANH {8:F0}..{9:F0} (chu {10:F0}/{11:F0}) -> {12}",
                W, H, s, td.x + rongTieuDe, cd.x, cd.xMax, rongCaiDat, cd.width, vn.x, vn.xMax, rongNhanh, vn.width,
                (!deNhau && !deTieuDe && trongKhung && vuaChu) ? "on"
                : (deNhau ? "DE NHAU " : "") + (deTieuDe ? "DE TIEU DE " : "")
                  + (!trongKhung ? "RA NGOAI " : "") + (!vuaChu ? "CHU TRAN" : "")));
            Kiem(!deNhau && !deTieuDe && trongKhung && vuaChu, "hang PHONG DANG CHO sai o man hinh " + W + "x" + H);
        }
    }

    // =============================================================
    // A3. TRANG WEB (index.html) - chay that bang node
    // =============================================================

    static void KiemTrangWeb()
    {
        const string DuongMau = "Assets/WebGLTemplates/Diablo25D/index.html";
        string html = File.ReadAllText(DuongMau);

        // Bo chu thich JS/CSS/HTML truoc khi do - chu thich duoc phep nhac ten
        string ma = Regex.Replace(html, @"//[^\n]*|/\*.*?\*/|<!--.*?-->", "", RegexOptions.Singleline);
        bool datDpr = Regex.IsMatch(ma, @"devicePixelRatio\s*=");
        bool docKhoa = ma.Contains("diablo25d.mucDoHoa");
        Ghi("3. trang web: dat devicePixelRatio " + (datDpr ? "CO" : "khong") + ", doc khoa cai dat " + (docKhoa ? "CO" : "khong")
            + " (ca hai phai 'khong': khung game luon du net, chi canh 3D thu nho trong game)");
        Kiem(!datDpr, "index.html van ha devicePixelRatio - giao dien se nhoe o muc thap");
        Kiem(!docKhoa, "index.html van doc khoa cai dat");
    }

    static void KiemChuyenKhoaCu()
    {
        // Chay THAT duong doc cua CaiDatDoHoa voi tung gia tri khoa cu; tra lai kho sau
        bool coMoi = PlayerPrefs.HasKey(CaiDatDoHoa.Khoa), coCu = PlayerPrefs.HasKey(CaiDatDoHoa.KhoaCu);
        int giuMoi = PlayerPrefs.GetInt(CaiDatDoHoa.Khoa, 0), giuCu = PlayerPrefs.GetInt(CaiDatDoHoa.KhoaCu, 0);
        int[] cu = { -1, 0, 1, 2 };
        MucDoHoa[] mong = { MucDoHoa.Cao, MucDoHoa.Cao, MucDoHoa.TrungBinh, MucDoHoa.RatYeu };
        var sb = new StringBuilder();
        for (int i = 0; i < cu.Length; i++)
        {
            PlayerPrefs.DeleteKey(CaiDatDoHoa.Khoa);
            if (cu[i] < 0) PlayerPrefs.DeleteKey(CaiDatDoHoa.KhoaCu); else PlayerPrefs.SetInt(CaiDatDoHoa.KhoaCu, cu[i]);
            CaiDatDoHoa.QuenBoNho();
            var m = CaiDatDoHoa.Muc;
            int ghiMoi = PlayerPrefs.GetInt(CaiDatDoHoa.Khoa, -1);
            sb.AppendFormat(" [cu {0} -> {1}, khoa moi {2}]", cu[i] < 0 ? "khong co" : cu[i].ToString(), CaiDatDoHoa.Ten[(int)m], ghiMoi);
            Kiem(m == mong[i], "chuyen khoa cu " + cu[i] + " sai: ra " + CaiDatDoHoa.Ten[(int)m]);
            if (cu[i] >= 0) Kiem(ghiMoi == (int)mong[i], "chuyen khoa cu " + cu[i] + " khong ghi sang khoa moi");
        }
        if (coMoi) PlayerPrefs.SetInt(CaiDatDoHoa.Khoa, giuMoi); else PlayerPrefs.DeleteKey(CaiDatDoHoa.Khoa);
        if (coCu) PlayerPrefs.SetInt(CaiDatDoHoa.KhoaCu, giuCu); else PlayerPrefs.DeleteKey(CaiDatDoHoa.KhoaCu);
        PlayerPrefs.Save();
        CaiDatDoHoa.QuenBoNho();
        Ghi("3b. chuyen khoa cu sang khoa moi:" + sb);
    }

    // =============================================================
    // B. TRONG PLAY
    // =============================================================

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 60 && !File.Exists(duong); i++)
            yield return new WaitForEndOfFrame();
    }

    static IEnumerator VaoSanh()
    {
        var dn = Object.FindAnyObjectByType<ManDangNhap>();
        if (dn != null && dn.daVao != null) dn.daVao();
        yield return new WaitForSecondsRealtime(1.0f);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("");
        yield return new WaitForSecondsRealtime(0.7f);

        // ---- 4. Dang nhap, vao sanh, mo bang ----
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau,
                                           (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] dang nhap: " + e); loi++; Ket(); yield break; }
        yield return VaoSanh();
        yield return new WaitForSecondsRealtime(1.5f);

        Ghi("4. vao sanh: muc dang dung = " + CaiDatDoHoa.Ten[(int)CaiDatDoHoa.Muc]
            + ", muc Unity = " + QualitySettings.names[QualitySettings.GetQualityLevel()]
            + ", man hinh Game " + Screen.width + "x" + Screen.height);
        Kiem(CaiDatDoHoa.Muc == MucDoHoa.Cao, "nguoi choi moi khong o muc Cao");
        Kiem(QualitySettings.names[QualitySettings.GetQualityLevel()] == "High",
             "muc Cao khong chay muc Unity High (muc WebGL mac dinh)");
        yield return Chup("caidat_1_sanh");

        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh == null || !sanh.enabled) { Ghi("[LOI] khong co sanh"); loi++; Ket(); yield break; }

        sanh.MoCaiDat();
        yield return new WaitForSecondsRealtime(0.5f);
        Kiem(sanh.DangMoCaiDat, "bam CAI DAT ma bang khong mo");
        yield return Chup("caidat_2_bang");
        Ghi("   mo bang: " + (sanh.DangMoCaiDat ? "mo" : "KHONG MO") + " - anh caidat_2_bang.png");

        // ---- 5. OK ma khong doi gi ----
        sanh.BamOKCaiDat();
        yield return null; yield return null; yield return null;
        bool conSanhCu = sanh != null;       // nap lai scene thi ManSanh cu bi huy
        Ghi("5. OK khi khong doi gi: bang " + (sanh != null && sanh.DangMoCaiDat ? "VAN MO" : "dong")
            + ", game " + (conSanhCu ? "khong tai lai" : "DA TAI LAI"));
        Kiem(conSanhCu && !sanh.DangMoCaiDat, "OK khi khong doi gi phai chi dong bang");

        // ---- 6. Ba muc ----
        MucDoHoa[] thuTu = { MucDoHoa.TrungBinh, MucDoHoa.Yeu, MucDoHoa.RatYeu, MucDoHoa.Cao };
        string[] mongMucUnity = { "High", "Medium", "Low", "Low" };
        float[] mongBong = { 60f, 40f, 25f, 20f };
        // Viet thang so, khong lay tu bang cua CaiDatDoHoa (phep kiem doc lap)
        float[] mongChiTietXa = { 1f, 0.7f, 0.55f, 0.4f };
        string[] mongKieuBong = { "All", "HardOnly", "HardOnly", "Disable" };
        var sangMuc = new float[CaiDatDoHoa.SoMuc];

        foreach (var m in thuTu)
        {
            int im = (int)m;
            Ghi("");
            Ghi("6. chon " + CaiDatDoHoa.Ten[im] + " roi bam OK:");

            sanh = Object.FindAnyObjectByType<ManSanh>();
            sanh.MoCaiDat();
            sanh.ChonMucDoHoa(m);
            yield return new WaitForSecondsRealtime(0.4f);
            if (m == MucDoHoa.RatYeu) yield return Chup("caidat_3_chon_ratyeu");

            float truoc = Time.realtimeSinceStartup;
            sanh.BamOKCaiDat();

            // Doi game tai lai: ManSanh cu bi huy, ManSanh moi xuat hien
            float han = Time.realtimeSinceStartup + 15f;
            while (sanh != null && Time.realtimeSinceStartup < han) yield return null;
            while (Object.FindAnyObjectByType<ManSanh>() == null && Time.realtimeSinceStartup < han)
                yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            bool daNapLai = sanh == null;

            // Doc lai tu KHO LUU, khong tin bien trong bo nho
            int trongKho = PlayerPrefs.GetInt(CaiDatDoHoa.Khoa, -1);
            string mucUnity = QualitySettings.names[QualitySettings.GetQualityLevel()];

            Ghi(string.Format("   game tai lai: {0} (sau {1:F1} giay), kho luu = {2}, muc Unity = {3}",
                daNapLai ? "co" : "KHONG", Time.realtimeSinceStartup - truoc, trongKho, mucUnity));
            Ghi(string.Format("   bong = {0}, phan giai bong = {1}, khu rang cua = {2}x, den tinh tung diem anh = {3}, vSync = {4}",
                QualitySettings.shadows, QualitySettings.shadowResolution, QualitySettings.antiAliasing,
                QualitySettings.pixelLightCount, QualitySettings.vSyncCount));

            Kiem(daNapLai, "bam OK ma game khong tai lai");
            Kiem(trongKho == im, "kho luu khong ghi muc " + CaiDatDoHoa.Ten[im]);
            Kiem(mucUnity == mongMucUnity[im], "muc Unity phai la " + mongMucUnity[im]);
            Kiem(QualitySettings.vSyncCount == 1, "vSync phai giu 1 nhu muc Cao");
            Ghi(string.Format("   chi tiet xa (lodBias) = {0} (mong {1}), kieu bong = {2} (mong {3})",
                QualitySettings.lodBias, mongChiTietXa[im], QualitySettings.shadows, mongKieuBong[im]));
            Kiem(Mathf.Abs(QualitySettings.lodBias - mongChiTietXa[im]) < 0.01f, "chi tiet xa sai o muc " + CaiDatDoHoa.Ten[im]);
            Kiem(QualitySettings.shadows.ToString() == mongKieuBong[im], "kieu bong sai o muc " + CaiDatDoHoa.Ten[im]);

            var sanhMoi = Object.FindAnyObjectByType<ManSanh>();
            Ghi("   con dang nhap: " + (FirebaseMang.DaDangNhap ? "co" : "KHONG")
                + "; sanh hien ngay sau khi nap lai: " + (sanhMoi != null && sanhMoi.enabled ? "co" : "khong (Editor)"));
            Kiem(FirebaseMang.DaDangNhap, "tai lai lam mat dang nhap");

            yield return VaoSanh();

            if (m == MucDoHoa.RatYeu)
            {
                // Anh bang sau khi tai lai - chu "(hien gio)" phai chuyen sang Rat yeu;
                // chu va khung phai net y het muc Cao (chi canh 3D thu nho)
                sanhMoi = Object.FindAnyObjectByType<ManSanh>();
                sanhMoi.MoCaiDat();
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Chup("caidat_4_ratyeu_hiengio");
                sanhMoi.BamOKCaiDat();      // khong doi gi -> chi dong
                yield return null;
            }

            // ---- Vao Act2 do bong that ----
            SceneManager.LoadScene("Act2");
            han = Time.realtimeSinceStartup + 30f;
            while (SceneManager.GetActiveScene().name != "Act2" && Time.realtimeSinceStartup < han)
                yield return null;
            yield return new WaitForSeconds(2.0f);

            var pc = Object.FindAnyObjectByType<PlayerController>();
            var mau = pc != null ? pc.GetComponent<Damageable>() : null;
            if (mau != null) mau.health = mau.maxHealth;

            // Hai khung de UnityStats co so lieu cua khung da ve xong
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            int boBong = UnityStats.shadowCasters;
            float tamGiac = UnityStats.triangles / 1000f;

            Ghi(string.Format("   Act2: khoang bong = {0} m (mong {1}), vat do bong = {2}, tam giac = {3:F0}k",
                QualitySettings.shadowDistance, mongBong[im], boBong, tamGiac));
            Kiem(Mathf.Abs(QualitySettings.shadowDistance - mongBong[im]) < 0.01f,
                 "khoang bong o Act2 sai");
            if (m == MucDoHoa.RatYeu) Kiem(boBong == 0, "muc Rat yeu van ve bong");
            else Kiem(boBong > 0, "muc " + CaiDatDoHoa.Ten[im] + " mat bong");

            // ---- Anh dem canh 3D ----
            yield return new WaitForSecondsRealtime(0.3f);   // TheoDoiCamera gan o LateUpdate
            var cam = Camera.main;
            var kx = cam != null ? cam.GetComponent<KetXuatThuNho>() : null;
            float heSo = CaiDatDoHoa.heSoPhanGiai[im];
            if (heSo >= 0.999f)
            {
                Ghi("   canh 3D 100%: KetXuatThuNho " + (kx == null ? "khong gan" : kx.enabled ? "DANG BAT" : "tat"));
                Kiem(kx == null || !kx.enabled, "muc Cao van ve qua anh dem");
            }
            else if (kx == null || !kx.enabled)
            {
                Ghi("[LOI] muc " + CaiDatDoHoa.Ten[im] + ": camera chinh khong co KetXuatThuNho"); loi++;
            }
            else
            {
                int phongTruoc = kx.SoKhungDaPhong;
                yield return new WaitForSecondsRealtime(0.5f);
                yield return new WaitForEndOfFrame();
                int mongRong = Mathf.Max(16, Mathf.RoundToInt(Screen.width * heSo));
                int mongCao = Mathf.Max(16, Mathf.RoundToInt(Screen.height * heSo));
                int thuTuBloom = -1, thuTuKx = -1, k = 0;
                foreach (var c in cam.GetComponents<MonoBehaviour>())
                {
                    if (c is SimpleBloom) thuTuBloom = k;
                    if (c == kx) thuTuKx = k;
                    k++;
                }
                Ghi(string.Format("   canh 3D {0:0}%: anh dem {1}x{2} (mong {3}x{4}), phong len man hinh {5} khung / 0,5 s; ngoai luc ve: targetTexture {6}, pixelWidth {7} (man hinh {8}); thu tu bloom {9} < ket xuat {10}",
                    heSo * 100f, kx.RongDem, kx.CaoDem, mongRong, mongCao, kx.SoKhungDaPhong - phongTruoc,
                    cam.targetTexture == null ? "trong" : cam.targetTexture.name, cam.pixelWidth, Screen.width,
                    thuTuBloom, thuTuKx));
                Kiem(kx.RongDem == mongRong && kx.CaoDem == mongCao, "anh dem sai kich thuoc");
                Kiem(kx.SoKhungDaPhong - phongTruoc >= 3, "anh dem khong duoc phong len man hinh");
                Kiem(cam.targetTexture == null, "ngoai luc ve camera van gan anh dem - toa do OnGUI/chuot se lech");
                Kiem(cam.pixelWidth == Screen.width, "pixelWidth camera khac man hinh");
                Kiem(thuTuBloom < 0 || thuTuBloom < thuTuKx, "KetXuatThuNho dung truoc SimpleBloom");
            }

            string tenAnh = "caidat_act2_" + (m == MucDoHoa.Cao ? "cao" : m == MucDoHoa.TrungBinh ? "trungbinh"
                                            : m == MucDoHoa.Yeu ? "yeu" : "ratyeu");
            yield return Chup(tenAnh);
            // Canh phai hien that (khong den, khong trong): do sang trung binh anh chup
            var anh = new Texture2D(2, 2);
            anh.LoadImage(File.ReadAllBytes("PlayTestShots/" + tenAnh + ".png"));
            float tong = 0f; int n = 0;
            for (int y = 0; y < anh.height; y += 7)
                for (int x = 0; x < anh.width; x += 7) { var c = anh.GetPixel(x, y); tong += 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; n++; }
            Object.DestroyImmediate(anh);
            sangMuc[im] = n > 0 ? tong / n : 0f;
            Ghi(string.Format("   anh chup {0}: do sang trung binh {1:F3}", tenAnh, sangMuc[im]));

            SceneManager.LoadScene("MainMenu");
            han = Time.realtimeSinceStartup + 30f;
            while (SceneManager.GetActiveScene().name != "MainMenu" && Time.realtimeSinceStartup < han)
                yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return VaoSanh();
        }

        float sangCao = sangMuc[(int)MucDoHoa.Cao];
        for (int i = 1; i < CaiDatDoHoa.SoMuc; i++)
            Kiem(sangCao <= 0f || sangMuc[i] > 0.5f * sangCao,
                 "anh chup muc " + CaiDatDoHoa.Ten[i] + " toi han so voi muc Cao - canh khong duoc phong len man hinh?");

        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/caidat.txt", bao.ToString());

        // Dang xuat tai khoan chay thu - khong de no nam lai trong bo nho
        FirebaseMang.Quen();

        var rac = GameObject.Find("TAM_ThuCaiDat");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;

        if (coMucGoc) PlayerPrefs.SetInt(CaiDatDoHoa.Khoa, mucGoc);
        else PlayerPrefs.DeleteKey(CaiDatDoHoa.Khoa);
        if (coMucCu) PlayerPrefs.SetInt(CaiDatDoHoa.KhoaCu, mucCu);
        else PlayerPrefs.DeleteKey(CaiDatDoHoa.KhoaCu);
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc);
        else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

        QualitySettings.SetQualityLevel(chatLuongGoc, true);

        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;

        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);

        Debug.Log("[CaiDat] da tra lai: muc do hoa, phien dang nhap, muc chat luong Editor = "
                  + QualitySettings.names[QualitySettings.GetQualityLevel()] + ", scene = "
                  + EditorSceneManager.GetActiveScene().path
                  + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
