using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHUP MUC DEN LOC XOAY + GIO LOC (nguoi dung 29/09/2026: "cho den hon 1 chut ca 2 skill", chon "chup 3 muc roi chon"). Menu 93.
///
/// Dat mot LOC XOAY THAT (Tornado.Spawn - hinh tu prefab, dung yen) va mot GIO LOC (BuildGioLoc, dung yen) canh nhau trong Act2,
/// chup DEM va NGAY o bon muc: goc, toi di 10 / 20 / 30%. Lam toi bang MaterialPropertyBlock (_TintColor x k) tren vo + cac he bui
/// + may (khong dung vat lieu goc - vat lieu prefab la asset, sua trong Play la ghi xuong dia); khong dong tia set, hao quang, den,
/// hat cat. Do do sang trung binh vung than moi loc. Anh PlayTestShots/denloc_&lt;muc&gt;_&lt;buoi&gt;.png, bao denloc.txt.
/// </summary>
public static class ThuDenLoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static readonly string[] TenBui = { "BuiChan", "BuiCuonLen", "BuiThanDuoi", "BuiCuon", "KhoiBui", "MayTrongLoc" };
    static readonly float[] Muc = { 1f, 0.9f, 0.8f, 0.7f };

    [MenuItem("Diablo 2.5D/93. Chup MUC DEN Loc xoay + Gio loc (3 muc de chon)", false, 183)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[DenLoc] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_DenLoc");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DenLoc] " + s); }

    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    /// <summary>Cac renderer can lam toi + mau goc cua chung.</summary>
    static void GomRenderer(GameObject goc, List<Renderer> rs, List<Color> mau)
    {
        foreach (var r in goc.GetComponentsInChildren<Renderer>(true))
        {
            bool vo = r is MeshRenderer;
            bool bui = r is ParticleSystemRenderer && System.Array.IndexOf(TenBui, r.name) >= 0;
            if (!vo && !bui) continue;
            if (r.sharedMaterial == null || !r.sharedMaterial.HasProperty("_TintColor")) continue;
            rs.Add(r); mau.Add(r.sharedMaterial.GetColor("_TintColor"));
        }
    }

    static void ApMuc(List<Renderer> rs, List<Color> mau, float k)
    {
        var mpb = new MaterialPropertyBlock();
        for (int i = 0; i < rs.Count; i++)
        {
            if (rs[i] == null) continue;
            rs[i].GetPropertyBlock(mpb);
            var c = mau[i]; c.r *= k; c.g *= k; c.b *= k;
            mpb.SetColor("_TintColor", c);
            rs[i].SetPropertyBlock(mpb);
        }
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] muc den Loc xoay + Gio loc - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        var cam = Camera.main;
        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (chuyen != null) chuyen.enabled = false;

        Vector3 goc = toi != null ? toi.transform.position : Vector3.zero;
        Vector3 huong = toi != null ? toi.transform.forward : Vector3.forward; huong.y = 0f; huong.Normalize();
        Vector3 phai = Vector3.Cross(Vector3.up, huong);
        Vector3 pLx = goc + huong * 22f - phai * 7f;
        // Gio loc: ngay truoc nhan vat 8 m (cach dat cua menu 71c) - lan chup dau dat 18 m ben phai thi bi goc cay che
        Vector3 pGl = goc + huong * 8f;
        pLx.y = GioLoc.MatDatY(pLx, pLx.y); pGl.y = GioLoc.MatDatY(pGl, pGl.y);
        var loc = Tornado.Spawn(pLx, huong, 0);
        loc.moveSpeed = 0f; loc.wanderAmount = 0f; loc.duration = 120f;
        var gl = VfxFactory.BuildGioLoc();
        gl.name = "TAM_GioLocDen";
        gl.transform.position = pGl;
        yield return new WaitForSeconds(3.5f);     // cho bui day

        // Hai khung: RONG cho Loc xoay, GAN cho Gio loc (may quay 9 m sau, cao 3,2 m - nhu 71c)
        Vector3 camLxViTri = goc - huong * 4f + Vector3.up * 5f, camLxNhin = goc + huong * 20f + Vector3.up * 5.5f;
        Vector3 camGlViTri = pGl - huong * 9f + Vector3.up * 3.2f, camGlNhin = pGl + Vector3.up * 2.2f;

        var rsLx = new List<Renderer>(); var mauLx = new List<Color>();
        var rsGl = new List<Renderer>(); var mauGl = new List<Color>();
        GomRenderer(loc.gameObject, rsLx, mauLx);
        GomRenderer(gl, rsGl, mauGl);
        Ghi(string.Format("Loc xoay: {0} renderer lam toi; Gio loc: {1} renderer", rsLx.Count, rsGl.Count));

        foreach (var buoi in new[] { "dem", "ngay" })
        {
            if (chuyen != null) { chuyen.ApGiay(buoi == "dem" ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); yield return null; yield return null; }
            foreach (var k in Muc)
            {
                ApMuc(rsLx, mauLx, k); ApMuc(rsGl, mauGl, k);
                yield return new WaitForSeconds(0.3f);
                float sLx = 0f, sGl = 0f;
                for (int lan = 0; lan < 6; lan++)
                {
                    yield return new WaitForEndOfFrame();
                    cam.transform.position = camLxViTri; cam.transform.LookAt(camLxNhin);
                    sLx += DoVung(Chup(cam, 640, 360), cam, pLx, 2.6f, 14f, 640, 360);
                    cam.transform.position = camGlViTri; cam.transform.LookAt(camGlNhin);
                    sGl += DoVung(Chup(cam, 640, 360), cam, pGl, 1.0f, 4.5f, 640, 360);
                    yield return new WaitForSeconds(0.1f);
                }
                string tenK = ((int)Mathf.Round((1f - k) * 100f)).ToString();
                cam.transform.position = camLxViTri; cam.transform.LookAt(camLxNhin);
                yield return ChupAnh(cam, "PlayTestShots/denloc_" + tenK + "_" + buoi + ".png");
                cam.transform.position = camGlViTri; cam.transform.LookAt(camGlNhin);
                yield return ChupAnh(cam, "PlayTestShots/denloc_gl_" + tenK + "_" + buoi + ".png");
                Ghi(string.Format("{0} toi di {1}%: do sang than Loc xoay {2:F3}, Gio loc {3:F3}", buoi == "dem" ? "DEM " : "NGAY", tenK, sLx / 6f, sGl / 6f));
            }
        }
        ApMuc(rsLx, mauLx, 1f); ApMuc(rsGl, mauGl, 1f);
        Object.Destroy(loc.gameObject);
        Object.Destroy(gl);
        Ket();
    }

    /// <summary>Do sang trung binh hinh chu nhat man hinh bao than loc (tu chan len caoDo, ngang +-rong).</summary>
    static float DoVung(Color[] px, Camera cam, Vector3 chan, float rong, float caoDo, int W, int H)
    {
        float x0 = 1f, x1 = 0f, y0 = 1f, y1 = 0f;
        foreach (var dy in new[] { 0.3f, caoDo * 0.5f, caoDo })
            foreach (var dx in new[] { -rong, rong })
            {
                var v = cam.WorldToViewportPoint(chan + Vector3.up * dy + cam.transform.right * dx);
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
            }
        int ax = Mathf.Clamp((int)(x0 * W), 0, W - 1), bx = Mathf.Clamp((int)(x1 * W), 0, W - 1);
        int ay = Mathf.Clamp((int)(y0 * H), 0, H - 1), by = Mathf.Clamp((int)(y1 * H), 0, H - 1);
        double t = 0; int n = 0;
        for (int y = ay; y <= by; y++) for (int x = ax; x <= bx; x++) { t += Sang(px[y * W + x]); n++; }
        return n > 0 ? (float)(t / n) : 0f;
    }

    static Color[] Chup(Camera cam, int W, int H)
    {
        var rt = new RenderTexture(W, H, 24);
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        var px = t.GetPixels();
        Object.Destroy(t); Object.Destroy(rt);
        return px;
    }

    static IEnumerator ChupAnh(Camera cam, string duong)
    {
        yield return new WaitForEndOfFrame();
        int W = 960, H = 540;
        var rt = new RenderTexture(W, H, 24);
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        File.WriteAllBytes(duong, t.EncodeToPNG());
        Object.Destroy(t); Object.Destroy(rt);
        Ghi("anh: " + duong);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/denloc.txt", bao.ToString());
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t != null && t.parent == null && t.name.StartsWith("TAM_")) Object.Destroy(t.gameObject);
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
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
