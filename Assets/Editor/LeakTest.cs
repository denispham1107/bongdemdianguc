using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// KIEM TRA RAC: tung phep lua va phep bang, doi vai giay roi dem xem
/// con vat the hieu ung nao SOT LAI tren man hinh khong.
/// Vao menu "Diablo 2.5D > 7. Kiem tra hieu ung con sot".
/// </summary>
public static class LeakTest
{
    static float t0 = -1f;
    static int stage;
    static bool prevOpt;
    static EnterPlayModeOptions prevMode;

    [MenuItem("Diablo 2.5D/7. Kiem tra hieu ung con sot (Leak Test)", false, 42)]
    public static void Run()
    {
        prevOpt = EditorSettings.enterPlayModeOptionsEnabled;
        prevMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/Act1.unity");
        EditorApplication.update += Tick;
        stage = 0;
        t0 = -1f;
        EditorApplication.EnterPlaymode();
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.realtimeSinceStartup;
        float t = Time.realtimeSinceStartup - t0;

        var player = Object.FindAnyObjectByType<PlayerController>();

        switch (stage)
        {
            case 0:
                if (t > 1.5f && player != null)
                {
                    // Ban 3 qua cau lua ve 3 huong
                    player.CastAt(0, player.transform.position + new Vector3(7f, 0f, 3f));
                    stage++;
                }
                break;
            case 1:
                if (t > 2.4f && player != null)
                {
                    player.CastAt(0, player.transform.position + new Vector3(-6f, 0f, 5f));
                    stage++;
                }
                break;
            case 2:
                if (t > 3.4f && player != null)
                {
                    player.CastAt(1, player.transform.position + new Vector3(0f, 0f, 7f));
                    stage++;
                }
                break;
            case 3:
                if (t > 6f) { Report("SAU 6 GIAY"); Shot("leak_06s"); stage++; }
                break;
            case 4:
                if (t > 14f)
                {
                    Report("SAU 14 GIAY (luc nay phai SACH)");
                    Shot("leak_14s");
                    EditorApplication.update -= Tick;
                    EditorSettings.enterPlayModeOptionsEnabled = prevOpt;
                    EditorSettings.enterPlayModeOptions = prevMode;
                    EditorApplication.isPlaying = false;
                    EditorApplication.Exit(0);
                }
                break;
        }
    }

    static void Shot(string name)
    {
        var cam = Camera.main;
        if (cam == null) return;

        const int W = 1280, H = 720;
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGBHalf);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;

        string dir = System.Environment.GetEnvironmentVariable("LEAK_SHOT_DIR");
        if (string.IsNullOrEmpty(dir)) dir = System.IO.Directory.GetCurrentDirectory();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(rt);
        Debug.Log("[LeakTest] Da chup: " + name);
    }

    /// <summary>Dem cac vat the hieu ung con lai trong scene.</summary>
    static void Report(string title)
    {
        var counts = new Dictionary<string, int>();

        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            var tr = all[i];
            if (tr.parent != null) continue;           // chi dem vat the goc

            string n = tr.name;
            if (n.Contains("Vfx_") || n.Contains("Explosion") || n.Contains("Impact") ||
                n.Contains("Shatter") || n.Contains("Storm") || n.Contains("Shard") ||
                n.Contains("Fireball") || n.Contains("Decal") || n.Contains("Hit") ||
                n.Contains("Skill_") || n.Contains("Dmg"))
            {
                if (!counts.ContainsKey(n)) counts[n] = 0;
                counts[n]++;
            }
        }

        var sb = new StringBuilder();
        sb.Append("[LeakTest] ").Append(title).Append(" - ");
        if (counts.Count == 0) sb.Append("khong con gi (tot).");
        else
            foreach (var kv in counts) sb.Append(kv.Key).Append(" x").Append(kv.Value).Append("; ");

        // Dem chi tiet cac mieng hinh con dang hien
        int expandFades = Object.FindObjectsByType<ExpandFade>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int autoDestroys = Object.FindObjectsByType<AutoDestroy>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        sb.Append(" | ExpandFade=").Append(expandFades)
          .Append(" AutoDestroy=").Append(autoDestroys)
          .Append(" ParticleSystem=").Append(particles);

        Debug.Log(sb.ToString());
    }
}
