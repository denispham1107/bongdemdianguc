using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 78): CUM 7 NUT KY NANG TRON CUA BAN CAM UNG.
///
/// Nguoi dung 19/09/2026: "cho 7 o ky nang tron o giao dien cam ung to ra them 20%, chu y sap xep lai
/// sao cho cac o khong tran chong len, hoac tran qua ben phai man hinh va tran xuong phia duoi man hinh".
///
/// Cach lam: nhan TAT CA kich thuoc cho 1,2 - ke ca LE tu goc man hinh va ban kinh hai cung - chu khong
/// chi nut. Giu nguyen goc. Nhu vay moi khoang cach len theo dung mot ti le, ti le ho giu y nguyen.
///
/// Do bang SO (khong can vao Play - toan bo bo cuc la ham thuan GameHUD.LechNut):
///   A. TO DUNG 20%: ban kinh nut, be ngang va chieu cao ca cum deu x1,20 so voi ban truoc.
///   B. KHONG CHONG NHAU: 21 cap nut, khoang cach tam phai lon hon duong kinh nut. In cho hep nhat.
///      DOI CHUNG: tinh lai cum voi "chi nut to len, cung giu nguyen" -> phai CHONG NHAU, de biet
///      phep do nay thuc su bat duoc loi chu khong phai luc nao cung xanh.
///   C. KHONG TRAN MEP PHAI / MEP DUOI o moi do phan giai that (dung chinh cong thuc cua HUD).
///   D. KHONG DE LEN JOYSTICK (goc trai duoi), nut SACH PHEP va nut CON MAT (goc phai tren).
///   E. Anh chup that trong Play voi HUD cam ung.
///
/// Ket qua: PlayTestShots/cumnut.txt, anh PlayTestShots/cumnut_*.png.
/// </summary>
public static class ThuCumNutCamUng
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    /// <summary>Ban kinh nut TRUOC khi nguoi dung xin to them 20% - moc de kiem "to dung 20%".</summary>
    const float RTruoc = 60.72f;
    const float LeTruoc = 101.2f;
    const float HeSoXin = 1.20f;

    /// <summary>Man hinh that de thu: ten, rong, cao, co phai man NGANG khong.</summary>
    static readonly object[][] ManThu =
    {
        new object[] { "may tinh 1920x1080",      1920f, 1080f, true  },
        new object[] { "may tinh 1280x720",       1280f,  720f, true  },
        new object[] { "may tinh 4:3 1024x768",   1024f,  768f, true  },
        new object[] { "dien thoai ngang 844x390", 844f,  390f, true  },
        new object[] { "dien thoai ngang 915x412", 915f,  412f, true  },
        new object[] { "dien thoai ngang 2340x1080", 2340f, 1080f, true },
        new object[] { "may bang ngang 1180x820", 1180f,  820f, true  },
        new object[] { "dien thoai DOC 390x844",   390f,  844f, false },
    };

    [MenuItem("Diablo 2.5D/78. Chay thu CUM NUT KY NANG cam ung (to 20%)", false, 167)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu cum nut",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] cum 7 nut ky nang tron - to them 20% (nguoi dung 19/09/2026)");

        DoNgoaiPlay();

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
        if (GameObject.Find("TAM_CumNut") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_CumNut");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CumNut] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    // ================================================================
    // Phan do THUAN - khong can Play
    // ================================================================
    static void DoNgoaiPlay()
    {
        float r, rong, cao;
        GameHUD.HinhHocCumNut(out r, out rong, out cao);

        // ---- A. TO DUNG 20% ----
        // Ca cum duoc nhan cung mot he so nen be ngang / chieu cao cung phai len dung 20%.
        // Tinh lai kich thuoc cum CU tu chinh cong thuc hien tai, chia nguoc he so.
        float rongCu = 0f, caoCu = 0f;
        for (int i = 0; i < 7; i++)
        {
            Vector2 l = GameHUD.LechNut(i) / HeSoXin;
            rongCu = Mathf.Max(rongCu, l.x + RTruoc);
            caoCu = Mathf.Max(caoCu, l.y + RTruoc);
        }
        Ghi("");
        Ghi(string.Format("A. ban kinh nut {0:F3} (truoc {1:F3} -> x{2:F3}); ca cum {3:F1} x {4:F1} (truoc {5:F1} x {6:F1} -> x{7:F3} va x{8:F3})",
            r, RTruoc, r / RTruoc, rong, cao, rongCu, caoCu, rong / rongCu, cao / caoCu));
        Kiem(Mathf.Abs(r / RTruoc - HeSoXin) < 0.001f, "ban kinh nut khong to dung 20%");
        Kiem(Mathf.Abs(rong / rongCu - HeSoXin) < 0.001f, "be ngang cum khong len dung 20% - cum bi keo meo");
        Kiem(Mathf.Abs(cao / caoCu - HeSoXin) < 0.001f, "chieu cao cum khong len dung 20% - cum bi keo meo");

        // ---- B. KHONG NUT NAO CHONG NUT NAO ----
        float hepNhat; int iA, iB;
        // Ban CU = cum thu nho ve 1/1,2 VA nut ban kinh cu
        float hepCu = ChoHepNhat(1f / HeSoXin, RTruoc, out iA, out iB);
        hepNhat = ChoHepNhat(1f, r, out iA, out iB);
        Ghi("");
        Ghi(string.Format("B. 21 cap nut: cho hep nhat {0:F1} (giua nut {1} va nut {2}), duong kinh nut {3:F1}; ban truoc cho hep nhat {4:F1} / duong kinh {5:F1}",
            hepNhat, iA + 1, iB + 1, r * 2f, hepCu, RTruoc * 2f));
        Kiem(hepNhat > 0f, "co nut CHONG LEN nhau");
        // Ti le ho / duong kinh phai GIU NGUYEN nhu truoc khi to (ca cum nhan cung he so)
        Kiem(Mathf.Abs((hepNhat / (r * 2f)) - (hepCu / (RTruoc * 2f))) < 0.001f,
             "ti le cho ho doi - cum khong duoc nhan deu");

        // DOI CHUNG: neu CHI nut to len ma hai cung giu nguyen thi phai CHONG NHAU.
        // Khong co doi chung nay thi phep do tren luon xanh va khong chung minh duoc gi.
        int cA, cB;
        float hepSai = ChoHepNhat(1f / HeSoXin, r, out cA, out cB);
        Ghi(string.Format("B2. DOI CHUNG (chi nut to, hai cung giu nguyen): cho hep nhat {0:F1} giua nut {1} va nut {2} -> {3}",
            hepSai, cA + 1, cB + 1, hepSai <= 0f ? "CHONG NHAU dung nhu mong doi" : "van con ho (phep do B khong bat duoc loi!)"));
        Kiem(hepSai <= 0f, "doi chung hong: nut to len ma cung giu nguyen van khong chong nhau - phep do B vo nghia");

        // ---- C. KHONG TRAN MEP PHAI / MEP DUOI ----
        Ghi("");
        Ghi("C. tran mep o tung do phan giai (cum do tu GOC PHAI DUOI; + la con cach mep, - la TRAN ra ngoai)");
        foreach (var m in ManThu)
        {
            string ten = (string)m[0];
            float W = (float)m[1], H = (float)m[2];
            bool ngang = (bool)m[3];
            float s = GameHUD.TiLeMan(H);

            float duPhai = float.MaxValue, duDuoi = float.MaxValue, duTrai = float.MaxValue, duTren = float.MaxValue;
            for (int i = 0; i < 7; i++)
            {
                Vector2 t = TamNut(i, W, s);
                float rr = r * s;
                duPhai = Mathf.Min(duPhai, W - (t.x + rr));
                duTrai = Mathf.Min(duTrai, t.x - rr);
                duDuoi = Mathf.Min(duDuoi, t.y - rr);
                duTren = Mathf.Min(duTren, H - (t.y + rr));
            }
            Ghi(string.Format("    {0}: phai {1:F1} | duoi {2:F1} | trai {3:F1} | tren {4:F1}  (ban kinh nut {5:F1} px)",
                ten, duPhai, duDuoi, duTrai, duTren, r * s));
            Kiem(duPhai >= 0f, ten + ": nut TRAN qua mep phai " + (-duPhai).ToString("F1") + " px");
            Kiem(duDuoi >= 0f, ten + ": nut TRAN xuong duoi mep " + (-duDuoi).ToString("F1") + " px");
            if (ngang)
            {
                Kiem(duTrai >= 0f, ten + ": nut tran qua mep TRAI " + (-duTrai).ToString("F1") + " px");
                Kiem(duTren >= 0f, ten + ": nut tran qua mep TREN " + (-duTren).ToString("F1") + " px");
            }
            else
            {
                // Man DOC khong phai the choi cua ban cam ung (can dieu khien o goc trai duoi, cum nut o
                // goc phai duoi - hai cai chi vua nhau khi man nam ngang), nen chi GHI LAI chu khong tinh
                // la loi. Ghi kem muc tran cua BAN CU de biet day khong phai thu moi sinh ra.
                float traiCu = float.MaxValue;
                for (int i = 0; i < 7; i++)
                {
                    Vector2 t = TamNut(i, W, s / HeSoXin);
                    traiCu = Mathf.Min(traiCu, t.x - RTruoc * s);
                }
                Ghi(string.Format("       (man doc khong tinh la loi - chi ghi lai: mep trai ban NAY {0:F1}, ban TRUOC khi to {1:F1})",
                    duTrai, traiCu));
            }
        }

        // ---- D. KHONG DE LEN JOYSTICK ----
        Ghi("");
        Ghi("D. khoang cach toi can joystick (goc trai duoi)");
        foreach (var m in ManThu)
        {
            string ten = (string)m[0];
            float W = (float)m[1], H = (float)m[2];
            if (!(bool)m[3]) continue;                       // man dung: do o muc C roi
            float s = GameHUD.TiLeMan(H);
            Vector2 tamJ = GameHUD.TamJoystick(s);
            float bkJ = GameHUD.BanKinhJoystick(s);
            float ho = float.MaxValue;
            for (int i = 0; i < 7; i++)
                ho = Mathf.Min(ho, Vector2.Distance(TamNut(i, W, s), tamJ) - bkJ - r * s);
            Ghi(string.Format("    {0}: cho ho gan nhat {1:F1} px (can joystick ban kinh {2:F1})", ten, ho, bkJ));
            Kiem(ho > 0f, ten + ": cum nut DE LEN can joystick");
        }
    }

    /// <summary>Tam nut thu i tren man rong W, theo dung cach GameHUD dat (tu goc PHAI DUOI, y dem tu duoi len).</summary>
    static Vector2 TamNut(int i, float W, float s)
    {
        Vector2 lech = GameHUD.LechNut(i);
        return new Vector2(W - lech.x * s, lech.y * s);
    }

    /// <summary>
    /// Cho ho HEP NHAT giua hai nut bat ky = khoang cach tam nho nhat tru duong kinh nut.
    /// <paramref name="coCum"/> thu nho cum lai (de dung lam doi chung "chi nut to, cum giu nguyen").
    /// </summary>
    static float ChoHepNhat(float coCum, float banKinhNut, out int a, out int b)
    {
        float hep = float.MaxValue; a = -1; b = -1;
        for (int i = 0; i < 7; i++)
            for (int j = i + 1; j < 7; j++)
            {
                float d = Vector2.Distance(GameHUD.LechNut(i) * coCum, GameHUD.LechNut(j) * coCum) - banKinhNut * 2f;
                if (d < hep) { hep = d; a = i; b = j; }
            }
        return hep;
    }

    // ================================================================
    // Phan chup anh trong Play
    // ================================================================
    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);

        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);

        // ⚠️ Phai dat hud.epCamUng chu khong phai CamUng.EpBat: GameHUD.Update GHI DE
        // CamUng.EpBat = epCamUng moi khung hinh, nen dat cai kia thi anh chup ra ban MAY TINH
        // (lan chup dau 19/09/2026 ra thanh o vuong).
        var hud = Object.FindAnyObjectByType<GameHUD>();
        bool epCu = hud != null && hud.epCamUng;
        if (hud != null) hud.epCamUng = true;
        else { Ghi("[LOI] khong tim thay GameHUD de ep ban cam ung"); loi++; }
        CapDo.BatDauTranMoi();
        for (int ky = 0; ky < CapDo.SoKyNang; ky++) CapDo.MoCaDuongChoPhepThu(ky);
        yield return new WaitForSeconds(0.6f);

        Ghi("");
        Ghi(string.Format("E. anh chup HUD cam ung o man {0}x{1}", Screen.width, Screen.height));
        yield return Chup("cumnut_1_hud_camung");

        // Kiem THAT su dang ve ban cam ung truoc khi tin vao anh chup
        Ghi("E2. dang ve ban cam ung: CamUng.DangDung = " + CamUng.DangDung);
        Kiem(CamUng.DangDung, "anh chup khong phai ban cam ung - cum nut tron khong co trong anh");

        if (hud != null) hud.epCamUng = epCu;
        if (dir != null) dir.enabled = true;

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[CumNut] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/cumnut.txt", bao.ToString());
        var rac = GameObject.Find("TAM_CumNut");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
