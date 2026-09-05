using UnityEngine;

/// <summary>
/// DUNG NAM NGOI MO.
///
/// Nam kieu khac han nhau chu khong phai mot kieu doi mau, de rai ra ban do
/// khong bi lo la cung mot vat lap lai:
///
///   0. BIA DA NUT      - tam bia nghieng, nut doc than, khac thanh gia mo di
///   1. THANH GIA GAY   - thanh gia go xieu veo, gay mot ben tay, quan gie rach
///   2. MO BAT NAP      - huyet mo ha mieng, nap da truot sang ben, ban tay xuong cao len
///   3. QUAN TAI DA     - hom da khoet rong, NAP HE RA O DAU TREN lo ca BO XUONG
///                        nguoi nam ben trong, xich sat khoa nua duoi
///   4. COC TREO SO     - coc go dong xuong u dat, ba cai so nguoi treo lung lang
///
/// Ngoai ra con hai kieu xac treo len cay, deu biet du dua va thinh thoang
/// ha ham la het (xem <see cref="XacLacLu"/>):
///   - <see cref="BoXuongTreo"/>    bi giao dam xuyen nguc, ghim vao than cay
///   - <see cref="BoXuongTreoCo"/>  bi treo co lung lang duoi canh cay
///
/// Deu sinh bang code nhu moi thu khac trong game, khong dung file .fbx nao.
/// </summary>
public static class GraveFactory
{
    public const int SoKieu = 5;

    // ================================================================
    //  VAT LIEU RIENG CUA KHU MO
    // ================================================================

    static Material mDatMo, mMaTroi, mGoMuc, mGieRach;

    /// <summary>Dat moi dao len: den, am, khac han dat kho xung quanh.</summary>
    public static Material DatMo
    {
        get
        {
            if (mDatMo == null)
                // Do do phai day len va nga am han. Mau cu (0.115, 0.095, 0.080) qua
                // toi, gap anh trang mau xanh la thanh ra xam lam nhu be tong uot.
                // Mau o day NHAN voi anh DarkSoil (do sang trung binh chi 0.22, 0.13, 0.06).
                // De mau toi o ca hai ben thi ket qua ra gan nhu den; ma vat den thui
                // thi chi con hat lai mau cua den chieu vao - thanh ra xam lam.
                // Nen mau nay phai de SANG, cho anh keo xuong moi vua.
                mDatMo = Mats.Solid("DatMo", new Color(0.88f, 0.64f, 0.46f), 0.05f, 0f,
                                    TextureFactory.DarkSoil(), 3.2f);
            return mDatMo;
        }
    }

    /// <summary>Anh ma troi xanh benh hoan roi ra tu khe nut cua mo.</summary>
    public static Material MaTron
    {
        get
        {
            if (mMaTroi == null)
                // Do sang 2.6 truoc day lam mau bi chay het thanh TRANG, mat sach
                // ve xanh benh hoan. Ha xuong 0.95 thi moi giu duoc mau.
                mMaTroi = Mats.Glow("MaTroi", new Color(0.10f, 0.24f, 0.13f),
                                    new Color(0.16f, 0.62f, 0.22f), 0.95f);
            return mMaTroi;
        }
    }

    /// <summary>Go muc lau nam ngoai mua: xam den, gan nhu khong con mau go.</summary>
    public static Material GoMuc
    {
        get
        {
            if (mGoMuc == null)
                // Cung ly do nhu DatMo: anh vo cay von da toi san
                mGoMuc = Mats.Solid("GoMuc", new Color(0.46f, 0.40f, 0.34f), 0.05f, 0f,
                                    TextureFactory.Bark(), 1.6f);
            return mGoMuc;
        }
    }

    /// <summary>Gie rach bac phech quan quanh thanh gia.</summary>
    public static Material GieRach
    {
        get
        {
            if (mGieRach == null)
                mGieRach = Mats.Solid("GieRach", new Color(0.34f, 0.31f, 0.27f), 0.05f, 0f);
            return mGieRach;
        }
    }

    // ================================================================
    //  MANH GHEP DUNG CHUNG
    // ================================================================

    /// <summary>Tam bia da: than vuong, dinh vom tron, be mat da bi mua gio an mon.</summary>
    static Mesh TamBia(float rong, float cao, float day, bool coVom, int seed, float ganghe)
    {
        var b = new ProcMesh.Builder();
        b.Append(ProcMesh.Box(new Vector3(rong, cao, day), Color.white),
                 Matrix4x4.Translate(new Vector3(0f, cao * 0.5f, 0f)), Color.white);

        if (coVom)
        {
            // Nua hinh tru nam ngang lam dinh vom
            var vom = ProcMesh.Taper(rong * 0.5f, rong * 0.5f, day, 14, true, Color.white);
            b.Append(vom, Matrix4x4.TRS(new Vector3(0f, cao, -day * 0.5f),
                                        Quaternion.Euler(90f, 0f, 0f), Vector3.one), Color.white);
        }

        var m = b.Build("tambia");
        ProcMesh.Displace(m, ganghe, 5.5f, seed);
        m.RecalculateNormals();
        m.RecalculateTangents();
        return m;
    }

    /// <summary>U dat dap len tren huyet mo.</summary>
    static Mesh UDat(float ban, float cao, int seed)
    {
        // Bet han xuong va go ghe han. Truoc day u dat tron mun nhu qua trung
        // up nguoc, nhin ra tang da chu khong ra dong dat moi dap.
        var m = ProcMesh.Sphere(ban, 16, 10, 1f, Color.white);
        ProcMesh.ScaleMesh(m, new Vector3(0.86f, cao / ban * 0.72f, 1.28f));
        ProcMesh.Displace(m, ban * 0.26f, 5.8f, seed);
        m.RecalculateNormals();
        m.RecalculateTangents();
        return m;
    }

    /// <summary>So nguoi: hop so tron, ham duoi nho ra, hai hoc mat den ngom.</summary>
    public static GameObject So(Transform cha, string ten, Vector3 tai, Quaternion xoay, float r)
    {
        var go = new GameObject(ten);
        go.transform.SetParent(cha, false);
        go.transform.localPosition = tai;
        go.transform.localRotation = xoay;

        var hop = ProcMesh.Sphere(r, 14, 10, 1.06f, Color.white);
        ProcMesh.ScaleMesh(hop, new Vector3(0.90f, 1f, 1.05f));
        ProcMesh.Part("HopSo", go.transform, hop, Mats.BoneMat, Vector3.zero);

        // Ham duoi treo tren mot BAN LE dat o phia sau, ngang cho khop ham that.
        // Co ban le thi xoay quanh no ham moi ha xuong dung kieu ha mieng;
        // xoay thang cai ham thi no quay quanh chinh giua, nhin nhu ham roi ra.
        var banLe = new GameObject("HamPivot");
        banLe.transform.SetParent(go.transform, false);
        banLe.transform.localPosition = new Vector3(0f, -r * 0.30f, -r * 0.05f);

        var ham = ProcMesh.Sphere(r * 0.60f, 10, 7, 0.72f, Color.white);
        ProcMesh.Part("Ham", banLe.transform, ham, Mats.BoneMat,
                      new Vector3(0f, -r * 0.30f, r * 0.47f),
                      Quaternion.identity, new Vector3(0.88f, 1f, 1.20f), true);

        // Hai hoc mat: hai cai lo den, thu lam cai so trong ra "so"
        // Hoc mat phai TO va SAU thi nhin xa moi doc ra la cai so.
        // Truoc day ban kinh 0.30r, tu goc choi chi con vai diem anh, cai so hoa
        // ra hon khoai tay.
        var hoc = ProcMesh.Sphere(r * 0.46f, 9, 7, 0.80f, Color.white);
        var matDen = Mats.Solid("HocMat", new Color(0.020f, 0.018f, 0.022f), 0.03f, 0f);
        for (int i = 0; i < 2; i++)
        {
            float ben = i == 0 ? 1f : -1f;
            ProcMesh.Part("HocMat" + i, go.transform, hoc, matDen,
                          new Vector3(r * 0.38f * ben, r * 0.14f, r * 0.66f),
                          Quaternion.identity, new Vector3(1.20f, 0.95f, 0.7f), false);
        }

        // Hoc mui
        ProcMesh.Part("HocMui", go.transform, hoc, matDen,
                      new Vector3(0f, -r * 0.20f, r * 0.80f),
                      Quaternion.identity, new Vector3(0.55f, 0.75f, 0.5f), false);
        return go;
    }

    /// <summary>Mot khuc xuong dai.</summary>
    public static void Xuong(Transform cha, string ten, Vector3 tai, Quaternion xoay, float dai, float day)
    {
        var m = ProcMesh.Taper(day, day * 0.88f, dai, 7, true, Color.white);
        ProcMesh.Part(ten, cha, m, Mats.BoneMat, tai, xoay, Vector3.one, true);

        // Hai dau xuong phinh to
        var dau = ProcMesh.Sphere(day * 1.45f, 8, 6, 0.85f, Color.white);
        ProcMesh.Part(ten + "_a", cha, dau, Mats.BoneMat, tai, xoay, Vector3.one, false);
        ProcMesh.Part(ten + "_b", cha, dau, Mats.BoneMat,
                      tai + xoay * new Vector3(0f, dai, 0f), xoay, Vector3.one, false);
    }

    /// <summary>
    /// BO XUONG NGUOI nam ngua trong long hom: so, xuong song, long nguc,
    /// xuong chau, hai tay xuoi hai ben va hai chan duoi thang.
    ///
    /// Dat theo truc Z: dau o phia +Z (cho nap ha mieng), chan o phia -Z.
    /// </summary>
    static void BoXuongNam(Transform cha, float rong, float dai, float mucDay, int seed)
    {
        Random.InitState(seed);

        var go = new GameObject("BoXuong");
        go.transform.SetParent(cha, false);
        go.transform.localPosition = new Vector3(0f, mucDay, 0f);
        // Nam hoi lech di mot chut cho khoi ngay ngan nhu do xep
        go.transform.localRotation = Quaternion.Euler(0f, Random.Range(-4f, 4f), 0f);

        float nuaDai = dai * 0.5f;

        // ---- SO, nga sang mot ben ----
        So(go.transform, "So", new Vector3(Random.Range(-0.03f, 0.03f), 0.062f, nuaDai * 0.74f),
           Quaternion.Euler(Random.Range(-16f, -4f), Random.Range(-34f, 34f), Random.Range(-16f, 16f)),
           0.105f);

        // ---- XUONG SONG ----
        var dot = ProcMesh.Box(new Vector3(0.042f, 0.030f, 0.052f), Color.white);
        for (int i = 0; i < 7; i++)
        {
            float t = i / 6f;
            ProcMesh.Part("DotSong" + i, go.transform, dot, Mats.BoneMat,
                          new Vector3(0f, 0.012f, Mathf.Lerp(nuaDai * 0.56f, -nuaDai * 0.12f, t)),
                          Quaternion.Euler(0f, Random.Range(-6f, 6f), 0f), Vector3.one, false);
        }

        // ---- LONG NGUC: nam cap suon cong om lay long nguc ----
        for (int i = 0; i < 5; i++)
        {
            float t = i / 4f;
            float z = Mathf.Lerp(nuaDai * 0.52f, nuaDai * 0.06f, t);
            float beRong = Mathf.Lerp(0.80f, 1f, Mathf.Sin(t * Mathf.PI)) * rong * 0.40f;

            // Xem ghi chu o BoXuongTreo: he so phai la tong do cong chia chieu dai
            var suon = ProcMesh.Taper(0.015f, 0.010f, beRong, 7, false, Color.white);
            ProcMesh.Bend(suon, 460f, Vector3.right);

            for (int b = 0; b < 2; b++)
            {
                float ben = b == 0 ? 1f : -1f;
                ProcMesh.Part("Suon" + i + "_" + b, go.transform, suon, Mats.BoneMat,
                              new Vector3(0.018f * ben, 0.020f, z),
                              Quaternion.Euler(0f, 0f, 74f * ben), Vector3.one, true);
            }
        }

        // ---- XUONG CHAU ----
        var chau = ProcMesh.Box(new Vector3(rong * 0.46f, 0.038f, 0.13f), Color.white);
        ProcMesh.Part("XuongChau", go.transform, chau, Mats.BoneMat,
                      new Vector3(0f, 0.016f, -nuaDai * 0.20f),
                      Quaternion.Euler(0f, Random.Range(-5f, 5f), 0f), Vector3.one, true);

        // ---- HAI TAY xuoi hai ben suon ----
        for (int b = 0; b < 2; b++)
        {
            float ben = b == 0 ? 1f : -1f;
            float x = rong * 0.36f * ben;

            Xuong(go.transform, "CanhTay" + b, new Vector3(x, 0.012f, nuaDai * 0.44f),
                  Quaternion.Euler(90f, Random.Range(-8f, 8f), 0f), nuaDai * 0.40f, 0.017f);
            Xuong(go.transform, "CangTay" + b, new Vector3(x * 1.04f, 0.012f, nuaDai * 0.04f),
                  Quaternion.Euler(90f, Random.Range(-10f, 10f), 0f), nuaDai * 0.34f, 0.014f);

            // Ban tay: mot cum dot nho
            for (int n = 0; n < 3; n++)
                ProcMesh.Part("DotTay" + b + "_" + n, go.transform,
                              ProcMesh.Taper(0.007f, 0.005f, 0.045f, 4, false, Color.white),
                              Mats.BoneMat,
                              new Vector3(x * 1.06f + (n - 1) * 0.016f, 0.012f, -nuaDai * 0.34f),
                              Quaternion.Euler(92f, 0f, (n - 1) * 9f), Vector3.one, false);
        }

        // ---- HAI CHAN duoi thang xuong phia chan hom ----
        for (int b = 0; b < 2; b++)
        {
            float ben = b == 0 ? 1f : -1f;
            float x = rong * 0.17f * ben;

            Xuong(go.transform, "XuongDui" + b, new Vector3(x, 0.014f, -nuaDai * 0.28f),
                  Quaternion.Euler(90f, Random.Range(-6f, 6f), 0f), nuaDai * 0.42f, 0.021f);
            Xuong(go.transform, "XuongOng" + b, new Vector3(x * 1.05f, 0.014f, -nuaDai * 0.70f),
                  Quaternion.Euler(90f, Random.Range(-8f, 8f), 0f), nuaDai * 0.24f, 0.017f);

            // Ban chan quay ngang ra
            ProcMesh.Part("BanChan" + b, go.transform,
                          ProcMesh.Box(new Vector3(0.045f, 0.022f, 0.075f), Color.white),
                          Mats.BoneMat,
                          new Vector3(x * 1.05f, 0.016f, -nuaDai * 0.90f),
                          Quaternion.Euler(0f, Random.Range(-22f, 22f) + 12f * ben, 0f),
                          Vector3.one, true);
        }
    }

    /// <summary>Vai vien da vun ke quanh chan mo.</summary>
    public static void DaVun(Transform cha, int soVien, float banKinh, int seed)
    {
        Random.InitState(seed);
        for (int i = 0; i < soVien; i++)
        {
            float goc = Random.Range(0f, 360f);
            float xa = Random.Range(banKinh * 0.45f, banKinh);
            Vector3 p = Quaternion.Euler(0f, goc, 0f) * new Vector3(0f, 0f, xa);
            p.y = Random.Range(-0.04f, 0.05f);

            var m = ProcMesh.Rock(Random.Range(0.055f, 0.135f), seed * 31 + i, 0.42f, 7, Color.white);
            m.RecalculateTangents();
            ProcMesh.Part("DaVun" + i, cha, m, Mats.Rock, p,
                          Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), 0f),
                          Vector3.one, true);
        }
    }

    /// <summary>
    /// Mot doan day xich: nhieu khoen nho noi tiep, khoen le xoay 90 do so voi
    /// khoen chan - dung nhu day xich that, nhin nghieng moi ra mat xich.
    /// </summary>
    public static void DoanXich(Transform cha, string ten, Material mat, Vector3 giua, Vector3 co, int soKhoen)
    {
        bool nam = co.x > co.y;                      // doan nam ngang hay doan doc
        float dai = nam ? co.x : co.y;

        for (int i = 0; i < soKhoen; i++)
        {
            float t = soKhoen == 1 ? 0.5f : i / (float)(soKhoen - 1);
            float lech = (t - 0.5f) * dai;
            Vector3 p = giua + (nam ? new Vector3(lech, 0f, 0f) : new Vector3(0f, lech, 0f));

            var khoen = ProcMesh.Taper(co.z * 0.5f, co.z * 0.5f, dai / soKhoen * 1.25f, 6, true, Color.white);
            Quaternion xoay = nam ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
            // Khoen le xoay nghieng di cho ra ve mat xich
            if (i % 2 == 1) xoay = xoay * Quaternion.Euler(0f, 90f, 0f);

            ProcMesh.Part(ten + "_" + i, cha, khoen, mat, p, xoay,
                          new Vector3(1f, 1f, 0.55f), false);
        }
    }

    /// <summary>Dom lua ma xanh lo lung tren mo - thu lam nguoi choi thay ron nguoi.</summary>
    public static void DomMa(Transform cha, Vector3 tai, float manh)
    {
        var go = new GameObject("DomMa");
        go.transform.SetParent(cha, false);
        go.transform.localPosition = tai;

        var nhan = ProcMesh.Sphere(0.042f, 10, 8, 1f, Color.white);
        ProcMesh.Part("Nhan", go.transform, nhan, MaTron, Vector3.zero,
                      Quaternion.identity, Vector3.one, false);

        var den = go.AddComponent<Light>();
        den.type = LightType.Point;
        den.color = new Color(0.42f, 1f, 0.50f);
        den.intensity = 1.15f * manh;
        den.range = 4.2f * manh;
        den.shadows = LightShadows.None;

        var nhay = go.AddComponent<LightFlicker>();
        nhay.baseIntensity = 1.15f * manh;
        nhay.amount = 0.45f;
        nhay.speed = 3.2f;
        nhay.rangeWobble = 0.16f;
    }

    // ================================================================
    //  BO XUONG BI DONG GIAO TREO LEN CAY
    // ================================================================

    /// <summary>
    /// Mot BO XUONG NGUOI bi ngon giao dam xuyen nguc, ghim vao than cay.
    ///
    /// Truc toa do cua vat nay: goc dat ngay CHO GIAO CAM VAO VO CAY.
    ///   +Z  huong ra ngoai than cay (ve phia nguoi nhin)
    ///   +Y  huong len troi
    /// Nho vay luc dat len cay chi can day ra bang ban kinh than cay va xoay
    /// quanh truc dung la xong.
    ///
    /// Xac treo thi phai RU XUONG: dau guc ve truoc, hai tay buong thong,
    /// hai chan dong dua khong cham dat. Dung thang nguoi thi ra hinh nom
    /// chu khong ra cai xac.
    /// </summary>
    public static GameObject BoXuongTreo(Transform parent, int seed)
    {
        Random.InitState(seed * 613 + 7);

        var go = new GameObject("XuongTreo");
        if (parent != null) go.transform.SetParent(parent, false);

        // ---- NGON GIAO dam xuyen nguc, cam sau vao than cay ----
        var matMuiGiao = Mats.Solid("MuiGiao", new Color(0.30f, 0.29f, 0.28f), 0.42f, 0.70f);

        // Can giao: chay tu trong than cay (-Z) ra tan phia truoc (+Z)
        // Xoay 90 do quanh truc X thi +Y thanh +Z, nen can giao chay tu z = -0.55
        // (cam sau trong than cay) xuyen qua long nguc o z = +0.17 roi tho ra +0.60.
        // Neu xoay -90 do thi truc lat nguoc, can giao chui het vao trong cay -
        // day dung la loi toi mac o lan dung dau tien.
        var can = ProcMesh.Taper(0.026f, 0.022f, 1.15f, 7, true, Color.white);
        ProcMesh.Part("CanGiao", go.transform, can, GoMuc,
                      new Vector3(0f, 0f, -0.55f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, true);

        // Mui giao nhon tho han ra khoi long nguc
        var mui = ProcMesh.Crystal(0.052f, 0.26f, 4, 0.08f, Color.white);
        ProcMesh.Part("MuiGiao", go.transform, mui, matMuiGiao,
                      new Vector3(0f, 0f, 0.60f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, true);

        // Vong chan o co giao
        ProcMesh.Part("VongGiao", go.transform,
                      ProcMesh.Taper(0.034f, 0.034f, 0.05f, 7, true, Color.white), matMuiGiao,
                      new Vector3(0f, 0f, 0.55f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, false);

        // ---- THAN XAC: ru ve truoc, hoi ngheo sang mot ben ----
        var than = new GameObject("Than");
        than.transform.SetParent(go.transform, false);
        than.transform.localPosition = new Vector3(0f, 0f, 0.115f);
        than.transform.localRotation = Quaternion.Euler(Random.Range(6f, 15f), 0f,
                                                        Random.Range(-11f, 11f));

        // Long nguc - chinh cho ngon giao xuyen qua
        for (int i = 0; i < 6; i++)
        {
            float t = i / 5f;
            float y = Mathf.Lerp(0.20f, -0.14f, t);
            float beRong = Mathf.Lerp(0.15f, 0.19f, Mathf.Sin(t * Mathf.PI * 0.9f));

            // Bend tinh TONG do cong = chieu dai x he so. Suon chi dai 0.19m nen
            // he so phai lon (~480) moi cong duoc gan mot phan tu vong om lay nguc.
            // He so 82 truoc day chi cong 15 do - ra day song ngang nhu cai thang.
            var suon = ProcMesh.Taper(0.014f, 0.010f, beRong, 7, false, Color.white);
            ProcMesh.Bend(suon, 470f, Vector3.right);

            for (int b = 0; b < 2; b++)
            {
                float ben = b == 0 ? 1f : -1f;
                ProcMesh.Part("Suon" + i + "_" + b, than.transform, suon, Mats.BoneMat,
                              new Vector3(0.020f * ben, y, 0f),
                              Quaternion.Euler(0f, 0f, 76f * ben), Vector3.one, true);
            }
        }

        // Xuong song chay tu co xuong chau
        var dot = ProcMesh.Box(new Vector3(0.045f, 0.034f, 0.050f), Color.white);
        for (int i = 0; i < 8; i++)
        {
            float t = i / 7f;
            ProcMesh.Part("DotSong" + i, than.transform, dot, Mats.BoneMat,
                          new Vector3(0f, Mathf.Lerp(0.27f, -0.30f, t), -0.045f),
                          Quaternion.Euler(0f, 0f, Random.Range(-4f, 4f)), Vector3.one, false);
        }

        // Vai
        var vai = ProcMesh.Taper(0.017f, 0.014f, 0.30f, 6, true, Color.white);
        ProcMesh.Part("XuongDon", than.transform, vai, Mats.BoneMat,
                      new Vector3(0f, 0.245f, 0.005f), Quaternion.Euler(0f, 0f, 90f), Vector3.one, true);

        // Xuong chau
        ProcMesh.Part("XuongChau", than.transform,
                      ProcMesh.Box(new Vector3(0.175f, 0.10f, 0.085f), Color.white), Mats.BoneMat,
                      new Vector3(0f, -0.345f, -0.015f), Quaternion.identity, Vector3.one, true);

        // ---- SO guc han ve truoc, khong con do noi ----
        So(than.transform, "So", new Vector3(Random.Range(-0.03f, 0.03f), 0.40f, 0.045f),
           Quaternion.Euler(Random.Range(34f, 58f), Random.Range(-26f, 26f), Random.Range(-18f, 18f)),
           0.115f);

        // Vai dot co lo ra giua so va vai
        for (int i = 0; i < 2; i++)
            ProcMesh.Part("DotCo" + i, than.transform,
                          ProcMesh.Taper(0.026f, 0.024f, 0.035f, 6, true, Color.white), Mats.BoneMat,
                          new Vector3(0f, 0.285f + i * 0.038f, 0.008f + i * 0.012f),
                          Quaternion.Euler(20f, 0f, 0f), Vector3.one, false);

        // ---- HAI TAY buong thong xuong ----
        for (int b = 0; b < 2; b++)
        {
            float ben = b == 0 ? 1f : -1f;
            float x = 0.145f * ben;

            var vaiGo = new GameObject("Tay" + b);
            vaiGo.transform.SetParent(than.transform, false);
            vaiGo.transform.localPosition = new Vector3(x, 0.235f, 0f);
            vaiGo.transform.localRotation = Quaternion.Euler(Random.Range(-14f, 10f),
                                                             Random.Range(-14f, 14f),
                                                             Random.Range(4f, 16f) * ben);

            Xuong(vaiGo.transform, "CanhTay", Vector3.zero,
                  Quaternion.Euler(180f, 0f, 0f), 0.26f, 0.020f);

            var khuyu = new GameObject("Khuyu" + b);
            khuyu.transform.SetParent(vaiGo.transform, false);
            khuyu.transform.localPosition = new Vector3(0f, -0.26f, 0f);
            khuyu.transform.localRotation = Quaternion.Euler(Random.Range(-22f, 6f), 0f,
                                                             Random.Range(-9f, 9f));

            Xuong(khuyu.transform, "CangTay", Vector3.zero,
                  Quaternion.Euler(180f, 0f, 0f), 0.24f, 0.016f);

            // Ban tay: mot cum dot ngon quap lai
            for (int n = 0; n < 4; n++)
                ProcMesh.Part("DotTay" + n, khuyu.transform,
                              ProcMesh.Taper(0.008f, 0.005f, 0.052f, 4, false, Color.white),
                              Mats.BoneMat,
                              new Vector3((n - 1.5f) * 0.017f, -0.265f, 0.004f),
                              Quaternion.Euler(Random.Range(160f, 200f), 0f, (n - 1.5f) * 8f),
                              Vector3.one, false);
        }

        // ---- HAI CHAN dong dua, hoi co lai ----
        for (int b = 0; b < 2; b++)
        {
            float ben = b == 0 ? 1f : -1f;

            var hong = new GameObject("Chan" + b);
            hong.transform.SetParent(than.transform, false);
            hong.transform.localPosition = new Vector3(0.075f * ben, -0.385f, 0f);
            hong.transform.localRotation = Quaternion.Euler(Random.Range(-16f, 8f), 0f,
                                                            Random.Range(2f, 13f) * ben);

            Xuong(hong.transform, "XuongDui", Vector3.zero,
                  Quaternion.Euler(180f, 0f, 0f), 0.34f, 0.025f);

            var goi = new GameObject("Goi" + b);
            goi.transform.SetParent(hong.transform, false);
            goi.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            goi.transform.localRotation = Quaternion.Euler(Random.Range(8f, 30f), 0f, 0f);

            Xuong(goi.transform, "XuongOng", Vector3.zero,
                  Quaternion.Euler(180f, 0f, 0f), 0.32f, 0.019f);

            // Ban chan buong thong, mui chuc xuong
            ProcMesh.Part("BanChan", goi.transform,
                          ProcMesh.Box(new Vector3(0.052f, 0.10f, 0.030f), Color.white), Mats.BoneMat,
                          new Vector3(0f, -0.365f, 0.018f),
                          Quaternion.Euler(Random.Range(-24f, -6f), Random.Range(-14f, 14f), 0f),
                          Vector3.one, true);
        }

        // ---- Vai manh gie rach con dinh tren xuong ----
        for (int i = 0; i < 2; i++)
        {
            var gie = ProcMesh.Box(new Vector3(0.13f, Random.Range(0.20f, 0.34f), 0.010f), Color.white);
            ProcMesh.Bend(gie, Random.Range(16f, 34f), Vector3.right);
            var gieGo = ProcMesh.Part("Gie" + i, than.transform, gie, GieRach,
                                      new Vector3(Random.Range(-0.10f, 0.10f),
                                                  Random.Range(-0.30f, -0.05f),
                                                  Random.Range(0.03f, 0.09f)),
                                      Quaternion.Euler(Random.Range(-12f, 12f),
                                                       Random.Range(-25f, 25f),
                                                       Random.Range(-14f, 14f)),
                                      Vector3.one, false);
            gieGo.GetComponent<Renderer>().sharedMaterial = Mats.TwoSided(GieRach);
        }

        // Bi ghim vao than cay thi khong du dua duoc nhieu - chi ron ren nhe
        GanCuDong(go, than.transform, 2.2f, 3.0f, 0.42f);
        return go;
    }

    // ================================================================
    //  BO XUONG BI TREO CO TREN CANH CAY
    // ================================================================

    /// <summary>
    /// Mot BO XUONG bi treo co lung lang duoi canh cay.
    ///
    /// Goc toa do dat ngay CHO BUOC DAY VAO CANH. Day thong xuong, den nut thong
    /// long thi den co cai xac; ca cai xac treo o do nen du dua rat thoai mai.
    /// </summary>
    public static GameObject BoXuongTreoCo(Transform parent, int seed)
    {
        Random.InitState(seed * 811 + 13);

        var go = new GameObject("XuongTreoCo");
        if (parent != null) go.transform.SetParent(parent, false);

        float dayDay = Random.Range(0.55f, 0.95f);

        // ---- SOI DAY thong tu canh xuong ----
        var day = ProcMesh.Taper(0.017f, 0.015f, dayDay, 5, false, Color.white);
        ProcMesh.Part("SoiDay", go.transform, day, GieRach,
                      new Vector3(0f, -dayDay, 0f), Quaternion.identity, Vector3.one, true);

        // ---- KHOI TREO: tu day tro xuong, day la thu du dua ----
        var treo = new GameObject("Treo");
        treo.transform.SetParent(go.transform, false);
        treo.transform.localPosition = new Vector3(0f, -dayDay, 0f);
        treo.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f),
                                                        Random.Range(0f, 360f),
                                                        Random.Range(-6f, 6f));

        // Nut thong long quanh co
        for (int i = 0; i < 7; i++)
        {
            float a = i / 7f * Mathf.PI * 2f;
            ProcMesh.Part("Nut" + i, treo.transform,
                          ProcMesh.Taper(0.012f, 0.012f, 0.055f, 4, true, Color.white), GieRach,
                          new Vector3(Mathf.Cos(a) * 0.062f, -0.045f, Mathf.Sin(a) * 0.062f),
                          Quaternion.Euler(72f, -a * Mathf.Rad2Deg, 0f), Vector3.one, false);
        }

        // ---- SO nga han sang mot ben: co da gay ----
        var soGo = So(treo.transform, "So", new Vector3(0f, 0.055f, 0.010f),
                      Quaternion.Euler(Random.Range(10f, 26f),
                                       Random.Range(-30f, 30f),
                                       Random.Range(38f, 62f) * (Random.value < 0.5f ? 1f : -1f)),
                      0.115f);

        // ---- Xuong song va long nguc ----
        var dot = ProcMesh.Box(new Vector3(0.045f, 0.034f, 0.050f), Color.white);
        for (int i = 0; i < 8; i++)
            ProcMesh.Part("DotSong" + i, treo.transform, dot, Mats.BoneMat,
                          new Vector3(0f, Mathf.Lerp(-0.09f, -0.62f, i / 7f), -0.030f),
                          Quaternion.Euler(0f, 0f, Random.Range(-4f, 4f)), Vector3.one, false);

        for (int i = 0; i < 6; i++)
        {
            float t = i / 5f;
            float beRong = Mathf.Lerp(0.15f, 0.19f, Mathf.Sin(t * Mathf.PI * 0.9f));
            var suon = ProcMesh.Taper(0.014f, 0.010f, beRong, 7, false, Color.white);
            ProcMesh.Bend(suon, 470f, Vector3.right);

            for (int b = 0; b < 2; b++)
            {
                float ben = b == 0 ? 1f : -1f;
                ProcMesh.Part("Suon" + i + "_" + b, treo.transform, suon, Mats.BoneMat,
                              new Vector3(0.020f * ben, Mathf.Lerp(-0.14f, -0.46f, t), 0f),
                              Quaternion.Euler(0f, 0f, 76f * ben), Vector3.one, true);
            }
        }

        ProcMesh.Part("XuongDon", treo.transform,
                      ProcMesh.Taper(0.017f, 0.014f, 0.30f, 6, true, Color.white), Mats.BoneMat,
                      new Vector3(0f, -0.115f, 0.005f), Quaternion.Euler(0f, 0f, 90f),
                      Vector3.one, true);

        ProcMesh.Part("XuongChau", treo.transform,
                      ProcMesh.Box(new Vector3(0.175f, 0.10f, 0.085f), Color.white), Mats.BoneMat,
                      new Vector3(0f, -0.68f, -0.010f), Quaternion.identity, Vector3.one, true);

        // ---- HAI TAY buong thang xuong ----
        var tayChan = new System.Collections.Generic.List<Transform>();

        for (int b = 0; b < 2; b++)
        {
            float ben = b == 0 ? 1f : -1f;

            var vai = new GameObject("Tay" + b);
            vai.transform.SetParent(treo.transform, false);
            vai.transform.localPosition = new Vector3(0.145f * ben, -0.125f, 0f);
            vai.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 12f), 0f,
                                                           Random.Range(3f, 13f) * ben);
            tayChan.Add(vai.transform);

            Xuong(vai.transform, "CanhTay", Vector3.zero, Quaternion.Euler(180f, 0f, 0f), 0.26f, 0.020f);

            var khuyu = new GameObject("Khuyu");
            khuyu.transform.SetParent(vai.transform, false);
            khuyu.transform.localPosition = new Vector3(0f, -0.26f, 0f);
            khuyu.transform.localRotation = Quaternion.Euler(Random.Range(-16f, 8f), 0f, 0f);
            tayChan.Add(khuyu.transform);

            Xuong(khuyu.transform, "CangTay", Vector3.zero, Quaternion.Euler(180f, 0f, 0f), 0.24f, 0.016f);

            for (int n = 0; n < 4; n++)
                ProcMesh.Part("DotTay" + n, khuyu.transform,
                              ProcMesh.Taper(0.008f, 0.005f, 0.052f, 4, false, Color.white),
                              Mats.BoneMat,
                              new Vector3((n - 1.5f) * 0.017f, -0.265f, 0.004f),
                              Quaternion.Euler(Random.Range(165f, 195f), 0f, (n - 1.5f) * 8f),
                              Vector3.one, false);
        }

        // ---- HAI CHAN thong xuong, mui ban chan chuc ----
        for (int b = 0; b < 2; b++)
        {
            float ben = b == 0 ? 1f : -1f;

            var hong = new GameObject("Chan" + b);
            hong.transform.SetParent(treo.transform, false);
            hong.transform.localPosition = new Vector3(0.075f * ben, -0.72f, 0f);
            hong.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), 0f,
                                                            Random.Range(1f, 9f) * ben);
            tayChan.Add(hong.transform);

            Xuong(hong.transform, "XuongDui", Vector3.zero, Quaternion.Euler(180f, 0f, 0f), 0.34f, 0.025f);

            var goi = new GameObject("Goi");
            goi.transform.SetParent(hong.transform, false);
            goi.transform.localPosition = new Vector3(0f, -0.34f, 0f);
            goi.transform.localRotation = Quaternion.Euler(Random.Range(2f, 18f), 0f, 0f);
            tayChan.Add(goi.transform);

            Xuong(goi.transform, "XuongOng", Vector3.zero, Quaternion.Euler(180f, 0f, 0f), 0.32f, 0.019f);

            ProcMesh.Part("BanChan", goi.transform,
                          ProcMesh.Box(new Vector3(0.052f, 0.10f, 0.030f), Color.white), Mats.BoneMat,
                          new Vector3(0f, -0.365f, 0.018f),
                          Quaternion.Euler(Random.Range(-30f, -12f), Random.Range(-14f, 14f), 0f),
                          Vector3.one, true);
        }

        // ---- Gie rach ----
        for (int i = 0; i < 2; i++)
        {
            var gie = ProcMesh.Box(new Vector3(0.13f, Random.Range(0.22f, 0.38f), 0.010f), Color.white);
            ProcMesh.Bend(gie, Random.Range(60f, 120f), Vector3.right);
            var gieGo = ProcMesh.Part("Gie" + i, treo.transform, gie, GieRach,
                                      new Vector3(Random.Range(-0.10f, 0.10f),
                                                  Random.Range(-0.58f, -0.28f),
                                                  Random.Range(0.02f, 0.07f)),
                                      Quaternion.Euler(Random.Range(-10f, 10f),
                                                       Random.Range(-25f, 25f),
                                                       Random.Range(-12f, 12f)),
                                      Vector3.one, false);
            gieGo.GetComponent<Renderer>().sharedMaterial = Mats.TwoSided(GieRach);
        }

        // Treo lung lang thi du dua thoai mai, va xoay tron cham tren soi day
        GanCuDong(go, treo.transform, 6.5f, 11f, 0.34f, tayChan.ToArray(), soGo.transform);
        return go;
    }

    /// <summary>Gan bo cu dong cho mot cai xac treo.</summary>
    public static void GanCuDong(GameObject go, Transform than, float bienDoLac, float bienDoXoay,
                          float tocDo, Transform[] tayChan = null, Transform dau = null)
    {
        var lac = go.AddComponent<XacLacLu>();
        lac.than = than;
        lac.bienDoLac = bienDoLac;
        lac.bienDoXoay = bienDoXoay;
        lac.tocDoLac = tocDo;

        if (dau == null) dau = TimSau(than, "So");
        lac.dau = dau;
        lac.ham = dau != null ? TimSau(dau, "HamPivot") : null;

        if (tayChan == null)
        {
            var ds = new System.Collections.Generic.List<Transform>();
            foreach (Transform c in than)
                if (c.name.StartsWith("Tay") || c.name.StartsWith("Chan")
                    || c.name.StartsWith("Khuyu") || c.name.StartsWith("Goi"))
                    ds.Add(c);
            tayChan = ds.ToArray();
        }
        lac.tayChan = tayChan;
    }

    /// <summary>Tim mot doi tuong con theo ten, tim ca trong cac cap con chau.</summary>
    public static Transform TimSau(Transform goc, string ten)
    {
        if (goc == null) return null;
        if (goc.name == ten) return goc;
        foreach (Transform c in goc)
        {
            var kq = TimSau(c, ten);
            if (kq != null) return kq;
        }
        return null;
    }

    // ================================================================
    //  NAM KIEU MO
    // ================================================================

    /// <summary>Dung mot ngoi mo. <paramref name="kieu"/> tu 0 den 4.</summary>
    public static GameObject Build(Transform parent, int kieu, int seed)
    {
        Random.InitState(seed * 977 + 31);
        switch (((kieu % SoKieu) + SoKieu) % SoKieu)
        {
            case 0: return BiaDaNut(parent, seed);
            case 1: return ThanhGiaGay(parent, seed);
            case 2: return MoBatNap(parent, seed);
            case 3: return QuanTaiDa(parent, seed);
            default: return CocTreoSo(parent, seed);
        }
    }

    // ---- 0. BIA DA NUT ----------------------------------------------
    static GameObject BiaDaNut(Transform parent, int seed)
    {
        var go = new GameObject("Mo_BiaDaNut");
        if (parent != null) go.transform.SetParent(parent, false);

        float rong = Random.Range(0.62f, 0.80f);
        float cao = Random.Range(1.05f, 1.45f);

        // Tam bia nghieng han ve mot ben - dung thang thi khong ra ve bo hoang
        var bia = TamBia(rong, cao, 0.17f, true, seed, 0.022f);
        var biaGo = ProcMesh.Part("Bia", go.transform, bia, Mats.Rock,
                                  new Vector3(0f, -0.06f, 0f),
                                  Quaternion.Euler(Random.Range(-13f, -5f), Random.Range(-14f, 14f),
                                                   Random.Range(-9f, 9f)),
                                  Vector3.one, true);

        // KHE NUT chay doc than bia: mot lat den mong xen giua
        var khe = ProcMesh.Box(new Vector3(0.028f, cao * 0.82f, 0.20f), Color.white);
        ProcMesh.Bend(khe, 9f, Vector3.forward);
        ProcMesh.Part("KheNut", biaGo.transform, khe,
                      Mats.Solid("KheNutMo", new Color(0.035f, 0.030f, 0.032f), 0.04f, 0f),
                      new Vector3(rong * Random.Range(-0.18f, 0.18f), cao * 0.48f, 0f),
                      Quaternion.Euler(0f, 0f, Random.Range(-7f, 7f)), Vector3.one, false);

        // Thanh gia khac chim tren mat bia, da mo gan het
        var matKhac = Mats.Solid("KhacMo", new Color(0.20f, 0.155f, 0.125f), 0.05f, 0f);
        ProcMesh.Part("KhacDoc", biaGo.transform, ProcMesh.Box(new Vector3(0.075f, cao * 0.42f, 0.02f), Color.white),
                      matKhac, new Vector3(0f, cao * 0.60f, 0.093f), Quaternion.identity, Vector3.one, false);
        ProcMesh.Part("KhacNgang", biaGo.transform, ProcMesh.Box(new Vector3(0.30f, 0.075f, 0.02f), Color.white),
                      matKhac, new Vector3(0f, cao * 0.70f, 0.093f), Quaternion.identity, Vector3.one, false);

        // U dat truoc bia
        ProcMesh.Part("UDat", go.transform, UDat(0.52f, 0.20f, seed + 5), DatMo,
                      new Vector3(0f, -0.09f, 0.62f), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                      Vector3.one, true);

        // Vai khuc xuong tro len khoi u dat
        Xuong(go.transform, "Xuong1", new Vector3(0.16f, -0.02f, 0.80f),
              Quaternion.Euler(72f, Random.Range(0f, 360f), 18f), 0.30f, 0.030f);

        DaVun(go.transform, 5, 0.75f, seed + 11);
        return go;
    }

    // ---- 1. THANH GIA GAY -------------------------------------------
    static GameObject ThanhGiaGay(Transform parent, int seed)
    {
        var go = new GameObject("Mo_ThanhGiaGay");
        if (parent != null) go.transform.SetParent(parent, false);

        float cao = Random.Range(1.25f, 1.65f);

        // Coc doc, nghieng han
        var truc = new GameObject("ThanhGia");
        truc.transform.SetParent(go.transform, false);
        truc.transform.localRotation = Quaternion.Euler(Random.Range(9f, 19f), Random.Range(0f, 360f),
                                                        Random.Range(-15f, 15f));

        var doc = ProcMesh.Box(new Vector3(0.10f, cao, 0.085f), Color.white);
        ProcMesh.Displace(doc, 0.010f, 7f, seed);
        doc.RecalculateNormals();
        ProcMesh.Part("Doc", truc.transform, doc, GoMuc, new Vector3(0f, cao * 0.5f - 0.12f, 0f));

        // Tay ngang: mot ben dai, MOT BEN GAY CUT
        float benDai = Random.Range(0.30f, 0.40f);
        float benCut = Random.Range(0.09f, 0.15f);

        var tayDai = ProcMesh.Box(new Vector3(benDai, 0.085f, 0.075f), Color.white);
        ProcMesh.Displace(tayDai, 0.009f, 8f, seed + 3);
        tayDai.RecalculateNormals();
        ProcMesh.Part("TayDai", truc.transform, tayDai, GoMuc,
                      new Vector3(benDai * 0.5f, cao * 0.70f, 0f),
                      Quaternion.Euler(0f, 0f, Random.Range(-6f, 6f)), Vector3.one, true);

        var tayCut = ProcMesh.Crystal(0.055f, benCut, 5, 0.15f, Color.white);   // dau gay nham nhoi
        ProcMesh.Part("TayCut", truc.transform, tayCut, GoMuc,
                      new Vector3(-benCut * 0.5f, cao * 0.70f, 0f),
                      Quaternion.Euler(0f, 0f, 90f), Vector3.one, true);

        // Gie rach quan cho tay ngang, buong thong xuong
        var gie = ProcMesh.Box(new Vector3(0.11f, 0.42f, 0.012f), Color.white);
        ProcMesh.Bend(gie, 26f, Vector3.right);
        var gieGo = ProcMesh.Part("GieRach", truc.transform, gie, GieRach,
                                  new Vector3(benDai * 0.72f, cao * 0.62f, 0.02f),
                                  Quaternion.Euler(0f, Random.Range(-20f, 20f), Random.Range(-12f, 12f)),
                                  Vector3.one, false);
        gieGo.GetComponent<Renderer>().sharedMaterial = Mats.TwoSided(GieRach);

        // U dat + so nguoi dat duoi chan thanh gia
        ProcMesh.Part("UDat", go.transform, UDat(0.46f, 0.17f, seed + 7), DatMo,
                      new Vector3(0f, -0.10f, 0.10f), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                      Vector3.one, true);

        So(go.transform, "So", new Vector3(Random.Range(-0.28f, 0.28f), 0.02f, Random.Range(0.35f, 0.55f)),
           Quaternion.Euler(Random.Range(-25f, 10f), Random.Range(0f, 360f), Random.Range(-20f, 20f)), 0.115f);

        DaVun(go.transform, 4, 0.62f, seed + 13);
        return go;
    }

    // ---- 2. MO BAT NAP ----------------------------------------------
    static GameObject MoBatNap(Transform parent, int seed)
    {
        var go = new GameObject("Mo_BatNap");
        if (parent != null) go.transform.SetParent(parent, false);

        float dai = Random.Range(1.35f, 1.65f);
        float rong = Random.Range(0.68f, 0.82f);

        // LONG HUYET.
        //
        // Mat dat la mot khoi dac, khong khoet lo that duoc. Nen cai "ho" phai
        // dung SAT NAT: day ho dat NHINH TREN mat dat mot chut, roi dap mot vanh
        // dat cao han quanh mieng. Mat nguoi choi doc ra ngay la cai huyet dao do.
        // (Truoc do toi dat day ho o duoi mat dat -0,30m nen no bi mat dat che
        //  kin hoan toan, nhin ra bai phang chu khong ra cai mo.)
        var loDen = Mats.Solid("LongHuyet", new Color(0.012f, 0.010f, 0.014f), 0.02f, 0f);
        ProcMesh.Part("DayHuyet", go.transform,
                      ProcMesh.Box(new Vector3(rong, 0.06f, dai), Color.white), loDen,
                      new Vector3(0f, -0.01f, 0f), Quaternion.identity, Vector3.one, false);

        // Bon vach trong long huyet, doc xuong - tao chieu sau
        for (int i = 0; i < 4; i++)
        {
            bool doc = i < 2;
            float ben = (i % 2 == 0) ? 1f : -1f;
            Vector3 tai = doc ? new Vector3(rong * 0.5f * ben, 0.10f, 0f)
                              : new Vector3(0f, 0.10f, dai * 0.5f * ben);
            Vector3 co = doc ? new Vector3(0.05f, 0.24f, dai)
                             : new Vector3(rong, 0.24f, 0.05f);
            ProcMesh.Part("VachTrong" + i, go.transform, ProcMesh.Box(co, Color.white),
                          loDen, tai, Quaternion.identity, Vector3.one, false);
        }

        // Vanh dat dao len dap quanh mieng huyet - cao han han mat dat
        for (int i = 0; i < 4; i++)
        {
            bool doc = i < 2;
            float ben = (i % 2 == 0) ? 1f : -1f;
            Vector3 tai = doc ? new Vector3((rong * 0.5f + 0.15f) * ben, 0.06f, 0f)
                              : new Vector3(0f, 0.06f, (dai * 0.5f + 0.15f) * ben);
            Vector3 co = doc ? new Vector3(0.34f, 0.30f, dai + 0.62f)
                             : new Vector3(rong + 0.62f, 0.30f, 0.34f);

            var vien = ProcMesh.Box(co, Color.white);
            ProcMesh.Displace(vien, 0.055f, 3.4f, seed * 5 + i);
            vien.RecalculateNormals(); vien.RecalculateTangents();
            ProcMesh.Part("VanhDat" + i, go.transform, vien, DatMo, tai);
        }

        // NAP DA da bi day truot sang mot ben, nut doi
        var napGo = new GameObject("NapDa");
        napGo.transform.SetParent(go.transform, false);
        napGo.transform.localPosition = new Vector3(rong * Random.Range(0.95f, 1.25f), 0.22f,
                                                    dai * Random.Range(-0.20f, 0.20f));
        napGo.transform.localRotation = Quaternion.Euler(Random.Range(-9f, 9f), Random.Range(-28f, 28f),
                                                         Random.Range(-7f, 7f));

        var nap1 = ProcMesh.Box(new Vector3(rong * 0.55f, 0.13f, dai), Color.white);
        ProcMesh.Displace(nap1, 0.020f, 5f, seed + 21);
        nap1.RecalculateNormals(); nap1.RecalculateTangents();
        ProcMesh.Part("NapA", napGo.transform, nap1, Mats.Rock, new Vector3(-rong * 0.30f, 0f, 0f));

        var nap2 = ProcMesh.Box(new Vector3(rong * 0.48f, 0.13f, dai * 0.86f), Color.white);
        ProcMesh.Displace(nap2, 0.020f, 5f, seed + 22);
        nap2.RecalculateNormals(); nap2.RecalculateTangents();
        ProcMesh.Part("NapB", napGo.transform, nap2, Mats.Rock,
                      new Vector3(rong * 0.28f, -0.02f, dai * 0.10f),
                      Quaternion.Euler(0f, Random.Range(8f, 22f), Random.Range(3f, 11f)), Vector3.one, true);

        // BAN TAY XUONG cao len khoi mieng huyet - diem dang so nhat cua kieu mo nay
        var tayGo = new GameObject("TayCaoLen");
        tayGo.transform.SetParent(go.transform, false);
        tayGo.transform.localPosition = new Vector3(rong * Random.Range(-0.22f, 0.10f), 0.02f,
                                                    dai * Random.Range(-0.28f, 0.14f));
        tayGo.transform.localRotation = Quaternion.Euler(Random.Range(-22f, -8f),
                                                         Random.Range(0f, 360f), Random.Range(-14f, 14f));

        Xuong(tayGo.transform, "CangTay", Vector3.zero, Quaternion.Euler(6f, 0f, 4f), 0.46f, 0.036f);

        // Ban tay: long ban tay + nam ngon quap lai nhu dang bau viu
        ProcMesh.Part("LongTay", tayGo.transform,
                      ProcMesh.Sphere(0.075f, 10, 7, 0.58f, Color.white), Mats.BoneMat,
                      new Vector3(0f, 0.48f, 0f), Quaternion.identity, new Vector3(1.1f, 1f, 0.70f), true);

        for (int i = 0; i < 5; i++)
        {
            float t = (i - 2f) / 2f;
            // Cung ly do: ngon tay dai 0.18m, he so 40-75 chi quap duoc 10 do.
            var ngon = ProcMesh.Taper(0.020f, 0.010f, Random.Range(0.155f, 0.215f), 6, true, Color.white);
            ProcMesh.Bend(ngon, Random.Range(230f, 380f), Vector3.right);
            ProcMesh.Part("Ngon" + i, tayGo.transform, ngon, Mats.BoneMat,
                          new Vector3(t * 0.060f, 0.53f, 0.015f),
                          Quaternion.Euler(Random.Range(-38f, -14f), t * 30f, t * 20f),
                          Vector3.one, true);
        }

        DaVun(go.transform, 6, 1.05f, seed + 17);
        return go;
    }

    // ---- 3. QUAN TAI DA ---------------------------------------------
    static GameObject QuanTaiDa(Transform parent, int seed)
    {
        var go = new GameObject("Mo_QuanTaiDa");
        if (parent != null) go.transform.SetParent(parent, false);

        float dai = Random.Range(1.75f, 2.05f);
        float rong = Random.Range(0.72f, 0.86f);
        float cao = Random.Range(0.52f, 0.64f);

        // Be do
        var be = ProcMesh.Box(new Vector3(rong + 0.26f, 0.14f, dai + 0.26f), Color.white);
        ProcMesh.Displace(be, 0.018f, 4.5f, seed);
        be.RecalculateNormals(); be.RecalculateTangents();
        ProcMesh.Part("Be", go.transform, be, Mats.Rock, new Vector3(0f, -0.02f, 0f));

        // THAN HOM KHOET RONG.
        //
        // Truoc day than hom la mot khoi DAC, nen du co day nap ra cung chi thay
        // mat da o duoi. Gio dung day + bon vach, long hom rong that, day nap ra
        // la nhin thau vao trong.
        float dayVach = 0.10f;

        // Day hom phai DAY, cho bo xuong nam CAO gan mieng hom.
        // Neu de day mong (0.10) thi bo xuong tut sau xuong duoi, bi thanh hom che
        // gan het - nhin tu goc choi 48 do chi thay long hom trong khong.
        float dayNen = cao * 0.62f;
        float matNen = 0.05f + dayNen;                 // do cao mat tren cua nen

        var day2 = ProcMesh.Box(new Vector3(rong, dayNen, dai), Color.white);
        ProcMesh.Displace(day2, 0.014f, 4f, seed + 1);
        day2.RecalculateNormals(); day2.RecalculateTangents();
        ProcMesh.Part("DayHom", go.transform, day2, Mats.Rock,
                      new Vector3(0f, 0.05f + dayNen * 0.5f, 0f));

        for (int i = 0; i < 4; i++)
        {
            bool doc = i < 2;
            float ben = (i % 2 == 0) ? 1f : -1f;
            Vector3 tai = doc
                ? new Vector3((rong * 0.5f - dayVach * 0.5f) * ben, cao * 0.5f + 0.10f, 0f)
                : new Vector3(0f, cao * 0.5f + 0.10f, (dai * 0.5f - dayVach * 0.5f) * ben);
            Vector3 co = doc ? new Vector3(dayVach, cao, dai)
                             : new Vector3(rong, cao, dayVach);

            var vach = ProcMesh.Box(co, Color.white);
            ProcMesh.Displace(vach, 0.020f, 4f, seed + 40 + i);
            vach.RecalculateNormals(); vach.RecalculateTangents();
            ProcMesh.Part("Vach" + i, go.transform, vach, Mats.Rock, tai);
        }

        // BO XUONG NGUOI nam trong long hom
        BoXuongNam(go.transform, rong - dayVach * 2f, dai - dayVach * 2f, matNen + 0.01f, seed + 60);

        // NAP DAY HE RA O DAU TREN.
        // Nap chi con day duoc nua duoi (phia chan), nua tren ha mieng cho thay
        // cai so va long nguc ben trong.
        float phanDay = Random.Range(0.50f, 0.62f);          // bao nhieu phan hom con bi day
        float truotDoc = dai * (1f - phanDay) * 0.5f;

        var napChinh = ProcMesh.Box(new Vector3(rong + 0.05f, 0.12f, dai * phanDay), Color.white);
        ProcMesh.Displace(napChinh, 0.018f, 5f, seed + 30);
        napChinh.RecalculateNormals(); napChinh.RecalculateTangents();
        ProcMesh.Part("NapChinh", go.transform, napChinh, Mats.Rock,
                      new Vector3(Random.Range(-0.05f, 0.05f), cao + 0.16f, -truotDoc),
                      Quaternion.Euler(Random.Range(-3f, 1f), Random.Range(-4f, 4f), Random.Range(-3f, 3f)),
                      Vector3.one, true);

        // Manh nap vo, truot han ra ngoai dua vao suon hom
        var napVo = ProcMesh.Box(new Vector3(rong * 0.62f, 0.11f, dai * 0.34f), Color.white);
        ProcMesh.Displace(napVo, 0.020f, 5f, seed + 31);
        napVo.RecalculateNormals(); napVo.RecalculateTangents();
        ProcMesh.Part("NapVo", go.transform, napVo, Mats.Rock,
                      new Vector3(rong * Random.Range(0.72f, 0.95f), cao * 0.42f, dai * 0.30f),
                      Quaternion.Euler(Random.Range(-14f, -4f), Random.Range(-30f, 30f),
                                       Random.Range(38f, 62f)),
                      Vector3.one, true);

        // Anh ma xanh hat len tu trong long hom dang ha mieng
        DomMa(go.transform, new Vector3(rong * 0.30f, cao * 0.95f, dai * 0.34f), 0.75f);

        // XICH SAT vat ngang than hom - khoa cai gi do lai ben trong.
        //
        // Truoc day toi xep 14 khoen theo mot vong tron, ket qua ra mot vach trang
        // thang bang cat ngang nap - vua sai vua choi mat. Gio lam dung kieu day
        // xich that: mot doan vat qua NAP, hai doan tut xuong hai SUON hom.
        var matXich = Mats.Solid("XichSat", new Color(0.135f, 0.130f, 0.125f), 0.30f, 0.55f);
        float dinhNap = cao + 0.22f;

        for (int v = 0; v < 2; v++)
        {
            // Ca hai day xich deu nam o NUA DUOI - nua tren nap da bat ra roi,
            // vat xich qua cho trong khong thi vo ly.
            float z = dai * (v == 0 ? -0.34f : -0.12f);

            // Doan vat qua nap
            DoanXich(go.transform, "XichTren" + v, matXich,
                     new Vector3(0f, dinhNap, z), new Vector3(rong * 1.06f, 0.055f, 0.075f), 5);

            // Hai doan tut xuong hai suon
            for (int b = 0; b < 2; b++)
            {
                float ben = b == 0 ? 1f : -1f;
                DoanXich(go.transform, "XichBen" + v + "_" + b, matXich,
                         new Vector3(rong * 0.53f * ben, cao * 0.55f, z),
                         new Vector3(0.075f, cao * 0.95f, 0.075f), 4);
            }
        }

        // So nguoi khac o dau hom, phia CHAN (dau kia dang ha mieng)
        So(go.transform, "SoKhac", new Vector3(0f, cao * 0.62f, -dai * 0.5f - 0.04f),
           Quaternion.Euler(0f, 180f, 0f), 0.13f);

        DaVun(go.transform, 5, 1.25f, seed + 19);
        return go;
    }

    // ---- 4. COC TREO SO ---------------------------------------------
    static GameObject CocTreoSo(Transform parent, int seed)
    {
        var go = new GameObject("Mo_CocTreoSo");
        if (parent != null) go.transform.SetParent(parent, false);

        float cao = Random.Range(1.55f, 1.95f);

        var cocGo = new GameObject("Coc");
        cocGo.transform.SetParent(go.transform, false);
        cocGo.transform.localRotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f),
                                                         Random.Range(-11f, 11f));

        var coc = ProcMesh.Taper(0.075f, 0.052f, cao, 8, true, Color.white);
        ProcMesh.Displace(coc, 0.012f, 6f, seed);
        coc.RecalculateNormals();
        ProcMesh.Part("Than", cocGo.transform, coc, GoMuc, new Vector3(0f, -0.14f, 0f));

        // Thanh ngang buoc chao cho so treo len
        var ngang = ProcMesh.Taper(0.038f, 0.030f, Random.Range(0.55f, 0.72f), 6, true, Color.white);
        ProcMesh.Part("Ngang", cocGo.transform, ngang, GoMuc,
                      new Vector3(0f, cao * 0.78f, 0f),
                      Quaternion.Euler(0f, Random.Range(0f, 180f), 90f), Vector3.one, true);

        // BA CAI SO treo lung lang, moi cai mot do cao khac nhau
        int soLuong = Random.Range(2, 4);
        for (int i = 0; i < soLuong; i++)
        {
            float t = soLuong == 1 ? 0.5f : i / (float)(soLuong - 1);
            float lech = Mathf.Lerp(-0.24f, 0.24f, t);
            float dayDay = Random.Range(0.14f, 0.30f);

            // Soi day
            var day = ProcMesh.Taper(0.016f, 0.016f, dayDay, 5, false, Color.white);
            ProcMesh.Part("Day" + i, cocGo.transform, day, GieRach,
                          new Vector3(lech, cao * 0.78f - dayDay, 0f));

            So(cocGo.transform, "So" + i,
               new Vector3(lech, cao * 0.78f - dayDay - 0.115f, 0f),
               Quaternion.Euler(Random.Range(6f, 26f), Random.Range(0f, 360f), Random.Range(-22f, 22f)),
               Random.Range(0.105f, 0.135f));
        }

        // U dat + long nguc tro len khoi dat
        ProcMesh.Part("UDat", go.transform, UDat(0.58f, 0.19f, seed + 9), DatMo,
                      new Vector3(0f, -0.10f, 0.05f), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                      Vector3.one, true);

        var suon = new GameObject("LongNguc");
        suon.transform.SetParent(go.transform, false);
        suon.transform.localPosition = new Vector3(Random.Range(-0.18f, 0.18f), 0.02f, Random.Range(0.30f, 0.50f));
        suon.transform.localRotation = Quaternion.Euler(Random.Range(58f, 78f), Random.Range(0f, 360f), 0f);

        for (int i = 0; i < 5; i++)
        {
            var xuongSuon = ProcMesh.Taper(0.011f, 0.008f, 0.20f + i * 0.015f, 7, false, Color.white);
            ProcMesh.Bend(xuongSuon, 400f, Vector3.right);
            for (int b = 0; b < 2; b++)
            {
                float ben = b == 0 ? 1f : -1f;
                ProcMesh.Part("Suon" + i + "_" + b, suon.transform, xuongSuon, Mats.BoneMat,
                              new Vector3(0.012f * ben, i * 0.052f, 0f),
                              Quaternion.Euler(0f, 0f, 68f * ben), Vector3.one, true);
            }
        }

        DaVun(go.transform, 4, 0.70f, seed + 23);
        return go;
    }
}
