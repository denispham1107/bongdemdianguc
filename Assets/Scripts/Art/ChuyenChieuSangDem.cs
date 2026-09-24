using UnityEngine;

/// <summary>
/// ACT2: VONG NGAY DEM LAP LAI - DEM 4 PHUT -> NGAY 2 PHUT -> CHIEU 2 PHUT -> DEM ... cho toi het tran.
///
/// Lich su yeu cau:
///   19/09/2026 vao tran luc xe chieu, 2 phut toi dan thanh dem;
///   24/09/2026 vao tran ban ngay -> 2 phut xe chieu -> 2 phut nua dem (roi dung o dem);
///   25/09/2026 (hien hanh) "vua moi bat dau game se la ban dem trong 4ph roi chuyen sang ban ngay trong 2ph roi
///              chuyen sang buoi chieu 2ph roi cu lap di lap lai nhu ban dau cho toi khi game ket thuc" - nguoi
///              dung chon "chuyen muot 30 giay": moi buoi GIU NGUYEN anh sang cua no, 30 giay CUOI moi chuyen
///              dan sang buoi sau (binh minh, chieu xuong, hoang hon).
///
/// Mot vong 8 phut (<see cref="ChuKy"/>), tinh tu luc nap man:
///     0 - 210 s  DEM         | 210 - 240 s  binh minh (dem -> ngay)
///   240 - 330 s  NGAY        | 330 - 360 s  chieu xuong (ngay -> chieu)
///   360 - 450 s  CHIEU       | 450 - 480 s  hoang hon (chieu -> dem)   roi quay lai 0.
/// Chi Act2 - Act1 va man chinh giu nguyen.
///
/// CACH LAM: ba bo anh sang <see cref="BoAnhSang"/>. NGAY va CHIEU la hang so trong file nay; DEM thi
/// KHONG viet tay ma DOC THANG tu canh ngay luc bat dau (sau khi <see cref="WorldFactory.BuildSkyAndFog"/> va
/// <see cref="WorldFactory.SetupMoonlight"/> da dat xong) - sau nay ai chinh mau ban dem trong WorldFactory
/// thi cai dich o day tu di theo.
///
/// Doi nhung gi (deu la anh sang THOI GIAN THUC - Act2 khong nuong lightmap nao):
///   - den huong (Moonlight): mau, do manh, do dam cua bong, va GOC CHIEU (ngay 55 do, chieu 13 do, dem 42 do);
///   - anh sang moi truong ba tang (Trilight), mau va do dam cua suong mu;
///   - bau troi (shader Diablo25D/SkyGradient): dinh troi / chan troi / may, sao, dia sang tren troi;
///   - den diem bam theo nhan vat (HeroLight).
///
/// LO LUA DA chi chay khi troi toi (nguoi dung 19/09/2026): vao tran la dem nen lo chay ngay tu dau; binh minh
/// sang qua <see cref="MucNhomLua"/> thi TAT ca muoi lo, hoang hon toi qua muc ay thi NHOM lai - moi lan qua muc
/// chi lam MOT lan (goi Chay() moi khung se nhom lai ca cai lo Gio loc vua dap tat).
///
/// Dong ho: <see cref="Time.timeSinceLevelLoad"/> - moi may tu chay tu luc nap man. Ca phong vao tran
/// cung luc (dem nguoc 10 giay o sanh) nen moi nguoi thay gan nhu cung mot khung troi; khong ton goi tin nao.
/// </summary>
public class ChuyenChieuSangDem : MonoBehaviour
{
    /// <summary>Do dai tung buoi, giay - nguoi dung chot 25/09/2026.</summary>
    public const float GiayDem = 240f, GiayNgay = 120f, GiayChieu = 120f;
    /// <summary>Moi lan doi buoi chuyen dan trong 30 giay CUOI cua buoi truoc (nguoi dung chon).</summary>
    public const float GiayChuyenBuoi = 30f;
    /// <summary>Mot vong day du: dem + ngay + chieu = 8 phut.</summary>
    public const float ChuKy = GiayDem + GiayNgay + GiayChieu;

    /// <summary>Moc bat dau cua ngay / chieu trong vong (giay).</summary>
    public const float MocNgay = GiayDem, MocChieu = GiayDem + GiayNgay;

    /// <summary>Mot giay GIUA moi buoi (anh sang dung yen) - phep thu dung de dat canh ve mot buoi.</summary>
    public const float GiayGiuaDem = 100f, GiayGiuaNgay = MocNgay + 45f, GiayGiuaChieu = MocChieu + 45f;

    /// <summary>Mot bo anh sang day du - de noi suy moi lan chuyen bang CUNG mot doan code.</summary>
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
                bong = Mathf.Lerp(a.bong, b.bong, k),
                // Goc: SLERP quaternion chu khong Lerp tung goc Euler - dem doc tu canh ghi goc dang 0..360,
                // Lerp Euler thi co the mat troi quay nguoc ca vong trong 30 giay binh minh
                goc = Quaternion.Slerp(Quaternion.Euler(a.goc), Quaternion.Euler(b.goc), k).eulerAngles,
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

    // ---------------- BAN NGAY ----------------
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

    // ---------------- BUOI CHIEU ----------------
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
    /// Do toi (0 = ngay/chieu, 1 = dem han) ma tu do tro len LO LUA DA chay. 0,70 roi vao khoang troi da toi han -
    /// anh sang moi truong con mot nua, sao da hien ro - chu khong phai luc con quang do o chan troi.
    /// </summary>
    public const float MucNhomLua = 0.70f;

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

    /// <summary>Giay (trong vong) luc hoang hon toi qua muc nhom lua - phep thu doc, khong tu tinh lai.</summary>
    public static float GiayNhomLua() { return ChuKy - GiayChuyenBuoi + GiayChuyenBuoi * DaoSmoothstep(MucNhomLua); }
    /// <summary>Giay (trong vong) luc binh minh sang qua muc nhom lua - lo tat.</summary>
    public static float GiayTatLua() { return MocNgay - GiayChuyenBuoi + GiayChuyenBuoi * DaoSmoothstep(1f - MucNhomLua); }

    /// <summary>Ten buoi o giay <paramref name="giay"/> (khong dau - chi de ghi bao cao phep thu).</summary>
    public static string TenBuoi(float giay)
    {
        float g = Mathf.Repeat(giay, ChuKy);
        if (g < MocNgay - GiayChuyenBuoi) return "dem";
        if (g < MocNgay) return "binh minh";
        if (g < MocChieu - GiayChuyenBuoi) return "ngay";
        if (g < MocChieu) return "chieu xuong";
        if (g < ChuKy - GiayChuyenBuoi) return "chieu";
        return "hoang hon";
    }

    // ---------------- BAN DEM (doc tu canh luc bat dau) ----------------
    Light den, heroLight;
    Material troi;
    BoAnhSang dem;

    /// <summary>
    /// ⚠️ DANG CHAY MOT KICH BAN CHAY THU thi DUNG YEN O BAN DEM, khong chay vong.
    ///
    /// Hang chuc phep thu chup anh trong Act2 va so voi anh doi chung; vong ngay dem ma chay thi phep thu nao
    /// dai qua 3,5 phut se sang ban ngay giua chung va bao hong nhung thu chang lien quan gi den anh sang.
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

    /// <summary>Dua canh ve ban dem va dung vong (goi tu ChayThuMang khi no thuc day sau Start).</summary>
    public static void ToiDemNgay()
    {
        var c = FindAnyObjectByType<ChuyenChieuSangDem>();
        if (c == null || !c.sanSang || ChoPhepChuyenTrongPhepThu) return;
        c.ApGiay(GiayGiuaDem);
        c.enabled = false;
    }

    float batDau;
    bool sanSang;
    /// <summary>Lo lua dang o trang thai CHAY (theo vong ngay dem) - chi doi khi do toi vuot qua MucNhomLua.</summary>
    bool loDangChay = true;
    /// <summary>Buoi dung yen da dat xong (khoi ghi lai moi khung): -1 = chua / dang chuyen, 0 dem, 1 ngay, 2 chieu.</summary>
    int buoiDaDat = -1;

    /// <summary>Giay da troi trong vong hien tai (0..480). Phep thu doc.</summary>
    public float GiayTrongVong { get { return Mathf.Repeat(Time.timeSinceLevelLoad - batDau, ChuKy); } }

    /// <summary>Lo lua dang duoc phep chay theo vong ngay dem (phep thu doc).</summary>
    public bool LoDangChay { get { return loDangChay; } }

    /// <summary>Bo anh sang ban dem da doc tu canh (phep thu doc de so).</summary>
    public BoAnhSang Dem { get { return dem; } }

    /// <summary>Gan vao mot vat the trong canh Act2. Goi SAU khi bau troi va anh trang da dat xong.</summary>
    public static ChuyenChieuSangDem Gan(GameObject cho, Light denHuong)
    {
        // Vao tran la BAN DEM nen lo chay ngay tu dau. Dat co NGAY TRONG AWAKE cua GameBootstrap, truoc moi Start:
        // cac lo lua trong canh doc no o Start cua chung (co la static, con giu gia tri cua tran truoc).
        LoLuaDa.ChoPhepNhomLua = true;

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

        // ---- DOC trang thai DEM tu chinh canh (thieu thu gi thi lay tam cua buoi chieu) ----
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
        loDangChay = LoLuaDa.ChoPhepNhomLua;

        ApGiay(0f);
        if (DangChayThu()) { ApGiay(GiayGiuaDem); enabled = false; }
    }

    Color DocMau(string ten, Color neuThieu) { return troi.HasProperty(ten) ? troi.GetColor(ten) : neuThieu; }
    float DocSo(string ten, float neuThieu) { return troi.HasProperty(ten) ? troi.GetFloat(ten) : neuThieu; }

    void DatMau(string ten, Color c) { if (troi.HasProperty(ten)) troi.SetColor(ten, c); }
    void DatSo(string ten, float v) { if (troi.HasProperty(ten)) troi.SetFloat(ten, v); }

    void Update()
    {
        if (!sanSang) return;
        ApGiay(GiayTrongVong);
    }

    /// <summary>
    /// Tinh bo anh sang va DO TOI (0..1) o giay <paramref name="giay"/> trong vong. Moi lan chuyen buoi di theo
    /// smoothstep rieng: dau va cuoi doi cham, khuc giua doi nhanh.
    /// </summary>
    public BoAnhSang TinhLuc(float giay, out float doToi, out int buoiDungYen)
    {
        float g = Mathf.Repeat(giay, ChuKy);
        buoiDungYen = -1;
        BoAnhSang b;
        if (g < MocNgay - GiayChuyenBuoi) { b = dem; doToi = 1f; buoiDungYen = 0; }
        else if (g < MocNgay)
        {
            // BINH MINH: sao tat trong NUA DAU (troi da xanh ma con day sao thi trong gia)
            float k = Smoothstep((g - (MocNgay - GiayChuyenBuoi)) / GiayChuyenBuoi);
            b = BoAnhSang.Lerp(dem, Ngay, k);
            b.sao = Mathf.Lerp(dem.sao, Ngay.sao, Mathf.Clamp01(k / 0.55f));
            doToi = 1f - k;
        }
        else if (g < MocChieu - GiayChuyenBuoi) { b = Ngay; doToi = 0f; buoiDungYen = 1; }
        else if (g < MocChieu)
        {
            float k = Smoothstep((g - (MocChieu - GiayChuyenBuoi)) / GiayChuyenBuoi);
            b = BoAnhSang.Lerp(Ngay, XeChieu, k);
            doToi = 0f;
        }
        else if (g < ChuKy - GiayChuyenBuoi) { b = XeChieu; doToi = 0f; buoiDungYen = 2; }
        else
        {
            // HOANG HON: sao chi hien o NUA SAU (troi con do quang o chan troi ma da day sao thi trong gia)
            float k = Smoothstep((g - (ChuKy - GiayChuyenBuoi)) / GiayChuyenBuoi);
            b = BoAnhSang.Lerp(XeChieu, dem, k);
            b.sao = Mathf.Lerp(XeChieu.sao, dem.sao, Mathf.Clamp01((k - 0.45f) / 0.55f));
            doToi = k;
        }
        return b;
    }

    /// <summary>Dat anh sang (va lo lua) dung nhu o giay <paramref name="giay"/> trong vong.</summary>
    public void ApGiay(float giay)
    {
        float doToi; int buoi;
        var b = TinhLuc(giay, out doToi, out buoi);

        // LO LUA DA: chi doi khi do toi VUOT QUA muc - moi lan qua muc lam mot lan
        bool nenChay = doToi >= MucNhomLua;
        if (nenChay != loDangChay)
        {
            loDangChay = nenChay;
            LoLuaDa.ChoPhepNhomLua = nenChay;
            foreach (var lo in FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None))
            {
                if (nenChay) lo.Chay();
                else lo.DapTat();
            }
        }

        // Buoi dung yen da dat roi thi khoi ghi lai moi khung (chi Update goi lien tuc; phep thu goi lung tung
        // thi buoiDaDat van dung vi moi lan dat deu ghi lai)
        if (buoi >= 0 && buoi == buoiDaDat) return;
        buoiDaDat = buoi;

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
