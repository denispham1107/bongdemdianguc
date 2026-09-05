using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// XUONG HIEU UNG (lua, bang, khoi, tia lua, anh sang).
///
/// Moi he hat (ParticleSystem) o day deu duoc dung bang code: hinh phun,
/// mau theo thoi gian, do lon theo thoi gian, nhieu loan gio... nen hieu ung
/// lua chay va bang no trong "song" chu khong phai anh dan cung.
/// </summary>
public static partial class VfxFactory
{
    // ---- Vat lieu hat dung chung (tao mot lan) ----
    static Material mFlame, mSpark, mSmoke, mEmber, mFrostMist, mSnow, mShard,
                    mRingFire, mRingIce, mScorch, mFrostPatch, mGlowDot,
                    mShardKhieng, mRingKhieng;
    static Material mCumBang, mNuoc, mGonSong, mHaoQuangBang, mKhoiSet;
    static Material mBoltCore, mBoltGlow, mSparkBlue, mPool, mRingBolt, mStormCloud;

    public static Material FlameMat { get { if (mFlame == null) mFlame = Mats.Additive("P_Flame", TextureFactory.FlameLick(), new Color(1f, 0.65f, 0.28f, 1f), 1.7f); return mFlame; } }
    public static Material SparkMat { get { if (mSpark == null) mSpark = Mats.Additive("P_Spark", TextureFactory.SoftDot(2.2f), new Color(1f, 0.78f, 0.35f, 1f), 2.4f); return mSpark; } }
    public static Material EmberMat { get { if (mEmber == null) mEmber = Mats.Additive("P_Ember", TextureFactory.SoftDot(1.6f), new Color(1f, 0.45f, 0.12f, 1f), 2.0f); return mEmber; } }
    public static Material SmokeMat { get { if (mSmoke == null) mSmoke = Mats.Alpha("P_Smoke", TextureFactory.Smoke(), new Color(0.12f, 0.10f, 0.09f, 0.75f)); return mSmoke; } }
    public static Material MistMat { get { if (mFrostMist == null) mFrostMist = Mats.Alpha("P_Mist", TextureFactory.Smoke(), new Color(0.65f, 0.85f, 1f, 0.45f)); return mFrostMist; } }
    public static Material SnowMat { get { if (mSnow == null) mSnow = Mats.Additive("P_Snow", TextureFactory.Snowflake(), new Color(0.75f, 0.92f, 1f, 1f), 1.4f); return mSnow; } }
    public static Material ShardMat { get { if (mShard == null) mShard = Mats.Additive("P_Shard", TextureFactory.IceShard(), new Color(0.70f, 0.90f, 1f, 1f), 1.8f); return mShard; } }
    public static Material GlowMat { get { if (mGlowDot == null) mGlowDot = Mats.Additive("P_Glow", TextureFactory.SoftDot(1.3f), Color.white, 1.6f); return mGlowDot; } }
    public static Material RingFireMat { get { if (mRingFire == null) mRingFire = Mats.Additive("P_RingFire", TextureFactory.Ring(0.22f), new Color(1f, 0.5f, 0.16f, 0.9f), 1.3f); return mRingFire; } }
    public static Material RingIceMat { get { if (mRingIce == null) mRingIce = Mats.Additive("P_RingIce", TextureFactory.Ring(0.14f), new Color(0.42f, 0.72f, 1f, 0.85f), 0.85f); return mRingIce; } }

    // ---- Vat lieu RIENG cho khieng vo ----
    //
    // Khong dung lai ShardMat / RingIceMat cua MUA BANG duoc: hai vat lieu ay
    // da to san mau xanh bang, ma vat lieu hat NHAN voi mau hat - dat mau tim
    // cho hat xong van ra xanh nhat. Da vap dung loi do: doi startColor sang
    // tim ma manh vo van xanh nhu cu.
    /// <summary>
    /// Manh khieng vo. Dung LUOI 16 HINH MANH khac nhau chu khong dung anh
    /// manh bang: anh manh bang la mot hinh tam giac duy nhat, ca tram hat deu
    /// giong het nhau nhu cat bang khuon.
    /// </summary>
    public static Material ShardKhiengMat { get { if (mShardKhieng == null) mShardKhieng = Mats.Additive("P_ShardKhieng", TextureFactory.ManhVoSheet(), new Color(1f, 0.80f, 0.32f, 1f), 2.1f); return mShardKhieng; } }
    public static Material RingKhiengMat { get { if (mRingKhieng == null) mRingKhieng = Mats.Additive("P_RingKhieng", TextureFactory.Ring(0.14f), new Color(1f, 0.76f, 0.26f, 0.85f), 1.05f); return mRingKhieng; } }
    // ---- CUM BANG MOC LEN TU DAT ----
    //
    // Phai la vat lieu RIENG, khong dung chung Mats.Ice voi chop sang va manh
    // vun. Ban truoc dung chung nen cum bang ra TRANG CHAY: do sang do tren anh
    // chup that duoc R=0,879 G=0,890 B=0,890 - hieu B-R chi 0,011, tuc gan nhu
    // khong con mau xanh nao.
    //
    // Ba thu trong S_Ice cong THANG mau trang vao, khong qua mau nao ca:
    //   col += _RimColor * rim * _Glow;   <- _RimColor cu la (0,85 0,97 1,0)
    //   col += sparkle;                   <- so vo huong, cong deu ca ba kenh
    //   col += spec * 0.9;
    // Luoi cum gai co rat nhieu mat nghieng nen "rim" lon o gan nhu moi diem -
    // cong dong ba thu lai thi ca cum trang xoa.
    //
    // Chua bang cach: vien doi sang XANH, _RimPower cao hon cho vien HEP lai,
    // ha _Glow va _Sparkle. Van phat sang, nhung phat sang mau xanh.
    public static Material CumBangMat
    {
        get
        {
            if (mCumBang == null)
            {
                mCumBang = new Material(Mats.IceShader);
                mCumBang.name = "CumBang";
                mCumBang.SetColor("_Color", new Color(0.42f, 0.66f, 0.94f, 0.90f));
                mCumBang.SetColor("_DeepColor", new Color(0.09f, 0.28f, 0.66f, 1f));
                mCumBang.SetColor("_RimColor", new Color(0.52f, 0.76f, 1f, 1f));
                mCumBang.SetFloat("_RimPower", 3.6f);
                mCumBang.SetFloat("_Glow", 0.75f);
                mCumBang.SetFloat("_Sparkle", 0.35f);
                mCumBang.SetFloat("_CrackAmount", 0.30f);
            }
            return mCumBang;
        }
    }

    /// <summary>Hao quang xanh o chan cum bang - phan "phat sang" nhin thay
    /// duoc tu xa, khong phu thuoc goc nhin nhu vien fresnel.</summary>
    public static Material HaoQuangBangMat { get { if (mHaoQuangBang == null) mHaoQuangBang = Mats.Additive("P_HaoQuangBang", TextureFactory.GlowPool(), new Color(0.34f, 0.62f, 1f, 1f), 1.15f); return mHaoQuangBang; } }

    // ---- NUOC ----
    //
    // Giot nuoc dung ALPHA chu khong ADDITIVE. Additive thi giot nuoc sang len
    // nhu tan lua - nuoc that lam toi nen di, khong lam sang.
    // SoftDot(1.2) chu khong 1.9: luy thua cang cao thi cham cang mo ra ria,
    // gan nhu chi con mot vet suong. Giot nuoc can co LOI DAC.
    public static Material NuocMat { get { if (mNuoc == null) mNuoc = Mats.Alpha("P_Nuoc", TextureFactory.SoftDot(1.2f), new Color(0.74f, 0.90f, 1f, 0.95f)); return mNuoc; } }

    /// <summary>Gon song lan tren mat dat. Vanh MANH (0,06) chu khong day nhu
    /// vong bang - gon song la mot duong vien, khong phai mot dia.</summary>
    public static Material GonSongMat { get { if (mGonSong == null) mGonSong = Mats.Additive("P_GonSong", TextureFactory.Ring(0.10f), new Color(0.62f, 0.85f, 1f, 0.85f), 1.25f); return mGonSong; } }

    /// <summary>
    /// Khoi cho cho set vua danh chay. RIENG, khong dung chung SmokeMat.
    ///
    /// SmokeMat to (0,12 0,10 0,09) voi do duc 0,75 - dung mau khoi den that,
    /// va tren canh dem thi no gan nhu VO HINH. Do bang cach chup hai anh co
    /// khoi va khong khoi roi tru nhau: chi 8% diem anh doi khac, do lech trung
    /// binh 0,0128 - tuc mat thuong khong nhin ra.
    ///
    /// Xam dam voi do duc 1,0 thi van doc ra la khoi chay, ma noi len duoc tren
    /// ca mat dat lan bau troi dem.
    /// </summary>
    public static Material KhoiSetMat { get { if (mKhoiSet == null) mKhoiSet = Mats.Alpha("P_KhoiSet", TextureFactory.Smoke(), new Color(0.42f, 0.39f, 0.36f, 1f)); return mKhoiSet; } }

    public static Material ScorchMat { get { if (mScorch == null) mScorch = Mats.Alpha("P_Scorch", TextureFactory.ScorchMark(), new Color(1f, 1f, 1f, 0.85f)); return mScorch; } }
    public static Material FrostMat { get { if (mFrostPatch == null) mFrostPatch = Mats.Alpha("P_Frost", TextureFactory.FrostPatch(), new Color(0.62f, 0.80f, 1f, 0.5f)); return mFrostPatch; } }

    // ---- Vat lieu rieng cho SAM SET ----
    public static Material BoltCoreMat { get { if (mBoltCore == null) mBoltCore = Mats.Additive("P_BoltCore", TextureFactory.BoltStrip(), Color.white, 3.2f); return mBoltCore; } }
    public static Material BoltGlowMat { get { if (mBoltGlow == null) mBoltGlow = Mats.Additive("P_BoltGlow", TextureFactory.BoltStrip(), new Color(0.45f, 0.70f, 1f, 1f), 1.5f); return mBoltGlow; } }
    public static Material SparkBlueMat { get { if (mSparkBlue == null) mSparkBlue = Mats.Additive("P_SparkBlue", TextureFactory.SoftDot(2.2f), new Color(0.72f, 0.88f, 1f, 1f), 2.6f); return mSparkBlue; } }
    public static Material PoolMat { get { if (mPool == null) mPool = Mats.Additive("P_Pool", TextureFactory.GlowPool(), new Color(0.88f, 0.94f, 1f, 1f), 2.3f); return mPool; } }
    public static Material RingBoltMat { get { if (mRingBolt == null) mRingBolt = Mats.Additive("P_RingBolt", TextureFactory.Ring(0.16f), new Color(0.55f, 0.75f, 1f, 0.5f), 1.05f); return mRingBolt; } }
    public static Material StormCloudMat { get { if (mStormCloud == null) mStormCloud = Mats.Alpha("P_StormCloud", TextureFactory.Smoke(), new Color(0.16f, 0.18f, 0.30f, 0.8f)); return mStormCloud; } }

    // ================================================================
    //  HAM DUNG HE HAT
    // ================================================================

    static ParticleSystem NewPS(string name, Transform parent, Vector3 localPos, Material mat,
                                ParticleSystemRenderMode mode)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        var ps = go.AddComponent<ParticleSystem>();
        // Dung ngay lai de con chinh thong so; PlayOnStart se bat chay o khung hinh dau
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        go.AddComponent<PlayOnStart>();

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = mode;
        r.material = mat;
        r.sortingFudge = -5f;
        r.alignment = ParticleSystemRenderSpace.View;
        if (mode == ParticleSystemRenderMode.Stretch)
        {
            r.velocityScale = 0.12f;
            r.lengthScale = 2.2f;
        }
        return ps;
    }

    // ================================================================
    //  FLIPBOOK - anh dong cho tung hat
    // ================================================================

    /// <summary>
    /// Nap mot LUOI KHUNG HINH tu Assets/Resources/Flipbooks.
    ///
    /// Tra ve null neu chua co file - de ben goi con quay ve dung texture sinh
    /// bang code. Nho vay them flipbook la viec CONG THEM, khong lam hong thu
    /// dang chay.
    /// </summary>
    static Material mKhoiCuon;

    /// <summary>
    /// Vat lieu khoi FLIPBOOK - dam khoi cuon xoay ben trong tung hat.
    ///
    /// Tra ve null neu chua co file flipbook, de ben goi quay ve dung SmokeMat
    /// sinh bang code. Nho vay du an van chay duoc khi thieu file anh.
    /// </summary>
    public static Material KhoiCuonMat
    {
        get
        {
            if (mKhoiCuon == null)
            {
                var tex = NapFlipbook("KhoiCuon");
                if (tex == null) return null;
                mKhoiCuon = Mats.FlipbookAlpha("P_KhoiCuon", tex, Color.white);
            }
            return mKhoiCuon;
        }
    }

    static Material mLuaCuon;

    /// <summary>
    /// Vat lieu LUA flipbook - ngon lua liem va doi hinh ngay trong tung hat.
    ///
    /// Dung FlipbookAdd (cong sang) chu khong phai alpha: lua phat sang, no lam
    /// canh vat chung quanh sang len chu khong che mat.
    ///
    /// Anh nay GIU NGUYEN MAU (vang - cam - do theo nhiet do), khac voi flipbook
    /// khoi von render trang de Unity to mau sau. Mau lua doi theo TUNG DIEM
    /// trong ngon lua, to mot mau duy nhat len anh trang la mat het chuyen mau.
    /// Nen tint de TRANG.
    /// </summary>
    public static Material LuaCuonMat
    {
        get
        {
            if (mLuaCuon == null)
            {
                var tex = NapFlipbook("LuaCuon");
                if (tex == null) return null;
                mLuaCuon = Mats.FlipbookAdd("P_LuaCuon", tex, Color.white, 1.35f);
            }
            return mLuaCuon;
        }
    }

    /// <summary>
    /// He so PHONG TO / THU NHO hat khi doi tu texture sinh bang code sang
    /// flipbook, de kich thuoc NHIN THAY giu nguyen.
    ///
    /// Do bang "ban kinh hieu dung": ban kinh cua hinh tron co cung dien tich
    /// voi phan nhin thay duoc cua anh. Ket qua bat ngo - toi da doan nham mot
    /// lan: nhin luoi anh thay qua cau lua nam gon giua o nen tuong phai phong
    /// to 1,7 lan, nhung do ra chi 1,18. Con anh lua va khoi thi flipbook con
    /// TO HON texture cu, phai thu nho lai mot chut.
    /// </summary>
    public const float BuLua = 0.90f;
    public const float BuKhoi = 0.75f;
    public const float BuVuNo = 1.20f;

    static Material mKhoiKhieng;

    /// <summary>
    /// Khoi TIM cho luc khieng vo - dung lai DUNG luoi anh KhoiCuon.
    ///
    /// Duoc vi luoi ay render TRANG: to mau nao len cung ra mau ay. Neu hoi ay
    /// render khoi mau den thi bay gio da phai mo phong lai ca mot loat 36 khung
    /// chi de doi mau.
    /// </summary>
    public static Material KhoiKhiengMat
    {
        get
        {
            if (mKhoiKhieng == null)
            {
                var tex = NapFlipbook("KhoiCuon");
                if (tex == null) return null;
                mKhoiKhieng = Mats.FlipbookAlpha("P_KhoiKhieng", tex, new Color(1f, 0.72f, 0.24f, 1f));
            }
            return mKhoiKhieng;
        }
    }

    static Material mVuNo;

    /// <summary>
    /// Vat lieu VU NO flipbook - qua cau lua bung ra roi tan, ngay ben trong
    /// tung hat. Giu nguyen mau nhu flipbook lua.
    /// </summary>
    public static Material VuNoMat
    {
        get
        {
            if (mVuNo == null)
            {
                var tex = NapFlipbook("VuNo");
                if (tex == null) return null;
                mVuNo = Mats.FlipbookAdd("P_VuNo", tex, Color.white, 1.5f);
            }
            return mVuNo;
        }
    }

    public static Texture2D NapFlipbook(string ten)
    {
        return Resources.Load<Texture2D>("Flipbooks/" + ten);
    }

    /// <summary>
    /// Bat FLIPBOOK cho mot he hat: tung hat se chay lan luot qua cac o cua
    /// luoi khung hinh trong suot doi song cua no.
    ///
    /// PHAN DE SAI NHAT la Custom Vertex Streams. Shader flipbook can BA thu:
    /// uv cua khung hien tai, uv cua khung ke tiep, va he so tron giua hai
    /// khung. Unity chi gui du ba thu do khi streams duoc khai bao dung THU TU
    ///     Position, Color, UV, UV2, AnimBlend
    /// Sai thu tu thi khong bao loi gi ca - hat chi ra sai mau hoac dung im o
    /// mot khung, rat kho lan ra nguyen nhan.
    ///
    /// <paramref name="soVong"/> = so lan chay het luoi trong mot doi hat. De 1
    /// cho khoi/lua (mot chu ky chay chậm, tu nhien); de cao hon cho tia lua
    /// nho nhay lien tuc.
    /// </summary>
    public static void BatFlipbook(ParticleSystem ps, int cot, int hang, int soVong = 1)
    {
        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.mode = ParticleSystemAnimationMode.Grid;
        tsa.numTilesX = cot;
        tsa.numTilesY = hang;
        tsa.animation = ParticleSystemAnimationType.WholeSheet;
        tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
        tsa.cycleCount = Mathf.Max(1, soVong);

        // Moi hat bat dau o mot khung KHAC NHAU. Khong lam vay thi ca dam khoi
        // cuon xoay dong loat nhu mot, nhin ra ngay la hieu ung may moc.
        tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, cot * hang);

        var r = ps.GetComponent<ParticleSystemRenderer>();
        var luong = new List<ParticleSystemVertexStream>
        {
            ParticleSystemVertexStream.Position,
            ParticleSystemVertexStream.Color,
            ParticleSystemVertexStream.UV,
            ParticleSystemVertexStream.UV2,
            ParticleSystemVertexStream.AnimBlend,
        };
        r.SetActiveVertexStreams(luong);
    }

    static Gradient Grad(Color a, float ta, Color b, float tb, Color c, float tc,
                         float a0, float a1, float a2, float a3)
    {
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(a, ta), new GradientColorKey(b, tb), new GradientColorKey(c, tc) },
            new GradientAlphaKey[] { new GradientAlphaKey(a0, 0f), new GradientAlphaKey(a1, 0.15f),
                                     new GradientAlphaKey(a2, 0.6f), new GradientAlphaKey(a3, 1f) });
        return g;
    }

    /// <summary>
    /// Do duc GIU NGUYEN den moc <paramref name="giu"/> roi tat han ve 0.
    ///
    /// Mau de TRANG. colorOverLifetime NHAN vao startColor, to mau o day nua la
    /// mau bi nhan hai lan - giot nuoc xanh se ra xanh tham.
    /// </summary>
    static Gradient TatDanCuoi(float giu)
    {
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f),
                                     new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f),
                                     new GradientAlphaKey(1f, Mathf.Clamp01(giu)),
                                     new GradientAlphaKey(0f, 1f) });
        return g;
    }

    static AnimationCurve Curve(float v0, float v1, float v2)
    {
        return new AnimationCurve(new Keyframe(0f, v0), new Keyframe(0.35f, v1), new Keyframe(1f, v2));
    }

    // ================================================================
    //  LUA - BAM VAO QUA CAU LUA DANG BAY
    // ================================================================

    /// <summary>Toan bo phan "nhin thay duoc" cua qua cau lua.</summary>
    public static void BuildFireballVisual(Transform parent, float radius)
    {
        // 1) Loi lua dac - dung shader lua co noise cuon
        var core = ProcMesh.Part("Core", parent, ProcMesh.Sphere(radius, 18, 12, 1f, Color.white),
                                 Mats.Fire, Vector3.zero, Quaternion.identity, Vector3.one, false);
        var spin = core.AddComponent<Spin>();
        spin.axis = new Vector3(0.3f, 1f, 0.2f).normalized;
        spin.degreesPerSecond = 220f;

        // 2) Vo lua ben ngoai mo hon, quay nguoc chieu -> lua cuon xoay
        var shell = ProcMesh.Part("Shell", parent, ProcMesh.Sphere(radius * 1.55f, 16, 10, 1f, Color.white),
                                  Mats.FireSoft, Vector3.zero, Quaternion.identity, Vector3.one, false);
        var spin2 = shell.AddComponent<Spin>();
        spin2.axis = new Vector3(-0.5f, 1f, 0.4f).normalized;
        spin2.degreesPerSecond = -140f;

        // 3) Luoi lua liem ra phia sau
        var flames = NewPS("Flames", parent, Vector3.zero, FlameMat, ParticleSystemRenderMode.Billboard);
        var m = flames.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        m.startSize = new ParticleSystem.MinMaxCurve(radius * 1.6f, radius * 3.0f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 260;
        m.gravityModifier = -0.12f;
        var em = flames.emission; em.rateOverTime = 90f;
        var sh = flames.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = radius * 0.7f;
        var col = flames.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.95f, 0.70f), 0f,
            new Color(1f, 0.45f, 0.10f), 0.45f,
            new Color(0.35f, 0.04f, 0.01f), 1f,
            0f, 1f, 0.7f, 0f));
        var sol = flames.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.35f, 1f, 0.15f));
        var nz = flames.noise; nz.enabled = true; nz.strength = 0.55f; nz.frequency = 1.4f;

        // 4) Tan lua bay ra sau nhu duoi sao choi
        var sparks = NewPS("Sparks", parent, Vector3.zero, SparkMat, ParticleSystemRenderMode.Stretch);
        var sm = sparks.main;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 2.2f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.09f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.gravityModifier = 0.35f;
        sm.maxParticles = 300;
        var sem = sparks.emission; sem.rateOverTime = 70f;
        var ssh = sparks.shape; ssh.shapeType = ParticleSystemShapeType.Sphere; ssh.radius = radius * 0.9f;
        var scol = sparks.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.95f, 0.75f), 0f,
            new Color(1f, 0.55f, 0.15f), 0.5f,
            new Color(0.6f, 0.10f, 0.02f), 1f,
            1f, 1f, 0.8f, 0f));

        // 5) Khoi den vuong lai phia sau
        var smoke = NewPS("Smoke", parent, Vector3.zero, SmokeMat, ParticleSystemRenderMode.Billboard);
        var km = smoke.main;
        km.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        km.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        km.startSize = new ParticleSystem.MinMaxCurve(radius * 1.8f, radius * 3.4f);
        km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        km.simulationSpace = ParticleSystemSimulationSpace.World;
        km.gravityModifier = -0.05f;
        km.maxParticles = 120;
        var kem = smoke.emission; kem.rateOverTime = 22f;
        var ksh = smoke.shape; ksh.shapeType = ParticleSystemShapeType.Sphere; ksh.radius = radius * 0.5f;
        var kcol = smoke.colorOverLifetime; kcol.enabled = true;
        kcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.35f, 0.25f, 0.20f), 0f,
            new Color(0.14f, 0.12f, 0.11f), 0.4f,
            new Color(0.06f, 0.06f, 0.06f), 1f,
            0f, 0.45f, 0.3f, 0f));
        var ksol = smoke.sizeOverLifetime; ksol.enabled = true;
        ksol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.5f, 1.1f, 1.8f));

        // 6) Anh sang lua hat ra canh vat xung quanh
        var lightGo = new GameObject("FireLight");
        lightGo.transform.SetParent(parent, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.55f, 0.20f);
        lt.intensity = 6f;
        lt.range = 12f;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 6f; fl.amount = 0.28f; fl.speed = 12f; fl.rangeWobble = 0.12f;
    }

    /// <summary>Dung hinh vu no lua bang code (dung khi chua co prefab).</summary>
    public static GameObject BuildFireExplosion(Vector3 pos, float radius)
    {
        var root = new GameObject("FireExplosion");
        root.transform.position = pos;
        AutoDestroy.Add(root, 4.5f);

        // 1) Qua cau lua bung ra roi tan
        var ball = ProcMesh.Part("Blast", root.transform, ProcMesh.Sphere(1f, 18, 12, 1f, Color.white),
                                 new Material(Mats.Fire), Vector3.zero, Quaternion.identity, Vector3.one, false);
        var ef = ball.AddComponent<ExpandFade>();
        ef.duration = 0.55f;
        ef.startScale = Vector3.one * radius * 0.35f;
        ef.endScale = Vector3.one * radius * 1.35f;
        ef.alphaProperty = "_Alpha";
        var bspin = ball.AddComponent<Spin>();
        bspin.degreesPerSecond = 90f;

        // 2) Vong lua lan tren mat dat
        var ring = ProcMesh.Part("Ring", root.transform, GroundDecal.QuadMesh(), new Material(RingFireMat),
                                 new Vector3(0f, 0.06f, 0f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, false);
        var ringFade = ring.AddComponent<ExpandFade>();
        ringFade.duration = 0.65f;
        ringFade.startScale = Vector3.one * radius * 0.4f;
        ringFade.endScale = Vector3.one * radius * 3.4f;

        // 3) Luoi lua bung toa moi huong
        // Uu tien FLIPBOOK vu no: moi manh lua bung toa la mot qua cau lua
        // dang no thu nho, khong phai mot tam anh lua dung im.
        var matNo = VuNoMat;
        var burst = NewPS("Burst", root.transform, Vector3.zero,
                          matNo != null ? matNo : FlameMat,
                          ParticleSystemRenderMode.Billboard);
        if (matNo != null) BatFlipbook(burst, 6, 6, 1);

        // Anh flipbook co qua cau lua nam GON GIUA o, chi chiem chung 40% be
        // rong - quanh no la vien trong. Nen mot hat co "1 met" thi ngon lua
        // nhin thay chi khoang 0,4 met. Phai phong to hat len bu lai, khong thi
        // doi sang flipbook la ca vu no teo di.
        float buFlipbook = matNo != null ? BuVuNo : 1f;
        var m = burst.main;
        m.duration = 0.4f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 7.5f);
        m.startSize = new ParticleSystem.MinMaxCurve(radius * 0.8f * buFlipbook,
                                                     radius * 1.9f * buFlipbook);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = -0.25f;
        m.maxParticles = 400;
        m.stopAction = ParticleSystemStopAction.None;
        var em = burst.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 70) });
        var sh = burst.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = radius * 0.3f;
        var col = burst.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.98f, 0.85f), 0f,
            new Color(1f, 0.48f, 0.10f), 0.4f,
            new Color(0.30f, 0.03f, 0.01f), 1f,
            0.9f, 1f, 0.75f, 0f));
        var sol = burst.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.5f, 1.2f, 0.4f));
        var nz = burst.noise; nz.enabled = true; nz.strength = 1.1f; nz.frequency = 1.1f;

        // 4) Tan lua ban vang ra xa, roi xuong dat
        var sparks = NewPS("Sparks", root.transform, Vector3.zero, SparkMat, ParticleSystemRenderMode.Stretch);
        var sm = sparks.main;
        sm.duration = 0.3f; sm.loop = false;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.5f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(4f, 13f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.gravityModifier = 1.1f;
        sm.maxParticles = 250;
        var sem = sparks.emission;
        sem.rateOverTime = 0f;
        sem.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 90) });
        var ssh = sparks.shape; ssh.shapeType = ParticleSystemShapeType.Sphere; ssh.radius = radius * 0.2f;
        var scol = sparks.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.95f, 0.75f), 0f,
            new Color(1f, 0.50f, 0.12f), 0.55f,
            new Color(0.5f, 0.08f, 0.01f), 1f,
            1f, 1f, 0.7f, 0f));

        // 5) Cot khoi den boc len
        var smoke = NewPS("Smoke", root.transform, Vector3.zero, SmokeMat, ParticleSystemRenderMode.Billboard);
        var km = smoke.main;
        km.duration = 0.6f; km.loop = false;
        km.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
        km.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.6f);
        km.startSize = new ParticleSystem.MinMaxCurve(radius * 1.2f, radius * 2.6f);
        km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        km.simulationSpace = ParticleSystemSimulationSpace.World;
        km.gravityModifier = -0.12f;
        km.maxParticles = 120;
        var kem = smoke.emission;
        kem.rateOverTime = 0f;
        kem.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.05f, 26) });
        var ksh = smoke.shape; ksh.shapeType = ParticleSystemShapeType.Hemisphere; ksh.radius = radius * 0.45f;
        var kcol = smoke.colorOverLifetime; kcol.enabled = true;
        kcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.40f, 0.26f, 0.18f), 0f,
            new Color(0.16f, 0.14f, 0.13f), 0.35f,
            new Color(0.05f, 0.05f, 0.05f), 1f,
            0f, 0.6f, 0.35f, 0f));
        var ksol = smoke.sizeOverLifetime; ksol.enabled = true;
        ksol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.6f, 1.3f, 2.2f));

        // 6) Anh sang loe manh
        var lightGo = new GameObject("BlastLight");
        lightGo.transform.SetParent(root.transform, false);
        var lb = lightGo.AddComponent<LightBurst>();
        lb.peak = 22f; lb.duration = 0.55f; lb.range = radius * 8f;
        var lt = lightGo.AddComponent<Light>();
        lt.color = new Color(1f, 0.55f, 0.20f);

        return root;
    }

    /// <summary>VU NO LUA khi qua cau cham muc tieu (uu tien dung prefab).</summary>
    public static void FireExplosion(Vector3 pos, float radius)
    {
        var pf = GameAssets.I != null ? GameAssets.I.fireExplosionPrefab : null;
        if (pf != null)
        {
            var go = GameAssets.Make(pf, pos);
            float k = radius / 3.4f;                 // prefab duoc nuong o ban kinh 3.4
            if (Mathf.Abs(k - 1f) > 0.05f) go.transform.localScale = Vector3.one * k;
        }
        else
        {
            BuildFireExplosion(pos, radius);
        }

        // Vet chay den in xuong dat
        GroundDecal.Spawn(new Vector3(pos.x, GroundY(pos), pos.z), radius * 1.1f,
                          new Material(ScorchMat), 14f, 8f);
    }

    /// <summary>Ngon lua bam tren nguoi quai dang chay (uu tien dung prefab).</summary>
    public static GameObject AttachBurning(Transform target, float height, float radius)
    {
        var pf = GameAssets.I != null ? GameAssets.I.burningPrefab : null;
        if (pf != null)
        {
            var inst = Object.Instantiate(pf, target);
            inst.name = "Burning";
            inst.transform.localPosition = new Vector3(0f, height * 0.45f, 0f);
            return inst;
        }
        return BuildBurning(target, height, radius);
    }

    /// <summary>Dung hinh lua bam nguoi bang code (dung khi chua co prefab).</summary>
    public static GameObject BuildBurning(Transform target, float height, float radius)
    {
        var go = new GameObject("Burning");
        go.transform.SetParent(target, false);
        go.transform.localPosition = new Vector3(0f, height * 0.45f, 0f);

        var matLua = LuaCuonMat;
        var flames = NewPS("Flames", go.transform, Vector3.zero,
                           matLua != null ? matLua : FlameMat,
                           ParticleSystemRenderMode.Billboard);
        if (matLua != null) BatFlipbook(flames, 6, 6, 1);
        float bl = matLua != null ? BuLua : 1f;

        var m = flames.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.3f);
        m.startSize = new ParticleSystem.MinMaxCurve(radius * 1.0f * bl, radius * 2.0f * bl);
        m.startRotation = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = -0.25f;
        m.maxParticles = 120;
        var em = flames.emission; em.rateOverTime = 45f;
        var sh = flames.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = radius * 0.9f;
        var col = flames.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.90f, 0.55f), 0f,
            new Color(1f, 0.40f, 0.08f), 0.45f,
            new Color(0.30f, 0.03f, 0.01f), 1f,
            0f, 1f, 0.6f, 0f));
        var sol = flames.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.4f, 1f, 0.1f));

        var embers = NewPS("Embers", go.transform, Vector3.zero, EmberMat, ParticleSystemRenderMode.Billboard);
        var e2 = embers.main;
        e2.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.6f);
        e2.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        e2.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
        e2.simulationSpace = ParticleSystemSimulationSpace.World;
        e2.gravityModifier = -0.35f;
        e2.maxParticles = 80;
        var eem = embers.emission; eem.rateOverTime = 14f;
        var esh = embers.shape; esh.shapeType = ParticleSystemShapeType.Sphere; esh.radius = radius;
        var nz2 = embers.noise; nz2.enabled = true; nz2.strength = 0.6f; nz2.frequency = 0.8f;

        var lightGo = new GameObject("BurnLight");
        lightGo.transform.SetParent(go.transform, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.5f, 0.18f);
        lt.intensity = 2.2f;
        lt.range = 5f;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 2.2f; fl.amount = 0.4f; fl.speed = 9f;

        return go;
    }

    /// <summary>Lua duoc / lo suoi trong canh vat.</summary>
    public static GameObject Brazier(Vector3 pos, Transform parent, float scale)
    {
        var go = new GameObject("BrazierFire");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        var flames = NewPS("Flames", go.transform, Vector3.zero, FlameMat, ParticleSystemRenderMode.Billboard);
        var m = flames.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.7f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.45f * scale, 1.0f * scale);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = -0.35f;
        m.maxParticles = 150;
        var em = flames.emission; em.rateOverTime = 40f;
        var sh = flames.shape; sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 12f; sh.radius = 0.18f * scale;
        sh.rotation = new Vector3(-90f, 0f, 0f);
        var col = flames.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.92f, 0.60f), 0f,
            new Color(1f, 0.42f, 0.08f), 0.5f,
            new Color(0.25f, 0.03f, 0.01f), 1f,
            0f, 1f, 0.55f, 0f));
        var sol = flames.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.4f, 1f, 0.05f));
        var nz = flames.noise; nz.enabled = true; nz.strength = 0.35f; nz.frequency = 1.2f;

        var embers = NewPS("Embers", go.transform, Vector3.zero, EmberMat, ParticleSystemRenderMode.Billboard);
        var e2 = embers.main;
        e2.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 3.2f);
        e2.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.0f);
        e2.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        e2.simulationSpace = ParticleSystemSimulationSpace.World;
        e2.gravityModifier = -0.25f;
        var eem = embers.emission; eem.rateOverTime = 9f;
        var esh = embers.shape; esh.shapeType = ParticleSystemShapeType.Cone;
        esh.angle = 18f; esh.radius = 0.2f * scale; esh.rotation = new Vector3(-90f, 0f, 0f);
        var nz2 = embers.noise; nz2.enabled = true; nz2.strength = 1.1f; nz2.frequency = 0.5f;

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.5f * scale, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.52f, 0.18f);
        lt.intensity = 3.2f * scale;
        lt.range = 12f * scale;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 3.2f * scale; fl.amount = 0.22f; fl.speed = 7f; fl.rangeWobble = 0.08f;

        return go;
    }

    // ================================================================
    //  BANG
    // ================================================================

    /// <summary>Dung hinh vung bao tuyet bang code (dung khi chua co prefab).</summary>
    public static GameObject BuildIceStormField(Vector3 center, float radius, float duration)
    {
        var go = new GameObject("IceStormField");
        go.transform.position = center;
        AutoDestroy.Add(go, duration + 3f);

        // Vong phep bam theo mat dat (truoc day la tam phang nen dat go ghe hay che mat)
        var ring = GroundRing.Create("Ring", go.transform, center, new Material(RingIceMat),
                                     radius * 1.05f);
        ring.xuyenVatCan = true;                 // vong phai nhin thay tron ven qua bia mo
        ring.hold = duration + 0.4f;
        ring.fadeTime = 0.4f;
        ring.segments = 40;
        ring.rings = 4;

        // Bui tuyet roi trong vung
        var snow = NewPS("Snow", go.transform, new Vector3(0f, 6f, 0f), SnowMat, ParticleSystemRenderMode.Billboard);
        var m = snow.main;
        m.duration = duration; m.loop = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.28f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 0.6f;
        m.maxParticles = 500;
        var em = snow.emission; em.rateOverTime = 160f;
        var sh = snow.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 4f; sh.radius = radius; sh.rotation = new Vector3(90f, 0f, 0f);
        var col = snow.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.85f, 0.95f, 1f), 0f,
            new Color(0.65f, 0.88f, 1f), 0.5f,
            new Color(0.45f, 0.75f, 1f), 1f,
            0f, 1f, 1f, 0f));
        var nz = snow.noise; nz.enabled = true; nz.strength = 0.8f; nz.frequency = 0.6f;

        // Suong lanh la la mat dat
        var mist = NewPS("Mist", go.transform, new Vector3(0f, 0.25f, 0f), MistMat, ParticleSystemRenderMode.Billboard);
        var mm = mist.main;
        mm.duration = duration; mm.loop = true;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.9f);
        mm.startSize = new ParticleSystem.MinMaxCurve(radius * 0.6f, radius * 1.2f);
        mm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        mm.maxParticles = 120;
        var mem = mist.emission; mem.rateOverTime = 16f;
        var msh = mist.shape; msh.shapeType = ParticleSystemShapeType.Circle; msh.radius = radius * 0.8f;
        var mcol = mist.colorOverLifetime; mcol.enabled = true;
        mcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.75f, 0.90f, 1f), 0f,
            new Color(0.60f, 0.82f, 1f), 0.5f,
            new Color(0.45f, 0.70f, 0.95f), 1f,
            0f, 0.5f, 0.35f, 0f));

        // Anh sang lanh phu ca vung
        var lightGo = new GameObject("ColdLight");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 3f, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.45f, 0.75f, 1f);
        lt.intensity = 3.2f;
        lt.range = radius * 3.2f;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 3.2f; fl.amount = 0.18f; fl.speed = 3f;

        return go;
    }

    /// <summary>Vung bao tuyet (uu tien dung prefab).</summary>
    public static GameObject IceStormField(Vector3 center, float radius, float duration)
    {
        GameObject go;
        var pf = GameAssets.I != null ? GameAssets.I.iceStormFieldPrefab : null;

        if (pf != null)
        {
            go = GameAssets.Make(pf, center);
            float k = radius / 5.5f;                 // prefab duoc nuong o ban kinh 5.5
            if (Mathf.Abs(k - 1f) > 0.05f) go.transform.localScale = Vector3.one * k;

            var ad = go.GetComponent<AutoDestroy>();
            if (ad == null) ad = go.AddComponent<AutoDestroy>();
            ad.lifetime = duration + 3f;

            // PHAI dat lai o day nua.
            //
            // Prefab duoc nuong tu truoc, va trong do GroundRing van giu gia tri
            // mac dinh xuyenVatCan = false. Sua trong BuildXxxField thi chi an
            // khi KHONG co prefab - tuc la khong bao gio, vi prefab luon co.
            // Cung cai bay da vap voi catchRadius cua Loc xoay.
            //
            // "hold" cung vay, va con nguy hiem hon: prefab nuong cung 5,4 s /
            // 2,8 s - dung bang duration MAC DINH cua hai skill cong 0,4. Nhung
            // duration la mot truong public, nen prefab CUA SKILL de bao nhieu
            // thi luc chay la bay nhieu. Hai con so khong con dinh gi den nhau,
            // va vong tat truoc khi phep het hieu luc.
            //
            // Nay lay thang tu duration duoc truyen vao - mot nguon su that duy
            // nhat, khong con ban chep tay thu hai de lech nhau.
            foreach (var gr in go.GetComponentsInChildren<GroundRing>(true))
            {
                gr.xuyenVatCan = true;
                gr.hold = duration + 0.4f;
            }
        }
        else
        {
            go = BuildIceStormField(center, radius, duration);
        }

        // Vet bang lan tren mat dat
        GroundDecal.Spawn(new Vector3(center.x, GroundY(center), center.z), radius,
                          new Material(FrostMat), duration + 3.5f, duration * 0.6f);

        return go;
    }

    /// <summary>Dung hinh manh bang roi bang code (dung khi chua co prefab).</summary>
    // ================================================================
    //  TANG BANG DUNG TRONG BLENDER
    // ================================================================
    //
    // Sau tang lon nho hinh dang khac nhau, va bon manh vun cho luc vo. Truoc
    // day tat ca deu la ProcMesh.Crystal - mot hinh non sau canh DEU, cung mot
    // co: ca tran mua trong nhu do tu mot cai khuon duy nhat ra.

    static Mesh[] meshTang, meshManh;

    static Mesh[] NapMesh(string tienTo, int so)
    {
        var ds = new System.Collections.Generic.List<Mesh>();
        for (int i = 0; i < so; i++)
        {
            var go = Resources.Load<GameObject>("BangRoi/" + tienTo + i);
            if (go == null) continue;
            var mf = go.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) ds.Add(mf.sharedMesh);
        }
        if (ds.Count == 0)
            Debug.LogWarning("[Vfx] Khong nap duoc mesh Resources/BangRoi/" + tienTo + "*");
        return ds.ToArray();
    }

    /// <summary>Sau tang bang roi. Truc dai theo Y, dau nhon quay XUONG.</summary>
    public static Mesh[] MeshTangBang
    {
        get { if (meshTang == null) meshTang = NapMesh("TangBang", 6); return meshTang; }
    }

    /// <summary>Bon manh vun, dung cho hat kieu Mesh luc tang bang vo.</summary>
    public static Mesh[] MeshManhVo
    {
        get { if (meshManh == null) meshManh = NapMesh("ManhVo", 4); return meshManh; }
    }

    static Mesh[] meshCumGai;

    /// <summary>
    /// Bon CUM GAI BANG moc len tu mat dat sau khi tang bang vo.
    ///
    /// Moi cum la mot khoi gom bon den chin tinh the nhon chum lai, nghieng dan
    /// ra ngoai - dung dang nhung dai bang trong game mau. Ban truoc dung
    /// ProcMesh.Crystal: mot hinh non NAM canh deu, moi cho chi moc len dung
    /// mot cai, nhin ra cai coc chu khong ra cum bang.
    ///
    /// Day cum nam o y = 0 de ExpandFade keo tu 0 len 1 theo truc Y - trong nhu
    /// no dam tu duoi dat troi len.
    /// </summary>
    public static Mesh[] MeshCumGai
    {
        get { if (meshCumGai == null) meshCumGai = NapMesh("CumGai", 4); return meshCumGai; }
    }

    /// <summary>
    /// Buoc nap lai toan bo mesh tu Resources.
    ///
    /// Ba mang mesh o tren duoc nho lai sau lan doc dau, ma bien static thi song
    /// qua ca nhung lan nhap lai asset - sua mesh ben Blender xong, nhap vao
    /// Unity, ma van thay hinh cu. Goi ham nay sau khi thay file mesh.
    /// </summary>
    public static void NapLaiMesh()
    {
        meshTang = null;
        meshManh = null;
        meshCumGai = null;
    }

    public static GameObject BuildIceShardFalling(Vector3 target, float height, float fallTime)
    {
        var go = new GameObject("IceShard");
        go.transform.position = target + Vector3.up * height;

        // KHONG xoay 180 quanh X nua. Mesh tu Blender da co dau nhon quay xuong
        // -Y roi; lat them 180 do la chia nguoc len troi.
        go.transform.rotation = Quaternion.Euler(Random.Range(-13f, 13f),
                                                 Random.Range(0f, 360f),
                                                 Random.Range(-13f, 13f));

        // TANG phai TO HAN MANH VO thi luc vo moi doc ra la "mot tang vua vo
        // thanh nhieu manh".
        //
        // Truoc do tang cao 0,23-1,12 m con manh vo cao 0,08-0,47 m: hai dai
        // CHONG LEN NHAU, tang nho nhat con be hon manh to nhat - roi xuong dat
        // thi khong thay vo, chi thay mot dam manh y het cai vua roi.
        //
        // Gio tang 0,55-1,95 m (mesh Blender dai san 0,7-1,3 don vi), manh vo
        // 0,07-0,32 m. Hai dai roi han nhau, ti le trung binh khoang 6 lan.
        float size = Random.Range(1.04f, 2.08f);

        var ms = MeshTangBang;
        if (ms != null && ms.Length > 0)
        {
            var con = new GameObject("Tang");
            con.transform.SetParent(go.transform, false);
            con.transform.localScale = Vector3.one * size;
            var mf = con.AddComponent<MeshFilter>();
            mf.sharedMesh = ms[Random.Range(0, ms.Length)];
            var mr = con.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Mats.Ice;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
        else
        {
            // Chua nap duoc mesh thi van co cai gi do roi xuong, con hon khong
            ProcMesh.Part("Shard", go.transform,
                          ProcMesh.Crystal(0.11f * size, 0.62f * size, 6, 0.45f, Color.white),
                          Mats.Ice, Vector3.zero, Quaternion.identity, Vector3.one, false);
        }

        // NUOC VAN RA tren duong roi: tang bang dang tan, nuoc bi gio thoi bat
        // lai phia sau thanh mot vet giot li ti.
        //
        // Rat THUA (18 hat/giay, song 0,2-0,45 giay) va rat NHO. Tang roi het
        // 0,336 giay nen chi kip nha ra dam chuc giot - vet nuoc mo theo sau,
        // khong phai mot voi nuoc.
        var vetNuoc = NewPS("NuocVan", go.transform, Vector3.zero, NuocMat,
                            ParticleSystemRenderMode.Stretch);
        var rvn = vetNuoc.GetComponent<ParticleSystemRenderer>();
        rvn.velocityScale = 0.05f;
        rvn.lengthScale = 2.0f;
        rvn.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rvn.receiveShadows = false;

        var mvn = vetNuoc.main;
        mvn.startLifetime = new ParticleSystem.MinMaxCurve(0.20f, 0.45f);
        mvn.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.5f);
        mvn.startSize = new ParticleSystem.MinMaxCurve(0.07f * size, 0.17f * size);
        mvn.simulationSpace = ParticleSystemSimulationSpace.World;
        mvn.gravityModifier = 1.4f;
        mvn.maxParticles = 30;
        mvn.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.70f, 0.88f, 1f, 0.90f), new Color(0.90f, 0.97f, 1f, 0.75f));
        var evn = vetNuoc.emission; evn.rateOverTime = 18f;
        var svn = vetNuoc.shape;
        svn.shapeType = ParticleSystemShapeType.Sphere;
        svn.radius = 0.14f * size;
        var cvn = vetNuoc.colorOverLifetime;
        cvn.enabled = true;
        cvn.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.45f));

        // KHI LANH bao quanh tang bang tren duong roi: suong trang xanh toa ra
        // hai ben roi tan dan, khong phai nhung dom bang lap lanh.
        var suong = NewPS("KhiLanh", go.transform, Vector3.zero, MistMat,
                          ParticleSystemRenderMode.Billboard);
        var sm = suong.main;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.70f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.35f * size, 0.85f * size);
        sm.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.maxParticles = 60;
        sm.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.62f, 0.84f, 1f, 0.30f), new Color(0.88f, 0.96f, 1f, 0.42f));
        var sem = suong.emission; sem.rateOverTime = 38f;
        var ssh = suong.shape;
        ssh.shapeType = ParticleSystemShapeType.Sphere;
        ssh.radius = 0.16f * size;
        var scol = suong.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.4f, Color.white, 1f,
            0f, 0.55f, 0.35f, 0f));
        var ssz = suong.sizeOverLifetime; ssz.enabled = true;
        ssz.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0.55f), new Keyframe(1f, 1.7f)));

        // Duoi bang lap lanh keo theo
        var trail = NewPS("Trail", go.transform, Vector3.zero, ShardMat, ParticleSystemRenderMode.Billboard);
        var m = trail.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.6f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 80;
        var em = trail.emission; em.rateOverTime = 55f;
        var sh = trail.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.12f;
        var col = trail.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.9f, 0.97f, 1f), 0f,
            new Color(0.6f, 0.85f, 1f), 0.5f,
            new Color(0.35f, 0.6f, 0.95f), 1f,
            1f, 1f, 0.6f, 0f));

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(go.transform, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.5f, 0.8f, 1f);
        lt.intensity = 1.8f; lt.range = 5f; lt.shadows = LightShadows.None;

        var mover = go.AddComponent<FallingShard>();
        mover.target = target;
        mover.travelTime = fallTime;
        return go;
    }

    /// <summary>Manh bang roi tu tren troi xuong (uu tien dung prefab).</summary>
    public static GameObject IceShardFalling(Vector3 target, float height, float fallTime)
    {
        var pf = GameAssets.I != null ? GameAssets.I.iceShardPrefab : null;
        if (pf == null) return BuildIceShardFalling(target, height, fallTime);

        var go = GameAssets.Make(pf, target + Vector3.up * height,
                                 Quaternion.Euler(Random.Range(-13f, 13f),
                                                  Random.Range(0f, 360f),
                                                  Random.Range(-13f, 13f)), null);

        // PREFAB CHI GIU DUOC MOT hinh va MOT co - no la anh chup mot the hien
        // duy nhat. Khong boc lai o day thi ca tran mua bang chi co dung mot
        // tang bang lap di lap lai, tuc mat sach cai "lon nho hinh dang khac
        // nhau" ma cong dung sau cai mesh de lam.
        var tang = go.transform.Find("Tang");
        if (tang != null)
        {
            var ms = MeshTangBang;
            var mf = tang.GetComponent<MeshFilter>();
            if (ms != null && ms.Length > 0 && mf != null)
                mf.sharedMesh = ms[Random.Range(0, ms.Length)];
            tang.localScale = Vector3.one * Random.Range(1.04f, 2.08f);
        }

        var mover = go.GetComponent<FallingShard>();
        if (mover == null) mover = go.AddComponent<FallingShard>();
        mover.target = target;
        mover.travelTime = fallTime;
        return go;
    }

    /// <summary>Dung hinh vu no bang bang code (dung khi chua co prefab).</summary>
    public static GameObject BuildIceImpact(Vector3 pos, float radius)
    {
        var root = new GameObject("IceImpact");
        root.transform.position = pos;
        AutoDestroy.Add(root, 4f);

        // 1) Chop sang lanh
        var flash = ProcMesh.Part("Flash", root.transform, ProcMesh.Sphere(1f, 14, 10, 1f, Color.white),
                                  new Material(Mats.Ice), Vector3.zero, Quaternion.identity, Vector3.one, false);
        var ef = flash.AddComponent<ExpandFade>();
        ef.duration = 0.35f;
        ef.startScale = Vector3.one * radius * 0.2f;
        ef.endScale = Vector3.one * radius * 1.1f;

        // 2) Vong bang lan ra tren dat
        var ring = ProcMesh.Part("Ring", root.transform, GroundDecal.QuadMesh(), new Material(RingIceMat),
                                 new Vector3(0f, 0.05f, 0f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, false);
        var rf = ring.AddComponent<ExpandFade>();
        rf.duration = 0.6f;
        rf.startScale = Vector3.one * radius * 0.3f;
        rf.endScale = Vector3.one * radius * 3f;

        // 3) Gai bang tu duoi dat troi len
        int spikes = Random.Range(7, 11);
        for (int i = 0; i < spikes; i++)
        {
            float a = Random.Range(0f, 360f);
            float d = Random.Range(0.15f, 0.85f) * radius;
            Vector3 p = pos + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, d);
            p.y = GroundY(p);

            float h = Random.Range(0.5f, 1.3f);

            GameObject spike;
            var cg = MeshCumGai;
            if (cg != null && cg.Length > 0)
            {
                spike = new GameObject("CumGai" + i);
                spike.transform.SetParent(root.transform, false);
                spike.transform.localPosition = p - root.transform.position;
                // Chi xoay quanh truc DUNG. Nghieng nguyen ca cum di thi chan
                // cum nhac khoi mat dat, nhin ra cai cum bang dang bay lo lung.
                spike.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                // 1,0925 = 0,95 x 1,15 - to them 15%.
                spike.transform.localScale = Vector3.one * (h * 1.0925f);

                var mfg = spike.AddComponent<MeshFilter>();
                mfg.sharedMesh = cg[Random.Range(0, cg.Length)];
                var mrg = spike.AddComponent<MeshRenderer>();
                mrg.sharedMaterial = CumBangMat;
                mrg.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mrg.receiveShadows = false;

                // HAO QUANG o chan cum. Vien fresnel cua shader chi sang o meo
                // nhin gan tiep tuyen, nen tu xa cum bang khong "phat sang" chut
                // nao. Mot dia sang nam ngang duoi chan thi goc nhin nao cung
                // thay - va no hat mau xanh len phan duoi cua cum.
                var hq = ProcMesh.Part("HaoQuang" + i, spike.transform,
                                       GroundDecal.QuadMesh(), HaoQuangBangMat,
                                       new Vector3(0f, 0.04f, 0f),
                                       Quaternion.Euler(90f, 0f, 0f),
                                       Vector3.one * 1.6f, false);
                var hf = hq.AddComponent<ExpandFade>();
                hf.duration = Random.Range(1.6f, 2.6f);
                hf.startScale = Vector3.one * 0.4f;
                hf.endScale = Vector3.one * 1.9f;
            }
            else
            {
                spike = ProcMesh.Part("Spike" + i, root.transform,
                                      ProcMesh.Crystal(0.16f, h, 5, 0.2f, Color.white),
                                      Mats.Ice, p - root.transform.position,
                                      Quaternion.Euler(Random.Range(-22f, 22f), Random.Range(0f, 360f), Random.Range(-22f, 22f)),
                                      Vector3.one, false);
            }

            var sf = spike.AddComponent<ExpandFade>();
            sf.duration = Random.Range(1.6f, 2.6f);
            // Giu nguyen be ngang, chi keo CHIEU CAO tu 0 len - dung kieu dam
            // tu duoi dat troi len. Dat endScale = Vector3.one thi cum bi ep ve
            // co 1,0 va mat het cai localScale vua dat o tren.
            Vector3 co = spike.transform.localScale;
            sf.startScale = new Vector3(co.x, co.y * 0.05f, co.z);
            sf.endScale = co;
            sf.ease = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.18f, 1f), new Keyframe(1f, 1f));
        }

        // 3b) NUOC BAN TUNG TOE.
        //
        // Dung ALPHA chu khong additive: nuoc that che bot nen phia sau chu
        // khong lam nen sang len. Kieu ve STRETCH cho moi giot keo dai theo
        // huong bay - giot tron thi nhin ra hat tuyet, khong ra nuoc.
        var giot = NewPS("GiotNuoc", root.transform, new Vector3(0f, 0.12f, 0f),
                         NuocMat, ParticleSystemRenderMode.Stretch);
        var rg = giot.GetComponent<ParticleSystemRenderer>();
        rg.velocityScale = 0.055f;
        rg.lengthScale = 1.6f;
        rg.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rg.receiveShadows = false;

        var mg = giot.main;
        mg.duration = 0.22f; mg.loop = false;
        mg.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.05f);
        mg.startSpeed = new ParticleSystem.MinMaxCurve(2.4f, 6.2f);
        // 0,10-0,28 chu khong 0,05-0,16. Ban dau de nho that de "giong giot
        // nuoc", nhung trong canh dem thi 34 giot ay gan nhu VO HINH tren anh
        // chup - dem duoc so hat song ma nhin khong ra.
        mg.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.28f);
        mg.simulationSpace = ParticleSystemSimulationSpace.World;
        mg.gravityModifier = 3.2f;      // nuoc roi nhanh, khong lo lung nhu suong
        mg.maxParticles = 70;
        mg.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.66f, 0.86f, 1f, 0.95f), new Color(0.88f, 0.96f, 1f, 0.85f));

        var eg = giot.emission;
        eg.rateOverTime = 0f;
        eg.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 46) });

        var sg = giot.shape;
        sg.shapeType = ParticleSystemShapeType.Hemisphere;
        sg.radius = radius * 0.20f;

        var cg2 = giot.colorOverLifetime;
        cg2.enabled = true;
        cg2.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.55f));

        // 3c) GON SONG lan tren mat dat.
        //
        // HorizontalBillboard: vong luon NAM NGANG. Billboard thuong thi vong
        // quay theo camera va gon song dung thang len nhu mot cai vong co.
        //
        // Ba nhip lan tiep nhau (0 / 0,16 / 0,34 giay) - mot vong don thi ra
        // "mot cai vong lan ra", ba vong noi nhau moi thanh gon song.
        var song = NewPS("GonSongNuoc", root.transform, new Vector3(0f, 0.09f, 0f),
                         GonSongMat, ParticleSystemRenderMode.HorizontalBillboard);
        var rs = song.GetComponent<ParticleSystemRenderer>();
        rs.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rs.receiveShadows = false;

        var msg = song.main;
        msg.duration = 0.6f; msg.loop = false;
        msg.startLifetime = 1.0f;
        msg.startSpeed = 0f;
        msg.startSize = radius * 0.70f;
        msg.simulationSpace = ParticleSystemSimulationSpace.World;
        msg.maxParticles = 8;
        msg.startColor = new Color(0.70f, 0.88f, 1f, 0.90f);

        var esg = song.emission;
        esg.rateOverTime = 0f;
        esg.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 1),
            new ParticleSystem.Burst(0.16f, 1),
            new ParticleSystem.Burst(0.34f, 1),
        });

        var ssg = song.shape;
        ssg.enabled = false;

        var szg = song.sizeOverLifetime;
        szg.enabled = true;
        szg.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.30f), new Keyframe(1f, 3.6f)));

        var csg = song.colorOverLifetime;
        csg.enabled = true;
        csg.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.30f));

        // 4a) MANH BANG THAT van ra: hat kieu MESH, moi hat lay mot trong bon
        //     manh vun dung trong Blender.
        //
        // Dung he hat chu khong tao tung GameObject: he hat lo san chuyen dong,
        // trong luc, xoay lon va tu don dep - tao tay thi phai viet lai het.
        var mv = MeshManhVo;
        if (mv != null && mv.Length > 0)
        {
            var manh3D = NewPS("Manh3D", root.transform, Vector3.zero, Mats.Ice,
                               ParticleSystemRenderMode.Mesh);
            var r3 = manh3D.GetComponent<ParticleSystemRenderer>();
            r3.SetMeshes(mv, mv.Length);
            r3.alignment = ParticleSystemRenderSpace.World;
            r3.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r3.receiveShadows = false;

            var m3 = manh3D.main;
            m3.duration = 0.25f; m3.loop = false;
            m3.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.5f);
            m3.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 7.5f);
            m3.startSize = new ParticleSystem.MinMaxCurve(0.30f, 0.70f);
            m3.startRotation3D = true;
            m3.startRotationX = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m3.startRotationY = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m3.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m3.simulationSpace = ParticleSystemSimulationSpace.World;
            m3.gravityModifier = 2.0f;
            m3.maxParticles = 40;
            m3.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.72f, 0.92f, 1f), new Color(0.96f, 0.99f, 1f));

            var e3 = manh3D.emission;
            e3.rateOverTime = 0f;
            e3.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 22) });

            var s3 = manh3D.shape;
            s3.shapeType = ParticleSystemShapeType.Hemisphere;
            s3.radius = radius * 0.22f;

            // Manh vun xoay lon trong khong khi khi bay
            var rot3 = manh3D.rotationOverLifetime;
            rot3.enabled = true;
            rot3.separateAxes = true;
            rot3.x = new ParticleSystem.MinMaxCurve(-4.5f, 4.5f);
            rot3.y = new ParticleSystem.MinMaxCurve(-4.5f, 4.5f);
            rot3.z = new ParticleSystem.MinMaxCurve(-4.5f, 4.5f);

            var c3 = manh3D.colorOverLifetime;
            c3.enabled = true;
            c3.color = new ParticleSystem.MinMaxGradient(Grad(
                Color.white, 0f, Color.white, 0.55f, Color.white, 1f,
                1f, 1f, 1f, 0f));
        }

        // 4) Bui vun nho van ra tu tam no
        var shards = NewPS("Shards", root.transform, Vector3.zero, ShardMat, ParticleSystemRenderMode.Stretch);
        var m = shards.main;
        m.duration = 0.3f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 9f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 1.6f;
        m.maxParticles = 200;
        var em = shards.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 55) });
        var sh = shards.shape; sh.shapeType = ParticleSystemShapeType.Hemisphere; sh.radius = radius * 0.25f;
        var col = shards.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.95f, 0.99f, 1f), 0f,
            new Color(0.65f, 0.88f, 1f), 0.5f,
            new Color(0.35f, 0.60f, 0.95f), 1f,
            1f, 1f, 0.8f, 0f));

        // 5) Hoi lanh phut ra la la mat dat
        var mist = NewPS("Mist", root.transform, Vector3.zero, MistMat, ParticleSystemRenderMode.Billboard);
        var mm = mist.main;
        mm.duration = 0.4f; mm.loop = false;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        mm.startSize = new ParticleSystem.MinMaxCurve(radius * 0.7f, radius * 1.5f);
        mm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        mm.maxParticles = 90;
        var mem = mist.emission;
        mem.rateOverTime = 0f;
        mem.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 24) });
        var msh = mist.shape; msh.shapeType = ParticleSystemShapeType.Circle; msh.radius = radius * 0.3f;
        var mcol = mist.colorOverLifetime; mcol.enabled = true;
        mcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.85f, 0.95f, 1f), 0f,
            new Color(0.62f, 0.85f, 1f), 0.5f,
            new Color(0.45f, 0.70f, 0.95f), 1f,
            0f, 0.75f, 0.4f, 0f));
        var msol = mist.sizeOverLifetime; msol.enabled = true;
        msol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.4f, 1.1f, 1.7f));

        // 6) Anh sang lanh loe len
        var lightGo = new GameObject("IceLight");
        lightGo.transform.SetParent(root.transform, false);
        var lb = lightGo.AddComponent<LightBurst>();
        lb.peak = 14f; lb.duration = 0.45f; lb.range = radius * 7f;
        var lt = lightGo.AddComponent<Light>();
        lt.color = new Color(0.5f, 0.8f, 1f);

        return root;
    }

    /// <summary>NO BANG: manh bang vo tung, hoi lanh phut ra, gai bang moc len.</summary>
    public static void IceImpact(Vector3 pos, float radius)
    {
        var pf = GameAssets.I != null ? GameAssets.I.iceImpactPrefab : null;
        if (pf != null)
        {
            var go = GameAssets.Make(pf, pos);
            float k = radius / 1.7f;                 // prefab duoc nuong o ban kinh 1.7
            if (Mathf.Abs(k - 1f) > 0.05f) go.transform.localScale = Vector3.one * k;
        }
        else
        {
            BuildIceImpact(pos, radius);
        }

        // Chi mot phan nho de lai vet bang cho do roi mat
        if (Random.value < 0.35f)
            GroundDecal.Spawn(new Vector3(pos.x, GroundY(pos), pos.z), radius * 0.9f,
                              new Material(FrostMat), 6f, 3f);
    }

    /// <summary>Vo bang VO TAN khi quai dang dong bang bi tieu diet.</summary>
    public static void FrozenShatter(Vector3 pos, float scale)
    {
        var pf = GameAssets.I != null ? GameAssets.I.frozenShatterPrefab : null;
        if (pf != null)
        {
            var inst = GameAssets.Make(pf, pos);
            if (Mathf.Abs(scale - 1f) > 0.05f) inst.transform.localScale = Vector3.one * scale;
            return;
        }
        BuildFrozenShatter(pos, scale);
    }

    /// <summary>Dung hinh vo bang vo tan bang code.</summary>
    public static GameObject BuildFrozenShatter(Vector3 pos, float scale)
    {
        var root = new GameObject("Shatter");
        root.transform.position = pos;
        AutoDestroy.Add(root, 3f);

        var shards = NewPS("Shards", root.transform, Vector3.zero, ShardMat, ParticleSystemRenderMode.Stretch);
        var m = shards.main;
        m.duration = 0.2f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.6f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 8f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.15f * scale, 0.40f * scale);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 2f;
        m.maxParticles = 180;
        var em = shards.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 70) });
        var sh = shards.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.35f * scale;
        var col = shards.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.95f, 0.99f, 1f), 0f,
            new Color(0.60f, 0.85f, 1f), 0.6f,
            new Color(0.30f, 0.55f, 0.90f), 1f,
            1f, 1f, 0.9f, 0f));

        var mist = NewPS("Puff", root.transform, Vector3.zero, MistMat, ParticleSystemRenderMode.Billboard);
        var mm = mist.main;
        mm.duration = 0.2f; mm.loop = false;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mm.startSize = new ParticleSystem.MinMaxCurve(0.7f * scale, 1.6f * scale);
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        var mem = mist.emission;
        mem.rateOverTime = 0f;
        mem.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18) });
        var msh = mist.shape; msh.shapeType = ParticleSystemShapeType.Sphere; msh.radius = 0.4f * scale;

        var lightGo = new GameObject("Flash");
        lightGo.transform.SetParent(root.transform, false);
        var lb = lightGo.AddComponent<LightBurst>();
        lb.peak = 9f; lb.duration = 0.35f; lb.range = 7f * scale;
        var lt = lightGo.AddComponent<Light>();
        lt.color = new Color(0.6f, 0.85f, 1f);
        return root;
    }

    // ================================================================
    //  SAM SET
    // ================================================================

    /// <summary>Dung hinh vung giong bang code (dung khi chua co prefab).</summary>
    public static GameObject BuildLightningStormField(Vector3 center, float radius, float duration)
    {
        var go = new GameObject("LightningStormField");
        go.transform.position = center;
        AutoDestroy.Add(go, duration + 3f);

        // 1) Vong phep dien bam theo mat dat
        var ring = GroundRing.Create("Ring", go.transform, center, new Material(RingBoltMat),
                                     radius * 1.05f);
        ring.xuyenVatCan = true;                 // vong phai nhin thay tron ven qua bia mo
        ring.hold = duration + 0.4f;
        ring.fadeTime = 0.4f;
        ring.segments = 40;
        ring.rings = 4;

        // 2) May den tu tap tren cao
        var cloud = NewPS("Cloud", go.transform, new Vector3(0f, 7.5f, 0f), StormCloudMat,
                          ParticleSystemRenderMode.Billboard);
        var cm = cloud.main;
        cm.duration = duration; cm.loop = true;
        cm.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 3.4f);
        cm.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.1f);
        cm.startSize = new ParticleSystem.MinMaxCurve(radius * 1.1f, radius * 2.2f);
        cm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        cm.simulationSpace = ParticleSystemSimulationSpace.World;
        cm.maxParticles = 90;
        var cem = cloud.emission; cem.rateOverTime = 18f;
        var csh = cloud.shape; csh.shapeType = ParticleSystemShapeType.Circle; csh.radius = radius * 0.9f;
        var ccol = cloud.colorOverLifetime; ccol.enabled = true;
        ccol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.30f, 0.34f, 0.50f), 0f,
            new Color(0.14f, 0.16f, 0.28f), 0.45f,
            new Color(0.06f, 0.07f, 0.14f), 1f,
            0f, 0.75f, 0.5f, 0f));
        var crot = cloud.rotationOverLifetime; crot.enabled = true;
        crot.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);

        // 3) Bui dien nho bay nguoc len tu mat dat (khong khi dang tich dien)
        var motes = NewPS("Static", go.transform, new Vector3(0f, 0.15f, 0f), SparkBlueMat,
                          ParticleSystemRenderMode.Billboard);
        var sm = motes.main;
        sm.duration = duration; sm.loop = true;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.4f);
        sm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.gravityModifier = -0.25f;
        sm.maxParticles = 260;
        var sem = motes.emission; sem.rateOverTime = 85f;
        var ssh = motes.shape; ssh.shapeType = ParticleSystemShapeType.Circle; ssh.radius = radius * 0.95f;
        var scol = motes.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.66f, 0.85f, 1f), 0.5f,
            new Color(0.35f, 0.60f, 1f), 1f,
            0f, 1f, 0.8f, 0f));
        var snz = motes.noise; snz.enabled = true; snz.strength = 0.7f; snz.frequency = 1.1f;

        // 4) Anh sang xanh trang nhap nhay phu ca vung
        var lightGo = new GameObject("StormLight");
        lightGo.transform.SetParent(go.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.62f, 0.80f, 1f);
        lt.intensity = 3.6f;
        lt.range = radius * 3.4f;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 3.6f; fl.amount = 0.55f; fl.speed = 16f; fl.rangeWobble = 0.1f;

        return go;
    }

    /// <summary>Vung giong sam set (uu tien dung prefab).</summary>
    public static GameObject LightningStormField(Vector3 center, float radius, float duration)
    {
        GameObject go;
        var pf = GameAssets.I != null ? GameAssets.I.lightningStormFieldPrefab : null;

        if (pf != null)
        {
            go = GameAssets.Make(pf, center);
            float k = radius / 6f;                    // prefab duoc nuong o ban kinh 6
            if (Mathf.Abs(k - 1f) > 0.05f) go.transform.localScale = Vector3.one * k;

            var ad = go.GetComponent<AutoDestroy>();
            if (ad == null) ad = go.AddComponent<AutoDestroy>();
            ad.lifetime = duration + 3f;

            // PHAI dat lai o day nua.
            //
            // Prefab duoc nuong tu truoc, va trong do GroundRing van giu gia tri
            // mac dinh xuyenVatCan = false. Sua trong BuildXxxField thi chi an
            // khi KHONG co prefab - tuc la khong bao gio, vi prefab luon co.
            // Cung cai bay da vap voi catchRadius cua Loc xoay.
            //
            // "hold" cung vay, va con nguy hiem hon: prefab nuong cung 5,4 s /
            // 2,8 s - dung bang duration MAC DINH cua hai skill cong 0,4. Nhung
            // duration la mot truong public, nen prefab CUA SKILL de bao nhieu
            // thi luc chay la bay nhieu. Hai con so khong con dinh gi den nhau,
            // va vong tat truoc khi phep het hieu luc.
            //
            // Nay lay thang tu duration duoc truyen vao - mot nguon su that duy
            // nhat, khong con ban chep tay thu hai de lech nhau.
            foreach (var gr in go.GetComponentsInChildren<GroundRing>(true))
            {
                gr.xuyenVatCan = true;
                gr.hold = duration + 0.4f;
            }
        }
        else
        {
            go = BuildLightningStormField(center, radius, duration);
        }

        return go;
    }

    /// <summary>Khoanh sang mo bao truoc cho set sap danh xuong.</summary>
    public static GameObject LightningWarning(Vector3 pos, float radius, float life)
    {
        var go = new GameObject("StrikeWarning");
        go.transform.position = new Vector3(pos.x, GroundY(pos) + 0.05f, pos.z);
        AutoDestroy.Add(go, life + 0.1f);

        var ring = GroundRing.Create("Mark", go.transform, go.transform.position,
                                     new Material(RingBoltMat), radius * 1.2f);
        // Khoanh nay la CHI BAO "set sap danh xuong day" - bi mot tam bia che
        // mat thi nguoi choi khong kip tranh. Cho no ve xuyen qua nhu vong vung.
        ring.xuyenVatCan = true;
        ring.startRadius = radius * 1.2f;
        ring.endRadius = radius * 0.55f;                // thu nho lai -> nhu dang ngam ban
        ring.duration = Mathf.Max(0.05f, life);
        ring.segments = 22;
        ring.rings = 2;

        return go;
    }

    /// <summary>Dung hinh cho set cham dat bang code (dung khi chua co prefab).</summary>
    public static GameObject BuildLightningImpact(Vector3 pos, float radius)
    {
        var root = new GameObject("LightningImpact");
        root.transform.position = pos;
        AutoDestroy.Add(root, 3.5f);

        float groundY = GroundY(pos);
        float lift = groundY - pos.y;

        // 1) Vung sang trang loang rong tren mat dat
        var pool = GroundRing.Create("Pool", root.transform, root.transform.position,
                                     new Material(PoolMat), radius * 0.4f);
        pool.startRadius = radius * 0.4f;
        pool.endRadius = radius * 1.3f;
        pool.duration = 0.55f;
        pool.lift = 0.16f;
        pool.segments = 24; pool.rings = 3;

        // 2) Loi sang trang chay o ngay tam
        var hot = GroundRing.Create("Hot", root.transform, root.transform.position,
                                    new Material(PoolMat), radius * 0.18f);
        hot.startRadius = radius * 0.18f;
        hot.endRadius = radius * 0.75f;
        hot.duration = 0.3f;
        hot.lift = 0.19f;
        hot.segments = 20; hot.rings = 2;

        // 3) Song xung kich hinh vong tron lan ra
        var ring = GroundRing.Create("Ring", root.transform, root.transform.position,
                                     new Material(RingBoltMat), radius * 0.2f);
        ring.startRadius = radius * 0.2f;
        ring.endRadius = radius * 1.1f;
        ring.duration = 0.5f;
        ring.lift = 0.22f;
        ring.segments = 24; ring.rings = 2;

        // 4) Cot sang dung boc quanh tia set (hai mieng cat cheo nhau)
        for (int i = 0; i < 2; i++)
        {
            var col = ProcMesh.Part("Column" + i, root.transform, GroundDecal.QuadMesh(),
                                    new Material(BoltGlowMat),
                                    new Vector3(0f, lift + radius * 1.5f, 0f),
                                    Quaternion.Euler(0f, i * 90f, 0f), Vector3.one, false);
            var cf = col.AddComponent<ExpandFade>();
            cf.duration = 0.4f;
            cf.startScale = new Vector3(radius * 1.4f, radius * 3.2f, 1f);
            cf.endScale = new Vector3(radius * 0.35f, radius * 3.2f, 1f);
        }

        // 5) Tan dien ban toe ra moi phia roi roi xuong
        var sparks = NewPS("Sparks", root.transform, new Vector3(0f, lift + 0.1f, 0f), SparkBlueMat,
                           ParticleSystemRenderMode.Stretch);
        var m = sparks.main;
        m.duration = 0.25f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 1.1f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(4f, 12f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 1.5f;
        m.maxParticles = 220;
        var em = sparks.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 80) });
        var sh = sparks.shape; sh.shapeType = ParticleSystemShapeType.Hemisphere; sh.radius = radius * 0.25f;
        var col2 = sparks.colorOverLifetime; col2.enabled = true;
        col2.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.70f, 0.88f, 1f), 0.45f,
            new Color(0.30f, 0.55f, 1f), 1f,
            1f, 1f, 0.85f, 0f));

        // 6) Mot chum tia phut thang len troi tai diem cham
        var jet = NewPS("Jet", root.transform, new Vector3(0f, lift + 0.1f, 0f), SparkBlueMat,
                        ParticleSystemRenderMode.Stretch);
        var jm = jet.main;
        jm.duration = 0.2f; jm.loop = false;
        jm.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        jm.startSpeed = new ParticleSystem.MinMaxCurve(7f, 16f);
        jm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
        jm.simulationSpace = ParticleSystemSimulationSpace.World;
        jm.gravityModifier = 2.2f;
        jm.maxParticles = 120;
        var jem = jet.emission;
        jem.rateOverTime = 0f;
        jem.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 34) });
        var jsh = jet.shape;
        jsh.shapeType = ParticleSystemShapeType.Cone;
        jsh.angle = 9f; jsh.radius = radius * 0.12f; jsh.rotation = new Vector3(-90f, 0f, 0f);

        // 7) Bui dat boc len sau cu danh
        var dust = NewPS("Dust", root.transform, new Vector3(0f, lift + 0.05f, 0f), SmokeMat,
                         ParticleSystemRenderMode.Billboard);
        var dm = dust.main;
        dm.duration = 0.3f; dm.loop = false;
        dm.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.6f);
        dm.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
        dm.startSize = new ParticleSystem.MinMaxCurve(radius * 0.7f, radius * 1.5f);
        dm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        dm.simulationSpace = ParticleSystemSimulationSpace.World;
        dm.maxParticles = 60;
        var dem = dust.emission;
        dem.rateOverTime = 0f;
        dem.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 16) });
        var dsh = dust.shape; dsh.shapeType = ParticleSystemShapeType.Circle; dsh.radius = radius * 0.35f;
        var dcol = dust.colorOverLifetime; dcol.enabled = true;
        dcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.55f, 0.60f, 0.72f), 0f,
            new Color(0.28f, 0.30f, 0.38f), 0.45f,
            new Color(0.10f, 0.11f, 0.16f), 1f,
            0f, 0.55f, 0.3f, 0f));
        var dsol = dust.sizeOverLifetime; dsol.enabled = true;
        dsol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.5f, 1.2f, 2f));

        // 8) Chop sang chua mat
        var lightGo = new GameObject("BoltLight");
        lightGo.transform.SetParent(root.transform, false);
        var lb = lightGo.AddComponent<LightBurst>();
        lb.peak = 34f; lb.duration = 0.4f; lb.range = radius * 9f;
        var lt = lightGo.AddComponent<Light>();
        lt.color = new Color(0.80f, 0.90f, 1f);

        return root;
    }

    /// <summary>SET CHAM DAT: sang loa, dat rung, tan dien ban ra (uu tien dung prefab).</summary>
    /// <summary>
    /// CHO SET VUA DANH TRUNG: khoi den boc len, than do am i, vet chay tren dat.
    ///
    /// Khong dung LightningImpact o day. LightningImpact toe ra mot dam TAN DIEN
    /// XANH cung mau voi tia set - nhin ra "co dien o day", khong ra "cho nay
    /// vua bi danh chay". Ba lop dat canh nhau moi doc ra duoc canh chay:
    ///
    ///   1. KHOI DEN boc len va NO RA khi len cao. Khoi that loang dan chu
    ///      khong giu nguyen be ngang, nen sizeOverLifetime la phan quan trong
    ///      nhat - thieu no thi ra mot cot xam thang duon.
    ///   2. THAN DO li ti bay len roi tat. Khong co no thi khoi den tren nen
    ///      dem gan nhu vo hinh.
    ///   3. VET CHAY tren mat dat, o lai 4 giay sau khi khoi tan het.
    /// </summary>
    public static void SetChayDen(Vector3 pos, float radius)
    {
        SetChayDen(pos, radius, 1f);
    }

    /// <summary>
    /// Nhu tren nhung chinh duoc LUONG KHOI. Sam set tha 22 tia trong 2,4 giay -
    /// moi tia mot dam khoi day nhu Giut set thi ca vung chim trong khoi va
    /// khong con nhin ra san choi.
    /// </summary>
    public static void SetChayDen(Vector3 pos, float radius, float mucDo)
    {
        mucDo = Mathf.Clamp(mucDo, 0.1f, 1f);
        var root = new GameObject("SetChayDen");
        root.transform.position = pos;
        AutoDestroy.Add(root, 3.2f);

        // ---- 1. KHOI DEN ----
        var khoi = NewPS("KhoiDen", root.transform, new Vector3(0f, 0.12f, 0f),
                         KhoiSetMat, ParticleSystemRenderMode.Billboard);
        var m = khoi.main;
        m.duration = 0.5f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.85f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.55f, 1.55f);
        m.startSize = new ParticleSystem.MinMaxCurve(radius * 0.50f, radius * 1.10f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        // Trong luc AM: khoi nhe hon khong khi nen no BOC LEN, khong roi xuong.
        m.gravityModifier = -0.16f;
        m.maxParticles = 40;
        // KHOI DEN van phai SANG HON MOT CHUT thi moi nhin ra.
        //
        // Ban dau to (0,10 0,09 0,08) - dung mau khoi den that. Do trong Play
        // mode: hat co that, dung cho, boc len toi y = 2,83, ma tren anh chup
        // KHONG NHIN THAY GI - mau hat ra RGBA(47, 43, 39, 111), gan het het
        // vao nen dem. Xam dam thi van doc ra la khoi chay, ma lai noi len
        // duoc tren ca mat dat lan bau troi.
        // Mau nay NHAN voi tint cua KhoiSetMat (0,42 0,39 0,36), nen mau cuoi
        // cung ra khoang (0,26 0,23 0,19) - xam rat dam.
        //
        // Da thu ha them mot bac nua (0,50) cho "den" hon: khoi mo han di, tren
        // anh chi con mot vet xam nhat. Do la cai gia phai tra de nhin thay
        // khoi tren nen dem - muon no doc ra la khoi thi phai de no sang hon
        // nen, khong ha them duoc nua.
        m.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.62f, 0.58f, 0.54f, 1f), new Color(0.92f, 0.88f, 0.84f, 0.95f));

        var e = khoi.emission;
        e.rateOverTime = 0f;
        e.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)Mathf.Max(2, 12 * mucDo)),
            new ParticleSystem.Burst(0.10f, (short)Mathf.Max(1, 8 * mucDo)),
            new ParticleSystem.Burst(0.24f, (short)Mathf.Max(1, 6 * mucDo)),
        });

        var sh = khoi.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 18f;
        sh.radius = radius * 0.23f;
        sh.rotation = new Vector3(-90f, 0f, 0f);      // mieng non huong LEN TROI

        // No ra khi len cao - day la thu lam no ra KHOI chu khong ra cot xam
        var sz = khoi.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.42f), new Keyframe(0.45f, 0.95f), new Keyframe(1f, 1.62f)));

        var co = khoi.colorOverLifetime;
        co.enabled = true;
        co.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.35f));

        var rot = khoi.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-1.1f, 1.1f);

        // ---- 2. THAN DO am i ----
        var than = NewPS("ThanDo", root.transform, new Vector3(0f, 0.08f, 0f),
                         EmberMat, ParticleSystemRenderMode.Billboard);
        var m2 = than.main;
        m2.duration = 0.3f; m2.loop = false;
        m2.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
        m2.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 2.1f);
        m2.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.10f);
        m2.simulationSpace = ParticleSystemSimulationSpace.World;
        m2.gravityModifier = -0.05f;
        m2.maxParticles = 26;
        m2.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.42f, 0.10f, 1f), new Color(1f, 0.72f, 0.28f, 1f));

        var e2 = than.emission;
        e2.rateOverTime = 0f;
        e2.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)Mathf.Max(2, 14 * mucDo)) });

        var sh2 = than.shape;
        sh2.shapeType = ParticleSystemShapeType.Hemisphere;
        sh2.radius = radius * 0.22f;

        var co2 = than.colorOverLifetime;
        co2.enabled = true;
        co2.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.25f));

        // ---- 3. VET CHAY tren dat ----
        var tam = new Vector3(pos.x, GroundY(pos), pos.z);
        GroundDecal.Spawn(tam, radius * 0.85f, new Material(ScorchMat), 4f, 2.2f);
    }

    /// <summary>Mot mieng vuong 4 dinh MOI. Khong dung chung QuadMesh() duoc:
    /// GroundDecal.OnDestroy huy luon mesh cua no, ma QuadMesh() la mesh TINH
    /// dung chung - huy mot lan la moi thu khac dung no cung mat hinh.</summary>
    static Mesh MiengVuongMoi()
    {
        var m = new Mesh();
        m.name = "VetNam";
        m.vertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),   new Vector3(-0.5f, 0.5f, 0f) };
        m.uv = new Vector2[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        m.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        m.normals = new Vector3[] { -Vector3.forward, -Vector3.forward,
                                    -Vector3.forward, -Vector3.forward };
        m.RecalculateBounds();
        return m;
    }

    /// <summary>
    /// VET NAM DEN dan len BE MAT vat the tai cho set danh trung.
    ///
    /// GroundDecal.Spawn khong dung duoc: no dung mot cai DIA om theo mat dat,
    /// nen dan len than cay hay bia mo dung thi vet nam nam ngang lo lung trong
    /// khong. Day la mot mieng vuong xoay theo PHAP TUYEN be mat.
    /// </summary>
    public static void NamDen(Vector3 diem, Vector3 phap, float banKinh)
    {
        var go = new GameObject("VetNam");
        // Nhac ra khoi be mat 2 cm, khong thi hai mat phang trung nhau va vet
        // nam nhap nhay theo goc nhin (z-fighting).
        go.transform.position = diem + phap * 0.02f;
        go.transform.rotation = Quaternion.LookRotation(-phap, Vector3.up);
        go.transform.localScale = Vector3.one * banKinh * 2f;

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = MiengVuongMoi();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(ScorchMat);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        var d = go.AddComponent<GroundDecal>();
        d.lifetime = 7f;
        d.fadeStart = 4f;
    }

    /// <summary>
    /// Quet cac CHUONG NGAI VAT quanh cho set danh, nam den chung va cho boc
    /// mot it khoi ngay tren be mat bi trung.
    ///
    /// Dung RAYCAST tu tam no ra chu khong dung Collider.ClosestPoint: raycast
    /// tra ve ca DIEM lan PHAP TUYEN that cua be mat, con ClosestPoint chi tra
    /// ve diem, va no con nem loi tren MeshCollider khong loi.
    ///
    /// Bo qua TerrainCollider - mat dat da co vet chay rieng roi, dan them mot
    /// mieng vuong dung len giua dat thi nhin ra mot tam bia.
    /// </summary>
    public static int NamChuongNgai(Vector3 tam, float banKinh, LayerMask mask)
    {
        var col = Physics.OverlapSphere(tam, banKinh, mask, QueryTriggerInteraction.Ignore);
        int dem = 0;
        for (int i = 0; i < col.Length && dem < 4; i++)
        {
            if (col[i] is TerrainCollider) continue;

            Vector3 dich = col[i].bounds.center;
            Vector3 huong = dich - tam;
            float xa = huong.magnitude;

            RaycastHit hit;
            bool trung = false;

            if (xa >= 0.05f)
            {
                huong /= xa;
                trung = Physics.Raycast(tam, huong, out hit, xa + 1.2f, mask,
                                        QueryTriggerInteraction.Ignore);
            }
            else hit = default(RaycastHit);

            if (!trung)
            {
                // TAM NO NAM NGAY TRONG VAT - set danh trung chan mot cai lang
                // mo chang han. Raycast tu trong ra khong trung gi ca: Physics
                // bo qua chinh cai collider ma tia bat dau ben trong no. Phai
                // ban NGUOC tu ngoai vao.
                //
                // Da vap: danh vao mot lang mo cao 4,68 m thi tao duoc DUNG 0
                // vet nam, trong khi danh vao mot manh nam sat dat thi ra 2.
                Vector3 ng = Random.insideUnitCircle.normalized;
                Vector3 h2 = new Vector3(ng.x, 0.22f, ng.y).normalized;
                float banKinhVat = col[i].bounds.extents.magnitude + 0.6f;
                Vector3 ngoai = dich + h2 * banKinhVat;
                trung = Physics.Raycast(ngoai, -h2, out hit, banKinhVat * 2f, mask,
                                        QueryTriggerInteraction.Ignore);
            }

            if (!trung) continue;
            if (hit.collider is TerrainCollider) continue;

            NamDen(hit.point, hit.normal, Random.Range(0.34f, 0.62f));

            // Mot nhum khoi nho ngay tren be mat bi nam
            var khoi = NewPS("KhoiNam", null, hit.point + hit.normal * 0.06f,
                             KhoiSetMat, ParticleSystemRenderMode.Billboard);
            AutoDestroy.Add(khoi.gameObject, 2.6f);
            var m = khoi.main;
            m.duration = 0.35f; m.loop = false;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 0.95f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.52f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.gravityModifier = -0.14f;
            m.maxParticles = 14;
            m.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.60f, 0.56f, 0.52f, 1f), new Color(0.86f, 0.82f, 0.78f, 0.9f));
            var e = khoi.emission;
            e.rateOverTime = 0f;
            e.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 6) });
            var sh = khoi.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 0.12f;
            var sz = khoi.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(1f, 1.5f)));
            var co = khoi.colorOverLifetime;
            co.enabled = true;
            co.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.35f));

            dem++;
        }
        return dem;
    }

    /// <summary>Lop cua chuong ngai vat (cay, bia mo, da...). Tinh mot lan.</summary>
    public static LayerMask LopChuongNgai
    {
        get
        {
            if (mLopChuongNgai < 0)
                mLopChuongNgai = LayerMask.GetMask("Default", "Ground");
            return mLopChuongNgai;
        }
    }
    static int mLopChuongNgai = -1;

    public static void LightningImpact(Vector3 pos, float radius)
    {
        var pf = GameAssets.I != null ? GameAssets.I.lightningImpactPrefab : null;
        if (pf != null)
        {
            var go = GameAssets.Make(pf, pos);
            float k = radius / 2.1f;                  // prefab duoc nuong o ban kinh 2.1
            if (Mathf.Abs(k - 1f) > 0.05f) go.transform.localScale = Vector3.one * k;
        }
        else
        {
            BuildLightningImpact(pos, radius);
        }

        // Thinh thoang de lai vet chay xem tren dat
        if (Random.value < 0.15f)
            GroundDecal.Spawn(new Vector3(pos.x, GroundY(pos), pos.z), radius * 0.55f,
                              new Material(ScorchMat), 8f, 4f);
    }

    /// <summary>Hieu ung bam tren nguoi quai dang BI CHOANG (uu tien dung prefab).</summary>
    public static GameObject AttachStunned(Transform target, float height, float radius)
    {
        var pf = GameAssets.I != null ? GameAssets.I.stunnedPrefab : null;
        if (pf != null)
        {
            var inst = Object.Instantiate(pf, target);
            inst.name = "Stunned";
            inst.transform.localPosition = new Vector3(0f, height, 0f);
            return inst;
        }
        return BuildStunned(target, height, radius);
    }

    /// <summary>Dung hinh trang thai choang bang code: vong dom sang xoay tron tren dau.</summary>
    public static GameObject BuildStunned(Transform target, float height, float radius)
    {
        var go = new GameObject("Stunned");
        go.transform.SetParent(target, false);
        go.transform.localPosition = new Vector3(0f, height, 0f);

        // Vong dom sang chay vong quanh dau
        var ps = NewPS("Stars", go.transform, new Vector3(0f, 0.35f, 0f), SparkBlueMat,
                       ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.8f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
        m.simulationSpace = ParticleSystemSimulationSpace.Local;
        m.maxParticles = 90;
        var em = ps.emission; em.rateOverTime = 55f;
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = Mathf.Max(0.25f, radius * 1.5f);
        sh.radiusThickness = 0f;
        sh.arcMode = ParticleSystemShapeMultiModeValue.Loop;
        sh.arcSpeed = 2.2f;
        sh.rotation = new Vector3(90f, 0f, 0f);
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.70f, 0.88f, 1f), 0.5f,
            new Color(0.40f, 0.65f, 1f), 1f,
            0f, 1f, 0.9f, 0f));

        // Anh sang xanh giat nhe theo nhip dien
        var lightGo = new GameObject("StunLight");
        lightGo.transform.SetParent(go.transform, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.60f, 0.82f, 1f);
        lt.intensity = 1.9f;
        lt.range = 4.5f;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 1.9f; fl.amount = 0.6f; fl.speed = 18f;

        return go;
    }

    // ================================================================
    //  LOC XOAY
    // ================================================================

    static Material mTornado, mDebris, mSpiral, mRiseWisp, mDaThienThach;

    /// <summary>
    /// Da thien thach: khoi da toi mau nhung dang nung do tu ben trong.
    ///
    /// Dung Glow chu khong dung Mats.Rock: da xam thuong nam trong bung mot khoi
    /// lua thi den thui, khong nhin ra hinh thu gi. Cho no tu phat sang mau cam
    /// thi moi tach duoc khoi nen lua sang chung quanh.
    /// </summary>
    public static Material DaThienThachMat
    {
        get
        {
            if (mDaThienThach == null)
                // Do phat sang VUA PHAI. De 1,15 thi ca khoi sang deu mot mau
                // cam, mat het cam giac da; 0,6 thi mat huong ve phia lua sang
                // len con mat khuat toi di, nhin ra khoi da dang nung do.
                mDaThienThach = Mats.Glow("DaThienThach",
                                          new Color(0.16f, 0.075f, 0.05f),
                                          new Color(0.95f, 0.34f, 0.06f), 0.60f);
            return mDaThienThach;
        }
    }

    /// <summary>
    /// Manh dat da bi loc hut len.
    ///
    /// Dung LUOI 16 KHOI DA dung san trong Blender (bao loi cua chum diem ngau
    /// nhien, co chieu sang nen moi mat mot do sang khac nhau). Truoc day dung
    /// TextureFactory.IceShard - mot hinh tam giac nhon duy nhat, nen ca dam
    /// manh bi loc cuon len deu la nhung tam giac giong het nhau.
    ///
    /// Chua co file thi quay ve anh manh bang cu.
    /// </summary>
    public static Material DebrisMat
    {
        get
        {
            if (mDebris == null)
            {
                var tex = NapFlipbook("ManhDatDa");
                mDebris = Mats.Alpha("P_Debris",
                                     tex != null ? tex : TextureFactory.IceShard(),
                                     new Color(1f, 0.96f, 0.90f, 1f));
            }
            return mDebris;
        }
    }

    public static Material TornadoMat
    {
        get
        {
            if (mTornado == null)
            {
                mTornado = Mats.Alpha("P_Tornado", TextureFactory.TornadoWall(), Color.white);
                mTornado.mainTextureScale = new Vector2(3f, 1f);
            }
            return mTornado;
        }
    }

    /// <summary>Duong vien cua than loc: thit lai o chan, phinh dan va loe rong o dinh.</summary>
    /// <summary>
    /// HE SO NO NGANG cua con loc.
    ///
    /// Nhan RIENG vao ban kinh, KHONG nhan vao chieu cao - de "phinh ra" chu
    /// khong phai "to lên toàn bộ". Nhan vao scale thi con loc cao them 20%
    /// nua, va no dam thung dam may o dinh.
    ///
    /// Dung chung cho: vo phieu, dai xoan, bui o chan, tia vut len, dam manh
    /// vun, may dinh - va cho ca <see cref="Tornado.FunnelRadiusAt"/> voi
    /// <c>catchRadius</c>. Bo sot mot cho thi phan do teo lai giua than loc.
    /// </summary>
    public const float LocNoNgang = 1.20f;

    static Vector2[] FunnelProfile(float scale)
    {
        float r = scale * LocNoNgang;
        return new Vector2[]
        {
            new Vector2(0.85f * r,  0.0f),
            new Vector2(0.95f * r,  1.2f * scale),
            new Vector2(1.15f * r,  3.0f * scale),
            new Vector2(1.55f * r,  5.2f * scale),
            new Vector2(2.20f * r,  7.4f * scale),
            new Vector2(3.10f * r,  9.4f * scale),
            new Vector2(4.40f * r, 11.2f * scale),
            new Vector2(6.20f * r, 12.6f * scale),
        };
    }

    /// <summary>
    /// DUNG HINH CON LOC: ba lop vo hinh phieu chong len nhau, quay nhanh cham khac nhau
    /// va hoa tiet truot lien tuc -> nhin nhu khoi bui dang cuon tron.
    /// Kem bui dat cuon o chan, manh vun bay quanh va may den o dinh.
    /// </summary>
    public static GameObject BuildTornado(float scale)
    {
        var root = new GameObject("Tornado");

        // ---- Ba lop vo cuon ----
        // Mau XAM TRANG (truoc day nau dat). Toc do truot doc (so thu hai) da tang
        // manh: day chinh la thu lam hoa tiet chay tu duoi len, thay ro dang cuon.
        BuildFunnelShell(root.transform, scale * 0.82f, new Color(0.80f, 0.82f, 0.86f, 0.95f),
                         320f, new Vector2(1.5f, -2.10f), 4f);
        BuildFunnelShell(root.transform, scale * 1.00f, new Color(0.90f, 0.92f, 0.95f, 0.72f),
                         -210f, new Vector2(-1.1f, -1.55f), 3f);
        BuildFunnelShell(root.transform, scale * 1.22f, new Color(0.96f, 0.97f, 1f, 0.40f),
                         140f, new Vector2(0.7f, -1.10f), 2.2f);

        // ---- Nhung dai bui quan xoan quanh than, boc tu chan len ngon ----
        BuildSpiralRibbons(root.transform, scale);

        // ---- Bui dat cuon tung mu o chan loc ----
        var dust = NewPS("Dust", root.transform, new Vector3(0f, 0.15f, 0f), SmokeMat,
                         ParticleSystemRenderMode.Billboard);
        var dm = dust.main;
        dm.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.8f);
        dm.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        dm.startSize = new ParticleSystem.MinMaxCurve(1.2f * scale, 3.2f * scale);
        dm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        dm.simulationSpace = ParticleSystemSimulationSpace.World;
        dm.maxParticles = 220;
        dm.gravityModifier = -0.08f;
        var dem = dust.emission; dem.rateOverTime = 110f;
        var dsh = dust.shape;
        dsh.shapeType = ParticleSystemShapeType.Circle;
        dsh.radius = 1.6f * scale * LocNoNgang;
        var dvel = dust.velocityOverLifetime;
        dvel.enabled = true;
        dvel.space = ParticleSystemSimulationSpace.Local;
        dvel.orbitalY = new ParticleSystem.MinMaxCurve(5.5f, 9f);      // cuon quanh truc
        dvel.radial = new ParticleSystem.MinMaxCurve(-0.6f, 0.3f);
        // Xam trang. Rieng bui o chan KHONG con nhuom theo mau dat nua - no la mot
        // phan cua "than loc" ma than loc phai la xam trang. Manh vun van doi mau
        // theo dat (xem BuildDebrisSwarm) nen van thay loc boc dat cho nao len cho do.
        var dmain = dust.main;
        dmain.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.88f, 0.90f, 0.94f), new Color(1f, 1f, 1f));
        var dcol = dust.colorOverLifetime; dcol.enabled = true;
        dcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.86f, 0.88f, 0.92f), 0.45f,
            new Color(0.62f, 0.65f, 0.72f), 1f,
            0f, 0.80f, 0.50f, 0f));

        // ---- Nhung vet bui trang vut TU CHAN LEN NGON, bam sat vo loc ----
        // Bay theo hinh loxo: vua quay quanh truc rat nhanh vua leo len deu,
        // nen mat nguoi choi bat duoc huong "di len" chu khong chi thay quay tron.
        var rise = NewPS("Vut", root.transform, new Vector3(0f, 0.2f, 0f), RiseWispMat,
                         ParticleSystemRenderMode.Stretch);
        var rm = rise.main;
        rm.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.4f);
        rm.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        rm.startSize = new ParticleSystem.MinMaxCurve(0.30f * scale, 0.80f * scale);
        rm.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.86f, 0.89f, 0.95f), new Color(1f, 1f, 1f));
        rm.simulationSpace = ParticleSystemSimulationSpace.Local;
        rm.maxParticles = 190;
        var rem = rise.emission; rem.rateOverTime = 85f;
        var rsh = rise.shape;
        rsh.shapeType = ParticleSystemShapeType.Circle;
        rsh.radius = 1.0f * scale * LocNoNgang;
        rsh.radiusThickness = 0.35f;                   // rai quanh vanh, khong dung mot vong

        // TIA PHAI BAM THEO THAN LOC, khong xoe ngang.
        //
        // Che do Stretch keo moi hat dai ra theo VECTO VAN TOC TONG. Ban truoc
        // van toc tiep tuyen (orbital 9-14, tuc 9-14 m/s o ban kinh 1 m) lon hon
        // van toc leo len (5,5-8,5) va con cong them radial 1-2 loe ra, nen o
        // chan loc huong van toc gan nhu NAM NGANG: ca chum tia xoe ra nhu nan
        // quat, nhin rat ky. Truoc day loi nay bi dam manh vo to che mat, doi
        // sang manh nho thi lo ra.
        //
        // Chua bang cach dao lai ti le: leo len MANH hon tiep tuyen, va gan nhu
        // bo han thanh phan loe ngang.
        var rvel = rise.velocityOverLifetime;
        rvel.enabled = true;
        rvel.space = ParticleSystemSimulationSpace.Local;
        rvel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        rvel.orbitalY = new ParticleSystem.MinMaxCurve(5.5f, 8f);   // quay quanh truc
        rvel.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
        rvel.radial = new ParticleSystem.MinMaxCurve(0.15f, 0.5f);  // loe rat nhe
        rvel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        rvel.y = new ParticleSystem.MinMaxCurve(8f, 12f);           // LEO LEN - manh nhat
        rvel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var rcol = rise.colorOverLifetime; rcol.enabled = true;
        rcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.95f, 0.96f, 1f), 0.5f,
            new Color(0.72f, 0.76f, 0.86f), 1f,
            0f, 0.9f, 0.75f, 0f));

        // Keo NGAN lai: tia dai 2,4 lan cong them 0,16 theo van toc thi moi tia
        // thanh mot vach dai ngoang, cang lam ro cai xoe ngang.
        var rren = rise.GetComponent<ParticleSystemRenderer>();
        if (rren != null) { rren.lengthScale = 1.5f; rren.velocityScale = 0.06f; }

        // ---- Hat cat li ti bi hut bay vong quanh ----
        // TRUOC DAY con mot bay "Debris" co 0,22-0,95 nua - nhung tang dat to
        // bang ca cai bia mo bay lo lung quanh than loc, nhin ra manh giay den
        // cat roi chu khong ra dat da. Da bo han; gio chi con hat li ti, con
        // phan "loc boc thu gi len" duoc the hien bang khoi bui phia sau.
        BuildDebrisSwarm(root.transform, scale, "Grit", 0.05f, 0.20f, 160f, 340,
                         2.4f * LocNoNgang, 6.5f * LocNoNgang);

        // ---- Khoi bui den keo dai phia sau duong loc di ----
        BuildKhoiBuiLoc(root.transform, scale);

        // ---- May den un un o dinh loc ----
        var cloud = NewPS("Cloud", root.transform, new Vector3(0f, 12.4f * scale, 0f), StormCloudMat,
                          ParticleSystemRenderMode.Billboard);
        var cm = cloud.main;
        cm.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.6f);
        cm.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.4f);
        cm.startSize = new ParticleSystem.MinMaxCurve(5f * scale, 11f * scale);
        cm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        cm.simulationSpace = ParticleSystemSimulationSpace.Local;
        cm.maxParticles = 70;
        var cem = cloud.emission; cem.rateOverTime = 14f;
        var csh = cloud.shape; csh.shapeType = ParticleSystemShapeType.Circle;
        csh.radius = 5f * scale * LocNoNgang;
        var ccol = cloud.colorOverLifetime; ccol.enabled = true;
        ccol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(0.30f, 0.30f, 0.34f), 0f,
            new Color(0.16f, 0.16f, 0.20f), 0.45f,
            new Color(0.07f, 0.07f, 0.10f), 1f,
            0f, 0.85f, 0.6f, 0f));
        var crot = cloud.rotationOverLifetime; crot.enabled = true;
        crot.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        // ---- Anh chop xanh hat ra tu trong long loc ----
        var lightGo = new GameObject("StormLight");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 5f * scale, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.66f, 0.80f, 1f);
        lt.intensity = 2.6f;
        lt.range = 16f * scale;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 2.6f; fl.amount = 0.6f; fl.speed = 14f; fl.rangeWobble = 0.12f;

        return root;
    }

    /// <summary>
    /// KHOI BUI DEN KEO DAI PHIA SAU CON LOC.
    ///
    /// Phun o KHONG GIAN THE GIOI, va hat gan nhu dung yen tai cho sau khi sinh
    /// ra. Con loc chay toi truoc, dam khoi o lai dang sau - thanh mot vet dai
    /// danh dau duong no vua di qua. Phun theo vat the thi ca dam khoi se di
    /// theo loc va khong bao gio thay duoc cai vet do.
    ///
    /// Song lau (2,5-5 giay) va NO RONG DAN ra theo doi, nen vet khoi cang ve
    /// sau cang bung to va tan loang - dung kieu bui dat lang xuong.
    ///
    /// LUONG KHOI DO <see cref="Tornado"/> chinh, tang dan theo thoi gian loc
    /// ton tai. O day chi dat muc khoi dau.
    /// </summary>
    public static ParticleSystem BuildKhoiBuiLoc(Transform parent, float scale)
    {
        // Uu tien FLIPBOOK: dam khoi cuon xoay ngay ben trong tung hat, thay
        // vi mot cham mo dung im chi biet to len roi nhat di. Chua co file
        // flipbook thi quay ve SmokeMat sinh bang code.
        var matKhoi = KhoiCuonMat;
        var ps = NewPS("KhoiBui", parent, new Vector3(0f, 0.25f, 0f),
                       matKhoi != null ? matKhoi : SmokeMat,
                       ParticleSystemRenderMode.Billboard);
        if (matKhoi != null) BatFlipbook(ps, 6, 6, 1);

        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 5.0f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.9f);
        m.startSize = new ParticleSystem.MinMaxCurve(1.4f * scale, 3.4f * scale);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 700;
        m.gravityModifier = -0.02f;               // bo len rat cham roi lang lai

        // Muc khoi dau. Tornado se tang dan len tu day.
        var em = ps.emission; em.rateOverTime = 30f;

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 1.9f * scale;
        sh.radiusThickness = 1f;

        // DEN KHOI, khong nhuom theo mau dat - day la khoi chu khong phai dat cuon
        m.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.10f, 0.09f, 0.09f), new Color(0.22f, 0.21f, 0.20f));

        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 1f, 1f), 0f,
            new Color(0.85f, 0.85f, 0.85f), 0.4f,
            new Color(0.6f, 0.6f, 0.6f), 1f,
            0f, 0.80f, 0.55f, 0f));

        // No rong dan ra - vet khoi cang ve sau cang bung to
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0.45f),
                               new Keyframe(0.5f, 1.15f),
                               new Keyframe(1f, 1.9f)));

        var rot = ps.rotationOverLifetime; rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-0.7f, 0.7f);

        // Quan nhe quanh truc luc moi sinh ra, roi thoi
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        // Ca BA truc orbital phai cung mot kieu duong cong, khong thi Unity
        // keu "Particle Orbital Velocity curves must all be in the same mode"
        // moi khung hinh. Chi dat orbitalY (hai hang so) ma de X va Z o mac
        // dinh (mot hang so) la du lam no keu.
        vel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.orbitalY = new ParticleSystem.MinMaxCurve(2.2f, 4.0f);
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.radial = new ParticleSystem.MinMaxCurve(0.1f, 0.7f);

        return ps;
    }

    /// <summary>
    /// Mot bay manh vun bi hut quanh than loc.
    /// Goi hai lan voi hai khoang kich thuoc khac nhau se ra dam manh to nho lan lon.
    /// </summary>
    static void BuildDebrisSwarm(Transform parent, float scale, string name,
                                 float sizeMin, float sizeMax, float rate, int maxParticles,
                                 float riseMin, float riseMax)
    {
        // Billboard chu khong Stretch: Stretch keo manh dai theo huong bay
        // thanh vet thuon, con manh dat bi loc cuon thi XOAY LON.
        var ps = NewPS(name, parent, new Vector3(0f, 0.35f, 0f), DebrisMat,
                       ParticleSystemRenderMode.Billboard);

        // Moi hat lay MOT O co dinh trong luoi 4x4 -> 16 hinh manh khac nhau
        if (NapFlipbook("ManhDatDa") != null)
        {
            var tsa = ps.textureSheetAnimation;
            tsa.enabled = true;
            tsa.mode = ParticleSystemAnimationMode.Grid;
            tsa.numTilesX = 4;
            tsa.numTilesY = 4;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
            tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 16f);
        }

        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 2f);
        m.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        // Anh manh da DA CO mau dat va co khoi san, nen tint gan trang - to dam
        // them nua la mat het chuyen sang giua cac mat
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.82f, 0.78f, 0.70f),
                                                         new Color(1f, 0.97f, 0.92f));
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = maxParticles;
        m.gravityModifier = -0.35f;                       // bi hut nguoc len tren

        var em = ps.emission; em.rateOverTime = rate;

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 2.2f * scale;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.orbitalY = new ParticleSystem.MinMaxCurve(7f, 12f);
        vel.y = new ParticleSystem.MinMaxCurve(riseMin, riseMax);
        vel.radial = new ParticleSystem.MinMaxCurve(-1.2f, -0.2f);

        // Manh to xoay lat nhieu hon manh nho
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
    }

    /// <summary>Vat lieu cua nhung dai bui quan xoan quanh than loc.</summary>
    public static Material SpiralMat
    {
        get
        {
            if (mSpiral == null)
                mSpiral = Mats.Alpha("P_XoanLoc", TextureFactory.SpiralStreak(),
                                     new Color(1f, 1f, 1f, 0.55f));
            return mSpiral;
        }
    }

    /// <summary>
    /// Vat lieu cho nhung vet bui vut len doc than loc.
    ///
    /// CO Y dung anh cham tron mo dan (SoftDot) chu KHONG dung SpiralStreak:
    /// SpiralStreak lam lien mach doc chieu dai de quan quanh than loc cho khong
    /// bi dut, nen hai dau no khong mo di. Dem lam hat billboard keo dai thi hai
    /// dau do hien nguyen thanh canh vuong sac le ra giua troi.
    /// </summary>
    public static Material RiseWispMat
    {
        get
        {
            if (mRiseWisp == null)
                mRiseWisp = Mats.Alpha("P_VutLoc", TextureFactory.SoftDot(1.5f),
                                       new Color(1f, 1f, 1f, 0.5f));
            return mRiseWisp;
        }
    }

    /// <summary>Ban kinh va do cao cua than loc tai vi tri <paramref name="t"/> (0 = chan, 1 = ngon).</summary>
    static void FunnelPointAt(float t, float scale, out float radius, out float height)
    {
        var p = FunnelProfile(scale);
        float fi = Mathf.Clamp01(t) * (p.Length - 1);
        int i0 = Mathf.FloorToInt(fi);
        int i1 = Mathf.Min(i0 + 1, p.Length - 1);
        float k = fi - i0;
        radius = Mathf.Lerp(p[i0].x, p[i1].x, k);
        height = Mathf.Lerp(p[i0].y, p[i1].y, k);
    }

    /// <summary>
    /// NHUNG DAI BUI QUAN XOAN quanh than loc, boc tu chan len ngon.
    ///
    /// Day la thu lam nguoi choi nhin phat la thay "loc dang cuon tu duoi len".
    /// Meo o cho chieu quan: goc quay GIAM dan khi len cao (dai thuan tay trai),
    /// nen khi ca khoi quay theo chieu duong thi mat nhin thay van xoan chay
    /// nguoc len tren - dung nhu con oc van vao go.
    /// Quay nguoc chieu hoac quan nguoc tay thi se thanh chay tut xuong.
    /// </summary>
    static void BuildSpiralRibbons(Transform parent, float scale)
    {
        const int strands = 3;      // so dai
        const int steps = 72;       // do muot cua duong xoan
        const float turns = 2.35f;  // so vong quan tu chan len ngon

        for (int s = 0; s < strands; s++)
        {
            float phase = s / (float)strands * Mathf.PI * 2f;
            float halfWidth = Mathf.Lerp(0.34f, 0.20f, s / (float)Mathf.Max(1, strands - 1)) * scale;

            var b = new ProcMesh.Builder();

            // Moi dai gom HAI mieng bat cheo nhau (mot dung, mot nam ngang).
            // Neu chi mot mieng phang thi den cho duong xoan vong ra hai ben,
            // mieng do quay dung CANH ve phia may quay va xep thanh mot vach
            // cung le ra giua troi. Co mieng thu hai vuong goc thi luon con
            // it nhat mot mieng huong mat ve phia nguoi nhin.
            for (int lop = 0; lop < 2; lop++)
            {
                int goc = b.VertexCount;

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    float r, y;
                    FunnelPointAt(t, scale, out r, out y);
                    r *= 1.04f;                                // nhoi ra ngoai cho khoi lut vao vo

                    float a = phase - t * turns * Mathf.PI * 2f;   // GIAM dan -> nhin thay chay len
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    Vector3 c = dir * r + Vector3.up * y;

                    // Mo muot han tren 18% dau va 22% cuoi, khong de dut phut
                    float k = Mathf.Clamp01(t / 0.18f) * Mathf.Clamp01((1f - t) / 0.22f);
                    float nhon = k * k * (3f - 2f * k);        // duong muot: 0 -> 1 -> 0

                    float w = halfWidth * Mathf.Lerp(0.8f, 2.6f, t) * nhon;
                    var mau = new Color(1f, 1f, 1f, nhon * (lop == 0 ? 1f : 0.85f));

                    // lop 0: be rong theo chieu DUNG. lop 1: be rong theo chieu NGANG
                    Vector3 rong = lop == 0 ? Vector3.up : dir;
                    Vector3 phap = lop == 0 ? dir : Vector3.up;

                    b.AddVertex(c + rong * w, phap, new Vector2(t * 3f, 0f), mau);
                    b.AddVertex(c - rong * w, phap, new Vector2(t * 3f, 1f), mau);
                }

                for (int i = 0; i < steps; i++)
                {
                    int a0 = goc + i * 2, a1 = goc + i * 2 + 1;
                    int b0 = goc + i * 2 + 2, b1 = goc + i * 2 + 3;
                    b.AddQuad(a0, b0, b1, a1);   // mat ngoai
                    b.AddQuad(a1, b1, b0, a0);   // mat trong
                }
            }

            var mesh = b.Build("spiral" + s);

            var mat = new Material(SpiralMat);
            mat.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.62f - s * 0.13f));

            var go = ProcMesh.Part("DaiXoan" + s, parent, mesh, mat, Vector3.zero,
                                   Quaternion.identity, Vector3.one, false);

            var spin = go.AddComponent<Spin>();
            spin.axis = Vector3.up;
            spin.degreesPerSecond = 230f + s * 55f;    // DUONG: hop voi chieu quan de thay chay len

            var scroll = go.AddComponent<ScrollUV>();
            scroll.speed = new Vector2(-1.4f - s * 0.4f, 0f);
        }
    }

    /// <summary>Mot lop vo cua than loc.</summary>
    static void BuildFunnelShell(Transform parent, float scale, Color tint,
                                 float spinSpeed, Vector2 scrollSpeed, float uTiling)
    {
        var mesh = ProcMesh.Revolve(FunnelProfile(scale), 28, false, false, Color.white, "funnel");

        // Dai mau tu duoi len va do mo o hai dau duoc ghi vao MAU DINH.
        // Neu de trong anh thi khi anh cuon doc, ca dai mo se cuon theo va hien
        // ra thanh mot vach cat ngang chay giua than loc.
        var v = mesh.vertices;
        var cols = new Color[v.Length];
        float cao = 0f;
        for (int i = 0; i < v.Length; i++) if (v[i].y > cao) cao = v[i].y;

        for (int i = 0; i < v.Length; i++)
        {
            float t = cao > 0.001f ? Mathf.Clamp01(v[i].y / cao) : 0f;

            // Duoi chan trang duc, len cao nga xam xanh nhu chan may giong
            Color c = Color.Lerp(new Color(0.98f, 0.99f, 1f),
                                 new Color(0.60f, 0.64f, 0.74f), Mathf.Pow(t, 0.85f));

            // Mo dan o sat dat va o mieng loe tren cung
            float mo = Mathf.Clamp01(t * 9f) * Mathf.Clamp01((1f - t) * 3.2f);
            c.a = mo;
            cols[i] = c;
        }
        mesh.colors = cols;

        var mat = new Material(TornadoMat);
        mat.SetColor("_TintColor", tint);
        mat.mainTextureScale = new Vector2(uTiling, 1f);

        var go = ProcMesh.Part("Shell", parent, mesh, mat, Vector3.zero,
                               Quaternion.identity, Vector3.one, false);

        var spin = go.AddComponent<Spin>();
        spin.axis = Vector3.up;
        spin.degreesPerSecond = spinSpeed;

        var scroll = go.AddComponent<ScrollUV>();
        scroll.speed = scrollSpeed;
    }

    /// <summary>
    /// Tia set lach tach trong than loc. Moi nhip phat HAI tia khac kieu nhau.
    /// </summary>
    public static void TornadoBolt(Vector3 basePos, float scale)
    {
        var prof = FunnelProfile(scale);

        // ---- TIA 1: bam tren VO loc ----
        // Noi hai do cao khac nhau, hai diem deu nam sat mat ngoai than loc.
        int i = Random.Range(0, prof.Length - 2);
        int k = Random.Range(i + 1, prof.Length);

        float a1 = Random.Range(0f, Mathf.PI * 2f);
        float a2 = a1 + Random.Range(-1.4f, 1.4f);

        Vector3 p1 = basePos + new Vector3(Mathf.Cos(a1) * prof[i].x, prof[i].y, Mathf.Sin(a1) * prof[i].x);
        Vector3 p2 = basePos + new Vector3(Mathf.Cos(a2) * prof[k].x, prof[k].y, Mathf.Sin(a2) * prof[k].x);

        var arc = LightningArc.Create(p2, p1, 0.85f, Random.Range(0.16f, 0.28f));
        arc.segments = 16;
        arc.jitter = 1.5f;
        arc.branches = Random.Range(1, 3);

        // ---- TIA 2: xuyen DOC TRONG LONG loc ----
        // Khac han tia thu nhat: tia kia bam ngoai vo, con tia nay di tu thap
        // len cao ngay GIUA than loc, nen nhin vao thay set nhay BEN TRONG con
        // loc chu khong chi lap loe o ria.
        //
        // Lay mot diem o phan ba duoi va mot diem o phan ba tren de tia du dai;
        // chon ngau nhien trong ca than thi nhieu lan hai diem sat nhau, ra mot
        // doan cut ngan.
        int lo = Random.Range(0, Mathf.Max(1, prof.Length / 3));
        int hi = Random.Range(prof.Length * 2 / 3, prof.Length);

        float g1 = Random.Range(0f, Mathf.PI * 2f);
        // Len cao thi lech goc di - tia nghieng theo chieu loc dang xoay
        float g2 = g1 + Random.Range(1.0f, 2.2f);

        // He so 0,35: keo hai dau vao GAN TRUC, khong de chung bam vo
        const float TrongLong = 0.35f;
        Vector3 q1 = basePos + new Vector3(Mathf.Cos(g1) * prof[lo].x * TrongLong,
                                           prof[lo].y,
                                           Mathf.Sin(g1) * prof[lo].x * TrongLong);
        Vector3 q2 = basePos + new Vector3(Mathf.Cos(g2) * prof[hi].x * TrongLong,
                                           prof[hi].y,
                                           Mathf.Sin(g2) * prof[hi].x * TrongLong);

        var arc2 = LightningArc.Create(q1, q2, 0.70f, Random.Range(0.13f, 0.22f));
        arc2.segments = 22;                      // tia dai hon nen can nhieu doan hon
        arc2.jitter = 1.05f;                     // giat NHE thoi: tia dai ma giat manh
                                                 // thi tan thanh mot dam bui sang
        arc2.branches = Random.Range(2, 4);
    }

    // ================================================================
    //  NIEM CHU (tich tu phep o dau gay)
    // ================================================================

    public static GameObject CastCharge(Transform at, bool fire, float duration)
    {
        return CastCharge(at, fire ? 0 : 1, duration);
    }

    /// <summary>Phep dang tich tu o dau gay. Nguyen to: 0 = lua, 1 = bang, 2 = set.</summary>
    public static GameObject CastCharge(Transform at, int element, float duration)
    {
        var pf = GameAssets.I != null
            ? (element == 0 ? GameAssets.I.castFirePrefab
             : element == 1 ? GameAssets.I.castIcePrefab
                            : GameAssets.I.castLightningPrefab)   // set va gio dung chung
            : null;
        if (pf != null)
        {
            var inst = Object.Instantiate(pf, at);
            inst.name = pf.name;
            inst.transform.localPosition = Vector3.zero;
            var ad = inst.GetComponent<AutoDestroy>();
            if (ad == null) ad = inst.AddComponent<AutoDestroy>();
            ad.lifetime = duration + 0.4f;
            return inst;
        }
        return BuildCastCharge(at, element, duration);
    }

    public static GameObject BuildCastCharge(Transform at, bool fire, float duration)
    {
        return BuildCastCharge(at, fire ? 0 : 1, duration);
    }

    /// <summary>Dung hinh phep dang tich tu bang code (0 = lua, 1 = bang, 2 = set).</summary>
    public static GameObject BuildCastCharge(Transform at, int element, float duration)
    {
        bool fire = element == 0;
        bool bolt = element >= 2;

        var go = new GameObject(fire ? "ChargeFire" : bolt ? "ChargeBolt" : "ChargeIce");
        go.transform.SetParent(at, false);
        AutoDestroy.Add(go, duration + 0.4f);

        var ps = NewPS("Charge", go.transform, Vector3.zero,
                       fire ? SparkMat : bolt ? SparkBlueMat : SnowMat,
                       ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(-2.2f, -1.2f);   // am = hut vao trong
        m.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
        m.simulationSpace = ParticleSystemSimulationSpace.Local;
        m.maxParticles = 150;
        var em = ps.emission; em.rateOverTime = bolt ? 160f : 120f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.6f;
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = fire
            ? new ParticleSystem.MinMaxGradient(Grad(new Color(1f, 0.9f, 0.6f), 0f, new Color(1f, 0.5f, 0.12f), 0.5f, new Color(1f, 0.3f, 0.05f), 1f, 0f, 1f, 1f, 0.2f))
            : bolt
            ? new ParticleSystem.MinMaxGradient(Grad(new Color(1f, 1f, 1f), 0f, new Color(0.70f, 0.88f, 1f), 0.5f, new Color(0.38f, 0.62f, 1f), 1f, 0f, 1f, 1f, 0.2f))
            : new ParticleSystem.MinMaxGradient(Grad(new Color(0.9f, 0.97f, 1f), 0f, new Color(0.6f, 0.85f, 1f), 0.5f, new Color(0.4f, 0.7f, 1f), 1f, 0f, 1f, 1f, 0.2f));

        var glow = ProcMesh.Part("Glow", go.transform, ProcMesh.Sphere(0.10f, 12, 8, 1f, Color.white),
                                 fire ? new Material(Mats.Fire) : new Material(Mats.Ice),
                                 Vector3.zero, Quaternion.identity, Vector3.one, false);
        var ef = glow.AddComponent<ExpandFade>();
        ef.duration = duration;
        ef.startScale = Vector3.one * 0.2f;
        ef.endScale = Vector3.one * 1.6f;
        ef.ease = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        // Phep set con co vai tia dien nho lach tach quanh dau gay
        if (bolt)
        {
            var crackle = go.AddComponent<CastCrackle>();
            crackle.radius = 0.55f;
        }

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(go.transform, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = fire ? new Color(1f, 0.55f, 0.2f)
                 : bolt ? new Color(0.70f, 0.86f, 1f)
                        : new Color(0.5f, 0.8f, 1f);
        lt.intensity = 4f; lt.range = 6f; lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 4f; fl.amount = bolt ? 0.55f : 0.3f; fl.speed = bolt ? 18f : 10f;

        return go;
    }

    /// <summary>Tia mau / bui khi bi danh trung don thuong.</summary>
    public static void HitBurst(Vector3 pos, Color color)
    {
        var pf = GameAssets.I != null ? GameAssets.I.hitBurstPrefab : null;
        if (pf != null)
        {
            var inst = GameAssets.Make(pf, pos);
            var ps = inst.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var mm = ps.main;
                mm.startColor = new ParticleSystem.MinMaxGradient(color);
            }
            return;
        }
        BuildHitBurst(pos, color);
    }

    /// <summary>Dung hinh tia mau bang code.</summary>
    public static GameObject BuildHitBurst(Vector3 pos, Color color)
    {
        var go = new GameObject("Hit");
        go.transform.position = pos;
        AutoDestroy.Add(go, 1.5f);

        var ps = NewPS("Spark", go.transform, Vector3.zero, GlowMat, ParticleSystemRenderMode.Stretch);
        var m = ps.main;
        m.duration = 0.2f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        m.startColor = new ParticleSystem.MinMaxGradient(color);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 1.4f;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 14) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.12f;
        return go;
    }

    // ================================================================
    //  TIEN ICH
    // ================================================================

    // ================================================================
    //  HIEU UNG LOI NUOC
    // ================================================================

    static Material mGonNuoc, mBotNuoc;

    /// <summary>Vong gon lan ra tren mat nuoc.</summary>
    public static Material GonNuocMat
    {
        get
        {
            if (mGonNuoc == null)
                mGonNuoc = Mats.Alpha("P_GonNuoc", TextureFactory.Ring(0.13f),
                                      new Color(0.82f, 0.88f, 0.86f, 0.55f));
            return mGonNuoc;
        }
    }

    /// <summary>Bot trang li ti.</summary>
    public static Material BotNuocMat
    {
        get
        {
            if (mBotNuoc == null)
                mBotNuoc = Mats.Alpha("P_BotNuoc", TextureFactory.SoftDot(1.7f),
                                      new Color(0.90f, 0.94f, 0.92f, 0.85f));
            return mBotNuoc;
        }
    }

    /// <summary>
    /// Vong gon lan ra tu cho chan khuay nuoc.
    ///
    /// Phun o KHONG GIAN THE GIOI: phun theo vat the thi vong gon di theo chan
    /// nguoi choi, ma gon song that phai dung yen tai cho roi loang ra.
    ///
    /// Vong nam NGANG (xoay 90 do quanh truc X) chu khong quay ve phia may quay
    /// nhu hat thuong - no la vet tren mat nuoc, khong phai dom sang lo lung.
    /// </summary>
    public static ParticleSystem GonNuoc(Transform parent)
    {
        var ps = NewPS("GonNuoc", parent, Vector3.zero, GonNuocMat,
                       ParticleSystemRenderMode.Billboard);
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.alignment = ParticleSystemRenderSpace.World;

        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 1.15f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
        m.startRotation3D = true;
        m.startRotationX = new ParticleSystem.MinMaxCurve(Mathf.PI * 0.5f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 0f;
        m.maxParticles = 40;
        m.playOnAwake = false;

        var em = ps.emission; em.rateOverTime = 0f;    // chi Emit tay
        var sh = ps.shape; sh.enabled = false;

        // Loang rong dan ra roi mo di - dung nhu vong song tren mat nuoc
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, 2.6f)));

        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.35f, Color.white, 1f,
            0f, 0.85f, 0.35f, 0f));

        ps.Play();
        return ps;
    }

    /// <summary>Bot trang li ti bam quanh cho chan khuay nuoc.</summary>
    public static ParticleSystem BotNuoc(Transform parent)
    {
        var ps = NewPS("BotNuoc", parent, Vector3.zero, BotNuocMat,
                       ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.13f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 0.10f;
        m.maxParticles = 120;
        m.playOnAwake = false;

        var em = ps.emission; em.rateOverTime = 0f;    // LoiNuoc tu chinh
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 0.34f;
        sh.radiusThickness = 1f;

        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.4f, Color.white, 1f,
            0.9f, 0.8f, 0.35f, 0f));

        return ps;
    }

    /// <summary>Nuoc toe len mot cai luc vua buoc xuong.</summary>
    public static ParticleSystem ToeNuoc(Transform parent)
    {
        var ps = NewPS("ToeNuoc", parent, Vector3.zero, BotNuocMat,
                       ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.30f, 0.62f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(1.1f, 2.8f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.17f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 1.5f;                       // toe len roi roi xuong
        m.maxParticles = 60;
        m.playOnAwake = false;

        var em = ps.emission; em.rateOverTime = 0f;     // chi Emit tay
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 42f;
        sh.radius = 0.22f;
        sh.rotation = new Vector3(-90f, 0f, 0f);        // huong len troi

        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.4f, Color.white, 1f,
            1f, 0.9f, 0.45f, 0f));

        ps.Play();
        return ps;
    }

    // ================================================================
    //  THIEN THACH
    // ================================================================

    /// <summary>
    /// KHOI DA RUC LUA dang lao tu tren troi xuong.
    ///
    /// Khac qua cau lua o cho: qua cau lua la mot khoi lua tron, con thien thach
    /// phai co LOI DA DAC nhin ro duoc, boc trong lua, va keo mot DUOI LUA DAI
    /// phia sau. Cai duoi moi la thu lam nguoi choi thay no dang lao rat nhanh.
    ///
    /// Duoi lua duoc phun o KHONG GIAN THE GIOI: phun theo vat the thi ca dam
    /// lua di theo khoi da va khong bao gio thanh cai duoi.
    /// </summary>
    public static void BuildThienThachVisual(Transform parent, float radius)
    {
        // 1) LOI DA - phai NHIN RO DUOC.
        //
        // Dung ProcMesh.Rock (mat xu xi, meo mo) chu khong dung hinh cau tron:
        // cau tron nam trong bung lua thi chi con la mot vet sang tron, khong ai
        // nhan ra do la tang da.
        //
        // To gan bang ban kinh danh nghia, va lop lua ben ngoai duoc keo mong lai
        // de khoi da con tho ra ma nhin thay.
        var core = ProcMesh.Part("LoiDa", parent,
                                 ProcMesh.Rock(radius * 0.95f, 7, 0.26f, 16, Color.white),
                                 DaThienThachMat, Vector3.zero, Quaternion.identity,
                                 Vector3.one, false);
        var spin = core.AddComponent<Spin>();
        spin.axis = new Vector3(0.4f, 1f, 0.25f).normalized;
        spin.degreesPerSecond = 95f;

        // 2) Vo lua boc quanh loi da
        // 2) LUA LIEM QUANH VIEN - bang HAT, khong bang vo cau dac.
        //
        // Truoc day toi boc khoi da bang hai lop vo hinh cau dung vat lieu lua
        // phat sang. Vo cau BAO KIN khoi da, hai lop cong don lai thanh mot cuc
        // trang xoa - nhin khong ra tang da dau ca, do lai duoc 19.614 diem chay
        // trang tren anh.
        //
        // Hat thi khac: chung roi rac, luon co khe ho giua cac hat, nen mat da
        // van lo ra. Phun tu mot VO CAU MONG (radiusThickness nho) ngay sat ria
        // khoi da, nen lua om lay vien chu khong lap day ben trong.
        var matLuaTT = LuaCuonMat;
        float bLua = matLuaTT != null ? BuLua : 1f;

        var vien = NewPS("LuaVien", parent, Vector3.zero,
                         matLuaTT != null ? matLuaTT : FlameMat,
                         ParticleSystemRenderMode.Billboard);
        if (matLuaTT != null) BatFlipbook(vien, 6, 6, 1);
        var vm = vien.main;
        vm.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
        vm.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.7f);
        vm.startSize = new ParticleSystem.MinMaxCurve(radius * 0.55f * bLua, radius * 1.15f * bLua);
        vm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        vm.simulationSpace = ParticleSystemSimulationSpace.Local;   // bam theo khoi da
        vm.maxParticles = 160;
        var vem = vien.emission; vem.rateOverTime = 130f;
        var vsh = vien.shape;
        vsh.shapeType = ParticleSystemShapeType.Sphere;
        vsh.radius = radius * 1.08f;
        vsh.radiusThickness = 0.12f;          // chi o LOP VO, khong sinh trong ruot
        var vcol = vien.colorOverLifetime; vcol.enabled = true;
        vcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.88f, 0.55f), 0f,
            new Color(1f, 0.44f, 0.09f), 0.45f,
            new Color(0.42f, 0.07f, 0.01f), 1f,
            0f, 0.85f, 0.5f, 0f));

        // 3) DUOI LUA DAI keo phia sau - phun o khong gian THE GIOI nen hat o lai
        //    dang sau trong khi khoi da lao tiep
        // Phun LUI VE PHIA SAU khoi da (truc z am la phia sau, vi vat the nhin
        // theo huong bay). Truoc day phun ngay tai tam: hat lua sinh ra trong
        // long khoi da, phu kin len no va ca quan chay trang xoa - nhin chi thay
        // mot cuc sang chu khong thay tang da dau ca.
        var duoi = NewPS("DuoiLua", parent, new Vector3(0f, 0f, -radius * 1.15f),
                         matLuaTT != null ? matLuaTT : FlameMat,
                         ParticleSystemRenderMode.Billboard);
        // So VONG cao hon o duoi lua: hat song rat ngan (0,35-0,85 giay) ma van
        // phai thay lua CUON, nen cho no chay het luoi hai lan trong doi hat.
        if (matLuaTT != null) BatFlipbook(duoi, 6, 6, 2);
        var dm = duoi.main;
        dm.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
        dm.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.4f);
        // Hat duoi qua to thi chung chong len nhau ngay o loi, chay trang xoa
        // va nuot mat khoi da ben trong
        dm.startSize = new ParticleSystem.MinMaxCurve(radius * 1.5f * bLua, radius * 3.2f * bLua);
        dm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        dm.simulationSpace = ParticleSystemSimulationSpace.World;
        dm.maxParticles = 500;
        dm.gravityModifier = -0.15f;
        var dem2 = duoi.emission; dem2.rateOverTime = 150f;
        var dsh2 = duoi.shape;
        dsh2.shapeType = ParticleSystemShapeType.Sphere;
        dsh2.radius = radius * 0.55f;
        var dcol2 = duoi.colorOverLifetime; dcol2.enabled = true;
        dcol2.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.95f, 0.72f), 0f,
            new Color(1f, 0.48f, 0.10f), 0.42f,
            new Color(0.30f, 0.05f, 0.01f), 1f,
            0f, 0.95f, 0.65f, 0f));
        var dsz2 = duoi.sizeOverLifetime; dsz2.enabled = true;
        dsz2.size = new ParticleSystem.MinMaxCurve(1f, Curve(1f, 0.75f, 0.15f));

        // 4) Tan lua vang ra hai ben
        var tan = NewPS("TanLua", parent, Vector3.zero, EmberMat,
                        ParticleSystemRenderMode.Stretch);
        var tm2 = tan.main;
        tm2.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
        tm2.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 5f);
        tm2.startSize = new ParticleSystem.MinMaxCurve(radius * 0.18f, radius * 0.45f);
        tm2.simulationSpace = ParticleSystemSimulationSpace.World;
        tm2.maxParticles = 220;
        tm2.gravityModifier = 0.35f;
        var tem = tan.emission; tem.rateOverTime = 85f;
        var tsh = tan.shape;
        tsh.shapeType = ParticleSystemShapeType.Sphere;
        tsh.radius = radius * 1.25f;
        tsh.radiusThickness = 0.45f;      // vang ra tu VO, khong sinh o giua

        // 5) Khoi den un un phia sau
        var khoi = NewPS("KhoiThienThach", parent, Vector3.zero, SmokeMat,
                         ParticleSystemRenderMode.Billboard);
        var km = khoi.main;
        km.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 2.2f);
        km.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.1f);
        km.startSize = new ParticleSystem.MinMaxCurve(radius * 2.4f, radius * 5f);
        km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        km.simulationSpace = ParticleSystemSimulationSpace.World;
        km.maxParticles = 260;
        km.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.14f, 0.12f, 0.11f), new Color(0.30f, 0.26f, 0.24f));
        var kem = khoi.emission; kem.rateOverTime = 70f;
        var ksh = khoi.shape; ksh.shapeType = ParticleSystemShapeType.Sphere;
        ksh.radius = radius * 0.9f;
        var kcol = khoi.colorOverLifetime; kcol.enabled = true;
        kcol.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.4f, Color.white, 1f,
            0f, 0.75f, 0.45f, 0f));
        var ksz = khoi.sizeOverLifetime; ksz.enabled = true;
        ksz.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.9f)));

        // 6) Anh lua hat ra chung quanh khi no lao qua
        var lightGo = new GameObject("AnhLua");
        lightGo.transform.SetParent(parent, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.62f, 0.26f);
        lt.intensity = 4.2f;
        lt.range = 15f * radius;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 4.2f; fl.amount = 0.35f; fl.speed = 18f; fl.rangeWobble = 0.10f;
    }

    /// <summary>
    /// VUNG DAT CHAY AM I sau khi thien thach roi trung.
    ///
    /// Lua liem len TU MAT DAT trong ca vung, khong phai mot cot lua o giua:
    /// hinh phun la mot HINH TRON DAC (radiusThickness = 1) nen ngon lua moc len
    /// rai deu khap vung, dung nhu ca bai dat dang chay.
    ///
    /// Tu tat sau <paramref name="thoiGian"/> giay - ngung phun truoc, roi de
    /// dam lua con lai tu tan het. Tat ca mot luc thi nhin nhu bi loi.
    /// </summary>
    public static GameObject BuildVungLua(Transform parent, float banKinh, float thoiGian)
    {
        var root = new GameObject("VfxVungLua");
        if (parent != null) root.transform.SetParent(parent, false);

        // 1) Vet chay den in xuong dat.
        //    Dung GroundDecal chu khong dap mot tam phang: mat dat o day ghe,
        //    tam phang se bi chon mot nua xuong duoi dat.
        //    Vet o LAI LAU HON ngon lua - chay xong thi dat van con den mot luc.
        GroundDecal.Spawn(new Vector3(root.transform.position.x,
                                      GroundY(root.transform.position),
                                      root.transform.position.z),
                          banKinh * 1.15f, new Material(ScorchMat),
                          thoiGian + 9f, thoiGian + 4f);

        // 2) Ngon lua liem len khap vung
        // Uu tien FLIPBOOK: ngon lua liem va doi hinh ngay trong tung hat.
        // Chua co file thi quay ve FlameMat sinh bang code.
        var matLua = LuaCuonMat;
        var lua = NewPS("LuaNen", root.transform, new Vector3(0f, 0.05f, 0f),
                        matLua != null ? matLua : FlameMat,
                        ParticleSystemRenderMode.Billboard);
        if (matLua != null) BatFlipbook(lua, 6, 6, 1);
        // NGON LUA PHAI THAP, chi liem tren mat dat khoang mot tam nguoi.
        // Do lai lan dau: hat co toi 2,8 m, bay len voi toc do 2,4 m/s va song
        // hon mot giay - cot lua cao 5-6 m, cao gap may lan bia mo, che kin ca
        // khung hinh. Ba so duoi day keo no ve khoang 1,2-1,8 m.
        var lm = lua.main;
        lm.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.70f);
        lm.startSpeed = new ParticleSystem.MinMaxCurve(0.45f, 1.25f);
        // HAT PHAI NHO. Do lai canh mot cai lu lua trong canh: lu lua nhin RAT
        // NET voi luoi lua canh ro, con vung lua nay thi nhoe thanh mot mang
        // cam - cung mot texture, khac nhau o cho hat cua no to gap may lan.
        //
        // Hat to 0,99-2,48 m, 205 hat song cung luc, blend cong sang: do duoc
        // 28,6% so diem anh sang co kenh DO KEP o 255. Moi bien thien trong
        // vung do bi cat mat - do moi la cai "mo", chu khong phai thieu diem
        // anh (texture 128 px/o van dang bi THU NHO o goc choi that).
        lm.startSize = new ParticleSystem.MinMaxCurve(banKinh * 0.17f, banKinh * 0.42f);
        lm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        lm.simulationSpace = ParticleSystemSimulationSpace.World;
        lm.maxParticles = 1600;
        lm.gravityModifier = -0.20f;                  // lua boc LEN, nhung nhe thoi
        // PHAI THAT NHIEU HAT. Hat nho ma thua thi nhin ra tung manh lua roi
        // troi lo lung, thay ro tung canh tam giac cua anh ngon lua. Phai du day
        // de chung chong len nhau thanh mot mang lua lien tuc - do moi ra "ca
        // vung dat dang chay". Do lai: 94 hat/giay ra 48 hat cung luc, thua hoac;
        // 320 hat/giay moi du.
        // Hat nho di thi dien tich moi hat con 58%; nang so hat len 1,7 lan
        // de tong dien tich phu giu nguyen - vung lua van day nhu cu, nhung
        // chia thanh nhieu luoi lua nho co canh doc duoc thay vi vai mang to.
        var lem = lua.emission; lem.rateOverTime = 150f * banKinh;
        var lsh = lua.shape;
        lsh.shapeType = ParticleSystemShapeType.Circle;
        lsh.radius = banKinh * 0.94f;
        lsh.radiusThickness = 1f;                     // dac ruot: lua moc khap vung
        var lcol = lua.colorOverLifetime; lcol.enabled = true;
        // Dinh alpha 0,95 -> 0,72: hat chong nhau nhieu lop, de nguyen thi tong
        // vuot 255 o kenh do va ca dam lua bet lai thanh mot mang.
        lcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.92f, 0.66f), 0f,
            new Color(1f, 0.45f, 0.10f), 0.40f,
            new Color(0.26f, 0.04f, 0.01f), 1f,
            0f, 0.72f, 0.46f, 0f));
        var lsz = lua.sizeOverLifetime; lsz.enabled = true;
        lsz.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.55f, 1f, 0.2f));

        // KHONG BAT NOISE o day, du cai lu lua trong canh co bat va nhin rat
        // net. Da thu: noise gom hat lai thanh cum nen chung chong nhau day
        // hon, va bao hoa kenh do vot tu 11,4% len 38,1% - te hon ca ban goc.
        // Do net cung tut tu 3,76 xuong 3,37.
        //
        // Lu lua thoat duoc vi no chi co 150 hat trong mot cot hep; vung lua
        // nay co 350 hat trai tren ban kinh 4,5 m.

        // 3) Tan lua bay len roi tat
        var tan = NewPS("TanBay", root.transform, new Vector3(0f, 0.1f, 0f), EmberMat,
                        ParticleSystemRenderMode.Billboard);
        var tm = tan.main;
        tm.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 2.2f);
        tm.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 2.2f);
        tm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
        tm.simulationSpace = ParticleSystemSimulationSpace.World;
        tm.maxParticles = 220;
        tm.gravityModifier = -0.22f;
        var tem2 = tan.emission; tem2.rateOverTime = 9f * banKinh;
        var tsh2 = tan.shape;
        tsh2.shapeType = ParticleSystemShapeType.Circle;
        tsh2.radius = banKinh * 0.9f;
        tsh2.radiusThickness = 1f;
        var tcol = tan.colorOverLifetime; tcol.enabled = true;
        tcol.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.85f, 0.45f), 0f,
            new Color(1f, 0.42f, 0.08f), 0.5f,
            new Color(0.5f, 0.10f, 0.02f), 1f,
            0f, 0.9f, 0.5f, 0f));

        // 4) Khoi den bo len
        var khoi = NewPS("KhoiVungLua", root.transform, new Vector3(0f, 0.15f, 0f), SmokeMat,
                         ParticleSystemRenderMode.Billboard);
        var km = khoi.main;
        km.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
        km.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.6f);
        km.startSize = new ParticleSystem.MinMaxCurve(banKinh * 0.35f, banKinh * 0.8f);
        km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        km.simulationSpace = ParticleSystemSimulationSpace.World;
        km.maxParticles = 200;
        km.gravityModifier = -0.30f;
        km.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.16f, 0.14f, 0.13f), new Color(0.34f, 0.30f, 0.27f));
        var kem = khoi.emission; kem.rateOverTime = 7f * banKinh;
        var ksh = khoi.shape;
        ksh.shapeType = ParticleSystemShapeType.Circle;
        ksh.radius = banKinh * 0.8f;
        ksh.radiusThickness = 1f;
        var kcol = khoi.colorOverLifetime; kcol.enabled = true;
        kcol.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.4f, Color.white, 1f,
            0f, 0.55f, 0.35f, 0f));

        // 5) Anh lua hat len canh vat quanh do
        var lightGo = new GameObject("AnhVungLua");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(1f, 0.58f, 0.22f);
        lt.intensity = 2.8f;
        lt.range = 7f + banKinh * 2.2f;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 2.8f; fl.amount = 0.45f; fl.speed = 9f; fl.rangeWobble = 0.14f;

        // 6) Het gio thi NGUNG PHUN, khong tat ngay - dam lua con lai tu tan
        var tatDan = root.AddComponent<TatDanVungLua>();
        tatDan.thoiGian = thoiGian;
        tatDan.den = lt;
        tatDan.denSangGoc = lt.intensity;

        return root;
    }

    // ================================================================
    //  KHIENG BAO VE
    // ================================================================

    static Material mKhieng;

    /// <summary>
    /// Chieu cao cua VOM khieng, tinh theo ti le voi ban kinh ngang.
    ///
    /// Khieng KHONG phai qua cau tron ma la mot cai VOM BAN CAU up xuong dat:
    /// chi co nua tren, day cat phang nam sat mat dat.
    /// </summary>
    // 1,0957 = 1,26 / 1,15. Ban kinh vua noi 15% ma day la TI LE cao tren ban
    // kinh, nen phai chia cho dung 1,15 thi chieu cao thuc te moi giu nguyen
    // 3,33 m. De nguyen 1,26 la vom cao len theo, khong con "giu nguyen chieu
    // cao" nhu yeu cau.
    public const float CaoVomKhieng = 1.0957f;

    /// <summary>Vat lieu qua cau khieng. Moi cai khieng se lay mot BAN RIENG.</summary>
    public static Material KhiengMat
    {
        get
        {
            if (mKhieng == null)
            {
                var sh = Shader.Find("Diablo25D/Khieng");
                if (sh == null)
                {
                    // KHONG duoc tra ve mot vat lieu HIEU DUNG o day.
                    //
                    // Ban truoc tra ve Mats.FireSoft, tuc vat lieu LUA. Tren
                    // WebGL shader nay bi loai khoi build nen nhanh nay chay
                    // that - va cai vom khieng ra mot QUA CAU LUA vang cam cuon
                    // xoay. Trong nhu mot hieu ung co that, chi la sai hieu ung,
                    // nen khong ai doan duoc rang goc re la THIEU SHADER.
                    //
                    // Magenta thi khong the nham voi bat cu thu gi: nhin mot cai
                    // la biet phai di tim shader, khong phai di sua so lieu.
                    Debug.LogError("[Vfx] Khong tim thay shader Diablo25D/Khieng - " +
                                   "shader chua duoc dua vao build. Them no vao " +
                                   "Project Settings > Graphics > Always Included Shaders.");
                    return Mats.Additive("M_KhiengThieuShader", TextureFactory.Ring(0.14f),
                                         Color.magenta, 1f);
                }
                mKhieng = new Material(sh) { name = "M_Khieng" };
            }
            return mKhieng;
        }
    }

    /// <summary>
    /// VOM khieng bao quanh nhan vat.
    ///
    /// Chan vom nam NGAY TREN MAT DAT, tuc ngay o chan nhan vat - day la mot
    /// cai vom up xuong dat, khong phai qua cau lo lung.
    ///
    /// Luoi chia min (34 canh x 16 vong) vi tia set tren shader chay theo phap
    /// tuyen - luoi tho qua thi tia bi gay khuc o cac canh.
    /// </summary>
    public static GameObject BuildKhieng(Transform parent, float banKinh)
    {
        var root = new GameObject("Khieng");
        root.transform.SetParent(parent, false);

        // Chan vom nam NGAY TREN MAT DAT, tuc ngay o chan nhan vat
        root.transform.localPosition = Vector3.zero;

        var qua = ProcMesh.Part("QuaCau", root.transform,
                                LuoiVom(banKinh),
                                KhiengMat, Vector3.zero, Quaternion.identity,
                                Vector3.one, false);

        var mr = qua.GetComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        // Vai dom sang troi ben trong vom cho no co chieu sau
        var dom = NewPS("DomKhieng", root.transform,
                        new Vector3(0f, banKinh * CaoVomKhieng * 0.42f, 0f), GlowMat,
                        ParticleSystemRenderMode.Billboard);
        var dm = dom.main;
        dm.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
        dm.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
        dm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
        dm.simulationSpace = ParticleSystemSimulationSpace.Local;
        dm.maxParticles = 60;
        // Vang kim, khop voi mau vom - xem bang mau trong S_Khieng.shader
        dm.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.82f, 0.34f), new Color(1f, 0.97f, 0.82f));
        var dem = dom.emission; dem.rateOverTime = 22f;
        var dsh = dom.shape;
        dsh.shapeType = ParticleSystemShapeType.Sphere;
        dsh.radius = banKinh * 0.92f;
        dsh.radiusThickness = 0.35f;
        // Bop hinh phun theo dung do det cua qua cau, khong thi dom sang bay ca
        // ra ngoai vom o phia tren va phia duoi
        dsh.scale = new Vector3(1f, CaoVomKhieng * 0.72f, 1f);
        var dcol = dom.colorOverLifetime; dcol.enabled = true;
        dcol.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.45f, Color.white, 1f,
            0f, 0.85f, 0.5f, 0f));

        // Anh hat ra chung quanh, dat lung chung trong vom
        var lightGo = new GameObject("AnhKhieng");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, banKinh * CaoVomKhieng * 0.5f, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        // Anh tim hat ra dat va len nguoi nhan vat, cho khieng an nhap voi
        // canh chu khong nhu mot cai vom dan len tren
        lt.color = new Color(0.72f, 0.48f, 1.0f);
        lt.intensity = 1.62f;      // sang hon 35% (1,2 x 1,35)
        lt.range = banKinh * 4.5f;
        lt.shadows = LightShadows.None;

        return root;
    }

    /// <summary>
    /// LUOI VOM BAN CAU cho khieng: chi nua tren, day cat phang sat mat dat.
    ///
    /// Chia deu theo GOC chu khong theo chieu cao. Chia theo chieu cao thi sat
    /// dinh cac vong thua ra va cho chum lai bi gay khuc - nhin ra dinh nhon
    /// hoat, du do ban kinh hai ben lech nhau dung 0,000 m. Toi da vap dung
    /// loi do mot lan.
    ///
    /// Khong dong day (capBottom = false): day nam sat dat nen khong ai thay,
    /// ma shader lai ve ca hai mat nen dung ben trong nhin ra van thay du vom.
    /// </summary>
    static Mesh LuoiVom(float banKinh)
    {
        const int Vong = 16;                // so vong tu chan len dinh
        const int Canh = 34;                // so canh quanh moi vong

        float cao = banKinh * CaoVomKhieng;
        var vien = new Vector2[Vong + 1];

        for (int i = 0; i <= Vong; i++)
        {
            // 0 = chan vom (sat dat), pi/2 = dinh
            float goc = Mathf.PI * 0.5f * i / Vong;
            vien[i] = new Vector2(Mathf.Cos(goc) * banKinh, Mathf.Sin(goc) * cao);
        }
        vien[Vong] = new Vector2(0f, cao);   // khep han dinh lai

        return ProcMesh.Revolve(vien, Canh, false, false, Color.white, "LuoiVomKhieng");
    }

    /// <summary>Khieng het mau: no tung thanh manh nang luong.</summary>
    public static void KhiengVo(Vector3 tam, float banKinh)
    {
        var root = new GameObject("KhiengVo");
        root.transform.position = tam;

        // Manh vo bay tung ra moi huong
        // Billboard + tu xoay, KHONG dung Stretch: Stretch keo manh dai ra theo
        // huong bay, thanh nhung vet thuon dai. Manh vo that thi xoay lon trong
        // khong khi chu khong bi keo dai.
        var manh = NewPS("ManhKhieng", root.transform, Vector3.zero, ShardKhiengMat,
                         ParticleSystemRenderMode.Billboard);

        // Moi hat lay MOT O CO DINH trong luoi 4x4, giu nguyen suot doi.
        //
        // frameOverTime = 0 nen khung khong chay; startFrame ngau nhien 0..16
        // nen moi hat mot hinh manh khac nhau. Day KHAC voi flipbook khoi/lua -
        // o day khong co gi phai chuyen dong ben trong hat, chi can moi manh
        // mot hinh.
        var mtsa = manh.textureSheetAnimation;
        mtsa.enabled = true;
        mtsa.mode = ParticleSystemAnimationMode.Grid;
        mtsa.numTilesX = 4;
        mtsa.numTilesY = 4;
        mtsa.animation = ParticleSystemAnimationType.WholeSheet;
        mtsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
        mtsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        mtsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 16f);
        var mm = manh.main;
        mm.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        mm.startSpeed = new ParticleSystem.MinMaxCurve(4f, 11f);
        // Kich thuoc CHENH NHAU NHIEU: mot cai vo that cho ra vai manh lon lan
        // rat nhieu manh vun. De khoang hep (0,12-0,42) thi manh nao cung xap xi
        // nhau, nhin ra mot bo canh giong nhau chu khong ra vo.
        mm.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.52f);
        mm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mm.simulationSpace = ParticleSystemSimulationSpace.World;
        mm.maxParticles = 160;
        mm.gravityModifier = 0.55f;
        // Ca hai dau gradient deu nga tim. De mot dau trang thi mot nua so
        // manh bay ra mau trang, nhin ra manh thuy tinh chu khong ra manh cua
        // cai khieng tim vua vo.
        mm.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.70f, 0.14f), new Color(1f, 0.94f, 0.70f));
        var mem = manh.emission;
        mem.rateOverTime = 0f;
        mem.SetBursts(new[] { new ParticleSystem.Burst(0f, 115) });
        var msh = manh.shape;
        msh.shapeType = ParticleSystemShapeType.Sphere;
        msh.radius = banKinh * 0.85f;
        msh.radiusThickness = 0.2f;

        // Xoay lon trong khong khi - manh vo bay ra thi phai lat
        var mrot = manh.rotationOverLifetime; mrot.enabled = true;
        mrot.z = new ParticleSystem.MinMaxCurve(-6.5f, 6.5f);

        // Nho dan ve cuoi: manh bay xa thi nhin nho lai roi tan
        var msz = manh.sizeOverLifetime; msz.enabled = true;
        msz.size = new ParticleSystem.MinMaxCurve(1f, Curve(1f, 0.9f, 0.35f));

        // Khoi tim un len tu cho khieng vua vo.
        //
        // Manh vo bay di rat nhanh (0,5-1,2 giay) roi het, khong con dau vet gi.
        // Dam khoi o lai lau hon, danh dau cho cai khieng vua tan - nhin ra mot
        // cu no chu khong phai vai manh thuy tinh bay ngang qua.
        var matKhoiKhieng = KhoiKhiengMat;
        if (matKhoiKhieng != null)
        {
            var khoi = NewPS("KhoiKhieng", root.transform, Vector3.zero, matKhoiKhieng,
                             ParticleSystemRenderMode.Billboard);
            BatFlipbook(khoi, 6, 6, 1);
            var km = khoi.main;
            km.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.5f);
            km.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.4f);
            // Hat khoi phai NHO. Do lai lan dau: 0,55-1,20 lan ban kinh khieng
            // (tuc 1,1-2,4 met moi hat) cong voi 38 hat bung ra cung luc thi
            // dam khoi trum kin ca khung hinh, khong con thay nhan vat lan manh
            // vo dau ca - dung cai loi ma khoi le ra phai lam noi bat.
            km.startSize = new ParticleSystem.MinMaxCurve(banKinh * 0.26f * BuKhoi,
                                                          banKinh * 0.58f * BuKhoi);
            km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            km.simulationSpace = ParticleSystemSimulationSpace.World;
            km.maxParticles = 90;
            km.gravityModifier = -0.05f;
            var kem = khoi.emission;
            kem.rateOverTime = 0f;
            kem.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });
            var ksh = khoi.shape;
            ksh.shapeType = ParticleSystemShapeType.Sphere;
            ksh.radius = banKinh * 0.55f;
            ksh.radiusThickness = 0.5f;
            var kcol = khoi.colorOverLifetime; kcol.enabled = true;
            kcol.color = new ParticleSystem.MinMaxGradient(Grad(
                new Color(1f, 0.98f, 0.88f), 0f,
                new Color(1f, 0.78f, 0.26f), 0.35f,
                new Color(0.45f, 0.26f, 0.05f), 1f,
                0f, 0.55f, 0.30f, 0f));
            var ksz = khoi.sizeOverLifetime; ksz.enabled = true;
            ksz.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.6f, 1.25f, 1.7f));
            var krot = khoi.rotationOverLifetime; krot.enabled = true;
            krot.z = new ParticleSystem.MinMaxCurve(-1.1f, 1.1f);
        }

        // Vong song lan ra tren mat dat
        var song = NewPS("SongKhieng", root.transform, Vector3.zero, RingKhiengMat,
                         ParticleSystemRenderMode.Billboard);
        var sm = song.main;
        sm.startLifetime = 0.55f;
        sm.startSpeed = 0f;
        sm.startSize = banKinh * 1.1f;
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.maxParticles = 6;
        var sem = song.emission;
        sem.rateOverTime = 0f;
        sem.SetBursts(new[] { new ParticleSystem.Burst(0f, 2) });
        var ssh = song.shape; ssh.enabled = false;
        var ssz = song.sizeOverLifetime; ssz.enabled = true;
        ssz.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 3.2f)));
        var scol = song.colorOverLifetime; scol.enabled = true;
        scol.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.4f, Color.white, 1f,
            0.9f, 0.6f, 0.25f, 0f));

        // Loe sang mot cai roi tat - dung LightBurst co san cua du an
        var lightGo = new GameObject("AnhVo");
        lightGo.transform.SetParent(root.transform, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.70f, 0.42f, 1f);
        lt.shadows = LightShadows.None;
        var burst = lightGo.AddComponent<LightBurst>();
        burst.peak = 5.5f;
        burst.duration = 0.45f;
        burst.range = banKinh * 6f;

        var ad = root.AddComponent<AutoDestroy>();
        ad.lifetime = 2.2f;
    }

    /// <summary>Tim do cao mat dat ngay duoi mot diem.</summary>
    public static float GroundY(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 8f, Vector3.down, out hit, 40f,
                            LayerMask.GetMask("Ground", "Default"), QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return 0f;
    }
}
