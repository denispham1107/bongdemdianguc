using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: MUA BANG + SAM SET (kich thuoc, lam cham, dong cung, choang).
///
/// Nguoi dung xin (12/09/2026):
///   - Tang bang roi to them 15%, cum bang duoi dat to them 10%.
///   - Tang bang TRUNG ke dich: chac chan cham 50% trong 2 giay, va 35% so lan
///     dong cung hoan toan 1,5 giay (khong di, khong dung skill).
///   - Tia set: 35% lam ke dich choang.
///   - Mua bang cua chinh minh KHONG duoc uu tien dap xuong dau minh.
///   - Va loi nang nhat: "danh vao nguoi choi khac chi hien len hieu ung hinh
///     anh chu khong lam ho dung yen" - PlayerController truoc day khong he doc
///     FrozenEffect / StunnedEffect, chi EnemyAI doc.
///
/// Do bang SO:
///   A. Kich thuoc - do HINH HOC THAT cua vat sinh ra luc chay, doi chieu voi
///      moc cu lay tu git (ban truoc khi sua).
///   B. Tham so di dung duong: prefab -> IceStorm -> FallingShard.
///   C. Xac suat tren 1000 lan gieo: dong cung ~35%, cham 100%, choang ~35%.
///   D. NGUOI CHOI bi dong cung / choang thi dung yen va khong tung duoc phep -
///      co MAU DOI CHUNG (luc khong bi gi) de biet phep do con nhin thay gi.
///   E. Mua bang khong nham vao chinh nguoi tung.
///
/// Ket qua ghi ra <c>PlayTestShots/bang_set.txt</c>.
/// </summary>
public static class ThuBangSet
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    /// <summary>Tong localScale cua 8 cum bang trong Vfx_NoBang.prefab TRUOC khi
    /// sua - lay bang "git show HEAD:Assets/Prefabs/Vfx_NoBang.prefab" ngay
    /// truoc lan sua nay. La moc lich su, khong phai cong thuc dang nghi ngo.</summary>
    const float TongCumBangCu = 9.172859f;

    /// <summary>Dai co cua TANG bang truoc khi sua (lay tu git, VfxFactory).</summary>
    const float TangCuMin = 1.04f, TangCuMax = 2.08f;

    const int SoMauGieo = 1000;
    const int SoTangDo = 600;

    [MenuItem("Diablo 2.5D/58. Chay thu MUA BANG + SAM SET (dong bang, choang)", false, 149)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu bang / set",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] mua bang + sam set: kich thuoc, lam cham, dong cung, choang");

        // A2 lam NGAY, khong can vao Play: doc thang file prefab
        DoCumBangTrongPrefab();

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
        if (GameObject.Find("TAM_BangSet") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_BangSet");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BangSet] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    // ================================================================
    //  A2. CUM BANG DUOI DAT - doc thang trong prefab
    // ================================================================
    static void DoCumBangTrongPrefab()
    {
        Ghi("");
        Ghi("A2. cum bang duoi dat (doc thang Vfx_NoBang.prefab)");

        var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vfx_NoBang.prefab");
        if (pf == null) { Loi("khong tim thay Vfx_NoBang.prefab"); return; }

        float tong = 0f; int dem = 0;
        foreach (var t in pf.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("CumGai")) continue;
            tong += t.localScale.x; dem++;
        }
        float tiLe = TongCumBangCu > 0f ? tong / TongCumBangCu : 0f;
        Ghi("    " + dem + " cum, tong co = " + tong.ToString("F4")
            + " (truoc khi sua: " + TongCumBangCu.ToString("F4") + ") -> to them "
            + ((tiLe - 1f) * 100f).ToString("F1") + "%");
        Kiem(dem == 8, "so cum bang trong prefab khong con la 8");
        Kiem(Mathf.Abs(tiLe - 1.10f) < 0.005f, "cum bang khong to them dung 10%");
    }

    // ================================================================
    //  Bia do don: mot vat co mau, nam tren lop Enemy
    // ================================================================
    static Damageable TaoBia(string ten, Vector3 p)
    {
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 10000000f; d.health = 10000000f;
        return d;
    }

    static void XoaHieuUng(Damageable d)
    {
        var f = d.GetComponent<FrozenEffect>();
        if (f != null) Object.DestroyImmediate(f);
        var s = d.GetComponent<StunnedEffect>();
        if (s != null) Object.DestroyImmediate(s);
    }

    /// <summary>Nguoi choi CUA MAY NAY (khong phai ban sao).</summary>
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

    static float quangDuongDo;

    /// <summary>Day can ve mot huong trong may giay roi do quang duong NGANG di duoc.</summary>
    static IEnumerator DoDiChuyen(PlayerController pc, float giay, Vector3 huong)
    {
        var g = GoiInput.Rong(0f);
        g.huongDi = huong.normalized;
        pc.tuDocInput = false;
        pc.input = g;

        Vector3 dau = pc.transform.position;
        float t0 = Time.time;
        while (Time.time - t0 < giay) yield return null;
        Vector3 cuoi = pc.transform.position;

        pc.input = GoiInput.Rong(0f);
        quangDuongDo = Vector2.Distance(new Vector2(dau.x, dau.z), new Vector2(cuoi.x, cuoi.z));
    }

    static int tongTangDo, vaoToiDo, vaoKeDichDo;

    /// <summary>Theo doi tung tang bang con bao tha ra: no nham vao ai.</summary>
    static IEnumerator DemTangRoi(IceStorm bao2, Vector3 tam, PlayerController pc, float giay)
    {
        var daThay = new HashSet<FallingShard>();
        tongTangDo = 0; vaoToiDo = 0; vaoKeDichDo = 0;
        float han = Time.time + giay;
        while (Time.time < han)
        {
            foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None))
            {
                if (!daThay.Add(fs)) continue;
                tongTangDo++;
                var t2 = new Vector2(fs.target.x, fs.target.z);
                if (Vector2.Distance(t2, new Vector2(pc.transform.position.x, pc.transform.position.z)) < 1.2f)
                    vaoToiDo++;

                // "Ke dich" = bat ky vat co mau nao KHONG phai nguoi choi nay -
                // ke ca quai that cua man dang bo vay quanh, khong rieng hai bia.
                foreach (var d in Object.FindObjectsByType<Damageable>(FindObjectsSortMode.None))
                {
                    if (d == null || d.IsDead || d == pc.health) continue;
                    if (Vector2.Distance(t2, new Vector2(d.transform.position.x, d.transform.position.z)) < 1.2f)
                    { vaoKeDichDo++; break; }
                }
            }
            yield return null;
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
        var mauToi = pc.GetComponent<Damageable>();
        Vector3 goc = pc.transform.position;

        // ================================================================
        // A1. TANG BANG ROI - do hinh hoc that
        // ================================================================
        Ghi("");
        Ghi("A1. tang bang roi (do " + SoTangDo + " tang sinh ra that)");

        // Chieu cao LUOI GOC (chua nhan co) - de doi chieu voi dai co cu
        float meshY = 0f; int soMesh = 0;
        var dsMesh = VfxFactory.MeshTangBang;
        if (dsMesh != null)
            foreach (var m in dsMesh) { if (m != null) { meshY += m.bounds.size.y; soMesh++; } }
        meshY = soMesh > 0 ? meshY / soMesh : 0f;

        Vector3 choTha = goc + new Vector3(0f, 40f, 0f);
        float tong = 0f, nhoNhat = float.MaxValue, toNhat = 0f;
        int demTang = 0;
        var rac = new List<GameObject>();
        for (int i = 0; i < SoTangDo; i++)
        {
            var go = VfxFactory.IceShardFalling(choTha, 0f, 999f);
            rac.Add(go);
            var tang = go.transform.Find("Tang");
            if (tang == null) continue;
            var mf = tang.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            // Lay hinh hoc cua LUOI nhan voi co, KHONG lay Renderer.bounds:
            // tang bang duoc xoay ngau nhien +-13 do, ma bounds la hop bao theo
            // TRUC THE GIOI nen goc xoay lam no phinh ra - lan do dau tien bao
            // "to them 22%" chi vi cai goc xoay ay.
            float cao = mf.sharedMesh.bounds.size.y * tang.lossyScale.y;
            tong += cao; demTang++;
            nhoNhat = Mathf.Min(nhoNhat, cao);
            toNhat = Mathf.Max(toNhat, cao);
        }
        foreach (var g in rac) if (g != null) Object.DestroyImmediate(g);

        float caoTB = demTang > 0 ? tong / demTang : 0f;
        float caoCuTB = meshY * (TangCuMin + TangCuMax) * 0.5f;
        float tiLeTang = caoCuTB > 0f ? caoTB / caoCuTB : 0f;

        Ghi("    luoi goc cao trung binh " + meshY.ToString("F3") + " m (" + soMesh + " hinh)");
        Ghi("    " + demTang + " tang: cao " + nhoNhat.ToString("F2") + " - " + toNhat.ToString("F2")
            + " m, trung binh " + caoTB.ToString("F3") + " m");
        Ghi("    truoc khi sua co " + TangCuMin + "-" + TangCuMax + " -> cao trung binh "
            + caoCuTB.ToString("F3") + " m; nay to them " + ((tiLeTang - 1f) * 100f).ToString("F1") + "%");
        Kiem(demTang == SoTangDo, "co tang bang khong dung duoc hinh");
        Kiem(Mathf.Abs(tiLeTang - 1.15f) < 0.03f, "tang bang khong to them dung 15%");

        // ================================================================
        // B. THAM SO DI DUNG DUONG: prefab -> IceStorm -> FallingShard
        // ================================================================
        Ghi("");
        Ghi("B. tham so di tu prefab xuong tung tang bang");

        var pfMua = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Skill_MuaBang.prefab");
        var mauMua = pfMua != null ? pfMua.GetComponent<IceStorm>() : null;
        var pfSet = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Skill_SamSet.prefab");
        var mauSet = pfSet != null ? pfSet.GetComponent<LightningStorm>() : null;
        if (mauMua == null || mauSet == null) { Loi("khong doc duoc prefab ky nang"); Ket(); yield break; }

        Ghi("B1. prefab Skill_MuaBang: cham " + (mauMua.chamTiLe * 100f).ToString("F0") + "% trong "
            + mauMua.chamGiay + " giay; dong cung " + (mauMua.freezeChance * 100f).ToString("F0")
            + "% trong " + mauMua.freezeSeconds + " giay");
        Kiem(Mathf.Abs(mauMua.chamTiLe - 0.5f) < 0.001f, "prefab mua bang: ti le lam cham khong phai 50%");
        Kiem(Mathf.Abs(mauMua.chamGiay - 2f) < 0.001f, "prefab mua bang: lam cham khong phai 2 giay");
        Kiem(Mathf.Abs(mauMua.freezeChance - 0.35f) < 0.001f, "prefab mua bang: xac suat dong cung khong phai 35%");
        Kiem(Mathf.Abs(mauMua.freezeSeconds - 1.5f) < 0.001f, "prefab mua bang: dong cung khong phai 1,5 giay");

        Ghi("B2. prefab Skill_SamSet: choang " + (mauSet.stunChance * 100f).ToString("F0") + "% trong "
            + mauSet.stunSeconds + " giay");
        Kiem(Mathf.Abs(mauSet.stunChance - 0.35f) < 0.001f, "prefab sam set: xac suat choang khong phai 35%");

        // Tung mot con mua bang THAT, doc lai tham so tren tang bang dau tien
        int maskEnemy = LayerMask.GetMask("Enemy");
        var muaThat = IceStorm.Spawn(goc + new Vector3(0f, 0f, 30f), maskEnemy);
        muaThat.boQua = mauToi;
        FallingShard mot = null;
        float hanB = Time.time + 3f;
        while (mot == null && Time.time < hanB)
        {
            mot = Object.FindAnyObjectByType<FallingShard>();
            yield return null;
        }
        if (mot == null) Loi("mua bang khong tha duoc tang bang nao");
        else
        {
            Ghi("B3. tang bang that nhan duoc: cham " + (mot.chamTiLe * 100f).ToString("F0") + "% trong "
                + mot.chamGiay + " giay; dong cung " + (mot.freezeChance * 100f).ToString("F0")
                + "% trong " + mot.freezeSeconds + " giay");
            Kiem(Mathf.Abs(mot.chamTiLe - mauMua.chamTiLe) < 0.001f
                 && Mathf.Abs(mot.chamGiay - mauMua.chamGiay) < 0.001f
                 && Mathf.Abs(mot.freezeChance - mauMua.freezeChance) < 0.001f
                 && Mathf.Abs(mot.freezeSeconds - mauMua.freezeSeconds) < 0.001f,
                 "tang bang khong nhan dung tham so cua con bao");
        }
        if (muaThat != null) Object.DestroyImmediate(muaThat.gameObject);
        foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None))
            Object.DestroyImmediate(fs.gameObject);

        // ================================================================
        // C. XAC SUAT tren 1000 lan gieo
        // ================================================================
        Ghi("");
        Ghi("C. gieo " + SoMauGieo + " lan len mot bia do don");

        var bia = TaoBia("TAM_Bia1", goc + new Vector3(0f, 0f, 25f));
        yield return null;

        int soDongCung = 0, soCham = 0;
        for (int i = 0; i < SoMauGieo; i++)
        {
            XoaHieuUng(bia);
            bia.health = bia.maxHealth;
            int sd;
            CombatUtil.AreaFreeze(bia.transform.position + Vector3.up, 1.5f, 1f, maskEnemy,
                                  mauMua.chamTiLe, mauMua.chamGiay,
                                  mauMua.freezeChance, mauMua.freezeSeconds, null, out sd);
            var f = bia.GetComponent<FrozenEffect>();
            if (f != null && f.IsFullyFrozen) soDongCung++;
            if (f != null && f.slow >= mauMua.chamTiLe - 0.001f && f.remaining > 0f) soCham++;
        }
        XoaHieuUng(bia);
        float tlDong = 100f * soDongCung / SoMauGieo, tlCham = 100f * soCham / SoMauGieo;
        Ghi("C1. mua bang: dong cung " + tlDong.ToString("F1") + "% (mong doi 35%), lam cham "
            + tlCham.ToString("F1") + "% (mong doi 100%)");
        Kiem(Mathf.Abs(tlDong - 35f) <= 5f, "xac suat dong cung lech qua xa 35%");
        Kiem(tlCham > 99.9f, "lam cham khong phai lan nao trung cung dinh");

        int soChoang = 0;
        for (int i = 0; i < SoMauGieo; i++)
        {
            XoaHieuUng(bia);
            bia.health = bia.maxHealth;
            CombatUtil.AreaShock(bia.transform.position + Vector3.up, 1.5f, 1f, maskEnemy,
                                 mauSet.stunChance, mauSet.stunSeconds, null);
            var s = bia.GetComponent<StunnedEffect>();
            if (s != null && s.IsStunned) soChoang++;
        }
        XoaHieuUng(bia);
        float tlChoang = 100f * soChoang / SoMauGieo;
        Ghi("C2. sam set: choang " + tlChoang.ToString("F1") + "% (mong doi 35%)");
        Kiem(Mathf.Abs(tlChoang - 35f) <= 5f, "xac suat choang lech qua xa 35%");

        // ================================================================
        // D. NGUOI CHOI CO THUC SU BI GIU CHAN KHONG
        // ================================================================
        Ghi("");
        Ghi("D. nguoi choi bi dong cung / choang (day can 0,6 giay moi lan)");

        int soPhepBayRa = 0;
        System.Action<int, Vector3> demPhep = (s, a) => soPhepBayRa++;
        pc.DaTungPhep += demPhep;
        Vector3 huongThu = Vector3.forward;

        // D1. MAU DOI CHUNG - khong bi gi
        XoaHieuUng(mauToi);
        yield return DoDiChuyen(pc, 0.6f, huongThu);
        float sBinhThuong = quangDuongDo;
        soPhepBayRa = 0;
        pc.CastAt(1, pc.transform.position + huongThu * 6f);
        yield return null;
        int phepBinhThuong = soPhepBayRa;
        Ghi("D1. khong bi gi: di duoc " + sBinhThuong.ToString("F2") + " m, tung phep ra "
            + phepBinhThuong + " lan");
        Kiem(sBinhThuong > 1.5f, "mau doi chung khong di duoc - phep do khong noi len duoc gi");
        Kiem(phepBinhThuong == 1, "mau doi chung khong tung duoc phep");

        // D2. DONG CUNG HOAN TOAN
        yield return new WaitForSeconds(0.6f);
        XoaHieuUng(mauToi);
        FrozenEffect.Apply(mauToi, 3f);
        yield return null;
        bool khoaBang = pc.DangBiKhoaCung;
        yield return DoDiChuyen(pc, 0.6f, huongThu);
        float sDongCung = quangDuongDo;
        soPhepBayRa = 0;
        pc.CastAt(2, pc.transform.position + huongThu * 6f);
        pc.CastAt(3, pc.transform.position + huongThu * 6f);
        yield return null;
        int phepKhiDongCung = soPhepBayRa;
        Ghi("D2. dong cung: DangBiKhoaCung = " + khoaBang + ", di duoc " + sDongCung.ToString("F2")
            + " m, bam 2 phep -> bay ra " + phepKhiDongCung + ", loi nhac: " + pc.LastMessage);
        Kiem(khoaBang, "nguoi choi bi dong cung ma game khong coi la bi khoa");
        Kiem(sDongCung < 0.1f, "nguoi choi van chay duoc trong khi dang bi dong cung");
        Kiem(phepKhiDongCung == 0, "nguoi choi van tung duoc phep trong khi dang bi dong cung");

        // D3. HET DONG CUNG, CON LOP CHAM 50%
        XoaHieuUng(mauToi);
        FrozenEffect.ApCham(mauToi, 0.5f, 3f);
        yield return null;
        yield return DoDiChuyen(pc, 0.6f, huongThu);
        float sCham = quangDuongDo;
        float tiLeCham = sBinhThuong > 0f ? sCham / sBinhThuong : 0f;
        Ghi("D3. chi bi cham 50%: di duoc " + sCham.ToString("F2") + " m = "
            + (tiLeCham * 100f).ToString("F0") + "% quang duong binh thuong");
        Kiem(Mathf.Abs(tiLeCham - 0.5f) < 0.12f, "lop cham khong giam dung mot nua toc do");

        // D4. CHOANG
        XoaHieuUng(mauToi);
        StunnedEffect.Apply(mauToi, 3f);
        yield return null;
        bool khoaChoang = pc.DangBiKhoaCung;
        yield return DoDiChuyen(pc, 0.6f, huongThu);
        float sChoang = quangDuongDo;
        soPhepBayRa = 0;
        pc.CastAt(4, pc.transform.position + huongThu * 6f);
        yield return null;
        int phepKhiChoang = soPhepBayRa;
        Ghi("D4. choang: DangBiKhoaCung = " + khoaChoang + ", di duoc " + sChoang.ToString("F2")
            + " m, bam phep -> bay ra " + phepKhiChoang + ", loi nhac: " + pc.LastMessage);
        Kiem(khoaChoang, "nguoi choi bi choang ma game khong coi la bi khoa");
        Kiem(sChoang < 0.1f, "nguoi choi van chay duoc trong khi dang bi choang");
        Kiem(phepKhiChoang == 0, "nguoi choi van tung duoc phep trong khi dang bi choang");

        // D5. HET HIEU UNG THI TRA LAI NHU CU
        XoaHieuUng(mauToi);
        yield return null;
        yield return DoDiChuyen(pc, 0.6f, huongThu);
        float sSauKhiTan = quangDuongDo;
        Ghi("D5. sau khi tan bang: di duoc " + sSauKhiTan.ToString("F2") + " m (truoc do "
            + sBinhThuong.ToString("F2") + " m)");
        Kiem(sSauKhiTan > sBinhThuong * 0.8f, "tan bang roi ma nguoi choi van bi giu chan");

        pc.DaTungPhep -= demPhep;
        pc.tuDocInput = true;

        // ================================================================
        // E. MUA BANG KHONG NHAM VAO CHINH NGUOI TUNG
        // ================================================================
        Ghi("");
        Ghi("E. mua bang tung ngay duoi chan minh, canh co hai bia");

        Vector3 tam = pc.transform.position;
        var bia1 = TaoBia("TAM_Bia2", tam + new Vector3(3.5f, 0f, 0f));
        var bia2 = TaoBia("TAM_Bia3", tam + new Vector3(-3.5f, 0f, 0f));
        yield return null;

        int maskCaHai = LayerMask.GetMask("Enemy", "Player");
        var mua2 = IceStorm.Spawn(tam, maskCaHai);
        mua2.boQua = mauToi;
        mua2.duration = 3f;

        // HashSet theo THAM CHIEU: Unity 6.5 coi GetInstanceID la loi bien dich.
        yield return DemTangRoi(mua2, tam, pc, 3.2f);
        int tongTang = tongTangDo, vaoToi = vaoToiDo, vaoKeDich = vaoKeDichDo;
        Ghi("E1. co bo qua nguoi tung: " + tongTang + " tang - roi vao nguoi tung "
            + vaoToi + ", roi vao ke dich " + vaoKeDich);
        Kiem(tongTang > 10, "khong dem duoc du tang bang de ket luan");
        Kiem(vaoKeDich > tongTang / 2, "tang bang khong con nham vao ke dich nua");
        Kiem(vaoToi <= tongTang / 10, "tang bang VAN dap xuong dau chinh nguoi tung");
        if (mua2 != null) Object.DestroyImmediate(mua2.gameObject);
        foreach (var fs in Object.FindObjectsByType<FallingShard>(FindObjectsSortMode.None))
            Object.DestroyImmediate(fs.gameObject);
        yield return null;

        // E2. MAU DOI CHUNG - de chung to phep do nay bat duoc loi cu.
        // Bo di mot dong duy nhat (khong bao cho con bao biet ai la nguoi tung)
        // la ta duoc dung hien tuong nguoi dung chup anh: mua bang dap xuong dau
        // chinh minh.
        var mua3 = IceStorm.Spawn(tam, maskCaHai);
        mua3.boQua = null;
        mua3.duration = 3f;
        yield return DemTangRoi(mua3, tam, pc, 3.2f);
        Ghi("E2. KHONG bo qua nguoi tung (kieu cu): " + tongTangDo + " tang - roi vao nguoi tung "
            + vaoToiDo + ", roi vao ke dich " + vaoKeDichDo);
        Kiem(vaoToiDo > tongTangDo / 5, "mau doi chung khong tai hien duoc loi cu - phep do nay khong dang tin");
        if (mua3 != null) Object.DestroyImmediate(mua3.gameObject);

        // ================================================================
        // F. PHEP CUA NGUOI KHAC ROI TRUNG MINH THI MINH CO DINH KHONG
        // ================================================================
        //
        // Day dung la canh nguoi dung ke: A tung mua bang vao B. Tren may cua B,
        // phep ay duoc PHAT LAI (goi LoaiKyNang) voi boQua la ban sao cua A -
        // nen B khong duoc bo qua, B phai dinh that.
        Ghi("");
        Ghi("F. mua bang cua NGUOI KHAC roi trung minh");

        XoaHieuUng(mauToi);

        // PHAI CHO MAU THAT LON: mot con mua tha 20 tang, moi tang 29 sat thuong
        // - 600 mau khong song noi 4 giay, ma nhan vat chet thi man thua hien ra
        // va ca kich ban chet theo (lan dau chay phan nay bi dung dung o day).
        float mauCu = mauToi.maxHealth, mauCu2 = mauToi.health;
        mauToi.maxHealth = 10000000f; mauToi.health = 10000000f;

        var keKhac = TaoBia("TAM_Bia4", pc.transform.position + new Vector3(0f, 0f, 9f));
        var muaCuaHo = IceStorm.Spawn(pc.transform.position, maskCaHai);
        muaCuaHo.boQua = keKhac;      // nguoi tung la NGUOI KHAC, khong phai minh
        muaCuaHo.duration = 2.5f;

        bool daCham = false, daDongCung = false;
        float hanF = Time.time + 4f;
        float hanThat = Time.realtimeSinceStartup + 20f;   // chot chan, khong de treo mai
        while (Time.time < hanF && Time.realtimeSinceStartup < hanThat)
        {
            var f = mauToi.GetComponent<FrozenEffect>();
            if (f != null)
            {
                if (f.slow > 0.01f) daCham = true;
                if (f.IsFullyFrozen) daDongCung = true;
            }
            yield return null;
        }
        Ghi("F1. sau 4 giay duoi con mua cua nguoi khac: co bi cham = " + daCham
            + ", co luc bi dong cung = " + daDongCung);
        Kiem(daCham, "phep cua nguoi khac roi trung minh ma minh khong he bi lam cham");
        Kiem(daDongCung, "phep cua nguoi khac roi trung minh ma khong lan nao dong cung");

        if (muaCuaHo != null) Object.DestroyImmediate(muaCuaHo.gameObject);
        XoaHieuUng(mauToi);
        mauToi.maxHealth = mauCu; mauToi.health = mauCu2;

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
        Debug.Log("[BangSet] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/bang_set.txt", bao.ToString());
        string[] racTen = { "TAM_BangSet", "TAM_Bia1", "TAM_Bia2", "TAM_Bia3", "TAM_Bia4" };
        foreach (var n in racTen)
        {
            var g = GameObject.Find(n);
            if (g != null) Object.DestroyImmediate(g);
        }
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
