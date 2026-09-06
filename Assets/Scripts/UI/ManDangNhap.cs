using System.Collections;
using UnityEngine;

/// <summary>
/// MAN DANG NHAP / DANG KY - ve bang OnGUI ngay trong game.
///
/// Nguoi choi khong phai roi game de vao trang web nao ca: mo game len la
/// dang ky, dang nhap, roi vao thang sanh phong.
///
/// Trang web chi con mot viec duy nhat: cho admin quan ly tai khoan.
///
/// Ban Editor va ban PC deu chay duoc vi tat ca di qua REST API - xem
/// <see cref="FirebaseMang"/>.
/// </summary>
public class ManDangNhap : MonoBehaviour
{
    const float Ref = 1080f;

    public enum Trang { DangNhap, DangKy }

    Trang trang = Trang.DangNhap;
    string email = "", matKhau = "", ten = "";
    string bao = "";
    bool dangCho;
    bool daThuTuDangNhap;

    GUIStyle kieuTieuDe, kieuNhan, kieuNut, kieuNutPhu, kieuO, kieuBao, kieuTab;
    Texture2D nenMo;

    /// <summary>Goi khi dang nhap xong - ManSanh se nghe cai nay.</summary>
    public System.Action daVao;

    void Start()
    {
        nenMo = Mau(new Color(0f, 0f, 0f, 0.62f));

        // Con phien tu lan truoc thi vao thang, khong bat go lai mat khau
        if (FirebaseMang.CoTheTuDangNhapLai && !FirebaseMang.DaDangNhap)
            StartCoroutine(ThuTuDangNhap());
    }

    IEnumerator ThuTuDangNhap()
    {
        daThuTuDangNhap = true;
        dangCho = true;
        bao = "Dang khoi phuc phien truoc...";

        bool ok = false; string loi = null;
        yield return FirebaseMang.TuDangNhapLai((o, e) => { ok = o; loi = e; });

        if (ok)
        {
            yield return HoSoMang.TaiHoacTao(null, (o2, e2) => { ok = o2; loi = e2; });
        }

        dangCho = false;
        if (ok) { bao = ""; if (daVao != null) daVao(); }
        else { bao = loi ?? ""; FirebaseMang.Quen(); }
    }

    // ================================================================

    void EnsureStyles(float s)
    {
        if (kieuTieuDe != null) { CapNhatCo(s); return; }

        kieuTieuDe = new GUIStyle(GUI.skin.label);
        kieuTieuDe.fontStyle = FontStyle.Bold;
        kieuTieuDe.alignment = TextAnchor.MiddleCenter;
        kieuTieuDe.normal.textColor = new Color(0.85f, 0.15f, 0.08f);

        kieuNhan = new GUIStyle(GUI.skin.label);
        kieuNhan.normal.textColor = new Color(0.70f, 0.70f, 0.72f);

        kieuNut = new GUIStyle(GUI.skin.button);
        kieuNut.fontStyle = FontStyle.Bold;

        kieuNutPhu = new GUIStyle(GUI.skin.button);

        kieuO = new GUIStyle(GUI.skin.textField);

        kieuBao = new GUIStyle(GUI.skin.label);
        kieuBao.alignment = TextAnchor.MiddleCenter;
        kieuBao.wordWrap = true;

        kieuTab = new GUIStyle(GUI.skin.button);
        kieuTab.fontStyle = FontStyle.Bold;

        CapNhatCo(s);
    }

    void CapNhatCo(float s)
    {
        kieuTieuDe.fontSize = Mathf.RoundToInt(64f * s);
        kieuNhan.fontSize   = Mathf.RoundToInt(20f * s);
        kieuNut.fontSize    = Mathf.RoundToInt(26f * s);
        kieuNutPhu.fontSize = Mathf.RoundToInt(20f * s);
        kieuO.fontSize      = Mathf.RoundToInt(24f * s);
        kieuBao.fontSize    = Mathf.RoundToInt(20f * s);
        kieuTab.fontSize    = Mathf.RoundToInt(22f * s);
    }

    void OnGUI()
    {
        if (FirebaseMang.DaDangNhap) return;

        float s = Screen.height / Ref;
        EnsureStyles(s);

        float rong = Mathf.Min(Screen.width * 0.86f, 620f * s);
        float x = (Screen.width - rong) * 0.5f;
        float y = Screen.height * 0.16f;

        GUI.Label(new Rect(0f, y, Screen.width, 90f * s), "DIABLO 2.5D", kieuTieuDe);
        y += 105f * s;

        GUI.DrawTexture(new Rect(x - 18f * s, y - 14f * s, rong + 36f * s, 470f * s),
                        nenMo, ScaleMode.StretchToFill, true);

        // ---- Hai tab ----
        float rongTab = (rong - 10f * s) * 0.5f;
        var mauCu = GUI.backgroundColor;

        GUI.backgroundColor = trang == Trang.DangNhap ? new Color(0.62f, 0.16f, 0.14f) : mauCu;
        if (GUI.Button(new Rect(x, y, rongTab, 46f * s), "DANG NHAP", kieuTab))
        { trang = Trang.DangNhap; bao = ""; }

        GUI.backgroundColor = trang == Trang.DangKy ? new Color(0.62f, 0.16f, 0.14f) : mauCu;
        if (GUI.Button(new Rect(x + rongTab + 10f * s, y, rongTab, 46f * s), "TAO TAI KHOAN", kieuTab))
        { trang = Trang.DangKy; bao = ""; }

        GUI.backgroundColor = mauCu;
        y += 62f * s;

        GUI.enabled = !dangCho;

        // ---- Ten (chi khi dang ky) ----
        if (trang == Trang.DangKy)
        {
            GUI.Label(new Rect(x, y, rong, 26f * s), "Ten trong game (2-16 ky tu)", kieuNhan);
            y += 28f * s;
            ten = GUI.TextField(new Rect(x, y, rong, 44f * s), ten, 16, kieuO);
            y += 56f * s;
        }

        GUI.Label(new Rect(x, y, rong, 26f * s), "Email", kieuNhan);
        y += 28f * s;
        email = GUI.TextField(new Rect(x, y, rong, 44f * s), email, 64, kieuO);
        y += 56f * s;

        GUI.Label(new Rect(x, y, rong, 26f * s), "Mat khau (it nhat 6 ky tu)", kieuNhan);
        y += 28f * s;
        matKhau = GUI.PasswordField(new Rect(x, y, rong, 44f * s), matKhau, '*', 64, kieuO);
        y += 60f * s;

        string chuNut = dangCho
            ? "DANG CHO..."
            : (trang == Trang.DangKy ? "TAO TAI KHOAN" : "DANG NHAP");

        if (GUI.Button(new Rect(x, y, rong, 54f * s), chuNut, kieuNut) && !dangCho)
            BamNutChinh();

        y += 64f * s;
        GUI.enabled = true;

        if (!string.IsNullOrEmpty(bao))
        {
            kieuBao.normal.textColor = bao.StartsWith("Dang")
                ? new Color(0.75f, 0.75f, 0.78f)
                : new Color(0.90f, 0.42f, 0.40f);
            GUI.Label(new Rect(x, y, rong, 64f * s), bao, kieuBao);
        }

        // Enter de gui - go xong mat khau la bam Enter, khong phai rê chuot
        var e = Event.current;
        if (e.type == EventType.KeyDown
            && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            && !dangCho)
        {
            BamNutChinh();
            e.Use();
        }
    }

    void BamNutChinh()
    {
        if (trang == Trang.DangKy && ten.Trim().Length < 2)
        { bao = "Ten phai tu 2 ky tu tro len."; return; }
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        { bao = "Email khong hop le."; return; }
        if (matKhau.Length < 6)
        { bao = "Mat khau phai tu 6 ky tu tro len."; return; }

        StartCoroutine(trang == Trang.DangKy ? ChayDangKy() : ChayDangNhap());
    }

    IEnumerator ChayDangKy()
    {
        dangCho = true; bao = "Dang tao tai khoan...";
        bool ok = false; string loi = null;

        yield return FirebaseMang.DangKy(email.Trim(), matKhau, (o, e) => { ok = o; loi = e; });

        if (ok)
            yield return HoSoMang.TaiHoacTao(ten.Trim(), (o, e) => { ok = o; loi = e; });

        dangCho = false;
        if (ok) { bao = ""; if (daVao != null) daVao(); }
        else bao = loi ?? "Khong tao duoc tai khoan.";
    }

    IEnumerator ChayDangNhap()
    {
        dangCho = true; bao = "Dang dang nhap...";
        bool ok = false; string loi = null;

        yield return FirebaseMang.DangNhap(email.Trim(), matKhau, (o, e) => { ok = o; loi = e; });

        if (ok)
            yield return HoSoMang.TaiHoacTao(null, (o, e) => { ok = o; loi = e; });

        dangCho = false;
        if (ok) { bao = ""; if (daVao != null) daVao(); }
        else
        {
            bao = loi ?? "Khong dang nhap duoc.";
            // Ho so bao bi khoa thi phai bo phien di, khong giu lai
            if (bao.Contains("bi khoa")) FirebaseMang.Quen();
        }
    }

    static Texture2D Mau(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}
