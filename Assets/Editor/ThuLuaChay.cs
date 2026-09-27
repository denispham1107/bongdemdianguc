using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: LUA CHAY TOAN THAN (menu 90).
///
/// Lan 2 (28/09/2026, nguoi dung): lua la MOT CUM toan than; di chuyen thi lua KHONG bi bo lai phia sau; het chay thi lua
/// bien mat NGAY. Do doc lap voi cach LuaToanThan dat khoi lua:
///   A. MOT CUM + PHU THAN: so khoi lua song, do lech ngang cua moi khoi so voi xuong Hips; nuong hinh that (BakeMesh), chieu
///      dinh len mat phang nhin cua may quay, dinh nao nam trong phan co lua cua mot khoi (60% ngang x 90% doc) la "co lua";
///      chia than 5 dai chan -> dau.
///   B. DI CHUYEN 6 m/s trong 1 s: khoang cach ngang trung binh tu tam cac khoi lua (vi tri THE GIOI) toi xuong Hips.
///      DOI CHUNG: cung phep do voi LuaToanThan.DoiChungTheGioi (mo phong the gioi nhu ban cu).
///   C. TAT NGAY: het gio chay / bi go giua chung -> khung ke tiep khong con vat hinh lua nao.
///   E. Nhip chay khong phun tia trung don (doi chung 1 don lua thuong).
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
        LuaToanThan.DoiChungTheGioi = false;
        EditorApplication.update -= Nhip;
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

    static Transform Hong(Damageable d)
    {
        foreach (var smr in d.GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (var b in smr.bones) if (b != null && b.name == "Hips") return b;
        return d.transform;
    }

    static void DatCho(Component c, Vector3 p)
    {
        var cc = c.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        c.transform.position = p;
        if (cc != null) cc.enabled = true;
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

        Vector3 p = new Vector3(0f, 0f, -10.5f);
        p.y = DatY(p);
        DatCho(pc, p + Vector3.up * 0.05f);
        pc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        pc.enabled = false;

        var loai = new[] { MonsterType.Skeleton, MonsterType.QuyCay };
        var quai = new List<Damageable>();
        for (int i = 0; i < loai.Length; i++)
        {
            var q = p + new Vector3(-2.2f + i * 4.4f, 0f, -0.3f);
            q.y = DatY(q);
            var go = EnemyFactory.Spawn(loai[i], q, null, pc.transform);
            go.name = "TAM_Quai_" + loai[i];
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var ai = go.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
            var d = go.GetComponent<Damageable>();
            d.maxHealth = 100000f; d.health = 100000f;
            quai.Add(d);
        }
        yield return new WaitForSeconds(0.6f);

        // ================= A. MOT CUM + PHU THAN =================
        var tatCa = new List<Damageable> { mauPc }; tatCa.AddRange(quai);
        foreach (var d in tatCa) BurningEffect.Apply(d, 0.01f, 3.5f, null);
        float giayChay = mauPc.GetComponent<BurningEffect>().remaining;   // AddComponent mang san 4 s (xem HUONG-DAN)
        float t0 = Time.time;
        while (Time.time - t0 < 1.2f) yield return null;

        cam.transform.position = p + new Vector3(0f, 4.2f, -6.2f);
        cam.transform.LookAt(p + new Vector3(0f, 1.0f, 0f));
        yield return null;
        Chup(cam, "PlayTestShots/lua_chay_gan.png");
        foreach (var d in tatCa)
        {
            var l = d.GetComponentInChildren<LuaToanThan>();
            Kiem(l != null, d.name + ": khong co LuaToanThan");
            if (l == null) continue;
            int soKhoi; float lechMax; float phuMin; string dai;
            DoCum(d, l, cam, out soKhoi, out lechMax, out phuMin, out dai);
            float lo, hi; KhungDoc(d, out lo, out hi);
            Ghi(string.Format("A. {0}: than cao theo xuong {1:F2} m / hinh that {2:F2} m; {3} khoi lua song, lech ngang toi da so voi Hips {4:F2} m; khoi {5:F2} x {6:F2} m; PHU THAN (chan->dau) {7}, dai kem nhat {8:P0}",
                d.name, l.ChieuCaoThan, hi - lo, soKhoi, lechMax, l.CoKhoi.x, l.CoKhoi.y, dai, phuMin));
            Kiem(l.CoXuong, d.name + ": khong tim thay xuong");
            Kiem(Mathf.Abs(l.ChieuCaoThan / (hi - lo) - 1f) < 0.15f, d.name + ": chieu cao theo xuong lech hinh that qua 15%");
            Kiem(soKhoi >= 2 && soKhoi <= 12, d.name + ": so khoi lua khong phai mot cum vai khoi chong nhau");
            Kiem(lechMax < 0.55f, d.name + ": co khoi lua lech khoi than (khong phai mot cum)");
            Kiem(phuMin >= 0.85f, d.name + ": co dai than khong nam trong khoi lua");
        }
        cam.transform.position = p + new Vector3(0f, 13f, -11.5f);
        cam.transform.LookAt(p);
        yield return null;
        Chup(cam, "PlayTestShots/lua_chay_goc_choi.png");

        // ================= C1. HET GIO -> TAT NGAY =================
        bool daThayHet = false; int khungSauHet = -1; bool conHinh = false;
        while (Time.time - t0 < giayChay + 1f)
        {
            bool coChay = mauPc.GetComponent<BurningEffect>() != null;
            bool coHinh = mauPc.GetComponentInChildren<LuaToanThan>() != null;
            if (!coChay && !daThayHet) { daThayHet = true; khungSauHet = 0; conHinh = coHinh; }
            else if (daThayHet && khungSauHet == 0) { khungSauHet = 1; conHinh |= coHinh; }
            yield return null;
        }
        Ghi(string.Format("C1. het chay ({0:F2} s): khung dau khong con BurningEffect -> vat hinh lua con: {1}", giayChay, conHinh));
        Kiem(daThayHet && !conHinh, "het gio chay ma lua van con");

        // ================= B. DI CHUYEN: LUA BAM THEO NGUOI =================
        var dB = quai[0];
        var hongB = Hong(dB);
        var ketQua = new float[2];
        for (int bien = 0; bien < 2; bien++)
        {
            LuaToanThan.DoiChungTheGioi = bien == 1;
            var goc = p + new Vector3(-6f, 0f, 3f); goc.y = DatY(goc);
            DatCho(dB, goc);
            dB.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            BurningEffect.Apply(dB, 0.01f, 3f, null);
            float tb = Time.time; while (Time.time - tb < 0.8f) yield return null;   // dung yen cho du khoi lua
            float tc = Time.time;
            bool daChup = false;
            while (Time.time - tc < 1.0f)
            {
                var q = dB.transform.position + Vector3.right * 6f * Time.deltaTime;
                q.y = DatY(q);
                DatCho(dB, q);
                if (bien == 0 && !daChup && Time.time - tc > 0.7f)
                {
                    daChup = true;
                    cam.transform.position = q + new Vector3(0f, 4.2f, -6.2f);
                    cam.transform.LookAt(q + Vector3.up);
                    yield return new WaitForEndOfFrame();
                    Chup(cam, "PlayTestShots/lua_chay_dang_chay.png");
                }
                yield return null;
            }
            yield return new WaitForEndOfFrame();
            var l = dB.GetComponentInChildren<LuaToanThan>();
            float s = 0f; int n = 0;
            if (l != null)
                foreach (var ps in new[] { l.LopTruoc, l.LopSau })
                {
                    var hat = new ParticleSystem.Particle[ps.main.maxParticles];
                    int k = ps.GetParticles(hat);
                    for (int i = 0; i < k; i++)
                    {
                        Vector3 w = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
                        s += Vector2.Distance(new Vector2(w.x, w.z), new Vector2(hongB.position.x, hongB.position.z)); n++;
                    }
                }
            ketQua[bien] = n > 0 ? s / n : -1f;
            Ghi(string.Format("B. {0}: chay 6 m/s trong 1 s -> {1} khoi lua, tam khoi cach xuong Hips (ngang) trung binh {2:F2} m",
                bien == 0 ? "BAN SUA (cuc bo theo than)" : "DOI CHUNG mo phong the gioi (ban cu)", n, ketQua[bien]));
            Object.Destroy(dB.GetComponent<BurningEffect>());
            yield return null; yield return null;
        }
        LuaToanThan.DoiChungTheGioi = false;
        Kiem(ketQua[0] >= 0f && ketQua[0] < 0.5f, "dang chay ma lua khong bam theo nguoi");
        Kiem(ketQua[1] > 1.0f, "doi chung mo phong the gioi khong bi bo lai - phep do khong bat duoc loi cu");

        // ================= C2. BI GO GIUA CHUNG -> TAT NGAY =================
        var dC = quai[1];
        BurningEffect.Apply(dC, 0.01f, 5f, null);
        float t2 = Time.time; while (Time.time - t2 < 1.0f) yield return null;
        bool coTruoc = dC.GetComponentInChildren<LuaToanThan>() != null;
        Object.Destroy(dC.GetComponent<BurningEffect>());    // nhu TocBien.GoSachTrangThai
        yield return null;
        bool conSau = dC.GetComponentInChildren<LuaToanThan>() != null;
        Ghi(string.Format("C2. go giua chung: truoc co lua {0} -> khung ke tiep con lua {1}", coTruoc, conSau));
        Kiem(coTruoc && !conSau, "bi go giua chung ma lua khong mat ngay");

        // ================= E. NHIP CHAY KHONG PHUN TIA TRUNG DON =================
        var dE = quai[0];
        var daCo = new HashSet<Transform>();
        foreach (var t in Object.FindObjectsByType<Transform>()) if (t.parent == null) daCo.Add(t);
        float mauTruoc = dE.health;
        BurningEffect.Apply(dE, 5f, 2.2f, null);
        int chumChay = 0;
        float te = Time.time;
        while (Time.time - te < 2.0f)
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

        float tk = Time.time; while (Time.time - tk < 0.5f) yield return null;
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

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

    /// <summary>
    /// Khoi lua la billboard quay mat ve may quay: chieu dinh than va tam khoi len mat phang nhin (toa do may quay x, y).
    /// Dinh "co lua" khi nam trong phan co lua cua MOT khoi: |dx| <= 0,30 be ngang, dy trong [-0,41; +0,37] chieu cao khung
    /// (lua thay ro tu 9% den 87% khung, do tren anh).
    /// </summary>
    static void DoCum(Damageable d, LuaToanThan l, Camera cam, out int soKhoi, out float lechMax, out float phuMin, out string dai)
    {
        soKhoi = 0; lechMax = 0f; phuMin = 0f; dai = "";
        var hong = Hong(d);
        var tam = new List<Vector3>(); var co = new List<Vector3>();
        foreach (var ps in new[] { l.LopTruoc, l.LopSau })
        {
            var hat = new ParticleSystem.Particle[ps.main.maxParticles];
            int k = ps.GetParticles(hat);
            for (int i = 0; i < k; i++)
            {
                Vector3 w = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
                tam.Add(w); co.Add(hat[i].GetCurrentSize3D(ps));
                float lech = Vector2.Distance(new Vector2(w.x, w.z), new Vector2(hong.position.x, hong.position.z));
                if (lech > lechMax) lechMax = lech;
            }
        }
        soKhoi = tam.Count;
        var dinh = new List<Vector3>();
        foreach (var smr in d.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var m = new Mesh();
            smr.BakeMesh(m, true);
            var v = m.vertices;
            int buoc = Mathf.Max(1, v.Length / 800);
            for (int i = 0; i < v.Length; i += buoc) dinh.Add(smr.transform.TransformPoint(v[i]));
            Object.DestroyImmediate(m);
        }
        if (dinh.Count == 0 || soKhoi == 0) return;
        float lo = 1e9f, hi = -1e9f;
        foreach (var x in dinh) { if (x.y < lo) lo = x.y; if (x.y > hi) hi = x.y; }
        int[] tong = new int[5], coLua = new int[5];
        var W2V = cam.worldToCameraMatrix;
        foreach (var x in dinh)
        {
            int b = Mathf.Clamp((int)((x.y - lo) / Mathf.Max(0.01f, hi - lo) * 5f), 0, 4);
            tong[b]++;
            Vector3 vx = W2V.MultiplyPoint(x);
            for (int i = 0; i < tam.Count; i++)
            {
                Vector3 vc = W2V.MultiplyPoint(tam[i]);
                float dy = (vx.y - vc.y) / co[i].y;
                if (Mathf.Abs(vx.x - vc.x) <= 0.30f * co[i].x && dy >= -LuaToanThan.TamTrenChanLua && dy <= 0.37f) { coLua[b]++; break; }
            }
        }
        phuMin = 1f;
        for (int b = 0; b < 5; b++)
        {
            float f = tong[b] > 0 ? (float)coLua[b] / tong[b] : 1f;
            if (f < phuMin) phuMin = f;
            dai += (b > 0 ? " " : "") + f.ToString("P0");
        }
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
        LuaToanThan.DoiChungTheGioi = false;
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
