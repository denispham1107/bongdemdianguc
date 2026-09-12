using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: CHO XUAT PHAT NGAU NHIEN + LUAT DOT QUAI MOI CUA ACT2.
///
/// Nguoi dung xin (12/09/2026):
///   - Het dem nguoc 10 giay, moi nguoi hien ra mot cho NGAU NHIEN tren ban do,
///     khong ai gan ai.
///   - Act2 bo het luat cu (bon bo xuong, quai rai san, dong quy du / quy cay).
///     Moi dot: quanh MOI nguoi choi bon con - bo xuong, mu phu thuy, quy cay,
///     quy du. Giet het -> doi 30 giay -> dot sau y het, cong them quai bat ki
///     (cong don: +1, +3, +6...). Moi dot quai manh hon 5% mau va 5% sat thuong.
///
/// Do bang SO, khong doan:
///   A. Cho xuat phat: cung ma phong thi hai may ra CUNG danh sach; khoang cach
///      nho nhat giua hai nguoi; cho co nam tren dat / ngoai nuoc / khong vuong
///      vat can khong.
///   B. Act2 vao man: dem tung loai quai quanh tung nguoi choi.
///   C. Dot sau: so quai va he so manh (mau, sat thuong) so voi dot truoc.
///   D. Act1 KHONG bi doi luat.
///
/// Ket qua ghi ra <c>PlayTestShots/dotquai_act2.txt</c>.
/// </summary>
public static class ThuDotQuaiAct2
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/56. Chay thu DOT QUAI Act2 + cho xuat phat", false, 147)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu dot quai",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 3] cho xuat phat ngau nhien + luat dot quai Act2");

        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_DotQuai") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_DotQuai");
        // PHAI giu qua lan nap canh: phan D nap Act1, khong giu thi kich ban
        // chet giua chung va bao cao khong bao gio duoc ghi (da vap).
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DotQuai] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    /// <summary>Dem quai dang song theo loai.</summary>
    static Dictionary<MonsterType, int> DemTheoLoai(out int tong)
    {
        var d = new Dictionary<MonsterType, int>();
        tong = 0;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            var mau = n.GetComponent<Damageable>();
            if (mau == null || mau.IsDead) continue;
            d[n.loai] = (d.TryGetValue(n.loai, out int c) ? c : 0) + 1;
            tong++;
        }
        return d;
    }

    static string ViDem(Dictionary<MonsterType, int> d)
    {
        var sb = new StringBuilder();
        foreach (var kv in d) sb.Append(kv.Key + "=" + kv.Value + " ");
        return sb.ToString().Trim();
    }

    /// <summary>
    /// Mau va sat thuong cua mot con bo xuong DANG SONG.
    ///
    /// Phai loc xac chet: xac nam lai 6 giay sau khi chet, va lan do dau tien
    /// cua toi vo phai xac dot truoc nen dot 2 va 3 deu bao "manh x1,000" -
    /// bon loi oan.
    /// </summary>
    static void DoBoXuongDangSong(out float mau, out float sat)
    {
        mau = 0f; sat = 0f;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            if (n.loai != MonsterType.Skeleton) continue;
            var m = n.GetComponent<Damageable>();
            if (m == null || m.IsDead) continue;
            var ai = n.GetComponent<EnemyAI>();
            mau = m.maxHealth;
            if (ai != null) sat = ai.attackDamage;
            return;
        }
    }

    /// <summary>Giet sach quai dang song - de xem dot sau.</summary>
    static void GietSach()
    {
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            var mau = n.GetComponent<Damageable>();
            if (mau != null && !mau.IsDead) mau.TakeDamage(mau.maxHealth * 10f, DamageType.Fire, n.transform.position);
        }
    }

    static IEnumerator KichBan()
    {
        // ================================================================
        // A. CHO XUAT PHAT
        // ================================================================
        Ghi("");
        Ghi("A. cho xuat phat ngau nhien (ham thuan, khong can vao tran)");

        var dir = GameDirector.Instance;
        float han = Time.time + 25f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }
        yield return new WaitForSeconds(1f);

        int hat = ChoXuatPhat.HatTuMaPhong("PHONG-123");
        var cho1 = ChoXuatPhat.ChoChoCaPhong(hat, 4, dir.arenaCenter, dir.arenaRadius);
        var cho2 = ChoXuatPhat.ChoChoCaPhong(hat, 4, dir.arenaCenter, dir.arenaRadius);

        bool khop = cho1.Count == cho2.Count;
        for (int i = 0; khop && i < cho1.Count; i++) khop = Vector3.Distance(cho1[i], cho2[i]) < 0.001f;
        Ghi("A1. cung ma phong -> hai may tinh ra cung danh sach: " + khop);
        Kiem(khop, "hai may ra hai danh sach khac nhau - nguoi nay se thay nguoi kia o cho khac");

        var khac = ChoXuatPhat.ChoChoCaPhong(ChoXuatPhat.HatTuMaPhong("PHONG-456"), 4,
                                             dir.arenaCenter, dir.arenaRadius);
        bool doiPhong = Vector3.Distance(cho1[0], khac[0]) > 1f;
        Ghi("A2. phong khac -> cho khac: " + doiPhong);
        Kiem(doiPhong, "phong nao cung xuat phat o dung mot cho");

        float ganNhat = float.MaxValue;
        for (int i = 0; i < cho1.Count; i++)
            for (int j = i + 1; j < cho1.Count; j++)
                ganNhat = Mathf.Min(ganNhat, Vector3.Distance(cho1[i], cho1[j]));
        Ghi("A3. bon cho: " + cho1[0].ToString("F1") + " " + cho1[1].ToString("F1") + " "
            + cho1[2].ToString("F1") + " " + cho1[3].ToString("F1"));
        Ghi("    hai nguoi gan nhau nhat: " + ganNhat.ToString("F1") + " m (can >= "
            + ChoXuatPhat.CachNhauToiThieu + ")");
        Kiem(ganNhat >= ChoXuatPhat.CachNhauToiThieu - 0.01f, "hai nguoi xuat phat qua gan nhau");

        // Cho co dung tren dat, ngoai nuoc, khong vuong vat can khong
        int treoLoLung = 0, trongNuoc = 0, vuong = 0;
        var nuoc = GameObject.Find("MatNuoc");
        foreach (var p in cho1)
        {
            RaycastHit hit;
            bool chamDat = Physics.Raycast(p + Vector3.up * 60f, Vector3.down, out hit, 120f, ~0,
                                           QueryTriggerInteraction.Ignore);
            if (!chamDat || Mathf.Abs(hit.point.y - p.y) > 0.4f) treoLoLung++;
            if (chamDat && !(hit.collider is TerrainCollider)
                && hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground")) vuong++;
            if (nuoc != null)
                foreach (var r in nuoc.GetComponentsInChildren<Renderer>())
                {
                    var b = r.bounds;
                    if (p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z
                        && p.y < b.max.y + 0.3f) { trongNuoc++; break; }
                }
            if (Physics.CheckCapsule(p + Vector3.up * 0.55f, p + Vector3.up * 1.9f, 0.45f,
                                     1 << 0, QueryTriggerInteraction.Ignore)) vuong++;
        }
        Ghi("A4. bon cho: lo lung " + treoLoLung + ", duoi nuoc " + trongNuoc + ", vuong vat can " + vuong);
        Kiem(treoLoLung == 0, "co cho khong nam tren mat dat");
        Kiem(trongNuoc == 0, "co cho nam duoi nuoc");
        Kiem(vuong == 0, "co cho vuong bia / da / nha");

        // ================================================================
        // B. DOT DAU O ACT2
        // ================================================================
        Ghi("");
        Ghi("B. Act2 - dot dau");
        Ghi("che do dot quanh nguoi choi: " + dir.CheDoDotQuanhNguoi);
        Kiem(dir.CheDoDotQuanhNguoi, "Act2 khong bat luat dot moi");

        // Them mot nguoi choi gia (nhu ban sao nguoi khac) de do "quanh MOI nguoi"
        var toi = Object.FindAnyObjectByType<PlayerController>();
        Kiem(toi != null, "khong tim thay nhan vat");
        TranHienTai.DangChoiMang = true;
        var kia = NguoiChoiKhac.Sinh("uid-thu", "Người thứ hai",
                                     toi.transform.position + new Vector3(26f, 0f, 0f));
        yield return null;
        Ghi("B1. so nguoi choi trong canh: " + dir.moiNguoi.Count);
        Kiem(dir.moiNguoi.Count == 2, "phai co hai nguoi de do luat 'quanh moi nguoi'");

        // Xoa sach roi goi dot dau cho gon (Start co the da sinh mot dot)
        GietSach();
        yield return new WaitForSeconds(0.4f);
        dir.SinhDotQuanhNguoi();
        yield return new WaitForSeconds(0.5f);

        int tong1;
        var dem1 = DemTheoLoai(out tong1);
        Ghi("B2. dot " + dir.Wave + ": tong " + tong1 + " con - " + ViDem(dem1));
        Kiem(tong1 == 8, "dot dau phai la 8 con (2 nguoi x 4 loai), dang co " + tong1);
        foreach (var loai in new[] { MonsterType.Skeleton, MonsterType.Witch, MonsterType.QuyCay, MonsterType.QuyDu })
            Kiem(dem1.TryGetValue(loai, out int c) && c == 2, "thieu " + loai + " - moi nguoi phai co mot con");

        // Quai co dung quanh nguoi choi khong
        float xaNhat = 0f; int xaQua = 0;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            float gan = float.MaxValue;
            foreach (var t in dir.moiNguoi)
                if (t != null) gan = Mathf.Min(gan, Vector3.Distance(n.transform.position, t.position));
            xaNhat = Mathf.Max(xaNhat, gan);
            if (gan > GameDirector.XaNhatQuanhNguoi + 3f) xaQua++;
        }
        Ghi("B3. con quai xa nguoi choi nhat: " + xaNhat.ToString("F1") + " m (dat trong "
            + GameDirector.GanNhatQuanhNguoi + "-" + GameDirector.XaNhatQuanhNguoi + " m), so con xa qua: " + xaQua);
        Kiem(xaQua == 0, "co quai sinh xa nguoi choi - khong dung 'quanh nguoi choi'");

        float mauGoc, satGoc;
        DoBoXuongDangSong(out mauGoc, out satGoc);
        Ghi("B4. bo xuong dot 1: mau " + mauGoc.ToString("F1") + ", sat thuong " + satGoc.ToString("F1"));

        // ================================================================
        // C. DOT SAU: SO QUAI VA DO MANH
        // ================================================================
        Ghi("");
        Ghi("C. cac dot sau");

        int[] mongDoi = { 9, 11, 14 };            // 8 + 1, + 3, + 6 (cong don)
        for (int dot = 2; dot <= 4; dot++)
        {
            GietSach();
            yield return new WaitForSeconds(0.4f);
            dir.SinhDotQuanhNguoi();
            yield return new WaitForSeconds(0.5f);

            int tong;
            var dem = DemTheoLoai(out tong);
            float mau, sat;
            DoBoXuongDangSong(out mau, out sat);

            float heSoMongDoi = Mathf.Pow(1.05f, dir.Wave - 1);
            Ghi("C" + dot + ". dot " + dir.Wave + ": tong " + tong + " con (mong doi " + mongDoi[dot - 2] + ") - "
                + ViDem(dem));
            Ghi("     bo xuong: mau " + mau.ToString("F1") + " (x" + (mauGoc > 0f ? mau / mauGoc : 0f).ToString("F3")
                + "), sat thuong " + sat.ToString("F1") + " (x" + (satGoc > 0f ? sat / satGoc : 0f).ToString("F3")
                + "), mong doi x" + heSoMongDoi.ToString("F3"));
            Kiem(tong == mongDoi[dot - 2], "so quai dot " + dir.Wave + " khong dung");
            if (mauGoc > 0f) Kiem(Mathf.Abs(mau / mauGoc - heSoMongDoi) < 0.01f, "mau khong tang 5% moi dot");
            if (satGoc > 0f) Kiem(Mathf.Abs(sat / satGoc - heSoMongDoi) < 0.01f, "sat thuong khong tang 5% moi dot");
        }

        // ================================================================
        // D. ACT1 GIU NGUYEN
        // ================================================================
        Ghi("");
        Ghi("D. Act1 giu nguyen luat cu");
        TranHienTai.Xoa();
        if (kia != null) NguoiChoiKhac.Bo(kia);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Act1");
        float hanAct1 = Time.time + 25f;
        GameDirector d1 = null;
        while ((d1 == null || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Act1")
               && Time.time < hanAct1)
        { d1 = Object.FindAnyObjectByType<GameDirector>(); yield return null; }
        yield return new WaitForSeconds(2.5f);
        if (d1 == null) d1 = Object.FindAnyObjectByType<GameDirector>();
        int tongAct1;
        var demAct1 = DemTheoLoai(out tongAct1);
        Ghi("D1. Act1: che do dot quanh nguoi = " + (d1 != null && d1.CheDoDotQuanhNguoi)
            + " (phai la False), quai dang co: " + tongAct1 + " - " + ViDem(demAct1));
        Kiem(d1 != null && !d1.CheDoDotQuanhNguoi, "Act1 bi doi sang luat moi");

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[DotQuai] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/dotquai_act2.txt", bao.ToString());
        var rac = GameObject.Find("TAM_DotQuai");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
