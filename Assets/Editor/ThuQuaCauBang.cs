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
///   J. (16/09/2026, nguoi dung: "Mua bang - cho bang tao ra tren mat dat cung to nhu bang cua Qua cau
///      bang") DO KICH THUOC THAT cum gai bang: bat vat hieu ung moi sinh ra, lay hop bao cac gai (bo
///      vong phang tren dat) lon nhat trong 0,8 giay. Mua bang THAT (tang bang roi) vs Qua cau bang THAT
///      (vu no) vs doi chung co cu (IceImpact 1,7 m).
///   K. (16/09/2026, nguoi dung: "Mua bang - thay vi roi cac tang bang thi roi cac qua cau bang, co luong
///      khong khi lanh phia sau giong y chang Qua cau bang") Mua bang THAT: moi vat dang roi la qua cau bang
///      (luoi Blender, luong khi lanh dang phat, vet bang, duoi gai phia SAU huong roi, khong den rieng),
///      khong con tang bang cu; bia trong vung van mat mau + bi cham/dong cung; dem tong so hat; chup anh.
///   L. (16/09/2026, nguoi dung: qua cau cua Mua bang "chi trung mat dat thi no binh thuong khong tao khoi bang;
///      trung do vat / nguoi choi khac / quai thi vua no vua tao bang") - roi THAT tung ca, dem CumGai dang bat
///      trong hieu ung no: dat trong -> 0; bia mo trong 1,7 m -> co; ke dich trong 1,7 m -> co; DOI CHUNG bia mo
///      ngoai 1,7 m -> 0; cac phan con lai cua vu no (chop, vong, suong...) van co. Them thong ke mot con Mua bang that.
///   K (sua lai 17/09/2026, nguoi dung: Mua bang "bo khoi cau, cho cac vet sang bang rot tu tren troi xuong"; chon: chi bo
///      khoi cau, vet dai manh nhu sao bang, ve bang Blender MCP, cham dat giu nguyen): moi qua roi KHONG co MeshRenderer nao
///      (DOI CHUNG: qua cua ky nang Qua cau bang dung cung ham phai co luoi); vet dung anh VetSaoBang keo gian; do DO DAI THAT
///      cua vet (tong doan noi cac diem TrailRenderer) luc qua xuong thap, va ti le dai / rong; hao quang, luong khi lanh,
///      manh bang van phat; roi thang dung, gay sat thuong, lam cham nhu cu. Ky nang Qua cau bang (muc C/G) van luoi co gai.
///      Sua tiep cung ngay (nguoi dung: to them 20% be ngang; dau vet "giong nhu bi cat mat ngang" -> dau tron phat sang; to nho
///      ngau nhien 70%..100%): be rong THAT dau vet moi qua (widthCurve(0) x widthMultiplier) nam trong 0,462..0,66 m va phan tan;
///      dom sang tron DauSaoBang dang phat, co dung ti le; doc THANG file PNG tren dia (khong qua anh Unity da nen): cot dau vet
///      alpha ~0 (DOI CHUNG ban cu trong git la canh cat sang), anh dau tron sang o tam va tat o mep.
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

    static IEnumerator ChupCanDiem(Vector3 w, string ten)
    {
        yield return new WaitForEndOfFrame();
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

        // ================= J. CO CUM BANG DUOI DAT =================
        Ghi("");
        {
            foreach (var st in Object.FindObjectsByType<IceStorm>(FindObjectsInactive.Exclude)) Object.Destroy(st.gameObject);
            yield return new WaitForSeconds(2.5f);          // hieu ung cu tan het
            Vector3 cho = goc + huong * 6f;
            cho.y = VfxFactory.GroundY(cho);

            float[] rong = new float[3], cao = new float[3];
            bool daChupMua = false;
            string[] ten = { "Mua bang (tang bang that)", "Qua cau bang (vu no that)", "doi chung co cu IceImpact 1,7 m" };
            for (int ca = 0; ca < 3; ca++)
            {
                var truoc = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
                IceStorm bao = null;
                if (ca == 0) bao = IceStorm.Spawn(cho, maskEnemy);
                else if (ca == 1) VfxFactory.NoQuaCauBang(cho, QuaCauBang.BanKinhNo);
                else VfxFactory.IceImpact(cho, 1.7f);

                GameObject cum = null;
                float hanJ = Time.time + 4f;
                float tBat = -1f;
                while (Time.time < hanJ)
                {
                    if (cum == null)
                    {
                        foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                        {
                            if (truoc.Contains(g) || g == null) continue;
                            if (g.name.Contains("NoQuaCauBang") || g.GetComponentInChildren<IceStorm>() != null) continue;
                            if (!(g.name.Contains("NoBang") || g.name.Contains("IceImpact"))) continue;
                            cum = g; tBat = Time.time; break;
                        }
                    }
                    else
                    {
                        if (Time.time - tBat > 0.8f) break;
                        if (ca == 0 && !daChupMua && Time.time - tBat > 0.35f) { daChupMua = true; yield return Chup("quacaubang_4_muabang"); }
                        // Hop bao cac GAI (bo vong phang va hat): renderer luoi co chieu cao that
                        Bounds hop = new Bounds(cum.transform.position, Vector3.zero);
                        bool co = false;
                        foreach (var r in cum.GetComponentsInChildren<MeshRenderer>())
                        {
                            if (r.bounds.size.y < 0.15f) continue;
                            if (!co) { hop = r.bounds; co = true; } else hop.Encapsulate(r.bounds);
                        }
                        if (co)
                        {
                            rong[ca] = Mathf.Max(rong[ca], Mathf.Max(hop.size.x, hop.size.z));
                            cao[ca] = Mathf.Max(cao[ca], hop.max.y - cum.transform.position.y);
                        }
                    }
                    yield return null;
                }
                if (bao != null) Object.Destroy(bao.gameObject);
                yield return new WaitForSeconds(2.5f);
            }
            Ghi(string.Format("J. cum gai bang duoi dat (lon nhat trong 0,8 giay): {0}: rong {1:F2} m, cao {2:F2} m | {3}: rong {4:F2} m, cao {5:F2} m | {6}: rong {7:F2} m, cao {8:F2} m",
                ten[0], rong[0], cao[0], ten[1], rong[1], cao[1], ten[2], rong[2], cao[2]));
            float tiRong = rong[1] > 0 ? rong[0] / rong[1] : 0f, tiCao = cao[1] > 0 ? cao[0] / cao[1] : 0f;
            float doiChung = rong[1] > 0 ? rong[2] / rong[1] : 0f;
            Ghi(string.Format("    Mua bang / Qua cau bang: rong x{0:F2}, cao x{1:F2}; doi chung co cu / Qua cau bang: rong x{2:F2}", tiRong, tiCao, doiChung));
            Kiem(rong[0] > 0f && rong[1] > 0f && rong[2] > 0f, "khong bat duoc cum bang de do");
            Kiem(Mathf.Abs(tiRong - 1f) < 0.15f && Mathf.Abs(tiCao - 1f) < 0.15f, "cum bang cua Mua bang khong to bang cua Qua cau bang");
            Kiem(doiChung < 0.85f, "doi chung hong: co cu 1,7 m khong nho hon - phep do khong phan biet duoc");
        }

        // ================= K. MUA BANG ROI QUA CAU BANG =================
        Ghi("");
        {
            foreach (var st in Object.FindObjectsByType<IceStorm>(FindObjectsInactive.Exclude)) Object.Destroy(st.gameObject);
            yield return new WaitForSeconds(2.5f);
            Vector3 cho = goc + huong * 8f;
            cho.y = VfxFactory.GroundY(cho);
            var biaMua = TaoBia("TAM_BiaMua", cho);
            yield return new WaitForFixedUpdate();
            float mauMua0 = biaMua.health;
            var bao = IceStorm.Spawn(cho, maskEnemy);
            int soRoiMax = 0, soTangCu = 0, soCoSuong = 0, soCoVet = 0, soCoDen = 0, soXet = 0;
            int soThangDung = 0; float lechNgangMax = 0f;
            // DOI CHUNG: qua cua ky nang Qua cau bang dung cung ham (banRoi = false) phai co luoi
            var goDC = new GameObject("TAM_DoiChungLuoi");
            VfxFactory.BuildQuaCauBangVisual(goDC.transform, 0.43f, false);
            int luoiDoiChung = goDC.GetComponentsInChildren<MeshRenderer>().Length;
            Object.Destroy(goDC);
            int soKhongLuoi = 0, soVetSaoBang = 0, soHaoQuang = 0, soManh = 0, soDauTron = 0, soDauDungCo = 0;
            float rongMin = float.MaxValue, rongMax = 0f;
            var vetDaiNhat = new Dictionary<FallingShard, float>();
            int hatMax = 0; bool biCham = false, daChup = false;
            var daXet = new HashSet<FallingShard>();
            float hanK = Time.time + 5.5f;
            while (Time.time < hanK)
            {
                var roi = Object.FindObjectsByType<FallingShard>(FindObjectsInactive.Exclude);
                soRoiMax = Mathf.Max(soRoiMax, roi.Length);
                foreach (var f in roi)
                {
                    if (f.transform.Find("Tang") != null) soTangCu++;
                    if (daXet.Contains(f)) continue;
                    // xet moi qua khi no da roi duoc mot doan (luong khi lanh da kip phat)
                    float conCach = Vector3.Distance(f.transform.position, f.target);
                    if (conCach > 12f) continue;
                    // bo qua khung CHAM DAT: vet vua duoc tha ra va vi tri trung dich (lan do dau dem nham 1/34)
                    if (conCach < 0.6f) continue;
                    daXet.Add(f); soXet++;
                    // Nguoi dung 16/09/2026: roi THANG DUNG. Do lech ngang giua vi tri hien tai va diem roi.
                    float lechNgang = new Vector2(f.transform.position.x - f.target.x, f.transform.position.z - f.target.z).magnitude;
                    lechNgangMax = Mathf.Max(lechNgangMax, lechNgang);
                    if (lechNgang < 0.01f && Vector3.Angle(f.transform.forward, Vector3.down) < 0.5f) soThangDung++;
                    if (f.GetComponentsInChildren<MeshRenderer>().Length == 0) soKhongLuoi++;
                    var trv = f.GetComponentInChildren<TrailRenderer>();
                    if (trv != null && trv.sharedMaterial != null && trv.sharedMaterial.mainTexture != null
                        && trv.sharedMaterial.mainTexture.name == "VetSaoBang" && trv.textureMode == LineTextureMode.Stretch) soVetSaoBang++;
                    var hq = f.transform.Find("HaoQuang");
                    if (hq != null && hq.GetComponent<ParticleSystem>().isEmitting) soHaoQuang++;
                    if (trv != null)
                    {
                        float rong = trv.widthCurve.Evaluate(0f) * trv.widthMultiplier;
                        rongMin = Mathf.Min(rongMin, rong); rongMax = Mathf.Max(rongMax, rong);
                        var dau = f.transform.Find("DauSaoBang");
                        var dps = dau != null ? dau.GetComponent<ParticleSystem>() : null;
                        if (dps != null && dps.isEmitting && dps.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture.name == "DauSaoBang")
                        {
                            soDauTron++;
                            float co = (dps.main.startSize.constantMin + dps.main.startSize.constantMax) * 0.5f;
                            if (Mathf.Abs(co - rong * VfxFactory.HeSoDauSaoBang) < 0.02f) soDauDungCo++;
                        }
                    }
                    var mb = f.transform.Find("ManhBangRoi");
                    if (mb != null && mb.GetComponent<ParticleSystem>().isEmitting) soManh++;
                    var ps = f.transform.Find("LuongKhiLanh");
                    if (ps != null && ps.GetComponent<ParticleSystem>().particleCount > 0) soCoSuong++;
                    if (trv != null) soCoVet++;
                    if (f.GetComponentInChildren<Light>() != null) soCoDen++;
                }
                // Do dai that cua vet moi qua (lay lon nhat trong doi qua)
                foreach (var f in roi)
                {
                    var trv = f.GetComponentInChildren<TrailRenderer>();
                    if (trv == null || trv.positionCount < 2) continue;
                    float dai = 0f;
                    for (int i = 1; i < trv.positionCount; i++) dai += Vector3.Distance(trv.GetPosition(i), trv.GetPosition(i - 1));
                    // doan tu diem vet gan nhat toi qua (khong gia dinh thu tu diem cua TrailRenderer)
                    dai += Mathf.Min(Vector3.Distance(trv.GetPosition(0), f.transform.position),
                                     Vector3.Distance(trv.GetPosition(trv.positionCount - 1), f.transform.position));
                    float cu; vetDaiNhat.TryGetValue(f, out cu);
                    vetDaiNhat[f] = Mathf.Max(cu, dai);
                }
                int hat = 0;
                foreach (var p in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude)) hat += p.particleCount;
                hatMax = Mathf.Max(hatMax, hat);
                if (biaMua.GetComponent<FrozenEffect>() != null) biCham = true;
                // Chup khi co qua da xuong THAP: o do cao 20 m qua cau nam ngoai khung hinh
                FallingShard thap = null;
                foreach (var f in roi)
                {
                    float c = Vector3.Distance(f.transform.position, f.target);
                    // o tam camera 2.5D qua cau chi lot vao khung hinh o 3-4 m cuoi truoc khi cham dat
                    if (c > 1.2f && c < 4f) { thap = f; break; }
                }
                if (!daChup && thap != null && Time.time > hanK - 3.5f)
                {
                    daChup = true;
                    Vector3 wThap = thap.transform.position;
                    yield return ChupCanDiem(wThap, "quacaubang_5b_muabang_roi_can");
                    yield return Chup("quacaubang_5_muabang_roi");
                }
                yield return null;
            }
            float matMua = mauMua0 - biaMua.health;
            float vetMin = float.MaxValue, vetMax = 0f, vetTong = 0f;
            foreach (var kv in vetDaiNhat) { vetMin = Mathf.Min(vetMin, kv.Value); vetMax = Mathf.Max(vetMax, kv.Value); vetTong += kv.Value; }
            if (vetDaiNhat.Count == 0) vetMin = 0f;
            float vetTB = vetDaiNhat.Count > 0 ? vetTong / vetDaiNhat.Count : 0f;
            Ghi(string.Format("K. Mua bang that: vat dang roi cung luc toi da {0}; da xet {1} qua: KHONG co luoi (khoi cau) {2} (doi chung qua cua ky nang Qua cau bang: {3} luoi); vet sao bang (anh VetSaoBang, keo gian) {4}; hao quang dang phat {5}, manh bang dang phat {6}, luong khi lanh dang phat {7}, co vet {8}, co den rieng {9}; tang bang cu (con \"Tang\") {10}",
                soRoiMax, soXet, soKhongLuoi, luoiDoiChung, soVetSaoBang, soHaoQuang, soManh, soCoSuong, soCoVet, soCoDen, soTangCu));
            Ghi(string.Format("    do dai vet lon nhat moi qua ({0} qua): ngan nhat {1:F2} m, trung binh {2:F2} m, dai nhat {3:F2} m; rong dau vet trung binh ~{4:F2} m -> dai / rong x{5:F1}",
                vetDaiNhat.Count, vetMin, vetTB, vetMax, (rongMin + rongMax) * 0.5f, vetTB / Mathf.Max(0.01f, (rongMin + rongMax) * 0.5f)));
            Ghi(string.Format("    be rong dau vet that: nho nhat {0:F3} m, lon nhat {1:F3} m (cho phep {2:F3}..{3:F3}; ban truoc 0,55 -> x1,2 = 0,66); dom sang tron o dau {4}/{5}, dung co {6}",
                rongMin, rongMax, 0.66f * 0.7f, 0.66f, soDauTron, soXet, soDauDungCo));
            // Doc thang PNG tren dia - doc lap voi cach Unity nhap anh
            System.Func<string, Texture2D> docPng = duong =>
            {
                if (!System.IO.File.Exists(duong)) return null;
                var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                t.LoadImage(System.IO.File.ReadAllBytes(duong));
                return t;
            };
            var anhVet = docPng("Assets/Resources/KyNang/QuaCauBang/VetSaoBang.png");
            var anhDau = docPng("Assets/Resources/KyNang/QuaCauBang/DauSaoBang.png");
            float cotDau = 0f, cotSau = 0f, dauTam = 0f, dauMep = 0f;
            if (anhVet != null)
                for (int y = 0; y < anhVet.height; y++)
                {
                    cotDau = Mathf.Max(cotDau, anhVet.GetPixel(0, y).a);
                    cotSau = Mathf.Max(cotSau, anhVet.GetPixel(anhVet.width / 10, y).a);
                }
            if (anhDau != null)
            {
                dauTam = anhDau.GetPixel(anhDau.width / 2, anhDau.height / 2).a;
                for (int i = 0; i < anhDau.width; i++)
                    dauMep = Mathf.Max(dauMep, Mathf.Max(anhDau.GetPixel(i, 0).a, anhDau.GetPixel(0, i).a));
            }
            // DOI CHUNG: anh vet ban truoc (git HEAD~, co canh cat) - tim bang file tam do phep thu ghi ra neu co
            float cotDauCu = -1f;
            var anhCu = docPng("Temp/VetSaoBang_cu.png");
            if (anhCu != null) for (int y = 0; y < anhCu.height; y++) cotDauCu = Mathf.Max(cotDauCu, anhCu.GetPixel(0, y).a);
            Ghi(string.Format("    anh PNG tren dia: vet - alpha lon nhat cot dau (u=0) {0:F3}, cot u=0,1 {1:F3} (DOI CHUNG anh cu cot dau {2:F3}); dau tron - tam {3:F3}, mep {4:F3}",
                cotDau, cotSau, cotDauCu, dauTam, dauMep));
            if (anhVet != null) Object.DestroyImmediate(anhVet);
            if (anhDau != null) Object.DestroyImmediate(anhDau);
            if (anhCu != null) Object.DestroyImmediate(anhCu);
            Ghi(string.Format("    bia giua vung mat {0:F0} mau, bi cham/dong cung {1}; tong so hat cung luc toi da {2}", matMua, biCham, hatMax));
            Ghi(string.Format("    roi THANG DUNG: {0}/{1} qua (lech ngang lon nhat so voi diem roi {2:F3} m, truc bay chi thang xuong)", soThangDung, soXet, lechNgangMax));
            Kiem(soThangDung == soXet, "qua cau bang cua Mua bang khong roi thang dung");
            Kiem(soXet >= 10, "Mua bang khong roi du qua de xet");
            Kiem(luoiDoiChung > 0, "doi chung: qua cua ky nang Qua cau bang khong co luoi - phep dem luoi vo nghia");
            Kiem(soKhongLuoi == soXet, "qua roi cua Mua bang van con khoi cau");
            Kiem(soVetSaoBang == soXet && soCoVet == soXet, "qua roi cua Mua bang khong co vet sao bang");
            Kiem(vetDaiNhat.Count >= 10 && vetTB > 2.5f && vetTB / 0.66f > 5f, "vet sang bang khong dai manh");
            Kiem(rongMin >= 0.66f * 0.7f - 0.001f && rongMax <= 0.661f && rongMax - rongMin > 0.08f, "be rong vet khong nam trong 70%..100% cua 0,66 m / khong ngau nhien");
            Kiem(soDauTron == soXet && soDauDungCo == soXet, "dau vet thieu dom sang tron / sai co");
            Kiem(cotDau < 0.02f && cotSau > 0.5f && dauTam > 0.9f && dauMep < 0.02f, "anh vet con canh cat o dau / anh dau tron hong");
            Kiem(cotDauCu < 0f || cotDauCu > 0.5f, "doi chung: anh cu khong co canh cat - phep do cot dau vo nghia");
            Kiem(soHaoQuang == soXet && soManh == soXet && soCoSuong == soXet, "mat hao quang / manh bang / luong khi lanh (chi duoc bo khoi cau)");
            Kiem(soCoDen == 0, "qua cau roi van gan den rieng (nang cho dien thoai)");
            Kiem(soTangCu == 0, "van con tang bang cu roi xuong");
            Kiem(matMua > 1f && biCham, "Mua bang doi hinh xong khong con gay sat thuong / lam cham");
            Object.Destroy(biaMua.gameObject);
            if (bao != null) Object.Destroy(bao.gameObject);
        }

        // ================= L. CUM GAI CHI KHI TRUNG DO VAT / KE DICH =================
        Ghi("");
        {
            yield return new WaitForSeconds(2.5f);
            int lopVat = LayerMask.GetMask("Default");
            // Tim mot bia mo (BoxCollider lop Default ten TS_) co mot huong trong: diem 1,0 m ngoai mat bia chi
            // cham DUNG bia nay, diem 2,6 m ngoai mat bia khong cham vat nao trong 1,7 m.
            Vector3 gan = Vector3.zero, xa = Vector3.zero, trong = Vector3.zero; string tenBia = "";
            bool timDuoc = false;
            foreach (var c in Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude))
            {
                if (c.gameObject.layer != 0 || !c.name.StartsWith("TS_")) continue;
                for (int h = 0; h < 8 && !timDuoc; h++)
                {
                    Vector3 huongBia = Quaternion.AngleAxis(h * 45f, Vector3.up) * Vector3.forward;
                    Vector3 mat = c.ClosestPoint(c.bounds.center + huongBia * 10f);
                    Vector3 g1 = mat + huongBia * 1.0f; g1.y = VfxFactory.GroundY(g1);
                    Vector3 g2 = mat + huongBia * 2.6f; g2.y = VfxFactory.GroundY(g2);
                    Vector3 g3 = mat + huongBia * 7.0f; g3.y = VfxFactory.GroundY(g3);
                    var o1 = Physics.OverlapSphere(g1, 1.7f, lopVat, QueryTriggerInteraction.Ignore);
                    bool chiBiaNay = o1.Length == 1 && o1[0] == c;
                    bool xaTrong = !Physics.CheckSphere(g2, 1.7f, lopVat, QueryTriggerInteraction.Ignore);
                    bool datTrong = !Physics.CheckSphere(g3, 3.5f, lopVat, QueryTriggerInteraction.Ignore);
                    if (chiBiaNay && xaTrong && datTrong) { gan = g1; xa = g2; trong = g3; tenBia = c.name; timDuoc = true; }
                }
                if (timDuoc) break;
            }
            if (!timDuoc) { Ghi("[LOI] khong tim duoc bia mo co cho trong de thu"); loi++; }
            else
            {
                string[] tenCa = { "dat trong (khong vat, khong ke dich)", "bia mo " + tenBia + " cach 1,0 m", "ke dich (bia do don) cach 1,0 m", "DOI CHUNG bia mo cach 2,6 m (ngoai 1,7 m)" };
                Vector3[] choCa = { trong, gan, trong, xa };
                var gaiBat = new int[4]; var phanKhac = new int[4]; var coGaiDem = new int[4];
                for (int ca = 0; ca < 4; ca++)
                {
                    Damageable keDich = null;
                    if (ca == 2) { keDich = TaoBia("TAM_KeDichL", choCa[ca] + Vector3.right * 1.0f); yield return new WaitForFixedUpdate(); }
                    var truoc = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
                    int coGai0 = FallingShard.SoLanCoGai;
                    var qua = VfxFactory.QuaCauBangRoi(choCa[ca], 3f, 0.12f);
                    var fs = qua.GetComponent<FallingShard>();
                    fs.damage = 1f; fs.impactRadius = 1.7f; fs.damageMask = maskEnemy;
                    GameObject no = null;
                    float hanL = Time.time + 1.5f;
                    while (no == null && Time.time < hanL)
                    {
                        foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                            if (!truoc.Contains(g) && (g.name.Contains("NoBang") || g.name.Contains("IceImpact"))) { no = g; break; }
                        yield return null;
                    }
                    if (no != null)
                        foreach (Transform con in no.transform)
                        {
                            if (con.name.StartsWith("CumGai")) { if (con.gameObject.activeSelf) gaiBat[ca]++; }
                            else if (!con.name.StartsWith("HaoQuang") && con.gameObject.activeSelf) phanKhac[ca]++;
                        }
                    coGaiDem[ca] = FallingShard.SoLanCoGai - coGai0;
                    if (ca == 0) { yield return new WaitForSeconds(0.35f); yield return Chup("quacaubang_6_trung_dat_khong_gai"); }
                    if (ca == 1) { yield return new WaitForSeconds(0.35f); yield return Chup("quacaubang_7_trung_bia_co_gai"); }
                    if (keDich != null) Object.Destroy(keDich.gameObject);
                    yield return new WaitForSeconds(1.2f);
                }
                for (int ca = 0; ca < 4; ca++)
                    Ghi(string.Format("L. {0}: cum gai dang bat {1}, cac phan no khac dang bat {2}, bo dem co gai +{3}", tenCa[ca], gaiBat[ca], phanKhac[ca], coGaiDem[ca]));
                Kiem(gaiBat[0] == 0 && gaiBat[3] == 0, "chi trung mat dat ma van moc cum gai bang");
                Kiem(gaiBat[1] > 0 && gaiBat[2] > 0, "trung do vat / ke dich ma khong co cum gai bang");
                Kiem(phanKhac[0] > 0 && phanKhac[0] == phanKhac[1], "no khong co gai ma mat luon cac phan no khac");
            }

            // Thong ke mot con Mua bang that o cho trong, co bia do don giua vung
            if (timDuoc)
            {
                var biaL = TaoBia("TAM_BiaL", trong);
                yield return new WaitForFixedUpdate();
                int c0 = FallingShard.SoLanCham, g0 = FallingShard.SoLanCoGai;
                var baoL = IceStorm.Spawn(trong, maskEnemy);
                yield return new WaitForSeconds(6.8f);
                Ghi(string.Format("L. Mua bang that o cho trong (bia do don giua vung): {0} qua cham dat, {1} qua co cum gai (trung bia do don), {2} qua khong gai",
                    FallingShard.SoLanCham - c0, FallingShard.SoLanCoGai - g0, (FallingShard.SoLanCham - c0) - (FallingShard.SoLanCoGai - g0)));
                Kiem(FallingShard.SoLanCham - c0 > 20, "con Mua bang that roi qua it qua");
                Object.Destroy(biaL.gameObject);
                if (baoL != null) Object.Destroy(baoL.gameObject);
                yield return new WaitForSeconds(2f);

                // Con thu hai KHONG co ke dich: aimAtEnemyChance = 1 nen con dau qua nao cung nham vao bia (35/35 co gai).
                // Khong co ai thi qua roi NGAU NHIEN trong ban kinh - chi qua roi gan do vat moi co gai. Dem doc lap:
                // qua nao co vat lop Default trong 1,7 m quanh diem roi.
                int c1 = FallingShard.SoLanCham, g1b = FallingShard.SoLanCoGai;
                int ganVatDocLap = 0;
                var tenKeDichGan = new HashSet<string>();
                var daDem = new HashSet<FallingShard>();
                var baoL2 = IceStorm.Spawn(trong, maskEnemy);
                float hanL2 = Time.time + 6.8f;
                while (Time.time < hanL2)
                {
                    foreach (var f in Object.FindObjectsByType<FallingShard>(FindObjectsInactive.Exclude))
                        if (daDem.Add(f))
                        {
                            if (Physics.CheckSphere(f.target, 1.7f, lopVat, QueryTriggerInteraction.Ignore)) ganVatDocLap++;
                            // Chan doan: ai tren damageMask nam trong 1,7 m quanh diem roi
                            foreach (var col in Physics.OverlapSphere(f.target, 1.7f, maskEnemy, QueryTriggerInteraction.Collide))
                            {
                                var dd = col.GetComponentInParent<Damageable>();
                                if (dd != null && !dd.IsDead) tenKeDichGan.Add(dd.name + "@" + LayerMask.LayerToName(col.gameObject.layer));
                            }
                        }
                    yield return null;
                }
                int cham2 = FallingShard.SoLanCham - c1, gai2 = FallingShard.SoLanCoGai - g1b;
                Ghi(string.Format("L. Mua bang that KHONG co ke dich: {0} qua cham dat, {1} qua co cum gai, {2} qua khong gai; dem doc lap theo diem roi: {3}/{4} qua co do vat trong 1,7 m",
                    cham2, gai2, cham2 - gai2, ganVatDocLap, daDem.Count));
                Ghi("    (chan doan) Damageable song tren damageMask trong 1,7 m quanh cac diem roi: " + (tenKeDichGan.Count == 0 ? "khong co" : string.Join(", ", tenKeDichGan)));
                Kiem(gai2 == ganVatDocLap, "so qua co gai khong khop so qua roi gan do vat");
                Kiem(cham2 - gai2 > 0, "khong co ke dich ma qua nao cung co gai");
                if (baoL2 != null) Object.Destroy(baoL2.gameObject);
            }
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
