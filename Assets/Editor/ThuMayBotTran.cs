using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CHAY THU: MAY BOT TRONG TRAN - BUOC 2 (menu 113, 08/10/2026).
///
/// Phong THAT tren Firebase (tai khoan chay thu B la chu phong, doc tu chay-thu-mang.txt), them BOT bang PhongMang.ThemBot,
/// roi vao Act2 dung duong cua game (TranHienTai + KhoiDongTranMang tu dung khi nap man). Ban Editor khong co WebRTC nen
/// day dung la ca "chu phong + BOT, khong ai khac". Kenh mang GIA LAP (KenhTrucTiep.GiaLapMo) de bat goi chu phong gui di.
///   A. BOT co mat: dung so, la BOT (mau do may nay quyet, bang cap RIENG, bo nao BotDieuKhien), dung cho xuat phat theo ghe
///      (ChoXuatPhat tinh doc lap), co trong danh sach nguoi choi cua GameDirector va trong DongBoTran (ChiSoCua).
///   B. Cap rieng: BOT len cap -> mau toi da BOT x1,15; nhan vat cua may va BOT kia khong doi.
///   C. Dot quai sinh quanh MOI nguoi ke ca BOT (nguoi dung chon "tinh nhu nguoi"); BOT ha quai -> kinh nghiem vao bang
///      cua BOT, khong vao cua may; doi chung: may ha quai -> vao cua may.
///   D. Mang: goi trang thai chu phong gui CO ghe BOT dung vi tri; BOT tung phep -> goi phep mang ghe BOT + cap cua BOT;
///      goi cua chinh minh doi lai (echo) khong sinh ban sao BOT.
///   E. Tran DON: may ha BOT 1 -> goi chet (ghe BOT 1, ke ha 0) + may duoc 250 kinh nghiem; BOT 2 ha may -> BOT 2 duoc
///      250; con BOT 2 -> ket tran, ghe thang = BOT 2, ten "BOT ...", BOT bi tat dieu khien. Khong ghi thanh tich (dang xuat).
///   F. Tran DOI: may + BOT (doi A) vs BOT (doi B): doi dung, don cua dong doi khong gay sat thuong, ha BOT doi B -> doi A thang.
/// Ket qua PlayTestShots/maybot_tran.txt, anh maybot_tran_*.png. Don sach phong thu.
/// </summary>
public static class ThuMayBotTran
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";
    static readonly List<string> phongThu = new List<string>();
    static readonly List<KeyValuePair<int, string>> daGui = new List<KeyValuePair<int, string>>();

    [MenuItem("Diablo 2.5D/113. Chay thu MAY BOT trong tran (buoc 2)", false, 206)]
    public static void Chay()
    {
        if (!ThongTinChayThu.DocHoacBao()) return;
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogError("[BotTran] scene co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien); PlayerPrefs.Save();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (EditorSceneManager.GetActiveScene().name != "MainMenu") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        bao.Length = 0; loi = 0; daBatDau = false; phongThu.Clear();
        FirebaseMang.Quen();
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_BotTran");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BotTran] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator DangNhapB()
    {
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong dang nhap duoc B: " + e);
    }

    /// <summary>Tao phong (chu phong B), them BOT, roi vao Act2 dung duong cua game. Tra ve so BOT da dung.</summary>
    static IEnumerator VaoTranVoiBot(string cheDo, int[] doiBot, System.Action<int> xong)
    {
        bool ok = false; string e = null;
        yield return PhongMang.TaoPhong(cheDo == CheDoTran.Doi ? "Thử BOT đôi" : "Thử BOT đơn", cheDo, (o, err) => { ok = o; e = err; });
        Kiem(ok, "khong tao duoc phong: " + e);
        if (!ok) { xong(0); yield break; }
        string ma = PhongMang.PhongHienTai.ma;
        phongThu.Add(ma);
        for (int i = 0; i < doiBot.Length; i++)
            yield return PhongMang.ThemBot(i % 2 == 0 ? MayBot.Kho : MayBot.Thuong, doiBot[i], (o, err) => { });
        yield return PhongMang.TaiLaiPhong(ma, (o, err) => { });

        TranHienTai.MaPhong = ma;
        TranHienTai.ManChoi = PhongMang.ManMacDinh;
        TranHienTai.LaHost = true;
        TranHienTai.DangChoiMang = true;
        KenhTrucTiep.Dong();
        KenhTrucTiep.guiSangKenh = null;
        SceneManager.LoadScene(PhongMang.ManMacDinh);

        float han = Time.realtimeSinceStartup + 40f;
        while (Time.realtimeSinceStartup < han && KhoiDongTranMang.BotDaDung.Count < doiBot.Length) yield return null;
        for (int i = 0; i < 10; i++) yield return null;
        // Phep thu buoc 2 do BOT DUNG YEN (tu buoc 3 BOT tu di lai - di thi lam nhieu so do vi tri / ket tran)
        foreach (var bot in KhoiDongTranMang.BotDaDung)
        { var nao = bot != null ? bot.GetComponent<BotDieuKhien>() : null; if (nao != null) nao.dungYen = true; }
        xong(KhoiDongTranMang.BotDaDung.Count);
    }

    static PlayerController ToiCua()
    {
        var dir = GameDirector.Instance;
        return dir != null && dir.player != null ? dir.player.GetComponent<PlayerController>() : null;
    }

    static void Giet(Damageable nan, Damageable ke)
    {
        if (nan == null) return;
        if (ke != null) nan.GhiKeDanh(ke);
        nan.TakeDamage(nan.maxHealth * 50f + 99999f, DamageType.Physical, nan.transform.position + Vector3.up);
    }

    static List<byte[]> GoiDaGui(byte loai)
    {
        var ra = new List<byte[]>();
        foreach (var kv in daGui)
        {
            var b = GoiTin.TuChuoi(kv.Value);
            if (b != null && GoiTin.LoaiCuaGoi(b) == loai) ra.Add(b);
        }
        return ra;
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] may BOT trong tran (buoc 2) - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        yield return DangNhapB();
        if (loi > 0) { yield return Ket(); yield break; }
        yield return PhongMang.DoDongHoMayChu();

        // ============ TRAN DON: chu phong + 2 BOT ============
        int soBot = 0;
        yield return VaoTranVoiBot(CheDoTran.Don, new int[] { CheDoTran.KhongDoi, CheDoTran.KhongDoi }, n => soBot = n);
        var toi = ToiCua();
        var dongBo = Object.FindAnyObjectByType<DongBoTran>();
        var ket = KetTran.Hien;
        Ghi("A0. vao Act2: " + soBot + " BOT dung, nhan vat cua may " + (toi != null) + ", DongBoTran " + (dongBo != null) + ", KetTran " + (ket != null)
            + ", trang thai \"" + KhoiDongTranMang.TrangThai + "\"");
        Kiem(soBot == 2 && toi != null && dongBo != null && ket != null, "khong dung du BOT / bo dong bo / ket tran");
        if (soBot < 2 || toi == null || dongBo == null) { yield return Ket(); yield break; }

        var b1 = KhoiDongTranMang.BotDaDung[0];
        var b2 = KhoiDongTranMang.BotDaDung[1];
        var n1 = b1.GetComponent<BotDieuKhien>();
        var n2 = b2.GetComponent<BotDieuKhien>();
        var d0 = toi.GetComponent<Damageable>();
        var d1 = b1.GetComponent<Damageable>();
        var d2 = b2.GetComponent<Damageable>();

        // ---- A. BOT co mat ----
        var dir = GameDirector.Instance;
        // Cho xuat phat: do bang LUAT cua ChoXuatPhat (moi cap nguoi cach nhau >= CachNhauToiThieu), khong tinh lai danh
        // sach - ChoXuatPhat kiem vuong vat can bang vat ly nen tinh lai luc da co nguoi dung thi ra danh sach khac.
        var caBa = new[] { toi.transform, b1.transform, b2.transform };
        float ganNhatCap = float.MaxValue;
        for (int i = 0; i < 3; i++)
            for (int j = i + 1; j < 3; j++)
                ganNhatCap = Mathf.Min(ganNhatCap, Vector2.Distance(new Vector2(caBa[i].position.x, caBa[i].position.z), new Vector2(caBa[j].position.x, caBa[j].position.z)));
        Ghi("A1a. cap gan nhat trong (may, BOT 1, BOT 2) cach nhau " + ganNhatCap.ToString("F1") + " m (luat >= " + ChoXuatPhat.CachNhauToiThieu + " m)");
        Kiem(ganNhatCap >= ChoXuatPhat.CachNhauToiThieu - 0.5f, "BOT xuat phat qua gan nguoi khac");
        foreach (var b in new[] { b1, b2 })
        {
            var n = b.GetComponent<BotDieuKhien>();
            var d = b.GetComponent<Damageable>();
            var bt = b.GetComponent<BangTen>();
            // Dung tren dat: tia xuong lop Ground ngay duoi chan
            RaycastHit hitDat;
            float hoDat = Physics.Raycast(b.transform.position + Vector3.up * 3f, Vector3.down, out hitDat, 30f, LayerMask.GetMask("Ground"))
                ? b.transform.position.y - hitDat.point.y : 99f;
            Ghi("A1. " + b.name + ": ghe " + n.ghe + ", laBot " + b.laBot + ", mau do may khac quyet " + d.mauDoMayKhacQuyet
                + ", bang cap rieng " + (b.Cap != CapDo.CuaMay) + ", do kho " + MayBot.TenDoKho(n.doKho) + " (uid " + n.uid.Substring(0, 6)
                + "), he " + MayBot.TenHe(n.he) + ", ten tren dau \"" + (bt != null ? bt.ten : "?") + "\", cach dat " + hoDat.ToString("F2")
                + " m, cach may " + Vector3.Distance(b.transform.position, toi.transform.position).ToString("F1") + " m, mau " + d.health + "/" + d.maxHealth);
            Kiem(b.laBot && !d.mauDoMayKhacQuyet && b.Cap != CapDo.CuaMay && n.doKho == MayBot.DoKhoCua(n.uid), b.name + " khong phai BOT dung kieu");
            Kiem(Mathf.Abs(hoDat) < 0.6f, b.name + " khong dung tren mat dat");
            Kiem(bt != null && bt.ten.StartsWith("BOT"), b.name + " khong co ten BOT tren dau");
            Kiem(dir.moiNguoi.Contains(b.transform), b.name + " khong co trong danh sach nguoi choi cua GameDirector");
            Kiem(dongBo.LaBotCucBo(n.ghe) && dongBo.ChiSoCua(b.transform) == n.ghe && dongBo.NhanVatCuaGhe(n.ghe) == b, b.name + " chua dang ky vao DongBoTran");
        }
        Kiem(b1.Cap != b2.Cap, "hai BOT dung chung mot bang cap");
        Kiem(KetTran.GheCuaToi == 0 && ket.GheCoTrongPhong != null && ket.GheCoTrongPhong.Count == 3, "ket tran khong tinh du 3 ghe (ke ca BOT)");

        // Anh: may quay nhin BOT 1
        var rig = Object.FindAnyObjectByType<CameraRig>();
        Transform camCu = rig != null ? rig.target : null;
        if (rig != null) rig.target = b1.transform;
        yield return new WaitForSecondsRealtime(1.2f);
        yield return Chup("maybot_tran_bot1");
        if (rig != null) rig.target = camCu;

        // ---- B. Cap rieng ----
        float mauToi0 = d0.maxHealth, mau1Truoc = d1.maxHealth, mau2Truoc = d2.maxHealth;
        int capMay0 = CapDo.Cap;
        b1.Cap.Them(CapDo.CanDeLenCap(1));
        yield return null;
        Ghi("B. BOT 1 len cap: cap " + b1.Cap.Cap + ", mau toi da " + mau1Truoc + " -> " + d1.maxHealth + " (x" + (d1.maxHealth / mau1Truoc).ToString("F3")
            + ") | nhan vat may: cap " + capMay0 + " -> " + CapDo.Cap + ", mau toi da " + mauToi0 + " -> " + d0.maxHealth + " | BOT 2 cap " + b2.Cap.Cap + ", mau " + d2.maxHealth);
        Kiem(b1.Cap.Cap == 2 && Mathf.Abs(d1.maxHealth / mau1Truoc - 1.15f) < 0.001f, "BOT len cap khong cong mau");
        Kiem(CapDo.Cap == capMay0 && Mathf.Approximately(d0.maxHealth, mauToi0), "BOT len cap lam nhan vat cua may doi theo");
        Kiem(b2.Cap.Cap == 1 && Mathf.Approximately(d2.maxHealth, mau2Truoc), "BOT 2 doi theo BOT 1");

        // ---- C. Dot quai quanh moi nguoi + kinh nghiem ha quai ----
        foreach (var q in Object.FindObjectsByType<NhanDangQuai>()) Object.Destroy(q.gameObject);
        yield return null;
        dir.SinhDotQuanhNguoi();
        for (int i = 0; i < 3; i++) yield return null;
        var quai = Object.FindObjectsByType<NhanDangQuai>();
        var nguoi = new[] { toi.transform, b1.transform, b2.transform };
        var dem = new int[3];
        foreach (var q in quai)
        {
            int gan = 0; float g = float.MaxValue;
            for (int k = 0; k < 3; k++) { float dd = Vector3.Distance(q.transform.position, nguoi[k].position); if (dd < g) { g = dd; gan = k; } }
            dem[gan]++;
        }
        Ghi("C1. sinh mot dot: " + quai.Length + " con quai, gan may " + dem[0] + " / BOT 1 " + dem[1] + " / BOT 2 " + dem[2] + " (moi nguoi phai >= 4)");
        Kiem(dem[0] >= 4 && dem[1] >= 4 && dem[2] >= 4, "dot quai khong sinh quanh moi BOT");

        int kn1Truoc = TongKn(b2.Cap), knMayTruoc = TongKn(CapDo.CuaMay), knBot1Truoc = TongKn(b1.Cap);
        Damageable q1 = null, q2 = null;
        foreach (var q in quai) { var dq = q.GetComponent<Damageable>(); if (dq == null || dq.IsDead) continue; if (q1 == null) q1 = dq; else if (q2 == null) { q2 = dq; break; } }
        int knQ1 = q1 != null ? CapDo.KnCuaQuai(q1.GetComponent<NhanDangQuai>().loai) : 0;
        int knQ2 = q2 != null ? CapDo.KnCuaQuai(q2.GetComponent<NhanDangQuai>().loai) : 0;
        if (q1 != null) { q1.tiLeDoDon = 0f; Giet(q1, d2); }
        yield return null;
        if (q2 != null) { q2.tiLeDoDon = 0f; Giet(q2, d0); }
        yield return null;
        int dBot2 = TongKn(b2.Cap) - kn1Truoc, dMay = TongKn(CapDo.CuaMay) - knMayTruoc, dBot1 = TongKn(b1.Cap) - knBot1Truoc;
        Ghi("C2. BOT 2 ha quai (" + knQ1 + " kn): BOT 2 +" + dBot2 + "; doi chung may ha quai (" + knQ2 + " kn): may +" + dMay + "; BOT 1 +" + dBot1 + " (phai 0)");
        Kiem(q1 != null && q2 != null && dBot2 == knQ1 && dMay == knQ2 && dBot1 == 0, "kinh nghiem ha quai khong vao dung bang cap");
        foreach (var q in Object.FindObjectsByType<NhanDangQuai>()) Object.Destroy(q.gameObject);

        // ---- D. Mang (kenh gia lap) ----
        daGui.Clear();
        KenhTrucTiep.GiaLapMo(5);      // kenh cua ghe 5 (trong) - ghe 1 la BOT, kenh ghe BOT khong bao gio mo
        KenhTrucTiep.guiSangKenh = (k, t) => daGui.Add(new KeyValuePair<int, string>(k, t));
        yield return new WaitForSecondsRealtime(0.3f);
        var goiTT = GoiDaGui(GoiTin.LoaiTrangThai);
        var demNhan = new GoiTin.MotNguoi[8];
        int moc, soTrongGoi = goiTT.Count > 0 ? GoiTin.DocTrangThai(goiTT[goiTT.Count - 1], demNhan, out moc) : -1;
        string dsGhe = ""; float lechViTri = 0f; bool coDu = soTrongGoi == 3;
        for (int i = 0; i < soTrongGoi; i++)
        {
            dsGhe += demNhan[i].chiSo + " ";
            var nv = dongBo.NhanVatCuaGhe(demNhan[i].chiSo);
            if (nv == null) { coDu = false; continue; }
            lechViTri = Mathf.Max(lechViTri, Vector3.Distance(nv.transform.position, demNhan[i].viTri));
        }
        Ghi("D1. " + goiTT.Count + " goi trang thai trong 0,3 s; goi cuoi co " + soTrongGoi + " nguoi, ghe [ " + dsGhe + "], lech vi tri lon nhat " + lechViTri.ToString("F3") + " m");
        Kiem(coDu && lechViTri < 0.1f, "goi trang thai chu phong khong mang dung BOT");

        b1.Cap.MoCaDuongChoPhepThu(0);
        b1.Cap.ThemDiemChoPhepThu(2); b1.Cap.NangCap(0); b1.Cap.NangCap(0);
        b1.mana = b1.maxMana;
        int capPhepMay = CapDo.CapCuaKyNang(0);
        daGui.Clear();
        b1.CastAt(0, b1.transform.position + b1.transform.forward * 8f);
        yield return new WaitForSecondsRealtime(1.0f);
        var goiPhep = GoiDaGui(GoiTin.LoaiKyNang);
        GoiTin.MotPhep ph = new GoiTin.MotPhep();
        bool docDuoc = goiPhep.Count > 0 && GoiTin.DocKyNang(goiPhep[0], out ph);
        Ghi("D2. BOT 1 (ghe " + n1.ghe + ", Qua cau lua cap " + b1.Cap.CapCuaKyNang(0) + ") tung phep: " + goiPhep.Count + " goi phep (gui lap 3 lan), goi dau: ghe "
            + (docDuoc ? ph.chiSo.ToString() : "?") + ", ky nang " + (docDuoc ? ph.kyNang.ToString() : "?") + ", cap " + (docDuoc ? ph.capKyNang.ToString() : "?")
            + " | cap Qua cau lua cua nhan vat may: " + capPhepMay);
        Kiem(docDuoc && ph.chiSo == n1.ghe && ph.kyNang == 0 && ph.capKyNang == 3, "goi phep cua BOT sai ghe / sai cap");

        // Echo: goi trang thai cua chinh may doi lai -> khong duoc sinh ban sao BOT
        int banSaoTruoc = DemBanSao();
        if (goiTT.Count > 0) KenhTrucTiep.GiaLapNhan(5, GoiTin.SangChuoi(goiTT[goiTT.Count - 1]));
        for (int i = 0; i < 3; i++) yield return null;
        Ghi("D3. goi trang thai cua minh doi lai qua kenh: ban sao nguoi khac " + banSaoTruoc + " -> " + DemBanSao() + ", SoNguoiKhac " + dongBo.SoNguoiKhac + " (phai 0 / 0)");
        Kiem(DemBanSao() == banSaoTruoc && dongBo.SoNguoiKhac == 0, "goi cua chinh minh sinh ban sao BOT");

        // ---- E. Chet + ket tran DON ----
        FirebaseMang.Quen();          // khong ghi thanh tich tai khoan chay thu
        daGui.Clear();
        int knMayE = TongKn(CapDo.CuaMay), knB2E = TongKn(b2.Cap);
        d1.tiLeDoDon = 0f;
        Giet(d1, d0);
        yield return new WaitForSecondsRealtime(0.5f);
        var goiChet = GoiDaGui(GoiTin.LoaiChet);
        byte gc = 255, gh = 255;
        bool chet1 = goiChet.Count > 0 && GoiTin.DocChet(goiChet[0], out gc, out gh);
        Ghi("E1. may ha BOT 1: BOT 1 chet " + d1.IsDead + ", goi chet (ghe " + gc + ", ke ha " + gh + ") x" + goiChet.Count + ", may +" + (TongKn(CapDo.CuaMay) - knMayE)
            + " kinh nghiem (phai " + CapDo.KnGietNguoi + "), ket tran xong " + KetTran.DaXong + " (phai False - con 2 nguoi)");
        Kiem(d1.IsDead && chet1 && gc == n1.ghe && gh == 0, "chet BOT khong bao ca phong dung");
        Kiem(TongKn(CapDo.CuaMay) - knMayE == CapDo.KnGietNguoi, "ha BOT khong duoc kinh nghiem ha nguoi");
        Kiem(!KetTran.DaXong, "ket tran som khi con 2 nguoi song");

        Giet(d0, d2);
        float han = Time.realtimeSinceStartup + 4f;
        while (!KetTran.DaXong && Time.realtimeSinceStartup < han) yield return null;
        Ghi("E2. BOT 2 ha may: BOT 2 +" + (TongKn(b2.Cap) - knB2E) + " kinh nghiem; ket tran " + KetTran.DaXong + ", ghe thang " + KetTran.GheThang
            + " (BOT 2 ghe " + n2.ghe + "), ten \"" + KetTran.TenNguoiThang + "\", toi thang " + KetTran.ToiThang
            + ", BOT 2 con dieu khien " + b2.enabled + "/" + n2.enabled + ", goi ket tran " + GoiDaGui(GoiTin.LoaiKetTran).Count);
        Kiem(TongKn(b2.Cap) - knB2E == CapDo.KnGietNguoi, "BOT ha nguoi khong duoc kinh nghiem");
        Kiem(KetTran.DaXong && KetTran.GheThang == n2.ghe && KetTran.TenNguoiThang.StartsWith("BOT") && !KetTran.ToiThang, "ket tran Don voi BOT sai");
        Kiem(!b2.enabled && !n2.enabled, "BOT van dieu khien sau ket tran");
        Kiem(GoiDaGui(GoiTin.LoaiKetTran).Count > 0, "khong gui goi ket tran");
        yield return Chup("maybot_tran_ketdon");

        // ============ TRAN DOI: may + BOT (A) vs BOT (B) ============
        KenhTrucTiep.guiSangKenh = null;
        KenhTrucTiep.Dong();
        yield return DangNhapB();
        yield return VaoTranVoiBot(CheDoTran.Doi, new int[] { CheDoTran.DoiA, CheDoTran.DoiB }, n => soBot = n);
        toi = ToiCua();
        d0 = toi != null ? toi.GetComponent<Damageable>() : null;
        PlayerController bA = null, bB = null;
        foreach (var b in KhoiDongTranMang.BotDaDung)
        {
            var d = b.GetComponent<Damageable>();
            if (d.doi == CheDoTran.DoiA) bA = b; else if (d.doi == CheDoTran.DoiB) bB = b;
        }
        Ghi("F1. tran Doi: " + soBot + " BOT, doi may " + (d0 != null ? d0.doi.ToString() : "?") + ", BOT doi A " + (bA != null) + ", BOT doi B " + (bB != null)
            + ", ten tren dau doi " + (bA != null ? bA.GetComponent<BangTen>().doi.ToString() : "?") + "/" + (bB != null ? bB.GetComponent<BangTen>().doi.ToString() : "?"));
        Kiem(soBot == 2 && d0 != null && d0.doi == CheDoTran.DoiA && bA != null && bB != null, "BOT tran Doi sai doi");
        if (bA != null && bB != null && d0 != null)
        {
            FirebaseMang.Quen();
            var dA = bA.GetComponent<Damageable>(); var dB = bB.GetComponent<Damageable>();
            dA.tiLeDoDon = 0f; dB.tiLeDoDon = 0f;
            float mauA = dA.health;
            dA.GhiKeDanh(d0);
            dA.TakeDamage(300f, DamageType.Fire, dA.transform.position);
            Ghi("F2. may danh dong doi BOT doi A 300: mau " + mauA + " -> " + dA.health + " (phai khong doi)");
            Kiem(Mathf.Approximately(dA.health, mauA), "dong doi BOT van an don");
            Giet(dB, d0);
            float han2 = Time.realtimeSinceStartup + 4f;
            while (!KetTran.DaXong && Time.realtimeSinceStartup < han2) yield return null;
            Ghi("F3. ha BOT doi B: ket tran " + KetTran.DaXong + ", doi thang " + KetTran.DoiThang + " (" + KetTran.TenNguoiThang + "), toi thang " + KetTran.ToiThang);
            Kiem(KetTran.DaXong && KetTran.DoiThang == CheDoTran.DoiA && KetTran.ToiThang, "tran Doi voi BOT ket sai");
            yield return Chup("maybot_tran_ketdoi");
        }

        yield return Ket();
    }

    static int TongKn(BangCap b)
    {
        int tong = b.KinhNghiem;
        for (int c = 1; c < b.Cap; c++) tong += CapDo.CanDeLenCap(c);
        return tong;
    }

    static int DemBanSao()
    {
        int n = 0;
        foreach (var pc in Object.FindObjectsByType<PlayerController>())
            if (!pc.laBot && pc.gameObject.name.StartsWith("NguoiChoi_")) n++;
        return n;
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
        KenhTrucTiep.guiSangKenh = null;
        KenhTrucTiep.Dong();
        if (phongThu.Count > 0)
        {
            yield return DangNhapB();
            foreach (var ma in phongThu)
            {
                yield return FirebaseMang.Xoa("phong/" + ma, (o, e) => { });
                yield return FirebaseMang.Xoa("tran/" + ma, (o, e) => { });
                string con = null;
                yield return FirebaseMang.Doc("phong/" + ma, s => con = s);
                bool sach = string.IsNullOrEmpty(con) || con == "null";
                Ghi("don phong " + ma + ": " + (sach ? "sach" : "VAN CON"));
                Kiem(sach, "phong thu con tren Firebase");
            }
        }
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/maybot_tran.txt", bao.ToString());
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
