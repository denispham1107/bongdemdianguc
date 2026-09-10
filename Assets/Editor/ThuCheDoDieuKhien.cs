using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CHAY THU: MAY TINH PHAI CHOI KIEU PC, DIEN THOAI MOI CHOI KIEU CAM UNG.
///
/// Nguoi choi bao: mo game bang trinh duyet tren may tinh de ban ma van bi bat
/// vao che do cam ung - joystick ao che mat man hinh, khong dung duoc chuot va
/// ban phim.
///
/// Nguyen nhan: <c>Input.touchSupported</c> tra ve TRUE tren gan nhu MOI may
/// tinh Windows doi moi. Chrome bao theo API cua he dieu hanh chu khong theo
/// viec may co man hinh cam ung hay khong.
///
/// PHEP THU NAY DO CAI GI: no chay lai chinh cai LUAT loc trong
/// <c>CauNoiThietBi.jslib</c> tren mot bang user agent THAT, roi doi chieu voi
/// cau tra loi dung. Khong the vao Play de thu, vi jslib chi song trong trinh
/// duyet - nhung cai de sai o day khong phai cau lenh JavaScript, ma la LUAT
/// loc: bo sot may tinh bang Android, hoac de lot iPad doi moi.
///
/// Ket qua ghi ra <c>PlayTestShots/chedodieukhien.txt</c>.
/// </summary>
public static class ThuCheDoDieuKhien
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CheDo] " + s); }

    /// <summary>Mot may that, kem cau tra loi dung.</summary>
    struct MotMay
    {
        public string ten, ua;
        public int soDiemCham;
        public bool phaiLaDiDong;
    }

    [MenuItem("Diablo 2.5D/42. Chay thu CHE DO DIEU KHIEN", false, 129)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0;

        Ghi("[ban 1] may tinh choi kieu PC, dien thoai moi choi kieu cam ung");

        var dsMay = new[]
        {
            // ---- PHAI la may tinh ----
            new MotMay {
                ten = "Windows 11 + Chrome (man hinh thuong)",
                ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                   + "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
                soDiemCham = 0, phaiLaDiDong = false },

            // Day la cai may lam hong tat ca: Windows co bao cam ung
            new MotMay {
                ten = "Windows 11 + Chrome (laptop CO man hinh cam ung)",
                ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                   + "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
                soDiemCham = 10, phaiLaDiDong = false },

            new MotMay {
                ten = "MacBook + Safari",
                ua = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 "
                   + "(KHTML, like Gecko) Version/17.1 Safari/605.1.15",
                soDiemCham = 0, phaiLaDiDong = false },

            new MotMay {
                ten = "Linux + Firefox",
                ua = "Mozilla/5.0 (X11; Linux x86_64; rv:133.0) Gecko/20100101 Firefox/133.0",
                soDiemCham = 0, phaiLaDiDong = false },

            // ---- PHAI la di dong ----
            new MotMay {
                ten = "iPhone + Safari",
                ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) "
                   + "AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1",
                soDiemCham = 5, phaiLaDiDong = true },

            new MotMay {
                ten = "Dien thoai Android + Chrome",
                ua = "Mozilla/5.0 (Linux; Android 14; SM-S918B) AppleWebKit/537.36 "
                   + "(KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36",
                soDiemCham = 5, phaiLaDiDong = true },

            // May tinh bang Android KHONG co chu "Mobile" - loc theo chu ay la bo sot ca dong
            new MotMay {
                ten = "May tinh bang Android (KHONG co chu Mobile)",
                ua = "Mozilla/5.0 (Linux; Android 13; SM-X710) AppleWebKit/537.36 "
                   + "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
                soDiemCham = 5, phaiLaDiDong = true },

            new MotMay {
                ten = "iPad doi cu (con khai la iPad)",
                ua = "Mozilla/5.0 (iPad; CPU OS 12_5 like Mac OS X) AppleWebKit/605.1.15 "
                   + "(KHTML, like Gecko) Version/12.1 Mobile/15E148 Safari/604.1",
                soDiemCham = 5, phaiLaDiDong = true },

            // Cai bay lon nhat: iPadOS 13+ khai bao y het macOS
            new MotMay {
                ten = "iPad doi moi (tu nhan la may Mac)",
                ua = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 "
                   + "(KHTML, like Gecko) Version/17.1 Safari/605.1.15",
                soDiemCham = 5, phaiLaDiDong = true },
        };

        int sai = 0;
        foreach (var m in dsMay)
        {
            bool ra = LuatLoc(m.ua, m.soDiemCham);
            bool dung = ra == m.phaiLaDiDong;
            if (!dung) sai++;

            Ghi(string.Format("  {0} {1,-45} cham={2,-3} -> {3}  (phai la {4})",
                dung ? "dat " : "SAI ", m.ten, m.soDiemCham,
                ra ? "CAM UNG" : "PC", m.phaiLaDiDong ? "CAM UNG" : "PC"));
        }

        Ghi("1. " + dsMay.Length + " loai may, so ket luan sai: " + sai + " (phai la 0)");
        if (sai > 0) { Ghi("[LOI] luat loc thiet bi con sai"); loi++; }

        // ---- 2. Luat trong jslib phai KHOP voi luat vua do ----
        //
        // Phep do tren chay tren mot ban chep bang C#. Neu ban chep va ban that
        // troi ra khac nhau thi phep do khong con nghia gi - nen doi chieu lai
        // chinh file jslib: no phai co du bon manh loc.
        string duong = "Assets/Plugins/WebGL/CauNoiThietBi.jslib";
        string js = File.Exists(duong) ? File.ReadAllText(duong) : "";

        var manh = new[]
        {
            "userAgentData",   // hoi thang trinh duyet khi co
            "Android",         // ca dien thoai lan may tinh bang
            "iPhone|iPod|iPad",
            "maxTouchPoints",  // cai bay iPad doi moi
        };

        int thieu = 0;
        foreach (var t in manh)
            if (!js.Contains(t)) { Ghi("   [thieu trong jslib] " + t); thieu++; }

        Ghi("2. file jslib co du " + (manh.Length - thieu) + "/" + manh.Length
            + " manh loc, va KHONG con hoi touchSupported");
        if (thieu > 0) { Ghi("[LOI] ban that trong jslib thieu manh loc"); loi++; }

        // ---- 3. Duong cu phai bi cat han ----
        // Con mot dong Input.touchSupported trong CamUng.DangDung la du de
        // may tinh bi bat lai vao che do cam ung.
        string cs = File.ReadAllText("Assets/Scripts/UI/CamUng.cs");
        int viTri = cs.IndexOf("public static bool DangDung");
        int ketThuc = viTri >= 0 ? cs.IndexOf("public static int TraLoiCuaTrinhDuyet", viTri) : -1;
        string than = viTri >= 0 && ketThuc > viTri ? cs.Substring(viTri, ketThuc - viTri) : cs;

        // Bo phan chu thich roi moi tim, khong thi dong giai thich "KHONG DUOC
        // HOI Input.touchSupported" lai bi tinh la mot loi goi that.
        string thanKhongChuThich = Regex.Replace(than, @"//.*?$", "", RegexOptions.Multiline);
        thanKhongChuThich = Regex.Replace(thanKhongChuThich, @"///.*?$", "", RegexOptions.Multiline);

        bool conGoi = thanKhongChuThich.Contains("Input.touchSupported");
        Ghi("3. CamUng.DangDung con hoi Input.touchSupported: " + conGoi + " (phai la False)");
        if (conGoi) { Ghi("[LOI] duong cu chua bi cat - may tinh se lai bi coi la cam ung"); loi++; }

        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/chedodieukhien.txt", bao.ToString());
    }

    /// <summary>
    /// BAN CHEP CUA LUAT LOC TRONG <c>CauNoiThietBi.jslib</c>.
    ///
    /// Phai giu khop voi ban that. Chieu do thu 2 kiem dieu do bang cach doc
    /// lai chinh file jslib.
    /// </summary>
    static bool LuatLoc(string ua, int soDiemCham)
    {
        if (Regex.IsMatch(ua, "Android", RegexOptions.IgnoreCase)) return true;
        if (Regex.IsMatch(ua, "iPhone|iPod|iPad", RegexOptions.IgnoreCase)) return true;

        // iPad doi moi tu nhan la may Mac - phan biet bang so diem cham
        if (Regex.IsMatch(ua, "Macintosh", RegexOptions.IgnoreCase) && soDiemCham > 1)
            return true;

        if (Regex.IsMatch(ua, "webOS|BlackBerry|IEMobile|Opera Mini|Windows Phone|Silk",
                          RegexOptions.IgnoreCase)) return true;

        return false;
    }
}
