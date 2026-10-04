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

    // Duong vien vo chinh Vo2: r = 2,6 + 4,2 * t^1,6 (29/09/2026 nguoi dung: "than duoi nhin nhu cay kem oc que" - chan x2, than to
    // dan deu, mieng 6,8 giu nguyen; Blender MCP CongCu/Blender/loc_xoay_than_rong.blend, ca 5 luoi nhan cung k(z) = r_moi / r_cu).
    // Truoc: r = 1,3 + 5,5 * t^1,9 (loc_xoay.blend). Qui dao ke bi cuon (Tornado.FunnelRadiusAt) + tia set doc than theo cong thuc nay.
    const float ChanLocXoay = 2.6f, LoeLocXoay = 4.2f, MuLocXoay = 1.6f;

    /// <summary>Vong phun bui xam o chan Loc xoay (m, nhan scale). 29/09/2026 rong theo chan moi: 2,0 -> 4,0.</summary>
    public const float BanKinhVongBuiLocXoay = 4.0f;

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
                // Mau goc x HeSoToiLocXoay (29/09/2026 toi di 20%). ⚠️ Hinh trong game lay tu PREFAB: vat lieu M_P_LX_Vo0-3/Vanh da nhan cung he so
                case "Vo0":  anh = "GioVo0";  mau = ToiLocXoay(new Color(0.86f, 0.88f, 0.92f, 1.00f)); quay = 210f; truot = TruotLenLocXoay[0]; break;
                case "Vo1":  anh = "GioVo1";  mau = ToiLocXoay(new Color(0.92f, 0.94f, 0.97f, 1.00f)); quay = 165f; truot = TruotLenLocXoay[1]; break;
                case "Vo2":  anh = "GioVo2";  mau = ToiLocXoay(new Color(0.97f, 0.98f, 1.00f, 1.00f)); quay = 130f; truot = TruotLenLocXoay[2]; break;
                case "Vo3":  anh = "GioVo3";  mau = ToiLocXoay(new Color(1.00f, 1.00f, 1.00f, 0.90f)); quay = 100f; truot = TruotLenLocXoay[3]; break;
                case "Vanh": anh = "GioVanh"; mau = ToiLocXoay(new Color(1.00f, 1.00f, 1.00f, 0.95f)); quay = 120f; truot = 0f; break;   // vanh: ong vong, khong truot
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
        BuiXamChanLoc(hinh.transform, "BuiChan", new Vector3(0f, 0.3f, 0f), scale, BanKinhVongBuiLocXoay * scale, 1f, false);

        return root;
    }

    /// <summary>
    /// TIA SET cua Loc xoay moi nhip (Tornado.Zap, 0,45 s) - Gio loc (Loc xoay thu nho) dung chung voi scale 0,318.
    /// 01/10/2026 nguoi dung: "cho cac tia set trong Loc xoay va Gio loc giong nhu tia set trong Sam set"; chon "tu mieng loc xuong dat"
    /// (khong vet chay xem - nguoi dung da xoa dau vet tren dat), Gio loc thu tia theo loc. Nay HAI tia KIEU SAM SET (LightningStrike.Strike):
    /// LightningArc MAC DINH (khong anh Blender, mau mac dinh), 20 doan, 2-3 nhanh, song 0,30 s, be ngang x scale; tu MIENG loc giang xuong
    /// cham DAT canh chan loc (lech goc theo chieu cuon) va loe sang cham dat (Vfx_SetChamDat ban kinh 2,1 x scale). Ca tia lan loe
    /// BAM THEO loc (Gio loc bay 9,5 m/s). Truoc: kieu Giut set (anh Blender, 3-5 nhanh) giang doc than - git 7c45d73.
    /// </summary>
    public static void TornadoBolt(Transform loc, float scale, string ten = null, bool coLoe = true)
    {
        if (loc == null) return;
        Vector3 goc = loc.position;
        for (int i = 0; i < 2; i++)
        {
            float a1 = Random.Range(0f, Mathf.PI * 2f);
            float h1 = Random.Range(12.5f, 14.8f) * scale;
            float r1 = BanKinhLocXoay(h1, scale) * Random.Range(0.55f, 0.95f);
            // Cham dat canh chan loc: lech goc theo chieu cuon, 0,6 - 1,15 ban kinh chan
            float a2 = a1 + ChieuQuayGioLoc * -1f * Random.Range(0.3f, 1.0f);
            float r2 = BanKinhLocXoay(0f, scale) * Random.Range(0.6f, 1.15f);
            Vector3 p1 = goc + new Vector3(Mathf.Cos(a1) * r1, h1, Mathf.Sin(a1) * r1);
            Vector3 p2 = goc + new Vector3(Mathf.Cos(a2) * r2, 0f, Mathf.Sin(a2) * r2);
            p2.y = GioLoc.MatDatY(p2, goc.y);
            var arc = LightningArc.Create(p1, p2, scale, 0.30f);
            if (ten != null) arc.name = ten;
            arc.segments = 20;
            arc.branches = Random.Range(2, 4);
            arc.BamTheo(loc);
            if (!coLoe) continue;                       // Gio loc: chi tia, khong loe (nguoi dung 01/10/2026)
            var loe = LoeSetChamDat(p2, LightningStrikeBanKinh * scale);
            if (loe != null)
            {
                DiTheo.Gan(loe, loc);       // chay theo loc, KHONG lam con (Hoa loc xoay / thu nho se phong ca loe)
                if (scale < 0.95f) ThuLoeSet(loe, scale);
            }
            // 05/10/2026 nguoi dung: Loc xoay cung mau "A + vet nut cua B" nhu Sam set / May giong - tat vet gach Sparks / Jet,
            // TIA DIEN BO TREN DAT bam theo loc (loc di 3,4-9,5 m/s, tia dung yen thi tut lai sau 1-3 m) + VET NUT dung yen tren dat
            TatVetGach(loe);
            TiaBoDat.Tao(p2, LightningStrikeBanKinh * scale, loc);
            VetNutSet.Tao(p2, LightningStrikeBanKinh * scale);
        }
    }

    /// <summary>Ban kinh loe cham dat cua tia Sam set (LightningStrike.impactRadius mac dinh).</summary>
    public const float LightningStrikeBanKinh = 2.1f;

    /// <summary>Loe cham dat thu nho (Gio loc): he hat prefab ti le Local khong an ti le cha -> dat Hierarchy; tam den x scale.</summary>
    static void ThuLoeSet(GameObject loe, float scale)
    {
        foreach (var ps in loe.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            m.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
        foreach (var lt in loe.GetComponentsInChildren<Light>(true)) lt.range *= scale;
    }

    /// <summary>Vat lieu bui xam chan loc (anh BuiXam 2x2 cua Loc xoay) - Gio loc dung chung tu 28/09/2026.</summary>
    public static Material BuiXamMat
    {
        get { return VatLieuLocXoay("BuiXam", "BuiXam", new Color(0.86f, 0.87f, 0.90f, 0.75f), false); }
    }

    /// <summary>LOC XOAY DEN HON (nguoi dung 29/09/2026: "cho den hon 1 chut ca 2 skill"; menu 93 chup toi di 10/20/30%, chon 20%):
    /// mau vo + mau bui x 0,8. Khong dong vat lieu bui dung chung BuiXamMat (Gio loc cung dung) - chi mau hat.</summary>
    public const float HeSoToiLocXoay = 0.8f;

    static Color ToiLocXoay(Color c) { return new Color(c.r * HeSoToiLocXoay, c.g * HeSoToiLocXoay, c.b * HeSoToiLocXoay, c.a); }

    /// <summary>Mau bui xam cua Loc xoay (goc 0,80 - 0,95 x HeSoToiLocXoay). Gio loc tu 28/09 ghi de bang mau may giong cua no.</summary>
    public static readonly Color MauBuiXamToi = ToiLocXoay(new Color(0.80f, 0.81f, 0.84f, 0.70f)), MauBuiXamSang = ToiLocXoay(new Color(0.95f, 0.95f, 0.97f, 0.90f));

    /// <summary>So hat/giay cua lop bui CUON LEN TAN DINH (29/09/2026, nguoi dung chon): Loc xoay 80 (+ 40 bui chan = 120),
    /// Gio loc 40 (+ 80 bui chan = 120 moi con).</summary>
    // 01/10/2026 nguoi dung: Loc xoay "bui khoi nhieu hon va bay cuon len tan dinh" - chon lop len dinh x2: 80 -> 160 (Gio loc moi = Loc
    // xoay thu nho nen theo luon; TocBuiCuonLenGioLoc chi con cho hinh Gio loc CU).
    public const float TocBuiCuonLenLocXoay = 160f, TocBuiCuonLenGioLoc = 40f;

    /// <summary>
    /// BUI CUON LEN TAN DINH LOC, OM THEO THAN (nguoi dung 29/09/2026: Loc xoay "bui khoi cuon len day dac hon nua len tan dinh";
    /// Gio loc "bui chi den tam nua than la het"). Bui chan cu song 1,6-3 s, bay len 0,5-1,6 m/s nen chi toi ~1-4 m (Loc xoay cao
    /// 15,7) va ~2,5 m (Gio loc cao 5). Lop nay: sinh tren VONG o chan (ban kinh = than), bay len DEU toi dinh trong mot doi hat
    /// (van toc = cao / doi trung binh), va dat ra ngoai theo DUNG duong cong ban kinh than (van toc toa = dr/dt tinh tu banKinh)
    /// nen vong bui no rong theo than, loe ra o mieng; xoay cung chieu cuon. Khong gian CUC BO: bui di theo con loc khi no chay.
    /// ⚠️ Moi truc van toc cung MOT kieu duong cong (Curve) - lech kieu la Unity bo qua ca mo-dun.
    /// </summary>
    public static ParticleSystem BuiCuonLenTheoThan(Transform cha, string ten, float cao, System.Func<float, float> banKinh, float tocPhun,
                                                     float songMin, float songMax, float coMin, float coMax, float quay, Color mauToi, Color mauSang)
    {
        var bui = NewPS(ten, cha, Vector3.zero, BuiXamMat, ParticleSystemRenderMode.Billboard);
        DatKhungBuiXam(bui);
        float songTB = 0.5f * (songMin + songMax);
        var bm = bui.main;
        bm.startLifetime = new ParticleSystem.MinMaxCurve(songMin, songMax);
        bm.startSpeed = 0f;
        bm.startSize = new ParticleSystem.MinMaxCurve(coMin, coMax);
        bm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        bm.startColor = new ParticleSystem.MinMaxGradient(mauToi, mauSang);
        bm.simulationSpace = ParticleSystemSimulationSpace.Local;
        bm.scalingMode = ParticleSystemScalingMode.Hierarchy;
        bm.maxParticles = Mathf.CeilToInt(tocPhun * songMax * 1.1f);
        bm.gravityModifier = 0f;
        var be = bui.emission; be.rateOverTime = tocPhun;
        var bs = bui.shape; bs.shapeType = ParticleSystemShapeType.Circle; bs.radius = banKinh(0f);
        bs.radiusThickness = 0f;                                   // chi mep vong = sat than
        bs.rotation = new Vector3(-90f, 0f, 0f);                   // Circle mac dinh nam trong mat XY
        var bv = bui.velocityOverLifetime;
        bv.enabled = true;
        bv.space = ParticleSystemSimulationSpace.Local;
        var phang = AnimationCurve.Constant(0f, 1f, 1f);
        var khong = AnimationCurve.Constant(0f, 1f, 0f);
        bv.x = new ParticleSystem.MinMaxCurve(1f, khong);
        bv.y = new ParticleSystem.MinMaxCurve(cao / songTB, phang);
        bv.z = new ParticleSystem.MinMaxCurve(1f, khong);
        bv.orbitalX = new ParticleSystem.MinMaxCurve(1f, khong);
        bv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * quay, phang);
        bv.orbitalZ = new ParticleSystem.MinMaxCurve(1f, khong);
        // ⚠️ Van toc TOA (radial) cua Unity tinh theo huong 3 CHIEU tu tam: hat len cao thi huong ay gan nhu thang dung, day hat vot
        // QUA DINH (menu 82 do lan dau: cao nhat 19,6 m tren than 15). Doi tam (orbitalOffset) len theo do cao cua hat -> toa NAM NGANG.
        bv.orbitalOffsetX = new ParticleSystem.MinMaxCurve(1f, khong);
        bv.orbitalOffsetY = new ParticleSystem.MinMaxCurve(cao, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        bv.orbitalOffsetZ = new ParticleSystem.MinMaxCurve(1f, khong);
        // van toc toa: dr/dt = (dr/dh) * (dh/dt), lay mau 11 moc theo doi hat
        var toa = new AnimationCurve();
        for (int i = 0; i <= 10; i++)
        {
            float tau = i / 10f, e = 0.02f;
            float h0 = cao * Mathf.Max(0f, tau - e), h1 = cao * Mathf.Min(1f, tau + e);
            toa.AddKey(tau, (banKinh(h1) - banKinh(h0)) / ((h1 - h0) / cao * songTB));
        }
        bv.radial = new ParticleSystem.MinMaxCurve(1f, toa);
        var bc = bui.colorOverLifetime; bc.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.88f, 0.89f, 0.93f), 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0.85f, 0.80f), new GradientAlphaKey(0f, 1f) });
        bc.color = new ParticleSystem.MinMaxGradient(g);
        var bz = bui.sizeOverLifetime; bz.enabled = true;
        bz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(1f, 1.4f)));
        var br = bui.rotationOverLifetime; br.enabled = true; br.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
        return bui;
    }

    /// <summary>Lop bui cuon len cua LOC XOAY: om vo chinh (0,95 x BanKinhLocXoay) tu chan len 15 m. Goi tu Tornado.Start - hinh
    /// Loc xoay trong game lay tu PREFAB nuong san (khong chay BuildLocXoay), nen phai gan luc chay; da co thi bo qua.</summary>
    public static void DamBaoBuiCuonLenLocXoay(Transform loc, float scale)
    {
        if (loc == null) return;
        Transform hinh = null;
        ParticleSystem buiChan = null;
        bool coLen = false, coDuoi = false;
        foreach (var t in loc.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "BuiCuonLen") coLen = true;
            if (t.name == "BuiThanDuoi") coDuoi = true;
            if (t.name == "LocXoayHinh") hinh = t;
            if (t.name == "BuiChan") buiChan = t.GetComponent<ParticleSystem>();
        }
        if (hinh == null) return;                                   // hinh cu (BuildTornadoCu) - khong co than Blender
        HatDenCuonLen(hinh, scale);
        if (!coLen)
            BuiCuonLenTheoThan(hinh, "BuiCuonLen", CaoThanLocXoay * scale, h => 0.95f * BanKinhLocXoay(h, scale), TocBuiCuonLenLocXoay,
                               3.0f, 3.6f, 2.2f * scale, 4.2f * scale, 2.1f, MauBuiXamToi, MauBuiXamSang);
        if (coDuoi) return;
        // THAN DUOI DAY DAC HON (nguoi dung 29/09/2026 khoanh vung than duoi tren anh, chon: them lop khoi than duoi + bui chan x2,
        // chi tang so hat - do duc giu): lop BuiThanDuoi cung kieu om than nhung chi bay toi NUA THAN (7,5 m), cham hon (~2,8 m/s)
        BuiCuonLenTheoThan(hinh, "BuiThanDuoi", 0.5f * CaoThanLocXoay * scale, h => 0.95f * BanKinhLocXoay(h, scale), TocBuiThanDuoiLocXoay,
                           2.4f, 3.0f, 2.2f * scale, 4.2f * scale, 2.1f, MauBuiXamToi, MauBuiXamSang);
        // Bui chan x2 (40 -> 80 hat/giay, tran 120 -> 240). Dat o day chu khong trong BuildLocXoay: hinh trong game lay tu PREFAB
        // (so 40 nuong san) - mot cho phu ca prefab lan hinh dung bang code. BuildLocXoay / prefab van ghi 40 (goc cua ham chung).
        if (buiChan != null)
        {
            var m = buiChan.main; m.maxParticles = Mathf.RoundToInt(120 * HeSoBuiChanLocXoay);
            m.startColor = new ParticleSystem.MinMaxGradient(MauBuiXamToi, MauBuiXamSang);   // prefab nuong mau cu (chua toi 20%)
            var e = buiChan.emission; e.rateOverTime = 40f * HeSoBuiChanLocXoay;
        }
    }

    /// <summary>Hat DAT VUN den (trong / ngoai than) va CUM BUI DEN (trong / ngoai than): hat moi giay moi lop. Nguoi dung 04/10/2026
    /// chon "vua phai": tong 120 hat/giay (80 dat vun + 40 cum bui), Gio loc cung so (thu theo co).</summary>
    public const float TocHatDenLocXoay = 40f, TocCumBuiDenLocXoay = 20f;

    /// <summary>Ban kinh lop TRONG / NGOAI so voi vo chinh (BanKinhLocXoay).</summary>
    public const float BanKinhTrongHatDen = 0.55f, BanKinhNgoaiHatDen = 1.18f;

    /// <summary>
    /// HAT BUI DEN BI CUON LEN (nguoi dung 04/10/2026: "cho them cac hat bui mau den bi cuon tu duoi len ben trong va ca ben ngoai tu duoi
    /// day len tan dinh cua loc" - Loc xoay + Gio loc; chon "ca hai loai", "vua phai", Gio loc "nhu Loc xoay, thu theo co"). Bon lop
    /// (dung lai quy dao BuiCuonLenTheoThan: sinh o chan, bay len deu toi dinh, dat ra theo dung cong thuc ban kinh than, quay cung chieu):
    ///   - HatDenTrong / HatDenNgoai: DAT VUN, SOI den (anh Blender MCP CongCu/Blender/gio_loc_xoan.blend scene HatDenAnh -> HatDen.png 2x2:
    ///     vien soi, cuc dat, manh dai, chum hat li ti), lon nhao nhanh, quay nhanh hon bui.
    ///   - DenCuonTrong / DenCuonNgoai: CUM BUI DEN mem (flipbook BuiDenCuon 6x6 - anh Blender MCP cua Gio loc cu, GIU LAI khi xoa hinh cu).
    /// Trong = 0,55 vo chinh (sinh trai 0,36-0,55 - lo qua vo trong suot), ngoai = 1,18. Goi trong DamBaoBuiCuonLenLocXoay -> Loc xoay
    /// (Tornado.Start) va Gio loc (BuildGioLoc, roi phong x0,318 theo Hierarchy) deu co; da co thi bo qua.
    /// </summary>
    static void HatDenCuonLen(Transform hinh, float scale)
    {
        if (hinh.Find("HatDenTrong") != null) return;
        float cao = CaoThanLocXoay * scale;
        var matHat = VatLieuLocXoay("HatDen", "HatDen", Color.white, false);
        Color h0 = new Color(0.10f, 0.09f, 0.08f, 1f), h1 = new Color(0.22f, 0.20f, 0.18f, 1f);
        foreach (var lop in new[] { ("HatDenTrong", BanKinhTrongHatDen, 0.4f), ("HatDenNgoai", BanKinhNgoaiHatDen, 0f) })
        {
            float f = lop.Item2;
            var ps = BuiCuonLenTheoThan(hinh, lop.Item1, cao, h => f * BanKinhLocXoay(h, scale), TocHatDenLocXoay,
                                        2.8f, 3.6f, 0.12f * scale, 0.40f * scale, 2.6f, h0, h1);
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = matHat;
            var sh = ps.shape; sh.radiusThickness = lop.Item3;
            var ro = ps.rotationOverLifetime; ro.z = new ParticleSystem.MinMaxCurve(-4f, 4f);   // lon nhao
            var sz = ps.sizeOverLifetime; sz.enabled = false;                                  // hat cung, khong no
        }
        var matBui = BuiDenCuonMat;
        if (matBui == null) return;
        Color b0 = new Color(0.08f, 0.08f, 0.09f, 0.55f), b1 = new Color(0.16f, 0.15f, 0.15f, 0.75f);
        foreach (var lop in new[] { ("DenCuonTrong", BanKinhTrongHatDen, 0.4f), ("DenCuonNgoai", BanKinhNgoaiHatDen, 0f) })
        {
            float f = lop.Item2;
            var ps = BuiCuonLenTheoThan(hinh, lop.Item1, cao, h => f * BanKinhLocXoay(h, scale), TocCumBuiDenLocXoay,
                                        3.0f, 3.8f, 1.0f * scale, 2.4f * scale, 2.1f, b0, b1);
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = matBui;
            BatFlipbook(ps, 6, 6, 1);
            var sh = ps.shape; sh.radiusThickness = lop.Item3;
        }
    }

    /// <summary>Lop khoi THAN DUOI Loc xoay (hat/giay) va he so bui chan (29/09/2026): tong bui 80 chan + 160 cuon len (01/10/2026, truoc 80) + 80 than duoi = 320.</summary>
    public const float TocBuiThanDuoiLocXoay = 80f, HeSoBuiChanLocXoay = 2f;

    /// <summary>Dat 4 dam bui ngau nhien cua anh BuiXam 2x2, khong chay khung (moi hat mot dam).</summary>
    public static void DatKhungBuiXam(ParticleSystem ps)
    {
        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true; tsa.mode = ParticleSystemAnimationMode.Grid; tsa.numTilesX = 2; tsa.numTilesY = 2;
        tsa.animation = ParticleSystemAnimationType.WholeSheet; tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f); tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
    }

    /// <summary>
    /// BUI XAM CUON O CHAN LOC - kieu cua Loc xoay (anh mau: dam bui xam duoi chan). Tach ra 28/09/2026 de GIO LOC dung chung
    /// (nguoi dung: khoi bui den cua Gio loc "cho cung mau cung hieu ung giong skill Loc xoay").
    /// <paramref name="coHat"/> nhan co hat, <paramref name="banKinhVong"/> vong phun, <paramref name="heSoBay"/> nhan van toc bay len
    /// (Gio loc thap 5 m so voi 15,7 m). <paramref name="cucBo"/>: Loc xoay di cham 3,4 m/s nen bui o khong gian THE GIOI; Gio loc
    /// bay 9,5 m/s - bui the gioi se rot lai thanh vet dai sau lung, nen de CUC BO cho bui om chan (vet phia sau da co KhoiBui).
    /// </summary>
    public static ParticleSystem BuiXamChanLoc(Transform cha, string ten, Vector3 viTri, float coHat, float banKinhVong, float heSoBay, bool cucBo)
    {
        var bui = NewPS(ten, cha, viTri, BuiXamMat, ParticleSystemRenderMode.Billboard);
        DatKhungBuiXam(bui);
        var bm = bui.main;
        bm.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 3.0f);
        bm.startSpeed = new ParticleSystem.MinMaxCurve(0.4f * heSoBay, 1.4f * heSoBay);
        bm.startSize = new ParticleSystem.MinMaxCurve(2.6f * coHat, 5.5f * coHat);
        bm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        bm.startColor = new ParticleSystem.MinMaxGradient(MauBuiXamToi, MauBuiXamSang);
        bm.simulationSpace = cucBo ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
        bm.scalingMode = ParticleSystemScalingMode.Hierarchy;
        bm.maxParticles = 120;
        bm.gravityModifier = -0.04f;
        var be = bui.emission; be.rateOverTime = 40f;
        var bs = bui.shape; bs.shapeType = ParticleSystemShapeType.Circle; bs.radius = banKinhVong;
        // Circle mac dinh DUNG trong mat XY -> xoay nam phang tren dat. 29/09/2026: truoc chi xoay khi cucBo - bui chan LOC XOAY (the
        // gioi) phun tren vong DUNG, do duoc hat sinh tu -3,97 den +3,61 m (nua so hat chui duoi dat); nay nam phang ca hai.
        bs.rotation = new Vector3(-90f, 0f, 0f); bs.radiusThickness = 1f;
        var bv = bui.velocityOverLifetime;
        bv.enabled = true;
        bv.space = ParticleSystemSimulationSpace.Local;
        // Ca BA truc cung MOT kieu duong cong (hai hang so) - lech kieu la Unity bo qua ca mo-dun (hat cat Grit ban cu)
        bv.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        bv.y = new ParticleSystem.MinMaxCurve(0.5f * heSoBay, 1.6f * heSoBay);
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
        return bui;
    }
}
