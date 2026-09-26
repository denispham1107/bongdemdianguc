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
///   ⚠️ H do cot khoi / quang sat dat bang HINH VE THAT (ParticleSystemRenderer.BakeMesh): hat Vertical/HorizontalBillboard ve
///      chi bang 0,7071 kich thuoc dat - ban truoc doc kich thuoc DAT nen bao "cot cham dat" trong khi chan cot lo lung ~0,9 m.
///   ⚠️ 26/09/2026: hoi chieu 5,5 s; BO cot khoi + quang may sat dat (H kiem khong con); may to x1,15 (chi hinh); het 5 giay may
///      TU BAY 4 giay 1,5 m/s huong "ngau nhien" GIONG NHAU moi may (bam tu diem ngam da nen nhu goi tin), vua bay vua mua +
///      set (tong 36 tia) - C do quang duong / huong / bam dat / tia roi quanh cho may DANG O; K: mua dap tat lo lua, 30 s chay lai.
///   J. MUA (nguoi dung 25/09/2026 khuya): vet mua roi va cham dat that (vong nuoc nam dung mat dat do bang tia Ground); moi doi
///      thu trong vung 6 m BI UOT (nguoi tung khong, ngoai vung khong), het mua / ra khoi vung thi uot them 5 giay; hinh uot tren
///      quai co SkinnedMesh; +50% cho Giut set / Sam set / Qua cau dien / tia May giong len ke uot (so voi ke kho cung don),
///      DOI CHUNG: Giut set cua QUAI va sat thuong vung thuong KHONG duoc cong; moi tia May giong mot vet chay xem SetChayDen
///      nhu tia Sam set that; cot khoi rong x1,2.
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

    /// <summary>
    /// HINH VE THAT cua mot he hat: BakeMesh (toa do the gioi) roi tach moi 4 dinh thanh mot tam. Hat Vertical/HorizontalBillboard
    /// ve chi bang 0,7071 kich thuoc dat - doc kich thuoc hat la do sai (menu 83 da bao sai mot lan).
    /// </summary>
    static float hetUotA = -1f, hetUotR = -1f;

    /// <summary>Moi renderer mo cua quai deu mang DU hai lop phu P_BiUot va P_ChayDen.</summary>
    static bool HaiLopPhu(GameObject q)
    {
        int soR = 0, du = 0;
        foreach (var rr in q.GetComponentsInChildren<Renderer>())
        {
            if (!(rr is MeshRenderer) && !(rr is SkinnedMeshRenderer)) continue;
            soR++; bool u = false, c = false;
            foreach (var mm in rr.sharedMaterials) { if (mm == null) continue; if (mm.name.StartsWith("P_BiUot")) u = true; if (mm.name.StartsWith("P_ChayDen")) c = true; }
            if (u && c) du++;
        }
        return soR > 0 && du == soR;
    }
    static readonly Dictionary<Renderer, Material[]> vatLieuGocQ = new Dictionary<Renderer, Material[]>();

    /// <summary>Ghi so giay tu moc toi luc het uot (chay song song voi kich ban chinh).</summary>
    static IEnumerator TheoDoiHetUot(Damageable d, float moc, bool laDiRa)
    {
        for (float h = Time.time + 12f; Time.time < h && d != null; )
        {
            if (!BiUot.DangUot(d)) { if (laDiRa) hetUotR = Time.time - moc; else hetUotA = Time.time - moc; yield break; }
            yield return null;
        }
    }

    static List<Vector3[]> TamVeThat(ParticleSystem ps, Camera cam)
    {
        var ds = new List<Vector3[]>();
        if (ps == null || cam == null) return ds;
        var luoi = new Mesh();
        ps.GetComponent<ParticleSystemRenderer>().BakeMesh(luoi, cam, ParticleSystemBakeMeshOptions.Default);
        var v = luoi.vertices;
        for (int i = 0; i + 3 < v.Length; i += 4) ds.Add(new[] { v[i], v[i + 1], v[i + 2], v[i + 3] });
        Object.Destroy(luoi);
        return ds;
    }

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
        Ghi(string.Format("A. tam ngam {0} m (Sam set {1} m); vung ngam {2} m; du bac de mo ngay dau tran {3} (phai False - can Loc xoay cap 5); gay sat thuong (Tang hinh x2) {4}; mo ta {5} ky tu",
            toi.TamNgam(K), toi.TamNgam(2), toi.BanKinhSatThuong(K), CapDo.DuBacDeMo(K), PlayerController.KyGaySatThuong(K), mt.Length));
        // 26/09/2026: mo khoa CAN LOC XOAY CAP 5 (truoc do khong dieu kien)
        CapDo.BatDauTranMoi();
        CapDo.ThemDiemChoPhepThu(30);
        bool chanDauTran = !CapDo.MoKhoaDuoc(K);
        CapDo.MoCaDuongChoPhepThu(3);
        for (int i = 0; i < 3; i++) CapDo.NangCap(3);                // Loc xoay cap 4
        bool chanCap4 = CapDo.CapCuaKyNang(3) == 4 && !CapDo.MoKhoaDuoc(K);
        string nhac = SachPhep.NhacDieuKien(K);
        CapDo.NangCap(3);                                            // cap 5
        bool moCap5 = CapDo.CapCuaKyNang(3) == 5 && CapDo.MoKhoa(K);
        Ghi(string.Format("A. mo khoa: dau tran (co diem) bi chan {0}; Loc xoay cap 4 bi chan {1} (nhac \"{2}\"); Loc xoay cap 5 mo duoc {3}",
            chanDauTran, chanCap4, nhac, moCap5));
        Kiem(chanDauTran && chanCap4 && moCap5 && nhac != null && nhac.Contains("LỐC XOÁY") && nhac.Contains("cấp 5"),
             "May giong khong doi Loc xoay cap 5 moi mo khoa");
        CapDo.BatDauTranMoi();
        Kiem(K == 21 && CapDo.SoKyNang == 22 && SachPhep.SoKyNangCoTen() == CapDo.SoKyNang, "so hieu / so ky nang sai");
        Kiem(SachPhep.Ten(K) == "MÂY GIÔNG" && trongPhong, "ten / nhom PHONG sai");
        Kiem(mt.Contains("125") && mt.Contains("45%") && mt.Contains("0,85") && mt.Contains("3 giây") && mt.Contains("20 tia") && mt.Contains("5 giây") && mt.Contains("ƯỚT") && mt.Contains("50%"), "mo ta Sach phep thieu con so / thieu mua uot");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null, "thieu icon May giong");
        Kiem(Mathf.Approximately(toi.mayGiongCost, 50f) && Mathf.Approximately(toi.mayGiongCooldown, 5.5f) && Mathf.Approximately(toi.mayGiongCastTime, 0.5f)
             && Mathf.Approximately(nl, 50f) && Mathf.Approximately(hc, 5.5f), "nang luong / hoi chieu 5,5 / niem sai");
        Kiem(Mathf.Approximately(toi.TamNgam(K), toi.TamNgam(2)) && Mathf.Abs(toi.TamNgam(K) - 12f) < 0.01f, "tam khong bang Sam set (12 m)");
        Kiem(Mathf.Approximately(toi.BanKinhSatThuong(K), 6f), "vung ngam khong phai 6 m");
        Kiem(PlayerController.KyGaySatThuong(K), "May giong khong tinh la ky nang gay sat thuong");

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
        float hanC = Time.time + MayGiong.GiayHetMay + 0.6f;
        // Pha BAY: vi tri luc bat dau / luc cuoi, bam dat, moi tia khi bay roi trong vung quanh cho may DANG O
        Vector3 viTriDauBay = Vector3.zero, viTriCuoiBay = Vector3.zero; bool coDauBay = false;
        int tiaDaThay = MayGiong.SoTiaDaDanh, tiaKhiBay = 0, tiaBayNgoaiVung = 0; float lechDatBay = 0f;
        while (Time.time < hanC)
        {
            if (may != null)
            {
                float tuoiMay = may.DongHo;   // dong ho CUA MAY (lucTung con tinh ca 0,5 s niem)
                if (!coDauBay && tuoiMay >= MayGiong.GiayTuMay + MayGiong.ThoiGian) { coDauBay = true; viTriDauBay = may.transform.position; }
                if (coDauBay)
                {
                    viTriCuoiBay = may.transform.position;
                    lechDatBay = Mathf.Max(lechDatBay, Mathf.Abs(may.transform.position.y - GioLoc.MatDatY(may.transform.position, may.transform.position.y)));
                }
                if (MayGiong.SoTiaDaDanh != tiaDaThay)
                {
                    tiaDaThay = MayGiong.SoTiaDaDanh;
                    if (coDauBay)
                    {
                        tiaKhiBay++;
                        Vector3 v = MayGiong.DiemTiaCuoi - may.transform.position; v.y = 0f;
                        if (v.magnitude > MayGiong.BanKinh + 0.05f) tiaBayNgoaiVung++;
                    }
                }
            }
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
        // Cum bia dung giua mua nen deu BI UOT truoc tia dau (mua quet truoc tia trong cung khung) -> moi tia 125 x 1,5 = 187,5
        int boiSo125 = 0; foreach (var m in buocMat) { float k = m / (125f * BiUot.HeSoSetDien); if (Mathf.Abs(k - Mathf.Round(k)) < 0.01f && Mathf.Round(k) >= 1f) boiSo125++; }
        float khoangTB = lucTia.Count > 1 ? (lucTia[lucTia.Count - 1] - lucTia[0]) / (lucTia.Count - 1) : 0f;
        Ghi(string.Format("B. ton {0} nang luong, hoi chieu ngay sau {1:F2} s; bam lai ngay khi dang hoi chieu -> suot 10 giay chi {2} dam may; may hien sau {3:F2} s, lech cho ngam {4:F2} m",
            manaTon, hoiNgaySau, mayDaThay.Count, lucThay, lechCho));
        Ghi(string.Format("C. {0} tia (bo dem {1}); tia dau {2:F2} s, tia cuoi {3:F2} s, cach nhau TB {4:F3} s; anh Blender {5}, cung mot kieu {6}",
            lucTia.Count, soTia, lucTia.Count > 0 ? lucTia[0] : -1f, lucTia.Count > 0 ? lucTia[lucTia.Count - 1] : -1f, khoangTB, anhBlender, cungKieu));
        Ghi(string.Format("D. {0} lan bia mat mau, boi so 125: {1}; tia nham ke dich {2}/{3} ({4:P0}, mong ~65%)",
            buocMat.Count, boiSo125, soNham, soTia, soTia > 0 ? soNham / (float)soTia : 0f));
        Ghi("D. (cum bia dung giua mua -> deu BI UOT: moi lan mat mau phai la boi so 187,5 = 125 x 1,5)");
        Kiem(Mathf.Abs(manaTon - 50f) < 0.01f && Mathf.Abs(hoiNgaySau - 5.5f) < 0.05f, "khong ton 50 / hoi chieu 5,5");

        // HUONG BAY giong nhau moi may: diem ngam that va diem da qua goi tin (nen 0,01 m) ra cung huong; 40 cho ngam trai deu 4 goc
        {
            int khop = 0; var goc4 = new int[4];
            for (int i = 0; i < 40; i++)
            {
                Vector3 p = goc + new Vector3(Random.Range(-40f, 40f), 0f, Random.Range(-40f, 40f));
                Vector3 quaMang = new Vector3(GoiTin.NenToaDo(p.x) * 0.01f, p.y, GoiTin.NenToaDo(p.z) * 0.01f);
                Vector3 h1 = MayGiong.HuongBay(p), h2 = MayGiong.HuongBay(quaMang);
                if (Vector3.Angle(h1, h2) < 0.01f) khop++;
                goc4[(int)(Mathf.Repeat(Mathf.Atan2(h1.z, h1.x), Mathf.PI * 2f) / (Mathf.PI * 0.5f)) % 4]++;
            }
            Ghi(string.Format("B. huong bay: 40 cho ngam - {0}/40 khop giua diem that va diem qua goi tin; chia 4 goc phan tu {1}/{2}/{3}/{4}", khop, goc4[0], goc4[1], goc4[2], goc4[3]));
            Kiem(khop == 40, "huong bay khac nhau giua may tung va may nhan");
            Kiem(goc4[0] >= 4 && goc4[1] >= 4 && goc4[2] >= 4 && goc4[3] >= 4, "huong bay khong ngau nhien (don vao mot phia)");
        }
        Kiem(thayMay && lechCho >= 0f && lechCho < 0.3f, "may khong hien dung cho ngam");
        Kiem(mayDaThay.Count == 1, "bam lai khi dang hoi chieu van ra may moi");
        Kiem(lucTia.Count == MayGiong.SoTiaTong && soTia == MayGiong.SoTiaTong && MayGiong.SoTiaTong == 36, "khong dung 36 tia (20 dung yen + 16 khi bay)");
        Kiem(lucTia.Count == 36 && lucTia[0] > 0.2f && lucTia[0] < 0.6f && lucTia[19] < 5.6f && lucTia[35] < 9.6f && Mathf.Abs(khoangTB - 0.25f) < 0.03f, "36 tia khong trai deu 4 tia/giay suot 9 giay");
        // Pha bay: quang duong ~6 m (1,5 m/s x 4 s), dung huong HuongBay(diem ngam), bam mat dat, tia roi quanh cho may dang o
        Vector3 diBay = viTriCuoiBay - viTriDauBay; diBay.y = 0f;
        Vector3 huongMong = MayGiong.HuongBay(aim);
        Ghi(string.Format("C. BAY: {0} tia khi bay ({1} tia roi ngoai vung 6 m quanh cho may dang o); di {2:F2} m (mong ~6), lech huong HuongBay(diem ngam) {3:F1} do; lech mat dat Ground toi da {4:F3} m",
            tiaKhiBay, tiaBayNgoaiVung, diBay.magnitude, diBay.sqrMagnitude > 0.01f ? Vector3.Angle(diBay, huongMong) : -1f, lechDatBay));
        Kiem(coDauBay && tiaKhiBay == MayGiong.SoTiaKhiBay && tiaBayNgoaiVung == 0, "khi bay khong du 16 tia / tia roi ngoai vung may dang o");
        Kiem(diBay.magnitude > 5.6f && diBay.magnitude < 6.2f && Vector3.Angle(diBay, huongMong) < 3f, "may khong bay ~6 m theo HuongBay");
        Kiem(lechDatBay < 0.05f, "may bay khong bam mat dat (lop Ground)");
        Kiem(anhBlender == lucTia.Count && cungKieu == lucTia.Count, "tia khong cung mot kieu anh Blender");
        Kiem(buocMat.Count >= 10 && boiSo125 == buocMat.Count, "moi tia len bia UOT khong gay dung 187,5 (125 x 1,5)");
        Kiem(soTia > 0 && soNham / (float)soTia > 0.4f && soNham / (float)soTia < 0.9f, "ti le nham ke dich khong quanh 65%");

        // DOI CHUNG vung trong (khong ai): 0 tia nham ke dich, van du 20 tia
        foreach (var b in bia) Object.Destroy(b.gameObject);
        bia.Clear();
        yield return new WaitForSeconds(0.3f);
        tia0 = MayGiong.SoTiaDaDanh; nham0 = MayGiong.SoTiaNhamKeDich;
        var mayTrong = MayGiong.Spawn(goc + huong * 12f, maskEnemy, mauToi);
        yield return new WaitForSeconds(MayGiong.GiayHetMay + 0.4f);
        Ghi(string.Format("D2. DOI CHUNG vung trong: {0} tia, nham ke dich {1}", MayGiong.SoTiaDaDanh - tia0, MayGiong.SoTiaNhamKeDich - nham0));
        Kiem(MayGiong.SoTiaDaDanh - tia0 == MayGiong.SoTiaTong && MayGiong.SoTiaNhamKeDich - nham0 == 0, "DOI CHUNG: vung trong van nham ke dich / thieu tia");

        // ================= E. HAT NGA =================
        Ghi("");
        {
            var cum = new List<Damageable>();
            for (int i = 0; i < 6; i++) { Vector2 l = Random.insideUnitCircle * 0.8f; cum.Add(TaoBia("TAM_BiaNga" + i, goc + huong * 12f + new Vector3(l.x, 0f, l.y))); }
            trung0 = MayGiong.SoLanTrung; nga0 = MayGiong.SoLanNga;
            float giayNgaDo = -1f;
            for (int lan = 0; lan < 3; lan++)
            {
                var mE = MayGiong.Spawn(goc + huong * 12f, maskEnemy, mauToi);
                float hanE = Time.time + 5.7f;
                while (Time.time < hanE)
                {
                    if (giayNgaDo < 0f) foreach (var b in cum) { var n = b.GetComponent<BiDanhNga>(); if (n != null) { giayNgaDo = n.thoiGian; break; } }
                    yield return null;
                }
                if (mE != null) Object.Destroy(mE.gameObject);   // chi do pha dung yen tren cum bia (pha bay roi cum)
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
                    foreach (var kv in goc0) { int soCD = 0; foreach (var mm in kv.Key.sharedMaterials) if (mm != null && mm.name.StartsWith("P_ChayDen")) soCD++; if (soCD == 1) phuDu++; }
                float dam = cd != null ? cd.DoDam : -1f;
                // Mua lam quai UOT truoc tia dau (thu tu that trong tran) - lop chay den van phai phu (dem o tren). Do xong
                // thi go uot: phan duoi do "tra lai dung vat lieu" chi cua chay den (uot song 5 s, lau hon chay den)
                var buF = quai.GetComponent<BiUot>(); if (buF != null) Object.Destroy(buF);
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
                    bool laLocal = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;   // may bay: lop may mo phong Local
                    for (int i = 0; i < n; i++)
                    {
                        Vector3 wp = laLocal ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
                        lechY = Mathf.Max(lechY, Mathf.Abs(wp.y - ps.transform.position.y));
                        xaNgang = Mathf.Max(xaNgang, new Vector2(wp.x - hinh.transform.position.x, wp.z - hinh.transform.position.z).magnitude);
                    }
                }
                den = hinh.GetComponentsInChildren<Light>().Length;
            }
            Ghi(string.Format("H. may: dam sang {0}, dam xam {1}; lech doc toi da {2:F2} m (nam NGANG), xa tam toi da {3:F2} m (vung 6); den {4}; cao day may {5} m",
                sang, xam, lechY, xaNgang, den, MayGiong.CaoMay));
            Kiem(sang == 26 && xam == 16 && den >= 1, "thieu dam may / den");
            Kiem(lechY < 0.3f && xaNgang > 3f && xaNgang < 7.2f, "may khong nam ngang trai tren vung 6 m (x1,15)");

            // 26/09/2026 nguoi dung BO cot khoi + quang may sat dat: khong con lop nao trong 4 lop cu
            int conLopCu = 0;
            if (hinh != null) foreach (var ten in new[] { "CotMay", "KhoiCot", "SuongDat", "MayThap" }) if (hinh.transform.Find(ten) != null) conLopCu++;
            Ghi("H. cot khoi + quang may sat dat (da bo): con " + conLopCu + " lop");
            Kiem(hinh != null && conLopCu == 0, "van con cot khoi / quang may sat dat");

            // May TO x1,15 (chi hinh): co dam + ban kinh rai so voi ban truoc (chep tay: sang 4,2-7,5 / r 5,1; xam 5-8,5 / r 5,7)
            if (hinh != null)
            {
                var pSang = hinh.transform.Find("MaySang"); var pXam = hinh.transform.Find("MayXam");
                if (pSang != null && pXam != null)
                {
                    var a = pSang.GetComponent<ParticleSystem>(); var b = pXam.GetComponent<ParticleSystem>();
                    float k1 = a.main.startSize.constantMax / 7.5f, k2 = a.main.startSize.constantMin / 4.2f, k3 = a.shape.radius / 5.1f;
                    float k4 = b.main.startSize.constantMax / 8.5f, k5 = b.main.startSize.constantMin / 5f, k6 = b.shape.radius / 5.7f;
                    bool local = a.main.simulationSpace == ParticleSystemSimulationSpace.Local && b.main.simulationSpace == ParticleSystemSimulationSpace.Local;
                    Ghi(string.Format("H. may to: sang co x{0:F3}/x{1:F3} rai x{2:F3} | xam co x{3:F3}/x{4:F3} rai x{5:F3}; mo phong Local (bay theo) {6}; vung mua van {7} m",
                        k1, k2, k3, k4, k5, k6, local, MayGiong.BanKinh));
                    Kiem(Mathf.Abs(k1 - 1.15f) < 0.005f && Mathf.Abs(k2 - 1.15f) < 0.005f && Mathf.Abs(k3 - 1.15f) < 0.005f
                         && Mathf.Abs(k4 - 1.15f) < 0.005f && Mathf.Abs(k5 - 1.15f) < 0.005f && Mathf.Abs(k6 - 1.15f) < 0.005f, "dam may khong to x1,15");
                    Kiem(local, "lop may khong mo phong Local (may bay thi dam may o lai)");
                    Kiem(Mathf.Approximately(MayGiong.BanKinh, 6f), "vung mua / set bi doi (nguoi dung chon chi hinh)");
                }
                else Kiem(false, "khong tim thay MaySang / MayXam");
            }

            if (hinh != null)
            {
                // May giong DEN: do sang (luminance) mau tung lop so voi mau ban truoc (chep tay) ~0,26; day may toi hon dinh
                string[] tenLop = { "MaySang", "MayXam" };
                Color[] mauCu = { new Color(0.95f, 0.97f, 1f), new Color(0.50f, 0.53f, 0.60f) };
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
                Kiem(boLoe != null && boLoe.SoRenderer == 2, "thieu bo set roi sang tren 2 lop may");
                Kiem(kMax > 1.5f, "set khong roi sang dam may");
                Kiem(soKhung > 0 && khungToi / (float)soKhung > 0.25f, "may sang gan het thoi gian - khong con toi");
                Kiem(soTiaB >= 5 && soMang == soTiaB, "moi tia khong co mot mang sang");
            }
            if (m2 != null) Object.Destroy(m2.gameObject);
        }

        // ================= J. MUA - BI UOT - +50% SET - CHAY XEM =================
        {
            Ghi("");
            vatLieuGocQ.Clear();
            Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;
            Vector3 tamJ = goc + huong * 11f; tamJ.y = VfxFactory.GroundY(tamJ);
            // May se BAY 6 m theo HuongBay(tamJ): bia A dat theo huong bay (con trong mua toi het), bia 'kho' va bia 'di ra' dat
            // PHIA NGUOC huong bay - may khong mang mua toi chung
            Vector3 hb = MayGiong.HuongBay(tamJ), hbNgang = Vector3.Cross(Vector3.up, hb).normalized;
            var A = TaoBia("TAM_UotA", tamJ + hb * 2f);
            var B = TaoBia("TAM_UotB", tamJ + hbNgang * 4f);
            var C = TaoBia("TAM_UotC", tamJ - hbNgang * 5.5f);
            var ngoai = TaoBia("TAM_UotNgoai", tamJ - hb * 7.5f);
            var diRa = TaoBia("TAM_UotDiRa", tamJ - hb * 3f);
            var nguoiTung = TaoBia("TAM_UotNguoiTung", tamJ + hbNgang * 1f);     // dung giua vung, la boQua
            // Quai co SkinnedMesh de xem hinh uot
            GameObject pfJ = null;
            if (GameAssets.I != null) foreach (var pp in GameAssets.I.enemyPrefabs) if (pp != null && pp.GetComponentInChildren<SkinnedMeshRenderer>(true) != null) { pfJ = pp; break; }
            Vector3 choQ = tamJ + hb * 1.5f + hbNgang * 2f; choQ.y = VfxFactory.GroundY(choQ);
            var quaiJ = pfJ != null ? Object.Instantiate(pfJ, choQ, Quaternion.identity) : null;
            Damageable dQ = null;
            if (quaiJ != null)
            {
                quaiJ.name = "TAM_QuaiUot";
                foreach (var ai in quaiJ.GetComponentsInChildren<EnemyAI>()) ai.enabled = false;
                dQ = quaiJ.GetComponent<Damageable>(); dQ.maxHealth = 1e6f; dQ.health = 1e6f;
                foreach (var rr in quaiJ.GetComponentsInChildren<Renderer>()) if (rr is MeshRenderer || rr is SkinnedMeshRenderer) vatLieuGocQ[rr] = rr.sharedMaterials;
            }
            int chu0 = BiUot.SoLanBaoChu, uot0 = MayGiong.SoLanTrungUot;
            bool coChongLop = false;   // luc nao do quai vua uot vua chay den (hai lop phu cung luc)
            var chayXem = new HashSet<GameObject>();
            System.Action demChayXem = () => { foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude)) if (t.name == "SetChayDen") chayXem.Add(t.gameObject); };
            demChayXem();
            var chayXemCu = new HashSet<GameObject>(chayXem); chayXem.Clear();
            int tiaJ0 = MayGiong.SoTiaDaDanh;
            var mayJ = MayGiong.Spawn(tamJ, maskEnemy, nguoiTung);
            float t0 = Time.time;
            if (rig != null) rig.enabled = false;
            if (cam != null) { cam.transform.position = tamJ - huong * 20f + Vector3.up * 7f; cam.transform.LookAt(tamJ + Vector3.up * 3f); }
            while (Time.time - t0 < 0.9f) { demChayXem(); yield return null; }
            bool uA = BiUot.DangUot(A), uB = BiUot.DangUot(B), uC = BiUot.DangUot(C), uN = BiUot.DangUot(ngoai), uR = BiUot.DangUot(diRa), uT = BiUot.DangUot(nguoiTung), uQ = BiUot.DangUot(dQ);
            int soChu = BiUot.SoLanBaoChu - chu0;
            Ghi(string.Format("J. sau 0,9 s mua: uot trong vung A(2 m) {0} B(4 m) {1} C(5,5 m) {2} dung-roi-di-ra(3 m) {3} quai(3,5 m) {4} | ngoai vung(7,5 m) {5} | nguoi tung {6} | chu \"BỊ ƯỚT\" bay len {7} lan",
                uA, uB, uC, uR, uQ, uN, uT, soChu));
            Kiem(uA && uB && uC && uR && (dQ == null || uQ), "doi thu dung trong mua khong bi uot 100%");
            Kiem(!uN, "doi thu ngoai vung 6 m cung bi uot");
            Kiem(!uT, "nguoi tung bi uot");
            Kiem(soChu == (dQ != null ? 5 : 4), "chu BI UOT khong bay len dung mot lan moi ke");

            // Hinh uot tren quai co xuong
            if (dQ != null)
            {
                var bu = dQ.GetComponent<BiUot>();
                int soSkin = quaiJ.GetComponentsInChildren<SkinnedMeshRenderer>().Length;
                Ghi(string.Format("J. hinh uot tren quai ({0} SkinnedMesh): phu lop bong uot {1} renderer, do dam {2:F2}, nho nuoc {3}",
                    soSkin, bu != null ? bu.SoRendererDaPhu : 0, bu != null ? bu.DoDam : 0f, bu != null && bu.CoNhoNuoc));
                Kiem(bu != null && bu.SoRendererDaPhu >= 1 && bu.DoDam > 0.9f && bu.CoNhoNuoc, "quai uot khong co lop bong uot / nho nuoc");
            }

            // 1,5 s: bia diRa ra khoi vung
            while (Time.time - t0 < 1.5f) { demChayXem(); yield return null; }
            diRa.transform.position = tamJ - hb * 9.5f + Vector3.up * (VfxFactory.GroundY(tamJ - hb * 9.5f) - tamJ.y); Physics.SyncTransforms();
            float lucDiRa = Time.time;
            // Theo doi luc het uot SONG SONG (cac phep thu sat thuong phia sau chiem vai giay)
            hetUotA = -1f; hetUotR = -1f;
            toi.StartCoroutine(TheoDoiHetUot(diRa, lucDiRa, true));

            // 2,2 s: do mua
            while (Time.time - t0 < 2.2f)
            {
                demChayXem();
                if (!coChongLop && dQ != null && dQ.GetComponent<BiUot>() != null && dQ.GetComponent<ChayDenToanThan>() != null) coChongLop = HaiLopPhu(quaiJ);
                yield return null;
            }
            int soVetMua = 0, soVong = 0; float lechDatVong = 0f, xaMua = 0f; int soVongDo = 0;
            var hinhJ = mayJ != null ? GameObject.Find("MayGiongHinh") : null;
            foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                if (tr.name == "MayGiongHinh" && new Vector2(tr.position.x - tamJ.x, tr.position.z - tamJ.z).magnitude < 0.3f) hinhJ = tr.gameObject;
            if (hinhJ != null)
                foreach (var ps in hinhJ.GetComponentsInChildren<ParticleSystem>())
                {
                    var hat = new ParticleSystem.Particle[ps.main.maxParticles]; int n = ps.GetParticles(hat);
                    if (ps.name == "MuaRoi")
                    {
                        soVetMua = n;
                        for (int i = 0; i < n; i++) xaMua = Mathf.Max(xaMua, new Vector2(hat[i].position.x - tamJ.x, hat[i].position.z - tamJ.z).magnitude);
                    }
                    if (ps.name == "VongNuoc")
                    {
                        soVong = n;
                        // Doc lap voi he va cham: ban tia CHI lop Ground ngay tai cho vong nuoc
                        for (int i = 0; i < n; i++)
                        {
                            Vector3 q = hat[i].position;
                            if (Physics.Raycast(q + Vector3.up * 3f, Vector3.down, out RaycastHit hDat, 8f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                            { lechDatVong += Mathf.Abs(q.y - hDat.point.y); soVongDo++; }
                        }
                    }
                }
            if (soVongDo > 0) lechDatVong /= soVongDo;
            Ghi(string.Format("J. mua: {0} vet mua dang roi (trai xa tam toi {1:F2} m, vung 6), {2} vong nuoc cham dat - lech mat dat that TB {3:F2} m ({4} vong do duoc)",
                soVetMua, xaMua, soVong, lechDatVong, soVongDo));
            Kiem(soVetMua >= 30 && xaMua > 4.5f && xaMua < 6.3f, "mua khong roi khap vung 6 m");
            Kiem(soVong >= 5 && soVongDo >= 5 && lechDatVong < 0.25f, "vong nuoc khong nam dung mat dat that (mua khong cham dat)");
            yield return Chup("maygiong_5_mua");

            // Doi may tan (5,35 s) roi moi thu sat thuong (tia May giong khong danh chen vao)
            while (mayJ != null && Time.time - t0 < MayGiong.GiayHetMay + 2f)
            {
                demChayXem();
                if (!coChongLop && dQ != null && dQ.GetComponent<BiUot>() != null && dQ.GetComponent<ChayDenToanThan>() != null) coChongLop = HaiLopPhu(quaiJ);
                yield return null;
            }
            float lucHetMua = Time.time;
            toi.StartCoroutine(TheoDoiHetUot(A, lucHetMua, false));
            int soTiaJ = MayGiong.SoTiaDaDanh - tiaJ0;
            demChayXem();
            int soChayXem = 0; foreach (var g in chayXem) if (!chayXemCu.Contains(g)) soChayXem++;
            int uotTrung = MayGiong.SoLanTrungUot - uot0;
            Ghi(string.Format("J. {0} tia May giong -> {1} vet chay xem SetChayDen; {2} lan tia trung ke UOT", soTiaJ, soChayXem, uotTrung));
            Kiem(soTiaJ == MayGiong.SoTiaTong && soChayXem == MayGiong.SoTiaTong, "moi tia May giong khong de dung mot vet chay xem");
            Kiem(uotTrung >= 5, "tia May giong khong trung ke uot lan nao");

            // DOI CHUNG chay xem: tia Sam set THAT (LightningStrike) cung de dung MOT SetChayDen
            {
                chayXem.Clear(); demChayXem(); var cu2 = new HashSet<GameObject>(chayXem); chayXem.Clear();
                Vector3 choSS = tamJ + huong * 14f; choSS.y = VfxFactory.GroundY(choSS);
                var ss = LightningStrike.Spawn(choSS, 18f, 0f); ss.damage = 0f;
                for (float h = Time.time + 0.6f; Time.time < h; ) { demChayXem(); yield return null; }
                int soSS = 0; foreach (var g in chayXem) if (!cu2.Contains(g)) soSS++;
                Ghi("J. DOI CHUNG: mot tia Sam set that -> " + soSS + " vet chay xem SetChayDen");
                Kiem(soSS == 1, "doi chung Sam set khong ra 1 vet chay xem (phep dem sai)");
            }

            // +50%: moi don mot bia UOT (A) va mot bia KHO (ngoai), cung hinh hoc
            // Giut set NGUOI CHOI: 1 tia, khong lan, tam 3 m, nham thang vao bia
            float hA, hN;
            // -- Giut set
            hA = A.health; hN = ngoai.health;
            foreach (var d in new[] { A, ngoai })
            {
                Vector3 o = d.transform.position - huong * 2f + Vector3.up * 1.4f;
                var gs = GiatSet.Phong(o, huong, maskEnemy); gs.soTiaDau = 1; gs.maxChains = 0; gs.xacSuatChoang = 0f; gs.range = 3f; gs.damage = GiatSet.SatThuongNguoiChoi;
            }
            yield return new WaitForSeconds(0.5f);
            float gsUot = hA - A.health, gsKho = hN - ngoai.health;
            // -- Giut set cua QUAI (doi chung: khong cong)
            hA = A.health; hN = ngoai.health;
            foreach (var d in new[] { A, ngoai })
            {
                Vector3 o = d.transform.position - huong * 2f + Vector3.up * 1.4f;
                GiatSet.PhongCuaQuai(o, huong, maskEnemy, 40f, 3f, Color.white, Color.green);
            }
            yield return new WaitForSeconds(0.5f);
            float gqUot = hA - A.health, gqKho = hN - ngoai.health;
            // -- Sam set (AreaShock, dung tam = bia -> falloff 1)
            hA = A.health; hN = ngoai.health;
            CombatUtil.AreaShock(A.transform.position, 0.5f, 26f, maskEnemy, 0f, 0f);
            CombatUtil.AreaShock(ngoai.transform.position, 0.5f, 26f, maskEnemy, 0f, 0f);
            float ssUot = hA - A.health, ssKho = hN - ngoai.health;
            // -- sat thuong vung thuong (doi chung: khong cong)
            hA = A.health; hN = ngoai.health;
            CombatUtil.AreaDamage(A.transform.position, 0.5f, 50f, maskEnemy, DamageType.Fire, 0f);
            CombatUtil.AreaDamage(ngoai.transform.position, 0.5f, 50f, maskEnemy, DamageType.Fire, 0f);
            float adUot = hA - A.health, adKho = hN - ngoai.health;
            Ghi(string.Format("J. +50%: Giut set uot {0:F2} / kho {1:F2} (x{2:F3}) | Sam set uot {3:F2} / kho {4:F2} (x{5:F3}) | DOI CHUNG Giut set cua quai uot {6:F2} / kho {7:F2} (x{8:F3}), sat thuong vung thuong uot {9:F2} / kho {10:F2} (x{11:F3})",
                gsUot, gsKho, gsKho > 0 ? gsUot / gsKho : 0f, ssUot, ssKho, ssKho > 0 ? ssUot / ssKho : 0f, gqUot, gqKho, gqKho > 0 ? gqUot / gqKho : 0f, adUot, adKho, adKho > 0 ? adUot / adKho : 0f));
            Kiem(gsKho > 0f && Mathf.Abs(gsUot / gsKho - 1.5f) < 0.01f, "Giut set khong +50% len ke uot");
            Kiem(ssKho > 0f && Mathf.Abs(ssUot / ssKho - 1.5f) < 0.01f, "Sam set khong +50% len ke uot");
            Kiem(gqKho > 0f && Mathf.Abs(gqUot / gqKho - 1f) < 0.01f, "Giut set cua QUAI cung duoc +50% (sai)");
            Kiem(adKho > 0f && Mathf.Abs(adUot / adKho - 1f) < 0.01f, "sat thuong khong phai he SET cung duoc +50% (sai)");

            // -- Qua cau dien: MOI BIA MOT QUA RIENG dat canh bia (bia kho nam nguoc huong bay, cach bia uot 9,5 m > tam 9 m cua
            //    qua cau) - doc don DAU TIEN len bia ay. Moi tia 155,52 (Giut set cap 5), bia uot phai x1,5
            {
                var lanDau = new Dictionary<Damageable, float>();
                foreach (var d in new[] { A, ngoai })
                {
                    float truoc = d.health;
                    var qc = QuaCauDien.Spawn(d.transform.position, maskEnemy, nguoiTung);
                    for (float h = Time.time + 2.5f; Time.time < h && !lanDau.ContainsKey(d); )
                    {
                        if (d.health < truoc - 0.01f) lanDau[d] = truoc - d.health;
                        yield return null;
                    }
                    if (qc != null) Object.Destroy(qc.gameObject);
                    yield return new WaitForSeconds(0.2f);
                }
                float qUot = lanDau.ContainsKey(A) ? lanDau[A] : 0f, qKho = lanDau.ContainsKey(ngoai) ? lanDau[ngoai] : 0f;
                Ghi(string.Format("J. +50%: Qua cau dien don dau len uot {0:F2} / kho {1:F2} (x{2:F3}; tia goc {3:F2})", qUot, qKho, qKho > 0 ? qUot / qKho : 0f, QuaCauDien.SatThuongTia));
                Kiem(qKho > 0f && Mathf.Abs(qUot / qKho - 1.5f) < 0.01f, "Qua cau dien khong +50% len ke uot");
            }

            // Het mua / ra khoi vung -> con uot them ~5 giay
            for (float h = Time.time + 9f; Time.time < h && (hetUotA < 0f || hetUotR < 0f); ) yield return null;
            float hetA = hetUotA, hetR = hetUotR;
            Ghi(string.Format("J. het uot: sau khi may tan {0:F2} s (bia A), sau khi ra khoi vung {1:F2} s (mong ~5, lan lam moi cuoi tre toi 0,2 s)", hetA, hetR));
            Kiem(hetA > 4.4f && hetA < 5.3f, "het mua ma khong uot them ~5 giay");
            // (A dat theo huong bay: con trong mua toi luc may tan)
            Kiem(hetR > 4.7f && hetR < 5.3f, "ra khoi vung ma khong uot them ~5 giay");

            // Quai vua uot vua chay den: het ca hai thi moi renderer tro ve DUNG mang vat lieu goc (moi hieu ung go dung lop minh)
            if (dQ != null)
            {
                for (float h = Time.time + 6f; Time.time < h && (dQ.GetComponent<BiUot>() != null || dQ.GetComponent<ChayDenToanThan>() != null); ) yield return null;
                yield return null;
                int dung = 0;
                foreach (var kv in vatLieuGocQ)
                {
                    var ht = kv.Key.sharedMaterials; bool giong = ht.Length == kv.Value.Length;
                    for (int i = 0; giong && i < ht.Length; i++) if (ht[i] != null && ht[i].name.StartsWith("P_")) giong = false;
                    if (giong) dung++;
                }
                Ghi(string.Format("J. quai co luc mang CA HAI lop phu (bong uot + than den) tren moi renderer {0}; het ca hai -> {1}/{2} renderer ve dung so lop, khong con lop phu", coChongLop, dung, vatLieuGocQ.Count));
                Kiem(coChongLop, "ke dang UOT bi set danh ma khong co lop CHAY DEN (hoac nguoc lai)");
                Kiem(dung == vatLieuGocQ.Count && vatLieuGocQ.Count > 0, "go uot / chay den de lai lop phu hoac lam mat lop");
            }

            foreach (var d in new[] { A, B, C, ngoai, diRa, nguoiTung }) if (d != null) Object.Destroy(d.gameObject);
            if (quaiJ != null) Object.Destroy(quaiJ);
            if (rig != null) rig.enabled = true;
            yield return new WaitForSeconds(0.5f);
        }

        // ================= K. MUA DAP TAT LO LUA (nguoi dung 26/09/2026) =================
        {
            Ghi("");
            LoLuaDa loGan = null, loXa = null; float xaNhat = 0f;
            var cacLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsInactive.Exclude);
            foreach (var lo in cacLo) if (lo.DangChay && (loGan == null || Vector3.Distance(lo.transform.position, goc) < Vector3.Distance(loGan.transform.position, goc))) loGan = lo;
            if (loGan != null)
                foreach (var lo in cacLo) { float d = Vector3.Distance(lo.transform.position, loGan.transform.position); if (lo.DangChay && d > 20f && (loXa == null || d < xaNhat)) { loXa = lo; xaNhat = d; } }
            if (loGan == null) { Ghi("[LOI] K: khong co lo lua nao dang chay de thu"); loi++; }
            else
            {
                // May dat LECH 3 m khoi lo, NGUOC huong bay (lo nam trong vung 6 m ngay tu dau)
                Vector3 choK = loGan.transform.position - MayGiong.HuongBay(loGan.transform.position) * 3f;
                // HuongBay tinh tu cho ngam - lap lai 1 lan de huong khop cho ngam that
                choK = loGan.transform.position - MayGiong.HuongBay(choK) * 3f;
                int dap0 = MayGiong.SoLoDapTat;
                var mK = MayGiong.Spawn(choK, maskEnemy, mauToi);
                float tK = Time.time, lucTat = -1f;
                for (float h = Time.time + 1.5f; Time.time < h && lucTat < 0f; ) { if (!loGan.DangChay) lucTat = Time.time - tK; yield return null; }
                float henLai = loGan.HenChayLai - Time.time;
                bool xaVanChay = loXa == null || loXa.DangChay;
                Ghi(string.Format("K. lo cach may {0:F1} m: tat sau {1:F2} s (mua bat dau o {2:F2} s); hen chay lai sau {3:F1} s (mong ~30); dem dap {4}; lo DOI CHUNG cach {5:F0} m van chay {6}",
                    Vector3.Distance(new Vector3(loGan.transform.position.x, 0f, loGan.transform.position.z), new Vector3(choK.x, 0f, choK.z)), lucTat, MayGiong.GiayTuMay, henLai, MayGiong.SoLoDapTat - dap0, xaNhat, xaVanChay));
                Kiem(lucTat > 0f && lucTat < MayGiong.GiayTuMay + 0.4f, "mua khong dap tat lo lua");
                Kiem(henLai > 28f && henLai < 30.5f, "lo khong hen chay lai sau 30 giay");
                Kiem(xaVanChay, "lo ngoai vung mua cung bi tat (DOI CHUNG)");
                if (mK != null) Object.Destroy(mK.gameObject);
                loGan.Chay();   // tra lo ve nhu cu cho cac muc sau
            }
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
