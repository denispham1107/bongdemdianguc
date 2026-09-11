using UnityEngine;

/// <summary>
/// BO GIAO DIEN DUNG CHUNG cho man dang nhap, sanh cho, trong phong, dem
/// nguoc va bang cai dat - tat ca ve bang OnGUI.
///
/// PHONG CACH: game kinh di. Nen gan nhu den, vien do mau, goc khung co moc
/// sat, vet mau nho giot tu mep tren khung, chu mau giay cu. Tieu de chap
/// chon nhu anh nen. Bon goc man hinh toi lai va mot lop suong do duoi day.
///
/// FONT: Inter (Assets/Resources/Fonts, giay phep SIL OFL - file
/// Inter-LICENSE.txt di kem). KHONG dung font mac dinh cua Unity: font ay
/// THIEU cac chu a-ngang nang, e-mu nang, u-moc, o-moc... (dai U+1EA0-U+1EF9
/// va U+01A0/U+01B0). Trong Editor khong ai thay, vi Windows tu muon Arial de
/// ve bu - len ban web khong co ai cho muon, va "CAI DAT" hien thanh "CAI D T".
/// Da doc thang bang ky tu cua Inter: du ca 134 chu co dau tieng Viet.
///
/// CHU KHONG DUOC TRAN: moi nhan va nut deu di qua <see cref="Chu"/> hoac
/// <see cref="Nut"/> - chu dai hon cho thi tu thu nho, van dai thi cat va them
/// dau "…".
/// </summary>
public static class GiaoDien
{
    // ================================================================
    //  FONT
    // ================================================================

    static Font chuThuong, chuDam;

    public static Font ChuThuong { get { NapFont(); return chuThuong; } }
    public static Font ChuDam { get { NapFont(); return chuDam; } }

    static void NapFont()
    {
        if (chuThuong != null) return;
        chuThuong = Resources.Load<Font>("Fonts/Inter-Regular");
        chuDam = Resources.Load<Font>("Fonts/Inter-SemiBold");
        if (chuDam == null) chuDam = chuThuong;
        if (chuThuong == null) Debug.LogError("[GiaoDien] Khong nap duoc font Inter trong Resources/Fonts");
    }

    // ================================================================
    //  MAU
    // ================================================================

    public static readonly Color MauGiay = new Color(0.91f, 0.85f, 0.74f);     // chu chinh
    public static readonly Color MauMo = new Color(0.62f, 0.57f, 0.52f);       // chu phu
    public static readonly Color MauToi = new Color(0.42f, 0.39f, 0.37f);      // chu mo han
    public static readonly Color MauMau = new Color(0.72f, 0.07f, 0.05f);      // do mau
    public static readonly Color MauMauSang = new Color(0.95f, 0.18f, 0.11f);
    public static readonly Color MauVang = new Color(0.88f, 0.68f, 0.33f);     // chu phong
    public static readonly Color MauXanh = new Color(0.52f, 0.84f, 0.46f);     // san sang
    public static readonly Color MauLoi = new Color(1f, 0.50f, 0.42f);

    /// <summary>
    /// Ti le giao dien. Theo chieu CAO man hinh (1080 la chuan), nhung khong
    /// duoc qua chieu NGANG: dien thoai cam doc ma lay theo chieu cao thi moi
    /// thu to gap ruoi va tran ra hai ben.
    /// </summary>
    public static float TiLe { get { return TinhTiLe(Screen.width, Screen.height); } }

    /// <summary>Do duc long khung (0 = trong han, 1 = den dac). Truoc la ~0,94 -
    /// nguoi dung thay khung che mat canh phia sau.</summary>
    public const float DoDucKhung = 0.55f;
    /// <summary>Do duc long mot hang trong danh sach / the ghe.</summary>
    public const float DoDucHang = 0.45f;
    /// <summary>Bang noi tren sanh (Cai dat) duc hon: sanh mo phia sau lot qua
    /// long bang qua nhieu thi chu hai lop chong len nhau, kho doc.</summary>
    public const float DoDucBangNoi = 0.66f;

    /// <summary>Ti le cho mot man hinh W x H bat ky - phep thu dung de xep thu nhieu co man hinh.</summary>
    public static float TinhTiLe(float W, float H)
    {
        return Mathf.Max(0.35f, Mathf.Min(H / 1080f, W / 1180f));
    }

    // ---- Dem so lan chu khong vua o, de phep thu (menu 50) doc ----
    /// <summary>So lan phai CAT chu (them "…") ke tu lan dat lai cuoi.</summary>
    public static int SoLanCat;
    /// <summary>So lan phai THU NHO co chu cho vua.</summary>
    public static int SoLanThuNho;
    public static string ChuBiCatCuoi = "";
    /// <summary>So luot ve (Repaint) da di qua ChuanBi - bang 0 nghia la khong man nao ve ca,
    /// va hai bo dem tren bang 0 chang noi len dieu gi.</summary>
    public static int SoLuotVe;
    /// <summary>Nhung chu da phai thu nho (moi chu mot lan) - de phep thu ghi ra.</summary>
    public static readonly System.Collections.Generic.List<string> ChuBiThuNho = new System.Collections.Generic.List<string>();
    public static void DatLaiDem() { SoLanCat = 0; SoLanThuNho = 0; ChuBiCatCuoi = ""; SoLuotVe = 0; ChuBiThuNho.Clear(); }

    // ================================================================
    //  ANH NEN VE BANG CODE
    // ================================================================

    static Texture2D trang, nenBang, nenHang, toi4Goc, suong, giotMau, vachMo, hatKim;
    static Texture2D nutMau, nutMauSang, nutMauNhan, nutDa, nutDaSang, nutDaNhan, oNhap, oNhapSang;
    static bool daTao;

    public static Texture2D Trang { get { TaoAnh(); return trang; } }

    static Texture2D Anh(int w, int h, System.Func<int, int, Color> mau, bool lapNgang = false)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.hideFlags = HideFlags.HideAndDontSave;
        t.filterMode = FilterMode.Bilinear;
        t.wrapModeU = lapNgang ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        t.wrapModeV = TextureWrapMode.Clamp;
        var px = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = mau(x, y);
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    /// <summary>Nut ve bang 9 manh: vien 1 diem, vien sang mong o mep tren,
    /// long chuyen mau tu tren xuong.</summary>
    static Texture2D AnhNut(Color tren, Color duoi, Color vien, Color anhSangTren)
    {
        const int N = 24;
        return Anh(N, N, (x, y) =>
        {
            bool mep = x == 0 || y == 0 || x == N - 1 || y == N - 1;
            if (mep) return vien;
            float k = y / (float)(N - 1);                     // 0 = duoi, 1 = tren
            Color c = Color.Lerp(duoi, tren, k);
            if (y == N - 2) c = Color.Lerp(c, anhSangTren, anhSangTren.a);
            c.a = 1f;
            return c;
        });
    }

    static void TaoAnh()
    {
        if (daTao && trang != null) return;
        daTao = true;

        trang = Anh(1, 1, (x, y) => Color.white);

        // Long khung: tren hoi am (anh do hat len), duoi den dac
        nenBang = Anh(4, 64, (x, y) =>
        {
            float k = y / 63f;
            Color c = Color.Lerp(new Color(0.030f, 0.022f, 0.022f, 1f),
                                 new Color(0.085f, 0.050f, 0.045f, 1f), k * k);
            return c;
        });

        nenHang = Anh(4, 32, (x, y) =>
            Color.Lerp(new Color(0.07f, 0.05f, 0.05f, 1f), new Color(0.12f, 0.08f, 0.07f, 1f), y / 31f));

        // Bon goc man hinh toi lai
        toi4Goc = Anh(128, 128, (x, y) =>
        {
            float dx = (x - 63.5f) / 63.5f, dy = (y - 63.5f) / 63.5f;
            float d = Mathf.Sqrt(dx * dx * 0.8f + dy * dy);
            float a = Mathf.SmoothStep(0.25f, 1.25f, d);
            return new Color(0f, 0f, 0f, a * 0.72f);
        });

        // Suong do boc len tu day man hinh
        suong = Anh(4, 64, (x, y) =>
        {
            float k = 1f - y / 63f;                           // 1 = duoi day
            return new Color(0.30f, 0.02f, 0.02f, Mathf.Pow(k, 2.2f) * 0.45f);
        });

        // Vet mau nho giot - lap theo chieu ngang
        giotMau = TaoGiotMau();

        // Duong ke mo hai dau
        vachMo = Anh(64, 1, (x, y) =>
        {
            float k = 1f - Mathf.Abs(x - 31.5f) / 31.5f;
            return new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, k));
        });

        // Hinh thoi nho gan o giua mep khung
        hatKim = Anh(16, 16, (x, y) =>
        {
            float d = Mathf.Abs(x - 7.5f) + Mathf.Abs(y - 7.5f);
            return new Color(1f, 1f, 1f, Mathf.Clamp01(8f - d));
        });

        nutMau = AnhNut(new Color(0.55f, 0.08f, 0.05f), new Color(0.28f, 0.03f, 0.02f),
                        new Color(0.80f, 0.22f, 0.13f), new Color(1f, 0.55f, 0.40f, 0.45f));
        nutMauSang = AnhNut(new Color(0.72f, 0.12f, 0.07f), new Color(0.38f, 0.05f, 0.03f),
                            new Color(1f, 0.42f, 0.25f), new Color(1f, 0.70f, 0.50f, 0.55f));
        nutMauNhan = AnhNut(new Color(0.25f, 0.03f, 0.02f), new Color(0.42f, 0.06f, 0.04f),
                            new Color(0.70f, 0.18f, 0.10f), new Color(0f, 0f, 0f, 0f));

        nutDa = AnhNut(new Color(0.14f, 0.11f, 0.10f), new Color(0.06f, 0.05f, 0.05f),
                       new Color(0.36f, 0.24f, 0.21f), new Color(1f, 0.85f, 0.75f, 0.12f));
        nutDaSang = AnhNut(new Color(0.22f, 0.13f, 0.11f), new Color(0.09f, 0.06f, 0.05f),
                           new Color(0.78f, 0.24f, 0.16f), new Color(1f, 0.70f, 0.55f, 0.25f));
        nutDaNhan = AnhNut(new Color(0.05f, 0.04f, 0.04f), new Color(0.11f, 0.08f, 0.07f),
                           new Color(0.60f, 0.18f, 0.12f), new Color(0f, 0f, 0f, 0f));

        oNhap = AnhNut(new Color(0.02f, 0.018f, 0.018f), new Color(0.045f, 0.035f, 0.033f),
                       new Color(0.33f, 0.24f, 0.22f), new Color(0f, 0f, 0f, 0f));
        oNhapSang = AnhNut(new Color(0.03f, 0.02f, 0.02f), new Color(0.06f, 0.04f, 0.035f),
                           new Color(0.88f, 0.22f, 0.13f), new Color(0f, 0f, 0f, 0f));
    }

    /// <summary>
    /// Ve mot dai mau: mep tren dac, tu do nho xuong nhung giot dai ngan khac
    /// nhau, dau giot tron. Dung so ngau nhien CO DINH de lan nao mo game cung
    /// y mot hinh - vet mau nhay lung tung moi lan tai trang thi trong re tien.
    /// </summary>
    static Texture2D TaoGiotMau()
    {
        const int W = 256, H = 72;
        var rnd = new System.Random(1666);
        var dai = new float[W];
        var rong = new float[W];

        // Moi giot: vi tri, be rong, chieu dai
        int soGiot = 11;
        var gx = new float[soGiot]; var gr = new float[soGiot]; var gd = new float[soGiot];
        for (int i = 0; i < soGiot; i++)
        {
            gx[i] = (i + 0.2f + (float)rnd.NextDouble() * 0.6f) * W / soGiot;
            gr[i] = 2.2f + (float)rnd.NextDouble() * 3.2f;
            gd[i] = 10f + (float)Mathf.Pow((float)rnd.NextDouble(), 1.6f) * (H - 18f);
        }

        return Anh(W, H, (x, yTex) =>
        {
            float y = H - 1 - yTex;                           // 0 = mep tren
            float a = 0f;
            float sang = 0f;

            // Dai dac o mep tren, gon song nhe
            float day = 5f + 1.6f * Mathf.Sin(x * 0.11f) + 1.1f * Mathf.Sin(x * 0.037f + 1.3f);
            if (y < day) a = 1f;
            else if (y < day + 1.2f) a = day + 1.2f - y;

            for (int i = 0; i < soGiot; i++)
            {
                float dx = Mathf.Abs(x - gx[i]);
                if (dx > gr[i] + 2f) continue;
                // Than giot hep dan xuong duoi, dau tron phinh ra
                float k = Mathf.Clamp01(y / gd[i]);
                float r = gr[i] * Mathf.Lerp(1f, 0.55f, k);
                float aThan = y <= gd[i] ? Mathf.Clamp01(r - dx + 0.5f) : 0f;
                float dy = y - gd[i];
                float rDau = gr[i] * 1.05f;
                float aDau = Mathf.Clamp01(rDau - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                float ai = Mathf.Max(aThan, aDau);
                if (ai > a) { a = ai; sang = Mathf.Clamp01(1f - (x - gx[i] + r) / (r * 0.8f)) * 0.35f; }
            }

            Color c = Color.Lerp(new Color(0.34f, 0.015f, 0.01f), new Color(0.62f, 0.07f, 0.05f), sang);
            c.a = a;
            return c;
        }, true);
    }

    // ================================================================
    //  KIEU CHU
    // ================================================================

    public static GUIStyle KieuTieuDeGame, KieuTieuDe, KieuTieuDeNho, KieuChu, KieuChuMo,
                           KieuChuNho, KieuNutMau, KieuNutDa, KieuO, KieuTab, KieuSoLon;

    static float tiLeDaDat = -1f;

    /// <summary>Goi DAU moi OnGUI. Tao kieu lan dau, cap nhat co chu khi doi man hinh.</summary>
    public static void ChuanBi()
    {
        TaoAnh();
        if (Event.current != null && Event.current.type == EventType.Repaint) SoLuotVe++;
        float s = TiLe;

        if (KieuChu == null)
        {
            KieuTieuDeGame = Kieu(ChuDam, MauMauSang, TextAnchor.MiddleCenter);
            KieuTieuDe = Kieu(ChuDam, MauGiay, TextAnchor.MiddleLeft);
            KieuTieuDeNho = Kieu(ChuDam, MauGiay, TextAnchor.MiddleLeft);
            KieuChu = Kieu(ChuThuong, MauGiay, TextAnchor.MiddleLeft);
            KieuChuMo = Kieu(ChuThuong, MauMo, TextAnchor.MiddleLeft);
            KieuChuNho = Kieu(ChuThuong, MauMo, TextAnchor.MiddleLeft);
            KieuSoLon = Kieu(ChuDam, MauMauSang, TextAnchor.MiddleCenter);

            KieuNutMau = KieuNut(nutMau, nutMauSang, nutMauNhan, MauGiay);
            KieuNutDa = KieuNut(nutDa, nutDaSang, nutDaNhan, MauGiay);

            KieuO = new GUIStyle();
            KieuO.font = ChuThuong;
            KieuO.normal.background = oNhap;
            KieuO.hover.background = oNhap;
            KieuO.focused.background = oNhapSang;
            KieuO.onFocused.background = oNhapSang;
            KieuO.normal.textColor = KieuO.hover.textColor = KieuO.focused.textColor = MauGiay;
            KieuO.onFocused.textColor = MauGiay;
            KieuO.border = new RectOffset(3, 3, 3, 3);
            KieuO.alignment = TextAnchor.MiddleLeft;
            KieuO.clipping = TextClipping.Clip;

            KieuTab = new GUIStyle();
            KieuTab.font = ChuDam;
            KieuTab.alignment = TextAnchor.MiddleCenter;
            KieuTab.normal.textColor = MauMo;
            KieuTab.hover.textColor = MauGiay;
            KieuTab.active.textColor = MauMauSang;
        }

        if (!Mathf.Approximately(s, tiLeDaDat))
        {
            tiLeDaDat = s;
            KieuTieuDeGame.fontSize = Co(104f, s);
            KieuTieuDe.fontSize = Co(34f, s);
            KieuTieuDeNho.fontSize = Co(25f, s);
            KieuChu.fontSize = Co(22f, s);
            KieuChuMo.fontSize = Co(20f, s);
            KieuChuNho.fontSize = Co(17f, s);
            KieuSoLon.fontSize = Co(200f, s);
            KieuNutMau.fontSize = Co(23f, s);
            KieuNutDa.fontSize = Co(20f, s);
            KieuO.fontSize = Co(22f, s);
            KieuTab.fontSize = Co(22f, s);

            int dem = Mathf.RoundToInt(14f * s);
            KieuO.padding = new RectOffset(dem, dem, 0, 0);
            KieuNutMau.padding = new RectOffset(dem, dem, 0, 0);
            KieuNutDa.padding = new RectOffset(dem, dem, 0, 0);
        }

        // Con tro va vung boi den trong o nhap theo mau cua game
        GUI.skin.settings.cursorColor = MauGiay;
        GUI.skin.settings.selectionColor = new Color(0.75f, 0.12f, 0.08f, 0.55f);
    }

    static int Co(float co, float s) { return Mathf.Max(10, Mathf.RoundToInt(co * s)); }

    static GUIStyle Kieu(Font f, Color mau, TextAnchor canh)
    {
        var k = new GUIStyle();
        k.font = f;
        k.normal.textColor = mau;
        k.alignment = canh;
        k.clipping = TextClipping.Overflow;
        k.wordWrap = false;
        return k;
    }

    static GUIStyle KieuNut(Texture2D thuong, Texture2D sang, Texture2D nhan, Color chu)
    {
        var k = new GUIStyle();
        k.font = ChuDam;
        k.alignment = TextAnchor.MiddleCenter;
        k.border = new RectOffset(3, 3, 3, 3);
        k.normal.background = thuong;
        k.hover.background = sang;
        k.active.background = nhan;
        k.focused.background = thuong;
        k.normal.textColor = chu;
        k.hover.textColor = Color.white;
        k.active.textColor = new Color(0.85f, 0.75f, 0.65f);
        k.focused.textColor = chu;
        k.clipping = TextClipping.Clip;
        return k;
    }

    // ================================================================
    //  VE
    // ================================================================

    static readonly GUIContent noiDung = new GUIContent();

    /// <summary>Nen toan man hinh: toi bon goc va suong do duoi day.</summary>
    public static void VeNen(float doToiThem = 0f)
    {
        var mauCu = GUI.color;
        if (doToiThem > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, doToiThem);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), trang);
        }
        GUI.color = mauCu;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), toi4Goc, ScaleMode.StretchToFill, true);
        float cao = Screen.height * 0.38f;
        GUI.DrawTexture(new Rect(0, Screen.height - cao, Screen.width, cao), suong, ScaleMode.StretchToFill, true);
    }

    /// <summary>To mot hinh chu nhat mau tron.</summary>
    public static void To(Rect r, Color c)
    {
        var mauCu = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, trang);
        GUI.color = mauCu;
    }

    /// <summary>Duong ke ngang mo dan hai dau.</summary>
    public static void DuongKe(Rect r, Color c)
    {
        var mauCu = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, vachMo, ScaleMode.StretchToFill, true);
        GUI.color = mauCu;
    }

    /// <summary>
    /// Khung chinh: long toi, vien do sam, moc sat o bon goc, hinh thoi o
    /// giua mep tren. <paramref name="coGiotMau"/> thi mau nho giot tu mep tren.
    /// </summary>
    public static void Khung(Rect r, float s, bool coGiotMau = false, float doDuc = -1f)
    {
        // Long khung TRONG SUOT mot phan de thay canh phia sau (nguoi dung xin
        // 11/09/2026). Chi long khung - vien, moc sat, nut, o nhap va chu van dac.
        var mauLong = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, doDuc < 0f ? DoDucKhung : doDuc);
        GUI.DrawTexture(r, nenBang, ScaleMode.StretchToFill, true);
        GUI.color = mauLong;

        float d = Mathf.Max(1f, Mathf.Round(1.5f * s));
        var vien = new Color(0.40f, 0.09f, 0.07f, 0.95f);
        To(new Rect(r.x, r.y, r.width, d), vien);
        To(new Rect(r.x, r.yMax - d, r.width, d), vien);
        To(new Rect(r.x, r.y, d, r.height), vien);
        To(new Rect(r.xMax - d, r.y, d, r.height), vien);

        // Moc sat bon goc
        float dai = 22f * s, day = Mathf.Max(2f, Mathf.Round(3f * s));
        var moc = MauMau;
        To(new Rect(r.x - day, r.y - day, dai, day), moc);
        To(new Rect(r.x - day, r.y - day, day, dai), moc);
        To(new Rect(r.xMax - dai + day, r.y - day, dai, day), moc);
        To(new Rect(r.xMax, r.y - day, day, dai), moc);
        To(new Rect(r.x - day, r.yMax, dai, day), moc);
        To(new Rect(r.x - day, r.yMax - dai + day, day, dai), moc);
        To(new Rect(r.xMax - dai + day, r.yMax, dai, day), moc);
        To(new Rect(r.xMax, r.yMax - dai + day, day, dai), moc);

        if (coGiotMau)
        {
            float cao = 40f * s;             // noi dung trong khung bat dau tu 44 don vi
            float lap = r.width / (256f * 1.15f * s);
            GUI.DrawTextureWithTexCoords(new Rect(r.x + 10f * s, r.y + d, r.width - 20f * s, cao),
                                         giotMau, new Rect(0.13f, 0f, lap, 1f), true);
        }

        // Hinh thoi o giua mep tren
        float kt = 14f * s;
        var mauCu = GUI.color;
        GUI.color = MauMauSang;
        GUI.DrawTexture(new Rect(r.center.x - kt * 0.5f, r.y - kt * 0.5f, kt, kt), hatKim, ScaleMode.StretchToFill, true);
        GUI.color = mauCu;
    }

    /// <summary>Mot hang trong danh sach: long hoi sang hon khung, vach do ben trai.</summary>
    public static void Hang(Rect r, float s, Color vach)
    {
        var mauLong = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, DoDucHang);
        GUI.DrawTexture(r, nenHang, ScaleMode.StretchToFill, true);
        GUI.color = mauLong;
        To(new Rect(r.x, r.y, Mathf.Max(2f, 4f * s), r.height), vach);
        To(new Rect(r.x, r.yMax - 1f, r.width, 1f), new Color(0.3f, 0.1f, 0.08f, 0.6f));
    }

    /// <summary>
    /// Ten game o giua, chap chon nhu anh nen: bong den phia duoi, quang do
    /// quanh chu, do sang dao dong theo nhieu Perlin.
    /// </summary>
    public static void TieuDeGame(Rect r, float s, string chu = "DIABLO 2.5D")
    {
        float t = Time.unscaledTime;
        float nhay = 0.72f + 0.28f * Mathf.PerlinNoise(t * 2.7f, 0.37f);

        var k = KieuTieuDeGame;
        var mauGoc = k.normal.textColor;

        // Bong den
        k.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
        GUI.Label(new Rect(r.x + 4f * s, r.y + 6f * s, r.width, r.height), chu, k);

        // Quang do - ve chong nhieu lan quanh tam
        float bk = 5f * s;
        k.normal.textColor = new Color(0.95f, 0.10f, 0.05f, 0.10f * nhay);
        for (int i = 0; i < 8; i++)
        {
            float g = i * Mathf.PI / 4f;
            GUI.Label(new Rect(r.x + Mathf.Cos(g) * bk, r.y + Mathf.Sin(g) * bk, r.width, r.height), chu, k);
        }

        k.normal.textColor = Color.Lerp(new Color(0.55f, 0.05f, 0.03f), mauGoc, nhay);
        GUI.Label(r, chu, k);
        k.normal.textColor = mauGoc;

        // Vet mau chay giot ngay duoi chan chu, rong bang dong chu
        noiDung.text = chu;
        float rongChu = k.CalcSize(noiDung).x;
        float caoChu = k.CalcSize(noiDung).y;
        float yDuoi = r.center.y + caoChu * 0.36f;
        float caoGiot = 46f * s;
        var mauCu = GUI.color;
        GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.95f);
        GUI.DrawTextureWithTexCoords(new Rect(r.center.x - rongChu * 0.48f, yDuoi, rongChu * 0.96f, caoGiot),
                                     giotMau, new Rect(0.41f, 0f, rongChu / (256f * 1.3f * s), 1f), true);
        GUI.color = mauCu;
    }

    /// <summary>
    /// Ve mot dong chu KHONG BAO GIO TRAN khoi o: dai qua thi thu nho co chu
    /// toi 62%, van dai thi cat bot va them "…".
    /// </summary>
    public static void Chu(Rect r, string chu, GUIStyle k)
    {
        if (string.IsNullOrEmpty(chu)) return;
        if (k.wordWrap) { GUI.Label(r, chu, k); return; }

        int goc = k.fontSize;
        string ve = VuaO(chu, k, r.width - k.padding.horizontal);
        GUI.Label(r, ve, k);
        k.fontSize = goc;
    }

    /// <summary>Chu nhieu dong, xuong dong trong be rong o.</summary>
    public static void ChuNhieuDong(Rect r, string chu, GUIStyle k)
    {
        bool cu = k.wordWrap;
        k.wordWrap = true;
        GUI.Label(r, chu, k);
        k.wordWrap = cu;
    }

    /// <summary>
    /// Thu nho co chu (sua thang k.fontSize - nguoi goi tra lai) va cat chu
    /// cho vua be rong. Tra ve chuoi se ve.
    /// </summary>
    static string VuaO(string chu, GUIStyle k, float rong)
    {
        if (rong <= 4f) return "";
        int goc = k.fontSize;
        noiDung.text = chu;
        float w = k.CalcSize(noiDung).x;
        if (w <= rong) return chu;

        int moi = Mathf.Max(Mathf.RoundToInt(goc * 0.62f), Mathf.FloorToInt(goc * rong / w));
        k.fontSize = moi;
        w = k.CalcSize(noiDung).x;
        if (Event.current == null || Event.current.type == EventType.Repaint)
        {
            SoLanThuNho++;
            if (ChuBiThuNho.Count < 40 && !ChuBiThuNho.Contains(chu)) ChuBiThuNho.Add(chu);
        }
        if (w <= rong) return chu;

        if (Event.current == null || Event.current.type == EventType.Repaint)
        { SoLanCat++; ChuBiCatCuoi = chu; }

        // Van dai: cat dan tung chu
        for (int n = chu.Length - 1; n > 0; n--)
        {
            noiDung.text = chu.Substring(0, n).TrimEnd() + "…";
            if (k.CalcSize(noiDung).x <= rong) return noiDung.text;
        }
        return "…";
    }

    /// <summary>Nut co chu tu vua o.</summary>
    public static bool Nut(Rect r, string chu, GUIStyle k)
    {
        int goc = k.fontSize;
        string ve = VuaO(chu, k, r.width - k.padding.horizontal - 4f);
        bool bam = GUI.Button(r, ve, k);
        k.fontSize = goc;
        return bam;
    }

    /// <summary>Tab chu co gach chan do khi dang chon.</summary>
    public static bool Tab(Rect r, string chu, bool chon, float s)
    {
        var k = KieuTab;
        var cu = k.normal.textColor;
        if (chon) k.normal.textColor = MauGiay;
        int goc = k.fontSize;
        string ve = VuaO(chu, k, r.width - 8f);
        bool bam = GUI.Button(r, ve, k);
        k.fontSize = goc;
        k.normal.textColor = cu;

        float d = Mathf.Max(2f, 3f * s);
        if (chon) To(new Rect(r.x, r.yMax - d, r.width, d), MauMauSang);
        else To(new Rect(r.x, r.yMax - 1f, r.width, 1f), new Color(0.35f, 0.22f, 0.2f, 0.7f));
        return bam;
    }

    /// <summary>O nhap chu, co dong goi y mo khi trong va chua bam vao.</summary>
    public static string ONhap(Rect r, string ten, string giaTri, int toiDa, string goiY, bool matKhau = false)
    {
        GUI.SetNextControlName(ten);
        string ra = matKhau ? GUI.PasswordField(r, giaTri, '•', toiDa, KieuO)
                            : GUI.TextField(r, giaTri, toiDa, KieuO);

        if (string.IsNullOrEmpty(ra) && GUI.GetNameOfFocusedControl() != ten && !string.IsNullOrEmpty(goiY))
        {
            var k = KieuChuNho;
            int goc = k.fontSize;
            k.fontSize = Mathf.RoundToInt(KieuO.fontSize * 0.9f);
            var mauCu = k.normal.textColor;
            k.normal.textColor = MauToi;
            Chu(new Rect(r.x + KieuO.padding.left, r.y, r.width - KieuO.padding.horizontal, r.height), goiY, k);
            k.normal.textColor = mauCu;
            k.fontSize = goc;
        }
        return ra;
    }

    /// <summary>Nhan nho phia tren mot o nhap.</summary>
    public static void NhanO(Rect r, string chu)
    {
        Chu(r, chu, KieuChuMo);
    }
}
