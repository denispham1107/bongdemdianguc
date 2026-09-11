using UnityEngine;

/// <summary>
/// LUA CHO LO DA o man hinh chinh.
///
/// Lua cu cua lo sat la hat dung mot anh TINH (TextureFactory.FlameLick) - moi
/// hat chi to len, nho di, mo di - nen dam lua nhin ra tung manh tam giac
/// (nguoi dung bao dung vay). Lua moi co nam lop:
///
///   1. LUOI LUA - flipbook <c>Flipbooks/LuaNgon</c> (8x8): moi o la mot thoi
///      diem trong doi MOT luoi lua (nhen -> vuon cao lac lu -> ngon dut ra ->
///      tan). Moi hat song tron doi mot luoi, nen ben trong tung hat ngon lua
///      co dong chay that. Bat dau o khung dau (khong ngau nhien nhu khoi) vi
///      day la mot vong doi, khong phai mot vong lap.
///   2. QUANG SANG o mieng chau - lam loi lua sang ruc, noi cac luoi voi nhau.
///   3. KHOI xam bay len tren ngon lua.
///   4. TAN LUA nho bay lang xang.
///   5. DEN chap chon.
///
/// Anh LuaNgon sinh bang nhieu Perlin cuon (xem HUONG-DAN, muc lo lua da),
/// mau theo nhiet do tung diem - to tint TRANG, khong nhuom lai.
/// </summary>
public static partial class VfxFactory
{
    static Material mLuaNgon;

    public static Material LuaNgonMat
    {
        get
        {
            if (mLuaNgon == null)
            {
                var tex = NapFlipbook("LuaNgon");
                if (tex == null) return null;
                mLuaNgon = Mats.FlipbookAdd("P_LuaNgon", tex, new Color(1f, 0.84f, 0.68f, 1f), 0.9f);
            }
            return mLuaNgon;
        }
    }

    /// <summary>
    /// Dung lua cho lo da. <paramref name="banKinh"/> la ban kinh mieng chau
    /// (m) - lua choan het long chau.
    /// </summary>
    public static GameObject LuaLoDa(Transform parent, Vector3 localPos, float banKinh)
    {
        var go = new GameObject("LuaLoDa");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        // ---- 1. Luoi lua ----
        var mat = LuaNgonMat ?? FlameMat;
        var luoi = NewPS("LuoiLua", go.transform, Vector3.zero, mat, ParticleSystemRenderMode.VerticalBillboard);
        var m = luoi.main;
        // Song ngan va boc cham: luoi lua phai TAN ngay trong long ngon lua.
        // Lan dau (song 1,35 s, boc 0,45 m/s) luoi lua tach ra bay lo lung
        // tren mieng lo ~0,5 m - nhin nhu mieng lua treo giua troi.
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
        m.startSize = new ParticleSystem.MinMaxCurve(banKinh * 1.8f, banKinh * 2.7f);
        m.startRotation = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);   // radian, ~ +-10 do
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.gravityModifier = -0.06f;
        m.maxParticles = 60;
        // 26 hat moi giay cong sang chong len nhau thanh mot khoi trang loa
        var em = luoi.emission; em.rateOverTime = 18f;
        var sh = luoi.shape; sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 6f; sh.radius = banKinh * 0.62f; sh.rotation = new Vector3(-90f, 0f, 0f);
        var sol = luoi.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.75f, 1.05f, 0.9f));
        var nz = luoi.noise; nz.enabled = true; nz.strength = 0.18f; nz.frequency = 0.9f; nz.scrollSpeed = 0.6f;
        if (mat != FlameMat)
        {
            BatFlipbook(luoi, 8, 8, 1);
            var tsa = luoi.textureSheetAnimation;
            tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 3f);   // vong doi, bat dau tu dau
        }
        // Tam hat o GIUA anh, ma ngon lua nam o nua duoi o anh: nang hat len
        // de chan ngon lua trung mieng chau
        var r = luoi.GetComponent<ParticleSystemRenderer>();
        r.pivot = new Vector3(0f, 0.32f, 0f);
        // Lat ngang mot nua so luoi lua - khong thi cung mot dang luoi lap lai
        r.flip = new Vector3(0.5f, 0f, 0f);

        // ---- 2. Quang sang loi lua ----
        var loi = NewPS("QuangLua", go.transform, new Vector3(0f, banKinh * 0.25f, 0f), GlowMat, ParticleSystemRenderMode.Billboard);
        var ml = loi.main;
        ml.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        ml.startSpeed = 0f;
        ml.startSize = new ParticleSystem.MinMaxCurve(banKinh * 2.0f, banKinh * 2.6f);
        ml.startColor = new Color(1f, 0.42f, 0.10f, 0.14f);
        ml.simulationSpace = ParticleSystemSimulationSpace.Local;
        ml.maxParticles = 12;
        var eml = loi.emission; eml.rateOverTime = 14f;
        var shl = loi.shape; shl.enabled = false;
        var coll = loi.colorOverLifetime; coll.enabled = true;
        coll.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 1f, 1f, 0f));

        // ---- 3. Khoi ----
        var matKhoi = KhoiCuonMat ?? SmokeMat;
        var khoi = NewPS("KhoiLua", go.transform, new Vector3(0f, banKinh * 1.4f, 0f), matKhoi, ParticleSystemRenderMode.Billboard);
        var mk = khoi.main;
        mk.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 3.6f);
        mk.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        mk.startSize = new ParticleSystem.MinMaxCurve(banKinh * 1.0f, banKinh * 1.6f);
        mk.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        mk.startColor = new Color(0.10f, 0.085f, 0.08f, 0.55f);
        mk.simulationSpace = ParticleSystemSimulationSpace.World;
        mk.gravityModifier = -0.06f;
        mk.maxParticles = 30;
        var emk = khoi.emission; emk.rateOverTime = 5f;
        var shk = khoi.shape; shk.shapeType = ParticleSystemShapeType.Cone;
        shk.angle = 10f; shk.radius = banKinh * 0.4f; shk.rotation = new Vector3(-90f, 0f, 0f);
        var solk = khoi.sizeOverLifetime; solk.enabled = true;
        solk.size = new ParticleSystem.MinMaxCurve(1f, Curve(0.6f, 1.4f, 2.2f));
        var colk = khoi.colorOverLifetime; colk.enabled = true;
        colk.color = new ParticleSystem.MinMaxGradient(Grad(
            Color.white, 0f, Color.white, 0.5f, Color.white, 1f, 0f, 0.8f, 0.45f, 0f));
        var nzk = khoi.noise; nzk.enabled = true; nzk.strength = 0.3f; nzk.frequency = 0.4f;
        if (matKhoi != SmokeMat) BatFlipbook(khoi, 6, 6, 1);

        // ---- 4. Tan lua ----
        var tan = NewPS("TanLua", go.transform, new Vector3(0f, banKinh * 0.3f, 0f), EmberMat, ParticleSystemRenderMode.Billboard);
        var mt = tan.main;
        mt.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
        mt.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 1.8f);
        mt.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.05f);
        mt.simulationSpace = ParticleSystemSimulationSpace.World;
        mt.gravityModifier = -0.22f;
        mt.maxParticles = 40;
        var emt = tan.emission; emt.rateOverTime = 8f;
        var sht = tan.shape; sht.shapeType = ParticleSystemShapeType.Cone;
        sht.angle = 20f; sht.radius = banKinh * 0.5f; sht.rotation = new Vector3(-90f, 0f, 0f);
        var nzt = tan.noise; nzt.enabled = true; nzt.strength = 1.0f; nzt.frequency = 0.6f;
        var colt = tan.colorOverLifetime; colt.enabled = true;
        colt.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.85f, 0.5f), 0f, new Color(1f, 0.45f, 0.1f), 0.5f, new Color(0.6f, 0.1f, 0.02f), 1f,
            0f, 1f, 0.8f, 0f));

        // ---- 5. Den ----
        var denGo = new GameObject("DenLua");
        denGo.transform.SetParent(go.transform, false);
        denGo.transform.localPosition = new Vector3(0f, banKinh * 1.1f, 0f);
        var den = denGo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(1f, 0.50f, 0.17f);
        den.intensity = 3.0f;
        den.range = 9f;
        den.shadows = LightShadows.None;
        var fl = denGo.AddComponent<LightFlicker>();
        fl.baseIntensity = 3.0f; fl.amount = 0.25f; fl.speed = 8f; fl.rangeWobble = 0.08f;

        return go;
    }
}
