using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 131 - TIENG MAY GIONG (16/10/2026, nguoi dung: them tieng GIONG va MUA ROI; tia set dung tieng Sam set; trong suong thi khong nghe). Mau C
/// "Giong xa" (CongCu/AmThanh/tieng_may_giong.py). Play Act2 that, tat GameDirector + xoa quai.
///   A. (ngoai Play) 2 file vong lap trung tung byte ban goc MayGiongThu/C_giong_xa_*.wav, STEREO, giong 4 s / mua 3 s.
///   B. TUNG THAT May giong (CastAt 21) quanh 3 bia: 1 giong + 1 mua VONG LAP bam theo may (ca luc may TU BAY - vi tri tieng = vi tri may); moi tia
///      1 tieng danh + 1 chay xem (tieng Sam set); may het -> hai vong lap tat; moi giong can bang (2D); loa co tieng, khong mono.
///   C. SUONG MU: may o 30 m -> giong + mua giu giong muc 0, tieng tia bi bo, loa im; doi chung tat suong -> co tieng.
/// Ket qua: PlayTestShots/tiengmaygiong.txt.
/// </summary>
public static class ThuTiengMayGiong
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengMayGiong";

    [MenuItem("Diablo 2.5D/131. Chay thu TIENG MAY GIONG (giong · mua · tia set, suong mu)", false, 220)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengMayGiong] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengMayGiong] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng May giong (mau C 'Giong xa': giong + mua vong lap) + tia set = tieng Sam set");
        int dung = 0;
        foreach (var kv in new[] { new[] { "MayGiongGiong", "C_giong_xa_giong", "4" }, new[] { "MayGiongMua", "C_giong_xa_mua", "3" } })
        {
            string game = "Assets/Resources/AmThanh/KyNang/" + kv[0] + ".wav";
            string g = "CongCu/AmThanh/MayGiongThu/" + kv[1] + ".wav";
            var imp = AssetImporter.GetAtPath(game) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
            bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
            if (trung && c.channels == 2 && imp != null && !imp.forceToMono && Mathf.Abs(c.length - float.Parse(kv[2])) < 0.001f) dung++;
            else Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh; dai {3}", kv[0], trung, c != null ? c.channels : 0, c != null ? c.length : 0));
        }
        Ghi(string.Format("A. {0}/2 file trung ban goc, 2 kenh, dung do dai vong lap", dung));
        Kiem(dung == 2, "file vong lap khong dung ban goc / khong STEREO");
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengMayGiong] " + s); }
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

    static Damageable TaoBia(string ten, Vector3 p)
    {
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = d.health = 1e7f;
        d.tiLeDoDon = 0f;
        return d;
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
        Vector3 phai = camT.right; phai.y = 0; phai.Normalize();
        Vector3 goc = pc.transform.position;
        float[] kq = new float[5];
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(CapDo.KyMayGiong);
        yield return ChoIm(3f);

        // ===== B. tung that =====
        Ghi("");
        Vector3 tam = Dat(goc + truoc * 8f);
        var bias = new List<Damageable>();
        for (int i = -1; i <= 1; i++) bias.Add(TaoBia("TAM_BiaMay" + (i + 1), Dat(tam + phai * i * 2.5f)));
        Physics.SyncTransforms();
        pc.mana = pc.maxMana;
        int g0 = SL(TiengViTri.Loai.MayGiongGiong), m0 = SL(TiengViTri.Loai.MayGiongMua), d0 = SL(TiengViTri.Loai.SamSetDanh), c0 = SL(TiengViTri.Loai.SamSetChay),
            r0 = SL(TiengViTri.Loai.SamSetRen), t0 = MayGiong.SoTiaDaDanh;
        pc.CastAt(CapDo.KyMayGiong, tam);
        MayGiong may = null;
        float hanB = Time.time + 3f;
        while (may == null && Time.time < hanB) { may = Object.FindAnyObjectByType<MayGiong>(); yield return null; }
        yield return new WaitForSeconds(1.0f);
        var gg = Giong(TiengViTri.Loai.MayGiongGiong); var gm = Giong(TiengViTri.Loai.MayGiongMua);
        bool bam = may != null && gg.Count == 1 && gm.Count == 1 && gg[0].theo == may.transform && gm[0].theo == may.transform && gg[0].bamTheo && gm[0].bamTheo;
        bool canBang = gg.Count == 1 && gm.Count == 1 && gg[0].nguon.spatialBlend == 0f && gm[0].nguon.spatialBlend == 0f;
        yield return Nghe(1.5f, kq);
        float[] kqDung = (float[])kq.Clone();
        // cho may tu bay: do vi tri tieng so voi may
        float lechMax = 0f, diMax = 0f; Vector3 choDau = may != null ? may.transform.position : Vector3.zero;
        float hanBay = Time.time + 9f;
        while (may != null && Time.time < hanBay)
        {
            foreach (var g in Giong(TiengViTri.Loai.MayGiongMua)) lechMax = Mathf.Max(lechMax, (g.nguon.transform.position - may.transform.position).magnitude);
            diMax = Mathf.Max(diMax, (may.transform.position - choDau).magnitude);
            yield return null;
        }
        yield return new WaitForSeconds(0.4f);
        int vongCon = Giong(TiengViTri.Loai.MayGiongGiong).Count + Giong(TiengViTri.Loai.MayGiongMua).Count;
        int soTia = MayGiong.SoTiaDaDanh - t0;
        Ghi(string.Format("B. tung that: giong +{0}, mua +{1} (bam theo may {2}, can bang 2D {3}); {4} tia -> danh +{5}, chay xem +{6}, sam ren +{7}",
            SL(TiengViTri.Loai.MayGiongGiong) - g0, SL(TiengViTri.Loai.MayGiongMua) - m0, bam, canBang, soTia,
            SL(TiengViTri.Loai.SamSetDanh) - d0, SL(TiengViTri.Loai.SamSetChay) - c0, SL(TiengViTri.Loai.SamSetRen) - r0));
        Ghi(string.Format("   may tu bay {0:F1} m, tieng mua lech may toi da {1:F3} m; may het -> vong lap con {2}; loa luc dang mua: RMS {3:F4}, tuong quan {4:F3}",
            diMax, lechMax, vongCon, kqDung[0], kqDung[3]));
        Kiem(SL(TiengViTri.Loai.MayGiongGiong) - g0 == 1 && SL(TiengViTri.Loai.MayGiongMua) - m0 == 1 && bam, "khong co 1 giong + 1 mua vong lap bam theo may");
        Kiem(canBang, "giong / mua khong phat can bang");
        Kiem(soTia == MayGiong.SoTiaTong && SL(TiengViTri.Loai.SamSetDanh) - d0 == soTia && SL(TiengViTri.Loai.SamSetChay) - c0 == soTia && SL(TiengViTri.Loai.SamSetRen) - r0 == 0,
             "moi tia khong 1 tieng danh + 1 chay xem (tieng Sam set)");
        Kiem(diMax > 4f && lechMax < 1.6f, "tieng mua khong bam theo may luc may tu bay");
        Kiem(vongCon == 0, "may het ma giong / mua van keu");
        Kiem(kqDung[0] > 1e-3f && kqDung[3] < 0.6f, "loa khong co tieng / gan mono");
        foreach (var b in bias) Object.Destroy(b.gameObject);
        yield return ChoIm(6f);

        // ===== C. suong mu =====
        Ghi("");
        Vector3 xa = Dat(goc + truoc * (TamNhin.BanKinh + 10f)) + Vector3.up;
        var vat = new GameObject("TAM_MayXa").transform; vat.position = xa;
        int che0 = TiengViTri.SoBiSuongChe;
        var gx1 = TiengViTri.MayGiongGiong(vat); var gx2 = TiengViTri.MayGiongMua(vat);
        TiengViTri.SamSetDanh(xa); TiengViTri.SamSetChay(xa);
        yield return new WaitForSeconds(0.3f);
        yield return Nghe(0.8f, kq);
        Ghi(string.Format("C. may o 30 m (MucThay {0:F2}): giong / mua muc {1:F3} / {2:F3}; tieng tia bi suong che +{3}; RMS loa {4:F5}",
            TamNhin.MucThay(xa), gx1 != null ? gx1.MucDangDat : -1f, gx2 != null ? gx2.MucDangDat : -1f, TiengViTri.SoBiSuongChe - che0, kq[0]));
        Kiem((gx1 == null || gx1.MucDangDat == 0f) && (gx2 == null || gx2.MucDangDat == 0f) && TiengViTri.SoBiSuongChe - che0 == 2 && kq[0] < 1e-4f,
             "May giong trong suong van nghe thay");
        TamNhin.BatTrongPhepThu = false;
        yield return new WaitForSeconds(0.3f);
        float mucDc = gx2 != null ? gx2.MucDangDat : -1f;
        Ghi(string.Format("C2. DOI CHUNG tat suong: mua o 30 m muc {0:F3}", mucDc));
        Kiem(mucDc > 0.01f, "doi chung tat suong ma van im");
        TamNhin.BatTrongPhepThu = true;
        TiengViTri.Dung(gx1, vat); TiengViTri.Dung(gx2, vat);
        Object.Destroy(vat.gameObject);
        yield return ChoIm(5f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengmaygiong.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TamNhin.BatTrongPhepThu = false;
        TranHienTai.Xoa();
        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (d0 != null && d0.name.StartsWith("TAM_")) Object.DestroyImmediate(d0.gameObject);
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
        Debug.Log("[TiengMayGiong] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
