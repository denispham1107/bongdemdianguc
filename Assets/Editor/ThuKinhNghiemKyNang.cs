using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: GIET BANG KY NANG NAO CUNG PHAI DUOC KINH NGHIEM.
///
/// Nguoi dung bao (13/09/2026, "loi rat nghiem trong"): giet quai hoac nguoi
/// choi bang Mua bang va Sam set thi khong duoc diem kinh nghiem.
///
/// Kinh nghiem va bang diem cuoi tran deu doc <c>Damageable.keDanhCuoi</c> -
/// "ai danh don cuoi". Truong ay chi duoc ghi o nhung cho goi GhiKeDanh; duong
/// sat thuong nao quen goi thi ke giet thanh VO DANH va khong ai duoc gi.
///
/// Phep thu nay KHONG doc code de doan duong nao sot. No:
///   A. Tung TUNG KY NANG THAT tu nhan vat vao mot con quai that dung yen, mau
///      1, va do: con quai co chet, "ke danh cuoi" co phai nhan vat, kinh
///      nghiem co tang dung gia con quai.
///   B. Tach rieng cac duong gay chet CHAM (khong phai don truc tiep): chay
///      theo thoi gian, bi loc cuon, vung lua cua thien thach - vi o phan A
///      don truc tiep da ghi ke danh truoc, che mat loi cua duong cham.
///
/// Ket qua ghi ra <c>PlayTestShots/kinhnghiem_kynang.txt</c>.
/// </summary>
public static class ThuKinhNghiemKyNang
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/61. Chay thu KINH NGHIEM theo tung ky nang", false, 152)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu kinh nghiem",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] giet bang ky nang nao cung phai duoc kinh nghiem");

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
        if (GameObject.Find("TAM_KnKyNang") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_KnKyNang");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[KnKyNang] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    // ================================================================
    //  TIEN ICH
    // ================================================================

    /// <summary>Tong kinh nghiem da nhan trong tran, tinh tu cap + phan le.</summary>
    static int TongKn()
    {
        int t = CapDo.KinhNghiem;
        for (int c = 1; c < CapDo.Cap; c++) t += CapDo.CanDeLenCap(c);
        return t;
    }

    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            return pc;
        }
        return null;
    }

    static List<Damageable> QuaiSong()
    {
        var ds = new List<Damageable>();
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            var d = n.GetComponent<Damageable>();
            if (d != null && !d.IsDead) ds.Add(d);
        }
        return ds;
    }

    /// <summary>Dua mot con quai toi cho, TAT nao va dong bo vat ly ngay.</summary>
    static void DatQuai(Damageable q, Vector3 p)
    {
        var ai = q.GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;
        p.y = VfxFactory.GroundY(p);
        q.transform.position = p;
        Physics.SyncTransforms();
    }

    /// <summary>
    /// Huong TRONG truoc mat nhan vat: qua cau lua va giut set bay thang, vuong
    /// mot tam bia la truot va phep thu bao "khong chet" oan.
    /// </summary>
    static Vector3 HuongTrong(Vector3 goc, float xa)
    {
        for (int i = 0; i < 24; i++)
        {
            float a = i * 15f * Mathf.Deg2Rad;
            var h = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            bool vuong = Physics.SphereCast(goc + Vector3.up * 1.2f, 0.6f, h, out RaycastHit hit, xa + 2f,
                                            LayerMask.GetMask("Default", "Ground"), QueryTriggerInteraction.Ignore);
            if (!vuong) return h;
        }
        return Vector3.forward;
    }

    static void DonPhepTonDong()
    {
        foreach (var x in Object.FindObjectsByType<Tornado>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<IceStorm>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<LightningStorm>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<LightningStrike>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)) Object.Destroy(x.gameObject);
    }

    static float TenKn(Damageable q)
    {
        var n = q.GetComponent<NhanDangQuai>();
        return CapDo.KnCuaQuai(n != null ? n.loai : MonsterType.Skeleton);
    }

    static string TenLoai(Damageable q)
    {
        var n = q.GetComponent<NhanDangQuai>();
        return n != null ? n.loai.ToString() : "?";
    }

    // ================================================================
    //  KICH BAN
    // ================================================================

    static readonly string[] TenKyNang = { "Qua cau lua", "Mua bang", "Sam set", "Loc xoay", "Thien thach", "Khien", "Giut set" };

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }
        yield return new WaitForSeconds(2f);

        var pc = TimToi();
        if (pc == null) { Loi("khong tim thay nguoi choi"); Ket(); yield break; }
        var toi = pc.GetComponent<Damageable>();
        toi.maxHealth = 10000000f; toi.health = 10000000f;

        // Cap 7 = 7 diem ky nang, du mo sau ky nang gay sat thuong, va con xa
        // cap 10 (cap toi da thi kinh nghiem ngung dem - phep do se ra 0 oan).
        CapDo.BatDauTranMoi();
        CapDo.Them(100 + 135 + 180 + 245 + 330 + 445);
        foreach (int k in new[] { 0, 1, 2, 3, 4, 6 }) CapDo.MoKhoa(k);
        Ghi("chuan bi: cap " + CapDo.Cap + ", mo khoa " + CapDo.DaMo(0) + CapDo.DaMo(1) + CapDo.DaMo(2)
            + CapDo.DaMo(3) + CapDo.DaMo(4) + CapDo.DaMo(6));

        // Moi con quai dang co: tat nao, don ra xa - khong de chung chen vao phep do
        if (QuaiSong().Count < 8) { dir.SinhDotQuanhNguoi(); yield return new WaitForSeconds(1f); }
        if (QuaiSong().Count < 8) { dir.SinhDotQuanhNguoi(); yield return new WaitForSeconds(1f); }
        var kho = QuaiSong();
        for (int i = 0; i < kho.Count; i++)
            DatQuai(kho[i], pc.transform.position + new Vector3(60f + i * 3f, 0f, 60f));
        Ghi("quai dung de thu: " + kho.Count + " con");
        yield return null;

        // ================================================================
        // A. TUNG KY NANG THAT
        // ================================================================
        Ghi("");
        Ghi("A. tung tung ky nang that vao mot con quai mau 1");

        int dung = 0;
        foreach (int k in new[] { 0, 1, 2, 3, 4, 6 })
        {
            if (dung >= kho.Count) { Loi("het quai de thu"); break; }
            var q = kho[dung++];

            // DON SACH phep cua lan truoc. Lan chay dau tien, con quai cua Giut
            // set dung dung cho vung lua cua Thien thach con dang chay - no chet
            // vi lua truoc khi tia set toi, va Giut set bi bao loi oan.
            DonPhepTonDong();
            yield return null;

            Vector3 goc = pc.transform.position;
            Vector3 huong = HuongTrong(goc, 7f);
            Vector3 cho = goc + huong * 7f;
            DatQuai(q, cho);
            q.keDanhCuoi = null;
            q.maxHealth = 1f; q.health = 1f;
            pc.transform.rotation = Quaternion.LookRotation(huong);
            Physics.SyncTransforms();
            yield return null;

            int knTruoc = TongKn();
            float gia = TenKn(q);
            string loai = TenLoai(q);          // doc TRUOC: quai dong bang chet la vo tan, bi Destroy
            pc.mana = pc.maxMana;
            pc.CastAt(k, q.transform.position);

            // IsDead va keDanhCuoi la truong C# thuong - van doc duoc sau khi
            // Unity da Destroy vat the (so sanh "q == null" thi lai ra true).
            float hanK = Time.time + 9f;
            while (!q.IsDead && Time.time < hanK) yield return null;
            yield return new WaitForSeconds(0.3f);

            bool chet = q.IsDead;
            bool keDung = ReferenceEquals(q.keDanhCuoi, toi);
            int duoc = TongKn() - knTruoc;

            Ghi("A" + k + ". " + TenKyNang[k] + " -> " + loai + ": chet " + chet
                + ", ke danh cuoi la minh " + keDung + ", kinh nghiem +" + duoc + " (gia " + gia + ")");
            if (!chet) { Loi(TenKyNang[k] + ": phep khong giet duoc quai (khong phai loi kinh nghiem - phep do hong)"); continue; }
            Kiem(keDung, TenKyNang[k] + ": giet xong ma 'ke danh cuoi' khong phai nguoi tung");
            Kiem(duoc == (int)gia, TenKyNang[k] + ": giet xong ma khong duoc dung kinh nghiem");

            // doi het phep dang bay/roi truoc khi thu ky nang sau
            yield return new WaitForSeconds(1.2f);
        }

        // ================================================================
        // B. DUONG GAY CHET CHAM - tach khoi don truc tiep
        // ================================================================
        Ghi("");
        Ghi("B. chet vi hieu ung cham (khong co don truc tiep ghi ke danh truoc)");

        int maskQuai = LayerMask.GetMask("Enemy");

        // ---- B1. Chay theo thoi gian ----
        if (dung < kho.Count)
        {
            var q = kho[dung++];
            DatQuai(q, pc.transform.position + new Vector3(0f, 0f, 30f));
            q.maxHealth = 1000f; q.health = 1000f;
            yield return null;
            // Don nho de gan lua (va ghi ke danh), roi XOA ke danh: chet luc sau
            // la vi CHAY, va chay phai tu nho ai gay ra.
            CombatUtil.AreaDamage(q.transform.position + Vector3.up, 1.5f, 1f, maskQuai,
                                  DamageType.Fire, 6f, toi);
            var chay = q.GetComponent<BurningEffect>();
            q.keDanhCuoi = null;
            q.health = 3f;
            float hanB = Time.time + 6f;
            while (!q.IsDead && Time.time < hanB) yield return null;
            Ghi("B1. chet vi chay: co bi chay " + (chay != null) + ", chet " + q.IsDead
                + ", ke danh cuoi la minh " + ReferenceEquals(q.keDanhCuoi, toi));
            Kiem(chay != null && q.IsDead, "phep do chay khong dung duoc canh");
            Kiem(ReferenceEquals(q.keDanhCuoi, toi), "chet vi CHAY thi ke danh cuoi bi mat - khong ai duoc kinh nghiem");
        }

        // ---- B2. Bi loc cuon (khong cho tia set cua loc danh trung) ----
        if (dung < kho.Count)
        {
            var q = kho[dung++];
            Vector3 goc = pc.transform.position + new Vector3(0f, 0f, -30f);
            DatQuai(q, goc + Vector3.forward * 3f);
            q.keDanhCuoi = null;
            q.maxHealth = 30f; q.health = 30f;
            yield return null;
            var loc = Tornado.Spawn(goc, Vector3.forward, maskQuai);
            loc.boQua = toi;
            loc.boltInterval = 9999f;        // tat tia set (no ghi ke danh rieng)
            loc.boltDamage = 0f;
            float hanL = Time.time + 8f;
            while (!q.IsDead && Time.time < hanL) yield return null;
            Ghi("B2. chet vi bi loc cuon: chet " + q.IsDead + ", ke danh cuoi la minh " + ReferenceEquals(q.keDanhCuoi, toi));
            Kiem(q.IsDead, "loc xoay khong giet duoc quai - phep do khong noi len gi");
            Kiem(ReferenceEquals(q.keDanhCuoi, toi), "chet vi bi LOC CUON thi ke danh cuoi bi mat");
            if (loc != null) Object.Destroy(loc.gameObject);
        }

        // ---- B3. Vung lua cua thien thach phai biet nguoi tung ----
        {
            var truoc = new HashSet<VungLua>(Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None));
            ThienThach.SpawnLoat(pc.transform.position + new Vector3(20f, 0f, 20f),
                                 LayerMask.GetMask("Enemy", "Ground", "Default"), maskQuai, toi, 1);
            VungLua vl = null;
            float hanV = Time.time + 5f;
            while (vl == null && Time.time < hanV)
            {
                foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None))
                    if (!truoc.Contains(v)) { vl = v; break; }
                yield return null;
            }
            Ghi("B3. vung lua cua thien thach: co vung lua " + (vl != null)
                + ", biet nguoi tung " + (vl != null && vl.boQua == toi));
            Kiem(vl != null, "thien thach khong de lai vung lua - phep do khong noi len gi");
            Kiem(vl != null && vl.boQua == toi, "vung lua cua thien thach KHONG biet ai tung - quai chet trong lua thanh vo danh (va lua dot ca nguoi tung)");
        }

        // ---- B4. Cay bi dot phai nho ai dot ----
        {
            GameObject cay = null;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.name.StartsWith("TREE_") && t.GetComponent<CayChay>() == null) { cay = t.gameObject; break; }
            if (cay == null) Ghi("B4. khong tim thay cay nao de dot - bo qua");
            else
            {
                var cc = CayChay.Dot(cay, maskQuai, 10f, toi);
                Ghi("B4. dot cay " + cay.name + ": bat lua " + (cc != null) + ", nho ke dot " + (cc != null && cc.keDot == toi));
                Kiem(cc != null && cc.keDot == toi, "cay chay khong nho ai dot - quai chet vi cay chay thanh vo danh");
            }
        }

        // ================================================================
        // C. NAN NHAN LA NGUOI CHOI
        // ================================================================
        //
        // Ha nguoi choi khac thi may NAN NHAN doc keDanhCuoi cua CHINH NO roi gui
        // goi chet kem ghe ke ha (KetTran.BaoToiChetNeuCan) - may ke ha nghe goi
        // ay moi duoc kinh nghiem. Nen o day do: moi loai don trung nhan vat cua
        // may nay co ghi dung "nguoi choi khac" vao keDanhCuoi khong.
        Ghi("");
        Ghi("C. don cua NGUOI CHOI KHAC trung nhan vat cua may nay");
        {
            var keKhacGo = new GameObject("TAM_NguoiKhac");
            keKhacGo.transform.position = pc.transform.position + new Vector3(0f, 0f, 40f);
            var keKhac = keKhacGo.AddComponent<Damageable>();
            int maskToi = 1 << pc.gameObject.layer;
            Vector3 tam = pc.transform.position + Vector3.up;

            System.Action<string, System.Action> thu = (ten, danh) =>
            {
                toi.keDanhCuoi = null;
                toi.health = toi.maxHealth;
                danh();
                bool dung = ReferenceEquals(toi.keDanhCuoi, keKhac);
                Ghi("C. " + ten + " trung minh -> ke danh cuoi la nguoi kia: " + dung);
                Kiem(dung, ten + ": ha nguoi choi khac se khong duoc kinh nghiem (keDanhCuoi sai)");
            };

            thu("Mua bang", () => CombatUtil.AreaFreeze(tam, 2f, 1f, maskToi, 0.5f, 0.1f, 0f, 0f, keKhac, out int _));
            thu("Sam set", () => CombatUtil.AreaShock(tam, 2f, 1f, maskToi, 0f, 0f, keKhac));
            thu("Qua cau lua / Thien thach (no)", () => CombatUtil.AreaDamage(tam, 2f, 1f, maskToi, DamageType.Fire, 0f, keKhac));

            // Chay sau khi no: xoa ke danh roi cho mot nhip chay
            CombatUtil.AreaDamage(tam, 2f, 1f, maskToi, DamageType.Fire, 3f, keKhac);
            toi.keDanhCuoi = null;
            float hanC = Time.time + 1.2f;
            while (Time.time < hanC && toi.keDanhCuoi == null) yield return null;
            Ghi("C. chay (sau vu no) tren minh -> ke danh cuoi la nguoi kia: " + ReferenceEquals(toi.keDanhCuoi, keKhac));
            Kiem(ReferenceEquals(toi.keDanhCuoi, keKhac), "chet vi chay do nguoi khac gay ra se khong ai duoc kinh nghiem");
            var bc = toi.GetComponent<BurningEffect>();
            if (bc != null) Object.Destroy(bc);
            var fr = toi.GetComponent<FrozenEffect>();
            if (fr != null) Object.Destroy(fr);

            Object.Destroy(keKhacGo);
        }

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
        Debug.Log("[KnKyNang] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/kinhnghiem_kynang.txt", bao.ToString());
        var rac = GameObject.Find("TAM_KnKyNang");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
