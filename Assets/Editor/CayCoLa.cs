using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// DOI 30% CAY TRO CANH ACT2 THANH CAY CO LA (17/10/2026, nguoi dung: "cay khong co la nhieu qua, cho 30% so cay khong co la doi thanh
/// cay co la"; chon: 17 cay, DUNG GIONG, rai deu; chi Act2 - Man chinh giu nguyen).
///
/// Act2 co 56 cay tro (6 cay chet LIB_023 khong co ban co la -> giu) + 15 cay co la. 30% x 56 = 17 cay, chia theo giong bang phan du lon nhat
/// tren 50 cay tro co ban co la: soi C 8 · soi A 6 · soi B 3 (cay manh chi 1 cay, phan du nho nhat -> 0).
/// Chon RAI DEU: moi buoc lay cay (trong giong con suat) XA NHAT so voi moi cay co la (15 cay san + cay vua chon) -> khong don mot goc.
/// Doi: luoi LIB_0xx_TREE_*_bare -> TREE_*_conhanh (cung giong, cung goc ở day than - min z bang nhau), vat lieu [vo, la], CapsuleCollider chep tu
/// mot cay co la SAN cua giong do (toa do cuc bo luoi), ten bo "_bare" theo quy uoc cay co la. Giu vi tri, huong, ti le.
/// Sau do PHAI chay lai menu 135b (KichThuocGoc theo ten + luoi). Diem moi lua (menu 20) da co bo cho luoi conhanh.
/// Chay MOT lan: da du 32 cay co la thi tu chan.
/// </summary>
public static class CayCoLa
{
    const string ThuMuc = "Assets/BlenderMaps/GraveyardAct2/CayDaChua/";
    public const int SoCayDoi = 17;

    [MenuItem("Diablo 2.5D/144 Doi 30% cay tro thanh cay co la (Act2)")]
    static void Menu() { Debug.Log(Doi()); }

    static string Giong(string luoi)
    {
        foreach (var g in new[] { "oakA", "oakB", "oakC", "thin" }) if (luoi.Contains("TREE_" + g)) return g;
        return null;
    }

    public static string Doi()
    {
        if (EditorApplication.isPlaying) return "dang Play";
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.path.EndsWith("Act2.unity")) return "phai mo Act2";
        var bao = new StringBuilder();
        var vo = AssetDatabase.LoadAssetAtPath<Material>(ThuMuc + "Act2_VoCay_SanSui.mat");
        var tro = new List<MeshRenderer>(); var coLa = new List<MeshRenderer>();
        var mau = new Dictionary<string, MeshRenderer>();   // cay co la SAN theo giong
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
        {
            if (r.sharedMaterial != vo) continue;
            var m = r.GetComponent<MeshFilter>().sharedMesh;
            string g = Giong(m.name);
            if (r.sharedMaterials.Length >= 2) { coLa.Add(r); if (g != null && !mau.ContainsKey(g)) mau[g] = r; }
            else if (g != null && m.name.EndsWith("_bare")) tro.Add(r);
        }
        bao.AppendLine("truoc: co la " + coLa.Count + ", tro co ban co la " + tro.Count);
        if (coLa.Count >= 15 + SoCayDoi) return "da doi roi (co la " + coLa.Count + ")";
        bao.AppendLine(KhoangCach("truoc", vo));

        // suat moi giong: phan du lon nhat
        var demGiong = new Dictionary<string, int>();
        foreach (var r in tro) { string g = Giong(r.GetComponent<MeshFilter>().sharedMesh.name); demGiong[g] = demGiong.ContainsKey(g) ? demGiong[g] + 1 : 1; }
        var suat = new Dictionary<string, int>(); var du = new List<KeyValuePair<string, float>>(); int tong = 0;
        foreach (var kv in demGiong)
        {
            float q = SoCayDoi * kv.Value / (float)tro.Count;
            suat[kv.Key] = Mathf.FloorToInt(q); tong += suat[kv.Key]; du.Add(new KeyValuePair<string, float>(kv.Key, q - suat[kv.Key]));
        }
        du.Sort((a, b) => b.Value.CompareTo(a.Value));
        for (int i = 0; tong < SoCayDoi; i++, tong++) suat[du[i].Key]++;
        foreach (var kv in suat) bao.AppendLine("  suat " + kv.Key + ": " + kv.Value + " / " + demGiong[kv.Key]);

        // rai deu: xa nhat so voi moi cay co la
        var diemLa = new List<Vector2>();
        foreach (var r in coLa) diemLa.Add(XZ(r));
        tro.Sort((a, b) => a.name.CompareTo(b.name));
        var chon = new List<MeshRenderer>();
        var conLai = new List<MeshRenderer>(tro);
        while (chon.Count < SoCayDoi)
        {
            MeshRenderer tot = null; float xa = -1;
            foreach (var r in conLai)
            {
                string g = Giong(r.GetComponent<MeshFilter>().sharedMesh.name);
                if (suat[g] <= 0) continue;
                float d = float.MaxValue; Vector2 p = XZ(r);
                foreach (var q in diemLa) d = Mathf.Min(d, Vector2.Distance(p, q));
                if (d > xa) { xa = d; tot = r; }
            }
            chon.Add(tot); conLai.Remove(tot); diemLa.Add(XZ(tot));
            suat[Giong(tot.GetComponent<MeshFilter>().sharedMesh.name)]--;
        }

        foreach (var r in chon)
        {
            var mf = r.GetComponent<MeshFilter>();
            string g = Giong(mf.sharedMesh.name);
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(ThuMuc + "TREE_" + g + "_conhanh.asset");
            var mauR = mau[g];
            string tenCu = r.name;
            Undo.RecordObject(mf, "cay co la"); Undo.RecordObject(r, "cay co la"); Undo.RecordObject(r.gameObject, "cay co la");
            mf.sharedMesh = mesh;
            r.sharedMaterials = mauR.sharedMaterials;
            r.shadowCastingMode = mauR.shadowCastingMode; r.receiveShadows = mauR.receiveShadows;
            var cc = r.GetComponent<CapsuleCollider>(); var ccM = mauR.GetComponent<CapsuleCollider>();
            if (cc != null && ccM != null)
            {
                Undo.RecordObject(cc, "cay co la");
                cc.center = ccM.center; cc.radius = ccM.radius; cc.height = ccM.height; cc.direction = ccM.direction;
            }
            r.gameObject.layer = mauR.gameObject.layer;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, GameObjectUtility.GetStaticEditorFlags(mauR.gameObject));
            r.name = tenCu.Replace("_bare", "");
            Vector2 p = XZ(r);
            bao.AppendLine(string.Format("  {0} -> {1} ({2:0.0}, {3:0.0}), cao {4:0.00} m", tenCu, r.name, p.x, p.y, r.bounds.size.y));
        }
        // khoang cach toi cay co la gan nhat: truoc / sau
        bao.AppendLine(KhoangCach("sau", vo));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        File.WriteAllText("PlayTestShots/caycola.txt", bao.ToString());
        return bao.ToString();
    }

    /// <summary>
    /// Do tai: 25 cho (luoi 25 m) x 2 goc may quay game (3D tu do: nghieng 22, 7,5 m, fov 55 · 2.5D: nghieng 48, 13 m, fov 36; xa = may quay chinh
    /// cua scene) -> so DINH luoi cay co khung bao trong khung hinh (TB / lon nhat) + so cay. Chua tinh luot bong do (cung so dinh nua).
    /// </summary>
    public static string DoDinh(string nhan)
    {
        var vo = AssetDatabase.LoadAssetAtPath<Material>(ThuMuc + "Act2_VoCay_SanSui.mat");
        var cay = new List<MeshRenderer>();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude)) if (r.sharedMaterial == vo) cay.Add(r);
        float xa = Camera.main != null ? Camera.main.farClipPlane : 300f;
        var go = new GameObject("TAM_DoDinh"); go.hideFlags = HideFlags.HideAndDontSave;
        var cam = go.AddComponent<Camera>(); cam.enabled = false; cam.nearClipPlane = 0.3f; cam.farClipPlane = xa; cam.aspect = 16f / 9f;
        var mp = new Plane[6];
        float[][] goc = { new[] { 22f, 7.5f, 55f, 1.1f }, new[] { 48f, 13f, 36f, 1.0f } };
        string[] tenGoc = { "3D tu do", "2.5D" };
        var sb = new StringBuilder();
        long tongAll = 0; foreach (var r in cay) tongAll += r.GetComponent<MeshFilter>().sharedMesh.vertexCount;
        sb.AppendLine(string.Format("{0}: {1} cay, tong dinh luoi cay ca man {2:N0} (xa may quay {3} m)", nhan, cay.Count, tongAll, xa));
        for (int k = 0; k < 2; k++)
        {
            double tb = 0, tbCay = 0; long lon = 0; int n = 0;
            for (float x = -50; x <= 50; x += 25) for (float z = -50; z <= 50; z += 25)
                for (int h = 0; h < 4; h++)
                {
                    Vector3 dich = new Vector3(x, 0, z); dich.y = Terrain.activeTerrain != null ? Terrain.activeTerrain.SampleHeight(dich) + Terrain.activeTerrain.transform.position.y : 0;
                    Vector3 nhin = dich + Vector3.up * goc[k][3];
                    var q = Quaternion.Euler(goc[k][0], 45f + 90f * h, 0);
                    go.transform.rotation = q; go.transform.position = nhin - q * Vector3.forward * goc[k][1];
                    cam.fieldOfView = goc[k][2];
                    GeometryUtility.CalculateFrustumPlanes(cam, mp);
                    long dinh = 0; int soCay = 0;
                    foreach (var r in cay) if (GeometryUtility.TestPlanesAABB(mp, r.bounds)) { dinh += r.GetComponent<MeshFilter>().sharedMesh.vertexCount; soCay++; }
                    tb += dinh; tbCay += soCay; lon = System.Math.Max(lon, dinh); n++;
                }
            sb.AppendLine(string.Format("  {0}: dinh cay trong khung TB {1:N0} lon nhat {2:N0}; so cay TB {3:0.0} ({4} khung)", tenGoc[k], tb / n, lon, tbCay / n, n));
        }
        Object.DestroyImmediate(go);
        return sb.ToString();
    }

    static Vector2 XZ(Renderer r) { var c = r.transform.position; return new Vector2(c.x, c.z); }

    /// <summary>Do rai deu: moi diem luoi 4 m trong |x|,|z| &lt;= 60 -> khoang cach toi cay co la gan nhat (TB / lon nhat).</summary>
    public static string KhoangCach(string nhan, Material vo)
    {
        var la = new List<Vector2>(); int tro = 0;
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
            if (r.sharedMaterial == vo) { if (r.sharedMaterials.Length >= 2) la.Add(XZ(r)); else tro++; }
        double tb = 0; float lon = 0; int n = 0;
        for (float x = -60; x <= 60; x += 4) for (float z = -60; z <= 60; z += 4)
        {
            float d = float.MaxValue; foreach (var q in la) d = Mathf.Min(d, Vector2.Distance(new Vector2(x, z), q));
            tb += d; lon = Mathf.Max(lon, d); n++;
        }
        return string.Format("{0}: co la {1}, tro {2}; tu moi cho toi cay co la gan nhat TB {3:0.0} m, xa nhat {4:0.0} m", nhan, la.Count, tro, tb / n, lon);
    }
}
