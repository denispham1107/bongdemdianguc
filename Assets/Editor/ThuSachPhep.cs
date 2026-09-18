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
///   F. Cua so to bang khung xanh; hang dang chon sang han, da mo sang hon con
///      khoa; o dang chon noi bat; o giu ky nang chua mo co o khoa (do tren anh chup).
///   D. Trong Play: nut con mat nam dung goc phai tren, cua so mo/dong duoc,
///      dang mo thi input tran dau bi khoa. Chup anh ca hai ban.
///   E. Nut Sach phep (13/09/2026, nguoi dung chup chu "Sach phep" bi cat nua
///      duoi va che hinh cuon sach so sai): tren ANH CHUP man hinh, do chieu cao
///      net chu that roi so voi chieu cao net tinh tu bang glyph cua font Inter
///      (doc lap voi cach HUD dat khung). Mau doi chung ve lai dung kieu chu CU
///      phai bi cat - khong thi phep do khong bat duoc loi. Hinh sach: dem so mau
///      khac nhau o long nut (ban cu 4 hinh chu nhat mau tron chi vai mau).
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

    // ================================================================
    //  E. NUT SACH PHEP: CHU KHONG BI CAT, HINH SACH CO CHI TIET
    // ================================================================
    const string ChuNut = "Sách phép";

    /// <summary>Chieu cao net chu (diem anh) tinh tu bang glyph - khong dung khung cua HUD.</summary>
    static float NetChuLyThuyet(int co)
    {
        var f = GiaoDien.ChuDam;
        f.RequestCharactersInTexture(ChuNut, co);
        int tren = int.MinValue, duoi = int.MaxValue;
        foreach (char c in ChuNut)
        {
            if (c == ' ') continue;
            CharacterInfo ci;
            if (!f.GetCharacterInfo(c, out ci, co)) continue;
            tren = Mathf.Max(tren, ci.maxY);
            duoi = Mathf.Min(duoi, ci.minY);
        }
        return tren - duoi;
    }

    // Chu mau kem (do > lam) tren nen dat Act2 nga xanh xam (lam >= do). Nguong
    // thap vi chu 8 diem anh khu rang cua thanh nhieu diem mo - nguong 0,6 lan
    // truoc bo sot dau va chan chu "p" nen do ra 6/11 du anh chup thay du chu.
    static bool LaMauChu(Color c)
    {
        return c.r > 0.28f && c.r - c.b > 0.035f;
    }

    static bool LaDiemSang(Color c)
    {
        return (c.r + c.g + c.b) / 3f > 0.10f;
    }

    /// <summary>Hang tren cung / duoi cung (toa do GUI) co diem mau chu trong hop.</summary>
    static void QuetChu(Texture2D tex, float xa, float xb, float ya, float yb, out int tren, out int duoi)
    {
        QuetChu(tex, xa, xb, ya, yb, LaMauChu, out tren, out duoi);
    }

    static void QuetChu(Texture2D tex, float xa, float xb, float ya, float yb,
                        System.Func<Color, bool> la, out int tren, out int duoi)
    {
        tren = -1; duoi = -1;
        int H = tex.height;
        for (int gy = Mathf.Max(0, (int)ya); gy < Mathf.Min(H, (int)yb); gy++)
        {
            int ty = H - 1 - gy;
            for (int x = Mathf.Max(0, (int)xa); x < Mathf.Min(tex.width, (int)xb); x++)
                if (la(tex.GetPixel(x, ty))) { if (tren < 0) tren = gy; duoi = gy; break; }
        }
    }

    static IEnumerator DoNutSach(GameHUD hud, string ban)
    {
        yield return new WaitForEndOfFrame();
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        float s = Screen.height / 1080f;
        Vector2 t = hud.TamNutSachPhep(s);
        float r = hud.BanKinhNutSachPhep(s);
        float gx = t.x, gy = Screen.height - t.y;
        int co = Mathf.Max(8, Mathf.RoundToInt(15f * s));
        float lyThuyet = NetChuLyThuyet(co);

        // Quet tu duoi mep hinh sach xuong 3 lan co chu
        int tren, duoi;
        QuetChu(tex, gx - r * 1.8f, gx + r * 1.8f, gy + r * 0.97f, gy + r * 0.92f + co * 3f, out tren, out duoi);
        float thucTe = tren < 0 ? 0f : duoi - tren + 1;
        Ghi("E. nut sach phep - ban " + ban + " (man " + Screen.width + "x" + Screen.height + ", co chu " + co + ")");
        Ghi("E1. chu \"" + ChuNut + "\": net that cao " + thucTe.ToString("F0") + " diem, bang glyph "
            + lyThuyet.ToString("F0") + " diem -> " + (lyThuyet > 0 ? (thucTe / lyThuyet * 100f).ToString("F0") : "?") + "%");
        // mep tren cua quet bo mat phan dau chu (dau sac) nam trong 0,05r - cho sai 20%
        Kiem(tren >= 0, "khong thay chu Sach phep duoi nut (" + ban + ")");
        float chuan;
        if (netChuan.TryGetValue(co, out chuan))
        {
            Ghi("    so voi nhan chuan khong cat cung co chu: " + thucTe + " / " + chuan);
            // tren nen dat, nguong mau chu khat hon nen den - cho hut 1 diem
            Kiem(thucTe >= chuan - 1f, "chu Sach phep bi cat (" + ban + "): " + thucTe + " / " + chuan);
        }
        else Loi("chua co nhan chuan cho co chu " + co + " - phai chay DoiChungChuCu truoc");

        // Hinh sach: so mau khac nhau (luong tu 4 bit/kenh) trong o vuong giua nut
        var mau = new System.Collections.Generic.HashSet<int>();
        int nua = Mathf.RoundToInt(r * 0.35f);
        for (int y = (int)gy - nua; y <= (int)gy + nua; y++)
            for (int x = (int)gx - nua; x <= (int)gx + nua; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                Color c = tex.GetPixel(x, tex.height - 1 - y);
                mau.Add(((int)(c.r * 15.99f) << 8) | ((int)(c.g * 15.99f) << 4) | (int)(c.b * 15.99f));
            }
        Ghi("E2. long nut (" + (nua * 2 + 1) + "x" + (nua * 2 + 1) + " diem): " + mau.Count + " mau khac nhau");
        Kiem(mau.Count >= 40, "hinh sach van la hinh khoi mau tron (" + ban + "): " + mau.Count + " mau");
        Object.Destroy(tex);
    }

    /// <summary>
    /// Mau doi chung tren nen den, bon nhan canh nhau:
    ///   CU    - dung kieu cu (GUI.skin.label, khung cao 22s, co RoundToInt(15s));
    ///   CHUAN CU - cung co chu voi CU nhung khung cao 10 lan, khong the cat;
    ///   MOI   - kieu cua HUD bay gio;
    ///   CHUAN - cung co chu voi MOI, khung khong the cat.
    /// So net chu voi nhan chuan cung co, KHONG so voi bang glyph cua font: bang
    /// glyph lech net render ~2 diem (lam tron + khu rang cua), lan chay dau vi no
    /// ma bao nham kieu moi "bi cat".
    ///
    /// Kieu cu CHI bi cat khi man THAP: le cua GUI.skin.label tinh bang diem anh co
    /// dinh, khong co theo s, nen man cang thap le cang an nhieu phan khung 22s. Do
    /// o man 1080 kieu cu van du net - dien thoai ngang tren web (index.html khong
    /// dat devicePixelRatio) chi cao ~390 diem, dung truong hop nguoi dung chup.
    /// </summary>
    static readonly System.Collections.Generic.Dictionary<int, float> netChuan =
        new System.Collections.Generic.Dictionary<int, float>();

    static IEnumerator DoiChungChuCu()
    {
        yield return DoiChungMotCo(390f / 1080f);          // dien thoai ngang tren web
        yield return DoiChungMotCo(Screen.height / 1080f); // man Game dang mo
        yield return DoiChungMotCo(1f);
        yield return DoiChungMotCo(2f);
    }

    static IEnumerator DoiChungMotCo(float s)
    {
        var tam = GameObject.Find("TAM_SachPhep").GetComponent<ChayThuMang>();
        int co = Mathf.Max(8, Mathf.RoundToInt(15f * s));
        int coCu = Mathf.RoundToInt(15f * s);
        float r = 40f * s;
        float hop = Mathf.Max(r * 3.6f, 70f);
        float buoc = hop + 50f;
        float[] xs = new float[4];
        for (int k = 0; k < 4; k++) xs[k] = 30f + k * buoc;
        float y0 = Screen.height * 0.22f;
        float cao0 = Mathf.Max(co, coCu) * 5f + 40f;
        tam.veGUI = () =>
        {
            GUI.color = Color.black;
            // hop den PHU KIN vung quet (quet toi y0 + 3,5 co) - hop ngan hon thi dem ca canh phia sau
            foreach (float x in xs)
                GUI.DrawTexture(new Rect(x - 20, y0 - 20, hop + 40, cao0), Texture2D.whiteTexture);
            GUI.color = Color.white;
            var cu = new GUIStyle(GUI.skin.label);
            cu.font = GiaoDien.ChuDam; cu.alignment = TextAnchor.UpperCenter; cu.fontSize = coCu;
            cu.normal.textColor = new Color(0.93f, 0.86f, 0.74f, 0.92f);
            GUI.Label(new Rect(xs[0], y0, hop, 22f * s), ChuNut, cu);
            var chuanCu = new GUIStyle(cu);
            chuanCu.clipping = TextClipping.Overflow;
            GUI.Label(new Rect(xs[1], y0, hop, coCu * 10f), ChuNut, chuanCu);
            var moi = new GUIStyle(cu);
            moi.padding = new RectOffset(0, 0, 0, 0); moi.margin = new RectOffset(0, 0, 0, 0);
            moi.clipping = TextClipping.Overflow; moi.fontSize = co;
            GUI.Label(new Rect(xs[2], y0, hop, co * 1.9f), ChuNut, moi);
            GUI.Label(new Rect(xs[3], y0, hop, co * 10f), ChuNut, moi);
        };
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        tam.veGUI = null;
        var cao = new float[4];
        for (int k = 0; k < 4; k++)
        {
            int tren, duoi;
            QuetChu(tex, xs[k] - 8, xs[k] + hop + 8, y0 - 12, y0 - 20 + cao0 - 4, LaDiemSang, out tren, out duoi);
            cao[k] = tren < 0 ? 0 : duoi - tren + 1;
        }
        netChuan[co] = cao[3];
        Ghi("E0. doi chung nen den, s = " + s.ToString("F2") + ": kieu CU (co " + coCu + ") net " + cao[0] + " / chuan " + cao[1]
            + "  |  kieu MOI (co " + co + ") net " + cao[2] + " / chuan " + cao[3]);
        Kiem(cao[1] > 0 && cao[3] > 0, "khong do duoc nhan chuan o s = " + s);
        if (s < 0.6f) Kiem(cao[0] < cao[1], "kieu cu KHONG bi cat o man thap s = " + s.ToString("F2") + " - phep do khong bat duoc loi nay");
        Kiem(cao[2] >= cao[3], "kieu chu moi van bi cat o s = " + s.ToString("F2") + ": " + cao[2] + " / " + cao[3]);
        Object.Destroy(tex);
    }

    // ================================================================
    //  F. CUA SO TO HON, HANG DANG CHON SANG, O DANG CHON, O KHOA TRONG O
    // ================================================================
    //
    // Nguoi dung (13/09/2026, ve khung xanh tren anh chup 1560x572):
    //   - cua so to bang khung xanh (~865 x 442 diem);
    //   - hang ky nang dang chon phai sang han; ky nang da mo / nang cap sang
    //     noi hon ky nang con khoa;
    //   - cham o nao trong vung o thi o ay hien la dang chon; ky nang chua mo
    //     trong o van co o khoa nhu ngoai tran.
    // Do tren ANH CHUP man hinh (do sang diem anh), khong doc bien cua cua so.

    static float DoSang(Texture2D tex, Rect r)
    {
        int x0 = Mathf.Max(0, (int)r.xMin), x1 = Mathf.Min(tex.width, (int)r.xMax);
        int y0 = Mathf.Max(0, (int)r.yMin), y1 = Mathf.Min(tex.height, (int)r.yMax);
        double tong = 0; int dem = 0;
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
            {
                var c = tex.GetPixel(x, tex.height - 1 - y);
                tong += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b; dem++;
            }
        return dem > 0 ? (float)(tong / dem) : 0f;
    }

    /// <summary>Do sang cua VANH ngoai o (tu mep o ra them 2..9 diem) - noi co quang sang / vien.</summary>
    static float DoSangVanh(Texture2D tex, Rect r, float s)
    {
        float a = 2f * s, b = 9f * s;
        var tren = new Rect(r.x - b, r.y - b, r.width + 2f * b, b - a);
        var duoi = new Rect(r.x - b, r.yMax + a, r.width + 2f * b, b - a);
        var trai = new Rect(r.x - b, r.y, b - a, r.height);
        var phai = new Rect(r.xMax + a, r.y, b - a, r.height);
        return (DoSang(tex, tren) + DoSang(tex, duoi) + DoSang(tex, trai) + DoSang(tex, phai)) * 0.25f;
    }

    static float DoBaoHoa(Texture2D tex, Rect r)
    {
        int x0 = Mathf.Max(0, (int)r.xMin), x1 = Mathf.Min(tex.width, (int)r.xMax);
        int y0 = Mathf.Max(0, (int)r.yMin), y1 = Mathf.Min(tex.height, (int)r.yMax);
        double tong = 0; int dem = 0;
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
            {
                var c = tex.GetPixel(x, tex.height - 1 - y);
                float mx = Mathf.Max(c.r, Mathf.Max(c.g, c.b)), mn = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                tong += mx > 0.001f ? (mx - mn) / mx : 0f; dem++;
            }
        return dem > 0 ? (float)(tong / dem) : 0f;
    }

    static IEnumerator ChupTex(System.Action<Texture2D> nhan)
    {
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    /// <summary>Vung cua hang mot ky nang - HOI CHINH cua so ve, vi cot nay xep theo nhom
    /// (18/09/2026) nen "chi so x chieu cao hang" khong con dung.</summary>
    static Rect HangKho(CuaSoSachPhep.BoCuc b, int i, float s)
    {
        return CuaSoSachPhep.VungHangKyNang(b, i, s);
    }

    static IEnumerator DoSangCuaSo(string ban)
    {
        float s = Screen.height / 1080f;
        Ghi("");
        Ghi("F. sach phep - ban " + ban + " (man " + Screen.width + "x" + Screen.height + ")");

        // ---- F0. kich thuoc: tren DUNG man anh chup cua nguoi dung ----
        if (ban == "cam ung")
        {
            var b0 = CuaSoSachPhep.TinhBoCuc(1560f, 572f, 572f / 1080f);
            Ghi("F0. man 1560x572: cua so " + b0.khung.width.ToString("F0") + " x " + b0.khung.height.ToString("F0")
                + " (khung xanh nguoi dung ve ~865 x 442; ban cu 625 x 381)");
            Kiem(Mathf.Abs(b0.khung.width - 865f) < 865f * 0.03f && Mathf.Abs(b0.khung.height - 442f) < 442f * 0.03f,
                 "cua so khong to bang khung xanh");
        }

        // ---- Chuan bi: cap cao, mo khoa 1 va 3, nang 3 len cap 3. Ky nang 5 con khoa ----
        CapDo.BatDauTranMoi();
        CapDo.Them(100 + 135 + 180 + 245);          // cap 5 = 5 diem
        CapDo.MoKhoa(1); CapDo.MoKhoa(3); CapDo.NangCap(3); CapDo.NangCap(3);
        SachPhep.DatLai();
        CuaSoSachPhep.Mo();
        var b = CuaSoSachPhep.TinhBoCuc(Screen.width, Screen.height, s);

        // ---- F1. hang dang chon sang nhat; da mo sang hon con khoa ----
        foreach (int chon in new[] { 4, 1 })
        {
            CuaSoSachPhep.ChonKyNang(chon);
            Texture2D tex = null;
            yield return ChupTex(t => tex = t);
            var sang = new float[SachPhep.SoKyNang];
            var thay = new bool[SachPhep.SoKyNang];     // hang nao dang bi cuon khuat thi khong do
            var sb = new StringBuilder();
            for (int i = 0; i < SachPhep.SoKyNang; i++)
            {
                var vung = HangKho(b, i, s);
                thay[i] = vung.width > 1f && vung.height > 1f;
                if (!thay[i]) continue;
                sang[i] = DoSang(tex, vung);
                sb.Append(i).Append(CapDo.DaMo(i) ? "(mo)" : "(khoa)").Append(i == chon ? "*" : "")
                  .Append("=").Append(sang[i].ToString("F3")).Append(" ");
            }
            float moMin = 9f, khoaMax = 0f, khacMax = 0f;
            for (int i = 0; i < SachPhep.SoKyNang; i++)
            {
                if (i == chon || !thay[i]) continue;
                khacMax = Mathf.Max(khacMax, sang[i]);
                if (CapDo.DaMo(i)) moMin = Mathf.Min(moMin, sang[i]); else khoaMax = Mathf.Max(khoaMax, sang[i]);
            }
            if (!thay[chon]) { Ghi("F1. ky nang " + chon + " dang bi cuon khuat - bo qua"); continue; }
            Ghi("F1. chon ky nang " + chon + (CapDo.DaMo(chon) ? " (da mo)" : " (con khoa)") + " - do sang tung hang: " + sb.ToString().Trim());
            Kiem(sang[chon] >= khacMax * 1.4f, "hang dang chon " + chon + " khong sang han cac hang khac: "
                 + sang[chon].ToString("F3") + " / " + khacMax.ToString("F3"));
            Kiem(moMin >= khoaMax * 1.15f, "hang da mo khong sang hon hang con khoa: " + moMin.ToString("F3") + " / " + khoaMax.ToString("F3"));
            if (chon == 1)
            {
                // Hinh o hang con khoa (hang 0): diem sang > 0,5 chi co the la o khoa (hinh da nhan <= 0,40)
                var h0 = HangKho(b, 0, s);
                float kt = h0.height - 10f * s;
                var rIcon = new Rect(h0.x + 10f * s, h0.y + 5f * s, kt, kt);
                int sangKhoa = 0;
                for (int y = (int)rIcon.yMin; y < (int)rIcon.yMax; y++)
                    for (int x = (int)rIcon.xMin; x < (int)rIcon.xMax; x++)
                    {
                        var c = tex.GetPixel(x, tex.height - 1 - y);
                        if (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b > 0.5f) sangKhoa++;
                    }
                Ghi("F1b. hinh hang 0 (con khoa): diem sang cua o khoa " + sangKhoa);
                Kiem(sangKhoa >= 3, "khong thay o khoa o hang ky nang con khoa ben cot trai");
            }
            if (ban == "cam ung") yield return Chup("sachphep_6_chon_" + chon + "_camung");
            else yield return Chup("sachphep_7_chon_" + chon + "_maytinh");
            Object.Destroy(tex);
        }

        // ---- F2. o dang chon: vanh quanh o sang hon moi o khac ----
        // Quang sang o dang chon DAP NHIP (do dac 0,40 - 0,62, chu ky ~1,57 giay). Chup MOT khung
        // thi ket qua tuy pha luc chup: cung ma nguon, ban cam ung lan truoc 1,65 lan, lan sau 1,60
        // va 1,44 (13/09/2026). Nay chup 4 khung trai deu mot chu ky roi lay TRUNG BINH. Nguong
        // 1,4: o tron cam ung nam sat nhau, quang cua o chon loang sang vanh o ben canh.
        {
            int n = SachPhep.SoODangDung;
            int oGiu1 = SachPhep.ONaoGiu(1);
            CuaSoSachPhep.ChonO(oGiu1);
            float vChon = 0f, vKhacMax = 0f;
            var vKhac = new float[n];
            for (int lan = 0; lan < 4; lan++)
            {
                Texture2D tex = null;
                yield return ChupTex(t => tex = t);
                vChon += DoSangVanh(tex, CuaSoSachPhep.OTaiVung(b.vungO, oGiu1, s), s) * 0.25f;
                for (int o = 0; o < n; o++)
                    if (o != oGiu1) vKhac[o] += DoSangVanh(tex, CuaSoSachPhep.OTaiVung(b.vungO, o, s), s) * 0.25f;
                Object.Destroy(tex);
                yield return new WaitForSecondsRealtime(0.39f);
            }
            for (int o = 0; o < n; o++) vKhacMax = Mathf.Max(vKhacMax, vKhac[o]);
            Ghi("F2. cham o " + (oGiu1 + 1) + " (giu ky nang 1): dang xem = " + CuaSoSachPhep.DangXem
                + ", vanh o chon " + vChon.ToString("F3") + " / vanh o khac sang nhat " + vKhacMax.ToString("F3")
                + " (trung binh 4 khung, x" + (vKhacMax > 0 ? (vChon / vKhacMax).ToString("F2") : "?") + ")");
            Kiem(CuaSoSachPhep.DangXem == 1, "cham o ma khong chon ky nang trong o");
            Kiem(vChon >= vKhacMax * 1.4f, "o dang chon khong noi bat hon cac o khac");

            // O TRONG: bo ky nang o o 2 ra roi cham vao -> chinh o trong ay sang, o giu ky nang dang xem thi tat
            SachPhep.BoKhoiO(2);
            CuaSoSachPhep.ChonO(2);
            float vTrong = 0f, vCu = 0f;
            for (int lan = 0; lan < 4; lan++)
            {
                Texture2D tex = null;
                yield return ChupTex(t => tex = t);
                vTrong += DoSangVanh(tex, CuaSoSachPhep.OTaiVung(b.vungO, 2, s), s) * 0.25f;
                vCu += DoSangVanh(tex, CuaSoSachPhep.OTaiVung(b.vungO, oGiu1, s), s) * 0.25f;
                Object.Destroy(tex);
                yield return new WaitForSecondsRealtime(0.39f);
            }
            Ghi("F2b. cham o trong 3: vanh o trong " + vTrong.ToString("F3") + ", vanh o " + (oGiu1 + 1) + " (vua chon luc nay) "
                + vCu.ToString("F3") + " (x" + (vCu > 0 ? (vTrong / vCu).ToString("F2") : "?") + ")");
            Kiem(vTrong >= vCu * 1.4f, "cham o trong ma o trong khong hien la dang chon");
            SachPhep.DatLai();
        }

        // ---- F3. o khoa trong o: CUNG ky nang 5, chup luc khoa roi mo khoa ----
        {
            int o5 = SachPhep.ONaoGiu(5);
            var r5 = CuaSoSachPhep.OTaiVung(b.vungO, o5, s);
            var giua = new Rect(r5.center.x - r5.width * 0.3f, r5.center.y - r5.height * 0.3f, r5.width * 0.6f, r5.height * 0.6f);
            CuaSoSachPhep.ChonKyNang(0);
            Texture2D khoa = null, mo = null;
            yield return ChupTex(t => khoa = t);
            float sKhoa = DoSang(khoa, giua);
            CapDo.MoKhoa(5);
            yield return ChupTex(t => mo = t);
            float sMo = DoSang(mo, giua);
            // "XAM DI" o ngoai tran la NHAN mau (0,38; 0,36; 0,40) - lam TOI chu khong
            // giam do bao hoa (lan do dau do bao hoa: 0,39 vs 0,40, bao nham "khong xam").
            // Do DO SANG.
            // O KHOA: hinh ky nang da bi nhan <= 0,40 nen KHONG diem nao cua no sang qua
            // 0,40; diem sang > 0,5 o giua o chi co the la quai / dinh tan cua o khoa.
            // (Lan do dau dem mau do cua lo khoa - lo qua nho, ban cam ung ra 0 diem.)
            int diemSang = 0;
            for (int y = (int)giua.yMin; y < (int)giua.yMax; y++)
                for (int x = (int)giua.xMin; x < (int)giua.xMax; x++)
                {
                    var c = khoa.GetPixel(x, khoa.height - 1 - y);
                    if (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b > 0.5f) diemSang++;
                }
            Ghi("F3. o " + (o5 + 1) + " giu Khien: luc KHOA do sang " + sKhoa.ToString("F3") + ", diem sang cua o khoa " + diemSang
                + " | sau MO KHOA do sang " + sMo.ToString("F3"));
            Kiem(sKhoa < sMo * 0.6f, "ky nang chua mo trong o khong bi toi di nhu ngoai tran");
            Kiem(diemSang >= 4, "khong thay o khoa tren o ky nang chua mo");
            Object.Destroy(khoa); Object.Destroy(mo);
        }

        CuaSoSachPhep.Dong();
        CapDo.BatDauTranMoi();
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
        yield return DoiChungChuCu();
        yield return DoNutSach(hud, "cam ung");

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
        yield return DoNutSach(hud, "may tinh");
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

        // ---- F. cua so to, hang / o dang chon, o khoa - ca hai ban ----
        yield return DoSangCuaSo("cam ung");
        hud.epCamUng = false;
        CamUng.EpBat = false;
        yield return new WaitForSeconds(0.4f);
        yield return DoSangCuaSo("may tinh");

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
