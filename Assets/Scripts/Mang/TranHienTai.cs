/// <summary>
/// MOT CHO DE NHO XEM DANG CHOI VAN NAO.
///
/// Sanh o scene MainMenu, con tran o scene Act1/Act2 - hai scene khac nhau,
/// nen phai co cho nao do song qua lan nap scene de man choi biet no dang o
/// phong nao, ai la host.
///
/// Dung truong static chu khong dung mot MonoBehaviour DontDestroyOnLoad:
/// khong co gi de ve, khong co gi de cap nhat moi khung hinh, chi la vai con
/// so. Them mot vat the vao scene chi de giu ba bien la thua.
///
/// Nho ZERO HOA khi ve lai MainMenu (<see cref="Xoa"/>), khong thi choi don
/// mot minh sau do van tuong minh dang trong mot tran mang.
/// </summary>
public static class TranHienTai
{
    /// <summary>Dang choi qua mang hay choi don. Choi don thi moi thu nhu cu.</summary>
    public static bool DangChoiMang;

    /// <summary>Ma phong tren Realtime Database.</summary>
    public static string MaPhong;

    /// <summary>"Act1" hoac "Act2".</summary>
    public static string ManChoi;

    /// <summary>May nay co phai host khong - host la trong tai cua van dau.</summary>
    public static bool LaHost;

    public static void Xoa()
    {
        DangChoiMang = false;
        MaPhong = null;
        ManChoi = null;
        LaHost = false;
    }
}
