using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 81): THIEN THACH DOT CHAY BIA MO VA NHA MO (nguoi dung 25/09/2026).
///
/// Nguoi dung: "them vao kha nang dot chay moi thu nhu dot chay cay (hieu ung giong y chang)"; chon BIA MO + NHA MO,
/// chay xong "y het cay": lua lan khap be mat, den dan, chay rui BIEN MAT, 30 giay sau moc lai.
///
/// Do bang SO:
///   A. Diem moi lua nuong san: moi luoi bia mo / nha mo trong Act2 deu co bo diem (DOI CHUNG: luoi da, khong chay duoc,
///      khong co bo diem nao); nhan dien: 452 bia + 5 nha "chay duoc", 229 da khong.
///   B. Tung THIEN THACH THAT (CastAt) vao mot bia mo va mot nha mo that: bat lua, dung bo diem nuong (khong phai
///      duong lui 120 diem), lua lan (tien do tang), than den dan (mau _Color giam), chay rui thi tat hinh + va cham,
///      30 giay sau hien lai nguyen mau. Khong vat nao ngoai cay / bia / nha bat lua (DOI CHUNG: da quanh cho roi).
///      So vat chay cung luc khong vuot CayChay.ToiDaCungLuc.
///
/// Ket qua: PlayTestShots/thienthach_dot_bia.txt, anh thienthach_bia_*.png.
/// </summary>
public static class ThuThienThachDotBia
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/81. Chay thu THIEN THACH DOT BIA MO, NHA MO", false, 170)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Thien thach dot chay bia mo, nha mo y het cay");
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
        if (GameObject.Find("TAM_DotBia") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_DotBia");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DotBia] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
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

    static void DatCho(PlayerController pc, Vector3 p)
    {
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    /// <summary>Do sang mau _Color dang ve (MaterialPropertyBlock neu co, khong thi mau vat lieu).</summary>
    static float DoSang(Renderer r)
    {
        if (r == null || r.sharedMaterial == null || !r.sharedMaterial.HasProperty("_Color")) return -1f;
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        Color c = mpb.isEmpty ? r.sharedMaterial.GetColor("_Color") : mpb.GetColor("_Color");
        if (!mpb.isEmpty && c == new Color(0, 0, 0, 0)) c = r.sharedMaterial.GetColor("_Color");
        return (c.r + c.g + c.b) / 3f;
    }

    /// <summary>Vat trong nhom <paramref name="nhom"/> co tran troi thoang (khong co cay trong 7 m) va cho dung cach 8 m.</summary>
    static Transform TimVat(string nhom, float cachCay, out Vector3 choDung)
    {
        choDung = Vector3.zero;
        var g = GameObject.Find(nhom);
        var cay = GameObject.Find("Cay");
        if (g == null) return null;
        for (int i = 0; i < g.transform.childCount; i++)
        {
            var t = g.transform.GetChild(i);
            var r = t.GetComponent<Renderer>();
            if (r == null) continue;
            if (nhom == CayChay.NhomBiaMo && r.bounds.size.y < 1.0f) continue;     // bia qua thap kho nhin
            Vector3 p = r.bounds.center;
            bool ganCay = false;
            if (cay != null)
                for (int k = 0; k < cay.transform.childCount && !ganCay; k++)
                {
                    Vector3 c = cay.transform.GetChild(k).position; c.y = p.y;
                    if (Vector3.Distance(c, p) < cachCay) ganCay = true;
                }
            if (ganCay) continue;
            for (int h = 0; h < 8; h++)
            {
                Vector3 hd = Quaternion.AngleAxis(h * 45f, Vector3.up) * Vector3.forward;
                Vector3 cho = p + hd * 8f;
                cho.y = VfxFactory.GroundY(cho) + 0.2f;
                if (Physics.CheckSphere(cho + Vector3.up * 1f, 0.6f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore)) continue;
                choDung = cho;
                return t;
            }
        }
        return null;
    }

    struct TheoDoi
    {
        public string ten; public Transform vat; public Renderer r; public Collider c;
        public float sangGoc, sangGiua, tienDoGiua, lucBat, thoiGianChay; public int soDiem;
        public bool coBat, daMat, vaChamTat, daHien, mauVe;
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);

        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;

        // ================= A. DIEM MOI LUA + NHAN DIEN =================
        Ghi("");
        foreach (var nhom in new[] { CayChay.NhomBiaMo, CayChay.NhomNhaMo, "Da" })
        {
            var g = GameObject.Find(nhom);
            if (g == null) { Ghi("[LOI] khong co nhom " + nhom); loi++; continue; }
            var luoi = new HashSet<string>();
            int soVat = 0, chayDuoc = 0;
            for (int i = 0; i < g.transform.childCount; i++)
            {
                var t = g.transform.GetChild(i);
                soVat++;
                if (CayChay.ChayDuoc(t.gameObject)) chayDuoc++;
                var mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) luoi.Add(mf.sharedMesh.name);
            }
            int coDiem = 0, itDiem = 0;
            foreach (var l in luoi)
            {
                var d = DiemLuaCay.Nap(l);
                if (d != null && d.diem != null && d.diem.Length > 0) { coDiem++; if (d.diem.Length < 300) itDiem++; }
            }
            Ghi(string.Format("A. nhom '{0}': {1} vat, chay duoc {2}; {3} kieu luoi, co bo diem moi lua {4} (it hon 300 diem: {5})",
                nhom, soVat, chayDuoc, luoi.Count, coDiem, itDiem));
            if (nhom == "Da")
                Kiem(chayDuoc == 0 && coDiem == 0, "doi chung: da cung chay duoc / co bo diem");
            else
                Kiem(chayDuoc == soVat && soVat > 0 && coDiem == luoi.Count && itDiem == 0, "nhom " + nhom + " chua chay duoc het / thieu bo diem moi lua");
        }

        // ================= B. TUNG THIEN THACH THAT =================
        Ghi("");
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(4);
        var vatThu = new List<TheoDoi>();
        int toiDaCungLuc = 0, vatLaChay = 0;
        // LAN LUOT tung vat: tung, theo doi TRON vong doi (bat lua -> den -> chay rui -> 30 giay -> moc lai), chup giua chung
        foreach (var nhom in new[] { CayChay.NhomBiaMo, CayChay.NhomNhaMo })
        {
            Vector3 cho;
            var vat = TimVat(nhom, nhom == CayChay.NhomBiaMo ? 7f : 9f, out cho);
            if (vat == null) { Ghi("[LOI] khong tim duoc " + nhom + " thoang de thu"); loi++; continue; }
            var td = new TheoDoi { ten = nhom + "/" + vat.name, vat = vat, r = vat.GetComponent<Renderer>(), c = vat.GetComponent<Collider>() };
            td.sangGoc = DoSang(td.r);

            DatCho(toi, cho);
            toi.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(td.r.bounds.center - cho, Vector3.up));
            yield return new WaitForSeconds(0.6f);
            toi.mana = toi.maxMana;
            toi.CastAt(4, td.r.bounds.center);
            float hB = Time.time + 4f;
            while (vat.GetComponent<CayChay>() == null && Time.time < hB) yield return null;
            td.coBat = vat.GetComponent<CayChay>() != null;
            td.lucBat = Time.time;
            if (!td.coBat) Ghi("    " + td.ten + " khong bat lua, loi nhac: " + toi.LastMessage);

            bool daChup = false;
            float hanTheo = Time.time + 60f;
            while (td.coBat && Time.time < hanTheo)
            {
                int dang = 0;
                foreach (var c in Object.FindObjectsByType<CayChay>(FindObjectsSortMode.None))
                {
                    dang++;
                    if (!CayChay.ChayDuoc(c.gameObject)) vatLaChay++;
                }
                toiDaCungLuc = Mathf.Max(toiDaCungLuc, dang);

                var cc = td.vat.GetComponent<CayChay>();
                if (cc != null && td.soDiem == 0) { td.soDiem = cc.SoDiem; td.thoiGianChay = cc.thoiGianChay; }
                float tuoi = Time.time - td.lucBat;
                if (cc != null && !daChup && td.thoiGianChay > 0f && tuoi > td.thoiGianChay * 0.45f)
                {
                    daChup = true;
                    // May quay game tam chia thang vao vat (goc may mac dinh co dinh huong, vat hay nam ngoai khung)
                    var cam = Camera.main;
                    var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;
                    Vector3 camP = cam != null ? cam.transform.position : Vector3.zero;
                    Quaternion camQ = cam != null ? cam.transform.rotation : Quaternion.identity;
                    if (rig != null) rig.enabled = false;
                    if (cam != null)
                    {
                        var bao = td.r.bounds;
                        float xa = Mathf.Max(6f, bao.size.magnitude * 1.6f);
                        Vector3 tu = (cho - bao.center); tu.y = 0f; tu = tu.normalized;
                        cam.transform.position = bao.center + tu * xa + Vector3.up * xa * 0.6f;
                        cam.transform.LookAt(bao.center);
                    }
                    yield return null;
                    yield return Chup("thienthach_bia_" + (vatThu.Count + 1) + "_" + nhom);
                    if (cam != null) { cam.transform.position = camP; cam.transform.rotation = camQ; }
                    if (rig != null) rig.enabled = true;
                }
                if (cc != null && !td.mauVe && td.thoiGianChay > 0f && tuoi > td.thoiGianChay * 0.7f)
                {
                    td.mauVe = true;
                    td.sangGiua = DoSang(td.r);
                    td.tienDoGiua = cc.TienDoLan;
                }
                if (!td.daMat && td.r != null && !td.r.enabled) { td.daMat = true; td.vaChamTat = td.c != null && !td.c.enabled; }
                if (td.daMat && td.r.enabled && cc == null) { td.daHien = true; break; }
                yield return null;
            }
            vatThu.Add(td);
            foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        for (int i = 0; i < vatThu.Count; i++)
        {
            var td = vatThu[i];
            float sangSau = DoSang(td.r);
            Ghi(string.Format("B{0}. {1}: bat lua {2}; {3} diem moi lua; chay {4:F1} giay; giua doan chay lan {5:P0}, do sang {6:F3} -> {7:F3} (x{8:F2}); chay rui mat hinh {9}, tat va cham {10}; moc lai {11}, do sang sau khi moc {12:F3}",
                i + 1, td.ten, td.coBat, td.soDiem, td.thoiGianChay, td.tienDoGiua, td.sangGoc, td.sangGiua,
                td.sangGoc > 0f ? td.sangGiua / td.sangGoc : -1f, td.daMat, td.vaChamTat, td.daHien, sangSau));
            Kiem(td.coBat, td.ten + ": Thien thach roi trung ma khong bat lua");
            Kiem(td.soDiem >= 300, td.ten + ": khong dung bo diem moi lua nuong san (" + td.soDiem + " diem)");
            Kiem(td.tienDoGiua > 0.5f, td.ten + ": lua khong lan");
            Kiem(td.sangGoc > 0f && td.sangGiua < td.sangGoc * 0.6f, td.ten + ": khong den dan khi chay");
            Kiem(td.daMat && td.vaChamTat, td.ten + ": chay rui ma khong bien mat / con va cham");
            Kiem(td.daHien && Mathf.Abs(sangSau - td.sangGoc) < 0.01f, td.ten + ": khong moc lai nguyen mau sau 30 giay");
        }
        Ghi(string.Format("B3. so vat chay cung luc nhieu nhat {0} (tran {1}); vat KHONG phai cay / bia / nha bi dot: {2} (phai 0)",
            toiDaCungLuc, CayChay.ToiDaCungLuc, vatLaChay));
        Kiem(toiDaCungLuc <= CayChay.ToiDaCungLuc, "vuot tran so vat chay cung luc");
        Kiem(vatLaChay == 0, "vat khong duoc phep chay (da, hang rao...) lai bat lua");

        foreach (var v in Object.FindObjectsByType<VungLua>(FindObjectsSortMode.None)) Object.Destroy(v.gameObject);
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
        Debug.Log("[DotBia] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/thienthach_dot_bia.txt", bao.ToString());
        var r = GameObject.Find("TAM_DotBia");
        if (r != null) Object.DestroyImmediate(r);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
