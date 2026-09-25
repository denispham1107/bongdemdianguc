using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HINH MAY GIONG (ky nang 21, nguoi dung 25/09/2026 kem anh mau). Moi anh dung bang BLENDER MCP
/// (CongCu/Blender/may_giong.blend -> Resources/KyNang/MayGiong/):
///   MayGiong.png   bon dam may bong 2x2 - loi trang xanh sang tu trong, vien xam, mep loi lom tung cum
///   ChopSet.png    loe sang cham dat hinh sao (loi trang + tia toa xanh)
///   ChayDen.png    than den lien mach + vet nut hong lua (lop phu ChayDenToanThan)
///   CotMay.png     bon COT KHOI 2x2 (moi o 512x1024) - may ru tu day may xuong TAN MAT DAT (nguoi dung 25/09/2026 gui
///                  anh thu hai: "may giong keo dai xuong duoi tan mat dat"; chon "mot cot khoi giua vung")
/// Tia set DUNG Y GIUT SET (nguoi dung 25/09/2026: "tia set giong nhu tia set trong skill Giut set"; chon "dung y"): cung
/// GiatSet.KieuTia, be ngang x1, loi trang + quang XANH DAM GiatSet.MauQuangNguoiChoi, hien GiatSet.GiayTiaHien 0,6 s, 2 nhanh.
/// Ban dau (truoc 25/09 chieu) tia day x1,7, quang xanh nhat mac dinh, song 0,30-0,42 s, 4-6 nhanh + loe hinh sao cham dat.
/// Khoi den tu ke bi chay dung anh bui Blender cua Loc xoay (BuiXam) to toi.
/// </summary>
public static partial class VfxFactory
{
    const string ThuMucMayGiong = "KyNang/MayGiong/";
    static readonly Dictionary<string, Material> matMayGiong = new Dictionary<string, Material>();

    // Kiem bang null cua Unity: vat lieu tao luc Play bi xoa khi thoat Play (memory vat-lieu-static-bi-xoa-khi-thoat-play)
    static Material VatLieuMayGiong(string ten, string thuMuc, string anh, Color mau, bool cong, float cuong = 1f)
    {
        Material m;
        if (matMayGiong.TryGetValue(ten, out m) && m != null) return m;
        var t = Resources.Load<Texture2D>(thuMuc + anh);
        m = cong ? Mats.Additive("P_MG_" + ten, t, mau, cuong) : Mats.Alpha("P_MG_" + ten, t, mau);
        matMayGiong[ten] = m;
        return m;
    }

    static void LuoiAnh2x2(ParticleSystem ps)
    {
        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true; tsa.mode = ParticleSystemAnimationMode.Grid; tsa.numTilesX = 2; tsa.numTilesY = 2;
        tsa.animation = ParticleSystemAnimationType.WholeSheet; tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f); tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
    }

    /// <summary>
    /// Dam MAY GIONG tren vung <paramref name="banKinh"/> m, day may o <paramref name="cao"/> m, song <paramref name="song"/> giay:
    /// tang may SANG (loi trang xanh) + tang may XAM ben duoi cho day, loe sang chop trong may, den nhap nhay. Tu tan.
    /// </summary>
    /// <summary>Be ngang cot khoi (m) - o anh rong 512 px, than cot chiem ~45% nen than thay ~2,3 m, mieng tren loe ~4 m.</summary>
    public const float BeNgangCotMay = 5f;
    /// <summary>So lop anh cot chong len nhau (moi lop mot o anh khac nhau).</summary>
    public const int SoLopCotMay = 3;

    public static GameObject MayGiongHinh(Vector3 chan, float banKinh, float cao, float song)
    {
        var root = new GameObject("MayGiongHinh");
        root.transform.position = chan;

        // ---- Tang may sang (tren) + tang may xam (duoi) ----
        for (int tang = 0; tang < 2; tang++)
        {
            bool sang = tang == 0;
            var mat = VatLieuMayGiong(sang ? "MaySang" : "MayXam", ThuMucMayGiong, "MayGiong",
                sang ? new Color(0.95f, 0.97f, 1f, 1f) : new Color(0.50f, 0.53f, 0.60f, 0.95f), false);
            var ps = NewPS(sang ? "MaySang" : "MayXam", root.transform, new Vector3(0f, cao + (sang ? 0.6f : -0.5f), 0f), mat,
                           ParticleSystemRenderMode.Billboard);
            LuoiAnh2x2(ps);
            var m = ps.main;
            m.duration = song; m.loop = false;
            m.startLifetime = song;
            m.startSpeed = 0f;
            m.startSize = sang ? new ParticleSystem.MinMaxCurve(4.2f, 7.5f) : new ParticleSystem.MinMaxCurve(5f, 8.5f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.maxParticles = 40;
            var em = ps.emission; em.rateOverTime = 0f;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(sang ? 26 : 16)) });
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = banKinh * (sang ? 0.85f : 0.95f);
            sh.rotation = new Vector3(90f, 0f, 0f);
            var col = ps.colorOverLifetime; col.enabled = true;
            float hien = 0.35f / song, tat = 1f - 0.6f / song;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, hien), new GradientAlphaKey(1f, tat), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);
            var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            var sz = ps.sizeOverLifetime; sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(0.1f, 1f), new Keyframe(1f, 1.12f)));
        }

        // ---- COT KHOI ru tu day may xuong tan mat dat (anh Blender CotMay 2x2, dung thang - VerticalBillboard) ----
        // Chan hat thap hon mat dat 0,35 m vi anh mo dan 10% duoi cung; dinh vao han trong may (day may o cao - 0,5).
        {
            var mat = VatLieuMayGiong("CotMay", ThuMucMayGiong, "CotMay", new Color(0.80f, 0.84f, 0.92f, 1f), false);
            float chanCot = -0.35f, dinhCot = cao + 1.2f, caoCot = dinhCot - chanCot;
            var ps = NewPS("CotMay", root.transform, new Vector3(0f, (chanCot + dinhCot) * 0.5f, 0f), mat,
                           ParticleSystemRenderMode.VerticalBillboard);
            LuoiAnh2x2(ps);
            var m = ps.main;
            m.duration = song; m.loop = false;
            m.startLifetime = song;
            m.startSpeed = 0f;
            m.startSize3D = true;
            m.startSizeX = new ParticleSystem.MinMaxCurve(BeNgangCotMay * 0.9f, BeNgangCotMay * 1.1f);
            m.startSizeY = caoCot; m.startSizeZ = 1f;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.maxParticles = 4;
            var em = ps.emission; em.rateOverTime = 0f;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)SoLopCotMay) });
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 0.35f; sh.rotation = new Vector3(90f, 0f, 0f);
            var col = ps.colorOverLifetime; col.enabled = true;
            float hien = 0.5f / song, tat = 1f - 0.7f / song;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, hien), new GradientAlphaKey(1f, tat), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            // Khoi cuon doc theo cot (anh may Blender, nho) - cho cot co chuyen dong, khong phai tam anh dung im
            var matK = VatLieuMayGiong("KhoiCot", ThuMucMayGiong, "MayGiong", new Color(0.70f, 0.74f, 0.82f, 0.75f), false);
            var k = NewPS("KhoiCot", root.transform, new Vector3(0f, cao * 0.5f, 0f), matK, ParticleSystemRenderMode.Billboard);
            LuoiAnh2x2(k);
            var km = k.main;
            km.duration = song - 0.4f; km.loop = false;
            km.startDelay = 0.3f;
            km.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.0f);
            km.startSpeed = 0f;
            km.startSize = new ParticleSystem.MinMaxCurve(1.6f, 3.0f);
            km.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            km.simulationSpace = ParticleSystemSimulationSpace.World;
            km.maxParticles = 40;
            var ke = k.emission; ke.rateOverTime = 20f;
            var ks = k.shape; ks.shapeType = ParticleSystemShapeType.Box; ks.scale = new Vector3(1.4f, cao, 1.4f);
            var kv = k.velocityOverLifetime; kv.enabled = true; kv.space = ParticleSystemSimulationSpace.World;
            kv.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f); kv.y = new ParticleSystem.MinMaxCurve(-0.6f, -0.2f);
            kv.z = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            var kc = k.colorOverLifetime; kc.enabled = true;
            kc.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 1f, 0.6f, 0f));
            var kr = k.rotationOverLifetime; kr.enabled = true; kr.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            var kz = k.sizeOverLifetime; kz.enabled = true;
            kz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(1f, 1.25f)));
        }

        // ---- Loe sang chop trong may (anh chop Blender, cong) ----
        var matLoe = VatLieuMayGiong("LoeMay", ThuMucMayGiong, "ChopSet", new Color(0.75f, 0.85f, 1f, 1f), true, 1.2f);
        var loe = NewPS("LoeTrongMay", root.transform, new Vector3(0f, cao, 0f), matLoe, ParticleSystemRenderMode.Billboard);
        var lm = loe.main;
        lm.duration = song; lm.loop = false;
        lm.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        lm.startSpeed = 0f;
        lm.startSize = new ParticleSystem.MinMaxCurve(3f, 6.5f);
        lm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        lm.simulationSpace = ParticleSystemSimulationSpace.World;
        lm.maxParticles = 20;
        var le = loe.emission; le.rateOverTime = 7f;
        var ls = loe.shape; ls.shapeType = ParticleSystemShapeType.Circle; ls.radius = banKinh * 0.7f; ls.rotation = new Vector3(90f, 0f, 0f);
        var lc = loe.colorOverLifetime; lc.enabled = true;
        lc.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 1f, 0.6f, 0f));

        // ---- Den trong may, nhap nhay ----
        var den = new GameObject("DenMay");
        den.transform.SetParent(root.transform, false);
        den.transform.localPosition = new Vector3(0f, cao - 0.5f, 0f);
        var lt = den.AddComponent<Light>();
        lt.type = LightType.Point; lt.color = new Color(0.72f, 0.82f, 1f); lt.intensity = 2.2f; lt.range = banKinh * 3f;
        lt.shadows = LightShadows.None;
        var fl = den.AddComponent<LightFlicker>();
        fl.baseIntensity = 2.2f; fl.amount = 0.8f; fl.speed = 16f; fl.rangeWobble = 0.15f;
        var tatDen = den.AddComponent<AutoDestroy>(); tatDen.lifetime = song - 0.2f;

        AutoDestroy.Add(root, song + 0.4f);
        return root;
    }

    /// <summary>
    /// Mot tia set May giong tu day may xuong dat - DUNG Y tia Giut set (cung ham, be ngang, mau, thoi gian song, so nhanh):
    /// giong <c>GiatSet.VeTia</c> voi day = 1. Chi them chop den cho mat dat sang len.
    /// </summary>
    public static LightningArc TiaMayGiong(Vector3 tu, Vector3 den, Transform bamCuoi)
    {
        Vector3 cham = den + Vector3.up * 0.15f;
        var arc = LightningArc.Create(tu, cham, 1f, GiatSet.GiayTiaHien);
        arc.name = "TiaMayGiong";
        arc.coreColor = Color.white;
        arc.glowColor = GiatSet.MauQuangNguoiChoi;
        GiatSet.KieuTia(arc, 1f, null, bamCuoi);

        var sang = new GameObject("ChopDenMayGiong");
        sang.transform.position = cham + Vector3.up * 1.2f;
        var lb = sang.AddComponent<LightBurst>(); lb.peak = 6f; lb.duration = 0.25f; lb.range = 9f;
        return arc;
    }

    /// <summary>Tia set NGANG lach tach trong long may (chi hinh) - anh mau co nhieu nhanh toa ngang trong may.</summary>
    public static void TiaNgangTrongMay(Vector3 tamMay, float banKinh)
    {
        Vector2 a = Random.insideUnitCircle * banKinh * 0.8f, b = Random.insideUnitCircle * banKinh * 0.8f;
        Vector3 p1 = tamMay + new Vector3(a.x, Random.Range(-0.4f, 0.6f), a.y);
        Vector3 p2 = tamMay + new Vector3(b.x, Random.Range(-0.4f, 0.6f), b.y);
        var arc = LightningArc.Create(p1, p2, 1f, Random.Range(0.18f, 0.3f));
        arc.name = "TiaNgangMayGiong";
        arc.coreColor = Color.white;
        arc.glowColor = GiatSet.MauQuangNguoiChoi;
        GiatSet.KieuTia(arc, 1f, null, null);
    }

    /// <summary>Khoi den boc len tu nguoi dang chay den (anh bui Blender BuiXam to toi), bam theo nguoi.</summary>
    public static GameObject KhoiChayDen(Transform nguoi)
    {
        var mat = VatLieuMayGiong("KhoiChayDen", "KyNang/LocXoay/", "BuiXam", new Color(0.10f, 0.09f, 0.08f, 0.8f), false);
        var ps = NewPS("KhoiChayDen", nguoi, new Vector3(0f, 1.0f, 0f), mat, ParticleSystemRenderMode.Billboard);
        LuoiAnh2x2(ps);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = -0.12f;
        m.maxParticles = 30;
        var em = ps.emission; em.rateOverTime = 14f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.45f;
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 0.9f, 0.5f, 0f));
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 1.8f)));
        return ps.gameObject;
    }
}
