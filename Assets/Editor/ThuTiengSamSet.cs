using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 130 - TIENG SAM SET (16/10/2026, nguoi dung: tia set PHONG tu tren cao xuong · trung mat dat / ke dich nghe CHAY XEM · them SAM REN (nguoi
/// dung chon) · stereo that, KHONG bi Unity bop · trong suong thi khong nghe). Mau B2 "Set xe troi" dai + day (CongCu/AmThanh/tieng_sam_set.py --b2).
/// Play Act2 that, tat GameDirector + xoa quai.
///   A. (ngoai Play) 9 file trung tung byte ban goc SamSetThu/B2_*.wav, STEREO, khong ForceToMono.
///   B. TUNG THAT Sam set (CastAt 2) quanh 3 bia: 1 tieng sam ren; moi tia danh xuong 1 yeu cau tieng danh + 1 tieng chay xem; cung luc toi da 4 + 4;
///      moi giong phat CAN BANG (spatialBlend 0); loa co tieng, khong mono.
///   C. SUONG MU: danh / chay / ren o 40 m -> bo, loa im; doi chung tat suong -> co giong.
///   D. BIEN THE: 24 lan moi loai -> du 3 bien the, cao do +-6%.
///   E. CAN BANG vs DOI CHUNG 3D spread 120 (cung clip chay xem): giua stereo hon han, ben phai ro ben ma khong mono.
/// Ket qua: PlayTestShots/tiengsamset.txt.
/// </summary>
public static class ThuTiengSamSet
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengSamSet";
    const string Goc = "CongCu/AmThanh/SamSetThu/B2_";

    [MenuItem("Diablo 2.5D/130. Chay thu TIENG SAM SET (danh · chay xem · sam ren, can bang, suong mu)", false, 219)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengSamSet] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengSamSet] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng Sam set (B2 'Set xe troi' dai + day) + cach phat can bang");
        int dung = 0, tong = 0;
        foreach (var kv in new[] { new[] { "SamSetDanh", "danh" }, new[] { "SamSetChay", "chay" }, new[] { "SamSetRen", "ren" } })
            for (int i = 1; i <= 3; i++)
            {
                tong++;
                string game = "Assets/Resources/AmThanh/KyNang/" + kv[0] + i + ".wav";
                string g = Goc + kv[1] + i + ".wav";
                var imp = AssetImporter.GetAtPath(game) as AudioImporter;
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
                bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
                if (trung && c.channels == 2 && imp != null && !imp.forceToMono) dung++;
                else Ghi(string.Format("A. {0}{1}: trung ban goc {2}; {3} kenh", kv[0], i, trung, c != null ? c.channels : 0));
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengSamSet] " + s); }
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
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(2);
        yield return ChoIm(3f);

        // ===== B. tung that =====
        Ghi("");
        Vector3 tam = Dat(goc + truoc * 7f);
        var bias = new List<Damageable>();
        for (int i = -1; i <= 1; i++) bias.Add(TaoBia("TAM_BiaSam" + (i + 1), Dat(tam + phai * i * 2.5f)));
        Physics.SyncTransforms();
        pc.mana = pc.maxMana;
        int d0 = SL(TiengViTri.Loai.SamSetDanh), c0 = SL(TiengViTri.Loai.SamSetChay), r0 = SL(TiengViTri.Loai.SamSetRen), n0 = LightningStrike.SoLanDanh;
        pc.CastAt(2, tam);
        int danhMax = 0, chayMax = 0; bool canBang = true; bool daNghe = false;
        float hanB = Time.time + 6f;
        while (Time.time < hanB)
        {
            var gd = Giong(TiengViTri.Loai.SamSetDanh); var gc = Giong(TiengViTri.Loai.SamSetChay); var gr = Giong(TiengViTri.Loai.SamSetRen);
            danhMax = Mathf.Max(danhMax, gd.Count); chayMax = Mathf.Max(chayMax, gc.Count);
            foreach (var g in gd) if (g.nguon.spatialBlend != 0f) canBang = false;
            foreach (var g in gc) if (g.nguon.spatialBlend != 0f) canBang = false;
            foreach (var g in gr) if (g.nguon.spatialBlend != 0f) canBang = false;
            if (!daNghe && SL(TiengViTri.Loai.SamSetDanh) - d0 >= 3) { daNghe = true; yield return Nghe(1.5f, kq); continue; }
            yield return null;
        }
        int soTia = LightningStrike.SoLanDanh - n0;
        Ghi(string.Format("B. tung that: {0} tia danh xuong; yeu cau danh +{1}, chay +{2}, ren +{3}; cung luc toi da danh {4} / chay {5} (tran {6}); moi giong can bang (2D) {7}",
            soTia, SL(TiengViTri.Loai.SamSetDanh) - d0, SL(TiengViTri.Loai.SamSetChay) - c0, SL(TiengViTri.Loai.SamSetRen) - r0, danhMax, chayMax, TiengViTri.SoSamSetToiDa, canBang));
        Ghi(string.Format("   loa giua tran set: RMS {0:F4} (dinh {1:F4}), tuong quan {2:F3}", kq[0], kq[4], kq[3]));
        Kiem(soTia >= 15 && SL(TiengViTri.Loai.SamSetDanh) - d0 == soTia && SL(TiengViTri.Loai.SamSetChay) - c0 == soTia && SL(TiengViTri.Loai.SamSetRen) - r0 == 1,
             "khong dung moi tia 1 tieng danh + 1 tieng chay, moi lan tung 1 sam ren");
        Kiem(danhMax >= 2 && danhMax <= TiengViTri.SoSamSetToiDa && chayMax >= 2 && chayMax <= TiengViTri.SoSamSetToiDa, "vuot tran giong / khong chong nhau");
        Kiem(canBang, "co giong Sam set khong phat can bang");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.6f, "loa khong co tieng / gan mono");
        foreach (var b in bias) Object.Destroy(b.gameObject);
        yield return ChoIm(8f);

        // ===== C. suong mu =====
        Ghi("");
        Vector3 xa = Dat(goc + truoc * 40f) + Vector3.up;
        int che0 = TiengViTri.SoBiSuongChe;
        TiengViTri.SamSetDanh(xa); TiengViTri.SamSetChay(xa); TiengViTri.SamSetRen(xa);
        yield return new WaitForSeconds(0.2f);
        yield return Nghe(0.6f, kq);
        int giongXa = Giong(TiengViTri.Loai.SamSetDanh).Count + Giong(TiengViTri.Loai.SamSetChay).Count + Giong(TiengViTri.Loai.SamSetRen).Count;
        Ghi(string.Format("C. o 40 m (MucThay {0:F2}): bi suong che +{1}, giong {2}, RMS loa {3:F5}", TamNhin.MucThay(xa), TiengViTri.SoBiSuongChe - che0, giongXa, kq[0]));
        Kiem(TiengViTri.SoBiSuongChe - che0 == 3 && giongXa == 0 && kq[0] < 1e-4f, "tieng sam set trong suong van nghe thay");
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        int che1 = TiengViTri.SoBiSuongChe;
        TiengViTri.SamSetDanh(xa);
        yield return null;
        int giongDc = Giong(TiengViTri.Loai.SamSetDanh).Count;
        Ghi(string.Format("C2. DOI CHUNG tat suong: danh o 40 m -> giong {0}, bi suong che +{1}", giongDc, TiengViTri.SoBiSuongChe - che1));
        Kiem(giongDc == 1 && TiengViTri.SoBiSuongChe - che1 == 0, "doi chung tat suong ma van bi bo");
        TamNhin.BatTrongPhepThu = true;
        yield return ChoIm(6f);

        // ===== D. bien the =====
        Ghi("");
        var loai = new[] { TiengViTri.Loai.SamSetDanh, TiengViTri.Loai.SamSetChay, TiengViTri.Loai.SamSetRen };
        var dongD = new List<string>(); bool du = true; float caoMin = 9f, caoMax = 0f;
        foreach (var l in loai)
        {
            var clipL = new HashSet<string>(); int soDuoc = 0;
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = Dat(goc + truoc * (3f + (i % 4) * 3f) + phai * ((i % 5) - 2f) * 2f) + Vector3.up;
                int truocPhat = TiengViTri.SoLanDuocGiong;
                if (l == TiengViTri.Loai.SamSetDanh) TiengViTri.SamSetDanh(p);
                else if (l == TiengViTri.Loai.SamSetChay) TiengViTri.SamSetChay(p);
                else TiengViTri.SamSetRen(p);
                var gm = TiengViTri.GiongMoiNhat;
                if (TiengViTri.SoLanDuocGiong > truocPhat && gm != null && gm.loai == l && gm.nguon != null && gm.nguon.clip != null)
                {
                    soDuoc++; clipL.Add(gm.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                }
                yield return new WaitForSeconds(0.3f);
            }
            dongD.Add(l + " " + clipL.Count + " (" + soDuoc + " lan duoc giong)");
            if (clipL.Count != 3) du = false;
            yield return ChoIm(8f);
        }
        Ghi(string.Format("D. 24 lan moi loai: so bien the {0}; cao do {1:F3} - {2:F3}", string.Join(", ", dongD), caoMin, caoMax));
        Kiem(du, "khong du 3 bien the moi loai");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");

        // ===== E. can bang vs 3D =====
        Ghi("");
        Vector3 giua = Dat(goc + truoc * 3f) + Vector3.up, benPhai = Dat(goc + truoc * 3f + phai * 6f) + Vector3.up;
        var kqE = new Dictionary<string, float[]>();
        foreach (bool tat in new[] { false, true })
        {
            TiengViTri.TatCanBangChoPhepThu = tat;
            foreach (var cho in new[] { "giua", "phai" })
            {
                double sl = 0, sr = 0, slr = 0;
                for (int i = 0; i < 3; i++)
                {
                    TiengViTri.SamSetChay(cho == "giua" ? giua : benPhai);
                    var k1 = new float[5];
                    yield return Nghe(0.5f, k1);
                    sl += k1[1]; sr += k1[2]; slr += k1[3] * System.Math.Sqrt(k1[1] * k1[2]);
                    yield return ChoIm(3f);
                }
                kqE[(tat ? "3d_" : "cb_") + cho] = new float[] { (float)(sr / System.Math.Max(1e-12, sl)), (float)(slr / System.Math.Sqrt(System.Math.Max(1e-24, sl * sr))) };
            }
        }
        TiengViTri.TatCanBangChoPhepThu = false;
        Ghi(string.Format("E. loa (3 tieng chay xem): CAN BANG giua tuong quan {0:F2}, phai: phai/trai {1:F2} tuong quan {2:F2} | DOI CHUNG 3D spread 120: giua {3:F2}, phai: phai/trai {4:F2} tuong quan {5:F2}",
            kqE["cb_giua"][1], kqE["cb_phai"][0], kqE["cb_phai"][1], kqE["3d_giua"][1], kqE["3d_phai"][0], kqE["3d_phai"][1]));
        Kiem(kqE["cb_giua"][1] < kqE["3d_giua"][1] - 0.1f, "can bang khong stereo hon 3D o giua");
        Kiem(kqE["cb_phai"][0] > 1.5f && kqE["cb_phai"][0] > kqE["3d_phai"][0] && kqE["cb_phai"][1] < 0.6f, "can bang khong ro ben phai / bi gop mono");
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengsamset.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TamNhin.BatTrongPhepThu = false;
        TiengViTri.TatCanBangChoPhepThu = false;
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
        Debug.Log("[TiengSamSet] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
