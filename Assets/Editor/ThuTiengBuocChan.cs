using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 140 - TIENG BUOC CHAN (17/10/2026, nguoi dung: "nguoi choi va tat ca nhan vat co trong phong di chuyen deu co am thanh stereo la tieng buoc
/// chan"; chon nguoi choi + BOT (quai khong), mat dat dat soi + loi nuoc; mau A ban 2 "dat soi kho, day + stereo hon", am luong NHO HON 25%).
/// Play Act2 that, tat GameDirector + xoa quai, BAT tam nhin + tieng.
///   A. (ngoai Play) 24 file trung tung byte ban goc (7-12 = ban LAT), STEREO, khong ForceToMono.
///   B. DI THAT (bom input) 4 s: tieng chan trai / phai XEN KE, so buoc = 2 x so chu ky clip di (doc toc do clip that); luc moi tieng ban chan
///      ay o SAT DAT (do cao chan tren mat dat, chuan hoa theo thap nhat / cao nhat cua chinh chan ay); giong can bang (2D), khong bam theo;
///      he so <= 0,5625 (0,75 x 0,75 - nho hon 25% roi nho them 25%, so viet tay); loa co tieng, khong mono; dung lai -> im.
///   C. LOI NUOC: dung trong vung nuoc (VungNuoc.DoNgap 0,1-0,45 m), di tai cho -> chi tieng NUOC; ra cho kho -> chi tieng DAT.
///   D. NGUOI CHOI KHAC (ban sao that NguoiChoiKhac.Sinh, di tai cho nhu tocDo01 tu mang): o tam nhin + 10 m -> bi suong che, loa im; doi chung
///      tat suong -> co giong; o 8 m ben PHAI -> loa phai to hon; 2 m to hon 14 m.
///   E. BIEN THE: moi loai du 6 clip, cao do +-6%.
/// Ket qua: PlayTestShots/tiengbuocchan.txt.
/// </summary>
public static class ThuTiengBuocChan
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengBuocChan";
    static readonly TiengViTri.Loai[] CacLoai = { TiengViTri.Loai.BuocDatTrai, TiengViTri.Loai.BuocDatPhai, TiengViTri.Loai.BuocNuocTrai, TiengViTri.Loai.BuocNuocPhai };

    [MenuItem("Diablo 2.5D/140. Chay thu TIENG BUOC CHAN (dat soi · loi nuoc, nguoi choi khac, suong mu)", false, 230)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengBuocChan] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengBuocChan] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 2 - nho them 25%] buoc chan mau A ban 2 (dat soi kho day), 6 bien the moi mat dat, chan phai = ban lat, he so " + TiengViTri.HeSoBuocChan);
        int dung = 0, tong = 0;
        foreach (var ten in new[] { "Dat", "Nuoc" })
            for (int i = 1; i <= 12; i++)
            {
                tong++;
                string game = "Assets/Resources/AmThanh/KyNang/Buoc" + ten + i + ".wav";
                string g = "CongCu/AmThanh/BuocChanThu/A2_dat_soi_kho_day_" + ten.ToLower() + (i <= 6 ? i.ToString() : (i - 6) + "_lat") + ".wav";
                var imp = AssetImporter.GetAtPath(game) as AudioImporter;
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
                bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
                bool ok = trung && c.channels == 2 && imp != null && !imp.forceToMono;
                if (ok) dung++;
                else Ghi(string.Format("A. Buoc{0}{1}: trung ban goc {2}; {3} kenh; ForceToMono {4}", ten, i, trung, c != null ? c.channels : 0, imp != null && imp.forceToMono));
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengBuocChan] " + s); }
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
    static int TongBuoc() { int s = 0; foreach (var l in CacLoai) s += SL(l); return s; }

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

    static void DatCho(PlayerController pc, Vector3 p)
    {
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = p + Vector3.up * 0.3f;
        if (cc != null) cc.enabled = true;
    }

    static Transform TimXuong(Transform goc, string ten)
    {
        foreach (var t in goc.GetComponentsInChildren<Transform>(true)) if (t.name == ten) return t;
        return null;
    }

    /// <summary>Huong di trong 18 m (4 s di ~16 m): khong vat can (Default), khong nuoc, dat khong doc qua.</summary>
    static bool HuongTrong(Vector3 goc, Vector3 h)
    {
        if (Physics.SphereCast(goc + Vector3.up * 1.0f, 0.6f, h, out _, 18f, 1 << 0, QueryTriggerInteraction.Ignore)) return false;
        for (float d = 0f; d <= 18f; d += 0.5f)
        {
            Vector3 p = Dat(goc + h * d);
            if (VungNuoc.DoNgap(p) > 0f) return false;
            if (Mathf.Abs(p.y - goc.y) > 0.8f) return false;
        }
        return true;
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
        var hh = pc.GetComponent<NguoiChoiHoatHinh>();
        var anim = pc.GetComponentInChildren<Animation>();
        NhacNen.BatTrongPhepThu = true;
        TamNhin.BatTrongPhepThu = true;
        var nn = NhacNen.Ban;
        if (nn != null) { nn.sanh.nguon.mute = true; nn.tran.nguon.mute = true; }
        yield return new WaitForSeconds(1.0f);
        float[] kq = new float[5];
        bool tuDoc = pc.tuDocInput;
        pc.tuDocInput = false;
        pc.input = GoiInput.Rong(0f);
        Vector3 goc = Dat(pc.transform.position);
        Transform[] chan = { TimXuong(pc.transform, "LeftFoot"), TimXuong(pc.transform, "RightFoot") };
        if (hh == null || anim == null || chan[0] == null || chan[1] == null) { Ghi("[LOI] thieu NguoiChoiHoatHinh / Animation / xuong chan"); loi++; Ket(); yield break; }
        yield return ChoIm(3f);

        // ===== B. di that =====
        Ghi("");
        Vector3 huong = Vector3.zero;
        for (int k = 0; k < 16 && huong == Vector3.zero; k++)
        {
            Vector3 h = Quaternion.Euler(0f, k * 22.5f, 0f) * Vector3.forward;
            if (HuongTrong(goc, h)) huong = h;
        }
        if (huong == Vector3.zero)
        {
            // cho dat dau khong co huong trong: do theo luoi khap ban do (chi truy van, chua dich nhan vat), cho dung kho
            for (float x = -45f; x <= 45f && huong == Vector3.zero; x += 6f)
                for (float z = -45f; z <= 45f && huong == Vector3.zero; z += 6f)
                {
                    Vector3 g = Dat(new Vector3(x, 0f, z));
                    if (VungNuoc.DoNgap(g) > 0f || Physics.CheckSphere(g + Vector3.up * 1f, 0.7f, 1 << 0, QueryTriggerInteraction.Ignore)) continue;
                    for (int j = 0; j < 8 && huong == Vector3.zero; j++)
                    {
                        Vector3 h = Quaternion.Euler(0f, j * 45f, 0f) * Vector3.forward;
                        if (HuongTrong(g, h)) { huong = h; goc = g; }
                    }
                }
            if (huong != Vector3.zero) { DatCho(pc, goc); yield return new WaitForSeconds(0.8f); }
        }
        Ghi(string.Format("B. duong thu: tu ({0:F1}, {1:F1}, {2:F1}) huong ({3:F2}, {4:F2})", goc.x, goc.y, goc.z, huong.x, huong.z));
        Kiem(huong != Vector3.zero, "khong tim duoc duong trong 18 m de di thu");
        {
            var trang = anim[hh.tenClipDi];
            int b0 = TongBuoc();
            int[] daTr = new int[2]; float[] hMin = { 9f, 9f }, hMax = { -9f, -9f };
            var suKien = new List<Vector3>();   // x = ben, y = do cao chan luc tieng, z = giay tu luc bat dau di
            var gocCao = new List<float>();   // goc nhan vat cao hon mat dat bao nhieu luc tieng (hang chan)
            var heSo = new List<float>(); int canBang = 0, bamTheo = 0, soGiong = 0; int xenKeSai = 0, benTruoc = -1;
            int sTr = SL(TiengViTri.Loai.BuocDatTrai), sPh = SL(TiengViTri.Loai.BuocDatPhai);
            double chuKy = 0; float lucDau = -1f;
            var goi = GoiInput.Rong(Time.deltaTime); goi.huongDi = huong;
            float hanDi = Time.time + 4f;
            var rmsDi = new List<float>(); var tqDi = new List<float>();
            var bl = new float[1024]; var br = new float[1024];
            double el = 0, er = 0, elr = 0; float tongRms = 0; int soRms = 0;
            while (Time.time < hanDi)
            {
                goi.dt = Time.deltaTime; pc.input = goi;
                yield return new WaitForEndOfFrame();
                if (trang != null && anim.IsPlaying(hh.tenClipDi)) { chuKy += Time.deltaTime * trang.speed / Mathf.Max(0.01f, trang.length); if (lucDau < 0f) lucDau = Time.time; }
                float[] hc = new float[2];
                for (int b = 0; b < 2; b++)
                {
                    hc[b] = chan[b].position.y - GioLoc.MatDatY(chan[b].position, chan[b].position.y + 2f);
                    hMin[b] = Mathf.Min(hMin[b], hc[b]); hMax[b] = Mathf.Max(hMax[b], hc[b]);
                }
                int nTr = SL(TiengViTri.Loai.BuocDatTrai), nPh = SL(TiengViTri.Loai.BuocDatPhai);
                for (int b = 0; b < 2; b++)
                {
                    int moi = b == 0 ? nTr - sTr : nPh - sPh;
                    for (int m = 0; m < moi; m++)
                    {
                        suKien.Add(new Vector3(b, hc[b], Time.time - (hanDi - 4f)));
                        gocCao.Add(pc.transform.position.y - GioLoc.MatDatY(pc.transform.position, pc.transform.position.y + 2f));
                        if (benTruoc == b) xenKeSai++;
                        benTruoc = b;
                    }
                }
                if (nTr != sTr || nPh != sPh)
                {
                    var gm = TiengViTri.GiongMoiNhat;
                    if (gm != null && (gm.loai == TiengViTri.Loai.BuocDatTrai || gm.loai == TiengViTri.Loai.BuocDatPhai))
                    {
                        soGiong++; heSo.Add(gm.heSo);
                        if (gm.nguon.spatialBlend == 0f) canBang++;
                        if (gm.bamTheo) bamTheo++;
                    }
                }
                sTr = nTr; sPh = nPh;
                AudioListener.GetOutputData(bl, 0); AudioListener.GetOutputData(br, 1);
                double a = 0, c = 0, ac = 0;
                for (int i = 0; i < bl.Length; i++) { a += bl[i] * bl[i]; c += br[i] * br[i]; ac += bl[i] * br[i]; }
                el += a; er += c; elr += ac; tongRms += (float)System.Math.Sqrt((a + c) / (2.0 * bl.Length)); soRms++;
            }
            pc.input = GoiInput.Rong(Time.deltaTime);
            int soBuoc = TongBuoc() - b0;
            float mongDoi = (float)(2.0 * chuKy);
            // do cao chan luc tieng, chuan hoa theo dai cua chinh chan ay
            var chuan = new List<float>(); float lonNhat = 0f;
            for (int si = 0; si < suKien.Count; si++)
            {
                var s = suKien[si];
                int b = (int)s.x;
                float u = (s.y - hMin[b]) / Mathf.Max(0.01f, hMax[b] - hMin[b]);
                chuan.Add(u); lonNhat = Mathf.Max(lonNhat, u);
                Ghi(string.Format("B. (chi tiet) tieng chan {0} luc {1:F2} s: chan cao {2:F3} m = {3:F2}; goc nhan vat tren dat {4:F3} m", b == 0 ? "trai" : "phai", s.z, s.y, u, gocCao[si]));
            }
            chuan.Sort();
            float trungVi = chuan.Count > 0 ? chuan[chuan.Count / 2] : 1f;
            heSo.Sort();
            float tq = (el > 0 && er > 0) ? (float)(elr / System.Math.Sqrt(el * er)) : 1f;
            Vector3 diDuoc = pc.transform.position - goc; diDuoc.y = 0f;
            Ghi(string.Format("B. di that {0:F1} m trong 4 s (toc clip {1:F2}): {2} tieng buoc (chan trai {3}, phai {4}), mong doi 2 x {5:F2} chu ky clip = {6:F1}; xen ke sai {7}",
                diDuoc.magnitude, trang != null ? trang.speed : 0f, soBuoc, suKien.FindAll(s => s.x == 0).Count, suKien.FindAll(s => s.x == 1).Count, chuKy, mongDoi, xenKeSai));
            Ghi(string.Format("B. do cao ban chan tren dat luc tieng (0 = thap nhat, 1 = cao nhat cua chinh chan ay; chan trai {0:F3}-{1:F3} m, phai {2:F3}-{3:F3} m): trung vi {4:F2}, lon nhat {5:F2}",
                hMin[0], hMax[0], hMin[1], hMax[1], trungVi, lonNhat));
            Ghi(string.Format("B. giong: {0} lan, can bang 2D {1}, bam theo {2}; he so {3:F3} - {4:F3} (HeSoBuocChan {5}); loa RMS TB {6:F4}, tuong quan {7:F3}",
                soGiong, canBang, bamTheo, heSo.Count > 0 ? heSo[0] : 0f, heSo.Count > 0 ? heSo[heSo.Count - 1] : 0f, TiengViTri.HeSoBuocChan, soRms > 0 ? tongRms / soRms : 0f, tq));
            Kiem(diDuoc.magnitude > 4f, "nhan vat khong di duoc");
            Kiem(soBuoc >= 4 && Mathf.Abs(soBuoc - mongDoi) <= 2f, "so tieng buoc khong khop 2 lan moi chu ky clip di");
            Kiem(xenKeSai == 0, "tieng chan trai / phai khong xen ke");
            Kiem(trungVi <= 0.15f && lonNhat <= 0.35f, "tieng buoc khong roi vao luc ban chan cham dat");
            Kiem(soGiong == soBuoc && canBang == soGiong && bamTheo == 0, "giong buoc chan khong can bang / bi bam theo");
            Kiem(heSo.Count > 0 && heSo[heSo.Count - 1] <= TiengViTri.HeSoBuocChan + 1e-4f && heSo[0] >= TiengViTri.HeSoBuocChan * 0.55f - 1e-4f, "he so am luong buoc chan sai (phai <= 0,5625)");
            Kiem(Mathf.Abs(TiengViTri.HeSoBuocChan - 0.5625f) < 1e-4f, "HeSoBuocChan khong bang 0,75 x 0,75 = 0,5625 (so viet tay)");
            Kiem(soRms > 0 && tongRms / soRms > 1e-3f && tq < 0.9f, "loa khong co tieng buoc / gan mono");
            // dung lai -> im
            yield return new WaitForSeconds(0.6f);
            int d0 = TongBuoc();
            yield return new WaitForSeconds(1.5f);
            Ghi(string.Format("B2. dung lai: 1,5 s sau them {0} tieng buoc", TongBuoc() - d0));
            Kiem(TongBuoc() - d0 == 0, "dung yen ma van co tieng buoc");
        }
        yield return ChoIm(3f);

        // ===== C. loi nuoc =====
        Ghi("");
        {
            Vector3 choNuoc = Vector3.zero; float ngap = 0f;
            foreach (var v in Object.FindObjectsByType<VungNuoc>(FindObjectsSortMode.None))
            {
                for (float x = -6f; x <= 6f && choNuoc == Vector3.zero; x += 0.5f)
                    for (float z = -6f; z <= 6f && choNuoc == Vector3.zero; z += 0.5f)
                    {
                        Vector3 p = Dat(v.transform.position + new Vector3(x, 0f, z));
                        float n = VungNuoc.DoNgap(p + Vector3.up * 0.08f);
                        if (n > 0.1f && n < 0.45f) { choNuoc = p; ngap = n; }
                    }
                if (choNuoc != Vector3.zero) break;
            }
            Kiem(choNuoc != Vector3.zero, "khong tim duoc cho nuoc nong de thu");
            if (choNuoc != Vector3.zero)
            {
                DatCho(pc, choNuoc);
                yield return new WaitForSeconds(0.6f);
                var ln = pc.GetComponent<LoiNuoc>();
                int dT = SL(TiengViTri.Loai.BuocDatTrai) + SL(TiengViTri.Loai.BuocDatPhai), nT = SL(TiengViTri.Loai.BuocNuocTrai), nP = SL(TiengViTri.Loai.BuocNuocPhai);
                hh.tocDoEp = 1f;
                yield return Nghe(3f, kq);
                hh.tocDoEp = 0f;
                int dat = SL(TiengViTri.Loai.BuocDatTrai) + SL(TiengViTri.Loai.BuocDatPhai) - dT, nuocTr = SL(TiengViTri.Loai.BuocNuocTrai) - nT, nuocPh = SL(TiengViTri.Loai.BuocNuocPhai) - nP;
                Ghi(string.Format("C. dung trong nuoc (ngap tai cho {0:F2} m, LoiNuoc.DoNgap {1:F2}), di tai cho 3 s: tieng NUOC trai {2} phai {3}, tieng DAT {4}; loa RMS {5:F4} tuong quan {6:F3}",
                    ngap, ln != null ? ln.DoNgap : -1f, nuocTr, nuocPh, dat, kq[0], kq[3]));
                Kiem(nuocTr >= 2 && nuocPh >= 2 && dat == 0, "trong nuoc khong ra tieng loi nuoc");
                Kiem(kq[0] > 1e-3f && kq[3] < 0.9f, "loa khong co tieng loi nuoc / gan mono");
                yield return ChoIm(2f);
                // ra cho kho
                DatCho(pc, goc);
                yield return new WaitForSeconds(0.6f);
                int dT2 = SL(TiengViTri.Loai.BuocDatTrai) + SL(TiengViTri.Loai.BuocDatPhai), n2 = SL(TiengViTri.Loai.BuocNuocTrai) + SL(TiengViTri.Loai.BuocNuocPhai);
                hh.tocDoEp = 1f;
                yield return new WaitForSeconds(2f);
                hh.tocDoEp = 0f;
                int dat2 = SL(TiengViTri.Loai.BuocDatTrai) + SL(TiengViTri.Loai.BuocDatPhai) - dT2, nuoc2 = SL(TiengViTri.Loai.BuocNuocTrai) + SL(TiengViTri.Loai.BuocNuocPhai) - n2;
                Ghi(string.Format("C2. ra cho kho di tai cho 2 s: tieng DAT {0}, tieng NUOC {1}", dat2, nuoc2));
                Kiem(dat2 >= 3 && nuoc2 == 0, "cho kho ma khong ra tieng dat");
            }
        }
        hh.tocDoEp = 0f;
        DatCho(pc, goc);
        yield return new WaitForSeconds(0.8f);
        yield return ChoIm(3f);

        // ===== D. nguoi choi khac =====
        Ghi("");
        {
            var cam = Camera.main.transform;
            Vector3 truoc = cam.forward; truoc.y = 0f; truoc.Normalize();
            Vector3 phai = Vector3.Cross(Vector3.up, truoc);
            Vector3 xa = Dat(goc + truoc * (TamNhin.BanKinh + 10f));
            var khac = NguoiChoiKhac.Sinh("tam_khac", "TAM_Khac", xa + Vector3.up * 0.3f);
            if (khac == null) { Ghi("[LOI] khong sinh duoc nguoi choi khac"); loi++; }
            else
            {
                khac.gameObject.name = "TAM_NguoiKhac";
                var hk = khac.GetComponent<NguoiChoiHoatHinh>();
                yield return new WaitForSeconds(0.5f);
                // trong suong (renderer bi an -> Animation bi cull -> tu dem pha)
                int s0 = TiengViTri.SoBiSuongChe, b0 = TongBuoc(), tp0 = NguoiChoiHoatHinh.SoLanTuDemPha;
                hk.tocDoEp = 1f;
                yield return Nghe(2.5f, kq);
                int soBuocXa = TongBuoc() - b0, biChe = TiengViTri.SoBiSuongChe - s0;
                Ghi(string.Format("D. nguoi khac o {0:F0} m (MucThay {1:F2}) di 2,5 s: {2} tieng buoc, bi suong che {3}; loa RMS {4:F5}; khung clip bi cull tu dem pha {5}",
                    TamNhin.BanKinh + 10f, TamNhin.MucThay(xa), soBuocXa, biChe, kq[0], NguoiChoiHoatHinh.SoLanTuDemPha - tp0));
                Kiem(soBuocXa >= 3 && biChe == soBuocXa && kq[0] < 1e-4f, "nguoi khac trong suong van nghe tieng buoc");
                // doi chung tat suong
                TamNhin.BatTrongPhepThu = false;
                yield return null; yield return null;
                int s1 = TiengViTri.SoBiSuongChe, g1 = TiengViTri.SoLanDuocGiong, b1 = TongBuoc();
                yield return new WaitForSeconds(2.5f);
                Ghi(string.Format("D2. DOI CHUNG tat suong: {0} tieng buoc, bi che {1}, duoc giong {2}", TongBuoc() - b1, TiengViTri.SoBiSuongChe - s1, TiengViTri.SoLanDuocGiong - g1));
                Kiem(TongBuoc() - b1 >= 3 && TiengViTri.SoLanDuocGiong - g1 >= 3, "doi chung tat suong ma van im");
                TamNhin.BatTrongPhepThu = true;
                hk.tocDoEp = 0f;
                yield return ChoIm(3f);
                // GAN (trong tam nhin) MA NGOAI KHUNG HINH: Animation bi cull ma van phai co tieng
                var cc = khac.GetComponent<CharacterController>();
                {
                    var mp = GeometryUtility.CalculateFrustumPlanes(Camera.main);
                    Vector3 choNgoai = Vector3.zero;
                    for (float d = 4f; d <= TamNhin.BanKinh - 3f && choNgoai == Vector3.zero; d += 1f)
                        for (int k = 0; k < 12 && choNgoai == Vector3.zero; k++)
                        {
                            Vector3 p = Dat(goc + Quaternion.Euler(0f, 90f + k * 15f, 0f) * truoc * d);
                            if (TamNhin.MucThay(p) < 0.99f) continue;
                            if (GeometryUtility.TestPlanesAABB(mp, new Bounds(p + Vector3.up * 1f, new Vector3(3f, 3f, 3f)))) continue;
                            choNgoai = p;
                        }
                    Kiem(choNgoai != Vector3.zero, "khong tim duoc cho trong tam nhin ma ngoai khung hinh");
                    if (cc != null) cc.enabled = false;
                    khac.transform.position = choNgoai + Vector3.up * 0.3f;
                    if (cc != null) cc.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                    bool thay = false;
                    foreach (var r in khac.GetComponentsInChildren<Renderer>()) if (r.isVisible) thay = true;
                    Vector3 cs = khac.transform.position - goc; cs.y = 0f;
                    int b2 = TongBuoc(), g2 = TiengViTri.SoLanDuocGiong, tp2 = NguoiChoiHoatHinh.SoLanTuDemPha;
                    hk.tocDoEp = 1f;
                    yield return Nghe(2.5f, kq);
                    hk.tocDoEp = 0f;
                    Ghi(string.Format("D1. nguoi khac NGOAI KHUNG HINH cach nhan vat {0:F1} m (MucThay {1:F2}, renderer hien {2}) di 2,5 s: {3} tieng buoc, duoc giong {4}, khung tu dem pha {5}; loa RMS {6:F4}",
                        cs.magnitude, TamNhin.MucThay(khac.transform.position), thay, TongBuoc() - b2, TiengViTri.SoLanDuocGiong - g2, NguoiChoiHoatHinh.SoLanTuDemPha - tp2, kq[0]));
                    Kiem(!thay && TongBuoc() - b2 >= 3 && TiengViTri.SoLanDuocGiong - g2 >= 3 && kq[0] > 1e-3f, "nguoi gan ma ngoai khung hinh di khong co tieng buoc");
                    yield return ChoIm(3f);
                }
                // ben phai 8 m
                if (cc != null) cc.enabled = false;
                khac.transform.position = Dat(goc + phai * 8f) + Vector3.up * 0.3f;
                if (cc != null) cc.enabled = true;
                yield return new WaitForSeconds(0.5f);
                hk.tocDoEp = 1f;
                yield return Nghe(2.5f, kq);
                hk.tocDoEp = 0f;
                float tiPhai = kq[2] / Mathf.Max(1e-12f, kq[1]);
                Ghi(string.Format("D3. nguoi khac 8 m ben PHAI di 2,5 s: loa phai / trai {0:F2}, RMS {1:F4}, tuong quan {2:F3}", tiPhai, kq[0], kq[3]));
                Kiem(tiPhai > 1.5f && kq[0] > 1e-3f, "tieng buoc nguoi ben phai khong lech phai");
                yield return ChoIm(3f);
                // gan / xa
                float[] muc = new float[2]; float[] cach = { 2f, 14f };
                for (int i = 0; i < 2; i++)
                {
                    if (cc != null) cc.enabled = false;
                    khac.transform.position = Dat(goc + phai * cach[i]) + Vector3.up * 0.3f;
                    if (cc != null) cc.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                    hk.tocDoEp = 1f;
                    float lon = 0f; float t0 = Time.time;
                    while (Time.time - t0 < 2f)
                    {
                        yield return null;
                        foreach (var g in TiengViTri.CacGiong)
                            if (!g.Ranh && (g.loai == TiengViTri.Loai.BuocDatTrai || g.loai == TiengViTri.Loai.BuocDatPhai)) lon = Mathf.Max(lon, g.MucDangDat / Mathf.Max(1e-4f, g.heSo));
                    }
                    hk.tocDoEp = 0f;
                    muc[i] = lon;
                    yield return ChoIm(3f);
                }
                Ghi(string.Format("D4. he so khoang cach (muc / heSo): 2 m {0:F2}, 14 m {1:F2}", muc[0], muc[1]));
                Kiem(muc[0] > 0.95f && muc[1] < muc[0] * 0.85f && muc[1] > 0f, "tieng buoc khong nho dan theo khoang cach");
                Object.Destroy(khac.gameObject);
            }
        }
        yield return ChoIm(3f);

        // ===== E. bien the + cao do =====
        Ghi("");
        {
            var dong = new List<string>(); bool du = true; float caoMin = 9f, caoMax = 0f;
            for (int k = 0; k < 4; k++)
            {
                var ten = new HashSet<string>();
                for (int i = 0; i < 48; i++)
                {
                    int tp = TiengViTri.SoLanDuocGiong;
                    TiengViTri.BuocChan(goc + Vector3.right * 1.5f, k % 2 == 1, k >= 2);
                    var gm = TiengViTri.GiongMoiNhat;
                    if (TiengViTri.SoLanDuocGiong > tp && gm != null && gm.loai == CacLoai[k] && gm.nguon.clip != null)
                    {
                        ten.Add(gm.nguon.clip.name);
                        caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                    }
                    yield return new WaitForSeconds(0.3f);
                }
                var ds = new List<string>(ten); ds.Sort();
                dong.Add(CacLoai[k] + ": " + ds.Count + " (" + string.Join(" ", ds) + ")");
                int tu = k % 2 == 0 ? 1 : 7;
                bool dungDai = ds.Count == 6;
                foreach (var t in ds) { int so; int.TryParse(t.Replace("BuocDat", "").Replace("BuocNuoc", ""), out so); if (so < tu || so > tu + 5 || !t.StartsWith(k < 2 ? "BuocDat" : "BuocNuoc")) dungDai = false; }
                if (!dungDai) du = false;
                yield return ChoIm(3f);
            }
            foreach (var d in dong) Ghi("E. " + d);
            Ghi(string.Format("E. cao do {0:F3} - {1:F3}", caoMin, caoMax));
            Kiem(du, "khong du 6 bien the dung bo moi loai (chan trai 1-6, chan phai 7-12)");
            Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");
        }
        pc.tuDocInput = tuDoc;
        hh.tocDoEp = -1f;
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengbuocchan.txt", bao.ToString());
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
        Debug.Log("[TiengBuocChan] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
