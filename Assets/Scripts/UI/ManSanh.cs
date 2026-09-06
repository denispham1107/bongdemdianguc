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

    GUIStyle kieuTieuDe, kieuNhan, kieuNut, kieuNutNho, kieuO, kieuBao, kieuDem, kieuHang;
    Texture2D nenMo, nenHang;

    void Start()
    {
        nenMo = Mau(new Color(0f, 0f, 0f, 0.62f));
        nenHang = Mau(new Color(1f, 1f, 1f, 0.05f));
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

        if (PhongMang.LaHost)
            yield return PhongMang.DanhDauDangChoi(null);

        // Giu lai thong tin phong de man choi biet minh dang o dau
        TranHienTai.MaPhong = p.ma;
        TranHienTai.ManChoi = p.manChoi;
        TranHienTai.LaHost = PhongMang.LaHost;
        TranHienTai.DangChoiMang = true;

        SceneManager.LoadScene(p.manChoi);
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

        if (dangO == Cho.Sanh) VeSanh(s);
        else VeTrongPhong(s);
    }

    // ---------------- NGOAI SANH ----------------

    void VeSanh(float s)
    {
        float rong = Mathf.Min(Screen.width * 0.92f, 900f * s);
        float x = (Screen.width - rong) * 0.5f;
        float y = 24f * s;

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

        GUI.enabled = !dangCho;
        if (GUI.Button(new Rect(x + rong - 200f * s, yy, 186f * s, 42f * s), "TAO PHONG", kieuNut))
            StartCoroutine(ChayTaoPhong());
        GUI.enabled = true;

        y += 144f * s;

        // ---- Danh sach ----
        GUI.Label(new Rect(x, y, 300f * s, 32f * s), "Phong dang cho", kieuTieuDe);

        GUI.enabled = !dangCho;
        if (GUI.Button(new Rect(x + rong - 220f * s, y, 220f * s, 38f * s),
                       "VAO PHONG NHANH", kieuNutNho))
            StartCoroutine(ChayVaoNhanh());
        GUI.enabled = true;

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

                GUI.enabled = !dangCho && p.ConCho;
                if (GUI.Button(new Rect(vungTrong.width - 200f * s, yh + 8f * s, 190f * s, 38f * s),
                               p.ConCho ? "VAO PHONG" : "DA DAY", kieuNutNho))
                    StartCoroutine(ChayVaoPhong(p.ma));
                GUI.enabled = true;

                yh += caoHang + 8f * s;
            }
            GUI.EndScrollView();
        }

        if (!string.IsNullOrEmpty(bao))
            GUI.Label(new Rect(x, Screen.height - 60f * s, rong, 50f * s), bao, kieuBao);
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
