using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 83): KY NANG 21 - MAY GIONG (nhom PHONG, nguoi dung 25/09/2026).
///
/// Nguoi dung: vung may giong (hinh nhu anh mau) ngay cho chon, tam = tam Sam set, 5 giay 20 tia set, moi tia 125 (cap 1),
/// 45% hat nga 0,85 giay, ke bi trung chay den toan than; hoi chieu 7 giay; moi hinh dung bang Blender.
/// Chot them: 50 nang luong, niem 0,5 s; vung 6 m uu tien ke dich; khong can dieu kien mo khoa; chay den chi hinh, 3 giay.
///
///   A. Thong so: so hieu 21, 22 ky nang, ten / tom tat / mo ta co dau, nhom PHONG, icon, 50 / 7 / 0,5, tam = Sam set (12),
///      vung ngam 6 m, khong dieu kien mo khoa, tinh la ky nang gay sat thuong (Tang hinh x2).
///   B. Tung that: ton 50, hoi chieu 7, may hien DUNG cho ngam; bam lai -> tu choi. Ngam 25 m -> may o 12 m (kep tam).
///   C. Nhip: dung 20 tia trong ~5 giay (tia dau ~0,35 s, cach nhau 0,25 s); moi tia cung MOT kieu (chup thong so).
///   D. Sat thuong: moi lan mat mau cua bia la BOI SO 125 (cap 1); ti le tia nham ke dich ~65%. DOI CHUNG vung trong: 0 tia nham.
///   E. Hat nga: ti le ~45% tren nhieu lan trung; do dai 0,85 s (cap 1).
///   F. Chay den: quai THAT (co SkinnedMeshRenderer) bi trung -> phu lop than den len MOI renderer; dam ~1; het 3 giay go SACH
///      (mang vat lieu tra lai dung nhu cu); chong voi vo bang thi go xong vo bang VAN CON.
///   G. Qua mang: TungPhepTheoMang(21) phat lai duoc.
///   H. Hinh: may nam NGANG tren cao 7 m, du dam may sang + xam, den; COT KHOI dung thang tu trong may xuong TAN MAT DAT
///      (hat + do thang anh CotMay.png: hang diem anh thap nhat con khoi); khoi cuon doc cot; anh chup dem + ngay o goc choi.
///   I. Tia HINH DANG Y GIUT SET (nguoi dung 25/09/2026): tung Giut set THAT tu nhan vat, chup thong so tia cua no, so voi tia
///      May giong - khong so voi hang so trong code (phep kiem doc lap). Rieng quang: May giong XANH DAM HON (do/luc < 60% cua
///      Giut set) va day x1,2; Giut set phai giu nguyen mau (0,14 0,34 1) + vien 1,10.
///   H (them). Quang may mong sat dat: 5 dam nam phang, cao < 0,6 m, lan ~3 m (kem phan anh thay duoc doc tu PNG).
///
/// Ket qua: PlayTestShots/maygiong.txt, anh maygiong_*.png.
/// </summary>
public static class ThuMayGiong
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyMayGiong;
    const int KyGiatSet = 6;
    static string kieuMayGiong;

    /// <summary>Thong so HINH DANG cua mot tia (bo qua duong di/so khuc - theo do dai tia; bo qua mau quang va do day quang -
    /// hai thu ay May giong co y lam khac Giut set, so rieng o muc I).</summary>
    static string KieuTia(LightningArc a)
    {
        return string.Format("anhBlender {0} | beNgang {1:F3} | loi {2:F3} | quang {3:F3} | mauLoi {4} | song {5:F2} s | nhanh {6} x {7:F2} | bung {8:F2}/{9:F2}",
            a.anhBlender, a.beNgang, a.coreWidth, a.glowWidth, a.coreColor, a.lifetime, a.branches, a.branchLength,
            a.coBungDau, a.coBungCuoi);
    }
    static Color quangMayGiong; static float vienMayGiong, haoMayGiong;

    /// <summary>Doc thang file PNG 2x2: diem anh xa tam o nhat con alpha > 0,25, chia cho nua o (lay o lan xa nhat).</summary>
    static float TiLeBanKinhAnh(string duong)
    {
        if (!File.Exists(duong)) return -1f;
        var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!t.LoadImage(File.ReadAllBytes(duong))) return -1f;
        int w = t.width / 2, h = t.height / 2; float xa = 0f;
        var px = t.GetPixels32();
        for (int o = 0; o < 4; o++)
        {
            int x0 = (o % 2) * w, y0 = (o / 2) * h;
            for (int y = 0; y < h; y += 2)
                for (int x = 0; x < w; x += 2)
                    if (px[(y0 + y) * t.width + x0 + x].a > 64)
                        xa = Mathf.Max(xa, new Vector2(x - w * 0.5f, y - h * 0.5f).magnitude / (w * 0.5f));
        }
        Object.DestroyImmediate(t);
        return xa;
    }

    /// <summary>Doc thang file PNG: hang diem anh THAP NHAT (tinh tu duoi o anh) con alpha > 0,25, lay o thap nhat trong 4 o.
    /// Tra ve ti le 0..1 theo chieu cao o; -1 neu khong doc duoc.</summary>
    static float TiLeChanKhoi(string duong)
    {
        if (!File.Exists(duong)) return -1f;
        var t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!t.LoadImage(File.ReadAllBytes(duong))) return -1f;
        int w = t.width / 2, h = t.height / 2; float tot = 1f;
        var px = t.GetPixels32();
        for (int o = 0; o < 4; o++)
        {
            int x0 = (o % 2) * w, y0 = (o / 2) * h;
            for (int y = 0; y < h; y++)
            {
                bool co = false;
                for (int x = 0; x < w && !co; x++) if (px[(y0 + y) * t.width + x0 + x].a > 64) co = true;
                if (co) { tot = Mathf.Min(tot, y / (float)h); break; }
            }
        }
        Object.DestroyImmediate(t);
        return tot;
    }

    [MenuItem("Diablo 2.5D/83. Chay thu MAY GIONG (ky nang moi)", false, 172)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu May giong", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false; kieuMayGiong = null;
        Ghi("[ban 1] May giong");
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
        if (GameObject.Find("TAM_MayGiong") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_MayGiong");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[MayGiong] " + s); }
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
        d.maxHealth = 1e6f; d.health = 1e6f;
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
            if (!Physics.SphereCast(goc, 1.5f, h, out RaycastHit _, 26f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static MayGiong TimMay(HashSet<MayGiong> cu)
    {
        foreach (var m in Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude)) if (!cu.Contains(m)) return m;
        return null;
    }

    static int DemMay() { return Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude).Length; }

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
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;

        // ================= A. THONG SO =================
        Ghi("");
        CapDo.BatDauTranMoi();
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var bo = IconKyNang.BoDayDu();
        bool trongPhong = System.Array.IndexOf(SachPhep.KyNangTheoNhom[3], K) >= 0;
        string mt = SachPhep.MoTa(K);
        Ghi(string.Format("A. so hieu {0}, so ky nang {1}; ten \"{2}\", tom tat \"{3}\", nhom PHONG {4}; icon {5}; nang luong {6} / hoi chieu {7} / niem {8} (Sach phep {9}/{10}/{11})",
            K, CapDo.SoKyNang, SachPhep.Ten(K), SachPhep.TomTat(K), trongPhong, bo != null && bo.Length > K && bo[K] != null,
            toi.mayGiongCost, toi.mayGiongCooldown, toi.mayGiongCastTime, nl, hc, nc));
        Ghi(string.Format("A. tam ngam {0} m (Sam set {1} m); vung ngam {2} m; mo khoa khong dieu kien {3}; gay sat thuong (Tang hinh x2) {4}; mo ta {5} ky tu",
            toi.TamNgam(K), toi.TamNgam(2), toi.BanKinhSatThuong(K), CapDo.DuBacDeMo(K), PlayerController.KyGaySatThuong(K), mt.Length));
        Kiem(K == 21 && CapDo.SoKyNang == 22 && SachPhep.SoKyNangCoTen() == CapDo.SoKyNang, "so hieu / so ky nang sai");
        Kiem(SachPhep.Ten(K) == "MÂY GIÔNG" && trongPhong, "ten / nhom PHONG sai");
        Kiem(mt.Contains("125") && mt.Contains("45%") && mt.Contains("0,85") && mt.Contains("3 giây") && mt.Contains("20 tia") && mt.Contains("5 giây"), "mo ta Sach phep thieu con so");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null, "thieu icon May giong");
        Kiem(Mathf.Approximately(toi.mayGiongCost, 50f) && Mathf.Approximately(toi.mayGiongCooldown, 7f) && Mathf.Approximately(toi.mayGiongCastTime, 0.5f)
             && Mathf.Approximately(nl, 50f) && Mathf.Approximately(hc, 7f), "nang luong / hoi chieu / niem sai");
        Kiem(Mathf.Approximately(toi.TamNgam(K), toi.TamNgam(2)) && Mathf.Abs(toi.TamNgam(K) - 12f) < 0.01f, "tam khong bang Sam set (12 m)");
        Kiem(Mathf.Approximately(toi.BanKinhSatThuong(K), 6f), "vung ngam khong phai 6 m");
        Kiem(CapDo.DuBacDeMo(K) && PlayerController.KyGaySatThuong(K), "May giong can dieu kien mo khoa / khong tinh la ky nang gay sat thuong");

        // ================= B + C + D. TUNG THAT, NHIP, SAT THUONG =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;
        CapDo.MoCaDuongChoPhepThu(K);
        Vector3 aim = goc + huong * 10f; aim.y = VfxFactory.GroundY(aim);
        // Cum 5 bia sat nhau o giua vung: moi tia nham ke dich trung ca cum (can du lan trung de dem)
        var bia = new List<Damageable>();
        for (int i = 0; i < 5; i++)
        {
            Vector2 l = Random.insideUnitCircle * 0.9f;
            bia.Add(TaoBia("TAM_BiaMG" + i, aim + new Vector3(l.x, 0f, l.y)));
        }
        var mau0 = new float[5]; for (int i = 0; i < 5; i++) mau0[i] = bia[i].health;
        var buocMat = new List<float>();
        var mayCu = new HashSet<MayGiong>(Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude));
        int tia0 = MayGiong.SoTiaDaDanh, nham0 = MayGiong.SoTiaNhamKeDich, trung0 = MayGiong.SoLanTrung, nga0 = MayGiong.SoLanNga;
        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana;
        toi.CastAt(K, aim);
        float manaTon = manaTruoc - toi.mana;
        float hoiNgaySau = toi.HoiChieuGiay(K);
        int soMayTruocBamLai = DemMay();
        toi.CastAt(K, aim);           // bam lai ngay: dang hoi chieu -> tu choi
        MayGiong may = null; float lucThay = -1f, lucTung = Time.time;
        for (float h = Time.time + 2f; Time.time < h && may == null; ) { may = TimMay(mayCu); if (may == null) yield return null; }
        if (may != null) lucThay = Time.time - lucTung;
        float lechCho = may != null ? new Vector2(may.transform.position.x - aim.x, may.transform.position.z - aim.z).magnitude : -1f;
        // Ghi ket qua NGAY: may tu tan sau 5,1 s, kiem sau vong do thi 'may' da la null (lan chay dau bao nham)
        bool thayMay = may != null;
        var mayDaThay = new HashSet<MayGiong>(); if (may != null) mayDaThay.Add(may);

        // May quay nhin vung may (he hat cuc bo ngoai khung khong mo phong)
        if (rig != null) rig.enabled = false;
        if (cam != null) { cam.transform.position = aim - huong * 22f + Vector3.up * 6f; cam.transform.LookAt(aim + Vector3.up * 6f); }

        // C. dem tia + thoi diem, D. buoc mat mau tung bia moi khung
        var tiaCu = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
        var lucTia = new List<float>(); int anhBlender = 0, cungKieu = 0;
        float lucMay = Time.time;
        var mauTruoc = new float[5]; for (int i = 0; i < 5; i++) mauTruoc[i] = bia[i].health;
        bool daChup = false;
        float hanC = Time.time + 6.2f;
        while (Time.time < hanC)
        {
            foreach (var mm in Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude)) if (!mayCu.Contains(mm)) mayDaThay.Add(mm);
            foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
            {
                if (!tiaCu.Add(a) || a.name != "TiaMayGiong") continue;
                lucTia.Add(Time.time - lucMay);
                if (a.anhBlender) anhBlender++;
                string kieu = KieuTia(a);
                if (kieuMayGiong == null) { kieuMayGiong = kieu; quangMayGiong = a.glowColor; vienMayGiong = a.heSoVien; haoMayGiong = a.heSoHaoQuang; }
                if (kieu == kieuMayGiong && a.glowColor == quangMayGiong && Mathf.Abs(a.heSoVien - vienMayGiong) < 1e-4f) cungKieu++;
            }
            for (int i = 0; i < 5; i++)
            {
                float mat = mauTruoc[i] - bia[i].health;
                if (mat > 0.01f) { buocMat.Add(mat); mauTruoc[i] = bia[i].health; }
            }
            if (!daChup && Time.time - lucMay > 2.0f && cam != null)
            {
                daChup = true;
                cam.transform.position = aim - huong * 16f + Vector3.up * 1.8f;
                cam.transform.LookAt(aim + Vector3.up * 7f);
                yield return null;
                yield return Chup("maygiong_1_dem_can");
                cam.transform.position = aim - huong * 22f + Vector3.up * 6f; cam.transform.LookAt(aim + Vector3.up * 6f);
            }
            yield return null;
        }
        int soTia = MayGiong.SoTiaDaDanh - tia0, soNham = MayGiong.SoTiaNhamKeDich - nham0;
        int boiSo125 = 0; foreach (var m in buocMat) { float k = m / 125f; if (Mathf.Abs(k - Mathf.Round(k)) < 0.01f && Mathf.Round(k) >= 1f) boiSo125++; }
        float khoangTB = lucTia.Count > 1 ? (lucTia[lucTia.Count - 1] - lucTia[0]) / (lucTia.Count - 1) : 0f;
        Ghi(string.Format("B. ton {0} nang luong, hoi chieu ngay sau {1:F2} s; bam lai ngay khi dang hoi chieu -> suot 6 giay chi {2} dam may; may hien sau {3:F2} s, lech cho ngam {4:F2} m",
            manaTon, hoiNgaySau, mayDaThay.Count, lucThay, lechCho));
        Ghi(string.Format("C. {0} tia (bo dem {1}); tia dau {2:F2} s, tia cuoi {3:F2} s, cach nhau TB {4:F3} s; anh Blender {5}, cung mot kieu {6}",
            lucTia.Count, soTia, lucTia.Count > 0 ? lucTia[0] : -1f, lucTia.Count > 0 ? lucTia[lucTia.Count - 1] : -1f, khoangTB, anhBlender, cungKieu));
        Ghi(string.Format("D. {0} lan bia mat mau, boi so 125: {1}; tia nham ke dich {2}/{3} ({4:P0}, mong ~65%)",
            buocMat.Count, boiSo125, soNham, soTia, soTia > 0 ? soNham / (float)soTia : 0f));
        Kiem(Mathf.Abs(manaTon - 50f) < 0.01f && Mathf.Abs(hoiNgaySau - 7f) < 0.05f, "khong ton 50 / hoi chieu 7");
        Kiem(thayMay && lechCho >= 0f && lechCho < 0.3f, "may khong hien dung cho ngam");
        Kiem(mayDaThay.Count == 1, "bam lai khi dang hoi chieu van ra may moi");
        Kiem(lucTia.Count == 20 && soTia == 20, "khong dung 20 tia");
        Kiem(lucTia.Count == 20 && lucTia[0] > 0.2f && lucTia[0] < 0.6f && lucTia[19] < 5.6f && Mathf.Abs(khoangTB - 0.25f) < 0.03f, "20 tia khong trai deu trong 5 giay");
        Kiem(anhBlender == lucTia.Count && cungKieu == lucTia.Count, "tia khong cung mot kieu anh Blender");
        Kiem(buocMat.Count >= 10 && boiSo125 == buocMat.Count, "moi tia khong gay dung 125 (cap 1)");
        Kiem(soTia > 0 && soNham / (float)soTia > 0.4f && soNham / (float)soTia < 0.9f, "ti le nham ke dich khong quanh 65%");

        // DOI CHUNG vung trong (khong ai): 0 tia nham ke dich, van du 20 tia
        foreach (var b in bia) Object.Destroy(b.gameObject);
        bia.Clear();
        yield return new WaitForSeconds(0.3f);
        tia0 = MayGiong.SoTiaDaDanh; nham0 = MayGiong.SoTiaNhamKeDich;
        var mayTrong = MayGiong.Spawn(goc + huong * 12f, maskEnemy, mauToi);
        yield return new WaitForSeconds(5.8f);
        Ghi(string.Format("D2. DOI CHUNG vung trong: {0} tia, nham ke dich {1}", MayGiong.SoTiaDaDanh - tia0, MayGiong.SoTiaNhamKeDich - nham0));
        Kiem(MayGiong.SoTiaDaDanh - tia0 == 20 && MayGiong.SoTiaNhamKeDich - nham0 == 0, "DOI CHUNG: vung trong van nham ke dich / thieu tia");

        // ================= E. HAT NGA =================
        Ghi("");
        {
            var cum = new List<Damageable>();
            for (int i = 0; i < 6; i++) { Vector2 l = Random.insideUnitCircle * 0.8f; cum.Add(TaoBia("TAM_BiaNga" + i, goc + huong * 12f + new Vector3(l.x, 0f, l.y))); }
            trung0 = MayGiong.SoLanTrung; nga0 = MayGiong.SoLanNga;
            float giayNgaDo = -1f;
            for (int lan = 0; lan < 3; lan++)
            {
                MayGiong.Spawn(goc + huong * 12f, maskEnemy, mauToi);
                float hanE = Time.time + 5.7f;
                while (Time.time < hanE)
                {
                    if (giayNgaDo < 0f) foreach (var b in cum) { var n = b.GetComponent<BiDanhNga>(); if (n != null) { giayNgaDo = n.thoiGian; break; } }
                    yield return null;
                }
            }
            int trung = MayGiong.SoLanTrung - trung0, nga = MayGiong.SoLanNga - nga0;
            float tile = trung > 0 ? nga / (float)trung : 0f;
            Ghi(string.Format("E. 3 dam may x 20 tia vao cum 6 bia: {0} lan trung, {1} lan hat nga ({2:P1}, mong 45%); thoi gian nga do duoc {3:F2} s (mong 0,85)", trung, nga, tile, giayNgaDo));
            Kiem(trung >= 150 && Mathf.Abs(tile - MayGiong.XacSuatNga) < 0.08f, "ti le hat nga khong quanh 45%");
            Kiem(Mathf.Abs(giayNgaDo - 0.85f) < 0.01f, "hat nga khong phai 0,85 giay (cap 1)");
            foreach (var b in cum) Object.Destroy(b.gameObject);
            yield return new WaitForSeconds(1.5f);
        }

        // ================= F. CHAY DEN TOAN THAN (quai that, model co xuong) =================
        Ghi("");
        {
            // Chon con quai co MODEL XUONG (SkinnedMeshRenderer) - con dau kho (Quy lun) dung bang code, 0 SkinnedMesh
            GameObject pf = null;
            if (GameAssets.I != null) foreach (var p in GameAssets.I.enemyPrefabs) if (p != null && p.GetComponentInChildren<SkinnedMeshRenderer>(true) != null) { pf = p; break; }
            Vector3 cho = goc + huong * 9f; cho.y = VfxFactory.GroundY(cho);
            var quai = pf != null ? Object.Instantiate(pf, cho, Quaternion.identity) : null;
            if (quai == null) { Ghi("[LOI] khong co quai trong kho de thu chay den"); loi++; }
            else
            {
                quai.name = "TAM_QuaiChayDen";
                foreach (var ai in quai.GetComponentsInChildren<EnemyAI>()) ai.enabled = false;
                var dq = quai.GetComponent<Damageable>(); dq.maxHealth = 1e6f; dq.health = 1e6f;
                yield return null;
                // Mang vat lieu goc cua moi renderer mo duoc
                var goc0 = new Dictionary<Renderer, Material[]>(); int soSkinned = 0;
                foreach (var r in quai.GetComponentsInChildren<Renderer>())
                {
                    if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
                    bool ts = false; foreach (var m in r.sharedMaterials) if (m == null || m.renderQueue >= 3000) ts = true;
                    if (ts) continue;
                    goc0[r] = r.sharedMaterials; if (r is SkinnedMeshRenderer) soSkinned++;
                }
                // Trung tia that: may giong ngay tren quai
                int apDung0 = ChayDenToanThan.SoLanApDung;
                var mq = MayGiong.Spawn(cho, maskEnemy, mauToi);
                ChayDenToanThan cd = null;
                for (float h = Time.time + 5f; Time.time < h && cd == null; ) { cd = quai.GetComponent<ChayDenToanThan>(); if (cd == null) yield return null; }
                float lucThayChay = Time.time;
                Object.Destroy(mq.gameObject);                 // dung may: khong bi danh them, do dung 3 giay
                yield return new WaitForSeconds(0.4f);
                int phuDu = 0;
                if (cd != null)
                    foreach (var kv in goc0) if (kv.Key.sharedMaterials.Length == kv.Value.Length + 1 && kv.Key.sharedMaterials[kv.Value.Length].name.StartsWith("P_ChayDen")) phuDu++;
                float dam = cd != null ? cd.DoDam : -1f;
                if (cam != null) { cam.transform.position = cho - huong * 5f + Vector3.up * 2.2f; cam.transform.LookAt(cho + Vector3.up * 1.1f); }
                yield return null;
                yield return Chup("maygiong_2_chay_den");
                float giayCo = -1f;
                for (float h = Time.time + 5f; Time.time < h; )
                {
                    if (quai.GetComponent<ChayDenToanThan>() == null) { giayCo = Time.time - lucThayChay; break; }
                    yield return null;
                }
                yield return null;
                // Tra lai dung: so o vat lieu ve nhu cu va KHONG con lop than den. So theo tham chieu thi sai: trung don quai nhay
                // trang bang BAN SAO vat lieu (lan chay dau bao 2/33 'tra dung' vi vay, khong phai loi chay den)
                int traDung = 0;
                foreach (var kv in goc0)
                {
                    var hienTai = kv.Key.sharedMaterials; bool giong = hienTai.Length == kv.Value.Length;
                    foreach (var m in hienTai) if (m != null && m.name.StartsWith("P_ChayDen")) giong = false;
                    if (giong) traDung++;
                }
                Ghi(string.Format("F. quai '{0}': {1} renderer mo ({2} SkinnedMesh); bi tia that trung -> chay den (ap dung +{3}); phu lop than den {4}/{1}, do dam {5:F2}; tu go sau ~{6:F2} s (mong 3); vat lieu tra lai dung {7}/{1}",
                    pf.name, goc0.Count, soSkinned, ChayDenToanThan.SoLanApDung - apDung0, phuDu, dam, giayCo, traDung));
                Kiem(goc0.Count > 0 && soSkinned > 0 && phuDu == goc0.Count, "lop chay den khong phu du moi renderer (ca SkinnedMesh)");
                Kiem(dam > 0.95f, "chay den khong dam");
                Kiem(Mathf.Abs(giayCo - 3f) < 0.25f, "chay den khong keo dai 3 giay");
                Kiem(traDung == goc0.Count, "go chay den khong tra lai dung vat lieu goc");

                // DOI CHUNG chong vo bang: bang truoc, chay den sau, chay den go ra thi vo bang VAN CON
                FrozenEffect.Apply(dq, 6f);
                yield return null; yield return null;
                ChayDenToanThan.Apply(dq, 0.6f);
                yield return new WaitForSeconds(1.2f);
                int conBang = 0, conChay = 0;
                foreach (var r in goc0.Keys)
                    foreach (var m in r.sharedMaterials) { if (m != null && m.shader == Mats.FrozenShader) conBang++; if (m != null && m.name.StartsWith("P_ChayDen")) conChay++; }
                Ghi(string.Format("F2. bang 6 s + chay den 0,6 s: sau khi chay den go -> lop vo bang con {0}, lop chay den con {1}", conBang, conChay));
                Kiem(conBang > 0 && conChay == 0, "go chay den lam mat vo bang (hoac chay den khong go)");
                var fe = dq.GetComponent<FrozenEffect>(); if (fe != null) fe.Thaw();
                Object.Destroy(quai);
            }
        }

        // ================= G. QUA MANG =================
        Ghi("");
        {
            int truoc = DemMay();
            toi.TungPhepTheoMang(K, goc + huong * 8f, 0f, 1, false);
            MayGiong m2 = null;
            for (float h = Time.time + 2f; Time.time < h && m2 == null; ) { foreach (var m in Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude)) m2 = m; if (m2 == null) yield return null; }
            Ghi(string.Format("G. TungPhepTheoMang(21): may {0} -> {1}", truoc, DemMay()));
            Kiem(m2 != null, "goi phep qua mang khong phat lai May giong");

            // ================= H. HINH =================
            yield return new WaitForSeconds(1.0f);
            // Lay DUNG dam may cua con May giong o muc G (theo vi tri): Find theo ten co luc bat trung may cu cua muc truoc dang tan
            GameObject hinh = null;
            if (m2 != null)
                foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                    if (tr.name == "MayGiongHinh" && new Vector2(tr.position.x - m2.transform.position.x, tr.position.z - m2.transform.position.z).magnitude < 0.3f) hinh = tr.gameObject;
            int sang = 0, xam = 0, den = 0; float lechY = 0f, xaNgang = 0f;
            if (hinh != null)
            {
                foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>())
                {
                    if (ps.name != "MaySang" && ps.name != "MayXam") continue;
                    var hat = new ParticleSystem.Particle[ps.main.maxParticles]; int n = ps.GetParticles(hat);
                    if (ps.name == "MaySang") sang = n; else xam = n;
                    for (int i = 0; i < n; i++)
                    {
                        lechY = Mathf.Max(lechY, Mathf.Abs(hat[i].position.y - ps.transform.position.y));
                        xaNgang = Mathf.Max(xaNgang, new Vector2(hat[i].position.x - hinh.transform.position.x, hat[i].position.z - hinh.transform.position.z).magnitude);
                    }
                }
                den = hinh.GetComponentsInChildren<Light>().Length;
            }
            Ghi(string.Format("H. may: dam sang {0}, dam xam {1}; lech doc toi da {2:F2} m (nam NGANG), xa tam toi da {3:F2} m (vung 6); den {4}; cao day may {5} m",
                sang, xam, lechY, xaNgang, den, MayGiong.CaoMay));
            Kiem(sang == 26 && xam == 16 && den >= 1, "thieu dam may / den");
            Kiem(lechY < 0.3f && xaNgang > 3f && xaNgang < 6.5f, "may khong nam ngang trai tren vung 6 m");

            // Cot khoi: hat dung thang, chan duoi dat, dinh trong may; do THANG anh xem khoi thuc su chay toi dau
            int soCot = 0, soKhoiCot = 0; bool dungThang = false; float chanHat = 99f, dinhHat = -99f, rongCot = 0f;
            float matDat = hinh != null ? hinh.transform.position.y : 0f;
            if (hinh != null)
                foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>())
                {
                    var hat = new ParticleSystem.Particle[ps.main.maxParticles]; int n = ps.GetParticles(hat);
                    if (ps.name == "KhoiCot") soKhoiCot = n;
                    if (ps.name != "CotMay") continue;
                    soCot = n;
                    dungThang = ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.VerticalBillboard;
                    for (int i = 0; i < n; i++)
                    {
                        Vector3 c3 = hat[i].GetCurrentSize3D(ps);
                        chanHat = Mathf.Min(chanHat, hat[i].position.y - c3.y * 0.5f - matDat);
                        dinhHat = Mathf.Max(dinhHat, hat[i].position.y + c3.y * 0.5f - matDat);
                        rongCot = Mathf.Max(rongCot, c3.x);
                    }
                }
            // Hang diem anh thap nhat con khoi (alpha > 0,25) trong 4 o anh -> do cao khoi thay duoc o chan cot
            float tiLeChanAnh = TiLeChanKhoi("Assets/Resources/KyNang/MayGiong/CotMay.png");
            float chanThay = chanHat + tiLeChanAnh * (dinhHat - chanHat);
            Ghi(string.Format("H. cot khoi: {0} lop dung thang {1}, rong {2:F1} m; hat tu {3:F2} m toi {4:F2} m tren mat dat (day may {5:F1} m); anh: khoi bat dau o {6:P1} chieu cao -> khoi thay duoc tu {7:F2} m; khoi cuon doc cot {8} hat",
                soCot, dungThang, rongCot, chanHat, dinhHat, MayGiong.CaoMay - 0.5f, tiLeChanAnh, chanThay, soKhoiCot));
            Kiem(soCot == VfxFactory.SoLopCotMay && dungThang, "thieu cot khoi / cot khong dung thang");
            Kiem(dinhHat >= MayGiong.CaoMay - 0.5f, "cot khoi khong vao toi day may");
            Kiem(tiLeChanAnh >= 0f && chanThay <= 0.3f && chanThay >= -0.6f, "cot khoi khong cham mat dat");
            Kiem(soKhoiCot >= 5, "khong co khoi cuon doc cot");

            // Quang may mong sat dat: dam nam PHANG, sat dat, lan ra ~3 m (tinh ca phan anh thay duoc - doc thang PNG)
            int soSuong = 0, soMayThap = 0; bool nam = false; float caoSuong = -1f, xaSuong = 0f;
            float tiLeSuong = TiLeBanKinhAnh("Assets/Resources/KyNang/MayGiong/SuongDat.png");
            if (hinh != null)
                foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>())
                {
                    var hat = new ParticleSystem.Particle[ps.main.maxParticles]; int n = ps.GetParticles(hat);
                    if (ps.name == "MayThap") soMayThap = n;
                    if (ps.name != "SuongDat") continue;
                    soSuong = n;
                    nam = ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.HorizontalBillboard;
                    for (int i = 0; i < n; i++)
                    {
                        caoSuong = Mathf.Max(caoSuong, hat[i].position.y - matDat);
                        float xa = new Vector2(hat[i].position.x - hinh.transform.position.x, hat[i].position.z - hinh.transform.position.z).magnitude;
                        xaSuong = Mathf.Max(xaSuong, xa + hat[i].GetCurrentSize(ps) * 0.5f * tiLeSuong);
                    }
                }
            Ghi(string.Format("H. quang may sat dat: {0} dam nam phang {1}, cao {2:F2} m tren dat; anh: suong toi {3:P0} nua o -> lan xa tam {4:F2} m (mong ~3,45; ban 3 m do 3,13); cum may thap {5}",
                soSuong, nam, caoSuong, tiLeSuong, xaSuong, soMayThap));
            Kiem(soSuong == VfxFactory.SoDamSuongDat && nam, "thieu quang may sat dat / khong nam phang");
            Kiem(caoSuong >= 0f && caoSuong < 0.6f, "quang may khong sat mat dat");
            Kiem(xaSuong > 2.9f && xaSuong < 4.6f, "quang may khong lan ~3,45 m");
            Kiem(soMayThap >= 4, "thieu cum may thap");

            // Nguoi dung 25/09 toi: quang sat dat RONG x1,15, DAC + CAO x1,10 - so voi thong so ban truoc (3d312bd, chep tay o day)
            if (hinh != null)
            {
                var sd = hinh.transform.Find("SuongDat"); var trMt = hinh.transform.Find("MayThap");
                if (sd != null && trMt != null)
                {
                    var psSd = sd.GetComponent<ParticleSystem>(); var psMt = trMt.GetComponent<ParticleSystem>();
                    float rongCo = psSd.main.startSize.constantMax / 5.0f, rongRai = psSd.shape.radius / 1.6f, rongThap = psMt.shape.radius / 2.2f;
                    float dacSd = sd.GetComponent<ParticleSystemRenderer>().sharedMaterial.GetColor("_TintColor").a / 0.85f;
                    float dacMt = trMt.GetComponent<ParticleSystemRenderer>().sharedMaterial.GetColor("_TintColor").a / 0.45f;
                    float caoCo = psMt.main.startSize.constantMax / 2.0f, caoDat = trMt.localPosition.y / 0.45f;
                    Ghi(string.Format("H. so voi ban truoc: rong co dam x{0:F3}, rai x{1:F3}, cum thap rai x{2:F3} | dac suong x{3:F3}, dac cum thap x{4:F3} | cum thap co x{5:F3}, cao x{6:F3}",
                        rongCo, rongRai, rongThap, dacSd, dacMt, caoCo, caoDat));
                    Kiem(Mathf.Abs(rongCo - 1.15f) < 0.005f && Mathf.Abs(rongRai - 1.15f) < 0.005f && Mathf.Abs(rongThap - 1.15f) < 0.005f, "quang sat dat khong rong x1,15");
                    Kiem(Mathf.Abs(dacSd - 1.10f) < 0.005f && Mathf.Abs(dacMt - 1.10f) < 0.005f && Mathf.Abs(caoCo - 1.10f) < 0.005f && Mathf.Abs(caoDat - 1.10f) < 0.005f, "quang sat dat khong dac/cao x1,10");
                }
                else Kiem(false, "khong tim thay lop SuongDat / MayThap");

                // May giong DEN: do sang (luminance) mau tung lop so voi mau ban truoc (chep tay) ~0,26; day may toi hon dinh
                string[] tenLop = { "MaySang", "MayXam", "CotMay", "KhoiCot", "SuongDat", "MayThap" };
                Color[] mauCu = { new Color(0.95f, 0.97f, 1f), new Color(0.50f, 0.53f, 0.60f), new Color(0.80f, 0.84f, 0.92f),
                               new Color(0.70f, 0.74f, 0.82f), new Color(0.80f, 0.84f, 0.92f), new Color(0.70f, 0.74f, 0.82f) };
                var sbT = new StringBuilder(); int toiDung = 0; float lumSang = 0f, lumXam = 0f;
                for (int i = 0; i < tenLop.Length; i++)
                {
                    var trT = hinh.transform.Find(tenLop[i]);
                    if (trT == null) { sbT.Append(tenLop[i] + " THIEU; "); continue; }
                    Color mauT = trT.GetComponent<ParticleSystemRenderer>().sharedMaterial.GetColor("_TintColor");
                    float lumM = 0.2126f * mauT.r + 0.7152f * mauT.g + 0.0722f * mauT.b, lumC = 0.2126f * mauCu[i].r + 0.7152f * mauCu[i].g + 0.0722f * mauCu[i].b;
                    float tiLe = lumM / lumC;
                    if (tiLe > 0.22f && tiLe < 0.32f) toiDung++;
                    if (i == 0) lumSang = lumM; if (i == 1) lumXam = lumM;
                    sbT.Append(string.Format("{0} {1:F3} (x{2:F2}); ", tenLop[i], lumM, tiLe));
                }
                Ghi("H. may DEN - do sang mau tung lop: " + sbT);
                Kiem(toiDung == tenLop.Length, "co lop may chua toi ~26% mau cu");
                Kiem(lumXam < lumSang, "day may khong toi hon dinh may");
            }
            // Set roi sang: ca dam loe (he so > 1,5) roi tat han ve 1 giua cac tia; moi tia mot mang sang
            {
                var boLoe = hinh != null ? hinh.GetComponent<LoeSangMay>() : null;
                int mang0 = VfxFactory.SoMangSangMay, tia0b = MayGiong.SoTiaDaDanh, soKhung = 0, khungToi = 0; float kMax = 0f;
                for (float hanLoe = Time.time + 2f; Time.time < hanLoe && boLoe != null; )
                {
                    yield return null;
                    soKhung++; kMax = Mathf.Max(kMax, boLoe.HeSoHienTai);
                    if (boLoe.HeSoHienTai < 1.02f) khungToi++;
                }
                int soMang = VfxFactory.SoMangSangMay - mang0, soTiaB = MayGiong.SoTiaDaDanh - tia0b;
                Ghi(string.Format("H. set roi sang: {0} lop may, he so loe cao nhat x{1:F2}, {2}/{3} khung may o mau toi goc ({4:P0}); {5} tia -> {6} mang sang",
                    boLoe != null ? boLoe.SoRenderer : 0, kMax, khungToi, soKhung, soKhung > 0 ? khungToi / (float)soKhung : 0f, soTiaB, soMang));
                Kiem(boLoe != null && boLoe.SoRenderer == 6, "thieu bo set roi sang tren 6 lop may");
                Kiem(kMax > 1.5f, "set khong roi sang dam may");
                Kiem(soKhung > 0 && khungToi / (float)soKhung > 0.25f, "may sang gan het thoi gian - khong con toi");
                Kiem(soTiaB >= 5 && soMang == soTiaB, "moi tia khong co mot mang sang");
            }
            if (m2 != null) Object.Destroy(m2.gameObject);
        }

        // ================= I. TIA GIONG Y GIUT SET =================
        {
            CapDo.MoCaDuongChoPhepThu(KyGiatSet);
            yield return new WaitForSeconds(0.8f);
            var cuI = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            toi.mana = toi.maxMana;
            toi.CastAt(KyGiatSet, toi.transform.position + huong * 8f);
            string kieuGiatSet = null; int soTiaGS = 0; Color quangGS = Color.clear; float vienGS = 0f, haoGS = 0f; var cacKieu = new List<string>();
            for (float h = Time.time + 1.5f; Time.time < h; )
            {
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (!cuI.Add(a) || a.name == "TiaMayGiong" || a.name == "TiaNgangMayGiong") continue;
                    if (Vector3.Distance(a.start, toi.transform.position) > 3f) continue;   // chi tia ra tu tay nhan vat
                    soTiaGS++;
                    string kk = KieuTia(a);
                    if (!cacKieu.Contains(kk)) cacKieu.Add(kk);
                    // Tia CHINH cua Giut set la tia ve bang anh Blender; con lai la tia loe phu luc niem o tay (song 0,11 s)
                    if (kieuGiatSet == null && a.anhBlender) { kieuGiatSet = kk; quangGS = a.glowColor; vienGS = a.heSoVien; haoGS = a.heSoHaoQuang; }
                }
                yield return null;
            }
            foreach (var kk in cacKieu) Ghi("I. moi kieu tia bay ra khi tung Giut set: " + kk);
            Ghi("I. tia CHINH Giut set THAT (" + soTiaGS + " tia): " + kieuGiatSet);
            Ghi("I. tia May giong:              " + kieuMayGiong);
            Kiem(kieuGiatSet != null, "tung Giut set khong ra tia de so");
            Kiem(kieuGiatSet != null && kieuGiatSet == kieuMayGiong, "HINH DANG tia May giong KHAC tia Giut set");
            // Mau: May giong XANH DAM HON (do + luc thap hon ro, lam van du), vien + hao quang day x1,2 so voi tia Giut set THAT
            Ghi(string.Format("I. quang Giut set {0} vien {1:F3} hao quang {2:F3} | May giong {3} vien {4:F3} hao quang {5:F3} (ti le {6:F2} / {7:F2})",
                quangGS, vienGS, haoGS, quangMayGiong, vienMayGiong, haoMayGiong, vienGS > 0f ? vienMayGiong / vienGS : 0f, haoGS > 0f ? haoMayGiong / haoGS : 0f));
            Kiem(quangMayGiong.r < quangGS.r * 0.6f && quangMayGiong.g < quangGS.g * 0.6f && quangMayGiong.b >= 0.99f, "quang May giong khong xanh dam hon Giut set");
            Kiem(vienGS > 0f && Mathf.Abs(vienMayGiong / vienGS - 1.2f) < 0.01f && haoGS > 0f && Mathf.Abs(haoMayGiong / haoGS - 1.2f) < 0.01f, "quang May giong khong day x1,2");
            // Giut set KHONG bi doi theo (so voi mau nguoi dung da duyet 25/09/2026, CLAUDE.md)
            Kiem(Mathf.Abs(quangGS.r - 0.14f) < 0.005f && Mathf.Abs(quangGS.g - 0.34f) < 0.005f && Mathf.Abs(vienGS - 1.10f) < 1e-3f, "Giut set bi doi mau / vien theo");
        }

        // Anh ban dem o goc choi that (cot khoi phai thay tu may xuong dat)
        if (rig != null) rig.enabled = true;
        yield return new WaitForSeconds(0.6f);
        {
            Vector3 truocD = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : huong;
            var mDem = MayGiong.Spawn(toi.transform.position + truocD * 10f, maskEnemy, mauToi);
            yield return new WaitForSeconds(2.2f);
            yield return Chup("maygiong_4_dem_goc_choi");
            if (mDem != null) Object.Destroy(mDem.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        // Anh ban ngay o goc choi that
        var ngay = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (ngay != null) { ngay.enabled = false; ngay.ApGiay(ChuyenChieuSangDem.GiayGiuaNgay); }
        if (rig != null) rig.enabled = true;
        yield return new WaitForSeconds(0.6f);
        Vector3 truoc2 = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : huong;
        var mAnh = MayGiong.Spawn(toi.transform.position + truoc2 * 10f, maskEnemy, mauToi);
        yield return new WaitForSeconds(2.2f);
        yield return Chup("maygiong_3_ngay_goc_choi");
        if (mAnh != null) Object.Destroy(mAnh.gameObject);
        if (mayTrong != null) Object.Destroy(mayTrong.gameObject);

        Ghi("");
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[MayGiong] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        Ghi("so loi ghi nhan = " + loi);
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/maygiong.txt", bao.ToString());
        var rac = GameObject.Find("TAM_MayGiong");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
