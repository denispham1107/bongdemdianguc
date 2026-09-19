using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 79): ACT2 VAO TRAN LUC XE CHIEU, TOI DAN THANH DEM TRONG 2 PHUT.
///
/// Nguoi dung 19/09/2026: "moi vao game hay thay doi cho toi chi trong Scene Act2 anh sang buoi xe chieu
/// roi trong vong 2 phut chuyen dan anh sang tu tu qua dem toi giong hien gio".
///
/// Do bang SO:
///   A. VUA VAO TRAN la xe chieu that: den AM (do > lam) va manh hon ban dem, suong am, CHUA CO SAO.
///      DOI CHUNG: ban dem thi nguoc lai - den LANH (lam > do).
///   B. Di mot chieu: qua 5 moc 0 / 0,25 / 0,5 / 0,75 / 1 thi do sang giam dan, mau nguoi dan,
///      sao nhieu dan - khong moc nao di nguoc.
///   C. Dong ho chay THAT: cho mot doan roi doc lai tien do, so voi thoi gian troi qua.
///   D. Het 2 phut thi TRUNG KHOP anh sang dem "hien gio" - so tung so voi moc do duoc truoc khi lam
///      tinh nang nay (nguoi dung chot "giong hien gio", nen doi mau dem la phep thu phai bao).
///   E. Anh chup 5 moc.
///   G. LO LUA DA: chua toi dem thi CA MUOI lo deu tat; vuot muc nhom lua thi ca muoi cai chay.
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

    [MenuItem("Diablo 2.5D/79. Chay thu ANH SANG xe chieu -> dem (Act2)", false, 168)]
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
        Ghi("[ban 1] Act2: vao tran luc xe chieu, toi dan thanh dem trong " + ChuyenChieuSangDem.GiayChuyen + " giay");

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

    /// <summary>Do "am" cua mau: do tru lam. Duong = ngA cam, am = ngA xanh lanh.</summary>
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

        // ================= A. VUA VAO TRAN LA XE CHIEU =================
        // ⚠️ PHAI TAT component truoc khi dat tay mot moc: Update cua no goi Ap(TienDo) MOI KHUNG,
        // nen Ap(0,5) vua dat xong la khung sau bi keo ve dung cho dong ho dang chay (lan chay dau
        // 19/09/2026: ca nam moc deu ra y nhu nhau, va muc D bao sai tat ca).
        chuyen.enabled = false;
        chuyen.Ap(0f);
        yield return null;
        float amChieu = Am(den.color), sangChieuDen = den.intensity;
        float sangAmbChieu = Sang(RenderSettings.ambientEquatorColor);
        float amSuongChieu = Am(RenderSettings.fogColor);
        float saoChieu = SoTroi("_StarAmount", -1f);
        Ghi("");
        Ghi(string.Format("A. vua vao tran: den {0} (am = do-lam = {1:F3}), manh {2:F2}, goc {3:F1} do; anh sang moi truong {4:F3}; suong am {5:F3}; sao {6:F2}",
            den.color, amChieu, sangChieuDen, den.transform.eulerAngles.x, sangAmbChieu, amSuongChieu, saoChieu));
        Kiem(amChieu > 0.25f, "den luc vua vao tran khong AM (xe chieu phai nga cam)");
        Kiem(sangChieuDen > DemManh, "den luc vua vao tran khong sang hon ban dem");
        Kiem(amSuongChieu > 0.2f, "suong luc vua vao tran khong nga am");
        Kiem(saoChieu <= 0.01f, "vua vao tran troi con sang ma da day sao");
        Kiem(den.transform.eulerAngles.x < DemGoc.x - 5f, "mat troi luc xe chieu khong THAP hon anh trang ban dem");
        yield return Chup("anhsang_1_xechieu");

        // ================= G. LO LUA DA =================
        // ⚠️ Phai do NGAY DAU, truoc moi muc khac: chi can mot lan Ap(1f) o dau do (muc B chay het nam
        // moc, muc D nhay thang toi 1) la lua da nhom - va lua KHONG tat lai nua (chi nhom mot lan),
        // nen do sau do thi moc nao cung thay du muoi lo dang chay.
        Ghi("");
        var dsLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None);
        var mocG = new float[] { 0f, 0.3f, ChuyenChieuSangDem.MucNhomLua - 0.05f, ChuyenChieuSangDem.MucNhomLua + 0.05f, 1f };
        var demChay = new int[mocG.Length];
        for (int i = 0; i < mocG.Length; i++)
        {
            // Ap() nhan TIEN DO, con muc nhom lua do theo duong cong da lam muot - doi nguoc lai
            float tG = TienDoChoMuc(mocG[i]);
            chuyen.Ap(tG);
            yield return null;
            foreach (var lo in dsLo) if (lo.DangChay) demChay[i]++;
        }
        Ghi(string.Format("G. muoi lo lua theo duong doi (muc nhom lua {0:F2}): "
            + "duong cong {1:F2} -> {2} lo chay | {3:F2} -> {4} | {5:F2} -> {6} | {7:F2} -> {8} | {9:F2} -> {10}",
            ChuyenChieuSangDem.MucNhomLua,
            mocG[0], demChay[0], mocG[1], demChay[1], mocG[2], demChay[2], mocG[3], demChay[3], mocG[4], demChay[4]));
        Kiem(dsLo.Length >= 10, "khong tim thay du muoi lo lua trong Act2");
        Kiem(demChay[0] == 0 && demChay[1] == 0 && demChay[2] == 0, "chua toi dem ma lo lua da chay");
        Kiem(demChay[3] == dsLo.Length && demChay[4] == dsLo.Length, "toi dem roi ma lo lua khong chay du ca muoi cai");
        yield return Chup("anhsang_6_lo_lua_dem");

        // ================= B. DI MOT CHIEU =================
        Ghi("");
        Ghi("B. nam moc tren duong doi (0 = vua vao, 1 = dem han)");
        float[] moc = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        var sangDen = new float[moc.Length];
        var amDen = new float[moc.Length];
        var sangAmb = new float[moc.Length];
        var sao = new float[moc.Length];
        for (int i = 0; i < moc.Length; i++)
        {
            chuyen.Ap(moc[i]);
            yield return null;
            sangDen[i] = den.intensity;
            amDen[i] = Am(den.color);
            sangAmb[i] = Sang(RenderSettings.ambientEquatorColor);
            sao[i] = SoTroi("_StarAmount", -1f);
            Ghi(string.Format("    t={0:F2}: den manh {1:F3} am {2:F3} | moi truong {3:F4} | suong dam {4:F4} | sao {5:F2} | goc {6:F1}",
                moc[i], sangDen[i], amDen[i], sangAmb[i], RenderSettings.fogDensity, sao[i], den.transform.eulerAngles.x));
            if (i == 1) yield return Chup("anhsang_2_hoangHon");
            if (i == 2) yield return Chup("anhsang_3_chapVang");
            if (i == 3) yield return Chup("anhsang_4_gan_toi");
        }
        bool giamDan = true, nguoiDan = true, saoTangDan = true;
        for (int i = 1; i < moc.Length; i++)
        {
            if (sangDen[i] > sangDen[i - 1] + 0.001f) giamDan = false;
            if (amDen[i] > amDen[i - 1] + 0.001f) nguoiDan = false;
            if (sao[i] < sao[i - 1] - 0.001f) saoTangDan = false;
        }
        Ghi(string.Format("B1. den yeu dan {0}; mau nguoi dan (bot am) {1}; sao nhieu dan {2}; moi truong toi dan {3}",
            giamDan, nguoiDan, saoTangDan, sangAmb[4] < sangAmb[0]));
        Kiem(giamDan, "den khong yeu dan deu - co moc di nguoc");
        Kiem(nguoiDan, "mau den khong nguoi dan deu - co moc am tro lai");
        Kiem(saoTangDan, "sao khong nhieu dan deu");
        Kiem(sangAmb[4] < sangAmb[0] * 0.6f, "anh sang moi truong cuoi chang khong toi han so voi luc dau");

        // ================= C. DONG HO CHAY THAT =================
        Ghi("");
        chuyen.enabled = true;
        float tTruoc = chuyen.TienDo, gioTruoc = Time.timeSinceLevelLoad;
        yield return new WaitForSeconds(6f);
        float tSau = chuyen.TienDo, gioSau = Time.timeSinceLevelLoad;
        float tienDoTheoGio = (gioSau - gioTruoc) / ChuyenChieuSangDem.GiayChuyen;
        Ghi(string.Format("C. sau {0:F2} giay that: tien do {1:F4} -> {2:F4} (tang {3:F4}, mong {4:F4})",
            gioSau - gioTruoc, tTruoc, tSau, tSau - tTruoc, tienDoTheoGio));
        Kiem(Mathf.Abs((tSau - tTruoc) - tienDoTheoGio) < 0.005f, "dong ho chuyen anh sang khong khop thoi gian that");

        // ================= D. HET GIO THI TRUNG KHOP ANH SANG DEM =================
        Ghi("");
        chuyen.enabled = false;              // lai phai tat: Update se keo ve cho dong ho dang chay
        chuyen.Ap(1f);
        yield return null;
        Ghi(string.Format("D. het {0} giay: den {1} manh {2:F3} bong {3:F2} goc {4:F1}; moi truong {5}/{6}/{7}; suong {8} dam {9:F4}; sao {10:F2}; quang dia {11:F2}",
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

        // DOI CHUNG cho muc A: ban dem thi den LANH chu khong am
        Ghi(string.Format("D2. DOI CHUNG: luc dem den am = {0:F3} (luc xe chieu {1:F3})", Am(den.color), amChieu));
        Kiem(Am(den.color) < 0f, "doi chung hong: den ban dem cung nga am, vay muc A khong chung minh duoc gi");

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

    /// <summary>Tien do t sao cho duong cong smoothstep(t) = muc mong muon (dao nguoc smoothstep).</summary>
    static float TienDoChoMuc(float muc)
    {
        muc = Mathf.Clamp01(muc);
        float lo = 0f, hi = 1f;
        for (int i = 0; i < 40; i++)                // chia doi 40 lan - dung tran, khong vong vo han
        {
            float giua = (lo + hi) * 0.5f;
            if (giua * giua * (3f - 2f * giua) < muc) lo = giua; else hi = giua;
        }
        return (lo + hi) * 0.5f;
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
