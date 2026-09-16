using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 68): KY NANG "QUA CAU BANG" (so hieu 9, 16/09/2026).
///
/// Nguoi dung: 3 qua cau bang nhu Qua cau lua, co luong khong khi lanh + vet bang phia sau;
/// sat thuong ban dau 65; hoi chieu 0,55 giay; 40% lam cham 50% trong 2 giay; no gay sat
/// thuong moi ke dich trong vung no. Hinh dung bang Blender MCP.
///
/// Do nhung gi (Play, Act2):
///   A. Tai nguyen Blender nap duoc: luoi (so tam giac), hai anh hat co kenh trong suot, icon.
///   B. Thong so THAT tren nhan vat (prefab khong de): hoi chieu 0,55, SachPhep.ThongSo khop.
///   C. Tung that bang CastAt(9): con khoa thi tu choi; mo khoa -> DaTungPhep, dung 3 qua bay
///      ra, tru nang luong, bam lai ngay bi tu choi vi hoi chieu, doi 0,6 giay thi tung duoc.
///   D. Trung bia truoc mat: bia mat mau (tren duong thang, doi chung bia dat LECH khoi duong
///      bay khong mat gi).
///   E. Sat thuong GOC: no ngay tai tam bia -> mat dung 65; vung no: bia 1 m va 2,5 m trung,
///      bia 5 m ngoai ban kinh khong trung.
///   F. Lam cham: gieo 1000 lan -> ti le ~40% (doc lap: dem bia co FrozenEffect), slow = 0,5,
///      con 2 giay, KHONG dong cung; cap ky nang 3 keo dai them 0,30 giay.
///   G. Hinh anh luc bay: loi dung luoi Blender, DUOI GAI nam SAU huong bay, co luong khi lanh
///      (hat dang phat), vet bang (TrailRenderer), anh sang; no xong luong khi lanh + vet bang
///      con o lai tan dan (khong bien mat cung qua cau). Chup anh.
///   H. Qua mang: nguoi kia tung Qua cau bang bang GOI KY NANG that (kyNang = 9) -> may minh
///      phat lai ra 3 qua; minh tung thi goi gui di mang kyNang = 9.
///   I. HUD / Sach phep: BoIcon co 10 hinh, hinh so 9 la icon Blender; ten, tom tat co dau.
///
/// Ket qua: PlayTestShots/quacaubang.txt, anh quacaubang_*.png.
/// </summary>
public static class ThuQuaCauBang
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyQuaCauBang;

    [MenuItem("Diablo 2.5D/68. Chay thu QUA CAU BANG (ky nang moi)", false, 156)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu qua cau bang", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Qua cau bang");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_QuaCauBang") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_QuaCauBang");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[QuaCauBang] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static Damageable TaoBia(string ten, Vector3 p)
    {
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 10000000f; d.health = 10000000f;
        return d;
    }

    static void XoaHieuUng(Damageable d)
    {
        var f = d.GetComponent<FrozenEffect>(); if (f != null) Object.DestroyImmediate(f);
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
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoCauBang") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>Tim mot huong TRONG (khong vuong vat can trong 12 m) quanh nhan vat.</summary>
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

    /// <summary>Chup can canh quanh mot qua cau (phong 3 lan) de xem hinh pha le + luong khi lanh.</summary>
    static IEnumerator ChupCan(QuaCauBang q, string ten)
    {
        if (q == null) yield break;
        Vector3 w = q.transform.position - q.transform.forward * 0.8f;
        yield return new WaitForEndOfFrame();
        if (q == null) yield break;
        var cam = Camera.main;
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        var sp = cam.WorldToScreenPoint(w);
        int co = Mathf.RoundToInt(Screen.height * 0.30f);
        int x0 = Mathf.Clamp((int)sp.x - co / 2, 0, tex.width - co), y0 = Mathf.Clamp((int)sp.y - co / 2, 0, tex.height - co);
        var cat = new Texture2D(co * 3, co * 3, TextureFormat.RGB24, false);
        for (int y = 0; y < co * 3; y++)
            for (int x = 0; x < co * 3; x++)
                cat.SetPixel(x, y, tex.GetPixelBilinear((x0 + x / 3f) / tex.width, (y0 + y / 3f) / tex.height));
        File.WriteAllBytes("PlayTestShots/" + ten + ".png", cat.EncodeToPNG());
        Object.Destroy(tex); Object.Destroy(cat);
    }

    static int DemQua() { return Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude).Length; }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);
        int maskEnemy = LayerMask.GetMask("Enemy");

        // ================= A. TAI NGUYEN BLENDER =================
        Ghi("");
        var luoi = VfxFactory.LuoiQuaCauBang;
        var tSuong = Resources.Load<Texture2D>("KyNang/QuaCauBang/SuongLanh");
        var tManh = Resources.Load<Texture2D>("KyNang/QuaCauBang/ManhBang");
        var tIcon = Resources.Load<Texture2D>("Icons/CauBang");
        Ghi(string.Format("A. luoi Blender: {0} tam giac; SuongLanh {1} (alpha {2}); ManhBang {3} (alpha {4}); icon {5}",
            luoi != null ? (int)(luoi.GetIndexCount(0) / 3) : -1,
            tSuong != null ? tSuong.width + "x" + tSuong.height : "KHONG", tSuong != null && GraphicsFormatUtilityCoAlpha(tSuong),
            tManh != null ? tManh.width + "x" + tManh.height : "KHONG", tManh != null && GraphicsFormatUtilityCoAlpha(tManh),
            tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG"));
        // GetIndexCount chu khong .triangles: luoi khong bat Read/Write (khong can - chi de ve), trong Play .triangles tra rong
        Kiem(luoi != null && luoi.GetIndexCount(0) > 300, "khong nap duoc luoi qua cau bang tu Blender");
        Kiem(tSuong != null && tManh != null && tIcon != null, "thieu anh hat / icon");

        // ================= B. THONG SO THAT =================
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        Ghi(string.Format("B. tren nhan vat: nang luong {0}, hoi chieu {1} giay, niem {2}; Sach phep doc ra {3}/{4}/{5}; sat thuong goc {6}, cham {7:P0} x{8} trong {9} giay, ban kinh no {10} m",
            toi.quaCauBangCost, toi.quaCauBangCooldown, toi.quaCauBangCastTime, nl, hc, nc,
            QuaCauBang.SatThuongGoc, QuaCauBang.XacSuatCham, QuaCauBang.TiLeCham, QuaCauBang.GiayCham, QuaCauBang.BanKinhNo));
        Kiem(Mathf.Approximately(toi.quaCauBangCooldown, 0.55f) && Mathf.Approximately(hc, 0.55f), "hoi chieu khong phai 0,55 giay");
        Kiem(Mathf.Approximately(QuaCauBang.SatThuongGoc, 65f), "sat thuong goc khong phai 65");

        // ================= C. TUNG THAT =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;
        int soPhep = 0; int kyVuaTung = -1;
        System.Action<int, Vector3> dem = (s, a) => { soPhep++; kyVuaTung = s; };
        toi.DaTungPhep += dem;

        CapDo.BatDauTranMoi();
        int truocKhoa = soPhep;
        toi.CastAt(K, goc + huong * 8f);
        bool tuChoiKhoa = soPhep == truocKhoa;
        string nhacKhoa = toi.LastMessage;
        CapDo.MoKhoa(K);

        var biaDuong = TaoBia("TAM_BiaDuong", goc + huong * 8f);
        var vuong = Vector3.Cross(Vector3.up, huong).normalized;
        var biaLech = TaoBia("TAM_BiaLech", goc + huong * 8f + vuong * 7f);
        yield return new WaitForSeconds(0.3f);

        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana;
        int quaTruoc = DemQua();
        float mauDuongTruoc = biaDuong.health, mauLechTruoc = biaLech.health;
        int noTruoc = QuaCauBang.SoLanNo;
        toi.CastAt(K, biaDuong.transform.position);
        bool daTung = soPhep == truocKhoa + 1 && kyVuaTung == K;
        float manaTon = manaTruoc - toi.mana;
        // bam lai ngay: hoi chieu
        int soTruocBamLai = soPhep;
        yield return new WaitForSeconds(0.45f);         // qua het thoi gian niem, van trong hoi chieu
        int soQuaBay = DemQua() - quaTruoc;
        toi.CastAt(K, biaDuong.transform.position);
        bool tuChoiHoiChieu = soPhep == soTruocBamLai;
        string nhacHoiChieu = toi.LastMessage;

        // G. hinh anh luc dang bay
        var mot = Object.FindAnyObjectByType<QuaCauBang>();
        string hinh = "khong co qua dang bay";
        bool duoiSau = false, coSuong = false, coVet = false, coDen = false, dungLuoi = false;
        if (mot != null)
        {
            var mfLoi = mot.transform.Find("LoiBang");
            var mr = mfLoi != null ? mfLoi.GetComponent<MeshRenderer>() : null;
            var mf = mfLoi != null ? mfLoi.GetComponent<MeshFilter>() : null;
            dungLuoi = mf != null && mf.sharedMesh == luoi;
            if (mr != null)
            {
                Vector3 lech = mr.bounds.center - mot.transform.position;
                duoiSau = Vector3.Dot(lech, mot.transform.forward) < -0.02f;
                hinh = "tam hop bao lech theo huong bay " + Vector3.Dot(lech, mot.transform.forward).ToString("F3") + " m";
            }
            var ps = mot.transform.Find("LuongKhiLanh");
            coSuong = ps != null && ps.GetComponent<ParticleSystem>().particleCount > 0;
            coVet = mot.GetComponentInChildren<TrailRenderer>() != null;
            coDen = mot.GetComponentInChildren<Light>() != null;
            yield return ChupCan(mot, "quacaubang_1b_bay_can");
            yield return Chup("quacaubang_1_bay");
        }
        // cho no xong
        float hanNo = Time.time + 3f;
        while (QuaCauBang.SoLanNo < noTruoc + 3 && Time.time < hanNo) yield return null;
        int suongConLai = 0;
        foreach (var p in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude))
            if (p.name == "LuongKhiLanh" && p.transform.parent == null) suongConLai++;
        yield return Chup("quacaubang_2_no");
        yield return new WaitForSeconds(0.2f);
        float matDuong = mauDuongTruoc - biaDuong.health, matLech = mauLechTruoc - biaLech.health;

        yield return new WaitForSeconds(0.4f);
        int soTruocSau = soPhep;
        toi.mana = toi.maxMana;
        toi.CastAt(K, biaDuong.transform.position);
        bool tungLaiDuoc = soPhep == soTruocSau + 1;

        Ghi(string.Format("C. con khoa: tu choi {0} (\"{1}\"); mo khoa roi tung: DaTungPhep {2}, so qua bay ra {3}, ton nang luong {4:F1}; bam lai sau 0,45 giay: tu choi {5} (\"{6}\"); sau 0,6+ giay: tung duoc {7}",
            tuChoiKhoa, nhacKhoa, daTung, soQuaBay, manaTon, tuChoiHoiChieu, nhacHoiChieu, tungLaiDuoc));
        Kiem(tuChoiKhoa, "ky nang con khoa ma van tung duoc");
        Kiem(daTung && soQuaBay == 3, "tung Qua cau bang khong ra dung 3 qua");
        Kiem(Mathf.Abs(manaTon - toi.quaCauBangCost) < 0.01f, "ton nang luong sai");
        Kiem(tuChoiHoiChieu, "chua het hoi chieu ma van tung duoc");
        Kiem(tungLaiDuoc, "het hoi chieu 0,55 giay ma khong tung duoc");
        Ghi(string.Format("D. bia tren duong bay (8 m) mat {0:F1} mau; bia lech 7 m khoi duong bay mat {1:F1}", matDuong, matLech));
        Kiem(matDuong >= 65f * 0.55f - 0.5f && matDuong <= 3f * 65f + 0.5f, "bia tren duong bay khong mat mau dung khoang cua 1..3 qua");
        Kiem(matLech < 0.5f, "bia nam ngoai duong bay cung mat mau");
        Ghi(string.Format("G. luc bay: dung luoi Blender {0}, {1} -> duoi gai phia SAU {2}; luong khi lanh dang phat {3}, vet bang {4}, anh sang {5}; sau khi no: {6} luong khi lanh con o lai tan dan",
            dungLuoi, hinh, duoiSau, coSuong, coVet, coDen, suongConLai));
        Kiem(dungLuoi && duoiSau && coSuong && coVet && coDen, "hinh anh qua cau bang dang bay thieu phan");
        Kiem(suongConLai >= 1, "no xong luong khi lanh bien mat cung qua cau");
        toi.DaTungPhep -= dem;
        yield return new WaitForSeconds(1.5f);

        // ================= E. SAT THUONG GOC + VUNG NO =================
        Ghi("");
        Vector3 tam = goc + huong * 20f + Vector3.up * 50f;     // xa canh vat, tren khong
        var b0 = TaoBia("TAM_Bia0", tam);
        var b1 = TaoBia("TAM_Bia1", tam + vuong * 1f);
        var b25 = TaoBia("TAM_Bia25", tam + vuong * 2.5f);
        var b5 = TaoBia("TAM_Bia5", tam + vuong * 5f);
        yield return new WaitForFixedUpdate();
        float m0 = b0.health, m1 = b1.health, m25 = b25.health, m5 = b5.health;
        int soTrung = QuaCauBang.NoBang(tam, QuaCauBang.BanKinhNo, QuaCauBang.SatThuongGoc, maskEnemy, QuaCauBang.GiayCham, null);
        Ghi(string.Format("E. no tai tam bia: trung {0} muc tieu; mat mau: tam {1:F2} (mong 65), 1 m {2:F2}, 2,5 m {3:F2}, 5 m {4:F2} (ngoai ban kinh 3,4)",
            soTrung, m0 - b0.health, m1 - b1.health, m25 - b25.health, m5 - b5.health));
        Kiem(Mathf.Abs((m0 - b0.health) - 65f) < 0.01f, "no dung tam khong mat dung 65");
        Kiem(m1 - b1.health > 1f && m25 - b25.health > 1f, "ke dich trong vung no khong mat mau");
        Kiem(m5 - b5.health < 0.01f, "ke dich ngoai vung no van mat mau");
        Object.Destroy(b1.gameObject); Object.Destroy(b25.gameObject); Object.Destroy(b5.gameObject);

        // ================= F. LAM CHAM =================
        yield return new WaitForFixedUpdate();
        int soCham = 0, soDongCung = 0; float slowMin = 1f, slowMax = 0f, conMin = 99f, conMax = 0f;
        const int N = 1000;
        for (int i = 0; i < N; i++)
        {
            XoaHieuUng(b0); b0.health = b0.maxHealth;
            QuaCauBang.NoBang(tam, QuaCauBang.BanKinhNo, QuaCauBang.SatThuongGoc, maskEnemy, QuaCauBang.GiayCham, null);
            var f = b0.GetComponent<FrozenEffect>();
            if (f == null) continue;
            soCham++;
            if (f.IsFullyFrozen) soDongCung++;
            slowMin = Mathf.Min(slowMin, f.slow); slowMax = Mathf.Max(slowMax, f.slow);
            conMin = Mathf.Min(conMin, f.remaining); conMax = Mathf.Max(conMax, f.remaining);
        }
        XoaHieuUng(b0);
        float tl = 100f * soCham / N;
        // cap 3: giay cham + 2 * 0,15
        float them = CapDo.ThemGiayHieuUngTheoCap(3);
        QuaCauBang.SpawnChum(tam + Vector3.up * 5f, Vector3.down, 0, maskEnemy, null, 1, 0f, 1f, them);
        var quaCap3 = Object.FindAnyObjectByType<QuaCauBang>();
        float giayCap3 = quaCap3 != null ? quaCap3.giayCham : -1f;
        if (quaCap3 != null) Object.Destroy(quaCap3.gameObject);
        Ghi(string.Format("F. {0} lan no: bi cham {1:F1}% (mong 40%), dong cung {2}; slow {3}..{4} (mong 0,5), thoi gian cham {5:F2}..{6:F2} giay (mong 2); qua cau cap 3 cham {7:F2} giay (mong 2,30)",
            N, tl, soDongCung, slowMin, slowMax, conMin, conMax, giayCap3));
        Kiem(Mathf.Abs(tl - 40f) <= 5f, "ti le lam cham lech qua xa 40%");
        Kiem(soDongCung == 0, "qua cau bang lai dong cung (chi duoc lam cham)");
        Kiem(Mathf.Approximately(slowMin, 0.5f) && Mathf.Approximately(slowMax, 0.5f), "muc cham khong phai 50%");
        Kiem(Mathf.Abs(conMin - 2f) < 0.01f && Mathf.Abs(conMax - 2f) < 0.01f, "lam cham khong keo dai 2 giay");
        Kiem(Mathf.Abs(giayCap3 - 2.30f) < 0.01f, "cap ky nang khong keo dai lop cham");
        Object.Destroy(b0.gameObject);

        // ================= H. QUA MANG =================
        Ghi("");
        TranHienTai.DangChoiMang = true;
        var kia = NguoiChoiKhac.Sinh("uid-caubang", "Người ném băng", goc + vuong * 6f);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi kia"); loi++; }
        else
        {
            var goDb = new GameObject("TAM_DongBoCauBang");
            var db = goDb.AddComponent<DongBoTran>();
            db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
            KenhTrucTiep.Tao(); KenhTrucTiep.NhanTraLoi("{}");
            var chay = Object.FindAnyObjectByType<ChayThuMang>();
            if (chay != null) chay.StartCoroutine(GiuSong());
            var daGui = new List<string>();
            KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
            yield return new WaitForSeconds(0.6f);

            int truocMang = DemQua();
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            {
                chiSo = 1, kyNang = (byte)K, capKyNang = 1, soThuTu = 900, diemNgam = goc + huong * 8f
            })));
            int maxQua = 0;
            float hanM = Time.time + 1.0f;
            while (Time.time < hanM) { maxQua = Mathf.Max(maxQua, DemQua() - truocMang); yield return null; }

            yield return new WaitForSeconds(0.6f);
            daGui.Clear();
            toi.mana = toi.maxMana;
            toi.CastAt(K, goc + huong * 8f);
            yield return null; yield return null;
            bool goiCo9 = false;
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s);
                GoiTin.MotPhep p;
                if (b != null && GoiTin.LoaiCuaGoi(b) == GoiTin.LoaiKyNang && GoiTin.DocKyNang(b, out p) && p.kyNang == K) goiCo9 = true;
            }
            Ghi("H. goi ky nang so 9 tu nguoi kia -> may minh phat lai " + maxQua + " qua (mong 3); minh tung -> goi gui di mang kyNang = 9: " + goiCo9);
            Kiem(maxQua == 3, "may minh khong phat lai Qua cau bang cua nguoi kia");
            Kiem(goiCo9, "tung Qua cau bang ma goi ky nang khong mang so 9");
            KenhTrucTiep.guiSangBenKia = null;
            KenhTrucTiep.Dong();
            Object.DestroyImmediate(goDb);
            NguoiChoiKhac.Bo(kia);
        }

        // ================= I. HUD / SACH PHEP =================
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        Ghi(string.Format("I. BoIcon {0} hinh, hinh so 9 {1}; ten \"{2}\", tom tat \"{3}\"; mo ta {4} ky tu",
            bo != null ? bo.Length : -1, bo != null && bo.Length > 9 && bo[9] != null ? "co" : "KHONG",
            SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length));
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null, "HUD thieu icon Qua cau bang");
        Kiem(SachPhep.Ten(K) == "QUẢ CẦU BĂNG" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu cho Qua cau bang");
        yield return Chup("quacaubang_3_ket");

        foreach (var ten in new[] { "TAM_BiaDuong", "TAM_BiaLech" })
        {
            var g = GameObject.Find(ten); if (g != null) Object.Destroy(g);
        }
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static bool GraphicsFormatUtilityCoAlpha(Texture2D t)
    {
        return UnityEngine.Experimental.Rendering.GraphicsFormatUtility.HasAlphaChannel(t.graphicsFormat);
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[QuaCauBang] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/quacaubang.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_QuaCauBang", "TAM_DongBoCauBang" })
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
