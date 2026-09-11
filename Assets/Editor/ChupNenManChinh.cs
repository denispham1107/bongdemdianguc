using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHUP NEN MAN HINH CHINH (khong co giao dien) de nhin lo da va ngon lua.
///
/// Vao Play o MainMenu, tat giao dien dang nhap / sanh, doi lua chay deu 3
/// giay, roi chup: toan canh, can lo da ben trai, can ngon lua. Ghi kem so do:
/// tung tam flipbook (khoi den, hai tam lua) co dung mot hat va dung texture
/// LuaLo / KhoiDen khong, vat lieu da co du ba texture khong.
///
/// Anh ra <c>PlayTestShots/nen_*.png</c>, so do ra <c>PlayTestShots/nenmanchinh.txt</c>.
/// </summary>
public static class ChupNenManChinh
{
    static readonly StringBuilder bao = new StringBuilder();
    static bool daBatDau;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static bool coPhien; static string phien;
    const string KhoaPhien = "diablo25d_refresh";

    [MenuItem("Diablo 2.5D/51c. Chup nen man chinh (lo da, ngon lua)", false, 140)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; daBatDau = false;
        coPhien = PlayerPrefs.HasKey(KhoaPhien); phien = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();       // khong tu dang nhap
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ChupNen");
        go.AddComponent<ChayThuMang>().batDau = Kich();
    }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    static void ChupKhung(Camera cam, string ten, int w, int h)
    {
        var rt = new RenderTexture(w, h, 24);
        var cu = cam.targetTexture;
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var truoc = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(w, h, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, w, h), 0, 0); t.Apply();
        RenderTexture.active = truoc;
        File.WriteAllBytes("PlayTestShots/" + ten + ".png", t.EncodeToPNG());
        Object.DestroyImmediate(t); rt.Release(); Object.DestroyImmediate(rt);
    }

    static IEnumerator Kich()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var dn in Object.FindObjectsByType<ManDangNhap>()) dn.enabled = false;
        foreach (var s in Object.FindObjectsByType<ManSanh>()) s.enabled = false;
        yield return new WaitForSeconds(3.0f);

        var cam = Camera.main;
        Vector3 camPos = cam.transform.position; Quaternion camRot = cam.transform.rotation;

        // ---- So do ----
        var cacLo = Object.FindObjectsByType<LoLuaDa>();
        bao.AppendLine("so lo da: " + cacLo.Length + ", man hinh " + Screen.width + "x" + Screen.height);
        foreach (var lo in cacLo)
        {
            // Moi tam flipbook (KhoiDen, LuaA, LuaB) phai dung dung 1 hat song, dung vat lieu co texture
            var tam = new StringBuilder();
            foreach (var ps in lo.GetComponentsInChildren<ParticleSystem>())
            {
                if (ps.name != "KhoiDen" && ps.name != "LuaA" && ps.name != "LuaB" && ps.name != "TanLua") continue;
                var vlp = ps.GetComponent<ParticleSystemRenderer>().sharedMaterial;
                var tx = vlp != null ? vlp.mainTexture : null;
                tam.AppendFormat(" [{0}: hat {1}, vat lieu {2}, texture {3}]", ps.name, ps.particleCount,
                    vlp != null ? vlp.name : "-", tx != null ? tx.name + " " + tx.width + "x" + tx.height : "-");
            }
            var mr = lo.GetComponentInChildren<MeshRenderer>();
            var vl = mr != null ? mr.sharedMaterial : null;
            bao.AppendLine(string.Format("   {0}:{1}\n      vat lieu da {2} (mau {3}, phap {4}, bong {5})",
                lo.name, tam, vl != null ? vl.name : "-",
                vl != null && vl.GetTexture("_MainTex") != null, vl != null && vl.GetTexture("_BumpMap") != null,
                vl != null && vl.GetTexture("_MetallicGlossMap") != null));
        }

        yield return new WaitForEndOfFrame();
        bao.AppendLine(string.Format("khung hinh man chinh: tam giac {0:F0}k, vat do bong {1}, lan goi ve {2}",
            UnityStats.triangles / 1000f, UnityStats.shadowCasters, UnityStats.drawCalls));
        int soDen = 0; foreach (var l in Object.FindObjectsByType<Light>()) if (l.enabled) soDen++;
        bao.AppendLine("so den dang bat: " + soDen + ", so renderer: " + Object.FindObjectsByType<Renderer>().Length);
        yield return Chup("nen_1_toancanh");
        // Game view trong Editor co ti le tuy y (vd 1568x505) - render them dung
        // khung 16:9 nguoi choi thay
        yield return new WaitForEndOfFrame();
        ChupKhung(cam, "nen_0_1080p", 1920, 1080);

        // ---- Can lo ben trai ----
        var trai = GameObject.Find("LoLua_Trai");
        if (trai != null)
        {
            var p = trai.transform.position;
            var huong = (camPos - p); huong.y = 0; huong.Normalize();
            cam.transform.position = p + huong * 2.1f + Vector3.up * 1.15f;
            cam.transform.LookAt(p + Vector3.up * 0.85f);
            yield return null; yield return null;
            yield return Chup("nen_2_canlo");

            cam.transform.position = p + huong * 1.5f + Vector3.up * 1.75f;
            cam.transform.LookAt(p + Vector3.up * 1.55f);
            yield return null; yield return null;
            yield return Chup("nen_3_canlua");
            yield return new WaitForSeconds(0.35f);
            yield return Chup("nen_4_canlua_b");
        }
        cam.transform.SetPositionAndRotation(camPos, camRot);

        File.WriteAllText("PlayTestShots/nenmanchinh.txt", bao.ToString());
        Debug.Log("[ChupNen]\n" + bao);
        var rac = GameObject.Find("TAM_ChupNen");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        if (coPhien) PlayerPrefs.SetString(KhoaPhien, phien); else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
    }
}
