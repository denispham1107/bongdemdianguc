using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 156 - TANG BANG VO: BO VOM TRANG, CHI CON MANH BANG + SUONG LANH (01/11/2026, nguoi dung gui anh: "luc moi be xuat hien 1 vom
/// trang duc qua xau; chi cho khoi suong lanh va cac manh bang vang ra").
///
/// Vao Play Act2 (dem), tat GameDirector:
///   A. Tang bang THAT (IceImpact + TangBangNo.Gan) no: suot doi vu no 0 luoi dang hien (ChopLanh, VongLanh), 0 den dang bat; manh bang
///      (Shards, dang luoi) bay xa khoi cho no, suong (SuongNo + Puff) co hat.
///   B. DOI CHUNG doc lap: them lai dung cai vom cu (hinh cau Mats.Ice 0,25 -> 0,95 ban kinh, 0,3 s) vao cung cho, cung may quay; dem diem anh
///      GAN TRANG tren anh (may quay game co bloom). Moi &lt; 25% cu. Anh: PlayTestShots/vo_tang_bang/.
///   C. DOI CHUNG: quai dong bang chet vo tan (FrozenShatter) GIU NGUYEN den "Flash" cua prefab.
/// </summary>
public static class ThuVoTangBang
{
    const string Thu = "PlayTestShots/vo_tang_bang/";
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/156 Tang bang vo: bo vom trang, chi con manh bang + suong lanh", false, 230)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuVoTangBang] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        nk.Length = 0; loi = 0; daBatDau = false;
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
        var go = new GameObject("TAM_ThuVoTangBang");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    /// <summary>Vat cua vu no moi sinh: goc "NoTangBang" + goc vo bang (prefab "...VoBang..." / "Shatter").</summary>
    static List<GameObject> VatCuaVuNo(HashSet<GameObject> truoc)
    {
        var ds = new List<GameObject>();
        foreach (var g in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
        {
            if (g.transform.parent != null || truoc.Contains(g)) continue;
            if (g.name.StartsWith("NoTangBang") || g.name.Contains("VoBang") || g.name.StartsWith("Shatter")) ds.Add(g);
        }
        return ds;
    }

    static HashSet<GameObject> ChupGoc()
    {
        var hs = new HashSet<GameObject>();
        foreach (var g in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude)) if (g.transform.parent == null) hs.Add(g);
        return hs;
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        TamNhin.BatTrongPhepThu = true;
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var cam = Camera.main;
        var rig = cam.GetComponent<CameraRig>();
        if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
        yield return new WaitForSeconds(1.0f);
        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 cho = toi.transform.position + F * 8f;
        cho.y = VfxFactory.GroundY(cho);

        // ================= A. Tang bang that =================
        nk.AppendLine("== A Tang bang THAT no");
        {
            var tang = VfxFactory.IceImpact(cho, 1.7f, true);
            TangBangNo.Gan(tang, 0, null);
            var truoc = ChupGoc();
            int no0 = TangBangNo.SoLanNo;
            for (float h = Time.time + 6f; TangBangNo.SoLanNo == no0 && Time.time < h; ) yield return null;
            Kiem(TangBangNo.SoLanNo == no0 + 1, "tang bang no " + (TangBangNo.SoLanNo - no0) + " lan");
            var vat = VatCuaVuNo(truoc);
            int luoiMax = 0, denMax = 0, shardsMax = 0, suongMax = 0, puffMax = 0, shardsMesh = 0;
            float xaNhat = 0f;
            var buf = new ParticleSystem.Particle[256];
            for (float h = Time.time + 1.2f; Time.time < h; )
            {
                int luoi = 0, den = 0;
                foreach (var g in vat)
                {
                    if (g == null) continue;
                    foreach (var r in g.GetComponentsInChildren<MeshRenderer>(false)) if (r.enabled) luoi++;
                    foreach (var l in g.GetComponentsInChildren<Light>(false)) if (l.enabled) den++;
                    foreach (var ps in g.GetComponentsInChildren<ParticleSystem>(false))
                    {
                        if (ps.name == "SuongNo") suongMax = Mathf.Max(suongMax, ps.particleCount);
                        else if (ps.name == "Puff") puffMax = Mathf.Max(puffMax, ps.particleCount);
                        else if (ps.name == "Shards")
                        {
                            shardsMax = Mathf.Max(shardsMax, ps.particleCount);
                            if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Mesh) shardsMesh = 1;
                            int n = ps.GetParticles(buf);
                            for (int i = 0; i < n; i++) xaNhat = Mathf.Max(xaNhat, Vector3.Distance(buf[i].position, cho));
                        }
                    }
                }
                luoiMax = Mathf.Max(luoiMax, luoi); denMax = Mathf.Max(denMax, den);
                yield return null;
            }
            Kiem(vat.Count >= 2, "tim thay " + vat.Count + " goc vu no (NoTangBang + vo bang)");
            Kiem(luoiMax == 0, "luoi dang hien lon nhat " + luoiMax + " (phai 0 - khong vom, khong vong)");
            Kiem(denMax == 0, "den dang bat lon nhat " + denMax + " (phai 0)");
            Kiem(shardsMax > 20 && shardsMesh == 1, "manh bang " + shardsMax + " hat, dang luoi " + shardsMesh);
            Kiem(xaNhat > 1.5f, "manh bang van ra xa nhat " + xaNhat.ToString("F2") + " m");
            Kiem(suongMax > 5 && puffMax > 5, "suong lanh: SuongNo " + suongMax + ", Puff " + puffMax + " hat");
        }
        yield return new WaitForSeconds(1.5f);

        // ================= B. Anh: moi / doi chung vom cu =================
        nk.AppendLine("== B Anh, doi chung them lai vom cu");
        {
            // Chup bang CHINH may quay game (co bloom nhu nguoi choi thay), khong may quay sao chep
            if (rig != null) rig.enabled = false;
            cam.transform.rotation = Quaternion.LookRotation(cho + Vector3.up * 0.8f - cam.transform.position);
            var cc = cam;
            Time.captureDeltaTime = 1f / 60f;
            yield return null; yield return null;
            var nen = Chup(cc);
            int[] dinh = new int[2];
            for (int lan = 0; lan < 2; lan++)
            {
                VfxFactory.NoTangBang(cho, TangBangNo.BanKinh);
                if (lan == 0)
                {
                    // Vom cu viet tay dung thong so ban truoc 01/11/2026
                    var goc = new GameObject("TAM_VomCu");
                    goc.transform.position = cho;
                    var chop = ProcMesh.Part("ChopLanh", goc.transform, ProcMesh.Sphere(1f, 14, 10, 1f, Color.white),
                                             new Material(Mats.Ice), Vector3.up * 0.4f, Quaternion.identity, Vector3.one, false);
                    var cf = chop.AddComponent<ExpandFade>();
                    cf.duration = 0.3f;
                    cf.startScale = Vector3.one * TangBangNo.BanKinh * 0.25f;
                    cf.endScale = Vector3.one * TangBangNo.BanKinh * 0.95f;
                    Object.Destroy(goc, 1f);
                }
                for (int k = 1; k <= 24; k++)
                {
                    yield return null;
                    var tx = Chup(cc);
                    dinh[lan] = Mathf.Max(dinh[lan], DemSangLen(nen, tx));
                    if (k == 6 || k == 14) File.WriteAllBytes(Thu + (lan == 0 ? "cu_" : "moi_") + k + ".png", tx.EncodeToPNG());
                    Object.Destroy(tx);
                }
                for (int i = 0; i < 150; i++) yield return null;
            }
            Time.captureDeltaTime = 0f;
            nk.AppendLine("  diem anh GAN TRANG lon nhat: doi chung vom cu " + dinh[0] + " / moi " + dinh[1] + " (tren 640x480)");
            Kiem(dinh[0] > 1000 && dinh[1] * 4 < dinh[0], "vom tren anh: cu " + dinh[0] + ", moi " + dinh[1] + " (moi < 25% cu)");
            Object.Destroy(nen);
            if (rig != null) rig.enabled = true;
        }

        // ================= C. Doi chung: quai dong bang chet =================
        nk.AppendLine("== C DOI CHUNG FrozenShatter cua quai dong bang chet");
        {
            var vo = VfxFactory.FrozenShatter(cho + Vector3.up, 1f);
            var flash = vo != null ? vo.transform.Find("Flash") : null;
            Kiem(flash != null && flash.gameObject.activeSelf, "FrozenShatter giu den Flash: " + (flash != null ? flash.gameObject.activeSelf.ToString() : "khong co"));
        }
        yield return new WaitForSeconds(0.5f);

        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 156 - Tang bang vo: bo vom trang: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuVoTangBang] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuVoTangBang");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
    }

    static Texture2D Chup(Camera cc)
    {
        var rt = RenderTexture.GetTemporary(640, 480, 24);
        cc.targetTexture = rt; cc.Render(); cc.targetTexture = null;
        RenderTexture.active = rt;
        var tx = new Texture2D(640, 480, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 640, 480), 0, 0); tx.Apply();
        RenderTexture.active = null; RenderTexture.ReleaseTemporary(rt);
        return tx;
    }

    /// <summary>So diem anh GAN TRANG (ca 3 kenh >= 215) ma nen khong trang - vom cu chay trang, suong mem + manh bang thi it.</summary>
    static int DemSangLen(Texture2D nen, Texture2D tx)
    {
        var a = nen.GetPixels32(); var b = tx.GetPixels32();
        int n = 0;
        for (int i = 0; i < a.Length; i++)
            if (b[i].r >= 215 && b[i].g >= 215 && b[i].b >= 215 && !(a[i].r >= 215 && a[i].g >= 215 && a[i].b >= 215)) n++;
        return n;
    }
}
