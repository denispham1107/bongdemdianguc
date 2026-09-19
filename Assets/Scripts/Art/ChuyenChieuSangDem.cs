using UnityEngine;

/// <summary>
/// ACT2: VAO TRAN LUC XE CHIEU, ROI TOI DAN THANH DEM TRONG 2 PHUT.
///
/// Nguoi dung xin 19/09/2026: "moi vao game ... anh sang buoi xe chieu roi trong vong 2 phut chuyen dan
/// anh sang tu tu qua dem toi giong hien gio". Chi Act2 - Act1 va man chinh giu nguyen.
///
/// CACH LAM: trang thai DEM khong viet tay o day ma DOC THANG tu canh ngay luc bat dau (sau khi
/// <see cref="WorldFactory.BuildSkyAndFog"/> va <see cref="WorldFactory.SetupMoonlight"/> da dat xong).
/// Nho vay sau nay ai chinh mau ban dem trong WorldFactory thi cai dich o day tu dong di theo - khong co
/// chuyen hai noi giu hai bang mau roi lech nhau (dung cai loi bo bieu tuong ky nang da mac 19/09/2026).
/// Chi co trang thai XE CHIEU la hang so trong file nay.
///
/// Doi nhung gi (deu la anh sang THOI GIAN THUC - Act2 khong nuong lightmap nao, do duoc 0 lightmap):
///   - den huong (Moonlight): mau, do manh, do dam cua bong, va GOC CHIEU (chieu thi mat troi thap nen
///     bong do dai va xien; toi dan thi den len cao 42 do nhu anh trang hien gio);
///   - anh sang moi truong ba tang (Trilight), mau va do dam cua suong mu;
///   - bau troi (shader Diablo25D/SkyGradient): mau dinh troi / chan troi / may, do sang cua sao,
///     va dia sang tren troi - lucs dau la MAT TROI cam to, cuoi cung la MAT TRANG trang nho;
///   - den diem bam theo nhan vat (HeroLight): chieu thi gan nhu khong thay, dem thi xanh lanh nhu cu.
///
/// Dong ho: <see cref="Time.timeSinceLevelLoad"/> - moi may tu chay tu luc nap man. Ca phong vao tran
/// cung luc (dem nguoc 10 giay o sanh) nen moi nguoi thay gan nhu cung mot khung troi; khong ton goi tin nao.
/// </summary>
public class ChuyenChieuSangDem : MonoBehaviour
{
    /// <summary>Hai phut - nguoi dung chot.</summary>
    public const float GiayChuyen = 120f;

    // ---------------- Trang thai XE CHIEU (diem xuat phat) ----------------
    // Nang cuoi ngay: cam do o chan troi, dinh troi da nga xanh tham, chua co sao.
    public static readonly Color DenChieu = new Color(1.00f, 0.64f, 0.34f);
    public const float ManhChieu = 1.45f;
    public const float BongChieu = 0.80f;
    /// <summary>Goc chieu luc xe chieu: mat troi THAP (13 do) nen bong do dai va xien.</summary>
    public static readonly Vector3 GocChieu = new Vector3(13f, 148f, 0f);

    public static readonly Color AmbientTroiChieu = new Color(0.46f, 0.34f, 0.28f);
    public static readonly Color AmbientNgangChieu = new Color(0.42f, 0.27f, 0.18f);
    public static readonly Color AmbientDatChieu = new Color(0.24f, 0.14f, 0.09f);

    public static readonly Color SuongChieu = new Color(0.60f, 0.36f, 0.24f);
    /// <summary>Chieu con nhin xa hon dem: suong thua hon.</summary>
    public const float DamSuongChieu = 0.0072f;

    public static readonly Color TroiDinhChieu = new Color(0.115f, 0.150f, 0.300f);
    public static readonly Color TroiChanChieu = new Color(0.960f, 0.420f, 0.170f);
    public static readonly Color TroiDuoiChieu = new Color(0.200f, 0.110f, 0.080f);
    public static readonly Color MayChieu = new Color(0.850f, 0.440f, 0.270f);
    public const float DamMayChieu = 0.62f;
    /// <summary>Chua co sao luc chieu.</summary>
    public const float SaoChieu = 0f;
    /// <summary>Dia MAT TROI: cam, to va sang hon dia trang ban dem.</summary>
    public static readonly Color DiaChieu = new Color(1.00f, 0.66f, 0.30f);
    public const float CoDiaChieu = 0.060f;
    public const float QuangDiaChieu = 0.55f;
    public const float SangDiaChieu = 1.80f;

    /// <summary>Den diem bam theo nhan vat: luc chieu troi con sang nen ha xuong cho khoi loe.</summary>
    public static readonly Color HeroChieu = new Color(1.00f, 0.82f, 0.62f);
    public const float ManhHeroChieu = 0.35f;

    // ---------------- Trang thai DEM (doc tu canh luc bat dau) ----------------
    Light den, heroLight;
    Material troi;
    Color denDem, ambTroiDem, ambNgangDem, ambDatDem, suongDem, heroDem;
    float manhDem, bongDem, damSuongDem, manhHeroDem;
    Vector3 gocDem;
    Color troiDinhDem, troiChanDem, troiDuoiDem, mayDem, diaDem;
    float damMayDem, saoDem, coDiaDem, quangDiaDem, sangDiaDem;

    /// <summary>
    /// ⚠️ DANG CHAY MOT KICH BAN CHAY THU thi NHAY THANG toi dem, khong chuyen dan.
    ///
    /// Hang chuc phep thu chup anh trong Act2 o nhung giay dau tran (mau hat lua, do sang vanh hinh,
    /// mau vo bang...). De troi cam luc xe chieu thi moi anh doi chung ay deu lech, va chung se bao
    /// hong nhung thu chang lien quan gi den anh sang.
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

    /// <summary>Phan duong da di (0 = vua vao tran, 1 = da thanh dem han). Phep thu doc.</summary>
    public float TienDo
    {
        get { return GiayChuyen <= 0f ? 1f : Mathf.Clamp01((Time.timeSinceLevelLoad - batDau) / GiayChuyen); }
    }

    /// <summary>Gan vao mot vat the trong canh Act2. Goi SAU khi bau troi va anh trang da dat xong.</summary>
    public static ChuyenChieuSangDem Gan(GameObject cho, Light denHuong)
    {
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
            troi.name = "Sky_ChieuToiDem";
            RenderSettings.skybox = troi;
        }

        // ---- DOC trang thai DEM tu chinh canh ----
        if (den != null)
        {
            denDem = den.color; manhDem = den.intensity; bongDem = den.shadowStrength;
            gocDem = den.transform.eulerAngles;
        }
        if (heroLight != null) { heroDem = heroLight.color; manhHeroDem = heroLight.intensity; }

        ambTroiDem = RenderSettings.ambientSkyColor;
        ambNgangDem = RenderSettings.ambientEquatorColor;
        ambDatDem = RenderSettings.ambientGroundColor;
        suongDem = RenderSettings.fogColor;
        damSuongDem = RenderSettings.fogDensity;

        if (troi != null)
        {
            troiDinhDem = DocMau("_TopColor", TroiDinhChieu);
            troiChanDem = DocMau("_HorizColor", TroiChanChieu);
            troiDuoiDem = DocMau("_BottomColor", TroiDuoiChieu);
            mayDem = DocMau("_CloudColor", MayChieu);
            damMayDem = DocSo("_CloudAmount", DamMayChieu);
            saoDem = DocSo("_StarAmount", 1f);
            diaDem = DocMau("_MoonColor", DiaChieu);
            coDiaDem = DocSo("_MoonSize", 0.024f);
            quangDiaDem = DocSo("_MoonGlow", 0.30f);
            sangDiaDem = DocSo("_MoonStrength", 0.9f);
        }

        batDau = Time.timeSinceLevelLoad;
        sanSang = true;

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
    /// Dat anh sang o mot diem giua duong. <paramref name="t"/> 0 = xe chieu, 1 = dem.
    /// Duong cong smoothstep: nhung giay dau va nhung giay cuoi doi cham, khuc giua doi nhanh -
    /// nhin ra "troi sap toi" chu khong phai mot cai van vo tu tu deu deu.
    /// </summary>
    public void Ap(float t)
    {
        t = Mathf.Clamp01(t);
        float k = t * t * (3f - 2f * t);

        if (den != null)
        {
            den.color = Color.Lerp(DenChieu, denDem, k);
            den.intensity = Mathf.Lerp(ManhChieu, manhDem, k);
            den.shadowStrength = Mathf.Lerp(BongChieu, bongDem, k);
            den.transform.rotation = Quaternion.Euler(Vector3.Lerp(GocChieu, gocDem, k));
        }
        if (heroLight != null)
        {
            heroLight.color = Color.Lerp(HeroChieu, heroDem, k);
            heroLight.intensity = Mathf.Lerp(ManhHeroChieu, manhHeroDem, k);
        }

        RenderSettings.ambientSkyColor = Color.Lerp(AmbientTroiChieu, ambTroiDem, k);
        RenderSettings.ambientEquatorColor = Color.Lerp(AmbientNgangChieu, ambNgangDem, k);
        RenderSettings.ambientGroundColor = Color.Lerp(AmbientDatChieu, ambDatDem, k);
        RenderSettings.fogColor = Color.Lerp(SuongChieu, suongDem, k);
        RenderSettings.fogDensity = Mathf.Lerp(DamSuongChieu, damSuongDem, k);

        if (troi != null)
        {
            DatMau("_TopColor", Color.Lerp(TroiDinhChieu, troiDinhDem, k));
            DatMau("_HorizColor", Color.Lerp(TroiChanChieu, troiChanDem, k));
            DatMau("_BottomColor", Color.Lerp(TroiDuoiChieu, troiDuoiDem, k));
            DatMau("_CloudColor", Color.Lerp(MayChieu, mayDem, k));
            DatSo("_CloudAmount", Mathf.Lerp(DamMayChieu, damMayDem, k));
            // Sao chi hien o NUA SAU: troi con do quang o chan troi ma da day sao thi trong rat gia
            DatSo("_StarAmount", Mathf.Lerp(SaoChieu, saoDem, Mathf.Clamp01((k - 0.45f) / 0.55f)));
            DatMau("_MoonColor", Color.Lerp(DiaChieu, diaDem, k));
            DatSo("_MoonSize", Mathf.Lerp(CoDiaChieu, coDiaDem, k));
            DatSo("_MoonGlow", Mathf.Lerp(QuangDiaChieu, quangDiaDem, k));
            DatSo("_MoonStrength", Mathf.Lerp(SangDiaChieu, sangDiaDem, k));

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
