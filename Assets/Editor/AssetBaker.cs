using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CONG CU "NUONG" ASSET.
///
/// Bam mot nut la toan bo nhan vat, quai vat, phep thuat, hieu ung va canh vat
/// (von duoc sinh bang code) se duoc GHI THANH FILE THAT trong Project:
///
///   Assets/Textures   - hoa tiet vai, da, xuong, lua, bang... (file .png)
///   Assets/Materials  - vat lieu (file .mat)
///   Assets/Models     - hinh khoi 3D (file .asset, moi nhan vat mot file)
///   Assets/Prefabs    - phu thuy, 4 loai quai, phep lua, phep bang, hieu ung, canh vat
///   Assets/Scenes     - man choi Act1 da dat san day du vat the
///
/// Sau khi nuong xong ban co the bam vao tung prefab de xem, sua, keo tha
/// nhu moi du an Unity binh thuong.
/// </summary>
public static class AssetBaker
{
    const string TexDir = "Assets/Textures";
    const string MatDir = "Assets/Materials";
    const string MeshDir = "Assets/Models";
    const string PrefabDir = "Assets/Prefabs";
    const string SceneDir = "Assets/Scenes";
    const string TerrainDir = "Assets/Terrain";
    const string ScenePath = SceneDir + "/Act1.unity";
    const string MenuPath = SceneDir + "/MainMenu.unity";

    /// <summary>Hat giong ngau nhien khi nuong prefab (hinh dang da, cay...). Giu nguyen.</summary>
    const int WorldSeed = 12345;

    // Thong so cua man choi duy nhat. Day von la thong so cua Act2 cu
    // (rong hon, quai dong hon) - Act1 cu da bi bo.
    const float ArenaRadius = 42f;
    const int ActSeed = 777;
    // Do quai dot dau. 8 con la qua nang: moi con 9 sat thuong / 1.6 giay, tam
    // con vay kin la 51 sat thuong moi giay, ma nhan vat chi co 170 mau - dung
    // yen 3.3 giay la guc. Ma khi da guc thi Update cua nhan vat thoat ngay o
    // dong dau, bam phim ky nang nao cung khong an. Tra ve 5 nhu ban dau.
    const int StartingEnemies = 4;
    const float WaveDelay = 3f;

    static Dictionary<Texture2D, Texture2D> texMap;
    static Dictionary<Material, Material> matMap;
    static Dictionary<Mesh, Mesh> meshMap;
    static HashSet<string> usedNames;
    static TerrainLayer[] terrainLayers;

    // ================================================================
    //  MENU
    // ================================================================

    [MenuItem("Diablo 2.5D/1. Nuong Asset + Dung Man Choi", false, 0)]
    public static void BakeEverything()
    {
        if (!Application.isBatchMode &&
            !EditorUtility.DisplayDialog(
                "Nuong asset",
                "Cong cu se tao lai toan bo Textures / Materials / Models / Prefabs\n" +
                "va dung lai scene Act1.\n\nMoi chinh sua tay trong nhung thu muc do se mat.\n\nTiep tuc?",
                "Lam ngay", "Huy"))
            return;

        BakeSilent();

        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Xong!",
                "Da nuong xong asset va dung lai man choi.\n\n" +
                "Mo Assets/Prefabs de xem nhan vat, quai vat, phep thuat.\n" +
                "Bam Play de choi.",
                "Tuyet");
    }

    public static void BakeSilent()
    {
        texMap = new Dictionary<Texture2D, Texture2D>();
        matMap = new Dictionary<Material, Material>();
        meshMap = new Dictionary<Mesh, Mesh>();
        usedNames = new HashSet<string>();
        terrainLayers = null;

        EnsureFolders();
        ClearGenerated();

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var lib = BakePrefabs();

        // Hai man hinh: menu chinh va man choi
        BuildScene(lib, ScenePath, "Act1", ActSeed, ArenaRadius, StartingEnemies, WaveDelay);
        BuildMenuScene(lib);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(MenuPath, true),
            new EditorBuildSettingsScene(ScenePath, true),
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        VerifyPrefabs();

        Debug.Log("[AssetBaker] Xong: " + texMap.Count + " anh, " + matMap.Count + " vat lieu, "
                  + meshMap.Count + " hinh khoi, scene: " + ScenePath);
    }

    // ================================================================
    //  KIEM TRA PREFAB SAU KHI NUONG
    // ================================================================

    /// <summary>
    /// Bat loi "mat script": Unity chi giu duoc mot MonoBehaviour khi class do
    /// nam trong file CUNG TEN. Neu sai, prefab se ghi m_Script: {fileID: 0}
    /// va cac script tu huy (ExpandFade, AutoDestroy...) se khong chay -> hieu ung
    /// dinh mai tren man hinh.
    /// </summary>
    static void VerifyPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDir });
        int broken = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string txt = File.ReadAllText(path);
            int n = 0, idx = 0;
            while ((idx = txt.IndexOf("m_Script: {fileID: 0}", idx)) >= 0) { n++; idx += 10; }

            if (n > 0)
            {
                broken++;
                Debug.LogError("[AssetBaker] Prefab MAT SCRIPT (" + n + " cho): " + path +
                               " - moi class MonoBehaviour phai nam trong file cung ten voi class.");
            }
        }

        if (broken == 0)
            Debug.Log("[AssetBaker] Kiem tra prefab: tat ca " + guids.Length + " prefab deu giu du script.");
    }

    // ================================================================
    //  THU MUC
    // ================================================================

    static void EnsureFolders()
    {
        string[] dirs = { TexDir, MatDir, MeshDir, PrefabDir, SceneDir, TerrainDir };
        for (int i = 0; i < dirs.Length; i++)
            if (!AssetDatabase.IsValidFolder(dirs[i]))
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(dirs[i]));
    }

    static void ClearGenerated()
    {
        // CO Y khong co TerrainDir: dia hinh ban da to tay phai duoc giu lai.
        // Muon xoa lam lai thi dung menu "8. Dung lai dia hinh".
        string[] dirs = { TexDir, MatDir, MeshDir, PrefabDir };
        for (int i = 0; i < dirs.Length; i++)
        {
            var guids = AssetDatabase.FindAssets("", new[] { dirs[i] });
            for (int k = 0; k < guids.Length; k++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[k]);
                if (!string.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
                    AssetDatabase.DeleteAsset(path);
            }
        }
        AssetDatabase.Refresh();
    }

    // ================================================================
    //  NUONG PREFAB
    // ================================================================

    class Library
    {
        public GameObject player;
        public GameObject[] enemies = new GameObject[7];
        public GameObject fireball, iceStorm, iceStormField, iceShard;
        public GameObject lightningStorm, lightningStormField, lightningImpact, castLightning, stunned;
        public GameObject tornado;
        public GameObject fireExplosion, iceImpact, burning, castFire, castIce, frozenShatter, hitBurst;
        public GameObject brazier;
        public List<GameObject> rocks = new List<GameObject>();
        public List<GameObject> pillars = new List<GameObject>();
        public List<GameObject> trees = new List<GameObject>();
        public List<GameObject> leafyTrees = new List<GameObject>();
        public List<GameObject> bushes = new List<GameObject>();
        public GameObject bone;
        public List<GameObject> graves = new List<GameObject>();
        public List<GameObject> tents = new List<GameObject>();
        public List<GameObject> hangingBones = new List<GameObject>();
        public List<GameObject> hangedBones = new List<GameObject>();
    }

    static Library BakePrefabs()
    {
        var lib = new Library();
        var temp = new GameObject("__BakeTemp");

        // ---------- NHAN VAT ----------
        var bootHolder = new GameObject("__Boot");
        var boot = bootHolder.AddComponent<GameBootstrap>();

        // Nhan vat dung MODEL SAN neu tim thay, khong thi ve bang code nhu cu
        // Tu khoa phai la manh ten RIENG cua model. Trong MeshyImports dang co
        // ca "Violet Lichlord" lan "The Violet Lich King" lan "Voidborn Lich
        // King" - lay tu khoa "Lich" thi trung ca ba.
        var modelNguoiChoi = DungModel("Voidborn", "Player_PhuThuy");
        GameObject playerGo = modelNguoiChoi != null
            ? boot.BuildPlayerFromModel(modelNguoiChoi)
            : boot.BuildPlayerFromCode();

        if (modelNguoiChoi == null)
            Debug.LogWarning("[AssetBaker] Khong tim thay model nhan vat - ve bang code.");

        lib.player = SavePrefab(playerGo, "Player_Sorceress");
        Object.DestroyImmediate(bootHolder);

        // ---------- QUAI VAT ----------
        string[] enemyNames = { "Enemy_QuyLun", "Enemy_BoXuong", "Enemy_XacSong", "Enemy_QuyKhongLo" };
        for (int i = 0; i < 4; i++)
        {
            // Bo xuong gio dung MODEL SAN, bo qua o day
            if (i == (int)MonsterType.Skeleton) continue;

            var go = EnemyFactory.BuildFromCode((MonsterType)i, Vector3.zero, null, null);
            lib.enemies[i] = SavePrefab(go, enemyNames[i]);
        }

        // Hai loai dung MODEL SAN dua tu Meshy vao
        lib.enemies[(int)MonsterType.Witch] = NuongQuaiTuModel(
            "Veil", "Enemy_PhuThuy", new Color(0.82f, 0.81f, 0.84f),
            EnemyFactory.LapRapPhuThuy);

        lib.enemies[(int)MonsterType.Skeleton] = NuongQuaiTuModel(
            "Bone_Warlord", "Enemy_BoXuong", new Color(0.80f, 0.76f, 0.68f),
            EnemyFactory.LapRapBoXuong);

        // Tu khoa PHAI la "Abyssal", khong duoc la "Abyss" cung khong duoc la
        // "Horned". Trong MeshyImports dang co ba con dinh nhau:
        //   Abyssal Horned Demon  (Quy du)
        //   Horned Shadowfiend    (Quy cay)
        //   Veil of the Abyss     (Phu thuy)
        // "Abyss" trung con phu thuy, "Horned" trung con Quy cay - lay nham la
        // Quy du bi dung bang model cua con khac ma khong bao loi gi.
        lib.enemies[(int)MonsterType.QuyDu] = NuongQuaiTuModel(
            "Abyssal", "Enemy_QuyDu", new Color(0.78f, 0.75f, 0.70f),
            EnemyFactory.LapRapQuyDu);

        lib.enemies[(int)MonsterType.QuyCay] = NuongQuaiTuModel(
            "Shadowfiend", "Enemy_QuyCay", new Color(0.72f, 0.78f, 0.70f),
            EnemyFactory.LapRapQuyCay);

        // Thieu model thi quay ve dung bang code, cho khoi mat hang mot loai quai
        if (lib.enemies[(int)MonsterType.Skeleton] == null)
        {
            Debug.LogWarning("[AssetBaker] Khong co model bo xuong - dung tam con ve bang code.");
            var go = EnemyFactory.BuildFromCode(MonsterType.Skeleton, Vector3.zero, null, null);
            lib.enemies[(int)MonsterType.Skeleton] = SavePrefab(go, "Enemy_BoXuong");
        }

        // ---------- KY NANG ----------
        var fireballGo = new GameObject("Skill_QuaCauLua");
        var fb = fireballGo.AddComponent<Fireball>();
        VfxFactory.BuildFireballVisual(fireballGo.transform, fb.bodyRadius);
        lib.fireball = SavePrefab(fireballGo, "Skill_QuaCauLua");

        var stormGo = new GameObject("Skill_MuaBang");
        stormGo.AddComponent<IceStorm>();
        lib.iceStorm = SavePrefab(stormGo, "Skill_MuaBang");

        var fieldGo = VfxFactory.BuildIceStormField(Vector3.zero, 5.5f, 5f);
        RemoveComponent<AutoDestroy>(fieldGo);
        lib.iceStormField = SavePrefab(fieldGo, "Vfx_VungBaoTuyet");

        var shardGo = VfxFactory.BuildIceShardFalling(Vector3.zero, 0f, 0.42f);
        shardGo.transform.position = Vector3.zero;
        shardGo.transform.rotation = Quaternion.identity;
        lib.iceShard = SavePrefab(shardGo, "Vfx_TangBangRoi");

        var boltStormGo = new GameObject("Skill_SamSet");
        boltStormGo.AddComponent<LightningStorm>();
        lib.lightningStorm = SavePrefab(boltStormGo, "Skill_SamSet");

        var boltFieldGo = VfxFactory.BuildLightningStormField(Vector3.zero, 6f, 2.4f);
        RemoveComponent<AutoDestroy>(boltFieldGo);
        lib.lightningStormField = SavePrefab(boltFieldGo, "Vfx_VungGiong");

        var tornadoGo = VfxFactory.BuildTornado(1f);
        tornadoGo.AddComponent<Tornado>();
        lib.tornado = SavePrefab(tornadoGo, "Skill_LocXoay");

        // ---------- HIEU UNG ----------
        var boom = VfxFactory.BuildFireExplosion(Vector3.zero, 3.4f);
        lib.fireExplosion = SavePrefab(boom, "Vfx_NoLua");

        var ice = VfxFactory.BuildIceImpact(Vector3.zero, 1.7f);
        lib.iceImpact = SavePrefab(ice, "Vfx_NoBang");

        var shock = VfxFactory.BuildLightningImpact(Vector3.zero, 2.1f);
        lib.lightningImpact = SavePrefab(shock, "Vfx_SetChamDat");

        var stun = VfxFactory.BuildStunned(temp.transform, 0f, 0.42f);
        stun.transform.SetParent(null, false);
        stun.transform.localPosition = Vector3.zero;
        lib.stunned = SavePrefab(stun, "Vfx_Choang");

        var burn = VfxFactory.BuildBurning(temp.transform, 1.8f, 0.42f);
        burn.transform.SetParent(null, false);
        burn.transform.localPosition = Vector3.zero;
        lib.burning = SavePrefab(burn, "Vfx_BongChay");

        var chargeF = VfxFactory.BuildCastCharge(temp.transform, true, 0.5f);
        chargeF.transform.SetParent(null, false);
        RemoveComponent<AutoDestroy>(chargeF);
        lib.castFire = SavePrefab(chargeF, "Vfx_TichTuLua");

        var chargeI = VfxFactory.BuildCastCharge(temp.transform, false, 0.7f);
        chargeI.transform.SetParent(null, false);
        RemoveComponent<AutoDestroy>(chargeI);
        lib.castIce = SavePrefab(chargeI, "Vfx_TichTuBang");

        var chargeB = VfxFactory.BuildCastCharge(temp.transform, 2, 0.62f);
        chargeB.transform.SetParent(null, false);
        RemoveComponent<AutoDestroy>(chargeB);
        lib.castLightning = SavePrefab(chargeB, "Vfx_TichTuSet");

        var shatter = VfxFactory.BuildFrozenShatter(Vector3.zero, 1f);
        lib.frozenShatter = SavePrefab(shatter, "Vfx_VoBang");

        var hit = VfxFactory.BuildHitBurst(Vector3.zero, new Color(0.6f, 0.05f, 0.05f));
        lib.hitBurst = SavePrefab(hit, "Vfx_TrungDon");

        // ---------- CANH VAT ----------
        Random.InitState(WorldSeed);

        for (int i = 0; i < 6; i++)
        {
            var rock = WorldFactory.BuildRockProp(null, 100 + i, 1f, true);
            lib.rocks.Add(SavePrefab(rock, "Prop_DaTang_" + (i + 1)));
        }
        for (int i = 0; i < 3; i++)
        {
            var pil = WorldFactory.BuildPillar(null, Vector3.zero, 2.0f + i * 1.2f, i);
            lib.pillars.Add(SavePrefab(pil, "Prop_CotDa_" + (i + 1)));
        }
        for (int i = 0; i < 3; i++)
        {
            var tree = WorldFactory.BuildDeadTree(null, Vector3.zero, i);
            lib.trees.Add(SavePrefab(tree, "Prop_CayChet_" + (i + 1)));
        }
        for (int i = 0; i < 4; i++)
        {
            var tree = WorldFactory.BuildLeafyTree(null, Vector3.zero, i);
            lib.leafyTrees.Add(SavePrefab(tree, "Prop_CayXanh_" + (i + 1)));
        }
        for (int i = 0; i < 4; i++)
        {
            var bush = WorldFactory.BuildBush(null, Vector3.zero, i);
            lib.bushes.Add(SavePrefab(bush, "Prop_BuiRam_" + (i + 1)));
        }

        var brz = WorldFactory.BuildBrazier(null, Vector3.zero);
        lib.brazier = SavePrefab(brz, "Prop_LoLua");

        var boneGo = WorldFactory.BuildBoneProp(null, 3);
        lib.bone = SavePrefab(boneGo, "Prop_KhucXuong");

        // ---------- BO XUONG TREO TREN CAY ----------
        for (int i = 0; i < 3; i++)
        {
            var xt = GraveFactory.BoXuongTreo(null, 700 + i * 23);
            lib.hangingBones.Add(SavePrefab(xt, "Prop_XuongTreo_" + (i + 1)));
        }

        // ---------- XAC BI TREO CO TREN CANH CAY ----------
        for (int i = 0; i < 3; i++)
        {
            var tc = GraveFactory.BoXuongTreoCo(null, 900 + i * 29);
            lib.hangedBones.Add(SavePrefab(tc, "Prop_XuongTreoCo_" + (i + 1)));
        }

        // ---------- NAM NGOI MO ----------
        string[] tenMo = { "BiaDaNut", "ThanhGiaGay", "MoBatNap", "QuanTaiDa", "CocTreoSo" };
        for (int i = 0; i < GraveFactory.SoKieu; i++)
        {
            var mo = GraveFactory.Build(null, i, 400 + i * 17);
            lib.graves.Add(SavePrefab(mo, "Prop_Mo_" + (i + 1) + "_" + tenMo[i]));
        }

        // ---------- BON CAN LEU THAY MO ----------
        // Nuong SAN bon mau khac nhau chu khong dung mot mau dat bon cho: bon
        // can giong het nhau dung canh nhau tren cung mot ban do la lo ngay.
        for (int i = 0; i < 4; i++)
        {
            var leu = LeuFactory.Build(null, 1300 + i * 53);
            lib.tents.Add(SavePrefab(leu, "Prop_Leu_ThayMo_" + (i + 1)));
        }

        Object.DestroyImmediate(temp);
        AssetDatabase.SaveAssets();
        return lib;
    }

    // ================================================================
    //  MU PHU THUY (model san dua tu Meshy vao)
    // ================================================================

    /// <summary>Thu muc chua model dua tu Meshy vao. KHONG bi xoa luc nuong lai.</summary>
    const string MeshyDir = "Assets/MeshyImports";

    /// <summary>
    /// Nuong RIENG prefab Quy du, khong dung den phan con lai cua du an.
    ///
    /// Muc 1 nuong lai TAT CA va dung lai ca ba canh - chay no chi de them mot
    /// con quai la xoa sach dia hinh Act2 da ve lai va moi thu dat tay khac.
    /// </summary>
    [MenuItem("Diablo 2.5D/11. Nuong rieng prefab Quy du", false, 92)]
    public static void NuongRiengQuyDu()
    {
        // PHAI khoi tao ba bo dem nay bang tay.
        //
        // Chung von chi duoc dung o dau muc 1 (BakeSilent). Goi thang xuong day
        // ma khong khoi tao thi BakeMaterial nem NullReferenceException ngay
        // dong dau - va luc do model da duoc dung trong canh dang mo roi, nen
        // canh dinh ba vat the rac. Toi da vap dung the.
        DungBoDem();

        var pf = NuongQuaiTuModel("Abyssal", "Enemy_QuyDu",
                                  new Color(0.78f, 0.75f, 0.70f),
                                  EnemyFactory.LapRapQuyDu);
        if (pf == null)
        {
            Debug.LogError("[AssetBaker] Khong nuong duoc Quy du - thieu model 'Horned' trong "
                         + MeshyDir + ".");
            return;
        }
        Debug.Log("[AssetBaker] Da nuong " + AssetDatabase.GetAssetPath(pf)
                + ". Con phai gan no vao mang enemyPrefabs cua tung canh nua.");
    }

    /// <summary>Nuong RIENG prefab Quy cay. Xem ghi chu o NuongRiengQuyDu.</summary>
    [MenuItem("Diablo 2.5D/12. Nuong rieng prefab Quy cay", false, 93)]
    public static void NuongRiengQuyCay()
    {
        DungBoDem();

        var pf = NuongQuaiTuModel("Shadowfiend", "Enemy_QuyCay",
                                  new Color(0.72f, 0.78f, 0.70f),
                                  EnemyFactory.LapRapQuyCay);
        if (pf == null)
        {
            Debug.LogError("[AssetBaker] Khong nuong duoc Quy cay - thieu model 'Shadowfiend' trong "
                         + MeshyDir + ".");
            return;
        }
        Debug.Log("[AssetBaker] Da nuong " + AssetDatabase.GetAssetPath(pf)
                + ". Con phai gan no vao mang enemyPrefabs cua tung canh nua.");
    }

    /// <summary>
    /// Nuong RIENG prefab LOC XOAY.
    ///
    /// Can den moi khi doi HINH DANG con loc trong VfxFactory.BuildTornado.
    /// Prefab da co san 12 vat the con - ba lop vo, ba dai xoan, nam he hat va
    /// mot ngon den - va <see cref="Tornado.Start"/> chi dung hinh bang code khi
    /// prefab KHONG co con nao. Nen sua BuildTornado ma khong nuong lai thi
    /// trong game van la con loc cu, y nguyen kich thuoc cu.
    /// </summary>
    [MenuItem("Diablo 2.5D/13. Nuong rieng prefab Loc xoay", false, 94)]
    public static void NuongRiengLocXoay()
    {
        DungBoDem();

        var go = VfxFactory.BuildTornado(1f);
        go.AddComponent<Tornado>();
        var pf = SavePrefab(go, "Skill_LocXoay");

        if (pf == null)
        {
            Debug.LogError("[AssetBaker] Khong nuong duoc Loc xoay.");
            return;
        }

        var t = pf.GetComponent<Tornado>();
        Debug.Log("[AssetBaker] Da nuong " + AssetDatabase.GetAssetPath(pf)
                + "  catchRadius=" + (t != null ? t.catchRadius.ToString("F2") : "?")
                + ". Prefab ghi de dung duong dan cu nen cac canh van tro dung.");
    }

    /// <summary>
    /// Khoi tao ba bo dem cho mot lan nuong LE, khong chay ca muc 1.
    ///
    /// Nap san moi ten file DANG CO trong Materials va Textures vao usedNames.
    /// Khong nap thi UniqueName tuong chua ai dung ten do, va
    /// AssetDatabase.CreateAsset se GHI DE thang len vat lieu cua con quai khac.
    /// </summary>
    static void DungBoDem()
    {
        texMap = new Dictionary<Texture2D, Texture2D>();
        matMap = new Dictionary<Material, Material>();
        meshMap = new Dictionary<Mesh, Mesh>();
        usedNames = new HashSet<string>();

        EnsureFolders();

        foreach (var thuMuc in new[] { MatDir, TexDir, MeshDir })
        {
            if (!AssetDatabase.IsValidFolder(thuMuc)) continue;
            foreach (var d in AssetDatabase.FindAssets("", new[] { thuMuc }))
            {
                string duong = AssetDatabase.GUIDToAssetPath(d);
                string ten = Path.GetFileNameWithoutExtension(duong);
                if (!string.IsNullOrEmpty(ten)) usedNames.Add(ten);
            }
        }
    }

    /// <summary>Kieu ham lap rap mot con quai quanh cai model.</summary>
    delegate GameObject HamLapRap(GameObject model, Transform parent, Transform player);

    /// <summary>
    /// Nuong prefab mot con quai tu model Meshy.
    ///
    /// <paramref name="tuKhoa"/> la manh ten RIENG cua con do trong duong dan,
    /// vi du "Veil" hay "Bone_Warlord". PHAI co: truoc day toi tim file dau tien
    /// co chu "Walking" trong ca thu muc, den luc co hai con thi mu phu thuy bi
    /// dung bang dung cai model bo xuong.
    ///
    /// Tra ve null neu khong tim thay model - luc do nguoi goi tu lieu.
    /// </summary>
    static GameObject NuongQuaiTuModel(string tuKhoa, string tenPrefab,
                                       Color mauKhiThieuAnh, HamLapRap lapRap)
    {
        string fbx = TimFbxTheoTen(tuKhoa);
        if (string.IsNullOrEmpty(fbx))
        {
            Debug.LogWarning("[AssetBaker] Khong tim thay model '" + tuKhoa + "' trong "
                             + MeshyDir + ".");
            return null;
        }

        var model = DungModel(tuKhoa, tenPrefab, mauKhiThieuAnh);
        if (model == null) return null;

        var quai = lapRap(model, null, null);
        var pf = SavePrefab(quai, tenPrefab);

        Debug.Log("[AssetBaker] Da nuong " + tenPrefab + " tu "
                  + System.IO.Path.GetFileName(fbx));
        return pf;
    }

    /// <summary>
    /// Dung mot ban model Meshy trong canh, da chinh cach nhap va mac vat lieu
    /// tu te, san sang de lap thanh nhan vat hoac quai.
    ///
    /// Tra ve null neu khong tim thay model mang <paramref name="tuKhoa"/>.
    /// </summary>
    static GameObject DungModel(string tuKhoa, string tenVatLieu)
    {
        return DungModel(tuKhoa, tenVatLieu, Color.white);
    }

    static GameObject DungModel(string tuKhoa, string tenVatLieu, Color mauKhiThieuAnh)
    {
        string fbx = TimFbxTheoTen(tuKhoa);
        if (string.IsNullOrEmpty(fbx))
        {
            Debug.LogWarning("[AssetBaker] Khong tim thay model '" + tuKhoa + "' trong " + MeshyDir + ".");
            return null;
        }

        if (!ChuanBiModel(fbx)) return null;

        var goc = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        if (goc == null)
        {
            Debug.LogWarning("[AssetBaker] Khong nap duoc " + fbx);
            return null;
        }

        var model = (GameObject)PrefabUtility.InstantiatePrefab(goc);
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely,
                                           InteractionMode.AutomatedAction);

        MacAoChoModel(model, fbx, tenVatLieu, mauKhiThieuAnh);
        return model;
    }

    /// <summary>
    /// Tim file FBX co <paramref name="tuKhoa"/> trong duong dan.
    ///
    /// Uu tien ban CO HOAT HINH (ten co chu "Walking" hoac "Animation"): ban
    /// "Character_output" chi co mot clip rong, dung no thi con quai truot di
    /// nhu ma khong nhac chan.
    /// </summary>
    static string TimFbxTheoTen(string tuKhoa)
    {
        if (!AssetDatabase.IsValidFolder(MeshyDir)) return null;

        var guids = AssetDatabase.FindAssets("t:Model", new[] { MeshyDir });
        string duNhat = null;

        for (int i = 0; i < guids.Length; i++)
        {
            string d = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (!d.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
            if (d.IndexOf(tuKhoa, System.StringComparison.OrdinalIgnoreCase) < 0) continue;

            if (d.IndexOf("Walking", System.StringComparison.OrdinalIgnoreCase) >= 0
                || d.IndexOf("Animation", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return d;

            if (duNhat == null) duNhat = d;
        }
        return duNhat;
    }

    /// <summary>
    /// Chinh cach nhap file FBX cho hop voi cach dung trong game.
    ///
    /// Meshy dat san kieu Generic. Doi sang LEGACY vi moi con chi co DUNG MOT
    /// clip: voi Legacy thi mot dong lenh <c>Animation.Play</c> la xong, con
    /// Generic thi phai dung them mot file dieu khien (AnimatorController) chi
    /// de bat tat mot clip duy nhat.
    ///
    /// Va PHAI bat lap lai cho clip di bo: khong lap thi con quai buoc dung mot
    /// buoc roi dung cung do giua duong.
    ///
    /// Ham nay chay lai duoc nhieu lan; da chinh dung roi thi khong nhap lai nua
    /// (nhap lai file mot van dinh moi lan nuong thi rat lau).
    /// </summary>
    static bool ChuanBiModel(string fbx)
    {
        var imp = AssetImporter.GetAtPath(fbx) as ModelImporter;
        if (imp == null)
        {
            Debug.LogWarning("[AssetBaker] " + fbx + " khong phai file mo hinh.");
            return false;
        }

        var clips = imp.clipAnimations;
        if (clips == null || clips.Length == 0) clips = imp.defaultClipAnimations;

        bool dungRoi = imp.animationType == ModelImporterAnimationType.Legacy
                       && clips != null && clips.Length > 0
                       && clips[0].name == TenClipDi && clips[0].loopTime;
        if (dungRoi) return true;

        imp.animationType = ModelImporterAnimationType.Legacy;
        imp.importAnimation = true;

        if (clips != null && clips.Length > 0)
        {
            clips[0].name = TenClipDi;
            clips[0].loopTime = true;
            clips[0].wrapMode = WrapMode.Loop;
            imp.clipAnimations = clips;
        }

        imp.SaveAndReimport();
        Debug.Log("[AssetBaker] Da chinh cach nhap " + System.IO.Path.GetFileName(fbx)
                  + ": Legacy + clip '" + TenClipDi + "' lap lai.");
        return true;
    }

    /// <summary>
    /// Nang chat luong cach nhap cho moi anh trong mot thu muc model.
    ///
    /// HAI THU DUOC SUA:
    ///
    /// 1. KIEU NEN. Unity mac dinh nen anh kieu cu (DXT1/DXT5): no chia anh
    ///    thanh tung o 4x4 diem anh roi rut moi o xuong con hai mau, cac diem
    ///    con lai pha tu hai mau do. Voi bo giap nhieu chi tiet va nhieu mau
    ///    ruc thi cach do de lo ra tung o vuong. Doi sang CompressedHQ
    ///    (tuc BC7 tren may tinh): cung ton bay nhieu bo nho, nhung moi o giu
    ///    duoc nhieu mau hon han.
    ///
    /// 2. DO LOC XIEN (anisotropic). Mac dinh chi co 1, tuc la khong loc xien.
    ///    Camera cua game nhin cheo tu tren xuong 48 do, nen mat dat va nhung
    ///    be mat nam ngang deu bi nhin rat xien - dung muc 1 thi cang xa cang
    ///    nhoe va rung ram. Nang len 4 la het, ma gan nhu khong ton them gi.
    ///
    /// Chi nhap lai nhung anh THUC SU can doi, vi nhap lai mot anh 2048 la
    /// vai giay, ma moi lan nuong lai deu chay qua day.
    /// </summary>
    static void NangChatLuongAnh(string thuMuc)
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { thuMuc });
        int daSua = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string d = AssetDatabase.GUIDToAssetPath(guids[i]);
            var ti = AssetImporter.GetAtPath(d) as TextureImporter;
            if (ti == null) continue;

            bool doi = false;

            if (ti.textureCompression != TextureImporterCompression.CompressedHQ)
            {
                ti.textureCompression = TextureImporterCompression.CompressedHQ;
                doi = true;
            }

            if (ti.anisoLevel < 4)
            {
                ti.anisoLevel = 4;
                doi = true;
            }

            // Noi tran kich thuoc len 8192. Day chi la TRAN, khong phai ep:
            // anh goc 2048 van giu nguyen 2048, khong ton them gi.
            //
            // CAI GIA cua anh 8K: mot anh 8192x8192 nen BC7 chiem khoang 67 MB
            // bo nho card do hoa, cong them cac muc thu nho la gan 90 MB - cho
            // MOT anh. Ca ba anh cua nhan vat la khoang 130 MB. Chap nhan duoc
            // tren may tinh de ban, nhung dung rai kieu do cho hang chuc con
            // quai.
            if (ti.maxTextureSize < 8192)
            {
                ti.maxTextureSize = 8192;
                doi = true;
            }

            if (!ti.mipmapEnabled)
            {
                ti.mipmapEnabled = true;
                doi = true;
            }

            if (doi)
            {
                ti.SaveAndReimport();
                daSua++;
            }
        }

        if (daSua > 0)
            Debug.Log("[AssetBaker] Da nang chat luong " + daSua + " anh trong "
                      + System.IO.Path.GetFileName(thuMuc));
    }

    /// <summary>
    /// Lam vat lieu tu te cho mot model Meshy.
    ///
    /// Meshy gui kem MOI mot anh gan (normal map), KHONG co anh mau. Te hon nua
    /// la cai anh gan do cung khong duoc noi vao vat lieu: ket qua la mot pho
    /// tuong trang loa, phang li, khong thay lay mot nep vai.
    ///
    /// O day noi lai anh gan, ha do bong xuong cho ra chat vai/xuong, va ha mau
    /// trang tinh (1,1,1) xuong mot bac. Trang tinh duoi anh trang xanh cua man
    /// choi se chay trang xoa het chi tiet; xuong mot bac thi cac nep moi con
    /// cho toi cho sang de nhin ra hinh.
    ///
    /// Neu sau nay ban xuat lai tu Meshy CO KEM anh mau, cu de file anh do canh
    /// file FBX - ham nay tu tim thay va dung, khoi phai sua code.
    /// </summary>
    static void MacAoChoModel(GameObject model, string fbx, string tenVatLieu,
                              Color mauKhiThieuAnh)
    {
        var smr = model.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (smr == null) return;

        var vl = new Material(Shader.Find("Standard"));
        vl.name = tenVatLieu;

        string thuMuc = System.IO.Path.GetDirectoryName(fbx).Replace('\\', '/');
        NangChatLuongAnh(thuMuc);

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { thuMuc });

        Texture2D anhMau = null, anhGan = null, anhKimLoai = null;
        for (int i = 0; i < guids.Length; i++)
        {
            string d = AssetDatabase.GUIDToAssetPath(guids[i]);
            string ten = System.IO.Path.GetFileNameWithoutExtension(d).ToLowerInvariant();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(d);
            if (tex == null) continue;

            // Xet "metallic" TRUOC "normal": ten file cua Meshy la
            // meshy_metallic_smoothness, khong dinh chu normal, nhung xet sau
            // basecolor thi de bi nhanh else-if nuot mat.
            if (ten.Contains("metallic") || ten.Contains("smoothness")) anhKimLoai = tex;
            else if (ten.Contains("normal")) anhGan = tex;
            else if (ten.Contains("basecolor") || ten.Contains("albedo")
                     || ten.Contains("diffuse") || ten.Contains("texture")) anhMau = tex;
        }

        if (anhMau != null)
        {
            vl.mainTexture = anhMau;
            vl.color = Color.white;
        }
        else
        {
            vl.color = mauKhiThieuAnh;
        }

        if (anhGan != null)
        {
            vl.SetTexture("_BumpMap", anhGan);
            vl.EnableKeyword("_NORMALMAP");
            vl.SetFloat("_BumpScale", 1.6f);
        }

        if (anhKimLoai != null)
        {
            // Co anh kim loai/do bong rieng thi dung no, va PHAI bat tu khoa
            // _METALLICGLOSSMAP - khong bat thi Unity van deo anh vao nhung
            // shader bo qua, ket qua nhin y het nhu khong co.
            vl.SetTexture("_MetallicGlossMap", anhKimLoai);
            vl.EnableKeyword("_METALLICGLOSSMAP");
            vl.SetFloat("_GlossMapScale", 1f);
        }
        else
        {
            vl.SetFloat("_Glossiness", 0.06f);      // vai tho / xuong kho, khong bong
            vl.SetFloat("_Metallic", 0f);
        }

        AssetDatabase.CreateAsset(vl, MatDir + "/" + tenVatLieu + ".mat");
        smr.sharedMaterial = vl;

        Debug.Log("[AssetBaker] Vat lieu " + tenVatLieu + ": anh mau = "
                  + (anhMau != null ? anhMau.name : "khong co (dung mau phang)")
                  + " | anh gan = " + (anhGan != null ? anhGan.name : "khong co")
                  + " | anh kim loai = " + (anhKimLoai != null ? anhKimLoai.name : "khong co"));
    }

    /// <summary>Ten clip di bo sau khi doi. Phai khop voi ModelHoatHinh.tenClipDi.</summary>
    public const string TenClipDi = "diBo";

    static void RemoveComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c != null) Object.DestroyImmediate(c);
    }

    // ================================================================
    //  GHI MOT VAT THE THANH PREFAB (kem hinh khoi + vat lieu + anh)
    // ================================================================

    static GameObject SavePrefab(GameObject root, string name)
    {
        root.name = name;
        root.transform.position = Vector3.zero;

        SaveMeshes(root, name);
        RemapMaterials(root);

        string path = PrefabDir + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    /// <summary>Ghi moi hinh khoi cua vat the vao MOT file .asset (giong file .fbx).</summary>
    static void SaveMeshes(GameObject root, string name)
    {
        string path = MeshDir + "/" + name + ".asset";
        bool created = false;
        int index = 0;

        var filters = root.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < filters.Length; i++)
        {
            var m = filters[i].sharedMesh;
            if (m == null) continue;

            Mesh saved;
            if (meshMap.TryGetValue(m, out saved))
            {
                filters[i].sharedMesh = saved;
                continue;
            }

            m.name = name + "_" + (index++) + "_" + (string.IsNullOrEmpty(m.name) ? "mesh" : m.name);
            if (!created)
            {
                AssetDatabase.CreateAsset(m, path);
                created = true;
            }
            else
            {
                AssetDatabase.AddObjectToAsset(m, path);
            }
            meshMap[m] = m;
        }

        // Va cham dung chung hinh khoi da luu
        var colliders = root.GetComponentsInChildren<MeshCollider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            var m = colliders[i].sharedMesh;
            Mesh saved;
            if (m != null && meshMap.TryGetValue(m, out saved))
                colliders[i].sharedMesh = saved;
        }
    }

    /// <summary>Doi moi vat lieu chay bang code thanh file .mat that.</summary>
    static void RemapMaterials(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            var psr = r as ParticleSystemRenderer;

            if (psr != null)
            {
                // HE HAT: dat TUNG O MOT, khong bao gio dung mang sharedMaterials.
                //
                // Voi ParticleSystemRenderer, mang do KHONG phai la danh sach o
                // vat lieu binh thuong - Unity nhet ca trailMaterial vao trong
                // no. Gan nguyen mang tro lai thi hai o bi tron len nhau, va cai
                // roi ra ngoai la o CHINH: renderer giu mot o rong, Unity ve ra
                // mau HONG CANH SEN.
                //
                // Ba prefab da dinh dung loi nay - Vfx_NoLua, Vfx_SetChamDat,
                // Skill_QuaCauLua - va no am tu thang 8 den luc bi bat gap trong
                // mot anh chup kiem chung.
                psr.sharedMaterial = BakeMaterial(psr.sharedMaterial);
                if (psr.trailMaterial != null)
                    psr.trailMaterial = BakeMaterial(psr.trailMaterial);
                continue;
            }

            var mats = r.sharedMaterials;
            bool changed = false;

            for (int k = 0; k < mats.Length; k++)
            {
                var baked = BakeMaterial(mats[k]);
                if (baked != mats[k]) { mats[k] = baked; changed = true; }
            }
            if (changed) r.sharedMaterials = mats;
        }
    }

    static Material BakeMaterial(Material src)
    {
        if (src == null) return null;

        Material found;
        if (matMap.TryGetValue(src, out found)) return found;
        if (AssetDatabase.Contains(src)) { matMap[src] = src; return src; }

        // Doi MOI anh trong vat lieu sang file PNG that.
        // Phai duyet tung o anh cua shader chu khong chi lay "_MainTex": mat dat
        // dung toi hai anh (_GrassTex + _DirtTex), con shader lua/bang thi khong
        // co o anh nao ca - hoi thang "_MainTex" se bao loi do.
        var shader = src.shader;
        if (shader != null)
        {
            int n = shader.GetPropertyCount();
            for (int i = 0; i < n; i++)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture)
                    continue;

                string prop = shader.GetPropertyName(i);
                var tex2d = src.GetTexture(prop) as Texture2D;
                if (tex2d == null) continue;

                var baked = BakeTexture(tex2d);
                if (baked != null) src.SetTexture(prop, baked);
            }
        }

        string baseName = string.IsNullOrEmpty(src.name) ? "Mat" : src.name.Replace(" (Instance)", "");
        string path = MatDir + "/M_" + UniqueName(baseName) + ".mat";
        src.name = Path.GetFileNameWithoutExtension(path);

        AssetDatabase.CreateAsset(src, path);
        matMap[src] = src;
        return src;
    }

    static Texture2D BakeTexture(Texture2D src)
    {
        if (src == null) return null;

        Texture2D found;
        if (texMap.TryGetValue(src, out found)) return found;
        if (AssetDatabase.Contains(src)) { texMap[src] = src; return src; }

        byte[] png;
        try { png = src.EncodeToPNG(); }
        catch { return null; }
        if (png == null) return null;

        string baseName = string.IsNullOrEmpty(src.name) ? "Tex" : src.name;
        string path = TexDir + "/" + UniqueName(baseName) + ".png";
        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = src.wrapMode;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 4;
            importer.mipmapEnabled = true;
            importer.alphaIsTransparency = true;

            // Anh PHAP TUYEN phai bao cho Unity biet no la phap tuyen. De nguyen kieu
            // anh mau thi Unity doc sai ba kenh mau va mat da se sang toi lung tung.
            // Quy uoc: ten anh co chu "normal".
            if (baseName.ToLower().Contains("normal"))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.alphaIsTransparency = false;
            }

            importer.SaveAndReimport();
        }

        var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        texMap[src] = asset;
        return asset;
    }

    static string UniqueName(string wanted)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < wanted.Length; i++)
        {
            char c = wanted[i];
            if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
        }
        string name = sb.Length > 0 ? sb.ToString() : "Asset";

        string test = name;
        int n = 2;
        while (usedNames.Contains(test)) test = name + "_" + (n++);
        usedNames.Add(test);
        return test;
    }

    // ================================================================
    //  DUNG SCENE BANG CAC PREFAB VUA NUONG
    // ================================================================

    static void BuildScene(Library lib, string scenePath, string sceneName,
                           int seed, float arenaRadius, int startingCount, float waveDelay)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Random.InitState(seed);
        WorldFactory.BuildSkyAndFog();

        // Bau troi: ghi thanh file .mat that
        if (RenderSettings.skybox != null && !AssetDatabase.Contains(RenderSettings.skybox))
        {
            var sky = RenderSettings.skybox;
            sky.name = "M_BauTroi";
            AssetDatabase.CreateAsset(sky, MatDir + "/M_BauTroi.mat");
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/M_BauTroi.mat");
        }

        var world = new GameObject("World").transform;

        // ---------- MANG LUOI DUONG ----------
        // Phai sinh TRUOC MOI THU: mat dat can no de to lop soi da, con co, cay,
        // da, mo thi can no de tranh ra, khong moc giua long duong.
        var mangDuong = DuongFactory.Dung(arenaRadius, seed);

        // ---------- MAT DAT (Unity Terrain) ----------
        var matDat = BuildTerrainAsset(world, arenaRadius, sceneName);
        ToDuongLenTerrain(matDat.GetComponent<Terrain>(), mangDuong);

        // ---------- THAM CO ----------
        var grass = WorldFactory.BuildGrassField(world, arenaRadius);
        SaveMeshes(grass, "ThamCo_" + sceneName);
        RemapMaterials(grass);

        // ---------- VACH DA BAO QUANH ----------
        var cliffs = new GameObject("VachDa").transform;
        cliffs.SetParent(world, false);
        for (int i = 0; i < 76; i++)
        {
            float a = (float)i / 76f * 360f + Random.Range(-2.2f, 2.2f);
            float r = arenaRadius + Random.Range(-0.6f, 2.4f);
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, r);
            pos.y = Random.Range(-1.2f, 0.4f);

            float sc = Random.Range(2.6f, 5.4f);
            var go = Place(lib.rocks[Random.Range(0, lib.rocks.Count)], cliffs, pos,
                           Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f)),
                           new Vector3(sc * Random.Range(0.85f, 1.30f), sc * Random.Range(1.0f, 1.45f), sc * Random.Range(0.85f, 1.30f)));
            go.name = "VachDa_" + i;
        }

        // ---------- DA, COT, CAY, XUONG ----------
        var props = new GameObject("TrangTri").transform;
        props.SetParent(world, false);

        for (int i = 0; i < 42; i++)
        {
            Vector3 p = ScatterPoint(arenaRadius * 0.92f, 6f, 1.3f);
            float sc = Random.Range(0.75f, 2.2f);
            Place(lib.rocks[Random.Range(0, lib.rocks.Count)], props, p + Vector3.down * sc * 0.35f,
                  Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-20f, 20f)),
                  Vector3.one * sc).name = "DaTang_" + i;
        }

        for (int i = 0; i < 11; i++)
        {
            Vector3 p = ScatterPoint(arenaRadius * 0.85f, 8f, 1.2f);
            Place(lib.pillars[Random.Range(0, lib.pillars.Count)], props, p + Vector3.down * 0.3f,
                  Quaternion.Euler(Random.Range(-7f, 7f), Random.Range(0f, 360f), Random.Range(-7f, 7f)),
                  Vector3.one * Random.Range(0.85f, 1.35f)).name = "CotDa_" + i;
        }

        int demTreo = 0, demTreoCo = 0;
        for (int i = 0; i < 22; i++)
        {
            Vector3 p = ScatterPoint(arenaRadius * 0.9f, 7f, 1.6f);
            bool leafy = Random.value < 0.62f;
            var pool = leafy ? lib.leafyTrees : lib.trees;
            var cay = Place(pool[Random.Range(0, pool.Count)], props, p,
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                            Vector3.one * Random.Range(0.8f, 1.3f));
            cay.name = (leafy ? "CayXanh_" : "CayChet_") + i;

            // 60% so cay co mot bo xuong bi giao dam xuyen nguc, ghim len than.
            // 40% con lai thi treo co mot cai xac lung lang duoi canh - de KHONG
            // cay nao trong tron.
            if (lib.hangingBones.Count > 0 && Random.value < 0.60f)
            {
                TreoXuongLenCay(lib, cay.transform, i);
                demTreo++;
            }
            else if (lib.hangedBones.Count > 0)
            {
                // Cay khong co canh nao vua tam de buoc day thi ghim bang giao,
                // de khong cay nao trong tron
                if (TreoCoLenCanh(lib, cay.transform, i)) demTreoCo++;
                else { TreoXuongLenCay(lib, cay.transform, i); demTreo++; }
            }
        }
        Debug.Log("[AssetBaker] Cay: " + demTreo + " cai bi giao dam ("
                  + Mathf.RoundToInt(demTreo * 100f / 22f) + "%), "
                  + demTreoCo + " cai bi treo co.");

        for (int i = 0; i < 40; i++)
        {
            Vector3 p = ScatterPoint(arenaRadius * 0.93f, 5f, 0.7f);
            Place(lib.bushes[Random.Range(0, lib.bushes.Count)], props, p,
                  Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                  Vector3.one * Random.Range(0.75f, 1.5f)).name = "BuiRam_" + i;
        }

        for (int i = 0; i < 16; i++)
        {
            Vector3 p = ScatterPoint(arenaRadius * 0.9f, 4f);
            Place(lib.bone, props, p + Vector3.up * 0.04f,
                  Quaternion.Euler(90f, Random.Range(0f, 360f), 0f), Vector3.one).name = "Xuong_" + i;
        }

        // ---------- NGHIA DIA ----------
        // Mo duoc gom thanh vai CUM chu khong rai deu khap ban do: mot bai tha ma
        // co dam mo chum vao nhau moi ra khong khi, chu moi noi mot cai thi chi
        // giong do trang tri vat vai.
        BuildNghiaDia(lib, world, arenaRadius);

        // ---------- LEU THAY MO ----------
        DatLeuThayMo(lib, world, arenaRadius);

        // ---------- LO LUA ----------
        var fires = new GameObject("LoLua").transform;
        fires.SetParent(world, false);
        for (int i = 0; i < 3; i++)
        {
            float a = i / 3f * 360f + 18f;

            // Lo lua cung phai tranh long duong: day dan ra xa tam cho toi khi
            // ra khoi duong, van giu nguyen huong de ba cai nam deu ba goc
            Vector3 p = Vector3.zero;
            for (int lan = 0; lan < 24; lan++)
            {
                float r = Random.Range(10f, 15f) + lan * 0.7f;
                p = Quaternion.Euler(0f, a + lan * 3f, 0f) * new Vector3(0f, 0f, r);
                if (!DuongFactory.TranhDuong(p.x, p.z, 1.4f)) break;
            }

            Place(lib.brazier, fires, p, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), Vector3.one)
                .name = "LoLua_" + i;
        }

        // ---------- NHAN VAT ----------
        var player = Place(lib.player, null, new Vector3(0f, 1.2f, 0f), Quaternion.identity, Vector3.one);
        player.name = "Player";
        player.tag = "Player";

        // ---------- CAMERA ----------
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
        var rig = camGo.AddComponent<CameraRig>();
        rig.target = player.transform;
        camGo.AddComponent<SimpleBloom>();
        camGo.transform.position = new Vector3(0f, 9f, -9f);
        camGo.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        // ---------- ANH TRANG ----------
        var lightGo = new GameObject("Moonlight");
        WorldFactory.SetupMoonlight(lightGo.AddComponent<Light>());

        // ---------- BO DIEU KHIEN + THU VIEN PREFAB ----------
        var gameGo = new GameObject("GAME");
        var boot = gameGo.AddComponent<GameBootstrap>();
        boot.arenaRadius = arenaRadius;
        boot.worldSeed = seed;

        var assets = gameGo.AddComponent<GameAssets>();
        assets.playerPrefab = lib.player;
        assets.enemyPrefabs = lib.enemies;
        assets.fireballPrefab = lib.fireball;
        assets.iceStormPrefab = lib.iceStorm;
        assets.iceStormFieldPrefab = lib.iceStormField;
        assets.iceShardPrefab = lib.iceShard;
        assets.lightningStormPrefab = lib.lightningStorm;
        assets.lightningStormFieldPrefab = lib.lightningStormField;
        assets.tornadoPrefab = lib.tornado;
        assets.fireExplosionPrefab = lib.fireExplosion;
        assets.iceImpactPrefab = lib.iceImpact;
        assets.lightningImpactPrefab = lib.lightningImpact;
        assets.stunnedPrefab = lib.stunned;
        assets.burningPrefab = lib.burning;
        assets.castFirePrefab = lib.castFire;
        assets.castIcePrefab = lib.castIce;
        assets.castLightningPrefab = lib.castLightning;
        assets.frozenShatterPrefab = lib.frozenShatter;
        assets.hitBurstPrefab = lib.hitBurst;
        assets.brazierPrefab = lib.brazier;
        assets.rockPrefabs = lib.rocks.ToArray();
        assets.pillarPrefabs = lib.pillars.ToArray();
        assets.treePrefabs = lib.trees.ToArray();
        assets.leafyTreePrefabs = lib.leafyTrees.ToArray();
        assets.bushPrefabs = lib.bushes.ToArray();

        var dirGo = new GameObject("GameDirector");
        var dir = dirGo.AddComponent<GameDirector>();
        dir.player = player.transform;
        dir.arenaRadius = arenaRadius;
        dir.startingCount = startingCount;
        dir.waveDelay = waveDelay;

        var hudGo = new GameObject("HUD");
        var hud = hudGo.AddComponent<GameHUD>();
        hud.player = player.GetComponent<PlayerController>();
        hud.playerHealth = player.GetComponent<Damageable>();
        hud.cameraRig = rig;
        hud.director = dir;

        // ---------- LUU SCENE ----------
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ================================================================
    //  MAN HINH CHINH
    // ================================================================

    static void BuildMenuScene(Library lib)
    {
        // Man hinh chinh gio dung tu CANH ACT2 (nghia dia) voi hai lo da lam
        // trong Blender - xem DungManChinh (menu 51). Ban cu dung mot bai dat
        // nho bang code voi hai lo sat; nguoi dung xin doi (11/09/2026).
        DungManChinh.DungCanh();
    }

    /// <summary>Tat dieu khien de nhan vat chi dung trung bay o man hinh chinh.</summary>
    static void DisableForShowcase(GameObject hero)
    {
        var pc = hero.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
        var cc = hero.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        var dmg = hero.GetComponent<Damageable>();
        if (dmg != null) dmg.enabled = false;
    }

    /// <summary>
    /// Ghim mot bo xuong len than cay.
    ///
    /// Goc cua prefab xuong treo nam ngay CHO GIAO CAM VAO VO CAY, truc +Z huong
    /// ra ngoai. Nen chi can xoay quanh truc dung mot goc bat ky roi day ra bang
    /// ban kinh than cay la xong.
    ///
    /// Than cay co nghieng, nhung o do cao nay (1,5 - 2,1m tren cay cao 6-8m) do
    /// nghieng moi chi vai centimet, ma can giao lai cam sau 0,5m vao trong than,
    /// nen khong can tinh duong tam cay cho phuc tap.
    /// </summary>
    static void TreoXuongLenCay(Library lib, Transform cay, int stt)
    {
        var pf = lib.hangingBones[Random.Range(0, lib.hangingBones.Count)];
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf);
        go.name = "XuongTreo_" + stt;
        go.transform.SetParent(cay, false);

        float goc = Random.Range(0f, 360f);
        float caoTreo = Random.Range(1.55f, 2.15f);
        float raNgoai = Random.Range(0.06f, 0.12f);   // xap xi ban kinh than cay o do cao do

        go.transform.localRotation = Quaternion.Euler(0f, goc, 0f);
        go.transform.localPosition = Quaternion.Euler(0f, goc, 0f) * new Vector3(0f, 0f, raNgoai)
                                   + Vector3.up * caoTreo;
        go.transform.localScale = Vector3.one * Random.Range(0.92f, 1.08f);
    }

    /// Buoc mot cai xac treo co vao CANH cay.
    ///
    /// PHAI lay mot diem NAM THAT TREN lop vo canh. Lan truoc toi lay hop bao
    /// (bounds) cua canh roi ghep: toa do ngang = giua hop, do cao = day hop.
    /// Voi canh choc xien len cao thi day hop nam o CHAN canh, con giua hop lai
    /// o ngoai xa - ghep hai thu do ra mot diem treo lo lung giua troi, duoi canh.
    ///
    /// Gio duyet thang tung DINH cua luoi canh: chi xet khuc giua-ngoai cua canh,
    /// trong do chon dinh thap nhat, tuc mat duoi canh. Diem do chac chan dinh vo.
    ///
    /// Tra ve false neu cay khong co canh nao vua tam (cay non, canh cut) - luc do
    /// nguoi goi se ghim cho no mot bo xuong bang giao thay vi treo co.
    /// </summary>
    static bool TreoCoLenCanh(Library lib, Transform cay, int stt)
    {
        // Chieu cao ma cai xac chiem: soi day + than nguoi
        const float caoXac = 2.05f;

        var ungVien = new List<Vector3>();

        foreach (var mf in cay.GetComponentsInChildren<MeshFilter>())
        {
            if (!mf.name.StartsWith("Branch")) continue;
            var luoi = mf.sharedMesh;
            if (luoi == null) continue;

            var dinh = luoi.vertices;
            var tf = mf.transform;
            Vector2 chanCanh = new Vector2(tf.position.x, tf.position.z);

            // Canh voi ra xa duoc bao nhieu
            float xaNhat = 0f;
            for (int i = 0; i < dinh.Length; i++)
            {
                Vector3 w = tf.TransformPoint(dinh[i]);
                float d = Vector2.Distance(new Vector2(w.x, w.z), chanCanh);
                if (d > xaNhat) xaNhat = d;
            }
            if (xaNhat < 0.30f) continue;               // canh cut ngun, khong treo duoc

            // Trong khuc giua-ngoai, lay dinh THAP NHAT = mat duoi canh
            Vector3 tot = Vector3.zero;
            float thapNhat = float.MaxValue;
            bool co = false;

            for (int i = 0; i < dinh.Length; i++)
            {
                Vector3 w = tf.TransformPoint(dinh[i]);
                float d = Vector2.Distance(new Vector2(w.x, w.z), chanCanh);
                if (d < xaNhat * 0.45f || d > xaNhat * 0.88f) continue;
                if (w.y < thapNhat) { thapNhat = w.y; tot = w; co = true; }
            }
            if (!co) continue;

            float caoSoVoiGoc = tot.y - cay.position.y;
            if (caoSoVoiGoc < caoXac + 0.35f) continue;  // treo vao thi chan cham dat
            if (caoSoVoiGoc > 5.5f) continue;            // tit tren ngon, nhin khong ra

            ungVien.Add(tot);
        }

        if (ungVien.Count == 0) return false;

        Vector3 diemBuoc = ungVien[Random.Range(0, ungVien.Count)];

        var pf = lib.hangedBones[Random.Range(0, lib.hangedBones.Count)];
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf);
        go.name = "XuongTreoCo_" + stt;
        go.transform.SetParent(cay, true);          // giu nguyen toa do the gioi
        go.transform.position = diemBuoc;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.transform.localScale = Vector3.one * Random.Range(0.92f, 1.06f);
        return true;
    }

    /// <summary>
    /// Rai mo thanh vai cum nghia dia nho.
    ///
    /// Trong moi cum, cac mo deu QUAY CUNG MOT HUONG (lech nhau chut it) - dung
    /// nhu nghia trang that, vi nguoi ta chon theo hang. Quay lung tung thi nhin
    /// ra bai da vun chu khong ra khu mo.
    /// </summary>
    /// <summary>
    /// DAT BON CAN LEU THAY MO ra ban do.
    ///
    /// Leu la vat to (chan rong hon 3 m) nen phai chua duong that xa: chua 2.6 m
    /// tinh tu mep long duong. Chua it hon thi cai chan giao se thoc ra giua loi
    /// di, nguoi choi chay qua se dam vao.
    ///
    /// Bon can cung phai dung XA NHAU - moi can mot goc ban do - chu chum lai
    /// thanh mot cai lang thi lai ra chuyen khac.
    /// </summary>
    static void DatLeuThayMo(Library lib, Transform world, float arenaRadius)
    {
        if (lib.tents.Count == 0) return;

        var goc = new GameObject("LeuThayMo").transform;
        goc.SetParent(world, false);

        const float ChuaDuong = 2.6f;      // chua ra hai ben mep duong, met
        const float CachNhau  = 13f;       // hai can leu phai cach nhau it nhat bay nhieu

        var daDat = new List<Vector3>();
        float gocDau = Random.Range(0f, 360f);

        for (int i = 0; i < 4; i++)
        {
            // Moi can ngam mot cung troi rieng, nen bon can toa deu quanh san
            float huongUuTien = gocDau + i * 90f;
            Vector3 cho = Vector3.zero;
            bool tim = false;

            for (int lan = 0; lan < 120; lan++)
            {
                float a = huongUuTien + Random.Range(-42f, 42f);
                float r = arenaRadius * Random.Range(0.40f, 0.82f);
                Vector3 thu = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, r);

                if (DuongFactory.TranhDuong(thu.x, thu.z, ChuaDuong)) continue;

                bool quaGan = false;
                for (int k = 0; k < daDat.Count; k++)
                    if (Vector3.Distance(thu, daDat[k]) < CachNhau) { quaGan = true; break; }
                if (quaGan) continue;

                cho = thu; tim = true; break;
            }

            if (!tim)
            {
                Debug.LogWarning("[AssetBaker] Khong tim duoc cho trong cho can leu thu "
                                 + (i + 1) + " - bo qua can nay.");
                continue;
            }

            daDat.Add(cho);
            cho.y = VfxFactory.GroundY(cho) - 0.05f;   // lun chan xuong dat mot chut

            var pf = lib.tents[i % lib.tents.Count];
            Place(pf, goc, cho, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                  Vector3.one * Random.Range(0.94f, 1.08f)).name = "Leu_" + (i + 1);
        }

        Debug.Log("[AssetBaker] Da dat " + daDat.Count + " can leu thay mo, deu tranh duong "
                  + ChuaDuong + " m.");
    }

    static void BuildNghiaDia(Library lib, Transform world, float arenaRadius)
    {
        if (lib.graves.Count == 0) return;

        var nghiaDia = new GameObject("NghiaDia").transform;
        nghiaDia.SetParent(world, false);

        const int soCum = 4;
        int dem = 0;

        for (int c = 0; c < soCum; c++)
        {
            Vector3 tamCum = ScatterPoint(arenaRadius * 0.80f, 11f);
            float huongCum = Random.Range(0f, 360f);
            int soMo = Random.Range(3, 6);

            for (int i = 0; i < soMo; i++)
            {
                // Xep thanh hang thua, hoi so le
                float hang = i / 2;
                float cot = (i % 2) * 2f - 0.5f;
                Vector3 lech = Quaternion.Euler(0f, huongCum, 0f)
                             * new Vector3(cot * Random.Range(1.5f, 2.2f),
                                           0f,
                                           hang * Random.Range(1.9f, 2.6f));
                lech += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

                Vector3 p = tamCum + lech;

                // Mo bi lech vao long duong thi day no ra
                for (int lan = 0; lan < 8 && DuongFactory.TranhDuong(p.x, p.z, 1.5f); lan++)
                {
                    Vector2 raNgoai = new Vector2(p.x - tamCum.x, p.z - tamCum.z);
                    if (raNgoai.sqrMagnitude < 0.01f) raNgoai = Random.insideUnitCircle.normalized;
                    raNgoai = raNgoai.normalized * 0.9f;
                    p += new Vector3(raNgoai.x, 0f, raNgoai.y);
                }

                p.y = VfxFactory.GroundY(p);

                var pf = lib.graves[Random.Range(0, lib.graves.Count)];
                var go = Place(pf, nghiaDia, p + Vector3.down * 0.06f,
                               Quaternion.Euler(0f, huongCum + Random.Range(-16f, 16f), 0f),
                               Vector3.one * Random.Range(0.88f, 1.20f));
                go.name = "Mo_" + dem;
                dem++;
            }
        }

        // ---- NAM HOM MO DA rai rieng khap ban do ----
        // Kieu 3 (QuanTaiDa) la cai hom da nap he ra lo bo xuong ben trong.
        // Dat rieng nam cai nua, tach khoi cac cum tren, de di dau tren ban do
        // cung gap duoc mot cai.
        const int kieuQuanTai = 3;
        if (lib.graves.Count > kieuQuanTai)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = ScatterPoint(arenaRadius * 0.86f, 9f, 1.6f);

                var go = Place(lib.graves[kieuQuanTai], nghiaDia, p + Vector3.down * 0.06f,
                               Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                               Vector3.one * Random.Range(0.95f, 1.18f));
                go.name = "HomMo_" + i;
                dem++;
            }
        }

        Debug.Log("[AssetBaker] Da dat " + dem + " ngoi mo (" + soCum
                  + " cum nghia dia + 5 hom mo da rai rieng).");
    }

    // ================================================================
    //  MAT DAT: NUONG THANH UNITY TERRAIN
    // ================================================================

    /// <summary>
    /// Xoa het dia hinh dang co roi sinh lai tu dau bang ham toan.
    /// Dung khi ban to hong va muon lam lai, HOAC khi vua sua ham
    /// <see cref="WorldFactory.AnalyticHeight"/> trong code.
    /// </summary>
    [MenuItem("Diablo 2.5D/8. Dung lai dia hinh (xoa net to tay)", false, 60)]
    public static void RebuildTerrain()
    {
        if (!EditorUtility.DisplayDialog(
                "Dung lai dia hinh",
                "Toan bo net to tay tren mat dat se bi xoa " +
                "va sinh lai tu dau bang cong thuc. Tiep tuc?",
                "Lam ngay", "Huy"))
            return;

        var guids = AssetDatabase.FindAssets("t:TerrainData", new[] { TerrainDir });
        for (int i = 0; i < guids.Length; i++)
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guids[i]));
        AssetDatabase.Refresh();

        BakeSilent();
        EditorUtility.DisplayDialog("Xong!", "Da dung lai dia hinh va man choi.", "OK");
    }


    /// <summary>
    /// TO MANG DUONG len mat dat bang lop vat lieu SOI DA.
    ///
    /// Chi dong vao nhung o nam tren duong, cac cho khac giu nguyen - ke ca net
    /// ban tu to tay truoc do. Long duong la soi da dac, ra toi mep thi nhat dan
    /// de an vao co xung quanh chu khong cat mot duong vien sac le.
    /// </summary>
    static void ToDuongLenTerrain(Terrain terrain, List<DuongFactory.Doan> mang)
    {
        if (terrain == null || mang == null || mang.Count == 0) return;

        var td = terrain.terrainData;
        int res = td.alphamapResolution;
        int soLop = td.terrainLayers.Length;
        if (soLop < 4) return;

        var alpha = td.GetAlphamaps(0, 0, res, res);
        float be = td.size.x;
        int demO = 0;

        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
            {
                float px = ((float)x / (res - 1) - 0.5f) * be;
                float pz = ((float)z / (res - 1) - 0.5f) * be;

                float beRong;
                float d = DuongFactory.KhoangCach(mang, px, pz, out beRong);

                float nua = beRong * 0.5f;
                if (d > nua + DuongFactory.VienMo) continue;      // ngoai vung anh huong

                // 1 o giua long duong, giam dan ve 0 o ria vien mo
                float k = 1f - Mathf.Clamp01((d - nua) / DuongFactory.VienMo);
                k = Mathf.SmoothStep(0f, 1f, k);

                // Mep duong hoi lam nham cho khoi thang bang nhu ke
                float nham = Mathf.PerlinNoise(px * 0.85f + 40f, pz * 0.85f + 17f);
                k = Mathf.Clamp01(k * Mathf.Lerp(0.78f, 1.12f, nham));
                if (k < 0.02f) continue;

                // Long duong la soi da; cac lop cu bi day lui theo dung ti le k
                float con = 1f - k;
                alpha[z, x, TerrainFactory.LayerCo] *= con;
                alpha[z, x, TerrainFactory.LayerDatKho] *= con;
                alpha[z, x, TerrainFactory.LayerBun] *= con;
                alpha[z, x, TerrainFactory.LayerSoiDa] =
                    alpha[z, x, TerrainFactory.LayerSoiDa] * con + k;

                demO++;
            }

        td.SetAlphamaps(0, 0, alpha);
        EditorUtility.SetDirty(td);

        Debug.Log("[AssetBaker] Da to " + mang.Count + " doan duong len mat dat ("
                  + demO + " o luoi, " + Mathf.RoundToInt(demO * 100f / (res * res)) + "% ban do).");
    }

    /// <summary>
    /// Ghi bon lop vat lieu (.terrainlayer) mot lan, dung chung cho moi man choi.
    /// </summary>
    /// <summary>
    /// Ghi anh cua mot lop vat lieu thanh file .png NGAY TRONG thu muc Terrain.
    ///
    /// Co y KHONG dung chung thu muc Textures: thu muc do bi xoa sach moi lan nuong,
    /// ma cac file .terrainlayer thi duoc giu lai - de chung o hai noi khac nhau
    /// thi sau mot lan nuong lop vat lieu se mat anh va mat dat hoa ra ban co.
    /// </summary>
    static Texture2D BakeTerrainTexture(Texture2D src, string name)
    {
        if (src == null) return null;

        string path = TerrainDir + "/Anh_" + name + ".png";

        // GHI DE moi lan, khong dung lai file cu. Neu dung lai thi luc sua mau
        // lop vat lieu trong code se khong an gi ca - rat de tuong nham la code
        // sai, trong khi that ra chi la anh cu chua bi thay.
        byte[] png = src.EncodeToPNG();
        if (png == null) return null;
        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.filterMode = FilterMode.Bilinear;
            imp.anisoLevel = 8;
            imp.mipmapEnabled = true;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static TerrainLayer[] EnsureTerrainLayers()
    {
        if (terrainLayers != null) return terrainLayers;

        var layers = TerrainFactory.MakeLayers();
        for (int i = 0; i < layers.Length; i++)
        {
            string ten = layers[i].name;
            string path = TerrainDir + "/" + ten + ".terrainlayer";
            var anh = BakeTerrainTexture(layers[i].diffuseTexture, ten);

            // Da co file lop nay roi thi GIU NGUYEN FILE (de moi tham chieu tu
            // TerrainData khong bi dut) nhung CAP NHAT NOI DUNG theo code.
            // Giu nguyen ca noi dung thi sua mau trong code se khong an gi.
            var cu = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (cu != null)
            {
                cu.diffuseTexture = anh;
                cu.tileSize = layers[i].tileSize;
                cu.specular = layers[i].specular;
                cu.smoothness = layers[i].smoothness;
                EditorUtility.SetDirty(cu);

                Object.DestroyImmediate(layers[i]);
                layers[i] = cu;
                continue;
            }

            layers[i].diffuseTexture = anh;
            AssetDatabase.CreateAsset(layers[i], path);
            layers[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        }

        AssetDatabase.SaveAssets();
        terrainLayers = layers;
        return terrainLayers;
    }

    /// <summary>
    /// Ghi TerrainData thanh file roi dat tam terrain vao scene.
    ///
    /// File nam o <c>Assets/Terrain/DiaHinh_&lt;man&gt;.asset</c>. Bam vao doi tuong
    /// "MatDat" trong scene la sua duoc bang cong cu to cua Unity.
    /// </summary>
    static GameObject BuildTerrainAsset(Transform world, float arenaRadius, string sceneName)
    {
        var layers = EnsureTerrainLayers();
        string path = TerrainDir + "/DiaHinh_" + sceneName + ".asset";

        // DA CO dia hinh roi thi DUNG LAI - khong sinh de len net to tay cua ban
        var data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
        if (data == null)
        {
            data = TerrainFactory.Make(arenaRadius, layers);
            AssetDatabase.CreateAsset(data, path);
            data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
        }

        var go = TerrainFactory.Build(world, arenaRadius, data);

        // Cay coi, da, bui ram duoc dat bang cach ban tia xuong mat dat.
        // Phai bao he vat ly biet tam terrain vua xuat hien, khong thi tia ban truot.
        Physics.SyncTransforms();
        return go;
    }

    static GameObject Place(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = rot;
        go.transform.localScale = scale;
        return go;
    }

    /// <summary>
    /// Diem ngau nhien tren mat dat, khong qua gan giua san VA KHONG NAM TREN DUONG.
    ///
    /// <paramref name="chuaRa"/> la khoang chua them ra hai ben mep duong: cay to
    /// thi can chua nhieu, hon da nho thi it thoi. Thu 60 lan van khong ra cho
    /// trong thi danh chiu, tra ve diem cuoi cung - tha lech mot cai con hon
    /// khong dat duoc gi.
    /// </summary>
    static Vector3 ScatterPoint(float maxRadius, float minRadius, float chuaRa = 0.9f)
    {
        Vector3 cuoi = new Vector3(minRadius, 0f, 0f);

        for (int i = 0; i < 60; i++)
        {
            Vector2 c = Random.insideUnitCircle * maxRadius;
            if (c.magnitude < minRadius) continue;

            Vector3 p = new Vector3(c.x, 0f, c.y);
            cuoi = p;
            if (DuongFactory.TranhDuong(p.x, p.z, chuaRa)) continue;   // dung tren duong

            p.y = VfxFactory.GroundY(p);
            return p;
        }

        cuoi.y = VfxFactory.GroundY(cuoi);
        return cuoi;
    }
}
