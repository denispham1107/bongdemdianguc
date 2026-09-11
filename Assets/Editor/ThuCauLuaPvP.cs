using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: QUA CAU LUA TRUNG NGUOI CHOI KHAC VA TRUNG KHIENG.
///
/// Nguoi dung bao hai loi:
///   1. Qua cau lua trung nguoi choi khac co no, nhung KHONG thay sat thuong
///      cua cu no - chi thay sat thuong chay lien tuc.
///   2. Qua cau lua trung khieng cua nguoi choi khac KHONG no, bay xuyen qua.
///
/// Menu 37 truoc day chi hoi "nguoi trung don co mat mau khong" - mat mau vi
/// CHAY cung tinh la dat, nen no khong the thay loi 1. Phep thu nay ghi TUNG
/// cu mat mau cua nguoi trung don, kem thoi diem, va tach hai loai:
///   - cu no: 55 x (0,55..1) = 30..55 mau, xay ra DUNG khung hinh qua cau no.
///   - cu chay: ~5 mau moi nua giay, sau do.
/// Va ghi CHO qua cau no, de biet no no o mat khieng hay da lot vao trong.
///
/// Ca hai chieu mang deu do, vi hai may thay hai thu khac nhau:
///   A. Nguoi khac ban minh (goi phep di qua mang, may nay phat lai).
///   B. Minh ban nguoi khac (qua cau cua may nay trung ban sao).
///
/// Ket qua ghi ra <c>PlayTestShots/cauluapvp.txt</c>.
/// </summary>
public static class ThuCauLuaPvP
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/49. Chay thu CAU LUA trung nguoi va khieng", false, 136)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != Canh)
            EditorSceneManager.OpenScene(Canh);

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_CauLuaPvP");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CauLuaPvP] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    // ---------------------------------------------------------------

    /// <summary>Ket qua mot lan ban.</summary>
    class KetQua
    {
        public readonly List<Vector4> no = new List<Vector4>();      // xyz = cho no, w = luc no
        public readonly List<Vector2> su = new List<Vector2>();      // x = luc, y = so mau
        public float mauMat, khiengMat;
        public bool biChay;
    }

    const float NguongCuNo = 20f;     // cu no >= 30, cu chay ~5

    static IEnumerator Do(Damageable dich, System.Action ban, KetQua kq)
    {
        System.Action<Damageable, float> nghe = (d, a) => kq.su.Add(new Vector2(Time.time, a));
        dich.onDamaged += nghe;

        float mauTruoc = dich.health;
        var k = dich.GetComponent<Khieng>();
        float khiengTruoc = k != null ? k.mau : 0f;

        var theo = new Dictionary<Fireball, Vector4>();
        ban();

        float batDau = Time.time;
        bool daThay = false;
        while (Time.time < batDau + 10f)
        {
            foreach (var fb in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None))
            {
                var p = fb.transform.position;
                theo[fb] = new Vector4(p.x, p.y, p.z, Time.time);
                daThay = true;
            }

            // Qua cau vua bien mat thi vi tri cuoi cung cua no la cho no no:
            // Explode dat vi tri roi Destroy o CUOI khung, ma coroutine chay sau
            // Update - nen o khung no, phep thu van doc duoc dung cho no.
            var mat = new List<Fireball>();
            foreach (var cap in theo) if (cap.Key == null) mat.Add(cap.Key);
            foreach (var m in mat) { kq.no.Add(theo[m]); theo.Remove(m); }

            if (daThay && theo.Count == 0 && kq.no.Count > 0
                && Time.time > kq.no[kq.no.Count - 1].w + 4.5f) break;
            yield return null;
        }

        kq.biChay = dich.GetComponent<BurningEffect>() != null || kq.su.Count > 0 && kq.su.FindAll(s => s.y < NguongCuNo).Count > 0;
        dich.onDamaged -= nghe;
        kq.mauMat = mauTruoc - dich.health;
        kq.khiengMat = khiengTruoc - (k != null ? k.mau : 0f);

        // Het chay roi moi sang lan sau
        var ch = dich.GetComponent<BurningEffect>();
        if (ch != null) Object.Destroy(ch);
        yield return null;
    }

    /// <summary>Ghi ket qua. Tra ve khoang cach tu cho no GAN NHAT toi tam.</summary>
    static float GhiKetQua(KetQua kq, Vector3 tam, string tenTam)
    {
        float ganNhat = 999f;
        float lucNoDau = kq.no.Count > 0 ? kq.no[0].w : -1f;
        foreach (var n in kq.no)
        {
            var p = new Vector3(n.x, n.y, n.z);
            float d = Vector3.Distance(p, tam);
            Ghi(string.Format("     qua cau no tai {0}, cach {1} {2:F2} m (ngang {3:F2} m)",
                p.ToString("F1"), tenTam, d,
                Vector2.Distance(new Vector2(p.x, p.z), new Vector2(tam.x, tam.z))));
            if (d < ganNhat) { ganNhat = d; lucNoDau = n.w; }
        }

        int cuNo = 0, cuChay = 0;
        float cuNoLon = 0f;
        var sb = new StringBuilder();
        foreach (var s in kq.su)
        {
            if (s.y >= NguongCuNo) { cuNo++; cuNoLon = Mathf.Max(cuNoLon, s.y); }
            else cuChay++;
            if (sb.Length < 400)
                sb.Append(string.Format("{0:+0.00;-0.00}s:{1:F0} ", s.x - lucNoDau, s.y));
        }
        Ghi("     cac cu mat mau (tinh tu luc no gan nhat): " + (sb.Length == 0 ? "(khong co)" : sb.ToString()));
        Ghi(string.Format("     -> cu no: {0} (lon nhat {1:F0}), cu chay: {2}, tong mat {3:F0} mau, khieng mat {4:F0}, dang chay: {5}",
            cuNo, cuNoLon, cuChay, kq.mauMat, kq.khiengMat, kq.biChay ? "co" : "khong"));
        return ganNhat;
    }

    static int SoCuNo(KetQua kq)
    {
        int n = 0;
        foreach (var s in kq.su) if (s.y >= NguongCuNo) n++;
        return n;
    }

    static IEnumerator ChupLucNo(PlayerController toi, Damageable mauToi, int stt)
    {
        float lucNo = -1f, lucChay = -1f;
        System.Action<Damageable, float> nghe = (d, a) =>
        {
            if (a >= NguongCuNo && lucNo < 0f) lucNo = Time.time;
            else if (a < NguongCuNo && lucChay < 0f) lucChay = Time.time;
        };
        mauToi.onDamaged += nghe;
        NhetGoiPhep(1, 0, stt, toi.transform.position);

        float han = Time.time + 6f;
        while (lucNo < 0f && Time.time < han) yield return null;
        if (lucNo < 0f) { mauToi.onDamaged -= nghe; Ghi("A0. khong co cu no nao de chup"); yield break; }

        yield return Chup("caulua_no_1_ngay");
        Ghi("A0. luc no: so con so sat thuong dang hien = " + DamagePopup.SoDangHien);
        while (Time.time < lucNo + 0.15f) yield return null;
        yield return Chup("caulua_no_2_015s");
        while (Time.time < lucNo + 0.4f) yield return null;
        yield return Chup("caulua_no_3_040s");
        while (lucChay < 0f && Time.time < lucNo + 2f) yield return null;
        yield return Chup("caulua_no_4_chaydau");
        mauToi.onDamaged -= nghe;

        Ghi("A0. da chup luc no: caulua_no_1_ngay / 2_015s / 3_040s / 4_chaydau");
        while (Time.time < lucNo + 5f) yield return null;
        var ch = mauToi.GetComponent<BurningEffect>();
        if (ch != null) Object.Destroy(ch);
        yield return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        yield return new WaitForEndOfFrame();
        yield return null;
    }

    /// <summary>Huong ngang ma ca chum ba qua bay 9 m khong vuong gi. Khong co thi zero.</summary>
    static Vector3 TimHuongTrong(Vector3 P)
    {
        int mask = LayerMask.GetMask("Default", "Ground", "Enemy");
        for (int goc = 0; goc < 360; goc += 10)
        {
            Vector3 h = Quaternion.AngleAxis(goc, Vector3.up) * Vector3.forward;
            bool trong = true;
            foreach (float lech in new[] { -11f, 0f, 11f })
            {
                Vector3 hl = Quaternion.AngleAxis(lech, Vector3.up) * h;
                // Tu tay nguoi ban (cao 1,4 m) toi nguc nguoi kia (cao 0,9 m)
                Vector3 tu = P + Vector3.up * 1.4f;
                Vector3 den = P + hl * 9f + Vector3.up * 0.9f;
                if (Physics.SphereCast(tu, 0.5f, (den - tu).normalized, out _, (den - tu).magnitude,
                                       mask, QueryTriggerInteraction.Collide))
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

    // ---------------------------------------------------------------

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] qua cau lua trung nguoi choi khac va trung khieng");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        // Doi GameDirector rai xong roi don sach quai (xem menu 37)
        yield return new WaitForSeconds(2f);
        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        DonSachQuai();
        yield return new WaitForSeconds(1f);
        DonSachQuai();

        TranHienTai.DangChoiMang = true;

        Vector3 P = toi.transform.position;
        var mauToi = toi.GetComponent<Damageable>();

        // CHON MOT HUONG TRONG. Lan chay dau dat nguoi kia cach 6 m ve phia +x
        // va ca hai qua cau no vao mot tam bia o giua duong, cach nguoi tung
        // 1,7 m - phep thu bao "khong co cu no" ma chang noi gi ve cau lua ca.
        // Nen hoi truoc: ca ba lan bay cua chum (giua va hai ben 11 do) deu
        // khong vuong vat gi trong 9 m.
        Vector3 huong = TimHuongTrong(P);
        Ghi("huong ban: " + huong.ToString("F2") + " (ca ba lan bay trong 9 m khong vuong vat can)");
        if (huong == Vector3.zero) { Ghi("[LOI] khong tim duoc huong nao trong"); loi++; Ket(); yield break; }

        // Nguoi kia: ban sao, o cach 6 m
        var kia = NguoiChoiKhac.Sinh("uid-kia", "Nguoi kia", P + huong * 6f);
        var mauKia = kia.GetComponent<Damageable>();

        var goDb = new GameObject("TAM_DongBo");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi;
        db.chiSoCuaToi = 0;
        db.GanTaiNghe();
        db.ThemNguoi(1, kia);

        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        if (chay != null) chay.StartCoroutine(GiuSong());

        yield return new WaitForSeconds(0.5f);
        DonSachQuai();
        int soThuTu = 100;

        // ================= A0. Chup hinh ngay luc no =================
        // So do cu no co that (A1). Con lai cau hoi: nguoi choi co NHIN THAY
        // con so ay khong. Chup ba khung: ngay sau cu no, 0,15 s va 0,4 s sau,
        // va khung co cu chay dau tien de so.
        yield return ChupLucNo(toi, mauToi, ++soThuTu);

        // ================= A1. Nguoi kia ban minh =================
        Ghi("");
        Ghi("A1. NGUOI KIA ban cau lua vao minh (goi phep qua mang, may nay phat lai):");
        var a1 = new KetQua();
        int stt1 = ++soThuTu;
        yield return Do(mauToi, () => NhetGoiPhep(1, 0, stt1, toi.transform.position), a1);
        GhiKetQua(a1, toi.transform.position + Vector3.up * 0.9f, "minh");
        Kiem(SoCuNo(a1) >= 1, "trung nguoi ma khong co cu no nao - chi co chay");

        // ================= A2. Nguoi kia ban minh khi minh BAT KHIENG =================
        Ghi("");
        Ghi("A2. NGUOI KIA ban vao minh khi minh dang BAT KHIENG (ban kinh " + toi.khiengBanKinh.ToString("F2") + " m):");
        var kToi = Khieng.Bat(toi.gameObject, toi.khiengMau, toi.khiengBanKinh);
        mauToi.khieng = kToi;
        yield return null;
        var a2 = new KetQua();
        int stt2 = ++soThuTu;
        yield return Do(mauToi, () => NhetGoiPhep(1, 0, stt2, toi.transform.position), a2);
        float dA2 = GhiKetQua(a2, toi.transform.position, "tam khieng");
        Kiem(dA2 >= toi.khiengBanKinh - 0.6f,
             "qua cau lot vao trong khieng (no cach tam " + dA2.ToString("F2") + " m, khieng " + toi.khiengBanKinh.ToString("F2") + " m)");
        Kiem(a2.khiengMat > 0f, "khieng khong mat mau nao");
        Kiem(a2.mauMat <= 0.01f, "nguoi dung trong khieng van mat mau");
        Kiem(!a2.biChay, "nguoi dung trong khieng van boc chay");
        kToi.Tat();
        mauToi.khieng = null;

        // ================= B. Minh ban nguoi kia =================
        // Nguoi ban la mot nhan vat CUA MAY NAY (khong phai ban sao), dung cho
        // nhan vat that. Nhan vat that sinh ra truoc khi co mang nen mat na cua
        // no khong co lop Player - dung nhan vat moi cho dung nhu trong tran.
        DatCho(toi, P - huong * 30f);
        var ban = NguoiChoiKhac.Sinh("uid-ban", "Minh ban", P);
        var mauBan = ban.GetComponent<Damageable>();
        mauBan.mauDoMayKhacQuyet = false;
        yield return new WaitForSeconds(0.3f);

        Ghi("");
        Ghi("B1. MINH ban cau lua vao nguoi kia (qua cau cua may nay trung ban sao):");
        var b1 = new KetQua();
        yield return Do(mauKia, () => ban.CastAt(0, kia.transform.position), b1);
        GhiKetQua(b1, kia.transform.position + Vector3.up * 0.9f, "nguoi kia");
        Kiem(SoCuNo(b1) >= 1, "minh ban trung ban sao ma khong hien cu no nao");

        Ghi("");
        Ghi("B2. MINH ban vao nguoi kia khi ho dang BAT KHIENG (khieng tu goi trang thai):");
        HieuUngQuaMang.ApKhieng(kia, 1f);
        yield return null;
        var b2 = new KetQua();
        yield return Do(mauKia, () => ban.CastAt(0, kia.transform.position), b2);
        float dB2 = GhiKetQua(b2, kia.transform.position, "tam khieng ho");
        Kiem(dB2 >= kia.khiengBanKinh - 0.6f,
             "qua cau lot vao trong khieng nguoi kia (no cach tam " + dB2.ToString("F2") + " m)");
        Kiem(!b2.biChay, "nguoi kia dung trong khieng van boc chay");
        var kKia = kia.GetComponent<Khieng>();
        if (kKia != null) kKia.Tat();
        mauKia.khieng = null;

        Ghi("");
        Ghi("B3. MINH dang BAT KHIENG va ban ra - qua cau khong duoc no tren khieng CUA MINH:");
        var kBan = Khieng.Bat(ban.gameObject, ban.khiengMau, ban.khiengBanKinh);
        mauBan.khieng = kBan;
        yield return new WaitForSeconds(0.6f);   // cho het hoi chieu
        var b3 = new KetQua();
        yield return Do(mauKia, () => ban.CastAt(0, kia.transform.position), b3);
        GhiKetQua(b3, kia.transform.position + Vector3.up * 0.9f, "nguoi kia");
        float xaNhat = 0f;
        foreach (var n in b3.no)
            xaNhat = Mathf.Max(xaNhat, Vector3.Distance(new Vector3(n.x, n.y, n.z), ban.transform.position));
        Ghi("     qua cau xa nguoi ban nhat: " + xaNhat.ToString("F2") + " m, khieng cua minh mat "
            + (ban.khiengMau - kBan.mau).ToString("F0") + " mau");
        Kiem(SoCuNo(b3) >= 1, "bat khieng thi cau lua cua minh khong toi duoc nguoi kia");
        Kiem(ban.khiengMau - kBan.mau <= 0.01f, "cau lua cua minh tu lam mat mau khieng cua minh");
        kBan.Tat();

        // ---- Don ----
        KenhTrucTiep.Dong();
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(kia);
        NguoiChoiKhac.Bo(ban);
        TranHienTai.DangChoiMang = false;

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static int DonSachQuai()
    {
        int n = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include,
                                                           FindObjectsSortMode.None))
        { Object.DestroyImmediate(q.gameObject); n++; }
        return n;
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
        byte[] b = GoiTin.VietKyNang(new GoiTin.MotPhep
        {
            chiSo = chiSo, kyNang = kyNang, soThuTu = soThuTu, diemNgam = ngam
        });
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(b));
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);
        Debug.Log("[CauLuaPvP] da tra lai scene " + EditorSceneManager.GetActiveScene().path
                  + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/cauluapvp.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_CauLuaPvP", "TAM_DongBo" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
