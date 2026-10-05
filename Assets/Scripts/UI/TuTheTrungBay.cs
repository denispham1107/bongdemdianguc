using UnityEngine;

/// <summary>
/// TU THE PHU THUY TRUNG BAY O MAN CHINH (nguoi dung 04/10/2026: "2 ban tay cua phu thuy ngua len troi va hoi co cao len 1 chut; 1 ban
/// tay phat sang qua cau bang, 1 ban tay phat sang qua cau lua"; chon: lua ben PHAI man hinh / bang ben TRAI (luc nhan vat quay mat ve
/// may quay), QUA CAU Y NHU TRONG TRAN thu nho, ban tay ngang BUNG chia sang hai ben).
///
/// Chay SAU NguoiChoiHoatHinh (no dat tu the dung + tho moi khung): ngam huong canh tay tren (buong xuong, hoi ra ngoai, hoi ra truoc)
/// va cang tay (ra truoc + ra ngoai, hoi len) bang NguoiChoiHoatHinh.NgamHuongKhop - cung cach TuTheGiatSet, khong doan goc Euler theo
/// truc khop Meshy. Roi XOAN cang tay quanh truc cua no + be co tay cho LONG BAN TAY NGUA LEN.
/// Khung xuong Meshy khong co xuong ngon tay: phap tuyen long ban tay (toa do cuc bo xuong Hand) DO bang menu 101 (xoay xuong Hand,
/// BakeMesh hai lan, PCA cac dinh di theo) va chon dau bang anh chup can menu 101b (phia co nhan o mu ngon la MU tay).
/// Hai qua cau: VfxFactory.BuildFireballVisual / BuildQuaCauBangVisual (dung hinh cua ky nang that), ban kinh BanKinhCau, lo lung tren
/// long ban tay, nhap nho nhe. Gan luc chay tu MainMenuUI.Start - KHONG sua scene MainMenu (dung bang menu 51).
///
/// DUNG HAI CHAN BANG NHAU, CHAM DAT, NHIN THANG MAY QUAY (nguoi dung 05/10/2026: "dung 2 chan bang nhau, khong lo lung tren khong trung,
/// nhin thang chinh dien ve phia nguoi choi; khong can tu quay"). Truoc do: menu 51 dat goc nhan vat CAO 0,2 m tren dat va quay lech may quay
/// 20 do, tu the goc cua prefab la buoc do (chan trai nhac cao hon chan phai 0,10 m) -> de giay ho dat 0,15 m. Nay: Start quay mat thang vao
/// may quay. Hai chan thang bang nhau + cham dat: NguoiChoiHoatHinh lam (phan "CHAN CHAM DAT" - dung chung voi nhan vat trong tran,
/// nguoi dung 05/10/2026 bao trong game chan cung lo lung); ban dau o day co rieng mot ban (ha goc mot lan bang BakeMesh) - da gop vao do.
/// </summary>
[DefaultExecutionOrder(10020)]
public class TuTheTrungBay : MonoBehaviour
{
    /// <summary>Phap tuyen LONG ban tay trong toa do cuc bo xuong LeftHand / RightHand (menu 101 + 101b).</summary>
    public static readonly Vector3 LongTayTrai = new Vector3(0.720f, 0.144f, 0.679f), LongTayPhai = new Vector3(-0.754f, 0.204f, 0.624f);

    /// <summary>Tam long ban tay trong toa do cuc bo xuong Hand (tam cac dinh ban tay, menu 101).</summary>
    public static readonly Vector3 TamLongTay = new Vector3(0f, 0.11f, -0.007f);

    /// <summary>Ban kinh qua cau tren tay (m) va qua cau lo lung cach long tay bao nhieu.</summary>
    public const float BanKinhCau = 0.13f, CachLongTay = 0.07f;

    /// <summary>Den cua qua cau tren tay: tam (m) va do sang toi da. Den cua hinh ky nang dat cho qua cau BAY trong tran (lua: 6 / 12 m)
    /// - de nguyen thi nhuom cam ca nen dat man chinh (anh menu 101c lan dau); o day chi can hat sang len tay, ao va mat dat quanh chan.</summary>
    public const float TamDenCau = 2.6f, DoSangDenCau = 2.2f;

    /// <summary>Goc ngon lua nam tren mat cau ban kinh BanKinhCau x BanKinhGocLua (sat mat loi lua sang).</summary>
    public const float BanKinhGocLua = 0.85f;

    /// <summary>Lop lua GIUA: goc lua trong long qua cau ban kinh BanKinhCau x BanKinhLuaGiua, so ngon moi giay.</summary>
    public const float BanKinhLuaGiua = 0.3f, LuaGiuaMoiGiay = 26f;

    /// <summary>Khoi den boc len tu qua cau lua: hat / giay, toc boc len (m/s).</summary>
    public const float KhoiDenMoiGiay = 9f, TocKhoiLen = 0.55f;

    /// <summary>
    /// KHOI DEN BOC LEN tu qua cau lua (nguoi dung 04/10/2026: "them 1 it hieu ung khoi den bay len tu qua cau lua"): flipbook khoi cuon
    /// (KhoiCuon 6x6) mau xam den, sinh o dinh qua cau, KHONG GIAN THE GIOI (boc len thanh lan, qua cau nhap nho / nhan vat xoay thi khoi
    /// o lai troi len nhu khoi that), no to dan roi tan. Lop "Smoke" cu cua hinh qua cau giu nguyen.
    /// </summary>
    static void TaoKhoiDen(Transform cau)
    {
        var mat = VfxFactory.KhoiCuonMat;
        if (mat == null) return;
        var go = new GameObject("KhoiDenBocLen");
        go.transform.SetParent(cau, false);
        go.transform.localPosition = Vector3.up * BanKinhCau * 0.8f;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.loop = true; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(1.3f, 1.9f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(BanKinhCau * 1.0f, BanKinhCau * 1.6f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.05f, 0.045f, 0.04f, 0.50f), new Color(0.11f, 0.10f, 0.09f, 0.65f));
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 30;
        var e = ps.emission; e.rateOverTime = KhoiDenMoiGiay;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = BanKinhCau * 0.35f;
        // Moi truc van toc CUNG kieu TwoConstants (lech kieu la Unity bo ca mo-dun)
        var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
        v.x = new ParticleSystem.MinMaxCurve(-0.06f, 0.06f);
        v.y = new ParticleSystem.MinMaxCurve(TocKhoiLen * 0.8f, TocKhoiLen * 1.25f);
        v.z = new ParticleSystem.MinMaxCurve(-0.06f, 0.06f);
        var sol = ps.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 3.2f)));
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0.6f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);
        var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.renderMode = ParticleSystemRenderMode.Billboard;
        VfxFactory.BatFlipbook(ps, 6, 6, 1);
        ps.Play();
    }

    /// <summary>Nhan vat vao tu the trong bao lau (giay).</summary>
    public const float GiayVaoTuThe = 0.8f;

    NguoiChoiHoatHinh hh;
    Transform tayTraiTren, tayTraiDuoi, banTayTrai, tayPhaiTren, tayPhaiDuoi, banTayPhai;
    Transform cauLua, cauBang;
    float batDau;

    /// <summary>Quay mat thang vao may quay chinh (chi quanh truc dung).</summary>
    void QuayMatVeMayQuay()
    {
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 v = cam.transform.position - transform.position; v.y = 0f;
        if (v.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(v.normalized, Vector3.up);
    }

    /// <summary>Phep thu (menu 101c) doc: tay nao cam lua.</summary>
    public Transform BanTayLua { get; private set; }
    public Transform BanTayBang { get; private set; }

    void Start()
    {
        hh = GetComponentInChildren<NguoiChoiHoatHinh>();
        if (hh == null) { enabled = false; return; }
        tayTraiTren = hh.tayTraiTren; tayTraiDuoi = hh.tayTraiDuoi; tayPhaiTren = hh.tayPhaiTren; tayPhaiDuoi = hh.tayPhaiDuoi;
        banTayTrai = hh.banTayTrai != null ? hh.banTayTrai : TimBanTay(tayTraiDuoi);
        banTayPhai = hh.banTayPhai != null ? hh.banTayPhai : TimBanTay(tayPhaiDuoi);
        if (tayTraiTren == null || tayTraiDuoi == null || banTayTrai == null || tayPhaiTren == null || tayPhaiDuoi == null || banTayPhai == null)
        { enabled = false; return; }
        batDau = Time.time;
        QuayMatVeMayQuay();
        // Man chinh: hai chan THANG, BANG NHAU (nguoi dung da duyet) - khong co goi / nghieng ban chan theo doc nhu trong tran
        hh.thichNghiDoc = false;

        // Lua ben PHAI man hinh: nhan vat nhin thang vao may quay (dung yen, khong con tu xoay) thi tay TRAI cua no nam ben phai man hinh
        BanTayLua = banTayTrai; BanTayBang = banTayPhai;

        var goLua = new GameObject("CauLuaTrenTay");
        VfxFactory.BuildFireballVisual(goLua.transform, BanKinhCau);
        cauLua = goLua.transform;
        var goBang = new GameObject("CauBangTrenTay");
        VfxFactory.BuildQuaCauBangVisual(goBang.transform, BanKinhCau);
        cauBang = goBang.transform;
        // Tan lua (Sparks - vet keo dai, "thanh nho mau lua") VA lua loi (Flames - anh tam giac cu Tex_flame) -> NGON LUA THAT (flipbook
        // mo phong Mantaflow, Blender MCP) - nguoi dung 04/10/2026. Trong tran lua loi da la flipbook LuaDuoi tu truoc (NangCapDuoiLua).
        VfxFactory.TanLuaThanhNgonLua(goLua.transform, BanKinhCau);
        var loi = goLua.transform.Find("Flames");
        if (loi != null) VfxFactory.DoiThanhNgonLuaThat(loi.GetComponent<ParticleSystem>(), BanKinhCau * 2.6f, BanKinhCau * 4.2f, 34f, 0.12f, 0.45f);
        // LUA BAM QUANH QUA CAU (nguoi dung 04/10/2026: "cac vet lua phai bao quanh lay qua cau lua, khong duoc bay lo lung o tren khong"
        // - chon chi qua cau tren tay; trong tran giu duoi lua khi bay): moi ngon lua MOC TU MAT QUA CAU va DI THEO no - mo phong CUC BO
        // (qua cau nhap nho / nhan vat xoay thi lua khong bi bo lai), khong van toc, khong boc len, khong nhieu day trot.
        foreach (var ten in new[] { "Sparks", "Flames" })
        {
            var t = goLua.transform.Find(ten);
            var ps = t != null ? t.GetComponent<ParticleSystem>() : null;
            if (ps == null) continue;
            var m = ps.main;
            m.simulationSpace = ParticleSystemSimulationSpace.Local;
            m.startSpeed = 0f;
            m.gravityModifier = 0f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = BanKinhCau * BanKinhGocLua; sh.radiusThickness = 0f;
            var nz = ps.noise; nz.enabled = false;
        }
        // LUA GIUA TAM (nguoi dung 04/10/2026: "lua chay qua 2 ben nhieu qua - giu luong lua 2 ben, tang lua o GIUA tam qua cau"): them
        // mot lop ngon lua that nhan ban tu lop Flames da chinh, goc lua gom vao long qua cau (ban kinh 0,3 r) -> ngon lua moc tu giua
        // liem len qua dinh qua cau; hai lop cu giu nguyen.
        if (loi != null)
        {
            var giua = Instantiate(loi.gameObject, goLua.transform, false);
            giua.name = "LuaGiua";
            var ps = giua.GetComponent<ParticleSystem>();
            var sh = ps.shape; sh.radius = BanKinhCau * BanKinhLuaGiua;
            var e = ps.emission; e.rateOverTime = LuaGiuaMoiGiay;
            ps.Play();
        }
        TaoKhoiDen(goLua.transform);
        foreach (var goCau in new[] { goLua, goBang })
            foreach (var lt in goCau.GetComponentsInChildren<Light>(true))
            {
                var fl = lt.GetComponent<LightFlicker>();
                if (fl != null) { fl.baseIntensity = Mathf.Min(fl.baseIntensity, DoSangDenCau); fl.DatTamGoc(Mathf.Min(lt.range, TamDenCau)); }
                else { lt.intensity = Mathf.Min(lt.intensity, DoSangDenCau); lt.range = Mathf.Min(lt.range, TamDenCau); }
            }
        CapNhatCau();
    }

    static Transform TimBanTay(Transform cangTay)
    {
        if (cangTay == null) return null;
        for (int i = 0; i < cangTay.childCount; i++) if (cangTay.GetChild(i).name.Contains("Hand")) return cangTay.GetChild(i);
        return null;
    }

    void LateUpdate()
    {
        float w = Mathf.SmoothStep(0f, 1f, (Time.time - batDau) / GiayVaoTuThe);
        Transform goc = hh.transform;
        Vector3 f = goc.forward, r = goc.right, u = Vector3.up;
        DatTay(tayTraiTren, tayTraiDuoi, banTayTrai, LongTayTrai, f, r, u, w, 0f);
        DatTay(tayPhaiTren, tayPhaiDuoi, banTayPhai, LongTayPhai, f, r, u, w, 1.7f);
        CapNhatCau();
    }

    void DatTay(Transform tren, Transform duoi, Transform tay, Vector3 longCucBo, Vector3 f, Vector3 r, Vector3 u, float w, float pha)
    {
        // ben ngoai: phia cua ban tay so voi truc than
        Vector3 ngoai = Vector3.Dot(tay.position - hh.transform.position, r) >= 0f ? r : -r;
        float tho = Mathf.Sin(Time.time * 1.3f + pha) * 0.03f;
        // canh tay tren buong xuong, hoi ra ngoai + ra truoc; cang tay ra truoc + ra hai ben, hoi len -> ban tay ngang bung
        Vector3 huongTren = (-u * 0.85f + ngoai * 0.40f + f * 0.22f).normalized;
        Vector3 huongDuoi = (f * 0.72f + ngoai * 0.55f + u * (0.14f + tho)).normalized;
        NguoiChoiHoatHinh.NgamHuongKhop(tren, duoi, huongTren, w);
        NguoiChoiHoatHinh.NgamHuongKhop(duoi, tay, huongDuoi, w);

        // XOAN cang tay quanh truc cua no cho phap tuyen long ban tay quay len troi (khong doi vi tri ban tay)
        Vector3 truc = (tay.position - duoi.position).normalized;
        Vector3 longTG = tay.TransformDirection(longCucBo);
        Vector3 a = Vector3.ProjectOnPlane(longTG, truc), b = Vector3.ProjectOnPlane(u, truc);
        if (a.sqrMagnitude > 1e-6f && b.sqrMagnitude > 1e-6f)
            duoi.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(a, b, truc) * w, truc) * duoi.rotation;
        // be co tay phan con lai (cang tay hoi nghieng len nen long tay con lech vai do)
        longTG = tay.TransformDirection(longCucBo);
        tay.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(longTG, u), w) * tay.rotation;
    }

    void CapNhatCau()
    {
        DatCau(cauLua, BanTayLua, BanTayLua == banTayTrai ? LongTayTrai : LongTayPhai, 0f);
        DatCau(cauBang, BanTayBang, BanTayBang == banTayTrai ? LongTayTrai : LongTayPhai, 2.1f);
    }

    void DatCau(Transform cau, Transform tay, Vector3 longCucBo, float pha)
    {
        if (cau == null || tay == null) return;
        Vector3 tam = tay.TransformPoint(TamLongTay);
        Vector3 len = tay.TransformDirection(longCucBo).normalized;
        float nhun = Mathf.Sin(Time.time * 2.1f + pha) * 0.015f;
        cau.position = tam + len * (BanKinhCau + CachLongTay + nhun);
    }

    void OnDestroy()
    {
        if (cauLua != null) Destroy(cauLua.gameObject);
        if (cauBang != null) Destroy(cauBang.gameObject);
    }
}
