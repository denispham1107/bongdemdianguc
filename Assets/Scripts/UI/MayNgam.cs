using UnityEngine;

/// <summary>
/// MAY TRANG THAI CUA MOT LAN NGAM: an giu nut ky nang - keo ra - tha.
///
/// Tach HAN ra khoi <see cref="GameHUD"/> vi hai ly do:
///
///   1. HUD co HAI duong vao giong het nhau ve y nghia - ngon tay tren may cam
///      ung, va con chuot de thu tren PC. Viet hai lan thi sua mot ben quen ben
///      kia, va loi kieu do chi lo ra tren dien thoai that.
///
///   2. Lop nay KHONG dung Input, khong dung Screen, khong dung Time. Nho vay
///      bo tu kiem tra goi thang duoc no bang toa do tu bia ra - khong the kiem
///      thu duoc mot ham doc Input.GetTouch.
///
/// Toa do vao la toa do CHAM (goc duoi trai), giong Input.mousePosition.
/// </summary>
public class MayNgam
{
    /// <summary>Viec phai lam khi nguoi choi tha tay.</summary>
    public enum Lenh
    {
        /// <summary>Khong dang ngam gi ca - bo qua.</summary>
        Khong,

        /// <summary>Cham roi tha ngay tai cho: danh vao ke dich gan nhat.</summary>
        TuDong,

        /// <summary>Da keo ra ngoai: danh theo huong va cu ly da ngam.</summary>
        TheoHuong,
    }

    /// <summary>Ky nang dang giu, -1 = khong giu gi.</summary>
    public int KyNang { get; private set; }

    /// <summary>Do lech cua ngon tay so voi TAM NUT, tinh bang diem anh.</summary>
    public Vector2 Lech { get; private set; }

    /// <summary>Da keo ra khoi o ky nang chua.</summary>
    public bool DaRaKhoiNut { get; private set; }

    public bool DangNgam { get { return KyNang >= 0; } }

    Vector2 tamNut;
    float banKinhNut = 1f;
    float keoToiDa = 1f;

    public MayNgam() { KyNang = -1; }

    /// <summary>
    /// Ngon tay vua dat xuong mot nut ky nang. CHUA danh gi ca.
    /// </summary>
    public void BatDau(int nut, Vector2 diem, Vector2 tamNut, float banKinhNut, float keoToiDa)
    {
        KyNang = nut;
        this.tamNut = tamNut;
        this.banKinhNut = Mathf.Max(1f, banKinhNut);
        this.keoToiDa = Mathf.Max(this.banKinhNut + 1f, keoToiDa);
        Lech = diem - tamNut;
        DaRaKhoiNut = false;
    }

    /// <summary>Ngon tay di chuyen.</summary>
    public void KeoToi(Vector2 diem)
    {
        if (!DangNgam) return;

        Lech = diem - tamNut;

        // Mot khi DA ra khoi nut thi khong bao gio quay lai trang thai "chua ra".
        //
        // Keo nguoc ngon tay ve gan nut de ngam mot muc tieu dang dung sat ben
        // la chuyen binh thuong. Cho co nay tat di thi cu do hoa ra "cham roi
        // tha" - va ky nang bay vao mot con hoan toan khac.
        if (!DaRaKhoiNut && Lech.magnitude > banKinhNut) DaRaKhoiNut = true;
    }

    /// <summary>
    /// Nguoi choi tha tay. Tra ve viec phai lam, roi TU DAT LAI ve trang thai
    /// khong ngam.
    /// </summary>
    public Lenh Tha(Vector2 diem)
    {
        if (!DangNgam) { Huy(); return Lenh.Khong; }

        KeoToi(diem);
        Lenh lenh = DaRaKhoiNut ? Lenh.TheoHuong : Lenh.TuDong;

        // GIU LAI Lech va KyNang cho ben goi doc: Huy() se xoa het. Nen tra loi
        // truoc, ben goi doc xong roi tu goi Huy.
        return lenh;
    }

    public void Huy()
    {
        KyNang = -1;
        Lech = Vector2.zero;
        DaRaKhoiNut = false;
    }

    /// <summary>
    /// Do dai da keo, quy ve 0..1.
    ///
    /// TRU DI ban kinh nut truoc khi chia: khong tru thi vua nhac ngon ra khoi
    /// mep nut la da duoc 0,35 roi - mat sach mot phan ba doan keo, dung cai
    /// doan dung de chinh cu ly.
    /// </summary>
    public float Xa01
    {
        get
        {
            if (!DangNgam) return 0f;
            return Mathf.Clamp01((Lech.magnitude - banKinhNut)
                                 / Mathf.Max(1f, keoToiDa - banKinhNut));
        }
    }
}
