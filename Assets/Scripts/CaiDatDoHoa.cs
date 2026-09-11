using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Ba muc do hoa nguoi choi chon trong CAI DAT &gt; Giao dien.</summary>
public enum MucDoHoa { Cao = 0, TrungBinh = 1, Yeu = 2 }

/// <summary>
/// CAI DAT DO HOA: DO PHAN GIAI + CHAT LUONG HINH.
///
/// Nguoi choi chon mot trong ba muc o sanh (nut CAI DAT, tab "Giao dien"),
/// bam OK thi game TAI LAI va chay theo muc moi.
///
/// Moi muc doi HAI thu:
///   1. Do phan giai ve hinh - so diem anh GPU phai to moi khung. Tren WebGL
///      thu nay CHI dat duoc truoc khi Unity khoi dong (trang bao quanh game
///      dat config.devicePixelRatio), nen moi phai tai lai trang. Xem
///      Assets/WebGLTemplates/Diablo25D/index.html - bang he so o do phai
///      KHOP voi <see cref="heSoPhanGiai"/>.
///   2. Muc chat luong cua Unity (bong do, khu rang cua, so den tinh tung diem
///      anh) - dat o day, TRUOC khi nap scene dau tien.
///
/// | Muc         | Phan giai | Muc Unity | Bong                  |
/// |-------------|-----------|-----------|-----------------------|
/// | Cao         | 100 %     | High      | mem, 2 tang, xa 60 m  |
/// | Trung binh  | 75 %      | Medium    | cung, 1 tang, xa 40 m |
/// | Yeu         | 50 %      | Low       | tat                   |
///
/// "Cao" chinh la game nhu truoc khi co cai dat nay: WebGL mac dinh chay muc
/// High, va GameBootstrap dat bong xa 60 m.
///
/// LUU O DAU: tren WebGL la localStorage cua trinh duyet (xem
/// CauNoiCaiDat.jslib - PlayerPrefs ghi khong dong bo, tai lai ngay thi mat).
/// Ngoai WebGL (Editor) la PlayerPrefs.
/// </summary>
public static class CaiDatDoHoa
{
    /// <summary>Khoa luu - index.html doc DUNG khoa nay, doi thi doi ca hai noi.</summary>
    public const string Khoa = "diablo25d.mucDoHoa";

    /// <summary>Ten hien len giao dien.</summary>
    public static readonly string[] Ten = { "Cao", "Trung bình", "Yếu" };

    /// <summary>Ten muc chat luong trong Project Settings &gt; Quality. Tra theo
    /// TEN chu khong theo so thu tu: them bot mot muc la so thu tu truot het.</summary>
    public static readonly string[] TenMucUnity = { "High", "Medium", "Low" };

    /// <summary>Nhan voi devicePixelRatio cua trinh duyet. PHAI khop index.html.</summary>
    public static readonly float[] heSoPhanGiai = { 1f, 0.75f, 0.5f };

    // Muc Low tat bong nen con so cua no khong dung den
    static readonly float[] khoangBong = { 60f, 40f, 20f };

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
            if (daDoc < 0) daDoc = Mathf.Clamp(DocTuKho(), 0, 2);
            return (MucDoHoa)daDoc;
        }
    }

    /// <summary>Khoang bong cho muc dang dung - GameBootstrap dat khi dung man.</summary>
    public static float KhoangBong { get { return khoangBong[(int)Muc]; } }

    public static float HeSoPhanGiai { get { return heSoPhanGiai[(int)Muc]; } }

    static int DocTuKho()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_DocSo(Khoa, 0); } catch { return 0; }
#else
        return PlayerPrefs.GetInt(Khoa, 0);
#endif
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
    }

    /// <summary>Dat muc chat luong cua Unity theo muc dang dung.</summary>
    public static void ApDung()
    {
        var m = Muc;
        int lv = TimMucUnity(TenMucUnity[(int)m]);
        if (lv >= 0 && lv != QualitySettings.GetQualityLevel())
            QualitySettings.SetQualityLevel(lv, true);

        // GIU NHIP KHUNG HINH NHU CU. Muc High va Medium bat vSync, muc Low thi
        // tat - ma tat vSync thi tren WebGL Application.targetFrameRate = 120
        // (GameBootstrap) co hieu luc, Unity chuyen tu requestAnimationFrame
        // sang setTimeout va chay voi 120 khung. May yeu chon "Yeu" de nhe di
        // lai bi ep ve NHIEU khung hon - nguoc han y muon.
        QualitySettings.vSyncCount = 1;

        Debug.Log("[CaiDat] muc do hoa = " + Ten[(int)m]
                  + ", muc Unity = " + QualitySettings.names[QualitySettings.GetQualityLevel()]
                  + ", he so phan giai = " + HeSoPhanGiai
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
    /// Tai lai game de ap dung. Tren WebGL la tai lai ca trang - cach duy nhat
    /// de doi do phan giai. Nguoi choi khong phai dang nhap lai: phien dang
    /// nhap nam trong kho cua trinh duyet tu luc dang nhap.
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
