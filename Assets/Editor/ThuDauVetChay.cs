using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 138 - DAU VET CHAY cua THIEN THACH (16/10/2026, nguoi dung: "Thien thach dot chay tat ca do vat, cay coi xong cung de lai dau vet chay den khi
/// tat ca do vat va cay coi xuat hien lai"; chon: tan tich chay den + tro (hinh dau vet Loc xoay), than hong am i ~10 giay roi nguoi).
/// Play Act2 that, tat GameDirector + xoa quai. Dot THAT (CayChay.Dot - duong Thien thach dung) mot CAY, mot BIA MO, mot NHA MO gan nhan vat:
///   A. Chay rui -> 1 dau vet CHAY moi vat, dung cho (tam bang nuong san <= 0,15 m), dung loai (Cay / Bia / NhaMo), vat bien mat.
///   B. Chay den: moi manh co lop phu Diablo25D/ThanChayPhu (khong phai VetMauPhu), _Color manh <= 0,2 mau goc; ho dat mau tro (toi hon dat xoi);
///      manh co va cham (MeshCollider bat).
///   C. Than hong NGUOI DAN - do tren ANH (doc lap bien _AmI): camera tam nhin xuong dau vet, dem diem anh DO RUC (do > luc x1,7, do > 0,35) luc 0,5 s
///      va luc 11 s sau khi chay rui; luc dau phai nhieu, sau 11 s ~0 (<= 15% luc dau). Khoi: co hat luc dau, 13 s sau 0 hat.
///   D. DOI CHUNG: dau vet Loc xoay (Tao khong chayDen) cung vat -> lop phu VetMauPhu, khong than hong tren anh.
///   E. Moc lai (GiayHoiSinh 30 s): vat hien lai, va cham dau vet tat ngay, dau vet bien mat trong 1 s.
/// Ket qua: PlayTestShots/dauvetchay.txt, anh dauvetchay_*.png.
/// </summary>
public static class ThuDauVetChay
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_DauVetChay";

    [MenuItem("Diablo 2.5D/138. Chay thu DAU VET CHAY cua Thien thach (cay · bia · nha mo)", false, 228)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[DauVetChay] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[DauVetChay] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] dau vet chay Thien thach");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DauVetChay] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find(Ten) != null) return;
        daBatDau = true;
        var go = new GameObject(Ten);
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    class Vat { public string nhom; public GameObject go; public Vector3 cho; public DauVetLocCuon dv; public float lucRui = -1f; public int doRucDau = -1, doRucSau = -1; }

    static GameObject GanNhat(string nhom, Vector3 goc, float toiThieu)
    {
        var cha = GameObject.Find("World/" + nhom);
        if (cha == null) return null;
        GameObject chon = null; float gan = 1e9f;
        foreach (Transform c in cha.transform)
        {
            if (!c.gameObject.activeInHierarchy || !CayChay.ChayDuoc(c.gameObject)) continue;
            if (!DauVetLocCuon.Bang().ContainsKey(DauVetLocCuon.Khoa(c.name, c.position))) continue;
            float d = Vector3.Distance(c.position, goc);
            if (d < toiThieu) continue;
            if (d < gan) { gan = d; chon = c.gameObject; }
        }
        return chon;
    }

    /// <summary>Dem diem anh DO RUC (than hong) trong anh chup tu tren xuong dau vet.</summary>
    static int DemDoRuc(Vector3 tam, float coVung, string tenAnh)
    {
        var goCam = new GameObject("TAM_CamChay");
        var cam = goCam.AddComponent<Camera>();
        cam.transform.position = tam + new Vector3(0f, coVung * 2.2f + 2f, -coVung * 0.6f);
        cam.transform.LookAt(tam);
        cam.fieldOfView = 40f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 60f;
        cam.clearFlags = CameraClearFlags.Skybox;
        var rt = new RenderTexture(320, 240, 24);
        cam.targetTexture = rt; cam.Render();
        var tx = new Texture2D(320, 240, TextureFormat.RGB24, false);
        var cu = RenderTexture.active; RenderTexture.active = rt;
        tx.ReadPixels(new Rect(0, 0, 320, 240), 0, 0); tx.Apply();
        RenderTexture.active = cu;
        // CHI vung giua anh (tan tich): lan 1 dem ca anh -> anh lua tan dan tren dat cua cay (mep anh) tinh nham thanh than hong (676 diem luc 11 s)
        int n = 0;
        var px = tx.GetPixels();
        for (int y = 60; y < 180; y++)
            for (int x = 90; x < 230; x++) { var p = px[y * 320 + x]; if (p.r > 0.35f && p.r > p.g * 1.7f && p.r > p.b * 1.7f) n++; }
        if (tenAnh != null) File.WriteAllBytes("PlayTestShots/" + tenAnh + ".png", tx.EncodeToPNG());
        cam.targetTexture = null; Object.Destroy(rt); Object.Destroy(tx); Object.Destroy(goCam);
        return n;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; Ket(); yield break; }
        dir.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(e.gameObject);
        var toi = dir.player.GetComponent<Damageable>();
        toi.maxHealth = toi.health = 1e7f;
        Vector3 goc = toi.transform.position;
        yield return new WaitForSeconds(0.5f);

        var ds = new List<Vat>();
        foreach (var nhom in new[] { "Cay", "BiaMo", "NhaMo" })
        {
            var g = GanNhat(nhom, goc, 4f);
            if (g == null) { Ghi("[LOI] khong tim thay " + nhom); loi++; continue; }
            ds.Add(new Vat { nhom = nhom, go = g, cho = g.transform.position });
        }
        // anh truoc
        foreach (var v in ds)
        {
            var sd0 = DauVetLocCuon.Bang()[DauVetLocCuon.Khoa(v.go.name, v.cho)];
            DemDoRuc(new Vector3(sd0.tam.x, v.cho.y, sd0.tam.y), Mathf.Max(1.5f, Mathf.Max(sd0.rong, sd0.day)), "dauvetchay_" + v.nhom + "_0_truoc");
        }
        int tao0 = DauVetLocCuon.SoLanTaoChay;
        foreach (var v in ds)
        {
            var c = CayChay.Dot(v.go, 0, 0f, null);
            Ghi("dot " + v.nhom + " " + v.go.name + ": " + (c != null ? "bat lua" : "KHONG bat lua"));
            Kiem(c != null, v.nhom + " khong bat lua");
        }

        // ===== A + B + C: cho chay rui, kiem dau vet =====
        float hanRui = Time.time + 40f;
        int conCho = ds.Count;
        while (conCho > 0 && Time.time < hanRui)
        {
            yield return null;
            foreach (var v in ds)
            {
                if (v.dv != null) continue;
                foreach (var d in DauVetLocCuon.DangCo)
                {
                    if (!d.chayDen) continue;
                    var sd = DauVetLocCuon.Bang()[DauVetLocCuon.Khoa(v.go.name, v.cho)];
                    if (new Vector2(d.transform.position.x - sd.tam.x, d.transform.position.z - sd.tam.y).magnitude < 0.15f) { v.dv = d; v.lucRui = Time.time; conCho--; break; }
                }
            }
        }
        Ghi("");
        foreach (var v in ds)
        {
            if (v.dv == null) { Ghi("A. " + v.nhom + ": KHONG co dau vet chay sau 40 s"); loi++; continue; }
            bool anHinh = true; foreach (var r in v.go.GetComponentsInChildren<Renderer>()) if (r.enabled && !(r is ParticleSystemRenderer)) anHinh = false;
            string kieuMong = v.nhom == "Cay" ? "Cay" : v.nhom == "BiaMo" ? "Bia" : "NhaMo";
            // B
            int soManh = 0, coThan = 0, coVetMau = 0, coVaCham = 0; float toiNhat = 0f;
            var goc0 = v.go.GetComponentInChildren<MeshRenderer>(true);
            Color mauGoc = goc0 != null && goc0.sharedMaterial != null && goc0.sharedMaterial.HasProperty("_Color") ? goc0.sharedMaterial.GetColor("_Color") : Color.white;
            var mpb = new MaterialPropertyBlock();
            foreach (var r in v.dv.GetComponentsInChildren<MeshRenderer>())
            {
                if (r.transform == v.dv.ho) continue;
                soManh++;
                foreach (var m in r.sharedMaterials) { if (m == null) continue; if (m.shader.name == "Diablo25D/ThanChayPhu") coThan++; if (m.shader.name == "Diablo25D/VetMauPhu") coVetMau++; }
                r.GetPropertyBlock(mpb);
                Color c = mpb.GetColor("_Color");
                toiNhat = Mathf.Max(toiNhat, c.maxColorComponent / Mathf.Max(0.01f, mauGoc.maxColorComponent));
                var mc = r.GetComponent<MeshCollider>(); if (mc != null && mc.enabled) coVaCham++;
            }
            var hoR = v.dv.ho != null ? v.dv.ho.GetComponent<MeshRenderer>() : null;
            Color mauHo = Color.white; if (hoR != null) { hoR.GetPropertyBlock(mpb); mauHo = mpb.GetColor("_Color"); }
            Ghi(string.Format("A. {0} ({1}): dau vet chay '{2}' kieu {3} (mong {4}), vat an hinh {5}", v.nhom, v.go.name, v.dv.name, v.dv.kieu, kieuMong, anHinh));
            Ghi(string.Format("B. {0}: {1} manh, lop than chay {2}, lop vet mau {3}, mau manh / goc lon nhat {4:F2}, ho mau ({5:F2} {6:F2} {7:F2}) so dat xoi 1,0; manh co va cham {8}",
                v.nhom, soManh, coThan, coVetMau, toiNhat, mauHo.r, mauHo.g, mauHo.b, coVaCham));
            Kiem(v.dv.kieu.ToString() == kieuMong && anHinh, v.nhom + ": dau vet sai loai / vat chua bien mat");
            Kiem(soManh > 0 && coThan == soManh && coVetMau == 0 && toiNhat <= 0.2f && mauHo.maxColorComponent < 0.3f, v.nhom + ": dau vet khong chay den");
            Kiem(coVaCham == soManh, v.nhom + ": manh dau vet chay khong co va cham");
        }

        // C: than hong tren anh luc ~0,5 s va 11 s
        Ghi("");
        foreach (var v in ds)
        {
            if (v.dv == null) continue;
            while (Time.time - v.lucRui < 0.5f) yield return null;
            var sd = DauVetLocCuon.Bang()[DauVetLocCuon.Khoa(v.go.name, v.cho)];
            v.doRucDau = DemDoRuc(v.dv.transform.position, Mathf.Max(1.5f, Mathf.Max(sd.rong, sd.day)), "dauvetchay_" + v.nhom + "_1_am_i");
        }
        int hatDau = 0; foreach (var v in ds) if (v.dv != null) foreach (var ps in v.dv.GetComponentsInChildren<ParticleSystem>()) hatDau += ps.particleCount;
        foreach (var v in ds)
        {
            if (v.dv == null) continue;
            while (Time.time - v.lucRui < 11f) yield return null;
            var sd = DauVetLocCuon.Bang()[DauVetLocCuon.Khoa(v.go.name, v.cho)];
            v.doRucSau = DemDoRuc(v.dv.transform.position, Mathf.Max(1.5f, Mathf.Max(sd.rong, sd.day)), "dauvetchay_" + v.nhom + "_2_nguoi");
        }
        float hanHat = Time.time + 2.5f; while (Time.time < hanHat) yield return null;
        int hatSau = 0; foreach (var v in ds) if (v.dv != null) foreach (var ps in v.dv.GetComponentsInChildren<ParticleSystem>()) hatSau += ps.particleCount;
        foreach (var v in ds)
        {
            if (v.dv == null) continue;
            Ghi(string.Format("C. {0}: diem anh DO RUC luc 0,5 s {1}, luc 11 s {2}; AmI luc 11 s {3:F2}", v.nhom, v.doRucDau, v.doRucSau, v.dv.AmIHienTai));
            Kiem(v.doRucDau >= 15 && v.doRucSau <= Mathf.Max(3, v.doRucDau * 0.15f), v.nhom + ": than hong khong am i / khong nguoi dan");
        }
        Ghi(string.Format("C. khoi: {0} hat luc dau, {1} hat luc 13,5 s", hatDau, hatSau));
        Kiem(hatDau > 5 && hatSau == 0, "khoi khong boc luc dau / khong tat sau 10 s");

        // D: doi chung dau vet Loc xoay
        Ghi("");
        {
            var v = ds.Count > 0 ? ds[0] : null;
            if (v != null && v.dv != null)
            {
                Bounds bh = new Bounds(v.cho, Vector3.one);
                var mr0 = v.go.GetComponentInChildren<MeshRenderer>(true);
                var dl = DauVetLocCuon.Tao(v.go, v.cho + new Vector3(0.001f, 0f, 0f) * 0f, v.go.transform.rotation, bh, mr0 != null ? mr0.sharedMaterial : null);
                dl.transform.position += new Vector3(30f, 0f, 30f);       // dat xa de anh khong lan voi dau vet chay
                Physics.SyncTransforms();
                int vetMau = 0, than = 0;
                foreach (var r in dl.GetComponentsInChildren<MeshRenderer>()) foreach (var m in r.sharedMaterials) { if (m == null) continue; if (m.shader.name == "Diablo25D/VetMauPhu") vetMau++; if (m.shader.name == "Diablo25D/ThanChayPhu") than++; }
                yield return null;
                int doRuc = DemDoRuc(dl.transform.position, 2.5f, "dauvetchay_doichung_loc");
                Ghi(string.Format("D. DOI CHUNG dau vet Loc xoay cung vat: chayDen {0}, lop vet mau {1}, lop than {2}, diem anh do ruc {3}", dl.chayDen, vetMau, than, doRuc));
                Kiem(!dl.chayDen && vetMau > 0 && than == 0, "dau vet Loc xoay bi doi sang chay den");
                Object.Destroy(dl.gameObject);
            }
        }

        // E: moc lai
        Ghi("");
        // theo doi CA BA song song (bia / nha chay rui som hon cay - lan 1 cho tung cai thi bo lo)
        var lucMoc = new Dictionary<Vat, float>(); var vaChamMoc = new Dictionary<Vat, int>(); var hienMoc = new Dictionary<Vat, bool>();
        var dvCu = new Dictionary<Vat, DauVetLocCuon>(); foreach (var v in ds) if (v.dv != null) dvCu[v] = v.dv;
        float hanE = Time.time + CayChay.GiayHoiSinh + 5f;
        while (lucMoc.Count < dvCu.Count && Time.time < hanE)
        {
            yield return null;
            foreach (var kv in dvCu)
            {
                var v = kv.Key;
                if (lucMoc.ContainsKey(v) || v.go.GetComponent<CayChay>() != null) continue;
                lucMoc[v] = Time.time - v.lucRui;
                bool hien = false; foreach (var r in v.go.GetComponentsInChildren<Renderer>()) if (r.enabled && !(r is ParticleSystemRenderer)) hien = true;
                hienMoc[v] = hien;
                int vc = 0; if (kv.Value != null) foreach (var c in kv.Value.GetComponentsInChildren<Collider>()) if (c.enabled) vc++;
                vaChamMoc[v] = vc;
            }
        }
        float hanMat = Time.time + 1.5f; while (Time.time < hanMat) yield return null;
        foreach (var kv in dvCu)
        {
            var v = kv.Key;
            bool daMoc = lucMoc.ContainsKey(v);
            Ghi(string.Format("E. {0}: moc lai sau {1:F1} s, hien hinh {2}; va cham dau vet con bat {3}; dau vet bien mat {4}", v.nhom,
                daMoc ? lucMoc[v] : -1f, daMoc && hienMoc[v], daMoc ? vaChamMoc[v] : -1, kv.Value == null));
            Kiem(daMoc && hienMoc[v] && vaChamMoc[v] == 0 && kv.Value == null && Mathf.Abs(lucMoc[v] - CayChay.GiayHoiSinh) < 1f,
                 v.nhom + ": moc lai ma dau vet chay con / va cham con / sai gio");
        }
        Ghi(string.Format("tong dau vet chay da tao: {0}", DauVetLocCuon.SoLanTaoChay - tao0));
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/dauvetchay.txt", bao.ToString());
        TranHienTai.Xoa();
        var r = GameObject.Find(Ten);
        if (r != null) Object.DestroyImmediate(r);
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
        Debug.Log("[DauVetChay] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
