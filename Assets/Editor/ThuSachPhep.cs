using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: NUT CON MAT DON LEN GOC, VA CUA SO "SACH PHEP".
///
/// Nguoi dung xin (12/09/2026):
///   - Ban cam ung bo han nut doi goc nhin (hinh may quay) o goc phai tren; nut
///     CON MAT don len dung cho do.
///   - Them nut hinh cuon sach ten "Sach phep": mo ra mot bang keo tha, cot
///     trai la kho ky nang, ben phai la loi ke chi tiet va cac O da chon. Sap
///     xep trong bang the nao thi ngoai man hinh game hien ra y het.
///   - Ban may tinh: cac o TRON doi thanh mot hang o VUONG nam ngang.
///
/// Do bang SO, phan lon KHONG can vao Play (ham thuan):
///   A. Bo cuc cua so tren 8 co man hinh x 2 ban: cac khung khong de len nhau,
///      moi o nam tron trong vung o, hai o khong cham nhau.
///   B. O TRON trong bang xep DUNG hinh cum nut ngoai tran dau (so tung cap
///      khoang cach, khong so bang mat).
///   C. Kho ky nang: dat vao o thi doi cho chu khong nhan ban; bo khoi o; luu
///      roi nap lai van y nguyen.
///   D. Trong Play: nut con mat nam dung goc phai tren, cua so mo/dong duoc,
///      dang mo thi input tran dau bi khoa. Chup anh ca hai ban.
///
/// Ket qua ghi ra <c>PlayTestShots/sachphep.txt</c>.
/// </summary>
public static class ThuSachPhep
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    static readonly Vector2[] ManHinh =
    {
        new Vector2(2000f, 920f),     // dien thoai ngang
        new Vector2(1170f, 540f),
        new Vector2(844f, 390f),
        new Vector2(1920f, 1080f),
        new Vector2(1366f, 768f),
        new Vector2(2560f, 1440f),
        new Vector2(1024f, 768f),
        new Vector2(1280f, 800f),
    };

    [MenuItem("Diablo 2.5D/59. Chay thu SACH PHEP (keo tha o ky nang)", false, 150)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu sach phep",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] nut con mat o goc, va cua so Sach phep");

        DoBoCuc();
        DoKho();

        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_SachPhep") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_SachPhep");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[SachPhep] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    static bool DeLenNhau(Rect a, Rect b)
    {
        return a.xMin < b.xMax - 0.5f && b.xMin < a.xMax - 0.5f
            && a.yMin < b.yMax - 0.5f && b.yMin < a.yMax - 0.5f;
    }

    // ================================================================
    //  A + B. BO CUC - ham thuan, khong can Play
    // ================================================================
    static void DoBoCuc()
    {
        Ghi("");
        Ghi("A. bo cuc cua so tren 8 co man hinh x 2 ban giao dien");

        bool epCu = CamUng.EpBat;
        int soDo = 0, loiA = 0;
        float hoNhoNhat = 999999f;
        string choChat = "";
        float hoChuNhoNhat = 999999f;
        string choChuChat = "";

        foreach (bool camUng in new[] { true, false })
        {
            CamUng.EpBat = camUng;
            foreach (var m in ManHinh)
            {
                float s = GiaoDien.TinhTiLe(m.x, m.y);
                var b = CuaSoSachPhep.TinhBoCuc(m.x, m.y, s);
                soDo++;
                string ten = (camUng ? "cam ung " : "may tinh ") + m;

                // Ca cua so phai nam gon trong man hinh
                if (b.khung.xMin < -0.5f || b.khung.yMin < -0.5f
                    || b.khung.xMax > m.x + 0.5f || b.khung.yMax > m.y + 0.5f)
                { Loi(ten + ": cua so tran ra ngoai man hinh"); loiA++; }

                // Ba khung khong duoc de len nhau
                if (DeLenNhau(b.kho, b.chiTiet)) { Loi(ten + ": cot trai de len khung chi tiet"); loiA++; }
                if (DeLenNhau(b.kho, b.vungO)) { Loi(ten + ": cot trai de len vung o"); loiA++; }
                if (DeLenNhau(b.chiTiet, b.vungO)) { Loi(ten + ": khung chi tiet de len vung o"); loiA++; }
                if (DeLenNhau(b.tieuDe, b.kho) || DeLenNhau(b.tieuDe, b.chiTiet))
                { Loi(ten + ": tieu de de len noi dung"); loiA++; }

                // Moi o phai nam TRON trong vung o, va hai o khong cham nhau
                int n = camUng ? SachPhep.SoOTron : SachPhep.SoOVuong;
                float dayChu = CuaSoSachPhep.DongNhacO(b.vungO, s).yMax;
                for (int i = 0; i < n; i++)
                {
                    var ri = CuaSoSachPhep.OTaiVung(b.vungO, i, s);
                    float hoChu = ri.yMin - dayChu;
                    if (hoChu < hoChuNhoNhat) { hoChuNhoNhat = hoChu; choChuChat = ten + " o " + (i + 1); }
                    if (hoChu < 0f)
                    { Loi(ten + ": o " + (i + 1) + " de len dong chu nhac " + (-hoChu).ToString("F1") + " diem"); loiA++; }
                    if (ri.xMin < b.vungO.xMin - 0.5f || ri.xMax > b.vungO.xMax + 0.5f
                        || ri.yMin < b.vungO.yMin - 0.5f || ri.yMax > b.vungO.yMax + 0.5f)
                    { Loi(ten + ": o " + (i + 1) + " tran ra ngoai vung o"); loiA++; }

                    for (int j = i + 1; j < n; j++)
                    {
                        var rj = CuaSoSachPhep.OTaiVung(b.vungO, j, s);
                        float ho;
                        if (camUng)
                            ho = Vector2.Distance(ri.center, rj.center) - (ri.width + rj.width) * 0.5f;
                        else
                            ho = Mathf.Abs(ri.center.x - rj.center.x) - (ri.width + rj.width) * 0.5f;

                        if (ho < hoNhoNhat) { hoNhoNhat = ho; choChat = ten + " o " + (i + 1) + "-" + (j + 1); }
                        if (ho < 0f) { Loi(ten + ": o " + (i + 1) + " va o " + (j + 1) + " de len nhau"); loiA++; }
                    }
                }
            }
        }
        Ghi("do " + soDo + " truong hop: " + loiA + " loi");
        Ghi("hai o gan nhau nhat: " + choChat + " - con ho " + hoNhoNhat.ToString("F1") + " diem");
        Ghi("o gan dong chu nhac nhat: " + choChuChat + " - con ho " + hoChuNhoNhat.ToString("F1") + " diem");

        // ---- O KHOA: co quai that, va nho hon ban cu 20% ----
        Ghi("");
        Ghi("A2. hinh o khoa");
        var anh = IconKhoa.Anh;
        var nd = IconKhoa.NoiDung;
        // Doc thang diem anh: giua long chu U cua quai phai TRONG SUOT, hai chan
        // quai va than phai DAC. Ban may tinh cu chi co mot thanh ngang - phep
        // nay tren anh ay se bao "khong co quai".
        System.Func<float, float, float> alpha = (u, v) =>
            anh.GetPixel(Mathf.Clamp(Mathf.RoundToInt(u * anh.width), 0, anh.width - 1),
                         Mathf.Clamp(Mathf.RoundToInt((1f - v) * anh.height), 0, anh.height - 1)).a;
        float aLongQuai = alpha(0.5f, 0.30f), aChanTrai = alpha(0.31f, 0.40f),
              aChanPhai = alpha(0.69f, 0.40f), aDinhQuai = alpha(0.5f, 0.17f), aThan = alpha(0.35f, 0.72f);
        Ghi("    alpha: long quai " + aLongQuai.ToString("F2") + " (phai ~0), chan trai " + aChanTrai.ToString("F2")
            + ", chan phai " + aChanPhai.ToString("F2") + ", dinh quai " + aDinhQuai.ToString("F2")
            + ", than " + aThan.ToString("F2") + " (phai ~1)");
        Kiem(aLongQuai < 0.1f && aChanTrai > 0.9f && aChanPhai > 0.9f && aDinhQuai > 0.9f && aThan > 0.9f,
             "hinh o khoa khong co quai chu U / than khong dac");

        // Kich thuoc so voi ban cu (ban cu: than rong 0,52r tren nut tron,
        // thanh rong 0,34 canh tren o vuong - doc tu git truoc lan sua nay)
        float r0 = 60.72f, canh0 = 84f;
        float rongTron = IconKhoa.KhungVe(Vector2.zero, r0 * GameHUD.RongKhoaTron).width * nd.width;
        float rongVuong = IconKhoa.KhungVe(Vector2.zero, canh0 * GameHUD.RongKhoaVuong).width * nd.width;
        Ghi("    nut tron ban kinh " + r0 + ": o khoa rong " + rongTron.ToString("F1") + " (cu " + (0.52f * r0).ToString("F1")
            + ") -> x" + (rongTron / (0.52f * r0)).ToString("0.000"));
        Ghi("    o vuong canh " + canh0 + ": o khoa rong " + rongVuong.ToString("F1") + " (cu " + (0.34f * canh0).ToString("F1")
            + ") -> x" + (rongVuong / (0.34f * canh0)).ToString("0.000"));
        Kiem(Mathf.Abs(rongTron / (0.52f * r0) - 0.8f) < 0.01f, "o khoa tren nut tron khong nho di 20%");
        Kiem(Mathf.Abs(rongVuong / (0.34f * canh0) - 0.8f) < 0.01f, "o khoa tren o vuong khong nho di 20%");

        // ---- B. O tron phai xep DUNG hinh cum nut ngoai tran dau ----
        Ghi("");
        Ghi("B. o tron trong bang co dung hinh cum nut ngoai tran dau khong");

        CamUng.EpBat = true;
        var mm = new Vector2(1920f, 1080f);
        float ss = GiaoDien.TinhTiLe(mm.x, mm.y);
        var bb = CuaSoSachPhep.TinhBoCuc(mm.x, mm.y, ss);

        // So TI LE khoang cach tung cap: neu bang chi thu nho ca cum thi moi
        // ti le d(bang) / d(game) deu bang nhau. Sai lech giua cap lon nhat va
        // nho nhat cho biet hinh co bi keo meo khong.
        float tiLeMin = float.MaxValue, tiLeMax = 0f;
        for (int i = 0; i < SachPhep.SoOTron; i++)
            for (int j = i + 1; j < SachPhep.SoOTron; j++)
            {
                float dGame = Vector2.Distance(GameHUD.LechNut(i), GameHUD.LechNut(j));
                float dBang = Vector2.Distance(CuaSoSachPhep.OTaiVung(bb.vungO, i, ss).center,
                                               CuaSoSachPhep.OTaiVung(bb.vungO, j, ss).center);
                if (dGame < 0.01f) continue;
                float k = dBang / dGame;
                tiLeMin = Mathf.Min(tiLeMin, k);
                tiLeMax = Mathf.Max(tiLeMax, k);
            }
        float lech = tiLeMax > 0f ? (tiLeMax - tiLeMin) / tiLeMax : 1f;
        Ghi("21 cap o: ti le thu nho tu " + tiLeMin.ToString("F4") + " den " + tiLeMax.ToString("F4")
            + " -> lech " + (lech * 100f).ToString("F2") + "%");
        Kiem(lech < 0.001f, "hinh cum nut trong bang bi keo meo so voi ngoai tran dau");

        CamUng.EpBat = epCu;
    }

    // ================================================================
    //  C. KHO KY NANG
    // ================================================================
    static void DoKho()
    {
        Ghi("");
        Ghi("C. kho ky nang: dat vao o, bo khoi o, luu roi nap lai");

        bool epCu = CamUng.EpBat;
        CamUng.EpBat = true;

        SachPhep.DatLai();
        var bo = SachPhep.BoDangDung;
        bool macDinhDung = true;
        for (int i = 0; i < SachPhep.SoOTron; i++) if (bo[i] != i) macDinhDung = false;
        Ghi("C1. mac dinh: " + Ke(bo) + " (phai la 0..6)");
        Kiem(macDinhDung, "thu tu mac dinh khong con la 0..6");

        // Dat ky nang 5 vao o 0: ky nang 0 phai DOI CHO sang o 5, khong duoc mat
        SachPhep.DatVaoO(0, 5);
        bo = SachPhep.BoDangDung;
        Ghi("C2. dat ky nang 5 vao o 0 -> " + Ke(bo));
        Kiem(bo[0] == 5 && bo[5] == 0, "dat vao o khong doi cho dung");
        Kiem(KhongTrung(bo), "mot ky nang nam o hai o - bi nhan ban");

        // Bo khoi o roi dat lai
        SachPhep.BoKhoiO(2);
        bo = SachPhep.BoDangDung;
        Ghi("C3. bo o 2 -> " + Ke(bo) + " (o 2 phai la -1)");
        Kiem(bo[2] == SachPhep.Trong, "bo khoi o khong an");

        // Luu - nap lai: phai y nguyen
        SachPhep.Luu();
        SachPhep.NapLai();
        var bo2 = SachPhep.BoDangDung;
        bool giu = true;
        for (int i = 0; i < SachPhep.SoOTron; i++) if (bo2[i] != bo[i]) giu = false;
        Ghi("C4. nap lai -> " + Ke(bo2));
        Kiem(giu, "nap lai ra ket qua khac - nguoi choi sap xong roi mat sach");

        // Hai ban giu rieng: doi sang ban may tinh thi bo o kia khong bi dong
        CamUng.EpBat = false;
        var boPc = SachPhep.BoDangDung;
        bool pcConNguyen = true;
        for (int i = 0; i < SachPhep.SoOVuong; i++) if (boPc[i] != i) pcConNguyen = false;
        Ghi("C5. doi sang ban may tinh -> " + Ke(boPc) + " (phai con 0..6)");
        Kiem(pcConNguyen, "sua ban cam ung lam doi luon ban may tinh");

        CamUng.EpBat = true;
        SachPhep.DatLai();
        CamUng.EpBat = false;
        SachPhep.DatLai();
        CamUng.EpBat = epCu;
    }

    static string Ke(int[] a)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < a.Length; i++) { if (i > 0) sb.Append(' '); sb.Append(a[i]); }
        return sb.ToString();
    }

    static bool KhongTrung(int[] a)
    {
        for (int i = 0; i < a.Length; i++)
            for (int j = i + 1; j < a.Length; j++)
                if (a[i] != SachPhep.Trong && a[i] == a[j]) return false;
        return true;
    }

    // ================================================================
    //  D. TRONG PLAY
    // ================================================================
    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
        long co = File.Exists(duong) ? new FileInfo(duong).Length : 0;
        Ghi("    anh " + ten + ": " + (co > 0 ? co / 1024 + " KB" : "KHONG CHUP DUOC"));
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }
        yield return new WaitForSeconds(1.5f);

        var hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud == null) { Loi("khong tim thay GameHUD"); Ket(); yield break; }

        Ghi("");
        Ghi("D. trong tran dau");

        // ---- D1. BAN CAM UNG: nut con mat o goc phai tren ----
        hud.epCamUng = true;
        CamUng.EpBat = true;
        yield return new WaitForSeconds(0.6f);

        float s = Screen.height / 1080f;
        var gocPhaiTren = new Vector2(Screen.width - 62f * s, Screen.height - 62f * s);
        Ghi("D1. ban cam ung: dung cam ung = " + CamUng.DangDung
            + ", goc phai tren o (" + gocPhaiTren.x.ToString("F0") + ", "
            + gocPhaiTren.y.ToString("F0") + ")");
        Kiem(CamUng.DangDung, "ep cam ung khong an");
        yield return Chup("sachphep_1_hud_camung");

        // ---- D2. MO SACH PHEP ----
        CuaSoSachPhep.Mo();
        yield return new WaitForSeconds(0.5f);
        Ghi("D2. mo sach phep: DangMo = " + CuaSoSachPhep.DangMo);
        Kiem(CuaSoSachPhep.DangMo, "khong mo duoc cua so");
        yield return Chup("sachphep_2_bang_camung");

        // ---- D3. DANG MO THI INPUT TRAN DAU BI KHOA ----
        var pc = Object.FindAnyObjectByType<PlayerController>();
        var doc = pc != null ? pc.GetComponent<DocInput>() : null;
        if (doc == null) Loi("khong tim thay bo doc input");
        else
        {
            var g = doc.Doc(0.016f);
            Ghi("D3. dang mo sach: goi input = huongDi " + g.huongDi.ToString("F2")
                + ", kyNang " + g.kyNang + ", muonDiToi " + g.muonDiToi);
            Kiem(g.huongDi.sqrMagnitude < 0.0001f && g.kyNang < 0 && !g.muonDiToi,
                 "dang mo sach phep ma input tran dau van chay");
        }

        // ---- D4. BAN MAY TINH: o vuong ----
        CuaSoSachPhep.Dong();
        hud.epCamUng = false;
        CamUng.EpBat = false;
        yield return new WaitForSeconds(0.6f);
        CapDo.BatDauTranMoi();                      // moi ky nang deu khoa
        yield return new WaitForSeconds(0.2f);
        yield return Chup("sachphep_5_hud_maytinh_khoa");
        CuaSoSachPhep.Mo();
        yield return new WaitForSeconds(0.5f);
        Ghi("D4. ban may tinh: dung cam ung = " + CamUng.DangDung
            + ", so o = " + SachPhep.SoODangDung);
        Kiem(!CamUng.DangDung, "khong tra ve duoc ban may tinh");
        yield return Chup("sachphep_3_bang_maytinh");

        // ---- D5. SAP XEP LAI THI CUM NUT NGOAI TRAN DOI THEO ----
        CuaSoSachPhep.Dong();
        hud.epCamUng = true;
        CamUng.EpBat = true;
        yield return new WaitForSeconds(0.4f);

        SachPhep.DatLai();
        SachPhep.DatVaoO(0, 4);          // dua Thien thach ra nut trong cung
        yield return new WaitForSeconds(0.4f);
        var boSau = SachPhep.BoDangDung;
        Ghi("D5. keo Thien thach (4) vao o 1 -> " + Ke(boSau));
        Kiem(boSau[0] == 4 && boSau[4] == 0, "sap xep lai khong an len bo o dang dung");
        yield return Chup("sachphep_4_hud_da_doi_cho");

        SachPhep.DatLai();
        CuaSoSachPhep.Dong();

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[SachPhep] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        CamUng.EpBat = false;
        File.WriteAllText("PlayTestShots/sachphep.txt", bao.ToString());
        var rac = GameObject.Find("TAM_SachPhep");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
