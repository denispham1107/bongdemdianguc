using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// CAI DAT AM THANH (10/10/2026, nguoi dung): tab "Âm thanh" trong CAI DAT cua sanh, mot thanh "Nhạc nền" 0-100%.
/// Mac dinh <see cref="MacDinhPhanTram"/> 60% (nguoi dung chon) cho nguoi chua tung chinh - KHONG ghi mac dinh xuong kho.
///
/// LUU O DAU: nhu <see cref="CaiDatDoHoa"/> - tren WebGL ghi thang localStorage (CauNoiCaiDat.jslib, PlayerPrefs WebGL ghi khong
/// dong bo), ngoai WebGL la PlayerPrefs. Doi am luong KHONG can tai lai game (khac muc do hoa).
///
/// NGHE THU: dang keo thanh thi <see cref="XemTruoc"/> - <see cref="NhacNen"/> doc <see cref="NhacDangNghe"/> moi khung nen nghe
/// thay ngay; HỦY / dong bang thi <see cref="BoXemTruoc"/> tra ve muc da luu, OK thi <see cref="Luu"/>.
/// </summary>
public static class CaiDatAmThanh
{
    public const string KhoaNhac = "diablo25d.amLuongNhac";
    public const int MacDinhPhanTram = 60;

    static int daDoc = -1;
    static int xemTruoc = -1;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int CD_DocSo(string khoa, int macDinh);
    [DllImport("__Internal")] static extern int CD_GhiSo(string khoa, int giaTri);
#endif

    // Editor tat domain reload: bien tinh giu gia tri lan Play truoc - doc lai tu kho moi lan khoi dong
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong() { daDoc = -1; xemTruoc = -1; }

    /// <summary>Am luong nhac DA LUU, 0-100.</summary>
    public static int NhacPhanTram
    {
        get
        {
            if (daDoc < 0) daDoc = Mathf.Clamp(DocSo(KhoaNhac, MacDinhPhanTram), 0, 100);
            return daDoc;
        }
    }

    /// <summary>Am luong dang nghe (0-1): muc dang keo thu neu bang cai dat dang mo, khong thi muc da luu.</summary>
    public static float NhacDangNghe { get { return (xemTruoc >= 0 ? xemTruoc : NhacPhanTram) / 100f; } }

    public static bool DangXemTruoc { get { return xemTruoc >= 0; } }

    public static void XemTruoc(int phanTram) { xemTruoc = Mathf.Clamp(phanTram, 0, 100); }
    public static void BoXemTruoc() { xemTruoc = -1; }

    /// <summary>Quen muc da doc - lan sau doc lai tu kho (phep thu dung).</summary>
    public static void QuenBoNho() { daDoc = -1; }

    /// <summary>Ghi xuong kho. Tra false neu trinh duyet cam luu (an danh).</summary>
    public static bool Luu(int phanTram)
    {
        daDoc = Mathf.Clamp(phanTram, 0, 100);
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_GhiSo(KhoaNhac, daDoc) == 1; } catch { return false; }
#else
        PlayerPrefs.SetInt(KhoaNhac, daDoc);
        PlayerPrefs.Save();
        return true;
#endif
    }

    static int DocSo(string khoa, int macDinh)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_DocSo(khoa, macDinh); } catch { return macDinh; }
#else
        return PlayerPrefs.GetInt(khoa, macDinh);
#endif
    }
}
