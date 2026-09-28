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
/// Lan 3 (28/09/2026, nguoi dung ve DUONG DO om sat dang nhan vat): "cho ngon lua dot lan khap nguoi, khong de nguyen 1 cuc
/// roi de nhan vat ben trong - lua chay va nam ben trong duong ke do". Khoi billboard lan 2 la tam anh dung truoc than,
/// tran ra hai ben va tren dau. Nay lua ve THANG TREN LUOI NHAN VAT: phu them vat lieu Diablo25D/LuaPhuThan (anh nhieu lua
/// Blender MCP, canh LuaPhuThan -> Resources/KyNang/Chay/LuaPhuThan.png; vat lieu goc Resources/KyNang/Chay/LuaPhuThan.mat
/// de shader vao ban build) len moi SkinnedMeshRenderer / MeshRenderer, nhu lop than cua ChayDenToanThan: lua om dung tay,
/// chan, dau, cu dong theo hoat hinh; vo lua phong ra 7 cm o vien. Khoi billboard lan 2 CHI con lam DOI CHUNG cho menu 90
/// (<see cref="DoiChungKhoiLua"/>). Tan lua, den giu nguyen (cuc bo); bo khoi.
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
    /// <summary>Phep thu (menu 90) bat de lam DOI CHUNG: khoi lua billboard cua lan 2 thay cho lop lua phu tren than.</summary>
    public static bool DoiChungKhoiLua;

    const string DuongVatLieuPhu = "KyNang/Chay/LuaPhuThan";
    static Material mPhuGoc;
    Material lopPhu;
    readonly List<Renderer> daPhu = new List<Renderer>();
    /// <summary>So renderer da phu lop lua - phep thu doc.</summary>
    public int SoRendererDaPhu { get { return daPhu.Count; } }
    public Material LopPhu { get { return lopPhu; } }

    Transform muc, hong, dauDinh;
    readonly List<Transform> xuongDuoi = new List<Transform>();
    float cao = 1.7f;
    float hDuPhong = 1.7f;

    ParticleSystem truoc, sau, quang, tan;
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
        if (DoiChungKhoiLua)
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
        }
        else PhuThan();

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

        // KHONG co khoi: khoi xam tren nen dem toi lam SANG nen quanh than thanh mot quang mo ngoai vien (menu 90 do duoc,
        // lan 3 - nguoi dung muon lua nam gon trong duong vien than).

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
        // Khoi billboard (doi chung): chan lua (9% khung) cham xuong thap nhat. Lop phu: goc o giua than.
        transform.position = truoc != null ? new Vector3(tam.x, day + cHat * TamTrenChanLua, tam.z) : new Vector3(tam.x, day + cao * 0.5f, tam.z);
        transform.rotation = Quaternion.identity;
        if (lopPhu != null)
        {
            // Goc toa do lua = chan (xuong thap nhat), ngang theo Hips: nhan vat chay thi hoa van lua di theo, khong truot
            lopPhu.SetVector("_Goc", new Vector4(tam.x, day, tam.z, 0f));
            lopPhu.SetFloat("_CaoThan", cao);
        }

        var cam = Camera.main;
        Vector3 huong = Vector3.back;
        if (cam != null)
        {
            huong = cam.transform.position - transform.position;
            huong.y = 0f;
            huong = huong.sqrMagnitude > 1e-4f ? huong.normalized : Vector3.back;
        }
        float r = cao * 0.16f;
        if (truoc != null) truoc.transform.position = transform.position + huong * r;
        if (sau != null) sau.transform.position = transform.position - huong * r * 0.6f;
        if (quang != null) quang.transform.position = transform.position + huong * r * 0.5f;
        tan.transform.position = transform.position;
        den.transform.position = transform.position + huong * r * 2f + Vector3.up * cao * 0.05f;   // den ra ngoai than: den trong than chieu nong mat trong
    }

    /// <summary>
    /// Phu lop lua len moi renderer cua nhan vat (bo renderer trong suot - vom khien, hat, tia set gan vao nguoi; chi xet vat
    /// lieu GOC o dau mang: lop phu them khac nhu vo bang, bong uot deu trong suot). Moi ke mot ban vat lieu rieng (_Goc).
    /// </summary>
    void PhuThan()
    {
        if (mPhuGoc == null) mPhuGoc = Resources.Load<Material>(DuongVatLieuPhu);
        if (mPhuGoc == null) return;
        lopPhu = new Material(mPhuGoc);
        lopPhu.name = "P_LuaPhuThan";
        foreach (var r in muc.GetComponentsInChildren<Renderer>())
        {
            if (r == null || !r.enabled) continue;
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            if (r.transform.IsChildOf(transform)) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0 || mats[0] == null || mats[0].renderQueue >= 3000) continue;
            var moi = new Material[mats.Length + 1];
            for (int k = 0; k < mats.Length; k++) moi[k] = mats[k];
            moi[mats.Length] = lopPhu;
            r.sharedMaterials = moi;
            daPhu.Add(r);
        }
    }

    /// <summary>Phep thu: an / hien toan bo hinh lua (lop phu + hat), de chup anh co / khong lua cung mot khung.</summary>
    public void AnHinh(bool an)
    {
        if (lopPhu != null) lopPhu.SetFloat("_Do", an ? 0f : 1f);
        foreach (var r in GetComponentsInChildren<ParticleSystemRenderer>()) r.enabled = !an;
    }

    public Light Den { get { return den; } }

    void OnDestroy()
    {
        // Go DUNG lop cua minh (khong tra ca mang goc - de khong mat vo bang / than den dang chong len)
        for (int i = 0; i < daPhu.Count; i++)
        {
            var r = daPhu[i];
            if (r == null) continue;
            var mats = r.sharedMaterials;
            var giu = new List<Material>(mats.Length);
            for (int k = 0; k < mats.Length; k++) if (mats[k] != lopPhu) giu.Add(mats[k]);
            r.sharedMaterials = giu.ToArray();
        }
        daPhu.Clear();
        if (lopPhu != null) Destroy(lopPhu);
    }

    void LateUpdate()
    {
        if (muc == null) { Destroy(gameObject); return; }
        BamThan();
    }
}
