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
        for (int i = 0; i < 3; i++) { nao[i] = bots[i].GetComponent<BotDieuKhien>(); nao[i].dungYen = true; bots[i].GetComponent<Damageable>().tiLeDoDon = 0f; }
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
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, 25f), 6f, out choToi);
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

            // Ngoai tam phat hien: van di san
            Vector3 choXa;
            BanDoBot.ODiDuocGanNhat(bot.transform.position + new Vector3(0f, 0f, -45f), 8f, out choXa);
            if (Ngang(choXa, bot.transform.position) < 35f) BanDoBot.ODiDuocGanNhat(bot.transform.position + new Vector3(-45f, 0f, 0f), 8f, out choXa);
            DatCho(toi, choXa); yield return null;
            float kc0 = Ngang(bot.transform.position, toi.transform.position);
            yield return new WaitForSeconds(6f);
            float kc1 = Ngang(bot.transform.position, toi.transform.position);
            Ghi("D2. doi thu cach " + kc0.ToString("F1") + " m (ngoai tam phat hien " + BotDieuKhien.TamPhatHien + "): 6 s sau con " + kc1.ToString("F1") + " m");
            Kiem(kc0 > BotDieuKhien.TamPhatHien && kc1 < kc0 - 15f, "BOT khong di san doi thu ngoai tam");

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
