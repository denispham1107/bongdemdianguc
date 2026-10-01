using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// KHOI GIO LOC LIEN MACH TU DAY LEN DINH (nguoi dung 01/10/2026, anh khoanh giua than: "bui khoi phai lien 1 dai xuyen suot tu day len
/// dinh, hien co 1 doan o than bui mong hon lam loc chia thanh 2 tang"). Menu 95b.
///
/// Nguyen nhan nghi: lop BuiThanDuoi MO DAN o cuoi doi (6 - 7,5 m don vi Loc xoay), lop BuiThanTren sinh DUNG 7,5 m va HIEN DAN (7,5 - 8,4)
/// -> dai mong ngang giua than. Phep do: Gio loc dung yen ban dem, dung lai lop BuiThanTren o cac do cao sinh khac nhau (VfxFactory.
/// DungBuiThanTren - so hat tang theo doan duong, giu mat do moi met); bui + vo len lop 31 ve tren nen DEN (tat suong / bloom / thu nho),
/// may quay NGANG giua than: anh chi vo -> vien than, anh chi bui -> DO DAY KHOI THEO TUNG MET CHIEU CAO (don vi Loc xoay, 15 dai 1 m).
/// "Lom" moi dai = do day / trung binh hai dai cach 2 m hai ben (1 = tron; &lt; 1 = lom). DOI CHUNG: sinh 7,5 m (ban dang co - nguoi dung
/// thay 2 tang) va khong co lop than tren. Trung binh 6 khung. Bao builienmach.txt, anh goc choi builienmach_&lt;cao&gt;.png.
/// </summary>
public static class ThuBuiLienMach
{
    static readonly StringBuilder bao = new StringBuilder();
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/95b. Do KHOI LIEN MACH Gio loc (do day theo do cao, chon cho sinh lop than tren)", false, 186)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[LienMach] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_BuiLienMach");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[LienMach] " + s); }
    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static IEnumerator KichBan()
    {
        Ghi("[ban 2 - theo THOI GIAN sau khi tung] khoi Gio loc lien mach - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        // Ban 1 (do luc dung yen 3,5 s, khoi da day): moi cau hinh deu phang 0,35-0,37, ke ca ban nguoi dung thay 2 tang -> do SAI LUC:
        // Gio loc chi song 4,5 s, cac lop mo tu day (~2,8 m/s don vi Loc xoay) con lop than tren mo tu GIUA than -> giay dau doan giua trong.
        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        var cam = Camera.main;
        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (chuyen != null) { chuyen.enabled = false; chuyen.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); }

        Vector3 goc = toi != null ? toi.transform.position : Vector3.zero;
        Vector3 huong = toi != null ? toi.transform.forward : Vector3.forward; huong.y = 0f; huong.Normalize();
        Vector3 q0 = goc + huong * 10f;
        q0.y = GioLoc.MatDatY(q0, q0.y);
        float k = VfxFactory.HeSoHinhGioLoc;
        const int SoDai = 15;
        float[] mocGiay = { 0.5f, 1.0f, 1.5f, 2.5f, 3.5f };
        // (ten, do cao sinh lop than tren don vi Loc xoay, prewarm)
        // 02/10/2026 nguoi dung chon C (khoi co san). Nay code da prewarm -> A, B TAT prewarm bang tay de lam DOI CHUNG; "Code" = dung y
        // BuildGioLoc (khong dung lai, khong ep gi) - chinh thu trong game.
        var cauHinh = new[] { new { ten = "A_7_5", cao = 7.5f, truoc = false }, new { ten = "B_5_0", cao = 5.0f, truoc = false }, new { ten = "Code", cao = -1f, truoc = true } };
        foreach (var ch in cauHinh)
        {
            var gl = VfxFactory.BuildGioLoc();
            gl.name = "TAM_GioLocLienMach";
            gl.transform.position = q0;
            var hinh = gl.transform.Find("LocXoayHinh");
            if (ch.cao > 0f)
            {
                var cu = hinh.Find("BuiThanTren");
                if (cu != null) Object.DestroyImmediate(cu.gameObject);
                VfxFactory.DungBuiThanTren(hinh, ch.cao);
            }
            int soPrewarm = 0, soBui = 0;
            foreach (var ps in gl.GetComponentsInChildren<ParticleSystem>(true))
            {
                var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                if (!ps.name.StartsWith("Bui")) continue;
                if (ch.cao > 0f) m.prewarm = ch.truoc;          // doi chung: tat prewarm
                soBui++; if (m.prewarm && m.loop) soPrewarm++;
            }
            Ghi(string.Format("{0}: {1}/{2} lop bui prewarm", ch.ten, soPrewarm, soBui));
            float H = 0f;
            foreach (var r in gl.GetComponentsInChildren<MeshRenderer>()) H = Mathf.Max(H, r.bounds.max.y - q0.y);
            float yGiua = 0.5f * VfxFactory.CaoThanLocXoay * k;
            float lucSinh = Time.time;
            foreach (var moc in mocGiay)
            {
                while (Time.time - lucSinh < moc) yield return null;
                yield return new WaitForEndOfFrame();
                var bui = new List<Renderer>(); var vo = new List<Renderer>();
                foreach (var r in gl.GetComponentsInChildren<Renderer>())
                {
                    if (r is ParticleSystemRenderer && r.name.StartsWith("Bui")) bui.Add(r);
                    else if (r is MeshRenderer) vo.Add(r);
                }
                int W = 480, Hh = 540;
                cam.transform.position = q0 - huong * (2.4f * H) + Vector3.up * yGiua;
                cam.transform.LookAt(q0 + Vector3.up * yGiua);
                var dongDai = new float[SoDai + 1];
                for (int d = 0; d <= SoDai; d++) dongDai[d] = cam.WorldToViewportPoint(q0 + Vector3.up * (d * k)).y * Hh;
                int maskCu = cam.cullingMask; var clearCu = cam.clearFlags; var nenCu = cam.backgroundColor; bool suongCu = RenderSettings.fog;
                var lopCu = new Dictionary<Renderer, int>();
                foreach (var r in bui) { lopCu[r] = r.gameObject.layer; r.gameObject.layer = 31; }
                foreach (var r in vo) { lopCu[r] = r.gameObject.layer; r.gameObject.layer = 31; }
                var tatHieuUng = new List<Behaviour>();
                foreach (var bh in cam.GetComponents<Behaviour>())
                    if (bh.enabled && (bh is SimpleBloom || bh is KetXuatThuNho)) { bh.enabled = false; tatHieuUng.Add(bh); }
                cam.cullingMask = 1 << 31; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black; RenderSettings.fog = false;
                foreach (var r in bui) r.enabled = false;
                var chiVo = Chup(cam, W, Hh);
                foreach (var r in bui) r.enabled = true;
                foreach (var r in vo) r.enabled = false;
                var chiBui = Chup(cam, W, Hh);
                foreach (var r in vo) r.enabled = true;
                foreach (var kv in lopCu) kv.Key.gameObject.layer = kv.Value;
                foreach (var bh in tatHieuUng) bh.enabled = true;
                cam.cullingMask = maskCu; cam.clearFlags = clearCu; cam.backgroundColor = nenCu; RenderSettings.fog = suongCu;
                var v = new float[SoDai];
                for (int d = 0; d < SoDai; d++)
                {
                    int y0 = Mathf.Clamp(Mathf.RoundToInt(dongDai[d]), 0, Hh - 1), y1 = Mathf.Clamp(Mathf.RoundToInt(dongDai[d + 1]), 0, Hh - 1);
                    double s = 0; int n = 0;
                    for (int y = y0; y < y1; y++)
                        for (int x = 0; x < W; x++)
                        {
                            int i = y * W + x;
                            if (Sang(chiVo[i]) <= 0.01f) continue;
                            s += Sang(chiBui[i]); n++;
                        }
                    v[d] = n > 0 ? (float)(s / n) : 0f;
                }
                // "Lom kep giua": do day mot dai / min(day nhat BEN DUOI, day nhat BEN TREN) - 1 = lien mach, nho = trong giua hai tang.
                // Profile chi mo dan tu day len (ben tren mong) thi khong tinh la lom.
                float lomMin = 9f; int dLom = -1;
                for (int d = 1; d < SoDai - 1; d++)
                {
                    float duoi = 0f, tren = 0f;
                    for (int j = 0; j < d; j++) duoi = Mathf.Max(duoi, v[j]);
                    for (int j = d + 1; j < SoDai - 1; j++) tren = Mathf.Max(tren, v[j]);
                    float kep = Mathf.Min(duoi, tren);
                    if (kep < 0.05f) continue;
                    float r = v[d] / kep;
                    if (r < lomMin) { lomMin = r; dLom = d; }
                }
                var sb = new StringBuilder();
                for (int d = 0; d < SoDai; d++) sb.AppendFormat(" {0:F2}", v[d]);
                Ghi(string.Format("{0} t={1:F1} s: do day 0->15:{2} | lom kep giua x{3:F2} o {4}-{5} m", ch.ten, moc, sb, lomMin > 8f ? 1f : lomMin, dLom, dLom + 1));
                if (Mathf.Abs(moc - 1.0f) < 0.01f)
                {
                    cam.transform.position = q0 - huong * (2.4f * H) + Vector3.up * (1.3f * H);
                    cam.transform.LookAt(q0 + Vector3.up * (0.45f * H));
                    yield return ChupAnh(cam, "PlayTestShots/builienmach_" + ch.ten + "_1s.png");
                }
            }
            Object.Destroy(gl);
            yield return new WaitForSeconds(0.3f);
        }
        Ket();
    }

    static Color[] Chup(Camera cam, int W, int H)
    {
        var rt = new RenderTexture(W, H, 24);
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        var px = t.GetPixels();
        Object.Destroy(t); Object.Destroy(rt);
        return px;
    }

    static IEnumerator ChupAnh(Camera cam, string duong)
    {
        yield return new WaitForEndOfFrame();
        int W = 480, H = 540;
        var rt = new RenderTexture(W, H, 24);
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        File.WriteAllBytes(duong, t.EncodeToPNG());
        Object.Destroy(t); Object.Destroy(rt);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/builienmach.txt", bao.ToString());
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t != null && t.parent == null && t.name.StartsWith("TAM_")) Object.Destroy(t.gameObject);
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
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
