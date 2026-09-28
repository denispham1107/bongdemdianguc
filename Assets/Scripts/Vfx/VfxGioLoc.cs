using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "GIO LOC" - DUNG LAI BANG BLENDER MCP (17/09/2026).
///
/// Nguoi dung: ban loc nho cu (Loc xoay thu nho nhuom nau) "qua xau"; xin dung lai bang Blender MCP cho thay gio loc
/// xoay that chi tiet, XOAY MOT CHIEU TU DUOI LEN, kem KHOI BUI DEN cuon bay len, mau nhu Loc xoay (xam trang);
/// bo tia set. Nguoi dung chon: cao ~5 m; khoi bui vua cuon quanh than vua de lai vet phia sau.
///
/// Tai nguyen dung trong Blender (CongCu/Blender/gio_loc.blend), nam o Resources/KyNang/GioLoc:
///   - LocNho.fbx   : Vo0, Vo1, Vo2 (ba vo phieu cao 5 m, loe mieng 2,6 m, UV quan quanh than, mau dinh xam trang;
///                    CHAN TO THEM 40% (so ban goc) roi THEM 20% nua tren ban ay (x1,68 so goc), nho dan ve 0 o 2,3 m - nguoi dung khoanh do phan than duoi 17/09/2026
///                    mo o chan va mieng) + DaiGio (5 dai gio xoan 1,6 vong, hai mieng bat cheo).
///   - GioDai.png   : dai gio mem nghieng, LIEN MACH ca u va v (nhieu 4D quan tren mat xuyen) - vo trong.
///   - GioSoi.png   : soi gio manh, thua - vo ngoai va dai gio.
///   - BuiDenCuon.png : flipbook 6x6 dam bui den cuon meo no dan.
///
/// MOT CHIEU TU DUOI LEN: do tren luoi da nhap (menu 71): vet gio tren anh VA dai gio cung "goc atan2(z,x) GIAM khi len
/// cao". Quay lam goc TANG thi moi vet chay len tren (nhu cot den cat toc). Transform.Rotate quanh +Y voi goc DUONG lam
/// atan2(z,x) GIAM -> moi thu quay AM (<see cref="ChieuQuayGioLoc"/>). Anh truot v AM cung day vet len. Khong co lop nao
/// quay nguoc (Loc xoay lon co vo quay nguoc chieu nhau - chinh la thu lam no xoay "hai chieu").
/// </summary>
public static partial class VfxFactory
{
    const string ThuMucGioLoc = "KyNang/GioLoc/";

    /// <summary>Dau cua do/giay cho Spin quanh +Y: AM = vet gio di len (do menu 71).</summary>
    public const float ChieuQuayGioLoc = -1f;

    /// <summary>Dau cua van toc quy dao hat quanh +Y de hat cuon CUNG CHIEU voi than loc (do menu 71).</summary>
    public const float ChieuQuyDaoGioLoc = -1f;

    /// <summary>Toan bo ban kinh loc (vo, dai gio, vong bui, vet bui, hat dat) nhan them - nguoi dung 17/09/2026: to them 10%.
    /// Chieu cao giu 5 m.</summary>
    public const float HeSoBanKinhGioLoc = 1.1f;

    static GameObject khoLocNho;
    static bool daTimLocNho;
    static Material mGioDai, mGioSoi, mBuiDenCuon;

    static GameObject KhoLocNho
    {
        get
        {
            if (!daTimLocNho) { daTimLocNho = true; khoLocNho = Resources.Load<GameObject>(ThuMucGioLoc + "LocNho"); }
            return khoLocNho;
        }
    }

    public static Material BuiDenCuonMat
    {
        get
        {
            if (mBuiDenCuon == null)
            {
                var t = Resources.Load<Texture2D>(ThuMucGioLoc + "BuiDenCuon");
                if (t == null) return null;
                mBuiDenCuon = Mats.FlipbookAlpha("P_BuiDenCuon", t, Color.white);
            }
            return mBuiDenCuon;
        }
    }

    static Material VatLieuGio(ref Material kho, string ten)
    {
        if (kho == null)
        {
            var t = Resources.Load<Texture2D>(ThuMucGioLoc + ten);
            kho = Mats.Alpha("P_" + ten, t != null ? t : TextureFactory.TornadoWall(), Color.white);
        }
        return kho;
    }

    /// <summary>Dung hinh mot con loc nho. Goc = chan loc tren mat dat.</summary>
    public static GameObject BuildGioLoc()
    {
        var root = new GameObject("GioLocHinh");
        var kho = KhoLocNho;
        if (kho != null)
        {
            foreach (var mf in kho.GetComponentsInChildren<MeshFilter>(true))
            {
                // (ten, anh, mau vo, do/giay, truot UV) - vo trong day va nhanh, ra ngoai mong va cham dan.
                // Hai vo ngoai MO (0,34 / 0,26): de 0,55 / 0,42 thi ba lop chong nhau thanh mot man suong trang deu, mat het
                // soi gio (anh gioloc_can_zoom 17/09/2026). Dai gio dam 0,9 de doc ra duong xoan.
                Material goc; Color mau; float quay; Vector2 truot;
                switch (mf.name)
                {
                    case "Vo0":    goc = VatLieuGio(ref mGioDai, "GioDai"); mau = new Color(0.82f, 0.84f, 0.88f, DoDucVoGioLoc[0]); quay = 330f; truot = new Vector2(0f, -0.95f); break;
                    case "Vo1":    goc = VatLieuGio(ref mGioDai, "GioDai"); mau = new Color(0.90f, 0.92f, 0.95f, DoDucVoGioLoc[1]); quay = 250f; truot = new Vector2(0f, -0.70f); break;
                    case "Vo2":    goc = VatLieuGio(ref mGioSoi, "GioSoi"); mau = new Color(0.96f, 0.97f, 1.00f, DoDucVoGioLoc[2]); quay = 180f; truot = new Vector2(0f, -0.50f); break;
                    default:       goc = VatLieuGio(ref mGioSoi, "GioSoi"); mau = new Color(1.00f, 1.00f, 1.00f, DoDucVoGioLoc[3]); quay = 400f; truot = new Vector2(-1.1f, 0f); break;
                }
                var go = new GameObject(mf.name);
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = mf.transform.localPosition;
                go.transform.localRotation = mf.transform.localRotation;
                go.transform.localScale = Vector3.Scale(mf.transform.localScale, new Vector3(HeSoBanKinhGioLoc, 1f, HeSoBanKinhGioLoc));
                go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                var mr = go.AddComponent<MeshRenderer>();
                var m = new Material(goc);
                m.SetColor("_TintColor", mau);
                mr.sharedMaterial = m;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
                var sp = go.AddComponent<Spin>();
                sp.axis = Vector3.up;
                sp.degreesPerSecond = ChieuQuayGioLoc * quay;
                // Dai gio: u = do cao x 2 nen truot u AM la day soi gio len doc dai
                var sc = go.AddComponent<ScrollUV>();
                sc.speed = truot;
            }
        }

        // BUI XAM CUON QUANH CHAN - KIEU LOC XOAY (nguoi dung 28/09/2026: khoi bui den "cho cung mau cung hieu ung giong skill Loc
        // xoay"; truoc la khoi DEN BuiDenCuon). Ten giu "BuiCuon" (menu 71 doc). Co / vong theo chan MOI (x2): chan Vo1 1,11 m so voi
        // vo chinh Loc xoay 1,3 m -> co 0,85; bay len x0,5 vi loc chi cao 5 m. Cuc bo de om chan khi loc bay 9,5 m/s.
        BuiXamChanLoc(root.transform, "BuiCuon", new Vector3(0f, 0.2f, 0f), CoBuiGioLoc, BanKinhVongBuiGioLoc, 0.5f, true);

        // Vet bui o lai phia sau duong loc di (khong gian the gioi) - nay cung BUI XAM cua Loc xoay (truoc: khoi den)
        var vet = BuildKhoiBuiLoc(root.transform, 0.55f * HeSoBanKinhGioLoc * 2f);
        vet.GetComponent<ParticleSystemRenderer>().sharedMaterial = BuiXamMat;
        DatKhungBuiXam(vet);
        var vm = vet.main; vm.maxParticles = 110;
        vm.startColor = new ParticleSystem.MinMaxGradient(MauBuiXamToi, MauBuiXamSang);
        var vcol = vet.colorOverLifetime; vcol.enabled = true;
        vcol.color = new ParticleSystem.MinMaxGradient(Grad(Color.white, 0f, Color.white, 0.5f, new Color(0.85f, 0.86f, 0.9f), 1f, 0f, 0.7f, 0.45f, 0f));
        var vem = vet.emission; vem.rateOverTime = 20f;
        var vv = vet.velocityOverLifetime;
        vv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 2.2f, ChieuQuyDaoGioLoc * 4.0f);

        // Hat dat cat li ti bi hut quay quanh than
        BuildDebrisSwarm(root.transform, 0.90f * HeSoBanKinhGioLoc, "Grit", 0.04f, 0.13f, 50f, 110, 2.2f, 4.6f);   // chan x2 -> vong cat x2
        var grit = root.transform.Find("Grit").GetComponent<ParticleSystem>();
        var gv = grit.velocityOverLifetime;
        gv.orbitalY = new ParticleSystem.MinMaxCurve(ChieuQuyDaoGioLoc * 7f, ChieuQuyDaoGioLoc * 11f);
        return root;
    }

    /// <summary>So nhip set trong loc da phong (moi nhip 2 tia) - phep thu menu 71 doc.</summary>
    public static int SoNhipSetTrongGioLoc;

    /// <summary>Moi dau tia set trong loc lech truc toi da 0,6 m -> hai tia doi dien cach nhau ~1,2 m.</summary>
    public const float LechTiaSetGioLoc = 0.6f;

    /// <summary>Dau tia khong qua ti le nay cua ban kinh vo trong cung o cung do cao (con nam trong long loc).</summary>
    public const float TiLeTrongVoGioLoc = 0.85f;

    // Ban kinh vo TRONG CUNG Vo0 cua LocNho.fbx moi 0,5 m do cao (chua nhan HeSoBanKinhGioLoc). 28/09/2026 THAN TO RA (nguoi
    // dung: "nhin loc nhu cay kem oc que"): chan x2 (0,413 -> 0,827), to dan deu r = 0,826 + 1,019 (z/5)^1,6, mieng giu 1,845 -
    // sua trong Blender (CongCu/Blender/gio_loc_than_rong.blend). Bang cu 17/09: 0,41 0,42 0,445 0,48 0,53 0,67 0,87 1,05 1,30 1,54 1,85.
    static readonly float[] banKinhVo0 = { 0.827f, 0.852f, 0.904f, 0.975f, 1.062f, 1.163f, 1.276f, 1.402f, 1.539f, 1.687f, 1.845f };

    /// <summary>Bui xam chan Gio loc: co hat (so voi Loc xoay) va ban kinh vong phun (m) - theo chan moi.</summary>
    public const float CoBuiGioLoc = 0.85f, BanKinhVongBuiGioLoc = 1.0f;

    /// <summary>
    /// Do duc (alpha _TintColor) Vo0, Vo1, Vo2, DaiGio. 28/09/2026 than to gap doi thi ba lop chong thanh khoi TRANG DAC, che het bui -
    /// nguoi dung: "cho than trong hon de thay bui ben trong". Truoc: 0,72 / 0,34 / 0,26 / 0,90 (menu 71c giu lam doi chung).
    /// </summary>
    public static readonly float[] DoDucVoGioLoc = { 0.42f, 0.20f, 0.16f, 0.70f };

    /// <summary>Ban kinh vo trong cung (da nhan 1,1) o do cao y tinh tu chan loc.</summary>
    public static float BanKinhVoTrongGioLoc(float y)
    {
        float f = Mathf.Clamp(y / 0.5f, 0f, banKinhVo0.Length - 1.001f);
        int i = Mathf.FloorToInt(f);
        return Mathf.Lerp(banKinhVo0[i], banKinhVo0[i + 1], f - i) * HeSoBanKinhGioLoc;
    }

    /// <summary>Chieu cao Loc xoay lon do duoc (menu 71, 17/09/2026: 15,37 m) - lay ti le thu nho tia set cho Gio loc cao 5 m.</summary>
    public const float ChieuCaoLocXoayDo = 15.37f;

    /// <summary>
    /// HAI TIA SET TRONG LONG GIO LOC, danh tu DINH loc xuong (nguoi dung 17/09/2026: "luon cho 2 tia set xuat hien trong loc,
    /// xuat phat tu tren dinh loc danh xuong, giong 2 tia set cua Loc xoay, nhung kich thuoc phu hop voi loc nho"). Chep cach
    /// dung tia cua <see cref="TornadoBolt"/> (do day 0,85 / 0,70, doi 0,16-0,28 / 0,13-0,22 s, 16 / 22 doan, giat 1,5 / 1,05,
    /// nhanh 1-2 / 2-3) nhung: CA HAI tia deu tu dinh xuong va nam gan truc; BE DAY nhan ti le chieu cao 5 / 15,37 (do giat va
    /// nhanh la ti le theo do dai tia nen tu nho theo). CHI HINH, khong sat thuong. Goi moi 0,45 s nhu Loc xoay.
    /// </summary>
    public static void GioLocSetTrongLoc(Transform loc)
    {
        Vector3 chan = loc.position;
        SoNhipSetTrongGioLoc++;
        float k = GioLoc.ChieuCao / ChieuCaoLocXoayDo;
        float goc = Random.Range(0f, Mathf.PI * 2f);
        // Hai tia o HAI PHIA DOI DIEN truc, cung goc xoan khi di xuong -> luon doi dien nhau (nguoi dung 17/09/2026: hai tia "gan
        // sat nhau qua", xin cach ~1,2 m nhung van trong long loc). Moi dau tia lech truc LechTiaSetGioLoc (0,6 m), nhung khong qua
        // TiLeTrongVoGioLoc x ban kinh vo TRONG CUNG o do cao do - chan loc hep (0,46 m) nen doan duoi tu thu vao.
        float xoan = Random.Range(0.8f, 1.8f);                 // xuong thap thi lech goc - tia nghieng theo chieu xoay
        for (int i = 0; i < 2; i++)
        {
            float a1 = goc + i * Mathf.PI;
            float a2 = a1 + xoan;
            float yDinh = GioLoc.ChieuCao * Random.Range(0.86f, 0.96f);
            float yDuoi = i == 0 ? Random.Range(0.3f, 1.2f) : Random.Range(1.0f, 2.2f);
            float r1 = Mathf.Min(LechTiaSetGioLoc, TiLeTrongVoGioLoc * BanKinhVoTrongGioLoc(yDinh));
            float r2 = Mathf.Min(LechTiaSetGioLoc, TiLeTrongVoGioLoc * BanKinhVoTrongGioLoc(yDuoi));
            Vector3 dinh = chan + new Vector3(Mathf.Cos(a1) * r1, yDinh, Mathf.Sin(a1) * r1);
            Vector3 duoi = chan + new Vector3(Mathf.Cos(a2) * r2, yDuoi, Mathf.Sin(a2) * r2);
            var arc = i == 0
                ? LightningArc.Create(dinh, duoi, 0.85f * k, Random.Range(0.16f, 0.28f))
                : LightningArc.Create(dinh, duoi, 0.70f * k, Random.Range(0.13f, 0.22f));
            arc.name = "SetTrongGioLoc";
            // Tia BAM THEO loc (nguoi dung 17/09/2026: tia "luon bi bo lai phia sau" - loc 9,5 m/s di mat 1,2-2,7 m trong doi tia)
            arc.BamTheo(loc);
            arc.segments = i == 0 ? 16 : 22;
            arc.jitter = i == 0 ? 1.5f : 1.05f;
            arc.branches = i == 0 ? Random.Range(1, 3) : Random.Range(2, 4);
        }
    }
}
