using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Bon muc do hoa nguoi choi chon trong CAI DAT &gt; Giao dien.</summary>
public enum MucDoHoa { Cao = 0, TrungBinh = 1, Yeu = 2, RatYeu = 3 }

/// <summary>
/// CAI DAT DO HOA: DO PHAN GIAI CANH 3D + CHAT LUONG HINH.
///
/// Nguoi choi chon mot trong bon muc o sanh (nut CAI DAT, tab "Giao dien"),
/// bam OK thi game TAI LAI va chay theo muc moi.
///
/// Moi muc doi HAI thu - va CHI doi canh trong game / nen man menu:
///   1. Do phan giai CANH 3D: camera chinh ve vao anh dem nho roi phong len
///      (<see cref="KetXuatThuNho"/>). KHUNG GAME luon o do phan giai day du,
///      nen chu, khung, nut OnGUI sac net y het muc Cao. (Truoc 12/09/2026 trang
///      web ha devicePixelRatio cua CA khung game - giao dien mo nhoe theo.)
///   2. Muc chat luong cua Unity (bong do, khu rang cua, so den tinh tung diem
///      anh, chi tiet xa) - dat o day, TRUOC khi nap scene dau tien.
///
/// | Muc         | Canh 3D | Muc Unity | Bong                  |
/// |-------------|---------|-----------|-----------------------|
/// | Cao         | 100 %   | High      | mem, 2 tang, xa 60 m  |
/// | Trung binh  | 75 %    | Medium    | cung, 1 tang, xa 40 m |
/// | Yeu         | 62 %    | Low (+)   | cung, phan giai thap, xa 25 m; chi tiet xa 0,55 |
/// | Rat yeu     | 50 %    | Low       | tat                   |
///
/// "Rat yeu" chinh la muc "Yeu" cu. "Yeu" moi (12/09/2026) nam giua.
///
/// LUU O DAU: tren WebGL la localStorage cua trinh duyet (xem
/// CauNoiCaiDat.jslib - PlayerPrefs ghi khong dong bo, tai lai ngay thi mat).
/// Ngoai WebGL (Editor) la PlayerPrefs. Khoa MOI "diablo25d.mucDoHoa2": khoa cu
/// luu 0/1/2 voi 2 = Yeu cu (50%) - doc bang nghia moi thi nguoi dang chon
/// 50% bi day len 62%. Lan dau doc thi chuyen: 0 -> Cao, 1 -> Trung binh,
/// 2 -> Rat yeu.
/// </summary>
public static class CaiDatDoHoa
{
    public const string Khoa = "diablo25d.mucDoHoa2";
    /// <summary>Khoa cu (ba muc) - chi doc mot lan de chuyen sang khoa moi.</summary>
    public const string KhoaCu = "diablo25d.mucDoHoa";

    public const int SoMuc = 4;

    /// <summary>Ten hien len giao dien, dung thu tu MucDoHoa.</summary>
    public static readonly string[] Ten = { "Cao", "Trung bình", "Yếu", "Rất yếu" };

    /// <summary>Ten muc chat luong trong Project Settings &gt; Quality. Tra theo
    /// TEN chu khong theo so thu tu: them bot mot muc la so thu tu truot het.</summary>
    public static readonly string[] TenMucUnity = { "High", "Medium", "Low", "Low" };

    /// <summary>Ti le moi chieu cua canh 3D so voi man hinh.</summary>
    public static readonly float[] heSoPhanGiai = { 1f, 0.75f, 0.62f, 0.5f };

    // Muc Rat yeu tat bong nen con so cua no khong dung den
    static readonly float[] khoangBong = { 60f, 40f, 25f, 20f };

    // Bon gia tri moi muc tu dat (xem ApDung). Cao/Trung binh/Rat yeu = bo goc
    // cua High/Medium/Low trong ProjectSettings; Yeu = Low + bong cung, chi tiet xa 0,55
    public static readonly ShadowQuality[] BongTheoMuc =
        { ShadowQuality.All, ShadowQuality.HardOnly, ShadowQuality.HardOnly, ShadowQuality.Disable };
    static readonly ShadowResolution[] PhanGiaiBongTheoMuc =
        { ShadowResolution.Medium, ShadowResolution.Low, ShadowResolution.Low, ShadowResolution.Low };
    static readonly int[] TangBongTheoMuc = { 2, 1, 1, 1 };
    public static readonly float[] ChiTietXaTheoMuc = { 1f, 0.7f, 0.55f, 0.4f };

    static int daDoc = -1;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int CD_DocSo(string khoa, int macDinh);
    [DllImport("__Internal")] static extern int CD_GhiSo(string khoa, int giaTri);
    [DllImport("__Internal")] static extern void CD_NapLai();
#endif

    /// <summary>Muc dang dung.</summary>
    public static MucDoHoa Muc
    {
        get
        {
            if (daDoc < 0) daDoc = Mathf.Clamp(DocTuKho(), 0, SoMuc - 1);
            return (MucDoHoa)daDoc;
        }
    }

    /// <summary>Khoang bong cho muc dang dung - GameBootstrap dat khi dung man.</summary>
    public static float KhoangBong { get { return khoangBong[(int)Muc]; } }

    public static float HeSoPhanGiai { get { return heSoPhanGiai[(int)Muc]; } }

    static int DocSo(string khoa, int macDinh)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_DocSo(khoa, macDinh); } catch { return macDinh; }
#else
        return PlayerPrefs.GetInt(khoa, macDinh);
#endif
    }

    /// <summary>Quen muc da doc - lan sau doc lai tu kho (phep thu menu 48 dung).</summary>
    public static void QuenBoNho() { daDoc = -1; }

    /// <summary>Doi so cua khoa cu (0 Cao, 1 Trung binh, 2 Yeu cu = 50%) sang muc moi.</summary>
    public static int ChuyenMucCu(int cu)
    {
        return cu == 1 ? (int)MucDoHoa.TrungBinh : cu == 2 ? (int)MucDoHoa.RatYeu : (int)MucDoHoa.Cao;
    }

    static int DocTuKho()
    {
        int moi = DocSo(Khoa, -1);
        if (moi >= 0) return moi;
        int cu = DocSo(KhoaCu, -1);
        if (cu < 0) return 0;
        int m = ChuyenMucCu(cu);
        Luu((MucDoHoa)m);                       // lan sau doc thang khoa moi
        return m;
    }

    /// <summary>Ghi lua chon xuong kho. Tra false neu trinh duyet cam luu.</summary>
    public static bool Luu(MucDoHoa m)
    {
        daDoc = (int)m;
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_GhiSo(Khoa, (int)m) == 1; } catch { return false; }
#else
        PlayerPrefs.SetInt(Khoa, (int)m);
        PlayerPrefs.Save();
        return true;
#endif
    }

    // Chay mot lan khi game khoi dong, TRUOC scene dau tien - de ngay khung
    // hinh dau da ve dung muc. Doc lai tu kho: voi Editor tat domain reload,
    // bien tinh con giu gia tri cua lan Play truoc.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong()
    {
        daDoc = -1;
        ApDung();
        TheoDoiCamera.DamBao();
    }

    /// <summary>Dat muc chat luong cua Unity theo muc dang dung.</summary>
    public static void ApDung()
    {
        var m = Muc;
        int lv = TimMucUnity(TenMucUnity[(int)m]);
        if (lv >= 0) QualitySettings.SetQualityLevel(lv, true);

        // MOI MUC TU DAT DU BON GIA TRI NAY, khong dua vao bo thong so cua muc
        // Unity. CAI BAY DA VAP: gan QualitySettings.shadows... la GHI THANG vao
        // bo thong so cua muc dang dung (Low) - "Yeu" bat bong cung tren Low roi
        // "Rat yeu" dat lai Low van nhan bong cung (menu 48: 51 vat do bong).
        // Gia tri = dung bo goc cua muc Unity tuong ung; "Yeu" nam giua.
        int i = (int)m;
        QualitySettings.shadows = BongTheoMuc[i];
        QualitySettings.shadowResolution = PhanGiaiBongTheoMuc[i];
        QualitySettings.shadowCascades = TangBongTheoMuc[i];
        QualitySettings.lodBias = ChiTietXaTheoMuc[i];

        // GIU NHIP KHUNG HINH NHU CU. Muc High va Medium bat vSync, muc Low thi
        // tat - ma tat vSync thi tren WebGL Application.targetFrameRate = 120
        // (GameBootstrap) co hieu luc, Unity chuyen tu requestAnimationFrame
        // sang setTimeout va chay voi 120 khung. May yeu chon muc thap de nhe di
        // lai bi ep ve NHIEU khung hon - nguoc han y muon.
        QualitySettings.vSyncCount = 1;

        Debug.Log("[CaiDat] muc do hoa = " + Ten[(int)m]
                  + ", muc Unity = " + QualitySettings.names[QualitySettings.GetQualityLevel()]
                  + ", canh 3D = " + HeSoPhanGiai
                  + ", man hinh = " + Screen.width + "x" + Screen.height);
    }

    static int TimMucUnity(string ten)
    {
        var ds = QualitySettings.names;
        for (int i = 0; i < ds.Length; i++)
            if (ds[i] == ten) return i;
        return -1;
    }

    /// <summary>
    /// Tai lai game de ap dung. Tren WebGL la tai lai ca trang. Nguoi choi khong
    /// phai dang nhap lai: phien dang nhap nam trong kho cua trinh duyet.
    /// Ngoai WebGL (Editor) thi dat lai muc chat luong va nap lai scene.
    /// </summary>
    public static void NapLaiGame()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CD_NapLai();
#else
        ApDung();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }
}
