using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHUP BA MAN HINH CUA PHAN MANG: dang nhap, sanh phong, trong phong.
///
/// Do tang REST bang so lieu (menu 26, 27) van chua chung minh duoc mot dieu:
/// giao dien co VE RA khong. OnGUI hong mot chut la ca man hinh trong, ma so
/// do mang van dep nhu thuong.
///
/// Chup Game view chu khong dung camera phu: camera phu khong ve OnGUI, chup
/// bang no thi ket luan sai (xem memory "chup billboard phai dung camera game").
///
/// Anh ra <c>PlayTestShots/mang_man_*.png</c>. Buoc cuoi luon xoa phong da tao.
/// </summary>
public static class ChupManMang
{
    // Doc tu file ngoai git - xem ThongTinChayThu
    static string Email { get { return ThongTinChayThu.EmailB; } }
    static string MatKhau { get { return ThongTinChayThu.MatKhau; } }

    static bool daBatDau;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/28. Chup man DANG NHAP va SANH PHONG", false, 114)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        // Thieu thong tin tai khoan thi dung han o day, dung vao Play roi
        // moi hong - vao Play xong bao loi thi nhin het nhu loi mang.
        if (!ThongTinChayThu.DocHoacBao()) return;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().name != "MainMenu")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        // Bo phien cu TRUOC khi vao Play. ManDangNhap.Start() chay ngay khung
        // hinh dau, thay con phien la tu dang nhap lai - lam vay thi khong con
        // man dang nhap de chup. Bo o day chu khong goi Quen() trong coroutine:
        // luc do viec tu dang nhap dang chay do dang, Quen() giat mat token
        // giua chung va sinh ra mot loi 403 khong co that.
        PlayerPrefs.DeleteKey("diablo25d_refresh");
        PlayerPrefs.Save();

        daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;

        var go = new GameObject("TAM_ChupMang");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);

        ScreenCapture.CaptureScreenshot(duong);

        // CaptureScreenshot ghi file o cuoi khung hinh sau, khong ghi ngay
        for (int i = 0; i < 60 && !File.Exists(duong); i++)
            yield return new WaitForEndOfFrame();

        long co = File.Exists(duong) ? new FileInfo(duong).Length : 0;
        Debug.Log("[ChupMang] " + ten + ": " + (co > 0 ? co / 1024 + " KB" : "KHONG CHUP DUOC"));
    }

    static IEnumerator ChayKichBan()
    {
        var bao = new System.Text.StringBuilder();
        bao.AppendLine("[ban 3] chup man hinh phan mang");

        // ---- 1. MAN DANG NHAP ----
        yield return new WaitForSecondsRealtime(0.7f);
        yield return Chup("mang_man_1_dangnhap");
        bao.AppendLine("1. man dang nhap: " + CoAnh("mang_man_1_dangnhap"));

        // ---- 2. SANH PHONG ----
        // Di dung duong nguoi choi di: dang nhap that roi bao cho MainMenuUI
        // qua chinh callback ma no da noi vao ManDangNhap.
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(Email, MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        bao.AppendLine("   dang nhap: " + (ok ? "OK, ten = " + HoSoMang.CuaToi.ten : "LOI - " + e));
        if (!ok) { Ket(bao); yield break; }

        var manDangNhap = Object.FindAnyObjectByType<ManDangNhap>();
        if (manDangNhap == null || manDangNhap.daVao == null)
        {
            bao.AppendLine("[LOI] khong tim thay ManDangNhap hoac MainMenuUI chua noi callback");
            Ket(bao); yield break;
        }
        manDangNhap.daVao();

        yield return new WaitForSecondsRealtime(2.5f);   // doi mot nhip hoi danh sach
        yield return Chup("mang_man_2_sanh");
        bao.AppendLine("2. sanh phong: " + CoAnh("mang_man_2_sanh"));

        // ---- 3. TRONG PHONG ----
        yield return PhongMang.TaoPhong("Phong chup hinh", "Act2", (o, err) => { ok = o; e = err; });
        bao.AppendLine("   tao phong: " + (ok ? "OK - " + PhongMang.PhongHienTai.ma : "LOI - " + e));

        if (ok)
        {
            // dangO la truong rieng cua ManSanh - o day dang thay cho cu bam
            // nut "TAO PHONG", chu khong phai duong ma nguoi choi di.
            var manSanh = Object.FindAnyObjectByType<ManSanh>();
            var f = typeof(ManSanh).GetField("dangO",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            if (manSanh != null && f != null)
            {
                f.SetValue(manSanh, System.Enum.Parse(f.FieldType, "TrongPhong"));
                yield return new WaitForSecondsRealtime(2.0f);
                yield return Chup("mang_man_3_trongphong");
                bao.AppendLine("3. trong phong: " + CoAnh("mang_man_3_trongphong"));
            }
            else bao.AppendLine("[LOI] khong dat duoc trang thai TrongPhong");

            // ---- DON ----
            yield return FirebaseMang.Xoa("phong/" + PhongMang.PhongHienTai.ma, (o, err) => { });
            yield return FirebaseMang.Xoa("tran/" + PhongMang.PhongHienTai.ma, (o, err) => { });
            bao.AppendLine("   don phong chup hinh: xong");
        }

        Ket(bao);
    }

    static string CoAnh(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (!File.Exists(d)) return "KHONG CO ANH";
        return new FileInfo(d).Length / 1024 + " KB";
    }

    static void Ket(System.Text.StringBuilder bao)
    {
        File.WriteAllText("PlayTestShots/mang_chup.txt", bao.ToString());
        Debug.Log("[ChupMang]\n" + bao);

        var rac = GameObject.Find("TAM_ChupMang");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }
}
