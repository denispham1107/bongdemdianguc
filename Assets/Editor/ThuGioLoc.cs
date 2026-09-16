using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 71): KY NANG "GIO LOC" (so hieu 10, 16/09/2026).
///
/// Nguoi dung: 3 loc nho (hinh Loc xoay, cao duoi 1/2, mau nau, co tia set), bay bang toc do Qua cau bang,
/// tan sau 3,5 giay; sat thuong 75, hoi chieu 0,4; 55% hat tung 0,5 giay (khong cuon), dang tung chieu thi
/// bi ngat; xuyen moi vat can va nguoi choi. Nguoi dung chon them: 75 mot lan moi muc tieu moi loc, tia set 15,
/// 55% moi loc, 20 nang luong, toa quat 11 do, ngat ca quai, niem 0,38, vung trung 2,2 m, cao 1,5 m, dap tat lo lua
/// (chay lai sau 30 giay), icon Loc xoay nhuom nau.
/// 17/09/2026 nguoi dung doi: hinh dung lai bang Blender (xam trang nhu Loc xoay, cao ~5 m, xoay MOT CHIEU tu duoi len,
/// khoi bui den cuon len + vet phia sau), BO tia set (chi con 75), toc do giam 25% (12,75 m/s).
///
///   A. Thong so that: hang so, nhan vat (nang luong / hoi chieu / niem), Sach phep, HUD 11 icon, icon nap duoc,
///      toc do = truong speed cua mot Qua cau bang THAT.
///   B. Tung that CastAt(10): khoa thi tu choi; mo khoa -> dung 3 loc, tru 20 nang luong, bam lai bi hoi chieu,
///      doi 0,45 s thi tung duoc. Trung bia -> keDanhCuoi = nguoi tung.
///   C. Hinh: luoi Blender (Vo0-2, DaiGio); cao ~5 m (doi chung Loc xoay that); xam trang; khong den; KHONG tia set;
///      XOAY MOT CHIEU TU DUOI LEN: doc xoan cua dai gio doc tu luoi + chieu quay do bang goc that cua vo theo thoi gian
///      + chieu truot anh, moi lop deu cung chieu; khoi bui den cuon quanh than: hat bay LEN va quay CUNG chieu (do tung
///      hat theo randomSeed), vet bui con lai phia sau (hat khong gian the gioi); chup anh.
///   D. Toc do do bang vi tri (m/s) va thoi gian song (luc ngung di).
///   E. Xuyen vat can: bay thang qua mot bia mo; xuyen nguoi choi / ke dich: di tiep sau khi trung.
///   F. Sat thuong: mot loc qua bia dung yen = 75 dung mot lan; ba loc cung trung = 225;
///      vung trung: bia lech 2,5 m trung, 2,7 m truot (tinh toi mat than bia ban kinh 0,4).
///   G. Hat tung: 19 bia x 10 loc -> ti le (dem doc lap bang component xuat hien), do cao hinh lon nhat,
///      thoi gian bay; bia co khieng -> 0 lan.
///   H. Ngat chieu: nguoi choi dang niem Qua cau lua bi hat -> 0 qua bay ra (doi chung khong hat -> 3);
///      loc that trung nguoi dang niem; bi hat thi CastAt bi tu choi. Quai dang ra don bi hat -> khong trung
///      (doi chung -> trung).
///   I. Qua mang: goi ky nang so 10; goi trang thai minh mang bit hat tung; ban sao nhan bit -> bay len;
///      ban sao dang niem nhan bit -> khong phong phep (doi chung phong 3 qua); goi tre khong hat lai lan hai;
///      mat na 5 bit ca goi nguoi choi va goi quai.
///   J. Lo lua: loc luot qua -> tat, lo doi chung van chay; 25 giay van tat, 31 giay chay lai.
///
/// Ket qua: PlayTestShots/gioloc.txt, anh gioloc_*.png.
/// </summary>
public static class ThuGioLoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyGioLoc;

    [MenuItem("Diablo 2.5D/71. Chay thu GIO LOC (ky nang moi)", false, 159)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu gio loc", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Gio loc");
        DoDocXoanNgoaiPlay();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static int docTangEditor, docGiamEditor;
    static float rChanVo1, rGiuaVo1;

    /// <summary>
    /// DOC XOAN cua dai gio doc thang tu luoi FBX - LAM NGOAI PLAY: trong Play luoi khong bat Read/Write tra ve mang rong.
    /// Dinh ke tiep doc dai (uv.x lon hon mot buoc) len cao thi goc atan2(z,x) tang hay giam.
    /// </summary>
    static void DoDocXoanNgoaiPlay()
    {
        docTangEditor = 0; docGiamEditor = 0; rChanVo1 = 0f; rGiuaVo1 = 0f;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/KyNang/GioLoc/LocNho.fbx"))
        {
            var m = o as Mesh;
            // Ban kinh vo Vo1 o chan va o 2,5 m (tren doan noi): nguoi dung xin chan to them 20%, phan tren giu nguyen
            if (m != null && m.name == "Vo1")
                foreach (var p in m.vertices)
                {
                    float r = new Vector2(p.x, p.z).magnitude;
                    if (p.y < 0.05f) rChanVo1 = Mathf.Max(rChanVo1, r);
                    if (Mathf.Abs(p.y - 2.5f) < 0.1f) rGiuaVo1 = Mathf.Max(rGiuaVo1, r);
                }
            if (m == null || m.name != "DaiGio") continue;
            var v = m.vertices; var uv = m.uv;
            for (int i = 0; i < v.Length; i += 5)
            {
                // Bo chan loc (< 1,5 m): ban kinh 0,3 m, nam dai cach nhau ~0,37 m - tim dinh ke tiep de bat nham sang dai ben
                if (v[i].y < 1.5f) continue;
                // Dinh ke tiep = dinh GAN NHAT thoa uv (lay ung vien dau tien thi co luc bat nham dai ben canh)
                int tot = -1; float ganNhat = 0.6f;
                for (int j = 0; j < v.Length; j++)
                {
                    float du = uv[j].x - uv[i].x;
                    if (du < 0.02f || du > 0.04f || Mathf.Abs(uv[j].y - uv[i].y) > 0.01f || v[j].y <= v[i].y) continue;
                    float kc = (v[j] - v[i]).magnitude;
                    if (kc < ganNhat) { ganNhat = kc; tot = j; }
                }
                if (tot < 0) continue;
                float dg = Mathf.DeltaAngle(Mathf.Atan2(v[i].z, v[i].x) * Mathf.Rad2Deg, Mathf.Atan2(v[tot].z, v[tot].x) * Mathf.Rad2Deg);
                if (dg > 0.01f) docTangEditor++; else if (dg < -0.01f) docGiamEditor++;
            }
        }
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_GioLoc") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_GioLoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GioLoc] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    /// <summary>Bia do don co mot khoi hinh con ("Hinh") de BiHatTung co cai nhac len.</summary>
    static Damageable TaoBia(string ten, Vector3 p)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var hinh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(hinh.GetComponent<Collider>());
        hinh.name = "Hinh";
        hinh.transform.SetParent(go.transform, false);
        hinh.transform.localPosition = Vector3.up;
        hinh.transform.localScale = new Vector3(0.7f, 2f, 0.7f);
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 10000000f; d.health = 10000000f;
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

    static IEnumerator GiuSong()
    {
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoGioLoc") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static Vector3 HuongTrong(PlayerController pc)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 0.6f, h, out RaycastHit _, 12f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static int DemLoc() { return Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude).Length; }
    static int DemCauLua() { return Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude).Length; }

    static void XoaLoc()
    {
        foreach (var g in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) Object.Destroy(g.gameObject);
    }

    /// <summary>Do cao tu goc toi dinh hop bao cac MeshRenderer (vo, dai xoan) cua mot vat.</summary>
    static float CaoHinh(GameObject go)
    {
        float dinh = float.MinValue;
        foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
            dinh = Mathf.Max(dinh, r.bounds.max.y);
        return dinh - go.transform.position.y;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        // Phep thu dai hon 30 giay: tat GameDirector de dot quai dau khong chen vao
        if (dir != null) dir.enabled = false;
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);
        int maskEnemy = LayerMask.GetMask("Enemy");

        // ================= A. THONG SO =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var quaThat = QuaCauBang.Spawn(toi.transform.position + Vector3.up * 30f, Vector3.up, 0, 0);
        float tocQuaCau = quaThat.speed;
        Object.DestroyImmediate(quaThat.gameObject);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/GioLoc");
        Ghi(string.Format("A. nhan vat: nang luong {0}, hoi chieu {1} s, niem {2} s; Sach phep doc {3}/{4}/{5}; sat thuong {6}, hat tung {7:P0} x {8} s cao {9} m, vung trung {10} m, song {11} s, cao hinh {12} m",
            toi.gioLocCost, toi.gioLocCooldown, toi.gioLocCastTime, nl, hc, nc, GioLoc.SatThuongGoc,
            GioLoc.XacSuatHatTung, BiHatTung.GiayMacDinh, BiHatTung.CaoBay, GioLoc.BanKinhTrung, GioLoc.ThoiGianSong, GioLoc.ChieuCao));
        Ghi(string.Format("A. toc do loc {0} m/s, toc do mot Qua cau bang that {1} m/s; HUD {2} icon, icon so 10 {3}; icon file {4}; ten \"{5}\", tom tat \"{6}\", mo ta {7} ky tu",
            GioLoc.TocDo, tocQuaCau, bo != null ? bo.Length : -1, bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG",
            tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG", SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length));
        Kiem(Mathf.Approximately(toi.gioLocCost, 20f) && Mathf.Approximately(nl, 20f), "nang luong khong phai 20");
        Kiem(Mathf.Approximately(toi.gioLocCooldown, 0.4f) && Mathf.Approximately(hc, 0.4f), "hoi chieu khong phai 0,4");
        Kiem(Mathf.Approximately(toi.gioLocCastTime, 0.38f), "niem khong phai 0,38");
        // Doi chung doc lap voi code: ban kinh goc = cong thuc Blender cu R(t) = 0,30 + 1,95 t^1,9 (Vo1 he so 1)
        float rChanGoc = 0.30f, rGiuaGoc = 0.30f + 1.95f * Mathf.Pow(0.5f, 1.9f);
        Ghi(string.Format("A. luoi Vo1: ban kinh chan {0:F3} m (goc {1:F3}, x{2:F3}); o 2,5 m {3:F3} m (goc {4:F3}, x{5:F3})",
            rChanVo1, rChanGoc, rChanVo1 / rChanGoc, rGiuaVo1, rGiuaGoc, rGiuaVo1 / rGiuaGoc));
        Kiem(Mathf.Abs(rChanVo1 / rChanGoc - 1.3f) < 0.01f, "chan loc khong to them 30% so ban goc");
        Kiem(Mathf.Abs(rGiuaVo1 / rGiuaGoc - 1f) < 0.01f, "phan than tren bi doi kich thuoc");
        Kiem(Mathf.Abs(GioLoc.TocDo - 8f) < 0.001f, "toc do loc khong phai 8 m/s");
        Kiem(!SachPhep.MoTa(K).Contains("sét"), "mo ta Sach phep van nhac tia set");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Gio loc");
        Kiem(SachPhep.Ten(K) == "GIÓ LỐC" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Gio loc");

        // ================= B. TUNG THAT =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;
        int soPhep = 0, kyVua = -1;
        System.Action<int, Vector3> dem = (s, a) => { soPhep++; kyVua = s; };
        toi.DaTungPhep += dem;
        CapDo.BatDauTranMoi();
        int s0 = soPhep;
        toi.CastAt(K, goc + huong * 8f);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoKhoa(K);
        // Cap 1 chi co 1 diem: len mot cap de co them diem mo Qua cau lua (muc H can mot phep dang niem)
        CapDo.Them(CapDo.CanDeLenCap(1));
        CapDo.MoKhoa(0);
        Kiem(CapDo.DaMo(K) && CapDo.DaMo(0), "khong mo khoa duoc Gio loc va Qua cau lua cho phep thu");
        var biaB = TaoBia("TAM_BiaB", goc + huong * 8f);
        yield return new WaitForSeconds(0.3f);
        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana, mauB = biaB.health;
        toi.CastAt(K, biaB.transform.position);
        bool daTung = soPhep == s0 + 1 && kyVua == K;
        float manaTon = manaTruoc - toi.mana;
        int sBam = soPhep;
        float lucTung = Time.time;
        float hoiNgaySau = toi.HoiChieuGiay(K);
        // Bam lai MOI KHUNG cho toi khi duoc nhan: do bang dong ho game, khong phu thuoc khung hinh Editor giat
        bool thayNhacHoi = false; float lucNhan = -1f; int soLocBay = 0;
        float hanB = Time.time + 1.5f;
        while (Time.time < hanB)
        {
            soLocBay = Mathf.Max(soLocBay, DemLoc());
            toi.CastAt(K, biaB.transform.position);
            if (soPhep > sBam) { lucNhan = Time.time - lucTung; break; }
            if (toi.LastMessage == "GIÓ LỐC đang hồi chiêu") thayNhacHoi = true;
            yield return null;
        }
        bool tuChoiHoiChieu = lucNhan >= 0.4f - 0.001f;
        bool tungLai = lucNhan > 0f && lucNhan < 0.5f;
        yield return new WaitForSeconds(0.6f);
        Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> tung {1}, {2} loc bay, ton {3} nang luong; hoi chieu ngay sau khi tung {4:F2} s; bam lai moi khung -> duoc nhan sau {5:F3} s (thay nhac hoi chieu: {6}); bia truoc mat mat {7:F0} mau, ke danh cuoi la nguoi tung {8}",
            tuChoiKhoa, daTung, soLocBay, manaTon, hoiNgaySau, lucNhan, thayNhacHoi, mauB - biaB.health, biaB.keDanhCuoi == mauToi));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(daTung && soLocBay == 3, "tung Gio loc khong ra dung 3 loc");
        Kiem(Mathf.Abs(manaTon - 20f) < 0.01f, "khong ton dung 20 nang luong");
        Kiem(tuChoiHoiChieu && tungLai, "hoi chieu 0,4 giay khong dung");
        Kiem(biaB.keDanhCuoi == mauToi, "trung bia ma khong ghi ke danh (mat kinh nghiem)");
        Object.Destroy(biaB.gameObject);
        XoaLoc();
        yield return new WaitForSeconds(0.5f);

        // ================= C + D. HINH, TOC DO, THOI GIAN SONG =================
        Ghi("");
        {
            var locXoay = Tornado.Spawn(goc + huong * 40f, huong, 0);
            yield return null; yield return null;
            float caoLon = CaoHinh(locXoay.gameObject);
            Object.Destroy(locXoay.gameObject);

            var arcTruoc = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            var loc = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
            float lucSinh = Time.time;
            yield return null; yield return null;
            float caoNho = CaoHinh(loc.gameObject);
            int soDen = loc.GetComponentsInChildren<Light>(true).Length;

            // Luoi Blender + mau vo
            var tenLuoi = new List<string>();
            Color tb = Color.black; int soVo = 0;
            Transform vo0 = null, dai = null;
            foreach (var mf in loc.GetComponentsInChildren<MeshFilter>())
            {
                tenLuoi.Add(mf.name + "(" + (mf.sharedMesh != null ? mf.sharedMesh.name : "null") + ")");
                if (mf.name == "Vo0") vo0 = mf.transform;
                if (mf.name == "DaiGio") dai = mf.transform;
                var mat = mf.GetComponent<MeshRenderer>().sharedMaterial;
                if (mf.name.StartsWith("Vo") && mat.HasProperty("_TintColor")) { tb += mat.GetColor("_TintColor"); soVo++; }
            }
            if (soVo > 0) tb /= soVo;

            // DOC XOAN cua dai gio: do ngoai Play (DoDocXoanNgoaiPlay)
            int docTang = docTangEditor, docGiam = docGiamEditor;

            // CHIEU QUAY THAT cua tung lop: goc cua truc x cuc bo sau 0,1 s; CHIEU TRUOT anh
            var gocTruoc = new Dictionary<Transform, float>();
            var sps = loc.GetComponentsInChildren<Spin>();
            foreach (var sp in sps) { var x = sp.transform.right; gocTruoc[sp.transform] = Mathf.Atan2(x.z, x.x) * Mathf.Rad2Deg; }

            // Hat khoi bui cuon: chup vi tri cuc bo theo randomSeed
            var psBui = loc.transform.Find("GioLocHinh/BuiCuon").GetComponent<ParticleSystem>();
            yield return new WaitForSeconds(0.5f);
            var hat0 = new ParticleSystem.Particle[psBui.main.maxParticles];
            int n0 = psBui.GetParticles(hat0);
            var cuHat = new Dictionary<uint, Vector3>();
            // VONG PHUN: nhan ban he hat, tat moi van toc, phun 200 hat mot luc roi doc ngay - lech doc va ban kinh luc sinh
            var ban = Object.Instantiate(psBui.gameObject, psBui.transform.parent);
            var psBan = ban.GetComponent<ParticleSystem>();
            psBan.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var vBan = psBan.velocityOverLifetime; vBan.enabled = false;
            var mBan = psBan.main; mBan.startSpeed = 0f; mBan.maxParticles = 300;
            var eBan = psBan.emission; eBan.enabled = false;
            psBan.Emit(200);
            var hBan = new ParticleSystem.Particle[300];
            int nBan = psBan.GetParticles(hBan);
            float yMaxLucSinh = 0f, rMaxLucSinh = 0f; int soSinhMoi = nBan;
            for (int i = 0; i < nBan; i++)
            {
                yMaxLucSinh = Mathf.Max(yMaxLucSinh, Mathf.Abs(hBan[i].position.y));
                rMaxLucSinh = Mathf.Max(rMaxLucSinh, new Vector2(hBan[i].position.x, hBan[i].position.z).magnitude);
            }
            Object.Destroy(ban);
            for (int i = 0; i < n0; i++)
            {
                cuHat[hat0[i].randomSeed] = hat0[i].position;
            }
            foreach (var sp in sps) { var x = sp.transform.right; gocTruoc[sp.transform] = Mathf.Atan2(x.z, x.x) * Mathf.Rad2Deg; }
            yield return new WaitForSeconds(0.1f);
            int quayTang = 0, quayGiam = 0;
            foreach (var sp in sps)
            {
                var x = sp.transform.right;
                float dg = Mathf.DeltaAngle(gocTruoc[sp.transform], Mathf.Atan2(x.z, x.x) * Mathf.Rad2Deg);
                if (dg > 0f) quayTang++; else quayGiam++;
            }
            int truotLen = 0, truotSai = 0;
            foreach (var su in loc.GetComponentsInChildren<ScrollUV>())
            {
                bool dung = su.name == "DaiGio" ? su.speed.x < 0f : su.speed.y < 0f;
                if (dung) truotLen++; else truotSai++;
            }
            var hat1 = new ParticleSystem.Particle[psBui.main.maxParticles];
            int n1 = psBui.GetParticles(hat1);
            int hatLen = 0, hatXuong = 0, hatCungChieu = 0, hatNguoc = 0;
            for (int i = 0; i < n1; i++)
            {
                Vector3 p0;
                if (!cuHat.TryGetValue(hat1[i].randomSeed, out p0)) continue;
                Vector3 p1 = hat1[i].position;
                if (p1.y > p0.y) hatLen++; else hatXuong++;
                float dg = Mathf.DeltaAngle(Mathf.Atan2(p0.z, p0.x) * Mathf.Rad2Deg, Mathf.Atan2(p1.z, p1.x) * Mathf.Rad2Deg);
                if (dg > 0f) hatCungChieu++; else hatNguoc++;
            }
            // Vet bui phia sau: he hat khong gian the gioi
            var psVet = loc.transform.Find("GioLocHinh/KhoiBui").GetComponent<ParticleSystem>();
            int tongHat = 0;
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>()) tongHat += ps.particleCount;

            yield return Chup("gioloc_1_bay");

            Vector3 p0v = loc.transform.position; float t0 = Time.time;
            yield return new WaitForSeconds(1f);
            Vector3 p1v = loc.transform.position; float t1 = Time.time;
            float toc = new Vector2(p1v.x - p0v.x, p1v.z - p0v.z).magnitude / (t1 - t0);

            float batDau = lucSinh;
            Vector3 cu = loc.transform.position; float lucDiCuoi = Time.time;
            int soArc = 0;
            foreach (var aNew in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude)) if (arcTruoc.Add(aNew)) soArc++;
            float hanS = Time.time + 2.6f;
            while (Time.time < hanS && loc != null)
            {
                foreach (var aNew in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                    if (arcTruoc.Add(aNew)) soArc++;
                if ((loc.transform.position - cu).sqrMagnitude > 1e-6f) { lucDiCuoi = Time.time; cu = loc.transform.position; }
                yield return null;
            }
            float song = lucDiCuoi - batDau;
            Ghi(string.Format("C. luoi: {0}; cao hinh loc nho {1:F2} m, Loc xoay that {2:F2} m, ti le {3:F2}; mau vo trung binh ({4:F2}, {5:F2}, {6:F2}); den {7}; tia set {8}",
                string.Join(", ", tenLuoi.ToArray()), caoNho, caoLon, caoNho / caoLon, tb.r, tb.g, tb.b, soDen, soArc));
            Ghi(string.Format("C. xoan: dai gio len cao goc tang {0} / giam {1}; quay that sau 0,1 s: goc tang {2} lop / giam {3} lop; truot anh len {4} lop / sai {5}",
                docTang, docGiam, quayTang, quayGiam, truotLen, truotSai));
            Ghi(string.Format("C. khoi bui cuon: {0} hat so sanh duoc - bay len {1} / xuong {2}, quay goc tang {3} / giam {4}; vong phun ({6} hat phun thu, tat van toc) lech doc toi da {5:F3} m, ban kinh toi da {9:F2} m; vet phia sau khong gian {7}; tong hat mot loc {8}",
                hatLen + hatXuong, hatLen, hatXuong, hatCungChieu, hatNguoc, yMaxLucSinh, soSinhMoi, psVet.main.simulationSpace, tongHat, rMaxLucSinh));
            Ghi(string.Format("D. toc do do {0:F2} m/s; ngung di sau {1:F2} s", toc, song));
            Kiem(tenLuoi.Count == 4 && vo0 != null && dai != null, "khong dung luoi Blender (Vo0-2, DaiGio)");
            Kiem(Mathf.Abs(caoNho - 5f) < 0.35f && caoNho / caoLon < 0.5f, "loc nho khong cao ~5 m");
            Kiem(Mathf.Abs(tb.r - tb.b) < 0.12f && tb.r > 0.75f, "vo loc khong xam trang nhu Loc xoay");
            Kiem(soDen == 0, "loc nho con den diem");
            Kiem(soArc == 0, "loc nho van con tia set");
            // Vet gio "goc giam khi len cao" + quay lam goc TANG = vet chay LEN
            bool xoanGiam = docGiam > 50 && docTang == 0;
            Kiem(xoanGiam && quayTang == sps.Length && quayGiam == 0, "cac lop khong xoay cung mot chieu di len");
            Kiem(truotSai == 0 && truotLen == sps.Length, "anh gio truot khong cung chieu di len");
            Kiem(hatLen + hatXuong > 10 && hatXuong == 0, "khoi bui khong bay len");
            Kiem(hatCungChieu > (hatCungChieu + hatNguoc) * 0.9f, "khoi bui khong cuon cung chieu than loc");
            Kiem(soSinhMoi >= 150 && yMaxLucSinh < 0.02f && rMaxLucSinh > 0.3f, "vong phun khoi bui khong nam ngang tren dat");
            Kiem(psVet.main.simulationSpace == ParticleSystemSimulationSpace.World, "khong co vet bui o lai phia sau");
            Kiem(Mathf.Abs(toc - 8f) < 0.35f, "toc do loc khong phai 8 m/s");
            Kiem(Mathf.Abs(song - 3.5f) < 0.12f, "loc khong tan sau 3,5 giay");
            float hanX = Time.time + 7f;
            while (loc != null && Time.time < hanX) yield return null;
            Kiem(loc == null, "loc tan roi ma vat the khong bi xoa");
        }

        // ================= E. XUYEN VAT CAN =================
        Ghi("");
        {
            BoxCollider bia = null; Vector3 tu = Vector3.zero, huongE = Vector3.zero;
            foreach (var c in Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude))
            {
                if (c.gameObject.layer != 0 || !c.name.StartsWith("TS_")) continue;
                if (Vector3.Distance(c.transform.position, goc) > 40f) continue;
                Vector3 h = (c.bounds.center - goc); h.y = 0f; h.Normalize();
                bia = c; huongE = h; tu = c.bounds.center - h * 6f; break;
            }
            if (bia == null) { Ghi("[LOI] khong tim thay bia mo de thu xuyen"); loi++; }
            else
            {
                var loc = GioLoc.Spawn(tu, huongE, maskEnemy);
                float xaNhat = 0f;
                float hanE = Time.time + 1.9f;
                while (Time.time < hanE && loc != null)
                {
                    Vector3 v = loc.transform.position - tu; v.y = 0f;
                    xaNhat = Mathf.Max(xaNhat, Vector3.Dot(v, huongE));
                    yield return null;
                }
                // Doi chung: tia thang doc duong ay co cham bia that khong
                bool coCham = Physics.Raycast(tu, huongE, 12f, 1 << 0, QueryTriggerInteraction.Ignore);
                Ghi(string.Format("E. bia {0} nam giua duong (tia doi chung cham vat can: {1}); loc di duoc {2:F1} m doc huong trong 1,9 s (bia o 6 m)", bia.name, coCham, xaNhat));
                Kiem(coCham, "doi chung: duong thu khong co vat can - phep do vo nghia");
                Kiem(xaNhat > 12f, "loc bi vat can chan lai");
                if (loc != null) Object.Destroy(loc.gameObject);
            }
        }

        // ================= F. SAT THUONG + VUNG TRUNG =================
        Ghi("");
        {
            Vector3 tu = goc + huong * 1.2f;
            var vuong = Vector3.Cross(Vector3.up, huong).normalized;
            var bGiua = TaoBia("TAM_FGiua", tu + huong * 10f);
            var b25 = TaoBia("TAM_F25", tu + huong * 14f + vuong * 2.5f);
            var b27 = TaoBia("TAM_F27", tu + huong * 18f - vuong * 2.7f);
            yield return new WaitForFixedUpdate();
            float m1 = bGiua.health, m25 = b25.health, m27 = b27.health;
            var loc = GioLoc.Spawn(tu, huong, maskEnemy);
            float xaSauTrung = 0f;
            float hanF = Time.time + 3.8f;
            while (Time.time < hanF && loc != null)
            {
                Vector3 v = loc.transform.position - tu; v.y = 0f;
                xaSauTrung = Mathf.Max(xaSauTrung, Vector3.Dot(v, huong));
                yield return null;
            }
            // Ghi so mat mau TRUOC khi ba loc sau bay qua (cung duong, se trung them cac bia tren)
            float matGiua = m1 - bGiua.health, mat25 = m25 - b25.health, mat27 = m27 - b27.health;
            // Bia truoc mat nhan vat 4 m: ca ba loc cung trung
            var bBa = TaoBia("TAM_FBa", goc + huong * 4f);
            yield return new WaitForFixedUpdate();
            float mBa = bBa.health;
            GioLoc.SpawnChum(goc + huong * 1.2f, huong, maskEnemy, mauToi);
            yield return new WaitForSeconds(1f);
            Ghi(string.Format("F. mot loc qua bia dung yen mat {0:F0}; bia lech 2,5 m mat {1:F0}; bia lech 2,7 m mat {2:F0}; loc di tiep sau khi trung toi {3:F1} m; ba loc cung qua mot bia mat {4:F0}",
                matGiua, mat25, mat27, xaSauTrung, mBa - bBa.health));
            Kiem(Mathf.Abs(matGiua - 75f) < 0.5f, "mot loc khong gay dung 75 mot lan");
            Kiem(Mathf.Abs(mat25 - 75f) < 0.5f && mat27 < 0.5f, "vung trung 2,2 m khong dung");
            Kiem(xaSauTrung > 25f, "loc dung lai sau khi trung ke dich");
            Kiem(Mathf.Abs((mBa - bBa.health) - 225f) < 0.5f, "ba loc cung trung khong ra 225");
            foreach (var b in new[] { bGiua, b25, b27, bBa }) Object.Destroy(b.gameObject);
            XoaLoc();
            yield return new WaitForSeconds(0.3f);
        }

        // ================= G. HAT TUNG =================
        Ghi("");
        {
            var hang = new List<Damageable>();
            var bay = new Dictionary<Damageable, float>();
            var tu = goc + huong * 1.2f;
            // 8 m/s x 3,5 s = 28 m: 19 bia cach 1,3 m tu 4 m toi 27,4 m
            for (int i = 0; i < 19; i++) hang.Add(TaoBia("TAM_G" + i, tu + huong * (4f + i * 1.3f)));
            yield return new WaitForFixedUpdate();
            int soTrung = 0, soHat = 0; float caoMax = 0f; float tongGiay = 0f; int soDoGiay = 0;
            int hat0 = GioLoc.SoLanHat, trung0 = GioLoc.SoLanTrung;
            bool daChupHat = false;
            for (int lan = 0; lan < 10; lan++)
            {
                var loc = GioLoc.Spawn(tu, huong, maskEnemy);
                var dangCo = new HashSet<Damageable>();
                var batDau = new Dictionary<Damageable, float>();
                float hanG = Time.time + 4.2f;
                while (Time.time < hanG)
                {
                    foreach (var d in hang)
                    {
                        var h = d.GetComponent<BiHatTung>();
                        var hinh = d.transform.Find("Hinh");
                        if (hinh != null) caoMax = Mathf.Max(caoMax, hinh.localPosition.y - 1f);
                        if (h != null && !dangCo.Contains(d))
                        {
                            dangCo.Add(d); batDau[d] = Time.time; soHat++;
                            if (!daChupHat && d == hang[1]) { daChupHat = true; }
                        }
                        if (h == null && batDau.ContainsKey(d))
                        {
                            tongGiay += Time.time - batDau[d]; soDoGiay++;
                            batDau.Remove(d);
                        }
                    }
                    yield return null;
                }
                if (loc != null) Object.Destroy(loc.gameObject);
            }
            soTrung = GioLoc.SoLanTrung - trung0;
            float tile = soTrung > 0 ? soHat / (float)soTrung : 0f;
            Ghi(string.Format("G. {0} lan loc trung bia; dem doc lap {1} lan bia co BiHatTung ({2:P1}), bo dem trong code {3}; do cao hinh lon nhat {4:F2} m; bay trung binh {5:F2} s ({6} lan do)",
                soTrung, soHat, tile, GioLoc.SoLanHat - hat0, caoMax, soDoGiay > 0 ? tongGiay / soDoGiay : 0f, soDoGiay));
            Kiem(soTrung == 190, "khong du 190 lan trung (19 bia x 10 loc)");
            Kiem(tile > 0.47f && tile < 0.63f, "ti le hat tung khong quanh 55%");
            Kiem(soHat == GioLoc.SoLanHat - hat0, "dem doc lap khac bo dem trong code");
            Kiem(Mathf.Abs(caoMax - 1.5f) < 0.08f, "do cao hat tung khong phai 1,5 m");
            Kiem(soDoGiay > 50 && Mathf.Abs(tongGiay / soDoGiay - 0.5f) < 0.06f, "thoi gian bay khong phai 0,5 giay");
            foreach (var d in hang) Object.Destroy(d.gameObject);
            yield return new WaitForSeconds(0.3f);

            // Anh: mot bia dang bay len
            var biaAnh = TaoBia("TAM_GAnh", goc + huong * 7f);
            yield return new WaitForFixedUpdate();
            var locAnh = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
            locAnh.xacSuatHatTung = 1f;
            float hanA = Time.time + 1f;
            while (Time.time < hanA)
            {
                var h = biaAnh.GetComponent<BiHatTung>();
                if (h != null && h.daTroi > 0.2f) { yield return Chup("gioloc_2_hat_tung"); break; }
                yield return null;
            }
            Object.Destroy(biaAnh.gameObject);
            XoaLoc();

            // Khieng
            var biaK = TaoBia("TAM_GKhieng", goc + huong * 6f);
            biaK.khieng = Khieng.Bat(biaK.gameObject, 1e6f, 1.2f);
            yield return new WaitForFixedUpdate();
            float mK = biaK.health; int hatK = 0;
            for (int lan = 0; lan < 20; lan++)
            {
                var l = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
                l.xacSuatHatTung = 1f;
                float hanK = Time.time + 0.6f;
                bool coLan = false;
                while (Time.time < hanK) { if (biaK.GetComponent<BiHatTung>() != null) coLan = true; yield return null; }
                if (coLan) hatK++;
                Object.Destroy(l.gameObject);
            }
            Ghi(string.Format("G. bia co khieng, 20 loc (ti le hat 100%): hat tung {0} lan, mau mat {1:F0}", hatK, mK - biaK.health));
            Kiem(hatK == 0, "khieng khong chan hat tung");
            Object.Destroy(biaK.gameObject);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= H. NGAT CHIEU =================
        Ghi("");
        {
            // H1: nguoi choi dang niem Qua cau lua, bi hat (Apply that) -> khong qua nao
            int nNgat = 0, nDoiChung = 0, soNgatCode0 = PlayerController.SoLanNgatChieu;
            for (int lan = 0; lan < 3; lan++)
            {
                toi.mana = toi.maxMana;
                int truoc = DemCauLua(), maxMoi = 0;
                toi.CastAt(0, goc + huong * 10f);
                yield return null;
                BiHatTung.Apply(mauToi, 0.5f);
                bool khoa = toi.DangBiKhoaCung;
                float hanH = Time.time + 1f;
                while (Time.time < hanH) { maxMoi = Mathf.Max(maxMoi, DemCauLua() - truoc); yield return null; }
                if (maxMoi == 0 && khoa) nNgat++;
                yield return new WaitForSeconds(0.4f);

                toi.mana = toi.maxMana;
                truoc = DemCauLua(); maxMoi = 0;
                toi.CastAt(0, goc + huong * 10f);
                hanH = Time.time + 1f;
                while (Time.time < hanH) { maxMoi = Mathf.Max(maxMoi, DemCauLua() - truoc); yield return null; }
                if (maxMoi == 3) nDoiChung++;
                yield return new WaitForSeconds(0.4f);
            }
            Ghi(string.Format("H1. dang niem Qua cau lua bi hat tung: {0}/3 lan khong qua nao bay ra (bo dem ngat {1}); doi chung khong hat: {2}/3 lan ra du 3 qua",
                nNgat, PlayerController.SoLanNgatChieu - soNgatCode0, nDoiChung));
            Kiem(nNgat == 3 && nDoiChung == 3, "bi hat tung ma chieu dang niem khong bi ngat");

            // H2: loc THAT trung nguoi choi dang niem; bi hat thi CastAt bi tu choi
            toi.mana = toi.maxMana;
            int truocH2 = DemCauLua(), maxH2 = 0;
            toi.CastAt(0, goc + huong * 10f);
            var locH2 = GioLoc.Spawn(goc - huong * 3f, huong, LayerMask.GetMask("Player"));
            locH2.xacSuatHatTung = 1f;
            bool biHat = false; string nhac = null;
            float hanH2 = Time.time + 1f;
            while (Time.time < hanH2)
            {
                maxH2 = Mathf.Max(maxH2, DemCauLua() - truocH2);
                if (!biHat && toi.GetComponent<BiHatTung>() != null)
                {
                    biHat = true;
                    toi.mana = toi.maxMana;
                    yield return new WaitForSeconds(0.45f);   // het hoi chieu cau lua, van dang bay
                    if (toi.GetComponent<BiHatTung>() != null) { toi.CastAt(0, goc + huong * 10f); nhac = toi.LastMessage; }
                }
                yield return null;
            }
            Ghi(string.Format("H2. loc that trung nguoi dang niem: bi hat {0}, qua cau lua bay ra {1}; bam ky nang luc dang bay -> \"{2}\"", biHat, maxH2, nhac));
            Kiem(biHat && maxH2 == 0, "loc that trung nguoi dang niem ma chieu van ra");
            XoaLoc();
            yield return new WaitForSeconds(0.8f);

            // H3: QUAI dang ra don bi hat -> khong trung; doi chung -> trung
            var goQuai = EnemyFactory.Spawn(MonsterType.Skeleton, goc + huong * 1.6f, null, toi.transform);
            var ai = goQuai != null ? goQuai.GetComponent<EnemyAI>() : null;
            if (ai == null) { Ghi("[LOI] khong sinh duoc bo xuong"); loi++; }
            else
            {
                var mauQuai = goQuai.GetComponent<Damageable>();
                mauQuai.maxHealth = 1e7f; mauQuai.health = 1e7f;
                int donNgat = 0, donDoiChung = 0, lanNgat = 0, lanDoi = 0;
                int dongNgat0 = EnemyAI.SoLanNgatDon;
                for (int lan = 0; lan < 10; lan++)
                {
                    bool hat = lan % 2 == 0;
                    float hanQ = Time.time + 6f;
                    while (!ai.DangRaDonChuaTrung && Time.time < hanQ) yield return null;
                    if (!ai.DangRaDonChuaTrung) continue;
                    int soDon = 0;
                    System.Action<EnemyAI, int, Transform, Vector3> nghe = (e, k, t, p) => soDon++;
                    ai.DaRaDon += nghe;
                    float mTruoc = mauToi.health;
                    if (hat) BiHatTung.Apply(mauQuai, 0.5f);
                    yield return new WaitForSeconds(0.9f);
                    ai.DaRaDon -= nghe;
                    bool trung = soDon > 0 || mauToi.health < mTruoc - 0.1f;
                    if (hat) { lanNgat++; if (trung) donNgat++; } else { lanDoi++; if (trung) donDoiChung++; }
                }
                Ghi(string.Format("H3. bo xuong dang vung tay bi hat tung: {0}/{1} don van trung (bo dem ngat {2}); doi chung khong hat: {3}/{4} don trung",
                    donNgat, lanNgat, EnemyAI.SoLanNgatDon - dongNgat0, donDoiChung, lanDoi));
                Kiem(lanNgat >= 4 && donNgat == 0, "quai bi hat tung ma don van trung");
                Kiem(lanDoi >= 4 && donDoiChung == lanDoi, "doi chung: quai ra don ma khong trung - phep do vo nghia");
                Object.Destroy(goQuai);
            }
            mauToi.health = mauToi.maxHealth;
            yield return new WaitForSeconds(0.5f);
        }

        // ================= J. LO LUA (bat dau - kiem tra lan cuoi o cuoi phep thu) =================
        Ghi("");
        LoLuaDa loThu = null, loDoiChung = null;
        float lucDapTat = -1f;
        {
            var cacLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsInactive.Exclude);
            float ganNhat = 1e9f;
            foreach (var lo in cacLo)
            {
                float kc = Vector3.Distance(lo.transform.position, goc);
                if (lo.DangChay && kc < ganNhat) { ganNhat = kc; loThu = lo; }
            }
            if (loThu != null)
                foreach (var lo in cacLo)
                    if (lo != loThu && lo.DangChay && Vector3.Distance(lo.transform.position, loThu.transform.position) > 15f) { loDoiChung = lo; break; }
            if (loThu == null || loDoiChung == null) { Ghi("[LOI] Act2 khong co du lo lua dang chay"); loi++; }
            else
            {
                var h = Vector3.forward;
                var loc = GioLoc.Spawn(loThu.transform.position - h * 8f, h, maskEnemy);
                float hanJ = Time.time + 1.2f;
                while (Time.time < hanJ && loThu.DangChay) yield return null;
                lucDapTat = loThu.DangChay ? -1f : Time.time;
                Ghi(string.Format("J. loc luot qua lo {0}: lua tat {1}; lo doi chung {2} van chay {3}", loThu.name, !loThu.DangChay, loDoiChung.name, loDoiChung.DangChay));
                Kiem(!loThu.DangChay, "loc luot qua ma lo lua khong tat");
                Kiem(loDoiChung.DangChay, "lo doi chung o xa cung bi tat");
                yield return new WaitForSeconds(1.2f);
                if (loc != null) Object.Destroy(loc.gameObject);
            }
        }

        // ================= I. QUA MANG =================
        Ghi("");
        TranHienTai.DangChoiMang = true;
        var vuongI = Vector3.Cross(Vector3.up, huong).normalized;
        var kia = NguoiChoiKhac.Sinh("uid-gioloc", "Người bị hất", goc + vuongI * 6f);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi kia"); loi++; }
        else
        {
            var mauKia = kia.GetComponent<Damageable>();
            var goDb = new GameObject("TAM_DongBoGioLoc");
            var db = goDb.AddComponent<DongBoTran>();
            db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
            KenhTrucTiep.Tao(); KenhTrucTiep.NhanTraLoi("{}");
            var chay = Object.FindAnyObjectByType<ChayThuMang>();
            if (chay != null) chay.StartCoroutine(GiuSong());
            var daGui = new List<string>();
            KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
            yield return new WaitForSeconds(0.6f);
            System.Action<byte> nhetCo = co =>
            {
                var ds = new GoiTin.MotNguoi[1];
                ds[0] = new GoiTin.MotNguoi { chiSo = 1, viTri = kia.transform.position, gocY = kia.transform.eulerAngles.y, mau01 = 1f, coHieuUng = co };
                KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
            };

            // I1. goi ky nang 10
            int truocMang = DemLoc(), maxLoc = 0;
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            { chiSo = 1, kyNang = (byte)K, capKyNang = 1, soThuTu = 900, diemNgam = kia.transform.position + vuongI * 10f })));
            float hanI = Time.time + 1f;
            while (Time.time < hanI) { maxLoc = Mathf.Max(maxLoc, DemLoc() - truocMang); nhetCo(0); yield return null; }
            yield return new WaitForSeconds(0.5f);
            daGui.Clear();
            toi.mana = toi.maxMana;
            toi.CastAt(K, goc + huong * 8f);
            yield return null; yield return null;
            bool goi10 = false;
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s); GoiTin.MotPhep p;
                if (b != null && GoiTin.LoaiCuaGoi(b) == GoiTin.LoaiKyNang && GoiTin.DocKyNang(b, out p) && p.kyNang == K) goi10 = true;
            }
            Ghi(string.Format("I1. goi ky nang so 10 tu nguoi kia -> may minh phat lai {0} loc (mong 3); minh tung -> goi mang kyNang = 10: {1}", maxLoc, goi10));
            Kiem(maxLoc == 3, "may minh khong phat lai Gio loc cua nguoi kia");
            Kiem(goi10, "goi ky nang khong mang so 10");
            XoaLoc();
            yield return new WaitForSeconds(0.5f);

            // I2. minh bi hat -> goi trang thai minh gui di co bit hat tung
            daGui.Clear();
            BiHatTung.Apply(mauToi, 0.5f);
            float hanI2 = Time.time + 0.3f;
            while (Time.time < hanI2) { nhetCo(0); yield return null; }
            bool goiCoBit = false;
            var ra = new GoiTin.MotNguoi[4];
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s);
                if (b == null || GoiTin.LoaiCuaGoi(b) != GoiTin.LoaiTrangThai) continue;
                int moc; int n = GoiTin.DocTrangThai(b, ra, out moc);
                for (int i = 0; i < n; i++) if (ra[i].chiSo == 0 && (ra[i].coHieuUng & HieuUngQuaMang.CoHatTung) != 0) goiCoBit = true;
            }
            // Mat na ca hai goi: viet 0x1F doc ra 0x1F
            var ds1 = new GoiTin.MotNguoi[] { new GoiTin.MotNguoi { chiSo = 2, coHieuUng = 0x1F, mau01 = 1f } };
            var ra1 = new GoiTin.MotNguoi[1]; int mc;
            GoiTin.DocTrangThai(GoiTin.VietTrangThai(1, ds1, 1), ra1, out mc);
            var dq = new GoiTin.MotQuai[] { new GoiTin.MotQuai { id = 5, coHieuUng = 0x1F, mau01 = 1f } };
            var rq = new GoiTin.MotQuai[1];
            GoiTin.DocQuai(GoiTin.VietQuai(1, dq, 0, 1), rq, out mc);
            Ghi(string.Format("I2. minh bi hat -> goi trang thai gui di co bit hat tung: {0}; mat na: goi nguoi choi 0x1F -> 0x{1:X2}, goi quai 0x1F -> 0x{2:X2} (co chet quai {3})",
                goiCoBit, ra1[0].coHieuUng, rq[0].coHieuUng, rq[0].daChet));
            Kiem(goiCoBit, "bi hat tung ma goi trang thai khong bao cho may kia");
            Kiem(ra1[0].coHieuUng == 0x1F && rq[0].coHieuUng == 0x1F && !rq[0].daChet, "mat na goi tin cat mat bit hat tung");
            yield return new WaitForSeconds(0.6f);

            // I3. ban sao nhan bit -> bay len; goi tre sau khi roi xuong khong hat lai
            nhetCo(HieuUngQuaMang.CoHatTung);
            bool coHtKia = false;
            float caoKia = 0f;
            float hanI3 = Time.time + 1.2f;
            float lucHet = -1f;
            while (Time.time < hanI3)
            {
                var h = kia.GetComponent<BiHatTung>();
                if (h != null) { coHtKia = true; caoKia = Mathf.Max(caoKia, h.CaoHienTai); }
                if (h == null && coHtKia && lucHet < 0f) { lucHet = Time.time; nhetCo(HieuUngQuaMang.CoHatTung); }   // goi tre
                yield return null;
                nhetCo(0);
            }
            bool hatLaiLanHai = kia.GetComponent<BiHatTung>() != null;
            Ghi(string.Format("I3. ban sao nhan bit hat tung: bay len {0}, cao nhat {1:F2} m; goi 'dang bay' den tre ngay sau khi roi xuong -> hat lai lan hai: {2}",
                coHtKia, caoKia, hatLaiLanHai));
            Kiem(coHtKia && caoKia > 1.3f, "ban sao khong bay len khi nhan bit hat tung");
            Kiem(!hatLaiLanHai, "goi tre lam ban sao bi hat lai lan hai");
            yield return new WaitForSeconds(0.5f);

            // I4. ban sao dang niem Qua cau lua nhan bit -> khong phong; doi chung -> 3 qua
            int ngatBanSao = 0, raDoiChung = 0;
            for (int lan = 0; lan < 3; lan++)
            {
                for (int ca = 0; ca < 2; ca++)
                {
                    bool coBit = ca == 0;
                    int truoc = DemCauLua(), maxMoi = 0;
                    KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
                    { chiSo = 1, kyNang = 0, capKyNang = 1, soThuTu = 950 + lan * 2 + ca, diemNgam = kia.transform.position + vuongI * 12f })));
                    yield return null;
                    if (coBit) nhetCo(HieuUngQuaMang.CoHatTung);
                    float hanI4 = Time.time + 1f;
                    while (Time.time < hanI4) { maxMoi = Mathf.Max(maxMoi, DemCauLua() - truoc); yield return null; nhetCo(0); }
                    if (coBit && maxMoi == 0) ngatBanSao++;
                    if (!coBit && maxMoi == 3) raDoiChung++;
                    yield return new WaitForSeconds(0.6f);
                }
            }
            Ghi(string.Format("I4. ban sao dang niem Qua cau lua nhan bit hat tung: {0}/3 lan khong qua nao; doi chung khong bit: {1}/3 lan ra du 3 qua", ngatBanSao, raDoiChung));
            Kiem(ngatBanSao == 3 && raDoiChung == 3, "ban sao bi hat tung ma van phong phep");

            KenhTrucTiep.guiSangBenKia = null;
            KenhTrucTiep.Dong();
            Object.DestroyImmediate(goDb);
            NguoiChoiKhac.Bo(kia);
        }
        TranHienTai.DangChoiMang = false;

        // ================= J. LO LUA CHAY LAI =================
        if (loThu != null && lucDapTat > 0f)
        {
            while (Time.time < lucDapTat + 25f) yield return null;
            bool tat25 = !loThu.DangChay;
            while (Time.time < lucDapTat + 31f) yield return null;
            bool chay31 = loThu.DangChay;
            Ghi(string.Format("J. lo {0}: 25 giay sau van tat {1}; 31 giay sau chay lai {2}", loThu.name, tat25, chay31));
            Kiem(tat25 && chay31, "lo lua khong chay lai dung sau 30 giay");
        }

        toi.DaTungPhep -= dem;
        if (dir != null) dir.enabled = true;
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[GioLoc] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/gioloc.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_GioLoc", "TAM_DongBoGioLoc" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
