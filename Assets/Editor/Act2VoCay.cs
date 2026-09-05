using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// DUNG BO ANH VO CAY CHO ACT2: san sui, nam moc, va troc lo go ben duoi.
///
/// Sinh ra hai anh gach duoc (tileable) 1024x1024:
///
///   VoCay_Mau2.png   mau vo   (kenh alpha = cho co nhua cay ri ra)
///   VoCay_Gan2.png   anh gan  (normal map, dung tu do cao)
///
/// ===================================================================
///  VI SAO SINH BANG CODE CHU KHONG VE TAY
/// ===================================================================
///
/// Shader <c>Diablo25D/BarkTriplanar</c> chieu anh tu ba huong roi tron lai -
/// vi luoi cay dua tu Blender KHONG CO toa do anh. Chieu kieu do thi anh phai
/// <b>gach duoc</b> theo ca hai chieu, khong duoc co duong noi. Anh chup that
/// hay anh ve tay deu phai xu ly lai cho lien mach; sinh bang nhieu tuan hoan
/// thi lien mach san.
///
/// Moi lop nhieu deu lay chu ki bang dung so o cua no (<c>Mod(xi, perX)</c>),
/// nen manh nao cung khop voi manh ben canh.
///
/// ===================================================================
///  BON LOP LAM NEN VE NGOAI CUA VO CAY
/// ===================================================================
///
/// <code>
///   VAN DOC    nhieu day theo chieu ngang, thua theo chieu doc (26 x 5)
///              -> van chay DOC than, dung nhu vo cay that
///   RANH NUT   nhieu "song nui" - vach sac, khong tron - lam khe nut sau
///   MANG TROC  nhieu tan so THAP (5 x 4) roi lay nguong -> tung mang vo bong
///              ra, lo go SANG ben duoi. Day la cai nhin ra ro nhat.
///   NAM MOC    dom xam xanh, chi bam vao cho vo con nguyen - go moi troc thi
///              chua kip moc
/// </code>
///
/// Mep mang troc duoc lam <b>tham lai va go len</b> mot vanh hep: cho vo con
/// dinh dang nham vao go. Thieu cai vanh do thi mang troc nhin nhu mot vet son
/// dan len, khong ra vet bong.
/// </summary>
public static class Act2VoCay
{
    public const int DoMin = 1024;

    const string ThuMuc = "Assets/BlenderMaps/GraveyardAct2/CayDaChua";
    const string DuongMau = ThuMuc + "/VoCay_Mau2.png";
    const string DuongGan = ThuMuc + "/VoCay_Gan2.png";
    const string DuongMat = ThuMuc + "/Act2_VoCay_SanSui.mat";

    // ================================================================
    //  NHIEU GACH DUOC
    // ================================================================

    static float Bam(int x, int y, int hat)
    {
        unchecked
        {
            int n = x * 374761393 + y * 668265263 + hat * 1442695040;
            n = (n ^ (n >> 13)) * 1274126177;
            n = n ^ (n >> 16);
            return (n & 0x7fffffff) / 2147483647f;
        }
    }

    static int Mod(int a, int b) { int r = a % b; return r < 0 ? r + b : r; }

    /// <summary>Nhieu gia tri, lap lai dung sau perX / perY o.</summary>
    static float Nhieu(float x, float y, int perX, int perY, int hat)
    {
        int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
        float xf = x - xi, yf = y - yi;
        float u = xf * xf * (3f - 2f * xf), v = yf * yf * (3f - 2f * yf);
        float a = Bam(Mod(xi, perX), Mod(yi, perY), hat);
        float b = Bam(Mod(xi + 1, perX), Mod(yi, perY), hat);
        float c = Bam(Mod(xi, perX), Mod(yi + 1, perY), hat);
        float d = Bam(Mod(xi + 1, perX), Mod(yi + 1, perY), hat);
        return Mathf.Lerp(Mathf.Lerp(a, b, u), Mathf.Lerp(c, d, u), v);
    }

    static float Fbm(float u, float v, int perX, int perY, int hat, int lop, float tat)
    {
        float tong = 0f, bd = 1f, tongBd = 0f;
        int px = perX, py = perY;
        for (int i = 0; i < lop; i++)
        {
            tong += bd * Nhieu(u * px, v * py, px, py, hat + i * 7919);
            tongBd += bd;
            bd *= tat;
            px *= 2; py *= 2;
        }
        return tong / tongBd;
    }

    /// <summary>Nhieu "song nui" - dinh sac chu khong tron. Dung cho khe nut.</summary>
    static float Song(float u, float v, int perX, int perY, int hat, int lop)
    {
        float tong = 0f, bd = 1f, tongBd = 0f;
        int px = perX, py = perY;
        for (int i = 0; i < lop; i++)
        {
            float n = Nhieu(u * px, v * py, px, py, hat + i * 4211);
            tong += bd * (1f - Mathf.Abs(2f * n - 1f));
            tongBd += bd;
            bd *= 0.55f;
            px *= 2; py *= 2;
        }
        return tong / tongBd;
    }

    // ================================================================
    //  BON LOP
    // ================================================================

    static float VanDoc(float u, float v) { return Fbm(u, v, 26, 5, 1301, 5, 0.55f); }
    static float RanhNut(float u, float v) { return Song(u, v, 14, 3, 4507, 4); }
    static float MangTroc(float u, float v) { return Fbm(u, v, 5, 4, 8821, 4, 0.5f); }
    static float NamMoc(float u, float v) { return Fbm(u, v, 11, 9, 6113, 4, 0.6f); }
    static float HatSan(float u, float v) { return Fbm(u, v, 96, 96, 2237, 2, 0.5f); }

    /// <summary>Muc "da vao trong mang troc": 0 = vo con nguyen, 1 = troc han.</summary>
    static float MucTroc(float troc) { return Mathf.InverseLerp(0.50f, 0.60f, troc); }

    /// <summary>Dai hep chay doc theo MEP mang troc.</summary>
    static float VanhMep(float troc, float rong)
    {
        float t = (troc - 0.50f) / rong;
        return Mathf.Exp(-t * t);
    }

    // ================================================================
    //  DIEM VAO
    // ================================================================

    [MenuItem("Diablo 2.5D/15. Dung lai anh vo cay Act2", false, 96)]
    public static void Dung()
    {
        var cao = new float[DoMin * DoMin];
        var pMau = new Color32[DoMin * DoMin];

        // Mau vo toi / vo sang, mau go lo ra khi troc, mau nam moc
        var voToi = new Vector3(0.105f, 0.098f, 0.086f);
        var voSang = new Vector3(0.30f, 0.285f, 0.25f);
        var goLo = new Vector3(0.74f, 0.70f, 0.60f);
        var namXam = new Vector3(0.52f, 0.55f, 0.50f);

        for (int y = 0; y < DoMin; y++)
        {
            float v = y / (float)DoMin;
            for (int x = 0; x < DoMin; x++)
            {
                float u = x / (float)DoMin;
                int i = y * DoMin + x;

                float van = VanDoc(u, v);
                float nut = RanhNut(u, v);
                float troc = MangTroc(u, v);
                float nam = NamMoc(u, v);
                float hat = HatSan(u, v);

                float m = MucTroc(troc);

                // ---------------- DO CAO ----------------
                float h = 0.45f + (van - 0.5f) * 0.75f;      // go song chay doc
                h -= Mathf.Pow(nut, 3.2f) * 0.62f;           // khe nut an sau
                h += (hat - 0.5f) * 0.16f;                   // hat san li ti
                h -= m * 0.30f;                              // mang troc sut xuong mot bac
                h += VanhMep(troc, 0.035f) * 0.22f;          // mep troc go len thanh vanh
                cao[i] = Mathf.Clamp01(h);

                // ---------------- MAU ----------------
                float t = Mathf.Clamp01(van * 1.15f - 0.08f);
                var c = Vector3.Lerp(voToi, voSang, t);

                c *= 1f - Mathf.Pow(nut, 3.0f) * 0.72f;      // day khe gan den

                // Trong long mang troc van phai co van go, khong thi thanh mang
                // phang mot mau, nhin nhu vet son chu khong ra go
                var go = goLo * (0.72f + hat * 0.45f + (van - 0.5f) * 0.22f);
                c = Vector3.Lerp(c, go, m);
                c *= 1f - VanhMep(troc, 0.030f) * 0.45f;     // mep troc tham lai

                // Nam moc chi bam vao cho vo con nguyen
                float dom = Mathf.InverseLerp(0.56f, 0.70f, nam) * (1f - m * 0.85f);
                c = Vector3.Lerp(c, namXam, dom * 0.55f);

                c *= 0.86f + hat * 0.28f;

                // Kenh alpha = nhua cay: chi ri o day khe nut sau, va khong ri
                // o cho da troc het vo
                float nhua = Mathf.InverseLerp(0.80f, 0.96f, nut) * (1f - m);

                pMau[i] = new Color32(
                    (byte)(Mathf.Clamp01(c.x) * 255f),
                    (byte)(Mathf.Clamp01(c.y) * 255f),
                    (byte)(Mathf.Clamp01(c.z) * 255f),
                    (byte)(Mathf.Clamp01(nhua) * 255f));
            }
        }

        GhiPng(DuongMau, pMau, TextureFormat.RGBA32);
        GhiPng(DuongGan, TuDoCaoRaGan(cao, 5.5f), TextureFormat.RGB24);

        AssetDatabase.Refresh();
        DatKieuNhap();
        GanVaoVatLieu();

        Debug.Log(string.Format("[Act2VoCay] da dung {0}x{0}: {1} va {2}", DoMin, DuongMau, DuongGan));
    }

    /// <summary>
    /// Doi ban do do cao thanh anh gan.
    ///
    /// Lay hieu hai o hai ben (<c>hL - hR</c>) chu khong lay hieu voi chinh no:
    /// sai phan hai phia doi xung nen khong bi lech nua diem anh. Va chi so o
    /// vong lai bang <c>Mod</c> - anh gach duoc thi anh gan cung phai gach duoc,
    /// khong thi bon canh hien ra thanh bon duong noi sang.
    /// </summary>
    static Color32[] TuDoCaoRaGan(float[] cao, float manh)
    {
        var ra = new Color32[DoMin * DoMin];
        for (int y = 0; y < DoMin; y++)
            for (int x = 0; x < DoMin; x++)
            {
                float hL = cao[y * DoMin + Mod(x - 1, DoMin)];
                float hR = cao[y * DoMin + Mod(x + 1, DoMin)];
                float hD = cao[Mod(y - 1, DoMin) * DoMin + x];
                float hU = cao[Mod(y + 1, DoMin) * DoMin + x];

                var n = new Vector3((hL - hR) * manh, (hD - hU) * manh, 1f).normalized;
                ra[y * DoMin + x] = new Color32(
                    (byte)((n.x * 0.5f + 0.5f) * 255f),
                    (byte)((n.y * 0.5f + 0.5f) * 255f),
                    (byte)((n.z * 0.5f + 0.5f) * 255f), 255);
            }
        return ra;
    }

    static void GhiPng(string duong, Color32[] diem, TextureFormat dinhDang)
    {
        var tex = new Texture2D(DoMin, DoMin, dinhDang, false);
        tex.SetPixels32(diem);
        tex.Apply();
        File.WriteAllBytes(duong, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    /// <summary>
    /// Anh gan PHAI nhap kieu NormalMap.
    ///
    /// De kieu Default thi Unity coi no la anh mau thuong, va <c>UnpackNormal</c>
    /// trong shader giai sai - be mat hien ra phang li nhu chua he co anh gan.
    /// Mau vo thi nguoc lai: phai giu sRGB, khong thi ca than cay bac phech.
    /// </summary>
    static void DatKieuNhap()
    {
        var tiMau = AssetImporter.GetAtPath(DuongMau) as TextureImporter;
        if (tiMau != null)
        {
            tiMau.textureType = TextureImporterType.Default;
            tiMau.sRGBTexture = true;
            tiMau.alphaSource = TextureImporterAlphaSource.FromInput;
            tiMau.alphaIsTransparency = false;
            tiMau.wrapMode = TextureWrapMode.Repeat;
            tiMau.maxTextureSize = 1024;
            tiMau.SaveAndReimport();
        }

        var tiGan = AssetImporter.GetAtPath(DuongGan) as TextureImporter;
        if (tiGan != null)
        {
            tiGan.textureType = TextureImporterType.NormalMap;
            tiGan.wrapMode = TextureWrapMode.Repeat;
            tiGan.maxTextureSize = 1024;
            tiGan.SaveAndReimport();
        }
    }

    static void GanVaoVatLieu()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(DuongMat);
        if (mat == null) { Debug.LogError("[Act2VoCay] khong thay " + DuongMat); return; }

        mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(DuongMau));
        mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(DuongGan));

        // Mau nen gan TRANG. Anh moi da tu mang du mau va do tuong phan; nhan
        // them mot mau toi (ban cu de 0,115) thi mang troc bi dim xuong bang
        // voi vo, va cong sinh anh do khong con thay o dau.
        mat.SetColor("_Color", new Color(0.88f, 0.86f, 0.82f, 1f));
        mat.SetFloat("_TiLe", 0.85f);        // van to hon -> nhin ro tung mang troc
        mat.SetFloat("_KeoDoc", 2.30f);
        mat.SetFloat("_DoSanSui", 2.60f);
        mat.SetFloat("_Glossiness", 0.055f);
        mat.SetFloat("_XanhReu", 0.20f);     // ha xuong: reu xanh phu len ca cho troc
        mat.SetFloat("_NhuaMuc", 0.30f);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
    }
}
