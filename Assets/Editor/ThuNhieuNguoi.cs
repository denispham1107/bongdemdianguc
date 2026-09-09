using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU BUOC 3: MOT CANH CHUA NHIEU NHAN VAT.
///
/// Do bon dieu:
///
///  1. SINH DUOC nhan vat cho nguoi khac, va no khong cuop camera cua minh.
///
///  2. QUAI CHIA NHAU RA - day la phep do co suc nang nhat. Truoc day quai
///     duoc gan thang GameDirector.player luc sinh ra, nen dat them mot nguoi
///     nua vao canh thi ca bay quai VAN chi duoi mot nguoi. Phai do rang co
///     con nham nguoi moi.
///
///  3. NGUOI CHET BI BO QUA khi chon muc tieu - quai khong dam vao mot cai xac.
///
///  4. BO NGUOI CHOI ra thi danh sach sach, khong con tham chieu chet.
///
/// Ket qua ghi ra <c>PlayTestShots/nhieu_nguoi.txt</c>.
/// </summary>
public static class ThuNhieuNguoi
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/32. Chay thu NHIEU NGUOI (buoc 3)", false, 119)]
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
        var go = new GameObject("TAM_NhieuNguoi");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[NhieuNguoi] " + s);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] buoc 3 - mot canh chua nhieu nhan vat");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat cua minh"); loi++; Ket(); yield break; }

        yield return new WaitForSeconds(2f);

        var dir = GameDirector.Instance;
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; Ket(); yield break; }

        Ghi("truoc khi them: " + dir.moiNguoi.Count + " nguoi trong danh sach");
        if (dir.moiNguoi.Count != 1)
        { Ghi("[LOI] choi mot minh ma danh sach khong phai 1 nguoi"); loi++; }

        int soCameraTruoc = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length;

        // ---- 1. SINH NHAN VAT CHO NGUOI KHAC ----
        // Dat cach xa nhan vat cua minh de con phan biet duoc quai nham ai
        Vector3 choBan = toi.transform.position + new Vector3(22f, 0f, 0f);
        var ban = NguoiChoiKhac.Sinh("uid-thu", "BanThu", choBan);

        yield return new WaitForSeconds(1f);

        Ghi("1. sinh nhan vat cho nguoi khac: " + (ban != null ? "OK - " + ban.name : "THAT BAI"));
        if (ban == null) { Ghi("[LOI] khong sinh duoc"); loi++; Ket(); yield break; }

        Ghi("   danh sach gio co " + dir.moiNguoi.Count + " nguoi (phai la 2)");
        if (dir.moiNguoi.Count != 2) { Ghi("[LOI] khong duoc them vao danh sach"); loi++; }

        Ghi("   nguoi khac tu doc ban phim khong: " + ban.tuDocInput + " (phai la False)");
        if (ban.tuDocInput) { Ghi("[LOI] nhan vat nguoi khac doc ban phim cua may nay"); loi++; }

        int soCameraSau = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length;
        Ghi("   so camera: " + soCameraTruoc + " -> " + soCameraSau + " (khong duoc tang)");
        if (soCameraSau > soCameraTruoc)
        { Ghi("[LOI] nhan vat nguoi khac mang theo camera - se cuop goc nhin"); loi++; }

        // ---- 2. QUAI CHIA NHAU RA ----
        // Cho mot nhip de quai ngo lai muc tieu (NhipChonLaiGiay = 0,7 giay)
        yield return new WaitForSeconds(2.5f);

        var quai = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        int nhamToi = 0, nhamBan = 0, nhamKhac = 0;

        foreach (var q in quai)
        {
            if (q == null || q.target == null) { nhamKhac++; continue; }
            if (q.target == toi.transform) nhamToi++;
            else if (q.target == ban.transform) nhamBan++;
            else nhamKhac++;
        }

        Ghi(string.Format("2. {0} con quai -> nham minh {1}, nham nguoi kia {2}, khac {3}",
                          quai.Length, nhamToi, nhamBan, nhamKhac));

        if (quai.Length == 0)
        { Ghi("[LOI] khong co con quai nao - phep do vo nghia"); loi++; }
        else if (nhamBan == 0)
        { Ghi("[LOI] dat mot nguoi cach 22 m ma KHONG con quai nao nham - "
              + "quai van om mot muc tieu nhu truoc"); loi++; }

        // ---- 3. NGUOI CHET BI BO QUA ----
        var mauBan = ban.GetComponent<Damageable>();
        if (mauBan != null)
        {
            // Dat nguoi kia sat ngay canh mot con quai roi giet no di: quai do
            // phai quay sang nham nguoi con song, chu khong dam vao xac.
            var gan = dir.GanNhat(choBan);
            Ghi("3. truoc khi chet, gan " + choBan + " nhat la: "
                + (gan == ban.transform ? "nguoi kia (dung)" : "nguoi khac"));

            mauBan.TakeDamage(999999f, DamageType.Physical, ban.transform.position);
            yield return new WaitForSeconds(0.5f);

            var ganSauChet = dir.GanNhat(choBan);
            bool boQua = ganSauChet != ban.transform;
            Ghi("   sau khi nguoi kia guc -> GanNhat tra ve nguoi khac: " + boQua
                + " (phai la True)");
            if (!boQua) { Ghi("[LOI] van nham vao nguoi da chet"); loi++; }

            Ghi("   so nguoi con song: " + dir.SoNguoiConSong() + " (phai la 1)");
            if (dir.SoNguoiConSong() != 1)
            { Ghi("[LOI] dem sai so nguoi con song"); loi++; }
        }

        // ---- 4. BO NGUOI CHOI ----
        NguoiChoiKhac.Bo(ban);
        yield return new WaitForSeconds(0.3f);

        Ghi("4. sau khi bo: danh sach con " + dir.moiNguoi.Count + " nguoi (phai la 1)");
        if (dir.moiNguoi.Count != 1) { Ghi("[LOI] bo khong sach"); loi++; }

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
        File.WriteAllText("PlayTestShots/nhieu_nguoi.txt", bao.ToString());

        var rac = GameObject.Find("TAM_NhieuNguoi");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.update += TraLaiCanh;
    }
}
