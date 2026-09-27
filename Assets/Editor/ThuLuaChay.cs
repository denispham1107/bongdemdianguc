using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: LUA CHAY TOAN THAN (menu 90, 28/09/2026).
///
/// Nguoi dung: "chi vang ra cac dom chay nho qua don so" -> toan than boc chay boi lua that, sap het chay thi lua giam dan roi
/// bien mat. Do bang so, doc lap voi cach LuaToanThan chon diem phat:
///   A. PHU THAN: nuong hinh that cua nhan vat (SkinnedMeshRenderer.BakeMesh), lay mau dinh; ti le dinh co ngon lua trong
///      0,35 m ("do phu") va ti le ngon lua nam sat than. DOI CHUNG: hinh lua cu (VfxFactory.BuildBurning) tren con cung loai.
///   B. GIAM DAN: dem hat lua + co trung binh suot lan chay 3,5 s; giay cuoi phai it / nho hon luc chay manh, het gio
///      thi con hat (khong tat mot phat) va ve 0 trong ~1 s, vat hinh tu xoa.
///   C. BI GO GIUA CHUNG (nhu Toc bien / Tang hinh): xoa BurningEffect -> lua con 0,15 s sau, het trong 1,5 s.
///   D. NHOM LAI khi dang tat dan: Apply them -> do manh ve 1.
///   Anh: PlayTestShots/lua_chay_*.png. Ket qua: PlayTestShots/lua_chay.txt.
/// </summary>
public static class ThuLuaChay
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/90. Chay thu LUA CHAY TOAN THAN (nguoi choi + quai)", false, 179)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_LuaChay");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[LuaChay] " + s); }
    static void Kiem(bool dung, string loiNeuSai) { if (!dung) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static float DatY(Vector3 p)
    {
        var t = Terrain.activeTerrain;
        return t != null ? t.SampleHeight(p) + t.transform.position.y : 0f;
    }

    static IEnumerator KichBan()
    {
        PlayerController pc = null;
        float han = Time.time + 25f;
        while (pc == null && Time.time < han) { pc = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        var cam = Camera.main;
        if (pc == null || cam == null) { Ghi("[LOI] thieu nhan vat / camera"); loi++; Ket(); yield break; }
        var dir = Object.FindAnyObjectByType<GameDirector>();
        if (dir != null) dir.enabled = false;
        var rig = cam.GetComponent<CameraRig>();
        if (rig != null) rig.enabled = false;
        var mauPc = pc.GetComponent<Damageable>();

        // Cho dat trong: tim quanh (0, -10) mot diem khong vuong vat can
        Vector3 p = new Vector3(0f, 0f, -10.5f);
        p.y = DatY(p);
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = p + Vector3.up * 0.05f;
        pc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        if (cc != null) cc.enabled = true;
        pc.enabled = false;           // dung yen, khong doc input

        // Hai quai that (bo xuong, quy cay) + hai con doi chung cung loai mang hinh lua CU
        var loai = new[] { MonsterType.Skeleton, MonsterType.QuyCay };
        var moi = new List<Damageable>(); var cu = new List<Damageable>();
        for (int i = 0; i < loai.Length; i++)
        {
            for (int b = 0; b < 2; b++)
            {
                var q = p + new Vector3(-2.2f + i * 4.4f, 0f, b == 0 ? -0.3f : 4.5f);
                q.y = DatY(q);
                var go = EnemyFactory.Spawn(loai[i], q, null, pc.transform);
                go.name = "TAM_Quai_" + loai[i] + (b == 0 ? "_moi" : "_cu");
                go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                var ai = go.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
                var d = go.GetComponent<Damageable>();
                d.maxHealth = 100000f; d.health = 100000f;
                (b == 0 ? moi : cu).Add(d);
            }
        }
        yield return new WaitForSeconds(0.6f);

        // ================= A. PHU THAN =================
        var tatCa = new List<Damageable> { mauPc }; tatCa.AddRange(moi);
        // Chieu cao THAT khi dang dung (doi chung doc lap cho cach LuaToanThan do chieu cao theo xuong)
        var caoThat = new Dictionary<Damageable, float>();
        foreach (var d in tatCa) { float lo, hi; KhungDoc(d, out lo, out hi); caoThat[d] = hi - lo; }
        foreach (var d in tatCa) BurningEffect.Apply(d, 0.01f, 3.5f, null);
        // AddComponent mang remaining mac dinh 4 s, Apply lay Max -> lan chay that dai bao nhieu thi DOC ra, khong gia dinh 3,5
        float giayChay = mauPc.GetComponent<BurningEffect>().remaining;
        // Doi chung: hinh cu gan thang (khong qua BurningEffect)
        var hinhCu = new List<GameObject>();
        foreach (var d in cu)
        {
            float h = d.rig != null ? d.rig.bodyHeight : 1.8f, r = d.rig != null ? d.rig.bodyRadius : 0.35f;
            hinhCu.Add(VfxFactory.BuildBurning(d.transform, h, r * 1.15f));
        }
        float t0 = Time.time;
        while (Time.time - t0 < 1.1f) yield return null;

        cam.transform.position = p + new Vector3(0f, 4.2f, -6.2f);
        cam.transform.LookAt(p + new Vector3(0f, 1.0f, 1.5f));
        yield return null;
        Chup(cam, "PlayTestShots/lua_chay_gan.png");
        // Anh RIENG tung lop hat cua nguoi choi (soi lop nao ve ra gi)
        var lopPc = mauPc.GetComponentInChildren<LuaToanThan>();
        if (lopPc != null)
        {
            var rs = lopPc.GetComponentsInChildren<ParticleSystemRenderer>();
            var hc = new List<ParticleSystemRenderer>();
            foreach (var d in moi) hc.AddRange(d.GetComponentsInChildren<ParticleSystemRenderer>());
            foreach (var d in cu) hc.AddRange(d.GetComponentsInChildren<ParticleSystemRenderer>());
            foreach (var x in hc) x.enabled = false;
            for (int i = 0; i < rs.Length; i++)
            {
                for (int j = 0; j < rs.Length; j++) rs[j].enabled = i == j;
                yield return null;
                Chup(cam, "PlayTestShots/lua_chay_rieng_" + rs[i].name + ".png");
            }
            foreach (var x in rs) x.enabled = true;
            foreach (var x in hc) x.enabled = true;
        }
        cam.transform.position = p + new Vector3(0f, 13f, -11.5f);
        cam.transform.LookAt(p + new Vector3(0f, 0f, 1.5f));
        yield return null;
        Chup(cam, "PlayTestShots/lua_chay_goc_choi.png");

        foreach (var d in tatCa)
        {
            var l = d.GetComponentInChildren<LuaToanThan>();
            Kiem(l != null, d.name + ": khong co LuaToanThan");
            if (l == null) continue;
            float phu, sat, phuMin; int nHat; string dai;
            DoPhu(d, l.HatLua, out phu, out sat, out phuMin, out nHat, out dai);
            Ghi(string.Format("A. {0}: {1} khuc xuong, than cao theo xuong {2:F2} m / hinh that {3:F2} m, {4} ngon lua, DO PHU {5:P0} (dai kem nhat {6:P0}; chan->dau: {7}), {8:P0} ngon lua sat than (<0,3 m)",
                d.name, l.SoKhucXuong, l.ChieuCaoThan, caoThat[d], nHat, phu, phuMin, dai, sat));
            Kiem(l.SoKhucXuong == 16, d.name + ": khong tim du 16 khuc xuong");
            Kiem(Mathf.Abs(l.ChieuCaoThan / caoThat[d] - 1f) < 0.15f, d.name + ": chieu cao than do theo xuong lech hinh that qua 15%");
            Kiem(phuMin >= 0.5f, d.name + ": co mot dai than (chan / dui / bung / nguc / dau) gan nhu khong co lua");
            Kiem(sat >= 0.6f, d.name + ": lua khong nam tren than");
        }
        foreach (var d in cu)
        {
            var g = d.transform.Find("Burning");
            var ps = g != null ? g.Find("Flames").GetComponent<ParticleSystem>() : null;
            float phu, sat, phuMin; int nHat; string dai;
            DoPhu(d, ps, out phu, out sat, out phuMin, out nHat, out dai);
            Ghi(string.Format("A. DOI CHUNG hinh cu {0}: {1} hat, do phu {2:P0} (dai kem nhat {3:P0}; chan->dau: {4}), sat than {5:P0}", d.name, nHat, phu, phuMin, dai, sat));
            Kiem(phuMin < 0.5f, "doi chung hinh cu lai phu du moi dai than - phep do khong phan biet duoc");
        }
        foreach (var g in hinhCu) if (g != null) Object.Destroy(g);

        // ================= B. GIAM DAN (lan chay bat dau o t0, dai 3,5 s) =================
        var lPc = mauPc.GetComponentInChildren<LuaToanThan>();
        var nhan = new List<float>(); var dem = new List<int>(); var co = new List<float>();
        var hat = new ParticleSystem.Particle[400];
        bool chupTat = false;
        while (Time.time - t0 < giayChay + 1.6f)
        {
            float tt = Time.time - t0;
            if (lPc == null) { nhan.Add(tt); dem.Add(-1); co.Add(0f); }
            else
            {
                int n = lPc.HatLua.GetParticles(hat);
                float s = 0f; for (int i = 0; i < n; i++) s += hat[i].GetCurrentSize(lPc.HatLua);
                nhan.Add(tt); dem.Add(n); co.Add(n > 0 ? s / n : 0f);
            }
            if (!chupTat && tt >= giayChay - 0.45f)
            {
                chupTat = true;
                cam.transform.position = p + new Vector3(0f, 4.2f, -6.2f);
                cam.transform.LookAt(p + new Vector3(0f, 1.0f, 1.5f));
                Chup(cam, "PlayTestShots/lua_chay_tat_dan.png");
            }
            yield return null;
        }
        float G = giayChay;
        float tbManh = TrungBinh(nhan, dem, 1.0f, G - 1.3f), tbCuoi = TrungBinh(nhan, dem, G - 0.3f, G), sauHet = TrungBinh(nhan, dem, G + 0.05f, G + 0.2f);
        float coManh = TrungBinh(nhan, co, 1.0f, G - 1.3f), coCuoi = TrungBinh(nhan, co, G - 0.3f, G);
        float tHet = -1f; for (int i = 0; i < nhan.Count; i++) if (nhan[i] > G && dem[i] <= 0) { tHet = nhan[i]; break; }
        var sb = new StringBuilder("B. so ngon lua theo thoi gian (s:hat):");
        for (float m = 0.25f; m < giayChay + 1.6f; m += 0.25f) sb.Append(string.Format(" {0:F2}:{1:F0}", m, TrungBinh(nhan, dem, m - 0.06f, m + 0.06f)));
        Ghi(sb.ToString());
        Ghi(string.Format("B. lan chay dai {7:F2} s | chay manh (1,0 s -> {8:F2} s) TB {0:F1} hat co {1:F2} m | 0,3 s cuoi TB {2:F1} hat co {3:F2} m | 0,05-0,2 s sau khi het {4:F1} hat | ve 0 luc {5:F2} s; vat hinh con: {6}",
            tbManh, coManh, tbCuoi, coCuoi, sauHet, tHet, lPc != null, G, G - 1.3f));
        Kiem(tbCuoi < 0.6f * tbManh, "giay cuoi lua khong thua di");
        Kiem(coCuoi < 0.9f * coManh, "giay cuoi ngon lua khong nho di");
        Kiem(sauHet > 0.5f, "het gio lua tat mot phat (khong con hat nao ngay sau)");
        Kiem(tHet > G && tHet < G + 1.2f, "lua khong tan het trong ~1 s sau khi het chay");
        Kiem(lPc == null, "vat hinh lua khong tu xoa sau khi tat");
        Kiem(mauPc.GetComponent<BurningEffect>() == null, "BurningEffect khong het");

        // ================= C. BI GO GIUA CHUNG =================
        var dC = moi[0];
        BurningEffect.Apply(dC, 0.01f, 6f, null);
        float tc = Time.time; while (Time.time - tc < 1.2f) yield return null;
        var lC = dC.GetComponentInChildren<LuaToanThan>();
        int nTruoc = lC != null ? lC.HatLua.particleCount : 0;
        Object.Destroy(dC.GetComponent<BurningEffect>());      // nhu TocBien.GoSachTrangThai
        tc = Time.time; while (Time.time - tc < 0.15f) yield return null;
        int nSau015 = lC != null ? lC.HatLua.particleCount : 0;
        tc = Time.time;
        float giayHetLua = -1f;
        while (lC != null && Time.time - tc < 4f)
        {
            if (giayHetLua < 0f && lC.HatLua.particleCount == 0) giayHetLua = Time.time - tc + 0.15f;
            yield return null;
        }
        float giayHet = Time.time - tc + 0.15f;
        Ghi(string.Format("C. go giua chung: {0} ngon lua -> 0,15 s sau {1} -> het ngon lua sau {2:F2} s, vat hinh (cho khoi tan) xoa sau {3:F2} s", nTruoc, nSau015, giayHetLua, giayHet));
        Kiem(nTruoc > 10 && nSau015 > 0, "bi go thi lua bien mat mot phat");
        Kiem(giayHetLua > 0.3f && giayHetLua < 1.5f, "bi go ma lua khong tu tat trong 1,5 s");
        Kiem(lC == null && giayHet < 2.8f, "vat hinh lua khong tu xoa sau khi khoi tan");

        // ================= D. NHOM LAI khi dang tat dan =================
        var dD = moi[1];
        BurningEffect.Apply(dD, 0.01f, 1.5f, null);
        var beD = dD.GetComponent<BurningEffect>();
        tc = Time.time; while (beD != null && beD.remaining > 0.4f && Time.time - tc < 6f) yield return null;
        var lD = dD.GetComponentInChildren<LuaToanThan>();
        float kTruoc = lD != null ? lD.DoManh : -1f;
        BurningEffect.Apply(dD, 0.01f, 3f, null);
        yield return null; yield return null;
        float kSau = lD != null ? lD.DoManh : -1f;
        Ghi(string.Format("D. dang tat dan do manh {0:F2} -> chay them 3 s: {1:F2}", kTruoc, kSau));
        Kiem(kTruoc > 0f && kTruoc < 0.5f && kSau > 0.99f, "nhom lai khong dua lua ve manh");
        Object.Destroy(dD.GetComponent<BurningEffect>());

        // ================= E. NHIP CHAY KHONG PHUN TIA TRUNG DON =================
        // Dem chum tia trung don (Vfx_TrungDon / HitBurst) sinh ra: 2 s chay (4 nhip) va DOI CHUNG 1 don lua thuong.
        tc = Time.time; while (Time.time - tc < 1.5f) yield return null;
        var dE = moi[0];
        var daCo = new HashSet<Transform>();
        foreach (var t in Object.FindObjectsByType<Transform>()) if (t.parent == null) daCo.Add(t);
        float mauTruoc = dE.health;
        BurningEffect.Apply(dE, 5f, 2.2f, null);
        int chumChay = 0;
        tc = Time.time;
        while (Time.time - tc < 2.0f)
        {
            foreach (var t in Object.FindObjectsByType<Transform>())
                if (t.parent == null && (t.name.Contains("TrungDon") || t.name == "Hit") && daCo.Add(t)) chumChay++;
            yield return null;
        }
        float mauMat = mauTruoc - dE.health;
        dE.TakeDamage(10f, DamageType.Fire, dE.transform.position + Vector3.up);
        int chumDon = 0;
        yield return null;
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t.parent == null && (t.name.Contains("TrungDon") || t.name == "Hit") && daCo.Add(t)) chumDon++;
        Ghi(string.Format("E. 2 s chay: mat {0:F1} mau, {1} chum tia trung don | DOI CHUNG 1 don lua thuong: {2} chum", mauMat, chumChay, chumDon));
        Kiem(mauMat > 5f, "chay khong con tru mau");
        Kiem(chumChay == 0, "nhip chay van phun tia trung don");
        Kiem(chumDon >= 1, "doi chung: don thuong khong sinh chum tia - phep dem khong bat duoc gi");
        Object.Destroy(dE.GetComponent<BurningEffect>());

        tc = Time.time; while (Time.time - tc < 1.5f) yield return null;
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static float TrungBinh(List<float> t, List<int> v, float a, float b)
    {
        float s = 0f; int n = 0;
        for (int i = 0; i < t.Count; i++) if (t[i] >= a && t[i] <= b && v[i] >= 0) { s += v[i]; n++; }
        return n > 0 ? s / n : -1f;
    }
    static float TrungBinh(List<float> t, List<float> v, float a, float b)
    {
        float s = 0f; int n = 0;
        for (int i = 0; i < t.Count; i++) if (t[i] >= a && t[i] <= b) { s += v[i]; n++; }
        return n > 0 ? s / n : -1f;
    }

    /// <summary>
    /// Do phu tren HINH THAT: BakeMesh cua moi SkinnedMeshRenderer (dinh the gioi), lay mau toi da 600 dinh. phu = ti le dinh
    /// co ngon lua trong 0,35 m; sat = ti le ngon lua cach dinh gan nhat duoi 0,3 m. Diem ngon lua = tam hat + pivot (chan lua).
    /// </summary>
    static void KhungDoc(Damageable d, out float lo, out float hi)
    {
        lo = 1e9f; hi = -1e9f;
        foreach (var smr in d.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var m = new Mesh();
            smr.BakeMesh(m, true);
            foreach (var v in m.vertices) { float y = smr.transform.TransformPoint(v).y; if (y < lo) lo = y; if (y > hi) hi = y; }
            Object.DestroyImmediate(m);
        }
    }

    static void DoPhu(Damageable d, ParticleSystem ps, out float phu, out float sat, out float phuMin, out int nHat, out string dai)
    {
        phu = 0f; sat = 0f; nHat = 0; phuMin = 0f; dai = "";
        if (ps == null) return;
        var dinh = new List<Vector3>();
        foreach (var smr in d.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var m = new Mesh();
            smr.BakeMesh(m, true);
            var v = m.vertices;
            int buoc = Mathf.Max(1, v.Length / 600);
            for (int i = 0; i < v.Length; i += buoc) dinh.Add(smr.transform.TransformPoint(v[i]));
            Object.DestroyImmediate(m);
        }
        var hat = new ParticleSystem.Particle[ps.main.maxParticles];
        nHat = ps.GetParticles(hat);
        if (dinh.Count == 0 || nHat == 0) return;
        // Chia than thanh 5 DAI theo chieu cao hinh that (chan, dui, bung, nguc, dau): lua that phai co o moi dai.
        float lo = 1e9f, hi = -1e9f;
        foreach (var x in dinh) { if (x.y < lo) lo = x.y; if (x.y > hi) hi = x.y; }
        int[] tong = new int[5], co = new int[5];
        int coLua = 0;
        foreach (var x in dinh)
        {
            int b = Mathf.Clamp((int)((x.y - lo) / Mathf.Max(0.01f, hi - lo) * 5f), 0, 4);
            tong[b]++;
            for (int i = 0; i < nHat; i++)
                if ((hat[i].position - x).sqrMagnitude < 0.35f * 0.35f) { coLua++; co[b]++; break; }
        }
        phuMin = 1f;
        for (int b = 0; b < 5; b++)
        {
            float f = tong[b] > 0 ? (float)co[b] / tong[b] : 1f;
            if (f < phuMin) phuMin = f;
            dai += (b > 0 ? " " : "") + f.ToString("P0");
        }
        int satThan = 0;
        for (int i = 0; i < nHat; i++)
        {
            float best = 1e9f;
            foreach (var x in dinh) { float q = (hat[i].position - x).sqrMagnitude; if (q < best) best = q; }
            if (best < 0.3f * 0.3f) satThan++;
        }
        phu = (float)coLua / dinh.Count;
        sat = (float)satThan / nHat;
    }

    static void Chup(Camera cam, string duong)
    {
        var rt = RenderTexture.GetTemporary(960, 540, 24, RenderTextureFormat.ARGB32);
        var cuRt = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = cuRt;
        var cu = RenderTexture.active;
        RenderTexture.active = rt;
        var tx = new Texture2D(960, 540, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
        tx.Apply();
        RenderTexture.active = cu;
        RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(duong, tx.EncodeToPNG());
        Object.DestroyImmediate(tx);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/lua_chay.txt", bao.ToString());
        foreach (var go in Object.FindObjectsByType<Transform>())
            if (go != null && go.parent == null && go.name.StartsWith("TAM_")) Object.Destroy(go.gameObject);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
        if (!string.IsNullOrEmpty(canhCu)) EditorApplication.update += TraLaiCanh;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
