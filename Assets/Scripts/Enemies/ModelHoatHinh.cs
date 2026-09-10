using UnityEngine;

/// <summary>
/// HOAT HINH CHO QUAI DUNG MODEL SAN (dua tu Meshy vao).
///
/// Model Meshy chi kem DUNG MOT clip: di bo. Khong co clip dung yen, khong co
/// clip ra don, khong co clip guc nga. Ba tu the con thieu deu duoc dung TAI CHO
/// bang cach xoay thang cac khop xuong trong <see cref="LateUpdate"/> - lam o
/// LateUpdate thi no de len tren ket qua cua he hoat hinh, neu khong se bi clip
/// di bo ghi de mat.
///
///   - DUNG YEN : than tho nhe, dau khe lac - de dung im khong ra pho tuong
///   - DI       : phat clip di bo, hoa dan vao ra theo toc do that
///   - RA DON   : hai kieu, xem <see cref="KieuDon"/>
///   - CHET     : do nguoi ve truoc va lun dan xuong dat
///
/// MOI LAN ROI KHOI MOT TU THE DEU CO DOAN HOA VE (xem <see cref="thoiGianVeChuan"/>),
/// y het nhu nhan vat nguoi choi. Thieu doan do thi con quai dung phat la dong
/// cung giua buoc chan, hoac ra don xong la ket nguyen tay o tren troi.
///
/// Cac khop lay theo TEN trong model Meshy: Hips / Spine / Head /
/// LeftArm / LeftForeArm / RightArm / RightForeArm.
/// </summary>
public class ModelHoatHinh : MonoBehaviour
{
    /// <summary>Cach con quai ra don.</summary>
    public enum KieuDon
    {
        /// <summary>Ngua nguoi go suc roi hat CA HAI tay ve truoc - danh tu xa.</summary>
        NemPhep,
        /// <summary>Vung tay phai len cao ra sau roi bo xuong - danh can chien.</summary>
        ChemToi
    }

    [Header("Kieu ra don")]
    public KieuDon kieuDon = KieuDon.NemPhep;

    [Header("Khop xuong")]
    public Transform hips;
    public Transform spine;
    public Transform head;
    public Transform tayTraiTren, tayTraiDuoi;
    public Transform tayPhaiTren, tayPhaiDuoi;

    [Header("Tham chieu")]
    public Animation boPhat;          // he hoat hinh cu, giu clip di bo
    public string tenClipDi = "diBo";
    public EnemyAI ai;
    public Damageable mau;

    [Header("Nhip dieu")]
    [Tooltip("Doi lai mot nhip rieng cho tung con, khoi ca dam lac dong loat")]
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

    public float nhipRieng;
    public float tocDoTho = 1.35f;
    public float bienDoTho = 1.9f;

    [Header("Hoa ve tu the chuan")]
    [Tooltip("Bao lau de hoa tu tu tu the dang co ve tu the dung yen, giay")]
    public float thoiGianVeChuan = 0.26f;

    [Header("Ra don")]
    public float thoiGianDon = 0.85f;
    [Tooltip("Vao luc nay trong don thi don an - phai khop voi EnemyAI.attackHitAt")]
    public float mocAn = 0.45f;

    // Tu the goc cua tung khop, doc mot lan luc bat dau
    Quaternion qSpine, qHead, qTayTraiTren, qTayTraiDuoi, qTayPhaiTren, qTayPhaiDuoi;
    Vector3 pHips;
    bool daLuu;

    // ---- Tu the CHUAN cua TOAN BO khung xuong ----
    // Clip di bo xoay MOI khop, nen luc tra ve cung phai tra ve du MOI khop.
    // Chi tra ve vai khop thi nhung khop con lai ket lai giua buoc di.
    Transform[] moiKhop;
    Quaternion[] qChuan;
    Vector3[] pChuan;

    // ---- Tu the ngay luc roi khoi mot trang thai, de hoa ve tu tu ----
    Quaternion[] qRoiKhoi;
    Vector3[] pRoiKhoi;
    float veChuanTimer = -1f;
    bool clipDangLai;

    float donTimer = -1f;
    float mucDi;                       // 0 = dung, 1 = di het toc
    float nghiengChet;

    void Awake()
    {
        if (ai == null) ai = GetComponentInParent<EnemyAI>();
        if (mau == null) mau = GetComponentInParent<Damageable>();
        if (boPhat == null) boPhat = GetComponentInChildren<Animation>();

        // TAT TU PHAT. Clip "diBo" cua model de playAutomatically = true, nen no
        // lay mau khung xuong ngay tu khung hinh dau. Tu the chuan doc sau do se
        // ra tu the DANG BUOC chu khong phai tu the dung yen, va moi lan hoa ve
        // deu ve nham cho. Ta tu quyet dinh luc nao phat, trong PhatClipDi.
        if (boPhat != null)
        {
            boPhat.playAutomatically = false;
            boPhat.Stop();
        }

        if (mau != null) mau.onDeath += ThayChet;
    }

    void OnDestroy()
    {
        if (mau != null) mau.onDeath -= ThayChet;
    }

    /// <summary>
    /// Doc tu the chuan cua ca khung xuong. Goi TRE, khong goi trong Awake.
    ///
    /// VI SAO PHAI GOI TRE: <c>EnemyFactory</c> lam theo thu tu
    /// <c>AddComponent&lt;ModelHoatHinh&gt;()</c> roi MOI gan <c>hips</c>,
    /// <c>spine</c>... Ma <c>AddComponent</c> tren mot vat dang bat thi chay
    /// <c>Awake</c> NGAY LAP TUC, tuc luc do moi khop van con la null. Doc o
    /// Awake thi <c>qSpine</c> va cac ban cua no ket lai o <c>default(Quaternion)</c>
    /// = (0,0,0,0) - day KHONG phai quaternion don vi ma la quaternion rong,
    /// nhan voi no ra rong, dap vao khop la con quai bien dang.
    ///
    /// <c>nhipRieng</c> cung phai tinh tre: doc toa do luc Awake thi con quai
    /// chua duoc dat cho, ca dan deu ra nhip 0 va tho lac dong loat.
    /// </summary>
    void LuuTuTheGoc()
    {
        if (daLuu) return;
        if (hips == null && spine == null) return;   // chua gan xong, doi luot sau

        if (spine != null) qSpine = spine.localRotation;
        if (head != null) qHead = head.localRotation;
        if (tayTraiTren != null) qTayTraiTren = tayTraiTren.localRotation;
        if (tayTraiDuoi != null) qTayTraiDuoi = tayTraiDuoi.localRotation;
        if (tayPhaiTren != null) qTayPhaiTren = tayPhaiTren.localRotation;
        if (tayPhaiDuoi != null) qTayPhaiDuoi = tayPhaiDuoi.localRotation;
        if (hips != null) pHips = hips.localPosition;

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

        if (nhipRieng == 0f)
        {
            // Nhip lay tu chinh toa do, nen moi lan choi lai van y nhu cu
            Vector3 p = transform.position;
            nhipRieng = Mathf.Repeat(Mathf.Abs(p.x * 12.9898f + p.z * 78.233f), 10f);
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

    /// <summary>Dat MOI khop ve dung tu the chuan.</summary>
    void TraVeTuTheChuan()
    {
        for (int i = 0; i < moiKhop.Length; i++)
        {
            moiKhop[i].localRotation = qChuan[i];
            moiKhop[i].localPosition = pChuan[i];
        }
    }

    /// <summary>
    /// Hoa tu tu tu tu the LUC ROI KHOI ve tu the vua tinh o tren.
    ///
    /// Chay SAU CUNG, sau khi moi thu khac da viet xong, nen no hoa ve dung cai
    /// dich that su - du dich do la dung yen, dang tho, hay dang ra don.
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

    void ThayChet(Damageable d)
    {
        donTimer = -1f;
        veChuanTimer = -1f;
        clipDangLai = false;
        if (boPhat != null) boPhat.Stop();
    }

    /// <summary>Bat dau mot don danh. <see cref="EnemyAI"/> goi sang.</summary>
    public void RaDon(float thoiGian)
    {
        thoiGianDon = Mathf.Max(0.2f, thoiGian);
        donTimer = 0f;
    }

    void Update()
    {
        LuuTuTheGoc();                    // lan dau chay duoc thi moi doc duoc
        if (!daLuu) return;

        float dt = Time.deltaTime;

        if (mau != null && mau.IsDead)
        {
            nghiengChet = Mathf.MoveTowards(nghiengChet, 1f, dt * 1.7f);
            veChuanTimer = -1f;
            return;
        }

        if (donTimer >= 0f)
        {
            donTimer += dt;
            if (donTimer >= thoiGianDon)
            {
                // Ra don xong: hoa tu tu ve tu the dung yen, khong bo tay cai rup
                donTimer = -1f;
                BatDauHoaVe();
            }
        }

        if (veChuanTimer >= 0f)
        {
            veChuanTimer += dt;
            if (veChuanTimer >= thoiGianVeChuan) veChuanTimer = -1f;
        }

        // Dang ra don thi dung han lai, khong buoc
        float muon = (donTimer >= 0f) ? 0f : TocDoDangDi();
        mucDi = Mathf.MoveTowards(mucDi, muon, dt * 3.4f);

        PhatClipDi();
    }

    /// <summary>Con nay dang di nhanh cham the nao, quy ve 0..1.</summary>
    float TocDoDangDi()
    {
        if (tocDoEp >= 0f) return Mathf.Clamp01(tocDoEp);

        var cc = ai != null ? ai.GetComponent<CharacterController>() : null;
        if (cc == null || ai == null) return 0f;

        Vector3 v = cc.velocity; v.y = 0f;
        return Mathf.Clamp01(v.magnitude / Mathf.Max(0.1f, ai.moveSpeed));
    }

    /// <summary>
    /// Bat tat clip di bo.
    ///
    /// LUC TAT PHAI GHI LAI TU THE TRUOC. <c>Animation.Stop</c> cua he hoat hinh
    /// cu de nguyen khung xuong o dung khung hinh cuoi cua clip, khong tu tra ve
    /// tu the chuan. Khong ghi lai va hoa dan thi con quai dung phat la dong
    /// cung giua buoc chan - chan truoc chan sau, treo lo lung.
    /// </summary>
    void PhatClipDi()
    {
        if (boPhat == null) return;
        var trang = boPhat[tenClipDi];
        if (trang == null) return;

        if (mucDi >= 0.02f)
        {
            if (!boPhat.IsPlaying(tenClipDi)) boPhat.Play(tenClipDi);
            trang.weight = mucDi;
            trang.speed = Mathf.Lerp(0.55f, 1.15f, mucDi);
            clipDangLai = true;
            return;
        }

        // Dung han clip lai, tra khop ve tu the goc de phan tho nhe ben duoi
        // duoc nhin thay. De clip chay tiep voi trong so 0 thi khop van bi no
        // ghi de moi khung hinh.
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

        // ---- Ra don ----
        if (donTimer >= 0f)
        {
            float u = Mathf.Clamp01(donTimer / thoiGianDon);
            if (kieuDon == KieuDon.NemPhep) TuTheNemPhep(u);
            else TuTheChemToi(u);
        }

        HoaVe();
    }

    /// <summary>Tho nhe khi dung yen - de dung im khong ra pho tuong.</summary>
    void TuTheThoNhe()
    {
        float t = Time.time + nhipRieng;

        // Cang di nhanh thi phan tho cang mo di, de khoi danh nhau voi clip di bo
        float doTho = (1f - mucDi) * bienDoTho;
        if (doTho <= 0.01f) return;

        float song = Mathf.Sin(t * tocDoTho);
        float song2 = Mathf.Sin(t * tocDoTho * 0.63f + 1.1f);

        if (spine != null)
            spine.localRotation = qSpine * Quaternion.Euler(song * doTho * 0.6f, 0f, song2 * doTho * 0.35f);
        if (head != null)
            head.localRotation = qHead * Quaternion.Euler(song2 * doTho * 0.9f, song * doTho * 1.4f, 0f);
        if (hips != null)
            hips.localPosition = pHips + new Vector3(0f, song * doTho * 0.0012f, 0f);
    }

    /// <summary>
    /// Tach mot don danh thanh hai pha: GO SUC roi BUNG RA.
    ///
    /// <paramref name="t"/> chay 0..1 trong ca don. Don an dung o
    /// <see cref="mocAn"/>, nen doan truoc moc do la go suc (<paramref name="go"/>
    /// tang dan len 1) va doan sau la bung ra (<paramref name="bung"/> vot len 1
    /// that nhanh roi ha ve 0).
    ///
    /// CA HAI DEU PHAI VE 0 O CUOI DON. Ban dau toi de <c>go = 1 - bung * 0.85</c>,
    /// ma cuoi don thi <c>bung</c> = 0 nen <c>go</c> hoa ra bang 1 - tu the go
    /// suc dang o muc manh nhat dung luc dong ho het gio, roi ket cung o do.
    /// </summary>
    void ChiaPha(float t, out float go, out float bung)
    {
        if (t < mocAn)
        {
            go = Mathf.SmoothStep(0f, 1f, t / mocAn);
            bung = 0f;
            return;
        }

        float u = (t - mocAn) / (1f - mocAn);
        bung = u < 0.28f ? Mathf.SmoothStep(0f, 1f, u / 0.28f)
                         : Mathf.SmoothStep(1f, 0f, (u - 0.28f) / 0.72f);

        // Nua sau cua don, phan go suc nhat dan ve 0 de ket thuc o tu the chuan
        float nhat = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((u - 0.40f) / 0.60f));
        go = (1f - bung * 0.85f) * nhat;
    }

    /// <summary>Nem phep: ngua nguoi go suc, roi hat manh CA HAI tay ve truoc.</summary>
    void TuTheNemPhep(float t)
    {
        float go, hat;
        ChiaPha(t, out go, out hat);

        if (spine != null)
            spine.localRotation = qSpine * Quaternion.Euler(-14f * go + 26f * hat, 0f, 0f);
        if (head != null)
            head.localRotation = qHead * Quaternion.Euler(-10f * go + 16f * hat, 0f, 0f);

        NangTay(tayPhaiTren, tayPhaiDuoi, qTayPhaiTren, qTayPhaiDuoi, go, hat, 1f);
        NangTay(tayTraiTren, tayTraiDuoi, qTayTraiTren, qTayTraiDuoi, go, hat, -1f);
    }

    void NangTay(Transform tren, Transform duoi, Quaternion qTren, Quaternion qDuoi,
                 float go, float hat, float ben)
    {
        if (tren != null)
            tren.localRotation = qTren * Quaternion.Euler(-58f * go - 34f * hat,
                                                          18f * go * ben,
                                                          -22f * go * ben);
        if (duoi != null)
            duoi.localRotation = qDuoi * Quaternion.Euler(-46f * go + 40f * hat, 0f, 0f);
    }

    /// <summary>
    /// Chem toi: vung tay phai len cao ra sau, xoay nguoi lay da, roi BO MANH
    /// xuong truoc mat. Tay trai (cam khien) dua len che nguoi trong luc do.
    /// </summary>
    void TuTheChemToi(float t)
    {
        float go, chem;
        ChiaPha(t, out go, out chem);

        // Xoay nguoi lay da roi xoay nguoc lai theo cu chem
        if (spine != null)
            spine.localRotation = qSpine * Quaternion.Euler(-10f * go + 16f * chem,
                                                            -26f * go + 30f * chem, 0f);
        if (head != null)
            head.localRotation = qHead * Quaternion.Euler(-6f * go + 14f * chem, 0f, 0f);

        // Tay phai: giơ ra sau len cao (-100 do) roi bo xuong truoc (+72 do)
        if (tayPhaiTren != null)
            tayPhaiTren.localRotation = qTayPhaiTren
                * Quaternion.Euler(-100f * go + 172f * chem, -28f * go, -30f * go + 18f * chem);
        if (tayPhaiDuoi != null)
            tayPhaiDuoi.localRotation = qTayPhaiDuoi
                * Quaternion.Euler(-62f * go + 74f * chem, 0f, 0f);

        // Tay trai dua khien len che
        if (tayTraiTren != null)
            tayTraiTren.localRotation = qTayTraiTren
                * Quaternion.Euler(-34f * go, -16f * go, 24f * go);
        if (tayTraiDuoi != null)
            tayTraiDuoi.localRotation = qTayTraiDuoi * Quaternion.Euler(-40f * go, 0f, 0f);
    }

    /// <summary>Guc nga: do nguoi ve truoc va lun dan xuong dat.</summary>
    void TuTheChet()
    {
        // ThayChet goi boPhat.Stop() nen ca khung xuong dong cung o khung hinh
        // cuoi cua clip di bo. Tra ve tu the chuan truoc roi moi do nguoi xuong,
        // khong thi cai xac nam do voi mot chan van dang buoc.
        TraVeTuTheChuan();

        float k = Mathf.SmoothStep(0f, 1f, nghiengChet);

        if (spine != null) spine.localRotation = qSpine * Quaternion.Euler(58f * k, 0f, 14f * k);
        if (head != null) head.localRotation = qHead * Quaternion.Euler(34f * k, 0f, 0f);
        if (hips != null) hips.localPosition = pHips + new Vector3(0f, -0.42f * k, 0f);

        NangTay(tayPhaiTren, tayPhaiDuoi, qTayPhaiTren, qTayPhaiDuoi, 0f, 0f, 1f);
        NangTay(tayTraiTren, tayTraiDuoi, qTayTraiTren, qTayTraiDuoi, 0f, 0f, -1f);
    }
}
