using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KET TRAN: NGUOI SONG SOT CUOI CUNG THANG (giai doan 2, buoc 6).
///
/// Truoc buoc nay, tran mang khong bao gio KET THUC: ai chet thi nam do, ai
/// song thi danh quai mai mai, va khong ai duoc ghi mot tran thang nao ca
/// (<c>HoSoMang.CongThanhTich</c> viet xong tu buoc 1 ma chua he co ai goi).
///
/// AI PHAN QUYET: CHU PHONG, mot minh.
/// De moi may tu ket luan thi hai man hinh co the bao hai nguoi thang khac
/// nhau - goi tin khong den cung luc, may nay thay doi phuong chet truoc khi
/// may kia thay minh chet. Chu phong dem, roi phat
/// <see cref="GoiTin.LoaiKetTran"/> kem CA BANG DIEM, nen moi may hien dung
/// mot ket qua.
///
/// AI CHET THI BIET: moi may la trong tai cua chinh nhan vat minh (quy uoc cu
/// tu buoc 5). Cai chet cua minh di theo co <c>daChet</c> trong goi trang thai
/// - nhung co ay khong noi AI HA. Nen may nan nhan gui them
/// <see cref="GoiTin.LoaiChet"/>: "toi chet, nguoi ha toi ngoi ghe kia", doc
/// tu <see cref="Damageable.keDanhCuoi"/>.
///
/// ROI TRAN CUNG LA RA KHOI TRAN: nguoi dong tab khong con la "nguoi song
/// sot". Khong tinh the thi hai nguoi danh nhau, mot nguoi thoat, nguoi con
/// lai dung giua nghia dia doi mot ket qua khong bao gio den.
///
/// CHOI MOT MINH KHONG DUNG DEN LOP NAY - luat thang thua cu cua
/// <see cref="GameDirector"/> giu nguyen.
/// </summary>
public class KetTran : MonoBehaviour
{
    /// <summary>Gui lai goi ket tran may lan - mat goi la nguoi ta ngoi mai.</summary>
    public const int SoLanGuiLai = 6;
    public const float CachNhauGuiLai = 0.4f;

    /// <summary>Bao lau moi dem lai mot lan. Dem moi khung hinh la thua.</summary>
    public const float NhipDem = 0.25f;

    public static KetTran Hien { get; private set; }

    // ---- Trang thai de HUD doc ----
    /// <summary>Van dau da xong chua.</summary>
    public static bool DaXong;
    /// <summary>Ghe thang - 255 la khong ai song sot.</summary>
    public static byte GheThang = 255;
    /// <summary>Minh co thang khong.</summary>
    public static bool ToiThang;
    /// <summary>Ten nguoi thang, da co dau.</summary>
    public static string TenNguoiThang = "";
    /// <summary>Bang diem cuoi tran, theo ghe.</summary>
    public static GoiTin.KetQua BangDiem;
    /// <summary>Ten tung ghe (da co dau) - HUD ve bang diem doc o day.</summary>
    public static string[] TenTheoGhe = new string[GoiTin.SoGheToiDa];
    /// <summary>Ghe nao co mat trong tran nay.</summary>
    public static bool[] CoTrongTran = new bool[GoiTin.SoGheToiDa];
    /// <summary>Ghe cua may nay - de HUD to dam dong cua minh.</summary>
    public static byte GheCuaToi = 255;

    // ---- Tran DOI (28/09/2026) ----
    /// <summary>Doi thang (tran Doi): 0 = A, 1 = B, -1 = khong doi nao song / tran Don.</summary>
    public static int DoiThang = -1;
    /// <summary>Doi cua may nay trong tran Doi (-1 = tran Don).</summary>
    public static int DoiCuaToi = -1;
    /// <summary>Doi cua tung ghe - HUD ve bang diem theo doi.</summary>
    public static sbyte[] DoiTheoGhe = new sbyte[GoiTin.SoGheToiDa];
    /// <summary>So nguoi / so nguoi CON SONG moi doi (chi so 0 = A, 1 = B) - HUD ve o dau man, moi may tu dem.</summary>
    public static int[] SoNguoiDoi = new int[2], SoSongDoi = new int[2];

    /// <summary>
    /// Tran Doi: doi du nguoi trong phong vao tran bao lau (giay) truoc khi tinh ai chua vao la "da ra". Truoc moc nay mot doi
    /// chua ai kip tai xong man KHONG bi coi la chet het - khong thi doi kia "thang" ngay giay dau tien.
    /// </summary>
    public const float GiayChoCaPhongVao = 45f;

    public DongBoTran dongBo;
    public PlayerController toi;
    public byte gheToi = 255;

    /// <summary>Ten theo ghe - de ve bang diem. KhoiDongTranMang gan.</summary>
    public System.Func<byte, string> TenCuaGhe;
    /// <summary>Doi theo ghe (tran Doi) - KhoiDongTranMang gan. null / -1 = tran Don.</summary>
    public System.Func<byte, sbyte> DoiCuaGhe;
    /// <summary>Moi ghe co trong PHONG luc vao tran (ke ca nguoi chua tai xong man) - tran Doi can biet ai con dang toi.</summary>
    public List<byte> GheCoTrongPhong;
    float lucGan;

    readonly HashSet<byte> daChet = new HashSet<byte>();
    readonly byte[] quaiTheoGhe = new byte[GoiTin.SoGheToiDa];
    readonly byte[] nguoiTheoGhe = new byte[GoiTin.SoGheToiDa];

    bool daBaoToiChet;
    float demLanSau;

    void Awake()
    {
        Hien = this;
        Xoa();
    }

    void OnDestroy() { if (Hien == this) Hien = null; }

    /// <summary>Ve sanh / vao tran moi thi phai quen het - khong thi tran sau
    /// vua vao da hien man ket qua cua tran truoc.</summary>
    public static void Xoa()
    {
        DaXong = false;
        GheThang = 255;
        ToiThang = false;
        TenNguoiThang = "";
        BangDiem = new GoiTin.KetQua
        {
            gheThang = 255,
            quaiTheoGhe = new byte[GoiTin.SoGheToiDa],
            nguoiTheoGhe = new byte[GoiTin.SoGheToiDa]
        };
        TenTheoGhe = new string[GoiTin.SoGheToiDa];
        CoTrongTran = new bool[GoiTin.SoGheToiDa];
        GheCuaToi = 255;
        DoiThang = -1;
        DoiCuaToi = -1;
        DoiTheoGhe = new sbyte[GoiTin.SoGheToiDa];
        for (int i = 0; i < DoiTheoGhe.Length; i++) DoiTheoGhe[i] = CheDoTran.KhongDoi;
        SoNguoiDoi = new int[2]; SoSongDoi = new int[2];
    }

    sbyte DoiCua(byte ghe) { return DoiCuaGhe != null ? DoiCuaGhe(ghe) : CheDoTran.KhongDoi; }

    public void Gan(DongBoTran db, PlayerController nhanVat, byte ghe)
    {
        dongBo = db; toi = nhanVat; gheToi = ghe;
        GheCuaToi = ghe;
        lucGan = Time.unscaledTime;
        DoiCuaToi = CheDoTran.LaTranDoi ? DoiCua(ghe) : -1;
        if (GheCoTrongPhong != null)
            foreach (var g in GheCoTrongPhong) if (g < DoiTheoGhe.Length) DoiTheoGhe[g] = CheDoTran.LaTranDoi ? DoiCua(g) : CheDoTran.KhongDoi;
        if (dongBo != null)
        {
            dongBo.KhiNgheChet += NgheChet;
            dongBo.KhiNgheKetTran += NgheKetTran;
            dongBo.KhiNgheKinhNghiem += NgheKinhNghiem;
        }
    }

    void OnDisable()
    {
        if (dongBo == null) return;
        dongBo.KhiNgheChet -= NgheChet;
        dongBo.KhiNgheKetTran -= NgheKetTran;
        dongBo.KhiNgheKinhNghiem -= NgheKinhNghiem;
    }

    // ================================================================
    //  GHI CONG
    // ================================================================

    /// <summary>
    /// Mot con quai vua chet - cong cho nguoi ha no.
    ///
    /// Chi chay o may CHU PHONG: quai la cua chu phong, may khach chi ve lai
    /// nhung con nghe duoc. Nen bang diem quai cung do chu phong giu, va no di
    /// kem goi ket tran sang may khach.
    /// </summary>
    public void GhiQuaiChet(Damageable keDanh)
    {
        byte ghe = GheCuaDamageable(keDanh);
        if (ghe < quaiTheoGhe.Length && quaiTheoGhe[ghe] < 255) quaiTheoGhe[ghe]++;
    }

    /// <summary>
    /// Ten nguoi choi cua mot nhan vat - null neu do khong phai nguoi choi nao
    /// trong tran (quai, hoac dang choi mot minh).
    /// </summary>
    public static string TenNhanVat(Damageable d)
    {
        if (Hien == null || d == null) return null;
        byte ghe = Hien.GheCuaDamageable(d);
        if (ghe == 255 || Hien.TenCuaGhe == null) return null;
        return Hien.TenCuaGhe(ghe);
    }

    /// <summary>Ghe cua mot Damageable - 255 neu khong phai nhan vat nguoi choi nao.</summary>
    public byte GheCuaDamageable(Damageable d)
    {
        if (d == null || dongBo == null) return 255;
        return dongBo.ChiSoCua(d.transform);
    }

    // ================================================================
    //  CHET
    // ================================================================

    void NgheChet(byte ghe, byte gheKeHa)
    {
        bool moi = !daChet.Contains(ghe);
        ThemChet(ghe, gheKeHa);

        // HA MOT NGUOI CHOI thi duoc kinh nghiem. Goi chet do chinh may NAN
        // NHAN gui (no la trong tai cai chet cua minh) va duoc gui lai vai lan,
        // nen chi cong o lan dau nghe.
        if (moi && gheKeHa == gheToi && gheKeHa != ghe)
            CapDo.Them(CapDo.KnGietNguoi);
    }

    /// <summary>Chu phong bao: ghe nay vua ha mot con quai, duoc bay nhieu diem.</summary>
    void NgheKinhNghiem(byte ghe, int diem)
    {
        if (ghe != gheToi) return;      // goi phat cho ca phong, chi phan cua minh moi tinh
        CapDo.Them(diem);
    }

    void ThemChet(byte ghe, byte gheKeHa)
    {
        if (ghe >= GoiTin.SoGheToiDa) return;
        bool moi = daChet.Add(ghe);
        if (!moi) return;                                  // goi gui lai - dung cong hai lan

        if (gheKeHa < nguoiTheoGhe.Length && gheKeHa != ghe && nguoiTheoGhe[gheKeHa] < 255)
            nguoiTheoGhe[gheKeHa]++;
    }

    /// <summary>Nhan vat cua ghe nay dang chet (hoac da roi tran).</summary>
    bool DaRaKhoiTran(byte ghe)
    {
        if (daChet.Contains(ghe)) return true;
        if (dongBo != null && dongBo.GheDaRoi(ghe)) return true;
        var nv = dongBo != null ? dongBo.NhanVatCuaGhe(ghe) : null;
        if (nv == null) return true;
        var mau = nv.GetComponent<Damageable>();
        return mau == null || mau.IsDead;
    }

    // ================================================================
    //  DEM
    // ================================================================

    void Update()
    {
        if (!TranHienTai.DangChoiMang || DaXong) return;

        BaoToiChetNeuCan();

        XemTiepNeuDaChet();

        if (Time.unscaledTime < demLanSau) return;
        demLanSau = Time.unscaledTime + NhipDem;

        if (CheDoTran.LaTranDoi) DemDoi();

        if (!TranHienTai.LaHost) return;                   // chi chu phong phan quyet
        XetXong();
    }

    /// <summary>
    /// Nhan vat CUA MINH vua chet thi bao cho ca phong, kem ten ke ha minh.
    ///
    /// Doc <see cref="Damageable.keDanhCuoi"/> chu khong doan: don cuoi cung
    /// den tu quai thi khong ai duoc cong nguoi ha.
    /// </summary>
    void BaoToiChetNeuCan()
    {
        if (daBaoToiChet || toi == null) return;
        var mau = toi.GetComponent<Damageable>();
        if (mau == null || !mau.IsDead) return;

        daBaoToiChet = true;
        byte keHa = GheCuaDamageable(mau.keDanhCuoi);
        ThemChet(gheToi, keHa);

        if (dongBo != null) StartCoroutine(GuiLai(GoiTin.VietChet(gheToi, keHa)));
    }

    /// <summary>
    /// CHET ROI THI NGOI XEM, khong bam R choi lai (GameDirector.DuocChoiLai da
    /// chan tu buoc 5) va cung khong nhin mai cai xac cua minh: camera chuyen
    /// sang bam mot nguoi CON SONG, doi xem ai la nguoi cuoi cung.
    /// </summary>
    void XemTiepNeuDaChet()
    {
        if (!daBaoToiChet || toi == null) return;
        if (camera3D == null) camera3D = Object.FindAnyObjectByType<CameraRig>();
        if (camera3D == null) return;

        // Con dang bam mot nguoi song thi de yen
        if (camera3D.target != null && camera3D.target != toi.transform)
        {
            var m = camera3D.target.GetComponent<Damageable>();
            if (m != null && !m.IsDead) return;
        }

        // Tran Doi: xem DONG DOI con song truoc (luot 0), het dong doi moi xem doi kia (luot 1)
        for (int luot = 0; luot < 2; luot++)
            for (byte g = 0; g < GoiTin.SoGheToiDa; g++)
            {
                if (g == gheToi || DaRaKhoiTran(g)) continue;
                bool dongDoi = CheDoTran.LaTranDoi && DoiCua(g) == DoiCuaToi;
                if (luot == 0 && CheDoTran.LaTranDoi && !dongDoi) continue;
                var nv = dongBo != null ? dongBo.NhanVatCuaGhe(g) : null;
                if (nv == null) continue;
                camera3D.target = nv.transform;
                return;
            }
    }

    /// <summary>
    /// TRAN DOI: dem so nguoi / so nguoi con song moi doi cho HUD (moi may tu dem - ban sao cua nguoi khac co co "da chet"
    /// trong goi trang thai). Ghe chua kip vao tran tinh la con song.
    /// </summary>
    void DemDoi()
    {
        SoNguoiDoi[0] = SoNguoiDoi[1] = SoSongDoi[0] = SoSongDoi[1] = 0;
        if (GheCoTrongPhong == null) return;
        foreach (var g in GheCoTrongPhong)
        {
            int d = DoiCua(g);
            if (d != CheDoTran.DoiA && d != CheDoTran.DoiB) continue;
            SoNguoiDoi[d]++;
            if (!DaThay(g) || !DaRaKhoiTran(g)) SoSongDoi[d]++;
        }
    }

    /// <summary>Ghe nay da tung co mat trong tran (co nhan vat, da chet, da roi) - hay la minh.</summary>
    bool DaThay(byte g)
    {
        if (g == gheToi) return true;
        if (daChet.Contains(g)) return true;
        if (dongBo == null) return false;
        return dongBo.NhanVatCuaGhe(g) != null || dongBo.GheDaRoi(g);
    }

    CameraRig camera3D;

    IEnumerator GuiLai(byte[] goi)
    {
        for (int i = 0; i < SoLanGuiLai; i++)
        {
            if (dongBo == null) yield break;
            dongBo.GuiGoi(goi);
            yield return new WaitForSecondsRealtime(CachNhauGuiLai);
        }
    }

    /// <summary>
    /// CHU PHONG DEM: con may nguoi vua song vua chua roi tran.
    ///
    /// Chi ket thuc khi tran DA TUNG co tu hai nguoi tro len. Mot nguoi duy
    /// nhat trong phong (ban khac dang tai man) thi "con mot nguoi song" khong
    /// co nghia la anh ta vua thang.
    /// </summary>
    void XetXong()
    {
        if (CheDoTran.LaTranDoi) { XetXongDoi(); return; }

        var ghe = GheTrongTran();
        if (ghe.Count < 2) return;

        byte conSong = 255;
        int dem = 0;
        foreach (var g in ghe)
        {
            if (DaRaKhoiTran(g)) continue;
            dem++;
            conSong = g;
        }
        if (dem > 1) return;

        var kq = new GoiTin.KetQua
        {
            gheThang = dem == 1 ? conSong : (byte)255,
            quaiTheoGhe = quaiTheoGhe,
            nguoiTheoGhe = nguoiTheoGhe
        };
        ApKetQua(kq);
        if (dongBo != null) StartCoroutine(GuiLai(GoiTin.VietKetTran(kq)));
    }

    /// <summary>
    /// CHU PHONG DEM - TRAN DOI: doi nao con nguoi song cuoi cung thi thang (nguoi dung 28/09/2026). Ca phong mot doi -> doi ay
    /// thang ngay (nguoi dung chon). Doi CA PHONG vao tran (hoac qua <see cref="GiayChoCaPhongVao"/> giay) roi moi phan quyet:
    /// goi ket tran gui luc khach chua noi xong thi khach khong bao gio nghe duoc.
    /// </summary>
    void XetXongDoi()
    {
        if (GheCoTrongPhong == null || GheCoTrongPhong.Count == 0) return;
        bool quaHan = Time.unscaledTime - lucGan > GiayChoCaPhongVao;

        var doi = new List<sbyte>();
        var song = new List<bool>();
        foreach (var g in GheCoTrongPhong)
        {
            bool daThay = DaThay(g);
            if (!daThay && !quaHan) return;                 // con nguoi dang tai man
            doi.Add(DoiCua(g));
            song.Add(daThay && !DaRaKhoiTran(g));
        }

        int kq = CheDoTran.DoiThang(doi, song);
        if (kq == CheDoTran.ChuaXong) return;

        var ketQua = new GoiTin.KetQua
        {
            gheThang = 255,
            doiThang = GoiTin.MaDoiThang(kq),
            quaiTheoGhe = quaiTheoGhe,
            nguoiTheoGhe = nguoiTheoGhe
        };
        ApKetQua(ketQua);
        if (dongBo != null) StartCoroutine(GuiLai(GoiTin.VietKetTran(ketQua)));
    }

    /// <summary>Nhung ghe co mat trong tran nay (ke ca da chet, tru nguoi roi tran).</summary>
    List<byte> GheTrongTran()
    {
        var ra = new List<byte>();
        for (byte g = 0; g < GoiTin.SoGheToiDa; g++)
        {
            if (g == gheToi) { ra.Add(g); continue; }
            if (dongBo == null) continue;
            // Ghe da roi tran VAN tinh la "da tung co mat" - neu khong, hai
            // nguoi ma mot nguoi thoat thi so ghe tut ve 1 va tran khong bao gio xong.
            if (dongBo.NhanVatCuaGhe(g) != null || dongBo.GheDaRoi(g)) ra.Add(g);
        }
        return ra;
    }

    // ================================================================
    //  KET QUA
    // ================================================================

    void NgheKetTran(GoiTin.KetQua kq) { ApKetQua(kq); }

    void ApKetQua(GoiTin.KetQua kq)
    {
        if (DaXong) return;
        DaXong = true;
        GheThang = kq.gheThang;
        BangDiem = kq;
        if (kq.doiThang != GoiTin.KhongDoiThang)
        {
            // Tran Doi: ca doi thang, ke ca nguoi da nga xuong giua tran
            DoiThang = GoiTin.DoiTuMa(kq.doiThang);
            ToiThang = DoiThang >= 0 && DoiThang == DoiCuaToi;
            TenNguoiThang = DoiThang >= 0 ? CheDoTran.TenDoi(DoiThang) : "";
        }
        else
        {
            ToiThang = kq.gheThang == gheToi;
            TenNguoiThang = kq.gheThang == 255 ? ""
                : (TenCuaGhe != null ? TenCuaGhe(kq.gheThang) : "người chơi " + (kq.gheThang + 1));
        }

        // Ten va danh sach ghe cho bang diem - HUD khong biet gi ve phong
        var dsGhe = GheTrongTran();
        if (GheCoTrongPhong != null) foreach (var g in GheCoTrongPhong) if (!dsGhe.Contains(g)) dsGhe.Add(g);
        foreach (var g in dsGhe)
        {
            CoTrongTran[g] = true;
            TenTheoGhe[g] = TenCuaGhe != null ? TenCuaGhe(g) : "người chơi " + (g + 1);
            DoiTheoGhe[g] = CheDoTran.LaTranDoi ? DoiCua(g) : CheDoTran.KhongDoi;
        }

        // Nhan vat khong con dieu khien duoc nua - van dau xong roi
        if (toi != null) toi.enabled = false;

        GhiThanhTich(kq);
        Debug.Log("[KetTran] xong - ghe thang " + kq.gheThang + ", toi ghe " + gheToi);
    }

    /// <summary>
    /// Ghi mot tran da choi (va mot tran thang neu minh thang) vao ho so.
    ///
    /// Moi may tu ghi ho so CUA MINH: luat bao mat cua Firestore chi cho moi
    /// nguoi sua document cua chinh ho, nen chu phong khong ghi ho nguoi khac
    /// duoc - va cung khong nen, khong ai muon diem cua minh do may nguoi khac
    /// quyet.
    /// </summary>
    void GhiThanhTich(GoiTin.KetQua kq)
    {
        if (string.IsNullOrEmpty(FirebaseMang.Uid)) return;
        int quai = gheToi < kq.quaiTheoGhe.Length ? kq.quaiTheoGhe[gheToi] : 0;
        int nguoi = gheToi < kq.nguoiTheoGhe.Length ? kq.nguoiTheoGhe[gheToi] : 0;
        StartCoroutine(HoSoMang.CongThanhTich(1, ToiThang ? 1 : 0, quai, nguoi,
            ok => Debug.Log("[KetTran] ghi thanh tich: " + (ok ? "xong" : "khong ghi duoc"))));
    }
}
