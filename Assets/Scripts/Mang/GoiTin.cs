using System;
using UnityEngine;

/// <summary>
/// DONG GOI TRANG THAI NHAN VAT THANH BYTE, VA MO RA.
///
/// VI SAO NHI PHAN CHU KHONG PHAI JSON: mot goi JSON cho bon nguoi khoang
/// 400 byte, nhi phan con khoang 60. Gap gan bay lan bang thong - va tren
/// mang di dong thi bang thong bien thanh DO TRE, dung thu ma ca giai doan 2
/// dang co gang cat giam tung mili giay.
///
/// GUI 20 LAN MOI GIAY, 4 NGUOI:
///   JSON     400 byte x 20 = 8 000 byte/giay
///   nhi phan  60 byte x 20 = 1 200 byte/giay
///
/// NEN VI TRI XUONG SO NGUYEN 16 BIT:
/// Ban do rong chung 70 m. Luu vi tri bang so nguyen 16 bit voi do phan giai
/// 1 cm thi phu duoc +-327 m - thua sucher, ma moi truc chi ton 2 byte thay
/// vi 4. Sai so 1 cm khong ai nhin thay, va nhan vat nguoi khac von da duoc
/// noi suy nen con muot hon nua.
///
/// GOC CHI CAN MOT TRUC: nhan vat luon dung thang, chi xoay quanh truc dung.
/// Goi ca bon so cua quaternion la phi ba phan tu.
/// </summary>
public static class GoiTin
{
    /// <summary>Do phan giai vi tri: 1 don vi = 1 cm.</summary>
    const float DonViMet = 0.01f;

    /// <summary>Loai goi - byte dau tien cua moi goi.</summary>
    public const byte LoaiTrangThai = 1;
    public const byte LoaiInput = 2;

    /// <summary>
    /// GOI KY NANG: "toi vua tung phep so may, nham vao cho nay".
    ///
    /// Vi sao phai co mot loai goi RIENG chu khong nhet vao goi trang thai:
    /// trang thai gui 60 lan moi giay va MAT DUOC PHEP - mat mot goi thi goi
    /// sau da bu ngay. Tung phep thi khac: no xay ra dung mot lan, mat la mat
    /// han, nguoi kia se thay dam lua no ra ma khong hieu tu dau. Nen goi ky
    /// nang duoc gui LAP LAI vai lan (xem DongBoTran) - re, vi no chi 14 byte
    /// va mot tran chi co vai chuc lan tung phep.
    /// </summary>
    public const byte LoaiKyNang = 3;

    /// <summary>Mot lan tung phep.</summary>
    public struct MotPhep
    {
        public byte chiSo;      // ai tung
        public byte kyNang;     // 0..6
        public int soThuTu;     // de ben nhan bo qua ban sao lap lai
        public Vector3 diemNgam;
    }

    /// <summary>Mot nguoi choi trong goi tin.</summary>
    public struct MotNguoi
    {
        public byte chiSo;          // ai - chi so trong phong, khong gui ca uid dai
        public Vector3 viTri;
        public float gocY;          // do
        public float mau01;         // 0..1
        public bool dangChay;
        public bool daChet;
    }

    // ================================================================
    //  VIET
    // ================================================================

    static short NenToaDo(float met)
    {
        float v = met / DonViMet;
        if (v > short.MaxValue) v = short.MaxValue;
        if (v < short.MinValue) v = short.MinValue;
        return (short)Mathf.RoundToInt(v);
    }

    static float MoToaDo(short s) { return s * DonViMet; }

    /// <summary>
    /// Dong goi trang thai cua nhieu nguoi.
    ///
    /// <paramref name="mocThoiGian"/> la gio may chu luc chup - ben nhan can
    /// no de noi suy. Gui bang so nguyen 32 bit mili giay ke tu luc vao tran,
    /// khong gui ca dau thoi gian Unix: 8 byte moi goi, 20 goi moi giay, bon
    /// nguoi - cong lai la mot con so dang ke cho mot thu khong ai doc.
    /// </summary>
    public static byte[] VietTrangThai(int mocThoiGian, MotNguoi[] ds, int soNguoi)
    {
        // 1 loai + 4 moc + 1 so nguoi + moi nguoi 12 byte
        var b = new byte[6 + soNguoi * 12];
        int i = 0;

        b[i++] = LoaiTrangThai;

        b[i++] = (byte)(mocThoiGian & 0xFF);
        b[i++] = (byte)((mocThoiGian >> 8) & 0xFF);
        b[i++] = (byte)((mocThoiGian >> 16) & 0xFF);
        b[i++] = (byte)((mocThoiGian >> 24) & 0xFF);

        b[i++] = (byte)soNguoi;

        for (int n = 0; n < soNguoi; n++)
        {
            var p = ds[n];
            b[i++] = p.chiSo;

            short x = NenToaDo(p.viTri.x), y = NenToaDo(p.viTri.y), z = NenToaDo(p.viTri.z);
            b[i++] = (byte)(x & 0xFF); b[i++] = (byte)((x >> 8) & 0xFF);
            b[i++] = (byte)(y & 0xFF); b[i++] = (byte)((y >> 8) & 0xFF);
            b[i++] = (byte)(z & 0xFF); b[i++] = (byte)((z >> 8) & 0xFF);

            // Goc 0..360 do nen vao 16 bit: sai so 0,005 do
            ushort g = (ushort)Mathf.RoundToInt(Mathf.Repeat(p.gocY, 360f) / 360f * 65535f);
            b[i++] = (byte)(g & 0xFF); b[i++] = (byte)((g >> 8) & 0xFF);

            b[i++] = (byte)Mathf.RoundToInt(Mathf.Clamp01(p.mau01) * 255f);

            byte co = 0;
            if (p.dangChay) co |= 1;
            if (p.daChet) co |= 2;
            b[i++] = co;
        }
        return b;
    }

    // ================================================================
    //  DOC
    // ================================================================

    /// <summary>
    /// Mo mot goi trang thai. Tra ve so nguoi doc duoc, hoac -1 neu goi hong.
    ///
    /// PHAI kiem do dai truoc khi doc: goi den tu may khac qua duong truyen
    /// khong tin cay, va mot goi bi cat doi ma cu doc bua thi nem
    /// IndexOutOfRange giua tran dau.
    /// </summary>
    public static int DocTrangThai(byte[] b, MotNguoi[] ra, out int mocThoiGian)
    {
        mocThoiGian = 0;
        if (b == null || b.Length < 6) return -1;
        if (b[0] != LoaiTrangThai) return -1;

        int i = 1;
        mocThoiGian = b[i] | (b[i + 1] << 8) | (b[i + 2] << 16) | (b[i + 3] << 24);
        i += 4;

        int soNguoi = b[i++];
        if (soNguoi > ra.Length) return -1;
        if (b.Length < 6 + soNguoi * 12) return -1;

        for (int n = 0; n < soNguoi; n++)
        {
            var p = new MotNguoi();
            p.chiSo = b[i++];

            short x = (short)(b[i] | (b[i + 1] << 8)); i += 2;
            short y = (short)(b[i] | (b[i + 1] << 8)); i += 2;
            short z = (short)(b[i] | (b[i + 1] << 8)); i += 2;
            p.viTri = new Vector3(MoToaDo(x), MoToaDo(y), MoToaDo(z));

            ushort g = (ushort)(b[i] | (b[i + 1] << 8)); i += 2;
            p.gocY = g / 65535f * 360f;

            p.mau01 = b[i++] / 255f;

            byte co = b[i++];
            p.dangChay = (co & 1) != 0;
            p.daChet = (co & 2) != 0;

            ra[n] = p;
        }
        return soNguoi;
    }

    // ================================================================
    //  GOI KY NANG
    // ================================================================

    /// <summary>Dong goi mot lan tung phep. 14 byte.</summary>
    public static byte[] VietKyNang(MotPhep p)
    {
        var b = new byte[14];
        int i = 0;

        b[i++] = LoaiKyNang;
        b[i++] = p.chiSo;
        b[i++] = p.kyNang;

        b[i++] = (byte)(p.soThuTu & 0xFF);
        b[i++] = (byte)((p.soThuTu >> 8) & 0xFF);
        b[i++] = (byte)((p.soThuTu >> 16) & 0xFF);
        b[i++] = (byte)((p.soThuTu >> 24) & 0xFF);

        short x = NenToaDo(p.diemNgam.x), y = NenToaDo(p.diemNgam.y), z = NenToaDo(p.diemNgam.z);
        b[i++] = (byte)(x & 0xFF); b[i++] = (byte)((x >> 8) & 0xFF);
        b[i++] = (byte)(y & 0xFF); b[i++] = (byte)((y >> 8) & 0xFF);
        b[i++] = (byte)(z & 0xFF); b[i++] = (byte)((z >> 8) & 0xFF);

        return b;
    }

    /// <summary>Mo mot goi ky nang. Tra ve false neu goi hong hoac khong phai
    /// loai nay - kiem do dai truoc, vi goi den qua duong truyen khong tin cay.</summary>
    public static bool DocKyNang(byte[] b, out MotPhep ra)
    {
        ra = new MotPhep();
        if (b == null || b.Length < 14) return false;
        if (b[0] != LoaiKyNang) return false;

        int i = 1;
        ra.chiSo = b[i++];
        ra.kyNang = b[i++];

        ra.soThuTu = b[i] | (b[i + 1] << 8) | (b[i + 2] << 16) | (b[i + 3] << 24);
        i += 4;

        short x = (short)(b[i] | (b[i + 1] << 8)); i += 2;
        short y = (short)(b[i] | (b[i + 1] << 8)); i += 2;
        short z = (short)(b[i] | (b[i + 1] << 8)); i += 2;
        ra.diemNgam = new Vector3(MoToaDo(x), MoToaDo(y), MoToaDo(z));

        return true;
    }

    /// <summary>Byte dau cua goi cho biet no la loai gi. 0 neu goi rong.</summary>
    public static byte LoaiCuaGoi(byte[] b)
    {
        return b == null || b.Length < 1 ? (byte)0 : b[0];
    }

    /// <summary>Doi mang byte thanh chuoi de gui qua kenh - kenh WebRTC ben
    /// .jslib dang nhan chuoi. Base64 phinh them 33%, nhung 60 byte thanh 80
    /// van con re hon JSON rat nhieu.</summary>
    public static string SangChuoi(byte[] b) { return Convert.ToBase64String(b); }

    public static byte[] TuChuoi(string s)
    {
        try { return Convert.FromBase64String(s); }
        catch { return null; }
    }
}
