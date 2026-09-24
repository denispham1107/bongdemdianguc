using UnityEngine;

/// <summary>
/// ACT2: VAO TRAN LUC BAN NGAY -> 2 PHUT SAU LA XE CHIEU -> 2 PHUT NUA LA DEM.
///
/// Nguoi dung xin hai lan:
///   19/09/2026 "moi vao game ... anh sang buoi xe chieu roi trong vong 2 phut chuyen dan anh sang tu tu qua
///              dem toi giong hien gio";
///   24/09/2026 "luc moi vao game cho them anh sang ban ngay roi trong vong 2 phut chuyen dan anh sang sang
///              buoi xe chieu" - va nguoi dung chot: sau do xe chieu van toi dan thanh dem trong 2 phut nua
///              (tong 4 phut: NGAY 0 s -> XE CHIEU 120 s -> DEM 240 s).
/// Chi Act2 - Act1 va man chinh giu nguyen.
///
/// CACH LAM: ba bo anh sang <see cref="BoAnhSang"/>. NGAY va XE CHIEU la hang so trong file nay; DEM thi
/// KHONG viet tay ma DOC THANG tu canh ngay luc bat dau (sau khi <see cref="WorldFactory.BuildSkyAndFog"/> va
/// <see cref="WorldFactory.SetupMoonlight"/> da dat xong) - sau nay ai chinh mau ban dem trong WorldFactory
/// thi cai dich o day tu di theo, khong co chuyen hai noi giu hai bang mau roi lech nhau (dung cai loi bo
/// bieu tuong ky nang da mac 19/09/2026).
///
/// Doi nhung gi (deu la anh sang THOI GIAN THUC - Act2 khong nuong lightmap nao, do duoc 0 lightmap):
///   - den huong (Moonlight): mau, do manh, do dam cua bong, va GOC CHIEU (trua mat troi cao 55 do,
///     xe chieu ha thap 13 do nen bong do dai va xien, dem len 42 do nhu anh trang hien gio);
///   - anh sang moi truong ba tang (Trilight), mau va do dam cua suong mu;
///   - bau troi (shader Diablo25D/SkyGradient): mau dinh troi / chan troi / may, do sang cua sao, va
///     dia sang tren troi - MAT TROI trang vang, roi mat troi cam to, cuoi cung MAT TRANG trang nho;
///   - den diem bam theo nhan vat (HeroLight): ban ngay gan nhu tat, dem thi xanh lanh nhu cu.
///
/// Dong ho: <see cref="Time.timeSinceLevelLoad"/> - moi may tu chay tu luc nap man. Ca phong vao tran
/// cung luc (dem nguoc 10 giay o sanh) nen moi nguoi thay gan nhu cung mot khung troi; khong ton goi tin nao.
/// </summary>
public class ChuyenChieuSangDem : MonoBehaviour
{
    /// <summary>Moi chang 2 phut - nguoi dung chot.</summary>
    public const float GiayMoiChang = 120f;
    /// <summary>Hai chang: ngay -> xe chieu, xe chieu -> dem.</summary>
    public const float GiayChuyen = GiayMoiChang * 2f;
    /// <summary>Tien do (0..1 tren ca 4 phut) ung voi dung luc XE CHIEU.</summary>
    public const float TienDoXeChieu = 0.5f;

    /// <summary>Mot bo anh sang day du - de noi suy hai chang bang CUNG mot doan code.</summary>
    public struct BoAnhSang
    {
        public Color den; public float manh, bong; public Vector3 goc;
        public Color ambTroi, ambNgang, ambDat, suong; public float damSuong;
        public Color troiDinh, troiChan, troiDuoi, may; public float damMay, sao;
        public Color dia; public float coDia, quangDia, sangDia;
        public Color hero; public float manhHero;

        public static BoAnhSang Lerp(BoAnhSang a, BoAnhSang b, float k)
        {
            return new BoAnhSang
            {
                den = Color.Lerp(a.den, b.den, k), manh = Mathf.Lerp(a.manh, b.manh, k),
                bong = Mathf.Lerp(a.bong, b.bong, k), goc = Vector3.Lerp(a.goc, b.goc, k),
                ambTroi = Color.Lerp(a.ambTroi, b.ambTroi, k), ambNgang = Color.Lerp(a.ambNgang, b.ambNgang, k),
                ambDat = Color.Lerp(a.ambDat, b.ambDat, k), suong = Color.Lerp(a.suong, b.suong, k),
                damSuong = Mathf.Lerp(a.damSuong, b.damSuong, k),
                troiDinh = Color.Lerp(a.troiDinh, b.troiDinh, k), troiChan = Color.Lerp(a.troiChan, b.troiChan, k),
                troiDuoi = Color.Lerp(a.troiDuoi, b.troiDuoi, k), may = Color.Lerp(a.may, b.may, k),
                damMay = Mathf.Lerp(a.damMay, b.damMay, k), sao = Mathf.Lerp(a.sao, b.sao, k),
                dia = Color.Lerp(a.dia, b.dia, k), coDia = Mathf.Lerp(a.coDia, b.coDia, k),
                quangDia = Mathf.Lerp(a.quangDia, b.quangDia, k), sangDia = Mathf.Lerp(a.sangDia, b.sangDia, k),
                hero = Color.Lerp(a.hero, b.hero, k), manhHero = Mathf.Lerp(a.manhHero, b.manhHero, k),
            };
        }
    }

    // ---------------- BAN NGAY (vua vao tran) ----------------
    // Nang trua nhat nhat cua nghia dia: mat troi cao, anh sang trang hoi am, troi xanh nhat, suong xam
    // xanh thua - van u am mot chut chu khong ruc ro nhu dong co.
    public static readonly BoAnhSang Ngay = new BoAnhSang
    {
        den = new Color(1.00f, 0.95f, 0.86f), manh = 1.35f, bong = 0.70f, goc = new Vector3(55f, 148f, 0f),
        ambTroi = new Color(0.55f, 0.62f, 0.72f), ambNgang = new Color(0.50f, 0.52f, 0.52f),
        ambDat = new Color(0.30f, 0.28f, 0.24f),
        suong = new Color(0.62f, 0.68f, 0.74f), damSuong = 0.0060f,
        troiDinh = new Color(0.30f, 0.48f, 0.78f), troiChan = new Color(0.72f, 0.80f, 0.86f),
        troiDuoi = new Color(0.25f, 0.25f, 0.25f), may = new Color(0.88f, 0.90f, 0.94f), damMay = 0.55f, sao = 0f,
        dia = new Color(1.00f, 0.95f, 0.80f), coDia = 0.045f, quangDia = 0.45f, sangDia = 2.2f,
        hero = new Color(1.00f, 0.95f, 0.88f), manhHero = 0.15f,
    };

    // ---------------- XE CHIEU (sau 2 phut) ----------------
    // Nang cuoi ngay: cam do o chan troi, dinh troi da nga xanh tham, chua co sao. Mat troi THAP (13 do)
    // nen bong do dai va xien. (Cung bo so da chot 19/09/2026 cho "vao tran luc xe chieu".)
    public static readonly BoAnhSang XeChieu = new BoAnhSang
    {
        den = new Color(1.00f, 0.64f, 0.34f), manh = 1.45f, bong = 0.80f, goc = new Vector3(13f, 148f, 0f),
        ambTroi = new Color(0.46f, 0.34f, 0.28f), ambNgang = new Color(0.42f, 0.27f, 0.18f),
        ambDat = new Color(0.24f, 0.14f, 0.09f),
        suong = new Color(0.60f, 0.36f, 0.24f), damSuong = 0.0072f,
        troiDinh = new Color(0.115f, 0.150f, 0.300f), troiChan = new Color(0.960f, 0.420f, 0.170f),
        troiDuoi = new Color(0.200f, 0.110f, 0.080f), may = new Color(0.850f, 0.440f, 0.270f), damMay = 0.62f, sao = 0f,
        dia = new Color(1.00f, 0.66f, 0.30f), coDia = 0.060f, quangDia = 0.55f, sangDia = 1.80f,
        hero = new Color(1.00f, 0.82f, 0.62f), manhHero = 0.35f,
    };

    /// <summary>
    /// TOI DAY THI NHOM LUA O CAC LO DA (nguoi dung: "khi khong phai ban dem thi cho tat lua o Lo Lua").
    /// Do tren duong cong da lam muot cua CHANG XE CHIEU -> DEM: 0,70 roi vao khoang troi da toi han -
    /// anh sang moi truong con mot nua, sao da hien ro - chu khong phai luc con quang do o chan troi.
    /// Ca chang ban ngay lan xe chieu lo deu tat.
    /// </summary>
    public const float MucNhomLua = 0.70f;

    /// <summary>Tien do (0..1 tren ca 4 phut) luc lo bat dau chay - phep thu doc, khong tu tinh lai.</summary>
    public static float TienDoNhomLua()
    {
        return TienDoXeChieu + (1f - TienDoXeChieu) * DaoSmoothstep(MucNhomLua);
    }

    static float Smoothstep(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

    /// <summary>Tim x sao cho smoothstep(x) = y (chia doi 40 lan - co tran, khong vong vo han).</summary>
    static float DaoSmoothstep(float y)
    {
        y = Mathf.Clamp01(y);
        float lo = 0f, hi = 1f;
        for (int i = 0; i < 40; i++)
        {
            float giua = (lo + hi) * 0.5f;
            if (Smoothstep(giua) < y) lo = giua; else hi = giua;
        }
        return (lo + hi) * 0.5f;
    }

    // ---------------- BAN DEM (doc tu canh luc bat dau) ----------------
    Light den, heroLight;
    Material troi;
    BoAnhSang dem;

    /// <summary>
    /// ⚠️ DANG CHAY MOT KICH BAN CHAY THU thi NHAY THANG toi dem, khong chuyen dan.
    ///
    /// Hang chuc phep thu chup anh trong Act2 o nhung giay dau tran (mau hat lua, do sang vanh hinh,
    /// mau vo bang...). De troi sang ban ngay thi moi anh doi chung ay deu lech, va chung se bao hong
    /// nhung thu chang lien quan gi den anh sang.
    ///
    /// Nhan ra "dang chay thu" bang su co mat cua <see cref="ChayThuMang"/>. Phai kiem o CA HAI dau vi
    /// thu tu khong co dinh: vat the chay thu duoc tao tu EditorApplication.update, co the truoc hoac
    /// SAU Start cua lop nay - nen <see cref="ChayThuMang"/> con goi thang <see cref="ToiDemNgay"/>.
    ///
    /// Menu 79 - phep thu CUA chinh tinh nang nay - bat <see cref="ChoPhepChuyenTrongPhepThu"/>.
    /// </summary>
    public static bool ChoPhepChuyenTrongPhepThu;

    static bool DangChayThu()
    {
        return !ChoPhepChuyenTrongPhepThu && FindAnyObjectByType<ChayThuMang>() != null;
    }

    /// <summary>Dua canh ve han ban dem ngay lap tuc (goi tu ChayThuMang khi no thuc day sau Start).</summary>
    public static void ToiDemNgay()
    {
        var c = FindAnyObjectByType<ChuyenChieuSangDem>();
        if (c == null || !c.sanSang || ChoPhepChuyenTrongPhepThu) return;
        c.Ap(1f);
        c.enabled = false;
    }

    float batDau;
    bool sanSang;
    bool daNhomLua;

    /// <summary>Phan duong da di tren ca 4 phut (0 = vua vao tran, 0,5 = xe chieu, 1 = dem han). Phep thu doc.</summary>
    public float TienDo
    {
        get { return GiayChuyen <= 0f ? 1f : Mathf.Clamp01((Time.timeSinceLevelLoad - batDau) / GiayChuyen); }
    }

    /// <summary>Bo anh sang ban dem da doc tu canh (phep thu doc de so).</summary>
    public BoAnhSang Dem { get { return dem; } }

    /// <summary>Gan vao mot vat the trong canh Act2. Goi SAU khi bau troi va anh trang da dat xong.</summary>
    public static ChuyenChieuSangDem Gan(GameObject cho, Light denHuong)
    {
        // Dat co NGAY TRONG AWAKE cua GameBootstrap, truoc moi Start: cac lo lua trong canh se doc no
        // o Start cua chung. Dang chay phep thu (nhay thang toi dem) thi cu de lua chay nhu cu.
        LoLuaDa.ChoPhepNhomLua = DangChayThu();

        var c = cho.AddComponent<ChuyenChieuSangDem>();
        c.den = denHuong;
        return c;
    }

    void Start()
    {
        if (den == null)
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { den = l; break; }

        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Point && l.name == "HeroLight") { heroLight = l; break; }

        // Bau troi: LAM BAN SAO roi moi sua. Material cua RenderSettings co the la thu nam trong scene -
        // sua thang vao no la canh cua nguoi dung ban sau khi thoat Play.
        if (RenderSettings.skybox != null)
        {
            troi = new Material(RenderSettings.skybox);
            troi.name = "Sky_NgayChieuDem";
            RenderSettings.skybox = troi;
        }

        // ---- DOC trang thai DEM tu chinh canh (thieu thu gi thi lay tam cua xe chieu) ----
        dem = XeChieu;
        if (den != null)
        {
            dem.den = den.color; dem.manh = den.intensity; dem.bong = den.shadowStrength;
            dem.goc = den.transform.eulerAngles;
        }
        if (heroLight != null) { dem.hero = heroLight.color; dem.manhHero = heroLight.intensity; }
        dem.ambTroi = RenderSettings.ambientSkyColor;
        dem.ambNgang = RenderSettings.ambientEquatorColor;
        dem.ambDat = RenderSettings.ambientGroundColor;
        dem.suong = RenderSettings.fogColor;
        dem.damSuong = RenderSettings.fogDensity;
        if (troi != null)
        {
            dem.troiDinh = DocMau("_TopColor", dem.troiDinh);
            dem.troiChan = DocMau("_HorizColor", dem.troiChan);
            dem.troiDuoi = DocMau("_BottomColor", dem.troiDuoi);
            dem.may = DocMau("_CloudColor", dem.may);
            dem.damMay = DocSo("_CloudAmount", dem.damMay);
            dem.sao = DocSo("_StarAmount", 1f);
            dem.dia = DocMau("_MoonColor", dem.dia);
            dem.coDia = DocSo("_MoonSize", 0.024f);
            dem.quangDia = DocSo("_MoonGlow", 0.30f);
            dem.sangDia = DocSo("_MoonStrength", 0.9f);
        }

        batDau = Time.timeSinceLevelLoad;
        sanSang = true;
        daNhomLua = false;

        if (DangChayThu()) { Ap(1f); enabled = false; return; }
        Ap(0f);
    }

    Color DocMau(string ten, Color neuThieu) { return troi.HasProperty(ten) ? troi.GetColor(ten) : neuThieu; }
    float DocSo(string ten, float neuThieu) { return troi.HasProperty(ten) ? troi.GetFloat(ten) : neuThieu; }

    void DatMau(string ten, Color c) { if (troi.HasProperty(ten)) troi.SetColor(ten, c); }
    void DatSo(string ten, float v) { if (troi.HasProperty(ten)) troi.SetFloat(ten, v); }

    void Update()
    {
        if (!sanSang) return;
        Ap(TienDo);
        if (TienDo >= 1f) { Ap(1f); enabled = false; }     // toi dem roi thi thoi, khong tinh moi khung nua
    }

    /// <summary>
    /// Dat anh sang o mot diem tren ca 4 phut. <paramref name="t"/> 0 = ban ngay, 0,5 = xe chieu, 1 = dem.
    /// Moi chang di theo duong cong smoothstep rieng: dau chang va cuoi chang doi cham, khuc giua doi
    /// nhanh - nhin ra "nang nga dan" / "troi sap toi" chu khong phai mot cai van vo deu deu, va luc giao
    /// hai chang (dung xe chieu) anh sang dung lai mot nhip chu khong bi gay.
    /// </summary>
    public void Ap(float t)
    {
        t = Mathf.Clamp01(t);
        BoAnhSang b;
        float kDem;                      // do tien tren CHANG XE CHIEU -> DEM (0 o ca chang ban ngay)
        if (t <= TienDoXeChieu)
        {
            b = BoAnhSang.Lerp(Ngay, XeChieu, Smoothstep(t / TienDoXeChieu));
            kDem = 0f;
        }
        else
        {
            kDem = Smoothstep((t - TienDoXeChieu) / (1f - TienDoXeChieu));
            b = BoAnhSang.Lerp(XeChieu, dem, kDem);
            // Sao chi hien o NUA SAU chang cuoi: troi con do quang o chan troi ma da day sao thi trong rat gia
            b.sao = Mathf.Lerp(XeChieu.sao, dem.sao, Mathf.Clamp01((kDem - 0.45f) / 0.55f));
        }

        // LO LUA DA: chua toi dem thi de tat. Chi nhom MOT LAN khi vuot muc - goi Chay() moi khung se
        // nhom lai ca cai lo ma Gio loc vua dap tat (LoLuaDa.DapTatRoiChayLai hen 30 giay).
        if (kDem >= MucNhomLua && !daNhomLua)
        {
            daNhomLua = true;
            LoLuaDa.ChoPhepNhomLua = true;
            foreach (var lo in FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None)) lo.Chay();
        }

        if (den != null)
        {
            den.color = b.den;
            den.intensity = b.manh;
            den.shadowStrength = b.bong;
            den.transform.rotation = Quaternion.Euler(b.goc);
        }
        if (heroLight != null)
        {
            heroLight.color = b.hero;
            heroLight.intensity = b.manhHero;
        }

        RenderSettings.ambientSkyColor = b.ambTroi;
        RenderSettings.ambientEquatorColor = b.ambNgang;
        RenderSettings.ambientGroundColor = b.ambDat;
        RenderSettings.fogColor = b.suong;
        RenderSettings.fogDensity = b.damSuong;

        if (troi != null)
        {
            DatMau("_TopColor", b.troiDinh);
            DatMau("_HorizColor", b.troiChan);
            DatMau("_BottomColor", b.troiDuoi);
            DatMau("_CloudColor", b.may);
            DatSo("_CloudAmount", b.damMay);
            DatSo("_StarAmount", b.sao);
            DatMau("_MoonColor", b.dia);
            DatSo("_MoonSize", b.coDia);
            DatSo("_MoonGlow", b.quangDia);
            DatSo("_MoonStrength", b.sangDia);

            // Dia sang phai nam DUNG huong den dang chieu toi (day la huong NHIN TOI no, nguoc voi
            // forward cua den) - khong thi mat troi mot noi, bong do mot neo.
            if (den != null && troi.HasProperty("_MoonDir"))
            {
                Vector3 f = den.transform.forward;
                troi.SetVector("_MoonDir", new Vector4(-f.x, -f.y, -f.z, 0f));
            }
        }
    }
}
