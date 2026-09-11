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
/// Firebase van con viec cua no: cac may dung no de TIM THAY NHAU (xem
/// <see cref="BatTay"/>). Bat tay xong thi no dung sang mot ben, va tran dau
/// khong con goi tin nao di qua Google nua.
///
/// NHIEU KENH, NOI HINH SAO QUA CHU PHONG.
///
/// Ban dau lop nay giu DUNG MOT ket noi - phong cho vao bon nguoi ma tran chi
/// noi duoc hai, nguoi thu ba vao tran khong thay ai. Gio moi ket noi la mot
/// kenh danh so 0..<see cref="SoKenhToiDa"/>-1, va SO KENH = SO GHE CUA NGUOI
/// O DAU BEN KIA:
///   - tren may chu phong, kenh 2 la nguoi khach ngoi ghe 2;
///   - tren may khach, kenh 0 luon la chu phong (ghe 0).
/// Khach khong noi thang voi nhau - chu phong chuyen tiep (xem DongBoTran).
///
/// Cac ham khong co so kenh la kenh 0, giu lai de nhung cho chi can mot ket
/// noi (va cac kich ban chay thu cu) khong phai sua.
///
/// HAI DUONG, MOT GIAO DIEN:
///   - WebGL (ban choi chinh): goi thang RTCPeerConnection cua trinh duyet
///     qua <c>CauNoiWebRTC.jslib</c>.
///   - Editor / PC: KHONG co WebRTC. Chay mot kenh gia lap trong bo nho de
///     kiem duoc phan logic - nhung no KHONG do duoc do tre that.
/// </summary>
public static class KenhTrucTiep
{
    /// <summary>STUN cua Google: mien phi, cong khai, va van la Google Cloud.
    /// No chi noi cho may biet dia chi that cua no nhin tu ben ngoai NAT.</summary>
    public const string MayChuStun =
        "stun:stun.l.google.com:19302,stun:stun1.l.google.com:19302";

    /// <summary>Bon ghe trong phong, nen toi da bon kenh (0..3).</summary>
    public const int SoKenhToiDa = 4;

    static readonly bool[] daTao = new bool[SoKenhToiDa];

    static bool HopLe(int k) { return k >= 0 && k < SoKenhToiDa; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern int RTC_Tao(int k, string dsStun);
    [DllImport("__Internal")] static extern void RTC_TaoLoiMoi(int k);
    [DllImport("__Internal")] static extern void RTC_TraLoi(int k, string jsonMoi);
    [DllImport("__Internal")] static extern void RTC_NhanTraLoi(int k, string jsonTra);
    [DllImport("__Internal")] static extern void RTC_ThemUngVien(int k, string jsonUv);
    [DllImport("__Internal")] static extern IntPtr RTC_LayMoTa(int k);
    [DllImport("__Internal")] static extern IntPtr RTC_LayUngVien(int k);
    [DllImport("__Internal")] static extern int RTC_DaMo(int k);
    [DllImport("__Internal")] static extern IntPtr RTC_LayLoi(int k);
    [DllImport("__Internal")] static extern int RTC_Gui(int k, string tin);
    [DllImport("__Internal")] static extern IntPtr RTC_Nhan(int k);
    [DllImport("__Internal")] static extern void RTC_CapNhatKieuKetNoi(int k);
    [DllImport("__Internal")] static extern IntPtr RTC_LayKieuKetNoi(int k);
    [DllImport("__Internal")] static extern void RTC_Dong(int k);

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
    // tin nhan co toi noi khong, chu phong chuyen tiep dung cho chua) ma khong
    // phai build WebGL 5 phut moi lan. KHONG dung no de bao cao do tre.
    static readonly Queue<string>[] hangNhan = TaoHang();
    static readonly bool[] daMoGiaLap = new bool[SoKenhToiDa];
    static readonly string[] moTaGiaLap = new string[SoKenhToiDa];

    static Queue<string>[] TaoHang()
    {
        var h = new Queue<string>[SoKenhToiDa];
        for (int i = 0; i < SoKenhToiDa; i++) h[i] = new Queue<string>();
        return h;
    }

    /// <summary>Noi kenh gia lap voi ben kia - chay thu dat vao day de bat tin
    /// nhan gui di tu MOT kenh bat ky. Mac dinh khong noi voi ai.</summary>
    public static Action<string> guiSangBenKia;

    /// <summary>Nhu tren nhung biet ca so kenh - de kiem chu phong chuyen tiep
    /// dung cho (khong gui nguoc lai nguoi vua gui).</summary>
    public static Action<int, string> guiSangKenh;

    /// <summary>Ben gia lap: nhet mot tin nhan vao kenh 0 nhu the vua nhan duoc.</summary>
    public static void GiaLapNhan(string tin) { GiaLapNhan(0, tin); }

    public static void GiaLapNhan(int k, string tin)
    {
        if (HopLe(k)) hangNhan[k].Enqueue(tin);
    }

    /// <summary>Mo thang mot kenh gia lap ma khong qua bat tay - chi chay thu dung.</summary>
    public static void GiaLapMo(int k)
    {
        if (!HopLe(k)) return;
        daTao[k] = true;
        daMoGiaLap[k] = true;
    }
#endif

    // ================================================================
    //  TRANG THAI
    // ================================================================

    /// <summary>Kenh 0 da mo chua.</summary>
    public static bool DaMo { get { return DaMoKenh(0); } }

    public static bool DaMoKenh(int k)
    {
        if (!HopLe(k)) return false;
#if UNITY_WEBGL && !UNITY_EDITOR
        return daTao[k] && RTC_DaMo(k) == 1;
#else
        return daMoGiaLap[k];
#endif
    }

    public static bool DaTao { get { return daTao[0]; } }
    public static bool DaTaoKenh(int k) { return HopLe(k) && daTao[k]; }

    // ================================================================
    //  BAT TAY
    // ================================================================

    /// <summary>Dong TAT CA roi tao lai kenh 0. Giu nguyen nghia cu - cac kich
    /// ban chay thu dung no de "bat dau lai tu dau".</summary>
    public static void Tao()
    {
        Dong();
        Tao(0);
    }

    /// <summary>Tao (lai) mot kenh. Chi dong chinh kenh ay, cac kenh khac giu nguyen.</summary>
    public static void Tao(int k)
    {
        if (!HopLe(k)) return;
        Dong(k);
#if UNITY_WEBGL && !UNITY_EDITOR
        daTao[k] = RTC_Tao(k, MayChuStun) == 1;
#else
        hangNhan[k].Clear();
        moTaGiaLap[k] = "";
        daMoGiaLap[k] = false;
        daTao[k] = true;
#endif
    }

    /// <summary>May MOI goi: sinh loi moi (offer).</summary>
    public static void TaoLoiMoi() { TaoLoiMoi(0); }
    public static void TaoLoiMoi(int k)
    {
        if (!HopLe(k)) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_TaoLoiMoi(k);
#else
        moTaGiaLap[k] = "{\"type\":\"offer\",\"sdp\":\"gia-lap\"}";
#endif
    }

    /// <summary>May VAO goi: nhan loi moi roi sinh cau tra loi (answer).</summary>
    public static void TraLoi(string jsonMoi) { TraLoi(0, jsonMoi); }
    public static void TraLoi(int k, string jsonMoi)
    {
        if (!HopLe(k)) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_TraLoi(k, jsonMoi);
#else
        moTaGiaLap[k] = "{\"type\":\"answer\",\"sdp\":\"gia-lap\"}";
        daMoGiaLap[k] = true;
#endif
    }

    /// <summary>May MOI goi khi nhan duoc cau tra loi.</summary>
    public static void NhanTraLoi(string jsonTra) { NhanTraLoi(0, jsonTra); }
    public static void NhanTraLoi(int k, string jsonTra)
    {
        if (!HopLe(k)) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_NhanTraLoi(k, jsonTra);
#else
        daMoGiaLap[k] = true;
#endif
    }

    public static void ThemUngVien(string jsonUv) { ThemUngVien(0, jsonUv); }
    public static void ThemUngVien(int k, string jsonUv)
    {
        if (!HopLe(k)) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_ThemUngVien(k, jsonUv);
#endif
    }

    /// <summary>Offer/answer vua sinh ra. Chua co thi chuoi rong.</summary>
    public static string LayMoTa() { return LayMoTa(0); }
    public static string LayMoTa(int k)
    {
        if (!HopLe(k)) return "";
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_LayMoTa(k));
#else
        string s = moTaGiaLap[k] ?? ""; moTaGiaLap[k] = ""; return s;
#endif
    }

    /// <summary>Mot ung vien ICE cua may nay, de gui sang may kia. Het thi rong.</summary>
    public static string LayUngVien() { return LayUngVien(0); }
    public static string LayUngVien(int k)
    {
        if (!HopLe(k)) return "";
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_LayUngVien(k));
#else
        return "";
#endif
    }

    public static string LayLoi() { return LayLoi(0); }
    public static string LayLoi(int k)
    {
        if (!HopLe(k)) return "";
#if UNITY_WEBGL && !UNITY_EDITOR
        return LayChuoi(RTC_LayLoi(k));
#else
        return "";
#endif
    }

    // ================================================================
    //  GUI / NHAN
    // ================================================================

    /// <summary>Gui tren kenh 0.</summary>
    public static bool Gui(string tin) { return Gui(0, tin); }

    public static bool Gui(int k, string tin)
    {
        if (!HopLe(k)) return false;
#if UNITY_WEBGL && !UNITY_EDITOR
        return daTao[k] && RTC_Gui(k, tin) == 1;
#else
        if (!daMoGiaLap[k]) return false;
        if (guiSangBenKia != null) guiSangBenKia(tin);
        if (guiSangKenh != null) guiSangKenh(k, tin);
        return true;
#endif
    }

    /// <summary>
    /// Gui tren MOI kenh dang mo, tru mot kenh (-1 = khong tru kenh nao).
    ///
    /// Chu phong dung de phat cho ca phong, va de CHUYEN TIEP: goi cua khach
    /// ghe 2 phai sang ghe 1 va ghe 3, nhung KHONG duoc vong nguoc ve ghe 2 -
    /// vong ve thi ho nhan lai chinh vi tri cua minh tu 30 ms truoc, va ban sao
    /// "chinh minh" se hien ra dung sau lung minh.
    /// Tra ve so kenh da gui duoc.
    /// </summary>
    public static int GuiTatCa(string tin, int truKenh)
    {
        int n = 0;
        for (int k = 0; k < SoKenhToiDa; k++)
        {
            if (k == truKenh || !DaMoKenh(k)) continue;
            if (Gui(k, tin)) n++;
        }
        return n;
    }

    /// <summary>Lay mot tin nhan da nhan tren kenh 0. Het thi chuoi rong.</summary>
    public static string Nhan() { return Nhan(0); }

    public static string Nhan(int k)
    {
        if (!HopLe(k)) return "";
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!daTao[k]) return "";
        return LayChuoi(RTC_Nhan(k));
#else
        return hangNhan[k].Count > 0 ? hangNhan[k].Dequeue() : "";
#endif
    }

    /// <summary>
    /// Doc kieu ket noi - noi thang hay phai nho nguoi khac tiep suc.
    ///
    /// Goi LUC DANG NOI chu dung doi den luc dong: dong roi thi khong con cap
    /// ung vien nao de xem, va ket qua se la "khong xac dinh" - dung cai dong
    /// quan trong nhat. Da vap dung loi nay o cong cu do tren web.
    /// </summary>
    public static string KieuKetNoi() { return KieuKetNoi(0); }
    public static string KieuKetNoi(int k)
    {
        if (!HopLe(k)) return "";
#if UNITY_WEBGL && !UNITY_EDITOR
        RTC_CapNhatKieuKetNoi(k);
        return LayChuoi(RTC_LayKieuKetNoi(k));
#else
        return "gia-lap";
#endif
    }

    // ================================================================
    //  DONG
    // ================================================================

    /// <summary>Dong TAT CA cac kenh - dung khi roi tran.</summary>
    public static void Dong()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool coKenh = false;
        for (int k = 0; k < SoKenhToiDa; k++) coKenh |= daTao[k];
        if (coKenh) RTC_Dong(-1);
#else
        for (int k = 0; k < SoKenhToiDa; k++)
        {
            daMoGiaLap[k] = false;
            hangNhan[k].Clear();
        }
#endif
        for (int k = 0; k < SoKenhToiDa; k++) daTao[k] = false;
    }

    /// <summary>Dong mot kenh - chu phong dung khi mot nguoi khach roi tran.</summary>
    public static void Dong(int k)
    {
        if (!HopLe(k)) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        if (daTao[k]) RTC_Dong(k);
#else
        daMoGiaLap[k] = false;
        hangNhan[k].Clear();
#endif
        daTao[k] = false;
    }
}
