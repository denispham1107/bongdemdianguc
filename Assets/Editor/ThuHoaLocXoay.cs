using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 75): GIO LOC hoi mana + ky nang moi "HOA LOC XOAY" (so hieu 14, 18/09/2026).
///
/// Nguoi dung:
///   - Gio loc: moi lan danh trung ke dich HOI 10 MANA co dinh cho ca 5 cap; CAP 5 chi ton 25 nang luong.
///   - Hoa loc xoay: doi con Gio loc dang bay cua LAN TUNG GAN NHAT thanh Loc xoay (to dan), giu toc do bay
///     cua Gio loc va moi hieu ung cuon cua Loc xoay; sat thuong = cap hien tai Gio loc + cap hien tai Loc xoay;
///     45 nang luong, hoi chieu 0,5 giay. Khong co loc nao dang bay -> tu choi, khong ton mana.
///     Gio loc cap 5 ra hai con -> hoa ca hai.
///
///   A. Thong so: so hieu 14, 15 ky nang; 45 nang luong, hoi chieu 0,5; ten co dau; HUD 15 icon; icon file.
///   B. Gio loc hoi mana: ban mot loc vao 3 bia -> hoi dung 3 x 10 mana (DOI CHUNG: khong trung ai thi hoi 0).
///   C. Nang luong Gio loc theo cap: cap 1..4 tinh theo cong thuc cu, CAP 5 = dung 25.
///   D. Hoa that: tung Gio loc roi bam Hoa loc xoay -> con Gio loc BIEN MAT, co Loc xoay moi ngay cho do,
///      toc do bay = 9,5 (cua Gio loc), song 6 giay, va hinh TO DAN (do ti le hinh hai thoi diem).
///   E. Sat thuong cong ca hai: bia dung yen -> an don cham 75 x cap Gio loc, roi mat mau tiep theo giay.
///   F. Khong co loc nao dang bay -> tu choi, KHONG tru mana, KHONG vao hoi chieu.
///   G. Gio loc cap 5 (hai loc) -> mot lan bam hoa CA HAI.
///
/// Ket qua: PlayTestShots/hoalocxoay.txt, anh hoalocxoay_*.png.
/// </summary>
public static class ThuHoaLocXoay
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyHoaLocXoay;

    [MenuItem("Diablo 2.5D/75. Chay thu HOA LOC XOAY + Gio loc hoi mana", false, 164)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu Hoa loc xoay", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Hoa loc xoay + Gio loc hoi mana");
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
        if (GameObject.Find("TAM_HoaLoc") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_HoaLoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[HoaLoc] " + s); }
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
            if (!Physics.SphereCast(goc, 0.9f, h, out RaycastHit _, 20f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static int DonQuai(Vector3 quanh, float trong)
    {
        int n = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude))
            if (q != null && Vector3.Distance(q.transform.position, quanh) < trong) { Object.Destroy(q.gameObject); n++; }
        return n;
    }

    static int DonLoc()
    {
        int n = 0;
        foreach (var g in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude))
            if (g != null) { Object.DestroyImmediate(g.gameObject); n++; }
        foreach (var t in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude))
            if (t != null) Object.DestroyImmediate(t.gameObject);
        return n;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han0 = Time.time + 30f;
        while (dir == null && Time.time < han0) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e6f; mauToi.health = 1e6f;
        int maskEnemy = LayerMask.GetMask("Enemy");

        // ================= A. THONG SO =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/HoaLocXoay");
        Ghi(string.Format("A. so hieu {0}, so ky nang {1}; nang luong {2}, hoi chieu {3} s, niem {4} s; Sach phep doc {5}/{6}/{7}",
            K, CapDo.SoKyNang, toi.hoaLocXoayCost, toi.hoaLocXoayCooldown, toi.hoaLocXoayCastTime, nl, hc, nc));
        Ghi(string.Format("A. ten \"{0}\", tom tat \"{1}\", mo ta {2} ky tu; HUD {3} icon, icon so 14 {4}; icon file {5}; Gio loc hoi {6} mana moi lan trung",
            SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length, bo != null ? bo.Length : -1,
            bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG",
            tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG", GioLoc.ManaHoiMoiLanTrung));
        Kiem(K == 14 && CapDo.SoKyNang == 15, "so hieu / so ky nang sai");
        Kiem(Mathf.Approximately(toi.hoaLocXoayCost, 45f) && Mathf.Approximately(nl, 45f), "nang luong khong phai 45");
        Kiem(Mathf.Approximately(toi.hoaLocXoayCooldown, 0.5f) && Mathf.Approximately(hc, 0.5f), "hoi chieu khong phai 0,5 giay");
        Kiem(Mathf.Approximately(GioLoc.ManaHoiMoiLanTrung, 10f), "Gio loc khong hoi 10 mana moi lan trung");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Hoa loc xoay");
        Kiem(SachPhep.Ten(K) == "HOÁ LỐC XOÁY" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Hoa loc xoay");

        // ================= C. NANG LUONG GIO LOC THEO CAP =================
        Ghi("");
        {
            var sb = new StringBuilder();
            for (int c = 1; c <= 5; c++)
            {
                float ton = GioLoc.NangLuongCan(c, toi.gioLocCost, CapDo.ManaTheoCap(c));
                sb.Append("cap ").Append(c).Append("=").Append(ton.ToString("F1")).Append("  ");
            }
            float ton5 = GioLoc.NangLuongCan(5, toi.gioLocCost, CapDo.ManaTheoCap(5));
            float cu5 = toi.gioLocCost * CapDo.ManaTheoCap(5) * GioLoc.HeSoNangLuongTheoCap(5);
            Ghi("C. nang luong Gio loc: " + sb.ToString().Trim() + string.Format("  (cap 5 ban cu la {0:F1})", cu5));
            Kiem(Mathf.Abs(ton5 - 25f) < 0.01f, "cap 5 khong ton dung 25 nang luong");
        }

        // ================= B. GIO LOC HOI MANA KHI TRUNG =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        int quaiDon = DonQuai(toi.transform.position, 30f);
        yield return new WaitForSeconds(0.3f);

        CapDo.BatDauTranMoi();
        CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap) * 3);
        CapDo.MoCaDuongChoPhepThu(CapDo.KyGioLoc);
        CapDo.MoCaDuongChoPhepThu(3);              // Loc xoay
        CapDo.MoCaDuongChoPhepThu(K);

        int soPhep = 0;
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; };
        toi.DaTungPhep += dem;

        {
            // Ba bia xep doc duong bay - mot con loc quet qua ca ba
            var bia = new List<Damageable>();
            for (int i = 0; i < 3; i++)
                bia.Add(TaoBia("TAM_HL_B" + i, toi.transform.position + huong * (5f + i * 3f), 0.6f));
            yield return new WaitForFixedUpdate();

            toi.mana = toi.maxMana * 0.5f;
            float mana0 = toi.mana;
            float hoi0 = GioLoc.ManaDaHoi;
            int trung0 = GioLoc.SoLanTrung;
            toi.CastAt(CapDo.KyGioLoc, toi.transform.position + huong * 12f);
            yield return new WaitForSeconds(0.5f);
            float manaSauTru = toi.mana;          // da tru nang luong, chua trung ai
            float han = Time.time + 3.5f;
            while (Time.time < han) yield return null;

            int soTrung = GioLoc.SoLanTrung - trung0;
            float hoiDuoc = GioLoc.ManaDaHoi - hoi0;
            Ghi(string.Format("B. ({0} quai da don) mot con Gio loc quet 3 bia: trung {1} lan -> hoi {2:F0} mana (mong {3:F0}); mana {4:F1} -> {5:F1} -> {6:F1}",
                quaiDon, soTrung, hoiDuoc, soTrung * GioLoc.ManaHoiMoiLanTrung, mana0, manaSauTru, toi.mana));
            Kiem(soTrung == 3, "con loc khong quet trung ca 3 bia");
            Kiem(Mathf.Abs(hoiDuoc - soTrung * GioLoc.ManaHoiMoiLanTrung) < 0.01f, "khong hoi dung 10 mana moi lan trung");

            // DOI CHUNG: ban ra cho trong, khong trung ai -> khong hoi mana
            foreach (var b in bia) if (b != null) Object.Destroy(b.gameObject);
            DonLoc();
            yield return new WaitForSeconds(0.6f);
            float hoiTruoc2 = GioLoc.ManaDaHoi;
            toi.mana = toi.maxMana * 0.5f;
            toi.CastAt(CapDo.KyGioLoc, toi.transform.position + huong * 12f);
            yield return new WaitForSeconds(4f);
            float hoi2 = GioLoc.ManaDaHoi - hoiTruoc2;
            Ghi(string.Format("B. DOI CHUNG ban vao cho trong (khong co ai): hoi {0:F0} mana (phai la 0)", hoi2));
            Kiem(Mathf.Abs(hoi2) < 0.01f, "khong trung ai ma van hoi mana");
            DonLoc();
        }

        // ================= F. KHONG CO LOC NAO DANG BAY =================
        Ghi("");
        {
            DonLoc();
            yield return new WaitForSeconds(0.3f);
            toi.mana = toi.maxMana;
            float manaTruoc = toi.mana;
            int phepTruoc = soPhep;
            toi.CastAt(K, toi.transform.position + huong * 8f);
            yield return null;
            bool tuChoi = soPhep == phepTruoc;
            float hoiConLai = toi.HoiChieuGiay(K);
            Ghi(string.Format("F. khong co Gio loc nao dang bay -> tu choi {0}, mana {1:F1} -> {2:F1}, hoi chieu {3:F2} s (phai 0)",
                tuChoi, manaTruoc, toi.mana, hoiConLai));
            Kiem(tuChoi, "khong co loc ma van tung duoc");
            Kiem(Mathf.Abs(manaTruoc - toi.mana) < 0.01f, "bam hut ma van tru mana");
            Kiem(hoiConLai <= 0.01f, "bam hut ma van vao hoi chieu");
        }

        // ================= D + E. HOA THAT =================
        Ghi("");
        {
            DonLoc();
            DonQuai(toi.transform.position, 30f);
            var bia = TaoBia("TAM_HL_E", toi.transform.position + huong * 9f, 0.8f);
            yield return new WaitForFixedUpdate();

            toi.mana = toi.maxMana;
            toi.CastAt(CapDo.KyGioLoc, toi.transform.position + huong * 14f);
            yield return new WaitForSeconds(0.55f);       // qua thoi gian niem, loc da bay ra

            var locTruoc = Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude);
            int soGioLocTruoc = locTruoc.Length;

            float mauB0 = bia.health;
            float manaTruocHoa = toi.mana;
            toi.CastAt(K, toi.transform.position + huong * 14f);

            // Con Gio loc BAY 9,5 m/s nen vi tri cua no doi tung khung: phai theo doi den TAN LUC HOA,
            // khong duoc doc mot lan roi so sanh (lan chay dau bao oan "lech 5,12 m" - dung bang quang
            // duong loc bay trong 0,55 giay niem chieu).
            Vector3 choLocCuoi = soGioLocTruoc > 0 ? locTruoc[0].transform.position : Vector3.zero;
            Vector3 choLocKhiHoa = Vector3.zero, choXoayLucDau = Vector3.zero;
            bool daThayXoay = false;
            float coNhoNhat = 9f, coLonNhat = -9f;
            Tornado xoay = null;
            float hanHoa = Time.time + 1.2f;
            while (Time.time < hanHoa)
            {
                var gl = Object.FindAnyObjectByType<GioLoc>();
                if (gl != null) choLocCuoi = gl.transform.position;

                var tn = Object.FindAnyObjectByType<Tornado>();
                if (tn != null)
                {
                    xoay = tn;
                    // CON LOC XOAY CUNG BAY 9,5 m/s: phai ghi vi tri o KHUNG DAU TIEN thay no, doc sau
                    // thi no da di xa (lan chay truoc bao oan "lech 9,35 m").
                    if (!daThayXoay)
                    {
                        daThayXoay = true;
                        choXoayLucDau = tn.transform.position;
                        choLocKhiHoa = choLocCuoi;
                    }
                    if (tn.transform.childCount > 0)
                    {
                        float co = tn.transform.GetChild(0).localScale.x;
                        coNhoNhat = Mathf.Min(coNhoNhat, co);
                        coLonNhat = Mathf.Max(coLonNhat, co);
                    }
                }
                yield return null;
            }
            Vector3 choLoc = choLocKhiHoa;

            int soGioLocSau = Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude).Length;
            if (xoay == null) xoay = Object.FindAnyObjectByType<Tornado>();
            float tocDo = xoay != null ? xoay.moveSpeed : -1f;
            float song = xoay != null ? xoay.duration : -1f;
            float donCham = xoay != null ? xoay.donChamMotLan : -1f;
            float dps = xoay != null ? xoay.damagePerSecond : -1f;
            float lechCho = daThayXoay && soGioLocTruoc > 0
                          ? Vector3.Distance(new Vector3(choXoayLucDau.x, 0f, choXoayLucDau.z),
                                             new Vector3(choLoc.x, 0f, choLoc.z)) : -1f;

            // TO DAN: doc o TRONG vong theo doi tren (coNhoNhat luc vua hoa, coLonNhat luc phinh xong)
            float coDau = coNhoNhat < 9f ? coNhoNhat : -1f;
            float coSau = coLonNhat > -9f ? coLonNhat : -1f;
            yield return Chup("hoalocxoay_1_da_hoa");

            // E. sat thuong: doi con loc cuon qua bia
            float han = Time.time + 4f;
            while (Time.time < han) yield return null;
            float mat = mauB0 - bia.health;
            float capGioLoc = CapDo.SatThuongTheoCap(Mathf.Max(1, CapDo.CapCuaKyNang(CapDo.KyGioLoc)));
            float donMong = GioLoc.SatThuongGoc * capGioLoc;

            Ghi(string.Format("D. truoc khi hoa co {0} con Gio loc; sau khi hoa con {1} Gio loc, co Loc xoay {2} (lech cho {3:F2} m); ton {4:F0} mana",
                soGioLocTruoc, soGioLocSau, xoay != null, lechCho, manaTruocHoa - toi.mana));
            Ghi(string.Format("D. Loc xoay hoa ra: toc do {0:F1} m/s (Gio loc {1}, Loc xoay goc 3,4), song {2:F1} s, don cham mot lan {3:F1} (mong {4:F1}), sat thuong moi giay {5:F1}",
                tocDo, GioLoc.TocDo, song, donCham, donMong, dps));
            Ghi(string.Format("D. TO DAN: ti le hinh ngay sau khi hoa {0:F2} -> sau {1} giay {2:F2}", coDau, HoaLocXoay.GiayPhongTo, coSau));
            Ghi(string.Format("E. bia dung yen mat {0:F0} mau (don cham {1:F0} + sat thuong moi giay cua Loc xoay)", mat, donMong));

            Kiem(soGioLocTruoc > 0, "doi chung hong: khong co con Gio loc nao de hoa");
            Kiem(soGioLocSau == 0 && xoay != null, "hoa xong ma van con Gio loc / khong co Loc xoay");
            Kiem(lechCho >= 0f && lechCho < 2f, "Loc xoay khong hien ra ngay cho con Gio loc");
            Kiem(Mathf.Abs(tocDo - GioLoc.TocDo) < 0.01f, "Loc xoay hoa ra khong giu toc do bay cua Gio loc");
            Kiem(Mathf.Abs(donCham - donMong) < 0.5f, "don cham mot lan khong bang sat thuong Gio loc cap hien tai");
            Kiem(coDau > 0f && coDau < 0.6f && coSau > 0.95f, "hinh khong TO DAN sau khi hoa (phai tu ~0,42 len 1,00)");
            Kiem(mat > donMong, "bia khong an ca don cham lan sat thuong moi giay");
            if (bia != null) Object.Destroy(bia.gameObject);
            DonLoc();
        }

        // ================= G. CAP 5 HOA CA HAI CON =================
        Ghi("");
        {
            DonLoc();
            for (int i = 0; i < 6 && CapDo.CapCuaKyNang(CapDo.KyGioLoc) < 5; i++)
            {
                CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
                CapDo.NangCap(CapDo.KyGioLoc);
            }
            int capGL = CapDo.CapCuaKyNang(CapDo.KyGioLoc);
            toi.mana = toi.maxMana;
            toi.CastAt(CapDo.KyGioLoc, toi.transform.position + huong * 14f);
            yield return new WaitForSeconds(0.55f);
            int soLocRa = Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude).Length;

            int hoa0 = HoaLocXoay.SoLocDaHoa;
            toi.mana = toi.maxMana;
            toi.CastAt(K, toi.transform.position + huong * 14f);
            yield return new WaitForSeconds(0.55f);
            int daHoa = HoaLocXoay.SoLocDaHoa - hoa0;
            int soXoay = Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude).Length;
            int conGioLoc = Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude).Length;

            Ghi(string.Format("G. Gio loc cap {0}: tung ra {1} con; mot lan bam Hoa loc xoay -> hoa {2} con, tren canh co {3} Loc xoay, con {4} Gio loc",
                capGL, soLocRa, daHoa, soXoay, conGioLoc));
            Kiem(capGL == 5 && soLocRa == 2, "doi chung hong: cap 5 khong ra hai con Gio loc");
            Kiem(daHoa == 2 && soXoay >= 2 && conGioLoc == 0, "mot lan bam khong hoa ca hai con");
            DonLoc();
        }

        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude))
            if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
        toi.DaTungPhep -= dem;
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
        Debug.Log("[HoaLoc] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/hoalocxoay.txt", bao.ToString());
        var rac = GameObject.Find("TAM_HoaLoc");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
