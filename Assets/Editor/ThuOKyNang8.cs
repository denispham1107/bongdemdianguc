using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 120 - 8 O KY NANG (nguoi dung 10/10/2026: them mot o vuong ban may tinh + mot nut tron ban cam ung o cho khoanh do,
/// sap lai bo cuc khong chong nhau, khong tran mep duoi / mep phai / ra ngoai Sach phep). Play Act2.
///   A. CAM UNG o 8 co man NGANG (H/1080 = ti le): 8 nut nam trong man (mep phai, mep duoi), ho giua hai nut bat ky, dau "+" khong de
///      len nut khac, khong cham can dieu khien va cot nut goc phai tren.
///   B. MAY TINH: 8 o vuong + dong chu phim tat nam tron trong man, khong chong nhau.
///   C. SACH PHEP (ca hai ban, 8 co man): 8 o (+ so thu tu ban may tinh) nam trong vung o va trong khung cua so, khong chong nhau,
///      nut tron khong de len dong nhac.
///   D. BAN GHI CU 7 O: giu cach xep, o 8 = Nhao lon (neu Nhao lon da o o cu thi o 8 trong); mac dinh moi = 0..6 + Nhao lon.
///   E. Anh: PlayTestShots/o8_*.png.
/// </summary>
public static class ThuOKyNang8
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    static readonly Vector2[] CoMan = {
        new Vector2(844, 390), new Vector2(1280, 720), new Vector2(1619, 580), new Vector2(1920, 1080),
        new Vector2(2400, 1080), new Vector2(2778, 1284), new Vector2(1024, 768), new Vector2(2048, 1536) };

    [MenuItem("Diablo 2.5D/120. Chay thu 8 O KY NANG (bo cuc cam ung + may tinh + Sach phep)", false, 213)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[O8] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] 8 o ky nang");
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
        if (GameObject.Find("TAM_O8") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_O8");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[O8] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 90 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    static bool ChongChuNhat(Rect a, Rect b) { return a.xMin < b.xMax && b.xMin < a.xMax && a.yMin < b.yMax && b.yMin < a.yMax; }

    // ---------------- A: cam ung ----------------
    static void DoCamUng()
    {
        const int n = SachPhep.SoOTron;
        float rU = 65.5776f;
        // Hinh hoc thuan (don vi bo cuc, tu goc phai duoi)
        float hoMin = float.MaxValue; string capHep = "";
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                float d = Vector2.Distance(GameHUD.LechNut(i), GameHUD.LechNut(j)) - 2f * rU;
                if (d < hoMin) { hoMin = d; capHep = (i + 1) + "-" + (j + 1); }
            }
        // dau "+": tam cao 0,9r tren tam nut, ban kinh 0,36r - khong de len nut KHAC
        float hoCong = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            Vector2 c = GameHUD.LechNut(i) + new Vector2(0f, 0.9f * rU);
            for (int j = 0; j < n; j++) if (j != i) hoCong = Mathf.Min(hoCong, Vector2.Distance(c, GameHUD.LechNut(j)) - rU - 0.36f * rU);
        }
        Vector2 l8 = GameHUD.LechNut(7);
        Ghi(string.Format("A0. {0} nut; nut 8 lech ({1:F0}, {2:F0}) tu goc phai duoi; ho hep nhat giua hai nut {3:F1} ({4}), duong kinh {5:F1}; dau \"+\" cach nut khac >= {6:F1}",
            n, l8.x, l8.y, hoMin, capHep, 2f * rU, hoCong));
        Kiem(n == 8, "khong phai 8 nut tron");
        Kiem(hoMin >= 10f, "hai nut tron gan / chong nhau");
        Kiem(hoCong >= 0f, "dau \"+\" de len nut khac");

        foreach (var m in CoMan)
        {
            float W = m.x, H = m.y, s = H / 1080f;
            float r = rU * s;
            float lePhai = float.MaxValue, leDuoi = float.MaxValue, cachCan = float.MaxValue, cachCot = float.MaxValue, leTrai = float.MaxValue;
            Vector2 can = GameHUD.TamJoystick(s); float rCan = GameHUD.BanKinhJoystick(s);
            for (int i = 0; i < n; i++)
            {
                Vector2 l = GameHUD.LechNut(i);
                Vector2 c = new Vector2(W - l.x * s, l.y * s);           // y tu DUOI len
                lePhai = Mathf.Min(lePhai, W - (c.x + r));
                leDuoi = Mathf.Min(leDuoi, c.y - r);
                leTrai = Mathf.Min(leTrai, c.x - r);
                cachCan = Mathf.Min(cachCan, Vector2.Distance(c, can) - r - rCan);
                // cot goc phai tren: thoat / mat / sach (tam W-62s, H-62s - k*88s, ban kinh 40s) + chu "Sach phep" duoi nut sach ~ 40s
                for (int k = 0; k < 3; k++)
                {
                    Vector2 g = new Vector2(W - 62f * s, H - 62f * s - k * GameHUD.KhoangCotGoc * s);
                    float rg = 40f * s + (k == 2 ? 40f * s : 0f);
                    cachCot = Mathf.Min(cachCot, Vector2.Distance(c, g) - r - rg);
                }
            }
            Ghi(string.Format("A. {0}x{1} (s {2:F3}): le phai {3:F1}, le duoi {4:F1}, le trai {5:F0}, cach can dieu khien {6:F0}, cach cot goc phai tren {7:F0} (diem anh)",
                W, H, s, lePhai, leDuoi, leTrai, cachCan, cachCot));
            Kiem(lePhai >= 0f && leDuoi >= 2f, "cam ung " + W + "x" + H + ": nut tran mep phai / mep duoi");
            Kiem(cachCan > 0f && cachCot > 0f && leTrai > 0f, "cam ung " + W + "x" + H + ": nut cham can dieu khien / cot goc / tran trai");
        }
    }

    // ---------------- B: may tinh ----------------
    static void DoMayTinh()
    {
        foreach (var m in CoMan)
        {
            float W = m.x, H = m.y, s = H / 1080f;
            const int n = SachPhep.SoOVuong;
            float leTrai = float.MaxValue, lePhai = float.MaxValue, leDuoiChu = float.MaxValue; bool chong = false;
            for (int i = 0; i < n; i++)
            {
                Rect r = GameHUD.RectOVuong(i, s, W, H);
                leTrai = Mathf.Min(leTrai, r.xMin); lePhai = Mathf.Min(lePhai, W - r.xMax);
                // dong chu phim tat: tu r.yMax + 2s, cao 22s (DrawSkillSlot*), chu ~ 16s
                leDuoiChu = Mathf.Min(leDuoiChu, H - (r.yMax + 2f * s + 22f * s));
                for (int j = i + 1; j < n; j++) if (ChongChuNhat(r, GameHUD.RectOVuong(j, s, W, H))) chong = true;
            }
            Ghi(string.Format("B. {0}x{1}: {2} o vuong, le trai {3:F0} / phai {4:F0}, dong chu phim cach mep duoi {5:F1}, chong nhau {6}",
                W, H, n, leTrai, lePhai, leDuoiChu, chong));
            Kiem(n == 8 && leTrai >= 0f && lePhai >= 0f && leDuoiChu >= 0f && !chong, "may tinh " + W + "x" + H + ": o vuong tran / chong / chu bi cat");
        }
    }

    // ---------------- C: Sach phep ----------------
    static void DoSachPhep(GameHUD hud)
    {
        bool cu = hud.epCamUng;
        foreach (bool tron in new[] { true, false })
        {
            hud.epCamUng = tron; CamUng.EpBat = tron;
            foreach (var m in CoMan)
            {
                float W = m.x, H = m.y, s = H / 1080f;
                var b = CuaSoSachPhep.TinhBoCuc(W, H, s);
                int n = tron ? SachPhep.SoOTron : SachPhep.SoOVuong;
                float hoMin = float.MaxValue; bool ngoaiVung = false, ngoaiKhung = false, deNhac = false;
                Rect nhac = CuaSoSachPhep.DongNhacO(b.vungO, s);
                for (int i = 0; i < n; i++)
                {
                    Rect r = CuaSoSachPhep.OTaiVung(b.vungO, i, s);
                    Rect bao = tron ? r : Rect.MinMaxRect(r.xMin, r.yMin, r.xMax, r.yMax + 4f * s + 18f * s);   // ban may tinh: so thu tu duoi o
                    if (bao.xMin < b.vungO.xMin - 0.5f || bao.xMax > b.vungO.xMax + 0.5f || bao.yMin < b.vungO.yMin - 0.5f || bao.yMax > b.vungO.yMax + 0.5f) ngoaiVung = true;
                    if (bao.xMin < b.khung.xMin || bao.xMax > b.khung.xMax || bao.yMax > b.khung.yMax) ngoaiKhung = true;
                    if (r.yMin < nhac.yMax) deNhac = true;
                    for (int j = i + 1; j < n; j++)
                    {
                        Rect q = CuaSoSachPhep.OTaiVung(b.vungO, j, s);
                        float ho = tron ? Vector2.Distance(r.center, q.center) - r.width * 0.5f - q.width * 0.5f
                                        : Mathf.Max(q.xMin - r.xMax, r.xMin - q.xMax);
                        hoMin = Mathf.Min(hoMin, ho);
                    }
                }
                Ghi(string.Format("C. Sach phep {0} {1}x{2}: {3} o, ho hep nhat {4:F1} diem, ra ngoai vung o {5}, ra ngoai khung {6}, de len dong nhac {7}",
                    tron ? "cam ung" : "may tinh", W, H, n, hoMin, ngoaiVung, ngoaiKhung, deNhac));
                Kiem(hoMin > 0.5f && !ngoaiVung && !ngoaiKhung && !deNhac, "Sach phep " + (tron ? "cam ung " : "may tinh ") + W + "x" + H + ": o chong / tran");
            }
        }
        hud.epCamUng = cu; CamUng.EpBat = cu;
    }

    // ---------------- D: ban ghi cu ----------------
    static void DoBanGhiCu()
    {
        const string kT = "diablo25d.sachphep.tron", kV = "diablo25d.sachphep.vuong";
        string luuT = PlayerPrefs.GetString(kT, null), luuV = PlayerPrefs.GetString(kV, null);
        bool coT = PlayerPrefs.HasKey(kT), coV = PlayerPrefs.HasKey(kV);
        try
        {
            PlayerPrefs.DeleteKey(kT); PlayerPrefs.DeleteKey(kV); SachPhep.NapLai();
            string md = string.Join(",", SachPhep.OVuong);
            PlayerPrefs.SetString(kV, "6,5,4,3,2,1,0"); PlayerPrefs.SetString(kT, "2,22,0,1,3,4,5"); SachPhep.NapLai();
            string v = string.Join(",", SachPhep.OVuong), t = string.Join(",", SachPhep.OTron);
            Ghi("D. mac dinh moi: [" + md + "]; ban ghi cu 7 o vuong 6..0 -> [" + v + "]; ban ghi cu tron co san Nhao lon o o 2 -> [" + t + "]");
            Kiem(md == "0,1,2,3,4,5,6," + CapDo.KyNhaoLon, "mac dinh 8 o sai");
            Kiem(v == "6,5,4,3,2,1,0," + CapDo.KyNhaoLon, "ban ghi cu khong giu cach xep / o 8 khong phai Nhao lon");
            Kiem(t == "2," + CapDo.KyNhaoLon + ",0,1,3,4,5," + SachPhep.Trong, "Nhao lon nam hai o");
        }
        finally
        {
            if (coT) PlayerPrefs.SetString(kT, luuT); else PlayerPrefs.DeleteKey(kT);
            if (coV) PlayerPrefs.SetString(kV, luuV); else PlayerPrefs.DeleteKey(kV);
            PlayerPrefs.Save();
            SachPhep.NapLai();
        }
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while ((dir == null || GameHUD.Ban == null) && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var hud = GameHUD.Ban;
        if (dir == null || hud == null) { Ghi("[LOI] khong co GameDirector / HUD"); loi++; Ket(); yield break; }
        dir.enabled = false;
        dir.player.GetComponent<Damageable>().maxHealth = 1e7f; dir.player.GetComponent<Damageable>().health = 1e7f;

        DoCamUng();
        DoMayTinh();
        DoSachPhep(hud);
        DoBanGhiCu();

        // ===== E: anh o man that =====
        float sThat = Screen.height / 1080f;
        hud.epCamUng = true; CamUng.EpBat = true;
        yield return new WaitForSeconds(0.5f);
        Vector2 t8 = hud.TamNut(7, sThat);
        Ghi(string.Format("E. man that {0}x{1}: nut 8 tam ({2:F0}, {3:F0}) tu duoi, ban kinh {4:F1}; o 8 cam ung = {5}",
            Screen.width, Screen.height, t8.x, t8.y, hud.BanKinhNut(sThat), SachPhep.Ten(SachPhep.OTron[7])));
        yield return Chup("o8_1_camung");
        CuaSoSachPhep.Mo(); yield return new WaitForSeconds(0.5f);
        yield return Chup("o8_2_sachphep_camung");
        CuaSoSachPhep.Dong();
        hud.epCamUng = false; CamUng.EpBat = false;
        yield return new WaitForSeconds(0.5f);
        yield return Chup("o8_3_maytinh");
        // Chu phim tat "[1/Z]" tren ANH THAT: hang thap nhat co chu vang (1 0,9 0,6) duoi thanh o vuong phai con cach mep duoi
        {
            Texture2D tx = null;
            yield return null; yield return new WaitForEndOfFrame();
            tx = ScreenCapture.CaptureScreenshotAsTexture();
            Rect dau = GameHUD.RectOVuong(0, sThat), cuoi = GameHUD.RectOVuong(SachPhep.SoOVuong - 1, sThat);
            int x0 = Mathf.Max(0, (int)dau.xMin), x1 = Mathf.Min(tx.width - 1, (int)cuoi.xMax);
            int yDayO = (int)(Screen.height - dau.yMax);          // hang texture (tu duoi) cua day o vuong
            // Chu bi KHUNG NHAN cat (le skin day chu xuong) van cach mep man vai diem - phai do CHIEU CAO chu hien ra so voi co chu.
            // Doi chung (anh cu truoc khi sua, 1619x580): 5 hang / co chu 9.
            int thapNhat = -1, caoNhat = -1, soDiem = 0;
            for (int y = 0; y < yDayO; y++)
                for (int x = x0; x <= x1; x++)
                {
                    var p = tx.GetPixel(x, y);
                    if (p.r > 0.7f && p.g > 0.55f && p.b < 0.55f && p.r - p.b > 0.25f)
                    { soDiem++; if (thapNhat < 0) thapNhat = y; caoNhat = y; }
                }
            int coChu = Mathf.Max(8, Mathf.RoundToInt(16f * sThat));
            int caoChu = thapNhat < 0 ? 0 : caoNhat - thapNhat + 1;
            Ghi(string.Format("E2. anh may tinh {0}x{1}: chu phim tat {2} diem vang, cao {3} hang / co chu {4}, hang thap nhat cach mep duoi {5}",
                Screen.width, Screen.height, soDiem, caoChu, coChu, thapNhat));
            Kiem(soDiem > 30 && caoChu >= 0.8f * coChu && thapNhat >= 2, "chu phim tat duoi o vuong bi cat");
            Object.Destroy(tx);
        }
        CuaSoSachPhep.Mo(); yield return new WaitForSeconds(0.5f);
        yield return Chup("o8_4_sachphep_maytinh");
        CuaSoSachPhep.Dong();

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        CamUng.EpBat = false;
        File.WriteAllText("PlayTestShots/o8.txt", bao.ToString());
        var rac = GameObject.Find("TAM_O8");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
