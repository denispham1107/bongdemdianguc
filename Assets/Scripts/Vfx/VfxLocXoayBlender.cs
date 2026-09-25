using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HINH LOC XOAY DUNG BANG BLENDER MCP (nguoi dung 25/09/2026, gui hai anh mau: "dung lai hieu ung Loc xoay giong nhu
/// tren hinh 100%, loc khi di chuyen cuon len chi quay xoay theo truc 1 chieu"). Chot: cao 15,4 m dang theo anh
/// (chan hep ~1,3 m, mieng ~6,8 m, loe dan), BO may giong den va vet khoi den (thay bang quang sang tren mieng + bui xam
/// nhat duoi chan), tia set kieu Giut set nhieu nhanh. Vung hut / sat thuong giu nguyen.
///
/// Tai nguyen: CongCu/Blender/loc_xoay.blend -> Resources/KyNang/LocXoay/
///   LocXoay.fbx   bon vo pheu Vo0..Vo3 (ban kinh x0,70 / 0,84 / 1,00 / 1,13) + vanh cuon o mieng "Vanh"
///   GioVo0..3, GioVanh   anh gio xoan LIEN MACH theo u (moi vong u lech dung mot dai -> dai xoan oc)
///   HaoQuangDinh  quang sang tron;  BuiXam  bon dam bui 2x2
///
/// MOT CHIEU: moi lop quay theo <see cref="ChieuQuayGioLoc"/> (goc atan2(z,x) TANG - dung chieu quai / vat bi cuon bay).
/// Tren luoi da nhap, u tang thi goc TANG (do 25/09/2026, 5/5 luoi), ma dai trong anh di len thi u tang -> quay goc tang
/// thi dai TRUOT XUONG. Lat u cua anh (<see cref="LatUAnhLocXoay"/> = -1) thi dai di len thi goc GIAM, quay goc tang ->
/// dai CUON LEN (nhu cot den cat toc), ma moi lop van quay mot chieu.
/// </summary>
public static partial class VfxFactory
{
    const string ThuMucLocXoay = "KyNang/LocXoay/";

    /// <summary>Chieu cao than loc (khong ke vanh cuon o mieng, vanh cao them ~0,4 m).</summary>
    public const float CaoThanLocXoay = 15.0f;

    // Duong vien vo chinh Vo2 (cung cong thuc trong loc_xoay.blend): r = 1,3 + 5,5 * t^1,9
    const float ChanLocXoay = 1.3f, LoeLocXoay = 5.5f, MuLocXoay = 1.9f;

    /// <summary>He so u cua anh gio tren moi lop: -1 = lat, de dai xoan cuon LEN khi quay theo chieu cuon.</summary>
    public const float LatUAnhLocXoay = -1f;

    /// <summary>
    /// DAI KHOI TROI LEN THAT (nguoi dung 25/09/2026 duyet "dai khoi troi len that" trong bon phuong an "cuon tu duoi len"):
    /// anh gio ve lai LIEN MACH CA THEO v (nhieu 4D tren hinh xuyen), tan dan chan / mieng va toi dan xuong chan chuyen sang
    /// MAU DINH cua luoi FBX (khong troi theo anh). Truot v AM = anh chay LEN (v tang tu chan len dinh - do tren luoi).
    /// Don vi: chieu cao anh / giay (1 = 15 m): 0,20 -> 3 m/s o loi, lop ngoai cham dan.
    /// </summary>
    public static readonly float[] TruotLenLocXoay = { 0.20f, 0.16f, 0.13f, 0.10f };

    /// <summary>Ban kinh vo chinh (Vo2) cua Loc xoay o do cao <paramref name="h"/> tinh tu chan.</summary>
    public static float BanKinhLocXoay(float h, float scale)
    {
        float t = Mathf.Clamp01(h / Mathf.Max(0.01f, CaoThanLocXoay * scale));
        return (ChanLocXoay + LoeLocXoay * Mathf.Pow(t, MuLocXoay)) * scale;
    }

    static readonly Dictionary<string, Material> matLocXoay = new Dictionary<string, Material>();

    // Kiem bang null cua Unity: vat lieu tao luc Play bi xoa khi thoat Play (memory vat-lieu-static-bi-xoa-khi-thoat-play)
    static Material VatLieuLocXoay(string ten, string anh, Color mau, bool cong, float cuong = 1f)
    {
        Material m;
        if (matLocXoay.TryGetValue(ten, out m) && m != null) return m;
        var t = Resources.Load<Texture2D>(ThuMucLocXoay + anh);
        m = cong ? Mats.Additive("P_LX_" + ten, t, mau, cuong) : Mats.Alpha("P_LX_" + ten, t, mau);
        matLocXoay[ten] = m;
        return m;
    }

    /// <summary>
    /// Dung hinh Loc xoay (goc = chan loc). Moi thu nam duoi MOT con "LocXoayHinh" - Hoa loc xoay phong to con dau tien
    /// (truoc day chi to duoc lop vo dau, cac lop khac hien ngay du co). Thieu FBX thi quay ve hinh cu dung bang code.
    /// </summary>
    public static GameObject BuildLocXoay(float scale)
    {
        var kho = Resources.Load<GameObject>(ThuMucLocXoay + "LocXoay");
        if (kho == null) return BuildTornadoCu(scale);

        var root = new GameObject("Tornado");
        var hinh = new GameObject("LocXoayHinh");
        hinh.transform.SetParent(root.transform, false);

        // ---- Bon vo pheu + vanh: (anh, mau, do/giay). Lop trong quay nhanh hon lop ngoai, TAT CA cung mot chieu ----
        foreach (var mf in kho.GetComponentsInChildren<MeshFilter>(true))
        {
            string anh; Color mau; float quay, truot;
            switch (mf.name)
            {
                case "Vo0":  anh = "GioVo0";  mau = new Color(0.86f, 0.88f, 0.92f, 1.00f); quay = 210f; truot = TruotLenLocXoay[0]; break;
                case "Vo1":  anh = "GioVo1";  mau = new Color(0.92f, 0.94f, 0.97f, 1.00f); quay = 165f; truot = TruotLenLocXoay[1]; break;
                case "Vo2":  anh = "GioVo2";  mau = new Color(0.97f, 0.98f, 1.00f, 1.00f); quay = 130f; truot = TruotLenLocXoay[2]; break;
                case "Vo3":  anh = "GioVo3";  mau = new Color(1.00f, 1.00f, 1.00f, 0.90f); quay = 100f; truot = TruotLenLocXoay[3]; break;
                case "Vanh": anh = "GioVanh"; mau = new Color(1.00f, 1.00f, 1.00f, 0.95f); quay = 120f; truot = 0f; break;   // vanh: ong vong, khong truot
                default: continue;
            }
            var go = new GameObject(mf.name);
            go.transform.SetParent(hinh.transform, false);
            go.transform.localPosition = mf.transform.localPosition * scale;
            go.transform.localRotation = mf.transform.localRotation;
            go.transform.localScale = mf.transform.localScale * scale;
            go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var mr = go.AddComponent<MeshRenderer>();
            var mat = VatLieuLocXoay(mf.name, anh, mau, false);
            mat.mainTextureScale = new Vector2(LatUAnhLocXoay, 1f);
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            var sp = go.AddComponent<Spin>();
            sp.axis = Vector3.up;
            sp.degreesPerSecond = ChieuQuayGioLoc * quay;
            if (truot > 0f)
            {
                var sc = go.AddComponent<ScrollUV>();
                sc.speed = new Vector2(0f, -truot);      // v AM = chay len (ScrollUV tu tao ban sao vat lieu, giu tiling -1)
            }
        }

        // ---- Quang sang trang tren mieng loc (anh mau: diem sang ngay giua mieng) ----
        var matQuang = VatLieuLocXoay("HaoQuang", "HaoQuangDinh", new Color(0.92f, 0.96f, 1f, 1f), true, 1.1f);
        var quang = NewPS("HaoQuang", hinh.transform, new Vector3(0f, 14.4f * scale, 0f), matQuang, ParticleSystemRenderMode.Billboard);
        var qm = quang.main;
        qm.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        qm.startSpeed = 0f;
        // 3,0 - 4,6 m: ban dau 5,5 - 8,5 thanh mang trang che gan het mieng (anh thanlocxoay_0_dem_can), anh mau chi la mot dom sang
        qm.startSize = new ParticleSystem.MinMaxCurve(3.0f * scale, 4.6f * scale);
        qm.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.45f), new Color(0.85f, 0.92f, 1f, 0.70f));
        qm.simulationSpace = ParticleSystemSimulationSpace.Local;
        qm.scalingMode = ParticleSystemScalingMode.Hierarchy;
        qm.maxParticles = 16;
        var qe = quang.emission; qe.rateOverTime = 12f;
        var qs = quang.shape; qs.shapeType = ParticleSystemShapeType.Sphere; qs.radius = 0.8f * scale;
        var qc = quang.colorOverLifetime; qc.enabled = true;
        qc.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 1f, 0.8f, 0f));

        // Anh chop trang xanh tu trong long loc (giu tu ban cu, doi len mieng va sang mau trang)
        var lightGo = new GameObject("StormLight");
        lightGo.transform.SetParent(hinh.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 13.5f * scale, 0f);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.80f, 0.88f, 1f);
        lt.intensity = 2.4f;
        lt.range = 18f * scale;
        lt.shadows = LightShadows.None;
        var fl = lightGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 2.4f; fl.amount = 0.6f; fl.speed = 14f; fl.rangeWobble = 0.12f;

        // ---- Bui xam nhat cuon o chan (anh mau: dam bui xam duoi chan, KHONG phai khoi den) ----
        var matBui = VatLieuLocXoay("BuiXam", "BuiXam", new Color(0.86f, 0.87f, 0.90f, 0.75f), false);
        var bui = NewPS("BuiChan", hinh.transform, new Vector3(0f, 0.3f, 0f), matBui, ParticleSystemRenderMode.Billboard);
        var tsa = bui.textureSheetAnimation;
        tsa.enabled = true; tsa.mode = ParticleSystemAnimationMode.Grid; tsa.numTilesX = 2; tsa.numTilesY = 2;
        tsa.animation = ParticleSystemAnimationType.WholeSheet; tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f); tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
        var bm = bui.main;
        bm.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 3.0f);
        bm.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
        bm.startSize = new ParticleSystem.MinMaxCurve(2.6f * scale, 5.5f * scale);
        bm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        bm.startColor = new ParticleSystem.MinMaxGradient(new Color(0.80f, 0.81f, 0.84f, 0.70f), new Color(0.95f, 0.95f, 0.97f, 0.90f));
        bm.simulationSpace = ParticleSystemSimulationSpace.World;
        bm.scalingMode = ParticleSystemScalingMode.Hierarchy;
        bm.maxParticles = 120;
        bm.gravityModifier = -0.04f;
        var be = bui.emission; be.rateOverTime = 40f;
        var bs = bui.shape; bs.shapeType = ParticleSystemShapeType.Circle; bs.radius = 2.0f * scale;
        var bv = bui.velocityOverLifetime;
        bv.enabled = true;
        bv.space = ParticleSystemSimulationSpace.Local;
        // Ca BA truc cung MOT kieu duong cong (hai hang so) - lech kieu la Unity bo qua ca mo-dun (hat cat Grit ban cu)
        bv.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        bv.y = new ParticleSystem.MinMaxCurve(0.5f, 1.6f);
        bv.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        bv.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        bv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 2.5f, ChieuQuyDaoGioLoc * 4.5f);
        bv.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
        bv.radial = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        var bc = bui.colorOverLifetime; bc.enabled = true;
        bc.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, new Color(0.85f, 0.86f, 0.9f), 1f, 0f, 0.85f, 0.55f, 0f));
        var bz = bui.sizeOverLifetime; bz.enabled = true;
        bz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 1.5f)));
        var br = bui.rotationOverLifetime; br.enabled = true; br.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);

        return root;
    }

    /// <summary>
    /// TIA SET tren than Loc xoay moi nhip (Tornado.Zap, 0,45 s): HAI tia kieu GIUT SET (loi trang + quang xanh, anh Blender)
    /// giang tu mieng loc xuong doc than, nhieu nhanh - nhu anh mau. Hai dau BAM theo con loc (loc chay 3,4 - 9,5 m/s).
    /// </summary>
    public static void TornadoBolt(Transform loc, float scale)
    {
        if (loc == null) return;
        Vector3 goc = loc.position;
        for (int i = 0; i < 2; i++)
        {
            float a1 = Random.Range(0f, Mathf.PI * 2f);
            // Tia xuong CHEO theo chieu cuon: goc lech 40-110 do
            float a2 = a1 + ChieuQuayGioLoc * -1f * Random.Range(0.7f, 1.9f);
            float h1 = Random.Range(12.5f, 14.8f) * scale;
            float h2 = Random.Range(2.5f, 8.5f) * scale;
            float r1 = BanKinhLocXoay(h1, scale) * Random.Range(0.55f, 0.95f);
            float r2 = BanKinhLocXoay(h2, scale) * Random.Range(0.95f, 1.12f);
            Vector3 p1 = goc + new Vector3(Mathf.Cos(a1) * r1, h1, Mathf.Sin(a1) * r1);
            Vector3 p2 = goc + new Vector3(Mathf.Cos(a2) * r2, h2, Mathf.Sin(a2) * r2);
            var arc = LightningArc.Create(p1, p2, 1f, Random.Range(0.28f, 0.42f));
            GiatSet.KieuTia(arc, 1.35f * scale, loc, loc);
            arc.branches = Random.Range(3, 6);      // nhieu nhanh chang chit nhu anh mau
            arc.branchLength = 0.42f;
        }
    }
}
