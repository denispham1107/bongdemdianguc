using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: NGUOI KHAC VA DAN QUAI PHAI BUOC CHAN, KHONG PHAI TRUOT.
///
/// Nguoi choi bao: chu phong thay nguoi kia "dang bay"; may khach thi thay CA
/// DAN QUAI lan nguoi kia deu bay - truot tren mat dat, chan khong nhuc nhich.
///
/// Nguyen nhan chung cho ca hai: ban sao duoc DAT THANG vi tri theo goi tin,
/// khong di qua CharacterController. Ma MOI bo hoat hinh trong game deu doc
/// toc do tu <c>cc.velocity</c> - va velocity cua chung luon bang 0.
///
/// Phep thu do CON SO ma bo hoat hinh nhan duoc, chu khong chup anh: mot buoc
/// chan dung hay sai thi anh tinh khong noi len duoc, con con so thi co.
///
/// Ket qua ghi ra <c>PlayTestShots/nhipbuoc.txt</c>.
/// </summary>
public static class ThuNhipBuoc
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/41. Chay thu NHIP BUOC qua mang", false, 128)]
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
        var go = new GameObject("TAM_NhipBuoc");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[NhipBuoc] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] nguoi khac va dan quai phai buoc chan, khong phai truot");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = false;          // dong vai may khach
        yield return new WaitForSeconds(2f);

        // ================================================================
        //  PHAN 1: BAN SAO NGUOI CHOI KHAC
        // ================================================================
        Vector3 choKia = toi.transform.position + new Vector3(6f, 0f, 0f);
        choKia.y = VfxFactory.GroundY(choKia) + 0.15f;
        var kia = NguoiChoiKhac.Sinh("uid-kia", "Nguoi kia", choKia);
        if (kia == null) { Ghi("[LOI] khong sinh duoc ban sao"); loi++; Ket(); yield break; }

        var goDb = new GameObject("TAM_DongBoNhip");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);

        // Cho ho DI: nhet mot chuoi goi vi tri tien deu, dung nhu mot nguoi
        // dang chay ngang qua.
        float toiDaKhiDi = 0f;
        int soGoi = 0;
        for (int i = 0; i < 30; i++)
        {
            Vector3 cho = choKia + new Vector3(0f, 0f, i * 0.09f);   // ~5,4 m/giay
            NhetViTri(db, 1, cho, false);
            soGoi++;
            yield return null;
            toiDaKhiDi = Mathf.Max(toiDaKhiDi, DocNhipNguoi(kia));
        }

        Ghi("1. nguoi kia chay (" + soGoi + " goi) -> nhip buoc bo hoat hinh nhan duoc: "
            + toiDaKhiDi.ToString("F2") + " (phai lon hon 0)");
        if (toiDaKhiDi <= 0.01f)
        { Ghi("[LOI] nguoi kia truot tren mat dat - dung hien tuong nguoi choi bao"); loi++; }

        // Dung lai thi chan phai dung theo, khong dap mai
        Vector3 choDung = choKia + new Vector3(0f, 0f, 30 * 0.09f);
        for (int i = 0; i < 25; i++) { NhetViTri(db, 1, choDung, false); yield return null; }

        float khiDung = DocNhipNguoi(kia);
        Ghi("2. nguoi kia dung lai -> nhip buoc: " + khiDung.ToString("F2")
            + " (phai gan 0)");
        if (khiDung > 0.15f)
        { Ghi("[LOI] dung roi ma chan van dap"); loi++; }

        // ================================================================
        //  PHAN 2: DAN QUAI BEN MAY KHACH
        // ================================================================
        var goQuai = new GameObject("TAM_BoQuaiNhip");
        var boQuai = goQuai.AddComponent<DongBoQuai>();
        boQuai.dongBo = db;
        db.quai = boQuai;

        Vector3 choQuai = toi.transform.position + new Vector3(-7f, 0f, 0f);
        choQuai.y = VfxFactory.GroundY(choQuai) + 0.15f;

        float quaiToiDa = 0f;
        GameObject conQuai = null;
        for (int i = 0; i < 30; i++)
        {
            NhetViTriQuai(boQuai, db, 9100, choQuai + new Vector3(0f, 0f, i * 0.07f));
            yield return null;

            if (conQuai == null)
                foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
                    if (n.id == 9100) conQuai = n.gameObject;

            if (conQuai != null) quaiToiDa = Mathf.Max(quaiToiDa, DocNhipQuai(conQuai));
        }

        if (conQuai == null) { Ghi("[LOI] khach khong dung duoc con quai"); loi++; }
        else
        {
            Ghi("3. quai di ben may khach -> nhip buoc: " + quaiToiDa.ToString("F2")
                + " (phai lon hon 0)");
            if (quaiToiDa <= 0.01f)
            { Ghi("[LOI] ca dan quai truot tren mat dat"); loi++; }

            Vector3 quaiDung = choQuai + new Vector3(0f, 0f, 30 * 0.07f);
            for (int i = 0; i < 25; i++)
            { NhetViTriQuai(boQuai, db, 9100, quaiDung); yield return null; }

            float quaiKhiDung = DocNhipQuai(conQuai);
            Ghi("4. quai dung lai -> nhip buoc: " + quaiKhiDung.ToString("F2")
                + " (phai gan 0)");
            if (quaiKhiDung > 0.15f) { Ghi("[LOI] quai dung roi ma chan van dap"); loi++; }
        }

        // ---- 5. Ban sao khong duoc tu di ----
        //
        // Ban dau toi doi o day "cc.velocity phai bang 0", vi tuong nguyen nhan
        // la bo hoat hinh doc mot van toc luon rong. Phep thu bac lai ngay: no
        // do duoc 1,35 m/giay. Gia thiet cua toi sai, va cai sai do suyt dan
        // den mot ban sua khong dung cho.
        //
        // Nguyen nhan that: ban sao VAN CHAY tron vong di chuyen cua
        // PlayerController voi input rong - vua goi cc.Move keo minh theo trong
        // luc cua may nay, vua dat nhip buoc ve 0 MOI KHUNG HINH, de len con so
        // ma DongBoTran vua tinh.
        //
        // Nen cau hoi dung khong phai "van toc co bang 0 khong" ma la "ban sao
        // co con tu di nua khong": no phai khong tu dat duoc nhip buoc ve 0.
        for (int i = 0; i < 12; i++)
        {
            NhetViTri(db, 1, choDung + new Vector3(0f, 0f, i * 0.09f), false);
            yield return null;
        }

        var ccKia = kia.GetComponent<CharacterController>();
        float vanTocCu = ccKia != null ? new Vector2(ccKia.velocity.x, ccKia.velocity.z).magnitude : -1f;
        float nhipMoi = DocNhipNguoi(kia);

        Ghi("5. trong luc ban sao dang chay: nhip buoc giu duoc = " + nhipMoi.ToString("F2")
            + " (phai lon hon 0 - khong bi vong di chuyen dat lai ve 0)");
        Ghi("   (cc.velocity cua ban sao: " + vanTocCu.ToString("F2")
            + " m/giay - chi de tham khao, no khong con la nguon nua)");
        if (nhipMoi <= 0.01f)
        { Ghi("[LOI] nhip buoc bi dat lai ve 0 - ban sao van tu chay vong di chuyen"); loi++; }

        // ---- Don ----
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id >= 9100) Object.DestroyImmediate(n.gameObject);
        Object.DestroyImmediate(goQuai);
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(kia);
        TranHienTai.Xoa();

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>Nhip buoc ma bo hoat hinh cua nhan vat nay dang nhan.</summary>
    static float DocNhipNguoi(PlayerController pc)
    {
        float ra = 0f;

        var anim = pc.GetComponentInChildren<ProceduralAnimator>();
        if (anim != null) ra = Mathf.Max(ra, anim.TocDoDi01);

        var hh = pc.GetComponentInChildren<NguoiChoiHoatHinh>();
        if (hh != null && hh.tocDoEp >= 0f) ra = Mathf.Max(ra, hh.tocDoEp);

        return ra;
    }

    static float DocNhipQuai(GameObject vat)
    {
        float ra = 0f;

        var anim = vat.GetComponentInChildren<ProceduralAnimator>();
        if (anim != null) ra = Mathf.Max(ra, anim.TocDoDi01);

        var model = vat.GetComponentInChildren<ModelHoatHinh>();
        if (model != null && model.tocDoEp >= 0f) ra = Mathf.Max(ra, model.tocDoEp);

        return ra;
    }

    static void NhetViTri(DongBoTran db, byte chiSo, Vector3 cho, bool daChet)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = chiSo, viTri = cho, gocY = 0f,
            mau01 = 1f, dangChay = true, daChet = daChet
        };
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
    }

    static void NhetViTriQuai(DongBoQuai bq, DongBoTran db, ushort id, Vector3 cho)
    {
        var ds = new GoiTin.MotQuai[1];
        ds[0] = new GoiTin.MotQuai
        {
            id = id, loai = (byte)MonsterType.Skeleton, viTri = cho,
            gocY = 0f, mau01 = 1f, daChet = false
        };
        bq.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, 1));
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
        File.WriteAllText("PlayTestShots/nhipbuoc.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_NhipBuoc", "TAM_DongBoNhip", "TAM_BoQuaiNhip" })
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
