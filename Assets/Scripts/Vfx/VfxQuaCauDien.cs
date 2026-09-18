using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "QUA CAU DIEN" (ky nang 13, them 18/09/2026).
///
/// Nguoi dung gui anh Diablo III: mot QUA CAU DIEN xanh lam lo lung, mat cau go ghe va co
/// nhung cung dien cuon quanh, thinh thoang BAN CAC TIA DIEN ra moi ke dich chung quanh.
///
/// Tai nguyen dung bang Blender MCP (CongCu/Blender/qua_cau_dien.blend) -> Resources/KyNang/QuaCauDien:
///   - CauDien.fbx      : LoiCauDien (khoi cau go ghe) + VoDienCauDien (cung dien cuon quanh + gai toe ra);
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

    static Mesh luoiLoiCauDien, luoiVoCauDien;
    static bool daTimLuoiCauDien;
    static Material mLoiCauDien, mVoCauDien, mHaoQuangDien, mHatDien, mTiaDienVet;

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
            else if (mf.name.StartsWith("Vo")) luoiVoCauDien = mf.sharedMesh;
        }
    }

    /// <summary>Khoi cau go ghe o giua (luoi Blender; null thi dung khoi cau tron cua Unity).</summary>
    public static Mesh LuoiLoiCauDien { get { TimLuoiCauDien(); return luoiLoiCauDien; } }
    /// <summary>Cac cung dien cuon quanh cau + gai toe ra.</summary>
    public static Mesh LuoiVoCauDien { get { TimLuoiCauDien(); return luoiVoCauDien; } }

    public static Material LoiCauDienMat
    {
        get
        {
            if (mLoiCauDien == null)
                mLoiCauDien = Mats.Additive("P_LoiCauDien", TextureFactory.SoftDot(1.3f), new Color(0.20f, 0.46f, 1f, 1f), 1.15f);
            return mLoiCauDien;
        }
    }

    public static Material VoCauDienMat
    {
        get
        {
            if (mVoCauDien == null)
                mVoCauDien = Mats.Additive("P_VoCauDien", TextureFactory.SoftDot(1.3f), new Color(0.62f, 0.84f, 1f, 1f), 2.1f);
            return mVoCauDien;
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
                                              new Color(0.34f, 0.62f, 1f, 1f), 1.25f);
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

        // 2) VO DIEN - quay NGUOC chieu loi cho ra cam giac dien cuon
        if (luoiVoCauDien != null)
        {
            var vo = new GameObject("VoDien");
            vo.transform.SetParent(goc.transform, false);
            vo.transform.localScale = Vector3.one * banKinh * 1.04f;
            vo.AddComponent<MeshFilter>().sharedMesh = luoiVoCauDien;
            vo.AddComponent<MeshRenderer>().sharedMaterial = VoCauDienMat;
            var quayV = vo.AddComponent<Spin>();
            quayV.axis = new Vector3(-0.15f, 1f, 0.25f).normalized;
            quayV.degreesPerSecond = -120f;
        }

        // 3) HAO QUANG: tam anh luon quay mat ve may quay.
        // Tam nay la CON cua mot vat co Billboard - Billboard xoay CHA, con tam thi tu quay quanh truc
        // cua chinh no, nen van huong ve may quay ma van thay van dien chay vong.
        var neo = new GameObject("NeoHaoQuang");
        neo.transform.SetParent(goc.transform, false);
        neo.AddComponent<Billboard>();
        var hq = ProcMesh.Part("HaoQuang", neo.transform, GroundDecal.QuadMesh(), HaoQuangDienMat,
                               Vector3.zero, Quaternion.identity, Vector3.one * banKinh * 3.4f, false);
        var xoay = hq.AddComponent<Spin>();
        xoay.axis = Vector3.forward;
        xoay.degreesPerSecond = 26f;

        // 4) HAT DIEN bay quanh cau
        var hat = NewPS("HatDien", goc.transform, Vector3.zero, HatDienMat, ParticleSystemRenderMode.Billboard);
        var hm = hat.main;
        hm.duration = song; hm.loop = true;
        hm.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);
        hm.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
        hm.startSize = new ParticleSystem.MinMaxCurve(banKinh * 0.14f, banKinh * 0.38f);
        hm.simulationSpace = ParticleSystemSimulationSpace.World;
        hm.maxParticles = 160;
        var hem = hat.emission; hem.rateOverTime = 70f;
        var hsh = hat.shape; hsh.shapeType = ParticleSystemShapeType.Sphere; hsh.radius = banKinh * 1.15f;
        var hcol = hat.colorOverLifetime; hcol.enabled = true;
        hcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.62f, 0.86f, 1f), 0.5f,
            new Color(0.22f, 0.50f, 1f), 1f,
            0f, 1f, 0.85f, 0f));

        // 5) DEN XANH nhap nhay
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

    /// <summary>Mot tia dien ban tu qua cau toi ke dich - mau xanh lam ruc nhu trong anh.</summary>
    public static LightningArc TiaCauDien(Vector3 tu, Vector3 den)
    {
        var arc = LightningArc.Create(tu, den, 1.35f, 0.22f);
        arc.segments = 16;
        arc.branches = Random.Range(1, 3);
        arc.jitter = 0.7f;
        // Phai to mau NGAY DAY: LightningArc to mau trong Start, doi o khung sau thi tia loe sai mau mot khung
        arc.coreColor = new Color(1f, 1f, 1f, 1f);
        arc.glowColor = new Color(0.32f, 0.66f, 1f, 1f);
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
