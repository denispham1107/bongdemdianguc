using System.IO;
using UnityEngine;

/// <summary>
/// EMAIL VA MAT KHAU CUA HAI TAI KHOAN CHAY THU - DOC TU FILE NGOAI GIT.
///
/// Truoc day ba kich ban chay thu (menu 26, 27, 28) viet thang mat khau vao
/// code. Ma nguon de o repo cong khai thi ai cung dang nhap duoc vao hai tai
/// khoan do - tao phong rac, lam ban co so du lieu that.
///
/// Nen chuyen sang doc file <c>chay-thu-mang.txt</c> o goc du an, va file do
/// nam trong .gitignore. Ba dong dau bat buoc, hai dong sau tuy chon:
///
///     thunghiem.a@vidu.com        &lt;- tai khoan nguoi choi A
///     thunghiem.b@vidu.com        &lt;- tai khoan nguoi choi B
///     matkhaucuahaicaitren
///     admin@vidu.com              &lt;- TUY CHON: tai khoan admin
///     matkhaucuaadmin
///
/// Hai dong admin chi menu 27 can, de khoa/mo khoa tai khoan khac. Khong khai
/// thi menu 27 bo qua phan do va noi ro, chu khong bao la loi.
///
/// Khong co file thi cac kich ban chay thu bao ro rang chu khong im lang
/// that bai - im lang thi nhin het nhu loi mang.
/// </summary>
public static class ThongTinChayThu
{
    const string TenFile = "chay-thu-mang.txt";

    public static string EmailA { get; private set; }
    public static string EmailB { get; private set; }
    public static string MatKhau { get; private set; }

    /// <summary>Tai khoan admin - co the khong khai.</summary>
    public static string EmailAdmin { get; private set; }
    public static string MatKhauAdmin { get; private set; }

    public static bool CoTaiKhoanAdmin
    {
        get { return !string.IsNullOrEmpty(EmailAdmin) && !string.IsNullOrEmpty(MatKhauAdmin); }
    }

    /// <summary>Doc file. Tra ve false kem loi neu thieu.</summary>
    public static bool Doc(out string loi)
    {
        loi = null;

        if (!File.Exists(TenFile))
        {
            loi = "Thieu file " + TenFile + " o goc du an. Tao file do voi ba dong: "
                + "email tai khoan A, email tai khoan B, mat khau chung. "
                + "File nay nam ngoai git nen khong bi day len GitHub.";
            return false;
        }

        var dong = File.ReadAllLines(TenFile);
        int d = 0;
        string[] sach = new string[5];
        foreach (var l in dong)
        {
            string t = l.Trim();
            if (t.Length == 0 || t.StartsWith("#")) continue;   // bo dong trong va ghi chu
            if (d < 5) sach[d++] = t;
        }

        if (d < 3)
        {
            loi = "File " + TenFile + " phai co it nhat ba dong: emailA, emailB, mat khau.";
            return false;
        }

        EmailA = sach[0];
        EmailB = sach[1];
        MatKhau = sach[2];

        // Hai dong admin la tuy chon
        EmailAdmin = d >= 5 ? sach[3] : null;
        MatKhauAdmin = d >= 5 ? sach[4] : null;
        return true;
    }

    /// <summary>Doc, va neu thieu thi ghi Log loi luon.</summary>
    public static bool DocHoacBao()
    {
        string loi;
        if (Doc(out loi)) return true;
        Debug.LogError("[ChayThu] " + loi);
        return false;
    }
}
