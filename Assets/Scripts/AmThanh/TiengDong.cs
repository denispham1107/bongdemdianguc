using UnityEngine;

/// <summary>
/// TIENG DONG (11/10/2026, nguoi dung): MOI tieng dong trong game di qua day - am luong theo thanh "Hiệu ứng" trong CAI DAT &gt;
/// Âm thanh (<see cref="CaiDatAmThanh.HieuUngDangNghe"/>, mac dinh 80%). Them tieng moi (ky nang, quai...) thi cung phat qua day
/// (hoac nhan voi HieuUngDangNghe) - khong thi thanh Hiệu ứng khong dieu khien duoc no.
///
/// Hien co TIENG BAM NUT (nguoi dung chon mau "Đá mộ" trong 3 mau, hai tieng):
///   - <see cref="Bam"/>: moi nut o dang nhap, dang ky, sanh / phong, nut giao dien trong tran (Sach phep, con mat, Thoat tran,
///     nut trong bang). Nut TUNG KY NANG va can dieu khien KHONG keu (nguoi dung chon).
///   - <see cref="XacNhan"/>: nut chinh - VÀO GAME, TẠO TÀI KHOẢN, OK, TẠO PHÒNG, SẴN SÀNG, MỞ KHÓA / NÂNG CẤP (ke ca dau "+").
/// File: Resources/AmThanh/HieuUng/BamThuong.wav, BamXacNhan.wav - chep nguyen byte tu CongCu/AmThanh/TiengBamThu/A_da_mo_*.wav
/// (CongCu/AmThanh/tieng_bam.py, numpy, STEREO). Nang luong chinh 200 Hz - 1 kHz: loa dien thoai phat duoc.
///
/// Mot nguon 2D tren vat the <see cref="NhacNen"/> (song qua cac man), PlayOneShot. Moi khung chi MOT tieng bam (cu cham tren WebGL
/// sinh ca su kien cham lan chuot - khong keu doi); xac nhan thang bam thuong cung khung. Dang chay phep thu (co ChayThuMang) thi
/// IM - tru <see cref="NhacNen.BatTrongPhepThu"/>; van dem <see cref="SoLan"/> de phep thu kiem.
/// </summary>
public static class TiengDong
{
    public const string DuongBamThuong = "AmThanh/HieuUng/BamThuong";
    public const string DuongBamXacNhan = "AmThanh/HieuUng/BamXacNhan";

    public enum Loai { BamThuong = 0, BamXacNhan = 1 }

    static readonly AudioClip[] clip = new AudioClip[2];
    static readonly string[] duong = { DuongBamThuong, DuongBamXacNhan };
    static int khungCuoi = -1;
    static Loai loaiCuoi;

    /// <summary>Dem so tieng da phat theo loai (phep thu doc / dat lai).</summary>
    public static readonly int[] SoLan = new int[2];
    public static Loai? LoaiGanNhat;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong() { khungCuoi = -1; SoLan[0] = 0; SoLan[1] = 0; LoaiGanNhat = null; }

    /// <summary>
    /// NAP SAN hai tieng bam (NhacNen goi khi scene dau tien da nap). Tren WebGL clip giai ma BAT DONG BO: nap luc bam thi lan bam
    /// DAU TIEN mat tieng (do tren trang that 11/10/2026: lan 1 khong co buffer nao duoc phat, lan 2 moi keu).
    /// </summary>
    public static void NapTruoc()
    {
        for (int i = 0; i < 2; i++)
        {
            if (clip[i] == null) clip[i] = Resources.Load<AudioClip>(duong[i]);
            if (clip[i] != null && clip[i].loadState == AudioDataLoadState.Unloaded) clip[i].LoadAudioData();
        }
    }

    /// <summary>Tieng bam nut thuong.</summary>
    public static void Bam() { Phat(Loai.BamThuong); }

    /// <summary>Tieng bam nut CHINH (xac nhan).</summary>
    public static void XacNhan() { Phat(Loai.BamXacNhan); }

    public static void Phat(Loai loai)
    {
        // Mot tieng moi khung; xac nhan thang thuong (vd. nut "+" goi ca hai duong)
        if (Time.frameCount == khungCuoi && (loai <= loaiCuoi)) return;
        bool thayThe = Time.frameCount == khungCuoi;
        khungCuoi = Time.frameCount;
        loaiCuoi = loai;
        if (thayThe) SoLan[0]--;
        SoLan[(int)loai]++;
        LoaiGanNhat = loai;

        var nn = NhacNen.DamBao();
        if (nn == null) return;
        int i = (int)loai;
        // Clip tinh: thoat Play trong Editor la clip bi huy - kiem bang null cua Unity, khong co co "da nap"
        if (clip[i] == null) clip[i] = Resources.Load<AudioClip>(duong[i]);
        if (clip[i] == null) { Debug.LogWarning("[TiengDong] thieu " + duong[i]); return; }
        nn.PhatHieuUng(clip[i], CaiDatAmThanh.HieuUngDangNghe, thayThe);
    }
}
