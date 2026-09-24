using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 79): ACT2 VONG NGAY DEM LAP LAI - DEM 4 PHUT -> NGAY 2 PHUT -> CHIEU 2 PHUT -> DEM ...
///
/// Nguoi dung 25/09/2026: "vua moi bat dau game se la ban dem trong 4ph roi chuyen sang ban ngay trong 2ph roi
/// chuyen sang buoi chieu 2ph roi cu lap di lap lai" - chon "chuyen muot 30 giay" (30 giay cuoi moi buoi).
/// (Truoc do 24/09/2026: ngay -> xe chieu -> dem 4 phut roi dung.)
///
/// Do bang SO:
///   A. VUA VAO TRAN la BAN DEM that (so voi moc dem do 19/09/2026) va ca muoi lo lua dang chay.
///   B. Ca vong 8 phut, 31 moc (ca hai vong): B1 moi buoi GIU NGUYEN anh sang (doi chung: ba buoi khac han
///      nhau); B2 binh minh / chieu xuong / hoang hon di MOT CHIEU va cuoi moi lan chuyen trung buoi sau;
///      B3 vong hai y het vong dau; B4 lo lua chay suot dem, tat het ngay + chieu, tat / nhom dung giay.
///   G. Lo bi Gio loc dap ban dem, hen chay lai: van dem thi chay lai (doi chung), troi da sang thi KHONG.
///   C. Dong ho chay THAT va bat lai thi canh tu ve dung buoi.
///   D. Ban dem TRUNG KHOP anh sang dem cu.  F. CHI ACT2.
///
/// Ket qua: PlayTestShots/anhsang_chieu_dem.txt, anh anhsang_*.png.
/// </summary>
public static class ThuAnhSangChieuDem
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    // ---- Moc ANH SANG DEM do duoc 19/09/2026, TRUOC khi them tinh nang nay ----
    // Nguoi dung chot "dem toi giong hien gio", nen day la cai dich phai tra ve dung.
    static readonly Color DemDen = new Color(0.520f, 0.720f, 0.980f);
    const float DemManh = 0.88f, DemBong = 0.62f;
    static readonly Vector3 DemGoc = new Vector3(42f, 148f, 0f);
    static readonly Color DemAmbTroi = new Color(0.120f, 0.205f, 0.265f);
    static readonly Color DemAmbNgang = new Color(0.085f, 0.155f, 0.190f);
    static readonly Color DemAmbDat = new Color(0.045f, 0.085f, 0.105f);
    static readonly Color DemSuong = new Color(0.115f, 0.175f, 0.215f);
    const float DemDamSuong = 0.0105f, DemSao = 1f, DemSangDia = 0.9f;

    [MenuItem("Diablo 2.5D/79. Chay thu ANH SANG vong dem -> ngay -> chieu (Act2)", false, 168)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu anh sang",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        // Phep thu NAY do chinh cai chuyen anh sang, nen khong duoc nhay thang toi dem nhu cac phep thu khac
        ChuyenChieuSangDem.ChoPhepChuyenTrongPhepThu = true;
        Ghi("[ban 3] Act2: vong " + ChuyenChieuSangDem.ChuKy + " giay - dem " + ChuyenChieuSangDem.GiayDem + " / ngay "
            + ChuyenChieuSangDem.GiayNgay + " / chieu " + ChuyenChieuSangDem.GiayChieu + ", chuyen " + ChuyenChieuSangDem.GiayChuyenBuoi + " giay");

        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_AnhSang") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_AnhSang");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[AnhSang] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    /// <summary>Do sang cam nhan duoc cua mot mau (kieu mat nguoi).</summary>
    static float Sang(Color c) { return 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b; }

    /// <summary>Do "am" cua mau: do tru lam. Duong = nga cam, am = nga xanh lanh.</summary>
    static float Am(Color c) { return c.r - c.b; }

    static float SoTroi(string ten, float neuThieu)
    {
        var m = RenderSettings.skybox;
        return m != null && m.HasProperty(ten) ? m.GetFloat(ten) : neuThieu;
    }

    static Light DenHuong()
    {
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) return l;
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>Do sang cam nhan duoc cua CAI DEN HUONG (mau x do manh) - con so mat nguoi thay.</summary>
    static float SangDen(Light den) { return Sang(den.color) * den.intensity; }

    /// <summary>Mot lan do o giay g trong vong.</summary>
    struct Moc { public float g; public string buoi; public float sangDen, am, amb, sao, goc; public int lo; }

    static IEnumerator DoMoc(ChuyenChieuSangDem chuyen, Light den, LoLuaDa[] dsLo, float g, System.Collections.Generic.List<Moc> ra)
    {
        chuyen.ApGiay(g);
        yield return null;
        var m = new Moc
        {
            g = g, buoi = ChuyenChieuSangDem.TenBuoi(g), sangDen = SangDen(den), am = Am(den.color),
            amb = Sang(RenderSettings.ambientEquatorColor), sao = SoTroi("_StarAmount", -1f), goc = den.transform.eulerAngles.x,
        };
        foreach (var lo in dsLo) if (lo != null && lo.DangChay) m.lo++;
        ra.Add(m);
        Ghi(string.Format("    giay {0,6:F1} {1,-12}: den sang {2:F3} am {3:F3} | moi truong {4:F4} | sao {5:F2} | goc {6,5:F1} | lo chay {7}",
            m.g, m.buoi, m.sangDen, m.am, m.amb, m.sao, m.goc, m.lo));
    }

    static Moc Tim(System.Collections.Generic.List<Moc> ds, float g)
    {
        foreach (var m in ds) if (Mathf.Abs(m.g - g) < 0.001f) return m;
        return default(Moc);
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.0f);

        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;                 // dot quai sinh giua chung lam nhieu anh chup
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);

        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        var den = DenHuong();
        var dsLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None);
        if (chuyen == null || den == null)
        {
            Ghi("[LOI] khong tim thay ChuyenChieuSangDem hoac den huong trong Act2");
            loi++; Ket(); yield break;
        }

        // ================= A. VUA VAO TRAN LA BAN DEM, LO DANG CHAY =================
        // Doc TRUOC khi dat tay bat ky moc nao - day la canh that cua tran vua bat dau
        float giayDau = chuyen.GiayTrongVong;
        int loDau = 0; foreach (var lo in dsLo) if (lo.DangChay) loDau++;
        Ghi("");
        Ghi(string.Format("A. vua vao tran (giay {0:F2} trong vong, buoi '{1}'): den {2} manh {3:F3} goc {4:F1}; moi truong {5}; sao {6:F2}; lo dang chay {7}/{8}",
            giayDau, ChuyenChieuSangDem.TenBuoi(giayDau), den.color, den.intensity, den.transform.eulerAngles.x,
            RenderSettings.ambientEquatorColor, SoTroi("_StarAmount", -1f), loDau, dsLo.Length));
        Kiem(giayDau < 10f, "dong ho vong ngay dem khong bat dau tu luc vao tran");
        Kiem(SoSanh(den.color, DemDen) && Mathf.Abs(den.intensity - DemManh) < 0.002f
             && SoSanh(RenderSettings.ambientEquatorColor, DemAmbNgang) && Mathf.Abs(SoTroi("_StarAmount", -1f) - DemSao) < 0.01f,
             "vua vao tran ma khong phai ban dem (nguoi dung: bat dau game la ban dem)");
        Kiem(dsLo.Length >= 10 && loDau == dsLo.Length, "vua vao tran (ban dem) ma lo lua khong chay du");
        yield return Chup("anhsang_1_dem_dau_tran");

        // ================= B. CA VONG 8 PHUT, DO TUNG MOC =================
        // ⚠️ PHAI TAT component truoc khi dat tay: Update goi ApGiay(dong ho) moi khung va keo ve cho cu.
        chuyen.enabled = false;
        Ghi("");
        Ghi(string.Format("B. ca vong {0:F0} giay (dem {1:F0} / ngay {2:F0} / chieu {3:F0}, chuyen {4:F0} giay cuoi moi buoi); lo tat o giay {5:F2}, nhom o giay {6:F2}",
            ChuyenChieuSangDem.ChuKy, ChuyenChieuSangDem.GiayDem, ChuyenChieuSangDem.GiayNgay, ChuyenChieuSangDem.GiayChieu,
            ChuyenChieuSangDem.GiayChuyenBuoi, ChuyenChieuSangDem.GiayTatLua(), ChuyenChieuSangDem.GiayNhomLua()));
        float tat = ChuyenChieuSangDem.GiayTatLua(), nhom = ChuyenChieuSangDem.GiayNhomLua();
        var mocs = new float[] { 0f, 100f, 205f, 212f, 218f, tat - 0.3f, tat + 0.3f, 228f, 235f, 239.9f, 245f, 300f, 325f,
                                 333f, 340f, 345f, 352f, 359.9f, 365f, 420f, 445f, 453f, 460f, nhom - 0.3f, nhom + 0.3f, 472f, 479.9f,
                                 480f, 480f + 300f, 480f + 420f, 960f + 100f };
        var ds = new System.Collections.Generic.List<Moc>();
        for (int i = 0; i < mocs.Length; i++)
        {
            yield return DoMoc(chuyen, den, dsLo, mocs[i], ds);
            if (Mathf.Approximately(mocs[i], 228f)) yield return Chup("anhsang_2_binh_minh");
            if (Mathf.Approximately(mocs[i], 300f)) yield return Chup("anhsang_3_ngay");
            if (Mathf.Approximately(mocs[i], 345f)) yield return Chup("anhsang_4_chieu_xuong");
            if (Mathf.Approximately(mocs[i], 420f)) yield return Chup("anhsang_5_chieu");
            if (Mathf.Approximately(mocs[i], 465f) || Mathf.Approximately(mocs[i], 460f)) yield return Chup("anhsang_6_hoang_hon");
        }

        // B1. Moi buoi GIU NGUYEN anh sang cua no
        var d0 = Tim(ds, 0f); var d1 = Tim(ds, 100f); var d2 = Tim(ds, 205f);
        var n0 = Tim(ds, 245f); var n1 = Tim(ds, 300f); var n2 = Tim(ds, 325f);
        var c0 = Tim(ds, 365f); var c1 = Tim(ds, 420f); var c2 = Tim(ds, 445f);
        bool demYen = Mathf.Abs(d0.amb - d1.amb) < 1e-4f && Mathf.Abs(d0.amb - d2.amb) < 1e-4f && Mathf.Abs(d0.sangDen - d2.sangDen) < 1e-4f;
        bool ngayYen = Mathf.Abs(n0.amb - n1.amb) < 1e-4f && Mathf.Abs(n0.amb - n2.amb) < 1e-4f && Mathf.Abs(n0.sangDen - n2.sangDen) < 1e-4f;
        bool chieuYen = Mathf.Abs(c0.amb - c1.amb) < 1e-4f && Mathf.Abs(c0.amb - c2.amb) < 1e-4f && Mathf.Abs(c0.sangDen - c2.sangDen) < 1e-4f;
        Ghi(string.Format("B1. giu nguyen trong buoi: dem (0/100/205) {0}, ngay (245/300/325) {1}, chieu (365/420/445) {2}", demYen, ngayYen, chieuYen));
        Kiem(demYen && ngayYen && chieuYen, "mot buoi khong giu nguyen anh sang (nguoi dung chon: chi chuyen 30 giay cuoi)");
        Kiem(n1.goc > 45f && n1.sao <= 0.01f, "ban ngay ma mat troi khong cao / con sao");
        Kiem(c1.goc < 20f && c1.am > 0.25f && c1.sao <= 0.01f, "buoi chieu ma den khong am / mat troi khong thap / co sao");
        Kiem(Mathf.Abs(d1.goc - DemGoc.x) < 0.3f && d1.sao > 0.99f, "ban dem ma goc den / sao khong nhu dem cu");
        // DOI CHUNG: ba buoi phai khac han nhau (neu Ap khong lam gi thi ca ba bang nhau va B1 van dung)
        Kiem(n1.amb > d1.amb * 2f && n1.amb > c1.amb * 1.3f && c1.am > n1.am + 0.2f && d1.am < 0f,
             "doi chung: ba buoi khong khac nhau ro rang - phep do B1 vo nghia");

        // B2. Ba lan chuyen di MOT CHIEU, khong moc nao nguoc
        bool binhMinh = true, chieuXuong = true, hoangHon = true, saoBM = true, saoHH = true;
        float[] bm = { 205f, 212f, 218f, 228f, 235f, 239.9f, 245f };
        float[] cx = { 325f, 333f, 340f, 345f, 352f, 359.9f, 365f };
        float[] hh = { 445f, 453f, 460f, 472f, 479.9f };
        for (int i = 1; i < bm.Length; i++)
        {
            var a = Tim(ds, bm[i - 1]); var b = Tim(ds, bm[i]);
            if (b.amb < a.amb - 1e-4f) binhMinh = false;
            if (b.sao > a.sao + 1e-4f) saoBM = false;
        }
        for (int i = 1; i < cx.Length; i++)
            if (Tim(ds, cx[i]).am < Tim(ds, cx[i - 1]).am - 1e-4f) chieuXuong = false;
        for (int i = 1; i < hh.Length; i++)
        {
            var a = Tim(ds, hh[i - 1]); var b = Tim(ds, hh[i]);
            if (b.amb > a.amb + 1e-4f) hoangHon = false;
            if (b.sao < a.sao - 1e-4f) saoHH = false;
        }
        Ghi(string.Format("B2. binh minh sang dan {0}, sao tat dan {1}; chieu xuong den am dan {2}; hoang hon toi dan {3}, sao hien dan {4}",
            binhMinh, saoBM, chieuXuong, hoangHon, saoHH));
        Kiem(binhMinh && saoBM, "binh minh khong sang dan mot chieu");
        Kiem(chieuXuong, "chieu xuong khong am dan mot chieu");
        Kiem(hoangHon && saoHH, "hoang hon khong toi dan mot chieu");
        // Het hoang hon phai TRUNG KHOP dem, het binh minh trung khop ngay - khong co buoc nhay luc sang buoi
        var hetHH = Tim(ds, 479.9f); var hetBM = Tim(ds, 239.9f); var hetCX = Tim(ds, 359.9f);
        Kiem(Mathf.Abs(hetHH.amb - d1.amb) < 0.002f && Mathf.Abs(hetBM.amb - n1.amb) < 0.002f && Mathf.Abs(hetCX.am - c1.am) < 0.01f,
             "cuoi lan chuyen khong trung voi buoi sau - anh sang nhay giat khi sang buoi");

        // B3. LAP LAI: vong thu hai y het vong dau
        var l0 = Tim(ds, 480f); var l1 = Tim(ds, 780f); var l2 = Tim(ds, 900f); var l3 = Tim(ds, 1060f);
        bool lap = Mathf.Abs(l0.amb - d0.amb) < 1e-4f && Mathf.Abs(l1.amb - n1.amb) < 1e-4f && Mathf.Abs(l2.amb - c1.amb) < 1e-4f
                   && Mathf.Abs(l3.amb - d1.amb) < 1e-4f && l0.buoi == "dem" && l1.buoi == "ngay" && l2.buoi == "chieu" && l3.buoi == "dem";
        Ghi(string.Format("B3. lap lai: giay 480 '{0}', 780 '{1}', 900 '{2}', 1060 '{3}' - trung vong dau {4}", l0.buoi, l1.buoi, l2.buoi, l3.buoi, lap));
        Kiem(lap, "het 8 phut khong lap lai nhu vong dau");

        // B4. LO LUA: dem chay du, ngay / chieu tat het, tat dung luc binh minh, nhom dung luc hoang hon
        int soLo = dsLo.Length;
        bool loDem = Tim(ds, 100f).lo == soLo && Tim(ds, 205f).lo == soLo && Tim(ds, 480f + 0f).lo == soLo && Tim(ds, 1060f).lo == soLo;
        bool loNgay = Tim(ds, 245f).lo == 0 && Tim(ds, 300f).lo == 0 && Tim(ds, 365f).lo == 0 && Tim(ds, 445f).lo == 0 && Tim(ds, 780f).lo == 0;
        bool loTat = Tim(ds, tat - 0.3f).lo == soLo && Tim(ds, tat + 0.3f).lo == 0;
        bool loNhom = Tim(ds, nhom - 0.3f).lo == 0 && Tim(ds, nhom + 0.3f).lo == soLo;
        Ghi(string.Format("B4. lo lua: dem chay du {0}; ngay + chieu tat het {1}; binh minh tat dung giay {2:F1} {3}; hoang hon nhom dung giay {4:F1} {5}",
            loDem, loNgay, tat, loTat, nhom, loNhom));
        Kiem(loDem, "ban dem ma lo khong chay du");
        Kiem(loNgay, "ban ngay / buoi chieu ma lo van chay");
        Kiem(loTat, "lo khong tat dung luc troi sang");
        Kiem(loNhom, "lo khong nhom dung luc troi toi");

        // ================= G. GIO LOC DAP LO BAN DEM, SANG NGAY THI KHONG DUOC TU CHAY LAI =================
        Ghi("");
        if (dsLo.Length > 0)
        {
            var lo0 = dsLo[0];
            // Doi chung: dem dap roi hen 1 giay -> phai chay lai
            chuyen.ApGiay(100f);
            yield return null;
            if (!lo0.DangChay) lo0.Chay();
            lo0.DapTatRoiChayLai(1f);
            yield return new WaitForSeconds(1.6f);
            bool demChayLai = lo0.DangChay;
            // Ca that: dap roi hen 1 giay, nhung troi sang ngay truoc khi toi hen
            lo0.DapTatRoiChayLai(1f);
            chuyen.ApGiay(300f);
            yield return new WaitForSeconds(1.6f);
            bool ngayChayLai = lo0.DangChay;
            Ghi(string.Format("G. lo bi Gio loc dap, hen chay lai sau 1 giay: van dem -> chay lai {0} (doi chung, phai True); troi da sang ngay -> chay lai {1} (phai False)",
                demChayLai, ngayChayLai));
            Kiem(demChayLai, "doi chung hong: ban dem lo bi dap khong tu chay lai");
            Kiem(!ngayChayLai, "lo bi Gio loc dap ban dem roi TU CHAY LAI giua ban ngay");
        }

        // ================= C. DONG HO CHAY THAT =================
        Ghi("");
        chuyen.enabled = true;
        float gTruoc = chuyen.GiayTrongVong, gioTruoc = Time.timeSinceLevelLoad;
        yield return new WaitForSeconds(6f);
        float gSau = chuyen.GiayTrongVong, gioSau = Time.timeSinceLevelLoad;
        Ghi(string.Format("C. sau {0:F2} giay that: giay trong vong {1:F2} -> {2:F2} (tang {3:F2}); buoi hien tai '{4}'",
            gioSau - gioTruoc, gTruoc, gSau, gSau - gTruoc, ChuyenChieuSangDem.TenBuoi(gSau)));
        Kiem(Mathf.Abs((gSau - gTruoc) - (gioSau - gioTruoc)) < 0.05f, "dong ho vong ngay dem khong khop thoi gian that");
        // Component bat lai thi phai TU DUA canh ve dung buoi cua dong ho (luc nay dang la dem)
        Kiem(SoSanh(den.color, DemDen) && Mathf.Abs(SoTroi("_StarAmount", -1f) - DemSao) < 0.01f,
             "bat lai dong ho ma canh khong ve dung buoi dem hien tai");

        // ================= D. BAN DEM TRUNG KHOP ANH SANG DEM "HIEN GIO" =================
        Ghi("");
        chuyen.enabled = false;
        chuyen.ApGiay(ChuyenChieuSangDem.GiayGiuaDem);
        yield return null;
        Ghi(string.Format("D. ban dem: den {0} manh {1:F3} bong {2:F2} goc {3:F1}; moi truong {4}/{5}/{6}; suong {7} dam {8:F4}; sao {9:F2}; quang dia {10:F2}",
            den.color, den.intensity, den.shadowStrength, den.transform.eulerAngles.x,
            RenderSettings.ambientSkyColor, RenderSettings.ambientEquatorColor, RenderSettings.ambientGroundColor,
            RenderSettings.fogColor, RenderSettings.fogDensity, SoTroi("_StarAmount", -1f), SoTroi("_MoonStrength", -1f)));
        Kiem(SoSanh(den.color, DemDen), "mau den ban dem khong bang anh trang cu");
        Kiem(Mathf.Abs(den.intensity - DemManh) < 0.002f, "do manh ban dem khong bang cu");
        Kiem(Mathf.Abs(den.shadowStrength - DemBong) < 0.002f, "do dam bong ban dem khong bang cu");
        Kiem(Mathf.Abs(Mathf.DeltaAngle(den.transform.eulerAngles.x, DemGoc.x)) < 0.2f, "goc den ban dem khong phai 42 do");
        Kiem(SoSanh(RenderSettings.ambientSkyColor, DemAmbTroi)
             && SoSanh(RenderSettings.ambientEquatorColor, DemAmbNgang)
             && SoSanh(RenderSettings.ambientGroundColor, DemAmbDat), "anh sang moi truong ban dem khong bang cu");
        Kiem(SoSanh(RenderSettings.fogColor, DemSuong) && Mathf.Abs(RenderSettings.fogDensity - DemDamSuong) < 0.0002f,
             "suong mu ban dem khong bang cu");
        Kiem(Mathf.Abs(SoTroi("_MoonStrength", -1f) - DemSangDia) < 0.01f, "dia trang ban dem khong bang cu");

        // ================= F. CHI ACT2 =================
        Ghi("");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Act1");
        yield return new WaitForSeconds(2.5f);
        var o1 = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        var den1 = DenHuong();
        Ghi(string.Format("F. nap Act1: co ChuyenChieuSangDem = {0} (phai False); den Act1 {1} manh {2:F2}",
            o1 != null, den1 != null ? den1.color.ToString() : "KHONG", den1 != null ? den1.intensity : -1f));
        Kiem(o1 == null, "Act1 cung bi doi anh sang - nguoi dung chi xin cho Act2");

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static bool SoSanh(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.003f && Mathf.Abs(a.g - b.g) < 0.003f && Mathf.Abs(a.b - b.b) < 0.003f;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[AnhSang] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/anhsang_chieu_dem.txt", bao.ToString());
        // Tra co ve mac dinh: phep thu SAU phai duoc anh sang dung yen nhu cu
        ChuyenChieuSangDem.ChoPhepChuyenTrongPhepThu = false;
        var rac = GameObject.Find("TAM_AnhSang");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
