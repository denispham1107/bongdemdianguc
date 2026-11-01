using UnityEngine;

/// <summary>
/// HINH VU NO CUA TANG BANG (nguoi dung 19/09/2026: Mua bang / Qua cau bang cap 5, tang bang het gio
/// thi no tung thay vi tan im lang - xem <see cref="TangBangNo"/>).
///
/// ⚠️ KHONG duoc dung lai VfxFactory.NoQuaCauBang cho vu no nay: ham ay goi IceImpact, tuc lai moc
/// them mot tang bang moi - tang bang no ra tang bang, khong bao gio dut.
///
/// Nen vu no dung rieng: manh bang vo tung (FrozenShatter) + bung suong lanh - thu da co trong VfxFactory, khong them tai nguyen.
///
/// ⚠️ 01/11/2026 (nguoi dung, anh: "vom trang duc qua xau; chi cho khoi suong lanh va cac manh bang vang ra"): BO qua cau chop "ChopLanh"
/// (hinh cau shader Ice phong toi 0,95 ban kinh = cai vom), vong lanh tren dat va ca den loe (den "DenNoBang" + "Flash" cua prefab vo bang).
/// Menu 156 do.
/// </summary>
public static partial class VfxFactory
{
    /// <summary>Tang bang vo tung: CHI manh bang bay ra + suong lanh bung len (khong vom, khong vong, khong den).</summary>
    public static GameObject NoTangBang(Vector3 cho, float banKinh)
    {
        // 1) Manh bang vo tung - dung chung hinh voi luc quai dang dong bang bi giet, nhung tat den "Flash"
        var vo = FrozenShatter(cho + Vector3.up * 0.35f, banKinh / 1.7f);
        float yDat = GroundY(cho);
        if (vo != null)
        {
            var flash = vo.transform.Find("Flash");
            if (flash != null) flash.gameObject.SetActive(false);
            var puff = vo.transform.Find("Puff");
            if (puff != null) MemSatDat(puff.GetComponent<ParticleSystem>(), yDat);
        }

        var goc = new GameObject("NoTangBang");
        goc.transform.position = cho;
        AutoDestroy.Add(goc, 2.2f);

        // 2) Suong bung len
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

        MemSatDat(suong, yDat);       // khong de tam suong cam xuyen dat thanh duong thang

        return goc;
    }
}
