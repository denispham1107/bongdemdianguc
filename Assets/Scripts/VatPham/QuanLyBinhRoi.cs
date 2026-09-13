using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BINH MAU / BINH MANA ROI RA KHI GIET QUAI - chung ca phong.
///
/// Nguoi dung (13/09/2026): giet xong quai co 10% ra binh mau, 10% ra binh mana;
/// nguoi choi toi gan thi binh tu bay vao nguoi va so binh hien tren o ky nang.
/// Choi nhieu nguoi thi binh la CUA CHUNG ca phong - ai toi truoc nguoi ay duoc.
///
/// TRONG TAI LA MAY CHU PHONG (cung la may chay AI quai, <see cref="GameDirector.LaTrongTaiCuaQuai"/>):
///   1. Quai chet tren may chu phong -> gieo 10% / 10% -> binh co SO HIEU, gui
///      goi LoaiBinhRoi cho ca phong.
///   2. Nhan vat CUA MOT MAY vao pham vi hut -> may ay xin (LoaiXinBinh). Chu
///      phong tu xin thi khong qua mang.
///   3. Chu phong giao binh cho nguoi xin DAU TIEN (binh da co chu thi bo qua
///      moi loi xin sau), bao ca phong (LoaiBinhThuoc).
///   4. Moi may thay binh bay vao dung nguoi ay; CHI may cua nguoi ay cong so binh.
///
/// Vi sao khong de moi may tu thay minh toi gan la tu nhat: hai nguoi dung hai
/// ben mot binh cung luc thi ca hai cung nhat duoc - mot binh thanh hai.
///
/// Kenh mang hanh xu nhu UDP (co the mat goi) nen ba loai goi deu GUI LAP 3 lan,
/// va ben nhan bo qua ban lap theo so hieu. Goi "thuoc ve" den TRUOC goi "roi ra"
/// (thu tu dao) thi ghi nho lai, luc binh hien ra la giao ngay.
/// </summary>
public static class QuanLyBinhRoi
{
    public const float TiLeRoiBinhMau = 0.10f;
    public const float TiLeRoiBinhMana = 0.10f;

    /// <summary>Nhan vat vao trong ban kinh nay (met, tinh ngang) thi binh tu bay vao.</summary>
    public const float BanKinhHut = 3.5f;

    /// <summary>Moi goi binh gui lap bay nhieu lan (kenh co the mat goi).</summary>
    public const int SoLanGui = 3;

    static readonly Dictionary<int, BinhRoi> binh = new Dictionary<int, BinhRoi>();
    static readonly Dictionary<int, byte> chuDenTruoc = new Dictionary<int, byte>();
    static readonly HashSet<int> daXong = new HashSet<int>();
    static int soHieuTiep = 1;

    /// <summary>Tong so binh da gieo ra tren may nay (phep thu doc).</summary>
    public static int SoDaGieo { get; private set; }

    /// <summary>Moi binh dang ton tai (ke ca dang bay vao nguoi).</summary>
    public static IEnumerable<BinhRoi> TatCa { get { return binh.Values; } }

    public static int SoBinhDangCo { get { return binh.Count; } }

    public static BinhRoi TimBinh(int soHieu)
    {
        BinhRoi b;
        return binh.TryGetValue(soHieu, out b) ? b : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhoiDong()
    {
        SceneManager.sceneLoaded -= KhiNapCanh;
        SceneManager.sceneLoaded += KhiNapCanh;
        DonSach();
    }

    static void KhiNapCanh(Scene c, LoadSceneMode m) { if (m == LoadSceneMode.Single) DonSach(); }

    /// <summary>Xoa het binh va so dem - vao tran moi (so hieu dem lai tu 1 tren moi may).</summary>
    public static void DonSach()
    {
        foreach (var b in binh.Values) if (b != null) Object.Destroy(b.gameObject);
        binh.Clear();
        chuDenTruoc.Clear();
        daXong.Clear();
        soHieuTiep = 1;
        SoDaGieo = 0;
    }

    static DongBoTran Mang
    {
        get
        {
            if (!TranHienTai.DangChoiMang) return null;
            var d = Object.FindAnyObjectByType<DongBoTran>();
            return d != null && d.CoKenhNaoMo ? d : null;
        }
    }

    // ================================================================
    //  1. GIEO (chi may trong tai quai)
    // ================================================================

    /// <summary>Mot con quai vua chet o <paramref name="viTri"/>: gieo 10% binh mau, 10% binh mana.</summary>
    public static void GieoKhiQuaiChet(Vector3 viTri)
    {
        if (!GameDirector.LaTrongTaiCuaQuai) return;
        bool mau = Random.value < TiLeRoiBinhMau;
        bool mana = Random.value < TiLeRoiBinhMana;
        // Ra ca hai thi dat lech nhau, khong chong len nhau thanh mot hinh
        if (mau) Roi(CapDo.KyBinhMau, viTri + (mana ? new Vector3(-0.45f, 0f, 0f) : Vector3.zero));
        if (mana) Roi(CapDo.KyBinhMana, viTri + (mau ? new Vector3(0.45f, 0f, 0f) : Vector3.zero));
    }

    /// <summary>Tha mot binh xuong dat va bao ca phong. Tra ve so hieu binh.</summary>
    public static int Roi(int ky, Vector3 viTri)
    {
        viTri.y = VfxFactory.GroundY(viTri);
        int id = soHieuTiep++;
        if (soHieuTiep > 65000) soHieuTiep = 1;
        SoDaGieo++;
        TaoBinh(id, ky, viTri);
        var m = Mang;
        if (m != null) m.GuiNhieuLan(GoiTin.VietBinhRoi(id, ky, viTri), SoLanGui);
        return id;
    }

    static BinhRoi TaoBinh(int id, int ky, Vector3 viTri)
    {
        var b = BinhRoi.Tao(id, ky, viTri);
        binh[id] = b;
        return b;
    }

    // ================================================================
    //  NHAN TU MANG
    // ================================================================

    /// <summary>May khach: chu phong bao co binh roi ra.</summary>
    public static void NhanRoi(int id, int ky, Vector3 viTri)
    {
        if (!CapDo.LaKyBinh(ky)) return;
        if (binh.ContainsKey(id) || daXong.Contains(id)) return;       // goi lap / da nhat roi
        var b = TaoBinh(id, ky, viTri);
        byte ghe;
        if (chuDenTruoc.TryGetValue(id, out ghe)) { chuDenTruoc.Remove(id); GiaoCho(b, ghe); }
    }

    /// <summary>May chu phong: mot may khach xin nhat binh.</summary>
    public static void NhanXin(int id, byte ghe) { QuyetDinh(id, ghe); }

    /// <summary>May khach: chu phong bao binh nay thuoc ve ghe nao.</summary>
    public static void NhanThuoc(int id, byte ghe)
    {
        BinhRoi b;
        if (binh.TryGetValue(id, out b) && b != null) { if (!b.DaCoChu) GiaoCho(b, ghe); }
        else if (!daXong.Contains(id)) chuDenTruoc[id] = ghe;
    }

    // ================================================================
    //  2-3. XIN VA QUYET DINH
    // ================================================================

    /// <summary>Goi tu BinhRoi: nhan vat CUA MAY NAY vua vao pham vi hut.</summary>
    public static void XinNhat(BinhRoi b)
    {
        if (b == null || b.DaCoChu || b.DaXin) return;
        var m = Mang;
        if (m == null) { GiaoCho(b, 0, true); return; }             // choi mot minh
        if (m.LaChuPhong) { QuyetDinh(b.SoHieu, m.chiSoCuaToi); return; }
        b.DanhDauDaXin();
        m.GuiNhieuLan(GoiTin.VietXinBinh(b.SoHieu, m.chiSoCuaToi), SoLanGui);
    }

    /// <summary>Chu phong giao binh cho nguoi xin DAU TIEN.</summary>
    static void QuyetDinh(int id, byte ghe)
    {
        BinhRoi b;
        if (!binh.TryGetValue(id, out b) || b == null || b.DaCoChu) return;
        GiaoCho(b, ghe);
        var m = Mang;
        if (m != null) m.GuiNhieuLan(GoiTin.VietBinhThuoc(id, ghe), SoLanGui);
    }

    static void GiaoCho(BinhRoi b, byte ghe) { GiaoCho(b, ghe, false); }

    static void GiaoCho(BinhRoi b, byte ghe, bool motMinh)
    {
        var m = Mang;
        bool laToi = motMinh || m == null || ghe == m.chiSoCuaToi;
        Transform dich = null;
        if (laToi)
        {
            var dir = GameDirector.Instance;
            dich = dir != null ? dir.player : null;
            if (m != null && m.NhanVatCuaGhe(ghe) != null) dich = m.NhanVatCuaGhe(ghe).transform;
        }
        else if (m != null && m.NhanVatCuaGhe(ghe) != null) dich = m.NhanVatCuaGhe(ghe).transform;
        b.BayVao(dich, ghe, laToi);
    }

    // ================================================================
    //  4. BAY TOI NOI
    // ================================================================

    /// <summary>Goi tu BinhRoi luc da bay vao nguoi (hoac nguoi nhan da bien mat).</summary>
    public static void DaToiNoi(BinhRoi b)
    {
        if (b == null) return;
        if (b.LaCuaToi) CapDo.ThemBinh(b.Ky);
        binh.Remove(b.SoHieu);
        daXong.Add(b.SoHieu);
    }
}
