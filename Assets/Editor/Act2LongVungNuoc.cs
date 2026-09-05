using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// DAT CAC VUNG NUOC ACT2 VAO DUNG LONG CHAO CUA CHUNG.
///
/// Hai viec, va PHAI theo thu tu nay:
///
///   1. HA MAT NUOC xuong duoi duong bo, cho mep luoi chim han vao dat
///   2. KHOET LONG ben trong duong bo cho nuoc con do sau
///
/// Nguoc thu tu la hong: khoet dua theo cao do mat nuoc, ma cao do mat nuoc lai
/// dua theo dat o duong bo.
///
/// ===================================================================
///  BAN TRUOC SAI O DAU
/// ===================================================================
///
/// Ban truoc chi lam viec 2, va con khoet ca <b>duong bo</b> xuong duoi mat
/// nuoc 25 cm (<c>SauMep</c>) - de chac chan het z-fighting o vanh. Ket qua:
/// <b>ca 11/11 vung nuoc deu co 100% so dinh nam TREN mat dat</b>, trung binh
/// cao 0,32 m, cao nhat 0,87 m.
///
/// Nguoi choi mo ta dung mot cau: "vung nuoc dang bi lo lung o tren khong
/// trung". Het z-fighting that, nhung doi lay mot cai dia nuoc bay giua nghia
/// dia - te hon han cai loi ban dau.
///
/// Bai hoc: chua mot be mat khoi z-fighting thi phai hoi <b>no se nam o dau sau
/// khi chua</b>, chu khong chi hoi "hai mat da du xa nhau chua".
///
/// ===================================================================
///  HINH DANG SAU KHI SUA
/// ===================================================================
///
/// <code>
///   d = 0        tam vung, dat thap hon nuoc SauTam (0,55 m)
///   d -> 0,85    dat nong dan len, vua bang mat nuoc dung o 0,85
///   d = 0,85..1  DUONG BO - khong dong den. Mep luoi nuoc chim vao day.
///   d > 1        dia hinh tu nhien, khong dong den
/// </code>
///
/// Ban kinh <c>d</c> do tu <b>dinh luoi</b> chu khong tu hop bao: hop bao la
/// mot cai hop VUONG om lay mat nuoc TRON, goc hop thua ra 41%.
///
/// Chi HA dat, khong bao gio nang - nang len thi bia mo quanh bo bi chon.
/// Chay lai bao nhieu lan cung ra dung mot ket qua, vi vung khoet dung lai
/// truoc duong bo nen cai moc do (dat o bo) khong bao gio bi doi.
/// </summary>
public static class Act2LongVungNuoc
{
    const string DuongScene = "Assets/Scenes/Act2.unity";
    const string DuongSaoLuu = "Assets/Editor/Act2_DoCaoTruocKhiKhoet.bytes";

    /// <summary>Long vung sau bao nhieu met so voi mat nuoc, do tai TAM.</summary>
    const float SauTam = 0.55f;

    /// <summary>
    /// Chi khoet trong pham vi nay cua ban kinh mat nuoc; tu day tro ra KHONG
    /// dong den dia hinh.
    ///
    /// Vanh ngoai chinh la <b>duong bo</b>: mep luoi nuoc phai chim vao day.
    /// Khoet ca vanh do - nhu ban truoc lam - thi mep nuoc lo han ra khoi mat
    /// dat, va ca vung nuoc nhin nhu mot cai dia lo lung.
    /// </summary>
    const float MepLong = 0.80f;

    /// <summary>
    /// Do cao dat o duong bo thi doc tu vanh nay tro ra.
    ///
    /// <b>Phai cach <see cref="MepLong"/> mot khoang dem.</b> Luoi do cao cua
    /// terrain la 513 diem tren 109 m, tuc <b>21 cm mot o</b>, va
    /// <c>SampleHeight</c> noi suy tu bon o quanh diem hoi. Neu hai vanh sat
    /// nhau - hoi o 0,86 ma khoet toi 0,85, tren ban kinh 7 m thi chi cach nhau
    /// 7 cm, chua bang mot phan ba o luoi - thi cai hoi RO RI sang cai khoet:
    /// moi lan chay lai, dat o bo do ra thap di mot chut, mat nuoc tut theo, va
    /// cu the tut mai. Da do duoc: lech 0,54 m moi lan, sau khi cho hai cho
    /// dung chung cong thuc thi con 0,044 m - van chua dung.
    ///
    /// 0,80 va 0,88 tren ban kinh 7 m la cach nhau 56 cm, hon hai o luoi.
    /// </summary>
    const float BoDo = 0.88f;

    /// <summary>Mep luoi nuoc phai nam duoi mat dat ngan nay.</summary>
    const float MepChim = 0.08f;

    /// <summary>
    /// So cung de do ban kinh cua mat nuoc.
    ///
    /// Luoi dam lay la hinh bat quy tac, khong phai hinh tron: cung mot vung
    /// ma huong nay dai 7 m, huong kia chi 3,5 m. Lay MOT ban kinh chung - cai
    /// lon nhat - thi moi phep tinh "d = khoang cach / ban kinh" deu sai o
    /// nhung huong luoi ngan: mep that o do roi vao khoang d = 0,5, tuc la nam
    /// gon trong vung khoet va khong bao gio duoc dem la mep.
    ///
    /// Do la ly do <c>DamLay_10</c> van lo lung nua met o huong 90 do ma phep
    /// do bao "0 dinh mep lo": no chi nhin nhung dinh o d >= 0,88, ma o huong
    /// do khong co dinh nao dat toi.
    /// </summary>
    const int SoCung = 72;

    struct Vung
    {
        public Vector2 tam;
        public float banKinh;
        public float yNuoc;
        public string ten;
        public Transform tran;
        public Mesh luoi;
        public float[] banKinhCung;
    }

    [MenuItem("Diablo25D/Act2 - Khoet long cho cac vung nuoc")]
    public static void Khoet()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.path != DuongScene)
        {
            if (sc.isDirty
                && !EditorUtility.DisplayDialog("Canh dang co thay doi chua luu",
                       "Mo Act2 se lam mat cac thay doi do. Van mo?", "Mo", "Thoi"))
                return;
            EditorSceneManager.OpenScene(DuongScene, OpenSceneMode.Single);
        }

        var terrain = Object.FindFirstObjectByType<Terrain>();
        if (terrain == null) { Debug.LogError("[Khoet] Khong thay Terrain trong Act2."); return; }

        var vung = TimCacVungNuoc();
        if (vung.Count == 0) { Debug.LogError("[Khoet] Khong thay vung nuoc nao."); return; }

        // Dat mat nuoc VAO trong long TRUOC khi khoet. Thu tu nay khong doi
        // duoc: khoet dua theo cao do mat nuoc, ma cao do mat nuoc lai dua theo
        // dat o duong bo - va duong bo thi khong bao gio bi khoet.
        int daHa = DatMatNuocVaoLong(terrain, vung);
        Debug.Log("[Khoet] Da dat lai cao do cho " + daHa + " / " + vung.Count + " mat nuoc.");

        var td = terrain.terrainData;
        int res = td.heightmapResolution;
        float[,] h = td.GetHeights(0, 0, res, res);

        SaoLuuNeuChua(h, res);

        Vector3 goc = terrain.transform.position;
        Vector3 co = td.size;

        int soDoi = 0;
        for (int z = 0; z < res; z++)
        {
            // Toa do the gioi cua o luoi nay
            float pz = goc.z + (z / (float)(res - 1)) * co.z;

            for (int x = 0; x < res; x++)
            {
                float px = goc.x + (x / (float)(res - 1)) * co.x;

                float cu = h[z, x];
                float moi = CaoDoMoi(px, pz, cu, goc.y, co.y, vung);
                if (!Mathf.Approximately(cu, moi)) soDoi++;
                h[z, x] = Mathf.Clamp01(moi);
            }
        }

        td.SetHeights(0, 0, h);
        terrain.Flush();

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        AssetDatabase.SaveAssets();

        Debug.Log("[Khoet] Xong: " + vung.Count + " vung nuoc, doi " + soDoi
                  + " / " + (res * res) + " o luoi do cao.");
    }

    /// <summary>
    /// Do cao moi tai mot o luoi.
    ///
    /// Lay cai THAP NHAT trong so cac vung anh huong toi o nay: hai vung ke nhau
    /// thi cho giao phai theo cai sau hon, khong thi giua chung noi len mot go
    /// dung ngay duoi mat nuoc.
    /// </summary>
    static float CaoDoMoi(float px, float pz, float cu, float gocY, float caoY, List<Vung> vung)
    {
        float ra = cu;

        for (int i = 0; i < vung.Count; i++)
        {
            float dx = px - vung[i].tam.x, dz = pz - vung[i].tam.y;
            float bk = BanKinhTheoHuong(vung[i].banKinhCung, dx, dz);
            float d = Mathf.Sqrt(dx * dx + dz * dz) / bk;

            // Tu duong bo tro ra: KHONG dong den. Dat o day chinh la cai giu
            // cho mep luoi nuoc chim vao, dao no di la lo mep.
            if (d >= MepLong) continue;

            // Do cao 0..1 cua mat nuoc tren terrain nay
            float nuoc01 = (vung[i].yNuoc - gocY) / caoY;

            // Sau nhat o tam, thoai dan ve 0 dung o duong bo
            float sau = SauTam * (1f - Mathf.SmoothStep(0f, 1f, d / MepLong));
            float dich01 = nuoc01 - sau / caoY;

            // CHI HA, khong bao gio nang: hai vung ke nhau thi cho giao phai
            // theo cai sau hon, va nang dat len thi bia mo quanh bo bi chon.
            ra = Mathf.Min(ra, dich01);
        }
        return ra;
    }

    /// <summary>
    /// Cac vung nuoc trong canh. Nhan ra bang SHADER chu khong bang ten:
    /// ten co the doi, con shader Diablo25D/NuocDam thi chi mat nuoc moi dung.
    /// </summary>
    static List<Vung> TimCacVungNuoc()
    {
        var ra = new List<Vung>();
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
        {
            if (r is ParticleSystemRenderer) continue;
            var m = r.sharedMaterial;
            if (m == null || m.shader == null || !m.shader.name.Contains("Nuoc")) continue;

            var mf = r.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            var b = r.bounds;

            // Ban kinh DO TU DINH LUOI, khong lay tu hop bao. Hop bao la mot cai
            // hop VUONG om lay mot mat nuoc TRON: goc hop thua ra 41%, con canh
            // hop thi vua khit. Lay nua canh hop lam ban kinh thi vanh khoet
            // hut mat mot phan mep, lay nua duong cheo thi no trum ra qua xa.
            var tamTG = new Vector2(b.center.x, b.center.z);
            var cung = DungBanKinhCung(r.transform, mf.sharedMesh, tamTG);
            float bkMax = 0.01f;
            for (int c = 0; c < cung.Length; c++) if (cung[c] > bkMax) bkMax = cung[c];

            ra.Add(new Vung
            {
                tam = tamTG,
                banKinh = bkMax,
                yNuoc = b.center.y,
                ten = r.name,
                tran = r.transform,
                luoi = mf.sharedMesh,
                banKinhCung = cung
            });
        }
        return ra;
    }

    /// <summary>
    /// Do ban kinh cua mat nuoc RIENG cho tung huong, trong toa do the gioi.
    ///
    /// Chia vong tron thanh <see cref="SoCung"/> cung; moi cung lay dinh xa tam
    /// nhat. Cung nao trong (luoi thua o huong do) thi muon cua cung co gia tri
    /// gan nhat. Cuoi cung lam tron nhe bang trung binh ba cung lien nhau, de
    /// "d" khong nhay bac khi di vong quanh vung - nhay bac thi vanh khoet co
    /// rang cua.
    /// </summary>
    static float[] DungBanKinhCung(Transform tran, Mesh luoi, Vector2 tamTG)
    {
        var bk = new float[SoCung];
        var dinh = luoi.vertices;

        for (int i = 0; i < dinh.Length; i++)
        {
            var w = tran.TransformPoint(dinh[i]);
            float dx = w.x - tamTG.x, dz = w.z - tamTG.y;
            float r = Mathf.Sqrt(dx * dx + dz * dz);
            int c = Cung(dx, dz);
            if (r > bk[c]) bk[c] = r;
        }

        // Cung trong: muon cua cung gan nhat co gia tri
        for (int c = 0; c < SoCung; c++)
        {
            if (bk[c] > 0f) continue;
            for (int b = 1; b < SoCung; b++)
            {
                float t = bk[(c - b + SoCung) % SoCung];
                float p = bk[(c + b) % SoCung];
                if (t > 0f || p > 0f) { bk[c] = Mathf.Max(t, p); break; }
            }
            if (bk[c] <= 0f) bk[c] = 0.01f;
        }

        // Lam tron
        var ra = new float[SoCung];
        for (int c = 0; c < SoCung; c++)
            ra[c] = (bk[(c - 1 + SoCung) % SoCung] + bk[c] + bk[(c + 1) % SoCung]) / 3f;
        return ra;
    }

    static int Cung(float dx, float dz)
    {
        float goc = Mathf.Atan2(dz, dx);                     // -PI .. PI
        int c = Mathf.FloorToInt((goc + Mathf.PI) / (2f * Mathf.PI) * SoCung);
        return Mathf.Clamp(c, 0, SoCung - 1);
    }

    /// <summary>Ban kinh o huong (dx,dz), noi suy giua hai cung ke nhau.</summary>
    static float BanKinhTheoHuong(float[] cung, float dx, float dz)
    {
        if (cung == null || cung.Length == 0) return 0.01f;
        float goc = Mathf.Atan2(dz, dx);
        float f = (goc + Mathf.PI) / (2f * Mathf.PI) * SoCung;
        int c0 = Mathf.FloorToInt(f);
        float t = f - c0;
        c0 = ((c0 % SoCung) + SoCung) % SoCung;
        int c1 = (c0 + 1) % SoCung;
        return Mathf.Max(0.01f, Mathf.Lerp(cung[c0], cung[c1], t));
    }

    /// <summary>
    /// Ha mat nuoc xuong cho no NAM TRONG long chao.
    ///
    /// Bai hoc: ban truoc chi lo khoet dat cho sau, ma khong ai hoi <b>mep luoi
    /// nuoc dang o dau so voi mat dat</b>. Do ra thi ca 11 vung nuoc deu co
    /// <b>100% so dinh nam TREN mat dat</b>, trung binh cao 0,32 m - nguoi choi
    /// thay dung mot cai dia nuoc lo lung giua nghia dia.
    ///
    /// Cach dat: lay cho THAP NHAT cua mat dat doc theo DUONG BO (vanh ngoai
    /// cua luoi), roi ha mat nuoc xuong duoi do them <see cref="MepChim"/>. Khi
    /// do moi diem tren mep luoi deu chim vao dat, khong con canh nao lo ra.
    ///
    /// Lam TRUOC khi khoet, va vung khoet dung lai o <see cref="MepLong"/> nen
    /// dat o duong bo khong bao gio bi dong toi - chay ham nay bao nhieu lan
    /// cung ra dung mot ket qua.
    /// </summary>
    static int DatMatNuocVaoLong(Terrain terrain, List<Vung> vung)
    {
        float goc = terrain.transform.position.y;
        int n = 0;

        for (int i = 0; i < vung.Count; i++)
        {
            var v = vung[i];
            if (v.tran == null || v.luoi == null) continue;

            var dinh = v.luoi.vertices;

            // Cho thap nhat cua dat doc theo DUONG BO.
            //
            // Cong thuc cua "d" o day phai giong HET trong CaoDoMoi - cung tam,
            // cung ban kinh, cung don vi. Lech mot chut thoi la vanh khoet an
            // vao dung nhung dinh dang duoc dung lam moc: chay lan hai, dat o bo
            // da thap di, mat nuoc lai tut theo, va cu the tut mai. Do la loi da
            // gap: chay lan hai DamLay_7 tut them 0,54 m.
            float datThap = 9e9f;
            int soBo = 0;
            for (int k = 0; k < dinh.Length; k++)
            {
                var w = v.tran.TransformPoint(dinh[k]);
                float dx = w.x - v.tam.x, dz = w.z - v.tam.y;
                float d = Mathf.Sqrt(dx * dx + dz * dz) / BanKinhTheoHuong(v.banKinhCung, dx, dz);
                if (d < BoDo) continue;
                soBo++;
                float dat = terrain.SampleHeight(w) + goc;
                if (dat < datThap) datThap = dat;
            }
            if (soBo == 0) continue;

            float yMoi = datThap - MepChim;
            float dy = yMoi - v.yNuoc;
            if (Mathf.Abs(dy) < 0.002f) continue;

            Undo.RecordObject(v.tran, "dat mat nuoc vao long");
            v.tran.position += new Vector3(0f, dy, 0f);
            EditorUtility.SetDirty(v.tran);

            v.yNuoc = yMoi;
            vung[i] = v;
            n++;
        }
        return n;
    }

    /// <summary>
    /// Sao luu do cao mot lan duy nhat - lan dau chay.
    ///
    /// Chay lan hai ma ghi de sao luu thi mat ban goc: luc do "ban truoc khi
    /// khoet" da la ban DA khoet roi, va khong con duong lui.
    /// </summary>
    static void SaoLuuNeuChua(float[,] h, int res)
    {
        if (System.IO.File.Exists(DuongSaoLuu)) return;

        var bytes = new byte[res * res * 4];
        System.Buffer.BlockCopy(ToMang(h, res), 0, bytes, 0, bytes.Length);
        System.IO.File.WriteAllBytes(DuongSaoLuu, bytes);
        AssetDatabase.ImportAsset(DuongSaoLuu);
        Debug.Log("[Khoet] Da sao luu do cao truoc khi khoet vao " + DuongSaoLuu);
    }

    [MenuItem("Diablo25D/Act2 - Tra lai do cao truoc khi khoet")]
    public static void TraLai()
    {
        if (!System.IO.File.Exists(DuongSaoLuu))
        { Debug.LogError("[Khoet] Khong co sao luu."); return; }

        var sc = EditorSceneManager.GetActiveScene();
        if (sc.path != DuongScene) EditorSceneManager.OpenScene(DuongScene, OpenSceneMode.Single);

        var terrain = Object.FindFirstObjectByType<Terrain>();
        var td = terrain.terrainData;
        int res = td.heightmapResolution;

        var bytes = System.IO.File.ReadAllBytes(DuongSaoLuu);
        var phang = new float[res * res];
        System.Buffer.BlockCopy(bytes, 0, phang, 0, bytes.Length);

        var h = new float[res, res];
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                h[z, x] = phang[z * res + x];

        td.SetHeights(0, 0, h);
        terrain.Flush();
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log("[Khoet] Da tra lai do cao truoc khi khoet.");
    }

    static float[] ToMang(float[,] h, int res)
    {
        var ra = new float[res * res];
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                ra[z * res + x] = h[z, x];
        return ra;
    }
}
