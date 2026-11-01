using UnityEngine;

/// <summary>Dung hinh cu luot Bo xuong (nguoi dung 01/11/2026) - xem <see cref="HinhLuotBoXuong"/>, <see cref="EnemyAI"/> (EnemyAILuot.cs).</summary>
public static partial class VfxFactory
{
    static Material mVetHon;

    /// <summary>Vat lieu vet hon xanh: anh Blender MCP KyNang/BoXuong/VetHon (cong sang). Thieu anh thi dung vet mem co san.</summary>
    static Material VetHonMat
    {
        get
        {
            if (mVetHon == null)
            {
                var anh = Resources.Load<Texture2D>("KyNang/BoXuong/VetHon");
                if (anh != null) anh.wrapMode = TextureWrapMode.Clamp;     // vet keo gian 1 lan, khong lap
                mVetHon = Mats.Additive("P_VetHon", anh != null ? anh : TextureFactory.Smoke(), Color.white, 2.1f);
            }
            return mVetHon;
        }
    }

    /// <summary>Bat hinh luot cho mot con Bo xuong: bong mo + vet hon + bui dat. Goi Dung() khi het cu luot (khong goi thi tu tat).</summary>
    public static HinhLuotBoXuong LuotBoXuong(Transform quai)
    {
        if (quai == null) return null;
        TiengViTri.BoXuongLao(quai);

        var bui = NewPS("BuiLuot", null, Vector3.zero, BuiXamMat, ParticleSystemRenderMode.Billboard);
        DatKhungBuiXam(bui);
        var m = bui.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.1f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.5f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startColor = new ParticleSystem.MinMaxGradient(MauBuiXamToi, MauBuiXamSang);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 40;
        var e = bui.emission; e.rateOverTime = 0f; e.rateOverDistance = 3.5f;
        var sh = bui.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 0.35f;
        sh.rotation = new Vector3(-90f, 0f, 0f);

        var mauVet = Grad(new Color(0.55f, 0.85f, 1f), 0f, new Color(0.25f, 0.55f, 1f), 0.5f, new Color(0.10f, 0.25f, 0.8f), 1f, 1f, 1f, 0.6f, 0f);
        return HinhLuotBoXuong.Tao(quai, VetHonMat, mauVet, bui);
    }
}
