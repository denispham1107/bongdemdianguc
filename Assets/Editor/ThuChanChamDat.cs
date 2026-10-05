using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 109 - CHAN NGUOI CHOI CO CHAM DAT KHONG (nguoi dung 05/10/2026: "trong game rat nhieu cho 2 chan nhan vat dung van con nhu bay lo
/// lung tren mat dat").
///
/// Vao Play THAT o Act2 (qua GameBootstrap, nhu menu 40), tat GameDirector (khong sinh quai). Dua nhan vat toi nhieu cho ngau nhien (co dinh
/// hat giong) tren dat trong ban do, de CharacterController tu roi xuong 0,6 s roi do:
///   - goc nhan vat cach mat dat (Terrain.SampleHeight) bao nhieu, collider nao ngay duoi goc;
///   - DE GIAY: BakeMesh, dinh thap nhat moi ben (chia theo mat phang giua hai xuong dui), so voi Terrain.SampleHeight ngay duoi dinh ay.
/// Ket qua: PlayTestShots/chan_cham_dat.txt + anh can vai cho.
/// </summary>
public static class ThuChanChamDat
{
    const string Ra = "PlayTestShots/chan_cham_dat.txt";
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/109 Chan nguoi choi cham dat (Act2)", false, 177)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) return;
        Directory.CreateDirectory("PlayTestShots");
        if (File.Exists(Ra)) File.Delete(Ra);
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        daBatDau = false;
        EditorApplication.update -= Nhip; EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ChanChamDat");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    /// <summary>So do mot cho dung.</summary>
    public struct SoDo { public float goc, deThap, deCao, doc; public string duoi; }

    /// <summary>Do khe goc + de giay - mat dat (Terrain.SampleHeight). deThap = ben gan dat hon, deCao = ben ho hon.</summary>
    public static SoDo Do(Transform nv)
    {
        var s = new SoDo();
        var ter = Terrain.activeTerrain;
        System.Func<Vector3, float> dat = p => ter.SampleHeight(p) + ter.transform.position.y;
        s.goc = nv.position.y - dat(nv.position);
        Vector3 n = ter.terrainData.GetInterpolatedNormal((nv.position.x - ter.transform.position.x) / ter.terrainData.size.x,
                                                          (nv.position.z - ter.transform.position.z) / ter.terrainData.size.z);
        s.doc = Vector3.Angle(n, Vector3.up);
        RaycastHit h;
        int matNa = ~(1 << LayerMask.NameToLayer("Player"));
        s.duoi = Physics.Raycast(nv.position + Vector3.up * 0.5f, Vector3.down, out h, 3f, matNa, QueryTriggerInteraction.Ignore)
            ? h.collider.name + " (" + LayerMask.LayerToName(h.collider.gameObject.layer) + ")" : "-";
        Transform duiT = null, duiP = null;
        foreach (var t in nv.GetComponentsInChildren<Transform>(true)) { if (t.name == "LeftUpLeg") duiT = t; if (t.name == "RightUpLeg") duiP = t; }
        Vector3 giua = (duiT.position + duiP.position) * 0.5f, phai = duiP.position - duiT.position; phai.y = 0f; phai.Normalize();
        var thap = new[] { Vector3.up * 1e6f, Vector3.up * 1e6f };
        var luoi = new Mesh(); var dinh = new List<Vector3>();
        foreach (var smr in nv.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!smr.enabled) continue;
            smr.BakeMesh(luoi, true); luoi.GetVertices(dinh);
            foreach (var v in dinh)
            {
                Vector3 p = smr.transform.position + smr.transform.rotation * v;
                int i = Vector3.Dot(p - giua, phai) >= 0f ? 1 : 0;
                if (p.y < thap[i].y) thap[i] = p;
            }
        }
        Object.Destroy(luoi);
        float a = thap[0].y - dat(thap[0]), b = thap[1].y - dat(thap[1]);
        s.deThap = Mathf.Min(a, b); s.deCao = Mathf.Max(a, b);
        return s;
    }

    static IEnumerator KichBan()
    {
        var sb = new StringBuilder(); int loi = 0;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Act2");
        float han = Time.time + 25f;
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Act2" && Time.time < han) yield return null;
        PlayerController toi = null;
        han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { sb.AppendLine("LOI: khong thay nhan vat"); loi++; }
        else
        {
            var gd = Object.FindAnyObjectByType<GameDirector>(); if (gd != null) gd.enabled = false;
            foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(q.gameObject);
            yield return new WaitForSeconds(1.0f);
            var cc = toi.GetComponent<CharacterController>();
            sb.AppendLine(string.Format("CharacterController: center y {0:F3}, cao {1:F3}, ban kinh {2:F2}, skinWidth {3:F3} -> day con nhong o goc + {4:F3} m",
                cc.center.y, cc.height, cc.radius, cc.skinWidth, cc.center.y - cc.height * 0.5f));
            var ter = Terrain.activeTerrain;
            var hh = toi.GetComponentInChildren<NguoiChoiHoatHinh>();
            var rnd = new System.Random(109);
            var cacMoi = new List<SoDo>(); var cacCu = new List<SoDo>();
            int lan = 0;
            while (cacMoi.Count < 40 && lan < 400)
            {
                lan++;
                float x = (float)(rnd.NextDouble() * 2 - 1) * 55f, z = (float)(rnd.NextDouble() * 2 - 1) * 55f;
                Vector3 p = new Vector3(x, 0f, z); p.y = ter.SampleHeight(p) + ter.transform.position.y;
                // bo cho co vat (bia, cay, nha, nuoc) - chi do tren dat trong
                if (Physics.CheckCapsule(p + Vector3.up * 0.45f, p + Vector3.up * 1.8f, 0.6f, ~(1 << LayerMask.NameToLayer("Player")) & ~(1 << LayerMask.NameToLayer("Ground")), QueryTriggerInteraction.Ignore)) continue;
                if (p.y < -0.6f) continue;   // ho / vung nuoc trung
                hh.chamDat = true;
                cc.enabled = false; toi.transform.position = p + Vector3.up * 0.4f; cc.enabled = true;
                yield return new WaitForSeconds(0.6f);
                yield return new WaitForEndOfFrame();
                var s = Do(toi.transform);
                // DOI CHUNG cung cho: tat chan cham dat (tu the + do cao nhu ban cu)
                hh.chamDat = false;
                yield return null; yield return null;
                yield return new WaitForEndOfFrame();
                var c = Do(toi.transform);
                if (cacMoi.Count < 3) { Chup(toi.transform, "chan_cham_dat_cu_" + (cacMoi.Count + 1)); }
                hh.chamDat = true;
                yield return null; yield return null;
                yield return new WaitForEndOfFrame();
                if (cacMoi.Count < 3) { Chup(toi.transform, "chan_cham_dat_moi_" + (cacMoi.Count + 1)); }
                cacMoi.Add(s); cacCu.Add(c);
                sb.AppendLine(string.Format("  cho {0,2} ({1,6:F1},{2,6:F1}) doc {3,4:F1} do | MOI: de giay - dat chan thap {4:F3} / chan ho {5:F3} m, ha than {6:F3}, nhac {7:F3} | CU: {8:F3} / {9:F3} m | duoi goc: {10}",
                    cacMoi.Count, p.x, p.z, s.doc, s.deThap, s.deCao, hh.DoHaThan, Mathf.Max(hh.NhacChan.x, hh.NhacChan.y), c.deThap, c.deCao, s.duoi));
            }
            TongKet(sb, "MOI", cacMoi, out int hoMoi, out int lunMoi);
            TongKet(sb, "CU (doi chung)", cacCu, out int hoCu, out int lunCu);
            kiem(ref loi, sb, hoCu > cacCu.Count / 2, "doi chung (ban cu) khong lo lung -> phep do khong phan biet");
            kiem(ref loi, sb, hoMoi == 0, "con cho de giay ho dat > 2 cm (lo lung)");
            kiem(ref loi, sb, lunMoi == 0, "co cho de giay lun > 3 cm");

            // DANG DI (ep toc do di bo tai cho): chan THAP moi khung phai cham dat
            foreach (bool bat in new[] { true, false })
            {
                hh.chamDat = bat;
                Vector3 p = new Vector3(-4.1f, 0f, -3.6f); p.y = ter.SampleHeight(p) + ter.transform.position.y;
                cc.enabled = false; toi.transform.position = p + Vector3.up * 0.3f; cc.enabled = true;
                hh.tocDoEp = 1f;
                yield return new WaitForSeconds(1.0f);
                float tong = 0f, lonNhat = -1f, nhoNhat = 1f; int k = 0;
                for (int f = 0; f < 40; f++)
                {
                    yield return new WaitForEndOfFrame();
                    var s = Do(toi.transform);
                    tong += s.deThap; lonNhat = Mathf.Max(lonNhat, s.deThap); nhoNhat = Mathf.Min(nhoNhat, s.deThap); k++;
                    yield return null;
                }
                hh.tocDoEp = -1f;
                sb.AppendLine(string.Format("DANG DI tai cho ({0}): chan thap - dat TB {1:F3} m, {2:F3} .. {3:F3} ({4} khung)", bat ? "MOI" : "CU", tong / k, nhoNhat, lonNhat, k));
                if (bat) kiem(ref loi, sb, Mathf.Abs(tong / k) <= 0.025f && lonNhat <= 0.05f && nhoNhat >= -0.05f, "dang di: chan tru khong cham dat");
                else kiem(ref loi, sb, tong / k > 0.04f, "doi chung dang di khong lo lung -> phep do khong phan biet");
            }
            hh.chamDat = true;
        }
        sb.AppendLine("so loi ghi nhan = " + loi);
        File.WriteAllText(Ra, sb.ToString());
        var rac = GameObject.Find("TAM_ChanChamDat"); if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }

    static void TongKet(StringBuilder sb, string ten, List<SoDo> ds, out int ho, out int lun)
    {
        float tbThap = 0, tbCao = 0, maxCao = -1, minThap = 1; ho = 0; lun = 0;
        foreach (var s in ds)
        {
            tbThap += s.deThap; tbCao += s.deCao; maxCao = Mathf.Max(maxCao, s.deCao); minThap = Mathf.Min(minThap, s.deThap);
            if (s.deCao > 0.02f) ho++;
            if (s.deThap < -0.03f) lun++;
        }
        int n = Mathf.Max(1, ds.Count);
        sb.AppendLine(string.Format("TONG {0} - {1} cho: chan thap TB {2:F3} (thap nhat {3:F3}), chan ho TB {4:F3} (lon nhat {5:F3}); ho > 2 cm o {6} cho, lun > 3 cm o {7} cho",
            ten, ds.Count, tbThap / n, minThap, tbCao / n, maxCao, ho, lun));
    }

    static void Chup(Transform nv, string ten)
    {
        var goC = new GameObject("TAM_CamChan"); var cam = goC.AddComponent<Camera>(); cam.CopyFrom(Camera.main); cam.enabled = false;
        cam.fieldOfView = 30f; cam.nearClipPlane = 0.05f;
        Vector3 tam = nv.position + Vector3.up * 0.5f;
        goC.transform.position = tam + nv.right * 3f + Vector3.up * 0.15f; goC.transform.LookAt(tam);
        var rt = new RenderTexture(600, 600, 24); cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
        var cu = RenderTexture.active; RenderTexture.active = rt;
        var tx = new Texture2D(600, 600, TextureFormat.RGB24, false); tx.ReadPixels(new Rect(0, 0, 600, 600), 0, 0); tx.Apply();
        RenderTexture.active = cu;
        File.WriteAllBytes("PlayTestShots/" + ten + ".png", tx.EncodeToPNG());
        Object.Destroy(tx); Object.Destroy(rt); Object.Destroy(goC);
    }

    static void kiem(ref int loi, StringBuilder sb, bool dung, string l) { if (!dung) { loi++; sb.AppendLine("  LOI: " + l); } }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }
}
