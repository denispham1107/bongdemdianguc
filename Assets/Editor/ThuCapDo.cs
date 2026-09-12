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
/// nguoi choi khac thi duoc kinh nghiem, toi da cap 10. Moi cap mau +15%,
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
            + ", toc do x" + CapDo.HeSoTocTheoCap(10).ToString("0.000"));
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
        int soMo = 0;
        for (int k = 0; k < CapDo.SoKyNang; k++) if (CapDo.DaMo(k)) soMo++;
        Ghi("C1. vao tran: cap " + CapDo.Cap + ", diem " + CapDo.DiemKyNang
            + ", so ky nang da mo " + soMo);
        Kiem(CapDo.Cap == 1 && CapDo.DiemKyNang == 1 && soMo == 0,
             "vao tran khong dung: phai la cap 1, mot diem, khong ky nang nao mo");

        bool mo1 = CapDo.MoKhoa(2);
        bool mo2 = CapDo.MoKhoa(3);        // het diem roi, phai truot
        Ghi("C2. mo ky nang 2 -> " + mo1 + " (phai True); mo tiep ky nang 3 -> " + mo2 + " (phai False)");
        Kiem(mo1 && !mo2, "mo khoa khong tru dung diem");
        Kiem(CapDo.CapCuaKyNang(2) == 1, "mo khoa xong ky nang khong o cap 1");

        // Len cap roi nang cap ky nang toi da
        CapDo.Them(999999);
        int truoc = CapDo.DiemKyNang;
        int lanNang = 0;
        while (CapDo.NangCap(2)) lanNang++;
        Ghi("C3. sau khi len cap 10 con " + truoc + " diem; nang ky nang 2 duoc " + lanNang
            + " lan -> cap " + CapDo.CapCuaKyNang(2) + " (toi da " + CapDo.CapKyNangToiDa + ")");
        Kiem(CapDo.CapCuaKyNang(2) == CapDo.CapKyNangToiDa, "khong nang duoc toi cap toi da");
        Kiem(!CapDo.NangCapDuoc(2), "da toi da ma van nang duoc nua");

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

        Ghi("");
        Ghi("D. trong tran dau");

        // ---- D1. VAO TRAN LA CAP 1, MOI KY NANG DEU KHOA ----
        CapDo.BatDauTranMoi();
        yield return null;
        int soMo = 0;
        for (int k = 0; k < CapDo.SoKyNang; k++) if (CapDo.DaMo(k)) soMo++;
        Ghi("D1. vao tran: cap " + CapDo.Cap + ", mau toi da " + mau.maxHealth.ToString("F0")
            + ", nang luong toi da " + pc.maxMana.ToString("F0")
            + ", toc do " + pc.moveSpeed.ToString("F2") + ", ky nang da mo " + soMo);
        Kiem(CapDo.Cap == 1 && soMo == 0, "vao tran khong phai cap 1 / khong khoa het ky nang");

        // ---- D2. CHUA MO THI KHONG TUNG DUOC ----
        int soPhep = 0;
        System.Action<int, Vector3> dem = (a, v) => soPhep++;
        pc.DaTungPhep += dem;

        pc.CastAt(1, pc.transform.position + pc.transform.forward * 6f);
        yield return null;
        Ghi("D2. bam Mua bang khi chua mo -> phep bay ra " + soPhep + ", loi nhac: " + pc.LastMessage);
        Kiem(soPhep == 0, "ky nang chua mo ma van tung duoc");

        // ---- D3. MO KHOA XONG THI TUNG DUOC ----
        CapDo.MoKhoa(1);
        soPhep = 0;
        pc.CastAt(1, pc.transform.position + pc.transform.forward * 6f);
        yield return null;
        Ghi("D3. mo khoa Mua bang -> phep bay ra " + soPhep);
        Kiem(soPhep == 1, "mo khoa roi van khong tung duoc");

        // Doc suc manh cua con bao vua tung (ky nang cap 1)
        yield return new WaitForSeconds(0.9f);
        var bao1 = Object.FindAnyObjectByType<IceStorm>();
        float satCap1 = bao1 != null ? bao1.shardDamage : 0f;
        float dongCap1 = bao1 != null ? bao1.freezeSeconds : 0f;
        if (bao1 != null) Object.DestroyImmediate(bao1.gameObject);
        foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None))
            Object.DestroyImmediate(fs.gameObject);

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
        CapDo.Them(999999);                     // len thang cap 10 de co du diem
        while (CapDo.NangCapDuoc(1)) CapDo.NangCap(1);
        yield return null;

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

        pc.DaTungPhep -= dem;
        CapDo.BatDauTranMoi();

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
