using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CHE DO TRAN: "ĐƠN" va "ĐÔI" (nguoi dung 28/09/2026).
///
///   - ĐƠN: moi nguoi mot minh, giet nhau, nguoi song sot cuoi cung thang (luat cu cua KetTran). Toi da 6 nguoi.
///   - ĐÔI: hai doi, ĐỘI A va ĐỘI B, moi doi toi da 3 nguoi. Doi nao con nguoi song cuoi cung thi thang
///     (vd cuoi tran doi A con 2, doi B chet het -> doi A thang).
///
/// Nguoi dung chon (28/09/2026):
///   - dong doi KHONG gay sat thuong va KHONG dinh hieu ung cua nhau; ky nang tu nham bo qua dong doi;
///   - dong doi xuat phat GAN NHAU, hai doi o hai phia ban do (<see cref="ChoXuatPhat.ChoChoDoi"/>);
///   - ten tren dau theo MAU DOI (A xanh duong, B do) kem chu "ĐỘI A/B"; HUD ghi so nguoi con song moi doi;
///   - vao phong Doi thi tu vao doi IT NGUOI HON (bang nhau -> A), roi tu bam doi doi hoac chu phong chuyen;
///   - chu phong bat dau luc nao cung duoc; vao tran ma ca phong cung mot doi thi doi ay THANG NGAY.
///
/// Tren Firebase: phong/{ma}/cheDo = "don" | "doi"; phong/{ma}/nguoiChoi/{uid}/doi = 0 | 1. Phong cu khong co
/// cheDo -> Don. Trong tran: <see cref="Damageable.doi"/> cua moi nhan vat nguoi choi (-1 = khong doi nao).
///
/// Cac ham o day la ham THUAN (tru <see cref="LaDongDoi"/> doc truong cua Damageable) de con kiem duoc bang so (menu 92).
/// </summary>
public static class CheDoTran
{
    public const string Don = "don";
    public const string Doi = "doi";

    public const sbyte DoiA = 0, DoiB = 1, KhongDoi = -1;

    /// <summary>Moi doi toi da bay nhieu nguoi (nguoi dung: "chia deu co dinh moi doi chi duoc toi da 3 nguoi").</summary>
    public const int SoNguoiMoiDoi = 3;

    /// <summary>Tran DANG CHOI co phai che do Doi khong (KhoiDongTranMang dat luc vao tran, TranHienTai.Xoa tra ve false).</summary>
    public static bool LaTranDoi;

    public static bool LaCheDoDoi(string cheDo) { return cheDo == Doi; }

    /// <summary>Ten hien cho nguoi choi: "ĐƠN" / "ĐÔI".</summary>
    public static string TenCheDo(string cheDo) { return LaCheDoDoi(cheDo) ? "ĐÔI" : "ĐƠN"; }

    /// <summary>Mot dong giai thich luat, hien trong phong.</summary>
    public static string LuatCheDo(string cheDo)
    {
        return LaCheDoDoi(cheDo)
            ? "hai đội, mỗi đội tối đa 3 người"
            : "mỗi người một mình";
    }

    public static string TenDoi(int doi) { return doi == DoiA ? "ĐỘI A" : doi == DoiB ? "ĐỘI B" : ""; }

    public static readonly Color MauDoiA = new Color(0.42f, 0.68f, 1.00f);
    public static readonly Color MauDoiB = new Color(1.00f, 0.38f, 0.30f);

    public static Color MauDoi(int doi) { return doi == DoiA ? MauDoiA : doi == DoiB ? MauDoiB : Color.white; }

    public static int DoiKia(int doi) { return doi == DoiA ? DoiB : DoiA; }

    // ================================================================
    //  DONG DOI TRONG TRAN
    // ================================================================

    /// <summary>
    /// Hai nhan vat NGUOI CHOI khac nhau cung mot doi. Doi luu tren tung Damageable (<see cref="Damageable.doi"/>) - may nao
    /// cung gan cho nhan vat cua minh lan ban sao cua nguoi khac tu bang ghe cua phong, nen hoi o may nao cung ra mot dap an.
    /// Che do Don moi nguoi deu -1 nen luon false.
    /// </summary>
    public static bool LaDongDoi(Damageable a, Damageable b)
    {
        if (a == null || b == null || a == b) return false;
        if (!a.isPlayer || !b.isPlayer) return false;
        return a.doi >= 0 && a.doi == b.doi;
    }

    /// <summary>
    /// Ky nang cua <paramref name="nguoiTung"/> bo qua <paramref name="d"/>: chinh nguoi tung, hoac dong doi cua ho.
    /// Thay cho moi cho "d == boQua" trong ky nang - de ky nang tu nham khong chon dong doi, va vung no khong dinh dong doi.
    /// </summary>
    public static bool BoQua(Damageable nguoiTung, Damageable d)
    {
        if (nguoiTung == null) return false;
        return d == nguoiTung || LaDongDoi(nguoiTung, d);
    }

    // ================================================================
    //  XEP DOI TRONG PHONG (ham thuan)
    // ================================================================

    public static int DemDoi(List<PhongMang.NguoiTrongPhong> ds, int doi, string truUid = null)
    {
        int n = 0;
        if (ds == null) return 0;
        foreach (var x in ds) if (x != null && x.doi == doi && x.uid != truUid) n++;
        return n;
    }

    /// <summary>Nguoi <paramref name="uid"/> vao phong Doi thi vao doi nao: doi IT NGUOI HON (khong dem chinh ho), bang nhau -> A.</summary>
    public static sbyte DoiKhiVao(List<PhongMang.NguoiTrongPhong> ds, string uid)
    {
        int a = DemDoi(ds, DoiA, uid), b = DemDoi(ds, DoiB, uid);
        return b < a ? DoiB : DoiA;
    }

    /// <summary>Nguoi nay chuyen sang doi <paramref name="doiMoi"/> duoc khong (doi ay con cho).</summary>
    public static bool ChuyenDuoc(List<PhongMang.NguoiTrongPhong> ds, string uid, int doiMoi)
    {
        if (doiMoi != DoiA && doiMoi != DoiB) return false;
        return DemDoi(ds, doiMoi, uid) < SoNguoiMoiDoi;
    }

    /// <summary>
    /// CHU PHONG CAN BANG DOI: tra ve danh sach (uid, doi moi) can ghi.
    ///
    /// Hai truong hop:
    ///   1. Nguoi CHUA CO DOI (ban game cu khong ghi "doi", hoac ghi hong) -> doi it nguoi hon.
    ///   2. Mot doi QUA 3 nguoi - hai nguoi vao cung luc cung doc thay doi B con cho roi cung ngoi vao (cuoc dua y het nut
    ///      "Vao phong nhanh"). Nguoi VAO SAU CUNG cua doi ay sang doi kia (neu doi kia con cho).
    /// Moi may tinh ra cung mot dap an, nhung chi chu phong ghi.
    /// </summary>
    public static List<KeyValuePair<string, sbyte>> CanBangDoi(List<PhongMang.NguoiTrongPhong> ds)
    {
        var ra = new List<KeyValuePair<string, sbyte>>();
        if (ds == null) return ra;

        // Ban sao de tinh dan (ghi xong nguoi truoc thi nguoi sau thay so moi)
        var doi = new Dictionary<string, sbyte>();
        var thuTu = new List<PhongMang.NguoiTrongPhong>(ds);
        thuTu.RemoveAll(x => x == null || string.IsNullOrEmpty(x.uid));
        thuTu.Sort((x, y) =>
        {
            int c = x.vaoLuc.CompareTo(y.vaoLuc);
            return c != 0 ? c : string.CompareOrdinal(x.uid, y.uid);
        });
        foreach (var x in thuTu) doi[x.uid] = x.doi;

        System.Func<int, int> dem = d => { int n = 0; foreach (var kv in doi) if (kv.Value == d) n++; return n; };

        foreach (var x in thuTu)
        {
            if (doi[x.uid] == DoiA || doi[x.uid] == DoiB) continue;
            sbyte moi = dem(DoiB) < dem(DoiA) ? DoiB : DoiA;
            if (dem(moi) >= SoNguoiMoiDoi) moi = (sbyte)DoiKia(moi);
            doi[x.uid] = moi;
            ra.Add(new KeyValuePair<string, sbyte>(x.uid, moi));
        }

        for (int d = DoiA; d <= DoiB; d++)
        {
            int kia = DoiKia(d);
            for (int i = thuTu.Count - 1; i >= 0 && dem(d) > SoNguoiMoiDoi && dem(kia) < SoNguoiMoiDoi; i--)
            {
                var x = thuTu[i];
                if (doi[x.uid] != d) continue;
                doi[x.uid] = (sbyte)kia;
                ra.RemoveAll(kv => kv.Key == x.uid);
                ra.Add(new KeyValuePair<string, sbyte>(x.uid, (sbyte)kia));
            }
        }
        return ra;
    }

    // ================================================================
    //  THANG THUA CHE DO DOI (ham thuan)
    // ================================================================

    public const int ChuaXong = -2, KhongAiSong = -1;

    /// <summary>
    /// Doi nao thang, tu tinh trang tung ghe CO TRONG TRAN.
    ///
    /// <paramref name="doiTheoGhe"/>[i] = doi cua ghe i (-1 = ghe khong co ai); <paramref name="conSong"/>[i] = ghe i con song
    /// (ghe CHUA KIP VAO tran cung tinh la con song - nguoi ta dang tai man, khong phai da thua).
    /// Tra ve <see cref="DoiA"/> / <see cref="DoiB"/> = doi thang, <see cref="KhongAiSong"/> = hai doi deu chet het,
    /// <see cref="ChuaXong"/> = ca hai doi con nguoi song.
    ///
    /// Ca phong CHI CO MOT DOI (nguoi dung chon "doi do thang ngay") -> doi ay thang, du ai con song hay khong.
    /// </summary>
    public static int DoiThang(IList<sbyte> doiTheoGhe, IList<bool> conSong)
    {
        bool coA = false, coB = false, songA = false, songB = false;
        for (int i = 0; i < doiTheoGhe.Count; i++)
        {
            int d = doiTheoGhe[i];
            bool s = i < conSong.Count && conSong[i];
            if (d == DoiA) { coA = true; songA |= s; }
            else if (d == DoiB) { coB = true; songB |= s; }
        }
        if (!coA && !coB) return ChuaXong;
        if (coA != coB) return coA ? DoiA : DoiB;      // ca phong mot doi
        if (songA && songB) return ChuaXong;
        if (songA) return DoiA;
        if (songB) return DoiB;
        return KhongAiSong;
    }
}
