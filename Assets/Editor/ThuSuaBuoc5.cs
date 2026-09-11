using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: BON CHO HO CUA BUOC 5 DA DUOC VA CHUA.
///
/// Menu 43 (kiem toan) do ra hai lo hong bang so, va doc code thay them hai.
/// Phep thu nay do lai CHINH NHUNG TINH HUONG AY sau khi sua, theo dung thu tu
/// nguoi dung chon:
///
///   2. Quai chet gia ben may khach (va ban sao nguoi choi cung vay)
///   1. HUD may khach hien sai so quai
///   4. Phim R nap lai man giua tran mang
///   5. Mat ket noi giua tran khong duoc xu ly
///
/// Ket qua ghi ra <c>PlayTestShots/sua_buoc5.txt</c>.
/// </summary>
public static class ThuSuaBuoc5
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/44. Chay thu SUA BUOC 5 (2-1-4-5)", false, 131)]
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
        var go = new GameObject("TAM_SuaBuoc5");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[SuaBuoc5] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] bon cho ho cua buoc 5, theo thu tu 2 - 1 - 4 - 5");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        // Doi rai xong roi xoa sach - dong vai may khach, von khong co quai rieng
        yield return new WaitForSeconds(2.5f);
        var dir = GameDirector.Instance;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include,
                                                           FindObjectsSortMode.None))
            Object.DestroyImmediate(q.gameObject);

        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = false;
        yield return null;

        var goDb = new GameObject("TAM_SuaDb");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi; db.chiSoCuaToi = 1;
        var boQuai = goDb.AddComponent<DongBoQuai>();
        boQuai.dongBo = db; db.quai = boQuai;

        const int SoCon = 8;
        var ds = new GoiTin.MotQuai[SoCon];
        Vector3 goc = toi.transform.position + new Vector3(0f, 0f, 10f);
        for (int i = 0; i < SoCon; i++)
        {
            Vector3 p = goc + new Vector3((i % 4) * 2.5f - 4f, 0f, (i / 4) * 2.5f);
            p.y = VfxFactory.GroundY(p) + 0.15f;
            ds[i] = new GoiTin.MotQuai
            {
                id = (ushort)(8000 + i), loai = (byte)MonsterType.Skeleton,
                viTri = p, gocY = 0f, mau01 = 1f, daChet = false
            };
        }
        for (int k = 0; k < 5; k++)
        {
            boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
            yield return new WaitForSeconds(0.1f);
        }

        // ================================================================
        //  2. QUAI CHET GIA BEN MAY KHACH
        // ================================================================
        Ghi("");
        Ghi("--- 2. quai chet gia ben may khach ---");

        NhanDangQuai con = null;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id == 8000) con = n;

        if (con == null) { Ghi("[LOI] khong dung duoc ban sao quai"); loi++; }
        else
        {
            var mau = con.GetComponent<Damageable>();
            float truoc = mau.health;
            mau.TakeDamage(mau.maxHealth * 3f, DamageType.Fire, con.transform.position);
            yield return null;

            Ghi("2a. phep cua khach trung ban sao quai (gap 3 lan mau) -> IsDead = "
                + mau.IsDead + ", mau " + truoc.ToString("F0") + " -> "
                + mau.health.ToString("F0") + " (phai: khong chet, mau khong doi)");
            if (mau.IsDead) { Ghi("[LOI] ban sao quai van chet cuc bo"); loi++; }
            if (Mathf.Abs(mau.health - truoc) > 0.5f)
            { Ghi("[LOI] ban sao quai van bi tru mau cuc bo"); loi++; }

            // Chu phong bao "con nay da chet that" -> luc do moi duoc chet
            ds[0].mau01 = 0f; ds[0].daChet = true;
            boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
            yield return null;
            bool chetKhiDuocBao = mau == null || mau.IsDead;
            Ghi("2b. chu phong bao 'da chet' -> ban sao chet: " + chetKhiDuocBao
                + " (phai la True)");
            if (!chetKhiDuocBao) { Ghi("[LOI] chu phong bao chet ma ban sao khong chet"); loi++; }

            // Xac khong duoc danh nguoi
            var aiXac = con != null ? con.GetComponent<EnemyAI>() : null;
            var mauToi = toi.GetComponent<Damageable>();
            float truocXac = mauToi.health;
            if (aiXac != null)
                aiXac.DienLaiDon(0, toi.transform.position, mauToi);
            yield return null;
            float matVoiXac = truocXac - mauToi.health;
            Ghi("2c. mot goi 'ra don' den muon tu CAI XAC -> minh mat "
                + matVoiXac.ToString("F0") + " mau (phai la 0)");
            if (matVoiXac > 0f) { Ghi("[LOI] xac quai van danh duoc nguoi"); loi++; }

            // Goi ke tiep van nhac ten con da chet: khong duoc dung lai xac moi
            yield return new WaitForSeconds(0.3f);
            for (int k = 0; k < 3; k++)
            {
                boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
                yield return new WaitForSeconds(0.1f);
            }
            int soMang8000 = 0;
            foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
                if (n.id == 8000) soMang8000++;
            Ghi("2d. chu phong van nhac ten con da chet -> so vat the mang so 8000 tren canh: "
                + soMang8000 + " (phai <= 1, khong dung them xac)");
            if (soMang8000 > 1) { Ghi("[LOI] dung lai xac cho mot con da chet"); loi++; }
        }

        // Ban sao NGUOI CHOI khac cung vay
        var banSao = NguoiChoiKhac.Sinh("uid-bs", "Ban sao", toi.transform.position + new Vector3(4f, 0f, 0f));
        if (banSao != null)
        {
            var mauBs = banSao.GetComponent<Damageable>();
            float truocBs = mauBs.health;
            mauBs.TakeDamage(mauBs.maxHealth * 3f, DamageType.Fire, banSao.transform.position);
            yield return null;
            Ghi("2e. phep cua minh trung BAN SAO NGUOI CHOI (gap 3 lan mau) -> IsDead = "
                + mauBs.IsDead + ", mau khong doi: " + (Mathf.Abs(mauBs.health - truocBs) < 0.5f)
                + " (phai: False, True)");
            if (mauBs.IsDead) { Ghi("[LOI] ban sao nguoi choi chet cuc bo"); loi++; }
            NguoiChoiKhac.Bo(banSao);
        }

        // ================================================================
        //  1. HUD MAY KHACH
        // ================================================================
        Ghi("");
        Ghi("--- 1. HUD may khach ---");
        Ghi("1a. truoc khi nghe bang so: HUD ghi 'Quai con lai: " + dir.Alive
            + "'  'Da diet: " + dir.Kills + "'");

        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietBangSo(3, 27, 19, 0f)));
        yield return null; yield return null;

        Ghi("1b. chu phong bao dot 3, da diet 27, con 19 -> HUD may khach: 'Dot "
            + Mathf.Max(1, dir.Wave) + "  Quai con lai: " + dir.Alive + "  Da diet: " + dir.Kills + "'");
        if (dir.Wave != 3 || dir.Kills != 27 || dir.Alive != 19)
        { Ghi("[LOI] HUD may khach van khong khop chu phong"); loi++; }

        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietBangSo(3, 45, 0, 4.2f)));
        yield return null; yield return null;
        Ghi("1c. het dot, dot moi sau 4,2 giay -> HUD may khach: 'Dot moi sau "
            + Mathf.CeilToInt(dir.NextWaveIn) + " giay'");
        if (Mathf.Abs(dir.NextWaveIn - 4.2f) > 0.15f)
        { Ghi("[LOI] dem nguoc dot moi ben khach khong khop"); loi++; }

        // ================================================================
        //  4. PHIM R
        // ================================================================
        Ghi("");
        Ghi("--- 4. phim R ---");
        TranHienTai.DangChoiMang = false;
        bool donDuoc = GameDirector.DuocChoiLai;
        TranHienTai.DangChoiMang = true;
        bool mangDuoc = GameDirector.DuocChoiLai;
        Ghi("4. phim R nap lai man: choi mot minh = " + donDuoc + ", trong tran mang = "
            + mangDuoc + " (phai la True, False)");
        if (!donDuoc || mangDuoc) { Ghi("[LOI] phim R van pha duoc tran mang"); loi++; }

        // ================================================================
        //  5. MAT KET NOI
        // ================================================================
        Ghi("");
        Ghi("--- 5. mat ket noi giua tran ---");

        // Mo kenh gia lap va dung mot bo noi mang THAT de no tu xu ly
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");

        int quaiTruocIm = boQuai.SoQuaiDangGiu;
        float lucIm = Time.unscaledTime;

        // Nghe du lau de chac chan "da tung nghe", roi IM LANG han
        for (int k = 0; k < 3; k++)
        {
            boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
            yield return new WaitForSeconds(0.1f);
        }
        lucIm = Time.unscaledTime;

        // 3,5 giay: phai o "dang cho tin hieu", chua ket luan
        while (Time.unscaledTime - lucIm < 3.5f) yield return null;
        var tt35 = db.TinhTrang;
        Ghi("5a. im lang 3,5 giay -> tinh trang: " + tt35 + " (phai la DangChoTinHieu)");
        if (tt35 != DongBoTran.TinhTrangKetNoi.DangChoTinHieu)
        { Ghi("[LOI] khong bao 'dang cho tin hieu'"); loi++; }

        // 6 giay: qua nguong go quai cu (5 giay) - dan quai KHONG duoc bien mat
        while (Time.unscaledTime - lucIm < 6f) yield return null;
        Ghi("5b. im lang 6 giay (qua nguong go quai 5 giay) -> dan quai con "
            + boQuai.SoQuaiDangGiu + "/" + quaiTruocIm + " con (phai giu nguyen)");
        if (boQuai.SoQuaiDangGiu < quaiTruocIm)
        { Ghi("[LOI] ca dan quai bien mat trong luc cho tin hieu"); loi++; }

        // Tin hieu quay lai truoc 10 giay -> tro ve binh thuong
        boQuai.NhanGoiQuai(GoiTin.VietQuai(db.GioTran(), ds, 0, SoCon));
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietBangSo(3, 45, 7, 0f)));
        yield return null; yield return null;
        Ghi("5c. tin hieu quay lai o giay thu 6 -> tinh trang: " + db.TinhTrang
            + " (phai la Tot - chuyen tab mot lat khong duoc lam vo tran)");
        if (db.TinhTrang != DongBoTran.TinhTrangKetNoi.Tot)
        { Ghi("[LOI] tin hieu quay lai ma khong hoi phuc"); loi++; }

        // Im lang han 10,5 giay -> ket luan da roi tran
        bool daBaoMat = false;
        db.KhiMatKetNoi += () => daBaoMat = true;
        lucIm = Time.unscaledTime;
        while (Time.unscaledTime - lucIm < 10.5f) yield return null;
        Ghi("5d. im lang 10,5 giay -> tinh trang: " + db.TinhTrang + ", su kien mat ket noi da ban: "
            + daBaoMat + " (phai la DaMat, True)");
        if (db.TinhTrang != DongBoTran.TinhTrangKetNoi.DaMat || !daBaoMat)
        { Ghi("[LOI] khong ket luan duoc nguoi kia da roi tran"); loi++; }

        // Kenh dong han: ket luan NGAY, khong doi 10 giay.
        //
        // Go bo dong bo cu TRUOC: hai bo cung doc chung mot hang nhan, cai cu
        // co the nuot mat goi cua cai moi - va phep do se bao "khong nhan ra"
        // vi mot ly do chang lien quan gi.
        Object.DestroyImmediate(goDb);
        goDb = null;
        yield return null;

        var goDb2 = new GameObject("TAM_SuaDb2");
        var db2 = goDb2.AddComponent<DongBoTran>();
        db2.toi = toi; db2.chiSoCuaToi = 0;
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
        yield return null; yield return null;

        float lucDong = Time.unscaledTime;
        KenhTrucTiep.Dong();
        while (db2.TinhTrang != DongBoTran.TinhTrangKetNoi.DaMat
               && Time.unscaledTime - lucDong < 3f) yield return null;
        float matBaoLau = Time.unscaledTime - lucDong;
        Ghi("5e. nguoi kia DONG kenh (bam ve sanh / dong tab) -> ket luan sau "
            + (matBaoLau * 1000f).ToString("F0") + " ms (phai ngay, khong doi 10 giay)");
        if (db2.TinhTrang != DongBoTran.TinhTrangKetNoi.DaMat)
        { Ghi("[LOI] dong kenh ma khong nhan ra"); loi++; }

        // ---- Don ----
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id >= 8000) Object.DestroyImmediate(n.gameObject);
        if (goDb != null) Object.DestroyImmediate(goDb);
        Object.DestroyImmediate(goDb2);
        KenhTrucTiep.Dong();
        TranHienTai.Xoa();

        Ghi("");
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
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/sua_buoc5.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_SuaBuoc5", "TAM_SuaDb", "TAM_SuaDb2" })
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
