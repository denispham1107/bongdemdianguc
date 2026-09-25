using UnityEngine;

/// <summary>
/// VONG PHEP khi HET TANG HINH (nguoi dung 26/09/2026, kem anh mau: vong phep xanh lam duoi chan - hai vong tron, sao nam canh,
/// ky hieu). Cung ngay nguoi dung BO 4 duoi mui ten va lop suong xanh trong vong: "chi hien thi noi bat cac vien sang, phan ben trong
/// khong can sang" - anh chi con NET sang, long vong den (nen cat nguong 0,13 cho quang toa sang loang cung mat). Anh dung bang BLENDER MCP
/// (CongCu/Blender/tang_hinh_vong_phep.blend -> Resources/KyNang/TangHinh/VongPhep.png): nen DEN, net + toa sang (compositor
/// Fog Glow) -> dung vat lieu CONG SANG, den = khong ve.
///
/// LUOI BAM DAT (khong dung hat Horizontal/VerticalBillboard - loai hat ay ve 0,7071 kich thuoc dat va con bi maxParticleSize
/// ep, memory maxparticlesize-ep-hat-to): o vuong 24 x 24, moi dinh ban tia CHI lop Ground xuong roi nang 6 cm. Ban dau la mot
/// tam PHANG o do cao chan nguoi: dat Act2 go ghe, cho dat cao hon CHE mat ca mang vong (anh goc choi 26/09/2026 mat gan het
/// nua gan). Hien nhanh 0,18 s, xoay cham, giu, tat dan toi 1,9 s - xoay / phong to lam tren TOA DO ANH (VongPhepSang doi uv),
/// luoi dung yen tren dat. Vong ngoai trong anh o ban kinh 0,40 cua o 1 x 1 -> canh = 2 x 0,5 x (banKinh / 0,40).
/// Kem den loe va hat sang (anh Blender ChopSet) bay len trong vong.
/// </summary>
public static partial class VfxFactory
{
    const string AnhVongPhep = "KyNang/TangHinh/VongPhep";
    /// <summary>Ban kinh vong ngoai trong anh (theo o 1 x 1).</summary>
    public const float BanKinhVongTrongAnh = 0.40f;
    static Texture2D anhVongPhep;

    public static int SoVongPhepDaBat;

    public static GameObject VongPhepTangHinh(Vector3 chan, float banKinh)
    {
        SoVongPhepDaBat++;
        if (anhVongPhep == null) anhVongPhep = Resources.Load<Texture2D>(AnhVongPhep);

        var root = new GameObject("VongPhepTangHinh");
        root.transform.position = chan;

        // ---- Vong phep: luoi bam mat dat ----
        var tam = new GameObject("VongPhep");
        tam.transform.SetParent(root.transform, false);
        tam.transform.localPosition = Vector3.zero;
        float canh = 2f * 0.5f * (banKinh / BanKinhVongTrongAnh);
        var mf = tam.AddComponent<MeshFilter>();
        mf.sharedMesh = LuoiBamDat(chan, canh);
        var mr = tam.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        var mat = Mats.Additive("P_VongPhepTangHinh", anhVongPhep, new Color(0.55f, 0.85f, 1f, 0f), 1.6f);
        mr.sharedMaterial = mat;
        var hoat = tam.AddComponent<VongPhepSang>();
        hoat.mat = mat; hoat.luoi = mf.sharedMesh;

        // ---- Hat sang bay len trong vong ----
        var matHat = VatLieuMayGiong("HatVongPhep", ThuMucMayGiong, "ChopSet", new Color(0.55f, 0.85f, 1f, 1f), true, 1.4f);
        var ps = NewPS("HatVongPhep", root.transform, new Vector3(0f, 0.2f, 0f), matHat, ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.duration = 0.6f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 60;
        var em = ps.emission; em.rateOverTime = 0f; em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)40) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = banKinh; sh.rotation = new Vector3(90f, 0f, 0f);
        var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f); vel.y = new ParticleSystem.MinMaxCurve(1.2f, 2.6f); vel.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 1f, 0.6f, 0f));

        // ---- Den loe xanh ----
        var den = new GameObject("DenVongPhep");
        den.transform.SetParent(root.transform, false);
        den.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        var lb = den.AddComponent<LightBurst>(); lb.peak = 7f; lb.duration = 0.6f; lb.range = banKinh * 2.2f;
        var lt = den.GetComponent<Light>(); if (lt != null) lt.color = new Color(0.55f, 0.85f, 1f);

        AutoDestroy.Add(root, VongPhepSang.TongGiay + 0.3f);
        return root;
    }

    /// <summary>So o moi canh cua luoi bam dat.</summary>
    public const int SoOVongPhep = 24;
    /// <summary>Nang luoi khoi mat dat bao nhieu (m).</summary>
    public const float NangVongPhep = 0.06f;

    /// <summary>
    /// Luoi vuong <paramref name="canh"/> x <paramref name="canh"/> quanh <paramref name="tam"/>, (SoOVongPhep+1)^2 dinh, moi dinh
    /// bam mat dat that (tia CHI lop Ground, GroundY gom ca mai nha / bia) + NangVongPhep. Toa do theo goc = tam. Luoi RIENG moi lan
    /// (dat moi cho mot khac) - VongPhepSang xoa khi tan.
    /// </summary>
    static Mesh LuoiBamDat(Vector3 tam, float canh)
    {
        int n = SoOVongPhep, m = n + 1;
        var v = new Vector3[m * m]; var uv = new Vector2[m * m];
        int lopDat = LayerMask.GetMask("Ground");
        for (int iz = 0; iz < m; iz++)
            for (int ix = 0; ix < m; ix++)
            {
                float u = ix / (float)n, w = iz / (float)n;
                float x = (u - 0.5f) * canh, z = (w - 0.5f) * canh;
                float y = 0f;
                Vector3 goc = tam + new Vector3(x, 4f, z);
                if (Physics.Raycast(goc, Vector3.down, out RaycastHit h, 12f, lopDat, QueryTriggerInteraction.Ignore)) y = h.point.y - tam.y;
                v[iz * m + ix] = new Vector3(x, y + NangVongPhep, z);
                uv[iz * m + ix] = new Vector2(u, w);
            }
        var tri = new int[n * n * 6]; int k = 0;
        for (int iz = 0; iz < n; iz++)
            for (int ix = 0; ix < n; ix++)
            {
                int a = iz * m + ix, b = a + 1, c2 = a + m, d = c2 + 1;
                tri[k++] = a; tri[k++] = c2; tri[k++] = b;
                tri[k++] = b; tri[k++] = c2; tri[k++] = d;
            }
        var l = new Mesh { name = "LuoiVongPhep" };
        l.vertices = v; l.uv = uv; l.triangles = tri;
        l.RecalculateNormals(); l.RecalculateBounds();
        l.MarkDynamic();
        return l;
    }
}
