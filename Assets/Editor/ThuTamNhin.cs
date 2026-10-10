using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 117 - TAM NHIN 25 m + SUONG CHIEN TRANH (nguoi dung 10/10/2026, kieu StarCraft 2). Play Act2 that, bat phan hinh (TamNhin.BatTrongPhepThu).
///   A. Bien toan cuc: _TN_Bat = 1, nguon nhin = nhan vat; vat lieu Standard cua canh da doi sang ChuanSuong; co tam suong dia hinh.
///   B. ANH THAT (may quay truc giao nhin thang xuong, ve hai lan TRONG CUNG KHUNG: suong tat / bat): do sang dia hinh (lop Ground) va canh
///      vat (Default) trong vanh 6-20 m (phai giu nguyen) va 31-45 m (phai toi han nhung KHONG den - kieu SC2 "thay dia hinh mo").
///   C. SUONG KHOANG CACH CU CON NGUYEN: shader co finalcolor thi Unity khong tu ap suong - tam phang dung shader DaMo / ChuanSuong / Standard
///      (doi chung, Unity tu ap) o 40 m, do he so suong k = (bat - mauSuong) / (tat - mauSuong) so voi exp(-(mat do x khoang cach)^2) viet tay:
///      khop = co suong dung mot lan (ap hai lan thi ra k^2).
///   D. AN VAT DONG: quai o 20 / 30 / 60 m -> hien / an / an; hieu ung hat sinh luc chay o 10 / 35 m -> hien / an; den diem o 5 / 45 m.
///   E. DONG DOI (Doi): mot "dong doi" o 40 m -> quai canh no hien; NhinThay cho BOT.
///   F. CHET: giu tam nhin quanh xac.
///   G. Anh: may quay game keo xa het co (2.5D va 3D tu do) -> PlayTestShots/tamnhin_*.png.
/// </summary>
public static class ThuTamNhin
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/117. Chay thu TAM NHIN 25 m + suong chien tranh", false, 210)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu tam nhin", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tam nhin 25 m + suong chien tranh");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
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
        if (GameObject.Find("TAM_TamNhin") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_TamNhin");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TamNhin] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }
    static float Ngang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    static void DatCho(Transform t, Vector3 p)
    {
        var cc = t.GetComponent<CharacterController>();
        bool bat = cc != null && cc.enabled;
        if (bat) cc.enabled = false;
        p.y = VfxFactory.GroundY(p) + 0.05f;
        t.position = p;
        if (bat) cc.enabled = true;
        Physics.SyncTransforms();
    }

    /// <summary>Diem dat duoc (tren dat, trong ban do) theo huong goc / khoang cach tu nhan vat.</summary>
    static Vector3 Cho(Vector3 tam, float goc, float kc)
    {
        float a = goc * Mathf.Deg2Rad;
        var p = tam + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * kc;
        p.y = VfxFactory.GroundY(p);
        return p;
    }

    static bool DangAn(Component c)
    {
        bool coRenderer = false;
        foreach (var r in c.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            coRenderer = true;
            if (!r.forceRenderingOff) return false;
        }
        return coRenderer;
    }

    // ---------- do anh ----------
    static Texture2D Ve(Camera cam, RenderTexture rt)
    {
        cam.targetTexture = rt; cam.Render();
        var tx = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        var cu = RenderTexture.active; RenderTexture.active = rt;
        tx.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tx.Apply();
        RenderTexture.active = cu;
        return tx;
    }

    static Texture2D Doc(RenderTexture rt)
    {
        var tx = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        var cu = RenderTexture.active; RenderTexture.active = rt;
        tx.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tx.Apply();
        RenderTexture.active = cu;
        return tx;
    }

    /// <summary>Do sang trung binh cac diem anh khac nen (khong den tuyet doi) trong vanh [r0, r1] m quanh tam anh.</summary>
    static float DoSangVanh(Texture2D tx, float metMoiPx, float r0, float r1, out int so)
    {
        double tong = 0; so = 0;
        int w = tx.width, h = tx.height; var px = tx.GetPixels32();
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - w / 2f) * metMoiPx, dy = (y - h / 2f) * metMoiPx;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d < r0 || d > r1) continue;
                var c = px[y * w + x];
                if (c.a == 0 || (c.r | c.g | c.b) == 0) continue;
                tong += 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b; so++;
            }
        return so > 0 ? (float)(tong / so / 255.0) : 0f;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator KichBan()
    {
        TamNhin.BatTrongPhepThu = true;
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while ((dir == null || TamNhin.Hien == null) && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (dir == null || TamNhin.Hien == null) { Ghi("[LOI] khong co GameDirector / TamNhin"); loi++; Ket(); yield break; }
        dir.enabled = false;
        var toi = dir.player.GetComponent<Damageable>();
        var pc = dir.player.GetComponent<PlayerController>();
        toi.maxHealth = toi.health = 1e7f;
        Vector3 P = dir.player.position;
        var rig = Object.FindAnyObjectByType<CameraRig>();

        // ===== A =====
        Ghi(string.Format("A. _TN_Bat {0}, ban kinh {1}, mep {2}; vat lieu Standard da doi sang ChuanSuong: {3}; tam suong dia hinh: {4}; ThayDuoc(nhan vat) {5}",
            Shader.GetGlobalFloat("_TN_Bat"), Shader.GetGlobalFloat("_TN_BanKinh"), Shader.GetGlobalFloat("_TN_Mem"), TamNhin.SoVatLieuDaDoi,
            TamNhin.TamSuongDat != null, TamNhin.ThayDuoc(P)));
        Kiem(Mathf.Approximately(Shader.GetGlobalFloat("_TN_Bat"), 1f) && Mathf.Approximately(Shader.GetGlobalFloat("_TN_BanKinh"), 25f), "bien toan cuc sai");
        Kiem(TamNhin.SoVatLieuDaDoi >= 4 && TamNhin.TamSuongDat != null, "chua doi vat lieu Standard / thieu tam suong dia hinh");
        Kiem(TamNhin.ThayDuoc(P) && TamNhin.ThayDuoc(Cho(P, 0, 24f)) && !TamNhin.ThayDuoc(Cho(P, 0, 26f)), "ThayDuoc khong dung ban kinh 25 m");

        // ===== B: anh that =====
        var goCam = new GameObject("TAM_CamDo"); var cam = goCam.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 50f; cam.transform.position = P + Vector3.up * 70f; cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.clear; cam.nearClipPlane = 1f; cam.farClipPlane = 200f; cam.enabled = false;
        var rt = new RenderTexture(400, 400, 24);
        float mpx = 100f / 400f;
        string[] lop = { "Ground", "Default" };
        foreach (var tenLop in lop)
        {
            cam.cullingMask = LayerMask.GetMask(tenLop);
            yield return new WaitForEndOfFrame();
            Shader.SetGlobalFloat("_TN_Bat", 0f);
            var tTat = Ve(cam, rt);
            Shader.SetGlobalFloat("_TN_Bat", 1f);
            var tBat = Ve(cam, rt);
            int n1, n2, n3, n4;
            float trongTat = DoSangVanh(tTat, mpx, 6f, 20f, out n1), trongBat = DoSangVanh(tBat, mpx, 6f, 20f, out n2);
            float ngoaiTat = DoSangVanh(tTat, mpx, 31f, 45f, out n3), ngoaiBat = DoSangVanh(tBat, mpx, 31f, 45f, out n4);
            float kTrong = trongBat / Mathf.Max(1e-4f, trongTat), kNgoai = ngoaiBat / Mathf.Max(1e-4f, ngoaiTat);
            Ghi(string.Format("B. lop {0}: vanh 6-20 m sang {1:F3} -> {2:F3} (x{3:F2}, {4} diem); vanh 31-45 m {5:F3} -> {6:F3} (x{7:F2}, {8} diem)",
                tenLop, trongTat, trongBat, kTrong, n1, ngoaiTat, ngoaiBat, kNgoai, n3));
            Kiem(n1 > 200 && n3 > 200, "lop " + tenLop + ": qua it diem anh de do");
            Kiem(Mathf.Abs(kTrong - 1f) < 0.04f, "lop " + tenLop + ": trong tam nhin bi doi mau");
            Kiem(kNgoai < 0.6f && kNgoai > 0.12f, "lop " + tenLop + ": ngoai tam nhin khong toi kieu SC2 (phai toi han nhung con thay)");
            File.WriteAllBytes("PlayTestShots/tamnhin_tren_xuong_" + tenLop + "_bat.png", tBat.EncodeToPNG());
            Object.Destroy(tTat); Object.Destroy(tBat);
        }

        // ===== C: suong khoang cach cu con dung mot lan =====
        {
            // tam thu dat xa nhan vat - tat phan hinh (an vat dong) trong luc do, chi do suong khoang cach cua Unity
            TamNhin.BatTrongPhepThu = false; yield return null; yield return null;
            int lop31 = 31;
            cam.orthographic = false; cam.fieldOfView = 20f; cam.cullingMask = 1 << lop31;
            cam.transform.position = new Vector3(0f, 300f, 0f); cam.transform.rotation = Quaternion.identity;
            float kc = 40f;
            string[] shader = { "Standard", "Diablo25D/DaMoTriplanar", "Diablo25D/ChuanSuong" };
            var anh = new Texture2D(2, 2); anh.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white }); anh.Apply();
            bool coFog = RenderSettings.fog; Color fc = RenderSettings.fogColor;
            float mongK = Mathf.Exp(-Mathf.Pow(RenderSettings.fogDensity * kc, 2f));
            foreach (var sh in shader)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad); q.name = "TAM_QuadSuong"; q.layer = lop31;
                Object.Destroy(q.GetComponent<Collider>());
                q.transform.position = cam.transform.position + Vector3.forward * kc; q.transform.localScale = Vector3.one * 30f;
                q.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                var m = new Material(Shader.Find(sh)); m.SetColor("_Color", new Color(0.35f, 0.35f, 0.35f)); m.SetTexture("_MainTex", anh);
                var mr = q.GetComponent<MeshRenderer>(); mr.sharedMaterial = m; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var den = new GameObject("TAM_DenSuong").AddComponent<Light>(); den.type = LightType.Directional; den.cullingMask = 1 << lop31;
                den.transform.rotation = Quaternion.LookRotation(Vector3.forward); den.intensity = 0.6f;    // khong chay sang (>1 thi suong bi che)
                yield return new WaitForEndOfFrame();
                Shader.SetGlobalFloat("_TN_Bat", 0f);
                // cam.Render() goi tay KHONG an suong (doi chung Standard cung ra k = 1) - de may quay chay theo vong ve binh thuong
                cam.targetTexture = rt; cam.enabled = true;
                RenderSettings.fog = false; yield return null; yield return null; yield return new WaitForEndOfFrame(); var tTat = Doc(rt);
                RenderSettings.fog = true; yield return null; yield return null; yield return new WaitForEndOfFrame(); var tBat = Doc(rt);
                RenderSettings.fog = coFog; cam.enabled = false;
                Color a = tTat.GetPixel(200, 200), b = tBat.GetPixel(200, 200);
                float lt = 0.2126f * a.r + 0.7152f * a.g + 0.0722f * a.b, lb = 0.2126f * b.r + 0.7152f * b.g + 0.0722f * b.b;
                float lfg = 0.2126f * fc.r + 0.7152f * fc.g + 0.0722f * fc.b;
                float k = (lb - lfg) / Mathf.Max(1e-4f, lt - lfg);
                Ghi(string.Format("C. {0}: tat suong {1:F3}, bat suong {2:F3}, mau suong {3:F3} -> k {4:F3} (mong exp(-(mat do x {5} m)^2) = {6:F3}; ap hai lan = {7:F3})",
                    sh, lt, lb, lfg, k, kc, mongK, mongK * mongK));
                Kiem(Mathf.Abs(k - mongK) < 0.06f, sh + ": suong khoang cach khong ap dung mot lan");
                Object.Destroy(q); Object.Destroy(den.gameObject); Object.Destroy(tTat); Object.Destroy(tBat);
            }
        }
        Object.Destroy(goCam); rt.Release();
        TamNhin.BatTrongPhepThu = true;
        yield return null; yield return null;

        // ===== D: an vat dong =====
        dir.SinhDotQuanhNguoi();
        yield return new WaitForSeconds(0.5f);
        var quai = new List<Damageable>();
        foreach (var ai in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            ai.enabled = false; var d = ai.GetComponent<Damageable>(); d.maxHealth = d.health = 1e7f; quai.Add(d);
        }
        Kiem(quai.Count >= 4, "khong du quai thu");
        float[] kcQ = { 20f, 30f, 60f };
        for (int i = 0; i < quai.Count; i++) DatCho(quai[i].transform, Cho(P, 37f * i, i < 3 ? kcQ[i] : 90f));
        var hat10 = new GameObject("TAM_Hat10").AddComponent<ParticleSystem>(); hat10.transform.position = Cho(P, 200, 10f) + Vector3.up;
        var hat35 = new GameObject("TAM_Hat35").AddComponent<ParticleSystem>(); hat35.transform.position = Cho(P, 200, 35f) + Vector3.up;
        foreach (var hs in new[] { hat10, hat35 })
        {
            var mn = hs.main; mn.startSpeed = 0f; mn.startLifetime = 5f; mn.startSize = 0.5f;
            var sh = hs.shape; sh.enabled = false; hs.Emit(5);
        }
        var den5 = new GameObject("TAM_Den5").AddComponent<Light>(); den5.type = LightType.Point; den5.range = 4f; den5.transform.position = Cho(P, 120, 5f) + Vector3.up;
        var den45 = new GameObject("TAM_Den45").AddComponent<Light>(); den45.type = LightType.Point; den45.range = 4f; den45.transform.position = Cho(P, 120, 45f) + Vector3.up;
        int mn5 = den5.cullingMask, mn45 = den45.cullingMask;
        yield return new WaitForSeconds(0.5f);
        TamNhin.Hien.QuetNgay();
        Ghi(string.Format("D. quai 20 / 30 / 60 m: an {0} / {1} / {2} (mong False / True / True); hat sinh luc chay 10 / 35 m: an {3} / {4}; den 5 / 45 m: mat na {5} / {6} (goc {7}); dang an {8} renderer, tat {9} den",
            DangAn(quai[0]), DangAn(quai[1]), DangAn(quai[2]), hat10.GetComponent<Renderer>().forceRenderingOff, hat35.GetComponent<Renderer>().forceRenderingOff,
            den5.cullingMask, den45.cullingMask, mn5, TamNhin.SoRendererDangAn, TamNhin.SoDenDangTat));
        Kiem(!DangAn(quai[0]) && DangAn(quai[1]) && DangAn(quai[2]), "quai ngoai 25 m khong bi an / trong 25 m bi an");
        Kiem(!hat10.GetComponent<Renderer>().forceRenderingOff && hat35.GetComponent<Renderer>().forceRenderingOff, "hieu ung ngoai tam nhin khong bi an");
        // den 5 m: DenMatDat tu bo lop Ground khoi den goc (-1 -> -257) - chi can KHONG bi tat (0)
        Kiem(den5.cullingMask != 0 && den45.cullingMask == 0, "den ngoai tam nhin khong tat / den trong tam bi tat");
        Kiem(!TamNhin.ThayDuoc(quai[1].transform.position + Vector3.up * 2.1f), "so sat thuong / ten tren dau o 30 m van ve");
        // quai di vao tam nhin -> hien lai
        DatCho(quai[1].transform, Cho(P, 37f, 15f)); TamNhin.Hien.QuetNgay();
        Ghi("D2. quai 30 m di vao 15 m: an " + DangAn(quai[1]));
        Kiem(!DangAn(quai[1]), "quai vao tam nhin khong hien lai");

        // D3. lua lo (hat cong sang cua canh vat - shader khong phu suong duoc): ngoai tam an, trong tam hien
        {
            LoLuaDa gan = null, xa = null; float dGan = 1e9f, dXa = 0f;
            foreach (var lo in Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None))
            {
                float d = Ngang(lo.transform.position, P);
                if (d < dGan) { dGan = d; gan = lo; }
                if (d > dXa) { dXa = d; xa = lo; }
            }
            if (gan != null && dGan > 18f) { DatCho(dir.player, gan.transform.position + new Vector3(4f, 0f, 4f)); P = dir.player.position; dGan = Ngang(gan.transform.position, P); }
            yield return null; TamNhin.Hien.QuetNgay();
            int hatGanHien = 0, hatGan = 0, hatXaAn = 0, hatXa = 0;
            if (gan != null) foreach (var r in gan.GetComponentsInChildren<ParticleSystemRenderer>()) { hatGan++; if (!r.forceRenderingOff) hatGanHien++; }
            if (xa != null) foreach (var r in xa.GetComponentsInChildren<ParticleSystemRenderer>()) { hatXa++; if (r.forceRenderingOff) hatXaAn++; }
            Ghi(string.Format("D3. lo lua gan ({0:F0} m): hat hien {1}/{2}; lo xa ({3:F0} m): hat an {4}/{5}", dGan, hatGanHien, hatGan, dXa, hatXaAn, hatXa));
            Kiem(hatGan > 0 && hatGanHien == hatGan && hatXa > 0 && hatXaAn == hatXa, "lua lo ngoai tam nhin khong an / trong tam bi an");
        }

        // D4. chi phi CPU moi luot quet (chay 5 lan / giay) - Editor, de so sanh (dien thoai cham hon vai lan)
        {
            int soR = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 20; i++) TamNhin.Hien.QuetNgay();
            sw.Stop();
            Ghi(string.Format("D4. mot luot quet: {0:F2} ms (TB 20 lan, {1} renderer trong canh) -> {2:F2} ms / giay", sw.Elapsed.TotalMilliseconds / 20.0, soR, sw.Elapsed.TotalMilliseconds / 20.0 * 5));
        }

        // ===== E: dong doi =====
        sbyte doiCu = toi.doi; toi.doi = 0;
        var dd = quai[3]; bool laNguoiCu = dd.isPlayer; dd.isPlayer = true; dd.doi = 0; dir.moiNguoi.Add(dd.transform);
        DatCho(dd.transform, Cho(P, 300f, 40f));
        var canhDd = quai[2]; DatCho(canhDd.transform, dd.transform.position + new Vector3(4f, 0f, 0f));
        yield return null; TamNhin.Hien.QuetNgay();
        bool thayCanh = !DangAn(canhDd), thayDd = !DangAn(dd);
        bool botThay = TamNhin.NhinThay(toi, canhDd.transform.position);
        Ghi(string.Format("E. dong doi o 40 m: thay dong doi {0}, thay quai canh dong doi (44 m tu minh) {1}; NhinThay (logic BOT) {2}", thayDd, thayCanh, botThay));
        Kiem(thayDd && thayCanh && botThay, "khong chia se tam nhin dong doi");
        dir.moiNguoi.Remove(dd.transform); dd.isPlayer = laNguoiCu; dd.doi = -1;
        yield return null; TamNhin.Hien.QuetNgay();
        bool sauBo = DangAn(canhDd);
        Ghi("E2. doi chung bo dong doi: quai do an " + sauBo);
        Kiem(sauBo, "doi chung: khong co dong doi van thay quai o 44 m");
        toi.doi = doiCu;

        // ===== G: anh may quay game keo xa =====
        foreach (var q in quai) if (q != null) DatCho(q.transform, Cho(P, Random.Range(0f, 360f), Random.Range(8f, 45f)));
        TamNhin.Hien.QuetNgay();
        if (rig != null) rig.enabled = false;
        var camG = Camera.main;
        if (camG != null)
        {
            camG.transform.position = P + new Vector3(-28f, 34f, -28f); camG.transform.LookAt(P);
            yield return new WaitForSeconds(0.3f); yield return Chup("tamnhin_25d_keo_xa");
            camG.transform.position = P + new Vector3(0f, 9f, -26f); camG.transform.LookAt(P + new Vector3(0f, 0f, 10f));
            yield return new WaitForSeconds(0.3f); yield return Chup("tamnhin_3d_keo_xa");
            TamNhin.BatTrongPhepThu = false; yield return new WaitForSeconds(1.3f);
            yield return Chup("tamnhin_25d_doi_chung_tat");
            TamNhin.BatTrongPhepThu = true; yield return new WaitForSeconds(1.3f);
        }

        // ===== F: chet =====
        toi.health = 1f; toi.TakeDamage(1e9f, DamageType.Physical, Vector3.zero);
        yield return new WaitForSeconds(0.5f);
        Vector3 xac = dir.player.position;
        Ghi(string.Format("F. da chet {0}: thay 10 m quanh xac {1}, 30 m {2}", toi.IsDead, TamNhin.ThayDuoc(Cho(xac, 0, 10f)), TamNhin.ThayDuoc(Cho(xac, 0, 30f))));
        Kiem(toi.IsDead && TamNhin.ThayDuoc(Cho(xac, 0, 10f)) && !TamNhin.ThayDuoc(Cho(xac, 0, 30f)), "chet roi khong giu tam nhin quanh xac");

        Object.Destroy(hat10.gameObject); Object.Destroy(hat35.gameObject); Object.Destroy(den5.gameObject); Object.Destroy(den45.gameObject);
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Ket()
    {
        TamNhin.BatTrongPhepThu = false;
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/tamnhin.txt", bao.ToString());
        var rac = GameObject.Find("TAM_TamNhin");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
