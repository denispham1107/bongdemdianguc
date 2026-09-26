using UnityEngine;

/// <summary>
/// CUA SO "SACH PHEP" - keo tha de sap xep cac o ky nang.
///
/// Bo cuc theo dung ban ve nguoi dung gui (cuasokynang.png):
///
///   +---------------------------------------------+
///   |                 SACH PHEP                   |
///   +------------------+--------------------------+
///   | [icon] Ten       |  Loi ke chi tiet cua ky  |
///   | [icon] Ten       |  nang dang chon (cuon)   |
///   |  ... (cuon)      +--------------------------+
///   |                  |  Cac O da chon - keo tha |
///   |                  |  tu cot trai vao day     |
///   +------------------+--------------------------+
///
/// HAI KIEU O, theo dung ban giao dien dang chay:
///   - Cam ung: BAY O TRON, xep y het cum nut ngoai man hinh game (dung chung
///     ham <see cref="GameHUD.LechNut"/>) - sap trong day the nao thi ra ngoai
///     tran dau the ay, khong phai doan.
///   - May tinh: MOT HANG O VUONG nam ngang, giong thanh ky nang giua day man
///     hinh.
///
/// VE va XU LY TACH LAM HAI: <see cref="CapNhat"/> goi tu Update (mot lan moi
/// khung hinh, doc ngon tay / chuot), <see cref="Ve"/> goi tu OnGUI (OnGUI chay
/// nhieu lan moi khung hinh - xu ly keo tha trong do thi mot cu tha bi tinh
/// vai lan).
/// </summary>
public static class CuaSoSachPhep
{
    public static bool DangMo { get; private set; }

    /// <summary>Ky nang dang xem chi tiet ben phai.</summary>
    static int dangXem;

    /// <summary>O vua cham vao. Chi dung khi o ay TRONG (khong co ky nang de
    /// ma chon theo) - o co ky nang thi sang theo dangXem.</summary>
    static int oDangChon = -1;

    public static int DangXem { get { return dangXem; } }

    /// <summary>Vi tri cuon cot trai / phan than chi tiet (diem anh). Dat duoc de phep thu cuon; bi kep lai o lan cap nhat / ve sau.</summary>
    public static float CuonKho { get { return cuonKho; } set { cuonKho = value; } }
    public static float CuonThan { get { return cuonChiTiet; } set { cuonChiTiet = value; } }
    /// <summary>Chieu cao noi dung phan than chi tiet o lan ve gan nhat.</summary>
    public static float CaoNoiDungThan { get { return caoNoiDungThan; } }

    /// <summary>Chon mot ky nang de xem (nhu cham vao hang cua no o cot trai).</summary>
    public static void ChonKyNang(int ky)
    {
        if (ky < 0 || ky >= SachPhep.SoKyNang) return;
        dangXem = ky; cuonChiTiet = 0f; oDangChon = -1;
    }

    /// <summary>Chon mot o (nhu cham vao o ay trong vung o).</summary>
    public static void ChonO(int o)
    {
        if (o < 0 || o >= SachPhep.SoODangDung) return;
        oDangChon = o;
        int ky = SachPhep.BoDangDung[o];
        if (ky != SachPhep.Trong) { dangXem = ky; cuonChiTiet = 0f; }
    }

    static float cuonKho, cuonChiTiet;

    // ---- Keo tha ----
    static int keoTuKho = -1;     // so hieu ky nang dang keo tu cot trai
    static int keoTuO = -1;       // o dang keo di
    static bool dangKeo;
    static bool dangCuon;
    static Vector2 diemBatDau;
    static Vector2 troHienGio;
    static bool nhanTruoc;

    /// <summary>He so co chu COT TRAI (danh sach ky nang): nguoi dung xin to them 15% (13/09/2026).</summary>
    public const float HeSoChuKho = 1.15f;
    /// <summary>He so co chu PHAN THAN chi tiet (dong cap + loi ke): to them 20%.</summary>
    public const float HeSoChuThan = 1.20f;

    /// <summary>Chieu cao noi dung phan than chi tiet o lan ve gan nhat - de kep cuon.</summary>
    static float caoNoiDungThan;
    static bool keoThan;          // ngon tay dat xuong trong phan than chi tiet -> vuot la cuon

    /// <summary>Nguong (diem anh) phai di qua thi moi tinh la keo, khong phai cham.</summary>
    const float NguongKeo = 12f;

    public static void Mo() { DangMo = true; xemTruoc = false; HuyKeo(); }
    public static void Dong() { DangMo = false; xemTruoc = false; HuyKeo(); }

    // ---- CHE DO XEM TRUOC (mo tu SANH, nut KY NANG) ----
    // Nguoi dung 14/09/2026: truoc khi vao tran phai doc duoc TAT CA ky nang va keo
    // tha san vao o, vao tran khong phai chinh lai. O sanh chua co cap do nen:
    //   - moi ky nang hien du mau, KHONG o khoa, keo tha duoc het;
    //   - khong co nut mo khoa / nang cap (chi co ghi chu cach mo trong tran);
    //   - khong hien so binh dang co.
    // Thu tu o van ghi vao cung cho voi trong tran (SachPhep) nen giu qua moi lan mo game.
    static bool xemTruoc;

    /// <summary>Dang mo o che do xem truoc (tu sanh).</summary>
    public static bool XemTruoc { get { return DangMo && xemTruoc; } }

    public static void MoXemTruoc()
    {
        DangMo = true; xemTruoc = true; HuyKeo();
        cuonKho = 0f; cuonChiTiet = 0f; oDangChon = -1;
    }

    /// <summary>Ky nang hien nhu DA MO (khong o khoa, keo tha duoc): that su da mo, hoac
    /// dang xem truoc o sanh. Public cho menu 66 hoi.</summary>
    public static bool HienDaMo(int ky) { return xemTruoc || CapDo.DaMo(ky); }

    /// <summary>
    /// Ky nang o cot trai keo vao o duoc khong. KE CA KY NANG CON KHOA (nguoi dung 26/09/2026 - truoc do bi chan): xep san
    /// vao o, o hien toi + o khoa, cham khong tung (GameHUD.NutTaiDiem / CastAt bo qua), co diem thi dau "+" tren o mo khoa
    /// ngay ngoai tran. Chi KY NANG BI DONG khong keo duoc (nguoi dung chot 19/09/2026): no tu chay, khong chiem o.
    /// </summary>
    public static bool KeoDuocTuKho(int ky) { return ky >= 0 && ky < CapDo.SoKyNang && !CapDo.LaKyBiDong(ky); }
    public static void DaoTrangThai() { if (DangMo) Dong(); else Mo(); }

    static void HuyKeo() { keoTuKho = -1; keoTuO = -1; dangKeo = false; dangCuon = false; }

    // ================================================================
    //  BO CUC - ham THUAN, do duoc ma khong can vao Play
    // ================================================================

    public struct BoCuc
    {
        public Rect khung;        // ca cua so
        public Rect tieuDe;
        public Rect nutDong;
        public Rect kho;          // cot trai
        public Rect chiTiet;      // phai tren
        public Rect vungO;        // phai duoi
        public float caoHang;     // mot hang trong kho
        public Rect nutHoc;       // nut mo khoa / nang cap
        public Rect thanChiTiet;  // phan CUON duoc cua chi tiet: dong cap + loi ke (duoi dau muc co dinh)
    }

    /// <summary>Chieu cao dau muc chi tiet (hinh + ten + thong so + vach) - dung yen, khong cuon.</summary>
    public static float CaoDauChiTiet(float s) { return 14f * s + 56f * s + 14f * s + 10f * s; }

    public static BoCuc TinhBoCuc(float W, float H, float s)
    {
        var b = new BoCuc();

        // Chua het man hinh nhung de lai vien: nguoi choi phai thay minh van
        // dang o trong tran dau chu khong phai da roi ra mot man khac.
        //
        // 1640 x 836 (truoc 1180 x 720): nguoi dung ve khung xanh tren anh chup
        // 1560x572 (13/09/2026) xin cua so to bang the de hien nhieu thong tin hon
        // - khung ay do duoc ~865 x 442 diem = 1640s x 836s voi s = 572/1080.
        float rong = Mathf.Min(W * 0.94f, 1640f * s);
        float cao = Mathf.Min(H * 0.90f, 836f * s);
        b.khung = new Rect((W - rong) * 0.5f, (H - cao) * 0.5f, rong, cao);

        float le = 16f * s;
        b.tieuDe = new Rect(b.khung.x, b.khung.y + 10f * s, b.khung.width, 44f * s);
        float kt = 34f * s;
        b.nutDong = new Rect(b.khung.xMax - kt - le, b.khung.y + 12f * s, kt, kt);

        // +24 chu khong +8: duoi tieu de con mot dong phu de (cap nhan vat,
        // diem ky nang chua dung), khong chua cho thi no de len cot trai.
        float tren = b.tieuDe.yMax + 24f * s;
        float duoi = b.khung.yMax - le;
        float rongKho = Mathf.Round(b.khung.width * 0.36f);

        b.kho = new Rect(b.khung.x + le, tren, rongKho - le * 1.5f, duoi - tren);

        float xPhai = b.kho.xMax + le;
        float rongPhai = b.khung.xMax - le - xPhai;

        // Vung O chiem phan duoi. Ban cam ung can cho hon vi cum nut tron xoe
        // rong; ban may tinh chi can mot hang o vuong.
        float caoO = CamUng.DangDung ? (duoi - tren) * 0.50f : 150f * s;
        b.chiTiet = new Rect(xPhai, tren, rongPhai, duoi - tren - caoO - 10f * s);
        b.vungO = new Rect(xPhai, b.chiTiet.yMax + 10f * s, rongPhai, caoO);

        b.caoHang = 64f * HeSoChuKho * s;       // chu to them 15% thi hang cao theo

        // Dai nut MO KHOA / NANG CAP nam sat day khung chi tiet
        b.nutHoc = new Rect(b.chiTiet.x + 14f * s, b.chiTiet.yMax - 50f * s,
                            b.chiTiet.width - 28f * s, 40f * s);

        float yThan = b.chiTiet.y + CaoDauChiTiet(s);
        b.thanChiTiet = new Rect(b.chiTiet.x, yThan, b.chiTiet.width, Mathf.Max(20f * s, b.nutHoc.y - 8f * s - yThan));
        return b;
    }

    /// <summary>Khung cua o thu <paramref name="i"/> trong vung o.</summary>
    public static Rect OTaiVung(Rect vung, int i, float s)
    {
        if (CamUng.DangDung)
        {
            // Dung DUNG hinh hoc cum nut ngoai tran dau, thu nho cho vua vung.
            //
            // Vi sao khong xep mot hang cho de: nguoi dung noi "trong nay sap
            // xep the nao thi ra ngoai man hinh game sap xep giong nhu vay".
            // Xep hang ngang trong day roi ngoai kia lai la hai cung thi o thu
            // tu trong bang khong biet ung voi nut nao.
            float rNut, chuanX, chuanY;
            GameHUD.HinhHocCumNut(out rNut, out chuanX, out chuanY);

            // CHUA DAI CHU NHAC o tren cung. Truoc day cum nut duoc can giua CA
            // vung o, nen o tren cung (loc xoay) chom len de len dong "Kéo kỹ
            // năng bên trái thả vào ô…" - nguoi dung chup anh bao 13/09/2026.
            float dinhChu = DongNhacO(vung, s).yMax + 6f * s;
            vung = Rect.MinMaxRect(vung.xMin, dinhChu, vung.xMax, vung.yMax);

            // Can giua theo HOP BAO THAT cua bay nut, khong theo hop tinh tu goc
            // man hinh: hop ay con ca le 101 diem giua cum va mep man hinh, nen
            // cum bi day len va de lai mot khoang trong lon ben duoi.
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            for (int j = 0; j < SachPhep.SoOTron; j++)
            {
                Vector2 l = GameHUD.LechNut(j);
                minX = Mathf.Min(minX, l.x - rNut); maxX = Mathf.Max(maxX, l.x + rNut);
                minY = Mathf.Min(minY, l.y - rNut); maxY = Mathf.Max(maxY, l.y + rNut);
            }
            float rongCum = maxX - minX, caoCum = maxY - minY;

            float le = 8f;
            float k = Mathf.Min((vung.width - le * 2f) / rongCum,
                                (vung.height - le * 2f) / caoCum);
            float r = rNut * k;

            Vector2 lech = GameHUD.LechNut(i);     // tu goc PHAI DUOI man hinh
            // Lech tang ve BEN TRAI va LEN TREN; dat tam hop bao vao tam vung
            float cx = vung.center.x - ((lech.x - (minX + maxX) * 0.5f) * k);
            float cy = vung.center.y - ((lech.y - (minY + maxY) * 0.5f) * k);
            return new Rect(cx - r, cy - r, r * 2f, r * 2f);
        }

        // May tinh: MOT HANG O VUONG nam ngang
        int n = SachPhep.SoOVuong;
        float hoNgang = 10f * s;
        float canh = Mathf.Min((vung.width - hoNgang * (n + 1)) / n, vung.height - 46f * s);
        float tong = canh * n + hoNgang * (n - 1);
        float x = vung.x + (vung.width - tong) * 0.5f;
        float y = vung.y + (vung.height - canh) * 0.5f + 8f * s;
        return new Rect(x + i * (canh + hoNgang), y, canh, canh);
    }

    // ================================================================
    //  DOC NGON TAY / CHUOT - goi tu Update
    // ================================================================

    /// <summary>Diem tro hien gio, he toa do GUI (y tu TREN xuong).</summary>
    static bool DocTro(out Vector2 diem, out bool dangNhan)
    {
        diem = Vector2.zero; dangNhan = false;

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            diem = new Vector2(t.position.x, Screen.height - t.position.y);
            dangNhan = t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
            return true;
        }

        var m = Input.mousePosition;
        diem = new Vector2(m.x, Screen.height - m.y);
        dangNhan = Input.GetMouseButton(0);
        return true;
    }

    public static void CapNhat(float s)
    {
        if (!DangMo) return;

        var b = TinhBoCuc(Screen.width, Screen.height, s);

        Vector2 tro; bool nhan;
        DocTro(out tro, out nhan);
        troHienGio = tro;

        bool vuaNhan = nhan && !nhanTruoc;
        bool vuaTha = !nhan && nhanTruoc;
        nhanTruoc = nhan;

        // Con lan chuot: cuon khung dang nam duoi con tro
        float lan = Input.mouseScrollDelta.y;
        if (Mathf.Abs(lan) > 0.01f)
        {
            if (b.kho.Contains(tro)) cuonKho -= lan * 42f * s;
            else if (b.thanChiTiet.Contains(tro)) cuonChiTiet -= lan * 42f * s;
        }

        if (vuaNhan)
        {
            diemBatDau = tro;
            dangKeo = false; dangCuon = false;
            keoTuKho = -1; keoTuO = -1;
            // Phan THAN chi tiet cuon bang ngon tay (vuot doc) - dien thoai khong co con lan
            keoThan = b.thanChiTiet.Contains(tro);

            if (b.nutDong.Contains(tro)) { Dong(); return; }

            // Nut MO KHOA / NANG CAP
            if (b.nutHoc.Contains(tro) && !xemTruoc)
            {
                if (CapDo.MoKhoaDuoc(dangXem)) CapDo.MoKhoa(dangXem);
                else if (CapDo.NangCapDuoc(dangXem)) CapDo.NangCap(dangXem);
                return;
            }

            int hang = HangTaiDiem(b, tro, s);
            if (hang >= 0)
            {
                ChonKyNang(hang);
                keoTuKho = KeoDuocTuKho(hang) ? hang : -1;
            }
            else
            {
                int o = OTaiDiem(b, tro, s);
                if (o >= 0)
                {
                    ChonO(o);
                    keoTuO = o;
                }
                // Cham ra ngoai han cua so thi dong lai - giong moi cua so khac
                else if (!b.khung.Contains(tro)) { Dong(); return; }
            }
        }

        if (nhan && keoThan)
        {
            if (!dangCuon && Mathf.Abs(tro.y - diemBatDau.y) > NguongKeo * s) dangCuon = true;
            if (dangCuon)
            {
                cuonChiTiet -= (tro.y - diemBatDau.y);
                diemBatDau = tro;
            }
        }

        if (nhan && !keoThan && (keoTuKho >= 0 || keoTuO >= 0 || b.kho.Contains(diemBatDau)))
        {
            Vector2 d = tro - diemBatDau;
            if (!dangKeo && !dangCuon && d.magnitude > NguongKeo * s)
            {
                // KEO DOC trong cot trai la CUON, keo ngang la mang ky nang di.
                // Khong tach hai y nay thi tren dien thoai khong con cach nao
                // cuon danh sach: moi lan vuot len la mot lan keo tha hut.
                if (Mathf.Abs(d.y) > Mathf.Abs(d.x) * 1.2f && b.kho.Contains(diemBatDau))
                { dangCuon = true; keoTuKho = -1; }
                else dangKeo = true;
            }

            if (dangCuon)
            {
                cuonKho -= (tro.y - diemBatDau.y);
                diemBatDau = tro;
            }
        }

        if (vuaTha)
        {
            if (dangKeo)
            {
                int o = OTaiDiem(b, tro, s);
                if (keoTuKho >= 0)
                {
                    if (o >= 0) { SachPhep.DatVaoO(o, keoTuKho); ChonO(o); }
                }
                else if (keoTuO >= 0)
                {
                    if (o >= 0 && o != keoTuO) SachPhep.DoiHaiO(keoTuO, o);
                    else if (o < 0 && !b.vungO.Contains(tro)) SachPhep.BoKhoiO(keoTuO);
                }
            }
            HuyKeo();
            keoThan = false;
        }

        KepCuon(b, s);
    }

    static void KepCuon(BoCuc b, float s)
    {
        float caoNoiDung = CaoDanhSach(b);
        float toiDa = Mathf.Max(0f, caoNoiDung - (b.kho.height - 12f * s));
        cuonKho = Mathf.Clamp(cuonKho, 0f, toiDa);
        cuonChiTiet = Mathf.Clamp(cuonChiTiet, 0f, Mathf.Max(0f, caoNoiDungThan - b.thanChiTiet.height + 8f * s));
    }

    /// <summary>
    /// Cot danh sach nay KHONG con moi dong mot ky nang: xen giua la cac DONG TIEU DE NHOM
    /// (LỬA / BĂNG / SÉT / PHONG / HỖ TRỢ - nguoi dung xin 18/09/2026), va dong tieu de thap hon
    /// hang ky nang. Nen moi cho tinh vi tri deu phai di qua hai ham nay, khong duoc nhan
    /// "chi so x chieu cao hang" nhu truoc.
    /// </summary>
    static float CaoDong(BoCuc b, int dong)
    {
        int nhom;
        SachPhep.KyNangODong(dong, out nhom);
        return nhom >= 0 ? b.caoHang * 0.52f : b.caoHang;
    }

    static float YCuaDong(BoCuc b, int dong)
    {
        float y = 0f;
        for (int i = 0; i < dong; i++) y += CaoDong(b, i);
        return y;
    }

    /// <summary>Tong chieu cao cua ca cot danh sach.</summary>
    static float CaoDanhSach(BoCuc b)
    {
        return YCuaDong(b, SachPhep.SoDongDanhSach);
    }

    /// <summary>
    /// Vung man hinh cua hang KY NANG <paramref name="ky"/> trong cot danh sach.
    /// Tra ve Rect rong neu ky nang ay dang bi cuon ra ngoai khung.
    /// Phep thu menu 59 goi ham nay chu khong tu nhan "chi so x chieu cao hang" - tu khi cot
    /// xep theo nhom (18/09/2026) cong thuc ay khong con dung.
    /// </summary>
    public static Rect VungHangKyNang(BoCuc b, int ky, float s)
    {
        float le = 6f * s;
        for (int dong = 0; dong < SachPhep.SoDongDanhSach; dong++)
        {
            int nhom;
            if (SachPhep.KyNangODong(dong, out nhom) != ky) continue;
            float y = b.kho.y + le + YCuaDong(b, dong) - cuonKho;
            var r = new Rect(b.kho.x + le, y, b.kho.width - le * 2f, b.caoHang - 6f * s);
            // Nam ngoai khung (dang bi cuon khuat) thi tra ve rong
            if (r.yMax > b.kho.yMax || r.y < b.kho.y) return new Rect();
            return r;
        }
        return new Rect();
    }

    static int HangTaiDiem(BoCuc b, Vector2 diem, float s)
    {
        if (!b.kho.Contains(diem)) return -1;
        float y = diem.y - (b.kho.y + 6f * s) + cuonKho;
        float moc = 0f;
        for (int dong = 0; dong < SachPhep.SoDongDanhSach; dong++)
        {
            float cao = CaoDong(b, dong);
            if (y >= moc && y < moc + cao)
            {
                int nhom;
                return SachPhep.KyNangODong(dong, out nhom);   // -1 neu cham vao dong tieu de
            }
            moc += cao;
        }
        return -1;
    }

    static int OTaiDiem(BoCuc b, Vector2 diem, float s)
    {
        int n = SachPhep.SoODangDung;
        for (int i = 0; i < n; i++)
        {
            var r = OTaiVung(b.vungO, i, s);
            if (CamUng.DangDung)
            {
                // O tron: do theo khoang cach toi tam, khong theo hinh vuong bao
                if (Vector2.Distance(diem, r.center) <= r.width * 0.5f) return i;
            }
            else if (r.Contains(diem)) return i;
        }
        return -1;
    }

    // ================================================================
    //  VE - goi tu OnGUI
    // ================================================================

    public static void Ve(float s, PlayerController pc, Texture2D[] icon)
    {
        if (!DangMo) return;

        GiaoDien.ChuanBi();
        var b = TinhBoCuc(Screen.width, Screen.height, s);

        // Man toi phu kin phia sau: dang mo sach thi khong nhin tran dau nua
        GUI.color = Color.white;   // GUI.color con sot lai tu HUD thi ca bang bi am mau
        GiaoDien.To(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.86f));

        GiaoDien.Khung(b.khung, s, true);

        var kTieuDe = new GUIStyle(GiaoDien.KieuTieuDe);
        kTieuDe.alignment = TextAnchor.MiddleCenter;
        kTieuDe.fontSize = Mathf.RoundToInt(30f * s);
        kTieuDe.normal.textColor = GiaoDien.MauMauSang;
        GUI.Label(b.tieuDe, "SÁCH PHÉP", kTieuDe);

        var kPhuDe = new GUIStyle(GiaoDien.KieuChuNho);
        kPhuDe.alignment = TextAnchor.MiddleCenter;
        kPhuDe.fontSize = Mathf.RoundToInt(15f * s);
        kPhuDe.normal.textColor = xemTruoc || CapDo.DiemKyNang > 0 ? GiaoDien.MauVang : GiaoDien.MauMo;
        GUI.Label(new Rect(b.tieuDe.x, b.tieuDe.yMax - 6f * s, b.tieuDe.width, 20f * s),
                  xemTruoc
                    ? "Xem trước mọi kỹ năng   ·   kéo vào ô để sắp sẵn, vào trận không phải chỉnh lại"
                    : "Nhân vật cấp " + CapDo.Cap + " / " + CapDo.CapToiDa
                      + "   ·   điểm kỹ năng chưa dùng: " + CapDo.DiemKyNang, kPhuDe);

        VeNutDong(b.nutDong, s);
        VeKho(b, s, icon);
        VeChiTiet(b, s, pc, icon);
        VeVungO(b, s, icon);
        VeThuDangKeo(s, icon);
    }

    static void VeNutDong(Rect r, float s)
    {
        GiaoDien.To(r, new Color(0.22f, 0.04f, 0.03f, 0.85f));
        var k = new GUIStyle(GiaoDien.KieuTieuDe);
        k.alignment = TextAnchor.MiddleCenter;
        k.fontSize = Mathf.RoundToInt(22f * s);
        k.normal.textColor = GiaoDien.MauGiay;
        GUI.Label(r, "X", k);
    }

    static void VeKho(BoCuc b, float s, Texture2D[] icon)
    {
        GiaoDien.To(b.kho, new Color(0.05f, 0.04f, 0.04f, 0.55f));

        float k = HeSoChuKho;
        var kTen = new GUIStyle(GiaoDien.KieuTieuDeNho);
        kTen.fontSize = Mathf.RoundToInt(19f * k * s);
        var kPhu = new GUIStyle(GiaoDien.KieuChuNho);
        kPhu.fontSize = Mathf.RoundToInt(14f * k * s);

        // Nhip dap cua hang / o dang chon (khong phu thuoc Time.timeScale)
        float nhip = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);

        // Kieu chu cua DONG TIEU DE NHOM
        var kNhom = new GUIStyle(GiaoDien.KieuChuNho);
        kNhom.fontSize = Mathf.RoundToInt(15f * k * s);
        kNhom.alignment = TextAnchor.LowerLeft;

        GUI.BeginGroup(b.kho);
        float le = 6f * s;
        for (int dong = 0; dong < SachPhep.SoDongDanhSach; dong++)
        {
            int nhom;
            int i = SachPhep.KyNangODong(dong, out nhom);
            float caoDong = CaoDong(b, dong);
            float y = le + YCuaDong(b, dong) - cuonKho;
            if (y > b.kho.height || y + caoDong < 0f) continue;

            // ----- DONG TIEU DE NHOM: ten he + mot duong ke mo keo het be ngang -----
            if (nhom >= 0)
            {
                var oNhom = new Rect(le + 4f * s, y, b.kho.width - le * 2f - 8f * s, caoDong - 3f * s);
                kNhom.normal.textColor = SachPhep.MauNhom[nhom];
                GUI.Label(oNhom, SachPhep.TenNhom[nhom], kNhom);

                // Duong ke chay tu sau chu den het be ngang
                float rongTenNhom = kNhom.CalcSize(new GUIContent(SachPhep.TenNhom[nhom])).x;
                var keNgang = new Rect(oNhom.x + rongTenNhom + 8f * s, oNhom.yMax - 6f * s,
                                       Mathf.Max(0f, oNhom.width - rongTenNhom - 10f * s), Mathf.Max(1f, 1.5f * s));
                var mauKe = SachPhep.MauNhom[nhom]; mauKe.a = 0.35f;
                GiaoDien.To(keNgang, mauKe);
                continue;
            }

            var hang = new Rect(le, y, b.kho.width - le * 2f, caoDong - 6f * s);
            bool chon = dangXem == i;
            int capKy = CapDo.CapCuaKyNang(i);
            bool daMo = xemTruoc || capKy > 0;

            // BA MUC SANG (nguoi dung 13/09/2026: hang dang chon phai "sang han
            // len", ky nang da mo / nang cap phai noi bat hon ky nang con khoa):
            //   dang chon  - long do ruc (dap nhe), vien sang, chu gan trang;
            //   da mo      - long am vang dong, chu vang sang, hinh du mau;
            //   con khoa   - long toi han, chu mo, hinh xam co o khoa.
            // Truoc day hang dang chon chi them mot lop do 28% va hang khoa bi phu
            // toi 45% - hang dang chon ma con khoa thi toi y nhu cac hang khac.
            GiaoDien.Hang(hang, s, GiaoDien.MauMau);
            if (chon) GiaoDien.To(hang, new Color(0.80f, 0.17f, 0.08f, 0.80f + 0.12f * nhip));
            else if (daMo) GiaoDien.To(hang, new Color(0.36f, 0.22f, 0.08f, 0.34f));
            else GiaoDien.To(hang, new Color(0.01f, 0.01f, 0.015f, 0.55f));

            float vach = Mathf.Max(2f, 5f * s);
            GiaoDien.To(new Rect(hang.x, hang.y, vach, hang.height),
                        chon ? new Color(1f, 0.80f, 0.45f) : daMo ? GiaoDien.MauVang : new Color(0.24f, 0.21f, 0.19f));

            float kt = hang.height - 10f * s;
            var rIcon = new Rect(hang.x + 10f * s, hang.y + 5f * s, kt, kt);
            if (icon != null && i < icon.Length && icon[i] != null)
            {
                var mc = GUI.color;
                GUI.color = daMo ? Color.white : new Color(0.38f, 0.36f, 0.40f, chon ? 0.95f : 0.70f);
                GUI.DrawTexture(rIcon, icon[i], ScaleMode.StretchToFill, true);
                GUI.color = mc;
            }
            // Vien MANH quanh hinh. Ban cu goi GiaoDien.DuongKe(rIcon) - ham ve DUONG
            // KE NGANG mo hai dau, bi keo gian phu kin ca hinh thanh mot dai nau cam
            // doc giua bieu tuong, va de mat luon o khoa (anh chup 13/09/2026).
            VienChuNhat(rIcon, chon ? new Color(1f, 0.75f, 0.40f, 1f) : new Color(0.40f, 0.30f, 0.20f, 0.9f),
                        Mathf.Max(1f, 1f * s));
            if (!daMo) IconKhoa.Ve(rIcon.center, rIcon.width * 0.55f);

            kTen.normal.textColor = chon ? new Color(1f, 0.95f, 0.84f)
                                  : daMo ? new Color(1f, 0.82f, 0.46f) : new Color(0.46f, 0.43f, 0.40f);
            kPhu.normal.textColor = chon ? GiaoDien.MauGiay
                                  : daMo ? new Color(0.80f, 0.74f, 0.66f) : new Color(0.38f, 0.36f, 0.34f);

            float xChu = rIcon.xMax + 10f * s;
            float rongChu = hang.xMax - xChu - 6f * s;
            GUI.Label(new Rect(xChu, hang.y + 6f * k * s, rongChu, 22f * k * s), SachPhep.Ten(i), kTen);
            GUI.Label(new Rect(xChu, hang.y + 28f * k * s, rongChu, 20f * k * s), SachPhep.TomTat(i), kPhu);

            // Goc phai moi hang: cap ky nang (hoac "chưa mở"), va o dang giu no
            var kGoc = new GUIStyle(kPhu);
            kGoc.alignment = TextAnchor.MiddleRight;
            kGoc.normal.textColor = daMo ? (chon ? new Color(0.72f, 1f, 0.62f) : GiaoDien.MauXanh)
                                         : (chon ? GiaoDien.MauLoi : new Color(0.40f, 0.37f, 0.35f));
            string chuGoc = xemTruoc ? ("tối đa cấp " + CapDo.CapToiDaCua(i))
                          : !daMo ? "chưa mở"
                          : CapDo.LaKyBinh(i) ? ("Cấp " + capKy + "/" + CapDo.CapToiDaCua(i) + "  ·  còn " + CapDo.SoBinh(i) + " bình")
                          : ("Cấp " + capKy + "/" + CapDo.CapToiDaCua(i));

            int o = SachPhep.ONaoGiu(i);
            if (daMo && o >= 0) chuGoc += "  ·  ô " + (o + 1);
            GUI.Label(new Rect(hang.x, hang.yMax - 24f * k * s, hang.width - 8f * s, 20f * k * s), chuGoc, kGoc);

            // Vien sang quanh hang dang chon - hai lop: net sang trong, quang mo ngoai
            if (chon)
            {
                VienChuNhat(hang, new Color(1f, 0.62f, 0.32f, 1f), Mathf.Max(1f, 2f * s));
                VienChuNhat(new Rect(hang.x - 2f * s, hang.y - 2f * s, hang.width + 4f * s, hang.height + 4f * s),
                            new Color(1f, 0.35f, 0.15f, 0.35f + 0.25f * nhip), Mathf.Max(1f, 2f * s));
            }
        }
        GUI.EndGroup();

        VeThanhCuon(b.kho, cuonKho, le * 2f + CaoDanhSach(b), s);
    }

    /// <summary>
    /// Thanh cuon mong o mep phai mot vung - chi hien khi noi dung dai hon vung.
    /// Nguoi dung xin hai vung cuon duoc vi sau nay nhieu ky nang, loi ke dai; khong
    /// co thanh cuon thi nguoi choi khong biet ben duoi con chu.
    /// </summary>
    static void VeThanhCuon(Rect vung, float cuon, float caoNoiDung, float s)
    {
        if (caoNoiDung <= vung.height + 1f) return;
        float rong = Mathf.Max(3f, 5f * s);
        var ray = new Rect(vung.xMax - rong - 3f * s, vung.y + 4f * s, rong, vung.height - 8f * s);
        GiaoDien.To(ray, new Color(0.18f, 0.05f, 0.04f, 0.55f));
        float tiLe = Mathf.Clamp01(vung.height / caoNoiDung);
        float caoCan = Mathf.Max(18f * s, ray.height * tiLe);
        float toiDa = Mathf.Max(1f, caoNoiDung - vung.height);
        float y = ray.y + (ray.height - caoCan) * Mathf.Clamp01(cuon / toiDa);
        GiaoDien.To(new Rect(ray.x, y, rong, caoCan), new Color(0.88f, 0.42f, 0.22f, 0.90f));
    }

    static void VeChiTiet(BoCuc b, float s, PlayerController pc, Texture2D[] icon)
    {
        GiaoDien.To(b.chiTiet, new Color(0.05f, 0.04f, 0.04f, 0.55f));

        var kTen = new GUIStyle(GiaoDien.KieuTieuDe);
        kTen.fontSize = Mathf.RoundToInt(24f * s);
        kTen.normal.textColor = GiaoDien.MauVang;
        var kSo = new GUIStyle(GiaoDien.KieuChu);
        kSo.fontSize = Mathf.RoundToInt(16f * s);

        // ---- DAU MUC: hinh + ten + thong so. DUNG YEN, khong cuon ----
        float le = 14f * s;
        float rong = b.chiTiet.width - le * 2f;
        float kt = 56f * s;
        float y = b.chiTiet.y + le;
        float x = b.chiTiet.x + le;
        if (icon != null && dangXem < icon.Length && icon[dangXem] != null)
            GUI.DrawTexture(new Rect(x, y, kt, kt), icon[dangXem], ScaleMode.StretchToFill, true);

        GUI.Label(new Rect(x + kt + 12f * s, y + 4f * s, rong - kt - 12f * s, 30f * s),
                  SachPhep.Ten(dangXem), kTen);

        bool laBinh = CapDo.LaKyBinh(dangXem);
        float nl, hc, nc;
        SachPhep.ThongSo(pc, dangXem, out nl, out hc, out nc);
        string dongSo = laBinh
            ? "Hồi tối đa " + (dangXem == CapDo.KyBinhMau
                                ? Mathf.RoundToInt(PlayerController.MauBinhTheoCap(Mathf.Max(1, CapDo.CapCuaKyNang(dangXem)))) + " máu"
                                : Mathf.RoundToInt(PlayerController.ManaBinhTheoCap(Mathf.Max(1, CapDo.CapCuaKyNang(dangXem)))) + " năng lượng")
              + "   ·   Hồi chiêu " + hc.ToString("0.##") + " giây"
              + (xemTruoc ? "" : "   ·   Đang có " + CapDo.SoBinh(dangXem) + " bình")
            : pc == null ? "Tối đa cấp " + CapDo.CapToiDaCua(dangXem)
            : "Năng lượng " + Mathf.RoundToInt(nl)
              + "   ·   Hồi chiêu " + hc.ToString("0.##") + " giây"
              + "   ·   Niệm " + nc.ToString("0.##") + " giây";
        GUI.Label(new Rect(x + kt + 12f * s, y + 32f * s, rong - kt - 12f * s, 22f * s), dongSo, kSo);

        GiaoDien.DuongKe(new Rect(x, b.thanChiTiet.y - 10f * s, rong, Mathf.Max(1f, s)),
                         new Color(0.40f, 0.09f, 0.07f, 0.9f));

        // ---- THAN: dong cap + loi ke. CUON DUOC, chu to them 20% ----
        // Nguoi dung (13/09/2026): vung nay chu to them 20% va phai cuon len xuong
        // duoc - sau nay nhieu ky nang, loi ke dai, mot khung co dinh khong chua het.
        // Truoc day ca khung chi tiet (ke ca hinh va ten) cuon chung va chi bang con
        // lan chuot; dien thoai khong cuon duoc.
        float h = HeSoChuThan;
        var kCap = new GUIStyle(kSo);
        kCap.fontSize = Mathf.RoundToInt(16f * h * s);
        kCap.wordWrap = true;
        kCap.alignment = TextAnchor.UpperLeft;
        var kChu = new GUIStyle(GiaoDien.KieuChu);
        kChu.fontSize = Mathf.RoundToInt(16f * h * s);
        kChu.wordWrap = true;
        kChu.alignment = TextAnchor.UpperLeft;

        int capKy = CapDo.CapCuaKyNang(dangXem);
        kCap.normal.textColor = xemTruoc || capKy > 0 ? GiaoDien.MauXanh : GiaoDien.MauLoi;
        string dongCap;
        if (xemTruoc)
        {
            // O sanh chua co cap: ke cach mo va SUC MANH O CAP TOI DA de nguoi choi so sanh
            int toiDa = CapDo.CapToiDaCua(dangXem);
            if (laBinh)
                dongCap = "Có sẵn cấp 1   ·   tối đa cấp " + toiDa + "   ·   ở cấp " + toiDa + ": hồi "
                        + (dangXem == CapDo.KyBinhMau
                           ? Mathf.RoundToInt(PlayerController.MauBinhTheoCap(toiDa)) + " máu"
                           : Mathf.RoundToInt(PlayerController.ManaBinhTheoCap(toiDa)) + " năng lượng");
            else if (dangXem == CapDo.KyTocDo)
                dongCap = "Mở khoá bằng 1 điểm kỹ năng   ·   tối đa cấp " + toiDa + "   ·   ở cấp " + toiDa
                        + ": nhanh hơn " + Mathf.RoundToInt(CapDo.TocThemTheoCap(toiDa) * 100f) + "% tốc độ gốc";
            else
                dongCap = "Mở khoá bằng 1 điểm kỹ năng   ·   tối đa cấp " + toiDa
                        + "   ·   ở cấp " + toiDa + ": sát thương ×" + CapDo.SatThuongTheoCap(toiDa).ToString("0.00")
                        + "   ·   năng lượng ×" + (CapDo.ManaTheoCap(toiDa) * (dangXem == CapDo.KyGioLoc ? GioLoc.HeSoNangLuongTheoCap(toiDa) : 1f)).ToString("0.00")
                        + (dangXem == 5
                           ? "   ·   máu khiên ×" + CapDo.MauKhiengTheoCap(toiDa).ToString("0.00")
                           : "   ·   hiệu ứng +" + CapDo.ThemGiayHieuUngTheoCap(toiDa).ToString("0.00") + " giây");
        }
        else if (capKy <= 0)
        {
            // Ky nang bi khoa sau mot ky nang khac (nguoi dung 19/09/2026) thi noi ro can gi,
            // khong de nguoi choi bam mai vao cai nut xam ma khong hieu vi sao.
            string nhac = SachPhep.NhacDieuKien(dangXem);
            dongCap = nhac != null ? "CHƯA MỞ KHOÁ — " + nhac : "CHƯA MỞ KHOÁ — cần 1 điểm kỹ năng";
        }
        else if (laBinh)
            dongCap = "Bình cấp " + capKy + " / " + CapDo.CapToiDaCua(dangXem) + "   ·   hồi tối đa "
                    + (dangXem == CapDo.KyBinhMau
                       ? Mathf.RoundToInt(PlayerController.MauBinhTheoCap(capKy)) + " máu"
                       : Mathf.RoundToInt(PlayerController.ManaBinhTheoCap(capKy)) + " năng lượng")
                    + "   ·   đang có " + CapDo.SoBinh(dangXem) + " bình";
        else if (dangXem == CapDo.KyTocDo)
            dongCap = "Bị động cấp " + capKy + " / " + CapDo.CapToiDaCua(dangXem) + "   ·   nhanh hơn "
                    + (CapDo.TocThemTheoCap(capKy) * 100f).ToString("0.#") + "% tốc độ gốc";
        else
            dongCap = "Kỹ năng cấp " + capKy + " / " + CapDo.CapToiDaCua(dangXem)
                    + "   ·   sát thương ×" + CapDo.SatThuongTheoCap(capKy).ToString("0.00")
                    + "   ·   năng lượng ×" + (CapDo.ManaTheoCap(capKy) * (dangXem == CapDo.KyGioLoc ? GioLoc.HeSoNangLuongTheoCap(capKy) : 1f)).ToString("0.00")
                    + (dangXem == 5
                       ? "   ·   máu khiên ×" + CapDo.MauKhiengTheoCap(capKy).ToString("0.00")
                       : "   ·   hiệu ứng +" + CapDo.ThemGiayHieuUngTheoCap(capKy).ToString("0.00") + " giây");

        var than = b.thanChiTiet;
        float rongChu = than.width - le * 2f - 10f * s;          // chua cho thanh cuon
        string mo = SachPhep.MoTa(dangXem);
        float caoCap = kCap.CalcHeight(new GUIContent(dongCap), rongChu);
        float caoChu = kChu.CalcHeight(new GUIContent(mo), rongChu);
        caoNoiDungThan = caoCap + 8f * s + caoChu + 6f * s;

        GUI.BeginGroup(than);
        float yt = -cuonChiTiet;
        GUI.Label(new Rect(le, yt, rongChu, caoCap), dongCap, kCap);
        yt += caoCap + 8f * s;
        GUI.Label(new Rect(le, yt, rongChu, caoChu), mo, kChu);
        GUI.EndGroup();

        VeThanhCuon(than, cuonChiTiet, caoNoiDungThan, s);
        VeNutHoc(b, s);
    }

    /// <summary>
    /// Nut MO KHOA / NANG CAP o day khung chi tiet.
    ///
    /// Mot cho duy nhat cho ca hai viec: con khoa thi la "mo khoa", da mo thi
    /// la "nang cap", het diem hoac da toi da thi ghi ro VI SAO khong bam duoc -
    /// nut xam khong noi gi chi lam nguoi choi bam mai.
    /// </summary>
    static void VeNutHoc(BoCuc b, float s)
    {
        var r = b.nutHoc;
        int capKy = CapDo.CapCuaKyNang(dangXem);
        bool moDuoc = CapDo.MoKhoaDuoc(dangXem);
        bool nangDuoc = CapDo.NangCapDuoc(dangXem);
        bool bamDuoc = moDuoc || nangDuoc;

        string chu;
        if (xemTruoc)
        {
            bamDuoc = false;
            chu = "Mở khoá và nâng cấp trong trận — mỗi lần lên cấp được 1 điểm kỹ năng";
        }
        else if (capKy == 0)
        {
            string nhac = SachPhep.NhacDieuKien(dangXem);
            chu = nhac != null ? nhac
                : (CapDo.DiemKyNang > 0 ? "MỞ KHOÁ  (1 điểm)" : "Hết điểm kỹ năng — lên cấp để có thêm");
        }
        else if (capKy >= CapDo.CapToiDaCua(dangXem))
            chu = "ĐÃ TỐI ĐA (cấp " + CapDo.CapToiDaCua(dangXem) + ")";
        else
            chu = CapDo.DiemKyNang > 0
                ? "NÂNG LÊN CẤP " + (capKy + 1) + "  (1 điểm)"
                : "Hết điểm kỹ năng — lên cấp để có thêm";

        GiaoDien.To(r, bamDuoc ? new Color(0.45f, 0.09f, 0.06f, 0.92f)
                               : new Color(0.12f, 0.11f, 0.11f, 0.85f));
        float d = Mathf.Max(1f, 2f * s);
        var vien = bamDuoc ? GiaoDien.MauMauSang : new Color(0.30f, 0.27f, 0.25f);
        GiaoDien.To(new Rect(r.x, r.y, r.width, d), vien);
        GiaoDien.To(new Rect(r.x, r.yMax - d, r.width, d), vien);
        GiaoDien.To(new Rect(r.x, r.y, d, r.height), vien);
        GiaoDien.To(new Rect(r.xMax - d, r.y, d, r.height), vien);

        var k = new GUIStyle(GiaoDien.KieuTieuDeNho);
        k.alignment = TextAnchor.MiddleCenter;
        k.fontSize = Mathf.RoundToInt(17f * s);
        k.normal.textColor = bamDuoc ? GiaoDien.MauGiay : GiaoDien.MauToi;
        GUI.Label(r, chu, k);
    }

    /// <summary>Dong chu nhac o tren cung vung o. Dung CHUNG cho luc ve va luc xep
    /// o - hai cho tu tinh rieng thi mot ngay nao do lech nhau va o de len chu.</summary>
    public static Rect DongNhacO(Rect vungO, float s)
    {
        return new Rect(vungO.x, vungO.y + 4f * s, vungO.width, 20f * s);
    }

    static Texture2D nenTron;

    /// <summary>Mot dia tron mep mem - nen cho o tron.</summary>
    static Texture2D NenTron
    {
        get
        {
            if (nenTron != null) return nenTron;
            const int n = 64;
            nenTron = new Texture2D(n, n, TextureFormat.RGBA32, false);
            nenTron.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n - 0.5f, dy = (y + 0.5f) / n - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    px[y * n + x] = new Color(1f, 1f, 1f, Mathf.Clamp01((0.5f - d) * n));
                }
            nenTron.SetPixels(px);
            nenTron.Apply(false, false);
            return nenTron;
        }
    }

    static void VeVungO(BoCuc b, float s, Texture2D[] icon)
    {
        GiaoDien.To(b.vungO, new Color(0.05f, 0.04f, 0.04f, 0.55f));

        var kNhac = new GUIStyle(GiaoDien.KieuChuNho);
        kNhac.fontSize = Mathf.RoundToInt(14f * s);
        kNhac.alignment = TextAnchor.UpperCenter;
        GUI.Label(DongNhacO(b.vungO, s),
                  CamUng.DangDung
                    ? "Kéo kỹ năng bên trái thả vào ô — sắp xếp ở đây thế nào thì trong trận hiện ra y hệt"
                    : "Kéo kỹ năng bên trái thả vào ô — thứ tự ô cũng là thứ tự phím 1…7",
                  kNhac);

        int n = SachPhep.SoODangDung;
        var bo = SachPhep.BoDangDung;
        float nhip = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);

        // O DANG CHON (nguoi dung 13/09/2026: cham vao o nao thi phai thay la
        // dang chon o do). O co ky nang thi sang theo KY NANG dang xem - chon o
        // cot trai cung sang luon o dang giu no, keo doi cho thi quang sang di
        // theo. Cham vao o TRONG thi chinh o trong ay sang.
        bool chonOTrong = oDangChon >= 0 && oDangChon < n && bo[oDangChon] == SachPhep.Trong;

        for (int i = 0; i < n; i++)
        {
            var r = OTaiVung(b.vungO, i, s);
            bool tron = CamUng.DangDung;
            int ky = bo[i];
            bool chon = chonOTrong ? i == oDangChon : (ky != SachPhep.Trong && ky == dangXem);
            var mauCu = GUI.color;

            // Quang sang sau o dang chon - ve TRUOC nen o
            if (chon)
            {
                float mo = 7f * s + 3f * s * nhip;
                var rq = new Rect(r.x - mo, r.y - mo, r.width + mo * 2f, r.height + mo * 2f);
                var mauQuang = new Color(1f, 0.52f, 0.18f, 0.40f + 0.22f * nhip);
                if (tron)
                {
                    GUI.color = mauQuang;
                    GUI.DrawTexture(rq, NenTron, ScaleMode.StretchToFill, true);
                }
                else GiaoDien.To(rq, mauQuang);
            }

            // Nen o. O TRON thi nen cung phai TRON: truoc day dung GiaoDien.Trang
            // (mot anh vuong) nen sau moi o tron lo ra mot o vuong xam.
            if (tron)
            {
                GUI.color = new Color(0f, 0f, 0f, 0.55f);
                GUI.DrawTexture(r, NenTron, ScaleMode.StretchToFill, true);
            }
            else GiaoDien.To(r, new Color(0.10f, 0.09f, 0.09f, 0.75f));
            GUI.color = mauCu;

            if (ky != SachPhep.Trong && icon != null && ky < icon.Length && icon[ky] != null)
            {
                // O dang duoc nhac di thi ve mo, cho biet no dang o dau ngon tay
                bool dangNhac = dangKeo && keoTuO == i;
                bool daMo = HienDaMo(ky);

                // KY NANG CHUA MO: xam di va co O KHOA - DUNG mau va DUNG ti le voi
                // cum nut / thanh o ngoai tran (GameHUD), nguoi dung xin "nhu o ngoai game"
                var c = GUI.color;
                GUI.color = daMo ? new Color(1f, 1f, 1f, dangNhac ? 0.25f : 1f)
                                 : new Color(0.38f, 0.36f, 0.40f, dangNhac ? 0.20f : 0.75f);
                float co = tron ? r.width * 0.86f : r.width - 8f * s;
                GUI.DrawTexture(new Rect(r.center.x - co * 0.5f, r.center.y - co * 0.5f, co, co),
                                icon[ky], ScaleMode.StretchToFill, true);
                GUI.color = c;
                if (!daMo && !dangNhac)
                    IconKhoa.Ve(r.center, tron ? r.width * 0.5f * GameHUD.RongKhoaTron
                                               : r.width * GameHUD.RongKhoaVuong);
            }

            if (CapDo.LaKyBinh(ky) && !xemTruoc) VeSoBinhO(r, ky, s);

            if (chon)
            {
                VienO(r, tron, new Color(1f, 0.86f, 0.46f, 1f), s, 3.5f);
                if (!tron)
                    VienChuNhat(new Rect(r.x - 3f * s, r.y - 3f * s, r.width + 6f * s, r.height + 6f * s),
                                new Color(1f, 0.45f, 0.18f, 0.55f + 0.3f * nhip), Mathf.Max(1f, 2f * s));
            }
            else VienO(r, tron, new Color(0.55f, 0.42f, 0.28f, 0.95f), s, 2f);

            // So thu tu chi ve o BAN MAY TINH - o day no chinh la phim tat.
            // Ban cam ung khong co phim tat, ma cac o tron nam sat nhau nen con
            // so lai de len o ben canh.
            if (!tron)
            {
                var kSo = new GUIStyle(GiaoDien.KieuChuNho);
                kSo.alignment = TextAnchor.MiddleCenter;
                kSo.fontSize = Mathf.RoundToInt(13f * s);
                kSo.normal.textColor = chon ? new Color(1f, 0.92f, 0.70f) : GiaoDien.MauVang;
                GUI.Label(new Rect(r.x, r.yMax + 4f * s, r.width, 18f * s), (i + 1).ToString(), kSo);
            }
        }
    }

    /// <summary>So binh con lai o goc phai duoi o - giong o ngoai tran.</summary>
    static void VeSoBinhO(Rect r, int ky, float s)
    {
        int n = CapDo.SoBinh(ky);
        var k = new GUIStyle(GiaoDien.KieuTieuDeNho);
        k.alignment = TextAnchor.MiddleCenter;
        // Bo le cua kieu goc: co le thi CalcSize tinh sai be ngang, "12" tran ra ngoai khung
        // (anh chup lan chay dau menu 65, o tron ban cam ung)
        k.padding = new RectOffset(0, 0, 0, 0);
        k.margin = new RectOffset(0, 0, 0, 0);
        k.fontSize = Mathf.Max(10, Mathf.RoundToInt(r.height * 0.30f));
        k.normal.textColor = n > 0 ? new Color(1f, 0.95f, 0.82f) : new Color(0.75f, 0.45f, 0.40f);
        k.clipping = TextClipping.Overflow;
        k.wordWrap = false;
        float kt = Mathf.Max(18f * s, r.height * 0.42f);
        string chu = n.ToString();
        float rong = Mathf.Max(kt, k.CalcSize(new GUIContent(chu)).x * 1.25f + 8f * s + 6f);
        var nen = new Rect(r.xMax - rong, r.yMax - kt, rong, kt);
        GiaoDien.To(nen, new Color(0.05f, 0.02f, 0.02f, 0.92f));
        VienChuNhat(nen, n > 0 ? new Color(0.85f, 0.62f, 0.30f, 0.95f) : new Color(0.40f, 0.30f, 0.28f, 0.9f), Mathf.Max(1f, s));
        GUI.Label(nen, chu, k);
    }

    static void VienChuNhat(Rect r, Color mau, float d)
    {
        GiaoDien.To(new Rect(r.x, r.y, r.width, d), mau);
        GiaoDien.To(new Rect(r.x, r.yMax - d, r.width, d), mau);
        GiaoDien.To(new Rect(r.x, r.y, d, r.height), mau);
        GiaoDien.To(new Rect(r.xMax - d, r.y, d, r.height), mau);
    }

    /// <summary>Vien quanh mot o - tron thi ve bang bon cung gia, vuong thi bon canh.</summary>
    static void VienO(Rect r, bool tron, Color mau, float s, float day)
    {
        float d = Mathf.Max(1f, day * s);
        if (!tron)
        {
            VienChuNhat(r, mau, d);
            return;
        }

        // Vong tron: ve mot vanh bang cach chong hai hinh tron (Trang la anh
        // tron mo dan - dung no lam vanh thi vien nhoe, nen ve bang 28 doan nho)
        int SoDoan = day > 2.5f ? 40 : 28;
        float bk = r.width * 0.5f;
        for (int i = 0; i < SoDoan; i++)
        {
            float a = i * Mathf.PI * 2f / SoDoan;
            var p = new Vector2(r.center.x + Mathf.Cos(a) * bk, r.center.y + Mathf.Sin(a) * bk);
            GiaoDien.To(new Rect(p.x - d, p.y - d, d * 2f, d * 2f), mau);
        }
    }

    static void VeThuDangKeo(float s, Texture2D[] icon)
    {
        if (!dangKeo) return;
        int ky = keoTuKho >= 0 ? keoTuKho
               : (keoTuO >= 0 ? SachPhep.BoDangDung[keoTuO] : SachPhep.Trong);
        if (ky == SachPhep.Trong || icon == null || ky >= icon.Length || icon[ky] == null) return;

        float co = 64f * s;
        var c = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.92f);
        GUI.DrawTexture(new Rect(troHienGio.x - co * 0.5f, troHienGio.y - co * 0.5f, co, co),
                        icon[ky], ScaleMode.StretchToFill, true);
        GUI.color = c;
    }
}
