using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 74): KY NANG "QUA CAU DIEN" (so hieu 13, them 18/09/2026).
///
/// Nguoi dung (kem anh Diablo III): mot QUA CAU DIEN hien ra NGAY GAN DOI THU, ban cac tia dien ra chung quanh;
/// hoi chieu 5 giay; tam danh BANG THIEN THACH; moi luot 5 tia toi moi quai / nguoi choi khac o gan, toi da 10 luot,
/// cach nhau 0,4 giay; moi tia sat thuong = GIUT SET cap 5, va 30% gay choang.
/// Hoi them, nguoi dung chot: 55 nang luong, ban trong 9 m, choang 1,5 giay, moi ke MOT tia (thua thi bo).
///
///   A. Thong so + tai nguyen Blender (FBX hai luoi, ba anh, icon); ten co dau; HUD 14 icon.
///   B. Tung that CastAt(13): khoa -> tu choi; mo khoa -> tru dung 55 nang luong, hoi chieu 5 giay, co qua cau tren canh.
///   C. Cho dat: co ke dich gan cho ngam -> qua cau dung CANH KE DICH (khong phai cho ngam);
///      DOI CHUNG khong co ai -> dung dung cho ngam.
///   D. Nhip ban: dung 10 luot, luot cach nhau 0,4 giay.
///   E. Moi luot 5 tia va MOI KE MOT TIA: 7 bia trong 9 m -> chi 5 bia GAN NHAT an don, 2 bia xa hon khong;
///      moi bia an dung 10 lan trung (10 luot).
///   F. Sat thuong moi tia = Giut set CAP 5 (75 x 1,2^4 = 155,5) - doi chung doc thang tu hang cua Giut set.
///   G. Choang 30% x 1,5 giay: 400 mau tia that.
///   I. CHUA BAN DU 10 LUOT THI QUA CAU VAN DUNG DO (nguoi dung 18/09/2026): tha cau o cho khong co ai ->
///      sau 6 giay (qua 4 giay cua ban cu) cau VAN CON va moi ban 0 luot; dat mot bia canh no -> ban du 10 luot
///      roi moi tan; hinh co vanh sang, co VIEN TRANG lon vong ngoai mat cau, va KHONG con tia set nho ban ra lien tuc.
///   H. Qua mang: goi phep so 13 tu nguoi kia -> may nay phat lai ra qua cau; minh tung -> goi gui di mang so 13.
///   J. TIA DIEN DUT QUANG (nguoi dung 18/09/2026, thay cac duong gan trang lien mach): bon khung VoTiaDien.fbx,
///      moi khung dut thanh nhieu doan roi (dem cum lien thong, DOI CHUNG bang vo gan cu), ong MONG hon han vo cu,
///      va tren canh la "TiaDienBoc" co ChopTiaDien doi khung lien tuc (do so lan doi trong 1,5 giay).
///
/// Ket qua: PlayTestShots/quacaudien.txt, anh quacaudien_*.png.
/// </summary>
public static class ThuQuaCauDien
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyCauDien;

    [MenuItem("Diablo 2.5D/74. Chay thu QUA CAU DIEN (ky nang moi)", false, 163)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu Qua cau dien", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Qua cau dien");
        DoHinhLuoiTruocKhiChay();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    // Ket qua do hinh luoi (do o Editor, TRUOC khi vao Play - xem DoHinhLuoiTruocKhiChay)
    static int cumTiaMoi = -1, cumVoCu = -1;
    static float dayTiaMoi = -1f, dayVoCu = -1f;

    /// <summary>
    /// Do HINH DANG luoi tia dien va luoi vo gan cu.
    ///
    /// ⚠️ Phai do o day, trong Editor, TRUOC khi vao Play: hai FBX nay de Read/Write TAT (dung cho
    /// dien thoai), nen trong Play `mesh.vertices` va `mesh.triangles` tra ve MANG RONG - lan chay
    /// dau 18/09/2026 do duoc "-1 cum" va bao oan ba loi. O day thi bat Read/Write tam, do xong tra
    /// lai dung gia tri cu roi nhap lai - dia van sach.
    /// </summary>
    static void DoHinhLuoiTruocKhiChay()
    {
        const string dTia = "Assets/Resources/KyNang/QuaCauDien/VoTiaDien.fbx";
        const string dCau = "Assets/Resources/KyNang/QuaCauDien/CauDien.fbx";
        var iTia = AssetImporter.GetAtPath(dTia) as ModelImporter;
        var iCau = AssetImporter.GetAtPath(dCau) as ModelImporter;
        if (iTia == null || iCau == null) return;
        bool cuTia = iTia.isReadable, cuCau = iCau.isReadable;
        try
        {
            if (!cuTia) { iTia.isReadable = true; iTia.SaveAndReimport(); }
            if (!cuCau) { iCau.isReadable = true; iCau.SaveAndReimport(); }

            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(dTia))
            {
                var m = o as Mesh;
                if (m != null && m.name.StartsWith("VoTia0")) { cumTiaMoi = SoCum(m); dayTiaMoi = BeDayOng(m); }
            }
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(dCau))
            {
                var m = o as Mesh;
                if (m != null && m.name.StartsWith("Vo")) { cumVoCu = SoCum(m); dayVoCu = BeDayOng(m); }
            }
        }
        finally
        {
            if (!cuTia) { iTia.isReadable = false; iTia.SaveAndReimport(); }
            if (!cuCau) { iCau.isReadable = false; iCau.SaveAndReimport(); }
        }
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_CauDien") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_CauDien");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CauDien] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static Damageable TaoBia(string ten, Vector3 p, float banKinh)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.radius = banKinh; cap.height = banKinh * 2f + 2f; cap.center = Vector3.up * (banKinh + 1f);
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 100000f; d.health = 100000f;
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
            if (!Physics.SphereCast(goc, 0.8f, h, out RaycastHit _, 18f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    /// <summary>Don quai that quanh mot cho: quai song cung an tia, lam so do khong con cong bang.</summary>
    static int DonQuai(Vector3 quanh, float trong)
    {
        int n = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude))
            if (q != null && Vector3.Distance(q.transform.position, quanh) < trong) { Object.Destroy(q.gameObject); n++; }
        return n;
    }

    /// <summary>Don sach moi qua cau con sot - tu 18/09/2026 cau doi den khi ban du 10 luot nen no khong tu tan.</summary>
    static int DonCau()
    {
        int n = 0;
        foreach (var c in Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude))
            if (c != null) { Object.DestroyImmediate(c.gameObject); n++; }
        return n;
    }

    static QuaCauDien CauTrenCanh()
    {
        var ds = Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude);
        return ds.Length > 0 ? ds[0] : null;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han0 = Time.time + 30f;
        while (dir == null && Time.time < han0) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);

        // ⚠️ TAT GameDirector SUOT PHEP THU (19/09/2026).
        // Phep thu nay chay lau (rieng muc G ban 400 tia set that de do ti le choang) va tu dung
        // lay bia rieng - no khong can mot con quai nao. De GameDirector chay thi dung giua chung
        // no sinh "dot 1" 24 con: 24 bo AI + model + hieu ung do vao giua luc dang ve hang tram
        // tia set, GPU qua tai, Windows reset driver (TDR) va Unity TAT HAN:
        //   "Failed to present D3D11 swapchain due to device reset/removed ... editor will shut down"
        // (da lam Unity cua nguoi dung sap mot lan, 19/09/2026).
        bool dirBatCu = false;
        if (dir != null) { dirBatCu = dir.enabled; dir.enabled = false; }
        int quaiDaDon = 0;
        foreach (var q0 in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude))
            if (q0 != null) { Object.Destroy(q0.gameObject); quaiDaDon++; }
        Ghi("(tat GameDirector suot phep thu, don " + quaiDaDon + " quai co san - GPU khong phai ve thua)");
        yield return null;

        var toi = TimToi();
        if (toi == null)
        {
            Ghi("[LOI] khong tim thay nhan vat"); loi++;
            if (dir != null) dir.enabled = dirBatCu;
            Ket(); yield break;
        }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e6f; mauToi.health = 1e6f;
        int maskEnemy = LayerMask.GetMask("Enemy");
        int maskCaNguoi = maskEnemy | LayerMask.GetMask("Player");

        // ================= A. THONG SO + TAI NGUYEN =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/CauDien");
        var mLoi = VfxFactory.LuoiLoiCauDien;
        var mVo = VfxFactory.LuoiVoCauDien;
        var aHaoQuang = Resources.Load<Texture2D>("KyNang/QuaCauDien/HaoQuangDien");
        var aTia = Resources.Load<Texture2D>("KyNang/QuaCauDien/TiaDien");
        var aHat = Resources.Load<Texture2D>("KyNang/QuaCauDien/HatDien");
        float satThuongMong = GiatSet.SatThuongNguoiChoi * CapDo.SatThuongTheoCap(5);

        Ghi(string.Format("A. so hieu {0}, so ky nang {1}; nang luong {2}, hoi chieu {3} s, niem {4} s; Sach phep doc {5}/{6}/{7}",
            K, CapDo.SoKyNang, toi.cauDienCost, toi.cauDienCooldown, toi.cauDienCastTime, nl, hc, nc));
        Ghi(string.Format("A. hang so: {0} luot x {1} s, {2} tia/luot, ban trong {3} m, tam ngam {4} m, choang {5:P0} x {6} s; sat thuong tia {7:F2} (Giut set {8} x 1,2^4 = {9:F2})",
            QuaCauDien.SoLuot, QuaCauDien.NhipLuot, QuaCauDien.SoTiaMoiLuot, QuaCauDien.BanKinhBan, QuaCauDien.Tam,
            QuaCauDien.XacSuatChoang, QuaCauDien.GiayChoang, QuaCauDien.SatThuongTia, GiatSet.SatThuongNguoiChoi, satThuongMong));
        // Luoi FBX de Read/Write TAT (nhu Qua cau bang) nen doc triangles ra rong - do bang KHUNG BAO
        Ghi(string.Format("A. luoi Blender: loi cau khung bao {0}, vo dien khung bao {1}; anh {2}/{3}/{4}; icon {5}",
            mLoi != null ? mLoi.bounds.size.ToString("F2") : "KHONG", mVo != null ? mVo.bounds.size.ToString("F2") : "KHONG",
            aHaoQuang != null ? aHaoQuang.width + "x" + aHaoQuang.height : "KHONG",
            aTia != null ? aTia.width + "x" + aTia.height : "KHONG",
            aHat != null ? aHat.width + "x" + aHat.height : "KHONG",
            tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG"));
        Ghi(string.Format("A. ten \"{0}\", tom tat \"{1}\", mo ta {2} ky tu; HUD {3} icon, icon so 13 {4}",
            SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length,
            bo != null ? bo.Length : -1, bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG"));
        Kiem(K == 13 && CapDo.SoKyNang == SachPhep.SoKyNangCoTen(), "so hieu sai, hoac co ky nang khong co ten");
        Kiem(Mathf.Approximately(toi.cauDienCost, 55f) && Mathf.Approximately(nl, 55f), "nang luong khong phai 55");
        Kiem(Mathf.Approximately(toi.cauDienCooldown, 5f) && Mathf.Approximately(hc, 5f), "hoi chieu khong phai 5 giay");
        Kiem(Mathf.Approximately(QuaCauDien.GiayChoToiDa, 30f), "han song cua qua cau khong phai 30 giay");
        Kiem(QuaCauDien.SoLuot == 10 && Mathf.Approximately(QuaCauDien.NhipLuot, 0.4f)
             && QuaCauDien.SoTiaMoiLuot == 5 && Mathf.Approximately(QuaCauDien.BanKinhBan, 9f)
             && Mathf.Approximately(QuaCauDien.XacSuatChoang, 0.3f) && Mathf.Approximately(QuaCauDien.GiayChoang, 1.5f)
             && Mathf.Approximately(QuaCauDien.Tam, 18f), "hang so Qua cau dien sai");
        Kiem(Mathf.Abs(QuaCauDien.SatThuongTia - satThuongMong) < 0.01f, "sat thuong tia khong bang Giut set cap 5");
        Kiem(mLoi != null && mVo != null && mLoi.bounds.size.magnitude > 0.5f && mVo.bounds.size.magnitude > 0.5f
             && aHaoQuang != null && aTia != null && aHat != null && tIcon != null,
             "thieu tai nguyen Blender cua Qua cau dien");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null, "HUD thieu icon Qua cau dien");
        Kiem(SachPhep.Ten(K) == "QUẢ CẦU ĐIỆN" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Qua cau dien");

        // ================= B + C. TUNG THAT, CHO DAT =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 choNgam = toi.transform.position + huong * 11f;
        choNgam.y = VfxFactory.GroundY(choNgam);
        int quaiDon = DonQuai(choNgam, 26f);
        yield return new WaitForSeconds(0.3f);

        int soPhep = 0, kyVua = -1;
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; kyVua = s; };
        toi.DaTungPhep += dem;
        CapDo.BatDauTranMoi();
        int s0 = soPhep;
        toi.CastAt(K, choNgam);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoCaDuongChoPhepThu(K);
        yield return new WaitForSeconds(0.3f);

        // C1. Co ke dich gan cho ngam -> cau phai dung CANH KE DICH
        Vector3 choBia = choNgam + huong * 3.5f;
        var biaC = TaoBia("TAM_CD_C", choBia, 0.6f);
        yield return new WaitForFixedUpdate();
        toi.mana = toi.maxMana;
        float mana0 = toi.mana;
        toi.CastAt(K, choNgam);
        float manaTon = mana0 - toi.mana;
        float hoiNgaySau = toi.HoiChieuGiay(K);
        yield return new WaitForSeconds(0.8f);
        var cau = CauTrenCanh();
        float xaBia = cau != null ? Vector3.Distance(new Vector3(cau.transform.position.x, 0f, cau.transform.position.z),
                                                    new Vector3(biaC.transform.position.x, 0f, biaC.transform.position.z)) : -1f;
        float xaNgam = cau != null ? Vector3.Distance(new Vector3(cau.transform.position.x, 0f, cau.transform.position.z),
                                                     new Vector3(choNgam.x, 0f, choNgam.z)) : -1f;
        int soRenderer = 0, soHat = 0;
        var hinh = GameObject.Find("HinhQuaCauDien");
        if (hinh != null)
        {
            foreach (var r in hinh.GetComponentsInChildren<MeshRenderer>(true)) if (r.enabled) soRenderer++;
            foreach (var p in hinh.GetComponentsInChildren<ParticleSystem>(true)) soHat++;
        }
        yield return Chup("quacaudien_1_cau");
        Ghi(string.Format("B. ({0} quai da don) khoa -> tu choi {1}; mo khoa -> tung {2}, ton {3:F1} nang luong, hoi chieu ngay sau {4:F1} s; qua cau tren canh {5} ({6} renderer, {7} he hat)",
            quaiDon, tuChoiKhoa, soPhep == s0 + 1 && kyVua == K, manaTon, hoiNgaySau, cau != null, soRenderer, soHat));
        Ghi(string.Format("C1. co ke dich cach cho ngam 3,5 m -> qua cau cach KE DICH {0:F2} m, cach cho ngam {1:F2} m (phai bam ke dich)",
            xaBia, xaNgam));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(Mathf.Abs(manaTon - 55f) < 0.01f, "khong ton dung 55 nang luong");
        Kiem(hoiNgaySau > 4f, "hoi chieu khong phai 5 giay");
        Kiem(cau != null && soRenderer >= 2 && soHat >= 1, "khong dung duoc hinh qua cau dien");
        Kiem(xaBia >= 0f && xaBia < 2.2f && xaBia < xaNgam, "qua cau khong hien ngay canh doi thu");

        // Doi con cau nay ban het roi moi do tiep, con thua thi don han
        float hanC = Time.time + 5.5f;
        while (CauTrenCanh() != null && Time.time < hanC) yield return null;
        Object.Destroy(biaC.gameObject);
        DonCau();
        yield return new WaitForSeconds(0.5f);

        // C2. DOI CHUNG: khong co ai gan cho ngam -> cau dung DUNG cho ngam
        {
            DonQuai(choNgam, 26f);
            // Hoi chieu 5 giay: con dang hoi ma goi CastAt thi phep bi tu choi va muc nay do mot con cau KHONG TON TAI
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            toi.mana = toi.maxMana;
            toi.CastAt(K, choNgam);
            yield return new WaitForSeconds(0.6f);
            var c2 = CauTrenCanh();
            float lech = c2 != null ? Vector3.Distance(new Vector3(c2.transform.position.x, 0f, c2.transform.position.z),
                                                      new Vector3(choNgam.x, 0f, choNgam.z)) : -1f;
            Ghi(string.Format("C2. DOI CHUNG khong co ke dich nao: qua cau cach cho ngam {0:F2} m (phai ~0)", lech));
            Kiem(c2 != null && lech < 0.4f, "khong co ke dich ma qua cau van khong dung cho ngam");
            float hanC2 = Time.time + 5.5f;
            while (CauTrenCanh() != null && Time.time < hanC2) yield return null;
            DonCau();
            yield return new WaitForSeconds(0.4f);
        }

        // ================= D + E + F. NHIP BAN, CHIA TIA, SAT THUONG =================
        Ghi("");
        {
            DonQuai(choNgam, 26f);
            DonCau();
            yield return new WaitForSeconds(0.3f);

            // 5 bia GAN (2,0..3,5 m) + 2 bia XA hon nhung VAN trong 9 m (6,5 m)
            var bia = new List<Damageable>();
            for (int i = 0; i < 5; i++)
            {
                float g = i * 72f;
                Vector3 p = choNgam + Quaternion.Euler(0f, g, 0f) * Vector3.forward * (2.0f + i * 0.3f);
                bia.Add(TaoBia("TAM_CD_G" + i, p, 0.5f));
            }
            for (int i = 0; i < 2; i++)
            {
                Vector3 p = choNgam + Quaternion.Euler(0f, 40f + i * 180f, 0f) * Vector3.forward * 6.5f;
                bia.Add(TaoBia("TAM_CD_X" + i, p, 0.5f));
            }
            yield return new WaitForFixedUpdate();

            var mau0 = new float[bia.Count];
            var lanTut = new int[bia.Count];
            var truoc = new float[bia.Count];
            for (int i = 0; i < bia.Count; i++) { mau0[i] = bia[i].health; truoc[i] = bia[i].health; }

            int luot0 = QuaCauDien.SoLuotDaBan, tia0 = QuaCauDien.SoTiaDaBan;
            // Ban thang tu cho ngam (khong qua CastAt) de qua cau dung GIUA vong bia
            var con = QuaCauDien.Spawn(choNgam, maskEnemy, mauToi);
            Vector3 choCau = con.transform.position;      // cau tu huy sau 10 luot; doc transform luc do la MissingReference
            float dauLuot = -1f, cuoiLuot = -1f;
            int luotTruoc = QuaCauDien.SoLuotDaBan;

            bool daChup = false;
            float han = Time.time + 7f;
            while (Time.time < han)
            {
                if (!daChup && QuaCauDien.SoLuotDaBan - luot0 >= 3)
                {
                    daChup = true;
                    yield return Chup("quacaudien_2_ban_tia");
                }
                if (QuaCauDien.SoLuotDaBan > luotTruoc)
                {
                    luotTruoc = QuaCauDien.SoLuotDaBan;
                    if (dauLuot < 0f) dauLuot = Time.time;
                    cuoiLuot = Time.time;
                }
                for (int i = 0; i < bia.Count; i++)
                {
                    if (bia[i] == null) continue;
                    if (bia[i].health < truoc[i] - 0.01f) { lanTut[i]++; truoc[i] = bia[i].health; }
                }
                yield return null;
            }

            int soLuot = QuaCauDien.SoLuotDaBan - luot0;
            int soTia = QuaCauDien.SoTiaDaBan - tia0;
            float keoDai = cuoiLuot - dauLuot;

            var mat = new float[bia.Count];
            for (int i = 0; i < bia.Count; i++) mat[i] = bia[i] != null ? mau0[i] - bia[i].health : 0f;

            Ghi(string.Format("D. so luot ban {0} (mong {1}), tu luot dau den luot cuoi {2:F2} giay (mong {3:F1} = 9 x 0,4)",
                soLuot, QuaCauDien.SoLuot, keoDai, (QuaCauDien.SoLuot - 1) * QuaCauDien.NhipLuot));
            Ghi(string.Format("E. 7 bia trong 9 m (5 bia gan 2,0-3,2 m, 2 bia xa 6,5 m): tong so tia ban ra {0} (mong {1} = 10 x 5)", soTia, QuaCauDien.SoLuot * QuaCauDien.SoTiaMoiLuot));
            for (int i = 0; i < bia.Count; i++)
                Ghi(string.Format("    bia {0} cach cau {1:F1} m: trung {2} lan, mat {3:F1} mau ({4:F1}/lan)",
                    i, bia[i] != null ? Vector3.Distance(bia[i].transform.position, choCau) : -1f,
                    lanTut[i], mat[i], lanTut[i] > 0 ? mat[i] / lanTut[i] : 0f));

            float moiTia = 0f; int demTia = 0;
            for (int i = 0; i < 5; i++) if (lanTut[i] > 0) { moiTia += mat[i] / lanTut[i]; demTia++; }
            if (demTia > 0) moiTia /= demTia;
            Ghi(string.Format("F. sat thuong trung binh moi tia len bia gan: {0:F2} (mong {1:F2} = Giut set cap 5)", moiTia, satThuongMong));

            Kiem(soLuot == QuaCauDien.SoLuot, "khong ban dung 10 luot");
            Kiem(Mathf.Abs(keoDai - (QuaCauDien.SoLuot - 1) * QuaCauDien.NhipLuot) < 0.25f, "nhip giua cac luot khong phai 0,4 giay");
            Kiem(soTia == QuaCauDien.SoLuot * QuaCauDien.SoTiaMoiLuot, "moi luot khong ban dung 5 tia");
            // Xep theo khoang cach THAT toi qua cau roi moi ket luan: dat bia theo vong tron quanh cho ngam thi
            // thu tu chi so KHONG phai thu tu gan-xa (lan chay dau 18/09/2026 bao oan vi doan theo chi so).
            var thuTu = new List<int>();
            for (int i = 0; i < bia.Count; i++) thuTu.Add(i);
            thuTu.Sort((x, y) =>
            {
                float dx = bia[x] != null ? Vector3.Distance(bia[x].transform.position, choCau) : 999f;
                float dy = bia[y] != null ? Vector3.Distance(bia[y].transform.position, choCau) : 999f;
                return dx.CompareTo(dy);
            });
            bool duGan = true, khongXa = true;
            for (int k = 0; k < QuaCauDien.SoTiaMoiLuot; k++)
                if (lanTut[thuTu[k]] < QuaCauDien.SoLuot) duGan = false;
            for (int k = QuaCauDien.SoTiaMoiLuot; k < thuTu.Count; k++)
                if (mat[thuTu[k]] > 0.5f) khongXa = false;
            Ghi(string.Format("E. 5 ke GAN NHAT (theo khoang cach that): bia {0},{1},{2},{3},{4}; ke thu 6 tro di: bia {5},{6}",
                thuTu[0], thuTu[1], thuTu[2], thuTu[3], thuTu[4], thuTu[5], thuTu[6]));
            Kiem(duGan, "5 ke gan nhat khong an du moi luot mot tia");
            Kiem(khongXa, "ke thu 6 tro di van an don - phai bo (nguoi dung chon moi ke mot tia, thua thi bo)");
            Kiem(Mathf.Abs(moiTia - satThuongMong) < 1f, "moi tia khong gay dung sat thuong Giut set cap 5");

            foreach (var b in bia) if (b != null) Object.Destroy(b.gameObject);
            if (con != null) Object.Destroy(con.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        // ================= G. CHOANG 30% x 1,5 GIAY =================
        Ghi("");
        {
            DonQuai(choNgam, 26f);
            DonCau();
            var bia = new List<Damageable>();
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = choNgam + Quaternion.Euler(0f, i * 72f, 0f) * Vector3.forward * 2.4f;
                bia.Add(TaoBia("TAM_CD_S" + i, p, 0.5f));
            }
            yield return new WaitForFixedUpdate();

            int soMau = 0, soChoang = 0;
            float giayDo = 0f;
            for (int lan = 0; lan < 8; lan++)
            {
                int tiaTruoc = QuaCauDien.SoTiaDaBan;
                var con = QuaCauDien.Spawn(choNgam, maskEnemy, mauToi);
                float han = Time.time + 4.6f;
                while (Time.time < han)
                {
                    foreach (var b in bia)
                    {
                        var st = b.GetComponent<StunnedEffect>();
                        if (st != null)
                        {
                            soChoang++;
                            giayDo = Mathf.Max(giayDo, st.remaining);
                            Object.DestroyImmediate(st);   // xoa ngay de dem duoc tung lan dinh
                        }
                    }
                    yield return null;
                }
                soMau += QuaCauDien.SoTiaDaBan - tiaTruoc;
                if (con != null) Object.Destroy(con.gameObject);
            }
            foreach (var b in bia) if (b != null) Object.Destroy(b.gameObject);

            Ghi(string.Format("G. {0} tia that -> choang {1} lan = {2:P1} (mong 30%); giay choang do duoc {3:F2} (mong {4})",
                soMau, soChoang, soMau > 0 ? soChoang / (float)soMau : 0f, giayDo, QuaCauDien.GiayChoang));
            Kiem(soMau >= 300, "qua it mau de ket luan ti le choang");
            Kiem(Mathf.Abs(soChoang / (float)soMau - 0.3f) < 0.07f, "ti le choang khong khoang 30%");
            Kiem(Mathf.Abs(giayDo - QuaCauDien.GiayChoang) < 0.05f, "choang khong keo 1,5 giay");
        }

        // ================= I. CHUA DU 10 LUOT THI VAN DUNG DO =================
        Ghi("");
        {
            DonQuai(choNgam, 26f);
            DonCau();
            yield return new WaitForSeconds(0.3f);

            int luot0 = QuaCauDien.SoLuotDaBan;
            var con = QuaCauDien.Spawn(choNgam, maskEnemy, mauToi);
            Vector3 choCau = con.transform.position;

            // 6 giay KHONG co ke dich nao: ban cu tan sau 4 giay, ban moi phai con nguyen.
            // Nhan tien dem luon tia set quanh cau - nguoi dung da BO hieu ung toe tia lien tuc (18/09/2026),
            // nen khong co ke dich thi phai KHONG co tia nao.
            int soKhungCoTia = 0;
            float han = Time.time + 6f;
            while (Time.time < han)
            {
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                    if (a != null && Vector3.Distance(a.transform.position, choCau) < 6f) { soKhungCoTia++; break; }
                yield return null;
            }
            bool conSongKhiVang = con != null;
            float lechCho = con != null ? Vector3.Distance(con.transform.position, choCau) : -1f;
            int luotSau6Giay = QuaCauDien.SoLuotDaBan - luot0;

            var hinhI = GameObject.Find("HinhQuaCauDien");
            bool coVanh = false;
            if (hinhI != null)
                foreach (var mf in hinhI.GetComponentsInChildren<MeshFilter>(true))
                    if (mf != null && mf.sharedMesh == VfxFactory.LuoiVanhCauDien && mf.sharedMesh != null) coVanh = true;

            // VIEN TRANG phai nam NGOAI mat cau (nguoi dung 18/09/2026: "keo cac vien trang ra ngoai y nhu
            // hinh phien ban truoc"), nhung khong duoc di qua xa - xa qua la thanh gai chia ra nhu ban bi che.
            float xaVien = 0f, xaVanh = 0f;
            var mVien = VfxFactory.LuoiVoCauDien; var mVanh = VfxFactory.LuoiVanhCauDien;
            if (mVien != null) xaVien = mVien.bounds.extents.magnitude / Mathf.Sqrt(3f);
            if (mVanh != null) xaVanh = mVanh.bounds.extents.magnitude / Mathf.Sqrt(3f);

            // Gio moi dat bia canh no -> phai ban du 10 luot
            var biaI = TaoBia("TAM_CD_I", choCau + Vector3.right * 2.5f, 0.6f);
            yield return new WaitForFixedUpdate();
            float hanI = Time.time + 9f;
            while (con != null && Time.time < hanI) yield return null;
            int luotTong = QuaCauDien.SoLuotDaBan - luot0;
            bool daTan = con == null;
            Object.Destroy(biaI.gameObject);

            Ghi(string.Format("I. tha cau o cho khong co ai: sau 6 giay cau con song {0} (ban cu tan sau 4 giay), da ban {1} luot, dich khoi cho {2:F2} m; han song {3} giay",
                conSongKhiVang, luotSau6Giay, lechCho, QuaCauDien.GiayChoToiDa));
            Ghi(string.Format("I. hinh: co vanh sang {0}; {1} khung thay tia set quanh cau (phai la 0 - da bo tia nho ban lien tuc); vien trang xa tam {2:F3}, vanh sang {3:F3} -> vien {4}",
                coVanh, soKhungCoTia, xaVien, xaVanh, xaVien > xaVanh ? "nam NGOAI nhu anh nguoi dung gui" : "DANG OM SAT cau"));
            Ghi(string.Format("I. dat bia canh no -> tong so luot da ban {0} (mong {1}), sau do tu tan {2}",
                luotTong, QuaCauDien.SoLuot, daTan));
            Kiem(conSongKhiVang && luotSau6Giay == 0, "khong co ke dich ma qua cau van tinh luot / van bien mat");
            Kiem(lechCho >= 0f && lechCho < 0.05f, "qua cau khong dung nguyen mot cho");
            Kiem(coVanh, "hinh thieu vanh sang bao boc ben ngoai");
            Kiem(soKhungCoTia == 0, "van con tia set toe ra khi khong co ke dich - nguoi dung da bo hieu ung nay");
            Kiem(xaVien > xaVanh && xaVien < xaVanh * 1.6f,
                 "vien trang khong nam ngoai mat cau (hoac chia ra qua xa thanh gai)");
            Kiem(luotTong == QuaCauDien.SoLuot && daTan, "co ke dich roi ma khong ban du 10 luot roi tan");
            if (con != null) Object.Destroy(con.gameObject);
            yield return new WaitForSeconds(0.4f);
        }

        // ================= H. QUA MANG =================
        Ghi("");
        TranHienTai.DangChoiMang = true;
        var vuong = Vector3.Cross(Vector3.up, huong).normalized;
        var kia = NguoiChoiKhac.Sinh("uid-caudien", "Người kia", toi.transform.position + vuong * 8f);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi choi khac"); loi++; }
        else
        {
            var goDb = new GameObject("TAM_DongBoCauDien");
            var db = goDb.AddComponent<DongBoTran>();
            db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
            KenhTrucTiep.Tao(); KenhTrucTiep.NhanTraLoi("{}");
            var daGui = new List<string>();
            KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
            yield return new WaitForSeconds(0.5f);

            int truocCon = Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude).Length;
            var goi = GoiTin.VietKyNang(new GoiTin.MotPhep
            {
                chiSo = 1, kyNang = (byte)K, capKyNang = 1, soThuTu = 601,
                diemNgam = kia.transform.position + vuong * 5f
            });
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(goi));
            yield return new WaitForSeconds(1.2f);
            int sauCon = Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude).Length;

            daGui.Clear();
            toi.mana = toi.maxMana;
            toi.CastAt(K, choNgam);
            yield return new WaitForSeconds(0.9f);
            bool guiSo13 = false;
            foreach (var g0 in daGui)
            {
                var b0 = GoiTin.TuChuoi(g0);
                if (b0 == null || GoiTin.LoaiCuaGoi(b0) != GoiTin.LoaiKyNang) continue;
                GoiTin.MotPhep pp;
                if (GoiTin.DocKyNang(b0, out pp) && pp.kyNang == K) guiSo13 = true;
            }
            Ghi(string.Format("H. goi phep so 13 tu nguoi kia -> so qua cau tren canh {0} -> {1}; minh tung -> goi gui di mang so 13: {2}",
                truocCon, sauCon, guiSo13));
            Kiem(sauCon > truocCon, "goi ky nang 13 tu mang khong phat lai duoc");
            Kiem(guiSo13, "minh tung Qua cau dien ma khong co goi nao mang so hieu 13");

            Object.DestroyImmediate(goDb);
            if (kia != null) Object.Destroy(kia.gameObject);
        }
        TranHienTai.DangChoiMang = false;

        // ================= J. TIA DIEN DUT QUANG + CHOP TAT =================
        Ghi("");
        {
            var khung = VfxFactory.KhungTiaDien;
            int soKhung = khung != null ? khung.Length : 0;

            // Cum lien thong va be day ong da do o Editor truoc khi vao Play (Read/Write TAT)
            int cumTia = cumTiaMoi;
            float dayTia = dayTiaMoi;

            // Tren canh: tha mot qua cau roi xem lop boc la gi
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            toi.mana = toi.maxMana;
            Vector3 choJ = toi.transform.position + huong * 9f;
            choJ.y = VfxFactory.GroundY(choJ);
            toi.CastAt(K, choJ);
            yield return new WaitForSeconds(0.3f);

            var boc = GameObject.Find("TiaDienBoc");
            var voLienMach = GameObject.Find("VoDien");
            var chop = boc != null ? boc.GetComponent<ChopTiaDien>() : null;
            var mfBoc = boc != null ? boc.GetComponent<MeshFilter>() : null;

            int doi0 = chop != null ? chop.soLanDoi : -1;
            var daThay = new System.Collections.Generic.HashSet<Object>();
            float hanJ = Time.time + 1.5f;
            while (Time.time < hanJ)
            {
                if (mfBoc != null && mfBoc.sharedMesh != null) daThay.Add(mfBoc.sharedMesh);
                yield return null;
            }
            int soDoi = chop != null ? chop.soLanDoi - doi0 : -1;

            Ghi(string.Format("J. VoTiaDien.fbx: {0} khung; khung 0 co {1} cum roi (doi chung vo gan cu: {2} cum); be day ong {3:F3} (vo cu {4:F3})",
                soKhung, cumTia, cumVoCu, dayTia, dayVoCu));
            Ghi(string.Format("J. tren canh: TiaDienBoc {0}, VoDien lien mach {1}, ChopTiaDien {2}; trong 1,5 s doi khung {3} lan, thay {4} khung khac nhau",
                boc != null, voLienMach != null, chop != null, soDoi, daThay.Count));
            Kiem(soKhung == 4, "khong nap du bon khung tia dien");
            Kiem(cumTia >= 15, "khung tia khong dut quang (qua it cum roi)");
            Kiem(cumVoCu > 0 && cumTia > cumVoCu * 2, "doi chung hong: tia moi khong dut nhieu hon vo gan cu");
            // Mong hon gan cu it nhat 20%. Khong dam mong hon nua: ban 0,011 do duoc 18/09/2026 vo
            // thanh dom lam tam khi nhin o cu ly choi that (anh tia_dien_trong_game.png) - het ra net tia.
            Kiem(dayTia > 0f && dayVoCu > 0f && dayTia < dayVoCu * 0.8f, "tia moi khong mong hon vo gan cu");
            Kiem(boc != null && chop != null && voLienMach == null, "tren canh khong phai lop tia dut quang");
            Kiem(soDoi >= 10 && daThay.Count >= 3, "tia khong chop tat lien tuc");

            foreach (var c in Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude)) if (c != null) Object.Destroy(c.gameObject);
            yield return new WaitForSeconds(0.2f);
        }

        foreach (var c in Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude)) if (c != null) Object.Destroy(c.gameObject);
        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude)) if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
        toi.DaTungPhep -= dem;
        if (dir != null) dir.enabled = dirBatCu;
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>
    /// So CUM LIEN THONG cua luoi: gop dinh trung vi tri (Unity tach dinh theo normal/UV) roi
    /// noi theo tam giac. Duong lien mach = 1 cum; tia dut quang = moi doan mot cum.
    /// </summary>
    static int SoCum(Mesh m)
    {
        if (m == null) return -1;
        var v = m.vertices; var tri = m.triangles;
        if (v.Length == 0 || tri.Length == 0) return -1;

        // gop dinh trung cho (lam tron 0,1 mm)
        var cho = new System.Collections.Generic.Dictionary<Vector3Int, int>();
        var goc = new int[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            var k = new Vector3Int(Mathf.RoundToInt(v[i].x * 10000f), Mathf.RoundToInt(v[i].y * 10000f), Mathf.RoundToInt(v[i].z * 10000f));
            int g;
            if (!cho.TryGetValue(k, out g)) { g = cho.Count; cho[k] = g; }
            goc[i] = g;
        }
        var cha = new int[cho.Count];
        for (int i = 0; i < cha.Length; i++) cha[i] = i;
        System.Func<int, int> tim = null;
        tim = x => { while (cha[x] != x) { cha[x] = cha[cha[x]]; x = cha[x]; } return x; };
        for (int t = 0; t + 2 < tri.Length; t += 3)
        {
            int a = tim(goc[tri[t]]), b = tim(goc[tri[t + 1]]), c = tim(goc[tri[t + 2]]);
            if (a != b) cha[b] = a;
            if (a != c) cha[tim(c)] = a;
        }
        var dau = new System.Collections.Generic.HashSet<int>();
        for (int i = 0; i < cha.Length; i++) dau.Add(tim(i));
        return dau.Count;
    }

    /// <summary>
    /// Be day ong: ong ba canh co canh ngang = r x can3, nen lay TRUNG VI canh ngan nhat cua moi
    /// tam giac roi chia can3. Do nay doc lap voi cach dung luoi (khong hoi Blender con so nao).
    /// </summary>
    static float BeDayOng(Mesh m)
    {
        if (m == null) return -1f;
        var v = m.vertices; var tri = m.triangles;
        if (v.Length == 0 || tri.Length == 0) return -1f;
        var ds = new System.Collections.Generic.List<float>();
        for (int t = 0; t + 2 < tri.Length; t += 3)
        {
            float a = Vector3.Distance(v[tri[t]], v[tri[t + 1]]);
            float b = Vector3.Distance(v[tri[t + 1]], v[tri[t + 2]]);
            float c = Vector3.Distance(v[tri[t + 2]], v[tri[t]]);
            float nho = Mathf.Min(a, Mathf.Min(b, c));
            if (nho > 1e-5f) ds.Add(nho);
        }
        if (ds.Count == 0) return -1f;
        ds.Sort();
        return ds[ds.Count / 2] / Mathf.Sqrt(3f);
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[CauDien] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/quacaudien.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_CauDien", "TAM_DongBoCauDien" })
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
