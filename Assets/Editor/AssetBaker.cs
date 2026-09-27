using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CONG CU "NUONG" PREFAB: ghi mot vat the sinh bang code (quai tu model Meshy, Loc xoay) thanh FILE THAT
/// trong Project - hinh khoi vao Assets/Models, vat lieu Assets/Materials, anh Assets/Textures, prefab
/// Assets/Prefabs.
///
/// Con ba muc: 11 Quy du, 12 Quy cay, 13 Loc xoay. Muc 1 (nuong TAT CA + dung lai canh Act1 va man chinh
/// kieu cu) va muc 8 (dung lai dia hinh Act1) da bo cung Act1 (27/09/2026, nguoi dung xoa han Act1).
/// </summary>
public static class AssetBaker
{
    const string TexDir = "Assets/Textures";
    const string MatDir = "Assets/Materials";
    const string MeshDir = "Assets/Models";
    const string PrefabDir = "Assets/Prefabs";

    static Dictionary<Texture2D, Texture2D> texMap;
    static Dictionary<Material, Material> matMap;
    static Dictionary<Mesh, Mesh> meshMap;
    static HashSet<string> usedNames;

    static void EnsureFolders()
    {
        string[] dirs = { TexDir, MatDir, MeshDir, PrefabDir };
        for (int i = 0; i < dirs.Length; i++)
            if (!AssetDatabase.IsValidFolder(dirs[i]))
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(dirs[i]));
    }


    // ================================================================
    //  MU PHU THUY (model san dua tu Meshy vao)
    // ================================================================

    /// <summary>Thu muc chua model dua tu Meshy vao. KHONG bi xoa luc nuong lai.</summary>
    const string MeshyDir = "Assets/MeshyImports";

    /// <summary>
    /// Nuong RIENG prefab Quy du, khong dung den phan con lai cua du an.
    /// </summary>
    [MenuItem("Diablo 2.5D/11. Nuong rieng prefab Quy du", false, 92)]
    public static void NuongRiengQuyDu()
    {
        // PHAI khoi tao ba bo dem nay truoc.
        //
        // Khong khoi tao thi BakeMaterial nem NullReferenceException ngay
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
    /// Khoi tao ba bo dem cho mot lan nuong.
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
            // Luoi DA LA ASSET (vd lay tu FBX Blender - Loc xoay 25/09/2026): prefab tro thang vao do. CreateAsset tren
            // no se bao loi va con doi ten sub-asset cua FBX.
            if (EditorUtility.IsPersistent(m)) continue;

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
}
