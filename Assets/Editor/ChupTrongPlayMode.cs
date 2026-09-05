using UnityEditor;
using UnityEngine;

/// <summary>
/// Dat lich chup mot anh NGAY KHI Play mode bat dau.
///
/// Vi sao phai qua EditorPrefs: bam Play lam Unity NAP LAI toan bo domain, moi
/// bien tinh deu ve gia tri dau. Ghi vao EditorPrefs thi lich chup song sot qua
/// lan nap lai do.
///
/// Cach dung, chay lam HAI lenh rieng:
///   1. ChupTrongPlayMode.DatLich(duong, 2f);  EditorApplication.EnterPlaymode();
///   2. (cho vai giay, Unity tu thoat Play)    doc file anh
/// Khong gop mot lenh duoc: Unity dang Play thi khong chay them lenh nao.
/// </summary>
[InitializeOnLoad]
public static class ChupTrongPlayMode
{
    const string KhoaBat = "ChupTrongPlayMode.Bat";
    const string KhoaDuong = "ChupTrongPlayMode.Duong";
    const string KhoaCho = "ChupTrongPlayMode.Cho";

    static ChupTrongPlayMode()
    {
        EditorApplication.playModeStateChanged -= OnDoiTrangThai;
        EditorApplication.playModeStateChanged += OnDoiTrangThai;
    }

    /// <summary>Hen: lan vao Play mode ke tiep se chup mot anh roi thoat.</summary>
    public static void DatLich(string duong, float choGiay)
    {
        EditorPrefs.SetBool(KhoaBat, true);
        EditorPrefs.SetString(KhoaDuong, duong);
        EditorPrefs.SetFloat(KhoaCho, choGiay);
    }

    static void OnDoiTrangThai(PlayModeStateChange trangThai)
    {
        if (trangThai != PlayModeStateChange.EnteredPlayMode) return;
        if (!EditorPrefs.GetBool(KhoaBat, false)) return;

        // Xoa lich NGAY, truoc khi lam gi khac: khong thi lan sau nguoi dung tu
        // bam Play de choi cung bi thoat ra giua chung.
        EditorPrefs.SetBool(KhoaBat, false);

        var go = new GameObject("ChupRoiThoat");
        Object.DontDestroyOnLoad(go);
        var c = go.AddComponent<ChupRoiThoat>();
        c.duong = EditorPrefs.GetString(KhoaDuong);
        c.choGiay = EditorPrefs.GetFloat(KhoaCho, 2f);
    }
}
