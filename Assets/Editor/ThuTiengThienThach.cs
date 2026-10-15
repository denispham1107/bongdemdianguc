using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 126 - TIENG THIEN THACH (15/10/2026, nguoi dung: mau A "Đá lửa thật"; roi · no · lua chay; ca Quy du va cay / bia / nha mo bi dot;
/// nguoi khac trong suong thi khong nghe). Play Act2 that, tat GameDirector + xoa quai, BAT tam nhin + tieng trong phep thu, tat nhac nen.
///   A. (ngoai Play) bay file (roi 1-3, no 1-3, lua chay) trung tung byte ban goc CongCu/AmThanh/ThienThachThu3/A_dien_anh_*.wav (ban 3,
///      16/10/2026 - nguoi dung: ban truoc "con qua mono", "chua giong thien thach roi va vo tung"), STEREO; lua chay 3 s.
///   G. BIEN THE: 30 tieng roi + 30 tieng no -> du 3 bien the moi loai, cao do trong +-6%.
///   B. TUNG THAT (CastAt 4) cach ~9 m: theo doi tung khung - moi thien thach mot tieng roi tu luc hien hinh, tieng roi CON KEU toi luc
///      cham va tat ngay sau; moi vu no mot tieng no; moi vung lua mot tieng lua (tran 4), lua yeu dan 2 s cuoi, het 5 s thi tat.
///      Loa co tieng, hai kenh khac nhau.
///   C. CAY BI DOT (CayChay.Dot cay that gan nhan vat): tieng lua o giua than cay, to dan theo phan da chay, tat khi chay rui.
///   D. QUY DU (ThienThach.Spawn, khong vung lua): roi + no, 0 tieng lua.
///   E. SUONG MU: thien thach o 40 m: tieng roi + lua muc 0 suot, tieng no bi bo, loa im; doi chung tat suong -> co tieng no.
/// Ket qua: PlayTestShots/tiengthienthach.txt.
/// </summary>
public static class ThuTiengThienThach
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengThienThach";

    [MenuItem("Diablo 2.5D/126. Chay thu TIENG THIEN THACH (roi · no · lua chay, suong mu)", false, 215)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengTT] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengTT] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 2] tieng thien thach (ban 3 'Dien anh', 3 bien the)");
        string[] ten = { "ThienThachRoi1", "ThienThachRoi2", "ThienThachRoi3", "ThienThachNo1", "ThienThachNo2", "ThienThachNo3", "LuaChay" },
                 goc = { "roi1", "roi2", "roi3", "no1", "no2", "no3", "chay" };
        for (int i = 0; i < ten.Length; i++)
        {
            string game = "Assets/Resources/AmThanh/KyNang/" + ten[i] + ".wav";
            string g = "CongCu/AmThanh/ThienThachThu3/A_dien_anh_" + goc[i] + ".wav";
            var imp = AssetImporter.GetAtPath(game) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
            bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
            Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh {3:F3} s; ForceToMono {4}", Path.GetFileName(game), trung,
                c != null ? c.channels : 0, c != null ? c.length : 0, imp != null && imp.forceToMono));
            Kiem(trung && c.channels == 2 && imp != null && !imp.forceToMono, "file " + ten[i] + " khong dung ban goc / khong STEREO");
            if (ten[i] == "LuaChay") Kiem(c != null && Mathf.Abs(c.length - 3f) < 0.001f, "tieng lua chay khong dai dung 3 s (vong lap)");
        }
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        tatTiengGoc = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = false;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static string Md5(string d) { using (var m = MD5.Create()) using (var f = File.OpenRead(d)) return System.BitConverter.ToString(m.ComputeHash(f)); }
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengTT] " + s); }
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

    static int Dem(TiengViTri.Loai l)
    {
        int n = 0;
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l && !g.dung) n++;
        return n;
    }

    static float MucLonNhat(TiengViTri.Loai l)
    {
        float m = 0f;
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l) m = Mathf.Max(m, g.muc);
        return m;
    }

    /// <summary>Muc / so giong lua cua VUNG LUA (bo tieng lua cua cay bi vung lua dot - cay chay lau hon).</summary>
    static float MucLuaVung(out int so)
    {
        float m = 0f; so = 0;
        foreach (var g in TiengViTri.CacGiong)
            if (!g.Ranh && g.loai == TiengViTri.Loai.LuaChay && g.theo != null && g.theo.GetComponent<VungLua>() != null) { m = Mathf.Max(m, g.muc); so++; }
        return m;
    }

    static IEnumerator ChoIm(float toiDa)
    {
        float han = Time.realtimeSinceStartup + toiDa;
        while (Time.realtimeSinceStartup < han)
        {
            bool ranh = true;
            foreach (var g in TiengViTri.CacGiong) if (!g.Ranh) ranh = false;
            if (ranh) break;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.15f);
    }

    static Vector3 TrenDat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f); return p; }

    static Vector3 HuongTrong(Vector3 goc, PlayerController pc, float dai)
    {
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc + Vector3.up, 0.6f, h, out RaycastHit _, dai, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return Vector3.forward;
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
        NhacNen.BatTrongPhepThu = true;
        TamNhin.BatTrongPhepThu = true;
        var nn = NhacNen.Ban;
        if (nn != null) { nn.sanh.nguon.mute = true; nn.tran.nguon.mute = true; }
        yield return new WaitForSeconds(1.0f);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        Vector3 goc = pc.transform.position;
        Vector3 h = HuongTrong(goc, pc, 12f);
        pc.transform.rotation = Quaternion.LookRotation(h);

        // Khoi dong (lan dau sinh hinh thien thach / vu no Editor khung giat - xem menu 125): mot qua o xa bo di
        var kd = ThienThach.Spawn(TrenDat(goc + h * 4f), 0, 0);
        kd.chayThoiGian = 0f; kd.chayBanKinh = 0f;
        yield return new WaitForSeconds(2.0f);
        yield return ChoIm(5f);

        // ===== B. tung that =====
        Ghi("");
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(4);
        pc.mana = pc.maxMana;
        int r0 = TiengViTri.SoLan[3], n0 = TiengViTri.SoLan[4], l0 = TiengViTri.SoLan[5];
        Vector3 ngam = TrenDat(goc + h * 9f);
        pc.CastAt(4, ngam);
        var bl = new float[1024]; var br = new float[1024];
        double sl = 0, sr = 0, slr = 0; float rmsDinh = 0;
        var tt = new List<ThienThach>();
        var batDauRoi = new Dictionary<ThienThach, float>();
        var roiConKeuLucNo = new List<bool>();
        var khungKeuCuoi = new Dictionary<ThienThach, int>();
        var giayRoi = new List<float>();
        int lucNoMax = 0, luaMax = 0;
        float t0 = Time.time, lucNoCuoi = -1f;
        var roiDangKeu = new Dictionary<ThienThach, TiengViTri.Giong>();
        var khungNo = new List<float>();
        float mucLua1s = -1f, mucLua4s5 = -1f, lucHetLua = -1f;
        while (Time.time - t0 < 9f)
        {
            foreach (var x in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None)) if (!tt.Contains(x)) tt.Add(x);
            // giong roi cua tung thien thach con song
            foreach (var x in tt)
            {
                if (x == null) continue;
                foreach (var g in TiengViTri.CacGiong)
                    if (!g.Ranh && g.loai == TiengViTri.Loai.ThienThachRoi && g.theo == x.transform)
                    {
                        if (!batDauRoi.ContainsKey(x)) batDauRoi[x] = Time.time;
                        roiDangKeu[x] = g;
                        // khung cuoi tieng roi CON KEU (No() goi Dung roi Destroy cuoi khung - khung ay giong da roi chu)
                        if (g.nguon.isPlaying && g.muc > 0.05f && !g.dung) khungKeuCuoi[x] = Time.frameCount;
                    }
            }
            // thien thach vua no (bi xoa) khung truoc: tieng roi luc ay con keu khong
            foreach (var kv in new List<KeyValuePair<ThienThach, TiengViTri.Giong>>(roiDangKeu))
                if (kv.Key == null)
                {
                    int kc; khungKeuCuoi.TryGetValue(kv.Key, out kc);
                    roiConKeuLucNo.Add(kc >= Time.frameCount - 3);
                    khungNo.Add(Time.time - t0);
                    giayRoi.Add(Time.time - batDauRoi[kv.Key]);
                    lucNoCuoi = Time.time;
                    roiDangKeu.Remove(kv.Key);
                }
            lucNoMax = Mathf.Max(lucNoMax, Dem(TiengViTri.Loai.ThienThachNo));
            int lua = Dem(TiengViTri.Loai.LuaChay);
            luaMax = Mathf.Max(luaMax, lua);
            if (lucNoCuoi > 0f)
            {
                float sau = Time.time - lucNoCuoi;
                int soVung;
                float mucVung = MucLuaVung(out soVung);
                if (mucLua1s < 0f && sau >= 1f) mucLua1s = mucVung;
                if (mucLua4s5 < 0f && sau >= 4.5f) mucLua4s5 = mucVung;
                if (lucHetLua < 0f && sau > 1f && soVung == 0) lucHetLua = sau;
            }
            yield return null;
            AudioListener.GetOutputData(bl, 0);
            AudioListener.GetOutputData(br, 1);
            double a = 0, b = 0, ab = 0;
            for (int i = 0; i < bl.Length; i++) { a += bl[i] * bl[i]; b += br[i] * br[i]; ab += bl[i] * br[i]; }
            sl += a; sr += b; slr += ab;
            rmsDinh = Mathf.Max(rmsDinh, (float)System.Math.Sqrt((a + b) / (2.0 * bl.Length)));
        }
        float tq = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        int roiConKeu = 0; foreach (var v in roiConKeuLucNo) if (v) roiConKeu++;
        float giayLua = ThienThachLuaGiay(tt);
        Ghi(string.Format("B. tung that: {0} thien thach; yeu cau roi/no/lua +{1}/+{2}/+{3}; giong roi bat dau {4}; no luc {5} s; tieng roi con keu ngay truoc khi no {6}/{7}",
            tt.Count, TiengViTri.SoLan[3] - r0, TiengViTri.SoLan[4] - n0, TiengViTri.SoLan[5] - l0, batDauRoi.Count,
            string.Join(" / ", khungNo.ConvertAll(x => x.ToString("F2"))), roiConKeu, roiConKeuLucNo.Count));
        Ghi("   tu luc bat dau tieng roi toi luc no: " + string.Join(" / ", giayRoi.ConvertAll(x => x.ToString("F2"))) + " s (clip roi dai 1,40 s gom duoi vang, to nhat o 0,80-0,9 s)");
        Ghi(string.Format("   tieng no cung luc toi da {0}; tieng lua toi da {1} (tran {2}); muc lua VUNG 1 s sau vu no cuoi {3:F2}, 4,5 s sau {4:F2}; het tieng lua {5:F2} s sau vu no cuoi (lua chay {6} s)",
            lucNoMax, luaMax, TiengViTri.SoTiengLuaToiDa, mucLua1s, mucLua4s5, lucHetLua, giayLua));
        Ghi(string.Format("   loa: RMS dinh {0:F4}, tuong quan hai kenh {1:F3}", rmsDinh, tq));
        int soQua = TiengViTri.SoLan[4] - n0;
        Kiem(tt.Count >= 3 && batDauRoi.Count == tt.Count && TiengViTri.SoLan[3] - r0 == tt.Count, "moi thien thach khong co mot tieng roi");
        Kiem(roiConKeuLucNo.Count == tt.Count && roiConKeu == tt.Count, "tieng roi khong keu toi luc cham dat");
        Kiem(soQua == tt.Count && lucNoMax >= 1, "moi thien thach khong co mot tieng no");
        int soCayB = Object.FindObjectsByType<CayChay>(FindObjectsSortMode.None).Length;     // vung lua co the dot cay gan do - cung keu
        Ghi("   cay / bia / nha bi dot trong luot nay: " + soCayB);
        Kiem(TiengViTri.SoLan[5] - l0 == tt.Count + soCayB && luaMax >= Mathf.Min(tt.Count, TiengViTri.SoTiengLuaToiDa) - 1 && luaMax <= TiengViTri.SoTiengLuaToiDa,
             "tieng lua chay khong theo vung lua / vuot tran");
        Kiem(mucLua1s > 0.2f && mucLua4s5 < mucLua1s * 0.5f, "tieng lua khong yeu dan cuoi doi vung lua");
        Kiem(lucHetLua > 0f && lucHetLua < giayLua + 0.6f, "tieng lua khong tat khi vung lua het");
        Kiem(rmsDinh > 1e-3f && tq < 0.99f, "loa khong co tieng / khong stereo");
        foreach (var x in Object.FindObjectsByType<CayChay>(FindObjectsSortMode.None)) Object.Destroy(x);   // tra cay ve nguyen ven
        yield return ChoIm(5f);

        // ===== C. cay bi dot =====
        Ghi("");
        GameObject cay = null; float ganNhat = 1e9f;
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            var gocCay = CayChay.GocCay(r.transform);
            if (gocCay == null || !CayChay.LaCay(gocCay.gameObject)) continue;
            float d = Vector3.Distance(gocCay.position, goc);
            if (d < ganNhat) { ganNhat = d; cay = gocCay.gameObject; }
        }
        if (cay == null) { Ghi("[LOI] khong tim thay cay"); loi++; }
        else
        {
            // dat nhan vat cach cay ~8 m (trong tam nhin)
            Vector3 v = goc - cay.transform.position; v.y = 0; if (v.sqrMagnitude < 0.01f) v = Vector3.forward;
            var cc = pc.GetComponent<CharacterController>();
            cc.enabled = false; pc.transform.position = TrenDat(cay.transform.position + v.normalized * 8f) + Vector3.up * 0.1f; cc.enabled = true;
            Physics.SyncTransforms();
            yield return new WaitForSeconds(0.3f);
            var cc2 = CayChay.Dot(cay, 0, 0f);
            yield return null; yield return null;
            TiengViTri.Giong gc = null;
            foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.LuaChay && g.theo == cay.transform) gc = g;
            float heDau = gc != null ? gc.heSo : -1f, heGiua = -1f, heCuoi = -1f;
            Vector3 choTieng = gc != null ? gc.viTri : Vector3.zero;
            float hanC = Time.time + 15f;
            float tc = cc2 != null ? cc2.thoiGianChay : 0f;
            float t1 = Time.time;
            bool tatKhiRui = false; float lucTat = -1f;
            while (Time.time < hanC && cc2 != null)
            {
                float tuoi = Time.time - t1;
                if (gc != null && gc.theo == cay.transform)
                {
                    if (heGiua < 0f && tuoi >= tc - 2.5f) heGiua = gc.heSo;
                    if (tuoi >= tc - 0.4f && tuoi < tc) heCuoi = gc.heSo;
                }
                if (tuoi > tc + 0.4f) { tatKhiRui = gc == null || gc.theo != cay.transform; lucTat = tuoi; break; }
                yield return null;
            }
            Ghi(string.Format("C. dot cay {0} (cach {1:F1} m, chay {2:F1} s): giong lua {3}, o cao {4:F1} m tren goc; he so luc dau {5:F2}, 2,5 s truoc rui {6:F2}, 0,4 s truoc rui {7:F2}; {8:F1} s sau: giong da tra {9}",
                cay.name, 8f, tc, gc != null, gc != null ? choTieng.y - cay.transform.position.y : 0f, heDau, heGiua, heCuoi, lucTat, tatKhiRui));
            Kiem(gc != null && heDau > 0.2f && heGiua > heDau && heCuoi < heGiua * 0.5f, "tieng lua cay khong to dan theo lua lan / khong yeu dan cuoi");
            Kiem(tatKhiRui, "cay chay rui ma tieng lua con keu");
            if (cc2 != null) Object.Destroy(cc2);      // OnDestroy tra lai hinh + va cham ngay (khong cho 30 s)
            cc.enabled = false; pc.transform.position = goc + Vector3.up * 0.1f; cc.enabled = true;
            Physics.SyncTransforms();
        }
        yield return ChoIm(3f);

        // ===== D. Quy du =====
        Ghi("");
        int dr = TiengViTri.SoLan[3], dn = TiengViTri.SoLan[4], dl = TiengViTri.SoLan[5];
        var qd = ThienThach.Spawn(TrenDat(goc + h * 6f), 0, 0);
        qd.chayBanKinh = 0f; qd.chayThoiGian = 0f;      // nhu EnemyAI.GoiThienThachTaiDiem
        yield return new WaitForSeconds(2.0f);
        Ghi(string.Format("D. thien thach Quy du (khong vung lua): yeu cau roi +{0}, no +{1}, lua +{2}", TiengViTri.SoLan[3] - dr, TiengViTri.SoLan[4] - dn, TiengViTri.SoLan[5] - dl));
        Kiem(TiengViTri.SoLan[3] - dr == 1 && TiengViTri.SoLan[4] - dn == 1 && TiengViTri.SoLan[5] - dl == 0, "thien thach Quy du khong co roi + no / co lua");
        yield return ChoIm(4f);

        // ===== E. suong mu =====
        Ghi("");
        int che0 = TiengViTri.SoBiSuongChe;
        Vector3 xa = TrenDat(goc + h * 40f);
        var tx = ThienThach.Spawn(xa, 0, 0);
        float mucRoiXa = 0f, mucLuaXa = 0f, rmsXa = 0f; int khung = 0;
        float hanE = Time.time + 3.5f;
        while (Time.time < hanE)
        {
            mucRoiXa = Mathf.Max(mucRoiXa, MucLonNhat(TiengViTri.Loai.ThienThachRoi));
            mucLuaXa = Mathf.Max(mucLuaXa, MucLonNhat(TiengViTri.Loai.LuaChay));
            yield return null;
            AudioListener.GetOutputData(bl, 0);
            float a = 0; foreach (var s in bl) a += s * s;
            rmsXa = Mathf.Max(rmsXa, Mathf.Sqrt(a / bl.Length)); khung++;
        }
        Ghi(string.Format("E. thien thach o 40 m (suong): muc roi lon nhat {0:F3}, muc lua lon nhat {1:F3}, tieng no bi suong che +{2}; RMS loa lon nhat {3:F5} ({4} khung)",
            mucRoiXa, mucLuaXa, TiengViTri.SoBiSuongChe - che0, rmsXa, khung));
        Kiem(mucRoiXa <= 0.001f && mucLuaXa <= 0.001f && TiengViTri.SoBiSuongChe - che0 == 1 && rmsXa < 1e-4f, "thien thach trong suong van nghe thay");
        foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);
        yield return ChoIm(3f);
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        int dc0 = Dem(TiengViTri.Loai.ThienThachNo), che1 = TiengViTri.SoBiSuongChe;
        var tdc = ThienThach.Spawn(xa, 0, 0);
        tdc.chayBanKinh = 0f; tdc.chayThoiGian = 0f;
        int noDc = 0;
        float hanDc = Time.time + 2.5f;
        while (Time.time < hanDc) { noDc = Mathf.Max(noDc, Dem(TiengViTri.Loai.ThienThachNo)); yield return null; }
        Ghi(string.Format("E2. DOI CHUNG tat suong: thien thach o 40 m -> giong no {0}, bi suong che +{1}", noDc - dc0, TiengViTri.SoBiSuongChe - che1));
        Kiem(noDc - dc0 == 1 && TiengViTri.SoBiSuongChe - che1 == 0, "doi chung: tat suong ma van bi bo (phep do khong phan biet)");
        TamNhin.BatTrongPhepThu = true;
        yield return ChoIm(4f);

        // ===== G. bien the + cao do =====
        Ghi("");
        var clipRoi = new HashSet<string>(); var clipNo = new HashSet<string>();
        float caoMin = 9f, caoMax = 0f;
        var vat = new GameObject("TAM_ThienThachGia").transform;
        for (int i = 0; i < 30; i++)
        {
            vat.position = TrenDat(goc + h * 5f) + Vector3.up * 6f;
            var gr = TiengViTri.ThienThachRoi(vat);
            if (gr != null && gr.nguon.clip != null) { clipRoi.Add(gr.nguon.clip.name); caoMin = Mathf.Min(caoMin, gr.nguon.pitch); caoMax = Mathf.Max(caoMax, gr.nguon.pitch); }
            TiengViTri.Dung(gr, vat);
            TiengViTri.ThienThachNo(TrenDat(goc + h * (4f + (i % 3) * 3f)));
            var gn = TiengViTri.GiongMoiNhat;
            if (gn != null && gn.loai == TiengViTri.Loai.ThienThachNo && gn.nguon.clip != null) { clipNo.Add(gn.nguon.clip.name); caoMin = Mathf.Min(caoMin, gn.nguon.pitch); caoMax = Mathf.Max(caoMax, gn.nguon.pitch); }
            yield return new WaitForSeconds(0.15f);
        }
        Object.Destroy(vat.gameObject);
        Ghi(string.Format("G. 30 lan roi + 30 lan no: bien the roi {0} ({1}), no {2} ({3}); cao do {4:F3} - {5:F3}", clipRoi.Count,
            string.Join(", ", clipRoi), clipNo.Count, string.Join(", ", clipNo), caoMin, caoMax));
        Kiem(clipRoi.Count == 3 && clipNo.Count == 3, "khong du 3 bien the roi / no");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f,
             "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(5f);
        Ket();
    }

    static float ThienThachLuaGiay(List<ThienThach> tt)
    {
        foreach (var x in tt) if (x != null) return x.chayThoiGian;
        var v = Object.FindAnyObjectByType<VungLua>();
        return v != null ? v.duration : 5f;
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengthienthach.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
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
        EditorUtility.audioMasterMute = tatTiengGoc;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        Debug.Log("[TiengTT] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
