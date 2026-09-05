using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CONG CU DUNG MAN CHOI.
/// Vao menu "Diablo 2.5D" tren thanh menu cua Unity de chay.
///
/// Man choi nay KHONG luu san hang nghin vat the trong scene: chi can mot vat the
/// ten "GAME" mang script GameBootstrap, con lai (dat da, phu thuy, quai vat,
/// hieu ung lua bang) deu duoc dung bang code ngay khi bam Play.
/// </summary>
public static class WorldBuilder
{
    const string SceneDir = "Assets/Scenes";
    const string ScenePath = SceneDir + "/Act1.unity";

    [MenuItem("Diablo 2.5D/5. Dung Scene Trong (khong dung prefab)", false, 40)]
    public static void BuildScene()
    {
        if (!Application.isBatchMode &&
            !EditorUtility.DisplayDialog(
                "Dung man choi",
                "Cong cu se tao lai scene Act1 (moi thay doi tay trong scene do se mat).\n\nTiep tuc?",
                "Dung ngay", "Huy"))
            return;

        BuildSilent();

        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Xong!",
                "Man choi da san sang.\n\nBam nut Play de choi.\n\n" +
                "Chuot trai: di chuyen - Chuot phai: tung phep\n" +
                "Phim 1: Qua cau lua - 2: Mua bang - 3: Sam set - 4: Loc xoay\n" +
                "Phim C: doi goc nhin 3D / 2.5D / 2D",
                "Choi thoi");
    }

    /// <summary>Ban chay khong hoi han - dung cho dong lenh.</summary>
    public static void BuildSilent()
    {
        EnsureLayersAndTags();

        if (!AssetDatabase.IsValidFolder(SceneDir))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Camera ---
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        // near 0,5 chu khong 0,15, far 220 chu khong 400.
        //
        // Do chinh xac cua bo dem chieu sau phu thuoc TI LE far/near, khong phai
        // hieu cua chung. 400/0,15 = 2667 la rat xau: hai be mat cach nhau vai
        // centimet o khoang 10 m khong con phan biet duoc, va tren WebGL dien
        // thoai (bo dem 16 bit) chung loang lo thanh tung o vuong. 220/0,5 = 440,
        // tot hon SAU LAN.
        //
        // Cat gi khong: may quay gan nhat la 4,1 m (zoom het co o goc nhin 3D tu
        // do), con xa nhat qua ban do 109 m la khoang 150 m - va o 200 m suong mu
        // da che 98,8% roi.
        cam.nearClipPlane = 0.5f;
        cam.farClipPlane = 220f;
        cam.allowHDR = true;
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, 12f, -12f);
        camGo.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        // --- Anh trang ---
        var lightGo = new GameObject("Moonlight");
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Directional;
        lt.color = new Color(0.55f, 0.62f, 0.85f);
        lt.intensity = 0.55f;
        lt.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(48f, 145f, 0f);

        // --- Vat the khoi dong game ---
        var gameGo = new GameObject("GAME");
        gameGo.AddComponent<GameBootstrap>();

        EditorSceneManager.SaveScene(scene, ScenePath);

        // Dua scene vao danh sach build
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AddShadersToAlwaysIncluded();

        Debug.Log("[Diablo 2.5D] Da dung xong man choi: " + ScenePath);
    }

    [MenuItem("Diablo 2.5D/2. Mo Man Choi (Open Scene)", false, 1)]
    public static void OpenScene()
    {
        if (System.IO.File.Exists(ScenePath))
            EditorSceneManager.OpenScene(ScenePath);
        else
            EditorUtility.DisplayDialog("Chua co man choi",
                "Hay chay 'Diablo 2.5D > 1. Dung Man Choi' truoc da.", "OK");
    }

    // ================================================================
    //  CHUAN BI LAYER / TAG
    // ================================================================

    static void EnsureLayersAndTags()
    {
        EnsureLayer("Ground", 8);
        EnsureLayer("Player", 9);
        EnsureLayer("Enemy", 10);
        EnsureLayer("Spell", 11);
        EnsureTag("Player");
        EnsureTag("Enemy");
    }

    static void EnsureLayer(string name, int index)
    {
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (asset == null || asset.Length == 0) return;
        var so = new SerializedObject(asset[0]);
        var layers = so.FindProperty("layers");
        if (layers == null || index >= layers.arraySize) return;

        // Neu o do trong hoac chua dung ten thi dat lai
        for (int i = 8; i < layers.arraySize; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == name) return;

        var slot = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(slot.stringValue))
        {
            slot.stringValue = name;
            so.ApplyModifiedProperties();
        }
    }

    static void EnsureTag(string name)
    {
        var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (asset == null || asset.Length == 0) return;
        var so = new SerializedObject(asset[0]);
        var tags = so.FindProperty("tags");
        if (tags == null) return;

        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == name) return;

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = name;
        so.ApplyModifiedProperties();
    }

    // ================================================================
    //  DAM BAO SHADER TU VIET KHONG BI CAT KHI BUILD
    // ================================================================

    /// <summary>
    /// Vat lieu duoc tao bang code luc chay, nen Unity khong biet la co dung
    /// nhung shader nay. Phai khai bao "luon dua vao ban build".
    /// </summary>
    static void AddShadersToAlwaysIncluded()
    {
        string[] names =
        {
            "Diablo25D/Fire", "Diablo25D/Ice", "Diablo25D/FrozenShell",
            "Diablo25D/ParticleAdditive", "Diablo25D/ParticleAlpha",
            "Diablo25D/SkyGradient", "Diablo25D/Bloom", "Diablo25D/ClothTwoSided"
        };

        var graphics = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (graphics == null || graphics.Length == 0) return;

        var so = new SerializedObject(graphics[0]);
        var list = so.FindProperty("m_AlwaysIncludedShaders");
        if (list == null) return;

        for (int i = 0; i < names.Length; i++)
        {
            var sh = Shader.Find(names[i]);
            if (sh == null) continue;

            bool found = false;
            for (int k = 0; k < list.arraySize; k++)
                if (list.GetArrayElementAtIndex(k).objectReferenceValue == sh) { found = true; break; }

            if (found) continue;

            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = sh;
        }

        so.ApplyModifiedProperties();
    }
}
