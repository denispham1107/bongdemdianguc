using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: THIEN THACH DANH NGA NGUOI CHOI KHAC - DI QUA DUONG MANG THAT.
///
/// Nguoi dung (13/09/2026): "Ban chua thu voi nhan vat khac trong game, ban moi
/// chi thu voi quai vat". Dung. Menu 62 thu nguoi choi bang cach GOI THANG
/// BiDanhNga.Apply va HieuUngQuaMang.ApCo - bo qua dung hai buoc de hong nhat:
/// qua thien thach that di qua goi ky nang, va co "dang nga" di qua goi trang
/// thai (dong goi / mo goi).
///
/// Phep thu nay dung bo dong bo that (DongBoTran) va kenh gia lap, nhu menu 37:
///   G1. MINH LA NAN NHAN: nguoi kia (ghe 1) tung thien thach vao cho minh bang
///       mot GOI KY NANG that. Phep phat lai tren may minh, qua that roi xuong,
///       nhan vat CUA MINH phai bi nga - dem tren nhieu lan, va do hinh that
///       (vi tri xuong dau) cua nhan vat nguoi choi, khong phai cua quai.
///   G2. MINH LA NGUOI XEM: may nguoi kia bao "toi dang nga" bang GOI TRANG THAI
///       that (VietTrangThai -> kenh -> DongBoTran). Ban sao cua ho tren may
///       minh phai nam xuong, co dau hieu tren dau, va dung day khi goi ngung.
///   G3. Byte co cua goi trang thai va goi quai phai giu duoc bit "dang nga".
///
/// Ket qua ghi ra <c>PlayTestShots/danhnga_nguoichoi.txt</c>.
/// </summary>
public static class ThuDanhNgaNguoiChoi
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/63. Chay thu DANH NGA NGUOI CHOI KHAC (qua mang)", false, 154)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu danh nga nguoi choi",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] thien thach danh nga NGUOI CHOI KHAC, qua duong mang that");

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
        if (GameObject.Find("TAM_NgaNguoi") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_NgaNguoi");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[NgaNguoi] " + s); }
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

    static Transform TimXuong(Transform goc, string ten)
    {
        foreach (var t in goc.GetComponentsInChildren<Transform>(true))
            if (t.name.ToLowerInvariant().Contains(ten)) return t;
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
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoNga") != null)
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

    /// <summary>Do tu the tu vi tri xuong dau that: cao bao nhieu, lech ra sau lung bao nhieu.</summary>
    static void DoTuThe(Transform nv, Transform dau, out float cao, out float sauLung)
    {
        cao = dau != null ? dau.position.y - nv.position.y : 0f;
        sauLung = dau != null ? Vector3.Dot(dau.position - nv.position, -nv.forward) : 0f;
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

        // ================================================================
        // G3. BYTE CO GIU DUOC BIT "DANG NGA" KHONG
        // ================================================================
        Ghi("");
        Ghi("G3. dong goi roi mo goi - bit dang nga con khong");
        {
            var dsN = new GoiTin.MotNguoi[1];
            dsN[0] = new GoiTin.MotNguoi { chiSo = 1, mau01 = 1f, coHieuUng = HieuUngQuaMang.CoNga };
            var bN = GoiTin.VietTrangThai(0, dsN, 1);
            var raN = new GoiTin.MotNguoi[4];
            int t0; GoiTin.DocTrangThai(bN, raN, out t0);
            bool conN = (raN[0].coHieuUng & HieuUngQuaMang.CoNga) != 0;

            var dsQ = new GoiTin.MotQuai[1];
            dsQ[0] = new GoiTin.MotQuai { id = 7, mau01 = 1f, coHieuUng = HieuUngQuaMang.CoNga };
            var bQ = GoiTin.VietQuai(0, dsQ, 0, 1);
            var raQ = new GoiTin.MotQuai[4];
            int t1; GoiTin.DocQuai(bQ, raQ, out t1);
            bool conQ = (raQ[0].coHieuUng & HieuUngQuaMang.CoNga) != 0;

            // Cac bit cu phai con nguyen ben canh bit moi
            byte tatCa = (byte)(HieuUngQuaMang.CoBang | HieuUngQuaMang.CoBangHoanToan | HieuUngQuaMang.CoChoang | HieuUngQuaMang.CoNga);
            dsN[0].coHieuUng = tatCa; dsN[0].dangChay = true; dsN[0].daChet = true;
            GoiTin.DocTrangThai(GoiTin.VietTrangThai(0, dsN, 1), raN, out t0);

            Ghi("G3. goi NGUOI CHOI: bit nga con " + conN + " | goi QUAI: bit nga con " + conQ
                + " | du 4 hieu ung + dang chay + da chet: " + raN[0].coHieuUng + " (phai " + tatCa + "), "
                + raN[0].dangChay + "/" + raN[0].daChet);
            Kiem(conN, "goi trang thai NGUOI CHOI danh mat bit dang nga - may kia khong bao gio thay minh nga");
            Kiem(conQ, "goi QUAI danh mat bit dang nga - may khach khong thay quai nga");
            Kiem(raN[0].coHieuUng == tatCa && raN[0].dangChay && raN[0].daChet, "them bit nga lam hong cac bit cu");
        }

        // ================================================================
        // Dung hai may bang bo dong bo that + kenh gia lap
        // ================================================================
        TranHienTai.DangChoiMang = true;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(q.gameObject);

        // Nguoi kia dung cach minh 9 m, huong trong
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        var kia = NguoiChoiKhac.Sinh("uid-nga", "Người bị ngã", toi.transform.position + huong * 9f);
        if (kia == null) { Loi("khong sinh duoc nguoi choi kia"); Ket(); yield break; }
        var mauKia = kia.GetComponent<Damageable>();

        var goDb = new GameObject("TAM_DongBoNga");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi;
        db.chiSoCuaToi = 0;
        db.GanTaiNghe();
        db.ThemNguoi(1, kia);
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");
        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        if (chay != null) chay.StartCoroutine(GiuSong());
        yield return new WaitForSeconds(0.6f);

        // ================================================================
        // G1. MINH LA NAN NHAN
        // ================================================================
        Ghi("");
        Ghi("G1. nguoi kia tung thien thach vao cho minh bang GOI KY NANG that (10 lan)");
        {
            var dauToi = TimXuong(toi.transform, "head");
            float caoDung, sauDung;
            DoTuThe(toi.transform, dauToi, out caoDung, out sauDung);
            Ghi("    xuong dau nhan vat minh: " + (dauToi != null ? dauToi.name : "KHONG") + ", dung thang cao " + caoDung.ToString("F2") + " m");

            int soNga = 0, soTrung = 0, soNam = 0, soCoDau = 0;
            bool daChup = false;
            for (int lan = 0; lan < 10; lan++)
            {
                var cu = toi.GetComponent<BiDanhNga>(); if (cu != null) Object.DestroyImmediate(cu);
                foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);
                mauToi.health = mauToi.maxHealth;
                yield return new WaitForSeconds(0.3f);

                float mauTruoc = mauToi.health;
                KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
                {
                    chiSo = 1, kyNang = 4, capKyNang = 1, soThuTu = 500 + lan,
                    diemNgam = toi.transform.position
                })));

                bool nga = false, nam = false, coDau = false;
                float hanL = Time.time + 3.6f;
                while (Time.time < hanL)
                {
                    var bn = toi.GetComponent<BiDanhNga>();
                    if (bn != null)
                    {
                        nga = true;
                        if (BiDanhNga.SoDauVeLanCuoi > 0) coDau = true;
                        if (bn.daTroi > 0.6f && bn.daTroi < 1.0f)
                        {
                            float cao, sau; DoTuThe(toi.transform, dauToi, out cao, out sau);
                            if (dauToi != null && cao < caoDung * 0.45f && sau > 0.4f) nam = true;
                            if (!daChup) { daChup = true; yield return Chup("danhnga_nguoichoi_1_minh_bi_nga"); }
                        }
                    }
                    yield return null;
                }
                if (mauToi.health < mauTruoc - 1f) soTrung++;
                if (nga) soNga++;
                if (nam) soNam++;
                if (coDau) soCoDau++;
                Ghi("    lan " + (lan + 1) + ": trung " + (mauToi.health < mauTruoc - 1f) + ", bi nga " + nga
                    + ", hinh nam ngua " + nam + ", dau hieu tren dau " + coDau);
            }
            Ghi("G1. trung " + soTrung + "/10, nhan vat minh bi nga " + soNga + "/10, nam ngua that " + soNam
                + ", co dau hieu " + soCoDau + " (mong doi ~78% so lan co nga)");
            Kiem(soTrung >= 8, "thien thach cua nguoi kia khong trung minh - phep do hong");
            Kiem(soNga >= 4, "thien thach cua NGUOI CHOI KHAC roi trung minh ma gan nhu khong nga");
            Kiem(soNam == soNga, "nhan vat nguoi choi bi nga ma HINH khong nam xuong");
            Kiem(soCoDau == soNga, "nhan vat nguoi choi bi nga ma khong co dau hieu tren dau");
        }

        // ================================================================
        // G2. MINH LA NGUOI XEM: nguoi kia bi nga tren may ho
        // ================================================================
        Ghi("");
        Ghi("G2. may nguoi kia bao 'toi dang nga' bang GOI TRANG THAI that");
        {
            mauKia.mauDoMayKhacQuyet = true;
            var dauKia = TimXuong(kia.transform, "head");
            float caoDung, sauDung;
            DoTuThe(kia.transform, dauKia, out caoDung, out sauDung);

            bool coNga = false, nam = false, coDau = false;
            float t0 = Time.time;
            while (Time.time - t0 < 1.5f)          // may kia gui "dang nga" 60 lan/giay trong 1,5 giay
            {
                NhetTrangThai(db, kia, HieuUngQuaMang.CoNga);
                var bn = kia.GetComponent<BiDanhNga>();
                if (bn != null)
                {
                    coNga = true;
                    if (BiDanhNga.SoDauVeLanCuoi > 0) coDau = true;
                    float cao, sau; DoTuThe(kia.transform, dauKia, out cao, out sau);
                    if (Time.time - t0 > 0.7f && dauKia != null && cao < caoDung * 0.45f && sau > 0.4f) nam = true;
                }
                if (Time.time - t0 > 0.9f && Time.time - t0 < 0.95f) yield return Chup("danhnga_nguoichoi_2_ban_sao_nga");
                yield return null;
            }
            // Goi ngung "dang nga" - van gui trang thai binh thuong
            float t1 = Time.time;
            while (Time.time - t1 < 1.0f) { NhetTrangThai(db, kia, 0); yield return null; }
            bool dungDay = kia.GetComponent<BiDanhNga>() == null;

            Ghi("G2. ban sao nguoi kia: nhan duoc nga " + coNga + ", hinh nam ngua " + nam + ", dau hieu tren dau " + coDau
                + ", goi het nga -> dung day " + dungDay);
            Kiem(coNga, "nguoi kia bi nga tren may ho ma may minh KHONG biet (co nga khong qua duoc goi tin)");
            Kiem(nam, "ban sao nguoi kia nhan nga ma hinh khong nam xuong");
            Kiem(coDau, "ban sao nguoi kia bi nga ma khong co dau hieu tren dau");
            Kiem(dungDay, "het nga ma ban sao van nam");
        }

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
        Debug.Log("[NgaNguoi] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/danhnga_nguoichoi.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_NgaNguoi", "TAM_DongBoNga" })
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
