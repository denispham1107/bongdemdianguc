using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GUI TRANG THAI CUA MINH, NHAN VA VE NHAN VAT CUA NGUOI KHAC.
///
/// Day la cho bon manh ghep cua giai doan 2 noi lai voi nhau:
///   - <see cref="KenhTrucTiep"/> (buoc 0) cho duong truyen,
///   - <see cref="GoiInput"/> (buoc 1) cho y muon,
///   - <see cref="DuDoan"/> (buoc 2) cho nhan vat cua minh,
///   - <see cref="NguoiChoiKhac"/> (buoc 3) cho nhan vat cua nguoi khac,
///   - <see cref="NoiSuy"/> (buoc nay) cho no di muot.
///
/// NHIP GUI 60 LAN MOI GIAY - va day la mot bai hoc phai tra gia.
///
/// Ban dau toi chon 20 lan/giay cho tiet kiem bang thong. Do ra: dem noi suy
/// phinh len 132 ms, vi dem KHONG THE mong hon khoang cach giua hai moc. Gui
/// 20 lan/giay nghia la hai moc cach nhau 50 ms, va dem phai chua duoc it nhat
/// tung ay moi luon co moc sau de ve o giua. Cong ca duong truyen thi do tre
/// nhin thay nhau len ~154 ms - gap doi muc tieu 50-80 ms.
///
/// Gui 60 lan/giay thi hai moc cach nhau 17 ms, dem mong han. Cai gia phai
/// tra la bang thong: 54 byte x 60 = 3,2 KB/giay moi nguoi. Tren 4G thi do la
/// con so khong dang ke - re hon nhieu so voi 80 ms do tre.
///
/// Bai hoc: tiet kiem bang thong o day KHONG doi lay duoc gi, ma con lam hong
/// dung cai dang co gang dat duoc.
/// </summary>
public class DongBoTran : MonoBehaviour
{
    /// <summary>Bao nhieu lan gui trang thai moi giay.</summary>
    public const int NhipGui = 60;

    /// <summary>Nhan vat cua may nay.</summary>
    public PlayerController toi;

    /// <summary>Chi so cua minh trong phong (0..3).</summary>
    public byte chiSoCuaToi;

    /// <summary>
    /// Goi ky nang duoc gui LAP LAI may lan, cach nhau bao lau.
    ///
    /// Duong truyen la UDP: mat goi la chuyen binh thuong. Voi trang thai thi
    /// khong sao - 17 ms sau da co goi moi. Nhung tung phep chi xay ra DUNG
    /// MOT LAN: mat la nguoi kia khong bao gio thay chieu do, hoac te hon, ho
    /// thay minh mat mau ma khong hieu vi sao. Nen gui ba lan, va ben nhan bo
    /// ban sao theo so thu tu.
    ///
    /// Ba lan x 14 byte = 42 byte cho mot lan tung phep. Khong dang ke.
    /// </summary>
    public const int SoLanGuiLaiPhep = 3;
    public const float CachNhauGuiLai = 0.05f;

    class MotNguoiKhac
    {
        public PlayerController nhanVat;
        public NoiSuy noiSuy = new NoiSuy();

        /// <summary>So thu tu phep gan nhat da thi hanh - de bo ban sao.</summary>
        public int phepDaLam = -1;
    }

    /// <summary>Mot goi phep dang cho gui lai.</summary>
    struct PhepChoGui
    {
        public byte[] goi;
        public int conLai;
        public float guiLanSau;
    }

    readonly Dictionary<byte, MotNguoiKhac> nguoiKhac = new Dictionary<byte, MotNguoiKhac>();
    readonly GoiTin.MotNguoi[] demGui = new GoiTin.MotNguoi[1];
    readonly GoiTin.MotNguoi[] demNhan = new GoiTin.MotNguoi[8];
    readonly List<PhepChoGui> phepChoGui = new List<PhepChoGui>();

    int soPhepDaTung;

    float guiLanSau;
    float batDauLuc;

    // ---- So dem de chan doan ----
    public int SoGoiDaGui { get; private set; }
    public int SoGoiDaNhan { get; private set; }
    public int SoGoiHong { get; private set; }
    public int SoByteDaGui { get; private set; }
    public int SoPhepDaGui { get; private set; }
    public int SoPhepDaNhan { get; private set; }
    public int SoPhepBoVITrung { get; private set; }

    void Awake()
    {
        batDauLuc = Time.unscaledTime;
    }

    void OnEnable()  { GanTaiNghe(); }
    void OnDisable() { if (toi != null) toi.DaTungPhep -= KhiToiTungPhep; }

    /// <summary>Nghe nhan vat cua may nay tung phep. Goi lai duoc nhieu lan -
    /// <c>toi</c> thuong duoc gan SAU khi component nay ra doi.</summary>
    public void GanTaiNghe()
    {
        if (toi == null) return;
        toi.DaTungPhep -= KhiToiTungPhep;
        toi.DaTungPhep += KhiToiTungPhep;
    }

    void KhiToiTungPhep(int kyNang, Vector3 diemNgam)
    {
        if (!KenhTrucTiep.DaMo) return;

        var goi = GoiTin.VietKyNang(new GoiTin.MotPhep
        {
            chiSo = chiSoCuaToi,
            kyNang = (byte)kyNang,
            soThuTu = ++soPhepDaTung,
            diemNgam = diemNgam
        });

        // Gui ngay mot lan, roi xep hang gui lai
        GuiMotGoi(goi);
        SoPhepDaGui++;
        phepChoGui.Add(new PhepChoGui
        {
            goi = goi,
            conLai = SoLanGuiLaiPhep - 1,
            guiLanSau = Time.unscaledTime + CachNhauGuiLai
        });
    }

    void GuiMotGoi(byte[] b)
    {
        if (KenhTrucTiep.Gui(GoiTin.SangChuoi(b)))
        {
            SoGoiDaGui++;
            SoByteDaGui += b.Length;
        }
    }

    void GuiLaiPhepDangCho()
    {
        for (int i = phepChoGui.Count - 1; i >= 0; i--)
        {
            var p = phepChoGui[i];
            if (Time.unscaledTime < p.guiLanSau) continue;

            GuiMotGoi(p.goi);
            p.conLai--;
            if (p.conLai <= 0) { phepChoGui.RemoveAt(i); continue; }

            p.guiLanSau = Time.unscaledTime + CachNhauGuiLai;
            phepChoGui[i] = p;
        }
    }

    /// <summary>Gio tran dau, mili giay ke tu luc vao. Dung lam moc thoi gian
    /// trong goi tin - khong gui ca dau thoi gian Unix cho ton byte.</summary>
    public int GioTran()
    {
        return Mathf.RoundToInt((Time.unscaledTime - batDauLuc) * 1000f);
    }

    /// <summary>Them mot nguoi choi khac vao tran.</summary>
    public void ThemNguoi(byte chiSo, PlayerController nv)
    {
        if (nv == null) return;
        nguoiKhac[chiSo] = new MotNguoiKhac { nhanVat = nv };
    }

    public void BoNguoi(byte chiSo)
    {
        MotNguoiKhac n;
        if (!nguoiKhac.TryGetValue(chiSo, out n)) return;
        NguoiChoiKhac.Bo(n.nhanVat);
        nguoiKhac.Remove(chiSo);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        NhanHet();
        VeNguoiKhac(dt);
        GuiLaiPhepDangCho();

        if (Time.unscaledTime >= guiLanSau)
        {
            guiLanSau = Time.unscaledTime + 1f / NhipGui;
            GuiTrangThaiCuaToi();
        }
    }

    void GuiTrangThaiCuaToi()
    {
        if (toi == null || !KenhTrucTiep.DaMo) return;

        var mau = toi.GetComponent<Damageable>();

        demGui[0] = new GoiTin.MotNguoi
        {
            chiSo = chiSoCuaToi,
            viTri = toi.transform.position,
            gocY = toi.transform.eulerAngles.y,
            mau01 = mau != null && mau.maxHealth > 0f ? mau.health / mau.maxHealth : 1f,
            dangChay = toi.input.huongDi.sqrMagnitude > 0.01f || toi.DangCoDiemDen,
            daChet = mau != null && mau.IsDead
        };

        GuiMotGoi(GoiTin.VietTrangThai(GioTran(), demGui, 1));
    }

    /// <summary>Vet sach hang cho - mot khung hinh co the co vai goi den cung luc.</summary>
    void NhanHet()
    {
        string s;
        while ((s = KenhTrucTiep.Nhan()).Length > 0)
        {
            byte[] b = GoiTin.TuChuoi(s);
            if (b == null) { SoGoiHong++; continue; }

            // Doc byte dau de biet goi loai gi. Truoc day o day chi co mot loai
            // nen doc thang - them loai thu hai ma quen phan loai thi goi ky
            // nang se bi dem la "goi hong".
            byte loai = GoiTin.LoaiCuaGoi(b);

            if (loai == GoiTin.LoaiKyNang) { NhanMotPhep(b); continue; }
            if (loai != GoiTin.LoaiTrangThai) { SoGoiHong++; continue; }

            int moc;
            int soNguoi = GoiTin.DocTrangThai(b, demNhan, out moc);
            if (soNguoi < 0) { SoGoiHong++; continue; }

            SoGoiDaNhan++;

            for (int i = 0; i < soNguoi; i++)
            {
                var p = demNhan[i];
                if (p.chiSo == chiSoCuaToi) continue;   // trang thai cua chinh minh

                MotNguoiKhac n;
                if (!nguoiKhac.TryGetValue(p.chiSo, out n)) continue;

                n.noiSuy.Nhan(moc, p.viTri, p.gocY, p.dangChay);
                ApMau(n, p.mau01, p.daChet);
            }
        }
    }

    /// <summary>
    /// NGUOI KIA VUA TUNG MOT PHEP - cho ban sao cua ho tung dung phep ay.
    ///
    /// Phep chay tren may nay nhu moi phep khac: co hieu ung, co sat thuong,
    /// va co the trung nhan vat CUA MINH. Ban sao tu bo qua chinh no
    /// (boQua trong PlayerController.Release), nen nguoi tung khong tu thieu.
    /// </summary>
    void NhanMotPhep(byte[] b)
    {
        GoiTin.MotPhep p;
        if (!GoiTin.DocKyNang(b, out p)) { SoGoiHong++; return; }

        SoGoiDaNhan++;
        if (p.chiSo == chiSoCuaToi) return;

        MotNguoiKhac n;
        if (!nguoiKhac.TryGetValue(p.chiSo, out n) || n.nhanVat == null) return;

        // Goi ky nang duoc gui ba lan cho chac - hai lan sau la ban sao, bo di.
        // Khong bo thi mot cu bam ra ba dam lua va ba lan sat thuong.
        if (p.soThuTu <= n.phepDaLam) { SoPhepBoVITrung++; return; }
        n.phepDaLam = p.soThuTu;

        SoPhepDaNhan++;
        n.nhanVat.TungPhepTheoMang(p.kyNang, p.diemNgam);
    }

    /// <summary>
    /// MAU CUA NGUOI KHAC LAY THANG TU MAY HO, KHONG TU TINH.
    ///
    /// Quy uoc: MOI MAY LA TRONG TAI CUA CHINH NHAN VAT MINH. Phep cua nguoi
    /// kia bay sang day, trung nhan vat cua toi, thi MAY TOI tru mau roi bao
    /// sang - va may kia chi viec hien thi con so ay. Nguoc lai cung vay.
    ///
    /// Vi sao khong cho moi may tu tinh ca hai ben: CombatUtil.AreaFreeze va
    /// AreaShock dung Random.value, moi may gieo rieng - nguoi nay thay dich
    /// dong bang, nguoi kia thay khong. Va sat thuong tinh tren vi tri, ma vi
    /// tri cua nguoi kia o day luon tre hon ben do vai chuc mili giay.
    ///
    /// Dat thang health chu khong goi TakeDamage: TakeDamage con ban ra hieu
    /// ung trung don va tinh lai chet/song - lam hai lan thi giat ca hai dau.
    /// </summary>
    void ApMau(MotNguoiKhac n, float mau01, bool daChet)
    {
        if (n.nhanVat == null) return;

        var mau = n.nhanVat.GetComponent<Damageable>();
        if (mau == null || mau.maxHealth <= 0f) return;

        mau.health = Mathf.Clamp(mau01 * mau.maxHealth, 0f, mau.maxHealth);

        // Chet thi phai chet han - khong the de mot xac van chay quanh. Nguoc
        // lai thi KHONG hoi sinh: song lai la viec cua man choi, khong phai
        // cua mot goi tin den muon.
        if (daChet && !mau.IsDead) mau.Die();
    }

    void VeNguoiKhac(float dt)
    {
        foreach (var cap in nguoiKhac)
        {
            var n = cap.Value;
            if (n.nhanVat == null) continue;

            n.noiSuy.CapNhat(dt);
            if (!n.noiSuy.SanSang) continue;

            // Dat thang, KHONG qua CharacterController: vi tri nay den tu may
            // kia, no da tinh va cham o ben do roi. Cho CharacterController
            // xen vao lan nua thi nhan vat bi ket vao tuong hai lan.
            var t = n.nhanVat.transform;
            var cc = n.nhanVat.GetComponent<CharacterController>();
            bool batLai = cc != null && cc.enabled;
            if (batLai) cc.enabled = false;

            t.position = n.noiSuy.ViTriHienThi;
            t.rotation = Quaternion.Euler(0f, n.noiSuy.GocHienThi, 0f);

            if (batLai) cc.enabled = true;

            // Cho chan buoc khop voi viec dang di hay dung
            var anim = n.nhanVat.GetComponentInChildren<ProceduralAnimator>();
            if (anim != null) anim.SetMoveSpeed(n.noiSuy.DangChay ? 1f : 0f);
        }
    }

    /// <summary>Do day dem cua mot nguoi - de chan doan.</summary>
    public float DemCuaNguoi(byte chiSo)
    {
        MotNguoiKhac n;
        return nguoiKhac.TryGetValue(chiSo, out n) ? n.noiSuy.DemGiay : 0f;
    }

    public NoiSuy NoiSuyCuaNguoi(byte chiSo)
    {
        MotNguoiKhac n;
        return nguoiKhac.TryGetValue(chiSo, out n) ? n.noiSuy : null;
    }
}
