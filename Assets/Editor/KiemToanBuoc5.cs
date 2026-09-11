using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// KIEM TOAN BUOC 5: DO NHUNG CHO NGHI CON HO, KHONG SUA GI CA.
///
/// Nguoi dung hoi "buoc 5 da xong het chua". Tra loi bang tri nho thi de noi
/// "xong roi". Nen day la mot phep do CHI DE BAO CAO - no dung lai dung tinh
/// huong tren may khach roi ghi ra so, de biet cho nao con ho that.
///
/// Hai cho nghi nang nhat:
///   A. May khach KHONG rai quai - vay HUD cua no dem quai tu dau? GameHUD doc
///      director.Alive va director.Kills, ma hai con so ay dem tu danh sach
///      rieng cua GameDirector - danh sach ay ben khach rong.
///   B. Phep cua chinh nguoi khach van trung ban sao quai ben may khach, tru
///      mau CUC BO. Neu mau ay ve 0 truoc goi tin ke tiep thi con quai co chet
///      ben khach trong khi ben chu phong no con song khong?
///
/// Ket qua ghi ra <c>PlayTestShots/kiemtoan_buoc5.txt</c>.
/// </summary>
public static class KiemToanBuoc5
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/43. Kiem toan buoc 5 (chi do, khong sua)", false, 130)]
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

        bao.Length = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_KiemToan");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[KiemToan] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[kiem toan buoc 5] chi do, khong sua");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("khong tim thay nhan vat"); Ket(); yield break; }

        // Doi GameDirector rai xong, roi xoa sach quai that de dong vai may
        // khach - may khach von khong co con quai nao cua rieng no.
        yield return new WaitForSeconds(2.5f);
        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = false;

        var dir = GameDirector.Instance;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include,
                                                           FindObjectsSortMode.None))
            Object.DestroyImmediate(q.gameObject);
        yield return null;

        var goDb = new GameObject("TAM_KiemToanDb");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi; db.chiSoCuaToi = 1;
        var boQuai = goDb.AddComponent<DongBoQuai>();
        boQuai.dongBo = db; db.quai = boQuai;

        // Chu phong ke lai mot dan 12 con
        const int SoCon = 12;
        var ds = new GoiTin.MotQuai[SoCon];
        Vector3 goc = toi.transform.position + new Vector3(0f, 0f, 9f);
        for (int i = 0; i < SoCon; i++)
        {
            Vector3 p = goc + new Vector3((i % 4) * 2.5f - 4f, 0f, (i / 4) * 2.5f);
            p.y = VfxFactory.GroundY(p) + 0.15f;
            ds[i] = new GoiTin.MotQuai
            {
                id = (ushort)(7000 + i), loai = (byte)MonsterType.Skeleton,
                viTri = p, gocY = 0f, mau01 = 1f, daChet = false
            };
        }

        for (int k = 0; k < 6; k++)
        {
            boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
            yield return new WaitForSeconds(0.1f);
        }

        // ---- A. HUD may khach dem quai tu dau ----
        int thatSuTrenCanh = Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None).Length;
        Ghi("");
        Ghi("A. MAY KHACH dang giu " + boQuai.SoQuaiDangGiu + " con quai (tren canh: "
            + thatSuTrenCanh + " con)");
        Ghi("   HUD se hien: 'Quai con lai: " + (dir != null ? dir.Alive.ToString() : "?")
            + "'   'Da diet: " + (dir != null ? dir.Kills.ToString() : "?") + "'"
            + "   'Dot " + (dir != null ? Mathf.Max(1, dir.Wave).ToString() : "?") + "'");
        if (dir != null && dir.NextWaveIn > 0.01f && dir.Alive == 0)
            Ghi("   va dong duoi se la: 'Dot moi sau " + Mathf.CeilToInt(dir.NextWaveIn)
                + " giay...' - DUNG YEN MAI, vi dong ho dot chi chay o chu phong");
        Ghi("   => " + (dir != null && dir.Alive != boQuai.SoQuaiDangGiu
                        ? "HO: HUD may khach sai so quai" : "khop"));

        // ---- B. Phep cua khach giet ban sao quai CUC BO ----
        //
        // Lay mot con, tru mau no cuc bo nhu phep cua nguoi khach vua trung -
        // du de chet - roi de goi tin chu phong ke tiep noi "con nay van song
        // nguyen mau" (vi ben do, don co the truot, hoac quai chiu duoc).
        NhanDangQuai conThu = null;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id == 7000) conThu = n;

        if (conThu == null) { Ghi("B. khong tim thay con quai de thu"); }
        else
        {
            var mau = conThu.GetComponent<Damageable>();
            Ghi("");
            Ghi("B. con quai so 7000 ben khach: " + mau.health.ToString("F0") + "/"
                + mau.maxHealth.ToString("F0") + " mau");

            mau.TakeDamage(mau.maxHealth * 3f, DamageType.Fire, conThu.transform.position);
            yield return null;
            Ghi("   phep cua khach trung no cuc bo -> IsDead = " + mau.IsDead
                + " (ben chu phong con so nay chua biet gi)");

            // Chu phong van ke: con nay song, day mau
            for (int k = 0; k < 10; k++)
            {
                boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
                yield return new WaitForSeconds(0.1f);
            }

            bool vatConSong = conThu != null && conThu.gameObject != null;
            var mauSau = vatConSong ? conThu.GetComponent<Damageable>() : null;
            Ghi("   1 giay sau, chu phong van bao 'song, 100% mau':");
            Ghi("     vat the goc con ton tai = " + vatConSong
                + (mauSau != null ? ", IsDead = " + mauSau.IsDead
                                  + ", mau = " + mauSau.health.ToString("F0") : ""));

            int cungSo = 0;
            foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
                if (n.id == 7000) cungSo++;
            Ghi("     so vat the mang so hieu 7000 tren canh = " + cungSo);

            bool hoB = mauSau != null && mauSau.IsDead;
            Ghi("   => " + (hoB ? "HO: quai chet ben khach nhung song ben chu phong - "
                              + "khach thay xac nam do ma chu phong van bi no danh"
                              : "khong ho"));
        }

        // ---- Don ----
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id >= 7000) Object.DestroyImmediate(n.gameObject);
        Object.DestroyImmediate(goDb);
        TranHienTai.Xoa();
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
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/kiemtoan_buoc5.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_KiemToan", "TAM_KiemToanDb" })
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
