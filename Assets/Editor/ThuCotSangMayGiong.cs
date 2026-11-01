using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 154 - MAY GIONG: COT SANG THANG DUNG THAY BANG TIA SET CON (01/11/2026, nguoi dung khoanh "1 tia sang thang dung rat la danh
/// cung luc voi tia set, khong phai tia set" -> "thay bang tia set"; chon them 1 tia set con, CHI May giong).
///
/// Vao Play Act2 (dem), tat GameDirector:
///   A. May giong THAT (MayGiong.Spawn) danh xuong dat ~4 s: moi vu loe cham dat "SetChamDat" KHONG con Column0/1 dang bat; moi tia chinh
///      cham DAT co dung 1 tia con cung khung: ngon cham dat (lech mat Ground < 0,05) canh cho cham 0,9-1,4 m, SONG SONG tia chinh (+-0,25 m),
///      truc hai tia cach nhau >= 0,6 m (khong dan cheo), manh hon (coreWidth x0,6), 1 nhanh. Tia chinh GAN THANG DUNG nhu Sam set
///      (lech ngang / cao <= 0,09 - cung ngay nguoi dung: "tia set danh xuong giong Sam set"; C do luon tia Sam set that).
///   B. Tia dung o mat vom khien (TiaMayGiong(.., false)): 0 tia con, 0 vu loe.
///   C. DOI CHUNG: Sam set THAT (CastAt 2) van con cot sang, 0 tia con.
///   D. ANH: chi vu loe cham dat (khong tia) truoc cung mot may quay - cu (giu cot) / moi; dem diem anh SANG LEN o khung 1,2-6 m tren
///      cho cham. + Anh may quay game 2.5D mot tia May giong cu / moi (PlayTestShots/cot_sang_may_giong/).
/// </summary>
public static class ThuCotSangMayGiong
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/cot_sang_may_giong/";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/154 May giong: cot sang thang dung thay bang tia set con", false, 228)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuCotSangMayGiong] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        nk.Length = 0; loi = 0;
        daBatDau = false;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuCotSangMayGiong");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static bool LaLoe(GameObject g) { return g.name.Contains("SetChamDat") || g.name.Contains("LightningImpact"); }

    static int CotDangBat(GameObject loe)
    {
        int n = 0;
        foreach (var t in loe.GetComponentsInChildren<Transform>(true))
            if (System.Array.IndexOf(VfxFactory.LopCotSang, t.name) >= 0 && t.gameObject.activeInHierarchy) n++;
        return n;
    }

    static float Ngang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    static PlayerController toi;

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        TamNhin.BatTrongPhepThu = true;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var cam = Camera.main;
        var rig = cam.GetComponent<CameraRig>();
        if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
        var hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.epCamUng = true;
        yield return new WaitForSeconds(1.0f);
        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        Vector3 P = toi.transform.position;
        toi.health.maxHealth = 100000f; toi.health.health = 100000f;
        var mauToi = toi.GetComponent<Damageable>();

        // ================= A. May giong that =================
        nk.AppendLine("== A May giong THAT danh xuong dat");
        {
            var cuArc = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            var cuGoc = new HashSet<GameObject>();
            foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) cuGoc.Add(g);
            int d0 = MayGiong.SoTiaDaDanh, v0 = MayGiong.SoChamVom, c0 = VfxFactory.SoTiaSetCon, t0 = VfxFactory.SoCotSangDaTat;
            var may = MayGiong.Spawn(P + F * 9f, LayerMask.GetMask("Enemy"), mauToi);
            int soLoe = 0, loeConCot = 0, soChinh = 0, soCon = 0, conKhop = 0, chinhKhongCon = 0;
            float lechNgonMin = 99f, lechNgonMax = 0f, lechDatMax = 0f, tiNgang = 0f;
            float nghiengChinhMax = 0f, nghiengConMax = 0f, songSongMax = 0f, cachTrucMin = 99f;
            for (float h = Time.time + 4.0f; Time.time < h; )
            {
                yield return null;
                var chinh = new List<LightningArc>(); var con = new List<LightningArc>();
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (!cuArc.Add(a)) continue;
                    if (a.name == "TiaMayGiong") chinh.Add(a);
                    else if (a.name == VfxFactory.TenTiaCon) con.Add(a);
                }
                foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (!cuGoc.Add(g) || !LaLoe(g)) continue;
                    soLoe++;
                    if (CotDangBat(g) > 0) loeConCot++;
                }
                soChinh += chinh.Count; soCon += con.Count;
                var daGhep = new HashSet<LightningArc>();
                foreach (var c in con)
                {
                    LightningArc gan = null; float best = 99f;
                    foreach (var m in chinh) { float dd = Ngang(m.end, c.end); if (dd < best && !daGhep.Contains(m)) { best = dd; gan = m; } }
                    if (gan == null) continue;
                    daGhep.Add(gan);
                    float ln = Ngang(gan.end, c.end);
                    float ld = Mathf.Abs(c.end.y - VfxFactory.GroundY(c.end));
                    lechNgonMin = Mathf.Min(lechNgonMin, ln); lechNgonMax = Mathf.Max(lechNgonMax, ln);
                    lechDatMax = Mathf.Max(lechDatMax, ld);
                    tiNgang = c.coreWidth / Mathf.Max(1e-4f, gan.coreWidth);
                    // do nghieng = lech ngang / chieu cao (Sam set: <= 0,09); song song = hieu hai vec-to nghieng; truc hai tia cach nhau o 11 do cao
                    float nm = Ngang(gan.start, gan.end) / Mathf.Max(0.1f, gan.start.y - gan.end.y);
                    float nc = Ngang(c.start, c.end) / Mathf.Max(0.1f, c.start.y - c.end.y);
                    Vector3 vm = gan.start - gan.end, vc = c.start - c.end; vm.y = 0f; vc.y = 0f;
                    float ss = (vm - vc).magnitude;
                    float ct = 99f;
                    for (int k = 0; k <= 10; k++) ct = Mathf.Min(ct, Ngang(Vector3.Lerp(gan.end, gan.start, k / 10f), Vector3.Lerp(c.end, c.start, k / 10f)));
                    nghiengChinhMax = Mathf.Max(nghiengChinhMax, nm); nghiengConMax = Mathf.Max(nghiengConMax, nc);
                    songSongMax = Mathf.Max(songSongMax, ss); cachTrucMin = Mathf.Min(cachTrucMin, ct);
                    if (ln >= 0.89f && ln <= 1.41f && ld < 0.05f && Mathf.Abs(c.start.y - gan.start.y) < 0.01f && ss <= VfxFactory.LechSongSong + 0.001f
                        && c.branches == 1 && Mathf.Abs(tiNgang - VfxFactory.HeSoNgangTiaCon) < 0.01f) conKhop++;
                }
                foreach (var m in chinh) if (!daGhep.Contains(m)) chinhKhongCon++;
            }
            if (may != null) Object.Destroy(may.gameObject);
            int datDat = (MayGiong.SoTiaDaDanh - d0) - (MayGiong.SoChamVom - v0);
            nk.AppendLine(string.Format("  tia chinh {0} (cham dat {1}), tia con {2}, vu loe cham dat {3}, cot sang da tat {4}",
                soChinh, datDat, soCon, soLoe, VfxFactory.SoCotSangDaTat - t0));
            nk.AppendLine(string.Format("  ngon tia con lech cho cham {0:F2}-{1:F2} m | ngon lech mat dat <= {2:F3} m | be ngang x{3:F2}",
                lechNgonMin, lechNgonMax, lechDatMax, tiNgang));
            nk.AppendLine(string.Format("  do nghieng (lech ngang / cao) lon nhat: tia chinh {0:F3}, tia con {1:F3} (Sam set <= {2:F2}; ban cu toi 0,34) | lech song song <= {3:F2} m | hai truc cach nhau >= {4:F2} m",
                nghiengChinhMax, nghiengConMax, MayGiong.LechTiaTheoCao, songSongMax, cachTrucMin));
            Kiem(nghiengChinhMax <= MayGiong.LechTiaTheoCao + 0.001f, "A tia chinh gan thang dung nhu Sam set (nghieng " + nghiengChinhMax.ToString("F3") + " <= 0,09)");
            Kiem(cachTrucMin >= 0.6f, "A tia con khong dan cheo tia chinh (truc cach nhau >= " + cachTrucMin.ToString("F2") + " m, phai >= 0,6)");
            Kiem(datDat >= 8, "A co " + datDat + " tia cham dat (>= 8)");
            Kiem(soLoe == datDat && loeConCot == 0, "A " + soLoe + " vu loe cham dat, con cot sang " + loeConCot + " (phai 0)");
            Kiem(VfxFactory.SoCotSangDaTat - t0 == 2 * soLoe, "A da tat " + (VfxFactory.SoCotSangDaTat - t0) + " tam cot sang = 2 x so vu loe");
            Kiem(soCon == datDat && VfxFactory.SoTiaSetCon - c0 == datDat && chinhKhongCon == 0, "A moi tia cham dat co dung 1 tia con (" + soCon + "/" + datDat + ", tia chinh thieu con " + chinhKhongCon + ")");
            Kiem(conKhop == soCon, "A " + conKhop + "/" + soCon + " tia con dung cho (song song +-0,25 m, ngon 0,9-1,4 m sat dat, manh x0,6, 1 nhanh)");
        }
        yield return new WaitForSeconds(1.0f);

        // ================= B. Tia dung o mat vom khien =================
        nk.AppendLine("== B Tia dung o mat vom (chamDat false)");
        {
            int c0 = VfxFactory.SoTiaSetCon, t0 = VfxFactory.SoCotSangDaTat;
            var cuGoc = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
            Vector3 cho = P + F * 6f; cho.y = VfxFactory.GroundY(cho) + 1.5f;
            VfxFactory.TiaMayGiong(cho + Vector3.up * MayGiong.CaoMay, cho, false);
            int loe = 0;
            foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) if (!cuGoc.Contains(g) && LaLoe(g)) loe++;
            Kiem(VfxFactory.SoTiaSetCon - c0 == 0 && loe == 0 && VfxFactory.SoCotSangDaTat - t0 == 0,
                 "B tia con +" + (VfxFactory.SoTiaSetCon - c0) + ", vu loe +" + loe + " (phai 0 / 0)");
        }
        yield return new WaitForSeconds(0.8f);

        // ================= C. Doi chung Sam set =================
        nk.AppendLine("== C DOI CHUNG Sam set THAT");
        {
            const int KySamSet = 2;
            CapDo.MoCaDuongChoPhepThu(KySamSet);
            yield return new WaitForSeconds(0.3f);
            int c0 = VfxFactory.SoTiaSetCon;
            var cuGoc = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
            toi.mana = toi.maxMana;
            toi.CastAt(KySamSet, P + F * 8f);
            int soLoe = 0, coCot = 0; float nghiengSS = 0f; int soTiaSS = 0;
            var cuArcC = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            for (float h = Time.time + 3.0f; Time.time < h; )
            {
                yield return null;
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (!cuArcC.Add(a) || a.start.y - a.end.y < 8f) continue;   // chi tia chinh tu tren troi
                    soTiaSS++;
                    nghiengSS = Mathf.Max(nghiengSS, Ngang(a.start, a.end) / (a.start.y - a.end.y));
                }
                foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (!cuGoc.Add(g) || !LaLoe(g)) continue;
                    soLoe++;
                    if (CotDangBat(g) > 0) coCot++;
                }
            }
            foreach (var st in Object.FindObjectsByType<LightningStorm>(FindObjectsInactive.Exclude)) Object.Destroy(st.gameObject);
            Kiem(soLoe >= 5 && coCot == soLoe, "C Sam set: " + soLoe + " vu loe, " + coCot + " con cot sang (giu nguyen)");
            Kiem(VfxFactory.SoTiaSetCon - c0 == 0, "C Sam set: tia con +" + (VfxFactory.SoTiaSetCon - c0) + " (phai 0)");
            nk.AppendLine("  Sam set " + soTiaSS + " tia, do nghieng lon nhat " + nghiengSS.ToString("F3"));
            Kiem(soTiaSS >= 5 && nghiengSS <= MayGiong.LechTiaTheoCao + 0.002f, "C do nghieng tia Sam set that <= 0,09 (cung chuan voi May giong)");
        }
        yield return new WaitForSeconds(1.5f);

        // ================= D. Anh =================
        nk.AppendLine("== D Anh (dem)");
        {
            if (rig != null) rig.enabled = false;
            Vector3 cho = P + F * 7f + R * 1.5f; cho.y = VfxFactory.GroundY(cho);
            // May quay can: ngang tam, cach 11 m, nhin vao giua cot
            var cg = new GameObject("TAM_CamCotSang");
            var cc = cg.AddComponent<Camera>();
            cc.CopyFrom(cam); cc.enabled = false;
            cg.transform.position = cho - F * 11f + Vector3.up * 3.2f;
            cg.transform.LookAt(cho + Vector3.up * 3.2f);
            Time.captureDeltaTime = 1f / 60f;
            yield return null; yield return null;
            var nen = ChupCan(cc);
            int[] dem = new int[2];
            for (int lan = 0; lan < 2; lan++)
            {
                var loe = VfxFactory.LoeSetChamDat(cho, MayGiong.BanKinhTia);
                VfxFactory.TatVetGach(loe);
                if (lan == 1) VfxFactory.TatCotSang(loe);
                yield return null; yield return null; yield return null;
                var tx = ChupCan(cc);
                dem[lan] = DemSangLen(nen, tx, cc, cho);
                File.WriteAllBytes(Thu + (lan == 0 ? "loe_cu_co_cot.png" : "loe_moi_khong_cot.png"), tx.EncodeToPNG());
                Object.Destroy(tx);
                Object.Destroy(loe);
                for (int i = 0; i < 90; i++) yield return null;
            }
            nk.AppendLine("  diem anh SANG LEN o khung 1,2-6 m tren cho cham: cu " + dem[0] + " / moi " + dem[1]);
            Kiem(dem[0] > 300 && dem[1] * 20 < dem[0], "D cot sang tren anh: cu " + dem[0] + ", moi " + dem[1] + " (moi < 5% cu)");
            Object.Destroy(nen);
            Object.Destroy(cg);

            // Anh may quay game (3D tu do nhu anh nguoi dung): mot tia May giong TRUOC (nghieng + tia con dan cheo) / moi, cho danh giua khung
            if (rig != null) { rig.enabled = true; rig.SetView((int)ViewMode.ThirdPerson3D); }
            Time.captureDeltaTime = 0f;
            yield return new WaitForSeconds(1.0f);
            if (rig != null) rig.enabled = false;
            {
                Vector3 truoc = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                cho = toi.transform.position + truoc * 9f; cho.y = VfxFactory.GroundY(cho);
            }
            Time.captureDeltaTime = 1f / 60f;
            for (int lan = 0; lan < 2; lan++)
            {
                Random.InitState(1107);
                Vector3 tu = cho + Vector3.up * MayGiong.CaoMay + cam.transform.right * (lan == 0 ? 2.2f : 0.5f);
                if (lan == 0)
                {
                    // TRUOC (commit 76ec369, anh nguoi dung): tia nghieng 2,2 m + tia con goc / ngon lech hai huong -> dan cheo
                    var arc = LightningArc.Create(tu, cho, 1f, VfxFactory.GiayTiaSamSet);
                    arc.name = "TAM_TiaCu"; arc.segments = 20; arc.branches = Random.Range(2, 4);
                    VfxFactory.LightningImpact(cho, MayGiong.BanKinhTia, true);
                    Vector3 tuCon = tu - cam.transform.right * 1.5f;
                    Vector3 denCon = cho + cam.transform.right * 1.1f; denCon.y = VfxFactory.GroundY(denCon);
                    var con = LightningArc.Create(tuCon, denCon, VfxFactory.HeSoNgangTiaCon, VfxFactory.GiayTiaSamSet);
                    con.name = "TAM_TiaConCu"; con.segments = 16; con.branches = 1;
                }
                else VfxFactory.TiaMayGiong(tu, cho);
                yield return null; yield return null; yield return null;
                yield return new WaitForEndOfFrame();
                var tx = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Thu + (lan == 0 ? "game_truoc.png" : "game_moi.png"), tx.EncodeToPNG());
                Object.Destroy(tx);
                for (int i = 0; i < 120; i++) yield return null;
            }
            Time.captureDeltaTime = 0f;
            if (rig != null) rig.enabled = true;
        }

        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 154 - May giong: cot sang thay bang tia set con: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuCotSangMayGiong] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuCotSangMayGiong");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static Texture2D ChupCan(Camera cc)
    {
        var rt = RenderTexture.GetTemporary(640, 480, 24);
        cc.targetTexture = rt; cc.Render(); cc.targetTexture = null;
        RenderTexture.active = rt;
        var tx = new Texture2D(640, 480, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 640, 480), 0, 0); tx.Apply();
        RenderTexture.active = null; RenderTexture.ReleaseTemporary(rt);
        return tx;
    }

    /// <summary>So diem anh sang len > 0,25 so voi nen VA nga xanh (b - r > 0,15), trong khung rong 3 m, cao 1,2-6 m tren cho cham (chieu qua may quay cc).</summary>
    static int DemSangLen(Texture2D nen, Texture2D tx, Camera cc, Vector3 cho)
    {
        var rt = RenderTexture.GetTemporary(640, 480, 24);
        cc.targetTexture = rt;
        Vector3 phai = cc.transform.right;
        Vector3 a = cc.WorldToScreenPoint(cho + Vector3.up * 1.2f - phai * 1.5f);
        Vector3 b = cc.WorldToScreenPoint(cho + Vector3.up * 6.0f + phai * 1.5f);
        cc.targetTexture = null; RenderTexture.ReleaseTemporary(rt);
        int x0 = Mathf.Clamp((int)Mathf.Min(a.x, b.x), 0, 639), x1 = Mathf.Clamp((int)Mathf.Max(a.x, b.x), 0, 639);
        int y0 = Mathf.Clamp((int)Mathf.Min(a.y, b.y), 0, 479), y1 = Mathf.Clamp((int)Mathf.Max(a.y, b.y), 0, 479);
        int n = 0;
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                Color c0 = nen.GetPixel(x, y), c1 = tx.GetPixel(x, y);
                // chi diem SANG LEN va NGA XANH (cot sang xanh trang) - than cay duoc den chop roi sang trang thi khong tinh
                if (c1.grayscale - c0.grayscale > 0.25f && c1.b - c1.r > 0.15f) n++;
            }
        return n;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        Debug.Log("[ThuCotSangMayGiong] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
