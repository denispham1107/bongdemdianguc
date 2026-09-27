using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU RA - VAO NHA MO (Act2) - menu 87.
///
/// Nguoi dung (27/09/2026): "nguoi choi da vao trong nha roi thi khi di ra bi ket khong ra duoc".
/// Do BANG NHAN VAT THAT (bom GoiInput, CharacterController that), di y het nguoi choi:
///   dat nhan vat ngoai nha 1,6 m o 8 huong, di thang vao tam 5 giay; neu toi duoc DUOI MAI thi tu CHINH cho ay thu di ra
///   5 huong: quay LUI dung duong vao truoc, roi lech +-45 va +-90 do (3,5 giay moi huong). Khong huong nao ra duoc = KET.
/// Hai luot:
///   1. DOI CHUNG - tam gan lai luoi va cham GOC (luoi hinh) cho moi nha, CHI trong Play, khong luu: phai thay KET (> 0),
///      khong thi phep thu da mat kha nang bat loi;
///   2. BAN DA SUA - luoi va cham HAI MAT (menu 88): KET phai = 0.
/// Ket qua: PlayTestShots/ra_khoi_nha_mo.txt.
/// </summary>
public static class ThuRaKhoiNhaMo
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    /// <summary>Bo luot doi chung luoi goc (chi do ban da sua, ~5 phut thay vi ~20). Tu tat sau moi lan chay.</summary>
    public static bool BoQuaDoiChung;

    [MenuItem("Diablo 2.5D/87. Chay thu RA - VAO NHA MO (Act2)", false, 176)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 2] ra - vao nha mo Act2 bang nhan vat that (doi chung luoi goc, roi luoi hai mat)");
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
        if (GameObject.Find("TAM_NhaMo") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_NhaMo");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[NhaMo] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

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

    static int matNa;
    static float rNhong, caoNhong;

    static bool VuongTaiCho(Vector3 p)
    {
        return Physics.CheckCapsule(p + Vector3.up * (rNhong + 0.06f), p + Vector3.up * (caoNhong - rNhong), rNhong * 0.98f,
                                    matNa, QueryTriggerInteraction.Ignore);
    }

    /// <summary>Tren dau cho p (chan) co mai cua nha mo col khong.</summary>
    static bool DuoiMai(Vector3 p, Collider col)
    {
        foreach (var h in Physics.RaycastAll(p + Vector3.up * 0.5f, Vector3.up, 12f, ~0, QueryTriggerInteraction.Ignore))
            if (h.collider == col) return true;
        return false;
    }

    static IEnumerator Di(PlayerController toi, Vector3 huong, float giay)
    {
        var goi = GoiInput.Rong(Time.deltaTime);
        goi.huongDi = new Vector3(huong.x, 0f, huong.z).normalized;
        float han = Time.time + giay;
        while (Time.time < han)
        {
            goi.dt = Time.deltaTime;
            toi.input = goi;
            yield return null;
        }
        toi.input = GoiInput.Rong(Time.deltaTime);
        yield return null;
    }

    static int luotVao, luotVaoDuoc, luotKet;

    /// <summary>Mot luot qua ca 7 nha: vao tu 8 huong, vao duoc thi thu ra.</summary>
    static IEnumerator MotLuot(PlayerController toi, string nhan)
    {
        luotVao = 0; luotVaoDuoc = 0; luotKet = 0;
        var nhom = GameObject.Find("World").transform.Find("NhaMo");
        foreach (Transform nha in nhom)
        {
            var col = nha.GetComponentInChildren<Collider>();
            if (col == null) continue;
            Vector3 tam = col.bounds.center;
            var vaoDuoc = new List<string>(); var ket = new List<string>(); var raMotPhan = new List<string>();
            for (int g = 0; g < 8; g++)
            {
                Vector3 h = Quaternion.AngleAxis(g * 45f, Vector3.up) * Vector3.forward;   // tu tam chi RA ngoai
                float bk = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z) + 1.6f;
                Vector3 ngoai = new Vector3(tam.x, 0f, tam.z) + h * bk;
                RaycastHit hd;
                if (!Physics.Raycast(ngoai + Vector3.up * 30f, Vector3.down, out hd, 60f, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore)) continue;
                ngoai.y = hd.point.y + 0.05f;
                if (VuongTaiCho(ngoai)) continue;
                DatCho(toi, ngoai);
                yield return new WaitForSeconds(0.25f);
                yield return Di(toi, -h, 5f);                    // di VAO (ve phia tam)
                luotVao++;
                Vector3 trong = toi.transform.position;
                if (!DuoiMai(trong, col)) continue;
                luotVaoDuoc++; vaoDuoc.Add((g * 45) + "°");
                // thu RA: k = 0 la quay LUI dung duong vao (+h), roi lech +-45, +-90 do
                int raDuoc = 0; string raHuong = "";
                foreach (int k in new[] { 0, 1, -1, 2, -2 })
                {
                    Vector3 hr = Quaternion.AngleAxis(k * 45f, Vector3.up) * h;
                    DatCho(toi, trong);
                    yield return new WaitForSeconds(0.2f);
                    yield return Di(toi, hr, 3.5f);
                    var p = toi.transform.position;
                    if (!DuoiMai(p, col) && new Vector2(p.x - trong.x, p.z - trong.z).magnitude >= 1.2f) { raDuoc++; raHuong += " " + (k * 45); }
                }
                if (raDuoc == 0)
                {
                    string chan = "-";
                    RaycastHit hit;
                    if (Physics.CapsuleCast(trong + Vector3.up * (rNhong + 0.06f), trong + Vector3.up * (caoNhong - rNhong), rNhong * 0.95f, h, out hit, 1.5f, matNa, QueryTriggerInteraction.Ignore))
                        chan = hit.collider.name + " cao hon chan " + (hit.point.y - trong.y).ToString("F2") + " m, phap tuyen " + hit.normal.ToString("F2")
                             + (Vector3.Dot(hit.normal, h) > 0f ? " (quay RA ngoai)" : " (quay VAO trong)");
                    ket.Add(string.Format("vao tu {0}° toi {1}: KET - quay lui cung khong ra; chan boi {2}", g * 45, trong.ToString("F2"), chan));
                }
                else if (raDuoc < 5) raMotPhan.Add(string.Format("vao tu {0}°: ra duoc {1}/5 huong (lech tu duong lui:{2})", g * 45, raDuoc, raHuong));
            }
            luotKet += ket.Count;
            Ghi(string.Format("   {0}: vao duoc duoi mai {1}/8 huong ({2}); KET {3}", nha.name, vaoDuoc.Count, string.Join(" ", vaoDuoc), ket.Count));
            foreach (var s in raMotPhan) Ghi("      " + s);
            foreach (var s in ket) Ghi("      " + s);
        }
        Ghi(string.Format("   => {0}: {1} lan thu vao, vao duoc duoi mai {2}, KET {3}", nhan, luotVao, luotVaoDuoc, luotKet));
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
        var mau = toi.GetComponent<Damageable>(); mau.maxHealth = 1e7f; mau.health = 1e7f;
        bool tuDoc = toi.tuDocInput;
        toi.tuDocInput = false;
        var cc = toi.GetComponent<CharacterController>();
        rNhong = cc.radius * Mathf.Max(toi.transform.lossyScale.x, toi.transform.lossyScale.z);
        caoNhong = cc.height * toi.transform.lossyScale.y;
        matNa = ~(1 << toi.gameObject.layer);
        Ghi(string.Format("nhan vat: CharacterController r {0:F2} cao {1:F2}, stepOffset {2:F2}, slopeLimit {3:F0}", rNhong, caoNhong, cc.stepOffset, cc.slopeLimit));

        // Luoi va cham dang gan (tu canh) va luoi GOC (luoi hinh) cua tung nha
        var nhom = GameObject.Find("World").transform.Find("NhaMo");
        var dangGan = new Dictionary<MeshCollider, Mesh>();
        int haiMat = 0;
        foreach (var mc in nhom.GetComponentsInChildren<MeshCollider>())
        {
            dangGan[mc] = mc.sharedMesh;
            if (mc.sharedMesh != null && mc.sharedMesh.name.EndsWith(VaChamHaiMatNhaMo.DuoiTen)) haiMat++;
        }
        Ghi("nha mo dang dung luoi va cham hai mat (menu 88): " + haiMat + "/" + dangGan.Count);
        Kiem(haiMat == dangGan.Count && haiMat > 0, "con nha mo chua gan luoi va cham hai mat - chay menu 88");

        // 1. DOI CHUNG: luoi goc (bo qua duoc - ca hai luot mat ~20 phut; 27/09/2026 luot nay da do: KET o ca 7 nha)
        int ketGoc = -1;
        Ghi("");
        if (BoQuaDoiChung) Ghi("1. DOI CHUNG - BO QUA lan nay (BoQuaDoiChung)");
        else
        {
            Ghi("1. DOI CHUNG - tam gan luoi va cham GOC (chi trong Play):");
            foreach (var kv in dangGan) kv.Key.sharedMesh = kv.Key.GetComponent<MeshFilter>().sharedMesh;
            Physics.SyncTransforms();
            yield return MotLuot(toi, "luoi goc");
            ketGoc = luotKet;
            Kiem(ketGoc > 0, "doi chung hong: luoi goc ma khong ket lan nao - phep thu khong con bat duoc loi");
        }

        // 2. BAN DA SUA
        Ghi("");
        Ghi("2. LUOI HAI MAT (ban dang chay trong game):");
        foreach (var kv in dangGan) kv.Key.sharedMesh = kv.Value;
        Physics.SyncTransforms();
        yield return MotLuot(toi, "luoi hai mat");
        Kiem(luotKet == 0, "van KET " + luotKet + " lan voi luoi hai mat");

        toi.tuDocInput = tuDoc;
        Ghi("");
        Ghi(string.Format("TONG: luoi goc KET {0} lan; luoi hai mat KET {1} lan", ketGoc, luotKet));
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[NhaMo] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/ra_khoi_nha_mo.txt", bao.ToString());
        BoQuaDoiChung = false;
        var r = GameObject.Find("TAM_NhaMo");
        if (r != null) Object.DestroyImmediate(r);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
