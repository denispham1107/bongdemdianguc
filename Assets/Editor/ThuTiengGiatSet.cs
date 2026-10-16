using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 129 - TIENG GIUT SET + QUA CAU DIEN (16/10/2026, nguoi dung: phong tia · tia trung doi thu · tia lan; tia dien cua Qua cau dien dung
/// chung; Qua cau dien hien dien thi dien xet lien tuc ben trong; Quy cay dung chung (nguoi dung chon); trong suong thi khong nghe).
/// Mau B2 "zzzt" (Giut set) + B3 (vong lap Qua cau dien) - CongCu/AmThanh/tieng_giat_set_2.py. Play Act2 that, tat GameDirector + xoa quai.
///   A. (ngoai Play) 10 file trung tung byte ban goc GiatSetThu2, STEREO, khong ForceToMono; vong lap 3 s.
///   B. TUNG THAT Giut set (CastAt 6) vao 6 bia (4 mach dau + nhip lan): 1 tieng phong; moi mach 1 tieng trung (ke dau 1, ke sau x0,6) tai cho bia; moi nhip lan
///      1 tieng lan (so tieng trung + lan = so tia ve); loa co tieng, khong mono.
///   C. BAN VAO KHONG KHI: khong ai trong tam -> van 1 tieng phong, 0 trung.
///   D. QUA CAU DIEN THAT (CastAt 13) giua 3 bia: 1 vong lap bam theo qua cau suot doi; moi luot 1 tieng lan (x0,75) + moi tia 1 tieng trung; qua cau tan
///      -> vong lap tat; loa luc qua cau dang xet co tieng, khong mono.
///   E. QUY CAY: GiatSet.PhongCuaQuai vao bia -> 1 phong + 1 trung.
///   F. SUONG MU: phong / trung / lan o 40 m -> bo, vong lap Qua cau dien o 40 m giu giong muc 0, loa im; doi chung tat suong -> co giong.
///   G. BIEN THE: 24 lan moi loai -> du 3 bien the, cao do +-6%.
/// Ket qua: PlayTestShots/tienggiatset.txt.
/// </summary>
public static class ThuTiengGiatSet
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengGiatSet";
    const string Goc = "CongCu/AmThanh/GiatSetThu2/";

    [MenuItem("Diablo 2.5D/129. Chay thu TIENG GIUT SET + QUA CAU DIEN (phong · trung · lan · dien xet, suong mu)", false, 218)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengGiatSet] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengGiatSet] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng Giut set (B2 'zzzt') + Qua cau dien (vong lap B3)");
        var ds = new List<KeyValuePair<string, string>>();
        foreach (var kv in new[] { new[] { "GiatSetPhong", "B2_zzzt_phong" }, new[] { "GiatSetTrung", "B2_zzzt_trung" }, new[] { "GiatSetLan", "B2_zzzt_lan" } })
            for (int i = 1; i <= 3; i++) ds.Add(new KeyValuePair<string, string>(kv[0] + i, kv[1] + i));
        ds.Add(new KeyValuePair<string, string>("CauDienVong", "B3_cau"));
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
            if (kv.Key == "CauDienVong") Kiem(c != null && Mathf.Abs(c.length - 3f) < 0.001f, "vong lap Qua cau dien khong dai dung 3 s");
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
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengGiatSet] " + s); }
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

    static List<TiengViTri.Giong> Giong(TiengViTri.Loai l)
    {
        var ds = new List<TiengViTri.Giong>();
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l && !g.dung) ds.Add(g);
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

    static Vector3 Dat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f); return p; }

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

    static Vector3 HuongTrong(Vector3 goc, PlayerController pc, float dai)
    {
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc + Vector3.up * 1.4f, 1.2f, h, out RaycastHit _, dai, pc.MatNaVatCan & ~LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
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
        Vector3 h = HuongTrong(goc, pc, 16f);
        Vector3 ngang = Vector3.Cross(Vector3.up, h);
        pc.transform.rotation = Quaternion.LookRotation(h);
        float[] kq = new float[5];
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(6); CapDo.MoCaDuongChoPhepThu(CapDo.KyCauDien);
        yield return ChoIm(3f);

        // ===== B. tung that Giut set =====
        Ghi("");
        var bias = new List<Damageable>();
        for (int i = -1; i <= 1; i++)
        {
            bias.Add(TaoBia("TAM_BiaSet" + (i + 1), Dat(goc + h * 9f + ngang * i * 3f)));
            bias.Add(TaoBia("TAM_BiaLan" + (i + 1), Dat(goc + h * 12f + ngang * i * 3.6f)));
        }
        Physics.SyncTransforms();
        pc.mana = pc.maxMana;
        int p0 = SL(TiengViTri.Loai.GiatSetPhong), t0 = SL(TiengViTri.Loai.GiatSetTrung), l0 = SL(TiengViTri.Loai.GiatSetLan), v0 = GiatSet.SoTiaDaVe;
        pc.CastAt(6, goc + h * 9f);
        var heSoTrung = new List<float>(); bool trungDungCho = true; int phongGiong = 0;
        float hanB = Time.time + 2f;
        bool daDoc = false;
        while (Time.time < hanB)
        {
            if (!daDoc && SL(TiengViTri.Loai.GiatSetTrung) > t0)
            {
                daDoc = true;
                yield return null;
                phongGiong = Giong(TiengViTri.Loai.GiatSetPhong).Count;
                foreach (var g in Giong(TiengViTri.Loai.GiatSetTrung))
                {
                    heSoTrung.Add(g.heSo);
                    bool gan = false; foreach (var b in bias) if ((g.viTri - b.transform.position).sqrMagnitude < 2.5f * 2.5f) gan = true;
                    if (!gan) trungDungCho = false;
                }
                yield return Nghe(0.5f, kq);
                continue;
            }
            yield return null;
        }
        heSoTrung.Sort(); heSoTrung.Reverse();
        int soTia = GiatSet.SoTiaDaVe - v0, soTrung = SL(TiengViTri.Loai.GiatSetTrung) - t0, soLan = SL(TiengViTri.Loai.GiatSetLan) - l0;
        Ghi(string.Format("B. tung that: phong +{0} (giong {1}); tia ve {2} = trung +{3} + lan +{4}; he so trung {5}; trung dung cho bia {6}",
            SL(TiengViTri.Loai.GiatSetPhong) - p0, phongGiong, soTia, soTrung, soLan, string.Join(" / ", heSoTrung.ConvertAll(x => x.ToString("F2"))), trungDungCho));
        Ghi(string.Format("   loa: RMS {0:F4} (dinh {1:F4}), tuong quan {2:F3}", kq[0], kq[4], kq[3]));
        Kiem(SL(TiengViTri.Loai.GiatSetPhong) - p0 == 1 && phongGiong == 1, "tung Giut set khong ra dung 1 tieng phong");
        // (lan chay 1 mong 3 mach: Giut set nguoi choi ban toi da GiatSet.SoTiaNguoiChoi = 4 tia dau - 6 bia phia truoc -> 4 mach + 2 nhip lan)
        Kiem(soTrung == Mathf.Min(GiatSet.SoTiaNguoiChoi, bias.Count) && soLan >= 1 && soTrung + soLan == soTia && trungDungCho,
             "khong dung moi mach 1 tieng trung + moi nhip lan 1 tieng lan");
        Kiem(heSoTrung.Count == soTrung && Mathf.Approximately(heSoTrung[0], 1f) && Mathf.Approximately(heSoTrung[heSoTrung.Count - 1], TiengViTri.HeSoSetKeSau),
             "tieng trung ke sau khong nho lai");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa khong co tieng / gan mono");
        yield return ChoIm(4f);

        // ===== C. ban vao khong khi =====
        Ghi("");
        foreach (var b in bias) b.gameObject.SetActive(false);
        Physics.SyncTransforms();
        yield return new WaitForSeconds(0.5f);
        pc.mana = pc.maxMana;
        int cp0 = SL(TiengViTri.Loai.GiatSetPhong), ct0 = SL(TiengViTri.Loai.GiatSetTrung);
        pc.CastAt(6, goc + h * 9f);
        yield return new WaitForSeconds(1.0f);
        Ghi(string.Format("C. ban vao khong khi: phong +{0}, trung +{1}", SL(TiengViTri.Loai.GiatSetPhong) - cp0, SL(TiengViTri.Loai.GiatSetTrung) - ct0));
        Kiem(SL(TiengViTri.Loai.GiatSetPhong) - cp0 == 1 && SL(TiengViTri.Loai.GiatSetTrung) - ct0 == 0, "ban vao khong khi khong co tieng phong / van co tieng trung");
        foreach (var b in bias) b.gameObject.SetActive(true);
        Physics.SyncTransforms();
        yield return ChoIm(4f);

        // ===== D. Qua cau dien that =====
        Ghi("");
        pc.mana = pc.maxMana;
        int dv0 = SL(TiengViTri.Loai.CauDienVong), dl0 = SL(TiengViTri.Loai.GiatSetLan), dt0 = SL(TiengViTri.Loai.GiatSetTrung), luot0 = QuaCauDien.SoLuotDaBan, tia0 = QuaCauDien.SoTiaDaBan;
        pc.CastAt(CapDo.KyCauDien, goc + h * 10.5f);
        QuaCauDien cau = null;
        float hanD = Time.time + 2f;
        while (cau == null && Time.time < hanD) { cau = Object.FindAnyObjectByType<QuaCauDien>(); yield return null; }
        yield return new WaitForSeconds(0.6f);
        var gv = Giong(TiengViTri.Loai.CauDienVong);
        bool vongBam = gv.Count == 1 && cau != null && gv[0].theo == cau.transform && gv[0].bamTheo;
        var heSoBan = new List<float>();
        foreach (var g in Giong(TiengViTri.Loai.GiatSetLan)) heSoBan.Add(g.heSo);
        yield return Nghe(1.0f, kq);
        float hanD2 = Time.time + 8f;
        while (cau != null && Time.time < hanD2) yield return null;
        yield return new WaitForSeconds(0.3f);
        int vongCon = Giong(TiengViTri.Loai.CauDienVong).Count;
        int luot = QuaCauDien.SoLuotDaBan - luot0, tia = QuaCauDien.SoTiaDaBan - tia0;
        Ghi(string.Format("D. Qua cau dien: vong lap +{0} (bam theo qua cau {1}); {2} luot, {3} tia -> tieng lan (ban) +{4}, trung +{5}; he so tieng ban {6}; qua cau tan -> vong lap con {7}",
            SL(TiengViTri.Loai.CauDienVong) - dv0, vongBam, luot, tia, SL(TiengViTri.Loai.GiatSetLan) - dl0, SL(TiengViTri.Loai.GiatSetTrung) - dt0,
            string.Join(" / ", heSoBan.ConvertAll(x => x.ToString("F2"))), vongCon));
        Ghi(string.Format("   loa luc qua cau xet: RMS {0:F4} (dinh {1:F4}), tuong quan {2:F3}", kq[0], kq[4], kq[3]));
        Kiem(SL(TiengViTri.Loai.CauDienVong) - dv0 == 1 && vongBam, "Qua cau dien khong co dung 1 vong lap bam theo");
        Kiem(luot == QuaCauDien.SoLuot && SL(TiengViTri.Loai.GiatSetLan) - dl0 == luot && SL(TiengViTri.Loai.GiatSetTrung) - dt0 == tia, "moi luot khong 1 tieng ban + moi tia 1 tieng trung");
        Kiem(heSoBan.Count >= 1 && heSoBan[0] <= TiengViTri.HeSoCauDienBan + 1e-4f, "tieng ban cua qua cau khong nho lai x0,75");
        Kiem(vongCon == 0, "qua cau tan ma vong lap van keu");
        Kiem(kq[0] > 1e-3f && kq[3] < 0.95f, "loa luc qua cau xet khong co tieng / gan mono");
        foreach (var b in bias) Object.Destroy(b.gameObject);
        yield return ChoIm(4f);

        // ===== E. Quy cay =====
        Ghi("");
        var biaE = TaoBia("TAM_BiaQuyCay", Dat(goc + h * 6f));
        Physics.SyncTransforms();
        int ep0 = SL(TiengViTri.Loai.GiatSetPhong), et0 = SL(TiengViTri.Loai.GiatSetTrung);
        GiatSet.PhongCuaQuai(goc + Vector3.up * 1.3f, h, LayerMask.GetMask("Enemy"), 10f, 12f, Color.white, Color.green);
        yield return new WaitForSeconds(0.6f);
        Ghi(string.Format("E. tia Quy cay (PhongCuaQuai): phong +{0}, trung +{1}", SL(TiengViTri.Loai.GiatSetPhong) - ep0, SL(TiengViTri.Loai.GiatSetTrung) - et0));
        Kiem(SL(TiengViTri.Loai.GiatSetPhong) - ep0 == 1 && SL(TiengViTri.Loai.GiatSetTrung) - et0 == 1, "tia Quy cay khong co tieng phong + trung");
        Object.Destroy(biaE.gameObject);
        yield return ChoIm(4f);

        // ===== F. suong mu =====
        Ghi("");
        Vector3 xa = Dat(goc + h * 40f) + Vector3.up;
        int che0 = TiengViTri.SoBiSuongChe;
        TiengViTri.GiatSetPhong(xa); TiengViTri.GiatSetTrung(xa); TiengViTri.GiatSetLan(xa);
        var vatXa = new GameObject("TAM_CauXa").transform; vatXa.position = xa;
        var gXa = TiengViTri.CauDienVong(vatXa);
        yield return new WaitForSeconds(0.3f);
        yield return Nghe(0.6f, kq);
        int giongMotLan = Giong(TiengViTri.Loai.GiatSetPhong).Count + Giong(TiengViTri.Loai.GiatSetTrung).Count + Giong(TiengViTri.Loai.GiatSetLan).Count;
        Ghi(string.Format("F. o 40 m (MucThay {0:F2}): bi suong che +{1}, giong mot lan {2}; vong lap giong {3} muc {4:F3}; RMS loa {5:F5}",
            TamNhin.MucThay(xa), TiengViTri.SoBiSuongChe - che0, giongMotLan, gXa != null ? 1 : 0, gXa != null ? gXa.MucDangDat : -1f, kq[0]));
        Kiem(TiengViTri.SoBiSuongChe - che0 == 3 && giongMotLan == 0 && (gXa == null || gXa.MucDangDat == 0f) && kq[0] < 1e-4f, "tieng dien trong suong van nghe thay");
        TiengViTri.Dung(gXa, vatXa);
        TamNhin.BatTrongPhepThu = false;
        yield return null; yield return null;
        int che1 = TiengViTri.SoBiSuongChe;
        TiengViTri.GiatSetTrung(xa);
        yield return null;
        int giongDc = Giong(TiengViTri.Loai.GiatSetTrung).Count;
        Ghi(string.Format("F2. DOI CHUNG tat suong: trung o 40 m -> giong {0}, bi suong che +{1}", giongDc, TiengViTri.SoBiSuongChe - che1));
        Kiem(giongDc == 1 && TiengViTri.SoBiSuongChe - che1 == 0, "doi chung tat suong ma van bi bo");
        TamNhin.BatTrongPhepThu = true;
        Object.Destroy(vatXa.gameObject);
        yield return ChoIm(4f);

        // ===== G. bien the =====
        Ghi("");
        var loai = new[] { TiengViTri.Loai.GiatSetPhong, TiengViTri.Loai.GiatSetTrung, TiengViTri.Loai.GiatSetLan };
        var dongG = new List<string>(); bool du = true; float caoMin = 9f, caoMax = 0f;
        foreach (var l in loai)
        {
            var clipL = new HashSet<string>(); int soDuoc = 0;
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = Dat(goc + h * (3f + (i % 4) * 3f) + ngang * ((i % 5) - 2f) * 2f) + Vector3.up;
                int truocPhat = TiengViTri.SoLanDuocGiong;
                if (l == TiengViTri.Loai.GiatSetPhong) TiengViTri.GiatSetPhong(p);
                else if (l == TiengViTri.Loai.GiatSetTrung) TiengViTri.GiatSetTrung(p);
                else TiengViTri.GiatSetLan(p);
                var gm = TiengViTri.GiongMoiNhat;
                if (TiengViTri.SoLanDuocGiong > truocPhat && gm != null && gm.loai == l && gm.nguon != null && gm.nguon.clip != null)
                {
                    soDuoc++; clipL.Add(gm.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                }
                yield return new WaitForSeconds(0.3f);
            }
            dongG.Add(l + " " + clipL.Count + " (" + soDuoc + " lan duoc giong)");
            if (clipL.Count != 3) du = false;
            yield return ChoIm(5f);
        }
        Ghi(string.Format("G. 24 lan moi loai: so bien the {0}; cao do {1:F3} - {2:F3}", string.Join(", ", dongG), caoMin, caoMax));
        Kiem(du, "khong du 3 bien the moi loai");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tienggiatset.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TamNhin.BatTrongPhepThu = false;
        TranHienTai.Xoa();
        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (d0 != null && d0.name.StartsWith("TAM_")) Object.DestroyImmediate(d0.gameObject);
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
        Debug.Log("[TiengGiatSet] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
