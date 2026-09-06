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
/// nam trong .gitignore. Ba dong:
///
///     thunghiem.a@vidu.com
///     thunghiem.b@vidu.com
///     matkhaucuaban
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
        string[] sach = new string[3];
        foreach (var l in dong)
        {
            string t = l.Trim();
            if (t.Length == 0 || t.StartsWith("#")) continue;   // bo dong trong va ghi chu
            if (d < 3) sach[d++] = t;
        }

        if (d < 3)
        {
            loi = "File " + TenFile + " phai co du ba dong: emailA, emailB, mat khau.";
            return false;
        }

        EmailA = sach[0];
        EmailB = sach[1];
        MatKhau = sach[2];
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
