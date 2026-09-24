using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 72): KY NANG "LUA DIA NGUC" (so hieu 11, 17/09/2026).
///
/// Nguoi dung: sat thuong ban dau = Qua cau lua cap 5, hoi chieu 0,5 s; 4 qua cau lua nhu Qua cau lua nhung TU DI dung vao
/// nguoi choi / quai o gan; van thieu dot. Nguoi dung chon: cap 1 = 176 roi van +20%/cap; chia toi da 4 ke gan nguoi tung nhat
/// trong 20 m (it hon thi qua du nham ke gan nhat, muc tieu chet thi doi); bam chac; 25 nang luong, niem 0,38; khong ai thi bay
/// thang toa quat; lua do sam; icon Blender MCP; toa quat roi uon cong.
///
///   Sua 17/09/2026 (nguoi dung): THEM MOT QUA (5 qua), nang luong 31; mau qua va vu no GIONG HET Qua cau lua (bo lop nhuom do sam).
///   A. Thong so: so hieu 11, 12 ky nang, nang luong / hoi chieu / niem that tren nhan vat + Sach phep, ten co dau, HUD 12 icon,
///      icon file; sat thuong goc = impactDamage PREFAB qua cau lua x 1,2^4.
///   B. Tung that CastAt(11): khoa -> tu choi; mo -> dung 5 qua "LuaDiaNguc", tru 31; bam lai moi khung -> duoc nhan sau 0,5 s.
///   C. Di muc tieu: 5 bia dat NGOAI hinh quat (hai ben, phia sau) trong 20 m + 1 bia 25 m. Moi qua gan mot bia khac nhau;
///      ca 5 bia trung, bia 25 m khong. DOI CHUNG: chum Qua cau lua thuong cung cho cung huong -> 0 bia trung.
///   D. Mot bia: ca 4 qua vao no. E. Bia CHAY NGANG 5 m/s: van trung. F. Muc tieu bi giet giua duong -> qua sang bia con lai.
///   G. Khong co ai: 4 qua bay thang (huong khong doi sau 0,4 s), lech nhau 18 do.
///   H. Sat thuong mot qua cap 1 so voi mot qua Qua cau lua CAP 5 that, cung khoang cach (ca hai co giam theo khoang cach) ~ 1;
///      bia bi thieu dot (BurningEffect); cap 3 impactDamage = goc x 1,44.
///   I. Mau: hat lua cua qua dia nguc GIONG HET qua cau lua thuong (ti le kenh xanh la / do = 1,000).
///   J. Qua mang: goi ky nang 11 tu nguoi kia -> may minh ra 4 qua; minh tung -> goi mang kyNang 11. Giet bia -> ke danh = nguoi tung.
///   K. XUYEN VAT CAN NHO (nguoi dung 19/09/2026): bay xuyen bia / mo / da, KHONG xuyen duoc cay coi va nha.
///      K1 phan loai TUNG vat can that trong Act2 (755 cai) theo nhom cha; K2/K3 ban qua that xuyen vat thu
///      tren troi; K4 ban qua that vao BIA THAT va CAY THAT trong nghia dia; K5 duong tung that co bat co xuyen.
///
/// Ket qua: PlayTestShots/luadianguc.txt, anh luadianguc_*.png.
/// </summary>
public static class ThuLuaDiaNguc
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyLuaDiaNguc;

    [MenuItem("Diablo 2.5D/72. Chay thu LUA DIA NGUC (ky nang moi)", false, 161)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu lua dia nguc", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Lua dia nguc");
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
        if (GameObject.Find("TAM_LuaDiNguc") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_LuaDiNguc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[LuaDiaNguc] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    const float MauBia = 100000f;   // 1e7 thi float chi chinh xac toi 1 don vi - lan truoc lech 312/311

    static Damageable TaoBia(string ten, Vector3 p)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = MauBia; d.health = MauBia;
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
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoLuaDiaNguc") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static List<Fireball> QuaDiaNguc()
    {
        var ds = new List<Fireball>();
        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude)) if (f.diaNguc) ds.Add(f);
        return ds;
    }

    static void XoaQua()
    {
        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude)) Object.Destroy(f.gameObject);
    }

    /// <summary>Tim cho dat bia o khoang cach kc quanh goc, huong lech 'goc' do so voi h, duong thang toi do trong (cau 1 m).</summary>
    static bool TimCho(PlayerController pc, Vector3 goc, Vector3 h, float lechDo, float kc, List<Vector3> daDat, out Vector3 cho)
    {
        for (int t = 0; t < 36; t++)
        {
            float g = lechDo + (t % 2 == 0 ? 1 : -1) * ((t + 1) / 2) * 5f;
            Vector3 hh = Quaternion.AngleAxis(g, Vector3.up) * h;
            Vector3 p = goc + hh * kc;
            bool trung = false;
            foreach (var q in daDat) if ((q - p).sqrMagnitude < 16f) trung = true;
            if (trung) continue;
            Vector3 tu = goc + Vector3.up * 1.4f;
            Vector3 den = p + Vector3.up * 1.0f;
            // CHI lop do vat Default: MatNaVatCan co ca mat dat, cau 1,5 m o do cao 1,2 m cham doc dat la loai het cho (muc H, J lan dau)
            int lopVat = LayerMask.GetMask("Default");
            if (Physics.SphereCast(tu, 0.8f, (den - tu).normalized, out RaycastHit _, (den - tu).magnitude, lopVat, QueryTriggerInteraction.Ignore)) continue;
            if (Physics.CheckSphere(p + Vector3.up * 1.5f, 1.0f, lopVat, QueryTriggerInteraction.Ignore)) continue;
            cho = p; return true;
        }
        cho = Vector3.zero; return false;
    }

    static Vector3 HuongTrong(PlayerController pc)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 0.6f, h, out RaycastHit _, 14f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    /// <summary>
    /// Theo doi cac qua dia nguc trong 'giay': vi tri cuoi cung moi qua; qua bien mat thi tinh la TRUNG bia nao gan cho no
    /// cuoi nhat (&lt; 1,6 m tinh tu than bia). Tra ve so qua trung moi bia.
    /// </summary>
    static readonly List<string> ChanDoan = new List<string>();

    static IEnumerator TheoDoi(List<Fireball> qua, List<Damageable> bia, float giay, int[] trungBia, Dictionary<Fireball, Damageable> ganLucDau)
    {
        var cuoi = new Dictionary<Fireball, Vector3>();
        foreach (var q in qua) { cuoi[q] = q.transform.position; if (ganLucDau != null) ganLucDau[q] = q.mucTieu; }
        var conLai = new List<Fireball>(qua);
        float han = Time.time + giay;
        while (Time.time < han && conLai.Count > 0)
        {
            foreach (var q in conLai) if (q != null) cuoi[q] = q.transform.position;
            yield return null;
            DemTrung(conLai, cuoi, bia, trungBia);
        }
    }

    /// <summary>Qua vua bien mat (da no): tinh trung bia gan cho cuoi cung cua no nhat (mep than bia &lt; 1,6 m).</summary>
    static void DemTrung(List<Fireball> conLai, Dictionary<Fireball, Vector3> cuoi, List<Damageable> bia, int[] trung)
    {
        for (int i = conLai.Count - 1; i >= 0; i--)
        {
            if (conLai[i] != null) continue;
            Vector3 p = cuoi[conLai[i]];
            int tot = -1; float g = 1.6f;
            for (int b = 0; b < bia.Count; b++)
            {
                if (bia[b] == null) continue;
                var c = bia[b].transform.position;
                float kc = new Vector2(p.x - c.x, p.z - c.z).magnitude - 0.4f;
                if (kc < g) { g = kc; tot = b; }
            }
            if (tot >= 0) trung[tot]++;
            else
            {
                // CHAN DOAN: qua no ma khong gan bia nao - no o dau, canh vat gi
                var ten = new List<string>();
                foreach (var c in Physics.OverlapSphere(p, 1.0f, ~0, QueryTriggerInteraction.Collide))
                    if (!ten.Contains(c.name + "@" + LayerMask.LayerToName(c.gameObject.layer))) ten.Add(c.name + "@" + LayerMask.LayerToName(c.gameObject.layer));
                string gan = "";
                for (int b = 0; b < bia.Count; b++) if (bia[b] != null) gan += string.Format(" bia{0}:{1:F1}m", b, new Vector2(p.x - bia[b].transform.position.x, p.z - bia[b].transform.position.z).magnitude);
                ChanDoan.Add(string.Format("no o ({0:F1},{1:F1},{2:F1}) cao tren dat {3:F2} m;{4}; va cham trong 1 m: {5}",
                    p.x, p.y, p.z, p.y - VfxFactory.GroundY(p), gan, ten.Count == 0 ? "khong" : string.Join(", ", ten.ToArray())));
            }
            conLai.RemoveAt(i);
        }
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han0 = Time.time + 30f;
        while (dir == null && Time.time < han0) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        if (dir != null) dir.enabled = false;
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);
        int maskEnemy = LayerMask.GetMask("Enemy");

        // TAT TAM VA CHAM DO VAT QUANH NHAN VAT (chi trong Play, bat lai cuoi phep thu). Do tren Act2 17/09/2026: KHONG co cho
        // trong ban kinh 11 m nao (bia, da, cay, lo lua day dac), qua uon cong vong rong thi 1/4 qua dam vao lo lua giua map -
        // phep do "tu di trung muc tieu" thanh do mat do vat can. Muc C..H do rieng kha nang di; dam vat can van no la luat
        // cu cua Qua cau lua (khong doi).
        var daTat = new List<Collider>();
        foreach (var c in Physics.OverlapSphere(toi.transform.position, 26f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            if (c.enabled && !c.isTrigger) { c.enabled = false; daTat.Add(c); }
        Physics.SyncTransforms();
        Ghi(string.Format("(chuan bi) tat tam {0} va cham lop Default trong 26 m quanh nhan vat (bat lai cuoi phep thu)", daTat.Count));

        // ================= A. THONG SO =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/LuaDiaNguc");
        var pfLua = GameAssets.I != null ? GameAssets.I.fireballPrefab : null;
        float impactPrefab = pfLua != null ? pfLua.GetComponent<Fireball>().impactDamage : -1f;
        Ghi(string.Format("A. so hieu {0}, so ky nang {1}; nhan vat: nang luong {2}, hoi chieu {3} s, niem {4} s; Sach phep doc {5}/{6}/{7}",
            K, CapDo.SoKyNang, toi.luaDiaNgucCost, toi.luaDiaNgucCooldown, toi.luaDiaNgucCastTime, nl, hc, nc));
        Ghi(string.Format("A. sat thuong goc {0:F2} = qua cau lua PREFAB {1} x 1,2^4 ({2:F2}); ten \"{3}\", tom tat \"{4}\", mo ta {5} ky tu; HUD {6} icon, icon so 11 {7}; icon file {8}",
            LuaDiaNguc.SatThuongGoc, impactPrefab, impactPrefab * Mathf.Pow(1.2f, 4), SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length,
            bo != null ? bo.Length : -1, bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG", tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG"));
        int soCoTen = SachPhep.SoKyNangCoTen();
        Kiem(K == 11 && CapDo.SoKyNang == soCoTen, "so hieu sai, hoac co ky nang khong co ten ("
             + CapDo.SoKyNang + " ky nang nhung " + soCoTen + " cai co ten)");
        Kiem(Mathf.Approximately(toi.luaDiaNgucCost, 31f) && Mathf.Approximately(nl, 31f), "nang luong khong phai 31");
        Kiem(Mathf.Approximately(toi.luaDiaNgucCooldown, 0.5f) && Mathf.Approximately(hc, 0.5f), "hoi chieu khong phai 0,5");
        Kiem(Mathf.Approximately(toi.luaDiaNgucCastTime, 0.38f), "niem khong phai 0,38");
        Kiem(impactPrefab > 0f && Mathf.Abs(LuaDiaNguc.SatThuongGoc - impactPrefab * Mathf.Pow(1.2f, 4)) < 0.01f, "sat thuong goc khong bang qua cau lua cap 5");
        Kiem(SachPhep.Ten(K) == "LỬA ĐỊA NGỤC" && SachPhep.MoTa(K).Length > 100 && SachPhep.MoTa(K).Contains("176"), "Sach phep thieu chu Lua dia nguc");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Lua dia nguc");

        // ================= B. TUNG THAT =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;
        int soPhep = 0, kyVua = -1;
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; kyVua = s; };
        toi.DaTungPhep += dem;
        CapDo.BatDauTranMoi();
        int s0 = soPhep;
        toi.CastAt(K, goc + huong * 8f);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoCaDuongChoPhepThu(K);
        Kiem(CapDo.DaMo(K), "khong mo khoa duoc");
        yield return new WaitForSeconds(0.3f);
        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana;
        toi.CastAt(K, goc + huong * 8f);
        bool daTung = soPhep == s0 + 1 && kyVua == K;
        float manaTon = manaTruoc - toi.mana;
        int sBam = soPhep; float lucTung = Time.time, lucNhan = -1f; int soQuaBay = 0;
        float hanB = Time.time + 1.5f;
        while (Time.time < hanB)
        {
            soQuaBay = Mathf.Max(soQuaBay, QuaDiaNguc().Count);
            toi.CastAt(K, goc + huong * 8f);
            if (soPhep > sBam) { lucNhan = Time.time - lucTung; break; }
            yield return null;
        }
        yield return new WaitForSeconds(0.6f);
        Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> tung {1}, {2} qua dia nguc bay, ton {3} nang luong; bam lai moi khung -> duoc nhan sau {4:F3} s",
            tuChoiKhoa, daTung, soQuaBay, manaTon, lucNhan));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(daTung && soQuaBay == LuaDiaNguc.SoQua && LuaDiaNguc.SoQua == 5, "tung khong ra dung 5 qua");
        Kiem(Mathf.Abs(manaTon - 31f) < 0.01f, "khong ton dung 31 nang luong");
        Kiem(lucNhan >= 0.5f - 0.001f && lucNhan < 0.6f, "hoi chieu 0,5 giay khong dung");
        XoaQua();
        yield return new WaitForSeconds(0.5f);

        Vector3 tuQua = goc + Vector3.up * 1.4f + huong * 0.8f;
        var datDat = new List<Vector3>();

        // ================= C. DI 4 MUC TIEU NGOAI HINH QUAT =================
        Ghi("");
        {
            // Moi bia mot goc phan tu rieng, duong bay (vong cung) toi bia nay khong di ngang bia khac - qua cau lua cham AI tren
            // duong la no ngay (luat cu), lan dau dat -90/-140 do thi qua di bia 14 m no vao bia 10 m.
            float[] lech = { 60f, -60f, 120f, -120f, 180f };
            float[] kcs = { 9f, 9f, 12f, 12f, 10f };
            var bia = new List<Damageable>();
            bool du = true;
            for (int i = 0; i < lech.Length; i++)
            {
                Vector3 cho;
                if (!TimCho(toi, goc, huong, lech[i], kcs[i], datDat, out cho)) { du = false; break; }
                datDat.Add(cho); bia.Add(TaoBia("TAM_C" + i, cho));
            }
            Vector3 choXa;
            bool coXa = TimCho(toi, goc, huong, 100f, 25f, datDat, out choXa);
            Damageable biaXa = coXa ? TaoBia("TAM_CXa", choXa) : null;
            if (!du) { Ghi("[LOI] khong tim du 5 cho trong de dat bia"); loi++; }
            yield return new WaitForFixedUpdate();
            var mau0 = new float[bia.Count]; for (int i = 0; i < bia.Count; i++) mau0[i] = bia[i].health;
            float mauXa0 = biaXa != null ? biaXa.health : 0f;

            // DOI CHUNG: chum Qua cau lua thuong (4 qua, 18 do, khong di)
            var biaVaXa = new List<Damageable>(bia); if (biaXa != null) biaVaXa.Add(biaXa);
            var truocDC = new HashSet<Fireball>(Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude));
            Fireball.SpawnChum(tuQua, huong, toi.MatNaVatCan, maskEnemy, mauToi, LuaDiaNguc.SoQua, LuaDiaNguc.GocToe);
            var quaDC = new List<Fireball>();
            foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude)) if (!truocDC.Contains(f)) quaDC.Add(f);
            var trungDC = new int[biaVaXa.Count];
            yield return TheoDoi(quaDC, biaVaXa, 4.5f, trungDC, null);
            int soBiaDC = 0; foreach (var t in trungDC) if (t > 0) soBiaDC++;
            ChanDoan.Clear();
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < bia.Count; i++) mau0[i] = bia[i].health;
            if (biaXa != null) mauXa0 = biaXa.health;

            // LUA DIA NGUC
            LuaDiaNguc.SpawnChum(tuQua, huong, goc, toi.MatNaVatCan, maskEnemy, mauToi);
            var qua = QuaDiaNguc();
            var gan = new Dictionary<Fireball, Damageable>();
            var trung = new int[biaVaXa.Count];
            // chup khi dang uon cong
            var chay = Object.FindAnyObjectByType<ChayThuMang>();
            yield return TheoDoiVaChup(qua, biaVaXa, trung, gan);
            var ganKhac = new HashSet<Damageable>(); int soGanBiaGan = 0;
            foreach (var kv in gan) { if (kv.Value != null) ganKhac.Add(kv.Value); if (bia.Contains(kv.Value)) soGanBiaGan++; }
            int soBiaTrung = 0; for (int i = 0; i < bia.Count; i++) if (trung[i] > 0) soBiaTrung++;
            yield return new WaitForSeconds(0.3f);
            int soBiaMatMau = 0; for (int i = 0; i < bia.Count; i++) if (mau0[i] - bia[i].health > 50f) soBiaMatMau++;
            float matXa = biaXa != null ? mauXa0 - biaXa.health : -1f;
            Ghi(string.Format("C. 5 bia ngoai hinh quat (lech 60/-60/120/-120/180 do, 9-12 m) + bia 25 m ({0}). DOI CHUNG chum Qua cau lua thuong: so bia trung {1}",
                coXa ? "co" : "KHONG dat duoc", soBiaDC));
            Ghi(string.Format("C. Lua dia nguc: {0} qua; luc sinh gan {1} muc tieu khac nhau ({2} qua gan vao 5 bia gan); qua trung moi bia: {3}; bia mat mau {4}/5; bia 25 m mat {5:F0}",
                qua.Count, ganKhac.Count, soGanBiaGan, string.Join(",", System.Array.ConvertAll(trung, x => x.ToString())), soBiaMatMau, matXa));
            Kiem(soBiaDC == 0, "doi chung: qua cau lua thuong cung trung bia ngoai hinh quat - phep do vo nghia");
            Kiem(qua.Count == 5 && ganKhac.Count == 5 && soGanBiaGan == 5, "5 qua khong chia cho 5 ke gan nhat");
            Kiem(soBiaTrung == 5 && soBiaMatMau == 5, "khong trung du 5 bia ngoai hinh quat");
            Kiem(biaXa == null || matXa < 0.5f, "bia ngoai 20 m van bi di trung");
            foreach (var cd in ChanDoan) Ghi("    (chan doan) " + cd);
            ChanDoan.Clear();
            foreach (var b in biaVaXa) if (b != null) Object.Destroy(b.gameObject);
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= D. MOT BIA - CA 4 QUA =================
        Ghi("");
        {
            Vector3 cho;
            if (TimCho(toi, goc, huong, 100f, 10f, new List<Vector3>(), out cho))
            {
                var b = TaoBia("TAM_D", cho);
                yield return new WaitForFixedUpdate();
                LuaDiaNguc.SpawnChum(tuQua, huong, goc, toi.MatNaVatCan, maskEnemy, mauToi);
                var qua = QuaDiaNguc();
                var trung = new int[1];
                var gan = new Dictionary<Fireball, Damageable>();
                yield return TheoDoi(qua, new List<Damageable> { b }, 4f, trung, gan);
                int ganB = 0; foreach (var kv in gan) if (kv.Value == b) ganB++;
                Ghi(string.Format("D. mot bia lech 100 do 10 m: {0}/5 qua gan vao no, {1}/5 qua trung no", ganB, trung[0]));
                Kiem(ganB == 5 && trung[0] == 5, "it ke dich hon 5 ma qua du khong nham ke gan nhat");
            foreach (var cd in ChanDoan) Ghi("    (chan doan) " + cd);
            ChanDoan.Clear();
                Object.Destroy(b.gameObject);
            }
            else { Ghi("[LOI] D khong tim duoc cho"); loi++; }
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= E. BIA CHAY NGANG =================
        Ghi("");
        {
            Vector3 cho;
            if (TimCho(toi, goc, huong, -100f, 11f, new List<Vector3>(), out cho))
            {
                var b = TaoBia("TAM_E", cho);
                yield return new WaitForFixedUpdate();
                float m0 = b.health;
                Vector3 vt = Vector3.Cross(Vector3.up, (cho - goc).normalized) * 5f;
                LuaDiaNguc.SpawnChum(tuQua, huong, goc, toi.MatNaVatCan, maskEnemy, mauToi);
                var qua = QuaDiaNguc();
                float hanE = Time.time + 3f; int conBay = qua.Count;
                while (Time.time < hanE && conBay > 0)
                {
                    b.transform.position += vt * Time.deltaTime;
                    Physics.SyncTransforms();   // collider khong co Rigidbody: doi transform ma khong dong bo thi va cham van o cho cu
                    conBay = 0; foreach (var q in qua) if (q != null) conBay++;
                    yield return null;
                }
                yield return new WaitForSeconds(0.1f);
                float mat = m0 - b.health;
                Ghi(string.Format("E. bia chay ngang 5 m/s: mat {0:F0} mau ({3} qua ~ {1:F0}-{2:F0}), con qua dang bay {4}", mat,
                    LuaDiaNguc.SoQua * LuaDiaNguc.SatThuongGoc * 0.55f, LuaDiaNguc.SoQua * LuaDiaNguc.SatThuongGoc, LuaDiaNguc.SoQua, conBay));
                Kiem(mat > (LuaDiaNguc.SoQua - 0.5f) * LuaDiaNguc.SatThuongGoc * 0.55f, "bia chay ngang khong bi trung du 5 qua");
                Object.Destroy(b.gameObject);
            }
            else { Ghi("[LOI] E khong tim duoc cho"); loi++; }
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= F. MUC TIEU CHET GIUA DUONG =================
        Ghi("");
        {
            Vector3 choA, choB;
            var dd = new List<Vector3>();
            bool okA = TimCho(toi, goc, huong, 90f, 9f, dd, out choA); dd.Add(choA);
            bool okB = TimCho(toi, goc, huong, -90f, 13f, dd, out choB);
            if (okA && okB)
            {
                var a = TaoBia("TAM_FA", choA); var b = TaoBia("TAM_FB", choB);
                yield return new WaitForFixedUpdate();
                float mB0 = b.health;
                LuaDiaNguc.SpawnChum(tuQua, huong, goc, toi.MatNaVatCan, maskEnemy, mauToi);
                var qua = QuaDiaNguc();
                int ganA = 0; foreach (var q in qua) if (q.mucTieu == a) ganA++;
                yield return new WaitForSeconds(0.15f);
                a.health = 0.5f; a.TakeDamage(10f, DamageType.Physical, a.transform.position);   // giet A giua duong
                bool aChet = a.IsDead;
                var trung = new int[2];
                yield return TheoDoi(qua, new List<Damageable> { a, b }, 4f, trung, null);
                yield return new WaitForSeconds(0.1f);
                Ghi(string.Format("F. bia A (9 m) nhan {0} qua luc sinh, bi giet sau 0,15 s (chet {1}); qua trung B {2}/5, B mat {3:F0}", ganA, aChet, trung[1], mB0 - b.health));
                Kiem(ganA >= 1 && aChet && trung[1] == LuaDiaNguc.SoQua, "muc tieu chet giua duong ma qua khong chuyen sang ke con song");
                Object.Destroy(a.gameObject); Object.Destroy(b.gameObject);
            }
            else { Ghi("[LOI] F khong tim duoc cho"); loi++; }
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= G. KHONG CO AI: BAY THANG TOA QUAT =================
        Ghi("");
        {
            foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude)) if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
            yield return new WaitForFixedUpdate();
            LuaDiaNguc.SpawnChum(tuQua, huong, goc, toi.MatNaVatCan, maskEnemy, mauToi);
            var qua = QuaDiaNguc();
            var h0 = new List<Vector3>(); foreach (var q in qua) h0.Add(q.HuongBay);
            yield return new WaitForSeconds(0.4f);
            float doiMax = 0f; int conSong = 0;
            for (int i = 0; i < qua.Count; i++) if (qua[i] != null) { conSong++; doiMax = Mathf.Max(doiMax, Vector3.Angle(h0[i], qua[i].HuongBay)); }
            var gocs = new List<float>();
            foreach (var h in h0) gocs.Add(Vector3.SignedAngle(huong, new Vector3(h.x, 0, h.z), Vector3.up));
            gocs.Sort();
            float lechMin = 999f, lechMax = 0f;
            for (int i = 1; i < gocs.Count; i++) { lechMin = Mathf.Min(lechMin, gocs[i] - gocs[i - 1]); lechMax = Mathf.Max(lechMax, gocs[i] - gocs[i - 1]); }
            Ghi(string.Format("G. khong ai: {0} qua, sau 0,4 s con {1}, huong doi lon nhat {2:F2} do; goc giua hai qua ke nhau {3:F1}-{4:F1} do", qua.Count, conSong, doiMax, lechMin, lechMax));
            Kiem(qua.Count == 5 && conSong > 0 && doiMax < 0.5f, "khong co ai ma qua van doi huong");
            Kiem(Mathf.Abs(lechMin - 18f) < 0.5f && Mathf.Abs(lechMax - 18f) < 0.5f, "5 qua khong toa quat 18 do");
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= H. SAT THUONG + THIEU DOT + CAP =================
        Ghi("");
        {
            Vector3 cho;
            if (TimCho(toi, goc, huong, 0f, 9f, new List<Vector3>(), out cho))
            {
                // mot qua Qua cau lua CAP 5 that bay thang vao bia
                var b1 = TaoBia("TAM_H1", cho);
                yield return new WaitForFixedUpdate();
                Vector3 toiBia = (cho + Vector3.up * 1.0f - tuQua).normalized;
                float m1 = b1.health;
                Fireball.SpawnChum(tuQua, toiBia, toi.MatNaVatCan, maskEnemy, mauToi, 1, 0f, CapDo.SatThuongTheoCap(5), 0f);
                float matLua = 0f;
                float hanH = Time.time + 2f;
                while (Time.time < hanH) { if (b1.health < m1) { matLua = m1 - b1.health; break; } yield return null; }
                Object.Destroy(b1.gameObject);
                XoaQua();
                yield return new WaitForSeconds(4.5f);   // cho bia cu va lua chay tat han
                // mot qua Lua dia nguc CAP 1, cung cho cung huong (dat 1 qua: goi SpawnChum roi xoa 3 qua)
                var b2 = TaoBia("TAM_H2", cho);
                yield return new WaitForFixedUpdate();
                float m2 = b2.health;
                LuaDiaNguc.SpawnChum(tuQua, toiBia, goc, toi.MatNaVatCan, maskEnemy, mauToi);
                var qua = QuaDiaNguc();
                // giu qua gan huong toiBia nhat (qua giua lech 9 do) - xoa 3 qua con lai
                Fireball giu = null; float nho = 999f;
                foreach (var q in qua) { float g = Vector3.Angle(q.HuongBay, toiBia); if (g < nho) { nho = g; giu = q; } }
                foreach (var q in qua) if (q != giu) Object.DestroyImmediate(q.gameObject);
                float impactDN = giu != null ? giu.impactDamage : -1f;
                float matDN = 0f;
                hanH = Time.time + 2f;
                while (Time.time < hanH) { if (b2.health < m2) { matDN = m2 - b2.health; break; } yield return null; }
                yield return new WaitForSeconds(0.6f);
                bool chay = b2.GetComponent<BurningEffect>() != null;
                float cap3 = LuaDiaNguc.SatThuongGoc * CapDo.SatThuongTheoCap(3);
                Ghi(string.Format("H. mot qua Qua cau lua CAP 5 that trung bia mat {0:F2}; mot qua Lua dia nguc CAP 1 (impactDamage {1:F2}) cung cho mat {2:F2} -> ti le {3:F3}; bia dang bi thieu dot {4}; cap 3 se la {5:F2} (x1,44)",
                    matLua, impactDN, matDN, matLua > 0 ? matDN / matLua : 0f, chay, cap3));
                Kiem(matLua > 0f && Mathf.Abs(matDN / matLua - 1f) < 0.08f, "sat thuong cap 1 khong bang qua cau lua cap 5");
                Kiem(Mathf.Abs(impactDN - LuaDiaNguc.SatThuongGoc) < 0.01f, "impactDamage qua khong dung sat thuong goc");
                Kiem(chay, "bia khong bi thieu dot");
                Object.Destroy(b2.gameObject);
            }
            else { Ghi("[LOI] H khong tim duoc cho"); loi++; }
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= I. MAU GIONG HET QUA CAU LUA =================
        Ghi("");
        {
            var thuong = Fireball.Spawn(tuQua + Vector3.up * 40f, Vector3.up, 0, 0);
            LuaDiaNguc.SpawnChum(tuQua + Vector3.up * 60f, Vector3.up, goc + Vector3.up * 200f, 0, 0, mauToi);
            var qua = QuaDiaNguc();
            System.Func<Fireball, float> xanhLa = f =>
            {
                float tong = 0f; int n = 0;
                foreach (var ps in f.GetComponentsInChildren<ParticleSystem>(true))
                {
                    var c = ps.main.startColor;
                    Color m = c.mode == ParticleSystemGradientMode.TwoColors ? c.colorMax : c.color;
                    if (c.mode == ParticleSystemGradientMode.Gradient || c.mode == ParticleSystemGradientMode.RandomColor) m = c.gradient.Evaluate(0.5f);
                    if (m.r > 0.05f) { tong += m.g / m.r; n++; }
                }
                return n > 0 ? tong / n : -1f;
            };
            float gThuong = xanhLa(thuong), gDN = qua.Count > 0 ? xanhLa(qua[0]) : -1f;
            Ghi(string.Format("I. ti le xanh la / do cua hat: qua cau lua thuong {0:F3}, lua dia nguc {1:F3} -> x{2:F3} (mong 1,000 - nguoi dung 17/09/2026 xin mau giong het)",
                gThuong, gDN, gThuong > 0 ? gDN / gThuong : 0f));
            Kiem(gThuong > 0f && Mathf.Abs(gDN / gThuong - 1f) < 0.001f, "qua Lua dia nguc khong cung mau voi Qua cau lua");
            XoaQua();
            yield return new WaitForSeconds(0.5f);
        }

        // ================= J. QUA MANG + KE DANH =================
        Ghi("");
        TranHienTai.DangChoiMang = true;
        var vuong = Vector3.Cross(Vector3.up, huong).normalized;
        var kia = NguoiChoiKhac.Sinh("uid-luadianguc", "Người ở xa", goc - vuong * 30f);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi kia"); loi++; }
        else
        {
            var goDb = new GameObject("TAM_DongBoLuaDiaNguc");
            var db = goDb.AddComponent<DongBoTran>();
            db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
            KenhTrucTiep.Tao(); KenhTrucTiep.NhanTraLoi("{}");
            var chay = Object.FindAnyObjectByType<ChayThuMang>();
            if (chay != null) chay.StartCoroutine(GiuSong());
            var daGui = new List<string>();
            KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
            yield return new WaitForSeconds(0.6f);
            int maxQua = 0;
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            { chiSo = 1, kyNang = (byte)K, capKyNang = 1, soThuTu = 900, diemNgam = kia.transform.position + huong * 10f })));
            float hanJ = Time.time + 1f;
            while (Time.time < hanJ) { maxQua = Mathf.Max(maxQua, QuaDiaNguc().Count); yield return null; }
            XoaQua();
            yield return new WaitForSeconds(0.6f);
            daGui.Clear();
            Vector3 cho;
            Damageable biaK = null;
            if (TimCho(toi, goc, huong, 0f, 8f, new List<Vector3>(), out cho)) { biaK = TaoBia("TAM_J", cho); biaK.maxHealth = 50f; biaK.health = 50f; }
            yield return new WaitForFixedUpdate();
            toi.mana = toi.maxMana;
            toi.CastAt(K, goc + huong * 8f);
            yield return null; yield return null;
            bool goi11 = false;
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s); GoiTin.MotPhep p;
                if (b != null && GoiTin.LoaiCuaGoi(b) == GoiTin.LoaiKyNang && GoiTin.DocKyNang(b, out p) && p.kyNang == K) goi11 = true;
            }
            yield return new WaitForSeconds(2.5f);
            bool keDanh = biaK != null && biaK.IsDead && biaK.keDanhCuoi == mauToi;
            Ghi(string.Format("    (chan doan) bia J: {0}, chet {1}, mau con {2}, ke danh cuoi {3}", biaK != null ? "co" : "KHONG dat duoc",
                biaK != null && biaK.IsDead, biaK != null ? biaK.health : -1f, biaK != null && biaK.keDanhCuoi != null ? biaK.keDanhCuoi.name : "null"));
            Ghi(string.Format("J. goi ky nang 11 tu nguoi kia -> may minh ra {0} qua (mong 5); minh tung -> goi mang kyNang 11: {1}; bia 50 mau bi giet, ke danh cuoi = nguoi tung: {2}",
                maxQua, goi11, keDanh));
            Kiem(maxQua == 5, "may minh khong phat lai 5 qua cua nguoi kia");
            Kiem(goi11, "goi ky nang khong mang so 11");
            Kiem(keDanh, "giet bang Lua dia nguc khong ghi ke danh");
            if (biaK != null) Object.Destroy(biaK.gameObject);
            Object.DestroyImmediate(goDb);
            if (kia != null) Object.Destroy(kia.gameObject);
        }
        TranHienTai.DangChoiMang = false;
        XoaQua();

        foreach (var c in daTat) if (c != null) c.enabled = true;
        Physics.SyncTransforms();

        // ================================================================
        // K. XUYEN VAT CAN NHO (bia, mo, da) - cay coi va nha van chan
        // ================================================================
        Ghi("");
        Ghi("K. xuyen vat can nho (nguong cao " + Fireball.CaoVatNho + " m, ngang " + Fireball.NgangVatNho + " m)");

        // ---- K1. Phan loai TUNG vat can that trong canh, theo nhom cha ----
        var demNho = new Dictionary<string, int>();
        var demTo = new Dictionary<string, int>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (c.isTrigger || c is CharacterController) continue;
            if (c.GetComponentInParent<Damageable>() != null) continue;       // nguoi / quai / bia thu
            string cha = c.transform.parent != null ? c.transform.parent.name : "(khong cha)";
            var d = Fireball.LaVatNho(c) ? demNho : demTo;
            d.TryGetValue(cha, out int cu); d[cha] = cu + 1;
        }
        var khoa = new List<string>();
        foreach (var k in demNho.Keys) if (!khoa.Contains(k)) khoa.Add(k);
        foreach (var k in demTo.Keys) if (!khoa.Contains(k)) khoa.Add(k);
        khoa.Sort();
        foreach (var k in khoa)
        {
            demNho.TryGetValue(k, out int n); demTo.TryGetValue(k, out int t);
            Ghi("K1. nhom \"" + k + "\": xuyen duoc " + n + ", bi chan " + t);
        }
        // Nhom cha cua Act2: BiaMo / Da phai XUYEN HET; Cay / NhaMo / HangRao / LoLua_Act2 phai CHAN HET
        // (lo lua chan tu 25/09/2026 - nguoi dung: "nha, cay coi, lo lua... van cho va cham no")
        foreach (var k in new[] { "BiaMo", "Da" })
        {
            demTo.TryGetValue(k, out int t); demNho.TryGetValue(k, out int n);
            Kiem(n > 0 && t == 0, "nhom " + k + " co " + t + " cai KHONG xuyen duoc (phai xuyen het)");
        }
        foreach (var k in new[] { "Cay", "NhaMo", "HangRao", "LoLua_Act2" })
        {
            demTo.TryGetValue(k, out int t); demNho.TryGetValue(k, out int n);
            Kiem(t > 0 && n == 0, "nhom " + k + " co " + n + " cai XUYEN QUA duoc (phai chan het)");
        }
        // Mat dat khong bao gio xuyen duoc
        var dat = Object.FindAnyObjectByType<TerrainCollider>();
        Ghi("K1b. mat dat (TerrainCollider) xuyen duoc = " + (dat != null && Fireball.LaVatNho(dat)) + " (phai False)");
        Kiem(dat != null && !Fireball.LaVatNho(dat), "mat dat lai xuyen duoc");

        // ---- K2 / K3. Ban qua THAT vao vat thu tren troi (khong co gi khac gan do) ----
        int lopVatCan = LayerMask.GetMask("Default");
        Vector3 choK = toi.transform.position + Vector3.up * 70f;
        Vector3 hK = Vector3.forward;

        // Vat thu lay dung khoang kich thuoc do duoc trong Act2: bia cao nhat 3,18 m; cay thap nhat 6,54 m;
        // nha mo 4,22 m va rong 4,37 m. Dung so SAT HAI BEN nguong de biet nguong dat dung cho.
        var vatK = new List<GameObject>();

        var biaK2 = new GameObject("TAM_BiaThu");
        biaK2.transform.position = choK;
        biaK2.AddComponent<BoxCollider>().size = new Vector3(1.4f, 3.0f, 1.0f);   // bia to nhat Act2: 3,18 m
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();

        yield return BanThu(choK - hK * 9f, hK, lopVatCan, true, 1.2f);
        bool songK2 = kConSong; float xaK2 = kXaNhat;
        yield return BanThu(choK - hK * 9f, hK, lopVatCan, false, 1.2f);
        bool songK2b = kConSong; float xaK2b = kXaNhat;
        Ghi("K2. bia thu cao 3,0 m: qua Lua dia nguc con song " + songK2 + ", di xa " + xaK2.ToString("F1")
            + " m; DOI CHUNG qua cau lua thuong con song " + songK2b + ", di xa " + xaK2b.ToString("F1") + " m");
        // kXaNhat do TU CHO BAN, ma cho ban cach vat 9 m: "no o vat" nghia la ~9 m, khong phai ~0 m.
        Kiem(songK2 && xaK2 > 12f, "qua Lua dia nguc KHONG xuyen qua duoc cai bia");
        Kiem(!songK2b && xaK2b > 7f && xaK2b < 9.5f,
             "doi chung sai: qua cau lua thuong phai no NGAY O BIA (~9 m), do duoc " + xaK2b.ToString("F1") + " m");
        Object.DestroyImmediate(biaK2);

        var cayK3 = new GameObject("TAM_CayThu");
        cayK3.transform.position = choK;
        var ccK3 = cayK3.AddComponent<CapsuleCollider>(); ccK3.height = 8f; ccK3.radius = 0.6f;  // cay that: 6,54..17,47 m
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return BanThu(choK - hK * 9f, hK, lopVatCan, true, 1.2f);
        bool songK3 = kConSong; float xaK3 = kXaNhat;
        Ghi("K3. cay thu cao 8 m: qua Lua dia nguc con song " + songK3 + ", di xa " + xaK3.ToString("F1")
            + " m (phai no ngay truoc cay, ~8 m tinh tu cho ban)");
        Kiem(!songK3, "qua Lua dia nguc xuyen qua CAY");
        Kiem(xaK3 > 7f && xaK3 < 9.5f, "qua khong no O CAY (cay cach cho ban 9 m)");
        Object.DestroyImmediate(cayK3);

        var nhaK3 = new GameObject("TAM_NhaThu");
        nhaK3.transform.position = choK;
        nhaK3.AddComponent<BoxCollider>().size = new Vector3(5f, 4.5f, 5f);       // nha mo that: cao 4,22-5,15 rong 4,37-6,16
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();
        yield return BanThu(choK - hK * 9f, hK, lopVatCan, true, 1.2f);
        bool songK3b = kConSong; float xaK3b = kXaNhat;
        Ghi("K3b. nha thu 5 x 4,5 x 5 m: qua con song " + songK3b + ", di xa " + xaK3b.ToString("F1") + " m");
        Kiem(!songK3b, "qua Lua dia nguc xuyen qua NHA");
        Object.DestroyImmediate(nhaK3);
        vatK.Clear();

        // ---- K4 / K5. Ban vao BIA THAT va CAY THAT trong nghia dia ----
        Vector3 tuBia, hBia;
        var biaThat = TimVatThu("BiaMo", true, out tuBia, out hBia, toi.MatNaVatCan);
        if (biaThat == null) Ghi("K4. (khong tim duoc bia that nao co duong ban trong - bo qua)");
        else
        {
            yield return BanThu(tuBia, hBia, toi.MatNaVatCan, true, 1.0f);
            bool songK4 = kConSong; float xaK4 = kXaNhat;
            yield return BanThu(tuBia, hBia, toi.MatNaVatCan, false, 1.0f);
            bool songK4b = kConSong; float xaK4b = kXaNhat;
            Ghi("K4. BIA THAT [" + biaThat.name + "] cao " + biaThat.bounds.size.y.ToString("F2")
                + " m: qua Lua dia nguc con song " + songK4 + " di xa " + xaK4.ToString("F1")
                + " m; DOI CHUNG qua thuong con song " + songK4b + " di xa " + xaK4b.ToString("F1") + " m");
            Kiem(songK4 && xaK4 > 12f, "khong xuyen qua duoc cai bia THAT trong nghia dia");
            Kiem(!songK4b && xaK4b < 10f, "doi chung sai: qua cau lua thuong cung qua duoc cai bia that");
        }

        Vector3 tuCay, hCay;
        var cayThat = TimVatThu("Cay", false, out tuCay, out hCay, toi.MatNaVatCan);
        if (cayThat == null) Ghi("K5. (khong tim duoc cay that nao co duong ban trong - bo qua)");
        else
        {
            float kcCay = Vector3.Dot(cayThat.bounds.center - tuCay, hCay);
            yield return BanThu(tuCay, hCay, toi.MatNaVatCan, true, 1.2f);
            Ghi("K5. CAY THAT [" + cayThat.name + "] cao " + cayThat.bounds.size.y.ToString("F2")
                + " m, tam cach cho ban " + kcCay.ToString("F1") + " m: qua con song " + kConSong
                + ", di xa " + kXaNhat.ToString("F1") + " m");
            Kiem(!kConSong, "qua Lua dia nguc xuyen qua CAY THAT trong nghia dia");
            Kiem(kXaNhat <= kcCay + 1.5f, "qua no o dau do PHIA SAU cay chu khong phai o cay");
        }

        // ---- K6. Duong tung THAT: chum Lua dia nguc bat co xuyen, chum Qua cau lua thi khong ----
        XoaQua();
        yield return null;
        Vector3 hK6 = HuongTrong(toi);
        LuaDiaNguc.SpawnChum(toi.transform.position + Vector3.up * 1.4f, hK6, toi.transform.position,
                             toi.MatNaVatCan, maskEnemy, mauToi);
        yield return null;
        int soXuyen = 0, soQuaK6 = 0;
        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude))
            if (f.diaNguc) { soQuaK6++; if (f.xuyenVatNho) soXuyen++; }
        XoaQua();
        yield return null;
        Fireball.SpawnChum(toi.transform.position + Vector3.up * 1.4f, hK6, toi.MatNaVatCan, maskEnemy, mauToi, 3, 11f);
        yield return null;
        int thuongXuyen = 0, soQuaThuongK6 = 0;
        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude))
            if (!f.diaNguc) { soQuaThuongK6++; if (f.xuyenVatNho) thuongXuyen++; }
        XoaQua();
        Ghi("K6. duong tung that: " + soQuaK6 + " qua Lua dia nguc, " + soXuyen + " cai bat xuyen vat nho; "
            + soQuaThuongK6 + " qua cau lua thuong, " + thuongXuyen + " cai bat (phai 0)");
        Kiem(soQuaK6 > 0 && soXuyen == soQuaK6, "khong phai qua Lua dia nguc nao cung bat xuyen vat nho");
        Kiem(thuongXuyen == 0, "Qua cau lua thuong cung bay xuyen bia - nguoi dung chi xin cho Lua dia nguc");

        toi.DaTungPhep -= dem;
        if (dir != null) dir.enabled = true;
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>Nhu TheoDoi nhung chup hai anh khi 4 qua dang uon cong.</summary>
    static IEnumerator TheoDoiVaChup(List<Fireball> qua, List<Damageable> bia, int[] trung, Dictionary<Fireball, Damageable> gan)
    {
        var cuoi = new Dictionary<Fireball, Vector3>();
        foreach (var q in qua) { cuoi[q] = q.transform.position; gan[q] = q.mucTieu; }
        var conLai = new List<Fireball>(qua);
        float batDau = Time.time, han = Time.time + 4.5f;
        bool chup1 = false;
        while (Time.time < han && conLai.Count > 0)
        {
            foreach (var q in conLai) if (q != null) cuoi[q] = q.transform.position;
            if (!chup1 && Time.time - batDau > 0.25f) { chup1 = true; yield return Chup("luadianguc_1_uon_cong"); foreach (var q in conLai) if (q != null) cuoi[q] = q.transform.position; }
            else yield return null;
            DemTrung(conLai, cuoi, bia, trung);
        }
        yield return Chup("luadianguc_2_no");
    }

    // ---- Do cho muc K: ban MOT qua roi xem no di duoc bao xa truoc khi no ----
    static bool kConSong;
    static float kXaNhat;

    /// <summary>
    /// Ban mot qua cau tu <paramref name="tu"/> theo huong <paramref name="h"/>, theo no toi khi no no hoac het gio.
    /// Ghi vao <see cref="kConSong"/> / <see cref="kXaNhat"/> (met di duoc theo huong ban - doc o khung CUOI CUNG
    /// truoc khi qua bien mat, nen no xap xi cho no).
    /// tocQueo = 0: muc nay do VA CHAM, khong do kha nang tu di muc tieu (muc C..G da do roi).
    /// </summary>
    static IEnumerator BanThu(Vector3 tu, Vector3 h, int lopVatCan, bool batXuyen, float giay)
    {
        var q = Fireball.Spawn(tu, h, lopVatCan, 0);
        q.xuyenVatNho = batXuyen;
        q.tocQueo = 0f;
        q.burnSeconds = 0f;
        kXaNhat = 0f;
        float han = Time.time + giay;                   // TRAN thoi gian - vong cho nao cung phai co
        while (q != null && Time.time < han)
        {
            kXaNhat = Vector3.Dot(q.transform.position - tu, h);
            yield return null;
        }
        kConSong = q != null;
        if (q != null) Object.DestroyImmediate(q.gameObject);
    }

    /// <summary>
    /// Tim mot vat can THAT trong nhom cha <paramref name="nhom"/> ma ban vao duoc: doan duong toi no (va qua no,
    /// neu <paramref name="doiDuongSauTrong"/>) khong co vat can TO nao khac chen ngang - khong thi phep do
    /// "xuyen qua bia" thuc ra lai do cai cay dung sau cai bia.
    /// </summary>
    static Collider TimVatThu(string nhom, bool doiDuongSauTrong, out Vector3 tu, out Vector3 huong, int lopVatCan)
    {
        tu = Vector3.zero; huong = Vector3.forward;
        var ds = new List<Collider>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (c.isTrigger || c.transform.parent == null || c.transform.parent.name != nhom) continue;
            if (c.bounds.size.y < 1.2f) continue;                 // qua thap thi qua cau bay ngang khong cham
            ds.Add(c);
        }
        for (int i = 0; i < ds.Count && i < 200; i++)
        {
            var c = ds[i];
            Vector3 tam = c.bounds.center;
            for (int g = 0; g < 8; g++)
            {
                Vector3 h = Quaternion.AngleAxis(g * 45f, Vector3.up) * Vector3.forward;
                Vector3 batDau = tam - h * 9f;
                float daiDo = doiDuongSauTrong ? 15f : 9f;        // bia: doi trong CA phia sau; cay: chi can phia truoc
                bool sach = true, chamChinhNo = false;
                foreach (var hit in Physics.SphereCastAll(batDau, 0.3f, h, daiDo, lopVatCan, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider == c) { chamChinhNo = true; continue; }
                    if (!Fireball.LaVatNho(hit.collider)) { sach = false; break; }
                }
                if (!sach || !chamChinhNo) continue;
                tu = batDau; huong = h; return c;
            }
        }
        return null;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[LuaDiaNguc] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/luadianguc.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_LuaDiNguc", "TAM_DongBoLuaDiaNguc" })
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
