using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 150 - MAU QUAI QUY CAY (01/11/2026, nguoi dung: "trang xam toan than qua xau", gui anh quy cay long / vo xu xam nau sam, soi sang xam be,
/// sung + gay xam be). Anh mau Meshy goc gan nhu khong sac (bao hoa 0,018, hoi nga xanh) -> Assets/Textures/QuyCay_Mau.png nuong bang Blender MCP
/// (CongCu/Blender/quy_cay_mau.py).
///
/// Vao Play Act2, tat GameDirector, sinh Quy cay THAT tu kho quai (AI tat) truoc mat nguoi choi. Do tren anh rieng con quai (lop 31, nen den,
/// may quay can truoc mat), dem va ngay, DOI CHUNG = cung vat lieu nhung _MainTex = anh goc Meshy, cung khung: do sang TB, do am (r - b),
/// ti le diem anh "xam trang" (do sang > 0,40 va bao hoa < 0,06).
/// PlayTestShots/quycay_mau/*.png + nhatky.txt.
/// </summary>
public static class ThuMauQuyCay
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/quycay_mau/";
    const string AnhMoi = "Assets/Textures/QuyCay_Mau.png";
    const string AnhGoc = "Assets/MeshyImports/Horned Shadowfiend_20260830_185741/meshy_basecolor.png";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/150 Mau quai Quy cay", false, 224)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuMauQuyCay] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        nk.Length = 0; loi = 0;
        daBatDau = false;
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
        daBatDau = true;
        var go = new GameObject("TAM_ThuMauQuyCay");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static IEnumerator ChupGame(string ten)
    {
        yield return new WaitForEndOfFrame();
        var tx = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Thu + ten + ".png", tx.EncodeToPNG());
        Object.Destroy(tx);
    }

    static void DatLop(GameObject g, int lop)
    {
        foreach (var t in g.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = lop;
    }

    struct SoDo { public float xamTrang, sang, am; public int bong; }

    static SoDo DoRieng(GameObject q, Camera cam, Transform hong, string ten)
    {
        int lopCu = q.layer;
        DatLop(q, 31);
        var rt = new RenderTexture(512, 512, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tx = new Texture2D(512, 512, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 512, 512), 0, 0); tx.Apply();
        RenderTexture.active = null; cam.targetTexture = null;
        Object.Destroy(rt);
        File.WriteAllBytes(Thu + ten + ".png", tx.EncodeToPNG());
        var px = tx.GetPixels();
        int n = 0, xt = 0; double sang = 0, am = 0;
        for (int i = 0; i < px.Length; i++)
        {
            var c = px[i];
            if (c.r + c.g + c.b < 0.004f) continue;
            n++;
            float l = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            float bh = Mathf.Max(c.r, Mathf.Max(c.g, c.b)) - Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            if (l > 0.40f && bh < 0.06f) xt++;
            sang += l; am += c.r - c.b;
        }
        Object.Destroy(tx);
        DatLop(q, lopCu);
        return new SoDo { xamTrang = n > 0 ? (float)xt / n : 0f, sang = n > 0 ? (float)(sang / n) : 0f, am = n > 0 ? (float)(am / n) : 0f, bong = n };
    }

    static string In(SoDo s)
    {
        return "xam trang " + s.xamTrang.ToString("P1") + " · sang TB " + s.sang.ToString("F3")
               + " · am (r-b) " + s.am.ToString("F3") + " · " + s.bong + " diem anh";
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        TamNhin.BatTrongPhepThu = true;
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var rig = Camera.main.GetComponent<CameraRig>();
        if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
        var hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.epCamUng = true;
        yield return new WaitForSeconds(1.5f);

        // ---- A. vat lieu prefab ----
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Enemy_QuyCay.mat");
        var anh = mat.GetTexture("_MainTex");
        Kiem(anh != null && AssetDatabase.GetAssetPath(anh) == AnhMoi, "A vat lieu: _MainTex = " + (anh != null ? AssetDatabase.GetAssetPath(anh) : "null"));
        Kiem(mat.GetTexture("_BumpMap") != null && mat.GetTexture("_MetallicGlossMap") != null, "A vat lieu: giu anh phap tuyen + do bong Meshy");
        var cu = new Material(mat); cu.name = "TAM_QuyCayCu"; cu.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(AnhGoc));

        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        Vector3 p = toi.transform.position + F * 5f;
        p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p));
        var q = EnemyFactory.Spawn(MonsterType.QuyCay, p, null, toi.transform);
        q.name = "TAM_QuyCay";
        var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
        q.transform.rotation = Quaternion.LookRotation(-F);
        var rs = q.GetComponentsInChildren<Renderer>();
        bool dungMat = false;
        foreach (var r in rs) if (r.sharedMaterial == mat) dungMat = true;
        Kiem(dungMat, "A quai sinh tu kho quai dung vat lieu Enemy_QuyCay (" + rs.Length + " renderer)");
        Transform hong = null;
        foreach (var t in q.GetComponentsInChildren<Transform>()) if (t.name == "Hips") { hong = t; break; }
        Kiem(hong != null, "A tim thay xuong Hips");
        if (hong == null) hong = q.transform;
        yield return new WaitForSeconds(0.6f);
        float cao = 0f;
        foreach (var r in rs) cao = Mathf.Max(cao, r.bounds.max.y - p.y);
        nk.AppendLine("  cao " + cao.ToString("F2") + " m, hong " + (hong.position.y - p.y).ToString("F2") + " m");

        var cg = new GameObject("TAM_CamQC");
        var cam = cg.AddComponent<Camera>();
        cam.cullingMask = 1 << 31; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        cam.fieldOfView = 40f; cam.nearClipPlane = 0.1f; cam.allowHDR = false;
        cam.enabled = false; cam.aspect = 1f;
        float kc = cao * 1.6f;
        cg.transform.position = p - F * kc + Vector3.up * (cao * 0.7f);
        cg.transform.LookAt(p + Vector3.up * (cao * 0.5f));

        var dem = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        foreach (var b in new[] { "dem", "ngay" })
        {
            if (dem != null) { dem.enabled = false; dem.ApGiay(b == "dem" ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); }
            yield return new WaitForSeconds(0.5f);
            foreach (var r in rs) if (r.sharedMaterial == cu) r.sharedMaterial = mat;
            yield return null;
            var moi = DoRieng(q, cam, hong, "rieng_" + b + "_moi");
            yield return ChupGame("game_" + b + "_moi");
            foreach (var r in rs) if (r.sharedMaterial == mat) r.sharedMaterial = cu;
            yield return null;
            var goc = DoRieng(q, cam, hong, "rieng_" + b + "_cu");
            yield return ChupGame("game_" + b + "_cu");
            foreach (var r in rs) if (r.sharedMaterial == cu) r.sharedMaterial = mat;
            nk.AppendLine("  " + b + " moi: " + In(moi));
            nk.AppendLine("  " + b + " cu : " + In(goc));
            Kiem(moi.am > goc.am + 0.01f, "B " + b + ": da am hon (r-b) " + moi.am.ToString("F3") + " (cu " + goc.am.ToString("F3") + ")");
            Kiem(moi.sang < goc.sang * 1.05f, "B " + b + ": khong sang hon ban cu (" + (moi.sang / Mathf.Max(goc.sang, 1e-4f)).ToString("F2") + "x)");
            // ban dem ban cu da ~0% "xam trang" (anh trang toi + xanh) -> chi xet khi ban cu co tu 2% (ban ngay: 17%)
            if (goc.xamTrang >= 0.02f)
                Kiem(moi.xamTrang < goc.xamTrang * 0.5f, "B " + b + ": diem anh xam trang " + moi.xamTrang.ToString("P1") + " (cu " + goc.xamTrang.ToString("P1") + ") giam > 50%");
        }
        if (dem != null) dem.enabled = true;

        Object.Destroy(cg); Object.Destroy(q); Object.Destroy(cu);
        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 150 - mau Quy cay: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuMauQuyCay] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuMauQuyCay");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        Debug.Log("[ThuMauQuyCay] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
