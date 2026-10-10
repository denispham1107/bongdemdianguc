using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CHAY THU: MAY BOT DI LAI - BUOC 3 (menu 114, 08/10/2026).
///
/// Phong THAT tren Firebase (tai khoan chay thu B la chu phong) + 3 BOT, vao Act2 bang duong cua game. Tat GameDirector
/// (khong cho dot quai chen vao) trong cac muc A-D, bat lai cho muc E.
///   A. Luoi BanDoBot: so o, thoi gian dung; ngoai 4 doan rao sap (vuc) la VUC, ben trong la DI DUOC; o cua bia mo to
///      (cao > 0,6 m) KHONG di duoc; mat dat trong lan can tam DI DUOC.
///   B. Vong vat can: BOT di tu truoc ra SAU tung nha mo (3 nha). Doi chung CUNG cho: tat tim duong (lai thang) -> ket.
///   C. Vuc: BOT o trong khe rao sap, dich nam NGOAI (giua vuc). Co chong vuc -> khong bao gio xuong duoi -1 m; doi chung
///      tat tim duong + tat chong vuc -> roi qua nguong dia nguc. Them ca: tat tim duong nhung GIU chong vuc -> van khong roi
///      (do rieng phan chong vuc).
///   D. Muc tieu: doi thu (nhan vat may) cach 25 m -> BOT toi gan <= TamGiu roi dung; doi thu cach 45 m (ngoai tam phat hien)
///      -> van di san; hai ben cung doi (dong doi) -> BOT KHONG nham vao.
///   E. Tran that 60 s: 3 BOT, quai ra dot: khong BOT nao roi xuong vuc, quang duong moi BOT, so lan ket / vong / dung vuc.
/// Ket qua PlayTestShots/maybot_dilai.txt, anh maybot_dilai_*.png. Don sach phong thu.
/// </summary>
public static class ThuMayBotDiLai
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";
    static string maPhong;

    [MenuItem("Diablo 2.5D/114. Chay thu MAY BOT di lai (buoc 3)", false, 207)]
    public static void Chay()
    {
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[BotDiLai] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (EditorSceneManager.GetActiveScene().name != "MainMenu") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; loi = 0; daBatDau = false; maPhong = null;
        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_BotDiLai");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BotDiLai] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator DangNhapB()
    {
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong dang nhap duoc B: " + e);
    }

    static void DatCho(PlayerController pc, Vector3 p)
    {
        var cc = pc.GetComponent<CharacterController>();
        bool bat = cc != null && cc.enabled;
        if (bat) cc.enabled = false;
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(p.x, 60f, p.z), Vector3.down, out hit, 120f, BanDoBot.MatNaDat, QueryTriggerInteraction.Ignore))
            p.y = hit.point.y + 0.1f;
        pc.transform.position = p;
        if (bat) cc.enabled = true;
        Physics.SyncTransforms();
    }

    static float Ngang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    const float TamGiaoTranhBot = BotDieuKhien.TamGiaoTranh;

    /// <summary>Dat BOT len dung cap (bang cap MOI tu cap 1 - LenCap cong mau / nang luong nhu that; goi xong phai tra chi so goc).</summary>
    static void LenCapBot(PlayerController bot, int cap)
    {
        var bang = new BangCap();
        bang.BatDauTranMoi();
        bot.DatLaBot(bang);
        for (int c = 1; c < cap; c++) bang.Them(CapDo.CanDeLenCap(c));
    }

    /// <summary>Dich mot vat co CharacterController (quai) toi cho, bam dat.</summary>
    static void DatChoVat(Transform t, Vector3 p)
    {
        var cc = t.GetComponent<CharacterController>();
        bool bat = cc != null && cc.enabled;
        if (bat) cc.enabled = false;
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(p.x, 60f, p.z), Vector3.down, out hit, 120f, BanDoBot.MatNaDat, QueryTriggerInteraction.Ignore))
            p.y = hit.point.y + 0.1f;
        t.position = p;
        if (bat) cc.enabled = true;
        Physics.SyncTransforms();
    }

    /// <summary>Cho BOT tu di toi dich (diemEp) toi da <paramref name="giay"/> giay; ghi y thap nhat.</summary>
    static IEnumerator ChoDiToi(BotDieuKhien nao, Vector3 dich, float giay, float toiKhi, System.Action<bool, float, float> xong)
    {
        nao.coDiemEp = true; nao.diemEp = dich; nao.dungYen = false;
        float t0 = Time.time, yThap = nao.transform.position.y;
        bool toi = false;
        while (Time.time - t0 < giay)
        {
            yThap = Mathf.Min(yThap, nao.transform.position.y);
            if (Ngang(nao.transform.position, dich) <= toiKhi) { toi = true; break; }
            yield return null;
        }
        nao.coDiemEp = false; nao.dungYen = true;
        xong(toi, Time.time - t0, yThap);
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] may BOT di lai (buoc 3) - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        yield return DangNhapB();
        if (loi > 0) { yield return Ket(); yield break; }
        yield return PhongMang.DoDongHoMayChu();

        bool ok = false; string e = null;
        yield return PhongMang.TaoPhong("Thử BOT đi lại", CheDoTran.Don, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong tao duoc phong: " + e);
        if (!ok) { yield return Ket(); yield break; }
        maPhong = PhongMang.PhongHienTai.ma;
        for (int i = 0; i < 3; i++) yield return PhongMang.ThemBot(MayBot.Kho, CheDoTran.KhongDoi, (o, err) => { });
        yield return PhongMang.TaiLaiPhong(maPhong, (o, err) => { });

        TranHienTai.MaPhong = maPhong; TranHienTai.ManChoi = PhongMang.ManMacDinh;
        TranHienTai.LaHost = true; TranHienTai.DangChoiMang = true;
        KenhTrucTiep.Dong();
        SceneManager.LoadScene(PhongMang.ManMacDinh);
        float han = Time.realtimeSinceStartup + 40f;
        while (Time.realtimeSinceStartup < han && (KhoiDongTranMang.BotDaDung.Count < 3 || !BanDoBot.HopLeChoCanh)) yield return null;
        Kiem(KhoiDongTranMang.BotDaDung.Count == 3 && BanDoBot.HopLeChoCanh, "khong dung du 3 BOT / luoi di lai");
        if (KhoiDongTranMang.BotDaDung.Count < 3 || !BanDoBot.HopLeChoCanh) { yield return Ket(); yield break; }

        var dir = GameDirector.Instance;
        dir.enabled = false;
        foreach (var q in Object.FindObjectsByType<NhanDangQuai>()) Object.Destroy(q.gameObject);
        var toi = dir.player.GetComponent<PlayerController>();
        var dToi = toi.GetComponent<Damageable>();
        dToi.maxHealth = dToi.health = 1e7f;
        var bots = KhoiDongTranMang.BotDaDung;
        var nao = new BotDieuKhien[3];
        for (int i = 0; i < 3; i++) { nao[i] = bots[i].GetComponent<BotDieuKhien>(); nao[i].dungYen = true; nao[i].dungDanh = false; bots[i].GetComponent<Damageable>().tiLeDoDon = 0f; }
        // Menu nay do DI LAI (buoc 3): tat danh (buoc 4, menu 115) - BOT tung phep giua tran 60 s + dot quai tung lam GPU qua tai, Unity tat han (09/10/2026)
        // Hai BOT phu dung yen o goc xa - khong vuong cac muc do
        DatCho(bots[1], dir.arenaCenter + new Vector3(-45f, 0f, -45f));
        DatCho(bots[2], dir.arenaCenter + new Vector3(-45f, 0f, -40f));
        yield return null;

        // ---- A. Luoi ----
        Ghi("A1. luoi " + BanDoBot.SoCanh + "x" + BanDoBot.SoCanh + " o " + BanDoBot.O + " m, dung " + BanDoBot.GiayDung.ToString("F2") + " s: di duoc "
            + BanDoBot.SoODiDuoc + ", vat can " + BanDoBot.SoOVatCan + ", vuc + sat vuc " + BanDoBot.SoOVuc);
        Vector3[] khe = { new Vector3(22.9f, 0f, -66.4f), new Vector3(-28f, 0f, 66.4f), new Vector3(-66.4f, 0f, -12.7f), new Vector3(66.4f, 0f, 33.1f) };
        int ngoaiVuc = 0, trongDi = 0;
        var trong = new Vector3[4]; var ngoai = new Vector3[4];
        for (int k = 0; k < 4; k++)
        {
            Vector3 vao = -new Vector3(Mathf.Abs(khe[k].x) > 60f ? Mathf.Sign(khe[k].x) : 0f, 0f, Mathf.Abs(khe[k].z) > 60f ? Mathf.Sign(khe[k].z) : 0f);
            ngoai[k] = khe[k] - vao * 8f;
            trong[k] = khe[k] + vao * 7f;
            if (BanDoBot.LaVuc(ngoai[k])) ngoaiVuc++;
            Vector3 tg;
            if (BanDoBot.ODiDuocGanNhat(trong[k], 2.5f, out tg)) { trong[k] = tg; trongDi++; }
        }
        Ghi("A2. 4 khe rao sap: diem 8 m NGOAI la vuc " + ngoaiVuc + "/4, co o di duoc trong 7 m (+-2,5) " + trongDi + "/4");
        Kiem(ngoaiVuc == 4 && trongDi == 4, "luoi sai o khe rao sap");

        int soBia = 0, biaCam = 0;
        var goBia = GameObject.Find("BiaMo");
        if (goBia != null)
            foreach (var c in goBia.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger || c.bounds.size.y < 0.8f || Mathf.Max(c.bounds.size.x, c.bounds.size.z) < 0.5f) continue;
                soBia++;
                if (!BanDoBot.DiDuoc(c.bounds.center)) biaCam++;
            }
        Ghi("A3. bia mo to (cao > 0,8 m, ngang > 0,5 m): " + biaCam + "/" + soBia + " o tam bia KHONG di duoc");
        Kiem(soBia > 50 && biaCam >= soBia * 0.9f, "luoi khong chan bia mo");

        // ---- B. Vong qua nha mo ----
        var goNha = GameObject.Find("NhaMo");
        var nhaDo = new List<Bounds>();
        if (goNha != null)
            for (int i = 0; i < goNha.transform.childCount && nhaDo.Count < 3; i++)
            {
                var rr = goNha.transform.GetChild(i).GetComponentsInChildren<Collider>();
                if (rr.Length == 0) continue;
                var b = rr[0].bounds; foreach (var c in rr) b.Encapsulate(c.bounds);
                if (Ngang(b.center, dir.arenaCenter) < 55f) nhaDo.Add(b);
            }
        Kiem(nhaDo.Count >= 3, "khong tim du 3 nha mo");
        int toiDuoc = 0, doiChungKet = 0, doiChungThu = 0;
        for (int k = 0; k < nhaDo.Count; k++)
        {
            var b = nhaDo[k];
            float r = Mathf.Max(b.extents.x, b.extents.z) + 3.5f;
            Vector3 tr = b.center + new Vector3(0f, 0f, -r), sa = b.center + new Vector3(0f, 0f, r);
            Vector3 trD, saD;
            if (!BanDoBot.ODiDuocGanNhat(tr, 3f, out trD) || !BanDoBot.ODiDuocGanNhat(sa, 3f, out saD)) { Ghi("B" + k + ". nha mo " + k + ": khong co cho dung hai phia - bo qua"); continue; }
            var bot = bots[0]; var n0 = nao[0];
            DatCho(bot, trD); yield return null;
            n0.soLanKet = 0; n0.soLanVong = 0; n0.quangDuong = 0f;
            bool toiN = false; float tg = 0f, yT = 0f;
            yield return ChoDiToi(n0, saD, 25f, 1.2f, (a, b2, c) => { toiN = a; tg = b2; yT = c; });
            float thang = Ngang(trD, saD);
            Ghi("B" + k + ". vong nha mo (" + b.size.x.ToString("F1") + "x" + b.size.z.ToString("F1") + " m): toi " + toiN + " sau " + tg.ToString("F1") + " s, di "
                + n0.quangDuong.ToString("F1") + " m (duong chim bay " + thang.ToString("F1") + "), ket " + n0.soLanKet + ", vong " + n0.soLanVong);
            if (toiN) toiDuoc++;
            if (k == 0) yield return Chup("maybot_dilai_nhamo");

            // Doi chung: lai thang
            DatCho(bot, trD); yield return null;
            n0.dungTimDuong = false;
            bool toiDC = false;
            yield return ChoDiToi(n0, saD, 10f, 1.2f, (a, b2, c) => { toiDC = a; });
            n0.dungTimDuong = true;
            doiChungThu++;
            if (!toiDC) doiChungKet++;
            Ghi("   doi chung lai thang cung cho: toi " + toiDC + " trong 10 s");
        }
        Kiem(nhaDo.Count > 0 && toiDuoc == nhaDo.Count, "BOT khong vong qua duoc nha mo");
        Kiem(doiChungThu > 0 && doiChungKet >= 1, "doi chung lai thang cung qua duoc het - phep do khong phan biet");

        // ---- C. Vuc ----
        int khongRoi = 0, roiDoiChung = 0, khongRoiChiChongVuc = 0;
        for (int k = 0; k < 4; k++)
        {
            var bot = bots[0]; var n0 = nao[0];
            DatCho(bot, trong[k]); yield return null;
            n0.soLanDungVuc = 0;
            float yT = 0f; bool tmp = false;
            yield return ChoDiToi(n0, ngoai[k], 8f, 0.5f, (a, b2, c) => { tmp = a; yT = c; });
            bool on = yT > -1f && !BanDoBot.LaVucThat(bot.transform.position);
            if (on) khongRoi++;
            Ghi("C" + k + ". khe " + k + ": dich giua vuc, 8 s -> y thap nhat " + yT.ToString("F2") + ", dung vuc " + n0.soLanDungVuc + " lan, dung cach khe "
                + Ngang(bot.transform.position, khe[k]).ToString("F1") + " m");

            // Chi chong vuc (tat tim duong): lai thang ra mep van phai dung
            DatCho(bot, trong[k]); yield return null;
            n0.dungTimDuong = false;
            yield return ChoDiToi(n0, ngoai[k], 6f, 0.5f, (a, b2, c) => { yT = c; });
            n0.dungTimDuong = true;
            if (yT > -1f) khongRoiChiChongVuc++;
            Ghi("   chi chong vuc (lai thang ra mep): y thap nhat " + yT.ToString("F2"));
        }
        Kiem(khongRoi == 4, "BOT roi / dung tren vuc khi dich nam ngoai vuc");
        Kiem(khongRoiChiChongVuc == 4, "phan chong vuc khong tu giu duoc BOT");
        // Doi chung: BOT 3 (hy sinh) - tat ca hai, lai thang ra vuc
        {
            var bot = bots[2]; var n2 = nao[2];
            DatCho(bot, trong[0]); yield return null;
            n2.dungTimDuong = false; n2.dungChongVuc = false;
            float yT = 0f;
            yield return ChoDiToi(n2, ngoai[0], 8f, 0.3f, (a, b2, c) => { yT = c; });
            if (yT < DiaNguc.NguongRoi) roiDoiChung++;
            Ghi("C4. doi chung tat tim duong + tat chong vuc, lai thang ra khe 0: y thap nhat " + yT.ToString("F2") + " (nguong dia nguc " + DiaNguc.NguongRoi + ")");
            Kiem(roiDoiChung == 1, "doi chung khong roi - phep do vuc khong phan biet");
            DatCho(bot, dir.arenaCenter + new Vector3(-45f, 0f, -40f));
        }

        // ---- D. Muc tieu ----
        {
            var bot = bots[0]; var n0 = nao[0];
            Vector3 tam = dir.arenaCenter; Vector3 choBot;
            BanDoBot.ODiDuocGanNhat(tam + new Vector3(10f, 0f, 0f), 6f, out choBot);
            Vector3 choToi;
            // 09/10/2026: doi thu trong TamGiaoTranh (20 m) thi giao tranh ngay du cap 1 -> dat 15 m (truoc 25 m)
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, 15f), 4f, out choToi);
            DatCho(bot, choBot); DatCho(toi, choToi);
            DatCho(bots[1], tam + new Vector3(-55f, 0f, -55f)); DatCho(bots[2], tam + new Vector3(-55f, 0f, -50f));
            yield return null;
            n0.dungYen = false;
            float t0 = Time.time; float kc = 99f;
            while (Time.time - t0 < 15f) { kc = Ngang(bot.transform.position, toi.transform.position); if (kc <= BotDieuKhien.TamGiu && n0.MucTieu == dToi) break; yield return null; }
            yield return new WaitForSeconds(1.5f);
            float kcSau = Ngang(bot.transform.position, toi.transform.position);
            Ghi("D1. doi thu cach " + Ngang(choBot, choToi).ToString("F1") + " m: muc tieu la doi thu " + (n0.MucTieu == dToi) + ", toi gan " + kc.ToString("F1")
                + " m sau " + (Time.time - t0 - 1.5f).ToString("F1") + " s, 1,5 s sau van " + kcSau.ToString("F1") + " m (dung lai, TamGiu " + BotDieuKhien.TamGiu + ")");
            Kiem(n0.MucTieu == dToi && kc <= BotDieuKhien.TamGiu && kcSau >= BotDieuKhien.TamGiu * 0.6f && kcSau <= BotDieuKhien.TamGiu + 0.5f, "BOT khong toi gan doi thu roi dung");

            // ---- 09/10/2026 (nguoi dung): duoi cap 7 uu tien giet quai, chi giao tranh khi doi thu trong 20 m / vua danh minh;
            //      tu cap 7 di san doi thu (quai ap sat trong 20 m thi giet truoc) ----
            var dBot = bot.GetComponent<Damageable>();
            var bangGoc = bot.Cap;
            Vector3 chiSoGoc = new Vector3(dBot.maxHealth, bot.maxMana, bot.moveSpeed);
            // 10/10/2026 TAM NHIN 25 m (nguoi dung): BOT chi thay quai / doi thu trong 25 m -> "vua" = 22-23 m (ngoai 20 m giao tranh, TRONG
            // tam nhin), "xa" = ~45 m (NGOAI tam nhin: khong biet co ai)
            Vector3 choXa, choVua, choQuaiVua, choQuaiGan;
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, -45f), 8f, out choXa);
            if (Ngang(choXa, choBot) < 35f) BanDoBot.ODiDuocGanNhat(choBot + new Vector3(-45f, 0f, 0f), 8f, out choXa);
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, -23f), 1.5f, out choVua);
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(21f, 0f, 6f), 1.5f, out choQuaiVua);
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(10f, 0f, 4f), 4f, out choQuaiGan);
            Ghi(string.Format("    cho thu: xa {0:F1} m, vua {1:F1} m, quai vua {2:F1} m, quai gan {3:F1} m (tam nhin {4}, giao tranh {5})",
                Ngang(choXa, choBot), Ngang(choVua, choBot), Ngang(choQuaiVua, choBot), Ngang(choQuaiGan, choBot), TamNhin.BanKinh, TamGiaoTranhBot));
            float kc0, kc1; bool tungNham;

            // D2. Cap 1, doi thu NGOAI tam nhin (45 m), khong quai, khong ai danh: KHONG biet -> khong nham
            DatCho(bot, choBot); DatCho(toi, choXa); yield return null;
            kc0 = Ngang(bot.transform.position, toi.transform.position);
            tungNham = false;
            // MucTieu con giu doi thu cua D1 toi lan chon lai dau tien (nhip phan xa) - chi dem tu sau do
            yield return new WaitForSeconds(BotDieuKhien.NhipPhanXa(n0.doKho) + 0.2f);
            for (float tD = Time.time; Time.time - tD < 6f; ) { if (n0.MucTieu == dToi) tungNham = true; yield return null; }
            kc1 = Ngang(bot.transform.position, toi.transform.position);
            Ghi("D2. cap " + bot.Cap.Cap + ", doi thu cach " + kc0.ToString("F1") + " m (ngoai tam nhin), khong quai: 6 s nham doi thu " + tungNham
                + ", con cach " + kc1.ToString("F1") + " m");
            Kiem(kc0 > TamNhin.BanKinh + 10f && !tungNham, "BOT cap 1 nham doi thu ngoai tam nhin");

            // D2b. Cap 1, doi thu 23 m (trong tam nhin, ngoai 20 m) BAN TRUNG BOT: danh tra; doi chung ban tu 45 m (ngoai tam nhin): khong biet
            DatCho(bot, choBot); DatCho(toi, choVua); yield return null;
            kc0 = Ngang(bot.transform.position, toi.transform.position);
            dBot.GhiKeDanh(dToi); dBot.TakeDamage(1f, DamageType.Physical, Vector3.zero);
            float tNham = -1f;
            for (float tD = Time.time; Time.time - tD < 6f; ) { if (tNham < 0f && n0.MucTieu == dToi) tNham = Time.time - tD; yield return null; }
            kc1 = Ngang(bot.transform.position, toi.transform.position);
            dBot.lucNguoiChoiDanh = -100f;
            DatCho(bot, choBot); DatCho(toi, choXa); yield return new WaitForSeconds(BotDieuKhien.NhipPhanXa(n0.doKho) + 0.3f);
            dBot.GhiKeDanh(dToi); dBot.TakeDamage(1f, DamageType.Physical, Vector3.zero);
            tungNham = false;
            for (float tD = Time.time; Time.time - tD < 3f; ) { if (n0.MucTieu == dToi) tungNham = true; yield return null; }
            dBot.lucNguoiChoiDanh = -100f;
            Ghi("D2b. cap 1, doi thu cach " + kc0.ToString("F1") + " m ban trung BOT: nham sau " + tNham.ToString("F2") + " s, 6 s sau con " + kc1.ToString("F1")
                + " m; doi chung ban tu ngoai tam nhin (45 m): nham " + tungNham);
            Kiem(tNham >= 0f && tNham <= BotDieuKhien.NhipPhanXa(n0.doKho) + 0.3f && kc1 < kc0 - 10f, "BOT cap 1 khong danh tra nguoi vua danh minh (trong tam nhin)");
            Kiem(!tungNham, "BOT biet ke ban minh tu ngoai tam nhin");

            // D2c. Cap 7: doi thu 23 m -> di san; doi thu 45 m (ngoai tam nhin) -> khong biet (di tuan)
            LenCapBot(bot, BotDieuKhien.CapSanNguoi);
            DatCho(bot, choBot); DatCho(toi, choVua); yield return null;
            kc0 = Ngang(bot.transform.position, toi.transform.position);
            yield return new WaitForSeconds(6f);
            kc1 = Ngang(bot.transform.position, toi.transform.position);
            DatCho(bot, choBot); DatCho(toi, choXa); yield return new WaitForSeconds(BotDieuKhien.NhipPhanXa(n0.doKho) + 0.3f);
            tungNham = false;
            for (float tD = Time.time; Time.time - tD < 4f; ) { if (n0.MucTieu == dToi) tungNham = true; yield return null; }
            Ghi("D2c. cap " + bot.Cap.Cap + ", doi thu cach " + kc0.ToString("F1") + " m: 6 s sau con " + kc1.ToString("F1") + " m; doi thu 45 m: nham " + tungNham);
            Kiem(bot.Cap.Cap == BotDieuKhien.CapSanNguoi && kc0 > TamGiaoTranhBot && kc1 < kc0 - 10f, "BOT cap 7 khong di san doi thu trong tam nhin");
            Kiem(!tungNham, "BOT cap 7 biet doi thu ngoai tam nhin");

            // Quai thu: mot dot that, tat nao, gom ve mot cho
            dir.SinhDotQuanhNguoi();
            foreach (var ai in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) ai.enabled = false;
            var quaiThu = new List<Damageable>(dir.QuaiConSong);
            Kiem(quaiThu.Count > 0, "khong sinh duoc quai thu");

            // D2d. Cap 1, quai 22 m + doi thu 23 m (deu trong tam nhin, ngoai 20 m): chon QUAI
            bot.DatLaBot(bangGoc); dBot.maxHealth = chiSoGoc.x; bot.maxMana = chiSoGoc.y; bot.moveSpeed = chiSoGoc.z;
            DatCho(bot, choBot); DatCho(toi, choVua);
            for (int i = 0; i < quaiThu.Count; i++) DatChoVat(quaiThu[i].transform, choQuaiVua + new Vector3((i % 4) * 0.8f, 0f, (i / 4) * 0.8f));
            yield return new WaitForSeconds(BotDieuKhien.NhipPhanXa(n0.doKho) + 0.4f);
            var mtD = n0.MucTieu;
            Ghi("D2d. cap " + bot.Cap.Cap + ", " + quaiThu.Count + " quai cach " + Ngang(choBot, choQuaiVua).ToString("F1") + " m, doi thu cach " + Ngang(choBot, choVua).ToString("F1")
                + " m: muc tieu " + (mtD == null ? "khong ai" : mtD.isPlayer ? "NGUOI CHOI" : "quai"));
            Kiem(mtD != null && quaiThu.Contains(mtD), "BOT cap 1 khong uu tien giet quai");

            // D2e. Cap 7, quai ap sat (10 m) + doi thu 23 m: giet quai truoc; D2f. quai 22 m: san doi thu
            LenCapBot(bot, BotDieuKhien.CapSanNguoi);
            DatCho(bot, choBot);
            for (int i = 0; i < quaiThu.Count; i++) DatChoVat(quaiThu[i].transform, choQuaiGan + new Vector3((i % 4) * 1.5f, 0f, (i / 4) * 1.5f));
            yield return new WaitForSeconds(BotDieuKhien.NhipPhanXa(n0.doKho) + 0.4f);
            var mtE = n0.MucTieu;
            DatCho(bot, choBot);
            for (int i = 0; i < quaiThu.Count; i++) DatChoVat(quaiThu[i].transform, choQuaiVua + new Vector3((i % 4) * 0.8f, 0f, (i / 4) * 0.8f));
            yield return new WaitForSeconds(BotDieuKhien.NhipPhanXa(n0.doKho) + 0.4f);
            var mtF = n0.MucTieu;
            Ghi("D2e. cap " + bot.Cap.Cap + ", quai cach ~10 m + doi thu 23 m: muc tieu " + (mtE == null ? "khong ai" : mtE.isPlayer ? "NGUOI CHOI" : "quai")
                + "; D2f. quai cach ~" + Ngang(choBot, choQuaiVua).ToString("F0") + " m: muc tieu " + (mtF == null ? "khong ai" : mtF.isPlayer ? "nguoi choi" : "QUAI"));
            Kiem(mtE != null && quaiThu.Contains(mtE), "BOT cap 7 khong giet quai ap sat truoc");
            Kiem(mtF == dToi, "BOT cap 7 khong uu tien san doi thu khi quai ngoai 20 m");

            // Don: giet quai thu, tra BOT ve cap goc
            foreach (var q in quaiThu) if (q != null && !q.IsDead) q.TakeDamage(1e9f, DamageType.Physical, Vector3.zero);
            bot.DatLaBot(bangGoc); dBot.maxHealth = chiSoGoc.x; dBot.health = dBot.maxHealth; bot.maxMana = chiSoGoc.y; bot.moveSpeed = chiSoGoc.z;
            yield return null;

            // D2g. DI TUAN KHAP BAN DO (nguoi dung chon): khong thay ai -> toi o lau chua ghe. Nhan vat may = DONG DOI cua BOT (khong la muc tieu)
            {
                sbyte dCu = dToi.doi, bCu = dBot.doi;
                dToi.doi = 5; dBot.doi = 5;
                DatCho(bot, choBot); yield return null;
                int o0 = n0.SoODaGhe; n0.quangDuong = 0f; n0.dungYen = false;
                var cacCho = new List<Vector3>();
                for (float tD = Time.time; Time.time - tD < 45f; ) { if (cacCho.Count == 0 || Ngang(cacCho[cacCho.Count - 1], bot.transform.position) > 6f) cacCho.Add(bot.transform.position); yield return null; }
                float xaNhat = 0f; foreach (var c in cacCho) xaNhat = Mathf.Max(xaNhat, Ngang(c, choBot));
                Ghi(string.Format("D2g. di tuan 45 s (khong thay ai): o da ghe {0} -> {1} / {2}, quang duong {3:F0} m, xa cho xuat phat nhat {4:F0} m, muc tieu {5}",
                    o0, n0.SoODaGhe, BotDieuKhien.SoOTuan * BotDieuKhien.SoOTuan, n0.quangDuong, xaNhat, n0.MucTieu == null ? "khong ai" : n0.MucTieu.name));
                Kiem(n0.SoODaGhe - o0 >= 4 && n0.quangDuong > 80f && xaNhat > 35f, "BOT khong di tuan khap ban do khi khong thay ai");
                n0.dungYen = true; dToi.doi = dCu; dBot.doi = bCu;
            }

            // Dong doi: khong nham
            sbyte doiCu = dToi.doi, doiBotCu = bot.GetComponent<Damageable>().doi;
            dToi.doi = 0; bot.GetComponent<Damageable>().doi = 0;
            foreach (var bb in new[] { bots[1], bots[2] }) bb.GetComponent<Damageable>().doi = 0;
            yield return new WaitForSeconds(1.5f);
            var mt = n0.MucTieu;
            Ghi("D3. cung doi voi moi nguoi: muc tieu " + (mt == null ? "khong ai (di tuan)" : mt.name));
            Kiem(mt == null, "BOT nham vao dong doi");
            dToi.doi = doiCu; bot.GetComponent<Damageable>().doi = doiBotCu;
            foreach (var bb in new[] { bots[1], bots[2] }) bb.GetComponent<Damageable>().doi = -1;
            n0.dungYen = true;
        }

        // ---- E. Tran that 60 s ----
        {
            DatCho(toi, dir.arenaCenter + new Vector3(0f, 0f, -50f));
            var cho = new[] { new Vector3(15f, 0f, 15f), new Vector3(-20f, 0f, 10f), new Vector3(5f, 0f, -25f) };
            var yThap = new float[3];
            for (int i = 0; i < 3; i++)
            {
                Vector3 c; BanDoBot.ODiDuocGanNhat(dir.arenaCenter + cho[i], 6f, out c);
                if (!bots[i].GetComponent<Damageable>().IsDead) DatCho(bots[i], c);
                nao[i].dungYen = false; nao[i].dungTimDuong = true; nao[i].dungChongVuc = true;
                nao[i].soLanKet = nao[i].soLanVong = nao[i].soLanDungVuc = 0; nao[i].quangDuong = 0f;
                // BOT da chet (BOT 3 roi o doi chung C4) khong tinh - chi do BOT con song
                yThap[i] = bots[i].GetComponent<Damageable>().IsDead ? 0f : bots[i].transform.position.y;
            }
            dir.enabled = true;
            dir.SinhDotQuanhNguoi();
            float t0 = Time.time;
            int lanDot = 1;
            var dungIm = new float[3]; var dungImMax = new float[3]; var viTriCu = new Vector3[3];
            for (int i = 0; i < 3; i++) viTriCu[i] = bots[i].transform.position;
            float lucDo = 0f;
            while (Time.time - t0 < 60f)
            {
                if (Time.time - t0 > 20f * lanDot) { dir.SinhDotQuanhNguoi(); lanDot++; }
                for (int i = 0; i < 3; i++) if (!bots[i].GetComponent<Damageable>().IsDead || bots[i].transform.position.y > BanDoBot.DatVucDuoi) yThap[i] = Mathf.Min(yThap[i], bots[i].transform.position.y);
                if (Time.time >= lucDo)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var dd = bots[i].GetComponent<Damageable>();
                        bool muonDi = nao[i].MucTieu != null && Ngang(bots[i].transform.position, nao[i].MucTieu.transform.position) > BotDieuKhien.TamGiu + 1f;
                        if (!dd.IsDead && muonDi && Ngang(bots[i].transform.position, viTriCu[i]) < 0.3f) dungIm[i] += 1f;
                        else dungIm[i] = 0f;
                        dungImMax[i] = Mathf.Max(dungImMax[i], dungIm[i]);
                        viTriCu[i] = bots[i].transform.position;
                    }
                    lucDo = Time.time + 1f;
                }
                yield return null;
            }
            dir.enabled = false;
            int roi = 0;
            for (int i = 0; i < 3; i++)
            {
                var dd = bots[i].GetComponent<Damageable>();
                if (yThap[i] < DiaNguc.NguongRoi) roi++;
                Ghi("E" + i + ". " + bots[i].name + ": di " + nao[i].quangDuong.ToString("F0") + " m, ket " + nao[i].soLanKet + ", vong " + nao[i].soLanVong + ", dung vuc "
                    + nao[i].soLanDungVuc + ", y thap nhat " + yThap[i].ToString("F2") + ", dung im lau nhat khi muon di " + dungImMax[i] + " s, mau " + Mathf.RoundToInt(dd.health)
                    + "/" + Mathf.RoundToInt(dd.maxHealth) + (dd.IsDead ? " (DA CHET)" : ""));
                Kiem(dd.IsDead || nao[i].quangDuong > 20f, bots[i].name + " gan nhu khong di");
                Kiem(dungImMax[i] <= 4f, bots[i].name + " dung im qua lau khi muon di (ket)");
            }
            Ghi("E. " + lanDot + " dot quai trong 60 s; BOT roi xuong vuc: " + roi + " (phai 0)");
            Kiem(roi == 0, "BOT roi xuong vuc trong tran that");
            yield return Chup("maybot_dilai_tran");
        }

        yield return Ket();
    }

    static IEnumerator Chup(string anh)
    {
        string d = "PlayTestShots/" + anh + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
        Ghi("   anh " + anh + ".png");
    }

    static IEnumerator Ket()
    {
        KenhTrucTiep.Dong();
        if (!string.IsNullOrEmpty(maPhong))
        {
            if (string.IsNullOrEmpty(FirebaseMang.Uid)) yield return DangNhapB();
            yield return FirebaseMang.Xoa("phong/" + maPhong, (o, er) => { });
            yield return FirebaseMang.Xoa("tran/" + maPhong, (o, er) => { });
            string con = null;
            yield return FirebaseMang.Doc("phong/" + maPhong, s => con = s);
            bool sach = string.IsNullOrEmpty(con) || con == "null";
            Ghi("don phong " + maPhong + ": " + (sach ? "sach" : "VAN CON"));
            Kiem(sach, "phong thu con tren Firebase");
        }
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/maybot_dilai.txt", bao.ToString());
        PhongMang.PhongHienTai = null;
        TranHienTai.Xoa();
        FirebaseMang.Quen();
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t != null && t.parent == null && t.name.StartsWith("TAM_")) Object.Destroy(t.gameObject);
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
        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc); else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
