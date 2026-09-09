using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NHAN VAT CUA MINH PHAI NHUC NHICH NGAY KHI BAM, KHONG DOI MANG.
///
/// Neu cho trong tai xac nhan roi moi di, thi voi 28 ms do tre (so do that o
/// buoc 0) nguoi choi bam mot cai phai doi 56 ms moi thay nhan vat cua chinh
/// minh nhuc nhich. Cam giac "nang tay" xuat hien ngay lap tuc.
///
/// Nen may nguoi choi thi hanh NGAY, va nho lai nhung goi input chua duoc
/// trong tai xac nhan. Khi trong tai bao "toi da xu ly toi so N, ket qua la
/// day":
///   - dat nhan vat ve dung trang thai do,
///   - CHAY LAI cac goi tu N+1 den hien tai,
///   - neu ra dung cho cu thi nguoi choi khong thay gi ca.
///
/// Chi khi trong tai bat duoc dieu gi khac (va cham, bi day lui, an don) thi
/// vi tri moi lech - luc do keo nhan vat ve tu tu chu khong giat mot phat.
///
/// DIEU KIEN SONG CON: cung mot chuoi input, chay lai phai ra CUNG MOT ket
/// qua. Khong tat dinh thi moi lan hieu chinh nhan vat lai nhay mot doan, ma
/// hieu chinh thi xay ra vai chuc lan moi giay. Menu 31 do dung dieu nay.
/// </summary>
public class DuDoan
{
    /// <summary>
    /// Giu lai nhieu nhat bao nhieu goi chua duoc xac nhan.
    ///
    /// 120 goi o 60 khung/giay la 2 giay - du cho ca luc mang chap chon nhat
    /// (do duoc o buoc 0: co goi mat toi 138 ms). Giu nhieu hon thi ton bo nho
    /// ma khong duoc gi: qua 2 giay khong nghe tin trong tai thi van de khong
    /// con la hieu chinh nua.
    /// </summary>
    public const int SoGoiGiuLai = 120;

    /// <summary>
    /// Lech duoi nguong nay thi coi nhu khop, khong dung den.
    ///
    /// Khong the doi khop tuyet doi: phep tinh dau phay dong tren hai may
    /// khong bao gio ra so giong het nhau den chu so cuoi. Doi bang nhau
    /// tuyet doi thi khung hinh nao cung "lech" va nhan vat rung lien tuc.
    /// </summary>
    public const float NguongLechMet = 0.05f;

    /// <summary>Lech qua ngan nay thi nhay thang ve, khong keo tu tu nua -
    /// keo mot quang xa qua thi nhan vat truot dai nhu di bang.</summary>
    public const float NguongNhayThangMet = 4f;

    struct MotBuoc
    {
        public GoiInput goi;
        public TrangThaiNhanVat sau;   // trang thai NGAY SAU khi thi hanh goi do
    }

    readonly List<MotBuoc> lichSu = new List<MotBuoc>(SoGoiGiuLai + 8);
    readonly PlayerController nhanVat;

    // ---- So dem de chan doan, doc duoc tu ngoai ----
    public int SoLanHieuChinh { get; private set; }
    public int SoLanChayLai { get; private set; }
    public float LechLonNhat { get; private set; }
    public float LechGanNhat { get; private set; }
    public int SoGoiDangGiu { get { return lichSu.Count; } }

    public DuDoan(PlayerController nv) { nhanVat = nv; }

    /// <summary>
    /// Ghi lai mot goi vua duoc thi hanh, kem trang thai sau do.
    /// Goi NGAY SAU <c>ThiHanhMotKhung</c>.
    /// </summary>
    public void GhiNhan(GoiInput goi)
    {
        var b = new MotBuoc();
        b.goi = goi;
        b.sau = nhanVat.ChupTrangThai();
        b.sau.soThuTu = goi.soThuTu;
        lichSu.Add(b);

        // Cat bot dau danh sach cho khoi phinh mai
        if (lichSu.Count > SoGoiGiuLai)
            lichSu.RemoveRange(0, lichSu.Count - SoGoiGiuLai);
    }

    /// <summary>
    /// Trong tai bao: "den goi so <paramref name="thatSu"/>.soThuTu thi su
    /// that la day". Doi chieu, va neu lech thi dat lai roi chay lai.
    /// </summary>
    /// <returns>Do lech phat hien duoc, tinh bang met.</returns>
    public float HieuChinh(TrangThaiNhanVat thatSu)
    {
        SoLanHieuChinh++;

        // Tim goi tuong ung trong lich su cua minh
        int i = lichSu.FindIndex(b => b.sau.soThuTu == thatSu.soThuTu);
        if (i < 0)
        {
            // Khong con nho goi nay - hoac qua cu, hoac chua tung co. Tin
            // trong tai va bat dau lai tu day.
            nhanVat.DatTrangThai(thatSu);
            lichSu.Clear();
            LechGanNhat = 0f;
            return 0f;
        }

        float lech = lichSu[i].sau.LechVoi(thatSu);
        LechGanNhat = lech;
        if (lech > LechLonNhat) LechLonNhat = lech;

        // Khop roi thi chi can quen di nhung goi da duoc xac nhan
        if (lech <= NguongLechMet)
        {
            lichSu.RemoveRange(0, i + 1);
            return lech;
        }

        // Lech that: dat ve su that roi chay lai nhung goi sau no
        nhanVat.DatTrangThai(thatSu);

        for (int j = i + 1; j < lichSu.Count; j++)
        {
            nhanVat.ThiHanhMotKhung(lichSu[j].goi);
            SoLanChayLai++;

            // Cap nhat lai trang thai da luu: tu gio ve sau, day moi la duong
            // di dung. Khong cap nhat thi lan hieu chinh sau doi chieu voi mot
            // duong di da bi bo.
            var b = lichSu[j];
            b.sau = nhanVat.ChupTrangThai();
            b.sau.soThuTu = b.goi.soThuTu;
            lichSu[j] = b;
        }

        lichSu.RemoveRange(0, i + 1);
        return lech;
    }

    /// <summary>
    /// Trang thai ma may nay da tinh ra tai goi so <paramref name="soThuTu"/>.
    ///
    /// Mo cua nay cho phep chay thu dong vai trong tai: no lay dung cai minh
    /// da tinh roi bao nguoc lai la "su that", de kiem rang khop thi nhan vat
    /// khong nhuc nhich. Trong tran that thi con so nay den tu may kia.
    /// </summary>
    public TrangThaiNhanVat TrangThaiTaiGoi(int soThuTu)
    {
        for (int i = 0; i < lichSu.Count; i++)
            if (lichSu[i].sau.soThuTu == soThuTu) return lichSu[i].sau;
        return new TrangThaiNhanVat { soThuTu = soThuTu };
    }

    /// <summary>Quen sach - dung khi vao tran moi hoac hoi sinh.</summary>
    public void Xoa()
    {
        lichSu.Clear();
        LechGanNhat = 0f;
    }
}
