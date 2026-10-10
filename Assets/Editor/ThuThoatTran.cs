using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MENU 118 - NUT THOAT TRAN (nguoi dung 10/10/2026). Play Act2 that.
///   A. BO CUC cot goc phai tren (cam ung + may tinh): nut thoat o DUNG goc, con mat / Sach phep don xuong 88 don vi, khong chong
///      nhau, nam trong man, khong de len khung dot quai giua mep tren; chuot tren nut thoat / Sach phep thi DocInput khong cho chay.
///   B. ANH THAT: chup man hinh, dem diem anh mau do-cam cua anh cua ham mo trong vong nut thoat; doi chung: vong cung hang lech trai.
///   C. BANG XAC NHAN mo -> KhoaInputTran, DocInput tra goi RONG du can dieu khien dang day (doi chung: dong bang -> co huong di);
///      Sach phep dang mo thi mo bang se dong sach. Anh: PlayTestShots/thoattran_*.png.
///   D. "O LAI" -> bang dong, input chay lai. E. "THOAT TRAN" -> ve scene MainMenu.
/// </summary>
public static class ThuThoatTran
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/118. Chay thu NUT THOAT TRAN (bang xac nhan)", false, 211)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            Debug.LogWarning("[ThoatTran] scene dang mo co thay doi chua luu - luu hoac bo truoc da");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] nut thoat tran");
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
        if (GameObject.Find("TAM_ThoatTran") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThoatTran");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[ThoatTran] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 90 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator ChupTex(System.Action<Texture2D> nhan)
    {
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    /// <summary>Dem diem anh do-cam (long ham + mui ten mau) trong hinh tron tam c (toa do cham, y tu duoi), ban kinh r.</summary>
    static int DemDoCam(Texture2D tx, Vector2 c, float r, out int tong)
    {
        int dem = 0; tong = 0;
        for (int y = (int)(c.y - r); y <= (int)(c.y + r); y++)
            for (int x = (int)(c.x - r); x <= (int)(c.x + r); x++)
            {
                if (x < 0 || y < 0 || x >= tx.width || y >= tx.height) continue;
                if ((x - c.x) * (x - c.x) + (y - c.y) * (y - c.y) > r * r) continue;
                tong++;
                var p = tx.GetPixel(x, y);
                if (p.r > 0.35f && p.r > p.g * 1.7f && p.r > p.b * 1.7f) dem++;
            }
        return dem;
    }

    /// <summary>Vung tieu de bang (toa do texture, y tu duoi): giua man, cao hon tam mot chut.</summary>
    static Rect VungTieuDe()
    {
        return new Rect(Screen.width * 0.35f, Screen.height * 0.5f, Screen.width * 0.3f, Screen.height * 0.25f);
    }

    static float DoSang(Texture2D tx, Rect r)
    {
        double tong = 0; int n = 0;
        for (int y = (int)r.yMin; y < (int)r.yMax; y += 2)
            for (int x = (int)r.xMin; x < (int)r.xMax; x += 2)
            { var p = tx.GetPixel(x, y); tong += 0.2126 * p.r + 0.7152 * p.g + 0.0722 * p.b; n++; }
        return n > 0 ? (float)(tong / n) : 0f;
    }

    static int DemDoVung(Texture2D tx, Rect r)
    {
        int dem = 0;
        for (int y = (int)r.yMin; y < (int)r.yMax; y++)
            for (int x = (int)r.xMin; x < (int)r.xMax; x++)
            { var p = tx.GetPixel(x, y); if (p.r > 0.5f && p.r > p.g * 2.2f && p.r > p.b * 2.2f) dem++; }
        return dem;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while ((dir == null || GameHUD.Ban == null) && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var hud = GameHUD.Ban;
        if (dir == null || hud == null) { Ghi("[LOI] khong co GameDirector / GameHUD"); loi++; Ket(); yield break; }
        dir.enabled = false;
        var toi = dir.player.GetComponent<Damageable>();
        toi.maxHealth = toi.health = 1e7f;
        var doc = dir.player.GetComponent<DocInput>();
        float s = Screen.height / 1080f;

        // ===== A: bo cuc =====
        hud.epCamUng = true; CamUng.EpBat = true;
        yield return new WaitForSeconds(0.5f);
        Vector2 thoat = hud.TamNutThoat(s), mat = hud.TamNutKhoaCam(s), sach = hud.TamNutSachPhep(s);
        float r = hud.BanKinhNutThoat(s);
        Ghi(string.Format("A1. cam ung {0}x{1} (s {2:F3}): thoat ({3:F0},{4:F0}) · con mat ({5:F0},{6:F0}) · Sach phep ({7:F0},{8:F0}); ban kinh {9:F1}",
            Screen.width, Screen.height, s, thoat.x, thoat.y, mat.x, mat.y, sach.x, sach.y, r));
        // goc phai tren viet tay: cach mep phai + mep tren 62 don vi
        Kiem(Mathf.Abs(thoat.x - (Screen.width - 62f * s)) < 0.5f && Mathf.Abs(thoat.y - (Screen.height - 62f * s)) < 0.5f, "nut thoat khong o goc phai tren");
        float kc1 = Vector2.Distance(thoat, mat), kc2 = Vector2.Distance(mat, sach);
        Ghi(string.Format("    khoang cach tam: thoat-mat {0:F1}, mat-sach {1:F1} (can > 2r = {2:F1}); mat & sach cung cot: {3}",
            kc1, kc2, 2f * r, Mathf.Abs(mat.x - thoat.x) < 0.5f && Mathf.Abs(sach.x - thoat.x) < 0.5f));
        Kiem(kc1 > 2f * r && kc2 > 2f * r && mat.y < thoat.y && sach.y < mat.y, "cac nut cot goc phai tren chong nhau / sai thu tu");
        Kiem(thoat.x + r <= Screen.width && thoat.y + r <= Screen.height, "nut thoat tran ra ngoai man hinh");
        // khung dot quai giua mep tren rong ~ 0,3 man: nut thoat phai o ngoai
        Kiem(thoat.x - r > Screen.width * 0.5f + 300f * s, "nut thoat de len khung dot quai");

        hud.epCamUng = false; CamUng.EpBat = false;
        yield return new WaitForSeconds(0.5f);
        Vector2 thoatPC = hud.TamNutThoat(s), sachPC = hud.TamNutSachPhep(s);
        bool chanThoat = GameHUD.ConTroTrenNutGoc(thoatPC), chanSach = GameHUD.ConTroTrenNutGoc(sachPC),
             chanGiua = GameHUD.ConTroTrenNutGoc(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        Ghi(string.Format("A2. may tinh: thoat ({0:F0},{1:F0}) · Sach phep ({2:F0},{3:F0}); chuot tren thoat chan chay {4}, tren sach {5}, giua man {6}",
            thoatPC.x, thoatPC.y, sachPC.x, sachPC.y, chanThoat, chanSach, chanGiua));
        Kiem(Vector2.Distance(thoatPC, thoat) < 0.5f && Mathf.Abs(sachPC.y - (thoatPC.y - 88f * s)) < 0.5f, "ban may tinh: Sach phep khong o ngay duoi nut thoat");
        Kiem(chanThoat && chanSach && !chanGiua, "ConTroTrenNutGoc sai");

        // ===== B: anh that (ban cam ung) =====
        hud.epCamUng = true; CamUng.EpBat = true;
        yield return new WaitForSeconds(0.5f);
        Texture2D tx = null;
        yield return ChupTex(t => tx = t);
        int tongT, tongD;
        int doT = DemDoCam(tx, thoat, r * 0.95f, out tongT);
        // doi chung: cung hang, lech trai 260 don vi (troi + cay, khong co nut nao)
        int doDoi = DemDoCam(tx, new Vector2(thoat.x - 260f * s, thoat.y), r * 0.95f, out tongD);
        Ghi(string.Format("B. anh: diem do-cam trong vong nut thoat {0}/{1} ({2:P1}); doi chung cung hang lech trai {3}/{4}",
            doT, tongT, tongT > 0 ? (float)doT / tongT : 0f, doDoi, tongD));
        Kiem(tongT > 0 && (float)doT / tongT > 0.04f && doT > 4 * (doDoi + 1), "khong thay anh cua ham mo o goc phai tren");
        // vung nen de do lam toi: dai ben trai giua man (khong co nut / bang)
        var vungNen = new Rect(Screen.width * 0.08f, Screen.height * 0.45f, Screen.width * 0.12f, Screen.height * 0.1f);
        float sangTruoc = DoSang(tx, vungNen);
        int doGiuaTruoc = DemDoVung(tx, VungTieuDe());
        Object.Destroy(tx);
        yield return Chup("thoattran_1_hud_camung");

        // ===== C: bang xac nhan khoa input =====
        // doi chung truoc: bang dong, can dang day -> co huong di
        CamUng.DangKeo = true; CamUng.Huong = new Vector2(1f, 0f);
        var g0 = doc.Doc(0.016f);
        CuaSoSachPhep.Mo();
        yield return null;
        hud.MoHoiThoat();
        bool sachDong = !CuaSoSachPhep.DangMo;
        CamUng.DangKeo = true; CamUng.Huong = new Vector2(1f, 0f);
        var g1 = doc.Doc(0.016f);
        Ghi(string.Format("C. doi chung bang dong: huongDi {0:F2}; mo bang: DangHoiThoat {1}, KhoaInputTran {2}, huongDi {3:F2}, Sach phep da dong {4}",
            g0.huongDi.magnitude, GameHUD.DangHoiThoat, GameHUD.KhoaInputTran, g1.huongDi.magnitude, sachDong));
        Kiem(g0.huongDi.magnitude > 0.5f, "doi chung hong: bang dong ma can khong cho huong di");
        Kiem(GameHUD.DangHoiThoat && GameHUD.KhoaInputTran && g1.huongDi.sqrMagnitude < 1e-6f, "bang mo ma input tran van chay");
        Kiem(sachDong, "mo bang thoat ma Sach phep van mo");
        GiaoDien.DatLaiDem();
        yield return new WaitForSeconds(0.4f);
        Ghi(string.Format("    chu bi cat trong bang: {0} (\"{1}\"), bi thu nho: {2}", GiaoDien.SoLanCat, GiaoDien.ChuBiCatCuoi, GiaoDien.SoLanThuNho));
        Kiem(GiaoDien.SoLanCat == 0, "chu trong bang thoat bi cat");
        Texture2D txB = null;
        yield return ChupTex(t => txB = t);
        float sangSau = DoSang(txB, vungNen);
        int doGiuaSau = DemDoVung(txB, VungTieuDe());
        Object.Destroy(txB);
        Ghi(string.Format("    anh bang: nen lam toi {0:F3} -> {1:F3} (x{2:F2}); diem do o vung tieu de giua man {3} -> {4}",
            sangTruoc, sangSau, sangTruoc > 0 ? sangSau / sangTruoc : 0f, doGiuaTruoc, doGiuaSau));
        Kiem(sangSau < sangTruoc * 0.6f, "bang mo ma nen khong toi - bang khong duoc ve?");
        Kiem(doGiuaSau > doGiuaTruoc + 50, "khong thay chu do THOAT TRAN giua man");
        yield return Chup("thoattran_2_bang_camung");
        hud.epCamUng = false; CamUng.EpBat = false;
        yield return new WaitForSeconds(0.4f);
        yield return Chup("thoattran_3_bang_maytinh");

        // ===== D: o lai =====
        GameHUD.DongHoiThoat();
        yield return null;
        CamUng.DangKeo = false;
        hud.epCamUng = true; CamUng.EpBat = true;
        yield return new WaitForSeconds(0.3f);
        CamUng.DangKeo = true; CamUng.Huong = new Vector2(1f, 0f);
        var g2 = doc.Doc(0.016f);
        Ghi(string.Format("D. o lai: DangHoiThoat {0}, KhoaInputTran {1}, huongDi {2:F2}", GameHUD.DangHoiThoat, GameHUD.KhoaInputTran, g2.huongDi.magnitude));
        Kiem(!GameHUD.DangHoiThoat && !GameHUD.KhoaInputTran && g2.huongDi.magnitude > 0.5f, "o lai ma input khong chay lai");
        CamUng.DangKeo = false; CamUng.Huong = Vector2.zero;
        hud.epCamUng = false; CamUng.EpBat = false;

        // ===== E: thoat =====
        hud.MoHoiThoat();
        yield return null;
        GameHUD.DongYThoat();
        han = Time.time + 15f;
        while (SceneManager.GetActiveScene().name != "MainMenu" && Time.time < han) yield return null;
        yield return new WaitForSeconds(1f);
        Ghi(string.Format("E. thoat tran: scene = {0}, DangHoiThoat {1}", SceneManager.GetActiveScene().name, GameHUD.DangHoiThoat));
        Kiem(SceneManager.GetActiveScene().name == "MainMenu" && !GameHUD.DangHoiThoat, "bam thoat tran ma khong ve MainMenu");

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        CamUng.EpBat = false; CamUng.DangKeo = false; CamUng.Huong = Vector2.zero;
        GameHUD.DongHoiThoat();
        File.WriteAllText("PlayTestShots/thoattran.txt", bao.ToString());
        var rac = GameObject.Find("TAM_ThoatTran");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
