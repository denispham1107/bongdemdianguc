using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: HAI KY NANG BINH MAU / BINH MANA, BINH ROI KHI GIET QUAI, SACH PHEP CHU TO + CUON.
///
/// Nguoi dung xin (13/09/2026):
///   - Chu cot trai Sach phep to them 15%, chu phan chi tiet to them 20%; hai vung
///     cuon len xuong duoc.
///   - Them hai ky nang Binh mau / Binh mana: mo khoa 1 diem, cap toi da 1; chi dung
///     khi da nhat binh; giet quai 10% ra binh mau, 10% ra binh mana; toi gan thi binh
///     tu bay vao nguoi; so binh hien tren o ky nang; 1 binh hoi toi da 100 mau / 50
///     nang luong; cho 0,5 giay. Choi nhieu nguoi binh la CUA CHUNG ca phong.
///
/// Do bang SO:
///   A. Ti le roi (3000 lan gieo) + quai that chet thi binh roi dung cho quai chet.
///   B. Nhat mot minh: ngoai ban kinh khong bay, trong ban kinh bay vao, so binh +1.
///   C. Uong: khoa / het binh / day / hoi chieu / hoi dung 100 - 50 / thieu it hoi dung phan thieu.
///   D. So binh tren o: anh chup o binh DOI khi so binh doi, o doi chung khong doi.
///   E. Chu to (do be ngang net chu tren anh chup so voi chu doi chung ve o co cu /
///      co moi) va cuon phan than chi tiet (than doi, dau muc dung yen).
///   F. Mang (kenh gia lap, bo dong bo that): may khach nhan binh, xin, duoc giao;
///      nguoi khac duoc giao thi minh khong cong; chu phong giao cho nguoi XIN TRUOC,
///      loi xin thu hai bi bo; goi "thuoc ve" den truoc goi "roi ra".
///
/// Ket qua ghi ra <c>PlayTestShots/binh_mau_mana.txt</c>.
/// </summary>
public static class ThuBinhMauMana
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/65. Chay thu BINH MAU - BINH MANA (roi, nhat, uong, mang)", false, 156)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu binh mau", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] binh mau / binh mana + sach phep chu to, cuon");

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
        if (GameObject.Find("TAM_BinhMau") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_BinhMau");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BinhMau] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            if (!pc.tuDocInput) continue;
            return pc;
        }
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator ChupTex(System.Action<Texture2D> nhan)
    {
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    /// <summary>Do khac nhau trung binh (0..1) cua hai anh trong mot vung (toa do GUI).</summary>
    static float KhacNhau(Texture2D a, Texture2D b, Rect r)
    {
        int x0 = Mathf.Max(0, (int)r.xMin), x1 = Mathf.Min(a.width, (int)r.xMax);
        int y0 = Mathf.Max(0, (int)r.yMin), y1 = Mathf.Min(a.height, (int)r.yMax);
        double tong = 0; int dem = 0;
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
            {
                var c1 = a.GetPixel(x, a.height - 1 - y); var c2 = b.GetPixel(x, b.height - 1 - y);
                tong += (Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b)) / 3f; dem++;
            }
        return dem > 0 ? (float)(tong / dem) : 0f;
    }

    /// <summary>Be ngang net chu: tu diem sang dau tien toi diem sang cuoi cung trong vung.</summary>
    static float BeNgangNet(Texture2D tex, Rect r, float nguong)
    {
        int x0 = Mathf.Max(0, (int)r.xMin), x1 = Mathf.Min(tex.width, (int)r.xMax);
        int y0 = Mathf.Max(0, (int)r.yMin), y1 = Mathf.Min(tex.height, (int)r.yMax);
        int trai = -1, phai = -1;
        for (int x = x0; x < x1; x++)
            for (int y = y0; y < y1; y++)
            {
                var c = tex.GetPixel(x, tex.height - 1 - y);
                if (Mathf.Max(c.r, Mathf.Max(c.g, c.b)) > nguong) { if (trai < 0) trai = x; phai = x; break; }
            }
        return trai < 0 ? 0f : phai - trai + 1;
    }

    static int DemBinh(int ky)
    {
        int n = 0;
        foreach (var b in QuanLyBinhRoi.TatCa) if (b != null && b.Ky == ky) n++;
        return n;
    }

    static IEnumerator DonQuai()
    {
        // Xoa quai (khong giet - giet thi lai gieo binh) de cac phan sau khong bi quay
        while (EditorApplication.isPlaying && GameObject.Find("TAM_BinhMau") != null)
        {
            if (khongDonQuai) { yield return null; continue; }
            foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(q.gameObject);
            yield return new WaitForSeconds(0.3f);
        }
    }
    static bool khongDonQuai;

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }
        yield return new WaitForSeconds(1.0f);

        var pc = TimToi();
        if (pc == null) { Loi("khong tim thay nhan vat"); Ket(); yield break; }
        var mau = pc.GetComponent<Damageable>();
        Kiem(dir.player == pc.transform, "GameDirector.player khong phai nhan vat cua may nay - binh se bay nham nguoi");
        var chay = Object.FindAnyObjectByType<ChayThuMang>();

        // ================================================================
        // A. TI LE ROI
        // ================================================================
        Ghi("");
        Ghi("A. ti le roi binh");
        TranHienTai.DangChoiMang = false;
        QuanLyBinhRoi.DonSach();
        Random.InitState(13092026);
        const int SoLan = 3000;
        Vector3 xa = pc.transform.position + new Vector3(0f, 0f, 45f);
        for (int i = 0; i < SoLan; i++) QuanLyBinhRoi.GieoKhiQuaiChet(xa);
        int nMau = DemBinh(CapDo.KyBinhMau), nMana = DemBinh(CapDo.KyBinhMana);
        Ghi("A1. " + SoLan + " con quai chet: " + nMau + " binh mau (" + (100f * nMau / SoLan).ToString("F1") + "%), "
            + nMana + " binh mana (" + (100f * nMana / SoLan).ToString("F1") + "%) - mong 10% moi loai");
        Kiem(nMau > SoLan * 0.085f && nMau < SoLan * 0.115f, "ti le binh mau lech khoi 10%");
        Kiem(nMana > SoLan * 0.085f && nMana < SoLan * 0.115f, "ti le binh mana lech khoi 10%");
        QuanLyBinhRoi.DonSach();
        yield return null;

        // A2. quai THAT chet (duong GameDirector.OnEnemyDeath)
        mau.maxHealth = 1e7f; mau.health = 1e7f;
        Random.InitState(777);
        dir.SinhDotQuanhNguoi();
        yield return null;
        var viTriChet = new List<Vector3>();
        int soGiet = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            var d = q.GetComponent<Damageable>();
            if (d == null || d.IsDead) continue;
            viTriChet.Add(q.transform.position);
            d.TakeDamage(d.maxHealth * 10f, DamageType.Fire, q.transform.position);
            soGiet++;
        }
        yield return null;
        int roi = QuanLyBinhRoi.SoBinhDangCo, dungCho = 0;
        foreach (var b in QuanLyBinhRoi.TatCa)
        {
            float gan = float.MaxValue;
            foreach (var v in viTriChet) gan = Mathf.Min(gan, new Vector2(b.transform.position.x - v.x, b.transform.position.z - v.z).magnitude);
            if (gan <= 1.0f) dungCho++;
        }
        Ghi("A2. giet that " + soGiet + " con: roi " + roi + " binh, nam dung cho quai chet (<= 1 m) " + dungCho + "/" + roi);
        Kiem(soGiet > 0 && roi > 0, "giet quai that ma khong roi binh nao (GameDirector khong goi gieo binh)");
        Kiem(dungCho == roi, "binh roi khong nam o cho quai chet");
        QuanLyBinhRoi.DonSach();
        if (chay != null) chay.StartCoroutine(DonQuai());
        yield return new WaitForSeconds(0.5f);

        // ================================================================
        // B. NHAT MOT MINH
        // ================================================================
        Ghi("");
        Ghi("B. nhat binh (choi mot minh)");
        CapDo.BatDauTranMoi();
        mau.maxHealth = 600f; mau.health = 600f;
        Vector3 huong = pc.transform.forward; huong.y = 0f; huong.Normalize();
        Vector3 goc = pc.transform.position;
        int id = QuanLyBinhRoi.Roi(CapDo.KyBinhMau, goc + huong * 6f);
        var binh = QuanLyBinhRoi.TimBinh(id);
        yield return new WaitForSeconds(1.0f);
        bool conNguyen = binh != null && !binh.DaCoChu && CapDo.SoBinhMau == 0;
        Ghi("B1. binh cach 6 m (ban kinh hut " + QuanLyBinhRoi.BanKinhHut + " m) sau 1 giay: van nam yen = " + conNguyen);
        Kiem(conNguyen, "binh ngoai ban kinh hut ma da bay vao nguoi");
        yield return Chup("binh_1_nam_dat");

        float t0 = Time.time;
        if (binh != null) binh.transform.position = pc.transform.position + huong * 2.5f;
        float bayMat = -1f;
        while (Time.time - t0 < 3f)
        {
            if (QuanLyBinhRoi.TimBinh(id) == null) { bayMat = Time.time - t0; break; }
            yield return null;
        }
        Ghi("B2. dua binh vao 2,5 m: bay vao nguoi sau " + bayMat.ToString("F2") + " giay, so binh mau = " + CapDo.SoBinhMau);
        Kiem(bayMat >= 0f && CapDo.SoBinhMau == 1, "binh trong ban kinh khong bay vao / khong cong so binh");

        // ================================================================
        // C. UONG
        // ================================================================
        Ghi("");
        Ghi("C. uong binh");
        float max = mau.maxHealth;
        mau.health = max - 250f;
        float hoi = pc.UongBinh(CapDo.KyBinhMau);
        Ghi("C1. CHUA MO KHOA, co 1 binh: hoi " + hoi + ", con " + CapDo.SoBinhMau + " binh");
        Kiem(hoi == 0f && CapDo.SoBinhMau == 1, "chua mo khoa ma van uong duoc binh");

        CapDo.Them(100 + 135);                     // cap 3 = 3 diem
        CapDo.MoKhoa(CapDo.KyBinhMau); CapDo.MoKhoa(CapDo.KyBinhMana);
        bool nangDuoc = CapDo.NangCapDuoc(CapDo.KyBinhMau);
        Ghi("C2. mo khoa: cap binh mau " + CapDo.CapCuaKyNang(CapDo.KyBinhMau) + "/" + CapDo.CapToiDaCua(CapDo.KyBinhMau)
            + ", con " + CapDo.DiemKyNang + " diem, nang cap tiep duoc = " + nangDuoc);
        Kiem(CapDo.DaMo(CapDo.KyBinhMau) && !nangDuoc, "binh mau phai mo khoa duoc va KHONG nang cap duoc (cap toi da 1)");

        CapDo.ThemBinh(CapDo.KyBinhMau); CapDo.ThemBinh(CapDo.KyBinhMau);      // 3 binh
        float truoc = mau.health;
        hoi = pc.UongBinh(CapDo.KyBinhMau);
        Ghi("C3. thieu 250 mau: hoi " + hoi + " (mau " + truoc + " -> " + mau.health + "), con " + CapDo.SoBinhMau + " binh");
        Kiem(Mathf.Abs(hoi - 100f) < 0.01f && Mathf.Abs(mau.health - truoc - 100f) < 0.01f && CapDo.SoBinhMau == 2,
             "mot binh mau khong hoi dung 100");
        hoi = pc.UongBinh(CapDo.KyBinhMau);
        Ghi("C4. uong tiep NGAY: hoi " + hoi + ", con " + CapDo.SoBinhMau + " binh (hoi chieu 0,5 giay)");
        Kiem(hoi == 0f && CapDo.SoBinhMau == 2, "khong co hoi chieu 0,5 giay giua hai binh");
        yield return new WaitForSeconds(0.30f);
        hoi = pc.UongBinh(CapDo.KyBinhMau);
        Kiem(hoi == 0f, "chua het 0,5 giay (moi 0,3) ma da uong duoc");
        yield return new WaitForSeconds(0.30f);
        // LEN CAP O C2 LAM TRAN MAU TANG (600 -> 793,5). Lan chay dau giu "max" tu truoc
        // khi len cap nen dat "thieu 30" thuc ra la thieu 223 - phep thu bao oan.
        max = mau.maxHealth;
        mau.health = max - 30f;
        hoi = pc.UongBinh(CapDo.KyBinhMau);
        Ghi("C5. sau 0,6 giay, thieu 30 mau: hoi " + hoi + ", mau " + mau.health + "/" + max + ", con " + CapDo.SoBinhMau + " binh");
        Kiem(Mathf.Abs(hoi - 30f) < 0.01f && Mathf.Abs(mau.health - max) < 0.01f && CapDo.SoBinhMau == 1,
             "thieu 30 mau ma khong hoi dung 30 (hoi TOI DA 100)");
        yield return new WaitForSeconds(0.6f);
        hoi = pc.UongBinh(CapDo.KyBinhMau);
        Ghi("C6. mau day: hoi " + hoi + ", con " + CapDo.SoBinhMau + " binh (khong phi binh)");
        Kiem(hoi == 0f && CapDo.SoBinhMau == 1, "mau day ma van mat binh");

        // Mana - qua duong CastAt that (duong cua nut bam / phim tat)
        CapDo.ThemBinh(CapDo.KyBinhMana); CapDo.ThemBinh(CapDo.KyBinhMana);
        pc.mana = pc.maxMana - 120f;
        float mTruoc = pc.mana;
        pc.CastAt(CapDo.KyBinhMana, pc.transform.position + huong * 5f);
        Ghi("C7. CastAt(binh mana), thieu 120 nang luong: " + mTruoc + " -> " + pc.mana + " (+" + (pc.mana - mTruoc) + "), con " + CapDo.SoBinhMana + " binh");
        Kiem(Mathf.Abs(pc.mana - mTruoc - 50f) < 0.01f && CapDo.SoBinhMana == 1, "mot binh mana khong hoi dung 50 qua CastAt");
        yield return new WaitForSeconds(0.6f);
        pc.mana = pc.maxMana - 20f;
        hoi = pc.UongBinh(CapDo.KyBinhMana);
        Ghi("C8. thieu 20 nang luong: hoi " + hoi + ", con " + CapDo.SoBinhMana + " binh");
        Kiem(Mathf.Abs(hoi - 20f) < 0.01f && CapDo.SoBinhMana == 0, "thieu 20 nang luong khong hoi dung 20");
        yield return new WaitForSeconds(0.6f);
        pc.mana = 0f;
        hoi = pc.UongBinh(CapDo.KyBinhMana);
        Ghi("C9. HET binh mana: hoi " + hoi);
        Kiem(hoi == 0f, "het binh ma van uong duoc");

        // ================================================================
        // D. SO BINH TREN O
        // ================================================================
        Ghi("");
        Ghi("D. so binh hien tren o ky nang");
        var hud = Object.FindAnyObjectByType<GameHUD>();
        float s = Screen.height / 1080f;
        foreach (bool camUng in new[] { false, true })
        {
            if (hud != null) hud.epCamUng = camUng;
            CamUng.EpBat = camUng;
            // CHO che do cam ung doi xong roi moi dat binh vao o: dat ngay trong khung hinh vua
            // bat thi BoDangDung van la bo o VUONG - binh vao nham bo, o tron khong co binh
            // (lan chay thu tu: o tron doi 0,0000).
            yield return new WaitForSeconds(0.3f);
            SachPhep.DatLai();
            SachPhep.DatVaoO(0, CapDo.KyBinhMau);            // o 1 giu binh mau, o 2 van la Mua bang
            Kiem(CamUng.DangDung == camUng && SachPhep.BoDangDung[0] == CapDo.KyBinhMau,
                 "khong dat duoc binh mau vao o 1 cua ban " + (camUng ? "cam ung" : "may tinh"));
            yield return new WaitForSeconds(0.2f);
            CuaSoSachPhep.Mo();
            // Chon ky nang KHONG nam o hai o dang do: o dang chon co quang sang dap nhip,
            // nam trong vung do thi thanh nhieu (lan chay thu hai: 0,0093 roi 0,0016)
            CuaSoSachPhep.ChonKyNang(3);
            var b = CuaSoSachPhep.TinhBoCuc(Screen.width, Screen.height, s);
            var rBinh = CuaSoSachPhep.OTaiVung(b.vungO, 0, s);
            var rDoiChung = CuaSoSachPhep.OTaiVung(b.vungO, 1, s);
            // DUA VE DUNG 3: luot ban may tinh da len 12, chi "them cho du 3" thi luot cam ung
            // chup hai lan deu la 12 (lan chay thu nam: o tron doi 0,0000)
            while (CapDo.SoBinhMau > 3) CapDo.BotBinh(CapDo.KyBinhMau);
            while (CapDo.SoBinhMau < 3) CapDo.ThemBinh(CapDo.KyBinhMau);
            Texture2D a3 = null, a12 = null;
            yield return ChupTex(t => a3 = t);
            while (CapDo.SoBinhMau < 12) CapDo.ThemBinh(CapDo.KyBinhMau);
            yield return ChupTex(t => a12 = t);
            // Do GOC PHAI DUOI o (noi co huy hieu so) - do ca o thi o tron nho, so bi pha loang
            System.Func<Rect, Rect> gocDuoi = r => new Rect(r.center.x, r.center.y, r.width * 0.75f, r.height * 0.75f);
            float kBinh = KhacNhau(a3, a12, gocDuoi(rBinh)), kDoi = KhacNhau(a3, a12, gocDuoi(rDoiChung));
            Ghi("D1. sach phep (" + (camUng ? "cam ung" : "may tinh") + "): 3 binh -> 12 binh, o binh doi " + kBinh.ToString("F4")
                + ", o doi chung doi " + kDoiChung(kDoi));
            Kiem(kBinh > 0.003f && kBinh > kDoi * 4f, "so binh tren o sach phep khong doi theo so binh that");
            yield return Chup(camUng ? "binh_2_sachphep_camung" : "binh_2_sachphep_maytinh");
            Object.Destroy(a3); Object.Destroy(a12);
            CuaSoSachPhep.Dong();
            yield return new WaitForSeconds(0.2f);
            yield return Chup(camUng ? "binh_3_hud_camung" : "binh_3_hud_maytinh");
        }
        if (hud != null) hud.epCamUng = false;
        CamUng.EpBat = false;
        SachPhep.DatLai();

        // ================================================================
        // E. CHU TO + CUON
        // ================================================================
        Ghi("");
        Ghi("E. sach phep: chu to va cuon");
        yield return DoChuVaCuon(s);

        // ================================================================
        // F. MANG
        // ================================================================
        Ghi("");
        Ghi("F. binh chung ca phong qua mang (bo dong bo that + kenh gia lap)");
        yield return DoMang(pc);

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static string kDoiChung(float v) { return v.ToString("F4"); }

    static IEnumerator DoChuVaCuon(float s)
    {
        CapDo.BatDauTranMoi();
        CapDo.Them(100);                           // cap 2 = 2 diem
        CapDo.MoKhoa(1);
        CuaSoSachPhep.Mo();
        CuaSoSachPhep.ChonKyNang(4);               // Thien thach, con khoa: dong cap la "CHUA MO KHOA..."
        CuaSoSachPhep.CuonKho = 0f; CuaSoSachPhep.CuonThan = 0f;
        var b = CuaSoSachPhep.TinhBoCuc(Screen.width, Screen.height, s);

        // Chu doi chung ve bang CUNG kieu chu, o co cu va co moi, tren nen den
        var tam = GameObject.Find("TAM_BinhMau").GetComponent<ChayThuMang>();
        const string TenMau = "MƯA BĂNG";
        const string CapMau = "CHƯA MỞ KHOÁ — cần 1 điểm kỹ năng";
        float yDc = Screen.height * 0.02f;
        var hopDc = new Rect[4];
        tam.veGUI = () =>
        {
            GiaoDien.ChuanBi();
            var kT = new GUIStyle(GiaoDien.KieuTieuDeNho);
            var kC = new GUIStyle(GiaoDien.KieuChu);
            string[] chu = { TenMau, TenMau, CapMau, CapMau };
            int[] co = { Mathf.RoundToInt(19f * s), Mathf.RoundToInt(19f * CuaSoSachPhep.HeSoChuKho * s),
                         Mathf.RoundToInt(16f * s), Mathf.RoundToInt(16f * CuaSoSachPhep.HeSoChuThan * s) };
            for (int i = 0; i < 4; i++)
            {
                var k = i < 2 ? kT : kC;
                k.fontSize = co[i];
                k.normal.textColor = Color.white;
                k.wordWrap = false;
                k.clipping = TextClipping.Overflow;
                k.normal.background = null;
                k.alignment = TextAnchor.UpperLeft;
                // Hai hop ten o tren cung, hai hop dong cap o DAY man hinh - lan chay dau hop 3-4 nam
                // de len mep tren cua so Sach phep (vien giot mau do sang) va do nham ca vien.
                float caoHop = 26f * Mathf.Max(1f, s * 1.6f);
                float yHop = i < 2 ? yDc + i * (caoHop + 4f) : Screen.height - (4 - i) * (caoHop + 4f);
                hopDc[i] = new Rect(10f, yHop, Screen.width * 0.30f, caoHop);
                GUI.color = Color.black;
                GUI.DrawTexture(hopDc[i], Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(hopDc[i], chu[i], k);
            }
        };
        Texture2D tex = null;
        yield return ChupTex(t => tex = t);
        tam.veGUI = null;

        float dc19 = BeNgangNet(tex, hopDc[0], 0.35f), dc22 = BeNgangNet(tex, hopDc[1], 0.35f);
        float dc16 = BeNgangNet(tex, hopDc[2], 0.35f), dc19b = BeNgangNet(tex, hopDc[3], 0.35f);

        // Ten "MUA BANG" o hang 1 (da mo, khong chon): dai chu ten nam tu 6k den 28k
        float k15 = CuaSoSachPhep.HeSoChuKho;
        float le = 6f * s;
        var hang = new Rect(b.kho.x + le, b.kho.y + le + 1 * b.caoHang, b.kho.width - le * 2f, b.caoHang - 6f * s);
        float ktIcon = hang.height - 10f * s;
        float xChu = hang.x + 10f * s + ktIcon + 10f * s;
        var daiTen = new Rect(xChu, hang.y + 6f * k15 * s + 1f, hang.xMax - xChu - 60f * s, 20f * k15 * s);
        float tenThat = BeNgangNet(tex, daiTen, 0.55f);

        // Dong cap o dau phan than chi tiet (mau do nhat tren nen toi)
        var than = b.thanChiTiet;
        var daiCap = new Rect(than.x + 14f * s, than.y + 1f, than.width * 0.9f, 16f * CuaSoSachPhep.HeSoChuThan * s);
        float capThat = BeNgangNet(tex, daiCap, 0.45f);

        Ghi("E1. ten ky nang cot trai: net that " + tenThat + " diem; doi chung co cu " + dc19 + ", co moi (x" + k15 + ") " + dc22
            + " -> that / cu = " + (dc19 > 0 ? (tenThat / dc19).ToString("F3") : "?"));
        Ghi("E2. dong cap phan than chi tiet: net that " + capThat + " diem; doi chung co cu " + dc16 + ", co moi (x"
            + CuaSoSachPhep.HeSoChuThan + ") " + dc19b + " -> that / cu = " + (dc16 > 0 ? (capThat / dc16).ToString("F3") : "?"));
        Kiem(dc19 > 0 && Mathf.Abs(tenThat - dc22) <= Mathf.Max(3f, dc22 * 0.05f) && tenThat >= dc19 + 4f,
             "chu cot trai khong to them ~15%");
        // So voi chu doi chung CO MOI la phep do chinh (khop +-5%). So voi co cu chi doi rong hon
        // han: man Editor nho, co chu lam tron thanh so nguyen (16 x 0,54 = 8,6 -> 9; x1,2 = 10,3 -> 10)
        // nen ti le toi da chi con 1,11 - lan chay thu ba bao nham vi doi 1,12.
        Kiem(dc16 > 0 && Mathf.Abs(capThat - dc19b) <= Mathf.Max(3f, dc19b * 0.05f) && capThat >= dc16 + 4f,
             "chu phan chi tiet khong to them ~20%");
        yield return Chup("binh_4_sachphep_chu_to");
        Object.Destroy(tex);

        // ---- Cuon phan than: BAN CAM UNG (vung o chiem nua duoi nen than ngan, loi ke tran) ----
        var hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.epCamUng = true;
        CamUng.EpBat = true;
        yield return new WaitForSeconds(0.3f);
        b = CuaSoSachPhep.TinhBoCuc(Screen.width, Screen.height, s);
        than = b.thanChiTiet;
        CuaSoSachPhep.ChonKyNang(CapDo.KyBinhMau);
        CuaSoSachPhep.CuonThan = 0f;
        Texture2D a0 = null, a1 = null;
        yield return ChupTex(t => a0 = t);
        float noiDung = CuaSoSachPhep.CaoNoiDungThan;
        CuaSoSachPhep.CuonThan = 100000f;
        yield return null;
        float cuonKep = CuaSoSachPhep.CuonThan;
        yield return ChupTex(t => a1 = t);
        var dauMuc = new Rect(b.chiTiet.x, b.chiTiet.y, b.chiTiet.width, than.y - b.chiTiet.y - 12f * s);
        float kThan = KhacNhau(a0, a1, than), kDau = KhacNhau(a0, a1, dauMuc);
        Ghi("E3. than chi tiet: noi dung cao " + noiDung.ToString("F0") + " / vung " + than.height.ToString("F0")
            + " diem; dat cuon 100000 -> kep con " + cuonKep.ToString("F0") + " (toi da " + Mathf.Max(0f, noiDung - than.height + 8f * s).ToString("F0")
            + "); anh doi: than " + kThan.ToString("F4") + ", dau muc " + kDau.ToString("F4"));
        Kiem(noiDung > than.height, "loi ke binh mau khong dai hon vung - man nay khong do duoc cuon");
        Kiem(cuonKep > 0f && cuonKep <= noiDung - than.height + 8f * s + 1f, "cuon than chi tiet khong kep dung");
        Kiem(kThan > 0.005f && kDau < kThan * 0.15f, "cuon than khong doi noi dung, hoac dau muc cuon theo");
        yield return Chup("binh_5_sachphep_cuon_than");
        Object.Destroy(a0); Object.Destroy(a1);
        CuaSoSachPhep.CuonThan = 0f;

        // ---- Cot trai: bao nhieu hang thi bat dau phai cuon ----
        foreach (var man in new[] { new Vector2(844f, 390f), new Vector2(640f, 300f) })
        {
            float sDt = man.y / 1080f;
            var bDt = CuaSoSachPhep.TinhBoCuc(man.x, man.y, sDt);
            float caoDs = 12f * sDt + SachPhep.SoKyNang * bDt.caoHang;
            Ghi("E4. man " + man.x + "x" + man.y + ": cot trai cao " + bDt.kho.height.ToString("F0") + ", 9 hang can " + caoDs.ToString("F0")
                + " -> phai cuon " + (caoDs > bDt.kho.height) + " (vua " + Mathf.FloorToInt((bDt.kho.height - 12f * sDt) / bDt.caoHang) + " hang)");
        }
        CuaSoSachPhep.Dong();
        if (hud != null) hud.epCamUng = false;
        CamUng.EpBat = false;
    }

    static bool giuSong;

    static IEnumerator GiuSong()
    {
        while (giuSong && EditorApplication.isPlaying)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static IEnumerator DoMang(PlayerController toi)
    {
        CapDo.BatDauTranMoi();
        QuanLyBinhRoi.DonSach();
        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = false;

        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        Vector3 ben = Vector3.Cross(Vector3.up, huong);
        var kia = NguoiChoiKhac.Sinh("uid-binh", "Người lượm bình", toi.transform.position + ben * 12f);
        if (kia == null) { Loi("khong sinh duoc nguoi choi kia"); yield break; }

        var goDb = new GameObject("TAM_DongBoBinh");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi;
        db.chiSoCuaToi = 0;
        db.GanTaiNghe();
        db.ThemNguoi(1, kia);
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");

        var daGui = new List<byte[]>();
        KenhTrucTiep.guiSangBenKia = tin => { var bb = GoiTin.TuChuoi(tin); if (bb != null) daGui.Add(bb); };
        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        giuSong = true;
        if (chay != null) chay.StartCoroutine(GiuSong());
        yield return new WaitForSeconds(0.6f);
        System.Func<byte, int> dem = loai => { int n = 0; foreach (var g in daGui) if (GoiTin.LoaiCuaGoi(g) == loai) n++; return n; };
        System.Action<byte[]> nhet = g => KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(g));

        // ---- F1. MAY KHACH: nhan binh roi, toi gan thi xin, duoc giao thi bay vao minh ----
        Vector3 gan = toi.transform.position + huong * 2.2f;
        for (int i = 0; i < 3; i++) nhet(GoiTin.VietBinhRoi(501, CapDo.KyBinhMau, gan));          // goi lap 3 lan
        yield return new WaitForSeconds(0.4f);
        int soBinh501 = 0; foreach (var bb in QuanLyBinhRoi.TatCa) if (bb.SoHieu == 501) soBinh501++;
        int xin = dem(GoiTin.LoaiXinBinh);
        int ghe = -1, soHieu = -1;
        foreach (var g in daGui)
            if (GoiTin.LoaiCuaGoi(g) == GoiTin.LoaiXinBinh) { int sh; byte gh; if (GoiTin.DocSoHieuGhe(g, GoiTin.LoaiXinBinh, out sh, out gh)) { soHieu = sh; ghe = gh; } }
        var b501 = QuanLyBinhRoi.TimBinh(501);
        bool chuaBay = b501 != null && !b501.DaCoChu;
        Ghi("F1. may KHACH nhan 3 goi 'roi binh' 501 (lap): hien " + soBinh501 + " binh; toi gan -> gui " + xin + " goi xin (so hieu "
            + soHieu + ", ghe " + ghe + "); chua duoc giao thi chua bay = " + chuaBay + ", so binh " + CapDo.SoBinhMau);
        Kiem(soBinh501 == 1, "goi roi binh lap lam hien nhieu binh");
        Kiem(xin >= 1 && soHieu == 501 && ghe == 0, "may khach toi gan ma khong gui dung loi xin");
        Kiem(chuaBay && CapDo.SoBinhMau == 0, "may khach tu nhat binh khong cho chu phong quyet (hai nguoi co the cung nhat)");
        for (int i = 0; i < 3; i++) nhet(GoiTin.VietBinhThuoc(501, 0));
        yield return new WaitForSeconds(1.2f);
        Ghi("F1b. chu phong giao 501 cho ghe 0 (minh, goi lap 3 lan): binh con = " + (QuanLyBinhRoi.TimBinh(501) != null)
            + ", so binh mau = " + CapDo.SoBinhMau);
        Kiem(QuanLyBinhRoi.TimBinh(501) == null && CapDo.SoBinhMau == 1, "duoc giao binh ma khong cong dung 1 binh");

        // ---- F2. binh giao cho NGUOI KHAC: bay vao ho, minh khong cong ----
        nhet(GoiTin.VietBinhRoi(502, CapDo.KyBinhMana, kia.transform.position + huong * 1.0f));
        yield return new WaitForSeconds(0.2f);
        var b502 = QuanLyBinhRoi.TimBinh(502);
        nhet(GoiTin.VietBinhThuoc(502, 1));
        float t0 = Time.time; float kcCuoi = 999f; Vector3 viTriCuoi = Vector3.zero;
        while (Time.time - t0 < 2f)
        {
            if (b502 != null) { viTriCuoi = b502.transform.position; kcCuoi = Vector3.Distance(b502.transform.position, kia.transform.position); }
            if (QuanLyBinhRoi.TimBinh(502) == null) break;
            yield return null;
        }
        Ghi("F2. binh 502 giao cho ghe 1 (nguoi khac): bien mat = " + (QuanLyBinhRoi.TimBinh(502) == null) + ", lan cuoi cach ho "
            + kcCuoi.ToString("F2") + " m, so binh mana cua minh = " + CapDo.SoBinhMana);
        Kiem(QuanLyBinhRoi.TimBinh(502) == null && CapDo.SoBinhMana == 0, "binh cua nguoi khac lai cong vao minh / khong bien mat");

        // ---- F3. goi 'thuoc ve' den TRUOC goi 'roi ra' ----
        nhet(GoiTin.VietBinhThuoc(503, 0));
        yield return null;
        nhet(GoiTin.VietBinhRoi(503, CapDo.KyBinhMau, toi.transform.position + huong * 8f));
        yield return new WaitForSeconds(1.5f);
        Ghi("F3. goi 'thuoc ve ghe 0' den truoc goi 'roi ra' (binh 503 o 8 m): so binh mau = " + CapDo.SoBinhMau);
        Kiem(CapDo.SoBinhMau == 2, "goi den sai thu tu thi mat binh");

        // ---- F4. MAY CHU PHONG: gieo -> gui goi roi; hai nguoi xin, nguoi xin TRUOC duoc ----
        TranHienTai.LaHost = true;
        daGui.Clear();
        int id = QuanLyBinhRoi.Roi(CapDo.KyBinhMana, kia.transform.position + huong * 30f);   // xa ca hai
        yield return new WaitForSeconds(0.3f);
        int goiRoi = dem(GoiTin.LoaiBinhRoi);
        nhet(GoiTin.VietXinBinh(id, 1));           // ghe 1 xin truoc
        yield return null; yield return null;
        nhet(GoiTin.VietXinBinh(id, 2));           // ghe 2 xin sau
        yield return new WaitForSeconds(0.4f);
        int giaoGhe1 = 0, giaoKhac = 0;
        foreach (var g in daGui)
        {
            if (GoiTin.LoaiCuaGoi(g) != GoiTin.LoaiBinhThuoc) continue;
            int sh; byte gh;
            if (!GoiTin.DocSoHieuGhe(g, GoiTin.LoaiBinhThuoc, out sh, out gh) || sh != id) continue;
            if (gh == 1) giaoGhe1++; else giaoKhac++;
        }
        var bId = QuanLyBinhRoi.TimBinh(id);
        Ghi("F4. may CHU PHONG gieo binh " + id + ": gui " + goiRoi + " goi 'roi binh' (lap 3 lan); ghe 1 xin truoc, ghe 2 xin sau -> gui 'thuoc ghe 1' "
            + giaoGhe1 + " lan, 'thuoc ghe khac' " + giaoKhac + " lan; binh dang bay ve ghe " + (bId != null ? bId.GheChu.ToString() : "(da toi)"));
        Kiem(goiRoi == QuanLyBinhRoi.SoLanGui, "chu phong khong gui du goi roi binh");
        Kiem(giaoGhe1 >= 1 && giaoKhac == 0, "chu phong khong giao cho nguoi xin TRUOC, hoac giao cho ca hai");

        // ---- F5. chu phong tu toi gan binh: tu giao cho minh, khong can goi xin ----
        int soTruoc = CapDo.SoBinhMau;
        daGui.Clear();
        int id2 = QuanLyBinhRoi.Roi(CapDo.KyBinhMau, toi.transform.position + huong * 2.0f);
        yield return new WaitForSeconds(1.5f);
        int thuocToi = 0;
        foreach (var g in daGui)
        {
            if (GoiTin.LoaiCuaGoi(g) != GoiTin.LoaiBinhThuoc) continue;
            int sh; byte gh;
            if (GoiTin.DocSoHieuGhe(g, GoiTin.LoaiBinhThuoc, out sh, out gh) && sh == id2 && gh == 0) thuocToi++;
        }
        Ghi("F5. chu phong dung gan binh " + id2 + ": so binh mau " + soTruoc + " -> " + CapDo.SoBinhMau + ", bao ca phong 'thuoc ghe 0' "
            + thuocToi + " lan, goi xin tu gui " + dem(GoiTin.LoaiXinBinh));
        Kiem(CapDo.SoBinhMau == soTruoc + 1 && thuocToi >= 1 && dem(GoiTin.LoaiXinBinh) == 0,
             "chu phong toi gan binh khong tu nhat / khong bao ca phong");

        giuSong = false;
        KenhTrucTiep.Dong();
        KenhTrucTiep.guiSangBenKia = null;
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(kia);
        QuanLyBinhRoi.DonSach();
        TranHienTai.Xoa();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[BinhMau] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        CamUng.EpBat = false;
        KenhTrucTiep.guiSangBenKia = null;
        SachPhep.DatLai();
        File.WriteAllText("PlayTestShots/binh_mau_mana.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_BinhMau", "TAM_DongBoBinh" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
