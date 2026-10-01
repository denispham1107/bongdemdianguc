using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// KHOI THAN TREN GIO LOC - chon so hat bang do tren anh (nguoi dung 01/10/2026 khoanh than tren tren anh: "chua phu bui khoi nhu than
/// duoi, them bui khoi cuon bay len tan dinh cho than tren"; chon "them lop khoi than tren", chi Gio loc). Menu 95.
///
/// Mot Gio loc dung yen (BuildGioLoc) ban dem; quet so hat lop BuiThanTren 0 (= truoc khi them - DOI CHUNG) / 80 / 160 / 240 / 320.
/// Moi muc (ban 2): may quay NGANG o giua than (cach 2,4 x chieu cao); bui + vo dua len lop 31, ve tren nen DEN (tat suong / bloom):
/// anh chi vo -> VIEN THAN (sang &gt; 0,01), anh chi bui -> "khoi" = do sang trung binh trong vien than (ti le do day), "phu" = ti le diem
/// anh &gt; 0,03; tach nua duoi / tren tai do cao giua than. Trung binh 6 khung. Anh luu: goc cao nhu luc choi.
/// Anh PlayTestShots/buithantren_&lt;so hat&gt;.png, bao buithantren.txt.
/// </summary>
public static class ThuBuiThanTren
{
    static readonly StringBuilder bao = new StringBuilder();
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static readonly float[] Muc = { 0f, 80f, 160f, 240f, 320f };

    [MenuItem("Diablo 2.5D/95. Chup KHOI THAN TREN Gio loc (chon so hat theo than duoi)", false, 185)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[ThanTren] scene co thay doi chua luu"); return; }
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
        var go = new GameObject("TAM_BuiThanTren");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[ThanTren] " + s); }
    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static IEnumerator KichBan()
    {
        Ghi("[ban 2 - tach lop bui tren nen den] khoi than tren Gio loc - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
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
        var gl = VfxFactory.BuildGioLoc();
        gl.name = "TAM_GioLocThanTren";
        gl.transform.position = q0;
        ParticleSystem tren = null;
        foreach (var ps in gl.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            if (ps.name == "BuiThanTren") tren = ps;
        }
        if (tren == null) { Ghi("[LOI] Gio loc khong co lop BuiThanTren"); Ket(); yield break; }
        float k = VfxFactory.HeSoHinhGioLoc;
        float H = 0f;
        foreach (var r in gl.GetComponentsInChildren<MeshRenderer>()) H = Mathf.Max(H, r.bounds.max.y - q0.y);
        float yGiua = 0.5f * VfxFactory.CaoThanLocXoay * k;
        Ghi(string.Format("Gio loc cao {0:F2} m, tach nua than o {1:F2} m; lop BuiThanTren mac dinh {2:F0} hat/giay", H, yGiua, tren.emission.rateOverTime.constant));

        var bui = new System.Collections.Generic.List<Renderer>();
        var vo = new System.Collections.Generic.List<Renderer>();
        foreach (var r in gl.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer && r.name.StartsWith("Bui")) bui.Add(r);
            else if (r is MeshRenderer) vo.Add(r);
        }

        foreach (var toc in Muc)
        {
            var m = tren.main; m.maxParticles = Mathf.Max(1, Mathf.CeilToInt(toc * 3.0f * 1.1f));
            var e = tren.emission; e.rateOverTime = toc;
            if (toc <= 0f) tren.Clear();
            yield return new WaitForSeconds(3.5f);
            int W = 480, Hh = 540;
            // Ban 2: ban 1 do CHENH SANG cua bui tren anh day du -> o than tren bui xam de len VO cung xam nen gan nhu khong doi sang
            // (0 -> 320 hat/giay: 0,037 -> 0,048) du anh thay ro day len; o than duoi bui de len DAT TOI nen chenh lon. Nay TACH LOP:
            // bui va vo len lop 31, may quay chi ve lop 31 tren nen DEN (tat suong, bloom, thu nho) -> do sang anh "chi bui" ti le voi do
            // day khoi. May quay NGANG o do cao giua than -> duong chia nua tren / duoi trung dung do cao giua than tren man hinh.
            cam.transform.position = q0 - huong * (2.4f * H) + Vector3.up * yGiua;
            cam.transform.LookAt(q0 + Vector3.up * yGiua);
            float yManHinh = cam.WorldToViewportPoint(q0 + Vector3.up * yGiua).y * Hh;
            double khoiD = 0, khoiT = 0, phuD = 0, phuT = 0; int soLan = 6;
            for (int lan = 0; lan < soLan; lan++)
            {
                yield return new WaitForEndOfFrame();
                int maskCu = cam.cullingMask; var clearCu = cam.clearFlags; var nenCu = cam.backgroundColor; bool suongCu = RenderSettings.fog;
                var lopCu = new System.Collections.Generic.Dictionary<Renderer, int>();
                foreach (var r in bui) { lopCu[r] = r.gameObject.layer; r.gameObject.layer = 31; }
                foreach (var r in vo) { lopCu[r] = r.gameObject.layer; r.gameObject.layer = 31; }
                var tatHieuUng = new System.Collections.Generic.List<Behaviour>();
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
                double sD = 0, sT = 0; int nD = 0, nT = 0, pD = 0, pT = 0;
                for (int y = 0; y < Hh; y++)
                    for (int x = 0; x < W; x++)
                    {
                        int i = y * W + x;
                        if (Sang(chiVo[i]) <= 0.01f) continue;                 // ngoai vien than
                        float d = Sang(chiBui[i]);
                        if (y < yManHinh) { sD += d; nD++; if (d > 0.03f) pD++; }
                        else { sT += d; nT++; if (d > 0.03f) pT++; }
                    }
                khoiD += nD > 0 ? sD / nD : 0; khoiT += nT > 0 ? sT / nT : 0;
                phuD += nD > 0 ? (double)pD / nD : 0; phuT += nT > 0 ? (double)pT / nT : 0;
                yield return new WaitForSeconds(0.2f);
            }
            khoiD /= soLan; khoiT /= soLan; phuD /= soLan; phuT /= soLan;
            // Anh cho nguoi dung: goc cao nhu luc choi, anh day du
            cam.transform.position = q0 - huong * (2.4f * H) + Vector3.up * (1.3f * H);
            cam.transform.LookAt(q0 + Vector3.up * (0.45f * H));
            yield return ChupAnh(cam, "PlayTestShots/buithantren_" + (int)toc + ".png");
            Ghi(string.Format("BuiThanTren {0,3:F0} hat/giay: khoi trong vien than - duoi {1:F4} / tren {2:F4} = x{3:F2}; phu - duoi {4:P0} / tren {5:P0}; hat dang song lop than tren {6}",
                toc, khoiD, khoiT, khoiD > 0 ? khoiT / khoiD : 0, phuD, phuT, tren.particleCount));
        }
        Object.Destroy(gl);
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
        File.WriteAllText("PlayTestShots/buithantren.txt", bao.ToString());
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
