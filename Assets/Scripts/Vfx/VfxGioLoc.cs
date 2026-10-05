using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "GIO LOC". Tu 01/10/2026 Gio loc = Loc xoay thu nho x0,318 (BuildGioLoc), khac Loc xoay o KhacLocXoay; than = 18 dai
/// gio xoan Blender (Resources/KyNang/GioLoc/GioXoan.fbx + GioXoan.png, ThanGioXoan), vet gio cuon len (VetGio.png, VetGioXoanLen), vet bui /
/// khoi sau lung (VetSauGioLoc). BuiDenCuon.png (flipbook 6x6 bui den) dung cho hat den cuon len (HatDenCuonLen).
/// 05/10/2026 nguoi dung cho XOA HINH CU (luoi LocNho.fbx, anh GioDai / GioSoi, mau may giong, may trong than - BuildGioLocCu, menu 71c / 94):
/// con trong git truoc commit nay.
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

    static Material mBuiDenCuon;

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
    /// Den StormLight thu tam theo. Tia set: GioLocSetTrongLoc -> TornadoBolt(x0,318).
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
            if (hinh == null) { Object.Destroy(root); return BuildTornadoCu(HeSoHinhGioLoc); }   // thieu FBX Loc xoay: Loc xoay cu dung bang code thu nho
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
            // KHOI CO SAN NGAY LUC TUNG (nguoi dung 02/10/2026 chon, khi con bui cuon len than): Gio loc chi song 4,5 s, bui moc dan tu cho
            // sinh -> prewarm (lap + mo phong truoc mot chu ky luc Play). 03/10/2026 chi con bui chan - giu prewarm de chan co bui ngay.
            if (ps.name.StartsWith("Bui") || ps.name.StartsWith("HatDen") || ps.name.StartsWith("DenCuon")) { m.loop = true; m.prewarm = true; }
            // Trong luc tinh bang m/s^2 THE GIOI, khong thu theo ti le -> bui chan (gravity -0,04 = boc len) bay cao x1,47 (menu 71 C3)
            m.gravityModifierMultiplier *= HeSoHinhGioLoc;
        }
        foreach (var lt in hinh.GetComponentsInChildren<Light>(true))
        {
            float tam = lt.range * HeSoHinhGioLoc;
            var fl = lt.GetComponent<LightFlicker>();
            if (fl != null) fl.DatTamGoc(tam); else lt.range = tam;
        }
        ThanGioXoan(root.transform);
        VetGioXoanLen(root.transform);
        VetSauGioLoc(root.transform);
        return root;
    }

    /// <summary>Gio loc toi hon Loc xoay bao nhieu (mau vo + bui, nguoi dung 01/10/2026 "xam den hon nua, con trang qua" - chon toi 30%).</summary>
    public const float HeSoToiGioLoc = 0.7f;

    /// <summary>
    /// GIO LOC KHAC LOC XOAY (nguoi dung 01/10/2026 lan ba, 03/10/2026):
    ///   1) BO HIEU UNG SANG: quang sang trang o mieng (HaoQuang) + den chop (StormLight); loe cham dat cua tia tat o GioLocSetTrongLoc.
    ///      Chi con 2 tia set (duong tia giu nguyen: mieng -> dat canh chan).
    ///   2) 03/10/2026 nguoi dung: "bo tat ca hieu ung bui khoi cuon tu chan loc den tan dinh, chi cho bui khoi cuon o duoi sat chan loc va
    ///      khi loc di chuyen de lai phia sau" - chon BO 3 lop cuon len than (BuiThanDuoi, BuiCuonLen, BuiThanTren); CHI GIU BuiChan.
    ///      (Lich su: 01-02/10 lop len dinh x2 + quay x2, lop than tren 240 -> 320 hat/giay sinh 5 m, prewarm - menu 95 / 95b, git 282e20f.)
    ///   3) Than + bui TOI x HeSoToiGioLoc: vo qua MaterialPropertyBlock (vat lieu M_P_LX_* la asset dung chung voi Loc xoay - KHONG sua),
    ///      bui qua mau hat.
    /// Goi TRUOC khi phong x0,318 (gia tri trong don vi Loc xoay goc). Vet sau lung: VetSauGioLoc.
    /// </summary>
    static void KhacLocXoay(Transform hinh)
    {
        // 03/10/2026 (lan hai): bo ca 4 VO PHEU + VANH cua Loc xoay - than thay bang cac DAI GIO XOAN (ThanGioXoan)
        foreach (var ten in new[] { "HaoQuang", "StormLight", "BuiThanDuoi", "BuiCuonLen", "Vo0", "Vo1", "Vo2", "Vo3", "Vanh" })
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
        }
    }

    /// <summary>Toc do quay (do/giay, nhan ChieuQuayGioLoc) va toc do anh truot LEN doc dai (uv/giay) cua 3 nhom dai trong / giua / ngoai.
    /// Quay CHAM: dai xoan cung chieu xoay ma quay nhanh thi hinh dai trong nhu troi XUONG (giao diem dai voi mot goc co dinh tut xuong
    /// w/c m/s) - luong gio di len la nho anh TRUOT doc dai.</summary>
    static readonly float[] QuayGioXoan = { 45f, 35f, 28f }, TruotGioXoan = { 1.25f, 1.05f, 0.9f };

    /// <summary>
    /// THAN CUON LEN (nguoi dung 04/10/2026: "loc chi la 1 hinh dung len va tien ve phia truoc, khong he co hieu ung cuon tu duoi len theo
    /// 1 huong nhat dinh"). Menu 97 do ban 03/10: dich chuyen bieu kien -0,04 than/giay (TROI XUONG, 0/12 cap khung len) - dai xoan CUNG
    /// chieu quay nen quay la giao diem dai voi mot goc co dinh tut xuong; anh gio soi keo doc nen truot doc dai gan nhu khong thay.
    /// Sua: dai dung lai XOAN NGUOC chieu quay (Blender guong x) -> quay la vet dai LEO LEN; anh gio co tung CUM dut doan; quay nhanh
    /// x HeSoQuayCuonGioLoc; them lop VET GIO hat bay xoan oc len (VetGioXoanLen). MucCuonGioLoc = muc toc do (1 = bang Loc xoay theo
    /// ti le than, nguoi dung chon trong 3 muc chup).
    /// </summary>
    public static float MucCuonGioLoc = 1.5f;   // nguoi dung 04/10/2026 chon muc 1,5 (anh dong menu 97b)

    /// <summary>Quay dai x bao nhieu so voi QuayGioXoan o muc 1 (hieu chinh bang menu 97 cho bang Loc xoay theo ti le than).</summary>
    public const float HeSoQuayCuonGioLoc = 4.5f;

    /// <summary>Mau 3 nhom dai: mau vo Loc xoay tuong ung (Vo0 / Vo1 / Vo2, da toi x0,8) x HeSoToiGioLoc.</summary>
    static readonly Color[] MauGioXoan = { new Color(0.86f, 0.88f, 0.92f), new Color(0.92f, 0.94f, 0.97f), new Color(0.97f, 0.98f, 1.00f) };

    static readonly Material[] matGioXoan = new Material[3];

    /// <summary>
    /// THAN GIO LOC = CAC DAI GIO XOAN (nguoi dung 03/10/2026: "phan than loc tu day den dinh thay qua ro la hinh tron, ve lai sao cho
    /// that tu nhien la cac luong gio cuon len thanh loc"; chon dung lai bang Blender MCP, chi Gio loc). Blender MCP
    /// 03/10/2026 lan ba (nguoi dung khoanh KHOANG TRONG o than / chan / tren): 12 -> 18 dai rong 0,55-0,90 m, 8 dai bat dau tu CHAN (so le
    /// 0-0,24 m, dau thon 25%), dai trai deu quanh truc theo goc vang, cung do doc ~1,7 vong / ca than; mo dau dai theo DO DAI TUYET DOI 0,35 m
    /// (truoc 22% dai -> chan thua), do dam toi da moi dai 0,62 (mau dinh) de cho chong nhau van thay tung luong. Do phu (anh truc giao,
    /// trong vien than) chan / giua / tren 87-89 / 99-100 / 86-90% (ban 12 dai 61-63 / 85-94 / 77%). Mo ta ban dau ben duoi:
    /// CongCu/Blender/gio_loc_xoan.blend (scene GioLocXoan) -> Resources/KyNang/GioLoc/GioXoan.fbx: 12 DAI XOAN OC HO (khong khep vong),
    /// 3 nhom DaiTrong / DaiGiua / DaiNgoai o 0,70-0,74 / 0,86-0,92 / 1,02-1,08 x ban kinh vo chinh Loc xoay thu nho (2,6 + 4,2 t^1,6) x 0,318;
    /// moi dai bat dau / ket thuc o do cao khac nhau (dinh so le 0,80-1,03 than), 0,6-1,4 vong, rong 0,42-0,72 m thon hai dau, duong tam luon
    /// song, mat cat NGHIENG theo chieu xoan (dung thang thi o mep than hien vach doc), ban kinh lech 7% + 4% theo goc; do trong o mau dinh
    /// mo 22% hai dau dai + 0,25 m sat dat. Anh GioXoan.png (Blender MCP, scene GioXoanAnh): soi gio keo doc, mep rach, LIEN MACH theo v
    /// (nhieu tren duong tron). UV: u ngang dai, v doc dai (tang theo chieu len) -> ScrollUV v AM = gio chay len. Dai khong nam duoi
    /// LocXoayHinh nen KHONG an ti le 0,318 - luoi dung o met that.
    /// </summary>
    static void ThanGioXoan(Transform goc)
    {
        var kho = Resources.Load<GameObject>(ThuMucGioLoc + "GioXoan");
        if (kho == null) return;
        var than = new GameObject("GioXoan");
        than.transform.SetParent(goc, false);
        var anh = Resources.Load<Texture2D>(ThuMucGioLoc + "GioXoan");
        string[] ten = { "DaiTrong", "DaiGiua", "DaiNgoai" };
        foreach (var mf in kho.GetComponentsInChildren<MeshFilter>(true))
        {
            int i = System.Array.IndexOf(ten, mf.name);
            if (i < 0) continue;
            // Kiem bang null cua Unity: vat lieu tao luc Play bi xoa khi thoat Play
            if (matGioXoan[i] == null)
            {
                var c = MauGioXoan[i] * (HeSoToiLocXoay * HeSoToiGioLoc); c.a = 1f;
                matGioXoan[i] = Mats.Alpha("P_GioXoan_" + mf.name, anh, c);
            }
            var go = new GameObject(mf.name);
            go.transform.SetParent(than.transform, false);
            go.transform.localPosition = mf.transform.localPosition;
            go.transform.localRotation = mf.transform.localRotation;
            go.transform.localScale = mf.transform.localScale;
            go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = matGioXoan[i];
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            var sp = go.AddComponent<Spin>();
            sp.axis = Vector3.up;
            sp.degreesPerSecond = ChieuQuayGioLoc * QuayGioXoan[i] * HeSoQuayCuonGioLoc * MucCuonGioLoc;
            var sc = go.AddComponent<ScrollUV>();
            sc.speed = new Vector2(0f, -TruotGioXoan[i]);
        }
    }

    /// <summary>Toc vet gio bay LEN (m/s) va quay quanh truc (rad/s) o muc 1; nhan MucCuonGioLoc.</summary>
    public const float TocLenVetGio = 1.0f, QuayVetGio = 2.6f;

    /// <summary>So vet gio moi giay va do dai vet (giay).</summary>
    public const float SoVetGioMoiGiay = 14f, GiayDuoiVetGio = 0.35f;

    /// <summary>Vet gio sang hon dai gio bao nhieu lan.</summary>
    public const float SangVetGio = 1.4f;

    static Material matVetGio;

    /// <summary>
    /// VET GIO BAY XOAN OC LEN quanh than (nguoi dung 04/10/2026 chon "dai gio + vet gio hat", khong phai bui khoi): hat KHONG ve, chi
    /// ve DUOI (Trails) bang anh VetGio.png (Blender MCP CongCu/Blender/gio_loc_xoan.blend, scene VetGioAnh: soi gio keo dai, dau day,
    /// duoi mo - luu lat ngang de dau o u = 0 nhu Trails). Quy dao dung lai BuiCuonLenTheoThan: sinh tren vong o chan sat than, bay len
    /// deu, dat ra theo dung cong thuc ban kinh than (x1,08 - ngay ngoai nhom dai ngoai), quay CUNG chieu loc. Cuc bo (duoi khong bi keo
    /// dai khi loc bay 9,5 m/s). Lap + prewarm: co vet tren ca than ngay luc tung.
    /// </summary>
    static void VetGioXoanLen(Transform goc)
    {
        float k = HeSoHinhGioLoc, cao = ChieuCaoLocXoayHinh * k * (15f / 15.72f);
        float len = TocLenVetGio * MucCuonGioLoc, songTB = cao / len;
        // vet sang hon dai x SangVetGio: cung toi nhu dai thi ban ngay chim vao nen dat (anh menu 97b)
        var c = MauGioXoan[2] * (HeSoToiLocXoay * HeSoToiGioLoc * SangVetGio); c.a = 0.9f;
        var ps = BuiCuonLenTheoThan(goc, "VetGioXoan", cao, h => 1.08f * BanKinhLocXoay(h, k), SoVetGioMoiGiay * MucCuonGioLoc,   // doi hat ngan lai theo muc -> giu so vet
                                    0.85f * songTB, 1.15f * songTB, 0.22f, 0.40f, QuayVetGio * MucCuonGioLoc, c, c);
        var m = ps.main; m.loop = true; m.prewarm = true;
        var ts = ps.textureSheetAnimation; ts.enabled = false;
        var ro = ps.rotationOverLifetime; ro.enabled = false;
        var tr = ps.trails;
        tr.enabled = true;
        tr.mode = ParticleSystemTrailMode.PerParticle;
        tr.worldSpace = false;
        tr.lifetime = new ParticleSystem.MinMaxCurve(Mathf.Clamp01(GiayDuoiVetGio / (1.15f * songTB)), Mathf.Clamp01(GiayDuoiVetGio / (0.85f * songTB)));
        tr.minVertexDistance = 0.08f;
        tr.textureMode = ParticleSystemTrailTextureMode.Stretch;
        tr.sizeAffectsWidth = true;
        tr.inheritParticleColor = true;
        tr.dieWithParticles = true;
        tr.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.35f)));
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.None;
        // Kiem bang null cua Unity: vat lieu tao luc Play bi xoa khi thoat Play
        if (matVetGio == null) matVetGio = Mats.Alpha("P_VetGio", Resources.Load<Texture2D>(ThuMucGioLoc + "VetGio"), Color.white);
        r.trailMaterial = matVetGio;
    }

    /// <summary>Vet sau lung Gio loc song bao lau (giay) - nguoi dung 03/10/2026 chon ~2 s (loc bay 9,5 m/s -> vet ~15-20 m).</summary>
    public const float GiayVetGioLoc = 2f;

    /// <summary>Hat moi MET loc di duoc: bui xam / khoi den xam. Theo QUANG DUONG (rateOverDistance) - loc dung yen khong de vet.</summary>
    public const float HatMoiMetBuiVet = 6f, HatMoiMetKhoiVet = 4f;

    /// <summary>
    /// VET SAU LUNG GIO LOC (nguoi dung 03/10/2026: "bui khoi cuon ... khi loc di chuyen de lai phia sau" + "them ca bui khoi den xam de lai
    /// phia sau moi khi loc di qua"; chon HAI LOP + ~2 s). Hai he hat KHONG GIAN THE GIOI, con truc tiep cua goc hinh (khong nam duoi
    /// LocXoayHinh nen khong an ti le 0,318 - so do o day la met that), sinh theo QUANG DUONG:
    ///   - VetBuiXam: bui xam y bui chan Gio loc (anh BuiXam 2x2, mau bui Loc xoay x0,7), vong 1,27 m (= vong bui chan 4 x 0,318), ha
    ///     dan tai cho roi tan.
    ///   - VetKhoiDen: khoi DEN XAM (flipbook KhoiCuon), boc len cham, no to roi tan.
    /// Vong phun Circle mac dinh DUNG trong mat XY -> xoay -90 quanh X (bai hoc bui chan Loc xoay 29/09/2026).
    /// GioLoc.Tan tat phat hat moi he trong hinh (ke ca hai he nay) va giu vat them doi hat dai nhat -> vet tan tu nhien.
    /// </summary>
    static void VetSauGioLoc(Transform goc)
    {
        float k = HeSoHinhGioLoc;
        Color t0 = MauBuiXamToi, t1 = MauBuiXamSang;
        t0.r *= HeSoToiGioLoc; t0.g *= HeSoToiGioLoc; t0.b *= HeSoToiGioLoc;
        t1.r *= HeSoToiGioLoc; t1.g *= HeSoToiGioLoc; t1.b *= HeSoToiGioLoc;

        var bui = NewPS("VetBuiXam", goc, new Vector3(0f, 0.15f, 0f), BuiXamMat, ParticleSystemRenderMode.Billboard);
        DatKhungBuiXam(bui);
        var m = bui.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.8f * GiayVetGioLoc, 1.1f * GiayVetGioLoc);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        m.startSize = new ParticleSystem.MinMaxCurve(2.6f * k, 5.5f * k);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startColor = new ParticleSystem.MinMaxGradient(t0, t1);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.scalingMode = ParticleSystemScalingMode.Local;
        m.maxParticles = 220;
        m.gravityModifier = -0.02f;
        var e = bui.emission; e.rateOverTime = 0f; e.rateOverDistance = HatMoiMetBuiVet;
        var sh = bui.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = BanKinhVongBuiLocXoay * k; sh.radiusThickness = 1f;
        sh.rotation = new Vector3(-90f, 0f, 0f);
        var col = bui.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, new Color(0.85f, 0.86f, 0.9f), 1f, 0f, 0.85f, 0.55f, 0f));
        var sz = bui.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.8f), new Keyframe(1f, 1.6f)));
        var rot = bui.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        var matKhoi = KhoiCuonMat;
        var khoi = NewPS("VetKhoiDen", goc, new Vector3(0f, 0.3f, 0f), matKhoi != null ? matKhoi : SmokeMat, ParticleSystemRenderMode.Billboard);
        if (matKhoi != null) BatFlipbook(khoi, 6, 6, 1);
        var mk = khoi.main;
        mk.startLifetime = new ParticleSystem.MinMaxCurve(0.8f * GiayVetGioLoc, 1.1f * GiayVetGioLoc);
        mk.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.7f);
        mk.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
        mk.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mk.startColor = new ParticleSystem.MinMaxGradient(new Color(0.16f, 0.16f, 0.17f, 0.75f), new Color(0.28f, 0.28f, 0.29f, 0.90f));
        mk.simulationSpace = ParticleSystemSimulationSpace.World;
        mk.scalingMode = ParticleSystemScalingMode.Local;
        mk.maxParticles = 160;
        mk.gravityModifier = -0.05f;                // boc len cham
        var ek = khoi.emission; ek.rateOverTime = 0f; ek.rateOverDistance = HatMoiMetKhoiVet;
        var shk = khoi.shape; shk.shapeType = ParticleSystemShapeType.Circle; shk.radius = 0.9f; shk.radiusThickness = 1f;
        shk.rotation = new Vector3(-90f, 0f, 0f);
        var ck = khoi.colorOverLifetime; ck.enabled = true;
        ck.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, new Color(0.85f, 0.85f, 0.85f), 0.4f, new Color(0.6f, 0.6f, 0.6f), 1f, 0f, 0.8f, 0.55f, 0f));
        var szk = khoi.sizeOverLifetime; szk.enabled = true;
        szk.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(1f, 1.8f)));
        var rk = khoi.rotationOverLifetime; rk.enabled = true; rk.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
    }

    /// <summary>So nhip set trong loc da phong (moi nhip 2 tia) - phep thu menu 71 doc.</summary>
    public static int SoNhipSetTrongGioLoc;

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
