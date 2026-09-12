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

    public DongBoTran dongBo;
    public PlayerController toi;
    public byte gheToi = 255;

    /// <summary>Ten theo ghe - de ve bang diem. KhoiDongTranMang gan.</summary>
    public System.Func<byte, string> TenCuaGhe;

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
    }

    public void Gan(DongBoTran db, PlayerController nhanVat, byte ghe)
    {
        dongBo = db; toi = nhanVat; gheToi = ghe;
        GheCuaToi = ghe;
        if (dongBo != null)
        {
            dongBo.KhiNgheChet += NgheChet;
            dongBo.KhiNgheKetTran += NgheKetTran;
        }
    }

    void OnDisable()
    {
        if (dongBo == null) return;
        dongBo.KhiNgheChet -= NgheChet;
        dongBo.KhiNgheKetTran -= NgheKetTran;
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
        ThemChet(ghe, gheKeHa);
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

        for (byte g = 0; g < GoiTin.SoGheToiDa; g++)
        {
            if (g == gheToi || DaRaKhoiTran(g)) continue;
            var nv = dongBo != null ? dongBo.NhanVatCuaGhe(g) : null;
            if (nv == null) continue;
            camera3D.target = nv.transform;
            return;
        }
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
        ToiThang = kq.gheThang == gheToi;
        BangDiem = kq;
        TenNguoiThang = kq.gheThang == 255 ? ""
            : (TenCuaGhe != null ? TenCuaGhe(kq.gheThang) : "người chơi " + (kq.gheThang + 1));

        // Ten va danh sach ghe cho bang diem - HUD khong biet gi ve phong
        foreach (var g in GheTrongTran())
        {
            CoTrongTran[g] = true;
            TenTheoGhe[g] = TenCuaGhe != null ? TenCuaGhe(g) : "người chơi " + (g + 1);
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
        StartCoroutine(HoSoMang.CongThanhTich(1, ToiThang ? 1 : 0, quai, nguoi, null));
    }
}
