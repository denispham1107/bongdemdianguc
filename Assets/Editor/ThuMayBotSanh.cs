using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: MAY BOT O GHE TRONG - BUOC 1 (menu 112, 08/10/2026, nguoi dung: "chu phong nhan vao ghe trong thi them may BOT").
///
/// Firebase THAT, tai khoan chay thu B (chu phong) va A (khach) doc tu chay-thu-mang.txt qua ThongTinChayThu. Do:
///   A. Nhan dang: uid BOT ba do kho -> LaBot / DoKhoCua dung; uid THAT cua tai khoan (28 ky tu chu + so) KHONG la BOT.
///   B. Phong Don: chu phong mo bang chon (ManSanh.MoChonBot) roi chon KHO / DE / THUONG qua CHINH ham nut goi
///      (ChonDoKhoBot) -> doc lai Firebase: uid bot_k_/d_/t_, ten BOT 1/2/3, san sang, ghe (cho) khong trung, soNguoi.
///   C. Phong du 6 -> them nua bi tu choi, soNguoi van 6. Duoi BOT (nut DUOI dung DuoiNguoi) -> mat ghe, soNguoi 5.
///   D. Khach (A) vao phong: thay BOT; khach goi ThemBot -> bi tu choi (chi chu phong).
///   E. Phong Doi: ba BOT vao Doi B -> B 3/3; BOT thu tu vao B bi tu choi; vao A duoc.
///   F. Vao tran: XepGhe co ghe BOT nhung danh sach CAN NOI (bat tay) bo qua BOT; dem nguoi that.
///   G. Anh: bang chon do kho, phong Don co BOT, phong Doi co BOT; khong chu nao bi cat.
/// Ket qua PlayTestShots/maybot_sanh.txt, anh maybot_*.png. Don sach phong thu.
/// </summary>
public static class ThuMayBotSanh
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";
    static string maDon, maDoi;

    [MenuItem("Diablo 2.5D/112. Chay thu MAY BOT o ghe trong (sanh - Firebase that)", false, 205)]
    public static void Chay()
    {
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[MayBot] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (EditorSceneManager.GetActiveScene().name != "MainMenu") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; loi = 0; daBatDau = false; maDon = maDoi = null;
        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_MayBot");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[MayBot] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator DangNhap(string email, string ten)
    {
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(email, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi("   dang nhap " + ten + ": " + (ok ? "OK" : "LOI - " + e));
        Kiem(ok, "khong dang nhap duoc " + ten);
    }

    static int SoBot(PhongMang.Phong p)
    {
        int n = 0;
        if (p != null) foreach (var x in p.nguoiChoi) if (MayBot.LaBot(x.uid)) n++;
        return n;
    }

    static string MoTa(PhongMang.Phong p)
    {
        var sb = new StringBuilder();
        foreach (var x in p.nguoiChoi)
            sb.Append(x.ten).Append("[").Append(MayBot.LaBot(x.uid) ? x.uid.Substring(0, 6) : "nguoi")
              .Append(" cho ").Append(x.cho).Append(x.sanSang ? " san" : " chua").Append(p.LaDoi ? " doi " + x.doi : "").Append("] ");
        return sb.ToString();
    }

    static IEnumerator ChoThemXong(ManSanh sanh)
    {
        yield return null;
        float han = Time.unscaledTime + 15f;
        while (sanh.DangThemBot && Time.unscaledTime < han) yield return null;
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] may BOT o ghe trong - sanh / phong tren Firebase that - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

        // ---- A. Nhan dang ----
        string uD = MayBot.TaoUid(MayBot.De), uT = MayBot.TaoUid(MayBot.Thuong), uK = MayBot.TaoUid(MayBot.Kho);
        Ghi("A. uid: " + uD + " / " + uT + " / " + uK + " -> LaBot " + MayBot.LaBot(uD) + MayBot.LaBot(uT) + MayBot.LaBot(uK)
            + ", do kho " + MayBot.DoKhoCua(uD) + MayBot.DoKhoCua(uT) + MayBot.DoKhoCua(uK)
            + " | 'bot_x_ab' " + MayBot.LaBot("bot_x_ab") + ", 'botk1234' " + MayBot.LaBot("botk1234"));
        Kiem(MayBot.LaBot(uD) && MayBot.LaBot(uT) && MayBot.LaBot(uK), "uid BOT khong nhan ra la BOT");
        Kiem(MayBot.DoKhoCua(uD) == 0 && MayBot.DoKhoCua(uT) == 1 && MayBot.DoKhoCua(uK) == 2, "do kho doc tu uid sai");
        Kiem(!MayBot.LaBot("bot_x_ab") && !MayBot.LaBot("botk1234") && !MayBot.LaBot(null), "uid khong phai BOT lai nhan la BOT");

        yield return DangNhap(ThongTinChayThu.EmailB, "B (chu phong)");
        if (loi > 0) { Ket(); yield break; }
        string uidB = FirebaseMang.Uid;
        Ghi("   uid that cua B (" + uidB.Length + " ky tu) LaBot = " + MayBot.LaBot(uidB));
        Kiem(!MayBot.LaBot(uidB), "uid THAT cua tai khoan bi nhan la BOT");
        yield return PhongMang.DoDongHoMayChu();

        // ---- B. Phong Don, them BOT qua sanh ----
        bool ok = false; string e = null;
        yield return PhongMang.TaoPhong("Phòng thử BOT", CheDoTran.Don, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong tao duoc phong Don: " + e);
        if (!ok) { Ket(); yield break; }
        maDon = PhongMang.PhongHienTai.ma;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        yield return new WaitForSecondsRealtime(3.5f);
        var sanh = Object.FindAnyObjectByType<ManSanh>();
        Kiem(sanh != null && sanh.enabled, "khong vao duoc sanh");
        if (sanh == null) { Ket(); yield break; }
        var fDangO = typeof(ManSanh).GetField("dangO", BindingFlags.NonPublic | BindingFlags.Instance);
        var fHoi = typeof(ManSanh).GetField("hoiLanSau", BindingFlags.NonPublic | BindingFlags.Instance);
        yield return PhongMang.TaiLaiPhong(maDon, (o, err) => { });
        fDangO.SetValue(sanh, System.Enum.Parse(fDangO.FieldType, "TrongPhong"));
        fHoi.SetValue(sanh, Time.unscaledTime + 60f);

        yield return DoMan("B0. phong Don chi co chu phong (ghe trong hien '+ THEM MAY BOT')", "maybot_phong_trong");
        sanh.MoChonBot(CheDoTran.KhongDoi);
        Kiem(sanh.DangMoChonBot, "MoChonBot khong mo bang");
        yield return DoMan("B1. bang chon do kho", "maybot_bang_chon");
        float sGd = GiaoDien.TiLe;
        var kb = ManSanh.KhungChonBot(sGd);
        Kiem(kb.xMin >= 0 && kb.yMin >= 0 && kb.xMax <= Screen.width && kb.yMax <= Screen.height, "bang chon BOT tran khoi man hinh");
        for (int d = 0; d < 3; d++) Kiem(kb.Contains(ManSanh.NutDoKhoBot(sGd, d).center), "nut do kho " + d + " nam ngoai bang");

        int[] thuTu = { MayBot.Kho, MayBot.De, MayBot.Thuong };
        foreach (int d in thuTu)
        {
            if (!sanh.DangMoChonBot) sanh.MoChonBot(CheDoTran.KhongDoi);
            sanh.ChonDoKhoBot(d);
            Kiem(!sanh.DangMoChonBot, "chon do kho ma bang khong dong");
            yield return ChoThemXong(sanh);
        }
        yield return PhongMang.TaiLaiPhong(maDon, (o, err) => { });
        var p = PhongMang.PhongHienTai;
        Ghi("B2. sau khi them KHO, DE, THUONG: soNguoi " + p.soNguoi + ", " + MoTa(p));
        var cacCho = new System.Collections.Generic.HashSet<int>();
        bool choTrung = false; foreach (var x in p.nguoiChoi) if (!cacCho.Add(x.cho)) choTrung = true;
        var b1 = p.nguoiChoi.Find(x => x.ten == "BOT 1");
        var b2 = p.nguoiChoi.Find(x => x.ten == "BOT 2");
        var b3 = p.nguoiChoi.Find(x => x.ten == "BOT 3");
        Kiem(SoBot(p) == 3 && p.soNguoi == 4, "khong du 3 BOT / soNguoi khac 4");
        Kiem(b1 != null && b1.uid.StartsWith("bot_k_") && b2 != null && b2.uid.StartsWith("bot_d_") && b3 != null && b3.uid.StartsWith("bot_t_"),
             "ten / do kho BOT sai thu tu");
        Kiem(p.nguoiChoi.TrueForAll(x => !MayBot.LaBot(x.uid) || x.sanSang), "BOT chua san sang");
        Kiem(!choTrung, "hai ghe trung cho");
        yield return DoMan("B3. phong Don: chu phong + 3 BOT", "maybot_phong_don");

        // ---- C. Day phong, tu choi, duoi ----
        yield return PhongMang.ThemBot(MayBot.Thuong, CheDoTran.KhongDoi, (o, err) => { });
        yield return PhongMang.ThemBot(MayBot.Thuong, CheDoTran.KhongDoi, (o, err) => { });
        bool themThu7 = true; string loi7 = null;
        yield return PhongMang.ThemBot(MayBot.De, CheDoTran.KhongDoi, (o, err) => { themThu7 = o; loi7 = err; });
        yield return PhongMang.TaiLaiPhong(maDon, (o, err) => { });
        p = PhongMang.PhongHienTai;
        Ghi("C1. phong 6 nguoi (" + SoBot(p) + " BOT), them nua -> " + themThu7 + " (\"" + loi7 + "\"), soNguoi " + p.soNguoi + ", " + p.nguoiChoi.Count + " ghe");
        Kiem(!themThu7 && p.soNguoi == 6 && p.nguoiChoi.Count == 6, "phong du 6 van them duoc BOT");
        string uidDuoi = b2 != null ? b2.uid : null;
        yield return PhongMang.DuoiNguoi(uidDuoi, null);
        yield return PhongMang.TaiLaiPhong(maDon, (o, err) => { });
        p = PhongMang.PhongHienTai;
        Ghi("C2. duoi BOT 2 -> con " + SoBot(p) + " BOT, soNguoi " + p.soNguoi + ", con BOT 2: " + p.nguoiChoi.Exists(x => x.uid == uidDuoi));
        Kiem(!p.nguoiChoi.Exists(x => x.uid == uidDuoi) && p.soNguoi == 5, "duoi BOT khong xoa ghe / sai soNguoi");

        // ---- F. Vao tran: xep ghe va danh sach bat tay ----
        var bang = KhoiDongTranMang.XepGhe(p.nguoiChoi, uidB);
        int soCanNoi = 0, soThat = 0, soBotGhe = 0;
        foreach (var g in bang)
        {
            if (MayBot.LaBot(g.uid)) { soBotGhe++; continue; }
            soThat++;
            if (g.uid != uidB) soCanNoi++;
        }
        Ghi("F. XepGhe: " + bang.Count + " ghe (" + soBotGhe + " BOT, " + soThat + " nguoi that) -> chu phong can bat tay " + soCanNoi + " (phai 0: BOT khong bat tay)");
        Kiem(bang.Count == 5 && soBotGhe == 4 && soCanNoi == 0, "xep ghe / danh sach bat tay sai voi BOT");

        // ---- D. Khach A vao phong ----
        yield return DangNhap(ThongTinChayThu.EmailA, "A (khach)");
        string uidA = FirebaseMang.Uid;
        yield return PhongMang.VaoPhong(maDon, (o, err) => { ok = o; e = err; });
        p = PhongMang.PhongHienTai;
        Ghi("D1. A vao phong: " + (ok ? "OK" : "LOI " + e) + ", A thay " + SoBot(p) + " BOT, soNguoi " + (p != null ? p.soNguoi : -1));
        Kiem(ok && SoBot(p) == 4 && p.soNguoi == 6, "khach vao phong co BOT sai");
        bool khachThem = true; string loiKhach = null;
        yield return PhongMang.ThemBot(MayBot.De, CheDoTran.KhongDoi, (o, err) => { khachThem = o; loiKhach = err; });
        Ghi("D2. khach goi ThemBot -> " + khachThem + " (\"" + loiKhach + "\")");
        Kiem(!khachThem, "khach them duoc BOT");
        bool ghiTho = true;
        yield return FirebaseMang.Ghi("phong/" + maDon + "/nguoiChoi/" + MayBot.TaoUid(MayBot.De),
                                      "{\"ten\":\"BOT 9\",\"sanSang\":true,\"cho\":3,\"vaoLuc\":1}", (o, err) => ghiTho = o);
        Ghi("D3. khach ghi THANG len Firebase mot ghe BOT -> " + ghiTho + " (phai False - luat chi cho chu phong)");
        Kiem(!ghiTho, "luat Firebase cho khach ghi ghe BOT");
        yield return PhongMang.RoiPhong(null);

        // ---- E. Phong Doi ----
        yield return DangNhap(ThongTinChayThu.EmailB, "B (chu phong)");
        yield return PhongMang.TaoPhong("Phòng đôi thử BOT", CheDoTran.Doi, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong tao duoc phong Doi: " + e);
        if (ok)
        {
            maDoi = PhongMang.PhongHienTai.ma;
            fDangO.SetValue(sanh, System.Enum.Parse(fDangO.FieldType, "TrongPhong"));
            fHoi.SetValue(sanh, Time.unscaledTime + 60f);
            for (int i = 0; i < 3; i++)
            {
                sanh.MoChonBot(CheDoTran.DoiB);
                sanh.ChonDoKhoBot(i);
                yield return ChoThemXong(sanh);
            }
            bool themB4 = true; string loiB4 = null;
            yield return PhongMang.ThemBot(MayBot.Kho, CheDoTran.DoiB, (o, err) => { themB4 = o; loiB4 = err; });
            sanh.MoChonBot(CheDoTran.DoiA);
            yield return DoMan("E0. bang chon BOT cho DOI A", "maybot_bang_chon_doi");
            sanh.ChonDoKhoBot(MayBot.Kho);
            yield return ChoThemXong(sanh);
            yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
            p = PhongMang.PhongHienTai;
            int a = CheDoTran.DemDoi(p.nguoiChoi, CheDoTran.DoiA), b = CheDoTran.DemDoi(p.nguoiChoi, CheDoTran.DoiB);
            Ghi("E1. phong Doi: 3 BOT vao Doi B -> A " + a + ", B " + b + "; BOT thu tu vao B -> " + themB4 + " (\"" + loiB4 + "\"); "
                + MoTa(p));
            Kiem(b == 3 && a == 2 && !themB4, "BOT vao sai doi / doi du 3 van them duoc");
            yield return DoMan("E2. phong Doi co BOT", "maybot_phong_doi");
        }

        // Don dep
        fDangO.SetValue(sanh, System.Enum.Parse(fDangO.FieldType, "Sanh"));
        foreach (var ma in new[] { maDon, maDoi })
        {
            if (string.IsNullOrEmpty(ma)) continue;
            yield return FirebaseMang.Xoa("phong/" + ma, (o2, e2) => { });
            yield return FirebaseMang.Xoa("tran/" + ma, (o2, e2) => { });
            string con = null;
            yield return FirebaseMang.Doc("phong/" + ma, s => con = s);
            bool sach = string.IsNullOrEmpty(con) || con == "null";
            Ghi("don phong " + ma + ": " + (sach ? "sach" : "VAN CON"));
            Kiem(sach, "phong thu con tren Firebase");
        }
        PhongMang.PhongHienTai = null;
        Ket();
    }

    static IEnumerator DoMan(string ten, string anh)
    {
        GiaoDien.DatLaiDem();
        for (int i = 0; i < 6; i++) yield return null;
        int cat = GiaoDien.SoLanCat, ve = GiaoDien.SoLuotVe;
        string chuCat = GiaoDien.ChuBiCatCuoi;
        string d = "PlayTestShots/" + anh + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
        Ghi(string.Format("{0}: {1} luot ve, chu bi cat {2} lan{3} - anh {4}.png", ten, ve, cat, cat > 0 ? " (\"" + chuCat + "\")" : "", anh));
        Kiem(ve > 0, ten + ": khong ve gi");
        Kiem(cat == 0, ten + ": co chu bi cat");
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/maybot_sanh.txt", bao.ToString());
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t != null && t.parent == null && t.name.StartsWith("TAM_")) Object.Destroy(t.gameObject);
        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc); else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
