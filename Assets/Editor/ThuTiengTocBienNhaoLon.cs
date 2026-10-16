using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 133 - TIENG TOC BIEN + NHAO LON (16/10/2026, nguoi dung: Toc bien - luc BIEN MAT va luc XUAT HIEN o cho moi; Nhao lon - "trong suot thoi gian
/// cuon nguoi nhao lon, tieng dong chan that"; nguoi choi khac trong suong thi khong nghe). Mau Toc bien C "Khoi no" (CongCu/AmThanh/tieng_toc_bien.py),
/// Nhao lon A "Dat soi" (tieng_nhao_lon.py). Play Act2 that, tat GameDirector + xoa quai, BAT tam nhin + tieng.
///   A. (ngoai Play) 15 file trung tung byte ban goc, STEREO, khong ForceToMono.
///   B. TUNG THAT Toc bien (CastAt 15) 10 m: 1 tieng bien mat TAI CHO CU (khong chu) + 1 tieng hien ra bam theo nguoi o cho moi; can bang (2D);
///      loa co tieng, khong mono.
///   C. TUNG THAT Nhao lon 8 m va 2 m: dung 1 VUT luc bat dau + 1 LAN + 1 DAP, bam theo nguoi; LAN / DAP keu DUNG LUC goc lat qua 80 / 300 do
///      (so voi thoi diem tinh tu NhaoLon.GocLat - lech <= 1 khung); lan 2 m som hon lan 8 m. Loa co tieng, khong mono.
///   D. BI HUY GIUA VONG (truoc khi vai cham dat): chi VUT, khong LAN / DAP.
///   E. BAN SAO (laChu false - may khac chi ve hinh): van du VUT + LAN + DAP.
///   F. SUONG MU: "nguoi khac" o tam nhin + 10 m: toc bien bien mat bi bo, hien ra / nhao lon giu giong muc 0, loa im; doi chung tat suong -> co giong.
///   G. BIEN THE: 24 lan moi loai -> du 3 bien the, cao do +-6%.
/// Ket qua: PlayTestShots/tiengtocbiennhaolon.txt.
/// </summary>
public static class ThuTiengTocBienNhaoLon
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengTocBienNhaoLon";

    [MenuItem("Diablo 2.5D/133. Chay thu TIENG TOC BIEN + NHAO LON (bien mat · hien ra · vut · lan · dap, suong mu)", false, 222)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengTocBienNhaoLon] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengTocBienNhaoLon] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng Toc bien (mau C 'Khoi no') + Nhao lon (mau A 'Dat soi'), 3 bien the moi tieng");
        int dung = 0, tong = 0;
        var ds = new[] {
            new[] { "TocBienMat", "CongCu/AmThanh/TocBienThu/C_khoi_no_bien_mat" }, new[] { "TocBienHien", "CongCu/AmThanh/TocBienThu/C_khoi_no_hien_ra" },
            new[] { "NhaoLonVut", "CongCu/AmThanh/NhaoLonThu/A_dat_soi_vut" }, new[] { "NhaoLonLan", "CongCu/AmThanh/NhaoLonThu/A_dat_soi_lan" },
            new[] { "NhaoLonDap", "CongCu/AmThanh/NhaoLonThu/A_dat_soi_dap" } };
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengTocBienNhaoLon] " + s); }
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

    /// <summary>Ti le thoi gian u ma NhaoLon.GocLat(u) dat goc (tinh lai doc lap bang quet - khong dung bien cua tieng).</summary>
    static float UTaiGoc(float goc)
    {
        for (int i = 0; i <= 20000; i++) { float u = i / 20000f; if (NhaoLon.GocLat(u) >= goc) return u; }
        return 1f;
    }

    static readonly TiengViTri.Loai[] LoaiNL = { TiengViTri.Loai.NhaoLonVut, TiengViTri.Loai.NhaoLonLan, TiengViTri.Loai.NhaoLonDap };

    /// <summary>Dem tieng hien co - goi TRUOC khi tung (VUT keu ngay luc NhaoLon.Bat).</summary>
    static int[] DemNL() { var d = new int[3]; for (int k = 0; k < 3; k++) d[k] = SL(LoaiNL[k]); return d; }

    /// <summary>Theo doi mot vong nhao lon: thoi diem (giay tu luc NhaoLon.Bat - SoLanBat tang) cua VUT / LAN / DAP, so tieng moi loai, bam theo
    /// nguoi khong. Do moi khung (coroutine chay truoc LateUpdate -> tieng keu trong LateUpdate thay o khung sau: tre <= 1 khung).</summary>
    static IEnumerator TheoDoiVong(Transform nguoi, int[] dau, int b0, float toiDa, float[] tg, int[] so, bool[] bam, float[] khungLon)
    {
        for (int k = 0; k < 3; k++) { tg[k] = -1f; so[k] = 0; bam[k] = false; }
        float lucBat = -1f; khungLon[0] = 0f;
        float han = Time.time + toiDa;
        while (Time.time < han)
        {
            if (lucBat < 0f && NhaoLon.SoLanBat > b0) lucBat = Time.time;
            khungLon[0] = Mathf.Max(khungLon[0], Time.deltaTime);
            for (int k = 0; k < 3; k++)
            {
                int moi = SL(LoaiNL[k]) - dau[k];
                if (moi > so[k])
                {
                    if (so[k] == 0) tg[k] = lucBat < 0f ? 0f : Time.time - lucBat;
                    so[k] = moi;
                    foreach (var g in Giong(LoaiNL[k])) if (g.theo == nguoi && g.bamTheo) bam[k] = true;
                }
            }
            yield return null;
        }
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
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(CapDo.KyTocBien);
        yield return ChoIm(3f);

        // ===== B. Toc bien that =====
        Ghi("");
        Vector3 goc = pc.transform.position;
        pc.mana = pc.maxMana;
        int m0 = SL(TiengViTri.Loai.TocBienMat), h0 = SL(TiengViTri.Loai.TocBienHien), n0 = TocBien.SoLanNhay;
        pc.CastAt(CapDo.KyTocBien, goc + truoc * 10f);
        float hanB = Time.time + 2f;
        while (TocBien.SoLanNhay == n0 && Time.time < hanB) yield return null;
        yield return null;
        var gMat = Giong(TiengViTri.Loai.TocBienMat);
        var gHien = Giong(TiengViTri.Loai.TocBienHien);
        bool matChoCu = gMat.Count == 1 && gMat[0].theo == null && Vector3.Distance(gMat[0].viTri, goc + Vector3.up) < 0.2f;
        bool hienTheo = gHien.Count == 1 && gHien[0].theo == pc.transform && gHien[0].bamTheo;
        bool canBang = true;
        foreach (var g in gMat) if (g.nguon.spatialBlend != 0f) canBang = false;
        foreach (var g in gHien) if (g.nguon.spatialBlend != 0f) canBang = false;
        float quang = Vector3.Distance(goc, pc.transform.position);
        yield return Nghe(0.6f, kq);
        Ghi(string.Format("B. tung that Toc bien: nhay {0} ({1:F1} m); bien mat +{2} (o cho cu {3}), hien ra +{4} (bam theo nguoi {5}); can bang 2D {6}; loa RMS {7:F4} (dinh {8:F4}), tuong quan {9:F3}",
            TocBien.SoLanNhay - n0, quang, SL(TiengViTri.Loai.TocBienMat) - m0, matChoCu, SL(TiengViTri.Loai.TocBienHien) - h0, hienTheo, canBang, kq[0], kq[4], kq[3]));
        Kiem(TocBien.SoLanNhay - n0 == 1 && SL(TiengViTri.Loai.TocBienMat) - m0 == 1 && SL(TiengViTri.Loai.TocBienHien) - h0 == 1 && matChoCu && hienTheo,
             "Toc bien khong ra dung 1 tieng bien mat o cho cu + 1 tieng hien ra bam theo nguoi");
        Kiem(canBang, "tieng Toc bien khong phat can bang");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa khong co tieng Toc bien / gan mono");
        yield return ChoIm(4f);

        // ===== C. Nhao lon that 8 m va 2 m =====
        Ghi("");
        float uLan = UTaiGoc(NhaoLon.GocTiengLan), uDap = UTaiGoc(NhaoLon.GocTiengDap);
        var tg = new float[3]; var so = new int[3]; var bam = new bool[3];
        float tgLan8 = 0f;
        foreach (float q in new[] { 8f, 2f })
        {
            goc = pc.transform.position;
            float tongTg = NhaoLon.ThoiGianLan(q);
            int b0 = NhaoLon.SoLanBat; var dau = DemNL(); var khungLon = new float[1];
            pc.CastAt(CapDo.KyNhaoLon, goc + truoc * q);
            var nghe = new float[5];
            var go = new GameObject("TAM_Nghe"); go.AddComponent<ChayThuMang>().batDau = Nghe(tongTg + 0.2f, nghe);
            yield return TheoDoiVong(pc.transform, dau, b0, tongTg + 0.5f, tg, so, bam, khungLon);
            Object.Destroy(go);
            float kyLan = uLan * tongTg, kyDap = uDap * tongTg;
            float khung = khungLon[0] * 2f;      // tre <= 1 khung + sai so cong don deltaTime
            Ghi(string.Format("C. nhao lon {0} m (vong {1:F2} s, bat {2}): VUT/LAN/DAP so {3}/{4}/{5}, luc {6:F3}/{7:F3}/{8:F3} s (mong 0 / {9:F3} / {10:F3}); bam theo nguoi {11}/{12}/{13}; loa RMS {14:F4} tuong quan {15:F3}",
                q, tongTg, NhaoLon.SoLanBat - b0, so[0], so[1], so[2], tg[0], tg[1], tg[2], kyLan, kyDap, bam[0], bam[1], bam[2], nghe[0], nghe[3]));
            Kiem(NhaoLon.SoLanBat - b0 == 1 && so[0] == 1 && so[1] == 1 && so[2] == 1 && bam[0] && bam[1] && bam[2], "nhao lon " + q + " m khong ra dung VUT + LAN + DAP bam theo nguoi");
            Kiem(tg[0] == 0f && Mathf.Abs(tg[1] - kyLan) <= khung + 0.02f && Mathf.Abs(tg[2] - kyDap) <= khung + 0.02f, "nhao lon " + q + " m: LAN / DAP keu sai luc so voi goc lat");
            Kiem(nghe[0] > 1e-3f && nghe[3] < 0.95f, "loa khong co tieng nhao lon / gan mono");
            if (q == 8f) tgLan8 = tg[2];
            else Kiem(tg[2] < tgLan8 - 0.1f, "lan 2 m ma tieng DAP khong som hon lan 8 m");
            yield return ChoIm(3f);
            yield return new WaitForSeconds(Mathf.Max(0f, NhaoLon.HoiChieu - 1f));
        }

        // ===== D. huy giua vong =====
        Ghi("");
        {
            goc = pc.transform.position;
            int v0 = SL(TiengViTri.Loai.NhaoLonVut), l0 = SL(TiengViTri.Loai.NhaoLonLan), d0 = SL(TiengViTri.Loai.NhaoLonDap);
            pc.CastAt(CapDo.KyNhaoLon, goc - truoc * 8f);
            yield return new WaitForSeconds(0.1f);
            var nl = pc.GetComponent<NhaoLon>();
            float gocLuc = nl != null ? nl.GocHienTai : -1f;
            if (nl != null) nl.Huy(true);
            yield return new WaitForSeconds(1.0f);
            Ghi(string.Format("D. huy o goc {0:F0} do (truoc {1}): VUT +{2}, LAN +{3}, DAP +{4}", gocLuc, NhaoLon.GocTiengLan, SL(TiengViTri.Loai.NhaoLonVut) - v0,
                SL(TiengViTri.Loai.NhaoLonLan) - l0, SL(TiengViTri.Loai.NhaoLonDap) - d0));
            Kiem(gocLuc >= 0f && gocLuc < NhaoLon.GocTiengLan && SL(TiengViTri.Loai.NhaoLonVut) - v0 == 1 && SL(TiengViTri.Loai.NhaoLonLan) - l0 == 0
                 && SL(TiengViTri.Loai.NhaoLonDap) - d0 == 0, "huy giua vong van keu LAN / DAP");
            yield return ChoIm(3f);
            yield return new WaitForSeconds(NhaoLon.HoiChieu);
        }

        // ===== E. ban sao (chi hinh) =====
        Ghi("");
        {
            var tgE = new float[3]; var soE = new int[3]; var bamE = new bool[3]; var kE = new float[1];
            int bE = NhaoLon.SoLanBat; var dauE = DemNL();
            NhaoLon.Bat(pc, pc.transform.position + phai * 8f, false);
            yield return TheoDoiVong(pc.transform, dauE, bE, NhaoLon.ThoiGianLan(8f) + 0.4f, tgE, soE, bamE, kE);
            Ghi(string.Format("E. ban sao (laChu false): VUT/LAN/DAP so {0}/{1}/{2}, luc {3:F3}/{4:F3}/{5:F3} s", soE[0], soE[1], soE[2], tgE[0], tgE[1], tgE[2]));
            Kiem(soE[0] == 1 && soE[1] == 1 && soE[2] == 1, "ban sao nhao lon khong du VUT + LAN + DAP");
            yield return ChoIm(3f);
        }

        // ===== F. suong mu =====
        Ghi("");
        goc = pc.transform.position;
        Vector3 xa = Dat(goc + truoc * (TamNhin.BanKinh + 10f));
        var khac = new GameObject("TAM_NguoiKhacXa").transform; khac.position = xa;
        int e0 = TiengViTri.SoBiSuongChe, em0 = SL(TiengViTri.Loai.TocBienMat);
        TiengViTri.TocBienMat(xa - phai * 6f);
        TiengViTri.TocBienHien(khac, xa);
        TiengViTri.NhaoLonTieng(khac, TiengViTri.Loai.NhaoLonVut);
        TiengViTri.NhaoLonTieng(khac, TiengViTri.Loai.NhaoLonDap);
        yield return null; yield return null;
        int giongMat = Giong(TiengViTri.Loai.TocBienMat).Count;
        float mucMax = 0f; int giongCo = 0;
        foreach (var l in new[] { TiengViTri.Loai.TocBienHien, TiengViTri.Loai.NhaoLonVut, TiengViTri.Loai.NhaoLonDap })
            foreach (var g in Giong(l)) { giongCo++; mucMax = Mathf.Max(mucMax, g.MucDangDat); }
        yield return Nghe(0.6f, kq);
        Ghi(string.Format("F. o {0:F0} m (MucThay {1:F2}): bien mat bi suong che +{2} (giong {3}); hien ra / vut / dap giong {4} muc lon nhat {5:F3}; loa RMS {6:F5}",
            TamNhin.BanKinh + 10f, TamNhin.MucThay(xa), TiengViTri.SoBiSuongChe - e0, giongMat, giongCo, mucMax, kq[0]));
        Kiem(SL(TiengViTri.Loai.TocBienMat) - em0 == 1 && TiengViTri.SoBiSuongChe - e0 == 1 && giongMat == 0 && giongCo == 3 && mucMax == 0f && kq[0] < 1e-4f,
             "Toc bien / Nhao lon trong suong van nghe thay");
        yield return ChoIm(3f);
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        int e1 = TiengViTri.SoBiSuongChe;
        TiengViTri.TocBienMat(xa - phai * 6f);
        yield return null;
        int giongDc = Giong(TiengViTri.Loai.TocBienMat).Count;
        Ghi(string.Format("F2. DOI CHUNG tat suong: bien mat o {0:F0} m -> giong {1}, bi suong che +{2}", TamNhin.BanKinh + 10f, giongDc, TiengViTri.SoBiSuongChe - e1));
        Kiem(giongDc == 1 && TiengViTri.SoBiSuongChe - e1 == 0, "doi chung tat suong ma van bi bo");
        TamNhin.BatTrongPhepThu = true;
        Object.Destroy(khac.gameObject);
        yield return ChoIm(4f);

        // ===== G. bien the + cao do =====
        Ghi("");
        var loai = new[] { TiengViTri.Loai.TocBienMat, TiengViTri.Loai.TocBienHien, TiengViTri.Loai.NhaoLonVut, TiengViTri.Loai.NhaoLonLan, TiengViTri.Loai.NhaoLonDap };
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
                int truocPhat = TiengViTri.SoLanDuocGiong;
                if (l == TiengViTri.Loai.TocBienMat) TiengViTri.TocBienMat(p);
                else if (l == TiengViTri.Loai.TocBienHien) TiengViTri.TocBienHien(vat, p);
                else TiengViTri.NhaoLonTieng(vat, l);
                var gm = TiengViTri.GiongMoiNhat;
                if (TiengViTri.SoLanDuocGiong > truocPhat && gm != null && gm.loai == l && gm.nguon != null && gm.nguon.clip != null)
                {
                    soDuoc++; clipL.Add(gm.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                }
                yield return new WaitForSeconds(0.25f);
            }
            dongG.Add(l + " " + clipL.Count + " (" + soDuoc + " lan duoc giong)");
            if (clipL.Count != 3) du = false;
            yield return ChoIm(4f);
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
        File.WriteAllText("PlayTestShots/tiengtocbiennhaolon.txt", bao.ToString());
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
        Debug.Log("[TiengTocBienNhaoLon] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
