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

    [Header("Ky nang 10 - Qua cau bang (so hieu 9)")]
    // Nguoi dung 16/09/2026: "Skill delay 0.55 giay" = hoi chieu bang Qua cau lua.
    // Truong MOI nen prefab chua luu - gia tri o day co hieu luc (khong bi prefab de).
    public float quaCauBangCost = 12f;
    public float quaCauBangCooldown = 0.55f;
    public float quaCauBangCastTime = 0.38f;

    [Header("Ky nang 11 - Gio loc (so hieu 10)")]
    // Nguoi dung 16/09/2026: "Skill delay 0.4 giay" = hoi chieu; chon 20 nang luong, niem 0,38 s nhu Qua cau bang.
    public float gioLocCost = 20f;
    public float gioLocCooldown = 0.4f;
    public float gioLocCastTime = 0.38f;

    [Header("Lua dia nguc (ky nang 11) - 4 qua cau lua tu di muc tieu")]
    public float luaDiaNgucCost = 31f;      // 5 qua (nguoi dung 17/09/2026; 25 x 5/4)
    public float luaDiaNgucCooldown = 0.5f;
    public float luaDiaNgucCastTime = 0.38f;

    [Header("Tang hinh (ky nang 12)")]
    public float tangHinhCost = 30f;
    // Doc THANG hang (thuoc tinh, khong luu duoc): doi 30 -> 10 ma de truong public thi prefab trong bo nho giu 30 (bay May giong)
    public float tangHinhCooldown { get { return TangHinh.HoiChieu; } }   // 10 giay, dem tu luc HIEN HINH
    public float tangHinhCastTime = 0.38f;

    [Header("Qua cau dien (ky nang 13) - 10 luot x 5 tia")]
    public float cauDienCost = QuaCauDien.NangLuong;        // 55
    public float cauDienCooldown = QuaCauDien.HoiChieu;     // 5 giay
    public float cauDienCastTime = 0.62f;

    [Header("May giong (ky nang 21) - 20 tia set trong 5 giay, vung 6 m")]
    // May giong: doc THANG hang so (thuoc tinh, khong phai truong luu duoc). 26/09/2026 doi hoi chieu 7 -> 5,5 thi Play van ra
    // 7: ban prefab Unity giu trong bo nho nap luc truong moi co mac dinh 7 va giu qua moi lan bien dich (prefab tren dia khong
    // co truong nay) - ban build WebGL cung lay tu do. Cung ly do voi binh mau / mana (HoiChieuBinh la hang).
    public float mayGiongCost { get { return MayGiong.NangLuong; } }        // 50
    public float mayGiongCooldown { get { return MayGiong.HoiChieu; } }     // 5,5 giay
    public float mayGiongCastTime { get { return MayGiong.NiemChu; } }      // 0,5 giay

    [Header("Hoa loc xoay (ky nang 14) - doi Gio loc dang bay thanh Loc xoay")]
    public float hoaLocXoayCost = HoaLocXoay.NangLuong;         // 45
    public float hoaLocXoayCooldown = HoaLocXoay.HoiChieu;      // 0,5 giay
    public float hoaLocXoayCastTime = 0.38f;

    [Header("Toc bien (ky nang 15) - dich chuyen tuc thoi 15 m")]
    public float tocBienCost = TocBien.NangLuong;               // 40
    // Hoi chieu GIAM 0,25 giay moi cap (TocBien.HoiChieuTheoCap) - truong nay chi la cap 1
    public float tocBienCooldown = TocBien.HoiChieuGoc;         // 5 giay o cap 1
    // "Khong niem chu" (nguoi dung chon): 0,01 giay - vua du mot khung de goi tin bay sang may khac
    public float tocBienCastTime = 0.01f;

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
    public float QuaCauBangCooldown01 { get { return Mathf.Clamp01(quaCauBangTimer / quaCauBangCooldown); } }
    public float GioLocCooldown01 { get { return Mathf.Clamp01(gioLocTimer / gioLocCooldown); } }
    public float LuaDiaNgucCooldown01 { get { return Mathf.Clamp01(luaDiaNgucTimer / luaDiaNgucCooldown); } }
    public float TangHinhCooldown01 { get { return Mathf.Clamp01(tangHinhTimer / tangHinhCooldown); } }
    public float CauDienCooldown01 { get { return Mathf.Clamp01(cauDienTimer / cauDienCooldown); } }
    public float MayGiongCooldown01 { get { return Mathf.Clamp01(mayGiongTimer / mayGiongCooldown); } }
    public float HoaLocXoayCooldown01 { get { return Mathf.Clamp01(hoaLocXoayTimer / hoaLocXoayCooldown); } }
    public float TocBienCooldown01 { get { return Mathf.Clamp01(tocBienTimer / HoiChieuTocBien); } }

    /// <summary>Hoi chieu Toc bien o CAP HIEN TAI - giam 0,25 giay moi cap.</summary>
    public float HoiChieuTocBien { get { return TocBien.HoiChieuTheoCap(Mathf.Max(1, CapDo.CapCuaKyNang(CapDo.KyTocBien))); } }

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
            case CapDo.KyBinhMau: return Mathf.Max(0f, binhMauTimer);
            case CapDo.KyBinhMana: return Mathf.Max(0f, binhManaTimer);
            case CapDo.KyQuaCauBang: return Mathf.Max(0f, quaCauBangTimer);
            case CapDo.KyGioLoc: return Mathf.Max(0f, gioLocTimer);
            case CapDo.KyLuaDiaNguc: return Mathf.Max(0f, luaDiaNgucTimer);
            case CapDo.KyTangHinh: return Mathf.Max(0f, tangHinhTimer);
            case CapDo.KyCauDien: return Mathf.Max(0f, cauDienTimer);
            case CapDo.KyHoaLocXoay: return Mathf.Max(0f, hoaLocXoayTimer);
            case CapDo.KyTocBien: return Mathf.Max(0f, tocBienTimer);
            case CapDo.KyMayGiong: return Mathf.Max(0f, mayGiongTimer);
        }
        return 0f;
    }

    public float BinhMauCooldown01 { get { return Mathf.Clamp01(binhMauTimer / HoiChieuBinh); } }
    public float BinhManaCooldown01 { get { return Mathf.Clamp01(binhManaTimer / HoiChieuBinh); } }

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
    float castTimer, castTotal, meteorTimer, khiengTimer, giatSetTimer, quaCauBangTimer, gioLocTimer, luaDiaNgucTimer, tangHinhTimer, cauDienTimer, hoaLocXoayTimer, tocBienTimer, mayGiongTimer;

    // ---- Binh mau / binh mana (ky nang 7, 8 - them 13/09/2026) ----
    // HANG SO chu khong phai truong public: truong public se bi prefab va hai
    // canh luu de len (da vap voi tornadoCooldown), sua so trong code khong an.
    /// <summary>Mot binh mau hoi TOI DA bay nhieu mau - o CAP 1.</summary>
    public const float MauMoiBinh = 200f;
    /// <summary>Mot binh mana hoi TOI DA bay nhieu nang luong - o CAP 1.</summary>
    public const float ManaMoiBinh = 75f;
    /// <summary>Moi cap binh mau hoi them bay nhieu mau (nguoi dung 25/09/2026).</summary>
    public const float MauThemMoiCapBinh = 75f;
    /// <summary>Moi cap binh mana hoi them bay nhieu nang luong (nguoi dung 25/09/2026).</summary>
    public const float ManaThemMoiCapBinh = 45f;

    /// <summary>Binh mau cap <paramref name="cap"/> hoi toi da bao nhieu: 200 / 275 / 350.</summary>
    public static float MauBinhTheoCap(int cap) { return MauMoiBinh + MauThemMoiCapBinh * Mathf.Max(0, cap - 1); }
    /// <summary>Binh mana cap <paramref name="cap"/> hoi toi da bao nhieu: 75 / 120 / 165.</summary>
    public static float ManaBinhTheoCap(int cap) { return ManaMoiBinh + ManaThemMoiCapBinh * Mathf.Max(0, cap - 1); }

    /// <summary>
    /// TOC DO GOC - moveSpeed luc vao tran, truoc moi lan tang theo cap nhan vat. Ky nang bi dong Toc do di
    /// chuyen cong % CUA SO NAY (nguoi dung 25/09/2026: "tang 10% vao toc do di chuyen goc cua nhan vat").
    /// </summary>
    public float TocGoc { get; private set; }
    /// <summary>Uong xong mot binh phai cho bay nhieu giay moi uong tiep duoc.</summary>
    public const float HoiChieuBinh = 0.5f;
    float binhMauTimer, binhManaTimer;
    Khieng khiengHienTai;
    int castingSkill = -1;
    Vector3 castAim;
    bool castReleased;
    GameObject chargeVfx;

    void Awake()
    {
        TocGoc = moveSpeed;
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

    // ================================================================
    //  LEN CAP
    // ================================================================

    void OnEnable() { CapDo.KhiLenCap += LenCap; }
    void OnDisable() { CapDo.KhiLenCap -= LenCap; }

    /// <summary>
    /// Vua len mot cap: mau +15%, nang luong +10%, toc do +3,5% (toc do CHI toi
    /// cap CapDo.CapTangTocToiDa - qua cap 10 thi giu nguyen).
    ///
    /// Mau va nang luong ĐANG CO cung duoc cong dung phan chenh - khong hoi day
    /// (len cap thanh mot binh mau mien phi) ma cung khong de nguoi choi tut
    /// ti le: dang 50% mau ma chi keo tran len thi tu nhien con 43%.
    ///
    /// CHI NHAN VAT CUA MAY NAY: ban sao cua nguoi khac lay mau tu goi tin theo
    /// TI LE, va cap cua ho may nay khong theo doi.
    /// </summary>
    void LenCap(int capMoi)
    {
        if (health != null && health.mauDoMayKhacQuyet) return;
        if (!tuDocInput) return;

        if (health != null)
        {
            float them = health.maxHealth * 0.15f;
            health.maxHealth += them;
            health.health = Mathf.Min(health.maxHealth, health.health + them);
        }

        float themMana = maxMana * 0.10f;
        maxMana += themMana;
        mana = Mathf.Min(maxMana, mana + themMana);

        if (capMoi <= CapDo.CapTangTocToiDa) moveSpeed *= 1.035f;

        Say("LÊN CẤP " + capMoi + "! Bạn có 1 điểm kỹ năng — mở SÁCH PHÉP");
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
        if (quaCauBangTimer > 0f) quaCauBangTimer -= dt;
        if (gioLocTimer > 0f) gioLocTimer -= dt;
        if (luaDiaNgucTimer > 0f) luaDiaNgucTimer -= dt;
        // Tang hinh: dang tang hinh thi GIU DAY hoi chieu (khong bam lai duoc), hien hinh xong moi dem 10 giay (26/09/2026)
        if (TangHinh.Dang(this)) tangHinhTimer = tangHinhCooldown;
        else if (tangHinhTimer > 0f) tangHinhTimer -= dt;
        if (cauDienTimer > 0f) cauDienTimer -= dt;
        if (mayGiongTimer > 0f) mayGiongTimer -= dt;
        if (hoaLocXoayTimer > 0f) hoaLocXoayTimer -= dt;
        if (tocBienTimer > 0f) tocBienTimer -= dt;
        if (binhMauTimer > 0f) binhMauTimer -= dt;
        if (binhManaTimer > 0f) binhManaTimer -= dt;

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
        return skill == 0 || skill == 3 || skill == 6 || skill == CapDo.KyQuaCauBang || skill == CapDo.KyGioLoc
            || skill == CapDo.KyLuaDiaNguc;
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
            case CapDo.KyQuaCauBang: return 14f;   // qua cau bang: bay nhu qua cau lua
            case CapDo.KyGioLoc: return 14f;       // gio loc: bay 17 m/s, xuyen vat can
            case CapDo.KyLuaDiaNguc: return LuaDiaNguc.TamTim;   // tu di ke dich trong 20 m
            case 1: return iceRange;         // 12
            case 2: return boltRange;        // 12
            case 3: return 12f;              // loc xoay truot ve phia ngam
            case 4: return 18f;              // thien thach roi tu tren troi
            case CapDo.KyCauDien: return QuaCauDien.Tam;   // 18 m - nguoi dung chon "bang tam Thien thach"
            case CapDo.KyTocBien: return TocBien.Tam;      // 15 m
            case CapDo.KyMayGiong: return boltRange;       // bang tam Sam set (nguoi dung 25/09/2026)
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
            case CapDo.KyQuaCauBang: return QuaCauBang.BanKinhNo;
            case CapDo.KyGioLoc: return GioLoc.BanKinhTrung;
            case CapDo.KyLuaDiaNguc:
            {
                var pf = kho != null ? kho.fireballPrefab : null;
                var c = pf != null ? pf.GetComponent<Fireball>() : null;
                return c != null ? c.blastRadius : 3.4f;
            }
            case 5:  return khiengBanKinh;
            case 6:  return 2.2f;             // tam lan cua tia set
            case CapDo.KyMayGiong: return MayGiong.BanKinh;   // vung may 6 m
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
    float TamGiatSet() { return GiatSet.TamNguoiChoi; }

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
        // Qua cau dien: nguoi dung chon tam BANG THIEN THACH (18 m)
        if (skill == CapDo.KyCauDien) return QuaCauDien.Tam;
        // Toc bien: nhay xa nhat 15 m
        if (skill == CapDo.KyTocBien) return TocBien.Tam;
        // May giong: tam BANG SAM SET (nguoi dung 25/09/2026)
        if (skill == CapDo.KyMayGiong) return boltRange;
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

    // ================================================================
    //  BI DONG BANG / BI CHOANG
    // ================================================================
    //
    // TRUOC DAY NGUOI CHOI MIEN NHIEM CA HAI. Quai thi da biet dung im tu lau
    // (EnemyAI doc FrozenEffect va StunnedEffect), con PlayerController KHONG
    // he doc hai thu do - nen nguoi choi bi phu kin trong vo bang van chay va
    // van tung phep binh thuong. Nguoi dung bao dung hien tuong nay (12/09/2026):
    // "danh vao nguoi choi khac chi hien len hieu ung hinh anh".
    //
    // Chi ap cho NHAN VAT CUA MAY NAY. Ban sao cua nguoi khac khong tu di
    // (HandleMovement thoat som) va khong tu tung phep (di duong TungPhepTheoMang),
    // vi trang thai that cua ho do MAY HO quyet - xem HieuUngQuaMang.

    /// <summary>He so toc do con lai do lop bang, 1 = khong bi gi.</summary>
    public float HeSoTocBang
    {
        get
        {
            var f = GetComponent<FrozenEffect>();
            return f != null ? f.HeSoToc : 1f;
        }
    }

    /// <summary>Dang bi dong cung hoan toan hoac bi choang - khong di, khong tung phep.</summary>
    public bool DangBiKhoaCung
    {
        get
        {
            var f = GetComponent<FrozenEffect>();
            if (f != null && f.IsFullyFrozen) return true;
            var ng = GetComponent<BiDanhNga>();
            if (ng != null && ng.DangNga) return true;
            var ht = GetComponent<BiHatTung>();
            if (ht != null && ht.DangBay) return true;
            var st = GetComponent<StunnedEffect>();
            return st != null && st.IsStunned;
        }
    }

    /// <summary>Ly do dang khong tung duoc phep, null neu tung duoc.</summary>
    string LyDoKhongTungDuoc()
    {
        var f = GetComponent<FrozenEffect>();
        if (f != null && f.IsFullyFrozen) return "BẠN ĐANG BỊ ĐÓNG BĂNG!";
        var ng = GetComponent<BiDanhNga>();
        if (ng != null && ng.DangNga) return "BẠN ĐANG BỊ HẤT NGÃ!";
        var ht = GetComponent<BiHatTung>();
        if (ht != null && ht.DangBay) return "BẠN ĐANG BỊ HẤT TUNG!";
        var st = GetComponent<StunnedEffect>();
        if (st != null && st.IsStunned) return "BẠN ĐANG BỊ CHOÁNG!";
        return null;
    }

    public void CastAt(int skill, Vector3 aim)
    {
        // Binh mau / binh mana KHONG niem chu, khong ngam, khong di qua mang: uong
        // ngay ca khi dang niem mot phep khac (dang niem ma sap chet thi phai uong
        // duoc). Mau cua minh do chinh may nay quyet, goi trang thai tu mang sang.
        if (CapDo.LaKyBinh(skill)) { UongBinh(skill); return; }

        // KY NANG BI DONG (Khang Lua/Bang/Set/Phong, 19/09/2026): khong tung duoc gi ca - mo khoa la
        // no tu cong vinh vien vao thuoc tinh. Chan o day de khong ai tru mana / vao hoi chieu oan.
        if (CapDo.LaKyBiDong(skill)) return;

        // Bam hut thi phai bao cho nguoi choi biet vi sao, khong duoc im lang.
        if (castTimer > 0f) { Say("Đang niệm chú, chờ một chút!"); return; }

        // Bi dong cung trong tang bang hay bi set danh choang thi KHONG TUNG
        // DUOC PHEP NAO. Chan o day chu khong ngat phep dang niem do:
        // goi "toi vua tung phep" da gui di tu luc BeginCast, ngat giua chung
        // thi may ben kia van ve ra qua phep va van tinh sat thuong cua no.
        // TOC BIEN CAP 5 la ngoai le duy nhat: nguoi dung chot 18/09/2026 - dung duoc NGAY khi dang
        // choang / nga / dong bang / hat tung (va nhay xong thi sach het). Rieng dang bi LOC XOAY cuon
        // thi van chiu - luc ay nguoi choi dang bay vong quanh truc loc.
        bool goTroiDuoc = skill == CapDo.KyTocBien && TocBien.CapNamGoTroiDuoc(this);
        string caidangkhoa = goTroiDuoc ? null : LyDoKhongTungDuoc();
        if (skill == CapDo.KyTocBien && TocBien.DangBiCuonLoc(this))
        { Say("BẠN ĐANG BỊ LỐC XOÁY CUỐN!"); return; }
        if (caidangkhoa != null) { Say(caidangkhoa); return; }

        // KY NANG CHUA MO THI KHONG TUNG DUOC. Vao tran ai cung cap 1 va moi
        // ky nang deu khoa; mo bang diem ky nang trong Sach phep.
        if (!CapDo.DaMo(skill))
        {
            Say(SachPhep.Ten(skill) + " chưa mở khoá — vào SÁCH PHÉP để mở");
            return;
        }

        // Ky nang cang cao cap cang ton nang luong (+10% moi cap)
        float tonThem = CapDo.ManaTheoCap(CapDo.CapCuaKyNang(skill));
        capPhepDangTung = CapDo.CapCuaKyNang(skill);

        aim = KepVaoTam(aim, TamCuaKyNang(skill));

        if (skill == 0)
        {
            if (fireballTimer > 0f) { Say("QUẢ CẦU LỬA đang hồi chiêu"); return; }
            if (mana < fireballCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= fireballCost * tonThem;
            fireballTimer = fireballCooldown;
            BeginCast(0, fireballCastTime, aim);
        }
        else if (skill == 1)
        {
            if (iceTimer > 0f) { Say("MƯA BĂNG đang hồi chiêu"); return; }
            if (mana < iceCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= iceCost * tonThem;
            iceTimer = iceCooldown;
            BeginCast(1, iceCastTime, aim);
        }
        else if (skill == 2)
        {
            if (boltTimer > 0f) { Say("SẤM SÉT đang hồi chiêu"); return; }
            if (mana < boltCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= boltCost * tonThem;
            boltTimer = boltCooldown;
            BeginCast(2, boltCastTime, aim);
        }
        else if (skill == 3)
        {
            if (tornadoTimer > 0f) { Say("LỐC XOÁY đang hồi chiêu"); return; }
            if (mana < tornadoCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= tornadoCost * tonThem;
            tornadoTimer = tornadoCooldown;
            BeginCast(3, tornadoCastTime, aim);
        }
        else if (skill == 4)
        {
            if (meteorTimer > 0f) { Say("THIÊN THẠCH đang hồi chiêu"); return; }
            if (mana < meteorCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= meteorCost * tonThem;
            meteorTimer = meteorCooldown;
            BeginCast(4, meteorCastTime, aim);
        }
        else if (skill == 5)
        {
            if (khiengTimer > 0f) { Say("KHIÊN đang hồi chiêu"); return; }
            if (mana < khiengCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= khiengCost * tonThem;
            khiengTimer = khiengCooldown;
            BeginCast(5, khiengCastTime, aim);
        }
        else if (skill == 6)
        {
            if (giatSetTimer > 0f) { Say("GIỰT SÉT đang hồi chiêu"); return; }
            if (mana < giatSetCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= giatSetCost * tonThem;
            giatSetTimer = giatSetCooldown;
            BeginCast(6, giatSetCastTime, aim);
        }
        else if (skill == CapDo.KyQuaCauBang)
        {
            if (quaCauBangTimer > 0f) { Say("QUẢ CẦU BĂNG đang hồi chiêu"); return; }
            if (mana < quaCauBangCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= quaCauBangCost * tonThem;
            quaCauBangTimer = quaCauBangCooldown;
            BeginCast(CapDo.KyQuaCauBang, quaCauBangCastTime, aim);
        }
        else if (skill == CapDo.KyGioLoc)
        {
            // Cap duoi 5: ton gap doi theo so loc; CAP 5 ton dung 25 (nguoi dung 18/09/2026, truoc do 58,6)
            float tonGioLoc = GioLoc.NangLuongCan(CapDo.CapCuaKyNang(skill), gioLocCost, tonThem);
            if (gioLocTimer > 0f) { Say("GIÓ LỐC đang hồi chiêu"); return; }
            if (mana < tonGioLoc) { Say("Không đủ năng lượng!"); return; }

            mana -= tonGioLoc;
            gioLocTimer = gioLocCooldown;
            BeginCast(CapDo.KyGioLoc, gioLocCastTime, aim);
        }
        else if (skill == CapDo.KyTangHinh)
        {
            if (tangHinhTimer > 0f) { Say("TÀNG HÌNH đang hồi chiêu"); return; }
            if (mana < tangHinhCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= tangHinhCost * tonThem;
            tangHinhTimer = tangHinhCooldown;
            BeginCast(CapDo.KyTangHinh, tangHinhCastTime, aim);
        }
        else if (skill == CapDo.KyTocBien)
        {
            if (tocBienTimer > 0f) { Say("TỐC BIẾN đang hồi chiêu"); return; }
            if (mana < tocBienCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            // Tim cho den TRUOC khi tru mana: ngam vao cho khong the dung thi khong mat gi
            Vector3 choDen;
            if (!TocBien.TimChoDen(transform.position, aim, obstacleMask, out choDen))
            { Say("Không dịch chuyển tới đó được"); return; }

            mana -= tocBienCost * tonThem;
            tocBienTimer = HoiChieuTocBien;
            choTocBien = choDen;
            BeginCast(CapDo.KyTocBien, tocBienCastTime, aim);
        }
        else if (skill == CapDo.KyHoaLocXoay)
        {
            if (hoaLocXoayTimer > 0f) { Say("HOÁ LỐC XOÁY đang hồi chiêu"); return; }
            // Khong co con Gio loc nao dang bay thi TU CHOI - khong tru mana, khong vao hoi chieu
            // (nguoi dung chon 18/09/2026).
            if (!HoaLocXoay.CoLocDeHoa(health)) { Say("Không có GIÓ LỐC nào đang bay!"); return; }
            if (mana < hoaLocXoayCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= hoaLocXoayCost * tonThem;
            hoaLocXoayTimer = hoaLocXoayCooldown;
            BeginCast(CapDo.KyHoaLocXoay, hoaLocXoayCastTime, aim);
        }
        else if (skill == CapDo.KyMayGiong)
        {
            if (mayGiongTimer > 0f) { Say("MÂY GIÔNG đang hồi chiêu"); return; }
            if (mana < mayGiongCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= mayGiongCost * tonThem;
            mayGiongTimer = mayGiongCooldown;
            BeginCast(CapDo.KyMayGiong, mayGiongCastTime, aim);
        }
        else if (skill == CapDo.KyCauDien)
        {
            if (cauDienTimer > 0f) { Say("QUẢ CẦU ĐIỆN đang hồi chiêu"); return; }
            if (mana < cauDienCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= cauDienCost * tonThem;
            cauDienTimer = cauDienCooldown;
            BeginCast(CapDo.KyCauDien, cauDienCastTime, aim);
        }
        else if (skill == CapDo.KyLuaDiaNguc)
        {
            if (luaDiaNgucTimer > 0f) { Say("LỬA ĐỊA NGỤC đang hồi chiêu"); return; }
            if (mana < luaDiaNgucCost * tonThem) { Say("Không đủ năng lượng!"); return; }

            mana -= luaDiaNgucCost * tonThem;
            luaDiaNgucTimer = luaDiaNgucCooldown;
            BeginCast(CapDo.KyLuaDiaNguc, luaDiaNgucCastTime, aim);
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
    /// UONG MOT BINH MAU / BINH MANA.
    ///
    /// Nguoi dung (13/09/2026): mo khoa bang 1 diem ky nang, chi dung duoc khi da
    /// NHAT duoc binh (roi 10% moi loai khi giet quai), mot binh hoi TOI DA 100 mau
    /// / 50 nang luong, cho 0,5 giay moi uong tiep.
    ///
    /// Day hoac het binh thi KHONG uong va noi ro vi sao - uong mat mot binh ma
    /// khong hoi duoc gi la nguoi choi mat trang ma khong biet.
    /// Bi dong cung / choang / nga thi cung khong uong duoc (luat chung: khong
    /// dung ky nang nao).
    /// </summary>
    /// <returns>Luong da hoi that (0 neu khong uong).</returns>
    public float UongBinh(int ky)
    {
        string ten = SachPhep.Ten(ky);
        string caidangkhoa = LyDoKhongTungDuoc();
        if (caidangkhoa != null) { Say(caidangkhoa); return 0f; }
        if (!CapDo.DaMo(ky)) { Say(ten + " chưa mở khoá — vào SÁCH PHÉP để mở"); return 0f; }
        if (CapDo.SoBinh(ky) <= 0)
        {
            Say("Hết " + (ky == CapDo.KyBinhMau ? "bình máu" : "bình mana") + " — giết quái để nhặt thêm");
            return 0f;
        }

        bool laMau = ky == CapDo.KyBinhMau;
        if ((laMau ? binhMauTimer : binhManaTimer) > 0f) { Say(ten + " đang hồi chiêu"); return 0f; }

        float thieu = laMau
            ? (health != null ? health.maxHealth - health.health : 0f)
            : maxMana - mana;
        if (laMau && health != null && health.IsDead) return 0f;
        if (thieu <= 0.5f) { Say(laMau ? "Máu đang đầy" : "Năng lượng đang đầy"); return 0f; }

        int capBinh = CapDo.CapCuaKyNang(ky);
        float hoi = Mathf.Min(laMau ? MauBinhTheoCap(capBinh) : ManaBinhTheoCap(capBinh), thieu);
        if (!CapDo.BotBinh(ky)) return 0f;
        if (laMau) { health.Heal(hoi); binhMauTimer = HoiChieuBinh; }
        else { mana = Mathf.Min(maxMana, mana + hoi); binhManaTimer = HoiChieuBinh; }

        DamagePopup.SpawnText(transform.position + Vector3.up * 2.3f, "+" + Mathf.RoundToInt(hoi),
                              laMau ? new Color(1f, 0.30f, 0.25f) : new Color(0.45f, 0.72f, 1f));
        return hoi;
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
    public event System.Action<int, Vector3, bool> DaTungPhep;

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
        TungPhepTheoMang(skill, aim, 0f, 1);
    }

    public void TungPhepTheoMang(int skill, Vector3 aim, float doTreGiay)
    {
        TungPhepTheoMang(skill, aim, doTreGiay, 1);
    }

    /// <summary>
    /// Nhu tren, nhung biet ca nguoi tung dang nhin thay ta TRE bao nhieu.
    ///
    /// Con so ay duoc giu lai den luc phep thuc su bay ra (Release), roi dung
    /// de lui moi nguoi ve dung khoanh khac ho bam - xem <see cref="BuTre"/>.
    /// </summary>
    public void TungPhepTheoMang(int skill, Vector3 aim, float doTreGiay, int capKyNang)
    {
        TungPhepTheoMang(skill, aim, doTreGiay, capKyNang, false);
    }

    /// <summary>Nhu tren, kem co "day la don dau tien cua Tang hinh" doc tu goi tin (sat thuong x2 tren MOI may).</summary>
    public void TungPhepTheoMang(int skill, Vector3 aim, float doTreGiay, int capKyNang, bool donTangHinh)
    {
        // 0..6, Qua cau bang (9), Gio loc (10). Binh (7, 8) khong bao gio di qua goi tung phep.
        if ((skill < 0 || skill > 6) && skill != CapDo.KyQuaCauBang && skill != CapDo.KyGioLoc && skill != CapDo.KyLuaDiaNguc
            && skill != CapDo.KyTangHinh && skill != CapDo.KyCauDien && skill != CapDo.KyHoaLocXoay
            && skill != CapDo.KyTocBien && skill != CapDo.KyMayGiong) return;
        buTreCuaPhepNay = doTreGiay;

        // CAP CUA NGUOI TUNG, khong phai cap cua nguoi xem: goi tin mang theo
        // con so nay. Lay cap cua minh o day thi don cua nguoi cap 5 danh vao
        // may nay lai yeu di theo cap cua minh, va hai may thay hai con so sat
        // thuong khac nhau.
        capPhepDangTung = Mathf.Clamp(capKyNang, 1, CapDo.CapKyNangToiDa);
        phepNayTuMang = true;
        donTangHinhTuMang = donTangHinh;
        BeginCast(skill, ThoiGianNiem(skill), aim);
    }

    /// <summary>Do tre cua phep dang niem, neu no den tu mang. 0 = phep cua chinh may nay.</summary>
    float buTreCuaPhepNay;

    /// <summary>Cho Toc bien se nhay toi - tinh o CastAt (truoc khi tru mana) va dung lai o Release.</summary>
    Vector3 choTocBien;

    /// <summary>
    /// Phep dang niem co phai DON DAU TIEN trong trang thai Tang hinh khong - TOAN BO sat thuong cua no x2
    /// (moi qua cau bang, moi vet mua bang... - nguoi dung chot 18/09/2026).
    /// Quyet dinh o BeginCast chu khong o Release, vi goi tin bao sang may khac ngay luc bat dau niem.
    /// </summary>
    public bool donTangHinhDangTung;

    /// <summary>Phep sap niem den tu mang (co "don dau" doc tu goi tin, khong tu tinh lai).</summary>
    bool phepNayTuMang, donTangHinhTuMang;

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
            case CapDo.KyQuaCauBang: return quaCauBangCastTime;
            case CapDo.KyGioLoc: return gioLocCastTime;
            case CapDo.KyLuaDiaNguc: return luaDiaNgucCastTime;
            case CapDo.KyTangHinh: return tangHinhCastTime;
            case CapDo.KyCauDien: return cauDienCastTime;
            case CapDo.KyHoaLocXoay: return hoaLocXoayCastTime;
            case CapDo.KyTocBien: return tocBienCastTime;
            case CapDo.KyMayGiong: return mayGiongCastTime;
            default: return giatSetCastTime;
        }
    }

    void BeginCast(int skill, float castTime, Vector3 aim)
    {
        // TANG HINH - DON DAU TIEN (18/09/2026): quyet dinh NGAY LUC BAT DAU NIEM, khong phai luc Release,
        // vi goi tin bay sang may khac tu day. Quyet dinh o Release thi ben kia phat lai phep voi sat thuong
        // thuong, con may minh tinh gap doi - hai may ke hai con so khac nhau.
        if (phepNayTuMang)
        {
            donTangHinhDangTung = donTangHinhTuMang;
            phepNayTuMang = false;
        }
        else
        {
            donTangHinhDangTung = false;
            var tgNiem = GetComponent<TangHinh>();
            if (tgNiem != null && tgNiem.conLai > 0f && tgNiem.conDonDau && KyGaySatThuong(skill))
            {
                donTangHinhDangTung = true;
                tgNiem.conDonDau = false;
            }
        }

        if (DaTungPhep != null) DaTungPhep(skill, aim, donTangHinhDangTung);

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
        // 9 = qua cau bang -> nguyen to BANG (1); 10 = gio loc -> nhu Loc xoay (3, roi vao nhanh set)
        int nguyenTo = (skill == 4) ? 0 : (skill == 5) ? 1 : (skill == 6) ? 2
                     : (skill == CapDo.KyQuaCauBang) ? 1 : (skill == CapDo.KyGioLoc) ? 3
                     : (skill == CapDo.KyLuaDiaNguc) ? 0 : (skill == CapDo.KyTangHinh) ? 1
                     : (skill == CapDo.KyCauDien) ? 2 : (skill == CapDo.KyHoaLocXoay) ? 3
                     : (skill == CapDo.KyTocBien) ? 1 : (skill == CapDo.KyMayGiong) ? 2 : skill;
        chargeVfx = VfxFactory.CastCharge(at, nguyenTo, castTime);
    }

    /// <summary>Ky nang nay co GAY SAT THUONG khong (Khien, hai binh va Tang hinh thi khong).</summary>
    public static bool KyGaySatThuong(int skill)
    {
        return skill != 5 && skill != CapDo.KyBinhMau && skill != CapDo.KyBinhMana && skill != CapDo.KyTangHinh;
    }

    /// <summary>So lan bi ngat chieu that (dang niem, chua phong) - phep thu doc.</summary>
    public static int SoLanNgatChieu;

    /// <summary>
    /// NGAT CHIEU DANG NIEM (bi Gio loc hat tung - nguoi dung 16/09/2026: "dang dung skill ma bi hat tung se
    /// bi ngat chieu do ngay lap tuc"). Phep CHUA phong ra thi khong bao gio phong; da phong roi thi thoi.
    ///
    /// Qua mang: may chu so huu ngat truoc khi toi luc phong cua minh. Ban sao tren may khac bat dau niem tre
    /// dung bang do tre goi tin, va co CoHatTung di qua cung duong ay - nen ban sao nhan co TRUOC luc no phong
    /// (HieuUngQuaMang.ApCo -> BiHatTung.ApTuMang -> day). Nang luong / hoi chieu da tru thi khong tra lai.
    /// </summary>
    public void NgatChieu()
    {
        if (castTimer <= 0f || castReleased) return;
        castTimer = 0f;
        castReleased = true;
        SoLanNgatChieu++;

        // Chieu bi ngat thi don chua bay ra: tra lai quyen "don dau x2" cho Tang hinh.
        if (donTangHinhDangTung)
        {
            donTangHinhDangTung = false;
            var tgNgat = GetComponent<TangHinh>();
            if (tgNgat != null) tgNgat.conDonDau = true;
        }
        if (chargeVfx != null) { Destroy(chargeVfx); chargeVfx = null; }
        if (anim != null) anim.DungNiem();
        if (hoatHinhRieng != null) hoatHinhRieng.HuyNiem();
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

    /// <summary>
    /// Cap cua ky nang dang tung. Phep cua CHINH may nay thi doc tu CapDo; phep
    /// den tu mang thi lay cap DI KEM GOI - nguoi tung cap 5 phai manh dung nhu
    /// cap 5 tren moi man hinh, khong phai manh theo cap cua nguoi xem.
    /// </summary>
    int capPhepDangTung = 1;

    void Release()
    {
        int capPhep = Mathf.Max(1, capPhepDangTung);
        float manhHon = CapDo.SatThuongTheoCap(capPhep);
        float themGiay = CapDo.ThemGiayHieuUngTheoCap(capPhep);

        // TANG HINH (18/09/2026): don dau tien bang mot ky nang GAY SAT THUONG an gap doi va lam tan tang hinh ngay.
        // Khien / binh / chinh Tang hinh khong tinh (nguoi dung chon).
        //
        // "manhHon" la he so sat thuong cua CA ky nang: no di vao tung qua cau bang, tung vet mua bang, tung
        // nhip chay cua lua... nen nhan o day la TOAN BO sat thuong cua ky nang ay x2 (nguoi dung chot 18/09/2026),
        // khong phai chi mot cu trung dau tien.
        if (donTangHinhDangTung)
        {
            manhHon *= TangHinh.NhanDonDau;
            donTangHinhDangTung = false;
            TangHinh.SoLanDonDau++;
            var tangHinh = GetComponent<TangHinh>();
            if (tangHinh != null) tangHinh.Tat();
        }

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
            // CAP 5 danh nga 30% trong 1,5 giay (nguoi dung 19/09/2026) - MOI QUA trong chum gieo rieng,
            // dung nhu loat ba qua Thien thach van lam.
            // Nguoi dung 25/09/2026: bay XUYEN vat nho (bia mo, da) nhu Lua dia nguc - nha, cay, lo lua van chan
            Fireball.SpawnChum(origin, dir.normalized, obstacleMask, enemyMask, health,
                               3, 11f, manhHon, themGiay,
                               capPhep >= Fireball.CapDanhNga ? Fireball.NgaXacSuatCap5 : 0f,
                               ThienThach.NgaGiayNguoiChoi, true, 1);      // soLanNay 1: vu no trung ke dich thi nay sang ke ke ben (26/09/2026)
            CameraShake.Shake(0.12f, 0.05f);
        }
        else if (castingSkill == CapDo.KyTangHinh)
        {
            var tgMoi = TangHinh.Bat(health, TangHinh.ThoiGian);
            // Cap cua NGUOI TUNG (ban sao: tu goi phep) - quyet dinh % mau cua vong phep luc het tang hinh
            if (tgMoi != null) tgMoi.capKyNang = capPhepDangTung;
        }
        else if (castingSkill == CapDo.KyTocBien)
        {
            // BAN SAO cua nguoi choi khac chi chay HIEU UNG: vi tri cua ho do goi trang thai quyet dinh,
            // tu keo o day thi khung sau goi tin lai kco ve - nhan vat giat qua giat lai.
            if (tuDocInput) TocBien.Nhay(this, choTocBien);
            else
            {
                VfxFactory.TocBienBienMat(transform.position);
                VfxFactory.TocBienHienRa(castAim);
            }
        }
        else if (castingSkill == CapDo.KyHoaLocXoay)
        {
            // Doi con Gio loc cua LAN TUNG GAN NHAT thanh Loc xoay. Sat thuong tong = cap hien tai
            // cua Gio loc (don cham) + cap hien tai cua Loc xoay (moi giay + tia set) - nguoi dung chot.
            float capGioLoc = CapDo.SatThuongTheoCap(Mathf.Max(1, CapDo.CapCuaKyNang(CapDo.KyGioLoc)));
            float capLocXoay = CapDo.SatThuongTheoCap(Mathf.Max(1, CapDo.CapCuaKyNang(3)));
            HoaLocXoay.Hoa(health, enemyMask, capGioLoc, capLocXoay);
        }
        else if (castingSkill == CapDo.KyMayGiong)
        {
            // Vung may giong ngay cho ngam (da kep trong tam Sam set) - 20 tia trong 5 giay
            MayGiong.Spawn(castAim, enemyMask, health, manhHon, themGiay);
        }
        else if (castingSkill == CapDo.KyCauDien)
        {
            // Qua cau dien hien ngay canh ke dich gan cho ngam nhat roi ban 10 luot x 5 tia (moi ke mot tia).
            QuaCauDien.Spawn(castAim, enemyMask, health, manhHon, themGiay);
        }
        else if (castingSkill == CapDo.KyLuaDiaNguc)
        {
            // 4 qua toa quat nhu Qua cau lua roi tu di toi da 4 ke dich gan NGUOI TUNG nhat (LuaDiaNguc.SpawnChum)
            Vector3 aimAt = castAim + Vector3.up * 0.9f;
            Vector3 dir = aimAt - origin;
            dir.y = Mathf.Clamp(dir.y, -1.5f, 1.5f);
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            LuaDiaNguc.SpawnChum(origin, dir.normalized, transform.position, obstacleMask, enemyMask, health, manhHon, themGiay);
        }
        else if (castingSkill == CapDo.KyQuaCauBang)
        {
            // Nhu qua cau lua: ba qua toe quanh truc DUNG. Cap ky nang tang sat thuong va
            // keo dai lop cham (+0,15 giay moi cap - luat chung cua hieu ung).
            Vector3 aimAt = castAim + Vector3.up * 0.9f;
            Vector3 dir = aimAt - origin;
            dir.y = Mathf.Clamp(dir.y, -1.5f, 1.5f);
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            // Cap 5 ra 5 qua (nguoi dung 17/09/2026) - cap cua NGUOI TUNG di kem goi tin
            QuaCauBang.SpawnChum(origin, dir.normalized, obstacleMask, enemyMask, health,
                                 QuaCauBang.SoQuaTheoCap(capPhep), 11f, manhHon, themGiay, capPhep);
            CameraShake.Shake(0.12f, 0.05f);
        }
        else if (castingSkill == CapDo.KyGioLoc)
        {
            // Chum loc sinh o mat dat ngay truoc mat, toe quat quanh huong ngam, bay thang
            // xuyen moi vat can. Cap ky nang tang sat thuong va keo dai hat tung +0,15 s moi cap.
            Vector3 dir = castAim - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            Vector3 chan = transform.position + dir * 1.2f;
            // HINH QUAT (nguoi dung 26/09/2026): cap 1-4 ba loc, cap 5 nam loc, 15 do giua hai loc. capPhep la cap cua NGUOI TUNG
            // (di qua goi tin) nen moi may ra cung so loc.
            GioLoc.SpawnChum(chan, dir, enemyMask, health, GioLoc.SoLocTheoCap(capPhep), GioLoc.GocQuat, manhHon, themGiay);
        }
        else if (castingSkill == 1)
        {
            var mua = IceStorm.Spawn(castAim, enemyMask);
            if (mua != null)
            {
                mua.boQua = health;
                mua.shardDamage *= manhHon;
                mua.freezeSeconds += themGiay;
                mua.chamGiay += themGiay;
                // Cap 5: tang bang het gio thi no tung (TangBangNo). capPhep la cap cua NGUOI TUNG.
                mua.capKyNang = capPhep;
            }
        }
        else if (castingSkill == 2)
        {
            var bao = LightningStorm.Spawn(castAim, enemyMask);
            if (bao != null)
            {
                bao.boQua = health;
                bao.strikeDamage *= manhHon;
                bao.stunSeconds += themGiay;
            }
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
            if (loc != null)
            {
                loc.boQua = health;
                loc.damagePerSecond *= manhHon;
                loc.boltDamage *= manhHon;
                loc.duration += themGiay;       // con loc song lau hon = cuon lau hon
            }
        }
        else if (castingSkill == 4)
        {
            // THIEN THACH khong bay ra tu tay phu thuy - no roi tu tren troi
            // xuong dung cho ngam, nen khong dung "origin" o day.
            //
            // Goi CA LOAT ba qua noi duoi nhau, cach nhau ThienThach.GiayCachNhau (0,35 giay tu 25/09/2026). Xem
            // ThienThach.SpawnLoat - hai qua sau lech ra chung quanh chu khong
            // roi trung mot cho.
            // 40% danh nga 1,5 giay; cap ky nang cao keo dai them nhu moi hieu ung khac
            // CAP 5: 5 qua thay vi 3 (nguoi dung 19/09/2026 - ThienThach.SoQuaTheoCap)
            ThienThach.SpawnLoat(castAim, obstacleMask, enemyMask, health,
                                 ThienThach.SoQuaTheoCap(capPhep), ThienThach.GiayCachNhau, 2.8f, manhHon, themGiay,
                                 ThienThach.NgaXacSuatNguoiChoi, ThienThach.NgaGiayNguoiChoi + themGiay);
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
                // Khien khong manh len theo sat thuong ma theo MAU KHIEN (+15% moi cap)
                khiengHienTai = Khieng.Bat(gameObject,
                                           khiengMau * CapDo.MauKhiengTheoCap(capPhep),
                                           khiengBanKinh);
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

            // Nguoi dung 24/09/2026: tia moc ra GIUA HAI TAY (tu the day hai tay), khong phai tu dau gay
            Transform diemTay = hoatHinhRieng != null ? hoatHinhRieng.DiemGiatSet : null;
            Vector3 tuTay = diemTay != null ? diemTay.position : origin;

            var set = GiatSet.Phong(tuTay, dir.normalized, enemyMask);
            if (set != null)
            {
                set.diemTay = diemTay;
                set.boQua = health;
                set.damage *= manhHon;
                set.giayChoang += themGiay;     // cap ky nang keo dai choang nhu moi hieu ung
            }
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
        //
        // Dieu kien la "KHONG doc phim VA mau do may khac quyet" chu khong chi
        // moi "khong doc phim": ban sao mang luon co ca hai co (NguoiChoiKhac
        // dat ca hai trong cung mot ham), nen cho nay van chan ban sao y het.
        // Con phep thu bom input thi dat moi tuDocInput - truoc day no bi chan
        // luon, va moi phep do di chuyen deu ra 0,00 m du nhan vat khong he bi
        // lam sao.
        if (!tuDocInput && health != null && health.mauDoMayKhacQuyet) return;

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
        // BI DONG "Toc do di chuyen" (nguoi dung 25/09/2026): cong % cua TOC DO GOC. Chi nhan vat cua may nay -
        // cap ky nang nam trong CapDo cua may nay; ban sao nguoi khac (mau do may khac quyet) di theo goi tin.
        // KHONG xet tuDocInput: phep thu bom input tat co ay, xet no la do mot nhan vat khong bao gio duoc cong.
        bool cuaMayNay = health == null || !health.mauDoMayKhacQuyet;
        float tocThem = cuaMayNay ? TocGoc * CapDo.TocThemBiDong : 0f;
        float speed = (moveSpeed + tocThem) * (loiNuoc != null ? loiNuoc.HeSoToc : 1f);
        if (castTimer > 0f) { wish = Vector3.zero; speed = 0f; }

        // LOP BANG NANG CHAN, DONG CUNG VA CHOANG THI DUNG HAN.
        // Nhan chu khong gan de, de con cong don voi loi nuoc o tren.
        speed *= HeSoTocBang;
        // TANG HINH: di nhanh hon 20% (nguoi dung 18/09/2026)
        if (TangHinh.Dang(this)) speed *= TangHinh.HeSoToc;
        if (DangBiKhoaCung) { wish = Vector3.zero; speed = 0f; }

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
        t.hoiCauBang    = quaCauBangTimer;
        t.hoiGioLoc     = gioLocTimer;
        t.hoiLuaDiaNguc = luaDiaNgucTimer;
        t.hoiTangHinh   = tangHinhTimer;
        t.hoiCauDien    = cauDienTimer;
        t.hoiMayGiong   = mayGiongTimer;
        t.hoiHoaLocXoay = hoaLocXoayTimer;
        t.hoiTocBien    = tocBienTimer;
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
        quaCauBangTimer = t.hoiCauBang;
        gioLocTimer   = t.hoiGioLoc;
        luaDiaNgucTimer = t.hoiLuaDiaNguc;
        tangHinhTimer   = t.hoiTangHinh;
        cauDienTimer    = t.hoiCauDien;
        mayGiongTimer   = t.hoiMayGiong;
        hoaLocXoayTimer = t.hoiHoaLocXoay;
        tocBienTimer    = t.hoiTocBien;
        castTimer     = t.dangNiem;
        hasMoveTarget = t.coDiemDen;
        moveTarget    = t.diemDen;
    }
}
