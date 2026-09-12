using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: DONG BANG, CHOANG VA MAU KHIENG PHAI GIONG NHAU TREN MOI MAY.
///
/// Truoc day moi may tu gieo hieu ung cho ban sao cua nguoi khac, bang
/// Random.value rieng - A thay B bi dong bang, B thi van chay nhay; khieng cua
/// B vo tren may A trong khi tren may B con nua mau.
///
/// Gio may chu so huu la trong tai: nguoi choi tu ke hieu ung va khieng cua
/// minh, chu phong ke hieu ung cua quai. Ban sao chi ve lai.
///
/// Phep thu do CA HAI CHIEU cua moi cho gac: ban sao khong tu gieo duoc, VA
/// nhan vat that van gieo duoc binh thuong. Thieu chieu thu hai thi khong phan
/// biet duoc "gac dung" voi "hieu ung hong han".
///
/// Ket qua ghi ra <c>PlayTestShots/hieuung_mang.txt</c>.
/// </summary>
public static class ThuHieuUngMang
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/46. Chay thu HIEU UNG qua mang", false, 133)]
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
        var go = new GameObject("TAM_HieuUng");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[HieuUng] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] dong bang, choang va mau khieng giong nhau tren moi may");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        yield return new WaitForSeconds(2.5f);
        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include,
                                                           FindObjectsSortMode.None))
            Object.DestroyImmediate(q.gameObject);

        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = false;
        yield return null;

        var mauToi = toi.GetComponent<Damageable>();
        var banSao = NguoiChoiKhac.Sinh("uid-kia", "Nguoi kia", toi.transform.position + new Vector3(5f, 0f, 0f));
        var mauBs = banSao.GetComponent<Damageable>();

        // ================================================================
        //  1. CHO GAC: ban sao khong tu gieo, nhan vat that van gieo duoc
        // ================================================================
        Ghi("");
        Ghi("--- 1. cho gac ---");

        FrozenEffect.Apply(mauBs, 3f);
        StunnedEffect.Apply(mauBs, 3f);
        bool bsBang = banSao.GetComponent<FrozenEffect>() != null;
        bool bsChoang = banSao.GetComponent<StunnedEffect>() != null;

        FrozenEffect.Apply(mauToi, 3f);
        StunnedEffect.Apply(mauToi, 3f);
        bool toiBang = toi.GetComponent<FrozenEffect>() != null;
        bool toiChoang = toi.GetComponent<StunnedEffect>() != null;

        Ghi("1a. gieo dong bang + choang: len BAN SAO -> " + bsBang + "/" + bsChoang
            + " (phai False/False); len NHAN VAT THAT -> " + toiBang + "/" + toiChoang
            + " (phai True/True)");
        Kiem(!bsBang && !bsChoang, "ban sao van tu gieo hieu ung - hai may se thay hai ket qua");
        Kiem(toiBang && toiChoang, "nhan vat that khong con bi hieu ung - cho gac chan nham");

        // AreaFreeze ti le 100% - duong ma phep Mua bang di qua
        int mask = LayerMask.GetMask("Enemy", "Player");
        int soDong;
        CombatUtil.AreaFreeze(banSao.transform.position + Vector3.up * 0.5f, 2f, 1f, mask,
                              0.5f, 2f, 1f, 3f, null, out soDong);
        Ghi("1b. AreaFreeze ti le 100% trung ban sao -> ban sao co bang: "
            + (banSao.GetComponent<FrozenEffect>() != null) + " (phai False)");
        Kiem(banSao.GetComponent<FrozenEffect>() == null, "AreaFreeze van dong bang ban sao cuc bo");

        // ================================================================
        //  2. MAY CHU SO HUU DOC DUNG CAI MINH DANG BI
        // ================================================================
        Ghi("");
        Ghi("--- 2. doc trang thai de gui di ---");

        byte coToi = HieuUngQuaMang.DocCo(toi.gameObject);
        Ghi("2a. nhan vat that dang bi dong bang + choang -> co gui di: bang="
            + ((coToi & HieuUngQuaMang.CoBang) != 0) + " dongCung="
            + ((coToi & HieuUngQuaMang.CoBangHoanToan) != 0) + " choang="
            + ((coToi & HieuUngQuaMang.CoChoang) != 0) + " (phai True/True/True)");
        Kiem(coToi == (HieuUngQuaMang.CoBang | HieuUngQuaMang.CoBangHoanToan | HieuUngQuaMang.CoChoang),
             "khong doc dung hieu ung dang co");

        var khToi = Khieng.Bat(toi.gameObject, 150f, 3f);
        mauToi.khieng = khToi;
        khToi.HapThu(75f);
        float kh01 = HieuUngQuaMang.DocKhieng(toi.gameObject);
        Ghi("2b. khieng 150 mau an 75 -> gui di " + kh01.ToString("F2") + " (phai 0,50)");
        Kiem(Mathf.Abs(kh01 - 0.5f) < 0.02f, "doc sai mau khieng");

        // ================================================================
        //  3. GOI TIN CHO DUOC
        // ================================================================
        Ghi("");
        Ghi("--- 3. goi tin ---");
        var dsG = new GoiTin.MotNguoi[1];
        dsG[0] = new GoiTin.MotNguoi
        {
            chiSo = 1, viTri = Vector3.one, gocY = 30f, mau01 = 0.7f,
            dangChay = true, daChet = false,
            coHieuUng = (byte)(HieuUngQuaMang.CoBang | HieuUngQuaMang.CoChoang), khieng01 = 0.42f
        };
        byte[] bt = GoiTin.VietTrangThai(100, dsG, 1);
        var dsD = new GoiTin.MotNguoi[2]; int moc;
        GoiTin.DocTrangThai(bt, dsD, out moc);
        Ghi("3a. goi trang thai " + bt.Length + " byte (khong to them): hieu ung "
            + dsG[0].coHieuUng + " -> " + dsD[0].coHieuUng + ", khieng 0,42 -> "
            + dsD[0].khieng01.ToString("F2") + ", dangChay/daChet con nguyen: "
            + dsD[0].dangChay + "/" + dsD[0].daChet);
        Kiem(bt.Length == 18, "goi trang thai to ra");
        Kiem(dsD[0].coHieuUng == dsG[0].coHieuUng && Mathf.Abs(dsD[0].khieng01 - 0.42f) < 0.01f
             && dsD[0].dangChay && !dsD[0].daChet, "goi trang thai doc ra khong khop");

        var dsQ = new GoiTin.MotQuai[2];
        dsQ[0] = new GoiTin.MotQuai { id = 5, loai = 1, daChet = true,  coHieuUng = HieuUngQuaMang.CoBang };
        dsQ[1] = new GoiTin.MotQuai { id = 6, loai = 1, daChet = false, coHieuUng = HieuUngQuaMang.CoChoang };
        var dsQd = new GoiTin.MotQuai[16];
        GoiTin.DocQuai(GoiTin.VietQuai(1, dsQ, 0, 2), dsQd, out moc);
        Ghi("3b. goi quai: (chet, bang) -> (" + dsQd[0].daChet + ", " + dsQd[0].coHieuUng
            + "); (song, choang) -> (" + dsQd[1].daChet + ", " + dsQd[1].coHieuUng + ")");
        Kiem(dsQd[0].daChet && dsQd[0].coHieuUng == HieuUngQuaMang.CoBang
             && !dsQd[1].daChet && dsQd[1].coHieuUng == HieuUngQuaMang.CoChoang,
             "goi quai tron lan co chet va co hieu ung");

        // ================================================================
        //  4. BAN SAO VE LAI THEO LOI KE
        // ================================================================
        Ghi("");
        Ghi("--- 4. ban sao ve lai ---");

        KenhTrucTiep.Dong();
        KenhTrucTiep.GiaLapMo(0);
        var goDb = new GameObject("TAM_HieuUngDb");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi; db.chiSoCuaToi = 0;
        db.ThemNguoi(1, banSao);
        yield return null;

        GuiTrangThai(db, banSao.transform.position,
                     (byte)(HieuUngQuaMang.CoBang | HieuUngQuaMang.CoBangHoanToan | HieuUngQuaMang.CoChoang), 0.6f);
        yield return null; yield return null;

        var fBs = banSao.GetComponent<FrozenEffect>();
        var sBs = banSao.GetComponent<StunnedEffect>();
        var kBs = banSao.GetComponent<Khieng>();
        Ghi("4a. may kia ke 'dong cung + choang + khieng 60%' -> ban sao: bang="
            + (fBs != null && fBs.IsFullyFrozen) + " choang=" + (sBs != null && sBs.IsStunned)
            + " khieng=" + (kBs != null && kBs.DangBat ? kBs.Mau01.ToString("F2") : "khong"));
        Kiem(fBs != null && fBs.IsFullyFrozen, "ban sao khong dong bang theo");
        Kiem(sBs != null && sBs.IsStunned, "ban sao khong choang theo");
        Kiem(kBs != null && kBs.DangBat && Mathf.Abs(kBs.Mau01 - 0.6f) < 0.02f, "khieng ban sao khong khop");

        // Don danh tren may nay KHONG duoc tru mau khieng ban sao
        mauBs.TakeDamage(100f, DamageType.Fire, banSao.transform.position);
        yield return null;
        Ghi("4b. don 100 trung ban sao tren may nay -> khieng ban sao: "
            + (kBs != null ? kBs.Mau01.ToString("F2") : "?") + " (phai giu 0,60)");
        Kiem(kBs != null && kBs.DangBat && Mathf.Abs(kBs.Mau01 - 0.6f) < 0.02f,
             "khieng ban sao bi tru cuc bo - se vo som hon khieng that");

        // May kia het het: tan bang, het choang, khieng vo
        GuiTrangThai(db, banSao.transform.position, 0, 0f);
        yield return new WaitForSeconds(0.5f);
        fBs = banSao.GetComponent<FrozenEffect>();
        sBs = banSao.GetComponent<StunnedEffect>();
        bool conBang = fBs != null && fBs.remaining > 0f;
        bool conChoang = sBs != null && sBs.IsStunned;
        bool conKhieng = kBs != null && kBs.DangBat;
        Ghi("4c. may kia ke 'het ca' -> ban sao con bang/choang/khieng: " + conBang + "/"
            + conChoang + "/" + conKhieng + " (phai False/False/False)");
        Kiem(!conBang && !conChoang && !conKhieng, "ban sao giu hieu ung sau khi ben kia da het");

        // Mat goi: hieu ung tu tan chu khong ket vinh vien
        GuiTrangThai(db, banSao.transform.position, HieuUngQuaMang.CoBang, 0f);
        yield return null;
        yield return new WaitForSeconds(HieuUngQuaMang.GiuSongGiay + 0.6f);
        fBs = banSao.GetComponent<FrozenEffect>();
        bool ketBang = fBs != null && fBs.remaining > 0f;
        Ghi("4d. mot goi 'dong bang' roi goi tin ngung han -> sau "
            + (HieuUngQuaMang.GiuSongGiay + 0.6f).ToString("F2") + " giay ban sao con bang: "
            + ketBang + " (phai False - khong ket lai vinh vien)");
        Kiem(!ketBang, "hieu ung ket vinh vien khi mat goi");

        // Phat lai phep Khieng tren ban sao: CHI dong tac, khieng den tu goi trang thai
        banSao.TungPhepTheoMang(5, banSao.transform.position);
        yield return new WaitForSeconds(1.2f);
        kBs = banSao.GetComponent<Khieng>();
        bool khiengTuPhep = kBs != null && kBs.DangBat;
        Ghi("4e. phat lai phep Khieng tren ban sao, chua co goi trang thai -> co khieng: "
            + khiengTuPhep + " (phai False - mot nguon duy nhat, khong nhap nhay)");
        Kiem(!khiengTuPhep, "ban sao tu dung khieng - hai nguon se dap nhau");

        // ================================================================
        //  5. QUAI BEN MAY KHACH
        // ================================================================
        Ghi("");
        Ghi("--- 5. quai ben may khach ---");
        var boQuai = goDb.AddComponent<DongBoQuai>();
        boQuai.dongBo = db; db.quai = boQuai;

        Vector3 choQ = toi.transform.position + new Vector3(-6f, 0f, 0f);
        choQ.y = VfxFactory.GroundY(choQ) + 0.15f;
        var q1 = new GoiTin.MotQuai[1];
        q1[0] = new GoiTin.MotQuai
        {
            id = 9300, loai = (byte)MonsterType.Skeleton, viTri = choQ, mau01 = 1f,
            coHieuUng = HieuUngQuaMang.CoChoang
        };
        boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), q1, 0, 1));
        yield return null;

        GameObject conQ = null;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id == 9300) conQ = n.gameObject;
        bool qChoang = conQ != null && conQ.GetComponent<StunnedEffect>() != null;
        Ghi("5a. chu phong ke 'con quai nay dang choang' -> ban sao quai choang: " + qChoang);
        Kiem(qChoang, "quai ben khach khong choang theo chu phong");

        q1[0].coHieuUng = 0;
        boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), q1, 0, 1));
        yield return new WaitForSeconds(0.3f);
        var stQ = conQ != null ? conQ.GetComponent<StunnedEffect>() : null;
        Ghi("5b. chu phong ke 'het choang' -> ban sao quai con choang: "
            + (stQ != null && stQ.IsStunned) + " (phai False)");
        Kiem(stQ == null || !stQ.IsStunned, "quai ben khach van choang sau khi chu phong da het");

        // ---- Don ----
        if (conQ != null) Object.DestroyImmediate(conQ);
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(banSao);
        KenhTrucTiep.Dong();
        TranHienTai.Xoa();

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void GuiTrangThai(DongBoTran db, Vector3 cho, byte co, float khieng01)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = 1, viTri = cho, gocY = 0f, mau01 = 1f,
            coHieuUng = co, khieng01 = khieng01
        };
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
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
        KenhTrucTiep.Dong();
        File.WriteAllText("PlayTestShots/hieuung_mang.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_HieuUng", "TAM_HieuUngDb" })
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
