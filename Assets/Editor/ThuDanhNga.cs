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
///   G. SO QUA THEO CAP (nguoi dung 19/09/2026): cap 1-4 roi 3 qua, CAP 5 roi 5 qua - dem tren duong tung THAT
///      (CastAt) chu khong goi thang SpawnLoat, vi so qua do PlayerController quyet dinh theo cap NGUOI TUNG.
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
        CapDo.MoCaDuongChoPhepThu(4);
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
        System.Action<int, Vector3, bool> dem = (a, b, d) => soPhep++;
        pc.DaTungPhep += dem;
        CapDo.MoCaDuongChoPhepThu(4); CapDo.Them(1000); CapDo.MoCaDuongChoPhepThu(0);
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
        // F. DUONG THAT: NHAN VAT TUNG THIEN THACH, QUA ROI TRUNG QUAI THAT
        // ================================================================
        //
        // Nguoi dung bao (13/09/2026): danh rat nhieu lan ma khong lan nao thay
        // nga. Cac phan tren goi THANG BiDanhNga.Apply va chi DOC tham so qua
        // thien thach - chua lan nao cho mot qua that roi trung mot con quai
        // that roi dem. Phan nay lam dung viec nguoi choi lam.
        Ghi("");
        Ghi("F. tung thien thach that 10 lan vao mot con quai that");
        {
            var qf = quai[3];
            int soTrungF = 0, soNgaF = 0, soTungF = 0;
            int khungNga = 0, khungCoDau = 0;
            bool daChupF = false;
            for (int lan = 0; lan < 10; lan++)
            {
                var cuF = qf.GetComponent<BiDanhNga>(); if (cuF != null) Object.DestroyImmediate(cuF);
                var chayF = qf.GetComponent<BurningEffect>(); if (chayF != null) Object.DestroyImmediate(chayF);
                foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);

                Vector3 hf = pc.transform.forward; hf.y = 0f; hf.Normalize();
                Vector3 cf = pc.transform.position + hf * 8f;
                qf.transform.position = new Vector3(cf.x, VfxFactory.GroundY(cf), cf.z);
                qf.maxHealth = 1e7f; qf.health = 1e7f;
                Physics.SyncTransforms();
                yield return new WaitForSeconds(2.2f);            // het hoi chieu 2 giay

                float mauTruocF = qf.health;
                pc.mana = pc.maxMana;
                int truocPhep = 0;
                System.Action<int, Vector3, bool> demF = (a, b, d) => truocPhep++;
                pc.DaTungPhep += demF;
                pc.CastAt(4, qf.transform.position);
                pc.DaTungPhep -= demF;
                if (truocPhep > 0) soTungF++;

                bool ngaLanNay = false;
                float hanF = Time.time + 3.5f;
                while (Time.time < hanF)
                {
                    var bnF = qf.GetComponent<BiDanhNga>();
                    if (bnF != null)
                    {
                        ngaLanNay = true;
                        khungNga++;
                        if (BiDanhNga.SoDauVeLanCuoi > 0) khungCoDau++;
                        // Chuoi anh trong suot luc nam - de biet lua no che toi giay thu may
                        if (!daChupF)
                        {
                            daChupF = true;
                            float[] moc = { 0.25f, 0.6f, 0.95f, 1.2f, 1.4f };
                            for (int m = 0; m < moc.Length; m++)
                            {
                                while (bnF != null && bnF.daTroi < moc[m]) yield return null;
                                yield return Chup("danhnga_3_chuoi_" + m + "_" + moc[m].ToString("0.00"));
                            }
                        }
                    }
                    yield return null;
                }
                if (qf.health < mauTruocF - 1f) soTrungF++;
                if (ngaLanNay) soNgaF++;
                Ghi("    lan " + (lan + 1) + ": tung " + (truocPhep > 0) + ", trung " + (qf.health < mauTruocF - 1f)
                    + " (mat " + (mauTruocF - qf.health).ToString("F0") + " mau), bi nga " + ngaLanNay);
            }
            Ghi("F1. tung " + soTungF + "/10, trung " + soTrungF + "/10, bi nga " + soNgaF + "/10"
                + " (moi lan 3 qua x 40% -> mong doi ~78% so lan co it nhat mot lan nga)");
            Kiem(soTungF == 10, "nhan vat khong tung du 10 lan - phep do hong");
            Kiem(soTrungF >= 8, "thien thach khong trung quai - phep do hong");
            Kiem(soNgaF >= 4, "thien thach THAT roi trung quai that ma gan nhu khong bao gio danh nga");

            // Dau hieu tren dau phai hien SUOT luc nga (lua 3D che kin con quai)
            float tiLeDau = khungNga > 0 ? 100f * khungCoDau / khungNga : 0f;
            Ghi("F2. dau hieu vong sao + chu NGA tren dau: hien " + khungCoDau + "/" + khungNga
                + " khung hinh dang nga (" + tiLeDau.ToString("F0") + "%)");
            Kiem(tiLeDau > 90f, "dang nga ma khong ve dau hieu tren dau - nguoi choi khong nhin thay");
        }

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

        // ================================================================
        // G. SO QUA MOI LOAT THEO CAP (cap 5 them 2 qua - nguoi dung 19/09/2026)
        // ================================================================
        Ghi("");
        Ghi("G. so qua moi loat theo cap ky nang");
        foreach (var ttCu in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None)) Object.DestroyImmediate(ttCu.gameObject);
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(4);
        Vector3 choG = pc.transform.position + pc.transform.forward * 9f;
        int[] demTheoCap = new int[CapDo.CapKyNangToiDa + 1];
        for (int capG = 1; capG <= CapDo.CapKyNangToiDa; capG++)
        {
            // Nang DUNG len capG (co TRAN: NangCap tra false khi het diem va khong doi gi - vong khong tran treo cung Unity)
            for (int v = 0; v < 60 && CapDo.CapCuaKyNang(4) < capG; v++)
            {
                CapDo.ThemDiemChoPhepThu(1);
                if (!CapDo.NangCap(4)) break;
            }
            if (CapDo.CapCuaKyNang(4) != capG) { Loi("khong nang duoc Thien thach len cap " + capG); break; }

            pc.mana = pc.maxMana;
            pc.CastAt(4, choG);
            // PHAI CHO HET THOI GIAN NIEM: bam la xong o day thi dem duoc 0 qua o MOI cap
            // (lan chay dau 19/09/2026 dem ngay sau mot khung hinh, ca nam cap deu ra 0).
            int demG = 0;
            float hanG = Time.time + 3f;                   // TRAN: vong cho nao cung phai co
            while (demG == 0 && Time.time < hanG)
            {
                yield return null;
                demG = 0;
                foreach (var tt in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None))
                    if (tt.boQua == toi) demG++;
            }
            if (demG == 0) Ghi("    cap " + capG + ": khong tung duoc, loi nhac: " + pc.LastMessage);
            demTheoCap[capG] = demG;
            // Xoa ngay: qua dang cho luot van la ThienThach, de lai thi cap sau dem cong don
            foreach (var tt in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None)) Object.DestroyImmediate(tt.gameObject);
            // CHO HET HOI CHIEU THAT (doc tu nhan vat, khong chep tay): lan truoc cho 1,2 giay thi cap 2
            // va cap 4 bi tu choi voi loi nhac "dang hoi chieu" va dem ra 0 qua.
            float nlG, hcG, ncG;
            SachPhep.ThongSo(pc, 4, out nlG, out hcG, out ncG);
            yield return new WaitForSeconds(hcG + 0.5f);
        }
        Ghi("G1. so qua theo cap 1..5 = " + demTheoCap[1] + ", " + demTheoCap[2] + ", " + demTheoCap[3] + ", "
            + demTheoCap[4] + ", " + demTheoCap[5] + "  (hang: SoQuaThuong " + ThienThach.SoQuaThuong
            + ", SoQuaCap5 " + ThienThach.SoQuaCap5 + " tu cap " + ThienThach.CapNamQua + ")");
        for (int capG = 1; capG < ThienThach.CapNamQua; capG++)
            Kiem(demTheoCap[capG] == ThienThach.SoQuaThuong, "cap " + capG + " khong roi " + ThienThach.SoQuaThuong + " qua ma roi " + demTheoCap[capG]);
        Kiem(demTheoCap[ThienThach.CapNamQua] == ThienThach.SoQuaCap5,
             "cap " + ThienThach.CapNamQua + " khong roi " + ThienThach.SoQuaCap5 + " qua ma roi " + demTheoCap[ThienThach.CapNamQua]);
        Kiem(demTheoCap[ThienThach.CapNamQua] - demTheoCap[1] == 2, "cap 5 khong THEM DUNG 2 qua so voi cap 1");
        // DOI CHUNG: hang doc tu chinh ThienThach, khong chep tay
        Ghi("G2. doi chung SoQuaTheoCap(1..5) = " + ThienThach.SoQuaTheoCap(1) + ", " + ThienThach.SoQuaTheoCap(2) + ", "
            + ThienThach.SoQuaTheoCap(3) + ", " + ThienThach.SoQuaTheoCap(4) + ", " + ThienThach.SoQuaTheoCap(5));
        foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);

        // G3. NHIP ROI (nguoi dung 25/09/2026: rut ngan khoang cho, chon 0,35 giay - truoc 0,7). Do THOI DIEM
        // CHAM DAT that cua tung qua (vat ThienThach bien mat khi no), khong doc lai hang so.
        {
            pc.mana = pc.maxMana;
            pc.CastAt(4, choG);
            var dsNo = new List<float>();
            int conTruoc = -1;
            float hanN = Time.time + 8f;
            bool daCo = false;
            while (Time.time < hanN)
            {
                yield return null;
                int con = 0;
                foreach (var tt in Object.FindObjectsByType<ThienThach>(FindObjectsSortMode.None)) if (tt.boQua == toi) con++;
                if (con > 0) daCo = true;
                if (daCo && conTruoc > con) for (int k = 0; k < conTruoc - con; k++) dsNo.Add(Time.time);
                conTruoc = con;
                if (daCo && con == 0) break;
            }
            var sbN = new System.Text.StringBuilder();
            float tong = 0f;
            for (int k = 1; k < dsNo.Count; k++) { float d = dsNo[k] - dsNo[k - 1]; tong += d; sbN.AppendFormat("{0:F2} ", d); }
            float tb = dsNo.Count > 1 ? tong / (dsNo.Count - 1) : -1f;
            Ghi(string.Format("G3. cap {0}: {1} qua cham dat, khoang cach giua hai lan cham dat: {2}-> trung binh {3:F3} giay (mong {4:F2}; cu 0,70); ca loat {5:F2} giay",
                CapDo.CapCuaKyNang(4), dsNo.Count, sbN.ToString(), tb, ThienThach.GiayCachNhau,
                dsNo.Count > 1 ? dsNo[dsNo.Count - 1] - dsNo[0] : -1f));
            Kiem(dsNo.Count == ThienThach.SoQuaCap5, "khong dem du " + ThienThach.SoQuaCap5 + " lan cham dat");
            Kiem(Mathf.Abs(tb - 0.35f) < 0.05f && Mathf.Abs(ThienThach.GiayCachNhau - 0.35f) < 0.001f,
                 "khoang cho giua hai qua khong phai 0,35 giay");
            foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);
        }

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
