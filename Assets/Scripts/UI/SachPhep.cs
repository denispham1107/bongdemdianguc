using UnityEngine;

/// <summary>
/// SACH PHEP - NGUOI CHOI TU SAP XEP CAC O KY NANG.
///
/// Truoc day bay ky nang nam CUNG cho, cung thu tu, khong doi duoc: nut so 1
/// luon la Qua cau lua, o vuong thu ba luon la Sam set. Nguoi dung xin mot cua
/// so rieng (12/09/2026) de keo tha: cot trai la kho ky nang, ben phai la cac O
/// - sap xep trong cua so the nao thi ngoai man hinh game hien ra y het.
///
/// HAI BO O RIENG, vi hai ban giao dien khac han nhau:
///   - <see cref="OTron"/>  : cum nut TRON o goc phai duoi cua ban cam ung.
///   - <see cref="OVuong"/> : hang o VUONG giua day man hinh cua ban may tinh.
/// Mot nguoi choi WebGL co the doi qua doi lai giua hai ban (cam ung hay khong
/// la do trinh duyet tra loi), nen giu rieng thi doi ban khong lam xao tron bo
/// kia. <see cref="BoDangDung"/> tra ve dung bo cua ban dang chay.
///
/// Moi o giu SO HIEU KY NANG (0..6), hoac <see cref="Trong"/> = -1. So hieu ay
/// la thu di qua mang (goi ky nang) va di vao PlayerController.CastAt - KHONG
/// duoc thay bang so thu tu o, khong thi hai may sap xep khac nhau se ban ra
/// hai phep khac nhau.
///
/// Cat vao PlayerPrefs (tren WebGL chinh la localStorage cua trinh duyet).
/// </summary>
public static class SachPhep
{
    /// <summary>So ky nang dang co trong kho.</summary>
    public const int SoKyNang = 7;

    /// <summary>So o tren ban cam ung - bang so nut tron dang ve.</summary>
    public const int SoOTron = 7;

    /// <summary>So o tren ban may tinh - bang so o vuong cua thanh ky nang.</summary>
    public const int SoOVuong = 7;

    /// <summary>O khong co ky nang nao.</summary>
    public const int Trong = -1;

    const string KhoaTron = "diablo25d.sachphep.tron";
    const string KhoaVuong = "diablo25d.sachphep.vuong";

    static int[] oTron, oVuong;

    public static int[] OTron { get { Nap(); return oTron; } }
    public static int[] OVuong { get { Nap(); return oVuong; } }

    /// <summary>Bo o cua ban giao dien dang chay (cam ung hay may tinh).</summary>
    public static int[] BoDangDung { get { return CamUng.DangDung ? OTron : OVuong; } }

    /// <summary>So o cua ban giao dien dang chay.</summary>
    public static int SoODangDung { get { return CamUng.DangDung ? SoOTron : SoOVuong; } }

    /// <summary>Ky nang o o thu <paramref name="o"/> cua ban dang chay, -1 neu trong.</summary>
    public static int KyNangTaiO(int o)
    {
        var bo = BoDangDung;
        return o >= 0 && o < bo.Length ? bo[o] : Trong;
    }

    // ================================================================
    //  NAP - LUU
    // ================================================================

    static void Nap()
    {
        if (oTron != null && oVuong != null) return;
        oTron = Doc(KhoaTron, SoOTron);
        oVuong = Doc(KhoaVuong, SoOVuong);
    }

    /// <summary>Quen het, doc lai tu dau - dung cho phep thu.</summary>
    public static void NapLai() { oTron = null; oVuong = null; Nap(); }

    static int[] MacDinh(int soO)
    {
        var a = new int[soO];
        for (int i = 0; i < soO; i++) a[i] = i < SoKyNang ? i : Trong;
        return a;
    }

    static int[] Doc(string khoa, int soO)
    {
        string s = PlayerPrefs.GetString(khoa, "");
        if (string.IsNullOrEmpty(s)) return MacDinh(soO);

        var phan = s.Split(',');
        var a = MacDinh(soO);
        // Chi nhan ban ghi DUNG SO O. Ban cu (it o hon) ma nhet vao thi nhung o
        // cuoi giu gia tri mac dinh, tuc mot ky nang hien ra hai cho.
        if (phan.Length != soO) return a;

        for (int i = 0; i < soO; i++)
        {
            int v;
            if (!int.TryParse(phan[i], out v)) return MacDinh(soO);
            a[i] = (v >= 0 && v < SoKyNang) ? v : Trong;
        }

        // KHONG cho mot ky nang nam o hai o: ban ghi hong (hoac nguoi choi sua
        // tay localStorage) thi bam o nay ra phep kia, khong hieu vi sao.
        for (int i = 0; i < soO; i++)
            for (int j = i + 1; j < soO; j++)
                if (a[i] != Trong && a[i] == a[j]) a[j] = Trong;

        return a;
    }

    public static void Luu()
    {
        Nap();
        PlayerPrefs.SetString(KhoaTron, string.Join(",", System.Array.ConvertAll(oTron, x => x.ToString())));
        PlayerPrefs.SetString(KhoaVuong, string.Join(",", System.Array.ConvertAll(oVuong, x => x.ToString())));
        PlayerPrefs.Save();
    }

    /// <summary>Tra moi o ve dung thu tu goc.</summary>
    public static void DatLai()
    {
        Nap();
        oTron = MacDinh(SoOTron);
        oVuong = MacDinh(SoOVuong);
        Luu();
    }

    // ================================================================
    //  KEO THA
    // ================================================================

    /// <summary>
    /// Dat mot KY NANG tu kho vao mot o.
    ///
    /// Ky nang ay dang nam o o khac thi HAI O DOI CHO cho nhau - khong nhan ban.
    /// Keo Sam set tu o 3 sang o 1 ma de nguyen o 3 thi nguoi choi co hai nut
    /// Sam set va mat mot ky nang khac ma khong hieu tai sao.
    /// </summary>
    public static void DatVaoO(int o, int kyNang)
    {
        var bo = BoDangDung;
        if (o < 0 || o >= bo.Length) return;
        if (kyNang != Trong && (kyNang < 0 || kyNang >= SoKyNang)) return;

        if (kyNang != Trong)
        {
            int cu = bo[o];
            for (int i = 0; i < bo.Length; i++)
                if (i != o && bo[i] == kyNang) bo[i] = cu;   // doi cho
        }
        bo[o] = kyNang;
        Luu();
    }

    /// <summary>Doi cho hai o cho nhau.</summary>
    public static void DoiHaiO(int a, int b)
    {
        var bo = BoDangDung;
        if (a < 0 || b < 0 || a >= bo.Length || b >= bo.Length || a == b) return;
        int t = bo[a]; bo[a] = bo[b]; bo[b] = t;
        Luu();
    }

    /// <summary>Bo ky nang khoi mot o (keo ra ngoai roi tha).</summary>
    public static void BoKhoiO(int o)
    {
        var bo = BoDangDung;
        if (o < 0 || o >= bo.Length) return;
        bo[o] = Trong;
        Luu();
    }

    /// <summary>Ky nang nay dang nam o o nao cua ban dang chay, -1 neu chua dat.</summary>
    public static int ONaoGiu(int kyNang)
    {
        var bo = BoDangDung;
        for (int i = 0; i < bo.Length; i++) if (bo[i] == kyNang) return i;
        return -1;
    }

    // ================================================================
    //  TEN VA LOI KE TUNG KY NANG
    // ================================================================
    //
    // CHU HIEN CHO NGUOI CHOI nen phai co dau day du (quy tac cua du an). Cac
    // con so thi KHONG viet cung o day ma lay tu PlayerController luc chay -
    // chep tay vao day thi sua can bang mot lan la bang thong so noi doi.

    public static string Ten(int ky)
    {
        switch (ky)
        {
            case 0: return "QUẢ CẦU LỬA";
            case 1: return "MƯA BĂNG";
            case 2: return "SẤM SÉT";
            case 3: return "LỐC XOÁY";
            case 4: return "THIÊN THẠCH";
            case 5: return "KHIÊN";
            case 6: return "GIỰT SÉT";
            default: return "";
        }
    }

    /// <summary>Mot dong ngan - de canh ten trong danh sach ben trai.</summary>
    public static string TomTat(int ky)
    {
        switch (ky)
        {
            case 0: return "Ba quả lửa bay thẳng";
            case 1: return "Bão băng, làm chậm và đóng băng";
            case 2: return "Giông sét, gây choáng";
            case 3: return "Cuốn mọi thứ lên trời";
            case 4: return "Ba khối đá lửa rơi xuống";
            case 5: return "Vòm chắn quanh mình";
            case 6: return "Tia sét nối liền kẻ địch";
            default: return "";
        }
    }

    /// <summary>Lời kể dài, hiện ở khung chi tiết bên phải.</summary>
    public static string MoTa(int ky)
    {
        switch (ky)
        {
            case 0:
                return "Ba quả cầu lửa cùng bay ra, toè thành hình quạt về phía trước.\n\n"
                     + "Quả nào chạm vật cản hay kẻ địch thì nổ tại chỗ, thiêu cháy mọi "
                     + "thứ quanh điểm nổ. Cây cối trúng lửa sẽ bắt cháy và cháy lan "
                     + "theo thân.\n\n"
                     + "Đây là đòn rẻ nhất và hồi nhanh nhất — thứ để dùng liên tục "
                     + "giữa hai lần tung phép lớn.";
            case 1:
                return "Gọi một cơn bão lạnh phủ xuống khu vực đã chọn: tuyết rơi, sương "
                     + "lạnh là đất, và từng tảng băng lớn liên tiếp rơi xuống.\n\n"
                     + "Tảng băng rơi trúng kẻ địch thì CHẮC CHẮN làm chậm một nửa tốc "
                     + "độ trong 2 giây, và 35% số lần còn đóng cứng hẳn 1,5 giây — kẻ "
                     + "bị đóng cứng không đi được và không tung được phép nào.\n\n"
                     + "Bão tự tìm kẻ địch trong vùng để dội xuống, và không bao giờ "
                     + "nhắm vào chính người tung ra nó.";
            case 2:
                return "Mây đen kéo tới, không khí tích điện, rồi hàng loạt tia sét từ "
                     + "trên cao liên tiếp giáng xuống khu vực đã chọn.\n\n"
                     + "Mỗi tia nổ ra một vùng sáng trắng trên mặt đất, gây sát thương "
                     + "và có 35% khả năng làm kẻ địch BỊ CHOÁNG — đứng im, không đánh "
                     + "và không tung phép được.\n\n"
                     + "Chỗ nào sét đánh xuống thì chỗ đó cháy sém và bốc khói.";
            case 3:
                return "Một cột lốc xoáy hình thành ngay trước mặt rồi trượt về phía "
                     + "ngắm, cuốn theo mọi thứ nó đi qua.\n\n"
                     + "Kẻ địch bị nhấc bổng lên trời, quay tròn rồi rơi xuống. Cây cối, "
                     + "bia mộ và cả lò lửa cũng bị cuốn đi — lò lửa thì bị dập tắt "
                     + "trước, và 30 giây sau mới mọc lại rồi cháy tiếp.\n\n"
                     + "Cảnh vật bị cuốn sẽ tự trở về chỗ cũ sau 30 giây.";
            case 4:
                return "Ba khối đá lửa nối đuôi nhau rơi từ trên trời xuống điểm ngắm, "
                     + "cách nhau nửa giây; hai khối sau lệch ra chung quanh chứ không "
                     + "rơi trùng một chỗ.\n\n"
                     + "Mỗi khối nổ ra một vùng lửa rộng, làm rung màn hình và để lại "
                     + "vũng lửa cháy trên mặt đất. Cây trúng thiên thạch sẽ cháy rụi "
                     + "rồi mọc lại sau 30 giây.\n\n"
                     + "Đòn mạnh nhất, và cũng tốn năng lượng nhất.";
            case 5:
                return "Dựng một vòm chắn sáng quanh chính mình. Mọi đòn đánh trừ vào "
                     + "khiên trước khi chạm tới máu.\n\n"
                     + "Đòn trúng khiên thì hiệu ứng đi kèm (bỏng, đóng băng) cũng bị "
                     + "chặn luôn — cầu lửa nổ trên mặt vòm mà người bên trong vẫn bốc "
                     + "cháy thì nhìn như lửa đã lọt qua khiên.\n\n"
                     + "Khiên vỡ khi hết máu, và mờ dần theo lượng máu còn lại.";
            case 6:
                return "Một tia sét phóng thẳng từ tay, chạm kẻ địch đầu tiên rồi NHẢY "
                     + "tiếp sang những kẻ đứng gần đó.\n\n"
                     + "Mỗi lần nhảy sát thương giảm bớt, nhưng một tia có thể quét hết "
                     + "cả một đám đang xúm lại. Đòn rẻ, hồi nhanh, hợp lúc bị vây.";
            default: return "";
        }
    }

    /// <summary>Ba con so chinh cua mot ky nang, doc THANG tu nhan vat.</summary>
    public static void ThongSo(PlayerController pc, int ky,
                               out float nangLuong, out float hoiChieu, out float niemChu)
    {
        nangLuong = 0f; hoiChieu = 0f; niemChu = 0f;
        if (pc == null) return;
        switch (ky)
        {
            case 0: nangLuong = pc.fireballCost; hoiChieu = pc.fireballCooldown; niemChu = pc.fireballCastTime; break;
            case 1: nangLuong = pc.iceCost; hoiChieu = pc.iceCooldown; niemChu = pc.iceCastTime; break;
            case 2: nangLuong = pc.boltCost; hoiChieu = pc.boltCooldown; niemChu = pc.boltCastTime; break;
            case 3: nangLuong = pc.tornadoCost; hoiChieu = pc.tornadoCooldown; niemChu = pc.tornadoCastTime; break;
            case 4: nangLuong = pc.meteorCost; hoiChieu = pc.meteorCooldown; niemChu = pc.meteorCastTime; break;
            case 5: nangLuong = pc.khiengCost; hoiChieu = pc.khiengCooldown; niemChu = pc.khiengCastTime; break;
            case 6: nangLuong = pc.giatSetCost; hoiChieu = pc.giatSetCooldown; niemChu = pc.giatSetCastTime; break;
        }
    }
}
