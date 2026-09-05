using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU CAY CHAY: tha mot qua thien thach vao mot cai cay roi DO xem lua co
/// lan het ca cay khong, co rung manh khong, cay co bien mat va co moc lai dung
/// han hay khong.
///
/// Vi sao phai la mot kich ban tu chay chu khong phai vai lenh MCP roi rac: ca
/// vong doi dai gan bon chuc giay, va moi lenh MCP la mot khung hinh khac nhau
/// o mot thoi diem KHONG doan truoc duoc. Muon biet "lua da lan het chua" thi
/// phai co ai do dem gio ngay trong Play mode.
///
/// Ket qua ghi ra <c>PlayTestShots/cay_chay_&lt;man&gt;.txt</c> kem vai tam anh.
/// </summary>
public static class ThuCayChay
{
    static float t0 = -1f, tChay = -1f, tLanHet = -1f;

    /// <summary>
    /// Nho lai thoi gian chay NGAY LUC BAT LUA.
    ///
    /// Doc <c>cc.thoiGianChay</c> o nhung buoc sau thi hong: luc cay moc lai,
    /// component tu goi Destroy(this) va <c>cc</c> thanh null - dieu kien
    /// "cc != null && ..." khong bao gio dung nua, kich ban treo trong Play va
    /// khong ghi ra ket qua nao. Da vap dung cai do o lan chay dau.
    /// </summary>
    static float tgChay;
    static int buoc;
    static int loi;
    static string thuMuc;
    static string tenMan = "Act2";
    static readonly StringBuilder bao = new StringBuilder();

    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    static Transform cay;
    static CayChay cc;
    static Damageable quai;
    static float mauDau;
    static Vector3 chanCay;

    /// <summary>
    /// Bam dung PHIM PHEP cua nguoi choi thay vi goi thang ThienThach.Spawn.
    ///
    /// Goi thang la kiem tra <c>ThienThach.No</c>, con cai nguoi choi thuc su
    /// lam la <c>CastAt(4)</c> -> <c>SpawnLoat</c> -> ba qua -> <c>No</c>, voi
    /// mat na va diem ngam cua chinh nguoi choi. Phai chay thu ca duong do,
    /// khong thi mot khac biet o mat na cung du lam ca tinh nang khong bao gio
    /// chay trong luc choi that.
    /// </summary>
    static bool phepThat;

    // ================================================================

    [MenuItem("Diablo 2.5D/18. Chay thu CAY CHAY - Act2", false, 100)]
    public static void Chay() { phepThat = false; Chay("Act2"); }

    /// <summary>
    /// Chay thu tren Act1 nua, vi hai man dung cay HOAN TOAN khac nhau:
    /// Act2 mot renderer mot luoi dua tu Blender (Read/Write tat, phai dung bo
    /// diem nuong san), con Act1 dung cay bang code thanh 88 manh con voi luoi
    /// doc thang duoc.
    /// </summary>
    [MenuItem("Diablo 2.5D/18b. Chay thu CAY CHAY - Act1", false, 101)]
    public static void ChayAct1() { phepThat = false; Chay("Act1"); }

    /// <summary>Nhu menu 18 nhung nguoi choi tu tung phep Thien thach.</summary>
    [MenuItem("Diablo 2.5D/18d. Chay thu CAY CHAY bang PHEP THAT - Act2", false, 104)]
    public static void ChayPhepThat() { phepThat = true; Chay("Act2"); }

    public static void Chay(string man)
    {
        tenMan = man;
        thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots");
        Directory.CreateDirectory(thuMuc);

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/" + man + ".unity");

        bao.Length = 0;
        loi = 0;
        buoc = 0;
        t0 = -1f; tChay = -1f; tLanHet = -1f;
        cay = null; cc = null; quai = null;

        Application.logMessageReceived += OnLog;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    // ================================================================
    //  THU TRAN "MAY CAY CHAY CUNG LUC"
    // ================================================================

    static float fpsNen, fpsChay;
    static int soCayBatLua;

    /// <summary>
    /// Do khung hinh bang TRUNG BINH tren mot cua so vai giay, khong lay
    /// <c>Time.smoothDeltaTime</c> tai mot khoanh khac.
    ///
    /// Lay mot khoanh khac thi so nhay lung tung: lan do dau ra 6,5 fps voi
    /// 7 154 hat, roi sau khi CAT BOT hat xuong 4 697 lai ra 5,1 fps - nguoc han
    /// chieu. Trong Editor, mot cu khung nao do (nap asset, ve Scene view) roi
    /// dung vao khung dang do la du lam hong ca phep do. Xem HUONG-DAN, muc
    /// "Do nhip thoi gian phai do lau".
    /// </summary>
    static float congDt;
    static int demKhung;

    static void BatDauDoFps() { congDt = 0f; demKhung = 0; }
    static void GopKhung() { congDt += Time.unscaledDeltaTime; demKhung++; }
    static float FpsTrungBinh() { return demKhung > 0 ? demKhung / Mathf.Max(0.0001f, congDt) : 0f; }

    /// <summary>
    /// Cham lua vao 12 cai cay MOT LUC de xem cai tran <see cref="CayChay.ToiDaCungLuc"/>
    /// co giu duoc khong, va khung hinh tut bao nhieu khi ca dam cung chay.
    /// </summary>
    [MenuItem("Diablo 2.5D/18c. Chay thu TRAN so cay chay cung luc - Act2", false, 102)]
    public static void ChayTran()
    {
        thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots");
        Directory.CreateDirectory(thuMuc);
        tenMan = "Act2";

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        bao.Length = 0;
        loi = 0; buoc = 0; t0 = -1f;
        fpsNen = fpsChay = 0f; soCayBatLua = 0;

        Application.logMessageReceived += OnLog;
        EditorApplication.update += NhipTran;
        EditorApplication.EnterPlaymode();
    }

    static void NhipTran()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.time;
        float t = Time.time - t0;

        switch (buoc)
        {
            // Bo qua hai giay dau (canh vua nap xong, khung hinh con giat), roi
            // do NEN trong mot cua so 2,5 giay
            case 0:
                if (t > 2.0f) { BatDauDoFps(); buoc++; }
                break;

            case 1:
                GopKhung();
                if (t > 4.5f)
                {
                    fpsNen = FpsTrungBinh();
                    Ghi(string.Format("khung hinh NEN (chua chay gi) = {0:F1} fps  ({1} khung / {2:F1}s)",
                                      fpsNen, demKhung, congDt));
                    buoc++;
                }
                break;

            case 2:
                if (t > 4.6f)
                {
                    var cams = Camera.main;
                    Vector3 giua = cams != null ? cams.transform.position : Vector3.zero;

                    var ds = new List<Transform>();
                    foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude,
                                                                          FindObjectsSortMode.None))
                        if (CayChay.LaCay(tr.gameObject)) ds.Add(tr);
                    ds.Sort((a, b) => Vector3.Distance(a.position, giua)
                                     .CompareTo(Vector3.Distance(b.position, giua)));

                    int thu = Mathf.Min(12, ds.Count);
                    for (int i = 0; i < thu; i++)
                        if (CayChay.Dot(ds[i].gameObject, LayerMask.GetMask("Enemy"), 22f) != null)
                            soCayBatLua++;

                    Ghi(string.Format("cham lua vao {0} cay -> {1} cai bat lua (tran = {2})",
                                      thu, soCayBatLua, CayChay.ToiDaCungLuc));
                    if (soCayBatLua > CayChay.ToiDaCungLuc)
                    { Ghi("[LOI] tran khong giu duoc"); loi++; }
                    buoc++;
                }
                break;

            // Cho lua lan het roi moi do - luc do moi la luc nang nhat
            case 3:
                if (t > 8.5f) { BatDauDoFps(); buoc++; }
                break;

            case 4:
                GopKhung();
                if (t > 11f)
                {
                    fpsChay = FpsTrungBinh();
                    int hat = 0;
                    foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude,
                                                                               FindObjectsSortMode.None))
                        hat += ps.particleCount;

                    Ghi(string.Format("dang chay {0} cay: {1:F1} fps (nen {2:F1}), tong hat trong canh = {3}",
                                      soCayBatLua, fpsChay, fpsNen, hat));
                    Chup("tran_cay_chay");
                    buoc++;
                }
                break;

            case 5:
                if (t > 11.5f) { KetTran(); buoc++; }
                break;
        }
    }

    static void KetTran()
    {
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText(Path.Combine(thuMuc, "cay_chay_tran.txt"), bao.ToString());

        EditorApplication.update -= NhipTran;
        Application.logMessageReceived -= OnLog;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }

    // ================================================================

    static void OnLog(string dong, string ngan, LogType loai)
    {
        if (loai == LogType.Error || loai == LogType.Exception || loai == LogType.Assert)
        {
            loi++;
            Ghi("[LOI] " + dong);
        }
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuCayChay] " + s);
    }

    /// <summary>
    /// DONG HO PHAI LA THOI GIAN CUA GAME, khong phai thoi gian that.
    ///
    /// Lan do dau tien dung <c>Time.realtimeSinceStartup</c> va bao "cay khong
    /// bat lua" - trong khi no co bat lua that. Luc Act2 vua vao Play, may khung
    /// hinh dau nang toi muc <c>Time.deltaTime</c> bi ket o tran 0,333 s: dong ho
    /// that da chay 4 giay ma trong game moi troi chua duoc mot giay, thien thach
    /// con dang lo lung tren khong.
    ///
    /// Va moc do KHONG dem tu luc tha thien thach ma dem tu luc CAY BAT LUA:
    /// qua da roi bao lau la chuyen cua qua da, con "lan het roi moi tat dan" la
    /// chuyen cua cai cay.
    /// </summary>
    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.time;
        float t = Time.time - t0;

        switch (buoc)
        {
            // ---- 1. Chon cay, dat camera, tha mot con quai duoi goc ----
            case 0:
                // ChuanBi tra ve false thi nhay THANG toi buoc ket. Truoc day no
                // tu dat buoc roi ben ngoai lai buoc++ - khong nhanh nao khop,
                // kich ban treo trong Play va khong ghi ra ket qua nao.
                if (t > 1.0f) buoc = ChuanBi() ? buoc + 1 : 9;
                break;

            // ---- 2. Tha thien thach xuong dung chan cay ----
            case 1:
                if (t > 1.6f)
                {
                    if (phepThat)
                    {
                        var nguoi = Object.FindAnyObjectByType<PlayerController>();
                        if (nguoi == null) { Ghi("[LOI] khong tim thay nguoi choi"); loi++; buoc = 9; break; }

                        // Dua nguoi choi lai gan cay - phep co tam ban, dung xa
                        // qua thi diem ngam bi keo ve trong tam va qua da roi
                        // cach cai cay may met.
                        // Dung XA dam lua va cho that nhieu mau.
                        //
                        // Lan truoc de nguoi choi dung cach 6 m: qua da roi ngay
                        // canh, nguoi choi an vung lua roi chet, GameDirector nap
                        // lai canh - va kich ban mat sach moi tham chieu giua
                        // chung. Phep do phai khong dinh gi toi cai no do.
                        nguoi.transform.position = chanCay + new Vector3(0f, 0.1f, 11f);
                        var mauNguoi = nguoi.GetComponent<Damageable>();
                        if (mauNguoi != null)
                        {
                            mauNguoi.maxHealth = 100000f;
                            mauNguoi.health = 100000f;
                        }
                        nguoi.CastAt(4, chanCay);
                        Ghi(string.Format("t={0:F2}s  nguoi choi TUNG PHEP Thien thach vao {1}",
                                          t, chanCay.ToString("F2")));
                    }
                    else
                    {
                        var tt = ThienThach.Spawn(chanCay, VfxFactory.LopChuongNgai,
                                                  LayerMask.GetMask("Enemy"));
                        Ghi(string.Format("t={0:F2}s  tha thien thach xuong {1}, dich {2}",
                                          t, chanCay.ToString("F2"), tt.DiemDich.ToString("F2")));
                    }
                    buoc++;
                }
                break;

            // ---- 3. Cho toi khi cay bat lua ----
            case 2:
                if (cay != null && cay.GetComponent<CayChay>() != null)
                {
                    tChay = Time.time;
                    cc = cay.GetComponent<CayChay>();
                    tgChay = cc.thoiGianChay;
                    Ghi(string.Format("t={0:F2}s  CAY BAT LUA (sau {1:F2}s ke tu luc tha)", t, t - 1.6f));
                    buoc++;
                }
                else if (t > 8f)
                {
                    Ghi("[LOI] tha 8 giay ma cay khong bat lua");
                    loi++;
                    buoc = 9;
                }
                break;

            // ---- 4. Vua bat lua: lua phai con o GOC, chua lan het ----
            case 3:
                if (Time.time - tChay > 0.6f)
                {
                    DoLucMoiBen();
                    Chup("cay_01_moi_ben");
                    buoc++;
                }
                break;

            // ---- 5. Giua doan lan: phai co manh rot xuong, phai den dan ----
            case 4:
                if (cc == null) { Ghi("[LOI] mat component giua chung"); loi++; buoc = 9; }
                else if (cc.TienDoLan > 0.6f)
                {
                    DemManh(string.Format("giua doan lan (tien do {0:P0})", cc.TienDoLan));
                    Chup("cay_02_dang_lan");
                    buoc++;
                }
                break;

            // ---- 6. LAN HET: moi bo phan deu dang chay, ma cay VAN CON ----
            case 5:
                if (cc == null) { Ghi("[LOI] cay bien mat TRUOC khi lua lan het"); loi++; buoc = 9; }
                else if (cc.TienDoLan >= 1f)
                {
                    tLanHet = Time.time;
                    DoLucLanHet();
                    Chup("cay_03_lan_het");
                    buoc++;
                }
                break;

            // ---- 7. Chay het thi cay phai BIEN MAT ----
            case 6:
                if (Time.time - tChay > tgChay + 1.5f)
                {
                    DoLucRui();
                    Chup("cay_04_da_rui");
                    buoc++;
                }
                break;

            // ---- 8. Giua doan cho: chua duoc moc lai ----
            case 7:
                if (Time.time - tChay > tgChay + CayChay.GiayHoiSinh * 0.5f)
                {
                    DoGiuaChung(Time.time - tChay);
                    buoc++;
                }
                break;

            // ---- 9. Het 30 giay thi cay phai MOC LAI ----
            case 8:
                if (Time.time - tChay > tgChay + CayChay.GiayHoiSinh + 1.5f)
                {
                    DoLucMocLai(Time.time - tChay);
                    Chup("cay_05_moc_lai");
                    buoc++;
                }
                break;

            case 9:
                Ket();
                break;
        }
    }

    // ================================================================

    static bool ChuanBi()
    {
        // Chon cai cay xa nguoi choi mot chut de khong bi nhan vat che
        Transform tot = null;
        float xaNhat = 0f;
        foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude,
                                                              FindObjectsSortMode.None))
        {
            if (!CayChay.LaCay(tr.gameObject)) continue;
            var r = tr.GetComponentInChildren<Renderer>();
            if (r == null) continue;
            float d = new Vector2(tr.position.x, tr.position.z).magnitude;
            if (d > 10f && d < 34f && d > xaNhat) { xaNhat = d; tot = tr; }
        }

        if (tot == null) { Ghi("[LOI] khong tim thay cay nao hop le"); loi++; return false; }
        cay = tot;

        var rs = cay.GetComponentsInChildren<Renderer>();
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        chanCay = new Vector3(b.center.x, b.min.y, b.center.z);

        Ghi(string.Format("cay = {0}  chan = {1}  bao = {2:F1} x {3:F1} x {4:F1}  renderer = {5}",
                          cay.name, chanCay.ToString("F2"), b.size.x, b.size.y, b.size.z, rs.Length));

        // Tha mot con quai DUNG YEN duoi goc de do sat thuong cua lua tren cay
        var go = EnemyFactory.Spawn(MonsterType.Skeleton, chanCay + new Vector3(0.8f, 0.1f, 0.4f),
                                    null, null);
        var ai = go.GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;                  // dung im trong dam lua
        quai = go.GetComponent<Damageable>();
        mauDau = quai != null ? quai.health : -1f;
        Ghi(string.Format("quai thu nghiem: mau dau = {0:F1}", mauDau));

        // Camera dung yen nhin cai cay. CameraRig phai TAT, khong thi moi khung
        // hinh no keo camera ve theo nguoi choi va may tam anh ra may canh khac
        // nhau (xem HUONG-DAN: "Do trong Play phai tat CameraRig").
        var cam = Camera.main;
        if (cam != null)
        {
            var rig = cam.GetComponent<CameraRig>();
            if (rig != null) rig.enabled = false;
            Vector3 mat = chanCay + new Vector3(0f, b.size.y * 0.45f, 0f);
            cam.transform.position = mat + new Vector3(9f, 6f, -14f);
            cam.transform.LookAt(mat);
        }
        return true;
    }

    /// <summary>Vua bat lua: lua phai con o GOC, chua lan het ca cay.</summary>
    static void DoLucMoiBen()
    {
        if (cc == null) { Ghi("[LOI] mat component CayChay"); loi++; return; }

        var vfx = GameObject.Find("VfxCayChay");
        int soHat = vfx != null ? vfx.GetComponentsInChildren<ParticleSystem>().Length : 0;
        int soDen = vfx != null ? vfx.GetComponentsInChildren<Light>().Length : 0;

        Ghi(string.Format("MOI BEN: {0} diem moi lua, lan het mat {1:F1}s, ban kinh lua goc {2:F2}m",
                          cc.SoDiem, cc.thoiGianLan, cc.banKinhLua));
        Ghi(string.Format("         he hat={0} den={1}, tien do lan = {2:P0}, dang chay {3}/{4} diem",
                          soHat, soDen, cc.TienDoLan, cc.SoDiemDangChay, cc.SoDiem));

        if (cc.SoDiem < 100) { Ghi("[LOI] qua it diem moi lua - lua se khong phu het cay"); loi++; }
        // Bon lop: LuaCanh, KhoiCanh, LuaGoc, TanCayChay
        if (soHat < 4) { Ghi("[LOI] thieu he hat cho cay chay"); loi++; }
        if (cc.TienDoLan >= 1f) { Ghi("[LOI] lua lan het ngay tu dau - khong co lan dan gi ca"); loi++; }
    }

    static void DemManh(string nhan)
    {
        int n = 0;
        foreach (var g in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude,
                                                              FindObjectsSortMode.None))
            if (g.name == "ManhCayChay") n++;

        float mau = quai != null ? quai.health : -1f;
        Ghi(string.Format("{0}: manh dang roi = {1}   dang chay {2}/{3} diem   mau quai = {4:F1}",
                          nhan, n, cc != null ? cc.SoDiemDangChay : -1,
                          cc != null ? cc.SoDiem : -1, mau));
        if (n == 0) { Ghi("[LOI] khong thay manh cay nao rot xuong"); loi++; }

        // Than cay den toi dau roi. Doc tu MaterialPropertyBlock cua chinh
        // renderer do - vat lieu goc khong duoc dung toi, no con dung chung cho
        // 57 cai cay khac.
        var r2 = cay != null ? cay.GetComponentInChildren<Renderer>() : null;
        if (r2 != null)
        {
            var mpb = new MaterialPropertyBlock();
            r2.GetPropertyBlock(mpb);
            bool co = mpb.HasColor("_Color");
            Color hien = co ? mpb.GetColor("_Color") : Color.magenta;
            Color goc = r2.sharedMaterial.GetColor("_Color");
            Ghi(string.Format("           mau than: goc {0} -> hien {1}  (con {2:P0} do sang)",
                              goc.ToString("F2"), hien.ToString("F2"),
                              goc.r > 0f ? hien.r / goc.r : 0f));
            if (!co || hien.r >= goc.r) { Ghi("[LOI] than cay KHONG den di"); loi++; }
        }
    }

    /// <summary>
    /// Lua da lan het: MOI diem moi lua deu dang chay, va cai cay VAN CON DUNG.
    ///
    /// Day la phep kiem dung trong tam cua yeu cau - "lua phai lan het toan bo
    /// cay roi moi cho ngon lua bien mat dan".
    /// </summary>
    static void DoLucLanHet()
    {
        int hien = 0;
        var rs = cay.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; i++) if (rs[i].enabled) hien++;

        int hat = 0;
        var vfx = GameObject.Find("VfxCayChay");
        if (vfx != null)
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
                if (ps.name == "LuaCanh") hat += ps.particleCount;

        Ghi(string.Format("LAN HET sau {0:F1}s (du tinh {1:F1}s): dang chay {2}/{3} diem",
                          tLanHet - tChay, cc.thoiGianLan, cc.SoDiemDangChay, cc.SoDiem));
        Ghi(string.Format("        cay VAN CON: renderer bat = {0}/{1}   hat lua bam canh = {2}",
                          hien, rs.Length, hat));

        if (cc.SoDiemDangChay < cc.SoDiem)
        { Ghi("[LOI] bao lan het ma van con diem chua chay"); loi++; }
        if (hien != rs.Length)
        { Ghi("[LOI] cay da bien mat TRUOC khi lua lan het"); loi++; }
        if (hat < 50)
        { Ghi("[LOI] lan het ma tren cay gan nhu khong co hat lua nao"); loi++; }
    }

    static void DoLucRui()
    {
        if (cay == null) return;
        var rs = cay.GetComponentsInChildren<Renderer>(true);
        int bat = 0;
        for (int i = 0; i < rs.Length; i++) if (rs[i].enabled) bat++;

        var cols = cay.GetComponentsInChildren<Collider>(true);
        int colBat = 0;
        for (int i = 0; i < cols.Length; i++) if (cols[i].enabled) colBat++;

        float mau = quai != null ? quai.health : -1f;
        Ghi(string.Format("DA RUI sau {0:F1}s: renderer bat = {1}/{2}  collider bat = {3}/{4}",
                          Time.time - tChay, bat, rs.Length, colBat, cols.Length));
        Ghi(string.Format("        mau quai = {0:F1} / {1:F1}  (mat {2:F1})", mau, mauDau, mauDau - mau));

        if (bat != 0) { Ghi("[LOI] cay VAN HIEN sau khi chay xong"); loi++; }
        if (colBat != 0) { Ghi("[LOI] va cham cua cay van bat sau khi chay xong"); loi++; }
        if (tLanHet < 0f) { Ghi("[LOI] cay rui ma chua bao gio lan het"); loi++; }
        if (quai != null && mau >= mauDau) { Ghi("[LOI] quai dung trong lua ma khong mat mau"); loi++; }
    }

    static void DoGiuaChung(float daChay)
    {
        if (cay == null) return;
        var rs = cay.GetComponentsInChildren<Renderer>(true);
        int bat = 0;
        for (int i = 0; i < rs.Length; i++) if (rs[i].enabled) bat++;
        Ghi(string.Format("giua doan cho ({0:F1}s ke tu luc bat lua): renderer bat = {1}/{2} (phai la 0)",
                          daChay, bat, rs.Length));
        if (bat != 0) { Ghi("[LOI] cay moc lai SOM hon han"); loi++; }
    }

    static void DoLucMocLai(float daChay)
    {
        if (cay == null) return;
        var conCc = cay.GetComponent<CayChay>();
        var rs = cay.GetComponentsInChildren<Renderer>(true);
        int bat = 0;
        for (int i = 0; i < rs.Length; i++) if (rs[i].enabled) bat++;

        var cols = cay.GetComponentsInChildren<Collider>(true);
        int colBat = 0;
        for (int i = 0; i < cols.Length; i++) if (cols[i].enabled) colBat++;

        var r = rs.Length > 0 ? rs[0] : null;
        var mpb = new MaterialPropertyBlock();
        bool coMau = false;
        if (r != null) { r.GetPropertyBlock(mpb); coMau = mpb.HasColor("_Color"); }

        Ghi(string.Format("MOC LAI sau {0:F1}s ke tu luc bat lua: renderer bat = {1}/{2}  collider bat = {3}/{4}",
                          daChay, bat, rs.Length, colBat, cols.Length));
        Ghi(string.Format("         component CayChay con = {0} (phai False), con de mau chay den = {1} (phai False)",
                          conCc != null, coMau));

        if (bat != rs.Length) { Ghi("[LOI] cay KHONG moc lai du"); loi++; }
        if (colBat != cols.Length) { Ghi("[LOI] va cham khong bat lai"); loi++; }
        if (conCc != null) { Ghi("[LOI] component CayChay khong tu go ra"); loi++; }
        if (coMau) { Ghi("[LOI] cay moc lai ma van con den"); loi++; }
    }

    static void Ket()
    {
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText(Path.Combine(thuMuc, "cay_chay_" + tenMan + (phepThat ? "_phepthat" : "") + ".txt"),
                          bao.ToString());

        EditorApplication.update -= Nhip;
        Application.logMessageReceived -= OnLog;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }

    static void Chup(string ten)
    {
        var cam = Camera.main;
        if (cam == null) return;

        const int W = 1280, H = 720;
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGBHalf);
        var truocTarget = cam.targetTexture;
        var truocActive = RenderTexture.active;

        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();

        cam.targetTexture = truocTarget;
        RenderTexture.active = truocActive;

        File.WriteAllBytes(Path.Combine(thuMuc, tenMan + "_" + ten + ".png"), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(rt);
    }
}
