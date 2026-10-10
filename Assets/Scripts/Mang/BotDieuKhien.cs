using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BO NAO CUA MOT MAY BOT - gan vao nhan vat BOT tren MAY CHU PHONG (KhoiDongTranMang.SinhBot).
///
/// BOT la PlayerController that (cung prefab nguoi choi), tuDocInput tat, mau do may nay quyet (khong phai ban sao). Moi
/// khung bo nao nay dat <see cref="PlayerController.input"/> TRUOC khi PlayerController.Update thi hanh no - cung duong
/// ma ban phim di (GoiInput), nen BOT di, tung phep, uong binh y het nguoi choi.
///
/// BUOC 3 (08/10/2026) - DI LAI. Nguoi dung: "biet tu kiem quai vat, doi thu nguoi choi khac, biet ne di vong cac chuong
/// ngai vat de khong bi mac ket, biet dung truoc vuc doi duong khac de khong bi rot xuong dia nguc".
///   - MUC TIEU (09/10/2026 nguoi dung: "dau tran han che tu kiem nhau giao tranh, uu tien giet quai de len cap; dat cap 7
///     moi uu tien kiem nhau giao tranh, tru khi co nguoi choi / BOT khac trong pham vi gan"): bo dong doi, nguoi dang Tang
///     hinh, ke da chet, ke duoi vuc. Doi thu trong <see cref="TamGiaoTranh"/> m thi giao tranh ngay (moi cap). Duoi cap
///     <see cref="CapSanNguoi"/>: DANH TRA nguoi vua danh minh trong <see cref="GiayDanhTra"/> s (du xa), khong thi di giet
///     QUAI gan nhat ca ban do, het quai thi DI TUAN. Tu cap 7: quai ap sat trong 20 m thi giet truoc, roi SAN doi thu gan
///     nhat ca ban do, khong con doi thu thi quai. Chon lai theo nhip phan xa cua do kho (<see cref="NhipPhanXa"/>).
///     10/10/2026 TAM NHIN 25 m (nguoi dung): moi thu tren chi tinh trong tam nhin (<see cref="TamNhin.NhinThay"/> - 25 m quanh BOT hoac
///     dong doi con song); "ca ban do" nay la "trong tam nhin". Khong thay ai -> DI TUAN KHAP BAN DO: luoi 8 x 8 o, toi o lau chua ghe
///     nhat (<see cref="ChonDiemTuan"/>).
///   - DUONG DI: A* tren luoi BanDoBot (dung mot lan luc vao tran), lam thang, tinh lai moi 1,2 s / khi dich doi cho. Toi gan
///     muc tieu <see cref="TamGiu"/> m thi dung (buoc 4: tung phep tu day).
///   - CHONG VUC: moi khung do dat PHIA TRUOC 0,9 / 1,7 m (tia xuong lop Ground + luoi). Hut chan -> DUNG, cam tam cho do,
///     tim duong khac.
///   - CHONG KET: cu 0,8 s ma muon di nhung dich chua toi 0,35 m -> cam tam cho chan truoc mat, tim lai; ket 3 lan lien -> di
///     vong ra mot diem ngau nhien gan do.
/// BUOC 4 (08/10/2026) - DANH. Nguoi dung: "biet su dung skill, combo skill cua cac he, dung hoi mau hoi mana".
///   - CONG DIEM khi co diem ky nang: MayBot.TieuDiem theo ke hoach he chinh (dung duong mo khoa theo bac).
///   - CHIEU: moi lan ra chieu (nhip theo do kho <see cref="NhipRaChieu"/>) chon theo uu tien cua he - Khien khi mau < 60%
///     va bi ap sat; roi chieu LIEN HOAN cua he (Lua: Thien thach danh nga -> Qua cau lua, nhom thi Lua dia nguc; Bang: Mua
///     bang dong bang -> Qua cau bang, mau thap thi Tang hinh; Set: Sam set choang -> Giut set, nhom thi Qua cau dien; Phong:
///     May giong, Loc xoay, Hoa loc xoay khi co Gio loc dang bay) voi xac suat <see cref="TiLeLienHoan"/>; khong thi chieu co
///     ban cua he. Phep bay thang (Qua cau lua / bang, Lua dia nguc) can THAY THANG muc tieu.
///   - NGAM: dich + van toc x thoi gian bay x muc don dau, cong sai so ngau nhien theo do kho (<see cref="SaiSoNgam"/>).
///   - BINH: mau < <see cref="NguongUongMau"/> thi uong binh mau; nang luong khong du chieu co ban thi uong binh mana. Can
///     binh ma co binh roi trong 14 m (va khong ke thu nao sat) thi di nhat - chu phong giao binh cho BOT
///     (QuanLyBinhRoi.XetBotNhat).
///   - LUI: Thuong / Kho bi ap sat duoi 4 m (khong dang niem) thi lui ra (o lui phai di duoc).
///   - NHAO LON (10/10/2026, nguoi dung): bi ap sat duoi 4 m thi moi giay xet MOT lan, voi xac suat <see cref="TiLeNhaoLon"/> (De it, Kho
///     nhieu) lan ra xa 5 m theo huong lui / cheo / ngang - o 2,5 m va 5 m phai di duoc (khong lan xuong vuc).
/// </summary>
[DefaultExecutionOrder(-40)]
public class BotDieuKhien : MonoBehaviour
{
    /// <summary>Ghe cua BOT trong tran.</summary>
    public byte ghe;
    /// <summary>MayBot.De / Thuong / Kho - doc tu uid.</summary>
    public int doKho = MayBot.Thuong;
    /// <summary>He chinh (MayBot.HeLua / HeBang / HeSet / HePhong) - boc ngau nhien luc sinh, chi may chu phong biet.</summary>
    public int he;
    public string uid;

    // ---- Hang so di lai ----
    /// <summary>Doi thu (nguoi choi / BOT khac) trong ban kinh nay thi giao tranh ngay du cap nao; tu cap 7 quai trong ban
    /// kinh nay cung danh truoc khi di san nguoi (nguoi dung chon 20 m, 09/10/2026).</summary>
    public const float TamGiaoTranh = 20f;
    /// <summary>Tu cap nhan vat nay BOT uu tien di tim doi thu (duoi cap nay uu tien giet quai) - nguoi dung 09/10/2026.</summary>
    public const int CapSanNguoi = 7;
    /// <summary>Bi nguoi choi / BOT danh trung trong ngan nay giay thi danh tra ke ay du o xa (nguoi dung chon).</summary>
    public const float GiayDanhTra = 5f;
    /// <summary>Toi gan muc tieu chung nay thi dung (tam phep tam trung binh 12-18 m - buoc 4 tung phep tu day).</summary>
    public const float TamGiu = 9f;
    const float NhipTimLai = 1.2f;
    const float NhipKiemKet = 0.8f;
    const float NguongKet = 0.35f;

    /// <summary>Bao lau chon lai muc tieu mot lan (giay) - phan xa theo do kho.</summary>
    public static float NhipPhanXa(int doKho) { return doKho == MayBot.De ? 1.2f : doKho == MayBot.Kho ? 0.35f : 0.7f; }

    // ---- Cho phep thu (menu 114) ----
    /// <summary>Tat = lai THANG toi dich (doi chung: khong tim duong).</summary>
    [System.NonSerialized] public bool dungTimDuong = true;
    /// <summary>Tat = khong do vuc phia truoc (doi chung).</summary>
    [System.NonSerialized] public bool dungChongVuc = true;
    /// <summary>Ep di toi mot diem (bo qua muc tieu) - phep thu.</summary>
    [System.NonSerialized] public bool coDiemEp;
    [System.NonSerialized] public Vector3 diemEp;
    /// <summary>Dung hoan toan (phep thu buoc 2 / ket tran).</summary>
    [System.NonSerialized] public bool dungYen;

    // ---- So dem chan doan ----
    [System.NonSerialized] public int soLanKet, soLanDungVuc, soLanVong;
    [System.NonSerialized] public float quangDuong;
    public Damageable MucTieu { get; private set; }
    public IReadOnlyList<Vector3> Duong { get { return duong; } }

    PlayerController pc;
    Damageable mau;
    float lucChonLai, lucTimLai, lucKiemKet = -1f, lucDoiTuan;
    readonly List<Vector3> duong = new List<Vector3>();
    int chiSoMoc;
    Vector3 dichCu, mocKiemKet, viTriTruoc, diemTuan;
    bool coDiemTuan, dangGiu;
    int ketLienTiep;
    float vongDen; Vector3 diemVong;
    float thoiGianMuonDi;

    // ================================================================
    //  BUOC 4 - DANH
    // ================================================================

    /// <summary>Giay toi thieu giua hai lan ra chieu (cong voi hoi chieu tung ky nang).</summary>
    public static float NhipRaChieu(int doKho) { return doKho == MayBot.De ? 1.0f : doKho == MayBot.Kho ? 0.15f : 0.45f; }
    /// <summary>Xac suat dung chieu LIEN HOAN cua he moi lan ra chieu (con lai: chieu co ban).</summary>
    public static float TiLeLienHoan(int doKho) { return doKho == MayBot.De ? 0.3f : doKho == MayBot.Kho ? 1f : 0.75f; }
    /// <summary>Ban kinh sai so ngam (m) - diem ngam lech ngau nhien trong vong nay.</summary>
    public static float SaiSoNgam(int doKho) { return doKho == MayBot.De ? 2.5f : doKho == MayBot.Kho ? 0.3f : 1.0f; }
    /// <summary>Don dau: nhan van toc muc tieu x thoi gian bay voi he so nay (0 = ngam cho dang dung).</summary>
    public static float HeSoDonDau(int doKho) { return doKho == MayBot.De ? 0f : doKho == MayBot.Kho ? 1f : 0.5f; }
    /// <summary>Mau duoi ti le nay thi uong binh mau.</summary>
    public static float NguongUongMau(int doKho) { return doKho == MayBot.De ? 0.25f : doKho == MayBot.Kho ? 0.45f : 0.35f; }
    public const float TamNhatBinh = 14f;
    /// <summary>Van toc muc tieu lon hon chung nay (m/s) la dich chuyen tuc thoi, khong phai chay.</summary>
    public const float TocToiDaHopLe = 15f;
    public const float GanLui = 4f;

    /// <summary>Tat = khong danh, khong uong binh, khong cong diem (phep thu buoc 3).</summary>
    [System.NonSerialized] public bool dungDanh = true;

    // So dem chan doan
    [System.NonSerialized] public int soPhepDaTung, soBinhMauDaUong, soBinhManaDaUong, soLanLui, soBinhDaNhatXin, soLanNhaoLon;

    /// <summary>Xac suat BOT nhao lon tranh moi lan xet (moi giay mot lan khi bi ap sat).</summary>
    public static float TiLeNhaoLon(int doKho) { return doKho == MayBot.De ? 0.2f : doKho == MayBot.Kho ? 0.85f : 0.5f; }
    float lucXetNhaoLon;
    [System.NonSerialized] public readonly int[] demTheoKy = new int[CapDo.SoKyNang];
    [System.NonSerialized] public float tongSaiSoNgam; [System.NonSerialized] public int soLanDoNgam;

    float lucDanhSau, lucChiDiem, lucXetThay;
    bool thayMucTieu = true;
    Vector3 viTriMucTieuTruoc, vanTocMucTieu;
    Damageable mucTieuTruoc;
    BinhRoi binhDangNham;

    /// <summary>Phep bay thang can thay thang muc tieu (bia / cay chan thi bay vao do).</summary>
    static bool CanThayThang(int ky) { return ky == 0 || ky == CapDo.KyQuaCauBang || ky == CapDo.KyLuaDiaNguc; }

    bool DuocDung(int ky)
    {
        return pc.Cap.DaMo(ky) && pc.HoiChieuGiay(ky) <= 0f && pc.mana >= pc.NangLuongCan(ky);
    }

    bool TrongTam(int ky, float kc) { return kc <= pc.TamNgam(ky) * 0.95f; }

    void CapNhatMucTieu(float dt)
    {
        if (MucTieu == null) { mucTieuTruoc = null; return; }
        Vector3 p = MucTieu.transform.position;
        if (MucTieu != mucTieuTruoc) { mucTieuTruoc = MucTieu; viTriMucTieuTruoc = p; vanTocMucTieu = Vector3.zero; }
        if (dt > 0.0001f)
        {
            Vector3 v = (p - viTriMucTieuTruoc) / dt; v.y = 0f;
            // Nhanh hon moi kieu chay (~10 m/s o cap 20) la DICH CHUYEN TUC THOI (Toc bien, bi dat cho) - tinh lai tu dau.
            // Khong loc thi van toc vot hang tram m/s va BOT don dau lech hang chuc met (menu 115 tung do ra TB 28 m).
            if (v.magnitude > TocToiDaHopLe) vanTocMucTieu = Vector3.zero;
            else vanTocMucTieu = Vector3.Lerp(vanTocMucTieu, v, 0.2f);
        }
        viTriMucTieuTruoc = p;

        if (Time.time >= lucXetThay)
        {
            lucXetThay = Time.time + 0.3f;
            thayMucTieu = !Physics.Linecast(transform.position + Vector3.up * 1.2f, p + Vector3.up * 1.0f,
                                            BanDoBot.MatNaVatCan, QueryTriggerInteraction.Ignore);
        }
    }

    /// <summary>So ke thu (quai + doi thu) trong ban kinh r quanh diem c.</summary>
    int SoKeThuQuanh(Vector3 c, float r)
    {
        var dir = GameDirector.Instance;
        if (dir == null) return 0;
        int n = 0; float r2 = r * r;
        var quai = dir.QuaiConSong;
        for (int i = 0; i < quai.Count; i++) if (HopLe(quai[i]) && (quai[i].transform.position - c).sqrMagnitude <= r2) n++;
        foreach (var t in dir.moiNguoi)
        {
            if (t == null || t == transform) continue;
            var d = t.GetComponent<Damageable>();
            if (!HopLe(d) || CheDoTran.LaDongDoi(mau, d)) continue;
            if ((t.position - c).sqrMagnitude <= r2) n++;
        }
        return n;
    }

    /// <summary>Ke thu gan nhat (quai / doi thu) - de lui va dung Khien.</summary>
    Damageable KeThuGanNhat(out float kc)
    {
        kc = float.MaxValue; Damageable gan = null;
        var dir = GameDirector.Instance;
        if (dir == null) return null;
        Vector3 p = transform.position;
        var quai = dir.QuaiConSong;
        for (int i = 0; i < quai.Count; i++)
        {
            if (!HopLe(quai[i])) continue;
            float d = KhoangNgang(p, quai[i].transform.position);
            if (d < kc) { kc = d; gan = quai[i]; }
        }
        foreach (var t in dir.moiNguoi)
        {
            if (t == null || t == transform) continue;
            var dd = t.GetComponent<Damageable>();
            if (!HopLe(dd) || CheDoTran.LaDongDoi(mau, dd)) continue;
            float d = KhoangNgang(p, t.position);
            if (d < kc) { kc = d; gan = dd; }
        }
        return gan;
    }

    void ChiDiemNeuCo()
    {
        if (Time.time < lucChiDiem) return;
        lucChiDiem = Time.time + 0.5f;
        if (pc.Cap.DiemKyNang > 0) MayBot.TieuDiem(pc.Cap, he);
    }

    void UongBinhNeuCan()
    {
        if (mau == null || mau.maxHealth <= 0f) return;
        if (mau.health / mau.maxHealth < NguongUongMau(doKho) && pc.Cap.SoBinh(CapDo.KyBinhMau) > 0 && pc.HoiChieuGiay(CapDo.KyBinhMau) <= 0f)
        {
            int truoc = pc.Cap.SoBinh(CapDo.KyBinhMau);
            pc.CastAt(CapDo.KyBinhMau, transform.position);
            if (pc.Cap.SoBinh(CapDo.KyBinhMau) < truoc) soBinhMauDaUong++;
        }
        int coBan = MayBot.KyCoBan(he);
        float can = pc.Cap.DaMo(coBan) ? pc.NangLuongCan(coBan) : 15f;
        if (pc.mana < can + 2f && pc.Cap.SoBinh(CapDo.KyBinhMana) > 0 && pc.HoiChieuGiay(CapDo.KyBinhMana) <= 0f)
        {
            int truoc = pc.Cap.SoBinh(CapDo.KyBinhMana);
            pc.CastAt(CapDo.KyBinhMana, transform.position);
            if (pc.Cap.SoBinh(CapDo.KyBinhMana) < truoc) soBinhManaDaUong++;
        }
    }

    /// <summary>Chon ky nang cho lan ra chieu nay. -1 = khong co gi tung duoc.</summary>
    int ChonKyNang(float kc)
    {
        float keGan;
        KeThuGanNhat(out keGan);
        float mau01 = mau != null && mau.maxHealth > 0f ? mau.health / mau.maxHealth : 1f;

        // Khien: mau duoi 60% va bi ap sat
        if (mau01 < 0.6f && keGan < 8f && DuocDung(5)) return 5;

        bool lienHoan = Random.value < TiLeLienHoan(doKho);
        var mt = MucTieu;
        if (lienHoan)
        {
            switch (he)
            {
                case MayBot.HeLua:
                    if (DuocDung(4) && TrongTam(4, kc) && mt.GetComponent<BiDanhNga>() == null) return 4;
                    if (DuocDung(CapDo.KyLuaDiaNguc) && thayMucTieu && kc <= pc.TamNgam(CapDo.KyLuaDiaNguc)
                        && (mt.isPlayer || SoKeThuQuanh(transform.position, LuaDiaNguc.TamTim) >= 2)) return CapDo.KyLuaDiaNguc;
                    break;
                case MayBot.HeBang:
                    if (DuocDung(CapDo.KyTangHinh) && mau01 < 0.4f && !TangHinh.Dang(this)) return CapDo.KyTangHinh;
                    if (DuocDung(1) && TrongTam(1, kc) && mt.GetComponent<FrozenEffect>() == null) return 1;
                    break;
                case MayBot.HeSet:
                    if (DuocDung(CapDo.KyCauDien) && TrongTam(CapDo.KyCauDien, kc)
                        && (mt.isPlayer || SoKeThuQuanh(mt.transform.position, 9f) >= 2)) return CapDo.KyCauDien;
                    if (DuocDung(2) && TrongTam(2, kc)) return 2;
                    break;
                default:
                    if (DuocDung(CapDo.KyMayGiong) && TrongTam(CapDo.KyMayGiong, kc)) return CapDo.KyMayGiong;
                    if (!CapDo.KyAn(CapDo.KyHoaLocXoay) && DuocDung(CapDo.KyHoaLocXoay) && HoaLocXoay.CoLocDeHoa(mau)) return CapDo.KyHoaLocXoay;
                    if (DuocDung(3) && TrongTam(3, kc)) return 3;
                    break;
            }
        }
        int coBan = MayBot.KyCoBan(he);
        if (DuocDung(coBan) && TrongTam(coBan, kc) && (!CanThayThang(coBan) || thayMucTieu)) return coBan;
        return -1;
    }

    /// <summary>Thoi gian tu luc tung toi luc phep cham muc tieu (de don dau).</summary>
    float ThoiGianToi(int ky, float kc)
    {
        switch (ky)
        {
            case 0: case CapDo.KyQuaCauBang: case CapDo.KyGioLoc: return kc / 15f + 0.2f;
            case 4: return 1.0f;
            case 1: return 0.8f;
            case 2: case 6: case CapDo.KyMayGiong: return 0.4f;
            default: return 0.3f;
        }
    }

    Vector3 NgamToi(int ky, float kc)
    {
        if (ky == 5 || ky == CapDo.KyTangHinh) return transform.position;
        Vector3 dich = MucTieu.transform.position + vanTocMucTieu * (ThoiGianToi(ky, kc) * HeSoDonDau(doKho));
        Vector2 lech = Random.insideUnitCircle * SaiSoNgam(doKho);
        return dich + new Vector3(lech.x, 0f, lech.y);
    }

    void DanhNeuDuoc()
    {
        if (MucTieu == null || MucTieu.IsDead || Time.time < lucDanhSau) return;
        if (pc.DangNiemChu || pc.DangBiKhoaCung) return;
        float kc = KhoangNgang(transform.position, MucTieu.transform.position);
        int ky = ChonKyNang(kc);
        if (ky < 0) { lucDanhSau = Time.time + 0.2f; return; }

        Vector3 ngam = NgamToi(ky, kc);
        float manaTruoc = pc.mana;
        pc.CastAt(ky, ngam);
        bool daTung = pc.DangNiemChu || pc.mana < manaTruoc - 0.01f || pc.HoiChieuGiay(ky) > 0.01f;
        if (daTung)
        {
            soPhepDaTung++;
            demTheoKy[ky]++;
            if (ky != 5 && ky != CapDo.KyTangHinh)
            {
                tongSaiSoNgam += KhoangNgang(ngam, MucTieu.transform.position);
                soLanDoNgam++;
            }
            lucDanhSau = Time.time + NhipRaChieu(doKho);
        }
        else lucDanhSau = Time.time + 0.25f;
    }

    /// <summary>Binh roi gan nhat chua co chu trong TamNhatBinh m (null neu khong co).</summary>
    BinhRoi BinhGanNhat()
    {
        BinhRoi gan = null; float kc = TamNhatBinh;
        foreach (var b in QuanLyBinhRoi.TatCa)
        {
            if (b == null || b.DaCoChu) continue;
            float d = KhoangNgang(transform.position, b.transform.position);
            if (d < kc && BanDoBot.DiDuoc(b.transform.position)) { kc = d; gan = b; }
        }
        return gan;
    }

    bool CanBinh()
    {
        float mau01 = mau != null && mau.maxHealth > 0f ? mau.health / mau.maxHealth : 1f;
        return pc.Cap.SoBinh(CapDo.KyBinhMau) < 3 || pc.Cap.SoBinh(CapDo.KyBinhMana) < 2 || mau01 < 0.7f;
    }

    void Awake()
    {
        pc = GetComponent<PlayerController>();
        mau = GetComponent<Damageable>();
        viTriTruoc = transform.position;
        // Moi BOT chon lai lech pha nhau - khong ca dan cung nghi mot khung
        lucChonLai = Time.time + Random.Range(0f, NhipPhanXa(doKho));
    }

    void Update()
    {
        if (pc == null) return;
        float dt = Time.deltaTime;
        var g = GoiInput.Rong(dt);

        Vector3 p = transform.position;
        quangDuong += Vector2.Distance(new Vector2(p.x, p.z), new Vector2(viTriTruoc.x, viTriTruoc.z));
        viTriTruoc = p;

        if (dungYen || KetTran.DaXong || (mau != null && mau.IsDead) || !pc.enabled || !BanDoBot.HopLeChoCanh)
        { pc.input = g; return; }

        if (Time.time >= lucChonLai)
        {
            MucTieu = ChonMucTieu();
            lucChonLai = Time.time + NhipPhanXa(doKho);
        }
        CapNhatMucTieu(dt);
        DanhDauOQua();
        if (dungDanh) { ChiDiemNeuCo(); UongBinhNeuCan(); }

        Vector3 huong = Vector3.zero;
        Vector3 dich;
        float dungKhi;
        if (TinhDich(out dich, out dungKhi)) huong = LaiToi(dich, dungKhi);

        if (dungChongVuc && huong.sqrMagnitude > 0.01f && PhiaTruocLaVuc(huong))
        {
            soLanDungVuc++;
            BanDoBot.CamTam(p + huong * 1.7f, 1.2f, 20f);
            lucTimLai = 0f;                       // tim duong khac ngay khung sau
            huong = Vector3.zero;
        }

        // NHAO LON tranh khi bi ap sat (moi do kho, 10/10/2026)
        if (dungDanh && !coDiemEp && !pc.DangNiemChu) NhaoLonNeuApSat(p);

        // LUI khi bi ap sat (Thuong / Kho): o lui phai di duoc - khong lui xuong vuc
        if (dungDanh && doKho >= MayBot.Thuong && !coDiemEp && !pc.DangNiemChu)
        {
            float keGan;
            var ke = KeThuGanNhat(out keGan);
            if (ke != null && keGan < GanLui)
            {
                Vector3 lui = HuongNgang(ke.transform.position, transform.position);
                Vector3 benCanh = new Vector3(-lui.z, 0f, lui.x);
                Vector3[] thu = { lui, (lui + benCanh).normalized, (lui - benCanh).normalized, benCanh, -benCanh };
                foreach (var h in thu)
                    if (BanDoBot.DiDuoc(p + h * 1.5f) && BanDoBot.DiDuoc(p + h * 3f)) { huong = h; soLanLui++; break; }
            }
        }

        KiemKet(huong, dt);

        g.huongDi = huong;
        pc.input = g;

        if (dungDanh) DanhNeuDuoc();
    }

    // ================================================================
    //  MUC TIEU
    // ================================================================

    Damageable ChonMucTieu()
    {
        var dir = GameDirector.Instance;
        if (dir == null) return null;
        Vector3 p = transform.position;
        float r2 = TamGiaoTranh * TamGiaoTranh;

        Damageable nguoiGan = null; float dNguoiGan = r2;
        Damageable nguoiXa = null; float dNguoiXa = float.MaxValue;
        Damageable quaiGan = null; float dQuaiGan = r2;
        Damageable quaiXa = null; float dQuaiXa = float.MaxValue;

        var quai = dir.QuaiConSong;
        for (int i = 0; i < quai.Count; i++)
        {
            var q = quai[i];
            if (!HopLe(q) || !TamNhin.NhinThay(mau, q.transform.position)) continue;    // chi thay trong 25 m (10/10/2026)
            float d = (q.transform.position - p).sqrMagnitude;
            if (d < dQuaiGan) { dQuaiGan = d; quaiGan = q; }
            if (d < dQuaiXa) { dQuaiXa = d; quaiXa = q; }
        }
        foreach (var t in dir.moiNguoi)
        {
            if (t == null || t == transform) continue;
            var d = t.GetComponent<Damageable>();
            if (!LaDoiThu(d) || !TamNhin.NhinThay(mau, t.position)) continue;
            float kc = (t.position - p).sqrMagnitude;
            if (kc < dNguoiGan) { dNguoiGan = kc; nguoiGan = d; }
            if (kc < dNguoiXa) { dNguoiXa = kc; nguoiXa = d; }
        }

        // Doi thu o gan: giao tranh ngay, cap nao cung vay
        if (nguoiGan != null) return nguoiGan;

        // Ke vua danh minh (du xa)
        Damageable keDanh = null;
        if (mau != null && Time.time - mau.lucNguoiChoiDanh <= GiayDanhTra && LaDoiThu(mau.nguoiChoiDanhCuoi)
            && TamNhin.NhinThay(mau, mau.nguoiChoiDanhCuoi.transform.position))
            keDanh = mau.nguoiChoiDanhCuoi;

        if (pc.Cap.Cap >= CapSanNguoi)
        {
            if (quaiGan != null) return quaiGan;          // quai ap sat: giet truoc roi di tiep
            if (keDanh != null) return keDanh;
            return nguoiXa != null ? nguoiXa : quaiXa;
        }
        // Duoi cap 7: uu tien giet quai de len cap; het quai -> null = di tuan
        if (keDanh != null) return keDanh;
        return quaiXa;
    }

    /// <summary>Nguoi choi / BOT khac co the nham: con song, khong duoi vuc, khong cung doi, khong dang Tang hinh.</summary>
    bool LaDoiThu(Damageable d)
    {
        return HopLe(d) && d != mau && !CheDoTran.LaDongDoi(mau, d) && !TangHinh.Dang(d.transform);
    }

    static bool HopLe(Damageable d)
    {
        return d != null && !d.IsDead && d.transform.position.y > BanDoBot.DatVucDuoi;
    }

    /// <summary>Diem can toi va khoang cach dung lai. False = dung yen.</summary>
    bool TinhDich(out Vector3 dich, out float dungKhi)
    {
        dich = transform.position; dungKhi = 0.7f;
        if (coDiemEp) { dich = diemEp; return true; }
        if (Time.time < vongDen) { dich = diemVong; return true; }

        // Can binh va co binh roi gan, khong ke thu nao sat -> di nhat
        if (dungDanh && CanBinh())
        {
            if (binhDangNham == null || binhDangNham.DaCoChu) binhDangNham = BinhGanNhat();
            float keGan;
            KeThuGanNhat(out keGan);
            if (binhDangNham != null && keGan > 6f)
            {
                dich = binhDangNham.transform.position; dungKhi = 0.4f;
                return true;
            }
        }

        if (MucTieu != null && !MucTieu.IsDead)
        {
            coDiemTuan = false;
            float kc = KhoangNgang(transform.position, MucTieu.transform.position);
            // Bi bia / cay che mat ma chieu co ban la phep bay thang: KHONG dung giu, tien len cho toi khi thay
            if (dungDanh && !thayMucTieu && CanThayThang(MayBot.KyCoBan(he)) && kc > 3f)
            {
                dangGiu = false;
                dich = MucTieu.transform.position; dungKhi = 3f;
                return true;
            }
            // Tre: vao gan TamGiu x 0,8 moi dung, ra xa qua TamGiu moi di lai - khong giat cuc buoc mot
            if (dangGiu && kc <= TamGiu) return false;
            dangGiu = kc <= TamGiu * 0.8f;
            if (dangGiu) return false;
            dich = MucTieu.transform.position; dungKhi = TamGiu * 0.8f;
            return true;
        }

        // Khong thay ai trong tam nhin: DI TUAN KHAP BAN DO (10/10/2026, nguoi dung chon) - toi o lau chua ghe nhat
        dangGiu = false;
        if (!coDiemTuan || Time.time >= lucDoiTuan || KhoangNgang(transform.position, diemTuan) < 2.5f)
        {
            coDiemTuan = ChonDiemTuan(out diemTuan);
            if (!coDiemTuan) coDiemTuan = BanDoBot.DiemNgauNhien(transform.position, 20f, out diemTuan);
            lucDoiTuan = Time.time + KhoangNgang(transform.position, diemTuan) / 3.5f + 8f;
        }
        if (!coDiemTuan) return false;
        dich = diemTuan;
        return true;
    }

    // ================================================================
    //  DI TUAN KHAP BAN DO (tam nhin 25 m - 10/10/2026)
    // ================================================================

    /// <summary>Luoi o di tuan: <see cref="SoOTuan"/> x <see cref="SoOTuan"/> o phu ban do (128 m quanh tam).</summary>
    public const int SoOTuan = 8;
    const float NuaBanDo = 64f;
    readonly float[] lucQuaO = new float[SoOTuan * SoOTuan];
    readonly bool[] oHong = new bool[SoOTuan * SoOTuan];
    readonly Vector3[] choO = new Vector3[SoOTuan * SoOTuan];
    readonly bool[] daTinhO = new bool[SoOTuan * SoOTuan];
    float lucDanhDau;
    /// <summary>So o khac nhau da ghe (phep thu menu 114 doc).</summary>
    public int SoODaGhe { get { int n = 0; for (int i = 0; i < lucQuaO.Length; i++) if (lucQuaO[i] > 0f) n++; return n; } }

    Vector3 TamO(int i)
    {
        var dir = GameDirector.Instance;
        Vector3 c = dir != null ? dir.arenaCenter : Vector3.zero;
        float co = NuaBanDo * 2f / SoOTuan;
        return c + new Vector3(-NuaBanDo + co * (i % SoOTuan + 0.5f), 0f, -NuaBanDo + co * (i / SoOTuan + 0.5f));
    }

    /// <summary>Danh dau cac o trong tam nhin la "vua ghe" (moi giay mot lan).</summary>
    void DanhDauOQua()
    {
        if (Time.time < lucDanhDau) return;
        lucDanhDau = Time.time + 1f;
        Vector3 p = transform.position;
        for (int i = 0; i < lucQuaO.Length; i++)
            if (KhoangNgang(TamO(i), p) < TamNhin.BanKinh * 0.8f) lucQuaO[i] = Time.time;
    }

    /// <summary>O lau chua ghe nhat (tru gan quanh minh, tru o khong co cho dung), gan hon thi duoc cong mot chut.</summary>
    bool ChonDiemTuan(out Vector3 ra)
    {
        ra = transform.position;
        Vector3 p = transform.position;
        float tot = float.MinValue; int chon = -1;
        for (int i = 0; i < lucQuaO.Length; i++)
        {
            if (oHong[i]) continue;
            if (!daTinhO[i])
            {
                daTinhO[i] = true;
                Vector3 c;
                if (BanDoBot.ODiDuocGanNhat(TamO(i), 7f, out c)) choO[i] = c; else { oHong[i] = true; continue; }
            }
            float kc = KhoangNgang(choO[i], p);
            if (kc < 18f) continue;
            float diem = (Time.time - lucQuaO[i]) - kc * 0.25f + Random.value * 6f;
            if (diem > tot) { tot = diem; chon = i; }
        }
        if (chon < 0) return false;
        ra = choO[chon];
        return true;
    }

    // ================================================================
    //  LAI THEO DUONG
    // ================================================================

    Vector3 LaiToi(Vector3 dich, float dungKhi)
    {
        Vector3 p = transform.position;
        if (KhoangNgang(p, dich) <= dungKhi) { duong.Clear(); return Vector3.zero; }

        if (!dungTimDuong) return HuongNgang(p, dich);

        if (duong.Count == 0 || Time.time >= lucTimLai || KhoangNgang(dich, dichCu) > 2.5f)
        {
            if (!BanDoBot.TimDuong(p, dich, duong)) duong.Clear();
            chiSoMoc = 0;
            dichCu = dich;
            lucTimLai = Time.time + NhipTimLai;
        }
        if (duong.Count == 0) return Vector3.zero;     // khong co duong: dung, khong lao bua (co the la vuc)

        while (chiSoMoc < duong.Count - 1 && KhoangNgang(p, duong[chiSoMoc]) < 0.7f) chiSoMoc++;
        return HuongNgang(p, duong[chiSoMoc]);
    }

    bool PhiaTruocLaVuc(Vector3 huong)
    {
        Vector3 p = transform.position;
        int matDat = BanDoBot.MatNaDat;
        for (int i = 0; i < 2; i++)
        {
            Vector3 q = p + huong.normalized * (i == 0 ? 0.9f : 1.7f);
            if (BanDoBot.LaVucThat(q)) return true;
            RaycastHit hit;
            if (!Physics.Raycast(new Vector3(q.x, p.y + 2.5f, q.z), Vector3.down, out hit, 8f, matDat, QueryTriggerInteraction.Ignore))
                return true;
            if (hit.point.y < p.y - 2.2f) return true;
        }
        return false;
    }

    void KiemKet(Vector3 huong, float dt)
    {
        bool muonDi = huong.sqrMagnitude > 0.01f;
        if (muonDi) thoiGianMuonDi += dt;
        if (lucKiemKet < 0f) { lucKiemKet = Time.time + NhipKiemKet; mocKiemKet = transform.position; thoiGianMuonDi = 0f; return; }
        if (Time.time < lucKiemKet) return;

        float daDi = KhoangNgang(transform.position, mocKiemKet);
        bool dangNiem = pc.DangNiem >= 0;
        if (thoiGianMuonDi > NhipKiemKet * 0.7f && daDi < NguongKet && !dangNiem && !pc.DangBiKhoaCung)
        {
            soLanKet++;
            ketLienTiep++;
            BanDoBot.CamTam(transform.position + huong.normalized * 0.9f, 0.5f, 6f);
            lucTimLai = 0f;
            if (ketLienTiep >= 3 && BanDoBot.DiemNgauNhien(transform.position, 6f, out diemVong))
            {
                soLanVong++;
                vongDen = Time.time + 1.5f;
                ketLienTiep = 0;
            }
        }
        else if (daDi >= NguongKet) ketLienTiep = 0;

        lucKiemKet = Time.time + NhipKiemKet;
        mocKiemKet = transform.position;
        thoiGianMuonDi = 0f;
    }

    static float KhoangNgang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    /// <summary>Bi ap sat duoi <see cref="GanLui"/>: moi giay xet mot lan, gieo <see cref="TiLeNhaoLon"/>, lan 5 m ra xa ke thu.</summary>
    void NhaoLonNeuApSat(Vector3 p)
    {
        if (Time.time < lucXetNhaoLon) return;
        if (NhaoLon.Dang(gameObject) || pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f || !pc.Cap.DaMo(CapDo.KyNhaoLon)) return;
        float keGan;
        var ke = KeThuGanNhat(out keGan);
        if (ke == null || keGan >= GanLui) return;
        lucXetNhaoLon = Time.time + 1f;
        if (Random.value > TiLeNhaoLon(doKho)) return;
        Vector3 lui = HuongNgang(ke.transform.position, p);
        Vector3 benCanh = new Vector3(-lui.z, 0f, lui.x);
        Vector3[] thu = { lui, (lui + benCanh).normalized, (lui - benCanh).normalized, benCanh, -benCanh };
        foreach (var h in thu)
            if (BanDoBot.DiDuoc(p + h * 2.5f) && BanDoBot.DiDuoc(p + h * NhaoLon.Tam))
            {
                pc.CastAt(CapDo.KyNhaoLon, p + h * NhaoLon.Tam);
                if (NhaoLon.Dang(gameObject) || pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) soLanNhaoLon++;
                return;
            }
    }

    static Vector3 HuongNgang(Vector3 tu, Vector3 den)
    {
        Vector3 d = den - tu; d.y = 0f;
        return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.zero;
    }
}
