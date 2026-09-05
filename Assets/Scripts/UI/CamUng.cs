using UnityEngine;

/// <summary>
/// TRANG THAI DIEU KHIEN CAM UNG, dung chung giua HUD va nhan vat.
///
/// HUD ve can joystick va cac nut ky nang, roi ghi huong day can vao day.
/// PlayerController doc huong do de di chuyen va de ngam ky nang. Tach ra mot
/// cho rieng thay vi cho hai lop goi thang nhau, vi tren PC khong co cai nao
/// trong hai thu nay ton tai ca.
///
/// PC KHONG DOI GI. Toan bo phan cam ung chi bat khi <see cref="DangDung"/> tra
/// ve true.
/// </summary>
public static class CamUng
{
    /// <summary>
    /// Bat tay de THU giao dien cam ung ngay tren PC. HUD dat co nay theo o
    /// "epCamUng" cua no.
    /// </summary>
    public static bool EpBat;

    /// <summary>
    /// Co dang dieu khien bang cam ung khong.
    ///
    /// WebGL chay tren dien thoai la truong hop de sot: Application.isMobilePlatform
    /// tra ve FALSE cho WebGL du dang chay tren dien thoai that. Nen phai hoi
    /// them Input.touchSupported.
    /// </summary>
    public static bool DangDung
    {
        get
        {
            if (EpBat) return true;
            if (Application.isMobilePlatform) return true;
            if (Application.platform == RuntimePlatform.WebGLPlayer && Input.touchSupported)
                return true;
            return false;
        }
    }

    /// <summary>
    /// Huong day can, do dai 0..1 trong mat phang man hinh (x = phai, y = len).
    /// Bang khong khi khong ai cham vao can.
    /// </summary>
    public static Vector2 Huong;

    /// <summary>Co ngon tay nao dang giu can khong.</summary>
    public static bool DangKeo;

    // ================================================================
    //  NGAM KY NANG - an giu roi keo ra khoi nut
    // ================================================================
    //
    // HUD ghi ba bien nay tu ngon dang giu mot nut ky nang. PlayerController va
    // ChiBaoNgam doc chung de ve duong ngam va de biet danh ve dau khi tha tay.
    //
    // Ngon dang ngam KHONG duoc tinh vao cu chi xoay camera - xem danh sach
    // 'ngonNut' trong GameHUD.

    /// <summary>Ky nang dang duoc giu, -1 = khong ai giu.</summary>
    public static int KyNangNgam = -1;

    /// <summary>
    /// Do lech cua ngon tay so voi TAM NUT, tinh bang diem anh (x phai, y len).
    /// </summary>
    public static Vector2 LechNgam;

    /// <summary>
    /// Da keo RA KHOI o ky nang chua.
    ///
    /// Chua ra khoi thi tha tay = danh tu dong vao ke dich gan nhat; da ra khoi
    /// thi tha tay = danh ve dung huong dang ngam.
    /// </summary>
    public static bool DaRaKhoiNut;

    // ================================================================
    //  CAMERA - CU CHI DO CHINH TA DOC, KHONG PHAI TRINH DUYET DICH HO
    // ================================================================
    //
    // Tren WebGL dien thoai KHONG duoc nho trinh duyet phien dich cu chi. Dat
    // hai ngon len man hinh la Safari/Chrome hieu thanh cuon/pinch cua TRANG WEB
    // roi bao cho Unity thanh "Mouse ScrollWheel" - nguoi choi dang giu can va
    // bam ky nang, khong he dinh xoay camera, ma goc nhin van nhay di.
    //
    // Nen CameraRig chan sach moi duong chuot va con lan khi dang o cam ung, chi
    // nhan hai bien duoi day. Chung do HUD ghi tu Input.touches, sau khi da loai
    // ngon dang giu can va ngon dang bam nut ra - nen mot cu cham CHI xoay camera
    // khi no that su bat dau o vung trong.

    /// <summary>
    /// Do lech cua ngon dang keo o vung trong, tinh bang DIEM ANH trong khung
    /// hinh nay. Bang khong khi khong ai keo. HUD ghi moi khung.
    /// </summary>
    public static Vector2 XoayCam;

    /// <summary>
    /// Hai ngon o vung trong chum vao / toe ra bao nhieu DIEM ANH trong khung
    /// hinh nay. Duong = toe ra (phong to). Bang khong khi khong du hai ngon.
    /// </summary>
    public static float ChumZoom;

    // ================================================================
    //  KHOA GOC NHIN
    // ================================================================

    /// <summary>
    /// Nguoi choi da KHOA goc nhin lai chua (nut con mat o goc phai tren).
    ///
    /// Khoa thi camera dung im o dung goc dang co: khong xoay, khong phong to,
    /// khong doi che do nhin - khong bang bat cu thao tac nao. No van bam theo
    /// nhan vat, khong thi nhan vat di vai buoc la ra khoi khung hinh.
    ///
    /// Co nam o day chu khong nam trong CameraRig, vi ca hai ben deu can:
    /// CameraRig doc de bo qua moi thao tac, con HUD doc de ve dau gach cheo va
    /// de lam mo nut doi goc nhin.
    /// </summary>
    public static bool KhoaCam;
}
