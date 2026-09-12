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

    static float cuonKho, cuonChiTiet;

    // ---- Keo tha ----
    static int keoTuKho = -1;     // so hieu ky nang dang keo tu cot trai
    static int keoTuO = -1;       // o dang keo di
    static bool dangKeo;
    static bool dangCuon;
    static Vector2 diemBatDau;
    static Vector2 troHienGio;
    static bool nhanTruoc;

    /// <summary>Nguong (diem anh) phai di qua thi moi tinh la keo, khong phai cham.</summary>
    const float NguongKeo = 12f;

    public static void Mo() { DangMo = true; HuyKeo(); }
    public static void Dong() { DangMo = false; HuyKeo(); }
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
    }

    public static BoCuc TinhBoCuc(float W, float H, float s)
    {
        var b = new BoCuc();

        // Chua het man hinh nhung de lai vien: nguoi choi phai thay minh van
        // dang o trong tran dau chu khong phai da roi ra mot man khac.
        float rong = Mathf.Min(W * 0.92f, 1180f * s);
        float cao = Mathf.Min(H * 0.88f, 720f * s);
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

        b.caoHang = 64f * s;

        // Dai nut MO KHOA / NANG CAP nam sat day khung chi tiet
        b.nutHoc = new Rect(b.chiTiet.x + 14f * s, b.chiTiet.yMax - 50f * s,
                            b.chiTiet.width - 28f * s, 40f * s);
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
            else if (b.chiTiet.Contains(tro)) cuonChiTiet -= lan * 42f * s;
        }

        if (vuaNhan)
        {
            diemBatDau = tro;
            dangKeo = false; dangCuon = false;
            keoTuKho = -1; keoTuO = -1;

            if (b.nutDong.Contains(tro)) { Dong(); return; }

            // Nut MO KHOA / NANG CAP
            if (b.nutHoc.Contains(tro))
            {
                if (CapDo.MoKhoaDuoc(dangXem)) CapDo.MoKhoa(dangXem);
                else if (CapDo.NangCapDuoc(dangXem)) CapDo.NangCap(dangXem);
                return;
            }

            int hang = HangTaiDiem(b, tro, s);
            if (hang >= 0)
            {
                dangXem = hang; cuonChiTiet = 0f;
                // KY NANG CON KHOA THI KHONG KEO DUOC: keo duoc thi no nam tren
                // thanh ky nang nhu mot nut that, bam vao chi hien ra loi tu choi.
                keoTuKho = CapDo.DaMo(hang) ? hang : -1;
            }
            else
            {
                int o = OTaiDiem(b, tro, s);
                if (o >= 0)
                {
                    int ky = SachPhep.BoDangDung[o];
                    if (ky != SachPhep.Trong) { dangXem = ky; cuonChiTiet = 0f; }
                    keoTuO = o;
                }
                // Cham ra ngoai han cua so thi dong lai - giong moi cua so khac
                else if (!b.khung.Contains(tro)) { Dong(); return; }
            }
        }

        if (nhan && (keoTuKho >= 0 || keoTuO >= 0 || b.kho.Contains(diemBatDau)))
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
                    if (o >= 0) SachPhep.DatVaoO(o, keoTuKho);
                }
                else if (keoTuO >= 0)
                {
                    if (o >= 0 && o != keoTuO) SachPhep.DoiHaiO(keoTuO, o);
                    else if (o < 0 && !b.vungO.Contains(tro)) SachPhep.BoKhoiO(keoTuO);
                }
            }
            HuyKeo();
        }

        KepCuon(b, s);
    }

    static void KepCuon(BoCuc b, float s)
    {
        float caoNoiDung = SachPhep.SoKyNang * b.caoHang;
        float toiDa = Mathf.Max(0f, caoNoiDung - (b.kho.height - 12f * s));
        cuonKho = Mathf.Clamp(cuonKho, 0f, toiDa);
        cuonChiTiet = Mathf.Max(0f, cuonChiTiet);
    }

    static int HangTaiDiem(BoCuc b, Vector2 diem, float s)
    {
        if (!b.kho.Contains(diem)) return -1;
        float y = diem.y - (b.kho.y + 6f * s) + cuonKho;
        int i = Mathf.FloorToInt(y / b.caoHang);
        return (i >= 0 && i < SachPhep.SoKyNang) ? i : -1;
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
        kPhuDe.normal.textColor = CapDo.DiemKyNang > 0 ? GiaoDien.MauVang : GiaoDien.MauMo;
        GUI.Label(new Rect(b.tieuDe.x, b.tieuDe.yMax - 6f * s, b.tieuDe.width, 20f * s),
                  "Nhân vật cấp " + CapDo.Cap + " / " + CapDo.CapToiDa
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

        var kTen = new GUIStyle(GiaoDien.KieuTieuDeNho);
        kTen.fontSize = Mathf.RoundToInt(19f * s);
        var kPhu = new GUIStyle(GiaoDien.KieuChuNho);
        kPhu.fontSize = Mathf.RoundToInt(14f * s);

        GUI.BeginGroup(b.kho);
        float le = 6f * s;
        for (int i = 0; i < SachPhep.SoKyNang; i++)
        {
            float y = le + i * b.caoHang - cuonKho;
            if (y > b.kho.height || y + b.caoHang < 0f) continue;

            var hang = new Rect(le, y, b.kho.width - le * 2f, b.caoHang - 6f * s);
            bool chon = dangXem == i;
            GiaoDien.Hang(hang, s, chon ? GiaoDien.MauMauSang : GiaoDien.MauMau);
            if (chon) GiaoDien.To(hang, new Color(0.45f, 0.08f, 0.06f, 0.28f));

            float kt = hang.height - 10f * s;
            var rIcon = new Rect(hang.x + 8f * s, hang.y + 5f * s, kt, kt);
            if (icon != null && i < icon.Length && icon[i] != null)
                GUI.DrawTexture(rIcon, icon[i], ScaleMode.StretchToFill, true);
            GiaoDien.DuongKe(rIcon, new Color(0.40f, 0.30f, 0.20f, 0.9f));

            float xChu = rIcon.xMax + 10f * s;
            float rongChu = hang.xMax - xChu - 6f * s;
            GUI.Label(new Rect(xChu, hang.y + 6f * s, rongChu, 22f * s), SachPhep.Ten(i), kTen);
            GUI.Label(new Rect(xChu, hang.y + 28f * s, rongChu, 20f * s), SachPhep.TomTat(i), kPhu);

            // Goc phai moi hang: cap ky nang (hoac "chưa mở"), va o dang giu no
            var kGoc = new GUIStyle(kPhu);
            kGoc.alignment = TextAnchor.MiddleRight;
            int capKy = CapDo.CapCuaKyNang(i);
            kGoc.normal.textColor = capKy > 0 ? GiaoDien.MauVang : GiaoDien.MauToi;
            string chuGoc = capKy > 0 ? ("Cấp " + capKy + "/" + CapDo.CapKyNangToiDa) : "chưa mở";

            int o = SachPhep.ONaoGiu(i);
            if (capKy > 0 && o >= 0) chuGoc += "  ·  ô " + (o + 1);
            GUI.Label(new Rect(hang.x, hang.yMax - 24f * s, hang.width - 8f * s, 20f * s), chuGoc, kGoc);

            // Con khoa thi ca hang xam di - liec mot cai la biet cai nao dung duoc
            if (capKy == 0) GiaoDien.To(hang, new Color(0.02f, 0.02f, 0.03f, 0.45f));
        }
        GUI.EndGroup();
    }

    static void VeChiTiet(BoCuc b, float s, PlayerController pc, Texture2D[] icon)
    {
        GiaoDien.To(b.chiTiet, new Color(0.05f, 0.04f, 0.04f, 0.55f));

        var kTen = new GUIStyle(GiaoDien.KieuTieuDe);
        kTen.fontSize = Mathf.RoundToInt(24f * s);
        kTen.normal.textColor = GiaoDien.MauVang;
        var kSo = new GUIStyle(GiaoDien.KieuChu);
        kSo.fontSize = Mathf.RoundToInt(16f * s);
        var kChu = new GUIStyle(GiaoDien.KieuChu);
        kChu.fontSize = Mathf.RoundToInt(16f * s);
        kChu.wordWrap = true;
        kChu.alignment = TextAnchor.UpperLeft;

        // Chua cho cho dai nut o day: cat group ngan lai thi loi ke dai khong
        // chay xuong duoi nut roi lo ra o hai ben.
        var vungChu = new Rect(b.chiTiet.x, b.chiTiet.y, b.chiTiet.width, b.chiTiet.height - 62f * s);
        GUI.BeginGroup(vungChu);
        float le = 14f * s;
        float y = le - cuonChiTiet;
        float rong = vungChu.width - le * 2f;

        float kt = 56f * s;
        if (icon != null && dangXem < icon.Length && icon[dangXem] != null)
            GUI.DrawTexture(new Rect(le, y, kt, kt), icon[dangXem], ScaleMode.StretchToFill, true);

        GUI.Label(new Rect(le + kt + 12f * s, y + 4f * s, rong - kt - 12f * s, 30f * s),
                  SachPhep.Ten(dangXem), kTen);

        float nl, hc, nc;
        SachPhep.ThongSo(pc, dangXem, out nl, out hc, out nc);
        GUI.Label(new Rect(le + kt + 12f * s, y + 32f * s, rong - kt - 12f * s, 22f * s),
                  "Năng lượng " + Mathf.RoundToInt(nl)
                  + "   ·   Hồi chiêu " + hc.ToString("0.##") + " giây"
                  + "   ·   Niệm " + nc.ToString("0.##") + " giây", kSo);

        y += kt + 14f * s;
        GiaoDien.DuongKe(new Rect(le, y, rong, Mathf.Max(1f, s)), new Color(0.40f, 0.09f, 0.07f, 0.9f));
        y += 10f * s;

        // Suc manh theo cap - doc THANG tu CapDo, khong chep lai cong thuc
        int capKy = CapDo.CapCuaKyNang(dangXem);
        var kCap = new GUIStyle(kSo);
        kCap.normal.textColor = capKy > 0 ? GiaoDien.MauXanh : GiaoDien.MauLoi;
        string dongCap = capKy > 0
            ? "Kỹ năng cấp " + capKy + " / " + CapDo.CapKyNangToiDa
              + "   ·   sát thương ×" + CapDo.SatThuongTheoCap(capKy).ToString("0.00")
              + "   ·   năng lượng ×" + CapDo.ManaTheoCap(capKy).ToString("0.00")
              + (dangXem == 5
                 ? "   ·   máu khiên ×" + CapDo.MauKhiengTheoCap(capKy).ToString("0.00")
                 : "   ·   hiệu ứng +" + CapDo.ThemGiayHieuUngTheoCap(capKy).ToString("0.00") + " giây")
            : "CHƯA MỞ KHOÁ — cần 1 điểm kỹ năng";
        GUI.Label(new Rect(le, y, rong, 22f * s), dongCap, kCap);
        y += 26f * s;

        string mo = SachPhep.MoTa(dangXem);
        float caoChu = kChu.CalcHeight(new GUIContent(mo), rong);
        GUI.Label(new Rect(le, y, rong, caoChu), mo, kChu);

        GUI.EndGroup();

        VeNutHoc(b, s);

        // Kep lai cho khoi cuon qua day
        float tongCao = kt + 50f * s + caoChu + le * 2f + 56f * s;   // +56 cho dai nut
        float toiDa = Mathf.Max(0f, tongCao - b.chiTiet.height);
        cuonChiTiet = Mathf.Clamp(cuonChiTiet, 0f, toiDa);
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
        if (capKy == 0)
            chu = CapDo.DiemKyNang > 0 ? "MỞ KHOÁ  (1 điểm)" : "Hết điểm kỹ năng — lên cấp để có thêm";
        else if (capKy >= CapDo.CapKyNangToiDa)
            chu = "ĐÃ TỐI ĐA (cấp " + CapDo.CapKyNangToiDa + ")";
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
        for (int i = 0; i < n; i++)
        {
            var r = OTaiVung(b.vungO, i, s);
            bool tron = CamUng.DangDung;
            int ky = bo[i];

            // Nen o. O TRON thi nen cung phai TRON: truoc day dung GiaoDien.Trang
            // (mot anh vuong) nen sau moi o tron lo ra mot o vuong xam.
            var mauCu = GUI.color;
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
                var c = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, dangNhac ? 0.25f : 1f);
                float co = tron ? r.width * 0.86f : r.width - 8f * s;
                GUI.DrawTexture(new Rect(r.center.x - co * 0.5f, r.center.y - co * 0.5f, co, co),
                                icon[ky], ScaleMode.StretchToFill, true);
                GUI.color = c;
            }

            VienO(r, tron, new Color(0.55f, 0.42f, 0.28f, 0.95f), s);

            // So thu tu chi ve o BAN MAY TINH - o day no chinh la phim tat.
            // Ban cam ung khong co phim tat, ma cac o tron nam sat nhau nen con
            // so lai de len o ben canh.
            if (!tron)
            {
                var kSo = new GUIStyle(GiaoDien.KieuChuNho);
                kSo.alignment = TextAnchor.MiddleCenter;
                kSo.fontSize = Mathf.RoundToInt(13f * s);
                kSo.normal.textColor = GiaoDien.MauVang;
                GUI.Label(new Rect(r.x, r.yMax + 1f * s, r.width, 18f * s), (i + 1).ToString(), kSo);
            }
        }
    }

    /// <summary>Vien quanh mot o - tron thi ve bang bon cung gia, vuong thi bon canh.</summary>
    static void VienO(Rect r, bool tron, Color mau, float s)
    {
        float d = Mathf.Max(1f, 2f * s);
        if (!tron)
        {
            GiaoDien.To(new Rect(r.x, r.y, r.width, d), mau);
            GiaoDien.To(new Rect(r.x, r.yMax - d, r.width, d), mau);
            GiaoDien.To(new Rect(r.x, r.y, d, r.height), mau);
            GiaoDien.To(new Rect(r.xMax - d, r.y, d, r.height), mau);
            return;
        }

        // Vong tron: ve mot vanh bang cach chong hai hinh tron (Trang la anh
        // tron mo dan - dung no lam vanh thi vien nhoe, nen ve bang 28 doan nho)
        const int SoDoan = 28;
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
