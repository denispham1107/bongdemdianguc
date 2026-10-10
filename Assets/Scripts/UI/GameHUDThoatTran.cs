using UnityEngine;

/// <summary>
/// NUT THOAT TRAN (10/10/2026, nguoi dung): nut hinh CANH CUA HAM MO HE MO + MUI TEN MAU chi ra ngoai o DUNG GOC PHAI TREN
/// (ca ban cam ung lan ban may tinh). Con mat (khoa goc nhin) va Sach phep don xuong mot nac (88 don vi) de nhuong cho.
///
/// Bam vao -> BANG XAC NHAN "THOAT TRAN?" (khong thoat ngay - bam nham o goc man hinh la mat ca tran). Dong y thi
/// <see cref="GameDirector.BackToMenu"/> - y het duong ESC cu: choi mang thi dong kenh truoc de may kia biet ngay minh da di.
/// Nguoi dung chon (10/10/2026): ban may tinh cung co nut + ESC cung HOI (ESC lan nua = o lai); CHU PHONG con nguoi khac
/// trong tran thi bang them dong canh bao "thoat se ket thuc tran cua moi nguoi" (quai + BOT chay tren may chu phong).
///
/// Anh nut: Blender MCP (CongCu/Blender/nut_thoat_tran.blend, scene "NutThoat") -> Resources/GiaoDien/ThoatTran.png:
/// vom da nut reu + vet mau, canh cua go muc dai sat dinh tan he mo, long ham anh do + doi mat vang, suong tran ra, mui ten mau.
///
/// Bang dang mo thi KHOA HET INPUT TRAN (y nhu Sach phep): DocInput tra goi rong, nut / thanh ky nang thoi nhan bam.
/// Da chet / het tran thi ESC ve sanh NGAY nhu cu (man hinh luc ay da ghi "Bam ESC de ve sanh").
/// </summary>
public partial class GameHUD
{
    /// <summary>Bang xac nhan thoat tran dang mo.</summary>
    public static bool DangHoiThoat { get; private set; }

    /// <summary>Input tran dau dang bi khoa boi mot bang (Sach phep hoac bang thoat tran).</summary>
    public static bool KhoaInputTran { get { return CuaSoSachPhep.DangMo || DangHoiThoat; } }

    Texture2D anhThoatTran;
    GUIStyle kieuHoiThoat, kieuHoiThoatTieuDe, kieuHoiThoatCanhBao;

    /// <summary>Khoang cach doc giua cac nut cot goc phai tren.</summary>
    public const float KhoangCotGoc = 88f;

    public float BanKinhNutThoat(float s) { return 40f * s; }

    /// <summary>Tam nut thoat tran theo toa do CHAM (y tu duoi len) - dung goc phai tren.</summary>
    public static Vector2 TamNutThoatTinh(float s) { return new Vector2(Screen.width - 62f * s, Screen.height - 62f * s); }
    public Vector2 TamNutThoat(float s) { return TamNutThoatTinh(s); }

    /// <summary>Tam nut Sach phep ban MAY TINH (duoi nut thoat). Ban cam ung: duoi con mat.</summary>
    public static Vector2 TamNutSachPhepMayTinh(float s)
    {
        var t = TamNutThoatTinh(s);
        return new Vector2(t.x, t.y - KhoangCotGoc * s);
    }

    /// <summary>
    /// Con tro chuot (ban may tinh) dang nam tren mot nut cot goc phai tren - DocInput khong cho nhan vat chay ve
    /// phia goc man hinh khi nguoi choi bam nut.
    /// </summary>
    public static bool ConTroTrenNutGoc(Vector2 chuot)
    {
        float s = Screen.height / Ref;
        float r = 46f * s;
        return Vector2.Distance(chuot, TamNutThoatTinh(s)) <= r
            || Vector2.Distance(chuot, TamNutSachPhepMayTinh(s)) <= r;
    }

    bool BamNutThoat(Vector2 diem, float s)
    {
        if (Vector2.Distance(diem, TamNutThoat(s)) > BanKinhNutThoat(s)) return false;
        MoHoiThoat();
        return true;
    }

    /// <summary>Mo bang xac nhan. Dong Sach phep neu dang mo (hai bang khong chong nhau).</summary>
    public void MoHoiThoat()
    {
        if (CuaSoSachPhep.DangMo) CuaSoSachPhep.Dong();
        DangHoiThoat = true;
    }

    public static void DongHoiThoat() { DangHoiThoat = false; }

    /// <summary>Phep thu (menu 118) goi de dong y thoat ma khong can bam.</summary>
    public static void DongYThoat()
    {
        DangHoiThoat = false;
        GameDirector.BackToMenu();
    }

    /// <summary>
    /// Phim ESC trong tran. Tra ve true neu da xu ly (GameDirector khong xu ly them).
    ///   bang thoat dang mo -> o lai; Sach phep mo -> dong sach; da chet / het tran -> ve sanh ngay (nhu cu); dang choi -> hoi.
    /// </summary>
    void XuLyEsc()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (DangHoiThoat) { DangHoiThoat = false; return; }
        if (CuaSoSachPhep.DangMo) { CuaSoSachPhep.Dong(); return; }
        if (KetTran.DaXong || (director != null && director.PlayerDead)) { GameDirector.BackToMenu(); return; }
        MoHoiThoat();
    }

    /// <summary>Ve nut thoat tran (vong nen + anh cua ham mo). Ban may tinh bat cu bam ngay tai day.</summary>
    void VeNutThoat(float s)
    {
        Vector2 t = TamNutThoat(s);
        float r = BanKinhNutThoat(s);
        float gx = t.x, gy = Screen.height - t.y;

        var cu = GUI.color;
        GUI.color = DangHoiThoat ? new Color(1f, 0.55f, 0.45f, 0.30f) : new Color(1f, 1f, 1f, 0.20f);
        GUI.DrawTexture(new Rect(gx - r, gy - r, r * 2f, r * 2f), vongNen, ScaleMode.StretchToFill, true);

        if (anhThoatTran != null)
        {
            float kt = r * 1.9f;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(gx - kt * 0.5f, gy - kt * 0.5f, kt, kt), anhThoatTran, ScaleMode.ScaleToFit, true);
        }
        else
        {
            // Khong nap duoc anh: van phai thay mot cai nut - khung cua + mui ten bang hinh chu nhat
            GUI.color = new Color(0.45f, 0.30f, 0.20f, 0.95f);
            GUI.DrawTexture(new Rect(gx - r * 0.45f, gy - r * 0.6f, r * 0.6f, r * 1.2f), Texture2D.whiteTexture);
            GUI.color = new Color(0.85f, 0.08f, 0.05f, 1f);
            GUI.DrawTexture(new Rect(gx, gy - r * 0.08f, r * 0.6f, r * 0.16f), Texture2D.whiteTexture);
        }
        GUI.color = cu;

        // Ban may tinh: bat cu bam bang IMGUI (ban cam ung co duong doc ngon rieng - xem DocCamUng)
        if (!CamUng.DangDung && !KhoaInputTran
            && GUI.Button(new Rect(gx - r, gy - r, r * 2f, r * 2f), GUIContent.none, GUIStyle.none))
            MoHoiThoat();
    }

    /// <summary>So nguoi choi THAT khac con trong tran (khong tinh BOT) - de canh bao chu phong.</summary>
    static int SoNguoiKhacTrongTran()
    {
        if (!TranHienTai.DangChoiMang) return 0;
        var db = Object.FindFirstObjectByType<DongBoTran>();
        return db != null ? db.SoNguoiKhac : 0;
    }

    /// <summary>Bang xac nhan giua man hinh - goi CUOI OnGUI de nam tren moi thu cua HUD.</summary>
    void VeHoiThoat(float s)
    {
        if (!DangHoiThoat) return;
        GiaoDien.ChuanBi();

        if (kieuHoiThoat == null)
        {
            kieuHoiThoat = new GUIStyle(GUI.skin.label);
            kieuHoiThoat.alignment = TextAnchor.MiddleCenter;
            kieuHoiThoat.wordWrap = true;
            kieuHoiThoat.padding = new RectOffset(0, 0, 0, 0);
            kieuHoiThoatTieuDe = new GUIStyle(kieuHoiThoat);
            kieuHoiThoatCanhBao = new GUIStyle(kieuHoiThoat);
        }
        kieuHoiThoat.font = GiaoDien.ChuThuong;
        kieuHoiThoat.fontSize = Mathf.RoundToInt(24f * s);
        kieuHoiThoat.normal.textColor = GiaoDien.MauGiay;
        kieuHoiThoatTieuDe.font = GiaoDien.ChuDam;
        kieuHoiThoatTieuDe.fontSize = Mathf.RoundToInt(44f * s);
        kieuHoiThoatTieuDe.normal.textColor = new Color(0.93f, 0.16f, 0.10f);
        kieuHoiThoatCanhBao.font = GiaoDien.ChuDam;
        kieuHoiThoatCanhBao.fontSize = Mathf.RoundToInt(22f * s);
        kieuHoiThoatCanhBao.normal.textColor = new Color(1f, 0.62f, 0.30f);

        // Lam toi ca man hinh - va chan moi cu bam xuyen qua bang
        var cu = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.62f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = cu;

        bool chuPhongCanhBao = TranHienTai.DangChoiMang && TranHienTai.LaHost && SoNguoiKhacTrongTran() > 0;
        float w = Mathf.Min(Screen.width - 40f * s, 760f * s);
        float h = (chuPhongCanhBao ? 400f : 340f) * s;
        var khung = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
        GiaoDien.Khung(khung, s, true, 0.92f);

        float y = khung.y + 52f * s;
        GUI.Label(new Rect(khung.x, y, w, 56f * s), "THOÁT TRẬN?", kieuHoiThoatTieuDe);
        y += 64f * s;
        GiaoDien.DuongKe(new Rect(khung.center.x - w * 0.32f, y, w * 0.64f, Mathf.Max(1f, 2f * s)), new Color(0.8f, 0.1f, 0.06f, 0.9f));
        y += 14f * s;
        GUI.Label(new Rect(khung.x + 30f * s, y, w - 60f * s, 70f * s),
                  "Bạn sẽ rời trận đấu và trở về sảnh. Cấp độ và kinh nghiệm của trận này sẽ không được giữ lại.", kieuHoiThoat);
        y += 74f * s;
        if (chuPhongCanhBao)
        {
            GUI.Label(new Rect(khung.x + 30f * s, y, w - 60f * s, 56f * s),
                      "Bạn là chủ phòng — thoát sẽ kết thúc trận của mọi người.", kieuHoiThoatCanhBao);
            y += 60f * s;
        }

        // Hai nut: THOAT (mau) - O LAI (da)
        float nutW = Mathf.Min(260f * s, (w - 90f * s) * 0.5f), nutH = 72f * s, khe = 30f * s;
        float nx = khung.center.x - nutW - khe * 0.5f;
        var oThoat = new Rect(nx, khung.yMax - nutH - 34f * s, nutW, nutH);
        var oOLai = new Rect(nx + nutW + khe, oThoat.y, nutW, nutH);
        if (GiaoDien.Nut(oThoat, "THOÁT TRẬN", GiaoDien.KieuNutMau)) { DongYThoat(); return; }
        if (GiaoDien.Nut(oOLai, "Ở LẠI", GiaoDien.KieuNutDa)) DangHoiThoat = false;

        // An het cu bam chuot con lai de khong lot xuong cac nut ben duoi. KHONG lay "bam ra ngoai = o lai": tren WebGL
        // cu cham mo bang (o goc phai tren) con sinh them su kien chuot ngay khung ay - bang vua mo da tu dong.
        var e = Event.current;
        if (e != null && (e.type == EventType.MouseDown || e.type == EventType.MouseUp)) e.Use();
    }
}
