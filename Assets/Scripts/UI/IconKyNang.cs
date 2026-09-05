using UnityEngine;

/// <summary>
/// SAU BIEU TUONG KY NANG.
///
/// Ruot cua moi bieu tuong la mot anh DUNG TRONG BLENDER - qua cau lua co van
/// cuon, tinh the bang co canh bat sang, tia set chay trong luong dien tim,
/// con loc xoan, tang thien thach chay do, vom khien nang luong. Chung nam o
/// Assets/Resources/Icons.
///
/// Truoc day sau hinh nay ve bang tay tung diem anh: ra hinh phang, canh cung
/// deu, khong hop voi khung canh trong game. Ve tay chi hop cho nhung mieng
/// don gian nhu vanh nut hay thanh mau.
///
/// Lop nay lo phan CON LAI: cai dia nut - mat loi, den tu tren trai, vanh vien
/// - roi CONG anh Blender vao. Cong chu khong tron, vi anh Blender nen DEN:
/// cho den khong anh huong gi, cho sang thi loe len, dung kieu mot hieu ung
/// phat sang.
/// </summary>
public static class IconKyNang
{
    const int S = 256;                 // bang dung do phan giai anh Blender

    // Nguon sang cua cai dia. Dung MOT huong cho ca sau, khong thi dat canh nhau
    // se thay cai nay sang goc nay cai kia goc kia.
    static readonly Vector3 Den = new Vector3(-0.40f, 0.62f, 0.68f).normalized;

    public static Texture2D Lua()        { return Ve("Icons/Lua",        new Color(0.30f, 0.09f, 0.03f)); }
    public static Texture2D Bang()       { return Ve("Icons/Bang",       new Color(0.05f, 0.14f, 0.30f)); }
    public static Texture2D Set()        { return Ve("Icons/Set",        new Color(0.08f, 0.06f, 0.22f)); }
    public static Texture2D Loc()        { return Ve("Icons/Loc",        new Color(0.03f, 0.17f, 0.18f)); }
    public static Texture2D ThienThach() { return Ve("Icons/ThienThach", new Color(0.24f, 0.11f, 0.03f)); }
    public static Texture2D Khieng()     { return Ve("Icons/Khieng",     new Color(0.20f, 0.12f, 0.02f)); }
    // Nen XANH LAM chu khong tim nhu Sam set: hai ky nang cung he set, phai
    // liec mot cai la phan biet duoc nut nao la nut nao.
    public static Texture2D GiatSet()    { return Ve("Icons/GiatSet",    new Color(0.04f, 0.11f, 0.28f)); }

    /// <summary>
    /// Dat anh Blender vao mot cai dia nut.
    /// </summary>
    /// <param name="duong">Duong trong Resources, khong co duoi file.</param>
    /// <param name="nenHe">Mau nen theo he nguyen to - TOI thoi, de chu the noi len.</param>
    static Texture2D Ve(string duong, Color nenHe)
    {
        var anh = Resources.Load<Texture2D>(duong);

        var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        var px = new Color[S * S];

        // Doc mot lan ra mang: GetPixelBilinear cho tung diem thi cham hon nhieu
        Color[] apx = null;
        int aw = 0, ah = 0;
        if (anh != null)
        {
            try
            {
                apx = anh.GetPixels();
                aw = anh.width;
                ah = anh.height;
            }
            catch (UnityException)
            {
                // Anh chua bat Read/Write. Van ve duoc cai dia, chi thieu ruot -
                // va bao ra Console chu khong im lang tra ve mot nut trong.
                Debug.LogWarning("[IconKyNang] Khong doc duoc " + duong
                               + " - can bat Read/Write Enabled cho anh nay.");
            }
        }
        else
        {
            Debug.LogWarning("[IconKyNang] Khong tim thay Resources/" + duong);
        }

        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float u = (x + 0.5f) / S * 2f - 1f;
                float v = (y + 0.5f) / S * 2f - 1f;
                float d = Mathf.Sqrt(u * u + v * v);

                float aDia = 1f - Muot(0.965f, 1.0f, d);
                if (aDia <= 0.001f) { px[y * S + x] = new Color(0f, 0f, 0f, 0f); continue; }

                // Phap tuyen kieu "cai goi": phang o giua, vat xuong o ria
                float f = Mathf.Pow(Mathf.Clamp01(d), 3.2f);
                Vector2 huong = d > 1e-5f ? new Vector2(u / d, v / d) : Vector2.zero;
                var N = new Vector3(huong.x * f, huong.y * f,
                                    Mathf.Sqrt(Mathf.Max(1e-4f, 1f - f * f)));
                float dif = Mathf.Clamp01(Vector3.Dot(N, Den));

                // Nen: toi, chi de cai nut co mot mau rieng theo he nguyen to
                Color c = nenHe * Mathf.Lerp(0.35f, 1.30f, dif);
                c += Color.white * (Mathf.Pow(dif, 18f) * 0.30f);

                float rim = Muot(0.62f, 0.99f, d) * Mathf.Clamp01(-Vector3.Dot(N, Den));
                c += new Color(0.45f, 0.50f, 0.68f) * (rim * 0.45f);

                // Ruot: CONG vao. Toi dan o ria de anh vuong khong bi cat cut
                // ngang o mep dia.
                if (apx != null)
                {
                    int ax = Mathf.Clamp(Mathf.RoundToInt((x + 0.5f) / S * aw - 0.5f), 0, aw - 1);
                    int ay = Mathf.Clamp(Mathf.RoundToInt((y + 0.5f) / S * ah - 0.5f), 0, ah - 1);
                    Color a = apx[ay * aw + ax];
                    float k = 1f - Muot(0.80f, 0.99f, d);
                    c += new Color(a.r, a.g, a.b) * k;
                }

                float vien = Muot(0.88f, 0.97f, d) * (1f - Muot(0.97f, 1.0f, d));
                c = Color.Lerp(c, new Color(1f, 0.97f, 0.88f), vien * 0.40f);

                c.r = Mathf.Clamp01(c.r);
                c.g = Mathf.Clamp01(c.g);
                c.b = Mathf.Clamp01(c.b);
                c.a = aDia;
                px[y * S + x] = c;
            }

        t.SetPixels(px);
        t.Apply();
        return t;
    }

    /// <summary>
    /// Chuyen muot tu 0 sang 1 khi <paramref name="x"/> di tu canh0 sang canh1.
    ///
    /// KHONG dung Mathf.SmoothStep: ten giong nhau nhung viec khac han. Cua
    /// Unity la "noi suy muot GIA TRI tu a den b theo t", nen
    /// Mathf.SmoothStep(0.965f, 1f, 0f) tra ve 0,965 chu khong phai 0. Toi da
    /// viet do trong o tinh alpha va ca sau bieu tuong ra gan nhu trong suot -
    /// hinh thi dung y nhung mau thi bay sach.
    /// </summary>
    static float Muot(float canh0, float canh1, float x)
    {
        float t = Mathf.Clamp01((x - canh0) / Mathf.Max(canh1 - canh0, 1e-6f));
        return t * t * (3f - 2f * t);
    }
}
