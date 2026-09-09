using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU BUOC 2: DU DOAN VA HIEU CHINH.
///
/// Do bon dieu, va dieu dau tien la dieu kien song con cua ca buoc 2:
///
///  1. TAT DINH - cung mot chuoi input, chay lai phai ra CUNG mot ket qua.
///     Khong tat dinh thi moi lan hieu chinh nhan vat lai nhay mot doan, ma
///     hieu chinh xay ra vai chuc lan moi giay.
///
///  2. HIEU CHINH KHI KHOP - trong tai xac nhan dung y trang thai minh doan.
///     Nhan vat KHONG duoc nhuc nhich. Day la truong hop xay ra 99% thoi
///     gian, va no ma lam giat thi game khong choi duoc.
///
///  3. HIEU CHINH KHI LECH - trong tai bao mot vi tri khac han. Nhan vat phai
///     ve dung do, roi chay lai cac goi con lai.
///
///  4. QUEN GOI DA XAC NHAN - lich su khong duoc phinh mai.
///
/// Ket qua ghi ra <c>PlayTestShots/du_doan.txt</c>.
/// </summary>
public static class ThuDuDoan
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/31. Chay thu DU DOAN (buoc 2)", false, 118)]
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
        var go = new GameObject("TAM_DuDoan");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[DuDoan] " + s);
    }

    /// <summary>
    /// Mot chuoi input co dinh, KHONG ngau nhien theo dong ho.
    ///
    /// Phai lap lai duoc y het thi moi so sanh duoc hai lan chay. Dung mot bo
    /// sinh so co hat giong co dinh chu khong dung Random cua Unity - Random
    /// do dung chung mot trang thai voi ca game, hat lua hay quai vat nhuc
    /// nhich mot cai la chuoi doi.
    /// </summary>
    static List<GoiInput> DungChuoiInput(int soGoi, float dt)
    {
        var ds = new List<GoiInput>(soGoi);
        uint hat = 12345u;

        for (int i = 0; i < soGoi; i++)
        {
            // Bo sinh so tuyen tinh don gian - du ngau nhien, va lap lai duoc
            hat = hat * 1664525u + 1013904223u;
            float a = ((hat >> 16) & 0xFFFF) / 65535f;
            hat = hat * 1664525u + 1013904223u;
            float b = ((hat >> 16) & 0xFFFF) / 65535f;

            var g = GoiInput.Rong(dt);
            g.soThuTu = i + 1;

            // Doi huong moi 20 khung, moi lan lech vua phai.
            //
            // Ban dau doi moi 12 khung voi buoc 1,7 rad: nhan vat xoay vong
            // tai cho, 60 goi ma chi dich duoc 0,94 m. Phep kiem tat dinh voi
            // mot nhan vat gan nhu dung yen thi chang chung minh duoc gi -
            // phai cho no di that xa roi moi so hai lan chay.
            float goc = Mathf.Floor(i / 20f) * 0.6f + a * 0.15f;
            g.huongDi = new Vector3(Mathf.Cos(goc), 0f, Mathf.Sin(goc)) * (0.7f + b * 0.3f);

            ds.Add(g);
        }
        return ds;
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 2] buoc 2 - chuoi input di xa hon de phep kiem co suc nang");

        PlayerController pc = null;
        float hetHan = Time.time + 20f;
        while (pc == null && Time.time < hetHan)
        {
            pc = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (pc == null) { Ghi("[LOI] khong tim thay PlayerController"); loi++; Ket(); yield break; }

        yield return new WaitForSeconds(1.5f);
        pc.tuDocInput = false;

        const int SO_GOI = 120;
        const float DT = 1f / 60f;
        var chuoi = DungChuoiInput(SO_GOI, DT);

        // ---- 1. TAT DINH ----
        // Chay chuoi input hai lan tu cung mot cho, phai ra cung mot cho.
        var dau = pc.ChupTrangThai();

        for (int i = 0; i < SO_GOI; i++) pc.ThiHanhMotKhung(chuoi[i]);
        var lan1 = pc.ChupTrangThai();

        pc.DatTrangThai(dau);
        yield return null;                       // cho CharacterController on dinh
        pc.DatTrangThai(dau);                    // dat lai lan nua cho chac

        for (int i = 0; i < SO_GOI; i++) pc.ThiHanhMotKhung(chuoi[i]);
        var lan2 = pc.ChupTrangThai();

        float lechTatDinh = lan1.LechVoi(lan2);
        Ghi(string.Format("1. tat dinh: chay {0} goi hai lan -> lech {1:F4} m "
                          + "(di duoc {2:F2} m)",
                          SO_GOI, lechTatDinh, Vector3.Distance(dau.viTri, lan1.viTri)));

        if (lechTatDinh > 0.01f)
        {
            Ghi("[LOI] khong tat dinh - chay lai ra ket qua khac, hieu chinh se giat");
            loi++;
        }

        // ---- 2. HIEU CHINH KHI KHOP: nhan vat khong duoc nhuc nhich ----
        pc.DatTrangThai(dau);
        yield return null;
        pc.DatTrangThai(dau);

        var duDoan = new DuDoan(pc);
        for (int i = 0; i < SO_GOI; i++)
        {
            pc.ThiHanhMotKhung(chuoi[i]);
            duDoan.GhiNhan(chuoi[i]);
        }

        var truocKhiHieuChinh = pc.ChupTrangThai();

        // Trong tai xac nhan dung y cai minh da tinh o goi thu 20
        var thatSuKhop = LayTrangThaiTaiGoi(pc, duDoan, 40);
        duDoan.HieuChinh(thatSuKhop);

        float xeDich = truocKhiHieuChinh.LechVoi(pc.ChupTrangThai());
        Ghi(string.Format("2. hieu chinh khi KHOP -> nhan vat xe dich {0:F4} m "
                          + "(phai gan 0), lech phat hien {1:F4} m",
                          xeDich, duDoan.LechGanNhat));

        if (xeDich > 0.01f)
        { Ghi("[LOI] trong tai xac nhan dung ma nhan vat van nhay - se giat lien tuc"); loi++; }

        // ---- 3. HIEU CHINH KHI LECH ----
        pc.DatTrangThai(dau);
        yield return null;
        pc.DatTrangThai(dau);

        duDoan.Xoa();
        for (int i = 0; i < SO_GOI; i++)
        {
            pc.ThiHanhMotKhung(chuoi[i]);
            duDoan.GhiNhan(chuoi[i]);
        }

        // Trong tai bao: o goi 20, nhan vat that ra o cho khac 2 met
        var thatSuLech = LayTrangThaiTaiGoi(pc, duDoan, 40);
        thatSuLech.viTri += new Vector3(2f, 0f, 0f);

        int chayLaiTruoc = duDoan.SoLanChayLai;
        float lechBao = duDoan.HieuChinh(thatSuLech);
        int soChayLai = duDoan.SoLanChayLai - chayLaiTruoc;

        Ghi(string.Format("3. hieu chinh khi LECH 2 m -> phat hien lech {0:F2} m, "
                          + "chay lai {1} goi", lechBao, soChayLai));

        if (lechBao < 1.5f)
        { Ghi("[LOI] lech 2 m ma khong phat hien ra"); loi++; }
        if (soChayLai != SO_GOI - 40)
        { Ghi("[LOI] phai chay lai " + (SO_GOI - 40) + " goi, thuc te " + soChayLai); loi++; }

        // ---- 4. QUEN GOI DA XAC NHAN ----
        Ghi("4. sau hieu chinh con giu " + duDoan.SoGoiDangGiu + " goi (phai la "
            + (SO_GOI - 40) + ")");
        if (duDoan.SoGoiDangGiu != SO_GOI - 40)
        { Ghi("[LOI] khong quen nhung goi da duoc xac nhan - lich su se phinh mai"); loi++; }

        pc.tuDocInput = true;
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>Trang thai ma chinh may nay da tinh ra tai goi thu n - dong
    /// vai "su that" ma trong tai gui ve.</summary>
    static TrangThaiNhanVat LayTrangThaiTaiGoi(PlayerController pc, DuDoan dd, int n)
    {
        // Dung phan chieu thi gon, nhung sandbox cam - nen DuDoan mo san mot
        // cua cho viec chay thu.
        return dd.TrangThaiTaiGoi(n);
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/du_doan.txt", bao.ToString());

        var rac = GameObject.Find("TAM_DuDoan");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.update += TraLaiCanh;
    }
}
