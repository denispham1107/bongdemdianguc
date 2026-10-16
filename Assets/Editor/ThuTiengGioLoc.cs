using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 132 - TIENG GIO LOC + LOC XOAY (16/10/2026, nguoi dung: Gio loc = ban 1 mau C "Loc cat" (gio thoi + bui cuon); Loc xoay = ban 3 A2 "hu lon" dai
/// hon + tieng set Sam set NHO HON gio hu; khong bi Unity bop; trong suong thi khong nghe). Play Act2 that, tat GameDirector + xoa quai.
///   A. (ngoai Play) 5 file trung tung byte ban goc, STEREO; vong lap Gio loc 3 s, Loc xoay 7,6 s.
///   B. GIO LOC THAT (CastAt 10): moi con loc 1 gio thoi + 1 bui VONG LAP bam theo, can bang (2D); loc tan -> het tieng; loa co tieng, khong mono.
///   C. LOC XOAY THAT (CastAt 3): 1 tieng gio bam theo loc, can bang; moi nhip tia 1 tieng set (giong Sam set danh, he so 0,35); loc xoa -> tat.
///   C2. Tieng set NHO HON tieng gio: phat rieng tung tieng tai cung cho, do RMS loa.
///   D. SUONG MU: o 40 m -> giong muc 0 / set bi bo, loa im; doi chung tat suong co tieng.
///   E. BIEN THE Loc xoay: 3 clip, cao do +-6%.
/// Ket qua: PlayTestShots/tienggioloc.txt.
/// </summary>
public static class ThuTiengGioLoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengGioLoc";

    [MenuItem("Diablo 2.5D/132. Chay thu TIENG GIO LOC + LOC XOAY (gio · bui · gio hu · set, suong mu)", false, 221)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengGioLoc] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengGioLoc] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng Gio loc (mau C 'Loc cat') + Loc xoay (A2 'hu lon' 7,6 s + set Sam set nho hon)");
        var ds = new[] {
            new[] { "GioLocThoi", "CongCu/AmThanh/GioLocThu/C_loc_cat_thoi.wav", "3" }, new[] { "GioLocBui", "CongCu/AmThanh/GioLocThu/C_loc_cat_bui.wav", "3" },
            new[] { "LocXoayGio1", "CongCu/AmThanh/GioLocThu3/LocXoay_hu_1.wav", "7.6" }, new[] { "LocXoayGio2", "CongCu/AmThanh/GioLocThu3/LocXoay_hu_2.wav", "7.6" },
            new[] { "LocXoayGio3", "CongCu/AmThanh/GioLocThu3/LocXoay_hu_3.wav", "7.6" } };
        int dung = 0;
        foreach (var kv in ds)
        {
            string game = "Assets/Resources/AmThanh/KyNang/" + kv[0] + ".wav";
            var imp = AssetImporter.GetAtPath(game) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
            bool trung = c != null && File.Exists(kv[1]) && Md5(kv[1]) == Md5(game);
            if (trung && c.channels == 2 && imp != null && !imp.forceToMono && Mathf.Abs(c.length - float.Parse(kv[2], System.Globalization.CultureInfo.InvariantCulture)) < 0.01f) dung++;
            else Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh; dai {3}", kv[0], trung, c != null ? c.channels : 0, c != null ? c.length : 0));
        }
        Ghi(string.Format("A. {0}/{1} file trung ban goc, 2 kenh, dung do dai", dung, ds.Length));
        Kiem(dung == ds.Length, "co file khong dung ban goc / khong STEREO / sai do dai");
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengGioLoc] " + s); }
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

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; Ket(); yield break; }
        dir.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(e.gameObject);
        var pc = dir.player.GetComponent<PlayerController>();
        var toi = dir.player.GetComponent<Damageable>();
        toi.maxHealth = toi.health = 1e7f;
        NhacNen.BatTrongPhepThu = true;
        TamNhin.BatTrongPhepThu = true;
        var nn = NhacNen.Ban;
        if (nn != null) { nn.sanh.nguon.mute = true; nn.tran.nguon.mute = true; }
        yield return new WaitForSeconds(1.0f);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        var camT = Camera.main.transform;
        Vector3 truoc = camT.forward; truoc.y = 0; truoc.Normalize();
        Vector3 goc = pc.transform.position;
        float[] kq = new float[5];
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(CapDo.KyGioLoc); CapDo.MoCaDuongChoPhepThu(3);
        yield return ChoIm(3f);

        // ===== B. Gio loc that =====
        Ghi("");
        pc.mana = pc.maxMana;
        pc.transform.rotation = Quaternion.LookRotation(truoc);
        int t0 = SL(TiengViTri.Loai.GioLocThoi), b0 = SL(TiengViTri.Loai.GioLocBui);
        pc.CastAt(CapDo.KyGioLoc, goc + truoc * 10f);
        var locs = new List<GioLoc>();
        float hanB = Time.time + 2f;
        while (locs.Count == 0 && Time.time < hanB) { locs.AddRange(Object.FindObjectsByType<GioLoc>(FindObjectsSortMode.None)); yield return null; }
        yield return new WaitForSeconds(0.3f);
        var gt = Giong(TiengViTri.Loai.GioLocThoi); var gb = Giong(TiengViTri.Loai.GioLocBui);
        int bamDung = 0; bool canBang = true;
        foreach (var g in gt) { foreach (var l in locs) if (l != null && g.theo == l.transform && g.bamTheo) bamDung++; if (g.nguon.spatialBlend != 0f) canBang = false; }
        foreach (var g in gb) { foreach (var l in locs) if (l != null && g.theo == l.transform && g.bamTheo) bamDung++; if (g.nguon.spatialBlend != 0f) canBang = false; }
        yield return Nghe(1.0f, kq);
        float[] kqB = (float[])kq.Clone();
        float hanB2 = Time.time + 7f;
        while (Time.time < hanB2) { bool con = false; foreach (var l in locs) if (l != null) con = true; if (!con) break; yield return null; }
        yield return new WaitForSeconds(0.3f);
        int conB = Giong(TiengViTri.Loai.GioLocThoi).Count + Giong(TiengViTri.Loai.GioLocBui).Count;
        Ghi(string.Format("B. Gio loc that: {0} con loc; gio thoi +{1}, bui +{2}; giong bam dung loc {3}; can bang {4}; loc tan -> giong con {5}; loa RMS {6:F4}, tuong quan {7:F3}",
            locs.Count, SL(TiengViTri.Loai.GioLocThoi) - t0, SL(TiengViTri.Loai.GioLocBui) - b0, bamDung, canBang, conB, kqB[0], kqB[3]));
        Kiem(locs.Count >= 2 && SL(TiengViTri.Loai.GioLocThoi) - t0 == locs.Count && SL(TiengViTri.Loai.GioLocBui) - b0 == locs.Count && bamDung == 2 * locs.Count,
             "khong moi con loc 1 gio thoi + 1 bui bam theo");
        Kiem(canBang, "tieng Gio loc khong phat can bang");
        Kiem(conB == 0, "loc tan ma van keu");
        Kiem(kqB[0] > 1e-3f && kqB[3] < 0.6f, "loa khong co tieng / gan mono");
        yield return ChoIm(4f);

        // ===== C. Loc xoay that =====
        Ghi("");
        pc.mana = pc.maxMana;
        int x0 = SL(TiengViTri.Loai.LocXoayGio), s0 = TiengViTri.SoLanSetLocXoay;
        pc.CastAt(3, goc + truoc * 9f);
        Tornado loc = null;
        float hanC = Time.time + 2f;
        while (loc == null && Time.time < hanC) { loc = Object.FindAnyObjectByType<Tornado>(); yield return null; }
        yield return new WaitForSeconds(1.2f);
        var gx = Giong(TiengViTri.Loai.LocXoayGio);
        bool bamLoc = loc != null && gx.Count == 1 && gx[0].theo == loc.transform && gx[0].bamTheo && gx[0].nguon.spatialBlend == 0f;
        float heSoSetMax = 0f;
        foreach (var g in Giong(TiengViTri.Loai.SamSetDanh)) heSoSetMax = Mathf.Max(heSoSetMax, g.heSo);
        yield return Nghe(1.0f, kq);
        float[] kqC = (float[])kq.Clone();
        float hanC2 = Time.time + 12f;
        while (loc != null && Time.time < hanC2) yield return null;
        yield return new WaitForSeconds(0.3f);
        int conC = Giong(TiengViTri.Loai.LocXoayGio).Count;
        int soSet = TiengViTri.SoLanSetLocXoay - s0;
        Ghi(string.Format("C. Loc xoay that: gio +{0} (bam theo loc, 2D {1}); tieng set +{2} (he so lon nhat {3:F2}); loc xoa -> gio con {4}; loa RMS {5:F4}, tuong quan {6:F3}",
            SL(TiengViTri.Loai.LocXoayGio) - x0, bamLoc, soSet, heSoSetMax, conC, kqC[0], kqC[3]));
        Kiem(SL(TiengViTri.Loai.LocXoayGio) - x0 == 1 && bamLoc, "Loc xoay khong co 1 tieng gio bam theo (2D)");
        Kiem(soSet >= 10 && heSoSetMax > 0f && heSoSetMax <= TiengViTri.HeSoSetLocXoay + 1e-4f, "tieng set Loc xoay khong du / khong nho lai");
        Kiem(conC == 0, "loc xoa ma gio van keu");
        Kiem(kqC[0] > 1e-3f && kqC[3] < 0.6f, "loa khong co tieng / gan mono");
        yield return ChoIm(6f);

        // ===== C2. set nho hon gio =====
        Ghi("");
        Vector3 cho = Dat(goc + truoc * 3f) + Vector3.up;
        var vat = new GameObject("TAM_LocGia").transform; vat.position = cho;
        var gg = TiengViTri.LocXoayGio(vat);
        yield return new WaitForSeconds(0.8f);
        yield return Nghe(2.0f, kq);
        float rmsGio = kq[0];
        TiengViTri.Dung(gg, vat);
        yield return ChoIm(3f);
        double tongSet = 0;
        for (int i = 0; i < 3; i++) { TiengViTri.LocXoaySet(cho); yield return Nghe(0.6f, kq); tongSet += kq[0]; yield return ChoIm(3f); }
        float rmsSet = (float)(tongSet / 3);
        Ghi(string.Format("C2. loa: tieng gio Loc xoay RMS {0:F4} | tieng set Loc xoay (0,6 s dau moi tieng) RMS {1:F4} -> set / gio {2:F2}", rmsGio, rmsSet, rmsSet / Mathf.Max(1e-6f, rmsGio)));
        Kiem(rmsGio > 1e-3f && rmsSet < rmsGio, "tieng set Loc xoay khong nho hon tieng gio");
        Object.Destroy(vat.gameObject);
        yield return ChoIm(3f);

        // ===== D. suong mu =====
        Ghi("");
        Vector3 xa = Dat(goc + truoc * 40f) + Vector3.up;
        var vxa = new GameObject("TAM_LocXa").transform; vxa.position = xa;
        int che0 = TiengViTri.SoBiSuongChe;
        var a1 = TiengViTri.GioLocThoi(vxa); var a2 = TiengViTri.GioLocBui(vxa); var a3 = TiengViTri.LocXoayGio(vxa);
        TiengViTri.LocXoaySet(xa);
        yield return new WaitForSeconds(0.3f);
        yield return Nghe(0.8f, kq);
        float mucMax = 0f; foreach (var a in new[] { a1, a2, a3 }) if (a != null) mucMax = Mathf.Max(mucMax, a.MucDangDat);
        Ghi(string.Format("D. o 40 m (MucThay {0:F2}): muc giong lon nhat {1:F3}; set bi suong che +{2}; RMS loa {3:F5}", TamNhin.MucThay(xa), mucMax, TiengViTri.SoBiSuongChe - che0, kq[0]));
        Kiem(mucMax == 0f && TiengViTri.SoBiSuongChe - che0 == 1 && kq[0] < 1e-4f, "Gio loc / Loc xoay trong suong van nghe thay");
        TamNhin.BatTrongPhepThu = false;
        yield return new WaitForSeconds(0.3f);
        float mucDc = a1 != null ? a1.MucDangDat : -1f;
        Ghi(string.Format("D2. DOI CHUNG tat suong: gio thoi o 40 m muc {0:F3}", mucDc));
        Kiem(mucDc > 0.01f, "doi chung tat suong ma van im");
        TamNhin.BatTrongPhepThu = true;
        TiengViTri.Dung(a1, vxa); TiengViTri.Dung(a2, vxa); TiengViTri.Dung(a3, vxa);
        Object.Destroy(vxa.gameObject);
        yield return ChoIm(4f);

        // ===== E. bien the Loc xoay =====
        Ghi("");
        var clips = new HashSet<string>(); float caoMin = 9f, caoMax = 0f;
        var v2 = new GameObject("TAM_LocBienThe").transform; v2.position = cho;
        for (int i = 0; i < 18; i++)
        {
            int truocPhat = TiengViTri.SoLanDuocGiong;
            var g = TiengViTri.LocXoayGio(v2);
            if (TiengViTri.SoLanDuocGiong > truocPhat && g != null && g.nguon.clip != null)
            { clips.Add(g.nguon.clip.name); caoMin = Mathf.Min(caoMin, g.nguon.pitch); caoMax = Mathf.Max(caoMax, g.nguon.pitch); }
            TiengViTri.Dung(g, v2);
            yield return new WaitForSeconds(0.2f);
        }
        Object.Destroy(v2.gameObject);
        Ghi(string.Format("E. 18 lan Loc xoay: {0} bien the; cao do {1:F3} - {2:F3}", clips.Count, caoMin, caoMax));
        Kiem(clips.Count == 3 && caoMin >= 0.94f - 1e-4f && caoMax <= 1.06f + 1e-4f && caoMax - caoMin > 0.05f, "khong du 3 bien the / cao do sai");
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tienggioloc.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TamNhin.BatTrongPhepThu = false;
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
        Debug.Log("[TiengGioLoc] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
