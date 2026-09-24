using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 77): NHOM BI DONG va BON KY NANG "KHANG ..." (them 19/09/2026).
///
/// Nguoi dung: "Trong Sach phep them 1 nhom skill ten la bi dong, cac skill nam trong nhom nay moi khi
/// mo khoa hoac nang cap se tang vinh vien thuoc tinh cua nhan vat" + bon ky nang Khang Lua / Bang /
/// Set / Phong: "nhan sat thuong tu cac skill thuoc nhom [he] TU NGUOI CHOI KHAC giam 25% cho cap dau
/// tien, va moi lan nang cap se giam them 5%".
/// Hoi lai, nguoi dung chot: khong keo duoc vao o ky nang; tia set trong long Loc xoay / Gio loc tinh
/// he PHONG (theo nhom ky nang) chu khong phai SET.
///
///   A. Thong so: 20 ky nang, so hieu 16..19; nhom "BỊ ĐỘNG" co du bon ky nang; ten / tom tat / mo ta
///      co dau; HUD 20 icon; bon file icon tren dia.
///   B. Bang ti le: cap 1..5 = 25 / 30 / 35 / 40 / 45 %.
///   C. GIAM THAT tren mau: don Lua cua NGUOI CHOI KHAC vao minh - do mau mat truoc va sau khi mo khang,
///      roi nang len cap 5. Lam cho ca bon he (Lua / Bang / Set / Phong) bang don THAT cua tung nhom.
///   D. DOI CHUNG 1: don cua QUAI KHONG bi giam (nguoi dung chi xin chan don cua nguoi choi).
///      DOI CHUNG 2: khang he nay khong chan don he khac.
///   E. Tia set trong long LOC XOAY tinh he PHONG: Khang Phong chan no, Khang Set thi khong.
///   F. Khong tung duoc, khong keo vao o: CastAt(16..19) khong ton mana / khong vao hoi chieu; cot danh
///      sach Sach phep co dong tieu de "BỊ ĐỘNG"; khong keo duoc vao o ky nang.
///
/// Ket qua: PlayTestShots/khanghe.txt
/// </summary>
public static class ThuKhangHe
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/77. Chay thu KHANG HE (nhom bi dong)", false, 166)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu Khang he", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Khang he (nhom bi dong)");
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
        if (GameObject.Find("TAM_KhangHe") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_KhangHe");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[KhangHe] " + s); }
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

    /// <summary>Mot "nguoi choi khac" gia: Damageable co isPlayer = true, KHONG phai minh.</summary>
    static Damageable TaoNguoiKhac(Vector3 p)
    {
        var go = new GameObject("TAM_NguoiKhac");
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Player");
        var d = go.AddComponent<Damageable>();
        d.isPlayer = true;
        d.maxHealth = 1e6f; d.health = 1e6f;
        return d;
    }

    static Damageable TaoQuai(Vector3 p)
    {
        var go = new GameObject("TAM_Quai");
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var d = go.AddComponent<Damageable>();
        d.isPlayer = false;
        d.maxHealth = 1e6f; d.health = 1e6f;
        return d;
    }

    /// <summary>Danh mot don he <paramref name="loai"/> tu <paramref name="keDanh"/> vao <paramref name="nan"/>, tra ve so mau MAT THAT.</summary>
    static float DanhMotDon(Damageable keDanh, Damageable nan, float manh, DamageType loai)
    {
        float truoc = nan.health;
        nan.GhiKeDanh(keDanh);
        nan.TakeDamage(manh, loai, nan.transform.position + Vector3.up);
        return truoc - nan.health;
    }

    /// <summary>Nang mot ky nang len dung cap mong muon (them diem neu thieu).</summary>
    static void LenCap(int ky, int cap)
    {
        for (int i = 0; i < 40 && CapDo.CapCuaKyNang(ky) < cap; i++)
        {
            if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            if (!CapDo.DaMo(ky)) CapDo.MoKhoa(ky); else CapDo.NangCap(ky);
        }
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han0 = Time.time + 30f;
        while (dir == null && Time.time < han0) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);

        // Tat GameDirector suot phep thu (xem bai hoc 19/09/2026: dot quai sinh giua chung lam GPU qua tai)
        bool dirBatCu = false;
        if (dir != null) { dirBatCu = dir.enabled; dir.enabled = false; }
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude))
            if (q != null) Object.Destroy(q.gameObject);
        yield return null;

        var toi = TimToi();
        if (toi == null)
        {
            Ghi("[LOI] khong tim thay nhan vat"); loi++;
            if (dir != null) dir.enabled = dirBatCu;
            Ket(); yield break;
        }
        var mauToi = toi.GetComponent<Damageable>();
        // 1e5 chu khong 1e7: so lon thi float mat do chinh xac, tru 400 tu 1e7 co the lech vai don vi
        mauToi.maxHealth = 1e5f; mauToi.health = 1e5f;
        CapDo.BatDauTranMoi();

        // CHAN DOAN: cac khang co san tren nhan vat (fireResist... ap cho MOI nguon, khong dinh gi
        // toi bon ky nang Khang moi) va khien - hai thu nay lam so do lech neu khong biet truoc.
        Ghi(string.Format("(chan doan) khang co san tren nhan vat: lua {0:P0}, bang {1:P0}, set {2:P0}; khien {3}",
            mauToi.fireResist, mauToi.iceResist, mauToi.lightningResist,
            mauToi.khieng != null ? (mauToi.khieng.DangBat ? "DANG BAT" : "co nhung tat") : "khong co"));

        int[] ky4 = { CapDo.KyKhangLua, CapDo.KyKhangBang, CapDo.KyKhangSet, CapDo.KyKhangPhong };
        HeSat[] he4 = { HeSat.Lua, HeSat.Bang, HeSat.Set, HeSat.Phong };
        DamageType[] loai4 = { DamageType.Fire, DamageType.Ice, DamageType.Lightning, DamageType.Physical };
        string[] tenHe = { "LỬA", "BĂNG", "SÉT", "PHONG" };

        // ================= A. THONG SO =================
        Ghi("");
        {
            int nhomBiDong = -1;
            for (int g = 0; g < SachPhep.SoNhom; g++)
                if (SachPhep.TenNhom[g] == "BỊ ĐỘNG") nhomBiDong = g;

            var hud = GameHUD.Ban;
            var bo = hud != null ? hud.BoIcon() : null;
            var anh = new Texture2D[4];
            string[] tenAnh = { "Icons/KhangLua", "Icons/KhangBang", "Icons/KhangSet", "Icons/KhangPhong" };
            for (int i = 0; i < 4; i++) anh[i] = Resources.Load<Texture2D>(tenAnh[i]);

            var sb = new StringBuilder();
            // CO DU bon ky nang Khang (khong dem cung "== 4": 25/09/2026 nhom them Toc do di chuyen)
            bool duNhom = nhomBiDong >= 0;
            if (duNhom) foreach (var k4 in ky4) if (System.Array.IndexOf(SachPhep.KyNangTheoNhom[nhomBiDong], k4) < 0) duNhom = false;
            if (nhomBiDong >= 0)
                foreach (int k in SachPhep.KyNangTheoNhom[nhomBiDong]) sb.Append(k).Append(' ');

            Ghi(string.Format("A. so ky nang {0}; so hieu Khang: {1}/{2}/{3}/{4}; nhom \"{5}\" (thu {6}) gom ky nang: {7}",
                CapDo.SoKyNang, CapDo.KyKhangLua, CapDo.KyKhangBang, CapDo.KyKhangSet, CapDo.KyKhangPhong,
                nhomBiDong >= 0 ? SachPhep.TenNhom[nhomBiDong] : "KHONG CO", nhomBiDong, sb.ToString().Trim()));
            for (int i = 0; i < 4; i++)
                Ghi(string.Format("A. ky {0}: ten \"{1}\", tom tat \"{2}\", mo ta {3} ky tu; icon file {4}; LaKyBiDong {5}; cap toi da {6}",
                    ky4[i], SachPhep.Ten(ky4[i]), SachPhep.TomTat(ky4[i]), SachPhep.MoTa(ky4[i]).Length,
                    anh[i] != null ? anh[i].width + "x" + anh[i].height : "KHONG",
                    CapDo.LaKyBiDong(ky4[i]), CapDo.CapToiDaCua(ky4[i])));
            Ghi(string.Format("A. HUD {0} icon; icon so 16..19 {1}", bo != null ? bo.Length : -1,
                bo != null && bo.Length == CapDo.SoKyNang && bo[16] != null && bo[17] != null && bo[18] != null && bo[19] != null ? "co du" : "THIEU"));

            Kiem(CapDo.SoKyNang == SachPhep.SoKyNangCoTen() && CapDo.KyKhangLua == 16 && CapDo.KyKhangPhong == 19,
                 "so hieu sai, hoac co ky nang khong co ten");
            Kiem(duNhom, "Sach phep khong co nhom BỊ ĐỘNG du bon ky nang");
            Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[16] != null && bo[19] != null, "HUD thieu icon khang");
            for (int i = 0; i < 4; i++)
            {
                Kiem(anh[i] != null, "thieu file icon " + tenAnh[i]);
                Kiem(SachPhep.Ten(ky4[i]).StartsWith("KHÁNG") && SachPhep.MoTa(ky4[i]).Length > 100, "Sach phep thieu chu cho " + tenHe[i]);
                Kiem(CapDo.LaKyBiDong(ky4[i]) && CapDo.CapToiDaCua(ky4[i]) == 5, "ky nang khang khong phai bi dong / cap toi da khong phai 5");
            }
        }

        // ================= B. BANG TI LE =================
        Ghi("");
        {
            var sb = new StringBuilder();
            bool dung = true;
            float[] mong = { 0f, 0.25f, 0.30f, 0.35f, 0.40f, 0.45f };
            for (int c = 0; c <= 5; c++)
            {
                float g = KhangHe.GiamOCap(c);
                sb.Append("cap ").Append(c).Append("=").Append((g * 100f).ToString("F0")).Append("%  ");
                if (Mathf.Abs(g - mong[c]) > 0.0001f) dung = false;
            }
            Ghi("B. ti le giam theo cap: " + sb.ToString().Trim() + "  (mong 0 / 25 / 30 / 35 / 40 / 45 %)");
            Kiem(dung, "bang ti le giam khong dung 25% + 5% moi cap");
        }

        // ================= C. GIAM THAT TREN MAU =================
        Ghi("");
        var nguoiKhac = TaoNguoiKhac(toi.transform.position + toi.transform.forward * 4f);
        var quai = TaoQuai(toi.transform.position + toi.transform.right * 4f);
        yield return new WaitForFixedUpdate();
        const float DON = 400f;
        {
            for (int i = 0; i < 4; i++)
            {
                mauToi.health = mauToi.maxHealth;
                float truocKhiMo = DanhMotDon(nguoiKhac, mauToi, DON, loai4[i]);

                LenCap(ky4[i], 1);
                float cap1 = DanhMotDon(nguoiKhac, mauToi, DON, loai4[i]);

                int capSauKhiMo = CapDo.CapCuaKyNang(ky4[i]);
                LenCap(ky4[i], 5);
                int cap5That = CapDo.CapCuaKyNang(ky4[i]);
                float cap5 = DanhMotDon(nguoiKhac, mauToi, DON, loai4[i]);

                Ghi(string.Format("C. {0}: don {1:F0} tu nguoi choi khac -> chua mo mat {2:F1}; cap {7} mat {3:F1} (giam {4:P0}); cap {8} mat {5:F1} (giam {6:P0})",
                    tenHe[i], DON, truocKhiMo, cap1, 1f - cap1 / DON, cap5, 1f - cap5 / DON, capSauKhiMo, cap5That));
                Kiem(capSauKhiMo == 1 && cap5That == 5, tenHe[i] + ": khong len dung cap de do (thieu diem ky nang?)");
                Kiem(Mathf.Abs(truocKhiMo - DON) < 0.5f, tenHe[i] + ": chua mo khang ma da giam sat thuong");
                Kiem(Mathf.Abs(cap1 - DON * 0.75f) < 0.5f, tenHe[i] + ": cap 1 khong giam dung 25%");
                Kiem(Mathf.Abs(cap5 - DON * 0.55f) < 0.5f, tenHe[i] + ": cap 5 khong giam dung 45%");
            }
        }

        // ================= D. DOI CHUNG =================
        Ghi("");
        {
            // D1. don cua QUAI khong bi giam (luc nay ca bon khang deu dang cap 5)
            mauToi.health = mauToi.maxHealth;
            float tuQuai = DanhMotDon(quai, mauToi, DON, DamageType.Fire);

            // D1b. don KHONG ai ghi ke danh (don tho, vi du bay / moi truong)
            float khongAi = mauToi.health;
            mauToi.TakeDamage(DON, DamageType.Fire, mauToi.transform.position);
            khongAi -= mauToi.health;

            // D2. khang he nay khong chan he khac: ha Khang Set ve 0 bang cach bat dau tran moi
            CapDo.BatDauTranMoi();
            LenCap(CapDo.KyKhangLua, 5);
            float donLua = DanhMotDon(nguoiKhac, mauToi, DON, DamageType.Fire);
            float donSet = DanhMotDon(nguoiKhac, mauToi, DON, DamageType.Lightning);

            Ghi(string.Format("D. don LỬA 400 cua QUAI (dang co du bon khang cap 5) -> mat {0:F1} (phai la 400, quai khong bi chan)", tuQuai));
            Ghi(string.Format("D. don 400 KHONG ai ghi ke danh -> mat {0:F1} (phai la 400)", khongAi));
            Ghi(string.Format("D. chi mo Khang LỬA cap 5: don Lua mat {0:F1} (giam), don SÉT mat {1:F1} (khong giam)", donLua, donSet));
            Kiem(Mathf.Abs(tuQuai - DON) < 0.5f, "don cua QUAI cung bi khang chan - nguoi dung chi xin chan don cua nguoi choi");
            Kiem(Mathf.Abs(khongAi - DON) < 0.5f, "don khong ro nguon cung bi chan");
            Kiem(Mathf.Abs(donLua - DON * 0.55f) < 0.5f, "Khang Lua cap 5 khong giam dung 45%");
            Kiem(Mathf.Abs(donSet - DON) < 0.5f, "Khang Lua lai chan ca don he SET");
        }

        // ================= E. TIA SET CUA LOC TINH HE PHONG =================
        Ghi("");
        {
            // Chi mo Khang SET cap 5 -> tia set cua loc KHONG duoc giam (vi no la he Phong)
            CapDo.BatDauTranMoi();
            LenCap(CapDo.KyKhangSet, 5);
            mauToi.health = mauToi.maxHealth;
            float tiaLoc_khangSet = mauToi.health;
            mauToi.GhiKeDanh(nguoiKhac, HeSat.Phong);                   // dung cach Tornado.Bolt goi
            mauToi.TakeDamage(DON, DamageType.Lightning, mauToi.transform.position);
            tiaLoc_khangSet -= mauToi.health;

            // DOI CHUNG: don SET that (Giut set) van bi Khang Set chan
            float setThat = DanhMotDon(nguoiKhac, mauToi, DON, DamageType.Lightning);

            // Doi sang Khang PHONG cap 5 -> tia set cua loc BI giam
            CapDo.BatDauTranMoi();
            LenCap(CapDo.KyKhangPhong, 5);
            float tiaLoc_khangPhong = mauToi.health;
            mauToi.GhiKeDanh(nguoiKhac, HeSat.Phong);
            mauToi.TakeDamage(DON, DamageType.Lightning, mauToi.transform.position);
            tiaLoc_khangPhong -= mauToi.health;

            Ghi(string.Format("E. tia set trong long loc (400): chi co Khang SÉT cap 5 -> mat {0:F1} (phai 400); don Giựt sét that -> mat {1:F1} (phai 220)",
                tiaLoc_khangSet, setThat));
            Ghi(string.Format("E. cung tia set do, chi co Khang PHONG cap 5 -> mat {0:F1} (phai 220 - he tinh theo NHOM ky nang)", tiaLoc_khangPhong));
            Kiem(Mathf.Abs(tiaLoc_khangSet - DON) < 0.5f, "tia set cua loc bi Khang SET chan - phai la he PHONG");
            Kiem(Mathf.Abs(setThat - DON * 0.55f) < 0.5f, "doi chung hong: Khang Set khong chan don Set that");
            Kiem(Mathf.Abs(tiaLoc_khangPhong - DON * 0.55f) < 0.5f, "Khang Phong khong chan tia set cua loc");
        }

        // ================= F. KHONG TUNG DUOC, KHONG KEO VAO O =================
        Ghi("");
        {
            CapDo.BatDauTranMoi();
            LenCap(CapDo.KyKhangLua, 5);
            toi.mana = toi.maxMana;
            float mana0 = toi.mana;
            int soPhep = 0;
            System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; };
            toi.DaTungPhep += dem;
            for (int i = 0; i < 4; i++) toi.CastAt(ky4[i], toi.transform.position + toi.transform.forward * 6f);
            yield return new WaitForSeconds(0.3f);
            float tonMana = mana0 - toi.mana;
            float hoiChieu = toi.HoiChieuGiay(CapDo.KyKhangLua);
            toi.DaTungPhep -= dem;

            // Cot danh sach Sach phep: co dong tieu de BỊ ĐỘNG, va bon ky nang nam sau no
            int dongTieuDe = -1, dongKhangDau = -1;
            for (int d = 0; d < SachPhep.SoDongDanhSach; d++)
            {
                int nhom;
                int ky = SachPhep.KyNangODong(d, out nhom);
                if (ky < 0 && nhom >= 0 && SachPhep.TenNhom[nhom] == "BỊ ĐỘNG") dongTieuDe = d;
                if (ky == CapDo.KyKhangLua) dongKhangDau = d;
            }

            Ghi(string.Format("F. bam ca bon ky nang khang: {0} phep bay ra (phai 0), ton {1:F0} mana (phai 0), hoi chieu {2:F2} s (phai 0)",
                soPhep, tonMana, hoiChieu));
            Ghi(string.Format("F. cot danh sach: dong tieu de \"BỊ ĐỘNG\" o hang {0}, Kháng Lửa ngay sau o hang {1}; tong {2} hang",
                dongTieuDe, dongKhangDau, SachPhep.SoDongDanhSach));
            Kiem(soPhep == 0 && tonMana < 0.01f && hoiChieu < 0.01f, "ky nang bi dong van tung ra phep / ton mana");
            Kiem(dongTieuDe >= 0 && dongKhangDau == dongTieuDe + 1, "cot danh sach khong co nhom BỊ ĐỘNG dung cho");
        }

        if (nguoiKhac != null) Object.Destroy(nguoiKhac.gameObject);
        if (quai != null) Object.Destroy(quai.gameObject);
        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude))
            if (d0 != null && d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
        if (dir != null) dir.enabled = dirBatCu;
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
        Debug.Log("[KhangHe] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/khanghe.txt", bao.ToString());
        var rac = GameObject.Find("TAM_KhangHe");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
