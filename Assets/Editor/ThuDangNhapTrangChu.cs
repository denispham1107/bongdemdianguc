using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 155 - DANG NHAP CHI O TRANG CHU (01/11/2026, nguoi dung: "bo chuc nang Dang nhap, Tao tai khoan - chi tao tai khoan, dang
/// nhap va an vao choi truc tiep tu trang chu"; chon bo tren web, giu trong Editor; dang xuat -> ve trang chu).
///
/// Vao Play MainMenu, gia lap BAN WEB: ManDangNhap.ChiDangNhapOTrangChu = true, ManDangNhap.ChuyenTrang ghi lai dia chi thay vi chuyen
/// trang that. Firebase THAT, tai khoan chay thu A (chay-thu-mang.txt qua ThongTinChayThu). Phien dang luu (PlayerPrefs) cat roi tra lai.
///   A. Khong co phien: chuyen "/?can-dang-nhap", 0 khung dang nhap duoc ve.
///   B. Phien gia (token hong): khoi phuc hong -> "/?can-dang-nhap=het-han", phien bi xoa.
///   C. Phien THAT con han: tu khoi phuc -> vao sanh, KHONG chuyen trang, 0 khung dang nhap.
///   D. Bam DANG XUAT o sanh (ManSanh.DangXuat): het dang nhap, phien bi xoa, chuyen "/".
///   E. DOI CHUNG ban Editor (ChiDangNhapOTrangChu = false), khong co phien: VE khung dang nhap, khong chuyen trang.
/// PlayTestShots/dang_nhap_trang_chu.txt
/// </summary>
public static class ThuDangNhapTrangChu
{
    const string Ra = "PlayTestShots/dang_nhap_trang_chu.txt";
    const string KhoaPhien = "diablo25d_refresh";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;
    static bool daBatDau, truocBat, coPhienGoc;
    static string phienGoc, canhGoc;
    static EnterPlayModeOptions truocOpt;
    static readonly List<string> daChuyen = new List<string>();

    [MenuItem("Diablo 2.5D/155 Dang nhap chi o trang chu (ban web bo man dang nhap)", false, 229)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuDangNhapTrangChu] dang Play hoac canh chua luu"); return; }
        string l;
        if (!ThongTinChayThu.Doc(out l)) { Debug.LogError("[ThuDangNhapTrangChu] " + l); return; }
        nk.Length = 0; loi = 0; daChuyen.Clear(); daBatDau = false;
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien); phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();
        canhGoc = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        ManDangNhap.ChiDangNhapOTrangChu = true;
        ManDangNhap.ChuyenTrang = d => daChuyen.Add(d);
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuDangNhapTrangChu");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    /// <summary>Xoa moi ManDangNhap dang co, dat lai dem, gan mot ManDangNhap moi (Start chay o khung sau).</summary>
    static ManDangNhap DungMoi(GameObject cho)
    {
        foreach (var m in Object.FindObjectsByType<ManDangNhap>(FindObjectsInactive.Include)) Object.Destroy(m);
        ManDangNhap.DatLaiChoPhepThu();
        daChuyen.Clear();
        return cho.AddComponent<ManDangNhap>();
    }

    static IEnumerator Cho(System.Func<bool> xong, float giay)
    {
        for (float t = 0f; t < giay && !xong(); t += Time.unscaledDeltaTime) yield return null;
    }

    static IEnumerator KichBan()
    {
        yield return new WaitForSeconds(0.5f);
        var tam = GameObject.Find("TAM_ThuDangNhapTrangChu");

        // ---- A. Khong co phien (ManDangNhap that cua MainMenuUI) ----
        nk.AppendLine("== A Khong co phien");
        yield return Cho(() => daChuyen.Count > 0, 3f);
        Kiem(daChuyen.Count == 1 && daChuyen[0] == ManDangNhap.DuongCanDangNhap, "A chuyen: " + string.Join(" | ", daChuyen.ToArray()) + " (phai " + ManDangNhap.DuongCanDangNhap + ")");
        Kiem(ManDangNhap.SoLanVeKhungDangNhap == 0, "A khung dang nhap ve " + ManDangNhap.SoLanVeKhungDangNhap + " lan (phai 0)");

        // ---- B. Phien gia ----
        nk.AppendLine("== B Phien gia (token hong)");
        PlayerPrefs.SetString(KhoaPhien, "TOKEN_GIA_KHONG_HOP_LE_155"); PlayerPrefs.Save();
        DungMoi(tam);
        yield return Cho(() => daChuyen.Count > 0, 12f);
        Kiem(daChuyen.Count == 1 && daChuyen[0] == ManDangNhap.DuongHetHan, "B chuyen: " + string.Join(" | ", daChuyen.ToArray()) + " (phai " + ManDangNhap.DuongHetHan + ")");
        Kiem(!PlayerPrefs.HasKey(KhoaPhien) || PlayerPrefs.GetString(KhoaPhien) == "", "B phien hong da bi xoa");
        Kiem(ManDangNhap.SoLanVeKhungDangNhap == 0, "B khung dang nhap ve " + ManDangNhap.SoLanVeKhungDangNhap + " lan (phai 0)");

        // ---- C. Phien that con han ----
        nk.AppendLine("== C Phien THAT con han (tai khoan chay thu A)");
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailA, ThongTinChayThu.MatKhau, (o, er) => { ok = o; e = er; });
        string tokenThat = PlayerPrefs.GetString(KhoaPhien, "");
        Kiem(ok && tokenThat.Length > 20, "C dang nhap tai khoan A bang code: " + (ok ? "duoc" : e));
        FirebaseMang.Quen();                                   // xoa trong bo nho + kho...
        PlayerPrefs.SetString(KhoaPhien, tokenThat); PlayerPrefs.Save();   // ...roi tra lai phien nhu lan mo game sau
        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh != null) sanh.enabled = false;
        var mdn = DungMoi(tam);
        mdn.daVao = () => { if (sanh != null) sanh.enabled = true; };
        yield return Cho(() => FirebaseMang.DaDangNhap && sanh != null && sanh.enabled, 12f);
        yield return new WaitForSeconds(1.0f);
        Kiem(FirebaseMang.DaDangNhap && sanh != null && sanh.enabled, "C tu khoi phuc phien -> vao sanh (dang nhap " + FirebaseMang.DaDangNhap + ", sanh bat " + (sanh != null && sanh.enabled) + ")");
        Kiem(daChuyen.Count == 0, "C khong chuyen trang (" + daChuyen.Count + ")");
        Kiem(ManDangNhap.SoLanVeKhungDangNhap == 0, "C khung dang nhap ve " + ManDangNhap.SoLanVeKhungDangNhap + " lan (phai 0)");

        // ---- D. Dang xuat o sanh ----
        nk.AppendLine("== D Bam DANG XUAT o sanh");
        if (sanh != null) sanh.DangXuat();
        yield return new WaitForSeconds(1.0f);
        Kiem(!FirebaseMang.DaDangNhap, "D het dang nhap");
        Kiem(PlayerPrefs.GetString(KhoaPhien, "") == "", "D phien da xoa (trang chu cung thay da dang xuat)");
        Kiem(daChuyen.Count == 1 && daChuyen[0] == ManDangNhap.DuongDangXuat, "D chuyen: " + string.Join(" | ", daChuyen.ToArray()) + " (phai " + ManDangNhap.DuongDangXuat + ", chi mot lan)");

        // ---- E. Doi chung ban Editor ----
        nk.AppendLine("== E DOI CHUNG ban Editor (ChiDangNhapOTrangChu = false)");
        ManDangNhap.ChiDangNhapOTrangChu = false;
        DungMoi(tam);
        yield return new WaitForSeconds(1.5f);
        Kiem(ManDangNhap.SoLanVeKhungDangNhap > 10 && daChuyen.Count == 0,
             "E ve khung dang nhap " + ManDangNhap.SoLanVeKhungDangNhap + " lan, chuyen trang " + daChuyen.Count + " (phai > 10 / 0)");

        nk.Insert(0, "MENU 155 - Dang nhap chi o trang chu: " + loi + " loi\n");
        File.WriteAllText(Ra, nk.ToString());
        Debug.Log("[ThuDangNhapTrangChu] " + loi + " loi\n" + nk);
        Ket();
    }

    static void Ket()
    {
        var rac = GameObject.Find("TAM_ThuDangNhapTrangChu");
        if (rac != null) Object.Destroy(rac);
        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        // Doi thoat HAN Play: mo canh trong luc dang chuyen thi Unity nap lai canh truoc Play de len (lan 1 con o MainMenu)
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= TraLai;
        ManDangNhap.ChiDangNhapOTrangChu = false;
        ManDangNhap.ChuyenTrang = null;
        ManDangNhap.DatLaiChuyenTrangThat();
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc); else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
        if (!string.IsNullOrEmpty(canhGoc) && EditorSceneManager.GetActiveScene().path != canhGoc) EditorSceneManager.OpenScene(canhGoc);
        Debug.Log("[ThuDangNhapTrangChu] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
