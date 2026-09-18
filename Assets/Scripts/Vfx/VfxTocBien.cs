using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "TOC BIEN" (ky nang 15, them 18/09/2026).
///
/// Nguoi dung xin hai hieu ung rieng: BIEN MAT o cho dang dung, va HIEN RA o cho moi.
///   - Bien mat: nguoi tan thanh khoi tim bay len, mot vong sang thu vao giua.
///   - Hien ra : vong sang no ra tu mot diem, hat tim toe len va mot cot sang ngan.
/// Mau tim nhu he phep cua phu thuy (Tang hinh dung xanh lo, Loi dien tim - toc bien lay tim tham
/// hoi hong de khong lan voi hai cai kia).
///
/// Dung bang code (khong co tai nguyen Blender): hieu ung nay chi la hat + vong sang, dung chung
/// vat lieu co san cua VfxFactory.
/// </summary>
public static partial class VfxFactory
{
    static Material mHatTocBien, mVongTocBien;

    public static Material HatTocBienMat
    {
        get
        {
            if (mHatTocBien == null)
                mHatTocBien = Mats.Additive("P_HatTocBien", TextureFactory.SoftDot(2.2f),
                                            new Color(0.78f, 0.45f, 1f, 1f), 2.4f);
            return mHatTocBien;
        }
    }

    public static Material VongTocBienMat
    {
        get
        {
            if (mVongTocBien == null)
                mVongTocBien = Mats.Additive("P_VongTocBien", TextureFactory.Ring(0.18f),
                                             new Color(0.70f, 0.40f, 1f, 0.7f), 1.6f);
            return mVongTocBien;
        }
    }

    /// <summary>Nguoi tan di: khoi tim bay len, vong sang THU VAO giua.</summary>
    public static GameObject TocBienBienMat(Vector3 cho)
    {
        var goc = new GameObject("TocBienBienMat");
        goc.transform.position = cho;
        AutoDestroy.Add(goc, 2.2f);

        // Vong sang tren mat dat THU LAI - nguoc voi luc hien ra
        var vong = GroundRing.Create("VongThu", goc.transform, cho, new Material(VongTocBienMat), 1.6f);
        vong.xuyenVatCan = true;
        vong.startRadius = 1.6f;
        vong.endRadius = 0.15f;
        vong.duration = 0.35f;
        vong.segments = 26; vong.rings = 3;

        // Hat tim bay LEN doc than nguoi roi tat
        var ps = NewPS("KhoiTan", goc.transform, Vector3.up * 0.2f, HatTocBienMat,
                       ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.duration = 0.5f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 4.2f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.32f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = -0.35f;
        m.maxParticles = 90;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 40, 55) });
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 6f; sh.radius = 0.45f;
        sh.rotation = new Vector3(-90f, 0f, 0f);
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.92f, 1f), 0f,
            new Color(0.80f, 0.48f, 1f), 0.45f,
            new Color(0.42f, 0.12f, 0.85f), 1f,
            1f, 1f, 0.7f, 0f));

        ThemDenTim(goc, cho, 2.6f, 0.30f);
        return goc;
    }

    /// <summary>Nguoi hien ra: vong sang NO RA, hat toe len va mot cot sang ngan.</summary>
    public static GameObject TocBienHienRa(Vector3 cho)
    {
        var goc = new GameObject("TocBienHienRa");
        goc.transform.position = cho;
        AutoDestroy.Add(goc, 2.2f);

        var vong = GroundRing.Create("VongNo", goc.transform, cho, new Material(VongTocBienMat), 0.2f);
        vong.xuyenVatCan = true;
        vong.startRadius = 0.2f;
        vong.endRadius = 2.1f;
        vong.duration = 0.45f;
        vong.segments = 30; vong.rings = 3;

        // Cot sang ngan boc len tu mat dat
        var cot = ProcMesh.Part("CotSang", goc.transform, GroundDecal.QuadMesh(), new Material(HatTocBienMat),
                                Vector3.up * 1.1f, Quaternion.identity, new Vector3(1.5f, 2.4f, 1f), false);
        cot.AddComponent<Billboard>();
        var tan = cot.AddComponent<ExpandFade>();
        tan.duration = 0.4f;
        tan.startScale = new Vector3(0.5f, 2.6f, 1f);
        tan.endScale = new Vector3(2.2f, 1.2f, 1f);

        var ps = NewPS("HatHienRa", goc.transform, Vector3.up * 0.2f, HatTocBienMat,
                       ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.duration = 0.5f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(2.4f, 6f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.26f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = 0.5f;
        m.maxParticles = 90;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 36, 50) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Hemisphere; sh.radius = 0.4f;
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.95f, 1f), 0f,
            new Color(0.82f, 0.52f, 1f), 0.4f,
            new Color(0.48f, 0.15f, 0.9f), 1f,
            1f, 1f, 0.75f, 0f));

        ThemDenTim(goc, cho, 3.4f, 0.35f);
        return goc;
    }

    static void ThemDenTim(GameObject cha, Vector3 cho, float manh, float giay)
    {
        var denGo = new GameObject("DenTocBien");
        denGo.transform.SetParent(cha.transform, false);
        denGo.transform.localPosition = Vector3.up * 1.2f;
        var den = denGo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(0.72f, 0.42f, 1f);
        den.shadows = LightShadows.None;
        var burst = denGo.AddComponent<LightBurst>();
        burst.peak = manh; burst.duration = giay; burst.range = 7f;
    }
}
