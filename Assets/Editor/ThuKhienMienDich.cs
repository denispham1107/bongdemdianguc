using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 157 - KHIEN: MIEN DICH MOI HIEU UNG, KHONG HOA GIAI, KHONG BAT DUOC KHI DANG DINH, +75 MAU (nguoi dung 01/11/2026).
///   A. Mau cap 1 = 225 o ca ba cho (nhan vat trong canh, prefab, khien bat that bang CastAt).
///   B. Dang bat: 9 hieu ung Apply vao deu bi chan (loc cuon bang Tornado THAT); DOI CHUNG tat khien cung nhan vat: 9/9 dinh.
///   C. Khong hoa giai: choang + chay dinh TRUOC khi bat van con chay; chay van tru vao mau khien.
///   D. Dang choang / dong bang / nga / hat tung / loc cuon: CastAt(Khien) khong bat, khong tru mana; DOI CHUNG het hieu ung thi bat duoc.
///   E. Dinh choang GIUA luc niem: khong ra khien.
///   F. Dung trong Loc xoay that: khong bi cuon, khien mat mau dan (khong bat tu); vo khien roi moi bi cuon.
///   G. KHIEN VO TUNG (02/11/2026): quai trong vom mat DUNG mau khien (225 cap 1, 393,5 cap 5 bat that), ngoai vom 0, chu khien 0; ghi ke danh; PhaVo cung no; Tat() khong no;
///      ban sao chu khien (mau may khac quyet) vo: dung so theo cap tu goi phep.
/// PlayTestShots/khien_mien_dich.txt
/// </summary>
public static class ThuKhienMienDich
{
    const string Ra = "PlayTestShots/khien_mien_dich.txt";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/157 Khien: mien dich hieu ung, khong hoa giai, +75 mau", false, 231)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuKhienMienDich] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        nk.Length = 0; loi = 0; daBatDau = false;
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
        var go = new GameObject("TAM_ThuKhienMienDich");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static PlayerController toi;
    static Tornado loc;

    static void GoSach()
    {
        foreach (var w in toi.GetComponents<WhirledEffect>()) Object.DestroyImmediate(w);
        foreach (var c in toi.GetComponents<StunnedEffect>()) Object.DestroyImmediate(c);
        foreach (var c in toi.GetComponents<FrozenEffect>()) Object.DestroyImmediate(c);
        foreach (var c in toi.GetComponents<BiDanhNga>()) Object.DestroyImmediate(c);
        foreach (var c in toi.GetComponents<BiHatTung>()) Object.DestroyImmediate(c);
        foreach (var c in toi.GetComponents<BurningEffect>()) Object.DestroyImmediate(c);
        foreach (var c in toi.GetComponents<BiUot>()) Object.DestroyImmediate(c);
        foreach (var c in toi.GetComponents<ChayDenToanThan>()) Object.DestroyImmediate(c);
        var cc = toi.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;
    }

    /// <summary>Goi lan luot 8 Apply (loc cuon di rieng), dem bao nhieu con hieu luc tren nhan vat.</summary>
    static int ApDu8(Damageable d)
    {
        StunnedEffect.Apply(d, 3f);
        FrozenEffect.Apply(d, 3f);
        FrozenEffect.ApCham(d, 0.5f, 3f);
        BiDanhNga.Apply(d, 3f);
        BiHatTung.Apply(d, 3f);
        BurningEffect.Apply(d, 5f, 3f);
        BiUot.Apply(d, 3f);
        ChayDenToanThan.Apply(d, 3f);
        int n = 0;
        if (d.GetComponent<StunnedEffect>() != null) n++;
        if (d.GetComponent<FrozenEffect>() != null) n += 2;     // Apply + ApCham cung mot component
        if (d.GetComponent<BiDanhNga>() != null) n++;
        if (d.GetComponent<BiHatTung>() != null) n++;
        if (d.GetComponent<BurningEffect>() != null) n++;
        if (d.GetComponent<BiUot>() != null) n++;
        if (d.GetComponent<ChayDenToanThan>() != null) n++;
        return n;
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var d = toi.health;
        d.maxHealth = 100000f; d.health = 100000f;
        toi.maxMana = 100000f; toi.mana = 100000f;
        toi.khiengCooldown = 0.05f;
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(5);
        yield return null;

        // ---- A ----
        nk.AppendLine("== A Mau khien");
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Sorceress.prefab");
        Kiem(toi.khiengMau == 225f, "nhan vat trong canh khiengMau = " + toi.khiengMau);
        Kiem(pf.GetComponent<PlayerController>().khiengMau == 225f, "prefab khiengMau = " + pf.GetComponent<PlayerController>().khiengMau);
        toi.CastAt(5, toi.transform.position);
        yield return new WaitForSeconds(toi.khiengCastTime + 0.2f);
        var k = toi.GetComponent<Khieng>();
        Kiem(k != null && k.DangBat && Mathf.Abs(k.mauToiDa - 225f) < 0.01f, "CastAt that: khien " + (k != null ? k.mau + " / " + k.mauToiDa : "khong co") + " (phai 225)");

        // ---- B ----
        nk.AppendLine("== B Dang bat khien: mien dich");
        int chan0 = Khieng.SoLanChanHieuUng;
        int coKhien = ApDu8(d);
        Kiem(coKhien == 0, "co khien: " + coKhien + " hieu ung dinh (phai 0), chan " + (Khieng.SoLanChanHieuUng - chan0));
        k.Tat();
        yield return null;
        int khongKhien = ApDu8(d);
        Kiem(khongKhien >= 7, "DOI CHUNG khong khien: " + khongKhien + " hieu ung dinh");
        GoSach();
        yield return new WaitForSeconds(0.3f);

        // ---- C ----
        nk.AppendLine("== C Khong hoa giai");
        StunnedEffect.Apply(d, 2f);
        BurningEffect.Apply(d, 50f, 2f);
        k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); d.khieng = k;
        yield return new WaitForSeconds(0.5f);
        var st = toi.GetComponent<StunnedEffect>();
        Kiem(st != null && st.IsStunned, "choang dinh truoc khi bat khien van con sau 0,5 s");
        Kiem(toi.GetComponent<BurningEffect>() != null, "chay dinh truoc khi bat van con");
        Kiem(k.mau < 225f, "chay van tru vao khien: mau " + k.mau.ToString("F1"));
        k.Tat(); GoSach();
        yield return new WaitForSeconds(0.3f);

        // ---- D ----
        nk.AppendLine("== D Dang dinh hieu ung thi khong bat duoc");
        string[] ten = { "choang", "dong bang", "nga", "hat tung", "loc cuon" };
        Vector3 huong = toi.transform.forward;
        for (int i = 0; i < ten.Length; i++)
        {
            if (i == 0) StunnedEffect.Apply(d, 2f);
            else if (i == 1) FrozenEffect.Apply(d, 2f);
            else if (i == 2) BiDanhNga.Apply(d, 2f);
            else if (i == 3) BiHatTung.Apply(d, 1.2f);
            else
            {
                loc = Tornado.Spawn(toi.transform.position + huong * 0.5f, huong, LayerMask.GetMask("Player"));
                loc.boQua = null;
                float han = Time.time + 2f;
                while (toi.GetComponent<WhirledEffect>() == null && Time.time < han) yield return null;
            }
            yield return null;
            float mana0 = toi.mana;
            toi.CastAt(5, toi.transform.position);
            bool niem = toi.DangNiemChu;
            yield return new WaitForSeconds(toi.khiengCastTime + 0.1f);
            var kk = toi.GetComponent<Khieng>();
            // Mana tu hoi trong luc cho nen chi kiem KHONG BI TRU (khong so bang)
            Kiem(!niem && (kk == null || !kk.DangBat) && toi.mana >= mana0, ten[i] + ": niem " + niem + ", khien " + (kk != null && kk.DangBat) + ", mana " + mana0.ToString("F0") + " -> " + toi.mana.ToString("F0"));
            if (loc != null) { Object.Destroy(loc.gameObject); loc = null; }
            GoSach();
            yield return new WaitForSeconds(0.2f);
        }
        toi.mana = toi.maxMana;
        toi.CastAt(5, toi.transform.position);
        yield return new WaitForSeconds(toi.khiengCastTime + 0.15f);
        k = toi.GetComponent<Khieng>();
        Kiem(k != null && k.DangBat, "DOI CHUNG het hieu ung: bat duoc khien");
        if (k != null) k.Tat();
        yield return new WaitForSeconds(0.2f);

        // ---- E ----
        nk.AppendLine("== E Dinh choang giua luc niem");
        toi.CastAt(5, toi.transform.position);
        yield return new WaitForSeconds(toi.khiengCastTime * 0.4f);
        StunnedEffect.Apply(d, 2f);
        yield return new WaitForSeconds(toi.khiengCastTime);
        k = toi.GetComponent<Khieng>();
        Kiem(k == null || !k.DangBat, "choang giua luc niem: khong ra khien");
        GoSach();
        yield return new WaitForSeconds(0.2f);

        // ---- F ----
        nk.AppendLine("== F Dung trong Loc xoay that");
        k = Khieng.Bat(toi.gameObject, 60f, toi.khiengBanKinh); d.khieng = k;
        loc = Tornado.Spawn(toi.transform.position + huong * 0.5f, huong, LayerMask.GetMask("Player"));
        float mauK0 = k.mau, t0 = Time.time;
        bool cuonKhiCoKhien = false;
        while (k.DangBat && Time.time < t0 + 5f)
        {
            if (toi.GetComponent<WhirledEffect>() != null) cuonKhiCoKhien = true;
            loc.transform.position = toi.transform.position + huong * 0.5f;     // giu loc o sat de do nhip
            yield return null;
        }
        Kiem(!cuonKhiCoKhien, "co khien: khong bi cuon");
        Kiem(!k.DangBat, "khien mat mau trong loc: " + mauK0 + " -> het sau " + (Time.time - t0).ToString("F2") + " s (" + loc.damagePerSecond + " mau/giay)");
        float hanC = Time.time + 1.5f;
        while (toi.GetComponent<WhirledEffect>() == null && Time.time < hanC) { if (loc != null) loc.transform.position = toi.transform.position + huong * 0.5f; yield return null; }
        Kiem(toi.GetComponent<WhirledEffect>() != null, "vo khien roi thi bi cuon");
        if (loc != null) Object.Destroy(loc.gameObject);
        GoSach();
        yield return new WaitForSeconds(0.5f);

        // ---- G ----
        nk.AppendLine("== G Khien vo tung");
        {
            Vector3 goc = toi.transform.position;
            Damageable SinhQuai(Vector3 p, string ten)
            {
                p.y = VfxFactory.GroundY(p);
                var go = EnemyFactory.Spawn(MonsterType.QuyCay, p, null, toi.transform);
                go.name = ten;
                go.GetComponent<EnemyAI>().enabled = false;
                var m = go.GetComponent<Damageable>();
                m.tiLeDoDon = 0f; m.maxHealth = 100000f; m.health = 100000f;
                return m;
            }
            var trong = SinhQuai(goc + huong * 2f, "TAM_QuaiTrong");
            var ngoai = SinhQuai(goc + huong * 6f, "TAM_QuaiNgoai");
            yield return null;
            Physics.SyncTransforms();

            // Cap 1, het mau that
            k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); d.khieng = k;
            float mTr = trong.health, mNg = ngoai.health, mCh = d.health;
            int no0 = Khieng.SoLanNo;
            d.TakeDamage(300f, DamageType.Physical, goc);
            yield return null;
            Kiem(!k.DangBat && Khieng.SoLanNo == no0 + 1, "het mau: khien vo, no " + (Khieng.SoLanNo - no0));
            Kiem(Mathf.Abs(mTr - trong.health - 225f) < 0.5f, "quai trong vom mat " + (mTr - trong.health).ToString("F1") + " (phai 225)");
            Kiem(Mathf.Abs(mNg - ngoai.health) < 0.01f, "quai ngoai vom mat " + (mNg - ngoai.health).ToString("F1"));
            Kiem(Mathf.Abs(mCh - d.health) < 0.01f, "chu khien mat " + (mCh - d.health).ToString("F1") + " (don pha vo khien khong tran sang)");
            Kiem(trong.keDanhCuoi == d, "ke danh ghi la chu khien");

            // Cap 5: bat that bang CastAt
            for (int lan = 0; lan < 4; lan++) { CapDo.ThemDiemChoPhepThu(1); CapDo.NangCap(5); }
            toi.mana = toi.maxMana;
            yield return new WaitForSeconds(0.1f);
            toi.CastAt(5, goc);
            yield return new WaitForSeconds(toi.khiengCastTime + 0.15f);
            k = toi.GetComponent<Khieng>();
            float mong5 = 225f * CapDo.MauKhiengTheoCap(5);
            Kiem(k != null && k.DangBat && Mathf.Abs(k.mauToiDa - mong5) < 0.5f, "cap 5 bat that: mau " + (k != null ? k.mauToiDa.ToString("F1") : "?") + " (phai " + mong5.ToString("F1") + ")");
            mTr = trong.health;
            k.PhaVo();      // duong Bo xuong xuyen
            yield return null;
            Kiem(Mathf.Abs(mTr - trong.health - mong5) < 0.5f, "PhaVo cap 5: quai mat " + (mTr - trong.health).ToString("F1"));

            // Tat khong no
            k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); d.khieng = k;
            mTr = trong.health; no0 = Khieng.SoLanNo;
            k.Tat();
            yield return null;
            Kiem(Khieng.SoLanNo == no0 && Mathf.Abs(mTr - trong.health) < 0.01f, "Tat(): khong no");

            // Ban sao chu khien (nguoi khac): dung so theo cap tu goi phep, qua duong goi trang thai that
            var banSao = Object.Instantiate(pf, goc + huong * 3.5f, Quaternion.identity);
            banSao.name = "TAM_BanSaoKhien";
            var pcS = banSao.GetComponent<PlayerController>();
            pcS.tuDocInput = false;
            var mS = banSao.GetComponent<Damageable>();
            mS.mauDoMayKhacQuyet = true;
            yield return null;
            pcS.capKhiengTuMang = 3;
            HieuUngQuaMang.ApKhieng(pcS, 1f);
            var kS = banSao.GetComponent<Khieng>();
            float mong3 = pcS.khiengMau * CapDo.MauKhiengTheoCap(3);
            Kiem(kS != null && kS.DangBat && Mathf.Abs(kS.mauToiDa - mong3) < 0.5f, "ban sao cap 3: mau " + (kS != null ? kS.mauToiDa.ToString("F1") : "?") + " (phai " + mong3.ToString("F1") + ")");
            Physics.SyncTransforms();
            mTr = trong.health;
            HieuUngQuaMang.ApKhieng(pcS, 0f);
            yield return null;
            Kiem(Mathf.Abs(mTr - trong.health - mong3) < 0.5f, "ban sao vo: quai mat " + (mTr - trong.health).ToString("F1") + " (phai " + mong3.ToString("F1") + ")");
            Object.Destroy(banSao);
            Object.Destroy(trong.gameObject);
            Object.Destroy(ngoai.gameObject);
            yield return new WaitForSeconds(0.2f);
        }

        nk.Insert(0, "MENU 157 - Khien mien dich: " + loi + " loi\n");
        File.WriteAllText(Ra, nk.ToString());
        Debug.Log("[ThuKhienMienDich] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuKhienMienDich");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
    }
}
