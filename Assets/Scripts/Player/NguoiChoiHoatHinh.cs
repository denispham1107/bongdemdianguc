using UnityEngine;

/// <summary>
/// HOAT HINH CHO NHAN VAT NGUOI CHOI (model dung san dua tu Meshy vao).
///
/// Model chi kem DUNG MOT clip: di bo. Moi tu the con lai deu duoc dung TAI CHO
/// bang cach xoay thang cac khop xuong trong <see cref="LateUpdate"/> - lam o
/// LateUpdate thi no de len tren ket qua cua he hoat hinh, neu khong se bi clip
/// di bo ghi de mat.
///
///   - DUNG YEN : than tho nhe, ao chung chinh - de dung im khong ra pho tuong
///   - DI       : phat clip di bo
///   - NIEM CHU : BON tu the khac nhau cho bon phep, xem <see cref="TuTheNiemChu"/>
///   - CHET     : nga nguoi ve sau va lun xuong dat
///
/// MOI LAN ROI KHOI MOT TU THE DEU CO DOAN HOA VE (xem <see cref="thoiGianVeChuan"/>).
/// Thieu doan do thi nhan vat dung phat la dong cung giua buoc chan, hoac niem
/// chu xong la ket nguyen tay o tren troi.
/// </summary>
public class NguoiChoiHoatHinh : MonoBehaviour
{
    [Header("Khop xuong")]
    public Transform hips;
    public Transform spine;
    public Transform head;
    public Transform tayTraiTren, tayTraiDuoi;
    public Transform tayPhaiTren, tayPhaiDuoi;
    [Tooltip("Ban tay - de trong thi tu tim khop con ten co chu Hand duoi cang tay")]
    public Transform banTayTrai, banTayPhai;

    [Header("Tham chieu")]
    public Animation boPhat;
    public string tenClipDi = "diBo";
    public CharacterController vaCham;
    public Damageable mau;
    public float tocDoDiToiDa = 5.2f;

    [Header("Tho nhe khi dung yen")]
    public float tocDoTho = 1.15f;
    public float bienDoTho = 1.7f;

    [Header("Hoa ve tu the chuan")]
    [Tooltip("Bao lau de hoa tu tu tu tu the dang co ve tu the dung yen, giay")]
    public float thoiGianVeChuan = 0.26f;

    [Header("Niem chu")]
    /// <summary>
    /// Phep bay ra o luc nao trong don niem chu (0..1).
    ///
    /// <c>PlayerController</c> tha phep khi <c>castTimer &lt;= castTotal * 0.45</c>,
    /// tuc la da troi qua 55% thoi gian niem. Nhung no goi sang day voi thoi
    /// gian dai gap 1.35 lan, nen quy ra tu the thi moc do roi vao 55/135 = 0.41.
    /// Dat sai cho nay thi tay vung mot dang ma phep bay ra mot neo.
    /// </summary>
    public float mocPhepBayRa = 0.41f;

    // ---- Tu the CHUAN cua toan bo khung xuong, doc mot lan luc bat dau ----
    Transform[] moiKhop;
    Quaternion[] qChuan;
    Vector3[] pChuan;

    // ---- Tu the ngay luc roi khoi mot trang thai, de hoa ve tu tu ----
    Quaternion[] qRoiKhoi;
    Vector3[] pRoiKhoi;
    float veChuanTimer = -1f;

    bool daLuu;
    bool clipDangLai;              // khung truoc clip di bo co dang chay khong

    int phepDangNiem = -1;
    float niemTimer = -1f;
    float thoiGianNiem = 0.8f;
    /// <summary>
    /// TOC DO DI EP TU BEN NGOAI, cho nhan vat khong tu di bang chan minh.
    ///
    /// Ban sao cua nguoi choi khac (va dan quai ben may khach) duoc DAT THANG
    /// vi tri theo goi tin, khong di qua CharacterController - nen
    /// <c>cc.velocity</c> cua chung luon bang 0 va bo hoat hinh ket luan la
    /// "dang dung yen". Ket qua: ho TRUOT tren mat dat nhu dang bay, chan
    /// khong nhuc nhich.
    ///
    /// Am (mac dinh -1) nghia la "tu do lay", tuc duong cu.
    /// </summary>
    public float tocDoEp = -1f;

    float mucDi;
    float nghiengChet;

    // ---- GIUT SET: day hai tay (nguoi dung 24/09/2026) ----
    bool niemGiatSet;
    float giayBayRa, giayGiuTay;

    /// <summary>
    /// Diem GIUA HAI BAN TAY, hoi nho ra truoc - tia Giut set moc ra tu day va bam theo no suot 0,6 giay.
    /// Cap nhat cuoi LateUpdate, sau khi tu the da dat xong.
    /// </summary>
    public Transform DiemGiatSet { get; private set; }

    /// <summary>Tay day ra truoc bao nhieu (0..1) o khung vua roi - cho phep thu doc.</summary>
    public float MucDayTay { get; private set; }

    // ================================================================
    //  CHAN CHAM DAT (nguoi dung 05/10/2026: "rat nhieu cho 2 chan nhan vat dung van con nhu bay lo lung tren mat dat")
    // ================================================================
    //
    // Menu 109 do 40 cho trong Act2 (ban cu): goc nhan vat LUON cao hon dat 0,08 m (skinWidth cua CharacterController - day con nhong
    // dat dung o goc), tu the dung yen la khung BUOC DO cua clip di (mot chan nhac ~5 cm), dat doc thi mot chan ho toi 0,155 m.
    // Sua o day (dung chung cho nhan vat cua minh, ban sao nguoi choi khac va nhan vat trung bay o man chinh), moi khung, SAU moi tu the:
    //   1. DUNG YEN (tron theo 1 - mucDi): chan ve tu the dung cua model (bind pose, ghi cung GocChanBind) khep bot chu A - hai chan THANG,
    //      bang nhau, ban chan phang (giong man chinh). ⚠️ Khong ngam rieng dui / cang chan thang xuong: xuong goi Meshy o bind pose da gap
    //      48,6 do trong khi luoi chan thang, be xuong thang thi LUOI chan cong.
    //   2. De giay moi ben = min(co chan - DeDuoiCoChan, mui chan - DeDuoiMuiChan) (do bang BakeMesh o bind pose), so voi mat dat ngay duoi
    //      (tia, cung cac lop CharacterController va cham). HA THAN qua xuong HONG: dung yen -> ben ho NHIEU nhat cham dat, dang di -> ben
    //      ho IT nhat cham dat (chan dang tru). Clip di ghi vi tri hong moi khung, dung yen thi TraVeTuTheChuan dat lai -> khong cong don.
    //   3. Ben kia (dung tren dat cao hon) NHAC LEN bang IK hai doan (dui + cang chan), ban chan giu goc cu.

    [Header("Chan cham dat")]
    [Tooltip("Tat thi bo qua (vd. phep thu can tu the goc)")]
    public bool chamDat = true;
    [Tooltip("Dat doc: nhac chan ben dat cao (IK) + nghieng ban chan theo mat dat. Tat (man chinh) thi hai chan luon thang, bang nhau - ben dat cao lun vai cm")]
    public bool thichNghiDoc = true;

    /// <summary>Thu hep chan chu A cua bind pose ve phia thang dung (0 = nguyen bind, co chan cach 0,40 m; 1 = thang dung, 0,23 m).
    /// 0,4 -> ~0,33 m, bang tu the cu ma nguoi dung muon giu o man chinh (0,34 m).</summary>
    public const float HeSoKhepChan = 0.4f;

    /// <summary>Xuong co chan (Foot) / mui chan (ToeBase) cao hon de giay bao nhieu khi ban chan dat phang (bind pose, BakeMesh 05/10/2026:
    /// trai 0,1363 / 0,0348, phai 0,1328 / 0,0367).</summary>
    public const float DeDuoiCoChan = 0.135f, DeDuoiMuiChan = 0.036f;

    /// <summary>Gioi han: ha than toi da / nang than toi da / nhac mot chan toi da (m). Khe lon hon HaThanToiDa = dang o tren khong, bo qua.</summary>
    public const float HaThanToiDa = 0.5f, NangThanToiDa = 0.15f, NhacChanToiDa = 0.3f;

    /// <summary>
    /// Goc xoay xuong chan o TU THE DUNG CUA MODEL (bind pose - luoi chan thang, ban chan phang), so voi transform cua SkinnedMeshRenderer;
    /// [ben, khop]: ben 0 trai / 1 phai, khop UpLeg / Leg / Foot / ToeBase. Doc tu Mesh.bindposes cua Player_Sorceress trong Editor
    /// 05/10/2026 roi GHI CUNG: luoi Meshy Read/Write TAT, ban build co the khong doc duoc bindposes (ngoai le tren WebGL = dung hinh).
    /// </summary>
    public static readonly Quaternion[,] GocChanBind =
    {
        { new Quaternion(0.99241f, 0.08686f, -0.08686f, 0.00484f), new Quaternion(0.87738f, -0.33069f, 0.33069f, -0.10719f),
          new Quaternion(0.89397f, -0.09097f, 0.09097f, 0.42926f), new Quaternion(0.69862f, -0.10925f, 0.10925f, 0.69862f) },
        { new Quaternion(0.99428f, -0.07553f, 0.07553f, 0.00201f), new Quaternion(0.88677f, 0.31438f, -0.31438f, -0.12634f),
          new Quaternion(0.88656f, 0.08998f, -0.08998f, 0.44476f), new Quaternion(0.69913f, 0.10594f, -0.10594f, 0.69913f) },
    };

    readonly Transform[] duiTren = new Transform[2], cangChan = new Transform[2], banChan = new Transform[2], muiChan = new Transform[2];
    bool coChan, daCoHaThan;
    Transform gocLuoi;          // transform cua SkinnedMeshRenderer - he quy chieu cua GocChanBind
    float haThan;
    Vector3 haHongCucBo;        // do dich hong (toa do cha cua hong) da cong o khung vua roi - BatDauHoaVe tru ra
    readonly float[] kheChan = new float[2];
    int matNaDat;               // cac lop CharacterController va cham (tru Player, Enemy, Ignore Raycast) - tinh mot lan
    readonly Vector3[] phapDat = { Vector3.up, Vector3.up };

    /// <summary>Ban chan nghieng theo mat dat toi da (do) khi dung yen.</summary>
    public const float GocNghiengBanChan = 30f;

    /// <summary>Phep thu doc: than da ha bao nhieu (m), khe de giay - dat moi ben TRUOC khi sua (trai, phai), chan nao da nhac bao nhieu.</summary>
    public float DoHaThan { get { return haThan; } }
    public Vector2 KheTruocKhiSua { get; private set; }
    public Vector2 NhacChan { get; private set; }

    void Awake()
    {
        if (vaCham == null) vaCham = GetComponent<CharacterController>();
        if (mau == null) mau = GetComponent<Damageable>();
        if (boPhat == null) boPhat = GetComponentInChildren<Animation>();
        // Prefab cu khong co hai truong ban tay: lay khop con cua cang tay
        if (banTayTrai == null) banTayTrai = TimBanTay(tayTraiDuoi);
        if (banTayPhai == null) banTayPhai = TimBanTay(tayPhaiDuoi);
        var d = new GameObject("DiemGiatSet");
        d.transform.SetParent(transform, false);
        d.transform.localPosition = new Vector3(0f, 1.25f, 0.45f);
        DiemGiatSet = d.transform;
        LuuTuTheChuan();
        ChuanBiChan();
    }

    void ChuanBiChan()
    {
        var smr = GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (smr == null || hips == null) return;
        gocLuoi = smr.transform;
        string[] ben = { "Left", "Right" };
        var moi = hips.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < 2; i++)
        {
            foreach (var t in moi)
            {
                if (t.name == ben[i] + "UpLeg") duiTren[i] = t;
                else if (t.name == ben[i] + "Leg") cangChan[i] = t;
                else if (t.name == ben[i] + "Foot") banChan[i] = t;
                else if (t.name == ben[i] + "ToeBase") muiChan[i] = t;
            }
            if (duiTren[i] == null || cangChan[i] == null || banChan[i] == null || muiChan[i] == null) return;
        }
        coChan = true;
    }

    static Transform TimBanTay(Transform cangTay)
    {
        if (cangTay == null) return null;
        for (int i = 0; i < cangTay.childCount; i++)
            if (cangTay.GetChild(i).name.Contains("Hand")) return cangTay.GetChild(i);
        return cangTay.childCount > 0 ? cangTay.GetChild(0) : null;
    }

    /// <summary>
    /// Chup lai tu the chuan cua TOAN BO khung xuong.
    ///
    /// Phai lay het moi khop chu khong chi bay cai tay chan minh dung toi: clip
    /// di bo xoay ca 24 khop, nen luc tra ve cung phai tra ve du 24. Chi tra ve
    /// vai khop thi nhung khop con lai ket lai giua buoc di.
    /// </summary>
    void LuuTuTheChuan()
    {
        if (daLuu) return;

        Transform goc = hips != null ? hips : transform;
        moiKhop = goc.GetComponentsInChildren<Transform>(true);

        qChuan = new Quaternion[moiKhop.Length];
        pChuan = new Vector3[moiKhop.Length];
        qRoiKhoi = new Quaternion[moiKhop.Length];
        pRoiKhoi = new Vector3[moiKhop.Length];

        for (int i = 0; i < moiKhop.Length; i++)
        {
            qChuan[i] = moiKhop[i].localRotation;
            pChuan[i] = moiKhop[i].localPosition;
        }

        daLuu = true;
    }

    /// <summary>Ghi lai tu the dang co, va bat dau doan hoa ve.</summary>
    void BatDauHoaVe()
    {
        if (!daLuu) return;
        for (int i = 0; i < moiKhop.Length; i++)
        {
            qRoiKhoi[i] = moiKhop[i].localRotation;
            pRoiKhoi[i] = moiKhop[i].localPosition;
            // do ha than cua khung truoc khong tinh la "tu the luc roi khoi" - khong thi luc hoa ve than bi ha HAI lan
            if (moiKhop[i] == hips) pRoiKhoi[i] -= haHongCucBo;
        }
        veChuanTimer = 0f;
    }

    /// <summary>Bat dau tu the niem chu cho <paramref name="phep"/> (0..3).</summary>
    public void NiemChu(int phep, float thoiGian)
    {
        // 6 = GIUT SET: tu the RIENG, day ca hai tay ra truoc va GIU suot luc tia con hien (nguoi dung
        // 24/09/2026, theo anh mau). Dong ho tinh bang GIAY chu khong theo ti le: phan giu tay dai
        // GiatSet.GiayTiaHien bat ke niem nhanh hay cham.
        niemGiatSet = phep == 6;
        if (niemGiatSet)
        {
            giayBayRa = Mathf.Max(0.2f, thoiGian) * mocPhepBayRa;
            giayGiuTay = GiatSet.GiayTiaHien - 0.08f;
            phepDangNiem = 6;
            thoiGianNiem = giayBayRa + giayGiuTay;
            niemTimer = 0f;
            return;
        }
        // 9 = Qua cau bang: cung dong tac DAM THANG ra truoc nhu Qua cau lua
        if (phep == CapDo.KyQuaCauBang) phep = 0;
        // 10 = Gio loc: dong tac cua Loc xoay
        if (phep == CapDo.KyGioLoc) phep = 3;
        // 11 = Lua dia nguc: dam thang nhu Qua cau lua
        if (phep == CapDo.KyLuaDiaNguc) phep = 0;
        // 12 = Tang hinh: dong tac nhu Khien (phep tu bao ve)
        if (phep == CapDo.KyTangHinh) phep = 5;
        // 13 = Qua cau dien: goi cau dien xuong nhu Sam set
        if (phep == CapDo.KyCauDien) phep = 2;
        // 14 = Hoa loc xoay: dong tac cua Loc xoay
        if (phep == CapDo.KyHoaLocXoay) phep = 3;
        // 15 = Toc bien: dong tac nhu Khien (phep tu than)
        if (phep == CapDo.KyTocBien) phep = 5;
        // 21 = May giong: goi may xuong nhu Sam set
        if (phep == CapDo.KyMayGiong) phep = 2;
        phepDangNiem = Mathf.Clamp(phep, 0, 5);
        thoiGianNiem = Mathf.Max(0.2f, thoiGian);
        niemTimer = 0f;
    }

    /// <summary>Bi ngat chieu: bo tu the niem, hoa ve tu the dung.</summary>
    public void HuyNiem()
    {
        if (niemTimer < 0f) return;
        niemTimer = -1f;
        phepDangNiem = -1;
        niemGiatSet = false;
        BatDauHoaVe();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (mau != null && mau.IsDead)
        {
            nghiengChet = Mathf.MoveTowards(nghiengChet, 1f, dt * 1.5f);
            niemTimer = -1f;
            veChuanTimer = -1f;
            if (boPhat != null && boPhat.IsPlaying(tenClipDi)) boPhat.Stop(tenClipDi);
            return;
        }

        // ---- Dong ho niem chu ----
        if (niemTimer >= 0f)
        {
            niemTimer += dt;
            if (niemTimer >= thoiGianNiem)
            {
                // Niem xong: hoa tu tu ve tu the dung yen, khong bo tay cai rup
                niemTimer = -1f;
                phepDangNiem = -1;
                niemGiatSet = false;
                BatDauHoaVe();
            }
            // Dang giu tay Giut set ma nguoi choi buoc di: bo tay xuong de chan buoc, khong truot nhu tuong
            else if (niemGiatSet && niemTimer > giayBayRa + 0.12f && TocDoDi() > 0.15f)
                HuyNiem();
        }

        // ---- Dong ho hoa ve ----
        if (veChuanTimer >= 0f)
        {
            veChuanTimer += dt;
            if (veChuanTimer >= thoiGianVeChuan) veChuanTimer = -1f;
        }

        // ---- Muc di ----
        float muon = niemTimer < 0f ? TocDoDi() : 0f;
        mucDi = Mathf.MoveTowards(mucDi, muon, dt * 4.2f);

        PhatClipDi();
    }

    /// <summary>Muc di bo 0..1 (toc do ep tu mang, hoac van toc that cua CharacterController).</summary>
    float TocDoDi()
    {
        if (tocDoEp >= 0f) return Mathf.Clamp01(tocDoEp);
        if (vaCham == null) return 0f;
        Vector3 v = vaCham.velocity; v.y = 0f;
        return Mathf.Clamp01(v.magnitude / Mathf.Max(0.1f, tocDoDiToiDa));
    }

    /// <summary>
    /// Bat tat clip di bo.
    ///
    /// LUC TAT PHAI GHI LAI TU THE TRUOC. <c>Animation.Stop</c> cua he hoat hinh
    /// cu de nguyen khung xuong o dung khung hinh cuoi cua clip, khong tu tra ve
    /// tu the chuan. Khong ghi lai va hoa dan thi nhan vat dung phat la dong
    /// cung giua buoc chan.
    /// </summary>
    void PhatClipDi()
    {
        if (boPhat == null) return;
        var trang = boPhat[tenClipDi];
        if (trang == null) return;

        bool nenChay = mucDi >= 0.02f;

        if (nenChay)
        {
            if (!boPhat.IsPlaying(tenClipDi)) boPhat.Play(tenClipDi);
            trang.speed = Mathf.Lerp(0.7f, 1.25f, mucDi);
            clipDangLai = true;
            return;
        }

        if (clipDangLai)
        {
            BatDauHoaVe();
            boPhat.Stop(tenClipDi);
            clipDangLai = false;
        }
    }

    // ================================================================
    //  DE TU THE TU DUNG LEN TREN KET QUA CUA CLIP
    // ================================================================

    void LateUpdate()
    {
        if (!daLuu) return;
        if (mau != null && mau.IsDead) { TuTheChet(); return; }

        // Dang di thi de clip lo, chi cho phan hoa ve (neu con) chen vao
        if (!clipDangLai)
        {
            TraVeTuTheChuan();
            TuTheThoNhe();
        }

        MucDayTay = 0f;
        if (niemTimer >= 0f)
        {
            if (niemGiatSet) TuTheGiatSet(niemTimer);
            else TuTheNiemChu(phepDangNiem, Mathf.Clamp01(niemTimer / thoiGianNiem));
        }

        HoaVe();
        ChanChamDat(Time.deltaTime);
        CapNhatDiemGiatSet();
    }

    /// <summary>Xem phan "CHAN CHAM DAT" o dau lop.</summary>
    void ChanChamDat(float dt)
    {
        haHongCucBo = Vector3.zero;
        if (!chamDat || !coChan) return;
        // Bi danh nga / hat tung: model con dang bi lat - de nguyen
        var nga = GetComponent<BiDanhNga>(); var hat = GetComponent<BiHatTung>();
        if ((nga != null && nga.DangNga) || (hat != null && hat.DangBay)) { daCoHaThan = false; return; }

        float w = 1f - Mathf.Clamp01(mucDi);
        if (w > 0.001f) DungChan(w);

        // Khe de giay - mat dat moi ben. Dung yen thi ban chan NGHIENG THEO MAT DAT (toi da GocNghiengBanChan) - dat doc 17 do ma ban chan
        // phang thi got / mui ho 4,8 cm (menu 109); de giay nghieng goc t thi co chan cao hon mat dat DeDuoiCoChan / cos t.
        // Dang di (w = 0): ban chan theo clip, de giay = diem thap nhat cua co chan / mui chan.
        int coDat = 0; float thap = 1e6f, cao = -1e6f;
        for (int i = 0; i < 2; i++)
        {
            float dat; Vector3 phap;
            if (MatDatDuoi(banChan[i].position, out dat, out phap))
            {
                Vector3 nw = thichNghiDoc ? Vector3.RotateTowards(Vector3.up, phap, GocNghiengBanChan * Mathf.Deg2Rad * w, 0f) : Vector3.up;
                phapDat[i] = nw;
                float deDi = Mathf.Min(banChan[i].position.y - DeDuoiCoChan, muiChan[i].position.y - DeDuoiMuiChan);
                float deDung = banChan[i].position.y - DeDuoiCoChan / Mathf.Max(0.5f, nw.y);
                kheChan[i] = Mathf.Lerp(deDi, deDung, w) - dat;
                coDat++; thap = Mathf.Min(thap, kheChan[i]); cao = Mathf.Max(cao, kheChan[i]);
            }
            else { kheChan[i] = float.NaN; phapDat[i] = Vector3.up; }
        }
        KheTruocKhiSua = new Vector2(kheChan[0], kheChan[1]);
        if (coDat == 0 || thap > HaThanToiDa) { daCoHaThan = false; NhacChan = Vector2.zero; return; }
        if (coDat == 1) cao = thap;

        float muon = Mathf.Clamp(Mathf.Lerp(thap, cao, w), -NangThanToiDa, HaThanToiDa);
        haThan = daCoHaThan ? Mathf.Lerp(haThan, muon, 1f - Mathf.Exp(-25f * dt)) : muon;
        daCoHaThan = true;
        Vector3 xuong = Vector3.down * haThan;
        hips.position += xuong;
        haHongCucBo = hips.parent != null ? hips.parent.InverseTransformVector(xuong) : xuong;

        // Ben dung tren dat cao hon: nhac len cho cham dat (dang di thi than ha theo ben thap -> khong ben nao can nhac)
        var nhac = Vector2.zero;
        for (int i = 0; i < 2; i++)
        {
            if (float.IsNaN(kheChan[i]) || !thichNghiDoc) continue;
            float n = Mathf.Min(haThan - kheChan[i], NhacChanToiDa);
            if (n > 0.002f) { NhacChanIK(i, n); nhac[i] = n; }
            // ban chan nghieng theo mat dat (xoay quanh co chan - co chan khong dich; mui chan la con nen di theo)
            if (phapDat[i] != Vector3.up) banChan[i].rotation = Quaternion.FromToRotation(Vector3.up, phapDat[i]) * banChan[i].rotation;
        }
        NhacChan = nhac;
    }

    /// <summary>Tu the dung: chan ve bind pose, khep bot chu A, ban chan phang - tron voi tu the dang co theo w.</summary>
    void DungChan(float w)
    {
        Quaternion q = gocLuoi.rotation;
        for (int i = 0; i < 2; i++)
        {
            Quaternion r0 = duiTren[i].rotation, r1 = cangChan[i].rotation, r2 = banChan[i].rotation, r3 = muiChan[i].rotation;
            duiTren[i].rotation = q * GocChanBind[i, 0];
            cangChan[i].rotation = q * GocChanBind[i, 1];
            Vector3 chan = (banChan[i].position - duiTren[i].position).normalized;
            duiTren[i].rotation = Quaternion.FromToRotation(chan, Vector3.Slerp(chan, Vector3.down, HeSoKhepChan)) * duiTren[i].rotation;
            if (w < 0.999f)
            {
                Quaternion t0 = duiTren[i].rotation, t1 = cangChan[i].rotation;
                duiTren[i].rotation = Quaternion.Slerp(r0, t0, w);
                cangChan[i].rotation = Quaternion.Slerp(r1, t1, w);
            }
            banChan[i].rotation = Quaternion.Slerp(r2, q * GocChanBind[i, 2], w);
            muiChan[i].rotation = Quaternion.Slerp(r3, q * GocChanBind[i, 3], w);
        }
    }

    /// <summary>IK hai doan: dua co chan ben i len cao them <paramref name="nhac"/> m (gap goi trong mat phang chan dang co), ban chan giu goc.</summary>
    void NhacChanIK(int i, float nhac)
    {
        Transform a = duiTren[i], b = cangChan[i], c = banChan[i];
        Quaternion qBan = c.rotation, qMui = muiChan[i].rotation;
        Vector3 pa = a.position, pb = b.position, pc = c.position;
        Vector3 dich = pc + Vector3.up * nhac;
        float la = (pb - pa).magnitude, lb = (pc - pb).magnitude;
        if (la < 1e-4f || lb < 1e-4f) return;
        float lc = Mathf.Clamp((dich - pa).magnitude, Mathf.Abs(la - lb) + 1e-3f, la + lb - 1e-3f);
        float k0 = Vector3.Angle(pa - pb, pc - pb);
        float k1 = Mathf.Acos(Mathf.Clamp((la * la + lb * lb - lc * lc) / (2f * la * lb), -1f, 1f)) * Mathf.Rad2Deg;
        Vector3 n = Vector3.Cross(pb - pa, pc - pb);
        if (n.sqrMagnitude < 1e-8f) n = -transform.right;
        b.rotation = Quaternion.AngleAxis(k0 - k1, n.normalized) * b.rotation;
        a.rotation = Quaternion.FromToRotation(c.position - pa, dich - pa) * a.rotation;
        c.rotation = qBan; muiChan[i].rotation = qMui;
    }

    /// <summary>Mat dat ngay duoi diem p (tia tu tren goc nhan vat xuong, cac lop ma CharacterController va cham, tru nguoi choi / quai).</summary>
    bool MatDatDuoi(Vector3 p, out float y, out Vector3 phap)
    {
        phap = Vector3.up;
        if (matNaDat == 0)
        {
            int lp = LayerMask.NameToLayer("Player"), le = LayerMask.NameToLayer("Enemy");
            for (int i = 0; i < 32; i++)
                if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i) && i != lp && i != le && i != 2) matNaDat |= 1 << i;
        }
        int lop = matNaDat;
        RaycastHit h;
        if (Physics.Raycast(new Vector3(p.x, transform.position.y + 0.8f, p.z), Vector3.down, out h, 2.5f, lop, QueryTriggerInteraction.Ignore))
        { y = h.point.y; phap = h.normal; return true; }
        y = 0f; return false;
    }

    /// <summary>Dat DiemGiatSet vao giua hai ban tay, nho ra truoc 0,18 m (tay co thi lay, khong thi giu cho cu).</summary>
    void CapNhatDiemGiatSet()
    {
        if (DiemGiatSet == null || banTayTrai == null || banTayPhai == null) return;
        DiemGiatSet.position = (banTayTrai.position + banTayPhai.position) * 0.5f + transform.forward * 0.18f;
    }

    /// <summary>
    /// GIUT SET (nguoi dung 24/09/2026, theo anh mau): GOM hai tay ve truoc nguc luc niem, roi DAY THANG ca
    /// hai tay ve phia truoc - hai ban tay chum vao nhau, tia moc ra giua - va GIU the do suot luc tia con
    /// hien, tay rung nhe theo dong dien. Het gio thi HoaVe dua ve tu the dung.
    ///
    /// Khac cac tu the kia (xoay THEM mot goc Euler vao khop): o day NGAM HUONG xuong - xoay canh tay cho
    /// no chi dung huong muon trong the gioi. Truc rieng cua khop model Meshy khong ro rang, doan goc Euler
    /// de "chi thang ra truoc" thi lech tuy model; ngam huong thi dung bat ke truc khop.
    /// </summary>
    void TuTheGiatSet(float giay)
    {
        float gom = Mathf.SmoothStep(0f, 1f, giay / Mathf.Max(0.01f, giayBayRa));
        float day = Mathf.SmoothStep(0f, 1f, (giay - giayBayRa * 0.75f) / 0.10f);
        MucDayTay = day;

        Vector3 f = transform.forward, r = transform.right, u = Vector3.up;
        float rung = day * 0.035f;
        float t = Time.time;

        // Gom: canh tay tren buong xuong hoi ra ngoai, cang tay chi vao giua truoc nguc
        // Day: ca hai tay thang ra truoc, hoi chum vao giua; tay phai cao hon mot chut nhu anh mau
        for (int ben = -1; ben <= 1; ben += 2)
        {
            bool phai = ben > 0;
            Vector3 ngoai = r * ben;
            Vector3 tren = (-u * 0.85f - f * 0.05f + ngoai * 0.35f).normalized;
            Vector3 duoi = (f * 0.75f + u * 0.30f - ngoai * 0.60f).normalized;
            Vector3 tren2 = (f - ngoai * 0.10f + u * (phai ? 0.08f : 0.0f)).normalized;
            Vector3 duoi2 = (f - ngoai * 0.17f + u * (phai ? 0.06f : 0.0f)
                            + r * rung * Mathf.Sin(t * 47f + ben) + u * rung * Mathf.Sin(t * 53f + ben * 2f)).normalized;

            Vector3 muonTren = Vector3.Slerp(tren, tren2, day);
            Vector3 muonDuoi = Vector3.Slerp(duoi, duoi2, day);
            float w = Mathf.Max(gom, day);

            NgamHuong(phai ? tayPhaiTren : tayTraiTren, phai ? tayPhaiDuoi : tayTraiDuoi, muonTren, w);
            NgamHuong(phai ? tayPhaiDuoi : tayTraiDuoi, phai ? banTayPhai : banTayTrai, muonDuoi, w);
        }

        // Nguoi hoi chui ve truoc theo tay (x duong = cui, xem QuaCauLua)
        Nghieng(spine, -4f * gom + 12f * day, 0f, 0f);
        Nghieng(head, -6f * day, 0f, 0f);
    }

    /// <summary>Ban cong khai cua NgamHuong - TuTheTrungBay (tu the man chinh) dung chung cach ngam huong xuong.</summary>
    public static void NgamHuongKhop(Transform khop, Transform con, Vector3 muon, float w) { NgamHuong(khop, con, muon, w); }

    /// <summary>Xoay <paramref name="khop"/> de doan khop -> <paramref name="con"/> chi theo huong <paramref name="muon"/> (the gioi), tron theo w.</summary>
    static void NgamHuong(Transform khop, Transform con, Vector3 muon, float w)
    {
        if (khop == null || con == null || w <= 0f) return;
        Vector3 d = con.position - khop.position;
        if (d.sqrMagnitude < 1e-8f) return;
        Quaternion q = Quaternion.FromToRotation(d.normalized, muon);
        khop.rotation = Quaternion.Slerp(Quaternion.identity, q, w) * khop.rotation;
    }

    /// <summary>Dat MOI khop ve dung tu the chuan.</summary>
    void TraVeTuTheChuan()
    {
        for (int i = 0; i < moiKhop.Length; i++)
        {
            moiKhop[i].localRotation = qChuan[i];
            moiKhop[i].localPosition = pChuan[i];
        }
    }

    void TuTheThoNhe()
    {
        float doTho = bienDoTho;
        float t = Time.time;
        float song = Mathf.Sin(t * tocDoTho);
        float song2 = Mathf.Sin(t * tocDoTho * 0.61f + 0.8f);

        if (spine != null)
            spine.localRotation *= Quaternion.Euler(song * doTho * 0.55f, 0f, song2 * doTho * 0.30f);
        if (head != null)
            head.localRotation *= Quaternion.Euler(song2 * doTho * 0.8f, song * doTho * 1.2f, 0f);
        if (hips != null)
            hips.localPosition += new Vector3(0f, song * doTho * 0.0011f, 0f);
    }

    /// <summary>
    /// Hoa tu tu tu tu the LUC ROI KHOI ve tu the vua tinh o tren.
    ///
    /// Chay SAU CUNG, sau khi moi thu khac da viet xong, nen no hoa ve dung cai
    /// dich that su - du dich do la dung yen, dang tho, hay dang niem chu.
    /// </summary>
    void HoaVe()
    {
        if (veChuanTimer < 0f) return;

        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(veChuanTimer / thoiGianVeChuan));

        for (int i = 0; i < moiKhop.Length; i++)
        {
            moiKhop[i].localRotation = Quaternion.Slerp(qRoiKhoi[i], moiKhop[i].localRotation, k);
            moiKhop[i].localPosition = Vector3.Lerp(pRoiKhoi[i], moiKhop[i].localPosition, k);
        }
    }

    /// <summary>
    /// Tach mot don niem chu thanh hai pha: GO SUC roi BUNG RA.
    ///
    /// Phep bay ra dung o <see cref="mocPhepBayRa"/>, nen doan truoc moc do la
    /// go suc (<paramref name="go"/> tang dan len 1) va doan sau la bung ra
    /// (<paramref name="bung"/> vot len 1 that nhanh roi ha ve 0).
    ///
    /// CA HAI DEU PHAI VE 0 O CUOI DON. Ban dau toi de <c>go = 1 - bung * 0.85</c>,
    /// ma cuoi don thi <c>bung</c> = 0 nen <c>go</c> hoa ra bang 1 - tu the go
    /// suc dang o muc manh nhat dung luc dong ho het gio, roi ket cung o do.
    /// </summary>
    void ChiaPha(float t, out float go, out float bung)
    {
        if (t < mocPhepBayRa)
        {
            go = Mathf.SmoothStep(0f, 1f, t / mocPhepBayRa);
            bung = 0f;
            return;
        }

        float u = (t - mocPhepBayRa) / (1f - mocPhepBayRa);
        bung = u < 0.24f ? Mathf.SmoothStep(0f, 1f, u / 0.24f)
                         : Mathf.SmoothStep(1f, 0f, (u - 0.24f) / 0.76f);

        // Nua sau cua don, phan go suc nhat dan ve 0 de ket thuc o tu the chuan
        float nhat = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((u - 0.40f) / 0.60f));
        go = (1f - bung * 0.85f) * nhat;
    }

    /// <summary>
    /// BON TU THE NIEM CHU, moi phep mot kieu.
    ///
    /// Bon phep khac han nhau ve tinh chat nen dong tac cung phai khac: mot qua
    /// cau ban thang thi phai VUON GAY RA TRUOC, con mot con bao tuyet trum ca
    /// vung thi phai GIO HAI TAY LEN TROI. Dung chung mot dong tac cho ca bon
    /// thi bam phim nao cung thay y het nhau, mat han cam giac moi phep mot ve.
    /// </summary>
    void TuTheNiemChu(int phep, float t)
    {
        float go, bung;
        ChiaPha(t, out go, out bung);

        switch (phep)
        {
            case 0: QuaCauLua(go, bung); break;
            case 1: MuaBang(go, bung); break;
            case 2: SamSet(go, bung); break;
            case 3: LocXoay(go, bung); break;
            case 4: ThienThach(go, bung); break;
            default: KhiengBaoVe(go, bung); break;
        }
    }

    /// <summary>Phep 1 - QUA CAU LUA: rut gay ve sau vai roi DAM THANG ra truoc.</summary>
    void QuaCauLua(float go, float bung)
    {
        Nghieng(spine, -12f * go + 20f * bung, -16f * go + 12f * bung, 0f);
        Nghieng(head, -6f * go + 10f * bung, 0f, 0f);

        Nghieng(tayPhaiTren, -46f * go - 62f * bung, 26f * go - 30f * bung, -18f * go);
        Nghieng(tayPhaiDuoi, -70f * go + 84f * bung, 0f, 0f);

        Nghieng(tayTraiTren, -22f * go - 14f * bung, -18f * go, 20f * go);
        Nghieng(tayTraiDuoi, -30f * go, 0f, 0f);
    }

    /// <summary>Phep 2 - MUA BANG: gio CA HAI tay len troi roi bo rong ra hai ben.</summary>
    void MuaBang(float go, float bung)
    {
        Nghieng(spine, -20f * go + 8f * bung, 0f, 0f);
        Nghieng(head, -24f * go + 6f * bung, 0f, 0f);   // ngua mat nhin troi

        Nghieng(tayPhaiTren, -118f * go + 44f * bung, 0f, -26f * go - 34f * bung);
        Nghieng(tayPhaiDuoi, -24f * go, 0f, 0f);

        Nghieng(tayTraiTren, -118f * go + 44f * bung, 0f, 26f * go + 34f * bung);
        Nghieng(tayTraiDuoi, -24f * go, 0f, 0f);
    }

    /// <summary>Phep 3 - SAM SET: ngua nguoi gio gay len troi roi BO MANH xuong dat.</summary>
    void SamSet(float go, float bung)
    {
        Nghieng(spine, -30f * go + 46f * bung, 0f, 0f);
        Nghieng(head, -20f * go + 30f * bung, 0f, 0f);

        Nghieng(tayPhaiTren, -142f * go + 190f * bung, 0f, -14f * go);
        Nghieng(tayPhaiDuoi, -18f * go + 40f * bung, 0f, 0f);

        Nghieng(tayTraiTren, -30f * go + 24f * bung, 0f, 22f * go);
        Nghieng(tayTraiDuoi, -46f * go, 0f, 0f);
    }

    /// <summary>
    /// Phep 6 - KHIENG BAO VE: THU NGUOI LAI, hai tay khoanh cheo truoc nguc de
    /// do, roi BUNG MANH ca hai tay ra hai ben - dung dong tac day mot buc tuong
    /// vo hinh ra khoi minh.
    ///
    /// Day la phep duy nhat trong sau phep KHONG vuon ra phia truoc: nam phep
    /// kia deu nem thu gi do di, con phep nay thi keo ve che chan.
    /// </summary>
    void KhiengBaoVe(float go, float bung)
    {
        // Thu nguoi lai roi uon nguc ra
        Nghieng(spine, 24f * go - 30f * bung, 0f, 0f);
        Nghieng(head, 16f * go - 22f * bung, 0f, 0f);

        // Hai tay khoanh cheo truoc nguc (go) roi bung ngang ra (bung)
        Nghieng(tayPhaiTren, -52f * go - 12f * bung, -48f * go + 96f * bung, -34f * go + 46f * bung);
        Nghieng(tayPhaiDuoi, -96f * go + 70f * bung, 0f, 0f);

        Nghieng(tayTraiTren, -52f * go - 12f * bung, 48f * go - 96f * bung, 34f * go - 46f * bung);
        Nghieng(tayTraiDuoi, -96f * go + 70f * bung, 0f, 0f);
    }

    /// <summary>
    /// Phep 5 - THIEN THACH: NGUA HAN NGUOI RA SAU, gio ca hai tay len troi goi
    /// da roi xuong, roi bo manh ca hai tay ve truoc chi diem roi.
    ///
    /// Phai khac han Sam set (mot tay vut len roi bo xuong) va Mua bang (hai tay
    /// len roi xoe rong sang hai ben): o day nguoi ngua ra sau nhieu nhat trong
    /// ca nam phep, vi phu thuy dang goi mot thu tu tren troi rat cao.
    /// </summary>
    void ThienThach(float go, float bung)
    {
        // Ngua han ra sau roi gap manh ve truoc
        Nghieng(spine, -42f * go + 60f * bung, 0f, 0f);
        Nghieng(head, -34f * go + 44f * bung, 0f, 0f);

        // CA HAI tay vut thang len troi, roi bo ve truoc
        Nghieng(tayPhaiTren, -158f * go + 196f * bung, 0f, -20f * go + 10f * bung);
        Nghieng(tayPhaiDuoi, -12f * go + 34f * bung, 0f, 0f);

        Nghieng(tayTraiTren, -158f * go + 196f * bung, 0f, 20f * go - 10f * bung);
        Nghieng(tayTraiDuoi, -12f * go + 34f * bung, 0f, 0f);
    }

    /// <summary>Phep 4 - LOC XOAY: quat gay mot vong quanh nguoi.</summary>
    void LocXoay(float go, float bung)
    {
        Nghieng(spine, 0f, -40f * go + 66f * bung, -10f * go + 14f * bung);
        Nghieng(head, 0f, -20f * go + 34f * bung, 0f);

        Nghieng(tayPhaiTren, -74f * go - 20f * bung, 54f * go - 96f * bung, -30f * go);
        Nghieng(tayPhaiDuoi, -50f * go + 26f * bung, 0f, 0f);

        Nghieng(tayTraiTren, -40f * go + 18f * bung, -30f * go + 40f * bung, 26f * go);
        Nghieng(tayTraiDuoi, -54f * go, 0f, 0f);
    }

    /// <summary>Xoay THEM mot goc so voi tu the dang co cua khop.</summary>
    static void Nghieng(Transform khop, float x, float y, float z)
    {
        if (khop == null) return;
        khop.localRotation *= Quaternion.Euler(x, y, z);
    }

    /// <summary>Guc nga: nga nguoi ve sau, lun dan xuong dat.</summary>
    void TuTheChet()
    {
        TraVeTuTheChuan();
        // XAC NAM (XacNam lat ca model nam ngua): bo tu the guc, nam thang tren vung mau
        if (XacNam.LaXac(this)) return;

        float k = Mathf.SmoothStep(0f, 1f, nghiengChet);

        Nghieng(spine, -52f * k, 0f, 18f * k);
        Nghieng(head, -30f * k, 0f, 0f);
        Nghieng(tayPhaiTren, 36f * k, 0f, -26f * k);
        Nghieng(tayTraiTren, 36f * k, 0f, 26f * k);
        Nghieng(tayPhaiDuoi, 20f * k, 0f, 0f);
        Nghieng(tayTraiDuoi, 20f * k, 0f, 0f);

        if (hips != null) hips.localPosition += new Vector3(0f, -0.55f * k, 0f);
    }
}
