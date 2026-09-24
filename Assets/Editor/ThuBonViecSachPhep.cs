using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 80): BON VIEC TRONG SACH PHEP (nguoi dung 25/09/2026).
///
///   1. Binh mau / Binh mana CO SAN cap 1, nang toi cap 3 (+75 mau / +45 mana moi cap) - menu 65 muc C do chi tiet;
///      o day chi kiem Sach phep (goc hang, nut nang cap) va chup anh.
///   2. Qua cau lua, Qua cau bang bay XUYEN vat nho (bia mo, da); nha, cay, LO LUA van chan - Lua dia nguc cung
///      va lo (nguoi dung chon).
///   3. Qua cau bang, Mua bang trung LO LUA thi dap tat nhu Gio loc (30 giay sau chay lai).
///   4. Bi dong moi "Toc do di chuyen": mo khoa +10% toc do GOC, moi cap +2,5% (cap 5 = +20%).
///
/// Do bang SO:
///   A. Toc do: A1 co trong nhom BI DONG, icon co hinh, khong tung duoc; A2 bom input di thang 1,2 giay tren CUNG
///      mot doan duong o cap 0 / cap 1 / cap 5 -> ti le toc do 1,00 / 1,10 / 1,20 (doi chung: cap 0).
///   B. Xuyen: B1 duong tung THAT (CastAt) bat co xuyen cho ca hai qua cau (doi chung: Fireball.Spawn tran - qua
///      cua quai - khong bat); B2 ban qua that vao bia / cay / LO thu tren troi, ca Qua cau lua lan Qua cau bang
///      (doi chung: qua khong bat xuyen no ngay o bia); B3 moi collider cua 10 lo that deu KHONG la vat nho.
///   C. Dap lo: C1 Qua cau bang no canh lo that -> tat (doi chung: no cach lo khac 6 m -> van chay);
///      C2 Mua bang: tang bang roi canh lo -> tat (doi chung: roi cach 5 m -> van chay); C3 30 giay sau lo chay lai.
///   D. Anh Sach phep: binh mau (cap 1/3, nut nang cap), Toc do (nhom BI DONG).
///
/// Ket qua: PlayTestShots/bonviec_sachphep.txt, anh bonviec_*.png.
/// </summary>
public static class ThuBonViecSachPhep
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/80. Chay thu BON VIEC SACH PHEP (binh, xuyen bia, dap lo, toc do)", false, 169)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] binh cap 1..3, qua cau xuyen bia, dap lo bang bang, bi dong toc do");
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
        if (GameObject.Find("TAM_BonViec") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_BonViec");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BonViec] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static readonly List<GameObject> rac = new List<GameObject>();

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

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

    static void DatCho(PlayerController pc, Vector3 p)
    {
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    // ---- ban mot qua that vao vat thu, do qua di xa bao nhieu va con song khong ----
    static float xaNhat; static bool conSong;

    static IEnumerator BanQua(bool bang, Vector3 tu, Vector3 h, int lop, bool xuyen, float giay)
    {
        GameObject go;
        if (bang)
        {
            var q = QuaCauBang.Spawn(tu, h, lop, 0);
            q.xuyenVatNho = xuyen;
            go = q.gameObject;
        }
        else
        {
            var q = Fireball.Spawn(tu, h, lop, 0);
            q.xuyenVatNho = xuyen;
            q.tocQueo = 0f;
            q.burnSeconds = 0f;
            go = q.gameObject;
        }
        xaNhat = 0f;
        float han = Time.time + giay;
        while (go != null && Time.time < han)
        {
            xaNhat = Vector3.Dot(go.transform.position - tu, h);
            yield return null;
        }
        conSong = go != null;
        if (go != null) Object.DestroyImmediate(go);
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);

        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        CapDo.BatDauTranMoi();

        // ================= A. BI DONG TOC DO =================
        Ghi("");
        int ky = CapDo.KyTocDo;
        bool trongNhom = System.Array.IndexOf(SachPhep.KyNangTheoNhom[SachPhep.KyNangTheoNhom.Length - 1], ky) >= 0;
        var icon = IconKyNang.BoDayDu();
        int diemSang = 0;
        if (ky < icon.Length && icon[ky] != null)
            foreach (var c in icon[ky].GetPixels()) if (c.a > 0.5f && c.g > 0.5f) diemSang++;
        int diemSangKhang = 0;
        foreach (var c in icon[CapDo.KyKhangPhong].GetPixels()) if (c.a > 0.5f && c.g > 0.5f) diemSangKhang++;
        Ghi(string.Format("A1. ky nang so {0} '{1}': nhom '{2}' co no {3}, la bi dong {4}; icon {5} diem sang (Khang Phong {6}); so ky nang co ten {7}/{8}",
            ky, SachPhep.Ten(ky), SachPhep.TenNhom[SachPhep.TenNhom.Length - 1], trongNhom, CapDo.LaKyBiDong(ky), diemSang,
            diemSangKhang, SachPhep.SoKyNangCoTen(), CapDo.SoKyNang));
        Kiem(ky == 20 && CapDo.SoKyNang == 21 && SachPhep.SoKyNangCoTen() == CapDo.SoKyNang, "so hieu / so ky nang sai");
        Kiem(trongNhom && CapDo.LaKyBiDong(ky), "Toc do khong nam trong nhom BI DONG");
        Kiem(diemSang > 2000, "icon Toc do trong / khong co hinh");

        CapDo.MoCaDuongChoPhepThu(ky);
        toi.mana = toi.maxMana;
        float mana0 = toi.mana;
        toi.CastAt(ky, toi.transform.position + toi.transform.forward * 5f);
        yield return null;
        Ghi(string.Format("A1b. bam ky nang bi dong: dang niem {0}, mana {1:F0} -> {2:F0} (phai khong doi gi)", toi.DangNiemChu, mana0, toi.mana));
        Kiem(!toi.DangNiemChu && Mathf.Abs(toi.mana - mana0) < 0.01f, "ky nang bi dong Toc do lai tung ra duoc");

        // A2. Do toc do THAT: bom input di thang, cung mot doan duong o ba muc cap
        Vector3 goc = toi.transform.position;
        Vector3 huong = Vector3.forward;
        for (int g = 0; g < 16; g++)
        {
            Vector3 h = Quaternion.AngleAxis(g * 22.5f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc + Vector3.up * 1.0f, 0.4f, h, out RaycastHit _, 9f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            { huong = h; break; }
        }
        var capDo = new[] { 0, 1, 5 };
        var tocDo = new float[3];
        bool tuDoc = toi.tuDocInput;
        toi.tuDocInput = false;
        for (int k = 0; k < 3; k++)
        {
            // dat cap ky nang Toc do: 0 = chua mo
            CapDo.BatDauTranMoi();
            if (capDo[k] > 0)
            {
                CapDo.MoCaDuongChoPhepThu(ky);
                for (int i = 1; i < capDo[k]; i++) { CapDo.ThemDiemChoPhepThu(1); CapDo.NangCap(ky); }
            }
            DatCho(toi, goc);
            yield return new WaitForSeconds(0.3f);
            var goi = GoiInput.Rong(Time.deltaTime);
            goi.huongDi = huong;
            float t0 = -1f; Vector3 p0 = Vector3.zero;
            float hanDi = Time.time + 1.6f;
            while (Time.time < hanDi)
            {
                goi.dt = Time.deltaTime;
                toi.input = goi;
                yield return null;
                // bo 0,3 giay dau (tang toc / roi xuong dat) roi moi bat dau dem
                if (t0 < 0f && Time.time > hanDi - 1.2f) { t0 = Time.time; p0 = toi.transform.position; }
            }
            Vector3 d = toi.transform.position - p0; d.y = 0f;
            tocDo[k] = d.magnitude / Mathf.Max(0.01f, Time.time - t0);
            toi.input = GoiInput.Rong(Time.deltaTime);
            yield return new WaitForSeconds(0.2f);
        }
        toi.tuDocInput = tuDoc;
        DatCho(toi, goc);
        float tl1 = tocDo[1] / Mathf.Max(0.01f, tocDo[0]), tl5 = tocDo[2] / Mathf.Max(0.01f, tocDo[0]);
        Ghi(string.Format("A2. di thang 1,2 giay cung doan duong: cap 0 {0:F3} m/s | cap 1 {1:F3} m/s (x{2:F3}, mong 1,100) | cap 5 {3:F3} m/s (x{4:F3}, mong 1,200); toc goc {5:F2}, moveSpeed {6:F2}",
            tocDo[0], tocDo[1], tl1, tocDo[2], tl5, toi.TocGoc, toi.moveSpeed));
        Kiem(tocDo[0] > 1f, "doi chung hong: cap 0 nhan vat khong di duoc - phep do vo nghia");
        Kiem(Mathf.Abs(tl1 - 1.10f) < 0.02f, "mo khoa Toc do khong tang dung 10%");
        Kiem(Mathf.Abs(tl5 - 1.20f) < 0.02f, "Toc do cap 5 khong tang dung 20%");
        Kiem(Mathf.Abs(CapDo.TocThemTheoCap(2) - 0.125f) < 1e-4f && Mathf.Abs(CapDo.TocThemTheoCap(3) - 0.15f) < 1e-4f,
             "moi cap khong them dung 2,5%");

        // ================= B. XUYEN VAT NHO =================
        Ghi("");
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(0);
        CapDo.MoCaDuongChoPhepThu(CapDo.KyQuaCauBang);
        yield return new WaitForSeconds(0.6f);
        // B1. duong tung that
        bool coLua = false, xuyenLua = true, coBang = false, xuyenBang = true;
        toi.mana = toi.maxMana;
        toi.CastAt(0, toi.transform.position + huong * 10f);
        float hB = Time.time + 1.5f;
        while (!coLua && Time.time < hB)
        {
            yield return null;
            foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude))
                if (f.boQua == mauToi) { coLua = true; if (!f.xuyenVatNho) xuyenLua = false; }
        }
        yield return new WaitForSeconds(0.8f);
        toi.mana = toi.maxMana;
        toi.CastAt(CapDo.KyQuaCauBang, toi.transform.position + huong * 10f);
        hB = Time.time + 1.5f;
        while (!coBang && Time.time < hB)
        {
            yield return null;
            foreach (var q in Object.FindObjectsByType<QuaCauBang>(FindObjectsInactive.Exclude))
                if (q.boQua == mauToi) { coBang = true; if (!q.xuyenVatNho) xuyenBang = false; }
        }
        var dcQua = Fireball.Spawn(Vector3.up * 900f, Vector3.forward, 0, 0);
        bool dcXuyen = dcQua.xuyenVatNho;
        Object.DestroyImmediate(dcQua.gameObject);
        Ghi(string.Format("B1. tung that: Qua cau lua co qua {0}, xuyen {1}; Qua cau bang co qua {2}, xuyen {3}; doi chung Fireball.Spawn tran (qua cua quai) xuyen {4}",
            coLua, xuyenLua, coBang, xuyenBang, dcXuyen));
        Kiem(coLua && xuyenLua, "Qua cau lua tung that khong bay xuyen vat nho");
        Kiem(coBang && xuyenBang, "Qua cau bang tung that khong bay xuyen vat nho");
        Kiem(!dcXuyen, "qua cau lua cua quai cung xuyen - chi ky nang nguoi choi duoc xuyen");
        yield return new WaitForSeconds(1.0f);

        // B2. ban qua that vao vat thu tren troi
        int lop = LayerMask.GetMask("Default");
        Vector3 cho = toi.transform.position + Vector3.up * 70f;
        Vector3 hK = Vector3.forward;
        Vector3 tu = cho - hK * 9f + Vector3.up * 1.2f;
        string[] tenVat = { "bia 1,4 x 3,0 x 1,0", "cay cao 8 m", "LO LUA 1,0 x 2,33 x 1,0" };
        for (int loai = 0; loai < 2; loai++)
        {
            bool bang = loai == 1;
            string tenQua = bang ? "Qua cau bang" : "Qua cau lua";
            for (int v = 0; v < 3; v++)
            {
                var vat = new GameObject("TAM_VatThu" + v);
                vat.transform.position = cho;
                if (v == 0) vat.AddComponent<BoxCollider>().size = new Vector3(1.4f, 3.0f, 1.0f);
                else if (v == 1) { var cc = vat.AddComponent<CapsuleCollider>(); cc.height = 8f; cc.radius = 0.6f; cc.center = Vector3.up * 3f; }
                else { var bc = vat.AddComponent<BoxCollider>(); bc.size = new Vector3(1.0f, 2.33f, 1.0f); bc.center = Vector3.up * 1.0f; vat.AddComponent<LoLuaDa>(); }
                rac.Add(vat);
                Physics.SyncTransforms();
                yield return BanQua(bang, tu, hK, lop, true, 1.3f);
                bool song = conSong; float xa = xaNhat;
                Ghi(string.Format("B2. {0} vao {1}: con song {2}, di xa {3:F1} m (vat cach 9 m)", tenQua, tenVat[v], song, xa));
                if (v == 0) Kiem(song && xa > 12f, tenQua + " KHONG xuyen qua bia");
                else Kiem(!song && xa > 7f && xa < 9.6f, tenQua + " khong no o " + tenVat[v]);
                // doi chung: qua khong bat xuyen thi no ngay o bia
                if (v == 0)
                {
                    yield return BanQua(bang, tu, hK, lop, false, 1.3f);
                    Ghi(string.Format("    doi chung (khong bat xuyen): con song {0}, di xa {1:F1} m", conSong, xaNhat));
                    Kiem(!conSong && xaNhat > 7f && xaNhat < 9.6f, "doi chung hong: qua khong bat xuyen van qua bia");
                }
                Object.DestroyImmediate(vat);
                yield return null;
            }
        }

        // B3. moi collider cua lo that khong la vat nho (Lua dia nguc cung va lo)
        var dsLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsSortMode.None);
        int colLo = 0, colLoNho = 0;
        foreach (var lo in dsLo)
            foreach (var c in lo.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger) continue;
                colLo++;
                if (Fireball.LaVatNho(c)) colLoNho++;
            }
        Ghi(string.Format("B3. {0} lo that, {1} collider - bi coi la vat nho (xuyen duoc) {2} (phai 0)", dsLo.Length, colLo, colLoNho));
        Kiem(dsLo.Length >= 10 && colLo >= 10 && colLoNho == 0, "lo lua that van bi coi la vat nho");

        // ================= C. DAP LO =================
        Ghi("");
        foreach (var lo in dsLo) lo.Chay();
        yield return null;
        if (dsLo.Length >= 4)
        {
            var lo0 = dsLo[0]; var lo1 = dsLo[1]; var lo2 = dsLo[2]; var lo3 = dsLo[3];
            int enemy = LayerMask.GetMask("Enemy");

            // C1. Qua cau bang bay vao lo0
            int bang0 = QuaCauBang.SoLoDapTat;
            Vector3 tamLo = lo0.transform.position + Vector3.up * 1.0f;
            Vector3 tuC = tamLo + new Vector3(4f, 0.2f, 0f);
            var qc = QuaCauBang.Spawn(tuC, (tamLo - tuC).normalized, lop, enemy);
            float hC = Time.time + 2f;
            while (qc != null && Time.time < hC) yield return null;
            // doi chung: no cach lo1 6 m (ngoai vung no 3,4 m) - no ngay luc sinh vi song 0,02 giay
            Vector3 choDc = lo1.transform.position + Vector3.up * 1.0f + new Vector3(6f, 0f, 0f);
            var qdc = QuaCauBang.Spawn(choDc, Vector3.up, 0, enemy);
            qdc.lifetime = 0.02f;
            float hDc = Time.time + 1f;
            while (qdc != null && Time.time < hDc) yield return null;
            Ghi(string.Format("C1. Qua cau bang no canh lo: lo tat {0} (dem +{1}); doi chung no cach lo khac 6 m: lo do van chay {2}",
                !lo0.DangChay, QuaCauBang.SoLoDapTat - bang0, lo1.DangChay));
            Kiem(!lo0.DangChay && QuaCauBang.SoLoDapTat - bang0 == 1, "Qua cau bang trung lo ma lo khong tat");
            Kiem(lo1.DangChay, "doi chung: qua no cach 6 m cung dap tat lo");

            // C2. Mua bang: tang bang roi canh lo2; doi chung roi cach lo3 5 m
            int mua0 = FallingShard.SoLoDapTat;
            var tb = VfxFactory.IceShardFalling(lo2.transform.position + new Vector3(0.6f, 0f, 0f), 8f, 0.3f);
            var fs = tb != null ? tb.GetComponent<FallingShard>() : null;
            bool coTangBang = fs != null;              // tang bang tu huy khi cham dat - ghi nhan luc tao
            if (fs != null) { fs.damage = 10f; fs.damageMask = enemy; }
            var tbDc = VfxFactory.IceShardFalling(lo3.transform.position + new Vector3(5f, 0f, 0f), 8f, 0.3f);
            var fsDc = tbDc != null ? tbDc.GetComponent<FallingShard>() : null;
            if (fsDc != null) { fsDc.damage = 10f; fsDc.damageMask = enemy; }
            yield return new WaitForSeconds(0.8f);
            Ghi(string.Format("C2. Mua bang: tang bang roi canh lo -> lo tat {0} (dem +{1}); doi chung roi cach lo khac 5 m -> lo do van chay {2}",
                !lo2.DangChay, FallingShard.SoLoDapTat - mua0, lo3.DangChay));
            Kiem(coTangBang && !lo2.DangChay && FallingShard.SoLoDapTat - mua0 == 1, "Mua bang trung lo ma lo khong tat");
            Kiem(lo3.DangChay, "doi chung: tang bang roi cach 5 m cung dap tat lo");

            // C3. 30 giay sau (nhu Gio loc) lo chay lai
            yield return new WaitForSeconds(GioLoc.GiayLoChayLai + 1.5f);
            Ghi(string.Format("C3. sau {0:F0} giay: lo trung Qua cau bang chay lai {1}, lo trung Mua bang chay lai {2}",
                GioLoc.GiayLoChayLai + 1.5f, lo0.DangChay, lo2.DangChay));
            Kiem(lo0.DangChay && lo2.DangChay, "lo bi dap khong chay lai sau 30 giay");
        }
        else { Ghi("[LOI] Act2 khong du 4 lo lua"); loi++; }

        // ================= D. ANH SACH PHEP =================
        Ghi("");
        CapDo.BatDauTranMoi();
        CapDo.ThemDiemChoPhepThu(2);
        CapDo.NangCap(CapDo.KyBinhMau);                 // binh mau cap 2
        CapDo.MoCaDuongChoPhepThu(ky);                  // toc do cap 1
        Ghi(string.Format("D. binh mau cap {0}/{1} (co san cap 1, vua nang 1 lan), binh mana cap {2}/{3}; toc do cap {4}",
            CapDo.CapCuaKyNang(CapDo.KyBinhMau), CapDo.CapToiDaCua(CapDo.KyBinhMau),
            CapDo.CapCuaKyNang(CapDo.KyBinhMana), CapDo.CapToiDaCua(CapDo.KyBinhMana), CapDo.CapCuaKyNang(ky)));
        Kiem(CapDo.CapCuaKyNang(CapDo.KyBinhMau) == 2 && CapDo.CapToiDaCua(CapDo.KyBinhMau) == 3, "binh mau khong len cap 2 / cap toi da khong phai 3");
        CuaSoSachPhep.Mo();
        CuaSoSachPhep.ChonKyNang(CapDo.KyBinhMau);
        CuaSoSachPhep.CuonKho = 99999f;                  // cuon het cot: hien nhom HO TRO + BI DONG
        yield return null; yield return null;
        yield return Chup("bonviec_1_binh_mau");
        CuaSoSachPhep.ChonKyNang(ky);
        yield return null; yield return null;
        yield return Chup("bonviec_2_toc_do");
        CuaSoSachPhep.Dong();

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
        Debug.Log("[BonViec] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        foreach (var g in rac) if (g != null) Object.DestroyImmediate(g);
        rac.Clear();
        File.WriteAllText("PlayTestShots/bonviec_sachphep.txt", bao.ToString());
        var r = GameObject.Find("TAM_BonViec");
        if (r != null) Object.DestroyImmediate(r);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
