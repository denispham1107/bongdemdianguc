using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 141 - MAT DAT SAN SUI: O GA · O VOI · RANH NUT · DAT VUN NGON NGANG (17/10/2026, nguoi dung: "mat dat van con tron bong nhin gia tao
/// qua, dung lai cho san sui, nhieu o ga o voi, nhieu cho nut vo dat cat ngon ngang tum lum" + "khong duoc de xay ra loi nhu lan truoc, xuat
/// hien nhieu o vuong o chu nhat trong suot ghep tung mang dat"). Nguoi dung chon: ca ba lop (do cao + anh dat moi + manh dat vun 3D),
/// luoi do cao 513 -> 1025, dung bang script tren API Terrain, ap ca Act2 lan Man chinh.
///
/// ===================================================================
///  "O VUONG" LAN TRUOC LA Z-FIGHTING - VA CACH TRANH O DAY
/// ===================================================================
/// Lan ve lai dia hinh truoc (HUONG-DAN "O vuong den duoi mat dat"), mat nuoc va de bia / phien da nam SAT SONG SONG voi mat dat moi
/// vai mm -> dien thoai (bo dem do sau 16 bit) ve hai mat de nhau nhap nhay. O day:
///   1. DUOI CHAN MOI DO VAT + quanh mat nuoc + dai vanh rao / vuc: do cao GIU NGUYEN (mat na = 0, thoai dan 0,8 m) -> khong mat nao
///      cua do vat doi khoang cach toi dat.
///   2. O ga / o voi / ranh nut deu LOM xuong; mep chi go nhe <= 3 cm.
///   3. Manh dat vun 3D: phien dat dat NGHIENG 8-25 do, dong dat co vanh CHIM -4 cm (cat mat dat theo goc), cuc dat / da vun chon 25-35%.
///   4. Phep thu menu 142 dem lai: do vat (Act2ChongDongPhang.Dem), mat nuoc, tam giac dat vun nam song song sat dat, tam suong
///      (TamNhin) bi dat dam xuyen.
///
/// CHAY LAI DUOC: lan dau luu do cao + net to GOC ra Assets/Terrain/{ten}_DoCaoTruocSanSui.bytes / _ToMauTruocSanSui.bytes; lan sau
/// dung lai tu do (khong cong don). Tra lai nguyen trang: menu 141b.
/// </summary>
public static class MatDatSanSui
{
    // ---------------- thong so (doi roi chay lai) ----------------
    public const int DoPhanGiai = 1025;
    /// <summary>O ga moi 100 m2 dat duoc phep (khong tinh chan do vat / nuoc / vanh).</summary>
    // lan 1: 2,2 / 0,32 / 0,9 / gon +-2,2 cm / 0,26 manh - duoi anh trang gan nhu khong doc ra
    public const float MatDoOGa = 5f;
    public const float MatDoOVoi = 1.1f;
    public const float MatDoVetNut = 2.5f;
    /// <summary>Gon san lam tam (m) - hai tang song 1,2 m va 0,45 m.</summary>
    public const float BienDoSan = 0.045f;
    /// <summary>Manh dat vun moi m2 (truoc khi nhan he so cum).</summary>
    public const float MatDoDatVun = 0.5f;
    public const float DoNoiPhapTuyen = 1.6f;
    // le quanh KHUNG BAO vat (moi mat cua vat nam trong khung bao -> ngoai khung doi dat khong doi khe mat vat - dat). Lan 1 le 0,35 + thoai 0,8:
    // nghia dia day dac -> chi con 32% dien tich duoc lam (18 o voi / 127 o ga) - qua it.
    // Lan 2 le 0,12 < mot o luoi (0,131 m): diem luoi ngay ngoai mep khung bao da vao vung thoai -> mep o ga ke ben keo goc khung bao lech 2,6 cm
    // (menu 142 B). Nay le 0,3 m (> 2 o).
    const float Le = 0.3f, Thoai = 0.45f, LeNuoc = 1.2f;
    const float CoO = 16f;

    public const string ThuMucDatVun = "Assets/Terrain/DatVun";
    public const string TenGocDatVun = "DatVun";

    static readonly StringBuilder bao = new StringBuilder();
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[MatDatSanSui] " + s); }

    [MenuItem("Diablo 2.5D/141. Dung lai MAT DAT san sui (o ga · o voi · ranh nut · dat vun) - Act2 + Man chinh", false, 231)]
    public static void ChayCaHai()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[MatDatSanSui] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[MatDatSanSui] scene dang mo co thay doi chua luu"); return; }
        string cu = EditorSceneManager.GetActiveScene().path;
        bao.Length = 0;
        ChuanBiAnh();
        Chay("Assets/Scenes/Act2.unity", "Act2");
        Chay("Assets/Scenes/MainMenu.unity", "ManChinh");
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText("PlayTestShots/matdatsansui.txt", bao.ToString());
        if (!string.IsNullOrEmpty(cu)) EditorSceneManager.OpenScene(cu, OpenSceneMode.Single);
    }

    [MenuItem("Diablo 2.5D/141b. Tra MAT DAT ve truoc khi san sui (Act2 + Man chinh)", false, 232)]
    public static void TraLaiCaHai()
    {
        if (EditorApplication.isPlaying) return;
        string cu = EditorSceneManager.GetActiveScene().path;
        foreach (var kv in new[] { new[] { "Assets/Scenes/Act2.unity", "Act2" }, new[] { "Assets/Scenes/MainMenu.unity", "ManChinh" } })
        {
            var sc = EditorSceneManager.OpenScene(kv[0], OpenSceneMode.Single);
            var ter = Object.FindFirstObjectByType<Terrain>();
            int r; float[,] h = DocDoCao(kv[1], out r); var a = DocToMau(kv[1]);
            if (ter == null || h == null) continue;
            var td = ter.terrainData; Vector3 kich = td.size;
            float y0, sy; DocKhung(kv[1], out y0, out sy);
            td.heightmapResolution = r; td.size = new Vector3(kich.x, sy, kich.z);
            var p = ter.transform.position; p.y = y0; ter.transform.position = p;
            td.SetHeights(0, 0, h);
            if (a != null) td.SetAlphamaps(0, 0, a);
            var goc = GameObject.Find(TenGocDatVun); if (goc != null) Object.DestroyImmediate(goc);
            EditorUtility.SetDirty(td); EditorSceneManager.MarkSceneDirty(sc); EditorSceneManager.SaveScene(sc);
        }
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(cu)) EditorSceneManager.OpenScene(cu, OpenSceneMode.Single);
        Debug.Log("[MatDatSanSui] da tra mat dat ve truoc khi san sui (lop anh dat giu anh moi - doi lai bang tay neu can)");
    }

    // ================================================================
    //  ANH DAT (Blender MCP CongCu/Blender/anh_dat_moi.py) -> 4 lop terrain
    // ================================================================
    static readonly string[] TenLop = { "CoChet", "DatTroc", "SoiDa", "Bun" };
    static readonly float[] CoLatLop = { 3.0f, 3.2f, 1.6f, 2.2f };

    static void ChuanBiAnh()
    {
        for (int i = 0; i < TenLop.Length; i++)
        {
            string mau = "Assets/Terrain/AnhDatMoi/" + TenLop[i] + "_Mau.png", phap = "Assets/Terrain/AnhDatMoi/" + TenLop[i] + "_Phap.png";
            var im = AssetImporter.GetAtPath(mau) as TextureImporter;
            if (im != null && (im.maxTextureSize != 1024 || !im.sRGBTexture || im.alphaIsTransparency || !im.mipmapEnabled))
            { im.maxTextureSize = 1024; im.sRGBTexture = true; im.alphaIsTransparency = false; im.mipmapEnabled = true; im.wrapMode = TextureWrapMode.Repeat; im.anisoLevel = 4; im.SaveAndReimport(); }
            var ip = AssetImporter.GetAtPath(phap) as TextureImporter;
            if (ip != null && (ip.textureType != TextureImporterType.NormalMap || ip.maxTextureSize != 1024))
            { ip.textureType = TextureImporterType.NormalMap; ip.maxTextureSize = 1024; ip.wrapMode = TextureWrapMode.Repeat; ip.anisoLevel = 4; ip.SaveAndReimport(); }
            var lop = AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Terrain/Act2_" + TenLop[i] + ".terrainlayer");
            if (lop == null) { Ghi("[LOI] thieu lop " + TenLop[i]); continue; }
            lop.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(mau);
            lop.normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(phap);
            lop.normalScale = DoNoiPhapTuyen;
            lop.tileSize = new Vector2(CoLatLop[i], CoLatLop[i]);
            EditorUtility.SetDirty(lop);
        }
        AssetDatabase.SaveAssets();
    }

    // ================================================================
    //  MOT SCENE
    // ================================================================
    struct ONhat { public Vector2 tam; public float r, sau; public bool voi; }

    public static void Chay(string duongScene, string ten)
    {
        var sc = EditorSceneManager.OpenScene(duongScene, OpenSceneMode.Single);
        var ter = Object.FindFirstObjectByType<Terrain>();
        if (ter == null) { Ghi("[LOI] " + ten + ": khong co Terrain"); return; }
        var td = ter.terrainData;
        Ghi("");
        Ghi(string.Format("===== {0}: {1} ({2}) =====", ten, AssetDatabase.GetAssetPath(td), td.size));
        // lop dung bon lop ten Act2_* (Man chinh dung chung) - kiem thu tu
        var lops = td.terrainLayers;
        int[] chiSo = new int[4];
        for (int i = 0; i < 4; i++) { chiSo[i] = -1; for (int k = 0; k < lops.Length; k++) if (lops[k] != null && lops[k].name == "Act2_" + TenLop[i]) chiSo[i] = k; }
        Ghi("lop terrain: " + string.Join(", ", System.Array.ConvertAll(lops, l => l != null ? l.name : "null")));

        // ---- 1. goc (lan dau luu) ----
        int r0; float[,] goc = DocDoCao(ten, out r0);
        float y0, sy0;
        if (goc == null)
        {
            r0 = td.heightmapResolution; goc = td.GetHeights(0, 0, r0, r0);
            y0 = ter.transform.position.y; sy0 = td.size.y;
            LuuDoCao(ten, goc, r0, y0, sy0);
            LuuToMau(ten, td.GetAlphamaps(0, 0, td.alphamapResolution, td.alphamapResolution));
            Ghi("lan dau: da luu do cao goc " + r0 + "x" + r0 + " + net to goc");
        }
        else DocKhung(ten, out y0, out sy0);
        var toMauGoc = DocToMau(ten);
        Vector3 kich = td.size; kich.y = sy0;
        Vector3 pos = ter.transform.position;
        int N = DoPhanGiai;
        float buoc = kich.x / (N - 1);

        // ---- 2. nang 513 -> 1025 bang CHINH ham noi suy cua Unity tren dat cu (TerrainData tam) ----
        // (lan 2 dung song tuyen: Unity noi suy theo TAM GIAC -> giua moi o cu lech toi ~8 mm cho dat bi van (menu 142 B: tam phien da -8 mm))
        var H = new float[N, N];
        var tdCu = new TerrainData { heightmapResolution = r0 };
        tdCu.size = new Vector3(kich.x, sy0, kich.z);
        tdCu.SetHeights(0, 0, goc);
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
                H[z, x] = y0 + tdCu.GetInterpolatedHeight(x / (float)(N - 1), z / (float)(N - 1));
        Object.DestroyImmediate(tdCu);

        // ---- 3. mat na: 0 duoi do vat / nuoc / vanh, 1 noi duoc lam ----
        float[,] mn = TaoMatNa(ter, pos, kich, N, buoc, H, out float[,] nuocY, out int soVat);
        int soDuoc = 0; foreach (var v in mn) if (v > 0.999f) soDuoc++;
        float dtDuoc = soDuoc * buoc * buoc;
        Ghi(string.Format("mat na: {0} vat giu nguyen chan; dien tich duoc lam {1:F0} m2 / {2:F0} m2", soVat, dtDuoc, kich.x * kich.z));

        // ---- 4. o ga, o voi, ranh nut, gon san ----
        var rnd = new System.Random(ten.GetHashCode() == 0 ? 1 : (ten == "Act2" ? 17102026 : 17102027));
        var ho = new float[N, N]; var mep = new float[N, N]; var nut = new float[N, N];
        var oNhat = new List<ONhat>();
        int soGa = Mathf.RoundToInt(dtDuoc / 100f * MatDoOGa), soVoi = Mathf.RoundToInt(dtDuoc / 100f * MatDoOVoi), soNut = Mathf.RoundToInt(dtDuoc / 100f * MatDoVetNut);
        int datGa = 0, datVoi = 0, datNut = 0;
        for (int k = 0, thu = 0; k < soVoi && thu < soVoi * 30; thu++)
        {
            var o = new ONhat { voi = true, r = Ngau(rnd, 0.8f, 1.7f), sau = Ngau(rnd, 0.20f, 0.45f) };
            o.tam = new Vector2(Ngau(rnd, pos.x + 2, pos.x + kich.x - 2), Ngau(rnd, pos.z + 2, pos.z + kich.z - 2));
            if (!ChoSach(mn, pos, buoc, N, o.tam, o.r * 1.45f)) continue;
            KhoetHo(ho, mep, pos, buoc, N, o, rnd); oNhat.Add(o); k++; datVoi++;
        }
        for (int k = 0, thu = 0; k < soGa && thu < soGa * 30; thu++)
        {
            var o = new ONhat { voi = false, r = Ngau(rnd, 0.22f, 0.55f), sau = Ngau(rnd, 0.06f, 0.16f) };
            o.tam = new Vector2(Ngau(rnd, pos.x + 2, pos.x + kich.x - 2), Ngau(rnd, pos.z + 2, pos.z + kich.z - 2));
            if (!ChoSach(mn, pos, buoc, N, o.tam, o.r * 1.45f)) continue;
            KhoetHo(ho, mep, pos, buoc, N, o, rnd); oNhat.Add(o); k++; datGa++;
        }
        for (int k = 0, thu = 0; k < soNut && thu < soNut * 30; thu++)
        {
            var p = new Vector2(Ngau(rnd, pos.x + 2, pos.x + kich.x - 2), Ngau(rnd, pos.z + 2, pos.z + kich.z - 2));
            if (!ChoSach(mn, pos, buoc, N, p, 0.6f)) continue;
            VeVetNut(nut, mn, pos, buoc, N, p, Ngau(rnd, 0f, Mathf.PI * 2f), Ngau(rnd, 2f, 7f), Ngau(rnd, 0.12f, 0.26f), Ngau(rnd, 0.03f, 0.07f), rnd, 0);
            k++; datNut++;
        }
        float lechHat = (float)rnd.NextDouble() * 500f;
        float minD = 0, maxD = 0, minW = 9e9f, maxW = -9e9f; int soKepNuoc = 0;
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float wx = pos.x + x * buoc, wz = pos.z + z * buoc;
                float san = BienDoSan * ((Mathf.PerlinNoise(wx / 1.2f + lechHat, wz / 1.2f) - 0.5f) * 1.4f + (Mathf.PerlinNoise(wx / 0.45f - lechHat, wz / 0.45f + 31f) - 0.5f) * 0.8f);
                float d = ho[z, x] + (ho[z, x] > -0.005f ? mep[z, x] : 0f) + nut[z, x] + san;
                d *= mn[z, x];
                float hc = H[z, x];
                H[z, x] += d;
                if (hc >= nuocY[z, x] + 0.05f && H[z, x] < nuocY[z, x] + 0.05f) { H[z, x] = nuocY[z, x] + 0.05f; soKepNuoc++; }
                minD = Mathf.Min(minD, d); maxD = Mathf.Max(maxD, d);
                minW = Mathf.Min(minW, H[z, x]); maxW = Mathf.Max(maxW, H[z, x]);
            }
        Ghi(string.Format("o voi {0} (r 0,8-1,7 m, sau 0,20-0,45) · o ga {1} (r 0,22-0,55, sau 0,06-0,16) · vet nut {2} chum (rong 0,12-0,26, sau 0,03-0,07) · gon san +-{3:F3}",
            datVoi, datGa, datNut, BienDoSan));
        Ghi(string.Format("do lech so voi dat cu: {0:F3} .. +{1:F3} m; dat the gioi {2:F2} .. {3:F2}; diem kep tren mat nuoc chim + 5 cm: {4}", minD, maxD, minW, maxW, soKepNuoc));

        // ---- 5. ghi do cao (mo rong khung neu ho sau hon day cu) ----
        float yMoi = Mathf.Min(y0, minW - 0.05f), dinh = Mathf.Max(y0 + sy0, maxW + 0.05f), syMoi = dinh - yMoi;
        var hn = new float[N, N];
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++) hn[z, x] = Mathf.Clamp01((H[z, x] - yMoi) / syMoi);
        td.heightmapResolution = N;
        td.size = new Vector3(kich.x, syMoi, kich.z);
        pos.y = yMoi; ter.transform.position = pos;
        td.SetHeights(0, 0, hn);
        Ghi(string.Format("terrain: {0} -> {1}, goc y {2:F3} -> {3:F3}, cao {4:F3} -> {5:F3}", r0, N, y0, yMoi, sy0, syMoi));

        // ---- 6. to lai mau ----
        if (toMauGoc != null && chiSo[0] >= 0 && chiSo[1] >= 0 && chiSo[2] >= 0 && chiSo[3] >= 0) ToLai(td, toMauGoc, chiSo, ho, mep, nut, mn, oNhat, pos, buoc, N, lechHat);
        else Ghi("[LOI] khong to lai duoc (thieu net to goc / lop)");

        // ---- 7. dat vun 3D ----
        RaiDatVun(ter, ten, mn, ho, oNhat, pos, buoc, N, rnd);

        EditorUtility.SetDirty(td);
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        AssetDatabase.SaveAssets();
    }

    static float Ngau(System.Random r, float a, float b) { return a + (float)r.NextDouble() * (b - a); }

    // ================================================================
    //  MAT NA
    // ================================================================
    static float[,] TaoMatNa(Terrain ter, Vector3 pos, Vector3 kich, int N, float buoc, float[,] Hcu, out float[,] nuocY, out int soVat)
    {
        var mn = new float[N, N];
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++) mn[z, x] = 1f;
        soVat = 0;
        // vanh: rao (HangRao) - giu dai ngoai rao 2,5 m vao trong (vach vuc dia nguc doc do cao dat o mep tung 1 m)
        float nuaRao = kich.x * 0.5f - 4f;
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (r.name.ToLower().Contains("hang_rao") || r.name.Contains("HangRao") || (r.transform.parent != null && r.transform.parent.name == "HangRao"))
                nuaRao = Mathf.Min(nuaRao, Mathf.Max(r.bounds.extents.x, r.bounds.extents.z) - 3.4f);
        Vector2 tamBd = new Vector2(pos.x + kich.x * 0.5f, pos.z + kich.z * 0.5f);
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float wx = pos.x + x * buoc, wz = pos.z + z * buoc;
                float dr = Mathf.Max(Mathf.Abs(wx - tamBd.x), Mathf.Abs(wz - tamBd.y));
                mn[z, x] = Mathf.Min(mn[z, x], Mathf.Clamp01((nuaRao - dr) / 1.5f));
            }
        // Vung GIU NGUYEN theo HINH THAT cua vat (lan 3 dung khung bao THANG TRUC: luoi nuoc dam lay rong toi 13 x 8 m (phan lon chim duoi dat)
        // -> nhieu mang dat PHANG HINH CHU NHAT 10-15 m giua nen san sui - chinh loai "o vuong o chu nhat" nguoi dung dan tranh):
        //   do vat = khung bao XOAY theo vat (8 goc khung luoi chieu xuong, bao loi); cay = hinh tron; mat nuoc = tung tam giac luoi nuoc;
        // roi tinh KHOANG CACH that (chamfer) toi vung ay -> vien chuyen tiep bo tron theo hinh vat.
        var vat = new bool[N, N]; var nuocO = new bool[N, N];
        // mat nuoc: luoi dam lay phan lon CHIM duoi dat (toi 13 x 8 m) -> chi giu quanh phan nuoc LO RA (dat cu < mat nuoc + 10 cm);
        // phan chim: Chay() khong cho dat moi xuong duoi mat nuoc + 5 cm (o ga khong lam lo nuoc, khong mat nao sat song song mat nuoc)
        nuocY = new float[N, N];
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++) nuocY[z, x] = float.NegativeInfinity;
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (r.GetComponentInParent<Terrain>() != null) continue;
            var t = r.transform; string ten = t.name;
            if (KhacDatVun(t)) continue;
            var b = r.bounds;
            if (b.size.x * b.size.z > 400f) continue;                  // rao / bau troi
            float datTam = ter.SampleHeight(b.center) + pos.y;
            if (b.min.y > datTam + 0.6f) continue;                     // treo cao, khong cham dat
            bool nuoc = t.parent != null && t.parent.name == "MatNuoc" || ten.Contains("Nuoc");
            bool cay = ten.StartsWith("TREE_") || (t.parent != null && t.parent.name == "Cay");
            var mf = r.GetComponent<MeshFilter>();
            if (nuoc && mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable)
            {
                var v = mf.sharedMesh.vertices; var tri = mf.sharedMesh.triangles; var M = t.localToWorldMatrix;
                var tam = new bool[N, N];
                for (int i = 0; i < tri.Length; i += 3)
                    ToDaGiac(tam, pos, buoc, N, new[] { XZ(M.MultiplyPoint3x4(v[tri[i]])), XZ(M.MultiplyPoint3x4(v[tri[i + 1]])), XZ(M.MultiplyPoint3x4(v[tri[i + 2]])) });
                float mat = b.max.y;
                for (int z = 0; z < N; z++)
                    for (int x = 0; x < N; x++)
                    {
                        if (!tam[z, x]) continue;
                        if (mat > nuocY[z, x]) nuocY[z, x] = mat;
                        if (Hcu[z, x] < mat + 0.10f) nuocO[z, x] = true;
                    }
            }
            else if (cay) ToTron(vat, pos, buoc, N, XZ(t.position), 1.1f);
            else if (mf != null && mf.sharedMesh != null && !(r is SkinnedMeshRenderer))
            {
                var lb = mf.sharedMesh.bounds; var M = t.localToWorldMatrix; var g8 = new Vector2[8];
                for (int i = 0; i < 8; i++)
                    g8[i] = XZ(M.MultiplyPoint3x4(lb.center + Vector3.Scale(lb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                ToDaGiac(nuoc ? nuocO : vat, pos, buoc, N, BaoLoi(g8));
            }
            else ToDaGiac(nuoc ? nuocO : vat, pos, buoc, N, new[] { new Vector2(b.min.x, b.min.z), new Vector2(b.max.x, b.min.z), new Vector2(b.max.x, b.max.z), new Vector2(b.min.x, b.max.z) });
            soVat++;
        }
        var pl = GameObject.Find("Player");                              // cho xuat phat nguoi choi
        if (pl != null) ToTron(vat, pos, buoc, N, XZ(pl.transform.position), 0.8f);
        var dVat = KhoangCach(vat, N, buoc); var dNuoc = KhoangCach(nuocO, N, buoc);
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float w = Mathf.Min(SS(Le, Le + Thoai, dVat[z, x]), SS(LeNuoc, LeNuoc + Thoai, dNuoc[z, x]));
                if (w < mn[z, x]) mn[z, x] = w;
            }
        return mn;
    }

    static Vector2 XZ(Vector3 v) { return new Vector2(v.x, v.z); }

    static void ToTron(bool[,] o, Vector3 pos, float buoc, int N, Vector2 c, float r)
    {
        int x0 = Mathf.Max(0, Mathf.FloorToInt((c.x - r - pos.x) / buoc)), x1 = Mathf.Min(N - 1, Mathf.CeilToInt((c.x + r - pos.x) / buoc));
        int z0 = Mathf.Max(0, Mathf.FloorToInt((c.y - r - pos.z) / buoc)), z1 = Mathf.Min(N - 1, Mathf.CeilToInt((c.y + r - pos.z) / buoc));
        for (int z = z0; z <= z1; z++) for (int x = x0; x <= x1; x++)
            if ((new Vector2(pos.x + x * buoc, pos.z + z * buoc) - c).sqrMagnitude <= r * r) o[z, x] = true;
    }

    /// <summary>To o luoi co TAM nam trong da giac LOI (dinh theo thu tu vong) - ca tam giac.</summary>
    static void ToDaGiac(bool[,] o, Vector3 pos, float buoc, int N, Vector2[] d)
    {
        float xa = 9e9f, xb = -9e9f, za = 9e9f, zb = -9e9f;
        foreach (var p in d) { xa = Mathf.Min(xa, p.x); xb = Mathf.Max(xb, p.x); za = Mathf.Min(za, p.y); zb = Mathf.Max(zb, p.y); }
        int x0 = Mathf.Max(0, Mathf.FloorToInt((xa - pos.x) / buoc)), x1 = Mathf.Min(N - 1, Mathf.CeilToInt((xb - pos.x) / buoc));
        int z0 = Mathf.Max(0, Mathf.FloorToInt((za - pos.z) / buoc)), z1 = Mathf.Min(N - 1, Mathf.CeilToInt((zb - pos.z) / buoc));
        for (int z = z0; z <= z1; z++)
            for (int x = x0; x <= x1; x++)
            {
                var p = new Vector2(pos.x + x * buoc, pos.z + z * buoc);
                bool duong = false, am = false;
                for (int i = 0; i < d.Length; i++)
                {
                    Vector2 a = d[i], bb = d[(i + 1) % d.Length];
                    float c = (bb.x - a.x) * (p.y - a.y) - (bb.y - a.y) * (p.x - a.x);
                    if (c > 1e-6f) duong = true; else if (c < -1e-6f) am = true;
                }
                if (!(duong && am)) o[z, x] = true;
            }
    }

    static Vector2[] BaoLoi(Vector2[] p)
    {
        var a = new List<Vector2>(p); a.Sort((u, v) => u.x != v.x ? u.x.CompareTo(v.x) : u.y.CompareTo(v.y));
        var h = new List<Vector2>();
        for (int pass = 0; pass < 2; pass++)
        {
            int dau = h.Count;
            for (int i = 0; i < a.Count; i++)
            {
                var q = pass == 0 ? a[i] : a[a.Count - 1 - i];
                while (h.Count >= dau + 2 && Cheo(h[h.Count - 2], h[h.Count - 1], q) <= 0) h.RemoveAt(h.Count - 1);
                h.Add(q);
            }
            h.RemoveAt(h.Count - 1);
        }
        return h.ToArray();
    }

    static float Cheo(Vector2 o, Vector2 a, Vector2 b) { return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x); }

    /// <summary>Khoang cach (m) toi o true gan nhat - chamfer 2 luot (1, can 2).</summary>
    static float[,] KhoangCach(bool[,] o, int N, float buoc)
    {
        var d = new float[N, N]; float a = buoc, c = buoc * 1.41421f;
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++) d[z, x] = o[z, x] ? 0f : 1e6f;
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float v = d[z, x];
                if (x > 0) v = Mathf.Min(v, d[z, x - 1] + a);
                if (z > 0) { v = Mathf.Min(v, d[z - 1, x] + a); if (x > 0) v = Mathf.Min(v, d[z - 1, x - 1] + c); if (x < N - 1) v = Mathf.Min(v, d[z - 1, x + 1] + c); }
                d[z, x] = v;
            }
        for (int z = N - 1; z >= 0; z--)
            for (int x = N - 1; x >= 0; x--)
            {
                float v = d[z, x];
                if (x < N - 1) v = Mathf.Min(v, d[z, x + 1] + a);
                if (z < N - 1) { v = Mathf.Min(v, d[z + 1, x] + a); if (x < N - 1) v = Mathf.Min(v, d[z + 1, x + 1] + c); if (x > 0) v = Mathf.Min(v, d[z + 1, x - 1] + c); }
                d[z, x] = v;
            }
        return d;
    }

    static bool KhacDatVun(Transform t)
    {
        for (var c = t; c != null; c = c.parent) if (c.name == TenGocDatVun) return true;
        return false;
    }

    static void DanMatNa(float[,] mn, Vector3 pos, float buoc, int N, Rect hcn, float le, float thoai)
    {
        float m = le + thoai;
        int x0 = Mathf.Max(0, Mathf.FloorToInt((hcn.xMin - m - pos.x) / buoc)), x1 = Mathf.Min(N - 1, Mathf.CeilToInt((hcn.xMax + m - pos.x) / buoc));
        int z0 = Mathf.Max(0, Mathf.FloorToInt((hcn.yMin - m - pos.z) / buoc)), z1 = Mathf.Min(N - 1, Mathf.CeilToInt((hcn.yMax + m - pos.z) / buoc));
        for (int z = z0; z <= z1; z++)
            for (int x = x0; x <= x1; x++)
            {
                float wx = pos.x + x * buoc, wz = pos.z + z * buoc;
                float dx = Mathf.Max(0f, Mathf.Max(hcn.xMin - wx, wx - hcn.xMax)), dz = Mathf.Max(0f, Mathf.Max(hcn.yMin - wz, wz - hcn.yMax));
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                float w = Mathf.Clamp01((d - le) / thoai);
                w = w * w * (3f - 2f * w);
                if (w < mn[z, x]) mn[z, x] = w;
            }
    }

    static float MatNaTai(float[,] mn, Vector3 pos, float buoc, int N, Vector2 p)
    {
        int x = Mathf.RoundToInt((p.x - pos.x) / buoc), z = Mathf.RoundToInt((p.y - pos.z) / buoc);
        if (x < 0 || z < 0 || x >= N || z >= N) return 0f;
        return mn[z, x];
    }

    /// <summary>Tam o sach hoan toan, nua trong cua o sach; mep ngoai cho phep cham vung thoai (o nhan mat na nen mo dan sat do vat).</summary>
    static bool ChoSach(float[,] mn, Vector3 pos, float buoc, int N, Vector2 tam, float r)
    {
        if (MatNaTai(mn, pos, buoc, N, tam) < 0.999f) return false;
        for (int k = 0; k < 12; k++)
        {
            float a = k / 12f * Mathf.PI * 2f;
            var h = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            if (MatNaTai(mn, pos, buoc, N, tam + h * r * 0.35f) < 0.999f) return false;
            if (MatNaTai(mn, pos, buoc, N, tam + h * r * 0.7f) < 0.5f) return false;
        }
        return true;
    }

    // ================================================================
    //  O GA / O VOI / VET NUT
    // ================================================================
    static void KhoetHo(float[,] ho, float[,] mep, Vector3 pos, float buoc, int N, ONhat o, System.Random rnd)
    {
        float R = o.r, rr = R * 1.4f;
        float ph1 = Ngau(rnd, 0, 6.28f), ph2 = Ngau(rnd, 0, 6.28f), ph3 = Ngau(rnd, 0, 6.28f);
        float lechDay = Ngau(rnd, 0, 100f);
        float caoMep = Mathf.Min(0.03f, o.sau * 0.15f);
        int x0 = Mathf.Max(0, Mathf.FloorToInt((o.tam.x - rr - pos.x) / buoc)), x1 = Mathf.Min(N - 1, Mathf.CeilToInt((o.tam.x + rr - pos.x) / buoc));
        int z0 = Mathf.Max(0, Mathf.FloorToInt((o.tam.y - rr - pos.z) / buoc)), z1 = Mathf.Min(N - 1, Mathf.CeilToInt((o.tam.y + rr - pos.z) / buoc));
        for (int z = z0; z <= z1; z++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = pos.x + x * buoc - o.tam.x, dz = pos.z + z * buoc - o.tam.y;
                float a = Mathf.Atan2(dz, dx);
                // vanh meo: 3 song goc
                float Ra = R * (1f + 0.13f * Mathf.Sin(2f * a + ph1) + 0.08f * Mathf.Sin(3f * a + ph2) + 0.05f * Mathf.Sin(5f * a + ph3));
                float t = Mathf.Sqrt(dx * dx + dz * dz) / Ra;
                if (t < 1f)
                {
                    float sau = -o.sau * Mathf.Pow(1f - Mathf.Pow(t, 2.5f), 0.8f);   // day bang hon, thanh doc hon (lan 1 (1-t2)^0,7: bat thoai, khong doc ra o)
                    // day lom lom (da vun, bun dong)
                    sau *= 0.85f + 0.3f * Mathf.PerlinNoise((pos.x + x * buoc) / 0.35f + lechDay, (pos.z + z * buoc) / 0.35f);
                    if (sau < ho[z, x]) ho[z, x] = sau;
                }
                else if (t < 1.4f)
                {
                    float g = caoMep * Mathf.Sin(Mathf.PI * (t - 1f) / 0.4f);
                    if (g > mep[z, x]) mep[z, x] = g;
                }
            }
    }

    static void VeVetNut(float[,] nut, float[,] mn, Vector3 pos, float buoc, int N, Vector2 p, float goc, float dai, float rong, float sau, System.Random rnd, int cap)
    {
        const float BuocNut = 0.12f;
        int soBuoc = Mathf.CeilToInt(dai / BuocNut);
        for (int s = 0; s < soBuoc; s++)
        {
            if (MatNaTai(mn, pos, buoc, N, p) < 0.999f) return;
            float tiLe = 1f - s / (float)soBuoc;                      // hep + nong dan ve cuoi
            float w = rong * (0.45f + 0.55f * tiLe), d = sau * (0.4f + 0.6f * tiLe);
            int x0 = Mathf.Max(0, Mathf.FloorToInt((p.x - w - pos.x) / buoc)), x1 = Mathf.Min(N - 1, Mathf.CeilToInt((p.x + w - pos.x) / buoc));
            int z0 = Mathf.Max(0, Mathf.FloorToInt((p.y - w - pos.z) / buoc)), z1 = Mathf.Min(N - 1, Mathf.CeilToInt((p.y + w - pos.z) / buoc));
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = pos.x + x * buoc - p.x, dz = pos.z + z * buoc - p.y;
                    float r = Mathf.Sqrt(dx * dx + dz * dz) / (w * 0.5f + 0.0001f);
                    if (r >= 1f) continue;
                    float v = -d * (1f - r);                            // mat cat chu V
                    if (v < nut[z, x]) nut[z, x] = v;
                }
            goc += (float)(NgauChuan(rnd) * 0.32);
            p += new Vector2(Mathf.Cos(goc), Mathf.Sin(goc)) * BuocNut;
            if (cap < 2 && rnd.NextDouble() < 0.035)
                VeVetNut(nut, mn, pos, buoc, N, p, goc + (rnd.NextDouble() < 0.5 ? 1 : -1) * Ngau(rnd, 0.5f, 1.2f), dai * Ngau(rnd, 0.25f, 0.55f) * tiLe,
                    rong * 0.7f, sau * 0.8f, rnd, cap + 1);
        }
    }

    static double NgauChuan(System.Random r)
    {
        double u1 = 1.0 - r.NextDouble(), u2 = r.NextDouble();
        return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);
    }

    // ================================================================
    //  TO MAU
    // ================================================================
    static void ToLai(TerrainData td, float[,,] goc, int[] ci, float[,] ho, float[,] mep, float[,] nut, float[,] mn, List<ONhat> oNhat, Vector3 pos, float buoc, int N, float lech)
    {
        int M = goc.GetLength(0), L = goc.GetLength(2);
        var a = (float[,,])goc.Clone();
        float kichX = td.size.x;
        // loai o (voi / ga) theo o luoi do cao: danh dau o voi
        var laVoi = new bool[N, N];
        foreach (var o in oNhat)
        {
            if (!o.voi) continue;
            float rr = o.r * 1.2f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt((o.tam.x - rr - pos.x) / buoc)), x1 = Mathf.Min(N - 1, Mathf.CeilToInt((o.tam.x + rr - pos.x) / buoc));
            int z0 = Mathf.Max(0, Mathf.FloorToInt((o.tam.y - rr - pos.z) / buoc)), z1 = Mathf.Min(N - 1, Mathf.CeilToInt((o.tam.y + rr - pos.z) / buoc));
            for (int z = z0; z <= z1; z++) for (int x = x0; x <= x1; x++) laVoi[z, x] = true;
        }
        double[] tong = new double[L], tongGoc = new double[L];
        for (int j = 0; j < M; j++)
            for (int i = 0; i < M; i++)
            {
                float wx = pos.x + (i + 0.5f) / M * kichX, wz = pos.z + (j + 0.5f) / M * kichX;
                int hx = Mathf.Clamp(Mathf.RoundToInt((wx - pos.x) / buoc), 0, N - 1), hz = Mathf.Clamp(Mathf.RoundToInt((wz - pos.z) / buoc), 0, N - 1);
                float w = Mathf.Clamp01(mn[hz, hx] * 1.25f);           // khong to quanh nuoc / vanh / chan do vat lon (dat tro quanh mo giu nguyen)
                float n1 = Mathf.PerlinNoise(wx / 9f + lech, wz / 9f - lech), n2 = Mathf.PerlinNoise(wx / 4.5f - lech, wz / 4.5f + 77f);
                float nutChuyen = SS(0.50f, 0.68f, n1) * 0.85f * w;     // co chet -> dat nut ne
                float soiChuyen = SS(0.56f, 0.76f, n2) * 0.75f * w;     // -> soi da vun
                Chuyen(a, j, i, ci[0], ci[1], a[j, i, ci[0]] * nutChuyen);
                Chuyen(a, j, i, ci[0], ci[2], a[j, i, ci[0]] * soiChuyen);
                float h = ho[hz, hx];
                // LONG o = bun uot TOI, MEP o = dat nut kho SANG (x1,7 nen) - vien sang om long toi thi o moi doc ra duoi anh trang
                // (lan 1: o ga to soi - sang hon nen, o voi to bun chi toi hon nen 15% -> anh chup trong game khong thay o)
                if (h < -0.015f)
                {
                    float k = Mathf.Clamp01(-h / 0.08f) * w;
                    for (int l = 0; l < L; l++) if (l != ci[3]) Chuyen(a, j, i, l, ci[3], a[j, i, l] * k);
                }
                else if (mep[hz, hx] > 0.002f)
                {
                    float k = Mathf.Clamp01(mep[hz, hx] / 0.012f) * 0.85f * w;
                    for (int l = 0; l < L; l++) if (l != ci[1]) Chuyen(a, j, i, l, ci[1], a[j, i, l] * k);
                }
                if (nut[hz, hx] < -0.012f)
                    for (int l = 0; l < L; l++) if (l != ci[3]) Chuyen(a, j, i, l, ci[3], a[j, i, l] * 0.6f * w);
                for (int l = 0; l < L; l++) { tong[l] += a[j, i, l]; tongGoc[l] += goc[j, i, l]; }
            }
        td.SetAlphamaps(0, 0, a);
        var sb = new StringBuilder("to mau (ti le phu truoc -> sau):");
        for (int l = 0; l < L; l++) sb.AppendFormat(" {0} {1:P0} -> {2:P0};", td.terrainLayers[l] != null ? td.terrainLayers[l].name : "?", tongGoc[l] / (M * M), tong[l] / (M * M));
        Ghi(sb.ToString());
    }

    static float SS(float a, float b, float x) { float t = Mathf.Clamp01((x - a) / (b - a)); return t * t * (3f - 2f * t); }
    static void Chuyen(float[,,] a, int j, int i, int tu, int den, float luong) { a[j, i, tu] -= luong; a[j, i, den] += luong; }

    // ================================================================
    //  DAT VUN 3D
    // ================================================================
    class Manh { public Mesh luoi; public int vl; public Matrix4x4 m; }

    static void RaiDatVun(Terrain ter, string ten, float[,] mn, float[,] ho, List<ONhat> oNhat, Vector3 pos, float buoc, int N, System.Random rnd)
    {
        string fbx = ThuMucDatVun + "/DatVun.fbx";
        var mi = AssetImporter.GetAtPath(fbx) as ModelImporter;
        if (mi == null) { Ghi("[LOI] thieu " + fbx); return; }
        if (!mi.isReadable || mi.materialImportMode != ModelImporterMaterialImportMode.None)
        { mi.isReadable = true; mi.materialImportMode = ModelImporterMaterialImportMode.None; mi.SaveAndReimport(); }
        var luoi = new Dictionary<string, Mesh>();
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(fbx)) if (o is Mesh) luoi[o.name] = (Mesh)o;
        var vls = new[] { VatLieu("DatVun_Dat", "DatTroc"), VatLieu("DatVun_Da", "SoiDa") };

        // xoa ban cu
        var cu = GameObject.Find(TenGocDatVun); if (cu != null) Object.DestroyImmediate(cu);
        string thuMuc = ThuMucDatVun + "/" + ten;                     // luoi gop cua ban dau (khong dung nua)
        if (AssetDatabase.IsValidFolder(thuMuc)) AssetDatabase.DeleteAsset(thuMuc);

        var td = ter.terrainData; Vector3 kich = td.size;
        var manh = new List<Manh>();
        int[] dem = new int[4]; int boDongPhang = 0;
        float lech = (float)rnd.NextDouble() * 300f;
        const float O = 0.5f;
        for (float wz = pos.z + 1f; wz < pos.z + kich.z - 1f; wz += O)
            for (float wx = pos.x + 1f; wx < pos.x + kich.x - 1f; wx += O)
            {
                float cum = 0.25f + 2.6f * SS(0.45f, 0.85f, Mathf.PerlinNoise(wx / 6f + lech, wz / 6f - lech));
                foreach (var o in oNhat)
                    if (o.voi && (new Vector2(wx, wz) - o.tam).sqrMagnitude < o.r * o.r * 2.6f) { cum += 1.5f; break; }
                float lambda = MatDoDatVun * O * O * cum;
                int so = Poisson(rnd, lambda);
                for (int s = 0; s < so; s++)
                {
                    var p2 = new Vector2(wx + Ngau(rnd, 0, O), wz + Ngau(rnd, 0, O));
                    double c = rnd.NextDouble();
                    int loai = c < 0.32 ? 0 : c < 0.75 ? 1 : c < 0.93 ? 2 : 3;      // cuc dat · da vun · mang dat · CUM ngon ngang
                    if (loai < 3) { if (ThemManh(manh, luoi, td, pos, mn, buoc, N, p2, loai, rnd, ref boDongPhang)) dem[loai]++; continue; }
                    // cum dat vo ngon ngang: 6-12 manh chong nhau trong 0,25-0,7 m (thay "dong dat" tron nhu qua trung o lan 1)
                    int k = rnd.Next(6, 13); float rc = Ngau(rnd, 0.25f, 0.7f);
                    for (int j = 0; j < k; j++)
                    {
                        float ga = Ngau(rnd, 0f, 6.28f), gr = rc * Mathf.Sqrt((float)rnd.NextDouble());
                        double cc = rnd.NextDouble();
                        int lj = cc < 0.45 ? 0 : cc < 0.8 ? 1 : 2;
                        if (ThemManh(manh, luoi, td, pos, mn, buoc, N, p2 + new Vector2(Mathf.Cos(ga), Mathf.Sin(ga)) * gr, lj, rnd, ref boDongPhang)) dem[lj]++;
                    }
                    dem[3]++;
                }
            }

        // sap theo o 16 m -> DatVunVe (GPU instancing; lan dau gop luoi: 572 nghin dinh, qua nang cho dien thoai)
        var tenLuoi = new List<string>(luoi.Keys); tenLuoi.Sort();
        var chiSoLuoi = new Dictionary<Mesh, int>();
        var dsLuoi = new Mesh[tenLuoi.Count]; var dsVl = new Material[tenLuoi.Count];
        for (int i = 0; i < tenLuoi.Count; i++) { dsLuoi[i] = luoi[tenLuoi[i]]; chiSoLuoi[dsLuoi[i]] = i; dsVl[i] = tenLuoi[i].StartsWith("DaVun") ? vls[1] : vls[0]; }
        var theoO = new SortedDictionary<long, List<Manh>>();
        foreach (var mh in manh)
        {
            var p = mh.m.GetColumn(3);
            long k = (long)Mathf.FloorToInt((p.x - pos.x) / CoO) * 1000 + Mathf.FloorToInt((p.z - pos.z) / CoO);
            List<Manh> ds; if (!theoO.TryGetValue(k, out ds)) theoO[k] = ds = new List<Manh>();
            ds.Add(mh);
        }
        var goc = new GameObject(TenGocDatVun);
        var ve = goc.AddComponent<DatVunVe>();
        int n = manh.Count;
        ve.luoi = dsLuoi; ve.vatLieu = dsVl;
        ve.loai = new int[n]; ve.viTri = new Vector3[n]; ve.xoay = new Quaternion[n]; ve.tiLe = new Vector3[n];
        ve.batDau = new int[theoO.Count + 1]; ve.khung = new Bounds[theoO.Count];
        int ci = 0, oi = 0, soTamGiac = 0;
        foreach (var kv in theoO)
        {
            ve.batDau[oi] = ci;
            Bounds kh = new Bounds();
            for (int i = 0; i < kv.Value.Count; i++)
            {
                var mh = kv.Value[i];
                ve.loai[ci] = chiSoLuoi[mh.luoi];
                ve.viTri[ci] = mh.m.GetColumn(3);
                ve.xoay[ci] = mh.m.rotation;
                ve.tiLe[ci] = mh.m.lossyScale;
                var bb = mh.luoi.bounds; var tam = mh.m.MultiplyPoint3x4(bb.center);
                var b2 = new Bounds(tam, Vector3.one * bb.size.magnitude * Mathf.Max(ve.tiLe[ci].x, ve.tiLe[ci].y));
                if (i == 0) kh = b2; else kh.Encapsulate(b2);
                soTamGiac += mh.luoi.triangles.Length / 3;
                ci++;
            }
            ve.khung[oi] = kh; oi++;
        }
        ve.batDau[oi] = ci;
        Ghi(string.Format("dat vun: {0} manh (cuc dat {1} · da vun {2} · mang dat {3}; {4} cum ngon ngang); bo {5} cho do vi nam song song sat dat; {6} o {7} m, {8} tam giac (ve bang instancing, chi o trong khung may quay)",
            manh.Count, dem[0], dem[1], dem[2], dem[3], boDongPhang, theoO.Count, CoO, soTamGiac));
    }

    static bool ThemManh(List<Manh> manh, Dictionary<string, Mesh> luoi, TerrainData td, Vector3 pos, float[,] mn, float buoc, int N, Vector2 p2, int loai,
        System.Random rnd, ref int boDongPhang)
    {
        if (MatNaTai(mn, pos, buoc, N, p2) < 0.97f) return false;
        Vector3 kich = td.size;
        float u = (p2.x - pos.x) / kich.x, v = (p2.y - pos.z) / kich.z;
        if (u < 0 || v < 0 || u > 1 || v > 1) return false;
        Vector3 n = td.GetInterpolatedNormal(u, v);
        float y = td.GetInterpolatedHeight(u, v) + pos.y;
        string tenLuoi = loai == 0 ? "CucDat" + rnd.Next(6) : loai == 1 ? "DaVun" + rnd.Next(4) : "MangDat" + rnd.Next(3);
        Mesh me; if (!luoi.TryGetValue(tenLuoi, out me)) return false;
        Quaternion q = Quaternion.FromToRotation(Vector3.up, n) * Quaternion.Euler(0f, Ngau(rnd, 0f, 360f), 0f);
        float tl = Ngau(rnd, 0.75f, 1.35f);
        if (loai == 2)
        {
            // phien dat NGHIENG 8-25 do quanh mot truc ngang (mot mep chon, mot mep venh len) - khong bao gio nam song song mat dat
            var truc = q * new Vector3(Mathf.Cos(Ngau(rnd, 0, 6.28f)), 0f, Mathf.Sin(Ngau(rnd, 0, 6.28f)));
            q = Quaternion.AngleAxis(Ngau(rnd, 8f, 25f), truc) * q;
        }
        var m = Matrix4x4.TRS(new Vector3(p2.x, y, p2.y), q, new Vector3(tl, tl * Ngau(rnd, 0.85f, 1.15f), tl));
        if (DongPhang(me, m, td, pos)) { boDongPhang++; return false; }
        manh.Add(new Manh { luoi = me, vl = loai == 1 ? 1 : 0, m = m });
        return true;
    }

    static int Poisson(System.Random r, float lambda)
    {
        double L = System.Math.Exp(-lambda), p = 1; int k = 0;
        do { k++; p *= r.NextDouble(); } while (p > L && k < 30);
        return k - 1;
    }

    /// <summary>Manh co mat nao (dien tich > 0,004 m2) gan SONG SONG voi mat dat (lech < 12 do) va cach dat < 2,5 cm khong - co thi bo.</summary>
    public static bool DongPhang(Mesh me, Matrix4x4 m, TerrainData td, Vector3 pos)
    {
        var v = me.vertices; var t = me.triangles; Vector3 kich = td.size;
        for (int i = 0; i < t.Length; i += 3)
        {
            Vector3 a = m.MultiplyPoint3x4(v[t[i]]), b = m.MultiplyPoint3x4(v[t[i + 1]]), c = m.MultiplyPoint3x4(v[t[i + 2]]);
            Vector3 cr = Vector3.Cross(b - a, c - a); float dt = cr.magnitude * 0.5f;
            if (dt < 0.004f) continue;
            Vector3 g = (a + b + c) / 3f;
            float u = (g.x - pos.x) / kich.x, w = (g.z - pos.z) / kich.z;
            if (u < 0 || w < 0 || u > 1 || w > 1) continue;
            Vector3 n = td.GetInterpolatedNormal(u, w);
            if (Vector3.Angle(cr, n) > 12f && Vector3.Angle(-cr, n) > 12f) continue;
            float khe = Mathf.Abs(g.y - (td.GetInterpolatedHeight(u, w) + pos.y));
            if (khe < 0.025f) return true;
        }
        return false;
    }

    static Material VatLieu(string ten, string lop)
    {
        string duong = ThuMucDatVun + "/" + ten + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(duong);
        var sd = Shader.Find("Diablo25D/ChuanSuong");
        if (m == null) { m = new Material(sd); AssetDatabase.CreateAsset(m, duong); }
        m.shader = sd;                                                  // Standard + suong chien tranh (_TN_Bat = 0 ngoai Act2 -> nhu Standard)
        m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Terrain/AnhDatMoi/" + lop + "_Mau.png"));
        m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Terrain/AnhDatMoi/" + lop + "_Phap.png"));
        m.SetFloat("_BumpScale", DoNoiPhapTuyen);
        m.SetFloat("_OcclusionStrength", 0f);
        m.SetFloat("_CoAnhKim", 0f); m.SetFloat("_CoPhat", 0f);
        m.SetFloat("_Glossiness", 0.06f);
        m.SetFloat("_Metallic", 0f);
        m.SetColor("_Color", Color.white);
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    // ================================================================
    //  LUU / DOC GOC
    // ================================================================
    static string DuongDoCao(string ten) { return "Assets/Terrain/" + ten + "_DoCaoTruocSanSui.bytes"; }
    static string DuongToMau(string ten) { return "Assets/Terrain/" + ten + "_ToMauTruocSanSui.bytes"; }

    static void LuuDoCao(string ten, float[,] h, int r, float y0, float sy)
    {
        using (var w = new BinaryWriter(File.Create(DuongDoCao(ten))))
        {
            w.Write(r); w.Write(y0); w.Write(sy);
            for (int z = 0; z < r; z++) for (int x = 0; x < r; x++) w.Write(h[z, x]);
        }
        AssetDatabase.ImportAsset(DuongDoCao(ten));
    }

    public static float[,] DocDoCao(string ten, out int r)
    {
        r = 0;
        if (!File.Exists(DuongDoCao(ten))) return null;
        using (var rd = new BinaryReader(File.OpenRead(DuongDoCao(ten))))
        {
            r = rd.ReadInt32(); rd.ReadSingle(); rd.ReadSingle();
            var h = new float[r, r];
            for (int z = 0; z < r; z++) for (int x = 0; x < r; x++) h[z, x] = rd.ReadSingle();
            return h;
        }
    }

    static void DocKhung(string ten, out float y0, out float sy)
    {
        using (var rd = new BinaryReader(File.OpenRead(DuongDoCao(ten)))) { rd.ReadInt32(); y0 = rd.ReadSingle(); sy = rd.ReadSingle(); }
    }

    static void LuuToMau(string ten, float[,,] a)
    {
        int M = a.GetLength(0), L = a.GetLength(2);
        using (var w = new BinaryWriter(File.Create(DuongToMau(ten))))
        {
            w.Write(M); w.Write(L);
            for (int j = 0; j < M; j++) for (int i = 0; i < M; i++) for (int l = 0; l < L; l++) w.Write(a[j, i, l]);
        }
        AssetDatabase.ImportAsset(DuongToMau(ten));
    }

    static float[,,] DocToMau(string ten)
    {
        if (!File.Exists(DuongToMau(ten))) return null;
        using (var rd = new BinaryReader(File.OpenRead(DuongToMau(ten))))
        {
            int M = rd.ReadInt32(), L = rd.ReadInt32();
            var a = new float[M, M, L];
            for (int j = 0; j < M; j++) for (int i = 0; i < M; i++) for (int l = 0; l < L; l++) a[j, i, l] = rd.ReadSingle();
            return a;
        }
    }
}
