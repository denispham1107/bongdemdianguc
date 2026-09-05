using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// DUNG MAN CHOI ACT2 TU BAN DO VE TAY TRONG BLENDER.
///
/// Khac han Act1: Act1 duoc sinh hoan toan bang code (dia hinh bang ham toan,
/// cay da mo rai ngau nhien theo seed). Act2 thi LAY NGUYEN ban do nguoi dung
/// dung tay trong Blender - tung ngoi mo, tung goc cay deu dung cho nguoi ta da
/// dat, khong duoc xe dich.
///
/// BAN DO DUOC DUA VAO QUA HAI FILE:
///   map_luoi.fbx  - 27 luoi GOC, moi luoi dung mot lan
///   map_vitri.fbx - 746 cai "moc" rong, moi cai la mot cho dat do
///
/// Tach lam hai la co chu y. Xuat thang ca 746 vat the thi FBX phong len hon
/// 150 MB va Unity phai giu 2.2 trieu dinh, vi moi ban sao mang mot ban luoi
/// rieng. Tach ra thi chi con 12 MB va 532 nghin dinh, vi 746 vat the cung tro
/// vao 27 luoi dung chung. Nhe hon bon lan.
///
/// MAT DAT thi khong dat thang vao canh: no duoc doi thanh Unity Terrain qua
/// <see cref="Act2Terrain"/>, de to duoc nhieu lop vat lieu chong len nhau va
/// de ban to them bang tay ngay trong Unity.
///
/// Phan "moc rong" con giai duoc mot viec nua: Blender dung truc Z huong len,
/// Unity dung Y huong len. De Unity tu doc transform tu file FBX thi no tu lo
/// phan doi truc - toi khong phai tu tinh tay, cho de sai nhat trong ca viec.
/// </summary>
public static class Act2Baker
{
    const string ThuMucBanDo = "Assets/BlenderMaps/GraveyardAct2";
    const string DuongLuoi   = ThuMucBanDo + "/map_luoi.fbx";
    const string DuongViTri  = ThuMucBanDo + "/map_vitri.fbx";
    const string DuongScene  = "Assets/Scenes/Act2.unity";
    const string ThuMucVatLieu = "Assets/Materials";

    /// <summary>Ban kinh vung choi. Hang rao bao quanh rong 107 m nen lay 50.</summary>
    const float BanKinhSan = 50f;

    /// <summary>
    /// So quai dot dau. Act1 la 5, Act2 GAP DOI theo yeu cau.
    /// So nay con tu tang len theo tung dot trong GameDirector.
    /// </summary>
    const int SoQuaiDotDau = 7;
    const float NghiGiuaDot = 3f;

    [MenuItem("Diablo 2.5D/9. Dung Act2 tu ban do Blender", false, 90)]
    public static void Dung()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Act2] Dang o che do choi - thoat ra roi chay lai.");
            return;
        }

        var luoiGoc = AssetDatabase.LoadAssetAtPath<GameObject>(DuongLuoi);
        var viTriGoc = AssetDatabase.LoadAssetAtPath<GameObject>(DuongViTri);
        if (luoiGoc == null || viTriGoc == null)
        {
            Debug.LogError("[Act2] Thieu " + DuongLuoi + " hoac " + DuongViTri);
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var vatLieu = LamVatLieu();
        var banLuoi = DocLuoiGoc(luoiGoc);
        ChuaCay(banLuoi, vatLieu);

        var world = new GameObject("World").transform;

        ManhLuoi luoiDat;
        Matrix4x4 datRaTheGioi;
        List<Vector3> viTriMo;
        int soDat = DatBanDo(world, viTriGoc, banLuoi, vatLieu,
                             out luoiDat, out datRaTheGioi, out viTriMo);

        if (luoiDat == null)
        {
            Debug.LogError("[Act2] Khong tim thay tam mat dat trong ban do.");
            return;
        }

        var nenDat = Act2Terrain.Dung(luoiDat.luoi, datRaTheGioi, world, viTriMo.ToArray());

        // Dao dam lay SAU khi da co dia hinh va da dat het do dac - no can biet
        // cay va nha mo dang o dau de khoi dao vao goc chung.
        Act2DamLay.Dao(nenDat != null ? nenDat.GetComponent<Terrain>() : null, world);

        LapBoKhung(world);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, DuongScene);

        ThemVaoDanhSachBuild();

        Debug.Log("[Act2] Xong: dat " + soDat + " vat the tu ban do Blender vao "
                  + DuongScene + ", moi dot " + SoQuaiDotDau + " quai.");
    }

    // ================================================================
    //  MOT MANH LUOI GOC
    // ================================================================

    class ManhLuoi
    {
        public Mesh luoi;
        public Material[] vatLieu;
        public Vector3 tiLeBu;      // ti le rieng cua manh trong file FBX
        public string ten;
        public Loai loai;
    }

    enum Loai { MatDat, HangRao, NhaMo, Bia, Da, Cay }

    /// <summary>
    /// Doc 27 luoi goc ra khoi map_luoi.fbx.
    ///
    /// Unity nhap luoi voi dinh thu nho 100 lan roi BU LAI bang ti le 100 tren
    /// transform cua manh. Phai giu lai cai ti le bu do, khong thi ca ban do se
    /// be bang mot cai hop diem.
    /// </summary>
    static Dictionary<int, ManhLuoi> DocLuoiGoc(GameObject goc)
    {
        var bang = new Dictionary<int, ManhLuoi>();

        foreach (Transform t in goc.transform)
        {
            var mf = t.GetComponent<MeshFilter>();
            var mr = t.GetComponent<MeshRenderer>();
            if (mf == null || mr == null || mf.sharedMesh == null) continue;

            // Ten dang LIB_017_...  -> lay so 017
            var manh = t.name.Split('_');
            int chiSo;
            if (manh.Length < 2 || !int.TryParse(manh[1], out chiSo)) continue;

            bang[chiSo] = new ManhLuoi
            {
                luoi = mf.sharedMesh,
                vatLieu = mr.sharedMaterials,
                tiLeBu = t.localScale,
                ten = t.name,
                loai = PhanLoai(t.name),
            };
        }

        return bang;
    }

    /// <summary>
    /// Chua hai loi cua cay truoc khi dat ra ban do.
    ///
    /// Xem <see cref="Act2Cay"/> de biet hai loi do la gi va vi sao phai chua
    /// o ben Unity chu khong sua trong file Blender cua nguoi dung.
    /// </summary>
    static void ChuaCay(Dictionary<int, ManhLuoi> banLuoi, Dictionary<string, Material> vatLieu)
    {
        var voCay = Act2Cay.VoCay();
        if (voCay != null) vatLieu["MAT_bark"] = voCay;

        int tongNhanh = 0, soCay = 0;

        foreach (var kv in banLuoi)
        {
            var ml = kv.Value;
            if (ml.loai != Loai.Cay) continue;

            int nhanh;
            var luoiMoi = Act2Cay.MocNhanh(ml.luoi, ml.tiLeBu.x,
                                           ml.ten.Substring(8) + "_conhanh", out nhanh);
            if (luoiMoi != null) ml.luoi = luoiMoi;

            tongNhanh += nhanh;
            soCay++;
        }

        Debug.Log("[Act2] Da moc " + tongNhanh + " nhanh con cho " + soCay + " loai cay.");
    }

    static Loai PhanLoai(string ten)
    {
        if (ten.Contains("PERIMETER")) return Loai.HangRao;
        if (ten.Contains("MAUS")) return Loai.NhaMo;
        if (ten.Contains("TS_")) return Loai.Bia;
        if (ten.Contains("PR_rock")) return Loai.Da;
        if (ten.Contains("TREE")) return Loai.Cay;
        return Loai.MatDat;          // LIB_017 la tam dat, ten bi Blender doi
    }

    // ================================================================
    //  DAT BAN DO
    // ================================================================

    static int DatBanDo(Transform world, GameObject viTriGoc,
                        Dictionary<int, ManhLuoi> banLuoi, Dictionary<string, Material> vatLieu,
                        out ManhLuoi luoiDat, out Matrix4x4 datRaTheGioi, out List<Vector3> viTriMo)
    {
        luoiDat = null;
        datRaTheGioi = Matrix4x4.identity;
        viTriMo = new List<Vector3>();

        var nhom = new Dictionary<Loai, Transform>();
        foreach (Loai l in System.Enum.GetValues(typeof(Loai)))
        {
            var g = new GameObject(TenNhom(l)).transform;
            g.SetParent(world, false);
            nhom[l] = g;
        }

        int lopDat = LayLop("Ground");
        int lopMacDinh = LayLop("Default");
        int dem = 0;

        foreach (Transform moc in viTriGoc.transform)
        {
            if (!moc.name.StartsWith("INS_")) continue;

            var manh = moc.name.Split('_');
            int chiSo;
            if (manh.Length < 2 || !int.TryParse(manh[1], out chiSo)) continue;

            ManhLuoi ml;
            if (!banLuoi.TryGetValue(chiSo, out ml)) continue;

            // TAM DAT PHANG KHONG DUOC DAT VAO CANH NUA. No chi dap duoc mot
            // vat lieu duy nhat nen ca ban do rong 109 m chi mot mau, nhin ra
            // tam bia phang chu khong ra mat dat. Giu lai luoi va vi tri de
            // Act2Terrain doi no thanh Unity Terrain to duoc nhieu lop.
            if (ml.loai == Loai.MatDat)
            {
                luoiDat = ml;
                datRaTheGioi = Matrix4x4.TRS(moc.localPosition, moc.localRotation, moc.localScale);
                continue;
            }

            if (ml.loai == Loai.Bia || ml.loai == Loai.NhaMo)
                viTriMo.Add(moc.localPosition);

            var go = new GameObject(ml.ten.Substring(8) + "_" + dem);
            go.transform.SetParent(nhom[ml.loai], false);

            // Chep NGUYEN transform cua cai moc. Ti le cua moc da gom san ca
            // phan bu 100 lan cua Unity, nen khong duoc nhan them gi.
            go.transform.localPosition = moc.localPosition;
            go.transform.localRotation = moc.localRotation;
            go.transform.localScale = moc.localScale;

            go.AddComponent<MeshFilter>().sharedMesh = ml.luoi;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = DoiVatLieu(ml.vatLieu, vatLieu);

            GanVaCham(go, ml, lopDat, lopMacDinh);
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic
                                                     | StaticEditorFlags.OccluderStatic
                                                     | StaticEditorFlags.OccludeeStatic);
            dem++;
        }

        return dem;
    }

    static string TenNhom(Loai l)
    {
        switch (l)
        {
            case Loai.MatDat: return "MatDat";
            case Loai.HangRao: return "HangRao";
            case Loai.NhaMo: return "NhaMo";
            case Loai.Bia: return "BiaMo";
            case Loai.Da: return "Da";
            default: return "Cay";
        }
    }

    /// <summary>
    /// Gan va cham, MOI LOAI MOT KIEU.
    ///
    /// Khong dung MeshCollider cho tat ca: mot cay la hon 100 nghin dinh, 58 cay
    /// thi hop va cham se nang gap nhieu lan ca man choi. Cay chi can mot ong tru
    /// om lay than la du - nguoi choi khong bao gio dung cham vao tan la.
    /// </summary>
    static void GanVaCham(GameObject go, ManhLuoi ml, int lopDat, int lopMacDinh)
    {
        switch (ml.loai)
        {
            case Loai.MatDat:
                // Khong con di qua day nua - mat dat da thanh Unity Terrain
                go.layer = lopDat;
                break;

            case Loai.HangRao:
            case Loai.NhaMo:
                go.AddComponent<MeshCollider>();
                go.layer = lopMacDinh;
                break;

            case Loai.Bia:
            case Loai.Da:
                {
                    // Bia mo va da nho: mot cai hop om vua khit la du, re hon
                    // MeshCollider nhieu ma nguoi choi khong phan biet duoc
                    var b = go.AddComponent<BoxCollider>();
                    b.center = ml.luoi.bounds.center;
                    b.size = ml.luoi.bounds.size;
                    go.layer = lopMacDinh;
                    break;
                }

            default:
                {
                    // Cay: mot ong tru om than
                    OngTruOmThan(go.AddComponent<CapsuleCollider>(), ml.luoi);
                    go.layer = lopMacDinh;
                    break;
                }
        }
    }

    /// <summary>
    /// Dat mot ong tru om lay THAN cay.
    ///
    /// Hai chuyen deu phai DO, khong duoc doan:
    ///
    /// 1. TRUC CUA THAN. Blender dung truc Z huong len, va sau khi qua FBX thi
    ///    than cay VAN nam doc theo Z CUA LUOI (vat the bi xoay -90 do quanh X).
    ///    Dat cung direction = 1 (truc Y) thi duoc mot vien nang NAM NGANG, va
    ///    vi tam bounds lech theo truc dai nen no con LO LUNG ngang tan cay -
    ///    nguoi choi di duoi goc khong dung phai gi ca. Do chinh la loi "di
    ///    xuyen qua cay" cua Act2.
    ///
    ///    Chon truc bang cach hoi "TRUC CUC BO NAO DANG HUONG LEN TROI", chu
    ///    KHONG phai "truc nao dai nhat". Cay Act2 co tan xoe rong hon than cao:
    ///    bounds cua TREE_oakC la 0,122 x 0,077 x 0,086 - dai nhat la X, ma X
    ///    chinh la BE RONG TAN. Lay truc dai nhat thi 50 trong 58 cay van dat
    ///    ong tru nam ngang. Cay thi bao gio cung moc thang len.
    ///
    /// 2. BAN KINH. Bounds om ca TAN LA, ma tan xoe rong gap nam sau lan than.
    ///    Nhan mot he so vao bounds thi cay nay vua van, cay khac lech ba lan.
    ///    Do thang tu cac dinh nam o 15% DUOI CUNG cua than thi dung cho moi cay.
    ///
    /// Da vap dung hai cai bay nay mot lan o Act3 roi.
    /// </summary>
    public static void OngTruOmThan(CapsuleCollider c, Mesh luoi)
    {
        var bb = luoi.bounds;

        // ---- 1. Truc cuc bo nao dang HUONG LEN TROI ----
        var t = c.transform;
        float dX = Mathf.Abs(Vector3.Dot(t.right, Vector3.up));
        float dY = Mathf.Abs(Vector3.Dot(t.up, Vector3.up));
        float dZ = Mathf.Abs(Vector3.Dot(t.forward, Vector3.up));

        int truc = 1;
        if (dX >= dY && dX >= dZ) truc = 0;
        else if (dZ >= dX && dZ >= dY) truc = 2;

        float cao = bb.size[truc];
        c.direction = truc;
        c.height = cao;

        // ---- 2. Ban kinh DO tu cac dinh o 15% duoi cung ----
        int a = (truc + 1) % 3, b = (truc + 2) % 3;   // hai truc NGANG con lai

        var dinh = luoi.vertices;
        float mocDuoi = bb.min[truc] + cao * 0.15f;

        float tongA = 0f, tongB = 0f;
        int dem = 0;
        for (int i = 0; i < dinh.Length; i++)
        {
            if (dinh[i][truc] > mocDuoi) continue;
            tongA += dinh[i][a];
            tongB += dinh[i][b];
            dem++;
        }

        // TAM cua than lay theo trung binh cac dinh goc, KHONG lay tam bounds:
        // tan la xoe lech mot ben thi tam bounds truot theo no, va ong tru bo
        // ra khoi than cay.
        float giuaA = dem > 0 ? tongA / dem : bb.center[a];
        float giuaB = dem > 0 ? tongB / dem : bb.center[b];

        // Lay PHAN VI 90%, khong lay dinh XA NHAT.
        //
        // Ba cay TREE_oakC co vai dinh le nam tan ngoai ria - mot canh rui
        // xuong, mot mang co moc o goc. Lay dinh xa nhat thi ong tru phong len
        // 5-6 m ban kinh trong khi than that chua toi mot met: nguoi choi dam
        // vao mot buc tuong vo hinh cach goc cay sau buoc chan.
        var xa = new List<float>();
        for (int i = 0; i < dinh.Length; i++)
        {
            if (dinh[i][truc] > mocDuoi) continue;
            float dx = dinh[i][a] - giuaA;
            float dy = dinh[i][b] - giuaB;
            xa.Add(Mathf.Sqrt(dx * dx + dy * dy));
        }
        xa.Sort();

        float banKinh = xa.Count > 0
            ? xa[Mathf.Clamp(Mathf.FloorToInt(xa.Count * 0.90f), 0, xa.Count - 1)]
            : 0f;

        // Khong do duoc (luoi rong, hoac ca cay nam gon trong 15% duoi) thi quay
        // ve cach cu - van con hon la khong co va cham nao.
        if (dem == 0 || banKinh <= 0.0001f)
            banKinh = Mathf.Max(bb.size[a], bb.size[b]) * 0.09f;

        // Tran cuoi cung: than cay khong bao gio day bang 18% chieu cao cua no.
        banKinh = Mathf.Min(banKinh, cao * 0.18f);

        c.radius = banKinh;

        var tam = Vector3.zero;
        tam[truc] = bb.center[truc];
        tam[a] = giuaA;
        tam[b] = giuaB;
        c.center = tam;
    }

    static int LayLop(string ten)
    {
        int l = LayerMask.NameToLayer(ten);
        return l < 0 ? 0 : l;
    }

    // ================================================================
    //  VAT LIEU
    // ================================================================

    /// <summary>
    /// Dung vat lieu cho ban do.
    ///
    /// Blender dung vat lieu thu tuc (node), ma FBX khong mang duoc node nao -
    /// nhap sang Unity thi tat ca ve xam tron. Nen phai dung lai bang tay, va
    /// day cung la co hoi cho ban do moi khop tong mau cua man choi cu: dat nau
    /// kho, da xam lanh, la nau ua.
    /// </summary>
    // Mau da duoc HA TOI mot bac so voi lan dau. Man choi la dem, chi co anh
    // trang xanh chieu vao; de mau nguyen ban (dat 0.52) thi mat dat sang bech
    // len, nhin nhu ban ngay am u chu khong ra dem, va lech han tong voi Act1.
    static Dictionary<string, Material> LamVatLieu()
    {
        var bang = new Dictionary<string, Material>();

        bang["MAT_ground"] = Luu("Act2_Dat",
            Mats.Solid("Act2_Dat", new Color(0.30f, 0.21f, 0.15f), 0.03f, 0f,
                       TextureFactory.DarkSoil(), 26f));

        bang["MAT_stone"] = Luu("Act2_DaBia",
            Mats.Solid("Act2_DaBia", new Color(0.46f, 0.45f, 0.43f), 0.06f, 0f,
                       TextureFactory.CliffRock(), 2.4f));

        bang["MAT_rock"] = Luu("Act2_DaTang",
            Mats.Solid("Act2_DaTang", new Color(0.42f, 0.39f, 0.36f), 0.05f, 0f,
                       TextureFactory.CliffRock(), 3.2f));

        bang["MAT_bark"] = Luu("Act2_VoCay",
            Mats.Solid("Act2_VoCay", new Color(0.31f, 0.26f, 0.20f), 0.03f, 0f,
                       TextureFactory.Bark(), 3f));

        bang["MAT_leaf"] = Luu("Act2_La",
            Mats.Solid("Act2_La", new Color(0.36f, 0.25f, 0.12f), 0.04f, 0f));

        bang["MAT_iron"] = Luu("Act2_Sat",
            Mats.Solid("Act2_Sat", new Color(0.34f, 0.32f, 0.30f), 0.24f, 0.45f,
                       TextureFactory.Metal(new Color(0.45f, 0.42f, 0.40f), 0.8f), 2f));

        // Long nha mo: gan nhu den tuyet doi, de nhin vao thay hun hut
        bang["MAT_void"] = Luu("Act2_LongMo",
            Mats.Solid("Act2_LongMo", new Color(0.02f, 0.02f, 0.03f), 0.02f, 0f));

        bang["MAT_grass"] = Luu("Act2_Co",
            Mats.Solid("Act2_Co", new Color(0.40f, 0.30f, 0.14f), 0.03f, 0f));

        bang["MAT_flower"] = bang["MAT_grass"];
        bang["MAT_fog"] = bang["MAT_void"];

        AssetDatabase.SaveAssets();
        return bang;
    }

    static Material Luu(string ten, Material m)
    {
        string d = ThuMucVatLieu + "/" + ten + ".mat";
        var cu = AssetDatabase.LoadAssetAtPath<Material>(d);
        if (cu != null)
        {
            cu.shader = m.shader;
            cu.CopyPropertiesFromMaterial(m);
            EditorUtility.SetDirty(cu);
            return cu;
        }

        m.name = ten;
        AssetDatabase.CreateAsset(m, d);
        return m;
    }

    static Material[] DoiVatLieu(Material[] goc, Dictionary<string, Material> bang)
    {
        var kq = new Material[goc.Length];
        for (int i = 0; i < goc.Length; i++)
        {
            Material m;
            string ten = goc[i] != null ? goc[i].name : "";
            kq[i] = bang.TryGetValue(ten, out m) ? m : bang["MAT_stone"];
        }
        return kq;
    }

    // ================================================================
    //  BO KHUNG MAN CHOI
    // ================================================================

    static void LapBoKhung(Transform world)
    {
        WorldFactory.BuildSkyAndFog();

        var kho = new Kho();

        // ---------- NHAN VAT ----------
        var player = (GameObject)PrefabUtility.InstantiatePrefab(kho.player);
        PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.Completely,
                                           InteractionMode.AutomatedAction);
        player.name = "Player";
        // BO HO, khong phai giua ho. Trung tam ban do gio la mat nuoc; tha nguoi
        // choi xuong day thi vao game da thay minh dam duoi nuoc.
        player.transform.position = new Vector3(0f, 3f, -10.5f);

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

        // ---------- THU VIEN PREFAB ----------
        var gameGo = new GameObject("GAME");
        var boot = gameGo.AddComponent<GameBootstrap>();
        boot.arenaRadius = BanKinhSan;
        boot.worldSeed = 2024;

        kho.DienVao(gameGo.AddComponent<GameAssets>());

        // ---------- BO THA QUAI ----------
        var dirGo = new GameObject("GameDirector");
        var dir = dirGo.AddComponent<GameDirector>();
        dir.player = player.transform;
        dir.arenaRadius = BanKinhSan;
        dir.startingCount = SoQuaiDotDau;
        dir.waveDelay = NghiGiuaDot;

        // Ban do rong hon Act1 (109 m so voi vung choi 83 m) nen day cho de quai
        // ra xa hon mot chut, khong thi mo dau man da co quai dung ngay sau lung
        dir.minSpawnDistance = 16f;
        dir.maxSpawnDistance = 34f;

        // ---------- HUD ----------
        var hudGo = new GameObject("HUD");
        var hud = hudGo.AddComponent<GameHUD>();
        hud.player = player.GetComponent<PlayerController>();
        hud.playerHealth = player.GetComponent<Damageable>();
        hud.cameraRig = rig;
        hud.director = dir;
    }

    /// <summary>Nap lai cac prefab da nuong, tim theo ten trong Assets/Prefabs.</summary>
    class Kho
    {
        public GameObject player;

        public GameObject Lay(string ten)
        {
            var g = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + ten + ".prefab");
            if (g == null) Debug.LogWarning("[Act2] Thieu prefab " + ten);
            return g;
        }

        public Kho()
        {
            player = Lay("Player_Sorceress");
        }

        public void DienVao(GameAssets a)
        {
            a.playerPrefab = player;
            // PHAI du CA BAY o va DUNG THU TU MonsterType. Truoc day o day chi
            // co nam o: nuong lai Act2 la Quy du va Quy cay mat sach prefab,
            // va game lang le dung hinh chung bang code thay vi model.
            a.enemyPrefabs = new[]
            {
                Lay("Enemy_QuyLun"), Lay("Enemy_BoXuong"), Lay("Enemy_XacSong"),
                Lay("Enemy_QuyKhongLo"), Lay("Enemy_PhuThuy"),
                Lay("Enemy_QuyDu"), Lay("Enemy_QuyCay"),
            };

            a.fireballPrefab = Lay("Skill_QuaCauLua");
            a.iceStormPrefab = Lay("Skill_MuaBang");
            a.iceStormFieldPrefab = Lay("Vfx_VungBaoTuyet");
            a.iceShardPrefab = Lay("Vfx_TangBangRoi");
            a.lightningStormPrefab = Lay("Skill_SamSet");
            a.lightningStormFieldPrefab = Lay("Vfx_VungGiong");
            a.tornadoPrefab = Lay("Skill_LocXoay");

            a.fireExplosionPrefab = Lay("Vfx_NoLua");
            a.iceImpactPrefab = Lay("Vfx_NoBang");
            a.lightningImpactPrefab = Lay("Vfx_SetChamDat");
            a.stunnedPrefab = Lay("Vfx_Choang");
            a.burningPrefab = Lay("Vfx_BongChay");
            a.castFirePrefab = Lay("Vfx_TichTuLua");
            a.castIcePrefab = Lay("Vfx_TichTuBang");
            a.castLightningPrefab = Lay("Vfx_TichTuSet");
            a.frozenShatterPrefab = Lay("Vfx_VoBang");
            a.hitBurstPrefab = Lay("Vfx_TrungDon");

            a.brazierPrefab = Lay("Prop_LoLua");

            // Ban do Blender da co san cay da bia mo roi, nen khong can thu vien
            // rai them - de trong de khong ai lo tay rai chong len ban do ve tay
            a.rockPrefabs = new GameObject[0];
            a.pillarPrefabs = new GameObject[0];
            a.treePrefabs = new GameObject[0];
            a.leafyTreePrefabs = new GameObject[0];
            a.bushPrefabs = new GameObject[0];
        }
    }

    // ================================================================
    //  DANH SACH BUILD
    // ================================================================

    static void ThemVaoDanhSachBuild()
    {
        var ds = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in ds)
            if (s.path == DuongScene) return;

        ds.Add(new EditorBuildSettingsScene(DuongScene, true));
        EditorBuildSettings.scenes = ds.ToArray();
        Debug.Log("[Act2] Da them " + DuongScene + " vao danh sach build.");
    }
}
