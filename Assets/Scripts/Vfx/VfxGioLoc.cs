using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "GIO LOC" - DUNG LAI BANG BLENDER MCP (17/09/2026).
///
/// Nguoi dung: ban loc nho cu (Loc xoay thu nho nhuom nau) "qua xau"; xin dung lai bang Blender MCP cho thay gio loc
/// xoay that chi tiet, XOAY MOT CHIEU TU DUOI LEN, kem KHOI BUI DEN cuon bay len, mau nhu Loc xoay (xam trang);
/// bo tia set. Nguoi dung chon: cao ~5 m; khoi bui vua cuon quanh than vua de lai vet phia sau.
///
/// Tai nguyen dung trong Blender (CongCu/Blender/gio_loc.blend), nam o Resources/KyNang/GioLoc:
///   - LocNho.fbx   : Vo0, Vo1, Vo2 (ba vo phieu cao 5 m, loe mieng 2,6 m, UV quan quanh than, mau dinh xam trang;
///                    CHAN TO THEM 20% nho dan ve 0 o 2 m - nguoi dung khoanh do phan than duoi 17/09/2026
///                    mo o chan va mieng) + DaiGio (5 dai gio xoan 1,6 vong, hai mieng bat cheo).
///   - GioDai.png   : dai gio mem nghieng, LIEN MACH ca u va v (nhieu 4D quan tren mat xuyen) - vo trong.
///   - GioSoi.png   : soi gio manh, thua - vo ngoai va dai gio.
///   - BuiDenCuon.png : flipbook 6x6 dam bui den cuon meo no dan.
///
/// MOT CHIEU TU DUOI LEN: do tren luoi da nhap (menu 71): vet gio tren anh VA dai gio cung "goc atan2(z,x) GIAM khi len
/// cao". Quay lam goc TANG thi moi vet chay len tren (nhu cot den cat toc). Transform.Rotate quanh +Y voi goc DUONG lam
/// atan2(z,x) GIAM -> moi thu quay AM (<see cref="ChieuQuayGioLoc"/>). Anh truot v AM cung day vet len. Khong co lop nao
/// quay nguoc (Loc xoay lon co vo quay nguoc chieu nhau - chinh la thu lam no xoay "hai chieu").
/// </summary>
public static partial class VfxFactory
{
    const string ThuMucGioLoc = "KyNang/GioLoc/";

    /// <summary>Dau cua do/giay cho Spin quanh +Y: AM = vet gio di len (do menu 71).</summary>
    public const float ChieuQuayGioLoc = -1f;

    /// <summary>Dau cua van toc quy dao hat quanh +Y de hat cuon CUNG CHIEU voi than loc (do menu 71).</summary>
    public const float ChieuQuyDaoGioLoc = -1f;

    static GameObject khoLocNho;
    static bool daTimLocNho;
    static Material mGioDai, mGioSoi, mBuiDenCuon;

    static GameObject KhoLocNho
    {
        get
        {
            if (!daTimLocNho) { daTimLocNho = true; khoLocNho = Resources.Load<GameObject>(ThuMucGioLoc + "LocNho"); }
            return khoLocNho;
        }
    }

    public static Material BuiDenCuonMat
    {
        get
        {
            if (mBuiDenCuon == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucGioLoc + "BuiDenCuon");
                if (t == null) return null;
                mBuiDenCuon = Mats.FlipbookAlpha("P_BuiDenCuon", t, Color.white);
            }
            return mBuiDenCuon;
        }
    }

    static Material VatLieuGio(ref Material kho, string ten)
    {
        if (kho == null)
        {
            var t = Resources.Load<Texture2D>(ThuMucGioLoc + ten);
            kho = Mats.Alpha("P_" + ten, t != null ? t : TextureFactory.TornadoWall(), Color.white);
        }
        return kho;
    }

    /// <summary>Dung hinh mot con loc nho. Goc = chan loc tren mat dat.</summary>
    public static GameObject BuildGioLoc()
    {
        var root = new GameObject("GioLocHinh");
        var kho = KhoLocNho;
        if (kho != null)
        {
            foreach (var mf in kho.GetComponentsInChildren<MeshFilter>(true))
            {
                // (ten, anh, mau vo, do/giay, truot UV) - vo trong day va nhanh, ra ngoai mong va cham dan.
                // Hai vo ngoai MO (0,34 / 0,26): de 0,55 / 0,42 thi ba lop chong nhau thanh mot man suong trang deu, mat het
                // soi gio (anh gioloc_can_zoom 17/09/2026). Dai gio dam 0,9 de doc ra duong xoan.
                Material goc; Color mau; float quay; Vector2 truot;
                switch (mf.name)
                {
                    case "Vo0":    goc = VatLieuGio(ref mGioDai, "GioDai"); mau = new Color(0.82f, 0.84f, 0.88f, 0.72f); quay = 330f; truot = new Vector2(0f, -0.95f); break;
                    case "Vo1":    goc = VatLieuGio(ref mGioDai, "GioDai"); mau = new Color(0.90f, 0.92f, 0.95f, 0.34f); quay = 250f; truot = new Vector2(0f, -0.70f); break;
                    case "Vo2":    goc = VatLieuGio(ref mGioSoi, "GioSoi"); mau = new Color(0.96f, 0.97f, 1.00f, 0.26f); quay = 180f; truot = new Vector2(0f, -0.50f); break;
                    default:       goc = VatLieuGio(ref mGioSoi, "GioSoi"); mau = new Color(1.00f, 1.00f, 1.00f, 0.90f); quay = 400f; truot = new Vector2(-1.1f, 0f); break;
                }
                var go = new GameObject(mf.name);
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = mf.transform.localPosition;
                go.transform.localRotation = mf.transform.localRotation;
                go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                var mr = go.AddComponent<MeshRenderer>();
                var m = new Material(goc);
                m.SetColor("_TintColor", mau);
                mr.sharedMaterial = m;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                var sp = go.AddComponent<Spin>();
                sp.axis = Vector3.up;
                sp.degreesPerSecond = ChieuQuayGioLoc * quay;
                // Dai gio: u = do cao x 2 nen truot u AM la day soi gio len doc dai
                var sc = go.AddComponent<ScrollUV>();
                sc.speed = truot;
            }
        }

        BuildBuiCuonQuanhThan(root.transform);

        // Vet khoi bui den o lai phia sau duong loc di (khong gian the gioi) - cung ham cua Loc xoay, doi sang bui Blender
        var vet = BuildKhoiBuiLoc(root.transform, 0.55f);
        var mbd = BuiDenCuonMat;
        if (mbd != null) vet.GetComponent<ParticleSystemRenderer>().sharedMaterial = mbd;
        var vm = vet.main; vm.maxParticles = 110;
        var vem = vet.emission; vem.rateOverTime = 20f;
        var vv = vet.velocityOverLifetime;
        vv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 2.2f, ChieuQuyDaoGioLoc * 4.0f);

        // Hat dat cat li ti bi hut quay quanh than
        BuildDebrisSwarm(root.transform, 0.45f, "Grit", 0.04f, 0.13f, 50f, 110, 2.2f, 4.6f);
        var grit = root.transform.Find("Grit").GetComponent<ParticleSystem>();
        var gv = grit.velocityOverLifetime;
        gv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 7f, ChieuQuyDaoGioLoc * 11f);
        return root;
    }

    /// <summary>
    /// KHOI BUI DEN CUON QUANH THAN BOC LEN: phun o chan, bay len doc than theo quy dao CUNG CHIEU xoay, no to va loe ra
    /// theo mieng loc roi tan. Khong gian CUC BO de di theo con loc (vet phia sau da co KhoiBui khong gian the gioi).
    /// </summary>
    static void BuildBuiCuonQuanhThan(Transform parent)
    {
        var mat = BuiDenCuonMat;
        var ps = NewPS("BuiCuon", parent, new Vector3(0f, 0.2f, 0f), mat != null ? mat : SmokeMat,
                       ParticleSystemRenderMode.Billboard);
        if (mat != null) BatFlipbook(ps, 6, 6, 1);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.2f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.9f, 1.8f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        // Den dam: 0,07-0,20 tren nen dem chi con vet xam nhat (anh 17/09/2026)
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.02f, 0.018f, 0.016f), new Color(0.09f, 0.08f, 0.07f));
        m.simulationSpace = ParticleSystemSimulationSpace.Local;
        m.maxParticles = 90;
        var em = ps.emission; em.rateOverTime = 44f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 0.45f; sh.radiusThickness = 1f;
        sh.rotation = new Vector3(-90f, 0f, 0f);     // vong nam NGANG tren mat dat (Circle mac dinh dung trong mat phang XY)

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 3.5f, ChieuQuyDaoGioLoc * 5.5f);
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.radial = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.y = new ParticleSystem.MinMaxCurve(2.6f, 3.6f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.8f)));
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f, new Color(0.9f, 0.9f, 0.9f), 0.5f, new Color(0.75f, 0.75f, 0.75f), 1f,
            0f, 1f, 0.75f, 0f));
        var rot = ps.rotationOverLifetime; rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-1.2f, -0.4f);
    }
}
