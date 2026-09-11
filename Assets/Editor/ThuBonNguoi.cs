using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: BON NGUOI TRONG MOT TRAN, NOI HINH SAO QUA CHU PHONG.
///
/// Truoc day KenhTrucTiep giu dung mot ket noi va KhoiDongTranMang dung lai o
/// nguoi dau tien tim thay - phong cho vao bon nguoi ma tran chi noi duoc hai.
///
/// Gio: chu phong giu mot kenh cho moi khach (so kenh = so ghe cua khach),
/// khach chi noi voi chu phong, va chu phong CHUYEN TIEP.
///
/// Khong the bat tay WebRTC that trong Editor, nen phep thu mo thang cac kenh
/// gia lap roi do dung nhung thu de sai: xep ghe, chuyen tiep dung cho (va
/// KHONG vong nguoc ve nguoi gui), tra loi nhip dung kenh, sinh ban sao khi can,
/// va mot nguoi roi tran thi nhung nguoi con lai deu biet.
///
/// Ket qua ghi ra <c>PlayTestShots/bonnguoi.txt</c>.
/// </summary>
public static class ThuBonNguoi
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/45. Chay thu BON NGUOI (noi hinh sao)", false, 132)]
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
        var go = new GameObject("TAM_BonNguoi");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BonNguoi] " + s); }

    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] bon nguoi trong mot tran, noi hinh sao qua chu phong");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        yield return new WaitForSeconds(2f);

        // ================================================================
        //  1. XEP GHE
        // ================================================================
        Ghi("");
        Ghi("--- 1. xep ghe ---");

        // Hai nguoi khach CUNG ghi o "cho = 1" - dung cuoc dua hai nguoi vao
        // phong cach nhau vai tram mili giay.
        var phong = new List<PhongMang.NguoiTrongPhong>
        {
            new PhongMang.NguoiTrongPhong { uid = "uid-C", ten = "Chi",  cho = 2 },
            new PhongMang.NguoiTrongPhong { uid = "uid-B", ten = "Binh", cho = 1 },
            new PhongMang.NguoiTrongPhong { uid = "uid-A", ten = "An",   cho = 0 },   // chu phong
            new PhongMang.NguoiTrongPhong { uid = "uid-D", ten = "Dung", cho = 1 },   // TRUNG ghe 1
        };
        var bang = KhoiDongTranMang.XepGhe(phong, "uid-A");

        var sbGhe = new StringBuilder();
        var daThay = new HashSet<byte>();
        bool trung = false;
        foreach (var g in bang)
        {
            sbGhe.Append(g.ten + "=" + g.ghe + (g.laChuPhong ? "(chu phong) " : " "));
            if (!daThay.Add(g.ghe)) trung = true;
        }
        Ghi("1a. phong co hai nguoi cung ghi 'ghe 1' -> xep: " + sbGhe);
        Kiem(bang.Count == 4, "xep thieu nguoi");
        Kiem(!trung, "hai nguoi trung ghe - goi tin cua ho se de len nhau");
        Kiem(KhoiDongTranMang.TimGhe(bang, "uid-A").ghe == 0, "chu phong khong ngoi ghe 0");

        // May khac doc cung phong nhung THU TU khac - phai ra cung bang ghe
        var phongDaoLon = new List<PhongMang.NguoiTrongPhong>(phong);
        phongDaoLon.Reverse();
        var bang2 = KhoiDongTranMang.XepGhe(phongDaoLon, "uid-A");
        bool khop = bang2.Count == bang.Count;
        for (int i = 0; khop && i < bang.Count; i++)
            khop = KhoiDongTranMang.TimGhe(bang2, bang[i].uid).ghe == bang[i].ghe;
        Ghi("1b. may khac doc cung phong theo thu tu nguoc -> cung bang ghe: " + khop);
        Kiem(khop, "hai may xep ghe khac nhau - ho se goi nhau bang hai ten khac nhau");

        // ================================================================
        //  2. CHU PHONG CHUYEN TIEP
        // ================================================================
        Ghi("");
        Ghi("--- 2. chu phong chuyen tiep ---");

        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = true;

        KenhTrucTiep.Dong();
        for (int k = 1; k <= 3; k++) KenhTrucTiep.GiaLapMo(k);

        var daGui = new List<KeyValuePair<int, string>>();
        KenhTrucTiep.guiSangKenh = (k, t) => daGui.Add(new KeyValuePair<int, string>(k, t));

        var goHost = new GameObject("TAM_BonNguoiHost");
        var host = goHost.AddComponent<DongBoTran>();
        host.toi = toi; host.chiSoCuaToi = 0;
        var boQuaiHost = goHost.AddComponent<DongBoQuai>();
        boQuaiHost.dongBo = host; host.quai = boQuaiHost;

        int soSinh = 0;
        host.TaoNguoiKhiCan = (ghe, viTri) =>
        {
            soSinh++;
            return NguoiChoiKhac.Sinh("uid-ghe" + ghe, "Ghe " + ghe, viTri);
        };
        yield return null;

        // 2a. Trang thai cua khach ghe 2 den tren kenh 2
        daGui.Clear();
        Vector3 choGhe2 = toi.transform.position + new Vector3(3f, 0f, 3f);
        KenhTrucTiep.GiaLapNhan(2, GoiTrangThai(2, choGhe2, host));
        yield return null;

        var kenhDaChuyen = new HashSet<int>();
        foreach (var cap in daGui)
        {
            byte[] b = GoiTin.TuChuoi(cap.Value);
            if (GoiTin.LoaiCuaGoi(b) != GoiTin.LoaiTrangThai) continue;
            var ds = new GoiTin.MotNguoi[4]; int moc;
            int n = GoiTin.DocTrangThai(b, ds, out moc);
            if (n > 0 && ds[0].chiSo == 2) kenhDaChuyen.Add(cap.Key);
        }
        Ghi("2a. goi trang thai cua ghe 2 den tren kenh 2 -> chu phong chuyen sang kenh: "
            + string.Join(",", kenhDaChuyen) + " (phai la 1,3 - KHONG vong ve 2)");
        Kiem(kenhDaChuyen.Contains(1) && kenhDaChuyen.Contains(3), "khong chuyen tiep du cho");
        Kiem(!kenhDaChuyen.Contains(2), "chuyen nguoc ve nguoi gui");

        // 2b. Ban sao cua ghe 2 duoc sinh khi can
        Ghi("2b. chu phong sinh ban sao cho ghe 2 khi goi dau tien den: "
            + host.CoNguoi(2) + " (so lan sinh: " + soSinh + ")");
        Kiem(host.CoNguoi(2), "khong sinh ban sao khi goi tin den");

        // 2c. Ky nang cua ghe 1 -> sang 2 va 3
        daGui.Clear();
        KenhTrucTiep.GiaLapNhan(1, GoiTrangThai(1, choGhe2 + Vector3.right * 2f, host));
        KenhTrucTiep.GiaLapNhan(1, GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
        {
            chiSo = 1, kyNang = 5, soThuTu = 1, diemNgam = choGhe2
        })));
        yield return null;
        var kenhPhep = new HashSet<int>();
        foreach (var cap in daGui)
            if (GoiTin.LoaiCuaGoi(GoiTin.TuChuoi(cap.Value)) == GoiTin.LoaiKyNang) kenhPhep.Add(cap.Key);
        Ghi("2c. ghe 1 tung phep -> chu phong chuyen sang kenh: " + string.Join(",", kenhPhep)
            + " (phai la 2,3)");
        Kiem(kenhPhep.Contains(2) && kenhPhep.Contains(3) && !kenhPhep.Contains(1),
             "ky nang khong toi du nguoi, hoac vong nguoc");

        // 2d. Hoi nhip tren kenh 3 -> chi tra loi kenh 3
        daGui.Clear();
        KenhTrucTiep.GiaLapNhan(3, GoiTin.SangChuoi(GoiTin.VietNhip(true, 777)));
        yield return null;
        var kenhNhip = new HashSet<int>();
        foreach (var cap in daGui)
        {
            bool laHoi; int moc;
            if (GoiTin.DocNhip(GoiTin.TuChuoi(cap.Value), out laHoi, out moc) && !laHoi && moc == 777)
                kenhNhip.Add(cap.Key);
        }
        Ghi("2d. ghe 3 hoi nhip -> chu phong tra loi tren kenh: " + string.Join(",", kenhNhip)
            + " (phai CHI la 3)");
        Kiem(kenhNhip.Count == 1 && kenhNhip.Contains(3), "tra loi nhip sai kenh");

        // 2e. Dan quai cua chu phong toi ca ba nguoi khach
        daGui.Clear();
        yield return new WaitForSeconds(0.25f);
        var kenhQuai = new HashSet<int>();
        foreach (var cap in daGui)
            if (GoiTin.LoaiCuaGoi(GoiTin.TuChuoi(cap.Value)) == GoiTin.LoaiQuai) kenhQuai.Add(cap.Key);
        Ghi("2e. chu phong ke lai dan quai -> toi kenh: " + string.Join(",", kenhQuai)
            + " (phai la 1,2,3)");
        Kiem(kenhQuai.Count == 3, "dan quai khong toi moi nguoi khach");

        // ================================================================
        //  3. MOT NGUOI KHACH ROI TRAN
        // ================================================================
        Ghi("");
        Ghi("--- 3. mot nguoi khach roi tran ---");

        // Ca ba kenh deu phai "da tung nghe" thi moi tinh la dang choi
        KenhTrucTiep.GiaLapNhan(3, GoiTrangThai(3, choGhe2 + Vector3.left * 2f, host));
        yield return null;

        var aiDi = new List<byte>();
        host.KhiMatNguoi += g => aiDi.Add(g);
        daGui.Clear();

        KenhTrucTiep.Dong(3);                    // ghe 3 dong tab
        yield return null; yield return null;

        var kenhBao = new HashSet<int>();
        foreach (var cap in daGui)
        {
            byte ghe;
            if (GoiTin.DocRoiTran(GoiTin.TuChuoi(cap.Value), out ghe) && ghe == 3) kenhBao.Add(cap.Key);
        }
        Ghi("3a. ghe 3 dong kenh -> chu phong ket luan ai di: [" + string.Join(",", aiDi)
            + "], bao cho kenh: " + string.Join(",", kenhBao) + " (phai [3], bao 1,2)");
        Kiem(aiDi.Count == 1 && aiDi[0] == 3, "chu phong khong nhan ra ghe 3 da di");
        Kiem(kenhBao.Contains(1) && kenhBao.Contains(2), "khong bao cho nhung nguoi con lai");
        Ghi("3b. tinh trang chung cua chu phong: " + host.TinhTrang
            + " (phai KHONG la DaMat - con hai nguoi)");
        Kiem(host.TinhTrang != DongBoTran.TinhTrangKetNoi.DaMat, "mot nguoi di ma ca tran vo");

        KenhTrucTiep.guiSangKenh = null;
        Object.DestroyImmediate(goHost);
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            if (pc != toi) NguoiChoiKhac.Bo(pc);
        yield return null;

        // ================================================================
        //  4. PHIA NGUOI KHACH (ghe 2)
        // ================================================================
        Ghi("");
        Ghi("--- 4. phia nguoi khach ghe 2 ---");

        TranHienTai.LaHost = false;
        KenhTrucTiep.Dong();
        KenhTrucTiep.GiaLapMo(0);

        var goKhach = new GameObject("TAM_BonNguoiKhach");
        var khach = goKhach.AddComponent<DongBoTran>();
        khach.toi = toi; khach.chiSoCuaToi = 2;
        khach.TaoNguoiKhiCan = (ghe, viTri) => NguoiChoiKhac.Sinh("uid-ghe" + ghe, "Ghe " + ghe, viTri);
        var khachAiDi = new List<byte>();
        khach.KhiMatNguoi += g => khachAiDi.Add(g);
        yield return null;

        // Tat ca deu den tren kenh 0 - chu phong va hai goi chuyen tiep
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(0, choGhe2, khach));
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(1, choGhe2 + Vector3.right * 2f, khach));
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(3, choGhe2 + Vector3.left * 2f, khach));
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(2, choGhe2, khach));     // chinh minh, qua vong
        yield return null;

        Ghi("4a. khach nhan tren mot kenh duy nhat -> co ban sao cho ghe 0/1/3: "
            + khach.CoNguoi(0) + "/" + khach.CoNguoi(1) + "/" + khach.CoNguoi(3)
            + ", co ban sao cho CHINH MINH (ghe 2): " + khach.CoNguoi(2)
            + " (phai True/True/True, False)");
        Kiem(khach.CoNguoi(0) && khach.CoNguoi(1) && khach.CoNguoi(3), "khach khong thay du ba nguoi kia");
        Kiem(!khach.CoNguoi(2), "khach dung ban sao cho chinh minh");

        // Chu phong bao ghe 3 da di
        KenhTrucTiep.GiaLapNhan(0, GoiTin.SangChuoi(GoiTin.VietRoiTran(3)));
        yield return null;
        bool con3 = khach.CoNguoi(3);
        if (khachAiDi.Contains(3)) khach.BoNguoi(3);   // KhoiDongTranMang lam viec nay trong tran that

        // Goi chuyen tiep den TRE cua ghe 3 - khong duoc sinh lai
        KenhTrucTiep.GiaLapNhan(0, GoiTrangThai(3, choGhe2 + Vector3.left * 2f, khach));
        yield return null;
        Ghi("4b. chu phong bao 'ghe 3 da roi tran' -> su kien: [" + string.Join(",", khachAiDi)
            + "]; mot goi tre cua ghe 3 den sau do -> sinh lai ban sao: " + khach.CoNguoi(3)
            + " (phai [3], False)");
        Kiem(khachAiDi.Contains(3), "khach khong biet ghe 3 da di");
        Kiem(!khach.CoNguoi(3), "goi tre lam nguoi da di hien ra lai");

        // ---- Don ----
        Object.DestroyImmediate(goKhach);
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            if (pc != toi) NguoiChoiKhac.Bo(pc);
        KenhTrucTiep.Dong();
        TranHienTai.Xoa();

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static string GoiTrangThai(byte ghe, Vector3 cho, DongBoTran db)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = ghe, viTri = cho, gocY = 0f, mau01 = 1f, dangChay = false, daChet = false
        };
        return GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1));
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangKenh = null;
        KenhTrucTiep.Dong();
        File.WriteAllText("PlayTestShots/bonnguoi.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_BonNguoi", "TAM_BonNguoiHost", "TAM_BonNguoiKhach" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.update += TraLaiCanh;
    }
}
