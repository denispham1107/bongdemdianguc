using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 79): ACT2 VAO TRAN LUC BAN NGAY -> 2 PHUT SAU XE CHIEU -> 2 PHUT NUA LA DEM.
///
/// Nguoi dung 19/09/2026: vao tran luc xe chieu, 2 phut sau thanh dem "giong hien gio".
/// Nguoi dung 24/09/2026: "luc moi vao game cho them anh sang ban ngay roi trong vong 2 phut chuyen dan
/// anh sang sang buoi xe chieu, khi khong phai ban dem thi cho tat lua o Lo Lua" - va chot mach 4 phut:
/// NGAY 0 s -> XE CHIEU 120 s -> DEM 240 s.
///
/// Do bang SO:
///   A. VUA VAO TRAN la BAN NGAY that: mat troi CAO, anh sang moi truong sang nhat ca ba, den it am hon
///      xe chieu han, chua co sao; dong ho thuc cua tran vua bat dau (tien do gan 0).
///   A2. Dung GIUA duong (tien do 0,5) la XE CHIEU: den am, mat troi thap, chua co sao.
///   G. LO LUA DA: ca chang ngay lan chang xe chieu deu TAT; qua muc nhom lua thi ca muoi cai chay.
///      (Phai do NGAY SAU A, truoc moi muc khac - lua nhom roi thi khong tat lai nua.)
///   B. Chin moc 0 -> 1: den toi dan, moi truong toi dan, sao khong bao gio bot - khong moc nao di nguoc;
///      mau den AM DAN o chang dau roi NGUOI DAN o chang sau.
///   C. Dong ho chay THAT: cho mot doan roi doc lai tien do, so voi thoi gian troi qua (tren 4 phut).
///   D. Het 4 phut thi TRUNG KHOP anh sang dem "hien gio" - so tung so voi moc do duoc 19/09/2026, truoc
///      khi co tinh nang nay. DOI CHUNG: ban dem den LANH, nguoc han luc xe chieu.
///   E. Anh chup: ngay, giua ngay-chieu, xe chieu, gan toi, dem co lo lua.
///   F. CHI ACT2: nap Act1 roi kiem khong co ChuyenChieuSangDem nao.
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

    [MenuItem("Diablo 2.5D/79. Chay thu ANH SANG ngay -> xe chieu -> dem (Act2)", false, 168)]
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
        Ghi("[ban 2] Act2: ban ngay -> " + ChuyenChieuSangDem.GiayMoiChang + " giay sau xe chieu -> "
            + ChuyenChieuSangDem.GiayChuyen + " giay la dem");

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
        if (chuyen == null || den == null)
        {
            Ghi("[LOI] khong tim thay ChuyenChieuSangDem hoac den huong trong Act2");
            loi++; Ket(); yield break;
        }

        // Dong ho THAT cua tran vua bat dau chua (doc truoc khi dat tay bat ky moc nao)
        float tienDoLucDau = chuyen.TienDo;
        float giayLucDau = Time.timeSinceLevelLoad;

        // ================= A. VUA VAO TRAN LA BAN NGAY =================
        // ⚠️ PHAI TAT component truoc khi dat tay mot moc: Update cua no goi Ap(TienDo) MOI KHUNG,
        // nen Ap(0,5) vua dat xong la khung sau bi keo ve dung cho dong ho dang chay (lan chay dau
        // 19/09/2026: ca nam moc deu ra y nhu nhau, va muc D bao sai tat ca).
        chuyen.enabled = false;
        chuyen.Ap(0f);
        yield return null;
        float amNgay = Am(den.color), sangDenNgay = SangDen(den), gocNgay = den.transform.eulerAngles.x;
        float ambNgay = Sang(RenderSettings.ambientEquatorColor);
        float saoNgay = SoTroi("_StarAmount", -1f);
        Ghi("");
        Ghi(string.Format("A. vua vao tran (giay {0:F2}, tien do that {1:F4}): den {2} am {3:F3}, do sang den {4:F3}, goc {5:F1} do; moi truong {6:F3}; suong {7}; sao {8:F2}",
            giayLucDau, tienDoLucDau, den.color, amNgay, sangDenNgay, gocNgay, ambNgay, RenderSettings.fogColor, saoNgay));
        Kiem(tienDoLucDau < 0.02f, "vao tran ma dong ho da chay xa - khong phai luc bat dau ban ngay");
        Kiem(gocNgay > 45f, "ban ngay ma mat troi khong cao");
        Kiem(saoNgay <= 0.01f, "ban ngay ma da co sao");
        yield return Chup("anhsang_1_ngay");

        // ⚠️ MOI anh chup truoc muc nhom lua phai chup o DAY, truoc muc G: sau G lua da nhom va khong tat
        // lai, anh "buoi chieu" se hien lo dang chay - sai voi game that (lan chay dau 24/09/2026).
        chuyen.Ap(0.25f);
        yield return null;
        yield return Chup("anhsang_2_giua_ngay_chieu");

        // ================= A2. GIUA DUONG LA XE CHIEU =================
        chuyen.Ap(ChuyenChieuSangDem.TienDoXeChieu);
        yield return null;
        float amChieu = Am(den.color), sangDenChieu = SangDen(den), gocChieu = den.transform.eulerAngles.x;
        float ambChieu = Sang(RenderSettings.ambientEquatorColor);
        float saoChieu = SoTroi("_StarAmount", -1f);
        Ghi(string.Format("A2. giua duong (tien do {0:F2} = giay {1:F0}): den {2} am {3:F3}, do sang den {4:F3}, goc {5:F1} do; moi truong {6:F3}; sao {7:F2}",
            ChuyenChieuSangDem.TienDoXeChieu, ChuyenChieuSangDem.TienDoXeChieu * ChuyenChieuSangDem.GiayChuyen,
            den.color, amChieu, sangDenChieu, gocChieu, ambChieu, saoChieu));
        Kiem(amChieu > 0.25f, "xe chieu ma den khong nga cam");
        Kiem(gocChieu < 20f, "xe chieu ma mat troi khong thap");
        Kiem(saoChieu <= 0.01f, "xe chieu ma da co sao");
        // So ngay voi xe chieu - DOI CHUNG cho muc A: ngay phai sang hon va it am hon han
        Kiem(ambNgay > ambChieu * 1.3f, "ban ngay khong sang hon xe chieu ro rang");
        Kiem(sangDenNgay > sangDenChieu, "den ban ngay khong sang hon den xe chieu");
        Kiem(amNgay < amChieu - 0.2f, "den ban ngay am gan bang xe chieu - khong phan biet duoc hai luc");
        yield return Chup("anhsang_3_xechieu");

        // Gan toi nhung CHUA toi muc nhom lua - lo van phai tat trong anh
        chuyen.Ap(0.75f);
        yield return null;
        yield return Chup("anhsang_4_gan_toi");

        // ================= G. LO LUA DA =================
        // ⚠️ Phai do o day, truoc moi muc khac: chi can mot lan Ap(1f) (muc B chay het chin moc, muc D nhay
        // thang toi 1) la lua da nhom - va lua KHONG tat lai nua (chi nhom mot lan), nen do sau do thi moc
        // nao cung thay du muoi lo dang chay. Cac moc phai TANG DAN.
        Ghi("");
        var dsLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None);
        float tNhom = ChuyenChieuSangDem.TienDoNhomLua();
        var mocG = new float[] { 0f, 0.25f, ChuyenChieuSangDem.TienDoXeChieu, 0.65f, tNhom - 0.02f, tNhom + 0.02f, 1f };
        var demChay = new int[mocG.Length];
        var sbG = new StringBuilder();
        for (int i = 0; i < mocG.Length; i++)
        {
            chuyen.Ap(mocG[i]);
            yield return null;
            foreach (var lo in dsLo) if (lo.DangChay) demChay[i]++;
            sbG.AppendFormat("{0:F3} (giay {1:F0}) -> {2} lo | ", mocG[i], mocG[i] * ChuyenChieuSangDem.GiayChuyen, demChay[i]);
        }
        Ghi(string.Format("G. muoi lo lua (nhom tai tien do {0:F3} = giay {1:F0}): {2}",
            tNhom, tNhom * ChuyenChieuSangDem.GiayChuyen, sbG.ToString().TrimEnd(' ', '|')));
        Kiem(dsLo.Length >= 10, "khong tim thay du muoi lo lua trong Act2");
        bool tatHet = true;
        for (int i = 0; i < mocG.Length; i++) if (mocG[i] < tNhom && demChay[i] != 0) tatHet = false;
        Kiem(tatHet, "chua toi dem (ngay / xe chieu / chang toi dan) ma lo lua da chay");
        Kiem(demChay[mocG.Length - 2] == dsLo.Length && demChay[mocG.Length - 1] == dsLo.Length,
             "toi dem roi ma lo lua khong chay du ca muoi cai");
        Kiem(tNhom > ChuyenChieuSangDem.TienDoXeChieu, "lo lua nhom ngay trong chang ban ngay / xe chieu");
        yield return Chup("anhsang_6_lo_lua_dem");

        // ================= B. DI MOT CHIEU =================
        Ghi("");
        Ghi("B. chin moc tren duong doi (0 = ngay, 0,5 = xe chieu, 1 = dem)");
        int soMoc = 9;
        var sangDen = new float[soMoc];
        var amDen = new float[soMoc];
        var sangAmb = new float[soMoc];
        var sao = new float[soMoc];
        for (int i = 0; i < soMoc; i++)
        {
            float t = i / (float)(soMoc - 1);
            chuyen.Ap(t);
            yield return null;
            sangDen[i] = SangDen(den);
            amDen[i] = Am(den.color);
            sangAmb[i] = Sang(RenderSettings.ambientEquatorColor);
            sao[i] = SoTroi("_StarAmount", -1f);
            Ghi(string.Format("    t={0:F3} (giay {1,3:F0}): den sang {2:F3} am {3:F3} | moi truong {4:F4} | suong dam {5:F4} | sao {6:F2} | goc {7:F1}",
                t, t * ChuyenChieuSangDem.GiayChuyen, sangDen[i], amDen[i], sangAmb[i], RenderSettings.fogDensity, sao[i], den.transform.eulerAngles.x));
        }
        int giua = (soMoc - 1) / 2;                   // moc xe chieu
        bool denToiDan = true, ambToiDan = true, saoKhongBot = true, amTangChangDau = true, amGiamChangSau = true;
        for (int i = 1; i < soMoc; i++)
        {
            if (sangDen[i] > sangDen[i - 1] + 0.001f) denToiDan = false;
            if (sangAmb[i] > sangAmb[i - 1] + 0.001f) ambToiDan = false;
            if (sao[i] < sao[i - 1] - 0.001f) saoKhongBot = false;
            if (i <= giua && amDen[i] < amDen[i - 1] - 0.001f) amTangChangDau = false;
            if (i > giua && amDen[i] > amDen[i - 1] + 0.001f) amGiamChangSau = false;
        }
        Ghi(string.Format("B1. den toi dan {0}; moi truong toi dan {1}; sao khong bao gio bot {2}; mau den AM DAN o chang dau {3}, NGUOI DAN o chang sau {4}",
            denToiDan, ambToiDan, saoKhongBot, amTangChangDau, amGiamChangSau));
        Kiem(denToiDan, "den khong toi dan deu - co moc sang tro lai");
        Kiem(ambToiDan, "anh sang moi truong khong toi dan deu");
        Kiem(saoKhongBot, "sao bot di o mot moc nao do");
        Kiem(amTangChangDau, "chang ngay -> xe chieu: mau den khong am dan");
        Kiem(amGiamChangSau, "chang xe chieu -> dem: mau den khong nguoi dan");
        Kiem(sangAmb[soMoc - 1] < sangAmb[0] * 0.4f, "cuoi chang moi truong khong toi han so voi ban ngay");

        // ================= C. DONG HO CHAY THAT =================
        Ghi("");
        chuyen.enabled = true;
        float tTruoc = chuyen.TienDo, gioTruoc = Time.timeSinceLevelLoad;
        yield return new WaitForSeconds(6f);
        float tSau = chuyen.TienDo, gioSau = Time.timeSinceLevelLoad;
        float tienDoTheoGio = (gioSau - gioTruoc) / ChuyenChieuSangDem.GiayChuyen;
        Ghi(string.Format("C. sau {0:F2} giay that: tien do {1:F4} -> {2:F4} (tang {3:F4}, mong {4:F4} = giay / {5:F0})",
            gioSau - gioTruoc, tTruoc, tSau, tSau - tTruoc, tienDoTheoGio, ChuyenChieuSangDem.GiayChuyen));
        Kiem(Mathf.Abs((tSau - tTruoc) - tienDoTheoGio) < 0.003f, "dong ho chuyen anh sang khong khop thoi gian that");

        // ================= D. HET GIO THI TRUNG KHOP ANH SANG DEM =================
        Ghi("");
        chuyen.enabled = false;              // lai phai tat: Update se keo ve cho dong ho dang chay
        chuyen.Ap(1f);
        yield return null;
        Ghi(string.Format("D. het {0:F0} giay: den {1} manh {2:F3} bong {3:F2} goc {4:F1}; moi truong {5}/{6}/{7}; suong {8} dam {9:F4}; sao {10:F2}; quang dia {11:F2}",
            ChuyenChieuSangDem.GiayChuyen, den.color, den.intensity, den.shadowStrength, den.transform.eulerAngles.x,
            RenderSettings.ambientSkyColor, RenderSettings.ambientEquatorColor, RenderSettings.ambientGroundColor,
            RenderSettings.fogColor, RenderSettings.fogDensity, SoTroi("_StarAmount", -1f), SoTroi("_MoonStrength", -1f)));
        Kiem(SoSanh(den.color, DemDen), "mau den cuoi cung khong bang anh trang ban dem");
        Kiem(Mathf.Abs(den.intensity - DemManh) < 0.002f, "do manh cuoi cung khong bang ban dem");
        Kiem(Mathf.Abs(den.shadowStrength - DemBong) < 0.002f, "do dam bong cuoi cung khong bang ban dem");
        Kiem(Mathf.Abs(Mathf.DeltaAngle(den.transform.eulerAngles.x, DemGoc.x)) < 0.2f, "goc den cuoi cung khong ve 42 do");
        Kiem(SoSanh(RenderSettings.ambientSkyColor, DemAmbTroi)
             && SoSanh(RenderSettings.ambientEquatorColor, DemAmbNgang)
             && SoSanh(RenderSettings.ambientGroundColor, DemAmbDat), "anh sang moi truong cuoi cung khong bang ban dem");
        Kiem(SoSanh(RenderSettings.fogColor, DemSuong) && Mathf.Abs(RenderSettings.fogDensity - DemDamSuong) < 0.0002f,
             "suong mu cuoi cung khong bang ban dem");
        Kiem(Mathf.Abs(SoTroi("_StarAmount", -1f) - DemSao) < 0.01f, "sao cuoi cung khong day nhu ban dem");
        Kiem(Mathf.Abs(SoTroi("_MoonStrength", -1f) - DemSangDia) < 0.01f, "dia trang cuoi cung khong bang ban dem");
        yield return Chup("anhsang_5_dem");

        Ghi(string.Format("D2. DOI CHUNG: luc dem den am = {0:F3} (xe chieu {1:F3}, ban ngay {2:F3})", Am(den.color), amChieu, amNgay));
        Kiem(Am(den.color) < 0f, "doi chung hong: den ban dem cung nga am");

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
