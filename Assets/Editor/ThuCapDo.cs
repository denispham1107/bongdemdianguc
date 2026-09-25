using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: CAP DO, KINH NGHIEM, DIEM KY NANG.
///
/// Nguoi dung chot (13/09/2026): vao tran ai cung cap 1, giet quai va giet
/// nguoi choi khac thi duoc kinh nghiem, toi da cap 20 (truoc 13/09/2026 la 10). Moi cap mau +15%,
/// nang luong +10%, toc do +3,5%. Bay ky nang deu khoa, cap 1 co mot diem de
/// mo mot cai, moi lan len cap them mot diem. Ky nang toi da cap 5: moi cap
/// +20% sat thuong, +10% nang luong, hieu ung +0,15 giay; rieng Khien +15%
/// mau khien.
///
/// Do bang SO:
///   A. Bang kinh nghiem va cong don (ham thuan).
///   B. He so chi so va he so ky nang.
///   C. Diem ky nang: mo khoa, nang cap, het diem.
///   D. Trong Play: chi so THAT cua nhan vat truoc/sau khi len cap; ky nang
///      chua mo thi khong tung duoc; nang cap xong thi phep manh len that.
///   E. Goi tin: cap ky nang va kinh nghiem qua mang.
///   F. DIEU KIEN MO KHOA THEO BAC (nguoi dung 19/09/2026): tam luat moi nhom he - Qua cau lua cap 2
///      moi mo duoc Thien thach, Thien thach cap 5 moi mo duoc Lua dia nguc, y het cho Bang / Set / Phong.
///      Do THAT: du diem ma chua du cap thi MoKhoa TRUOT; len du cap thi mo duoc; DOI CHUNG ky nang
///      khong co dieu kien (Khien, Giut set, bon Khang) mo duoc ngay. Sach phep phai noi ro thieu gi.
///   G. Binh mau 200 / binh mana 75 (nguoi dung 19/09/2026): uong THAT tren nhan vat.
///
/// Ket qua ghi ra <c>PlayTestShots/capdo.txt</c>.
/// </summary>
public static class ThuCapDo
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/60. Chay thu CAP DO (kinh nghiem, diem ky nang)", false, 151)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu cap do",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] cap do, kinh nghiem, diem ky nang");

        DoBangKinhNghiem();
        DoHeSo();
        DoDiemKyNang();
        DoGoiTin();

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
        if (GameObject.Find("TAM_CapDo") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_CapDo");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[CapDo] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    // ================================================================
    //  A. BANG KINH NGHIEM
    // ================================================================
    static void DoBangKinhNghiem()
    {
        Ghi("");
        Ghi("A. bang kinh nghiem");

        CapDo.BatDauTranMoi();
        var sb = new StringBuilder();
        int tong = 0;
        for (int c = 1; c < CapDo.CapToiDa; c++)
        {
            int can = CapDo.CanDeLenCap(c);
            tong += can;
            sb.Append(c).Append("->").Append(c + 1).Append(": ").Append(can).Append("  ");
        }
        Ghi("A1. " + sb.ToString().Trim());
        Ghi("A2. tong de tu cap 1 len cap " + CapDo.CapToiDa + ": " + tong + " kinh nghiem");
        // Bang phai du CapToiDa - 1 bac, bac nao cung duong va khong nho hon bac truoc
        // (bang thieu mot bac thi CanDeLenCap nem IndexOutOfRange ngay giua tran)
        bool bangTot = true;
        for (int c = 1; c < CapDo.CapToiDa; c++)
            if (CapDo.CanDeLenCap(c) <= 0 || (c > 1 && CapDo.CanDeLenCap(c) < CapDo.CanDeLenCap(c - 1))) bangTot = false;
        Kiem(CapDo.CapToiDa == 20, "cap toi da phai la 20, dang la " + CapDo.CapToiDa);
        Kiem(bangTot, "bang kinh nghiem thieu bac, co bac <= 0 hoac bac sau nho hon bac truoc");
        Kiem(CapDo.CanDeLenCap(CapDo.CapToiDa) == 0, "cap toi da van con doi kinh nghiem");

        // Cong dung mot bac
        CapDo.BatDauTranMoi();
        CapDo.Them(CapDo.CanDeLenCap(1) - 1);
        bool chuaLen = CapDo.Cap == 1;
        CapDo.Them(1);
        Ghi("A3. thieu 1 diem -> cap " + (chuaLen ? 1 : 9) + "; du 1 diem nua -> cap " + CapDo.Cap
            + ", kinh nghiem du lai " + CapDo.KinhNghiem);
        Kiem(chuaLen, "chua du ma da len cap");
        Kiem(CapDo.Cap == 2 && CapDo.KinhNghiem == 0, "du kinh nghiem ma khong len cap");

        // Nhay nhieu bac mot luc, va giu phan thua
        CapDo.BatDauTranMoi();
        CapDo.Them(500);
        Ghi("A4. cong mot phat 500 -> cap " + CapDo.Cap + ", con du " + CapDo.KinhNghiem
            + " (100+135+180 = 415 cho cap 4, du 85)");
        Kiem(CapDo.Cap == 4 && CapDo.KinhNghiem == 85, "cong mot phat nhieu bac ra sai");

        // Toi da thi dung han
        CapDo.BatDauTranMoi();
        CapDo.Them(999999);
        Ghi("A5. cong 999999 -> cap " + CapDo.Cap + ", diem ky nang " + CapDo.DiemKyNang);
        Kiem(CapDo.Cap == CapDo.CapToiDa, "khong len duoc toi cap toi da");
        Kiem(CapDo.DiemKyNang == CapDo.CapToiDa, "so diem ky nang ca tran khong bang so cap");

        // Kinh nghiem tung loai quai
        Ghi("A6. quai: bo xuong " + CapDo.KnCuaQuai(MonsterType.Skeleton)
            + ", phu thuy " + CapDo.KnCuaQuai(MonsterType.Witch)
            + ", quy cay " + CapDo.KnCuaQuai(MonsterType.QuyCay)
            + ", quy du " + CapDo.KnCuaQuai(MonsterType.QuyDu)
            + ", quy khong lo " + CapDo.KnCuaQuai(MonsterType.Brute)
            + " | nguoi choi " + CapDo.KnGietNguoi);
        // Kinh nghiem giet quai x1,62 (26/09/2026: +35% roi +20% tren muc do): so voi bang GOC chep tay (khong doc lai ham trong code)
        {
            var loaiK = new[] { MonsterType.Skeleton, MonsterType.Ghoul, MonsterType.Fallen, MonsterType.QuyCay, MonsterType.Witch, MonsterType.QuyDu, MonsterType.Brute };
            int[] gocK = { 18, 20, 22, 30, 32, 40, 70 };
            var sbK = new System.Text.StringBuilder(); int dungK = 0; int tongGoc = 0, tongMoi = 0;
            for (int i = 0; i < loaiK.Length; i++)
            {
                int moi = CapDo.KnCuaQuai(loaiK[i]);
                tongGoc += gocK[i]; tongMoi += moi;
                sbK.Append(loaiK[i] + " " + gocK[i] + "->" + moi + "; ");
                if (moi == Mathf.FloorToInt(gocK[i] * 1.62f + 0.5f)) dungK++;
            }
            Ghi("A6b. kinh nghiem quai x1,62 (1,35 x 1,2): " + sbK + "tong " + tongGoc + " -> " + tongMoi + " (x" + (tongMoi / (float)tongGoc).ToString("F3") + ")");
            Kiem(dungK == loaiK.Length && Mathf.Abs(tongMoi / (float)tongGoc - 1.62f) < 0.01f, "kinh nghiem quai khong phai x1,62 so voi goc");
            Kiem(CapDo.KnGietNguoi > 0, "kinh nghiem giet nguoi choi mat");
        }
        int motDot = CapDo.KnCuaQuai(MonsterType.Skeleton) + CapDo.KnCuaQuai(MonsterType.Witch)
                   + CapDo.KnCuaQuai(MonsterType.QuyCay) + CapDo.KnCuaQuai(MonsterType.QuyDu);
        Ghi("    mot dot Act2 (4 con quanh minh) = " + motDot + " kinh nghiem"
            + " -> du len cap 2 ngay dot dau: " + (motDot >= CapDo.CanDeLenCap(1)));
        Kiem(motDot >= CapDo.CanDeLenCap(1), "dot dau khong du len noi cap 2 - nhip len cap qua cham");
    }

    // ================================================================
    //  B. HE SO
    // ================================================================
    static void DoHeSo()
    {
        Ghi("");
        Ghi("B. he so theo cap");

        Ghi("B1. nhan vat cap 10: mau x" + CapDo.HeSoMauTheoCap(10).ToString("0.000")
            + ", nang luong x" + CapDo.HeSoManaTheoCap(10).ToString("0.000")
            + ", toc do x" + CapDo.HeSoTocTheoCap(10).ToString("0.000")
            + " | cap " + CapDo.CapToiDa + ": mau x" + CapDo.HeSoMauTheoCap(CapDo.CapToiDa).ToString("0.000")
            + ", nang luong x" + CapDo.HeSoManaTheoCap(CapDo.CapToiDa).ToString("0.000")
            + ", toc do x" + CapDo.HeSoTocTheoCap(CapDo.CapToiDa).ToString("0.000"));
        Kiem(Mathf.Abs(CapDo.HeSoMauTheoCap(20) - Mathf.Pow(1.15f, 19)) < 0.01f, "he so mau cap 20 sai");
        // Toc do dung o cap 10 (nguoi dung chot 13/09/2026); viet thang so 1,035^9, khong lay tu CapDo
        Kiem(Mathf.Abs(CapDo.HeSoTocTheoCap(10) - Mathf.Pow(1.035f, 9)) < 0.001f, "he so toc do cap 10 sai");
        Kiem(Mathf.Abs(CapDo.HeSoTocTheoCap(20) - Mathf.Pow(1.035f, 9)) < 0.001f, "toc do van tang sau cap 10");
        Kiem(Mathf.Abs(CapDo.HeSoTocTheoCap(9) - Mathf.Pow(1.035f, 8)) < 0.001f, "toc do cap 9 sai (chan som qua)");
        Kiem(Mathf.Abs(CapDo.HeSoMauTheoCap(10) - Mathf.Pow(1.15f, 9)) < 0.001f, "he so mau sai");
        Kiem(Mathf.Abs(CapDo.HeSoMauTheoCap(1) - 1f) < 0.0001f, "cap 1 da duoc cong chi so");

        Ghi("B2. ky nang cap 5: sat thuong x" + CapDo.SatThuongTheoCap(5).ToString("0.000")
            + ", nang luong x" + CapDo.ManaTheoCap(5).ToString("0.000")
            + ", hieu ung +" + CapDo.ThemGiayHieuUngTheoCap(5).ToString("0.00") + " giay"
            + ", mau khien x" + CapDo.MauKhiengTheoCap(5).ToString("0.000"));
        Kiem(Mathf.Abs(CapDo.ThemGiayHieuUngTheoCap(5) - 0.60f) < 0.0001f, "hieu ung khong dai them 0,15 giay moi cap");
        Kiem(Mathf.Abs(CapDo.SatThuongTheoCap(1) - 1f) < 0.0001f, "ky nang cap 1 da duoc nhan them");
    }

    // ================================================================
    //  C. DIEM KY NANG
    // ================================================================
    static void DoDiemKyNang()
    {
        Ghi("");
        Ghi("C. diem ky nang: mo khoa va nang cap");

        CapDo.BatDauTranMoi();
        // 25/09/2026: hai ky nang BINH co san cap 1 (nguoi dung) - moi ky nang khac con khoa
        int soMo = 0, soMoKhacBinh = 0;
        for (int k = 0; k < CapDo.SoKyNang; k++) if (CapDo.DaMo(k)) { soMo++; if (!CapDo.LaKyBinh(k)) soMoKhacBinh++; }
        Ghi("C1. vao tran: cap " + CapDo.Cap + ", diem " + CapDo.DiemKyNang
            + ", so ky nang da mo " + soMo + " (binh mau cap " + CapDo.CapCuaKyNang(CapDo.KyBinhMau)
            + ", binh mana cap " + CapDo.CapCuaKyNang(CapDo.KyBinhMana) + ")");
        Kiem(CapDo.Cap == 1 && CapDo.DiemKyNang == 1 && soMoKhacBinh == 0 && soMo == 2,
             "vao tran khong dung: phai la cap 1, mot diem, chi hai binh mo san");

        // Dung hai ky nang KHONG co dieu kien bac (Qua cau lua, Giut set): tu 19/09/2026 Sam set
        // doi Giut set cap 2 va Loc xoay doi Gio loc cap 2, lay chung thi truot vi dieu kien chu
        // khong phai vi het diem - muc nay dang do CHUYEN TRU DIEM.
        bool mo1 = CapDo.MoKhoa(0);
        bool mo2 = CapDo.MoKhoa(6);        // het diem roi, phai truot
        Ghi("C2. mo ky nang 0 -> " + mo1 + " (phai True); mo tiep ky nang 6 -> " + mo2 + " (phai False)");
        Kiem(mo1 && !mo2, "mo khoa khong tru dung diem");
        Kiem(CapDo.CapCuaKyNang(0) == 1, "mo khoa xong ky nang khong o cap 1");

        // Len cap roi nang cap ky nang toi da
        CapDo.Them(999999);
        int truoc = CapDo.DiemKyNang;
        int lanNang = 0;
        // Ky nang 0 (Qua cau lua) - vua mo o C2 va KHONG co dieu kien bac nao. Truoc day cho nay
        // dung ky nang 2 (Sam set), tu 19/09/2026 no bi khoa sau Giut set cap 2 nen nang khong duoc.
        while (CapDo.NangCap(0)) lanNang++;
        Ghi("C3. sau khi len cap " + CapDo.Cap + " con " + truoc + " diem; nang ky nang 0 duoc " + lanNang
            + " lan -> cap " + CapDo.CapCuaKyNang(0) + " (toi da " + CapDo.CapKyNangToiDa + ")");
        Kiem(CapDo.CapCuaKyNang(0) == CapDo.CapKyNangToiDa, "khong nang duoc toi cap toi da");
        Kiem(!CapDo.NangCapDuoc(0), "da toi da ma van nang duoc nua");

        CapDo.BatDauTranMoi();
    }

    // ================================================================
    //  E. GOI TIN
    // ================================================================
    static void DoGoiTin()
    {
        Ghi("");
        Ghi("E. goi tin mang");

        var p = new GoiTin.MotPhep
        {
            chiSo = 2, kyNang = 4, capKyNang = 5,
            soThuTu = 77, diemNgam = new Vector3(12.5f, 1f, -8.25f), doTreMs = 120
        };
        var b = GoiTin.VietKyNang(p);
        GoiTin.MotPhep ra;
        bool ok = GoiTin.DocKyNang(b, out ra);
        Ghi("E1. goi ky nang " + b.Length + " byte: cap " + p.capKyNang + " -> " + ra.capKyNang
            + ", ky nang " + ra.kyNang + ", ghe " + ra.chiSo + ", tre " + ra.doTreMs + " ms");
        Kiem(ok && ra.capKyNang == 5 && ra.kyNang == 4 && ra.chiSo == 2 && ra.doTreMs == 120,
             "goi ky nang khong giu duoc cap cua nguoi tung");

        var bk = GoiTin.VietKinhNghiem(3, 4096);
        byte ghe; int diem;
        bool ok2 = GoiTin.DocKinhNghiem(bk, out ghe, out diem);
        Ghi("E2. goi kinh nghiem " + bk.Length + " byte: ghe " + ghe + ", diem " + diem);
        Kiem(ok2 && ghe == 3 && diem == 4096, "goi kinh nghiem sai");
    }

    // ================================================================
    //  D. TRONG PLAY
    // ================================================================
    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            return pc;
        }
        return null;
    }

    /// <summary>
    /// Nang mot ky nang toi dung cap, tu them diem neu thieu.
    ///
    /// ⚠️ CO TRAN. Ban dau cho nay la "while (cap < mong) NangCap(...)" khong tran: het diem la
    /// NangCap luon tra false, vong quay vo tan NGAY TRONG MOT KHUNG HINH va Unity treo cung -
    /// khong ngoai le, khong log, nguoi dung phai tat Editor bang tay (19/09/2026).
    /// </summary>
    static void NangToiCap(int ky, int cap)
    {
        for (int i = 0; i < 60 && CapDo.CapCuaKyNang(ky) < cap; i++)
        {
            if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            if (!CapDo.DaMo(ky)) { if (!CapDo.MoKhoa(ky)) break; }
            else if (!CapDo.NangCap(ky)) break;
        }
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }
        yield return new WaitForSeconds(1.5f);

        var pc = TimToi();
        if (pc == null) { Loi("khong tim thay nguoi choi"); Ket(); yield break; }
        var mau = pc.GetComponent<Damageable>();

        // Tat GameDirector suot phep thu: dot quai sinh giua chung vua lam nang GPU vua lam nhieu
        // so do (bai hoc 19/09/2026 - GPU timeout tung tat han Unity).
        bool dirBatCu = dir.enabled;
        dir.enabled = false;

        Ghi("");
        Ghi("D. trong tran dau");

        // ---- D1. VAO TRAN LA CAP 1, MOI KY NANG DEU KHOA ----
        CapDo.BatDauTranMoi();
        yield return null;
        int soMo = 0;
        for (int k = 0; k < CapDo.SoKyNang; k++) if (CapDo.DaMo(k) && !CapDo.LaKyBinh(k)) soMo++;   // binh co san cap 1
        Ghi("D1. vao tran: cap " + CapDo.Cap + ", mau toi da " + mau.maxHealth.ToString("F0")
            + ", nang luong toi da " + pc.maxMana.ToString("F0")
            + ", toc do " + pc.moveSpeed.ToString("F2") + ", ky nang da mo (tru hai binh co san) " + soMo);
        Kiem(CapDo.Cap == 1 && soMo == 0, "vao tran khong phai cap 1 / khong khoa het ky nang");

        // ---- D2. CHUA MO THI KHONG TUNG DUOC ----
        int soPhep = 0;
        System.Action<int, Vector3, bool> dem = (a, v, d) => soPhep++;
        pc.DaTungPhep += dem;

        pc.CastAt(CapDo.KyQuaCauBang, pc.transform.position + pc.transform.forward * 6f);
        yield return null;
        Ghi("D2. bam Qua cau bang khi chua mo -> phep bay ra " + soPhep + ", loi nhac: " + pc.LastMessage);
        Kiem(soPhep == 0, "ky nang chua mo ma van tung duoc");

        // ---- D3. MO KHOA XONG THI TUNG DUOC ----
        // ⚠️ Dung mot ky nang KHONG co dieu kien bac va chi ton DUNG mot diem co san: MoCaDuongChoPhepThu
        // tu them kinh nghiem de co diem, nhan vat se nhay thang len cap 20 va muc D4 ben duoi
        // ("len cap 2, chi so phai tang 15%") khong con gi de do.
        CapDo.MoKhoa(CapDo.KyQuaCauBang);
        soPhep = 0;
        pc.CastAt(CapDo.KyQuaCauBang, pc.transform.position + pc.transform.forward * 6f);
        yield return null;
        Ghi("D3. mo khoa Qua cau bang -> phep bay ra " + soPhep + " (cap nhan vat van " + CapDo.Cap + ")");
        Kiem(soPhep == 1, "mo khoa roi van khong tung duoc");
        Kiem(CapDo.Cap == 1, "mo khoa ma nhan vat len cap - muc D4 se do rong");

        yield return new WaitForSeconds(0.9f);
        foreach (var qb in Object.FindObjectsByType<QuaCauBang>(FindObjectsSortMode.None))
            Object.DestroyImmediate(qb.gameObject);
        // Suc manh Mua bang cap 1 do o duoi (sau D4b) - Mua bang gio bi khoa sau Qua cau bang cap 2,
        // ma mo ca duong toi no thi nhan vat len cap, lam hong phep do "len cap 2" cua D4.
        float satCap1 = 0f, dongCap1 = 0f;

        // ---- D4. LEN CAP: CHI SO THAT PHAI TANG ----
        float mauTruoc = mau.maxHealth, manaTruoc = pc.maxMana, tocTruoc = pc.moveSpeed;
        CapDo.Them(CapDo.CanDeLenCap(1));
        yield return null;
        Ghi("D4. len cap 2: mau " + mauTruoc.ToString("F0") + " -> " + mau.maxHealth.ToString("F0")
            + " (x" + (mau.maxHealth / mauTruoc).ToString("0.000") + ")"
            + ", nang luong " + manaTruoc.ToString("F0") + " -> " + pc.maxMana.ToString("F0")
            + " (x" + (pc.maxMana / manaTruoc).ToString("0.000") + ")"
            + ", toc do " + tocTruoc.ToString("F2") + " -> " + pc.moveSpeed.ToString("F2")
            + " (x" + (pc.moveSpeed / tocTruoc).ToString("0.000") + ")");
        Kiem(Mathf.Abs(mau.maxHealth / mauTruoc - 1.15f) < 0.002f, "mau khong tang dung 15%");
        Kiem(Mathf.Abs(pc.maxMana / manaTruoc - 1.10f) < 0.002f, "nang luong khong tang dung 10%");
        Kiem(Mathf.Abs(pc.moveSpeed / tocTruoc - 1.035f) < 0.002f, "toc do khong tang dung 3,5%");
        Kiem(CapDo.DiemKyNang >= 1, "len cap ma khong duoc them diem ky nang");

        // ---- D5. NANG CAP KY NANG: PHEP MANH LEN THAT ----
        // Mo ca duong toi Mua bang (no doi Qua cau bang cap 2 tu 19/09/2026) roi tung o CAP 1
        // de do suc manh goc - lam o day chu khong o D3, vi mo ca duong se lam nhan vat len cap.
        CapDo.MoCaDuongChoPhepThu(1);
        pc.mana = pc.maxMana;
        pc.CastAt(1, pc.transform.position + pc.transform.forward * 6f);
        yield return new WaitForSeconds(0.9f);
        var baoGoc = Object.FindAnyObjectByType<IceStorm>();
        satCap1 = baoGoc != null ? baoGoc.shardDamage : 0f;
        dongCap1 = baoGoc != null ? baoGoc.freezeSeconds : 0f;
        if (baoGoc != null) Object.DestroyImmediate(baoGoc.gameObject);
        foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None))
            Object.DestroyImmediate(fs.gameObject);

        CapDo.Them(999999);                     // len thang cap toi da de co du diem
        while (CapDo.NangCapDuoc(1)) CapDo.NangCap(1);
        yield return null;

        // ---- D4b. CAP TOI DA TREN NHAN VAT THAT: toc do dung o cap 10, mau van tang du ----
        // So voi chi so luc CAP 1 (truoc D4). Viet thang so mu, khong goi CapDo.
        Ghi("D4b. len cap " + CapDo.Cap + ": toc do " + tocTruoc.ToString("F2") + " -> " + pc.moveSpeed.ToString("F2")
            + " (x" + (pc.moveSpeed / tocTruoc).ToString("0.000") + ", mong x" + Mathf.Pow(1.035f, 9).ToString("0.000") + " = dung o cap 10)"
            + ", mau x" + (mau.maxHealth / mauTruoc).ToString("0.00") + " (mong x" + Mathf.Pow(1.15f, CapDo.Cap - 1).ToString("0.00") + ")");
        Kiem(Mathf.Abs(pc.moveSpeed / tocTruoc - Mathf.Pow(1.035f, 9)) < 0.005f, "toc do nhan vat that khong dung o cap 10");
        Kiem(Mathf.Abs(mau.maxHealth / mauTruoc / Mathf.Pow(1.15f, CapDo.Cap - 1) - 1f) < 0.005f, "mau khong tang du toi cap toi da");

        // PHAI DOI HET HOI CHIEU: Mua bang hoi 6 giay. Lan do dau tien toi tung
        // lai sau 0,9 giay - phep bi tu choi, khong co con bao nao trong canh,
        // va phep do bao "sat thuong x0,000" nhu the code hong.
        //
        // Va phai CHO MAU THAT LON trong luc cho: dung giua Act2 6,6 giay la du
        // cho dan quai vay quanh giet chet - nguoi chet thi khong niem chu duoc
        // nua, khong co con bao nao, va phep do lai bao "sat thuong x0,000".
        float mauGoc = mau.maxHealth, mauDangCo = mau.health;
        mau.maxHealth = 10000000f; mau.health = 10000000f;
        yield return new WaitForSeconds(6.6f);

        pc.mana = pc.maxMana;
        soPhep = 0;
        pc.CastAt(1, pc.transform.position + pc.transform.forward * 6f);
        yield return null;
        if (soPhep != 1) Loi("khong tung duoc Mua bang lan hai (hoi chieu chua xong?)");
        yield return new WaitForSeconds(0.9f);
        var bao5 = Object.FindAnyObjectByType<IceStorm>();
        float satCap5 = bao5 != null ? bao5.shardDamage : 0f;
        float dongCap5 = bao5 != null ? bao5.freezeSeconds : 0f;
        if (bao5 != null) Object.DestroyImmediate(bao5.gameObject);
        foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None))
            Object.DestroyImmediate(fs.gameObject);

        mau.maxHealth = mauGoc; mau.health = Mathf.Min(mauGoc, mauDangCo);

        float tiLe = satCap1 > 0f ? satCap5 / satCap1 : 0f;
        Ghi("    (nhan vat con song: " + (!mau.IsDead) + ")");
        Ghi("D5. Mua bang cap " + CapDo.CapCuaKyNang(1) + ": sat thuong moi tang "
            + satCap1.ToString("F2") + " -> " + satCap5.ToString("F2") + " (x" + tiLe.ToString("0.000")
            + ", mong doi x" + CapDo.SatThuongTheoCap(5).ToString("0.000") + ")"
            + " | dong cung " + dongCap1.ToString("F2") + "s -> " + dongCap5.ToString("F2") + "s");
        Kiem(CapDo.CapCuaKyNang(1) == CapDo.CapKyNangToiDa, "khong nang duoc ky nang len cap 5");
        Kiem(Mathf.Abs(tiLe - CapDo.SatThuongTheoCap(5)) < 0.01f, "sat thuong khong tang dung theo cap");
        Kiem(Mathf.Abs((dongCap5 - dongCap1) - 0.60f) < 0.01f, "hieu ung khong dai them 0,60 giay o cap 5");

        // ---- D6. GIET QUAI DUOC KINH NGHIEM ----
        CapDo.BatDauTranMoi();
        CapDo.MoKhoa(0);
        yield return null;

        Damageable conQuai = null;
        MonsterType loaiQuai = MonsterType.Skeleton;
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            var m = n.GetComponent<Damageable>();
            if (m == null || m.IsDead) continue;
            conQuai = m; loaiQuai = n.loai; break;
        }

        if (conQuai == null) Ghi("D6. khong co quai nao dang song de thu - bo qua");
        else
        {
            int knTruoc = CapDo.KinhNghiem;
            conQuai.GhiKeDanh(mau);
            conQuai.TakeDamage(conQuai.maxHealth * 10f, DamageType.Fire, conQuai.transform.position);
            yield return new WaitForSeconds(0.4f);
            int duoc = CapDo.KinhNghiem - knTruoc;
            Ghi("D6. giet mot con " + loaiQuai + " -> duoc " + duoc + " kinh nghiem (bang gia: "
                + CapDo.KnCuaQuai(loaiQuai) + ")");
            Kiem(duoc == CapDo.KnCuaQuai(loaiQuai), "giet quai khong duoc dung so kinh nghiem");
        }

        // ================= F. DIEU KIEN MO KHOA THEO BAC =================
        Ghi("");
        {
            // {ky nang, ky nang can truoc, cap can}
            int[][] luat = {
                new[] { 4, 0, 2 }, new[] { CapDo.KyLuaDiaNguc, 4, 5 },
                new[] { 1, CapDo.KyQuaCauBang, 2 }, new[] { CapDo.KyTangHinh, 1, 5 },
                new[] { 2, 6, 2 }, new[] { CapDo.KyCauDien, 2, 5 },
                new[] { 3, CapDo.KyGioLoc, 2 }, new[] { CapDo.KyHoaLocXoay, 3, 5 },
            };
            bool tatCaDung = true;
            for (int i = 0; i < luat.Length; i++)
            {
                int ky = luat[i][0], can = luat[i][1], capCan = luat[i][2];
                CapDo.BatDauTranMoi();
                CapDo.Them(999999);                       // du diem, chi thieu moi DIEU KIEN

                // 1) chua co ky nang truoc -> truot
                bool truotKhiChuaCo = !CapDo.MoKhoa(ky);
                string nhac = SachPhep.NhacDieuKien(ky);

                // 2) co ky nang truoc nhung THIEU MOT CAP -> van truot
                //    ⚠️ Ban than "can" cung co the bi khoa sau mot ky nang khac (Thien thach doi
                //    Qua cau lua cap 2...), nen phai mo CA DUONG toi no truoc - lan chay dau
                //    19/09/2026 quen cho nay, bon luat "cap 5" bao loi oan.
                CapDo.MoCaDuongChoPhepThu(can);
                NangToiCap(can, capCan - 1);
                bool truotKhiThieuCap = !CapDo.MoKhoa(ky);
                int capDangCo = CapDo.CapCuaKyNang(can);

                // 3) len du cap -> mo duoc
                NangToiCap(can, capCan);
                bool moDuoc = CapDo.MoKhoa(ky);

                bool ok = truotKhiChuaCo && truotKhiThieuCap && moDuoc && nhac != null;
                if (!ok) tatCaDung = false;
                Ghi(string.Format("F. {0} <- {1} cap {2}: chua co -> truot {3}; {1} cap {4} -> truot {5}; du cap {2} -> mo duoc {6}",
                    SachPhep.Ten(ky), SachPhep.Ten(can), capCan, truotKhiChuaCo, capDangCo, truotKhiThieuCap, moDuoc));
                if (i == 0) Ghi("    (Sach phep nhac) " + nhac);
            }
            Kiem(tatCaDung, "tam luat mo khoa theo bac khong chay dung");

            // DOI CHUNG: ky nang KHONG co dieu kien thi mo duoc ngay
            CapDo.BatDauTranMoi();
            CapDo.Them(999999);
            bool khien = CapDo.MoKhoa(5);
            bool giutSet = CapDo.MoKhoa(6);
            bool cauLua = CapDo.MoKhoa(0);
            bool cauBang = CapDo.MoKhoa(CapDo.KyQuaCauBang);
            bool gioLoc = CapDo.MoKhoa(CapDo.KyGioLoc);
            bool khangLua = CapDo.MoKhoa(CapDo.KyKhangLua);
            bool tocBien = CapDo.MoKhoa(CapDo.KyTocBien);
            Ghi(string.Format("F. DOI CHUNG ky nang khong dieu kien mo ngay: Khien {0}, Giựt sét {1}, Cầu lửa {2}, Cầu băng {3}, Gió lốc {4}, Kháng Lửa {5}, Tốc biến {6}",
                khien, giutSet, cauLua, cauBang, gioLoc, khangLua, tocBien));
            Kiem(khien && giutSet && cauLua && cauBang && gioLoc && khangLua && tocBien,
                 "ky nang khong co dieu kien ma cung bi chan");
            Kiem(SachPhep.NhacDieuKien(0) == null && SachPhep.NhacDieuKien(5) == null,
                 "ky nang khong dieu kien ma van co cau nhac");
        }

        // ================= G. BINH MAU 200 / BINH MANA 75 =================
        Ghi("");
        {
            CapDo.BatDauTranMoi();
            CapDo.Them(999999);
            CapDo.MoKhoa(CapDo.KyBinhMau);
            CapDo.MoKhoa(CapDo.KyBinhMana);
            CapDo.ThemBinh(CapDo.KyBinhMau);
            CapDo.ThemBinh(CapDo.KyBinhMana);
            yield return null;

            var mauG = pc.GetComponent<Damageable>();
            mauG.maxHealth = 1000f; mauG.health = 100f;
            pc.maxMana = 500f; pc.mana = 100f;
            yield return null;

            float mauTruocG = mauG.health;
            pc.CastAt(CapDo.KyBinhMau, pc.transform.position);
            yield return null;
            float hoiMau = mauG.health - mauTruocG;

            yield return new WaitForSeconds(0.6f);          // qua hoi chieu binh
            // ⚠️ Do mana NGAY TRUOC khi uong. Do tu truoc luc cho 0,6 giay thi phan mana HOI TU NHIEN
            // trong lúc cho cung bi tinh vao binh - lan chay dau 19/09/2026 ra 81 thay vi 75.
            float manaTruocG = pc.mana;
            pc.CastAt(CapDo.KyBinhMana, pc.transform.position);
            yield return null;
            float hoiMana = pc.mana - manaTruocG;

            Ghi(string.Format("G. uong binh THAT: hoi {0:F0} mau (hang MauMoiBinh {1:F0}), hoi {2:F0} nang luong (hang ManaMoiBinh {3:F0})",
                hoiMau, PlayerController.MauMoiBinh, hoiMana, PlayerController.ManaMoiBinh));
            Kiem(Mathf.Abs(PlayerController.MauMoiBinh - 200f) < 0.01f, "hang binh mau khong phai 200");
            Kiem(Mathf.Abs(PlayerController.ManaMoiBinh - 75f) < 0.01f, "hang binh mana khong phai 75");
            Kiem(Mathf.Abs(hoiMau - 200f) < 0.5f, "uong binh mau khong hoi dung 200");
            Kiem(Mathf.Abs(hoiMana - 75f) < 0.5f, "uong binh mana khong hoi dung 75");
        }

        pc.DaTungPhep -= dem;
        CapDo.BatDauTranMoi();
        dir.enabled = dirBatCu;

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
        Debug.Log("[CapDo] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/capdo.txt", bao.ToString());
        var rac = GameObject.Find("TAM_CapDo");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
