using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU THANH KY NANG tren ban PC: duoi moi o chi con PHIM TAT, khong con
/// ten ky nang.
///
/// Do bang HAI duong doc lap nhau, vi mot duong khong du:
///
///   1. DO TREN ANH THAT - chup ca HUD bang <c>ScreenCapture</c> roi dem pixel
///      mau chu trong dai ngay duoi hang o ky nang. Duong nay khong hoi code
///      cau gi ca: no chi nhin cai da ve ra man hinh.
///   2. DO BE RONG CHU LY THUYET - hoi chinh <c>keyStyle</c> cua GameHUD xem
///      "[1/Z]" rong bao nhieu pixel va "[1/Z] QUA CAU LUA" rong bao nhieu.
///      Duong nay cho cai MOC de doi chieu voi be rong do duoc tren anh.
///
/// Neu chi dem pixel thi khong biet bao nhieu la it; neu chi tinh be rong thi
/// khong biet cai gi that su duoc ve. Ghep hai lai moi ket luan duoc.
///
/// So do CHI CO NGHIA khi dat canh mot ban doi chung do cung cach - nen ban
/// truoc khi sua duoc giu lai o <c>thanh_ky_nang_pc_CU.txt</c>. Nen canh cung
/// co cho sang vang lot vao bo loc mau, nhung no lot deu o ca hai ban.
///
/// Anh chup bang <c>ScreenCapture</c> chu khong bang <c>cam.Render()</c>: HUD ve
/// bang OnGUI, ma OnGUI KHONG di vao RenderTexture cua mot lan Render thu cong.
///
/// Ket qua ghi ra <c>PlayTestShots/thanh_ky_nang_pc.txt</c>.
/// </summary>
public static class ThuThanhKyNang
{
    const int SoO = 7;

    // KHONG chep hang so nay tu GameHUD sang. Lan do dau chep "720" trong khi
    // GameHUD dung 1080, the la moi o ky nang bi tinh rong 66 px thay vi 44 px
    // va ca bay vung quet deu truot sang trai - so do ra vo nghia ma van trong
    // nhu that. Doc thang tu GameHUD thi no sai cung sai theo, khong lech duoc.
    static float Ref = -1f;

    static float t0 = -1f;
    static int buoc, loi;
    static string thuMuc, canhCu;
    static readonly StringBuilder bao = new StringBuilder();

    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    static GameHUD hud;
    static string duongAnhPC, duongAnhCamUng;

    [MenuItem("Diablo 2.5D/22. Chay thu THANH KY NANG (PC)", false, 106)]
    public static void Chay()
    {
        thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots");
        Directory.CreateDirectory(thuMuc);
        duongAnhPC = Path.Combine(thuMuc, "thanh_ky_nang_pc.png");
        duongAnhCamUng = Path.Combine(thuMuc, "thanh_ky_nang_cam_ung.png");

        // Nho canh nguoi dung dang mo de tra lai luc xong
        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        bao.Length = 0;
        loi = 0; buoc = 0; t0 = -1f; hud = null;

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuThanhKyNang] " + s);
    }

    static void Nhip()
    {
        // Editor co the tu thoat Play giua chung; do tiep thi ra so vo nghia
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.time;
        float t = Time.time - t0;

        switch (buoc)
        {
            // ---- 1. Bat dam bao dang o CHE DO PC ----
            case 0:
                if (t > 1.0f)
                {
                    hud = Object.FindAnyObjectByType<GameHUD>();
                    if (hud == null) { Ghi("[LOI] khong tim thay GameHUD"); loi++; buoc = 9; break; }

                    // Dat qua o cua HUD: GameHUD.Update ghi de CamUng.EpBat moi
                    // khung, dat thang vao co tinh thi khung sau la mat.
                    hud.epCamUng = false;

                    var fRef = typeof(GameHUD).GetField("Ref",
                                   BindingFlags.Static | BindingFlags.NonPublic);
                    if (fRef == null) { Ghi("[LOI] khong doc duoc hang so Ref trong GameHUD"); loi++; buoc = 9; break; }
                    Ref = (float)fRef.GetValue(null);

                    Ghi(string.Format("man = {0}x{1}, Ref cua GameHUD = {2:F0}, cam ung dang dung = {3}",
                                      Screen.width, Screen.height, Ref, CamUng.DangDung));
                    if (CamUng.DangDung)
                    { Ghi("[LOI] dang o che do cam ung, khong do duoc thanh ky nang PC"); loi++; }
                    buoc++;
                }
                break;

            case 1:
                if (t > 2.0f) { Chup(duongAnhPC); buoc++; }
                break;

            // ---- 2. Doc lai anh vua chup roi dem pixel ----
            case 2:
                if (t > 3.4f) { DoTrenAnh(); buoc++; }
                break;

            // ---- 3. Ban cam ung phai khong bi anh huong ----
            case 3:
                if (t > 4.0f) { hud.epCamUng = true; buoc++; }
                break;

            case 4:
                if (t > 5.0f)
                {
                    Ghi("cam ung dang dung = " + CamUng.DangDung);
                    if (!CamUng.DangDung) { Ghi("[LOI] ep cam ung ma van o che do PC"); loi++; }
                    Chup(duongAnhCamUng);
                    buoc++;
                }
                break;

            case 5:
                if (t > 6.4f) { GhiNhanBanCamUng(); buoc++; }
                break;

            case 6:
                if (t > 6.8f) { Ket(); buoc++; }
                break;
        }
    }

    /// <summary>
    /// Duong do 1: dem pixel mau chu trong dai ngay duoi hang o ky nang.
    /// Khong hoi code xem no dinh ve gi - chi nhin cai da ve ra.
    /// </summary>
    static void DoTrenAnh()
    {
        var anh = DocAnh(duongAnhPC);
        if (anh == null) { Ghi("[LOI] khong doc duoc " + duongAnhPC); loi++; return; }

        // Anh chup co the to hon Screen (Retina / scale), nen quy ve ti le
        float ti = anh.width / (float)Screen.width;
        float s = Screen.height / Ref;
        float o = 84f * s, khe = 12f * s;
        float tong = o * SoO + khe * (SoO - 1);
        float x0 = (Screen.width - tong) * 0.5f;
        float yBar = Screen.height - o - 22f * s;

        // Dai chu: chu ve o (yBar + o + 2s), cao 22s. Bat dau quet tu 3s cho
        // CHAC CHAN nam duoi duong vien o - lan do dau lay tu day o, the la
        // dem ca vien vang cua o dang chon va coi do la chu.
        // Toa do GUI tinh tu TREN xuong, Texture2D tinh tu DUOI len -> phai lat.
        int guiTren = Mathf.RoundToInt((yBar + o + 3f * s) * ti);
        int guiDuoi = Mathf.RoundToInt((yBar + o + 26f * s) * ti);
        int texDuoi = anh.height - guiDuoi;
        int texTren = anh.height - guiTren;
        texDuoi = Mathf.Clamp(texDuoi, 0, anh.height - 1);
        texTren = Mathf.Clamp(texTren, 0, anh.height - 1);

        Ghi(string.Format("ti le anh/man = {0:F2}, dai chu = {1} pixel cao", ti, texTren - texDuoi));

        var px = anh.GetPixels32();
        int tongPixelChu = 0;
        var rongCum = new int[SoO];
        var demCum = new int[SoO];

        for (int i = 0; i < SoO; i++)
        {
            int ox0 = Mathf.RoundToInt((x0 + (o + khe) * i) * ti);
            int ox1 = Mathf.RoundToInt((x0 + (o + khe) * i + o) * ti);
            ox0 = Mathf.Clamp(ox0, 0, anh.width - 1);
            ox1 = Mathf.Clamp(ox1, 0, anh.width - 1);

            int trai = int.MaxValue, phai = int.MinValue, dem = 0;
            for (int y = texDuoi; y < texTren; y++)
                for (int x = ox0; x < ox1; x++)
                {
                    if (!LaMauChu(px[y * anh.width + x])) continue;
                    dem++;
                    if (x < trai) trai = x;
                    if (x > phai) phai = x;
                }

            demCum[i] = dem;
            rongCum[i] = dem > 0 ? (phai - trai + 1) : 0;
            tongPixelChu += dem;
        }

        // Duong do 2: be rong LY THUYET cua hai chuoi, hoi chinh style cua HUD
        float rongMoi = -1f, rongCu = -1f;
        var f = typeof(GameHUD).GetField("keyStyle", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f != null && f.GetValue(hud) is GUIStyle st)
        {
            rongMoi = st.CalcSize(new GUIContent("[1/Z]")).x * ti;
            rongCu = st.CalcSize(new GUIContent("[1/Z] QUA CAU LUA")).x * ti;
        }
        else { Ghi("[LOI] khong doc duoc keyStyle trong GameHUD"); loi++; }

        Ghi(string.Format("be rong LY THUYET: \"[1/Z]\" = {0:F0} px, \"[1/Z] QUA CAU LUA\" = {1:F0} px",
                          rongMoi, rongCu));
        Ghi("tong pixel mau chu duoi bay o = " + tongPixelChu);

        for (int i = 0; i < SoO; i++)
            Ghi(string.Format("  o {0}: {1} pixel chu, cum rong {2} px",
                              i + 1, demCum[i], rongCum[i]));

        // KHONG phan xet o day. Nen canh cung co cho sang vang, ma bao nhieu
        // pixel la "it" thi khong the doan ra duoc - phai co ban doi chung.
        // Viec ket luan danh cho ThuThanhKyNang_So, so hai anh cu/moi.

        Object.DestroyImmediate(anh);
    }

    /// <summary>
    /// Ban cam ung: KHONG dem pixel o day nua.
    ///
    /// Lan dau ham nay dem pixel mau chu trong vung bay o cua thanh PC, ra 87
    /// roi lan sau ra 4176 tren cung mot thu - vi giua hai lan chay co mot qua
    /// thien thach no sang ruc dung giua man hinh. No dang do dam lua chu khong
    /// do chu. Cai can biet o day thi da co san mot cach chac chan hon: neu
    /// CamUng.DangDung = true thi GameHUD di nhanh VeNutKyNangTron, khong goi
    /// DrawSkillBar - va anh chup con lai de nguoi doc tu nhin.
    /// </summary>
    static void GhiNhanBanCamUng()
    {
        Ghi("ban cam ung: khong ve thanh ky nang PC - xem thanh_ky_nang_cam_ung.png");
    }

    /// <summary>
    /// Mau chu cua keyStyle la (1; 0,9; 0,6) - vang nhat. Bat rong tay mot chut
    /// vi vien chu bi lam mem, nhung van doi do PHAI troi hon lam va lam phai
    /// troi hon xanh - de khong dem nham lua trong canh.
    /// </summary>
    static bool LaMauChu(Color32 c)
    {
        return c.r > 170 && c.g > 140 && c.b < 190 && c.r >= c.g && c.g > c.b + 25;
    }

    static Texture2D DocAnh(string duong)
    {
        if (!File.Exists(duong)) return null;
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        return tex.LoadImage(File.ReadAllBytes(duong)) ? tex : null;
    }

    /// <summary>
    /// ScreenCapture ghi file o KHUNG SAU, nen goi xong phai doi vai khung roi
    /// moi doc - do la ly do moi buoc do cach buoc chup hon mot giay.
    /// </summary>
    static void Chup(string duong)
    {
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
    }

    static void Ket()
    {
        if (hud != null) hud.epCamUng = false;
        CamUng.EpBat = false;

        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText(Path.Combine(thuMuc, "thanh_ky_nang_pc.txt"), bao.ToString());

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        // Tra lai canh nguoi dung dang mo truoc khi bam menu
        if (!string.IsNullOrEmpty(canhCu))
            EditorApplication.delayCall += () => EditorSceneManager.OpenScene(canhCu);
    }
}
