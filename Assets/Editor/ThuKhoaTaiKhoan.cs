using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// CHAY THU: ADMIN KHOA TAI KHOAN THI TRONG GAME CO VAO DUOC KHONG.
///
/// Day la cho de nut "Khoa" tren trang quan tri co y nghia. Neu game van cho
/// vao thi cai nut kia chi la mot o tich trong co so du lieu.
///
/// Khoa nam o Firestore (<c>nguoichoi/{uid}.biKhoa</c>) chu KHONG phai o
/// Firebase Auth, nen buoc dang nhap van thanh cong - phai den buoc tai ho so
/// moi bi chan. Phep thu kiem tra dung thu tu do.
///
/// PHEP THU TU CHUA: no TU KHOA tai khoan A roi TU MO lai, chu khong doi
/// truoc do co ai khoa san. Ban dau viet kieu "gia su A dang bi khoa" - chay
/// lai sau khi da mo khoa thi ket qua sai ma nhin van nhu that.
///
/// Viec khoa can quyen admin, nen phep thu dung TAI KHOAN ADMIN RIENG cua chu
/// game (khai o dong 4-5 cua chay-thu-mang.txt). Truoc day tien tay lay tai
/// khoan nguoi choi B lam admin - tien nhung sai: tai khoan chay thu ma cam
/// quyen quan tri thi mat mot cai la mat ca trang quan tri.
///
/// Ba lan do, khong phai mot: truoc khi khoa phai VAO DUOC, sau khi khoa phai
/// BI CHAN, mo khoa xong phai VAO DUOC lai. Chi do lan giua thi khong phan
/// biet duoc "khoa co tac dung" voi "tai khoan nay von khong vao duoc".
///
/// Ket qua ghi ra <c>PlayTestShots/mang_khoa.txt</c>.
/// </summary>
public static class ThuKhoaTaiKhoan
{
    // Doc tu file ngoai git - xem ThongTinChayThu
    static string EmailA { get { return ThongTinChayThu.EmailA; } }
    static string MatKhau { get { return ThongTinChayThu.MatKhau; } }

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/27. Chay thu MANG - khoa tai khoan", false, 113)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        // Thieu thong tin tai khoan thi dung han o day, dung vao Play roi
        // moi hong - vao Play xong bao loi thi nhin het nhu loi mang.
        if (!ThongTinChayThu.DocHoacBao()) return;

        // CAT PHIEN DANG NHAP DANG LUU TRUOC KHI VAO PLAY, tra lai khi xong.
        //
        // Con phien luu san thi ManDangNhap.Start TU DANG NHAP LAI ngay khung
        // hinh dau - chay song song voi phep thu va doi FirebaseMang.Uid sang
        // tai khoan khac giua chung. Da vap 11/09/2026: buoc 3 doc ho so cua
        // tai khoan kia (khong bi khoa) va bao "A bi khoa ma van vao duoc game".
        // Menu 28 va 50 da cat phien vi dung ly do nay.
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

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

    /// <summary>
    /// Dat co biKhoa cho MOT NGUOI KHAC. Chi admin lam duoc - luat Firestore
    /// cho phep 'laAdmin()' sua ho so bat ky. Viet o day chu khong them ham
    /// vao HoSoMang: game khong bao gio can khoa nguoi khac, chi trang quan
    /// tri moi can.
    /// </summary>
    static IEnumerator DatKhoa(string uid, bool khoa, System.Action<bool, string> xong)
    {
        string duong = FirebaseMang.DuongFirestore + "/nguoichoi/" + uid
                     + "?updateMask.fieldPaths=biKhoa";
        string than = "{\"fields\":{\"biKhoa\":{\"booleanValue\":"
                    + (khoa ? "true" : "false") + "}}}";

        using (var yc = new UnityWebRequest(duong, "PATCH"))
        {
            yc.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(than));
            yc.downloadHandler = new DownloadHandlerBuffer();
            yc.SetRequestHeader("Content-Type", "application/json");
            yc.SetRequestHeader("Authorization", "Bearer " + FirebaseMang.IdToken);
            yield return yc.SendWebRequest();

            bool ok = yc.result == UnityWebRequest.Result.Success;
            xong(ok, ok ? null : yc.downloadHandler.text);
        }
    }

    /// <summary>Dang nhap roi tai ho so. Tra ve co vao duoc game khong.</summary>
    static IEnumerator ThuVao(string email, System.Action<bool, string> xong)
    {
        yield return ThuVaoBangMatKhau(email, MatKhau, xong);
    }

    static IEnumerator ThuVaoBangMatKhau(string email, string matKhau,
                                         System.Action<bool, string> xong)
    {
        FirebaseMang.Quen();
        bool ok = false; string e = null;

        yield return FirebaseMang.DangNhap(email, matKhau, (o, err) => { ok = o; e = err; });
        if (!ok) { xong(false, "khong qua duoc buoc Auth - " + e); yield break; }

        yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        xong(ok, e);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 6] tu khoa roi tu mo lai; kiem ca nguoi thuong khong khoa duoc ai");

        bool ok = false; string e = null;

        // ---- 1. TRUOC KHI KHOA: A phai vao duoc ----
        yield return ThuVao(EmailA, (o, err) => { ok = o; e = err; });
        string uidA = FirebaseMang.Uid;
        Ghi("1. truoc khi khoa, A vao game: " + (ok ? "VAO DUOC" : "bi chan - " + e));
        if (!ok) { Ghi("[LOI] chua khoa ma A da khong vao duoc - phep thu vo nghia"); loi++; }

        if (string.IsNullOrEmpty(uidA))
        { Ghi("[LOI] khong lay duoc uid cua A"); loi++; Ket(); yield break; }

        // ---- 1b. NGUOI CHOI THUONG KHONG DUOC KHOA NGUOI KHAC ----
        // Buoc nay khong can quyen admin nen luon chay duoc. No do dung cai
        // ranh gioi: neu mot tai khoan nguoi choi binh thuong van khoa duoc
        // nguoi khac thi luat bao mat hong, va ca trang quan tri thanh vo
        // nghia. Truoc day tai khoan B tung co quyen admin - buoc nay chung
        // minh quyen do da bi thu hoi that.
        yield return ThuVao(ThongTinChayThu.EmailB, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] khong dang nhap duoc B: " + e); loi++; Ket(); yield break; }

        yield return DatKhoa(uidA, true, (o, err) => { ok = o; e = err; });
        Ghi("1b. nguoi choi thuong (B) thu khoa A: "
            + (ok ? "KHOA DUOC" : "bi tu choi - dung nhu mong doi"));
        if (ok)
        {
            Ghi("[LOI] nguoi choi thuong khoa duoc nguoi khac - luat bao mat hong");
            loi++;
        }

        // ---- 2. ADMIN KHOA A ----
        // Tai khoan admin la tai khoan RIENG cua chu game, khong phai tai khoan
        // nguoi choi. Chua khai bao thi dung o day - bo qua chu khong bao loi,
        // vi phan con lai cua phep thu khong the chay ma khong co quyen admin.
        if (!ThongTinChayThu.CoTaiKhoanAdmin)
        {
            Ghi("2. BO QUA phan con lai: chua khai tai khoan admin trong "
                + "chay-thu-mang.txt (dong 4 = email admin, dong 5 = mat khau).");
            Ghi("so loi ghi nhan = " + loi);
            Ket(); yield break;
        }

        yield return ThuVaoBangMatKhau(ThongTinChayThu.EmailAdmin,
                                       ThongTinChayThu.MatKhauAdmin,
                                       (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] khong dang nhap duoc tai khoan admin: " + e); loi++; Ket(); yield break; }

        yield return DatKhoa(uidA, true, (o, err) => { ok = o; e = err; });
        Ghi("2. admin khoa A: " + (ok ? "OK" : "LOI - " + e));
        if (!ok) { Ghi("[LOI] khoa khong duoc - tai khoan nay da co quyen admin chua?"); loi++; Ket(); yield break; }

        // ---- 3. SAU KHI KHOA: A phai bi chan ----
        yield return ThuVao(EmailA, (o, err) => { ok = o; e = err; });
        Ghi("3. sau khi khoa, A vao game: " + (ok ? "VAN VAO DUOC" : "bi chan - " + e));

        if (ok) { Ghi("[LOI] tai khoan bi khoa ma van vao duoc game"); loi++; }
        else if (!FirebaseMang.LaLoiBiKhoa(e))
        { Ghi("[LOI] bi chan nhung khong phai vi khoa: " + e); loi++; }

        // Phien phai bi bo di, khong duoc giu lai de lan sau tu vao thang
        Ghi("   phien cua A con giu lai khong: " + (FirebaseMang.DaDangNhap ? "CON" : "da bo"));
        if (FirebaseMang.DaDangNhap) { Ghi("[LOI] van giu phien cua tai khoan bi khoa"); loi++; }

        // ---- 4. MO KHOA ROI THU LAI ----
        yield return ThuVaoBangMatKhau(ThongTinChayThu.EmailAdmin,
                                       ThongTinChayThu.MatKhauAdmin,
                                       (o, err) => { ok = o; e = err; });
        if (ok) yield return DatKhoa(uidA, false, (o, err) => { ok = o; e = err; });
        Ghi("4. admin mo khoa A: " + (ok ? "OK" : "LOI - " + e));
        if (!ok) { Ghi("[LOI] khong mo khoa duoc - A se ket o trang thai bi khoa"); loi++; }

        yield return ThuVao(EmailA, (o, err) => { ok = o; e = err; });
        Ghi("5. sau khi mo khoa, A vao game: " + (ok ? "VAO DUOC" : "bi chan - " + e));
        if (!ok) { Ghi("[LOI] mo khoa roi ma van khong vao duoc"); loi++; }

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/mang_khoa.txt", bao.ToString());

        var rac = GameObject.Find("TAM_ThuKhoa");
        if (rac != null) Object.DestroyImmediate(rac);

        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraPhien;
    }

    static void TraPhien()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraPhien;
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc);
        else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
    }
}
