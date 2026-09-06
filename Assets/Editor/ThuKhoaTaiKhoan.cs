using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: ADMIN KHOA TAI KHOAN TREN WEB THI TRONG GAME CO VAO DUOC KHONG.
///
/// Day la cho de nut "Khoa" tren trang quan tri co y nghia. Neu game van cho
/// vao thi cai nut kia chi la mot o tich trong co so du lieu.
///
/// Khoa nam o Firestore (<c>nguoichoi/{uid}.biKhoa</c>) chu KHONG phai o
/// Firebase Auth, nen buoc dang nhap van thanh cong - phai den buoc tai ho so
/// moi bi chan. Phep thu nay kiem tra dung thu tu do.
///
/// De phep thu doc lap voi gia thiet: thu ca hai tai khoan. Neu tai khoan
/// KHONG bi khoa cung bi tu choi thi phep thu dang bao sai chu khong phai
/// khoa dang chay dung.
///
/// Ket qua ghi ra <c>PlayTestShots/mang_khoa.txt</c>.
/// </summary>
public static class ThuKhoaTaiKhoan
{
    const string EmailBiKhoa = "thunghiem.a.diablo25d@gmail.com";
    const string EmailBinhThuong = "thunghiem.b.diablo25d@gmail.com";
    const string MatKhau = "matkhau123456";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/27. Chay thu MANG - khoa tai khoan", false, 113)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().name != "MainMenu")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;

        var go = new GameObject("TAM_ThuKhoa");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuKhoa] " + s);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1]");

        // ---- 1. TAI KHOAN DANG BI KHOA ----
        FirebaseMang.Quen();
        bool ok = false; string e = null;

        yield return FirebaseMang.DangNhap(EmailBiKhoa, MatKhau, (o, err) => { ok = o; e = err; });
        Ghi("A (dang bi khoa) - dang nhap Auth: " + (ok ? "OK" : "LOI - " + e));
        if (!ok) { Ghi("[LOI] khoa o Firestore ma Auth cung tu choi - sai thiet ke"); loi++; }

        if (ok)
        {
            yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
            Ghi("A - tai ho so: " + (ok ? "VAO DUOC" : "bi chan - " + e));

            if (ok) { Ghi("[LOI] tai khoan bi khoa ma van vao duoc game"); loi++; }
            else if (e == null || !e.Contains("bi khoa"))
            { Ghi("[LOI] bi chan nhung khong phai vi khoa: " + e); loi++; }
        }

        // Phien phai bi bo di, khong duoc giu lai de lan sau tu vao
        Ghi("A - phien con giu lai khong: " + (FirebaseMang.DaDangNhap ? "CON" : "da bo"));

        // ---- 2. TAI KHOAN BINH THUONG: phai vao duoc ----
        // Neu buoc nay cung bi chan thi phep thu tren khong chung minh duoc gi.
        FirebaseMang.Quen();
        yield return FirebaseMang.DangNhap(EmailBinhThuong, MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi("B (khong khoa) - vao game: " + (ok ? "OK, ten = " + HoSoMang.CuaToi.ten : "LOI - " + e));
        if (!ok) { Ghi("[LOI] tai khoan binh thuong cung khong vao duoc"); loi++; }

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/mang_khoa.txt", bao.ToString());

        var rac = GameObject.Find("TAM_ThuKhoa");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }
}
