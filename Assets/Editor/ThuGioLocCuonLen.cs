using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MENU 97 - THAN LOC CO "CUON LEN" KHONG (nguoi dung 04/10/2026: "danh Gio loc ra, loc chi la 1 hinh dung len va tien ve phia truoc,
/// khong he co hieu ung cuon tu duoi len theo 1 huong nhat dinh").
/// Do NGOAI Play trong PreviewScene (khong dong vao scene dang mo): dung hinh than, gia lap dung Spin + ScrollUV (quay goc w*t, offset
/// anh = toc truot * t - y het hai component lam trong Update), chup hai khung cach dt bang camera TRUC GIAO di theo loc (bo chuyen dong
/// tien len), chi ve than (lop 31) tren nen den. Moi cap khung:
///   - THAY DOI = trung binh |B - A| / trung binh A (than dung im thi 0).
///   - DICH CHUYEN BIEU KIEN: tim (dx, dy) lam |B dich - A| nho nhat trong o giua than -> van toc doc (+ = LEN) tinh theo "than / giay".
///   - PHAN GIAI THICH DUOC boi dich chuyen = 1 - sai sau dich / sai khong dich (gan 0 = thay doi lon xon, khong co huong).
/// So: Gio loc hien tai (du / tat truot anh / tat quay) va DOI CHUNG Loc xoay that (nguoi dung da duyet "dai khoi troi len" 25/09/2026).
/// </summary>
public static class ThuGioLocCuonLen
{
    const int N = 300;
    const string Ra = "PlayTestShots/gioloc_cuonlen.txt";

    [MenuItem("Diablo 2.5D/97 Gio loc - do than co cuon len khong")]
    public static void Chay()
    {
        var sb = new System.Text.StringBuilder();
        var ps = EditorSceneManager.NewPreviewScene();
        try
        {
            // dai gio: do tren anh (vet gio bay ngang ~4 m/s, vuot cua so do anh -> do rieng bang vi tri hat that)
            foreach (var f in new[] { 1f, 1.5f, 2f })
                DoMot(sb, ps, "GIO LOC muc " + f + " - dai gio", false, f, true, false);
            DoMot(sb, ps, "DOI CHUNG dung im (khong quay, khong truot)", false, 0f, true, false);
            foreach (var f in new[] { 1f, 1.5f, 2f }) DoVet(sb, ps, f);
            DoMot(sb, ps, "DOI CHUNG Loc xoay that", true, 1f, true, false);
        }
        finally { EditorSceneManager.ClosePreviewScene(ps); }
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText(Ra, sb.ToString());
        Debug.Log("[ThuGioLocCuonLen]\n" + sb);
    }

    struct Lop { public Transform t; public Quaternion q0; public float w; public Material m; public Vector2 v; public Vector2 off0; }

    static void DoMot(System.Text.StringBuilder sb, Scene ps, string ten, bool locXoay, float muc, bool coDai, bool coVet)
    {
        GameObject goc; float cao; var than = new List<Renderer>(); var hats = new List<ParticleSystem>();
        float mucCu = VfxFactory.MucCuonGioLoc;
        if (locXoay)
        {
            goc = VfxFactory.BuildLocXoay(1f); cao = 15.72f;
            foreach (var r in goc.GetComponentsInChildren<MeshRenderer>(true))
                if (r.name.StartsWith("Vo") || r.name == "Vanh") than.Add(r);
        }
        else
        {
            // muc 0 = doi chung dung im: dung o muc 1 roi tat quay / truot / mo phong hat (giu nguyen hinh)
            VfxFactory.MucCuonGioLoc = muc > 0f ? muc : 1f;
            try { goc = VfxFactory.BuildGioLoc(); } finally { VfxFactory.MucCuonGioLoc = mucCu; }
            cao = 5f;
            var x = goc.transform.Find("GioXoan");
            if (coDai && x != null) foreach (var r in x.GetComponentsInChildren<MeshRenderer>(true)) than.Add(r);
            var v = goc.transform.Find("VetGioXoan");
            if (coVet && v != null) { var p = v.GetComponent<ParticleSystem>(); hats.Add(p); than.Add(p.GetComponent<Renderer>()); }
        }
        SceneManager.MoveGameObjectToScene(goc, ps);
        goc.transform.position = Vector3.zero;
        foreach (var r in goc.GetComponentsInChildren<Renderer>(true)) r.gameObject.layer = 0;
        foreach (var p in goc.GetComponentsInChildren<ParticleSystem>(true)) { p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); p.useAutoRandomSeed = false; p.randomSeed = 7; }
        float heSo = muc > 0f ? 1f : 0f;
        var lops = new List<Lop>();
        foreach (var r in than)
        {
            r.gameObject.layer = 31;
            if (r is ParticleSystemRenderer) continue;
            var sp = r.GetComponent<Spin>(); var sc = r.GetComponent<ScrollUV>();
            var m = new Material(r.sharedMaterial); r.sharedMaterial = m;
            lops.Add(new Lop { t = r.transform, q0 = r.transform.localRotation, w = sp != null ? sp.degreesPerSecond * heSo : 0f,
                               m = m, v = sc != null ? sc.speed * heSo : Vector2.zero, off0 = m.mainTextureOffset });
        }
        hatDangDo = hats; hatChay = muc > 0f;
        var goCam = new GameObject("TAM_CamCuonLen");
        SceneManager.MoveGameObjectToScene(goCam, ps);
        var cam = goCam.AddComponent<Camera>();
        cam.enabled = false; cam.scene = ps; cam.orthographic = true; cam.orthographicSize = cao * 0.6f;
        cam.cullingMask = 1 << 31; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.1f; cam.farClipPlane = cao * 10f;
        cam.transform.position = new Vector3(0f, cao * 0.5f, -cao * 3f); cam.transform.rotation = Quaternion.identity;
        float pxM = cao * 1.2f / N;   // met moi diem anh

        const float dt = 0.1f;
        float tongThay = 0f, tongVy = 0f, tongVx = 0f, tongGiai = 0f; int soMau = 0;
        var vys = new List<float>();
        for (int i = 0; i < 12; i++)
        {
            float t = 0.37f * i + 0.11f;   // hat: 3 s + t (da on dinh)
            var A = Chup(cam, lops, t); var B = Chup(cam, lops, t + dt);
            float tb = 0f; foreach (var a in A) tb += a; tb /= A.Length;
            float d0 = Sai(A, B, 0, 0);
            int bx = 0, by = 0; float best = d0;
            for (int dy = -12; dy <= 12; dy++)
                for (int dx = -12; dx <= 12; dx++)
                {
                    float d = Sai(A, B, dx, dy);
                    if (d < best - 1e-7f) { best = d; bx = dx; by = dy; }
                }
            float vy = by * pxM / dt / cao, vx = bx * pxM / dt / cao;
            tongThay += d0 / Mathf.Max(1e-5f, tb); tongVy += vy; tongVx += vx; tongGiai += d0 > 1e-6f ? 1f - best / d0 : 0f; soMau++;
            vys.Add(vy);
        }
        int len = 0; foreach (var v in vys) if (v > 0.001f) len++;
        sb.AppendLine(string.Format("{0}: thay doi moi 0,1 s {1:P0} | dich chuyen bieu kien doc {2:+0.000;-0.000} than/giay (LEN o {3}/{4} cap khung), ngang {5:+0.000;-0.000} than/giay | phan thay doi giai thich duoc boi dich chuyen {6:P0}",
            ten, tongThay / soMau, tongVy / soMau, len, soMau, tongVx / soMau, tongGiai / soMau));
        foreach (var l in lops) Object.DestroyImmediate(l.m);
        Object.DestroyImmediate(goCam); Object.DestroyImmediate(goc);
    }

    static List<ParticleSystem> hatDangDo; static bool hatChay;

    /// <summary>VET GIO: doc VI TRI THAT tung hat (theo randomSeed) o 3 s va 3,1 s - van toc len (m/s, than/giay) va chieu quay so voi
    /// chieu quay cua dai gio (lay tu Spin: xoay mot diem roi xem goc atan2(z,x) tang hay giam).</summary>
    static void DoVet(System.Text.StringBuilder sb, Scene ps, float muc)
    {
        float mucCu = VfxFactory.MucCuonGioLoc;
        VfxFactory.MucCuonGioLoc = muc;
        GameObject goc; try { goc = VfxFactory.BuildGioLoc(); } finally { VfxFactory.MucCuonGioLoc = mucCu; }
        SceneManager.MoveGameObjectToScene(goc, ps);
        var v = goc.transform.Find("VetGioXoan"); var sp = goc.GetComponentInChildren<Spin>();
        if (v == null || sp == null) { sb.AppendLine("VET GIO muc " + muc + ": THIEU he VetGioXoan / Spin"); Object.DestroyImmediate(goc); return; }
        var p = v.GetComponent<ParticleSystem>(); p.useAutoRandomSeed = false; p.randomSeed = 7;
        var r = p.GetComponent<ParticleSystemRenderer>();
        var A = new ParticleSystem.Particle[p.main.maxParticles]; var B = new ParticleSystem.Particle[p.main.maxParticles];
        const float dt = 0.1f;
        p.Simulate(3f, true, true, true); int na = p.GetParticles(A);
        p.Simulate(3f + dt, true, true, true); int nb = p.GetParticles(B);
        var tra = new Dictionary<uint, Vector3>(); for (int i = 0; i < nb; i++) tra[B[i].randomSeed] = B[i].position;
        int n = 0, len = 0, cungChieu = 0; float tongVy = 0f, tongW = 0f;
        // chieu dai gio: xoay diem (1, 0, 0) dung nhu Spin trong dt roi xem goc atan2(z, x) doi dau nao
        var q = Quaternion.AngleAxis(sp.degreesPerSecond * dt, Vector3.up) * Vector3.right;
        float chieuDai = Mathf.Sign(Mathf.Atan2(q.z, q.x));
        for (int i = 0; i < na; i++)
        {
            Vector3 b; if (!tra.TryGetValue(A[i].randomSeed, out b)) continue;
            var a = A[i].position; n++;
            float vy = (b.y - a.y) / dt; tongVy += vy; if (vy > 0f) len++;
            float w = Mathf.DeltaAngle(Mathf.Atan2(a.z, a.x) * Mathf.Rad2Deg, Mathf.Atan2(b.z, b.x) * Mathf.Rad2Deg) / dt;
            tongW += w; if (Mathf.Sign(w) == chieuDai) cungChieu++;
        }
        sb.AppendLine(string.Format("VET GIO muc {0}: {1} hat song, len {2:F2} m/s = {3:+0.000} than/giay (LEN {4}/{1}), quay {5:F0} do/s, cung chieu dai gio {6}/{1}; ve duoi: {7}, che do ve hat {8}",
            muc, n, tongVy / Mathf.Max(1, n), tongVy / Mathf.Max(1, n) / 5f, len, Mathf.Abs(tongW / Mathf.Max(1, n)), cungChieu, p.trails.enabled && r.trailMaterial != null ? r.trailMaterial.mainTexture.name : "KHONG", r.renderMode));
        Object.DestroyImmediate(goc);
    }

    static float[] Chup(Camera cam, List<Lop> lops, float t)
    {
        // hat: mo phong lai tu dau toi dung t (hat giong co dinh -> hai khung lien mach); doi chung dung im: giu o 3 s
        foreach (var p in hatDangDo) p.Simulate(3f + (hatChay ? t : 0f), true, true, true);
        foreach (var l in lops)
        {
            l.t.localRotation = l.q0 * Quaternion.AngleAxis(l.w * t, Vector3.up);
            var o = l.off0 + l.v * t; o.x -= Mathf.Floor(o.x); o.y -= Mathf.Floor(o.y);
            l.m.mainTextureOffset = o;
        }
        var rt = RenderTexture.GetTemporary(N, N, 24);
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
        var cu = RenderTexture.active; RenderTexture.active = rt;
        var tx = new Texture2D(N, N, TextureFormat.RGB24, false); tx.ReadPixels(new Rect(0, 0, N, N), 0, 0); tx.Apply();
        RenderTexture.active = cu; RenderTexture.ReleaseTemporary(rt);
        var px = tx.GetPixels(); Object.DestroyImmediate(tx);
        var L = new float[N * N];
        for (int i = 0; i < L.Length; i++) L[i] = 0.299f * px[i].r + 0.587f * px[i].g + 0.114f * px[i].b;
        return L;
    }

    // sai trung binh giua A va B dich (dx, dy) - chi o giua than (bo 20% bien moi phia)
    static float Sai(float[] A, float[] B, int dx, int dy)
    {
        int x0 = N / 5, x1 = N - N / 5, y0 = N / 5, y1 = N - N / 5; double s = 0; int n = 0;
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++) { s += Mathf.Abs(B[(y + dy) * N + x + dx] - A[y * N + x]); n++; }
        return (float)(s / n);
    }
}
