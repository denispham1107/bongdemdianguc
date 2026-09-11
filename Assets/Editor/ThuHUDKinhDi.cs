using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU HUD KINH DI TRONG TRAN (<see cref="GameHUD"/>, GameHUDKinhDi.cs).
///
/// Nguoi dung xin: thiet ke lai thong bao + mau / mana cho hop phong cach kinh
/// di; tieng Viet CO DAU; KHONG duoc tran, che lan nhau.
///
///   A. Ngoai Play - chay THAT ham bo cuc (GameHUD.TinhBoCuc) voi chu that cua
///      Inter, chu DAI NHAT co the gap, o 11 co man hinh x (PC, cam ung):
///        - moi khung chu nam trong man hinh;
///        - khong khung chu nao de len khung chu khac hay len nut / can / thanh
///          ky nang;
///        - con so mau / mana lot trong thanh; hai dong chu lot trong khung dot.
///   B. Chu: moi ky tu trong cac chuoi cua HUD co trong cmap Inter; khong con
///      cum chu KHONG DAU cu nao ("PHU THUY", "Quai con lai"...).
///   C. Trong Play (Act2): bat CUNG LUC moi thong bao + mau thap, doc lai bo cuc
///      that vua ve (BoCucCuoi), kiem de nhau / ra ngoai, chup anh.
///
/// Anh <c>PlayTestShots/hud_*.png</c>, so do <c>PlayTestShots/hudkinhdi.txt</c>.
/// </summary>
public static class ThuHUDKinhDi
{
    const string Canh = "Assets/Scenes/Act2.unity";
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;

    // Chu dai nhat co the gap (dinh dang lay dung tu code that)
    const string Dong1 = "ĐỢT 99";
    const string Dong2 = "Quái còn lại: 128   ·   Đã diệt: 99999";
    const string Dong2Cho = "Đợt mới sau 30 giây…";
    const string Mang = "Bản này chạy trong Unity Editor nên chưa nối mạng thật được (WebRTC chỉ có trên bản web). Hãy mở diablo25d-game.web.app trên hai máy.";
    const string Mang2 = "Đã nối 1/3 người. Không nối được với: Ác Quỷ Bóng Đêm, Kẻ Săn Hồn";
    const string KetNoi = "Chủ phòng đã rời trận — trận đấu dừng tại đây.\nBấm TRỞ VỀ để về sảnh.";
    const string BaoGiua = "CẢM ỨNG: cần điều khiển + nút tròn";
    const string BaoNV = "THIÊN THẠCH đang hồi chiêu";
    const string SoMau = "30000 / 30000", SoMana = "250 / 250";

    [MenuItem("Diablo 2.5D/53. Chay thu HUD KINH DI (mau, mana, thong bao)", false, 142)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] HUD kinh di trong tran");

        KiemBoCuc();
        KiemChu();

        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (canhCu != Canh) EditorSceneManager.OpenScene(Canh);
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_HUDKinhDi");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[HUD] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }

    // =============================================================
    // A. BO CUC O NHIEU CO MAN HINH
    // =============================================================

    /// <summary>Kiem mot bo cuc; tra so loi. <paramref name="nhan"/> dung khi ghi.</summary>
    static int KiemMotBoCuc(GameHUD.BoCucHUD b, float W, float H, string nhan, bool ghiChiTiet)
    {
        int n = 0;
        var r = b.tatCa;
        for (int i = 0; i < r.Length; i++)
        {
            if (b.laDieuKhien[i]) continue;
            bool trong = r[i].xMin >= -0.5f && r[i].yMin >= -0.5f && r[i].xMax <= W + 0.5f && r[i].yMax <= H + 0.5f;
            if (!trong) { n++; if (ghiChiTiet) Loi(nhan + ": \"" + b.tenKhung[i] + "\" ra ngoai man hinh " + r[i]); }
        }
        for (int i = 0; i < r.Length; i++)
            for (int j = i + 1; j < r.Length; j++)
            {
                if (b.laDieuKhien[i] && b.laDieuKhien[j]) continue;   // vi tri nut co san, khong thuoc HUD chu
                if (r[i].Overlaps(r[j]))
                { n++; if (ghiChiTiet) Loi(nhan + ": \"" + b.tenKhung[i] + "\" de len \"" + b.tenKhung[j] + "\""); }
            }
        return n;
    }

    static void KiemBoCuc()
    {
        var dam = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Inter-SemiBold.ttf");
        var thuong = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Inter-Regular.ttf");
        if (dam == null || thuong == null) { Loi("khong nap duoc font Inter"); return; }
        var k = new GameHUD.KieuHUD(dam, thuong);

        int[,] manHinh =
        {
            { 1920, 1080 }, { 1366, 768 }, { 1280, 720 }, { 1024, 768 }, { 2532, 1170 }, { 2400, 1080 },
            { 2732, 2048 }, { 1568, 505 }, { 800, 360 }, { 1080, 1920 }, { 1170, 2532 },
        };
        Ghi("A. bo cuc (chu dai nhat, moi thong bao cung hien):");
        int tong = 0;
        for (int i = 0; i < manHinh.GetLength(0); i++)
        {
            float W = manHinh[i, 0], H = manHinh[i, 1], s = H / 1080f;
            k.DatCo(s);
            var dong = new StringBuilder();
            foreach (bool camUng in new[] { false, true })
                foreach (string mang in new[] { Mang, Mang2 })
                    foreach (string d2 in new[] { Dong2, Dong2Cho })
                    {
                        var c = GameHUD.DoCoChu(k, W, s, Dong1, d2, mang, KetNoi, BaoGiua, BaoNV, SoMau, SoMana);
                        var b = GameHUD.TinhBoCuc(W, H, s, c, camUng);
                        string nhan = W + "x" + H + (camUng ? " cam ung" : " PC");
                        int n = KiemMotBoCuc(b, W, H, nhan, true);

                        // So trong thanh, chu trong khung dot
                        float le = 8f * s;
                        if (c.rongSoMau + le > GameHUD.RongThanh(s)) { n++; Loi(nhan + ": so mau tran thanh"); }
                        if (c.rongSoMana + le > GameHUD.RongThanh(s)) { n++; Loi(nhan + ": so mana tran thanh"); }
                        float r1 = k.dot.CalcSize(new GUIContent(Dong1)).x, r2 = k.dong2.CalcSize(new GUIContent(d2)).x;
                        if (Mathf.Max(r1, r2) > b.thongTin.width) { n++; Loi(nhan + ": chu tran khung dot quai"); }
                        tong += n;
                        if (mang == Mang && d2 == Dong2)
                            dong.AppendFormat("  {0}: dot {1:F0}x{2:F0}, mang {3:F0}x{4:F0} y{5:F0}, bao NV y{6:F0} -> {7}",
                                camUng ? "cam ung" : "PC", b.thongTin.width, b.thongTin.height, b.mang.width, b.mang.height,
                                b.mang.y, b.baoNhanVat.y, n == 0 ? "on" : n + " LOI");
                    }
            Ghi(string.Format("   {0}x{1} (ti le {2:F2}):{3}", W, H, H / 1080f, dong));
        }
        Ghi("   tong loi bo cuc (11 co man hinh x 2 che do x 4 bo chu) = " + tong);
    }

    // =============================================================
    // B. CHU CO DAU
    // =============================================================

    static readonly string[] TepHUD =
    {
        "Assets/Scripts/UI/GameHUD.cs", "Assets/Scripts/UI/GameHUDKinhDi.cs",
        "Assets/Scripts/Player/PlayerController.cs", "Assets/Scripts/Mang/KhoiDongTranMang.cs",
    };

    static void KiemChu()
    {
        var cmapDam = BangKyTuFont.Doc("Assets/Resources/Fonts/Inter-SemiBold.ttf");
        var cmapThuong = BangKyTuFont.Doc("Assets/Resources/Fonts/Inter-Regular.ttf");
        if (cmapDam == null || cmapThuong == null) { Loi("khong doc duoc cmap Inter"); return; }

        // Cum chu khong dau cu - khong duoc con trong chuoi hien thi
        string[] cuKhongDau = { "PHU THUY", "\"DOT ", "Quai con lai", "Da diet", "BAN DA GUC", "TRO VE",
                                "Khong du nang luong", "dang hoi chieu", "Dang niem chu", "Da khoa goc", "CAM UNG:" };
        var thieu = new StringBuilder();
        var conSot = new StringBuilder();
        int soChuoi = 0;
        foreach (var tep in TepHUD)
        {
            string nguon = File.ReadAllText(tep);
            foreach (Match m in Regex.Matches(nguon, "\"([^\"\\\\]|\\\\.)*\""))
            {
                string v = m.Value;
                // Bo chuoi trong dong chu thich / Debug.Log (khong hien cho nguoi choi)
                int dauDong = nguon.LastIndexOf('\n', m.Index) + 1;
                string truoc = nguon.Substring(dauDong, m.Index - dauDong);
                if (truoc.Contains("//") || truoc.Contains("Debug.Log")) continue;
                soChuoi++;
                foreach (char c in v)
                    if (c >= 128 && (!cmapDam.Contains(c) || !cmapThuong.Contains(c)) && thieu.ToString().IndexOf(c) < 0) thieu.Append(c);
                foreach (var cu in cuKhongDau)
                    if (v.Contains(cu)) conSot.Append(Path.GetFileName(tep)).Append(": ").Append(v).Append("  ");
            }
        }
        Ghi(string.Format("B. chu: {0} chuoi trong {1} file HUD; ky tu khong co trong Inter: \"{2}\"; cum chu khong dau cu con sot: {3}",
            soChuoi, TepHUD.Length, thieu, conSot.Length == 0 ? "khong" : conSot.ToString()));
        if (thieu.Length > 0) Loi("font Inter thieu ky tu: " + thieu);
        if (conSot.Length > 0) Loi("con chu khong dau: " + conSot);
    }

    // =============================================================
    // C. TRONG PLAY
    // =============================================================

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator KichBan()
    {
        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        yield return new WaitForSeconds(1.5f);
        var hud = GameHUD.Ban;
        if (toi == null || hud == null) { Loi("khong co nhan vat hoac HUD"); Ket(); yield break; }

        // Ban dau - binh thuong
        yield return new WaitForEndOfFrame();
        yield return Chup("hud_1_binhthuong");

        // Bat CUNG LUC moi thong bao + mau thap
        var mau = toi.GetComponent<Damageable>();
        float mauCu = mau != null ? mau.health : 0f;
        if (mau != null) mau.health = mau.maxHealth * 0.2f;
        KhoiDongTranMang.TrangThai = Mang;
        KhoiDongTranMang.ThongBaoKetNoi = KetNoi;
        for (int lan = 0; lan < 12; lan++)
        {
            hud.Say(BaoGiua);
            toi.SendMessage("Say", BaoNV);          // Say cua nhan vat la ham rieng - goi qua SendMessage
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        hud.Say(BaoGiua); toi.SendMessage("Say", BaoNV);
        yield return new WaitForEndOfFrame();

        var b = hud.BoCucCuoi;
        float s = Screen.height / 1080f;
        var ds = new StringBuilder();
        for (int i = 0; i < b.tatCa.Length; i++)
            ds.AppendFormat(" [{0}: {1:F0},{2:F0} {3:F0}x{4:F0}]", b.tenKhung[i], b.tatCa[i].x, b.tatCa[i].y, b.tatCa[i].width, b.tatCa[i].height);
        Ghi("C. trong Play " + Screen.width + "x" + Screen.height + ", moi thong bao cung hien, mau 20%:" + ds);
        int n = KiemMotBoCuc(b, Screen.width, Screen.height, "Play", true);
        Ghi("   de nhau / ra ngoai: " + n);
        var k = hud.Kieu;
        string fontDot = k != null && k.dot.font != null ? k.dot.font.name : "-";
        string fontMang = k != null && k.mang.font != null ? k.mang.font.name : "-";
        Ghi("   font khung dot: " + fontDot + ", font thong bao: " + fontMang);
        if (!fontDot.Contains("Inter") || !fontMang.Contains("Inter")) Loi("HUD khong dung font Inter");
        int soKhungChu = 0;
        for (int i = 0; i < b.tatCa.Length; i++) if (!b.laDieuKhien[i]) soKhungChu++;
        Ghi("   so khung chu dang hien: " + soKhungChu + " (mong 6: bang, dot, mang, ket noi, bao ngan, bao nhan vat)");
        if (soKhungChu != 6) Loi("khong du 6 khung chu cung hien - phep do de nhau khong day du");
        yield return Chup("hud_2_moithongbao");

        // Tra lai
        KhoiDongTranMang.TrangThai = "";
        KhoiDongTranMang.ThongBaoKetNoi = "";
        if (mau != null) mau.health = mauCu;
        yield return null;

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/hudkinhdi.txt", bao.ToString());
        var rac = GameObject.Find("TAM_HUDKinhDi");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        KhoiDongTranMang.TrangThai = ""; KhoiDongTranMang.ThongBaoKetNoi = "";
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh) EditorSceneManager.OpenScene(canhCu);
    }
}
