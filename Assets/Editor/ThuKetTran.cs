using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: KET TRAN - NGUOI SONG SOT CUOI CUNG THANG (giai doan 2, buoc 6).
///
/// Khong bat tay WebRTC that trong Editor duoc, nen phep thu mo thang cac kenh
/// GIA LAP (nhu menu 45) roi do dung nhung cho de sai:
///
///   A. Goi tin: viet ra doc lai co nguyen ven khong, goi hong co bi tu choi khong.
///   B. May CHU PHONG: nguoi cuoi cung con song thi tran xong, ghe thang dung,
///      bang diem cong dung nguoi, va goi ket tran co ra ca hai kenh khong.
///   C. May KHACH: khach KHONG duoc tu ket luan (hai man hinh se bao hai nguoi
///      thang khac nhau); chet thi phai bao "ai ha toi"; nghe chu phong bao thi
///      moi hien ket qua.
///   D. Chet roi thi camera chuyen sang bam nguoi con song (ngoi xem).
///   E. Man ket tran: chup anh thu, kiem chu co dau va khong tran ra ngoai.
///
/// Ket qua ghi ra <c>PlayTestShots/kettran.txt</c>, anh <c>kettran_*.png</c>.
/// </summary>
public static class ThuKetTran
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/55. Chay thu KET TRAN (nguoi song sot cuoi cung)", false, 146)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu ket tran",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 5] ket tran - nguoi song sot cuoi cung thang");

        KiemGoiTin();

        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != Canh) EditorSceneManager.OpenScene(Canh);

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        // Go truoc roi moi gan: mot lan chay truoc dut giua chung (loi, hoac
        // nguoi dung thoat Play bang tay) se de lai mot dang ky cu, va lan sau
        // KICH BAN CHAY HAI LUOT chong len nhau - bao cao in doi tung dong.
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_KetTran") != null) return;      // luot cu con dang chay
        daBatDau = true;
        var go = new GameObject("TAM_KetTran");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[KetTran-thu] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    // =============================================================
    // A. GOI TIN
    // =============================================================

    static void KiemGoiTin()
    {
        Ghi("");
        Ghi("A. goi tin (khong can Play)");

        byte g, h;
        Kiem(GoiTin.DocChet(GoiTin.VietChet(2, 1), out g, out h) && g == 2 && h == 1,
             "goi chet doc lai khong dung");
        Kiem(!GoiTin.DocChet(new byte[] { GoiTin.LoaiChet }, out g, out h),
             "goi chet cut duoi ma van doc duoc");

        var kq = new GoiTin.KetQua
        {
            gheThang = 3,
            quaiTheoGhe = new byte[] { 7, 0, 200, 1 },
            nguoiTheoGhe = new byte[] { 1, 2, 0, 3 }
        };
        GoiTin.KetQua doc;
        bool ok = GoiTin.DocKetTran(GoiTin.VietKetTran(kq), out doc);
        bool khop = ok && doc.gheThang == 3;
        for (int i = 0; i < GoiTin.SoGheToiDa; i++)
            khop = khop && doc.quaiTheoGhe[i] == kq.quaiTheoGhe[i] && doc.nguoiTheoGhe[i] == kq.nguoiTheoGhe[i];
        Ghi("A1. goi ket tran " + GoiTin.VietKetTran(kq).Length + " byte, doc lai nguyen ven: " + khop);
        Kiem(khop, "bang diem qua goi tin bi sai");

        Kiem(!GoiTin.DocKetTran(new byte[] { GoiTin.LoaiKetTran, 1, 2 }, out doc),
             "goi ket tran cut duoi ma van doc duoc");
    }

    // =============================================================
    //  DUNG MOT MAY GIA LAP
    // =============================================================

    class MotMay
    {
        public GameObject go;
        public DongBoTran dongBo;
        public KetTran ket;
        public List<KeyValuePair<int, string>> daGui = new List<KeyValuePair<int, string>>();
    }

    static MotMay DungMay(PlayerController toi, byte gheToi, bool laHost, int[] kenhMo)
    {
        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = laHost;
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
        m.dongBo.TaoNguoiKhiCan = (ghe, cho) => NguoiChoiKhac.Sinh("uid-" + ghe, "Người " + (ghe + 1), cho);

        m.ket = m.go.AddComponent<KetTran>();
        m.ket.TenCuaGhe = ghe => "Người " + (ghe + 1);
        m.ket.Gan(m.dongBo, toi, gheToi);
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

    /// <summary>Goi trang thai gia cua mot ghe khac - de may nay sinh ban sao cho ho.</summary>
    static string GoiTrangThai(MotMay m, byte ghe, Vector3 cho, bool daChet)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = ghe, viTri = cho, gocY = 0f,
            mau01 = daChet ? 0f : 1f, dangChay = false, daChet = daChet,
            coHieuUng = 0, khieng01 = 0f
        };
        return GoiTin.SangChuoi(GoiTin.VietTrangThai(m.dongBo.GioTran(), ds, 1));
    }

    static int DemGoiLoai(MotMay m, byte loai, out HashSet<int> kenh, out byte[] goiCuoi)
    {
        kenh = new HashSet<int>(); goiCuoi = null;
        int n = 0;
        foreach (var cap in m.daGui)
        {
            var b = GoiTin.TuChuoi(cap.Value);
            if (GoiTin.LoaiCuaGoi(b) != loai) continue;
            n++; kenh.Add(cap.Key); goiCuoi = b;
        }
        return n;
    }

    // =============================================================
    //  KICH BAN
    // =============================================================

    static IEnumerator KichBan()
    {
        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { Loi("khong tim thay nhan vat"); Ket(); yield break; }
        yield return new WaitForSeconds(1.5f);

        var mauToi = toi.GetComponent<Damageable>();

        // ================================================================
        // B. MAY CHU PHONG PHAN QUYET
        // ================================================================
        Ghi("");
        Ghi("B. may chu phong (ghe 0, ba nguoi trong tran)");

        var host = DungMay(toi, 0, true, new[] { 1, 2 });
        yield return null;

        // Hai nguoi khac vao tran
        KenhTrucTiep.GiaLapNhan(1, GoiTrangThai(host, 1, toi.transform.position + new Vector3(3f, 0f, 0f), false));
        KenhTrucTiep.GiaLapNhan(2, GoiTrangThai(host, 2, toi.transform.position + new Vector3(-3f, 0f, 0f), false));
        yield return null;
        Ghi("B1. ban sao cua ghe 1 va 2: " + (host.dongBo.NhanVatCuaGhe(1) != null)
            + " / " + (host.dongBo.NhanVatCuaGhe(2) != null));
        Kiem(host.dongBo.NhanVatCuaGhe(1) != null && host.dongBo.NhanVatCuaGhe(2) != null,
             "chu phong khong sinh du ban sao");

        // Quai chet vi don cua minh -> phai cong vao bang diem cua ghe 0
        Damageable quaiThu = null;
        foreach (var d in Object.FindObjectsByType<Damageable>(FindObjectsSortMode.None))
            if (d != null && !d.isPlayer && !d.IsDead && d.GetComponent<NhanDangQuai>() != null) { quaiThu = d; break; }
        if (quaiThu != null)
        {
            quaiThu.GhiKeDanh(mauToi);
            quaiThu.TakeDamage(quaiThu.maxHealth * 10f, DamageType.Fire, quaiThu.transform.position);
            yield return new WaitForSeconds(0.3f);
        }
        Ghi("B1b. giet mot con quai (" + (quaiThu != null ? quaiThu.name : "khong tim thay con nao") + ")");
        Kiem(quaiThu != null, "khong tim thay quai de thu - bang diem quai chua duoc do");

        // Ghe 1 chet, ke ha la ghe 0 (chinh minh)
        host.daGui.Clear();
        KenhTrucTiep.GiaLapNhan(1, GoiTin.SangChuoi(GoiTin.VietChet(1, 0)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.2f);
        Ghi("B2. ghe 1 chet (ke ha: ghe 0) -> tran xong chua: " + KetTran.DaXong + " (phai la False)");
        Kiem(!KetTran.DaXong, "moi mot nguoi chet ma da ket tran");

        // Ghe 2 chet -> chi con minh
        host.daGui.Clear();
        KenhTrucTiep.GiaLapNhan(2, GoiTin.SangChuoi(GoiTin.VietChet(2, 0)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.6f);

        HashSet<int> kenh; byte[] goi;
        int soGoi = DemGoiLoai(host, GoiTin.LoaiKetTran, out kenh, out goi);
        Ghi("B3. ghe 2 chet -> tran xong: " + KetTran.DaXong + ", ghe thang: " + KetTran.GheThang
            + ", toi thang: " + KetTran.ToiThang);
        Kiem(KetTran.DaXong, "con mot nguoi song ma tran khong ket thuc");
        Kiem(KetTran.GheThang == 0 && KetTran.ToiThang, "ghe thang sai");

        Ghi("B4. goi ket tran phat ra " + soGoi + " lan, toi kenh " + string.Join(",", kenh));
        Kiem(kenh.Contains(1) && kenh.Contains(2), "goi ket tran khong toi du hai khach");
        Kiem(soGoi >= 2, "goi ket tran khong duoc gui lai (mat goi la nguoi ta ngoi mai)");

        GoiTin.KetQua kq = new GoiTin.KetQua
        {
            quaiTheoGhe = new byte[GoiTin.SoGheToiDa],
            nguoiTheoGhe = new byte[GoiTin.SoGheToiDa]
        };
        bool docDuoc = goi != null && GoiTin.DocKetTran(goi, out kq);
        int haCuaToi = docDuoc ? kq.nguoiTheoGhe[0] : -1;
        int quaiCuaToi = docDuoc ? kq.quaiTheoGhe[0] : -1;
        Ghi("B5. bang diem trong goi: ghe 0 ha " + haCuaToi + " nguoi (phai la 2), "
            + "ghe 0 diet " + quaiCuaToi + " quai");
        Kiem(haCuaToi == 2, "khong cong dung so nguoi da ha");
        Kiem(KetTran.BangDiem.nguoiTheoGhe[0] == 2, "bang diem tren may chu phong sai");
        if (quaiThu != null) Kiem(quaiCuaToi >= 1, "quai chet vi don cua minh ma khong duoc cong");

        // Nhan vat khong con dieu khien duoc nua
        Ghi("B6. nhan vat con dieu khien duoc khong: " + toi.enabled + " (phai la False)");
        Kiem(!toi.enabled, "van dau xong ma van dieu khien duoc nhan vat");
        toi.enabled = true;

        DonMay(host);
        yield return null;

        // ================================================================
        // C. MAY KHACH
        // ================================================================
        Ghi("");
        Ghi("C. may khach (ghe 1)");

        var khach = DungMay(toi, 1, false, new[] { 0 });
        yield return null;
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(khach, 0, toi.transform.position + new Vector3(4f, 0f, 0f), false));
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(khach, 2, toi.transform.position + new Vector3(0f, 0f, 4f), false));
        yield return null;

        // Chinh minh chet TRUOC, hai nguoi kia con song: co the do duoc ca viec
        // bao chet lan viec ngoi xem. (Lan dau toi cho hai nguoi kia chet truoc
        // roi moi do camera - khong con ai de bam, phep thu bao loi oan.)
        var banSaoGhe0 = khach.dongBo.NhanVatCuaGhe(0);
        var mauKeHa = banSaoGhe0 != null ? banSaoGhe0.GetComponent<Damageable>() : null;
        khach.daGui.Clear();
        mauToi.GhiKeDanh(mauKeHa);
        mauToi.TakeDamage(mauToi.maxHealth * 10f, DamageType.Fire, toi.transform.position);
        yield return new WaitForSeconds(0.4f);

        HashSet<int> kenhChet; byte[] goiChet;
        int soChet = DemGoiLoai(khach, GoiTin.LoaiChet, out kenhChet, out goiChet);
        byte gheChet = 255, gheHa = 255;
        if (goiChet != null) GoiTin.DocChet(goiChet, out gheChet, out gheHa);
        Ghi("C2. minh chet -> gui goi chet " + soChet + " lan: ghe " + gheChet + ", ke ha ghe " + gheHa
            + " (phai la ghe 1, ke ha 0)");
        Kiem(soChet >= 1, "chet ma khong bao cho ai");
        Kiem(gheChet == 1 && gheHa == 0, "bao sai ai chet / ai ha");

        // D. Ngoi xem: camera chuyen sang nguoi con song
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        string bamAi = rig == null || rig.target == null ? "khong co" : rig.target.name;
        bool bamNguoiKhac = rig != null && rig.target != null && rig.target != toi.transform;
        Ghi("D. chet roi, camera bam: " + bamAi + " (phai la nhan vat nguoi khac)");
        Kiem(bamNguoiKhac, "chet roi van bam xac cua chinh minh, khong xem duoc tran");

        // Man hinh THUA (tran chua xong): phai co dong "Bi ... ha"
        yield return new WaitForEndOfFrame();
        yield return Chup("kettran_3_vua_bi_ha");

        // C1. Gio den luot hai nguoi kia chet - khach VAN khong duoc tu ket luan
        KenhTrucTiep.GiaLapNhan(0, GoiTin.SangChuoi(GoiTin.VietChet(0, 1)));
        KenhTrucTiep.GiaLapNhan(0, GoiTin.SangChuoi(GoiTin.VietChet(2, 1)));
        yield return new WaitForSeconds(KetTran.NhipDem + 0.3f);
        Ghi("C1. ca ba deu chet, khach tu ket luan: " + KetTran.DaXong + " (phai la False)");
        Kiem(!KetTran.DaXong, "may khach tu phan quyet - hai man hinh se bao hai nguoi thang");

        // C3. Chu phong bao ket tran -> khach hien ket qua
        var kqHost = new GoiTin.KetQua
        {
            gheThang = 2,
            quaiTheoGhe = new byte[] { 3, 5, 9, 0 },
            nguoiTheoGhe = new byte[] { 1, 0, 2, 0 }
        };
        KenhTrucTiep.GiaLapNhan(0, GoiTin.SangChuoi(GoiTin.VietKetTran(kqHost)));
        yield return null;
        Ghi("C3. nghe chu phong bao -> xong: " + KetTran.DaXong + ", ghe thang " + KetTran.GheThang
            + ", toi thang " + KetTran.ToiThang + ", ten nguoi thang \"" + KetTran.TenNguoiThang + "\"");
        Kiem(KetTran.DaXong && KetTran.GheThang == 2 && !KetTran.ToiThang, "khach khong hien dung ket qua");
        Kiem(KetTran.BangDiem.quaiTheoGhe[2] == 9 && KetTran.BangDiem.nguoiTheoGhe[2] == 2,
             "bang diem ben khach sai");

        // ================================================================
        // E. MAN KET TRAN
        // ================================================================
        Ghi("");
        Ghi("E. man ket tran tren HUD");
        KetTran.CoTrongTran[0] = KetTran.CoTrongTran[1] = KetTran.CoTrongTran[2] = true;
        KetTran.TenTheoGhe[0] = "Phạm Minh Quân";
        KetTran.TenTheoGhe[1] = "Người chơi số hai";
        KetTran.TenTheoGhe[2] = "Kẻ Săn Hồn Bóng Đêm";
        KetTran.TenNguoiThang = KetTran.TenTheoGhe[2];
        yield return new WaitForEndOfFrame();
        yield return Chup("kettran_1_thua");

        KetTran.GheThang = 1;
        KetTran.ToiThang = true;
        yield return new WaitForEndOfFrame();
        yield return Chup("kettran_2_thang");

        var hud = GameHUD.Ban;
        Ghi("E1. HUD dang song: " + (hud != null));
        Kiem(hud != null, "khong co HUD de ve man ket tran");

        // E2. "Bi ... ha": nguoi choi va quai
        string keHaNguoi = GameHUD.TenKeHa(mauToi);
        Ghi("E2. ke ha minh (ban sao ghe 0): \"" + keHaNguoi + "\" (phai la ten nguoi choi)");
        Kiem(keHaNguoi == "Người 1", "khong doc duoc ten nguoi ha minh");

        Damageable quaiBatKy = null;
        foreach (var d in Object.FindObjectsByType<Damageable>(FindObjectsSortMode.None))
            if (d != null && !d.isPlayer && d.GetComponent<NhanDangQuai>() != null) { quaiBatKy = d; break; }
        if (quaiBatKy != null)
        {
            mauToi.GhiKeDanh(quaiBatKy);
            string keHaQuai = GameHUD.TenKeHa(mauToi);
            Ghi("E3. bi quai ha: \"" + keHaQuai + "\" (loai "
                + quaiBatKy.GetComponent<NhanDangQuai>().loai + ")");
            Kiem(!string.IsNullOrEmpty(keHaQuai) && keHaQuai != "quái vật",
                 "chet vi quai ma khong goi duoc ten con quai");
        }
        else Ghi("E3. khong con quai nao trong canh de thu");

        // E4. Tran xong thi HUD phai chuyen sang doc NUT TRO VE (may cam ung
        // khong co phim ESC). Do gian tiep: nhanh nay khoa can dieu khien lai.
        CamUng.Huong = new Vector2(0.7f, 0.2f);
        CamUng.DangKeo = true;
        yield return null;
        yield return null;
        Ghi("E4. tran xong -> can dieu khien bi khoa: Huong = " + CamUng.Huong
            + ", DangKeo = " + CamUng.DangKeo + " (phai la (0,0) / False)");
        Kiem(CamUng.Huong == Vector2.zero && !CamUng.DangKeo,
             "HUD khong vao nhanh ket tran - nut TRO VE se ve ra ma bam khong an");

        DonMay(khach);
        TranHienTai.Xoa();
        Ghi("");
        Ghi("sau khi ve sanh: KetTran.DaXong = " + KetTran.DaXong + " (phai la False)");
        Kiem(!KetTran.DaXong, "ket qua tran cu con sot lai sang tran sau");

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
        Ghi("anh: " + d);
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[KetTran-thu] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangKenh = null;
        KenhTrucTiep.Dong();
        File.WriteAllText("PlayTestShots/kettran.txt", bao.ToString());
        var rac = GameObject.Find("TAM_KetTran");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
