using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MAY BOT TRONG PHONG - phan nhan dang (nguoi dung 08/10/2026: "chu phong nhan vao ghe trong thi them may BOT AI").
///
/// BOT la MOT GHE trong phong nhu nguoi that: phong/{ma}/nguoiChoi/{uid} voi uid bat dau "bot_". Luat Firebase da cho
/// chu phong ghi moi o nguoiChoi/* (de duoi nguoi / chuyen doi) nen KHONG can doi luat. Chi cho phep 5 truong ten / sanSang /
/// cho / doi / vaoLuc ($khac bi chan) - nen DO KHO nam ngay trong uid: "bot_d_..." de, "bot_t_..." thuong, "bot_k_...".
/// Uid that cua Firebase Auth chi co chu + so (28 ky tu), khong bao gio co dau "_" - nen tien to "bot_" khong trung ai.
///
/// BOT chay tren MAY CHU PHONG (chu phong da la tram trung chuyen moi goi tin) - phan ay o buoc 2.
/// Nguoi dung chon (08/10/2026): chon DO KHO khi them; moi BOT mot HE chinh ngau nhien; BOT tinh nhu nguoi khi ket tran
/// nhung KHONG luu thanh tich; lam tung buoc.
/// </summary>
public static class MayBot
{
    public const string TienTo = "bot_";

    public const int De = 0, Thuong = 1, Kho = 2;
    static readonly char[] MaDoKho = { 'd', 't', 'k' };

    /// <summary>Ghe nay la may BOT.</summary>
    public static bool LaBot(string uid)
    {
        return uid != null && uid.Length >= 7 && uid.StartsWith(TienTo) && uid[5] == '_'
            && System.Array.IndexOf(MaDoKho, uid[4]) >= 0;
    }

    /// <summary>Do kho doc tu uid; khong phai BOT thi -1.</summary>
    public static int DoKhoCua(string uid)
    {
        if (!LaBot(uid)) return -1;
        return System.Array.IndexOf(MaDoKho, uid[4]);
    }

    public static string TenDoKho(int doKho)
    {
        return doKho == De ? "DỄ" : doKho == Kho ? "KHÓ" : "THƯỜNG";
    }

    public static Color MauDoKho(int doKho)
    {
        return doKho == De ? new Color(0.55f, 0.85f, 0.55f)
             : doKho == Kho ? new Color(1.00f, 0.42f, 0.32f)
             : new Color(0.95f, 0.80f, 0.40f);
    }

    /// <summary>Uid moi cho mot BOT: "bot_k_" + 8 ky tu ngau nhien (khoa Firebase hop le).</summary>
    public static string TaoUid(int doKho)
    {
        const string KyTu = "abcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new System.Text.StringBuilder(TienTo);
        sb.Append(MaDoKho[Mathf.Clamp(doKho, De, Kho)]).Append('_');
        for (int i = 0; i < 8; i++) sb.Append(KyTu[Random.Range(0, KyTu.Length)]);
        return sb.ToString();
    }

    /// <summary>Ten hien thi: "BOT 1", "BOT 2"... - so nho nhat chua co BOT nao trong phong dung.</summary>
    public static string TenMoi(List<PhongMang.NguoiTrongPhong> ds)
    {
        for (int so = 1; so < 100; so++)
        {
            string ten = "BOT " + so;
            bool trung = false;
            if (ds != null) foreach (var n in ds) if (n != null && n.ten == ten) { trung = true; break; }
            if (!trung) return ten;
        }
        return "BOT";
    }

    /// <summary>So nguoi THAT (khong tinh BOT) trong mot danh sach ghe.</summary>
    public static int DemNguoiThat(List<PhongMang.NguoiTrongPhong> ds)
    {
        int n = 0;
        if (ds != null) foreach (var x in ds) if (x != null && !LaBot(x.uid)) n++;
        return n;
    }
}
