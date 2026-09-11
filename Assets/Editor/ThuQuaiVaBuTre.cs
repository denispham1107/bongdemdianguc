using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: CHUNG MOT DAN QUAI, VA DON TRUNG THEO CAI NGUOI BAN NHIN THAY.
///
/// Hai viec nguoi choi bao con thieu:
///
///  1. Moi may tu rai dan quai rieng, nen hai nguoi dung canh nhau ma danh hai
///     dan khac han - ca hai man hinh cung ghi "quai con lai 33" mot cach doc
///     lap. Sua: chu phong la trong tai, khach chi ve lai.
///
///  2. O 28 ms do tre, nguoi chay ngang ne duoc don du tren man hinh nguoi ban
///     no trung ro rang. Sua: lui nan nhan ve dung khoanh khac nguoi ban bam.
///
/// Ket qua ghi ra <c>PlayTestShots/quai_butre.txt</c>.
/// </summary>
public static class ThuQuaiVaBuTre
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/38. Chay thu QUAI CHUNG va BU TRE", false, 125)]
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
        var go = new GameObject("TAM_QuaiButre");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[QuaiButre] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] chung mot dan quai, va don trung theo cai nguoi ban nhin thay");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        yield return new WaitForSeconds(2f);

        // ================================================================
        //  PHAN 1: DAN QUAI
        // ================================================================

        // ---- 1. Ai duoc rai quai ----
        // Choi mot minh: co. Chu phong: co. Khach: KHONG.
        TranHienTai.DangChoiMang = false;
        bool donCo = GameDirector.LaTrongTaiCuaQuai;

        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = true;
        bool hostCo = GameDirector.LaTrongTaiCuaQuai;

        TranHienTai.LaHost = false;
        bool khachCo = GameDirector.LaTrongTaiCuaQuai;

        Ghi("1. duoc tu rai quai? choi don=" + donCo + " chu phong=" + hostCo
            + " khach=" + khachCo + " (phai la True True False)");
        if (!donCo || !hostCo || khachCo)
        { Ghi("[LOI] sai nguoi lam trong tai - hai ben se danh hai dan khac nhau"); loi++; }

        TranHienTai.LaHost = true;

        // ---- 2. Moi con quai phai co so hieu ----
        // Thieu so hieu la may khach khong biet con nao la con nao, va con ay
        // se khong bao gio hien len ben do.
        var dsQuai = Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None);
        var dsAI = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        int trung = 0;
        var daGap = new System.Collections.Generic.HashSet<ushort>();
        foreach (var n in dsQuai) if (!daGap.Add(n.id)) trung++;

        Ghi("2. quai trong canh: " + dsAI.Length + " con, co so hieu: " + dsQuai.Length
            + ", so hieu trung nhau: " + trung + " (phai bang nhau, va 0 trung)");
        if (dsQuai.Length != dsAI.Length)
        { Ghi("[LOI] con quai khong co so hieu se khong hien len may khach"); loi++; }
        if (trung > 0) { Ghi("[LOI] hai con cung mot so hieu"); loi++; }

        // ---- 3. Goi quai: viet ra roi doc lai phai khop ----
        var dsGoc = new GoiTin.MotQuai[3];
        for (int i = 0; i < 3; i++)
            dsGoc[i] = new GoiTin.MotQuai
            {
                id = (ushort)(100 + i), loai = (byte)MonsterType.Skeleton,
                viTri = new Vector3(3.21f * i, 0.5f, -7.65f), gocY = 90f * i,
                mau01 = 1f - i * 0.25f, daChet = i == 2
            };

        byte[] goiQuai = GoiTin.VietQuai(1234, dsGoc, 0, 3);
        var dsDoc = new GoiTin.MotQuai[GoiTin.SoQuaiMoiGoi];
        int moc;
        int soDoc = GoiTin.DocQuai(goiQuai, dsDoc, out moc);

        float lechXa = 0f; bool khopHet = soDoc == 3 && moc == 1234;
        for (int i = 0; i < soDoc && i < 3; i++)
        {
            lechXa = Mathf.Max(lechXa, Vector3.Distance(dsGoc[i].viTri, dsDoc[i].viTri));
            if (dsDoc[i].id != dsGoc[i].id || dsDoc[i].loai != dsGoc[i].loai
                || dsDoc[i].daChet != dsGoc[i].daChet
                || Mathf.Abs(dsDoc[i].mau01 - dsGoc[i].mau01) > 0.01f) khopHet = false;
        }

        Ghi("3. goi 3 con = " + goiQuai.Length + " byte (13 byte/con), doc lai khop: "
            + khopHet + ", lech vi tri xa nhat: " + lechXa.ToString("F4") + " m");
        if (!khopHet || lechXa > 0.02f)
        { Ghi("[LOI] goi quai doc ra khong khop luc viet"); loi++; }

        // ---- 4. Ca dan 33 con ton bao nhieu bang thong ----
        int soCon = Mathf.Max(dsQuai.Length, 1);
        int soGoi = Mathf.CeilToInt(soCon / (float)GoiTin.SoQuaiMoiGoi);
        int byteMotLuot = soCon * 13 + soGoi * 6;
        Ghi("4. ca dan " + soCon + " con = " + byteMotLuot + " byte moi luot, "
            + DongBoQuai.NhipGuiQuai + " luot/giay -> "
            + (byteMotLuot * DongBoQuai.NhipGuiQuai / 1024f).ToString("F1") + " KB/giay");

        // ================================================================
        //  PHAN 2: BU TRE
        // ================================================================

        // ---- 5. Lich su vi tri: hoi lui thi phai ra cho cu ----
        var ls = toi.gameObject.AddComponent<LichSuViTri>();
        var cc = toi.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 dau = toi.transform.position;
        yield return new WaitForSeconds(0.35f);

        // Di mot doan roi hoi: "0,3 giay truoc toi o dau".
        //
        // LAN SUA THU BA cua chieu nay, va lan nay sua dung cho. Hai lan truoc
        // toi chinh NGUONG (met -> giay, hai khung -> ba khung) trong khi sai
        // lech do duoc gan nhu khong doi: 62, 41, 60 ms, bat ke khung hinh
        // nhanh hay cham. Mot sai so khong co gian theo khung hinh thi khong
        // phai sai so cua nhip ghi - no la sai so cua CHINH PHEP DO: toi so
        // lich su voi "cho xuat phat", nhung luc hoi thi dong ho da troi them
        // mot, hai khung sau khi phep thu dung di chuyen, nen "0,4 giay truoc"
        // khong con la luc xuat phat nua.
        //
        // Gio phep thu TU GHI nhat ky (thoi diem, vi tri) cua chinh no, roi
        // doi chieu LichSuViTri voi nhat ky ay tai DUNG thoi diem duoc hoi.
        // Khong con cho nao de sai so cua phep do lan vao ket qua.
        Vector3 xa = dau + new Vector3(6f, 0f, 0f);
        var nhatKyLuc = new System.Collections.Generic.List<float>();
        var nhatKyCho = new System.Collections.Generic.List<Vector3>();
        float batDauDi = Time.unscaledTime;
        while (Time.unscaledTime - batDauDi < 0.6f)
        {
            float t = (Time.unscaledTime - batDauDi) / 0.6f;
            toi.transform.position = Vector3.Lerp(dau, xa, t);
            nhatKyLuc.Add(Time.unscaledTime);
            nhatKyCho.Add(toi.transform.position);
            yield return null;
        }

        // Hoi lui dung luc con dang di (0,3 giay truoc = giua quang duong)
        const float HoiLui = 0.3f;
        float lucHoi = Time.unscaledTime - HoiLui;
        Vector3 hoiLui = ls.ViTriLuc(HoiLui);

        // Vi tri THAT tai luc ay, noi suy tu nhat ky cua chinh phep thu
        Vector3 thatSu = nhatKyCho[0];
        for (int k = 1; k < nhatKyLuc.Count; k++)
        {
            if (nhatKyLuc[k] < lucHoi) continue;
            float a = nhatKyLuc[k - 1], b = nhatKyLuc[k];
            float u = b > a ? Mathf.Clamp01((lucHoi - a) / (b - a)) : 0f;
            thatSu = Vector3.Lerp(nhatKyCho[k - 1], nhatKyCho[k], u);
            break;
        }

        float lechLui = Vector3.Distance(hoiLui, thatSu);
        float daDi = Vector3.Distance(xa, dau);
        float tocDo = daDi / 0.6f;
        float saiGiay = tocDo > 0.01f ? lechLui / tocDo : 0f;

        // Lich su ghi o LateUpdate, nhat ky cua phep thu ghi truoc do trong cung
        // khung - cung mot moc thoi gian. Sai lech con lai chi la nhip ghi
        // (1/60 giay) cong mot khung. Hai khung la du.
        float haiKhung = 2f * Mathf.Max(Time.unscaledDeltaTime, 1f / LichSuViTri.NhipGhi);

        Ghi("5. di " + daDi.ToString("F2") + " m trong 0,6 giay ("
            + tocDo.ToString("F1") + " m/giay), hoi lai cho " + HoiLui.ToString("F1")
            + " giay truoc, doi chieu nhat ky rieng cua phep thu -> lech "
            + lechLui.ToString("F2") + " m = " + (saiGiay * 1000f).ToString("F0")
            + " ms (tran: hai khung hinh = " + (haiKhung * 1000f).ToString("F0") + " ms)");
        if (saiGiay > haiKhung)
        { Ghi("[LOI] lich su vi tri khong nho dung - bu tre se lui sai cho"); loi++; }

        // ---- 6. Bu tre that su lui nguoi, va tra lai ----
        Vector3 truocKhiLui = toi.transform.position;
        BuTre.Mo(0.4f, null);
        Vector3 trongLuc = toi.transform.position;
        BuTre.Dong();
        Vector3 sauKhiTra = toi.transform.position;

        float daLui = Vector3.Distance(truocKhiLui, trongLuc);
        float saiKhiTra = Vector3.Distance(truocKhiLui, sauKhiTra);
        Ghi("6. mo cua so bu tre -> nguoi bi lui " + daLui.ToString("F2")
            + " m; dong lai -> sai lech so voi cho cu: " + saiKhiTra.ToString("F4")
            + " m (phai lui > 0 va tra ve dung 0)");
        if (daLui < 0.5f) { Ghi("[LOI] khong lui duoc - bu tre khong co tac dung"); loi++; }
        if (saiKhiTra > 0.01f) { Ghi("[LOI] khong tra ve cho cu - nhan vat se nhay"); loi++; }

        // ---- 7. Bo qua nguoi tung: ho khong duoc lui ----
        // Lui ca nguoi tung la lam sai chinh cai minh dang chua.
        BuTre.Mo(0.4f, toi.GetComponent<Damageable>());
        float luiDuNoiBoQua = Vector3.Distance(truocKhiLui, toi.transform.position);
        BuTre.Dong();
        Ghi("7. bu tre nhung BO QUA chinh nguoi nay -> ho bi lui "
            + luiDuNoiBoQua.ToString("F4") + " m (phai la 0)");
        if (luiDuNoiBoQua > 0.01f)
        { Ghi("[LOI] lui ca nguoi tung phep"); loi++; }

        // ---- 8. Khong bao gio lui qua nguong ----
        // Mang te den may thi tha chiu truot, chu bu nua giay la sinh ra canh
        // vo ly khong ai giai thich noi.
        BuTre.Mo(5f, null);
        BuTre.Dong();
        Ghi("8. bao lui 5 giay -> thuc te lui " + BuTre.LuiLanCuoiGiay.ToString("F2")
            + " giay (tran la " + BuTre.LuiToiDaGiay.ToString("F2") + ")");
        if (BuTre.LuiLanCuoiGiay > BuTre.LuiToiDaGiay + 0.001f)
        { Ghi("[LOI] lui qua tran - se co canh nap sau tuong roi van an don"); loi++; }

        // ---- 9. Goi ky nang phai cho duoc do tre ----
        var phepGoc = new GoiTin.MotPhep
        {
            chiSo = 1, kyNang = 0, soThuTu = 5,
            diemNgam = Vector3.zero, doTreMs = 137
        };
        GoiTin.MotPhep phepDoc;
        byte[] bp = GoiTin.VietKyNang(phepGoc);
        bool docPhep = GoiTin.DocKyNang(bp, out phepDoc);
        Ghi("9. goi ky nang " + bp.Length + " byte, do tre viet 137 ms -> doc lai "
            + phepDoc.doTreMs + " ms");
        if (!docPhep || phepDoc.doTreMs != 137)
        { Ghi("[LOI] do tre khong di qua duoc goi tin - ben kia se khong biet lui bao nhieu"); loi++; }

        // ---- 10. Goi nhip: hoi va dap ----
        byte[] hoi = GoiTin.VietNhip(true, 4242);
        bool laHoi; int mocNhip;
        bool docNhip = GoiTin.DocNhip(hoi, out laHoi, out mocNhip);
        byte[] dap = GoiTin.VietNhip(false, mocNhip);
        bool laHoi2; int mocNhip2;
        bool docNhip2 = GoiTin.DocNhip(dap, out laHoi2, out mocNhip2);

        Ghi("10. goi nhip " + hoi.Length + " byte: hoi(4242) -> doc laHoi=" + laHoi
            + " moc=" + mocNhip + "; dap -> laHoi=" + laHoi2 + " moc=" + mocNhip2);
        if (!docNhip || !laHoi || mocNhip != 4242 || !docNhip2 || laHoi2 || mocNhip2 != 4242)
        { Ghi("[LOI] goi nhip hong - khong do duoc vong di-ve"); loi++; }

        // ---- 11. PHEP DO CHINH: bu tre bien TRUOT thanh TRUNG ----
        //
        // Dung lai dung canh nguoi choi bao: nguoi ban ngam vao cho ho THAY
        // doi phuong dang dung, nhung luc don toi noi thi doi phuong da chay
        // sang cho khac. Do hai lan cung mot cu no: khong bu -> phai truot,
        // co bu -> phai trung. Thieu nua duoi thi khong phan biet duoc "bu
        // dung" voi "don nay von trung san".
        TranHienTai.DangChoiMang = true;
        if (cc != null) cc.enabled = true;

        var mauToi = toi.GetComponent<Damageable>();
        Vector3 choCu = toi.transform.position;

        // Chay ngang 3 met trong 0,25 giay - dung nhu ne mot don.
        //
        // Nho MOC THOI GIAN luc con dung o cho cu: luc do phai lui dung bang
        // khoang da troi ke tu moc ay. Lan dau toi viet cung "0,25 giay" cho
        // ca hai, roi lai cho them 0,2 giay giua hai phep - the la lui 0,25
        // giay chi ve duoc den luc da dung o cho MOI, va phep thu bao "bu tre
        // khong an" trong khi no khong he duoc thu.
        Vector3 choMoi = choCu + new Vector3(0f, 0f, 3f);
        float lucODauCu = Time.unscaledTime;
        float batDauNe = Time.unscaledTime;
        while (Time.unscaledTime - batDauNe < 0.25f)
        {
            float t = (Time.unscaledTime - batDauNe) / 0.25f;
            if (cc != null) cc.enabled = false;
            toi.transform.position = Vector3.Lerp(choCu, choMoi, t);
            if (cc != null) cc.enabled = true;
            yield return null;
        }
        if (cc != null) cc.enabled = false;
        toi.transform.position = choMoi;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
        yield return null;

        int mask = LayerMask.GetMask("Enemy", "Player");

        // AreaDamage tru mau ngay trong loi goi, khong doi khung sau - nen do
        // duoc lien tiep, va PHAI do lien tiep de hai phep cung mot moc thoi gian.
        float luiBaoNhieu = Time.unscaledTime - lucODauCu;

        // 11a. KHONG bu tre: no vao cho cu -> phai truot
        float truoc11a = mauToi.health;
        CombatUtil.AreaDamage(choCu + Vector3.up * 0.5f, 1.6f, 40f, mask,
                              DamageType.Fire, 0f, null);
        float mat11a = truoc11a - mauToi.health;

        // 11b. CO bu tre: cung cho ay, cung ban kinh -> phai trung
        float truoc11b = mauToi.health;
        BuTre.Mo(luiBaoNhieu, null);
        CombatUtil.AreaDamage(choCu + Vector3.up * 0.5f, 1.6f, 40f, mask,
                              DamageType.Fire, 0f, null);
        BuTre.Dong();
        float mat11b = truoc11b - mauToi.health;

        Ghi("11. chay ngang 3,00 m roi no vao CHO CU (lui "
            + (luiBaoNhieu * 1000f).ToString("F0") + " ms):");
        Ghi("    a) khong bu tre -> mat " + mat11a.ToString("F0") + " mau (phai la 0 - don truot)");
        Ghi("    b) CO bu tre    -> mat " + mat11b.ToString("F0") + " mau (phai lon hon 0 - don trung)");
        if (mat11a > 0f)
        { Ghi("[LOI] khong bu ma van trung - phep do nay khong chung minh duoc gi"); loi++; }
        if (mat11b <= 0f)
        { Ghi("[LOI] bu tre khong bien duoc don truot thanh don trung"); loi++; }

        // ---- 12. May khach dung lai dan quai tu goi nghe duoc ----
        TranHienTai.LaHost = false;      // dong vai may khach

        var goKhach = new GameObject("TAM_QuaiKhach");
        var boQuai = goKhach.AddComponent<DongBoQuai>();

        var dsGui = new GoiTin.MotQuai[2];
        Vector3 choQuai = toi.transform.position + new Vector3(8f, 0f, 0f);
        choQuai.y = VfxFactory.GroundY(choQuai) + 0.15f;
        for (int i = 0; i < 2; i++)
            dsGui[i] = new GoiTin.MotQuai
            {
                id = (ushort)(9000 + i), loai = (byte)MonsterType.Skeleton,
                viTri = choQuai + new Vector3(0f, 0f, 2f * i), gocY = 0f,
                mau01 = 0.5f, daChet = false
            };

        int quaiTruoc = Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None).Length;
        boQuai.NhanGoiQuai(GoiTin.VietQuai(500, dsGui, 0, 2));
        yield return new WaitForSeconds(0.5f);

        int quaiSau = Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None).Length;

        // Tim lai hai con vua sinh de xem AI da tat va mau da ap dung chua
        bool aiDaTat = true; float mauCon = -1f;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            if (n.id < 9000) continue;
            foreach (var ai in n.GetComponentsInChildren<EnemyAI>(true))
                if (ai.enabled) aiDaTat = false;
            var d = n.GetComponent<Damageable>();
            if (d != null && d.maxHealth > 0f) mauCon = d.health / d.maxHealth;
        }

        Ghi("12. may khach nghe goi 2 con -> quai trong canh " + quaiTruoc + " -> " + quaiSau
            + " (giu " + boQuai.SoQuaiDangGiu + " con), AI cua chung da tat: " + aiDaTat
            + ", mau ap vao: " + (mauCon * 100f).ToString("F0") + "% (bao 50%)");
        if (quaiSau != quaiTruoc + 2)
        { Ghi("[LOI] khach khong dung lai duoc dan quai tu goi"); loi++; }
        if (!aiDaTat)
        { Ghi("[LOI] AI van chay - con quai ben nay se troi khoi con that"); loi++; }
        if (Mathf.Abs(mauCon - 0.5f) > 0.03f)
        { Ghi("[LOI] mau cua quai khong lay tu chu phong"); loi++; }

        // Don hai con vua sinh
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
            if (n.id >= 9000) Object.DestroyImmediate(n.gameObject);
        Object.DestroyImmediate(goKhach);

        // ---- Don ----
        if (cc != null) cc.enabled = true;
        Object.DestroyImmediate(ls);
        TranHienTai.Xoa();

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
        File.WriteAllText("PlayTestShots/quai_butre.txt", bao.ToString());

        var rac = GameObject.Find("TAM_QuaiButre");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.update += TraLaiCanh;
    }
}
