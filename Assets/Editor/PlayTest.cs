using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU TU DONG: mo man choi, bam Play, tu tung phep lua va mua bang,
/// chup lai vai tam hinh o cac goc nhin khac nhau roi thoat.
/// Dung de kiem tra nhanh xem game co chay dung khong.
/// Vao menu "Diablo 2.5D > 4. Chay thu va chup hinh".
/// </summary>
public static class PlayTest
{
    static float t0 = -1f;
    static int stage;
    static int errors;
    static string outDir;
    static bool prevPlayModeOptionsEnabled;
    static EnterPlayModeOptions prevPlayModeOptions;
    static readonly List<GameObject> posed = new List<GameObject>();

    [MenuItem("Diablo 2.5D/4. Chay thu va chup hinh (Play Test)", false, 21)]
    public static void Run()
    {
        outDir = ReadArg("-shotDir", Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots"));
        Directory.CreateDirectory(outDir);

        prevPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        prevPlayModeOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/Act1.unity");

        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
        stage = 0;
        errors = 0;
        t0 = -1f;
        posed.Clear();

        EditorApplication.EnterPlaymode();
    }

    /// <summary>Chup rieng man hinh chinh (MainMenu).</summary>
    [MenuItem("Diablo 2.5D/6. Chup man hinh chinh", false, 41)]
    public static void RunMenuShot()
    {
        outDir = ReadArg("-shotDir", Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots"));
        Directory.CreateDirectory(outDir);

        prevPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        prevPlayModeOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Application.logMessageReceived += OnLog;
        EditorApplication.update += MenuTick;
        errors = 0;
        t0 = -1f;
        EditorApplication.EnterPlaymode();
    }

    static void MenuTick()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.realtimeSinceStartup;
        if (Time.realtimeSinceStartup - t0 < 2.0f) return;

        Shot("00_man_hinh_chinh");
        Debug.Log("[PlayTest] Xong man hinh chinh. So loi: " + errors);

        EditorApplication.update -= MenuTick;
        Application.logMessageReceived -= OnLog;
        EditorSettings.enterPlayModeOptionsEnabled = prevPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptions = prevPlayModeOptions;
        EditorApplication.isPlaying = false;
        EditorApplication.Exit(errors > 0 ? 2 : 0);
    }

    static string ReadArg(string key, string fallback)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == key) return args[i + 1];
        return fallback;
    }

    static void OnLog(string condition, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            errors++;
            Debug.Log("[PlayTest][LOI] " + condition + "\n" + stack);
        }
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.realtimeSinceStartup;
        float t = Time.realtimeSinceStartup - t0;

        var player = Object.FindAnyObjectByType<PlayerController>();
        var rig = Object.FindAnyObjectByType<CameraRig>();

        switch (stage)
        {
            // ---- Chan dung tung nhan vat ----
            case 0:
                if (t > 0.8f) { SpawnPoseModels(); stage++; }
                break;
            case 1:
                if (t > 1.3f)
                {
                    if (player != null) Portrait("P1_phu_thuy", player.transform, 2.6f, 1.15f);
                    stage++;
                }
                break;
            case 2:
                if (t > 1.7f)
                {
                    string[] names = { "P2_quy_lun", "P3_bo_xuong", "P4_xac_song", "P5_quy_khong_lo" };
                    for (int i = 0; i < posed.Count && i < names.Length; i++)
                        if (posed[i] != null)
                            Portrait(names[i], posed[i].transform, 3.0f, 1.0f);
                    ClearPosed();
                    stage++;
                }
                break;

            // ---- Goc nhin va phep thuat ----
            case 3: if (t > 2.2f) { Shot("01_goc_3D"); stage++; } break;
            case 4: if (t > 2.5f) { if (rig != null) rig.SetView(1); stage++; } break;
            case 5: if (t > 3.8f) { Shot("02_goc_25D"); stage++; } break;
            case 6:
                if (t > 4.2f)
                {
                    if (player != null) player.CastAt(0, AimPoint(player, 9f));
                    stage++;
                }
                break;
            case 7: if (t > 4.75f) { Shot("03_lua_dang_bay"); stage++; } break;
            case 8: if (t > 5.3f) { Shot("04_lua_no"); stage++; } break;
            case 9:
                if (t > 6.0f)
                {
                    if (player != null)
                        player.CastAt(1, player.transform.position + player.transform.forward * 6f);
                    stage++;
                }
                break;
            case 10: if (t > 7.1f) { Shot("05_mua_bang"); stage++; } break;
            case 11: if (t > 8.2f) { Shot("06_mua_bang_2"); stage++; } break;

            // ---- Sam set ----
            case 12:
                if (t > 8.8f)
                {
                    if (player != null) player.CastAt(2, AimPoint(player, 7f));
                    stage++;
                }
                break;
            case 13: if (t > 9.5f) { Shot("07_sam_set"); stage++; } break;
            case 14: if (t > 10.3f) { Shot("08_sam_set_2"); stage++; } break;

            case 15: if (t > 10.9f) { if (rig != null) rig.SetView(2); stage++; } break;
            case 16: if (t > 12.1f) { Shot("09_goc_2D_tren_xuong"); stage++; } break;
            case 17: if (t > 12.5f) { if (rig != null) rig.SetView(3); stage++; } break;
            case 18: if (t > 13.7f) { Shot("10_goc_2D_ngang"); stage++; } break;
            case 19:
                if (t > 14.1f)
                {
                    int enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude).Length;
                    Debug.Log("[PlayTest] So quai dang song: " + enemies);
                    Debug.Log("[PlayTest] Xong. So loi ghi nhan: " + errors);
                    EditorApplication.update -= Tick;
                    Application.logMessageReceived -= OnLog;
                    EditorSettings.enterPlayModeOptionsEnabled = prevPlayModeOptionsEnabled;
                    EditorSettings.enterPlayModeOptions = prevPlayModeOptions;
                    EditorApplication.isPlaying = false;
                    EditorApplication.Exit(errors > 0 ? 2 : 0);
                }
                break;
        }
    }

    /// <summary>Tha 4 loai quai dung yen gan nhan vat de chup chan dung.</summary>
    static void SpawnPoseModels()
    {
        var types = new[] { MonsterType.Fallen, MonsterType.Skeleton, MonsterType.Ghoul, MonsterType.Brute };
        for (int i = 0; i < types.Length; i++)
        {
            var go = EnemyFactory.Spawn(types[i], new Vector3(-9f + i * 6f, 0.05f, -12f), null, null);
            var ai = go.GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;             // dung im de chup cho ro
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            posed.Add(go);
        }
    }

    static void ClearPosed()
    {
        for (int i = 0; i < posed.Count; i++)
            if (posed[i] != null) Object.Destroy(posed[i]);
        posed.Clear();
    }

    /// <summary>Chup can canh mot nhan vat (tat camera game trong luc chup).</summary>
    static void Portrait(string name, Transform subject, float distance, float height)
    {
        var cam = Camera.main;
        if (cam == null || subject == null) return;

        var rig = cam.GetComponent<CameraRig>();
        bool had = rig != null && rig.enabled;
        if (rig != null) rig.enabled = false;

        // Dat camera TRUOC MAT nhan vat de thay ro khuon mat
        Vector3 focus = subject.position + Vector3.up * height;
        Quaternion rot = Quaternion.Euler(6f, subject.eulerAngles.y + 180f, 0f);
        cam.transform.position = focus - rot * Vector3.forward * distance;
        cam.transform.rotation = rot;

        Shot(name);

        if (rig != null) rig.enabled = had;
    }

    /// <summary>Ngam vao con quai gan nhat, khong co thi ngam thang phia truoc.</summary>
    static Vector3 AimPoint(PlayerController player, float fallbackDistance)
    {
        var enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude);
        Transform best = null;
        float bestD = float.MaxValue;
        for (int i = 0; i < enemies.Length; i++)
        {
            float d = Vector3.Distance(enemies[i].transform.position, player.transform.position);
            if (d < bestD && d < 14f) { bestD = d; best = enemies[i].transform; }
        }
        if (best != null) return best.position;
        return player.transform.position + player.transform.forward * fallbackDistance;
    }

    static void Shot(string name)
    {
        var cam = Camera.main;
        if (cam == null) { Debug.Log("[PlayTest] Khong tim thay camera."); return; }

        const int W = 1280, H = 720;
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGBHalf);
        var prevTarget = cam.targetTexture;
        var prevActive = RenderTexture.active;

        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();

        cam.targetTexture = prevTarget;
        RenderTexture.active = prevActive;

        File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        rt.Release();
        Object.DestroyImmediate(rt);

        Debug.Log("[PlayTest] Da chup: " + name);
    }
}
