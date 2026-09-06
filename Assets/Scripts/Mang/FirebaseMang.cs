using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// NOI GAME VOI FIREBASE BANG REST API.
///
/// VI SAO KHONG DUNG FIREBASE UNITY SDK: goi do la lop boc quanh thu vien
/// native C++ (.aar cho Android, .framework cho iOS, .dll cho may ban).
/// WebGL khong nap duoc plugin native - Firebase Unity SDK khong co va se
/// khong co ban WebGL. Ma WebGL lai la nen tang chinh cua du an nay.
///
/// REST API thi chay o MOI noi: trong Editor, ban PC, WebGL, dien thoai -
/// chi can gui duoc mot yeu cau HTTP. Doi lai la khong co luong du lieu day
/// ve theo thoi gian thuc (streaming), nen sanh cho phai HOI LAI theo nhip
/// (xem <see cref="PhongMang"/>). Voi mot cai sanh cho thi hoi moi giay mot
/// lan la du, va no re hon nhieu so voi mo mot ket noi luon thuong truc.
///
/// GIU TOKEN: sau khi dang nhap, Firebase tra ve mot idToken song duoc mot
/// gio va mot refreshToken song lau. Moi yeu cau doc/ghi deu phai kem idToken.
/// </summary>
public static class FirebaseMang
{
    // ================================================================
    //  CAU HINH
    // ================================================================
    //
    // May khoa nay KHONG phai bi mat. Firebase thiet ke de chung nam cong khai
    // trong ung dung; cai bao ve du lieu la Security Rules chu khong phai viec
    // giau khoa.

    public const string ApiKey = "AIzaSyDeC_T3VxJc-tc-EeX7Vl_ajGU28ZNeCZA";
    public const string DuAn = "diablo25d-game";
    public const string DuongDatabase =
        "https://diablo25d-game-default-rtdb.asia-southeast1.firebasedatabase.app";
    public const string DuongFirestore =
        "https://firestore.googleapis.com/v1/projects/diablo25d-game/databases/(default)/documents";

    const string DuongXacThuc = "https://identitytoolkit.googleapis.com/v1/accounts";
    const string DuongLamMoi = "https://securetoken.googleapis.com/v1/token";

    // ================================================================
    //  PHIEN DANG NHAP
    // ================================================================

    public static string Uid { get; private set; }
    public static string Email { get; private set; }
    public static string IdToken { get; private set; }
    static string refreshToken;
    static float hetHanLuc;              // Time.realtimeSinceStartup luc token het han

    public static bool DaDangNhap { get { return !string.IsNullOrEmpty(IdToken); } }

    /// <summary>Ten hien thi trong game - doc tu ho so, khong phai tu Firebase Auth.</summary>
    public static string TenHienThi { get; set; }

    public static void Quen()
    {
        Uid = null; Email = null; IdToken = null; refreshToken = null;
        TenHienThi = null; hetHanLuc = 0f;
        PlayerPrefs.DeleteKey(KhoaLuuToken);
        PlayerPrefs.Save();
    }

    const string KhoaLuuToken = "diablo25d_refresh";

    /// <summary>Co the dang nhap lai tu lan truoc khong.</summary>
    public static bool CoTheTuDangNhapLai
    {
        get { return !string.IsNullOrEmpty(PlayerPrefs.GetString(KhoaLuuToken, "")); }
    }

    // ================================================================
    //  DANG KY / DANG NHAP
    // ================================================================

    /// <summary>
    /// Dang ky tai khoan moi. <paramref name="xong"/> nhan (thanhCong, loiBangTiengViet).
    /// </summary>
    public static IEnumerator DangKy(string email, string matKhau, Action<bool, string> xong)
    {
        string than = "{\"email\":\"" + Thoat(email) + "\",\"password\":\"" + Thoat(matKhau)
                    + "\",\"returnSecureToken\":true}";
        yield return GuiXacThuc(":signUp", than, xong);
    }

    public static IEnumerator DangNhap(string email, string matKhau, Action<bool, string> xong)
    {
        string than = "{\"email\":\"" + Thoat(email) + "\",\"password\":\"" + Thoat(matKhau)
                    + "\",\"returnSecureToken\":true}";
        yield return GuiXacThuc(":signInWithPassword", than, xong);
    }

    /// <summary>Dang nhap lai bang refresh token da luu tu lan truoc.</summary>
    public static IEnumerator TuDangNhapLai(Action<bool, string> xong)
    {
        string luu = PlayerPrefs.GetString(KhoaLuuToken, "");
        if (string.IsNullOrEmpty(luu)) { xong(false, "Chua tung dang nhap"); yield break; }

        refreshToken = luu;
        yield return LamMoiToken(xong);
    }

    static IEnumerator GuiXacThuc(string hanh, string than, Action<bool, string> xong)
    {
        using (var yc = TaoYeuCau(DuongXacThuc + hanh + "?key=" + ApiKey, "POST", than))
        {
            yield return yc.SendWebRequest();

            if (yc.result != UnityWebRequest.Result.Success)
            {
                xong(false, DichLoi(yc.downloadHandler.text));
                yield break;
            }

            var d = JsonUtility.FromJson<GoiXacThuc>(yc.downloadHandler.text);
            Uid = d.localId;
            Email = d.email;
            IdToken = d.idToken;
            refreshToken = d.refreshToken;

            int giay;
            if (!int.TryParse(d.expiresIn, out giay)) giay = 3600;
            hetHanLuc = Time.realtimeSinceStartup + giay - 60f;   // lam moi som mot phut

            PlayerPrefs.SetString(KhoaLuuToken, refreshToken);
            PlayerPrefs.Save();
            xong(true, null);
        }
    }

    static IEnumerator LamMoiToken(Action<bool, string> xong)
    {
        string than = "grant_type=refresh_token&refresh_token=" + refreshToken;
        using (var yc = new UnityWebRequest(DuongLamMoi + "?key=" + ApiKey, "POST"))
        {
            yc.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(than));
            yc.downloadHandler = new DownloadHandlerBuffer();
            yc.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return yc.SendWebRequest();

            if (yc.result != UnityWebRequest.Result.Success)
            {
                PlayerPrefs.DeleteKey(KhoaLuuToken);
                xong(false, "Phien dang nhap da het han, hay dang nhap lai.");
                yield break;
            }

            var d = JsonUtility.FromJson<GoiLamMoi>(yc.downloadHandler.text);
            IdToken = d.id_token;
            Uid = d.user_id;
            refreshToken = d.refresh_token;

            int giay;
            if (!int.TryParse(d.expires_in, out giay)) giay = 3600;
            hetHanLuc = Time.realtimeSinceStartup + giay - 60f;

            PlayerPrefs.SetString(KhoaLuuToken, refreshToken);
            PlayerPrefs.Save();
            xong(true, null);
        }
    }

    /// <summary>
    /// Bao dam token con han truoc khi goi API. Token Firebase song mot gio,
    /// ma mot van dau co the dai hon the.
    /// </summary>
    public static IEnumerator BaoDamConHan()
    {
        if (!DaDangNhap) yield break;
        if (Time.realtimeSinceStartup < hetHanLuc) yield break;
        if (string.IsNullOrEmpty(refreshToken)) yield break;

        bool xong = false;
        yield return LamMoiToken((ok, loi) => { xong = true; });
    }

    // ================================================================
    //  REALTIME DATABASE
    // ================================================================

    /// <summary>Doc mot nhanh. Tra ve chuoi JSON, hoac null neu loi.</summary>
    public static IEnumerator Doc(string duongDan, Action<string> xong)
    {
        yield return BaoDamConHan();
        string d = DuongDatabase + "/" + duongDan + ".json?auth=" + IdToken;
        using (var yc = UnityWebRequest.Get(d))
        {
            yield return yc.SendWebRequest();
            xong(yc.result == UnityWebRequest.Result.Success ? yc.downloadHandler.text : null);
        }
    }

    /// <summary>Ghi de mot nhanh (PUT).</summary>
    public static IEnumerator Ghi(string duongDan, string json, Action<bool, string> xong)
    {
        yield return BaoDamConHan();
        yield return GuiDatabase(duongDan, "PUT", json, xong);
    }

    /// <summary>Ghi mot phan (PATCH) - chi doi nhung khoa co trong json.</summary>
    public static IEnumerator VaGhi(string duongDan, string json, Action<bool, string> xong)
    {
        yield return BaoDamConHan();
        yield return GuiDatabase(duongDan, "PATCH", json, xong);
    }

    /// <summary>Them mot muc co khoa tu sinh (POST). Tra ve khoa vua tao.</summary>
    public static IEnumerator Them(string duongDan, string json, Action<string, string> xong)
    {
        yield return BaoDamConHan();
        string d = DuongDatabase + "/" + duongDan + ".json?auth=" + IdToken;
        using (var yc = TaoYeuCau(d, "POST", json))
        {
            yield return yc.SendWebRequest();
            if (yc.result != UnityWebRequest.Result.Success)
            { xong(null, DichLoi(yc.downloadHandler.text)); yield break; }

            var d2 = JsonUtility.FromJson<GoiKhoaMoi>(yc.downloadHandler.text);
            xong(d2.name, null);
        }
    }

    public static IEnumerator Xoa(string duongDan, Action<bool, string> xong)
    {
        yield return BaoDamConHan();
        string d = DuongDatabase + "/" + duongDan + ".json?auth=" + IdToken;
        using (var yc = UnityWebRequest.Delete(d))
        {
            yc.downloadHandler = new DownloadHandlerBuffer();
            yield return yc.SendWebRequest();
            bool ok = yc.result == UnityWebRequest.Result.Success;
            xong(ok, ok ? null : DichLoi(yc.downloadHandler.text));
        }
    }

    static IEnumerator GuiDatabase(string duongDan, string cach, string json,
                                   Action<bool, string> xong)
    {
        string d = DuongDatabase + "/" + duongDan + ".json?auth=" + IdToken;
        using (var yc = TaoYeuCau(d, cach, json))
        {
            yield return yc.SendWebRequest();
            bool ok = yc.result == UnityWebRequest.Result.Success;
            xong(ok, ok ? null : DichLoi(yc.downloadHandler.text));
        }
    }

    // ================================================================
    //  TIEN ICH
    // ================================================================

    static UnityWebRequest TaoYeuCau(string duong, string cach, string than)
    {
        var yc = new UnityWebRequest(duong, cach);
        yc.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(than ?? ""));
        yc.downloadHandler = new DownloadHandlerBuffer();
        yc.SetRequestHeader("Content-Type", "application/json");
        return yc;
    }

    /// <summary>Thoat ky tu de nhet chuoi vao JSON mot cach an toan.</summary>
    public static string Thoat(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new StringBuilder(s.Length + 8);
        foreach (char c in s)
        {
            switch (c)
            {
                case '"':  sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n");  break;
                case '\r': sb.Append("\\r");  break;
                case '\t': sb.Append("\\t");  break;
                default:
                    if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4"));
                    else sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>Doi ma loi cua Firebase thanh cau tieng Viet nguoi choi hieu.</summary>
    public static string DichLoi(string thanLoi)
    {
        if (string.IsNullOrEmpty(thanLoi)) return "Mat ket noi mang.";

        if (thanLoi.Contains("EMAIL_EXISTS"))        return "Email nay da co nguoi dung roi.";
        if (thanLoi.Contains("INVALID_EMAIL"))       return "Email khong hop le.";
        if (thanLoi.Contains("WEAK_PASSWORD"))       return "Mat khau phai tu 6 ky tu tro len.";
        if (thanLoi.Contains("EMAIL_NOT_FOUND"))     return "Khong co tai khoan nao dung email nay.";
        if (thanLoi.Contains("INVALID_PASSWORD"))    return "Sai mat khau.";
        if (thanLoi.Contains("INVALID_LOGIN_CREDENTIALS")) return "Sai email hoac mat khau.";
        if (thanLoi.Contains("USER_DISABLED"))       return "Tai khoan da bi khoa.";
        if (thanLoi.Contains("TOO_MANY_ATTEMPTS"))   return "Thu qua nhieu lan. Doi mot lat roi thu lai.";
        if (thanLoi.Contains("OPERATION_NOT_ALLOWED"))
            return "Dang nhap bang email chua duoc bat trong Firebase Console.";
        if (thanLoi.Contains("CONFIGURATION_NOT_FOUND"))
            return "Firebase Authentication chua duoc bat cho du an nay.";
        // Realtime Database noi "Permission denied", con Firestore noi
        // "Missing or insufficient permissions." - hai chu khac han nhau, phai
        // bat ca hai. Thieu ve Firestore thi nguoi choi nhin thay nguyen khoi
        // JSON do loe tren man dang nhap.
        if (thanLoi.Contains("Permission denied") || thanLoi.Contains("permission_denied")
            || thanLoi.Contains("Missing or insufficient permissions")
            || thanLoi.Contains("PERMISSION_DENIED"))
            return "Khong du quyen. Tai khoan cua ban co the da bi khoa.";

        if (thanLoi.Contains("UNAUTHENTICATED") || thanLoi.Contains("invalid authentication")
            || thanLoi.Contains("INVALID_ID_TOKEN") || thanLoi.Contains("TOKEN_EXPIRED"))
            return "Phien dang nhap da het han, hay dang nhap lai.";

        return "Loi: " + (thanLoi.Length > 120 ? thanLoi.Substring(0, 120) : thanLoi);
    }

    // ================================================================
    //  GOI JSON
    // ================================================================

    [Serializable] class GoiXacThuc
    {
        public string idToken, email, refreshToken, expiresIn, localId;
    }

    [Serializable] class GoiLamMoi
    {
        public string id_token, refresh_token, expires_in, user_id;
    }

    [Serializable] class GoiKhoaMoi { public string name; }
}
