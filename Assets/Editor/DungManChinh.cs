using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// DUNG MAN HINH CHINH TU CANH ACT2.
///
/// Nguoi dung xin (11/09/2026): nen phia sau man dang nhap / sanh dung CANH
/// QUAN CUA ACT2 (nghia dia), van hai lo lua hai ben, nhung lo lam lai bang da
/// that (dung trong Blender, be mat san sui, vet nut tham mau) va ngon lua that
/// hon (lua cu la hat anh tinh, nhin ra tung tam giac).
///
/// Cach lam:
///   1. Mo Act2, doc anh sang / suong / bau troi cua no.
///   2. Tao canh moi, chep sang PHAN canh quanh cho dung (dia hinh, hang rao,
///      nha mo, bia, da, cay, nuoc, co) - chi nhung gi trong vong
///      <see cref="BanKinhGiu"/> met va nam phia truoc camera. Chep ca Act2 thi
///      man dau tien cua game nang them ~700 vat the ma khong ai nhin thay.
///   3. Dat phu thuy, camera, hai lo da (prefab <c>Assets/Models/LoLuaDa</c>).
///   4. Bo vat nao chan giua camera va nhan vat, hoac de len cho lo lua.
///   5. Luu de len MainMenu.unity.
///
/// Cho dung chon bang menu 51b (chup thu cac goc): truoc nha mo MAUS_A_682,
/// nhin theo huong +x.
/// </summary>
public static class DungManChinh
{
    const string CanhAct2 = "Assets/Scenes/Act2.unity";
    const string CanhMenu = "Assets/Scenes/MainMenu.unity";
    public const string ThuMucLo = "Assets/Models/LoLuaDa";
    const string PrefabNhanVat = "Assets/Prefabs/Player_Sorceress.prefab";

    public static readonly Vector3 ChoDung = new Vector3(2.5f, 0f, -16.0f);   // y lay tu dia hinh
    public const float HuongNhin = 90f;
    const float BanKinhGiu = 45f;

    [MenuItem("Diablo 2.5D/51. Dung man chinh tu canh Act2", false, 138)]
    public static void Chay()
    {
        var hienTai = EditorSceneManager.GetActiveScene();
        if (hienTai.isDirty)
        {
            EditorUtility.DisplayDialog("Dung man chinh", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        var bao = DungCanh();
        File.WriteAllText("PlayTestShots/dungmanchinh.txt", bao);
        Debug.Log("[DungManChinh]\n" + bao);
    }

    /// <summary>Dung va luu MainMenu.unity. Tra ve bao cao.</summary>
    public static string DungCanh()
    {
        var bao = new StringBuilder();
        var prefabLo = TaoPrefabLo(bao);

        // ---- 1. Act2 ----
        var act2 = EditorSceneManager.OpenScene(CanhAct2, OpenSceneMode.Single);
        var anhSang = DocAnhSang();
        GameObject worldAct2 = null, coAct2 = null, trangAct2 = null;
        foreach (var g in act2.GetRootGameObjects())
        {
            if (g.name == "World") worldAct2 = g;
            if (g.name == "BuiCoRai") coAct2 = g;
            if (g.name == "Moonlight") trangAct2 = g;
        }

        // ---- 2. Canh moi ----
        var moi = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(moi);

        float yDung = ChupGocManChinh.MatDat(ChoDung.x, ChoDung.z);
        var cho = new Vector3(ChoDung.x, yDung, ChoDung.z);
        var q = Quaternion.Euler(0f, HuongNhin, 0f);
        Vector3 truoc = q * Vector3.forward, phai = q * Vector3.right;
        Vector3 viTriCam = cho - truoc * 4.2f + Vector3.up * 1.5f;

        var world = new GameObject("World");
        SceneManager.MoveGameObjectToScene(world, moi);
        int chep = 0, bo = 0;
        var daChep = new List<GameObject>();
        foreach (Transform nhom in worldAct2.transform)
        {
            var nhomMoi = new GameObject(nhom.name).transform;
            nhomMoi.SetParent(world.transform, false);
            nhomMoi.localPosition = nhom.localPosition; nhomMoi.localRotation = nhom.localRotation; nhomMoi.localScale = nhom.localScale;

            // Dia hinh va hang rao la mot khoi lon - chep nguyen
            bool chepNguyen = nhom.GetComponent<Terrain>() != null || nhom.name == "HangRao";
            if (nhom.childCount == 0 || chepNguyen)
            {
                if (nhom.GetComponents<Component>().Length > 1)
                {
                    Object.DestroyImmediate(nhomMoi.gameObject);
                    var ban = Object.Instantiate(nhom.gameObject, world.transform);
                    ban.name = nhom.name;
                    ban.transform.SetPositionAndRotation(nhom.position, nhom.rotation);
                    chep++;
                }
                continue;
            }
            foreach (Transform con in nhom)
            {
                if (!NenGiu(con, cho, viTriCam, truoc)) { bo++; continue; }
                var ban = Object.Instantiate(con.gameObject, nhomMoi);
                ban.name = con.name;
                ban.transform.SetPositionAndRotation(con.position, con.rotation);
                ban.transform.localScale = con.localScale;
                daChep.Add(ban);
                chep++;
            }
        }
        if (coAct2 != null)
        {
            var co = new GameObject("BuiCoRai").transform;
            SceneManager.MoveGameObjectToScene(co.gameObject, moi);
            foreach (Transform con in coAct2.transform)
            {
                if (!NenGiu(con, cho, viTriCam, truoc)) { bo++; continue; }
                var ban = Object.Instantiate(con.gameObject, co);
                ban.name = con.name;
                ban.transform.SetPositionAndRotation(con.position, con.rotation);
                daChep.Add(ban);
                chep++;
            }
        }
        bao.AppendLine("chep tu Act2: " + chep + " vat, bo " + bo + " vat (ngoai " + BanKinhGiu + " m hoac sau lung camera)");

        // Anh sang: den trang cua Act2 + suong, bau troi
        if (trangAct2 != null)
        {
            var trang = Object.Instantiate(trangAct2);
            trang.name = "Moonlight";
            SceneManager.MoveGameObjectToScene(trang, moi);
            anhSang.sun = trang.GetComponent<Light>();
        }
        GanAnhSang(anhSang);
        EditorSceneManager.CloseScene(act2, true);

        // ---- 3. Nhan vat, camera, lo lua ----
        var pfNv = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabNhanVat);
        var nv = (GameObject)PrefabUtility.InstantiatePrefab(pfNv, moi);
        nv.name = "PhuThuy_TrungBay";
        nv.tag = "Untagged";
        nv.transform.SetPositionAndRotation(cho + Vector3.up * 0.2f, Quaternion.Euler(0f, HuongNhin + 200f, 0f));
        TatDieuKhien(nv);

        var camGo = new GameObject("Main Camera");
        SceneManager.MoveGameObjectToScene(camGo, moi);
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.nearClipPlane = 0.1f; cam.farClipPlane = 300f;
        cam.allowHDR = true; cam.fieldOfView = 42f;
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<SimpleBloom>();
        camGo.transform.SetPositionAndRotation(viTriCam, Quaternion.Euler(6f, HuongNhin, 0f));

        var cacLo = new List<Vector3>();
        int stt = 0;
        foreach (float ben in new[] { -1f, 1f })
        {
            var p = cho + phai * (2.2f * ben) + truoc * 1.4f;
            p.y = ChupGocManChinh.MatDat(p.x, p.z);
            var lo = (GameObject)PrefabUtility.InstantiatePrefab(prefabLo, moi);
            lo.name = ben < 0 ? "LoLua_Trai" : "LoLua_Phai";
            lo.transform.SetPositionAndRotation(p, Quaternion.Euler(0f, HuongNhin + 37f * (stt++ * 2 - 1), 0f));
            lo.transform.SetParent(world.transform, true);
            cacLo.Add(p);
        }

        // ---- 4. Don vat vuong ----
        int donCho = 0;
        var diemTrong = new List<Vector3>(cacLo) { cho };
        var ray = new Ray(viTriCam, (cho + Vector3.up * 1.2f - viTriCam).normalized);
        float xaNv = Vector3.Distance(viTriCam, cho) - 0.5f;
        foreach (var go in daChep)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) continue;
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            bool vuong = false;
            foreach (var d in diemTrong)
                if (b.SqrDistance(d + Vector3.up * 0.8f) < 0.9f * 0.9f) vuong = true;
            // Chan tam nhin camera -> nhan vat (nguc). Cay cao thi xet than
            // cay chu khong xet ca tan la - hop bao tan la rong gap may lan than.
            float kc;
            if (b.size.y < 8f && b.IntersectRay(ray, out kc) && kc < xaNv) vuong = true;
            if (vuong) { bao.AppendLine("   bo vat vuong: " + go.name); Object.DestroyImmediate(go); donCho++; }
        }
        bao.AppendLine("bo " + donCho + " vat vuong cho nhan vat / lo lua / tam nhin camera");

        var menuGo = new GameObject("MENU");
        SceneManager.MoveGameObjectToScene(menuGo, moi);
        var menu = menuGo.AddComponent<MainMenuUI>();
        menu.showcase = nv.transform;

        // ---- 5. Luu ----
        EditorSceneManager.MarkSceneDirty(moi);
        EditorSceneManager.SaveScene(moi, CanhMenu);
        EditorSceneManager.OpenScene(CanhMenu, OpenSceneMode.Single);

        int soVat = 0;
        foreach (var g in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            soVat += g.GetComponentsInChildren<Transform>(true).Length;
        bao.AppendLine("cho dung " + cho.ToString("F2") + ", camera " + viTriCam.ToString("F2") + ", huong " + HuongNhin);
        bao.AppendLine("MainMenu.unity: " + soVat + " vat the");
        return bao.ToString();
    }

    static bool NenGiu(Transform t, Vector3 cho, Vector3 cam, Vector3 truoc)
    {
        var p = t.position;
        var d = new Vector2(p.x - cho.x, p.z - cho.z).magnitude;
        if (d > BanKinhGiu) return false;
        // Sau lung camera qua 4 m thi khong bao gio thay
        return Vector3.Dot(p - cam, truoc) > -4f;
    }

    static void TatDieuKhien(GameObject nv)
    {
        var pc = nv.GetComponent<PlayerController>(); if (pc != null) pc.enabled = false;
        var cc = nv.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
        var dmg = nv.GetComponent<Damageable>(); if (dmg != null) dmg.enabled = false;
    }

    // ------------------------------------------------------------------ anh sang
    class AnhSang
    {
        public bool fog; public Color fogColor; public FogMode fogMode; public float fogDensity, fogStart, fogEnd;
        public UnityEngine.Rendering.AmbientMode ambientMode;
        public Color sky, equator, ground; public float ambientIntensity;
        public Material skybox; public Light sun;
    }

    static AnhSang DocAnhSang()
    {
        return new AnhSang
        {
            fog = RenderSettings.fog, fogColor = RenderSettings.fogColor, fogMode = RenderSettings.fogMode,
            fogDensity = RenderSettings.fogDensity, fogStart = RenderSettings.fogStartDistance, fogEnd = RenderSettings.fogEndDistance,
            ambientMode = RenderSettings.ambientMode, sky = RenderSettings.ambientSkyColor,
            equator = RenderSettings.ambientEquatorColor, ground = RenderSettings.ambientGroundColor,
            ambientIntensity = RenderSettings.ambientIntensity,
            // Bau troi cua Act2 la vat lieu nam TRONG scene - phai nhan ban thi scene moi moi giu duoc
            skybox = RenderSettings.skybox != null ? new Material(RenderSettings.skybox) : null,
        };
    }

    static void GanAnhSang(AnhSang a)
    {
        RenderSettings.fog = a.fog; RenderSettings.fogColor = a.fogColor; RenderSettings.fogMode = a.fogMode;
        RenderSettings.fogDensity = a.fogDensity; RenderSettings.fogStartDistance = a.fogStart; RenderSettings.fogEndDistance = a.fogEnd;
        RenderSettings.ambientMode = a.ambientMode; RenderSettings.ambientSkyColor = a.sky;
        RenderSettings.ambientEquatorColor = a.equator; RenderSettings.ambientGroundColor = a.ground;
        RenderSettings.ambientIntensity = a.ambientIntensity;
        if (a.skybox != null) { a.skybox.name = "BauTroi_Act2"; RenderSettings.skybox = a.skybox; }
        if (a.sun != null) RenderSettings.sun = a.sun;
    }

    // ------------------------------------------------------------------ prefab lo da
    /// <summary>
    /// Cai dat nhap cho FBX + texture tu Blender, tao vat lieu va prefab lo da.
    /// Chay lai bao nhieu lan cung duoc - ghi de len cai cu.
    /// </summary>
    public static GameObject TaoPrefabLo(StringBuilder bao)
    {
        string fbx = ThuMucLo + "/LoLuaDa.fbx";
        var mi = AssetImporter.GetAtPath(fbx) as ModelImporter;
        if (mi == null) throw new System.Exception("Chua co " + fbx + " - chay script Blender truoc");
        mi.globalScale = 1f;
        mi.materialImportMode = ModelImporterMaterialImportMode.None;
        mi.importNormals = ModelImporterNormals.Import;
        mi.importTangents = ModelImporterTangents.CalculateMikk;
        mi.importAnimation = false;
        mi.isReadable = false;
        mi.SaveAndReimport();

        Texture2D Tex(string ten, bool mauSac, bool phap)
        {
            string duong = ThuMucLo + "/" + ten + ".png";
            var ti = AssetImporter.GetAtPath(duong) as TextureImporter;
            ti.textureType = phap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.sRGBTexture = mauSac;
            ti.maxTextureSize = 1024;
            ti.mipmapEnabled = true;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(duong);
        }

        string duongVl = ThuMucLo + "/LoLuaDa_Da.mat";
        var vl = AssetDatabase.LoadAssetAtPath<Material>(duongVl);
        if (vl == null) { vl = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(vl, duongVl); }
        vl.shader = Shader.Find("Standard");
        vl.SetTexture("_MainTex", Tex("LoLuaDa_Albedo", true, false));
        vl.SetTexture("_BumpMap", Tex("LoLuaDa_Normal", false, true));
        vl.SetFloat("_BumpScale", 1.0f);
        vl.SetTexture("_MetallicGlossMap", Tex("LoLuaDa_KimLoaiBong", false, false));
        vl.SetFloat("_GlossMapScale", 1f);
        vl.EnableKeyword("_NORMALMAP");
        vl.EnableKeyword("_METALLICGLOSSMAP");
        EditorUtility.SetDirty(vl);

        var than = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Coals.mat");

        var mo = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        var goc = new GameObject("LoLuaDa");
        var ban = (GameObject)PrefabUtility.InstantiatePrefab(mo);
        ban.transform.SetParent(goc.transform, false);
        foreach (var r in ban.GetComponentsInChildren<MeshRenderer>())
            r.sharedMaterial = r.name.Contains("Than") ? than : vl;
        goc.AddComponent<LoLuaDa>();
        var b = new Bounds(goc.transform.position, Vector3.zero);
        foreach (var r in goc.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);

        var pf = PrefabUtility.SaveAsPrefabAsset(goc, ThuMucLo + "/LoLuaDa.prefab");
        Object.DestroyImmediate(goc);
        AssetDatabase.SaveAssets();

        int tg = 0;
        foreach (var mf in mo.GetComponentsInChildren<MeshFilter>()) tg += mf.sharedMesh.triangles.Length / 3;
        bao.AppendLine(string.Format("prefab lo da: {0} tam giac, cao {1:F2} m, rong {2:F2} m", tg, b.size.y, b.size.x));
        return pf;
    }
}
