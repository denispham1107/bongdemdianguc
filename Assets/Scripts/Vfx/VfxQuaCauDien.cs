using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "QUA CAU DIEN" (ky nang 13, them 18/09/2026).
///
/// Nguoi dung gui anh Diablo III: mot QUA CAU DIEN xanh lam lo lung, mat cau go ghe va co
/// nhung cung dien cuon quanh, thinh thoang BAN CAC TIA DIEN ra moi ke dich chung quanh.
///
/// Tai nguyen dung bang Blender MCP (CongCu/Blender/qua_cau_dien.blend) -> Resources/KyNang/QuaCauDien:
///   - CauDien.fbx      : LoiCauDien (khoi cau, TOI mau nhu trong anh) + VanhSangCauDien (vo cau MIN lam VANH SANG
///                        boc ngoai) + VoDienCauDien (cac DUONG GAN TRANG lien mach vong quanh - KHONG DUNG NUA,
///                        thay bang VoTiaDien.fbx ben duoi);
///   - VoTiaDien.fbx    : BON KHUNG tia dien (VoTia0..3). Moi khung la CAC DOAN TIA DIEN RAT MONG (ong ban kinh
///                        0,016 so voi cau ban kinh 1) chay theo DUNG NAM DUONG VONG cu, nhung DUT QUANG - moi
///                        duong ngat thanh nhieu doan ngan co khoang ho, hai dau doan thuon nhon. Nguoi dung
///                        18/09/2026: "thay cac duong van trang thanh cac tia dien mong bao boc xung quanh qua cau
///                        giong nhu cac duong van trang hien gio (khong ve lien mach nhe)" + chon CHOP TAT LIEN TUC.
///                        Bon khung dung chung nam duong nen doi khung nhin ra "dien chay", khong nhay lung tung.
///   - HaoQuangDien.png : vang sang tron co van dien toa ra - hao quang boc quanh cau;
///   - TiaDien.png      : mot soi tia dien luon song, dung cho vet tia;
///   - HatDien.png      : dom sang bon canh - hat dien bay quanh cau.
/// Icon nut: Resources/Icons/CauDien.png (cung canh Blender).
///
/// Dung bang code, KHONG co prefab trong GameAssets (them truong prefab la phai sua ca hai scene) -
/// cung luat voi Qua cau bang.
/// </summary>
public static partial class VfxFactory
{
    const string ThuMucCauDien = "KyNang/QuaCauDien/";

    static Mesh luoiLoiCauDien, luoiVoCauDien, luoiVanhCauDien;
    static Mesh[] khungTiaDien;
    static bool daTimLuoiCauDien;
    static Material mLoiCauDien, mVoCauDien, mVanhCauDien, mHaoQuangDien, mHatDien, mTiaDienVet;

    static void TimLuoiCauDien()
    {
        if (daTimLuoiCauDien) return;
        daTimLuoiCauDien = true;
        var go = Resources.Load<GameObject>(ThuMucCauDien + "CauDien");
        if (go == null) return;
        foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (mf == null || mf.sharedMesh == null) continue;
            if (mf.name.StartsWith("Loi")) luoiLoiCauDien = mf.sharedMesh;
            else if (mf.name.StartsWith("Vanh")) luoiVanhCauDien = mf.sharedMesh;
            else if (mf.name.StartsWith("Vo")) luoiVoCauDien = mf.sharedMesh;
        }

        // Bon khung tia dien dut quang (VoTiaDien.fbx)
        var goTia = Resources.Load<GameObject>(ThuMucCauDien + "VoTiaDien");
        if (goTia != null)
        {
            var ds = new System.Collections.Generic.List<Mesh>();
            foreach (var mf in goTia.GetComponentsInChildren<MeshFilter>())
                if (mf != null && mf.sharedMesh != null && mf.name.StartsWith("VoTia")) ds.Add(mf.sharedMesh);
            // Xep theo ten (VoTia0..3) de thu tu chop on dinh giua cac lan chay
            ds.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            if (ds.Count > 0) khungTiaDien = ds.ToArray();
        }
    }

    /// <summary>Bon khung tia dien dut quang - doi qua lai de thanh "chop tat".</summary>
    public static Mesh[] KhungTiaDien { get { TimLuoiCauDien(); return khungTiaDien; } }

    /// <summary>Khoi cau go ghe o giua (luoi Blender; null thi dung khoi cau tron cua Unity).</summary>
    public static Mesh LuoiLoiCauDien { get { TimLuoiCauDien(); return luoiLoiCauDien; } }
    /// <summary>Cac cung dien cuon quanh cau + gai toe ra.</summary>
    public static Mesh LuoiVoCauDien { get { TimLuoiCauDien(); return luoiVoCauDien; } }
    /// <summary>Vo cau mong lam vanh sang boc ngoai loi.</summary>
    public static Mesh LuoiVanhCauDien { get { TimLuoiCauDien(); return luoiVanhCauDien; } }

    public static Material LoiCauDienMat
    {
        get
        {
            // Loi TOI (anh nguoi dung gui lan hai: giua qua cau la mot khoi xanh sam, khong phai dom trang)
            if (mLoiCauDien == null)
                mLoiCauDien = Mats.Additive("P_LoiCauDien", TextureFactory.SoftDot(1.3f), new Color(0.07f, 0.17f, 0.55f, 1f), 0.5f);
            return mLoiCauDien;
        }
    }

    public static Material VoCauDienMat
    {
        get
        {
            if (mVoCauDien == null)
                // Tia mong hon gan cu nhieu lan -> phai sang hon moi noi len tren nen qua cau
                mVoCauDien = Mats.Additive("P_VoCauDien", TextureFactory.SoftDot(1.3f), new Color(0.80f, 0.93f, 1f, 1f), 3.2f);
            return mVoCauDien;
        }
    }

    /// <summary>Vanh sang boc ngoai loi - cai vong xanh ruc trong anh.</summary>
    public static Material VanhCauDienMat
    {
        get
        {
            if (mVanhCauDien == null)
                mVanhCauDien = Mats.Additive("P_VanhCauDien", TextureFactory.SoftDot(1.3f), new Color(0.22f, 0.54f, 1f, 1f), 0.95f);
            return mVanhCauDien;
        }
    }

    public static Material HaoQuangDienMat
    {
        get
        {
            if (mHaoQuangDien == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucCauDien + "HaoQuangDien");
                mHaoQuangDien = Mats.Additive("P_HaoQuangDien", t != null ? t : TextureFactory.GlowPool(),
                                              new Color(0.30f, 0.58f, 1f, 1f), 0.85f);
            }
            return mHaoQuangDien;
        }
    }

    public static Material HatDienMat
    {
        get
        {
            if (mHatDien == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucCauDien + "HatDien");
                mHatDien = Mats.Additive("P_HatDien", t != null ? t : TextureFactory.SoftDot(2.2f),
                                         new Color(0.75f, 0.92f, 1f, 1f), 2.6f);
            }
            return mHatDien;
        }
    }

    /// <summary>Vet tia dien (anh Blender) - dung cho duong tia ban ra ke dich.</summary>
    public static Material TiaDienVetMat
    {
        get
        {
            if (mTiaDienVet == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucCauDien + "TiaDien");
                mTiaDienVet = Mats.Additive("P_TiaDienVet", t != null ? t : TextureFactory.BoltStrip(),
                                            new Color(0.62f, 0.86f, 1f, 1f), 2.6f);
            }
            return mTiaDienVet;
        }
    }

    /// <summary>
    /// Dung QUA CAU DIEN lo lung: loi cau go ghe + vo dien cuon quanh (cả hai quay nguoc chieu nhau),
    /// hao quang, hat dien bay quanh va mot ngon den xanh nhap nhay.
    /// </summary>
    public static GameObject QuaCauDienHinh(Vector3 cho, float banKinh, float song)
    {
        var goc = new GameObject("HinhQuaCauDien");
        goc.transform.position = cho;
        AutoDestroy.Add(goc, song + 1.2f);

        TimLuoiCauDien();

        // 1) LOI CAU
        var loi = new GameObject("LoiCau");
        loi.transform.SetParent(goc.transform, false);
        loi.transform.localScale = Vector3.one * banKinh;
        var mfL = loi.AddComponent<MeshFilter>();
        mfL.sharedMesh = luoiLoiCauDien != null ? luoiLoiCauDien : ProcMesh.Sphere(1f, Color.white);
        loi.AddComponent<MeshRenderer>().sharedMaterial = LoiCauDienMat;
        var quayL = loi.AddComponent<Spin>();
        quayL.axis = new Vector3(0.2f, 1f, 0.1f).normalized;
        quayL.degreesPerSecond = 55f;

        // 2) VANH SANG boc ngoai loi (vo cau mong) - quay cham nguoc chieu
        if (luoiVanhCauDien != null)
        {
            var vanh = new GameObject("VanhSang");
            vanh.transform.SetParent(goc.transform, false);
            vanh.transform.localScale = Vector3.one * banKinh;
            vanh.AddComponent<MeshFilter>().sharedMesh = luoiVanhCauDien;
            vanh.AddComponent<MeshRenderer>().sharedMaterial = VanhCauDienMat;
            var quayN = vanh.AddComponent<Spin>();
            quayN.axis = new Vector3(0.3f, 1f, -0.2f).normalized;
            quayN.degreesPerSecond = -38f;
        }

        // 3) TIA DIEN DUT QUANG boc ngoai - quay NGUOC chieu loi VA chop tat lien tuc.
        // Truoc day day la mot vo gan lien mach (luoiVoCauDien); nguoi dung 18/09/2026 doi thanh
        // cac doan tia mong. Doi MESH chu khong bat/tat renderer: chi mot renderer, doi sharedMesh
        // la xong - re nhat cho dien thoai.
        Mesh[] khung = khungTiaDien;
        if (khung != null && khung.Length > 0)
        {
            var vo = new GameObject("TiaDienBoc");
            vo.transform.SetParent(goc.transform, false);
            vo.transform.localScale = Vector3.one * banKinh * 1.04f;
            vo.AddComponent<MeshFilter>().sharedMesh = khung[0];
            vo.AddComponent<MeshRenderer>().sharedMaterial = VoCauDienMat;
            var quayV = vo.AddComponent<Spin>();
            quayV.axis = new Vector3(-0.15f, 1f, 0.25f).normalized;
            quayV.degreesPerSecond = -120f;
            vo.AddComponent<ChopTiaDien>().khung = khung;
        }
        else if (luoiVoCauDien != null)
        {
            // Du phong: thieu VoTiaDien.fbx thi ve lai vo lien mach cu con hon khong co gi
            var vo = new GameObject("VoDien");
            vo.transform.SetParent(goc.transform, false);
            vo.transform.localScale = Vector3.one * banKinh * 1.04f;
            vo.AddComponent<MeshFilter>().sharedMesh = luoiVoCauDien;
            vo.AddComponent<MeshRenderer>().sharedMaterial = VoCauDienMat;
            var quayV = vo.AddComponent<Spin>();
            quayV.axis = new Vector3(-0.15f, 1f, 0.25f).normalized;
            quayV.degreesPerSecond = -120f;
        }

        // KHONG co tia set toe ra lien tuc: nguoi dung xem anh trong game roi bo han (18/09/2026).
        // Tia chi ban ra khi CO KE DICH - do la viec cua QuaCauDien.BanMotLuot.

        // 4) HAO QUANG: anh sang BAO BOC ben ngoai, tam luon quay mat ve may quay.
        // Tam nay la CON cua mot vat co Billboard - Billboard xoay CHA, con tam thi tu quay quanh truc
        // cua chinh no, nen van huong ve may quay ma van thay van dien chay vong.
        var neo = new GameObject("NeoHaoQuang");
        neo.transform.SetParent(goc.transform, false);
        neo.AddComponent<Billboard>();
        var hq = ProcMesh.Part("HaoQuang", neo.transform, GroundDecal.QuadMesh(), HaoQuangDienMat,
                               Vector3.zero, Quaternion.identity, Vector3.one * banKinh * 3.0f, false);
        var xoay = hq.AddComponent<Spin>();
        xoay.axis = Vector3.forward;
        xoay.degreesPerSecond = 26f;

        // 5) HAT DIEN bay quanh cau
        var hat = NewPS("HatDien", goc.transform, Vector3.zero, HatDienMat, ParticleSystemRenderMode.Billboard);
        var hm = hat.main;
        hm.duration = song; hm.loop = true;
        hm.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);
        hm.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
        hm.startSize = new ParticleSystem.MinMaxCurve(banKinh * 0.10f, banKinh * 0.26f);
        hm.simulationSpace = ParticleSystemSimulationSpace.World;
        hm.maxParticles = 160;
        var hem = hat.emission; hem.rateOverTime = 38f;
        var hsh = hat.shape; hsh.shapeType = ParticleSystemShapeType.Sphere; hsh.radius = banKinh * 1.15f;
        var hcol = hat.colorOverLifetime; hcol.enabled = true;
        hcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.62f, 0.86f, 1f), 0.5f,
            new Color(0.22f, 0.50f, 1f), 1f,
            0f, 1f, 0.85f, 0f));

        // 6) DEN XANH nhap nhay
        var denGo = new GameObject("DenCauDien");
        denGo.transform.SetParent(goc.transform, false);
        var den = denGo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(0.42f, 0.70f, 1f);
        den.intensity = 3.4f;
        den.range = banKinh * 14f;
        den.shadows = LightShadows.None;
        var nhay = denGo.AddComponent<LightFlicker>();
        nhay.baseIntensity = 3.4f; nhay.amount = 0.45f; nhay.speed = 14f; nhay.rangeWobble = 0.08f;

        return goc;
    }

    /// <summary>Thoi gian song cua mot tia cau dien, giay. GIU NHU CU (0,22): cau ban moi 0,4 giay, tia song 0,6 giay nhu
    /// Giut set thi luot sau chong len luot truoc.</summary>
    public const float GiaySongTiaCauDien = 0.22f;

    /// <summary>
    /// Mot tia dien ban tu qua cau toi ke dich. Nguoi dung 25/09/2026: "cung co hieu ung nhu tia set cua Giut set" -
    /// dung chung GiatSet.KieuTia (anh Blender loi trang manh + vien xanh + soi re nhanh + cum bung, bam hai dau) va
    /// CUNG MAU voi Giut set. Truoc do la tia vet mo ve bang code, be ngang 1,35, mau (0,32 0,66 1).
    /// </summary>
    public static LightningArc TiaCauDien(Vector3 tu, Vector3 den, Transform bamDau = null, Transform bamCuoi = null)
    {
        var arc = LightningArc.Create(tu, den, 1.15f, GiaySongTiaCauDien);
        // Phai to mau NGAY DAY: LightningArc to mau trong Start, doi o khung sau thi tia loe sai mau mot khung
        arc.coreColor = Color.white;
        arc.glowColor = GiatSet.MauQuangNguoiChoi;
        // 1,15 = be day tia dau tien cua Giut set
        GiatSet.KieuTia(arc, 1.15f, bamDau, bamCuoi);
        return arc;
    }

    /// <summary>Cho tia dien cham vao ke dich: mot dom sang xanh loe len.</summary>
    public static GameObject ChamTiaDien(Vector3 cho)
    {
        var go = new GameObject("ChamTiaDien");
        go.transform.position = cho;
        AutoDestroy.Add(go, 1.2f);

        var ps = NewPS("Toe", go.transform, Vector3.zero, HatDienMat, ParticleSystemRenderMode.Stretch);
        var m = ps.main;
        m.duration = 0.3f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 6f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 40;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 12, 18) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.2f;

        var denGo = new GameObject("Loe");
        denGo.transform.SetParent(go.transform, false);
        var den = denGo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(0.45f, 0.75f, 1f);
        den.shadows = LightShadows.None;
        var burst = denGo.AddComponent<LightBurst>();
        burst.peak = 3.2f; burst.duration = 0.25f; burst.range = 5f;

        return go;
    }
}
