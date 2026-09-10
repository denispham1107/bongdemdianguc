using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TINH TRUNG THEO CAI MA NGUOI BAN NHIN THAY, KHONG PHAI CAI DANG THAY O DAY.
///
/// Van de: nguoi choi A thay nguoi choi B tre mat mot khoang - do tre duong
/// truyen cong dem noi suy, do duoc 82-131 ms. A ngam vao cho A THAY B dang
/// dung, bam phep. Goi tin bay sang may B mat them mot lat nua. Den luc may B
/// tinh trung thi B da chay tiep hon mot met - va don truot, du tren man hinh
/// cua A no trung ro rang.
///
/// Cach chua, giong het cac game ban sung: may B LUI B VE QUA KHU dung bang
/// khoang A da tre, tinh trung o do, roi tra B ve cho cu. Tat ca gon trong
/// mot khung hinh, khong render o giua, nen khong ai thay nhan vat nhay.
///
/// Cai gia phai tra la "chet sau goc tuong": B nap sau vat can roi van co the
/// an mot don ma A ban tu 100 ms truoc. Do la danh doi cua moi he bu tre -
/// chon ben ban duoc thuong hay ben chay duoc thuong. Cac game ban sung deu
/// chon ben ban, vi nguoi ban la nguoi vua thao tac va se to ra khong tin neu
/// don cua ho khong an.
///
/// GIOI HAN 300 ms: bu qua nua giay thi sinh ra nhung canh vo ly ma khong ai
/// giai thich noi. Mang te den the thi tha chiu truot.
/// </summary>
public static class BuTre
{
    /// <summary>Khong bao gio lui qua ngan nay - xem ghi chu o dau lop.</summary>
    public const float LuiToiDaGiay = 0.30f;

    /// <summary>
    /// Dan bay duoc tua truoc bao nhieu giay khi sinh ra tu mot phep den tu
    /// mang. <see cref="Fireball"/> doc bien nay ngay luc sinh.
    ///
    /// Vi sao dan bay can cach rieng: lui nan nhan chi cuu duoc don tinh trung
    /// NGAY (giut set). Qua cau lua con phai bay mot doan - luc no toi noi thi
    /// cua so lui da dong tu lau. Nen thay vi lui nan nhan, ta day qua cau toi
    /// truoc dung bang thoi gian no da mat de bay sang day.
    /// </summary>
    public static float TuaTruocGiay;

    static readonly List<LichSuViTri> dangLui = new List<LichSuViTri>();
    static int soLanMo;

    /// <summary>Do chan doan: da lui bao nhieu lan, va lan gan nhat lui bao lau.</summary>
    public static int SoLanDaLui { get; private set; }
    public static float LuiLanCuoiGiay { get; private set; }

    /// <summary>
    /// Lui MOI nhan vat co ghi lich su ve <paramref name="doTreGiay"/> truoc.
    ///
    /// Chi lui nhung ai KHONG phai nguoi tung: nguoi tung o may ben kia da o
    /// dung cho ho muon roi, lui ho la lam sai chinh cai minh dang chua.
    ///
    /// PHAI goi <see cref="Dong"/> trong cung khung hinh. Quen mot lan la nhan
    /// vat ket lai o qua khu vinh vien.
    /// </summary>
    public static void Mo(float doTreGiay, Damageable boQua)
    {
        if (doTreGiay <= 0.001f) return;

        soLanMo++;
        if (soLanMo > 1) return;        // long nhau thi chi tinh lan ngoai cung

        doTreGiay = Mathf.Min(doTreGiay, LuiToiDaGiay);
        dangLui.Clear();

        foreach (var ls in Object.FindObjectsByType<LichSuViTri>(FindObjectsSortMode.None))
        {
            if (ls == null) continue;
            if (boQua != null && ls.GetComponent<Damageable>() == boQua) continue;

            ls.Lui(doTreGiay);
            dangLui.Add(ls);
        }

        // Vat ly cua Unity giu mot ban sao vi tri rieng; khong dong bo lai thi
        // OverlapSphere van thay nhan vat o cho cu, va ca viec lui thanh vo
        // nghia ma khong bao gi ca.
        if (dangLui.Count > 0) Physics.SyncTransforms();

        SoLanDaLui++;
        LuiLanCuoiGiay = doTreGiay;
    }

    public static void Dong()
    {
        if (soLanMo <= 0) return;
        soLanMo--;
        if (soLanMo > 0) return;

        for (int i = 0; i < dangLui.Count; i++)
            if (dangLui[i] != null) dangLui[i].Tra();

        if (dangLui.Count > 0) Physics.SyncTransforms();
        dangLui.Clear();
    }
}
