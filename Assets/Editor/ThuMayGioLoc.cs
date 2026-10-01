using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHUP MAY GIONG TRONG GIO LOC o 3 MUC DAY (nguoi dung 01/10/2026: "cho may giong ben trong nhieu va day hon 1 chut nhung van phai
/// ben trong than loc", chon "chup 3 muc roi chon" + "mo xuong 1,8 - 4,3 m"). Menu 94.
///
/// Mot Gio loc dung yen (BuildGioLoc) truoc nhan vat 8 m; moi muc: xoa he "MayTrongLoc", dung lai bang VfxFactory.MayDinhGioLoc(k),
/// cho 3 s (doi hat 1,8 - 2,4 s) roi do: so dam, do cao tam dam, xa truc / ban kinh vo trong O DUNG DO CAO DAM (phai &lt; 0,8).
/// DOI CHUNG "cu" = cau hinh 29/09/2026 dung tay ngay trong phep thu (hop 2,5 - 4,3 m +-0,65, 20 dam, 6/giay, do duc 0,45 - 0,65).
/// Do "may thay duoc bao nhieu": chup cung khung hai lan - bat / tat renderer may - lay chenh lech do sang trung binh vung than
/// (trung binh 12 khung trong 3 s). Anh PlayTestShots/maygl_&lt;muc&gt;_&lt;buoi&gt;.png, bao maygl.txt.
/// </summary>
public static class ThuMayGioLoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static readonly float[] Muc = { 0f, 1.3f, 1.5f, 1.8f };   // 0 = cau hinh cu (doi chung)

    [MenuItem("Diablo 2.5D/94. Chup MAY GIONG trong Gio loc (3 muc day de chon)", false, 184)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[MayGL] scene co thay doi chua luu"); return; }
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
        var go = new GameObject("TAM_MayGioLoc");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[MayGL] " + s); }
    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static ParticleSystem TimMay(GameObject gl)
    {
        foreach (var ps in gl.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "MayTrongLoc") return ps;
        return null;
    }

    /// <summary>Doi chung: dung lai dung cau hinh 29/09/2026 tren he vua dung (chua chay - PlayOnStart bat o khung sau).</summary>
    static void DatCauHinhCu(ParticleSystem ps)
    {
        ps.transform.localPosition = new Vector3(0f, 3.4f, 0f);
        var m = ps.main;
        m.maxParticles = 20;
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.45f), new Color(1f, 1f, 1f, 0.65f));
        var em = ps.emission; em.rateOverTime = 6f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)8) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box;
        sh.rotation = Vector3.zero;
        sh.scale = new Vector3(1.3f, 1.8f, 1.3f);
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 2] may giong trong Gio loc - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        int loi = 0;
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
        Vector3 pGl = goc + huong * 8f;
        pGl.y = GioLoc.MatDatY(pGl, pGl.y);
        var gl = VfxFactory.BuildGioLoc();
        gl.name = "TAM_GioLocMay";
        gl.transform.position = pGl;
        // Lan 1 may quay 9 m: than loc chiem het khung, khong phan biet duoc cac muc -> lui ra 13 m thay ca con loc
        Vector3 camViTri = pGl - huong * 13f + Vector3.up * 2.8f, camNhin = pGl + Vector3.up * 2.5f;

        foreach (var k in Muc)
        {
            string ten = k <= 0f ? "cu" : ((int)Mathf.Round(k * 10f)).ToString();   // 13 / 15 / 18
            var cu = TimMay(gl);
            if (cu != null) Object.DestroyImmediate(cu.gameObject);
            VfxFactory.MayDinhGioLoc(gl.transform, k <= 0f ? 1f : k);
            var ps = TimMay(gl);
            if (k <= 0f) DatCauHinhCu(ps);
            yield return new WaitForSeconds(3f);

            var hat = new ParticleSystem.Particle[ps.main.maxParticles];
            int n = ps.GetParticles(hat);
            float yMin = 99f, yMax = -99f, tl = 0f; int duoi25 = 0;
            for (int i = 0; i < n; i++)
            {
                Vector3 l = gl.transform.InverseTransformPoint(ps.transform.TransformPoint(hat[i].position));
                float rr = new Vector2(l.x, l.z).magnitude;
                yMin = Mathf.Min(yMin, l.y); yMax = Mathf.Max(yMax, l.y);
                tl = Mathf.Max(tl, rr / VfxFactory.BanKinhVoTrongGioLoc(l.y));
                if (l.y < 2.4f) duoi25++;
            }
            var m = ps.main;
            Ghi(string.Format("Muc {0}: {1} dam (tran {2}, {3:F1}/giay), tam dam cao {4:F2} - {5:F2} m ({6} dam duoi 2,4 m), xa truc nhat = {7:P0} ban kinh vo trong, do duc {8:F2} - {9:F2}",
                ten, n, m.maxParticles, ps.emission.rateOverTime.constant, yMin, yMax, duoi25, tl, m.startColor.colorMin.a, m.startColor.colorMax.a));
            if (tl >= 0.8f) { Ghi("[LOI] may lo ra ngoai vo loc o muc " + ten); loi++; }
            if (k > 0f && (yMin < 1.7f || yMax > 4.45f)) { Ghi("[LOI] may ngoai vung 1,8 - 4,3 m o muc " + ten); loi++; }

            var r = ps.GetComponent<Renderer>();
            foreach (var buoi in new[] { "dem", "ngay" })
            {
                if (chuyen != null) { chuyen.ApGiay(buoi == "dem" ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); yield return null; yield return null; }
                cam.transform.position = camViTri; cam.transform.LookAt(camNhin);
                double tong = 0;
                for (int lan = 0; lan < 12; lan++)
                {
                    yield return new WaitForEndOfFrame();
                    r.enabled = true;  var a = Chup(cam, 640, 360);
                    r.enabled = false; var b = Chup(cam, 640, 360);
                    r.enabled = true;
                    tong += ChenhVung(a, b, cam, pGl, 640, 360);
                    yield return new WaitForSeconds(0.25f);
                }
                yield return ChupAnh(cam, "PlayTestShots/maygl_" + ten + "_" + buoi + ".png");
                Ghi(string.Format("   {0}: may lam vung than doi do sang trung binh {1:F4} (bat / tat renderer may cung khung)", buoi == "dem" ? "DEM " : "NGAY", tong / 12.0));
            }
        }
        Object.Destroy(gl);
        Ghi(loi == 0 ? "KET QUA: 0 loi" : "KET QUA: " + loi + " loi");
        Ket();
    }

    /// <summary>Chenh lech do sang trung binh (|a - b|) trong hinh chu nhat man hinh bao than loc (0,3 - 5 m, ngang +-2 m).</summary>
    static float ChenhVung(Color[] a, Color[] b, Camera cam, Vector3 chan, int W, int H)
    {
        float x0 = 1f, x1 = 0f, y0 = 1f, y1 = 0f;
        foreach (var dy in new[] { 0.3f, 2.6f, 5f })
            foreach (var dx in new[] { -2f, 2f })
            {
                var v = cam.WorldToViewportPoint(chan + Vector3.up * dy + cam.transform.right * dx);
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
            }
        int ax = Mathf.Clamp((int)(x0 * W), 0, W - 1), bx = Mathf.Clamp((int)(x1 * W), 0, W - 1);
        int ay = Mathf.Clamp((int)(y0 * H), 0, H - 1), by = Mathf.Clamp((int)(y1 * H), 0, H - 1);
        double t = 0; int n = 0;
        for (int y = ay; y <= by; y++) for (int x = ax; x <= bx; x++) { t += Mathf.Abs(Sang(a[y * W + x]) - Sang(b[y * W + x])); n++; }
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
        Ghi("   anh: " + duong);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/maygl.txt", bao.ToString());
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
