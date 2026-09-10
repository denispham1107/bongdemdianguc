using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CHAY THU: VAO MAN CHOI MANG THI CO AI DUNG DAY BO NOI MANG KHONG.
///
/// Hai nguoi vao dung mot phong, dung mot man, nhung van khong thay nhau - va
/// tren man hinh KHONG co lay mot dong chu nao cua phan mang. Do la dau hieu
/// noi rang <see cref="KhoiDongTranMang"/> chua bao gio duoc dung day.
///
/// Phep thu nay khong tin vao mat: no vao Play tu MainMenu (dung nhu nguoi
/// choi), bat co DangChoiMang, nap Act2, roi DEM xem trong canh co
/// KhoiDongTranMang khong. Dem duoc thi khong con gi de doan.
///
/// Ket qua ghi ra <c>PlayTestShots/tugan.txt</c>.
/// </summary>
public static class ThuTuGan
{
    const string CanhDau = "Assets/Scenes/MainMenu.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/36. Chay thu TU GAN bo noi mang", false, 123)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != CanhDau)
            EditorSceneManager.OpenScene(CanhDau);

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_TuGan");

        // PHAI SONG QUA LAN NAP MAN. Khong danh dau thi chinh vat the dang
        // chay phep thu bi huy giua chung, coroutine dut ngang va khong ai bao
        // gi ca - nhin het nhu phep thu "chay xong ma khong ghi ket qua".
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TuGan] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] vao man choi mang thi co ai dung day bo noi mang khong");
        Ghi("canh dau: " + SceneManager.GetActiveScene().name);

        // PHAI THU CA HAI MAN. Act1 dung bang code con Act2 la scene nuong san
        // - mot cach gan hong o mot man ma khong hong o man kia la chuyen da
        // xay ra nhieu lan trong du an nay.
        yield return ThuMotMan("Act2");
        yield return ThuMotMan("Act1");

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static IEnumerator ThuMotMan(string manCanThu)
    {
        Ghi("");
        Ghi("--- man " + manCanThu + " ---");
        yield return new WaitForSeconds(1f);

        // Dung nhu ManSanh.VaoTran lam ngay truoc khi nap man
        TranHienTai.MaPhong = "-thu-tu-gan";
        TranHienTai.ManChoi = manCanThu;
        TranHienTai.LaHost = true;
        TranHienTai.DangChoiMang = true;
        Ghi("da bat DangChoiMang = true, sap nap " + manCanThu);

        // XOA DAU VET CUA LAN THU TRUOC. Hai truong nay la static nen chung
        // song qua lan nap man - khong xoa thi lan thu Act1 co the doc lai ket
        // qua cua Act2 va bao "dat" ma chang do gi ca.
        KhoiDongTranMang.TrangThai = "";
        KhoiDongTranMang.NhanDang = "";
        KhoiDongTranMang.DaNoi = false;
        KhoiDongTranMang.LoiCuoi = null;

        SceneManager.LoadScene(manCanThu);

        // Cho man moi nap xong han
        float han = Time.time + 20f;
        while (SceneManager.GetActiveScene().name != manCanThu && Time.time < han)
            yield return null;
        yield return new WaitForSeconds(2f);

        Ghi("canh dang mo sau khi nap: " + SceneManager.GetActiveScene().name);

        // ---- Phep do chinh: bo noi mang co trong canh khong ----
        var bo = Object.FindAnyObjectByType<KhoiDongTranMang>();
        Ghi("1. KhoiDongTranMang trong canh: " + (bo == null ? "KHONG CO" : "co"));
        if (bo == null)
        {
            Ghi("[LOI] khong ai dung day bo noi mang - hai may khong the tim thay nhau");
            loi++;
        }

        // ---- Chieu doc lap: dong chu tren man hinh ----
        // Neu bo noi mang co chay thi no phai dat NhanDang. Do them chieu nay
        // vi day chinh la thu nguoi choi NHIN THAY - hoac khong nhin thay.
        yield return new WaitForSeconds(1f);
        Ghi("2. dong nhan dang tren man hinh: \""
            + (string.IsNullOrEmpty(KhoiDongTranMang.NhanDang) ? "(rong)" : KhoiDongTranMang.NhanDang)
            + "\"");
        if (string.IsNullOrEmpty(KhoiDongTranMang.NhanDang))
        {
            Ghi("[LOI] man hinh khong noi gi ca - dung nhu anh chup cua nguoi choi");
            loi++;
        }

        // ---- Chieu 3: no co tim thay nhan vat cua minh khong ----
        //
        // Dung day duoc roi ma khong tim ra nhan vat thi cung khong noi duoc
        // voi ai. Trong Editor thi khong co WebRTC, nen buoc tiep theo la mot
        // dong noi thang dieu do - va chinh dong ay chung minh no da di qua
        // duoc doan tim nhan vat.
        float han2 = Time.time + 30f;
        while (Time.time < han2)
        {
            string tt = KhoiDongTranMang.TrangThai ?? "";
            if (tt.Length > 0 && !tt.StartsWith("Đang tìm")) break;
            yield return null;
        }

        string trang = KhoiDongTranMang.TrangThai ?? "";
        Ghi("3. buoc tiep theo cua bo noi mang: \"" + trang + "\"");
        if (trang.Contains("khong dung duoc nhan vat"))
        { Ghi("[LOI] khong tim thay nhan vat cua minh trong man choi"); loi++; }
        else if (trang.StartsWith("Đang tìm"))
        { Ghi("[LOI] treo o buoc tim nhan vat suot 30 giay"); loi++; }
        else
        { Ghi("   (da qua duoc doan tim nhan vat - trong Editor thi dung o day "
              + "vi WebRTC chi co tren ban web)"); }
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
        File.WriteAllText("PlayTestShots/tugan.txt", bao.ToString());

        var rac = GameObject.Find("TAM_TuGan");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        EditorApplication.update += TraLaiCanh;
    }
}
