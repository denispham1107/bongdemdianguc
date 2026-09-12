using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CHAY THU: BAN PHIM AO KHONG DUOC CHE O NHAP.
///
/// Nguoi dung gap (12/09/2026, dien thoai): cham vao o nhap o man dang nhap thi
/// ban phim truot len CHE KIN ca khung - go ma khong thay minh go gi.
///
/// Do bang SO, tren HAM THUAN <see cref="ManDangNhap.TinhBoCuc"/> (khong can
/// vao Play, khong can dien thoai):
///
///   A. Khong co ban phim: bo cuc giu nguyen nhu cu - co anh tieu de, ca khoi
///      nam gon trong man hinh.
///   B. Co ban phim (che 35%, 45%, 55% man hinh): khung dang nhap phai nam TRON
///      trong phan con nhin thay, va phai cao hon mep ban phim.
///   C. O NHAP CUOI CUNG (mat khau) - cai de bi che nhat - cung phai nam tren
///      mep ban phim.
///   D. Trang "tao tai khoan" cao hon (them o ten) - cung phai lot.
///
/// Chay tren 8 co man hinh dien thoai / may tinh bang, ca ngang lan doc.
/// Ket qua ghi ra <c>PlayTestShots/banphimao.txt</c>.
/// </summary>
public static class ThuBanPhimAo
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;

    /// <summary>Ti le anh tieu de (rong / cao) - lay theo anh that trong Resources.</summary>
    const float TiLeAnh = 4.2f;

    static readonly Vector2[] ManHinh =
    {
        new Vector2(2000f, 920f),     // dien thoai ngang (anh nguoi dung gui)
        new Vector2(1170f, 540f),
        new Vector2(844f, 390f),
        new Vector2(1920f, 1080f),
        new Vector2(390f, 844f),      // dien thoai doc
        new Vector2(540f, 1170f),
        new Vector2(768f, 1024f),     // may tinh bang doc
        new Vector2(1024f, 768f),
    };

    /// <summary>Ban phim thuong chiem tu mot phan ba den quá nua man hinh.</summary>
    static readonly float[] MucChe = { 0.35f, 0.45f, 0.55f };

    [MenuItem("Diablo 2.5D/57. Chay thu BAN PHIM AO (o nhap khong bi che)", false, 148)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0;
        Ghi("[ban 2] ban phim ao khong duoc che o nhap");

        // ---- A. Khong co ban phim: nhu cu ----
        Ghi("");
        Ghi("A. khong co ban phim (che = 0)");
        int loiA = 0;
        foreach (var m in ManHinh)
            foreach (bool dangKy in new[] { false, true })
            {
                float s = TiLe(m);
                var b = ManDangNhap.TinhBoCuc(m.x, m.y, s, dangKy, 0f, TiLeAnh);
                if (!b.hienTieuDe) { Loi("man " + m + ": mat anh tieu de du khong co ban phim"); loiA++; }
                if (b.khung.yMax > m.y + 0.5f)
                { Loi("man " + m + (dangKy ? " (dang ky)" : "") + ": khung tran khoi man hinh"); loiA++; }
            }
        Ghi("8 co man hinh x 2 trang: " + loiA + " loi");

        // ---- B, C, D. Co ban phim ----
        Ghi("");
        Ghi("B. co ban phim - khung va o nhap cuoi phai nam tren mep ban phim");
        int soDo = 0, loiB = 0;
        float duNhoNhat = 999999f;
        string choChat = "";

        foreach (var m in ManHinh)
            foreach (float che in MucChe)
                foreach (bool dangKy in new[] { false, true })
                {
                    float s = TiLe(m);
                    var b = ManDangNhap.TinhBoCuc(m.x, m.y, s, dangKy, che, TiLeAnh);
                    float mepBanPhim = m.y * (1f - che);
                    soDo++;

                    if (b.hienTieuDe)
                    { Loi("man " + m + " che " + che + ": van ve anh tieu de - khong du cho"); loiB++; }

                    // Duoc phep tran len tren TOI DA bang hang tab - mat tab
                    // thi con dong duoc, mat o nhap thi khong.
                    if (b.khung.y < -ManDangNhap.TranLenToiDa(s) - 0.5f)
                    { Loi("man " + m + " che " + che + ": khung tran len khoi mep tren qua nhieu"); loiB++; }

                    // O de bi che nhat la o MAT KHAU - o cuoi cung
                    float du = mepBanPhim - b.oNhapCuoi.yMax;
                    if (du < duNhoNhat) { duNhoNhat = du; choChat = m + " che " + che + (dangKy ? " (dang ky)" : ""); }
                    if (du < 0f)
                    {
                        Loi("man " + m + " che " + che + (dangKy ? " (dang ky)" : "")
                            + ": o nhap cuoi bi ban phim che " + (-du).ToString("F0") + " diem");
                        loiB++;
                    }

                    // O NHAP DAU TIEN (ten khi dang ky, email khi dang nhap) cung
                    // phai con nhin thay - tran len khoi mep tren la go mu
                    float dinhONhapDau = b.khung.y + (dangKy ? 152f : 152f) * s;
                    if (dinhONhapDau < 0f)
                    {
                        Loi("man " + m + " che " + che + (dangKy ? " (dang ky)" : "")
                            + ": o nhap dau tien tran len khoi mep tren");
                        loiB++;
                    }
                }

        Ghi("do " + soDo + " truong hop (8 co man hinh x 3 muc che x 2 trang): " + loiB + " loi");
        Ghi("cho chat nhat: " + choChat + " - o nhap cuoi con cach mep ban phim "
            + duNhoNhat.ToString("F0") + " diem");

        // ---- E. Cang che nhieu thi khung cang len cao ----
        Ghi("");
        Ghi("E. ban phim cao hon thi khung phai len cao hon");
        var m0 = new Vector2(2000f, 920f);
        float s0 = TiLe(m0);
        float truoc = float.MaxValue;
        var sb = new StringBuilder();
        foreach (float che in new[] { 0f, 0.35f, 0.45f, 0.55f })
        {
            var b = ManDangNhap.TinhBoCuc(m0.x, m0.y, s0, false, che, TiLeAnh);
            sb.Append("che ").Append(che).Append(" -> khung y=").Append(b.khung.y.ToString("F0")).Append("  ");
            if (che > 0f && b.khung.y > truoc + 0.5f)
                Loi("che " + che + ": khung lai tut xuong thap hon muc che nho hon");
            truoc = b.khung.y;
        }
        Ghi(sb.ToString());

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/banphimao.txt", bao.ToString());
        Debug.Log("[BanPhimAo]\n" + bao);
    }

    /// <summary>Ti le giao dien - goi DUNG ham cua GiaoDien, khong chep lai cong thuc.</summary>
    static float TiLe(Vector2 m) { return GiaoDien.TinhTiLe(m.x, m.y); }

    static void Ghi(string s) { bao.AppendLine(s); }
    static void Loi(string s) { bao.AppendLine("[LOI] " + s); loi++; }
}
