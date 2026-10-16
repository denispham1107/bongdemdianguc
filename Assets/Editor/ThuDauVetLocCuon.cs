using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 135 - DAU VET VAT BI LOC XOAY CUON (16/10/2026, nguoi dung: "de lai dau vet cua cac do vat da cuon bay len chu khong de mat dat trong tron";
/// chon RIENG TUNG LOAI, ton tai DEN KHI VAT MOC LAI). Play Act2 that, tat GameDirector + xoa quai.
///   A. Chon 1 vat moi loai (cay, bia mo, da, nha mo, lo lua) gan nhan vat nhat ma Loc xoay cuon duoc; chup anh TRUOC (cung goc).
///   B. Tha Loc xoay THAT (Tornado.Spawn) ngay tai tung vat: sau 1,2 s moi vat co MOT dau vet dung kieu, dung cho cu (lech ngang < 0,05 m), manh tren
///      dung CHINH vat lieu cua vat, ho dat bam mat dat (vanh ngoai lech mat Ground < 0,06 m, dinh lo lung > 0,15 m: 0).
///   C. Anh SAU (loc da tan, cung goc) -> PlayTestShots/dauvet_[loai]_truoc|sau.png.
///   D. NGOAI TAM NHIN: dua nhan vat ra xa > tam nhin + 5 m -> moi renderer dau vet bi an (TamNhin), ve lai gan -> hien.
///   E. VAT MOC LAI (30 s): dau vet lun roi xoa het (con 0), SoLanXoa tang dung so.
/// Ket qua: PlayTestShots/dauvetloccuon.txt.
/// </summary>
public static class ThuDauVetLocCuon
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_ThuDauVetLocCuon";

    [MenuItem("Diablo 2.5D/135. Chay thu DAU VET Loc xoay cuon (goc cay · chan bia · nen nha · ho dat)", false, 224)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[DauVetLoc] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[DauVetLoc] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] dau vet vat bi Loc xoay cuon - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DauVetLoc] " + s); }
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

    static float Ngang(Vector3 a, Vector3 b) { a.y = 0; b.y = 0; return Vector3.Distance(a, b); }

    static void DatCho(Transform t, Vector3 p)
    {
        var cc = t.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        t.position = p;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }

    class MucTieu { public string ten; public DauVetLocCuon.Kieu kieu; public GameObject vat; public Vector3 cho; public Material vl; public Vector3 camPos, camNhin; }

    static Texture2D Chup(Camera cam, RenderTexture rt)
    {
        cam.targetTexture = rt; cam.Render();
        var tx = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        var cu = RenderTexture.active; RenderTexture.active = rt;
        tx.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tx.Apply();
        RenderTexture.active = cu;
        return tx;
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
        var pc = dir.player.GetComponent<PlayerController>();
        var toi = dir.player.GetComponent<Damageable>();
        toi.maxHealth = toi.health = 1e7f;
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        Vector3 goc = pc.transform.position;
        Physics.SyncTransforms();
        // chup BAN NGAY (lan chay 1 ban dem toi qua khong thay ro)
        var csd = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (csd != null) { csd.enabled = false; csd.ApGiay(ChuyenChieuSangDem.GiayGiuaNgay); }

        // ===== A. chon vat =====
        var nhom = new Dictionary<string, DauVetLocCuon.Kieu> { { "Cay", DauVetLocCuon.Kieu.Cay }, { "BiaMo", DauVetLocCuon.Kieu.Bia }, { "Da", DauVetLocCuon.Kieu.Da }, { "NhaMo", DauVetLocCuon.Kieu.NhaMo } };
        var ds = new List<MucTieu>();
        var tamGia = new GameObject("TAM_LocGia").AddComponent<Tornado>(); tamGia.enabled = false;     // chi de hoi CuonDuoc
        foreach (var kv in nhom)
        {
            var cha = GameObject.Find("World/" + kv.Key);
            if (cha == null) { Ghi("A. khong thay nhom World/" + kv.Key); continue; }
            GameObject tot = null; float dTot = 1e9f;
            foreach (Transform c in cha.transform)
            {
                if (!VatTheBiCuon.CuonDuoc(c.gameObject, tamGia)) continue;
                float d = Ngang(c.position, goc);
                bool canhKhac = false;
                foreach (var m in ds) if (Ngang(m.cho, c.position) < 14f) canhKhac = true;    // tach xa nhau de loc nay khong cuon vat cua muc khac
                if (!canhKhac && d < dTot) { dTot = d; tot = c.gameObject; }
            }
            if (tot != null) ds.Add(new MucTieu { ten = kv.Key, kieu = kv.Value, vat = tot, cho = tot.transform.position });
        }
        {
            LoLuaDa tot = null; float dTot = 1e9f;
            foreach (var lo in Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None))
            {
                bool canhKhac = false;
                foreach (var m in ds) if (Ngang(m.cho, lo.transform.position) < 14f) canhKhac = true;
                float d = Ngang(lo.transform.position, goc);
                if (!canhKhac && d < dTot) { dTot = d; tot = lo; }
            }
            if (tot != null) ds.Add(new MucTieu { ten = "Lo", kieu = DauVetLocCuon.Kieu.Lo, vat = tot.gameObject, cho = tot.transform.position });
        }
        Object.Destroy(tamGia.gameObject);
        foreach (var m in ds)
        {
            foreach (var r in m.vat.GetComponentsInChildren<MeshRenderer>()) { if (r.sharedMaterial != null) { m.vl = r.sharedMaterial; break; } }
            float co = m.kieu == DauVetLocCuon.Kieu.Cay ? 2.6f : m.kieu == DauVetLocCuon.Kieu.NhaMo ? 3.2f : m.kieu == DauVetLocCuon.Kieu.Da ? 1.2f : 1.6f;
            // huong may quay KHONG BI CHE (lan chay 1: anh Da bi bia mo che kin): thu 16 huong, chon huong it va cham nhat tren duong nhin
            m.camNhin = m.cho + Vector3.up * 0.3f;
            int itNhat = 99;
            for (int h = 0; h < 16; h++)
            {
                Vector3 huong = Quaternion.Euler(0f, h * 22.5f, 0f) * Vector3.forward;
                Vector3 p = m.cho + huong * 2.4f * co + Vector3.up * 2.0f * co;
                int so = 0;
                foreach (var hit in Physics.RaycastAll(p, (m.camNhin - p).normalized, Vector3.Distance(p, m.camNhin) - 0.2f, ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.collider.transform.IsChildOf(m.vat.transform) && hit.collider.GetComponent<TerrainCollider>() == null) so++;
                if (so < itNhat) { itNhat = so; m.camPos = p; }
            }
            Ghi(string.Format("A. {0}: {1} cach nhan vat {2:F1} m, vat lieu {3}", m.ten, m.vat.name, Ngang(m.cho, goc), m.vl != null ? m.vl.name : "null"));
        }
        Kiem(ds.Count == 5, "khong tim du 5 loai vat");

        var goCam = new GameObject("TAM_CamDauVet"); var cam = goCam.AddComponent<Camera>();
        cam.CopyFrom(Camera.main); cam.enabled = false; cam.fieldOfView = 45f;
        var rt = new RenderTexture(640, 480, 24);
        foreach (var m in ds)
        {
            cam.transform.position = m.camPos; cam.transform.LookAt(m.camNhin);
            yield return new WaitForEndOfFrame();
            File.WriteAllBytes("PlayTestShots/dauvet_" + m.ten + "_truoc.png", Chup(cam, rt).EncodeToPNG());
        }

        // ===== B. tha Loc xoay that tai tung vat =====
        Ghi("");
        int tao0 = DauVetLocCuon.SoLanTao, xoa0 = DauVetLocCuon.SoLanXoa;
        foreach (var m in ds) Tornado.Spawn(m.cho + Vector3.right * 0.3f, Vector3.forward, 0);
        yield return new WaitForSeconds(1.6f);
        foreach (var m in ds)
        {
            DauVetLocCuon dv = null; float dGan = 1e9f;
            foreach (var d in DauVetLocCuon.DangCo) { float dd = Ngang(d.transform.position, m.cho); if (dd < dGan) { dGan = dd; dv = d; } }
            bool coVet = dv != null && dGan < 0.05f;
            bool bay = m.vat.GetComponent<VatTheBiCuon>() != null;
            string vlManh = dv != null && dv.goc != null ? dv.goc.GetComponent<MeshRenderer>().sharedMaterial.name : "khong";
            // ho dat bam mat dat
            float lechVanh = 0f; int loLung = 0, soDinh = 0;
            if (dv != null && dv.ho != null)
            {
                var mf = dv.ho.GetComponent<MeshFilter>(); var v = mf.sharedMesh.vertices;
                var goc2 = Resources.LoadAll<Mesh>(DauVetLocCuon.DuongFbx);
                Mesh chuan = null; foreach (var mm in goc2) if (dv.ho.name == mm.name) chuan = mm;
                var vc = chuan != null ? chuan.vertices : v;
                for (int i = 0; i < v.Length; i++)
                {
                    Vector3 w = dv.ho.TransformPoint(v[i]);
                    float yd = DauVetLocCuon.MatDatY(w, w.y);
                    float mongMuon = vc[i].y * dv.ho.lossyScale.y;      // do cao rieng cua ho (tam lom, vanh do, cuc dat)
                    float lech = (w.y - yd) - mongMuon;
                    float rr = new Vector2(vc[i].x, vc[i].z).magnitude;
                    if (rr > 0.95f && rr < 1.3f && Mathf.Abs(vc[i].y + 0.02f) < 0.005f) lechVanh = Mathf.Max(lechVanh, Mathf.Abs(lech));
                    if (lech > 0.15f) loLung++;
                    soDinh++;
                }
            }
            Ghi(string.Format("B. {0}: vat dang bay {1}; dau vet {2} kieu {3} (mong {4}), lech cho cu {5:F3} m, kich thuoc do {6}; manh tren vat lieu {7} (vat: {8}); ho dat {9} dinh, vanh lech mat dat {10:F3} m, lo lung > 0,15 m {11}",
                m.ten, bay, coVet, dv != null ? dv.kieu.ToString() : "-", m.kieu, dGan, dv != null ? dv.kichThuoc.ToString("F2") : "-", vlManh, m.vl != null ? m.vl.name : "null", soDinh, lechVanh, loLung));
            Kiem(coVet && dv.kieu == m.kieu, m.ten + ": khong co dau vet dung kieu tai cho cu");
            Kiem(dv != null && dv.goc != null && m.vl != null && vlManh == m.vl.name, m.ten + ": manh tren khong dung vat lieu cua vat");
            Kiem(soDinh > 100 && lechVanh < 0.06f && loLung == 0, m.ten + ": ho dat khong bam mat dat");
        }
        int soTao = DauVetLocCuon.SoLanTao - tao0;
        Ghi(string.Format("B. tong dau vet tao trong luc nay {0} (gom ca vat quanh do bi cuon theo)", soTao));

        // ===== C. anh sau =====
        float hanLoc = Time.time + 8f;
        while (Object.FindAnyObjectByType<Tornado>() != null && Time.time < hanLoc) yield return null;
        yield return new WaitForSeconds(1.0f);
        foreach (var m in ds)
        {
            cam.transform.position = m.camPos; cam.transform.LookAt(m.camNhin);
            yield return new WaitForEndOfFrame();
            File.WriteAllBytes("PlayTestShots/dauvet_" + m.ten + "_sau.png", Chup(cam, rt).EncodeToPNG());
        }
        Ghi("C. anh PlayTestShots/dauvet_[Cay|BiaMo|Da|NhaMo|Lo]_truoc|sau.png");

        // ===== D. ngoai tam nhin =====
        Ghi("");
        TamNhin.BatTrongPhepThu = true;
        yield return null; yield return null;
        Vector3 xa = goc; float dXa = 0f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 thu = goc + Quaternion.Euler(0, i * 15f, 0) * Vector3.forward * 45f;
            thu.y = DauVetLocCuon.MatDatY(thu, goc.y);
            float dmin = 1e9f; foreach (var d in DauVetLocCuon.DangCo) dmin = Mathf.Min(dmin, Ngang(d.transform.position, thu));
            if (dmin > dXa && thu.y > -4f) { dXa = dmin; xa = thu; }
        }
        DatCho(pc.transform, xa);
        yield return null; TamNhin.Hien.QuetNgay(); yield return null;
        int an = 0, tong = 0;
        foreach (var d in DauVetLocCuon.DangCo) foreach (var r in d.GetComponentsInChildren<Renderer>()) { tong++; if (r.forceRenderingOff) an++; }
        DatCho(pc.transform, goc);
        yield return null; TamNhin.Hien.QuetNgay(); yield return null;
        int hienGan = 0, tongGan = 0;
        foreach (var d in DauVetLocCuon.DangCo)
        {
            if (Ngang(d.transform.position, goc) > TamNhin.BanKinh - TamNhin.Mem - 1f) continue;
            foreach (var r in d.GetComponentsInChildren<Renderer>()) { tongGan++; if (!r.forceRenderingOff) hienGan++; }
        }
        Ghi(string.Format("D. nhan vat o xa (dau vet gan nhat {0:F0} m): renderer dau vet bi an {1}/{2}; ve cho cu: renderer dau vet trong tam nhin hien {3}/{4}", dXa, an, tong, hienGan, tongGan));
        Kiem(dXa > TamNhin.BanKinh + 5f && tong > 0 && an == tong, "dau vet ngoai tam nhin khong bi an");
        Kiem(tongGan == 0 || hienGan == tongGan, "dau vet trong tam nhin bi an");
        TamNhin.BatTrongPhepThu = false;

        // ===== E. vat moc lai =====
        Ghi("");
        float hanE = Time.time + VatTheBiCuon.GiayHoiSinh + 6f;
        while (Time.time < hanE && (DauVetLocCuon.DangCo.Count > 0 || Object.FindAnyObjectByType<VatTheBiCuon>() != null)) yield return null;
        yield return new WaitForSeconds(DauVetLocCuon.GiayLun + 0.3f);
        int conVat = 0; foreach (var m in ds) if (m.vat.GetComponent<VatTheBiCuon>() != null) conVat++;
        soTao = DauVetLocCuon.SoLanTao - tao0;      // (lan chay 1: chot luc 1,6 s - loc con cuon them vat sau do -> 38 tao / 65 xoa, loi gia)
        Ghi(string.Format("E. sau {0:F0} s: vat con dang an {1}/{2}; dau vet con {3}, da xoa {4} (tao {5})", VatTheBiCuon.GiayHoiSinh, conVat, ds.Count,
            DauVetLocCuon.DangCo.Count, DauVetLocCuon.SoLanXoa - xoa0, soTao));
        Kiem(conVat == 0 && DauVetLocCuon.DangCo.Count == 0 && DauVetLocCuon.SoLanXoa - xoa0 == soTao, "vat moc lai ma dau vet khong mat het");
        Object.Destroy(goCam); rt.Release();
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/dauvetloccuon.txt", bao.ToString());
        TamNhin.BatTrongPhepThu = false;
        TranHienTai.Xoa();
        var rac = GameObject.Find(Ten);
        if (rac != null) Object.DestroyImmediate(rac);
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
        Debug.Log("[DauVetLoc] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
