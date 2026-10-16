using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 127 - TIENG QUA CAU BANG + BANG VO + MUA BANG (16/10/2026, nguoi dung: ban · bay · va cham + no; khoi bang tren dat vo - het gio moi cap,
/// cap 5 no tung, ke bi dong bang chet vo tan; Mua bang: qua roi + cham dat; nguoi choi khac trong suong thi khong nghe). Mau "Mua da" lam DAY
/// + "trong" (ban 4, CongCu/AmThanh/tieng_qua_cau_bang_4.py --trong). Play Act2 that, tat GameDirector + xoa quai, BAT tam nhin + tieng.
///   A. (ngoai Play) 19 file trung tung byte ban goc QuaCauBangThu4/B_mua_da_day_trong_*.wav, STEREO, khong ForceToMono; tieng bay 3 s.
///   B. TUNG THAT Qua cau bang (CastAt 9) vao hop ~9 m: 1 tieng ban (ca chum), moi qua 1 tieng bay, trung hop -> bay tat, moi qua 1 tieng no
///      (qua sau x0,45); loa co tieng, hai kenh khac nhau. Khoi bang tren dat: moi khoi 1 tieng VO dung luc cum gai dau tien tan (TiengTangBang.Moc).
///   C. CAP 5 (SpawnChum capKyNang 5): khoi bang no tung -> tieng BangVoNo moi khoi, KHONG keu tieng vo het gio.
///   D. MUA BANG THAT (CastAt 1) quanh mot bia: moi qua 1 yeu cau tieng roi (giong roi cung luc <= 3), moi qua cham dat 1 tieng cham (<= 6 cung luc);
///      ke bi dong bang -> khoi bang -> tieng vo.
///   E. KE BI DONG BANG CHET: FrozenEffect day du roi Die() -> 1 tieng vo tan (cao do x0,88).
///   F. SUONG MU: no / vo o 40 m -> bo (SoBiSuongChe), loa im; doi chung tat suong -> co giong.
///   G. BIEN THE: 30 lan moi loai (ban, no, vo, vo no, roi, cham) -> du 3 bien the, cao do +-6%.
///   H. QUA NAY: khong keu tieng ban, co tieng bay.
/// Ket qua: PlayTestShots/tiengquacaubang.txt.
/// </summary>
public static class ThuTiengQuaCauBang
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengQuaCauBang";
    const string Goc = "CongCu/AmThanh/QuaCauBangThu4/B_mua_da_day_trong_";

    [MenuItem("Diablo 2.5D/127. Chay thu TIENG QUA CAU BANG (ban · bay · no · bang vo · Mua bang, suong mu)", false, 216)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengBang] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengBang] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng qua cau bang + bang vo + mua bang (mau 'Mua da' day + trong, 3 bien the)");
        var ds = new List<KeyValuePair<string, string>>();
        foreach (var kv in new[] { new[] { "CauBangBan", "ban" }, new[] { "CauBangNo", "no" }, new[] { "BangVo", "vo" }, new[] { "BangVoNo", "vono" },
                                   new[] { "MuaBangRoi", "muaroi" }, new[] { "MuaBangCham", "muacham" } })
            for (int i = 1; i <= 3; i++) ds.Add(new KeyValuePair<string, string>(kv[0] + i, kv[1] + i));
        ds.Add(new KeyValuePair<string, string>("CauBangBay", "bay"));
        int dung = 0;
        foreach (var kv in ds)
        {
            string game = "Assets/Resources/AmThanh/KyNang/" + kv.Key + ".wav";
            string g = Goc + kv.Value + ".wav";
            var imp = AssetImporter.GetAtPath(game) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
            bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
            bool ok = trung && c.channels == 2 && imp != null && !imp.forceToMono;
            if (ok) dung++;
            else Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh; ForceToMono {3}", kv.Key, trung, c != null ? c.channels : 0, imp != null && imp.forceToMono));
            if (kv.Key == "CauBangBay") Kiem(c != null && Mathf.Abs(c.length - 3f) < 0.001f, "tieng bay khong dai dung 3 s (vong lap)");
        }
        Ghi(string.Format("A. {0}/{1} file trung ban goc, 2 kenh, khong ForceToMono", dung, ds.Count));
        Kiem(dung == ds.Count, "co file khong dung ban goc / khong STEREO");
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengBang] " + s); }
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

    static int SL(TiengViTri.Loai l) { return TiengViTri.SoLan[(int)l]; }

    static int Dem(TiengViTri.Loai l)
    {
        int n = 0;
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l && !g.dung) n++;
        return n;
    }

    static List<float> HeSo(TiengViTri.Loai l)
    {
        var ds = new List<float>();
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l) ds.Add(g.heSo);
        ds.Sort(); ds.Reverse();
        return ds;
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

    /// <summary>Nghe loa: kq = {RMS TB, nang luong trai, nang luong phai, tuong quan, RMS dinh}.</summary>
    static IEnumerator Nghe(float giay, float[] kq)
    {
        var bl = new float[1024]; var br = new float[1024];
        double sl = 0, sr = 0, slr = 0, tong = 0; int so = 0; float dinh = 0;
        float han = Time.realtimeSinceStartup + giay;
        while (Time.realtimeSinceStartup < han)
        {
            yield return null;
            AudioListener.GetOutputData(bl, 0);
            AudioListener.GetOutputData(br, 1);
            double a = 0, b = 0, ab = 0;
            for (int i = 0; i < bl.Length; i++) { a += bl[i] * bl[i]; b += br[i] * br[i]; ab += bl[i] * br[i]; }
            sl += a; sr += b; slr += ab;
            float rms = (float)System.Math.Sqrt((a + b) / (2.0 * bl.Length));
            tong += rms; so++; dinh = Mathf.Max(dinh, rms);
        }
        kq[0] = so > 0 ? (float)(tong / so) : 0f;
        kq[1] = (float)sl; kq[2] = (float)sr;
        kq[3] = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        kq[4] = dinh;
    }

    static Vector3 TrenDat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f) + 1f; return p; }

    static Vector3 HuongTrong(Vector3 goc, PlayerController pc, float dai, float rong)
    {
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc + Vector3.up * Mathf.Max(1f, rong + 0.3f), rong, h, out RaycastHit _, dai, pc.MatNaVatCan & ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                return h;
        }
        return Vector3.forward;
    }

    static Damageable TaoBia(string ten, Vector3 p)
    {
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = d.health = 1e7f;
        d.tiLeDoDon = 0f;
        return d;
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
        var cam = Camera.main.transform;
        Vector3 truoc = cam.forward; truoc.y = 0; truoc.Normalize();
        Vector3 goc = pc.transform.position;
        int maskEnemy = LayerMask.GetMask("Enemy");
        float[] kq = new float[5];

        // Khoi dong: qua cau bang dau tien (hinh pha le, shader) Editor khung giat - ban mot qua bo di
        var kd = QuaCauBang.Spawn(goc + Vector3.up * 40f, Vector3.up, 0, 0); kd.lifetime = 0.3f;
        yield return new WaitForSeconds(1.0f);
        yield return ChoIm(5f);

        // ===== B. tung that =====
        Ghi("");
        Vector3 h = HuongTrong(goc, pc, 8.5f, 2.0f);
        pc.transform.rotation = Quaternion.LookRotation(h);
        var hop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hop.name = "TAM_HopTiengBang";
        hop.transform.position = TrenDat(goc + h * 9f);
        hop.transform.localScale = new Vector3(7f, 5f, 1f);
        hop.transform.rotation = Quaternion.LookRotation(h);
        Physics.SyncTransforms();
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(CapDo.KyQuaCauBang);
        pc.mana = pc.maxMana;
        int b0 = SL(TiengViTri.Loai.CauBangBan), y0 = SL(TiengViTri.Loai.CauBangBay), n0 = SL(TiengViTri.Loai.CauBangNo), v0 = SL(TiengViTri.Loai.BangVo);
        int tv0 = TiengTangBang.SoLanVo;
        float lucTung = Time.time;
        pc.CastAt(CapDo.KyQuaCauBang, goc + h * 9f);
        var qua = new List<QuaCauBang>();
        float hanB = Time.time + 1.5f;
        while (qua.Count == 0 && Time.time < hanB) { qua.AddRange(Object.FindObjectsByType<QuaCauBang>(FindObjectsSortMode.None)); yield return null; }
        yield return new WaitForSeconds(0.06f);
        int banB = Dem(TiengViTri.Loai.CauBangBan), bayB = Dem(TiengViTri.Loai.CauBangBay);
        float[] kqBay = new float[5];
        yield return Nghe(0.12f, kqBay);
        float hanNo = Time.time + 2.5f, lucNo = -1f;
        while (Time.time < hanNo)
        {
            bool con = false; foreach (var q in qua) if (q != null) con = true;
            if (!con) { lucNo = Time.time; break; }
            yield return null;
        }
        yield return new WaitForSeconds(0.15f);
        int bayConB = Dem(TiengViTri.Loai.CauBangBay), noB = Dem(TiengViTri.Loai.CauBangNo);
        var heSoNo = HeSo(TiengViTri.Loai.CauBangNo);
        float[] kqNo = new float[5];
        yield return Nghe(0.8f, kqNo);
        // khoi bang tren dat: cho tieng vo (TiengTangBang.Moc ~1,6 s), ghi luc tung tieng vo
        var tangs = new List<TiengTangBang>(Object.FindObjectsByType<TiengTangBang>(FindObjectsSortMode.None));
        float mocMin = 9f, mocMax = 0f; foreach (var tg in tangs) if (tg.Moc > 0) { mocMin = Mathf.Min(mocMin, tg.Moc); mocMax = Mathf.Max(mocMax, tg.Moc); }
        var lucVo = new List<float>(); int voTruoc = SL(TiengViTri.Loai.BangVo);
        float hanV = Time.time + 3.5f;
        while (Time.time < hanV)
        {
            int v = SL(TiengViTri.Loai.BangVo);
            for (int k = voTruoc; k < v; k++) lucVo.Add(Time.time - lucNo);
            voTruoc = v;
            yield return null;
        }
        Ghi(string.Format("B. tung that: {0} qua; yeu cau ban/bay/no +{1}/+{2}/+{3}; giong luc bay: ban {4}, bay {5}; no xong luc {6:F2} s; 0,15 s sau: bay con {7}, no {8} (he so {9})",
            qua.Count, SL(TiengViTri.Loai.CauBangBan) - b0, SL(TiengViTri.Loai.CauBangBay) - y0, SL(TiengViTri.Loai.CauBangNo) - n0, banB, bayB,
            lucNo - lucTung, bayConB, noB, string.Join(" / ", heSoNo.ConvertAll(x => x.ToString("F2")))));
        Ghi(string.Format("   loa: luc bay RMS {0:F4}, luc no RMS {1:F4} (dinh {2:F4}), tuong quan hai kenh {3:F3} / {4:F3}", kqBay[0], kqNo[0], kqNo[4], kqBay[3], kqNo[3]));
        Ghi(string.Format("   khoi bang: {0} khoi (co TiengTangBang), moc cum gai dau tan {1:F2}-{2:F2} s; tieng vo +{3} (TiengTangBang {4}) luc {5} s sau vu no",
            tangs.Count, mocMin, mocMax, SL(TiengViTri.Loai.BangVo) - v0, TiengTangBang.SoLanVo - tv0, string.Join(" / ", lucVo.ConvertAll(x => x.ToString("F2")))));
        Kiem(qua.Count >= 4 && banB == 1 && bayB == qua.Count, "tung Qua cau bang khong ra 1 tieng ban + moi qua 1 tieng bay");
        Kiem(bayConB == 0 && noB == qua.Count && heSoNo.Count == qua.Count && Mathf.Approximately(heSoNo[0], 1f) && heSoNo[heSoNo.Count - 1] < 0.5f,
             "trung hop: tieng bay khong tat / khong du tieng no (qua sau nho lai)");
        Kiem(kqNo[0] > 1e-3f && kqNo[3] < 0.95f, "loa khong co tieng no / tieng no o loa gan mono");
        Kiem(tangs.Count == qua.Count && lucVo.Count == qua.Count, "khong du tieng vo cho moi khoi bang");
        bool dungMoc = lucVo.Count > 0;
        foreach (var lv in lucVo) if (lv < mocMin - 0.25f || lv > mocMax + 0.25f) dungMoc = false;
        Kiem(dungMoc, "tieng vo khong keu dung luc cum gai dau tien tan");
        Object.Destroy(hop);
        yield return ChoIm(4f);

        // ===== C. cap 5: khoi bang no tung =====
        Ghi("");
        var hop5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hop5.name = "TAM_HopTiengBang5";
        hop5.transform.position = TrenDat(goc + h * 9f);
        hop5.transform.localScale = new Vector3(7f, 5f, 1f);
        hop5.transform.rotation = Quaternion.LookRotation(h);
        Physics.SyncTransforms();
        int vn0 = SL(TiengViTri.Loai.BangVoNo), vc0 = SL(TiengViTri.Loai.BangVo), bq0 = TiengTangBang.SoLanBoQuaCap5, tn0 = TangBangNo.SoLanNo;
        QuaCauBang.SpawnChum(goc + Vector3.up * 1.2f + h, h, pc.MatNaVatCan, maskEnemy, toi, 2, 11f, 1f, 0f, 5);
        float hanC = Time.time + 5f;
        while (Time.time < hanC) yield return null;
        Ghi(string.Format("C. cap 5 (2 qua): khoi bang no tung {0}; tieng vo no +{1}; tieng vo het gio +{2}; TiengTangBang bo qua vi cap 5 +{3}",
            TangBangNo.SoLanNo - tn0, SL(TiengViTri.Loai.BangVoNo) - vn0, SL(TiengViTri.Loai.BangVo) - vc0, TiengTangBang.SoLanBoQuaCap5 - bq0));
        Kiem(TangBangNo.SoLanNo - tn0 == 2 && SL(TiengViTri.Loai.BangVoNo) - vn0 == 2 && SL(TiengViTri.Loai.BangVo) - vc0 == 0 && TiengTangBang.SoLanBoQuaCap5 - bq0 == 2,
             "cap 5: khong moi khoi mot tieng no tung / van keu tieng vo het gio");
        Object.Destroy(hop5);
        yield return ChoIm(4f);

        // ===== D. Mua bang that =====
        Ghi("");
        var bia = TaoBia("TAM_BiaMuaBang", TrenDat(goc + h * 8f) - Vector3.up);
        Physics.SyncTransforms();
        CapDo.MoCaDuongChoPhepThu(1);
        pc.mana = pc.maxMana;
        int r0 = SL(TiengViTri.Loai.MuaBangRoi), c0 = SL(TiengViTri.Loai.MuaBangCham), vd0 = SL(TiengViTri.Loai.BangVo), cham0 = FallingShard.SoLanCham, gai0 = FallingShard.SoLanCoGai;
        pc.CastAt(1, bia.transform.position);
        int roiMax = 0, chamMax = 0;
        float hanD = Time.time + 9f;
        float[] kqMua = new float[5];
        bool daNghe = false;
        while (Time.time < hanD)
        {
            roiMax = Mathf.Max(roiMax, Dem(TiengViTri.Loai.MuaBangRoi));
            chamMax = Mathf.Max(chamMax, Dem(TiengViTri.Loai.MuaBangCham));
            if (!daNghe && Time.time > hanD - 7f) { daNghe = true; yield return Nghe(0.6f, kqMua); continue; }
            yield return null;
        }
        int soCham = FallingShard.SoLanCham - cham0, soGai = FallingShard.SoLanCoGai - gai0;
        Ghi(string.Format("D. Mua bang that: {0} qua cham dat, yeu cau roi +{1}, cham +{2}; giong cung luc lon nhat roi {3} (tran {4}), cham {5} (tran {6}); khoi bang (dong bang) {7} -> tieng vo +{8}",
            soCham, SL(TiengViTri.Loai.MuaBangRoi) - r0, SL(TiengViTri.Loai.MuaBangCham) - c0, roiMax, TiengViTri.SoMuaRoiToiDa, chamMax, TiengViTri.SoMuaChamToiDa,
            soGai, SL(TiengViTri.Loai.BangVo) - vd0));
        Ghi(string.Format("   loa giua tran mua: RMS {0:F4}, tuong quan hai kenh {1:F3}", kqMua[0], kqMua[3]));
        Kiem(soCham > 20 && SL(TiengViTri.Loai.MuaBangRoi) - r0 == soCham && SL(TiengViTri.Loai.MuaBangCham) - c0 == soCham, "Mua bang: khong moi qua 1 tieng roi + 1 tieng cham");
        Kiem(roiMax >= 1 && roiMax <= TiengViTri.SoMuaRoiToiDa && chamMax >= 1 && chamMax <= TiengViTri.SoMuaChamToiDa, "Mua bang vuot tran giong / khong keu");
        Kiem(soGai == SL(TiengViTri.Loai.BangVo) - vd0, "khoi bang cua Mua bang khong keu tieng vo");
        Kiem(kqMua[0] > 1e-3f && kqMua[3] < 0.95f, "loa khong co tieng mua / gan mono");
        Object.Destroy(bia.gameObject);
        yield return ChoIm(5f);

        // ===== E. ke bi dong bang chet vo tan =====
        Ghi("");
        var biaE = TaoBia("TAM_BiaVoTan", TrenDat(goc + h * 5f) - Vector3.up);
        FrozenEffect.Apply(biaE, 3f);
        yield return null;
        bool dongCung = biaE.GetComponent<FrozenEffect>() != null && biaE.GetComponent<FrozenEffect>().IsFullyFrozen;
        int ve0 = SL(TiengViTri.Loai.BangVo);
        biaE.Die();
        yield return null;
        float caoVoTan = -1f;
        var gm = TiengViTri.GiongMoiNhat;
        if (gm != null && gm.loai == TiengViTri.Loai.BangVo && gm.nguon != null) caoVoTan = gm.nguon.pitch;
        Ghi(string.Format("E. bia dong bang hoan toan {0} chet -> tieng vo +{1}, cao do {2:F3} (x{3} +-6%)", dongCung, SL(TiengViTri.Loai.BangVo) - ve0, caoVoTan, TiengViTri.CaoDoVoTan));
        Kiem(dongCung && SL(TiengViTri.Loai.BangVo) - ve0 == 1 && caoVoTan > 0f && caoVoTan < 0.94f, "ke bi dong bang chet khong keu tieng vo tan");
        yield return ChoIm(4f);

        // ===== F. suong mu =====
        Ghi("");
        Vector3 xa = TrenDat(goc + truoc * 40f);
        int che0 = TiengViTri.SoBiSuongChe;
        TiengViTri.CauBangNo(xa); TiengViTri.BangVo(xa); TiengViTri.MuaBangCham(xa);
        yield return null;
        int giongXa = 0; foreach (var g in TiengViTri.CacGiong) if (!g.Ranh) giongXa++;
        yield return Nghe(0.6f, kq);
        float rmsXa = kq[0];
        Ghi(string.Format("F. no / vo / cham o 40 m (MucThay {0:F2}): bi suong che +{1}, giong {2}, RMS loa {3:F5}", TamNhin.MucThay(xa), TiengViTri.SoBiSuongChe - che0, giongXa, rmsXa));
        Kiem(TiengViTri.SoBiSuongChe - che0 == 3 && giongXa == 0 && rmsXa < 1e-4f, "tieng bang trong suong van nghe thay");
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        int che1 = TiengViTri.SoBiSuongChe;
        TiengViTri.CauBangNo(xa);
        yield return null;
        int giongDc = Dem(TiengViTri.Loai.CauBangNo);
        Ghi(string.Format("F2. DOI CHUNG tat suong: no o 40 m -> giong {0}, bi suong che +{1}", giongDc, TiengViTri.SoBiSuongChe - che1));
        Kiem(giongDc == 1 && TiengViTri.SoBiSuongChe - che1 == 0, "doi chung tat suong ma van bi bo");
        TamNhin.BatTrongPhepThu = true;
        yield return ChoIm(4f);

        // ===== G. bien the + cao do =====
        Ghi("");
        var loai = new[] { TiengViTri.Loai.CauBangBan, TiengViTri.Loai.CauBangNo, TiengViTri.Loai.BangVo, TiengViTri.Loai.BangVoNo, TiengViTri.Loai.MuaBangRoi, TiengViTri.Loai.MuaBangCham };
        var clipLoai = new Dictionary<TiengViTri.Loai, HashSet<string>>();
        foreach (var l in loai) clipLoai[l] = new HashSet<string>();
        float caoMin = 9f, caoMax = 0f;
        var vat = new GameObject("TAM_VatRoiGia").transform;
        // Moi loai rieng, cach 0,3 s; chi dem lan Phat DUOC GIONG (lan chay 1: goi 6 loai x 0,2 s day tran giong -> nhieu lan bi tu choi,
        // GiongMoiNhat la giong cu -> dem thieu bien the cham dat 2/3)
        var soDuoc = new Dictionary<TiengViTri.Loai, int>();
        for (int j = 0; j < loai.Length; j++)
        {
            soDuoc[loai[j]] = 0;
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = TrenDat(goc + truoc * (3f + (i % 4) * 4f) + Vector3.Cross(Vector3.up, truoc) * ((i % 5) - 2f) * 3f);
                int truocPhat = TiengViTri.SoLanDuocGiong;
                switch (loai[j])
                {
                    case TiengViTri.Loai.CauBangBan: TiengViTri.CauBangBan(p); break;
                    case TiengViTri.Loai.CauBangNo: TiengViTri.CauBangNo(p); break;
                    case TiengViTri.Loai.BangVo: TiengViTri.BangVo(p); break;
                    case TiengViTri.Loai.BangVoNo: TiengViTri.BangVoNo(p); break;
                    case TiengViTri.Loai.MuaBangRoi: { vat.position = p + Vector3.up * 5f; var g = TiengViTri.MuaBangRoi(vat); TiengViTri.Dung(g, vat); break; }
                    case TiengViTri.Loai.MuaBangCham: TiengViTri.MuaBangCham(p); break;
                }
                var gm2 = TiengViTri.GiongMoiNhat;
                if (TiengViTri.SoLanDuocGiong > truocPhat && gm2 != null && gm2.loai == loai[j] && gm2.nguon != null && gm2.nguon.clip != null)
                {
                    soDuoc[loai[j]]++;
                    clipLoai[loai[j]].Add(gm2.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm2.nguon.pitch); caoMax = Mathf.Max(caoMax, gm2.nguon.pitch);
                }
                yield return new WaitForSeconds(0.3f);
            }
            yield return ChoIm(5f);
        }
        Object.Destroy(vat.gameObject);
        var dongG = new List<string>(); bool du = true;
        foreach (var l in loai) { dongG.Add(l + " " + clipLoai[l].Count + " (" + soDuoc[l] + " lan duoc giong)"); if (clipLoai[l].Count != 3) du = false; }
        Ghi(string.Format("G. 30 lan moi loai: so bien the {0}; cao do {1:F3} - {2:F3}", string.Join(", ", dongG), caoMin, caoMax));
        Kiem(du, "khong du 3 bien the moi loai");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(5f);

        // ===== H. qua nay =====
        Ghi("");
        int hb0 = SL(TiengViTri.Loai.CauBangBan), hy0 = SL(TiengViTri.Loai.CauBangBay);
        var nay = QuaCauBang.Spawn(TrenDat(goc + truoc * 3f), truoc, 0, 0); nay.laQuaNay = true; nay.lifetime = 0.6f;
        yield return null; yield return null;
        Ghi(string.Format("H. qua nay: yeu cau ban +{0}, bay +{1}", SL(TiengViTri.Loai.CauBangBan) - hb0, SL(TiengViTri.Loai.CauBangBay) - hy0));
        Kiem(SL(TiengViTri.Loai.CauBangBan) - hb0 == 0 && SL(TiengViTri.Loai.CauBangBay) - hy0 == 1, "qua nay van keu tieng ban / khong co tieng bay");
        yield return new WaitForSeconds(1f);
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengquacaubang.txt", bao.ToString());
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
        Debug.Log("[TiengBang] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
