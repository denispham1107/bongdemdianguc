using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LUA CHAY TOAN THAN - hinh cua trang thai bong chay (<see cref="BurningEffect"/>) cho moi nguoi choi va moi quai.
///
/// Nguoi dung (28/09/2026): "chi vang ra cac dom chay nho qua don so" -> "toan than bi boc chay boi lua that su, sap het chay
/// thi lua giam dan roi bien mat". Ban cu: mot he hat hinh cau 0,4 m o giua nguoi + tia lua nho.
///
/// Ngon lua: anh flipbook 4x4 dung bang Blender MCP (CongCu/Blender/lua_chay_nguoi.blend -> Resources/Flipbooks/LuaChayNguoi):
/// 16 khung la TRON MOT DOI mot ngon lua - bung len, liem cao, tach luoi, tan thanh do sam. Moi hat chay tu khung 0 den 15
/// dung mot lan (khong bat dau o khung ngau nhien nhu khoi - bat dau giua chung la ngon lua hien ra da dang tan).
///
/// PHAT THEO XUONG: moi khung hinh (LateUpdate - sau hoat hinh) chon ngau nhien mot KHUC XUONG (hong-lung, lung-nguc, dui,
/// ong chan, canh tay, co-dau...) theo trong so dai x day, roi phat hat o mot diem quanh truc khuc xuong ay. Lua vi the bam
/// dung tay, chan, dau khi nhan vat chay, vung tay, nga, nam - khong phai mot cuc lua treo o giua nguoi.
/// Luoi nhan vat (Meshy) Read/Write TAT nen khong dung duoc hinh phat SkinnedMeshRenderer cua he hat (ban build im lang
/// khong phat gi). Moi quai + nguoi choi dung chung mot bo xuong Meshy (Hips, Spine, Spine01, Spine02, neck, Head, head_end,
/// Left/Right UpLeg, Leg, Foot, Arm, ForeArm, Hand). Vat khong co xuong (bia thu, hinh nhan) thi mot khuc dung giua than.
///
/// Mo phong THE GIOI: ngon lua o lai sau lung khi chay - nhu lua that.
///
/// GIAM DAN: <see cref="DatDoManh"/>(k) - k = 1 chay manh, 0 tat. So ngon lua moi giay x k, co lua x (0,45 + 0,55k),
/// den x k. BurningEffect dat k = con lai / <see cref="GiayTatDan"/> nen giay cuoi lua nho dan, thua dan roi het.
/// Bi go giua chung (Toc bien, Tang hinh, chet) thi <see cref="TatDan"/> tu ha k ve 0 trong <see cref="GiayTatNhanh"/>.
/// </summary>
public class LuaToanThan : MonoBehaviour
{
    /// <summary>Bao nhieu giay cuoi cua lan chay thi lua bat dau giam dan.</summary>
    public const float GiayTatDan = 1.2f;
    /// <summary>Bi go giua chung (Toc bien cap 5, Tang hinh, chet): lua tu tat dan trong chung nay giay.</summary>
    public const float GiayTatNhanh = 0.6f;

    // So hat moi giay khi chay manh, cho than cao 1,7 m (than to hon thi nhieu hon theo dien tich).
    public const float LuaMoiGiay = 95f;
    const float QuangMoiGiay = 9f;
    const float KhoiMoiGiay = 5f;
    const float TanMoiGiay = 16f;
    const float DenManh = 2.4f;

    struct Khuc { public Transform a, b; public Vector3 la, lb; public float r, w; }

    readonly List<Khuc> khuc = new List<Khuc>();
    float tongTrongSo;
    float cao = 1.7f;        // chieu cao than (m) do theo xuong
    float tiLe = 1f;         // cao / 1,7

    ParticleSystem lua, quang, khoi, tan;
    Light den;
    LightFlicker nhay;

    float doManh = 1f;
    bool dangTat;
    float giayTatConLai;
    float tichLua, tichQuang, tichKhoi, tichTan;

    /// <summary>Do manh hien tai (phep thu doc).</summary>
    public float DoManh { get { return doManh; } }
    /// <summary>So khuc xuong dang phat lua (phep thu doc - 0 nghia la dang dung khuc du phong).</summary>
    public int SoKhucXuong { get; private set; }
    public float ChieuCaoThan { get { return cao; } }
    public ParticleSystem HatLua { get { return lua; } }

    static Material mLua;
    /// <summary>Vat lieu ngon lua Blender (cong sang, giu nguyen mau trong anh). Null neu thieu anh.</summary>
    public static Material LuaMat
    {
        get
        {
            if (mLua == null)   // kiem bang null cua Unity: thoat Play la vat lieu tao luc chay bi xoa
            {
                var tex = VfxFactory.NapFlipbook("LuaChayNguoi");
                if (tex == null) return null;
                mLua = Mats.FlipbookAdd("P_LuaChayNguoi", tex, Color.white, 1.25f);
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
        l.TimXuong(muc, h, r);
        l.DungHat();
        return l;
    }

    public void DatDoManh(float k)
    {
        if (dangTat) return;
        doManh = Mathf.Clamp01(k);
    }

    /// <summary>Tu tat dan trong GiayTatNhanh roi tu xoa khi hat cuoi cung tan.</summary>
    public void TatDan()
    {
        if (dangTat) return;
        dangTat = true;
        giayTatConLai = GiayTatNhanh * doManh;
    }

    // ---------------------------------------------------------------- xuong

    void TimXuong(Transform muc, float h, float r)
    {
        var ten = new Dictionary<string, Transform>();
        var smr = muc.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null)
            foreach (var x in smr.bones) if (x != null && !ten.ContainsKey(x.name)) ten[x.name] = x;

        // Chieu cao than do theo XUONG, khong phu thuoc tu the (dang nga, dang nam van dung): hong -> dinh dau (thang,
        // khong cong tung dot song lung) + dui + ong chan + ~6% cho ban chan. Cong ca chuoi xuong tung dot thi ra 2,03 -
        // 2,26 m (chuoi zic-zac) trong khi hinh that 1,6 - 1,9 m. rig.bodyHeight cung sai (bo xuong 2,36 m).
        float dai = 0f; bool du = true;
        string[,] doan = { { "Hips", "head_end" }, { "LeftUpLeg", "LeftLeg" }, { "LeftLeg", "LeftFoot" } };
        for (int i = 0; i < doan.GetLength(0); i++)
        {
            Transform p, q;
            if (!ten.TryGetValue(doan[i, 0], out p) || !ten.TryGetValue(doan[i, 1], out q)) { du = false; break; }
            dai += Vector3.Distance(p.position, q.position);
        }
        dai *= 1.06f;

        if (du && dai > 0.3f)
        {
            cao = dai;
            // Ban kinh moi khuc theo ti le chieu cao (than nguoi 1,7 m: nguc ~0,17 m, dui ~0,10, canh tay ~0,06)
            ThemKhuc(ten, "Hips", "Spine01", 0.095f);
            ThemKhuc(ten, "Spine01", "Spine02", 0.100f);
            ThemKhuc(ten, "Spine02", "neck", 0.085f);
            ThemKhuc(ten, "neck", "head_end", 0.065f);
            ThemKhuc(ten, "Spine02", "LeftArm", 0.045f);
            ThemKhuc(ten, "Spine02", "RightArm", 0.045f);
            ThemKhuc(ten, "LeftArm", "LeftForeArm", 0.040f);
            ThemKhuc(ten, "RightArm", "RightForeArm", 0.040f);
            ThemKhuc(ten, "LeftForeArm", "LeftHand", 0.034f);
            ThemKhuc(ten, "RightForeArm", "RightHand", 0.034f);
            ThemKhuc(ten, "LeftUpLeg", "LeftLeg", 0.058f);
            ThemKhuc(ten, "RightUpLeg", "RightLeg", 0.058f);
            ThemKhuc(ten, "LeftLeg", "LeftFoot", 0.050f);
            ThemKhuc(ten, "RightLeg", "RightFoot", 0.050f);
            ThemKhuc(ten, "LeftFoot", "LeftToeBase", 0.045f);     // ban chan (Quy cay chan re to: thieu khuc nay la dai chan chi 21% co lua)
            ThemKhuc(ten, "RightFoot", "RightToeBase", 0.045f);
            SoKhucXuong = khuc.Count;
        }
        if (khuc.Count == 0)
        {
            // Khong co xuong: mot khuc dung giua than (toa do cua muc)
            cao = Mathf.Max(0.5f, h);
            khuc.Add(new Khuc { la = new Vector3(0f, cao * 0.12f, 0f), lb = new Vector3(0f, cao * 0.92f, 0f),
                                r = Mathf.Max(0.12f, r), w = 1f });
            tongTrongSo = 1f;
            SoKhucXuong = 0;
        }
        tiLe = Mathf.Clamp(cao / 1.7f, 0.5f, 2.5f);
    }

    void ThemKhuc(Dictionary<string, Transform> ten, string tu, string den, float heSoDay)
    {
        Transform a, b;
        if (!ten.TryGetValue(tu, out a) || !ten.TryGetValue(den, out b)) return;
        float r = heSoDay * cao;
        float dai = Vector3.Distance(a.position, b.position);
        float w = (dai + r) * r;      // dien tich mat ben ~ dai x day
        khuc.Add(new Khuc { a = a, b = b, r = r, w = w });
        tongTrongSo += w;
    }

    Vector3 DiemTren(int i, out float r, out float t)
    {
        var k = khuc[i];
        Vector3 pa = k.a != null ? k.a.position : transform.parent.TransformPoint(k.la);
        Vector3 pb = k.b != null ? k.b.position : transform.parent.TransformPoint(k.lb);
        t = Random.value;
        Vector3 p = Vector3.Lerp(pa, pb, t);
        Vector3 truc = pb - pa;
        Vector3 ngang = Vector3.Cross(truc.sqrMagnitude > 1e-6f ? truc.normalized : Vector3.up, Random.onUnitSphere);
        if (ngang.sqrMagnitude < 1e-6f) ngang = Vector3.right;
        r = k.r;
        return p + ngang.normalized * k.r * Random.Range(0.3f, 1f);
    }

    int ChonKhuc()
    {
        float x = Random.value * tongTrongSo;
        for (int i = 0; i < khuc.Count; i++) { x -= khuc[i].w; if (x <= 0f) return i; }
        return khuc.Count - 1;
    }

    // ---------------------------------------------------------------- hat

    ParticleSystem TaoHe(string ten, Material mat, int toiDa, float sortingFudge)
    {
        var go = new GameObject(ten);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = toiDa;
        m.playOnAwake = false;
        m.loop = true;
        // Phat bang tay (Emit) nen mo-dun phat de 0
        var em = ps.emission; em.rateOverTime = 0f;
        var sh = ps.shape; sh.enabled = false;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material = mat;
        r.sortingFudge = sortingFudge;
        r.alignment = ParticleSystemRenderSpace.View;
        r.maxParticleSize = 10f;   // mac dinh 0,5 man hinh: may quay gan thi lua bi ep nho
        ps.Play();
        return ps;
    }

    void DungHat()
    {
        var matLua = LuaMat;
        lua = TaoHe("LuaThan", matLua != null ? matLua : VfxFactory.FlameMat, 200, -6f);
        {
            var m = lua.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.85f);
            m.startSpeed = 0f;
            m.startRotation = new ParticleSystem.MinMaxCurve(-0.22f, 0.22f);
            m.gravityModifier = -0.04f;     // lua boc len (it thoi - boc cao qua thi thanh cot lua tren dau, than duoi trong)
            if (matLua != null)
            {
                VfxFactory.BatFlipbook(lua, 4, 4, 1);
                var tsa = lua.textureSheetAnimation;
                tsa.startFrame = 0f;          // moi ngon lua chay tron doi cua no tu khung 0
            }
            var col = lua.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var nz = lua.noise; nz.enabled = true; nz.strength = 0.35f; nz.frequency = 1.4f; nz.scrollSpeed = 0.8f;
            var r = lua.GetComponent<ParticleSystemRenderer>();
            r.flip = new Vector3(0.5f, 0f, 0f);          // nua so ngon lua lat ngang - 16 khung ma khong lap lai
            r.pivot = new Vector3(0f, 0.12f, 0f);         // ngon lua trong anh nam nua duoi o khung - nang nhe cho chan lua sat than
        }

        quang = TaoHe("QuangLua", VfxFactory.GlowMat, 40, -4f);
        {
            var m = quang.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            m.startSpeed = 0f;
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.40f, 0.08f, 0.30f), new Color(1f, 0.55f, 0.15f, 0.45f));
            var col = quang.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
        }

        // Khoi cuon flipbook (anh TRANG, to mau luc chay) nhuom xam dam: den kit thi tan vao nen dem (xem SetChayDen).
        // KhoiDen cua lo lua la luoi 8x6 khung DOC (cot khoi cao) - dat len hat vuong thi bep dep.
        var matKhoi = VfxFactory.KhoiCuonMat;
        khoi = TaoHe("KhoiChay", matKhoi != null ? matKhoi : VfxFactory.SmokeMat, 30, 4f);
        {
            var m = khoi.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.7f);
            m.startSpeed = 0f;
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m.gravityModifier = -0.10f;
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.24f, 0.21f, 0.19f, 0.50f), new Color(0.36f, 0.32f, 0.28f, 0.65f));
            if (matKhoi != null) VfxFactory.BatFlipbook(khoi, 6, 6, 1);
            var col = khoi.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var sol = khoi.sizeOverLifetime; sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.4f)));
        }

        tan = TaoHe("TanLua", VfxFactory.EmberMat, 60, -7f);
        {
            var m = tan.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
            m.startSpeed = 0f;
            m.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
            m.gravityModifier = -0.30f;
            var nz = tan.noise; nz.enabled = true; nz.strength = 0.7f; nz.frequency = 0.9f;
            var col = tan.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.85f, 0.45f), 0f), new GradientColorKey(new Color(1f, 0.30f, 0.05f), 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
        }

        var dgo = new GameObject("BurnLight");
        dgo.transform.SetParent(transform, false);
        den = dgo.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(1f, 0.5f, 0.18f);
        den.intensity = DenManh;
        den.range = 5f * Mathf.Sqrt(tiLe);
        den.shadows = LightShadows.None;
        nhay = dgo.AddComponent<LightFlicker>();
        nhay.baseIntensity = DenManh; nhay.amount = 0.35f; nhay.speed = 9f;
        CapNhatDen();
    }

    void CapNhatDen()
    {
        if (den == null || khuc.Count == 0) return;
        // Den o giua than (khuc dau tien = hong-lung khi co xuong)
        var k = khuc[0];
        Vector3 pa = k.a != null ? k.a.position : transform.parent.TransformPoint(k.la);
        Vector3 pb = k.b != null ? k.b.position : transform.parent.TransformPoint(k.lb);
        den.transform.position = Vector3.Lerp(pa, pb, 0.8f);
        nhay.baseIntensity = DenManh * doManh;
    }

    void LateUpdate()
    {
        if (transform.parent == null) { Destroy(gameObject); return; }
        float dt = Time.deltaTime;
        if (dangTat)
        {
            giayTatConLai -= dt;
            doManh = Mathf.Clamp01(giayTatConLai / GiayTatNhanh);
            if (doManh <= 0f && lua.particleCount == 0 && khoi.particleCount == 0 && tan.particleCount == 0)
            { Destroy(gameObject); return; }
        }

        CapNhatDen();
        if (doManh <= 0f) return;

        float dienTich = tiLe * tiLe;
        float k = doManh;
        Phat(lua, LuaMoiGiay * dienTich * k, ref tichLua, dt, KieuHat.Lua);
        Phat(quang, QuangMoiGiay * dienTich * k, ref tichQuang, dt, KieuHat.Quang);
        Phat(khoi, KhoiMoiGiay * dienTich * k, ref tichKhoi, dt, KieuHat.Khoi);
        Phat(tan, TanMoiGiay * dienTich * k, ref tichTan, dt, KieuHat.Tan);
    }

    enum KieuHat { Lua, Quang, Khoi, Tan }

    void Phat(ParticleSystem ps, float moiGiay, ref float tich, float dt, KieuHat kieu)
    {
        if (ps == null) return;
        tich += moiGiay * dt;
        int n = Mathf.FloorToInt(tich);
        if (n <= 0) return;
        tich -= n;
        n = Mathf.Min(n, 12);   // khung hinh giat thi khong do ca dong hat vao mot khung
        var ep = new ParticleSystem.EmitParams();
        float s = tiLe, k = doManh;
        for (int j = 0; j < n; j++)
        {
            float r, t;
            Vector3 p = DiemTren(ChonKhuc(), out r, out t);
            ep.position = p;
            switch (kieu)
            {
                case KieuHat.Lua:
                    // Ngon lua co theo do day cua khuc (than to lua to, co tay lua nho) va nho dan khi tat
                    ep.startSize = Mathf.Clamp(r * Random.Range(2.6f, 3.8f), 0.24f * s, 0.70f * s) * (0.45f + 0.55f * k);
                    ep.velocity = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.2f, 0.55f) * s, Random.Range(-0.1f, 0.1f));
                    ep.startColor = new Color(1f, 1f, 1f, 0.55f + 0.45f * k);
                    break;
                case KieuHat.Quang:
                    ep.startSize = Mathf.Max(0.35f * s, r * 7f) * (0.5f + 0.5f * k);
                    ep.velocity = Vector3.up * 0.3f * s;
                    ep.startColor = new Color(1f, Random.Range(0.38f, 0.55f), 0.1f, Random.Range(0.28f, 0.42f) * k);
                    break;
                case KieuHat.Khoi:
                    ep.position = p + Vector3.up * 0.35f * s;
                    ep.startSize = Random.Range(0.55f, 1.0f) * s * (0.6f + 0.4f * k);
                    ep.velocity = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.7f, 1.2f) * s, Random.Range(-0.1f, 0.1f));
                    break;
                case KieuHat.Tan:
                    ep.velocity = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(0.6f, 1.6f) * s, Random.Range(-0.4f, 0.4f));
                    break;
            }
            ps.Emit(ep, 1);
        }
    }
}
