using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 128 - TIENG TANG HINH (16/10/2026, nguoi dung: luc than chuyen TRONG SUOT, luc HIEN HINH lai, luc VONG PHEP xuat hien, va moi ke bi vong
/// phep trung mot tieng; nguoi choi khac trong suong thi khong nghe). Mau C "Hu khong" (CongCu/AmThanh/tieng_tang_hinh.py). Play Act2 that, tat
/// GameDirector + xoa quai, BAT tam nhin + tieng.
///   A. (ngoai Play) 12 file trung tung byte ban goc TangHinhThu/C_hu_khong_*.wav, STEREO, khong ForceToMono.
///   B. TUNG THAT Tang hinh (CastAt 12): 1 tieng bien trong suot, bam theo nguoi; loa co tieng, hai kenh khac nhau.
///   C. HET GIO: 3 bia trong 5 m + 1 bia o 7 m -> 1 tieng hien hinh + 1 tieng vong + 3 tieng trung (ke dau 1, ke sau x0,55), bia 7 m khong;
///      loa co tieng, khong mono.
///   D. DON DAU: dang tang hinh tung Qua cau lua -> tan tang hinh -> hien hinh + vong (moi thu MOT lan du Tat goi hai lan cung khung).
///   E. SUONG MU: tang hinh / hien hinh / vong / trung cua mot "nguoi khac" o 40 m -> loa im (vong + trung bi bo, an + hien giu giong muc 0);
///      doi chung tat suong -> co tieng.
///   F. BIEN THE: 24 lan moi loai -> du 3 bien the, cao do +-6%.
/// Ket qua: PlayTestShots/tiengtanghinh.txt.
/// </summary>
public static class ThuTiengTangHinh
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengTangHinh";
    const string Goc = "CongCu/AmThanh/TangHinhThu/C_hu_khong_";

    [MenuItem("Diablo 2.5D/128. Chay thu TIENG TANG HINH (trong suot · hien hinh · vong phep · trung, suong mu)", false, 217)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengTangHinh] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengTangHinh] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng Tang hinh (mau C 'Hu khong', 3 bien the moi tieng)");
        int dung = 0, tong = 0;
        foreach (var kv in new[] { new[] { "TangHinhAn", "an" }, new[] { "TangHinhHien", "hien" }, new[] { "TangHinhVong", "vong" }, new[] { "TangHinhTrung", "trung" } })
            for (int i = 1; i <= 3; i++)
            {
                tong++;
                string game = "Assets/Resources/AmThanh/KyNang/" + kv[0] + i + ".wav";
                string g = Goc + kv[1] + i + ".wav";
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengTangHinh] " + s); }
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
        var cam = Camera.main.transform;
        Vector3 truoc = cam.forward; truoc.y = 0; truoc.Normalize();
        Vector3 phai = Vector3.Cross(Vector3.up, truoc);
        Vector3 goc = pc.transform.position;
        float[] kq = new float[5];
        yield return ChoIm(3f);

        // ===== B. tung that =====
        Ghi("");
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(CapDo.KyTangHinh); CapDo.MoCaDuongChoPhepThu(0);
        pc.mana = pc.maxMana;
        int a0 = SL(TiengViTri.Loai.TangHinhAn);
        pc.CastAt(CapDo.KyTangHinh, goc + truoc * 3f);
        TangHinh th = null;
        float hanB = Time.time + 2f;
        while (th == null && Time.time < hanB) { th = pc.GetComponent<TangHinh>(); yield return null; }
        yield return null; yield return null;
        var gAn = Giong(TiengViTri.Loai.TangHinhAn);
        bool bamTheo = gAn.Count == 1 && gAn[0].theo == pc.transform && gAn[0].bamTheo;
        yield return Nghe(0.6f, kq);
        Ghi(string.Format("B. tung that: tang hinh {0}; yeu cau tieng trong suot +{1}, giong {2} (bam theo nguoi {3}); loa RMS {4:F4} (dinh {5:F4}), tuong quan {6:F3}",
            th != null, SL(TiengViTri.Loai.TangHinhAn) - a0, gAn.Count, bamTheo, kq[0], kq[4], kq[3]));
        Kiem(th != null && SL(TiengViTri.Loai.TangHinhAn) - a0 == 1 && gAn.Count == 1 && bamTheo, "tung Tang hinh khong ra dung 1 tieng trong suot bam theo nguoi");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa khong co tieng trong suot / gan mono");
        yield return ChoIm(4f);

        // ===== C. het gio: vong phep trung 3 bia =====
        Ghi("");
        var bias = new List<Damageable>
        {
            TaoBia("TAM_BiaVong1", Dat(goc + phai * 2f)),
            TaoBia("TAM_BiaVong2", Dat(goc - phai * 3.5f + truoc * 0.5f)),
            TaoBia("TAM_BiaVong3", Dat(goc + truoc * 4.3f)),
        };
        var biaXa = TaoBia("TAM_BiaVongXa", Dat(goc - truoc * 7f));
        Physics.SyncTransforms();
        int h0 = SL(TiengViTri.Loai.TangHinhHien), v0 = SL(TiengViTri.Loai.TangHinhVong), t0 = SL(TiengViTri.Loai.TangHinhTrung), k0 = TangHinh.SoKeTrungVong;
        float mauXa = biaXa.health;
        if (th != null) th.conLai = 0.02f;
        yield return null; yield return null;
        var gHien = Giong(TiengViTri.Loai.TangHinhHien);
        var gVong = Giong(TiengViTri.Loai.TangHinhVong);
        var gTrung = Giong(TiengViTri.Loai.TangHinhTrung);
        var heSoTrung = new List<float>(); foreach (var g in gTrung) heSoTrung.Add(g.heSo);
        heSoTrung.Sort(); heSoTrung.Reverse();
        bool trungDungCho = true;
        foreach (var g in gTrung)
        {
            bool gan = false;
            foreach (var b in bias) if ((g.viTri - (b.transform.position + Vector3.up)).sqrMagnitude < 0.01f) gan = true;
            if (!gan) trungDungCho = false;
        }
        yield return Nghe(0.8f, kq);
        Ghi(string.Format("C. het gio: yeu cau hien / vong / trung +{0} / +{1} / +{2} (ke trung vong {3}); giong {4} / {5} / {6}; he so trung {7}; trung dung cho bia {8}; bia 7 m mat mau {9}",
            SL(TiengViTri.Loai.TangHinhHien) - h0, SL(TiengViTri.Loai.TangHinhVong) - v0, SL(TiengViTri.Loai.TangHinhTrung) - t0, TangHinh.SoKeTrungVong - k0,
            gHien.Count, gVong.Count, gTrung.Count, string.Join(" / ", heSoTrung.ConvertAll(x => x.ToString("F2"))), trungDungCho, mauXa - biaXa.health));
        Ghi(string.Format("   loa: RMS {0:F4} (dinh {1:F4}), tuong quan {2:F3}", kq[0], kq[4], kq[3]));
        Kiem(SL(TiengViTri.Loai.TangHinhHien) - h0 == 1 && SL(TiengViTri.Loai.TangHinhVong) - v0 == 1 && gHien.Count == 1 && gVong.Count == 1,
             "het tang hinh khong ra dung 1 tieng hien hinh + 1 tieng vong");
        Kiem(TangHinh.SoKeTrungVong - k0 == 3 && SL(TiengViTri.Loai.TangHinhTrung) - t0 == 3 && gTrung.Count == 3 && trungDungCho, "vong trung 3 bia ma khong dung 3 tieng trung tai cho tung bia");
        Kiem(heSoTrung.Count == 3 && Mathf.Approximately(heSoTrung[0], 1f) && Mathf.Approximately(heSoTrung[2], TiengViTri.HeSoTrungKeSau), "tieng trung ke sau khong nho lai");
        Kiem(mauXa - biaXa.health == 0f, "bia ngoai vong van bi trung");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa khong co tieng vong / gan mono");
        foreach (var b in bias) Object.Destroy(b.gameObject);
        Object.Destroy(biaXa.gameObject);
        yield return ChoIm(5f);

        // ===== D. don dau tan tang hinh =====
        Ghi("");
        // (lan chay 1: kiem th2 != null SAU khi Tat -> component da huy, null cua Unity -> loi gia; ghi lai ngay luc bat)
        bool coTh2 = TangHinh.Bat(toi, 90f) != null;
        yield return null; yield return null;
        int dh0 = SL(TiengViTri.Loai.TangHinhHien), dv0 = SL(TiengViTri.Loai.TangHinhVong), dd0 = TangHinh.SoLanDonDau;
        pc.mana = pc.maxMana;
        pc.CastAt(0, goc + truoc * 8f);
        float hanD = Time.time + 2f;
        while (Time.time < hanD && TangHinh.SoLanDonDau == dd0) yield return null;
        yield return new WaitForSeconds(0.2f);
        Ghi(string.Format("D. don dau (Qua cau lua): tan tang hinh {0}; tieng hien +{1}, vong +{2}", TangHinh.SoLanDonDau - dd0,
            SL(TiengViTri.Loai.TangHinhHien) - dh0, SL(TiengViTri.Loai.TangHinhVong) - dv0));
        Kiem(coTh2 && TangHinh.SoLanDonDau - dd0 == 1 && SL(TiengViTri.Loai.TangHinhHien) - dh0 == 1 && SL(TiengViTri.Loai.TangHinhVong) - dv0 == 1,
             "don dau tan tang hinh khong ra dung 1 tieng hien hinh + 1 vong");
        yield return new WaitForSeconds(1.5f);
        yield return ChoIm(5f);

        // ===== E. suong mu: "nguoi khac" o 40 m =====
        Ghi("");
        Vector3 xa = Dat(goc + truoc * 40f);
        var khac = new GameObject("TAM_NguoiKhacXa");
        khac.transform.position = xa;
        var dk = khac.AddComponent<Damageable>(); dk.maxHealth = dk.health = 1e7f;
        var biaE = TaoBia("TAM_BiaXa", xa + phai * 2f);
        Physics.SyncTransforms();
        int e0 = TiengViTri.SoBiSuongChe, ea0 = SL(TiengViTri.Loai.TangHinhAn);
        var thXa = TangHinh.Bat(dk, 90f);
        yield return null; yield return null;
        yield return Nghe(0.4f, kq);
        float rmsAn = kq[0];
        var gAnXa = Giong(TiengViTri.Loai.TangHinhAn);
        float mucAn = gAnXa.Count > 0 ? gAnXa[0].MucDangDat : -1f;
        int et0 = SL(TiengViTri.Loai.TangHinhTrung);
        thXa.conLai = 0.02f;
        yield return null; yield return null;
        var gHienXa = Giong(TiengViTri.Loai.TangHinhHien);
        int vongTrungGiong = Giong(TiengViTri.Loai.TangHinhVong).Count + Giong(TiengViTri.Loai.TangHinhTrung).Count;
        yield return Nghe(0.6f, kq);
        Ghi(string.Format("E. o 40 m (MucThay {0:F2}): trong suot +{1} (muc {2:F3}, loa RMS {3:F5}); hien hinh giong {4} muc {5:F3}; vong + trung (+{6} trung) giong {7}, bi suong che +{8}; loa RMS {9:F5}",
            TamNhin.MucThay(xa), SL(TiengViTri.Loai.TangHinhAn) - ea0, mucAn, rmsAn, gHienXa.Count, gHienXa.Count > 0 ? gHienXa[0].MucDangDat : -1f,
            SL(TiengViTri.Loai.TangHinhTrung) - et0, vongTrungGiong, TiengViTri.SoBiSuongChe - e0, kq[0]));
        Kiem(SL(TiengViTri.Loai.TangHinhAn) - ea0 == 1 && mucAn == 0f && rmsAn < 1e-4f, "tang hinh trong suong van nghe thay");
        Kiem(SL(TiengViTri.Loai.TangHinhTrung) - et0 == 1 && vongTrungGiong == 0 && TiengViTri.SoBiSuongChe - e0 == 2 && kq[0] < 1e-4f
             && (gHienXa.Count == 0 || gHienXa[0].MucDangDat == 0f), "hien hinh / vong / trung trong suong van nghe thay");
        Object.Destroy(biaE.gameObject);
        yield return ChoIm(3f);
        // doi chung: tat suong
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        var biaE2 = TaoBia("TAM_BiaXa2", xa + phai * 2f);
        Physics.SyncTransforms();
        int e1 = TiengViTri.SoBiSuongChe;
        var thXa2 = TangHinh.Bat(dk, 90f);
        yield return null; yield return null;
        thXa2.conLai = 0.02f;
        yield return null; yield return null;
        int giongDc = Giong(TiengViTri.Loai.TangHinhVong).Count + Giong(TiengViTri.Loai.TangHinhTrung).Count;
        Ghi(string.Format("E2. DOI CHUNG tat suong: vong + trung giong {0}, bi suong che +{1}", giongDc, TiengViTri.SoBiSuongChe - e1));
        Kiem(giongDc == 2 && TiengViTri.SoBiSuongChe - e1 == 0, "doi chung tat suong ma van bi bo");
        TamNhin.BatTrongPhepThu = true;
        Object.Destroy(biaE2.gameObject); Object.Destroy(khac);
        yield return ChoIm(5f);

        // ===== F. bien the + cao do =====
        Ghi("");
        var loai = new[] { TiengViTri.Loai.TangHinhAn, TiengViTri.Loai.TangHinhHien, TiengViTri.Loai.TangHinhVong, TiengViTri.Loai.TangHinhTrung };
        var dongF = new List<string>(); bool du = true;
        float caoMin = 9f, caoMax = 0f;
        var vat = new GameObject("TAM_VatGia").transform;
        foreach (var l in loai)
        {
            var clipL = new HashSet<string>(); int soDuoc = 0;
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = Dat(goc + truoc * (3f + (i % 4) * 3f) + phai * ((i % 5) - 2f) * 2f);
                vat.position = p;
                int truocPhat = TiengViTri.SoLanDuocGiong;
                switch (l)
                {
                    case TiengViTri.Loai.TangHinhAn: TiengViTri.TangHinhAn(vat); break;
                    case TiengViTri.Loai.TangHinhHien: TiengViTri.TangHinhHien(vat); break;
                    case TiengViTri.Loai.TangHinhVong: TiengViTri.TangHinhVong(p); break;
                    case TiengViTri.Loai.TangHinhTrung: TiengViTri.TangHinhTrung(p); break;
                }
                var gm = TiengViTri.GiongMoiNhat;
                if (TiengViTri.SoLanDuocGiong > truocPhat && gm != null && gm.loai == l && gm.nguon != null && gm.nguon.clip != null)
                {
                    soDuoc++; clipL.Add(gm.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                }
                yield return new WaitForSeconds(0.3f);
            }
            dongF.Add(l + " " + clipL.Count + " (" + soDuoc + " lan duoc giong)");
            if (clipL.Count != 3) du = false;
            yield return ChoIm(5f);
        }
        Object.Destroy(vat.gameObject);
        Ghi(string.Format("F. 24 lan moi loai: so bien the {0}; cao do {1:F3} - {2:F3}", string.Join(", ", dongF), caoMin, caoMax));
        Kiem(du, "khong du 3 bien the moi loai");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengtanghinh.txt", bao.ToString());
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
        Debug.Log("[TiengTangHinh] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
