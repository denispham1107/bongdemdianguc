using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 82): LOC XOAY HINH BLENDER (nguoi dung 25/09/2026, hai anh mau: "dung lai hieu ung Loc xoay giong nhu
/// tren hinh 100%, loc khi di chuyen cuon len chi quay xoay theo truc 1 chieu") + Gio loc hat tung 80%.
/// Chot: cao 15,4 m dang theo anh, bo may giong + khoi den (thay quang sang mieng + bui xam chan), tia kieu Giut set.
///
///   A. Cau truc: mot con "LocXoayHinh" chua 4 vo + vanh (luoi TU FBX Blender), quang sang, bui chan, den; KHONG con
///      may giong / khoi den / dai xoan / hat cat / vet vut.
///   B. Kich thuoc that: cao ~15,4 m, mieng vo chinh ~6,8 m, chan ~1,3 m.
///   C. MOT CHIEU: chieu cuon THAT (goc bia dang bay quanh loc) so voi chieu quay DO DUOC cua tung lop + quy dao bui.
///      DOI CHUNG: con loc thu hai dao chieu MOT lop -> phep do phai bat duoc dung 1 lop nguoc.
///   D. CUON LEN: do nghieng dai tren FILE ANH (tu tuong quan hai hang anh), chieu u tren LUOI (do o Editor, luc Play
///      luoi FBX khong doc duoc) va dau tiling -> dai di len thi goc doi chieu nao -> voi chieu quay do o C, dai cuon
///      len hay troi xuong. DOI CHUNG: cung phep tinh voi tiling +1 (khong lat) phai ra "troi xuong".
///   E. Tia set: moi nhip co tia anh Blender (kieu Giut set), nhieu nhanh, BAM theo loc (doi loc 2 m -> dau tia doi 2 m).
///   F. Vung hut giu nguyen (5,184); quy dao ke bi cuon = 0,9 ban kinh vo chinh.
///   G. Gio loc: XacSuatHatTung 0,80 + chu Sach phep (ti le that: menu 71 muc G).
///   H. Anh: dem nhin tu duoi len (nhu anh mau), ban ngay goc choi that.
///
/// Ket qua: PlayTestShots/thanlocxoay.txt, anh thanlocxoay_*.png.
/// </summary>
public static class ThuThanLocXoay
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    // Do o Editor truoc khi vao Play (luoi FBX Read/Write TAT: trong Play vertices rong). Song qua Play nho tat Domain Reload.
    static int dauGocTheoU;          // +1: u tang thi goc atan2(z,x) tang tren luoi Vo2
    static int dauDaiTrongAnh;       // +1: di len (v tang) thi u cua dai trong anh tang
    static string ghiChuEditor = "";
    static int dauVTheoCao;          // +1: v tang theo chieu cao tren luoi Vo2
    static bool anhLienDoc, mauDinhMo;
    static string ghiChuTroi = "";

    [MenuItem("Diablo 2.5D/82. Chay thu LOC XOAY hinh Blender (mot chieu, cuon len) + Gio loc hat tung 80%", false, 171)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu Loc xoay", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false; ghiChuEditor = ""; ghiChuTroi = "";
        Ghi("[ban 3] Loc xoay hinh Blender (dai khoi troi len) + Gio loc hat tung 80%");
        DoTrongEditor();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>D (phan Editor): chieu u tren luoi + do nghieng dai tren file anh.</summary>
    static void DoTrongEditor()
    {
        dauGocTheoU = 0; dauDaiTrongAnh = 0;
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/KyNang/LocXoay/LocXoay.fbx");
        Mesh vo2 = null;
        if (fbx != null) foreach (var mf in fbx.GetComponentsInChildren<MeshFilter>(true)) if (mf.name == "Vo2") vo2 = mf.sharedMesh;
        if (vo2 != null)
        {
            var v = vo2.vertices; var uv = vo2.uv; int tang = 0, giam = 0;
            for (int i = 0; i < v.Length; i++)
                for (int j = i + 1; j < Mathf.Min(v.Length, i + 3); j++)
                {
                    if (Mathf.Abs(v[j].y - v[i].y) > 1e-3f) continue;
                    float du = uv[j].x - uv[i].x; if (Mathf.Abs(du) < 1e-4f || Mathf.Abs(du) > 0.1f) continue;
                    float dg = Mathf.DeltaAngle(Mathf.Atan2(v[i].z, v[i].x) * Mathf.Rad2Deg, Mathf.Atan2(v[j].z, v[j].x) * Mathf.Rad2Deg);
                    if (Mathf.Sign(dg) == Mathf.Sign(du)) tang++; else giam++;
                }
            dauGocTheoU = tang > giam ? 1 : -1;
            ghiChuEditor += string.Format("luoi Vo2: u tang -> goc TANG {0} cap, GIAM {1} cap; ", tang, giam);
            // v theo chieu cao + mau dinh (tan chan / mieng) - cho muc I
            float vChan = 0f, vDinh = 0f, aChan = 0f, aGiua = 0f; int nc = 0, nd = 0, ng = 0; var cl = vo2.colors;
            float ymax = vo2.bounds.max.y;
            for (int i = 0; i < v.Length; i++)
            {
                float tt = v[i].y / ymax;
                if (tt < 0.02f) { vChan += uv[i].y; nc++; if (cl.Length > i) aChan += cl[i].a; }
                if (tt > 0.98f) { vDinh += uv[i].y; nd++; }
                if (Mathf.Abs(tt - 0.5f) < 0.03f) { ng++; if (cl.Length > i) aGiua += cl[i].a; }
            }
            dauVTheoCao = nc > 0 && nd > 0 && vDinh / nd > vChan / nc ? 1 : -1;
            mauDinhMo = cl.Length == v.Length && nc > 0 && ng > 0 && aChan / nc < 0.05f && aGiua / ng > 0.95f;
            ghiChuTroi += string.Format("luoi Vo2: v o chan {0:F2}, o dinh {1:F2}; mau dinh alpha chan {2:F2}, giua {3:F2} ({4} mau). ",
                nc > 0 ? vChan / nc : -1f, nd > 0 ? vDinh / nd : -1f, nc > 0 ? aChan / nc : -1f, ng > 0 ? aGiua / ng : -1f, cl.Length);
        }
        // Anh LIEN MACH THEO v (truot len thi mep tren - duoi ghep nhau): lech hang tren / duoi so voi hai hang giua anh
        anhLienDoc = true;
        foreach (var lopAnh in new[] { "GioVo0", "GioVo1", "GioVo2", "GioVo3" })
        {
            string f = "Assets/Resources/KyNang/LocXoay/" + lopAnh + ".png";
            if (!File.Exists(f)) { anhLienDoc = false; continue; }
            var tx = new Texture2D(2, 2); tx.LoadImage(File.ReadAllBytes(f));
            int w = tx.width, h = tx.height; var p = tx.GetPixels32();
            float mep = 0f, giua = 0f;
            for (int x = 0; x < w; x++)
            {
                mep += Mathf.Abs(p[x].a - p[(h - 1) * w + x].a) / 255f;
                giua += Mathf.Abs(p[(h / 2) * w + x].a - p[(h / 2 + 1) * w + x].a) / 255f;
            }
            mep /= w; giua /= w;
            ghiChuTroi += string.Format("{0}: mep tren-duoi {1:F4} (hai hang giua {2:F4}); ", lopAnh, mep, giua);
            if (mep > 2f * giua + 0.005f) anhLienDoc = false;
            Object.DestroyImmediate(tx);
        }

        // Do nghieng dai tren file anh: tuong quan hang y voi hang y+3 lech dx (dai di len 3 px thi dich ngang bao nhieu)
        string duong = "Assets/Resources/KyNang/LocXoay/GioVo2.png";
        if (File.Exists(duong))
        {
            var t = new Texture2D(2, 2);
            t.LoadImage(File.ReadAllBytes(duong));
            int w = t.width, h = t.height; var px = t.GetPixels32();
            // PHA DAI theo tung cot: dai xoan la song theo v tan so N; tam dai o cot u lech bao nhieu -> pha. Ban dau do bang
            // tuong quan hai hang ra "len 3 px dich 3 px" (soi gio NGANG lan at dai nghieng) - sai lech 16 lan so voi 48 px
            // tinh theo cong thuc. Quet N 4..12, lay tan so manh nhat (khong can biet N truoc), roi cong don doi pha qua ca vong u.
            int nTot = 0; float bienTot = -1f;
            for (int N = 4; N <= 12; N++)
            {
                float bien = 0f;
                for (int x = 0; x < w; x += 8)
                {
                    float c = 0f, s = 0f;
                    for (int y = 0; y < h; y++) { float a = px[y * w + x].a / 255f, g = 2f * Mathf.PI * N * y / h; c += a * Mathf.Cos(g); s += a * Mathf.Sin(g); }
                    bien += Mathf.Sqrt(c * c + s * s);
                }
                if (bien > bienTot) { bienTot = bien; nTot = N; }
            }
            float phaTruoc = 0f, tongPha = 0f;
            for (int x = 0; x <= w; x += 4)
            {
                int xx = x % w; float c = 0f, s = 0f;
                for (int y = 0; y < h; y++) { float a = px[y * w + xx].a / 255f, g = 2f * Mathf.PI * nTot * y / h; c += a * Mathf.Cos(g); s += a * Mathf.Sin(g); }
                float pha = Mathf.Atan2(s, c);
                if (x > 0) { float d = pha - phaTruoc; while (d > Mathf.PI) d -= 2f * Mathf.PI; while (d < -Mathf.PI) d += 2f * Mathf.PI; tongPha += d; }
                phaTruoc = pha;
            }
            // Tam dai o v = (pha)/(2 pi N): pha TANG khi u tang = dai o cot sau nam CAO hon = di len thi u tang
            dauDaiTrongAnh = tongPha > 0.5f ? 1 : (tongPha < -0.5f ? -1 : 0);
            ghiChuEditor += string.Format("anh GioVo2 {0}x{1}: song dai manh nhat N = {2}; di het mot vong u, pha dai doi {3:+0.00;-0.00} vong (mot vong lech dung mot dai = +-1)",
                w, h, nTot, tongPha / (2f * Mathf.PI));
            Object.DestroyImmediate(t);
        }
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_ThanLoc") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThanLoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[ThanLoc] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static Damageable TaoBia(string ten, Vector3 p)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.radius = 0.4f; cap.height = 2.8f; cap.center = Vector3.up * 1.4f;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 100000f; d.health = 100000f;
        Physics.SyncTransforms();
        return d;
    }

    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            if (!pc.tuDocInput) continue;
            return pc;
        }
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static Vector3 HuongTrong(PlayerController pc)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 1.5f, h, out RaycastHit _, 25f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static Tornado ThaLoc(Vector3 p, int mask)
    {
        var t = Tornado.Spawn(p, Vector3.forward, mask);
        t.moveSpeed = 0f; t.wanderAmount = 0f; t.duration = 120f;
        return t;
    }

    static float Goc(Vector3 v) { return Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg; }
    static float Boc(float d) { while (d > 180f) d -= 360f; while (d < -180f) d += 360f; return d; }

    static readonly string[] tenLop = { "Vo0", "Vo1", "Vo2", "Vo3", "Vanh" };

    static List<Transform> Lop(Tornado t)
    {
        var ds = new List<Transform>();
        foreach (var ten in tenLop)
            foreach (var tr in t.GetComponentsInChildren<Transform>(true)) if (tr.name == ten) ds.Add(tr);
        return ds;
    }

    static Dictionary<uint, float> GocHat(ParticleSystem ps, Vector3 tam)
    {
        var d = new Dictionary<uint, float>();
        var hat = new ParticleSystem.Particle[ps.main.maxParticles];
        int n = ps.GetParticles(hat);
        bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        for (int i = 0; i < n; i++)
        {
            Vector3 p = cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
            d[hat[i].randomSeed] = Goc(p - tam);
        }
        return d;
    }

    /// <summary>Chieu quay tung lop (goc the gioi cua diem (1,0,0) cua lop) + quy dao bui chan. Tra ve (so cung chieu, tong).</summary>
    static IEnumerator DoChieu(Tornado t, float chieuCuon, string nhan, int[] kq)
    {
        var lop = Lop(t);
        ParticleSystem bui = null;
        foreach (var ps in t.GetComponentsInChildren<ParticleSystem>()) if (ps.name == "BuiChan") bui = ps;
        var g0 = new List<float>(); foreach (var tr in lop) g0.Add(Goc(tr.TransformPoint(Vector3.right) - tr.position));
        float t0 = Time.time;
        var h0 = bui != null ? GocHat(bui, t.transform.position) : null;
        for (int i = 0; i < 2; i++) yield return null;
        float dt = Mathf.Max(1e-4f, Time.time - t0);
        int cung = 0, tong = 0; var sb = new StringBuilder();
        for (int i = 0; i < lop.Count; i++)
        {
            float w = Boc(Goc(lop[i].TransformPoint(Vector3.right) - lop[i].position) - g0[i]) / dt;
            tong++; if (Mathf.Sign(w) == Mathf.Sign(chieuCuon)) cung++;
            sb.AppendFormat(" {0} {1:+0;-0}", lop[i].name, w);
        }
        if (bui != null)
        {
            var h1 = GocHat(bui, t.transform.position);
            int duong = 0, am = 0;
            foreach (var kv in h1) { float g; if (!h0.TryGetValue(kv.Key, out g)) continue; float d = Boc(kv.Value - g); if (Mathf.Abs(d) < 0.05f) continue; if (d > 0f) duong++; else am++; }
            int n = duong + am; float ti = n > 0 ? (chieuCuon > 0f ? duong : am) / (float)n : 0f;
            tong++; if (n >= 10 && ti > 0.75f) cung++;
            sb.AppendFormat(" BuiChan {0:P0} cung chieu ({1} hat)", ti, n);
        }
        Ghi(string.Format("{0}: {1}/{2} muc cung chieu cuon;{3}", nhan, cung, tong, sb));
        kq[0] = cung; kq[1] = tong;
    }

    /// <summary>
    /// Toc do dai khoi di len (m/s) cua 4 vo + vanh: doi offset v cua vat lieu (ban sao ScrollUV tao) theo Time.time.
    /// Van o v_anh = c nam o v_luoi = c - offset -> d(v_luoi)/dt = -d(offset)/dt; nhan dau v theo chieu cao (do o Editor)
    /// va chieu cao than -> m/s, duong = di LEN.
    /// </summary>
    static IEnumerator DoTroi(Tornado t, float[] kq)
    {
        var lop = Lop(t);
        var o0 = new float[lop.Count]; float t0 = Time.time;
        for (int i = 0; i < lop.Count; i++) o0[i] = lop[i].GetComponent<Renderer>().material.mainTextureOffset.y;
        yield return new WaitForSeconds(0.5f);
        float dt = Mathf.Max(1e-3f, Time.time - t0);
        for (int i = 0; i < lop.Count && i < kq.Length; i++)
        {
            float d = lop[i].GetComponent<Renderer>().material.mainTextureOffset.y - o0[i];
            d -= Mathf.Round(d);                        // ScrollUV boc offset ve [0,1)
            kq[i] = -d / dt * dauVTheoCao * VfxFactory.CaoThanLocXoay;
        }
    }

    static IEnumerator KichBan()
    {
        float han0 = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han0) yield return null;
        yield return new WaitForSeconds(1.5f);
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);

        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e6f; mauToi.health = 1e6f;
        int maskEnemy = LayerMask.GetMask("Enemy");
        Vector3 huong = HuongTrong(toi);
        Vector3 tam = toi.transform.position + huong * 14f;
        tam.y = VfxFactory.GroundY(tam);
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;

        // ================= A. CAU TRUC =================
        Ghi("");
        var loc = ThaLoc(tam, maskEnemy);
        yield return null; yield return null;
        var hinh = loc.transform.childCount > 0 ? loc.transform.GetChild(0) : null;
        var lop = Lop(loc);
        int tuFbx = 0;
        foreach (var tr in lop) { var mf = tr.GetComponent<MeshFilter>(); if (mf != null && mf.sharedMesh != null && mf.sharedMesh.name == tr.name) tuFbx++; }
        var ten = new List<string>(); foreach (var tr in loc.GetComponentsInChildren<Transform>(true)) ten.Add(tr.name);
        string[] boDi = { "Cloud", "KhoiBui", "DaiXoan0", "DaiXoan1", "DaiXoan2", "Grit", "Vut", "Dust", "Shell" };
        var conSot = new List<string>(); foreach (var b in boDi) if (ten.Contains(b)) conSot.Add(b);
        Ghi(string.Format("A. con dau '{0}' ({1} con chau), {2} lop hinh (luoi tu FBX {3}), quang sang {4}, bui chan {5}, den {6}; thu da bo con sot: {7}",
            hinh != null ? hinh.name : "-", hinh != null ? hinh.GetComponentsInChildren<Transform>(true).Length - 1 : 0, lop.Count, tuFbx,
            ten.Contains("HaoQuang"), ten.Contains("BuiChan"), ten.Contains("StormLight"), conSot.Count == 0 ? "khong" : string.Join(",", conSot)));
        Kiem(hinh != null && hinh.name == "LocXoayHinh" && loc.transform.childCount == 1, "hinh khong nam gon duoi mot con LocXoayHinh (Hoa loc xoay chi to con dau)");
        Kiem(lop.Count == 5 && tuFbx == 5, "thieu lop vo / vanh tu FBX Blender");
        Kiem(ten.Contains("HaoQuang") && ten.Contains("BuiChan") && ten.Contains("StormLight"), "thieu quang sang / bui chan / den");
        Kiem(conSot.Count == 0, "con sot may giong / khoi den / dai xoan / hat cu");

        // ================= B. KICH THUOC =================
        float cao = 0f, mieng = 0f, chan = 99f;
        foreach (var tr in lop)
        {
            var b = tr.GetComponent<MeshFilter>().sharedMesh.bounds; var s = tr.lossyScale;
            cao = Mathf.Max(cao, (b.center.y + b.extents.y) * s.y);
            if (tr.name == "Vo2") mieng = b.extents.x * s.x;
        }
        for (float h = 0.05f; h < 0.5f; h += 0.1f) chan = Mathf.Min(chan, VfxFactory.BanKinhLocXoay(h, 1f));
        Ghi(string.Format("B. cao {0:F2} m (mong 15,4), mieng vo chinh {1:F2} m (mong ~6,8), chan {2:F2} m; ti le be ngang mieng / cao {3:F2} - anh mau ~0,9",
            cao, mieng, chan, 2f * mieng / cao));
        Kiem(Mathf.Abs(cao - 15.4f) < 0.35f, "chieu cao khong phai 15,4 m");
        Kiem(mieng > 6.3f && mieng < 7.3f && chan < 1.6f, "dang loc khong theo anh (mieng ~6,8, chan ~1,3)");

        // ================= F. VUNG HUT =================
        float r7 = loc.FunnelRadiusAt(7f);
        Ghi(string.Format("F. catchRadius {0:F3} (giu 5,184); quy dao ke bi cuon o 7 m {1:F2} = 0,9 x vo chinh {2:F2}", loc.catchRadius, r7, VfxFactory.BanKinhLocXoay(7f, 1f)));
        Kiem(Mathf.Abs(loc.catchRadius - 5.184f) < 0.005f, "vung hut bi doi (phai giu nguyen)");
        Kiem(Mathf.Abs(r7 - 0.9f * VfxFactory.BanKinhLocXoay(7f, 1f)) < 0.01f, "quy dao ke bi cuon khong theo vo moi");

        // ================= C. MOT CHIEU =================
        Ghi("");
        Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;
        var bia = TaoBia("TAM_BiaCuon", tam + ngang * 3.5f);
        for (int i = 0; i < 20 && bia.GetComponent<WhirledEffect>() == null; i++) yield return null;
        float tongGoc = 0f; int soMau = 0;
        float gTruoc = Goc(bia.transform.position - loc.transform.position);
        for (int i = 0; i < 12 && bia != null && bia.GetComponent<WhirledEffect>() != null; i++)
        {
            yield return null;
            float g = Goc(bia.transform.position - loc.transform.position);
            tongGoc += Boc(g - gTruoc); gTruoc = g; soMau++;
        }
        float chieuCuon = Mathf.Sign(tongGoc);
        Ghi(string.Format("C1. bia bi cuon quay {0:+0.0;-0.0} do trong {1} khung -> chieu cuon: goc atan2(z,x) {2}", tongGoc, soMau, chieuCuon > 0 ? "TANG" : "GIAM"));
        Kiem(soMau >= 6 && Mathf.Abs(tongGoc) > 5f, "khong do duoc chieu cuon cua bia");

        if (rig != null) rig.enabled = false;
        if (cam != null) { cam.transform.position = tam - huong * 26f + Vector3.up * 8f; cam.transform.LookAt(tam + Vector3.up * 7f); }
        yield return new WaitForSeconds(1.2f);
        var kq = new int[2];
        yield return DoChieu(loc, chieuCuon, "C2. LOC XOAY", kq);
        Kiem(kq[1] == 6 && kq[0] == kq[1], "co lop / bui quay nguoc chieu cuon");

        // ================= D. CUON LEN =================
        Ghi("");
        var mat = lop.Count > 2 ? lop[2].GetComponent<Renderer>().sharedMaterial : null;
        float tiling = mat != null ? mat.mainTextureScale.x : 0f;
        Ghi("D0. (do o Editor) " + ghiChuEditor);
        // dai di len (v tang): u_anh doi dau dauDaiTrongAnh; u_luoi = u_anh / tiling; goc doi dau u_luoi * dauGocTheoU
        int dauGocKhiLen = dauDaiTrongAnh * (int)Mathf.Sign(tiling) * dauGocTheoU;
        int dauGocKhiLenDC = dauDaiTrongAnh * 1 * dauGocTheoU;
        // Quay lam goc doi dau chieuCuon: diem the gioi co dinh thay dai o do cao v voi dv/dt ~ -chieuCuon / dauGocKhiLen
        bool cuonLen = dauGocKhiLen != 0 && -chieuCuon * dauGocKhiLen > 0f;
        bool cuonLenDC = dauGocKhiLenDC != 0 && -chieuCuon * dauGocKhiLenDC > 0f;
        Ghi(string.Format("D1. tiling u {0}; dai di len thi goc {1} -> quay theo chieu cuon: dai {2}", tiling,
            dauGocKhiLen > 0 ? "TANG" : "GIAM", cuonLen ? "CUON LEN" : "TROI XUONG"));
        Ghi(string.Format("D2. DOI CHUNG tiling +1 (khong lat): dai di len thi goc {0} -> dai {1}", dauGocKhiLenDC > 0 ? "TANG" : "GIAM", cuonLenDC ? "CUON LEN" : "TROI XUONG"));
        Kiem(dauGocTheoU != 0 && dauDaiTrongAnh != 0, "khong do duoc chieu luoi / do nghieng dai trong anh");
        Kiem(cuonLen, "dai xoan troi xuong thay vi cuon len");
        Kiem(!cuonLenDC, "DOI CHUNG: khong lat u ma van ra cuon len - phep tinh khong phan biet duoc");

        // ================= I. DAI KHOI TROI LEN THAT =================
        Ghi("");
        Ghi("I0. (do o Editor) " + ghiChuTroi);
        Kiem(anhLienDoc, "anh gio chua lien mach theo chieu doc - truot len se lo duong noi");
        Kiem(mauDinhMo && dauVTheoCao > 0, "luoi thieu mau dinh (tan chan / mieng) hoac v khong tang theo chieu cao");
        var kqTroi = new float[5];
        yield return DoTroi(loc, kqTroi);
        int soLen = 0; var sbT = new StringBuilder();
        for (int i = 0; i < 4; i++) { if (kqTroi[i] > 0.5f) soLen++; sbT.AppendFormat(" {0} {1:+0.00;-0.00} m/s", tenLop[i], kqTroi[i]); }
        Ghi(string.Format("I1. toc do dai khoi di len (tu doi offset anh x chieu cao than 15 m):{0}; vanh {1:+0.00;-0.00} m/s", sbT, kqTroi[4]));
        Kiem(soLen == 4, "co lop vo dai khoi khong troi len");

        // ================= C3. DOI CHUNG chieu: dao MOT lop =================
        var dc = ThaLoc(tam + huong * 40f, 0);
        yield return null;
        var lopDc = Lop(dc);
        if (lopDc.Count > 1) { var sp = lopDc[1].GetComponent<Spin>(); sp.degreesPerSecond = -sp.degreesPerSecond; }
        // DOI CHUNG muc I: dao chieu truot cua Vo2 -> phep do phai bao Vo2 troi XUONG
        if (lopDc.Count > 2) { var sc = lopDc[2].GetComponent<ScrollUV>(); if (sc != null) sc.speed = -sc.speed; }
        if (cam != null) { cam.transform.position = tam + huong * 40f - huong * 26f + Vector3.up * 8f; cam.transform.LookAt(tam + huong * 40f + Vector3.up * 7f); }
        yield return new WaitForSeconds(1.2f);
        var kqDc = new int[2];
        yield return DoChieu(dc, chieuCuon, "C3. DOI CHUNG (dao chieu Vo1)", kqDc);
        Kiem(kqDc[1] - kqDc[0] == 1, "DOI CHUNG: phep do khong bat duoc dung mot lop quay nguoc");
        var kqTroiDc = new float[5];
        yield return DoTroi(dc, kqTroiDc);
        Ghi(string.Format("I2. DOI CHUNG dao truot Vo2: Vo2 {0:+0.00;-0.00} m/s (phai AM = troi xuong), Vo1 {1:+0.00;-0.00}", kqTroiDc[2], kqTroiDc[1]));
        Kiem(kqTroiDc[2] < -0.5f && kqTroiDc[1] > 0.5f, "DOI CHUNG: phep do khong phan biet troi len / troi xuong");
        Object.Destroy(dc.gameObject);

        // ================= E. TIA SET =================
        Ghi("");
        if (cam != null) { cam.transform.position = tam - huong * 26f + Vector3.up * 8f; cam.transform.LookAt(tam + Vector3.up * 7f); }
        var truocDo = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
        float het = Time.time + 1.2f; var moi = new List<LightningArc>();
        while (Time.time < het)
        {
            foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                if (!truocDo.Contains(a)) { truocDo.Add(a); moi.Add(a); }
            yield return null;
        }
        int anhBlender = 0, nhieuNhanh = 0, soCon = 0; float nhanhTB = 0f;
        foreach (var a in moi) { if (a == null) continue; soCon++; if (a.anhBlender) anhBlender++; if (a.branches >= 3) nhieuNhanh++; nhanhTB += a.branches; }
        Ghi(string.Format("E1. 1,2 giay: {0} tia moi ({1} con song luc dem), kieu anh Blender {2}, >= 3 nhanh {3}, nhanh TB {4:F1}",
            moi.Count, soCon, anhBlender, nhieuNhanh, soCon > 0 ? nhanhTB / soCon : 0f));
        Kiem(moi.Count >= 4 && anhBlender == soCon && nhieuNhanh * 2 >= soCon, "tia set tren than loc khong phai kieu Giut set nhieu nhanh");
        // Bam theo loc: tha mot nhip moi, doi loc 2 m, dau tia phai doi theo
        truocDo = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
        VfxFactory.TornadoBolt(loc.transform, 1f);
        LightningArc bam = null;
        foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude)) if (!truocDo.Contains(a)) bam = a;
        if (bam != null)
        {
            // Giu tia song du lau de do: tia that song 0,28 - 0,42 s, Editor cham vai khung la tia da bien mat (lan chay
            // 25/09/2026 bao -1,00 m = tia null, khong phai tia khong bam)
            bam.lifetime = 5f;
            yield return null;
            Vector3 s0 = bam != null ? bam.start : Vector3.zero;
            loc.transform.position += ngang * 2f;
            yield return null; yield return null;
            float dich = bam != null ? Vector3.Distance(bam.start, s0) : -1f;
            Ghi(string.Format("E2. doi loc 2,00 m -> dau tia doi {0:F2} m", dich));
            Kiem(Mathf.Abs(dich - 2f) < 0.05f, "tia set khong bam theo loc");
            loc.transform.position -= ngang * 2f;
        }
        else { Ghi("[LOI] khong bat duoc tia de do bam"); loi++; }

        // ================= H. ANH =================
        if (cam != null)
        {
            cam.transform.position = tam - huong * 21f + Vector3.up * 2.5f;
            cam.transform.LookAt(tam + Vector3.up * 8.5f);
            yield return new WaitForSeconds(0.5f);
            // Chup LUC co tia: phat mot nhip tia ngay truoc (tia song 0,28 - 0,42 s) - chup theo gio thi hay lech nhip
            VfxFactory.TornadoBolt(loc.transform, 1f); VfxFactory.TornadoBolt(loc.transform, 1f);
            yield return null; yield return null;
            yield return Chup("thanlocxoay_0_dem_can");
        }
        if (bia != null) Object.Destroy(bia.gameObject);
        Object.Destroy(loc.gameObject);
        var ngay = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (ngay != null) { ngay.enabled = false; ngay.ApGiay(ChuyenChieuSangDem.GiayGiuaNgay); }
        if (rig != null) rig.enabled = true;
        yield return new WaitForSeconds(0.6f);
        Vector3 truoc = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : huong;
        Vector3 choAnh = toi.transform.position + truoc * 7f; choAnh.y = VfxFactory.GroundY(choAnh);
        var locAnh = Tornado.Spawn(choAnh, truoc, 0);
        yield return new WaitForSeconds(1.4f);
        yield return Chup("thanlocxoay_1_ngay_goc_choi");
        if (locAnh != null) Object.Destroy(locAnh.gameObject);

        // ================= G. GIO LOC HAT TUNG =================
        Ghi("");
        Ghi(string.Format("G. Gio loc: XacSuatHatTung {0:F2}; Sach phep tom tat \"{1}\"; mo ta co \"80%\": {2}",
            GioLoc.XacSuatHatTung, SachPhep.TomTat(CapDo.KyGioLoc), SachPhep.MoTa(CapDo.KyGioLoc).Contains("80%")));
        Kiem(Mathf.Approximately(GioLoc.XacSuatHatTung, 0.80f), "Gio loc khong hat tung 80%");
        Kiem(SachPhep.TomTat(CapDo.KyGioLoc).Contains("80%") && SachPhep.MoTa(CapDo.KyGioLoc).Contains("80%") && !SachPhep.MoTa(CapDo.KyGioLoc).Contains("55%"),
            "chu Sach phep chua doi sang 80%");

        Ghi("");
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[ThanLoc] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        Ghi("so loi ghi nhan = " + loi);
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/thanlocxoay.txt", bao.ToString());
        var rac = GameObject.Find("TAM_ThanLoc");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
