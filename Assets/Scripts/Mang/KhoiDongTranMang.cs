using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// VAO TRAN MANG: BAT TAY, SINH NHAN VAT CHO NGUOI KIA, ROI DONG BO.
///
/// Day la cho moi thu cua giai doan 2 gap nhau lan dau tren mot man hinh
/// that. Truoc do tung manh deu da duoc do rieng, nhung do rieng khong chung
/// minh duoc chung ghep lai thi chay.
///
/// Trinh tu:
///   1. Doc lai phong tu Firebase - biet ai la chu phong, ai la khach.
///   2. Chu phong MOI, khach NHAN. Bat tay qua Realtime Database (mot lan,
///      1-2 giay), xong thi Firebase dung sang mot ben.
///   3. Sinh nhan vat cho nguoi kia.
///   4. Gan DongBoTran de gui va nhan vi tri 60 lan moi giay.
///
/// GIOI HAN HIEN TAI: HAI NGUOI.
/// KenhTrucTiep giu mot ket noi duy nhat. Bon nguoi thi phai noi hinh sao qua
/// chu phong va chu phong chuyen tiep - chua lam. Noi ro o day chu khong de
/// nguoi ta vao phong bon nguoi roi ngo ngac vi chi thay mot.
///
/// Ban Editor/PC KHONG noi mang that duoc: WebRTC chi co tren WebGL, ben nay
/// <see cref="KenhTrucTiep"/> chay kenh gia lap. Muon thu that thi phai xuat
/// ban WebGL roi mo tren hai may.
/// </summary>
public class KhoiDongTranMang : MonoBehaviour
{
    /// <summary>Trang thai de hien len man hinh - nguoi choi phai thay minh
    /// dang o buoc nao, khong duoc de mot man hinh im lang.</summary>
    public static string TrangThai = "";
    public static bool DaNoi;

    /// <summary>
    /// Dong bao ve duong truyen giua tran: "dang cho tin hieu", "nguoi kia da
    /// roi tran". Tach rieng voi TrangThai vi no co the hien ra bat cu luc nao
    /// sau khi dong "Da noi!" da tat.
    /// </summary>
    public static string ThongBaoKetNoi = "";
    static bool thongBaoLaLoi;
    public static bool ThongBaoLaLoi { get { return thongBaoLaLoi; } }

    /// <summary>
    /// Chu phong da roi tran, nguoi khach can mot duong ve sanh. HUD doc co nay
    /// de hien nut TRO VE tren may cam ung - ben do khong co phim ESC.
    /// </summary>
    public static bool CanNutVeSanh;

    float tatThongBaoLuc = -1f;
    public static string LoiCuoi;

    PlayerController toi;
    DongBoTran dongBo;
    float batDauLuc;

    /// <summary>
    /// TU DAT MINH VAO MAN CHOI, khong bat ai keo tay vao scene.
    ///
    /// Act1 dung bang code luc chay con Act2 la scene da nuong san - gan tay
    /// thi phai nho ca hai, va quen mot cai la mot man khong noi mang duoc ma
    /// khong bao gi.
    ///
    /// CAI BAY DA VAP: [RuntimeInitializeOnLoadMethod] chay DUNG MOT LAN, luc
    /// game vua khoi dong - KHONG chay lai moi lan nap scene. Ma luc ay nguoi
    /// choi con dang o MainMenu, DangChoiMang van con false, nen ham thoat ra
    /// ngay dong dau va khong bao gio quay lai. Nap Act2 sau do thi khong ai
    /// dung day bo noi mang ca: hai nguoi vao dung mot phong, dung mot man, ma
    /// khong he thay nhau - va man hinh khong noi mot chu nao.
    ///
    /// Nen phan viec cua no la DANG KY MOT LAN, roi de sceneLoaded goi lai sau
    /// moi lan nap man.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void DangKyNgheNapMan()
    {
        SceneManager.sceneLoaded -= KhiNapXongMan;
        SceneManager.sceneLoaded += KhiNapXongMan;

        // Man dang mo luc dang ky cung phai duoc xet: neu ai do vao thang Act2
        // ma khong qua MainMenu (chay thu trong Editor chang han) thi khong co
        // lan nap nao de nghe ca.
        KhiNapXongMan(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void KhiNapXongMan(Scene canh, LoadSceneMode kieu)
    {
        if (!TranHienTai.DangChoiMang) return;
        if (canh.name != "Act1" && canh.name != "Act2") return;
        if (Object.FindAnyObjectByType<KhoiDongTranMang>() != null) return;

        var go = new GameObject("TranMang");
        go.AddComponent<KhoiDongTranMang>();
    }

    void Start()
    {
        TrangThai = ""; DaNoi = false; LoiCuoi = null; NhanDang = "";
        ThongBaoKetNoi = ""; thongBaoLaLoi = false; CanNutVeSanh = false;

        // Choi mot minh thi khong lam gi ca - va phai tu bo di, khong nam lai
        // an bo nho cho mot viec khong bao gio xay ra.
        if (!TranHienTai.DangChoiMang) { Destroy(this); return; }

        StartCoroutine(ChayVao());
    }

    /// <summary>
    /// BANG NHAN DANG - LUON HIEN O GOC MAN CHOI MANG.
    ///
    /// Hai nguoi bao "toi vao hai ban do khac nhau" thi cau hoi dau tien la:
    /// hai may co dang o cung mot phong khong. Khong co dong nay thi khong ai
    /// tra loi duoc, ke ca toi - phai doan. Co no thi chi can chup hai man
    /// hinh la doc ra ngay: cung ma phong hay khac, cung man hay khac.
    /// </summary>
    public static string NhanDang = "";

    IEnumerator ChayVao()
    {
        batDauLuc = Time.unscaledTime;
        TrangThai = "Đang tìm người chơi khác...";

        string maNgan = string.IsNullOrEmpty(TranHienTai.MaPhong)
            ? "?" : TranHienTai.MaPhong.Substring(
                  Mathf.Max(0, TranHienTai.MaPhong.Length - 6));
        NhanDang = string.Format("phòng …{0} · {1} · {2}", maNgan,
                                 TranHienTai.ManChoi,
                                 TranHienTai.LaHost ? "chủ phòng" : "khách");

        // Doi nhan vat cua minh duoc dung xong
        float han = Time.unscaledTime + 25f;
        while (toi == null && Time.unscaledTime < han)
        {
            var dir = GameDirector.Instance;
            if (dir != null && dir.player != null)
                toi = dir.player.GetComponent<PlayerController>();
            if (toi == null) toi = Object.FindAnyObjectByType<PlayerController>();
            if (toi == null) yield return null;
        }

        if (toi == null) { Hong("khong dung duoc nhan vat cua minh"); yield break; }

        if (KenhTrucTiep.LaGiaLap)
        {
            // Noi thang ra chu khong de nguoi ta doi mai: ban Editor khong co
            // WebRTC, va mot cai vong xoay bat tan thi nhin nhu treo may.
            TrangThai = "Bản này chạy trong Unity Editor nên chưa nối mạng thật "
                      + "được (WebRTC chỉ có trên bản web). Hãy mở "
                      + "diablo25d-game.web.app trên hai máy.";
            yield break;
        }

        // ---- 1. Doc lai phong ----
        bool ok = false;
        yield return PhongMang.TaiLaiPhong(TranHienTai.MaPhong, (o, e) => { ok = o; });
        var phong = PhongMang.PhongHienTai;
        if (!ok || phong == null) { Hong("khong doc duoc phong"); yield break; }

        // ---- 2. Xep ghe ----
        bangGhe = XepGhe(phong.nguoiChoi, phong.hostUid);
        MotGhe gheToi = TimGhe(bangGhe, FirebaseMang.Uid);
        if (gheToi.uid == null) { Hong("khong thay minh trong danh sach phong"); yield break; }

        NhanDang += string.Format(" · ghế {0} · {1} người", gheToi.ghe, bangGhe.Count);

        if (bangGhe.Count < 2)
        {
            TrangThai = "Chơi một mình trong phòng - không có ai để nối.";
            yield break;
        }

        DatChoDungTheoGhe(gheToi.ghe);

        // Ten cua minh tren dau nhan vat cua minh (mau vang) - ban sao cua nguoi
        // khac tu gan ten luc sinh (NguoiChoiKhac.Sinh)
        BangTen.Gan(toi.gameObject, TenCuaGhe(gheToi.ghe), true);

        // ---- 3. Dung bo dong bo TRUOC khi bat tay ----
        // Goi tin cua nguoi ta co the den ngay khi kenh vua mo - bo dong bo
        // phai san sang nghe tu truoc, khong thi vai goi dau roi vao khoang.
        dongBo = gameObject.AddComponent<DongBoTran>();
        dongBo.toi = toi;
        dongBo.chiSoCuaToi = gheToi.ghe;

        // Phai goi TAY sau khi gan "toi": OnEnable cua DongBoTran da chay xong
        // tu luc AddComponent, luc ay "toi" con la null nen no khong nghe duoc
        // ai ca - va ky nang se khong bao gio duoc gui sang may kia.
        dongBo.GanTaiNghe();

        // Ca phong danh chung mot dan quai: chu phong ke lai, khach nghe.
        var boQuai = gameObject.AddComponent<DongBoQuai>();
        boQuai.dongBo = dongBo;
        dongBo.quai = boQuai;

        dongBo.TaoNguoiKhiCan = SinhBanSao;
        dongBo.KhiMatNguoi += KhiMotNguoiRoiTran;
        dongBo.KhiMatKetNoi += KhiMatHetKetNoi;

        // Ket tran: ai song sot cuoi cung thi thang (buoc 6). Chu phong phan
        // quyet, moi may hien cung mot bang diem.
        KetTran.Xoa();
        var ketTran = gameObject.AddComponent<KetTran>();
        ketTran.TenCuaGhe = TenCuaGhe;
        ketTran.Gan(dongBo, toi, gheToi.ghe);

        // Nhan vat cua minh ghi lai duong di: bu tre can biet "mot khoang truoc
        // day minh dung o dau" de lui ve do tinh trung.
        if (toi.GetComponent<LichSuViTri>() == null)
            toi.gameObject.AddComponent<LichSuViTri>();

        // ---- 4. Bat tay - SONG SONG voi moi nguoi can noi ----
        //
        // Chu phong noi voi tung nguoi khach, moi nguoi mot kenh, so kenh =
        // so ghe cua ho. Khach chi noi voi chu phong.
        //
        // Chay song song chu khong lan luot: lan luot thi nguoi khach thu ba
        // phai doi hai lan bat tay truoc, va neu mot lan hong (het 20 giay) thi
        // ca nguoi sau cung het gio theo.
        var canNoi = new List<MotGhe>();
        foreach (var g in bangGhe)
        {
            if (g.ghe == gheToi.ghe) continue;
            if (TranHienTai.LaHost || g.laChuPhong) canNoi.Add(g);
        }

        TrangThai = TranHienTai.LaHost
            ? string.Format("Đang mời {0} người nối vào...", canNoi.Count)
            : "Đang nối vào chủ phòng...";

        int conDoi = canNoi.Count;
        var loiTheoGhe = new Dictionary<byte, string>();
        var noiDuoc = new List<MotGhe>();

        foreach (var g in canNoi)
        {
            var gg = g;
            System.Action<bool, string> xong = (o, e) =>
            {
                if (o) noiDuoc.Add(gg);
                else loiTheoGhe[gg.ghe] = e ?? "không nối được";
                conDoi--;
            };

            if (TranHienTai.LaHost)
                StartCoroutine(BatTay.ChuPhongMoi(TranHienTai.MaPhong, g.uid, g.ghe, xong));
            else
                StartCoroutine(BatTay.NguoiVaoNhan(TranHienTai.MaPhong, g.uid, g.ghe, xong));
        }

        float hanBatTay = Time.unscaledTime + BatTay.GiayHetHan + 10f;
        while (conDoi > 0 && Time.unscaledTime < hanBatTay) yield return null;

        float matBaoLau = Time.unscaledTime - batDauLuc;

        if (noiDuoc.Count == 0)
        {
            string vi = "";
            foreach (var cap in loiTheoGhe) { vi = cap.Value; break; }
            Hong(vi.Length > 0 ? vi : "không nối được");
            yield break;
        }

        DaNoi = true;

        if (loiTheoGhe.Count == 0)
        {
            TrangThai = TranHienTai.LaHost && noiDuoc.Count > 1
                ? string.Format("Đã nối cả {0} người! (bắt tay mất {1:F1} giây)", noiDuoc.Count, matBaoLau)
                : string.Format("Đã nối! (bắt tay mất {0:F1} giây)", matBaoLau);
        }
        else
        {
            // Noi duoc mot phan: noi THANG ra ai khong vao duoc, dung im lang
            // de nguoi ta tu hoi vi sao ban minh khong thay dau.
            var sb = new System.Text.StringBuilder();
            foreach (var cap in loiTheoGhe)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(TenCuaGhe(cap.Key));
            }
            TrangThai = string.Format("Đã nối {0}/{1} người. Không nối được với: {2}",
                                      noiDuoc.Count, canNoi.Count, sb);
        }

        // Vai giay sau thi thoi bao, tra man hinh lai cho game
        yield return new WaitForSecondsRealtime(loiTheoGhe.Count == 0 ? 3f : 8f);
        TrangThai = "";
    }

    // ================================================================
    //  GHE NGOI
    // ================================================================

    /// <summary>Mot nguoi trong tran va cho ngoi cua ho.</summary>
    public struct MotGhe
    {
        public byte ghe;
        public string uid;
        public string ten;
        public bool laChuPhong;
    }

    /// <summary>Bang ghe cua tran nay - de tra ten tu so ghe.</summary>
    List<MotGhe> bangGhe = new List<MotGhe>();

    /// <summary>
    /// XEP GHE CHO CA PHONG - moi may tu tinh, va moi may ra CUNG MOT DAP AN.
    ///
    /// Chu phong luon ghe 0. Nhung nguoi con lai xep theo o "cho" ghi trong
    /// phong, bang nhau thi theo uid. Ghe cuoi cung la THU TU trong danh sach
    /// ay, khong phai o "cho" doc thang tu Firebase.
    ///
    /// Vi sao khong dung thang o "cho": hai nguoi vao phong cach nhau vai tram
    /// mili giay co the cung doc thay ghe 1 con trong va cung ngoi vao - dung
    /// cuoc dua da vap o nut "Vao phong nhanh". Hai nguoi cung ghe 1 thi goi
    /// tin cua ho de len nhau va mot nguoi bien mat. Xep lai theo mot quy tac
    /// tat dinh thi hai may doc cung mot danh sach luon ra cung mot bang ghe.
    ///
    /// Ham thuan - de con kiem duoc bang so.
    /// </summary>
    public static List<MotGhe> XepGhe(List<PhongMang.NguoiTrongPhong> ds, string hostUid)
    {
        var chuPhong = new List<PhongMang.NguoiTrongPhong>();
        var khach = new List<PhongMang.NguoiTrongPhong>();
        foreach (var n in ds)
        {
            if (n == null || string.IsNullOrEmpty(n.uid)) continue;
            if (n.uid == hostUid) chuPhong.Add(n); else khach.Add(n);
        }

        khach.Sort((a, b) =>
        {
            int c = a.cho.CompareTo(b.cho);
            return c != 0 ? c : string.CompareOrdinal(a.uid, b.uid);
        });

        var ra = new List<MotGhe>();
        foreach (var n in chuPhong)
            ra.Add(new MotGhe { ghe = 0, uid = n.uid, ten = n.ten, laChuPhong = true });
        foreach (var n in khach)
        {
            if (ra.Count >= KenhTrucTiep.SoKenhToiDa) break;
            ra.Add(new MotGhe { ghe = (byte)ra.Count, uid = n.uid, ten = n.ten, laChuPhong = false });
        }
        return ra;
    }

    public static MotGhe TimGhe(List<MotGhe> bang, string uid)
    {
        foreach (var g in bang) if (g.uid == uid) return g;
        return new MotGhe { ghe = 255 };
    }

    string TenCuaGhe(byte ghe)
    {
        foreach (var g in bangGhe)
            if (g.ghe == ghe) return string.IsNullOrEmpty(g.ten) ? ("người chơi " + (ghe + 1)) : g.ten;
        return "người chơi " + (ghe + 1);
    }

    /// <summary>
    /// Moi nguoi dung mot cho quanh diem xuat phat, theo ghe.
    ///
    /// Tat ca nhan vat deu sinh ra o CUNG MOT diem trong scene. Hai nguoi thi
    /// con chiu duoc; bon nguoi chong len nhau thanh mot khoi thi khong ai biet
    /// minh la ai. Xep thanh bon goc cua mot o vuong nho quanh diem ay.
    /// </summary>
    void DatChoDungTheoGhe(byte ghe)
    {
        if (toi == null || ghe == 0) return;       // chu phong giu dung cho cu

        const float BanKinh = 1.6f;
        float goc = ghe * 90f;
        Vector3 lech = Quaternion.Euler(0f, goc, 0f) * new Vector3(0f, 0f, BanKinh);
        Vector3 moi = toi.transform.position + lech;
        moi.y = VfxFactory.GroundY(moi) + 0.1f;

        var cc = toi.GetComponent<CharacterController>();
        bool batLai = cc != null && cc.enabled;
        if (batLai) cc.enabled = false;
        toi.transform.position = moi;
        if (batLai) cc.enabled = true;
    }

    /// <summary>
    /// Goi tin dau tien cua nguoi ngoi ghe <paramref name="ghe"/> vua den -
    /// dung ban sao cho ho ngay tai cho ho dang dung.
    /// </summary>
    PlayerController SinhBanSao(byte ghe, Vector3 viTri)
    {
        MotGhe g = new MotGhe { ghe = 255 };
        foreach (var x in bangGhe) if (x.ghe == ghe) g = x;
        if (g.uid == null) return null;      // ghe khong co trong phong - goi la

        var nv = NguoiChoiKhac.Sinh(g.uid, TenCuaGhe(ghe), viTri);
        if (nv == null) return null;

        // Ban sao cung ghi lai duong di - de phep cua CHINH HO khong bi lui
        // (BuTre.Mo bo qua nguoi tung), va de lui ho khi phep nguoi khac trung.
        if (nv.GetComponent<LichSuViTri>() == null)
            nv.gameObject.AddComponent<LichSuViTri>();

        return nv;
    }

    // ================================================================
    //  MAT KET NOI GIUA TRAN
    // ================================================================

    void Update()
    {
        if (dongBo == null || !DaNoi) return;

        if (tatThongBaoLuc > 0f && Time.unscaledTime >= tatThongBaoLuc)
        {
            ThongBaoKetNoi = "";
            tatThongBaoLuc = -1f;
        }

        if (dongBo.TinhTrang == DongBoTran.TinhTrangKetNoi.DangChoTinHieu)
        {
            ThongBaoKetNoi = string.Format(
                "Đang chờ tín hiệu từ {0}... ({1:F0} giây)",
                dongBo.ChiSoDangCho == 0 && !TranHienTai.LaHost ? "chủ phòng"
                    : TenCuaGhe(dongBo.ChiSoDangCho), dongBo.ImLangGiay);
            thongBaoLaLoi = false;
        }
        else if (dongBo.TinhTrang == DongBoTran.TinhTrangKetNoi.Tot
                 && !thongBaoLaLoi && tatThongBaoLuc < 0f)
        {
            // Tin hieu da quay lai - thoi bao
            ThongBaoKetNoi = "";
        }
    }

    /// <summary>
    /// MOT NGUOI vua roi tran. Xu ly khac nhau tuy AI di, vi moi phia mat
    /// mot thu khac nhau:
    ///
    ///   - Mot NGUOI KHACH di: tran van chay binh thuong - quai, nhip dot, moi
    ///     thu deu nam o may chu phong. Go ban sao cua ho de quai thoi duoi theo
    ///     mot cai bong, bao mot cau, roi choi tiep.
    ///   - CHU PHONG di (tren may khach): mat LUON ca tran - quai do chu phong
    ///     dieu khien, mau quai do chu phong tinh, va moi goi tin cua nhung
    ///     nguoi khach khac cung phai qua tay chu phong. Khong con gi de choi
    ///     tiep. Phai noi thang ra va dua cho nguoi ta mot duong ve.
    /// </summary>
    void KhiMotNguoiRoiTran(byte ghe)
    {
        string ten = TenCuaGhe(ghe);
        if (dongBo != null) dongBo.BoNguoi(ghe);

        bool chuPhongDi = !TranHienTai.LaHost && TimGheTheoSo(ghe).laChuPhong;

        if (chuPhongDi)
        {
            ThongBaoKetNoi = "Chủ phòng đã rời trận — trận đấu dừng tại đây."
                + System.Environment.NewLine
                + (CamUng.DangDung ? "Bấm TRỞ VỀ để về sảnh." : "Bấm ESC để về sảnh.");
            thongBaoLaLoi = true;
            CanNutVeSanh = true;
        }
        else
        {
            bool conAi = dongBo != null && dongBo.SoNguoiKhac > 0;
            ThongBaoKetNoi = ten + " đã rời trận."
                + (conAi ? "" : " Bạn chơi tiếp một mình.");
            thongBaoLaLoi = false;
            tatThongBaoLuc = Time.unscaledTime + 6f;
        }

        Debug.LogWarning("[TranMang] ghe " + ghe + " da roi tran");
    }

    /// <summary>Het sach ket noi. Voi may khach, KhiMotNguoiRoiTran da bao
    /// "chu phong da roi tran" roi - day chi la luoi an toan.</summary>
    void KhiMatHetKetNoi()
    {
        if (!TranHienTai.LaHost && !CanNutVeSanh)
            KhiMotNguoiRoiTran(0);
    }

    MotGhe TimGheTheoSo(byte ghe)
    {
        foreach (var g in bangGhe) if (g.ghe == ghe) return g;
        return new MotGhe { ghe = 255 };
    }

    void Hong(string vi)
    {
        LoiCuoi = vi;
        TrangThai = "Không nối được với người chơi kia: " + vi;
        Debug.LogWarning("[TranMang] " + vi);
    }

    // ================================================================
    //  BANG TRANG THAI - ve bang OnGUI nhu moi thu khac trong game nay
    // ================================================================

    void OnGUI()
    {
        float s = Screen.height / 1080f;

        if (!string.IsNullOrEmpty(NhanDang))
        {
            var kieuNho = new GUIStyle(GUI.skin.label);
            kieuNho.font = GiaoDien.ChuThuong;     // font mac dinh thieu chu co dau tieng Viet
            kieuNho.fontSize = Mathf.RoundToInt(15f * s);
            kieuNho.normal.textColor = new Color(0.70f, 0.70f, 0.74f, 0.85f);
            GUI.Label(new Rect(10f * s, Screen.height - 26f * s, 520f * s, 22f * s),
                      NhanDang, kieuNho);
        }

        // Trong tran luon co HUD: HUD ve TrangThai va ThongBaoKetNoi theo bo cuc
        // chung (GameHUDKinhDi). Truoc day ve o day, ngay giua mep tren - DE LEN
        // khung dot quai cua HUD. Chi tu ve khi khong co HUD.
        if (GameHUD.Ban != null && GameHUD.Ban.isActiveAndEnabled) return;

        if (!string.IsNullOrEmpty(ThongBaoKetNoi))
        {
            var kieuKN = new GUIStyle(GUI.skin.label);
            kieuKN.font = GiaoDien.ChuDam;
            kieuKN.fontSize = Mathf.RoundToInt(24f * s);
            kieuKN.alignment = TextAnchor.MiddleCenter;
            kieuKN.wordWrap = true;
            kieuKN.fontStyle = FontStyle.Bold;
            kieuKN.normal.textColor = thongBaoLaLoi ? new Color(0.95f, 0.45f, 0.40f)
                                                    : new Color(0.95f, 0.85f, 0.45f);

            float rongKN = Mathf.Min(Screen.width * 0.85f, 1000f * s);
            var oKN = new Rect((Screen.width - rongKN) * 0.5f, Screen.height * 0.28f,
                               rongKN, 110f * s);

            // Nen toi phia sau cho chu doc duoc tren canh sang
            var cu = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(oKN, Texture2D.whiteTexture);
            GUI.color = cu;
            GUI.Label(oKN, ThongBaoKetNoi, kieuKN);
        }

        if (string.IsNullOrEmpty(TrangThai)) return;

        var kieu = new GUIStyle(GUI.skin.label);
        kieu.font = GiaoDien.ChuThuong;
        kieu.fontSize = Mathf.RoundToInt(22f * s);
        kieu.alignment = TextAnchor.MiddleCenter;
        kieu.wordWrap = true;
        kieu.normal.textColor = DaNoi ? new Color(0.35f, 0.85f, 0.45f)
                              : LoiCuoi != null ? new Color(0.90f, 0.42f, 0.40f)
                              : new Color(0.92f, 0.86f, 0.55f);

        float rong = Mathf.Min(Screen.width * 0.8f, 900f * s);
        var o = new Rect((Screen.width - rong) * 0.5f, 24f * s, rong, 120f * s);

        GUI.Label(o, TrangThai, kieu);
    }
}
