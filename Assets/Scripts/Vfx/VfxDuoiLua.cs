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
    static Material mLuaDuoiFlipbook, mVetLuaDai;

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
}
