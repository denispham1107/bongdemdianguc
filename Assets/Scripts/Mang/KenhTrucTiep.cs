using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// KENH NOI THANG MAY VOI MAY - MOT CUA DUY NHAT CHO CA HAI NEN TANG.
///
/// Trong tran dau, vi tri nhan vat KHONG di qua Firebase. Do duoc: mot vong
/// khu hoi toi Firebase Singapore mat 49 ms, va do la SAN CUNG - cong them
/// nhip gui, dem noi suy va khung hinh thi vuot 80 ms. Noi thang may voi may
/// trong nuoc thi con 10-30 ms.
///
/// Firebase van con viec cua no: hai may dung no de TIM THAY NHAU (xem
/// <see cref="BatTay"/>). Bat tay xong thi no dung sang mot ben, va tran dau
/// khong con goi tin nao di qua Google nua.
///
/// HAI DUONG, MOT GIAO DIEN:
///   - WebGL (ban choi chinh): goi thang RTCPeerConnection cua trinh duyet
///     qua <c>CauNoiWebRTC.jslib</c>.
///   - Editor / PC: KHONG co WebRTC. Chay mot kenh gia lap trong bo nho de
///     kiem duoc phan logic - nhung no KHONG do duoc do tre that. Muon ban PC
///     noi mang that thi phai them goi com.unity.webrtc (chua lam).
///
/// Cho nao can biet "dang chay that hay dang gia lap" thi doc
/// <see cref="LaGiaLap"/> - dung de bao cao mot con so gia lap nhu the no la
/// so do that.
/// </summary>
public static class KenhTrucTiep
{
    /// <summary>STUN cua Google: mien phi, cong khai, va van la Google Cloud.
    /// No chi noi cho may biet dia chi that cua no nhin tu ben ngoai NAT.</summary>
    public const string MayChuStun =
        "stun:stun.l.google.com:19302,stun:stun1.l.google.com:19302";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int RTC_Tao(string dsStun);
    [DllImport("__Internal")] static extern void RTC_TaoLoiMoi();
    [DllImport("__Internal")] static extern void RTC_TraLoi(string jsonMoi);
    [DllImport("__Internal")] static extern void RTC_NhanTraLoi(string jsonTra);
    [DllImport("__Internal")] static extern void RTC_ThemUngVien(string jsonUv);
    [DllImport("__Internal")] static extern IntPtr RTC_LayMoTa();
    [DllImport("__Internal")] static extern IntPtr RTC_LayUngVien();
    [DllImport("__Internal")] static extern int RTC_DaMo();
    [DllImport("__Internal")] static extern IntPtr RTC_LayLoi();
    [DllImport("__Internal")] static extern int RTC_Gui(string tin);
    [DllImport("__Internal")] static extern IntPtr RTC_Nhan();
    [DllImport("__Internal")] static extern void RTC_CapNhatKieuKetNoi();
    [DllImport("__Internal")] static extern IntPtr RTC_LayKieuKetNoi();
    [DllImport("__Internal")] static extern void RTC_Dong();

    public const bool LaGiaLap = false;

    /// <summary>
    /// Doi con tro chuoi tu JavaScript sang string roi TRA LAI BO NHO.
    ///
    /// Ben .jslib cap phat bang _malloc trong heap cua Unity; khong giai phong
    /// thi moi tin nhan de lai mot manh rac, ma mot tran dau co hang chuc
    /// nghin tin nhan.
    /// </summary>
    static string LayChuoi(IntPtr p)
    {
        if (p == IntPtr.Zero) return "";
        string s = Marshal.PtrToStringUTF8(p);
        Marshal.FreeHGlobal(p);
        return s ?? "";
    }
#else
    public const bool LaGiaLap = true;

    // ---- Kenh gia lap cho Editor / PC ----
    //
    // Khong phai WebRTC. Chi de chay thu phan logic (bat tay dung thu tu chua,
    // tin nhan co toi noi khong) ma khong phai build WebGL 7 phut moi lan.
    // KHONG dung no de bao cao do tre.
    static readonly Queue<string> hangNhan = new Queue<string>();
    static readonly Queue<string> hangUngVien = new Queue<string>();
    static string moTaGiaLap = "", loiGiaLap = "";
    static bool daMoGiaLap;

    /// <summary>Noi hai kenh gia lap lai voi nhau - dung khi chay thu mot may
    /// dong hai vai. Ben ngoai tu goi, mac dinh khong noi voi ai.</summary>
    public static Action<string> guiSangBenKia;
#endif

    static bool daTao;

    /// <summary>Da mo kenh chua - mo roi moi gui duoc.</summary>
    public static bool DaMo
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return daTao && RTC_DaMo() == 1;
#else
            return daMoGiaLap;
#endif
        }
    }

    public static bool DaTao { get { return daTao; } }

    public static void Tao()
    {
        Dong();
#if UNITY_WEBGL && !UNITY_EDITOR
        daTao = RTC_Tao(MayChuStun) == 1;
#else
        hangNhan.Clear(); hangUngVien.Clear();
        moTaGiaLap = ""; loiGiaLap = ""; daMoGiaLap = false;
        daTao = true;
#endif
    }

    /// <summary>May MOI goi: sinh loi moi (offer).</summary>
    public static void TaoLoiMoi()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_TaoLoiMoi();
#else
        moTaGiaLap = "{\"type\":\"offer\",\"sdp\":\"gia-lap\"}";
#endif
    }

    /// <summary>May VAO goi: nhan loi moi roi sinh cau tra loi (answer).</summary>
    public static void TraLoi(string jsonMoi)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_TraLoi(jsonMoi);
#else
        moTaGiaLap = "{\"type\":\"answer\",\"sdp\":\"gia-lap\"}";
        daMoGiaLap = true;
#endif
    }

    /// <summary>May MOI goi khi nhan duoc cau tra loi.</summary>
    public static void NhanTraLoi(string jsonTra)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_NhanTraLoi(jsonTra);
#else
        daMoGiaLap = true;
#endif
    }

    public static void ThemUngVien(string jsonUv)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_ThemUngVien(jsonUv);
#endif
    }

    /// <summary>Offer/answer vua sinh ra. Chua co thi chuoi rong.</summary>
    public static string LayMoTa()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_LayMoTa());
#else
        string s = moTaGiaLap; moTaGiaLap = ""; return s;
#endif
    }

    /// <summary>Mot ung vien ICE cua may nay, de gui sang may kia. Het thi rong.</summary>
    public static string LayUngVien()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_LayUngVien());
#else
        return hangUngVien.Count > 0 ? hangUngVien.Dequeue() : "";
#endif
    }

    public static string LayLoi()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_LayLoi());
#else
        string s = loiGiaLap; loiGiaLap = ""; return s;
#endif
    }

    public static bool Gui(string tin)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return RTC_Gui(tin) == 1;
#else
        if (!daMoGiaLap) return false;
        if (guiSangBenKia != null) guiSangBenKia(tin);
        return true;
#endif
    }

    /// <summary>Lay mot tin nhan da nhan. Het thi chuoi rong.</summary>
    public static string Nhan()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_Nhan());
#else
        return hangNhan.Count > 0 ? hangNhan.Dequeue() : "";
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    /// <summary>Ben gia lap: nhet mot tin nhan vao nhu the vua nhan duoc.</summary>
    public static void GiaLapNhan(string tin) { hangNhan.Enqueue(tin); }
#endif

    /// <summary>
    /// Doc kieu ket noi - noi thang hay phai nho nguoi khac tiep suc.
    ///
    /// Goi LUC DANG NOI chu dung doi den luc dong: dong roi thi khong con cap
    /// ung vien nao de xem, va ket qua se la "khong xac dinh" - dung cai dong
    /// quan trong nhat. Da vap dung loi nay o cong cu do tren web.
    /// </summary>
    public static string KieuKetNoi()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_CapNhatKieuKetNoi();
        return LayChuoi(RTC_LayKieuKetNoi());
#else
        return "gia-lap";
#endif
    }

    public static void Dong()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (daTao) RTC_Dong();
#else
        daMoGiaLap = false;
        hangNhan.Clear(); hangUngVien.Clear();
#endif
        daTao = false;
    }
}
