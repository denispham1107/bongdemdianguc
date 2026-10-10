using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MENU 121 - NHAC NEN + TAB "Âm thanh" (10/10/2026, nguoi dung).
///
/// A. NGOAI PLAY: hai clip trong Resources trung tung byte voi ban goc o CongCu/AmThanh/NhacThu (md5), Unity doc ra STEREO 2 kenh
///    44,1 kHz, 128 / 192 s, KHONG Force To Mono (quy tac nguoi dung), kieu nap / nen.
/// B. MAN CHINH (dang nhap, sanh): co dung MOT vat the NhacNen, nhac sanh dang keu, am luong mac dinh 60% (khoa trong kho bi xoa
///    tam = nguoi choi moi; mac dinh KHONG duoc ghi xuong kho). Do TIENG THAT o loa (AudioListener.GetOutputData, doc lap voi code
///    nhac): co tieng ca hai kenh, hai kenh KHAC nhau (tuong quan &lt; 0,99 = stereo that, ban mono ra 1,00).
/// C. TAB "Âm thanh" (dang nhap tai khoan thu, vao sanh, mo CAI DAT, chon tab): anh chup; do tren ANH phan ray do da keo ~60% ray.
///    Thanh keo bang su kien chuot GIA (Event.current tu dung): bam 25% -> 0,25, keo 80% -> 0,80, keo qua phai -> 1, tha nhan,
///    bam ngoai o khong doi. Keo ve 30% -> tieng that nho ~x0,5 (doi qua lai 30 / 60 tung khung, so trung binh) ; HỦY -> 60%,
///    kho khong ghi; 45% + OK -> kho ghi 45, doc lai 45.
/// D. VAO TRAN (BatDauRoiSanh + LoadScene Act2, nhu ManSanh): theo tung khung - nhac sanh khong bao gio to len, ve 0 roi DUNG +
///    tha clip; nhac Act2 to dan len 45% x 0,7 = 31,5%; van MOT vat the NhacNen; tieng that trong tran co + stereo.
/// E. VE SANH (GameDirector.BackToMenu): nhac Act2 ve 0 roi dung; nhac sanh phat lai TU DAU (time &lt; 1 s luc nap xong) to len 45%.
///
/// Ket qua: PlayTestShots/nhacnen.txt, anh nhacnen_*.png. Tra lai: am luong da luu, phien dang nhap, scene, tat tieng Editor.
/// </summary>
public static class ThuNhacNen
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool coAmLuongGoc; static int amLuongGoc;
    static bool coPhienGoc; static string phienGoc;
    static bool truocBatPlayMode; static EnterPlayModeOptions truocPlayMode;
    static bool tatTiengGoc;

    const string KhoaPhien = "diablo25d_refresh";

    [MenuItem("Diablo 2.5D/121. Chay thu NHAC NEN (sanh -> tran, tab Am thanh)", false, 210)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorApplication.isPlaying) { Debug.LogWarning("[NhacNen] dang Play - thoat Play roi chay lai"); return; }

        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] nhac nen + tab Am thanh");

        KiemTep();

        canhCu = EditorSceneManager.GetActiveScene().path;
        coAmLuongGoc = PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac);
        amLuongGoc = PlayerPrefs.GetInt(CaiDatAmThanh.KhoaNhac, 0);
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaNhac);      // nguoi choi moi: chua tung chinh
        PlayerPrefs.DeleteKey(KhoaPhien);                    // khong de man dang nhap tu dang nhap chen ngang
        PlayerPrefs.Save();

        // Tieng that phai ra loa thi GetOutputData moi co so - Editor dang tat tieng (nut Mute Audio) thi mo tam
        tatTiengGoc = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = false;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        NhacNen.BatTrongPhepThu = true;
        var go = new GameObject("TAM_ThuNhacNen");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[NhacNen] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    // =============================================================
    // A. NGOAI PLAY
    // =============================================================
    static void KiemTep()
    {
        string[] trongGame = { "Assets/Resources/" + NhacNen.DuongNhacSanh + ".wav", "Assets/Resources/" + NhacNen.DuongNhacTran + ".wav" };
        string[] goc = { "CongCu/AmThanh/NhacThu/sanh_thu1.wav", "CongCu/AmThanh/NhacThu/tran_act2_thu3.wav" };
        float[] doDai = { 128f, 192f };
        for (int i = 0; i < 2; i++)
        {
            bool trung = File.Exists(goc[i]) && Md5(goc[i]) == Md5(trongGame[i]);
            var imp = AssetImporter.GetAtPath(trongGame[i]) as AudioImporter;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(trongGame[i]);
            if (imp == null || clip == null) { Ghi("[LOI] khong nap duoc " + trongGame[i]); loi++; continue; }
            var mac = imp.defaultSampleSettings;
            var web = imp.GetOverrideSampleSettings(BuildTargetGroup.WebGL);
            Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh, {3} Hz, {4:F2} s; ForceToMono {5}; nap {6}, nen {7} chat luong {8:F2}, nen trong nen {9}; WebGL nap {10}",
                Path.GetFileName(trongGame[i]), trung, clip.channels, clip.frequency, clip.length, imp.forceToMono,
                mac.loadType, mac.compressionFormat, mac.quality, imp.loadInBackground, web.loadType));
            Kiem(trung, "file trong game khac ban goc " + goc[i]);
            Kiem(clip.channels == 2 && !imp.forceToMono, "nhac khong STEREO (quy tac nguoi dung)");
            Kiem(clip.frequency == 44100 && Mathf.Abs(clip.length - doDai[i]) < 0.05f, "sai tan so / do dai");
        }
    }

    static string Md5(string duong)
    {
        using (var m = MD5.Create())
        using (var f = File.OpenRead(duong))
            return System.BitConverter.ToString(m.ComputeHash(f));
    }

    // =============================================================
    // DO TIENG THAT
    // =============================================================
    static readonly float[] bufL = new float[2048], bufR = new float[2048];

    /// <summary>Do tieng ra loa trong n khung: RMS trai / phai, tuong quan hai kenh.</summary>
    static IEnumerator DoTieng(int n, System.Action<float, float, float> nhan)
    {
        double sl = 0, sr = 0, slr = 0; int dem = 0;
        for (int k = 0; k < n; k++)
        {
            yield return null;
            AudioListener.GetOutputData(bufL, 0);
            AudioListener.GetOutputData(bufR, 1);
            for (int i = 0; i < bufL.Length; i++) { sl += bufL[i] * bufL[i]; sr += bufR[i] * bufR[i]; slr += bufL[i] * bufR[i]; }
            dem += bufL.Length;
        }
        float rl = (float)System.Math.Sqrt(sl / dem), rr = (float)System.Math.Sqrt(sr / dem);
        float tq = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        nhan(rl, rr, tq);
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 60 && !File.Exists(duong); i++)
            yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.2f);
    }

    static int DemNhacNen() { return Object.FindObjectsByType<NhacNen>(FindObjectsInactive.Include).Length; }

    // =============================================================
    // B - E. TRONG PLAY
    // =============================================================
    static IEnumerator ChayKichBan()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        var nn = NhacNen.Ban;
        Ghi("");
        if (nn == null) { Ghi("[LOI] khong co NhacNen"); loi++; Ket(); yield break; }

        // ---- B. man dang nhap ----
        string clipSanh = nn.sanh.nguon.clip != null ? nn.sanh.nguon.clip.name : "(khong)";
        Ghi(string.Format("B1. man dang nhap: {0} vat the NhacNen; nhac sanh {1} keu {2}, am luong {3:F3} (mac dinh 60% -> 0,600); nhac tran keu {4}; kho co khoa am luong {5}",
            DemNhacNen(), clipSanh, nn.sanh.LaDangKeu, nn.sanh.nguon.volume, nn.tran.LaDangKeu, PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac)));
        Kiem(DemNhacNen() == 1, "khong phai dung mot vat the NhacNen");
        Kiem(nn.sanh.LaDangKeu && clipSanh == "NhacSanh" && !nn.tran.LaDangKeu, "man dang nhap khong phat nhac sanh");
        Kiem(Mathf.Abs(nn.sanh.nguon.volume - 0.6f) < 0.005f, "am luong mac dinh khong phai 60%");
        Kiem(!PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac), "muc mac dinh bi ghi xuong kho");
        Ghi("    so lan nhac phai tu phuc hoi: " + nn.SoLanPhucHoi + " (phai 0 - phat dung ngay tu dau, khong nho phuc hoi) " + nn.LyDoPhucHoi);
        Kiem(nn.SoLanPhucHoi == 0, "nhac sanh chi keu nho tu phuc hoi");

        float rl = 0, rr = 0, tq = 1;
        yield return DoTieng(40, (a, b, c) => { rl = a; rr = b; tq = c; });
        Ghi(string.Format("B2. tieng that o loa (40 khung): RMS trai {0:F4} phai {1:F4}, tuong quan hai kenh {2:F3} (mono = 1,000)", rl, rr, tq));
        Kiem(rl > 0.003f && rr > 0.003f, "khong nghe thay nhac (loa im)");
        Kiem(tq < 0.99f, "hai kenh giong het nhau - nhac khong stereo");

        // Dang nhap that, vao sanh (cung scene MainMenu) - nhac phai CHAY LIEN, khong phat lai
        float tTruoc = nn.sanh.nguon.time;
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] dang nhap: " + e); loi++; Ket(); yield break; }
        var dn = Object.FindAnyObjectByType<ManDangNhap>();
        if (dn != null && dn.daVao != null) dn.daVao();
        yield return new WaitForSecondsRealtime(1.5f);
        float tSau = nn.sanh.nguon.time;
        Ghi(string.Format("B3. dang nhap -> sanh: nhac sanh tu giay {0:F2} toi {1:F2} (chay lien, khong ve 0), keu {2}, am luong {3:F3}",
            tTruoc, tSau, nn.sanh.LaDangKeu, nn.sanh.nguon.volume));
        Kiem(nn.sanh.LaDangKeu && tSau > tTruoc + 0.5f, "vao sanh ma nhac sanh bi ngat / phat lai tu dau");

        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh == null) { Ghi("[LOI] khong co sanh"); loi++; Ket(); yield break; }

        // ---- C. tab Am thanh ----
        sanh.MoCaiDat();
        sanh.ChonTabCaiDat(1);
        yield return new WaitForSecondsRealtime(0.4f);
        yield return Chup("nhacnen_1_tab_amthanh");
        DoAnhThanh("nhacnen_1_tab_amthanh", 0.60f);
        sanh.BamHuyCaiDat();        // dong bang that trong luc bom su kien gia (khong de hai thanh keo cung giu chuot)

        // Thanh keo voi su kien chuot gia - MOI su kien mot luot OnGUI rieng (so hieu dieu khien IMGUI danh lai tu dau moi luot;
        // don ca chuoi vao mot luot thi buoc keo khong khop buoc bam)
        var tc = Object.FindAnyObjectByType<ChayThuMang>();
        var kq = new List<string>();
        var r = new Rect(100, 100, 436, 84);           // le 18 -> ray 400 tu x = 118
        float v = 0.5f; int buoc = 0; bool giu = false, tha = false;
        EventType[] kieu = { EventType.MouseDown, EventType.MouseDrag, EventType.MouseDrag, EventType.MouseUp, EventType.MouseDown };
        float[] xs = { 118 + 100, 118 + 320, 900, 900, 50 };
        tc.veGUI = () =>
        {
            if (buoc >= kieu.Length || Event.current.type != EventType.Repaint) return;
            var goc = Event.current;
            if (buoc == 4) v = 0.7f;
            Event.current = new Event { type = kieu[buoc], mousePosition = new Vector2(xs[buoc], buoc == 4 ? 50f : r.center.y), button = 0 };
            v = GiaoDien.ThanhKeo(r, v, 1f);
            if (buoc == 2) giu = GUIUtility.hotControl != 0;
            if (buoc == 3) tha = GUIUtility.hotControl == 0;
            if (buoc != 3) kq.Add(v.ToString("F3"));
            Event.current = goc;
            buoc++;
        };
        for (int i = 0; i < 60 && buoc < kieu.Length; i++) yield return null;
        tc.veGUI = null;
        GUIUtility.hotControl = 0;
        kq.Add(giu + "/" + tha);
        Ghi("C2. thanh keo (su kien gia): bam 25% -> " + (kq.Count > 0 ? kq[0] : "?") + ", keo 80% -> " + (kq.Count > 1 ? kq[1] : "?")
            + ", keo qua phai -> " + (kq.Count > 2 ? kq[2] : "?") + ", bam ngoai o (dang 0,700) -> " + (kq.Count > 3 ? kq[3] : "?")
            + ", giu / tha nhan " + (kq.Count > 4 ? kq[4] : "?"));
        Kiem(kq.Count == 5 && kq[0] == "0.250" && kq[1] == "0.800" && kq[2] == "1.000" && kq[3] == "0.700" && kq[4] == "True/True",
             "thanh keo khong theo con tro");

        sanh.MoCaiDat(); sanh.ChonTabCaiDat(1);
        yield return null;
        // Keo ve 30%: tieng that nho ~ mot nua. Moi lan doc loa lay 2048 mau (~46 ms, 3 khung) - doi muc TUNG KHOI 0,4 s, bo 0,2 s
        // dau moi khoi cho am luong on dinh; 20 khoi xen ke (8 s) de san bang cho to / nho cua ban nhac
        double a60 = 0, a30 = 0; int n60 = 0, n30 = 0;
        for (int khoi = 0; khoi < 20; khoi++)
        {
            bool nua = khoi % 2 == 1;
            sanh.DatAmLuongNhac(nua ? 30 : 60);
            float tKhoi = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - tKhoi < 0.2f) yield return null;
            while (Time.realtimeSinceStartup - tKhoi < 0.4f)
            {
                yield return null;
                AudioListener.GetOutputData(bufL, 0);
                double sum = 0; for (int i = 0; i < bufL.Length; i++) sum += bufL[i] * bufL[i];
                if (nua) { a30 += sum; n30++; } else { a60 += sum; n60++; }
            }
        }
        float tiLe = (float)System.Math.Sqrt((a30 / n30) / (a60 / n60));
        sanh.DatAmLuongNhac(30);
        yield return null; yield return null;
        float am30 = nn.sanh.nguon.volume;
        Ghi(string.Format("C3. keo ve 30%: nguon {0:F3}; tieng that 30% / 60% = x{1:F2} (mong x0,50); kho chua ghi {2}",
            am30, tiLe, !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac)));
        Kiem(Mathf.Abs(am30 - 0.3f) < 0.005f, "keo thanh khong doi am luong nghe thu");
        Kiem(tiLe > 0.38f && tiLe < 0.62f, "tieng that khong nho theo thanh keo");
        Kiem(!PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac), "chua bam OK ma da ghi xuong kho");
        yield return Chup("nhacnen_2_keo_30");
        DoAnhThanh("nhacnen_2_keo_30", 0.30f);

        sanh.BamHuyCaiDat();
        yield return null; yield return null;
        Ghi(string.Format("C4. HUY: bang dong {0}, am luong ve {1:F3}, kho co khoa {2}", !sanh.DangMoCaiDat, nn.sanh.nguon.volume, PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac)));
        Kiem(!sanh.DangMoCaiDat && Mathf.Abs(nn.sanh.nguon.volume - 0.6f) < 0.005f && !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac), "HUY khong tra ve muc cu");

        sanh.MoCaiDat(); sanh.ChonTabCaiDat(1);
        sanh.DatAmLuongNhac(45);
        sanh.BamOKCaiDat();
        yield return null; yield return null;
        CaiDatAmThanh.QuenBoNho();
        Ghi(string.Format("C5. 45% + OK: bang dong {0}, kho ghi {1}, doc lai {2}, am luong {3:F3}, khong tai lai game (sanh con {4})",
            !sanh.DangMoCaiDat, PlayerPrefs.GetInt(CaiDatAmThanh.KhoaNhac, -1), CaiDatAmThanh.NhacPhanTram, nn.sanh.nguon.volume, sanh != null));
        Kiem(!sanh.DangMoCaiDat && PlayerPrefs.GetInt(CaiDatAmThanh.KhoaNhac, -1) == 45 && CaiDatAmThanh.NhacPhanTram == 45
             && Mathf.Abs(nn.sanh.nguon.volume - 0.45f) < 0.005f, "OK khong luu am luong");

        // ---- D. vao tran ----
        FirebaseMang.Quen();
        var mauSanh = new List<float>(); var mauTran = new List<float>(); var mauT = new List<float>();
        float t0 = Time.realtimeSinceStartup;
        NhacNen.BatDauRoiSanh();
        yield return null;
        float sanhSauGoi = nn.sanh.nguon.volume;
        SceneManager.LoadScene(NhacNen.TenCanhTran);
        float tNap = -1f, tSanhTat = -1f;
        bool sanhTangLen = false, tranGiam = false; float truocS = sanhSauGoi, truocT = 0f;
        for (int k = 0; k < 600; k++)
        {
            yield return null;
            if (tNap < 0f && SceneManager.GetActiveScene().name == NhacNen.TenCanhTran) tNap = Time.realtimeSinceStartup - t0;
            float vs = nn.sanh.nguon.volume, vt = nn.tran.nguon.volume;
            if (vs > truocS + 1e-4f) sanhTangLen = true;
            if (vt < truocT - 1e-4f) tranGiam = true;
            truocS = vs; truocT = vt;
            if (tSanhTat < 0f && !nn.sanh.LaDangKeu) tSanhTat = Time.realtimeSinceStartup - t0;
            mauT.Add(Time.realtimeSinceStartup - t0); mauSanh.Add(vs); mauTran.Add(vt);
            if (tNap > 0f && Time.realtimeSinceStartup - t0 > tNap + NhacNen.GiayToDan + 0.6f) break;
        }
        float dichTran = 0.45f * NhacNen.HeSoTrongTran;
        Ghi(string.Format("D1. vao tran: goi roi sanh -> am luong sanh {0:F3} (khung sau); nap Act2 xong luc {1:F2} s; nhac sanh tat han luc {2:F2} s, tha clip {3}, co luc to len {4}",
            sanhSauGoi, tNap, tSanhTat, nn.sanh.nguon.clip == null, sanhTangLen));
        Ghi(string.Format("D2. nhac Act2: clip {0}, keu {1}, am luong cuoi {2:F3} (mong 0,45 x 0,7 = {3:F3}), co luc nho di {4}; {5} vat the NhacNen",
            nn.tran.nguon.clip != null ? nn.tran.nguon.clip.name : "(khong)", nn.tran.LaDangKeu, nn.tran.nguon.volume, dichTran, tranGiam, DemNhacNen()));
        Ghi("    mau (giay: sanh / tran): " + MauRutGon(mauT, mauSanh, mauTran));
        Kiem(sanhSauGoi < 0.45f - 1e-4f || tNap < 0.05f, "BatDauRoiSanh khong lam nhac sanh nho di");
        Kiem(!sanhTangLen && tSanhTat > 0f && nn.sanh.nguon.clip == null, "nhac sanh khong nho dan ve 0 / khong dung");
        Kiem(nn.tran.LaDangKeu && nn.tran.nguon.clip != null && nn.tran.nguon.clip.name == "NhacTranAct2", "trong tran khong phat nhac Act2");
        Kiem(!tranGiam && Mathf.Abs(nn.tran.nguon.volume - dichTran) < 0.005f, "nhac Act2 khong to dan len 70% muc da chinh");
        Kiem(DemNhacNen() == 1, "qua man sinh them vat the NhacNen");

        yield return DoTieng(40, (a, b, c) => { rl = a; rr = b; tq = c; });
        Ghi(string.Format("D3. tieng that trong tran: RMS trai {0:F4} phai {1:F4}, tuong quan {2:F3}", rl, rr, tq));
        Kiem(rl > 0.002f && rr > 0.002f && tq < 0.99f, "trong tran khong nghe nhac stereo");

        // ---- E. ve sanh ----
        GameDirector.BackToMenu();
        float t1 = Time.realtimeSinceStartup, tMenu = -1f, timeLucNap = -1f, tTranTat = -1f;
        for (int k = 0; k < 600; k++)
        {
            yield return null;
            if (tMenu < 0f && SceneManager.GetActiveScene().name == NhacNen.TenCanhSanh) { tMenu = Time.realtimeSinceStartup - t1; timeLucNap = nn.sanh.nguon.time; }
            if (tTranTat < 0f && !nn.tran.LaDangKeu) tTranTat = Time.realtimeSinceStartup - t1;
            if (tMenu > 0f && Time.realtimeSinceStartup - t1 > tMenu + NhacNen.GiayNhoDan + 0.6f) break;
        }
        Ghi(string.Format("E. ve sanh: nap xong {0:F2} s; nhac sanh phat lai tu giay {1:F2}, keu {2}, am luong {3:F3} (mong 0,450); nhac Act2 tat luc {4:F2} s, tha clip {5}",
            tMenu, timeLucNap, nn.sanh.LaDangKeu, nn.sanh.nguon.volume, tTranTat, nn.tran.nguon.clip == null));
        Kiem(nn.sanh.LaDangKeu && timeLucNap >= 0f && timeLucNap < 1f && Mathf.Abs(nn.sanh.nguon.volume - 0.45f) < 0.005f, "ve sanh khong phat lai nhac sanh tu dau");
        Kiem(tTranTat > 0f && nn.tran.nguon.clip == null, "ve sanh ma nhac Act2 khong dung");
        Ghi("    so lan tu phuc hoi ca phep thu: " + nn.SoLanPhucHoi);
        Kiem(nn.SoLanPhucHoi == 0, "co luc nhac phai tu phuc hoi");

        Ket();
    }

    /// <summary>Do tren ANH: phan ray mau do (da keo) chiem bao nhieu phan ray - doc lap voi code ve.</summary>
    static void DoAnhThanh(string ten, float mong)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (!File.Exists(duong)) { Ghi("[LOI] khong co anh " + ten); loi++; return; }
        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(duong));
        float s = Screen.height / 1080f;
        // Bo cuc bang cai dat chep tu ManSanh.VeCaiDat
        float rong = Mathf.Min(Screen.width - 30f * s, 880f * s);
        float cao = Mathf.Min(Screen.height - 30f * s, 700f * s);
        float x = (Screen.width - rong) * 0.5f, y = (Screen.height - cao) * 0.5f, le = 34f * s;
        var hang = ManSanh.HangNhacNen(x + le, y + 100f * s + 70f * s, rong - 2f * le, s);
        var thanh = ManSanh.ThanhNhacNen(hang, s);
        float leR = Mathf.Max(10f, 18f * s);
        float trai = thanh.x + leR, phai = thanh.xMax - leR;
        float ti = tex.width / (float)Screen.width;
        int yAnh = tex.height - 1 - Mathf.RoundToInt(thanh.center.y * ti);
        // Quet ngang dong giua ray (rong ra hai ben): ray da keo = do sam (MauMau, R ~0,72), vien nut thoi = do tuoi (R > 0,85).
        // Tam nut = trung binh x cac diem do tuoi; ray da keo = cac diem do sam ben trai tam nut.
        int doSam = 0, doTuoi = 0; double tongX = 0;
        int x0 = Mathf.RoundToInt((trai - 40f * s) * ti), x1 = Mathf.RoundToInt((phai + 40f * s) * ti);
        for (int px = Mathf.Max(0, x0); px <= Mathf.Min(tex.width - 1, x1); px++)
        {
            var c = tex.GetPixel(px, yAnh);
            if (c.r > 0.85f && c.g < 0.32f && c.b < 0.25f) { doTuoi++; tongX += px; }
            else if (c.r > 0.5f && c.r <= 0.85f && c.g < 0.15f && c.b < 0.15f) doSam++;
        }
        float phan = doTuoi > 0 ? ((float)(tongX / doTuoi) / ti - trai) / (phai - trai) : -1f;
        Ghi(string.Format("C1. anh {0}: ray {1:F0}-{2:F0} (diem anh), tam nut thoi o {3:F3} chieu dai ray (mong {4:F2}), {5} diem do sam (phan da keo), {6} diem vien nut",
            ten, trai, phai, phan, mong, doSam, doTuoi));
        Kiem(Mathf.Abs(phan - mong) < 0.03f && doSam > 10, "anh thanh keo khong khop am luong");
        Object.DestroyImmediate(tex);
    }

    static string MauRutGon(List<float> t, List<float> a, List<float> b)
    {
        var sb = new StringBuilder();
        float moc = 0f;
        for (int i = 0; i < t.Count; i++)
            if (t[i] >= moc) { sb.AppendFormat("{0:F1}: {1:F2} / {2:F2}  ", t[i], a[i], b[i]); moc = t[i] + 0.5f; }
        return sb.ToString();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/nhacnen.txt", bao.ToString());
        FirebaseMang.Quen();
        NhacNen.BatTrongPhepThu = false;
        var rac = GameObject.Find("TAM_ThuNhacNen");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        if (coAmLuongGoc) PlayerPrefs.SetInt(CaiDatAmThanh.KhoaNhac, amLuongGoc); else PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaNhac);
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc); else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
        EditorUtility.audioMasterMute = tatTiengGoc;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);
        Debug.Log("[NhacNen] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
