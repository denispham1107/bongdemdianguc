using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HO SO NGUOI CHOI TREN FIRESTORE.
///
/// Firebase Auth chi giu email va mat khau. Ten trong game, so tran, so tran
/// thang, co bi khoa hay khong deu nam o collection "nguoichoi" cua Firestore.
///
/// VI SAO PHAI CO BAN SAO NAY: Firebase Auth khong cho phia ung dung liet ke
/// toan bo tai khoan - chi Admin SDK chay tren may chu moi lam duoc. Nen trang
/// quan tri doc collection "nguoichoi" chu khong doc Auth. Moi tai khoan tao ra
/// trong game deu phai co mot ban ghi o day, khong thi admin khong nhin thay.
///
/// Firestore REST dung mot kieu JSON rieng: moi truong boc trong mot the bao
/// kieu du lieu (stringValue, integerValue, booleanValue...). Lop nay lo phan
/// dich qua lai do.
/// </summary>
public static class HoSoMang
{
    public class HoSo
    {
        public string ten = "";
        public string email = "";
        public int soTranChoi, soTranThang, soQuaiDaDiet, soNguoiDaHa;
        public bool biKhoa;
    }

    public static HoSo CuaToi = new HoSo();

    static string DuongHoSo(string uid)
    {
        return FirebaseMang.DuongFirestore + "/nguoichoi/" + uid;
    }

    /// <summary>
    /// Doc ho so. Chua co thi tu tao voi moi con so bang 0 - dung nhu luat
    /// bao mat doi hoi (khong ai tu dat soTranThang = 99 luc dang ky).
    /// </summary>
    public static IEnumerator TaiHoacTao(string tenNeuTao, Action<bool, string> xong)
    {
        yield return FirebaseMang.BaoDamConHan();

        string uid = FirebaseMang.Uid;
        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(FirebaseMang.IdToken))
        {
            xong(false, "Phien dang nhap khong con hieu luc, hay dang nhap lai.");
            yield break;
        }

        using (var yc = UnityWebRequest.Get(DuongHoSo(uid)))
        {
            yc.SetRequestHeader("Authorization", "Bearer " + FirebaseMang.IdToken);
            yield return yc.SendWebRequest();

            // CHI 404 moi co nghia la "chua co ho so". Moi ma khac (403 khong du
            // quyen, 401 het han, mat mang) ma van di tao moi thi lan tao cung
            // hong, va nguoi choi doc duoc "Khong tao duoc ho so" - sai han
            // nguyen nhan that.
            if (yc.responseCode != 200 && yc.responseCode != 404)
            {
                xong(false, FirebaseMang.DichLoi(yc.downloadHandler != null
                                                 ? yc.downloadHandler.text : null));
                yield break;
            }

            if (yc.result == UnityWebRequest.Result.Success)
            {
                DocVao(yc.downloadHandler.text, CuaToi);
                FirebaseMang.TenHienThi = CuaToi.ten;

                if (CuaToi.biKhoa)
                {
                    xong(false, "Tai khoan cua ban da bi khoa. Hay lien he quan tri vien.");
                    yield break;
                }
                xong(true, null);
                yield break;
            }
        }

        // Chua co ho so - tao moi
        string ten = string.IsNullOrEmpty(tenNeuTao)
            ? (FirebaseMang.Email ?? "NguoiChoi").Split('@')[0]
            : tenNeuTao;
        if (ten.Length > 16) ten = ten.Substring(0, 16);

        string than =
            "{\"fields\":{"
            + "\"ten\":{\"stringValue\":\"" + FirebaseMang.Thoat(ten) + "\"},"
            + "\"email\":{\"stringValue\":\"" + FirebaseMang.Thoat(FirebaseMang.Email ?? "") + "\"},"
            + "\"ngayTao\":{\"timestampValue\":\"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\"},"
            + "\"lanDangNhapCuoi\":{\"timestampValue\":\"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\"},"
            + "\"soTranChoi\":{\"integerValue\":\"0\"},"
            + "\"soTranThang\":{\"integerValue\":\"0\"},"
            + "\"soQuaiDaDiet\":{\"integerValue\":\"0\"},"
            + "\"soNguoiDaHa\":{\"integerValue\":\"0\"},"
            + "\"biKhoa\":{\"booleanValue\":false},"
            + "\"ghiChuAdmin\":{\"stringValue\":\"\"}"
            + "}}";

        // documentId nam o chuoi truy van, khong nam trong than - Firestore REST
        // bat dat vay khi muon tu chon ma document (o day la uid).
        string duongTao = FirebaseMang.DuongFirestore + "/nguoichoi?documentId=" + uid;
        using (var yc = new UnityWebRequest(duongTao, "POST"))
        {
            yc.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(than));
            yc.downloadHandler = new DownloadHandlerBuffer();
            yc.SetRequestHeader("Content-Type", "application/json");
            yc.SetRequestHeader("Authorization", "Bearer " + FirebaseMang.IdToken);
            yield return yc.SendWebRequest();

            if (yc.result != UnityWebRequest.Result.Success)
            {
                xong(false, "Khong tao duoc ho so: "
                          + FirebaseMang.DichLoi(yc.downloadHandler.text));
                yield break;
            }

            CuaToi.ten = ten;
            CuaToi.email = FirebaseMang.Email ?? "";
            CuaToi.soTranChoi = CuaToi.soTranThang = 0;
            CuaToi.soQuaiDaDiet = CuaToi.soNguoiDaHa = 0;
            CuaToi.biKhoa = false;
            FirebaseMang.TenHienThi = ten;
            xong(true, null);
        }
    }

    /// <summary>
    /// Cong them thanh tich sau mot van. Luat bao mat chi cho tang, va moi lan
    /// mot tran - nen khong the goi ham nay de nhay len 99 tran thang.
    /// </summary>
    public static IEnumerator CongThanhTich(int themTranChoi, int themTranThang,
                                            int themQuaiDiet, int themNguoiHa,
                                            Action<bool> xong)
    {
        yield return FirebaseMang.BaoDamConHan();

        int tranChoi  = CuaToi.soTranChoi  + Mathf.Clamp(themTranChoi, 0, 1);
        int tranThang = CuaToi.soTranThang + Mathf.Clamp(themTranThang, 0, 1);
        int quaiDiet  = CuaToi.soQuaiDaDiet + Mathf.Clamp(themQuaiDiet, 0, 200);
        int nguoiHa   = CuaToi.soNguoiDaHa + Mathf.Clamp(themNguoiHa, 0, 3);

        // updateMask liet ke dung nhung truong duoc sua. Thieu no la Firestore
        // coi nhu ghi de ca document, va luat se tu choi vi dong den ca nhung
        // truong nguoi choi khong duoc phep sua (biKhoa, ngayTao...).
        string duong = DuongHoSo(FirebaseMang.Uid)
            + "?updateMask.fieldPaths=soTranChoi"
            + "&updateMask.fieldPaths=soTranThang"
            + "&updateMask.fieldPaths=soQuaiDaDiet"
            + "&updateMask.fieldPaths=soNguoiDaHa"
            + "&updateMask.fieldPaths=lanDangNhapCuoi";

        string than =
            "{\"fields\":{"
            + "\"soTranChoi\":{\"integerValue\":\"" + tranChoi + "\"},"
            + "\"soTranThang\":{\"integerValue\":\"" + tranThang + "\"},"
            + "\"soQuaiDaDiet\":{\"integerValue\":\"" + quaiDiet + "\"},"
            + "\"soNguoiDaHa\":{\"integerValue\":\"" + nguoiHa + "\"},"
            + "\"lanDangNhapCuoi\":{\"timestampValue\":\""
                + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\"}"
            + "}}";

        using (var yc = new UnityWebRequest(duong, "PATCH"))
        {
            yc.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(than));
            yc.downloadHandler = new DownloadHandlerBuffer();
            yc.SetRequestHeader("Content-Type", "application/json");
            yc.SetRequestHeader("Authorization", "Bearer " + FirebaseMang.IdToken);
            yield return yc.SendWebRequest();

            bool ok = yc.result == UnityWebRequest.Result.Success;
            if (ok)
            {
                CuaToi.soTranChoi = tranChoi;
                CuaToi.soTranThang = tranThang;
                CuaToi.soQuaiDaDiet = quaiDiet;
                CuaToi.soNguoiDaHa = nguoiHa;
            }
            else Debug.LogWarning("[HoSo] khong cong duoc thanh tich: " + yc.downloadHandler.text);
            xong(ok);
        }
    }

    // ================================================================
    //  DOC KIEU JSON CUA FIRESTORE
    // ================================================================
    //
    // Firestore tra ve dang:
    //   {"fields":{"ten":{"stringValue":"Quan"},"soTranChoi":{"integerValue":"3"}}}
    // JsonUtility khong doc noi kieu long nhau nay, ma keo ca Newtonsoft vao
    // chi de doc vai truong thi qua nang cho ban WebGL. Nen boc tay.

    static void DocVao(string json, HoSo ra)
    {
        ra.ten          = LayChuoi(json, "ten");
        ra.email        = LayChuoi(json, "email");
        ra.soTranChoi   = LaySo(json, "soTranChoi");
        ra.soTranThang  = LaySo(json, "soTranThang");
        ra.soQuaiDaDiet = LaySo(json, "soQuaiDaDiet");
        ra.soNguoiDaHa  = LaySo(json, "soNguoiDaHa");
        ra.biKhoa       = LayBool(json, "biKhoa");
    }

    static string LayChuoi(string json, string ten)
    {
        return LayThoTrong(json, ten, "stringValue") ?? "";
    }

    static int LaySo(string json, string ten)
    {
        string s = LayThoTrong(json, ten, "integerValue");
        int n;
        return int.TryParse(s, out n) ? n : 0;
    }

    static bool LayBool(string json, string ten)
    {
        string s = LayThoTrong(json, ten, "booleanValue");
        return s == "true";
    }

    /// <summary>
    /// Tim  "ten":{"kieu":<gia tri>}  trong chuoi JSON.
    /// Chi dung cho ho so - mot document phang, khong long nhau.
    /// </summary>
    static string LayThoTrong(string json, string ten, string kieu)
    {
        if (string.IsNullOrEmpty(json)) return null;

        int i = json.IndexOf("\"" + ten + "\"", StringComparison.Ordinal);
        if (i < 0) return null;

        int j = json.IndexOf("\"" + kieu + "\"", i, StringComparison.Ordinal);
        if (j < 0) return null;

        int k = json.IndexOf(':', j + kieu.Length + 2);
        if (k < 0) return null;
        k++;
        while (k < json.Length && (json[k] == ' ' || json[k] == '\n' || json[k] == '\r')) k++;
        if (k >= json.Length) return null;

        if (json[k] == '"')
        {
            int het = json.IndexOf('"', k + 1);
            while (het > 0 && json[het - 1] == '\\') het = json.IndexOf('"', het + 1);
            return het < 0 ? null : json.Substring(k + 1, het - k - 1);
        }

        int cuoi = k;
        while (cuoi < json.Length && json[cuoi] != ',' && json[cuoi] != '}') cuoi++;
        return json.Substring(k, cuoi - k).Trim();
    }
}
