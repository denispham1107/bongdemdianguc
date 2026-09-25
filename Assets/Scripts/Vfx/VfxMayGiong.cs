using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HINH MAY GIONG (ky nang 21, nguoi dung 25/09/2026 kem anh mau). Moi anh dung bang BLENDER MCP
/// (CongCu/Blender/may_giong.blend -> Resources/KyNang/MayGiong/):
///   MayGiong.png   bon dam may bong 2x2 - loi trang xanh sang tu trong, vien xam, mep loi lom tung cum
///   ChopSet.png    loe sang cham dat hinh sao (loi trang + tia toa xanh)
///   ChayDen.png    than den lien mach + vet nut hong lua (lop phu ChayDenToanThan)
/// Tia set dung lai anh Blender cua Giut set (LightningArc.anhBlender qua GiatSet.KieuTia) - trang xanh, re nhanh nhu anh mau.
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

    /// <summary>Mot tia set May giong tu day may xuong dat: kieu Giut set (anh Blender) nhieu nhanh + loe hinh sao cham dat.</summary>
    public static LightningArc TiaMayGiong(Vector3 tu, Vector3 den, Transform bamCuoi)
    {
        Vector3 cham = den + Vector3.up * 0.15f;
        var arc = LightningArc.Create(tu, cham, 1f, Random.Range(0.30f, 0.42f));
        arc.name = "TiaMayGiong";
        GiatSet.KieuTia(arc, 1.7f, null, bamCuoi);
        arc.branches = Random.Range(4, 7);
        arc.branchLength = 0.45f;

        // Loe cham dat (anh chop Blender) + chop den
        var matChop = VatLieuMayGiong("ChopDat", ThuMucMayGiong, "ChopSet", new Color(0.8f, 0.88f, 1f, 1f), true, 1.4f);
        var chop = NewPS("ChopDatMayGiong", null, cham, matChop, ParticleSystemRenderMode.Billboard);
        var cm = chop.main;
        cm.duration = 0.3f; cm.loop = false;
        cm.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.28f);
        cm.startSpeed = 0f;
        cm.startSize = new ParticleSystem.MinMaxCurve(3.2f, 4.4f);
        cm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        cm.simulationSpace = ParticleSystemSimulationSpace.World;
        var ce = chop.emission; ce.rateOverTime = 0f; ce.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)2) });
        var cc = chop.colorOverLifetime; cc.enabled = true;
        cc.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 1f, 1f, 0.5f, 0f));
        AutoDestroy.Add(chop.gameObject, 0.6f);

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
        GiatSet.KieuTia(arc, 0.9f, null, null);
        arc.branches = Random.Range(2, 4);
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
