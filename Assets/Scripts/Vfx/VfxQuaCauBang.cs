using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "QUA CAU BANG" (them 16/09/2026).
///
/// Nguoi dung: ba qua cau bang bay nhu Qua cau lua, nhung "khac han la tao ra mot luong
/// khong khi lanh phia sau va vet bang cua qua cau". Tai nguyen dung trong Blender qua
/// MCP (CongCu/Blender/qua_cau_bang.blend), nam o Resources/KyNang/QuaCauBang:
///   - QuaCauBang.fbx : loi pha le nhieu mat, gai bang toa quanh than, gai phia sau dai
///                      chum lai nhu duoi sao choi (truc bay = +Z trong Unity);
///   - SuongLanh.png  : dam suong lanh cuon (nhieu fBm, tat dan ra mep) - luong khi lanh;
///   - ManhBang.png   : manh tinh the bang co tia sang cheo - vet bang lap lanh roi xuong.
/// Icon nut: Resources/Icons/CauBang.png (cung canh Blender).
///
/// Khong dung prefab trong GameAssets: them truong prefab la phai sua ca hai scene. Dung
/// bang code nhu duong du phong cua Qua cau lua.
/// </summary>
public static partial class VfxFactory
{
    const string ThuMucQuaCauBang = "KyNang/QuaCauBang/";

    static Mesh luoiQuaCauBang;
    static bool daTimLuoiQuaCauBang;
    static Material mSuongLanh, mManhBang, mVetBang, mHaoQuangCauBang, mLoiCauBang;

    /// <summary>
    /// Vat lieu loi pha le: ban sao Mats.Ice nhung PHAT SANG IT va xanh sau hon. Dung thang Mats.Ice
    /// (glow 1,35) thi ba qua cau + bloom ra ba dom trang chop, khong doc ra mat pha le xanh.
    /// </summary>
    static Material LoiCauBangMat
    {
        get
        {
            if (mLoiCauBang == null)
            {
                mLoiCauBang = new Material(Mats.Ice);
                mLoiCauBang.name = "LoiCauBang";
                mLoiCauBang.SetColor("_Color", new Color(0.36f, 0.66f, 1f, 0.75f));
                mLoiCauBang.SetColor("_DeepColor", new Color(0.04f, 0.22f, 0.62f, 1f));
                mLoiCauBang.SetColor("_RimColor", new Color(0.70f, 0.92f, 1f, 1f));
                mLoiCauBang.SetFloat("_Glow", 0.55f);
            }
            return mLoiCauBang;
        }
    }

    /// <summary>Luoi Blender - null neu khong nap duoc (luc do dung khoi cau tron).</summary>
    public static Mesh LuoiQuaCauBang
    {
        get
        {
            if (!daTimLuoiQuaCauBang)
            {
                daTimLuoiQuaCauBang = true;
                var go = Resources.Load<GameObject>(ThuMucQuaCauBang + "QuaCauBang");
                var mf = go != null ? go.GetComponentInChildren<MeshFilter>() : null;
                luoiQuaCauBang = mf != null ? mf.sharedMesh : null;
            }
            return luoiQuaCauBang;
        }
    }

    /// <summary>Dam suong lanh (trong suot, khong cong sang: suong lanh la hoi mo chu khong phai lua).</summary>
    public static Material SuongLanhMat
    {
        get
        {
            if (mSuongLanh == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucQuaCauBang + "SuongLanh");
                mSuongLanh = Mats.Alpha("P_SuongLanh", t != null ? t : TextureFactory.Smoke(),
                                        new Color(0.66f, 0.84f, 1f, 0.72f));
            }
            return mSuongLanh;
        }
    }

    /// <summary>Manh bang lap lanh (cong sang).</summary>
    public static Material ManhBangMat
    {
        get
        {
            if (mManhBang == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucQuaCauBang + "ManhBang");
                mManhBang = Mats.Additive("P_ManhBang", t != null ? t : TextureFactory.IceShard(),
                                          new Color(0.80f, 0.95f, 1f, 1f), 2.0f);
            }
            return mManhBang;
        }
    }

    static Material VetBangMat
    {
        get
        {
            if (mVetBang == null)
                mVetBang = Mats.Additive("P_VetBang", TextureFactory.SoftDot(1.1f), new Color(0.55f, 0.82f, 1f, 1f), 1.35f);
            return mVetBang;
        }
    }

    static Material HaoQuangCauBangMat
    {
        get
        {
            if (mHaoQuangCauBang == null)
                mHaoQuangCauBang = Mats.Additive("P_HaoQuangCauBang", TextureFactory.SoftDot(1.8f), new Color(0.40f, 0.70f, 1f, 0.45f), 0.8f);
            return mHaoQuangCauBang;
        }
    }

    /// <summary>Toan bo phan nhin thay duoc cua MOT qua cau bang dang bay (truc bay = +Z cua parent).</summary>
    public static void BuildQuaCauBangVisual(Transform parent, float radius)
    {
        // 1) LOI PHA LE tu Blender. Luoi dung o ban kinh loi 0,5 -> phong theo radius.
        var luoi = LuoiQuaCauBang;
        float k = radius / 0.5f;
        GameObject loi;
        // Xoay 180 do quanh truc dung: truc duoi gai trong Blender la -Y, qua FBX thanh +Z
        // (do bang bounds: tam luoi lech +0,75 theo Z) - khong xoay thi duoi chi ve phia truoc.
        if (luoi != null)
            loi = ProcMesh.Part("LoiBang", parent, luoi, LoiCauBangMat, Vector3.zero, Quaternion.Euler(0f, 180f, 0f), Vector3.one * k, false);
        else
            loi = ProcMesh.Part("LoiBang", parent, ProcMesh.Sphere(radius, 12, 8, 1f, Color.white), Mats.Ice,
                                Vector3.zero, Quaternion.identity, Vector3.one, false);
        // Quay CHAM quanh truc bay: gai duoi van chum ve phia sau, chi lap lanh doi mat
        var xoay = loi.AddComponent<Spin>();
        xoay.axis = Vector3.forward;
        xoay.degreesPerSecond = 160f;

        // 2) Hao quang lanh bam quanh qua cau
        var hq = NewPS("HaoQuang", parent, Vector3.zero, HaoQuangCauBangMat, ParticleSystemRenderMode.Billboard);
        var hm = hq.main;
        hm.startLifetime = 0.18f; hm.startSpeed = 0f;
        // Nho va mo (lan dau 4-5 lan ban kinh, do 1,25: ba qua + bloom nhoe thanh mot dom trang, mat ca hinh pha le)
        hm.startSize = new ParticleSystem.MinMaxCurve(radius * 2.2f, radius * 2.8f);
        hm.simulationSpace = ParticleSystemSimulationSpace.Local; hm.maxParticles = 8;
        var hem = hq.emission; hem.rateOverTime = 16f;
        var hsh = hq.shape; hsh.enabled = false;

        // 3) LUONG KHONG KHI LANH phia sau: dam suong troi cham, to dan, hoi chim xuong
        //    (khi lanh nang hon khong khi) - khac han khoi den bay len cua qua cau lua.
        var suong = NewPS("LuongKhiLanh", parent, new Vector3(0f, 0f, -radius * 0.6f), SuongLanhMat, ParticleSystemRenderMode.Billboard);
        var sm = suong.main;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
        sm.startSize = new ParticleSystem.MinMaxCurve(radius * 1.8f, radius * 2.8f);
        sm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.gravityModifier = 0.06f;
        sm.maxParticles = 160;
        var sem = suong.emission; sem.rateOverTime = 70f;
        var ssh = suong.shape; ssh.shapeType = ParticleSystemShapeType.Sphere; ssh.radius = radius * 0.6f;
        var scol = suong.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.92f, 0.98f, 1f), 0f,
            new Color(0.62f, 0.82f, 1f), 0.45f,
            new Color(0.40f, 0.62f, 0.95f), 1f,
            0f, 0.85f, 0.45f, 0f));
        var ssol = suong.sizeOverLifetime; ssol.enabled = true;
        ssol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.55f, 1.15f, 1.9f));
        var srot = suong.rotationOverLifetime; srot.enabled = true;
        srot.z = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);

        // 4) VET BANG: dai sang lanh keo dai theo duong bay
        var vet = new GameObject("VetBang");
        vet.transform.SetParent(parent, false);
        var tr = vet.AddComponent<TrailRenderer>();
        tr.time = 0.42f;
        tr.minVertexDistance = 0.12f;
        tr.widthCurve = new AnimationCurve(new Keyframe(0f, radius * 1.1f), new Keyframe(1f, 0f));
        tr.material = VetBangMat;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(new Color(0.90f, 0.98f, 1f), 0f), new GradientColorKey(new Color(0.35f, 0.62f, 1f), 1f) },
                  new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.45f, 0.4f), new GradientAlphaKey(0f, 1f) });
        tr.colorGradient = g;
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tr.receiveShadows = false;

        // 5) MANH BANG lap lanh vang ra sau va roi xuong
        var manh = NewPS("ManhBangRoi", parent, Vector3.zero, ManhBangMat, ParticleSystemRenderMode.Billboard);
        var mm = manh.main;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.4f);
        mm.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.24f);
        mm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        mm.gravityModifier = 0.55f;
        mm.maxParticles = 120;
        var mem = manh.emission; mem.rateOverTime = 36f;
        var msh = manh.shape; msh.shapeType = ParticleSystemShapeType.Sphere; msh.radius = radius * 0.8f;
        var mcol = manh.colorOverLifetime; mcol.enabled = true;
        mcol.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 1f, 1f, 0.7f, 0f));
        var mrot = manh.rotationOverLifetime; mrot.enabled = true;
        mrot.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

        // 6) Anh sang lanh hat xuong mat dat
        var lightGo = new GameObject("AnhSangLanh");
        lightGo.transform.SetParent(parent, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.45f, 0.72f, 1f);
        lt.intensity = 2.4f;
        lt.range = 7f;
        lt.shadows = LightShadows.None;
    }

    /// <summary>
    /// Qua cau no / het tam: THA luong khi lanh va vet bang ra de chung tan dan tai cho,
    /// thay vi bien mat cung qua cau (hat he World + TrailRenderer mat ngay khi xoa cha).
    /// </summary>
    public static void ThaDuoiQuaCauBang(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var c = parent.GetChild(i);
            var ps = c.GetComponent<ParticleSystem>();
            var tr = c.GetComponent<TrailRenderer>();
            if (ps == null && tr == null) continue;
            if (ps != null && c.name == "HaoQuang") continue;
            c.SetParent(null, true);
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (tr != null) tr.emitting = false;
            AutoDestroy.Add(c.gameObject, 1.3f);
        }
    }

    /// <summary>Vu no bang cua qua cau: gai bang tu dat + vong lanh (IceImpact) + bung suong va manh bang.</summary>
    public static void NoQuaCauBang(Vector3 pos, float radius)
    {
        IceImpact(pos, radius * 0.75f);

        var root = new GameObject("NoQuaCauBang");
        root.transform.position = pos;
        AutoDestroy.Add(root, 2.6f);

        var suong = NewPS("SuongBung", root.transform, Vector3.zero, SuongLanhMat, ParticleSystemRenderMode.Billboard);
        var sm = suong.main;
        sm.loop = false; sm.duration = 0.2f;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(radius * 0.6f, radius * 1.5f);
        sm.startSize = new ParticleSystem.MinMaxCurve(radius * 0.7f, radius * 1.2f);
        sm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.maxParticles = 40;
        var sem = suong.emission; sem.rateOverTime = 0f;
        sem.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });
        var ssh = suong.shape; ssh.shapeType = ParticleSystemShapeType.Hemisphere; ssh.radius = radius * 0.3f;
        ssh.rotation = new Vector3(-90f, 0f, 0f);
        var slim = suong.limitVelocityOverLifetime; slim.enabled = true; slim.limit = 0.3f; slim.dampen = 0.12f;
        var scol = suong.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.95f, 0.99f, 1f), 0f, new Color(0.62f, 0.82f, 1f), 0.5f, new Color(0.42f, 0.62f, 0.95f), 1f,
            0f, 0.9f, 0.4f, 0f));
        var ssol = suong.sizeOverLifetime; ssol.enabled = true;
        ssol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.6f, 1.2f, 1.7f));

        var manh = NewPS("ManhBung", root.transform, Vector3.up * 0.4f, ManhBangMat, ParticleSystemRenderMode.Billboard);
        var mm = manh.main;
        mm.loop = false; mm.duration = 0.2f;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(radius * 1.6f, radius * 3.2f);
        mm.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        mm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        mm.gravityModifier = 1.4f;
        mm.maxParticles = 60;
        var mem = manh.emission; mem.rateOverTime = 0f;
        mem.SetBursts(new[] { new ParticleSystem.Burst(0f, 38) });
        var msh = manh.shape; msh.shapeType = ParticleSystemShapeType.Sphere; msh.radius = 0.2f;
        var mcol = manh.colorOverLifetime; mcol.enabled = true;
        mcol.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.6f, Color.white, 1f, 1f, 1f, 0.8f, 0f));
        var mrot = manh.rotationOverLifetime; mrot.enabled = true;
        mrot.z = new ParticleSystem.MinMaxCurve(-8f, 8f);

        // Vet suong gia luon de lai tren dat (IceImpact chi de 35%)
        GroundDecal.Spawn(new Vector3(pos.x, GroundY(pos), pos.z), radius * 0.9f, new Material(FrostMat), 5f, 2.5f);
    }
}
