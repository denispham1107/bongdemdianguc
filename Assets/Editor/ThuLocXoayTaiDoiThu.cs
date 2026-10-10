using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 99 - LOC XOAY HIEN NGAY TAI DOI THU (nguoi dung 04/10/2026: "tam danh bang Thien thach; khi danh thay vi di chuyen tu vi tri
/// nguoi choi ve phia truoc thi tu dong xuat hien ngay tai vi tri doi thu trong tam"; chon: doi thu GAN CHO NGAM nhat, khong co ai thi
/// hien tai cho ngam, roi BAM THEO doi thu ay). Vao Play Act2, tat GameDirector, mo khoa Loc xoay, tung THAT bang CastAt:
///   A. tam ngam Loc xoay = tam ngam Thien thach (doc TamNgam(4)); khong phai phep "don thang" nua.
///   B. hai bia trong tam (trai 9 m / phai 13 m) + bia NGOAI tam (24 m) dung sat cho ngam: ngam gan bia phai -> loc hien tai bia phai,
///      ngam gan bia trai -> tai bia trai, ngam sat bia ngoai tam -> KHONG chon no (chon bia trong tam gan cho ngam nhat).
///   C. khong ai trong tam, ngam 30 m -> loc hien tai cho ngam keo ve dung 18 m.
///   D. bia trung loc ngay lap tuc bi cuon (WhirledEffect) trong 0,5 s.
///   E. BAM THEO: loc dat bamTheo = bia KHONG nam trong mat na sat thuong (khong bi cuon), bia dung lech 8 m vuong goc huong tung -> sau
///      1,5 s khoang cach giam ro; DOI CHUNG cung cach dat nhung khong bamTheo -> khoang cach khong giam nhu vay; bia chet -> troi lai
///      theo huong luc tung.
/// Ket qua: PlayTestShots/locxoay_taidoithu.txt.
/// </summary>
public static class ThuLocXoayTaiDoiThu
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    static readonly StringBuilder bao = new StringBuilder();
    static int soLoi;
    const string Ra = "PlayTestShots/locxoay_taidoithu.txt";

    [MenuItem("Diablo 2.5D/99 Loc xoay hien tai doi thu (tam Thien thach, bam theo)", false, 164)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        bao.Length = 0; soLoi = 0; daBatDau = false;
        if (File.Exists(Ra)) File.Delete(Ra);
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
        daBatDau = true;
        var go = new GameObject("TAM_ThuLocXoayDoiThu");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); }
    static void Kiem(bool dung, string loi) { if (!dung) { soLoi++; bao.AppendLine("  LOI: " + loi); } }
    static float Ngang(Vector3 a, Vector3 b) { return new Vector2(a.x - b.x, a.z - b.z).magnitude; }

    static Damageable TaoBia(string ten, Vector3 p, int lop)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = lop;
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 10000000f; d.health = 10000000f;
        return d;
    }

    static Tornado LocMoiNhat(System.Collections.Generic.HashSet<Tornado> cu)
    {
        foreach (var t in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) if (!cu.Contains(t)) return t;
        return null;
    }

    /// <summary>Tung Loc xoay THAT bang CastAt roi cho loc moi xuat hien (qua thoi gian niem).</summary>
    static IEnumerator Tung(PlayerController toi, Vector3 aim, System.Action<Tornado> ra)
    {
        var cu = new System.Collections.Generic.HashSet<Tornado>(Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude));
        toi.mana = toi.maxMana;          // hoi chieu Loc xoay 2 s - cac lan tung cach nhau >= 4,5 s
        toi.CastAt(3, aim);
        Tornado moi = null;
        float han = Time.time + 3f;
        while (moi == null && Time.time < han) { moi = LocMoiNhat(cu); yield return null; }
        ra(moi);
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        CapDo.MoCaDuongChoPhepThu(3);
        int lopDich = LayerMask.NameToLayer("Enemy");
        Vector3 goc = toi.transform.position; goc.y = 0f;
        Vector3 truoc = toi.transform.forward; truoc.y = 0f; truoc.Normalize();
        Vector3 phai = Vector3.Cross(Vector3.up, truoc);

        // ---- A ----
        Ghi(string.Format("A. tam ngam Loc xoay {0} m, Thien thach {1} m; don thang (truot tu nguoi choi) {2}", toi.TamNgam(3), toi.TamNgam(4), toi.DonThang(3)));
        // 09/10/2026: Thien thach len 23 m, nguoi dung chon Loc xoay GIU 18 m (truoc "bang Thien thach") - so viet tay
        Kiem(Mathf.Abs(toi.TamNgam(3) - 18f) < 0.01f && Mathf.Abs(toi.TamNgam(4) - 23f) < 0.01f && !toi.DonThang(3), "tam Loc xoay khong phai 18 m / Thien thach khong phai 23 m / van la phep don thang");

        // ---- B ----
        var biaTrai = TaoBia("TAM_BiaTrai", goc + truoc * 7f - phai * 6f, lopDich);      // ~9,2 m
        var biaPhai = TaoBia("TAM_BiaPhai", goc + truoc * 10f + phai * 8f, lopDich);      // ~12,8 m
        var biaXa = TaoBia("TAM_BiaXa", goc + truoc * 22f + phai * 14f, lopDich);         // ~26 m - ngoai tam
        yield return new WaitForSeconds(0.2f);
        Tornado t1 = null, t2 = null, t3 = null;
        yield return Tung(toi, biaPhai.transform.position + truoc * 1.5f, x => t1 = x);
        float lech1 = t1 != null ? Ngang(t1.transform.position, biaPhai.transform.position) : 99f;
        bool cuon1 = false;
        { float h = Time.time + 0.5f; while (Time.time < h) { if (biaPhai.GetComponent<WhirledEffect>() != null) cuon1 = true; yield return null; } }
        if (t1 != null) Object.Destroy(t1.gameObject);
        yield return new WaitForSeconds(4.5f);   // bia rot xuong, het cuon
        yield return Tung(toi, biaTrai.transform.position - phai * 1.5f, x => t2 = x);
        float lech2 = t2 != null ? Ngang(t2.transform.position, biaTrai.transform.position) : 99f;
        if (t2 != null) Object.Destroy(t2.gameObject);
        yield return new WaitForSeconds(4.5f);
        // ngam SAT bia ngoai tam: cho ngam bi keo ve 18 m; bia ngoai tam khong duoc chon -> loc hien tai bia TRONG tam gan cho ngam nhat
        yield return Tung(toi, biaXa.transform.position, x => t3 = x);
        float kcXa = t3 != null ? Ngang(t3.transform.position, biaXa.transform.position) : -1f;
        float kcPhai3 = t3 != null ? Ngang(t3.transform.position, biaPhai.transform.position) : 99f;
        if (t3 != null) Object.Destroy(t3.gameObject);
        Ghi(string.Format("B. ngam gan bia PHAI: loc hien cach bia {0:F2} m, bia bi cuon trong 0,5 s {1} | ngam gan bia TRAI: cach {2:F2} m | ngam sat bia NGOAI tam (26 m): loc cach bia ay {3:F1} m, cach bia phai (trong tam, gan cho ngam nhat) {4:F2} m",
            lech1, cuon1, lech2, kcXa, kcPhai3));
        Kiem(lech1 < 0.6f && lech2 < 0.6f, "loc khong hien ngay tai doi thu gan cho ngam nhat");
        Kiem(cuon1, "doi thu khong bi cuon ngay khi loc hien");
        Kiem(kcXa > 5f && kcPhai3 < 0.6f, "chon nham doi thu ngoai tam 18 m");
        foreach (var b in new[] { biaTrai, biaPhai, biaXa }) Object.Destroy(b.gameObject);
        yield return new WaitForSeconds(4.5f);

        // ---- C ----
        Tornado t4 = null;
        Vector3 aimXa = goc + truoc * 30f;
        yield return Tung(toi, aimXa, x => t4 = x);
        float kcNguoi = t4 != null ? Ngang(t4.transform.position, toi.transform.position) : -1f;
        float lechHuong = t4 != null ? Vector3.Angle(new Vector3(t4.transform.position.x - toi.transform.position.x, 0f, t4.transform.position.z - toi.transform.position.z), truoc) : 180f;
        if (t4 != null) Object.Destroy(t4.gameObject);
        Ghi(string.Format("C. khong ai trong tam, ngam 30 m: loc hien cach nguoi tung {0:F2} m, lech huong ngam {1:F1} do (mong 18 m, 0 do)", kcNguoi, lechHuong));
        Kiem(Mathf.Abs(kcNguoi - 18f) < 0.5f && lechHuong < 3f, "khong co doi thu ma loc khong hien tai cho ngam (keo ve 18 m)");

        // ---- E: bam theo (bia o lop Default - KHONG nam trong mat na sat thuong nen khong bi cuon) ----
        float[] giam = new float[2]; float lechSauChet = 180f;
        for (int lan = 0; lan < 2; lan++)
        {
            bool bam = lan == 0;
            Vector3 cho = goc + truoc * 14f;
            var t = Tornado.Spawn(cho, truoc, 1 << LayerMask.NameToLayer("Enemy"));
            var bia = TaoBia("TAM_BiaBam", cho + phai * 8f, 2);   // lop Ignore Raycast: ngoai mat na sat thuong va ngoai lop canh vat (Default) bi cuon
            if (bam) t.bamTheo = bia;
            t.duration = 20f;
            yield return new WaitForSeconds(0.1f);
            float kc0 = Ngang(t.transform.position, bia.transform.position);
            yield return new WaitForSeconds(1.5f);
            giam[lan] = kc0 - Ngang(t.transform.position, bia.transform.position);
            if (bam)
            {
                bia.TakeDamage(1e9f, DamageType.Physical, bia.transform.position);
                yield return new WaitForSeconds(0.3f);
                Vector3 a = t.transform.position; yield return new WaitForSeconds(0.6f); Vector3 b = t.transform.position;
                lechSauChet = Vector3.Angle(new Vector3(b.x - a.x, 0f, b.z - a.z), truoc);
            }
            Object.Destroy(t.gameObject); if (bia != null) Object.Destroy(bia.gameObject);
            yield return new WaitForSeconds(0.3f);
        }
        Ghi(string.Format("E. bia lech 8 m vuong goc: co bam theo -> khoang cach giam {0:F2} m trong 1,5 s | DOI CHUNG khong bam {1:F2} m | bia chet -> loc troi lech huong luc tung {2:F0} do (lac lu ngau nhien +-30)",
            giam[0], giam[1], lechSauChet));
        Kiem(giam[0] > 3.5f && giam[0] > giam[1] + 2.5f, "loc khong bam theo doi thu");
        Kiem(lechSauChet < 40f, "doi thu chet ma loc khong troi lai theo huong luc tung");

        // ---- F: BI CUON KHONG DUNG DUOC KY NANG (nguoi dung 10/10/2026; chon: ngat phep dang niem, binh van uong, Toc bien cap 5 cung bi chan)
        {
            var mauToi = toi.GetComponent<Damageable>();
            mauToi.maxHealth = mauToi.health = 5000f;
            CapDo.MoCaDuongChoPhepThu(0);
            CapDo.MoCaDuongChoPhepThu(CapDo.KyTocBien);
            for (int v = 0; v < 40 && CapDo.CapCuaKyNang(CapDo.KyTocBien) < 5; v++) { CapDo.ThemDiemChoPhepThu(1); if (!CapDo.NangCap(CapDo.KyTocBien)) break; }
            System.Func<int> demCau = () => { int n = 0; foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)) if (f.boQua == mauToi) n++; return n; };
            // F1: dang niem Qua cau lua thi bi cuon -> ngat, khong qua nao bay ra
            toi.mana = toi.maxMana;
            int ngat0 = PlayerController.SoLanNgatChieu, cau0 = demCau();
            toi.CastAt(0, toi.transform.position + truoc * 10f);
            bool dangNiem = toi.DangNiemChu;
            var loc = Tornado.Spawn(toi.transform.position + truoc * 0.5f, truoc, 1 << toi.gameObject.layer);
            loc.duration = 8f; loc.moveSpeed = 0f;
            var w = WhirledEffect.Catch(mauToi, loc);
            int maxCau = 0;
            for (float tD = Time.time; Time.time - tD < 1.0f; ) { maxCau = Mathf.Max(maxCau, demCau() - cau0); yield return null; }
            Ghi(string.Format("F1. dang niem Qua cau lua ({0}) bi cuon: ngat {1} lan, qua cau bay ra {2}", dangNiem, PlayerController.SoLanNgatChieu - ngat0, maxCau));
            Kiem(w != null && dangNiem && PlayerController.SoLanNgatChieu - ngat0 == 1 && maxCau == 0, "bi cuon ma phep dang niem khong bi ngat");
            // F2: dang bi cuon: Qua cau lua, Toc bien cap 5 bi tu choi; binh mau van uong duoc
            bool conCuon = toi.GetComponent<WhirledEffect>() != null;
            toi.mana = toi.maxMana; float mana0 = toi.mana; int cau1 = demCau();
            toi.CastAt(0, toi.transform.position + truoc * 10f); string nhacCau = toi.LastMessage;
            yield return new WaitForSeconds(0.5f);
            int cauBay = demCau() - cau1;
            Vector3 vt0 = toi.transform.position;
            toi.CastAt(CapDo.KyTocBien, toi.transform.position + truoc * 8f); string nhacTb = toi.LastMessage;
            yield return null;
            bool coTocBien = toi.GetComponent<WhirledEffect>() == null;
            toi.Cap.ThemBinh(CapDo.KyBinhMau);
            mauToi.health = 1000f; int binh0 = toi.Cap.SoBinh(CapDo.KyBinhMau);
            toi.CastAt(CapDo.KyBinhMau, toi.transform.position);
            yield return null;
            Ghi(string.Format("F2. dang bi cuon {0}: Qua cau lua -> \"{1}\", bay ra {2}, mana {3:F0} -> {4:F0}; Toc bien cap {5} -> \"{6}\", thoat loc {7}; binh mau {8} -> {9}, mau 1000 -> {10:F0}",
                conCuon, nhacCau, cauBay, mana0, toi.mana, CapDo.CapCuaKyNang(CapDo.KyTocBien), nhacTb, coTocBien, binh0, toi.Cap.SoBinh(CapDo.KyBinhMau), mauToi.health));
            Kiem(conCuon && cauBay == 0 && toi.mana >= mana0 - 0.01f && nhacCau == "BẠN ĐANG BỊ LỐC XOÁY CUỐN!", "bi cuon van tung duoc ky nang");
            Kiem(CapDo.CapCuaKyNang(CapDo.KyTocBien) == 5 && !coTocBien && nhacTb == "BẠN ĐANG BỊ LỐC XOÁY CUỐN!", "Toc bien cap 5 van thoat duoc loc");
            Kiem(toi.Cap.SoBinh(CapDo.KyBinhMau) == binh0 - 1 && mauToi.health > 1000f, "bi cuon thi khong uong duoc binh");
            // F3: doi chung - thoat loc thi tung lai duoc
            if (loc != null) Object.Destroy(loc.gameObject);
            var wc = toi.GetComponent<WhirledEffect>(); if (wc != null) Object.Destroy(wc);
            yield return new WaitForSeconds(1.2f);
            toi.mana = toi.maxMana; int cau2 = demCau();
            toi.CastAt(0, toi.transform.position + truoc * 10f);
            int maxCau2 = 0;
            for (float tD = Time.time; Time.time - tD < 1.0f; ) { maxCau2 = Mathf.Max(maxCau2, demCau() - cau2); yield return null; }
            Ghi("F3. doi chung het bi cuon: Qua cau lua bay ra " + maxCau2);
            Kiem(maxCau2 >= 1, "doi chung: thoat loc ma van khong tung duoc");
        }

        Ghi("so loi ghi nhan = " + soLoi);
        File.WriteAllText(Ra, bao.ToString());
        var rac = GameObject.Find("TAM_ThuLocXoayDoiThu");
        if (rac != null) Object.Destroy(rac);
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
        Debug.Log("[ThuLocXoayTaiDoiThu] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
