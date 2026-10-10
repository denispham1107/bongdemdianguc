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
/// MENU 122 - TIENG BAM NUT + THANH "Hiệu ứng" (11/10/2026, nguoi dung).
///
/// A. NGOAI PLAY: hai tieng bam (mau A "Đá mộ") trung tung byte ban goc CongCu/AmThanh/TiengBamThu, STEREO 2 kenh, khong Force To
///    Mono; doc MA NGUON: cac nut chinh (VÀO GAME / TẠO TÀI KHOẢN, TẠO PHÒNG, SẴN SÀNG, OK, MỞ KHÓA / NÂNG CẤP) goi tieng xac nhan.
/// B. MAN CHINH: mac dinh 80% (khoa kho bi xoa tam, khong ghi mac dinh). GiaoDien.Nut / Tab voi su kien chuot GIA (moi su kien mot
///    luot OnGUI): bam -> 1 tieng thuong; Nut(xacNhan) -> 1 tieng xac nhan; bam ngoai nut -> 0. Cung khung hai lan -> 1 tieng;
///    thuong + xac nhan cung khung -> chi xac nhan. TIENG THAT o loa (nhac nen tat tieng tam): co tieng hai kenh, tuong quan
///    &lt; 0,99 (stereo); 40% / 80% -> dinh ~x0,5.
/// C. TAB "Âm thanh" (dang nhap tai khoan thu): anh chup - nut thoi hang Hieu ung o 0,80; keo 40% -> nghe thu (1 tieng bam, keo
///    tiep trong 0,15 s khong keu them), muc 0,40; HỦY -> 0,80 khong ghi; 40% + OK -> kho 40, nhac nen khong doi.
/// D. TRONG TRAN (Act2): dau "+" mo khoa Qua cau lua -> tieng xac nhan; bam "+" khi khong mo / nang duoc -> tieng thuong.
///
/// Ket qua: PlayTestShots/tiengbam.txt, anh tiengbam_*.png.
/// </summary>
public static class ThuTiengBam
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool coHUGoc, coNhacGoc, coPhienGoc; static int huGoc, nhacGoc; static string phienGoc;
    static bool truocBatPlayMode; static EnterPlayModeOptions truocPlayMode;
    static bool tatTiengGoc;
    const string KhoaPhien = "diablo25d_refresh";

    [MenuItem("Diablo 2.5D/122. Chay thu TIENG BAM NUT (thanh Hieu ung)", false, 211)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengBam] dang Play - thoat Play roi chay lai"); return; }
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng bam nut + thanh Hieu ung");
        KiemTep();
        KiemMaNguon();

        canhCu = EditorSceneManager.GetActiveScene().path;
        coHUGoc = PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng); huGoc = PlayerPrefs.GetInt(CaiDatAmThanh.KhoaHieuUng, 0);
        coNhacGoc = PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac); nhacGoc = PlayerPrefs.GetInt(CaiDatAmThanh.KhoaNhac, 0);
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien); phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaHieuUng);
        PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaNhac);
        PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
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
        var go = new GameObject("TAM_ThuTiengBam");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengBam] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    // ---------------- A ----------------
    static void KiemTep()
    {
        string[] game = { "Assets/Resources/" + TiengDong.DuongBamThuong + ".wav", "Assets/Resources/" + TiengDong.DuongBamXacNhan + ".wav" };
        string[] goc = { "CongCu/AmThanh/TiengBamThu/A_da_mo_thuong.wav", "CongCu/AmThanh/TiengBamThu/A_da_mo_xacnhan.wav" };
        for (int i = 0; i < 2; i++)
        {
            var imp = AssetImporter.GetAtPath(game[i]) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game[i]);
            if (imp == null || c == null) { Ghi("[LOI] khong nap duoc " + game[i]); loi++; continue; }
            bool trung = File.Exists(goc[i]) && Md5(goc[i]) == Md5(game[i]);
            Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh {3} Hz {4:F2} s; ForceToMono {5}; nap {6}",
                Path.GetFileName(game[i]), trung, c.channels, c.frequency, c.length, imp.forceToMono, imp.defaultSampleSettings.loadType));
            Kiem(trung && c.channels == 2 && !imp.forceToMono, "tieng bam khong dung ban goc / khong STEREO");
        }
    }

    static string Md5(string d) { using (var m = MD5.Create()) using (var f = File.OpenRead(d)) return System.BitConverter.ToString(m.ComputeHash(f)); }

    static void KiemMaNguon()
    {
        // (file, doan phai co) - nut chinh goi tieng xac nhan
        string[,] can = {
            { "Assets/Scripts/UI/ManDangNhap.cs", "chuNut, GiaoDien.KieuNutMau, !dangCho)" },
            { "Assets/Scripts/UI/ManSanh.cs", "\"TẠO PHÒNG\", GiaoDien.KieuNutMau, true)" },
            { "Assets/Scripts/UI/ManSanh.cs", "GiaoDien.KieuNutMau, !dangSanSang))" },
            { "Assets/Scripts/UI/ManSanh.cs", "\"OK\", GiaoDien.KieuNutMau, true)" },
            { "Assets/Scripts/UI/CuaSoSachPhep.cs", "if (duoc) TiengDong.XacNhan(); else TiengDong.Bam();" },
            { "Assets/Scripts/UI/GameHUD.cs", "if (duoc) TiengDong.XacNhan(); else TiengDong.Bam();" },
            { "Assets/Scripts/UI/GameHUD.cs", "TiengDong.Bam();\n        CuaSoSachPhep.Mo();" },
            { "Assets/Scripts/UI/GameHUD.cs", "TiengDong.Bam();\n        CamUng.KhoaCam" },
            { "Assets/Scripts/UI/GameHUDThoatTran.cs", "TiengDong.Bam();\n        MoHoiThoat();" },
        };
        int co = 0;
        for (int i = 0; i < can.GetLength(0); i++)
        {
            string t = File.ReadAllText(can[i, 0]).Replace("\r\n", "\n");
            if (t.Contains(can[i, 1])) co++; else Ghi("[LOI] thieu tieng bam o " + can[i, 0] + ": " + can[i, 1]);
        }
        Ghi("A2. ma nguon: " + co + "/" + can.GetLength(0) + " cho goi tieng dung loai");
        if (co != can.GetLength(0)) loi++;
    }

    // ---------------- do tieng ----------------
    static readonly float[] bufL = new float[1024], bufR = new float[1024];

    /// <summary>Phat mot tieng, do RMS LON NHAT moi kenh trong 0,5 s sau do + tuong quan hai kenh luc to nhat.</summary>
    static IEnumerator DoMotTieng(System.Action phat, System.Action<float, float, float> nhan)
    {
        phat();
        float t0 = Time.realtimeSinceStartup, maxL = 0, maxR = 0; double sl = 0, sr = 0, slr = 0;
        while (Time.realtimeSinceStartup - t0 < 0.5f)
        {
            yield return null;
            AudioListener.GetOutputData(bufL, 0);
            AudioListener.GetOutputData(bufR, 1);
            double a = 0, b = 0, ab = 0;
            for (int i = 0; i < bufL.Length; i++) { a += bufL[i] * bufL[i]; b += bufR[i] * bufR[i]; ab += bufL[i] * bufR[i]; }
            float rl = (float)System.Math.Sqrt(a / bufL.Length), rr = (float)System.Math.Sqrt(b / bufL.Length);
            maxL = Mathf.Max(maxL, rl); maxR = Mathf.Max(maxR, rr);
            sl += a; sr += b; slr += ab;
        }
        float tq = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        yield return new WaitForSecondsRealtime(0.9f);       // cho tieng vang tat han truoc lan sau
        nhan(maxL, maxR, tq);
    }

    static int Tong() { return TiengDong.SoLan[0] + TiengDong.SoLan[1]; }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.2f);
    }

    // ---------------- B - D ----------------
    static IEnumerator ChayKichBan()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        var nn = NhacNen.Ban;
        if (nn == null) { Ghi("[LOI] khong co NhacNen"); loi++; Ket(); yield break; }
        nn.sanh.nguon.mute = true;      // nhac nen im trong luc do tieng bam
        Ghi("");
        Ghi(string.Format("B1. mac dinh hieu ung {0}% (dang nghe {1:F2}); kho co khoa {2}", CaiDatAmThanh.HieuUngPhanTram,
            CaiDatAmThanh.HieuUngDangNghe, PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng)));
        Kiem(CaiDatAmThanh.HieuUngPhanTram == 80 && !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng), "mac dinh hieu ung khong phai 80% / bi ghi xuong kho");

        // B2. Nut / Tab voi su kien gia - moi su kien mot luot OnGUI
        var tc = Object.FindAnyObjectByType<ChayThuMang>();
        var r = new Rect(200, 200, 240, 60);
        // (loai nut, kieu su kien, x)
        int[] nut = { 0, 0, 1, 1, 2, 2, 0, 0 };          // 0 Nut thuong, 1 Nut xac nhan, 2 Tab
        EventType[] kieu = { EventType.MouseDown, EventType.MouseUp, EventType.MouseDown, EventType.MouseUp,
                             EventType.MouseDown, EventType.MouseUp, EventType.MouseDown, EventType.MouseUp };
        float[] xs = { 300, 300, 300, 300, 300, 300, 50, 50 };     // cap cuoi: bam ngoai nut
        var dem = new List<string>();
        int buoc = 0;
        tc.veGUI = () =>
        {
            if (buoc >= kieu.Length || Event.current.type != EventType.Repaint) return;
            var goc = Event.current;
            int truoc0 = TiengDong.SoLan[0], truoc1 = TiengDong.SoLan[1];
            Event.current = new Event { type = kieu[buoc], mousePosition = new Vector2(xs[buoc], xs[buoc] < 100 ? 50 : 230), button = 0 };
            if (nut[buoc] == 0) GiaoDien.Nut(r, "THỬ", GiaoDien.KieuNutDa);
            else if (nut[buoc] == 1) GiaoDien.Nut(r, "THỬ", GiaoDien.KieuNutMau, true);
            else GiaoDien.Tab(r, "THỬ", false, 1f);
            Event.current = goc;
            if (kieu[buoc] == EventType.MouseUp)
                dem.Add((TiengDong.SoLan[0] - truoc0) + "/" + (TiengDong.SoLan[1] - truoc1));
            buoc++;
        };
        for (int i = 0; i < 80 && buoc < kieu.Length; i++) yield return null;
        tc.veGUI = null;
        GUIUtility.hotControl = 0;
        Ghi("B2. su kien gia (them thuong/xac nhan): Nut " + (dem.Count > 0 ? dem[0] : "?") + ", Nut xac nhan " + (dem.Count > 1 ? dem[1] : "?")
            + ", Tab " + (dem.Count > 2 ? dem[2] : "?") + ", bam ngoai nut " + (dem.Count > 3 ? dem[3] : "?"));
        Kiem(dem.Count == 4 && dem[0] == "1/0" && dem[1] == "0/1" && dem[2] == "1/0" && dem[3] == "0/0", "nut / tab khong keu dung loai");

        // B3. chong lap trong mot khung
        yield return null;
        int a0 = TiengDong.SoLan[0], a1 = TiengDong.SoLan[1];
        TiengDong.Bam(); TiengDong.Bam();
        int d1 = TiengDong.SoLan[0] - a0;
        yield return null;
        a0 = TiengDong.SoLan[0]; a1 = TiengDong.SoLan[1];
        TiengDong.Bam(); TiengDong.XacNhan();
        Ghi(string.Format("B3. cung khung: hai lan bam -> {0} tieng; bam + xac nhan -> thuong {1}, xac nhan {2}", d1, TiengDong.SoLan[0] - a0, TiengDong.SoLan[1] - a1));
        Kiem(d1 == 1 && TiengDong.SoLan[0] - a0 == 0 && TiengDong.SoLan[1] - a1 == 1, "chong lap tieng trong mot khung sai");
        yield return new WaitForSecondsRealtime(1.2f);

        // B4. tieng that o loa
        float l80 = 0, r80 = 0, tq = 1, l40 = 0;
        double s80 = 0, s40 = 0;
        for (int lan = 0; lan < 4; lan++)
        {
            CaiDatAmThanh.LuuHieuUng(80);
            yield return DoMotTieng(TiengDong.XacNhan, (a, b, c) => { l80 = a; r80 = b; tq = c; });
            s80 += (l80 + r80) * 0.5f;
            CaiDatAmThanh.LuuHieuUng(40);
            yield return DoMotTieng(TiengDong.XacNhan, (a, b, c) => { l40 = (a + b) * 0.5f; });
            s40 += l40;
        }
        PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaHieuUng); PlayerPrefs.Save(); CaiDatAmThanh.QuenBoNho();
        float tiLe = s80 > 0 ? (float)(s40 / s80) : 0;
        Ghi(string.Format("B4. tieng that (xac nhan): dinh RMS trai {0:F4} phai {1:F4}, tuong quan hai kenh {2:F3} (mono = 1); 40% / 80% = x{3:F2} (mong 0,50)",
            l80, r80, tq, tiLe));
        Kiem(l80 > 0.01f && r80 > 0.01f, "khong nghe thay tieng bam");
        Kiem(tq < 0.99f, "tieng bam khong stereo");
        Kiem(tiLe > 0.4f && tiLe < 0.6f, "thanh Hieu ung khong doi do to tieng bam");

        // ---- C. tab Am thanh ----
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] dang nhap: " + e); loi++; Ket(); yield break; }
        var dn = Object.FindAnyObjectByType<ManDangNhap>();
        if (dn != null && dn.daVao != null) dn.daVao();
        yield return new WaitForSecondsRealtime(1.5f);
        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh == null) { Ghi("[LOI] khong co sanh"); loi++; Ket(); yield break; }
        sanh.MoCaiDat(); sanh.ChonTabCaiDat(1);
        yield return new WaitForSecondsRealtime(0.4f);
        yield return Chup("tiengbam_1_tab_amthanh");
        DoAnh("tiengbam_1_tab_amthanh");

        int t0 = TiengDong.SoLan[0];
        sanh.DatAmLuongHieuUng(40);
        sanh.DatAmLuongHieuUng(41);            // keo tiep trong 0,15 s: khong keu them
        int them = TiengDong.SoLan[0] - t0;
        yield return null;
        Ghi(string.Format("C2. keo Hieu ung 40 -> 41: {0} tieng bam thu (mong 1), dang nghe {1:F2}, kho chua ghi {2}", them,
            CaiDatAmThanh.HieuUngDangNghe, !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng)));
        Kiem(them == 1 && Mathf.Abs(CaiDatAmThanh.HieuUngDangNghe - 0.41f) < 0.001f && !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng), "keo thanh Hieu ung khong nghe thu dung");
        yield return Chup("tiengbam_2_keo_41");
        sanh.BamHuyCaiDat();
        yield return null;
        Ghi(string.Format("C3. HUY: hieu ung ve {0:F2}, kho co khoa {1}", CaiDatAmThanh.HieuUngDangNghe, PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng)));
        Kiem(Mathf.Abs(CaiDatAmThanh.HieuUngDangNghe - 0.8f) < 0.001f && !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaHieuUng), "HUY khong tra ve muc cu");
        sanh.MoCaiDat(); sanh.ChonTabCaiDat(1);
        yield return new WaitForSecondsRealtime(0.2f);
        sanh.DatAmLuongHieuUng(40);
        sanh.BamOKCaiDat();
        yield return null;
        CaiDatAmThanh.QuenBoNho();
        Ghi(string.Format("C4. 40% + OK: kho {0}, doc lai {1}; nhac nen van {2}% (khoa nhac co {3})", PlayerPrefs.GetInt(CaiDatAmThanh.KhoaHieuUng, -1),
            CaiDatAmThanh.HieuUngPhanTram, CaiDatAmThanh.NhacPhanTram, PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac)));
        Kiem(PlayerPrefs.GetInt(CaiDatAmThanh.KhoaHieuUng, -1) == 40 && CaiDatAmThanh.HieuUngPhanTram == 40
             && CaiDatAmThanh.NhacPhanTram == CaiDatAmThanh.MacDinhPhanTram && !PlayerPrefs.HasKey(CaiDatAmThanh.KhoaNhac), "OK khong luu dung am luong hieu ung");

        // ---- D. trong tran ----
        FirebaseMang.Quen();
        SceneManager.LoadScene("Act2");
        for (int i = 0; i < 300 && SceneManager.GetActiveScene().name != "Act2"; i++) yield return null;
        yield return new WaitForSecondsRealtime(1.5f);
        CapDo.BatDauTranMoi();
        CapDo.ThemDiemChoPhepThu(1);
        int x0 = TiengDong.SoLan[0], x1 = TiengDong.SoLan[1];
        bool moKhoa = GameHUD.BamDauCong(0);
        yield return null;
        int sauMo0 = TiengDong.SoLan[0] - x0, sauMo1 = TiengDong.SoLan[1] - x1;
        x0 = TiengDong.SoLan[0]; x1 = TiengDong.SoLan[1];
        bool lan2 = GameHUD.BamDauCong(4);          // Thien thach can Cau lua cap 2 + het diem -> khong duoc
        Ghi(string.Format("D. trong tran: dau \"+\" mo khoa Cau lua {0} -> thuong {1} / xac nhan {2}; \"+\" khong duoc ({3}) -> thuong {4} / xac nhan {5}",
            moKhoa, sauMo0, sauMo1, lan2, TiengDong.SoLan[0] - x0, TiengDong.SoLan[1] - x1));
        Kiem(moKhoa && sauMo0 == 0 && sauMo1 == 1 && !lan2 && TiengDong.SoLan[0] - x0 == 1 && TiengDong.SoLan[1] - x1 == 0, "dau + trong tran keu sai loai");

        Ket();
    }

    /// <summary>Do tren ANH: tam nut thoi hang Hieu ung (doc lap voi code ve).</summary>
    static void DoAnh(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (!File.Exists(d)) { Ghi("[LOI] khong co anh"); loi++; return; }
        var tex = new Texture2D(2, 2); tex.LoadImage(File.ReadAllBytes(d));
        float s = Screen.height / 1080f;
        float rong = Mathf.Min(Screen.width - 30f * s, 880f * s), cao = Mathf.Min(Screen.height - 30f * s, 700f * s);
        float x = (Screen.width - rong) * 0.5f, y = (Screen.height - cao) * 0.5f, le = 34f * s;
        float[] mong = { 0.60f, 0.80f };
        Rect[] hang = { ManSanh.HangNhacNen(x + le, y + 170f * s, rong - 2f * le, s), ManSanh.HangHieuUng(x + le, y + 170f * s, rong - 2f * le, s) };
        string[] tenHang = { "Nhac nen", "Hieu ung" };
        float ti = tex.width / (float)Screen.width;
        for (int h = 0; h < 2; h++)
        {
            var thanh = ManSanh.ThanhNhacNen(hang[h], s);
            float leR = Mathf.Max(10f, 18f * s), trai = thanh.x + leR, phai = thanh.xMax - leR;
            int yA = tex.height - 1 - Mathf.RoundToInt(thanh.center.y * ti);
            int dem = 0; double tong = 0;
            for (int px = Mathf.RoundToInt((trai - 40f * s) * ti); px <= Mathf.RoundToInt((phai + 40f * s) * ti); px++)
            {
                var c = tex.GetPixel(px, yA);
                if (c.r > 0.85f && c.g < 0.32f && c.b < 0.25f) { dem++; tong += px; }
            }
            float phan = dem > 0 ? ((float)(tong / dem) / ti - trai) / (phai - trai) : -1f;
            Ghi(string.Format("C1. anh: hang {0} - tam nut thoi o {1:F3} (mong {2:F2})", tenHang[h], phan, mong[h]));
            Kiem(Mathf.Abs(phan - mong[h]) < 0.03f, "anh thanh " + tenHang[h] + " khong khop muc");
        }
        Object.DestroyImmediate(tex);
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengbam.txt", bao.ToString());
        FirebaseMang.Quen();
        NhacNen.BatTrongPhepThu = false;
        var rac = GameObject.Find("TAM_ThuTiengBam");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        if (coHUGoc) PlayerPrefs.SetInt(CaiDatAmThanh.KhoaHieuUng, huGoc); else PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaHieuUng);
        if (coNhacGoc) PlayerPrefs.SetInt(CaiDatAmThanh.KhoaNhac, nhacGoc); else PlayerPrefs.DeleteKey(CaiDatAmThanh.KhoaNhac);
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc); else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
        EditorUtility.audioMasterMute = tatTiengGoc;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);
        Debug.Log("[TiengBam] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
