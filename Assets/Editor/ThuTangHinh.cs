using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 73): KY NANG "TANG HINH" (so hieu 12, them 18/09/2026).
///
/// Nguoi dung: than nguoi trong suot nhu Dark Templar; QUAI KHONG THAY va khong danh; nguoi choi khac chi thay duong net khi
/// minh DI CHUYEN, dung yen thi khong thay; MIEN moi hieu ung; di nhanh hon 20%; 20 giay; don danh dau tien bang mot ky nang
/// gay sat thuong an GAP DOI roi tan tang hinh; hoi chieu 30 giay. Nguoi dung chon them: x2 (khong phai x3); chi ky nang GAY
/// SAT THUONG moi tinh; mien hieu ung NHUNG VAN AN SAT THUONG; 30 nang luong, niem 0,38; dung yen mo dan 0,4 giay; CHINH MINH
/// van thay than minh mo mo; quai dang duoi thi mat dau ngay; icon dung bang Blender MCP.
///
///   A. Thong so: so hieu 12, 13 ky nang, nang luong / hoi chieu / niem that tren nhan vat + Sach phep; ten co dau; HUD 13 icon.
///   B. Tung that CastAt(12): khoa -> tu choi; mo khoa -> co component TangHinh, tru 30 nang luong, hoi chieu 30 giay.
///   C. Hinh: moi renderer cua model doi sang shader Diablo25D/TangHinh (DOI CHUNG: truoc khi bat la vat lieu khac); tat tang
///      hinh thi tra lai dung vat lieu cu. Chup anh.
///   D. Quai KHONG THAY: 6 quai that quanh nguoi -> truoc khi tang hinh chung chon minh lam muc tieu, sau khi bat thi 0 con
///      nham minh (GameDirector.GanNhat cung tra null); tat tang hinh thi chung nham lai.
///   E. Mien hieu ung: dong bang / cham / choang / nga / hat tung / thieu dot ap vao nguoi dang tang hinh -> khong dinh cai nao
///      (DOI CHUNG: tat tang hinh roi ap lai -> dinh du); hieu ung DANG DINH bi xoa khi bat; VAN AN SAT THUONG.
///   F. Toc do: do quang duong that trong 1 giay khi di thang - tang hinh / binh thuong ~ 1,20.
///   G. Don dau gap doi: bia do don an don Qua cau lua khi tang hinh vs khi khong -> ti le 2,00; sau don do tang hinh TAN;
///      don thu hai khong con nhan doi. Khien (khong gay sat thuong) khong lam tan tang hinh.
///   H. Het 20 giay thi tu tan.
///   J. Hai giay cuoi thi than NHAP NHAY (chi may cua MINH) - truoc do do hien dung yen; ban sao nguoi khac khong nhap nhay.
///   K. Don dau x2 cho TOAN BO sat thuong cua ky nang: Mua bang (moi vet roi) va Qua cau bang cap 5 (moi qua) deu x2,
///      khong phai chi cu trung dau tien (neu vay ti le se la 1,03 va 1,20).
///   L. Co "don dau tang hinh" di qua goi phep: viet-doc lai dung; ban sao phat lai phep theo goi co co thi x2; goi
///      minh gui di khi danh don dau co mang co.
///   ⚠️ 26/09/2026: tang hinh 90 GIAY; hoi chieu 10 giay dem tu luc HIEN HINH (dang tang hinh bam lai bi tu choi, o ky nang giu
///      day); het tang hinh (het gio HOAC don dau) no VONG PHEP 5 m: 100 + 10% mau TOI DA, +4%/cap.
///   V. Vong phep: bia co mau toi da khac nhau, mau hien tai chi con mot nua (phan biet "mau toi da" voi "mau hien tai"), mot bia
///      NGOAI 5 m, nguoi tung dung giua -> moi bia trong vong mat DUNG 100 + 10% mau toi da, bia ngoai 0, nguoi tung 0; hinh:
///      co tam vong phep, vong ngoai cua anh dung 5 m (do khung bao luoi that); cap 5 (tung THAT) -> 100 + 26%; tan do DON DAU
///      (Qua cau lua) cung no vong; het tang hinh -> hoi chieu 10 giay bat dau dem.
///   W. Ban sao nguoi khac: het tang hinh -> vong no tren may nay; bit "tang hinh" CU con tre trong goi tin ngay sau do khong
///      tao lai tang hinh (khong co vong thu hai).
///   I. Qua mang: minh tang hinh -> goi trang thai mang bit CoTangHinh (mat na goi len 0x3F); ban sao nhan bit -> co TangHinh,
///      dung yen thi renderer tat, di chuyen thi bat lai.
///
/// Ket qua: PlayTestShots/tanghinh.txt, anh tanghinh_*.png.
/// </summary>
public static class ThuTangHinh
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyTangHinh;

    [MenuItem("Diablo 2.5D/73. Chay thu TANG HINH (ky nang moi)", false, 162)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu tang hinh", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Tang hinh");
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
        if (GameObject.Find("TAM_TangHinh") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_TangHinh");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TangHinh] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static Damageable TaoBia(string ten, Vector3 p)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 100000f; d.health = 100000f;
        return d;
    }

    /// <summary>Bia THAN to: moi qua cau trong chum va moi vet mua bang deu dam trung, so do khong phu thuoc may man.</summary>
    static Damageable TaoBiaTo(string ten, Vector3 p, float banKinh)
    {
        var d = TaoBia(ten, p);
        var cap = d.GetComponent<CapsuleCollider>();
        cap.radius = banKinh; cap.height = banKinh * 2f + 2f; cap.center = Vector3.up * (banKinh + 1f);
        Physics.SyncTransforms();
        return d;
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

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>Chup anh trong khi GIU NGUYEN pha nhap nhay - anh mat vai khung moi ra dia, tha ra thi roi vao pha bat ky.</summary>
    static IEnumerator ChupGiuPha(string ten, TangHinh t, float pha)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 120 && !File.Exists(duong); i++)
        {
            if (t != null) t.conLai = pha;
            yield return new WaitForEndOfFrame();
        }
    }

    static IEnumerator GiuSong()
    {
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoTangHinh") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static Vector3 HuongTrong(PlayerController pc)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 0.6f, h, out RaycastHit _, 14f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    /// <summary>So renderer cua model dang dung shader tang hinh.</summary>
    static int DemShaderTangHinh(GameObject go)
    {
        int n = 0;
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            foreach (var m in r.sharedMaterials)
                if (m != null && m.shader == Mats.TangHinhShader) { n++; break; }
        }
        return n;
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
        int maskEnemy = LayerMask.GetMask("Enemy");

        // ================= A. THONG SO =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/TangHinh");
        Ghi(string.Format("A. so hieu {0}, so ky nang {1}; nang luong {2}, hoi chieu {3} s, niem {4} s; Sach phep doc {5}/{6}/{7}; thoi gian {8} s, he so toc {9}, nhan don dau {10}",
            K, CapDo.SoKyNang, toi.tangHinhCost, toi.tangHinhCooldown, toi.tangHinhCastTime, nl, hc, nc, TangHinh.ThoiGian, TangHinh.HeSoToc, TangHinh.NhanDonDau));
        Ghi(string.Format("A. ten \"{0}\", tom tat \"{1}\", mo ta {2} ky tu; HUD {3} icon, icon so 12 {4}; icon file {5}",
            SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length, bo != null ? bo.Length : -1,
            bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG", tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG"));
        Kiem(K == 12 && CapDo.SoKyNang == SachPhep.SoKyNangCoTen(), "so hieu sai, hoac co ky nang khong co ten");
        Kiem(Mathf.Approximately(toi.tangHinhCost, 30f) && Mathf.Approximately(nl, 30f), "nang luong khong phai 30");
        Kiem(Mathf.Approximately(toi.tangHinhCooldown, 10f) && Mathf.Approximately(hc, 10f), "hoi chieu khong phai 10 giay");
        Kiem(Mathf.Approximately(TangHinh.ThoiGian, 90f) && Mathf.Approximately(TangHinh.HeSoToc, 1.2f) && Mathf.Approximately(TangHinh.NhanDonDau, 2f),
             "hang so tang hinh sai (90 s / 1,2 / x2)");
        Kiem(SachPhep.MoTa(K).Contains("90 giây") && SachPhep.MoTa(K).Contains("VÒNG PHÉP") && SachPhep.MoTa(K).Contains("10 giây"), "Sach phep chua ghi 90 giay / vong phep / hoi chieu 10");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Tang hinh");
        Kiem(SachPhep.Ten(K) == "TÀNG HÌNH" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Tang hinh");

        // ================= B + C. TUNG THAT VA HINH =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        int soPhep = 0, kyVua = -1;
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; kyVua = s; };
        toi.DaTungPhep += dem;
        CapDo.BatDauTranMoi();
        int s0 = soPhep;
        toi.CastAt(K, toi.transform.position);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoCaDuongChoPhepThu(K);
        yield return new WaitForSeconds(0.3f);
        int shaderTruoc = DemShaderTangHinh(toi.gameObject);
        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana;
        toi.CastAt(K, toi.transform.position);
        float manaTon = manaTruoc - toi.mana;      // doc NGAY: doi 0,6 giay thi nang luong da hoi lai mot it (lan dau do ra 24,5)
        yield return new WaitForSeconds(0.6f);
        var tg = toi.GetComponent<TangHinh>();
        float conLaiNgay = tg != null ? tg.conLai : -1f;   // doc NGAY 0,6 s sau khi tung (anh vong phep phia duoi cho them vai giay)
        float hoiNgaySau = toi.HoiChieuGiay(K);
        int shaderSau = DemShaderTangHinh(toi.gameObject);
        yield return Chup("tanghinh_1_dang_tang_hinh");
        // Anh vong phep o GOC CHOI THAT luc canh con SACH (anh muc V chup sau nhieu muc thu - khoi / hieu ung sot che mat nua xa)
        {
            Vector3 choAnh = toi.transform.position + huong * 7f; choAnh.y = VfxFactory.GroundY(choAnh);
            VfxFactory.VongPhepTangHinh(choAnh, TangHinh.BanKinhVong);
            yield return new WaitForSeconds(0.35f);
            yield return Chup("tanghinh_6_vong_phep_goc_choi");
            yield return new WaitForSeconds(1.8f);
        }
        // Dang tang hinh bam lai: bi tu choi, khong ton nang luong; o ky nang giu DAY 10 giay
        toi.mana = toi.maxMana;
        int sLai = soPhep; float manaLai = toi.mana;
        toi.CastAt(K, toi.transform.position);
        bool tuChoiKhiDang = soPhep == sLai && Mathf.Approximately(toi.mana, manaLai);
        yield return new WaitForSeconds(1.0f);
        float hoiSau1s = toi.HoiChieuGiay(K);
        Ghi(string.Format("B. dang tang hinh bam lai -> tu choi {0}; hoi chieu giu day: ngay sau {1:F2} s, 1 giay sau van {2:F2} s (mong 10)", tuChoiKhiDang, hoiNgaySau, hoiSau1s));
        Kiem(tuChoiKhiDang, "dang tang hinh ma van tung lai duoc");
        Kiem(Mathf.Abs(hoiNgaySau - 10f) < 0.05f && Mathf.Abs(hoiSau1s - 10f) < 0.05f, "dang tang hinh ma hoi chieu khong giu day 10 giay");
        Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> tung {1}, co trang thai TangHinh {2} (con {3:F1} s), ton {4} nang luong, hoi chieu ngay sau {5:F1} s",
            tuChoiKhoa, soPhep == s0 + 1 && kyVua == K, tg != null, conLaiNgay, manaTon, hoiNgaySau));
        Ghi(string.Format("C. renderer dung shader tang hinh: truoc khi bat {0}, sau khi bat {1}", shaderTruoc, shaderSau));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(tg != null && conLaiNgay > 88f, "tung xong khong vao trang thai tang hinh 90 giay");
        Kiem(Mathf.Abs(manaTon - 30f) < 0.01f, "khong ton dung 30 nang luong");
        Kiem(shaderTruoc == 0 && shaderSau > 0, "than nguoi khong doi sang shader tang hinh");

        // ================= E. MIEN HIEU UNG (dang tang hinh) =================
        Ghi("");
        {
            float mau0 = mauToi.health;
            FrozenEffect.Apply(mauToi, 2f);
            FrozenEffect.ApCham(mauToi, 0.5f, 2f);
            StunnedEffect.Apply(mauToi, 2f);
            BiDanhNga.Apply(mauToi, 1.5f);
            BiHatTung.Apply(mauToi, 0.5f);
            BurningEffect.Apply(mauToi, 10f, 3f, null);
            yield return null;
            bool dinh = mauToi.GetComponent<FrozenEffect>() != null || mauToi.GetComponent<StunnedEffect>() != null
                     || mauToi.GetComponent<BiDanhNga>() != null || mauToi.GetComponent<BiHatTung>() != null
                     || mauToi.GetComponent<BurningEffect>() != null;
            // VAN AN SAT THUONG
            mauToi.TakeDamage(100f, DamageType.Physical, mauToi.transform.position);
            float mat = mau0 - mauToi.health;
            Ghi(string.Format("E. dang tang hinh: ap 6 hieu ung -> dinh cai nao {0}; van mat mau khi bi danh: {1:F0}", dinh, mat));
            Kiem(!dinh, "dang tang hinh ma van dinh hieu ung");
            Kiem(Mathf.Abs(mat - 100f) < 0.5f, "tang hinh ma khong con an sat thuong (nguoi dung chon van an)");
        }

        // ================= F. TOC DO =================
        Ghi("");
        {
            float tocTangHinh = 0f, tocThuong = 0f;
            for (int lan = 0; lan < 2; lan++)
            {
                if (lan == 1)
                {
                    var t2 = toi.GetComponent<TangHinh>(); if (t2 != null) t2.Tat();
                    yield return new WaitForSeconds(0.3f);
                }
                toi.transform.rotation = Quaternion.LookRotation(huong);
                Vector3 p0 = toi.transform.position;
                float t0 = Time.time;
                // Bom input nhu cac phep thu khac (ThuBangSet.DoDiChuyen): day can ve mot huong
                var g = GoiInput.Rong(0f);
                g.huongDi = huong.normalized;
                toi.tuDocInput = false;
                toi.input = g;
                while (Time.time < t0 + 1f) yield return null;
                toi.input = GoiInput.Rong(0f);
                toi.tuDocInput = true;
                float quang = Vector3.Distance(new Vector3(p0.x, 0, p0.z), new Vector3(toi.transform.position.x, 0, toi.transform.position.z));
                if (lan == 0) tocTangHinh = quang / (Time.time - t0); else tocThuong = quang / (Time.time - t0);
                // ve cho cu
                var cc = toi.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                toi.transform.position = p0;
                if (cc != null) cc.enabled = true;
                yield return new WaitForSeconds(0.2f);
            }
            Ghi(string.Format("F. toc do do duoc: tang hinh {0:F2} m/s, binh thuong {1:F2} m/s -> x{2:F3} (mong 1,200)",
                tocTangHinh, tocThuong, tocThuong > 0 ? tocTangHinh / tocThuong : 0f));
            Kiem(tocThuong > 0.5f && Mathf.Abs(tocTangHinh / tocThuong - 1.2f) < 0.05f, "tang hinh khong nhanh hon 20%");
        }

        // ================= E2. KHONG TANG HINH THI DINH DU (DOI CHUNG) =================
        Ghi("");
        {
            // LUU Y: BurningEffect.Apply lam TAN BANG (luat cu cua game - lua tan bang). Ap chung mot luot thi bang bi xoa
            // ngay va doi chung ra 2/3 (lan chay dau 18/09/2026). Nen do LAM HAI DOT: bang + choang, roi rieng thieu dot.
            FrozenEffect.Apply(mauToi, 2f);
            StunnedEffect.Apply(mauToi, 2f);
            yield return null;
            bool coBang = mauToi.GetComponent<FrozenEffect>() != null;
            bool coChoang = mauToi.GetComponent<StunnedEffect>() != null;
            var fCu = mauToi.GetComponent<FrozenEffect>(); if (fCu != null) fCu.Thaw();
            yield return null;
            BurningEffect.Apply(mauToi, 10f, 3f, null);
            yield return null;
            bool coChay = mauToi.GetComponent<BurningEffect>() != null;
            int soDinh = (coBang ? 1 : 0) + (coChoang ? 1 : 0) + (coChay ? 1 : 0);
            Ghi(string.Format("E2. DOI CHUNG (khong tang hinh): dong bang {0}, choang {1}, thieu dot {2} -> dinh {3}/3", coBang, coChoang, coChay, soDinh));

            // Bat tang hinh khi DANG dinh -> phai bi xoa sach
            toi.mana = toi.maxMana;
            TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            yield return null;
            int conLai = 0;
            if (mauToi.GetComponent<FrozenEffect>() != null) conLai++;
            if (mauToi.GetComponent<StunnedEffect>() != null) conLai++;
            if (mauToi.GetComponent<BurningEffect>() != null) conLai++;
            Ghi(string.Format("E2. bat tang hinh luc dang dinh 3 hieu ung -> con lai {0}", conLai));
            Kiem(conLai == 0, "bat tang hinh ma hieu ung dang dinh khong bi xoa");
        }

        // ================= D. QUAI KHONG THAY =================
        Ghi("");
        {
            var quai = new List<EnemyAI>(Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude));
            if (quai.Count < 3 && dir != null) { dir.SinhDotQuanhNguoi(); yield return new WaitForSeconds(1.5f); quai = new List<EnemyAI>(Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)); }
            // Tat tang hinh de chung nham minh truoc da
            var t3 = toi.GetComponent<TangHinh>(); if (t3 != null) t3.Tat();
            yield return new WaitForSeconds(1.6f);
            int nhamTruoc = 0;
            foreach (var q in quai) if (q != null && q.target == toi.transform) nhamTruoc++;
            var ganTruoc = dir != null ? dir.GanNhat(toi.transform.position + Vector3.right * 6f) : null;

            TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            yield return new WaitForSeconds(1.6f);
            int nhamSau = 0;
            foreach (var q in quai) if (q != null && q.target == toi.transform) nhamSau++;
            var ganSau = dir != null ? dir.GanNhat(toi.transform.position + Vector3.right * 6f) : null;
            Ghi(string.Format("D. {0} quai that: nham minh truoc khi tang hinh {1}, sau khi tang hinh {2}; GameDirector.GanNhat: truoc {3}, sau {4}",
                quai.Count, nhamTruoc, nhamSau, ganTruoc != null ? ganTruoc.name : "null", ganSau != null ? ganSau.name : "null"));
            Kiem(quai.Count >= 3 && nhamTruoc > 0 && ganTruoc != null, "doi chung: quai khong nham minh ngay ca khi chua tang hinh - phep do vo nghia");
            Kiem(nhamSau == 0 && ganSau == null, "quai van thay nguoi dang tang hinh");
            yield return Chup("tanghinh_2_quai_mat_dau");
        }

        // ================= G. DON DAU GAP DOI =================
        Ghi("");
        {
            var t4 = toi.GetComponent<TangHinh>(); if (t4 != null) t4.Tat();
            yield return new WaitForSeconds(0.4f);
            // Mo khoa Qua cau lua: can mot diem ky nang (diem dau da dung cho Tang hinh)
            if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            CapDo.MoCaDuongChoPhepThu(0);
            Vector3 h2 = HuongTrong(toi);
            toi.transform.rotation = Quaternion.LookRotation(h2);
            // Bia THAN TO cach 7 m: ca ba qua trong chum deu dam trung, so do khong phu thuoc may man
            // (bia nho 0,4 m o 8 m do ra luc 124,5 luc 187,8 tuy so qua trung - 18/09/2026).
            Vector3 choBia = toi.transform.position + h2 * 7f;

            // 1) don thuong
            var b1 = TaoBiaTo("TAM_G1", choBia, 2.5f);
            yield return new WaitForFixedUpdate();
            float m1 = b1.health;
            toi.mana = toi.maxMana;
            foreach (var fx in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)) Object.DestroyImmediate(fx.gameObject);
            toi.CastAt(0, b1.transform.position);
            // Doc SO GHI TREN TUNG QUA: dem mau bia mat thi ti le ra 1,4 - 2,1 tuy so qua trung duoc bia
            float donGhiThuong = 0f; int soQuaG0 = 0;
            float hanG0 = Time.time + 2f;
            while (soQuaG0 == 0 && Time.time < hanG0)
            {
                yield return null;
                foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None))
                { soQuaG0++; donGhiThuong = Mathf.Max(donGhiThuong, f.impactDamage); }
            }
            float han1 = Time.time + 3f;
            while (Time.time < han1 && b1.health >= m1) yield return null;
            yield return new WaitForSeconds(0.2f);
            float donThuong = m1 - b1.health;
            Object.Destroy(b1.gameObject);
            yield return new WaitForSeconds(1.2f);

            // 2) don dau khi dang tang hinh
            TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            var b2 = TaoBiaTo("TAM_G2", choBia, 2.5f);
            yield return new WaitForFixedUpdate();
            float m2 = b2.health;
            toi.mana = toi.maxMana;
            int donDau0 = TangHinh.SoLanDonDau;
            foreach (var fx in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)) Object.DestroyImmediate(fx.gameObject);
            toi.CastAt(0, b2.transform.position);
            float donGhiTang = 0f; int soQuaG1 = 0;
            float hanG1 = Time.time + 2f;
            while (soQuaG1 == 0 && Time.time < hanG1)
            {
                yield return null;
                foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None))
                { soQuaG1++; donGhiTang = Mathf.Max(donGhiTang, f.impactDamage); }
            }
            float han2 = Time.time + 3f;
            while (Time.time < han2 && b2.health >= m2) yield return null;
            yield return new WaitForSeconds(0.2f);
            float donTang = m2 - b2.health;
            bool conTangHinh = toi.GetComponent<TangHinh>() != null;
            int demDonDau = TangHinh.SoLanDonDau - donDau0;
            Object.Destroy(b2.gameObject);
            yield return new WaitForSeconds(1.2f);

            // 3) Khien khong lam tan tang hinh
            TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            CapDo.MoCaDuongChoPhepThu(5);
            toi.mana = toi.maxMana;
            toi.CastAt(5, toi.transform.position);
            yield return new WaitForSeconds(0.8f);
            bool conSauKhien = toi.GetComponent<TangHinh>() != null;
            var t5 = toi.GetComponent<TangHinh>(); if (t5 != null) t5.Tat();

            Ghi(string.Format("G. (mo khoa Qua cau lua: {6}) chum Qua cau lua: sat thuong GHI TREN QUA binh thuong {0:F2} ({7} qua), dang tang hinh {1:F2} ({8} qua) -> x{2:F2} (mong 2,00); bo dem don dau +{3}; sau don do con tang hinh {4}; tung KHIEN thi con tang hinh {5}",
                donGhiThuong, donGhiTang, donGhiThuong > 0f ? donGhiTang / donGhiThuong : 0f, demDonDau, conTangHinh, conSauKhien,
                CapDo.DaMo(0), soQuaG0, soQuaG1));
            Ghi(string.Format("G2. (tham khao) mau bia mat: {0:F1} va {1:F1} - KHONG kiem so nay vi so qua trung bia moi lan mot khac",
                donThuong, donTang));
            Kiem(soQuaG0 > 0 && soQuaG0 == soQuaG1, "hai lan tung ra so qua khac nhau - phep do vo nghia");
            Kiem(donGhiThuong > 1f && Mathf.Abs(donGhiTang / donGhiThuong - 2f) < 0.01f, "don dau khong gap doi");
            Kiem(demDonDau == 1 && !conTangHinh, "don dau khong lam tan tang hinh");
            Kiem(conSauKhien, "tung Khien (khong gay sat thuong) ma tang hinh lai tan");
        }

        // ================= J. NHAP NHAY HAI GIAY CUOI (chi MINH thay) =================
        Ghi("");
        {
            TangHinh.Bat(mauToi, 3.2f);
            float dauMin = 9f, dauMax = -9f, cuoiMin = 9f, cuoiMax = -9f;
            int soDoiChieu = 0, soKhungCuoi = 0; bool dangToi = false, daDat = false;
            bool coCoNhapNhay = false;
            while (true)
            {
                var t = toi.GetComponent<TangHinh>();
                if (t == null || t.conLai <= 0f) break;
                if (t.conLai > TangHinh.GiayNhapNhay)
                {
                    dauMin = Mathf.Min(dauMin, t.mucHien); dauMax = Mathf.Max(dauMax, t.mucHien);
                }
                else
                {
                    soKhungCuoi++;
                    if (t.dangNhapNhay) coCoNhapNhay = true;
                    cuoiMin = Mathf.Min(cuoiMin, t.mucHien); cuoiMax = Mathf.Max(cuoiMax, t.mucHien);
                    bool toiBayGio = t.mucHien < 0.3f;
                    if (!daDat) { dangToi = toiBayGio; daDat = true; }
                    else if (toiBayGio != dangToi) { soDoiChieu++; dangToi = toiBayGio; }
                }
                yield return null;
            }
            Ghi(string.Format("J. do hien cua than minh: luc con > {0} giay {1:F2}..{2:F2} (dung yen, khong nhap nhay); trong {0} giay cuoi {3:F2}..{4:F2}, doi sang/toi {5} lan trong {6} khung (nhip {7} s)",
                TangHinh.GiayNhapNhay, dauMin, dauMax, cuoiMin, cuoiMax, soDoiChieu, soKhungCuoi, TangHinh.NhipNhapNhay));
            Kiem(dauMax - dauMin < 0.05f && dauMin > 0.9f, "chua toi 2 giay cuoi ma than da nhap nhay (hoac mo di)");
            Kiem(coCoNhapNhay && soDoiChieu >= 8 && cuoiMin < 0.3f && cuoiMax > 0.9f, "hai giay cuoi khong nhap nhay");

            // Hai tam anh o hai pha cua cung mot nhip: nua SANG va nua MO.
            // Tat cai khieng con lai tu muc G - vom vang che kin nguoi, nhin anh khong ro than nhan vat.
            foreach (var kh in Object.FindObjectsByType<Khieng>(FindObjectsInactive.Exclude)) if (kh != null) kh.Tat();
            yield return new WaitForSeconds(0.3f);
            var tAnh = TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            yield return null;
            // Update tru dt TRUOC khi tinh nhip, nen chon pha nam GIUA moi nua chu khong sat mep:
            // 1,28 - dt ~ 1,264 -> Repeat = 0,164 (nua sang); 1,17 - dt ~ 1,154 -> Repeat = 0,054 (nua mo).
            float phaSang = 1.28f, phaMo = 1.17f;
            tAnh.conLai = phaSang; yield return null;
            float mucSang = tAnh.mucHien;
            yield return ChupGiuPha("tanghinh_3_nhapnhay_sang", tAnh, phaSang);
            tAnh.conLai = phaMo; yield return null;
            float mucMo = tAnh.mucHien;
            yield return ChupGiuPha("tanghinh_4_nhapnhay_mo", tAnh, phaMo);
            Ghi(string.Format("J. hai tam anh cung mot nhip: nua sang do hien {0:F2}, nua mo {1:F2}", mucSang, mucMo));
            Kiem(mucSang > 0.9f && mucMo < 0.3f, "hai pha nhap nhay khong khac nhau");
            var tAnhX = toi.GetComponent<TangHinh>(); if (tAnhX != null) tAnhX.Tat();
            yield return new WaitForSeconds(0.3f);
        }

        // ================= K. DON DAU x2 CHO TOAN BO SAT THUONG CUA KY NANG =================
        //  Nguoi dung 18/09/2026: "Mua bang thi TAT CA cac vet sang roi xuong deu tinh sat thuong 200%;
        //  Qua cau bang danh ra bao nhieu qua thi MOI qua deu x2" - khong phai chi cu trung dau tien.
        //  Neu chi cu dau tien x2 thi ti le tong se la (n+1)/n (1,03 voi 39 vet, 1,20 voi 5 qua), khong phai 2,00.
        Ghi("");
        {
            // Quai con song gan bia se hut bot vet mua bang (Mua bang nham ke dich 100%) -> don het cho sach
            int soQuaiDon = 0;
            foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude))
                if (q != null) { Object.Destroy(q.gameObject); soQuaiDon++; }
            yield return new WaitForSeconds(0.3f);
            var tK = toi.GetComponent<TangHinh>(); if (tK != null) tK.Tat();
            yield return new WaitForSeconds(0.3f);

            for (int i = 0; i < 12; i++) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));   // du diem de nang Qua cau bang len cap 5
            CapDo.MoCaDuongChoPhepThu(1);
            CapDo.MoCaDuongChoPhepThu(CapDo.KyQuaCauBang);
            for (int i = 0; i < 4 && CapDo.NangCapDuoc(CapDo.KyQuaCauBang); i++) CapDo.NangCap(CapDo.KyQuaCauBang);

            Vector3 hK = HuongTrong(toi);
            toi.transform.rotation = Quaternion.LookRotation(hK);

            // --- K1. MUA BANG: 39 vet roi trong 5 giay ---
            //  TONG sat thuong khong phai thuoc do chinh xac: AreaDamage giam dan tu tam ra ria (1 -> 0,55) va diem roi
            //  ngau nhien, nen hai con bao khong bao gio cong bang tuyet doi (do duoc 1,88 va 2,01 hai lan chay).
            //  Thuoc do CHINH la sat thuong CUA TUNG VET roi xuong: doc tren tung FallingShard sinh ra trong ca con bao -
            //  vet CUOI CUNG roi sau khi tang hinh da tan tu lau van phai mang so x2 (do la dieu nguoi dung xin).
            var tongMua = new float[2]; var vetMua = new int[2];
            var vetMin = new float[2]; var vetMax = new float[2]; var soVetDoc = new int[2];
            for (int lan = 0; lan < 2; lan++)
            {
                if (lan == 1) TangHinh.Bat(mauToi, TangHinh.ThoiGian);
                var bia = TaoBiaTo("TAM_K1_" + lan, toi.transform.position + hK * 9f, 1.2f);
                yield return new WaitForFixedUpdate();
                float m0 = bia.health, truoc = bia.health;
                int demTut = 0;
                vetMin[lan] = 1e9f; vetMax[lan] = -1e9f;
                var daDoc = new HashSet<Object>();   // khoa bang chinh tham chieu (GetInstanceID la loi bien dich tren Unity 6.5)
                toi.mana = toi.maxMana;
                toi.CastAt(1, bia.transform.position);
                float han = Time.time + 9f;
                while (Time.time < han)
                {
                    if (bia.health < truoc - 0.01f) { demTut++; truoc = bia.health; }
                    foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsInactive.Exclude))
                    {
                        if (fs == null || !daDoc.Add(fs)) continue;
                        vetMin[lan] = Mathf.Min(vetMin[lan], fs.damage);
                        vetMax[lan] = Mathf.Max(vetMax[lan], fs.damage);
                        soVetDoc[lan]++;
                    }
                    yield return null;
                }
                tongMua[lan] = m0 - bia.health; vetMua[lan] = demTut;
                Object.Destroy(bia.gameObject);
                var tx = toi.GetComponent<TangHinh>(); if (tx != null) tx.Tat();
                yield return new WaitForSeconds(0.6f);
            }
            Ghi(string.Format("K1. MUA BANG ({0} quai da don): sat thuong GHI TREN TUNG VET roi - binh thuong {1} vet, {2:F2}..{3:F2}; dang tang hinh {4} vet, {5:F2}..{6:F2} -> x{7:F2} (mong 2,00; neu chi vet dau x2 thi vet thu hai tro di van la {2:F2})",
                soQuaiDon, soVetDoc[0], vetMin[0], vetMax[0], soVetDoc[1], vetMin[1], vetMax[1], vetMin[0] > 0 ? vetMin[1] / vetMin[0] : 0f));
            float tbThuong = vetMua[0] > 0 ? tongMua[0] / vetMua[0] : 0f;
            float tbTang    = vetMua[1] > 0 ? tongMua[1] / vetMua[1] : 0f;
            Ghi(string.Format("K1. mau bia that su mat: binh thuong {0:F0} qua {1} lan tut ({2:F1}/lan); dang tang hinh {3:F0} qua {4} lan ({5:F1}/lan) -> moi lan trung x{6:F2}. TONG khong tron 2,00 (x{7:F2}) vi so vet TRUNG bia moi con bao moi khac va sat thuong vung giam dan tu tam ra ria",
                tongMua[0], vetMua[0], tbThuong, tongMua[1], vetMua[1], tbTang, tbThuong > 0 ? tbTang / tbThuong : 0f,
                tongMua[0] > 0 ? tongMua[1] / tongMua[0] : 0f));
            Kiem(soVetDoc[0] >= 25 && soVetDoc[1] >= 25, "doi chung hong: qua it vet bang sinh ra");
            Kiem(Mathf.Abs(vetMax[0] - vetMin[0]) < 0.01f && Mathf.Abs(vetMax[1] - vetMin[1]) < 0.01f,
                 "cac vet trong cung mot con bao khong cung mot con so - phep do nay vo nghia");
            Kiem(vetMin[0] > 1f && Mathf.Abs(vetMin[1] / vetMin[0] - 2f) < 0.01f, "Mua bang trong tang hinh: khong phai MOI vet deu x2");
            // KHONG kiem ti le nay: no dem tong mau bia mat, ma so vet TRUNG bia va cho trung moi con bao
            // moi khac (sat thuong vung giam dan tu tam ra ria) - do duoc 1,35 va 1,73 o hai lan chay lien
            // nhau cua CUNG mot ban code. Phan tren da do SO GHI TREN TUNG VET va ra dung 2,00 - chat che hon.

            // --- K2. QUA CAU BANG cap 5: 5 qua ---
            // Doc SAT THUONG GHI TREN TUNG QUA (khong dem mau bia mat): so qua TRUNG duoc bia va cho
            // trung moi lan mot khac, nen tong mau mat ra 1,72 - 1,87 tuy lan chay du moi qua deu x2.
            var tongCau = new float[2];
            var soQuaK2 = new int[2];
            var minQua = new float[2];
            var maxQua = new float[2];
            for (int lan = 0; lan < 2; lan++)
            {
                if (lan == 1) TangHinh.Bat(mauToi, TangHinh.ThoiGian);
                var bia = TaoBiaTo("TAM_K2_" + lan, toi.transform.position + hK * 7f, 2.5f);
                yield return new WaitForFixedUpdate();
                float m0 = bia.health;
                toi.mana = toi.maxMana;
                toi.CastAt(CapDo.KyQuaCauBang, bia.transform.position);

                // Doc ngay khi chum vua bay ra, truoc khi qua nao kip no
                minQua[lan] = float.MaxValue; maxQua[lan] = 0f;
                float hanQ = Time.time + 1.5f;
                while (soQuaK2[lan] == 0 && Time.time < hanQ)
                {
                    yield return null;
                    foreach (var q in Object.FindObjectsByType<QuaCauBang>(FindObjectsSortMode.None))
                    {
                        soQuaK2[lan]++;
                        minQua[lan] = Mathf.Min(minQua[lan], q.impactDamage);
                        maxQua[lan] = Mathf.Max(maxQua[lan], q.impactDamage);
                    }
                }
                yield return new WaitForSeconds(1.0f);
                // ⚠️ DON TANG BANG truoc khi dem: tu 19/09/2026 Qua cau bang cap 5 de lai tang bang, het
                // gio thi NO them 100 - mot con so CO DINH, khong di qua he so nhan doi cua Tang hinh.
                // De no no trong cua so dem thi ti le ra 1,87 thay vi 2,00 va phep thu bao hong oan
                // (muc nay do "moi qua trong chum co x2 khong", khong do vu no cua tang bang).
                foreach (var tb in Object.FindObjectsByType<TangBangNo>(FindObjectsSortMode.None))
                    Object.DestroyImmediate(tb);
                yield return new WaitForSeconds(1.5f);
                tongCau[lan] = m0 - bia.health;
                Object.Destroy(bia.gameObject);
                var tx = toi.GetComponent<TangHinh>(); if (tx != null) tx.Tat();
                yield return new WaitForSeconds(0.8f);
            }
            Ghi(string.Format("K2. QUA CAU BANG cap {0}: sat thuong GHI TREN TUNG QUA - binh thuong {1} qua, {2:F2}..{3:F2}; dang tang hinh {4} qua, {5:F2}..{6:F2} -> x{7:F2} (mong 2,00; neu chi qua dau x2 thi qua thu hai tro di van la {2:F2})",
                CapDo.CapCuaKyNang(CapDo.KyQuaCauBang), soQuaK2[0], minQua[0], maxQua[0], soQuaK2[1], minQua[1], maxQua[1],
                minQua[0] > 0f ? minQua[1] / minQua[0] : 0f));
            Ghi(string.Format("K2b. (tham khao) tong mau bia mat: binh thuong {0:F1}, tang hinh {1:F1} - KHONG kiem so nay vi so qua trung bia moi lan mot khac",
                tongCau[0], tongCau[1]));
            Kiem(soQuaK2[0] >= 2 && soQuaK2[0] == soQuaK2[1], "hai lan tung ra so qua khac nhau - phep do vo nghia");
            Kiem(Mathf.Abs(maxQua[0] - minQua[0]) < 0.01f && Mathf.Abs(maxQua[1] - minQua[1]) < 0.01f,
                 "cac qua trong cung mot chum khong cung mot con so - phep do nay vo nghia");
            Kiem(minQua[0] > 1f && Mathf.Abs(minQua[1] / minQua[0] - 2f) < 0.01f,
                 "Qua cau bang trong tang hinh: khong phai MOI qua deu x2");
        }

        // ================= H. HET GIO TU TAN =================
        Ghi("");
        {
            TangHinh.Bat(mauToi, 1.2f);
            float batDau = Time.time;
            while (toi.GetComponent<TangHinh>() != null && Time.time < batDau + 4f) yield return null;
            float song = Time.time - batDau;
            Ghi(string.Format("H. dat 1,2 giay -> tu tan sau {0:F2} giay", song));
            Kiem(Mathf.Abs(song - 1.2f) < 0.15f, "tang hinh khong tu tan dung han");
        }

        // ================= V. VONG PHEP LUC HET TANG HINH =================
        Ghi("");
        {
            var tc0 = toi.GetComponent<TangHinh>(); if (tc0 != null) tc0.Tat();
            yield return new WaitForSeconds(1.8f);
            Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;
            Vector3 tamV = toi.transform.position;
            float[] mauMax = { 600f, 1000f, 300f };
            float[] xa = { 2f, 4f, 4.8f };
            var biaV = new List<Damageable>();
            for (int i = 0; i < 3; i++)
            {
                Vector3 h = Quaternion.AngleAxis(i * 110f + 20f, Vector3.up) * huong;
                var b = TaoBia("TAM_VongPhep" + i, tamV + h * xa[i]);
                b.maxHealth = mauMax[i]; b.health = mauMax[i] * 0.5f;       // mau HIEN TAI chi mot nua -> tach "toi da" / "hien tai"
                biaV.Add(b);
            }
            var biaNgoai = TaoBia("TAM_VongPhepNgoai", tamV + ngang * 5.6f);
            biaNgoai.maxHealth = 800f; biaNgoai.health = 800f;
            yield return null;
            var truoc = new float[3]; for (int i = 0; i < 3; i++) truoc[i] = biaV[i].health;
            float truocNgoai = biaNgoai.health, truocToi = mauToi.health;
            int vong0 = TangHinh.SoLanNoVong, hinh0 = VfxFactory.SoVongPhepDaBat;
            // Cap 1: bat bang tay (khong qua CastAt) - cap mac dinh 1
            var tv = TangHinh.Bat(mauToi, 30f);
            int capV1 = tv != null ? tv.capKyNang : -1;
            yield return new WaitForSeconds(0.3f);
            // Ket thuc DONG BO roi doc mau ngay trong CUNG khung: cho het gio thi quai / chay trong canh chen don vao nguoi tung
            // (lan chay thu hai: nguoi tung mat 22 trong luc cho). Duong "tu het gio" do o phan cap 5 ben duoi.
            for (int i = 0; i < 3; i++) truoc[i] = biaV[i].health;
            truocNgoai = biaNgoai.health; truocToi = mauToi.health;
            if (tv != null) tv.Tat();
            float matToiNgay = truocToi - mauToi.health;
            var matNgay = new float[3]; for (int i = 0; i < 3; i++) matNgay[i] = truoc[i] - biaV[i].health;
            float matNgoaiNgay = truocNgoai - biaNgoai.health;
            yield return new WaitForSeconds(0.3f);             // qua pha hien + phong to 0,18 s
            // Hinh: tam vong phep that, khung bao luoi (vong ngoai trong anh o 0,40 cua o) va do sang
            GameObject vongGo = null;
            foreach (var t in Object.FindObjectsByType<VongPhepSang>(FindObjectsInactive.Exclude)) vongGo = t.gameObject;
            float banKinhVongThat = -1f, doSang = -1f, lechDatMax = -1f; int soDinhVong = 0;
            if (vongGo != null)
            {
                // Luoi bam dat dung yen (xoay / phong to lam tren uv): canh = khung luoi theo X, vong ngoai = 0,40 / 0,5 nua canh.
                // Va luoi phai BAM DAT: moi dinh cach mat dat that (tia CHI lop Ground, doc lap voi code dung luoi) ~6 cm
                var lv = vongGo.GetComponent<MeshFilter>().sharedMesh;
                banKinhVongThat = lv.bounds.size.x * 0.5f * (VfxFactory.BanKinhVongTrongAnh / 0.5f) * vongGo.GetComponent<VongPhepSang>().HeSoTo;
                var dv = lv.vertices; lechDatMax = 0f; int soDinhDo = 0;
                for (int i = 0; i < dv.Length; i++)
                {
                    Vector3 wv = vongGo.transform.TransformPoint(dv[i]);
                    if (Physics.Raycast(wv + Vector3.up * 3f, Vector3.down, out RaycastHit hd, 8f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
                    { lechDatMax = Mathf.Max(lechDatMax, Mathf.Abs(wv.y - hd.point.y - VfxFactory.NangVongPhep)); soDinhDo++; }
                }
                soDinhVong = soDinhDo;
                doSang = vongGo.GetComponent<VongPhepSang>().DoSang;
            }
            var camV = Camera.main; var rigV = camV != null ? camV.GetComponentInParent<CameraRig>() : null;
            if (rigV != null) rigV.enabled = false;
            if (camV != null) { camV.transform.position = tamV - huong * 9f + Vector3.up * 7f; camV.transform.LookAt(tamV); }
            yield return null;
            yield return Chup("tanghinh_5_vong_phep");
            if (rigV != null) rigV.enabled = true;
            var sb = new StringBuilder(); int dung = 0;
            for (int i = 0; i < 3; i++)
            {
                float mat = matNgay[i], mong = 100f + 0.10f * mauMax[i];
                sb.Append(string.Format("bia {0} m (toi da {1}, hien tai {2}): mat {3:F2} / mong {4:F2}; ", xa[i], mauMax[i], truoc[i], mat, mong));
                if (Mathf.Abs(mat - mong) < 0.01f) dung++;
            }
            float matNgoai = matNgoaiNgay, matToi = matToiNgay;
            Ghi("V. cap " + capV1 + ": " + sb);
            Ghi(string.Format("V. bia NGOAI 5,6 m mat {0:F2}; nguoi tung mat {1:F2}; vong no {2} lan, hinh {3}; vong ngoai cua anh ve that ban kinh {4:F2} m (mong 5); do sang ngay sau {5:F2}",
                matNgoai, matToi, TangHinh.SoLanNoVong - vong0, VfxFactory.SoVongPhepDaBat - hinh0, banKinhVongThat, doSang));
            Kiem(capV1 == 1 && dung == 3, "sat thuong vong phep khong dung 100 + 10% MAU TOI DA");
            Kiem(Mathf.Abs(matNgoai) < 0.01f && Mathf.Abs(matToi) < 0.01f, "vong phep danh ca bia ngoai 5 m / danh chinh minh");
            Kiem(TangHinh.SoLanNoVong - vong0 == 1 && VfxFactory.SoVongPhepDaBat - hinh0 == 1 && vongGo != null, "het tang hinh khong no dung MOT vong phep");
            Kiem(Mathf.Abs(banKinhVongThat - TangHinh.BanKinhVong) < 0.3f && doSang > 0.5f, "hinh vong phep sai co / khong sang");
            Ghi(string.Format("V. luoi vong phep BAM DAT: {0} dinh do duoc, lech khoi (mat dat that + 6 cm) toi da {1:F3} m", soDinhVong, lechDatMax));
            Kiem(soDinhVong > 400 && lechDatMax < 0.02f, "vong phep khong bam mat dat (dat go ghe se che mat vong)");

            // Hoi chieu bat dau dem khi het tang hinh (tang hinh bat bang tay khong dat hoi chieu -> dung CastAt o duoi)
            // Cap 5 TUNG THAT: cap di vao TangHinh qua Release
            CapDo.ThemDiemChoPhepThu(4);
            for (int i = 0; i < 4; i++) CapDo.NangCap(K);
            int capNay = CapDo.CapCuaKyNang(K);
            // 70% (khong phai 50%): cap 5 danh 100 + 26% - bia 300 mau con 150 thi CHET giua phep do (lan dau: mat 150, bia bi xoa,
            // ngoai le lam chet coroutine, Play ket)
            foreach (var b in biaV) b.health = b.maxHealth * 0.7f;
            for (int i = 0; i < 3; i++) truoc[i] = biaV[i].health;
            yield return new WaitForSeconds(10.5f);          // qua hoi chieu cua lan tung that o muc B
            toi.mana = toi.maxMana;
            toi.CastAt(K, toi.transform.position);
            yield return new WaitForSeconds(0.6f);
            var t5 = toi.GetComponent<TangHinh>();
            int capT5 = t5 != null ? t5.capKyNang : -1;
            if (t5 != null) t5.conLai = 0.05f;
            for (float h = Time.time + 2f; Time.time < h && toi.GetComponent<TangHinh>() != null; ) yield return null;
            float hoi0 = toi.HoiChieuGiay(K);
            yield return new WaitForSeconds(1.0f);
            float hoi1 = toi.HoiChieuGiay(K);
            var sb5 = new StringBuilder(); int dung5 = 0;
            for (int i = 0; i < 3; i++)
            {
                float mat = truoc[i] - biaV[i].health, mong = 100f + 0.26f * mauMax[i];
                sb5.Append(string.Format("mat {0:F2} / mong {1:F2}; ", mat, mong));
                if (Mathf.Abs(mat - mong) < 0.01f) dung5++;
            }
            Ghi(string.Format("V. cap {0} tung THAT (TangHinh.capKyNang {1}): {2}", capNay, capT5, sb5));
            Ghi(string.Format("V. het tang hinh -> hoi chieu ngay luc het {0:F2} s, 1 giay sau {1:F2} s (mong 10 roi dem xuong)", hoi0, hoi1));
            Kiem(capNay == 5 && capT5 == 5 && dung5 == 3, "cap 5 khong ra 100 + 26% mau toi da");
            Kiem(hoi0 > 9.8f && hoi0 <= 10.01f && Mathf.Abs((hoi0 - hoi1) - 1f) < 0.1f, "het tang hinh ma hoi chieu 10 giay khong bat dau dem");

            // Tan do DON DAU (Qua cau lua) cung no vong
            yield return new WaitForSeconds(9.5f);
            CapDo.MoCaDuongChoPhepThu(0);
            toi.mana = toi.maxMana;
            toi.CastAt(K, toi.transform.position);
            yield return new WaitForSeconds(0.6f);
            int vongD0 = TangHinh.SoLanNoVong;
            toi.mana = toi.maxMana;
            toi.CastAt(0, toi.transform.position + huong * 8f);
            for (float h = Time.time + 2f; Time.time < h && toi.GetComponent<TangHinh>() != null; ) yield return null;
            yield return new WaitForSeconds(0.2f);
            Ghi(string.Format("V. tan do don dau (Qua cau lua): con tang hinh {0}, vong no them {1}", toi.GetComponent<TangHinh>() != null, TangHinh.SoLanNoVong - vongD0));
            Kiem(toi.GetComponent<TangHinh>() == null && TangHinh.SoLanNoVong - vongD0 == 1, "tan do don dau ma khong no vong phep");

            foreach (var b in biaV) if (b != null) Object.Destroy(b.gameObject);
            if (biaNgoai != null) Object.Destroy(biaNgoai.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        // ================= I. QUA MANG =================
        Ghi("");
        TranHienTai.DangChoiMang = true;
        var vuong = Vector3.Cross(Vector3.up, huong).normalized;
        var kia = NguoiChoiKhac.Sinh("uid-tanghinh", "Người ẩn", toi.transform.position + vuong * 6f);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi kia"); loi++; }
        else
        {
            var goDb = new GameObject("TAM_DongBoTangHinh");
            var db = goDb.AddComponent<DongBoTran>();
            db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
            KenhTrucTiep.Tao(); KenhTrucTiep.NhanTraLoi("{}");
            var chay = Object.FindAnyObjectByType<ChayThuMang>();
            if (chay != null) chay.StartCoroutine(GiuSong());
            var daGui = new List<string>();
            KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
            yield return new WaitForSeconds(0.6f);

            // I1. minh tang hinh -> goi trang thai mang bit
            daGui.Clear();
            TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            yield return new WaitForSeconds(0.6f);
            bool coBit = false; byte coDoc = 0;
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s);
                if (b == null || GoiTin.LoaiCuaGoi(b) != GoiTin.LoaiTrangThai) continue;
                var ds = new GoiTin.MotNguoi[8];
                int moc;
                int soN = GoiTin.DocTrangThai(b, ds, out moc);
                for (int i = 0; i < soN && i < ds.Length; i++) if ((ds[i].coHieuUng & HieuUngQuaMang.CoTangHinh) != 0) { coBit = true; coDoc = ds[i].coHieuUng; }
            }

            // I2. ban sao nhan bit -> co TangHinh; dung yen thi tat renderer, di chuyen thi bat
            var mauKia = kia.GetComponent<Damageable>();
            var dsG = new GoiTin.MotNguoi[1];
            System.Action<byte, Vector3> nhet = (co, vt) =>
            {
                dsG[0] = new GoiTin.MotNguoi { chiSo = 1, viTri = vt, gocY = 0f, mau01 = 1f, coHieuUng = co };
                KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), dsG, 1)));
            };
            Vector3 cho = kia.transform.position;
            for (int i = 0; i < 12; i++) { nhet(HieuUngQuaMang.CoTangHinh, cho); yield return new WaitForSeconds(0.05f); }
            bool banSaoCo = kia.GetComponent<TangHinh>() != null;
            // dung yen 0,8 giay -> renderer tat
            float hanD = Time.time + 0.9f;
            while (Time.time < hanD) { nhet(HieuUngQuaMang.CoTangHinh, cho); yield return null; }
            int hienKhiDung = 0;
            foreach (var r in kia.GetComponentsInChildren<Renderer>(true)) if (r.enabled && (r is SkinnedMeshRenderer || r is MeshRenderer)) hienKhiDung++;
            // cho di chuyen -> hien lai
            float hanM = Time.time + 0.6f;
            while (Time.time < hanM)
            {
                cho += vuong * 4f * Time.deltaTime;
                nhet(HieuUngQuaMang.CoTangHinh, cho);
                yield return null;
            }
            int hienKhiDi = 0;
            foreach (var r in kia.GetComponentsInChildren<Renderer>(true)) if (r.enabled && (r is SkinnedMeshRenderer || r is MeshRenderer)) hienKhiDi++;
            // I3. Ban sao cua NGUOI KHAC khong bao gio nhap nhay (nguoi dung: dau hieu sap hien hinh chi MINH thay)
            var tKia = kia.GetComponent<TangHinh>();
            bool kiaNhapNhay = false;
            if (tKia != null)
            {
                tKia.conLai = 1f;                  // vao dung khoang "hai giay cuoi"
                for (int i = 0; i < 20; i++) { if (tKia.dangNhapNhay) kiaNhapNhay = true; yield return null; }
            }

            Ghi(string.Format("I. minh tang hinh -> goi trang thai co bit CoTangHinh: {0} (byte co = {1}); ban sao nhan bit: co trang thai {2}, renderer hien khi DUNG YEN {3}, khi DI CHUYEN {4}; ban sao nhap nhay {5} (phai la False)",
                coBit, coDoc, banSaoCo, hienKhiDung, hienKhiDi, kiaNhapNhay));
            Kiem(!kiaNhapNhay, "ban sao cua nguoi khac cung nhap nhay - nguoi dung chi cho MINH thay dau hieu nay");
            Kiem(coBit, "goi trang thai khong mang bit tang hinh");
            Kiem(banSaoCo, "ban sao khong nhan duoc trang thai tang hinh");
            Kiem(hienKhiDung == 0 && hienKhiDi > 0, "ban sao khong theo luat dung yen bien mat / di chuyen hien duong net");
            // ===== L. CO "DON DAU TANG HINH" DI QUA GOI TIN =====
            // Sat thuong x2 phai giong nhau tren MOI may: goi phep mang theo co nay (bit cao cua byte cap ky nang).
            {
                var tkia0 = kia.GetComponent<TangHinh>(); if (tkia0 != null) tkia0.Tat();
                yield return new WaitForSeconds(0.3f);

                // L1. Goi tin: viet roi doc lai
                GoiTin.MotPhep doc1, doc2;
                bool ok1 = GoiTin.DocKyNang(GoiTin.VietKyNang(new GoiTin.MotPhep { chiSo = 1, kyNang = 0, capKyNang = 5, soThuTu = 7, diemNgam = Vector3.zero }), out doc1);
                bool ok2 = GoiTin.DocKyNang(GoiTin.VietKyNang(new GoiTin.MotPhep { chiSo = 1, kyNang = 0, capKyNang = 5, soThuTu = 8, diemNgam = Vector3.zero, donTangHinh = true }), out doc2);
                Ghi(string.Format("L1. goi phep viet-doc: khong co co -> cap {0} don {1}; co co -> cap {2} don {3}",
                    doc1.capKyNang, doc1.donTangHinh, doc2.capKyNang, doc2.donTangHinh));
                Kiem(ok1 && ok2 && doc1.capKyNang == 5 && !doc1.donTangHinh && doc2.capKyNang == 5 && doc2.donTangHinh,
                     "co don tang hinh khong qua duoc goi tin (hoac lam hong cap ky nang)");

                // L2. Ban sao phat lai phep theo goi: co co thi sat thuong x2 tren may NAY
                Vector3 hL = HuongTrong(toi);
                var tongL = new float[2];
                var donL = new float[2];
                var soQuaL = new int[2];
                int stt = 100;
                for (int lan = 0; lan < 2; lan++)
                {
                    Vector3 choBiaL = kia.transform.position + hL * 7f;
                    var biaL = TaoBiaTo("TAM_L" + lan, choBiaL, 2.5f);
                    yield return new WaitForFixedUpdate();
                    float m0 = biaL.health;
                    var goiL = GoiTin.VietKyNang(new GoiTin.MotPhep
                    {
                        chiSo = 1, kyNang = 0, capKyNang = 1, soThuTu = ++stt,
                        diemNgam = biaL.transform.position, donTangHinh = lan == 1
                    });
                    // Xoa sach qua cau dang bay truoc khi phat lai goi: lan chay dau bat phai qua cau lua
                    // CUA QUAI (16,80 sat thuong) va ti le ra 0,20.
                    foreach (var fx in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None)) Object.DestroyImmediate(fx.gameObject);
                    KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(goiL));
                    // Doc SO GHI TREN TUNG QUA chu khong dem mau bia mat: chum ba qua, so qua trung bia
                    // va cho trung moi lan mot khac nen tong mau mat ra 1,89 du moi qua deu x2.
                    donL[lan] = 0f; soQuaL[lan] = 0;
                    float hanL = Time.time + 1.5f;
                    while (soQuaL[lan] == 0 && Time.time < hanL)
                    {
                        yield return null;
                        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None))
                        {
                            // CHI qua cua ban sao "kia": quai trong canh cung ban qua cau lua (16,80 sat
                            // thuong) va lan chay truoc phep do vo phai chung, ra ti le 0,20 roi 10,12.
                            if (f.boQua != mauKia) continue;
                            soQuaL[lan]++;
                            donL[lan] = Mathf.Max(donL[lan], f.impactDamage);
                        }
                    }
                    yield return new WaitForSeconds(2.5f);
                    tongL[lan] = m0 - biaL.health;
                    Object.Destroy(biaL.gameObject);
                    yield return new WaitForSeconds(0.6f);
                }
                Ghi(string.Format("L2. nguoi KIA tung Qua cau lua qua mang: sat thuong GHI TREN QUA - goi khong co co {0:F2} ({1} qua), goi co co don tang hinh {2:F2} ({3} qua) -> x{4:F2} (mong 2,00)",
                    donL[0], soQuaL[0], donL[1], soQuaL[1], donL[0] > 0f ? donL[1] / donL[0] : 0f));
                Ghi(string.Format("L2b. (tham khao) tong mau bia mat: {0:F1} va {1:F1} - KHONG kiem so nay vi so qua trung bia moi lan mot khac",
                    tongL[0], tongL[1]));
                Kiem(soQuaL[0] > 0 && soQuaL[0] == soQuaL[1], "hai lan phat lai ra so qua khac nhau - phep do vo nghia");
                Kiem(donL[0] > 1f && Mathf.Abs(donL[1] / donL[0] - 2f) < 0.01f, "goi phep co co ma ban sao khong danh x2");

                // L3. Khi MINH danh don dau trong tang hinh, goi gui di phai co co
                daGui.Clear();
                TangHinh.Bat(mauToi, TangHinh.ThoiGian);
                toi.mana = toi.maxMana;
                toi.transform.rotation = Quaternion.LookRotation(hL);
                toi.CastAt(0, toi.transform.position + hL * 8f);
                yield return new WaitForSeconds(0.6f);
                bool guiCoCo = false, guiCoGoiPhep = false;
                foreach (var g0 in daGui)
                {
                    var b0 = GoiTin.TuChuoi(g0);
                    if (b0 == null || GoiTin.LoaiCuaGoi(b0) != GoiTin.LoaiKyNang) continue;
                    GoiTin.MotPhep pp;
                    if (!GoiTin.DocKyNang(b0, out pp)) continue;
                    guiCoGoiPhep = true;
                    if (pp.donTangHinh) guiCoCo = true;
                }
                Ghi(string.Format("L3. minh danh don dau trong tang hinh -> goi phep gui di: co goi {0}, mang co don tang hinh {1}", guiCoGoiPhep, guiCoCo));
                Kiem(guiCoGoiPhep && guiCoCo, "don dau cua minh khong bao sang may kia (ben kia se tinh sat thuong thuong)");
                var txx = toi.GetComponent<TangHinh>(); if (txx != null) txx.Tat();
            }

            var tkia = kia.GetComponent<TangHinh>(); if (tkia != null) tkia.Tat();

            Object.DestroyImmediate(goDb);

            if (kia != null) Object.Destroy(kia.gameObject);
        }
        // ================= W. VONG PHEP CUA BAN SAO =================
        Ghi("");
            // W. BAN SAO MOI TINH (ban sao cua muc I con mang goi phep Tang hinh gia lap cu - bi bat lai 90 giay giua chung, lan chay 4): het tang hinh -> vong no tren may nay; bit cu tre sau do KHONG tao lai tang hinh (khong vong thu hai)
            {
                var kiaW = NguoiChoiKhac.Sinh("uid-tanghinh-w", "Người W", toi.transform.position + vuong * 6f);
                var dk = kiaW != null ? kiaW.GetComponent<Damageable>() : null;
                if (dk == null) { Ghi("[LOI] W: khong sinh duoc ban sao moi"); loi++; }
                else {   // (khong yield break: bo qua Ket thi Play ket mai)
                // TAT MOI bo dong bo tran con song (ngoai goDb con bo tu gan khi DangChoiMang): chung ap lai co 'dang tang hinh'
                // cua goi cuoi moi goi -> tang hinh ban sao khong bao gio het (lan chay 2 va 3 bao sai). W tu dong vai goi tin.
                var dbTat = new List<DongBoTran>();
                foreach (var dbW in Object.FindObjectsByType<DongBoTran>(FindObjectsInactive.Exclude)) if (dbW.enabled) { dbW.enabled = false; dbTat.Add(dbW); }
                var tkCu = kiaW.GetComponent<TangHinh>(); if (tkCu != null) tkCu.Tat();
                yield return new WaitForSeconds(TangHinh.GiayBoQuaBitCu + 0.2f);   // qua khoang bo qua bit cu cua lan Tat o tren
                int w0 = TangHinh.SoLanNoVong;
                TangHinh.ApTuMang(dk, 0.3f);
                bool coTH = kiaW.GetComponent<TangHinh>() != null;
                for (float h = Time.time + 2f; Time.time < h && kiaW.GetComponent<TangHinh>() != null; ) yield return null;
                int w1 = TangHinh.SoLanNoVong;
                TangHinh.ApTuMang(dk, 0.3f);                     // goi tin tre van mang bit "dang tang hinh"
                bool taoLai = kiaW.GetComponent<TangHinh>() != null;
                yield return new WaitForSeconds(0.6f);
                int w2 = TangHinh.SoLanNoVong;
                Ghi(string.Format("W. ban sao: nhan bit -> tang hinh {0}; het -> vong no {1}; bit cu tre ngay sau -> tao lai tang hinh {2}, vong them {3}", coTH, w1 - w0, taoLai, w2 - w1));
                Kiem(coTH && w1 - w0 == 1, "ban sao het tang hinh ma khong no vong phep tren may nay");
                Kiem(!taoLai && w2 == w1, "bit tang hinh cu lam ban sao no vong phep thu hai");
                foreach (var dbW in dbTat) if (dbW != null) dbW.enabled = true;
                }
                if (kiaW != null) Object.Destroy(kiaW.gameObject);
            }
        TranHienTai.DangChoiMang = false;

        var tCuoi = toi.GetComponent<TangHinh>(); if (tCuoi != null) tCuoi.Tat();
        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude)) if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
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
        Debug.Log("[TangHinh] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/tanghinh.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_TangHinh", "TAM_DongBoTangHinh" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
