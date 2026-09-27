using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LUA CHAY TOAN THAN - hinh cua trang thai bong chay (<see cref="BurningEffect"/>) cho moi nguoi choi va moi quai.
///
/// Lan 1 (28/09/2026, nguoi dung: "chi vang ra cac dom chay nho qua don so"): nhieu ngon lua nho phat theo tung khuc xuong.
/// Lan 2 (28/09/2026, nguoi dung, anh chup): "cho lua to thanh 1 CUM DUY NHAT toan than, khong phai tung dong lua nho";
/// "di chuyen thi lua bi day lui ve phia sau, khong co dinh tren nguoi"; "het thoi gian thi lua bien mat LUON, khong can mo dan".
///
/// Nay:
///   - Anh lua Blender MCP (CongCu/Blender/lua_chay_nguoi.blend, canh LuaToanThan -> Resources/Flipbooks/LuaToanThan.png):
///     MOT KHOI LUA cao bang ca nguoi (khung 1:2, 4x4 = 16 khung LAP VONG lien mach - nhieu tron hai nua chu ky): day lom
///     chom, thon o chan, phinh o than, tren dau tach thanh luoi lua cuon len (ban dau day thang + hai mep thang dung
///     trong nhu khung cua - bo).
///   - Vai hat LON (moi hat la ca khoi lua, chay vong 14 khung/giay tu khung ngau nhien) chong mo vao nhau -> mot cum lua
///     song dong. Hai lop: lop TRUOC lech ve phia may quay (lua trum len than, khong bi than che mat), lop SAU lech ra sau.
///   - MO PHONG CUC BO (Local) va goc hinh DI THEO THAN moi khung (tam = xuong Hips, day = xuong thap nhat): nhan vat chay
///     thi ca khoi lua di theo, khong con vet lua o lai phia sau (ban truoc mo phong THE GIOI).
///   - Het chay / bi go (Toc bien, Tang hinh, chet): BurningEffect xoa ngay - lua bien mat luon.
/// Kich thuoc theo chieu cao than do tu XUONG (rig.bodyHeight sai: bo xuong 2,36 m trong khi hinh cao 1,67). Vat khong co
/// xuong (bia thu) thi dung h, r truyen vao.
/// </summary>
public class LuaToanThan : MonoBehaviour
{
    /// <summary>So khoi lua sinh moi giay / tuoi moi khoi: luon co ~4 khoi chong mo vao nhau.</summary>
    const float KhoiMoiGiay = 2.4f;
    const float KhungMoiGiay = 14f;
    /// <summary>Lua cao hon dinh dau bao nhieu (m, cho than 1,7 m).</summary>
    const float LuaVuotDau = 0.60f;
    /// <summary>Trong anh, lua thay ro (sang > 40/255) tu 9% den 87% chieu cao khung (do tren 16 khung).</summary>
    const float PhanCaoCoLua = 0.78f;
    /// <summary>Tam khung cao hon chan lua: 0,5 - 0,09.</summary>
    public const float TamTrenChanLua = 0.41f;
    const float DenManh = 2.4f;
    /// <summary>Be ngang khung anh / chieu cao (anh ve 1:2; nong ra de trum ca tay dang, vu khi cam tay).</summary>
    const float BeNgang = 0.62f;

    /// <summary>Phep thu (menu 90) bat de lam DOI CHUNG: mo phong THE GIOI nhu ban cu - lua bi bo lai phia sau khi chay.</summary>
    public static bool DoiChungTheGioi;

    Transform muc, hong, dauDinh;
    readonly List<Transform> xuongDuoi = new List<Transform>();
    float cao = 1.7f;
    float hDuPhong = 1.7f;

    ParticleSystem truoc, sau, quang, tan, khoi;
    Light den;

    public float ChieuCaoThan { get { return cao; } }
    public bool CoXuong { get { return hong != null; } }
    public ParticleSystem LopTruoc { get { return truoc; } }
    public ParticleSystem LopSau { get { return sau; } }
    /// <summary>Kich thuoc khoi lua dang dat (rong, cao) - phep thu doc.</summary>
    public Vector2 CoKhoi { get; private set; }

    static Material mLua;
    /// <summary>Vat lieu khoi lua Blender (cong sang, giu nguyen mau trong anh). Null neu thieu anh.</summary>
    public static Material LuaMat
    {
        get
        {
            if (mLua == null)   // kiem bang null cua Unity: thoat Play la vat lieu tao luc chay bi xoa
            {
                var tex = VfxFactory.NapFlipbook("LuaToanThan");
                if (tex == null) return null;
                mLua = Mats.FlipbookAdd("P_LuaToanThan", tex, Color.white, 0.80f);
            }
            return mLua;
        }
    }

    /// <summary>Dung lua toan than cho <paramref name="muc"/>. h, r: chieu cao / ban kinh uoc luong (khi khong co xuong).</summary>
    public static LuaToanThan Dung(Transform muc, float h, float r)
    {
        var go = new GameObject("Burning");
        go.transform.SetParent(muc, false);
        go.transform.localPosition = Vector3.zero;
        var l = go.AddComponent<LuaToanThan>();
        l.muc = muc;
        l.hDuPhong = Mathf.Max(0.5f, h);
        l.TimXuong();
        l.DungHat();
        l.BamThan();
        return l;
    }

    void TimXuong()
    {
        var ten = new Dictionary<string, Transform>();
        var smr = muc.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null)
            foreach (var x in smr.bones) if (x != null && !ten.ContainsKey(x.name)) ten[x.name] = x;

        ten.TryGetValue("Hips", out hong);
        ten.TryGetValue("head_end", out dauDinh);
        foreach (var n in new[] { "LeftFoot", "RightFoot", "LeftToeBase", "RightToeBase", "Hips", "Head" })
        {
            Transform t;
            if (ten.TryGetValue(n, out t)) xuongDuoi.Add(t);
        }

        // Chieu cao than theo xuong, khong doi theo tu the: hong -> dinh dau + dui + ong chan + 6% ban chan.
        float dai = 0f; bool du = hong != null && dauDinh != null;
        string[,] doan = { { "Hips", "head_end" }, { "LeftUpLeg", "LeftLeg" }, { "LeftLeg", "LeftFoot" } };
        for (int i = 0; du && i < doan.GetLength(0); i++)
        {
            Transform p, q;
            if (!ten.TryGetValue(doan[i, 0], out p) || !ten.TryGetValue(doan[i, 1], out q)) { du = false; break; }
            dai += Vector3.Distance(p.position, q.position);
        }
        if (du && dai > 0.3f) cao = dai * 1.06f;
        else { hong = null; cao = hDuPhong; }
    }

    ParticleSystem TaoHe(string ten, Material mat, int toiDa, float sortingFudge)
    {
        var go = new GameObject(ten);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.simulationSpace = DoiChungTheGioi ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        m.scalingMode = ParticleSystemScalingMode.Shape;   // co hat theo so do, khong nhan ti le cua quai
        m.maxParticles = toiDa;
        m.playOnAwake = false;
        m.loop = true;
        var sh = ps.shape; sh.enabled = false;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material = mat;
        r.sortingFudge = sortingFudge;
        r.alignment = ParticleSystemRenderSpace.View;
        r.maxParticleSize = 10f;   // mac dinh 0,5 man hinh: may quay gan thi khoi lua bi ep nho
        return ps;
    }

    static Gradient MoVaoMoRa(float vao, float ra)
    {
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, vao), new GradientAlphaKey(1f, ra), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    ParticleSystem TaoLopLua(string ten, float fudge, float heSoCo, float doSang)
    {
        var matLua = LuaMat;
        var ps = TaoHe(ten, matLua != null ? matLua : VfxFactory.FlameMat, 10, fudge);
        var m = ps.main;
        m.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.4f);
        m.startSpeed = 0f;
        m.startSize3D = true;
        float h = cao + LuaVuotDau * cao / 1.7f;
        float cHat = h / PhanCaoCoLua * heSoCo;
        if (heSoCo == 1f) CoKhoi = new Vector2(cHat * BeNgang, cHat);   // lop chuan dat vi tri chan lua
        m.startSizeX = new ParticleSystem.MinMaxCurve(cHat * (BeNgang - 0.04f), cHat * (BeNgang + 0.04f));
        m.startSizeY = new ParticleSystem.MinMaxCurve(cHat * 0.95f, cHat * 1.05f);
        m.startSizeZ = 1f;
        m.startRotation = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
        m.startColor = new Color(1f, 1f, 1f, doSang);
        var em = ps.emission; em.rateOverTime = KhoiMoiGiay;
        // Mot khoi lua dung ngay tu dau (khong cho khoi dau tien mo dan vao)
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(cao * 0.06f, cao * 0.03f, 0.01f);
        if (matLua != null)
        {
            VfxFactory.BatFlipbook(ps, 4, 4, 1);
            var tsa = ps.textureSheetAnimation;
            tsa.timeMode = ParticleSystemAnimationTimeMode.FPS;
            tsa.fps = KhungMoiGiay;                                 // anh lap vong: chay mai khong vap
        }
        var col = ps.colorOverLifetime; col.enabled = true;
        col.color = MoVaoMoRa(0.22f, 0.72f);                      // cac khoi chong mo vao nhau -> lua song, khong nhay hinh
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.flip = new Vector3(0.5f, 0f, 0f);
        ps.Play();
        return ps;
    }

    void DungHat()
    {
        // Moi lop ~3 khoi chong nhau, cong sang: sang qua thi thanh mot cuc trang, mat het luoi lua (lan dau 8 khoi x 1,15)
        truoc = TaoLopLua("LuaTruoc", -6f, 1.0f, 0.85f);
        sau = TaoLopLua("LuaSau", -4f, 1.08f, 0.55f);

        quang = TaoHe("QuangLua", VfxFactory.GlowMat, 6, -3f);
        {
            var m = quang.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            m.startSpeed = 0f;
            m.startSize = new ParticleSystem.MinMaxCurve(cao * 0.9f, cao * 1.1f);
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.40f, 0.08f, 0.16f), new Color(1f, 0.55f, 0.15f, 0.24f));
            var em = quang.emission; em.rateOverTime = 5f; em.SetBursts(new[] { new ParticleSystem.Burst(0f, 2) });
            var col = quang.colorOverLifetime; col.enabled = true; col.color = MoVaoMoRa(0.3f, 0.6f);
            quang.Play();
        }

        tan = TaoHe("TanLua", VfxFactory.EmberMat, 40, -7f);
        {
            var m = tan.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
            var em = tan.emission; em.rateOverTime = 20f;
            var sh = tan.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(cao * 0.35f, cao * 0.8f, cao * 0.25f);
            sh.alignToDirection = false;
            var vel = tan.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f); vel.y = new ParticleSystem.MinMaxCurve(0.6f, 1.6f); vel.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            m.startSpeed = 0f;
            var nz = tan.noise; nz.enabled = true; nz.strength = 0.5f; nz.frequency = 0.9f;
            var col = tan.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.85f, 0.45f), 0f), new GradientColorKey(new Color(1f, 0.30f, 0.05f), 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            tan.Play();
        }

        // Khoi xam dam boc tu dinh khoi lua (cuc bo theo than - khong keo thanh vet phia sau)
        var matKhoi = VfxFactory.KhoiCuonMat;
        khoi = TaoHe("KhoiChay", matKhoi != null ? matKhoi : VfxFactory.SmokeMat, 12, 4f);
        {
            var m = khoi.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.3f);
            m.startSpeed = 0f;
            m.startSize = new ParticleSystem.MinMaxCurve(cao * 0.35f, cao * 0.55f);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.24f, 0.21f, 0.19f, 0.45f), new Color(0.36f, 0.32f, 0.28f, 0.60f));
            if (matKhoi != null) VfxFactory.BatFlipbook(khoi, 6, 6, 1);
            var em = khoi.emission; em.rateOverTime = 5f;
            var vel = khoi.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f); vel.y = new ParticleSystem.MinMaxCurve(0.8f, 1.2f); vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            var col = khoi.colorOverLifetime; col.enabled = true; col.color = MoVaoMoRa(0.25f, 0.5f);
            var sol = khoi.sizeOverLifetime; sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 1.4f)));
            khoi.transform.localPosition = new Vector3(0f, cao * 0.62f, 0f);
            khoi.Play();
        }

        var dgo = new GameObject("BurnLight");
        dgo.transform.SetParent(transform, false);
        den = dgo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(1f, 0.5f, 0.18f);
        den.intensity = DenManh;
        den.range = 5f * Mathf.Sqrt(cao / 1.7f);
        den.shadows = LightShadows.None;
        var nhay = dgo.AddComponent<LightFlicker>();
        nhay.baseIntensity = DenManh; nhay.amount = 0.35f; nhay.speed = 9f;
    }

    /// <summary>
    /// Dat goc hinh vao GIUA THAN moi khung: ngang theo xuong Hips (hoat hinh chay / nga van theo), day o xuong thap nhat.
    /// Lop truoc lech ve phia may quay, lop sau lech ra sau (theo huong nhin) - khoi lua co be day, trum ra truoc than.
    /// </summary>
    void BamThan()
    {
        Vector3 tam; float day;
        if (hong != null)
        {
            day = float.MaxValue;
            foreach (var t in xuongDuoi) if (t != null && t.position.y < day) day = t.position.y;
            day -= 0.05f;
            tam = new Vector3(hong.position.x, 0f, hong.position.z);
        }
        else
        {
            tam = muc.position; day = muc.position.y;
        }
        float cHat = CoKhoi.y;
        // Chan lua (9% khung) cham xuong thap nhat
        transform.position = new Vector3(tam.x, day + cHat * TamTrenChanLua, tam.z);
        transform.rotation = Quaternion.identity;

        var cam = Camera.main;
        Vector3 huong = Vector3.back;
        if (cam != null)
        {
            huong = cam.transform.position - transform.position;
            huong.y = 0f;
            huong = huong.sqrMagnitude > 1e-4f ? huong.normalized : Vector3.back;
        }
        float r = cao * 0.16f;
        truoc.transform.position = transform.position + huong * r;
        sau.transform.position = transform.position - huong * r * 0.6f;
        quang.transform.position = transform.position + huong * r * 0.5f;
        tan.transform.position = transform.position;
        den.transform.position = transform.position + huong * r + Vector3.up * cao * 0.05f;
    }

    void LateUpdate()
    {
        if (muc == null) { Destroy(gameObject); return; }
        BamThan();
    }
}
