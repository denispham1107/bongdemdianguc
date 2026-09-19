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
///   F (sua 17/09/2026, nguoi dung: "moi qua cau bang co 40% kha nang Dong bang doi thu... khong the di chuyen va su dung skill";
///      chon: TRUNG LA CHAM nhu cu + THEM 40% dong bang 1,5 giay): 1000 lan no - ti le cham 100%, ti le dong cung ~40%,
///      dong cung 1,5 giay, cap 3 keo dai ca hai (+0,30).
///   N. (19/09/2026, nguoi dung: "khi skill dat cap 5, tang bang den thoi gian bien mat thay vi bien mat nhu
///      binh thuong, thi se no tung gay them 100 sat thuong cho doi thu va cac doi thu o gan do" - cho CA Mua bang
///      lan Qua cau bang; hoi lai: ban kinh 3,4 m, dung 100 co dinh): tha tang bang THAT roi cho no het gio -
///      bia dung tai cho mat dung 100, bia cach 3,0 m mat it hon (giam dan), bia cach 4,0 m KHONG mat gi;
///      DOI CHUNG cap 4 -> tang bang tan im lang, khong ai mat mau. Lam ca voi Mua bang va Qua cau bang.
///   M. (cung ngay) CAP 5 ra 5 qua: tung that o cap 4 (DOI CHUNG) -> 3 qua, cap 5 -> 5 qua; goi ky nang cap 5 tu nguoi kia ->
///      may minh cung ra 5 qua. NGUOI CHOI bi dong bang: khong di duoc (HeSoTocBang = 0) va CastAt bi tu choi.
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
///      Sua lan 3 cung ngay (nguoi dung: dau "hoi nhon nhon va co rang cua mot chut"; chon mui chi xuong dat): dau la TAM TU GIAC
///      DauSaoBang - do truc len cua tam trung huong roi, mat tam quay ve camera, dung co; anh PNG: be ngang (alpha > 0,5) tai
///      mui hep hon o dom (nhon), dem so "rang" = so lan be ngang NO RA khi di tu dom ve mui (DOI CHUNG anh dom tron cu: 0 rang).
///   L (17/09/2026, nguoi dung: "chi tao bang tren mat dat khi danh trung nguoi choi khac hoac cac quai vat"): bia mo trong 1,7 m
///      KHONG con moc gai (doi chung doc lap: co va cham lop Default trong 1,7 m); quai (lop Enemy) -> co; NGUOI CHOI KHAC (lop
///      Player, mask co Player) -> co; chinh NGUOI TUNG dung trong vung (boQua) -> khong; Mua bang that khong co ai -> 0 gai du
///      co qua roi gan do vat.
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

        // ⚠️ TAT GameDirector SUOT PHEP THU. Phep thu nay rat dai va tu dung bia rieng; de
        // GameDirector chay thi giua chung no tha them dot quai 24 con vao dung luc dang ve hang
        // tram tang bang - 19/09/2026 menu 74 dinh dung canh nay va Unity TAT HAN vi GPU timeout
        // ("Failed to present D3D11 swapchain due to device reset/removed").
        bool dirBatCu = false;
        if (dir != null) { dirBatCu = dir.enabled; dir.enabled = false; }

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
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; kyVuaTung = s; };
        toi.DaTungPhep += dem;

        CapDo.BatDauTranMoi();
        int truocKhoa = soPhep;
        toi.CastAt(K, goc + huong * 8f);
        bool tuChoiKhoa = soPhep == truocKhoa;
        string nhacKhoa = toi.LastMessage;
        CapDo.MoCaDuongChoPhepThu(K);

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
        int soTrung = QuaCauBang.NoBang(tam, QuaCauBang.BanKinhNo, QuaCauBang.SatThuongGoc, maskEnemy, QuaCauBang.GiayCham, QuaCauBang.GiayDongBang, null);
        Ghi(string.Format("E. no tai tam bia: trung {0} muc tieu; mat mau: tam {1:F2} (mong 65), 1 m {2:F2}, 2,5 m {3:F2}, 5 m {4:F2} (ngoai ban kinh 3,4)",
            soTrung, m0 - b0.health, m1 - b1.health, m25 - b25.health, m5 - b5.health));
        Kiem(Mathf.Abs((m0 - b0.health) - 65f) < 0.01f, "no dung tam khong mat dung 65");
        Kiem(m1 - b1.health > 1f && m25 - b25.health > 1f, "ke dich trong vung no khong mat mau");
        Kiem(m5 - b5.health < 0.01f, "ke dich ngoai vung no van mat mau");
        Object.Destroy(b1.gameObject); Object.Destroy(b25.gameObject); Object.Destroy(b5.gameObject);

        // ================= F. LAM CHAM =================
        yield return new WaitForFixedUpdate();
        int soCham = 0, soDongCung = 0; float slowMin = 1f, slowMax = 0f, conMin = 99f, conMax = 0f, dongMin = 99f, dongMax = 0f;
        const int N = 1000;
        for (int i = 0; i < N; i++)
        {
            XoaHieuUng(b0); b0.health = b0.maxHealth;
            QuaCauBang.NoBang(tam, QuaCauBang.BanKinhNo, QuaCauBang.SatThuongGoc, maskEnemy, QuaCauBang.GiayCham, QuaCauBang.GiayDongBang, null);
            var f = b0.GetComponent<FrozenEffect>();
            if (f == null) continue;
            soCham++;
            if (f.IsFullyFrozen) { soDongCung++; dongMin = Mathf.Min(dongMin, f.dongCungConLai); dongMax = Mathf.Max(dongMax, f.dongCungConLai); }
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
        float dongCap3 = quaCap3 != null ? quaCap3.giayDongBang : -1f;
        if (quaCap3 != null) Object.Destroy(quaCap3.gameObject);
        float tlDong = 100f * soDongCung / N;
        Ghi(string.Format("F. {0} lan no: bi cham {1:F1}% (mong 100%), DONG BANG {2:F1}% (mong 40%); slow {3}..{4} (mong 0,5), thoi gian cham {5:F2}..{6:F2} giay (mong 2); dong cung {7:F2}..{8:F2} giay (mong 1,50); qua cau cap 3: cham {9:F2} giay (mong 2,30), dong bang {10:F2} giay (mong 1,80)",
            N, tl, tlDong, slowMin, slowMax, conMin, conMax, dongMin, dongMax, giayCap3, dongCap3));
        Kiem(Mathf.Abs(tl - 100f) < 0.01f, "trung ma khong chac chan bi lam cham");
        Kiem(Mathf.Abs(tlDong - 40f) <= 5f, "ti le dong bang lech qua xa 40%");
        Kiem(soDongCung > 0 && Mathf.Abs(dongMin - 1.5f) < 0.01f && Mathf.Abs(dongMax - 1.5f) < 0.01f, "dong bang khong phai 1,5 giay");
        Kiem(Mathf.Abs(dongCap3 - 1.80f) < 0.01f, "cap ky nang khong keo dai dong bang");
        Kiem(Mathf.Approximately(slowMin, 0.5f) && Mathf.Approximately(slowMax, 0.5f), "muc cham khong phai 50%");
        Kiem(Mathf.Abs(conMin - 2f) < 0.01f && Mathf.Abs(conMax - 2f) < 0.01f, "lam cham khong keo dai 2 giay");
        Kiem(SachPhep.MoTa(K).Contains("ĐÓNG BĂNG") && SachPhep.MoTa(K).Contains("NĂM quả"), "mo ta Sach phep chua noi dong bang / cap 5 nam qua");
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
            float rongMin = float.MaxValue, rongMax = 0f, dotMuiMin = 1f, dotCamMin = 1f;
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
                    int luoiKhac = 0;
                    foreach (var mrr in f.GetComponentsInChildren<MeshRenderer>()) if (mrr.name != "DauSaoBang") luoiKhac++;
                    if (luoiKhac == 0) soKhongLuoi++;
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
                        var dmr = dau != null ? dau.GetComponent<MeshRenderer>() : null;
                        if (dmr != null && dmr.enabled && dmr.sharedMaterial != null && dmr.sharedMaterial.mainTexture != null
                            && dmr.sharedMaterial.mainTexture.name == "DauSaoBang")
                        {
                            soDauTron++;
                            Vector3 huongRoiDau = (f.target - f.transform.position).normalized;
                            float dotMui = Vector3.Dot(dau.up, huongRoiDau);
                            var camC = Camera.main;
                            Vector3 toiCam = camC != null ? camC.transform.position - dau.position : Vector3.back;
                            toiCam -= huongRoiDau * Vector3.Dot(toiCam, huongRoiDau);
                            float dotCam = Mathf.Abs(Vector3.Dot(dau.forward, toiCam.normalized));
                            dotMuiMin = Mathf.Min(dotMuiMin, dotMui); dotCamMin = Mathf.Min(dotCamMin, dotCam);
                            if (Mathf.Abs(dau.lossyScale.x - rong * VfxFactory.HeSoDauSaoBang) < 0.02f && dotMui > 0.99f && dotCam > 0.99f) soDauDungCo++;
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
            Ghi(string.Format("    be rong dau vet that: nho nhat {0:F3} m, lon nhat {1:F3} m (cho phep {2:F3}..{3:F3}; ban truoc 0,55 -> x1,2 = 0,66); tam dau vet (giot mui nhon) {4}/{5}, dung co + mui trung huong roi + quay ve camera {6} (cos mui-huong roi nho nhat {7:F4}, cos mat-camera nho nhat {8:F4})",
                rongMin, rongMax, 0.66f * 0.7f, 0.66f, soDauTron, soXet, soDauDungCo, dotMuiMin, dotCamMin));
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
            // Hinh dau: be ngang (so diem anh alpha > 0,5) theo tung hang; dom o y = DomSangY, mui ve +y (hang tang)
            System.Func<Texture2D, int[]> beNgang = a =>
            {
                var bn = new int[a.height];
                for (int y = 0; y < a.height; y++) for (int x = 0; x < a.width; x++) if (a.GetPixel(x, y).a > 0.5f) bn[y]++;
                return bn;
            };
            System.Func<Texture2D, float, int> hangCua = (a, yy) => Mathf.Clamp(Mathf.RoundToInt((yy + 1f) * 0.5f * a.height), 0, a.height - 1);
            // so rang = so lan be ngang no ra >= 2 diem anh (sau khi da thu hep) khi di tu hang rong nhat ve mui
            System.Func<Texture2D, int> demRang = a =>
            {
                // bat dau tu hang RONG NHAT (tu dom tro len) - phan phinh cua dom tron khong bi dem la rang
                var bn = beNgang(a); int y0 = hangCua(a, DauSaoBangHuong.DomSangY);
                for (int y = y0; y < a.height; y++) if (bn[y] >= bn[y0]) y0 = y;
                int rang = 0, nhoNhat = bn[y0];
                for (int y = y0 + 1; y < a.height; y++)
                {
                    if (bn[y] >= nhoNhat + 2) { rang++; nhoNhat = bn[y]; }
                    nhoNhat = Mathf.Min(nhoNhat, bn[y]);
                }
                return rang;
            };
            int soRang = -1, rangCu = -1, rongDom = 0, rongGanMui = 0;
            if (anhDau != null)
            {
                dauTam = anhDau.GetPixel(anhDau.width / 2, hangCua(anhDau, DauSaoBangHuong.DomSangY)).a;
                for (int i = 0; i < anhDau.width; i++) dauMep = Mathf.Max(dauMep, Mathf.Max(anhDau.GetPixel(i, 0).a, anhDau.GetPixel(i, anhDau.height - 1).a));
                for (int i = 0; i < anhDau.height; i++) dauMep = Mathf.Max(dauMep, Mathf.Max(anhDau.GetPixel(0, i).a, anhDau.GetPixel(anhDau.width - 1, i).a));
                var bn = beNgang(anhDau);
                rongDom = bn[hangCua(anhDau, DauSaoBangHuong.DomSangY)];
                rongGanMui = bn[hangCua(anhDau, 0.45f)];
                soRang = demRang(anhDau);
            }
            // ⚠️ Anh DOI CHUNG de o PlayTestShots/doichung/ chu KHONG phai Temp/: Unity xoa sach Temp moi
            // lan khoi dong lai, nen sau lan Unity sap 19/09/2026 phep thu doc ra -1 va bao loi GIA
            // ("anh dom tron cu cung dem ra rang"). Hai file trich tu git: DauSaoBang cua ead6af9,
            // VetSaoBang cua f2e70d9 - tuc ban NGAY TRUOC lan sua tuong ung.
            var anhDauCu = docPng("PlayTestShots/doichung/DauSaoBang_cu.png");
            if (anhDauCu != null) { rangCu = demRang(anhDauCu); Object.DestroyImmediate(anhDauCu); }
            // DOI CHUNG: anh vet ban truoc (git HEAD~, co canh cat) - tim bang file tam do phep thu ghi ra neu co
            float cotDauCu = -1f;
            var anhCu = docPng("PlayTestShots/doichung/VetSaoBang_cu.png");
            if (anhCu != null) for (int y = 0; y < anhCu.height; y++) cotDauCu = Mathf.Max(cotDauCu, anhCu.GetPixel(0, y).a);
            Ghi(string.Format("    anh PNG tren dia: vet - alpha lon nhat cot dau (u=0) {0:F3}, cot u=0,1 {1:F3} (DOI CHUNG anh cu cot dau {2:F3}); dau - dom {3:F3}, mep {4:F3}, be ngang o dom {5} diem anh / gan mui (y 0,45) {6} diem anh, so rang cua {7} (DOI CHUNG anh dom tron cu: {8} rang)",
                cotDau, cotSau, cotDauCu, dauTam, dauMep, rongDom, rongGanMui, soRang, rangCu));
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
            Kiem(soDauTron == soXet && soDauDungCo == soXet, "dau vet thieu tam giot sang / sai co / mui khong chi huong roi / khong quay ve camera");
            Kiem(rongGanMui > 0 && rongGanMui * 2 < rongDom, "dau vet khong nhon ve mui");
            Kiem(soRang >= 3 && soRang <= 12, "dau vet khong co rang cua (hoac rang cua qua nhieu)");
            Kiem(rangCu == 0, "doi chung: anh dom tron cu cung dem ra rang - phep dem rang vo nghia");
            Kiem(cotDau < 0.02f && cotSau > 0.5f && dauTam > 0.9f && dauMep < 0.02f, "anh vet con canh cat o dau / anh dau tron hong");
            Kiem(cotDauCu < 0f || cotDauCu > 0.5f, "doi chung: anh cu khong co canh cat - phep do cot dau vo nghia");
            Kiem(soHaoQuang == soXet && soManh == soXet && soCoSuong == soXet, "mat hao quang / manh bang / luong khi lanh (chi duoc bo khoi cau)");
            Kiem(soCoDen == 0, "qua cau roi van gan den rieng (nang cho dien thoai)");
            Kiem(soTangCu == 0, "van con tang bang cu roi xuong");
            Kiem(matMua > 1f && biCham, "Mua bang doi hinh xong khong con gay sat thuong / lam cham");
            Object.Destroy(biaMua.gameObject);
            if (bao != null) Object.Destroy(bao.gameObject);
        }

        // ================= L. TANG BANG CHI MOC KHI DONG BANG DUOC (luat moi 19/09/2026) =================
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
                // QUAI THAT lang thang toi gan cho thu thi trong 1,7 m co Damageable con song -> cum gai moc len va
                // muc nay bao loi "dat trong ma van co gai". Lan chay 18/09/2026 dinh dung cai nay (ca 1 va ca 3 deu
                // ra 8 gai), chay lai thi het - dau hieu cua phep thu chap chon. Don sach quai quanh cho thu truoc da.
                int quaiDon = 0;
                foreach (var qL in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude))
                {
                    if (qL == null) continue;
                    float dL = Mathf.Min(Vector3.Distance(qL.transform.position, gan),
                               Mathf.Min(Vector3.Distance(qL.transform.position, xa), Vector3.Distance(qL.transform.position, trong)));
                    if (dL < 30f) { Object.Destroy(qL.gameObject); quaiDon++; }
                }
                yield return new WaitForSeconds(0.3f);
                Ghi("L. don " + quaiDon + " quai that quanh cho thu (quai song trong 1,7 m se lam moc cum gai)");

                // ⚠️ LUAT MOI (nguoi dung 19/09/2026): tang bang chi moc khi DONG BANG DUOC doi thu.
                // Nen moi ca deu dat freezeChance ro rang: 1 = chac chan dong bang, 0 = chac chan khong.
                // Ca 6 la ca QUAN TRONG NHAT cua luat moi: trung quai that nhung KHONG dong bang -> KHONG tang bang
                // (ban cu thi van moc, vi luat cu chi hoi "trong 1,7 m co ke dich nao khong").
                string[] tenCa = { "dat trong (khong vat, khong ke dich)", "bia mo " + tenBia + " cach 1,0 m (co va cham lop Default trong 1,7 m: " + (Physics.CheckSphere(gan, 1.7f, lopVat, QueryTriggerInteraction.Ignore) ? "co" : "KHONG") + ")",
                                   "quai vat (bia do don lop Enemy) cach 1,0 m, DONG BANG chac chan",
                                   "bia mo cach 2,6 m (ngoai 1,7 m)",
                                   "NGUOI CHOI KHAC (lop Player, mask Enemy+Player) cach 1,0 m, DONG BANG chac chan",
                                   "chinh NGUOI TUNG (lop Player, boQua) cach 1,0 m, mask Enemy+Player",
                                   "quai vat cach 1,0 m nhung KHONG dong bang duoc (xac suat 0)" };
                Vector3[] choCa = { trong, gan, trong, xa, trong, trong, trong };
                float[] xsBang = { 1f, 1f, 1f, 1f, 1f, 1f, 0f };
                int soCa = tenCa.Length;
                var gaiBat = new int[soCa]; var phanKhac = new int[soCa]; var coGaiDem = new int[soCa];
                int maskCaNguoi = maskEnemy | LayerMask.GetMask("Player");
                for (int ca = 0; ca < soCa; ca++)
                {
                    Damageable keDich = null;
                    if (ca == 2 || ca == 4 || ca == 5 || ca == 6)
                    {
                        keDich = TaoBia("TAM_KeDichL" + ca, choCa[ca] + Vector3.right * 1.0f);
                        if (ca == 4 || ca == 5) keDich.gameObject.layer = LayerMask.NameToLayer("Player");
                        yield return new WaitForFixedUpdate();
                    }
                    var truoc = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
                    int coGai0 = FallingShard.SoLanCoGai;
                    var qua = VfxFactory.QuaCauBangRoi(choCa[ca], 3f, 0.12f);
                    var fs = qua.GetComponent<FallingShard>();
                    fs.damage = 1f; fs.impactRadius = 1.7f;
                    fs.damageMask = (ca == 4 || ca == 5) ? maskCaNguoi : maskEnemy;
                    fs.freezeChance = xsBang[ca];
                    if (ca == 5) fs.boQua = keDich;
                    // Mot cu roi nay co the sinh NHIEU vat: mot tang bang duoi chan moi ke bi dong bang,
                    // cong mot vu no "khong gai" o dung cho roi. Gom HET roi cong don.
                    var dsNo = new List<GameObject>();
                    float hanL = Time.time + 1.5f;
                    while (dsNo.Count == 0 && Time.time < hanL)
                    {
                        foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                            if (!truoc.Contains(g) && (g.name.Contains("NoBang") || g.name.Contains("IceImpact"))) dsNo.Add(g);
                        yield return null;
                    }
                    foreach (var no in dsNo)
                        foreach (Transform con in no.transform)
                        {
                            if (con.name.StartsWith("CumGai")) { if (con.gameObject.activeSelf) gaiBat[ca]++; }
                            else if (!con.name.StartsWith("HaoQuang") && con.gameObject.activeSelf) phanKhac[ca]++;
                        }
                    coGaiDem[ca] = FallingShard.SoLanCoGai - coGai0;
                    if (ca == 0) { yield return new WaitForSeconds(0.35f); yield return Chup("quacaubang_6_trung_dat_khong_gai"); }
                    if (ca == 1) { yield return new WaitForSeconds(0.35f); yield return Chup("quacaubang_7_trung_bia_khong_gai"); }
                    if (keDich != null) Object.Destroy(keDich.gameObject);
                    yield return new WaitForSeconds(1.2f);
                }
                for (int ca = 0; ca < soCa; ca++)
                    Ghi(string.Format("L. {0}: cum gai dang bat {1}, cac phan no khac dang bat {2}, bo dem co gai +{3}", tenCa[ca], gaiBat[ca], phanKhac[ca], coGaiDem[ca]));
                Kiem(gaiBat[0] == 0 && gaiBat[3] == 0, "chi trung mat dat ma van moc tang bang");
                Kiem(gaiBat[1] == 0, "trung do vat (bia mo) ma van moc tang bang");
                Kiem(gaiBat[2] > 0 && gaiBat[4] > 0, "dong bang duoc quai / nguoi choi khac ma khong moc tang bang");
                Kiem(gaiBat[5] == 0, "chinh nguoi tung dung trong vung cung lam moc tang bang");
                Kiem(gaiBat[6] == 0 && coGaiDem[6] == 0, "trung ke dich ma KHONG dong bang duoc thi khong duoc moc tang bang (luat moi 19/09/2026)");
                Kiem(coGaiDem[2] == 1 && coGaiDem[4] == 1, "moi ke bi dong bang phai moc DUNG MOT tang bang");
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
                // Don moi bia do don TAM_ con song (TAM_BiaDuong cua muc C con dung gan day): Mua bang nham ke dich 100% nen chi
                // mot con sot lai la ca con Mua bang do vao no (lan chay 17/09/2026: 37/37 qua co gai, chan doan ra TAM_BiaDuong).
                foreach (var dd in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude))
                    if (dd.name.StartsWith("TAM_")) Object.Destroy(dd.gameObject);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
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
                Kiem(gai2 == 0, "Mua bang khong co ke dich ma van co qua moc gai (do vat khong duoc moc gai nua)");
                Kiem(ganVatDocLap > 0, "doi chung: khong qua nao roi gan do vat - chua thu duoc truong hop do vat");
                if (baoL2 != null) Object.Destroy(baoL2.gameObject);
            }
        }

        // ================= M. CAP 5 RA 5 QUA + NGUOI CHOI BI DONG BANG =================
        Ghi("");
        {
            foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude)) if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
            foreach (var q0 in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude)) Object.Destroy(q0.gameObject);
            yield return new WaitForSeconds(0.6f);
            Vector3 hM = HuongTrong(toi);
            toi.transform.rotation = Quaternion.LookRotation(hM);
            // nang ky nang len cap 4 (doi chung) roi 5
            int vong = 0;
            while (CapDo.CapCuaKyNang(K) < 4 && vong++ < 20)
            {
                if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
                CapDo.NangCap(K);
            }
            toi.mana = toi.maxMana;
            int truoc4 = DemQua();
            toi.CastAt(K, toi.transform.position + hM * 8f);
            // Dem LON NHAT theo tung khung: qua bay 17 m/s, cho 0,6 s roi dem mot lan thi vai qua da no (lan dau ra 2/3)
            int soQua4 = 0;
            float hanM4 = Time.time + 0.8f;
            while (Time.time < hanM4) { soQua4 = Mathf.Max(soQua4, DemQua() - truoc4); yield return null; }
            foreach (var q0 in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude)) Object.Destroy(q0.gameObject);
            yield return new WaitForSeconds(0.4f);
            if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            CapDo.NangCap(K);
            int cap5 = CapDo.CapCuaKyNang(K);
            toi.mana = toi.maxMana;
            int truoc5 = DemQua();
            toi.CastAt(K, toi.transform.position + hM * 8f);
            int soQua5 = 0;
            float hanM5 = Time.time + 0.8f;
            while (Time.time < hanM5) { soQua5 = Mathf.Max(soQua5, DemQua() - truoc5); yield return null; }
            foreach (var q0 in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude)) Object.Destroy(q0.gameObject);
            Ghi(string.Format("M. cap 4 (DOI CHUNG): {0} qua; cap {1}: {2} qua (mong 3 va 5)", soQua4, cap5, soQua5));
            Kiem(soQua4 == 3, "doi chung cap 4 khong phai 3 qua");
            Kiem(cap5 == 5 && soQua5 == 5, "cap 5 khong ra 5 qua");

            // NGUOI CHOI bi dong bang: khong di, khong tung phep
            yield return new WaitForSeconds(0.6f);
            var mauToiM = toi.GetComponent<Damageable>();
            float tocTruoc = toi.HeSoTocBang;
            bool khoaTruoc = toi.DangBiKhoaCung;
            FrozenEffect.Apply(mauToiM, 1.5f);
            yield return null;
            float tocSau = toi.HeSoTocBang;
            bool khoaSau = toi.DangBiKhoaCung;
            int truocBang = 0; int sauBang = 0;
            toi.mana = toi.maxMana;
            truocBang = DemQua();
            toi.CastAt(K, toi.transform.position + hM * 8f);
            yield return new WaitForSeconds(0.5f);
            sauBang = DemQua() - truocBang;
            string nhac = toi.LastMessage;
            var fM = toi.GetComponent<FrozenEffect>(); if (fM != null) Object.DestroyImmediate(fM);
            yield return new WaitForSeconds(0.5f);
            Ghi(string.Format("M. nguoi choi bi dong bang 1,5 s: he so toc {0:F2} -> {1:F2}, bi khoa cung {2} -> {3}; tung Qua cau bang luc dang bang ra {4} qua (nhac \"{5}\")",
                tocTruoc, tocSau, khoaTruoc, khoaSau, sauBang, nhac));
            Kiem(Mathf.Approximately(tocTruoc, 1f) && Mathf.Approximately(tocSau, 0f) && !khoaTruoc && khoaSau, "nguoi choi bi dong bang ma van di duoc");
            Kiem(sauBang == 0, "nguoi choi bi dong bang ma van tung duoc ky nang");
        }

        // ================= N. CAP 5: TANG BANG NO KHI HET GIO =================
        Ghi("");
        {
            foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude))
                if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
            foreach (var q0 in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude)) Object.Destroy(q0.gameObject);
            // ⚠️ DON MOI TANG BANG CON SONG. Muc M vua tung Qua cau bang THAT o cap 5, nen no de lai
            // may tang bang mang luat no - chung phat no trong 4 giay toi, dung vao giua luc muc N dang
            // do DOI CHUNG cap 4. Lan chay dau 19/09/2026 vi the bao "cap 4 ma tang bang van no" (2 vu
            // no, bia mat 55 mau = dung con so cua mot vu no o ria) - loi cua phep thu, khong phai cua game.
            foreach (var t0 in Object.FindObjectsByType<TangBangNo>(FindObjectsInactive.Exclude))
                if (t0 != null) Object.Destroy(t0.gameObject);
            yield return new WaitForSeconds(0.6f);

            Vector3 hN = HuongTrong(toi);
            Vector3 choN = toi.transform.position + hN * 9f;
            choN.y = VfxFactory.GroundY(choN);
            Vector3 ngang = Vector3.Cross(Vector3.up, hN).normalized;

            // Ba bia: ngay tai cho no, cach 3,0 m (trong 3,4 m) va cach 4,0 m (ngoai 3,4 m)
            var biaTam = TaoBia("TAM_NoTam", choN);
            var biaGan = TaoBia("TAM_NoGan", choN + ngang * 3.0f);
            var biaXa = TaoBia("TAM_NoXa", choN + ngang * 4.0f);
            yield return new WaitForFixedUpdate();
            float thatTam = Vector3.Distance(choN, biaTam.transform.position);
            float thatGan = Vector3.Distance(choN, biaGan.transform.position);
            float thatXa = Vector3.Distance(choN, biaXa.transform.position);

            // --- N1. DOI CHUNG: cap 4 thi tang bang tan im lang ---
            int no0 = TangBangNo.SoLanNo;
            var qua4 = VfxFactory.QuaCauBangRoi(choN, 3f, 0.12f);
            var fs4 = qua4.GetComponent<FallingShard>();
            fs4.damage = 1f; fs4.impactRadius = 1.7f; fs4.damageMask = maskEnemy;
            fs4.freezeChance = 1f; fs4.capKyNang = 4;
            yield return new WaitForSeconds(0.6f);
            float m4Tam = biaTam.health, m4Gan = biaGan.health;
            yield return new WaitForSeconds(5f);             // qua han 4 giay cua tang bang
            float mat4Tam = m4Tam - biaTam.health, mat4Gan = m4Gan - biaGan.health;
            int no4 = TangBangNo.SoLanNo - no0;

            // --- N2. CAP 5: tang bang no ---
            XoaHieuUng(biaTam); XoaHieuUng(biaGan); XoaHieuUng(biaXa);
            biaTam.health = biaTam.maxHealth; biaGan.health = biaGan.maxHealth; biaXa.health = biaXa.maxHealth;
            yield return null;

            int no1 = TangBangNo.SoLanNo;
            var qua5 = VfxFactory.QuaCauBangRoi(choN, 3f, 0.12f);
            var fs5 = qua5.GetComponent<FallingShard>();
            fs5.damage = 1f; fs5.impactRadius = 1.7f; fs5.damageMask = maskEnemy;
            fs5.freezeChance = 1f; fs5.capKyNang = 5;
            yield return new WaitForSeconds(0.6f);
            // Mau NGAY SAU cu roi (da tru sat thuong cua chinh qua bang) - vu no do tu moc nay
            float m5Tam = biaTam.health, m5Gan = biaGan.health, m5Xa = biaXa.health;
            int soTang = 0;
            foreach (var t in Object.FindObjectsByType<TangBangNo>(FindObjectsInactive.Exclude)) if (t != null) soTang++;
            yield return new WaitForSeconds(5f);
            float mat5Tam = m5Tam - biaTam.health, mat5Gan = m5Gan - biaGan.health, mat5Xa = m5Xa - biaXa.health;
            int no5 = TangBangNo.SoLanNo - no1;

            Ghi(string.Format("N. Mua bang CAP 4 (doi chung): {0} vu no; bia tai cho mat {1:F1} mau, bia 3,0 m mat {2:F1}",
                no4, mat4Tam, mat4Gan));
            Ghi(string.Format("N. Mua bang CAP 5: {0} tang bang co luat no, {1} vu no; bia tai cho ({2:F2} m) mat {3:F1}, bia {4:F2} m mat {5:F1}, bia {6:F2} m mat {7:F1} (ban kinh no {8} m, sat thuong {9})",
                soTang, no5, thatTam, mat5Tam, thatGan, mat5Gan, thatXa, mat5Xa, TangBangNo.BanKinh, TangBangNo.SatThuong));
            Kiem(no4 == 0 && mat4Tam < 1f, "doi chung hong: cap 4 ma tang bang van no");
            Kiem(soTang >= 1 && no5 >= 1, "cap 5 ma tang bang khong no");
            Kiem(Mathf.Abs(mat5Tam - TangBangNo.SatThuong) < 1f, "vu no khong gay dung 100 sat thuong o tam");
            Kiem(mat5Gan > 50f && mat5Gan < TangBangNo.SatThuong, "bia trong ban kinh khong an don giam dan");
            Kiem(mat5Xa < 1f, "bia NGOAI ban kinh 3,4 m ma van an don");

            // --- N3. QUA CAU BANG cap 5: tang bang sau vu no cung no ---
            XoaHieuUng(biaTam); XoaHieuUng(biaGan);
            biaTam.health = biaTam.maxHealth;
            yield return null;
            int no2 = TangBangNo.SoLanNo;
            var qc = QuaCauBang.Spawn(choN + hN * -3f + Vector3.up * 1f, hN, LayerMask.GetMask("Default"), maskEnemy);
            qc.capKyNang = 5;
            qc.boQua = mauToi;
            yield return new WaitForSeconds(1.2f);
            float mQC = biaTam.health;
            int tangQC = 0;
            foreach (var t in Object.FindObjectsByType<TangBangNo>(FindObjectsInactive.Exclude)) if (t != null) tangQC++;
            yield return new WaitForSeconds(5f);
            float matQC = mQC - biaTam.health;
            int noQC = TangBangNo.SoLanNo - no2;
            Ghi(string.Format("N. Qua cau bang CAP 5: {0} tang bang co luat no, {1} vu no; bia mat them {2:F1} mau sau khi qua cau da no xong",
                tangQC, noQC, matQC));
            Kiem(tangQC >= 1 && noQC >= 1, "Qua cau bang cap 5 ma tang bang khong no");
            Kiem(matQC > 50f, "vu no tang bang cua Qua cau bang khong gay sat thuong");

            Object.Destroy(biaTam.gameObject);
            Object.Destroy(biaGan.gameObject);
            Object.Destroy(biaXa.gameObject);
            foreach (var q0 in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude)) Object.Destroy(q0.gameObject);
            yield return new WaitForSeconds(0.4f);
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
        if (dir != null) dir.enabled = dirBatCu;
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
