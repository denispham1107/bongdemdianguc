using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 139 - TIENG QUA KEU MOI PHUT + BAY DOI 30 GIAY SAU (17/10/2026, nguoi dung; chon chi trong tran Act2, huong ngau nhien / bay ngang, thanh Hieu ung;
/// mau qua A "Qua den tham thiet" + doi C cua CongCu/AmThanh/tieng_qua_doi_3.py). Play Act2 that.
///   A. (ngoai Play) 12 file trung tung byte ban goc (4-6 = ban LAT trai / phai), STEREO, khong ForceToMono.
///   B. Dung 1 bo phat trong tran; MainMenu khong co GameBootstrap (sanh khong keu).
///   C. Lich (dong ho tran tua toi gan moc): qua dung giay 60, doi dung giay 90, qua 120, doi 150; loa co tieng, khong mono; qua lech dung ben cua
///      clip; doi BAY NGANG dung chieu (lech nua dau -> nua cuoi).
///   D. Am luong theo thanh Hieu ung: 40% / 80% = x0,50.
///   E. Phep thu khac dang chay (khong bat) -> im.
///   F. 16 lan moi loai: >= 4 clip khac nhau, khong lap clip lien truoc.
/// Ket qua: PlayTestShots/tiengmoitruong.txt.
/// </summary>
public static class ThuTiengMoiTruong
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengMoiTruong";

    [MenuItem("Diablo 2.5D/139. Chay thu TIENG QUA KEU · BAY DOI (moi phut, trong tran)", false, 229)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengMoiTruong] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengMoiTruong] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 3] qua mau A + doi mau C, 3 bien the + 3 ban lat moi loai");
        int dung = 0, tong = 0;
        var ds = new[] { new[] { "QuaKeu", "CongCu/AmThanh/QuaDoiThu3/A_qua_den_tham_thiet_qua" }, new[] { "DoiBay", "CongCu/AmThanh/QuaDoiThu3/C_bay_qua_hoang_loan_doi" } };
        foreach (var kv in ds)
            for (int i = 1; i <= 6; i++)
            {
                tong++;
                string game = "Assets/Resources/AmThanh/MoiTruong/" + kv[0] + i + ".wav";
                string g = kv[1] + (i <= 3 ? i.ToString() : (i - 3) + "_lat") + ".wav";
                var imp = AssetImporter.GetAtPath(game) as AudioImporter;
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
                bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
                bool ok = trung && c.channels == 2 && imp != null && !imp.forceToMono;
                if (ok) dung++;
                else Ghi(string.Format("A. {0}{1}: trung ban goc {2}; {3} kenh; ForceToMono {4}", kv[0], i, trung, c != null ? c.channels : 0, imp != null && imp.forceToMono));
            }
        Ghi(string.Format("A. {0}/{1} file trung ban goc, 2 kenh, khong ForceToMono", dung, tong));
        Kiem(dung == tong, "co file khong dung ban goc / khong STEREO");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        tatTiengGoc = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = false;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static string Md5(string d) { using (var m = MD5.Create()) using (var f = File.OpenRead(d)) return System.BitConverter.ToString(m.ComputeHash(f)); }
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengMoiTruong] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find(Ten) != null) return;
        daBatDau = true;
        var go = new GameObject(Ten);
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static int SL(TiengViTri.Loai l) { return TiengViTri.SoLan[(int)l]; }

    static List<TiengViTri.Giong> Giong(TiengViTri.Loai l)
    {
        var ds = new List<TiengViTri.Giong>();
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l && !g.dung) ds.Add(g);
        return ds;
    }

    static IEnumerator ChoIm(float toiDa)
    {
        float han = Time.realtimeSinceStartup + toiDa;
        while (Time.realtimeSinceStartup < han)
        {
            bool ranh = true;
            foreach (var g in TiengViTri.CacGiong) if (!g.Ranh) ranh = false;
            if (ranh) break;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.15f);
    }

    /// <summary>Nghe loa: kq = {RMS TB, nang luong trai, nang luong phai, tuong quan, RMS dinh}.</summary>
    static IEnumerator Nghe(float giay, float[] kq)
    {
        var bl = new float[1024]; var br = new float[1024];
        double sl = 0, sr = 0, slr = 0, tong = 0; int so = 0; float dinh = 0;
        float han = Time.realtimeSinceStartup + giay;
        while (Time.realtimeSinceStartup < han)
        {
            yield return null;
            AudioListener.GetOutputData(bl, 0);
            AudioListener.GetOutputData(br, 1);
            double a = 0, b = 0, ab = 0;
            for (int i = 0; i < bl.Length; i++) { a += bl[i] * bl[i]; b += br[i] * br[i]; ab += bl[i] * br[i]; }
            sl += a; sr += b; slr += ab;
            float rms = (float)System.Math.Sqrt((a + b) / (2.0 * bl.Length));
            tong += rms; so++; dinh = Mathf.Max(dinh, rms);
        }
        kq[0] = so > 0 ? (float)(tong / so) : 0f;
        kq[1] = (float)sl; kq[2] = (float)sr;
        kq[3] = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        kq[4] = dinh;
    }

    static Vector3 Dat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f); return p; }

    static Damageable TaoNguoi(string ten, Vector3 p, bool banSao)
    {
        var go = new GameObject(ten);
        go.transform.position = p;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = d.health = 1e7f; d.tiLeDoDon = 0f;
        d.mauDoMayKhacQuyet = banSao;
        return d;
    }

    static TiengViTri.Giong GiongMoi(int truocPhat, TiengViTri.Loai l)
    {
        var gm = TiengViTri.GiongMoiNhat;
        return TiengViTri.SoLanDuocGiong > truocPhat && gm != null && gm.loai == l ? gm : null;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; Ket(); yield break; }
        dir.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(e.gameObject);
        var toi = dir.player.GetComponent<Damageable>();
        toi.maxHealth = toi.health = 1e7f;
        NhacNen.BatTrongPhepThu = true;
        TiengMoiTruong.BatTrongPhepThu = true;
        var nn = NhacNen.Ban;
        if (nn != null) { nn.sanh.nguon.mute = true; nn.tran.nguon.mute = true; }
        yield return new WaitForSeconds(1.0f);
        float[] kq = new float[5];

        // ===== B. co dung mot bo phat trong tran =====
        var ds = Object.FindObjectsByType<TiengMoiTruong>(FindObjectsSortMode.None);
        var tm = TiengMoiTruong.Ban;
        Ghi(string.Format("B. so bo phat TiengMoiTruong trong Act2: {0}; GameBootstrap trong MainMenu.unity: {1} (chi Act2 tao bo phat)",
            ds.Length, File.ReadAllText("Assets/Scenes/MainMenu.unity").Contains("96d2553aa6c981d6669c4a7aca8d7237") ? "CO" : "khong"));
        Kiem(ds.Length == 1 && tm != null, "khong co dung 1 bo phat tieng moi truong trong tran");
        Kiem(!File.ReadAllText("Assets/Scenes/MainMenu.unity").Contains("96d2553aa6c981d6669c4a7aca8d7237"), "MainMenu co GameBootstrap - tieng se phat o sanh");
        if (tm == null) { Ket(); yield break; }
        var nguon = tm.GetComponent<AudioSource>();

        // ===== C. lich: qua 60, doi 90, qua 120, doi 150 (do bang dong ho tran that, tua toi gan moc) =====
        Ghi("");
        int q0 = TiengMoiTruong.SoLanQua, d0 = TiengMoiTruong.SoLanDoi;
        tm.DatDongHo(57f);
        float hanQ = Time.time + 6f;
        while (TiengMoiTruong.SoLanQua == q0 && Time.time < hanQ) yield return null;
        float lucQua = TiengMoiTruong.LucQuaCuoi;
        bool doiSom = TiengMoiTruong.SoLanDoi != d0;
        string clipQua = TiengMoiTruong.ClipQuaCuoi;
        yield return Nghe(3.0f, kq);
        float rmsQua = kq[0], tqQua = kq[3], lechQua = (kq[2] - kq[1]) / (kq[1] + kq[2] + 1e-12f);
        int soClip = 0; int.TryParse(clipQua != null ? clipQua.Replace("QuaKeu", "") : "0", out soClip);
        Ghi(string.Format("C1. dat dong ho 57 s -> qua keu luc dong ho {0:F2} s (clip {1}), doi phat som {2}; loa RMS {3:F4} tuong quan {4:F2}, lech (P-T)/(P+T) {5:F2} (clip 1-3 ben phai, 4-6 ben trai)",
            lucQua, clipQua, doiSom, rmsQua, tqQua, lechQua));
        Kiem(TiengMoiTruong.SoLanQua - q0 == 1 && Mathf.Abs(lucQua - 60f) < 0.1f && !doiSom, "qua khong keu dung giay 60");
        Kiem(rmsQua > 1e-3f && tqQua < 0.9f, "loa khong co tieng qua / gan mono");
        Kiem(soClip >= 1 && soClip <= 6 && ((soClip <= 3) == (lechQua > 0.05f)) && Mathf.Abs(lechQua) > 0.05f, "tieng qua khong lech dung ben cua clip");
        nguon.Stop();
        // doi luc 90
        tm.DatDongHo(88.5f);
        float hanD = Time.time + 4f;
        while (TiengMoiTruong.SoLanDoi == d0 && Time.time < hanD) yield return null;
        float lucDoi = TiengMoiTruong.LucDoiCuoi; string clipDoi = TiengMoiTruong.ClipDoiCuoi;
        // nghe tung nua: dau / cuoi -> huong bay
        float[] kqDau = new float[5], kqCuoi = new float[5];
        yield return Nghe(1.8f, kqDau);
        yield return Nghe(0.8f, kq);
        yield return Nghe(1.8f, kqCuoi);
        float lechDau = (kqDau[2] - kqDau[1]) / (kqDau[1] + kqDau[2] + 1e-12f), lechCuoi = (kqCuoi[2] - kqCuoi[1]) / (kqCuoi[1] + kqCuoi[2] + 1e-12f);
        int soDoi = 0; int.TryParse(clipDoi != null ? clipDoi.Replace("DoiBay", "") : "0", out soDoi);
        Ghi(string.Format("C2. dat 88,5 s -> doi bay luc {0:F2} s (clip {1}); lech nua dau {2:F2} -> nua cuoi {3:F2} (clip 1-3 trai -> phai, 4-6 phai -> trai); RMS {4:F4} tuong quan {5:F2}",
            lucDoi, clipDoi, lechDau, lechCuoi, kqDau[0], kqDau[3]));
        Kiem(TiengMoiTruong.SoLanDoi - d0 == 1 && Mathf.Abs(lucDoi - 90f) < 0.1f, "doi khong bay dung giay 90 (qua + 30)");
        Kiem(soDoi >= 1 && soDoi <= 6 && (soDoi <= 3 ? lechCuoi - lechDau > 0.2f : lechDau - lechCuoi > 0.2f), "doi khong bay ngang dung chieu");
        Kiem(kqDau[0] > 1e-3f && kqDau[3] < 0.9f, "loa khong co tieng doi / gan mono");
        nguon.Stop();
        // moc sau: 120 qua, 150 doi (khong qua)
        int q1 = TiengMoiTruong.SoLanQua, d1 = TiengMoiTruong.SoLanDoi;
        tm.DatDongHo(119.5f); float h2 = Time.time + 2f; while (TiengMoiTruong.SoLanQua == q1 && Time.time < h2) yield return null;
        float lucQua2 = TiengMoiTruong.LucQuaCuoi;
        nguon.Stop();
        tm.DatDongHo(149.5f); float h3 = Time.time + 2f; while (TiengMoiTruong.SoLanDoi == d1 && Time.time < h3) yield return null;
        float lucDoi2 = TiengMoiTruong.LucDoiCuoi;
        Ghi(string.Format("C3. qua tiep luc {0:F2} s, doi tiep luc {1:F2} s; trong 149,5-150,1 s qua them {2}", lucQua2, lucDoi2, TiengMoiTruong.SoLanQua - q1 - 1));
        Kiem(Mathf.Abs(lucQua2 - 120f) < 0.1f && Mathf.Abs(lucDoi2 - 150f) < 0.1f && TiengMoiTruong.SoLanQua - q1 == 1, "lich lap moi phut sai");
        nguon.Stop();
        yield return new WaitForSeconds(0.3f);

        // ===== D. am luong theo thanh Hieu ung =====
        Ghi("");
        float[] rms = new float[2];
        int[] muc = { 40, 80 };
        for (int i = 0; i < 2; i++)
        {
            CaiDatAmThanh.XemTruocHieuUng(muc[i]);
            int qx = TiengMoiTruong.SoLanQua;
            tm.DatDongHo(179.9f + 60f * i);
            while (TiengMoiTruong.SoLanQua == qx) yield return null;
            // cung clip de so: phat lai dung clip dau
            nguon.Stop(); nguon.clip = Resources.Load<AudioClip>("AmThanh/MoiTruong/QuaKeu1"); nguon.pitch = 1f; nguon.Play();
            yield return null;
            yield return Nghe(2.5f, kq);
            rms[i] = kq[0];
            nguon.Stop();
            yield return new WaitForSeconds(0.2f);
        }
        CaiDatAmThanh.BoXemTruoc();
        Ghi(string.Format("D. Hieu ung 40% -> RMS {0:F4}, 80% -> {1:F4}: ti le {2:F2} (mong 0,50)", rms[0], rms[1], rms[0] / Mathf.Max(1e-9f, rms[1])));
        Kiem(Mathf.Abs(rms[0] / Mathf.Max(1e-9f, rms[1]) - 0.5f) < 0.08f, "am luong khong theo thanh Hieu ung");

        // ===== E. im khi dang chay phep thu khac (khong bat) =====
        TiengMoiTruong.BatTrongPhepThu = false;
        int qe = TiengMoiTruong.SoLanQua;
        tm.DatDongHo(299.9f + 60f);
        while (TiengMoiTruong.SoLanQua == qe) yield return null;
        yield return null;
        Ghi(string.Format("E. co ChayThuMang, khong bat: qua van dem lich ({0}), am luong nguon {1:F3}", TiengMoiTruong.SoLanQua - qe, nguon.volume));
        Kiem(nguon.volume == 0f, "phep thu khac ma tieng moi truong van keu");
        TiengMoiTruong.BatTrongPhepThu = true;
        nguon.Stop();

        // ===== F. bien the: 16 lan moi loai, khong trung lan lien truoc =====
        Ghi("");
        var tenQ = new HashSet<string>(); var tenD = new HashSet<string>(); int trungQ = 0, trungD = 0; string truocQ = null, truocD = null;
        for (int i = 0; i < 16; i++)
        {
            int qx = TiengMoiTruong.SoLanQua, dx = TiengMoiTruong.SoLanDoi;
            tm.DatDongHo(1020f + i * 60f - 0.02f); while (TiengMoiTruong.SoLanQua == qx) yield return null;
            tenQ.Add(TiengMoiTruong.ClipQuaCuoi); if (TiengMoiTruong.ClipQuaCuoi == truocQ) trungQ++; truocQ = TiengMoiTruong.ClipQuaCuoi;
            tm.DatDongHo(1020f + i * 60f + TreDoiThu - 0.02f); while (TiengMoiTruong.SoLanDoi == dx) yield return null;
            tenD.Add(TiengMoiTruong.ClipDoiCuoi); if (TiengMoiTruong.ClipDoiCuoi == truocD) trungD++; truocD = TiengMoiTruong.ClipDoiCuoi;
        }
        nguon.Stop();
        Ghi(string.Format("F. 16 lan: qua {0} clip khac nhau (trung lien truoc {1}), doi {2} clip (trung {3})", tenQ.Count, trungQ, tenD.Count, trungD));
        Kiem(tenQ.Count >= 4 && tenD.Count >= 4 && trungQ == 0 && trungD == 0, "it bien the / lap lai clip lien truoc");
        Ket();
    }

    const float TreDoiThu = TiengMoiTruong.TreDoi;

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengmoitruong.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TiengMoiTruong.BatTrongPhepThu = false;
        CaiDatAmThanh.BoXemTruoc();
        TranHienTai.Xoa();
        var rac = GameObject.Find(Ten);
        if (rac != null) Object.DestroyImmediate(rac);
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
        EditorUtility.audioMasterMute = tatTiengGoc;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        Debug.Log("[TiengMoiTruong] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
