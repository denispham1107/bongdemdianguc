using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 67): MUA BANG CO THAT SU DONG BANG NGUOI CHOI KHAC KHONG - DUONG MANG THAT.
///
/// Nguoi dung (14/09/2026): "Mua bang da thuc su gay hieu ung Dong bang len nguoi choi khac
/// chua (khong de cap den quai vat) ma toi khong thay xuat hien chu Dong bang giong nhu
/// Choang, Nga. Hay kiem tra va bao cao lai".
///
/// Dung bo dong bo that (DongBoTran) + kenh gia lap nhu menu 63:
///   B1. MINH LA NAN NHAN: nguoi kia (ghe 1) tung Mua bang vao cho minh bang GOI KY NANG that.
///       Do: trung don, bi cham, bi dong cung, chan bi khoa that (DangBiKhoaCung, HeSoTocBang),
///       va GOI TRANG THAI minh gui di co mang bit dong bang (de may kia thay) khong.
///   B2. MINH LA NGUOI XEM: may nguoi kia bao "toi dang dong cung" bang goi trang thai that ->
///       ban sao cua ho tren may minh co dong cung, dung im, lop vo bang phu len HINH khong.
///   B3. CHU NOI: dem moi dong chu bay len (DamagePopup) trong B1, B2 - co chu nao ve bang
///       khong. DOI CHUNG: goi "choang" phai ra chu "CHOÁNG!" - khong thi bo dem hong.
///   B4. LOP VO BANG: FrozenEffect phu vat lieu vo bang len bao nhieu renderer cua nhan vat
///       nguoi choi, so voi mot con quai (doi chung); va DO ANH vung ban sao: truoc / dang
///       dong cung / sau khi tan - vo bang phai lam vung ay xanh len roi tra lai.
///
/// Lan chay dau (14/09/2026, truoc khi sua): dong bang CO tac dung that 6/6 nhung 0 chu noi va
/// vo bang phu 0 renderer (model co xuong). Sau khi sua: moi lan BAT DAU dong cung dung mot chu
/// "ĐÓNG BĂNG!", vo bang phu len hinh, "CHOÁNG!" co dau o ca duong may tu tinh.
///
/// Ket qua: PlayTestShots/bang_nguoichoi.txt, anh bang_nguoichoi_*.png.
/// </summary>
public static class ThuBangNguoiChoi
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/67. Chay thu MUA BANG dong bang NGUOI CHOI KHAC (qua mang)", false, 155)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu dong bang nguoi choi",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Mua bang dong bang NGUOI CHOI KHAC, qua duong mang that");

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
        if (GameObject.Find("TAM_BangNguoi") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_BangNguoi");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BangNguoi] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    // ---- Doc chu cua cac DamagePopup dang hien (truong rieng - phep thu Editor duoc phep soi) ----
    static readonly FieldInfo fDangHien = typeof(DamagePopup).GetField("dangHien", BindingFlags.NonPublic | BindingFlags.Static);
    static readonly FieldInfo fChu = typeof(DamagePopup).GetField("chu", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo fTouched = typeof(FrozenEffect).GetField("touched", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>Gom moi chu KHONG PHAI SO dang bay len vao tap (mot popup chi dem mot lan).</summary>
    static void GomChu(HashSet<DamagePopup> daThay, List<string> chu)
    {
        var ds = fDangHien != null ? fDangHien.GetValue(null) as List<DamagePopup> : null;
        if (ds == null) return;
        foreach (var p in ds)
        {
            if (p == null || daThay.Contains(p)) continue;
            daThay.Add(p);
            string c = fChu != null ? fChu.GetValue(p) as string : null;
            int so;
            if (!string.IsNullOrEmpty(c) && !int.TryParse(c, out so)) chu.Add(c);
        }
    }

    static int SoVoBang(FrozenEffect f)
    {
        var ds = f != null && fTouched != null ? fTouched.GetValue(f) as System.Collections.IList : null;
        return ds != null ? ds.Count : -1;
    }

    static string LoaiRenderer(GameObject go)
    {
        int sk = go.GetComponentsInChildren<SkinnedMeshRenderer>().Length;
        int me = go.GetComponentsInChildren<MeshRenderer>().Length;
        return sk + " SkinnedMeshRenderer (hinh nguoi), " + me + " MeshRenderer";
    }

    static IEnumerator ChupTex(System.Action<Texture2D> nhan)
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    /// <summary>Phan THAN cua khung bao nhan vat tren man hinh (toa do anh, y tu duoi len).</summary>
    static RectInt KhungManHinh(GameObject go)
    {
        var cam = Camera.main;
        var r = go.GetComponentInChildren<SkinnedMeshRenderer>();
        if (cam == null || r == null) return new RectInt(0, 0, 0, 0);
        var b = r.bounds;
        float x0 = 1e9f, y0 = 1e9f, x1 = -1e9f, y1 = -1e9f;
        for (int i = 0; i < 8; i++)
        {
            var g = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
            var sp = cam.WorldToScreenPoint(g);
            x0 = Mathf.Min(x0, sp.x); x1 = Mathf.Max(x1, sp.x); y0 = Mathf.Min(y0, sp.y); y1 = Mathf.Max(y1, sp.y);
        }
        // Lay phan GIUA khung (than nguoi), bo le - de khong an sang canh nen
        float w = x1 - x0, h = y1 - y0;
        return new RectInt(Mathf.RoundToInt(x0 + w * 0.3f), Mathf.RoundToInt(y0 + h * 0.2f),
                           Mathf.Max(1, Mathf.RoundToInt(w * 0.4f)), Mathf.Max(1, Mathf.RoundToInt(h * 0.6f)));
    }

    /// <summary>Trung binh (xanh lam - do) trong khung - vo bang xanh bang lam so nay tang.</summary>
    static float DoXanh(Texture2D t, RectInt k)
    {
        double tong = 0; int n = 0;
        for (int y = Mathf.Max(0, k.yMin); y < Mathf.Min(t.height, k.yMax); y++)
            for (int x = Mathf.Max(0, k.xMin); x < Mathf.Min(t.width, k.xMax); x++)
            { var c = t.GetPixel(x, y); tong += c.b - c.r; n++; }
        return n > 0 ? (float)(tong / n) : 0f;
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

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator GiuSong()
    {
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoBang") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static void NhetTrangThai(DongBoTran db, PlayerController kia, byte co)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = 1, viTri = kia.transform.position, gocY = kia.transform.eulerAngles.y,
            mau01 = 1f, coHieuUng = co
        };
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }
        yield return new WaitForSeconds(1.5f);

        var toi = TimToi();
        if (toi == null) { Loi("khong tim thay nhan vat"); Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;

        TranHienTai.DangChoiMang = true;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include))
            Object.DestroyImmediate(q.gameObject);

        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        var kia = NguoiChoiKhac.Sinh("uid-bang", "Người bị đóng băng", toi.transform.position + huong * 7f);
        if (kia == null) { Loi("khong sinh duoc nguoi choi kia"); Ket(); yield break; }
        var mauKia = kia.GetComponent<Damageable>();

        var goDb = new GameObject("TAM_DongBoBang");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi;
        db.chiSoCuaToi = 0;
        db.GanTaiNghe();
        db.ThemNguoi(1, kia);
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");
        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        if (chay != null) chay.StartCoroutine(GiuSong());

        // Bat moi goi minh gui sang ben kia
        var daGui = new List<string>();
        KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
        yield return new WaitForSeconds(0.6f);

        Ghi("");
        Ghi("Hinh nhan vat nguoi choi: " + LoaiRenderer(toi.gameObject));

        // ================================================================
        // B1. MINH LA NAN NHAN
        // ================================================================
        Ghi("");
        Ghi("B1. nguoi kia tung MUA BANG vao cho minh bang GOI KY NANG that (6 con bao, moi con 5 giay)");
        var chuB1 = new List<string>();
        var daThayB1 = new HashSet<DamagePopup>();
        int soTrung = 0, soCham = 0, soDongCung = 0, soKhoaChan = 0, soGoiCoBang = 0, soGoiCoDongCung = 0;
        int voBangToi = -1, soBatDauDongCung = 0;
        bool daChup = false;
        for (int lan = 0; lan < 6; lan++)
        {
            var cu = toi.GetComponent<FrozenEffect>(); if (cu != null) Object.DestroyImmediate(cu);
            foreach (var st in Object.FindObjectsByType<IceStorm>()) Object.Destroy(st.gameObject);
            mauToi.health = mauToi.maxHealth;
            yield return new WaitForSeconds(0.4f);
            daGui.Clear();

            float mauTruoc = mauToi.health;
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            {
                chiSo = 1, kyNang = 1, capKyNang = 1, soThuTu = 700 + lan,
                diemNgam = toi.transform.position
            })));

            bool cham = false, dongCung = false, khoaChan = false, dangCungTruoc = false;
            float tocMin = 1f;
            float hanL = Time.time + 6.8f;
            while (Time.time < hanL)
            {
                var f = toi.GetComponent<FrozenEffect>();
                if (f != null)
                {
                    if (f.slow >= 0.45f) cham = true;
                    if (f.IsFullyFrozen && !dangCungTruoc) soBatDauDongCung++;
                    if (f.IsFullyFrozen)
                    {
                        dongCung = true;
                        voBangToi = Mathf.Max(voBangToi, SoVoBang(f));
                        if (!daChup) { daChup = true; yield return Chup("bang_nguoichoi_1_minh_dong_cung"); }
                    }
                }
                dangCungTruoc = f != null && f.IsFullyFrozen;
                if (toi.DangBiKhoaCung) khoaChan = true;
                tocMin = Mathf.Min(tocMin, toi.HeSoTocBang);
                GomChu(daThayB1, chuB1);
                yield return null;
            }

            // Doc lai cac goi TRANG THAI minh da gui trong luc ay
            bool goiBang = false, goiDongCung = false;
            var ra = new GoiTin.MotNguoi[4];
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s);
                if (b == null || GoiTin.LoaiCuaGoi(b) != GoiTin.LoaiTrangThai) continue;
                int moc; int n = GoiTin.DocTrangThai(b, ra, out moc);
                for (int i = 0; i < n; i++)
                {
                    if (ra[i].chiSo != 0) continue;
                    if ((ra[i].coHieuUng & HieuUngQuaMang.CoBang) != 0) goiBang = true;
                    if ((ra[i].coHieuUng & HieuUngQuaMang.CoBangHoanToan) != 0) goiDongCung = true;
                }
            }

            bool trung = mauToi.health < mauTruoc - 1f;
            if (trung) soTrung++;
            if (cham) soCham++;
            if (dongCung) soDongCung++;
            if (khoaChan) soKhoaChan++;
            if (goiBang) soGoiCoBang++;
            if (goiDongCung) soGoiCoDongCung++;
            Ghi(string.Format("    bao {0}: trung {1} (mat {2:F0} mau), bi cham {3}, dong cung {4}, chan bi khoa {5}, toc thap nhat x{6:F2}; goi gui di: co bang {7}, co dong cung {8}",
                lan + 1, trung, mauTruoc - mauToi.health, cham, dongCung, khoaChan, tocMin, goiBang, goiDongCung));
        }
        Ghi(string.Format("B1. trung {0}/6, bi cham {1}/6, dong cung {2}/6, chan bi khoa {3}/6; goi trang thai gui di mang bit bang {4}/6, bit dong cung {5}/6",
            soTrung, soCham, soDongCung, soKhoaChan, soGoiCoBang, soGoiCoDongCung));
        Ghi("    chu noi xuat hien tren may minh trong B1: " + (chuB1.Count == 0 ? "(khong co chu nao)" : string.Join(" | ", chuB1.ToArray())));
        Kiem(soTrung >= 5, "Mua bang cua nguoi kia khong trung minh - phep do hong");
        Kiem(soCham >= 5, "Mua bang trung minh ma khong lam cham");
        Kiem(soDongCung >= 3, "Mua bang cua NGUOI CHOI KHAC trung minh ma gan nhu khong dong cung (mong ~35% moi tang, nhieu tang moi bao)");
        Kiem(soKhoaChan == soDongCung, "bi dong cung ma chan / phep khong bi khoa");
        Kiem(soGoiCoDongCung == soDongCung, "bi dong cung ma goi trang thai khong bao cho may kia");
        int chuBangB1 = chuB1.FindAll(c => c == "ĐÓNG BĂNG!").Count;
        Ghi("    so lan BAT DAU dong cung (dem tung khung) " + soBatDauDongCung + ", so chu \"ĐÓNG BĂNG!\" " + chuBangB1);
        Kiem(chuBangB1 >= 1 && Mathf.Abs(chuBangB1 - soBatDauDongCung) <= 1,
             "chu ĐÓNG BĂNG! tren may nan nhan khong khop so lan bat dau dong cung");

        // ================================================================
        // B2. MINH LA NGUOI XEM
        // ================================================================
        Ghi("");
        Ghi("B2. may nguoi kia bao 'toi dang dong cung' bang GOI TRANG THAI that (1,5 giay)");
        mauKia.mauDoMayKhacQuyet = true;
        var chuB2 = new List<string>();
        var daThayB2 = new HashSet<DamagePopup>();
        bool coBang = false, coDongCung = false; float tocKia = 1f; int voBangKia = -1;
        float xanhTruoc = 0f, xanhDang = 0f, xanhSau = 0f;
        {
            foreach (var st in Object.FindObjectsByType<IceStorm>()) Object.Destroy(st.gameObject);
            // Doi VUNG BANG DUOI DAT cua con bao cuoi tat han: no la vat rieng, tat cham hon con
            // bao. Lan do dau chi doi 1 giay - anh "truoc" bi vung ay nhuom xanh (0,129) cao hon ca
            // luc dang dong cung (0,121), du anh chup thay ro vo bang boc kin nguoi.
            yield return new WaitForSeconds(6.0f);
            Texture2D tTruoc = null; yield return ChupTex(t => tTruoc = t);
            xanhTruoc = DoXanh(tTruoc, KhungManHinh(kia.gameObject)); Object.Destroy(tTruoc);
            float t0 = Time.time; bool chup2 = false;
            while (Time.time - t0 < 1.5f)
            {
                NhetTrangThai(db, kia, (byte)(HieuUngQuaMang.CoBang | HieuUngQuaMang.CoBangHoanToan));
                var f = kia.GetComponent<FrozenEffect>();
                if (f != null)
                {
                    coBang = true;
                    if (f.IsFullyFrozen) { coDongCung = true; tocKia = Mathf.Min(tocKia, f.HeSoToc); voBangKia = Mathf.Max(voBangKia, SoVoBang(f)); }
                }
                GomChu(daThayB2, chuB2);
                if (!chup2 && Time.time - t0 > 0.8f)
                {
                    chup2 = true;
                    Texture2D tDang = null; yield return ChupTex(t => tDang = t);
                    xanhDang = DoXanh(tDang, KhungManHinh(kia.gameObject)); Object.Destroy(tDang);
                    yield return Chup("bang_nguoichoi_2_ban_sao_dong_cung");
                }
                yield return null;
            }
            float t1 = Time.time;
            while (Time.time - t1 < 1.0f) { NhetTrangThai(db, kia, 0); GomChu(daThayB2, chuB2); yield return null; }
            bool tan = kia.GetComponent<FrozenEffect>() == null;
            Texture2D tSau = null; yield return ChupTex(t => tSau = t);
            xanhSau = DoXanh(tSau, KhungManHinh(kia.gameObject)); Object.Destroy(tSau);
            Ghi("B2. ban sao nguoi kia: co lop bang " + coBang + ", dong cung " + coDongCung + ", he so toc x" + tocKia.ToString("F2")
                + ", goi het bang -> tan " + tan);
            Ghi("    chu noi tren may minh trong B2: " + (chuB2.Count == 0 ? "(khong co chu nao)" : string.Join(" | ", chuB2.ToArray())));
            Kiem(coBang && coDongCung, "nguoi kia dong cung tren may ho ma may minh KHONG biet");
            Kiem(tocKia <= 0.001f, "ban sao dong cung ma van di duoc");
            Kiem(tan, "het bang ma ban sao van dong cung");
            int chuBangB2 = chuB2.FindAll(c => c == "ĐÓNG BĂNG!").Count;
            Ghi("    chu \"ĐÓNG BĂNG!\" tren ban sao: " + chuBangB2 + " (mong dung 1 - goi den 60 lan/giay ma khong duoc lap chu)");
            Kiem(chuBangB2 == 1, "ban sao dong cung ma chu ĐÓNG BĂNG! khong hien dung mot lan");
            Ghi(string.Format("    do XANH (b - r) vung hinh ban sao tren anh: truoc {0:F3}, dang dong cung {1:F3}, sau khi tan {2:F3}",
                xanhTruoc, xanhDang, xanhSau));
            Kiem(xanhDang > xanhTruoc + 0.05f && xanhDang > xanhSau + 0.05f, "vo bang khong hien len hinh ban sao tren anh chup");
        }

        // ================================================================
        // B3. DOI CHUNG CHU NOI: goi choang
        // ================================================================
        var chuChoang = new List<string>();
        var daThayC = new HashSet<DamagePopup>();
        {
            float t0 = Time.time;
            while (Time.time - t0 < 0.6f) { NhetTrangThai(db, kia, HieuUngQuaMang.CoChoang); GomChu(daThayC, chuChoang); yield return null; }
            float t1 = Time.time;
            while (Time.time - t1 < 0.6f) { NhetTrangThai(db, kia, 0); yield return null; }
        }
        bool thayChoang = chuChoang.Exists(c => c.Contains("CHOÁNG"));
        // Duong MAY TU TINH: choang nhan vat cua chinh minh
        var chuChoangTuTinh = new List<string>();
        {
            var daThayT = new HashSet<DamagePopup>();
            var cu = toi.GetComponent<StunnedEffect>(); if (cu != null) Object.DestroyImmediate(cu);
            GomChu(daThayT, new List<string>());          // bo chu cu
            StunnedEffect.Apply(mauToi, 0.5f);
            GomChu(daThayT, chuChoangTuTinh);
            yield return new WaitForSeconds(0.7f);
        }
        Ghi("B3b. choang tren MAY TU TINH (nhan vat minh): chu noi " + (chuChoangTuTinh.Count == 0 ? "(khong co)" : string.Join(" | ", chuChoangTuTinh.ToArray())));
        Kiem(chuChoangTuTinh.Contains("CHOÁNG!"), "duong may tu tinh khong in dung chu CHOÁNG! co dau");
        bool coChuBang = chuB1.Exists(c => c.ToUpperInvariant().Contains("BĂNG")) || chuB2.Exists(c => c.ToUpperInvariant().Contains("BĂNG"));
        Ghi("");
        Ghi("B3. doi chung - goi CHOANG tren cung ban sao: chu noi " + (chuChoang.Count == 0 ? "(khong co)" : string.Join(" | ", chuChoang.ToArray()))
            + " -> bo dem chu " + (thayChoang ? "chay dung" : "HONG"));
        Ghi("    co chu nao ve DONG BANG (B1 + B2): " + (coChuBang ? "CO" : "KHONG"));
        Kiem(thayChoang, "bo dem chu noi hong (goi choang khong ra chu CHOÁNG!) - ket luan ve chu dong bang vo nghia");
        Kiem(coChuBang, "khong co chu ĐÓNG BĂNG! nao");

        // ================================================================
        // B4. LOP VO BANG PHU LEN HINH
        // ================================================================
        Ghi("");
        GameObject quai = null;
        {
            // Doi chung: mot con quai that
            quai = EnemyFactory.Spawn(MonsterType.Skeleton, toi.transform.position - huong * 6f, null, toi.transform);
            yield return new WaitForSeconds(0.3f);
            var mq = quai != null ? quai.GetComponent<Damageable>() : null;
            int voQuai = -1;
            if (mq != null)
            {
                FrozenEffect.Apply(mq, 1.5f);
                yield return null; yield return null;
                voQuai = SoVoBang(mq.GetComponent<FrozenEffect>());
            }
            Ghi("B4. lop VO BANG phu len so renderer: nhan vat minh (B1) " + voBangToi + ", ban sao nguoi kia (B2) " + voBangKia
                + " | doi chung quai bo xuong " + voQuai + " (" + (quai != null ? LoaiRenderer(quai) : "khong sinh duoc") + ")");
            Kiem(voBangToi > 0 && voBangKia > 0 && voQuai > 0, "vo bang khong phu len hinh (model co xuong)");
            if (quai != null) Object.Destroy(quai);
        }

        KenhTrucTiep.guiSangBenKia = null;
        KenhTrucTiep.Dong();
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(kia);

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
        Debug.Log("[BangNguoi] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/bang_nguoichoi.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_BangNguoi", "TAM_DongBoBang" })
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
