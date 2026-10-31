using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 148 - MAU AO QUAI PHU THUY (01/11/2026, nguoi dung: "quan ao trang bach toan than qua xau", gui anh nu tu ma: khan trum + ao
/// den, vien khan om mat + yem nguc trang, mat trang bach). Model Meshy "Veil of the Abyss" KHONG co anh mau - vat lieu cu chi co
/// _Color 0,82. Anh mau moi nuong bang Blender MCP (CongCu/Blender/phu_thuy_mau.py) -> Assets/Textures/PhuThuy_Mau.png.
///
/// Vao Play Act2, tat GameDirector, sinh Phu thuy THAT tu kho quai (AI tat) truoc mat nguoi choi. Do:
///   A. Vat lieu prefab dung anh moi (_MainTex = PhuThuy_Mau, _Color trang, giu anh phap tuyen).
///   B. Anh rieng con quai (lop 31, nen den, may quay can 2,4 m truoc mat), dem va ngay, DOI CHUNG = vat lieu cu (khong anh, _Color
///      0,82) cung khung: do sang 3 VUNG lay theo diem the gioi chieu len anh - yem nguc (1,33 m), ao duoi than (0,48 m), khan trum canh
///      mat. Ban cu mot mau nen yem / ao chi chenh vi anh sang (~1,4); ban moi yem trang phai noi han. Nguong tuyet doi ("sang > 0,45")
///      KHONG dung duoc: ban dem ca phan trang cung chi ~0,23.
///   C. Chup man Game o goc may quay 2.5D (dem / ngay, moi / cu) de nguoi dung xem.
/// PlayTestShots/phuthuy_mau/*.png + nhatky.txt.
/// </summary>
public static class ThuMauPhuThuy
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/phuthuy_mau/";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/148 Mau ao quai Phu thuy (nu tu ma)", false, 222)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuMauPhuThuy] dang Play hoac canh chua luu"); return; }
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
        var go = new GameObject("TAM_ThuMauPhuThuy");
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

    /// <summary>Do sang TB cua o chu nhat (nua rong, nua cao theo diem anh) quanh diem the gioi w, chi diem anh khong phai nen.</summary>
    static float DoVung(Texture2D tx, Camera cam, Vector3 w, int nx, int ny)
    {
        Vector3 s = cam.WorldToViewportPoint(w);   // cam.aspect co dinh 1 -> khop anh vuong du targetTexture da go
        int cx = Mathf.RoundToInt(s.x * tx.width), cy = Mathf.RoundToInt(s.y * tx.height);
        double t = 0; int n = 0;
        for (int y = cy - ny; y <= cy + ny; y++)
            for (int x = cx - nx; x <= cx + nx; x++)
            {
                if (x < 0 || y < 0 || x >= tx.width || y >= tx.height) continue;
                var c = tx.GetPixel(x, y);
                if (c.r + c.g + c.b < 0.004f) continue;
                t += 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; n++;
            }
        return n > 0 ? (float)(t / n) : 0f;
    }

    static Vector3 diemYem, diemAo, diemKhan;   // dat trong KichBan theo cho dung + huong may quay
    static Vector3 vung;                        // (yem, ao, khan) lan do gan nhat

    /// <summary>Anh rieng con quai tren nen den: (ti le sang, ti le toi, do sang TB) trong bong; ghi do sang 3 vung vao 'vung'.</summary>
    static Vector3 DoRieng(GameObject q, Camera cam, string ten)
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
        vung = new Vector3(DoVung(tx, cam, diemYem, 10, 20), DoVung(tx, cam, diemAo, 35, 50), DoVung(tx, cam, diemKhan, 10, 25));
        int bong = 0, sang = 0, toi = 0; double tong = 0;
        foreach (var c in tx.GetPixels())
        {
            float l = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            if (c.r + c.g + c.b < 0.004f) continue;   // nen den
            bong++; tong += l;
            if (l > 0.45f) sang++;
            if (l < 0.12f) toi++;
        }
        Object.Destroy(tx);
        DatLop(q, lopCu);
        if (bong == 0) return Vector3.zero;
        return new Vector3((float)sang / bong, (float)toi / bong, (float)(tong / bong));
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
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Enemy_PhuThuy.mat");
        var anh = mat.GetTexture("_MainTex");
        Kiem(anh != null && anh.name == "PhuThuy_Mau", "A vat lieu: _MainTex = " + (anh != null ? anh.name : "null"));
        Kiem(mat.GetColor("_Color") == Color.white, "A vat lieu: _Color = " + mat.GetColor("_Color"));
        Kiem(mat.GetTexture("_BumpMap") != null, "A vat lieu: giu anh phap tuyen " + (mat.GetTexture("_BumpMap") != null ? mat.GetTexture("_BumpMap").name : "null"));
        var cu = new Material(mat); cu.name = "TAM_PhuThuyCu"; cu.SetTexture("_MainTex", null); cu.SetColor("_Color", new Color(0.82f, 0.81f, 0.84f, 1f));

        // Phu thuy that, truoc mat nguoi choi 4 m, nhin ve may quay
        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 p = toi.transform.position + F * 4f;
        p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p));
        var q = EnemyFactory.Spawn(MonsterType.Witch, p, null, toi.transform);
        q.name = "TAM_PhuThuy";
        var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
        q.transform.rotation = Quaternion.LookRotation(-F);
        var rs = q.GetComponentsInChildren<Renderer>();
        bool dungMat = false;
        foreach (var r in rs) if (r.sharedMaterial == mat) dungMat = true;
        Kiem(dungMat, "A quai sinh tu kho quai dung vat lieu Enemy_PhuThuy (" + rs.Length + " renderer)");
        yield return new WaitForSeconds(0.6f);

        // may quay can rieng: 2,4 m truoc mat, cao 1,3 m
        var cg = new GameObject("TAM_CamPT");
        var cam = cg.AddComponent<Camera>();
        cam.cullingMask = 1 << 31; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        cam.fieldOfView = 40f; cam.nearClipPlane = 0.1f; cam.allowHDR = false;
        cam.enabled = false; cam.aspect = 1f;
        cg.transform.position = p - F * 2.4f + Vector3.up * 1.15f;
        cg.transform.LookAt(p + Vector3.up * 0.9f);
        // vung do (do tren anh can cua lan chay dau): yem nguc giua nguc, ao duoi than, khan trum ben trai man hinh canh mat
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        diemYem = p + Vector3.up * 1.33f;
        diemAo = p + Vector3.up * 0.48f + R * 0.03f;
        diemKhan = p + Vector3.up * 1.48f - R * 0.14f;

        var dem = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        string[] buoi = { "dem", "ngay" };
        foreach (var b in buoi)
        {
            if (dem != null) { dem.enabled = false; dem.ApGiay(b == "dem" ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); }
            yield return new WaitForSeconds(0.5f);
            foreach (var r in rs) if (r.sharedMaterial == mat || r.sharedMaterial == cu) r.sharedMaterial = mat;
            yield return null;
            Vector3 moi = DoRieng(q, cam, "rieng_" + b + "_moi"); Vector3 vMoi = vung;
            yield return ChupGame("game_" + b + "_moi");
            foreach (var r in rs) if (r.sharedMaterial == mat) r.sharedMaterial = cu;
            yield return null;
            Vector3 cuS = DoRieng(q, cam, "rieng_" + b + "_cu"); Vector3 vCu = vung;
            yield return ChupGame("game_" + b + "_cu");
            foreach (var r in rs) if (r.sharedMaterial == cu) r.sharedMaterial = mat;
            nk.AppendLine("  " + b + ": moi  bong quai: sang " + moi.x.ToString("P1") + " toi " + moi.y.ToString("P1") + " TB " + moi.z.ToString("F3")
                          + " | yem " + vMoi.x.ToString("F3") + " ao " + vMoi.y.ToString("F3") + " khan " + vMoi.z.ToString("F3"));
            nk.AppendLine("  " + b + ": cu   bong quai: sang " + cuS.x.ToString("P1") + " toi " + cuS.y.ToString("P1") + " TB " + cuS.z.ToString("F3")
                          + " | yem " + vCu.x.ToString("F3") + " ao " + vCu.y.ToString("F3") + " khan " + vCu.z.ToString("F3"));
            float tlMoi = vMoi.x / Mathf.Max(vMoi.y, 1e-4f), tlCu = vCu.x / Mathf.Max(vCu.y, 1e-4f);
            float khMoi = vMoi.x / Mathf.Max(vMoi.z, 1e-4f), khCu = vCu.x / Mathf.Max(vCu.z, 1e-4f);
            // yem trang noi han tren ao + khan den (ban cu cung mot mau: chi chenh vi anh sang)
            Kiem(tlMoi > tlCu * 1.5f, "B " + b + ": yem / ao " + tlMoi.ToString("F2") + " (cu " + tlCu.ToString("F2") + ") > x1,5 doi chung");
            Kiem(khMoi > khCu * 1.5f, "B " + b + ": yem / khan trum " + khMoi.ToString("F2") + " (cu " + khCu.ToString("F2") + ") > x1,5 doi chung");
            // ao + khan toi hon han ban cu (den), yem giu sang (khong bi toi theo)
            Kiem(vMoi.y < vCu.y * 0.6f && vMoi.z < vCu.z * 0.6f, "B " + b + ": ao + khan toi hon ban cu: x" + (vMoi.y / Mathf.Max(vCu.y, 1e-4f)).ToString("F2") + " / x" + (vMoi.z / Mathf.Max(vCu.z, 1e-4f)).ToString("F2"));
            Kiem(vMoi.x > vCu.x * 0.7f, "B " + b + ": yem giu sang: x" + (vMoi.x / Mathf.Max(vCu.x, 1e-4f)).ToString("F2") + " ban cu");
            Kiem(cuS.z > moi.z * 1.5f, "B " + b + ": ca con quai toi hon ban cu (TB x" + (cuS.z / Mathf.Max(moi.z, 1e-4f)).ToString("F2") + ")");
        }
        if (dem != null) dem.enabled = true;

        Object.Destroy(cg); Object.Destroy(q); Object.Destroy(cu);
        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 148 - mau ao Phu thuy: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuMauPhuThuy] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuMauPhuThuy");
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
        Debug.Log("[ThuMauPhuThuy] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
