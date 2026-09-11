using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU BUOC 5: DANH GIET LAN NHAU.
///
/// Do BON chieu, va thieu chieu nao cung thanh mot loi khac nhau:
///
///  1. Choi mot minh  -> mask KHONG duoc co lop Player. Thieu chieu nay thi
///     nguoi choi don tu thieu minh ma khong hieu tai sao.
///  2. Choi doi khang -> mask PHAI co lop Player.
///  3. Danh nguoi khac -> ho MAT MAU that.
///  4. Danh chinh minh -> KHONG mat mau. Day la chieu de bat loi nguy hiem
///     nhat: bat PvP xong ma quen bo qua chinh minh thi qua cau lua no duoi
///     chan se giet nguoi vua bam phim.
///
/// Ket qua ghi ra <c>PlayTestShots/pvp.txt</c>.
/// </summary>
public static class ThuPvP
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/34. Chay thu PvP (buoc 5)", false, 121)]
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
        var go = new GameObject("TAM_PvP");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[PvP] " + s);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] buoc 5 - danh giet lan nhau");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        yield return new WaitForSeconds(2f);

        int lopNguoiChoi = LayerMask.NameToLayer("Player");
        Ghi("lop Player co so hieu: " + lopNguoiChoi);
        if (lopNguoiChoi < 0) { Ghi("[LOI] du an khong co lop Player"); loi++; Ket(); yield break; }

        // ---- 1. CHOI MOT MINH: mask KHONG duoc co lop Player ----
        // TranHienTai.DangChoiMang dang la false vi chua vao tran mang nao
        var maskDon = TranHienTai.DangChoiMang
            ? LayerMask.GetMask("Enemy", "Player")
            : LayerMask.GetMask("Enemy");

        bool coTrongDon = (maskDon & (1 << lopNguoiChoi)) != 0;
        Ghi("1. choi mot minh -> mask co lop Player: " + coTrongDon + " (phai la False)");
        if (coTrongDon) { Ghi("[LOI] choi don ma van danh duoc nguoi choi - se tu thieu minh"); loi++; }

        // ---- 2. CHOI DOI KHANG: mask PHAI co lop Player ----
        TranHienTai.DangChoiMang = true;
        var maskDau = TranHienTai.DangChoiMang
            ? LayerMask.GetMask("Enemy", "Player")
            : LayerMask.GetMask("Enemy");

        bool coTrongDau = (maskDau & (1 << lopNguoiChoi)) != 0;
        Ghi("2. choi doi khang -> mask co lop Player: " + coTrongDau + " (phai la True)");
        if (!coTrongDau) { Ghi("[LOI] bat che do dau ma van khong danh duoc nhau"); loi++; }

        // ---- Dung mot nguoi choi thu hai lam bia ----
        Vector3 choBia = toi.transform.position + new Vector3(4f, 0f, 0f);
        var bia = NguoiChoiKhac.Sinh("uid-bia", "Bia", choBia);
        if (bia == null) { Ghi("[LOI] khong sinh duoc bia"); loi++; Ket(); yield break; }

        // Bia phai nam o lop Player thi moi trung don duoc
        bia.gameObject.layer = lopNguoiChoi;
        foreach (var t in bia.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = lopNguoiChoi;

        var mauBia = bia.GetComponent<Damageable>();
        // Bia nay dong vai "nguoi choi TREN MAY CUA HO" - mau do chinh may nay
        // quyet. NguoiChoiKhac.Sinh mac dinh tat viec tru mau cuc bo (ban sao
        // chi ve lai), nen phai bat lai o day, khong thi phep do sat thuong
        // luon ra 0 va khong con noi len dieu gi.
        if (mauBia != null) mauBia.mauDoMayKhacQuyet = false;
        var mauToi = toi.GetComponent<Damageable>();
        if (mauBia == null || mauToi == null)
        { Ghi("[LOI] thieu Damageable"); loi++; Ket(); yield break; }

        yield return new WaitForSeconds(0.5f);

        // ---- 3. DANH NGUOI KHAC -> HO MAT MAU ----
        float biaTruoc = mauBia.health;
        float toiTruoc = mauToi.health;

        // No mot qua cau lua ngay tren dau bia, va bo qua chinh minh
        CombatUtil.AreaDamage(choBia + Vector3.up * 0.5f, 3.5f, 40f,
                              maskDau, DamageType.Fire, 0f, mauToi);

        yield return new WaitForSeconds(0.3f);

        float biaMat = biaTruoc - mauBia.health;
        Ghi(string.Format("3. no ngay tren dau nguoi kia -> ho mat {0:F0} mau "
                          + "({1:F0} -> {2:F0})", biaMat, biaTruoc, mauBia.health));
        if (biaMat <= 0f) { Ghi("[LOI] danh nguoi khac ma ho khong mat mau"); loi++; }

        // ---- 4. DANH CHINH MINH -> KHONG MAT MAU ----
        toiTruoc = mauToi.health;
        CombatUtil.AreaDamage(toi.transform.position, 3.5f, 40f,
                              maskDau, DamageType.Fire, 0f, mauToi);

        yield return new WaitForSeconds(0.3f);

        float toiMat = toiTruoc - mauToi.health;
        Ghi(string.Format("4. no ngay duoi chan MINH -> minh mat {0:F0} mau "
                          + "(phai la 0)", toiMat));
        if (toiMat > 0f) { Ghi("[LOI] tu thieu chinh minh - quen bo qua nguoi tung phep"); loi++; }

        // ---- 4b. KHONG BO QUA thi PHAI mat mau ----
        // Chieu nguoc lai: neu khong truyen boQua thi don PHAI trung minh.
        // Thieu phep do nay thi khong phan biet duoc "bo qua dung" voi "phep
        // khong trung ai ca".
        toiTruoc = mauToi.health;
        CombatUtil.AreaDamage(toi.transform.position, 3.5f, 40f,
                              maskDau, DamageType.Fire, 0f, null);

        yield return new WaitForSeconds(0.3f);

        float toiMat2 = toiTruoc - mauToi.health;
        Ghi(string.Format("4b. cung cu no do nhung KHONG bo qua -> minh mat {0:F0} mau "
                          + "(phai lon hon 0)", toiMat2));
        if (toiMat2 <= 0f)
        { Ghi("[LOI] khong bo qua ma van khong trung - phep nay khong danh duoc nguoi choi"); loi++; }

        // ---- Don ----
        NguoiChoiKhac.Bo(bia);
        TranHienTai.DangChoiMang = false;

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }

    static void Ket()
    {
        TranHienTai.DangChoiMang = false;
        File.WriteAllText("PlayTestShots/pvp.txt", bao.ToString());

        var rac = GameObject.Find("TAM_PvP");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.update += TraLaiCanh;
    }
}
