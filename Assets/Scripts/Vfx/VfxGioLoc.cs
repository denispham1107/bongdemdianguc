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
///                    CHAN TO THEM 40% (so ban goc) roi THEM 20% nua tren ban ay (x1,68 so goc), nho dan ve 0 o 2,3 m - nguoi dung khoanh do phan than duoi 17/09/2026
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

    /// <summary>Toan bo ban kinh loc (vo, dai gio, vong bui, vet bui, hat dat) nhan them - nguoi dung 17/09/2026: to them 10%.
    /// Chieu cao giu 5 m.</summary>
    public const float HeSoBanKinhGioLoc = 1.1f;

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

    /// <summary>Chieu cao hinh Loc xoay that (4 vo 15 m + vanh cuon) - lay ti le thu nho cho Gio loc.</summary>
    public const float ChieuCaoLocXoayHinh = 15.72f;

    /// <summary>Gio loc = Loc xoay thu deu bao nhieu lan: than 5 m (GioLoc.ChieuCao) / 15,72 = 0,318. Chan vo chinh 2,6 -> 0,83 m,
    /// mieng 6,8 -> 2,16 m - gan trung Gio loc cu (0,91 / 2,03). Hoa loc xoay phinh tu dung co nay.</summary>
    public const float HeSoHinhGioLoc = 5f / ChieuCaoLocXoayHinh;

    /// <summary>
    /// GIO LOC = LOC XOAY THU NHO (nguoi dung 01/10/2026: "thu cho Gio loc co hieu ung giong hoan toan Lốc xoáy, chi co dieu cho kich
    /// thuoc Lốc xoáy bang kich thuoc Gio loc hien gio"; chon: thu DEU x0,318 giu dang, BO mau may giong + may trong than, GIU 240 hat
    /// moi giay nhu Loc xoay, Hoa loc xoay phinh tu x0,318). Lay DUNG phan hinh "LocXoayHinh" cua prefab Skill_LocXoay (thu Loc xoay
    /// that trong game dung), them cac lop bui cua Tornado.Start (DamBaoBuiCuonLenLocXoay), roi phong ca cum x HeSoHinhGioLoc.
    /// Moi he hat MO PHONG CUC BO + ti le Hierarchy -> ca chuyen dong (bay len, toa, quy dao) thu dung ti le, va hat di theo lốc
    /// 9,5 m/s (bui chan Loc xoay o khong gian THE GIOI vi no di 3,4 m/s - de the gioi o Gio loc thi thanh vet dai sau lung).
    /// Den StormLight thu tam theo. Tia set: GioLocSetTrongLoc -> TornadoBolt(x0,318). Ban cu: BuildGioLocCu (tam giu de doi chung).
    /// </summary>
    public static GameObject BuildGioLoc()
    {
        var root = new GameObject("GioLocHinh");
        Transform hinh = null;
        var pf = GameAssets.I != null ? GameAssets.I.tornadoPrefab : null;
        if (pf != null && pf.transform.childCount > 0)
        {
            hinh = Object.Instantiate(pf.transform.GetChild(0), root.transform, false);
            hinh.name = "LocXoayHinh";
        }
        else
        {
            var tam = BuildLocXoay(1f);
            hinh = tam.transform.Find("LocXoayHinh");
            if (hinh == null) { Object.Destroy(root); return BuildGioLocCu(); }   // thieu FBX Loc xoay
            hinh.SetParent(root.transform, false);
            Object.Destroy(tam);
        }
        DamBaoBuiCuonLenLocXoay(root.transform, 1f);
        KhacLocXoay(hinh);
        hinh.localScale = Vector3.one * HeSoHinhGioLoc;
        foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // PlayOnStart bat lai o khung dau
            var m = ps.main;
            m.simulationSpace = ParticleSystemSimulationSpace.Local;
            m.scalingMode = ParticleSystemScalingMode.Hierarchy;
            // Trong luc tinh bang m/s^2 THE GIOI, khong thu theo ti le -> bui chan (gravity -0,04 = boc len) bay cao x1,47 (menu 71 C3)
            m.gravityModifierMultiplier *= HeSoHinhGioLoc;
        }
        foreach (var lt in hinh.GetComponentsInChildren<Light>(true))
        {
            float tam = lt.range * HeSoHinhGioLoc;
            var fl = lt.GetComponent<LightFlicker>();
            if (fl != null) fl.DatTamGoc(tam); else lt.range = tam;
        }
        return root;
    }

    /// <summary>Gio loc toi hon Loc xoay bao nhieu (mau vo + bui, nguoi dung 01/10/2026 "xam den hon nua, con trang qua" - chon toi 30%).</summary>
    public const float HeSoToiGioLoc = 0.7f;

    /// <summary>Lop bui len tan dinh (BuiCuonLen) cua Gio loc: so hat x2 va toc quay quanh than x2 so voi Loc xoay (nguoi dung 01/10/2026:
    /// "bui khoi nhieu day hon, cuon len xoay tron xung quanh len tan dinh" - chon "len dinh x2, quay x2").</summary>
    public const float HeSoBuiLenGioLoc = 2f, HeSoQuayBuiGioLoc = 2f;

    /// <summary>
    /// GIO LOC KHAC LOC XOAY o ba cho (nguoi dung 01/10/2026, lan ba - sau khi Gio loc thanh Loc xoay thu nho):
    ///   1) BO HIEU UNG SANG: quang sang trang o mieng (HaoQuang) + den chop (StormLight); loe cham dat cua tia tat o GioLocSetTrongLoc.
    ///      Chi con 2 tia set (duong tia giu nguyen: mieng -> dat canh chan).
    ///   2) Lop bui len tan dinh x HeSoBuiLenGioLoc hat, quay quanh than x HeSoQuayBuiGioLoc.
    ///   3) Than + bui TOI x HeSoToiGioLoc: vo qua MaterialPropertyBlock (vat lieu M_P_LX_* la asset dung chung voi Loc xoay - KHONG sua),
    ///      bui qua mau hat.
    /// Goi TRUOC khi phong x0,318 (gia tri trong don vi Loc xoay goc).
    /// </summary>
    static void KhacLocXoay(Transform hinh)
    {
        foreach (var ten in new[] { "HaoQuang", "StormLight" })
        {
            var t = hinh.Find(ten);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
        var mpb = new MaterialPropertyBlock();
        foreach (var mr in hinh.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (mr.sharedMaterial == null || !mr.sharedMaterial.HasProperty("_TintColor")) continue;
            var c = mr.sharedMaterial.GetColor("_TintColor");
            mr.GetPropertyBlock(mpb);
            mpb.SetColor("_TintColor", new Color(c.r * HeSoToiGioLoc, c.g * HeSoToiGioLoc, c.b * HeSoToiGioLoc, c.a));
            mr.SetPropertyBlock(mpb);
        }
        foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            var c0 = m.startColor.colorMin; var c1 = m.startColor.colorMax;
            m.startColor = new ParticleSystem.MinMaxGradient(
                new Color(c0.r * HeSoToiGioLoc, c0.g * HeSoToiGioLoc, c0.b * HeSoToiGioLoc, c0.a),
                new Color(c1.r * HeSoToiGioLoc, c1.g * HeSoToiGioLoc, c1.b * HeSoToiGioLoc, c1.a));
            if (ps.name != "BuiCuonLen") continue;
            m.maxParticles = Mathf.CeilToInt(m.maxParticles * HeSoBuiLenGioLoc);
            var e = ps.emission; e.rateOverTimeMultiplier *= HeSoBuiLenGioLoc;
            var v = ps.velocityOverLifetime; v.orbitalYMultiplier *= HeSoQuayBuiGioLoc;
        }
    }

    /// <summary>Hinh Gio loc CU (luoi Blender LocNho, mau may giong, may trong than) - 17/09 - 30/09/2026. Khong con dung trong game;
    /// giu tam cho phep thu doi chung (menu 71) va menu 71c / 94 cho toi khi nguoi dung chot ban moi.</summary>
    public static GameObject BuildGioLocCu()
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
                    // MAU MAY GIONG (nguoi dung 28/09/2026: "doi mau Gio loc nhu mau dam may giong"; truoc: xam trang 0,82 - 1,00):
                    // vo trong cung = tang may XAM (duoi, toi hon), ra ngoai dan sang tang may SANG (tren) - nhu may: day toi, dinh sang
                    case "Vo0":    goc = VatLieuGio(ref mGioDai, "GioDai"); mau = MauMayGioLoc(0.0f, DoDucVoGioLoc[0]); quay = 330f; truot = new Vector2(0f, -0.95f); break;
                    case "Vo1":    goc = VatLieuGio(ref mGioDai, "GioDai"); mau = MauMayGioLoc(0.5f, DoDucVoGioLoc[1]); quay = 250f; truot = new Vector2(0f, -0.70f); break;
                    case "Vo2":    goc = VatLieuGio(ref mGioSoi, "GioSoi"); mau = MauMayGioLoc(1.0f, DoDucVoGioLoc[2]); quay = 180f; truot = new Vector2(0f, -0.50f); break;
                    default:       goc = VatLieuGio(ref mGioSoi, "GioSoi"); mau = MauMayGioLoc(1.0f, DoDucVoGioLoc[3]); quay = 400f; truot = new Vector2(-1.1f, 0f); break;
                }
                var go = new GameObject(mf.name);
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = mf.transform.localPosition;
                go.transform.localRotation = mf.transform.localRotation;
                go.transform.localScale = Vector3.Scale(mf.transform.localScale, new Vector3(HeSoBanKinhGioLoc, 1f, HeSoBanKinhGioLoc));
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

        // BUI XAM CUON QUANH CHAN - KIEU LOC XOAY (nguoi dung 28/09/2026: khoi bui den "cho cung mau cung hieu ung giong skill Loc
        // xoay"; truoc la khoi DEN BuiDenCuon). Ten giu "BuiCuon" (menu 71 doc). Co / vong theo chan MOI (x2): chan Vo1 1,11 m so voi
        // vo chinh Loc xoay 1,3 m -> co 0,85; bay len x0,5 vi loc chi cao 5 m. Cuc bo de om chan khi loc bay 9,5 m/s.
        var buiCuon = BuiXamChanLoc(root.transform, "BuiCuon", new Vector3(0f, 0.2f, 0f), CoBuiGioLoc, BanKinhVongBuiGioLoc, 0.5f, true);
        // cung kieu cuon cua Loc xoay nhung MAU MAY GIONG (28/09/2026) - Loc xoay giu bui xam nhat
        var bcm = buiCuon.main; bcm.startColor = new ParticleSystem.MinMaxGradient(MauBuiGioLocToi, MauBuiGioLocSang);
        // DAY DAC HON (nguoi dung 29/09/2026: "hieu ung bui khoi cuon len day dac hon nua", chon x2 so hat): 40 -> 80 hat/giay,
        // tran 120 -> 240. Chi Gio loc - bui chan Loc xoay giu 40 / 120 (cung ham BuiXamChanLoc).
        bcm.maxParticles = Mathf.RoundToInt(bcm.maxParticles * HeSoBuiDayGioLoc);
        var bce = buiCuon.emission; bce.rateOverTime = bce.rateOverTime.constant * HeSoBuiDayGioLoc;
        // CUON LEN TAN DINH (29/09/2026, nguoi dung: bui "chi den tam nua than la het"): them 40 hat/giay om than (giua Vo0 va Vo1),
        // bay len 5 m trong ~2 s, mau may giong nhu bui chan
        BuiCuonLenTheoThan(root.transform, "BuiCuonLen", CaoGioLocHinh, h => 1.15f * BanKinhVoTrongGioLoc(h), TocBuiCuonLenGioLoc,
                           1.8f, 2.2f, 1.2f, 2.6f, 3.5f, MauBuiGioLocToi, MauBuiGioLocSang);

        // Vet bui o lai phia sau duong loc di (khong gian the gioi) - nay cung BUI XAM cua Loc xoay (truoc: khoi den)
        var vet = BuildKhoiBuiLoc(root.transform, 0.55f * HeSoBanKinhGioLoc * 2f);
        vet.GetComponent<ParticleSystemRenderer>().sharedMaterial = BuiXamMat;
        DatKhungBuiXam(vet);
        var vm = vet.main; vm.maxParticles = 110;
        vm.startColor = new ParticleSystem.MinMaxGradient(MauBuiGioLocToi, MauBuiGioLocSang);
        var vcol = vet.colorOverLifetime; vcol.enabled = true;
        vcol.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, new Color(0.85f, 0.86f, 0.9f), 1f, 0f, 0.7f, 0.45f, 0f));
        var vem = vet.emission; vem.rateOverTime = 20f;
        var vv = vet.velocityOverLifetime;
        vv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 2.2f, ChieuQuyDaoGioLoc * 4.0f);

        // Hat dat cat li ti bi hut quay quanh than
        BuildDebrisSwarm(root.transform, 0.90f * HeSoBanKinhGioLoc, "Grit", 0.04f, 0.13f, 50f, 110, 2.2f, 4.6f);   // chan x2 -> vong cat x2
        var grit = root.transform.Find("Grit").GetComponent<ParticleSystem>();
        var gv = grit.velocityOverLifetime;
        gv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 7f, ChieuQuyDaoGioLoc * 11f);

        MayDinhGioLoc(root.transform);
        return root;
    }

    /// <summary>May giong TRONG LONG than Gio loc: tam hat sinh trong khoi NON quanh truc tu CaoDuoiMayGioLoc den CaoTrenMayGioLoc (m),
    /// ban kinh non = TiLeMayTrongVo x ban kinh vo trong o day / dinh non (vo trong o 1,8 m 1,13 m, o 4,3 m 1,79 m).
    /// 29/09/2026: hop 2,5 - 4,3 m, ngang +-0,65; 01/10/2026 nguoi dung "nhieu va day hon 1 chut, van trong than", chon mo xuong 1,8 m.</summary>
    public const float CaoDuoiMayGioLoc = 1.8f, CaoTrenMayGioLoc = 4.3f, TiLeMayTrongVo = 0.62f;

    /// <summary>Mat do may so voi ban 29/09/2026 (so dam moi met chieu cao x k, do duc x (1 + 0,5 (k - 1))). Nguoi dung chon x1,5
    /// (01/10/2026, menu 94 chup cu / 1,3 / 1,5 / 1,8: may lo ro hon x1,83 dem, x1,63 ngay so ban cu).</summary>
    public const float HeSoDayMayGioLoc = 1.5f;

    /// <summary>
    /// MAY GIONG NHE TRONG CON GIO LOC (nguoi dung 29/09/2026: "them hieu ung may giong nhe o tren dinh cac con loc", chon "may mong co
    /// mieng loc, loe khi set danh"; cung ngay doi: "cho may giong nam TRONG con loc, khong phai tren dau", chon "trong nua tren than
    /// 2,5 - 4,7 m"). Anh MayGiong (4 dam may Blender 2x2) cua ky nang May giong, mau may cua than Gio loc (tang giua,
    /// x HeSoSangMayGioLoc), mong (do duc hat 0,45 - 0,65), dam 1,4 - 2,2 m cho gon trong vo, xoay cham cung chieu cuon, CUC BO.
    /// Moi nhip set trong long loc (GioLocSetTrongLoc) goi LoeSangMay.Chop -> ca dam may loe nhe roi tat 0,15 s.
    /// </summary>
    public static void MayDinhGioLoc(Transform cha, float k = HeSoDayMayGioLoc)
    {
        var mat = VatLieuMayGiong("MayDinhGioLoc", ThuMucMayGiong, "MayGiong", MauMayGioLoc(0.5f, 1f), false);
        var ps = NewPS("MayTrongLoc", cha, new Vector3(0f, CaoDuoiMayGioLoc, 0f), mat, ParticleSystemRenderMode.Billboard);
        LuoiAnh2x2(ps);
        // Ban cu: 20 dam / 6 moi giay / dot dau 8 tren 1,8 m chieu cao -> giu mat do theo chieu cao moi roi nhan k
        float cao = CaoTrenMayGioLoc - CaoDuoiMayGioLoc, n = cao / 1.8f * k, duc = 1f + 0.5f * (k - 1f);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 2.4f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(1.4f, 2.2f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, Mathf.Min(1f, 0.45f * duc)), new Color(1f, 1f, 1f, Mathf.Min(1f, 0.65f * duc)));
        m.simulationSpace = ParticleSystemSimulationSpace.Local;
        m.scalingMode = ParticleSystemScalingMode.Hierarchy;
        m.maxParticles = Mathf.RoundToInt(20f * n);
        var em = ps.emission; em.rateOverTime = 6f * n;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(8f * n)) });   // co may ngay tu luc tung
        // Khoi NON quanh truc (sinh trong THE TICH non): day o CaoDuoi, cao toi CaoTren, ban kinh theo vo trong - gon trong vo.
        // Non mac dinh phun theo +Z -> xoay -90 quanh X cho truc non dung len +Y.
        float rDay = TiLeMayTrongVo * BanKinhVoTrongGioLoc(CaoDuoiMayGioLoc), rDinh = TiLeMayTrongVo * BanKinhVoTrongGioLoc(CaoTrenMayGioLoc);
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.ConeVolume;
        sh.rotation = new Vector3(-90f, 0f, 0f);
        sh.radius = rDay; sh.radiusThickness = 1f;
        sh.angle = Mathf.Atan2(rDinh - rDay, cao) * Mathf.Rad2Deg;
        sh.length = cao;
        // Ca ba truc cung kieu Constant (lech kieu la Unity bo ca mo-dun); khong troi len (giu trong than)
        var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.Local;
        v.x = new ParticleSystem.MinMaxCurve(0f); v.y = new ParticleSystem.MinMaxCurve(0f); v.z = new ParticleSystem.MinMaxCurve(0f);
        v.orbitalX = new ParticleSystem.MinMaxCurve(0f); v.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 0.8f); v.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.8f), new Keyframe(1f, 1.15f)));
        var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
        var loe = cha.GetComponent<LoeSangMay>();
        if (loe == null) loe = cha.gameObject.AddComponent<LoeSangMay>();
        loe.Gan(new Renderer[] { ps.GetComponent<Renderer>() });
    }

    /// <summary>So nhip set trong loc da phong (moi nhip 2 tia) - phep thu menu 71 doc.</summary>
    public static int SoNhipSetTrongGioLoc;

    // Ban kinh vo TRONG CUNG Vo0 cua LocNho.fbx moi 0,5 m do cao (chua nhan HeSoBanKinhGioLoc). 28/09/2026 THAN TO RA (nguoi
    // dung: "nhin loc nhu cay kem oc que"): chan x2 (0,413 -> 0,827), to dan deu r = 0,826 + 1,019 (z/5)^1,6, mieng giu 1,845 -
    // sua trong Blender (CongCu/Blender/gio_loc_than_rong.blend). Bang cu 17/09: 0,41 0,42 0,445 0,48 0,53 0,67 0,87 1,05 1,30 1,54 1,85.
    static readonly float[] banKinhVo0 = { 0.827f, 0.852f, 0.904f, 0.975f, 1.062f, 1.163f, 1.276f, 1.402f, 1.539f, 1.687f, 1.845f };

    /// <summary>Bui xam chan Gio loc: co hat (so voi Loc xoay) va ban kinh vong phun (m) - theo chan moi.</summary>
    public const float CoBuiGioLoc = 0.85f, BanKinhVongBuiGioLoc = 1.0f;

    /// <summary>
    /// Do duc (alpha _TintColor) Vo0, Vo1, Vo2, DaiGio. 28/09/2026 than to gap doi thi ba lop chong thanh khoi TRANG DAC, che het bui -
    /// nguoi dung: "cho than trong hon de thay bui ben trong". Truoc: 0,72 / 0,34 / 0,26 / 0,90 (menu 71c giu lam doi chung).
    /// </summary>
    public static readonly float[] DoDucVoGioLoc = { 0.72f, 0.34f, 0.26f, 0.90f };
    // ^ 28/09/2026 khuya: doi sang MAU MAY GIONG roi thi than toi + trong (0,42/0,20/0,16/0,70) CHIM vao nen dem -> nguoi dung chon
    //   TRA do duc ve muc cu (than dac, de thay; kho thay bui ben trong) VA pha mau may sang hon mot chut (HeSoSangMayGioLoc).

    /// <summary>Mau may cua Gio loc SANG HON mau may that bao nhieu lan (nguoi dung 28/09/2026: "pha mau may sang hon mot chut de
    /// than noi len nen dem"). Chi Gio loc - dam may May giong giu nguyen.</summary>
    /// <summary>So hat bui cuon quanh chan Gio loc gap bao nhieu lan bui chan Loc xoay (nguoi dung 29/09/2026 chon x2).</summary>
    public const float HeSoBuiDayGioLoc = 2f;

    public const float HeSoSangMayGioLoc = 2.025f;   // 29/09/2026 toi di 10% (2,25 x 0,9) - menu 93 chup 10/20/30%, nguoi dung chon 10%
    // ^ x1,5 van chim (noi x1,02) - menu 71c quet 1,5/2/2,5/3, nguoi dung chon 2,5 (noi x1,53). 29/09/2026 nguoi dung: "cho toan than
    //   den hon 1 chut" - quet 2,5/2,25/2,0/1,75 (bui da day x2), chon 2,25 CHO CA THAN LAN BUI (dem van noi x1,13 so ban chim).

    /// <summary>Mau mot lop vo Gio loc: noi tu tang may XAM (t = 0) sang tang may SANG (t = 1) cua May giong.</summary>
    public static Color MauMayGioLoc(float t, float alpha)
    {
        var c = Color.Lerp(MauMayGiongXam, MauMayGiongSang, t) * HeSoSangMayGioLoc;
        c.a = alpha;
        return c;
    }

    /// <summary>Bui cuon + vet bui Gio loc: mau hai tang may giong, do duc 0,70 / 0,90 nhu bui Loc xoay (28/09/2026).</summary>
    public static Color MauBuiGioLocToi { get { var c = MauMayGiongXam * HeSoSangMayGioLoc; c.a = 0.70f; return c; } }
    public static Color MauBuiGioLocSang { get { var c = MauMayGiongSang * HeSoSangMayGioLoc; c.a = 0.90f; return c; } }

    /// <summary>Ban kinh vo trong cung (da nhan 1,1) o do cao y tinh tu chan loc.</summary>
    /// <summary>Chieu cao hinh Gio loc (LocNho.fbx, than 5 m).</summary>
    public const float CaoGioLocHinh = 5f;

    public static float BanKinhVoTrongGioLoc(float y)
    {
        float f = Mathf.Clamp(y / 0.5f, 0f, banKinhVo0.Length - 1.001f);
        int i = Mathf.FloorToInt(f);
        return Mathf.Lerp(banKinhVo0[i], banKinhVo0[i + 1], f - i) * HeSoBanKinhGioLoc;
    }

    /// <summary>
    /// HAI TIA SET tren than Gio loc moi nhip (GioLoc.Update, 0,45 s): tu 01/10/2026 Gio loc la Loc xoay thu nho nen tia CHINH LA tia
    /// Loc xoay (TornadoBolt - kieu Giut set, 3-5 nhanh, giang tu mieng xuong doc than, bam theo loc) thu x HeSoHinhGioLoc.
    /// Ten "SetTrongGioLoc" de phep thu loc rieng. Ban 17/09 (tia trong long vo, may loe) con trong git 146c16c.
    /// </summary>
    public static void GioLocSetTrongLoc(Transform loc)
    {
        if (loc == null) return;
        SoNhipSetTrongGioLoc++;
        TornadoBolt(loc, HeSoHinhGioLoc, "SetTrongGioLoc", false);   // 01/10/2026 lan ba: Gio loc KHONG loe cham dat (nguoi dung bo hieu ung sang)
    }
}
