using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 98 - HAT BUI DEN CUON LEN trong + ngoai than (nguoi dung 04/10/2026, Loc xoay + Gio loc). Vao Play Act2, tat GameDirector, tung
/// Loc xoay THAT (Tornado.Spawn) va Gio loc THAT (GioLoc.Spawn); sau 3 s doc VI TRI THAT tung hat cua 4 lop (HatDenTrong / HatDenNgoai /
/// DenCuonTrong / DenCuonNgoai) trong toa do LocXoayHinh khu ti le (= don vi Loc xoay goc), roi doc lai sau 0,1 s theo randomSeed:
///   - phu chieu cao: so hat moi phan nam than (0-15 m) - phan tren cung phai co hat (len TAN DINH);
///   - ti le r / vo chinh tai dung do cao: lop trong < 0,8, lop ngoai > 1,0;
///   - van toc len > 0, chieu quay = chieu quay vo / dai gio (Spin);
///   - mau: do sang mau sinh < 0,25 (DEN). DOI CHUNG: lop BuiCuonLen xam cua Loc xoay that > 0,5 (phep do mau phan biet duoc).
/// Ket qua: PlayTestShots/hatden_cuonlen.txt (+ "so loi").
/// </summary>
public static class ThuHatDenCuonLen
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    static readonly StringBuilder bao = new StringBuilder();
    static int soLoi;
    const string Ra = "PlayTestShots/hatden_cuonlen.txt";
    static readonly string[] Lop = { "HatDenTrong", "HatDenNgoai", "DenCuonTrong", "DenCuonNgoai" };

    [MenuItem("Diablo 2.5D/98 Hat bui den cuon len (Loc xoay + Gio loc)", false, 162)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        bao.Length = 0; soLoi = 0; daBatDau = false;
        if (File.Exists(Ra)) File.Delete(Ra);
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
        var go = new GameObject("TAM_ThuHatDen");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); }
    static void Kiem(bool dung, string loi) { if (!dung) { soLoi++; bao.AppendLine("  LOI: " + loi); } }
    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        Vector3 xa = Vector3.forward, phai = Vector3.right;
        var lx = Tornado.Spawn(toi.transform.position + xa * 18f + phai * 10f, phai, 0);
        var gl = GioLoc.Spawn(toi.transform.position + xa * 10f - phai * 12f, phai, 0);
        yield return new WaitForSeconds(3f);
        float mauDoiChung = -1f;
        foreach (var ps in lx.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "BuiCuonLen") mauDoiChung = Sang(ps.main.startColor.colorMax);
        yield return DoMot("LOC XOAY", lx != null ? lx.transform : null);
        yield return DoMot("GIO LOC", gl != null ? gl.transform : null);
        Ghi(string.Format("DOI CHUNG mau: lop BuiCuonLen xam cua Loc xoay that do sang {0:F2} (phai > 0,5)", mauDoiChung));
        Kiem(mauDoiChung > 0.5f, "doi chung mau khong phan biet duoc");

        if (lx != null) Object.Destroy(lx.gameObject);
        if (gl != null) Object.Destroy(gl.gameObject);
        Ghi("so loi ghi nhan = " + soLoi);
        File.WriteAllText(Ra, bao.ToString());
        var rac = GameObject.Find("TAM_ThuHatDen");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static IEnumerator DoMot(string ten, Transform loc)
    {
        if (loc == null) { Ghi(ten + ": KHONG tung duoc"); Kiem(false, ten + " khong tung duoc"); yield break; }
        Transform hinh = null;
        foreach (var t in loc.GetComponentsInChildren<Transform>(true)) if (t.name == "LocXoayHinh") hinh = t;
        var sp = loc.GetComponentInChildren<Spin>();
        if (hinh == null || sp == null) { Kiem(false, ten + " thieu LocXoayHinh / Spin"); yield break; }
        var q = Quaternion.AngleAxis(sp.degreesPerSecond * 0.1f, sp.transform.TransformDirection(Vector3.up)) * Vector3.right;
        float chieuThan = Mathf.Sign(Mathf.Atan2(q.z, q.x));
        var A = new Dictionary<string, Dictionary<uint, Vector3>>();
        foreach (var n in Lop) A[n] = Chup(hinh, n);
        yield return new WaitForSeconds(0.1f);
        foreach (var n in Lop)
        {
            var ps = hinh.Find(n) != null ? hinh.Find(n).GetComponent<ParticleSystem>() : null;
            if (ps == null) { Ghi(ten + " " + n + ": THIEU"); Kiem(false, ten + " thieu lop " + n); continue; }
            var B = Chup(hinh, n);
            var tang = new int[5]; int nHat = 0, len = 0, cung = 0, trongVo = 0, ngoaiVo = 0; float tongTi = 0f;
            foreach (var kv in A[n])
            {
                Vector3 a = kv.Value;                   // he LocXoayHinh = don vi Loc xoay goc (da khu ti le k cua Gio loc)
                float h = a.y; if (h < 0f || h > 15.72f) continue;
                nHat++;
                tang[Mathf.Clamp((int)(h / 3f), 0, 4)]++;
                float ti = new Vector2(a.x, a.z).magnitude / VfxFactory.BanKinhLocXoay(h, 1f);
                tongTi += ti; if (ti < 0.8f) trongVo++; if (ti > 1.0f) ngoaiVo++;
                Vector3 b;
                if (B.TryGetValue(kv.Key, out b))
                {
                    if (b.y > a.y) len++;
                    float w = Mathf.DeltaAngle(Mathf.Atan2(a.z, a.x) * Mathf.Rad2Deg, Mathf.Atan2(b.z, b.x) * Mathf.Rad2Deg);
                    if (Mathf.Sign(w) == chieuThan) cung++;
                }
            }
            float sang = Sang(ps.main.startColor.colorMax);
            bool laTrong = n.EndsWith("Trong");
            Ghi(string.Format("{0} {1}: {2} hat; theo 5 phan cao (0-3-6-9-12-15 m don vi Loc xoay) {3}/{4}/{5}/{6}/{7}; r/vo chinh TB {8:F2} (trong vo {9}, ngoai vo {10}); len {11}/{2}, quay cung chieu than {12}/{2}; do sang mau {13:F2}; anh {14}",
                ten, n, nHat, tang[0], tang[1], tang[2], tang[3], tang[4], tongTi / Mathf.Max(1, nHat), trongVo, ngoaiVo, len, cung, sang,
                ps.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture.name));
            Kiem(nHat > 30 && tang[4] > 0 && tang[0] > 0, ten + " " + n + ": khong cuon tu chan len TAN DINH");
            Kiem(laTrong ? trongVo > 0.9f * nHat : ngoaiVo > 0.9f * nHat, ten + " " + n + ": khong nam " + (laTrong ? "TRONG" : "NGOAI") + " than");
            Kiem(len > 0.95f * nHat && cung > 0.95f * nHat, ten + " " + n + ": khong bay len / quay nguoc chieu than");
            Kiem(sang < 0.25f, ten + " " + n + ": khong phai mau den");
        }
    }

    static Dictionary<uint, Vector3> Chup(Transform hinh, string ten)
    {
        var d = new Dictionary<uint, Vector3>();
        var t = hinh.Find(ten); if (t == null) return d;
        var ps = t.GetComponent<ParticleSystem>();
        var arr = new ParticleSystem.Particle[ps.main.maxParticles];
        int n = ps.GetParticles(arr);
        // mo phong CUC BO: vi tri hat trong he cua chinh he hat (con truc tiep cua LocXoayHinh, khong lech) -> doi sang he LocXoayHinh
        for (int i = 0; i < n; i++) d[arr[i].randomSeed] = hinh.InverseTransformPoint(t.TransformPoint(arr[i].position));
        return d;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        Debug.Log("[ThuHatDenCuonLen] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
