using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: CHE DO ĐƠN / ĐÔI, TOI DA 6 NGUOI (nguoi dung 28/09/2026). Menu 92.
///
///   A. (khong Play) ham thuan: xep doi khi vao, chuyen doi (toi da 3), chu phong can bang doi, doi thang, doc phong tu JSON
///      (phong cu khong co "cheDo"/"doi"), goi ket tran mang ma doi thang (goi cu mac dinh = Don), dong doi.
///   B. (Play) KY NANG THAT khong dinh dong doi: 10 ky nang tung vao mot muc tieu cung doi -> 0 mau, 0 hieu ung;
///      DOI CHUNG: y het nhung muc tieu doi kia -> phai mat mau (khong thi phep do vo nghia).
///      B2: duong mang that - may nan nhan phat lai goi phep cua dong doi (qua cau lua) -> 0 mau; doi chung doi kia.
///      B3: ky nang tu nham (Lua dia nguc, Qua cau dien) bo qua dong doi dung GAN hon de chon doi thu o xa.
///   C. (Play) cho xuat phat tran Doi tren Act2: dong doi gan nhau, hai doi xa nhau, dung tren dat, moi may ra cung ket qua.
///   D. (Play) KET TRAN DOI qua kenh gia lap 6 ghe: chua du nguoi thi chua xet, doi B chet het -> doi A thang (ca nguoi da
///      nga), goi toi du 5 kenh, may khach doi A / doi B hien dung thang thua, phong mot doi thang ngay; tran Don 6 ghe.
///   E. (Play) ten tren dau theo mau doi + dong "ĐỘI A"; dong doi tren HUD; chup anh.
///
/// Ket qua: PlayTestShots/che_do_doi.txt, anh che_do_doi_*.png.
/// </summary>
public static class ThuCheDoDoi
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    static int soNgoaiLe;

    [MenuItem("Diablo 2.5D/92. Chay thu CHE DO DON - DOI (6 nguoi, dong doi, ket tran doi)", false, 181)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            Debug.LogError("[CheDoDoi] scene dang mo co thay doi chua luu - luu hoac bo truoc da");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false; soNgoaiLe = 0;
        Application.logMessageReceived -= NgheNhatKy;
        Application.logMessageReceived += NgheNhatKy;
        Ghi("[ban 1] che do Don / Doi, toi da 6 nguoi - " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

        KiemHamThuan();

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
        if (GameObject.Find("TAM_CheDoDoi") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_CheDoDoi");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void NgheNhatKy(string chu, string vet, LogType loai)
    {
        if (loai == LogType.Exception) { soNgoaiLe++; bao.AppendLine("[NGOAI LE] " + chu + "\n" + vet); }
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CheDoDoi] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    static PhongMang.NguoiTrongPhong N(string uid, sbyte doi, double vao = 0)
    {
        return new PhongMang.NguoiTrongPhong { uid = uid, ten = uid, doi = doi, vaoLuc = vao };
    }

    // =============================================================
    // A. HAM THUAN
    // =============================================================

    static void KiemHamThuan()
    {
        Ghi("");
        Ghi("A. ham thuan (khong can Play)");

        Kiem(PhongMang.SoNguoiToiDa == 6 && KenhTrucTiep.SoKenhToiDa == 6 && GoiTin.SoGheToiDa == 6,
             "gioi han chua phai 6 o ca ba cho (phong " + PhongMang.SoNguoiToiDa + ", kenh " + KenhTrucTiep.SoKenhToiDa + ", ghe " + GoiTin.SoGheToiDa + ")");

        // A1 xep doi khi vao
        var ds = new List<PhongMang.NguoiTrongPhong>();
        var kq = new List<int>();
        kq.Add(CheDoTran.DoiKhiVao(ds, "x"));                          // rong -> A
        ds.Add(N("h", 0)); kq.Add(CheDoTran.DoiKhiVao(ds, "x"));        // A1 -> B
        ds.Add(N("b1", 1)); kq.Add(CheDoTran.DoiKhiVao(ds, "x"));       // A1 B1 -> A
        ds.Add(N("a2", 0)); ds.Add(N("a3", 0)); kq.Add(CheDoTran.DoiKhiVao(ds, "x"));   // A3 B1 -> B
        Ghi("A1. vao phong Doi: rong -> " + kq[0] + ", A1 -> " + kq[1] + ", A1 B1 -> " + kq[2] + ", A3 B1 -> " + kq[3] + " (phai 0,1,0,1)");
        Kiem(kq[0] == 0 && kq[1] == 1 && kq[2] == 0 && kq[3] == 1, "xep doi khi vao sai");

        // A2 chuyen doi toi da 3
        bool duocSangA = CheDoTran.ChuyenDuoc(ds, "b1", 0);           // A da 3
        bool duocSangB = CheDoTran.ChuyenDuoc(ds, "a2", 1);           // B moi 1
        Ghi("A2. A du 3: b1 sang A = " + duocSangA + " (phai False), a2 sang B = " + duocSangB + " (phai True)");
        Kiem(!duocSangA && duocSangB, "gioi han 3 nguoi moi doi sai");

        // A3 can bang: 4 nguoi doi B (hai nguoi vao cung luc), 1 nguoi thieu doi
        var ds3 = new List<PhongMang.NguoiTrongPhong>
        {
            N("host", 0, 1), N("k1", 1, 2), N("k2", 1, 3), N("k3", 1, 4), N("k4", 1, 5), N("cu", -1, 6)
        };
        var cb = CheDoTran.CanBangDoi(ds3);
        var sau = new Dictionary<string, sbyte>();
        foreach (var n in ds3) sau[n.uid] = n.doi;
        foreach (var kv in cb) sau[kv.Key] = kv.Value;
        int a = 0, b = 0; foreach (var kv in sau) { if (kv.Value == 0) a++; else if (kv.Value == 1) b++; }
        var sb = new StringBuilder(); foreach (var kv in cb) sb.Append(kv.Key + "->" + kv.Value + " ");
        Ghi("A3. can bang (A1, B4, 1 nguoi cu khong doi): chuyen " + sb + "=> A " + a + ", B " + b + " (phai 3/3, k4 sang A)");
        Kiem(a == 3 && b == 3, "can bang doi khong ra 3/3");
        Kiem(sau["k4"] == 0 && sau["k1"] == 1, "can bang chuyen nham nguoi (phai nguoi vao sau cung)");
        var cbOn = CheDoTran.CanBangDoi(new List<PhongMang.NguoiTrongPhong> { N("h", 0), N("x", 1), N("y", 0) });
        Ghi("A3b. doi chung phong da can: " + cbOn.Count + " nguoi bi chuyen (phai 0)");
        Kiem(cbOn.Count == 0, "phong da can van bi chuyen nguoi");

        // A4 doi thang
        var dt = new[]
        {
            CheDoTran.DoiThang(new sbyte[] { 0, 0, 1, 1 }, new[] { true, false, true, false }),
            CheDoTran.DoiThang(new sbyte[] { 0, 0, 1, 1 }, new[] { false, true, false, false }),
            CheDoTran.DoiThang(new sbyte[] { 0, 1, 1 }, new[] { false, false, true }),
            CheDoTran.DoiThang(new sbyte[] { 0, 1 }, new[] { false, false }),
            CheDoTran.DoiThang(new sbyte[] { 1, 1, 1 }, new[] { false, true, false }),
            CheDoTran.DoiThang(new sbyte[] { 0, 0 }, new[] { false, false }),
        };
        Ghi("A4. doi thang: ca hai con song " + dt[0] + ", B chet het " + dt[1] + ", A chet het " + dt[2]
            + ", chet het " + dt[3] + ", ca phong doi B " + dt[4] + ", ca phong doi A (chet) " + dt[5] + " (phai -2, 0, 1, -1, 1, 0)");
        Kiem(dt[0] == -2 && dt[1] == 0 && dt[2] == 1 && dt[3] == -1 && dt[4] == 1 && dt[5] == 0, "luat doi thang sai");

        // A5 doc phong tu JSON: phong Doi moi, phong cu khong co cheDo/doi
        string moi = "{\"ten\":\"P\",\"hostUid\":\"h\",\"cheDo\":\"doi\",\"toiDa\":6,\"soNguoi\":2,\"trangThai\":\"cho\","
                   + "\"nguoiChoi\":{\"h\":{\"ten\":\"H\",\"sanSang\":true,\"cho\":0,\"doi\":0,\"vaoLuc\":100},"
                   + "\"k\":{\"ten\":\"K\",\"sanSang\":false,\"cho\":1,\"doi\":1,\"vaoLuc\":200}}}";
        string cu = "{\"ten\":\"P\",\"hostUid\":\"h\",\"toiDa\":4,\"soNguoi\":1,\"trangThai\":\"cho\","
                  + "\"nguoiChoi\":{\"h\":{\"ten\":\"H\",\"sanSang\":true,\"cho\":0,\"vaoLuc\":100}}}";
        var pMoi = PhongMang.DocPhong("m", moi);
        var pCu = PhongMang.DocPhong("c", cu);
        var kMoi = pMoi.nguoiChoi.Find(n => n.uid == "k");
        Ghi("A5. doc JSON: phong moi cheDo=" + pMoi.cheDo + " LaDoi=" + pMoi.LaDoi + ", doi k=" + (kMoi != null ? kMoi.doi : -9)
            + ", vaoLuc k=" + (kMoi != null ? kMoi.vaoLuc : -9) + " | phong cu cheDo=" + pCu.cheDo + ", doi=" + pCu.nguoiChoi[0].doi
            + " (phai doi, True, 1, 200 | don, -1)");
        Kiem(pMoi.LaDoi && kMoi != null && kMoi.doi == 1 && kMoi.vaoLuc == 200, "doc phong Doi sai");
        Kiem(!pCu.LaDoi && pCu.nguoiChoi[0].doi == -1, "phong cu khong co cheDo/doi doc sai (doi thieu phai la -1, khong phai A)");

        // A6 goi ket tran
        var k = new GoiTin.KetQua { gheThang = 255, doiThang = GoiTin.MaDoiThang(1), quaiTheoGhe = new byte[] { 1, 2, 3, 4, 5, 6 }, nguoiTheoGhe = new byte[] { 6, 5, 4, 3, 2, 1 } };
        GoiTin.KetQua d;
        bool ok = GoiTin.DocKetTran(GoiTin.VietKetTran(k), out d);
        bool khop = ok && d.doiThang == k.doiThang && GoiTin.DoiTuMa(d.doiThang) == 1;
        for (int i = 0; i < 6; i++) khop = khop && d.quaiTheoGhe[i] == k.quaiTheoGhe[i] && d.nguoiTheoGhe[i] == k.nguoiTheoGhe[i];
        var macDinh = new GoiTin.KetQua { gheThang = 2 };
        Ghi("A6. goi ket tran " + GoiTin.VietKetTran(k).Length + " byte, doi B thang doc lai: " + khop
            + " | KetQua mac dinh: doiThang=" + macDinh.doiThang + " = tran Don? " + (macDinh.doiThang == GoiTin.KhongDoiThang)
            + " | hai doi chet het -> ma " + GoiTin.MaDoiThang(CheDoTran.KhongAiSong) + ", DoiTuMa " + GoiTin.DoiTuMa(GoiTin.MaDoiThang(CheDoTran.KhongAiSong)));
        Kiem(khop, "goi ket tran doi thang doc lai sai");
        Kiem(macDinh.doiThang == GoiTin.KhongDoiThang, "KetQua mac dinh bi hieu nham la tran Doi");
        Kiem(GoiTin.DoiTuMa(GoiTin.MaDoiThang(CheDoTran.KhongAiSong)) == -1, "ma 'khong ai song' doc ra mot doi");
        Kiem(!GoiTin.DocKetTran(new byte[2 + 6 * 2], out d), "goi ket tran thieu byte doi van doc duoc");
    }

    // =============================================================
    //  KICH BAN PLAY
    // =============================================================

    static IEnumerator KichBan()
    {
        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { Loi("khong tim thay nhan vat"); Ket(); yield break; }
        yield return new WaitForSeconds(2f);
        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        DonSachQuai();
        yield return new WaitForSeconds(0.5f);
        DonSachQuai();

        yield return KiemDongDoi(toi);
        yield return KiemChoXuatPhat(toi);
        yield return KiemKetTran(toi);
        yield return KiemTenVaHUD(toi);

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    // =============================================================
    // B. KY NANG KHONG DINH DONG DOI
    // =============================================================

    class DoDon { public float mat; public string hieuUng = ""; }

    static IEnumerator KiemDongDoi(PlayerController toi)
    {
        Ghi("");
        Ghi("B. ky nang that khong dinh dong doi (moi ky nang: muc tieu CUNG doi, roi DOI CHUNG muc tieu doi kia)");
        TranHienTai.DangChoiMang = true;
        CheDoTran.LaTranDoi = true;
        CapDo.BatDauTranMoi();

        Vector3 P = toi.transform.position;
        Vector3 huong = TimHuongTrong(P);
        if (huong == Vector3.zero) { Loi("khong tim duoc huong trong"); yield break; }
        DatCho(toi, P - huong * 40f);

        // (ky nang, ten, khoang cach, giay theo doi)
        var bang = new List<object[]>
        {
            new object[] { 0, "Qua cau lua", 7f, 2.5f },
            new object[] { 1, "Mua bang", 7f, 5.8f },
            new object[] { 2, "Sam set", 7f, 2.5f },
            new object[] { 3, "Loc xoay", 6f, 4.5f },
            new object[] { 4, "Thien thach", 7f, 3.5f },
            new object[] { 6, "Giut set", 6f, 2.0f },
            new object[] { CapDo.KyQuaCauBang, "Qua cau bang", 7f, 2.5f },
            new object[] { CapDo.KyGioLoc, "Gio loc", 6f, 2.5f },
            new object[] { CapDo.KyLuaDiaNguc, "Lua dia nguc", 7f, 3.5f },
            new object[] { CapDo.KyCauDien, "Qua cau dien", 6f, 3.0f },
        };

        int soKyDat = 0;
        foreach (var dong in bang)
        {
            int ky = (int)dong[0]; string ten = (string)dong[1]; float xa = (float)dong[2]; float giay = (float)dong[3];
            CapDo.MoCaDuongChoPhepThu(ky);
            var dd = new DoDon(); var dc = new DoDon();
            yield return TungVao(P, huong, ky, xa, giay, CheDoTran.DoiA, dd);
            yield return TungVao(P, huong, ky, xa, giay, CheDoTran.DoiB, dc);
            bool dat = dd.mat <= 0.01f && dd.hieuUng.Length == 0 && dc.mat > 0.5f;
            if (dat) soKyDat++;
            Ghi(string.Format("B1. {0,-13} dong doi: mat {1,7:F1} mau, hieu ung [{2}] | doi chung doi kia: mat {3,7:F1} mau, hieu ung [{4}]",
                ten, dd.mat, dd.hieuUng.Trim(), dc.mat, dc.hieuUng.Trim()));
            Kiem(dc.mat > 0.5f, ten + ": doi chung (doi kia) khong mat mau - phep do vo nghia");
            Kiem(dd.mat <= 0.01f, ten + ": dong doi van mat mau");
            Kiem(dd.hieuUng.Length == 0, ten + ": dong doi van dinh hieu ung " + dd.hieuUng);
        }
        Ghi("B1. " + soKyDat + "/" + bang.Count + " ky nang dat");

        // B2. Duong mang that: may nan nhan (minh) phat lai goi phep cua dong doi
        Ghi("");
        Ghi("B2. duong mang that: goi phep QUA CAU LUA cua nguoi ghe 1 den may minh, minh la nan nhan");
        DatCho(toi, P);
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.doi = CheDoTran.DoiA;
        var kia = NguoiChoiKhac.Sinh("uid-kia", "Người kia", P + huong * 7f);
        var mauKia = kia.GetComponent<Damageable>();
        var goDb = new GameObject("TAM_DongBo");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        if (chay != null) chay.StartCoroutine(GiuSong());
        yield return new WaitForSeconds(0.4f);
        float[] matB2 = new float[2];
        for (int lan = 0; lan < 2; lan++)
        {
            mauKia.doi = lan == 0 ? CheDoTran.DoiA : CheDoTran.DoiB;
            TangHinh.XoaHieuUngDangDinh(toi.gameObject);
            mauToi.health = mauToi.maxHealth;
            yield return new WaitForSeconds(0.6f);
            float truoc = mauToi.health, thap = truoc;
            NhetGoiPhep(1, 0, 500 + lan, toi.transform.position);
            float t0 = Time.time;
            while (Time.time - t0 < 2.5f) { thap = Mathf.Min(thap, mauToi.health); yield return null; }
            matB2[lan] = truoc - thap;
        }
        Ghi(string.Format("B2. nguoi ghe 1 CUNG doi ban minh: mat {0:F1} | DOI CHUNG nguoi ghe 1 doi kia: mat {1:F1}", matB2[0], matB2[1]));
        Kiem(matB2[1] > 0.5f, "B2 doi chung khong mat mau - duong mang khong trung, phep do vo nghia");
        Kiem(matB2[0] <= 0.01f, "phep cua dong doi qua mang van tru mau minh");
        TangHinh.XoaHieuUngDangDinh(toi.gameObject);
        mauToi.health = mauToi.maxHealth;
        mauToi.doi = -1;
        KenhTrucTiep.Dong();
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(kia);

        // B3. Tu nham bo qua dong doi dung gan hon
        Ghi("");
        Ghi("B3. ky nang tu nham: dong doi dung GAN (4 m), doi thu dung XA (9 m)");
        DatCho(toi, P - huong * 40f);
        foreach (int ky in new[] { CapDo.KyLuaDiaNguc, CapDo.KyCauDien })
        {
            CapDo.MoCaDuongChoPhepThu(ky);
            float[] matGan = new float[2], matXa = new float[2];
            for (int lan = 0; lan < 2; lan++)
            {
                var ban = TaoNguoiTung(P, CheDoTran.DoiA);
                var gan = TaoMucTieu(P + huong * 4f, lan == 0 ? CheDoTran.DoiA : CheDoTran.DoiB, "Gần");
                var xa = TaoMucTieu(P + huong * 9f, CheDoTran.DoiB, "Xa");
                yield return new WaitForSeconds(0.3f);
                var mg = gan.GetComponent<Damageable>(); var mx = xa.GetComponent<Damageable>();
                float g0 = mg.health, x0 = mx.health, gT = g0, xT = x0;
                ban.mana = ban.maxMana;
                ban.CastAt(ky, P + huong * 6.5f);
                float t0 = Time.time;
                while (Time.time - t0 < 3.2f) { gT = Mathf.Min(gT, mg.health); xT = Mathf.Min(xT, mx.health); yield return null; }
                matGan[lan] = g0 - gT; matXa[lan] = x0 - xT;
                NguoiChoiKhac.Bo(ban); NguoiChoiKhac.Bo(gan); NguoiChoiKhac.Bo(xa);
                DonKyNang();
                yield return new WaitForSeconds(0.3f);
            }
            string ten = ky == CapDo.KyLuaDiaNguc ? "Lua dia nguc" : "Qua cau dien";
            Ghi(string.Format("B3. {0}: gan la DONG DOI -> gan mat {1:F1}, xa mat {2:F1} | DOI CHUNG gan la doi thu -> gan mat {3:F1}",
                ten, matGan[0], matXa[0], matGan[1]));
            Kiem(matGan[1] > 0.5f, ten + ": doi chung (gan la doi thu) khong trung ke gan - phep do vo nghia");
            Kiem(matGan[0] <= 0.01f && matXa[0] > 0.5f, ten + ": khong bo qua dong doi de nham doi thu");
        }

        DatCho(toi, P);
        TranHienTai.DangChoiMang = false;
        CheDoTran.LaTranDoi = false;
    }

    /// <summary>Mot nguoi tung moi (ban sao dung nhu nhan vat cua may nay) - moi lan tung mot nguoi moi, khong phai doi hoi chieu.</summary>
    static PlayerController TaoNguoiTung(Vector3 cho, sbyte doi)
    {
        var ban = NguoiChoiKhac.Sinh("uid-ban", "Người tung", cho);
        var m = ban.GetComponent<Damageable>();
        m.mauDoMayKhacQuyet = false;
        m.doi = doi;
        m.maxHealth = m.health = 100000f;
        return ban;
    }

    static PlayerController TaoMucTieu(Vector3 cho, sbyte doi, string ten)
    {
        var nv = NguoiChoiKhac.Sinh("uid-muc-" + ten, ten, cho);
        var m = nv.GetComponent<Damageable>();
        m.mauDoMayKhacQuyet = false;     // mat mau that tren may nay de do
        m.doi = doi;
        m.maxHealth = m.health = 100000f;
        return nv;
    }

    static IEnumerator TungVao(Vector3 P, Vector3 huong, int ky, float xa, float giay, sbyte doiMucTieu, DoDon kq)
    {
        var ban = TaoNguoiTung(P, CheDoTran.DoiA);
        var muc = TaoMucTieu(P + huong * xa, doiMucTieu, "Mục tiêu");
        var m = muc.GetComponent<Damageable>();
        yield return new WaitForSeconds(0.3f);
        float truoc = m.health, thap = truoc;
        var hu = new HashSet<string>();
        ban.mana = ban.maxMana;
        ban.CastAt(ky, muc.transform.position);
        float t0 = Time.time;
        while (Time.time - t0 < giay)
        {
            if (muc == null) break;
            thap = Mathf.Min(thap, m.health);
            if (muc.GetComponent<BurningEffect>() != null) hu.Add("chay");
            var f = muc.GetComponent<FrozenEffect>(); if (f != null) hu.Add(f.IsFullyFrozen ? "dong-bang" : "cham");
            if (muc.GetComponent<StunnedEffect>() != null) hu.Add("choang");
            if (muc.GetComponent<BiDanhNga>() != null) hu.Add("nga");
            if (muc.GetComponent<BiHatTung>() != null) hu.Add("hat-tung");
            if (muc.GetComponent<WhirledEffect>() != null) hu.Add("cuon");
            if (muc.GetComponent<BiUot>() != null) hu.Add("uot");
            yield return null;
        }
        kq.mat = truoc - thap;
        foreach (var h in hu) kq.hieuUng += h + " ";
        NguoiChoiKhac.Bo(ban);
        if (muc != null) NguoiChoiKhac.Bo(muc);
        DonKyNang();
        yield return new WaitForSeconds(0.4f);
    }

    /// <summary>Xoa phep con dang bay / dang chay (loc xoay, qua cau dien...) giua hai lan do - khong thi no danh sang lan sau.</summary>
    static void DonKyNang()
    {
        foreach (var x in Object.FindObjectsByType<Fireball>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<QuaCauBang>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<Tornado>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<GioLoc>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<QuaCauDien>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<IceStorm>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<VungLua>()) Object.Destroy(x.gameObject);
        // Manh bang Mua bang dang roi do (Mua bang keo 5 s): nguoi tung da bi xoa -> boQua null -> roi trung muc tieu LUOT SAU
        // (lan chay dau: "Sam set dong doi mat 52 mau, cham" - chinh la manh bang sot lai)
        foreach (var x in Object.FindObjectsByType<FallingShard>()) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<LightningStrike>()) Object.Destroy(x.gameObject);
    }

    // =============================================================
    // C. CHO XUAT PHAT
    // =============================================================

    static IEnumerator KiemChoXuatPhat(PlayerController toi)
    {
        Ghi("");
        Ghi("C. cho xuat phat tran Doi tren Act2 (6 ghe: A A B B A B)");
        var dir = GameDirector.Instance;
        Vector3 tam = dir != null ? dir.arenaCenter : Vector3.zero;
        float bk = dir != null ? dir.arenaRadius : 62f;
        var doi = new sbyte[] { 0, 0, 1, 1, 0, 1 };
        int soPhong = 0, loiC = 0;
        float xaDongDoiMax = 0f, ganDongDoiMin = 999f, ganHaiDoiMin = 999f;
        foreach (var ma in new[] { "-OaBcDeF001", "-QwErTy2", "-Zz9Yy8Xx7", "-Mn0Pq1Rs2", "-Phong5", "-Phong6", "-Lk3Jh4", "-Ab", "-X1Y2Z3W4", "-Cuoi" })
        {
            int hat = ChoXuatPhat.HatTuMaPhong(ma);
            var cho = ChoXuatPhat.ChoChoDoi(hat, doi, tam, bk);
            var lai = ChoXuatPhat.ChoChoDoi(hat, doi, tam, bk);
            bool giong = cho.Count == lai.Count;
            for (int i = 0; giong && i < cho.Count; i++) giong = (cho[i] - lai[i]).sqrMagnitude < 1e-6f;
            if (!giong) { loiC++; Loi("ma " + ma + ": hai lan tinh ra khac nhau (hai may se dat khac cho)"); }
            for (int i = 0; i < cho.Count; i++)
            {
                float y;
                bool datOk = ChoXuatPhat.DungTrenDat(cho[i], out y) && !ChoXuatPhat.DuoiNuoc(cho[i]) && !ChoXuatPhat.VuongVatCan(cho[i]);
                if (!datOk) { loiC++; Loi("ma " + ma + " ghe " + i + ": cho dung khong hop le " + cho[i].ToString("F1")); }
                for (int j = i + 1; j < cho.Count; j++)
                {
                    float d = new Vector2(cho[i].x - cho[j].x, cho[i].z - cho[j].z).magnitude;
                    if (doi[i] == doi[j]) { xaDongDoiMax = Mathf.Max(xaDongDoiMax, d); ganDongDoiMin = Mathf.Min(ganDongDoiMin, d); }
                    else ganHaiDoiMin = Mathf.Min(ganHaiDoiMin, d);
                }
            }
            soPhong++;
        }
        Ghi(string.Format("C1. {0} ma phong: dong doi cach nhau {1:F1} - {2:F1} m (phai 2,2 - 12), hai doi gan nhat {3:F1} m (phai >= 40), loi cho dung {4}",
            soPhong, ganDongDoiMin, xaDongDoiMax, ganHaiDoiMin, loiC));
        Kiem(ganDongDoiMin >= ChoXuatPhat.DongDoiCachNhau - 0.01f, "dong doi dung sat nhau qua");
        Kiem(xaDongDoiMax <= 2f * ChoXuatPhat.DongDoiGanToiDa + 0.01f, "dong doi xuat phat qua xa nhau");
        Kiem(ganHaiDoiMin >= 40f, "hai doi xuat phat gan nhau qua");

        // Doi chung: cach xep tran Don (moi nguoi >= 22 m)
        var don = ChoXuatPhat.ChoChoCaPhong(ChoXuatPhat.HatTuMaPhong("-OaBcDeF001"), 6, tam, bk);
        float ganDon = 999f;
        for (int i = 0; i < don.Count; i++) for (int j = i + 1; j < don.Count; j++)
                ganDon = Mathf.Min(ganDon, new Vector2(don[i].x - don[j].x, don[i].z - don[j].z).magnitude);
        Ghi(string.Format("C2. DOI CHUNG tran Don cung ma phong: hai nguoi gan nhat {0:F1} m (xa hon han dong doi tran Doi)", ganDon));
        Kiem(ganDon > xaDongDoiMax, "doi chung tran Don khong khac tran Doi - phep do khong phan biet");
        yield return null;
    }

    // =============================================================
    // D. KET TRAN DOI (kenh gia lap)
    // =============================================================

    class MotMay
    {
        public GameObject go;
        public DongBoTran dongBo;
        public KetTran ket;
        public List<KeyValuePair<int, string>> daGui = new List<KeyValuePair<int, string>>();
    }

    static readonly sbyte[] DoiGhe = { 0, 0, 1, 1, 0, 1 };      // A: 0 1 4, B: 2 3 5

    static MotMay DungMay(PlayerController toi, byte gheToi, bool laHost, int[] kenhMo, sbyte[] doiGhe, bool laDoi)
    {
        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = laHost;
        CheDoTran.LaTranDoi = laDoi;
        KetTran.Xoa();
        KenhTrucTiep.Dong();
        foreach (int k in kenhMo) KenhTrucTiep.GiaLapMo(k);

        var m = new MotMay();
        KenhTrucTiep.guiSangKenh = (k, t) => m.daGui.Add(new KeyValuePair<int, string>(k, t));
        m.go = new GameObject("TAM_May_" + gheToi);
        m.dongBo = m.go.AddComponent<DongBoTran>();
        m.dongBo.toi = toi;
        m.dongBo.chiSoCuaToi = gheToi;
        var quai = m.go.AddComponent<DongBoQuai>();
        quai.dongBo = m.dongBo; m.dongBo.quai = quai;
        m.dongBo.TaoNguoiKhiCan = (ghe, cho) =>
        {
            var nv = NguoiChoiKhac.Sinh("uid-" + ghe, "Người " + (ghe + 1), cho);
            if (nv != null && laDoi) KhoiDongTranMang.GanDoi(nv, nv.GetComponent<BangTen>(), doiGhe[ghe]);
            return nv;
        };
        m.ket = m.go.AddComponent<KetTran>();
        m.ket.TenCuaGhe = ghe => "Người " + (ghe + 1);
        m.ket.DoiCuaGhe = ghe => laDoi && ghe < doiGhe.Length ? doiGhe[ghe] : CheDoTran.KhongDoi;
        m.ket.GheCoTrongPhong = new List<byte>();
        for (byte g = 0; g < doiGhe.Length; g++) m.ket.GheCoTrongPhong.Add(g);
        m.ket.Gan(m.dongBo, toi, gheToi);
        toi.GetComponent<Damageable>().doi = laDoi ? doiGhe[gheToi] : CheDoTran.KhongDoi;
        return m;
    }

    static void DonMay(MotMay m)
    {
        if (m == null) return;
        if (m.dongBo != null)
            for (byte g = 0; g < GoiTin.SoGheToiDa; g++)
            {
                var nv = m.dongBo.NhanVatCuaGhe(g);
                if (nv != null && nv != m.dongBo.toi) Object.DestroyImmediate(nv.gameObject);
            }
        if (m.go != null) Object.DestroyImmediate(m.go);
        KenhTrucTiep.guiSangKenh = null;
        KenhTrucTiep.Dong();
    }

    static string GoiTrangThai(MotMay m, byte ghe, Vector3 cho, bool daChet)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi { chiSo = ghe, viTri = cho, gocY = 0f, mau01 = daChet ? 0f : 1f, dangChay = false, daChet = daChet, coHieuUng = 0, khieng01 = 0f };
        return GoiTin.SangChuoi(GoiTin.VietTrangThai(m.dongBo.GioTran(), ds, 1));
    }

    static int DemGoi(MotMay m, byte loai, out HashSet<int> kenh, out byte[] cuoi)
    {
        kenh = new HashSet<int>(); cuoi = null; int n = 0;
        foreach (var cap in m.daGui)
        {
            var b = GoiTin.TuChuoi(cap.Value);
            if (GoiTin.LoaiCuaGoi(b) != loai) continue;
            n++; kenh.Add(cap.Key); cuoi = b;
        }
        return n;
    }

    static IEnumerator KiemKetTran(PlayerController toi)
    {
        Ghi("");
        Ghi("D. ket tran Doi qua kenh gia lap (A: ghe 0 1 4, B: ghe 2 3 5; minh la chu phong ghe 0)");
        var mauToi = toi.GetComponent<Damageable>();
        Vector3 P = toi.transform.position;

        var host = DungMay(toi, 0, true, new[] { 1, 2, 3, 4, 5 }, DoiGhe, true);
        yield return null;
        // Chi ghe 1, 2 vao truoc
        KenhTrucTiep.GiaLapNhan(1, GoiTrangThai(host, 1, P + new Vector3(3f, 0f, 0f), false));
        KenhTrucTiep.GiaLapNhan(2, GoiTrangThai(host, 2, P + new Vector3(-3f, 0f, 0f), false));
        yield return null;
        KenhTrucTiep.GiaLapNhan(2, GoiTin.SangChuoi(GoiTin.VietChet(2, 0)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        Ghi("D1. moi 3/6 ghe vao tran, ghe 2 (B) chet -> tran xong: " + KetTran.DaXong + " (phai False: ghe 3, 5 dang tai man)");
        Kiem(!KetTran.DaXong, "chua du nguoi vao ma da ket tran");

        for (byte g = 3; g <= 5; g++) KenhTrucTiep.GiaLapNhan(g, GoiTrangThai(host, g, P + new Vector3(g * 2f, 0f, 4f), false));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        Ghi("D2. du 6 ghe; HUD: A " + KetTran.SoSongDoi[0] + "/" + KetTran.SoNguoiDoi[0] + " con song, B " + KetTran.SoSongDoi[1] + "/" + KetTran.SoNguoiDoi[1]
            + " (phai A 3/3, B 2/3); tran xong: " + KetTran.DaXong + " (phai False)");
        Kiem(KetTran.SoSongDoi[0] == 3 && KetTran.SoNguoiDoi[0] == 3 && KetTran.SoSongDoi[1] == 2 && KetTran.SoNguoiDoi[1] == 3, "dem nguoi moi doi sai");
        Kiem(!KetTran.DaXong, "hai doi con nguoi song ma da ket tran");

        // Ghe 1 (A) chet, ghe 3 (B) chet: con A (0, 4) va B (5)
        KenhTrucTiep.GiaLapNhan(1, GoiTin.SangChuoi(GoiTin.VietChet(1, 5)));
        KenhTrucTiep.GiaLapNhan(3, GoiTin.SangChuoi(GoiTin.VietChet(3, 4)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        Kiem(!KetTran.DaXong, "doi B con ghe 5 song ma da ket tran");
        host.daGui.Clear();
        KenhTrucTiep.GiaLapNhan(5, GoiTin.SangChuoi(GoiTin.VietChet(5, 0)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.6f);
        HashSet<int> kenh; byte[] goi;
        int so = DemGoi(host, GoiTin.LoaiKetTran, out kenh, out goi);
        var kq = default(GoiTin.KetQua);
        bool doc = goi != null && GoiTin.DocKetTran(goi, out kq);
        var dsKenh = new List<int>(kenh); dsKenh.Sort();
        Ghi("D3. doi B chet het (A con ghe 0, 4) -> xong " + KetTran.DaXong + ", doi thang " + KetTran.DoiThang + " (" + KetTran.TenNguoiThang
            + "), toi thang " + KetTran.ToiThang + "; goi ket tran " + so + " lan toi kenh " + string.Join(",", dsKenh)
            + ", ma doi trong goi " + (doc ? kq.doiThang.ToString() : "?"));
        Kiem(KetTran.DaXong && KetTran.DoiThang == CheDoTran.DoiA && KetTran.ToiThang, "doi A khong thang");
        Kiem(dsKenh.Count == 5, "goi ket tran khong toi du 5 kenh (6 nguoi)");
        Kiem(doc && GoiTin.DoiTuMa(kq.doiThang) == CheDoTran.DoiA, "goi ket tran mang sai doi thang");
        Kiem(KetTran.TenNguoiThang == "ĐỘI A", "ten doi thang sai: " + KetTran.TenNguoiThang);
        toi.enabled = true;
        yield return Chup("che_do_doi_ket_tran_thang");
        DonMay(host);
        yield return null;

        // D4. May khach: doi A (ghe 1, da chet giua tran) va doi B (ghe 2) nghe cung mot goi
        foreach (byte ghe in new byte[] { 1, 2 })
        {
            var khach = DungMay(toi, ghe, false, new[] { 0 }, DoiGhe, true);
            yield return null;
            KenhTrucTiep.GiaLapNhan(0, goi != null ? GoiTin.SangChuoi(goi) : "");
            yield return new WaitForSeconds(0.2f);
            Ghi("D4. may khach ghe " + ghe + " (" + CheDoTran.TenDoi(DoiGhe[ghe]) + ") nghe goi ket tran -> toi thang " + KetTran.ToiThang
                + " (phai " + (DoiGhe[ghe] == 0) + ")");
            Kiem(KetTran.DaXong && KetTran.ToiThang == (DoiGhe[ghe] == 0), "may khach ghe " + ghe + " hien sai thang thua");
            if (ghe == 2) { toi.enabled = true; yield return Chup("che_do_doi_ket_tran_thua"); }
            toi.enabled = true;
            DonMay(khach);
            yield return null;
        }

        // D5. Ca phong mot doi -> thang ngay khi du nguoi
        var motDoi = new sbyte[] { 1, 1, 1 };
        var h2 = DungMay(toi, 0, true, new[] { 1, 2 }, motDoi, true);
        h2.ket.GheCoTrongPhong = new List<byte> { 0, 1, 2 };
        yield return null;
        KenhTrucTiep.GiaLapNhan(1, GoiTrangThai(h2, 1, P + new Vector3(3f, 0f, 0f), false));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        bool xongSom = KetTran.DaXong;
        KenhTrucTiep.GiaLapNhan(2, GoiTrangThai(h2, 2, P + new Vector3(-3f, 0f, 0f), false));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.4f);
        Ghi("D5. ca phong doi B (3 nguoi): truoc khi du nguoi xong " + xongSom + " (phai False), du nguoi -> xong " + KetTran.DaXong
            + ", doi thang " + KetTran.DoiThang + " (phai 1), toi thang " + KetTran.ToiThang);
        Kiem(!xongSom && KetTran.DaXong && KetTran.DoiThang == CheDoTran.DoiB && KetTran.ToiThang, "phong mot doi khong thang ngay");
        toi.enabled = true;
        DonMay(h2);
        yield return null;

        // D6. Tran DON 6 ghe: nguoi cuoi cung (ghe 5) thang
        var h3 = DungMay(toi, 0, true, new[] { 1, 2, 3, 4, 5 }, new sbyte[] { -1, -1, -1, -1, -1, -1 }, false);
        yield return null;
        for (byte g = 1; g <= 5; g++) KenhTrucTiep.GiaLapNhan(g, GoiTrangThai(h3, g, P + new Vector3(g * 2f, 0f, -4f), false));
        yield return null;
        for (byte g = 1; g <= 4; g++) KenhTrucTiep.GiaLapNhan(g, GoiTin.SangChuoi(GoiTin.VietChet(g, 5)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        bool conHai = !KetTran.DaXong;
        KenhTrucTiep.GiaLapNhan(5, GoiTin.SangChuoi(GoiTin.VietChet(5, 0)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.6f);
        Ghi("D6. tran DON 6 nguoi: con 2 nguoi (ghe 0, 5) -> xong " + !conHai + " (phai False); ghe 5 chet -> ghe thang " + KetTran.GheThang
            + " (phai 0), doi thang " + KetTran.DoiThang + " (phai -1), ghe 5 ha " + KetTran.BangDiem.nguoiTheoGhe[5] + " nguoi (phai 4), ma doi "
            + KetTran.BangDiem.doiThang + " (phai 0 = Don)");
        Kiem(conHai && KetTran.DaXong && KetTran.GheThang == 0 && KetTran.ToiThang && KetTran.DoiThang == -1, "tran Don 6 nguoi phan quyet sai");
        Kiem(KetTran.BangDiem.nguoiTheoGhe[5] == 4, "bang diem ghe 5 (ghe thu sau) sai");
        Kiem(KetTran.BangDiem.doiThang == GoiTin.KhongDoiThang, "tran Don mang ma doi thang");
        toi.enabled = true;
        DonMay(h3);
        TranHienTai.Xoa();
        yield return null;
    }

    // =============================================================
    // E. TEN TREN DAU + HUD
    // =============================================================

    static IEnumerator KiemTenVaHUD(PlayerController cu)
    {
        Ghi("");
        Ghi("E. ten tren dau theo mau doi + dong doi tren HUD");
        var toi = cu;
        if (toi == null) { Loi("khong co nhan vat de chup ten"); yield break; }

        var cam = Camera.main;
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.target = toi.transform;
        Vector3 P = toi.transform.position;
        Vector3 huong = TimHuongTrong(P);
        if (huong == Vector3.zero) huong = Vector3.forward;
        Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;

        var host = DungMay(toi, 0, true, new[] { 1, 2, 3 }, DoiGhe, true);
        var bt = BangTen.Gan(toi.gameObject, "Tôi", true);
        KhoiDongTranMang.GanDoi(toi, bt, CheDoTran.DoiA);
        yield return null;
        KenhTrucTiep.GiaLapNhan(1, GoiTrangThai(host, 1, P + huong * 4f + ngang * 2.5f, false));
        KenhTrucTiep.GiaLapNhan(2, GoiTrangThai(host, 2, P + huong * 6f - ngang * 2.5f, false));
        KenhTrucTiep.GiaLapNhan(3, GoiTrangThai(host, 3, P + huong * 8f + ngang * 0.5f, false));
        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        yield return new WaitForSeconds(1.2f);

        var ds = new List<string>();
        int dung = 0;
        for (byte g = 1; g <= 3; g++)
        {
            var nv = host.dongBo.NhanVatCuaGhe(g);
            var b = nv != null ? nv.GetComponent<BangTen>() : null;
            if (b == null) { Loi("ghe " + g + " khong co bang ten"); continue; }
            Color mongDoi = CheDoTran.MauDoi(DoiGhe[g]);
            bool mauDung = b.khungVeCuoi >= Time.frameCount - 5 && Mathf.Abs(b.mauCuoi.r - mongDoi.r) < 0.02f && Mathf.Abs(b.mauCuoi.b - mongDoi.b) < 0.02f;
            bool chuDoiTren = b.oDoiCuoi.height > 0f && b.oDoiCuoi.yMax <= b.oCuoi.y + 6f;
            if (mauDung && chuDoiTren && b.doi == DoiGhe[g]) dung++;
            ds.Add("ghe " + g + " doi " + b.doi + " mau (" + b.mauCuoi.r.ToString("F2") + "," + b.mauCuoi.g.ToString("F2") + "," + b.mauCuoi.b.ToString("F2") + ")"
                   + (chuDoiTren ? " + chu doi tren ten" : " [THIEU chu doi]"));
        }
        Ghi("E1. " + string.Join(" | ", ds));
        Kiem(dung == 3, "ten tren dau khong theo mau doi / thieu dong ĐỘI A/B");
        var mauDauToi = toi.GetComponent<Damageable>();
        Ghi("E1b. dong doi cua minh (ghe 1) va minh: LaDongDoi = " + CheDoTran.LaDongDoi(mauDauToi, host.dongBo.NhanVatCuaGhe(1).GetComponent<Damageable>())
            + " | voi ghe 2 (doi kia): " + CheDoTran.LaDongDoi(mauDauToi, host.dongBo.NhanVatCuaGhe(2).GetComponent<Damageable>()));
        Kiem(CheDoTran.LaDongDoi(mauDauToi, host.dongBo.NhanVatCuaGhe(1).GetComponent<Damageable>())
             && !CheDoTran.LaDongDoi(mauDauToi, host.dongBo.NhanVatCuaGhe(2).GetComponent<Damageable>()), "ban sao mang khong mang dung doi");

        yield return new WaitForSeconds(KetTran.NhipDem + 0.2f);
        Ghi("E2. HUD dong doi: A " + KetTran.SoSongDoi[0] + "/" + KetTran.SoNguoiDoi[0] + ", B " + KetTran.SoSongDoi[1] + "/" + KetTran.SoNguoiDoi[1]);
        yield return Chup("che_do_doi_ten_hud");

        DonMay(host);
        TranHienTai.Xoa();
    }

    // =============================================================
    //  TIEN ICH
    // =============================================================

    static int DonSachQuai()
    {
        int n = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include))
        { Object.DestroyImmediate(q.gameObject); n++; }
        return n;
    }

    static Vector3 TimHuongTrong(Vector3 P)
    {
        int mask = LayerMask.GetMask("Default", "Ground", "Enemy");
        for (int goc = 0; goc < 360; goc += 10)
        {
            Vector3 h = Quaternion.AngleAxis(goc, Vector3.up) * Vector3.forward;
            bool trong = true;
            foreach (float lech in new[] { -14f, 0f, 14f })
            {
                Vector3 hl = Quaternion.AngleAxis(lech, Vector3.up) * h;
                Vector3 tu = P + Vector3.up * 1.4f;
                Vector3 den = P + hl * 11f + Vector3.up * 0.9f;
                if (Physics.SphereCast(tu, 0.5f, (den - tu).normalized, out _, (den - tu).magnitude, mask, QueryTriggerInteraction.Collide))
                { trong = false; break; }
            }
            if (trong) return h;
        }
        return Vector3.zero;
    }

    static void DatCho(PlayerController pc, Vector3 cho)
    {
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = cho;
        if (cc != null) cc.enabled = true;
    }

    static IEnumerator GiuSong()
    {
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBo") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static void NhetGoiPhep(byte chiSo, byte kyNang, int soThuTu, Vector3 ngam)
    {
        byte[] b = GoiTin.VietKyNang(new GoiTin.MotPhep { chiSo = chiSo, kyNang = kyNang, soThuTu = soThuTu, diemNgam = ngam, capKyNang = 1 });
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(b));
    }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
        Ghi("anh: " + d);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangKenh = null;
        KenhTrucTiep.Dong();
        Ghi("tong so ngoai le: " + soNgoaiLe);
        if (soNgoaiLe > 0) { bao.AppendLine("[LOI] co ngoai le - tren WebGL la game dung hinh"); loi++; }
        Application.logMessageReceived -= NgheNhatKy;
        File.WriteAllText("PlayTestShots/che_do_doi.txt", bao.ToString());
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t != null && t.parent == null && t.name.StartsWith("TAM_")) Object.Destroy(t.gameObject);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[CheDoDoi] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }
}
