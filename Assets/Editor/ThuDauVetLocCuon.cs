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
///   G. VA CHAM (16/10/2026, nguoi dung: "nhan vat va quai khong the di xuyen qua dau vet; neu thap thi leo len roi leo xuong"): sau khi loc tan, mot
///      CharacterController bac 0,55 m (nguoi choi) va 0,4 m (quai) di thang qua tam tung dau vet (5 m/s, trong luc): KHONG luc nao lot vao trong va cham
///      dau vet (OverlapCapsule thu nho 4 cm); di qua duoc thi phai da LEO LEN cao bang manh duoi duong di (-8 cm); bi chan thi bao cho chan cao bao nhieu.
///      Trong luc loc con chay: 0 dau vet bi loc cuon. Vat moc lai: va cham dau vet tat ngay khi bat dau lun.
///   F. TI LE THAT - TOAN BO 924 VAT (16/10/2026 lan 2, nguoi dung: "goc cay to gap doi cay, bia mo cung vay"): NGOAI Play do kich thuoc THAT cua tung vat
///      tu luoi (cay: duong kinh THAN o 0,2-0,45 m tren day; con lai: 2 x phan vi 90 ban kinh CHAN 0-0,3 m quanh tam chan) - code rieng cua phep thu, khong
///      dung bang nuong; trong Play dung dau vet cho TUNG vat (DauVetLocCuon.Tao) roi do CHINH hinh hoc manh tren cung cach (goc cay: dai 0,2-0,45 m; chan
///      bia / nen nha / nen lo: 0-0,12 m tren dat) -> ti le dau vet / that moi loai; doi chung: cach cu (theo va cham + goc loe 1,35).
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
        DoThatTeNgoaiPlay();
        Ghi("do that te ngoai Play: " + thatTe.Count + " vat");
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

    /// <summary>Kich thuoc THAT do ngoai Play (khoa ten@x,z): [0] duong kinh do (than cay / chan), [1] duong kinh theo cach CU (va cham, cay x1,35 loe).</summary>
    static readonly Dictionary<string, float[]> thatTe = new Dictionary<string, float[]>();

    /// <summary>2 x phan vi 90 khoang cach ngang toi tam (tam = trung binh) - "duong kinh" ben vung voi vai manh vang xa.</summary>
    static float DuongKinh(List<Vector3> ds)
    {
        if (ds.Count < 3) return 0f;
        Vector3 t = Vector3.zero; foreach (var p in ds) t += p; t /= ds.Count;
        var r = new List<float>(); foreach (var p in ds) r.Add(new Vector2(p.x - t.x, p.z - t.z).magnitude);
        r.Sort();
        return 2f * r[Mathf.Min(r.Count - 1, (int)(r.Count * 0.9f))];
    }

    /// <summary>Duong cheo HINH CHU NHAT BAO NHO NHAT (quay 0-90 do moi 1 do, lay goc dien tich nho nhat) - khong le thuoc mat do dinh (lan chay 2: phan vi 90
    /// lech theo mat do dinh -> 32 bia "sai"; lan 3: PCA tren hop co dinh don o 4 goc chon truc CHEO 45 do -> duong cheo ra w + d thay vi can(w2 + d2):
    /// 10 bia chu thap "x1,39" gia).</summary>
    static float CheoTruc(List<Vector3> ds)
    {
        if (ds.Count < 3) return 0f;
        float tot = 1e9f, cheo = 0f;
        for (int g = 0; g < 90; g++)
        {
            float r = g * Mathf.Deg2Rad, c = Mathf.Cos(r), sn = Mathf.Sin(r);
            float a0 = 1e9f, a1 = -1e9f, b0 = 1e9f, b1 = -1e9f;
            foreach (var p in ds) { float u = p.x * c + p.z * sn, v = -p.x * sn + p.z * c; a0 = Mathf.Min(a0, u); a1 = Mathf.Max(a1, u); b0 = Mathf.Min(b0, v); b1 = Mathf.Max(b1, v); }
            float dt = (a1 - a0) * (b1 - b0);
            if (dt < tot) { tot = dt; cheo = Mathf.Sqrt((a1 - a0) * (a1 - a0) + (b1 - b0) * (b1 - b0)); }
        }
        return cheo;
    }

    static void DoThatTeNgoaiPlay()
    {
        thatTe.Clear();
        foreach (var ten in new[] { "World/Cay", "World/BiaMo", "World/Da", "World/NhaMo", "LoLua_Act2" })
        {
            var cha = GameObject.Find(ten);
            if (cha == null) continue;
            var dsVat = new List<GameObject>();
            if (ten == "LoLua_Act2") { foreach (var lo in cha.GetComponentsInChildren<LoLuaDa>(true)) dsVat.Add(lo.gameObject); }
            else foreach (Transform c in cha.transform) dsVat.Add(c.gameObject);
            foreach (var g in dsVat)
            {
                var ws = new List<Vector3>();
                foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf.sharedMesh == null) continue;
                    Vector3[] v; try { v = mf.sharedMesh.vertices; } catch { continue; }
                    var m = mf.transform.localToWorldMatrix;
                    foreach (var q in v) ws.Add(m.MultiplyPoint3x4(q));
                }
                if (ws.Count < 4) continue;
                float minY = 1e9f, maxY = -1e9f; foreach (var w in ws) { minY = Mathf.Min(minY, w.y); maxY = Mathf.Max(maxY, w.y); }
                bool laCay = ten == "World/Cay", laDa = ten == "World/Da";
                var dai = new List<Vector3>();
                float lo0 = laCay ? minY + 0.2f : minY, hi0 = laCay ? minY + 0.45f : (laDa ? maxY + 1f : minY + Mathf.Min(0.3f, 0.3f * (maxY - minY)));
                foreach (var w in ws) if (w.y >= lo0 && w.y <= hi0) dai.Add(w);
                // cay, da: duong kinh (than tron / dong vun); bia, nha, lo: duong cheo theo hai truc chinh
                float dk = (laCay || laDa) ? DuongKinh(dai) : CheoTruc(dai);
                // cach CU: cay = capsule.radius x scale x 2 x 1,35 (goc loe); bia / da / nha / lo = canh dai cua va cham (BoxCollider size.x, size.z x scale)
                float cu = 0f;
                var col = g.GetComponent<Collider>();
                var ls = g.transform.lossyScale;
                if (col is CapsuleCollider cap) cu = cap.radius * Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.z)) * 2f * (laCay ? 1.35f : 1f);
                else if (col is BoxCollider box) cu = Mathf.Max(Mathf.Abs(box.size.x * ls.x), Mathf.Abs(box.size.z * ls.z));
                else if (col != null) cu = Mathf.Max(col.bounds.size.x, col.bounds.size.z);
                // cay: them BAN KINH THAN o 1 m (0,8-1,2 m) - do DOC LAP voi cach nuong (nuong = trung vi khoang cach toi tam; day = nua DuongKinh dai)
                float r1 = 0f;
                if (laCay)
                {
                    var dai1 = new List<Vector3>();
                    foreach (var w in ws) if (w.y >= minY + 0.8f && w.y <= minY + 1.2f) dai1.Add(w);
                    r1 = dai1.Count >= 4 ? DuongKinh(dai1) * 0.5f : 0f;
                }
                thatTe[DauVetLocCuon.Khoa(g.name, g.transform.position)] = new[] { dk, cu, r1 };
            }
        }
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

        // ===== F. ti le that - toan bo =====
        {
            int truocTao = DauVetLocCuon.SoLanTao;
            var tiLe = new Dictionary<DauVetLocCuon.Kieu, List<float>>(); var tiLeCu = new Dictionary<DauVetLocCuon.Kieu, List<float>>();
            var xauNhat = new Dictionary<DauVetLocCuon.Kieu, string>();
            int soDo = 0, coSoDo = 0;
            var cayNgang = new List<float>(); var cayRe = new List<float>(); var cayCao = new List<float>(); var cayXau = new List<string>();
            var vatF = new List<GameObject>();
            foreach (var ten in new[] { "World/Cay", "World/BiaMo", "World/Da", "World/NhaMo" }) { var cha = GameObject.Find(ten); if (cha != null) foreach (Transform c in cha.transform) vatF.Add(c.gameObject); }
            foreach (var lo in Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None)) vatF.Add(lo.gameObject);
            foreach (var g in vatF)
            {
                if (!thatTe.TryGetValue(DauVetLocCuon.Khoa(g.name, g.transform.position), out var tt) || tt[0] <= 0f) continue;
                Bounds bh = new Bounds(g.transform.position, Vector3.zero); bool co = false; Material vl = null;
                foreach (var r in g.GetComponentsInChildren<Renderer>()) { if (!co) { bh = r.bounds; co = true; } else bh.Encapsulate(r.bounds); if (vl == null && r is MeshRenderer && r.sharedMaterial != null) vl = r.sharedMaterial; }
                var dv = DauVetLocCuon.Tao(g, g.transform.position, g.transform.rotation, bh, vl);
                if (dv == null || dv.goc == null) continue;
                soDo++; if (dv.coSoDo) coSoDo++;
                // do CHINH hinh hoc manh tren
                var mfT = dv.goc.GetComponent<MeshFilter>(); var vT = mfT.sharedMesh.vertices; var mT = dv.goc.localToWorldMatrix;
                float y0 = dv.transform.position.y;
                bool laCay = dv.kieu == DauVetLocCuon.Kieu.Cay, laDa = dv.kieu == DauVetLocCuon.Kieu.Da, laLo = dv.kieu == DauVetLocCuon.Kieu.Lo;
                var dai = new List<Vector3>();
                var rGoc = new List<float>(); float caoGoc = 0f;     // cay: TOAN BO goc (lan 4) - ban kinh moi dinh tren dat + dinh cao nhat
                foreach (var q in vT)
                {
                    if (laCay)
                    {
                        var w0 = mT.MultiplyPoint3x4(q);
                        if (w0.y - y0 > 0.05f) { rGoc.Add(new Vector2(w0.x - dv.transform.position.x, w0.z - dv.transform.position.z).magnitude); caoGoc = Mathf.Max(caoGoc, w0.y - y0); }
                    }
                    if (laLo && new Vector2(q.x, q.z).magnitude > 0.62f) continue;        // lo: chi vong da chinh (bo da vang ra ngoai)
                    var w = mT.MultiplyPoint3x4(q);
                    float h = w.y - y0;
                    if (laCay ? (h >= 0.2f && h <= 0.45f) : (h >= -0.02f && h <= 0.12f)) dai.Add(w);
                }
                float dkT = (laCay || laDa) ? DuongKinh(dai) : CheoTruc(dai);
                Object.DestroyImmediate(dv.gameObject);
                if (dkT <= 0f) continue;
                float tl = dkT / tt[0], tlCu = tt[1] > 0f ? tt[1] / tt[0] : 0f;
                if (laCay && tt.Length > 2 && tt[2] > 0f && rGoc.Count > 10)
                {
                    // cay (lan 4): so TOAN BO goc voi THAN o 1 m: ngang P85, re xa nhat, dinh cao nhat
                    rGoc.Sort();
                    float ngang = rGoc[(int)(rGoc.Count * 0.85f)] / tt[2], re = rGoc[rGoc.Count - 1] / tt[2], cao = caoGoc / tt[2];
                    cayNgang.Add(ngang); cayRe.Add(re); cayCao.Add(cao);
                    if (ngang > 1.25f || re > 1.6f || cao > 1.3f || ngang < 0.7f) cayXau.Add(string.Format("{0} (than {1:F2} m: ngang x{2:F2}, re x{3:F2}, cao x{4:F2})", g.name, tt[2], ngang, re, cao));
                    tl = ngang;      // ti le cay = ngang P85 / than 1 m
                }
                if (!tiLe.ContainsKey(dv.kieu)) { tiLe[dv.kieu] = new List<float>(); tiLeCu[dv.kieu] = new List<float>(); }
                tiLe[dv.kieu].Add(tl); if (tlCu > 0f) tiLeCu[dv.kieu].Add(tlCu);
                float lech = Mathf.Abs(Mathf.Log(tl));
                if (!xauNhat.ContainsKey(dv.kieu) || lech > float.Parse(xauNhat[dv.kieu].Split('|')[0], System.Globalization.CultureInfo.InvariantCulture))
                    xauNhat[dv.kieu] = lech.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + g.name + string.Format(" ({0:F2} m that, {1:F2} m dau vet)", tt[0], dkT);
                if (soDo % 60 == 0) yield return null;
            }
            Ghi(string.Format("F. dung + do dau vet cho {0} vat ({1} co so do nuong san); tao them {2}", soDo, coSoDo, DauVetLocCuon.SoLanTao - truocTao));
            var gioiHan = new Dictionary<DauVetLocCuon.Kieu, Vector2> {
                { DauVetLocCuon.Kieu.Cay, new Vector2(0.7f, 1.25f) }, { DauVetLocCuon.Kieu.Bia, new Vector2(0.85f, 1.25f) }, { DauVetLocCuon.Kieu.Da, new Vector2(0.5f, 1.3f) },
                { DauVetLocCuon.Kieu.NhaMo, new Vector2(0.85f, 1.2f) }, { DauVetLocCuon.Kieu.Lo, new Vector2(0.8f, 1.4f) } };
            foreach (var kv in tiLe)
            {
                var a = kv.Value; a.Sort(); var c = tiLeCu[kv.Key]; c.Sort();
                int ngoai = 0; var gh = gioiHan[kv.Key]; foreach (var x in a) if (x < gh.x || x > gh.y) ngoai++;
                Ghi(string.Format("F. {0}: {1} vat, ti le dau vet / that min {2:F2} · trung vi {3:F2} · max {4:F2} (gioi han {5:F2}-{6:F2}, ngoai {7}); CACH CU trung vi {8:F2} · max {9:F2}; xau nhat {10}",
                    kv.Key, a.Count, a[0], a[a.Count / 2], a[a.Count - 1], gh.x, gh.y, ngoai, c.Count > 0 ? c[c.Count / 2] : 0f, c.Count > 0 ? c[c.Count - 1] : 0f, xauNhat[kv.Key].Split('|')[1]));
                Kiem(ngoai == 0, kv.Key + ": co dau vet sai ti le that");
            }
            Kiem(soDo > 900 && coSoDo == soDo, "khong do du 924 vat / co vat khong co so do nuong san");
            if (cayNgang.Count > 0)
            {
                cayNgang.Sort(); cayRe.Sort(); cayCao.Sort();
                Ghi(string.Format("F. CAY - toan bo goc so voi THAN o 1 m ({0} cay): ngang P85 x{1:F2}-{2:F2} (trung vi {3:F2}), re xa nhat x{4:F2}-{5:F2}, cao x{6:F2}-{7:F2}; vuot (ngang 0,7-1,25 / re <= 1,6 / cao <= 1,3): {8}",
                    cayNgang.Count, cayNgang[0], cayNgang[cayNgang.Count - 1], cayNgang[cayNgang.Count / 2], cayRe[0], cayRe[cayRe.Count - 1], cayCao[0], cayCao[cayCao.Count - 1], cayXau.Count));
                foreach (var x in cayXau) Ghi("F.    " + x);
                Kiem(cayXau.Count == 0, "goc cay to / cao hon than cay that");
            }
        }
        Ghi("");

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
            // tam dung = TAM THAT cua vat (so do nuong - than cay / chan bia / nen nha), khong phai pivot (lan chay 2: cay lech pivot 0,07 m -> loi gia)
            Vector3 tamDung = m.cho;
            if (DauVetLocCuon.Bang().TryGetValue(DauVetLocCuon.Khoa(m.vat.name, m.cho), out var sdB)) tamDung = new Vector3(sdB.tam.x, m.cho.y, sdB.tam.y);
            foreach (var d in DauVetLocCuon.DangCo) { float dd = Ngang(d.transform.position, tamDung); if (dd < dGan) { dGan = dd; dv = d; } }
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
        int biCuon = 0, coVaCham = 0;
        foreach (var d in DauVetLocCuon.DangCo) { if (d.GetComponentInParent<VatTheBiCuon>() != null || d.GetComponentInChildren<VatTheBiCuon>() != null) biCuon++; if (d.GetComponentInChildren<MeshCollider>() != null) coVaCham++; }
        Ghi(string.Format("B. trong luc loc con chay: dau vet co va cham {0}/{1}, dau vet bi loc cuon {2}", coVaCham, DauVetLocCuon.DangCo.Count, biCuon));
        Kiem(coVaCham == DauVetLocCuon.DangCo.Count && biCuon == 0, "dau vet khong co va cham / bi Loc xoay cuon di");
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

        // ===== G. va cham: di qua tung dau vet =====
        Ghi("");
        {
            var goCC = new GameObject("TAM_ThuVaCham");
            var cc = goCC.AddComponent<CharacterController>();
            cc.radius = 0.35f; cc.height = 1.8f; cc.center = new Vector3(0f, 0.9f, 0f); cc.slopeLimit = 55f; cc.skinWidth = 0.05f;
            int soChan = 0, soQua = 0;
            foreach (var m in ds)
            {
                Vector3 tamDung = m.cho;
                if (DauVetLocCuon.Bang().TryGetValue(DauVetLocCuon.Khoa(m.vat.name, m.cho), out var sdG)) tamDung = new Vector3(sdG.tam.x, m.cho.y, sdG.tam.y);
                DauVetLocCuon dv = null; float dg = 1e9f;
                foreach (var d in DauVetLocCuon.DangCo) { float dd = Ngang(d.transform.position, tamDung); if (dd < dg) { dg = dd; dv = d; } }
                if (dv == null || dg > 0.1f) { Ghi("G. " + m.ten + ": khong thay dau vet"); loi++; continue; }
                var vc = new List<Collider>(dv.GetComponentsInChildren<MeshCollider>());
                float R = 0f; foreach (var c in vc) R = Mathf.Max(R, Ngang(c.bounds.center, dv.transform.position) + Mathf.Max(c.bounds.extents.x, c.bounds.extents.z));
                // huong di: chon huong ma hai dau duong (ngoai dau vet) khong vuong vat khac
                Vector3 huong = Vector3.forward;
                for (int h = 0; h < 8; h++)
                {
                    Vector3 dthu = Quaternion.Euler(0, h * 45f, 0) * Vector3.forward;
                    Vector3 a0 = dv.transform.position - dthu * (R + 1.5f), a1 = dv.transform.position + dthu * (R + 1.5f);
                    a0.y = DauVetLocCuon.MatDatY(a0, a0.y); a1.y = DauVetLocCuon.MatDatY(a1, a1.y);
                    if (!Physics.CheckCapsule(a0 + Vector3.up * 0.4f, a0 + Vector3.up * 1.5f, 0.4f, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore)
                        && !Physics.CheckCapsule(a1 + Vector3.up * 0.4f, a1 + Vector3.up * 1.5f, 0.4f, ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore)) { huong = dthu; break; }
                }
                foreach (float buoc in new[] { 0.55f, 0.4f })
                {
                    cc.stepOffset = buoc;
                    Vector3 batDau = dv.transform.position - huong * (R + 1.5f);
                    batDau.y = DauVetLocCuon.MatDatY(batDau, batDau.y) + 0.06f;
                    cc.enabled = false; goCC.transform.position = batDau; cc.enabled = true; Physics.SyncTransforms();
                    float vy = 0f, leoMax = 0f, duoiDuong = 0f, xaNhat = -1e9f; int lot = 0;
                    for (int i = 0; i < 200; i++)          // 4 s, buoc 0,02 s (khong yield - mo phong co dinh)
                    {
                        vy = cc.isGrounded ? -1f : vy - 9.8f * 0.02f;
                        cc.Move(huong * 5f * 0.02f + Vector3.up * vy * 0.02f);
                        Vector3 p = goCC.transform.position;
                        float dat = DauVetLocCuon.MatDatY(p, p.y);
                        leoMax = Mathf.Max(leoMax, p.y - dat);
                        // manh dau vet ngay duoi duong di (tia xuong chi trung va cham dau vet)
                        foreach (var c in vc) { RaycastHit hit; if (c.Raycast(new Ray(p + Vector3.up * 3f, Vector3.down), out hit, 6f)) duoiDuong = Mathf.Max(duoiDuong, hit.point.y - dat); }
                        // lot vao trong?
                        foreach (var c in Physics.OverlapCapsule(p + Vector3.up * (0.35f + 0.04f), p + Vector3.up * (1.8f - 0.35f - 0.04f), 0.35f - 0.04f, ~0, QueryTriggerInteraction.Ignore))
                            if (vc.Contains(c)) lot++;
                        xaNhat = Mathf.Max(xaNhat, Vector3.Dot(p - dv.transform.position, huong));
                        if (xaNhat > R + 1.2f) break;
                    }
                    bool qua = xaNhat > R + 0.4f;
                    float caoChan = 0f;
                    if (!qua)
                    {
                        Vector3 p = goCC.transform.position + huong * (cc.radius + 0.12f);
                        foreach (var c in vc) { RaycastHit hit; if (c.Raycast(new Ray(p + Vector3.up * 3f, Vector3.down), out hit, 6f)) caoChan = Mathf.Max(caoChan, hit.point.y - DauVetLocCuon.MatDatY(p, p.y)); }
                        soChan++;
                    }
                    else soQua++;
                    Ghi(string.Format("G. {0} (bac {1:F2} m): {2}; manh duoi duong cao nhat {3:F2} m, leo cao nhat {4:F2} m; lot vao trong {5} lan{6}",
                        m.ten, buoc, qua ? "DI QUA" : "BI CHAN", duoiDuong, leoMax, lot, qua ? "" : string.Format("; cho chan cao {0:F2} m", caoChan)));
                    Kiem(lot == 0, m.ten + ": nhan vat lot vao trong dau vet");
                    Kiem(!qua || leoMax >= duoiDuong - 0.08f, m.ten + ": di qua ma khong leo len (xuyen qua dau vet)");
                    Kiem(qua || caoChan > buoc - 0.1f, m.ten + ": bi chan boi manh thap hon bac");
                }
            }
            Ghi(string.Format("G. tong: di qua {0}, bi chan {1}", soQua, soChan));
            // vat moc lai -> va cham tat ngay
            DauVetLocCuon dx = null; foreach (var d in DauVetLocCuon.DangCo) { dx = d; break; }
            if (dx != null)
            {
                dx.Xoa();
                int conBat = 0; foreach (var c in dx.GetComponentsInChildren<Collider>()) if (c.enabled) conBat++;
                Ghi("G. goi Xoa (vat moc lai): va cham dau vet con bat " + conBat);
                Kiem(conBat == 0, "vat moc lai ma va cham dau vet van bat");
            }
            Object.Destroy(goCC);
        }

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
