using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU NUT KHOA GOC NHIN (con mat) tren giao dien cam ung.
///
/// Do hai thu, va phai do ca hai:
///
///   1. Cai NUT co an cham khong - goi thang <c>BamNutKhoaCam</c> tai dung tam
///      nut. Nut ve dep ma khong an cham thi coi nhu khong co.
///   2. Khoa co GIU duoc goc nhin khong - thu day camera bang moi duong ma
///      nguoi choi co the day (xoay, phong to, doi che do nhin) roi do lai goc
///      quay that cua camera.
///
/// Anh chup bang <c>ScreenCapture</c> chu khong bang <c>cam.Render()</c>: HUD ve
/// bang OnGUI, ma OnGUI KHONG di vao RenderTexture cua mot lan Render thu cong -
/// chup kieu do thi ra mot canh game khong co lay mot nut nao.
/// </summary>
public static class ThuNutKhoaCam
{
    static float t0 = -1f;
    static int buoc, loi;
    static string thuMuc;
    static readonly StringBuilder bao = new StringBuilder();

    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    static CameraRig rig;
    static GameHUD hud;
    static Vector3 gocTruoc;
    static float fovTruoc, xaTruoc;
    static int cheDoTruoc;

    [MenuItem("Diablo 2.5D/21. Chay thu NUT KHOA GOC NHIN - Act2", false, 105)]
    public static void Chay()
    {
        thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots");
        Directory.CreateDirectory(thuMuc);

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        bao.Length = 0;
        loi = 0; buoc = 0; t0 = -1f;
        rig = null; hud = null;

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuNutKhoaCam] " + s);
    }

    /// <summary>Bam vao dung tam nut khoa, qua chinh ham ma cu cham that goi.</summary>
    static bool BamNut()
    {
        var kieu = typeof(GameHUD);
        var mTam = kieu.GetMethod("TamNutKhoaCam", BindingFlags.Instance | BindingFlags.NonPublic);
        var mBam = kieu.GetMethod("BamNutKhoaCam", BindingFlags.Instance | BindingFlags.NonPublic);
        if (mTam == null || mBam == null)
        {
            Ghi("[LOI] khong tim thay TamNutKhoaCam / BamNutKhoaCam trong GameHUD");
            loi++;
            return false;
        }

        float s = Screen.height / 720f;
        var tam = (Vector2)mTam.Invoke(hud, new object[] { s });
        return (bool)mBam.Invoke(hud, new object[] { tam, s });
    }

    /// <summary>Day camera bang MOI duong nguoi choi co the day.</summary>
    static void DayCamera()
    {
        if (rig != null)
        {
            rig.Orbit(40f, 12f);                  // cu chi keo ngon o vung trong
            rig.CycleView();                      // nut doi goc nhin
        }
        CamUng.XoayCam = new Vector2(60f, 20f);   // cu chi tho do HUD ghi
        CamUng.ChumZoom = 80f;                    // chum hai ngon de phong to
    }

    static void DoLai(string nhan, bool phaiGiuNguyen)
    {
        var cam = Camera.main;
        Vector3 goc = cam.transform.eulerAngles;
        float fov = cam.fieldOfView;
        float xa = rig != null && rig.target != null
                 ? Vector3.Distance(cam.transform.position, rig.target.position) : -1f;

        float lechGoc = Quaternion.Angle(Quaternion.Euler(gocTruoc), Quaternion.Euler(goc));
        float lechXa = Mathf.Abs(xa - xaTruoc);
        float lechFov = Mathf.Abs(fov - fovTruoc);
        bool doiCheDo = rig != null && rig.CurrentIndex != cheDoTruoc;

        Ghi(string.Format("{0}: lech goc = {1:F2} do, lech khoang cach = {2:F2} m, lech fov = {3:F2}, doi che do = {4}",
                          nhan, lechGoc, lechXa, lechFov, doiCheDo));

        if (phaiGiuNguyen)
        {
            // Nguong 0,05 do chu khong phai 0: camera van BAM THEO nhan vat, ma
            // nhan vat co the nhuc nhich mot chut ngay ca khi dung yen.
            if (lechGoc > 0.05f) { Ghi("[LOI] dang khoa ma goc quay van doi"); loi++; }
            if (lechXa > 0.05f) { Ghi("[LOI] dang khoa ma khoang cach van doi"); loi++; }
            if (lechFov > 0.05f) { Ghi("[LOI] dang khoa ma fov van doi"); loi++; }
            if (doiCheDo) { Ghi("[LOI] dang khoa ma van doi duoc che do nhin"); loi++; }
        }
        else
        {
            if (lechGoc < 1f) { Ghi("[LOI] da mo khoa ma camera van khong nhuc nhich"); loi++; }
        }
    }

    static void GhiMoc()
    {
        var cam = Camera.main;
        gocTruoc = cam.transform.eulerAngles;
        fovTruoc = cam.fieldOfView;
        xaTruoc = rig != null && rig.target != null
                ? Vector3.Distance(cam.transform.position, rig.target.position) : -1f;
        cheDoTruoc = rig != null ? rig.CurrentIndex : -1;
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.time;
        float t = Time.time - t0;

        switch (buoc)
        {
            // ---- 1. Bat giao dien cam ung ngay tren may ban ----
            case 0:
                if (t > 1.0f)
                {
                    rig = Object.FindAnyObjectByType<CameraRig>();
                    hud = Object.FindAnyObjectByType<GameHUD>();
                    if (rig == null || hud == null)
                    { Ghi("[LOI] khong tim thay CameraRig / GameHUD"); loi++; buoc = 9; break; }

                    // Dat qua O CUA HUD chu khong dat thang CamUng.EpBat:
                    // GameHUD.Update ghi de "CamUng.EpBat = epCamUng" moi khung,
                    // nen dat thang vao co tinh thi khung sau la mat. Lan do dau
                    // vap dung cai do - anh chup ra thanh ky nang cua PC.
                    hud.epCamUng = true;
                    CamUng.KhoaCam = false;
                    Ghi(string.Format("cam ung dang dung = {0}, khoa = {1}, che do nhin = {2}",
                                      CamUng.DangDung, CamUng.KhoaCam, rig.CurrentIndex));
                    buoc++;
                }
                break;

            case 1:
                if (t > 2.0f) { Chup("nut_khoa_01_chua_khoa"); buoc++; }
                break;

            // ---- 2. Bam nut ----
            case 2:
                if (t > 3.2f)
                {
                    bool an = BamNut();
                    Ghi(string.Format("bam vao tam nut -> nut nhan cham = {0}, khoa = {1}",
                                      an, CamUng.KhoaCam));
                    if (!an) { Ghi("[LOI] nut khong nhan cham"); loi++; }
                    if (!CamUng.KhoaCam) { Ghi("[LOI] bam roi ma van chua khoa"); loi++; }
                    GhiMoc();
                    buoc++;
                }
                break;

            // ---- 3. Day camera bang moi duong, trong mot giay ----
            case 3:
                DayCamera();
                if (t > 4.6f) { DoLai("DANG KHOA", true); Chup("nut_khoa_02_dang_khoa"); buoc++; }
                break;

            // ---- 4. Mo khoa roi day lai ----
            case 4:
                if (t > 5.6f)
                {
                    bool an = BamNut();
                    Ghi(string.Format("bam lan hai -> nut nhan cham = {0}, khoa = {1}", an, CamUng.KhoaCam));
                    if (CamUng.KhoaCam) { Ghi("[LOI] bam lan hai ma van con khoa"); loi++; }
                    GhiMoc();
                    buoc++;
                }
                break;

            case 5:
                DayCamera();
                if (t > 7.2f) { DoLai("DA MO KHOA", false); Chup("nut_khoa_03_da_mo"); buoc++; }
                break;

            case 6:
                if (t > 8.2f) { Ket(); buoc++; }
                break;
        }
    }

    static void Ket()
    {
        if (hud != null) hud.epCamUng = false;
        CamUng.EpBat = false;
        CamUng.KhoaCam = false;

        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText(Path.Combine(thuMuc, "nut_khoa_cam.txt"), bao.ToString());

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }

    /// <summary>
    /// Chup ca HUD. ScreenCapture ghi file o KHUNG SAU, nen dung no la phai
    /// chap nhan anh ra cham mot khung - doi lai la co giao dien trong anh.
    /// </summary>
    static void Chup(string ten)
    {
        string duong = Path.Combine(thuMuc, ten + ".png");
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
    }
}
