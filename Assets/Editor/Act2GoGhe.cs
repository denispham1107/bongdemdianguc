using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// VE LAI MAT DAT ACT2 CHO GO GHE NHU ACT1.
///
/// Mat dat Act2 duoc doc tu mot luoi Blender gan nhu phang li: do lech chuan do
/// cao trong vung loi chi 0,22 m, doc trung binh 1,9 do. Act1 - dat bang cong
/// thuc Perlin - la 0,43 m va 4,3 do. Di trong Act2 nhu di tren mat ban.
///
/// Cong cu nay CONG THEM mot lop dia hinh Perlin ba tang len tren nen san co,
/// dung tan so cua Act1 (<see cref="WorldFactory.AnalyticHeight"/>):
///
///   - doi thoai   buoc song ~28 m   -> suon dai, cho cao cho thap
///   - go nho      buoc song ~9 m    -> nhap nho vua tam nhin
///   - gon lan tan buoc song ~2,6 m  -> pha net phang cua be mat
///
/// KHONG cong deu khap noi. Ba cho phai giu nguyen, khong thi hong:
///
///   1. DAM LAY VA HO NUOC. Mat nuoc la luoi cat san theo long chao cu, do sau
///      luu trong mau dinh. Nang dat len duoi no la go dat dam thung mat nuoc;
///      ha xuong la nuoc treo lung lo. Giu nguyen ca vung, va vanh ngoai chi
///      cho phep NANG - de vung nuoc luon la cho trung nhat quanh no.
///   2. SAT RIA BAN DO. Hang rao bao quanh la MOT luoi lien, chan no o y=0,01
///      con dat quanh do cao 0,58-2,77 m (no von duoc chon vao vanh dat). Xe
///      dich dat o do la ho chan rao hoac chon mat rao.
///   3. DUOI NAM NHA MO. Chung la khoi da 4-6 m, dat tren suon doc thi ho chan
///      mot ben. San phang nen duoi chung nhu nguoi dung canh van lam.
///
/// SAU DO PHAI DAT LAI DO DAC. 452 bia mo, 229 tang da, 58 cay, 5 nha mo va 70
/// bui co dang cam san vao mat dat cu. Doi dat ma khong doi chung thi ca nghia
/// trang treo lo lung. Moi vat duoc doi dung bang phan dat duoi chan no da
/// dich, nen do sau chon xuong dat giu nguyen y cu.
///
/// CHAY LAI DUOC. Lan dau chay, do cao NEN PHANG duoc luu ra
/// <c>Assets/Terrain/Act2_DoCaoPhang.bytes</c>; nhung lan sau lay lai tu do chu
/// khong cong don. Muon doi bien do thi sua <see cref="BienDo"/> roi chay lai.
///
/// CANH BAO: chay lai se XOA net to tay bang cong cu Terrain neu ban da sua do
/// cao bang tay. Rieng viec to LOP VAT LIEU bang tay thi khong mat.
/// </summary>
public static class Act2GoGhe
{
    // ================================================================
    //  SO DO
    // ================================================================

    /// <summary>
    /// He so nhan cho ca ba tang. 1,0 = dung bien do cua Act1.
    /// Do sau khi chay: xem log, no in ra do lech chuan va do doc de doi chieu.
    /// </summary>
    public const float BienDo = 0.80f;

    /// <summary>Bien do ba tang, lay dung cua Act1.</summary>
    const float DoiThoai = 2.60f;    // buoc song ~28 m
    const float GoNho = 0.75f;    // buoc song ~9 m
    const float GonLanTan = 0.16f;    // buoc song ~2,6 m

    /// <summary>
    /// Lech hat cua Perlin. PHAI khac Act1, khong thi hai man choi ra dung mot
    /// hinh dang doi nui - nguoi choi di sang man moi ma thay quen mat.
    /// </summary>
    const float HatDoi = 61.3f, HatDoiZ = 27.9f;
    const float HatGo = 83.1f, HatGoZ = 45.7f;
    const float HatGon = 19.7f, HatGonZ = 71.4f;

    /// <summary>Vung nuoc: giu nguyen den ban kinh x, thoai dan het o ban kinh y.</summary>
    const float NuocGiu = 1.05f, NuocThoai = 2.30f;

    /// <summary>Nha mo: san phang den ban kinh nay, thoai dan het o ban kinh sau.</summary>
    const float MoGiu = 4.5f, MoThoai = 8.5f;

    /// <summary>Ria ban do (do theo canh vuong): giu nguyen tu day tro ra.</summary>
    const float RiaBatDau = 44f, RiaHet = 51f;

    const string DuongNenPhang = "Assets/Terrain/Act2_DoCaoPhang.bytes";
    const string DuongToMauGoc = "Assets/Terrain/Act2_ToMauGoc.bytes";

    // ================================================================
    //  DIEM VAO
    // ================================================================

    [MenuItem("Diablo 2.5D/10. Ve lai dia hinh Act2 (go ghe nhu Act1)", false, 91)]
    public static void VeLai()
    {
        VeLai(true);
    }

    /// <summary>
    /// Ve lai dia hinh tu nen phang goc.
    ///
    /// <paramref name="datLaiDoDac"/> = true la duong dung BINH THUONG: dia hinh
    /// doi thi do dac phai di theo, khong thi ca nghia dia lo lung hoac thut dat.
    ///
    /// De FALSE khi dang HOAN NGUYEN mot lan nghich dia hinh. Luc do "dat cu"
    /// chinh la cai dat da bi lam sai, nen dich do dac theo no la CHEP LAI cai
    /// sai: vat dang bi chon nua nguoi thi sau khi dich van chon nua nguoi, chi
    /// khac la ca hai cung tut xuong. Bo qua buoc do thi dat ha ve dung cho cu
    /// con do dac dung yen - dung la thu can.
    /// </summary>
    public static void VeLai(bool datLaiDoDac)
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Act2] Thoat Play mode truoc da.");
            return;
        }

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Act2")
        {
            Debug.LogError("[Act2] Mo canh Act2 truoc da - dang mo '" + scene.name + "'.");
            return;
        }

        var terr = Object.FindFirstObjectByType<Terrain>();
        if (terr == null) { Debug.LogError("[Act2] Khong tim thay Terrain."); return; }

        var td = terr.terrainData;
        int N = td.heightmapResolution;
        float canh = td.size.x;
        float buoc = canh / (N - 1);
        Vector3 goc = terr.transform.position;

        // ---- 1. Do cao TUYET DOI cua nen phang goc ----
        float[,] nen = DocNenPhang(td, goc.y, N);

        // ---- 2. Ghi nho dat dang o dau duoi chan tung do dac ----
        var world = GameObject.Find("World");
        var doDac = GomDoDac(world);
        var datCu = new Dictionary<Transform, float>(doDac.Count);
        foreach (var tr in doDac)
            datCu[tr] = terr.SampleHeight(tr.position) + goc.y;

        // ---- 3. Cac vung phai giu nguyen ----
        var vungNuoc = DocVungNuoc(world);
        var vungMo = DocVungNhaMo(world);

        // ---- 4. Cong lop dia hinh moi ----
        var moi = new float[N, N];
        float thap = 9e9f, cao = -9e9f;

        for (int z = 0; z < N; z++)
        {
            float pz = goc.z + z * buoc;
            for (int x = 0; x < N; x++)
            {
                float px = goc.x + x * buoc;

                float them = BaTang(px, pz) * MatNaRia(px, pz);
                them = KeoVeTam(them, px, pz, vungNuoc, NuocGiu, NuocThoai);
                them = KeoVeTam(them, px, pz, vungMo, 1f, MoThoai / MoGiu);

                float y = nen[z, x] + them;
                moi[z, x] = y;
                if (y < thap) thap = y;
                if (y > cao) cao = y;
            }
        }

        // ---- 5. Ap vao terrain ----
        float khoang = Mathf.Max(1f, cao - thap);
        var h01 = new float[N, N];
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
                h01[z, x] = (moi[z, x] - thap) / khoang;

        Undo.RecordObject(td, "Ve lai dia hinh Act2");
        Undo.RecordObject(terr.transform, "Ve lai dia hinh Act2");

        td.size = new Vector3(td.size.x, khoang, td.size.z);
        td.SetHeights(0, 0, h01);
        terr.transform.position = new Vector3(goc.x, thap, goc.z);

        // ---- 6. To lai vat lieu theo dia hinh moi ----
        int soO = ToLaiTheoDiaHinh(td);

        // ---- 7. Dat lai do dac theo mat dat moi ----
        float gocMoi = terr.transform.position.y;
        int daDoi = 0;
        float dichMax = 0f;
        if (datLaiDoDac)
        {
            foreach (var tr in doDac)
            {
                float datMoi = terr.SampleHeight(tr.position) + gocMoi;
                float dich = datMoi - datCu[tr];
                if (Mathf.Abs(dich) < 0.0005f) continue;

                Undo.RecordObject(tr, "Ve lai dia hinh Act2");
                tr.position = new Vector3(tr.position.x, tr.position.y + dich, tr.position.z);
                daDoi++;
                if (Mathf.Abs(dich) > Mathf.Abs(dichMax)) dichMax = dich;
            }
        }

        EditorUtility.SetDirty(td);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();

        Debug.Log(string.Format(
            "[Act2] Da ve lai dia hinh. Cao do {0:F2} .. {1:F2} m (khoang {2:F2} m). "
          + "Da doi {3} vat the, cho dich nhieu nhat {4:F2} m. To lai {5} o soi da.",
            thap, cao, khoang, daDoi, dichMax, soO));

        Do(terr, 30f);
        Do(terr, 45f);
    }

    // ================================================================
    //  TRUONG DO CAO
    // ================================================================

    /// <summary>Ba tang Perlin, cung tan so voi Act1.</summary>
    static float BaTang(float px, float pz)
    {
        float doi = (Mathf.PerlinNoise(px * 0.035f + HatDoi, pz * 0.035f + HatDoiZ) - 0.5f) * DoiThoai;
        float go = (Mathf.PerlinNoise(px * 0.110f + HatGo, pz * 0.110f + HatGoZ) - 0.5f) * GoNho;
        float gon = (Mathf.PerlinNoise(px * 0.380f + HatGon, pz * 0.380f + HatGonZ) - 0.5f) * GonLanTan;
        return (doi + go + gon) * BienDo;
    }

    /// <summary>
    /// Tat dan ve 0 khi ra sat ria. Do theo CANH VUONG (max cua |x|,|z|) chu
    /// khong theo vong tron: hang rao la mot khung vuong, tat theo vong tron thi
    /// bon goc rao van bi xe dich.
    /// </summary>
    static float MatNaRia(float px, float pz)
    {
        float m = Mathf.Max(Mathf.Abs(px), Mathf.Abs(pz));
        if (m <= RiaBatDau) return 1f;
        if (m >= RiaHet) return 0f;
        return Mathf.SmoothStep(1f, 0f, (m - RiaBatDau) / (RiaHet - RiaBatDau));
    }

    struct Vung { public Vector2 tam; public float banKinh; }

    /// <summary>
    /// GIU MOT VUNG PHANG bang cach KEO gia tri ve dung gia tri TAI TAM vung,
    /// chu khong nhan ve 0.
    ///
    /// Nhan ve 0 la cach nghi dau tien va no SAI. Do cao them o ngoai vanh len
    /// toi +-3,4 m ma trong vanh la 0, nen ca vanh chuyen tiep bien thanh mot
    /// cai go bao quanh vung nuoc. Do duoc: 63% so o doc hon 18 do nam dung
    /// trong vanh nay, cho doc nhat 35,8 do - Act1 chi co 12,6 do.
    ///
    /// Keo ve gia tri tai tam thi long chao duoc nang ha NGUYEN KHOI theo dia
    /// hinh: hinh dang long chao va do sau nuoc giu y nguyen, ma hai dau vanh
    /// chuyen tiep chi lech nhau dung phan dia hinh bien thien trong vai met -
    /// tang doi thoai co buoc song 28 m nen phan do rat nho.
    /// </summary>
    static float KeoVeTam(float f, float px, float pz, List<Vung> vung,
                          float giu, float thoai)
    {
        for (int i = 0; i < vung.Count; i++)
        {
            float dx = px - vung[i].tam.x, dz = pz - vung[i].tam.y;
            float bk = Mathf.Max(0.01f, vung[i].banKinh);
            float d = Mathf.Sqrt(dx * dx + dz * dz) / bk;
            if (d >= thoai) continue;

            float tam = BaTang(vung[i].tam.x, vung[i].tam.y)
                      * MatNaRia(vung[i].tam.x, vung[i].tam.y);

            if (d <= giu) return tam;

            float k = Mathf.SmoothStep(0f, 1f, (d - giu) / (thoai - giu));
            f = Mathf.Lerp(tam, f, k);
        }
        return f;
    }

    // ================================================================
    //  NEN PHANG GOC
    // ================================================================

    /// <summary>
    /// Do cao tuyet doi cua nen TRUOC KHI cong dia hinh. Lan dau chay thi lay
    /// tu chinh terrain roi luu ra file; nhung lan sau doc lai file.
    ///
    /// Khong luu thi chay lai lan hai se cong chong len ket qua lan mot, doi
    /// nui cao gap doi va do dac tut het xuong.
    /// </summary>
    static float[,] DocNenPhang(TerrainData td, float gocY, int N)
    {
        string duongThat = Path.Combine(Directory.GetCurrentDirectory(), DuongNenPhang);

        if (File.Exists(duongThat))
        {
            var byt = File.ReadAllBytes(duongThat);
            if (byt.Length == N * N * 4)
            {
                var kq = new float[N, N];
                int i = 0;
                for (int z = 0; z < N; z++)
                    for (int x = 0; x < N; x++, i += 4)
                        kq[z, x] = System.BitConverter.ToSingle(byt, i);
                Debug.Log("[Act2] Lay nen phang tu " + DuongNenPhang + " (chay lai, khong cong don).");
                return kq;
            }
            Debug.LogWarning("[Act2] " + DuongNenPhang + " sai kich thuoc - dung lai nen dang co.");
        }

        var h = td.GetHeights(0, 0, N, N);
        var nen = new float[N, N];
        var ra = new byte[N * N * 4];
        int j = 0;
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++, j += 4)
            {
                nen[z, x] = gocY + h[z, x] * td.size.y;
                System.BitConverter.GetBytes(nen[z, x]).CopyTo(ra, j);
            }

        File.WriteAllBytes(duongThat, ra);
        AssetDatabase.ImportAsset(DuongNenPhang, ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("[Act2] Da luu nen phang goc ra " + DuongNenPhang + " de con chay lai duoc.");
        return nen;
    }

    // ================================================================
    //  VUNG PHAI GIU
    // ================================================================

    static List<Vung> DocVungNuoc(GameObject world)
    {
        var kq = new List<Vung>();
        if (world == null) return kq;
        var cha = world.transform.Find("MatNuoc");
        if (cha == null) return kq;

        foreach (Transform c in cha)
        {
            var r = c.GetComponent<Renderer>();
            if (r == null) continue;
            kq.Add(new Vung
            {
                tam = new Vector2(r.bounds.center.x, r.bounds.center.z),
                banKinh = Mathf.Max(r.bounds.extents.x, r.bounds.extents.z)
            });
        }
        return kq;
    }

    static List<Vung> DocVungNhaMo(GameObject world)
    {
        var kq = new List<Vung>();
        if (world == null) return kq;
        var cha = world.transform.Find("NhaMo");
        if (cha == null) return kq;

        foreach (Transform c in cha)
        {
            var r = c.GetComponentInChildren<Renderer>();
            if (r == null) continue;
            // Ban kinh do bang MoGiu de KeoVeTam chia ra dung ti le giu/thoai
            kq.Add(new Vung
            {
                tam = new Vector2(r.bounds.center.x, r.bounds.center.z),
                banKinh = MoGiu
            });
        }
        return kq;
    }

    // ================================================================
    //  DO DAC
    // ================================================================

    /// <summary>
    /// Nhung gi phai di theo mat dat.
    ///
    /// MAT NUOC CO trong danh sach. Long chao duoc nang ha nguyen khoi (xem
    /// <see cref="KeoVeTam"/>) nen do dich o tam vung dung bang do dich o moi
    /// diem trong vung - mat nuoc di theo dung mot doan la mep nuoc va do sau
    /// luu trong mau dinh van khop y nhu cu.
    ///
    /// KHONG lay:
    ///   - MatDat_Terrain: chinh no
    ///   - HangRao: chan no chon san vao vanh dat, dat o do cung khong doi
    /// </summary>
    static List<Transform> GomDoDac(GameObject world)
    {
        var kq = new List<Transform>();
        if (world != null)
            foreach (var ten in new[] { "BiaMo", "Da", "Cay", "NhaMo", "MatNuoc" })
            {
                var cha = world.transform.Find(ten);
                if (cha == null) continue;
                foreach (Transform c in cha) kq.Add(c);
            }

        var buiCo = GameObject.Find("BuiCoRai");
        if (buiCo != null)
            foreach (Transform c in buiCo.transform) kq.Add(c);

        var p = GameObject.Find("Player");
        if (p != null) kq.Add(p.transform);

        return kq;
    }

    // ================================================================
    //  TO SOI DA RA SUON DOC
    // ================================================================

    /// <summary>
    /// TO LAI BON LOP VAT LIEU THEO DIA HINH MOI.
    ///
    /// Chi sua do cao thoi la CHUA DU. Do duoc tren anh chup cung mot goc may:
    /// sau khi dia hinh da khop Act1 ve hinh hoc (do lech chuan 0,45 m so voi
    /// 0,43 m; doc 4,2 do so voi 4,3 do), do tuong phan cua mat dat trong anh
    /// van chi 6,4 trong khi Act1 la 10,1.
    ///
    /// Ly do: duoi anh trang, mot suon doc 4 do chi doi duoc chung 3 muc xam -
    /// mat khong doc ra noi. Cai lam Act1 nhin ra dia hinh la VAT LIEU: no doi
    /// mau theo cho cao cho thap. Act2 thi 81% ban do la mot lop co chet duy
    /// nhat, nen go nao cung dung mot mau.
    ///
    /// Hai viec lam o day:
    ///   1. SUON DOC lo soi da ra (nguong 5 do chu khong phai 9 - dia hinh nay
    ///      doc trung binh co 4,2 do, de nguong 9 thi gan nhu khong cho nao to)
    ///   2. DINH GO tro rom kho sang, LONG TRUNG dong dat am toi. Do "cao hon
    ///      hay thap hon" so voi VUNG XUNG QUANH trong ban kinh 7 m, khong phai
    ///      so voi do cao tuyet doi - mot cho o lung chung suon doi van co the
    ///      la dinh go cua rieng no.
    ///
    /// CHAY LAI DUOC: net to goc duoc luu ra file lan dau, nhung lan sau doc
    /// lai tu do. Khong luu thi chay lai lan hai se dam them mot lop soi nua
    /// len ket qua lan mot.
    /// </summary>
    static int ToLaiTheoDiaHinh(TerrainData td)
    {
        int M = td.alphamapResolution;
        int soLop = td.alphamapLayers;
        if (soLop <= Act2Terrain.LopSoiDa) return 0;

        var a = DocToMauGoc(td, M, soLop);
        var chenh = ChenhSoVoiXungQuanh(td, 7f);
        int nCao = td.heightmapResolution;
        int dem = 0;

        for (int z = 0; z < M; z++)
        {
            float v = (float)z / (M - 1);
            for (int x = 0; x < M; x++)
            {
                float u = (float)x / (M - 1);

                // --- 1. Soi da tren suon doc ---
                float doc = td.GetSteepness(u, v) / 90f;
                float soi = Mathf.Clamp01((doc - 0.056f) * 5.5f);

                float con = 0f;
                for (int l = 0; l < soLop; l++)
                    if (l != Act2Terrain.LopSoiDa) con += a[z, x, l];

                if (con > 1e-4f)
                {
                    float ti = (1f - soi) / con;
                    for (int l = 0; l < soLop; l++)
                        if (l != Act2Terrain.LopSoiDa) a[z, x, l] *= ti;
                }
                else a[z, x, Act2Terrain.LopDatTroc] = 1f - soi;
                a[z, x, Act2Terrain.LopSoiDa] = soi;

                // --- 2. Dinh go kho sang, long trung am toi ---
                int hx = Mathf.Clamp(Mathf.RoundToInt(u * (nCao - 1)), 0, nCao - 1);
                int hz = Mathf.Clamp(Mathf.RoundToInt(v * (nCao - 1)), 0, nCao - 1);
                float k = Mathf.Clamp(chenh[hz, hx] / 0.40f, -1f, 1f);

                // Chi DOI CHO nhau giua hai lop, giu nguyen tong cua chung: net
                // to san (vung dat troc quanh 452 ngoi mo, lop bun cua dam lay)
                // khong bi ve de len.
                float tongCT = a[z, x, Act2Terrain.LopCoChet] + a[z, x, Act2Terrain.LopDatTroc];
                if (tongCT > 1e-4f)
                {
                    float pCo = a[z, x, Act2Terrain.LopCoChet] / tongCT;
                    pCo = Mathf.Clamp01(pCo + k * 0.34f);
                    a[z, x, Act2Terrain.LopCoChet] = tongCT * pCo;
                    a[z, x, Act2Terrain.LopDatTroc] = tongCT * (1f - pCo);
                }
                dem++;
            }
        }

        td.SetAlphamaps(0, 0, a);
        return dem;
    }

    /// <summary>
    /// Cho nay cao hon hay thap hon VUNG XUNG QUANH bao nhieu met.
    ///
    /// Lam mo do cao bang mot cua so vuong ban kinh <paramref name="banKinh"/>
    /// met roi lay do cao that tru di. Phai lam hai luot mot chieu (ngang roi
    /// doc) - lam thang bang cua so vuong tren luoi 513x513 voi ban kinh 33 o
    /// la mot ty phep cong.
    /// </summary>
    static float[,] ChenhSoVoiXungQuanh(TerrainData td, float banKinh)
    {
        int N = td.heightmapResolution;
        var h = td.GetHeights(0, 0, N, N);
        float buoc = td.size.x / (N - 1);
        int r = Mathf.Max(1, Mathf.RoundToInt(banKinh / buoc));

        var tam = new float[N, N];
        var mo = new float[N, N];

        for (int z = 0; z < N; z++)
        {
            float tong = 0f;
            for (int x = -r; x <= r; x++) tong += h[z, Mathf.Clamp(x, 0, N - 1)];
            for (int x = 0; x < N; x++)
            {
                tam[z, x] = tong / (2 * r + 1);
                tong += h[z, Mathf.Clamp(x + r + 1, 0, N - 1)]
                      - h[z, Mathf.Clamp(x - r, 0, N - 1)];
            }
        }

        for (int x = 0; x < N; x++)
        {
            float tong = 0f;
            for (int z = -r; z <= r; z++) tong += tam[Mathf.Clamp(z, 0, N - 1), x];
            for (int z = 0; z < N; z++)
            {
                mo[z, x] = tong / (2 * r + 1);
                tong += tam[Mathf.Clamp(z + r + 1, 0, N - 1), x]
                      - tam[Mathf.Clamp(z - r, 0, N - 1), x];
            }
        }

        var kq = new float[N, N];
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
                kq[z, x] = (h[z, x] - mo[z, x]) * td.size.y;
        return kq;
    }

    /// <summary>Net to bon lop TRUOC KHI sua. Luu ra file lan dau, sau doc lai.</summary>
    static float[,,] DocToMauGoc(TerrainData td, int M, int soLop)
    {
        string duongThat = Path.Combine(Directory.GetCurrentDirectory(), DuongToMauGoc);
        int can = M * M * soLop;

        if (File.Exists(duongThat))
        {
            var byt = File.ReadAllBytes(duongThat);
            if (byt.Length == can)
            {
                var kq = new float[M, M, soLop];
                int i = 0;
                for (int z = 0; z < M; z++)
                    for (int x = 0; x < M; x++)
                        for (int l = 0; l < soLop; l++, i++)
                            kq[z, x, l] = byt[i] / 255f;
                return kq;
            }
            Debug.LogWarning("[Act2] " + DuongToMauGoc + " sai kich thuoc - dung net to dang co.");
        }

        var a = td.GetAlphamaps(0, 0, M, M);
        var ra = new byte[can];
        int j = 0;
        for (int z = 0; z < M; z++)
            for (int x = 0; x < M; x++)
                for (int l = 0; l < soLop; l++, j++)
                    ra[j] = (byte)Mathf.RoundToInt(Mathf.Clamp01(a[z, x, l]) * 255f);

        File.WriteAllBytes(duongThat, ra);
        AssetDatabase.ImportAsset(DuongToMauGoc, ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("[Act2] Da luu net to goc ra " + DuongToMauGoc + ".");
        return a;
    }

    // ================================================================
    //  DO LAI DE DOI CHIEU VOI ACT1
    // ================================================================

    static void Do(Terrain terr, float banKinh)
    {
        var td = terr.terrainData;
        int N = td.heightmapResolution;
        var h = td.GetHeights(0, 0, N, N);
        float nua = td.size.x * 0.5f;
        float buoc = td.size.x / (N - 1);

        float lo = 9e9f, hi = -9e9f, tong = 0f; int dem = 0;
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float px = x * buoc - nua, pz = z * buoc - nua;
                if (px * px + pz * pz > banKinh * banKinh) continue;
                float y = h[z, x] * td.size.y;
                if (y < lo) lo = y; if (y > hi) hi = y;
                tong += y; dem++;
            }

        float tb = tong / dem;
        float s2 = 0f, docTong = 0f, docMax = 0f; int demD = 0;
        int b = Mathf.Max(1, Mathf.RoundToInt(2f / buoc));

        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float px = x * buoc - nua, pz = z * buoc - nua;
                if (px * px + pz * pz > banKinh * banKinh) continue;
                float y = h[z, x] * td.size.y;
                s2 += (y - tb) * (y - tb);
                if (x + b < N && z + b < N)
                {
                    float dx = Mathf.Abs(h[z, x + b] - h[z, x]) * td.size.y;
                    float dz = Mathf.Abs(h[z + b, x] - h[z, x]) * td.size.y;
                    float d = Mathf.Max(dx, dz) / (b * buoc);
                    docTong += d; if (d > docMax) docMax = d; demD++;
                }
            }

        Debug.Log(string.Format(
            "[Act2] loi r={0:F0} m: chenh cao {1:F2} m, do lech chuan {2:F2} m, doc tb {3:F1} do, doc max {4:F1} do"
          + "   (Act1: r30 -> 2,35 m / 0,43 m / 4,3 do / 12,6 do;  r45 -> 5,08 m / 1,06 m / 7,7 do / 21,4 do)",
            banKinh, hi - lo, Mathf.Sqrt(s2 / dem),
            Mathf.Atan(docTong / demD) * Mathf.Rad2Deg,
            Mathf.Atan(docMax) * Mathf.Rad2Deg));
    }
}
