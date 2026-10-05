using UnityEngine;

/// <summary>
/// VET LUA PHIA SAU QUA CAU LUA (ve lai 16/09/2026).
///
/// Nguoi dung: vet lua "dang co dang hinh tam giac khong co that lam". Nguyen nhan: he hat Flames cua
/// prefab Skill_QuaCauLua dung Tex_flame.png - mot HINH TAM GIAC mo; moi hat la mot tam giac dung yen
/// (billboard), xep noi nhau thanh vet rang cua.
///
/// Ban moi dung trong Blender qua MCP (CongCu/Blender/vet_lua_qua_cau_lua.blend):
///   - Flipbooks/LuaDuoi.png (4x4, 256 moi o): mot DAM LUA CUON tu luc bung sang toi luc vo vun thanh
///     than do - nhieu 4D chay theo thoi gian nen ben trong tung hat lua cung cuon;
///   - KyNang/QuaCauLua/VetLuaDai.png: vet lua dai lien mach (dau sang, than co van lua chay doc, duoi
///     thon do tham) - dung cho TrailRenderer, noi cac hat lai thanh mot duoi sao choi.
/// Sua luc chay tren ban sinh ra tu prefab (khong sua prefab: Flames con nam trong prefab va hai scene
/// tham chieu no). Qua cau cua quai Phu thuy (Fireball.Spawn) cung co vet moi.
/// </summary>
public static partial class VfxFactory
{
    static Material mLuaDuoiFlipbook, mVetLuaDai, mNgonLuaThat;

    /// <summary>
    /// NGON LUA THAT (nguoi dung 04/10/2026: tan lua cua qua cau lua "qua so sai, giong cac thanh nho mau lua chu khong phai lua that;
    /// dung MCP Blender dung lai cho that giong ngon lua that, khong duoc nhu hinh tam giac"; chon ap CA man chinh lan trong tran, va thay
    /// luon lua loi cua qua cau tren tay o man chinh). Blender MCP CongCu/Blender/ngon_lua_that.blend (scene NgonLuaThat): MO PHONG LUA
    /// MANTAFLOW (mien 0,8 x 0,8 x 2 m, do phan giai 96, nguon cau r ~0,19 o day, lua chay lau + xoay xoay manh -> ngon lua cao co loi
    /// vang sang o chan, dau ngon xe soi), vat lieu the tich phat sang theo truong "flame" (doc mau den -> do tham -> cam -> vang trang),
    /// render Cycles may quay truc giao khung DUNG 1:2 -> 16 khung lien tiep (khung 40..70 buoc 2) ghep KyNang/QuaCauLua/NgonLuaThat.png
    /// 4 x 4 (128 x 256 moi o, nen den - ve cong sang).
    /// </summary>
    static Material NgonLuaThatMat
    {
        get
        {
            // Kiem bang null cua Unity: vat lieu tao luc Play bi xoa khi thoat Play
            if (mNgonLuaThat == null)
            {
                var tex = Resources.Load<Texture2D>("KyNang/QuaCauLua/NgonLuaThat");
                if (tex != null) mNgonLuaThat = Mats.FlipbookAdd("P_NgonLuaThat", tex, Color.white, 0.9f);
            }
            return mNgonLuaThat;
        }
    }

    /// <summary>
    /// Doi mot he hat cua qua cau lua sang NGON LUA THAT: hat billboard DUNG (khong xoay, khong keo dai), o ti le 1:2 nhu khung anh,
    /// chay het 16 khung trong mot doi hat (lua liem, doi dang), boc len, lon dan roi tan. <paramref name="coMin"/>/<paramref name="coMax"/>
    /// la chieu CAO ngon lua (m); <paramref name="soMoiGiay"/> hat/giay.
    /// </summary>
    public static void DoiThanhNgonLuaThat(ParticleSystem ps, float coMin, float coMax, float soMoiGiay, float bocLen = 0.2f, float songMax = 0.45f)
    {
        var mat = NgonLuaThatMat;
        if (ps == null || mat == null) return;
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sharedMaterial = mat;
        // Goc hat o CHAN ngon lua (chan lua nam 11% tu day khung): tam hat giua tam anh thi nua duoi ngon lua chim vao loi sang cua
        // qua cau, chi con mot dom sang (anh menu 101c lan hai) - doi pivot len 0,38 chieu cao de ngon lua liem LEN tren mat qua cau
        r.pivot = new Vector3(0f, 0.38f, 0f);
        BatFlipbook(ps, 4, 4, 1);
        var m = ps.main;
        // Doi ngan + boc len nhe: lan dau (0,35-0,6 s, trong luc -0,35) ngon lua bay len thanh mot COT NGON NEN roi rac qua dau nhan
        // vat (anh menu 101c) - lua phai OM qua cau, ngon tach ra ngan roi tan
        m.startLifetime = new ParticleSystem.MinMaxCurve(songMax * 0.55f, songMax);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.25f);
        m.startSize3D = true;
        m.startSizeX = new ParticleSystem.MinMaxCurve(coMin * 0.5f, coMax * 0.5f);
        m.startSizeY = new ParticleSystem.MinMaxCurve(coMin, coMax);
        m.startSizeZ = 1f;
        m.startRotation = 0f;                     // ngon lua luon DUNG
        m.startColor = Color.white;
        m.gravityModifier = -bocLen;              // boc len
        m.maxParticles = Mathf.CeilToInt(soMoiGiay * 0.7f) + 4;
        var e = ps.emission; e.rateOverTime = soMoiGiay;
        var rot = ps.rotationOverLifetime; rot.enabled = false;
        var sol = ps.sizeOverLifetime; sol.enabled = true; sol.separateAxes = false;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.55f), new Keyframe(0.35f, 1f), new Keyframe(1f, 0.85f)));
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0.8f, 0.65f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);
        var nz = ps.noise; nz.enabled = true; nz.strength = 0.25f; nz.frequency = 1.2f;
    }

    /// <summary>Doi lop TAN LUA "Sparks" (vet keo dai - "thanh nho mau lua") cua qua cau lua sang ngon lua that. Moi qua cau lua (tran dau
    /// qua NangCapDuoiLua, man chinh qua TuTheTrungBay).</summary>
    public static void TanLuaThanhNgonLua(Transform qua, float radius)
    {
        var t = qua != null ? qua.Find("Sparks") : null;
        if (t != null) DoiThanhNgonLuaThat(t.GetComponent<ParticleSystem>(), radius * 1.2f, radius * 2.2f, 22f, 0.25f, 0.42f);
    }

    static Material LuaDuoiMat
    {
        get
        {
            if (mLuaDuoiFlipbook == null)
            {
                var tex = NapFlipbook("LuaDuoi");
                if (tex != null) mLuaDuoiFlipbook = Mats.FlipbookAdd("P_LuaDuoi", tex, Color.white, 0.75f);
            }
            return mLuaDuoiFlipbook;
        }
    }

    static Material VetLuaDaiMat
    {
        get
        {
            if (mVetLuaDai == null)
            {
                var tex = Resources.Load<Texture2D>("KyNang/QuaCauLua/VetLuaDai");
                if (tex != null) mVetLuaDai = Mats.Additive("P_VetLuaDai", tex, Color.white, 0.8f);
            }
            return mVetLuaDai;
        }
    }

    /// <summary>Thay vet lua tam giac bang flipbook lua cuon + vet lua dai. Goi ngay sau khi sinh qua cau.</summary>
    public static void NangCapDuoiLua(Transform qua, float radius)
    {
        if (qua == null) return;

        // 0) TAN LUA -> ngon lua that (04/10/2026)
        TanLuaThanhNgonLua(qua, radius);

        // 1) Hat LUA CUON (thay anh tam giac)
        var flames = qua.Find("Flames");
        var mat = LuaDuoiMat;
        if (flames != null && mat != null)
        {
            var ps = flames.GetComponent<ParticleSystem>();
            var r = flames.GetComponent<ParticleSystemRenderer>();
            if (ps != null && r != null)
            {
                r.renderMode = ParticleSystemRenderMode.Billboard;
                r.sharedMaterial = mat;
                BatFlipbook(ps, 4, 4, 1);

                var m = ps.main;
                m.startLifetime = new ParticleSystem.MinMaxCurve(0.30f, 0.55f);
                m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.9f);
                m.startSize = new ParticleSystem.MinMaxCurve(radius * 1.6f, radius * 2.7f);
                m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
                m.startColor = Color.white;
                var rot = ps.rotationOverLifetime; rot.enabled = true;
                rot.z = new ParticleSystem.MinMaxCurve(-2.2f, 2.2f);
                var sol = ps.sizeOverLifetime; sol.enabled = true;
                sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.4f, 1.1f), new Keyframe(1f, 1.5f)));
                // Mau da nam san trong flipbook (sang -> do tham): chi con mo dan o cuoi doi
                var col = ps.colorOverLifetime; col.enabled = true;
                var g = new Gradient();
                g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                          new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(0.85f, 0.6f), new GradientAlphaKey(0f, 1f) });
                col.color = new ParticleSystem.MinMaxGradient(g);
            }
        }

        // 2) VET LUA DAI lien mach
        var matVet = VetLuaDaiMat;
        if (matVet != null && qua.Find("VetLua") == null)
        {
            var vet = new GameObject("VetLua");
            vet.transform.SetParent(qua, false);
            var tr = vet.AddComponent<TrailRenderer>();
            tr.time = 0.30f;
            tr.minVertexDistance = 0.10f;
            tr.numCapVertices = 4;
            tr.textureMode = LineTextureMode.Stretch;
            tr.alignment = LineAlignment.View;
            // Do sang / be ngang ha sau anh chup dau: ba qua + bloom trum thanh mot khoi trang, khong doc ra van lua
            tr.widthCurve = new AnimationCurve(new Keyframe(0f, radius * 1.7f), new Keyframe(0.6f, radius * 0.9f), new Keyframe(1f, radius * 0.25f));
            tr.sharedMaterial = matVet;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            tr.colorGradient = g;
            tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tr.receiveShadows = false;
        }
    }

    /// <summary>
    /// Qua cau no / het tam: THA he hat duoi va vet lua ra de tan dan tai cho. Truoc day Destroy qua
    /// cau la ca dam lua duoi bien mat trong mot khung hinh.
    /// </summary>
    public static void ThaDuoiLua(Transform qua)
    {
        if (qua == null) return;
        for (int i = qua.childCount - 1; i >= 0; i--)
        {
            var c = qua.GetChild(i);
            var ps = c.GetComponent<ParticleSystem>();
            var tr = c.GetComponent<TrailRenderer>();
            if (ps == null && tr == null) continue;
            c.SetParent(null, true);
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (tr != null) tr.emitting = false;
            AutoDestroy.Add(c.gameObject, 1.2f);
        }
    }

    // ================================================================
    //  DIU SANG QUA CAU LUA + VU NO (nguoi dung 05/10/2026: "anh sang tu cac qua cau lua va khi va cham no qua sang, choi bi choi mat";
    //  chon ap MOI qua cau lua - nguoi choi, quai Phu thuy, Lua dia nguc; muc chon qua anh chup menu 108)
    // ================================================================

    /// <summary>He so do sang qua cau lua KHI BAY: 0,55 (nguoi dung chon "giam 45%" qua anh menu 108; do sang them vao canh x0,37).
    /// Bien tinh de menu 108 dat cac muc.</summary>
    public const float HeSoSangQuaCauLuaChon = 0.55f;
    public static float HeSoSangQuaCauLua = HeSoSangQuaCauLuaChon;
    /// <summary>He so do sang VU NO qua cau lua: 0,25 (nguoi dung chon "giam 75%"; 0,4 van trang chui - vo lua + hat bung bao hoa; do sang
    /// them x0,39, diem anh trang chui 1,21% -> 0,19%).</summary>
    public const float HeSoSangNoLuaChon = 0.25f;
    public static float HeSoSangNoLua = HeSoSangNoLuaChon;

    static MaterialPropertyBlock mpbDiu;
    static readonly int idCuongDo = Shader.PropertyToID("_Intensity");

    /// <summary>
    /// Nhan do sang moi lop PHAT SANG duoi <paramref name="goc"/> voi <paramref name="k"/>: vat lieu co _Intensity (Fire, ParticleAdditive,
    /// flipbook cong sang - qua MaterialPropertyBlock, KHONG sua vat lieu dung chung), den diem (LightFlicker.baseIntensity / LightBurst.peak /
    /// Light.intensity). Khoi (alpha, khong _Intensity) giu nguyen. Goi SAU khi da gan du lop (NangCapDuoiLua).
    /// </summary>
    public static void DiuSang(GameObject goc, float k)
    {
        if (goc == null || Mathf.Abs(k - 1f) < 0.001f) return;
        if (mpbDiu == null) mpbDiu = new MaterialPropertyBlock();
        foreach (var r in goc.GetComponentsInChildren<Renderer>(true))
        {
            var m = r.sharedMaterial;
            if (m == null || !m.HasProperty(idCuongDo)) continue;
            r.GetPropertyBlock(mpbDiu);
            mpbDiu.SetFloat(idCuongDo, m.GetFloat(idCuongDo) * k);
            r.SetPropertyBlock(mpbDiu);
        }
        foreach (var lf in goc.GetComponentsInChildren<LightFlicker>(true)) lf.baseIntensity *= k;
        foreach (var lb in goc.GetComponentsInChildren<LightBurst>(true)) lb.peak *= k;
        foreach (var lt in goc.GetComponentsInChildren<Light>(true))
            if (lt.GetComponent<LightFlicker>() == null && lt.GetComponent<LightBurst>() == null) lt.intensity *= k;
    }
}
