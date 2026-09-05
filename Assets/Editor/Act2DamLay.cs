using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// DAO 10 BAI DAM LAY VA MOT HO NUOC O TRUNG TAM cho ban do Act2.
///
/// Chay SAU khi <see cref="Act2Terrain"/> da dung xong dia hinh, va sau khi da
/// dat het do dac - vi no can biet cay va nha mo dang o dau de khoi dao vao.
///
/// BA VIEC PHAI LAM CUNG NHAU, thieu mot cai la lo:
///   1. DAO   - ha do cao terrain, mep thoai dan ra
///   2. TO    - quet lop bun vao chinh vung vua dao
///   3. HA DO - bia mo va da nam trong vung dao phai tut xuong theo, khong thi
///              chung treo lo lung giua khong khi
///
/// VE MUC NUOC: mat nuoc dat THAP HON mep bo, nen nhin tu tren xuong thay dung
/// mot vung nuoc nam gon trong long chao chu khong phai mot tam kinh phu len
/// ca vung. Nguoi choi loi qua duoc - khong co va cham nao tren mat nuoc, day
/// chao van la terrain collider nhu cu.
/// </summary>
public static class Act2DamLay
{
    // ================================================================
    //  SO DO
    // ================================================================

    public const int SoDamLay = 10;

    /// <summary>Ho o trung tam: ban kinh va do sau, tinh bang met.</summary>
    const float BanKinhHo = 5.4f;
    const float SauHo = 0.92f;

    /// <summary>Dam lay: ban kinh va do sau boc ngau nhien trong khoang nay.</summary>
    const float DamMin = 4.2f, DamMax = 7.6f;
    const float SauDamMin = 0.26f, SauDamMax = 0.44f;

    /// <summary>Muc nuoc dong trong long chao, tinh tu day len.</summary>
    const float NuocTrongHo = 0.74f;
    const float NuocTrongDam = 0.20f;

    /// <summary>Khong dao vao goc cay hay nha mo - chung khong tut xuong duoc.</summary>
    const float TranhCay = 7.0f;
    const float TranhNhaMo = 9.0f;

    /// <summary>Dam lay phai cach nhau va cach ho, khong thi chung dinh thanh mang.</summary>
    const float CachNhau = 15.0f;

    /// <summary>Chua ban do thi moi lan chay lai ra y het mot ket qua.</summary>
    const int Hat = 20260823;

    const string ThuMuc = "Assets/Terrain";

    // ================================================================
    //  BO NUOC DONG
    // ================================================================
    //
    // Mep nuoc khong duoc dung yen, khong thi quanh vung nuoc luon co mot vong
    // vien sang dung im nhu net ke bang but. Cach cho no dong:
    //
    //   - Luoi nuoc CAT RONG HON bo tinh, ra toi cho dat cao hon mat nuoc
    //     <see cref="TranToiDa"/> met.
    //   - Do sau luu trong mau dinh CO CA SO AM (cho dat cao hon mat nuoc).
    //     Mau dinh chi chua duoc 0..1 nen phai day len <see cref="LechAm"/> va
    //     chia cho <see cref="ThangSau"/>; shader tru va nhan lai.
    //   - Shader cong song vao do sau do. Song len thi cho dat hoi cao cung
    //     thanh co nuoc (tran ra), song rut thi lui vao.
    //
    // BA SO NAY PHAI KHOP VOI SHADER (_LechAm, _ThangSau). Lech la ca duong bo
    // xe dich di mot doan.

    /// <summary>Nuoc tran len qua bo tinh toi da bao nhieu met.</summary>
    const float TranToiDa = 0.14f;

    /// <summary>Mau dinh luu duoc do sau am toi bao nhieu met.</summary>
    const float LechAm = 0.25f;

    /// <summary>Thang do sau nhet vao mau dinh 0..1.</summary>
    const float ThangSau = 1.5f;

    // ================================================================

    /// <summary>Mot vung trung: tam, ban kinh, do sau, muc nuoc.</summary>
    class Vung
    {
        public Vector2 tam;          // toa do the gioi X/Z
        public float banKinh;
        public float sau;
        public float nuoc;           // tinh tu day chao len
        public float lechGoc;        // moi vung meo mot kieu
        public bool laHo;
    }

    // ================================================================
    //  DIEM VAO
    // ================================================================

    public static void Dao(Terrain terr, Transform world)
    {
        if (terr == null) { Debug.LogError("[Act2] Khong co Terrain de dao dam lay."); return; }

        // CHAY HAI LAN LA HONG: NoiChoDao noi size.y them mot lan nua, roi dao
        // chong len ho cu thanh sau gap doi. Co "MatNuoc" tuc la da dao roi.
        if (world != null && world.Find("MatNuoc") != null)
        {
            Debug.LogWarning("[Act2] Ban do nay da co dam lay roi - bo qua. "
                           + "Muon dao lai thi chay lai muc 9 cho no dung tu dau.");
            return;
        }

        var vung = ChonCho(world);
        if (vung.Count == 0) { Debug.LogWarning("[Act2] Khong tim duoc cho dao."); return; }

        NoiChoDao(terr);
        HaDoCao(terr, vung);
        ToLopBun(terr, vung);
        HaDoDac(terr, world, vung);
        DungMatNuoc(terr, world, vung);
        BatCameraDocDoSau();

        int soDam = vung.Count - 1;
        Debug.Log("[Act2] Da dao 1 ho nuoc trung tam (ban kinh " + BanKinhHo.ToString("F1")
                + " m, sau " + SauHo.ToString("F2") + " m) va " + soDam + " bai dam lay.");
    }

    // ================================================================
    //  1. CHON CHO
    // ================================================================

    /// <summary>
    /// Ho luon nam dung trung tam. Muoi bai dam lay rai quanh, tranh goc cay va
    /// nha mo, tranh de len nhau.
    ///
    /// KHONG tranh bia mo va da: nghia dia ngap nuoc, bia mo chim nua than trong
    /// bun la dung khong khi. Chung se duoc ha xuong o buoc <see cref="HaDoDac"/>.
    /// </summary>
    static List<Vung> ChonCho(Transform world)
    {
        var ds = new List<Vung>();

        ds.Add(new Vung
        {
            tam = Vector2.zero,
            banKinh = BanKinhHo,
            sau = SauHo,
            nuoc = NuocTrongHo,
            lechGoc = 0.3f,
            laHo = true
        });

        // Goc cay va nha mo - hai thu khong tut xuong theo dia hinh duoc
        var camDao = new List<Vector3>();
        if (world != null)
        {
            foreach (var mf in world.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                string n = mf.sharedMesh.name;
                bool laCay = n.Contains("TREE");
                bool laNhaMo = n.Contains("CRYPT") || n.Contains("MAUSOLEUM") || n.Contains("TOMB");
                if (!laCay && !laNhaMo) continue;

                Vector3 p = mf.transform.position;
                camDao.Add(new Vector3(p.x, laNhaMo ? TranhNhaMo : TranhCay, p.z));
            }
        }

        var rnd = new System.Random(Hat);
        const float MepBanDo = 46f;      // chua toi hang rao

        for (int lan = 0; lan < 4000 && ds.Count <= SoDamLay; lan++)
        {
            float x = (float)(rnd.NextDouble() * 2 - 1) * MepBanDo;
            float z = (float)(rnd.NextDouble() * 2 - 1) * MepBanDo;
            var tam = new Vector2(x, z);

            float bk = Mathf.Lerp(DamMin, DamMax, (float)rnd.NextDouble());

            // Khong de len vung da chon
            bool dung = false;
            foreach (var v in ds)
                if (Vector2.Distance(v.tam, tam) < CachNhau + bk * 0.35f) { dung = true; break; }
            if (dung) continue;

            // Khong dao vao goc cay hay nha mo
            foreach (var c in camDao)
            {
                float kc = Vector2.Distance(new Vector2(c.x, c.z), tam);
                if (kc < c.y + bk * 0.5f) { dung = true; break; }
            }
            if (dung) continue;

            ds.Add(new Vung
            {
                tam = tam,
                banKinh = bk,
                sau = Mathf.Lerp(SauDamMin, SauDamMax, (float)rnd.NextDouble()),
                nuoc = NuocTrongDam,
                lechGoc = (float)rnd.NextDouble() * 10f,
                laHo = false
            });
        }

        return ds;
    }

    // ================================================================
    //  2. NOI CHO DAO
    // ================================================================

    /// <summary>
    /// Dia hinh hien tai da cham DAY tuyet doi (diem thap nhat = 0.0005), tuc la
    /// KHONG CON CHO DAO XUONG. Phai noi <c>size.y</c> ra roi ha ca terrain
    /// xuong dung bang phan vua noi, de BE MAT KHONG XE DICH mot ly nao.
    ///
    /// Diem the gioi cu:  y = viTri.y + h * cao
    /// Sau khi noi them d: y = (viTri.y - d) + h' * (cao + d),  h' = (h*cao + d)/(cao+d)
    ///                       = viTri.y - d + h*cao + d = viTri.y + h*cao   ✓ y het cu
    /// </summary>
    static void NoiChoDao(Terrain terr)
    {
        var d = terr.terrainData;
        float caoCu = d.size.y;
        float them = SauHo + 0.35f;          // du cho ho sau nhat, con du mot chut

        int N = d.heightmapResolution;
        var h = d.GetHeights(0, 0, N, N);

        float caoMoi = caoCu + them;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
                h[y, x] = (h[y, x] * caoCu + them) / caoMoi;

        d.size = new Vector3(d.size.x, caoMoi, d.size.z);
        d.SetHeights(0, 0, h);

        var p = terr.transform.position;
        terr.transform.position = new Vector3(p.x, p.y - them, p.z);
    }

    // ================================================================
    //  3. DAO
    // ================================================================

    /// <summary>
    /// Ban kinh THAT tai mot goc. Vung tron deo tam thi nhin ra cai chau, phai
    /// meo di moi ra vung nuoc dong tu nhien.
    ///
    /// Ham nay duoc dung CHUNG cho ca luc dao lan luc dung mat nuoc, nen mep
    /// nuoc bam dung mep chao - lech ham la nuoc tran ra ngoai bo.
    /// </summary>
    static float BanKinhTaiGoc(Vung v, float goc)
    {
        float a = Mathf.Sin(goc * 3f + v.lechGoc) * 0.5f
                + Mathf.Sin(goc * 5f - v.lechGoc * 1.7f) * 0.3f
                + Mathf.Sin(goc * 2f + v.lechGoc * 0.6f) * 0.2f;
        return v.banKinh * (1f + a * 0.22f);
    }

    /// <summary>Chim bao sau tai mot diem: 1 o giua, 0 ngoai mep.</summary>
    static float DoChim(Vung v, float wx, float wz)
    {
        float dx = wx - v.tam.x, dz = wz - v.tam.y;
        float kc = Mathf.Sqrt(dx * dx + dz * dz);
        if (kc < 0.0001f) return 1f;

        float bk = BanKinhTaiGoc(v, Mathf.Atan2(dz, dx));
        if (kc >= bk) return 0f;

        // Mep THOAI, khong dung thanh. Long chao gan phang o giua.
        float u = kc / bk;
        return Mathf.SmoothStep(1f, 0f, Mathf.SmoothStep(0f, 1f, u));
    }

    static void HaDoCao(Terrain terr, List<Vung> vung)
    {
        var d = terr.terrainData;
        int N = d.heightmapResolution;
        var h = d.GetHeights(0, 0, N, N);

        float goc = terr.transform.position.x;      // canh terrain trong the gioi
        float gocZ = terr.transform.position.z;
        float buoc = d.size.x / (N - 1);
        float cao = d.size.y;

        foreach (var v in vung)
        {
            // Chi quet o vuong bao quanh vung, khoi duyet ca 513x513 muoi mot lan
            float bkMax = v.banKinh * 1.3f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt((v.tam.x - bkMax - goc) / buoc));
            int x1 = Mathf.Min(N - 1, Mathf.CeilToInt((v.tam.x + bkMax - goc) / buoc));
            int z0 = Mathf.Max(0, Mathf.FloorToInt((v.tam.y - bkMax - gocZ) / buoc));
            int z1 = Mathf.Min(N - 1, Mathf.CeilToInt((v.tam.y + bkMax - gocZ) / buoc));

            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    float chim = DoChim(v, goc + x * buoc, gocZ + z * buoc);
                    if (chim <= 0f) continue;
                    h[z, x] = Mathf.Max(0f, h[z, x] - chim * v.sau / cao);
                }
        }

        d.SetHeights(0, 0, h);
    }

    // ================================================================
    //  4. TO LOP BUN
    // ================================================================

    static void ToLopBun(Terrain terr, List<Vung> vung)
    {
        var d = terr.terrainData;
        int M = d.alphamapResolution;
        int soLop = d.alphamapLayers;
        var a = d.GetAlphamaps(0, 0, M, M);

        float goc = terr.transform.position.x;
        float gocZ = terr.transform.position.z;
        float buoc = d.size.x / (M - 1);

        foreach (var v in vung)
        {
            float bkMax = v.banKinh * 1.35f;
            int x0 = Mathf.Max(0, Mathf.FloorToInt((v.tam.x - bkMax - goc) / buoc));
            int x1 = Mathf.Min(M - 1, Mathf.CeilToInt((v.tam.x + bkMax - goc) / buoc));
            int z0 = Mathf.Max(0, Mathf.FloorToInt((v.tam.y - bkMax - gocZ) / buoc));
            int z1 = Mathf.Min(M - 1, Mathf.CeilToInt((v.tam.y + bkMax - gocZ) / buoc));

            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    float chim = DoChim(v, goc + x * buoc, gocZ + z * buoc);
                    if (chim <= 0f) continue;

                    // Toi tan MEP moi ra bun - vung trung uot lan ra ngoai bo mot chut
                    float bun = Mathf.Clamp01(chim * 2.4f);

                    for (int l = 0; l < soLop; l++)
                        a[z, x, l] *= (1f - bun);
                    if (Act2Terrain.LopBun < soLop)
                        a[z, x, Act2Terrain.LopBun] += bun;
                }
        }

        d.SetAlphamaps(0, 0, a);
    }

    // ================================================================
    //  5. HA DO DAC XUONG THEO
    // ================================================================

    /// <summary>
    /// Bia mo va da nam trong vung vua dao phai tut xuong theo, khong thi chung
    /// treo lo lung tren mieng ho. Chi ha dung bang phan dat da bi lay di ngay
    /// duoi chan no.
    /// </summary>
    static void HaDoDac(Terrain terr, Transform world, List<Vung> vung)
    {
        if (world == null) return;

        int daHa = 0;
        foreach (var mf in world.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            string n = mf.sharedMesh.name;
            if (n.Contains("TREE") || n.Contains("FENCE")) continue;   // da tranh tu dau

            Vector3 p = mf.transform.position;

            float tut = 0f;
            foreach (var v in vung)
                tut = Mathf.Max(tut, DoChim(v, p.x, p.z) * v.sau);

            if (tut <= 0.001f) continue;
            mf.transform.position = new Vector3(p.x, p.y - tut, p.z);
            daHa++;
        }

        Debug.Log("[Act2] Da ha " + daHa + " vat the xuong theo long dam lay.");
    }

    // ================================================================
    //  6. MAT NUOC
    // ================================================================

    static void DungMatNuoc(Terrain terr, Transform world, List<Vung> vung)
    {
        var vatLieu = LamVatLieuNuoc();
        var cha = new GameObject("MatNuoc").transform;
        cha.SetParent(world, false);

        for (int i = 0; i < vung.Count; i++)
        {
            var v = vung[i];

            // Mat nuoc nam thap hon mep bo, nen no nam GON trong long chao
            float dayY = terr.SampleHeight(new Vector3(v.tam.x, 0f, v.tam.y))
                       + terr.transform.position.y;
            float mucNuoc = dayY + v.nuoc;

            var go = new GameObject(v.laHo ? "HoNuoc_TrungTam" : "DamLay_" + i);
            go.transform.SetParent(cha, false);
            go.transform.position = new Vector3(v.tam.x, mucNuoc, v.tam.y);

            // HAI MANG BO KHAC NHAU, co chu y:
            //   boVe  - rong hon, de luoi tho ra ngoai lay cho cho song tran
            //   boLoi - bo TINH that su, dung cho VungNuoc; lay boVe thi nguoi
            //           choi se "loi nuoc" o ca nhung cho dang kho chan
            var boVe = DoBoNuoc(v, terr, mucNuoc + TranToiDa);
            var boLoi = DoBoNuoc(v, terr, mucNuoc);

            go.AddComponent<MeshFilter>().sharedMesh = LamDia(v, i, terr, mucNuoc, boVe);
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = vatLieu;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;

            // KHONG co collider: nguoi choi phai loi qua duoc, day chao van la
            // terrain collider nhu cu. Thay vao do gan VungNuoc de nguoi choi va
            // quai hoi duoc "dung o day thi ngap sau bao nhieu" - xem LoiNuoc.
            var vn = go.AddComponent<VungNuoc>();
            vn.mucNuoc = mucNuoc;
            vn.banKinhBo = boLoi;
            vn.laHo = v.laHo;
        }
    }

    /// <summary>
    /// Do duong bo nuoc theo tung huong.
    ///
    /// Muc nuoc nam THAP HON mep chao, nen bo that luon nam ben trong mep chao.
    /// Do bang <c>SampleHeight</c> tren terrain DA DAO XONG nen khong phu thuoc
    /// vao cong thuc dao, sai lech bao nhieu cung tu khop.
    ///
    /// Mang tra ve duoc dung CHUNG cho ca luoi ve va <see cref="VungNuoc"/> -
    /// hai cai lech nhau thi nguoi choi loi o cho khong co nuoc, hoac di kho
    /// chan ngay giua ho.
    /// </summary>
    static float[] DoBoNuoc(Vung v, Terrain terr, float mucDo)
    {
        const int Huong = 80;
        float gocY = terr.transform.position.y;
        float bkTran = v.banKinh * 1.45f;

        var bo = new float[Huong];
        for (int i = 0; i < Huong; i++)
        {
            float a = i / (float)Huong * Mathf.PI * 2f;
            float cx = Mathf.Cos(a), cz = Mathf.Sin(a);

            float r = 0f;
            for (float thu = 0.10f; thu <= bkTran; thu += 0.10f)
            {
                float day = terr.SampleHeight(
                    new Vector3(v.tam.x + cx * thu, 0f, v.tam.y + cz * thu)) + gocY;
                if (day >= mucDo) break;        // da len qua muc do -> dung o day
                r = thu;
            }
            bo[i] = Mathf.Max(r, 0.4f);
        }
        return bo;
    }

    /// <summary>
    /// Dia mat nuoc, cat dung theo DUONG BO THAT.
    ///
    /// Khong cat theo ban kinh hinh hoc: muc nuoc nam THAP HON mep chao, nen bo
    /// nuoc that luon nam ben trong mep chao. Cat theo ban kinh thi ria nuoc tho
    /// ra ngoai, nhin nhu mot tam kinh dat de len mat dat.
    ///
    /// Nen voi tung huong, do dan ra tu tam cho toi khi day cham muc nuoc - do
    /// chinh la bo. Do bang <c>SampleHeight</c> tren terrain DA DAO XONG nen
    /// khong phu thuoc vao cong thuc dao, sai lech bao nhieu cung tu khop.
    ///
    /// DO SAU NUOC DUOC NUONG VAO MAU DINH (kenh do), tinh bang met. Shader doc
    /// no de biet cho nao lo day bun cho nao duc han - khong can bo dem do sau
    /// cua camera, thu ma Built-in RP khong tu bat.
    /// </summary>
    static Mesh LamDia(Vung v, int soThuTu, Terrain terr, float mucNuoc, float[] bo)
    {
        const int Vong = 9;        // nhieu vong thi song va do sau moi muot

        float gocY = terr.transform.position.y;
        int Huong = bo.Length;

        // ---- Dung luoi ----
        var dinh = new List<Vector3>();
        var phap = new List<Vector3>();
        var mau = new List<Color>();
        var tam = new List<int>();

        float sauTam = mucNuoc
            - (terr.SampleHeight(new Vector3(v.tam.x, 0f, v.tam.y)) + gocY);

        dinh.Add(Vector3.zero);
        phap.Add(Vector3.up);
        mau.Add(new Color(NhetSau(sauTam), 0f, 0f, 1f));

        for (int j = 1; j <= Vong; j++)
        {
            float k = j / (float)Vong;
            for (int i = 0; i < Huong; i++)
            {
                float a = i / (float)Huong * Mathf.PI * 2f;
                float r = bo[i] * k;
                float dx = Mathf.Cos(a) * r, dz = Mathf.Sin(a) * r;

                float day = terr.SampleHeight(new Vector3(v.tam.x + dx, 0f, v.tam.y + dz)) + gocY;

                // KHONG cat ve 0. Cho dat cao hon mat nuoc thi do sau AM, va
                // chinh cho am do moi la vung ma song se tran vao rut ra.
                float sau = mucNuoc - day;

                dinh.Add(new Vector3(dx, 0f, dz));
                phap.Add(Vector3.up);
                mau.Add(new Color(NhetSau(sau), 0f, 0f, 1f));
            }
        }

        // THU TU DINH PHAI NGUOC CHIEU GOC TANG DAN.
        //
        // Dinh thu i nam o goc a = i/Huong*2pi, tuc (cos a, 0, sin a). Nhin tu
        // tren xuong (truc X sang phai, Z len tren), i tang dan la di NGUOC chieu
        // kim dong ho - ma Unity coi tam giac nguoc chieu kim dong ho la mat quay
        // ra SAU. Ket qua: ca mat nuoc quay mat xuong dat va bi culling xoa sach.
        //
        // Do duoc: giu thu tu cu thi dap vat lieu do dac vao van dem duoc DUNG 0
        // diem do tren anh. Dao lai la ra.
        for (int i = 0; i < Huong; i++)
        {
            tam.Add(0);
            tam.Add(1 + (i + 1) % Huong);
            tam.Add(1 + i);
        }
        for (int j = 1; j < Vong; j++)
        {
            int trong = 1 + (j - 1) * Huong;
            int ngoai = 1 + j * Huong;
            for (int i = 0; i < Huong; i++)
            {
                int i2 = (i + 1) % Huong;
                tam.Add(trong + i); tam.Add(ngoai + i); tam.Add(ngoai + i2);
                tam.Add(trong + i); tam.Add(ngoai + i2); tam.Add(trong + i2);
            }
        }

        // ---- TU KIEM HUONG MAT, dung tin vao suy luan ----
        //
        // Toi da dao nham winding hai lan lien tiep o day: lan dau ca dia quay
        // mat xuong dat (do duoc DUNG 0 diem mau do tren anh), lan sau sua thi
        // quat giua dung ma bon vanh ngoai lai sai (90 diem - vua bang dien tich
        // rieng cai quat giua). Nen thoi khong suy luan nua: tinh thang phap
        // tuyen hinh hoc cua tung tam giac, cai nao quay xuong thi dao lai.
        for (int i = 0; i + 2 < tam.Count; i += 3)
        {
            Vector3 A = dinh[tam[i]], B = dinh[tam[i + 1]], C = dinh[tam[i + 2]];
            if (Vector3.Cross(B - A, C - A).y < 0f)
            {
                int giu = tam[i + 1];
                tam[i + 1] = tam[i + 2];
                tam[i + 2] = giu;
            }
        }

        // LUOI PHAI THANH FILE TAI SAN. Luoi tao bang code ma chi nam trong bo
        // nho thi luu canh xong, mo lai la MeshFilter tro vao khoang khong -
        // mat nuoc bien mat sach.
        //
        // Va ghi THANG vao file cu chu khong xoa roi tao lai: xoa la doi ma GUID,
        // canh dang tro vao no se dut tham chieu.
        if (!AssetDatabase.IsValidFolder(ThuMuc))
            AssetDatabase.CreateFolder("Assets", "Terrain");

        string ten = v.laHo ? "Act2_LuoiHoNuoc" : "Act2_LuoiDamLay_" + soThuTu;
        string duong = ThuMuc + "/" + ten + ".asset";

        bool laMoi = false;
        var m = AssetDatabase.LoadAssetAtPath<Mesh>(duong);
        if (m == null) { m = new Mesh(); laMoi = true; }

        m.Clear();
        m.name = ten;
        m.SetVertices(dinh);
        m.SetNormals(phap);
        m.SetColors(mau);
        m.SetTriangles(tam, 0);
        m.RecalculateBounds();
        m.UploadMeshData(false);

        if (laMoi) AssetDatabase.CreateAsset(m, duong);
        else EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();

        return m;
    }

    /// <summary>
    /// Nhet do sau (met, co the am) vao mot kenh mau 0..1.
    ///
    /// Shader lam dieu nguoc lai: <c>doSau = color.r * _ThangSau - _LechAm</c>.
    /// </summary>
    static float NhetSau(float sauMet)
    {
        return Mathf.Clamp01((sauMet + LechAm) / ThangSau);
    }

    static Material LamVatLieuNuoc()
    {
        var sh = Shader.Find("Diablo25D/NuocDam");
        if (sh == null)
        {
            Debug.LogError("[Act2] Khong tim thay shader Diablo25D/NuocDam");
            return null;
        }

        string duong = ThuMuc + "/Act2_NuocDam.mat";
        var cu = AssetDatabase.LoadAssetAtPath<Material>(duong);
        var m = cu != null ? cu : new Material(sh);
        m.shader = sh;
        m.name = "Act2_NuocDam";

        m.SetColor("_Color", new Color(0.150f, 0.190f, 0.160f));
        m.SetColor("_ColorSau", new Color(0.042f, 0.070f, 0.068f));
        m.SetColor("_MepColor", new Color(0.40f, 0.48f, 0.44f));
        m.SetFloat("_DoSau", 0.85f);
        m.SetFloat("_TiLeGon", 0.75f);
        m.SetFloat("_TocDoGon", 0.035f);
        m.SetFloat("_DoGon", 0.55f);

        // Song: bien do 8,5 cm. Do lai tai mot diem bat ky thi song chi len xuong
        // khoang +-2 cm - ba nhip sin triet tieu bot nhau nen bien do thuc bao gio
        // cung nho hon con so nay. Cao hon nua thi mep nuoc bat dau nhap nho qua
        // bo dat, du da co TatOMep keo lai.
        m.SetFloat("_CaoSong", 0.085f);
        m.SetFloat("_NhipSong", 1.15f);
        m.SetFloat("_TocSong", 1.30f);

        // BOT PHAI MONG. Long chao thoai nen vung nong chiem dien tich rat rong;
        // de _BotDay 0,30 m thi bot lan gan het mat ho, nhin ra bat sua chu khong
        // ra nuoc. 0,14 m thi bot chi con vien sat mep, dung cho no can.
        m.SetColor("_BotColor", new Color(0.80f, 0.85f, 0.82f));
        m.SetFloat("_BotDay", 0.14f);
        m.SetFloat("_BotManh", 0.45f);

        // PHAI khop voi LechAm / ThangSau o tren
        m.SetFloat("_LechAm", LechAm);
        m.SetFloat("_ThangSau", ThangSau);
        m.SetFloat("_MemBo", 0.045f);

        // Nhip CAO thi dom nho va day; nhip thap thi dom to, dinh lai thanh
        // mang trang loa tren mat nuoc.
        m.SetFloat("_OngAnh", 0.85f);
        m.SetFloat("_OngAnhNhip", 13f);

        m.SetFloat("_DoBong", 0.82f);
        m.SetFloat("_VienManh", 3.2f);

        if (cu == null)
        {
            if (!AssetDatabase.IsValidFolder(ThuMuc))
                AssetDatabase.CreateFolder("Assets", "Terrain");
            AssetDatabase.CreateAsset(m, duong);
        }
        else EditorUtility.SetDirty(m);

        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<Material>(duong);
    }

    /// <summary>
    /// Shader nuoc doc bo dem do sau de biet cho nao nong cho nao sau. Trong
    /// Built-in RP, camera KHONG tu sinh ra bo dem do - phai bao no ghi. Thieu
    /// buoc nay thi mat nuoc duc deu mot mau, mat het phan nong dan o mep bo.
    /// </summary>
    static void BatCameraDocDoSau()
    {
        foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            c.depthTextureMode |= DepthTextureMode.Depth;
    }
}
