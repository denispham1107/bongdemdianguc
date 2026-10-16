using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 125 - TIENG QUA CAU LUA (15/10/2026, nguoi dung: mau C "Lửa thật"; ban · bay · no; nguoi choi khac trong suong thi khong nghe).
/// Play Act2 that, tat GameDirector + xoa quai, BAT phan hinh tam nhin (TamNhin.BatTrongPhepThu) va tieng (NhacNen.BatTrongPhepThu),
/// tat tieng nhac nen. Do TIENG THAT o loa bang AudioListener.GetOutputData.
///   A. (ngoai Play) bay file (ban 1-3, bay, no 1-3) trung tung byte ban goc CongCu/AmThanh/QuaCauLuaThu3/B_khoi_lua_lon_*.wav (ban 3,
///      15/10/2026 - nguoi dung: ban truoc "con qua mono", "chua giong lua that, don dieu"), STEREO, khong ForceToMono; tieng bay 3 s.
///   B. TUNG THAT (CastAt 0) vao hop o ~9 m: 1 giong ban (ca chum), 3 giong bay, trung hop -> bay tat, 3 giong no (qua 2, 3 nho lai);
///      loa co tieng, hai kenh khac nhau.
///   C. LECH TRAI / PHAI: no o ben phai / trai man hinh (8 m theo truc phai camera) -> nang luong kenh phai / trai lon hon; doi chung giua.
///   C2. DO RONG (Spread 90 / 120 / 150 / 180): tuong quan hai kenh o loa khi no giua + ti le phai / trai khi no hai ben - chon muc
///      TiengViTri.DoRongMacDinh: giu stereo rong ma van nghe ro ben. Phat CUNG mot bien the (no 1, cao do 1) cho moi muc.
///   G. BIEN THE: 30 tieng ban + 30 tieng no -> du 3 bien the moi loai, cao do trong +-6%.
///   D. XA NHO: no o 2 / 14 / 20 m truoc mat -> muc dat va RMS loa giam dung duong cong TiengViTri.HeSoKhoangCach.
///   E. SUONG MU: no o 30 m -> bo (SoBiSuongChe +1), loa im; DOI CHUNG tat suong (TamNhin.BatTrongPhepThu = false) -> co giong.
///      Qua cua "nguoi khac" bay tu 40 m vao: tieng ban bi bo, tieng bay im ngoai tam nhin, bat dau nghe khi vao tam.
///      Qua cua minh bay ra xa: tieng bay tat khi qua 25 m, no ngoai tam thi im.
///   F. QUA NAY khong keu tieng ban; Lua dia nguc (5 qua) 1 tieng ban + 5 tieng bay; qua quai (Fireball.Spawn) 1 + 1; tran 6 tieng bay;
///      qua bi xoa khong qua Explode -> tieng bay tat; thanh Hiệu ứng 0 -> moi giong am luong 0.
/// Ket qua: PlayTestShots/tiengquacaulua.txt.
/// </summary>
public static class ThuTiengQuaCauLua
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengQuaCauLua";

    [MenuItem("Diablo 2.5D/125. Chay thu TIENG QUA CAU LUA (ban · bay · no, suong mu)", false, 214)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengLua] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengLua] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 2] tieng qua cau lua (ban 3 'Khoi lua lon', 3 bien the)");
        string[] ten = { "Ban1", "Ban2", "Ban3", "Bay", "No1", "No2", "No3" }, goc = { "ban1", "ban2", "ban3", "bay", "no1", "no2", "no3" };
        for (int i = 0; i < ten.Length; i++)
        {
            string game = "Assets/Resources/AmThanh/KyNang/QuaCauLua" + ten[i] + ".wav";
            string g = "CongCu/AmThanh/QuaCauLuaThu3/B_khoi_lua_lon_" + goc[i] + ".wav";
            var imp = AssetImporter.GetAtPath(game) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
            bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
            Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh {3:F3} s; ForceToMono {4}; nap {5}, Vorbis {6:F2}", Path.GetFileName(game), trung,
                c != null ? c.channels : 0, c != null ? c.length : 0, imp != null && imp.forceToMono,
                imp != null ? imp.defaultSampleSettings.loadType.ToString() : "-", imp != null ? imp.defaultSampleSettings.quality : 0f));
            Kiem(trung && c.channels == 2 && imp != null && !imp.forceToMono, "file " + ten[i] + " khong dung ban goc / khong STEREO");
            if (ten[i] == "Bay") Kiem(c != null && Mathf.Abs(c.length - 3f) < 0.001f, "tieng bay khong dai dung 3 s (vong lap)");
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengLua] " + s); }
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

    static List<float> HeSoNo()
    {
        var ds = new List<float>();
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.QuaCauLuaNo) ds.Add(g.heSo);
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

    /// <summary>Nghe loa trong <paramref name="giay"/> s: kq = {RMS TB, nang luong trai, nang luong phai, tuong quan hai kenh, RMS dinh,
    /// khung dai nhat (s) - Editor khung giat thi bo dem GetOutputData bo sot tieng, so TB lech}.</summary>
    static IEnumerator Nghe(float giay, float[] kq)
    {
        var bl = new float[1024]; var br = new float[1024];
        double sl = 0, sr = 0, slr = 0, tong = 0; int so = 0; float dinh = 0;
        float han = Time.realtimeSinceStartup + giay;
        float truoc = Time.realtimeSinceStartup, khungDai = 0f;
        while (Time.realtimeSinceStartup < han)
        {
            yield return null;
            khungDai = Mathf.Max(khungDai, Time.realtimeSinceStartup - truoc);
            truoc = Time.realtimeSinceStartup;
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
        kq[5] = khungDai;
    }

    static Vector3 TrenDat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f) + 1f; return p; }

    /// <param name="rong">Ban kinh ong quet - muc B dung 1,9 m de trong CA CHUM (3 qua lech 11 do, o 9 m cach truc 1,7 m): ong 0,6 m
    /// tung de qua ben dam vao bia o 5,6 m truoc hai qua kia 0,12 s - hai vu no that su tach roi.</param>
    static Vector3 HuongTrong(Vector3 goc, PlayerController pc, float dai, float rong = 0.6f)
    {
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc + Vector3.up * Mathf.Max(1f, rong + 0.3f), rong, h, out RaycastHit _, dai, pc.MatNaVatCan & ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
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
        if (rig != null) rig.enabled = false;           // camera (AudioListener) dung yen suot phep do
        var cam = Camera.main.transform;
        Vector3 phai = cam.right; phai.y = 0; phai.Normalize();
        Vector3 truoc = cam.forward; truoc.y = 0; truoc.Normalize();
        Vector3 goc = pc.transform.position;
        float[] kq = new float[6];
        Ghi("TamNhin.Bat " + TamNhin.Bat + "; giong toi da " + TiengViTri.SoGiongToiDa + "; nghe nen (im) ...");
        yield return Nghe(0.6f, kq);
        float nen = kq[0];
        Ghi(string.Format("   RMS nen {0:F5}", nen));

        // ===== B. tung that vao hop =====
        // Khoi dong: qua cau lua DAU TIEN sau khi nap lai Editor khung giat rat lau (bien dich shader hieu ung) - lan 1 cua phep thu do
        // ngay khung giat ay: qua da toi hop, tieng ban da phat het theo dong ho that. Ban mot qua bo di truoc.
        var khoiDong = Fireball.Spawn(goc + Vector3.up * 40f, Vector3.up, 0, 0); khoiDong.lifetime = 0.3f;
        yield return new WaitForSeconds(1.0f);
        yield return ChoIm(4f);
        Ghi("");
        Vector3 h = HuongTrong(goc, pc, 8.5f, 1.9f);
        pc.transform.rotation = Quaternion.LookRotation(h);
        var hop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hop.name = "TAM_HopTiengLua";
        hop.transform.position = TrenDat(goc + h * 9f);
        hop.transform.localScale = new Vector3(6f, 4f, 1f);
        hop.transform.rotation = Quaternion.LookRotation(h);
        Physics.SyncTransforms();
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(0);
        pc.mana = pc.maxMana;
        int b0 = TiengViTri.SoLan[0], y0 = TiengViTri.SoLan[1], n0 = TiengViTri.SoLan[2];
        pc.CastAt(0, goc + h * 9f);
        var qua = new List<Fireball>();
        float hanB = Time.time + 1.5f;
        while (qua.Count == 0 && Time.time < hanB) { qua.AddRange(Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)); yield return null; }
        yield return new WaitForSeconds(0.08f);
        int banB = Dem(TiengViTri.Loai.QuaCauLuaBan), bayB = Dem(TiengViTri.Loai.QuaCauLuaBay);
        float mucBay = 0; foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.QuaCauLuaBay) mucBay = Mathf.Max(mucBay, g.muc);
        // nghe tu luc bay toi khi no xong
        float lucBatDau = Time.time;
        float[] kqBay = new float[6];
        yield return Nghe(0.15f, kqBay);
        float hanNo = Time.time + 2f;
        // cho + luc no cua tung qua (vi tri cuoi cung thay duoc) - de doc vi sao he so no trung khong nhu mong
        var viTriCuoi = new Dictionary<Fireball, Vector3>();
        var choNo = new List<string>();
        while (Time.time < hanNo)
        {
            bool con = false;
            foreach (var f in qua)
            {
                if (f != null) { viTriCuoi[f] = f.transform.position; con = true; }
                else if (viTriCuoi.ContainsKey(f))
                {
                    var p = viTriCuoi[f];
                    choNo.Add(string.Format("({0:F1}; {1:F1}; {2:F1}) luc {3:F3}", p.x - goc.x, p.y - goc.y, p.z - goc.z, Time.time - lucBatDau));
                    viTriCuoi.Remove(f);
                }
            }
            if (!con && viTriCuoi.Count == 0) break;
            yield return null;
        }
        float lucNo = Time.time - lucBatDau;
        yield return new WaitForSeconds(0.2f);
        int bayConB = Dem(TiengViTri.Loai.QuaCauLuaBay), noB = Dem(TiengViTri.Loai.QuaCauLuaNo);
        var heSoNo = HeSoNo();
        float[] kqNo = new float[6];
        yield return Nghe(1.0f, kqNo);
        Ghi(string.Format("B. tung that: {0} qua; yeu cau ban/bay/no +{1}/+{2}/+{3}; giong luc bay: ban {4}, bay {5} (muc {6:F2}); no sau {7:F2} s; 0,2 s sau: bay con {8}, no {9} (he so {10})",
            qua.Count, TiengViTri.SoLan[0] - b0, TiengViTri.SoLan[1] - y0, TiengViTri.SoLan[2] - n0, banB, bayB, mucBay, lucNo, bayConB, noB,
            string.Join(" / ", heSoNo.ConvertAll(x => x.ToString("F2")))));
        Ghi(string.Format("   loa: luc bay RMS {0:F4}, luc no RMS {1:F4} (dinh {2:F4}), tuong quan hai kenh {3:F3} / {4:F3}",
            kqBay[0], kqNo[0], kqNo[4], kqBay[3], kqNo[3]));
        Ghi("   cho no tung qua (so voi nhan vat) + luc no: " + string.Join(" | ", choNo));
        Kiem(qua.Count == 3 && banB == 1 && bayB == 3, "tung Qua cau lua khong ra 1 tieng ban + 3 tieng bay");
        Kiem(bayConB == 0 && noB == 3 && heSoNo.Count == 3 && Mathf.Approximately(heSoNo[0], 1f) && heSoNo[1] < 0.5f, "trung hop: tieng bay khong tat / khong ra 3 tieng no (qua sau nho lai)");
        Kiem(kqBay[0] > nen * 5f + 1e-4f && kqNo[0] > nen * 5f + 1e-4f, "loa khong co tieng khi tung / no");
        Kiem(kqNo[3] < 0.99f, "tieng no o loa khong stereo");
        Object.Destroy(hop);
        yield return ChoIm(4f);

        // ===== C. lech trai / phai =====
        Ghi("");
        var lr = new float[3];
        string[] tenBen = { "giua", "phai", "trai" };
        Vector3[] cho = { goc + truoc * 6f, goc + truoc * 3f + phai * 8f, goc + truoc * 3f - phai * 8f };
        for (int i = 0; i < 3; i++)
        {
            TiengViTri.QuaCauLuaNo(TrenDat(cho[i]));
            yield return Nghe(1.6f, kq);
            lr[i] = kq[2] / Mathf.Max(kq[1], 1e-9f);
            Ghi(string.Format("C. no ben {0}: nang luong phai / trai = {1:F2}; tuong quan {2:F3}", tenBen[i], lr[i], kq[3]));
            yield return ChoIm(3f);
        }
        Kiem(lr[1] > 1.3f && lr[2] < 1f / 1.3f && lr[1] > lr[0] && lr[2] < lr[0], "tieng khong lech theo ben trai / phai man hinh");

        // ===== C2. do rong (Spread) =====
        Ghi("");
        float[] cacMuc = { 90f, 120f, 150f, 180f };
        var tqMuc = new float[cacMuc.Length]; var phaiMuc = new float[cacMuc.Length]; var traiMuc = new float[cacMuc.Length];
        for (int m = 0; m < cacMuc.Length; m++)
        {
            TiengViTri.DoRong = cacMuc[m];
            for (int i = 0; i < 3; i++)
            {
                TiengViTri.QuaCauLuaNo(TrenDat(cho[i]));
                var gm = TiengViTri.GiongMoiNhat;
                if (gm != null && gm.nguon != null)
                {   // cung mot bien the + cao do cho moi muc - so sanh cong bang
                    gm.nguon.Stop();
                    gm.nguon.clip = Resources.Load<AudioClip>("AmThanh/KyNang/QuaCauLuaNo1");
                    gm.nguon.pitch = 1f;
                    gm.nguon.Play();
                }
                yield return Nghe(1.6f, kq);
                float r = kq[2] / Mathf.Max(kq[1], 1e-9f);
                if (i == 0) tqMuc[m] = kq[3]; else if (i == 1) phaiMuc[m] = r; else traiMuc[m] = r;
                yield return ChoIm(3f);
            }
            Ghi(string.Format("C2. Spread {0}: no giua tuong quan hai kenh {1:F3}; no phai: phai / trai {2:F2}; no trai: phai / trai {3:F2}",
                cacMuc[m], tqMuc[m], phaiMuc[m], traiMuc[m]));
        }
        TiengViTri.DoRong = TiengViTri.DoRongMacDinh;
        int chon = System.Array.IndexOf(cacMuc, TiengViTri.DoRongMacDinh);
        Ghi(string.Format("   muc dang dung (DoRongMacDinh) {0}", TiengViTri.DoRongMacDinh));
        Kiem(chon >= 0 && tqMuc[chon] < 0.6f, "tieng no giua o loa van gan mono (tuong quan >= 0,6) o muc Spread dang dung");
        Kiem(chon >= 0 && phaiMuc[chon] > 1.3f && traiMuc[chon] < 1f / 1.3f, "o muc Spread dang dung khong con nghe ro ben trai / phai");

        // ===== G. bien the + cao do =====
        Ghi("");
        var clipBan = new HashSet<string>(); var clipNo = new HashSet<string>();
        float caoMin = 9f, caoMax = 0f;
        for (int i = 0; i < 30; i++)
        {
            TiengViTri.QuaCauLuaBan(TrenDat(goc + truoc * 3f + phai * (i % 2 == 0 ? 6f : -6f)));
            var gb = TiengViTri.GiongMoiNhat;
            if (gb != null && gb.loai == TiengViTri.Loai.QuaCauLuaBan && gb.nguon.clip != null) { clipBan.Add(gb.nguon.clip.name); caoMin = Mathf.Min(caoMin, gb.nguon.pitch); caoMax = Mathf.Max(caoMax, gb.nguon.pitch); }
            TiengViTri.QuaCauLuaNo(TrenDat(goc + truoc * 3f + phai * (i % 2 == 0 ? -6f : 6f) + truoc * (i % 3) * 5f));
            var gn = TiengViTri.GiongMoiNhat;
            if (gn != null && gn.loai == TiengViTri.Loai.QuaCauLuaNo && gn.nguon.clip != null) { clipNo.Add(gn.nguon.clip.name); caoMin = Mathf.Min(caoMin, gn.nguon.pitch); caoMax = Mathf.Max(caoMax, gn.nguon.pitch); }
            yield return new WaitForSeconds(0.2f);
        }
        Ghi(string.Format("G. 30 lan ban + 30 lan no: bien the ban {0} ({1}), no {2} ({3}); cao do {4:F3} - {5:F3}", clipBan.Count,
            string.Join(", ", clipBan), clipNo.Count, string.Join(", ", clipNo), caoMin, caoMax));
        Kiem(clipBan.Count == 3 && clipNo.Count == 3, "khong du 3 bien the ban / no");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f,
             "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(5f);

        // ===== D. xa nho =====
        Ghi("");
        float[] kc = { 2f, 14f, TamNhin.BanKinh - TamNhin.Mem };    // 16/10/2026: tam nhin 20 m -> 17 m (con nghe du, chua vao mep suong)
        // Moi khoang cach do HAI lan, lay lan lon hon (va ghi ca hai): lan chay 15/10 co mot lan 14 m ra x0,37 (thap hon ca 20 m) - mot
        // lan nghe lech khoi tieng; phep do mot lan khong phan biet duoc voi loi that.
        var rmsD = new float[3]; var mucD = new float[3];
        for (int i = 0; i < 3; i++)
        {
            Vector3 p = TrenDat(goc + truoc * kc[i]);
            var lan = new float[2]; var khung = new float[2];
            for (int k = 0; k < 2; k++)
            {
                TiengViTri.QuaCauLuaNo(p);
                yield return null;
                foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.QuaCauLuaNo) mucD[i] = g.muc;
                yield return Nghe(1.6f, kq);
                lan[k] = kq[0]; khung[k] = kq[5];
                yield return ChoIm(3.5f);
            }
            rmsD[i] = Mathf.Max(lan[0], lan[1]);
            Ghi(string.Format("D. no o {0} m: muc dat {1:F3} (duong cong {2:F3}), RMS loa {3:F4} / {4:F4} (khung dai nhat {5:F2} / {6:F2} s) -> x{7:F2} so voi 2 m", kc[i], mucD[i],
                TiengViTri.HeSoKhoangCach(p), lan[0], lan[1], khung[0], khung[1], rmsD[i] / Mathf.Max(rmsD[0], 1e-9f)));
        }
        Kiem(Mathf.Abs(mucD[0] - 1f) < 0.01f && Mathf.Abs(mucD[1] - TiengViTri.HeSoKhoangCach(TrenDat(goc + truoc * 14f))) < 0.02f && mucD[2] < mucD[1],
             "muc am luong khong theo duong cong khoang cach");
        Kiem(rmsD[1] < rmsD[0] * 0.85f && rmsD[2] < rmsD[1] * 0.9f && rmsD[2] > nen * 3f, "loa khong nho dan theo khoang cach");

        // ===== E. suong mu =====
        Ghi("");
        // cho loa that su im (toi da 3 s) roi moi do
        for (int k = 0; k < 10; k++) { yield return Nghe(0.3f, kq); if (kq[4] < 1e-4f) break; }
        int che0 = TiengViTri.SoBiSuongChe;
        Vector3 xa = TrenDat(goc + truoc * (TamNhin.BanKinh + 10f));
        TiengViTri.QuaCauLuaNo(xa);
        yield return null;
        int noXa = Dem(TiengViTri.Loai.QuaCauLuaNo);
        yield return Nghe(1.2f, kq);
        float rmsXa = kq[0];
        var dangKeu = new List<string>();
        foreach (var a in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            if (a.isPlaying && a.clip != null && a.volume > 0f && !a.mute)
                dangKeu.Add(a.name + ":" + a.clip.name + " vol " + a.volume.ToString("F2") + " t " + a.time.ToString("F2"));
        Ghi("   nguon dang keu luc do: " + (dangKeu.Count > 0 ? string.Join(" | ", dangKeu) : "khong"));
        Ghi(string.Format("E1. no o 30 m (trong suong, MucThay {0:F2}): giong {1}, bi suong che +{2}, RMS loa {3:F5} (nen {4:F5}, khung dai nhat {5:F2} s)",
            TamNhin.MucThay(xa), noXa, TiengViTri.SoBiSuongChe - che0, rmsXa, nen, kq[5]));
        Kiem(noXa == 0 && TiengViTri.SoBiSuongChe - che0 == 1 && rmsXa < Mathf.Max(nen * 3f, rmsD[0] * 0.02f), "no trong suong van nghe thay");
        yield return ChoIm(2f);
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        TiengViTri.QuaCauLuaNo(xa);
        yield return null;
        int noXaDc = Dem(TiengViTri.Loai.QuaCauLuaNo);
        Ghi(string.Format("E1b. DOI CHUNG tat suong: no o 30 m -> giong {0} (MucThay {1:F2})", noXaDc, TamNhin.MucThay(xa)));
        Kiem(noXaDc == 1, "doi chung: tat suong ma no o 30 m van bi bo (phep do khong phan biet)");
        TamNhin.BatTrongPhepThu = true;
        yield return null; yield return null;
        yield return ChoIm(3f);

        // E2. qua cua "nguoi khac" bay tu 40 m vao
        int che1 = TiengViTri.SoBiSuongChe, ban1 = Dem(TiengViTri.Loai.QuaCauLuaBan);
        Vector3 tu = TrenDat(goc + truoc * 40f + phai * 1.5f);
        Vector3 huongVao = goc + Vector3.up - tu; huongVao.y = 0; huongVao.Normalize();
        var vao = Fireball.Spawn(tu, huongVao, 0, 0);
        vao.lifetime = 3.5f;
        yield return null; yield return null;
        TiengViTri.Giong gVao = null;
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.QuaCauLuaBay && g.theo == vao.transform) gVao = g;
        float dauNghe = -1f, mucNgoai = 0f; int mauNgoai = 0;
        var bl = new float[512];
        float rmsNgoai = 0; int soNgoai = 0;
        while (vao != null && gVao != null)
        {
            float d = Vector3.Distance(new Vector3(vao.transform.position.x, 0, vao.transform.position.z), new Vector3(goc.x, 0, goc.z));
            if (d > TamNhin.BanKinh + 0.5f) { mucNgoai = Mathf.Max(mucNgoai, gVao.muc); mauNgoai++; AudioListener.GetOutputData(bl, 0); float a = 0; foreach (var v in bl) a += v * v; rmsNgoai += Mathf.Sqrt(a / bl.Length); soNgoai++; }
            if (dauNghe < 0f && gVao.muc > 0.001f) dauNghe = d;
            if (d < 6f) break;
            yield return null;
        }
        Ghi(string.Format("E2. qua nguoi khac tu 40 m bay vao: tieng ban bi suong che +{0} (giong ban moi {1}); tieng bay muc lon nhat ngoai tam nhin {2:F3} ({3} khung, RMS loa {4:F5}); bat dau nghe o {5:F1} m",
            TiengViTri.SoBiSuongChe - che1, Dem(TiengViTri.Loai.QuaCauLuaBan) - ban1, mucNgoai, mauNgoai, soNgoai > 0 ? rmsNgoai / soNgoai : 0f, dauNghe));
        Kiem(gVao != null && TiengViTri.SoBiSuongChe - che1 == 1 && mucNgoai <= 0.001f && mauNgoai > 10, "qua ngoai tam nhin van nghe tieng ban / bay");
        Kiem(dauNghe > TamNhin.BanKinh - 6f && dauNghe <= TamNhin.BanKinh + 0.5f, "tieng bay khong bat dau nghe dung luc qua vao tam nhin");
        if (vao != null) Object.Destroy(vao.gameObject);
        yield return ChoIm(3f);

        // E3. qua cua minh bay ra xa
        Vector3 hRa = HuongTrong(goc, pc, 40f);
        int che2 = TiengViTri.SoBiSuongChe, no2 = TiengViTri.SoLan[2];
        // hitMask 0: bay du 68 m (khong vuong vat can) roi no NGOAI tam nhin
        var ra = Fireball.Spawn(goc + Vector3.up * 1.2f + hRa, hRa, 0, 0);
        ra.boQua = toi;
        yield return null; yield return null;
        TiengViTri.Giong gRa = null;
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.QuaCauLuaBay && g.theo == ra.transform) gRa = g;
        float mucTrong = 0f, mucNgoaiRa = 0f; int khungNgoai = 0; float dNo = 0f;
        float hanRa = Time.time + 5f;
        while (ra != null && Time.time < hanRa)
        {
            Vector3 p = ra.transform.position;
            dNo = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(goc.x, goc.z));
            if (gRa != null)
            {
                if (dNo < 15f) mucTrong = Mathf.Max(mucTrong, gRa.muc);
                if (dNo > TamNhin.BanKinh + 2.5f) { mucNgoaiRa = Mathf.Max(mucNgoaiRa, gRa.muc); khungNgoai++; }
            }
            yield return null;
        }
        yield return null;
        bool noNgoai = dNo > TamNhin.BanKinh + 0.5f;
        Ghi(string.Format("E3. qua cua minh bay ra xa: tieng bay trong 15 m muc {0:F3}, ngoai tam nhin + 2,5 m muc lon nhat {1:F3} ({2} khung); no o {3:F1} m -> yeu cau no +{4}, bi suong che +{5}",
            mucTrong, mucNgoaiRa, khungNgoai, dNo, TiengViTri.SoLan[2] - no2, TiengViTri.SoBiSuongChe - che2));
        Kiem(gRa != null && mucTrong > 0.3f && khungNgoai > 5 && mucNgoaiRa <= 0.001f, "tieng bay khong tat khi qua cua minh ra ngoai tam nhin");
        if (noNgoai) Kiem(TiengViTri.SoBiSuongChe - che2 == 1, "qua no ngoai tam nhin van keu");
        else Ghi("   (qua no trong tam - cham vat can - khong kiem phan no trong suong o muc nay)");
        if (ra != null) Object.Destroy(ra.gameObject);
        yield return ChoIm(3f);

        // ===== F. pham vi + gioi han =====
        Ghi("");
        // F1 qua nay
        int sb = TiengViTri.SoLan[0], sy = TiengViTri.SoLan[1];
        var nay = Fireball.Spawn(TrenDat(goc + truoc * 3f), truoc, 0, 0); nay.laQuaNay = true; nay.lifetime = 0.6f;
        yield return null; yield return null;
        int fBan = TiengViTri.SoLan[0] - sb, fBay = TiengViTri.SoLan[1] - sy;
        // F2 qua cua quai (EnemyAI goi Fireball.Spawn)
        yield return ChoIm(3f);
        sb = TiengViTri.SoLan[0]; sy = TiengViTri.SoLan[1];
        var quai = Fireball.Spawn(TrenDat(goc + truoc * 5f + phai * 3f), -truoc, 0, 0); quai.lifetime = 0.6f;
        yield return null; yield return null;
        int qBan = Dem(TiengViTri.Loai.QuaCauLuaBan), qBay = Dem(TiengViTri.Loai.QuaCauLuaBay), qYc = TiengViTri.SoLan[0] - sb;
        yield return ChoIm(3f);
        // F3 Lua dia nguc
        LuaDiaNguc.SpawnChum(goc + Vector3.up * 1.2f + truoc, truoc, goc, pc.MatNaVatCan, 0, toi);
        yield return null; yield return null;
        int lBan = Dem(TiengViTri.Loai.QuaCauLuaBan), lBay = Dem(TiengViTri.Loai.QuaCauLuaBay);
        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)) Object.Destroy(f.gameObject);
        yield return ChoIm(3f);
        // F4 tran tieng bay + qua bi xoa khong qua Explode
        var muoi = new List<Fireball>();
        for (int i = 0; i < 10; i++)
        {
            var f = Fireball.Spawn(TrenDat(goc + truoc * (2f + i * 0.4f) + phai * (i - 5) * 0.6f), truoc, 0, 0);
            f.lifetime = 10f; f.speed = 0.01f;
            muoi.Add(f);
        }
        yield return new WaitForSeconds(0.3f);
        int tran = Dem(TiengViTri.Loai.QuaCauLuaBay);
        // F5 thanh Hiệu ứng 0
        CaiDatAmThanh.XemTruocHieuUng(0);
        yield return new WaitForSecondsRealtime(0.25f);     // bo dem GetOutputData con giu tieng truoc khi keo ve 0
        float volMax = 0; foreach (var g in TiengViTri.CacGiong) if (!g.Ranh) volMax = Mathf.Max(volMax, g.nguon.volume);
        yield return Nghe(0.4f, kq);
        float rms0 = kq[0];
        CaiDatAmThanh.BoXemTruoc();
        foreach (var f in muoi) if (f != null) Object.Destroy(f.gameObject);
        yield return new WaitForSeconds(0.4f);
        // chi dem tieng BAY: tieng ban cua 10 qua (ban 3 dai 1,45 s, cu 0,8 s) con keu hop le luc nay
        int conSauXoa = 0; foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == TiengViTri.Loai.QuaCauLuaBay) conSauXoa++;
        Ghi(string.Format("F. qua nay: yeu cau ban +{0}, bay +{1}; qua quai (Fireball.Spawn): yeu cau ban +{2}, giong ban {3}, bay {4}; Lua dia nguc: giong ban {5}, bay {6}",
            fBan, fBay, qYc, qBan, qBay, lBan, lBay));
        Ghi(string.Format("   10 qua cung bay -> giong bay {0} (tran {1}); Hiệu ứng 0 -> am luong giong lon nhat {2:F3}, RMS loa {3:F5}; xoa 10 qua (khong no) -> giong bay con {4}",
            tran, TiengViTri.SoTiengBayToiDa, volMax, rms0, conSauXoa));
        Kiem(fBan == 0 && fBay == 1, "qua nay van keu tieng ban / khong co tieng bay");
        Kiem(qYc == 1 && qBan == 1 && qBay == 1, "qua cau cua quai khong co tieng ban + bay");
        Kiem(lBan == 1 && lBay == 5, "Lua dia nguc khong ra 1 tieng ban + 5 tieng bay");
        Kiem(tran == TiengViTri.SoTiengBayToiDa, "so tieng bay vuot tran");
        Kiem(volMax <= 0.0001f && rms0 < Mathf.Max(nen * 3f, 1e-4f), "thanh Hiệu ứng 0 ma van co tieng");
        Kiem(conSauXoa == 0, "qua bi xoa khong qua Explode ma tieng bay con keu");
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengquacaulua.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TamNhin.BatTrongPhepThu = false;
        CaiDatAmThanh.BoXemTruoc();
        TranHienTai.Xoa();
        foreach (var n in new[] { Ten, "TAM_HopTiengLua" })
        {
            var rac = GameObject.Find(n);
            if (rac != null) Object.DestroyImmediate(rac);
        }
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
        Debug.Log("[TiengLua] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
