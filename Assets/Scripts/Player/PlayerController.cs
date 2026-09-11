using UnityEngine;

/// <summary>
/// DIEU KHIEN PHU THUY.
///
/// - Chuot TRAI bam xuong dat: di chuyen den do (kieu Diablo). Giu chuot de di lien tuc.
/// - Phim 1 / 2 / 3 / 4 (hoac bam vao o ky nang duoi man hinh): DANH RA NGAY
///   ve phia con tro chuot. Khong con phai chon truoc roi bam chuot phai nua.
/// - Chuot PHAI: GIU rôi keo de tu xoay camera. Tha ra thi camera giu nguyen goc do.
/// - WASD / mui ten: di chuyen truc tiep (huong theo camera).
///
/// Cach dieu khien nay chay dung o CA BA goc nhin 3D / 2.5D / 2D vi
/// diem ngam luon duoc ban ra tu camera xuong mat dat.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Di chuyen")]
    public float moveSpeed = 5.2f;

    /// <summary>Co thi loi nuoc se lam cham lai. Khong co cung chay binh thuong.</summary>
    public LoiNuoc loiNuoc;
    public float turnSpeed = 12f;
    public float stopDistance = 0.25f;

    [Header("Nang luong")]
    public float maxMana = 250f;
    public float mana = 250f;
    public float manaRegen = 9f;

    [Header("Ky nang 1 - Qua cau lua")]
    public float fireballCost = 10f;
    public float fireballCooldown = 0.55f;
    public float fireballCastTime = 0.38f;

    [Header("Ky nang 2 - Mua bang")]
    [Tooltip("Tam tha xa nhat, met. Ngam xa hon thi phep roi o MEP tam.")]
    public float iceRange = 12f;
    public float iceCost = 34f;
    public float iceCooldown = 6f;
    public float iceCastTime = 0.75f;

    [Header("Ky nang 3 - Sam set")]
    [Tooltip("Tam tha xa nhat, met. Ngam xa hon thi phep roi o MEP tam.")]
    public float boltRange = 12f;
    public float boltCost = 42f;
    public float boltCooldown = 8f;
    public float boltCastTime = 0.62f;

    [Header("Ky nang 4 - Loc xoay")]
    public float tornadoCost = 55f;
    public float tornadoCooldown = 2f;
    public float tornadoCastTime = 0.72f;

    [Header("Ky nang 5 - Thien thach")]
    public float meteorCost = 60f;
    public float meteorCooldown = 2f;
    public float meteorCastTime = 0.85f;

    [Header("Ky nang 6 - Khieng bao ve")]
    public float khiengCost = 45f;
    public float khiengCooldown = 12f;
    public float khiengCastTime = 0.55f;

    [Tooltip("Mau cua khieng - het mau thi khieng no tung")]
    public float khiengMau = 150f;

    [Tooltip("Ban kinh qua cau khieng, met")]
    public float khiengBanKinh = 3.036f;     // rong hon 15% so voi 2,64 m

    [Header("Ky nang 7 - Giut set")]
    // 14 = 28 / 2. Giam mot nua theo yeu cau.
    //
    // ⚠️ Sua o day CHUA AN: prefab Player_Sorceress luu san mot ban va gia tri
    // trong prefab THANG gia tri mac dinh trong code. Da vap dung loi nay khi
    // ha hoi chieu tu 4 xuong 0,55 - code bao 0,55 ma trong game van 4 giay.
    public float giatSetCost = 14f;
    // 0,55 giay - bang Qua cau lua. Gia 28 nang luong moi lan ban van la thu
    // ghim toc do lai: hoi day 130 nang luong voi manaRegen 9/giay thi ban lien
    // tuc duoc chung nam lan roi phai cho hoi.
    public float giatSetCooldown = 0.4f;
    public float giatSetCastTime = 0.34f;

    [Header("Tham chieu")]
    public CharacterRig rig;
    public ProceduralAnimator anim;
    [Tooltip("Nhan vat dung model san (khong co ProceduralAnimator) thi gan bo hoat hinh rieng vao day")]
    public NguoiChoiHoatHinh hoatHinhRieng;
    public Damageable health;
    public CameraRig cameraRig;

    public int SelectedSkill { get; private set; }
    public float FireballCooldown01 { get { return Mathf.Clamp01(fireballTimer / fireballCooldown); } }
    public float IceCooldown01 { get { return Mathf.Clamp01(iceTimer / iceCooldown); } }
    public float BoltCooldown01 { get { return Mathf.Clamp01(boltTimer / boltCooldown); } }
    public float TornadoCooldown01 { get { return Mathf.Clamp01(tornadoTimer / tornadoCooldown); } }
    public float MeteorCooldown01 { get { return Mathf.Clamp01(meteorTimer / meteorCooldown); } }
    public float KhiengCooldown01 { get { return Mathf.Clamp01(khiengTimer / khiengCooldown); } }
    public float GiatSetCooldown01 { get { return Mathf.Clamp01(giatSetTimer / giatSetCooldown); } }

    /// <summary>
    /// Con bao nhieu GIAY nua thi dung duoc ky nang <paramref name="skill"/>.
    /// Bang 0 la dung duoc ngay.
    ///
    /// Bang ty le 0..1 o tren khong thay duoc dieu nay: 0,5 cua Qua cau lua la
    /// mot phan tu giay, con 0,5 cua Khieng la sau giay - nguoi choi can biet
    /// con phai cho bao lau chu khong phai con bao nhieu phan tram.
    /// </summary>
    public float HoiChieuGiay(int skill)
    {
        switch (skill)
        {
            case 0: return Mathf.Max(0f, fireballTimer);
            case 1: return Mathf.Max(0f, iceTimer);
            case 2: return Mathf.Max(0f, boltTimer);
            case 3: return Mathf.Max(0f, tornadoTimer);
            case 4: return Mathf.Max(0f, meteorTimer);
            case 5: return Mathf.Max(0f, khiengTimer);
            case 6: return Mathf.Max(0f, giatSetTimer);
        }
        return 0f;
    }

    /// <summary>Khieng con bao nhieu mau, 0..1. Bang 0 la khong co khieng.</summary>
    public float KhiengMau01 { get { return khiengHienTai != null ? khiengHienTai.Mau01 : 0f; } }
    public float Mana01 { get { return maxMana <= 0f ? 0f : Mathf.Clamp01(mana / maxMana); } }
    public bool IsCasting { get { return castTimer > 0f; } }
    public string LastMessage { get; private set; }
    public float LastMessageTime { get; private set; } = -99f;

    CharacterController cc;
    LayerMask groundMask, enemyMask, obstacleMask;

    // ================================================================
    //  Y MUON CUA NGUOI CHOI - DEN TU DAU
    // ================================================================

    /// <summary>
    /// Y muon cua khung hinh dang xu ly. Choi mot minh thi
    /// <see cref="DocInput"/> dien vao moi khung; choi mang thi may trong tai
    /// nhan goi nay QUA DUONG TRUYEN roi thi hanh y het.
    /// </summary>
    public GoiInput input = GoiInput.Rong(0f);

    /// <summary>
    /// Tu doc ban phim hay cho ngoai bom vao.
    ///
    /// Bat (mac dinh): nhan vat cua chinh nguoi ngoi truoc man hinh.
    /// Tat: nhan vat cua nguoi khac, hoac dang chay lai mot chuoi input de
    /// hieu chinh - luc do ban phim cua may nay khong duoc dinh vao.
    /// </summary>
    public bool tuDocInput = true;

    DocInput boDoc;

    Vector3 velocity;
    Vector3 moveTarget;
    bool hasMoveTarget;

    /// <summary>
    /// Nhan vat co dang tu chay ve mot diem da bam khong.
    ///
    /// Cong khai de bang chan doan hien duoc: tren dien thoai khong co Console,
    /// nen muon biet cham-de-di da thuc su khoa chua thi phai NHIN THAY co nay.
    /// </summary>
    public bool DangCoDiemDen { get { return hasMoveTarget; } }

    /// <summary>Dang niem chu hay khong - mo ra cho kich ban chay thu doc.</summary>
    public bool DangNiemChu { get { return castTimer > 0f; } }

    /// <summary>Phep cua nhan vat nay danh trung nhung lop nao - chi de doc.</summary>
    public int MatNaKeThu { get { return enemyMask; } }

    /// <summary>Qua cau lua dung lai khi cham nhung lop nao - chi de doc.</summary>
    public int MatNaVatCan { get { return obstacleMask; } }

    float fireballTimer, iceTimer, boltTimer, tornadoTimer;
    float castTimer, castTotal, meteorTimer, khiengTimer, giatSetTimer;
    Khieng khiengHienTai;
    int castingSkill = -1;
    Vector3 castAim;
    bool castReleased;
    GameObject chargeVfx;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        // Tu gan phan loi nuoc. Gan o day chu khong gan luc dung ban do vi quai
        // duoc tao tu prefab, con nguoi choi thi moi man mot duong dung khac
        // nhau - gan o Awake thi cho nao cung co, va man khong co nuoc thi
        // LoiNuoc tra ve he so 1 nen khong ton gi.
        if (loiNuoc == null)
        {
            loiNuoc = GetComponent<LoiNuoc>();
            if (loiNuoc == null) loiNuoc = gameObject.AddComponent<LoiNuoc>();
        }
        if (rig == null) rig = GetComponentInChildren<CharacterRig>();
        if (anim == null) anim = GetComponentInChildren<ProceduralAnimator>();
        if (health == null) health = GetComponent<Damageable>();

        groundMask = LayerMask.GetMask("Ground", "Default");

        // KHONG them lop Player vao day, du qua cau tung bay xuyen qua nguoi.
        // Da thu va hong: qua cau sinh ra NGAY BEN TRONG collider cua chinh
        // nguoi tung, nen no no ngay tren dau ho. Cho no nhan biet nguoi choi
        // bang cach khac - xem Fireball.Update, no tu do xem co ai trong tam
        // an don khong.
        obstacleMask = LayerMask.GetMask("Enemy", "Ground", "Default");

        // BAT PvP CHI BANG MOT DONG.
        //
        // Moi ky nang deu nhan damageMask tu ben ngoai, va enemyMask la NOI
        // DUY NHAT cap mask do - nen them lop Player vao day la ca bay phep
        // deu danh duoc nguoi choi khac, khong phai sua tung phep mot.
        //
        // Doi lai, phep cung danh duoc CHINH MINH: qua cau lua no ngay duoi
        // chan se giet nguoi vua bam phim. Nen moi ky nang deu duoc giao
        // "boQua = health" - xem Release().
        enemyMask = TranHienTai.DangChoiMang
            ? LayerMask.GetMask("Enemy", "Player")
            : LayerMask.GetMask("Enemy");

        // Tu gan bo doc input. Gan o day chu khong bat nguoi dung keo tay vao
        // prefab: nhan vat duoc dung tu code o ca hai man, quen mot cho la mot
        // man khong dieu khien duoc ma khong bao gi.
        boDoc = GetComponent<DocInput>();
        if (boDoc == null) boDoc = gameObject.AddComponent<DocInput>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Doc y muon TRUOC khi xet song chet: nguoi da guc van bam phim, va
        // van phai duoc nhac mot cau.
        if (tuDocInput && boDoc != null) input = boDoc.Doc(dt);

        // BAN SAO CUA NGUOI CHOI KHAC KHONG DOC PHIM, NHUNG DONG HO VAN PHAI CHAY.
        //
        // Loi da vap: ban sao giu nguyen goi y muon cu, ma goi ay co dt = 0.
        // The la ThiHanhMotKhung chay voi dt = 0 - dong ho niem chu dung im,
        // castTimer khong bao gio giam, va phep KHONG BAO GIO bay ra. Nhin tu
        // may ben kia thi nguoi ta bam phep ma chang thay gi ca, dung hien
        // tuong nguoi choi bao. Hoi chieu cung dong bang y het.
        else input.dt = dt;

        ThiHanhMotKhung(input);
    }

    /// <summary>
    /// THI HANH DUNG MOT KHUNG HINH THEO MOT GOI Y MUON.
    ///
    /// Tach rieng khoi <c>Update</c> de con GOI LAI DUOC. Khi may trong tai
    /// bao "toi da xu ly toi so N va ket qua la day", may nguoi choi dat lai
    /// trang thai roi chay lai tung goi tu N+1 - xem <see cref="DuDoan"/>.
    ///
    /// DUNG <c>g.dt</c> CHU KHONG PHAI <c>Time.deltaTime</c>: luc chay lai,
    /// mot khung hinh cu 16,7 ms phai duoc thi hanh dung 16,7 ms, chu khong
    /// phai theo nhip khung hinh hien tai. Sai cho nay thi moi lan hieu chinh
    /// nhan vat lai nhay mot doan.
    /// </summary>
    public void ThiHanhMotKhung(GoiInput g)
    {
        input = g;
        float dt = g.dt;

        if (health != null && health.IsDead)
        {
            if (anim != null) anim.SetMoveSpeed(0f);

            // NGUOI CHET THI KHONG TUNG CHIEU DUOC - nhung PHAI NOI RA.
            // Truoc day cho no lang le thoat ngay o day: nguoi choi bam 1/2/3/4
            // khong thay gi xay ra, tuong ky nang hong, trong khi that ra nhan vat
            // da guc tu luc nao. Bam phim ky nang luc nay thi nhac lai cho biet.
            if (g.kyNang >= 0)
                Say("BẠN ĐÃ GỤC NGÃ - bấm R để chơi lại");
            return;
        }

        mana = Mathf.Min(maxMana, mana + manaRegen * dt);
        if (fireballTimer > 0f) fireballTimer -= dt;
        if (iceTimer > 0f) iceTimer -= dt;
        if (boltTimer > 0f) boltTimer -= dt;
        if (tornadoTimer > 0f) tornadoTimer -= dt;
        if (meteorTimer > 0f) meteorTimer -= dt;
        if (khiengTimer > 0f) khiengTimer -= dt;
        if (giatSetTimer > 0f) giatSetTimer -= dt;

        HandleSkillSelect();
        HandleCasting(dt);
        HandleMovement(dt);
    }

    // ================================================================
    //  CHON VA TUNG KY NANG
    // ================================================================

    // ---- SO DEM CHAN DOAN ----
    // Bat tan tay xem phim co toi duoc game hay khong. Khong doan mo nua:
    //   SoVongUpdate khong tang  -> Update khong chay (nhan vat chet, hoac bi tat)
    //   SoVongUpdate tang, SoLanBamPhim = 0 -> phim KHONG toi duoc game
    //   SoLanBamPhim tang ma khong ra chieu -> loi nam trong CastAtCursor
    public static int SoVongUpdate;
    public static int SoLanBamPhim;
    public static int SoLanBamKyNang;
    public static string PhimGanNhat = "chua bam phim nao";

    void HandleSkillSelect()
    {
        SoVongUpdate++;

        // Quet TOAN BO ban phim, khong chi bon phim ky nang: neu nguoi choi bam
        // phim nao do ma o day khong thay gi ca thi chac chan la phim bi cai khac
        // (bo go tieng Viet chang han) chan mat, chu khong phai loi ky nang.
        if (input.coBamPhim)
        {
            SoLanBamPhim++;
            PhimGanNhat = "co phim (vong " + SoVongUpdate + ")";
        }

        // Bam phim la DANH RA NGAY ve phia con tro chuot.
        // Chuot phai khong con dung de tung ky nang - no danh cho viec xoay camera.
        int ky = input.kyNang;
        if (ky >= 0)
        {
            SoLanBamKyNang++;
            CastAtCursor(ky);
        }
    }

    /// <summary>
    /// Tung ky nang tren may CAM UNG: tu tim ke dich, khong can ngam.
    ///
    /// Uu tien ke dich nam theo HUONG DANG DAY CAN. Nguoi choi day can ve phia
    /// nao la co y nham ve phia do, du ngon tay kia dang o tan goc man hinh.
    /// Khong day can thi lay huong nhan vat dang quay mat.
    ///
    /// Khong co ke dich nao trong tam thi van danh, ra phia truoc - im lang
    /// khong lam gi thi nguoi choi tuong nut hong.
    /// </summary>
    public void DanhTuDong(int skill)
    {
        Vector3 huong = HuongNgamCamUng();
        Vector3 diem;

        var muc = TimKeDichGanNhat(huong);
        if (muc != null)
            diem = muc.position;
        else
        {
            diem = transform.position + huong * 9f;
            diem.y = VfxFactory.GroundY(diem);
        }

        SelectedSkill = skill;
        CastAt(skill, diem);
    }

    // ================================================================
    //  NGAM CO DINH HUONG (an giu nut ky nang roi keo ra)
    // ================================================================

    /// <summary>
    /// Doi mot do lech TREN MAN HINH thanh huong TRONG THE GIOI.
    ///
    /// Phai di qua <see cref="CameraRig.FlatForward"/> chu khong dung thang truc
    /// the gioi: nguoi choi xoay goc nhin di 90 do roi keo len tren man hinh thi
    /// y ho la "ve phia tren man hinh", khong phai "ve huong Bac cua ban do".
    /// </summary>
    public Vector3 HuongTuManHinh(Vector2 lech)
    {
        Vector3 fwd = cameraRig != null ? cameraRig.FlatForward : Vector3.forward;
        Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);

        Vector3 h = fwd * lech.y + right * lech.x;
        h.y = 0f;
        if (h.sqrMagnitude > 0.0001f) return h.normalized;

        Vector3 mat = transform.forward;
        mat.y = 0f;
        return mat.sqrMagnitude > 0.0001f ? mat.normalized : Vector3.forward;
    }

    /// <summary>
    /// Ky nang nay danh THANG mot duong (true) hay roi xuong MOT DIEM (false).
    ///
    /// Hai kieu ve duong ngam khac han nhau: don thang ve mot lan duong keo dai
    /// het tam, con don theo diem ve mot vanh tam quanh nguoi choi cong mot
    /// vong o cho phep se roi.
    /// </summary>
    public bool DonThang(int skill)
    {
        // 0 qua cau lua, 3 loc xoay, 6 giut set - deu di theo mot duong tu
        // nguoi choi ra. Con lai roi xuong mot diem da chon.
        return skill == 0 || skill == 3 || skill == 6;
    }

    /// <summary>
    /// Tam XA NHAT ma nguoi choi ngam duoc, tinh bang met.
    ///
    /// KHAC <see cref="TamCuaKyNang"/>: ham kia la tam CUNG cua phep (chi Mua
    /// bang va Sam set co), con day la do dai duong ngam VE RA - qua cau lua
    /// khong bi kep tam nhung van phai ve mot doan huu han, khong thi duong ngam
    /// keo ra vo tan.
    /// </summary>
    public float TamNgam(int skill)
    {
        switch (skill)
        {
            case 0: return 14f;              // qua cau lua bay den khi cham
            case 1: return iceRange;         // 12
            case 2: return boltRange;        // 12
            case 3: return 12f;              // loc xoay truot ve phia ngam
            case 4: return 18f;              // thien thach roi tu tren troi
            case 5: return khiengBanKinh;    // khieng: quanh chinh minh
            case 6: return TamGiatSet();
            default: return 12f;
        }
    }

    /// <summary>
    /// Ban kinh VUNG GAY SAT THUONG cua ky nang, tinh bang met.
    ///
    /// Doc tu PREFAB truoc, hang so trong code chi la duong lui.
    ///
    /// ⚠️ Bat buoc phai doc prefab: gia tri luu trong prefab DE LEN gia tri mac
    /// dinh trong code. Lay hang so o day thi duong ngam ve mot vong to 5,5 m
    /// trong khi phep that no 7 m - nguoi choi ngam theo vong ve va cu truot
    /// hoai ma khong hieu vi sao.
    /// </summary>
    public float BanKinhSatThuong(int skill)
    {
        var kho = GameAssets.I;

        switch (skill)
        {
            case 0:
            {
                var pf = kho != null ? kho.fireballPrefab : null;
                var c = pf != null ? pf.GetComponent<Fireball>() : null;
                return c != null ? c.blastRadius : 3.4f;
            }
            case 1:
            {
                var pf = kho != null ? kho.iceStormPrefab : null;
                var c = pf != null ? pf.GetComponent<IceStorm>() : null;
                return c != null ? c.radius : 5.5f;
            }
            case 2:
            {
                var pf = kho != null ? kho.lightningStormPrefab : null;
                var c = pf != null ? pf.GetComponent<LightningStorm>() : null;
                return c != null ? c.radius : 6f;
            }
            case 3:
            {
                var pf = kho != null ? kho.tornadoPrefab : null;
                var c = pf != null ? pf.GetComponent<Tornado>() : null;
                return c != null ? c.catchRadius : 3.6f;
            }
            case 4:  return 4.2f;             // ThienThach.blastRadius
            case 5:  return khiengBanKinh;
            case 6:  return 2.2f;             // tam lan cua tia set
            default: return 2f;
        }
    }

    /// <summary>
    /// Tam cua Giut set.
    ///
    /// Hang so chu khong doc prefab: Giut set KHONG co prefab trong GameAssets,
    /// no duoc dung thang bang code trong GiatSet.Phong. Sua GiatSet.range thi
    /// phai sua ca con so nay.
    /// </summary>
    float TamGiatSet() { return 19.5f; }

    /// <summary>
    /// Diem ma ky nang se roi vao, tinh tu huong ngam va do dai da keo.
    ///
    /// Don THANG thi luon lay het tam: keo dai ngan chi de chon HUONG, khong ai
    /// muon ban qua cau lua roi giua duong. Don theo DIEM thi do dai keo chinh
    /// la khoang cach - do la ca cai hay cua kieu ngam nay.
    /// </summary>
    public Vector3 DiemNgam(int skill, Vector3 huong, float xa01)
    {
        float tam = TamNgam(skill);
        float xa = DonThang(skill) ? tam : Mathf.Lerp(2f, tam, Mathf.Clamp01(xa01));

        Vector3 diem = transform.position + huong * xa;
        diem.y = VfxFactory.GroundY(diem);
        return diem;
    }

    /// <summary>
    /// Tung ky nang theo huong nguoi choi vua ngam bang cach keo tren man hinh.
    /// </summary>
    public void DanhTheoHuong(int skill, Vector3 huong, float xa01)
    {
        SelectedSkill = skill;
        CastAt(skill, DiemNgam(skill, huong, xa01));
    }

    /// <summary>Huong nham tren may cam ung: theo can joystick, khong thi theo mat.</summary>
    Vector3 HuongNgamCamUng()
    {
        if (input.huongCan.sqrMagnitude > 0.0001f) return input.huongCan;
        Vector3 mat = transform.forward;
        mat.y = 0f;
        return mat.sqrMagnitude > 0.0001f ? mat.normalized : Vector3.forward;
    }

    /// <summary>
    /// Ke dich dang de nham nhat theo huong <paramref name="huong"/>.
    ///
    /// Cham diem theo CA hai thu: lech huong bao nhieu va o xa bao nhieu. Chi
    /// lay con gan nhat thi day can sang phai van ban vao con dang dung sau
    /// lung; con chi lay con dung huong nhat thi mot con o tan cuoi ban do cung
    /// duoc chon chi vi no thang hang.
    /// </summary>
    Transform TimKeDichGanNhat(Vector3 huong)
    {
        const float TamXa = 22f;

        var quanh = Physics.OverlapSphere(transform.position, TamXa, enemyMask,
                                          QueryTriggerInteraction.Ignore);
        Transform tot = null;
        float diemTot = float.MaxValue;

        foreach (var c in quanh)
        {
            var d = c.GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;

            Vector3 toi = d.transform.position - transform.position;
            toi.y = 0f;
            float xa = toi.magnitude;
            if (xa < 0.01f) continue;

            // 0 khi dung huong, 1 khi nguoc huong hoan toan
            float lech = (1f - Vector3.Dot(toi / xa, huong)) * 0.5f;

            // KHOANG CACH la chinh, huong chi de PHA HOA.
            //
            // Truoc day lech huong nang gap ba lan khoang cach. Nhung "cham roi
            // tha ngay" bay gio co nghia ro rang: danh con GAN NHAT - con muon
            // chon huong thi da co cach keo ra de ngam. De huong nang tay thi
            // dang chay ve phia truoc, bam nhanh mot cai, phep bay vao con o tan
            // dang xa trong khi co mot con dang cau ngay sau lung.
            float diem = xa / TamXa + lech * 0.25f;
            if (diem < diemTot) { diemTot = diem; tot = d.transform; }
        }
        return tot;
    }

    /// <summary>Tung ngay mot ky nang ve phia con tro chuot dang tro.</summary>
    public void CastAtCursor(int skill)
    {
        SelectedSkill = skill;

        Vector3 aim;
        if (!GetAimPoint(out aim))
        {
            // Khong ngam duoc (con tro chuot ra ngoai cua so game chang han) thi
            // VAN PHAI DANH, danh thang ve phia truoc mat. Truoc day cho no lang
            // le bo qua - nguoi choi bam phim thay khong co gi xay ra, tuong game
            // hong ma khong biet vi sao.
            aim = transform.position + transform.forward * 9f;
            aim.y = VfxFactory.GroundY(aim);
        }
        CastAt(skill, aim);
    }

    /// <summary>
    /// Tung ky nang thang ve phia truoc mat.
    ///
    /// Dung khi nguoi choi BAM VAO O KY NANG duoi man hinh: luc do con tro dang
    /// nam tren thanh ky nang chu khong o trong san, ngam theo con tro se thanh
    /// ban xuong sat chan minh.
    /// </summary>
    public void CastForward(int skill)
    {
        SelectedSkill = skill;

        Vector3 aim = transform.position + transform.forward * 9f;
        aim.y = VfxFactory.GroundY(aim);
        CastAt(skill, aim);
    }

    /// <summary>
    /// Tung ky nang vao mot diem cu the (dung cho phim tat va kiem thu).
    ///
    /// HOI CHIEU TINH TU LUC BAM, khong phai tu luc phep tan. Dong ho duoc dat
    /// ngay o day, TRUOC ca BeginCast - va no chay doc lap hoan toan voi vong
    /// doi cua thu ma phep sinh ra. Con loc song 6 giay hay 60 giay cung khong
    /// dinh gi toi luc ban duoc tung con tiep theo.
    ///
    /// ⚠️ Sua con so hoi chieu o day thi CHUA AN. Cac con so nay da duoc luu vao
    /// prefab Player_Sorceress va vao ca hai canh; gia tri luu do DE LEN gia tri
    /// mac dinh trong code. Da vap dung mot lan: doi tornadoCooldown 13 -> 2 trong
    /// code, chay game van thay 13 giay, tuong la loi cach tinh hoi chieu.
    /// Sua so thi phai sua ca ba cho.
    /// </summary>
    /// <summary>
    /// Tam tha xa nhat cua mot ky nang, tinh bang met. 0 = khong gioi han.
    ///
    /// Chi Mua bang va Sam set co tam. Qua cau lua thi khong: no BAY di, cham
    /// gi thi no o do - kep diem ngam cua no chi lam doi HUONG ban chu khong
    /// lam no bay ngan lai, tuc chang duoc gi ma con ban lech.
    /// </summary>
    float TamCuaKyNang(int skill)
    {
        if (skill == 1) return iceRange;
        if (skill == 2) return boltRange;
        return 0f;
    }

    /// <summary>
    /// Keo diem ngam ve trong tam. Ngam xa hon thi phep roi o MEP tam, dung
    /// huong da ngam.
    ///
    /// Keo chu khong TU CHOI danh: bam ky nang ma khong co gi xay ra thi nguoi
    /// choi tuong nut hong - dung cai bay da vap o CastAtCursor. Roi o mep tam
    /// thi it ra con thay phep bay ra va tu hieu la minh ngam qua xa.
    ///
    /// Do cao lay lai tu mat dat: keo ngang xong ma giu nguyen y cu thi phep
    /// treo lo lung tren suon doi hoac thut xuong duoi dat.
    /// </summary>
    Vector3 KepVaoTam(Vector3 aim, float tam)
    {
        if (tam <= 0f) return aim;

        Vector3 tu = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 den = new Vector3(aim.x, 0f, aim.z);
        Vector3 v = den - tu;

        if (v.sqrMagnitude <= tam * tam) return aim;

        Vector3 kq = tu + v.normalized * tam;
        kq.y = VfxFactory.GroundY(kq);
        return kq;
    }

    public void CastAt(int skill, Vector3 aim)
    {
        // Bam hut thi phai bao cho nguoi choi biet vi sao, khong duoc im lang.
        if (castTimer > 0f) { Say("Đang niệm chú, chờ một chút!"); return; }

        aim = KepVaoTam(aim, TamCuaKyNang(skill));

        if (skill == 0)
        {
            if (fireballTimer > 0f) { Say("QUẢ CẦU LỬA đang hồi chiêu"); return; }
            if (mana < fireballCost) { Say("Không đủ năng lượng!"); return; }

            mana -= fireballCost;
            fireballTimer = fireballCooldown;
            BeginCast(0, fireballCastTime, aim);
        }
        else if (skill == 1)
        {
            if (iceTimer > 0f) { Say("MƯA BĂNG đang hồi chiêu"); return; }
            if (mana < iceCost) { Say("Không đủ năng lượng!"); return; }

            mana -= iceCost;
            iceTimer = iceCooldown;
            BeginCast(1, iceCastTime, aim);
        }
        else if (skill == 2)
        {
            if (boltTimer > 0f) { Say("SẤM SÉT đang hồi chiêu"); return; }
            if (mana < boltCost) { Say("Không đủ năng lượng!"); return; }

            mana -= boltCost;
            boltTimer = boltCooldown;
            BeginCast(2, boltCastTime, aim);
        }
        else if (skill == 3)
        {
            if (tornadoTimer > 0f) { Say("LỐC XOÁY đang hồi chiêu"); return; }
            if (mana < tornadoCost) { Say("Không đủ năng lượng!"); return; }

            mana -= tornadoCost;
            tornadoTimer = tornadoCooldown;
            BeginCast(3, tornadoCastTime, aim);
        }
        else if (skill == 4)
        {
            if (meteorTimer > 0f) { Say("THIÊN THẠCH đang hồi chiêu"); return; }
            if (mana < meteorCost) { Say("Không đủ năng lượng!"); return; }

            mana -= meteorCost;
            meteorTimer = meteorCooldown;
            BeginCast(4, meteorCastTime, aim);
        }
        else if (skill == 5)
        {
            if (khiengTimer > 0f) { Say("KHIÊN đang hồi chiêu"); return; }
            if (mana < khiengCost) { Say("Không đủ năng lượng!"); return; }

            mana -= khiengCost;
            khiengTimer = khiengCooldown;
            BeginCast(5, khiengCastTime, aim);
        }
        else if (skill == 6)
        {
            if (giatSetTimer > 0f) { Say("GIỰT SÉT đang hồi chiêu"); return; }
            if (mana < giatSetCost) { Say("Không đủ năng lượng!"); return; }

            mana -= giatSetCost;
            giatSetTimer = giatSetCooldown;
            BeginCast(6, giatSetCastTime, aim);
        }
        else
        {
            // Phai kiem tra HAN so hieu ky nang, khong duoc de "else" tron.
            // De "else" tron thi MOI so la deu roi vao nhanh cuoi cung - tuc
            // bam nham gi cung bat khieng len, ma lai im lang nen khong ai biet
            // vi sao khieng tu nhien hien ra.
            Debug.LogWarning("[PlayerController] So hieu ky nang la: " + skill);
        }
    }

    /// <summary>
    /// BAO RA MOI LAN NHAN VAT NAY THUC SU TUNG DUOC MOT PHEP.
    ///
    /// Dat o BeginCast chu khong o CastAt: CastAt con co the tu choi vi thieu
    /// nang luong hoac dang hoi chieu, ma bao ra mot phep khong he bay ra thi
    /// may ben kia se ve mot dam lua khong co that.
    ///
    /// <see cref="DongBoTran"/> nghe su kien nay de gui sang may kia. Khong
    /// nghe thi khong sao - nhan vat van tung phep binh thuong, chi la mot
    /// minh minh thay.
    /// </summary>
    public event System.Action<int, Vector3> DaTungPhep;

    /// <summary>
    /// TUNG PHEP THEO LENH TU MAY KIA - khong hoi nang luong, khong hoi chieu.
    ///
    /// May ben kia da kiem du ca hai roi; kiem lai o day thi mana va hoi chieu
    /// cua ban sao nay (von khong ai tru bao gio) se tu choi phep, va nguoi
    /// choi ben nay khong bao gio thay doi phuong tung chieu.
    ///
    /// Van di qua BeginCast nen tu the niem chu, hieu ung don phep va sat
    /// thuong deu giong het ban that.
    /// </summary>
    public void TungPhepTheoMang(int skill, Vector3 aim)
    {
        TungPhepTheoMang(skill, aim, 0f);
    }

    /// <summary>
    /// Nhu tren, nhung biet ca nguoi tung dang nhin thay ta TRE bao nhieu.
    ///
    /// Con so ay duoc giu lai den luc phep thuc su bay ra (Release), roi dung
    /// de lui moi nguoi ve dung khoanh khac ho bam - xem <see cref="BuTre"/>.
    /// </summary>
    public void TungPhepTheoMang(int skill, Vector3 aim, float doTreGiay)
    {
        if (skill < 0 || skill > 6) return;
        buTreCuaPhepNay = doTreGiay;
        BeginCast(skill, ThoiGianNiem(skill), aim);
    }

    /// <summary>Do tre cua phep dang niem, neu no den tu mang. 0 = phep cua chinh may nay.</summary>
    float buTreCuaPhepNay;

    /// <summary>Thoi gian niem chu cua tung phep - de cho ca duong mang dung chung.</summary>
    float ThoiGianNiem(int skill)
    {
        switch (skill)
        {
            case 0: return fireballCastTime;
            case 1: return iceCastTime;
            case 2: return boltCastTime;
            case 3: return tornadoCastTime;
            case 4: return meteorCastTime;
            case 5: return khiengCastTime;
            default: return giatSetCastTime;
        }
    }

    void BeginCast(int skill, float castTime, Vector3 aim)
    {
        if (DaTungPhep != null) DaTungPhep(skill, aim);

        castingSkill = skill;
        castTotal = castTime;
        castTimer = castTime;
        castAim = aim;
        castReleased = false;
        hasMoveTarget = false;

        if (anim != null) anim.PlayCast(castTime * 1.35f);

        // Moi phep mot tu the rieng - xem NguoiChoiHoatHinh.TuTheNiemChu
        if (hoatHinhRieng != null) hoatHinhRieng.NiemChu(skill, castTime * 1.35f);

        Transform at = rig != null && rig.castPoint != null ? rig.castPoint : transform;

        // CastCharge nhan NGUYEN TO chu khong phai so thu tu ky nang: 0 = lua,
        // 1 = bang, con lai = set. Thien thach la phep LUA nen phai doi ve 0,
        // khong thi tay phu thuy toe ra tia set xanh roi troi rot xuong hon lua.
        // 4 = thien thach (lua), 5 = khieng (dung hieu ung bang cho ra ve nang luong)
        // 6 = giut set -> nguyen to SET (2), khong thi so 6 roi ra ngoai bang
        // va tay phu thuy khong toe ra gi ca.
        int nguyenTo = (skill == 4) ? 0 : (skill == 5) ? 1 : (skill == 6) ? 2 : skill;
        chargeVfx = VfxFactory.CastCharge(at, nguyenTo, castTime);
    }

    void HandleCasting(float dt)
    {
        if (castTimer <= 0f) return;

        castTimer -= dt;

        // Quay mat ve huong niem chu
        Vector3 look = new Vector3(castAim.x - transform.position.x, 0f, castAim.z - transform.position.z);
        if (look.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 18f * dt);

        // Den giua thi phep bay ra
        if (!castReleased && castTimer <= castTotal * 0.45f)
        {
            castReleased = true;

            // Phep den tu mang thi tinh trung theo cai nguoi ban NHIN THAY:
            // lui moi nguoi ve dung khoanh khac ho bam, tinh xong tra lai ngay
            // trong khung hinh nay. Phep cua chinh may nay thi buTreCuaPhepNay
            // la 0 va ca doan duoi khong lam gi ca.
            if (buTreCuaPhepNay > 0f)
            {
                BuTre.TuaTruocGiay = buTreCuaPhepNay;
                BuTre.Mo(buTreCuaPhepNay, health);
                try { Release(); }
                finally { BuTre.Dong(); BuTre.TuaTruocGiay = 0f; }
            }
            else Release();
        }
    }

    void Release()
    {
        Vector3 origin = rig != null && rig.castPoint != null
            ? rig.castPoint.position
            : transform.position + Vector3.up * 1.4f;

        if (castingSkill == 0)
        {
            Vector3 aimAt = castAim + Vector3.up * 0.9f;
            Vector3 dir = aimAt - origin;
            dir.y = Mathf.Clamp(dir.y, -1.5f, 1.5f);
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;

            // BA qua bay cung luc, toe hinh quat ve phia truoc.
            // Xem Fireball.SpawnChum - chum toe quanh truc DUNG nen nham chech
            // len hay xuong deu khong lam hai qua bien lech khoi mat phang ngang.
            Fireball.SpawnChum(origin, dir.normalized, obstacleMask, enemyMask, health);
            CameraShake.Shake(0.12f, 0.05f);
        }
        else if (castingSkill == 1)
        {
            var mua = IceStorm.Spawn(castAim, enemyMask);
            if (mua != null) mua.boQua = health;
        }
        else if (castingSkill == 2)
        {
            var bao = LightningStorm.Spawn(castAim, enemyMask);
            if (bao != null) bao.boQua = health;
        }
        else if (castingSkill == 3)
        {
            // Loc xoay sinh ra ngay truoc mat phu thuy roi truot ve phia ngam
            Vector3 dir = castAim - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();

            Vector3 spawnAt = transform.position + dir * 3f;
            spawnAt.y = VfxFactory.GroundY(spawnAt);

            var loc = Tornado.Spawn(spawnAt, dir, enemyMask);
            if (loc != null) loc.boQua = health;
        }
        else if (castingSkill == 4)
        {
            // THIEN THACH khong bay ra tu tay phu thuy - no roi tu tren troi
            // xuong dung cho ngam, nen khong dung "origin" o day.
            //
            // Goi CA LOAT ba qua noi duoi nhau, cach nhau 0,5 giay. Xem
            // ThienThach.SpawnLoat - hai qua sau lech ra chung quanh chu khong
            // roi trung mot cho.
            ThienThach.SpawnLoat(castAim, obstacleMask, enemyMask, health);
        }
        else if (castingSkill == 5)
        {
            // KHIENG bat len quanh CHINH NGUOI CHOI, khong lien quan gi toi cho ngam.
            //
            // Ban sao KHONG tu dung khieng khi phat lai phep nay: khieng cua ban
            // sao den tu goi trang thai (HieuUngQuaMang.ApKhieng). Dung o CA HAI
            // duong thi chung lech nhau vai chuc mili giay - mot goi trang thai
            // "chua co khieng" den sau la dap vo cai khieng vua dung, roi dung
            // lai: nhap nhay, kem mot lan vo gia.
            if (health == null || !health.mauDoMayKhacQuyet)
            {
                khiengHienTai = Khieng.Bat(gameObject, khiengMau, khiengBanKinh);
                if (health != null) health.khieng = khiengHienTai;
                CameraShake.Shake(0.10f, 0.04f);
            }
        }
        else if (castingSkill == 6)
        {
            // GIUT SET bay ra TU TAY phu thuy nhu Qua cau lua, khong roi tu troi
            // xuong nhu Sam set - no la mot mach dien chay tu nguoi choi sang
            // dam quai.
            Vector3 dir = castAim - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;

            var set = GiatSet.Phong(origin, dir.normalized, enemyMask);
            if (set != null) set.boQua = health;
        }
        else
        {
            // Xem ghi chu o CastAt: khong de "else" tron o day. castingSkill
            // khoi tao bang -1, ma -1 roi vao nhanh cuoi thi khieng tu bat len.
            Debug.LogWarning("[PlayerController] Release voi castingSkill la: " + castingSkill);
        }

        if (chargeVfx != null) Destroy(chargeVfx, 0.2f);
    }

    // ================================================================
    //  DI CHUYEN
    // ================================================================

    void HandleMovement(float dt)
    {
        // BAN SAO CUA NGUOI CHOI KHAC KHONG TU DI.
        //
        // Vi tri cua ho den tu goi tin va duoc dat thang moi khung hinh. De
        // doan nay chay nua thi no lam hai viec hai, ca hai deu im lang:
        //
        //   1. cc.Move keo ban sao theo trong luc va va cham cua may NAY, danh
        //      nhau voi vi tri vua dat - nhan vat giat va troi.
        //   2. input cua ban sao luon rong, nen dong cuoi ham nay dat nhip buoc
        //      ve 0 MOI KHUNG HINH, de len con so ma DongBoTran vua tinh. Ket
        //      qua dung nhu nguoi choi mo ta: ho TRUOT tren mat dat, chan khong
        //      nhuc nhich - "dang bay".
        //
        // Niem chu, hoi chieu va moi thu khac van chay binh thuong: chi rieng
        // phan tu di la khong.
        if (!tuDocInput) return;

        Vector3 wish = Vector3.zero;

        // TREN MAY CAM UNG, KHONG BAO GIO co diem den.
        //
        // Xoa ngay tu dau moi khung hinh, truoc khi doc bat cu thu gi. Day la
        // chot chan cuoi cung: du sau nay co cho nao khac dat moveTarget - AI
        // dan duong, mot nut trong menu, hay chinh doan duoi day sot lai - thi
        // no cung bi xoa ngay khung hinh sau, nhan vat khong the tu chay di.
        if (input.laCamUng) hasMoveTarget = false;

        // Huong di da duoc DocInput doi sang toa do the gioi va gop ca ban phim
        // lan can joystick - xem GoiInput de biet vi sao doi truc o ben do.
        if (input.huongDi.sqrMagnitude > 0.0001f)
        {
            wish = input.huongDi;
            hasMoveTarget = false;
        }

        // --- Doi di toi mot diem (chuot trai) ---
        if (input.muonDiToi)
        {
            moveTarget = input.diemToi;
            hasMoveTarget = true;
        }

        if (hasMoveTarget && wish.sqrMagnitude < 0.01f)
        {
            Vector3 to = moveTarget - transform.position;
            to.y = 0f;
            if (to.magnitude <= stopDistance) hasMoveTarget = false;
            else wish = to.normalized;
        }

        // Dang niem chu thi dung yen
        // Loi nuoc thi nang chan lai - xem LoiNuoc.HeSoToc
        float speed = moveSpeed * (loiNuoc != null ? loiNuoc.HeSoToc : 1f);
        if (castTimer > 0f) { wish = Vector3.zero; speed = 0f; }

        velocity.x = wish.x * speed;
        velocity.z = wish.z * speed;

        if (cc.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += -22f * dt;
        cc.Move(velocity * dt);

        if (anim != null)
            anim.SetMoveSpeed(new Vector2(velocity.x, velocity.z).magnitude / Mathf.Max(0.01f, moveSpeed));

        if (wish.sqrMagnitude > 0.01f && castTimer <= 0f)
        {
            Quaternion want = Quaternion.LookRotation(wish);
            transform.rotation = Quaternion.Slerp(transform.rotation, want, turnSpeed * dt);
        }
    }

    // ================================================================
    //  NGAM
    // ================================================================

    /// <summary>
    /// Ban tia tu camera qua con tro chuot xuong the gioi.
    ///
    /// CHI MAY CUA NGUOI CHOI GOI HAM NAY - <see cref="DocInput"/> goi no roi
    /// nhet ket qua vao <see cref="GoiInput.diemNgam"/>. May trong tai khong
    /// goi: no khong co camera cua nguoi kia, va con tro chuot cua no thi
    /// chang lien quan gi. Trong tai chi nhan diem da ngam roi kep lai trong
    /// tam cho phep.
    /// </summary>
    public bool GetAimPoint(out Vector3 point)
    {
        point = transform.position + transform.forward * 5f;
        var cam = Camera.main;
        if (cam == null) return false;

        // Con tro ra ngoai cua so game thi Input.mousePosition tra ve so vo ly
        // (hang chuc nghin diem anh), tia ngam ban ra tan dau dau, khong trung gi.
        // Gap truong hop do thi bao la khong ngam duoc, de ben goi danh ve phia truoc.
        Vector3 chuot = Input.mousePosition;
        if (chuot.x < 0f || chuot.y < 0f || chuot.x > Screen.width || chuot.y > Screen.height)
            return false;

        Ray ray = cam.ScreenPointToRay(chuot);

        // Uu tien ngam vao quai vat neu con tro dang o tren no
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 300f, enemyMask, QueryTriggerInteraction.Collide))
        {
            point = hit.collider.bounds.center;
            point.y = hit.collider.bounds.min.y;
            return true;
        }

        if (Physics.Raycast(ray, out hit, 300f, groundMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            return true;
        }

        // Khong trung gi thi cat voi mat phang ngang qua chan nhan vat
        Plane plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        float dist;
        if (plane.Raycast(ray, out dist))
        {
            point = ray.GetPoint(dist);
            return true;
        }
        return false;
    }

    void Say(string msg)
    {
        LastMessage = msg;
        LastMessageTime = Time.time;
    }

    // ================================================================
    //  CHUP VA DAT LAI TRANG THAI - de con chay lai duoc
    // ================================================================

    /// <summary>Chup lai trang thai hien gio.</summary>
    public TrangThaiNhanVat ChupTrangThai()
    {
        var t = new TrangThaiNhanVat();
        t.soThuTu       = input.soThuTu;
        t.viTri         = transform.position;
        t.vanToc        = velocity;
        t.huongMat      = transform.rotation;
        t.mana          = mana;
        t.hoiCauLua     = fireballTimer;
        t.hoiBang       = iceTimer;
        t.hoiSet        = boltTimer;
        t.hoiLoc        = tornadoTimer;
        t.hoiThienThach = meteorTimer;
        t.hoiKhieng     = khiengTimer;
        t.hoiGiatSet    = giatSetTimer;
        t.dangNiem      = castTimer;
        t.coDiemDen     = hasMoveTarget;
        t.diemDen       = moveTarget;
        return t;
    }

    /// <summary>
    /// Dat nhan vat ve mot trang thai da chup.
    ///
    /// PHAI TAT CharacterController TRUOC KHI DOI VI TRI. No giu mot ban sao
    /// vi tri o tang duoi; gan thang transform.position trong khi no dang bat
    /// thi khung hinh sau no keo nguoc nhan vat ve cho cu, va hieu chinh nhin
    /// nhu khong an gi.
    /// </summary>
    public void DatTrangThai(TrangThaiNhanVat t)
    {
        bool batLai = cc != null && cc.enabled;
        if (batLai) cc.enabled = false;

        transform.position = t.viTri;
        transform.rotation = t.huongMat;

        if (batLai) cc.enabled = true;

        velocity      = t.vanToc;
        mana          = t.mana;
        fireballTimer = t.hoiCauLua;
        iceTimer      = t.hoiBang;
        boltTimer     = t.hoiSet;
        tornadoTimer  = t.hoiLoc;
        meteorTimer   = t.hoiThienThach;
        khiengTimer   = t.hoiKhieng;
        giatSetTimer  = t.hoiGiatSet;
        castTimer     = t.dangNiem;
        hasMoveTarget = t.coDiemDen;
        moveTarget    = t.diemDen;
    }
}
