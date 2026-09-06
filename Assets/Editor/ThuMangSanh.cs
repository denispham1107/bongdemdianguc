using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU DUONG MANG: dang nhap - tao phong - vao phong - san sang - dem nguoc.
///
/// Chay THAT tren Firebase chu khong gia lap: dung dung tai khoan, dung dung
/// co so du lieu ma nguoi choi se dung. Vi vay no de lai rac neu dut giua
/// chung - buoc cuoi luon don phong da tao.
///
/// Phai vao Play mode vi tat ca deu la coroutine (UnityWebRequest khong chay
/// ngoai Play). Dong ho dung <c>Time.realtimeSinceStartup</c> chu khong phai
/// <c>Time.time</c>: o day dang do THOI GIAN MANG chu khong phai nhip game,
/// va may khung dau <c>Time.time</c> bi kep lai.
///
/// Ket qua ghi ra <c>PlayTestShots/mang_sanh.txt</c>.
/// </summary>
public static class ThuMangSanh
{
    // Hai tai khoan da tao san khi chay thu tren web
    // Doc tu file ngoai git - xem ThongTinChayThu
    static string EmailA { get { return ThongTinChayThu.EmailA; } }
    static string EmailB { get { return ThongTinChayThu.EmailB; } }
    static string MatKhau { get { return ThongTinChayThu.MatKhau; } }

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static string maPhongDaTao;

    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/26. Chay thu MANG - dang nhap va phong cho", false, 112)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        // Thieu thong tin tai khoan thi dung han o day, dung vao Play roi
        // moi hong - vao Play xong bao loi thi nhin het nhu loi mang.
        if (!ThongTinChayThu.DocHoacBao()) return;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().name != "MainMenu")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; maPhongDaTao = null;
        daBatDau = false;

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static bool daBatDau;

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;

        // Can mot MonoBehaviour de chay coroutine trong Play mode
        var go = new GameObject("TAM_ThuMang");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuMang] " + s);
    }

    static IEnumerator ChayKichBan()
    {
        float t0 = Time.realtimeSinceStartup;

        // ---- 1. DANG NHAP ----
        FirebaseMang.Quen();
        bool ok = false; string e = null;
        float t = Time.realtimeSinceStartup;
        yield return FirebaseMang.DangNhap(EmailB, MatKhau, (o, err) => { ok = o; e = err; });
        Ghi(string.Format("dang nhap B: {0} ({1:F0} ms)",
                          ok ? "OK" : "LOI - " + e, (Time.realtimeSinceStartup - t) * 1000f));
        if (!ok) { loi++; Ket(); yield break; }

        // ---- 2. HO SO ----
        t = Time.realtimeSinceStartup;
        yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi(string.Format("tai ho so: {0}, ten = {1}, {2} thang / {3} tran ({4:F0} ms)",
                          ok ? "OK" : "LOI - " + e, HoSoMang.CuaToi.ten,
                          HoSoMang.CuaToi.soTranThang, HoSoMang.CuaToi.soTranChoi,
                          (Time.realtimeSinceStartup - t) * 1000f));
        if (!ok) { loi++; Ket(); yield break; }

        // ---- 3. DO DONG HO MAY CHU ----
        t = Time.realtimeSinceStartup;
        yield return PhongMang.DoDongHoMayChu();
        Ghi(string.Format("do dong ho may chu: {0:F0} ms", (Time.realtimeSinceStartup - t) * 1000f));

        // ---- 4. TAO PHONG ----
        t = Time.realtimeSinceStartup;
        yield return PhongMang.TaoPhong("Phong chay thu", "Act2", (o, err) => { ok = o; e = err; });
        Ghi(string.Format("tao phong: {0} ({1:F0} ms)",
                          ok ? "OK" : "LOI - " + e, (Time.realtimeSinceStartup - t) * 1000f));
        if (!ok) { loi++; Ket(); yield break; }

        maPhongDaTao = PhongMang.PhongHienTai.ma;
        Ghi("  ma phong = " + maPhongDaTao
            + ", man = " + PhongMang.PhongHienTai.manChoi
            + ", host = " + PhongMang.PhongHienTai.hostTen
            + ", la host = " + PhongMang.LaHost
            + ", so nguoi = " + PhongMang.PhongHienTai.soNguoi);

        if (!PhongMang.LaHost) { Ghi("[LOI] nguoi tao phong ma khong phai host"); loi++; }

        // ---- 5. DOC LAI DANH SACH: co thay phong minh vua tao khong ----
        t = Time.realtimeSinceStartup;
        System.Collections.Generic.List<PhongMang.Phong> ds = null;
        yield return PhongMang.LayDanhSach(k => ds = k);
        var thay = ds != null ? ds.Find(p => p.ma == maPhongDaTao) : null;
        Ghi(string.Format("doc danh sach: {0} phong, co phong vua tao = {1} ({2:F0} ms)",
                          ds == null ? 0 : ds.Count, thay != null,
                          (Time.realtimeSinceStartup - t) * 1000f));
        if (thay == null) { Ghi("[LOI] tao phong xong ma danh sach khong thay"); loi++; }

        // ---- 6. DOI MAN CHOI ----
        yield return PhongMang.DoiManChoi("Act1", null);
        yield return PhongMang.TaiLaiPhong(maPhongDaTao, (o, err) => { });
        Ghi("doi man sang Act1 -> doc lai duoc: " + PhongMang.PhongHienTai.manChoi);
        if (PhongMang.PhongHienTai.manChoi != "Act1") { Ghi("[LOI] doi man khong an"); loi++; }

        // ---- 7. BAT DAU DEM NGUOC ----
        t = Time.realtimeSinceStartup;
        yield return PhongMang.BatDauDemNguoc(null);
        yield return PhongMang.TaiLaiPhong(maPhongDaTao, (o, err) => { });

        double conLai = PhongMang.ConLaiGiay();
        Ghi(string.Format("bat dau dem nguoc: trang thai = {0}, con lai {1:F2} giay ({2:F0} ms)",
                          PhongMang.PhongHienTai.trangThai, conLai,
                          (Time.realtimeSinceStartup - t) * 1000f));

        if (PhongMang.PhongHienTai.trangThai != "demNguoc")
        { Ghi("[LOI] bam bat dau ma trang thai khong doi"); loi++; }

        // Dem nguoc dat 10 giay. Cho phep lech 1,5 giay cho duong truyen.
        if (conLai < PhongMang.GiayDemNguoc - 1.5 || conLai > PhongMang.GiayDemNguoc + 0.5)
        { Ghi("[LOI] dem nguoc lech qua nhieu so voi " + PhongMang.GiayDemNguoc + " giay"); loi++; }

        // ---- 8. DOI 3 GIAY ROI DO LAI: dong ho co chay deu khong ----
        double truocKhiDoi = PhongMang.ConLaiGiay();
        yield return new WaitForSecondsRealtime(3f);
        double sauKhiDoi = PhongMang.ConLaiGiay();
        double daChay = truocKhiDoi - sauKhiDoi;
        Ghi(string.Format("sau 3 giay thuc: dem nguoc tut {0:F2} giay (dung ra phai ~3,00)", daChay));
        if (daChay < 2.5 || daChay > 3.5) { Ghi("[LOI] dong ho dem nguoc chay sai nhip"); loi++; }

        // ---- 9. NGUOI THU HAI VAO PHONG ----
        // Dang nhap tai khoan A de bat chuoc nguoi choi khac. Phai luu lai
        // phien cua B truoc, khong thi buoc don dep phia sau khong con quyen.
        string uidB = FirebaseMang.Uid;
        string tokenB = FirebaseMang.IdToken;
        string tenB = FirebaseMang.TenHienThi;

        t = Time.realtimeSinceStartup;
        yield return FirebaseMang.DangNhap(EmailA, MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        Ghi(string.Format("dang nhap A: {0} ({1:F0} ms)",
                          ok ? "OK, ten = " + HoSoMang.CuaToi.ten : "LOI - " + e,
                          (Time.realtimeSinceStartup - t) * 1000f));

        if (ok)
        {
            // Phong dang o trang thai demNguoc nen VaoPhong se tu choi - dung
            // nhu thiet ke. Kiem tra dung dieu do.
            bool vaoDuoc = false; string loiVao = null;
            yield return PhongMang.VaoPhong(maPhongDaTao, (o, err) => { vaoDuoc = o; loiVao = err; });
            Ghi("vao phong dang dem nguoc: " + (vaoDuoc ? "VAO DUOC" : "bi tu choi - " + loiVao));
            if (vaoDuoc) { Ghi("[LOI] phong da bat dau ma van cho nguoi moi vao"); loi++; }
        }

        // ---- 10. DON DEP ----
        yield return FirebaseMang.DangNhap(EmailB, MatKhau, (o, err) => { ok = o; });
        if (ok && !string.IsNullOrEmpty(maPhongDaTao))
        {
            yield return FirebaseMang.Xoa("phong/" + maPhongDaTao, (o, err) => { });
            yield return FirebaseMang.Xoa("tran/" + maPhongDaTao, (o, err) => { });

            string con = null;
            yield return FirebaseMang.Doc("phong/" + maPhongDaTao, s => con = s);
            bool sach = string.IsNullOrEmpty(con) || con == "null";
            Ghi("don phong chay thu: " + (sach ? "sach" : "VAN CON"));
            if (!sach) loi++;
        }

        Ghi(string.Format("tong thoi gian: {0:F1} giay", Time.realtimeSinceStartup - t0));
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/mang_sanh.txt", bao.ToString());

        var rac = GameObject.Find("TAM_ThuMang");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }
}
