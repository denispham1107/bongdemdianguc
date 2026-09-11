using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CHAY THU: CHE DO BON BO XUONG.
///
/// Nguoi dung xin: vao man chi co 4 con bo xuong, khong loai nao khac; giet het
/// thi doi 30 giay ra 4 con moi; lap lai mai.
///
/// Phep thu vao Play THAT o CA HAI man (Act1 va Act2 dung khac han nhau, va
/// scene cua chung ghi de khac nhau), roi dem tung con quai theo loai - khong
/// tin vao con so HUD, vi HUD dem tu danh sach cua GameDirector, tuc la cai
/// dang can kiem.
///
/// Doi 30 giay bang cach TANG TOC thoi gian game (Time.timeScale), va do bang
/// dong ho game Time.time - khong phai dong ho that: 30 giay game moi la con so
/// nguoi choi cam thay.
///
/// Ket qua ghi ra <c>PlayTestShots/bonboxuong.txt</c>.
/// </summary>
public static class ThuBonBoXuong
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/47. Chay thu CHE DO BON BO XUONG", false, 134)]
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
        var go = new GameObject("TAM_BonBoXuong");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BonBoXuong] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] che do bon bo xuong (moi dot " + GameDirector.SoBoXuongMoiDot
            + " con, cho " + GameDirector.GiayChoDotBoXuong + " giay)");

        yield return ThuMotMan("Act2");
        yield return ThuMotMan("Act1");

        Time.timeScale = 1f;
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>Dem quai dang song tren canh, theo loai.</summary>
    static string DemTheoLoai(out int tong, out int boXuong)
    {
        var dem = new Dictionary<string, int>();
        tong = 0; boXuong = 0;
        foreach (var ai in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            var d = ai.GetComponent<Damageable>();
            if (d != null && d.IsDead) continue;

            var nd = ai.GetComponent<NhanDangQuai>();
            string loai = nd != null ? nd.loai.ToString() : ai.name;
            int c; dem.TryGetValue(loai, out c); dem[loai] = c + 1;
            tong++;
            if (nd != null && nd.loai == MonsterType.Skeleton) boXuong++;
        }

        var sb = new StringBuilder();
        foreach (var cap in dem) sb.Append(cap.Key).Append('=').Append(cap.Value).Append(' ');
        return tong == 0 ? "(khong con con nao)" : sb.ToString().Trim();
    }

    static void GietHet()
    {
        foreach (var ai in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            var d = ai.GetComponent<Damageable>();
            if (d != null && !d.IsDead) d.Die();
        }
    }

    static IEnumerator ThuMotMan(string man)
    {
        Ghi("");
        Ghi("--- man " + man + " ---");
        Time.timeScale = 1f;

        SceneManager.LoadScene(man);
        float han = Time.realtimeSinceStartup + 25f;
        while (SceneManager.GetActiveScene().name != man && Time.realtimeSinceStartup < han)
            yield return null;
        yield return new WaitForSeconds(2f);

        var dir = GameDirector.Instance;
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; yield break; }

        // Nhan vat bat tu trong luc do: bon con bo xuong se danh no, va neu no
        // guc thi GameDirector dung ca nhip dot - phep do se cho mai.
        var pc = Object.FindAnyObjectByType<PlayerController>();
        var mauToi = pc != null ? pc.GetComponent<Damageable>() : null;

        int tong, bx;

        // ---- 1. Vao man ----
        string luc0 = DemTheoLoai(out tong, out bx);
        Ghi("1. vao man 2 giay: quai dang song = " + tong + " [" + luc0 + "]  (phai: dung 4 bo xuong)");
        Kiem(tong == 4 && bx == 4, "vao man khong phai dung 4 con bo xuong");

        // ---- 2-4. Hai vong: giet het -> doi 30 giay -> 4 con moi ----
        for (int vong = 1; vong <= 2; vong++)
        {
            GietHet();
            yield return null; yield return null;

            float lucGiet = Time.time;
            DemTheoLoai(out tong, out bx);
            Ghi(string.Format("{0}a. vong {1}: giet het -> con {2} con, HUD dem nguoc: {3:F1} giay",
                              vong + 1, vong, tong, dir.NextWaveIn));
            Kiem(tong == 0, "giet het ma van con quai");

            // Tang toc thoi gian cho do phai doi 30 giay that
            Time.timeScale = 10f;

            // Truoc moc 30 giay thi KHONG duoc co con nao
            float lucRaSom = -1f;
            while (Time.time - lucGiet < GameDirector.GiayChoDotBoXuong - 1f)
            {
                DemTheoLoai(out tong, out bx);
                if (tong > 0 && lucRaSom < 0f) lucRaSom = Time.time - lucGiet;
                if (mauToi != null) mauToi.health = mauToi.maxHealth;
                yield return null;
            }

            // Doi den khi dot moi ra
            float lucRa = -1f;
            float hanRa = lucGiet + GameDirector.GiayChoDotBoXuong + 10f;
            while (Time.time < hanRa)
            {
                DemTheoLoai(out tong, out bx);
                if (tong > 0) { lucRa = Time.time - lucGiet; break; }
                yield return null;
            }
            Time.timeScale = 1f;
            yield return null;

            string sau = DemTheoLoai(out tong, out bx);
            Ghi(string.Format("{0}b. dot moi ra sau {1:F1} giay game (dat {2:F0}) -> {3} con [{4}]",
                              vong + 1, lucRa, GameDirector.GiayChoDotBoXuong, tong, sau));
            Kiem(lucRaSom < 0f, "co quai ra som truoc 30 giay (" + lucRaSom.ToString("F1") + " giay)");
            Kiem(lucRa > 0f && Mathf.Abs(lucRa - GameDirector.GiayChoDotBoXuong) < 1.5f,
                 "dot moi khong ra dung 30 giay");
            Kiem(tong == 4 && bx == 4, "dot moi khong phai dung 4 con bo xuong");
        }

        // ---- 4. Khong ai giet thi KHONG sinh them ----
        // Dong quy du co dong ho rieng 120 giay, quy cay 240 giay - neu che do
        // chay thu con de lot chung thi day la luc chung hien ra.
        Time.timeScale = 20f;
        float luc4 = Time.time;
        int nhieuNhat = 0;
        while (Time.time - luc4 < 260f)
        {
            DemTheoLoai(out tong, out bx);
            if (tong > nhieuNhat) nhieuNhat = tong;
            if (mauToi != null) mauToi.health = mauToi.maxHealth;
            yield return null;
        }
        Time.timeScale = 1f;
        yield return null;
        string cuoi = DemTheoLoai(out tong, out bx);
        Ghi("4. de nguyen 260 giay game (qua ca dong ho quy du 120 va quy cay 240) -> nhieu nhat "
            + nhieuNhat + " con, luc cuoi [" + cuoi + "]  (phai khong qua 4, chi bo xuong)");
        Kiem(nhieuNhat <= 4 && tong == bx, "van sinh them quai khac khi khong ai giet");
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }

    static void Ket()
    {
        Time.timeScale = 1f;
        File.WriteAllText("PlayTestShots/bonboxuong.txt", bao.ToString());

        var rac = GameObject.Find("TAM_BonBoXuong");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        EditorApplication.update += TraLaiCanh;
    }
}
