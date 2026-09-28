using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: SANH / PHONG CHE DO ĐƠN - ĐÔI TREN FIREBASE THAT (menu 92b, 28/09/2026).
///
/// Hai tai khoan chay thu (A, B - doc tu chay-thu-mang.txt qua ThongTinChayThu) va cac GHE GIA do chu phong ghi vao phong
/// (luat cho chu phong ghi moi o nguoiChoi/*). Do:
///   1. Tao phong Doi: cheDo, toiDa 6, chu phong vao Doi A. Luat: ghi doi = 2, toiDa 7, soNguoi 7 -> BI TU CHOI (doi chung).
///   2. Nguoi ban game cu (khong co "doi") -> chu phong xep doi (CanBangDoiNeuCan).
///   3. A vao phong -> vao doi it nguoi hon; A tu doi doi; A KHONG sua duoc doi cua nguoi khac (luat Firebase).
///   4. Doi du 3 -> chu phong khong chuyen them vao duoc; hai nguoi vao cung doi (4) -> chu phong can bang lai.
///   5. Anh: sanh co hai phong (nhan ĐƠN / ĐÔI), phong Doi 3/3 - 3/3, phong Don 6 nguoi; khong chu nao bi cat.
/// Ket qua PlayTestShots/sanh_doi.txt, anh sanh_doi_*.png. Don sach hai phong thu.
/// </summary>
public static class ThuSanhDoi
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";
    static string maDoi, maDon;

    [MenuItem("Diablo 2.5D/92b. Chay thu SANH DON - DOI tren Firebase that", false, 182)]
    public static void Chay()
    {
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[SanhDoi] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (EditorSceneManager.GetActiveScene().name != "MainMenu") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; loi = 0; daBatDau = false; maDoi = maDon = null;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_SanhDoi");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[SanhDoi] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator GhiTho(string duong, string json, System.Action<bool> xong)
    {
        bool ok = false;
        yield return FirebaseMang.Ghi(duong, json, (o, e) => ok = o);
        xong(ok);
    }

    static IEnumerator DangNhap(string email, string ten)
    {
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(email, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi("   dang nhap " + ten + ": " + (ok ? "OK (" + FirebaseMang.TenHienThi + ")" : "LOI - " + e));
        Kiem(ok, "khong dang nhap duoc " + ten);
    }

    static string GheGia(string ten, int doi, long vaoLuc, int cho, bool coDoi = true)
    {
        return "{\"ten\":\"" + FirebaseMang.Thoat(ten) + "\",\"sanSang\":" + (cho % 2 == 0 ? "true" : "false") + ",\"cho\":" + cho + ","
             + (coDoi ? "\"doi\":" + doi + "," : "") + "\"vaoLuc\":" + vaoLuc + "}";
    }

    static int DoiCua(string uid)
    {
        var p = PhongMang.PhongHienTai;
        if (p == null) return -9;
        var n = p.nguoiChoi.Find(x => x.uid == uid);
        return n != null ? n.doi : -9;
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] sanh / phong che do Don - Doi tren Firebase that - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        FirebaseMang.Quen();
        yield return DangNhap(ThongTinChayThu.EmailB, "B (chu phong)");
        if (loi > 0) { Ket(); yield break; }
        string uidB = FirebaseMang.Uid;
        yield return PhongMang.DoDongHoMayChu();
        long t0 = (long)PhongMang.GioMayChu();

        // ---- Phong Don 6 nguoi (de chup sanh + phong Don) ----
        bool ok = false; string e = null;
        yield return PhongMang.TaoPhong("Phòng đơn thử", CheDoTran.Don, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong tao duoc phong Don: " + e);
        if (!ok) { Ket(); yield break; }
        maDon = PhongMang.PhongHienTai.ma;
        string[] tenGia = { "Thị Hằng Nga", "ĐồTểLàngMa", "Bóng Ma", "Quỷ Lùn Đêm", "Hắc Vương" };
        for (int i = 0; i < tenGia.Length; i++)
            yield return GhiTho("phong/" + maDon + "/nguoiChoi/tam_d" + i, GheGia(tenGia[i], 0, t0 + i, i + 1, false), x => { });
        yield return GhiTho("phong/" + maDon + "/soNguoi", "6", x => ok = x);
        Kiem(ok, "luat moi khong cho soNguoi = 6");
        yield return PhongMang.TaiLaiPhong(maDon, (o, err) => { });
        Ghi("1a. phong Don: cheDo " + PhongMang.PhongHienTai.cheDo + ", toiDa " + PhongMang.PhongHienTai.toiDa + ", " + PhongMang.PhongHienTai.nguoiChoi.Count + " nguoi");
        Kiem(!PhongMang.PhongHienTai.LaDoi && PhongMang.PhongHienTai.toiDa == 6 && PhongMang.PhongHienTai.nguoiChoi.Count == 6, "phong Don 6 nguoi sai");

        // ---- Phong Doi ----
        yield return PhongMang.TaoPhong("Phòng đôi thử", CheDoTran.Doi, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong tao duoc phong Doi: " + e);
        if (!ok) { Ket(); yield break; }
        maDoi = PhongMang.PhongHienTai.ma;
        string tho = null;
        yield return FirebaseMang.Doc("phong/" + maDoi, s => tho = s);
        var p = PhongMang.PhongHienTai;
        Ghi("1b. phong Doi: cheDo \"" + p.cheDo + "\", toiDa " + p.toiDa + ", doi chu phong " + DoiCua(uidB)
            + " | JSON that co \"cheDo\":\"doi\": " + (tho != null && tho.Contains("\"cheDo\":\"doi\"")));
        Kiem(p.LaDoi && p.toiDa == 6 && DoiCua(uidB) == 0 && tho != null && tho.Contains("\"cheDo\":\"doi\""), "tao phong Doi sai");

        // Luat Firebase: doi chung ghi sai phai bi tu choi
        bool ghiDoi2 = true, ghiToiDa7 = true, ghiSoNguoi7 = true, ghiCho5 = false;
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/" + uidB + "/doi", "2", x => ghiDoi2 = x);
        yield return GhiTho("phong/" + maDoi + "/toiDa", "7", x => ghiToiDa7 = x);
        yield return GhiTho("phong/" + maDoi + "/soNguoi", "7", x => ghiSoNguoi7 = x);
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/" + uidB + "/cho", "5", x => ghiCho5 = x);
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/" + uidB + "/cho", "0", x => { });
        Ghi("1c. luat: ghi doi=2 " + ghiDoi2 + ", toiDa=7 " + ghiToiDa7 + ", soNguoi=7 " + ghiSoNguoi7 + " (ca ba phai False) | cho=5 " + ghiCho5 + " (phai True)");
        Kiem(!ghiDoi2 && !ghiToiDa7 && !ghiSoNguoi7, "luat Firebase cho ghi gia tri sai");
        Kiem(ghiCho5, "luat Firebase chan ghe thu 6 (cho = 5)");

        // 2. Nguoi choi ban cu: khong co "doi" -> chu phong xep
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/tam_cu", GheGia("Người bản cũ", 0, t0 + 10, 1, false), x => { });
        yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
        int truoc = DoiCua("tam_cu");
        int soChuyen = 0;
        yield return PhongMang.CanBangDoiNeuCan(n => soChuyen = n);
        yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
        Ghi("2. nguoi ban cu vao khong ghi doi: doi " + truoc + " -> chu phong xep " + soChuyen + " nguoi -> doi " + DoiCua("tam_cu") + " (phai -1 -> 1)");
        Kiem(truoc == -1 && DoiCua("tam_cu") == 1, "chu phong khong xep doi cho nguoi ban cu");

        // 3. A vao phong
        yield return DangNhap(ThongTinChayThu.EmailA, "A");
        string uidA = FirebaseMang.Uid;
        yield return PhongMang.VaoPhong(maDoi, (o, err) => { ok = o; e = err; });
        Ghi("3a. A vao phong Doi (A 1, B 1): " + (ok ? "OK" : "LOI " + e) + ", doi cua A = " + DoiCua(uidA) + " (phai 0 - bang nhau vao A)");
        Kiem(ok && DoiCua(uidA) == 0, "A vao sai doi");
        yield return PhongMang.DatDoi(uidA, 1, (o, err) => { ok = o; e = err; });
        Ghi("3b. A tu sang Doi B: " + (ok ? "OK" : "LOI " + e) + ", doi = " + DoiCua(uidA) + " (phai 1)");
        Kiem(ok && DoiCua(uidA) == 1, "A khong tu doi doi duoc");
        bool aSuaChu = true;
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/" + uidB + "/doi", "1", x => aSuaChu = x);
        yield return PhongMang.DatDoi(uidB, 1, (o, err) => { ok = o; e = err; });
        Ghi("3c. A sua doi cua CHU PHONG: ghi thang Firebase " + aSuaChu + " (phai False), qua DatDoi " + ok + " (\"" + e + "\")");
        Kiem(!aSuaChu && !ok, "nguoi khach sua duoc doi cua nguoi khac");

        // 4. Chu phong: lap day, chuyen doi, can bang
        yield return DangNhap(ThongTinChayThu.EmailB, "B (chu phong)");
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/tam_g1", GheGia("Bóng Ma", 1, t0 + 600030, 2), x => { });
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/tam_g2", GheGia("Thị Hằng Nga", 0, t0 + 600031, 3), x => { });
        yield return GhiTho("phong/" + maDoi + "/nguoiChoi/tam_g3", GheGia("ĐồTểLàngMa", 0, t0 + 600032, 4), x => { });
        yield return GhiTho("phong/" + maDoi + "/soNguoi", "6", x => { });
        yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
        int a = CheDoTran.DemDoi(PhongMang.PhongHienTai.nguoiChoi, 0), b = CheDoTran.DemDoi(PhongMang.PhongHienTai.nguoiChoi, 1);
        yield return PhongMang.DatDoi(uidA, 0, (o, err) => { ok = o; e = err; });
        Ghi("4a. phong du 6 (A " + a + ", B " + b + "): chu phong chuyen A sang Doi A (da du) -> " + ok + " (\"" + e + "\"), doi A van " + DoiCua(uidA));
        Kiem(a == 3 && b == 3, "phong khong du 3/3");
        Kiem(!ok && DoiCua(uidA) == 1, "chuyen duoc vao doi da du 3");

        // Chup: sanh (hai phong) + phong Doi 3/3 + phong Don 6 nguoi
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        yield return new WaitForSecondsRealtime(3.5f);
        var sanh = Object.FindAnyObjectByType<ManSanh>();
        Kiem(sanh != null && sanh.enabled, "khong vao duoc sanh");
        if (sanh != null)
        {
            sanh.cheDoMoi = CheDoTran.Doi;
            yield return DoMan("5a. sanh co phong Don + Doi", "sanh_doi_sanh");
            var fDangO = typeof(ManSanh).GetField("dangO", BindingFlags.NonPublic | BindingFlags.Instance);
            var fHoi = typeof(ManSanh).GetField("hoiLanSau", BindingFlags.NonPublic | BindingFlags.Instance);
            yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
            fDangO.SetValue(sanh, System.Enum.Parse(fDangO.FieldType, "TrongPhong"));
            fHoi.SetValue(sanh, Time.unscaledTime + 30f);         // khong de nhip hoi lai (va can bang) chen vao luc chup
            yield return DoMan("5b. phong Doi 3/3 - 3/3 (chu phong thay nut CHUYEN DOI)", "sanh_doi_phong_doi");

            // 4b. Hai nguoi vao cung luc cung chon Doi B -> 4 nguoi; chu phong can bang lai
            yield return GhiTho("phong/" + maDoi + "/nguoiChoi/tam_g3/doi", "1", x => { });
            yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
            int b4 = CheDoTran.DemDoi(PhongMang.PhongHienTai.nguoiChoi, 1);
            fHoi.SetValue(sanh, 0f);                                // de nhip hoi phong cua ManSanh tu can bang
            float han = Time.unscaledTime + 8f;
            while (Time.unscaledTime < han && (PhongMang.PhongHienTai == null || CheDoTran.DemDoi(PhongMang.PhongHienTai.nguoiChoi, 1) > 3))
                yield return null;
            yield return PhongMang.TaiLaiPhong(maDoi, (o, err) => { });
            int aSau = CheDoTran.DemDoi(PhongMang.PhongHienTai.nguoiChoi, 0), bSau = CheDoTran.DemDoi(PhongMang.PhongHienTai.nguoiChoi, 1);
            Ghi("4b. ghe tam_g3 vao nham Doi B (B " + b4 + ") -> nhip hoi phong cua chu phong can bang: A " + aSau + ", B " + bSau
                + ", tam_g3 doi " + DoiCua("tam_g3") + " (phai 3/3, tam_g3 ve 0 - nguoi vao sau cung)");
            Kiem(b4 == 4 && aSau == 3 && bSau == 3 && DoiCua("tam_g3") == 0, "chu phong khong can bang lai doi 4 nguoi");

            fHoi.SetValue(sanh, Time.unscaledTime + 30f);
            yield return PhongMang.TaiLaiPhong(maDon, (o, err) => { });
            yield return DoMan("5c. phong Don 6 nguoi", "sanh_doi_phong_don");
            fDangO.SetValue(sanh, System.Enum.Parse(fDangO.FieldType, "Sanh"));
        }

        // Don dep
        foreach (var ma in new[] { maDoi, maDon })
        {
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
        File.WriteAllText("PlayTestShots/sanh_doi.txt", bao.ToString());
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
