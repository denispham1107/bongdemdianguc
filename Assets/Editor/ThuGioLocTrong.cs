using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: THAN GIO LOC - DO TRONG / DO NOI TREN NEN DEM (nguoi dung 28/09/2026). Menu 71c.
/// Khuya 28/09/2026: sau khi doi sang MAU MAY GIONG, than toi + trong chim vao nen dem; nguoi dung chon do duc cu + mau may pha
/// sang x1,5 -> phep thu nay nay do "than NOI tren nen" (che nen) so voi DOI CHUNG la ban mau may goc do duc thap.
///
/// Dung hinh Gio loc dung yen trong Act2, may quay co dinh, TAT bloom. Moi khung render HAI lan vao cung mot RenderTexture:
/// co bui (BuiCuon + KhoiBui) va TAT bui. Trong vung than duoi (0 - 2,5 m):
///   - "bui lo ra" = trung binh |do sang co bui - tat bui| -> vo cang trong, bui cang lo;
///   - "do trang dac" = do sang trung binh anh TAT bui va ti le diem chay trang (> 0,85).
/// Do voi do duc MOI (VfxFactory.DoDucVoGioLoc) va DOI CHUNG do duc CU (0,72 / 0,34 / 0,26 / 0,90) trong cung mot luot, 12 khung moi ben.
/// Ket qua PlayTestShots/gioloc_trong.txt, anh gioloc_trong_moi.png / gioloc_trong_cu.png.
/// </summary>
public static class ThuGioLocTrong
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    // DOI CHUNG (28/09/2026 khuya): ban MAU MAY GIONG goc (he so sang 1) + do duc thap - ban da CHIM vao nen dem
    static readonly float[] DoDucCu = { 0.42f, 0.20f, 0.16f, 0.70f };
    static readonly float[] TMayCu = { 0f, 0.5f, 1f, 1f };

    [MenuItem("Diablo 2.5D/71c. Do THAN GIO LOC trong (thay bui ben trong)", false, 160)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[GioLocTrong] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_GioLocTrong");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GioLocTrong] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] than Gio loc trong hon - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        yield return new WaitForSeconds(1.5f);
        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        var cam = Camera.main;
        var bloom = cam != null ? cam.GetComponent<SimpleBloom>() : null;
        if (bloom != null) bloom.enabled = false;

        // Cho dung: truoc mat nhan vat 8 m, bam dat
        Vector3 goc = toi != null ? toi.transform.position : Vector3.zero;
        Vector3 huong = toi != null ? toi.transform.forward : Vector3.forward; huong.y = 0f; huong.Normalize();
        Vector3 P = goc + huong * 8f;
        P.y = GioLoc.MatDatY(P, P.y);
        var loc = VfxFactory.BuildGioLoc();
        loc.name = "TAM_GioLocHinh";
        loc.transform.position = P;
        cam.transform.position = P - huong * 9f + Vector3.up * 3.2f;
        cam.transform.LookAt(P + Vector3.up * 1.8f);
        // 29/09/2026 BUI DAY x2: DOI CHUNG = mot Gio loc nua, bui cuon tra ve muc cu 40 hat/giay, tran 120 - dat SAU LUNG may quay
        // (ngoai anh). Ca hai AlwaysSimulate: ngoai khung hinh, culling Automatic dung mo phong va dem ra 0.
        var locDc = VfxFactory.BuildGioLoc();
        locDc.name = "TAM_GioLocDoiChung";
        locDc.transform.position = cam.transform.position - huong * 30f;
        ParticleSystem buiMoi = null, buiDc = null;
        foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>()) if (ps.name == "BuiCuon") buiMoi = ps;
        foreach (var ps in locDc.GetComponentsInChildren<ParticleSystem>()) if (ps.name == "BuiCuon") buiDc = ps;
        {
            var m = buiDc.main; m.maxParticles = 120; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var e = buiDc.emission; e.rateOverTime = 40f;
            var m2 = buiMoi.main; m2.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        }
        yield return new WaitForSeconds(3.5f);            // cho bui day (doi song hat 1,6 - 3,0 s)
        {
            int nMoi = 0, nDc = 0;
            for (int k = 0; k < 10; k++) { nMoi += buiMoi.particleCount; nDc += buiDc.particleCount; yield return new WaitForSeconds(0.1f); }
            Ghi(string.Format("BUI DAY: hat bui cuon dang song (tb 10 lan do) moi {0:F1} / doi chung 40 hat/giay {1:F1} = x{2:F2}",
                nMoi / 10f, nDc / 10f, (float)nMoi / Mathf.Max(1, nDc)));
            Kiem(nDc > 20, "doi chung: bui cuon khong co hat - phep dem vo nghia");
            Kiem((float)nMoi / Mathf.Max(1, nDc) > 1.7f, "bui cuon khong day them ~x2");
        }
        Object.Destroy(locDc);

        var vo = new List<MeshRenderer>();
        foreach (var mr in loc.GetComponentsInChildren<MeshRenderer>()) vo.Add(mr);
        var bui = new List<Renderer>();
        foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>())
            if (ps.name == "BuiCuon" || ps.name == "KhoiBui" || ps.name == "BuiCuonLen") bui.Add(ps.GetComponent<Renderer>());   // BuiCuonLen: 29/09/2026
        Ghi(string.Format("dung hinh: {0} lop vo ({1}), {2} he bui", vo.Count, string.Join(",", vo.ConvertAll(v => v.name).ToArray()), bui.Count));
        Kiem(vo.Count == 4 && bui.Count == 3, "khong du 4 lop vo / 3 he bui (bui chan, vet, cuon len)");

        // Vung than duoi 0 - 2,5 m tren man hinh (ban kinh chan moi ~1,1 m)
        int W = 640, H = 360;
        var rt = new RenderTexture(W, H, 24);
        float xMin = 1f, xMax = 0f, yMin = 1f, yMax = 0f;
        foreach (var dy in new[] { 0.1f, 1.2f, 2.5f })
            foreach (var dx in new[] { -1.2f, 1.2f })
            {
                var v = cam.WorldToViewportPoint(P + Vector3.up * dy + cam.transform.right * dx);
                xMin = Mathf.Min(xMin, v.x); xMax = Mathf.Max(xMax, v.x); yMin = Mathf.Min(yMin, v.y); yMax = Mathf.Max(yMax, v.y);
            }
        var o = new RectInt(Mathf.RoundToInt(xMin * W), Mathf.RoundToInt(yMin * H), Mathf.RoundToInt((xMax - xMin) * W), Mathf.RoundToInt((yMax - yMin) * H));
        Ghi("vung do (diem anh 640x360): " + o);

        var kq = new Dictionary<string, float[]>();
        // Mau MOI dung nhu code dung ra - cat lai de tra ve sau luot doi chung
        var mauGoc = new Dictionary<MeshRenderer, Color>();
        foreach (var mr in vo) mauGoc[mr] = mr.sharedMaterial.GetColor("_TintColor");
        foreach (var ten in new[] { "moi", "cu" })
        {
            float[] doDuc = ten == "moi" ? VfxFactory.DoDucVoGioLoc : DoDucCu;
            string[] thuTu = { "Vo0", "Vo1", "Vo2", "DaiGio" };
            foreach (var mr in vo)
            {
                int i = System.Array.IndexOf(thuTu, mr.name); if (i < 0) i = 3;
                Color c;
                if (ten == "moi") c = mauGoc[mr];
                else { c = Color.Lerp(VfxFactory.MauMayGiongXam, VfxFactory.MauMayGiongSang, TMayCu[i]); c.a = doDuc[i]; }
                mr.sharedMaterial.SetColor("_TintColor", c);
            }
            yield return new WaitForSeconds(0.3f);
            float loTong = 0f, trangTong = 0f, chayTong = 0f, cheTong = 0f; int soKhung = 0;
            for (int k = 0; k < 12; k++)
            {
                yield return new WaitForEndOfFrame();
                var co = Chup(cam, rt, W, H);
                foreach (var r in bui) r.enabled = false;
                var khong = Chup(cam, rt, W, H);
                // NEN: tat ca vo lan bui - vo cang trong thi anh "tat bui" cang gan nen (dung cho ca vo trang lan vo toi mau may giong)
                foreach (var v in vo) v.enabled = false;
                var nen = Chup(cam, rt, W, H);
                foreach (var v in vo) v.enabled = true;
                foreach (var r in bui) r.enabled = true;
                float lo = 0f, trang = 0f, che = 0f; int chay = 0, n = 0;
                for (int y = o.yMin; y < o.yMax; y++)
                    for (int x = o.xMin; x < o.xMax; x++)
                    {
                        int id = y * W + x;
                        float a = Sang(co[id]), b = Sang(khong[id]);
                        lo += Mathf.Abs(a - b); trang += b; che += Mathf.Abs(b - Sang(nen[id])); if (b > 0.85f) chay++; n++;
                    }
                loTong += lo / n; trangTong += trang / n; chayTong += (float)chay / n; cheTong += che / n; soKhung++;
                yield return new WaitForSeconds(0.12f);
            }
            kq[ten] = new[] { loTong / soKhung, trangTong / soKhung, chayTong / soKhung, cheTong / soKhung };
            Ghi(string.Format("{0} (do duc {1}): bui lo ra {2:F4}, do sang than (tat bui) {3:F3}, diem chay trang {4:P1}, than che nen {5:F4}",
                ten == "moi" ? "MOI" : "DOI CHUNG (may goc, trong)", string.Join("/", System.Array.ConvertAll(doDuc, v => v.ToString("F2"))),
                kq[ten][0], kq[ten][1], kq[ten][2], kq[ten][3]));
            yield return ChupAnh(cam, "PlayTestShots/gioloc_trong_" + ten + ".png");
        }
        Ghi(string.Format("=> bui lo ra x{0:F2}, do sang than x{1:F2}, than che nen x{2:F2}", kq["moi"][0] / Mathf.Max(1e-6f, kq["cu"][0]),
            kq["moi"][1] / Mathf.Max(1e-6f, kq["cu"][1]), kq["moi"][3] / Mathf.Max(1e-6f, kq["cu"][3])));
        // 28/09/2026 khuya: muc tieu DOI - than MAU MAY GIONG phai NOI len nen dem (nguoi dung chon do duc cu + mau sang hon); chap
        // nhan kho thay bui hon. "Noi" = than che nen NHIEU hon ban doi chung (mau may goc, do duc thap - ban da chim vao nen).
        Kiem(kq["cu"][3] > 0.002f, "doi chung: than khong khac nen chut nao - phep do vo nghia");
        // 29/09/2026: den hon 1 chut (x2,25) - quet do duoc noi x1,13 so ban chim; van phai noi hon ban chim
        Kiem(kq["moi"][3] > kq["cu"][3] * 1.05f, "than moi khong noi len nen dem hon ban chim (it nhat x1,05)");

        // 29/09/2026 (nguoi dung: "cho toan than DEN hon 1 chut", chon x2,25 cho CA THAN LAN BUI sau khi xem anh quet 2,5/2,25/2,0/1,75):
        // DOI CHUNG = ban truoc x2,5 (vo + bui cuon + vet bui). Do do sang CA CON LOC (co bui) trong vung than, dem lan ngay.
        {
            var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
            if (chuyen != null) chuyen.enabled = false;
            Kiem(chuyen != null, "khong tim thay ChuyenChieuSangDem - khong chup duoc anh ban ngay");
            var heBui = new List<ParticleSystem>();
            // 29/09/2026: ca lop BuiCuonLen (moi) - bo sot thi lop ay giu x2,25 o CA HAI ban, pha loang chenh lech (do ra x0,98)
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>()) if (ps.name == "BuiCuon" || ps.name == "KhoiBui" || ps.name == "BuiCuonLen") heBui.Add(ps);
            var buiGoc = new Dictionary<ParticleSystem, ParticleSystem.MinMaxGradient>();
            foreach (var ps in heBui) buiGoc[ps] = ps.main.startColor;
            string[] thuTu = { "Vo0", "Vo1", "Vo2", "DaiGio" };
            float[] tMay = { 0f, 0.5f, 1f, 1f };
            var sang = new Dictionary<string, float[]>();
            foreach (var ban in new[] { "truoc", "moi" })
            {
                float k = ban == "truoc" ? 2.5f : VfxFactory.HeSoSangMayGioLoc;
                foreach (var mr in vo)
                {
                    int i = System.Array.IndexOf(thuTu, mr.name); if (i < 0) i = 3;
                    var c = Color.Lerp(VfxFactory.MauMayGiongXam, VfxFactory.MauMayGiongSang, tMay[i]) * k;
                    c.a = VfxFactory.DoDucVoGioLoc[i];
                    mr.sharedMaterial.SetColor("_TintColor", ban == "moi" ? mauGoc[mr] : c);
                }
                foreach (var ps in heBui)
                {
                    var m = ps.main;
                    if (ban == "moi") m.startColor = buiGoc[ps];
                    else
                    {
                        var c0 = VfxFactory.MauMayGiongXam * 2.5f; c0.a = 0.70f;
                        var c1 = VfxFactory.MauMayGiongSang * 2.5f; c1.a = 0.90f;
                        m.startColor = new ParticleSystem.MinMaxGradient(c0, c1);
                    }
                    ps.Clear();                       // hat cu giu mau cu - xoa roi cho hat moi day lai
                }
                yield return new WaitForSeconds(3.5f);
                var kqBan = new float[2];
                for (int buoi = 0; buoi < 2; buoi++)
                {
                    if (chuyen != null) { chuyen.ApGiay(buoi == 0 ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); yield return null; yield return null; }
                    float t = 0f;
                    for (int kk = 0; kk < 10; kk++)
                    {
                        yield return new WaitForEndOfFrame();
                        var px = Chup(cam, rt, W, H);
                        float a = 0f; int n = 0;
                        for (int y = o.yMin; y < o.yMax; y++)
                            for (int x = o.xMin; x < o.xMax; x++) { a += Sang(px[y * W + x]); n++; }
                        t += a / n;
                        yield return new WaitForSeconds(0.1f);
                    }
                    kqBan[buoi] = t / 10f;
                    yield return ChupAnh(cam, "PlayTestShots/gioloc_den_" + ban + (buoi == 0 ? "_dem" : "_ngay") + ".png");
                }
                if (chuyen != null) { chuyen.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); yield return null; }
                sang[ban] = kqBan;
                Ghi(string.Format("{0} (mau may x{1:F2}, ca vo lan bui): do sang ca con loc (co bui) DEM {2:F3}, NGAY {3:F3}",
                    ban == "moi" ? "MOI" : "DOI CHUNG ban truoc", k, kqBan[0], kqBan[1]));
            }
            float tlDem = sang["moi"][0] / Mathf.Max(1e-6f, sang["truoc"][0]), tlNgay = sang["moi"][1] / Mathf.Max(1e-6f, sang["truoc"][1]);
            Ghi(string.Format("=> den hon: dem x{0:F2}, ngay x{1:F2} so ban x2,5", tlDem, tlNgay));
            Kiem(tlNgay < 0.95f && tlDem < 0.97f, "con loc khong den hon ban x2,5 (ca vo lan bui)");
        }

        Object.Destroy(rt);
        Object.Destroy(loc);
        Ket();
    }

    /// <summary>Do sang trung binh vung than (TAT bui), trung binh 4 lan render.</summary>
    static float DoSangThan(Camera cam, RenderTexture rt, int W, int H, RectInt o, List<Renderer> bui)
    {
        float tong = 0f;
        foreach (var r in bui) r.enabled = false;
        for (int k = 0; k < 4; k++)
        {
            var px = Chup(cam, rt, W, H);
            float t = 0f; int n = 0;
            for (int y = o.yMin; y < o.yMax; y++)
                for (int x = o.xMin; x < o.xMax; x++) { t += Sang(px[y * W + x]); n++; }
            tong += t / n;
        }
        foreach (var r in bui) r.enabled = true;
        return tong / 4f;
    }

    static Color[] Chup(Camera cam, RenderTexture rt, int W, int H)
    {
        var cu = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        var px = t.GetPixels();
        Object.Destroy(t);
        return px;
    }

    static IEnumerator ChupAnh(Camera cam, string duong)
    {
        yield return new WaitForEndOfFrame();
        int W = 960, H = 540;
        var rt = new RenderTexture(W, H, 24);
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        File.WriteAllBytes(duong, t.EncodeToPNG());
        Object.Destroy(t); Object.Destroy(rt);
        Ghi("anh: " + duong);
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/gioloc_trong.txt", bao.ToString());
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
