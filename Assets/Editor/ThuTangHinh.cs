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
        Kiem(K == 12 && CapDo.SoKyNang == 13, "so hieu / so ky nang sai");
        Kiem(Mathf.Approximately(toi.tangHinhCost, 30f) && Mathf.Approximately(nl, 30f), "nang luong khong phai 30");
        Kiem(Mathf.Approximately(toi.tangHinhCooldown, 30f) && Mathf.Approximately(hc, 30f), "hoi chieu khong phai 30 giay");
        Kiem(Mathf.Approximately(TangHinh.ThoiGian, 20f) && Mathf.Approximately(TangHinh.HeSoToc, 1.2f) && Mathf.Approximately(TangHinh.NhanDonDau, 2f),
             "hang so tang hinh sai (20 s / 1,2 / x2)");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Tang hinh");
        Kiem(SachPhep.Ten(K) == "TÀNG HÌNH" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Tang hinh");

        // ================= B + C. TUNG THAT VA HINH =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        int soPhep = 0, kyVua = -1;
        System.Action<int, Vector3> dem = (s, a) => { soPhep++; kyVua = s; };
        toi.DaTungPhep += dem;
        CapDo.BatDauTranMoi();
        int s0 = soPhep;
        toi.CastAt(K, toi.transform.position);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoKhoa(K);
        yield return new WaitForSeconds(0.3f);
        int shaderTruoc = DemShaderTangHinh(toi.gameObject);
        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana;
        toi.CastAt(K, toi.transform.position);
        float manaTon = manaTruoc - toi.mana;      // doc NGAY: doi 0,6 giay thi nang luong da hoi lai mot it (lan dau do ra 24,5)
        yield return new WaitForSeconds(0.6f);
        var tg = toi.GetComponent<TangHinh>();
        float hoiNgaySau = toi.HoiChieuGiay(K);
        int shaderSau = DemShaderTangHinh(toi.gameObject);
        yield return Chup("tanghinh_1_dang_tang_hinh");
        Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> tung {1}, co trang thai TangHinh {2} (con {3:F1} s), ton {4} nang luong, hoi chieu ngay sau {5:F1} s",
            tuChoiKhoa, soPhep == s0 + 1 && kyVua == K, tg != null, tg != null ? tg.conLai : -1f, manaTon, hoiNgaySau));
        Ghi(string.Format("C. renderer dung shader tang hinh: truoc khi bat {0}, sau khi bat {1}", shaderTruoc, shaderSau));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(tg != null && tg.conLai > 18f, "tung xong khong vao trang thai tang hinh 20 giay");
        Kiem(Mathf.Abs(manaTon - 30f) < 0.01f, "khong ton dung 30 nang luong");
        Kiem(hoiNgaySau > 28f, "hoi chieu khong phai 30 giay");
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
            CapDo.MoKhoa(0);
            Vector3 h2 = HuongTrong(toi);
            toi.transform.rotation = Quaternion.LookRotation(h2);
            Vector3 choBia = toi.transform.position + h2 * 8f;

            // 1) don thuong
            var b1 = TaoBia("TAM_G1", choBia);
            yield return new WaitForFixedUpdate();
            float m1 = b1.health;
            toi.mana = toi.maxMana;
            toi.CastAt(0, b1.transform.position);
            float han1 = Time.time + 3f;
            while (Time.time < han1 && b1.health >= m1) yield return null;
            yield return new WaitForSeconds(0.2f);
            float donThuong = m1 - b1.health;
            Object.Destroy(b1.gameObject);
            yield return new WaitForSeconds(1.2f);

            // 2) don dau khi dang tang hinh
            TangHinh.Bat(mauToi, TangHinh.ThoiGian);
            var b2 = TaoBia("TAM_G2", choBia);
            yield return new WaitForFixedUpdate();
            float m2 = b2.health;
            toi.mana = toi.maxMana;
            int donDau0 = TangHinh.SoLanDonDau;
            toi.CastAt(0, b2.transform.position);
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
            CapDo.MoKhoa(5);
            toi.mana = toi.maxMana;
            toi.CastAt(5, toi.transform.position);
            yield return new WaitForSeconds(0.8f);
            bool conSauKhien = toi.GetComponent<TangHinh>() != null;
            var t5 = toi.GetComponent<TangHinh>(); if (t5 != null) t5.Tat();

            Ghi(string.Format("G. (mo khoa Qua cau lua: {6}) mot chum Qua cau lua vao bia: binh thuong mat {0:F1}, khi dang tang hinh mat {1:F1} -> x{2:F2} (mong 2,00); bo dem don dau +{3}; sau don do con tang hinh {4}; tung KHIEN thi con tang hinh {5}",
                donThuong, donTang, donThuong > 0 ? donTang / donThuong : 0f, demDonDau, conTangHinh, conSauKhien, CapDo.DaMo(0)));
            Kiem(donThuong > 1f && Mathf.Abs(donTang / donThuong - 2f) < 0.06f, "don dau khong gap doi");
            Kiem(demDonDau == 1 && !conTangHinh, "don dau khong lam tan tang hinh");
            Kiem(conSauKhien, "tung Khien (khong gay sat thuong) ma tang hinh lai tan");
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
            Ghi(string.Format("I. minh tang hinh -> goi trang thai co bit CoTangHinh: {0} (byte co = {1}); ban sao nhan bit: co trang thai {2}, renderer hien khi DUNG YEN {3}, khi DI CHUYEN {4}",
                coBit, coDoc, banSaoCo, hienKhiDung, hienKhiDi));
            Kiem(coBit, "goi trang thai khong mang bit tang hinh");
            Kiem(banSaoCo, "ban sao khong nhan duoc trang thai tang hinh");
            Kiem(hienKhiDung == 0 && hienKhiDi > 0, "ban sao khong theo luat dung yen bien mat / di chuyen hien duong net");
            var tkia = kia.GetComponent<TangHinh>(); if (tkia != null) tkia.Tat();
            Object.DestroyImmediate(goDb);
            if (kia != null) Object.Destroy(kia.gameObject);
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
