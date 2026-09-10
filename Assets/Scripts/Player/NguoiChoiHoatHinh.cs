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

    void Awake()
    {
        if (vaCham == null) vaCham = GetComponent<CharacterController>();
        if (mau == null) mau = GetComponent<Damageable>();
        if (boPhat == null) boPhat = GetComponentInChildren<Animation>();
        LuuTuTheChuan();
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
        }
        veChuanTimer = 0f;
    }

    /// <summary>Bat dau tu the niem chu cho <paramref name="phep"/> (0..3).</summary>
    public void NiemChu(int phep, float thoiGian)
    {
        // 6 = Giut set dung chung tu the voi Sam set (2): deu la phep set, va
        // bang tu the chi co SAU o. De nguyen thi Clamp keo 6 ve 5 - tuc phu
        // thuy lam dong tac bat KHIENG trong khi tia set bay ra tu tay.
        if (phep == 6) phep = 2;
        phepDangNiem = Mathf.Clamp(phep, 0, 5);
        thoiGianNiem = Mathf.Max(0.2f, thoiGian);
        niemTimer = 0f;
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
                BatDauHoaVe();
            }
        }

        // ---- Dong ho hoa ve ----
        if (veChuanTimer >= 0f)
        {
            veChuanTimer += dt;
            if (veChuanTimer >= thoiGianVeChuan) veChuanTimer = -1f;
        }

        // ---- Muc di ----
        float muon = 0f;
        if (niemTimer < 0f)
        {
            if (tocDoEp >= 0f) muon = Mathf.Clamp01(tocDoEp);
            else if (vaCham != null)
            {
                Vector3 v = vaCham.velocity; v.y = 0f;
                muon = Mathf.Clamp01(v.magnitude / Mathf.Max(0.1f, tocDoDiToiDa));
            }
        }
        mucDi = Mathf.MoveTowards(mucDi, muon, dt * 4.2f);

        PhatClipDi();
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

        if (niemTimer >= 0f)
            TuTheNiemChu(phepDangNiem, Mathf.Clamp01(niemTimer / thoiGianNiem));

        HoaVe();
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
