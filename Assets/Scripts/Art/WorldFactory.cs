using UnityEngine;

/// <summary>
/// BAU TROI, SUONG MU VA ANH TRANG cua man choi (Nghia dia - Act2), cong mau dat cho Loc xoay.
///
/// Truoc 27/09/2026 lop nay con dung ca dau truong Act1 bang code (dat, co, da, cay, vach nui) va giu
/// hai bo mau troi Act1/Act2. Act1 da xoa han (nguoi dung) - chi con bo mau Act2.
/// GameBootstrap goi <see cref="BuildSkyAndFog"/> va <see cref="SetupMoonlight"/> moi lan vao tran;
/// <see cref="ChuyenChieuSangDem"/> doc chinh trang thai ay lam "ban dem".
/// </summary>
public static class WorldFactory
{
    // ================================================================
    //  BAU TROI, SUONG MU, ANH SANG MOI TRUONG
    // ================================================================

    public static void BuildSkyAndFog()
    {
        if (Mats.SkyShader != null)
        {
            var sky = new Material(Mats.SkyShader);
            sky.name = "Sky";

            // ACT2 - troi dem XANH LAM LANH, sang han len o chan troi.
            //
            // Ban truoc troi gan nhu den, ma anh sang moi truong duoi dat
            // lai nga NAU - ca man choi ra tong nau do am.
            sky.SetColor("_TopColor", new Color(0.055f, 0.085f, 0.110f));
            sky.SetColor("_HorizColor", new Color(0.150f, 0.205f, 0.235f));
            sky.SetColor("_BottomColor", new Color(0.040f, 0.055f, 0.065f));
            sky.SetColor("_CloudColor", new Color(0.180f, 0.235f, 0.265f));
            sky.SetFloat("_CloudAmount", 0.50f);

            // MAT TRANG. Huong phai NGUOC voi huong chieu cua Moonlight: den
            // chieu THEO forward cua no, con day la huong NHIN TOI mat trang.
            // Dat sai dau la trang moc sau lung nguoi choi trong khi bong do
            // lai nga ve phia truoc.
            sky.SetVector("_MoonDir", -MoonForward());
            sky.SetColor("_MoonColor", new Color(0.92f, 0.95f, 1.0f));
            sky.SetFloat("_MoonSize", 0.024f);
            sky.SetFloat("_MoonGlow", 0.30f);
            sky.SetFloat("_MoonStrength", 0.9f);

            sky.SetFloat("_Exponent", 1.5f);
            sky.SetFloat("_StarAmount", 1f);        // ban dem thi bat sao
            RenderSettings.skybox = sky;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        // Anh sang moi truong XANH LAM ca ba tang.
        //
        // Tang equator va ground truoc day nga nau - do la thu keo ca man
        // choi ve tong nau do, chu khong phai bau troi. Anh sang hat len tu
        // mat dat cham vao MOI vat nen no quyet dinh tong mau chung.
        RenderSettings.ambientSkyColor = new Color(0.120f, 0.205f, 0.265f);
        RenderSettings.ambientEquatorColor = new Color(0.085f, 0.155f, 0.190f);
        RenderSettings.ambientGroundColor = new Color(0.045f, 0.085f, 0.105f);

        // Suong xanh lam, sang hon va thua hon: van nhin ro hang cay o xa
        RenderSettings.fogColor = new Color(0.115f, 0.175f, 0.215f);
        RenderSettings.fogDensity = 0.0105f;
    }

    /// <summary>Goc chieu cua anh trang. De o MOT CHO de bau troi dat dia
    /// trang dung cho anh sang hat toi.</summary>
    public static readonly Vector3 MoonAngles = new Vector3(42f, 148f, 0f);

    /// <summary>Huong CHIEU cua anh trang (forward cua den).</summary>
    public static Vector4 MoonForward()
    {
        Vector3 f = Quaternion.Euler(MoonAngles) * Vector3.forward;
        return new Vector4(f.x, f.y, f.z, 0f);
    }

    /// <summary>Anh trang: nguon sang chinh cua man choi ban dem.</summary>
    public static void SetupMoonlight(Light moon)
    {
        if (moon == null) return;
        moon.type = LightType.Directional;
        moon.shadows = LightShadows.Soft;

        // THANH PHAN DO phai xuong THAP.
        //
        // Mat dat cua Act2 von la dat nau (do cao trong albedo). Anh sang
        // chi NHAN vao albedo, nen chi cang lam do nen bao nhieu thi dat van
        // cu nau bay nhieu - muon ca man choi nga xanh thi phai HA do trong
        // nguon sang xuong, chu khong phai nang xanh len.
        moon.color = new Color(0.52f, 0.72f, 0.98f);
        moon.intensity = 0.88f;
        moon.shadowStrength = 0.62f;
        moon.transform.rotation = Quaternion.Euler(MoonAngles);
        RenderSettings.sun = moon;
    }

    // ================================================================
    //  MAU DAT (manh vun Loc xoay cuon len)
    // ================================================================

    /// <summary>Do "mau mo" cua mot cho dat (1 = dat thit am; 0 = dat cat kho tro).</summary>
    static float GrassAmount(float px, float pz)
    {
        float patch = Mathf.PerlinNoise(px * 0.045f + 31f, pz * 0.045f + 17f);
        float detail = Mathf.PerlinNoise(px * 0.16f + 5f, pz * 0.16f + 23f);

        float k = patch * 0.75f + detail * 0.25f;

        // Keo manh ve hai dau: phan lon la co, chi nhung cho trung nhat moi tro dat
        return Mathf.Clamp01((k - 0.26f) * 6.5f);
    }

    /// <summary>
    /// Mau cua dat bi boc len tai mot cho.
    ///
    /// Dung chung mot cach tinh voi mau mat dat (xem <see cref="GrassAmount"/>),
    /// nen manh vun bi loc xoay cuon len luon co mau khop voi cho no vua di qua:
    /// qua bai cat kho thi bay ra dat nhat mau, qua vung dat thit thi bay ra dat sam.
    /// </summary>
    public static Color SoilColorAt(float px, float pz)
    {
        float g = GrassAmount(px, pz);

        // Toi theo mat dat (da nhan 0.6) de manh vun khong sang hon cho no bi boc len
        var mud = new Color(0.240f, 0.198f, 0.150f);   // dat cat kho, nhat hon
        var sod = new Color(0.138f, 0.114f, 0.078f);   // dat thit am, sam mau

        var c = Color.Lerp(mud, sod, g);

        // Cho dam cho nhat mot chut cho khoi deu tam tap
        float n = Mathf.PerlinNoise(px * 0.32f + 12f, pz * 0.32f + 4f);
        float k = Mathf.Lerp(0.82f, 1.16f, n);
        return new Color(c.r * k, c.g * k, c.b * k, 1f);
    }
}
