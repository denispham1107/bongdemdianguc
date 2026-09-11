using System.Collections;
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

    /// <summary>
    /// Chu phong da roi tran, nguoi khach can mot duong ve sanh. HUD doc co nay
    /// de hien nut TRO VE tren may cam ung - ben do khong co phim ESC.
    /// </summary>
    public static bool CanNutVeSanh;

    byte chiSoKia;
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

        NhanDang += string.Format(" · {0} người", phong.nguoiChoi.Count);

        // ---- 2. Tim nguoi kia ----
        string uidKia = null;
        for (int i = 0; i < phong.nguoiChoi.Count; i++)
        {
            var n = phong.nguoiChoi[i];
            if (n.uid != FirebaseMang.Uid) { uidKia = n.uid; break; }
        }

        if (string.IsNullOrEmpty(uidKia))
        {
            TrangThai = "Chơi một mình trong phòng - không có ai để nối.";
            yield break;
        }

        // ---- 3. Bat tay ----
        TrangThai = TranHienTai.LaHost
            ? "Đang mời người kia nối vào..."
            : "Đang nối vào chủ phòng...";

        bool noiDuoc = false; string loi = null;

        if (TranHienTai.LaHost)
            yield return BatTay.ChuPhongMoi(TranHienTai.MaPhong, uidKia,
                                            (o, e) => { noiDuoc = o; loi = e; });
        else
            yield return BatTay.NguoiVaoNhan(TranHienTai.MaPhong, uidKia,
                                             (o, e) => { noiDuoc = o; loi = e; });

        if (!noiDuoc) { Hong(loi ?? "không nối được"); yield break; }

        float matBaoLau = Time.unscaledTime - batDauLuc;

        // ---- 4. Sinh nhan vat cho nguoi kia ----
        Vector3 cho = toi.transform.position + new Vector3(4f, 0f, 0f);
        var nguoiKia = NguoiChoiKhac.Sinh(uidKia, "Người chơi 2", cho);
        if (nguoiKia == null) { Hong("khong sinh duoc nhan vat cho nguoi kia"); yield break; }

        // ---- 4b. Cho ca hai nhan vat ghi lai duong di ----
        // Bu tre can biet "mot khoang truoc day nguoi nay dung o dau". Gan cho
        // CA HAI: nhan vat cua minh la nan nhan can lui, con ban sao nguoi kia
        // thi phep cua chinh ho khong duoc lui - BuTre.Mo tu bo qua nguoi tung.
        if (toi.GetComponent<LichSuViTri>() == null)
            toi.gameObject.AddComponent<LichSuViTri>();
        if (nguoiKia.GetComponent<LichSuViTri>() == null)
            nguoiKia.gameObject.AddComponent<LichSuViTri>();

        // ---- 5. Bat dong bo ----
        dongBo = gameObject.AddComponent<DongBoTran>();
        dongBo.toi = toi;

        // Phai goi TAY sau khi gan "toi": OnEnable cua DongBoTran da chay xong
        // tu luc AddComponent, luc ay "toi" con la null nen no khong nghe duoc
        // ai ca - va ky nang se khong bao gio duoc gui sang may kia.
        dongBo.GanTaiNghe();

        // Ca hai nguoi danh chung mot dan quai: chu phong ke lai, khach nghe.
        var boQuai = gameObject.AddComponent<DongBoQuai>();
        boQuai.dongBo = dongBo;
        dongBo.quai = boQuai;

        // Chu phong la 0, nguoi vao la 1. Hai nguoi thi the la du - bon nguoi
        // moi can danh so theo thu tu trong phong.
        dongBo.chiSoCuaToi = (byte)(TranHienTai.LaHost ? 0 : 1);
        chiSoKia = (byte)(TranHienTai.LaHost ? 1 : 0);
        dongBo.ThemNguoi(chiSoKia, nguoiKia);
        dongBo.KhiMatKetNoi += KhiNguoiKiaRoiTran;

        DaNoi = true;
        TrangThai = string.Format("Đã nối! (bắt tay mất {0:F1} giây)", matBaoLau);

        // Ba giay sau thi thoi bao, tra man hinh lai cho game
        yield return new WaitForSecondsRealtime(3f);
        TrangThai = "";
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
                TranHienTai.LaHost ? "người chơi kia" : "chủ phòng", dongBo.ImLangGiay);
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
    /// Nguoi kia da roi tran han. Hai phia xu ly khac nhau, vi hai phia mat
    /// hai thu khac nhau:
    ///
    ///   - CHU PHONG mat nguoi khach: tran van chay binh thuong - quai, nhip
    ///     dot, moi thu deu nam o may nay. Chi can go ban sao cua ho di de quai
    ///     thoi duoi theo mot cai bong, roi choi tiep mot minh.
    ///   - NGUOI KHACH mat chu phong: mat LUON ca tran - quai do chu phong dieu
    ///     khien, mau quai do chu phong tinh. Khong con gi de choi tiep. Phai
    ///     noi thang ra va dua cho nguoi ta mot duong ve.
    /// </summary>
    void KhiNguoiKiaRoiTran()
    {
        if (dongBo != null) dongBo.BoNguoi(chiSoKia);

        if (TranHienTai.LaHost)
        {
            ThongBaoKetNoi = "Người chơi kia đã rời trận. Bạn chơi tiếp một mình.";
            thongBaoLaLoi = false;
            tatThongBaoLuc = Time.unscaledTime + 6f;
        }
        else
        {
            ThongBaoKetNoi = "Chủ phòng đã rời trận — trận đấu dừng tại đây."
                + System.Environment.NewLine
                + (CamUng.DangDung ? "Bấm TRỞ VỀ để về sảnh." : "Bấm ESC để về sảnh.");
            thongBaoLaLoi = true;
            CanNutVeSanh = true;
        }

        Debug.LogWarning("[TranMang] nguoi kia da roi tran");
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
            kieuNho.fontSize = Mathf.RoundToInt(15f * s);
            kieuNho.normal.textColor = new Color(0.70f, 0.70f, 0.74f, 0.85f);
            GUI.Label(new Rect(10f * s, Screen.height - 26f * s, 520f * s, 22f * s),
                      NhanDang, kieuNho);
        }

        if (!string.IsNullOrEmpty(ThongBaoKetNoi))
        {
            var kieuKN = new GUIStyle(GUI.skin.label);
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
