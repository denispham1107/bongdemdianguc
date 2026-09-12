using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: LOC XOAY DAP TAT LO LUA ROI CUON CAI LO DI.
///
/// Nguoi dung xin (12/09/2026): loc xoay cua MOI nguoi choi khi trung lo lua thi
///   1. dap tat lua trong lo TRUOC,
///   2. roi cuon cai lo bay theo giong cay va bia mo,
///   3. 30 giay sau lo hien lai va chay tiep.
///
/// Phep thu vao Play THAT o Act2, tha mot con loc di thang vao mot cai lo va
/// theo doi tung khung hinh:
///   - luc lua tat  (LoLuaDa.DangChay tu true sang false)
///   - luc lo nhac khoi mat dat (y cao hon cho cu 0,3 m)
///   - luc lo bien mat (Renderer tat) va luc lo hien lai + chay tiep
/// Thu tu hai moc dau la CHUYEN PHAI CHUNG MINH: tat lua phai xay ra TRUOC.
///
/// Doc thang trang thai that (component, Renderer, ParticleSystem) chu khong doc
/// lai ham luat cua chinh cho sua - luat bao "dung" thi cung khong chung minh gi.
///
/// Ket qua ghi ra <c>PlayTestShots/locxoay_lolua.txt</c>, anh <c>locxoay_*.png</c>.
/// </summary>
public static class ThuLocXoayLoLua
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/54c. Chay thu LOC XOAY cuon lo lua", false, 145)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu loc xoay + lo lua",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 3] loc xoay dap tat lua roi cuon lo");

        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != DatLoLuaAct2.CanhAct2)
            EditorSceneManager.OpenScene(DatLoLuaAct2.CanhAct2, OpenSceneMode.Single);

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_LocXoayLoLua");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[LocLo] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }

    static IEnumerator KichBan()
    {
        float han = Time.time + 25f;
        PlayerController toi = null;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (!EditorApplication.isPlaying) { Ghi("[bo] Editor thoat Play"); Ket(); yield break; }

        var nhom = GameObject.Find(DatLoLuaAct2.TenNhom);
        if (nhom == null) { Loi("khong thay nhom " + DatLoLuaAct2.TenNhom); Ket(); yield break; }
        var lo = nhom.transform.GetChild(0);                    // LoLua_GiuaMap
        var loLua = lo.GetComponent<LoLuaDa>();
        var camCu = Camera.main;
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;

        Vector3 choCu = lo.position;
        Ghi("");
        Ghi("lo thu: " + lo.name + " " + choCu.ToString("F2"));
        int hatDau = DemHat(lo);
        Ghi("truoc khi danh: lua chay = " + loLua.DangChay + ", " + hatDau + " hat, "
            + (lo.GetComponentInChildren<Light>() != null ? "co den" : "KHONG co den"));
        if (!loLua.DangChay) Loi("lo chua chay tu dau - phep thu vo nghia");
        NgamCam(camCu, choCu);
        yield return null;
        Chup(camCu, "PlayTestShots/locxoay_1_dang_chay.png");

        // ---- Tha loc xoay di thang vao lo ----
        // Tha cach 7 m de con loc PHAI DI TOI: thay ro thu tu tat lua -> boc lo.
        Vector3 huong = Vector3.forward;
        Vector3 choTha = choCu - huong * 7f;
        choTha.y = VfxFactory.GroundY(choTha);
        var loc = Tornado.Spawn(choTha, huong, LayerMask.GetMask("Enemy"));
        Ghi("tha loc o " + choTha.ToString("F1") + ", di ve huong lo");

        float t0 = Time.time;
        float tTatLua = -1f, tNhacLen = -1f, tBienMat = -1f;
        bool coCuon = false;
        float caoNhat = choCu.y;
        bool daChupTat = false, daChupBay = false;

        while (Time.time - t0 < 12f && EditorApplication.isPlaying)
        {
            if (tTatLua < 0f && !loLua.DangChay) tTatLua = Time.time - t0;
            if (lo.GetComponent<VatTheBiCuon>() != null) coCuon = true;
            caoNhat = Mathf.Max(caoNhat, lo.position.y);
            if (tNhacLen < 0f && lo.position.y > choCu.y + 0.3f) tNhacLen = Time.time - t0;
            var r = lo.GetComponentInChildren<MeshRenderer>(true);
            if (tBienMat < 0f && r != null && !r.enabled) tBienMat = Time.time - t0;

            // Anh 2: lua vua tat, lo VAN CON dung tai cho
            if (!daChupTat && tTatLua >= 0f && tNhacLen < 0f)
            {
                daChupTat = true;
                NgamCam(camCu, choCu);
                Chup(camCu, "PlayTestShots/locxoay_2_lua_vua_tat.png");
            }
            // Anh 3: lo dang bay theo loc
            if (!daChupBay && lo.position.y > choCu.y + 1.5f)
            {
                daChupBay = true;
                NgamCam(camCu, choCu);
                Chup(camCu, "PlayTestShots/locxoay_3_dang_bay.png");
            }
            yield return null;
        }

        Ghi("");
        Ghi("moc thoi gian (giay tu luc tha loc):");
        Ghi("   lua tat      : " + (tTatLua < 0f ? "KHONG BAO GIO" : tTatLua.ToString("F2")));
        Ghi("   lo nhac len  : " + (tNhacLen < 0f ? "KHONG BAO GIO" : tNhacLen.ToString("F2")));
        Ghi("   lo bien mat  : " + (tBienMat < 0f ? "KHONG BAO GIO" : tBienMat.ToString("F2")));
        Ghi("   cao nhat     : " + (caoNhat - choCu.y).ToString("F2") + " m tren cho cu");
        Ghi("   co bi cuon   : " + coCuon);

        if (tTatLua < 0f) Loi("loc di qua ma lua khong tat");
        if (!coCuon || tNhacLen < 0f) Loi("lo khong bi cuon bay theo loc");
        if (tTatLua >= 0f && tNhacLen >= 0f)
        {
            if (tNhacLen < tTatLua) Loi("lo bay len TRUOC khi lua tat (" + tNhacLen.ToString("F2")
                                        + " < " + tTatLua.ToString("F2") + ")");
            else Ghi("   thu tu dung: tat lua truoc, boc lo sau " + (tNhacLen - tTatLua).ToString("F2") + " giay");
        }
        if (caoNhat - choCu.y < 1.5f) Loi("lo chi nhac len " + (caoNhat - choCu.y).ToString("F2") + " m");
        if (loLua.DangChay) Loi("lua van chay trong luc lo bi cuon");

        // ---- Trong luc bien mat ----
        yield return new WaitForSeconds(1f);
        var hinh = lo.GetComponentInChildren<MeshRenderer>(true);
        var vaCham = lo.GetComponent<CapsuleCollider>();
        Ghi("");
        Ghi("luc dang bien mat: hinh bat = " + (hinh != null && hinh.enabled)
            + ", va cham bat = " + (vaCham != null && vaCham.enabled)
            + ", cach cho cu " + Vector3.Distance(lo.position, choCu).ToString("F2") + " m");
        if (hinh != null && hinh.enabled) Loi("lo van hien hinh sau khi bi cuon mat");
        if (vaCham != null && vaCham.enabled) Loi("va cham van bat trong luc lo bien mat");
        if (Vector3.Distance(lo.position, choCu) > 0.05f) Loi("lo khong duoc tra ve cho cu luc an di");

        // ---- Doi moc lai ----
        float hanMoc = Time.time + VatTheBiCuon.GiayHoiSinh + 6f;
        while (Time.time < hanMoc && EditorApplication.isPlaying)
        {
            if (hinh != null && hinh.enabled && loLua.DangChay) break;
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        int hatSau = DemHat(lo);
        bool coDen = lo.GetComponentInChildren<Light>() != null;
        Ghi("");
        Ghi("sau " + VatTheBiCuon.GiayHoiSinh + " giay: hinh bat = " + (hinh != null && hinh.enabled)
            + ", va cham bat = " + (vaCham != null && vaCham.enabled)
            + ", lua chay = " + loLua.DangChay + ", " + hatSau + " hat, " + (coDen ? "co den" : "KHONG co den")
            + ", lech cho cu " + Vector3.Distance(lo.position, choCu).ToString("F2") + " m");
        if (hinh == null || !hinh.enabled) Loi("lo khong moc lai");
        if (vaCham == null || !vaCham.enabled) Loi("va cham khong bat lai");
        if (!loLua.DangChay) Loi("lo moc lai ma khong chay tiep");
        if (hatSau < 3) Loi("lua moc lai chi co " + hatSau + " hat");
        if (!coDen) Loi("lua moc lai khong co den");
        if (Vector3.Distance(lo.position, choCu) > 0.05f) Loi("lo moc lai lech cho cu");
        if (lo.GetComponent<VatTheBiCuon>() != null) Loi("van con VatTheBiCuon sau khi moc lai");
        NgamCam(camCu, choCu);
        yield return null;
        Chup(camCu, "PlayTestShots/locxoay_4_moc_lai.png");

        // ---- Hieu ung khac VAN khong bi cuon ----
        // Sua luat cuon thi de lam lot ca vong phep, dam lua. Dung mot vat gia
        // co he hat + hinh de kiem: no phai VAN bi tu choi.
        var gia = new GameObject("TAM_VatCoHieuUng");
        gia.transform.position = choCu + new Vector3(1f, 0f, 0f);
        var con = GameObject.CreatePrimitive(PrimitiveType.Cube);
        con.transform.SetParent(gia.transform, false);
        Object.DestroyImmediate(con.GetComponent<Collider>());
        gia.AddComponent<ParticleSystem>();
        var loc2 = Tornado.Spawn(choCu + new Vector3(0f, 0f, -2f), Vector3.forward, LayerMask.GetMask("Enemy"));
        bool cuonGia = VatTheBiCuon.CuonDuoc(gia, loc2);
        bool cuonLoDangChay = VatTheBiCuon.CuonDuoc(lo.gameObject, loc2);
        Ghi("");
        Ghi("vat co hieu ung (khong phai lo): cuon duoc = " + cuonGia + " (phai la False)");
        Ghi("lo DANG CHAY: cuon duoc = " + cuonLoDangChay + " (phai la False - phai tat lua truoc)");
        if (cuonGia) Loi("luat moi lam lot vat co hieu ung");
        if (cuonLoDangChay) Loi("lo dang chay ma da cuon duoc");
        Object.DestroyImmediate(gia);
        if (loc2 != null) Object.Destroy(loc2.gameObject);

        // ---- Than trong chau co thuc su TAT khong ----
        // Doc mau tren ANH chu khong doc thuoc tinh vat lieu: dat mot lo khac
        // (khong co con loc che) truoc camera, do do sang vung mieng chau luc
        // dang chay va luc vua dap tat.
        if (nhom.transform.childCount > 1)
        {
            var lo2 = nhom.transform.GetChild(1);
            var lua2 = lo2.GetComponent<LoLuaDa>();
            NgamCam(camCu, lo2.position);
            yield return null;
            float sangChay = Chup(camCu, "PlayTestShots/locxoay_5_than_do.png");
            lua2.DapTat();
            yield return new WaitForSeconds(5f);      // cho cuon khoi tan het
            NgamCam(camCu, lo2.position);
            yield return null;
            float sangTat = Chup(camCu, "PlayTestShots/locxoay_6_than_nguoi.png");
            Ghi("");
            Ghi("do sang vung mieng chau (" + lo2.name + "): dang chay " + sangChay.ToString("F1")
                + " -> vua tat " + sangTat.ToString("F1")
                + " (giam " + (100f * (1f - sangTat / Mathf.Max(0.01f, sangChay))).ToString("F0") + "%)");
            if (sangTat > sangChay * 0.5f) Loi("tat lua ma vung mieng chau van sang gan nhu cu - than con do?");
            lua2.Chay();
        }

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static int DemHat(Transform lo)
    {
        int n = 0;
        foreach (var ps in lo.GetComponentsInChildren<ParticleSystem>()) n += ps.particleCount;
        return n;
    }

    static void NgamCam(Camera cam, Vector3 cho)
    {
        if (cam == null) return;
        cam.transform.position = cho + new Vector3(0f, 6.5f, -8.5f);
        cam.transform.LookAt(cho + Vector3.up * 1.4f);
    }

    /// <summary>Chup, va tra ve DO SANG trung binh (0..255) o vung giua anh - cho mieng chau.</summary>
    static float Chup(Camera cam, string duong)
    {
        if (cam == null) return 0f;
        const int W = 960, H = 540;
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
        var cuRt = cam.targetTexture;
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = cuRt;
        RenderTexture.active = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0); tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(duong, tex.EncodeToPNG());

        // O do OM SAT MIENG CHAU (camera dat co dinh boi NgamCam): rong ra thi
        // mat dat chung quanh - von khong doi - lam loang ket qua.
        float tong = 0f; int dem = 0;
        for (int x = W / 2 - 40; x < W / 2 + 40; x++)
            for (int y = H / 2 - 20; y < H / 2 + 30; y++)
            {
                var c = tex.GetPixel(x, y);
                tong += (c.r * 0.6f + c.g * 0.3f + c.b * 0.1f) * 255f; dem++;
            }
        float sang = dem > 0 ? tong / dem : 0f;

        Object.DestroyImmediate(tex); rt.Release(); Object.DestroyImmediate(rt);
        Ghi("anh: " + duong + " (do sang giua " + sang.ToString("F1") + ")");
        return sang;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[LocLo] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/locxoay_lolua.txt", bao.ToString());
        var rac = GameObject.Find("TAM_LocXoayLoLua");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
