using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 149 - MAU DA QUAI QUY DU + VET MAU THAN TREN (01/11/2026, nguoi dung: "trang xam toan than qua xau", gui anh quy sung da xam be
/// van nau, tay chan nau sam; "them nhieu vet mau loang lo tren phan than tren"). Anh mau moi nuong bang Blender MCP
/// (CongCu/Blender/quy_du_mau.py) -> Assets/Textures/QuyDu_Mau.png.
///
/// Vao Play Act2, tat GameDirector, sinh Quy du THAT tu kho quai (AI tat) truoc mat nguoi choi. Do tren anh rieng con quai (lop 31, nen
/// den, may quay can truoc mat), dem va ngay, DOI CHUNG = vat lieu cung prefab nhung _MainTex = anh goc Meshy (meshy_basecolor), cung khung:
///   - diem anh MAU (do: r > 1,5 g va r > 1,5 b) o than TREN (tren xuong hong) va than DUOI;
///   - do am (r - b) va do sang trung binh ca con.
/// Xuong hong lay tu xuong "Hips" cua chinh con quai (chieu len anh) - khong dat cung diem anh.
/// PlayTestShots/quydu_mau/*.png + nhatky.txt.
/// </summary>
public static class ThuMauQuyDu
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/quydu_mau/";
    const string AnhMoi = "Assets/Textures/QuyDu_Mau.png";
    const string AnhGoc = "Assets/MeshyImports/Abyssal Horned Demon_20260830_134822/meshy_basecolor.png";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/149 Mau da quai Quy du + vet mau than tren", false, 223)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuMauQuyDu] dang Play hoac canh chua luu"); return; }
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
        var go = new GameObject("TAM_ThuMauQuyDu");
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

    struct SoDo { public float mauTren, mauDuoi, sang, am; public int bong; }

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
        int yHong = Mathf.RoundToInt(cam.WorldToViewportPoint(hong.position).y * 512);   // cam.aspect = 1
        var px = tx.GetPixels();
        int tren = 0, duoi = 0, mTren = 0, mDuoi = 0; double sang = 0, am = 0;
        for (int i = 0; i < px.Length; i++)
        {
            var c = px[i];
            if (c.r + c.g + c.b < 0.004f) continue;
            bool mau = c.r > 0.04f && c.r > 1.5f * c.g && c.r > 1.5f * c.b;
            if (i / 512 >= yHong) { tren++; if (mau) mTren++; } else { duoi++; if (mau) mDuoi++; }
            sang += 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            am += c.r - c.b;
        }
        Object.Destroy(tx);
        DatLop(q, lopCu);
        int n = tren + duoi;
        return new SoDo
        {
            mauTren = tren > 0 ? (float)mTren / tren : 0f, mauDuoi = duoi > 0 ? (float)mDuoi / duoi : 0f,
            sang = n > 0 ? (float)(sang / n) : 0f, am = n > 0 ? (float)(am / n) : 0f, bong = n
        };
    }

    static string In(SoDo s)
    {
        return "mau than tren " + s.mauTren.ToString("P1") + " · than duoi " + s.mauDuoi.ToString("P1") + " · sang TB " + s.sang.ToString("F3")
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
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Enemy_QuyDu.mat");
        var anh = mat.GetTexture("_MainTex");
        Kiem(anh != null && AssetDatabase.GetAssetPath(anh) == AnhMoi, "A vat lieu: _MainTex = " + (anh != null ? AssetDatabase.GetAssetPath(anh) : "null"));
        Kiem(mat.GetTexture("_BumpMap") != null && mat.GetTexture("_MetallicGlossMap") != null, "A vat lieu: giu anh phap tuyen + do bong Meshy");
        var cu = new Material(mat); cu.name = "TAM_QuyDuCu"; cu.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(AnhGoc));

        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        Vector3 p = toi.transform.position + F * 5f;
        p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p));
        var q = EnemyFactory.Spawn(MonsterType.QuyDu, p, null, toi.transform);
        q.name = "TAM_QuyDu";
        var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
        q.transform.rotation = Quaternion.LookRotation(-F);
        var rs = q.GetComponentsInChildren<Renderer>();
        bool dungMat = false;
        foreach (var r in rs) if (r.sharedMaterial == mat) dungMat = true;
        Kiem(dungMat, "A quai sinh tu kho quai dung vat lieu Enemy_QuyDu (" + rs.Length + " renderer)");
        Transform hong = null;
        foreach (var t in q.GetComponentsInChildren<Transform>()) if (t.name == "Hips") { hong = t; break; }
        Kiem(hong != null, "A tim thay xuong Hips");
        if (hong == null) hong = q.transform;
        yield return new WaitForSeconds(0.6f);
        float cao = 0f;
        foreach (var r in rs) cao = Mathf.Max(cao, r.bounds.max.y - p.y);
        nk.AppendLine("  cao " + cao.ToString("F2") + " m, hong " + (hong.position.y - p.y).ToString("F2") + " m");

        var cg = new GameObject("TAM_CamQD");
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
            // Ban dem anh trang xanh lam mau thanh mang sam (0,1% diem anh con do) - nguoi dung xem anh va CHON GIU (khong phat sang,
            // khong bong uot), nen chi doi mau do o ban ngay
            if (b == "ngay")
            {
                Kiem(moi.mauTren > 0.04f && moi.mauTren > goc.mauTren * 4f, "B " + b + ": vet mau than tren " + moi.mauTren.ToString("P1") + " (cu " + goc.mauTren.ToString("P1") + ") > 4% va > x4 doi chung");
                Kiem(moi.mauTren > moi.mauDuoi * 2.5f, "B " + b + ": mau tap trung than tren: tren " + moi.mauTren.ToString("P1") + " / duoi " + moi.mauDuoi.ToString("P1") + " > x2,5");
            }
            Kiem(moi.am > goc.am + 0.01f, "B " + b + ": da am hon (r-b) " + moi.am.ToString("F3") + " (cu " + goc.am.ToString("F3") + ")");
            Kiem(moi.sang < goc.sang * 1.15f, "B " + b + ": khong sang hon ban cu (" + (moi.sang / Mathf.Max(goc.sang, 1e-4f)).ToString("F2") + "x)");
        }
        if (dem != null) dem.enabled = true;

        Object.Destroy(cg); Object.Destroy(q); Object.Destroy(cu);
        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 149 - mau da Quy du + vet mau: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuMauQuyDu] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuMauQuyDu");
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
        Debug.Log("[ThuMauQuyDu] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
