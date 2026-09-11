using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// THANH THONG TIN: bang trang thai o GOC TRAI TREN (ten nhan vat, thanh mau,
/// thanh khieng, thanh nang luong), thanh ky nang o giua duoi, thong bao.
/// Ve bang OnGUI nen khong can cai them package UI nao.
/// </summary>
public class GameHUD : MonoBehaviour
{
    public PlayerController player;
    public Damageable playerHealth;
    public CameraRig cameraRig;
    public GameDirector director;

    [Header("Huong dan dau man")]
    // 0 = KHONG hien bang huong dan trong luc choi. Cach choi da co san o
    // man hinh chinh, khong can lap lai giua san va che mat tran dau.
    public float hintDuration = 0f;

    const float Ref = 1080f;             // thiet ke theo man hinh cao 1080

    Texture2D iconFire, iconIce, iconBolt, iconWind, iconMeteor, iconKhieng, iconGiatSet;
    Texture2D gradMau, gradMana, gradKhieng, gradKhiengYeu;
    Texture2D vongNen, vongNum, vanhNut;

    [Header("Cam ung (web / iOS / Android)")]
    [Tooltip("Bat tay de THU giao dien cam ung ngay tren PC")]
    public bool epCamUng = false;

    // Can joystick: tam, ban kinh, va ngon tay dang giu no
    int ngonJoy = -1;                 // -1 = khong ai giu; -2 = dang giu bang chuot

    /// <summary>
    /// Ngon dang giu nut "Tro ve" tren man hinh thua. -99 = khong ai giu,
    /// -2 = dang giu bang chuot (de thu tren PC).
    /// </summary>
    int ngonTroVe = -99;
    Vector2 diemJoy;                  // vi tri ngon tay tren man hinh

    // Ngon dang bam nut. Nho lai de ngon do KHONG bi tinh la keo camera: bam
    // ky nang bao gio cung keo le di mot ti, khong nho thi moi cu bam deu lam
    // goc nhin giat mot cai.
    readonly List<int> ngonNut = new List<int>();

    // ---- NGAM KY NANG: an giu mot nut roi keo ra ----
    //
    // -99 = khong ai ngam, -2 = dang ngam bang chuot (de thu tren PC).
    //
    // Ngon ngam CUNG nam trong 'ngonNut', nen no khong bao gio bi tinh la cu
    // chi xoay camera - dung yeu cau "khong duoc doi goc nhin trong luc keo".
    int ngonNgam = -99;

    /// <summary>
    /// May trang thai cua lan ngam dang dien ra. Ca duong CHAM lan duong CHUOT
    /// deu goi vao day - viet hai lan thi sua mot ben quen ben kia.
    /// </summary>
    readonly MayNgam mayNgam = new MayNgam();

    ChiBaoNgam chiBao;                // duong ngam ve tren mat dat

    float chumTruoc;                  // khoang cach hai ngon o khung truoc (pinch)
    bool chuotXoay;                   // duong chuot tren PC: dang keo de xoay camera
    Vector2 chuotTruoc;
    Texture2D panelTex, barTex;
    GUIStyle bigStyle, midStyle, smallStyle, keyStyle;
    GUIStyle soGiayStyle;             // so giay hoi chieu giua nut
    float startTime;

    /// <summary>
    /// Bang chan doan goc trai tren. Bat/tat bang phim F12.
    ///
    /// De o day de khi game "khong an phim" thi nhin mot cai la biet ngay hong o
    /// dau, khong phai doan mo: so vong Update co tang khong, phim co toi duoc
    /// game khong, nhan vat con song khong.
    /// </summary>
    // MAC DINH TAT. Day la bang go loi, khong phai thu nguoi choi can thay.
    // Van bat lai duoc bang F12 khi can xem game co "an phim" khong.
    public bool hienChanDoan = false;

    void Update()
    {
        // Dat co o DAY chu khong trong OnGUI. DocCamUng ngay ben duoi doc
        // CamUng.DangDung, ma Update chay TRUOC OnGUI - dat trong OnGUI thi ca
        // khung hinh dau tien chay bang gia tri cu, va neu HUD chua kip ve lan
        // nao (nhu trong Editor) thi co nay ket lai o gia tri cu mai.
        // F9: doi qua lai PC <-> cam ung NGAY TRONG LUC CHOI.
        //
        // Khong phai thoat Play, tim GameHUD trong canh, tick o roi bam Play
        // lai - chi de xem mot cai nut nam dung cho chua.
        if (Input.GetKeyDown(KeyCode.F9))
        {
            epCamUng = !epCamUng;
            Say(epCamUng ? "CAM UNG: joystick + nut tron"
                         : "PC: thanh ky nang vuong");
        }

        CamUng.EpBat = epCamUng;

        if (Input.GetKeyDown(KeyCode.F12)) hienChanDoan = !hienChanDoan;
        DocCamUng();
        CapNhatChiBao();
    }

    /// <summary>
    /// Ve (hoac xoa) duong ngam tren mat dat theo trang thai dang giu nut.
    ///
    /// Dat trong Update chu khong trong OnGUI: OnGUI chay nhieu lan moi khung,
    /// dung lai ca mot luoi mesh o do la lam viec ay ba bon lan mot khung.
    /// </summary>
    void CapNhatChiBao()
    {
        bool ve = CamUng.DangDung && player != null
                  && mayNgam.DangNgam && mayNgam.DaRaKhoiNut;

        if (!ve)
        {
            if (chiBao != null) chiBao.An();
            return;
        }

        // Dung MUON: tren PC khong cam ung thi khong bao gio can den no, va
        // dung san trong Start thi F9 bat cam ung giua chung se khong co.
        if (chiBao == null) chiBao = ChiBaoNgam.Tao(null);

        int ky = mayNgam.KyNang;
        Vector3 huong = player.HuongTuManHinh(mayNgam.Lech);
        float xa01 = mayNgam.Xa01;
        Vector3 chan = player.transform.position;

        chiBao.Hien(ky, chan, huong,
                    player.DiemNgam(ky, huong, xa01),
                    player.TamNgam(ky),
                    player.BanKinhSatThuong(ky),
                    player.DonThang(ky));
    }

    /// <summary>
    /// Keo xa bao nhieu diem anh thi tinh la het tam.
    ///
    /// 300 chon theo NGON CAI, khong theo man hinh: ngon cai voi tu goc phai
    /// duoi ra duoc chung do truoc khi phai nhac ca ban tay len. De xa hon thi
    /// khong ai ngam duoc toi tam toi da.
    /// </summary>
    float KeoToiDa(float s) { return 300f * s; }

    void Start()
    {
        startTime = Time.time;

        // Dai mau doc cho tung thanh - xem GradientDoc
        gradMau = GradientDoc(new Color(1.00f, 0.42f, 0.34f),
                              new Color(0.78f, 0.08f, 0.07f),
                              new Color(0.42f, 0.03f, 0.03f));
        gradMana = GradientDoc(new Color(0.55f, 0.82f, 1.00f),
                               new Color(0.10f, 0.32f, 0.85f),
                               new Color(0.04f, 0.13f, 0.45f));
        gradKhieng = GradientDoc(new Color(0.85f, 0.62f, 1.00f),
                                 new Color(0.55f, 0.24f, 0.92f),
                                 new Color(0.28f, 0.10f, 0.52f));
        gradKhiengYeu = GradientDoc(new Color(1.00f, 0.72f, 0.35f),
                                    new Color(0.95f, 0.34f, 0.14f),
                                    new Color(0.48f, 0.12f, 0.04f));
        iconFire = IconKyNang.Lua();
        iconIce = IconKyNang.Bang();
        iconBolt = IconKyNang.Set();
        iconWind = IconKyNang.Loc();
        iconMeteor = IconKyNang.ThienThach();
        iconKhieng = IconKyNang.Khieng();
        iconGiatSet = IconKyNang.GiatSet();
        vongNen = VongTron(96, new Color(1f, 1f, 1f, 1f), 0.86f, 0.10f);

        // VANH mong, khong phai dia. vongNen o tren long trong chi 0,10 - gan
        // nhu dac - nen to mau len no la phu mot lop kem len KIN mat nut, lam
        // moi bieu tuong deu ra mot mau xam nhu nhau.
        vanhNut = VongTron(128, new Color(1f, 1f, 1f, 1f), 1.00f, 0.86f);
        vongNum = VongTron(64, new Color(1f, 1f, 1f, 1f), 1f, 0f);
        panelTex = SolidTexture(new Color(0f, 0f, 0f, 0.55f));
        barTex = SolidTexture(new Color(1f, 1f, 1f, 1f));
    }

    void EnsureStyles(float s)
    {
        if (bigStyle == null)
        {
            bigStyle = new GUIStyle(GUI.skin.label);
            midStyle = new GUIStyle(GUI.skin.label);
            smallStyle = new GUIStyle(GUI.skin.label);
            keyStyle = new GUIStyle(GUI.skin.label);
        }
        bigStyle.fontSize = Mathf.RoundToInt(46f * s);
        bigStyle.fontStyle = FontStyle.Bold;
        bigStyle.alignment = TextAnchor.MiddleCenter;
        bigStyle.normal.textColor = new Color(1f, 0.85f, 0.55f);

        midStyle.fontSize = Mathf.RoundToInt(24f * s);
        midStyle.normal.textColor = new Color(0.95f, 0.92f, 0.85f);

        smallStyle.fontSize = Mathf.RoundToInt(18f * s);
        smallStyle.normal.textColor = new Color(0.85f, 0.82f, 0.75f);

        keyStyle.fontSize = Mathf.RoundToInt(16f * s);
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.alignment = TextAnchor.UpperCenter;
        keyStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
    }

    void OnGUI()
    {
        float s = Screen.height / Ref;
        EnsureStyles(s);

        VeBangTrangThai(s);

        // PC giu nguyen thanh ky nang vuong o day man hinh; may cam ung thi doi
        // sang cum nut tron o goc phai duoi cho vua tam ngon cai.
        if (CamUng.DangDung) { VeNutKyNangTron(s); VeNutGocNhin(s); VeNutKhoaCam(s); }
        else DrawSkillBar(s);
        DrawTopInfo(s);
        DrawMessages(s);
    }

    // ================================================================
    //  BANG TRANG THAI GOC TRAI TREN
    // ================================================================

    /// <summary>Ten nhan vat tren bang. De khong dau cho khoi vo font.</summary>
    public string tenNhanVat = "PHU THUY";

    /// <summary>
    /// Bang trang thai: ten, thanh mau, thanh khieng, thanh nang luong - xep
    /// tu tren xuong o goc trai tren.
    ///
    /// Cac dai KHONG duoc de len nhau. Moi thu chiem dung mot dai cao rieng,
    /// cong don xuong bang bien y - khong dat toa do tay cho tung thanh, vi
    /// doi chieu cao mot thanh la phai sua toa do tat ca nhung thanh duoi no.
    /// </summary>
    // Kich thuoc bang trang thai, de o MOT CHO duy nhat.
    //
    // Truoc day co dong "GOC NHIN: ..." ve ngay duoi bang nay - da bo han. Hai
    // lan no gay chuyen: lan dau ve o (12s, 10s) DE CHONG len ten nhan vat, lan
    // sau tren man hinh hep thi chu dai xuong hai dong va tran vao thanh nang
    // luong. Ca hai lan deu chi lo ra khi chup anh that trong Play mode.
    //
    // DayBangTrangThai() ben duoi gio khong con ai goi, nhung giu lai: thu gi
    // muon dat duoi bang trang thai deu can no de biet bang cao den dau.
    const float BangLeTrai = 14f;
    const float BangLeTren = 12f;
    /// <summary>Be rong bang trang thai goc tren trai, tinh o do phan giai 1080.
    /// Mo ra cho kich ban chay thu do xem con so co lot trong thanh khong.</summary>
    public const float BangRong = 320f;
    const float BangKhe = 5f;
    const float BangCaoTen = 26f;
    const float BangCaoMau = 30f;
    const float BangCaoKhieng = 9f;     // MONG, chi la mot vach nho
    const float BangCaoMana = 22f;

    /// <summary>Day bang trang thai nam o dau tren man hinh.</summary>
    public static float DayBangTrangThai(float s)
    {
        return (BangLeTren + BangCaoTen + BangCaoMau + BangKhe
              + BangCaoKhieng + BangKhe + BangCaoMana) * s;
    }

    void VeBangTrangThai(float s)
    {
        float leTrai = BangLeTrai * s;
        float leTren = BangLeTren * s;
        float rong = BangRong * s;
        float khe = BangKhe * s;

        float caoTen = BangCaoTen * s;
        float caoMau = BangCaoMau * s;
        float caoKhieng = BangCaoKhieng * s;
        float caoMana = BangCaoMana * s;

        float y = leTren;

        // ---- Ten nhan vat ----
        var stTen = new GUIStyle(midStyle);
        stTen.alignment = TextAnchor.MiddleLeft;
        stTen.fontSize = Mathf.RoundToInt(19f * s);
        stTen.fontStyle = FontStyle.Bold;
        stTen.normal.textColor = new Color(0.94f, 0.90f, 0.76f);

        // Bong chu: ve lech mot chut bang mau toi truoc
        var stBong = new GUIStyle(stTen);
        stBong.normal.textColor = new Color(0f, 0f, 0f, 0.75f);
        GUI.Label(new Rect(leTrai + 2f * s, y + 2f * s, rong, caoTen), tenNhanVat, stBong);
        GUI.Label(new Rect(leTrai, y, rong, caoTen), tenNhanVat, stTen);

        y += caoTen;                     // ten chiem tron dai cua no, thanh mau
                                         // khong ve de len

        // ---- Thanh mau ----
        float mau01 = playerHealth != null ? playerHealth.Health01 : 0f;
        string chuMau = playerHealth != null
            ? Mathf.CeilToInt(playerHealth.health) + " / " + Mathf.CeilToInt(playerHealth.maxHealth)
            : "";
        VeThanh3D(new Rect(leTrai, y, rong, caoMau), mau01, gradMau, chuMau, s, 17f);
        y += caoMau + khe;

        // ---- Thanh khieng: CHI hien khi dang co khieng ----
        // Khong co khieng thi de TRONG dai nay chu khong keo thanh mana len -
        // thanh mana nhay len nhay xuong moi lan bat/tat khieng thi roi mat.
        float khieng01 = player != null ? player.KhiengMau01 : 0f;
        if (khieng01 > 0.001f)
        {
            VeThanh3D(new Rect(leTrai, y, rong, caoKhieng), khieng01,
                      khieng01 > 0.35f ? gradKhieng : gradKhiengYeu, "", s, 0f);
        }
        y += caoKhieng + khe;

        // ---- Thanh nang luong ----
        float mana01 = player != null ? player.Mana01 : 0f;
        string chuMana = player != null
            ? Mathf.CeilToInt(player.mana) + " / " + Mathf.CeilToInt(player.maxMana)
            : "";
        VeThanh3D(new Rect(leTrai, y, rong, caoMana), mana01, gradMana, chuMana, s, 14f);
    }

    /// <summary>
    /// Mot thanh chi so kieu NOI KHOI, co do bong.
    ///
    /// Bon lop xep tu duoi len, thieu lop nao la thanh xep lai thanh mot hinh
    /// chu nhat phang:
    ///   1. BONG DO - hinh chu nhat den mo, lech xuong duoi ben phai. Day la
    ///      thu tach thanh ra khoi nen, cho no noi len.
    ///   2. RANH CHIM - nen toi, canh TREN toi va canh DUOI sang. Nguoc voi mot
    ///      khoi noi (tren sang duoi toi) - nho vay no chim xuong.
    ///   3. PHAN DAY - to bang dai mau doc (sang tren, dam giua, toi duoi) nen
    ///      nhin ra mat tru chu khong phai mieng giay mau.
    ///   4. VET BONG - dai trang mo o mot phan ba tren cung, giong anh sang hat
    ///      tren mat kinh cong.
    /// </summary>
    void VeThanh3D(Rect r, float tiLe, Texture2D grad, string chu, float s, float coChu)
    {
        tiLe = Mathf.Clamp01(tiLe);
        var cu = GUI.color;
        float vien = Mathf.Max(1f, 2f * s);

        // 1. Bong do
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(r.x + 3f * s, r.y + 3f * s, r.width, r.height),
                        barTex, ScaleMode.StretchToFill, true);

        // 2. Ranh chim
        GUI.color = new Color(0.05f, 0.04f, 0.05f, 0.92f);
        GUI.DrawTexture(r, barTex, ScaleMode.StretchToFill, true);

        GUI.color = new Color(0f, 0f, 0f, 0.85f);
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, vien), barTex, ScaleMode.StretchToFill, true);
        GUI.color = new Color(1f, 1f, 1f, 0.16f);
        GUI.DrawTexture(new Rect(r.x, r.yMax - vien, r.width, vien), barTex, ScaleMode.StretchToFill, true);

        // 3. Phan day
        if (tiLe > 0.001f)
        {
            var trong = new Rect(r.x + vien, r.y + vien,
                                 (r.width - vien * 2f) * tiLe, r.height - vien * 2f);
            GUI.color = Color.white;
            GUI.DrawTexture(trong, grad, ScaleMode.StretchToFill, true);

            // 4. Vet bong tren mat thanh
            GUI.color = new Color(1f, 1f, 1f, 0.22f);
            GUI.DrawTexture(new Rect(trong.x, trong.y, trong.width, trong.height * 0.34f),
                            barTex, ScaleMode.StretchToFill, true);

            // Vach sang o dau mut - cho thay ro thanh dang day toi dau
            GUI.color = new Color(1f, 1f, 1f, 0.45f);
            GUI.DrawTexture(new Rect(trong.xMax - vien, trong.y, vien, trong.height),
                            barTex, ScaleMode.StretchToFill, true);
        }

        GUI.color = cu;
        DrawBorder(r, new Color(0.62f, 0.55f, 0.40f, 0.75f), Mathf.Max(1f, vien * 0.6f));

        // Con so, dat GIUA thanh
        if (!string.IsNullOrEmpty(chu) && coChu > 0.5f)
        {
            var st = new GUIStyle(smallStyle);
            st.alignment = TextAnchor.MiddleCenter;
            st.fontSize = Mathf.RoundToInt(coChu * s);
            st.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(r.x + 1f * s, r.y + 1f * s, r.width, r.height), chu, st);
            st.normal.textColor = Color.white;
            GUI.Label(r, chu, st);
        }
    }

    // ================================================================
    //  DIEU KHIEN CAM UNG
    // ================================================================

    // Vi tri va co cua can joystick.
    //
    // Toa do nay dem tu DUOI len, giong Input.mousePosition - nen goc duoi trai
    // la y NHO. Lan dau toi viet "Screen.height - 150s" va can nhay len tan goc
    // TREN, de chong len bang trang thai.
    // To len hai dot: +25% roi +20% nua, tuc 115 -> 143,75 -> 172,5 (tong +50%
    // so voi ban dau).
    //
    // TAM phai day ra theo ban kinh, khong thi le bi an mat. Giu tam o 175 nhu
    // luc dau thi can cham mep man hinh (175 - 172,5 = 2,5 diem anh) va nua
    // vong ben trai nam ngoai vien. 172,5 + 60 = 232,5 giu dung le 60 nhu cu.
    Vector2 TamJoystick(float s) { return new Vector2(232.5f * s, 232.5f * s); }
    float BanKinhJoystick(float s) { return 172.5f * s; }

    /// <summary>
    /// Doc cham/chuot cho can joystick va cac nut ky nang.
    ///
    /// Lam trong Update chu khong trong OnGUI: OnGUI chay NHIEU LAN moi khung
    /// hinh (mot lan cho Layout, mot lan cho Repaint...), nen bat "vua nhan" o
    /// do se an mot cu bam thanh vai cu.
    /// </summary>
    /// <summary>Hien mot dong bao ngan giua man hinh.</summary>
    void Say(string chu)
    {
        baoChu = chu;
        baoLuc = Time.time;
    }

    string baoChu = "";
    float baoLuc = -99f;

    void DocCamUng()
    {
        CamUng.XoayCam = Vector2.zero;
        CamUng.ChumZoom = 0f;

        if (!CamUng.DangDung)
        {
            CamUng.Huong = Vector2.zero;
            CamUng.DangKeo = false;
            ngonJoy = -1;
            ngonNut.Clear();
            chumTruoc = 0f;
            chuotXoay = false;
            HuyNgam();
            return;
        }

        float s = Screen.height / Ref;

        // ================================================================
        //  0. MAN HINH THUA - luc nay chi con MOT nut
        // ================================================================
        //
        // Xet truoc moi thu roi thoat luon. De chay tiep thi ngon tay dat len
        // lop man hinh thua van keo joystick va bam trung nut ky nang nam ngay
        // phia duoi: nhan vat da guc ma HUD van sang nut, nhin nhu game treo.
        if (director != null && director.PlayerDead)
        {
            CamUng.Huong = Vector2.zero;
            CamUng.DangKeo = false;
            ngonJoy = -1;
            ngonNut.Clear();
            HuyNgam();
            DocNutTroVe(s);
            return;
        }

        // Chu phong da roi tran: tran dung han, nguoi khach can mot duong ve
        // sanh. Tren may cam ung khong co phim ESC - phai co nut.
        if (KhoiDongTranMang.CanNutVeSanh)
        {
            DocNutTroVe(s);
            return;
        }
        ngonTroVe = -99;

        Vector2 tam = TamJoystick(s);
        float bk = BanKinhJoystick(s);

        // ================================================================
        //  1. PHAN VAI CHO NGON VUA CHAM XUONG
        // ================================================================
        //
        // Moi ngon nhan vai DUNG MOT LAN, ngay luc no cham xuong, va giu vai do
        // cho den khi nhac len. Neu phan vai lai moi khung thi ngon dang keo
        // camera di ngang qua cum nut se bam nham ky nang, con ngon bam ky nang
        // - bao gio cung keo le mot chut - se thanh keo camera.
        //
        // Thu tu xet cung quan trong: CAN truoc, NUT sau, con lai moi la vung
        // trong.
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            if (ngonJoy == -1 && Vector2.Distance(t.position, tam) <= bk * 1.35f)
            {
                ngonJoy = t.fingerId;
                continue;
            }
            // Nut KHOA xet TRUOC nut doi goc nhin: hai nut nam sat nhau, va
            // cai nay doi hanh vi cua cai kia.
            if (BamNutKhoaCam(t.position, s))
            {
                ngonNut.Add(t.fingerId);
                continue;
            }
            if (BamNutGocNhin(t.position, s))
            {
                ngonNut.Add(t.fingerId);
                continue;
            }

            int nut = NutTaiDiem(t.position, s);
            if (nut >= 0)
            {
                // CHUA DANH GI CA. Chi nhan lay ngon nay va cho xem no se
                // duoc keo ra hay tha ngay tai cho - hai viec khac han nhau.
                ngonNut.Add(t.fingerId);
                if (ngonNgam == -99)
                {
                    ngonNgam = t.fingerId;
                    mayNgam.BatDau(nut, t.position, TamNut(nut, s),
                                   BanKinhNut(s), KeoToiDa(s));
                }
                continue;
            }
            // Con lai la ngon TU DO - danh cho camera, khong phai ghi gi ca.
        }

        // Nhac ngon len thi tra lai vai
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) continue;
            if (t.fingerId == ngonJoy) ngonJoy = -1;
            ngonNut.Remove(t.fingerId);

            if (t.fingerId == ngonNgam)
            {
                // Huy (Canceled) thi KHONG danh: cu chi bi he dieu hanh cat
                // ngang - co dien thoai goi den, keo thanh thong bao xuong -
                // chu nguoi choi khong he tha tay ra de danh.
                if (t.phase == TouchPhase.Ended) ThaNgam(t.position);
                HuyNgam();
            }
        }
        // Khong con ngon nao tren man hinh thi khong con vai nao ca. Doan nay de
        // phong khi mot su kien "nhac tay" bi mat - tren trinh duyet chuyen tab
        // hay co dien thoai goi den la hay xay ra.
        if (Input.touchCount == 0 && ngonNgam >= 0) HuyNgam();
        if (Input.touchCount == 0) ngonNut.Clear();

        // ---- Duong CHUOT: chi de THU tren PC ----
        if (Input.touchCount == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 pos = Input.mousePosition;
                if (ngonJoy == -1 && Vector2.Distance(pos, tam) <= bk * 1.35f)
                    ngonJoy = -2;
                else if (BamNutKhoaCam(pos, s))
                {
                    // da bat/tat khoa ngay luc bam, khong phai lam gi them
                }
                else if (BamNutGocNhin(pos, s))
                {
                    // da doi goc nhin ngay luc bam, khong phai lam gi them
                }
                else
                {
                    int nut = NutTaiDiem(pos, s);
                    if (nut >= 0)
                    {
                        ngonNgam = -2;
                        mayNgam.BatDau(nut, pos, TamNut(nut, s),
                                       BanKinhNut(s), KeoToiDa(s));
                    }
                    else
                    {
                        chuotXoay = true;
                        chuotTruoc = pos;
                    }
                }
            }
            if (Input.GetMouseButton(0) && ngonNgam == -2)
                mayNgam.KeoToi(Input.mousePosition);

            if (Input.GetMouseButtonUp(0) && ngonNgam == -2)
            {
                ThaNgam(Input.mousePosition);
                HuyNgam();
            }
            if (!Input.GetMouseButton(0))
            {
                chuotXoay = false;
                if (ngonJoy == -2) ngonJoy = -1;
                if (ngonNgam == -2) HuyNgam();
            }
        }

        // ================================================================
        //  2. CAN JOYSTICK
        // ================================================================
        bool conGiu = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.fingerId != ngonJoy) continue;
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
            diemJoy = t.position;
            conGiu = true;
        }
        if (ngonJoy == -2 && Input.GetMouseButton(0))
        {
            diemJoy = Input.mousePosition;
            conGiu = true;
        }

        if (conGiu)
        {
            Vector2 v = (diemJoy - tam) / bk;
            CamUng.Huong = v.sqrMagnitude > 1f ? v.normalized : v;
            CamUng.DangKeo = true;
        }
        else
        {
            CamUng.Huong = Vector2.zero;
            CamUng.DangKeo = false;
        }

        // ================================================================
        //  2b. NGON DANG NGAM KY NANG
        // ================================================================
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.fingerId != ngonNgam) continue;
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
            mayNgam.KeoToi(t.position);
        }

        // ================================================================
        //  3. CAMERA - CHI NGON O VUNG TRONG
        // ================================================================
        //
        // MOT ngon keo -> xoay quanh nhan vat. HAI ngon -> chum vao/toe ra de
        // phong to thu nho, va KHONG xoay: lam ca hai cung luc thi moi cu chum
        // deu keo goc nhin lech di mot it.
        // DANG NGAM thi KHOA HAN goc nhin, bang BAT CU ngon nao.
        //
        // Ngon dang ngam von da nam trong 'ngonNut' nen mot minh no khong lam
        // camera nhuc nhich. Nhung nguoi choi cham them ngon thu ba vao vung
        // trong - rat de xay ra khi hai tay dang ban - thi ngon do van xoay
        // duoc goc nhin, va ca duong ngam dang ve quay theo mot cai: dang nham
        // trung dau thi phep bay ra huong khac.
        //
        // Chan o day, sau khi da doc xong joystick: nguoi choi VAN di chuyen
        // duoc trong luc ngam.
        if (mayNgam.DangNgam)
        {
            chumTruoc = 0f;
            chuotXoay = false;
            GhiTrangThaiNgam();
            return;
        }

        int soTuDo = 0;
        Vector2 deltaTuDo = Vector2.zero, a = Vector2.zero, b = Vector2.zero;
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.fingerId == ngonJoy) continue;
            if (ngonNut.Contains(t.fingerId)) continue;
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;

            soTuDo++;
            if (soTuDo == 1) { a = t.position; deltaTuDo = t.deltaPosition; }
            else if (soTuDo == 2) b = t.position;
        }

        if (soTuDo >= 2)
        {
            float d = Vector2.Distance(a, b);
            // Khung dau tien cua cu chum chi de GHI NHO khoang cach. Lay hieu
            // ngay khung do la nhay mot phat bang ca khoang cach hai ngon.
            if (chumTruoc > 0f) CamUng.ChumZoom = d - chumTruoc;
            chumTruoc = d;
        }
        else
        {
            chumTruoc = 0f;
            if (soTuDo == 1) CamUng.XoayCam = deltaTuDo;
        }

        // Duong chuot tren PC - de thu cu chi xoay ma khong can man hinh cam ung
        if (chuotXoay && Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;
            CamUng.XoayCam = pos - chuotTruoc;
            chuotTruoc = pos;
        }

        // Ghi o CUOI, sau CA duong cham lan duong chuot. Dat trong nhanh cham
        // thi luc thu bang chuot tren PC (touchCount = 0) khong bao gio chay
        // toi, va CamUng ket lai o gia tri cu.
        GhiTrangThaiNgam();
    }

    // ================================================================
    //  NUT "TRO VE" TREN MAN HINH THUA - chi may cam ung
    // ================================================================

    /// <summary>
    /// O nut "Tro ve". Toa do GUI: dem tu TREN xuong.
    ///
    /// Dung chung cho ca luc VE lan luc DOC CHAM. Khong duoc tach thanh hai
    /// cong thuc: tach ra thi sua mot ben quen ben kia, nut hien mot cho ma
    /// bam lai an o cho khac.
    /// </summary>
    Rect ONutTroVe(float s)
    {
        // Cao do 0,66: dong thong bao ngan ("het nang luong"...) nam o 0,62 va
        // van co the con hien mot hai giay sau khi guc nga. Dat nut cao hon la
        // no de len chu.
        float w = 280f * s, h = 76f * s;
        return new Rect((Screen.width - w) * 0.5f, Screen.height * 0.66f, w, h);
    }

    /// <summary>Diem cham (dem tu DUOI len) co nam trong o GUI (dem tu TREN xuong)?</summary>
    static bool TrongO(Rect r, Vector2 diemCham)
    {
        return r.Contains(new Vector2(diemCham.x, Screen.height - diemCham.y));
    }

    /// <summary>
    /// Doc cham cho nut "Tro ve".
    ///
    /// Bam xuong chi GHI NHO ngon; phai NHAC LEN con trong o nut moi thuc su ve
    /// menu. Dat nham ngon thi keo ra ngoai roi tha la thoat - giong nut cua he
    /// dieu hanh. Man hinh thua hien ra ngay giua luc dang loan tay, khong nen
    /// de mot cai cham vo tinh la roi thang ve menu.
    /// </summary>
    void DocNutTroVe(float s)
    {
        Rect r = ONutTroVe(s);

        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began)
            {
                if (ngonTroVe == -99 && TrongO(r, t.position)) ngonTroVe = t.fingerId;
            }
            else if (t.fingerId == ngonTroVe
                     && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
            {
                bool trong = t.phase == TouchPhase.Ended && TrongO(r, t.position);
                ngonTroVe = -99;
                if (trong) { GameDirector.BackToMenu(); return; }
            }
        }

        // Nhac het tay ma van con ghi ngon la mot su kien "nhac len" bi mat -
        // chuyen tab trinh duyet hay co dien thoai goi den la hay xay ra.
        if (Input.touchCount == 0 && ngonTroVe >= 0) ngonTroVe = -99;

        // ---- Duong CHUOT: de thu che do cam ung tren PC bang F9 ----
        if (Input.touchCount == 0)
        {
            Vector2 chuot = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) && TrongO(r, chuot)) ngonTroVe = -2;
            else if (Input.GetMouseButtonUp(0) && ngonTroVe == -2)
            {
                ngonTroVe = -99;
                if (TrongO(r, chuot)) { GameDirector.BackToMenu(); return; }
            }
        }
    }

    void VeNutTroVe(float s)
    {
        Rect r = ONutTroVe(s);
        bool dangBam = ngonTroVe != -99;

        var prev = GUI.color;
        GUI.color = dangBam ? new Color(0.58f, 0.20f, 0.14f, 0.95f)
                            : new Color(0.14f, 0.12f, 0.11f, 0.92f);
        GUI.DrawTexture(r, barTex, ScaleMode.StretchToFill, true);
        GUI.color = prev;

        DrawBorder(r, new Color(0.95f, 0.78f, 0.45f, 0.95f), Mathf.Max(2f, 2.5f * s));

        var st = new GUIStyle(midStyle);
        st.alignment = TextAnchor.MiddleCenter;
        st.fontStyle = FontStyle.Bold;
        st.fontSize = Mathf.RoundToInt(30f * s);
        st.normal.textColor = dangBam ? new Color(1f, 1f, 0.85f)
                                      : new Color(1f, 0.90f, 0.62f);
        GUI.Label(r, "TRO VE", st);
    }

    /// <summary>
    /// Cham vao nut DOI GOC NHIN khong. Nut nay lam viec ngay luc cham xuong -
    /// no khong co gi de ngam ca.
    /// </summary>
    bool BamNutGocNhin(Vector2 diem, float s)
    {
        if (Vector2.Distance(diem, TamNutGocNhin(s)) > BanKinhNutGocNhin(s)) return false;

        // Dang khoa thi khong doi goc nhin, nhung VAN nhan lay ngon tay nay:
        // tra ve false la ngon do thanh "ngon tu do" va di keo camera - dung
        // cai ma khoa dang phai chan. (CameraRig con chan them mot lop nua.)
        if (cameraRig != null && !CamUng.KhoaCam) cameraRig.CycleView();
        return true;
    }

    /// <summary>
    /// Nut ky nang nam duoi diem cham, -1 neu khong trung nut nao.
    ///
    /// CHI TRA LOI, khong danh gi. Truoc day ham nay danh luon tai cho - khong
    /// tach ra thi khong the co chuyen "giu roi keo de ngam", vi ky nang da bay
    /// ra tu luc ngon tay moi vua cham xuong.
    /// </summary>
    int NutTaiDiem(Vector2 diem, float s)
    {
        float r = BanKinhNut(s);
        for (int i = 0; i < SoNut; i++)
            if (Vector2.Distance(diem, TamNut(i, s)) <= r) return i;
        return -1;
    }

    // ================================================================
    //  NGAM: GIU - KEO - THA
    // ================================================================

    /// <summary>Chep trang thai ngam sang <see cref="CamUng"/> cho ben khac doc.</summary>
    void GhiTrangThaiNgam()
    {
        CamUng.KyNangNgam = mayNgam.KyNang;
        CamUng.LechNgam = mayNgam.Lech;
        CamUng.DaRaKhoiNut = mayNgam.DaRaKhoiNut;
    }

    /// <summary>
    /// Tha tay: danh ky nang.
    ///
    /// Chua ra khoi nut  -> danh TU DONG vao ke dich gan nhat.
    /// Da keo ra ngoai   -> danh dung huong (va dung cu ly) da ngam.
    /// </summary>
    void ThaNgam(Vector2 diemTha)
    {
        if (player == null || !mayNgam.DangNgam) return;

        int ky = mayNgam.KyNang;
        MayNgam.Lenh lenh = mayNgam.Tha(diemTha);

        if (lenh == MayNgam.Lenh.TuDong) { player.DanhTuDong(ky); return; }
        if (lenh != MayNgam.Lenh.TheoHuong) return;

        player.DanhTheoHuong(ky, player.HuongTuManHinh(mayNgam.Lech), mayNgam.Xa01);
    }

    void HuyNgam()
    {
        ngonNgam = -99;
        mayNgam.Huy();
        GhiTrangThaiNgam();
        if (chiBao != null) chiBao.An();
    }

    /// <summary>
    /// Nut DOI GOC NHIN - thay cho phim C tren may cam ung.
    ///
    /// Dat o goc PHAI TREN: goc trai tren la bang trang thai, giua tren la so
    /// quai con lai, phai duoi la cum ky nang. Goc phai tren la cho trong duy
    /// nhat con lai, va cung la cho ngon cai it quet qua nhat - dung cho mot
    /// nut ma bam nham thi doi ca goc nhin.
    /// </summary>
    float BanKinhNutGocNhin(float s) { return 40f * s; }

    /// <summary>
    /// Ve nut doi goc nhin: vong tron mo, ben trong la hinh mot may quay phim.
    ///
    /// Ve bang cac hinh chu nhat chu khong dung anh: mot nut duy nhat khong bo
    /// cong lam ca mot texture, ma hinh may quay thi chi can than va ong kinh
    /// la nhan ra.
    /// </summary>
    void VeNutGocNhin(float s)
    {
        Vector2 t = TamNutGocNhin(s);
        float r = BanKinhNutGocNhin(s);

        // GUI dem y tu TREN xuong con cham dem tu DUOI len - phai lat lai
        float gx = t.x, gy = Screen.height - t.y;

        var cu = GUI.color;

        // Dang khoa goc nhin thi nut nay khong lam gi nua - ve MO han di de
        // nguoi choi khoi bam mai ma khong hieu sao khong doi duoc.
        float dam = CamUng.KhoaCam ? 0.35f : 1f;

        GUI.color = new Color(1f, 1f, 1f, 0.20f * dam);
        GUI.DrawTexture(new Rect(gx - r, gy - r, r * 2f, r * 2f),
                        vongNen, ScaleMode.StretchToFill, true);

        // Than may quay
        GUI.color = new Color(0.92f, 0.94f, 1f, 0.80f * dam);
        float w = r * 0.86f, h = r * 0.58f;
        GUI.DrawTexture(new Rect(gx - w * 0.62f, gy - h * 0.5f, w, h),
                        Texture2D.whiteTexture, ScaleMode.StretchToFill, false);

        // Ong kinh nho ra phia phai
        GUI.DrawTexture(new Rect(gx + w * 0.38f, gy - h * 0.28f, r * 0.34f, h * 0.56f),
                        Texture2D.whiteTexture, ScaleMode.StretchToFill, false);

        // Cuon phim tren nap
        GUI.DrawTexture(new Rect(gx - w * 0.34f, gy - h * 0.5f - r * 0.20f, r * 0.30f, r * 0.20f),
                        Texture2D.whiteTexture, ScaleMode.StretchToFill, false);

        GUI.color = cu;
    }

    Vector2 TamNutGocNhin(float s)
    {
        // Toa do dem tu DUOI len, giong Input.mousePosition
        return new Vector2(Screen.width - 62f * s, Screen.height - 62f * s);
    }

    // ================================================================
    //  NUT KHOA GOC NHIN - hinh CON MAT, ngay duoi nut doi goc nhin
    // ================================================================
    //
    // Bam mot cai la camera dung im o dung goc dang co: khong xoay, khong phong
    // to, khong doi che do - khong bang bat cu thao tac nao. Bam lai la mo.
    //
    // Dat NGAY DUOI nut doi goc nhin vi hai nut cung noi ve mot thu, va vi goc
    // phai tren la cho ngon cai it quet qua nhat - hai nut nay bam nham thi doi
    // ca khung hinh dang nhin.
    //
    // Cach nhau 88s tu tam den tam, trong khi ban kinh moi nut la 40s: con lai
    // 8s ho giua hai vanh, du de khong bam nham cai nay ra cai kia.

    float BanKinhNutKhoaCam(float s) { return 40f * s; }

    Vector2 TamNutKhoaCam(float s)
    {
        Vector2 t = TamNutGocNhin(s);
        return new Vector2(t.x, t.y - 88f * s);
    }

    /// <summary>
    /// Cham vao nut KHOA GOC NHIN khong. Lam viec ngay luc cham xuong, giong
    /// nut doi goc nhin - no khong co gi de ngam ca.
    /// </summary>
    bool BamNutKhoaCam(Vector2 diem, float s)
    {
        if (Vector2.Distance(diem, TamNutKhoaCam(s)) > BanKinhNutKhoaCam(s)) return false;

        CamUng.KhoaCam = !CamUng.KhoaCam;
        Say(CamUng.KhoaCam ? "Da khoa goc nhin" : "Da mo khoa goc nhin");
        return true;
    }

    /// <summary>
    /// Ve nut khoa goc nhin: vong tron mo, ben trong la mot CON MAT; dang khoa
    /// thi co them mot gach cheo mau do de len.
    ///
    /// Con mat ghep tu cac DAI DOC chu khong dung anh: chieu cao moi dai theo
    /// mot duong cong nen hai dau thon nhon lai thanh hinh qua hanh - ve bang
    /// mot hinh bau duc keo giang thi ra qua trung, khong ai doc ra la con mat.
    /// </summary>
    void VeNutKhoaCam(float s)
    {
        Vector2 t = TamNutKhoaCam(s);
        float r = BanKinhNutKhoaCam(s);

        // GUI dem y tu TREN xuong con cham dem tu DUOI len - phai lat lai
        float gx = t.x, gy = Screen.height - t.y;

        var cu = GUI.color;
        bool khoa = CamUng.KhoaCam;

        // ---- Nen tron ----
        GUI.color = khoa ? new Color(1f, 0.62f, 0.55f, 0.26f)
                         : new Color(1f, 1f, 1f, 0.20f);
        GUI.DrawTexture(new Rect(gx - r, gy - r, r * 2f, r * 2f),
                        vongNen, ScaleMode.StretchToFill, true);

        // ---- Trong mat: hinh qua hanh ----
        float W = r * 1.34f, H = r * 0.66f;
        const int SoCot = 17;
        float wCot = W / SoCot;

        GUI.color = khoa ? new Color(1f, 0.86f, 0.82f, 0.90f)
                         : new Color(0.95f, 0.97f, 1f, 0.88f);
        for (int i = 0; i < SoCot; i++)
        {
            float u = (i + 0.5f) / SoCot;          // 0..1 doc theo be ngang
            float d = u * 2f - 1f;                 // -1 o mep trai, +1 o mep phai
            // Mu 0,62 (chu khong phai 0,5 nhu duong tron) lam hai dau THON lai
            // thanh goc nhon - do la cai lam no ra con mat chu khong ra qua trung.
            float nua = H * 0.5f * Mathf.Pow(Mathf.Max(0f, 1f - d * d), 0.62f);
            if (nua < 0.5f) continue;
            GUI.DrawTexture(new Rect(gx - W * 0.5f + i * wCot, gy - nua, wCot + 0.5f, nua * 2f),
                            Texture2D.whiteTexture, ScaleMode.StretchToFill, false);
        }

        // ---- Con nguoi ----
        float rNguoi = H * 0.46f;
        GUI.color = new Color(0.10f, 0.10f, 0.13f, 0.95f);
        GUI.DrawTexture(new Rect(gx - rNguoi, gy - rNguoi, rNguoi * 2f, rNguoi * 2f),
                        vongNen, ScaleMode.StretchToFill, true);

        // Dom sang nho lech len goc trai - thieu no thi con nguoi la mot lo den
        float rDom = rNguoi * 0.34f;
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        GUI.DrawTexture(new Rect(gx - rNguoi * 0.45f - rDom, gy - rNguoi * 0.45f - rDom,
                                 rDom * 2f, rDom * 2f),
                        vongNen, ScaleMode.StretchToFill, true);

        // ---- Dang khoa: gach cheo do ----
        if (khoa)
        {
            var mCu = GUI.matrix;
            // Xoay 45 do quanh tam nut. Trong he toa do GUI truc y huong XUONG,
            // nen mot thanh NGANG xoay +45 do se chay tu tren-trai xuong
            // duoi-phai - dung net nguoi dung yeu cau, khong phai net nguoc lai.
            GUIUtility.RotateAroundPivot(45f, new Vector2(gx, gy));

            float dai = r * 1.92f, day = Mathf.Max(2.5f, r * 0.19f);

            // Vien toi phia sau, de net do noi len ca tren nen sang lan nen toi
            GUI.color = new Color(0f, 0f, 0f, 0.80f);
            GUI.DrawTexture(new Rect(gx - dai * 0.5f, gy - day * 0.85f, dai, day * 1.7f),
                            Texture2D.whiteTexture, ScaleMode.StretchToFill, false);

            GUI.color = new Color(0.95f, 0.13f, 0.10f, 1f);
            GUI.DrawTexture(new Rect(gx - dai * 0.5f, gy - day * 0.5f, dai, day),
                            Texture2D.whiteTexture, ScaleMode.StretchToFill, false);

            GUI.matrix = mCu;
        }

        GUI.color = cu;
    }

    // Nut phai DU TO cho ngon cai (dau ngon cai nguoi lon khoang 45-57 diem anh
    // o mat do man hinh thong thuong), nhung to qua thi sau nut khong xep vua
    // mot cum ma khong de len nhau.
    /// <summary>So nut ky nang tren cum cam ung.</summary>
    const int SoNut = 7;

    /// <summary>
    /// Le tu goc man hinh toi TAM cua cung. Phai lon hon ban kinh nut, khong
    /// thi nut trong cung bi cat mat mot mieng o mep man hinh.
    /// </summary>
    const float Le = 101.2f;

    float BanKinhNut(float s) { return 60.72f * s; }

    /// <summary>
    /// Tam cua nut ky nang thu <paramref name="i"/>, tinh theo GOC PHAI DUOI.
    ///
    /// Xep thanh HAI CUNG quanh goc man hinh chu khong thanh mot hang ngang:
    /// ngon cai quet theo cung tron, hang ngang thi dau nay voi toi dau kia phai
    /// nhac ca ban tay.
    ///
    /// Toa do tra ve dem tu DUOI len, giong Input.mousePosition - de so thang
    /// voi diem cham ma khong phai doi truc.
    /// </summary>
    Vector2 TamNut(int i, float s)
    {
        // Hai cung dong tam quanh goc phai duoi, moi cung ba nut.
        //
        // GOC GIUA HAI NUT phai du rong. Khoang cach giua hai nut lien ke tren
        // mot cung la 2*R*sin(buoc/2); phai lon hon DUONG KINH nut (2r) thi
        // chung moi khong de len nhau. Ban dau toi de cung R=170s voi buoc 29
        // do: ra 85s trong khi duong kinh nut la 124s - sau nut chong len nhau
        // thanh mot dam.
        //
        // NUT TO LEN THI CA CUM PHAI GIAN THEO CUNG MOT TI LE. Nut len 10% ma
        // giu nguyen hai cung thi cung trong chi con ho 2,6s giua hai nut lien
        // ke - gan nhu dinh nhau. Nhan TAT CA cho cung mot he so (ke ca le) thi
        // moi khoang cach cung len theo, ti le ho giu y nguyen nhu truoc.
        //
        // Da nhan hai lan: 1,1 roi 1,15. Goc thi GIU NGUYEN - chi ban kinh doi.
        //
        // So do hien tai: cung trong R=234,03s buoc 34 do -> 136,9s; cung ngoai
        // R=392,15s buoc 22 do -> 149,6s; duong kinh nut 2r = 121,4s. Cap gan
        // nhau nhat giua HAI cung la 158,1s. Tat ca deu con ho.
        // NUT THU BAY nam o mot cung RIENG, trong cung, sat goc man hinh.
        //
        // Sau nut cu xep thanh hai cung ba nut; nhet nut thu bay vao mot trong
        // hai cung ay thi buoc goc hep lai va cac nut dinh nhau. Cung thu ba
        // ban kinh 80,2 cho no nam GIUA nut Sam set va goc man hinh - dung cho
        // nguoi dung da khoanh tron trong hinh.
        //
        // Kiem lai khoang cach: toi nut Sam set 147,7 | toi nut Mua bang 149,0
        // | duong kinh nut 121,4. Cho hep nhat con ho 26,3.
        if (i == 6)
        {
            const float banBay = 92.23f;
            const float gocBay = 26.1f;
            float radBay = gocBay * Mathf.Deg2Rad;
            return new Vector2(Screen.width - Le * s - Mathf.Cos(radBay) * banBay * s,
                               Le * s + Mathf.Sin(radBay) * banBay * s);
        }

        bool trong = i < 3;
        float ban = (trong ? 234.03f : 392.15f) * s;
        int k = trong ? i : i - 3;

        // Trai ra (goc lon) den len tren (goc nho)
        float goc = trong ? Mathf.Lerp(78f, 10f, k / 2f)
                          : Mathf.Lerp(66f, 22f, k / 2f);
        float rad = goc * Mathf.Deg2Rad;

        // Le tinh tu goc man hinh phai lon hon ban kinh nut, khong thi nut ngoai
        // cung bi cat mat mot mieng o mep.
        return new Vector2(Screen.width - Le * s - Mathf.Cos(rad) * ban,
                           Le * s + Mathf.Sin(rad) * ban);
    }

    /// <summary>Can joystick va cum nut tron - chi ve tren may cam ung.</summary>
    void VeCanJoystick(float s)
    {
        Vector2 tam = TamJoystick(s);
        float bk = BanKinhJoystick(s);

        // GUI dem y tu TREN xuong con cham dem tu DUOI len - phai lat lai
        var rNen = new Rect(tam.x - bk, Screen.height - tam.y - bk, bk * 2f, bk * 2f);

        var cu = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.22f);
        GUI.DrawTexture(rNen, vongNen, ScaleMode.StretchToFill, true);

        // Num: lech theo huong dang day
        Vector2 lech = CamUng.Huong * bk * 0.62f;
        float bkNum = bk * 0.42f;
        var rNum = new Rect(tam.x + lech.x - bkNum,
                            Screen.height - (tam.y + lech.y) - bkNum,
                            bkNum * 2f, bkNum * 2f);
        GUI.color = CamUng.DangKeo
            ? new Color(1f, 0.92f, 0.65f, 0.85f)
            : new Color(1f, 1f, 1f, 0.45f);
        GUI.DrawTexture(rNum, vongNum, ScaleMode.StretchToFill, true);
        GUI.color = cu;
    }

    /// <summary>
    /// Cum nut ky nang hinh TRON o goc phai duoi. Chi ve hinh, khong ve ten -
    /// tren man hinh dien thoai chu do qua nho de doc ma lai an mat san choi.
    /// </summary>
    void VeNutKyNangTron(float s)
    {
        VeCanJoystick(s);

        var icon = new Texture2D[] { iconFire, iconIce, iconBolt, iconWind,
                                     iconMeteor, iconKhieng, iconGiatSet };
        float r = BanKinhNut(s);
        var cu = GUI.color;

        for (int i = 0; i < SoNut; i++)
        {
            Vector2 t = TamNut(i, s);
            var rect = new Rect(t.x - r, Screen.height - t.y - r, r * 2f, r * 2f);

            // Bong nut hat xuong nen, cho cum nut khong nhu dan bet vao san
            GUI.color = new Color(0f, 0f, 0f, 0.42f);
            GUI.DrawTexture(new Rect(rect.x + r * 0.10f, rect.y + r * 0.16f,
                                     rect.width, rect.height),
                            vongNum, ScaleMode.StretchToFill, true);

            // Hinh ky nang - CHIEM GAN TRON NUT.
            //
            // Bieu tuong da tu ve lay cai dia co mat loi va bong do cua no roi,
            // nen o day khong phu them lop nen nao nua: phu vao la sau cai nut
            // lai ra cung mot mau xam, ma hinh thi bi bop nho o giua.
            GUI.color = Color.white;
            float m = r * 0.04f;
            GUI.DrawTexture(new Rect(rect.x + m, rect.y + m, rect.width - m * 2f, rect.height - m * 2f),
                            icon[i], ScaleMode.StretchToFill, true);

            // Vanh sang bao ngoai: chi mot net mong, cho thay vung bam den dau.
            // Nut DANG GIU thi vanh sang han len - nguoi choi dang nhin san choi
            // chu khong nhin nut, van can mot cai liec la biet minh cam cai gi.
            bool dangGiu = mayNgam.KyNang == i;
            GUI.color = dangGiu ? new Color(1f, 1f, 0.90f, 1.00f)
                                : new Color(1f, 0.95f, 0.80f, 0.55f);
            GUI.DrawTexture(rect, vanhNut, ScaleMode.StretchToFill, true);
            if (dangGiu)
                GUI.DrawTexture(new Rect(rect.x - r * 0.12f, rect.y - r * 0.12f,
                                         rect.width + r * 0.24f, rect.height + r * 0.24f),
                                vanhNut, ScaleMode.StretchToFill, true);

            // Dang hoi chieu: nut toi di roi SANG DAN LEN, kem so giay con lai
            float hoi = HoiChieu01(i);
            if (hoi > 0.001f)
            {
                // Do toi ti le voi phan con phai cho: gan xong thi gan nhu ro
                // han - nguoi choi liec mot cai la biet ky nang nao sap dung duoc
                GUI.color = new Color(0f, 0f, 0f, 0.30f + 0.48f * hoi);
                GUI.DrawTexture(rect, vongNum, ScaleMode.StretchToFill, true);
                GUI.color = Color.white;

                VeCungHoiChieu(t, r * 1.00f, 1f - hoi, s);
                VeSoGiay(rect, player.HoiChieuGiay(i), s);
            }
        }
        GUI.color = cu;

        // Thanh mau khieng khong ve o day - no da co dai rieng tren bang trang thai
    }

    float HoiChieu01(int skill)
    {
        if (player == null) return 0f;
        switch (skill)
        {
            case 0: return player.FireballCooldown01;
            case 1: return player.IceCooldown01;
            case 2: return player.BoltCooldown01;
            case 3: return player.TornadoCooldown01;
            case 4: return player.MeteorCooldown01;
            case 5: return player.KhiengCooldown01;
            case 6: return player.GiatSetCooldown01;
            // Khong de "default" tra ve Khieng: them ky nang moi ma quen sua
            // cho nay thi nut moi lay nham vong hoi chieu cua Khieng, va loi ay
            // nhin vao khong doc ra duoc.
            default: return 0f;
        }
    }

    // ================================================================
    //  THANH KY NANG
    // ================================================================

    void DrawSkillBar(float s)
    {
        float slot = 84f * s;
        float gap = 12f * s;
        float total = slot * 7f + gap * 6f;
        float x = (Screen.width - total) * 0.5f;
        float y = Screen.height - slot - 22f * s;

        DrawSkillSlot(new Rect(x, y, slot, slot), iconFire, "1/Z", 0,
                      player != null ? player.FireballCooldown01 : 0f,
                      player != null && player.SelectedSkill == 0, s);

        DrawSkillSlot(new Rect(x + slot + gap, y, slot, slot), iconIce, "2/X", 1,
                      player != null ? player.IceCooldown01 : 0f,
                      player != null && player.SelectedSkill == 1, s);

        DrawSkillSlot(new Rect(x + (slot + gap) * 2f, y, slot, slot), iconBolt, "3/V", 2,
                      player != null ? player.BoltCooldown01 : 0f,
                      player != null && player.SelectedSkill == 2, s);

        DrawSkillSlot(new Rect(x + (slot + gap) * 3f, y, slot, slot), iconWind, "4/B", 3,
                      player != null ? player.TornadoCooldown01 : 0f,
                      player != null && player.SelectedSkill == 3, s);

        DrawSkillSlot(new Rect(x + (slot + gap) * 4f, y, slot, slot), iconMeteor, "5/N", 4,
                      player != null ? player.MeteorCooldown01 : 0f,
                      player != null && player.SelectedSkill == 4, s);

        DrawSkillSlot(new Rect(x + (slot + gap) * 5f, y, slot, slot), iconKhieng, "6/M", 5,
                      player != null ? player.KhiengCooldown01 : 0f,
                      player != null && player.SelectedSkill == 5, s);

        DrawSkillSlot(new Rect(x + (slot + gap) * 6f, y, slot, slot), iconGiatSet, "7/G", 6,
                      player != null ? player.GiatSetCooldown01 : 0f,
                      player != null && player.SelectedSkill == 6, s);

        // Mau khieng KHONG ve o day nua - no da co dai rieng tren bang trang
        // thai goc trai. Ve ca hai cho thi cung mot con so hien hai lan, ma cho
        // nay lai chen ngay tren hang ky nang.
    }

    void DrawSkillSlot(Rect r, Texture2D icon, string key, int skill,
                       float cooldown01, bool selected, float s)
    {
        GUI.DrawTexture(r, panelTex, ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(r.x + 4f * s, r.y + 4f * s, r.width - 8f * s, r.height - 8f * s),
                        icon, ScaleMode.StretchToFill, true);

        // Phan dang hoi chieu bi toi di, va SANG DAN LEN khi sap xong
        if (cooldown01 > 0.001f)
        {
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.28f + 0.42f * cooldown01);
            GUI.DrawTexture(r, barTex, ScaleMode.StretchToFill, true);

            // Mep nuoc dang rut xuong: cho biet con bao nhieu phan
            GUI.color = new Color(0f, 0f, 0f, 0.42f);
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, r.height * cooldown01),
                            barTex, ScaleMode.StretchToFill, true);
            GUI.color = prev;

            if (player != null) VeSoGiay(r, player.HoiChieuGiay(skill), s);
        }

        // Vien vang neu dang chon
        if (selected) DrawBorder(r, new Color(1f, 0.85f, 0.35f), Mathf.Max(2f, 3f * s));
        else DrawBorder(r, new Color(0.35f, 0.32f, 0.28f), Mathf.Max(1f, 2f * s));

        // Chi ve PHIM TAT, khong ve ten ky nang. Bay bieu tuong keo theo bay
        // dong chu "[1/Z] QUA CAU LUA" lam day man hinh, ma nguoi choi PC
        // nho ky nang theo bieu tuong va theo phim so chu khong doc chu.
        // Ban cam ung (VeNutKyNangTron) von chi ve bieu tuong, khong ve chu -
        // gio hai ban giong nhau o cho nay.
        GUI.Label(new Rect(r.x, r.y + r.height + 2f * s, r.width, 22f * s),
                  "[" + key + "]", keyStyle);

        // Bam thang vao o cung tung duoc ky nang. Luc nay con tro dang o tren
        // thanh ky nang nen khong ngam theo chuot duoc -> danh thang ve phia truoc.
        if (player != null && GUI.Button(r, GUIContent.none, GUIStyle.none))
            player.CastForward(skill);
    }

    /// <summary>
    /// So giay con lai, ve giua nut.
    ///
    /// Duoi 10 giay thi hien mot chu so le (2.4) - cho ky nang hoi nhanh nhu
    /// Qua cau lua, "0" tron treo suot nua giay khong cho biet gi. Tu 10 giay
    /// tro len thi lam tron, khong ai can biet 11,3 hay 11,4.
    ///
    /// <paramref name="tam"/> tinh tu DUOI len giong toa do cham.
    /// </summary>
    void VeSoGiay(Rect o, float giay, float s)
    {
        if (giay <= 0.05f) return;

        string chu = giay >= 10f
            ? Mathf.CeilToInt(giay).ToString()
            : giay.ToString("0.0");

        if (soGiayStyle == null) soGiayStyle = new GUIStyle(GUI.skin.label);

        // Dat LAI moi lan ve, khong chi luc tao. Dat mot lan luc tao thi so
        // giay bam o goc tren-trai o ky nang chu khong vao giua - co cai gi do
        // ghi de len style sau do, va sua o cho tao thi khong an.
        soGiayStyle.alignment = TextAnchor.MiddleCenter;
        soGiayStyle.fontStyle = FontStyle.Bold;
        soGiayStyle.padding = new RectOffset(0, 0, 0, 0);
        soGiayStyle.margin = new RectOffset(0, 0, 0, 0);
        soGiayStyle.contentOffset = Vector2.zero;
        soGiayStyle.wordWrap = false;
        soGiayStyle.clipping = TextClipping.Overflow;
        soGiayStyle.fixedWidth = 0f;
        soGiayStyle.fixedHeight = 0f;
        soGiayStyle.stretchWidth = true;
        soGiayStyle.stretchHeight = true;
        soGiayStyle.fontSize = Mathf.RoundToInt(Mathf.Max(12f, o.height * 0.36f));

        // Vien toi truoc: chu sang tren mot bieu tuong cung sang thi chim mat
        soGiayStyle.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
        float d = Mathf.Max(1f, 2f * s);
        foreach (var l in new[] { new Vector2(-d, 0), new Vector2(d, 0),
                                  new Vector2(0, -d), new Vector2(0, d) })
            GUI.Label(new Rect(o.x + l.x, o.y + l.y, o.width, o.height), chu, soGiayStyle);

        soGiayStyle.normal.textColor = new Color(1f, 0.96f, 0.86f);
        GUI.Label(o, chu, soGiayStyle);
    }

    /// <summary>
    /// Cung tien trinh chay quanh nut: phan DA hoi xong hien sang.
    ///
    /// OnGUI khong ve duoc hinh quat, nen cung nay ghep tu nhung cham nho chay
    /// doc vong tron. Bat dau tu dinh nut va chay theo chieu kim dong ho.
    /// </summary>
    void VeCungHoiChieu(Vector2 tam, float r, float xong01, float s)
    {
        const int SoCham = 44;
        int n = Mathf.RoundToInt(SoCham * Mathf.Clamp01(xong01));
        if (n <= 0) return;

        float co = Mathf.Max(2f, 5f * s);
        var cu = GUI.color;
        GUI.color = new Color(1f, 0.90f, 0.55f, 0.90f);
        for (int i = 0; i < n; i++)
        {
            float g = Mathf.PI * 0.5f - (i / (float)SoCham) * Mathf.PI * 2f;
            float x = tam.x + Mathf.Cos(g) * r;
            float y = tam.y + Mathf.Sin(g) * r;
            GUI.DrawTexture(new Rect(x - co * 0.5f, Screen.height - y - co * 0.5f, co, co),
                            vongNum, ScaleMode.StretchToFill, true);
        }
        GUI.color = cu;
    }

    void DrawBorder(Rect r, Color c, float t)
    {
        var prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), barTex);
        GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), barTex);
        GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), barTex);
        GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), barTex);
        GUI.color = prev;
    }

    // ================================================================
    //  THONG TIN TREN DINH MAN HINH
    // ================================================================

    void DrawTopInfo(float s)
    {
        float w = 320f * s, h = 76f * s;
        var r = new Rect((Screen.width - w) * 0.5f, 10f * s, w, h);
        GUI.DrawTexture(r, panelTex, ScaleMode.StretchToFill, true);

        var st = new GUIStyle(midStyle);
        st.alignment = TextAnchor.MiddleCenter;

        if (director != null)
        {
            string line1 = "DOT " + Mathf.Max(1, director.Wave) + "   -   Quai con lai: " + director.Alive;
            string line2 = "Da diet: " + director.Kills;
            if (director.NextWaveIn > 0.01f && director.Alive == 0)
                line2 = "Dot moi sau " + Mathf.CeilToInt(director.NextWaveIn) + " giay...";

            GUI.Label(new Rect(r.x, r.y + 4f * s, r.width, 34f * s), line1, st);
            GUI.Label(new Rect(r.x, r.y + 36f * s, r.width, 34f * s), line2, st);
        }

        // KHONG ve ten goc nhin nua. Nguoi choi biet minh dang o goc nhin nao vi
        // dang nhin thay no; dong chu chi chiem cho ngay duoi bang trang thai.
    }

    // ================================================================
    //  THONG BAO / HUONG DAN
    // ================================================================

    void DrawMessages(float s)
    {
        // Dong bao khi vua doi che do PC <-> cam ung (phim F9)
        float tuoi = Time.time - baoLuc;
        if (!string.IsNullOrEmpty(baoChu) && tuoi < 2f)
        {
            var st = new GUIStyle(midStyle);
            st.alignment = TextAnchor.MiddleCenter;
            st.fontSize = Mathf.RoundToInt(22f * s);
            st.normal.textColor = new Color(1f, 0.92f, 0.6f, Mathf.Clamp01(2f - tuoi));
            GUI.Label(new Rect(0f, Screen.height * 0.22f, Screen.width, 40f * s), baoChu, st);
        }

        // Huong dan dau man
        if (Time.time - startTime < hintDuration)
        {
            string[] lines =
            {
                "CHUOT TRAI: di chuyen        GIU CHUOT PHAI: tu xoay camera",
                "PHIM 1: Qua cau lua   2: Mua bang   3: Sam set   4: Loc xoay (cuon quai len troi)",
                "Hang so bi ket (bo go tieng Viet) thi dung Z X V B thay cho 1 2 3 4",
                "PHIM C: doi goc nhin 3D / 2.5D / 2D      Q,E: xoay      Con lan chuot: phong to",
                "WASD: di chuyen truc tiep     R: choi lai     ESC: ve man hinh chinh",
            };

            float w = 900f * s, lh = 30f * s;
            float h = lh * lines.Length + 16f * s;
            var r = new Rect((Screen.width - w) * 0.5f, Screen.height * 0.30f, w, h);

            float fade = Mathf.Clamp01(hintDuration - (Time.time - startTime));
            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, fade);
            GUI.DrawTexture(r, panelTex, ScaleMode.StretchToFill, true);

            var st = new GUIStyle(smallStyle);
            st.alignment = TextAnchor.MiddleCenter;
            for (int i = 0; i < lines.Length; i++)
                GUI.Label(new Rect(r.x, r.y + 8f * s + i * lh, r.width, lh), lines[i], st);
            GUI.color = prev;
        }

        // Thong bao ngan (het nang luong, doi ky nang...)
        if (player != null && Time.time - player.LastMessageTime < 1.6f && !string.IsNullOrEmpty(player.LastMessage))
        {
            var st = new GUIStyle(midStyle);
            st.alignment = TextAnchor.MiddleCenter;
            st.normal.textColor = new Color(1f, 0.85f, 0.5f);
            GUI.Label(new Rect(0f, Screen.height * 0.62f, Screen.width, 40f * s), player.LastMessage, st);
        }

        // Man hinh thua
        if (director != null && director.PlayerDead)
        {
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), barTex);
            GUI.color = prev;

            var st = new GUIStyle(bigStyle);
            st.normal.textColor = new Color(0.9f, 0.15f, 0.12f);
            GUI.Label(new Rect(0f, Screen.height * 0.40f, Screen.width, 60f * s), "BAN DA GUC NGA", st);

            var st2 = new GUIStyle(midStyle);
            st2.alignment = TextAnchor.MiddleCenter;

            // May cam ung KHONG CO BAN PHIM. Nhac "bam R / ESC" o day la chi ra
            // hai cai phim ma nguoi choi khong the bam - thay bang mot cai nut.
            string tomTat = "Da diet " + director.Kills + " quai o dot " + director.Wave + ".";
            if (!CamUng.DangDung)
                tomTat += GameDirector.DuocChoiLai
                    ? "  Bam R de choi lai, ESC de ve man hinh chinh."
                    // Choi mang thi khong co "choi lai" - chi co ve sanh
                    : "  Bam ESC de ve sanh.";

            GUI.Label(new Rect(0f, Screen.height * 0.50f, Screen.width, 40f * s), tomTat, st2);

            if (CamUng.DangDung) VeNutTroVe(s);
        }
        else if (KhoiDongTranMang.CanNutVeSanh && CamUng.DangDung)
        {
            VeNutTroVe(s);
        }

        VeBangChanDoan(s);
    }

    /// <summary>Bang chan doan goc trai tren man hinh (F12 de tat).</summary>
    void VeBangChanDoan(float s)
    {
        if (!hienChanDoan) return;

        bool chet = player != null && player.health != null && player.health.IsDead;

        string[] dong =
        {
            "CHAN DOAN (F12 de tat)",
            "So vong Update  : " + PlayerController.SoVongUpdate
                + (chet ? "  DUNG VI NHAN VAT DA CHET" : "  dang chay"),
            "Phim bat ky     : " + PlayerController.SoLanBamPhim
                + (PlayerController.SoLanBamPhim == 0 ? "  <- BAN PHIM KHONG TOI DUOC GAME" : ""),
            "Phim ky nang    : " + PlayerController.SoLanBamKyNang,
            "Nhan vat        : " + (chet ? (GameDirector.DuocChoiLai ? "DA CHET - bam R de choi lai"
                                                             : "DA CHET (tran mang - khong choi lai duoc)")
                                          : "con song"),
            "Cach tung chieu : 1 2 3 4  /  Z X V B  /  bam vao o ky nang",

            // Phan CAM UNG. Tren dien thoai khong co Console de xem log, nen
            // moi thu can biet phai hien thang len man hinh: che do co bat
            // khong, cham co toi duoc game khong, can co nhan duoc khong.
            "--- CAM UNG (F9 de doi che do) ---",
            "Che do          : " + (CamUng.DangDung ? "CAM UNG" : "PC")
                + "   (epCamUng=" + epCamUng + ")",
            // touchSupported van in ra, nhung chi de tham khao: no bao TRUE
            // tren gan nhu moi may tinh Windows doi moi nen KHONG duoc dung
            // lam thuoc do - xem ghi chu o CamUng.DangDung.
            "Nen tang        : " + Application.platform
                + " | mobile=" + Application.isMobilePlatform,
            "Trinh duyet bao : " + (CamUng.TraLoiCuaTrinhDuyet < 0 ? "chua hoi"
                : CamUng.TraLoiCuaTrinhDuyet == 1 ? "DI DONG" : "MAY TINH")
                + "   (touchSupported=" + Input.touchSupported + ", chi tham khao)",
            "So ngon dang cham: " + Input.touchCount
                + " | chuot trai=" + Input.GetMouseButton(0),
            "Can joystick    : " + (CamUng.DangKeo ? "DANG KEO" : "tha")
                + "  huong=" + CamUng.Huong.ToString("F2"),
            // Trinh duyet dien thoai tu bien hai ngon tren man hinh thanh cu chi
            // CUON / PINCH cua trang web, roi bao cho Unity thanh con lan chuot.
            // Dong nay hien CA gia tri tho (trinh duyet gui gi) LAN do lech that
            // cua camera: neu cot tho co so ma camera van dung 0 la khoa dang chay.
            "Camera          : chuot/con lan "
                + (CamUng.DangDung ? "DA KHOA" : "dang bat")
                + "   (trinh duyet gui: lan="
                + Input.GetAxis("Mouse ScrollWheel").ToString("F3")
                + " phai=" + Input.GetMouseButton(1)
                + " giua=" + Input.GetMouseButton(2) + ")",
            "  cu chi cua ta : keo=" + CamUng.XoayCam.ToString("F0")
                + " chum=" + CamUng.ChumZoom.ToString("F0")
                + " | ngon giu can=" + (ngonJoy != -1)
                + " ngon bam nut=" + ngonNut.Count,
            "  do lech camera: yaw=" + (cameraRig != null ? cameraRig.YawOffset : 0f).ToString("F2")
                + " chuc=" + (cameraRig != null ? cameraRig.PitchOffset : 0f).ToString("F2")
                + " xa=" + (cameraRig != null ? cameraRig.ZoomOffset : 0f).ToString("F2"),

            "Cham-de-di      : " + (CamUng.DangDung ? "DA KHOA" : "dang bat")
                + "  |  dang chay toi diem bam: "
                + (player != null && player.DangCoDiemDen ? "CO - VAN CHUA KHOA!" : "khong"),
        };

        float lh = 17f * s;
        var r = new Rect(8f * s, 8f * s, 470f * s, lh * dong.Length + 12f * s);

        var truoc = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.82f);
        GUI.DrawTexture(r, panelTex, ScaleMode.StretchToFill, true);
        GUI.color = truoc;

        var st = new GUIStyle(smallStyle);
        st.alignment = TextAnchor.MiddleLeft;
        for (int i = 0; i < dong.Length; i++)
        {
            st.normal.textColor = i == 0 ? new Color(1f, 0.82f, 0.45f)
                                         : new Color(0.86f, 0.88f, 0.9f);
            GUI.Label(new Rect(r.x + 8f * s, r.y + 6f * s + i * lh, r.width - 16f * s, lh),
                      dong[i], st);
        }
    }

    // ================================================================
    //  VE CAC ANH DUNG CHO HUD
    // ================================================================

    static Texture2D SolidTexture(Color c)
    {
        var t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        t.SetPixels(px); t.Apply();
        return t;
    }

    /// <summary>
    /// DAI MAU DOC cho thanh chi so: sang o tren, dam nhat o giua, toi lai o
    /// duoi.
    ///
    /// Ba chang chu khong phai hai. Chuyen thang tu sang xuong toi chi ra mot
    /// mat phang nghieng; con sang - dam - toi lam mat thanh cong len nhu mot
    /// nua ong tru, va do la thu cho cam giac KHOI.
    ///
    /// Anh cao 64 diem, rong 2 - GUI keo ngang bao nhieu cung duoc vi mau chi
    /// doi theo chieu doc.
    /// </summary>
    /// <summary>
    /// Anh HINH TRON de ve nut va can joystick.
    ///
    /// <paramref name="loiTrong"/> = ban kinh phan ruot (0..1) so voi ca anh.
    /// De 0 thi ra dia dac; de lon thi ra mot cai vanh.
    ///
    /// Vien duoc lam MEM bang cach lay do phu theo khoang cach: cat cung o mep
    /// thi vong tron hien ra rang cua, nhat la o co nho tren dien thoai.
    /// </summary>
    static Texture2D VongTron(int co, Color mau, float ngoai, float loiTrong)
    {
        var t = new Texture2D(co, co, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[co * co];
        float nua = co * 0.5f;
        float mem = 1.5f / nua;

        for (int y = 0; y < co; y++)
            for (int x = 0; x < co; x++)
            {
                float dx = (x + 0.5f - nua) / nua, dy = (y + 0.5f - nua) / nua;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float a = Mathf.Clamp01((ngoai - d) / mem);
                if (loiTrong > 0.001f)
                    a *= Mathf.Clamp01((d - loiTrong) / mem);

                px[y * co + x] = new Color(mau.r, mau.g, mau.b, mau.a * a);
            }
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    public static Texture2D GradientDoc(Color tren, Color giua, Color duoi)
    {
        const int H = 64;
        var t = new Texture2D(2, H, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;

        var px = new Color[2 * H];
        for (int y = 0; y < H; y++)
        {
            // y = 0 la DAY anh trong Unity, nen dao lai cho khop voi tren/duoi
            float v = 1f - y / (float)(H - 1);
            Color c = v < 0.5f
                ? Color.Lerp(giua, duoi, (0.5f - v) * 2f)
                : Color.Lerp(giua, tren, (v - 0.5f) * 2f);
            px[y * 2] = c;
            px[y * 2 + 1] = c;
        }
        t.SetPixels(px);
        t.Apply();
        return t;
    }



    /// <summary>Khoang cach tu mot diem toi mot doan thang.</summary>
    static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len2 = ab.sqrMagnitude;
        if (len2 < 1e-6f) return Vector2.Distance(p, a);
        float k = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
        return Vector2.Distance(p, a + ab * k);
    }
}
