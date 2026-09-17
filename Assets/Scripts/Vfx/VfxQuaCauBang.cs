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
        BuildQuaCauBangVisual(parent, radius, false);
    }

    /// <param name="banRoi">Ban cho MUA BANG (nhieu qua cung luc tren khong): nua luong hat, khong den rieng.</param>
    public static void BuildQuaCauBangVisual(Transform parent, float radius, bool banRoi)
    {
        float heSoHat = banRoi ? 0.5f : 1f;
        float k = radius / 0.5f;
        // 1) LOI. Ban roi cua Mua bang: cau TRON KHONG GAI boc khi lanh phat sang (nguoi dung 17/09/2026: "khong can
        //    cho cac gai xung quanh, chi can cho qua cau co boc khi lanh bang phat sang"). Ky nang Qua cau bang giu gai.
        if (!(banRoi && DungCauBangTron(parent, k)))
            DungLoiCoGai(parent, radius, k);

        BuildPhanBayQuaCauBang(parent, radius, banRoi, heSoHat);
    }

    static GameObject mauCauBangTron;
    static bool daTimCauBangTron;
    static Material mKhiLanhBoc;

    /// <summary>
    /// Cau bang tron dung bang Blender MCP (CongCu/Blender/cau_bang_tron.blend -> Resources/KyNang/QuaCauBang/CauBangTron.fbx):
    /// LoiTron (r 0,5, mat hoi gon) + hai vo KhiLanh0 (r 0,66) / KhiLanh1 (r 0,84) trai anh KhiLanhBoc (soi khi lanh xoan,
    /// lien mach theo u, mo dan ve hai cuc). Hai vo cong sang (additive), quay nguoc chieu nhau tren hai truc khac nhau
    /// va truot anh - soi khi cuon quanh loi. Tra false neu khong nap duoc (luc do dung loi co gai cu).
    /// </summary>
    static bool DungCauBangTron(Transform parent, float k)
    {
        if (!daTimCauBangTron)
        {
            daTimCauBangTron = true;
            mauCauBangTron = Resources.Load<GameObject>(ThuMucQuaCauBang + "CauBangTron");
        }
        if (mauCauBangTron == null) return false;
        if (mKhiLanhBoc == null)
        {
            var t = Resources.Load<Texture2D>(ThuMucQuaCauBang + "KhiLanhBoc");
            if (t == null) return false;
            mKhiLanhBoc = Mats.Additive("KhiLanhBoc", t, new Color(0.62f, 0.86f, 1f, 1f), 1.6f);
        }
        int soLop = 0;
        foreach (var mf in mauCauBangTron.GetComponentsInChildren<MeshFilter>())
        {
            bool laLoi = mf.name == "LoiTron";
            var go = laLoi
                ? ProcMesh.Part("LoiBang", parent, mf.sharedMesh, LoiCauBangMat, Vector3.zero, Quaternion.identity, Vector3.one * k, false)
                : ProcMesh.Part(mf.name, parent, mf.sharedMesh, mKhiLanhBoc, Vector3.zero,
                                mf.name == "KhiLanh1" ? Quaternion.Euler(23f, 11f, 63f) : Quaternion.identity, Vector3.one * k, false);
            var sp = go.AddComponent<Spin>();
            if (laLoi) { sp.axis = Vector3.forward; sp.degreesPerSecond = 90f; }
            else if (mf.name == "KhiLanh0") { sp.axis = new Vector3(0.3f, 1f, 0.2f).normalized; sp.degreesPerSecond = 210f; }
            else { sp.axis = new Vector3(-0.4f, -1f, 0.3f).normalized; sp.degreesPerSecond = 150f; }
            if (!laLoi)
            {
                var tr = go.AddComponent<ScrollUV>();
                tr.speed = mf.name == "KhiLanh0" ? new Vector2(0.35f, 0.18f) : new Vector2(-0.25f, -0.12f);
            }
            soLop++;
        }
        return soLop > 0;
    }

    static void DungLoiCoGai(Transform parent, float radius, float k)
    {
        // LOI PHA LE CO GAI tu Blender. Luoi dung o ban kinh loi 0,5 -> phong theo radius.
        var luoi = LuoiQuaCauBang;
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
    }

    /// <summary>Phan chung cua qua cau dang bay/roi: hao quang, luong khi lanh, vet bang, manh bang, den.</summary>
    static void BuildPhanBayQuaCauBang(Transform parent, float radius, bool banRoi, float heSoHat)
    {
        // 2) Hao quang lanh bam quanh qua cau
        var hq = NewPS("HaoQuang", parent, Vector3.zero, HaoQuangCauBangMat, ParticleSystemRenderMode.Billboard);
        var hm = hq.main;
        hm.startLifetime = 0.18f; hm.startSpeed = 0f;
        // Nho va mo (lan dau 4-5 lan ban kinh, do 1,25: ba qua + bloom nhoe thanh mot dom trang, mat ca hinh pha le)
        hm.startSize = new ParticleSystem.MinMaxCurve(radius * 2.2f, radius * 2.8f);
        hm.simulationSpace = ParticleSystemSimulationSpace.Local; hm.maxParticles = 8;
        var hem = hq.emission; hem.rateOverTime = 16f * heSoHat;
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
        var sem = suong.emission; sem.rateOverTime = 70f * heSoHat;
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
        var mem = manh.emission; mem.rateOverTime = 36f * heSoHat;
        var msh = manh.shape; msh.shapeType = ParticleSystemShapeType.Sphere; msh.radius = radius * 0.8f;
        var mcol = manh.colorOverLifetime; mcol.enabled = true;
        mcol.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 1f, 1f, 0.7f, 0f));
        var mrot = manh.rotationOverLifetime; mrot.enabled = true;
        mrot.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

        // 6) Anh sang lanh hat xuong mat dat - ban roi cua Mua bang KHONG co: 7-8 qua cung luc tren
        //    khong la 7-8 den diem, qua nang cho dien thoai; vung bao tuyet da co anh sang rieng
        if (banRoi) return;
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
    /// QUA CAU BANG ROI TU TROI (thay tang bang cua MUA BANG - nguoi dung 16/09/2026: "thay vi roi cac tang bang
    /// thi cho roi cac qua cau bang, co luong khong khi lanh phia sau giong y chang Qua cau bang").
    ///
    /// Roi THANG DUNG tu do cao <paramref name="height"/> (nguoi dung 16/09/2026). Ban dau toi cho roi cheo ngang man
    /// hinh de duoi hien ro hon - nguoi dung khong muon nghieng, bo.
    /// </summary>
    public static GameObject QuaCauBangRoi(Vector3 target, float height, float fallTime)
    {
        Vector3 tu = target + Vector3.up * height;

        var go = new GameObject("QuaCauBangRoi");
        go.transform.position = tu;
        // Huong bay thang xuong: truc +Z cua qua cau chi xuong dat (duoi gai huong len troi). Can vector 'len' khac
        // phuong voi huong nhin - LookRotation(xuong, len) la hai vector song song, ket qua khong xac dinh.
        go.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        BuildQuaCauBangVisual(go.transform, Random.Range(0.36f, 0.50f), true);

        var mover = go.AddComponent<FallingShard>();
        mover.target = target;
        mover.travelTime = fallTime;
        return go;
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
        IceImpact(pos, QuaCauBang.BanKinhHinhBang * radius / QuaCauBang.BanKinhNo);

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
