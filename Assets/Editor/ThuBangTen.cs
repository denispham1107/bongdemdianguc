using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU TEN TREN DAU NHAN VAT (<see cref="BangTen"/>).
///
/// Vao Play o Act2, gan ten cho nhan vat cua minh, sinh ba ban sao nguoi khac
/// (ten co dau tieng Viet) dung quanh minh, roi do tung bang ten:
///
///   1. CO VE that trong khung hinh vua roi, va nam trong man hinh.
///   2. NAM NGAY TREN DAU: day bang ten o tren chop mu that (dinh luoi BAKE o
///      tu the dang dien - doc lap voi xuong head_end ma BangTen dung) va cach
///      no khong qua 0,30 m.
///   3. DUNG MAU: cua minh vang, nguoi khac trang nga.
///   4. DUNG FONT Inter - font mac dinh thieu chu co dau.
///   5. BON BANG TEN KHONG DE LEN NHAU khi dung cach nhau ~2 m.
///   6. Nguoi da guc thi ten MO DI.
///   7. NEN TRONG SUOT - do tren anh chup, co mau doi chung o den de chung minh
///      phep do bat duoc nen den. Phep thu goi GiaoDien.ChuanBi (qua
///      VeThuOnGUI) nhu man sanh: thieu buoc ay thi nen den khong duoc ve trong
///      phep thu du nguoi choi van thay.
///   Them: font co du 134 chu co dau (doc thang cmap), dau roi duoc ghep dung.
///
/// Anh <c>PlayTestShots/bangten_1.png</c> (du ten), <c>bangten_2_guc.png</c>;
/// so do <c>PlayTestShots/bangten.txt</c>.
/// </summary>
public static class ThuBangTen
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/52. Chay thu TEN TREN DAU nhan vat", false, 141)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (canhCu != Canh) EditorSceneManager.OpenScene(Canh);
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_BangTen");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BangTen] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// CHOP MU THAT tren man hinh (toa do OnGUI, y tu tren xuong) va so diem anh
    /// cua 0,30 m tai cho ay. Dinh that = dinh cao nhat cua luoi da BAKE o tu the
    /// dang dien - doc lap voi cach BangTen tim dinh dau (xuong head_end).
    /// Khong dung con nhong va cham hay khung bao luoi: do duoc ca hai deu cao
    /// hon chop mu (2,06 m va 1,88 m so voi 1,61 m).
    /// </summary>
    static void ChopMu(Camera cam, GameObject nv, out float yChop, out float px30cm, out float caoChop)
    {
        float cao = 0f;
        Vector3 dinh = nv.transform.position;
        foreach (var smr in nv.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!smr.enabled) continue;
            var m = new UnityEngine.Mesh();
            smr.BakeMesh(m, true);
            foreach (var v in m.vertices)
            {
                var w = smr.transform.TransformPoint(v);
                if (w.y - nv.transform.position.y > cao) { cao = w.y - nv.transform.position.y; dinh = w; }
            }
            Object.DestroyImmediate(m);
        }
        caoChop = cao;
        float y0 = cam.WorldToScreenPoint(dinh).y;
        float y1 = cam.WorldToScreenPoint(dinh + Vector3.up * 0.30f).y;
        yChop = Screen.height - y0;
        px30cm = Mathf.Abs(y1 - y0);
    }

    /// <summary>Do sang trung vi cua mot o (toa do OnGUI, y tu tren xuong) tren anh chup.</summary>
    static float TrungViSang(Texture2D anh, Rect o)
    {
        var ds = new List<float>();
        int x0 = Mathf.Clamp(Mathf.RoundToInt(o.xMin), 0, anh.width - 1), x1 = Mathf.Clamp(Mathf.RoundToInt(o.xMax), 0, anh.width - 1);
        int y0 = Mathf.Clamp(Mathf.RoundToInt(o.yMin), 0, anh.height - 1), y1 = Mathf.Clamp(Mathf.RoundToInt(o.yMax), 0, anh.height - 1);
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                var c = anh.GetPixel(x, anh.height - 1 - y);          // anh dem y tu duoi len
                ds.Add(0.299f * c.r + 0.587f * c.g + 0.114f * c.b);
            }
        if (ds.Count == 0) return 0f;
        ds.Sort();
        return ds[ds.Count / 2];
    }

    /// <summary>
    /// Do sang dai SAT MEP trai/phai o chu (noi nen den cu tran ra, khong co net
    /// chu) chia cho dai nen xa hon mot chut. ~1: trong suot; ~0,62: co nen 0,38.
    /// </summary>
    // Nguong chung cho ca bang ten lan mau doi chung. Do o 1568x505: khong nen
    // 0,96-1,14, nen den cu ve lai 0,71-0,86 (dai do chi 1-2 diem anh nen dao
    // dong) - dat giua khoang trong.
    const float NguongNen = 0.90f;

    static float TiSoSatMep(Texture2D anh, Rect o, float s)
    {
        float le = Mathf.Max(3f, 6f * s), vien = Mathf.Max(1f, 1.5f * s);
        float ya = o.y + 0.25f * o.height, cao = 0.5f * o.height;
        float rong = Mathf.Max(1f, le - vien - 1f);
        float satMep = 0.5f * (TrungViSang(anh, new Rect(o.xMin - le, ya, rong, cao))
                             + TrungViSang(anh, new Rect(o.xMax + vien + 1f, ya, rong, cao)));
        float xaHon = 0.5f * (TrungViSang(anh, new Rect(o.xMin - le - 8f, ya, 5f, cao))
                            + TrungViSang(anh, new Rect(o.xMax + le + 3f, ya, 5f, cao)));
        return xaHon > 0.001f ? satMep / xaHon : 1f;
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] ten tren dau nhan vat");

        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { Loi("khong tim thay nhan vat cua minh"); Ket(); yield break; }
        yield return new WaitForSeconds(2f);

        var cam = Camera.main;
        if (cam == null) { Loi("khong co Camera.main"); Ket(); yield break; }

        // Dung LAI trang thai giao dien nhu nguoi choi di qua sanh: khong co buoc
        // nay thi GiaoDien.To khong ve gi va nen den cua bang ten bien mat trong
        // phep thu (da vap: phep thu bao dep, nguoi choi thay o den)
        var veThu = GameObject.Find("TAM_BangTen").AddComponent<VeThuOnGUI>();

        // Dat ba ban sao quanh minh, trong khung hinh camera
        Vector3 phai = cam.transform.right; phai.y = 0f; phai.Normalize();
        Vector3 truoc = cam.transform.forward; truoc.y = 0f; truoc.Normalize();
        Vector3 P = toi.transform.position;

        // Ten dau tien o dang DAU ROI (NFD) - nhu bo go "Unicode to hop" de lai
        var cacTen = new[] { "Ác Quỷ Bóng Đêm".Normalize(NormalizationForm.FormD), "Kẻ Săn Hồn", "Người chơi 4" };
        var cho = new[] { P - phai * 2.4f, P + phai * 2.4f, P + truoc * 2.6f };
        var banSao = new List<PlayerController>();
        for (int i = 0; i < 3; i++)
        {
            Vector3 c = cho[i]; c.y = VfxFactory.GroundY(c) + 0.1f;
            var bs = NguoiChoiKhac.Sinh("uid-bangten-" + i, cacTen[i], c);
            if (bs == null) { Loi("khong sinh duoc ban sao " + i); continue; }
            banSao.Add(bs);
        }
        BangTen.Gan(toi.gameObject, "Phù Thuỷ Đỏ", true);

        yield return new WaitForSeconds(1.2f);
        yield return new WaitForEndOfFrame();

        float s = Screen.height / 1080f;
        Ghi(string.Format("man hinh {0}x{1}, ti le {2:F2}, co chu {3}", Screen.width, Screen.height, s,
                          Mathf.Max(12, Mathf.RoundToInt(BangTen.CoChu * s))));

        // ---- 4. Font ----
        var f = BangTen.FontDangDung;
        Ghi("4. font bang ten: " + (f != null ? f.name : "(chua ve lan nao)"));
        if (f == null || !f.name.Contains("Inter")) Loi("bang ten khong dung font Inter");

        // Doc THANG bang ky tu cua file font (HasCharacter trong Editor noi doi -
        // Windows ve bu chu thieu)
        var cmap = BangKyTuFont.Doc("Assets/Resources/Fonts/Inter-SemiBold.ttf");
        if (cmap == null) Loi("khong doc duoc cmap cua Inter-SemiBold.ttf");
        else
        {
            var thieu = new StringBuilder();
            string can = BangKyTuFont.ChuVietCoDau + "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 _-.";
            foreach (char ch in can) if (!cmap.Contains(ch)) thieu.Append(ch);
            Ghi(string.Format("   cmap Inter-SemiBold: {0} ky tu; thieu trong {1} chu can co: \"{2}\"",
                              cmap.Count, can.Length, thieu));
            if (thieu.Length > 0) Loi("font thieu chu: " + thieu);
        }

        // ---- 4b. Ghep dau roi: doi chieu voi chuan Unicode cua .NET ----
        int sai = 0, soThu = 0;
        var viDuSai = new StringBuilder();
        foreach (char ch in BangKyTuFont.ChuVietCoDau)
        {
            string goc = ch.ToString();
            string nfd = goc.Normalize(NormalizationForm.FormD);
            if (nfd.Length == 1) continue;                       // d/D gach: khong co dang tach
            var cachGo = new List<string> { nfd };
            // Dau go SAI THU TU (dau thanh truoc, mu/trang/moc sau) - bo go van co the de lai
            if (nfd.Length == 3) cachGo.Add("" + nfd[0] + nfd[2] + nfd[1]);
            // Nua tach: nguyen am co mu dung san + dau thanh roi (Unikey "to hop")
            if (nfd.Length == 3) cachGo.Add(("" + nfd[0] + nfd[1]).Normalize(NormalizationForm.FormC) + nfd[2]);
            foreach (var go in cachGo)
            {
                soThu++;
                string ra = GhepDauTiengViet.Ghep(go);
                if (ra != goc) { sai++; if (viDuSai.Length < 60) viDuSai.Append(goc).Append(' '); }
            }
        }
        string cau = "Nguyễn Thị Hương Ưng Ợ".Normalize(NormalizationForm.FormD);
        bool cauDung = GhepDauTiengViet.Ghep(cau) == "Nguyễn Thị Hương Ưng Ợ";
        Ghi(string.Format("4b. ghep dau roi: {0} cach go cua 134 chu, sai {1} {2}; ca cau dang tach -> {3}",
                          soThu, sai, viDuSai, cauDung ? "dung" : "SAI"));
        if (sai > 0 || !cauDung) Loi("ghep dau roi sai");
        if (GhepDauTiengViet.Ghep("ChienBinhB") != "ChienBinhB") Loi("ghep lam hong ten khong dau");

        // ---- 1-3, 5: tung bang ten ----
        var tatCa = Object.FindObjectsByType<BangTen>(FindObjectsSortMode.None);
        Ghi("so bang ten trong canh: " + tatCa.Length + " (mong 4)");
        if (tatCa.Length != 4) Loi("so bang ten khac 4");

        var oDaVe = new List<KeyValuePair<string, Rect>>();
        foreach (var b in tatCa)
        {
            bool vuaVe = b.khungVeCuoi >= Time.frameCount - 3;
            float yChop, px30, caoChop;
            ChopMu(cam, b.gameObject, out yChop, out px30, out caoChop);
            var o = b.oCuoi;
            float khe = yChop - o.yMax;          // >0: day chu nam tren chop mu
            bool trongManHinh = o.xMin >= 0 && o.xMax <= Screen.width && o.yMin >= 0 && o.yMax <= Screen.height;
            float lechNgang = Mathf.Abs(o.center.x - cam.WorldToScreenPoint(b.transform.position).x);

            Ghi(string.Format("   \"{0}\" ({1}): ve {2}, o chu x {3:F0}..{4:F0} y {5:F0}..{6:F0}; chop mu cao {7:F2} m, y {8:F0}; khe tu day chu den chop mu {9:F1} px (0,30 m = {10:F1} px); lech ngang so voi chan {11:F1} px; mau {12}",
                b.ten, b.laToi ? "cua minh" : "nguoi khac", vuaVe, o.xMin, o.xMax, o.yMin, o.yMax,
                caoChop, yChop, khe, px30, lechNgang, b.mauCuoi));

            if (!vuaVe) Loi(b.ten + ": khong duoc ve trong khung hinh vua roi");
            if (!trongManHinh) Loi(b.ten + ": bang ten ra ngoai man hinh");
            if (khe < -2f) Loi(b.ten + ": bang ten de len chop mu");
            if (khe > px30) Loi(b.ten + ": bang ten lo lung cach chop mu hon 0,30 m (" + khe.ToString("F0") + " px)");

            // Moi chu cua ten dang hien phai co trong font, va khong con dau roi
            if (cmap != null)
                foreach (char ch in b.ten)
                    if (!cmap.Contains(ch)) Loi(b.ten + ": chu '" + ch + "' (U+" + ((int)ch).ToString("X4") + ") khong co trong font");
            foreach (char ch in b.ten)
                if (ch >= '̀' && ch <= 'ͯ') { Loi(b.ten + ": con dau roi chua ghep"); break; }

            Color mong = b.laToi ? new Color(1.00f, 0.84f, 0.42f) : new Color(0.95f, 0.92f, 0.86f);
            if (Mathf.Abs(b.mauCuoi.r - mong.r) > 0.02f || Mathf.Abs(b.mauCuoi.b - mong.b) > 0.02f)
                Loi(b.ten + ": sai mau");

            oDaVe.Add(new KeyValuePair<string, Rect>(b.ten, o));
        }

        int chong = 0;
        for (int i = 0; i < oDaVe.Count; i++)
            for (int j = i + 1; j < oDaVe.Count; j++)
                if (oDaVe[i].Value.Overlaps(oDaVe[j].Value))
                { chong++; Ghi("   chong nhau: " + oDaVe[i].Key + " / " + oDaVe[j].Key); }
        Ghi("5. so cap bang ten de len nhau: " + chong);
        if (chong > 0) Loi("bang ten de len nhau");

        yield return Chup("bangten_1");

        // ---- 7. Nen trong suot ----
        // Do tren CHINH ANH CHUP. Nen den cu tran ra hai ben o chu vai diem anh
        // (le = max(3, 6s)) - o do khong co net chu nao. So do sang trung vi cua
        // dai sat mep ay voi dai nen xa hon mot chut. (Do trung vi ca o chu thi
        // KHONG nhay: o co chu 12 diem anh, net chu + vien chiem qua nua o - da
        // thu tren anh cu, ti so van >= 1.)
        var anh = new Texture2D(2, 2);
        anh.LoadImage(File.ReadAllBytes("PlayTestShots/bangten_1.png"));
        foreach (var b in tatCa)
        {
            float tiSo = TiSoSatMep(anh, b.oCuoi, s);
            Ghi(string.Format("7. \"{0}\": do sang dai sat mep o ten / nen xa hon = {1:F2} (nen den cu thi < 0,90)", b.ten, tiSo));
            if (tiSo < NguongNen) Loi(b.ten + ": sat mep o ten toi hon nen - con nen den phia sau");
        }
        Object.DestroyImmediate(anh);

        // Mau DOI CHUNG: ve lai DUNG nen den cu (cung cong thuc o, GiaoDien.To,
        // do duc 0,38) quanh tung bang ten, chup, do bang CUNG phep do. Phai ra
        // < 0,85 - khong thi phep do tren khong bat duoc nen den va "dat" vo nghia.
        var oCu = new List<Rect>();
        foreach (var b in tatCa) oCu.Add(b.oCuoi);
        float leCu = Mathf.Max(3f, 6f * s);
        veThu.ve = () =>
        {
            if (Event.current.type != EventType.Repaint) return;
            GUI.depth = 10;
            foreach (var o in oCu)
                GiaoDien.To(new Rect(o.x - leCu, o.y + o.height * 0.08f, o.width + leCu * 2f, o.height * 0.86f),
                            new Color(0f, 0f, 0f, 0.38f));
        };
        yield return new WaitForEndOfFrame();
        yield return Chup("bangten_doichung");
        veThu.ve = null;
        var anh2 = new Texture2D(2, 2);
        anh2.LoadImage(File.ReadAllBytes("PlayTestShots/bangten_doichung.png"));
        int batDuoc = 0;
        var sbMau = new StringBuilder();
        foreach (var o in oCu)
        {
            float t = TiSoSatMep(anh2, o, s);
            sbMau.AppendFormat(" {0:F2}", t);
            if (t < NguongNen) batDuoc++;
        }
        Ghi("   doi chung (ve lai nen den cu quanh " + oCu.Count + " bang ten): ti so" + sbMau
            + " -> bat duoc " + batDuoc + "/" + oCu.Count);
        if (batDuoc < oCu.Count) Loi("phep do nen khong bat duoc nen den doi chung - so do muc 7 vo nghia");
        Object.DestroyImmediate(anh2);

        // ---- 6. Nguoi da guc ----
        if (banSao.Count > 0)
        {
            var mau = banSao[0].GetComponent<Damageable>();
            if (mau != null) mau.Die();
            yield return new WaitForSeconds(0.6f);
            yield return new WaitForEndOfFrame();
            var b0 = banSao[0].GetComponent<BangTen>();
            float a = b0 != null ? b0.mauCuoi.a : -1f;
            Ghi(string.Format("6. \"{0}\" da guc: do duc ten {1:F2} (mong < 0,6)", b0 != null ? b0.ten : "?", a));
            if (a < 0f || a >= 0.6f) Loi("nguoi da guc ma ten khong mo di");
            yield return Chup("bangten_2_guc");
        }

        foreach (var bs in banSao) NguoiChoiKhac.Bo(bs);
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/bangten.txt", bao.ToString());
        var rac = GameObject.Find("TAM_BangTen");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh) EditorSceneManager.OpenScene(canhCu);
    }
}
