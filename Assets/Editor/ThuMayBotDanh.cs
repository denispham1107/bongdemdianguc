using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CHAY THU: MAY BOT DANH - BUOC 4 (menu 115, 09/10/2026).
///
/// Phong THAT tren Firebase (tai khoan chay thu B la chu phong) + 3 BOT, vao Act2 bang duong cua game.
///   A. Cong diem (ham thuan MayBot.TieuDiem): ca 4 he tu cap 1 len cap 20 - cap 2 da nang chieu co ban, cap 3 mo ky thu
///      hai, het tran ba ky cua he deu cap 5, khong con diem thua.
///   B. Danh theo he: BOT (Kho, cap 12) danh muc tieu dung yen 10 m (nhan vat may, mau rat lon) 10 s: so loai chieu (lien hoan
///      >= 2 loai), sat thuong gay ra (mat mau that cua muc tieu).
///   C. Do kho: cung he Lua cap 1 (chi Qua cau lua), 10 s: sai so ngam TB, so chieu, sat thuong - De vs Kho.
///   D. Binh: mau 20% + 2 binh mau -> uong; nang luong 0 + 1 binh mana -> uong (so binh giam, mau / nang luong tang).
///   E. Nhat binh: binh mau roi cach 7 m, khong ke thu gan -> BOT di nhat, binh vao BANG CUA BOT (khong vao cua may).
///   F. Khien: mau 50%, ke thu cach 6 m -> bat Khien.
///   G. Tran that 90 s: 3 BOT cung doi (khong danh nhau, khong danh nhan vat may), 3 he khac nhau, quai ra dot: kinh nghiem
///      moi BOT tang (= ha quai - do bang bang cap, khong bang so dem cua BOT), so chieu, binh da uong, con song.
/// Ket qua PlayTestShots/maybot_danh.txt. Don sach phong thu.
/// </summary>
public static class ThuMayBotDanh
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";
    static string maPhong;

    [MenuItem("Diablo 2.5D/115. Chay thu MAY BOT danh (buoc 4)", false, 208)]
    public static void Chay()
    {
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[BotDanh] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (EditorSceneManager.GetActiveScene().name != "MainMenu") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; loi = 0; daBatDau = false; maPhong = null; chiSoGoc.Clear();
        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_BotDanh");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BotDanh] " + s); }
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

    static int TongKn(BangCap b)
    {
        int tong = b.KinhNghiem;
        for (int c = 1; c < b.Cap; c++) tong += CapDo.CanDeLenCap(c);
        return tong;
    }

    /// <summary>Dat lai BOT ve bang cap moi o cap <paramref name="cap"/> (diem tieu theo he), day mau / nang luong.</summary>
    static readonly Dictionary<PlayerController, Vector3> chiSoGoc = new Dictionary<PlayerController, Vector3>();

    static void LamMoiBot(PlayerController bot, BotDieuKhien nao, int he, int doKho, int cap)
    {
        nao.he = he; nao.doKho = doKho;
        // Mau / nang luong / toc do GOC: len cap nhan don vao chung - goi lai nhieu lan thi phai tra ve goc truoc
        var dg = bot.GetComponent<Damageable>();
        Vector3 goc;
        if (!chiSoGoc.TryGetValue(bot, out goc)) { goc = new Vector3(dg.maxHealth, bot.maxMana, bot.moveSpeed); chiSoGoc[bot] = goc; }
        dg.maxHealth = goc.x; bot.maxMana = goc.y; bot.moveSpeed = goc.z;
        var bang = new BangCap();
        bang.BatDauTranMoi();
        bot.DatLaBot(bang);                                  // nhan bang TRUOC khi len cap: LenCap cong mau / nang luong nhu that
        for (int c = 1; c < cap; c++) bang.Them(CapDo.CanDeLenCap(c));
        MayBot.TieuDiem(bang, he);
        var d = bot.GetComponent<Damageable>();
        d.health = d.maxHealth;
        bot.mana = bot.maxMana;
        nao.soPhepDaTung = 0; nao.soBinhMauDaUong = 0; nao.soBinhManaDaUong = 0; nao.soLanLui = 0; nao.soLanNhaoLon = 0;
        nao.tongSaiSoNgam = 0f; nao.soLanDoNgam = 0;
        for (int i = 0; i < nao.demTheoKy.Length; i++) nao.demTheoKy[i] = 0;
    }

    static string DemKy(BotDieuKhien nao, out int soLoai)
    {
        var sb = new StringBuilder(); soLoai = 0;
        for (int k = 0; k < nao.demTheoKy.Length; k++)
            if (nao.demTheoKy[k] > 0) { soLoai++; sb.Append(SachPhep.Ten(k)).Append(" x").Append(nao.demTheoKy[k]).Append(", "); }
        return sb.ToString();
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] may BOT danh (buoc 4) - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

        // ---- A. Cong diem (ham thuan) ----
        string[] kyHe = { "0,4,11", "9,1,12", "6,2,13", "10,3,21" };
        int[][] baKy = { new[] { 0, 4, CapDo.KyLuaDiaNguc }, new[] { CapDo.KyQuaCauBang, 1, CapDo.KyTangHinh },
                         new[] { 6, 2, CapDo.KyCauDien }, new[] { CapDo.KyGioLoc, 3, CapDo.KyMayGiong } };
        for (int he = 0; he < 4; he++)
        {
            var b = new BangCap(); b.BatDauTranMoi();
            MayBot.TieuDiem(b, he);
            int capCoBanO1 = b.CapCuaKyNang(baKy[he][0]);
            b.Them(CapDo.CanDeLenCap(1)); MayBot.TieuDiem(b, he);
            int capCoBanO2 = b.CapCuaKyNang(baKy[he][0]);
            b.Them(CapDo.CanDeLenCap(2)); MayBot.TieuDiem(b, he);
            bool moKy2O3 = b.DaMo(baKy[he][1]);
            for (int c = 3; c < CapDo.CapToiDa; c++) { b.Them(CapDo.CanDeLenCap(c)); MayBot.TieuDiem(b, he); }
            var sb = new StringBuilder();
            for (int k = 0; k < CapDo.SoKyNang; k++) if (b.CapCuaKyNang(k) > 0) sb.Append(k).Append(":").Append(b.CapCuaKyNang(k)).Append(" ");
            bool du = b.CapCuaKyNang(baKy[he][0]) == 5 && b.CapCuaKyNang(baKy[he][1]) == 5 && b.CapCuaKyNang(baKy[he][2]) == 5 && b.DaMo(5);
            Ghi("A" + he + ". he " + MayBot.TenHe(he) + " (" + kyHe[he] + "): cap 1 chieu co ban " + capCoBanO1 + ", cap 2 -> " + capCoBanO2 + ", cap 3 mo ky 2 "
                + moKy2O3 + " | cap 20: " + sb + "| diem thua " + b.DiemKyNang);
            Kiem(capCoBanO1 == 1 && capCoBanO2 == 2 && moKy2O3 && du && b.DiemKyNang == 0 && b.Cap == CapDo.CapToiDa, "cong diem he " + MayBot.TenHe(he) + " sai");
        }

        // ---- Vao tran ----
        yield return DangNhapB();
        if (string.IsNullOrEmpty(FirebaseMang.Uid)) { yield return Ket(); yield break; }
        yield return PhongMang.DoDongHoMayChu();
        bool ok = false; string e = null;
        yield return PhongMang.TaoPhong("Thử BOT đánh", CheDoTran.Don, (o, err) => { ok = o; e = err; });
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
        FirebaseMang.Quen();

        var dir = GameDirector.Instance;
        dir.enabled = false;
        foreach (var q in Object.FindObjectsByType<NhanDangQuai>()) Object.Destroy(q.gameObject);
        var toi = dir.player.GetComponent<PlayerController>();
        var dToi = toi.GetComponent<Damageable>();
        var bots = KhoiDongTranMang.BotDaDung;
        var nao = new BotDieuKhien[3];
        for (int i = 0; i < 3; i++)
        {
            nao[i] = bots[i].GetComponent<BotDieuKhien>();
            nao[i].dungYen = true;
            bots[i].GetComponent<Damageable>().tiLeDoDon = 0f;
        }
        Vector3 tam = dir.arenaCenter;
        Vector3 c1, c2;
        BanDoBot.ODiDuocGanNhat(tam + new Vector3(-50f, 0f, -48f), 8f, out c1); DatCho(bots[1], c1);
        BanDoBot.ODiDuocGanNhat(tam + new Vector3(-48f, 0f, -52f), 8f, out c2); DatCho(bots[2], c2);
        // BOT phu: khong danh, dung yen
        nao[1].dungDanh = false; nao[2].dungDanh = false;

        // Cho danh: BOT 0 o giua, muc tieu (nhan vat may) 10 m
        Vector3 choBot, choMuc;
        BanDoBot.ODiDuocGanNhat(tam + new Vector3(8f, 0f, 2f), 6f, out choBot);
        BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, 9f), 4f, out choMuc);
        var b0 = bots[0]; var n0 = nao[0];
        var d0 = b0.GetComponent<Damageable>();

        // ---- B. Danh theo he ----
        for (int he = 0; he < 4; he++)
        {
            dToi.maxHealth = dToi.health = 1e7f;
            DatCho(b0, choBot); DatCho(toi, choMuc);
            LamMoiBot(b0, n0, he, MayBot.Kho, 12);
            yield return new WaitForSeconds(0.5f);
            float mauTruoc = dToi.health;
            n0.dungYen = false;
            yield return new WaitForSeconds(10f);
            n0.dungYen = true;
            int soLoai;
            string ds = DemKy(n0, out soLoai);
            float satThuong = mauTruoc - dToi.health;
            Ghi("B" + he + ". he " + MayBot.TenHe(he) + " (Kho, cap 12): " + n0.soPhepDaTung + " chieu - " + ds + "sat thuong vao muc tieu " + Mathf.RoundToInt(satThuong)
                + ", binh mana uong " + n0.soBinhManaDaUong);
            Kiem(soLoai >= 2, "he " + MayBot.TenHe(he) + " khong dung chieu lien hoan (it hon 2 loai chieu)");
            Kiem(satThuong > 100f, "he " + MayBot.TenHe(he) + " khong gay sat thuong");
            // Doi hieu ung tren muc tieu tan
            foreach (var hu in toi.GetComponents<MonoBehaviour>())
                if (hu is FrozenEffect || hu is StunnedEffect || hu is BiDanhNga || hu is WhirledEffect || hu is BurningEffect || hu is BiHatTung) Object.Destroy(hu);
            yield return new WaitForSeconds(2.5f);
        }

        // ---- C. Do kho ----
        var kq = new float[3, 3];
        foreach (int dk in new[] { MayBot.De, MayBot.Kho })
        {
            dToi.maxHealth = dToi.health = 1e7f;
            DatCho(b0, choBot); DatCho(toi, choMuc);
            LamMoiBot(b0, n0, MayBot.HeLua, dk, 1);
            b0.mana = b0.maxMana = 1e5f;      // khong de het nang luong lam nhieu
            yield return new WaitForSeconds(0.5f);
            float mauTruoc = dToi.health;
            n0.dungYen = false;
            yield return new WaitForSeconds(10f);
            n0.dungYen = true;
            float sai = n0.soLanDoNgam > 0 ? n0.tongSaiSoNgam / n0.soLanDoNgam : -1f;
            kq[dk, 0] = sai; kq[dk, 1] = n0.soPhepDaTung; kq[dk, 2] = mauTruoc - dToi.health;
            Ghi("C. " + MayBot.TenDoKho(dk) + " (Lua cap 1, chi Qua cau lua): " + n0.soPhepDaTung + " chieu / 10 s, sai so ngam TB " + sai.ToString("F2")
                + " m (dat " + BotDieuKhien.SaiSoNgam(dk) + "), sat thuong " + Mathf.RoundToInt(kq[dk, 2]));
            b0.maxMana = 180f; b0.mana = 180f;     // muc vao tran (09/10/2026, truoc 250)
            yield return new WaitForSeconds(1.5f);
        }
        // 09/10/2026: Qua cau lua hoi chieu 0,9 s (nguoi dung) -> nhip ra chieu ca hai muc deu bi hoi chieu ghim (~10 chieu / 10 s), sat thuong
        // len bia di lai dao dong ngau nhien (lan do: De 951, Kho 813). Con phan biet duoc: ngam lech, va so chieu De khong nhieu hon Kho.
        Kiem(kq[MayBot.De, 0] > kq[MayBot.Kho, 0] * 2f && kq[MayBot.De, 1] <= kq[MayBot.Kho, 1],
             "BOT De khong kem BOT Kho (ngam lech / so chieu)");

        // ---- D. Binh ----
        {
            DatCho(toi, tam + new Vector3(-40f, 0f, 45f));
            LamMoiBot(b0, n0, MayBot.HeLua, MayBot.Kho, 3);
            DatCho(b0, choBot);
            b0.Cap.ThemBinh(CapDo.KyBinhMau); b0.Cap.ThemBinh(CapDo.KyBinhMau);
            d0.health = d0.maxHealth * 0.2f;
            float mauT = d0.health; int binhT = b0.Cap.SoBinh(CapDo.KyBinhMau);
            n0.dungYen = false;
            yield return new WaitForSeconds(1.5f);
            Ghi("D1. mau 20% + 2 binh mau: uong " + n0.soBinhMauDaUong + ", binh " + binhT + " -> " + b0.Cap.SoBinh(CapDo.KyBinhMau) + ", mau " + Mathf.RoundToInt(mauT) + " -> " + Mathf.RoundToInt(d0.health));
            Kiem(n0.soBinhMauDaUong >= 1 && b0.Cap.SoBinh(CapDo.KyBinhMau) < binhT && d0.health > mauT + 100f, "BOT khong uong binh mau");
            b0.Cap.ThemBinh(CapDo.KyBinhMana);
            b0.mana = 0f;
            int manaT = b0.Cap.SoBinh(CapDo.KyBinhMana);
            yield return new WaitForSeconds(1.0f);
            Ghi("D2. nang luong 0 + 1 binh mana: uong " + n0.soBinhManaDaUong + ", binh " + manaT + " -> " + b0.Cap.SoBinh(CapDo.KyBinhMana) + ", nang luong " + Mathf.RoundToInt(b0.mana));
            Kiem(n0.soBinhManaDaUong >= 1 && b0.Cap.SoBinh(CapDo.KyBinhMana) < manaT && b0.mana > 50f, "BOT khong uong binh mana");
            n0.dungYen = true;
        }

        // ---- E. Nhat binh ----
        {
            d0.health = d0.maxHealth * 0.5f;
            int binhBotT = b0.Cap.SoBinh(CapDo.KyBinhMau), binhMayT = CapDo.SoBinh(CapDo.KyBinhMau);
            Vector3 choBinh;
            BanDoBot.ODiDuocGanNhat(b0.transform.position + new Vector3(7f, 0f, 0f), 3f, out choBinh);
            choBinh.y = b0.transform.position.y;
            QuanLyBinhRoi.Roi(CapDo.KyBinhMau, choBinh);
            n0.dungYen = false;
            float t0 = Time.time;
            while (Time.time - t0 < 8f && b0.Cap.SoBinh(CapDo.KyBinhMau) <= binhBotT) yield return null;
            Ghi("E. binh mau roi cach " + Ngang(b0.transform.position, choBinh).ToString("F1") + " m luc dau: binh cua BOT " + binhBotT + " -> " + b0.Cap.SoBinh(CapDo.KyBinhMau)
                + " sau " + (Time.time - t0).ToString("F1") + " s, binh cua may " + binhMayT + " -> " + CapDo.SoBinh(CapDo.KyBinhMau));
            Kiem(b0.Cap.SoBinh(CapDo.KyBinhMau) == binhBotT + 1 && CapDo.SoBinh(CapDo.KyBinhMau) == binhMayT, "BOT khong nhat duoc binh / binh vao nham bang");
            n0.dungYen = true;
        }

        // ---- F. Khien ----
        {
            LamMoiBot(b0, n0, MayBot.HeLua, MayBot.Kho, 6);
            DatCho(b0, choBot);
            Vector3 gan;
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, 6f), 2f, out gan);
            DatCho(toi, gan);
            dToi.maxHealth = dToi.health = 1e7f;
            d0.health = d0.maxHealth * 0.5f;
            yield return null;
            n0.dungYen = false;
            float t0 = Time.time;
            while (Time.time - t0 < 3f && n0.demTheoKy[5] == 0) yield return null;
            Ghi("F. mau 50%, doi thu cach 6 m: Khien da mo " + b0.Cap.DaMo(5) + ", bat Khien " + n0.demTheoKy[5] + " lan sau " + (Time.time - t0).ToString("F1") + " s, khien con "
                + b0.KhiengMau01.ToString("F2"));
            Kiem(b0.Cap.DaMo(5) && n0.demTheoKy[5] >= 1, "BOT khong bat Khien khi mau thap bi ap sat");
            n0.dungYen = true;
        }

        // ---- F2. NHAO LON khi bi ap sat (10/10/2026) ----
        {
            LamMoiBot(b0, n0, MayBot.HeLua, MayBot.Kho, 3);
            DatCho(b0, choBot);
            Vector3 gan;
            BanDoBot.ODiDuocGanNhat(choBot + new Vector3(0f, 0f, 2.5f), 1f, out gan);
            DatCho(toi, gan);
            dToi.maxHealth = dToi.health = 1e7f;
            yield return null;
            Vector3 p0 = b0.transform.position;
            float kc0 = Ngang(p0, toi.transform.position);
            n0.dungYen = false;
            float t0 = Time.time; float xaNhat = 0f; bool daLan = false;
            while (Time.time - t0 < 4f)
            {
                if (NhaoLon.Dang(b0.gameObject)) daLan = true;
                xaNhat = Mathf.Max(xaNhat, Ngang(b0.transform.position, toi.transform.position));
                if (daLan && !NhaoLon.Dang(b0.gameObject)) break;
                yield return null;
            }
            Ghi(string.Format("F2. Kho, doi thu ap sat {0:F1} m: nhao lon {1} lan sau {2:F1} s, xa doi thu nhat {3:F1} m (ty le xet moi giay {4:P0})",
                kc0, n0.soLanNhaoLon, Time.time - t0, xaNhat, BotDieuKhien.TiLeNhaoLon(MayBot.Kho)));
            Kiem(n0.soLanNhaoLon >= 1 && daLan && xaNhat > kc0 + 3f, "BOT khong nhao lon tranh khi bi ap sat");
            n0.dungYen = true;
            yield return new WaitForSeconds(0.5f);
        }

        // ---- G. Tran that 90 s ----
        {
            DatCho(toi, tam + new Vector3(-50f, 0f, 50f));
            dToi.doi = 0;
            var cho = new[] { new Vector3(12f, 0f, 12f), new Vector3(-15f, 0f, 8f), new Vector3(4f, 0f, -18f) };
            var knTruoc = new int[3];
            for (int i = 0; i < 3; i++)
            {
                LamMoiBot(bots[i], nao[i], i, MayBot.Kho, 1);
                bots[i].GetComponent<Damageable>().doi = 0;      // cung doi voi nhau va voi nhan vat may: chi danh quai
                Vector3 c; BanDoBot.ODiDuocGanNhat(tam + cho[i], 6f, out c); DatCho(bots[i], c);
                nao[i].dungDanh = true; nao[i].dungYen = false;
                knTruoc[i] = TongKn(bots[i].Cap);
            }
            dir.enabled = true;
            dir.SinhDotQuanhNguoi();
            float t0 = Time.time; int lanDot = 1;
            while (Time.time - t0 < 90f)
            {
                if (Time.time - t0 > 25f * lanDot) { dir.SinhDotQuanhNguoi(); lanDot++; }
                yield return null;
            }
            dir.enabled = false;
            int coKn = 0;
            for (int i = 0; i < 3; i++)
            {
                var dd = bots[i].GetComponent<Damageable>();
                int kn = TongKn(bots[i].Cap) - knTruoc[i];
                if (kn > 0) coKn++;
                int soLoai;
                Ghi("G" + i + ". " + bots[i].name + " he " + MayBot.TenHe(nao[i].he) + ": +" + kn + " kinh nghiem (cap " + bots[i].Cap.Cap + "), " + nao[i].soPhepDaTung + " chieu ("
                    + DemKy(nao[i], out soLoai) + "), binh mau / mana uong " + nao[i].soBinhMauDaUong + " / " + nao[i].soBinhManaDaUong + ", lui " + nao[i].soLanLui + ", nhao lon " + nao[i].soLanNhaoLon
                    + ", mau " + Mathf.RoundToInt(dd.health) + "/" + Mathf.RoundToInt(dd.maxHealth) + (dd.IsDead ? " (DA CHET)" : ""));
            }
            Ghi("G. " + lanDot + " dot quai, " + coKn + "/3 BOT co kinh nghiem ha quai");
            Kiem(coKn == 3, "co BOT khong ha duoc con quai nao trong 90 s");
            yield return Chup("maybot_danh_tran");
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
        File.WriteAllText("PlayTestShots/maybot_danh.txt", bao.ToString());
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
