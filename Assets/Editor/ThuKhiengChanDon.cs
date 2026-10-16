using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 136 - DON CHI CHAM MAT KHIEN (16/10/2026, nguoi dung: "tat ca tia set, tia dien, bang, gio loc (tru Loc xoay) danh vao nguoi choi dang bat
/// khien thi chi cham vao khien, khong the xuyen vao ben trong"; chon: Gio loc TAN tai mat khien, tia lan tiep TU MAT KHIEN, ca tia Quy cay).
/// Play Act2 that, tat GameDirector + xoa quai. Bia T (lop Enemy) bat khien ban kinh nhu nguoi choi; E2 khong khien dung NGOAI vom.
/// "Tren mat vom" do DOC LAP voi Khieng.CatVom: MeshCollider tam dung CHINH luoi vom dang hien (ca do tho phong to / thu nho), tia ban tu ngoai
/// vao tam -> khoang ho giua diem do va mat luoi that (|ho| &lt;= 0,12 m; tia dien bo vom +4 cm).
///   A. Giut set (1 mach, choang 100%): tia dung o mat vom, bam theo vom khi T di 1 m, dien bo vom (moi dau tren mat), T khong mat mau / khong
///      choang, khien mat 75; tia LAN sang E2 XUAT PHAT TU MAT VOM, E2 an don + choang. Doi chung tat khien: tia toi nguc, T choang.
///   B. Tia Quy cay (GiatSet.PhongCuaQuai, quang xanh la): dung o mat vom, tia bo vom mau xanh la, khong choang.
///   C. Qua cau dien: cau hien NGOAI vom; moi tia vao T dung o mat vom, T khong choang; E2 an tia toi nguc.
///   D. Sam set (8 tia vao T, choang 100%): ca 8 dung o mat vom, khong choang, khien mat 8 x 26; doi chung E2: choang, khong cham vom.
///   E. May giong: co tia dung o mat vom, T khong chay den / khong nga / khong mat mau; E2 (ngoai vom) bi chay den.
///   F. Mua bang: co vien vo tren mat vom, T khong bang / khong cham; E2 bi cham. F2: vom trum vien bang dang roi -> vo ngay tren mat vom.
///   G. Gio loc (hat tung 100%): tan dung khi than loc cham vom (tam loc cach tam vom = banKinh + than chan), khien an 75, T khong hat tung;
///      doi chung E2: loc bay qua, E2 bi hat tung. G3: loc sinh TRONG vom (nguoi tung dung trong vom) -> khong tan.
///   H. Nguoi tung dung trong khien CUA MINH: tia ra ngoai binh thuong (khong bi khien minh chan).
/// Ket qua: PlayTestShots/khiengchandon.txt.
/// </summary>
public static class ThuKhiengChanDon
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_KhiengChanDon";
    const int LopDo = 31;

    [MenuItem("Diablo 2.5D/136. Chay thu DON CHI CHAM MAT KHIEN (set · dien · bang · gio loc)", false, 226)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[KhiengChanDon] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[KhiengChanDon] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[KhiengChanDon] " + s); }
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

    static Vector3 Dat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f); return p; }

    static readonly List<GameObject> rac = new List<GameObject>();

    static Damageable TaoBia(string ten, Vector3 p)
    {
        var go = new GameObject(ten);
        go.layer = LayerMask.NameToLayer("Enemy");
        go.transform.position = p;
        var than = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        than.name = "Than";
        than.layer = go.layer;
        than.transform.SetParent(go.transform, false);
        than.transform.localPosition = new Vector3(0f, 1f, 0f);
        var d = go.AddComponent<Damageable>();
        d.maxHealth = d.health = 1e7f; d.tiLeDoDon = 0f;
        rac.Add(go);
        return d;
    }

    static Khieng BatKhien(Damageable d)
    {
        var k = Khieng.Bat(d.gameObject, 1e6f, 3.036f);
        d.khieng = k;
        return k;
    }

    static void Dat(Damageable d, Vector3 p) { d.transform.position = Dat(p); Physics.SyncTransforms(); }

    static void XoaHieuUng(Damageable d)
    {
        foreach (var c in d.GetComponents<StunnedEffect>()) Object.DestroyImmediate(c);
        foreach (var c in d.GetComponents<FrozenEffect>()) Object.DestroyImmediate(c);
        foreach (var c in d.GetComponents<BiHatTung>()) Object.DestroyImmediate(c);
        foreach (var c in d.GetComponents<BiDanhNga>()) Object.DestroyImmediate(c);
        foreach (var c in d.GetComponents<ChayDenToanThan>()) Object.DestroyImmediate(c);
        foreach (var c in d.GetComponents<BiUot>()) Object.DestroyImmediate(c);
        d.health = d.maxHealth;
    }

    // ---------- do DOC LAP: mat luoi vom that ----------
    static GameObject doVom;

    /// <summary>Khoang ho (m) cua p so voi MAT LUOI VOM THAT cua k: duong = ngoai mat, am = trong. NaN = khong do duoc.</summary>
    static float HoVom(Khieng k, Vector3 p)
    {
        var qc = k.transform.Find("Khieng/QuaCau");
        if (qc == null) return float.NaN;
        var mf = qc.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return float.NaN;
        if (doVom == null)
        {
            doVom = new GameObject("TAM_DoVom");
            doVom.layer = LopDo;
            doVom.AddComponent<MeshCollider>();
            rac.Add(doVom);
        }
        var mc = doVom.GetComponent<MeshCollider>();
        if (mc.sharedMesh != mf.sharedMesh) mc.sharedMesh = mf.sharedMesh;
        doVom.transform.position = qc.position;
        doVom.transform.rotation = qc.rotation;
        doVom.transform.localScale = qc.lossyScale;
        Physics.SyncTransforms();
        Vector3 c = k.transform.position + Vector3.up * 0.3f;
        Vector3 v = p - c;
        if (v.sqrMagnitude < 1e-6f) return float.NaN;
        Vector3 h = v.normalized;
        RaycastHit hit;
        if (!Physics.Raycast(c + h * 15f, -h, out hit, 30f, 1 << LopDo, QueryTriggerInteraction.Collide)) return float.NaN;
        return v.magnitude - (hit.point - c).magnitude;
    }

    static bool TrenMat(Khieng k, Vector3 p, float dung = 0.12f)
    {
        float ho = HoVom(k, p);
        return !float.IsNaN(ho) && Mathf.Abs(ho) <= dung;
    }

    static List<LightningArc> TiaMoi(HashSet<LightningArc> cu, string ten)
    {
        var ds = new List<LightningArc>();
        foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsSortMode.None))
            if (!cu.Contains(a) && (ten == null ? a.name != VfxFactory.TenTiaBoVom : a.name == ten)) ds.Add(a);
        return ds;
    }

    static HashSet<LightningArc> ChupTia()
    {
        return new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsSortMode.None));
    }

    /// <summary>Moi dau moi tia bo vom moi sinh nam tren mat vom (ho 0..0,12 m, tren mat dat cua vom). Tra ve so tia, ho lon nhat.</summary>
    static int KiemTiaBoVom(Khieng k, HashSet<LightningArc> cu, out float hoMax, out float hoMin)
    {
        hoMax = -9f; hoMin = 9f; int so = 0;
        foreach (var a in TiaMoi(cu, VfxFactory.TenTiaBoVom))
        {
            so++;
            foreach (var p in new[] { a.start, a.end })
            {
                float ho = HoVom(k, p);
                if (float.IsNaN(ho)) ho = 99f;
                hoMax = Mathf.Max(hoMax, ho); hoMin = Mathf.Min(hoMin, ho);
            }
        }
        return so;
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
        yield return new WaitForSeconds(0.5f);
        var cam = Camera.main.transform;
        Vector3 truoc = cam.forward; truoc.y = 0; truoc.Normalize();
        Vector3 phai = Vector3.Cross(Vector3.up, truoc);
        Vector3 goc = toi.transform.position;
        LayerMask mask = LayerMask.GetMask("Enemy");

        var T = TaoBia("TAM_BiaKhien", Dat(goc + truoc * 8f));
        var E2 = TaoBia("TAM_BiaNgoai", Dat(goc + truoc * 6.5f + phai * 3.6f));
        var k = BatKhien(T);
        yield return null; yield return null;
        Ghi(string.Format("khien T: ban kinh {0:F3}, cao vom {1:F3}; E2 cach tam T {2:F2} m (ngoai vom)", k.banKinh, k.CaoVom,
            Vector3.Distance(Dat(T.transform.position), E2.transform.position)));
        Kiem(!float.IsNaN(HoVom(k, k.transform.position + Vector3.up * k.CaoVom)), "khong do duoc luoi vom that");

        // ===== A. Giut set =====
        Ghi("");
        {
            Vector3 tay = goc + Vector3.up * 1.3f;
            float mau0 = k.mau; int cv0 = GiatSet.SoChamVom, bv0 = VfxFactory.SoTiaBoVom;
            var cu = ChupTia();
            var gs = GiatSet.Phong(tay, (T.transform.position - goc), mask);
            gs.boQua = toi; gs.soTiaDau = 1; gs.xacSuatChoang = 1f;
            gs.hopDelay = 0.5f;     // nhip lan cham lai: khung dau Play co the dai hon 0,07 s, doc nham tia lan
            yield return null; yield return null;
            Vector3 dau = GiatSet.DenTiaCuoi;
            LightningArc tiaDau = null;
            foreach (var a in TiaMoi(cu, null)) if (Vector3.Distance(a.end, dau) < 0.05f) tiaDau = a;
            float hoDau = HoVom(k, dau);
            bool coTiaDau = tiaDau != null;      // tia song 0,6 s roi tu Destroy -> phai giu ket qua truoc
            float hoBo, hoBoMin; int soBo = KiemTiaBoVom(k, cu, out hoBo, out hoBoMin);
            var cuLan = ChupTia();
            // bam theo vom: T di 1 m (truoc khi tia dau het 0,6 s), roi ve cho cu truoc nhip lan
            Vector3 endTruoc = tiaDau != null ? tiaDau.end : Vector3.zero;
            T.transform.position += phai * 1f; Physics.SyncTransforms();
            yield return null; yield return null;
            float diTheo = tiaDau != null ? Vector3.Distance(tiaDau.end, endTruoc) : -1f;
            float hoSauDi = tiaDau != null ? HoVom(k, tiaDau.end) : float.NaN;
            T.transform.position -= phai * 1f; Physics.SyncTransforms();
            yield return new WaitForSeconds(0.65f);
            LightningArc tiaLan = null;
            Vector3 nguc2 = E2.GetComponentInChildren<Collider>().bounds.center;
            foreach (var a in TiaMoi(cuLan, null)) if (Vector3.Distance(a.end, nguc2) < 0.2f) tiaLan = a;
            float hoLan = tiaLan != null ? HoVom(k, tiaLan.start) : float.NaN;
            Ghi(string.Format("A. Giut set vao T: cham vom +{0}; diem cuoi ho {1:F3} m (nguc T ho {2:F3}); dien bo vom +{3} tia, ho {4:F3}..{5:F3}; T mat mau {6:F0}, choang {7}, khien mat {8:F1}",
                GiatSet.SoChamVom - cv0, hoDau, HoVom(k, T.GetComponentInChildren<Collider>().bounds.center), VfxFactory.SoTiaBoVom - bv0, hoBoMin, hoBo,
                T.maxHealth - T.health, T.GetComponent<StunnedEffect>() != null, mau0 - k.mau));
            Ghi(string.Format("A. tia LAN sang E2: co {0}, diem dau ho {1:F3} m; E2 mat mau {2:F1}, choang {3}; T di 1 m -> dau tia di {4:F3} m, ho sau khi di {5:F3}",
                tiaLan != null, hoLan, E2.maxHealth - E2.health, E2.GetComponent<StunnedEffect>() != null, diTheo, hoSauDi));
            Kiem(GiatSet.SoChamVom - cv0 == 1 && coTiaDau && Mathf.Abs(hoDau) <= 0.12f, "tia Giut set khong dung o mat vom");
            Kiem(soBo >= 3 && hoBoMin >= -0.06f && hoBo <= 0.16f, "dien bo vom thieu / khong nam tren mat vom");
            Kiem(T.health == T.maxHealth && T.GetComponent<StunnedEffect>() == null && Mathf.Abs(mau0 - k.mau - GiatSet.SatThuongNguoiChoi) < 0.5f,
                 "T mat mau / bi choang / khien khong an dung 75");
            Kiem(tiaLan != null && Mathf.Abs(hoLan) <= 0.12f && E2.health < E2.maxHealth && E2.GetComponent<StunnedEffect>() != null,
                 "tia lan khong xuat phat tu mat vom / E2 khong an don");
            Kiem(diTheo > 0.9f && diTheo < 1.1f && Mathf.Abs(hoSauDi) <= 0.12f, "dau tia khong bam theo vom khi T di");
            yield return new WaitForSeconds(0.8f);
            XoaHieuUng(T); XoaHieuUng(E2);

            // doi chung: tat khien
            k.Tat(); T.khieng = null;
            int cv1 = GiatSet.SoChamVom;
            var gs2 = GiatSet.Phong(tay, (T.transform.position - goc), mask);
            gs2.boQua = toi; gs2.soTiaDau = 1; gs2.xacSuatChoang = 1f; gs2.maxChains = 0;
            yield return null; yield return null;
            Vector3 ngucT = T.GetComponentInChildren<Collider>().bounds.center;
            Ghi(string.Format("A2. DOI CHUNG khong khien: cham vom +{0}, diem cuoi cach nguc {1:F3} m, T choang {2}",
                GiatSet.SoChamVom - cv1, Vector3.Distance(GiatSet.DenTiaCuoi, ngucT), T.GetComponent<StunnedEffect>() != null));
            Kiem(GiatSet.SoChamVom - cv1 == 0 && Vector3.Distance(GiatSet.DenTiaCuoi, ngucT) < 0.05f && T.GetComponent<StunnedEffect>() != null,
                 "doi chung khong khien sai (tia khong toi nguc / khong choang)");
            yield return new WaitForSeconds(0.8f);
            XoaHieuUng(T);
            k = BatKhien(T);
            yield return null;
        }

        // ===== B. Quy cay =====
        Ghi("");
        {
            int cv0 = GiatSet.SoChamVom;
            var cu = ChupTia();
            Color xanhLa = new Color(0.2f, 1f, 0.3f, 1f);
            Vector3 tu = Dat(goc + truoc * 1f) + Vector3.up * 1.5f;
            GiatSet.PhongCuaQuai(tu, T.transform.position - tu, mask, 40f, 12f, Color.white, xanhLa, 1f, 1f);
            yield return null; yield return null;
            float hoDau = HoVom(k, GiatSet.DenTiaCuoi);
            int soXanh = 0, soBo = 0;
            foreach (var a in TiaMoi(cu, VfxFactory.TenTiaBoVom)) { soBo++; if (a.glowColor == xanhLa) soXanh++; }
            Ghi(string.Format("B. tia Quy cay vao T: cham vom +{0}, ho {1:F3}; tia bo vom {2} (mau xanh la {3}); T choang {4}, mat mau {5:F0}",
                GiatSet.SoChamVom - cv0, hoDau, soBo, soXanh, T.GetComponent<StunnedEffect>() != null, T.maxHealth - T.health));
            Kiem(GiatSet.SoChamVom - cv0 == 1 && Mathf.Abs(hoDau) <= 0.12f && soBo >= 3 && soXanh == soBo && T.GetComponent<StunnedEffect>() == null
                 && T.health == T.maxHealth, "tia Quy cay xuyen khien / sai mau / choang");
            yield return new WaitForSeconds(0.8f);
            XoaHieuUng(T);
        }

        // ===== C. Qua cau dien =====
        Ghi("");
        {
            int cv0 = QuaCauDien.SoChamVom, ban0 = QuaCauDien.SoTiaDaBan;
            var cu = ChupTia();
            var qc = QuaCauDien.Spawn(T.transform.position, mask, toi);
            yield return null;
            Vector3 v = qc.transform.position - T.transform.position; v.y = 0f;
            float cachTam = v.magnitude;
            float hoLonNhat = 0f; int soTiaT = 0, soTiaE2 = 0, ngucE2 = 0; bool choang = false;
            Vector3 ngucT = T.GetComponentInChildren<Collider>().bounds.center;
            var daXet = new HashSet<LightningArc>(cu);
            float hanC = Time.time + 2.2f;
            while (Time.time < hanC)
            {
                yield return null;
                foreach (var a in TiaMoi(daXet, null))
                {
                    daXet.Add(a);
                    if (a.name == TiaBoDat.TenTia) continue;
                    Vector3 ngucE = E2.GetComponentInChildren<Collider>().bounds.center;
                    if (Vector3.Distance(a.end, ngucE) < 0.3f) { soTiaE2++; ngucE2++; continue; }
                    // tia vao T: dau tia o cau
                    if (Vector3.Distance(a.start, qc != null ? qc.transform.position : a.start) > 0.3f) continue;
                    soTiaT++;
                    float ho = HoVom(k, a.end);
                    if (float.IsNaN(ho)) ho = 99f;
                    hoLonNhat = Mathf.Max(hoLonNhat, Mathf.Abs(ho));
                }
                if (T.GetComponent<StunnedEffect>() != null) choang = true;
            }
            Ghi(string.Format("C. Qua cau dien: cau cach tam T {0:F2} m (vom {1:F2}); tia vao T {2}, cham vom +{3}, |ho| lon nhat {4:F3}; tia vao nguc E2 {5}; T choang {6}, mat mau {7:F0}",
                cachTam, k.banKinh, soTiaT, QuaCauDien.SoChamVom - cv0, hoLonNhat, ngucE2, choang, T.maxHealth - T.health));
            Kiem(cachTam > k.banKinh + 0.3f, "qua cau hien trong vom khien");
            Kiem(soTiaT >= 3 && QuaCauDien.SoChamVom - cv0 == soTiaT && hoLonNhat <= 0.12f, "tia Qua cau dien xuyen khien");
            Kiem(ngucE2 >= 3, "E2 (khong khien) khong an tia toi nguc");
            Kiem(!choang && T.health == T.maxHealth, "T bi choang / mat mau");
            if (qc != null) Object.Destroy(qc.gameObject);
            yield return new WaitForSeconds(0.8f);
            XoaHieuUng(T); XoaHieuUng(E2);
        }

        // ===== D. Sam set =====
        Ghi("");
        {
            float mau0 = k.mau; int cv0 = LightningStrike.SoChamVom;
            float hoMax = 0f; int trenMat = 0;
            for (int i = 0; i < 8; i++)
            {
                Vector2 r = Random.insideUnitCircle * 0.4f;
                var s = LightningStrike.Spawn(Dat(T.transform.position + new Vector3(r.x, 0f, r.y)), 20f, 0.02f);
                s.damage = 26f; s.impactRadius = 2.1f; s.damageMask = mask; s.boQua = toi; s.stunChance = 1f; s.stunSeconds = 1f;
                int truoc0 = LightningStrike.SoChamVom;
                float hanD = Time.time + 1f;
                while (LightningStrike.SoChamVom == truoc0 && Time.time < hanD) yield return null;
                float ho = HoVom(k, LightningStrike.DiemChamVomCuoi);
                if (!float.IsNaN(ho) && Mathf.Abs(ho) <= 0.12f) trenMat++;
                hoMax = Mathf.Max(hoMax, float.IsNaN(ho) ? 99f : Mathf.Abs(ho));
                yield return new WaitForSeconds(0.15f);
            }
            Ghi(string.Format("D. 8 tia Sam set vao T: cham vom +{0}, tren mat {1}/8 (|ho| lon nhat {2:F3}); T choang {3}, mat mau {4:F0}; khien mat {5:F1} (mong {6:F1})",
                LightningStrike.SoChamVom - cv0, trenMat, hoMax, T.GetComponent<StunnedEffect>() != null, T.maxHealth - T.health, mau0 - k.mau, 8 * 26f));
            Kiem(LightningStrike.SoChamVom - cv0 == 8 && trenMat == 8, "tia Sam set xuyen khien");
            Kiem(T.GetComponent<StunnedEffect>() == null && T.health == T.maxHealth && Mathf.Abs(mau0 - k.mau - 8 * 26f) < 1f, "T bi choang / mat mau / khien khong an du");
            // doi chung E2
            int cv1 = LightningStrike.SoChamVom;
            var s2 = LightningStrike.Spawn(Dat(E2.transform.position), 20f, 0.02f);
            s2.damage = 26f; s2.impactRadius = 2.1f; s2.damageMask = mask; s2.boQua = toi; s2.stunChance = 1f; s2.stunSeconds = 1f;
            yield return new WaitForSeconds(0.2f);
            Ghi(string.Format("D2. DOI CHUNG E2 (khong khien, ngoai vom): cham vom +{0}, E2 choang {1}, mat mau {2:F1}",
                LightningStrike.SoChamVom - cv1, E2.GetComponent<StunnedEffect>() != null, E2.maxHealth - E2.health));
            Kiem(LightningStrike.SoChamVom - cv1 == 0 && E2.GetComponent<StunnedEffect>() != null && E2.health < E2.maxHealth, "doi chung Sam set sai");
            yield return new WaitForSeconds(1.2f);
            XoaHieuUng(T); XoaHieuUng(E2);
        }

        // ===== E. May giong =====
        Ghi("");
        {
            int cv0 = MayGiong.SoChamVom;
            float hoMax = 0f; bool chayT = false, ngaT = false, chayE2 = false;
            var mg = MayGiong.Spawn(T.transform.position, mask, toi);
            int truocCv = MayGiong.SoChamVom;
            float hanE = Time.time + 5.6f;
            while (Time.time < hanE)
            {
                yield return null;
                if (MayGiong.SoChamVom != truocCv)
                {
                    truocCv = MayGiong.SoChamVom;
                    float ho = HoVom(k, MayGiong.DiemTiaCuoi);
                    hoMax = Mathf.Max(hoMax, float.IsNaN(ho) ? 99f : Mathf.Abs(ho));
                }
                if (T.GetComponent<ChayDenToanThan>() != null) chayT = true;
                if (T.GetComponent<BiDanhNga>() != null) ngaT = true;
                if (E2.GetComponent<ChayDenToanThan>() != null) chayE2 = true;
            }
            Ghi(string.Format("E. May giong tren T: cham vom +{0} (|ho| lon nhat {1:F3}); T chay den {2}, nga {3}, mat mau {4:F0}; E2 (ngoai vom) chay den {5}",
                MayGiong.SoChamVom - cv0, hoMax, chayT, ngaT, T.maxHealth - T.health, chayE2));
            Kiem(MayGiong.SoChamVom - cv0 >= 3 && hoMax <= 0.12f, "tia May giong xuyen khien");
            Kiem(!chayT && !ngaT && T.health == T.maxHealth, "T bi chay den / nga / mat mau");
            Kiem(chayE2, "doi chung E2 khong bi chay den");
            if (mg != null) Object.Destroy(mg.gameObject);
            yield return new WaitForSeconds(1f);
            XoaHieuUng(T); XoaHieuUng(E2);
        }

        // ===== F. Mua bang =====
        Ghi("");
        {
            int vv0 = FallingShard.SoVoTrenVom;
            float hoMax = 0f; bool bangT = false, chamE2 = false;
            var st = IceStorm.Spawn(T.transform.position, mask);
            st.boQua = toi;
            int truocV = vv0;
            float hanF = Time.time + st.duration + 1.2f;
            while (Time.time < hanF)
            {
                yield return null;
                if (FallingShard.SoVoTrenVom != truocV)
                {
                    truocV = FallingShard.SoVoTrenVom;
                    float ho = HoVom(k, FallingShard.DiemVoVomCuoi);
                    hoMax = Mathf.Max(hoMax, float.IsNaN(ho) ? 99f : Mathf.Abs(ho));
                }
                if (T.GetComponent<FrozenEffect>() != null) bangT = true;
                if (E2.GetComponent<FrozenEffect>() != null) chamE2 = true;
            }
            Ghi(string.Format("F. Mua bang tren T: vo tren vom +{0} (|ho| lon nhat {1:F3}); T dinh bang / cham {2}, mat mau {3:F0}; E2 (ngoai vom) bi cham {4}",
                FallingShard.SoVoTrenVom - vv0, hoMax, bangT, T.maxHealth - T.health, chamE2));
            Kiem(FallingShard.SoVoTrenVom - vv0 >= 3 && hoMax <= 0.12f, "vien Mua bang xuyen khien");
            Kiem(!bangT && T.health == T.maxHealth, "T bi bang / cham / mat mau");
            Kiem(chamE2, "doi chung E2 khong bi cham");
            yield return new WaitForSeconds(1f);
            XoaHieuUng(T); XoaHieuUng(E2);

            // F2: vom trum vien bang dang roi (vien bat dau TRONG vom)
            int vv1 = FallingShard.SoVoTrenVom;
            Vector3 dich = Dat(T.transform.position + phai * 1f);
            var vien = VfxFactory.QuaCauBangRoi(dich, 1.8f, 0.6f);
            var fs = vien.GetComponent<FallingShard>();
            bool coFs = fs != null;      // vien vo xong tu Destroy -> fs == null, phai giu ket qua truoc
            if (coFs) { fs.damage = 30f; fs.impactRadius = 1.7f; fs.damageMask = mask; fs.boQua = toi; }
            yield return null; yield return null; yield return null;
            float ho2 = HoVom(k, FallingShard.DiemVoVomCuoi);
            Ghi(string.Format("F2. vien bang sinh TRONG vom (1,8 m tren dat): vo +{0}, ho {1:F3}", FallingShard.SoVoTrenVom - vv1, ho2));
            Kiem(coFs && FallingShard.SoVoTrenVom - vv1 == 1 && Mathf.Abs(ho2) <= 0.12f, "vien bang trong vom khong vo len mat vom");
            yield return new WaitForSeconds(1f);
            XoaHieuUng(T);
        }

        // ===== G. Gio loc =====
        Ghi("");
        {
            float mau0 = k.mau; int tv0 = GioLoc.SoTanVaoVom;
            Vector3 chan = Dat(T.transform.position - truoc * 8f);
            var loc = GioLoc.Spawn(chan, truoc, mask);
            loc.boQua = toi; loc.xacSuatHatTung = 1f;
            float hanG = Time.time + 2f;
            while (GioLoc.SoTanVaoVom == tv0 && Time.time < hanG) yield return null;
            Vector3 cho = GioLoc.ChoTanVaoVomCuoi;
            Vector3 v = cho - T.transform.position; v.y = 0f;
            float mong = k.banKinh + GioLoc.BanKinhThanChan;
            // than loc cham mat luoi vom that o chan: diem tren than loc phia T, sat dat
            Vector3 mepLoc = cho - v.normalized * GioLoc.BanKinhThanChan + Vector3.up * 0.35f;
            float hoMep = HoVom(k, mepLoc);
            Vector3 locLucTan = loc != null ? loc.transform.position : Vector3.zero;
            yield return new WaitForSeconds(0.2f);
            bool hatT = T.GetComponent<BiHatTung>() != null;
            yield return new WaitForSeconds(0.6f);
            float locDi = loc != null ? Vector3.Distance(loc.transform.position, locLucTan) : 0f;
            Ghi(string.Format("G. Gio loc vao T: tan vao vom +{0}; tam loc cach tam vom {1:F3} m (ban kinh vom + than chan = {2:F3}); mep than loc ho {3:F3} m so voi luoi vom; khien mat {4:F1}; T hat tung {5}, mat mau {6:F0}; 0,8 s sau loc di them {7:F3} m",
                GioLoc.SoTanVaoVom - tv0, v.magnitude, mong, hoMep, mau0 - k.mau, hatT, T.maxHealth - T.health, locDi));
            Kiem(GioLoc.SoTanVaoVom - tv0 == 1 && Mathf.Abs(v.magnitude - mong) < 0.05f && Mathf.Abs(hoMep) <= 0.15f, "Gio loc khong tan tai mat vom");
            Kiem(Mathf.Abs(mau0 - k.mau - GioLoc.SatThuongGoc) < 0.5f && !hatT && T.health == T.maxHealth, "khien khong an 75 / T bi hat tung / mat mau");
            Kiem(locDi < 0.01f, "loc van bay tiep sau khi tan vao vom");
            if (loc != null) Object.Destroy(loc.gameObject);
            XoaHieuUng(T);

            // doi chung E2 khong khien: loc bay qua
            int tv1 = GioLoc.SoTanVaoVom;
            Vector3 chan2 = Dat(E2.transform.position - phai * 6f);
            var loc2 = GioLoc.Spawn(chan2, phai, mask);
            loc2.boQua = toi; loc2.xacSuatHatTung = 1f;
            yield return new WaitForSeconds(1.1f);
            Ghi(string.Format("G2. DOI CHUNG E2 (khong khien): tan vao vom +{0}, loc con bay {1}, E2 hat tung {2}, mat mau {3:F0}",
                GioLoc.SoTanVaoVom - tv1, loc2 != null, E2.GetComponent<BiHatTung>() != null, E2.maxHealth - E2.health));
            Kiem(GioLoc.SoTanVaoVom - tv1 == 0 && loc2 != null && E2.GetComponent<BiHatTung>() != null, "doi chung Gio loc sai");
            if (loc2 != null) Object.Destroy(loc2.gameObject);
            yield return new WaitForSeconds(1f);
            XoaHieuUng(E2);

            // G3: loc sinh TRONG vom
            int tv2 = GioLoc.SoTanVaoVom;
            var loc3 = GioLoc.Spawn(Dat(T.transform.position + phai * 0.5f), truoc, mask);
            loc3.boQua = toi;
            yield return new WaitForSeconds(0.25f);
            Ghi(string.Format("G3. loc sinh TRONG vom: tan vao vom +{0}, loc con bay {1}", GioLoc.SoTanVaoVom - tv2, loc3 != null));
            Kiem(GioLoc.SoTanVaoVom - tv2 == 0 && loc3 != null, "loc sinh trong vom bi chinh vom ay chan");
            if (loc3 != null) Object.Destroy(loc3.gameObject);
            yield return new WaitForSeconds(0.5f);
            XoaHieuUng(T);
        }

        // ===== H. nguoi tung dung trong khien cua minh =====
        Ghi("");
        {
            int cv0 = GiatSet.SoChamVom;
            Vector3 tay = T.transform.position + Vector3.up * 1.3f;
            var gs = GiatSet.Phong(tay, E2.transform.position - T.transform.position, mask);
            gs.boQua = T; gs.soTiaDau = 1; gs.maxChains = 0;
            yield return null; yield return null;
            Vector3 ngucE = E2.GetComponentInChildren<Collider>().bounds.center;
            Ghi(string.Format("H. T (co khien) phong Giut set ra E2: cham vom +{0}, diem cuoi cach nguc E2 {1:F3} m",
                GiatSet.SoChamVom - cv0, Vector3.Distance(GiatSet.DenTiaCuoi, ngucE)));
            Kiem(GiatSet.SoChamVom - cv0 == 0 && Vector3.Distance(GiatSet.DenTiaCuoi, ngucE) < 0.05f, "khien cua chinh nguoi tung chan tia cua ho");
        }

        yield return new WaitForSeconds(0.8f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/khiengchandon.txt", bao.ToString());
        foreach (var g in rac) if (g != null) Object.Destroy(g);
        rac.Clear(); doVom = null;
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
        Debug.Log("[KhiengChanDon] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
