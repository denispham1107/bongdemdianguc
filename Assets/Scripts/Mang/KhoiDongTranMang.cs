using System.Collections;
using UnityEngine;

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
    public static string LoiCuoi;

    PlayerController toi;
    DongBoTran dongBo;
    float batDauLuc;

    /// <summary>
    /// TU DAT MINH VAO MAN CHOI, khong bat ai keo tay vao scene.
    ///
    /// Act1 dung bang code luc chay con Act2 la scene da nuong san - gan tay
    /// thi phai nho ca hai, va quen mot cai la mot man khong noi mang duoc ma
    /// khong bao gi. Chay sau khi scene da nap xong nen GameBootstrap da kip
    /// dung nhan vat.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void TuGan()
    {
        if (!TranHienTai.DangChoiMang) return;

        string ten = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (ten != "Act1" && ten != "Act2") return;

        if (Object.FindAnyObjectByType<KhoiDongTranMang>() != null) return;

        var go = new GameObject("TranMang");
        go.AddComponent<KhoiDongTranMang>();
    }

    void Start()
    {
        TrangThai = ""; DaNoi = false; LoiCuoi = null;

        // Choi mot minh thi khong lam gi ca - va phai tu bo di, khong nam lai
        // an bo nho cho mot viec khong bao gio xay ra.
        if (!TranHienTai.DangChoiMang) { Destroy(this); return; }

        StartCoroutine(ChayVao());
    }

    IEnumerator ChayVao()
    {
        batDauLuc = Time.unscaledTime;
        TrangThai = "Đang tìm người chơi khác...";

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

        // ---- 5. Bat dong bo ----
        dongBo = gameObject.AddComponent<DongBoTran>();
        dongBo.toi = toi;

        // Chu phong la 0, nguoi vao la 1. Hai nguoi thi the la du - bon nguoi
        // moi can danh so theo thu tu trong phong.
        dongBo.chiSoCuaToi = (byte)(TranHienTai.LaHost ? 0 : 1);
        dongBo.ThemNguoi((byte)(TranHienTai.LaHost ? 1 : 0), nguoiKia);

        DaNoi = true;
        TrangThai = string.Format("Đã nối! (bắt tay mất {0:F1} giây)", matBaoLau);

        // Ba giay sau thi thoi bao, tra man hinh lai cho game
        yield return new WaitForSecondsRealtime(3f);
        TrangThai = "";
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
        if (string.IsNullOrEmpty(TrangThai)) return;

        float s = Screen.height / 1080f;
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
