using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// DUNG BO ANH DA MO CHO ACT2: muc nat, sut me, reu bam, moc va vet mau.
///
/// Sinh ra ba anh gach duoc (tileable) 1024x1024 va gan vao ba vat lieu:
///
///   DaMo_Mau.png    RGB = da muc,  A = VET VO
///   DaMo_Gan.png    anh gan (normal map)
///   DaMo_MatNa.png  R = reu,  G = moc,  B = vet mau
///
/// Ba mat na don vao MOT anh chu khong ba anh rieng: shader phai lay mau ba lan
/// cho ba huong chieu triplanar, nen moi anh them la them CHIN lan lay mau cho
/// mot diem anh. Don lai con ba.
///
/// ===================================================================
///  MOI LOP DEU PHAI CO LY DO HINH HOC
/// ===================================================================
///
/// <code>
///   HAT DA     nhieu tan so cao, deu - chat da
///   O BAC MAU  nhieu tan so thap - mang da bac mau theo nam thang
///   RANH NUT   nhieu "song nui" - vach sac chay ngoan ngoeo
///   VET VO     nhieu tan so thap lay nguong CAO - it thoi, vai cho
///   REU        mang lon, bam day hon o cho lom (shader nhan them huong len troi)
///   MOC        dom nho rai deu
///   MAU        keo DAN theo chieu doc -> vet chay xuong, khong phai dom tron
/// </code>
///
/// Vet mau phai keo dan theo chieu doc ngay tu trong ANH: shader khong biet
/// dau la tren duoi cua tung mat, chi biet phap tuyen. Neu de mau la dom tron
/// thi nhin ra vet son do cham cham, khong ra mau chay.
/// </summary>
public static class Act2DaMo
{
    public const int DoMin = 1024;

    const string ThuMuc = "Assets/BlenderMaps/GraveyardAct2/CayDaChua";
    const string DuongMau = ThuMuc + "/DaMo_Mau.png";
    const string DuongGan = ThuMuc + "/DaMo_Gan.png";
    const string DuongMatNa = ThuMuc + "/DaMo_MatNa.png";

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
    //  CAC LOP
    // ================================================================

    static float HatDa(float u, float v) { return Fbm(u, v, 64, 64, 3301, 3, 0.5f); }
    static float OBacMau(float u, float v) { return Fbm(u, v, 6, 6, 5711, 4, 0.55f); }
    static float RanhNut(float u, float v) { return Song(u, v, 9, 9, 9137, 4); }
    /// <summary>
    /// Vet vo: hinh khoi lay tu nhieu tan so thap, nhung RIA phai lom chom.
    ///
    /// Chi mot lop fBm roi lay nguong thi ra nhung lo TRON DEU nhu bot khi -
    /// da me khong bao gio tron nhu vay. Cong them mot lop nhieu tan so cao
    /// bien do nho vao TRUOC khi lay nguong thi duong bien gay khuc ra, va
    /// nguong hep (0,66 .. 0,70) giu cho mep van sac chu khong nhoe.
    /// </summary>
    static float VetVo(float u, float v)
    {
        return Fbm(u, v, 6, 6, 1489, 4, 0.5f) + (Fbm(u, v, 30, 30, 6229, 2, 0.5f) - 0.5f) * 0.20f;
    }

    /// <summary>Vet nut LON chay qua ca vien da - khac voi ranh nut li ti.</summary>
    static float NutLon(float u, float v) { return Song(u, v, 3, 3, 7727, 3); }
    static float Reu(float u, float v) { return Fbm(u, v, 4, 4, 2711, 4, 0.55f); }
    static float Moc(float u, float v) { return Fbm(u, v, 17, 17, 6607, 3, 0.55f); }

    /// <summary>
    /// Vet mau: nhieu KEO DAN theo chieu doc, nen thanh vet chay chu khong phai
    /// dom tron. perX 13 / perY 3 la ti le keo hon bon lan.
    /// </summary>
    static float VetMau(float u, float v) { return Fbm(u, v, 13, 3, 8017, 4, 0.52f); }

    /// <summary>Vai giot mau tron, roi rac - cho no khong chi toan vet chay.</summary>
    static float GiotMau(float u, float v) { return Fbm(u, v, 23, 23, 4363, 2, 0.5f); }

    // ================================================================
    //  DIEM VAO
    // ================================================================

    [MenuItem("Diablo 2.5D/16. Dung lai anh da mo Act2", false, 97)]
    public static void Dung()
    {
        var cao = new float[DoMin * DoMin];
        var pMau = new Color32[DoMin * DoMin];
        var pMatNa = new Color32[DoMin * DoMin];

        for (int y = 0; y < DoMin; y++)
        {
            float v = y / (float)DoMin;
            for (int x = 0; x < DoMin; x++)
            {
                float u = x / (float)DoMin;
                int i = y * DoMin + x;

                float hat = HatDa(u, v);
                float bac = OBacMau(u, v);
                float nut = RanhNut(u, v);
                float vo = VetVo(u, v);
                float reu = Reu(u, v);
                float moc = Moc(u, v);
                float mau = VetMau(u, v);
                float giot = GiotMau(u, v);
                float nutLon = NutLon(u, v);

                // ---------------- VET VO ----------------
                // Nguong CAO va HEP: vet vo phai it va phai co mep sac. Da mo
                // me het ca thi khong ra nghia dia nua, ma ra mot dong gach vun.
                float mucVo = Mathf.InverseLerp(0.660f, 0.700f, vo);

                // Vet nut lon: mot duong sac chay qua, sau hon ranh li ti
                float mucNut = Mathf.InverseLerp(0.88f, 0.98f, nutLon);

                // ---------------- DO CAO ----------------
                float h = 0.55f + (hat - 0.5f) * 0.30f;      // hat da li ti
                h += (bac - 0.5f) * 0.10f;                   // song lon rat nhe
                h -= Mathf.Pow(nut, 3.0f) * 0.45f;           // ranh nut li ti
                h -= mucNut * 0.50f;                         // vet nut lon chay qua
                h -= mucVo * 0.70f;                          // cho vo thut han vao
                cao[i] = Mathf.Clamp01(h);

                // ---------------- MAU DA ----------------
                // Da xam, sang toi theo hat va theo mang bac mau
                float d = 0.78f + (hat - 0.5f) * 0.34f;
                d *= 0.80f + bac * 0.34f;
                d *= 1f - Mathf.Pow(nut, 3.0f) * 0.55f;
                d *= 1f - mucNut * 0.60f;

                // Da hoi am sac vang o cho bac mau nhieu
                var c = new Vector3(d, d * (0.985f - bac * 0.03f), d * (0.95f - bac * 0.07f));

                // Long vet vo: da tuoi, sang va sach hon, hoi ngA vang
                var daTuoi = new Vector3(0.86f, 0.83f, 0.76f) * (0.80f + hat * 0.35f);
                c = Vector3.Lerp(c, daTuoi, mucVo * 0.75f);

                pMau[i] = new Color32(
                    (byte)(Mathf.Clamp01(c.x) * 255f),
                    (byte)(Mathf.Clamp01(c.y) * 255f),
                    (byte)(Mathf.Clamp01(c.z) * 255f),
                    (byte)(Mathf.Clamp01(mucVo) * 255f));

                // ---------------- BA MAT NA ----------------
                // REU: mang lon, mep mem. Nguong ha xuong so voi ban dau
                // (0,44 -> 0,38) vi shader con nhan them he so huong len troi,
                // nen luong reu THAY DUOC tren mat dung chi con mot phan nho.
                float mReu = Mathf.InverseLerp(0.38f, 0.56f, reu);
                mReu *= 0.75f + 0.25f * hat;                 // loang lo trong long mang
                mReu = Mathf.Clamp01(mReu + mucNut * 0.35f); // reu an vao khe nut

                // MOC: dom nho, thua
                float mMoc = Mathf.InverseLerp(0.52f, 0.70f, moc);

                // MAU: vet chay doc + vai giot tron. Nguong ha han xuong -
                // nguoi choi muon "nhieu vet mau do loang lo".
                float mMau = Mathf.InverseLerp(0.50f, 0.64f, mau);
                float mGiot = Mathf.InverseLerp(0.62f, 0.74f, giot);
                mMau = Mathf.Clamp01(Mathf.Max(mMau, mGiot * 0.90f));
                // vet mau khong dinh vao cho vua vo - cho do la da moi lo ra
                mMau *= 1f - mucVo * 0.75f;

                pMatNa[i] = new Color32(
                    (byte)(Mathf.Clamp01(mReu) * 255f),
                    (byte)(Mathf.Clamp01(mMoc) * 255f),
                    (byte)(Mathf.Clamp01(mMau) * 255f),
                    255);
            }
        }

        GhiPng(DuongMau, pMau, TextureFormat.RGBA32);
        GhiPng(DuongMatNa, pMatNa, TextureFormat.RGBA32);
        GhiPng(DuongGan, TuDoCaoRaGan(cao, 4.5f), TextureFormat.RGB24);

        AssetDatabase.Refresh();
        DatKieuNhap();
        GanVaoVatLieu();

        Debug.Log(string.Format("[Act2DaMo] da dung ba anh {0}x{0} trong {1}", DoMin, ThuMuc));
    }

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
    /// Anh gan phai nhap kieu NormalMap, con anh MAT NA phai TAT sRGB.
    ///
    /// Mat na khong phai mau ma la ba con so 0..1. De sRGB thi Unity ap duong
    /// cong gamma len chung, va muc reu 0,5 doc ra thanh 0,73 - reu day gap ruoi
    /// so voi y do.
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

        var tiMatNa = AssetImporter.GetAtPath(DuongMatNa) as TextureImporter;
        if (tiMatNa != null)
        {
            tiMatNa.textureType = TextureImporterType.Default;
            tiMatNa.sRGBTexture = false;
            tiMatNa.alphaSource = TextureImporterAlphaSource.None;
            tiMatNa.wrapMode = TextureWrapMode.Repeat;
            tiMatNa.maxTextureSize = 1024;
            tiMatNa.SaveAndReimport();
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

    /// <summary>
    /// Doi ba vat lieu da sang shader moi.
    ///
    /// Doi CHINH vat lieu dang dung chu khong tao vat lieu moi: 458 bia mo, 229
    /// tang da va 5 nha mo deu tro toi ba vat lieu nay, nen doi mot cho la ca
    /// nghia dia doi theo - khong phai di gan lai tung vat.
    ///
    /// Ba loai khac nhau o SAC va o LUONG REU, khong o anh: tang da nam lan
    /// trong co nen reu nhieu nhat; nha mo xay cao, kho hon.
    /// </summary>
    static void GanVaoVatLieu()
    {
        var sh = Shader.Find("Diablo25D/DaMoTriplanar");
        if (sh == null) { Debug.LogError("[Act2DaMo] khong tim thay shader DaMoTriplanar"); return; }

        var mau = AssetDatabase.LoadAssetAtPath<Texture2D>(DuongMau);
        var gan = AssetDatabase.LoadAssetAtPath<Texture2D>(DuongGan);
        var matNa = AssetDatabase.LoadAssetAtPath<Texture2D>(DuongMatNa);

        Dat("Assets/Materials/Act2_DaBia.mat", sh, mau, gan, matNa,
            new Color(0.70f, 0.71f, 0.68f), 0.95f, 0.60f, 1.00f, 1.70f);
        Dat("Assets/Materials/Act2_DaTang.mat", sh, mau, gan, matNa,
            new Color(0.62f, 0.615f, 0.57f), 1.00f, 0.65f, 0.55f, 1.35f);
        Dat("Assets/Materials/Act2_LongMo.mat", sh, mau, gan, matNa,
            new Color(0.67f, 0.675f, 0.65f), 0.75f, 0.55f, 1.00f, 1.10f);
    }

    static void Dat(string duong, Shader sh, Texture2D mau, Texture2D gan, Texture2D matNa,
                    Color sac, float reu, float moc, float vetMau, float tiLe)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(duong);
        if (m == null)
        {
            // Vat lieu co the nam trong thu muc khac - tim theo ten
            string ten = Path.GetFileNameWithoutExtension(duong);
            var guids = AssetDatabase.FindAssets(ten + " t:Material");
            for (int i = 0; i < guids.Length && m == null; i++)
            {
                string d = AssetDatabase.GUIDToAssetPath(guids[i]);
                var thu = AssetDatabase.LoadAssetAtPath<Material>(d);
                if (thu != null && thu.name == ten) m = thu;
            }
        }
        if (m == null) { Debug.LogError("[Act2DaMo] khong thay vat lieu " + duong); return; }

        m.shader = sh;
        m.SetTexture("_MainTex", mau);
        m.SetTexture("_BumpMap", gan);
        m.SetTexture("_MatNa", matNa);
        m.SetColor("_Color", sac);
        m.SetFloat("_ReuMuc", reu);
        m.SetFloat("_MocMuc", moc);
        m.SetFloat("_MauMuc", vetMau);
        m.SetFloat("_TiLe", tiLe);
        // Mang reu / mau lap moi ~2,4 m: to hon ca mot tam bia, nen moi vien da
        // rieng se roi vao mot cho khac nhau tren mang - khong cai nao giong cai nao.
        m.SetFloat("_TiLeMatNa", 0.42f);
        m.SetFloat("_DoSanSui", 2.4f);
        m.SetFloat("_VoMuc", 1.0f);
        m.SetFloat("_Glossiness", 0.10f);
        m.SetColor("_ReuColor", new Color(0.125f, 0.235f, 0.075f));
        m.SetColor("_MocColor", new Color(0.62f, 0.64f, 0.57f));
        // Mau do dam va bao hoa hon han: canh Act2 la ban dem duoi anh trang
        // xanh, mot mau do nhat se bi hut sach thanh xam hong.
        m.SetColor("_MauColor", new Color(0.44f, 0.045f, 0.030f));
        m.SetFloat("_ReuHuongLen", 0.60f);

        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        Debug.Log(string.Format("[Act2DaMo] {0} -> {1}", m.name, sh.name));
    }
}
