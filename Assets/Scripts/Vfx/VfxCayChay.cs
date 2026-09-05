using UnityEngine;

/// <summary>
/// PHAN HIEU UNG CUA MOT CAI CAY DANG BOC CHAY.
///
/// Nam rieng mot file nhung van la <see cref="VfxFactory"/> (partial) - no can
/// dung cac ham dung he hat rieng cua lop do (<c>NewPS</c>, <c>Grad</c>,
/// <c>BatFlipbook</c>...), ma nhung ham do khong nen mo ra cho ca du an.
/// </summary>
public static partial class VfxFactory
{
    // ================================================================
    //  CAY BOC CHAY
    // ================================================================

    /// <summary>
    /// Nhung he hat ma <see cref="CayChay"/> TU BAN HAT VAO - chung khong tu
    /// phun, vi cho phun la be mat that cua cai cay chu khong phai mot khoi
    /// hinh hoc nao.
    /// </summary>
    public struct BoLuaCay
    {
        public GameObject goc;
        public ParticleSystem luaCanh;
        public ParticleSystem khoiCanh;
    }

    static Material mThanCay;

    /// <summary>
    /// Manh cay da chay den, ben trong con am than do.
    ///
    /// Mau nen gan den han chu khong nau: manh roi xuong la manh DA CHAY, khong
    /// phai manh go tuoi. Phan phat sang mau cam do va rat nhe - de manh khong
    /// ra mot cuc than hong dang bay, chi la mot khuc den con do am i o khe nut.
    /// </summary>
    public static Material ThanCayMat
    {
        get
        {
            if (mThanCay == null)
                mThanCay = Mats.Glow("ThanCayChay", new Color(0.055f, 0.045f, 0.040f),
                                     new Color(0.95f, 0.30f, 0.06f), 0.42f);
            return mThanCay;
        }
    }

    static Mesh[] meshManhCay;

    /// <summary>
    /// Ba khuc canh gay DUNG CHUNG cho moi manh roi.
    ///
    /// Mot cay chay tha ca chuc manh, va vai cay chay cung luc thi len ca tram:
    /// dung mesh moi cho tung manh la tram cai luoi rac moi lan nguoi choi bam
    /// mot phim. Ba bien the la du de khong nhin ra chung giong het nhau, vi
    /// moi manh con duoc xoay ngau nhien va co lon khac nhau.
    /// </summary>
    static Mesh[] MeshManhCay
    {
        get
        {
            if (meshManhCay == null || meshManhCay[0] == null)
            {
                meshManhCay = new Mesh[3];
                for (int i = 0; i < 3; i++)
                {
                    // Khuc canh: mot dau to hon dau kia, cho ra hinh canh gay
                    var m = ProcMesh.Taper(0.055f + i * 0.018f, 0.028f,
                                           0.34f + i * 0.16f, 5, true, Color.white);
                    ProcMesh.Displace(m, 0.012f, 3.2f, 101 + i * 37);
                    m.name = "ManhCay" + i;
                    meshManhCay[i] = m;
                }
            }
            return meshManhCay;
        }
    }

    /// <summary>
    /// MOT MANG CAY CHAY ROT XUONG, keo mot dai khoi den theo sau.
    ///
    /// Khoi phun ra o he toa do THE GIOI du emitter di theo manh: hat da nha ra
    /// thi o lai cho do, nen manh roi xuong de lai mot vet khoi keo dai. De he
    /// toa do local thi ca dam khoi roi theo manh - nhin ra mot cuc bong bay
    /// chu khong ra mot manh dang chay roi xuong.
    ///
    /// Manh TU ROI bang <see cref="RoiXuongDat"/> chu khong dung Rigidbody: chi
    /// can no ha xuong dam lua duoi goc, khong can no lan hay nay.
    /// </summary>
    public static GameObject ManhCayRoi(Vector3 tu, float coLon)
    {
        var luoi = MeshManhCay;
        var go = new GameObject("ManhCayChay");
        go.transform.position = tu;
        go.transform.rotation = Random.rotation;
        go.transform.localScale = Vector3.one * coLon;

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = luoi[Random.Range(0, luoi.Length)];
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = ThanCayMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        // Khoi DEN keo theo, tham hon khoi cua vung lua: day la go dang chay do
        // dang chu khong phai lua sach.
        var khoi = NewPS("KhoiManh", go.transform, Vector3.zero, SmokeMat,
                         ParticleSystemRenderMode.Billboard);
        var km = khoi.main;
        km.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        km.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        km.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.95f);
        km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        km.simulationSpace = ParticleSystemSimulationSpace.World;
        km.maxParticles = 40;
        km.gravityModifier = -0.25f;
        km.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.10f, 0.09f, 0.08f), new Color(0.26f, 0.23f, 0.20f));
        var kem = khoi.emission; kem.rateOverTime = 26f;
        var ksh = khoi.shape; ksh.shapeType = ParticleSystemShapeType.Sphere; ksh.radius = 0.12f;
        var kco = khoi.colorOverLifetime; kco.enabled = true;
        kco.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.30f));
        var ksz = khoi.sizeOverLifetime; ksz.enabled = true;
        ksz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f), new Keyframe(1f, 1.5f)));

        // Vai dom lua bam tren manh. Thieu no thi manh den tren nen dem gan nhu
        // vo hinh: chi thay dai khoi bay ma khong thay cai gi dang roi.
        var dom = NewPS("DomLuaManh", go.transform, Vector3.zero, EmberMat,
                        ParticleSystemRenderMode.Billboard);
        var dm = dom.main;
        dm.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        dm.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.9f);
        dm.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
        dm.simulationSpace = ParticleSystemSimulationSpace.World;
        dm.maxParticles = 26;
        dm.gravityModifier = -0.10f;
        dm.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.52f, 0.14f), new Color(1f, 0.80f, 0.35f));
        var dem = dom.emission; dem.rateOverTime = 22f;
        var dsh = dom.shape; dsh.shapeType = ParticleSystemShapeType.Sphere; dsh.radius = 0.10f;
        var dco = dom.colorOverLifetime; dco.enabled = true;
        dco.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.25f));

        // Roi tu do; bat len mot chut luc dau cho ra ve vua bi bat ra khoi than
        var roi = RoiXuongDat.Gan(go, Random.Range(0.2f, 1.4f));
        roi.nganNguoi = false;
        roi.trongTruong = 15f;

        // Cham dat roi thi con am mot lat nua moi tan
        AutoDestroy.Add(go, 4.2f);
        return go;
    }

    /// <summary>
    /// CA MOT CAI CAY BOC CHAY.
    ///
    /// ===================================================================
    ///  LUA BAM BE MAT THAT, KHONG PHAI MOT KHOI HINH HOC QUANH THAN
    /// ===================================================================
    ///
    /// Ban dung dau tien rai lua bang mot cai HOP quanh than cong mot QUA CAU o
    /// tan. Ket qua: nhin ra "co mot dam lua o cho cai cay", chu khong ra "cai
    /// cay dang chay". Tan cay Act2 toe rong toi 23,6 m ma qua cau bi chan o
    /// 5,5 m, nen phan lon canh nam ngoai lua - dung nhu nguoi dung noi, lua chi
    /// hien ra mot phan tren cay.
    ///
    /// Nay hai lop <c>LuaCanh</c> va <c>KhoiCanh</c> KHONG tu phun
    /// (<c>rateOverTime = 0</c>): <see cref="CayChay"/> tu ban hat vao dung
    /// nhung diem nam TREN BE MAT cai cay - canh lon, canh nho, chum la - va lan
    /// dan tu goc ra toi dau canh cuoi cung.
    ///
    /// Ba lop con lai van la hinh hoc, vi chung khong bam theo cay:
    ///
    /// <code>
    ///   LUA GOC   dam lua day nhat om lay chan cay - dung cho ke dich dung
    ///   TAN LUA   dom do bay len - thu lam dam lua noi len tren nen dem
    ///   ANH LUA   ngon den hat sang len canh vat chung quanh
    /// </code>
    ///
    /// Dung CHUNG anh lua flipbook voi vung lua cua thien thach, nen hai dam lua
    /// nhin ra cung mot chat lieu - dung y do, vi lua nay do chinh thien thach do.
    /// </summary>
    public static BoLuaCay BuildCayChay(Vector3 chan, float cao, float banKinhLua,
                                        float banKinhCanh, float thoiGian, bool coDen)
    {
        var root = new GameObject("VfxCayChay");
        root.transform.position = chan;

        var matLua = LuaCuonMat;
        var dungLua = matLua != null ? matLua : FlameMat;

        // ---- 1. LUA BAM TREN CANH (CayChay tu ban hat vao) ----
        //
        // Hat NHO: no phai om lay mot khuc canh day vai chuc phan, khong phai
        // mot qua cau lua giua troi. Ban dau de hat 1,5..4,0 m thi tren tan cay
        // nhin ra tung canh lua roi rac troi lo lung, thay ro tung mieng anh.
        var luaCanh = NewPS("LuaCanh", root.transform, Vector3.zero, dungLua,
                            ParticleSystemRenderMode.Billboard);
        if (matLua != null) BatFlipbook(luaCanh, 6, 6, 1);
        var lm = luaCanh.main;
        lm.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.80f);
        lm.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.8f);
        lm.startSize = new ParticleSystem.MinMaxCurve(Mathf.Clamp(cao * 0.055f, 0.30f, 0.85f),
                                                      Mathf.Clamp(cao * 0.150f, 0.70f, 2.10f));
        lm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        lm.simulationSpace = ParticleSystemSimulationSpace.World;
        lm.maxParticles = 1200;
        lm.gravityModifier = -0.42f;                        // lua boc len
        var lem = luaCanh.emission; lem.rateOverTime = 0f;  // CayChay tu Emit
        var lco = luaCanh.colorOverLifetime; lco.enabled = true;
        lco.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.92f, 0.66f), 0f,
            new Color(1f, 0.45f, 0.10f), 0.40f,
            new Color(0.26f, 0.04f, 0.01f), 1f,
            0f, 0.72f, 0.46f, 0f));
        var lsz = luaCanh.sizeOverLifetime; lsz.enabled = true;
        lsz.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.5f, 1f, 0.22f));

        // ---- 2. KHOI DEN BOC LEN TU CHO DANG CHAY ----
        var khoiCanh = NewPS("KhoiCanh", root.transform, Vector3.zero, SmokeMat,
                             ParticleSystemRenderMode.Billboard);
        var kc = khoiCanh.main;
        kc.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
        kc.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 2.0f);
        kc.startSize = new ParticleSystem.MinMaxCurve(Mathf.Clamp(cao * 0.10f, 0.6f, 1.7f),
                                                      Mathf.Clamp(cao * 0.26f, 1.4f, 3.6f));
        kc.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        kc.simulationSpace = ParticleSystemSimulationSpace.World;
        kc.maxParticles = 300;
        kc.gravityModifier = -0.42f;
        kc.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.15f, 0.13f, 0.12f), new Color(0.33f, 0.29f, 0.26f));
        var kce = khoiCanh.emission; kce.rateOverTime = 0f;   // CayChay tu Emit
        var kcc = khoiCanh.colorOverLifetime; kcc.enabled = true;
        kcc.color = new ParticleSystem.MinMaxGradient(TatDanCuoi(0.42f));
        var kcs = khoiCanh.sizeOverLifetime; kcs.enabled = true;
        kcs.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.45f), new Keyframe(0.5f, 1.0f), new Keyframe(1f, 1.75f)));

        // ---- 3. LUA O GOC (cung la vung gay sat thuong) ----
        var goc = NewPS("LuaGoc", root.transform, new Vector3(0f, 0.1f, 0f),
                        dungLua, ParticleSystemRenderMode.Billboard);
        if (matLua != null) BatFlipbook(goc, 6, 6, 1);
        var gm = goc.main;
        gm.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.95f);
        gm.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.0f);
        gm.startSize = new ParticleSystem.MinMaxCurve(banKinhLua * 0.28f, banKinhLua * 0.70f);
        gm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        gm.simulationSpace = ParticleSystemSimulationSpace.World;
        gm.maxParticles = 500;
        gm.gravityModifier = -0.35f;
        var gem = goc.emission; gem.rateOverTime = 55f * banKinhLua;
        var gsh = goc.shape;
        gsh.shapeType = ParticleSystemShapeType.Circle;
        gsh.radius = banKinhLua * 0.85f;
        gsh.radiusThickness = 1f;
        var gco = goc.colorOverLifetime; gco.enabled = true;
        gco.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.92f, 0.66f), 0f,
            new Color(1f, 0.45f, 0.10f), 0.40f,
            new Color(0.26f, 0.04f, 0.01f), 1f,
            0f, 0.72f, 0.46f, 0f));
        var gsz = goc.sizeOverLifetime; gsz.enabled = true;
        gsz.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.55f, 1f, 0.25f));

        // ---- 4. TAN LUA BAY LEN ----
        var domLua = NewPS("TanCayChay", root.transform, new Vector3(0f, cao * 0.35f, 0f),
                           EmberMat, ParticleSystemRenderMode.Billboard);
        var dm2 = domLua.main;
        dm2.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
        dm2.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
        dm2.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.17f);
        dm2.simulationSpace = ParticleSystemSimulationSpace.World;
        dm2.maxParticles = 260;
        dm2.gravityModifier = -0.30f;
        var dem2 = domLua.emission; dem2.rateOverTime = 8f * banKinhCanh + 6f;
        var dsh2 = domLua.shape;
        dsh2.shapeType = ParticleSystemShapeType.Box;
        dsh2.scale = new Vector3(banKinhCanh * 1.5f, cao * 0.7f, banKinhCanh * 1.5f);
        var dco2 = domLua.colorOverLifetime; dco2.enabled = true;
        dco2.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.85f, 0.45f), 0f,
            new Color(1f, 0.42f, 0.08f), 0.5f,
            new Color(0.5f, 0.10f, 0.02f), 1f,
            0f, 0.9f, 0.5f, 0f));

        // ---- 5. ANH LUA HAT RA CHUNG QUANH ----
        //
        // CHI HAI CAY DAU DUOC CO DEN. Day la ngon den diem thoi gian thuc voi
        // tam 14 m giua mot nghia dia day dac bia mo: duong ve tien tien (forward)
        // ve LAI moi vat trong tam den mot lan nua cho moi ngon den. Sau ngon
        // cung luc la sau lan ve lai ca vung - dat hon nhieu so voi so hat.
        //
        // Ngon lua van sang vi no la vat lieu cong sang, tu no da rat noi tren
        // nen dem; cai mat di chi la anh hat len bia mo chung quanh.
        Light lt = null;
        if (coDen)
        {
            var lightGo = new GameObject("AnhCayChay");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, cao * 0.35f, 0f);
            lt = lightGo.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.56f, 0.20f);
            lt.intensity = 3.4f;
            lt.range = 9f + banKinhCanh * 2.6f + cao * 0.4f;
            lt.shadows = LightShadows.None;
            var fl = lightGo.AddComponent<LightFlicker>();
            fl.baseIntensity = 3.4f; fl.amount = 0.42f; fl.speed = 8f; fl.rangeWobble = 0.12f;
        }

        // ---- 6. VET CHAY DUOI GOC ----
        //    O lai lau hon ngon lua: cay bien mat roi ma dat duoi goc van con den.
        GroundDecal.Spawn(new Vector3(chan.x, GroundY(chan), chan.z),
                          banKinhLua * 1.3f, new Material(ScorchMat),
                          thoiGian + 12f, thoiGian + 6f);

        // ---- 7. HET GIO THI NGUNG PHUN, khong tat phut mot cai ----
        //
        // TatDanVungLua chi tat duoc nhung lop TU PHUN. Hai lop bam canh do
        // CayChay ban hat vao, nen chinh no phai ngung ban - xem CayChay.Update.
        var tatDan = root.AddComponent<TatDanVungLua>();
        tatDan.thoiGian = thoiGian;
        tatDan.thoiGianTan = 3.4f;
        tatDan.den = lt;
        tatDan.denSangGoc = lt != null ? lt.intensity : 0f;

        var bo = new BoLuaCay();
        bo.goc = root;
        bo.luaCanh = luaCanh;
        bo.khoiCanh = khoiCanh;
        return bo;
    }
}
