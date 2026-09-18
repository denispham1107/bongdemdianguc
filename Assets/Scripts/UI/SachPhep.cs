using System.Runtime.InteropServices;
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
/// CAT LAI: tren WebGL ghi THANG vao localStorage (cau noi CauNoiCaiDat.jslib),
/// giong CaiDatDoHoa - PlayerPrefs tren WebGL ghi xuong IndexedDB KHONG DONG BO,
/// nguoi choi sap o xong o sanh roi dong tab ngay thi thu tu mat. Ngoai WebGL
/// dung PlayerPrefs. Ban ghi cu chi nam trong PlayerPrefs van doc duoc.
/// </summary>
public static class SachPhep
{
    /// <summary>So ky nang dang co trong kho.</summary>
    public const int SoKyNang = CapDo.SoKyNang;     // 7 phep + binh mau + binh mana + qua cau bang + gio loc + lua dia nguc

    // ================================================================
    //  XEP NHOM CHO COT DANH SACH (nguoi dung chot 18/09/2026)
    // ================================================================
    //  Truoc day cot trai xep theo SO HIEU ky nang (0,1,2,...) - thu tu ay do lich su
    //  them ky nang quyet dinh chu khong theo he, nen Qua cau lua nam canh Mua bang.
    //  Nguoi dung xin xep theo HE va theo dung thu tu duoi day. Chi la THU TU HIEN THI:
    //  so hieu ky nang giu nguyen (no di qua goi tin va vao PlayerController.CastAt).

    /// <summary>Ten cac nhom, theo dung thu tu hien ra tren cot.</summary>
    public static readonly string[] TenNhom = { "LỬA", "BĂNG", "SÉT", "PHONG", "HỖ TRỢ" };

    /// <summary>Mau chu tieu de cua tung nhom - theo he.</summary>
    public static readonly Color[] MauNhom = {
        new Color(1.00f, 0.55f, 0.20f),   // lua
        new Color(0.55f, 0.85f, 1.00f),   // bang
        new Color(0.70f, 0.60f, 1.00f),   // set
        new Color(0.80f, 0.82f, 0.78f),   // phong
        new Color(1.00f, 0.84f, 0.42f),   // ho tro
    };

    /// <summary>Ky nang cua tung nhom, dung thu tu nguoi dung viet.</summary>
    public static readonly int[][] KyNangTheoNhom = {
        new[] { 0, 4, CapDo.KyLuaDiaNguc },                       // Qua cau lua, Thien thach, Lua dia nguc
        new[] { CapDo.KyQuaCauBang, 1, CapDo.KyTangHinh },        // Qua cau bang, Mua bang, Tang hinh
        new[] { 6, 2, CapDo.KyCauDien },                          // Giut set, Sam set, Qua cau dien
        new[] { CapDo.KyGioLoc, 3, CapDo.KyHoaLocXoay },          // Gio loc, Loc xoay, Hoa loc xoay
        new[] { CapDo.KyBinhMau, CapDo.KyBinhMana, 5 },           // Binh mau, Binh mana, Khien
    };

    /// <summary>So ky nang trong mot nhom.</summary>
    public static int SoKyNangNhom(int nhom)
    {
        return (nhom >= 0 && nhom < KyNangTheoNhom.Length) ? KyNangTheoNhom[nhom].Length : 0;
    }

    /// <summary>So nhom.</summary>
    public static int SoNhom { get { return KyNangTheoNhom.Length; } }

    /// <summary>So o trong danh sach: moi nhom mot dong tieu de + cac ky nang cua no.</summary>
    public static int SoDongDanhSach
    {
        get
        {
            int n = 0;
            for (int i = 0; i < KyNangTheoNhom.Length; i++) n += 1 + KyNangTheoNhom[i].Length;
            return n;
        }
    }

    /// <summary>
    /// Dong thu <paramref name="dong"/> cua cot danh sach la gi.
    /// Tra ve -1 va <paramref name="nhom"/> >= 0 neu do la DONG TIEU DE; nguoc lai tra so hieu ky nang.
    /// </summary>
    public static int KyNangODong(int dong, out int nhom)
    {
        nhom = -1;
        int d = 0;
        for (int g = 0; g < KyNangTheoNhom.Length; g++)
        {
            if (dong == d) { nhom = g; return -1; }        // dong tieu de cua nhom
            d++;
            var ds = KyNangTheoNhom[g];
            if (dong < d + ds.Length) return ds[dong - d];
            d += ds.Length;
        }
        return -1;
    }

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

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern string CD_DocChuoi(string khoa);
    [DllImport("__Internal")] static extern int CD_GhiChuoi(string khoa, string giaTri);
#endif

    static string DocChuoi(string khoa)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            string v = CD_DocChuoi(khoa);
            if (!string.IsNullOrEmpty(v)) return v;
        }
        catch { }
#endif
        return PlayerPrefs.GetString(khoa, "");
    }

    static void GhiChuoi(string khoa, string giaTri)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { CD_GhiChuoi(khoa, giaTri); } catch { }
#endif
        PlayerPrefs.SetString(khoa, giaTri);
    }

    static int[] Doc(string khoa, int soO)
    {
        string s = DocChuoi(khoa);
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
        GhiChuoi(KhoaTron, string.Join(",", System.Array.ConvertAll(oTron, x => x.ToString())));
        GhiChuoi(KhoaVuong, string.Join(",", System.Array.ConvertAll(oVuong, x => x.ToString())));
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
            case CapDo.KyBinhMau: return "BÌNH MÁU";
            case CapDo.KyBinhMana: return "BÌNH MANA";
            case CapDo.KyQuaCauBang: return "QUẢ CẦU BĂNG";
            case CapDo.KyGioLoc: return "GIÓ LỐC";
            case CapDo.KyLuaDiaNguc: return "LỬA ĐỊA NGỤC";
            case CapDo.KyTangHinh: return "TÀNG HÌNH";
            case CapDo.KyCauDien: return "QUẢ CẦU ĐIỆN";
            case CapDo.KyHoaLocXoay: return "HOÁ LỐC XOÁY";
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
            case 6: return "4 tia sét lan, 15% choáng";
            case CapDo.KyBinhMau: return "Uống một bình, hồi tối đa 100 máu";
            case CapDo.KyBinhMana: return "Uống một bình, hồi tối đa 50 năng lượng";
            case CapDo.KyQuaCauBang: return "Ba quả băng, 40% đóng băng";
            case CapDo.KyGioLoc: return "Một cơn lốc, 55% hất tung";
            case CapDo.KyLuaDiaNguc: return "Năm quả lửa tự đuổi kẻ địch";
            case CapDo.KyTangHinh: return "Trong suốt 20 giây, đòn sau gấp đôi";
            case CapDo.KyCauDien: return "Cầu điện bắn 10 lượt, mỗi lượt 5 tia";
            case CapDo.KyHoaLocXoay: return "Hoá Gió lốc đang bay thành Lốc xoáy";
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
                return "Tia sét phóng thẳng từ tay tới kẻ địch phía trước trong tầm 20 mét. Có "
                     + "tới 4 kẻ địch thì 4 tia cùng phóng ra một lúc, mỗi tia một kẻ. Chạm "
                     + "kẻ địch rồi mỗi tia lại NHẢY tiếp sang những kẻ đứng gần đó.\n\n"
                     + "Sát thương ban đầu 75, mỗi lần nhảy giảm bớt một ít. Mỗi cú đánh "
                     + "trúng — kể cả tia nhảy — có 15% khả năng làm kẻ địch BỊ CHOÁNG.\n\n"
                     + "Đòn rẻ, hồi nhanh, hợp lúc bị vây.";
            case CapDo.KyBinhMau:
                return "Uống cạn một bình máu đặc sánh, hồi ngay tối đa 100 máu. Máu đang đầy "
                     + "thì không uống — không phí bình.\n\n"
                     + "Không có sẵn: bình máu chỉ có khi NHẶT được. Mỗi con quái bị hạ có "
                     + "10% khả năng rơi ra một bình máu. Tới gần là bình tự bay vào người, và "
                     + "số bình đang có hiện ngay trên ô kỹ năng.\n\n"
                     + "Chơi nhiều người thì bình rơi là của CHUNG cả phòng — ai tới trước người "
                     + "ấy được.\n\n"
                     + "Chỉ cần mở khoá bằng 1 điểm kỹ năng, không nâng cấp được. Uống xong "
                     + "phải chờ 0,5 giây mới uống bình tiếp theo.";
            case CapDo.KyBinhMana:
                return "Uống một bình nước phép xanh lạnh buốt, hồi ngay tối đa 50 năng lượng. "
                     + "Năng lượng đang đầy thì không uống.\n\n"
                     + "Bình mana chỉ có khi NHẶT được: mỗi con quái bị hạ có 10% khả năng rơi "
                     + "ra một bình. Tới gần là bình tự bay vào người, số bình còn lại hiện "
                     + "trên ô kỹ năng.\n\n"
                     + "Chơi nhiều người thì bình rơi là của CHUNG cả phòng — ai tới trước người "
                     + "ấy được.\n\n"
                     + "Mở khoá bằng 1 điểm kỹ năng, không nâng cấp được. Uống xong phải chờ "
                     + "0,5 giây mới uống bình tiếp theo.";
            case CapDo.KyQuaCauBang:
                return "Ba quả cầu băng pha lê cùng phóng ra, toè thành hình quạt về phía trước. "
                     + "Phía sau mỗi quả kéo theo một luồng không khí lạnh buốt và một vệt băng "
                     + "lấp lánh rơi rớt dọc đường bay.\n\n"
                     + "Quả nào chạm vật cản hay kẻ địch thì vỡ tung thành một vụ nổ băng: mọi "
                     + "kẻ địch trong vùng nổ đều mất máu, đứng càng gần tâm càng đau. Sát thương "
                     + "ban đầu 65 mỗi quả.\n\n"
                     + "Kẻ nào trúng đều bị LÀM CHẬM một nửa tốc độ trong 2 giây, và có 40% khả năng bị "
                     + "ĐÓNG BĂNG 1,5 giây — đứng cứng tại chỗ, không đi và không dùng được kỹ năng nào.\n\n"
                     + "CẤP 5: mỗi lần tung ra NĂM quả cầu băng thay vì ba.\n\n"
                     + "Hồi chiêu nhanh như Quả cầu lửa — đổi sức thiêu đốt lấy khả năng ghìm "
                     + "chân kẻ địch.";
            case CapDo.KyGioLoc:
                return "Một cơn lốc xoáy phóng thẳng về phía trước. Gió cuộn một chiều từ "
                     + "chân lên miệng lốc, kéo theo khói bụi đen xoáy bốc lên và để lại vệt bụi phía sau. Lốc đi "
                     + "XUYÊN QUA mọi vật cản và người chơi, rồi tự tan sau 4,5 giây.\n\n"
                     + "Trong lòng lốc luôn lóe hai tia sét đánh từ đỉnh xuống (chỉ là hiệu ứng, không gây thêm sát thương).\n\n"
                     + "Lốc lướt qua một kẻ địch thì gây 75 sát thương — mỗi kẻ chỉ bị đánh một lần.\n\n"
                     + "Mỗi lần trúng có 55% khả năng HẤT TUNG kẻ địch lên khỏi mặt đất trong 0,5 giây: không "
                     + "đi, không đánh được, và chiêu đang niệm dở bị NGẮT NGAY.\n\n"
                     + "Lốc lướt qua lò lửa thì dập tắt lửa, 30 giây sau lò cháy lại.\n\n"
                     + "CẤP 5: phóng cùng lúc HAI cơn lốc song song, cách nhau 4 m — kẻ đứng giữa bị cả hai quét qua thì trúng hai lần — "
                     + "nhưng tốn GẤP ĐÔI năng lượng.";
            case CapDo.KyHoaLocXoay:
                return "Cơn GIÓ LỐC bạn vừa phóng ra sẽ PHÌNH TO thành một cơn LỐC XOÁY thật sự — vẫn bay nhanh "
                     + "9,5 m/s như Gió lốc, nhưng cuốn bổng mọi kẻ địch và cả cảnh vật lên trời như Lốc xoáy.\n\n"
                     + "Sát thương cộng cả hai: kẻ địch chạm vào ăn ngay đòn 75 của Gió lốc (một lần mỗi kẻ), rồi bị cuốn "
                     + "lên và tiếp tục chịu 20 sát thương mỗi giây cùng những tia sét trong lòng lốc. Mỗi phần mạnh theo "
                     + "CẤP HIỆN TẠI của kỹ năng tương ứng.\n\n"
                     + "Gió lốc cấp 5 phóng hai cơn thì một lần bấm hoá cả hai. Cơn lốc mới sống 6 giây.\n\n"
                     + "Không có cơn Gió lốc nào đang bay thì kỹ năng không tung ra và KHÔNG tốn năng lượng.\n\n"
                     + "Tốn 45 năng lượng, hồi chiêu 0,5 giây.";
            case CapDo.KyCauDien:
                return "Gọi ra một QUẢ CẦU ĐIỆN lơ lửng ngay cạnh kẻ địch gần chỗ bạn ngắm nhất — không có ai ở đó "
                     + "thì quả cầu đứng đúng chỗ ngắm. Tầm ngắm 18 m, bằng Thiên thạch.\n\n"
                     + "Cứ 0,4 giây quả cầu bắn ra một lượt tia điện, tối đa 10 lượt. Mỗi lượt bắn tới 5 tia, MỖI KẺ ĐỊCH "
                     + "MỘT TIA — chọn 5 kẻ gần quả cầu nhất trong 9 m, tính cả quái vật lẫn người chơi khác.\n\n"
                     + "Mỗi tia gây sát thương bằng GIỰT SÉT ở cấp 5, và có 30% khả năng làm kẻ trúng đòn CHOÁNG 1,5 giây — "
                     + "không đi, không đánh, không tung được phép.\n\n"
                     + "Chưa bắn đủ 10 lượt thì quả cầu VẪN ĐỨNG NGUYÊN CHỖ ĐÓ chờ kẻ địch tới gần — quá 20 giây không ai tới "
                     + "thì mới tan.\n\n"
                     + "Hồi chiêu 5 giây.";
            case CapDo.KyTangHinh:
                return "Cả thân người hoá trong suốt: quái vật KHÔNG THẤY và không đánh bạn nữa. Người chơi khác chỉ thấy "
                     + "vài đường nét mờ khi bạn DI CHUYỂN; bạn đứng yên thì họ không thấy gì.\n\n"
                     + "Trong 20 giây tàng hình, bạn MIỄN mọi hiệu ứng (đóng băng, làm chậm, choáng, ngã, hất tung, thiêu đốt) — "
                     + "các hiệu ứng đang dính cũng bị xoá — và đi nhanh hơn 20%. Vẫn mất máu nếu bị đánh trúng.\n\n"
                     + "Kỹ năng GÂY SÁT THƯƠNG đầu tiên bạn tung ra mạnh GẤP ĐÔI — TOÀN BỘ sát thương của kỹ năng ấy, "
                     + "nghĩa là mọi vệt Mưa băng rơi xuống, mọi quả cầu trong chùm, cả sát thương cháy theo sau. Ngay lúc "
                     + "tung ra, tàng hình tan. Khiên, bình máu và bình mana không làm tan tàng hình.\n\n"
                     + "Hai giây cuối, thân bạn NHẤP NHÁY để bạn biết sắp hiện hình — chỉ mình bạn thấy dấu hiệu này.\n\n"
                     + "Hồi chiêu 30 giây.";
            case CapDo.KyLuaDiaNguc:
                return "Phóng ra NĂM quả cầu lửa. Năm quả toả quạt rồi uốn cong, TỰ ĐUỔI theo tối đa "
                     + "năm kẻ địch (quái hoặc người chơi khác) gần bạn nhất trong 20 m — ít kẻ địch hơn thì quả dư lao vào kẻ gần nhất; "
                     + "mục tiêu gục giữa đường thì quả lửa chuyển sang kẻ còn sống gần nó nhất.\n\n"
                     + "Mỗi quả nổ gây 176 sát thương lửa (bằng Quả cầu lửa ở cấp 5) và THIÊU ĐỐT mọi kẻ trong vùng nổ như Quả cầu lửa. "
                     + "Không có ai quanh bạn thì năm quả bay thẳng. Đâm vào vật cản thì nổ tại đó.\n\n"
                     + "Nâng cấp: +20% sát thương mỗi cấp, lửa cháy lâu hơn.";
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
            case CapDo.KyBinhMau:
            case CapDo.KyBinhMana: nangLuong = 0f; hoiChieu = PlayerController.HoiChieuBinh; niemChu = 0f; break;
            case CapDo.KyQuaCauBang: nangLuong = pc.quaCauBangCost; hoiChieu = pc.quaCauBangCooldown; niemChu = pc.quaCauBangCastTime; break;
            case CapDo.KyGioLoc: nangLuong = pc.gioLocCost; hoiChieu = pc.gioLocCooldown; niemChu = pc.gioLocCastTime; break;
            case CapDo.KyLuaDiaNguc: nangLuong = pc.luaDiaNgucCost; hoiChieu = pc.luaDiaNgucCooldown; niemChu = pc.luaDiaNgucCastTime; break;
            case CapDo.KyTangHinh: nangLuong = pc.tangHinhCost; hoiChieu = pc.tangHinhCooldown; niemChu = pc.tangHinhCastTime; break;
            case CapDo.KyCauDien: nangLuong = pc.cauDienCost; hoiChieu = pc.cauDienCooldown; niemChu = pc.cauDienCastTime; break;
            case CapDo.KyHoaLocXoay: nangLuong = pc.hoaLocXoayCost; hoiChieu = pc.hoaLocXoayCooldown; niemChu = pc.hoaLocXoayCastTime; break;
        }
    }
}
