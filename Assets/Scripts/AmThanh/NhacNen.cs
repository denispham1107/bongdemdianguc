using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// NHAC NEN (10/10/2026, nguoi dung). Hai ban nhac tong hop bang numpy (CongCu/AmThanh/nhac_sanh.py, nhac_tran.py), WAV
/// 44,1 kHz STEREO (quy tac nguoi dung: MOI am thanh phai stereo, khong Force To Mono), lap lien mach:
///   - NhacSanh (128 s): man dang nhap + sanh - cung scene MainMenu nen phat LIEN, khong ngat khi dang nhap xong.
///   - NhacTranAct2 (192 s): trong tran.
///
/// Chuyen canh (nguoi dung chon):
///   - VAO TRAN: nhac sanh NHO DAN ve 0 trong <see cref="GiayNhoDan"/> - bat dau ngay luc roi sanh (<see cref="BatDauRoiSanh"/>,
///     ManSanh / MainMenuUI goi truoc LoadScene). LoadScene DONG BO chan vong lap trong luc nap man nen phan con lai nho tiep
///     sau khi nap xong. Nhac Act2 TO DAN tu 0 len <see cref="HeSoTrongTran"/> (70%) muc da chinh trong <see cref="GiayToDan"/>.
///   - VE SANH: nhac tran nho dan, nhac sanh phat lai TU DAU, to dan.
///   - Scene khac (canh thu cua menu 3...): ca hai nho dan ve 0.
///
/// Am luong: thanh "Nhạc nền" trong CAI DAT &gt; Âm thanh (<see cref="CaiDatAmThanh"/>, mac dinh 60%) - doc moi khung nen keo
/// thanh la nghe thu ngay.
///
/// WEB: trinh duyet chan tu phat am thanh truoc cu cham / bam dau tien. Unity WebGL tu mo lai AudioContext o cu tuong tac dau
/// tien - nhac bat dau tu do (khong can code rieng; menu 121 kiem tren trang that).
///
/// Mot vat the DontDestroyOnLoad, hai AudioSource 2D. Nguon nao da im han (0) thi DUNG + tha clip (Resources.UnloadAsset) - tren
/// WebGL moi clip giai ma ra nam trong bo nho trinh duyet, khong giu ca hai cung luc.
/// Dang chay phep thu (co <see cref="ChayThuMang"/>) thi IM LANG - tru <see cref="BatTrongPhepThu"/> (menu 121): phep thu chay
/// rat nhieu lan, khong de nguoi dung nghe nhac moi lan.
/// </summary>
public class NhacNen : MonoBehaviour
{
    public const string DuongNhacSanh = "AmThanh/Nhac/NhacSanh";
    public const string DuongNhacTran = "AmThanh/Nhac/NhacTranAct2";

    /// <summary>Trong tran nhac = 70% muc da chinh (nguoi dung chon) - chua cho tieng danh nhau.</summary>
    public const float HeSoTrongTran = 0.7f;
    /// <summary>Thoi gian nhac dang phat nho dan ve 0 khi roi canh.</summary>
    public const float GiayNhoDan = 2f;
    /// <summary>Thoi gian nhac tran to dan len khi vao tran.</summary>
    public const float GiayToDan = 3f;
    /// <summary>Thoi gian nhac sanh to dan len (luc mo game / ve sanh).</summary>
    public const float GiayToDanSanh = 1.5f;

    public const string TenCanhSanh = "MainMenu";
    public const string TenCanhTran = "Act2";

    public static NhacNen Ban { get; private set; }

    /// <summary>Phep thu (menu 121) bat de nghe / do nhac khi co ChayThuMang.</summary>
    public static bool BatTrongPhepThu;

    /// <summary>Mot ban nhac: nguon phat + he so mo dan hien tai / dich.</summary>
    public class BanNhac
    {
        public string duong;
        public AudioSource nguon;
        public AudioClip giu;       // giu tham chieu clip trong code (xem PhatTuDau)
        public float muc, dich, giayDoi;
        public float heSo = 1f;     // nhan them (trong tran 0,7)
        // Phai CO CLIP: AudioSource vua tao tren Unity 6.5 bao isPlaying = true khi chua co clip nao (menu 121: nhac sanh khong
        // phat luc mo game vi tuong "dang keu")
        public bool LaDangKeu { get { return nguon != null && nguon.clip != null && nguon.isPlaying; } }
    }

    public readonly BanNhac sanh = new BanNhac { duong = DuongNhacSanh, heSo = 1f };
    public readonly BanNhac tran = new BanNhac { duong = DuongNhacTran, heSo = HeSoTrongTran };

    bool coPhepThu;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong() { DamBao(); }

    /// <summary>Tao vat the nhac nen neu chua co (Editor tat domain reload: Ban cu da bi huy luc thoat Play - Unity null).</summary>
    public static NhacNen DamBao()
    {
        if (Ban != null) return Ban;
        var go = new GameObject("NhacNen");
        DontDestroyOnLoad(go);
        Ban = go.AddComponent<NhacNen>();
        return Ban;
    }

    void Awake()
    {
        if (Ban != null && Ban != this) { Destroy(gameObject); return; }
        Ban = this;
        sanh.nguon = TaoNguon("Sanh");
        tran.nguon = TaoNguon("Tran");
        SceneManager.sceneLoaded += KhiNapCanh;
        // KHONG chon nhac o day (BeforeSceneLoad - scene dau tien chua nap). sceneLoaded KHONG ban cho scene dau tien khi vao
        // Play trong Editor (menu 121: khong chon gi ca) - Update tu nhan ra scene dang mo da doi, ke ca scene dau tien
        // (xem canhDaChon).
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= KhiNapCanh;
        if (Ban == this) Ban = null;
    }

    AudioSource TaoNguon(string ten)
    {
        var a = gameObject.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.loop = true;
        a.spatialBlend = 0f;          // 2D - giu nguyen stereo cua ban nhac
        a.volume = 0f;
        a.priority = 0;               // nhac khong bao gio bi tieng khac chiem kenh
        a.bypassReverbZones = true;
        a.ignoreListenerPause = true;
        return a;
    }

    void KhiNapCanh(Scene c, LoadSceneMode kieu)
    {
        if (kieu != LoadSceneMode.Single) return;
        ChonTheoCanh(c.name);
    }

    string canhDaChon;

    void ChonTheoCanh(string ten)
    {
        canhDaChon = ten;
        coPhepThu = FindAnyObjectByType<ChayThuMang>() != null;
        if (ten == TenCanhSanh)
        {
            DatDich(tran, 0f, GiayNhoDan);
            // Dang keu (dang nhap -> sanh, nap lai sanh) thi giu nguyen, khong phat lai tu dau
            if (!sanh.LaDangKeu) PhatTuDau(sanh);
            DatDich(sanh, 1f, GiayToDanSanh);
        }
        else if (ten == TenCanhTran)
        {
            DatDich(sanh, 0f, GiayNhoDan);
            if (!tran.LaDangKeu) PhatTuDau(tran);    // choi lai cung man (phim R) thi nhac chay tiep
            DatDich(tran, 1f, GiayToDan);
        }
        else
        {
            DatDich(sanh, 0f, GiayNhoDan);
            DatDich(tran, 0f, GiayNhoDan);
        }
    }

    /// <summary>Roi sanh vao tran: bat dau cho nhac sanh nho dan NGAY (truoc LoadScene).</summary>
    public static void BatDauRoiSanh()
    {
        if (Ban == null) return;
        Ban.DatDich(Ban.sanh, 0f, GiayNhoDan);
    }

    void DatDich(BanNhac b, float dich, float giay)
    {
        b.dich = dich;
        b.giayDoi = Mathf.Max(0.01f, giay);
    }

    void PhatTuDau(BanNhac b)
    {
        // Giu clip o truong rieng (tha cung luc voi nguon - xem CapNhat)
        if (b.giu == null) b.giu = Resources.Load<AudioClip>(b.duong);
        if (b.nguon.clip != b.giu) b.nguon.clip = b.giu;
        if (b.nguon.clip == null) { Debug.LogWarning("[NhacNen] thieu " + b.duong); return; }
        b.muc = 0f;
        b.nguon.volume = 0f;
        b.nguon.time = 0f;
        b.nguon.Play();
        // Clip "nap trong nen" (loadInBackground): vai khung dau nguon chua keu - cho 2 giay moi xet tu phuc hoi
        henPhucHoi = Time.unscaledTime + 2f;
    }

    float henKiemPhepThu;

    void Update()
    {
        // Scene dang mo doi (ke ca scene dau tien) ma chua chon nhac -> chon. Nap lai CUNG scene (choi lai, tai lai sanh) thi
        // sceneLoaded lo; ten khong doi nen o day khong lam gi.
        var canh = SceneManager.GetActiveScene();
        if (canh.isLoaded && canh.name != canhDaChon) ChonTheoCanh(canh.name);

        // Phep thu gan ChayThuMang SAU khi scene da nap - kiem lai moi nua giay
        if (Time.unscaledTime >= henKiemPhepThu)
        {
            henKiemPhepThu = Time.unscaledTime + 0.5f;
            coPhepThu = FindAnyObjectByType<ChayThuMang>() != null;
        }
        // Thoi gian THAT: man ket tran / dung game co the dat timeScale 0, nhac van phai mo dan
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        CapNhat(sanh, dt);
        CapNhat(tran, dt);

        // TU PHUC HOI: dang can keu ma nguon im / mat clip (clip bi don, nguon bi dung ngoai y) -> nap + phat lai, toi da
        // moi giay mot lan (de khong phat lai lien tuc neu nen tang bao "khong keu" trong luc cho cu cham dau tien tren web)
        if (Time.unscaledTime >= henPhucHoi)
        {
            henPhucHoi = Time.unscaledTime + 1f;
            PhucHoi(sanh);
            PhucHoi(tran);
        }
    }

    float henPhucHoi;

    void PhucHoi(BanNhac b)
    {
        if (b.nguon == null || b.dich <= 0f) return;
        if (b.nguon.clip != null && (b.nguon.isPlaying || b.nguon.clip.loadState == AudioDataLoadState.Loading)) return;
        LyDoPhucHoi = string.Format("{0} luc {1:F2} s: clip {2}, keu {3}, nap {4}", b.duong, Time.unscaledTime,
            b.nguon.clip != null ? b.nguon.clip.name : "null", b.nguon.isPlaying,
            b.nguon.clip != null ? b.nguon.clip.loadState.ToString() : "-");
        float muc = b.muc;
        PhatTuDau(b);
        b.muc = muc;                 // giu do to dang mo dan, khong bat dau lai tu 0
        SoLanPhucHoi++;
    }

    /// <summary>So lan phai tu phuc hoi (phep thu doc - binh thuong 0).</summary>
    public int SoLanPhucHoi;
    /// <summary>Trang thai nguon luc tu phuc hoi gan nhat (phep thu doc).</summary>
    public string LyDoPhucHoi = "";

    void CapNhat(BanNhac b, float dt)
    {
        if (b.nguon == null) return;
        if (b.muc != b.dich) b.muc = Mathf.MoveTowards(b.muc, b.dich, dt / b.giayDoi);
        float tat = coPhepThu && !BatTrongPhepThu ? 0f : 1f;
        b.nguon.volume = b.muc * b.heSo * CaiDatAmThanh.NhacDangNghe * tat;

        // Im han va khong con muon keu -> dung, tha clip khoi bo nho
        if (b.muc <= 0f && b.dich <= 0f && b.nguon.clip != null)
        {
            b.nguon.Stop();
            var c = b.nguon.clip;
            b.nguon.clip = null;
            b.giu = null;
            Resources.UnloadAsset(c);
        }
    }

    /// <summary>Am luong dang dat cho nguon (phep thu doc).</summary>
    public float AmLuong(BanNhac b) { return b.nguon != null ? b.nguon.volume : 0f; }
}
