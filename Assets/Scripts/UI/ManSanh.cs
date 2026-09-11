using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SANH PHONG NGAY TRONG GAME - ve bang OnGUI.
///
/// Nguoi choi tao phong, xem danh sach phong nguoi khac tao, vao phong, bam
/// san sang. Host bam bat dau thi ca phong dem nguoc 10 giay roi cung nap man
/// choi. Host duoc bat dau KE CA khi chi co mot minh.
///
/// HOI LAI THEO NHIP chu khong co luong day ve: REST API khong co streaming.
/// Ngoai sanh hoi moi hai giay, trong phong hoi moi giay - nguoi dang cho thi
/// sot ruot hon. So do nay o <see cref="PhongMang.NhipHoiSanh"/>.
/// </summary>
public class ManSanh : MonoBehaviour
{
    const float Ref = 1080f;

    enum Cho { Sanh, TrongPhong }
    Cho dangO = Cho.Sanh;

    List<PhongMang.Phong> danhSach = new List<PhongMang.Phong>();
    string tenPhongMoi = "";
    string manChoiMoi = "Act2";
    string bao = "";
    bool dangCho;
    bool daVaoTran;
    Vector2 cuonDanhSach;

    float hoiLanSau;

    // ---- Bang CAI DAT ----
    // Nhieu tab, hien gio mot tab. Them tab thi them ten vao day va mot nhanh
    // trong VeCaiDat.
    static readonly string[] TenTabCaiDat = { "Giao diện" };
    bool moCaiDat;
    int tabCaiDat;
    MucDoHoa mucChon;

    GUIStyle kieuTieuDe, kieuNhan, kieuNut, kieuNutNho, kieuO, kieuBao, kieuDem, kieuHang;
    GUIStyle kieuLuaChon, kieuMoTa;
    Texture2D nenMo, nenHang, nenBang, nenChon, nenVach;

    void Start()
    {
        nenMo = Mau(new Color(0f, 0f, 0f, 0.62f));
        nenHang = Mau(new Color(1f, 1f, 1f, 0.05f));
        nenBang = Mau(new Color(0.07f, 0.07f, 0.09f, 0.97f));
        nenChon = Mau(new Color(0.85f, 0.20f, 0.10f, 0.35f));
        nenVach = Mau(new Color(0.85f, 0.20f, 0.10f, 1f));
    }

    void Update()
    {
        if (!FirebaseMang.DaDangNhap) return;

        // Dem nguoc xong thi vao tran - hoac host da vao truoc mot nhip
        // (xem PhongMang.DenGioVaoTran de biet vi sao phai xet ca hai)
        if (dangO == Cho.TrongPhong && !daVaoTran
            && PhongMang.DenGioVaoTran(PhongMang.PhongHienTai, PhongMang.ConLaiGiay()))
        {
            daVaoTran = true;
            StartCoroutine(VaoTran());
            return;
        }

        if (Time.unscaledTime < hoiLanSau) return;
        hoiLanSau = Time.unscaledTime
                  + (dangO == Cho.Sanh ? PhongMang.NhipHoiSanh : PhongMang.NhipHoiTrongPhong);
        StartCoroutine(dangO == Cho.Sanh ? HoiDanhSach() : HoiPhong());
    }

    IEnumerator HoiDanhSach()
    {
        yield return PhongMang.LayDanhSach(ds => danhSach = ds);
    }

    IEnumerator HoiPhong()
    {
        if (PhongMang.PhongHienTai == null) yield break;
        string ma = PhongMang.PhongHienTai.ma;

        // Chu phong dap nhip de sanh biet phong nay con song - xem
        // PhongMang.DapNhip. Nguoi khac khong dap: phong la cua chu phong.
        if (PhongMang.LaHost) yield return PhongMang.DapNhip();

        bool con = true;
        yield return PhongMang.TaiLaiPhong(ma, (ok, loi) => con = ok);

        if (!con)
        {
            // Host giai tan phong hoac mat mang
            dangO = Cho.Sanh;
            bao = "Phong da dong.";
        }
    }

    IEnumerator VaoTran()
    {
        var p = PhongMang.PhongHienTai;
        if (p == null) yield break;
        string ma = p.ma;

        // DOC LAI PHONG MOT LAN NUA NGAY TRUOC KHI NAP MAN.
        //
        // Ban sao trong tay co the da mot giay tuoi (sanh hoi lai moi giay).
        // Mot giay la du de chu phong doi man ma may nay chua biet - va the la
        // hai nguoi nap hai man khac nhau. Doc lai ton them mot vong di-ve
        // (~50 ms tren Firebase Singapore), doi lai chac chan cung man.
        yield return PhongMang.TaiLaiPhong(ma, (ok, loi) => { });
        if (PhongMang.PhongHienTai != null) p = PhongMang.PhongHienTai;

        if (PhongMang.LaHost)
            yield return PhongMang.DanhDauDangChoi(null);

        // Man khong doc duoc thi ve man mac dinh chu khong goi LoadScene(null):
        // LoadScene(null) nem loi va nguoi choi ket lai o MainMenu khong hieu vi sao.
        string man = p.manChoi == "Act1" || p.manChoi == "Act2"
                   ? p.manChoi : PhongMang.ManMacDinh;

        // Giu lai thong tin phong de man choi biet minh dang o dau
        TranHienTai.MaPhong = ma;
        TranHienTai.ManChoi = man;
        TranHienTai.LaHost = PhongMang.LaHost;
        TranHienTai.DangChoiMang = true;

        SceneManager.LoadScene(man);
    }

    // ================================================================

    void EnsureStyles(float s)
    {
        if (kieuTieuDe != null) { CapNhatCo(s); return; }

        kieuTieuDe = new GUIStyle(GUI.skin.label);
        kieuTieuDe.fontStyle = FontStyle.Bold;
        kieuTieuDe.normal.textColor = new Color(0.88f, 0.86f, 0.80f);

        kieuNhan = new GUIStyle(GUI.skin.label);
        kieuNhan.normal.textColor = new Color(0.70f, 0.70f, 0.72f);

        kieuNut = new GUIStyle(GUI.skin.button);
        kieuNut.fontStyle = FontStyle.Bold;

        kieuNutNho = new GUIStyle(GUI.skin.button);

        kieuO = new GUIStyle(GUI.skin.textField);

        kieuBao = new GUIStyle(GUI.skin.label);
        kieuBao.wordWrap = true;
        kieuBao.normal.textColor = new Color(0.90f, 0.55f, 0.40f);

        kieuDem = new GUIStyle(GUI.skin.label);
        kieuDem.fontStyle = FontStyle.Bold;
        kieuDem.alignment = TextAnchor.MiddleCenter;
        kieuDem.normal.textColor = Color.white;

        kieuHang = new GUIStyle(GUI.skin.label);
        kieuHang.normal.textColor = new Color(0.85f, 0.84f, 0.80f);

        // Moi lua chon do hoa la mot nut chu can trai; muc dang chon co nen do
        // ve rieng phia sau (nut mac dinh luc bat/tat trong gan nhu nhau).
        kieuLuaChon = new GUIStyle(GUI.skin.button);
        kieuLuaChon.alignment = TextAnchor.MiddleLeft;
        kieuLuaChon.fontStyle = FontStyle.Bold;

        kieuMoTa = new GUIStyle(GUI.skin.label);
        kieuMoTa.alignment = TextAnchor.MiddleLeft;
        kieuMoTa.wordWrap = true;
        kieuMoTa.normal.textColor = new Color(0.70f, 0.70f, 0.72f);

        CapNhatCo(s);
    }

    void CapNhatCo(float s)
    {
        kieuTieuDe.fontSize = Mathf.RoundToInt(30f * s);
        kieuNhan.fontSize   = Mathf.RoundToInt(19f * s);
        kieuNut.fontSize    = Mathf.RoundToInt(24f * s);
        kieuNutNho.fontSize = Mathf.RoundToInt(19f * s);
        kieuO.fontSize      = Mathf.RoundToInt(22f * s);
        kieuBao.fontSize    = Mathf.RoundToInt(20f * s);
        kieuDem.fontSize    = Mathf.RoundToInt(150f * s);
        kieuHang.fontSize   = Mathf.RoundToInt(21f * s);
        kieuLuaChon.fontSize = Mathf.RoundToInt(22f * s);
        kieuLuaChon.padding.left = Mathf.RoundToInt(22f * s);
        kieuMoTa.fontSize   = Mathf.RoundToInt(18f * s);
    }

    void OnGUI()
    {
        if (!FirebaseMang.DaDangNhap) return;

        float s = Screen.height / Ref;
        EnsureStyles(s);

        // Dem nguoc phu kin man hinh
        if (dangO == Cho.TrongPhong && PhongMang.PhongHienTai != null
            && PhongMang.PhongHienTai.trangThai == "demNguoc")
        {
            VeDemNguoc(s);
            return;
        }

        if (dangO != Cho.Sanh) moCaiDat = false;

        if (dangO == Cho.Sanh) VeSanh(s);
        else VeTrongPhong(s);

        // Bang cai dat ve SAU cung de nam tren. Sanh phia duoi da bi khoa
        // (GUI.enabled) trong luc bang mo - IMGUI trao cu bam cho nut nao ve
        // TRUOC, nen khong khoa thi bam vao bang lai trung nut cua sanh nam
        // ngay ben duoi.
        if (moCaiDat) VeCaiDat(s);
    }

    // ---------------- NGOAI SANH ----------------

    void VeSanh(float s)
    {
        float rong = Mathf.Min(Screen.width * 0.92f, 900f * s);
        float x = (Screen.width - rong) * 0.5f;
        float y = 24f * s;

        // Bang cai dat dang mo thi ca sanh bi khoa - xem OnGUI
        bool khoa = moCaiDat;
        GUI.enabled = !khoa;

        GUI.Label(new Rect(x, y, rong, 40f * s),
                  FirebaseMang.TenHienThi + "   -   "
                  + HoSoMang.CuaToi.soTranThang + " thang / "
                  + HoSoMang.CuaToi.soTranChoi + " tran", kieuTieuDe);

        if (GUI.Button(new Rect(x + rong - 150f * s, y, 150f * s, 36f * s), "DANG XUAT", kieuNutNho))
        {
            FirebaseMang.Quen();
            danhSach.Clear();
            return;
        }
        y += 52f * s;

        // ---- Tao phong ----
        GUI.DrawTexture(new Rect(x, y, rong, 130f * s), nenMo, ScaleMode.StretchToFill, true);
        float yy = y + 12f * s;

        GUI.Label(new Rect(x + 14f * s, yy, rong, 26f * s), "Tao phong moi", kieuNhan);
        yy += 32f * s;

        // Nut chon man phai du cho "MAN: NGHIA DIA" - 14 ky tu. Rong 150*s thi
        // o man hinh thap (s nho) chu bi cat mat dau va duoi, doc thanh
        // "IAN: NGHIA DI".
        const float RongNutMan = 230f;
        float rongO = rong - (230f + RongNutMan) * s;
        tenPhongMoi = GUI.TextField(new Rect(x + 14f * s, yy, rongO, 42f * s), tenPhongMoi, 24, kieuO);

        if (GUI.Button(new Rect(x + 14f * s + rongO + 10f * s, yy, RongNutMan * s, 42f * s),
                       manChoiMoi == "Act1" ? "MAN: DAU TRUONG" : "MAN: NGHIA DIA", kieuNutNho))
            manChoiMoi = manChoiMoi == "Act1" ? "Act2" : "Act1";

        GUI.enabled = !dangCho && !khoa;
        if (GUI.Button(new Rect(x + rong - 200f * s, yy, 186f * s, 42f * s), "TAO PHONG", kieuNut))
            StartCoroutine(ChayTaoPhong());
        GUI.enabled = !khoa;

        y += 144f * s;

        // ---- Danh sach ----
        GUI.Label(new Rect(x, y, 300f * s, 32f * s), "Phong dang cho", kieuTieuDe);

        // Nut CAI DAT nam ngay ben trai VAO PHONG NHANH, cung hang
        if (GUI.Button(ViTriNutCaiDat(x, y, rong, s), "CÀI ĐẶT", kieuNutNho))
            MoCaiDat();

        GUI.enabled = !dangCho && !khoa;
        if (GUI.Button(ViTriNutVaoNhanh(x, y, rong, s),
                       "VAO PHONG NHANH", kieuNutNho))
            StartCoroutine(ChayVaoNhanh());
        GUI.enabled = !khoa;

        y += 46f * s;

        var dangCho2 = danhSach.FindAll(p => p.DangCho);
        float caoVung = Screen.height - y - 70f * s;

        if (dangCho2.Count == 0)
        {
            GUI.Label(new Rect(x, y + 20f * s, rong, 40f * s),
                      "Chua co phong nao. Hay tao mot phong, hoac bam VAO PHONG NHANH.", kieuNhan);
        }
        else
        {
            float caoHang = 56f * s;
            var vungTrong = new Rect(0, 0, rong - 24f * s, dangCho2.Count * (caoHang + 8f * s));
            cuonDanhSach = GUI.BeginScrollView(new Rect(x, y, rong, caoVung),
                                               cuonDanhSach, vungTrong);
            float yh = 0f;
            foreach (var p in dangCho2)
            {
                GUI.DrawTexture(new Rect(0, yh, vungTrong.width, caoHang), nenHang,
                                ScaleMode.StretchToFill, true);

                GUI.Label(new Rect(12f * s, yh + 8f * s, vungTrong.width - 320f * s, 34f * s),
                          p.ten + "   [" + (p.manChoi == "Act1" ? "Dau truong" : "Nghia dia") + "]"
                          + "   chu phong: " + p.hostTen, kieuHang);

                GUI.Label(new Rect(vungTrong.width - 300f * s, yh + 8f * s, 90f * s, 34f * s),
                          p.soNguoi + "/" + (p.toiDa <= 0 ? PhongMang.SoNguoiToiDa : p.toiDa),
                          kieuHang);

                GUI.enabled = !dangCho && p.ConCho && !khoa;
                if (GUI.Button(new Rect(vungTrong.width - 200f * s, yh + 8f * s, 190f * s, 38f * s),
                               p.ConCho ? "VAO PHONG" : "DA DAY", kieuNutNho))
                    StartCoroutine(ChayVaoPhong(p.ma));
                GUI.enabled = !khoa;

                yh += caoHang + 8f * s;
            }
            GUI.EndScrollView();
        }

        if (!string.IsNullOrEmpty(bao))
            GUI.Label(new Rect(x, Screen.height - 60f * s, rong, 50f * s), bao, kieuBao);

        GUI.enabled = true;
    }

    /// <summary>Vi tri nut VAO PHONG NHANH - sat le phai cua hang "Phong dang cho".</summary>
    public static Rect ViTriNutVaoNhanh(float x, float y, float rong, float s)
    {
        return new Rect(x + rong - 220f * s, y, 220f * s, 38f * s);
    }

    /// <summary>Vi tri nut CAI DAT - ngay ben trai VAO PHONG NHANH, cach 12 diem.</summary>
    public static Rect ViTriNutCaiDat(float x, float y, float rong, float s)
    {
        var nhanh = ViTriNutVaoNhanh(x, y, rong, s);
        return new Rect(nhanh.x - 12f * s - 160f * s, y, 160f * s, 38f * s);
    }

    // ---------------- CAI DAT ----------------

    string baoCaiDat = "";
    bool dangNapLai;

    static readonly string[] MoTaMucDoHoa =
    {
        "100% độ phân giải · bóng mềm · khử răng cưa",
        "75% độ phân giải · bóng cứng · tắt khử răng cưa",
        "50% độ phân giải · tắt bóng · cho máy yếu",
    };

    /// <summary>Mo bang cai dat - nut CAI DAT goi, phep thu (menu 48) cung goi.</summary>
    public void MoCaiDat()
    {
        moCaiDat = true;
        tabCaiDat = 0;
        mucChon = CaiDatDoHoa.Muc;
        baoCaiDat = "";
        // O ten phong dang giu ban phim thi van go chu vao duoc du da bi khoa
        GUIUtility.keyboardControl = 0;
    }

    public bool DangMoCaiDat { get { return moCaiDat; } }

    public void ChonMucDoHoa(MucDoHoa m) { mucChon = m; baoCaiDat = ""; }

    /// <summary>
    /// Nut OK. Khong doi gi thi chi dong bang - tai lai ca game chi de ve y
    /// het nhu cu la bat nguoi choi doi vo ich. Co doi thi luu roi tai lai.
    /// </summary>
    public void BamOKCaiDat()
    {
        if (mucChon == CaiDatDoHoa.Muc) { moCaiDat = false; return; }

        // Luu hong ma van tai lai thi game khoi dong voi muc CU - nguoi choi
        // thay minh bam OK ma khong co gi xay ra. Bao ro thay vi im lang.
        if (!CaiDatDoHoa.Luu(mucChon))
        {
            baoCaiDat = "Trình duyệt không cho lưu cài đặt (có thể đang ở chế độ ẩn danh).";
            return;
        }
        dangNapLai = true;
        CaiDatDoHoa.NapLaiGame();
    }

    void VeCaiDat(float s)
    {
        // Phu toi ca sanh
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), nenMo, ScaleMode.StretchToFill, true);

        float rong = Mathf.Min(Screen.width * 0.94f, 860f * s);
        float cao = Mathf.Min(Screen.height * 0.94f, 600f * s);
        float x = (Screen.width - rong) * 0.5f;
        float y = (Screen.height - cao) * 0.5f;
        float le = 28f * s;

        GUI.DrawTexture(new Rect(x, y, rong, cao), nenBang, ScaleMode.StretchToFill, true);
        GUI.DrawTexture(new Rect(x, y, rong, 4f * s), nenVach, ScaleMode.StretchToFill, true);

        GUI.enabled = !dangNapLai;

        GUI.Label(new Rect(x + le, y + 18f * s, rong - 2f * le, 42f * s), "CÀI ĐẶT", kieuTieuDe);

        // ---- Hang tab ----
        float yt = y + 74f * s;
        float xt = x + le;
        for (int i = 0; i < TenTabCaiDat.Length; i++)
        {
            var o = new Rect(xt, yt, 200f * s, 46f * s);
            if (GUI.Button(o, TenTabCaiDat[i], kieuNutNho)) tabCaiDat = i;
            if (i == tabCaiDat)
            {
                GUI.DrawTexture(o, nenChon, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(new Rect(o.x, o.yMax, o.width, 4f * s), nenVach,
                                ScaleMode.StretchToFill, true);
            }
            xt += 210f * s;
        }
        GUI.DrawTexture(new Rect(x + le, yt + 50f * s, rong - 2f * le, 1f * s), nenHang,
                        ScaleMode.StretchToFill, true);

        float yn = yt + 66f * s;
        if (tabCaiDat == 0) VeTabGiaoDien(x + le, yn, rong - 2f * le, s);

        // ---- Nut duoi cung ----
        float yb = y + cao - 74f * s;
        string ghiChu = !string.IsNullOrEmpty(baoCaiDat) ? baoCaiDat
                      : dangNapLai ? "Đang tải lại game..."
                      : mucChon != CaiDatDoHoa.Muc ? "Bấm OK: game sẽ tải lại để áp dụng."
                      : "";
        if (ghiChu.Length > 0)
            GUI.Label(new Rect(x + le, yb - 44f * s, rong - 2f * le, 36f * s), ghiChu,
                      string.IsNullOrEmpty(baoCaiDat) ? kieuNhan : kieuBao);

        if (GUI.Button(new Rect(x + rong - le - 180f * s - 14f * s - 180f * s, yb, 180f * s, 52f * s),
                       "HỦY", kieuNutNho))
            moCaiDat = false;

        if (GUI.Button(new Rect(x + rong - le - 180f * s, yb, 180f * s, 52f * s), "OK", kieuNut))
            BamOKCaiDat();

        GUI.enabled = true;
    }

    void VeTabGiaoDien(float x, float y, float rong, float s)
    {
        GUI.Label(new Rect(x, y, rong, 32f * s), "Độ phân giải và chất lượng hình", kieuNhan);
        y += 42f * s;

        float caoHang = 64f * s;
        float rongTen = 290f * s;
        for (int i = 0; i < 3; i++)
        {
            var m = (MucDoHoa)i;
            var o = new Rect(x, y, rong, caoHang);

            string ten = CaiDatDoHoa.Ten[i] + (m == CaiDatDoHoa.Muc ? " (hiện giờ)" : "");
            if (GUI.Button(o, ten, kieuLuaChon)) ChonMucDoHoa(m);

            if (m == mucChon)
            {
                GUI.DrawTexture(o, nenChon, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(new Rect(o.x, o.y, 6f * s, o.height), nenVach,
                                ScaleMode.StretchToFill, true);
            }

            // Mo ta nam de len nut: nhan chu khong an cu bam, bam vao dau
            // tren hang cung trung nut
            GUI.Label(new Rect(o.x + rongTen, o.y, o.width - rongTen - 12f * s, o.height),
                      MoTaMucDoHoa[i], kieuMoTa);

            y += caoHang + 10f * s;
        }
    }

    // ---------------- TRONG PHONG ----------------

    void VeTrongPhong(float s)
    {
        var p = PhongMang.PhongHienTai;
        if (p == null) { dangO = Cho.Sanh; return; }

        float rong = Mathf.Min(Screen.width * 0.9f, 820f * s);
        float x = (Screen.width - rong) * 0.5f;
        float y = 40f * s;

        GUI.Label(new Rect(x, y, rong - 160f * s, 40f * s),
                  p.ten + "   [" + (p.manChoi == "Act1" ? "Dau truong" : "Nghia dia") + "]",
                  kieuTieuDe);

        if (GUI.Button(new Rect(x + rong - 150f * s, y, 150f * s, 36f * s), "ROI PHONG", kieuNutNho))
        { StartCoroutine(ChayRoiPhong()); return; }

        y += 54f * s;

        if (PhongMang.LaHost)
        {
            if (GUI.Button(new Rect(x, y, 260f * s, 40f * s),
                           "DOI MAN: " + (p.manChoi == "Act1" ? "DAU TRUONG" : "NGHIA DIA"),
                           kieuNutNho))
                StartCoroutine(PhongMang.DoiManChoi(p.manChoi == "Act1" ? "Act2" : "Act1", null));
            y += 50f * s;
        }

        // ---- Danh sach nguoi trong phong ----
        float caoHang = 52f * s;
        foreach (var n in p.nguoiChoi)
        {
            GUI.DrawTexture(new Rect(x, y, rong, caoHang), nenHang, ScaleMode.StretchToFill, true);

            string nhan = n.ten;
            if (n.uid == p.hostUid) nhan += "   (CHU PHONG)";
            if (n.uid == FirebaseMang.Uid) nhan += "   <- ban";
            GUI.Label(new Rect(x + 14f * s, y + 10f * s, rong - 320f * s, 32f * s), nhan, kieuHang);

            var mauCu = GUI.color;
            GUI.color = n.sanSang ? new Color(0.35f, 0.85f, 0.45f) : new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(x + rong - 300f * s, y + 10f * s, 160f * s, 32f * s),
                      n.sanSang ? "SAN SANG" : "dang cho", kieuHang);
            GUI.color = mauCu;

            if (PhongMang.LaHost && n.uid != FirebaseMang.Uid)
            {
                if (GUI.Button(new Rect(x + rong - 120f * s, y + 8f * s, 108f * s, 36f * s),
                               "DUOI", kieuNutNho))
                    StartCoroutine(PhongMang.DuoiNguoi(n.uid, null));
            }
            y += caoHang + 8f * s;
        }

        y += 16f * s;

        // ---- Nut san sang / bat dau ----
        var toi = p.nguoiChoi.Find(n => n.uid == FirebaseMang.Uid);
        bool dangSanSang = toi != null && toi.sanSang;

        float rongNut = PhongMang.LaHost ? (rong - 12f * s) * 0.5f : rong;

        if (GUI.Button(new Rect(x, y, rongNut, 54f * s),
                       dangSanSang ? "BO SAN SANG" : "SAN SANG", kieuNut))
            StartCoroutine(PhongMang.DatSanSang(!dangSanSang, null));

        if (PhongMang.LaHost)
        {
            if (GUI.Button(new Rect(x + rongNut + 12f * s, y, rongNut, 54f * s),
                           "BAT DAU", kieuNut))
                StartCoroutine(PhongMang.BatDauDemNguoc(null));
        }

        y += 66f * s;
        GUI.Label(new Rect(x, y, rong, 60f * s),
                  PhongMang.LaHost
                    ? "Ban la chu phong - bam BAT DAU luc nao cung duoc, ke ca khi chi co mot minh."
                    : "Cho chu phong bam bat dau.",
                  kieuNhan);
    }

    // ---------------- DEM NGUOC ----------------

    void VeDemNguoc(float s)
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
                        nenMo, ScaleMode.StretchToFill, true);

        int con = Mathf.Max(0, Mathf.CeilToInt((float)PhongMang.ConLaiGiay()));

        kieuNhan.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(0, Screen.height * 0.32f, Screen.width, 40f * s),
                  "TRAN DAU BAT DAU SAU", kieuNhan);
        GUI.Label(new Rect(0, Screen.height * 0.38f, Screen.width, 190f * s),
                  con.ToString(), kieuDem);
        GUI.Label(new Rect(0, Screen.height * 0.62f, Screen.width, 40f * s),
                  PhongMang.PhongHienTai.manChoi == "Act1" ? "Dau truong" : "Nghia dia", kieuNhan);
        kieuNhan.alignment = TextAnchor.UpperLeft;
    }

    // ---------------- HANH DONG ----------------

    IEnumerator ChayTaoPhong()
    {
        dangCho = true; bao = "";
        bool ok = false; string loi = null;
        yield return PhongMang.TaoPhong(tenPhongMoi.Trim(), manChoiMoi,
                                        (o, e) => { ok = o; loi = e; });
        dangCho = false;
        if (ok) { dangO = Cho.TrongPhong; daVaoTran = false; hoiLanSau = 0f; }
        else bao = loi ?? "Khong tao duoc phong.";
    }

    IEnumerator ChayVaoPhong(string ma)
    {
        dangCho = true; bao = "";
        bool ok = false; string loi = null;
        yield return PhongMang.VaoPhong(ma, (o, e) => { ok = o; loi = e; });
        dangCho = false;
        if (ok) { dangO = Cho.TrongPhong; daVaoTran = false; hoiLanSau = 0f; }
        else bao = loi ?? "Khong vao duoc phong.";
    }

    IEnumerator ChayVaoNhanh()
    {
        dangCho = true; bao = "";
        bool ok = false; string loi = null;
        yield return PhongMang.VaoPhongNhanh((o, e) => { ok = o; loi = e; });
        dangCho = false;
        if (ok) { dangO = Cho.TrongPhong; daVaoTran = false; hoiLanSau = 0f; }
        else bao = loi ?? "Khong vao duoc phong nao.";
    }

    IEnumerator ChayRoiPhong()
    {
        dangCho = true;
        yield return PhongMang.RoiPhong(null);
        dangCho = false;
        dangO = Cho.Sanh;
        hoiLanSau = 0f;
    }

    static Texture2D Mau(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
