using UnityEngine;

/// <summary>
/// HINH VU NO CUA TANG BANG (nguoi dung 19/09/2026: Mua bang / Qua cau bang cap 5, tang bang het gio
/// thi no tung thay vi tan im lang - xem <see cref="TangBangNo"/>).
///
/// ⚠️ KHONG duoc dung lai VfxFactory.NoQuaCauBang cho vu no nay: ham ay goi IceImpact, tuc lai moc
/// them mot tang bang moi - tang bang no ra tang bang, khong bao gio dut.
///
/// Nen vu no dung rieng cac manh co san: manh bang vo tung (FrozenShatter), mot vong lanh lan ra tren
/// dat va mot bung suong - deu la thu da co trong VfxFactory, khong them tai nguyen nao.
/// </summary>
public static partial class VfxFactory
{
    /// <summary>Tang bang vo tung: manh bang bay ra, vong lanh lan tren dat, suong bung len.</summary>
    public static GameObject NoTangBang(Vector3 cho, float banKinh)
    {
        // 1) Manh bang vo tung - dung chung hinh voi luc quai dang dong bang bi giet
        FrozenShatter(cho + Vector3.up * 0.35f, banKinh / 1.7f);

        var goc = new GameObject("NoTangBang");
        goc.transform.position = cho;
        AutoDestroy.Add(goc, 2.2f);

        // 2) Chop lanh o tam
        var chop = ProcMesh.Part("ChopLanh", goc.transform, ProcMesh.Sphere(1f, 14, 10, 1f, Color.white),
                                 new Material(Mats.Ice), Vector3.up * 0.4f, Quaternion.identity, Vector3.one, false);
        var cf = chop.AddComponent<ExpandFade>();
        cf.duration = 0.3f;
        cf.startScale = Vector3.one * banKinh * 0.25f;
        cf.endScale = Vector3.one * banKinh * 0.95f;

        // 3) Vong lanh lan ra tren dat - cho nguoi choi thay dung tam voi cua vu no
        var vong = ProcMesh.Part("VongLanh", goc.transform, GroundDecal.QuadMesh(), new Material(RingIceMat),
                                 new Vector3(0f, 0.06f, 0f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, false);
        var vf = vong.AddComponent<ExpandFade>();
        vf.duration = 0.5f;
        vf.startScale = Vector3.one * banKinh * 0.4f;
        vf.endScale = Vector3.one * banKinh * 2.2f;

        // 4) Suong bung len
        var suong = NewPS("SuongNo", goc.transform, Vector3.up * 0.2f, SuongLanhMat,
                          ParticleSystemRenderMode.Billboard);
        var sm = suong.main;
        sm.loop = false; sm.duration = 0.2f;
        sm.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        sm.startSpeed = new ParticleSystem.MinMaxCurve(banKinh * 0.5f, banKinh * 1.3f);
        sm.startSize = new ParticleSystem.MinMaxCurve(banKinh * 0.6f, banKinh * 1.1f);
        sm.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        sm.simulationSpace = ParticleSystemSimulationSpace.World;
        sm.maxParticles = 36;
        var sem = suong.emission; sem.rateOverTime = 0f;
        sem.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });
        var ssh = suong.shape;
        ssh.shapeType = ParticleSystemShapeType.Hemisphere;
        ssh.radius = banKinh * 0.3f;
        ssh.rotation = new Vector3(-90f, 0f, 0f);

        // 5) Den lanh loe len mot nhip
        var denGo = new GameObject("DenNoBang");
        denGo.transform.SetParent(goc.transform, false);
        denGo.transform.localPosition = Vector3.up * 0.8f;
        var den = denGo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(0.55f, 0.82f, 1f);
        den.shadows = LightShadows.None;
        var loe = denGo.AddComponent<LightBurst>();
        loe.peak = 3.2f; loe.duration = 0.3f; loe.range = banKinh * 2.4f;

        return goc;
    }
}
