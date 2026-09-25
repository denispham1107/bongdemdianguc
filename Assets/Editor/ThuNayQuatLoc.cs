using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 84): QUA NAY (Qua cau lua, Qua cau bang) + GIO LOC HINH QUAT (nguoi dung 26/09/2026).
///
/// Nguoi dung: "khi danh trung ke thu, neu co ke thu gan ben qua cau se nay qua va gay sat thuong" (chon: nay 1 lan, 100%, 6 m);
/// Gio loc "cap 1-4 danh ra 3 loc cung luc theo hinh quat, cap 5 danh ra 5 loc" (chon: 15 do giua hai loc, nang luong nhu cu,
/// Hoa loc xoay chi hoa loc GIUA).
///
///   A. Qua cau lua: qua that ban vao bia A, bia B SAU A 3,9 m (NGOAI vung no dau nhung A TRONG vung no cua qua nay -> bia A
///      phai KHONG an lai), bia C phia kia cach A 7 m (ngoai 6 m) -> dung 1 qua nay; B mat = A mat (100%); C 0; A chi mat mot lan.
///      DOI CHUNG: bia dung mot minh -> 0 qua nay; qua soLanNay 0 (quai / Lua dia nguc) -> 0 qua nay, B 0.
///      Tung THAT (CastAt 0): moi qua cua nguoi choi mang soLanNay 1.
///   B. Qua cau bang: y het A.
///   C. Gio loc tung THAT: cap 1 ra 3 loc, huong lech -15 / 0 / +15 do so voi huong ngam, cung moc lucTung, dung MOT loc giua
///      (0 do); ton 20 nang luong. Cap 5: 5 loc -30..+30, ton 25.
///   D. Hoa loc xoay sau Gio loc cap 5: dung 1 Loc xoay, o cho loc GIUA; 4 loc kia van la Gio loc.
///
/// Ket qua: PlayTestShots/nay_quat_loc.txt.
/// </summary>
public static class ThuNayQuatLoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/84. Chay thu QUA NAY + GIO LOC HINH QUAT", false, 173)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] qua nay + gio loc hinh quat");
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
        if (GameObject.Find("TAM_NayQuatLoc") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_NayQuatLoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[NayQuatLoc] " + s); }
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

    /// <summary>Theo doi mau cac bia moi khung: lan mat mau DAU TIEN (luong + luc) va so lan mat mau.</summary>
    class TheoDoi
    {
        public Damageable d; public float truoc, lanDau = -1f, lucDau = -1f; public int soLan;
        public TheoDoi(Damageable x) { d = x; truoc = x.health; }
        public void Doc()
        {
            if (d == null) return;
            float mat = truoc - d.health;
            if (mat > 0.01f) { soLan++; if (lanDau < 0f) { lanDau = mat; lucDau = Time.time; } truoc = d.health; }
        }
    }

    /// <summary>Ban MOT qua (lua hoac bang) vao bia A voi soLanNay, doi toi 3 giay, tra ve A / B / C + so qua nay.</summary>
    static IEnumerator BanMotQua(bool laLua, int soLanNay, Vector3 goc, Vector3 huong, bool coBiaPhu,
                                 PlayerController toi, int maskEnemy, System.Action<TheoDoi, TheoDoi, TheoDoi, int> ketQua)
    {
        Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;
        Vector3 choA = goc + huong * 10f;
        var A = TaoBia("TAM_NayA", choA);
        // B phia SAU A 3,9 m: vu no dau o mat truoc A -> cach THAN B (ban kinh 0,4) 3,9 > 3,4 nen B chua trung; qua nay no o mat
        // truoc B -> cach than A 3,1 < 3,4 nen A NAM TRONG vu no thu hai - A phai khong an lai. (Ban dau dat B ngang 3,6 m tinh
        // theo TAM: vung no tinh theo THAN nen B trung ngay vu no dau, khong con ai de nay.)
        Damageable B = coBiaPhu ? TaoBia("TAM_NayB", choA + huong * 3.9f) : null;
        Damageable C = coBiaPhu ? TaoBia("TAM_NayC", choA - ngang * 7f) : null;
        yield return new WaitForFixedUpdate();
        var tA = new TheoDoi(A); var tB = B != null ? new TheoDoi(B) : null; var tC = C != null ? new TheoDoi(C) : null;
        int nay0 = laLua ? Fireball.SoLanNay : QuaCauBang.SoLanNay;
        Vector3 tu = goc + huong * 3f + Vector3.up * 1.2f;
        Vector3 den = A.transform.position + Vector3.up * 1.2f;
        if (laLua)
        {
            var f = Fireball.Spawn(tu, (den - tu).normalized, toi.MatNaVatCan, maskEnemy);
            f.impactDamage = 85f; f.burnSeconds = 0f; f.soLanNay = soLanNay;     // khong chay: nhip dot theo sau lan vao phep dem
            // XUYEN VAT NHO nhu qua THAT cua nguoi choi: ban dau de mac dinh (tat) nen di nhanh SphereCast mot vat - nhanh ay
            // bo qua vat xuat phat BEN TRONG, che mat loi qua nay no ngay tren than ke vua trung (Qua cau bang lo ra)
            f.xuyenVatNho = true;
        }
        else
        {
            var q = QuaCauBang.Spawn(tu, (den - tu).normalized, toi.MatNaVatCan, maskEnemy);
            q.impactDamage = 65f; q.soLanNay = soLanNay;
        }
        QuaCauBang nayB = null; Vector3 nayDau = Vector3.zero, nayCuoi = Vector3.zero; float nayLuc = -1f, nayHet = -1f;
        for (float h = Time.time + 3f; Time.time < h; )
        {
            tA.Doc(); if (tB != null) tB.Doc(); if (tC != null) tC.Doc();
            // (chan doan) duong bay qua nay bang
            if (!laLua)
            {
                if (nayB == null && nayLuc < 0f)
                    foreach (var q in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude))
                        if (q.khongCham != null) { nayB = q; nayDau = q.transform.position; nayLuc = Time.time; }
                if (nayB != null) nayCuoi = nayB.transform.position;
                else if (nayLuc >= 0f && nayHet < 0f) nayHet = Time.time - nayLuc;
            }
            yield return null;
        }
        if (!laLua && nayLuc >= 0f && B != null)
            Ghi(string.Format("   (chan doan qua nay bang) sinh o {0}, tat o {1} sau {2:F2} s; B o {3}; A o {4}; muc tieu {5}",
                nayDau.ToString("F1"), nayCuoi.ToString("F1"), nayHet, B.transform.position.ToString("F1"), A.transform.position.ToString("F1"),
                nayB != null && nayB.mucTieu != null ? nayB.mucTieu.name : "(da tat)"));
        int nay = (laLua ? Fireball.SoLanNay : QuaCauBang.SoLanNay) - nay0;
        ketQua(tA, tB, tC, nay);
        Object.Destroy(A.gameObject); if (B != null) Object.Destroy(B.gameObject); if (C != null) Object.Destroy(C.gameObject);
        yield return new WaitForSeconds(0.4f);
    }

    static IEnumerator ThuMotLoai(bool laLua, Vector3 goc, Vector3 huong, PlayerController toi, int maskEnemy)
    {
        string ten = laLua ? "Qua cau lua" : "Qua cau bang";
        string muc = laLua ? "A" : "B";
        TheoDoi a = null, b = null, c = null; int nay = -1;
        yield return BanMotQua(laLua, 1, goc, huong, true, toi, maskEnemy, (x, y, z, n) => { a = x; b = y; c = z; nay = n; });
        float tiLe = a.lanDau > 0f ? b.lanDau / a.lanDau : 0f;
        Ghi(string.Format("{0}. {1}: A mat {2:F2} ({3} lan), B (sau A 3,9 m) mat {4:F2} ({5} lan, sau A {6:F2} s), C (cach A 7 m) mat {7:F2}; qua nay {8}; B/A = {9:F3}",
            muc, ten, a.lanDau, a.soLan, b.lanDau, b.soLan, b.lucDau >= 0f && a.lucDau >= 0f ? b.lucDau - a.lucDau : -1f, Mathf.Max(0f, c.lanDau), nay, tiLe));
        Kiem(nay == 1, ten + ": trung ke dich ma khong nay dung 1 lan");
        Kiem(a.lanDau > 0f && b.lanDau > 0f && Mathf.Abs(tiLe - 1f) < 0.1f, ten + ": qua nay khong gay ~100% sat thuong");
        Kiem(a.soLan == 1, ten + ": bia A an ca vu no cua qua nay (phai bo qua ke da trung)");
        Kiem(c.soLan == 0, ten + ": nay toi ke ngoai 6 m / nay lan thu hai");

        // DOI CHUNG 1: bia dung mot minh -> khong nay
        yield return BanMotQua(laLua, 1, goc, huong, false, toi, maskEnemy, (x, y, z, n) => { a = x; nay = n; });
        Ghi(string.Format("{0}. DOI CHUNG bia dung mot minh: A mat {1:F2}, qua nay {2}", muc, a.lanDau, nay));
        Kiem(a.lanDau > 0f && nay == 0, ten + ": khong co ai gan ma van nay");
        // DOI CHUNG 2: soLanNay 0 (qua cua quai / Lua dia nguc) -> khong nay du co B
        yield return BanMotQua(laLua, 0, goc, huong, true, toi, maskEnemy, (x, y, z, n) => { a = x; b = y; nay = n; });
        Ghi(string.Format("{0}. DOI CHUNG soLanNay 0: A mat {1:F2}, B mat {2:F2}, qua nay {3}", muc, a.lanDau, Mathf.Max(0f, b.lanDau), nay));
        Kiem(nay == 0 && b.soLan == 0, ten + ": qua khong duoc nay (quai / Lua dia nguc) van nay");
    }

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
        CapDo.BatDauTranMoi();
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;

        // ================= A, B. QUA NAY =================
        Ghi("");
        yield return ThuMotLoai(true, goc, huong, toi, maskEnemy);
        Ghi("");
        yield return ThuMotLoai(false, goc, huong, toi, maskEnemy);

        // Tung THAT: qua cua nguoi choi mang soLanNay 1
        Ghi("");
        foreach (int ky in new[] { 0, CapDo.KyQuaCauBang })
        {
            CapDo.MoCaDuongChoPhepThu(ky);
            var cu = new HashSet<Object>();
            foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude)) cu.Add(f);
            foreach (var q in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude)) cu.Add(q);
            yield return new WaitForSeconds(0.6f);
            toi.mana = toi.maxMana;
            toi.CastAt(ky, goc + huong * 12f);
            int so = 0, mang1 = 0;
            for (float h = Time.time + 1.2f; Time.time < h; )
            {
                foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude))
                    if (cu.Add(f) && f.boQua == mauToi) { so++; if (f.soLanNay == 1) mang1++; }
                foreach (var q in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude))
                    if (cu.Add(q) && q.boQua == mauToi) { so++; if (q.soLanNay == 1) mang1++; }
                yield return null;
            }
            Ghi(string.Format("{0}. tung THAT {1}: {2} qua cua nguoi choi, mang soLanNay 1: {3}", ky == 0 ? "A" : "B", ky == 0 ? "Qua cau lua" : "Qua cau bang", so, mang1));
            Kiem(so >= 3 && mang1 == so, "qua tung that cua nguoi choi khong duoc nay");
            yield return new WaitForSeconds(1.5f);
        }

        // ================= C. GIO LOC HINH QUAT =================
        Ghi("");
        int K = CapDo.KyGioLoc;
        CapDo.MoCaDuongChoPhepThu(K);
        for (int lan = 0; lan < 2; lan++)
        {
            if (lan == 1) { CapDo.ThemDiemChoPhepThu(4); for (int i = 0; i < 4; i++) CapDo.NangCap(K); }
            int cap = CapDo.CapCuaKyNang(K);
            foreach (var g in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) Object.Destroy(g.gameObject);
            yield return new WaitForSeconds(0.8f);
            var cu = new HashSet<GioLoc>(Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude));
            toi.mana = toi.maxMana;
            float mana0 = toi.mana;
            Vector3 aim = goc + huong * 12f;
            toi.CastAt(K, aim);
            float ton = mana0 - toi.mana;
            yield return new WaitForSeconds(0.6f);
            var moi = new List<GioLoc>();
            foreach (var g in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!cu.Contains(g)) moi.Add(g);
            Vector3 huongNgam = aim - toi.transform.position; huongNgam.y = 0f; huongNgam.Normalize();
            var goc2 = new List<float>(); int giua = 0; float gocGiua = 99f; bool cungMoc = true;
            foreach (var g in moi)
            {
                float a = Vector3.SignedAngle(huongNgam, g.dir, Vector3.up);
                goc2.Add(a);
                if (g.laLocGiua) { giua++; gocGiua = a; }
                if (Mathf.Abs(g.lucTung - moi[0].lucTung) > 0.001f) cungMoc = false;
            }
            goc2.Sort();
            var sb = new StringBuilder(); foreach (var a in goc2) sb.Append(a.ToString("F1") + " ");
            int mong = cap >= 5 ? 5 : 3;
            float mongTon = cap >= 5 ? GioLoc.NangLuongCap5 : toi.gioLocCost * CapDo.ManaTheoCap(cap);
            Ghi(string.Format("C. cap {0}: {1} loc (mong {2}); goc so voi huong ngam: {3}; cung moc {4}; loc giua {5} (goc {6:F1}); ton {7:F2} nang luong (mong {8:F2})",
                cap, moi.Count, mong, sb, cungMoc, giua, gocGiua, ton, mongTon));
            bool gocDung = goc2.Count == mong;
            for (int i = 0; gocDung && i < mong; i++) if (Mathf.Abs(goc2[i] - (i - (mong - 1) * 0.5f) * GioLoc.GocQuat) > 1.5f) gocDung = false;
            Kiem(moi.Count == mong, "Gio loc cap " + cap + " khong ra dung " + mong + " loc");
            Kiem(gocDung, "cac loc khong toe dung 15 do");
            Kiem(cungMoc && giua == 1 && Mathf.Abs(gocGiua) < 1.5f, "khong co dung MOT loc giua / khac moc tung");
            Kiem(Mathf.Abs(ton - mongTon) < 0.05f, "nang luong Gio loc khong giu nhu cu");

            // ================= D. HOA LOC XOAY (sau lan cap 5) =================
            if (lan == 1)
            {
                CapDo.MoCaDuongChoPhepThu(CapDo.KyHoaLocXoay);
                Vector3 choGiua = Vector3.zero; foreach (var g in moi) if (g != null && g.laLocGiua) choGiua = g.transform.position;
                var xoayCu = new HashSet<Tornado>(Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude));
                toi.mana = toi.maxMana;
                toi.CastAt(CapDo.KyHoaLocXoay, aim);
                yield return new WaitForSeconds(0.7f);
                int soXoay = 0; float kcGiua = -1f;
                foreach (var t in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude))
                    if (!xoayCu.Contains(t)) { soXoay++; Vector3 v = t.transform.position - choGiua; v.y = 0f; kcGiua = v.magnitude; }
                int conGioLoc = 0; foreach (var g in moi) if (g != null) conGioLoc++;
                Ghi(string.Format("D. Hoa loc xoay: {0} Loc xoay moi (cach cho loc giua luc bam {1:F2} m, loc bay 9,5 m/s trong 0,7 s + niem); con {2} Gio loc", soXoay, kcGiua, conGioLoc));
                Kiem(soXoay == 1, "Hoa loc xoay khong hoa DUNG MOT (loc giua)");
                Kiem(kcGiua >= 0f && kcGiua < 12f && conGioLoc == 4, "Loc xoay khong o cho loc giua / cac loc kia bi hoa theo");
                foreach (var t in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) if (!xoayCu.Contains(t)) Object.Destroy(t.gameObject);
            }
            foreach (var g in moi) if (g != null) Object.Destroy(g.gameObject);
        }

        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude)) if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/nay_quat_loc.txt", bao.ToString());
        var rac = GameObject.Find("TAM_NayQuatLoc");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
