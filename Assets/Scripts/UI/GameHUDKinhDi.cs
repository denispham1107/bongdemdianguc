using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUD TRONG TRAN KIEU KINH DI: bang mau / mana, khung dot quai, cac dong
/// thong bao - cung phong cach voi sanh (khung da toi, moc sat do o goc, chu
/// Inter co dau tieng Viet).
///
/// Truoc 12/09/2026 cac thu nay ve bang font MAC DINH cua Unity, chu KHONG dau
/// ("PHU THUY", "DOT 1 - Quai con lai", "Da diet") va thong bao mang cua
/// KhoiDongTranMang ve DE LEN khung dot quai (hai file tu chon cho, khong ai
/// biet ai). Gio:
///   - Moi chu di qua font Inter (GiaoDien.ChuThuong / ChuDam).
///   - MOT ham bo cuc thuan (<see cref="TinhBoCuc"/>) xep moi khung: khung sau
///     vuong khung truoc thi tu day xuong. Ham thuan de phep thu (menu 53) goi
///     voi hang chuc co man hinh ma khong can vao Play.
///   - Thong bao mang (KhoiDongTranMang.TrangThai / ThongBaoKetNoi) do HUD ve,
///     theo cung bo cuc.
/// </summary>
public partial class GameHUD
{
    /// <summary>HUD dang song trong canh - KhoiDongTranMang nhuong viec ve thong bao cho no.</summary>
    public static GameHUD Ban { get; private set; }

    void OnEnable() { Ban = this; }
    void OnDisable() { if (Ban == this) Ban = null; }

    // ================================================================
    //  KICH THUOC (don vi o man hinh cao 1080)
    // ================================================================

    /// <summary>Be rong bang trang thai goc tren trai. Menu 40 do con so mau co lot trong thanh.</summary>
    public const float BangRong = 344f;
    const float BangLeTrai = 16f, BangLeTren = 16f;   // vi tri khung (chua cho moc sat nho ra 3 don vi)
    const float BangLe = 14f;                          // le trong khung
    const float BangCaoTen = 28f;
    const float BangCaoMau = 28f;
    const float BangCaoKhieng = 8f;
    const float BangCaoMana = 20f;
    const float BangKhe = 6f;
    public const float CoChuMau = 16f, CoChuMana = 13f, CoChuTen = 19f;

    /// <summary>Be rong THAT cua thanh mau / mana (trong khung, tru le hai ben).</summary>
    public static float RongThanh(float s) { return (BangRong - 2f * BangLe) * s; }

    public static float CaoBangTrangThai(float s)
    {
        return (BangLe * 2f + BangCaoTen + BangCaoMau + BangKhe + BangCaoKhieng + BangKhe + BangCaoMana) * s;
    }

    /// <summary>Day bang trang thai nam o dau tren man hinh.</summary>
    public static float DayBangTrangThai(float s) { return BangLeTren * s + CaoBangTrangThai(s); }

    // ================================================================
    //  KIEU CHU - tao bang new GUIStyle() (khong can GUI.skin) nen dung duoc
    //  ca ngoai OnGUI: phep thu do chu bang CUNG kieu voi luc ve
    // ================================================================

    public class KieuHUD
    {
        public GUIStyle ten, soThanh, dot, dong2, mang, bao, ketNoi, tieuDeChet, tomTatChet, nut;

        public KieuHUD(Font dam, Font thuong)
        {
            ten = Moi(dam, TextAnchor.MiddleLeft, false);
            soThanh = Moi(dam, TextAnchor.MiddleCenter, false);
            dot = Moi(dam, TextAnchor.MiddleCenter, false);
            dong2 = Moi(thuong, TextAnchor.MiddleCenter, false);
            mang = Moi(thuong, TextAnchor.MiddleCenter, true);
            bao = Moi(dam, TextAnchor.MiddleCenter, true);
            ketNoi = Moi(dam, TextAnchor.MiddleCenter, true);
            tieuDeChet = Moi(dam, TextAnchor.MiddleCenter, false);
            tomTatChet = Moi(thuong, TextAnchor.MiddleCenter, true);
            nut = Moi(dam, TextAnchor.MiddleCenter, false);
        }

        static GUIStyle Moi(Font f, TextAnchor canh, bool xuongDong)
        {
            var k = new GUIStyle();
            k.font = f;
            k.alignment = canh;
            k.wordWrap = xuongDong;
            k.clipping = TextClipping.Overflow;
            k.padding = new RectOffset(0, 0, 0, 0);
            k.normal.textColor = Color.white;
            return k;
        }

        float daDat = -1f;

        /// <summary>Dat co chu theo ti le man hinh (chi lam lai khi ti le doi).</summary>
        public void DatCo(float s)
        {
            if (Mathf.Approximately(s, daDat)) return;
            daDat = s;
            ten.fontSize = Co(CoChuTen, s);
            soThanh.fontSize = Co(CoChuMau, s);
            dot.fontSize = Co(27f, s);
            dong2.fontSize = Co(17f, s);
            mang.fontSize = Co(18f, s);
            bao.fontSize = Co(21f, s);
            ketNoi.fontSize = Co(22f, s);
            tieuDeChet.fontSize = Co(64f, s);
            tomTatChet.fontSize = Co(21f, s);
            nut.fontSize = Co(28f, s);
        }

        public static int Co(float coChu, float s) { return Mathf.Max(10, Mathf.RoundToInt(coChu * s)); }
    }

    // ================================================================
    //  DO CHU + BO CUC (thuan - khong doc Screen, khong ve gi)
    // ================================================================

    /// <summary>Kich thuoc cac khung chu, do bang kieu chu that.</summary>
    public struct CoChuHUD
    {
        public Vector2 thongTin;       // ca khung dot quai (da cong le)
        public float caoDong1, caoDong2;
        public Vector2 mang;           // khung thong bao mang (da cong le), (0,0) neu khong co
        public Vector2 ketNoi;         // khung bao mat ket noi
        public Vector2 baoGiua;        // dong bao ngan (F9, khoa goc nhin)
        public Vector2 baoNhanVat;     // "Khong du nang luong!"...
        public float rongSoMau, rongSoMana;
    }

    public struct BoCucHUD
    {
        public Rect bangTrangThai, thongTin, mang, ketNoi, baoGiua, baoNhanVat;
        public Rect[] tatCa;           // nhung khung dang hien - de kiem de nhau
        public string[] tenKhung;
        public bool[] laDieuKhien;     // nut / can / thanh ky nang (khong phai khung chu)
    }

    static Vector2 KhungChu(GUIStyle k, string chu, float rongToiDa, float leNgang, float leDoc)
    {
        if (string.IsNullOrEmpty(chu)) return Vector2.zero;
        var nd = new GUIContent(chu);
        float rong = k.CalcSize(nd).x;
        float rongChu = Mathf.Min(rong, rongToiDa - 2f * leNgang);
        float cao = k.wordWrap ? k.CalcHeight(nd, rongChu) : k.CalcSize(nd).y;
        return new Vector2(rongChu + 2f * leNgang, cao + 2f * leDoc);
    }

    public static CoChuHUD DoCoChu(KieuHUD k, float W, float s, string dong1, string dong2,
                                   string mang, string ketNoi, string baoGiua, string baoNhanVat,
                                   string soMau, string soMana)
    {
        var c = new CoChuHUD();
        float rongToiDa = W - 32f * s;

        var n1 = new GUIContent(dong1 ?? ""); var n2 = new GUIContent(dong2 ?? "");
        c.caoDong1 = k.dot.CalcSize(n1).y;
        c.caoDong2 = k.dong2.CalcSize(n2).y;
        float rong = Mathf.Max(k.dot.CalcSize(n1).x, k.dong2.CalcSize(n2).x) + 2f * 30f * s;
        c.thongTin = new Vector2(Mathf.Min(rongToiDa, Mathf.Max(260f * s, rong)),
                                 14f * s + c.caoDong1 + 4f * s + c.caoDong2 + 14f * s);

        c.mang = KhungChu(k.mang, mang, Mathf.Min(rongToiDa, 780f * s), 22f * s, 12f * s);
        c.ketNoi = KhungChu(k.ketNoi, ketNoi, Mathf.Min(rongToiDa, 900f * s), 28f * s, 18f * s);
        c.baoGiua = KhungChu(k.bao, baoGiua, Mathf.Min(rongToiDa, 800f * s), 18f * s, 8f * s);
        c.baoNhanVat = KhungChu(k.bao, baoNhanVat, Mathf.Min(rongToiDa, 800f * s), 18f * s, 8f * s);

        c.rongSoMau = string.IsNullOrEmpty(soMau) ? 0f : k.soThanh.CalcSize(new GUIContent(soMau)).x;
        var coCu = k.soThanh.fontSize;
        k.soThanh.fontSize = KieuHUD.Co(CoChuMana, s);
        c.rongSoMana = string.IsNullOrEmpty(soMana) ? 0f : k.soThanh.CalcSize(new GUIContent(soMana)).x;
        k.soThanh.fontSize = coCu;
        return c;
    }

    /// <summary>
    /// Xep moi khung. Thu tu uu tien: bang trang thai (goc trai tren, co dinh),
    /// khung dot quai (giua mep tren), thong bao mang ngay duoi, bao mat ket noi,
    /// dong bao ngan, dong bao cua nhan vat (0,62 chieu cao). Khung nao vuong
    /// khung truoc (hoac cot nut cam ung goc phai) thi DAY XUONG ngay duoi cai
    /// no vuong.
    /// </summary>
    public static BoCucHUD TinhBoCuc(float W, float H, float s, CoChuHUD c, bool camUng)
    {
        var b = new BoCucHUD();
        float khe = 10f * s;
        var daXep = new List<Rect>();
        var ten = new List<string>();

        b.bangTrangThai = new Rect(BangLeTrai * s, BangLeTren * s, BangRong * s, CaoBangTrangThai(s));
        daXep.Add(b.bangTrangThai); ten.Add("bang mau/mana");

        if (camUng)
        {
            // Cot nut doi goc nhin + khoa camera (tam cach mep 62 don vi, ban kinh 40, cach nhau 88)
            daXep.Add(new Rect(W - 106f * s, 18f * s, 88f * s, 176f * s));
            ten.Add("nut cam ung goc phai");
        }

        float y = 12f * s;
        b.thongTin = XepGiua(c.thongTin, W, H, y, daXep, khe, s);
        if (c.thongTin.x > 0f) { daXep.Add(b.thongTin); ten.Add("khung dot quai"); y = b.thongTin.yMax + khe; }

        b.mang = XepGiua(c.mang, W, H, y, daXep, khe, s);
        if (c.mang.x > 0f) { daXep.Add(b.mang); ten.Add("thong bao mang"); y = b.mang.yMax + khe; }

        b.ketNoi = XepGiua(c.ketNoi, W, H, Mathf.Max(H * 0.26f, y), daXep, khe, s);
        if (c.ketNoi.x > 0f) { daXep.Add(b.ketNoi); ten.Add("bao ket noi"); y = b.ketNoi.yMax + khe; }

        b.baoGiua = XepGiua(c.baoGiua, W, H, Mathf.Max(H * 0.22f, y), daXep, khe, s);
        if (c.baoGiua.x > 0f) { daXep.Add(b.baoGiua); ten.Add("bao ngan"); }

        // Vung dieu khien o mep duoi: dong bao cua nhan vat vuong thi DAY LEN
        if (camUng)
        {
            // Cum nut ky nang hai cung quanh goc phai duoi (cung ngoai 392 + ban kinh nut 61)
            daXep.Add(new Rect(W - 500f * s, H - 500f * s, 500f * s, 500f * s)); ten.Add("cum nut ky nang");
            // Can joystick: tam (232,5 ; 232,5) tu goc trai duoi, ban kinh 172,5
            daXep.Add(new Rect(60f * s, H - 405f * s, 345f * s, 345f * s)); ten.Add("can joystick");
        }
        else
        {
            // Thanh ky nang vuong: 7 o 84 cach 12, cach day 22, chu phim tat ben duoi
            daXep.Add(new Rect((W - 660f * s) * 0.5f, H - 106f * s, 660f * s, 106f * s)); ten.Add("thanh ky nang");
        }

        b.baoNhanVat = XepGiua(c.baoNhanVat, W, H, H * 0.62f - c.baoNhanVat.y * 0.5f, daXep, khe, s, true);

        // KHONG CON CHO NAO (dien thoai dung doc, cam ung, bon thong bao cung luc:
        // cum nut chiem het be ngang phia duoi, khoang trong con ~60 diem ma dong
        // bao cao 97 - menu 53): hai dong bao NGAN HAN dung chung mot cho - bao
        // cua nhan vat ("Khong du nang luong!") the cho dong bao ngan. Khong bao gio
        // de chu de len nhau.
        if (c.baoNhanVat.x > 0f && c.baoGiua.x > 0f && !HopLe(b.baoNhanVat, daXep, khe, H, s))
        {
            int iBao = ten.IndexOf("bao ngan");
            if (iBao >= 0) { daXep.RemoveAt(iBao); ten.RemoveAt(iBao); }
            b.baoNhanVat = XepGiua(c.baoNhanVat, W, H, b.baoGiua.y, daXep, khe, s);
            b.baoGiua = new Rect(0, 0, 0, 0);
        }
        if (c.baoNhanVat.x > 0f) { daXep.Add(b.baoNhanVat); ten.Add("bao nhan vat"); }

        b.tatCa = daXep.ToArray();
        b.tenKhung = ten.ToArray();
        b.laDieuKhien = new bool[b.tenKhung.Length];
        for (int i = 0; i < b.tenKhung.Length; i++)
            b.laDieuKhien[i] = b.tenKhung[i].StartsWith("nut") || b.tenKhung[i].StartsWith("cum")
                               || b.tenKhung[i].StartsWith("can") || b.tenKhung[i].StartsWith("thanh");
        return b;
    }

    static Rect XepGiua(Vector2 co, float W, float H, float y, List<Rect> daXep, float khe, float s, bool dayLen = false)
    {
        if (co.x <= 0f) return new Rect(0, 0, 0, 0);
        var r = new Rect((W - co.x) * 0.5f, y, co.x, co.y);
        float yMuon = y;
        // Day xuong (hoac len) den khi khong vuong ai (toi da 12 lan - moi lan qua mot khung)
        for (int lan = 0; lan < 12; lan++)
        {
            bool vuong = false;
            foreach (var o in daXep)
            {
                var noRong = new Rect(o.x - khe * 0.5f, o.y - khe * 0.5f, o.width + khe, o.height + khe);
                if (!noRong.Overlaps(r)) continue;
                if (dayLen) r.y = o.y - khe - r.height; else r.y = o.yMax + khe;
                vuong = true;
            }
            if (!vuong) break;
        }
        r.x = Mathf.Clamp(r.x, 8f * s, Mathf.Max(8f * s, W - r.width - 8f * s));

        // Day het cho ma van ra ngoai man hinh (dien thoai dung doc: cum nut ky
        // nang chiem het be ngang phia duoi, day len lai vuong chong thong bao
        // phia tren, menu 53 do: y = -78) -> do tim KHOANG TRONG gan cho muon nhat,
        // len va xuong xen ke
        if (r.yMin < 4f * s || r.yMax > H - 4f * s || VuongAi(r, daXep, khe))
        {
            float buoc = Mathf.Max(2f, 4f * s);
            for (float d = 0f; d < H; d += buoc)
            {
                foreach (float dau in new[] { 1f, -1f })
                {
                    float yy = yMuon + dau * d;
                    if (yy < 4f * s || yy + r.height > H - 4f * s) continue;
                    var thu = new Rect(r.x, yy, r.width, r.height);
                    if (!VuongAi(thu, daXep, khe)) return thu;
                }
            }
        }
        return r;
    }

    static bool HopLe(Rect r, List<Rect> daXep, float khe, float H, float s)
    {
        return r.yMin >= 4f * s && r.yMax <= H - 4f * s && !VuongAi(r, daXep, khe);
    }

    static bool VuongAi(Rect r, List<Rect> daXep, float khe)
    {
        foreach (var o in daXep)
            if (new Rect(o.x - khe * 0.5f, o.y - khe * 0.5f, o.width + khe, o.height + khe).Overlaps(r)) return true;
        return false;
    }

    // ================================================================
    //  VE
    // ================================================================

    KieuHUD kieu;
    /// <summary>Bo cuc cua lan ve gan nhat - phep thu (menu 53) doc lai.</summary>
    public BoCucHUD BoCucCuoi { get; private set; }
    public KieuHUD Kieu { get { return kieu; } }

    string TenHienThi
    {
        get
        {
            // Choi mang: ten nguoi choi (co the go dau roi - ghep lai); choi don: ten lop nhan vat
            if (TranHienTai.DangChoiMang && !string.IsNullOrEmpty(FirebaseMang.TenHienThi))
                return GhepDauTiengViet.Ghep(FirebaseMang.TenHienThi);
            return "PHÙ THỦY";
        }
    }

    /// <summary>Ve bang trang thai + khung dot quai + moi dong thong bao - goi tu OnGUI.</summary>
    void VeHUDKinhDi(float s)
    {
        GiaoDien.ChuanBi();                                   // anh khung, moc sat
        if (kieu == null) kieu = new KieuHUD(GiaoDien.ChuDam, GiaoDien.ChuThuong);
        kieu.DatCo(s);

        // ---- Noi dung ----
        string dong1 = "", dong2 = "";
        if (director != null)
        {
            dong1 = "ĐỢT " + Mathf.Max(1, director.Wave);
            dong2 = director.NextWaveIn > 0.01f && director.Alive == 0
                ? "Đợt mới sau " + Mathf.CeilToInt(director.NextWaveIn) + " giây…"
                : "Quái còn lại: " + director.Alive + "   ·   Đã diệt: " + director.Kills;
        }
        string mang = TrangThaiMangHienTai();
        string ketNoi = KhoiDongTranMang.ThongBaoKetNoi;
        float tuoiBao = Time.time - baoLuc;
        string baoGiua = !string.IsNullOrEmpty(baoChu) && tuoiBao < 2f ? baoChu : "";
        string baoNV = player != null && Time.time - player.LastMessageTime < 1.6f ? player.LastMessage : "";
        if (director != null && director.PlayerDead) baoNV = "";   // man hinh thua che het
        string soMau = playerHealth != null
            ? Mathf.CeilToInt(playerHealth.health) + " / " + Mathf.CeilToInt(playerHealth.maxHealth) : "";
        string soMana = player != null ? Mathf.CeilToInt(player.mana) + " / " + Mathf.CeilToInt(player.maxMana) : "";

        var c = DoCoChu(kieu, Screen.width, s, dong1, dong2, mang, ketNoi, baoGiua, baoNV, soMau, soMana);
        if (director == null) c.thongTin = Vector2.zero;
        var b = TinhBoCuc(Screen.width, Screen.height, s, c, CamUng.DangDung);
        BoCucCuoi = b;

        VeBangTrangThaiKinhDi(b.bangTrangThai, s, soMau, soMana);
        if (director != null) VeKhungDot(b.thongTin, s, dong1, dong2, c);
        if (c.mang.x > 0f) VeThongBaoMang(b.mang, s, mang);
        if (c.ketNoi.x > 0f) VeBaoKetNoi(b.ketNoi, s, ketNoi);
        if (b.baoGiua.width > 0f)             // co the da nhuong cho cho bao nhan vat
            VeChuVien(b.baoGiua, baoGiua, kieu.bao, new Color(1f, 0.90f, 0.62f, Mathf.Clamp01(2f - tuoiBao)), s);
        if (b.baoNhanVat.width > 0f)
            VeChuVien(b.baoNhanVat, baoNV, kieu.bao, new Color(1f, 0.52f, 0.36f), s);
    }

    /// <summary>Dong trang thai noi mang (dang tim nguoi choi, da noi...) - rong neu khong co.</summary>
    static string TrangThaiMangHienTai() { return KhoiDongTranMang.TrangThai ?? ""; }

    // ---------------- Bang mau / mana ----------------

    void VeBangTrangThaiKinhDi(Rect r, float s, string soMau, string soMana)
    {
        GiaoDien.Khung(r, s, false, 0.62f);

        float le = BangLe * s;
        float x = r.x + le, rong = RongThanh(s);
        float y = r.y + le;

        // Ten + duong ke do mo hai dau
        var kTen = kieu.ten;
        VeChuVien(new Rect(x, y, rong, BangCaoTen * s), TenHienThi, kTen, GiaoDien.MauGiay, s);
        GiaoDien.DuongKe(new Rect(x, y + BangCaoTen * s - 2f * s, rong, Mathf.Max(1f, 1.5f * s)),
                         new Color(0.75f, 0.12f, 0.08f, 0.85f));
        y += BangCaoTen * s + 4f * s;

        // Mau: dap do theo nhip tim khi duoi 30%
        float mau01 = playerHealth != null ? playerHealth.Health01 : 0f;
        float nhip = 0f;
        if (mau01 < 0.3f && mau01 > 0f)
        {
            float t = Mathf.Repeat(Time.time, 1.1f) / 1.1f;
            nhip = Mathf.Max(Mathf.Exp(-Mathf.Pow((t - 0.10f) * 14f, 2f)), 0.7f * Mathf.Exp(-Mathf.Pow((t - 0.32f) * 14f, 2f)));
        }
        VeThanhKinhDi(new Rect(x, y, rong, BangCaoMau * s), mau01, gradMau, soMau, CoChuMau, s, nhip, 0.12f);
        y += BangCaoMau * s + BangKhe * s;

        // Khien: CHI hien khi dang co - khong co thi de trong dai nay (mana khong nhay len xuong)
        float khieng01 = player != null ? player.KhiengMau01 : 0f;
        if (khieng01 > 0.001f)
            VeThanhKinhDi(new Rect(x, y, rong, BangCaoKhieng * s), khieng01,
                          khieng01 > 0.35f ? gradKhieng : gradKhiengYeu, "", 0f, s, 0f, 0f);
        y += BangCaoKhieng * s + BangKhe * s;

        float mana01 = player != null ? player.Mana01 : 0f;
        VeThanhKinhDi(new Rect(x, y, rong, BangCaoMana * s), mana01, gradMana, soMana, CoChuMana, s, 0f, 0.09f);
    }

    /// <summary>
    /// Mot thanh chi so kieu ONG CHAT LONG: ranh sat toi, chat long mau/linh hon
    /// chuyen doc, vach chia 10%, mot vet sang troi ngang nhu chat long sanh,
    /// vien sat + chi do. <paramref name="nhip"/> &gt; 0 thi vien bung do (mau thap).
    /// </summary>
    void VeThanhKinhDi(Rect r, float tiLe, Texture2D grad, string chu, float coChu, float s, float nhip, float sangTroi)
    {
        tiLe = Mathf.Clamp01(tiLe);
        var cu = GUI.color;
        float v = Mathf.Max(1f, Mathf.Round(1.5f * s));

        // Bong do lech duoi phai
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(r.x + 2f * s, r.y + 3f * s, r.width, r.height), barTex);
        // Ranh sat toi, chim: canh tren den, canh duoi hoi sang
        GUI.color = new Color(0.035f, 0.02f, 0.02f, 0.95f);
        GUI.DrawTexture(r, barTex);
        GUI.color = new Color(0f, 0f, 0f, 0.9f);
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, v * 1.5f), barTex);

        var trong = new Rect(r.x + v, r.y + v, (r.width - 2f * v) * tiLe, r.height - 2f * v);
        if (tiLe > 0.001f)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(trong, grad, ScaleMode.StretchToFill, true);

            // Mat chat long: dai sang mong tren cung
            GUI.color = new Color(1f, 1f, 1f, 0.16f);
            GUI.DrawTexture(new Rect(trong.x, trong.y, trong.width, trong.height * 0.28f), barTex);

            // Vet sang troi ngang (chat long sanh) - chu ky 3,2 giay, chi trong phan day
            if (sangTroi > 0f)
            {
                float rongVet = 38f * s;
                float p = Mathf.Repeat(Time.time / 3.2f, 1f);
                float xv = trong.x - rongVet + (trong.width + rongVet) * p;
                float x0 = Mathf.Max(trong.x, xv), x1 = Mathf.Min(trong.xMax, xv + rongVet);
                if (x1 > x0)
                {
                    GUI.color = new Color(1f, 0.85f, 0.8f, sangTroi);
                    GUI.DrawTexture(new Rect(x0, trong.y, x1 - x0, trong.height), barTex);
                }
            }

            // Mep mat chat long dang dang / rut
            GUI.color = new Color(1f, 0.9f, 0.85f, 0.5f);
            GUI.DrawTexture(new Rect(trong.xMax - v, trong.y, v, trong.height), barTex);
        }

        // Vach chia 10% - nhin nhu ong do tren binh mau
        if (r.height > 12f * s)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.38f);
            for (int i = 1; i < 10; i++)
            {
                float xx = r.x + v + (r.width - 2f * v) * i / 10f;
                float cao = i == 5 ? r.height - 2f * v : (r.height - 2f * v) * 0.45f;
                GUI.DrawTexture(new Rect(xx, r.yMax - v - cao, Mathf.Max(1f, s), cao), barTex);
            }
        }

        // Vien: sat toi ngoai, chi do trong; mau thap thi chi do bung sang theo nhip tim
        GUI.color = cu;
        DrawBorder(r, new Color(0.10f, 0.05f, 0.04f, 1f), v);
        var chiDo = Color.Lerp(new Color(0.42f, 0.08f, 0.06f, 0.9f), new Color(1f, 0.18f, 0.10f, 1f), nhip);
        DrawBorder(new Rect(r.x + v, r.y + v, r.width - 2f * v, r.height - 2f * v), chiDo, Mathf.Max(1f, s));
        if (nhip > 0.01f)
        {
            GUI.color = new Color(1f, 0.1f, 0.05f, 0.35f * nhip);
            GUI.DrawTexture(new Rect(r.x - 3f * s, r.y - 3f * s, r.width + 6f * s, r.height + 6f * s), barTex);
            GUI.color = cu;
        }

        if (!string.IsNullOrEmpty(chu) && coChu > 0.5f)
        {
            var k = kieu.soThanh;
            int coCu = k.fontSize;
            k.fontSize = KieuHUD.Co(coChu, s);
            VeChuVien(r, chu, k, new Color(1f, 0.95f, 0.9f), s);
            k.fontSize = coCu;
        }
        GUI.color = cu;
    }

    // ---------------- Khung dot quai ----------------

    void VeKhungDot(Rect r, float s, string dong1, string dong2, CoChuHUD c)
    {
        GiaoDien.Khung(r, s, false, 0.62f);
        float y = r.y + 14f * s;
        VeChuVien(new Rect(r.x, y, r.width, c.caoDong1), dong1, kieu.dot, new Color(0.96f, 0.24f, 0.14f), s);
        y += c.caoDong1 + 4f * s;
        GiaoDien.DuongKe(new Rect(r.x + r.width * 0.2f, y - 3f * s, r.width * 0.6f, Mathf.Max(1f, 1.5f * s)),
                         new Color(0.75f, 0.12f, 0.08f, 0.8f));
        VeChuVien(new Rect(r.x, y, r.width, c.caoDong2), dong2, kieu.dong2, GiaoDien.MauGiay, s);
    }

    // ---------------- Thong bao mang ----------------

    void VeThongBaoMang(Rect r, float s, string chu)
    {
        GiaoDien.Khung(r, s, false, 0.70f);
        Color mau = KhoiDongTranMang.DaNoi ? new Color(0.50f, 0.90f, 0.45f)
                  : KhoiDongTranMang.LoiCuoi != null ? new Color(1f, 0.50f, 0.42f)
                  : new Color(0.98f, 0.86f, 0.55f);
        VeChuVien(new Rect(r.x + 22f * s, r.y, r.width - 44f * s, r.height), chu, kieu.mang, mau, s);
    }

    void VeBaoKetNoi(Rect r, float s, string chu)
    {
        GiaoDien.Khung(r, s, false, 0.80f);
        Color mau = KhoiDongTranMang.ThongBaoLaLoi ? new Color(1f, 0.46f, 0.40f) : new Color(0.98f, 0.86f, 0.48f);
        VeChuVien(new Rect(r.x + 28f * s, r.y, r.width - 56f * s, r.height), chu, kieu.ketNoi, mau, s);
    }

    /// <summary>Chu co vien toi bon phia - doc duoc tren nen lua sang, khong can nen.</summary>
    static void VeChuVien(Rect r, string chu, GUIStyle k, Color mau, float s)
    {
        if (string.IsNullOrEmpty(chu)) return;
        float v = Mathf.Max(1f, 1.5f * s);
        var mauCu = k.normal.textColor;
        k.normal.textColor = new Color(0f, 0f, 0f, 0.85f * mau.a);
        GUI.Label(new Rect(r.x - v, r.y, r.width, r.height), chu, k);
        GUI.Label(new Rect(r.x + v, r.y, r.width, r.height), chu, k);
        GUI.Label(new Rect(r.x, r.y - v, r.width, r.height), chu, k);
        GUI.Label(new Rect(r.x, r.y + v, r.width, r.height), chu, k);
        k.normal.textColor = mau;
        GUI.Label(r, chu, k);
        k.normal.textColor = mauCu;
    }

    // ---------------- Man hinh thua ----------------

    /// <summary>
    /// MAN KET TRAN (choi mang): ai song sot cuoi cung, va bang diem.
    ///
    /// Ve tu tren xuong theo chieu cao DO THAT cua tung dong (CalcHeight), roi
    /// moi dong day dong sau xuong - khong dat toa do co dinh: ten nguoi choi
    /// dai ngan khac nhau, va man hinh dien thoai dung doc thi mot bang cung
    /// nhac se tran ra ngoai.
    /// </summary>
    void VeManKetTran(float s)
    {
        var prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), barTex);
        GUI.color = prev;

        string tieuDe = KetTran.GheThang == 255 ? "KHÔNG AI SỐNG SÓT"
                      : KetTran.ToiThang ? "BẠN SỐNG SÓT CUỐI CÙNG"
                      : GhepDauTiengViet.Ghep(KetTran.TenNguoiThang).ToUpperInvariant() + " ĐÃ THẮNG";

        // Tieu de: co chu tu thu nho lai cho vua be ngang man hinh
        var kT = new GUIStyle(kieu.tieuDeChet);
        float leNgang = 30f * s;
        while (kT.fontSize > 20 && kT.CalcSize(new GUIContent(tieuDe)).x > Screen.width - leNgang * 2f)
            kT.fontSize -= 2;

        float nhay = 0.8f + 0.2f * Mathf.PerlinNoise(Time.unscaledTime * 2.4f, 0.11f);
        var mauTieuDe = KetTran.ToiThang
            ? Color.Lerp(new Color(0.85f, 0.62f, 0.18f), new Color(1f, 0.88f, 0.45f), nhay)
            : Color.Lerp(new Color(0.55f, 0.04f, 0.03f), new Color(0.96f, 0.16f, 0.10f), nhay);

        float caoT = kT.CalcSize(new GUIContent(tieuDe)).y;
        float y = Screen.height * 0.22f;
        var oT = new Rect(0f, y, Screen.width, caoT);
        VeChuVien(oT, tieuDe, kT, mauTieuDe, s);
        y = oT.yMax + 8f * s;

        GiaoDien.DuongKe(new Rect(Screen.width * 0.5f - 300f * s, y, 600f * s, Mathf.Max(1f, 2f * s)),
                         new Color(0.8f, 0.1f, 0.06f, 0.9f));
        y += 18f * s;

        // ---- Bang diem ----
        var kDong = kieu.tomTatChet;
        float caoDong = kDong.CalcHeight(new GUIContent("X"), 400f * s) + 6f * s;
        float rongBang = Mathf.Min(Screen.width - 40f * s, 760f * s);
        float xBang = (Screen.width - rongBang) * 0.5f;
        float cotTen = rongBang * 0.5f, cotQuai = rongBang * 0.28f;

        VeChuVien(new Rect(xBang, y, cotTen, caoDong), "NGƯỜI CHƠI", kDong, GiaoDien.MauMo, s);
        VeChuVien(new Rect(xBang + cotTen, y, cotQuai, caoDong), "Quái đã diệt", kDong, GiaoDien.MauMo, s);
        VeChuVien(new Rect(xBang + cotTen + cotQuai, y, rongBang - cotTen - cotQuai, caoDong),
                  "Đã hạ", kDong, GiaoDien.MauMo, s);
        y += caoDong + 2f * s;

        for (byte g = 0; g < GoiTin.SoGheToiDa; g++)
        {
            if (!KetTran.CoTrongTran[g]) continue;
            string ten = GhepDauTiengViet.Ghep(KetTran.TenTheoGhe[g] ?? ("người chơi " + (g + 1)));
            if (g == KetTran.GheCuaToi) ten += " (bạn)";
            var mau = g == KetTran.GheThang ? new Color(1f, 0.86f, 0.45f) : GiaoDien.MauGiay;

            VeChuVien(new Rect(xBang, y, cotTen, caoDong), ten, kDong, mau, s);
            VeChuVien(new Rect(xBang + cotTen, y, cotQuai, caoDong),
                      KetTran.BangDiem.quaiTheoGhe[g].ToString(), kDong, mau, s);
            VeChuVien(new Rect(xBang + cotTen + cotQuai, y, rongBang - cotTen - cotQuai, caoDong),
                      KetTran.BangDiem.nguoiTheoGhe[g].ToString(), kDong, mau, s);
            y += caoDong;
        }

        // ---- Ai da ha minh (neu minh khong phai nguoi thang) ----
        if (!KetTran.ToiThang)
        {
            string keHa = DongKeHa();
            float caoKH = kDong.CalcHeight(new GUIContent(keHa), rongBang);
            VeChuVien(new Rect(xBang, y + 12f * s, rongBang, caoKH), keHa, kDong,
                      new Color(1f, 0.72f, 0.62f), s);
            y += 12f * s + caoKH;
        }

        // ---- Loi nhac ve sanh ----
        if (!CamUng.DangDung)
        {
            string nhac = "Bấm ESC để về sảnh.";
            float rongN = Mathf.Min(Screen.width - 40f * s, 760f * s);
            float caoN = kieu.tomTatChet.CalcHeight(new GUIContent(nhac), rongN);
            VeChuVien(new Rect((Screen.width - rongN) * 0.5f, y + 16f * s, rongN, caoN), nhac,
                      kieu.tomTatChet, GiaoDien.MauGiay, s);
        }
    }

    /// <summary>
    /// TEN KE VUA HA NHAN VAT NAY - de man hinh thua noi ro "Bị ... hạ".
    ///
    /// Doc <see cref="Damageable.keDanhCuoi"/>: moi phep cua nguoi choi va moi
    /// don cua quai deu ghi vao do. null la khong ro (chet vi ngã, vi hieu ung
    /// con sot lai...).
    /// </summary>
    public static string TenKeHa(Damageable nanNhan)
    {
        if (nanNhan == null) return null;
        var ke = nanNhan.keDanhCuoi;
        if (ke == null) return null;
        if (ke == nanNhan) return "chính mình";

        if (ke.isPlayer || ke.GetComponent<PlayerController>() != null)
        {
            string ten = KetTran.TenNhanVat(ke);
            if (!string.IsNullOrEmpty(ten)) return GhepDauTiengViet.Ghep(ten);
            return "một người chơi khác";
        }

        var nd = ke.GetComponent<NhanDangQuai>();
        return nd != null ? TenQuai(nd.loai) : "quái vật";
    }

    /// <summary>Ten tieng Viet cua tung loai quai.</summary>
    public static string TenQuai(MonsterType loai)
    {
        switch (loai)
        {
            case MonsterType.Skeleton: return "Bộ xương";
            case MonsterType.Witch:    return "Mụ phù thủy";
            case MonsterType.QuyDu:    return "Quỷ dữ";
            case MonsterType.QuyCay:   return "Quỷ cây";
            case MonsterType.Fallen:   return "Quỷ lùn";
            case MonsterType.Ghoul:    return "Xác sống";
            case MonsterType.Brute:    return "Quỷ khổng lồ";
            default:                   return "quái vật";
        }
    }

    /// <summary>Dong "Bị ... hạ." cho man hinh thua / man ket tran.</summary>
    string DongKeHa()
    {
        var mau = player != null ? player.GetComponent<Damageable>() : null;
        string ke = TenKeHa(mau);
        return string.IsNullOrEmpty(ke) ? "Không rõ ai đã hạ bạn." : "Bị " + ke + " hạ.";
    }

    void VeManHinhThua(float s)
    {
        var prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), barTex);
        GUI.color = prev;

        float nhay = 0.8f + 0.2f * Mathf.PerlinNoise(Time.unscaledTime * 2.7f, 0.37f);
        string tieuDe = "BẠN ĐÃ GỤC NGÃ";
        var kT = kieu.tieuDeChet;
        float caoT = kT.CalcSize(new GUIContent(tieuDe)).y;
        var oT = new Rect(0f, Screen.height * 0.40f - caoT * 0.5f, Screen.width, caoT);
        // Quang do chap chon quanh chu
        kT.normal.textColor = new Color(0.95f, 0.08f, 0.04f, 0.10f * nhay);
        for (int i = 0; i < 8; i++)
        {
            float g = i * Mathf.PI / 4f;
            GUI.Label(new Rect(oT.x + Mathf.Cos(g) * 6f * s, oT.y + Mathf.Sin(g) * 6f * s, oT.width, oT.height), tieuDe, kT);
        }
        VeChuVien(oT, tieuDe, kT, Color.Lerp(new Color(0.55f, 0.04f, 0.03f), new Color(0.96f, 0.16f, 0.10f), nhay), s);
        GiaoDien.DuongKe(new Rect(Screen.width * 0.5f - 300f * s, oT.yMax + 6f * s, 600f * s, Mathf.Max(1f, 2f * s)),
                         new Color(0.8f, 0.1f, 0.06f, 0.9f));

        // May cam ung KHONG CO BAN PHIM: khong nhac "bam R / ESC" - co nut TRO VE
        string tomTat = "Đã diệt " + director.Kills + " quái ở đợt " + director.Wave + ".";
        // Choi mang: tran chua xong, minh ngoi xem nguoi khac danh not
        if (TranHienTai.DangChoiMang)
            tomTat = "Đã diệt " + director.Kills + " quái. Đang xem trận đấu — chờ người sống sót cuối cùng.";
        if (!CamUng.DangDung)
            tomTat += GameDirector.DuocChoiLai
                ? "  Bấm R để chơi lại, ESC để về màn hình chính."
                : "  Bấm ESC để về sảnh.";          // choi mang: khong co "choi lai"
        float rongTT = Mathf.Min(Screen.width - 40f * s, 900f * s);

        // AI DA HA MINH - dong rieng, ngay duoi tieu de: nguoi choi hoi dau
        // tien khi guc nga la "vua roi ai giet toi?"
        string keHa = DongKeHa();
        float caoKH = kieu.tomTatChet.CalcHeight(new GUIContent(keHa), rongTT);
        float yKH = oT.yMax + 22f * s;
        VeChuVien(new Rect((Screen.width - rongTT) * 0.5f, yKH, rongTT, caoKH), keHa,
                  kieu.tomTatChet, new Color(1f, 0.72f, 0.62f), s);

        float caoTT = kieu.tomTatChet.CalcHeight(new GUIContent(tomTat), rongTT);
        VeChuVien(new Rect((Screen.width - rongTT) * 0.5f, yKH + caoKH + 8f * s, rongTT, caoTT), tomTat,
                  kieu.tomTatChet, GiaoDien.MauGiay, s);
    }
}
