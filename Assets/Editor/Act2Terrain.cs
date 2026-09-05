using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// DOI TAM MAT DAT VE TAY TRONG BLENDER THANH UNITY TERRAIN THAT.
///
/// VI SAO PHAI DOI: tam dat tu Blender chi la mot luoi tam, dap duoc dung MOT
/// vat lieu. Ca ban do rong 109 m ma chi mot mau thi nhin ra tam bia phang chu
/// khong ra mat dat. Unity Terrain thi tron duoc NHIEU LOP vat lieu tren cung
/// mot cho - co chet, dat troc, soi da, bun - va con cho ban to them bang tay
/// ngay trong Unity.
///
/// HINH DANG DUOC GIU NGUYEN. Do cao duoc doc lai tu chinh cai luoi Blender
/// bang cach RAI TUNG TAM GIAC len luoi o vuong, nen tung go dat tung cho trung
/// deu y het ban dung. Neu khong giu dung, 745 ngoi mo va goc cay dat san se
/// lo lung giua troi hoac thut xuong dat.
/// </summary>
public static class Act2Terrain
{
    /// <summary>Do min cua luoi do cao. Phai la 2^n + 1.</summary>
    public const int DoMinCao = 513;

    /// <summary>Do min cua luoi to mau.</summary>
    public const int DoMinTo = 1024;

    public const int LopCoChet = 0;
    public const int LopDatTroc = 1;
    public const int LopSoiDa = 2;
    public const int LopBun = 3;

    const string ThuMucTerrain = "Assets/Terrain";

    /// <summary>
    /// Dung Terrain tu luoi mat dat cua Blender.
    ///
    /// <paramref name="luoiDat"/> la luoi goc, <paramref name="tranDat"/> la
    /// transform cua vat the mat dat trong canh (de doi ra toa do the gioi).
    /// </summary>
    public static GameObject Dung(Mesh luoiDat, Matrix4x4 doiRaTheGioi,
                                  Transform cha, Vector3[] cacNgoiMo)
    {
        float thap, cao;
        float[,] doCao = DocDoCao(luoiDat, doiRaTheGioi, out thap, out cao);

        float khoangCao = Mathf.Max(0.5f, cao - thap);

        var td = new TerrainData();
        td.name = "Act2_MatDat";
        td.heightmapResolution = DoMinCao;
        td.size = new Vector3(109.2f, khoangCao, 109.2f);
        td.SetHeights(0, 0, doCao);

        td.alphamapResolution = DoMinTo;
        td.terrainLayers = GhiLopRaFile();
        ToMau(td, cacNgoiMo, thap, khoangCao);

        // Ghi ra file de con to tay duoc trong Unity
        if (!AssetDatabase.IsValidFolder(ThuMucTerrain))
            AssetDatabase.CreateFolder("Assets", "Terrain");
        string duong = ThuMucTerrain + "/Act2_MatDat.asset";
        AssetDatabase.DeleteAsset(duong);
        AssetDatabase.CreateAsset(td, duong);

        var go = Terrain.CreateTerrainGameObject(td);
        go.name = "MatDat_Terrain";
        go.transform.SetParent(cha, false);

        // Terrain dung goc THAP TRAI lam moc, con ban do Blender lay TAM lam moc
        go.transform.position = new Vector3(-54.6f, thap, -54.6f);

        var t = go.GetComponent<Terrain>();
        t.heightmapPixelError = 3f;
        t.basemapDistance = 200f;
        // Dia hinh thoai, bong do cua chinh no khong thay gi ma lai ton
        t.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // PHAI dat vat lieu ro rang. De null thi Unity khong tu tim ve vat lieu
        // mac dinh ma ve ra mau hong canh sen - dau hieu thieu shader. Act1 dung
        // dung cai nay nen Act2 lay cho khop.
        var shTerrain = Shader.Find("Nature/Terrain/Standard");
        if (shTerrain != null)
        {
            var vl = new Material(shTerrain);
            vl.name = "Act2_MatDat_VatLieu";
            string duongVl = ThuMucTerrain + "/Act2_MatDat_VatLieu.mat";
            AssetDatabase.DeleteAsset(duongVl);
            AssetDatabase.CreateAsset(vl, duongVl);
            t.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(duongVl);
        }

        int lop = LayerMask.NameToLayer("Ground");
        if (lop >= 0) go.layer = lop;

        return go;
    }

    // ================================================================
    //  DOC DO CAO TU LUOI
    // ================================================================

    /// <summary>
    /// Rai tung tam giac cua luoi len luoi o vuong de lay do cao.
    ///
    /// KHONG dung cach ban tia (raycast): trong Editor, va cham cua mot luoi vua
    /// tao ra chua kip vao he vat ly, ban tia se truot het va tra ve 0 - toi da
    /// dinh dung cai bay do mot lan roi. Rai tam giac thi khong phu thuoc vao he
    /// vat ly, va cho ket qua giong het moi lan chay.
    /// </summary>
    static float[,] DocDoCao(Mesh luoi, Matrix4x4 doiRaTheGioi, out float thap, out float cao)
    {
        var dinh = luoi.vertices;
        var tam = luoi.triangles;

        var p = new Vector3[dinh.Length];
        thap = float.MaxValue; cao = float.MinValue;
        for (int i = 0; i < dinh.Length; i++)
        {
            p[i] = doiRaTheGioi.MultiplyPoint3x4(dinh[i]);
            if (p[i].y < thap) thap = p[i].y;
            if (p[i].y > cao) cao = p[i].y;
        }

        int n = DoMinCao;
        var kq = new float[n, n];
        var daCo = new bool[n, n];

        const float NuaCanh = 54.6f;
        float buoc = 109.2f / (n - 1);

        for (int t = 0; t < tam.Length; t += 3)
        {
            Vector3 a = p[tam[t]], b = p[tam[t + 1]], c = p[tam[t + 2]];

            float xMin = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
            float xMax = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            float zMin = Mathf.Min(a.z, Mathf.Min(b.z, c.z));
            float zMax = Mathf.Max(a.z, Mathf.Max(b.z, c.z));

            int ix0 = Mathf.Max(0, Mathf.FloorToInt((xMin + NuaCanh) / buoc));
            int ix1 = Mathf.Min(n - 1, Mathf.CeilToInt((xMax + NuaCanh) / buoc));
            int iz0 = Mathf.Max(0, Mathf.FloorToInt((zMin + NuaCanh) / buoc));
            int iz1 = Mathf.Min(n - 1, Mathf.CeilToInt((zMax + NuaCanh) / buoc));

            // Mau so cua toa do trong tam - tinh mot lan cho ca tam giac
            float d = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
            if (Mathf.Abs(d) < 1e-9f) continue;

            for (int iz = iz0; iz <= iz1; iz++)
            {
                float pz = iz * buoc - NuaCanh;
                for (int ix = ix0; ix <= ix1; ix++)
                {
                    float px = ix * buoc - NuaCanh;

                    float w1 = ((b.z - c.z) * (px - c.x) + (c.x - b.x) * (pz - c.z)) / d;
                    float w2 = ((c.z - a.z) * (px - c.x) + (a.x - c.x) * (pz - c.z)) / d;
                    float w3 = 1f - w1 - w2;

                    const float le = -0.0005f;
                    if (w1 < le || w2 < le || w3 < le) continue;

                    float y = w1 * a.y + w2 * b.y + w3 * c.y;

                    // Cho nao hai tam giac chong len nhau thi lay cai CAO HON,
                    // de mep vach hay bo da khong bi khoet thung
                    if (!daCo[iz, ix] || y > kq[iz, ix])
                    {
                        kq[iz, ix] = y;
                        daCo[iz, ix] = true;
                    }
                }
            }
        }

        // O nao khong tam giac nao phu (mep luoi) thi muon do cao cua o ben canh
        VaLoTrong(kq, daCo, n);

        // Doi ve thang 0..1 nhu Terrain doi hoi
        float khoang = Mathf.Max(0.5f, cao - thap);
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
                kq[z, x] = Mathf.Clamp01((kq[z, x] - thap) / khoang);

        return kq;
    }

    /// <summary>Lap cac o chua co do cao bang cach loang dan tu o ben canh.</summary>
    static void VaLoTrong(float[,] cao, bool[,] daCo, int n)
    {
        for (int vong = 0; vong < 4; vong++)
        {
            bool conThieu = false;
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    if (daCo[z, x]) continue;

                    float tong = 0f; int dem = 0;
                    for (int dz = -1; dz <= 1; dz++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int zz = z + dz, xx = x + dx;
                            if (zz < 0 || zz >= n || xx < 0 || xx >= n) continue;
                            if (!daCo[zz, xx]) continue;
                            tong += cao[zz, xx]; dem++;
                        }

                    if (dem > 0) { cao[z, x] = tong / dem; daCo[z, x] = true; }
                    else conThieu = true;
                }
            if (!conThieu) break;
        }
    }

    // ================================================================
    //  LOP VAT LIEU
    // ================================================================

    /// <summary>
    /// Bon lop vat lieu cho nghia dia.
    ///
    /// Moi lop mot NHIP LAP KHAC NHAU (2.4 / 3.7 / 1.9 / 5.1). Cho hai lop cung
    /// mot nhip thi cho nao tron hai lop se lo ra o vuong lap lai deu tam tap.
    /// </summary>
    static TerrainLayer[] LamLop()
    {
        var co = new TerrainLayer();
        co.name = "Act2_CoChet";
        // KHONG dung anh GrassField: no von la co XANH, ma Tinted chi NHAN mau
        // chu khong khu duoc mau san co - nhuom nau vao chi ra mot thu xanh reu
        // xin xit. Lay anh dat roi nhuom mau rom kho thi moi ra co chet that.
        co.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.DarkSoil(), new Color(0.60f, 0.49f, 0.31f), "Act2_CoChet");
        co.tileSize = new Vector2(2.4f, 2.4f);
        co.specular = Color.black;
        co.smoothness = 0.02f;

        var troc = new TerrainLayer();
        troc.name = "Act2_DatTroc";
        // Dung anh KHAC voi lop co, khong thi hai lop chong nhau se ra mot mang
        // deu tam tap vi cung mot van
        troc.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.MudGround(), new Color(0.44f, 0.32f, 0.22f), "Act2_DatTroc");
        troc.tileSize = new Vector2(3.7f, 3.7f);
        troc.specular = Color.black;
        troc.smoothness = 0.02f;

        var soi = new TerrainLayer();
        soi.name = "Act2_SoiDa";
        soi.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.RockGround(new Color(0.30f, 0.28f, 0.26f),
                                      new Color(0.72f, 0.70f, 0.66f), 0.55f),
            new Color(0.60f, 0.56f, 0.50f), "Act2_SoiDa");
        soi.tileSize = new Vector2(1.9f, 1.9f);
        soi.specular = Color.black;
        soi.smoothness = 0.06f;

        var bun = new TerrainLayer();
        bun.name = "Act2_Bun";
        bun.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.MudGround(), new Color(0.22f, 0.18f, 0.14f), "Act2_Bun");
        bun.tileSize = new Vector2(5.1f, 5.1f);
        bun.specular = Color.black;
        bun.smoothness = 0.16f;

        return new[] { co, troc, soi, bun };
    }

    /// <summary>
    /// Ghi bon lop vat lieu ra FILE roi moi dung.
    ///
    /// PHAI ghi ra file, khong duoc dung thang doi tuong vua tao trong bo nho.
    /// TerrainLayer va anh cua no la tai san rieng; luu TerrainData thanh file
    /// ma cac lop chi nam trong bo nho thi moi tham chieu deu dut, mo lai canh
    /// se thay bon lop rong khong - toi da vap dung loi nay.
    /// </summary>
    static TerrainLayer[] GhiLopRaFile()
    {
        if (!AssetDatabase.IsValidFolder(ThuMucTerrain))
            AssetDatabase.CreateFolder("Assets", "Terrain");

        var lop = LamLop();

        for (int i = 0; i < lop.Length; i++)
        {
            string ten = lop[i].name;
            string duong = ThuMucTerrain + "/" + ten + ".terrainlayer";
            var anh = GhiAnhRaFile(lop[i].diffuseTexture, ten);

            // Da co file thi GIU NGUYEN FILE (de tham chieu cu khong dut) nhung
            // CAP NHAT NOI DUNG theo code - khong thi sua mau se khong an gi
            var cu = AssetDatabase.LoadAssetAtPath<TerrainLayer>(duong);
            if (cu != null)
            {
                cu.diffuseTexture = anh;
                cu.tileSize = lop[i].tileSize;
                cu.specular = lop[i].specular;
                cu.smoothness = lop[i].smoothness;
                EditorUtility.SetDirty(cu);

                Object.DestroyImmediate(lop[i]);
                lop[i] = cu;
                continue;
            }

            lop[i].diffuseTexture = anh;
            AssetDatabase.CreateAsset(lop[i], duong);
            lop[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>(duong);
        }

        AssetDatabase.SaveAssets();
        return lop;
    }

    /// <summary>
    /// Do bong cua mat dat, nhet vao kenh alpha cua anh vat lieu.
    /// Dat rat thap: dat nghia trang am uot van la dat, khong phai mat kinh.
    /// </summary>
    const float DoBongMatDat = 0.04f;

    /// <summary>Ghi mot anh vat lieu ra file PNG de no thanh tai san that.</summary>
    static Texture2D GhiAnhRaFile(Texture2D nguon, string ten)
    {
        if (nguon == null) return null;

        string duong = ThuMucTerrain + "/Anh_" + ten + ".png";

        // HA KENH ALPHA XUONG THAP VA DEU TRUOC KHI GHI.
        //
        // Voi terrain cua Unity, kenh alpha cua anh vat lieu KHONG phai do trong
        // suot - no la DO BONG. Cac ham ve dat sinh ra alpha loang lo 0,5-1,0,
        // tuc mat dat bong gan nhu guong: den cua nguoi choi quet toi dau la noi
        // len mot mang van trang loe loet nhu bot nuoc, nhin khong ra dat.
        //
        // Da do: truoc khi sua, diem sang nhat cua mat dat la 1,00 (chay trang);
        // sau khi ha alpha ve 0,04 thi con 0,37 va khong con diem trang nao.
        //
        // Chinh o LOP (TerrainLayer.smoothness) khong an thua gi - khi khong co
        // anh mask thi shader lay thang tu alpha nay.
        var oAlpha = nguon.GetPixels();
        for (int i = 0; i < oAlpha.Length; i++) oAlpha[i].a = DoBongMatDat;
        nguon.SetPixels(oAlpha);
        nguon.Apply();

        // GHI DE moi lan. Dung lai file cu thi luc sua mau trong code se khong
        // an gi ca, rat de tuong nham la code sai.
        byte[] png = nguon.EncodeToPNG();
        if (png == null) return null;
        File.WriteAllBytes(duong, png);
        AssetDatabase.ImportAsset(duong, ImportAssetOptions.ForceSynchronousImport);

        var imp = AssetImporter.GetAtPath(duong) as TextureImporter;
        if (imp != null)
        {
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.filterMode = FilterMode.Bilinear;
            imp.anisoLevel = 8;
            imp.mipmapEnabled = true;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(duong);
    }

    // ================================================================
    //  TO MAU
    // ================================================================

    /// <summary>
    /// To bon lop len mat dat.
    ///
    /// Cach tron:
    ///   - Nen la co chet lan dat troc, chia nhau theo mot lop nhieu lon
    ///   - Cho nao DOC thi tro soi da ra, vi dat khong bam duoc tren suon doc
    ///   - Cho nao TRUNG thi dong bun, dung nhu nuoc mua se dong lai
    ///   - Quanh moi ngoi mo la mot vung DAT TROC, dau chan nguoi vien mo giam nat
    /// </summary>
    static void ToMau(TerrainData td, Vector3[] cacNgoiMo, float thap, float khoangCao)
    {
        int n = td.alphamapResolution;
        var alpha = new float[n, n, 4];

        int nCao = td.heightmapResolution;
        var cao = td.GetHeights(0, 0, nCao, nCao);

        const float NuaCanh = 54.6f;

        for (int z = 0; z < n; z++)
        {
            float pz = (float)z / (n - 1) * 109.2f - NuaCanh;

            for (int x = 0; x < n; x++)
            {
                float px = (float)x / (n - 1) * 109.2f - NuaCanh;

                // Do doc va do cao tai cho nay
                float u = (float)x / (n - 1);
                float v = (float)z / (n - 1);
                float doDoc = td.GetSteepness(u, v) / 90f;
                float hCao = LayCao(cao, nCao, u, v);

                // Lop nhieu lon: mang co chet xen mang dat troc
                float nhieu = Mathf.PerlinNoise(px * 0.055f + 11.3f, pz * 0.055f + 4.7f);
                float nhieuNho = Mathf.PerlinNoise(px * 0.31f, pz * 0.31f);
                float g = Mathf.Clamp01((nhieu - 0.42f) * 3.4f + (nhieuNho - 0.5f) * 0.5f);

                float aCo = g;
                float aTroc = 1f - g;
                float aSoi = 0f;
                float aBun = 0f;

                // Suon doc: dat troi ra, lo soi da
                float doc = Mathf.Clamp01((doDoc - 0.10f) * 6f);
                aSoi = doc;

                // Cho trung: bun dong lai
                float trung = Mathf.Clamp01((0.30f - hCao) * 3.2f)
                            * Mathf.Clamp01(1f - doDoc * 5f);
                aBun = trung * 0.85f;

                float con = Mathf.Max(0f, 1f - aSoi - aBun);
                aCo *= con;
                aTroc *= con;

                alpha[z, x, LopCoChet] = aCo;
                alpha[z, x, LopDatTroc] = aTroc;
                alpha[z, x, LopSoiDa] = aSoi;
                alpha[z, x, LopBun] = aBun;
            }
        }

        DamNatQuanhMo(alpha, n, cacNgoiMo);
        ChuanHoa(alpha, n);

        td.SetAlphamaps(0, 0, alpha);
    }

    static float LayCao(float[,] cao, int n, float u, float v)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(u * (n - 1)), 0, n - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(v * (n - 1)), 0, n - 1);
        return cao[z, x];
    }

    /// <summary>
    /// Dam mot vung dat troc quanh moi ngoi mo.
    ///
    /// Khong duyet ca luoi cho tung ngoi mo - 452 mo nhan mot trieu o luoi la
    /// nam tram trieu phep tinh. Chi dong dau len dung vung nho quanh moi mo.
    /// </summary>
    static void DamNatQuanhMo(float[,,] alpha, int n, Vector3[] mo)
    {
        if (mo == null) return;

        const float NuaCanh = 54.6f;
        const float BanKinh = 1.35f;
        float oMoiMet = (n - 1) / 109.2f;
        int banKinhO = Mathf.CeilToInt(BanKinh * oMoiMet);

        for (int i = 0; i < mo.Length; i++)
        {
            int cx = Mathf.RoundToInt((mo[i].x + NuaCanh) * oMoiMet);
            int cz = Mathf.RoundToInt((mo[i].z + NuaCanh) * oMoiMet);

            for (int dz = -banKinhO; dz <= banKinhO; dz++)
            {
                int z = cz + dz;
                if (z < 0 || z >= n) continue;

                for (int dx = -banKinhO; dx <= banKinhO; dx++)
                {
                    int x = cx + dx;
                    if (x < 0 || x >= n) continue;

                    float d = Mathf.Sqrt(dx * dx + dz * dz) / banKinhO;
                    if (d > 1f) continue;

                    // Nhat dan ra ria, va hoi nham cho khoi tron vanh vach
                    float k = Mathf.SmoothStep(1f, 0f, d);
                    k *= Mathf.Lerp(0.7f, 1.1f, Mathf.PerlinNoise(x * 0.4f, z * 0.4f));
                    k = Mathf.Clamp01(k) * 0.8f;

                    float con = 1f - k;
                    alpha[z, x, LopCoChet] *= con;
                    alpha[z, x, LopSoiDa] *= con;
                    alpha[z, x, LopBun] *= con;
                    alpha[z, x, LopDatTroc] = alpha[z, x, LopDatTroc] * con + k;
                }
            }
        }
    }

    static void ChuanHoa(float[,,] alpha, int n)
    {
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
            {
                float tong = 0f;
                for (int l = 0; l < 4; l++) tong += alpha[z, x, l];

                if (tong < 1e-4f) { alpha[z, x, LopDatTroc] = 1f; continue; }
                for (int l = 0; l < 4; l++) alpha[z, x, l] /= tong;
            }
    }
}
