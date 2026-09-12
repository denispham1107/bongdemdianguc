using UnityEngine;

/// <summary>
/// HINH O KHOA tren nut ky nang chua mo.
///
/// Ban dau o khoa ghep tu ba bon hinh chu nhat trang (GameHUD) - nguoi dung
/// che (13/09/2026): qua don gian, khong hop khong khi kinh di cua game. Ban
/// may tinh con te hon: chi ve moi mot thanh ngang, nhin ra MOT HINH VUONG
/// TRANG chu khong ra o khoa.
///
/// Nay la MOT ANH sinh bang code luc chay, dung chung cho CA HAI ban (cum nut
/// tron cam ung va thanh o vuong may tinh) - hai ban khong the lech nhau nua:
///   - quai sat tron, co anh sang doc than nhu ong kim loai,
///   - than sat den gi, vien trong do gi, bon dinh tan,
///   - lo khoa hinh giot, trong long hat anh do nhu lua duoi dia nguc,
///   - vien den quanh ca khoi va bong do lech duoi phai.
///
/// Ve bang TRUONG KHOANG CACH (SDF) roi lam mem mep theo khoang cach, nen o
/// co nao cung khong rang cua. Sinh mot lan 128x128, khong ton gi luc choi.
///
/// KICH THUOC tinh theo BE RONG PHAN CO HINH (khong tinh le trong suot cua anh)
/// - xem <see cref="KhungVe"/> - de noi "nho hon 20%" la dung 20% cua cai nguoi
/// choi nhin thay.
/// </summary>
public static class IconKhoa
{
    const int N = 128;

    static Texture2D anh;
    static Rect noiDung;          // phan co hinh, chuan hoa 0..1, y tinh tu TREN xuong

    public static Texture2D Anh { get { Dung(); return anh; } }

    /// <summary>Phan co hinh trong anh (alpha &gt; 0,5), chuan hoa 0..1, y tu tren xuong.</summary>
    public static Rect NoiDung { get { Dung(); return noiDung; } }

    /// <summary>
    /// Khung GUI de ve o khoa sao cho PHAN CO HINH rong dung
    /// <paramref name="rongNoiDung"/> diem va tam cua no trung <paramref name="tam"/>.
    /// </summary>
    public static Rect KhungVe(Vector2 tam, float rongNoiDung)
    {
        Dung();
        float kt = rongNoiDung / Mathf.Max(0.01f, noiDung.width);
        return new Rect(tam.x - noiDung.center.x * kt, tam.y - noiDung.center.y * kt, kt, kt);
    }

    /// <summary>Ve o khoa giua <paramref name="tam"/> (toa do GUI).</summary>
    public static void Ve(Vector2 tam, float rongNoiDung)
    {
        var cu = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(KhungVe(tam, rongNoiDung), Anh, ScaleMode.StretchToFill, true);
        GUI.color = cu;
    }

    // ================================================================
    //  SINH ANH
    // ================================================================

    static float SdHopBo(Vector2 p, Vector2 tam, Vector2 nua, float bo)
    {
        Vector2 q = new Vector2(Mathf.Abs(p.x - tam.x), Mathf.Abs(p.y - tam.y)) - nua + new Vector2(bo, bo);
        return new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude
               + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - bo;
    }

    static float SdDoan(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 pa = p - a, ba = b - a;
        float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
        return (pa - ba * h).magnitude;
    }

    /// <summary>Khoang cach toi DUONG TAM cua quai: nua vong tron phia tren + hai chan thang.</summary>
    static float KhoangCachQuai(Vector2 p)
    {
        Vector2 c = new Vector2(0.5f, 0.36f);
        const float R = 0.19f, dayChan = 0.50f;
        float d;
        if (p.y <= c.y) d = Mathf.Abs((p - c).magnitude - R);
        else d = float.MaxValue;
        d = Mathf.Min(d, SdDoan(p, new Vector2(c.x - R, c.y), new Vector2(c.x - R, dayChan)));
        d = Mathf.Min(d, SdDoan(p, new Vector2(c.x + R, c.y), new Vector2(c.x + R, dayChan)));
        return d;
    }

    static readonly Vector2 TamThan = new Vector2(0.5f, 0.66f);
    static readonly Vector2 NuaThan = new Vector2(0.31f, 0.23f);
    const float BoThan = 0.07f;
    const float NuaDayQuai = 0.0425f;

    static float SdThan(Vector2 p) { return SdHopBo(p, TamThan, NuaThan, BoThan); }
    static float SdQuai(Vector2 p) { return KhoangCachQuai(p) - NuaDayQuai; }

    static float SdLoKhoa(Vector2 p)
    {
        float tron = (p - new Vector2(0.5f, 0.615f)).magnitude - 0.055f;
        float t = Mathf.InverseLerp(0.64f, 0.775f, p.y);
        float nua = Mathf.Lerp(0.020f, 0.042f, t);
        float than = Mathf.Max(Mathf.Abs(p.x - 0.5f) - nua, Mathf.Max(0.64f - p.y, p.y - 0.775f));
        return Mathf.Min(tron, than);
    }

    /// <summary>Do phu cua mot mep co khoang cach d (don vi chuan hoa) - mep mem mot diem anh.</summary>
    static float Phu(float d) { return Mathf.Clamp01(0.5f - d * N); }

    static float Bam(int i, int j)
    {
        float h = Mathf.Sin(i * 12.9898f + j * 78.233f) * 43758.5453f;
        return h - Mathf.Floor(h);
    }

    static Color De(Color duoi, Color tren, float phu)
    {
        float a = tren.a * phu;
        float aRa = a + duoi.a * (1f - a);
        if (aRa < 0.0001f) return new Color(0f, 0f, 0f, 0f);
        Color c = (tren * a + duoi * duoi.a * (1f - a)) / aRa;
        c.a = aRa;
        return c;
    }

    static void Dung()
    {
        if (anh != null) return;

        anh = new Texture2D(N, N, TextureFormat.RGBA32, false);
        anh.name = "IconKhoa";
        anh.wrapMode = TextureWrapMode.Clamp;
        anh.filterMode = FilterMode.Bilinear;

        var px = new Color[N * N];
        float minX = 1f, minY = 1f, maxX = 0f, maxY = 0f;

        Vector2[] dinh =
        {
            new Vector2(0.27f, 0.50f), new Vector2(0.73f, 0.50f),
            new Vector2(0.27f, 0.82f), new Vector2(0.73f, 0.82f),
        };

        for (int j = 0; j < N; j++)
            for (int i = 0; i < N; i++)
            {
                // v tinh tu TREN xuong - de doc giong toa do GUI
                var p = new Vector2((i + 0.5f) / N, (j + 0.5f) / N);
                Color c = new Color(0f, 0f, 0f, 0f);

                float dQuai = SdQuai(p), dThan = SdThan(p);
                float dKhoi = Mathf.Min(dQuai, dThan);

                // ---- Bong do lech duoi phai, mo rong ----
                var pb = p - new Vector2(0.022f, 0.032f);
                float dBong = Mathf.Min(SdQuai(pb), SdThan(pb));
                float aBong = Mathf.Clamp01(1f - Mathf.Max(0f, dBong) / 0.05f) * 0.45f;
                c = De(c, new Color(0f, 0f, 0f, 1f), aBong);

                // ---- Vien den quanh ca khoi ----
                c = De(c, new Color(0.02f, 0.01f, 0.01f, 0.95f), Phu(dKhoi - 0.024f));

                // ---- Quai: ong sat tron ----
                {
                    float k = Mathf.Clamp01(KhoangCachQuai(p) / NuaDayQuai);
                    float tron = Mathf.Sqrt(Mathf.Max(0f, 1f - k * k));       // giua ong sang nhat
                    float benTrai = Mathf.Clamp01(0.55f - (p.x - 0.5f) * 1.6f); // anh sang tu tren trai
                    float sang = 0.22f + 0.60f * tron * (0.55f + 0.45f * benTrai);
                    var mau = Color.Lerp(new Color(0.10f, 0.09f, 0.09f), new Color(0.80f, 0.77f, 0.72f), sang);
                    mau.a = 1f;
                    c = De(c, mau, Phu(dQuai));
                }

                // ---- Than: sat den gi, sang dan len tren ----
                {
                    float t = Mathf.InverseLerp(TamThan.y - NuaThan.y, TamThan.y + NuaThan.y, p.y);
                    var mau = Color.Lerp(new Color(0.42f, 0.35f, 0.30f), new Color(0.15f, 0.11f, 0.10f), t);

                    // Van gi: vet lom dom khong deu
                    float gi = Bam(i / 3, j / 3);
                    mau = Color.Lerp(mau, new Color(0.30f, 0.12f, 0.06f), gi > 0.78f ? 0.35f : 0f);
                    mau *= 0.92f + 0.10f * Bam(i, j);

                    // Canh tren bat anh sang
                    float canhTren = Mathf.Clamp01(1f - Mathf.Abs(p.y - (TamThan.y - NuaThan.y + 0.03f)) / 0.018f);
                    mau = Color.Lerp(mau, new Color(0.78f, 0.70f, 0.60f), canhTren * 0.55f);

                    // Vien trong do gi
                    float vien = Mathf.Clamp01(1f - Mathf.Abs(dThan + 0.034f) / 0.008f);
                    mau = Color.Lerp(mau, new Color(0.58f, 0.10f, 0.06f), vien * 0.85f);

                    mau.a = 1f;
                    c = De(c, mau, Phu(dThan));
                }

                // ---- Bon dinh tan ----
                for (int k = 0; k < dinh.Length; k++)
                {
                    float dd = (p - dinh[k]).magnitude - 0.021f;
                    if (dd > 0.01f) continue;
                    float sang = Mathf.Clamp01(1f - (p - dinh[k] + new Vector2(0.008f, 0.008f)).magnitude / 0.026f);
                    var mau = Color.Lerp(new Color(0.20f, 0.17f, 0.15f), new Color(0.88f, 0.82f, 0.72f), sang);
                    mau.a = 1f;
                    c = De(c, mau, Phu(dd));
                }

                // ---- Lo khoa: den, trong long hat anh do ----
                {
                    float dLo = SdLoKhoa(p);
                    float giua = Mathf.Clamp01(1f - (p - new Vector2(0.5f, 0.66f)).magnitude / 0.11f);
                    var mau = Color.Lerp(new Color(0.03f, 0.01f, 0.01f), new Color(0.85f, 0.14f, 0.05f), giua * giua * 0.9f);
                    mau.a = 1f;
                    c = De(c, mau, Phu(dLo) * Phu(dThan));
                }

                px[(N - 1 - j) * N + i] = c;       // Texture2D dem y tu DUOI len

                if (c.a > 0.5f)
                {
                    minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                    minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
                }
            }

        anh.SetPixels(px);
        anh.Apply(false, false);
        noiDung = Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
