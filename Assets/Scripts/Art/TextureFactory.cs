using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VE ANH (TEXTURE) BANG CODE.
///
/// Game khong dung file anh nao tren o cung - moi hoa tiet (vai ao, da nguoi,
/// xuong, da tang, dom lua, bong tuyet...) deu duoc ve bang phep tinh khi khoi dong.
/// </summary>
public static class TextureFactory
{
    static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

    // ================================================================
    //  TONG MAU CAY CO  -  doi o day la doi ca bai co lan tan la
    // ================================================================

    // Tong hien tai: NAU UA VANG KHO, hoi nga cam.
    // Ca bon mau deu nam quanh sac cam-ho phach (30-34 do tren vong mau) thay vi
    // xanh la (90-100 do) nhu truoc. Chon vay vi ban do von day cay chet, xuong va
    // da tro - co xanh muot mau thuan voi chinh khung canh do; va mau am nam doi
    // dien mau dat tim lanh nen co van tach khoi nen, khong bi chim.

    /// <summary>Mau goc la co (phan sat dat, toi hon).</summary>
    public static Color GrassRoot = new Color(0.22f, 0.15f, 0.08f);

    /// <summary>Mau ngon la co (phan tren, sang hon).</summary>
    public static Color GrassTip = new Color(0.62f, 0.43f, 0.19f);

    /// <summary>Mau giua tan la (sang hon).</summary>
    public static Color LeafLight = new Color(0.36f, 0.25f, 0.12f);

    /// <summary>Mau ria tan la (toi hon).</summary>
    public static Color LeafDark = new Color(0.18f, 0.12f, 0.055f);

    static Texture2D Get(string key, System.Func<Texture2D> make)
    {
        Texture2D t;
        if (cache.TryGetValue(key, out t) && t != null) return t;
        t = make();
        t.name = "Tex_" + CleanName(key);
        cache[key] = t;
        return t;
    }

    /// <summary>Bo cac ky tu la trong khoa de dat ten file cho gon.</summary>
    static string CleanName(string key)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < key.Length && sb.Length < 22; i++)
        {
            char c = key[i];
            if (char.IsLetterOrDigit(c)) sb.Append(c);
        }
        return sb.Length > 0 ? sb.ToString() : "tex";
    }

    /// <summary>
    /// Nhan mot anh co san voi mot mau. Dung cho lop vat lieu cua Unity Terrain:
    /// TerrainLayer khong co o "mau nhuom" nhu shader rieng, nen phai nhuom san vao anh.
    /// </summary>
    public static Texture2D Tinted(Texture2D src, Color tint, string name)
    {
        return Get("nhuom_" + name, delegate
        {
            var px = src.GetPixels();
            for (int i = 0; i < px.Length; i++)
                px[i] = new Color(px[i].r * tint.r, px[i].g * tint.g, px[i].b * tint.b, px[i].a);

            var t = New(src.width, src.height, true);
            t.SetPixels(px);
            t.Apply(true);
            return t;
        });
    }

    static Texture2D New(int w, int h, bool repeat)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, true);
        t.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        t.anisoLevel = 4;
        return t;
    }

    // ================================================================
    //  ANH CHO HAT (PARTICLE)
    // ================================================================

    /// <summary>Dom sang tron, mo dan ra ria - dung cho tan lua, tia sang, bui bang.</summary>
    public static Texture2D SoftDot(float power)
    {
        return Get("dot" + power, delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x + 0.5f) / S - 0.5f;
                    float dy = (y + 0.5f) / S - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), power);
                    px[y * S + x] = new Color(1f, 1f, 1f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Ngon lua hinh giot nuoc co ria rung rinh.</summary>
    public static Texture2D FlameLick()
    {
        return Get("flame", delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = (x + 0.5f) / S - 0.5f;
                    float v = (y + 0.5f) / S;

                    // Be ngang thu hep dan len tren -> hinh luoi lua
                    float width = Mathf.Lerp(0.42f, 0.02f, Mathf.Pow(v, 0.8f));
                    float d = Mathf.Abs(u) / Mathf.Max(width, 0.001f);
                    float body = Mathf.Clamp01(1f - d);

                    // Ria lua go ghe
                    float n = Mathf.PerlinNoise(u * 9f + 12.3f, v * 7f);
                    body *= Mathf.Lerp(0.6f, 1.15f, n);
                    body *= Mathf.Clamp01(1f - Mathf.Pow(v, 3f));
                    body *= Mathf.Clamp01(v * 6f);

                    float a = Mathf.Clamp01(body);
                    float core = Mathf.Pow(a, 2.2f);
                    px[y * S + x] = new Color(1f, 0.55f + core * 0.45f, 0.25f + core * 0.6f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Cuon khoi (dung cho khoi den sau vu no lua, hoi lanh cua bang).</summary>
    public static Texture2D Smoke()
    {
        return Get("smoke", delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x + 0.5f) / S - 0.5f;
                    float dy = (y + 0.5f) / S - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;

                    float n = 0f, amp = 0.5f, f = 3f;
                    for (int o = 0; o < 4; o++)
                    {
                        n += amp * Mathf.PerlinNoise(x / (float)S * f + 5.1f, y / (float)S * f + 9.7f);
                        f *= 2.1f; amp *= 0.5f;
                    }
                    float a = Mathf.Clamp01(1f - d) * Mathf.Clamp01(n * 1.5f);
                    a = Mathf.Pow(a, 1.4f);
                    px[y * S + x] = new Color(1f, 1f, 1f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Manh bang nhon (dung cho tan bang bay ra khi no).</summary>
    public static Texture2D IceShard()
    {
        return Get("shard", delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = (x + 0.5f) / S - 0.5f;
                    float v = (y + 0.5f) / S;
                    float width = Mathf.Lerp(0.30f, 0.0f, v) * Mathf.Clamp01(v * 5f);
                    float a = Mathf.Abs(u) < width ? 1f : 0f;

                    // Vat sang chay doc than manh bang
                    float shine = Mathf.Clamp01(1f - Mathf.Abs(u + 0.05f) * 12f);
                    float c = 0.72f + shine * 0.28f;
                    px[y * S + x] = new Color(c, c * 0.98f + 0.02f, 1f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// LUOI MANH VO: 4x4 = 16 hinh manh KHAC NHAU trong cung mot anh.
    ///
    /// VI SAO CAN NHIEU HINH: manh vo cua mot cai khien no ra thi moi manh mot
    /// hinh. Dung MOT anh duy nhat cho ca tram hat thi ca dam manh giong het
    /// nhau nhu duoc cat bang khuon - do chinh la cai "hinh da giac" ma nhin
    /// vao la biet ngay do khong phai vo that.
    ///
    /// He hat lay MOT o CO DINH cho moi hat (frameOverTime = 0, startFrame ngau
    /// nhien) chu khong chay lan luot qua cac o nhu flipbook khoi va lua.
    ///
    /// Ve TRANG de vat lieu to mau sau - nho vay dung duoc cho manh tim cua
    /// khieng lan manh mau khac neu can.
    /// </summary>
    public static Texture2D ManhVoSheet()
    {
        return Get("manhvo", delegate
        {
            const int Cot = 4, O = 64, S = Cot * O;
            var t = New(S, S, false);
            var px = new Color[S * S];

            var state = Random.state;
            Random.InitState(90210);      // co dinh hat giong cho anh khong doi moi lan chay

            for (int o = 0; o < Cot * Cot; o++)
            {
                int ox = (o % Cot) * O, oy = (o / Cot) * O;

                // Da giac loi 5-7 dinh, ban kinh TUNG DINH khac nhau -> hinh bat
                // dinh, khong phai tam giac hay ngu giac deu
                int n = Random.Range(5, 8);
                var gx = new float[n];
                var gy = new float[n];
                float goc0 = Random.value * 6.2832f;
                for (int i = 0; i < n; i++)
                {
                    float g = goc0 + (i + Random.Range(-0.20f, 0.20f)) * 6.2832f / n;
                    float r = Random.Range(0.18f, 0.44f);
                    gx[i] = Mathf.Cos(g) * r;
                    gy[i] = Mathf.Sin(g) * r * Random.Range(0.65f, 1.15f);
                }

                // Huong MAT VAT: manh vo la khoi ran, mot ben hung sang mot ben
                // toi. Khong co cai nay thi manh phang li nhu mieng giay cat.
                float vg = Random.value * 6.2832f;
                float vatX = Mathf.Cos(vg), vatY = Mathf.Sin(vg);

                for (int y = 0; y < O; y++)
                    for (int x = 0; x < O; x++)
                    {
                        float u = (x + 0.5f) / O - 0.5f;
                        float v = (y + 0.5f) / O - 0.5f;

                        bool trong = false;
                        float ganCanh = 9f;
                        for (int i = 0, j = n - 1; i < n; j = i++)
                        {
                            if (((gy[i] > v) != (gy[j] > v)) &&
                                (u < (gx[j] - gx[i]) * (v - gy[i]) / (gy[j] - gy[i]) + gx[i]))
                                trong = !trong;

                            float ex = gx[j] - gx[i], ey = gy[j] - gy[i];
                            float k = Mathf.Clamp01(((u - gx[i]) * ex + (v - gy[i]) * ey)
                                                    / (ex * ex + ey * ey + 1e-6f));
                            float dx = u - (gx[i] + ex * k), dy = v - (gy[i] + ey * k);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d < ganCanh) ganCanh = d;
                        }

                        if (!trong) { px[(oy + y) * S + ox + x] = new Color(0, 0, 0, 0); continue; }

                        // Ban dau toi cho VIEN SANG DEU quanh ca chu vi: ket qua
                        // ra nhung mieng nhu vien da quy phang li, van "hinh hoc"
                        // y nhu cu. Manh vo that thi khac ba diem:
                        //
                        //   1. MAT VAT manh - mot ben hung sang ro, ben kia toi
                        //      han. Do la thu cho manh co BE DAY.
                        //   2. Chi VAI CANH bat sang, khong phai tat ca. Canh nao
                        //      huong ve phia sang thi ro, con lai chim.
                        //   3. Co VET NUT ben trong.
                        float canh = Mathf.Clamp01(1f - ganCanh * 26f);
                        float vat = Mathf.Clamp01(0.5f + (u * vatX + v * vatY) * 2.6f);

                        // Canh chi sang o nua HUONG VE phia sang
                        float canhSang = canh * Mathf.Clamp01(vat * 1.6f - 0.25f);

                        // Vet nut: mot vai duong manh cat qua than manh
                        float nut = Mathf.Abs(Mathf.Sin((u * 9.3f + v * 6.7f) * 3.1f
                                                        + o * 1.7f));
                        nut = Mathf.Clamp01(1f - nut * 7f) * 0.55f;

                        float sang = 0.30f + vat * 0.70f + canhSang * 0.85f + nut;

                        // Do dac: lan truoc toi ha xuong 0,30 cho "trong nhu manh
                        // nang luong", ket qua la ca dam manh tan thanh nhung dom
                        // mo - mat han hinh manh. Phai du dac de thay ro HINH,
                        // chi hoi trong o giua.
                        float a = Mathf.Clamp01(0.62f + canhSang * 0.38f + vat * 0.25f + nut * 0.4f)
                                * Mathf.Clamp01(ganCanh * O * 1.6f);
                        px[(oy + y) * S + ox + x] = new Color(sang, sang, sang, a);
                    }
            }

            Random.state = state;
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Bong tuyet 6 canh (bui bang bay lo lung).</summary>
    public static Texture2D Snowflake()
    {
        return Get("snow", delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = (x + 0.5f) / S - 0.5f;
                    float v = (y + 0.5f) / S - 0.5f;
                    float r = Mathf.Sqrt(u * u + v * v) * 2f;
                    float ang = Mathf.Atan2(v, u);

                    float a = 0f;
                    for (int k = 0; k < 6; k++)
                    {
                        float ak = k * Mathf.PI / 3f;
                        float d = Mathf.Abs(Mathf.Sin(ang - ak));
                        if (d < 0.06f && r < 1f) a = Mathf.Max(a, 1f - r);
                    }
                    a = Mathf.Max(a, Mathf.Clamp01(1f - r * 6f));
                    px[y * S + x] = new Color(0.85f, 0.95f, 1f, Mathf.Clamp01(a));
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Vong tron sang (song xung kich, vong phep duoi chan).</summary>
    public static Texture2D Ring(float thickness)
    {
        return Get("ring" + thickness, delegate
        {
            const int S = 256;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x + 0.5f) / S - 0.5f;
                    float dy = (y + 0.5f) / S - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.85f) / Mathf.Max(thickness, 0.001f));
                    a = Mathf.Pow(a, 1.6f);
                    px[y * S + x] = new Color(1f, 1f, 1f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// Dai sang cua TIA SET: trang chay o giua, nhoe dan ra hai ben.
    /// Truc ngang (u) la be ngang tia set, truc doc (v) chay doc than tia.
    /// </summary>
    /// <summary>
    /// MAT CAT NGANG cua tia set: loi trang dac o giua, quang xanh toa ra hai
    /// ben. Day khong phai hinh tia set - LightningArc dung mat cat nay keo
    /// doc theo duong gap khuc de thanh tia.
    ///
    /// Uu tien anh dung trong Blender (Cycles + Glare Fog Glow). Quang do Glare
    /// tinh ra mem va lan rong khong deu nhu quang that; ban tinh bang
    /// Mathf.Pow o duoi ra mot vien gradient deu tam tap, nhin ra ong nhua chu
    /// khong ra tia dien. Thieu anh thi van co ban ve tay de dung.
    /// </summary>
    public static Texture2D BoltStrip()
    {
        // Anh Blender tra ve THANG, khong di qua Get.
        //
        // Get dat lai t.name = "Tex_..." cho moi thu no tra ve. Voi anh ve bang
        // code thi khong sao, nhung day la mot ASSET THAT trong Resources - doi
        // ten no la sua asset cua du an tu trong luc chay.
        var blender = Resources.Load<Texture2D>("Vfx/BoltStrip");
        if (blender != null) return blender;

        return Get("boltstrip", delegate
        {

            const int W = 64, H = 128;
            var t = New(W, H, false);
            var px = new Color[W * H];

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    // 0 o giua, 1 o ria
                    float u = Mathf.Abs((x + 0.5f) / W - 0.5f) * 2f;

                    float a = Mathf.Pow(Mathf.Clamp01(1f - u), 2.6f);
                    if (u < 0.16f) a = 1f;                       // loi trang dac

                    // Nhap nhay nhe doc than tia cho do tron nhu ong nuoc
                    float n = Mathf.PerlinNoise(0f, y / 7f);
                    a *= Mathf.Lerp(0.82f, 1f, n);

                    // Giua trang tinh, ra ria nga sang xanh
                    var c = Color.Lerp(new Color(1f, 1f, 1f),
                                       new Color(0.42f, 0.66f, 1f), Mathf.Pow(u, 0.8f));
                    px[y * W + x] = new Color(c.r, c.g, c.b, a);
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Vung sang trang loang tren mat dat noi tia set vua cham xuong.</summary>
    public static Texture2D GlowPool()
    {
        return Get("glowpool", delegate
        {
            const int S = 256;
            var t = New(S, S, false);
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x + 0.5f) / S - 0.5f;
                    float dy = (y + 0.5f) / S - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float ang = Mathf.Atan2(dy, dx);

                    // Loi sang trang day dac, ria toe ra thanh nhung tia nho
                    float core = Mathf.Pow(Mathf.Clamp01(1f - d), 1.5f);
                    float rays = Mathf.Clamp01(Mathf.Sin(ang * 11f) * 0.5f + 0.5f);
                    float halo = Mathf.Pow(Mathf.Clamp01(1f - d), 3f) * rays * 0.45f;

                    float a = Mathf.Clamp01(core * 1.15f + halo);
                    var c = Color.Lerp(new Color(0.62f, 0.82f, 1f), Color.white,
                                       Mathf.Clamp01(core * 1.9f));
                    px[y * S + x] = new Color(c.r, c.g, c.b, a);
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Vet bang tren mat dat (dung lam vet phep "Mua bang").</summary>
    public static Texture2D FrostPatch()
    {
        return Get("frostpatch", delegate
        {
            const int S = 256;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x + 0.5f) / S - 0.5f;
                    float dy = (y + 0.5f) / S - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;

                    float n = Mathf.PerlinNoise(x / 26f, y / 26f) * 0.6f
                            + Mathf.PerlinNoise(x / 9f, y / 9f) * 0.4f;
                    float edge = Mathf.Clamp01(1f - d) * Mathf.Lerp(0.55f, 1.35f, n);
                    float a = Mathf.Clamp01(edge);
                    a = Mathf.Pow(a, 1.5f);

                    // Van bang nut ran
                    float crack = 1f - Mathf.Clamp01(Mathf.Abs(n - 0.5f) * 9f);
                    float c = 0.72f + crack * 0.28f;
                    px[y * S + x] = new Color(c * 0.85f, c * 0.95f, 1f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Vet chay den tren mat dat sau khi qua cau lua no.</summary>
    public static Texture2D ScorchMark()
    {
        return Get("scorch", delegate
        {
            const int S = 256;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = (x + 0.5f) / S - 0.5f;
                    float dy = (y + 0.5f) / S - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float n = Mathf.PerlinNoise(x / 18f + 3f, y / 18f + 7f);
                    float a = Mathf.Clamp01(1f - d) * Mathf.Lerp(0.5f, 1.3f, n);
                    a = Mathf.Pow(Mathf.Clamp01(a), 1.7f);
                    float glow = Mathf.Clamp01(n - 0.55f) * 1.6f * Mathf.Clamp01(1f - d * 1.3f);
                    px[y * S + x] = new Color(0.10f + glow, 0.05f + glow * 0.4f, 0.04f, a);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    // ================================================================
    //  HOA TIET BE MAT (VAI, DA, XUONG, DA TANG...)
    // ================================================================

    /// <summary>Vai det: co soi doc soi ngang va nep nhan.</summary>
    public static Texture2D Cloth(Color a, Color b, float weave)
    {
        return Get("cloth" + a + b + weave, delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float w = (Mathf.Sin(x * 0.7f) * Mathf.Sin(y * 0.7f)) * 0.5f + 0.5f;
                    float n = Mathf.PerlinNoise(x / 30f, y / 30f) * 0.6f
                            + Mathf.PerlinNoise(x / 11f, y / 11f) * 0.4f;
                    float k = Mathf.Clamp01(n * 0.75f + w * weave * 0.35f);
                    px[y * S + x] = Color.Lerp(a, b, k);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Da nguoi / da quai: lo chan long li ti, dom mau khong deu.</summary>
    public static Texture2D Skin(Color baseColor, float blotch, int seed)
    {
        return Get("skin" + baseColor + blotch + seed, delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];
            float o = seed * 17.3f;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float pore = Mathf.PerlinNoise(x / 2.2f + o, y / 2.2f + o) * 0.12f;
                    float n = Mathf.PerlinNoise(x / 40f + o, y / 40f + o) * 0.7f
                            + Mathf.PerlinNoise(x / 13f + o, y / 13f + o) * 0.3f;
                    float k = (n - 0.5f) * blotch + pore - 0.06f;
                    var c = new Color(
                        Mathf.Clamp01(baseColor.r + k),
                        Mathf.Clamp01(baseColor.g + k * 0.85f),
                        Mathf.Clamp01(baseColor.b + k * 0.7f), 1f);
                    px[y * S + x] = c;
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Xuong: hoi vang vang nhat, co vet ran va vet o.</summary>
    public static Texture2D Bone()
    {
        return Get("bone", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float n = Mathf.PerlinNoise(x / 35f, y / 35f);
                    float streak = Mathf.PerlinNoise(x / 3f, y / 60f);
                    float dirt = Mathf.Clamp01(Mathf.PerlinNoise(x / 15f + 4f, y / 15f + 8f) - 0.45f) * 1.6f;
                    float k = 0.78f + n * 0.16f + streak * 0.08f - dirt * 0.35f;
                    px[y * S + x] = new Color(k, k * 0.96f, k * 0.82f, 1f);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Da tang / nen dat da kieu ham nguc.</summary>
    public static Texture2D RockGround(Color dark, Color light, float grain)
    {
        return Get("rock" + dark + light + grain, delegate
        {
            const int S = 512;
            var t = New(S, S, true);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float n = 0f, amp = 0.5f, f = 4f;
                    for (int o = 0; o < 5; o++)
                    {
                        n += amp * Mathf.PerlinNoise(x / (float)S * f + 1.7f, y / (float)S * f + 2.3f);
                        f *= 2.07f; amp *= 0.5f;
                    }
                    // Vet nut giua cac vien da
                    float cell = Mathf.PerlinNoise(x / 26f, y / 26f);
                    float crack = 1f - Mathf.Clamp01(Mathf.Abs(cell - 0.5f) * 12f);
                    float k = Mathf.Clamp01(n * grain + Random.value * 0.03f);
                    var c = Color.Lerp(dark, light, k);
                    c = Color.Lerp(c, dark * 0.75f, crack * 0.35f);
                    px[y * S + x] = c;
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Kim loai co vet xuoc va ri set.</summary>
    // ================================================================
    //  DONG CO (mat dat, bui co, la cay)
    // ================================================================

    /// <summary>Tham co nhin tu tren xuong: nhieu tung dam dam nhat khac nhau.</summary>
    public static Texture2D GrassField()
    {
        return Get("grassfield", delegate
        {
            const int S = 512;
            var t = New(S, S, true);
            var px = new Color[S * S];

            var dark = new Color(0.20f, 0.28f, 0.11f);
            var mid = new Color(0.34f, 0.44f, 0.18f);
            var light = new Color(0.52f, 0.60f, 0.26f);
            var dry = new Color(0.56f, 0.52f, 0.24f);

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    // Nhieu tang van nhieu -> tung dam co to nho khac nhau
                    float n = 0f, amp = 0.5f, f = 3f;
                    for (int o = 0; o < 5; o++)
                    {
                        n += amp * Mathf.PerlinNoise(x / (float)S * f + 3.1f, y / (float)S * f + 8.4f);
                        f *= 2.13f; amp *= 0.5f;
                    }

                    var c = Color.Lerp(dark, mid, Mathf.Clamp01(n * 1.6f));
                    c = Color.Lerp(c, light, Mathf.Clamp01((n - 0.55f) * 2.4f));

                    // Vai mang co uom vang cho do deu mau
                    float dryPatch = Mathf.PerlinNoise(x / 70f + 21f, y / 70f + 13f);
                    c = Color.Lerp(c, dry, Mathf.Clamp01((dryPatch - 0.62f) * 2.2f) * 0.55f);

                    // Soi co manh: nhung net doc rat manh
                    float blade = Mathf.PerlinNoise(x * 0.55f, y * 0.09f);
                    c *= Mathf.Lerp(0.86f, 1.14f, blade);

                    px[y * S + x] = c;
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// DAT NAU DEN: lop dat thit am, san sui, lam tam soi da nho va vet nut.
    /// Day la lop nen chinh cua ca man choi.
    /// </summary>
    public static Texture2D DarkSoil()
    {
        return Get("darksoil", delegate
        {
            const int S = 512;
            var t = New(S, S, true);
            var px = new Color[S * S];

            // Sac nau phai rat dam vi anh trang mau xanh se lam nhat mau di nhieu
            var deep = new Color(0.135f, 0.072f, 0.032f);   // dat am sam nhat
            var mid = new Color(0.300f, 0.180f, 0.088f);
            var dry = new Color(0.470f, 0.312f, 0.172f);    // cho kho hon, nga cat

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    // Nhieu tang -> mang dat cho am cho kho
                    float n = 0f, amp = 0.5f, f = 3f;
                    for (int o = 0; o < 5; o++)
                    {
                        n += amp * Mathf.PerlinNoise(x / (float)S * f + 17f, y / (float)S * f + 41f);
                        f *= 2.07f; amp *= 0.5f;
                    }

                    var c = Color.Lerp(deep, mid, Mathf.Clamp01(n * 1.5f));
                    c = Color.Lerp(c, dry, Mathf.Clamp01((n - 0.58f) * 2.3f));

                    // Hat cat li ti lam mat dat khong bi min nhu nhua
                    float grain = Mathf.PerlinNoise(x * 0.9f + 3f, y * 0.9f + 8f);
                    c *= Mathf.Lerp(0.80f, 1.22f, grain);

                    // Soi da nho lo ra
                    float peb = Mathf.PerlinNoise(x / 6.5f + 77f, y / 6.5f + 23f);
                    if (peb > 0.80f)
                        c = Color.Lerp(c, new Color(0.42f, 0.37f, 0.30f), (peb - 0.80f) * 3.2f);

                    // Vet nut ran nho
                    float crack = 1f - Mathf.Clamp01(Mathf.Abs(n - 0.5f) * 14f);
                    c *= Mathf.Lerp(1f, 0.62f, crack);

                    px[y * S + x] = c;
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Dat bun bi giam nat: nau xam, co soi da nho va vet lom.</summary>
    public static Texture2D MudGround()
    {
        return Get("mud", delegate
        {
            const int S = 512;
            var t = New(S, S, true);
            var px = new Color[S * S];

            var wet = new Color(0.26f, 0.17f, 0.10f);
            var dryDirt = new Color(0.62f, 0.46f, 0.29f);

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float n = 0f, amp = 0.5f, f = 4f;
                    for (int o = 0; o < 5; o++)
                    {
                        n += amp * Mathf.PerlinNoise(x / (float)S * f + 6.6f, y / (float)S * f + 1.2f);
                        f *= 2.05f; amp *= 0.5f;
                    }

                    var c = Color.Lerp(wet, dryDirt, Mathf.Clamp01(n * 1.5f));

                    // Soi da nho lam tam trong bun
                    float peb = Mathf.PerlinNoise(x / 5.5f + 44f, y / 5.5f + 17f);
                    if (peb > 0.78f) c = Color.Lerp(c, new Color(0.58f, 0.56f, 0.52f), (peb - 0.78f) * 3.4f);

                    // Vet banh xe / vet chan giam lom xuong
                    float rut = Mathf.PerlinNoise(x / 42f, y / 12f);
                    c *= Mathf.Lerp(0.78f, 1.06f, rut);

                    px[y * S + x] = c;
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// Mot bui co: nhieu la manh moc toe ra tu goc, nen trong suot.
    /// Dung dan len mieng vai cat cheo nhau.
    /// </summary>
    public static Texture2D GrassTuft()
    {
        Color root = GrassRoot, tip = GrassTip;
        return Get("grasstuft" + root + tip, delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0f, 0f, 0f, 0f);

            var state = Random.state;
            Random.InitState(4242);

            int blades = 13;
            for (int b = 0; b < blades; b++)
            {
                float rootX = Random.Range(0.24f, 0.76f);
                float height = Random.Range(0.45f, 0.98f);
                float lean = Random.Range(-0.30f, 0.30f);
                float width = Random.Range(0.020f, 0.040f);
                float shade = Random.Range(0.62f, 1f);

                // Ve tung la theo duong cong
                int steps = 90;
                for (int s = 0; s <= steps; s++)
                {
                    float k = s / (float)steps;
                    float yy = k * height;
                    // Cong dan ve phia ngon
                    float xx = rootX + lean * k * k;
                    float w = width * (1f - k * 0.92f);      // nhon dan len ngon

                    int cx = Mathf.RoundToInt(xx * S);
                    int cy = Mathf.RoundToInt(yy * S);
                    int rad = Mathf.Max(1, Mathf.RoundToInt(w * S));

                    for (int oy = -rad; oy <= rad; oy++)
                        for (int ox = -rad; ox <= rad; ox++)
                        {
                            int qx = cx + ox, qy = cy + oy;
                            if (qx < 0 || qx >= S || qy < 0 || qy >= S) continue;

                            float d = Mathf.Sqrt(ox * ox + oy * oy) / Mathf.Max(1f, rad);
                            if (d > 1f) continue;

                            // Goc la sam, ngon la sang hon
                            var c = Color.Lerp(root, tip, k);
                            c *= shade;

                            float a = Mathf.Clamp01((1f - d) * 2.2f);
                            int idx = qy * S + qx;
                            if (a > px[idx].a) px[idx] = new Color(c.r, c.g, c.b, a);
                        }
                }
            }

            Random.state = state;
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Mot chum la bui ram: nhieu dom la tron chong len nhau.</summary>
    public static Texture2D LeafClump()
    {
        Color sang = LeafLight, toi = LeafDark;
        return Get("leafclump" + sang + toi, delegate
        {
            const int S = 128;
            var t = New(S, S, false);
            var px = new Color[S * S];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0f, 0f, 0f, 0f);

            var state = Random.state;
            Random.InitState(909);

            for (int b = 0; b < 26; b++)
            {
                float cx = Random.Range(0.18f, 0.82f) * S;
                float cy = Random.Range(0.12f, 0.86f) * S;
                float rx = Random.Range(0.055f, 0.135f) * S;
                float ry = rx * Random.Range(0.45f, 0.85f);
                float ang = Random.Range(0f, Mathf.PI);
                float shade = Random.Range(0.55f, 1f);

                float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);

                int r = Mathf.CeilToInt(Mathf.Max(rx, ry)) + 1;
                for (int oy = -r; oy <= r; oy++)
                    for (int ox = -r; ox <= r; ox++)
                    {
                        int qx = Mathf.RoundToInt(cx) + ox;
                        int qy = Mathf.RoundToInt(cy) + oy;
                        if (qx < 0 || qx >= S || qy < 0 || qy >= S) continue;

                        // Xoay ve he truc cua chiec la
                        float lx = (ox * ca + oy * sa) / rx;
                        float ly = (-ox * sa + oy * ca) / ry;
                        float d = lx * lx + ly * ly;
                        if (d > 1f) continue;

                        var c = Color.Lerp(sang, toi, Mathf.Sqrt(d));
                        c *= shade;

                        // Gan la co song giua sang hon
                        if (Mathf.Abs(ly) < 0.12f) c *= 1.25f;

                        float a = Mathf.Clamp01((1f - d) * 3f);
                        int idx = qy * S + qx;
                        if (a > px[idx].a) px[idx] = new Color(c.r, c.g, c.b, a);
                    }
            }

            Random.state = state;
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// THAN LOC XOAY: nhung vet bui keo xien cuon quanh than loc.
    /// Anh lap duoc theo chieu ngang de dan quanh hinh non ma khong thay duong noi.
    /// </summary>
    public static Texture2D TornadoWall()
    {
        return Get("tornadowall", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];
            const float TAU = Mathf.PI * 2f;

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = x / (float)S;
                    float v = y / (float)S;

                    // Keo xien 2 vong dung: SO NGUYEN nen khi anh cuon het mot vong
                    // theo chieu doc thi hoa tiet noi lai khop, khong lo vach cat.
                    float su = u + v * 2f;

                    // Nhieu tuan hoan tren CA HAI chieu (dung tan so nguyen), nho vay
                    // cuon mai cung khong bao gio lo duong noi.
                    float n = 0f, amp = 0.5f;
                    int[] freq = { 3, 6, 12, 24 };
                    for (int o = 0; o < freq.Length; o++)
                    {
                        float au = su * TAU * freq[o];
                        float av = v * TAU * freq[o];
                        n += amp * Mathf.PerlinNoise(
                                 Mathf.Cos(au) * 1.7f + Mathf.Sin(av) * 0.9f + 30f,
                                 Mathf.Sin(au) * 1.7f + Mathf.Cos(av) * 0.9f + 12f);
                        amp *= 0.5f;
                    }

                    // Soi bui manh chay doc huong xoan (13 vong -> lap kin)
                    float streak = Mathf.Abs(Mathf.Sin(su * TAU * 13f + n * 4f));
                    streak = Mathf.Pow(streak, 0.75f);

                    // Vach TO va thua (3 vong -> lap kin): day la thu lam nguoi choi
                    // nhin ra ngay la loc dang cuon chu khong phai dung yen
                    float band = Mathf.Abs(Mathf.Sin(su * TAU * 3f + n * 1.6f));
                    band = Mathf.Pow(band, 1.8f);

                    float a = Mathf.Clamp01(n * 1.25f)
                            * Mathf.Lerp(0.35f, 1f, streak)
                            * Mathf.Lerp(0.55f, 1.25f, band);

                    // Anh chi giu HINH, khong giu mau: mau xam trang nhat dan len cao
                    // va do mo o hai dau deu nam trong MAU DINH cua luoi. De o day thi
                    // khi anh cuon doc, ca dai mau lan dai mo se cuon theo -> lo vach.
                    float sang = Mathf.Lerp(0.78f, 1f, n) + band * 0.22f;
                    sang = Mathf.Clamp01(sang);

                    px[y * S + x] = new Color(sang, sang, sang, Mathf.Clamp01(a));
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// Mot vach sang mem, dung cho nhung dai xoan quan quanh than loc.
    /// Dam nhat o giua be ngang, nhat dan ve hai mep; doc theo chieu dai thi
    /// dut quang thanh tung doan cho giong luong bui bi cuon chu khong phai day thung.
    /// </summary>
    public static Texture2D SpiralStreak()
    {
        return Get("spiralstreak", delegate
        {
            const int S = 128;
            var t = New(S, S, true);
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = x / (float)S;      // doc theo chieu dai dai
                    float v = y / (float)S;      // ngang qua be rong dai

                    // Mem dan ve hai mep
                    float across = 1f - Mathf.Abs(v - 0.5f) * 2f;
                    across = Mathf.Pow(Mathf.Clamp01(across), 1.4f);

                    // Dut quang doc theo chieu dai, lap kin duoc de truot khong lo moi noi
                    float n = Mathf.PerlinNoise(Mathf.Cos(u * Mathf.PI * 2f) * 2.1f + 11f,
                                                Mathf.Sin(u * Mathf.PI * 2f) * 2.1f + v * 3f);
                    float along = Mathf.Lerp(0.30f, 1f, Mathf.Clamp01(n * 1.5f));

                    float a = across * along;
                    px[y * S + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// DA SA THACH NAU DO: than da mau gach nung, xen ke via sam mau, khe nut den
    /// va nhung vet khoang xam lanh - giong tang da that trong anh mau.
    /// </summary>
    /// <summary>
    /// Lay MOT diem tren mat da: vua ra mau, vua ra do CAO THAP cua be mat.
    ///
    /// Hai thu nay phai tinh chung mot cho thi khe nut moi lom dung vao cho toi,
    /// hat da moi noi dung cho sang. Tach ra hai ham rieng la lech nhau ngay.
    /// </summary>
    static void RockSample(float u, float v, int x, int y, out Color mau, out float cao)
    {
        // Sac do phai rat manh vi anh trang xanh se lam nhat mau di nhieu
        var dark = new Color(0.19f, 0.090f, 0.055f);   // day khe nut
        var mid = new Color(0.54f, 0.290f, 0.165f);    // than da, mau gach nung
        var pale = new Color(0.86f, 0.545f, 0.360f);   // cho bi nang mai mon
        var grey = new Color(0.44f, 0.40f, 0.38f);     // vet khoang xam lanh

        // Nhieu nen
        float n = 0f, amp = 0.5f, f = 3f;
        for (int o = 0; o < 4; o++)
        {
            n += amp * Mathf.PerlinNoise(u * f * 4f + 61f, v * f * 4f + 13f);
            f *= 2.1f; amp *= 0.5f;
        }

        // Via da nam ngang nhung BI UON LUON MANH, khong ke soc deu tam tap
        float warp = Mathf.PerlinNoise(u * 2.2f + 7f, v * 1.4f) * 0.55f
                   + Mathf.PerlinNoise(u * 6.5f + 21f, v * 4.5f) * 0.22f;
        float band = Mathf.Sin((v * 4.5f + warp) * Mathf.PI * 2f);
        float layer = Mathf.Pow(Mathf.Abs(band), 0.7f);

        var c = Color.Lerp(dark, mid, Mathf.Clamp01(n * 1.5f));
        c = Color.Lerp(c, pale, Mathf.Clamp01((n - 0.55f) * 2.2f));
        c = Color.Lerp(c * 0.88f, c * 1.08f, layer);

        // Nhung mang khoang xam lanh loang tren nen da do
        float mineral = Mathf.PerlinNoise(u * 2.6f + 130f, v * 2.0f + 66f);
        c = Color.Lerp(c, grey, Mathf.Clamp01((mineral - 0.66f) * 2.4f) * 0.50f);

        // Khe nut: chi hien o mot so cho chu khong chay het vong quanh
        float crackMask = Mathf.PerlinNoise(u * 3.1f + 55f, v * 2.4f + 71f);
        float crack = (1f - Mathf.Clamp01(Mathf.Abs(band) * 9f))
                    * Mathf.Clamp01((crackMask - 0.42f) * 2.6f);
        c *= Mathf.Lerp(1f, 0.52f, crack);

        // Bui dat sam dong lai trong khe nut
        float grime = Mathf.PerlinNoise(u * 3.5f + 44f, v * 3.5f + 90f);
        c = Color.Lerp(c, dark, Mathf.Clamp01((grime - 0.58f) * 1.9f) * (0.30f + crack * 0.55f));

        // Hat da li ti
        float grain = Mathf.PerlinNoise(x * 1.1f, y * 1.1f);
        c *= Mathf.Lerp(0.90f, 1.10f, grain);

        // Nhung ho ram nho li ti do da bi mua gio bao mon - chinh no lam mat da
        // "ram" chu khong tron nhu da mai
        float ram = Mathf.PerlinNoise(u * 46f + 200f, v * 46f + 310f);
        float ho = Mathf.Clamp01((ram - 0.62f) * 3.4f);
        c *= Mathf.Lerp(1f, 0.80f, ho);

        mau = c;

        // ---- Do cao be mat ----
        // via da noi len, khe nut an sau, hat va ho ram lam nham be mat
        cao = 0.45f
            + layer * 0.22f          // via da go len
            + n * 0.16f              // nhap nho lon
            + grain * 0.10f          // hat li ti
            - crack * 0.55f          // khe nut lom sau
            - ho * 0.26f;            // ho ram
        cao = Mathf.Clamp01(cao);
    }

    /// <summary>DA SA THACH NAU DO: mau gach nung, via sam, khe nut den, mang khoang xam.</summary>
    public static Texture2D CliffRock()
    {
        return Get("cliffrock", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    Color c; float h;
                    RockSample(x / (float)S, y / (float)S, x, y, out c, out h);
                    px[y * S + x] = c;
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// BAN DO PHAP TUYEN cua mat da - thu lam cho da het tron.
    ///
    /// Anh mau chi noi cho nao dam cho nao nhat; may van to ca mat da phang ly.
    /// Ban do phap tuyen noi cho may biet TUNG DIEM tren mat da NGHIENG ve huong nao,
    /// nho vay anh trang hat vao se an vao khe nut va dong lai tren via da noi -
    /// dung nhu da that.
    ///
    /// Ten co chu "normal" de luc nuong ra file, cong cu biet ma dat dung kieu anh.
    /// </summary>
    public static Texture2D CliffRockNormal()
    {
        return Get("cliffrock_normal", delegate
        {
            const int S = 256;
            const float manh = 3.6f;      // cang lon mat da cang go ghe

            var cao = new float[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    Color c; float h;
                    RockSample(x / (float)S, y / (float)S, x, y, out c, out h);
                    cao[y * S + x] = h;
                }

            var t = New(S, S, true);
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    // Lay chenh lech do cao hai ben (vong lai o mep cho anh lap kin)
                    int xl = (x - 1 + S) % S, xr = (x + 1) % S;
                    int yd = (y - 1 + S) % S, yu = (y + 1) % S;

                    float dx = (cao[y * S + xr] - cao[y * S + xl]) * manh;
                    float dy = (cao[yu * S + x] - cao[yd * S + x]) * manh;

                    var n = new Vector3(-dx, -dy, 1f).normalized;
                    px[y * S + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>
    /// LOP CHI TIET cua mat da - hat va ho ram li ti, lap rat day.
    ///
    /// Lay muc XAM 0.5 lam goc chu khong lay anh da that. Ly do: Unity nhan doi anh
    /// lop chi tiet roi moi chong len anh chinh, nen chi co xam 0.5 moi la "khong doi
    /// gi ca". Dem anh da (trung binh khoang 0.4) vao thi ca khoi da toi di gan 20%.
    /// </summary>
    public static Texture2D CliffRockDetail()
    {
        return Get("cliffrock_detail", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    Color c; float h;
                    RockSample(x / (float)S, y / (float)S, x, y, out c, out h);

                    // Do sang cua diem, keo ve quanh muc 0.5
                    float sang = c.r * 0.35f + c.g * 0.45f + c.b * 0.20f;
                    // 0.27 la do sang trung binh THUC DO duoc cua anh da; lay dung
                    // so do lam moc thi anh chi tiet moi trung tinh, khong lam da toi di
                    float k = 0.5f + (sang - 0.27f) * 0.55f;
                    k = Mathf.Clamp01(k);

                    px[y * S + x] = new Color(k, k, k, 1f);
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Da phu reu: xam o duoi, xanh reu loang o tren.</summary>
    public static Texture2D MossyRock()
    {
        return Get("mossyrock", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float n = 0f, amp = 0.5f, f = 4f;
                    for (int o = 0; o < 4; o++)
                    {
                        n += amp * Mathf.PerlinNoise(x / (float)S * f + 2.9f, y / (float)S * f + 5.5f);
                        f *= 2.1f; amp *= 0.5f;
                    }

                    var c = Color.Lerp(new Color(0.30f, 0.30f, 0.29f),
                                       new Color(0.62f, 0.62f, 0.59f), Mathf.Clamp01(n * 1.4f));

                    // Mang reu loang lo
                    float moss = Mathf.PerlinNoise(x / 34f + 61f, y / 34f + 29f)
                               + Mathf.PerlinNoise(x / 11f, y / 11f) * 0.35f;
                    c = Color.Lerp(c, new Color(0.24f, 0.34f, 0.15f),
                                   Mathf.Clamp01((moss - 0.72f) * 2.6f));

                    px[y * S + x] = c;
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    public static Texture2D Metal(Color baseColor, float rust)
    {
        return Get("metal" + baseColor + rust, delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float scratch = Mathf.PerlinNoise(x / 1.5f, y / 45f) * 0.18f;
                    float n = Mathf.PerlinNoise(x / 28f, y / 28f);
                    float r = Mathf.Clamp01(n - 0.55f) * 2f * rust;
                    var c = baseColor * (0.82f + scratch + n * 0.2f);
                    c = Color.Lerp(c, new Color(0.35f, 0.15f, 0.06f), r);
                    c.a = 1f;
                    px[y * S + x] = c;
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    /// <summary>Go muc (than cay chet, can duoc).</summary>
    /// <summary>
    /// VO CAY SAN SUI: nhung song doc chay tu goc len ngon, day khe nut den kit,
    /// song vo mau nau, va nhung mang reu / dia y xanh nhat bam loang lo.
    /// </summary>
    public static Texture2D Bark()
    {
        return Get("bark", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];

            var crack = new Color(0.028f, 0.022f, 0.018f);   // day khe nut - den kit
            var mid = new Color(0.175f, 0.125f, 0.085f);     // vo cay
            var ridge = new Color(0.54f, 0.375f, 0.215f);    // song vo noi len - nau
            var lichen = new Color(0.44f, 0.52f, 0.285f);    // reu / dia y xanh nhat

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = x / (float)S;      // vong quanh than
                    float v = y / (float)S;      // doc theo than

                    // Song doc: it song hon nhung TO va uon luon manh -> ra vo cay
                    // san sui chu khong phai nhung soi chi ke deu tam tap
                    float warp = Mathf.PerlinNoise(u * 3f + 11f, v * 1.3f) * 1.1f
                               + Mathf.PerlinNoise(u * 8f + 3f, v * 3.5f) * 0.45f;
                    float ridges = Mathf.Sin((u * 5f + warp * 0.9f) * Mathf.PI * 2f);

                    // Khe nut: cho nao song cham nhau thi lom sau xuong
                    float groove = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(ridges)), 0.55f);

                    // Vo cay bi dut doan theo chieu doc -> tung mang vay
                    float scale = Mathf.PerlinNoise(u * 5f + 40f, v * 11f);
                    float chunk = Mathf.Clamp01((scale - 0.34f) * 2.2f);

                    var c = Color.Lerp(crack, mid, Mathf.Clamp01(Mathf.Pow(groove, 1.5f) * 1.5f));
                    c = Color.Lerp(c, ridge, Mathf.Clamp01((groove - 0.42f) * 2.0f) * chunk);

                    // Khe nut ngang cat qua song doc
                    float split = Mathf.PerlinNoise(u * 1.8f + 70f, v * 14f);
                    c = Color.Lerp(c, crack, Mathf.Clamp01((split - 0.72f) * 3.5f));

                    // Mang reu xanh bam tung cho
                    float moss = Mathf.PerlinNoise(u * 1.9f + 88f, v * 1.6f + 15f)
                               + Mathf.PerlinNoise(u * 5f, v * 4f) * 0.28f;
                    c = Color.Lerp(c, lichen, Mathf.Clamp01((moss - 0.60f) * 2.0f) * (0.50f + groove * 0.45f));

                    // Hat go li ti
                    float grain = Mathf.PerlinNoise(x * 1.3f, y * 0.7f);
                    c *= Mathf.Lerp(0.86f, 1.14f, grain);

                    px[y * S + x] = c;
                }

            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }

    public static Texture2D Wood()
    {
        return Get("wood", delegate
        {
            const int S = 256;
            var t = New(S, S, true);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float grain = Mathf.Sin(x * 0.35f + Mathf.PerlinNoise(x / 20f, y / 90f) * 6f) * 0.5f + 0.5f;
                    float n = Mathf.PerlinNoise(x / 40f, y / 12f);
                    float k = 0.35f + grain * 0.22f + n * 0.2f;
                    px[y * S + x] = new Color(k * 0.55f, k * 0.40f, k * 0.28f, 1f);
                }
            t.SetPixels(px); t.Apply(true);
            return t;
        });
    }
}
