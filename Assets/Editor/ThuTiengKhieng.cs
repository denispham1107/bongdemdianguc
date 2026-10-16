using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 134 - TIENG KHIEN (16/10/2026, nguoi dung: luc BAT khien, khi khien NO TUNG, va khi don danh cua ke thu TRUNG khien nghe tieng doi ra "BOONG
/// BOONG"; nguoi choi khac trong suong thi khong nghe). Ban 2 mau A "Thep duc" (CongCu/AmThanh/tieng_khieng_2.py - ban 1 bi che "gia tao, don gian").
/// Play Act2 that, tat GameDirector + xoa quai, BAT tam nhin + tieng.
///   A. (ngoai Play) 9 file trung tung byte ban goc, STEREO, khong ForceToMono.
///   B. TUNG THAT Khien (CastAt 5): 1 tieng bat, bam theo nguoi, can bang (2D); loa co tieng, khong mono.
///   C. DON TRUNG: 3 don cach 0,4 s -> 3 tieng "boong" bam theo nguoi; 2 don CUNG khung -> 1 tieng; don thu hai sau 0,1 s -> he so x0,6; sat thuong
///      RI (Damageable.LaSatThuongRi - nhip chay, loc cuon) tru mau khien nhung KHONG keu. Loa co tieng.
///   D. VO: don lon pha khien -> 1 tieng no tai cho vom (khong chu) + 1 tieng trung.
///   E. BAN SAO (mau do may khac quyet): Bat -> tieng bat; DatMauTuMang tut 75 -> 1 tieng trung, tut 1,5 (nhip ri) -> khong; VoTuMang -> tieng vo.
///   F. SUONG MU: khien cua "nguoi khac" o tam nhin + 10 m: bat / trung giu giong muc 0, vo bi bo, loa im; doi chung tat suong -> co giong vo.
///   G. BIEN THE: 24 lan moi loai -> du 3 bien the, cao do +-6%.
/// Ket qua: PlayTestShots/tiengkhieng.txt.
/// </summary>
public static class ThuTiengKhieng
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengKhieng";

    [MenuItem("Diablo 2.5D/134. Chay thu TIENG KHIEN (bat · trung boong boong · vo, suong mu)", false, 223)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengKhieng] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengKhieng] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng Khien (ban 2 mau A Thep duc), 3 bien the moi tieng");
        int dung = 0, tong = 0;
        var ds = new[] {
            new[] { "KhiengBat", "CongCu/AmThanh/KhiengThu2/A_thep_duc_bat" }, new[] { "KhiengTrung", "CongCu/AmThanh/KhiengThu2/A_thep_duc_trung" },
            new[] { "KhiengVo", "CongCu/AmThanh/KhiengThu2/A_thep_duc_vo" } };
        foreach (var kv in ds)
            for (int i = 1; i <= 3; i++)
            {
                tong++;
                string game = "Assets/Resources/AmThanh/KyNang/" + kv[0] + i + ".wav";
                string g = kv[1] + i + ".wav";
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengKhieng] " + s); }
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
        var cam = Camera.main.transform;
        Vector3 truoc = cam.forward; truoc.y = 0; truoc.Normalize();
        Vector3 phai = Vector3.Cross(Vector3.up, truoc);
        float[] kq = new float[5];
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(5);
        Vector3 goc = pc.transform.position;
        yield return ChoIm(3f);

        // ===== B. tung that =====
        Ghi("");
        pc.mana = pc.maxMana;
        int b0 = SL(TiengViTri.Loai.KhiengBat);
        pc.CastAt(5, goc + truoc * 3f);
        Khieng k = null;
        float hanB = Time.time + 2f;
        while ((k == null || !k.DangBat) && Time.time < hanB) { k = pc.GetComponent<Khieng>(); yield return null; }
        yield return null;
        var gBat = Giong(TiengViTri.Loai.KhiengBat);
        bool batTheo = gBat.Count == 1 && gBat[0].theo == pc.transform && gBat[0].bamTheo && gBat[0].nguon.spatialBlend == 0f;
        yield return Nghe(0.8f, kq);
        Ghi(string.Format("B. tung that Khien: dang bat {0} (mau {1:F0}); tieng bat +{2}, giong {3} (bam theo nguoi + can bang 2D {4}); loa RMS {5:F4} (dinh {6:F4}), tuong quan {7:F3}",
            k != null && k.DangBat, k != null ? k.mau : 0f, SL(TiengViTri.Loai.KhiengBat) - b0, gBat.Count, batTheo, kq[0], kq[4], kq[3]));
        Kiem(k != null && k.DangBat && SL(TiengViTri.Loai.KhiengBat) - b0 == 1 && batTheo, "tung Khien khong ra dung 1 tieng bat bam theo nguoi, can bang");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa khong co tieng bat khien / gan mono");
        yield return ChoIm(4f);
        if (k == null) { Ket(); yield break; }
        k.mauToiDa = k.mau = 1e6f;      // khong vo giua muc C

        // ===== C. don trung =====
        Ghi("");
        int t0 = SL(TiengViTri.Loai.KhiengTrung);
        bool trungTheo = true;
        var rmsTrung = new List<float>(); var tqTrung = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            toi.TakeDamage(30f, DamageType.Physical, Vector3.zero);
            yield return null;
            foreach (var g in Giong(TiengViTri.Loai.KhiengTrung)) if (g.theo != pc.transform || !g.bamTheo) trungTheo = false;
            yield return Nghe(0.4f, kq);
            rmsTrung.Add(kq[0]); tqTrung.Add(kq[0].ToString("F4") + "/" + kq[3].ToString("F2"));
        }
        int c3 = SL(TiengViTri.Loai.KhiengTrung) - t0;
        bool nguoiNguyen = toi.health == toi.maxHealth;
        yield return ChoIm(4f);
        // hai don CUNG khung
        int t1 = SL(TiengViTri.Loai.KhiengTrung);
        toi.TakeDamage(30f, DamageType.Physical, Vector3.zero); toi.TakeDamage(30f, DamageType.Fire, Vector3.zero);
        int cungKhung = SL(TiengViTri.Loai.KhiengTrung) - t1;
        yield return new WaitForSeconds(0.1f);
        int truocPhat = TiengViTri.SoLanDuocGiong;
        toi.TakeDamage(30f, DamageType.Physical, Vector3.zero);
        var gSau = GiongMoi(truocPhat, TiengViTri.Loai.KhiengTrung);
        float heSoSau = gSau != null ? gSau.heSo : -1f;
        yield return ChoIm(4f);
        // sat thuong ri
        yield return new WaitForSeconds(0.2f);
        int t2 = SL(TiengViTri.Loai.KhiengTrung); float mk0 = k.mau;
        Damageable.LaSatThuongRi = true;
        toi.TakeDamage(6f, DamageType.Fire, Vector3.zero);
        Damageable.LaSatThuongRi = false;
        int ri = SL(TiengViTri.Loai.KhiengTrung) - t2;
        Ghi(string.Format("C. 3 don cach 0,4 s: tieng trung +{0} (bam theo nguoi {1}), loa RMS/tuong quan {2}; nguoi khong mat mau {3}; 2 don cung khung +{4}; don sau 0,1 s he so {5:F2}; sat thuong ri +{6} (khien mat {7:F0} mau)",
            c3, trungTheo, string.Join(" ", tqTrung), nguoiNguyen, cungKhung, heSoSau, ri, mk0 - k.mau));
        Kiem(c3 == 3 && trungTheo, "3 don trung khien khong ra 3 tieng bam theo nguoi");
        Kiem(nguoiNguyen, "khien khong an don (nguoi mat mau)");
        Kiem(cungKhung == 1, "2 don cung khung keu 2 lan");
        Kiem(Mathf.Abs(heSoSau - TiengViTri.HeSoSetKeSau) < 0.01f, "don ngay sau don truoc khong nho lai");
        Kiem(ri == 0 && mk0 - k.mau > 5f, "sat thuong ri van keu boong (hoac khong tru khien)");
        bool coLoa = true; foreach (var v in rmsTrung) if (v < 1e-3f) coLoa = false;
        Kiem(coLoa, "loa khong co tieng don trung khien");
        yield return ChoIm(4f);

        // ===== D. vo =====
        Ghi("");
        k.mauToiDa = 150f; k.mau = 40f;
        int v0 = SL(TiengViTri.Loai.KhiengVo), tv0 = SL(TiengViTri.Loai.KhiengTrung);
        Vector3 tamVom = pc.transform.position + Vector3.up * k.banKinh * 0.55f;
        toi.TakeDamage(200f, DamageType.Physical, Vector3.zero);
        yield return null;
        var gVo = Giong(TiengViTri.Loai.KhiengVo);
        bool voDungCho = gVo.Count == 1 && gVo[0].theo == null && Vector3.Distance(gVo[0].viTri, tamVom) < 0.2f;
        yield return Nghe(1.0f, kq);
        Ghi(string.Format("D. don 200 pha khien (con 40): khien bat {0}, nguoi mat mau {1:F0}; tieng vo +{2} (tai cho vom, khong chu {3}), trung +{4}; loa RMS {5:F4} tuong quan {6:F3}",
            k.DangBat, toi.maxHealth - toi.health, SL(TiengViTri.Loai.KhiengVo) - v0, voDungCho, SL(TiengViTri.Loai.KhiengTrung) - tv0, kq[0], kq[3]));
        Kiem(!k.DangBat && SL(TiengViTri.Loai.KhiengVo) - v0 == 1 && voDungCho && SL(TiengViTri.Loai.KhiengTrung) - tv0 == 1, "khien vo khong ra dung 1 tieng vo tai cho vom");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa khong co tieng vo / gan mono");
        yield return ChoIm(5f);

        // ===== E. ban sao =====
        Ghi("");
        {
            var bs = TaoNguoi("TAM_BanSao", Dat(goc + phai * 5f), true);
            int eb = SL(TiengViTri.Loai.KhiengBat), et = SL(TiengViTri.Loai.KhiengTrung), ev = SL(TiengViTri.Loai.KhiengVo);
            var kb = Khieng.Bat(bs.gameObject, 150f, 3f); bs.khieng = kb;
            yield return new WaitForSeconds(0.5f);
            kb.DatMauTuMang(0.5f);                 // tut 75
            int sauTut = SL(TiengViTri.Loai.KhiengTrung) - et;
            yield return new WaitForSeconds(0.5f);
            kb.DatMauTuMang(0.49f);                // tut 1,5 - nhip ri
            int sauRi = SL(TiengViTri.Loai.KhiengTrung) - et;
            yield return new WaitForSeconds(0.3f);
            kb.VoTuMang();
            Ghi(string.Format("E. ban sao: bat +{0}; tut 75 mau -> trung +{1}; tut 1,5 -> trung tong +{2}; VoTuMang -> vo +{3}", SL(TiengViTri.Loai.KhiengBat) - eb, sauTut, sauRi,
                SL(TiengViTri.Loai.KhiengVo) - ev));
            Kiem(SL(TiengViTri.Loai.KhiengBat) - eb == 1 && sauTut == 1 && sauRi == 1 && SL(TiengViTri.Loai.KhiengVo) - ev == 1, "ban sao khien khong keu dung bat / trung / vo");
            Object.Destroy(bs.gameObject);
            yield return ChoIm(5f);
        }

        // ===== F. suong mu =====
        Ghi("");
        {
            Vector3 xa = Dat(goc + truoc * (TamNhin.BanKinh + 10f));
            var kh = TaoNguoi("TAM_NguoiKhacXa", xa, false);
            int e0 = TiengViTri.SoBiSuongChe, fv0 = SL(TiengViTri.Loai.KhiengVo), ft0 = SL(TiengViTri.Loai.KhiengTrung);
            var kx = Khieng.Bat(kh.gameObject, 150f, 3f); kh.khieng = kx;
            yield return null;
            kh.TakeDamage(30f, DamageType.Physical, Vector3.zero);
            yield return null; yield return null;
            float mucMax = 0f; int giongCo = 0;
            foreach (var l in new[] { TiengViTri.Loai.KhiengBat, TiengViTri.Loai.KhiengTrung })
                foreach (var g in Giong(l)) { giongCo++; mucMax = Mathf.Max(mucMax, g.MucDangDat); }
            yield return Nghe(0.5f, kq);
            float rmsXa = kq[0];
            kh.TakeDamage(500f, DamageType.Physical, Vector3.zero);
            yield return null;
            int giongVo = Giong(TiengViTri.Loai.KhiengVo).Count;
            Ghi(string.Format("F. o {0:F0} m (MucThay {1:F2}): bat + trung (+{2}) giong {3} muc lon nhat {4:F3}, loa RMS {5:F5}; vo +{6} giong {7}, bi suong che +{8}",
                TamNhin.BanKinh + 10f, TamNhin.MucThay(xa), SL(TiengViTri.Loai.KhiengTrung) - ft0, giongCo, mucMax, rmsXa, SL(TiengViTri.Loai.KhiengVo) - fv0, giongVo, TiengViTri.SoBiSuongChe - e0));
            Kiem(giongCo == 2 && mucMax == 0f && rmsXa < 1e-4f && SL(TiengViTri.Loai.KhiengVo) - fv0 == 1 && giongVo == 0 && TiengViTri.SoBiSuongChe - e0 == 1,
                 "khien trong suong van nghe thay");
            yield return ChoIm(3f);
            TamNhin.BatTrongPhepThu = false;
            yield return null; yield return null;
            int e1 = TiengViTri.SoBiSuongChe;
            var kx2 = Khieng.Bat(kh.gameObject, 150f, 3f); kh.khieng = kx2;
            kh.TakeDamage(500f, DamageType.Physical, Vector3.zero);
            yield return null;
            int giongDc = Giong(TiengViTri.Loai.KhiengVo).Count;
            Ghi(string.Format("F2. DOI CHUNG tat suong: vo o {0:F0} m -> giong {1}, bi suong che +{2}", TamNhin.BanKinh + 10f, giongDc, TiengViTri.SoBiSuongChe - e1));
            Kiem(giongDc == 1 && TiengViTri.SoBiSuongChe - e1 == 0, "doi chung tat suong ma van bi bo");
            TamNhin.BatTrongPhepThu = true;
            Object.Destroy(kh.gameObject);
            yield return ChoIm(5f);
        }

        // ===== G. bien the + cao do =====
        Ghi("");
        var loai = new[] { TiengViTri.Loai.KhiengBat, TiengViTri.Loai.KhiengTrung, TiengViTri.Loai.KhiengVo };
        var dongG = new List<string>(); bool du = true;
        float caoMin = 9f, caoMax = 0f;
        var vat = new GameObject("TAM_VatGia").transform;
        foreach (var l in loai)
        {
            var clipL = new HashSet<string>(); int soDuoc = 0;
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = Dat(goc + truoc * (3f + (i % 4) * 3f) + phai * ((i % 5) - 2f) * 2f);
                vat.position = p;
                int tp = TiengViTri.SoLanDuocGiong;
                if (l == TiengViTri.Loai.KhiengBat) TiengViTri.KhiengBat(vat);
                else if (l == TiengViTri.Loai.KhiengTrung) TiengViTri.KhiengTrung(vat);
                else TiengViTri.KhiengVo(p + Vector3.up);
                var gm = GiongMoi(tp, l);
                if (gm != null && gm.nguon != null && gm.nguon.clip != null)
                {
                    soDuoc++; clipL.Add(gm.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                }
                yield return new WaitForSeconds(0.3f);
            }
            dongG.Add(l + " " + clipL.Count + " (" + soDuoc + " lan duoc giong)");
            if (clipL.Count != 3) du = false;
            yield return ChoIm(6f);
        }
        Object.Destroy(vat.gameObject);
        Ghi(string.Format("G. 24 lan moi loai: so bien the {0}; cao do {1:F3} - {2:F3}", string.Join(", ", dongG), caoMin, caoMax));
        Kiem(du, "khong du 3 bien the moi loai");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengkhieng.txt", bao.ToString());
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
        Debug.Log("[TiengKhieng] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
