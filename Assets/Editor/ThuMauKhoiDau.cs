using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: VAO MAN LA CO DU MAU KHOI DAU.
///
/// Con so mau nguoi choi nam o BA cho, va sua thieu mot cho la khong doi gi ca:
///
///   1. <c>GameBootstrap.playerMaxHealth</c> - mac dinh viet trong code
///   2. Gia tri luu trong Act1.unity va Act2.unity - Unity luu gia tri cua
///      component vao scene, va no DE LEN mac dinh o (1)
///   3. <c>Player_Sorceress.prefab</c> - ban sao cua nguoi choi khac lay mau
///      tu day, khong di qua GameBootstrap
///
/// Phep thu vao Play THAT o ca hai man roi doc mau tu Damageable - khong doc
/// file, vi doc file thi chi chung minh duoc "da sua chu", con cai chay len
/// man hinh thi chua chac.
///
/// Ket qua ghi ra <c>PlayTestShots/mau_khoi_dau.txt</c>.
/// </summary>
public static class ThuMauKhoiDau
{
    /// <summary>Muc mau dang mong doi. Doi o day khi doi muc mau chay thu.</summary>
    public const float MauMongDoi = 30000f;

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/40. Chay thu MAU KHOI DAU", false, 127)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_MauKhoiDau");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[MauKhoiDau] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] vao man la co du mau khoi dau (mong doi "
            + MauMongDoi.ToString("F0") + ")");

        yield return ThuMotMan("Act2");
        yield return ThuMotMan("Act1");

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static IEnumerator ThuMotMan(string man)
    {
        Ghi("");
        Ghi("--- man " + man + " ---");

        UnityEngine.SceneManagement.SceneManager.LoadScene(man);

        float han = Time.time + 25f;
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != man
               && Time.time < han)
            yield return null;

        PlayerController toi = null;
        han = Time.time + 25f;
        while (toi == null && Time.time < han)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; yield break; }

        yield return new WaitForSeconds(1.5f);

        var mau = toi.GetComponent<Damageable>();
        if (mau == null) { Ghi("[LOI] nhan vat khong co Damageable"); loi++; yield break; }

        Ghi("1. nhan vat cua minh: " + mau.health.ToString("F0") + " / "
            + mau.maxHealth.ToString("F0"));
        if (Mathf.Abs(mau.maxHealth - MauMongDoi) > 0.5f)
        { Ghi("[LOI] mau toi da khong dung - xem lai scene " + man); loi++; }
        if (Mathf.Abs(mau.health - MauMongDoi) > 0.5f)
        { Ghi("[LOI] vao man ma khong day mau"); loi++; }

        // ---- Ban sao cua nguoi choi khac ----
        // No KHONG di qua GameBootstrap, no lay mau thang tu prefab. Thieu
        // chieu nay thi sua prefab hay khong cung khong ai biet.
        TranHienTai.DangChoiMang = true;
        var kia = NguoiChoiKhac.Sinh("uid-thu", "Nguoi kia",
                                     toi.transform.position + new Vector3(4f, 0f, 0f));
        if (kia == null) { Ghi("[LOI] khong sinh duoc ban sao"); loi++; }
        else
        {
            var mauKia = kia.GetComponent<Damageable>();
            Ghi("2. ban sao nguoi choi khac: " + mauKia.health.ToString("F0") + " / "
                + mauKia.maxHealth.ToString("F0") + " (lay thang tu prefab)");
            if (Mathf.Abs(mauKia.maxHealth - MauMongDoi) > 0.5f)
            { Ghi("[LOI] prefab nguoi choi con giu muc mau cu"); loi++; }
            NguoiChoiKhac.Bo(kia);
        }
        TranHienTai.Xoa();

        // ---- Danh thu mot cai xem mau co tru dung khong ----
        // Mau lon ma thanh mau khong nhuc nhich thi cung vo dung.
        float truoc = mau.health;
        mau.TakeDamage(250f, DamageType.Fire, toi.transform.position);
        yield return new WaitForSeconds(0.2f);
        float mat = truoc - mau.health;
        Ghi("3. an mot don 250 -> mat " + mat.ToString("F0") + " mau, con "
            + mau.health.ToString("F0"));
        if (mat <= 0f) { Ghi("[LOI] mau khong tru duoc"); loi++; }

        // ---- 4. Con so co lot trong thanh mau khong ----
        //
        // "30000 / 30000" dai hon "400 / 400" gan gap ruoi. Bao xong ma nguoi
        // dung mo ra thay chu tran ra ngoai thanh thi cung nhu chua lam.
        //
        // BOC TRY/CATCH: lan truoc ca chieu do nay bien mat khoi bao cao ma
        // khong ai bao gi - mot ngoai le trong coroutine bi Unity cat im lang,
        // va console thi day loi hat che mat. Gio no phai tu khai ra giay.
        DoBeRongChu(mau.maxHealth);
    }

    /// <summary>Do xem con so co lot trong thanh mau khong.</summary>
    static void DoBeRongChu(float mauToiDa)
    {
        try
        {
            float ti = Screen.height / 1080f;
            // Be rong THAT cua thanh (trong khung HUD kinh di, tru le) - khong phai ca bang
            float rongThanh = GameHUD.RongThanh(ti);

            // Do bang DUNG font HUD dang ve (Inter dam) - font mac dinh cu da bo
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Inter-SemiBold.ttf");
            if (font == null)
            { Ghi("4. khong nap duoc font Inter - bo qua chieu nay"); return; }

            var kieu = new GUIStyle();
            kieu.font = font;
            kieu.fontSize = GameHUD.KieuHUD.Co(GameHUD.CoChuMau, ti);   // co chu thanh mau

            string chu = Mathf.CeilToInt(mauToiDa) + " / " + Mathf.CeilToInt(mauToiDa);
            float rongChu = kieu.CalcSize(new GUIContent(chu)).x;

            Ghi("4. chu \"" + chu + "\" rong " + rongChu.ToString("F0")
                + " diem, thanh mau rong " + rongThanh.ToString("F0")
                + " diem (man hinh cao " + Screen.height + ")");
            if (rongChu > rongThanh)
            { Ghi("[LOI] con so tran ra ngoai thanh mau"); loi++; }
        }
        catch (System.Exception e)
        {
            Ghi("4. [khong do duoc] " + e.GetType().Name + ": " + e.Message);
        }
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
        File.WriteAllText("PlayTestShots/mau_khoi_dau.txt", bao.ToString());

        var rac = GameObject.Find("TAM_MauKhoiDau");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        EditorApplication.update += TraLaiCanh;
    }
}
