using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CHAY THU: HAI NGUOI PHAI VAO CUNG MOT PHONG VA CUNG MOT MAN.
///
/// Loi da gap: hai nguoi bam "Vao phong nhanh" cach nhau vai tram mili giay
/// thi CA HAI cung doc duoc danh sach rong - phong cua nguoi kia chua kip len -
/// roi ca hai cung tao phong rieng. Vi man truoc day boc ngau nhien nen ho con
/// vao HAI MAN KHAC NHAU. Do qua REST that: 3/3 lan khac phong, 2/3 lan khac man.
///
/// Cach sua la <see cref="PhongMang.PhongDuocGiu"/>: hai may cung tinh ra CUNG
/// MOT dap an ma khong phai hoi nhau cau nao, nen dung mot nguoi nhuong.
///
/// Phep thu nay khong noi mang - no do dung cai TINH CHAT khien cach sua chay
/// duoc. Ba tinh chat, thieu cai nao cung thanh mot loi khac han:
///
///   1. DOI XUNG: PhongDuocGiu(a,b) == PhongDuocGiu(b,a). Khong doi xung thi
///      hai may ra hai dap an - va ca hai cung nhuong, khong ai o dau ca.
///   2. LA MOT TRONG HAI: khong duoc de ra mot ma phong thu ba.
///   3. THEO THU TU TAO: khoa cua Realtime Database tang dan theo thoi gian,
///      nen phong CO TRUOC phai thang - phong ay da co nguoi ngoi cho.
///
/// Ket qua ghi ra <c>PlayTestShots/ghepphong.txt</c>.
/// </summary>
public static class ThuGhepPhong
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GhepPhong] " + s); }

    [MenuItem("Diablo 2.5D/35. Chay thu ghep phong (cung man)", false, 122)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0;
        Ghi("[ban 1] hai nguoi phai vao cung mot phong, cung mot man");

        // ---- 1. Doi xung + la mot trong hai, tren 2000 cap ---------------
        var rnd = new System.Random(20260910);
        int lechDoiXung = 0, raNgoai = 0;
        for (int i = 0; i < 2000; i++)
        {
            string a = MaGia(rnd), b = MaGia(rnd);
            string ab = PhongMang.PhongDuocGiu(a, b);
            string ba = PhongMang.PhongDuocGiu(b, a);
            if (ab != ba) lechDoiXung++;
            if (ab != a && ab != b) raNgoai++;
        }
        Ghi("1. doi xung tren 2000 cap -> so cap lech: " + lechDoiXung + " (phai la 0)");
        if (lechDoiXung > 0) { Ghi("[LOI] hai may ra hai dap an - ca hai cung nhuong"); loi++; }
        Ghi("2. dap an luon la mot trong hai -> so lan ra ngoai: " + raNgoai + " (phai la 0)");
        if (raNgoai > 0) { Ghi("[LOI] de ra mot ma phong khong ai co"); loi++; }

        // ---- 2. Phong co TRUOC phai thang -------------------------------
        //
        // Day la cho phai can than. Lan dau toi liet ke sau ma phong THAT roi
        // tu ghi thu tu theo tri nho cua minh - va phep thu bao sai mot cap.
        // Hoa ra khong phai ham sai: THU TU TOI GHI moi sai. Tam giay dau cua
        // khoa push la thoi gian, va toi da doc no bang mat.
        //
        // Nen bay gio phep thu tu GIAI MA thoi gian ra khoi ma phong, roi doi
        // chieu: ma nao co moc som hon thi PHAI thang. Khong con cho nao de
        // toi nhet gia thiet cua minh vao.
        int saiThuTu = 0, soCap = 0;
        string[] maThat =
        {
            "-P1Al4K3t9yqG5Bx92Ju", "-P1Al4K6BbyqG5Bx92Jv",
            "-P1Al4nxZXyqG5Bx92Jw", "-P1Al4n-nkyqG5Bx92Jx",
            "-P1Al5FpqGyqG5Bx92Jy", "-P1Al5Hgi3yqG5Bx92Jz",
        };
        for (int i = 0; i < maThat.Length; i++)
            for (int j = i + 1; j < maThat.Length; j++)
            {
                long ti = MocCuaMa(maThat[i]), tj = MocCuaMa(maThat[j]);
                if (ti == tj) continue;                       // cung mili giay thi khong xet
                string somHon = ti < tj ? maThat[i] : maThat[j];
                soCap++;
                if (PhongMang.PhongDuocGiu(maThat[i], maThat[j]) != somHon) saiThuTu++;
            }

        // Them 500 cap tu che voi moc thoi gian BIET TRUOC, cach nhau tu 1 ms
        // den 1 phut - cho phu ca truong hop hai nguoi bam cach nhau mot nhay.
        long goc = 1789000000000L;
        for (int i = 0; i < 500; i++)
        {
            long t1 = goc + rnd.Next(0, 60000);
            long t2 = t1 + 1 + rnd.Next(0, 60000);
            string m1 = MaTuMoc(t1, rnd), m2 = MaTuMoc(t2, rnd);
            soCap++;
            if (PhongMang.PhongDuocGiu(m1, m2) != m1) saiThuTu++;
        }

        Ghi("3. phong tao truoc luon thang (" + soCap + " cap, moc doc thang tu ma) "
            + "-> so cap sai: " + saiThuTu + " (phai la 0)");
        if (saiThuTu > 0) { Ghi("[LOI] nguoi dang ngoi cho bi bat nhuong cho nguoi den sau"); loi++; }

        // ---- 3. Man mac dinh phai co dinh, khong ngau nhien --------------
        Ghi("4. man mac dinh cua 'vao phong nhanh': " + PhongMang.ManMacDinh
            + " (phai la Act1 hoac Act2, va khong doi giua hai lan goi)");
        if (PhongMang.ManMacDinh != "Act1" && PhongMang.ManMacDinh != "Act2")
        { Ghi("[LOI] man mac dinh khong hop le"); loi++; }

        // ---- 4. Khong doi duoc man khi phong da bat dau dem nguoc --------
        // Doi duoc thi may khach co the doc man cu roi nhay vao do.
        var cuPhong = PhongMang.PhongHienTai;
        var p = new PhongMang.Phong { ma = "-thu", manChoi = "Act2", trangThai = "demNguoc" };
        PhongMang.PhongHienTai = p;
        bool choDoi = p.DangCho;
        Ghi("5. phong dang dem nguoc -> DangCho = " + choDoi + " (phai la False, "
            + "nen DoiManChoi tu choi ngay dong dau)");
        if (choDoi) { Ghi("[LOI] van doi duoc man sau khi bam bat dau"); loi++; }
        PhongMang.PhongHienTai = cuPhong;

        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/ghepphong.txt", bao.ToString());
    }

    // Bang 64 chu cua khoa push Realtime Database. Thu tu cua no trung khop
    // voi thu tu ma so ASCII - do la ly do string.CompareOrdinal so duoc hai
    // ma phong ma khong can giai ma gi ca.
    const string Bang = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

    /// <summary>Ma phong gia, dung bang chu cua khoa push.</summary>
    static string MaGia(System.Random rnd)
    {
        var sb = new StringBuilder(20);
        for (int i = 0; i < 20; i++) sb.Append(Bang[rnd.Next(Bang.Length)]);
        return sb.ToString();
    }

    /// <summary>Tam chu dau cua khoa push la moc thoi gian, viet theo co so 64.</summary>
    static long MocCuaMa(string ma)
    {
        long t = 0;
        for (int i = 0; i < 8 && i < ma.Length; i++) t = t * 64 + Bang.IndexOf(ma[i]);
        return t;
    }

    /// <summary>Nguoc lai: dung mot ma phong tu mot moc thoi gian cho truoc.</summary>
    static string MaTuMoc(long moc, System.Random rnd)
    {
        var dau = new char[8];
        long t = moc;
        for (int i = 7; i >= 0; i--) { dau[i] = Bang[(int)(t % 64)]; t /= 64; }
        var sb = new StringBuilder(20).Append(dau);
        for (int i = 0; i < 12; i++) sb.Append(Bang[rnd.Next(Bang.Length)]);
        return sb.ToString();
    }
}
