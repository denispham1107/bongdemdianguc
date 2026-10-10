using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// CAI DAT AM THANH: tab "Âm thanh" trong CAI DAT cua sanh, HAI thanh 0-100%:
///   - "Nhạc nền" (10/10/2026) - <see cref="NhacNen"/>; mac dinh <see cref="MacDinhPhanTram"/> 60%.
///   - "Hiệu ứng" (11/10/2026, nguoi dung) - MOI tieng dong trong game (tieng bam nut, sau nay tieng ky nang...) qua
///     <see cref="TiengDong"/>; mac dinh <see cref="MacDinhHieuUng"/> 80% (nguoi dung chon).
/// Nguoi chua tung chinh dung mac dinh - KHONG ghi mac dinh xuong kho.
///
/// LUU O DAU: nhu <see cref="CaiDatDoHoa"/> - tren WebGL ghi thang localStorage (CauNoiCaiDat.jslib, PlayerPrefs WebGL ghi khong
/// dong bo), ngoai WebGL la PlayerPrefs. Doi am luong KHONG can tai lai game (khac muc do hoa).
///
/// NGHE THU: dang keo thanh thi XemTruoc - nguon am doc muc "DangNghe" moi lan phat nen nghe thay ngay; HỦY / dong bang thi
/// BoXemTruoc tra ve muc da luu, OK thi Luu.
/// </summary>
public static class CaiDatAmThanh
{
    public const string KhoaNhac = "diablo25d.amLuongNhac";
    public const int MacDinhPhanTram = 60;
    public const string KhoaHieuUng = "diablo25d.amLuongHieuUng";
    public const int MacDinhHieuUng = 80;

    // [0] nhac nen, [1] hieu ung
    static readonly string[] khoa = { KhoaNhac, KhoaHieuUng };
    static readonly int[] macDinh = { MacDinhPhanTram, MacDinhHieuUng };
    static readonly int[] daDoc = { -1, -1 };
    static readonly int[] xemTruoc = { -1, -1 };

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int CD_DocSo(string khoa, int macDinh);
    [DllImport("__Internal")] static extern int CD_GhiSo(string khoa, int giaTri);
#endif

    // Editor tat domain reload: bien tinh giu gia tri lan Play truoc - doc lai tu kho moi lan khoi dong
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong() { for (int i = 0; i < 2; i++) { daDoc[i] = -1; xemTruoc[i] = -1; } }

    static int DaLuu(int i)
    {
        if (daDoc[i] < 0) daDoc[i] = Mathf.Clamp(DocSo(khoa[i], macDinh[i]), 0, 100);
        return daDoc[i];
    }

    static float DangNghe(int i) { return (xemTruoc[i] >= 0 ? xemTruoc[i] : DaLuu(i)) / 100f; }

    // ---------------- NHAC NEN ----------------
    /// <summary>Am luong nhac DA LUU, 0-100.</summary>
    public static int NhacPhanTram { get { return DaLuu(0); } }
    /// <summary>Am luong nhac dang nghe (0-1): muc dang keo thu neu bang cai dat dang mo, khong thi muc da luu.</summary>
    public static float NhacDangNghe { get { return DangNghe(0); } }
    public static void XemTruoc(int phanTram) { xemTruoc[0] = Mathf.Clamp(phanTram, 0, 100); }
    public static bool Luu(int phanTram) { return LuuSo(0, phanTram); }

    // ---------------- HIEU UNG ----------------
    /// <summary>Am luong hieu ung DA LUU, 0-100.</summary>
    public static int HieuUngPhanTram { get { return DaLuu(1); } }
    /// <summary>Am luong hieu ung dang nghe (0-1) - MOI tieng dong trong game nhan so nay (<see cref="TiengDong"/>).</summary>
    public static float HieuUngDangNghe { get { return DangNghe(1); } }
    public static void XemTruocHieuUng(int phanTram) { xemTruoc[1] = Mathf.Clamp(phanTram, 0, 100); }
    public static bool LuuHieuUng(int phanTram) { return LuuSo(1, phanTram); }

    // ---------------- CHUNG ----------------
    public static bool DangXemTruoc { get { return xemTruoc[0] >= 0 || xemTruoc[1] >= 0; } }
    /// <summary>Bo nghe thu CA HAI thanh - tra ve muc da luu.</summary>
    public static void BoXemTruoc() { xemTruoc[0] = -1; xemTruoc[1] = -1; }

    /// <summary>Quen muc da doc - lan sau doc lai tu kho (phep thu dung).</summary>
    public static void QuenBoNho() { daDoc[0] = -1; daDoc[1] = -1; }

    /// <summary>Ghi xuong kho. Tra false neu trinh duyet cam luu (an danh).</summary>
    static bool LuuSo(int i, int phanTram)
    {
        daDoc[i] = Mathf.Clamp(phanTram, 0, 100);
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_GhiSo(khoa[i], daDoc[i]) == 1; } catch { return false; }
#else
        PlayerPrefs.SetInt(khoa[i], daDoc[i]);
        PlayerPrefs.Save();
        return true;
#endif
    }

    static int DocSo(string k, int md)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { return CD_DocSo(k, md); } catch { return md; }
#else
        return PlayerPrefs.GetInt(k, md);
#endif
    }
}
