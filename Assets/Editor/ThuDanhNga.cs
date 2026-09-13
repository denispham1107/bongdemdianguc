using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: THIEN THACH DANH NGA KE DICH.
///
/// Nguoi dung xin (13/09/2026): qua thien thach roi xuong co 40% danh nga ke
/// dich 1,5 giay; ke bi nga bi HAT NHE LEN roi rot xuong NAM NGUA, khong di
/// duoc, khong dung duoc ky nang.
///
/// Do bang SO:
///   A. Xac suat tren 1000 lan gieo (~40%), va thien thach cua Quy du KHONG
///      danh nga (nguoi dung chi xin cho ky nang cua nguoi choi).
///   B. Tung thien thach THAT tu nhan vat: qua roi mang dung 40% va 1,5 giay.
///   C. Hinh that tren mot con quai: co bi hat len, co nam NGUA (dau nga ra
///      SAU va thap sat dat - do bang VI TRI XUONG DAU, khong doc lai goc minh
///      vua dat), co dung im va khong danh, het 1,5 giay thi dung day va chay tiep.
///   D. Nguoi choi bi nga: khong di, khong tung phep.
///   E. Qua mang: co "dang nga" gui di va ve lai tren ban sao.
///
/// Ket qua ghi ra <c>PlayTestShots/danhnga.txt</c>, anh <c>danhnga_*.png</c>.
/// </summary>
public static class ThuDanhNga
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/62. Chay thu THIEN THACH DANH NGA", false, 153)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu danh nga",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] thien thach danh nga ke dich");

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
        if (GameObject.Find("TAM_DanhNga") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_DanhNga");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DanhNga] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

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

    static Transform TimXuong(Transform goc, string ten)
    {
        foreach (var t in goc.GetComponentsInChildren<Transform>(true))
            if (t.name.ToLowerInvariant().Contains(ten)) return t;
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
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
        var toi = pc.GetComponent<Damageable>();
        toi.maxHealth = 10000000f; toi.health = 10000000f;

        // Can quai that - dot dau Act2 doi 30 giay nen goi tay mot dot
        dir.SinhDotQuanhNguoi();
        yield return new WaitForSeconds(1f);
        var quai = new List<Damageable>();
        foreach (var n in Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None))
        {
            var d = n.GetComponent<Damageable>();
            if (d == null || d.IsDead) continue;
            var ai = n.GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;
            d.transform.position = pc.transform.position + new Vector3(80f + quai.Count * 3f, 0f, 80f);
            quai.Add(d);
        }
        Physics.SyncTransforms();
        Ghi("quai dung de thu: " + quai.Count);
        if (quai.Count < 3) { Loi("khong du quai"); Ket(); yield break; }
        int maskQuai = LayerMask.GetMask("Enemy");

        // ================================================================
        // A. XAC SUAT
        // ================================================================
        Ghi("");
        Ghi("A. xac suat danh nga");
        var bia = quai[0];
        Vector3 choBia = pc.transform.position + new Vector3(0f, 0f, 30f);
        bia.transform.position = new Vector3(choBia.x, VfxFactory.GroundY(choBia), choBia.z);
        bia.maxHealth = 1e7f; bia.health = 1e7f;
        Physics.SyncTransforms();
        int soNga = 0; const int SoLan = 1000;
        for (int i = 0; i < SoLan; i++)
        {
            var cu = bia.GetComponent<BiDanhNga>();
            if (cu != null) Object.DestroyImmediate(cu);
            soNga += ThienThach.GieoDanhNga(bia.transform.position + Vector3.up, 2f, maskQuai, null,
                                            ThienThach.NgaXacSuatNguoiChoi, ThienThach.NgaGiayNguoiChoi);
        }
        var sot = bia.GetComponent<BiDanhNga>();
        if (sot != null) Object.DestroyImmediate(sot);
        float tl = 100f * soNga / SoLan;
        Ghi("A1. " + SoLan + " lan gieo -> danh nga " + tl.ToString("F1") + "% (mong doi 40%)");
        Kiem(Mathf.Abs(tl - 40f) <= 5f, "xac suat danh nga lech qua xa 40%");

        // Thien thach mac dinh (Quy du dung) khong danh nga
        var ttMacDinh = ThienThach.Spawn(pc.transform.position + new Vector3(0f, 0f, -40f),
                                         LayerMask.GetMask("Enemy", "Ground", "Default"), maskQuai);
        Ghi("A2. thien thach mac dinh (cua Quy du): xac suat nga " + ttMacDinh.ngaXacSuat + " (phai 0)");
        Kiem(ttMacDinh.ngaXacSuat == 0f, "thien thach cua quai cung danh nga nguoi choi - khong ai xin");
        Object.DestroyImmediate(ttMacDinh.gameObject);

        // ================================================================
        // B. THIEN THACH THAT TU NHAN VAT
        // ================================================================
        Ghi("");
        Ghi("B. tung thien thach that");
        CapDo.BatDauTranMoi();
        CapDo.MoKhoa(4);
        pc.mana = pc.maxMana;
        pc.CastAt(4, pc.transform.position + pc.transform.forward * 9f);
        ThienThach ttThat = null;
        float hanB = Time.time + 2f;
        // Loc dung qua CUA NHAN VAT (boQua == toi): lan dau phep thu vo phai qua
        // thien thach mac dinh o buoc A2 - Destroy chi huy o cuoi khung hinh.
        while (ttThat == null && Time.time < hanB)
        {
            foreach (var tt in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None))
                if (tt.boQua == toi) { ttThat = tt; break; }
            yield return null;
        }
        if (ttThat == null) Loi("nhan vat khong tung ra thien thach");
        else
        {
            Ghi("B1. qua thien thach cua nguoi choi: danh nga " + (ttThat.ngaXacSuat * 100f).ToString("F0")
                + "% trong " + ttThat.ngaGiay + " giay");
            Kiem(Mathf.Abs(ttThat.ngaXacSuat - 0.40f) < 0.001f, "thien thach nguoi choi khong mang 40%");
            Kiem(Mathf.Abs(ttThat.ngaGiay - 1.5f) < 0.001f, "thien thach nguoi choi khong mang 1,5 giay");
        }
        yield return new WaitForSeconds(3f);
        foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);

        // ================================================================
        // C. HINH THAT TREN MOT CON QUAI
        // ================================================================
        Ghi("");
        Ghi("C. con quai bi danh nga (hinh that)");
        var q = quai[1];
        // Dat quai truoc mat nhan vat, cho camera nhin thay, bat lai nao cho no DUOI
        Vector3 huong = pc.transform.forward; huong.y = 0f; huong.Normalize();
        Vector3 cho = pc.transform.position + huong * 5f;
        q.transform.position = new Vector3(cho.x, VfxFactory.GroundY(cho), cho.z);
        q.transform.rotation = Quaternion.LookRotation(-huong);
        q.maxHealth = 1e7f; q.health = 1e7f;
        var aiQ = q.GetComponent<EnemyAI>();
        if (aiQ != null) aiQ.enabled = true;
        Physics.SyncTransforms();
        yield return new WaitForSeconds(0.3f);

        var dau = TimXuong(q.transform, "head");
        var chan = q.transform;
        Ghi("    xuong dau tim duoc: " + (dau != null ? dau.name : "KHONG"));

        float caoDauDung = dau != null ? dau.position.y - chan.position.y : 0f;
        Vector3 viTriTruoc = q.transform.position;
        float mauToiTruoc = toi.health;

        BiDanhNga.Apply(q, 1.5f);
        var ng = q.GetComponent<BiDanhNga>();
        Transform hinh = null;
        foreach (Transform c in q.transform) if (c.GetComponent<Animation>() != null) { hinh = c; break; }

        float caoHinhMax = 0f, caoDauNam = 999f, sauLung = 0f;
        bool daChup = false;
        float t0 = Time.time;
        while (Time.time - t0 < 1.35f)
        {
            if (hinh != null) caoHinhMax = Mathf.Max(caoHinhMax, hinh.localPosition.y);
            float e = Time.time - t0;
            if (e > 0.6f && e < 1.0f && dau != null)
            {
                caoDauNam = Mathf.Min(caoDauNam, dau.position.y - chan.position.y);
                // Dau nam ve phia SAU lung: nguoc huong mat cua goc nhan vat
                sauLung = Vector3.Dot(dau.position - chan.position, -q.transform.forward);
                if (!daChup) { daChup = true; yield return Chup("danhnga_1_quai_nam_ngua"); }
            }
            yield return null;
        }
        bool vanNga = ng != null && ng.DangNga;
        float diChuyen = Vector2.Distance(new Vector2(viTriTruoc.x, viTriTruoc.z),
                                          new Vector2(q.transform.position.x, q.transform.position.z));
        Ghi("C1. hat len: hinh cao toi da " + caoHinhMax.ToString("F2") + " m (phai ~0,5-0,9)");
        Ghi("C2. luc nam: dau cach dat " + caoDauNam.ToString("F2") + " m (dung thang: " + caoDauDung.ToString("F2")
            + " m), dau nam ve sau lung " + sauLung.ToString("F2") + " m (phai > 0 = NGUA)");
        Ghi("C3. trong 1,35 giay: van dang nga " + vanNga + ", di chuyen " + diChuyen.ToString("F2")
            + " m, mau nguoi choi mat " + (mauToiTruoc - toi.health).ToString("F0"));
        Kiem(caoHinhMax > 0.45f, "khong thay bi hat len khoi mat dat");
        Kiem(dau == null || caoDauNam < caoDauDung * 0.45f, "luc nam dau van o cao - khong nam xuong");
        Kiem(dau == null || sauLung > 0.4f, "khong nam NGUA (dau khong nga ra sau lung)");
        Kiem(vanNga, "chua het 1,5 giay ma da het nga");
        Kiem(diChuyen < 0.2f, "dang bi nga ma quai van di chuyen");

        yield return new WaitForSeconds(0.6f);
        bool hetNga = q.GetComponent<BiDanhNga>() == null;
        float caoDauSau = dau != null ? dau.position.y - chan.position.y : 0f;
        Ghi("C4. sau 1,95 giay: het nga " + hetNga + ", dau cao " + caoDauSau.ToString("F2") + " m (dung thang "
            + caoDauDung.ToString("F2") + ")");
        Kiem(hetNga, "het 1,5 giay ma van nga");
        Kiem(dau == null || Mathf.Abs(caoDauSau - caoDauDung) < 0.25f, "het nga ma khong dung day lai");
        yield return Chup("danhnga_2_quai_dung_day");
        if (aiQ != null) aiQ.enabled = false;

        // ================================================================
        // D. NGUOI CHOI BI NGA
        // ================================================================
        Ghi("");
        Ghi("D. nguoi choi bi nga");
        int soPhep = 0;
        System.Action<int, Vector3> dem = (a, b) => soPhep++;
        pc.DaTungPhep += dem;
        CapDo.MoKhoa(4); CapDo.Them(1000); CapDo.MoKhoa(0);
        BiDanhNga.Apply(toi, 1.5f);
        yield return null;
        bool khoa = pc.DangBiKhoaCung;
        var g = GoiInput.Rong(0f); g.huongDi = Vector3.forward;
        pc.tuDocInput = false; pc.input = g;
        Vector3 truocD = pc.transform.position;
        yield return new WaitForSeconds(0.6f);
        float diD = Vector2.Distance(new Vector2(truocD.x, truocD.z), new Vector2(pc.transform.position.x, pc.transform.position.z));
        pc.input = GoiInput.Rong(0f);
        pc.CastAt(0, pc.transform.position + pc.transform.forward * 6f);
        yield return null;
        Ghi("D1. DangBiKhoaCung " + khoa + ", day can 0,6 giay di " + diD.ToString("F2") + " m, bam phep -> bay ra "
            + soPhep + ", loi nhac: " + pc.LastMessage);
        Kiem(khoa && diD < 0.1f && soPhep == 0, "nguoi choi bi nga van di / van tung phep");
        pc.DaTungPhep -= dem;
        pc.tuDocInput = true;
        yield return new WaitForSeconds(1.2f);

        // ================================================================
        // E. QUA MANG
        // ================================================================
        Ghi("");
        Ghi("E. qua mang");
        var q2 = quai[2];
        BiDanhNga.Apply(q2, 1.5f);
        byte co = HieuUngQuaMang.DocCo(q2.gameObject);
        Ghi("E1. may chu so huu doc co: dang nga = " + ((co & HieuUngQuaMang.CoNga) != 0));
        Kiem((co & HieuUngQuaMang.CoNga) != 0, "co dang nga khong duoc gui di");
        var bn = q2.GetComponent<BiDanhNga>(); if (bn != null) Object.DestroyImmediate(bn);

        q2.mauDoMayKhacQuyet = true;
        BiDanhNga.Apply(q2, 1.5f);
        bool tuGieo = q2.GetComponent<BiDanhNga>() != null;
        HieuUngQuaMang.ApCo(q2, HieuUngQuaMang.CoNga);
        bool veLai = q2.GetComponent<BiDanhNga>() != null;
        yield return new WaitForSeconds(1.0f);          // goi ngung den
        bool tuDungDay = q2.GetComponent<BiDanhNga>() == null;
        Ghi("E2. ban sao: tu gieo " + tuGieo + " (phai False), goi toi -> nga " + veLai
            + ", goi ngung 1 giay -> da dung day " + tuDungDay);
        Kiem(!tuGieo, "ban sao tu gieo danh nga - hai may thay hai ket qua");
        Kiem(veLai, "ban sao khong ve lai trang thai nga tu goi tin");
        Kiem(tuDungDay, "goi ngung roi ma ban sao nam mai");
        q2.mauDoMayKhacQuyet = false;

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
        Debug.Log("[DanhNga] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/danhnga.txt", bao.ToString());
        var rac = GameObject.Find("TAM_DanhNga");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
