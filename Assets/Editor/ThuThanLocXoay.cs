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
    static float rChanLuoi, rGiuaLuoi, rMiengLuoi;   // vo chinh Vo2 do tu DINH LUOI FBX (Editor - trong Play luoi khong doc duoc)

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
            // 29/09/2026: ban kinh vo chinh doc thang tu dinh luoi (doc lap voi cong thuc BanKinhLocXoay trong code)
            rChanLuoi = 0f; rGiuaLuoi = 0f; rMiengLuoi = 0f;
            for (int i = 0; i < v.Length; i++)
            {
                float r = new Vector2(v[i].x, v[i].z).magnitude;
                if (v[i].y < 0.05f) rChanLuoi = Mathf.Max(rChanLuoi, r);
                if (Mathf.Abs(v[i].y - 7.5f) < 0.2f) rGiuaLuoi = Mathf.Max(rGiuaLuoi, r);
                if (v[i].y > 14.95f) rMiengLuoi = Mathf.Max(rMiengLuoi, r);
            }
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

    /// <summary>Do vi tri THAT tung hat (so voi goc loc): chieu cao h90 / lon nhat, va do lech ban kinh so voi than (trung vi
    /// |r - R(h)| / R(h), chi hat o 10-95% chieu cao). Hat Local doi qua transform he hat, hat World lay thang.</summary>
    static void DoBuiTheoCao(ParticleSystem ps, Transform goc, float cao, System.Func<float, float> R, out float h90, out float hMax, out float lech, out int n)
    {
        h90 = 0f; hMax = 0f; lech = 99f; n = 0;
        if (ps == null) return;
        var hat = new ParticleSystem.Particle[ps.particleCount];
        n = ps.GetParticles(hat);
        var cac = new List<float>(); var ls = new List<float>();
        bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        for (int i = 0; i < n; i++)
        {
            Vector3 w = cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
            Vector3 l = goc.InverseTransformPoint(w);
            cac.Add(l.y);
            if (l.y > 0.1f * cao && l.y < 0.95f * cao)
            {
                float rr = new Vector2(l.x, l.z).magnitude, rt = R(l.y);
                ls.Add(Mathf.Abs(rr - rt) / Mathf.Max(0.01f, rt));
            }
        }
        if (cac.Count == 0) return;
        cac.Sort(); ls.Sort();
        h90 = cac[Mathf.Min(cac.Count - 1, Mathf.FloorToInt(cac.Count * 0.9f))];
        hMax = cac[cac.Count - 1];
        if (ls.Count > 0) lech = ls[ls.Count / 2];
    }

    /// <summary>Ti le hat dang song cao hon nguongY (so voi goc loc).</summary>
    static float TiLeTren(ParticleSystem ps, Transform goc, float nguongY)
    {
        if (ps == null) return 0f;
        var hat = new ParticleSystem.Particle[ps.particleCount];
        int n = ps.GetParticles(hat), tren = 0;
        bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        for (int i = 0; i < n; i++)
        {
            Vector3 w = cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
            if (goc.InverseTransformPoint(w).y > nguongY) tren++;
        }
        return n > 0 ? (float)tren / n : 0f;
    }

    /// <summary>Dem hat dang song co do cao (so voi goc loc) tu 0 den caoToi.</summary>
    static int DemDuoi(ParticleSystem ps, Transform goc, float caoToi)
    {
        var hat = new ParticleSystem.Particle[ps.particleCount];
        int n = ps.GetParticles(hat), dem = 0;
        bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        for (int i = 0; i < n; i++)
        {
            Vector3 w = cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
            float y = goc.InverseTransformPoint(w).y;
            if (y >= 0f && y <= caoToi) dem++;
        }
        return dem;
    }

    static float Goc(Vector3 v) { return Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg; }

    /// <summary>Vat la loe set cham dat: prefab Vfx_SetChamDat (GameAssets.Make dat ten theo prefab) hoac BuildLightningImpact.</summary>
    static bool LaLoe(GameObject g)
    {
        return g.name == "Vfx_SetChamDat" || g.name == "LightningImpact";
    }
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
        // 29/09/2026 (nguoi dung: "than duoi nhu cay kem oc que"): chan x2 so voi ban cu 1,3 m, than to dan deu, mieng 6,8 giu nguyen.
        // Do tu DINH LUOI FBX (Editor), so voi SO CU VIET TAY (1,3 / 2,774 o 7,5 m / 6,8) va voi cong thuc trong code.
        Ghi(string.Format("B2. vo chinh tu dinh luoi: chan {0:F3} m (cu 1,300 -> x{1:F2}), o 7,5 m {2:F3} (cu 2,774 -> x{3:F2}), mieng {4:F3} (cu 6,800); cong thuc code: chan {5:F3}, 7,5 m {6:F3}, mieng {7:F3}",
            rChanLuoi, rChanLuoi / 1.3f, rGiuaLuoi, rGiuaLuoi / 2.774f, rMiengLuoi, VfxFactory.BanKinhLocXoay(0f, 1f), VfxFactory.BanKinhLocXoay(7.5f, 1f), VfxFactory.BanKinhLocXoay(15f, 1f)));
        Kiem(Mathf.Abs(rChanLuoi / 1.3f - 2f) < 0.03f, "chan luoi khong to x2 so ban cu (1,3 m)");
        Kiem(rGiuaLuoi > 2.774f * 1.2f && rGiuaLuoi < rMiengLuoi, "than giua khong to dan deu (7,5 m phai to hon ban cu, nho hon mieng)");
        Kiem(Mathf.Abs(rMiengLuoi - 6.8f) < 0.02f, "mieng loc bi doi (phai giu 6,8 m)");
        Kiem(Mathf.Abs(VfxFactory.BanKinhLocXoay(0f, 1f) - rChanLuoi) < 0.02f && Mathf.Abs(VfxFactory.BanKinhLocXoay(7.5f, 1f) - rGiuaLuoi) < 0.03f,
             "cong thuc BanKinhLocXoay trong code lech luoi FBX (qui dao ke bi cuon / tia set se lech than)");
        // Vong bui chan: lay tu con loc THAT vua tha (prefab Skill_LocXoay de len code) - rong theo chan moi; DOI CHUNG Gio loc giu 1,0
        ParticleSystem buiChan = null;
        foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "BuiChan") buiChan = ps;
        var gioLoc = VfxFactory.BuildGioLocCu();   // doi chung: hinh Gio loc CU (01/10/2026 Gio loc moi la Loc xoay thu nho)
        ParticleSystem buiGl = null;
        foreach (var ps in gioLoc.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "BuiCuon") buiGl = ps;
        float rBui = buiChan != null ? buiChan.shape.radius : -1f, rBuiGl = buiGl != null ? buiGl.shape.radius : -1f;
        Object.Destroy(gioLoc);
        Ghi(string.Format("B3. vong bui chan Loc xoay that {0:F2} m (cu 2,00 -> x{1:F2}); doi chung vong bui Gio loc {2:F2} m (giu 1,00)", rBui, rBui / 2f, rBuiGl));
        Kiem(Mathf.Abs(rBui - 4f) < 0.02f, "vong bui chan Loc xoay khong rong theo chan moi (2,0 -> 4,0) - prefab con so cu?");
        // 29/09/2026: vong phun phai NAM PHANG tren dat (Circle mac dinh dung trong mat XY - nua so hat tung sinh duoi dat)
        float xoayX = buiChan != null ? buiChan.shape.rotation.x : 0f;
        Ghi(string.Format("B3b. vong phun bui chan xoay {0:F0} do quanh X (mong -90: nam phang tren dat)", xoayX));
        Kiem(Mathf.Abs(xoayX + 90f) < 0.5f, "vong phun bui chan Loc xoay con dung (hat sinh duoi dat) - prefab con so cu?");
        // B3c (29/09/2026, nguoi dung chon toi di 20%): mau tren con loc THAT tu prefab, so voi SO VIET TAY (goc x 0,8)
        {
            Color vo0 = Color.clear, vanh = Color.clear;
            foreach (var mr in loc.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (mr.name == "Vo0") vo0 = mr.sharedMaterial.GetColor("_TintColor");
                if (mr.name == "Vanh") vanh = mr.sharedMaterial.GetColor("_TintColor");
            }
            Color bui0 = buiChan != null ? buiChan.main.startColor.colorMin : Color.clear;
            Ghi(string.Format("B3c. mau vo Vo0 ({0:F3} {1:F3} {2:F3}) mong (0,688 0,704 0,736); Vanh {3:F3} mong 0,800; bui chan toi {4:F3} mong 0,640",
                vo0.r, vo0.g, vo0.b, vanh.r, bui0.r));
            Kiem(Mathf.Abs(vo0.r - 0.688f) < 0.01f && Mathf.Abs(vo0.b - 0.736f) < 0.01f && Mathf.Abs(vanh.r - 0.8f) < 0.01f, "vo Loc xoay chua toi 20% (vat lieu prefab?)");
            Kiem(Mathf.Abs(bui0.r - 0.64f) < 0.01f, "bui chan Loc xoay chua toi 20%");
        }
        Kiem(Mathf.Abs(rBuiGl - 1f) < 0.02f, "vong bui Gio loc bi doi theo (chi Loc xoay doi)");

        // B4 (29/09/2026, nguoi dung: bui cuon len "day dac hon nua len tan dinh"): lop BuiCuonLen tren con loc THAT tu prefab (gan luc
        // chay trong Tornado.Start). Do vi tri tung hat; DOI CHUNG la bui chan BuiChan (cu) - phai van thap.
        ParticleSystem psLen = null, psChanDo = null;
        foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>(true)) { if (ps.name == "BuiCuonLen") psLen = ps; if (ps.name == "BuiChan") psChanDo = ps; }
        foreach (var ps in new[] { psLen, psChanDo }) if (ps != null) { var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate; }
        yield return new WaitForSeconds(4.5f);
        float caoLx = VfxFactory.CaoThanLocXoay * loc.scale;
        System.Func<float, float> vo = h => VfxFactory.BanKinhLocXoay(h, loc.scale);
        float h90L, hMaxL, lechL, h90C, hMaxC, lechC; int nL, nC;
        DoBuiTheoCao(psLen, loc.transform, caoLx, vo, out h90L, out hMaxL, out lechL, out nL);
        DoBuiTheoCao(psChanDo, loc.transform, caoLx, vo, out h90C, out hMaxC, out lechC, out nC);
        float tocLen = psLen != null ? psLen.emission.rateOverTime.constant : 0f, tocChan = psChanDo != null ? psChanDo.emission.rateOverTime.constant : 0f;
        Ghi(string.Format("B4. bui cuon len (than {0:F1} m): {1} hat, cao 90% {2:F2} m, cao nhat {3:F2} m, lech ban kinh so voi vo chinh (trung vi) {4:P0}; DOI CHUNG bui chan: {5} hat, cao 90% {6:F2} m, cao nhat {7:F2} m; so hat/giay {8:F0} + {9:F0} = {10:F0} (cu 40)",
            caoLx, nL, h90L, hMaxL, lechL, nC, h90C, hMaxC, tocLen, tocChan, tocLen + tocChan));
        Kiem(psLen != null && nL > 100, "khong co lop bui cuon len tren con loc that (prefab?)");
        Kiem(hMaxL > 0.9f * caoLx && h90L > 0.7f * caoLx, "bui khong cuon len toi dinh loc");
        Kiem(lechL < 0.35f, "bui cuon len khong om theo than loc");
        // DOI CHUNG: lop moi phai toi PHAN DINH ma bui chan cu khong toi - so TI LE HAT O 20% TREN CUNG THAN (cung cach menu 71;
        // cao-90% / cao-90% thi chap chon voi Gio loc vi bui chan tu no da len 65-70% than)
        float trenL = TiLeTren(psLen, loc.transform, 0.8f * caoLx), trenC = TiLeTren(psChanDo, loc.transform, 0.8f * caoLx);
        Ghi(string.Format("B4. ti le hat tren 80% than: lop cuon len {0:P0}, bui chan {1:P0}", trenL, trenC));
        Kiem(trenL > 0.12f && trenL > 2f * trenC, "DOI CHUNG: bui chan cu toi dinh ngang lop moi - phep do khong phan biet duoc");
        // B5 (29/09/2026, nguoi dung khoanh THAN DUOI tren anh: "khoi cuon len va bui day dac hon nua"; chon them lop than duoi + bui
        // chan x2): lop BuiThanDuoi chi toi nua than, bui chan 80. DOI CHUNG CUNG LUOT: con loc thu hai dat ve CAU HINH CU (tat
        // BuiThanDuoi, bui chan 40 / 120) - dem hat that o NUA THAN DUOI ca hai con.
        ParticleSystem psDuoi = null;
        foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "BuiThanDuoi") psDuoi = ps;
        float tocDuoi = psDuoi != null ? psDuoi.emission.rateOverTime.constant : 0f;
        float h90D, hMaxD, lechD; int nD;
        DoBuiTheoCao(psDuoi, loc.transform, 0.5f * caoLx, vo, out h90D, out hMaxD, out lechD, out nD);
        Ghi(string.Format("B5. lop than duoi: {0} hat, cao 90% {1:F2} m, cao nhat {2:F2} m (nua than {3:F1}), lech ban kinh {4:P0}; so hat/giay chan {5:F0} + cuon len {6:F0} + than duoi {7:F0} = {8:F0}",
            nD, h90D, hMaxD, 0.5f * caoLx, lechD, tocChan, tocLen, tocDuoi, tocChan + tocLen + tocDuoi));
        Kiem(psDuoi != null && nD > 100, "khong co lop khoi than duoi tren con loc that");
        Kiem(h90D > 0.7f * 0.5f * caoLx && hMaxD < 0.75f * caoLx, "lop than duoi khong nam o nua than duoi");
        Kiem(lechD < 0.35f, "lop than duoi khong om theo than");
        Kiem(Mathf.Abs(tocChan - 80f) < 0.5f && psChanDo.main.maxParticles == 240, "bui chan Loc xoay khong gap doi (80 / 240)");
        // 01/10/2026 nguoi dung: "bui khoi nhieu hon va bay cuon len tan dinh" - chon lop len dinh x2 (80 -> 160): tong 320
        Kiem(Mathf.Abs(tocLen - 160f) < 0.5f && Mathf.Abs(tocChan + tocLen + tocDuoi - 320f) < 0.5f, "lop bui len dinh khong phai 160 / tong bui Loc xoay khong phai 320 hat/giay");
        {
            var loc2 = ThaLoc(tam + huong * 40f, maskEnemy);
            yield return null; yield return null;
            ParticleSystem d2 = null, c2 = null;
            foreach (var ps in loc2.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps.name == "BuiThanDuoi") d2 = ps;
                if (ps.name == "BuiChan") c2 = ps;
                var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            }
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>(true)) { var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate; }
            if (d2 != null) { var e = d2.emission; e.enabled = false; d2.Clear(); }
            if (c2 != null) { var m = c2.main; m.maxParticles = 120; var e = c2.emission; e.rateOverTime = 40f; }
            // B7 (01/10/2026): DOI CHUNG CUNG LUOT thu hai - cau hinh HOM QUA (lop len dinh 80 hat/giay, tran theo cong thuc cu)
            var loc4 = ThaLoc(tam + huong * 40f - Vector3.Cross(Vector3.up, huong) * 18f, maskEnemy);
            yield return null; yield return null;
            foreach (var ps in loc4.GetComponentsInChildren<ParticleSystem>(true))
            {
                var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                if (ps.name == "BuiCuonLen") { m.maxParticles = Mathf.CeilToInt(80f * 3.6f * 1.1f); var e = ps.emission; e.rateOverTime = 80f; }
            }
            yield return new WaitForSeconds(4.5f);
            {
                int trenMoi = 0, trenCu = 0, dinhMoi = 0, dinhCu = 0;
                foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>(true))
                    if (ps.name.StartsWith("Bui")) { trenMoi += DemDuoi(ps, loc.transform, 99f) - DemDuoi(ps, loc.transform, 0.5f * caoLx); dinhMoi += DemDuoi(ps, loc.transform, 99f) - DemDuoi(ps, loc.transform, 0.8f * caoLx); }
                foreach (var ps in loc4.GetComponentsInChildren<ParticleSystem>(true))
                    if (ps.name.StartsWith("Bui")) { trenCu += DemDuoi(ps, loc4.transform, 99f) - DemDuoi(ps, loc4.transform, 0.5f * caoLx); dinhCu += DemDuoi(ps, loc4.transform, 99f) - DemDuoi(ps, loc4.transform, 0.8f * caoLx); }
                Ghi(string.Format("B7. hat bui dang song o NUA THAN TREN (> {0:F1} m): moi {1} / cau hinh hom qua cung luot {2} = x{3:F2}; o 20% tren cung (> {4:F1} m): {5} / {6} = x{7:F2}",
                    0.5f * caoLx, trenMoi, trenCu, (float)trenMoi / Mathf.Max(1, trenCu), 0.8f * caoLx, dinhMoi, dinhCu, (float)dinhMoi / Mathf.Max(1, dinhCu)));
                Kiem(trenCu > 50 && dinhCu > 15, "doi chung: cau hinh hom qua khong co bui o than tren - phep dem vo nghia");
                Kiem((float)trenMoi / Mathf.Max(1, trenCu) > 1.7f && (float)dinhMoi / Mathf.Max(1, dinhCu) > 1.7f, "bui o than tren / tan dinh khong day hon ro (it nhat x1,7)");
            }
            Object.Destroy(loc4.gameObject);
            int demMoi = 0, demCu = 0;
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>(true))
                if (ps.name == "BuiChan" || ps.name == "BuiCuonLen" || ps.name == "BuiThanDuoi") demMoi += DemDuoi(ps, loc.transform, 0.5f * caoLx);
            foreach (var ps in loc2.GetComponentsInChildren<ParticleSystem>(true))
                if (ps.name == "BuiChan" || ps.name == "BuiCuonLen" || ps.name == "BuiThanDuoi") demCu += DemDuoi(ps, loc2.transform, 0.5f * caoLx);
            Ghi(string.Format("B5. hat bui dang song o NUA THAN DUOI (0 - {0:F1} m): moi {1} / cau hinh cu cung luot {2} = x{3:F2}", 0.5f * caoLx, demMoi, demCu, (float)demMoi / Mathf.Max(1, demCu)));
            Kiem(demCu > 50, "doi chung: con loc cau hinh cu khong co bui - phep dem vo nghia");
            Kiem((float)demMoi / Mathf.Max(1, demCu) > 1.8f, "nua than duoi khong day dac hon ro (it nhat x1,8)");
            Object.Destroy(loc2.gameObject);
        }

        // B6 (29/09/2026): dau vet tren mat dat (dai dat cay + vet chay xem, them sang cung ngay) - nguoi dung "xoa cac vet di chuyen
        // cua loc o ca 2 skill", chon xoa ca vet chay. Mot con loc THAT chay 3 s co set danh -> KHONG duoc sinh vat the dau vet nao.
        {
            Vector3 phaiB6 = Vector3.Cross(Vector3.up, huong).normalized;
            Vector3 dauB6 = tam - huong * 7f - phaiB6 * 6f; dauB6.y = VfxFactory.GroundY(dauB6);
            var loc3 = Tornado.Spawn(dauB6, phaiB6, maskEnemy);
            loc3.moveSpeed = 3.4f; loc3.wanderAmount = 0f; loc3.duration = 60f;
            yield return null;
            Vector3 xuatPhat = loc3.transform.position;
            yield return new WaitForSeconds(3f);
            Vector3 dDi = loc3.transform.position - xuatPhat; dDi.y = 0f;
            int soVet = 0;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
                if (go.name == "VetLoc" || go.name == "ChayXem") soVet++;
            Ghi(string.Format("B6. loc that chay {0:F2} m trong 3 s: vat the dau vet tren mat dat {1} (mong 0 - da go bo)", dDi.magnitude, soVet));
            Kiem(dDi.magnitude > 8f, "doi chung: loc khong chay - phep kiem vo nghia");
            Kiem(soVet == 0, "loc van de lai dau vet tren mat dat (da go bo)");
            Object.Destroy(loc3.gameObject);
            yield return null;
        }

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
        // E1 (01/10/2026 nguoi dung: tia trong Loc xoay "giong tia set trong Sam set", chon tu mieng loc xuong dat, khong vet chay):
        // DOI CHUNG la mot cu SAM SET THAT (LightningStrike.Spawn, sat thuong 0, o xa) - so tung thong so tia voi no. Moi tia cham dat
        // phai co mot LOE cham dat (vat co con "BoltLight" nhu loe cua Sam set that) dinh vao loc.
        // Thong so tia Sam set CHEP RA ngay khi bat duoc (tia song 0,30 s - doc lai sau thi da bi xoa)
        bool coSS = false; bool ssAnh = true; int ssDoan = 0, ssNhanh = 0, loeSS = 0;
        float ssLoi = 0f, ssQuang = 0f, ssSong = 0f, ssGiat = 0f, ssNhanhDai = 0f; Color ssMauLoi = Color.clear, ssMauQuang = Color.clear;
        {
            var arcTruocSS = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            var gocTruocSS = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
            Vector3 choSS = tam + huong * 60f; choSS.y = VfxFactory.GroundY(choSS);
            var ss = LightningStrike.Spawn(choSS, 20f, 0f);
            ss.damage = 0f;
            for (int i = 0; i < 60 && !coSS; i++)
            {
                yield return null;
                foreach (var x in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (arcTruocSS.Contains(x) || (x.end - choSS).magnitude > 0.5f) continue;    // chi tia cua cu Sam set (tia loc khong cham choSS)
                    coSS = true; ssAnh = x.anhBlender; ssDoan = x.segments; ssNhanh = x.branches; ssLoi = x.coreWidth; ssQuang = x.glowWidth;
                    ssSong = x.lifetime; ssGiat = x.jitter; ssNhanhDai = x.branchLength; ssMauLoi = x.coreColor; ssMauQuang = x.glowColor;
                }
            }
            foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (!gocTruocSS.Contains(g) && LaLoe(g) && (g.transform.position - choSS).magnitude < 1f) loeSS++;
        }
        var truocDo = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
        var loeTruoc = new HashSet<DiTheo>(Object.FindObjectsByType<DiTheo>(FindObjectsInactive.Exclude));
        int conTruoc = loc.transform.childCount;
        float het = Time.time + 1.2f; var moi = new List<LightningArc>();
        int kieuSS = 0, soChamDat = 0, dauMieng = 0;
        yield return new WaitForEndOfFrame();
        while (Time.time < het)
        {
            foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
            {
                if (truocDo.Contains(a)) continue;
                truocDo.Add(a);
                // 05/10/2026: tia set con BO TREN DAT quanh cho cham (mau A) khong phai tia tu mieng loc
                if (a.name == TiaBoDat.TenTia) continue;
                moi.Add(a);
                if (coSS && !a.anhBlender && a.segments == ssDoan && a.branches >= 2 && a.branches <= 3
                    && Mathf.Abs(a.coreWidth - ssLoi * loc.scale) < 1e-4f && Mathf.Abs(a.glowWidth - ssQuang * loc.scale) < 1e-4f
                    && a.coreColor == ssMauLoi && a.glowColor == ssMauQuang && Mathf.Abs(a.lifetime - ssSong) < 1e-4f
                    && Mathf.Abs(a.jitter - ssGiat) < 1e-4f && Mathf.Abs(a.branchLength - ssNhanhDai) < 1e-4f) kieuSS++;
                // Mat dat doc THANG tu terrain (doc lap voi tia do dat cua code)
                var ter = Terrain.activeTerrain;
                float datY = ter != null ? ter.SampleHeight(a.end) + ter.transform.position.y : VfxFactory.GroundY(a.end);
                if (Mathf.Abs(a.end.y - datY) < 0.3f) soChamDat++;
                if (a.start.y - loc.transform.position.y > 12.2f) dauMieng++;
            }
            yield return new WaitForEndOfFrame();
        }
        int loeMoi = 0;
        foreach (var d in Object.FindObjectsByType<DiTheo>(FindObjectsInactive.Exclude))
            if (!loeTruoc.Contains(d) && d.theo == loc.transform && LaLoe(d.gameObject)) loeMoi++;
        Ghi(string.Format("E1. DOI CHUNG Sam set that: tia {0} (rong loi {1:F3} / quang {2:F3}, {3} doan, {4} nhanh, song {5:F2} s, anh Blender {6}), {7} loe cham dat",
            coSS ? "co" : "KHONG", ssLoi, ssQuang, ssDoan, ssNhanh, ssSong, ssAnh, loeSS));
        Ghi(string.Format("E1. Loc xoay 1,2 giay: {0} tia moi; dung kieu Sam set {1}; dau o mieng (> 12,2 m) {2}; cham dat {3}; loe cham dat moi di theo loc {4}; so con cua loc {5} -> {6}",
            moi.Count, kieuSS, dauMieng, soChamDat, loeMoi, conTruoc, loc.transform.childCount));
        Kiem(coSS && !ssAnh && loeSS == 1, "doi chung: khong bat duoc tia / loe cua mot cu Sam set that");
        Kiem(moi.Count >= 4 && kieuSS == moi.Count, "tia set Loc xoay khong dung kieu tia Sam set");
        Kiem(soChamDat >= 4 && dauMieng >= soChamDat, "tia set khong danh tu mieng loc xuong dat");
        Kiem(loeMoi == soChamDat, "moi tia cham dat khong co dung mot loe cham dat di theo loc");
        Kiem(loc.transform.childCount == conTruoc, "loe cham dat lam con cua loc (Hoa loc xoay se phong ca loe)");
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
