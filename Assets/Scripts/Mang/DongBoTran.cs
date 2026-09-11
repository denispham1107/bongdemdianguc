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

    /// <summary>Bo dong bo dan quai - dat kem de goi quai co cho di ve.</summary>
    public DongBoQuai quai;

    // ================================================================
    //  MAT KET NOI
    // ================================================================

    /// <summary>
    /// Im lang bao lau thi bao "dang cho tin hieu" - chua ket luan gi.
    ///
    /// Trinh duyet DUNG HAN game khi tab bi an: nguoi kia chuyen tab mot lat,
    /// hay khoa man hinh dien thoai, la goi tin ngung den. Neu ket luan "da
    /// roi tran" ngay thi mot cu liec sang tin nhan cung lam vo tran.
    /// </summary>
    public const float GiayChoTinHieu = 3f;

    /// <summary>
    /// Im lang bao lau thi coi nhu nguoi kia da roi tran han.
    ///
    /// Truoc day khong co gi ca: nguoi kia dong tab thi ban sao cua ho dung im
    /// mai mai, khong mot loi bao; chu phong thoat thi 5 giay sau ca dan quai
    /// ben khach bien mat, cung khong mot loi giai thich.
    /// </summary>
    public const float GiayMatKetNoi = 10f;

    public enum TinhTrangKetNoi { Tot, DangChoTinHieu, DaMat }

    public TinhTrangKetNoi TinhTrang { get; private set; }

    /// <summary>Goi DUNG MOT LAN khi ket luan nguoi kia da roi tran.</summary>
    public event System.Action KhiMatKetNoi;

    /// <summary>Bao lau roi khong nghe thay gi tu nguoi kia, giay.</summary>
    public float ImLangGiay
    {
        get { return daTungNghe ? Time.unscaledTime - ngheLanCuoi : 0f; }
    }

    float ngheLanCuoi;
    bool daTungNghe;
    bool daTungMo;

    void XetMatKetNoi()
    {
        if (TinhTrang == TinhTrangKetNoi.DaMat) return;     // ket luan roi thi thoi
        if (!daTungNghe) return;                            // chua noi xong thi chua tinh

        if (KenhTrucTiep.DaMo) daTungMo = true;

        // Kenh DONG han la dau hieu chac chan nhat - nguoi kia dong tab hoac
        // bam ve sanh (BackToMenu dong kenh truoc khi di).
        bool kenhDaDong = daTungMo && !KenhTrucTiep.DaMo;

        float im = ImLangGiay;
        if (kenhDaDong || im >= GiayMatKetNoi)
        {
            TinhTrang = TinhTrangKetNoi.DaMat;
            if (KhiMatKetNoi != null) KhiMatKetNoi();
            return;
        }

        TinhTrang = im >= GiayChoTinHieu ? TinhTrangKetNoi.DangChoTinHieu
                                         : TinhTrangKetNoi.Tot;
    }

    /// <summary>Bao lau hoi nhip moi giay - chi de do vong di-ve.</summary>
    public const float NhipHoiVong = 2f;

    /// <summary>
    /// Mot vong di-ve mat bao nhieu mili giay, trung binh truot.
    ///
    /// Lam min bang trung binh truot chu khong lay so do gan nhat: mot goi ket
    /// mang lam RTT vot len 300 ms trong dung mot nhip, ma neu tin ngay con so
    /// ay thi cu don ke tiep se bu tre gap ba - nguoi da nap sau goc tuong roi
    /// van an don.
    /// </summary>
    public float RttMs { get; private set; }

    float hoiVongLanSau;

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

        /// <summary>Cho ve o khung truoc - de tinh ra ho dang di nhanh bao nhieu.</summary>
        public Vector3 choKhungTruoc;
        public bool daCoChoTruoc;

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
            diemNgam = diemNgam,

            // Bao luon cho ben kia biet minh dang nhin thay ho tre bao nhieu.
            // Ho se lui ve dung khoanh khac nay roi moi tinh trung.
            doTreMs = (ushort)Mathf.RoundToInt(DoTreNguoiKiaThayMinh() * 1000f)
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

    void HoiNhip()
    {
        if (!KenhTrucTiep.DaMo) return;
        if (Time.unscaledTime < hoiVongLanSau) return;
        hoiVongLanSau = Time.unscaledTime + 1f / NhipHoiVong;

        GuiMotGoi(GoiTin.VietNhip(true, GioTran()));
    }

    /// <summary>
    /// Goi nhip den. Neu la cau HOI thi nem tra lai nguyen ven, neu la cau DAP
    /// thi lay hieu ra do tre.
    ///
    /// Ca hai dau tru deu la gio cua CHINH may nay, nen dong ho hai ben lech
    /// bao nhieu cung khong anh huong - do la ly do cai moc duoc nem tra lai
    /// nguyen ven chu khong ai viet lai no.
    /// </summary>
    void NhanMotNhip(byte[] b)
    {
        bool laHoi; int moc;
        if (!GoiTin.DocNhip(b, out laHoi, out moc)) { SoGoiHong++; return; }

        if (laHoi) { GuiMotGoi(GoiTin.VietNhip(false, moc)); return; }

        float vong = Mathf.Max(0f, GioTran() - moc);
        RttMs = RttMs <= 0f ? vong : Mathf.Lerp(RttMs, vong, 0.25f);
    }

    /// <summary>
    /// Nguoi kia dang nhin thay MINH tre bao nhieu giay.
    ///
    /// Hai phan cong lai: nua vong di-ve (goi tin bay sang do), va do day cua
    /// dem noi suy ben do (ho co tinh xem cham lai chung ay de bu jitter).
    /// Dem ben do khong doc duoc tu day, nhung hai may chay cung mot cong thuc
    /// tren cung mot duong truyen nen dem cua minh la uoc luong sat nhat.
    /// </summary>
    public float DoTreNguoiKiaThayMinh()
    {
        float nuaVong = RttMs * 0.5f / 1000f;

        float dem = 0f;
        foreach (var cap in nguoiKhac)
        {
            float d = cap.Value.noiSuy.DemGiay;
            if (d > dem) dem = d;
        }

        return Mathf.Clamp(nuaVong + dem, 0f, BuTre.LuiToiDaGiay);
    }

    /// <summary>
    /// Nhan vat nay la nguoi choi so may. 255 neu khong phai nguoi choi nao ca.
    ///
    /// Dung khi ke lai mot don cua quai: phai noi ro don nham vao AI, de ben
    /// kia biet co phai tu tru mau minh khong.
    /// </summary>
    public byte ChiSoCua(Transform t)
    {
        if (t == null) return 255;
        if (toi != null && t == toi.transform) return chiSoCuaToi;

        foreach (var cap in nguoiKhac)
            if (cap.Value.nhanVat != null && cap.Value.nhanVat.transform == t)
                return cap.Key;

        return 255;
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
        XetMatKetNoi();
        VeNguoiKhac(dt);
        GuiLaiPhepDangCho();
        HoiNhip();

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

            // Bat cu goi nao doc duoc cung la dau hieu nguoi kia con do
            ngheLanCuoi = Time.unscaledTime;
            daTungNghe = true;

            // Doc byte dau de biet goi loai gi. Truoc day o day chi co mot loai
            // nen doc thang - them loai thu hai ma quen phan loai thi goi ky
            // nang se bi dem la "goi hong".
            byte loai = GoiTin.LoaiCuaGoi(b);

            if (loai == GoiTin.LoaiKyNang) { NhanMotPhep(b); continue; }
            if (loai == GoiTin.LoaiNhip) { NhanMotNhip(b); continue; }
            if (loai == GoiTin.LoaiBangSo)
            {
                int wave, kills, con; float dotMoi;
                if (!GoiTin.DocBangSo(b, out wave, out kills, out con, out dotMoi))
                { SoGoiHong++; continue; }
                var dir = GameDirector.Instance;
                if (dir != null) dir.NhanBangSoTuChuPhong(wave, kills, con, dotMoi);
                continue;
            }
            if (loai == GoiTin.LoaiDonQuai)
            {
                // Chi may khach dien lai don - chu phong da danh that roi,
                // dien lai nua la mot cu vung kiem an mau hai lan.
                if (quai != null && !GameDirector.LaTrongTaiCuaQuai)
                    quai.NhanDonQuai(b, chiSoCuaToi);
                continue;
            }
            if (loai == GoiTin.LoaiQuai)
            {
                // May khach nhan dan quai tu chu phong. Chu phong khong nghe
                // goi loai nay: no la nguoi ke chuyen, khong phai nguoi nghe.
                if (quai != null && !GameDirector.LaTrongTaiCuaQuai) quai.NhanGoiQuai(b);
                continue;
            }
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
        n.nhanVat.TungPhepTheoMang(p.kyNang, p.diemNgam, p.doTreMs / 1000f);
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

            Vector3 choMoi = n.noiSuy.ViTriHienThi;

            // TOC DO THAT, do bang chinh quang duong vua di.
            //
            // Loi da vap: ban sao duoc DAT THANG vi tri nen CharacterController
            // cua no khong bao gio chuyen dong, va moi bo hoat hinh trong game
            // deu doc toc do tu cc.velocity. Ket qua: nguoi choi kia TRUOT tren
            // mat dat nhu dang bay, chan khong nhuc nhich. Nguoi dung goi dung
            // ten no: "thay nguoi nay dang bay".
            //
            // Khong dung co "dangChay" trong goi tin lam nguon: no chi noi
            // co/khong, ma buoc chan can biet NHANH BAO NHIEU thi moi khop
            // nhip. Quang duong chia thoi gian moi ra duoc con so ay.
            float tocDo = 0f;
            if (n.daCoChoTruoc && dt > 0.0001f)
            {
                Vector3 di = choMoi - n.choKhungTruoc;
                di.y = 0f;
                tocDo = di.magnitude / dt;
            }
            n.choKhungTruoc = choMoi;
            n.daCoChoTruoc = true;

            t.position = choMoi;
            t.rotation = Quaternion.Euler(0f, n.noiSuy.GocHienThi, 0f);

            if (batLai) cc.enabled = true;

            // Cho chan buoc khop voi viec dang di hay dung, tren CA HAI kieu bo
            // hoat hinh: nhan vat dung bang code va nhan vat dung tu model san.
            float tocDo01 = Mathf.Clamp01(tocDo / Mathf.Max(0.1f, n.nhanVat.moveSpeed));

            var anim = n.nhanVat.GetComponentInChildren<ProceduralAnimator>();
            if (anim != null) anim.SetMoveSpeed(tocDo01);

            var hh = n.nhanVat.GetComponentInChildren<NguoiChoiHoatHinh>();
            if (hh != null) hh.tocDoEp = tocDo01;
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
