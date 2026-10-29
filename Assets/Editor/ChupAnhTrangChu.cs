using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 147 - CHUP ANH GIOI THIEU KY NANG CHO TRANG CHU WEB (29/10/2026, nguoi dung: trang chu acquytrolai.web.app co phan gioi thieu
/// Loc xoay, Thien thach (2 tam), Qua cau bang (2 tam), Giut set (6 quai, tia lan), Sam set). Nguoi dung chon BAN DEM; luc dau chon goc
/// dien anh gan, roi gui anh mau va DOI: chup DUNG GOC MAY QUAY GAME 2.5D + giao dien CAM UNG - chup ca man hinh Game kem HUD.
///
/// Vao Play Act2, tat GameDirector (khong sinh dot quai), bat tam nhin + suong chien tranh nhu khi choi that, quai THAT tu kho quai (AI
/// tat, mau 100 000, khong do don) dung thanh cum, nguoi choi tung ky nang THAT bang CastAt o cap 5. May quay theo nguoi choi nhu khi
/// choi, nen moi canh dat nguoi choi o cho P roi dung canh TRUOC MAT may quay (FlatForward) / sang ngang - cach toi da ~8 m de nam trong
/// khung. Moi canh chup mot LOAT khung (co man Game, JPG) de chon khung dep nhat: PlayTestShots/trangchu/{canh}_{i}.jpg, kem
/// trangchu/nhatky.txt (cho dat, so vat bi cuon, lua lan toi dau...). Anh chon xong chep sang web/trangchu/anh/ (HUONG-DAN, "Trang chu web").
/// </summary>
public static class ChupAnhTrangChu
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/trangchu/";
    static readonly StringBuilder nk = new StringBuilder();

    /// <summary>Chi chup cac canh co ten trong day (rong = tat ca). Menu 147b chup lai rieng Giut set + Sam set.</summary>
    static string chiCanh = "";
    static bool Co(string canh) { return chiCanh == "" || chiCanh.Contains(canh); }

    [MenuItem("Diablo 2.5D/147 Chup anh gioi thieu ky nang cho trang chu web", false, 220)]
    public static void Chay() { Chay(""); }

    [MenuItem("Diablo 2.5D/147b Chup lai anh trang chu: Giut set", false, 221)]
    public static void ChayGiatSetSamSet() { Chay("giatset"); }

    static void Chay(string canh)
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ChupAnhTrangChu] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        chiCanh = canh;
        if (canh == "" && Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        if (File.Exists(Thu + "nhatky.txt")) File.Delete(Thu + "nhatky.txt");
        nk.Length = 0;
        daBatDau = false;
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
        var go = new GameObject("TAM_ChupTrangChu");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    // ================================================================
    //  TIEN ICH
    // ================================================================

    static PlayerController toi;
    static CameraRig rig;
    static GameHUD hud;
    static int lopQuai;
    static Vector3 F, R;   // huong truoc / phai cua may quay tren mat dat
    static readonly List<GameObject> quai = new List<GameObject>();

    /// <summary>Chup CA man hinh Game (co HUD OnGUI) - cuoi khung hinh.</summary>
    static IEnumerator Chup(string ten)
    {
        yield return new WaitForEndOfFrame();
        var tx = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(Thu + ten + ".jpg", tx.EncodeToJPG(93));
        Object.Destroy(tx);
    }

    static float Dat(Vector3 p) { return GioLoc.MatDatY(p, VfxFactory.GroundY(p)); }

    /// <summary>Cho trong: khong vat can lop Default trong ban kinh r, trong rao, dat khong qua thap.</summary>
    static readonly List<Transform> dsCay = new List<Transform>();
    static readonly List<Vector3> dsLo = new List<Vector3>();
    static float tranhCay;   // > 0: cho dung phai cach moi goc cay chung nay (tan cay che khung 2.5D)

    static float Ngang(Vector3 a, Vector3 b) { a.y = 0; b.y = 0; return Vector3.Distance(a, b); }

    /// <summary>
    /// Khong tan cay nao CHE khung khi nguoi choi dung o P: may quay 2.5D lui ~8,7 m ve -F va cao ~9,7 m, nen cay o giua may
    /// quay va nguoi choi (va cay co la quanh P) phu kin canh (lan 2, 3 cua menu 147 hong vi vay). Bo qua cay 'troi' (cay cua canh).
    /// </summary>
    static bool KhongChe(Vector3 P, Transform troi)
    {
        Vector3 C = P - F * 8.7f;
        foreach (var c in dsCay)
        {
            if (c == troi) continue;
            bool la = !c.name.Contains("_bare") && !c.name.Contains("dead");
            if (Ngang(c.position, P) < (la ? 10f : 6.5f)) return false;
            if (Ngang(c.position, C) < (la ? 8f : 5f)) return false;
            if (Ngang(c.position, (P + C) * 0.5f) < (la ? 8f : 5f)) return false;
        }
        return true;
    }

    static bool Trong(Vector3 p, float r)
    {
        if (Mathf.Abs(p.x) > 56f || Mathf.Abs(p.z) > 56f) return false;
        foreach (var l in dsLo) if ((new Vector3(l.x - p.x, 0, l.z - p.z)).sqrMagnitude < 16f) return false;
        if (tranhCay > 0f) foreach (var cy in dsCay) if (Ngang(cy.position, p) < tranhCay) return false;
        float y = Dat(p); if (y < -2.5f) return false;
        var c = Physics.OverlapCapsule(new Vector3(p.x, y + r + 0.1f, p.z), new Vector3(p.x, y + 2f, p.z), r, 1 << 0, QueryTriggerInteraction.Ignore);
        foreach (var x in c) if (x.GetComponent<Terrain>() == null && x.GetComponent<TerrainCollider>() == null) return false;
        return true;
    }

    static void DoiCho(Vector3 p, Vector3 huong)
    {
        var cc = toi.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        p.y = Dat(p);
        toi.transform.SetPositionAndRotation(p, Quaternion.LookRotation(new Vector3(huong.x, 0, huong.z)));
        if (cc != null) cc.enabled = true;
    }

    /// <summary>Dua nguoi choi toi P, cho may quay bam theo kip (followSmooth 0,12 s).</summary>
    static IEnumerator ToiCho(Vector3 P)
    {
        DoiCho(P, F);
        yield return new WaitForSeconds(1.2f);
    }

    static readonly MonsterType[] Loai = { MonsterType.Witch, MonsterType.Skeleton, MonsterType.QuyCay, MonsterType.QuyDu };

    static GameObject SinhQuai(int i, Vector3 p, Vector3 nhinVe)
    {
        p.y = Dat(p);
        var q = EnemyFactory.Spawn(Loai[i % Loai.Length], p, null, toi.transform);
        q.name = "TAM_Quai" + quai.Count;
        var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
        var d = q.GetComponent<Damageable>(); d.maxHealth = 100000f; d.health = 100000f; d.tiLeDoDon = 0f;
        Vector3 h = nhinVe - p; h.y = 0; if (h.sqrMagnitude > 0.01f) q.transform.rotation = Quaternion.LookRotation(h);
        quai.Add(q);
        return q;
    }

    static void XoaQuai()
    {
        foreach (var q in quai) if (q != null) Object.Destroy(q);
        quai.Clear();
    }

    /// <summary>Cum quai quanh tam, moi con o cho trong, cach nhau &gt;= 2 m.</summary>
    static void CumQuai(Vector3 tam, int so, float banKinh, Vector3 nhinVe, int gieo)
    {
        var rng = new System.Random(gieo);
        var da = new List<Vector3>();
        for (int thu = 0; thu < 400 && da.Count < so; thu++)
        {
            float g = (float)rng.NextDouble() * Mathf.PI * 2f, r = Mathf.Sqrt((float)rng.NextDouble()) * banKinh;
            var p = tam + new Vector3(Mathf.Cos(g), 0, Mathf.Sin(g)) * r;
            if (!Trong(p, 0.5f)) continue;
            bool gan = false; foreach (var d in da) if ((d - p).sqrMagnitude < 4f) gan = true;
            if (gan) continue;
            da.Add(p);
        }
        for (int i = 0; i < da.Count; i++) SinhQuai(i, da[i], nhinVe);
        nk.AppendLine("   cum quai: " + da.Count + "/" + so + " con quanh " + tam.ToString("F1"));
    }

    static void LenCap5(int ky)
    {
        CapDo.MoCaDuongChoPhepThu(ky);
        for (int i = 0; i < 12 && CapDo.CapCuaKyNang(ky) < 5; i++)
            if (!CapDo.NangCap(ky)) CapDo.ThemDiemChoPhepThu(1);
        nk.AppendLine("   ky " + ky + " cap " + CapDo.CapCuaKyNang(ky));
    }

    static void Tung(int ky, Vector3 ngam)
    {
        toi.mana = toi.maxMana;
        toi.CastAt(ky, ngam);
    }

    /// <summary>Cho dat nguoi choi P sao cho diem dich D nam o (truoc, phai) so voi P; thu xoay quanh neu vuong.</summary>
    static bool ChoNguoiTruoc(Vector3 D, float truoc, float phai, out Vector3 P)
    {
        P = D - F * truoc - R * phai;
        if (Trong(P, 0.6f)) return true;
        Vector3 goc = P;
        for (int i = 1; i <= 12; i++)
        {
            var q = goc + Quaternion.Euler(0, i * 30f, 0) * Vector3.forward * (0.6f + i * 0.15f);
            if (Trong(q, 0.6f)) { P = q; return true; }
        }
        return false;
    }

    /// <summary>Mot cho rong (khong vat can trong r) ma nguoi choi dung duoc o (truoc, phai) phia sau no.</summary>
    static Vector3 ChoRong(int gieo, float r, float truoc, float phai)
    {
        var rng = new System.Random(gieo);
        for (int thu = 0; thu < 3000; thu++)
        {
            var p = new Vector3((float)rng.NextDouble() * 80f - 40f, 0, (float)rng.NextDouble() * 80f - 40f);
            if (!Trong(p, r)) continue;
            Vector3 P; if (ChoNguoiTruoc(p, truoc, phai, out P) && KhongChe(P, null)) return p;
        }
        return Vector3.zero;
    }

    static void DonCanh()
    {
        XoaQuai();
        foreach (var t in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) Object.Destroy(t.gameObject);
    }

    // ================================================================
    //  KICH BAN
    // ================================================================

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        TamNhin.BatTrongPhepThu = true;           // anh giong khi choi that: tam nhin 20 m + suong chien tranh
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        rig = Camera.main.GetComponent<CameraRig>();
        if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
        hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.epCamUng = true;
        lopQuai = LayerMask.NameToLayer("Enemy");
        foreach (int ky in new[] { 2, 3, 4, 6, 9 }) LenCap5(ky);
        yield return new WaitForSeconds(1.5f);
        F = rig != null ? rig.FlatForward : Vector3.forward;
        R = Vector3.Cross(Vector3.up, F).normalized;
        nk.AppendLine("man hinh " + Screen.width + "x" + Screen.height + ", goc " + (rig != null ? rig.CurrentName : "?") + ", F " + F.ToString("F2"));

        var cayGoc = GameObject.Find("World/Cay").transform;
        dsCay.Clear(); foreach (Transform c in cayGoc) dsCay.Add(c);
        dsLo.Clear(); foreach (var l in Object.FindObjectsByType<LoLuaDa>(FindObjectsInactive.Exclude)) dsLo.Add(l.transform.position);
        var biaGoc = GameObject.Find("World/BiaMo").transform;
        var bia = new List<Vector3>(); foreach (Transform b in biaGoc) bia.Add(b.position);

        if (Co("locxoay")) yield return LocXoay(cayGoc, bia);
        if (Co("thienthach")) yield return ThienThachRoi();
        if (Co("caychay")) yield return CayChayThienThach(cayGoc);
        if (Co("caubang")) yield return QuaCauBangCanh();
        if (Co("giatset")) yield return GiatSetCanh();
        if (Co("samset")) yield return SamSetCanh();

        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        var rac = GameObject.Find("TAM_ChupTrangChu");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    // ---- 1. LOC XOAY cuon quai + cay + bia (canh o BEN PHAI nguoi choi, giua chieu cao man hinh) ----
    static IEnumerator LocXoay(Transform cayGoc, List<Vector3> bia)
    {
        nk.AppendLine("== Loc xoay");
        Transform tot = null; int diemTot = -1;
        foreach (Transform c in cayGoc)
        {
            if (!c.name.Contains("_bare")) continue;               // cay tro: khong co tan la che canh
            var p = c.position; if (Mathf.Abs(p.x) > 50f || Mathf.Abs(p.z) > 50f) continue;
            Vector3 P0; if (!ChoNguoiTruoc(p, 0.8f, 5f, out P0) || !KhongChe(P0, c)) continue;
            int n = 0; foreach (var b in bia) if ((b - p).sqrMagnitude < 36f) n++;
            if (n > diemTot) { diemTot = n; tot = c; }
        }
        Vector3 T = tot.position;
        nk.AppendLine("   cay " + tot.name + " " + T.ToString("F1") + ", bia trong 6 m: " + diemTot);
        Vector3 P; ChoNguoiTruoc(T, 0.8f, 5f, out P);
        yield return ToiCho(P);
        CumQuai(T - R * 1.5f, 3, 2.5f, P, 11);
        yield return new WaitForSeconds(0.5f);
        yield return Chup("locxoay_truoc");
        Tung(3, quai.Count > 0 ? quai[0].transform.position : T);
        for (int i = 0; i < 28; i++)
        {
            yield return new WaitForSeconds(0.2f);
            yield return Chup("locxoay_" + i.ToString("00"));
            if (i % 4 == 0) nk.AppendLine("   khung " + i + ": vat bi cuon " + Object.FindObjectsByType<VatTheBiCuon>(FindObjectsInactive.Exclude).Length
                + ", quai bi cuon " + Object.FindObjectsByType<WhirledEffect>(FindObjectsInactive.Exclude).Length);
        }
        DonCanh();
        yield return new WaitForSeconds(1f);
    }

    // ---- 2. THIEN THACH dang roi (cum quai DUOI-phai nguoi choi tren man hinh -> thay ca duong roi) ----
    static IEnumerator ThienThachRoi()
    {
        nk.AppendLine("== Thien thach roi");
        tranhCay = 6f;
        Vector3 M = ChoRong(21, 3f, -2.2f, 1.5f);
        Vector3 P; ChoNguoiTruoc(M, -2.2f, 1.5f, out P);
        tranhCay = 0f;
        yield return ToiCho(P);
        CumQuai(M, 4, 2.6f, P, 22);
        yield return new WaitForSeconds(0.5f);
        Tung(4, M);
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForSeconds(0.03f);
            yield return Chup("thienthach_roi_" + i.ToString("00"));
        }
        yield return new WaitForSeconds(3f);
        DonCanh();
    }

    // ---- 3. Cay bi THIEN THACH dot chay toan cay: goc cay DUOI-phai nguoi choi, ngon vuon len giua man; thien thach ngam PHIA SAU cay
    //      (vung no + vung lua khong che than cay, ban kinh bat lua 4,5 m van toi) ----
    static IEnumerator CayChayThienThach(Transform cayGoc)
    {
        nk.AppendLine("== Cay chay");
        Transform tot = null; float diemTot = -1;
        foreach (Transform c in cayGoc)
        {
            if (c.name.Contains("_bare") || c.name.Contains("dead") || c.name.Contains("thin")) continue;
            var r = c.GetComponent<Renderer>(); if (r == null) continue;
            float cao = r.bounds.size.y; if (cao < 6.5f || cao > 13f) continue;
            var p = c.position; if (Mathf.Abs(p.x) > 48f || Mathf.Abs(p.z) > 48f) continue;
            Vector3 P0; if (!ChoNguoiTruoc(p, -1.8f, 4.2f, out P0) || !KhongChe(P0, c)) continue;
            float diem = 20f - Mathf.Abs(cao - 10f);
            if (diem > diemTot) { diemTot = diem; tot = c; }
        }
        if (tot == null)
        {
            nk.AppendLine("   KHONG co cay co la khong bi che -> bo dieu kien che");
            foreach (Transform c in cayGoc)
            {
                if (c.name.Contains("_bare") || c.name.Contains("dead")) continue;
                var r = c.GetComponent<Renderer>(); if (r == null) continue;
                float cao = r.bounds.size.y; if (cao < 6.5f || cao > 13f) continue;
                Vector3 P0; if (!ChoNguoiTruoc(c.position, -1.8f, 4.2f, out P0)) continue;
                tot = c; break;
            }
        }
        Vector3 T = tot.position; float caoCay = tot.GetComponent<Renderer>().bounds.size.y;
        nk.AppendLine("   cay " + tot.name + " cao " + caoCay.ToString("F1") + " m");
        Vector3 P; ChoNguoiTruoc(T, -1.8f, 4.2f, out P);
        yield return ToiCho(P);
        Tung(4, T + F * 2.8f);
        for (int i = 0; i < 8; i++) { yield return new WaitForSeconds(0.1f); yield return Chup("caychay_roi_" + i.ToString("00")); }
        yield return new WaitForSeconds(1.2f);
        var cc = tot.GetComponent<CayChay>();
        if (cc == null)
        {
            nk.AppendLine("   thien thach khong dot trung cay -> cham lua bang CayChay.Dot");
            cc = CayChay.Dot(tot.gameObject, 1 << lopQuai, 0f);
        }
        float han = Time.time + 30f, moc = 0.4f; int k = 0;
        while (cc != null && cc.TienDoLan < 0.99f && Time.time < han)
        {
            if (cc.TienDoLan >= moc) { yield return Chup("caychay_lan_" + k.ToString("00")); k++; moc += 0.1f; }
            yield return null;
        }
        nk.AppendLine("   lua lan toi " + (cc != null ? cc.TienDoLan.ToString("F2") : "?") + ", thoi gian chay " + (cc != null ? cc.thoiGianChay.ToString("F1") : "?"));
        for (int i = 0; i < 10; i++)
        {
            yield return Chup("caychay_" + i.ToString("00"));
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(4f);
        DonCanh();
    }

    // ---- 4. QUA CAU BANG phong sang PHAI + no thanh tang bang ----
    static IEnumerator QuaCauBangCanh()
    {
        nk.AppendLine("== Qua cau bang");
        tranhCay = 6f;
        Vector3 M = ChoRong(41, 3f, 0.6f, 6.5f);
        Vector3 P; ChoNguoiTruoc(M, 0.6f, 6.5f, out P);
        tranhCay = 0f;
        yield return ToiCho(P);
        CumQuai(M, 5, 2.4f, P, 42);
        yield return new WaitForSeconds(0.5f);
        Tung(9, M + Vector3.up * 1f);
        for (int i = 0; i < 24; i++)
        {
            yield return null;
            yield return Chup("caubang_" + i.ToString("00"));
        }
        for (int i = 24; i < 44; i++)
        {
            yield return new WaitForSeconds(0.1f);
            yield return Chup("caubang_" + i.ToString("00"));
        }
        yield return new WaitForSeconds(3f);
        DonCanh();
    }

    // ---- 5. GIUT SET 6 quai hinh quat BEN PHAI, tia lan (anh chon: lan 4 giatset_0_03 - 4 tia + tia lan, 6 quai trung) ----
    static IEnumerator GiatSetCanh()
    {
        nk.AppendLine("== Giut set");
        tranhCay = 6f;
        Vector3 M = ChoRong(61, 4f, 0.5f, 6f);
        Vector3 P; ChoNguoiTruoc(M, 0.5f, 6f, out P);
        yield return ToiCho(P);
        // 6 quai hinh quat ben phai, cach nhau ~2,6 m (tia lan 5,5 m)
        int dat = 0;
        float[] goc = { 0f, 22f, -22f, 44f, -44f, 11f, -11f, 33f, -33f };
        float[] xa = { 6.5f, 5.5f, 5.5f, 5f, 5f, 8.2f, 8.2f, 7.5f, 7.5f };
        for (int i = 0; i < goc.Length && dat < 6; i++)
        {
            var h = Quaternion.AngleAxis(-goc[i], Vector3.up) * R;
            var p = P + h * xa[i];
            if (!Trong(p, 0.5f)) continue;
            SinhQuai(dat, p, P); dat++;
        }
        tranhCay = 0f;
        nk.AppendLine("   quai: " + dat);
        yield return new WaitForSeconds(0.5f);
        for (int lan = 0; lan < 3; lan++)
        {
            Tung(6, P + R * 6f);
            for (int i = 0; i < 14; i++)
            {
                yield return new WaitForSeconds(0.04f);
                yield return Chup("giatset_" + lan + "_" + i.ToString("00"));
            }
            yield return new WaitForSeconds(1.2f);
        }
        DonCanh();
        yield return new WaitForSeconds(1f);
    }

    // ---- 6. SAM SET danh xuong (vung ben phai nguoi choi) ----
    static IEnumerator SamSetCanh()
    {
        nk.AppendLine("== Sam set");
        tranhCay = 6f;
        Vector3 M = ChoRong(81, 4f, 3f, 3.6f);
        Vector3 P; ChoNguoiTruoc(M, 3f, 3.6f, out P);
        tranhCay = 0f;
        yield return ToiCho(P);
        CumQuai(M, 5, 2.6f, P, 82);
        yield return new WaitForSeconds(0.5f);
        Tung(2, M);
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForSeconds(0.05f);
            yield return Chup("samset_" + i.ToString("00"));
        }
        yield return new WaitForSeconds(1f);
        DonCanh();
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        Debug.Log("[ChupAnhTrangChu] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
