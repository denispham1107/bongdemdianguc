using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "QUA CAU BANG" (them 16/09/2026).
///
/// Nguoi dung: ba qua cau bang bay nhu Qua cau lua, nhung "khac han la tao ra mot luong
/// khong khi lanh phia sau va vet bang cua qua cau". Tai nguyen dung trong Blender qua
/// MCP, nam o Resources/KyNang/QuaCauBang:
///   - CauBangPhaLe.fbx + .png + .mat (28/09/2026, CongCu/Blender/cau_bang_pha_le.blend - nguoi dung: ban cu "chi la
///     hinh khoi tron dinh gai xung quanh, qua so sai", chon "khoi bang pha le" tong xanh lam): KHOI BANG DEO THO 63 mat cat
///     khong deu + 6 tinh the luc giac dau nhon moc lech (2 lon, 4 nho), anh chi tiet nuong (R vet nut to + mang nut manh,
///     G bot khi, B mang suong gia) doc bang shader Diablo25D/CauBangPhaLe (mat cat phang loe sang, nut chim sau theo goc
///     nhin, vien fresnel); LOI SANG la DOM SANG MEM ben trong (hat cong sang ve truoc lop bang - thu luoi loi 34 mat roi:
///     hien thanh mot khoi xanh canh sac dan vao giua, bo). Tinh the dang LANG TRU MAP NGAN (ban dai manh doc ra gai - dung
///     cai nguoi dung che). Quay lan tren mot truc nghieng;
///     (ban cu QuaCauBang.fbx - loi tron dinh gai + duoi gai - da xoa);
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
    static Material mSuongLanh, mManhBang, mVetBang, mHaoQuangCauBang, mKhoiBang, mLoiSangCauBang;

    /// <summary>Vat lieu khoi bang pha le (asset trong Resources - giu shader trong ban build). Null thi dung Mats.Ice.</summary>
    public static Material KhoiBangPhaLeMat
    {
        get
        {
            // Kiem bang null cua Unity chu khong co "da nap": vat lieu / luoi nap luc Play bi xoa khi thoat Play
            if (mKhoiBang == null) mKhoiBang = Resources.Load<Material>(ThuMucQuaCauBang + "CauBangPhaLe");
            return mKhoiBang;
        }
    }

    /// <summary>Dom sang mem trong long khoi bang: cong sang, ve TRUOC lop bang (hang doi 2999 &lt; 3001 cua shader khoi bang).</summary>
    static Material LoiSangCauBangMat
    {
        get
        {
            if (mLoiSangCauBang == null)
            {
                mLoiSangCauBang = Mats.Additive("LoiSangCauBang", TextureFactory.SoftDot(2.4f), new Color(0.62f, 0.86f, 1f, 1f), 1.25f);
                mLoiSangCauBang.renderQueue = 2999;
            }
            return mLoiSangCauBang;
        }
    }

    /// <summary>Luoi khoi bang pha le (Blender) - null neu khong nap duoc (luc do dung khoi cau tron).</summary>
    public static Mesh LuoiQuaCauBang
    {
        get
        {
            if (luoiQuaCauBang == null)
            {
                var go = Resources.Load<GameObject>(ThuMucQuaCauBang + "CauBangPhaLe");
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
                mHaoQuangCauBang = Mats.Additive("P_HaoQuangCauBang", TextureFactory.SoftDot(1.8f), new Color(0.40f, 0.70f, 1f, 0.45f), 0.6f);   // 28/09/2026 0,8 -> 0,6: khoi pha le khong bi nhoe thanh dom sang
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
        // 1) LOI PHA LE - chi ky nang Qua cau bang. Ban roi cua Mua bang KHONG co khoi cau (nguoi dung 17/09/2026: "bo khoi
        //    cau, cho cac vet sang bang rot tu tren troi xuong"): chi con vet sao bang + hao quang + khi lanh + manh bang.
        //    (Ban truoc cung ngay: cau tron boc khi lanh CauBangTron - da bo.)
        if (!banRoi)
            DungLoiCoGai(parent, radius, k);

        BuildPhanBayQuaCauBang(parent, radius, banRoi, heSoHat);
    }

    /// <summary>Truc quay lan cua khoi bang (khong gian rieng cua khoi) - nghieng de moi vong mot bo mat cat khac loe sang.</summary>
    public static readonly Vector3 TrucLanKhoiBang = new Vector3(0.35f, 0.55f, 1f).normalized;

    static void DungLoiCoGai(Transform parent, float radius, float k)
    {
        // KHOI BANG PHA LE tu Blender (ban kinh 0,5 -> phong theo radius). Ten "LoiBang" giu nguyen cho phep thu / cho khac tim.
        var luoi = LuoiQuaCauBang;
        var mat = KhoiBangPhaLeMat;
        GameObject loi;
        // Goc xoay ngau nhien: ba qua trong chum khong giong het nhau
        var goc = Random.rotation;
        if (luoi != null && mat != null)
        {
            loi = ProcMesh.Part("LoiBang", parent, luoi, mat, Vector3.zero, goc, Vector3.one * k, false);
            // Dom sang mem trong long: vai hat cong sang chong nhau o tam, nhap nhay nhe
            var ls = NewPS("LoiSang", parent, Vector3.zero, LoiSangCauBangMat, ParticleSystemRenderMode.Billboard);
            var lm = ls.main;
            lm.startLifetime = 0.30f; lm.startSpeed = 0f;
            lm.startSize = new ParticleSystem.MinMaxCurve(radius * 1.05f, radius * 1.35f);
            lm.simulationSpace = ParticleSystemSimulationSpace.Local; lm.maxParticles = 6;
            var lem = ls.emission; lem.rateOverTime = 14f;
            var lsh = ls.shape; lsh.enabled = false;
            var lcol = ls.colorOverLifetime; lcol.enabled = true;
            lcol.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 0.8f, 0.5f, 0f));
        }
        else
            loi = ProcMesh.Part("LoiBang", parent, ProcMesh.Sphere(radius, 12, 8, 1f, Color.white), Mats.Ice,
                                Vector3.zero, Quaternion.identity, Vector3.one, false);
        // Quay lan: mat cat thay nhau loe sang anh trang
        var xoay = loi.AddComponent<Spin>();
        xoay.axis = TrucLanKhoiBang;
        xoay.degreesPerSecond = 150f;
    }

    // ================================================================
    //  TANG BANG PHA LE TREN MAT DAT (28/09/2026)
    // ================================================================
    //
    // Nguoi dung (anh chup): tang bang moc tren dat cua Qua cau bang / Mua bang "qua choi va tho so sai, chua giong that" ->
    // dung lai bang Blender MCP theo dang KHOI BANG PHA LE tong xanh lam nhu qua cau (CongCu/Blender/tang_bang_pha_le.blend):
    // 4 cum TangBangPhaLe.fbx (CumBang0-3: 3-6 lang tru bang deo nhieu mat dinh gay vat, nghieng ra ngoai + tang thap o chan,
    // 158-244 tam giac, chan lun -0,2 m) + anh chi tiet (nut / bot / suong, suong DAY o chan) + TangBangPhaLe.mat (cung shader
    // Diablo25D/CauBangPhaLe). Prefab Vfx_NoBang giu nguyen cau truc (ten CumGai*, ExpandFade, quang chan HaoQuang*) - moi lan
    // sinh ra thi THAY luoi + vat lieu cua tung cum (chon ngau nhien, xoay ngau nhien quanh truc dung), nen TangBangNo cap 5,
    // phep thu, moc tu dat troi len van y nguyen. Ban cu: shader Ice phat sang + quang chan 1,15 -> "choi".

    static Mesh[] luoiTangBangPhaLe;
    static Material mTangBangPhaLe, mHaoQuangBangDiu;

    /// <summary>Bon luoi cum tang bang pha le (Blender) - rong neu khong nap duoc (luc do giu cum gai cu).</summary>
    public static Mesh[] LuoiTangBangPhaLe
    {
        get
        {
            if (luoiTangBangPhaLe == null || luoiTangBangPhaLe.Length == 0 || luoiTangBangPhaLe[0] == null)
            {
                var ds = new System.Collections.Generic.List<Mesh>();
                var go = Resources.Load<GameObject>(ThuMucQuaCauBang + "TangBangPhaLe");
                if (go != null)
                    foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
                        if (mf.sharedMesh != null) ds.Add(mf.sharedMesh);
                luoiTangBangPhaLe = ds.ToArray();
            }
            return luoiTangBangPhaLe;
        }
    }

    public static Material TangBangPhaLeMat
    {
        get
        {
            if (mTangBangPhaLe == null) mTangBangPhaLe = Resources.Load<Material>(ThuMucQuaCauBang + "TangBangPhaLe");
            return mTangBangPhaLe;
        }
    }

    /// <summary>Quang chan cum bang - diu hon ban cu (1,15 -> 0,5) de khoi bang khong choi.</summary>
    public const float DoSangQuangChanBang = 0.5f;

    static Material HaoQuangBangDiuMat
    {
        get
        {
            if (mHaoQuangBangDiu == null)
                mHaoQuangBangDiu = Mats.Additive("P_HaoQuangBangDiu", TextureFactory.GlowPool(), new Color(0.34f, 0.62f, 1f, 1f), DoSangQuangChanBang);
            return mHaoQuangBangDiu;
        }
    }

    // ================================================================
    //  MANH BANG VO PHA LE (28/09/2026)
    // ================================================================
    //
    // Nguoi dung (2 anh): manh nho no ra cua Mua bang / Qua cau bang "chi la hinh tam giac, so sai" -> manh bang pha le
    // trong suot tong xanh lam that. Nguon cu: Shards (hat KEO DAN anh canh dieu Tex_shard / T_MB_P_Shard), Manh3D (luoi
    // ManhVo + shader Ice phat sang), ManhBung / ManhBangRoi (hat phang anh ManhBang). Blender MCP
    // (CongCu/Blender/manh_bang_pha_le.blend) -> ManhBangPhaLe.fbx: 8 manh 3D (4 PHIEN vien lom chom day, mat vo vat lech;
    // 2 CUC; 2 KIM), 14-28 tam giac, canh dai nhat 1 m + anh chi tiet + ManhBangPhaLe.mat (shader Diablo25D/ManhBangPhaLe:
    // nhan mau hat nen colorOverLifetime van lam mo). Hat doi sang dang LUOI, goc 3D ngau nhien, lon nhao quanh ca 3 truc.

    static Mesh[] luoiManhBang;
    static Material mManhBangPhaLe;

    public static Mesh[] LuoiManhBangPhaLe
    {
        get
        {
            if (luoiManhBang == null || luoiManhBang.Length == 0 || luoiManhBang[0] == null)
            {
                var ds = new System.Collections.Generic.List<Mesh>();
                var go = Resources.Load<GameObject>(ThuMucQuaCauBang + "ManhBangPhaLe");
                if (go != null)
                    foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
                        if (mf.sharedMesh != null) ds.Add(mf.sharedMesh);
                luoiManhBang = ds.ToArray();
            }
            return luoiManhBang;
        }
    }

    public static Material ManhBangPhaLeMat
    {
        get
        {
            if (mManhBangPhaLe == null) mManhBangPhaLe = Resources.Load<Material>(ThuMucQuaCauBang + "ManhBangPhaLe");
            return mManhBangPhaLe;
        }
    }

    /// <summary>Toc lon nhao toi da cua manh (rad/giay, moi truc).</summary>
    public const float TocLonNhaoManhBang = 7f;

    /// <summary>
    /// Doi mot he hat manh bang sang MANH BANG PHA LE 3D. Goi ngay sau khi dung / sinh, truoc nhip mo phong dau.
    /// <paramref name="heSoCo"/> nhan co hat (hat keo dan cu to hon ve mat nhin so voi mot manh 3D day).
    /// </summary>
    public static void DoiThanhManhBangPhaLe(ParticleSystem ps, float heSoCo)
    {
        if (ps == null) return;
        var luoi = LuoiManhBangPhaLe;
        var mat = ManhBangPhaLeMat;
        if (luoi == null || luoi.Length == 0 || mat == null) return;
        var r = ps.GetComponent<ParticleSystemRenderer>();
        if (r == null) return;
        r.renderMode = ParticleSystemRenderMode.Mesh;
        r.SetMeshes(luoi);
        r.meshDistribution = ParticleSystemMeshDistribution.UniformRandom;
        r.sharedMaterial = mat;
        r.alignment = ParticleSystemRenderSpace.World;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;

        var m = ps.main;
        if (Mathf.Abs(heSoCo - 1f) > 0.001f)
        {
            var co = m.startSize;
            if (co.mode == ParticleSystemCurveMode.TwoConstants)
                m.startSize = new ParticleSystem.MinMaxCurve(co.constantMin * heSoCo, co.constantMax * heSoCo);
            else if (co.mode == ParticleSystemCurveMode.Constant)
                m.startSize = new ParticleSystem.MinMaxCurve(co.constant * heSoCo);
        }
        m.startRotation3D = true;
        m.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        // Ca ba truc CUNG kieu TwoConstants - lech kieu la Unity bo qua ca mo-dun (da vap voi hat Grit cua Gio loc)
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.separateAxes = true;
        rot.x = new ParticleSystem.MinMaxCurve(-TocLonNhaoManhBang, TocLonNhaoManhBang);
        rot.y = new ParticleSystem.MinMaxCurve(-TocLonNhaoManhBang, TocLonNhaoManhBang);
        rot.z = new ParticleSystem.MinMaxCurve(-TocLonNhaoManhBang, TocLonNhaoManhBang);
    }

    /// <summary>Doi moi he hat manh (Manh3D, Shards) duoi mot goc vu no sang manh bang pha le.</summary>
    public static void DoiManhBangTrong(GameObject goc)
    {
        if (goc == null) return;
        foreach (var ps in goc.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name == "Manh3D") DoiThanhManhBangPhaLe(ps, 1f);
            else if (ps.name == "Shards") DoiThanhManhBangPhaLe(ps, 0.8f);
        }
    }

    /// <summary>
    /// Thay moi cum gai (CumGai*) cua mot tang bang vua sinh bang cum bang pha le. Goi NGAY sau khi dung / sinh tu prefab,
    /// TRUOC Start cua ExpandFade (no lay ban sao vat lieu o Start).
    /// </summary>
    public static void NangCapTangBang(GameObject tang)
    {
        if (tang == null) return;
        DoiManhBangTrong(tang);      // manh vo tam giac -> manh bang pha le 3D
        var luoi = LuoiTangBangPhaLe;
        var mat = TangBangPhaLeMat;
        if (luoi == null || luoi.Length == 0 || mat == null) return;
        foreach (Transform con in tang.transform)
        {
            if (!con.name.StartsWith("CumGai")) continue;
            var mf = con.GetComponent<MeshFilter>();
            var mr = con.GetComponent<MeshRenderer>();
            if (mf == null || mr == null) continue;
            mf.sharedMesh = luoi[Random.Range(0, luoi.Length)];
            mr.sharedMaterial = mat;
            con.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            foreach (Transform q in con)
                if (q.name.StartsWith("HaoQuang"))
                {
                    var qr = q.GetComponent<MeshRenderer>();
                    if (qr != null) qr.sharedMaterial = HaoQuangBangDiuMat;
                }
        }
    }

    static Material mVetSaoBang, mDauSaoBang;

    /// <summary>Be rong vet sao bang CO GOC o dau / duoi (m). 17/09/2026 nguoi dung: to them 20% chi be ngang (0,55/0,30 -> 0,66/0,36).</summary>
    public const float RongDauVetSaoBang = 0.66f, RongDuoiVetSaoBang = 0.36f;

    /// <summary>Moi vet ngau nhien TiLeNhoNhat..1 lan co goc (nguoi dung: to nho khac nhau, khong be hon 70% co goc).</summary>
    public const float TiLeVetSaoBangNhoNhat = 0.7f;

    /// <summary>Be rong tam dau vet (giot sang mui nhon) = he so x be rong dau vet; cao gap doi.</summary>
    public const float HeSoDauSaoBang = 1.6f;

    /// <summary>Do dai (giay) vet sao bang cua Mua bang - qua roi 20 m trong 0,96 s, nhanh dan (cuoi ~42 m/s) nen vet dai 3-7 m.</summary>
    public const float GiayVetSaoBang = 0.17f;

    /// <summary>
    /// VET SAO BANG cua Mua bang (nguoi dung 17/09/2026: vet sang bang "dai, manh nhu sao bang"). Anh VetSaoBang.png ve bang
    /// Blender MCP (CongCu/Blender/vet_sao_bang.blend): u = 0 dau vet (nut sang trang), loi manh nho dan va nhat dan ve u = 1,
    /// hao quang xanh lanh, vai dom lap lanh. TrailRenderer keo gian anh doc theo vet (Stretch) - dau vet o u = 0 nhu widthCurve.
    /// Ten "VetBang" giu nguyen de ThaDuoiQuaCauBang tha no ra tan dan nhu truoc.
    /// </summary>
    static void DungVetSaoBang(Transform parent)
    {
        if (mVetSaoBang == null)
        {
            var t = Resources.Load<Texture2D>(ThuMucQuaCauBang + "VetSaoBang");
            mVetSaoBang = Mats.Additive("VetSaoBang", t != null ? t : TextureFactory.SoftDot(1.1f), new Color(0.80f, 0.93f, 1f, 1f), 2.2f);
        }
        if (mDauSaoBang == null)
        {
            var t = Resources.Load<Texture2D>(ThuMucQuaCauBang + "DauSaoBang");
            mDauSaoBang = Mats.Additive("DauSaoBang", t != null ? t : TextureFactory.SoftDot(1.4f), new Color(0.85f, 0.95f, 1f, 1f), 2.0f);
        }
        float tiLe = Random.Range(TiLeVetSaoBangNhoNhat, 1f);

        // DAU VET (nguoi dung 17/09/2026): lan 1 "giong nhu bi cat mat ngang" -> anh vet mo vao tu u = 0, loi thuon ve dau, them dom
        // sang; lan 2 "hoi nhon nhon va co rang cua mot chut" -> anh DauSaoBang doi thanh giot sang mui nhon chi huong roi, rang cua
        // hai suon, tren tam tu giac xoay theo truc roi (DauSaoBangHuong) thay hat billboard tron.
        var dau = new GameObject("DauSaoBang");
        dau.transform.SetParent(parent, false);
        dau.AddComponent<MeshFilter>().sharedMesh = DauSaoBangHuong.TuGiac;
        var dmr = dau.AddComponent<MeshRenderer>();
        dmr.sharedMaterial = mDauSaoBang;
        dmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dmr.receiveShadows = false;
        var huong = dau.AddComponent<DauSaoBangHuong>();
        huong.huongRoi = parent.forward;
        huong.rong = RongDauVetSaoBang * HeSoDauSaoBang * tiLe;

        var vet = new GameObject("VetBang");
        vet.transform.SetParent(parent, false);
        var tr = vet.AddComponent<TrailRenderer>();
        tr.time = GiayVetSaoBang;
        tr.minVertexDistance = 0.15f;
        tr.textureMode = LineTextureMode.Stretch;
        tr.alignment = LineAlignment.View;
        tr.widthCurve = new AnimationCurve(new Keyframe(0f, RongDauVetSaoBang), new Keyframe(1f, RongDuoiVetSaoBang));
        tr.widthMultiplier = tiLe;
        tr.material = mVetSaoBang;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        tr.colorGradient = g;
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tr.receiveShadows = false;
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

        // 4) VET BANG: dai sang lanh keo dai theo duong bay. Ban roi: VET SAO BANG dai manh (anh Blender)
        if (banRoi) DungVetSaoBang(parent);
        else
        {
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
        }

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
        DoiThanhManhBangPhaLe(manh, 1f);

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
            if (ps != null && (c.name == "HaoQuang" || c.name == "DauSaoBang" || c.name == "LoiSang")) continue;
            c.SetParent(null, true);
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (tr != null) tr.emitting = false;
            AutoDestroy.Add(c.gameObject, 1.3f);
        }
    }

    /// <summary>Vu no bang cua qua cau: gai bang tu dat + vong lanh (IceImpact) + bung suong va manh bang.</summary>
    /// <returns>TANG BANG vua moc - de noi goi gan TangBangNo khi ky nang dat cap 5.</returns>
    public static GameObject NoQuaCauBang(Vector3 pos, float radius)
    {
        var tangBang = IceImpact(pos, QuaCauBang.BanKinhHinhBang * radius / QuaCauBang.BanKinhNo);

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
        DoiThanhManhBangPhaLe(manh, 1f);

        // Vet suong gia luon de lai tren dat (IceImpact chi de 35%)
        GroundDecal.Spawn(new Vector3(pos.x, GroundY(pos), pos.z), radius * 0.9f, new Material(FrostMat), 5f, 2.5f);

        return tangBang;
    }
}
