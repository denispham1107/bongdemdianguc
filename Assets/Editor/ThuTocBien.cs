using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 76): KY NANG "TOC BIEN" (so hieu 15, them 18/09/2026, nhom HO TRO).
///
/// Nguoi dung: dich chuyen tuc thoi toi cho khac trong 15 m, co hieu ung BIEN MAT o cho cu va HIEN RA
/// o cho moi; hoi chieu 5 giay, MOI CAP GIAM 0,25 giay. Hoi them, nguoi dung chot: 40 nang luong;
/// ngam vao cho khong dung duoc thi LUI VE diem trong gan nhat (di xuyen tuong duoc); KHONG NIEM CHU.
///
///   A. Thong so: so hieu 15, 16 ky nang; 40 nang luong, hoi chieu 5 s o cap 1, tam 15 m; ten co dau;
///      HUD 16 icon; icon file; Sach phep xep vao nhom HO TRO.
///   B. Nhay that: khoa -> tu choi; mo khoa -> nhan vat DOI CHO dung huong ngam, tru 40 nang luong,
///      vao hoi chieu; co ca hai hieu ung tren canh.
///   C. Kep tam 15 m: ngam ra 30 m -> chi nhay toi da 15 m.
///   D. Ngam vao cho KHONG DUNG DUOC (trong long mot khoi da) -> van nhay nhung lui ve diem trong gan do.
///   E. Hoi chieu giam theo cap: cap 1..5 = 5 / 4,75 / 4,5 / 4,25 / 4 giay (do tren nhan vat that).
///   F. Khong niem chu: tu luc bam den luc doi cho khong qua 0,2 giay.
///   G. CAP 5 (nguoi dung 18/09/2026): dang choang / nga / dong bang / hat tung van bam duoc va nhay xong
///      thi SACH het trang thai bat loi; DOI CHUNG cap 4 thi bi chan. Dang bi LOC XOAY cuon thi KHONG nhay duoc.
///
/// Ket qua: PlayTestShots/tocbien.txt, anh tocbien_*.png.
/// </summary>
public static class ThuTocBien
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyTocBien;

    [MenuItem("Diablo 2.5D/76. Chay thu TOC BIEN (ky nang moi)", false, 165)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu Toc bien", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Toc bien");
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
        if (GameObject.Find("TAM_TocBien") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_TocBien");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TocBien] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            if (!pc.tuDocInput) continue;
            return pc;
        }
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>Huong quang dai nhat quanh nhan vat - can 20 m trong de do tam nhay.</summary>
    static Vector3 HuongTrong(PlayerController pc, float can)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 36; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 10f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 0.8f, h, out RaycastHit _, can, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static float XaNgang(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han0 = Time.time + 30f;
        while (dir == null && Time.time < han0) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e6f; mauToi.health = 1e6f;

        // ================= A. THONG SO =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/TocBien");

        // Toc bien phai nam trong nhom HO TRO cua Sach phep
        int nhomCua = -1;
        for (int g = 0; g < SachPhep.SoNhom; g++)
            foreach (int k in SachPhep.KyNangTheoNhom[g])
                if (k == K) nhomCua = g;

        Ghi(string.Format("A. so hieu {0}, so ky nang {1}; nang luong {2}, hoi chieu {3} s (cap 1), niem {4} s; Sach phep doc {5}/{6}/{7}; tam {8} m",
            K, CapDo.SoKyNang, toi.tocBienCost, toi.tocBienCooldown, toi.tocBienCastTime, nl, hc, nc, TocBien.Tam));
        Ghi(string.Format("A. ten \"{0}\", tom tat \"{1}\", mo ta {2} ky tu; HUD {3} icon, icon so 15 {4}; icon file {5}; nhom trong Sach phep: {6}",
            SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length, bo != null ? bo.Length : -1,
            bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG",
            tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG",
            nhomCua >= 0 ? SachPhep.TenNhom[nhomCua] : "KHONG CO"));
        Kiem(K == 15 && CapDo.SoKyNang == 16, "so hieu / so ky nang sai");
        Kiem(Mathf.Approximately(toi.tocBienCost, 40f) && Mathf.Approximately(nl, 40f), "nang luong khong phai 40");
        Kiem(Mathf.Approximately(TocBien.Tam, 15f), "tam khong phai 15 m");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Toc bien");
        Kiem(SachPhep.Ten(K) == "TỐC BIẾN" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Toc bien");
        Kiem(nhomCua >= 0 && SachPhep.TenNhom[nhomCua] == "HỖ TRỢ", "Toc bien khong nam trong nhom HO TRO");

        // ================= E. HOI CHIEU GIAM THEO CAP =================
        Ghi("");
        {
            var sb = new StringBuilder();
            bool dung = true;
            for (int c = 1; c <= 5; c++)
            {
                float hcC = TocBien.HoiChieuTheoCap(c);
                float mong = 5f - 0.25f * (c - 1);
                sb.Append("cap ").Append(c).Append("=").Append(hcC.ToString("F2")).Append("  ");
                if (Mathf.Abs(hcC - mong) > 0.001f) dung = false;
            }
            Ghi("E. hoi chieu theo cap: " + sb.ToString().Trim() + "  (mong 5 / 4,75 / 4,5 / 4,25 / 4)");
            Kiem(dung, "hoi chieu khong giam dung 0,25 giay moi cap");
        }

        // ================= B + F. NHAY THAT =================
        Ghi("");
        CapDo.BatDauTranMoi();
        Vector3 huong = HuongTrong(toi, 20f);
        toi.transform.rotation = Quaternion.LookRotation(huong);

        int soPhep = 0;
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; };
        toi.DaTungPhep += dem;

        int s0 = soPhep;
        toi.CastAt(K, toi.transform.position + huong * 10f);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoKhoa(K);
        yield return new WaitForSeconds(0.3f);

        {
            Vector3 choCu = toi.transform.position;
            Vector3 ngam = choCu + huong * 10f;
            toi.mana = toi.maxMana;
            float mana0 = toi.mana;
            int nhay0 = TocBien.SoLanNhay;

            float batDau = Time.time;
            toi.CastAt(K, ngam);
            float manaTon = mana0 - toi.mana;
            float hoiNgaySau = toi.HoiChieuGiay(K);

            // Cho den khi that su doi cho (khong niem chu -> phai xong gan nhu ngay)
            float han = Time.time + 1f;
            while (TocBien.SoLanNhay == nhay0 && Time.time < han) yield return null;
            float mat = Time.time - batDau;

            yield return null;
            Vector3 choMoi = toi.transform.position;
            float diDuoc = XaNgang(choCu, choMoi);
            float lechNgam = XaNgang(choMoi, ngam);
            bool coBienMat = GameObject.Find("TocBienBienMat") != null;
            bool coHienRa = GameObject.Find("TocBienHienRa") != null;
            yield return Chup("tocbien_1_vua_nhay");

            Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> nhay {1} lan, di duoc {2:F2} m (ngam 10 m, lech {3:F2} m), ton {4:F0} nang luong, hoi chieu ngay sau {5:F2} s",
                tuChoiKhoa, TocBien.SoLanNhay - nhay0, diDuoc, lechNgam, manaTon, hoiNgaySau));
            Ghi(string.Format("B. hieu ung tren canh: bien mat {0}, hien ra {1}", coBienMat, coHienRa));
            Ghi(string.Format("F. tu luc bam den luc doi cho: {0:F3} giay (khong niem chu)", mat));
            Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
            Kiem(diDuoc > 8f && lechNgam < 1.5f, "khong nhay toi dung cho ngam");
            Kiem(Mathf.Abs(manaTon - 40f) < 0.01f, "khong ton dung 40 nang luong");
            Kiem(hoiNgaySau > 4.5f, "hoi chieu cap 1 khong phai 5 giay");
            Kiem(coBienMat && coHienRa, "thieu hieu ung bien mat / hien ra");
            Kiem(mat < 0.2f, "bam xong khong doi cho ngay (khong duoc niem chu)");
        }

        // ================= C. KEP TAM 15 M =================
        Ghi("");
        {
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            Vector3 choCu = toi.transform.position;
            huong = HuongTrong(toi, 20f);
            toi.mana = toi.maxMana;
            int nhay0 = TocBien.SoLanNhay;
            toi.CastAt(K, choCu + huong * 30f);          // ngam ra 30 m
            float han = Time.time + 1f;
            while (TocBien.SoLanNhay == nhay0 && Time.time < han) yield return null;
            yield return null;
            float diDuoc = XaNgang(choCu, toi.transform.position);
            Ghi(string.Format("C. ngam ra 30 m -> nhay duoc {0:F2} m (tam toi da {1} m)", diDuoc, TocBien.Tam));
            Kiem(diDuoc <= TocBien.Tam + 0.5f && diDuoc > TocBien.Tam - 3f, "khong kep dung tam 15 m");
        }

        // ================= D. NGAM VAO CHO KHONG DUNG DUOC =================
        Ghi("");
        {
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            huong = HuongTrong(toi, 20f);
            Vector3 choCu = toi.transform.position;

            // Dung mot KHOI DA dac ngay tren duong nhay, cach 8 m
            Vector3 choDa = choCu + huong * 8f;
            choDa.y = VfxFactory.GroundY(choDa);
            var da = GameObject.CreatePrimitive(PrimitiveType.Cube);
            da.name = "TAM_KhoiDa";
            da.transform.position = choDa + Vector3.up * 1.2f;
            da.transform.localScale = new Vector3(3f, 2.4f, 3f);
            da.layer = 0;                                  // lop Default - nam trong obstacleMask
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            bool dungDuocTrongDa = TocBien.DungDuoc(choDa, toi.MatNaVatCan);
            toi.mana = toi.maxMana;
            int nhay0 = TocBien.SoLanNhay;
            toi.CastAt(K, choDa);                          // ngam thang vao giua khoi da
            float han = Time.time + 1f;
            while (TocBien.SoLanNhay == nhay0 && Time.time < han) yield return null;
            yield return null;

            Vector3 choMoi = toi.transform.position;
            float diDuoc = XaNgang(choCu, choMoi);
            float cachDa = XaNgang(choMoi, choDa);
            bool ketThucDungDuoc = TocBien.DungDuoc(choMoi, toi.MatNaVatCan);
            Object.Destroy(da);

            Ghi(string.Format("D. ngam vao giua khoi da (cho do dung duoc: {0}) -> van nhay {1} lan, di {2:F2} m, dung cach khoi da {3:F2} m, cho dung cuoi cung dung duoc {4}",
                dungDuocTrongDa, TocBien.SoLanNhay - nhay0, diDuoc, cachDa, ketThucDungDuoc));
            Kiem(!dungDuocTrongDa, "doi chung hong: giua khoi da van 'dung duoc'");
            Kiem(TocBien.SoLanNhay > nhay0, "ngam vao vat can thi khong nhay duoc gi ca");
            Kiem(ketThucDungDuoc && cachDa > 1f, "nhay xong dung ngay trong vat can");
        }

        // ================= G. CAP 5: GO TROI MOI TRANG THAI =================
        Ghi("");
        {
            // --- DOI CHUNG: cap 4 thi bi chan ---
            for (int i = 0; i < 8 && CapDo.CapCuaKyNang(K) < 4; i++)
            {
                CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
                CapDo.NangCap(K);
            }
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            StunnedEffect.Apply(mauToi, 3f);
            yield return null;
            Vector3 truoc4 = toi.transform.position;
            toi.mana = toi.maxMana;
            int nhay4 = TocBien.SoLanNhay;
            huong = HuongTrong(toi, 20f);
            toi.CastAt(K, toi.transform.position + huong * 8f);
            yield return new WaitForSeconds(0.2f);
            bool cap4BiChan = TocBien.SoLanNhay == nhay4;
            int capLuc4 = CapDo.CapCuaKyNang(K);
            var stConLai = mauToi.GetComponent<StunnedEffect>();
            bool con4 = stConLai != null && stConLai.IsStunned;

            // --- CAP 5: bam duoc va sach het ---
            for (int i = 0; i < 8 && CapDo.CapCuaKyNang(K) < 5; i++)
            {
                CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
                CapDo.NangCap(K);
            }
            while (toi.HoiChieuGiay(K) > 0f) yield return null;

            // Ap DU BON trang thai nguoi dung neu dich danh, cung mot luc.
            // ⚠️ KHONG dinh them BurningEffect o day: BurningEffect.Start goi frozen.Thaw() ("lua thieu
            // lam tan bang"), nen lan chay dau chi con 4 trang thai va cai DONG BANG bien mat truoc khi
            // do - phep thu se xanh ma chua he kiem duoc thu nguoi dung noi toi. Chay do rieng ben duoi.
            FrozenEffect.Apply(mauToi, 3f);
            StunnedEffect.Apply(mauToi, 3f);
            BiDanhNga.Apply(mauToi, 3f);
            BiHatTung.Apply(mauToi, 1.5f);
            yield return null;
            int dinhTruoc = 0;
            var frTruoc = mauToi.GetComponent<FrozenEffect>();
            if (frTruoc != null && frTruoc.IsFullyFrozen) dinhTruoc++;
            if (mauToi.GetComponent<StunnedEffect>() != null) dinhTruoc++;
            if (mauToi.GetComponent<BiDanhNga>() != null) dinhTruoc++;
            if (mauToi.GetComponent<BiHatTung>() != null) dinhTruoc++;

            Vector3 truoc5 = toi.transform.position;
            toi.mana = toi.maxMana;
            int nhay5 = TocBien.SoLanNhay;
            huong = HuongTrong(toi, 20f);
            toi.CastAt(K, toi.transform.position + huong * 8f);
            float han5 = Time.time + 1f;
            while (TocBien.SoLanNhay == nhay5 && Time.time < han5) yield return null;
            yield return null;

            bool cap5NhayDuoc = TocBien.SoLanNhay > nhay5;
            float di5 = XaNgang(truoc5, toi.transform.position);
            int conLai = 0;
            var fr = mauToi.GetComponent<FrozenEffect>();
            if (fr != null && (fr.IsFullyFrozen || fr.remaining > 0f)) conLai++;
            var st2 = mauToi.GetComponent<StunnedEffect>();
            if (st2 != null && st2.IsStunned) conLai++;
            var ng2 = mauToi.GetComponent<BiDanhNga>();
            if (ng2 != null && ng2.DangNga) conLai++;
            var ht2 = mauToi.GetComponent<BiHatTung>();
            if (ht2 != null && ht2.DangBay) conLai++;

            Ghi(string.Format("G. DOI CHUNG cap {0}: dang choang ma bam -> bi chan {1} (van con choang {2})",
                capLuc4, cap4BiChan, con4));
            Ghi(string.Format("G. CAP 5: dinh {0} trang thai bat loi -> bam duoc {1}, nhay {2:F2} m, con lai {3} trang thai",
                dinhTruoc, cap5NhayDuoc, di5, conLai));
            Kiem(capLuc4 == 4 && cap4BiChan, "doi chung hong: cap 4 ma van bam duoc khi dang choang");
            Kiem(CapDo.CapCuaKyNang(K) == 5 && cap5NhayDuoc && di5 > 5f, "cap 5 khong bam duoc khi dang bi khoa cung");
            Kiem(dinhTruoc == 4 && conLai == 0, "cap 5 nhay xong ma trang thai bat loi khong bi xoa sach");

            // --- Chay (BurningEffect) do RIENG: no tu lam tan bang nen khong do chung duoc ---
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            BurningEffect.Apply(mauToi, 5f, 6f, null);
            yield return null;
            bool coChay = mauToi.GetComponent<BurningEffect>() != null;
            toi.mana = toi.maxMana;
            int nhayChay = TocBien.SoLanNhay;
            huong = HuongTrong(toi, 20f);
            toi.CastAt(K, toi.transform.position + huong * 8f);
            float hanChay = Time.time + 1f;
            while (TocBien.SoLanNhay == nhayChay && Time.time < hanChay) yield return null;
            yield return null;
            bool conChay = mauToi.GetComponent<BurningEffect>() != null;
            Ghi(string.Format("G. dang CHAY ({0}) -> nhay xong con chay {1}", coChay, conChay));
            Kiem(coChay && !conChay, "cap 5 nhay xong ma con dang chay");

            // --- NGOAI LE: dang bi LOC XOAY cuon thi khong nhay duoc ---
            while (toi.HoiChieuGiay(K) > 0f) yield return null;
            Vector3 choLoc = toi.transform.position + huong * 2f;
            choLoc.y = VfxFactory.GroundY(choLoc);
            var loc = Tornado.Spawn(choLoc, huong, LayerMask.GetMask("Player"));
            loc.boQua = null;
            float hanCuon = Time.time + 3f;
            while (mauToi.GetComponent<WhirledEffect>() == null && Time.time < hanCuon) yield return null;
            bool biCuon = mauToi.GetComponent<WhirledEffect>() != null;

            int nhayLoc = TocBien.SoLanNhay;
            toi.mana = toi.maxMana;
            toi.CastAt(K, toi.transform.position + huong * 8f);
            yield return new WaitForSeconds(0.2f);
            bool locChan = TocBien.SoLanNhay == nhayLoc;
            Ghi(string.Format("G. dang bi Loc xoay cuon ({0}) -> bam Toc bien bi chan {1} (ngoai le duy nhat)", biCuon, locChan));
            Kiem(biCuon, "doi chung hong: khong bi Loc xoay cuon nen khong do duoc gi");
            Kiem(locChan, "dang bi Loc xoay cuon ma van toc bien duoc");
            if (loc != null) Object.Destroy(loc.gameObject);
            var w = mauToi.GetComponent<WhirledEffect>(); if (w != null) w.Release();
            yield return new WaitForSeconds(0.5f);
        }

        foreach (var go in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            if (go != null && go.name.StartsWith("TAM_KhoiDa")) Object.Destroy(go.gameObject);
        foreach (var t0 in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude))
            if (t0 != null) Object.Destroy(t0.gameObject);
        toi.DaTungPhep -= dem;
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
        Debug.Log("[TocBien] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/tocbien.txt", bao.ToString());
        var rac = GameObject.Find("TAM_TocBien");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
