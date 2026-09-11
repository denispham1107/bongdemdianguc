using UnityEngine;

/// <summary>
/// LUA CHO LO DA o man hinh chinh - dung tu MO PHONG LUA THAT trong Blender.
///
/// Lan 1: hat lua dung mot anh tinh -> nhin ra tung tam giac.
/// Lan 2: flipbook "luoi lua" sinh bang nhieu Perlin, moi hat mot luoi lua bay
///        len roi tan - nguoi dung van thay tam giac va "mieng lua lo lung giua
///        troi": luoi lua tach khoi goc bay len la dung cai loi ay.
/// Lan 3 (nay): mo phong Mantaflow (lua + khoi) dung trong Blender cua nguoi
///        dung qua Blender MCP, render thanh HAI luoi anh dong lap lien mach:
///          - <c>Flipbooks/LuaLo</c>  : lua (phat sang, cong sang)
///          - <c>Flipbooks/KhoiDen</c>: khoi (che phu, tron alpha)
///        Moi luoi 8 cot x 6 hang = 48 khung, o 160x320, chay 20 khung/giay.
///
/// KHONG CON HAT BAY: moi lop la MOT "tam" dung yen cam vao mieng chau, chay
/// vong lap anh. Ngon lua trong anh da co san ca vong doi (le len, xoan, tan
/// thanh khoi) nen khong can hat nao bay len nua. Hai tam lua lech pha 24 khung
/// va mot tam lat ngang - khong ai nhin ra vong lap 2,4 giay.
///
/// Kich thuoc: trong mo phong, luong than ban kinh 0,36 m, mien rong 1,2 m cao
/// 2,6 m, day luong than o z = 0,14. Ti le sang the gioi = banKinhChau / 0,36.
/// </summary>
public static partial class VfxFactory
{
    const float SoKhungGiay = 20f;
    const int CotLuoi = 8, HangLuoi = 6;
    // Kich thuoc mien mo phong (m) va do cao day luong than trong mien
    const float MienRong = 1.2f, MienCao = 2.6f, DayThan = 0.14f, BanKinhThan = 0.36f;
    // Bu cho VerticalBillboard (Unity ve tu giac 0,707 lan kich thuoc dat)
    const float BuDungDung = 1.41421356f;

    static Material mLuaLo, mKhoiDen;

    public static Material LuaLoMat
    {
        get
        {
            if (mLuaLo == null)
            {
                var tex = NapFlipbook("LuaLo");
                if (tex == null) return null;
                // Hai tam lua cong sang chong len nhau + bloom: moi tam 0,6 la vua,
                // 1,0 thi loi lua chay trang
                mLuaLo = Mats.FlipbookAdd("P_LuaLo", tex, Color.white, 0.6f);
            }
            return mLuaLo;
        }
    }

    public static Material KhoiDenMat
    {
        get
        {
            if (mKhoiDen == null)
            {
                var tex = NapFlipbook("KhoiDen");
                if (tex == null) return null;
                // Mau da nuong san trong anh: chan khoi hat anh cam cua lua, len cao
                // den kit (~0,05 - toi hon troi dem ~0,15, khong thi khoi tan vao troi)
                mKhoiDen = Mats.FlipbookAlpha("P_KhoiDen", tex, new Color(1f, 1f, 1f, 0.95f));
            }
            return mKhoiDen;
        }
    }

    /// <summary>
    /// Mot "tam" flipbook dung yen: dung MOT hat song mai, chay vong lap anh
    /// theo so khung moi giay. Dung he hat (khong phai quad thuong) de dung lai
    /// shader flipbook tron hai khung - shader can luong AnimBlend cua he hat.
    /// </summary>
    static ParticleSystem TamFlipbook(string ten, Transform cha, Vector3 viTri, Material mat,
                                      float rong, float cao, float khungDau, bool latNgang, float sapXep)
    {
        var ps = NewPS(ten, cha, viTri, mat, ParticleSystemRenderMode.VerticalBillboard);
        var m = ps.main;
        m.loop = true;
        m.duration = 1f;
        m.startLifetime = 1e6f;
        m.startSpeed = 0f;
        m.startSize3D = true;
        // VerticalBillboard cua Unity tu THU tu giac con 1/can2 (0,707) kich thuoc
        // dat - do bang BakeMesh: dat cao 3,033 m ra 2,145 m, o moi khoang cach
        // camera; Billboard thuong thi ra dung 3,033. Tam co quanh TAM cua no nen
        // day lua bi keo len 0,4 m khoi mieng chau = "lua lo lung". Bu lai can2.
        m.startSizeX = rong * BuDungDung; m.startSizeY = cao * BuDungDung; m.startSizeZ = 1f;
        m.simulationSpace = ParticleSystemSimulationSpace.Local;
        m.maxParticles = 1;
        m.playOnAwake = true;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var sh = ps.shape; sh.enabled = false;

        BatFlipbook(ps, CotLuoi, HangLuoi, 1);
        var tsa = ps.textureSheetAnimation;
        tsa.timeMode = ParticleSystemAnimationTimeMode.FPS;
        tsa.fps = SoKhungGiay;
        tsa.startFrame = khungDau;

        var r = ps.GetComponent<ParticleSystemRenderer>();
        if (latNgang) r.flip = new Vector3(1f, 0f, 0f);
        r.sortingFudge = sapXep;
        // Mac dinh Unity kep hat <= 0,5 man hinh: tam cao 3 m nhin gan (camera
        // chup can lo) se bi thu nho. Mo tran de tam giu dung kich thuoc.
        r.maxParticleSize = 100f;
        r.minParticleSize = 0f;
        return ps;
    }

    /// <summary>
    /// Dung lua cho lo da. <paramref name="banKinh"/> la ban kinh long chau (m).
    /// <paramref name="localPos"/> la mieng chau - day luong than.
    /// </summary>
    public static GameObject LuaLoDa(Transform parent, Vector3 localPos, float banKinh)
    {
        var go = new GameObject("LuaLoDa");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        // 0,82: ti le 1:1 cho lua cao ~1,4 m tren lo cao 1,3 m va day cot khoi den
        // ra ngoai mep tren khung hinh man chinh
        float tiLe = banKinh / BanKinhThan * 0.82f;
        float rong = MienRong * tiLe, cao = MienCao * tiLe;
        // Tam o giua mien; day luong than trung mieng chau
        var giua = new Vector3(0f, (MienCao * 0.5f - DayThan) * tiLe, 0f);

        var matLua = LuaLoMat;
        var matKhoi = KhoiDenMat;
        if (matLua != null && matKhoi != null)
        {
            // Khoi ve TRUOC (sau lung), lua cong sang de len
            TamFlipbook("KhoiDen", go.transform, giua + new Vector3(0f, 0f, 0.02f), matKhoi, rong, cao, 7f, false, 5f);
            TamFlipbook("LuaA", go.transform, giua, matLua, rong, cao, 0f, false, -5f);
            TamFlipbook("LuaB", go.transform, giua + new Vector3(0.03f * tiLe, -0.02f * tiLe, 0f), matLua, rong * 0.92f, cao * 0.92f, 24f, true, -6f);
        }
        else
        {
            Debug.LogWarning("[LuaLoDa] thieu flipbook LuaLo / KhoiDen - dung lua cu");
            Brazier(go.transform.position, go.transform, banKinh * 2f);
        }

        // Tan lua nho bay lang xang - thu duy nhat con la hat bay
        var tan = NewPS("TanLua", go.transform, new Vector3(0f, banKinh * 0.3f, 0f), EmberMat, ParticleSystemRenderMode.Billboard);
        var mt = tan.main;
        mt.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
        mt.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.5f);
        mt.startSize = new ParticleSystem.MinMaxCurve(0.016f, 0.045f);
        mt.simulationSpace = ParticleSystemSimulationSpace.World;
        mt.gravityModifier = -0.2f;
        mt.maxParticles = 30;
        var emt = tan.emission; emt.rateOverTime = 6f;
        var sht = tan.shape; sht.shapeType = ParticleSystemShapeType.Cone;
        sht.angle = 18f; sht.radius = banKinh * 0.5f; sht.rotation = new Vector3(-90f, 0f, 0f);
        var nzt = tan.noise; nzt.enabled = true; nzt.strength = 0.9f; nzt.frequency = 0.6f;
        var colt = tan.colorOverLifetime; colt.enabled = true;
        colt.color = new ParticleSystem.MinMaxGradient(Grad(
            new Color(1f, 0.85f, 0.5f), 0f, new Color(1f, 0.45f, 0.1f), 0.5f, new Color(0.6f, 0.1f, 0.02f), 1f,
            0f, 1f, 0.8f, 0f));

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
