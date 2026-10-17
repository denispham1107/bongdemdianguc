using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BIA THANH GIA NUT BE (17/10/2026, nguoi dung: "30% bia mo cay thanh gia bi nut be chi con ngau nhien 1/2 va 2/3 phan than"; chon: phan gay
/// NAM DUOI DAT canh be, mat gay dung bang Blender MCP).
///
/// Buoc 1 (file nay): xuat 4 luoi thanh gia (LIB_005_TS_cross + ban sut me _vo0/1/2) ra OBJ cho Blender MCP (CongCu/ThanhGiaVo/) va chon
/// 10 / 33 bia (30%) RAI DEU (moi buoc lay bia xa nhat cac bia da chon), moi bia gieo 1/2 hoac 2/3 -> PlayTestShots/thanhgiavo_chon.txt.
/// </summary>
public static class ThanhGiaVo
{
    public const string ThuMucRa = "CongCu/ThanhGiaVo";
    public const float TiLeVo = 0.30f;

    public static List<MeshRenderer> CacThanhGia()
    {
        var ds = new List<MeshRenderer>();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
            if (r.transform.parent != null && r.transform.parent.name == "BiaMo" && r.GetComponent<MeshFilter>().sharedMesh.name.StartsWith("LIB_005_TS_cross"))
                ds.Add(r);
        ds.Sort((a, b) => a.name.CompareTo(b.name));
        return ds;
    }

    public static string XuatVaChon()
    {
        Directory.CreateDirectory(ThuMucRa);
        var ds = CacThanhGia();
        var sb = new StringBuilder();
        var daXuat = new HashSet<string>();
        foreach (var r in ds)
        {
            var m = r.GetComponent<MeshFilter>().sharedMesh;
            if (!daXuat.Add(m.name)) continue;
            sb.AppendLine(XuatObj(m, Path.Combine(ThuMucRa, m.name + ".obj")));
        }
        // chon rai deu
        int can = Mathf.RoundToInt(ds.Count * TiLeVo);
        var chon = new List<MeshRenderer>(); var conLai = new List<MeshRenderer>(ds);
        var rng = new System.Random(1717);
        chon.Add(conLai[rng.Next(conLai.Count)]); conLai.Remove(chon[0]);
        while (chon.Count < can)
        {
            MeshRenderer tot = null; float xa = -1;
            foreach (var r in conLai)
            {
                float d = float.MaxValue;
                foreach (var c in chon) d = Mathf.Min(d, Vector3.Distance(r.transform.position, c.transform.position));
                if (d > xa) { xa = d; tot = r; }
            }
            chon.Add(tot); conLai.Remove(tot);
        }
        sb.AppendLine(string.Format("thanh gia {0}, chon {1} (30%)", ds.Count, can));
        foreach (var r in chon)
        {
            bool haiPhanBa = rng.NextDouble() < 0.5;
            sb.AppendLine(string.Format("  {0} | {1} | ({2:0.0}, {3:0.0}) | con {4} | cao {5:0.00} m",
                r.name, r.GetComponent<MeshFilter>().sharedMesh.name, r.transform.position.x, r.transform.position.z, haiPhanBa ? "2/3" : "1/2", r.bounds.size.y));
        }
        File.WriteAllText("PlayTestShots/thanhgiavo_chon.txt", sb.ToString());
        return sb.ToString();
    }

    // ================================================================
    //  BUOC 2: nap luoi Blender MCP (CongCu/Blender/thanh_gia_vo.py -> CongCu/ThanhGiaVo/Ra/*.txt) + ap vao Act2 (menu 146)
    // ================================================================
    public const string ThuMucLuoi = "Assets/Meshes/ThanhGiaVo";
    public const string TenDaVun = "DaVunThanhGia";

    [MenuItem("Diablo 2.5D/146 Bia thanh gia nut be (nap luoi Blender + ap vao Act2)")]
    static void MenuAp() { Debug.Log(ApVaoAct2()); }

    /// <summary>Doc file Blender (moi dong mot goc tam giac: vi tri + phap tuyen, don vi luoi Unity) -> Mesh .asset. Tangent dat VUONG GOC phap
    /// tuyen (shader DaMoTriplanar doi gan qua WorldNormalVector - tangent suy bien la mat gay loang lo).</summary>
    public static Mesh NapLuoi(string ten)
    {
        string duong = Path.Combine(ThuMucRa, "Ra", ten + ".txt");
        var dong = File.ReadAllLines(duong);
        var v = new Vector3[dong.Length]; var n = new Vector3[dong.Length]; var tg = new Vector4[dong.Length]; var tri = new int[dong.Length];
        var ci = CultureInfo.InvariantCulture;
        for (int i = 0; i < dong.Length; i++)
        {
            var p = dong[i].Split(' ');
            v[i] = new Vector3(float.Parse(p[0], ci), float.Parse(p[1], ci), float.Parse(p[2], ci));
            n[i] = new Vector3(float.Parse(p[3], ci), float.Parse(p[4], ci), float.Parse(p[5], ci)).normalized;
            Vector3 t = Vector3.Cross(n[i], Mathf.Abs(n[i].z) < 0.9f ? Vector3.forward : Vector3.right).normalized;
            tg[i] = new Vector4(t.x, t.y, t.z, 1f);
            tri[i] = i;
        }
        Directory.CreateDirectory(ThuMucLuoi);
        string asset = ThuMucLuoi + "/" + ten + ".asset";
        var m = AssetDatabase.LoadAssetAtPath<Mesh>(asset);
        bool moi = m == null; if (moi) m = new Mesh();
        m.Clear(); m.name = ten;
        m.vertices = v; m.normals = n; m.tangents = tg; m.triangles = tri; m.RecalculateBounds();
        if (moi) AssetDatabase.CreateAsset(m, asset); else EditorUtility.SetDirty(m);
        return m;
    }

    struct Chon { public string ten, luoi, phan; }

    static List<Chon> DocDanhSach()
    {
        var ds = new List<Chon>();
        foreach (var d in File.ReadAllLines("PlayTestShots/thanhgiavo_chon.txt"))
        {
            var p = d.Split('|'); if (p.Length < 5) continue;
            ds.Add(new Chon { ten = p[0].Trim(), luoi = p[1].Trim(), phan = p[3].Replace("con", "").Trim().Replace("/", "_") });
        }
        return ds;
    }

    static float DatY(Terrain ter, Vector3 p) { return ter.SampleHeight(p) + ter.transform.position.y; }

    /// <summary>Luoi thanh gia GOC theo ten (ban nguyen trong map_luoi.fbx, ban sut me trong Meshes/Act2Vo).</summary>
    static Mesh LuoiGoc(string ten)
    {
        var a = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Meshes/Act2Vo/" + ten + ".asset");
        if (a != null) return a;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/BlenderMaps/GraveyardAct2/map_luoi.fbx")) if (o is Mesh && o.name == ten) return (Mesh)o;
        return null;
    }

    /// <summary>Tra bia da chon ve nguyen ven: luoi goc, BoxCollider goc, xoa manh gay + da vun (de ap lai kieu gay khac).</summary>
    static void TraLai(Transform biaMo, Chon c)
    {
        var t = biaMo.Find(c.ten); if (t == null) return;
        var g = biaMo.Find(c.ten + "_Gay"); if (g != null) Undo.DestroyObjectImmediate(g.gameObject);
        for (int i = t.childCount - 1; i >= 0; i--) if (t.GetChild(i).name.StartsWith(TenDaVun + "_")) Undo.DestroyObjectImmediate(t.GetChild(i).gameObject);
        var mf = t.GetComponent<MeshFilter>(); var goc = LuoiGoc(c.luoi);
        if (goc != null && mf.sharedMesh != goc) { Undo.RecordObject(mf, "thanh gia vo"); mf.sharedMesh = goc; }
        var bc = t.GetComponent<BoxCollider>();
        if (bc != null && goc != null) { Undo.RecordObject(bc, "thanh gia vo"); bc.center = new Vector3(bc.center.x, bc.center.y, goc.bounds.center.z); bc.size = new Vector3(bc.size.x, bc.size.y, goc.bounds.size.z); }
    }

    /// <summary>Thay luoi 10 bia da chon, dat manh gay nam canh be + da vun. Chay lai duoc: tra ve nguyen ven truoc roi moi ap.
    /// Lan 1 cat ca ban sut me _vo0/_vo1 (luoi ho) -> dam mong, gai chia ngang -> nay moi bia cat tu thanh gia NGUYEN, kieu gay xoay vong
    /// (1/2: a b · 2/3: a b c).</summary>
    public static string ApVaoAct2()
    {
        var sc = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || !sc.path.EndsWith("Act2.unity")) return "phai mo Act2, ngoai Play";
        var biaMo = GameObject.Find("World/BiaMo").transform;
        var daVun = new Mesh[3]; for (int k = 0; k < 3; k++) daVun[k] = NapLuoi(TenDaVun + k);
        // vat can de manh gay tranh
        var vatCan = new List<Bounds>();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
        {
            var p = r.transform.parent; string cha = p != null ? p.name : "";
            if (cha == "BiaMo" || cha == "Da" || cha == "NhaMo" || cha == "MatNuoc" || r.GetComponentInParent<LoLuaDa>() != null) vatCan.Add(r.bounds);
            else if (cha == "Cay") { var c = r.transform.position; vatCan.Add(new Bounds(c + Vector3.up, new Vector3(1.6f, 2f, 1.6f))); }
        }
        var rng = new System.Random(2717);
        var sb = new StringBuilder();
        var dsChon = DocDanhSach();
        foreach (var c in dsChon) TraLai(biaMo, c);
        // vat can tinh lai sau khi tra ve (bo manh gay cu)
        vatCan.Clear();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
        {
            var p = r.transform.parent; string cha = p != null ? p.name : "";
            if (cha == "BiaMo" || cha == "Da" || cha == "NhaMo" || cha == "MatNuoc" || r.GetComponentInParent<LoLuaDa>() != null) vatCan.Add(r.bounds);
            else if (cha == "Cay") { var cc = r.transform.position; vatCan.Add(new Bounds(cc + Vector3.up, new Vector3(1.6f, 2f, 1.6f))); }
        }
        int dem12 = 0, dem23 = 0;
        foreach (var c in dsChon)
        {
            var t = biaMo.Find(c.ten); if (t == null) { sb.AppendLine("[LOI] khong thay " + c.ten); continue; }
            var mf = t.GetComponent<MeshFilter>(); var mr = t.GetComponent<MeshRenderer>();
            string hau = c.phan == "1_2" ? "ab"[dem12++ % 2].ToString() : "abc"[dem23++ % 3].ToString();
            var con = NapLuoi("ThanhGiaVo_con" + c.phan + "_" + hau); var gay = NapLuoi("ThanhGiaVo_gay" + c.phan + "_" + hau);
            float caoCu = mr.bounds.size.y;
            var banThan = mr.bounds;
            Undo.RecordObject(mf, "thanh gia vo"); mf.sharedMesh = con;
            var bc = t.GetComponent<BoxCollider>();
            if (bc != null) { Undo.RecordObject(bc, "thanh gia vo"); float top = con.bounds.max.z; bc.center = new Vector3(bc.center.x, bc.center.y, top * 0.5f); bc.size = new Vector3(bc.size.x, bc.size.y, top); }
            vatCan.RemoveAll(b => b.center == banThan.center && b.size == banThan.size);   // chinh no
            // ---- manh gay nam canh be ----
            float k0 = t.lossyScale.x;
            Vector3 chan = t.position;
            var go = new GameObject(c.ten + "_Gay"); Undo.RegisterCreatedObjectUndo(go, "thanh gia vo");
            go.transform.SetParent(biaMo, false); go.layer = t.gameObject.layer;
            GameObjectUtility.SetStaticEditorFlags(go, GameObjectUtility.GetStaticEditorFlags(t.gameObject));
            go.AddComponent<MeshFilter>().sharedMesh = gay;
            var gr = go.AddComponent<MeshRenderer>(); gr.sharedMaterials = mr.sharedMaterials; gr.shadowCastingMode = mr.shadowCastingMode; gr.receiveShadows = mr.receiveShadows;
            go.transform.localScale = Vector3.one * k0;
            var gv = gay.vertices; float zMin = gay.bounds.min.z;
            float goc0 = (float)rng.NextDouble() * 360f; bool dat = false; string huong = "";
            for (int thu = 0; thu < 12 && !dat; thu++)
            {
                float goc = goc0 + thu * 30f;
                Vector3 d = Quaternion.Euler(0, goc, 0) * Vector3.forward;
                // lan nghieng 15-28 do: tua tren mep canh tay, KHONG nam ap mat phang xuong dat (z-fighting)
                float lan = ((float)rng.NextDouble() < 0.5f ? -1 : 1) * Mathf.Lerp(15f, 28f, (float)rng.NextDouble());
                // LookRotation: z cuc bo (chieu dung cua thanh gia) -> d, y cuc bo (be day) -> len
                Quaternion R = Quaternion.AngleAxis(lan, d) * Quaternion.LookRotation(d, Vector3.up);
                float xa = 0.36f + 0.12f * (float)rng.NextDouble();
                Vector3 ngang = Vector3.Cross(Vector3.up, d) * (((float)rng.NextDouble() - 0.5f) * 0.25f);
                Vector3 P = chan + d * xa + ngang - R * new Vector3(0, 0, zMin * k0);
                go.transform.SetPositionAndRotation(P, R);
                var M = go.transform.localToWorldMatrix;
                var b = new Bounds(M.MultiplyPoint3x4(gv[0]), Vector3.zero); foreach (var q in gv) b.Encapsulate(M.MultiplyPoint3x4(q));
                bool dung = Mathf.Max(Mathf.Abs(b.max.x), Mathf.Abs(b.min.x), Mathf.Abs(b.max.z), Mathf.Abs(b.min.z)) < 62f;
                var bx = b; bx.Expand(new Vector3(0.15f, 20f, 0.15f));
                foreach (var o in vatCan) if (o.Intersects(bx)) { dung = false; break; }
                if (dung) { dat = true; huong = string.Format("huong {0:0} do, lan {1:0} do", goc % 360f, lan); }
            }
            if (!dat) sb.AppendLine("  [CANH BAO] " + c.ten + ": khong co huong trong hoan toan - dung huong cuoi");
            var box = go.AddComponent<BoxCollider>(); box.center = gay.bounds.center; box.size = gay.bounds.size;
            // ---- da vun quanh chan ----
            int soDa = 3 + rng.Next(3);
            for (int k = 0; k < soDa; k++)
            {
                var dv = new GameObject(TenDaVun + "_" + k); Undo.RegisterCreatedObjectUndo(dv, "thanh gia vo");
                dv.transform.SetParent(t, false); dv.layer = t.gameObject.layer;
                dv.AddComponent<MeshFilter>().sharedMesh = daVun[rng.Next(3)];
                var dr = dv.AddComponent<MeshRenderer>(); dr.sharedMaterials = mr.sharedMaterials; dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                float gk = (float)rng.NextDouble() * Mathf.PI * 2, rk = 0.36f + 0.45f * (float)rng.NextDouble();
                dv.transform.position = chan + new Vector3(Mathf.Cos(gk), 0, Mathf.Sin(gk)) * rk;
                dv.transform.rotation = Quaternion.Euler((float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f);
                dv.transform.localScale = Vector3.one;   // ti le the gioi = ti le bia (~100) -> da 4-9 cm
            }
            vatCan.Add(gr.bounds);
            sb.AppendLine(string.Format("  {0}: {1} -> {2} (cao {3:0.00} -> {4:0.00} m), manh gay {5}, {6} da vun", c.ten, c.luoi, con.name, caoCu, mr.bounds.size.y, huong, soDa));
        }
        sb.AppendLine(DatLaiChoDat());
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sc); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(sc);
        File.WriteAllText("PlayTestShots/thanhgiavo_ap.txt", sb.ToString());
        return sb.ToString();
    }

    /// <summary>Ha manh gay / da vun theo mat dat HIEN TAI: dinh thap nhat (so voi dat ngay duoi no) lun 3 cm (manh gay) / da vun lun 35% be cao.
    /// Goi lai sau moi lan menu 141 dung lai dat.</summary>
    public static string DatLaiChoDat()
    {
        var ter = Terrain.activeTerrain; int n = 0; float lechMax = 0;
        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include))
        {
            bool gay = mf.name.EndsWith("_Gay") && mf.transform.parent != null && mf.transform.parent.name == "BiaMo";
            bool da = mf.name.StartsWith(TenDaVun + "_");
            if (!gay && !da) continue;
            var M = mf.transform.localToWorldMatrix; var vs = mf.sharedMesh.vertices;
            float khe = float.MaxValue, y0 = float.MaxValue, y1 = float.MinValue;
            foreach (var q in vs)
            {
                var w = M.MultiplyPoint3x4(q); khe = Mathf.Min(khe, w.y - DatY(ter, w)); y0 = Mathf.Min(y0, w.y); y1 = Mathf.Max(y1, w.y);
            }
            float muon = gay ? -0.03f : -0.35f * (y1 - y0);
            Undo.RecordObject(mf.transform, "dat lai cho dat");
            mf.transform.position += Vector3.up * (muon - khe);
            lechMax = Mathf.Max(lechMax, Mathf.Abs(muon - khe)); n++;
        }
        return string.Format("dat lai cho dat: {0} vat, dich lon nhat {1:0.000} m", n, lechMax);
    }

    static string XuatObj(Mesh m, string duong)
    {
        var v = m.vertices; var n = m.normals; var uv = m.uv; var t = m.triangles;
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("# " + m.name + " (toa do cuc bo luoi Unity, z = chieu dung)");
        foreach (var p in v) sb.AppendLine(string.Format(ci, "v {0:R} {1:R} {2:R}", p.x, p.y, p.z));
        bool coUv = uv != null && uv.Length == v.Length, coN = n != null && n.Length == v.Length;
        if (coUv) foreach (var p in uv) sb.AppendLine(string.Format(ci, "vt {0:R} {1:R}", p.x, p.y));
        if (coN) foreach (var p in n) sb.AppendLine(string.Format(ci, "vn {0:R} {1:R} {2:R}", p.x, p.y, p.z));
        for (int i = 0; i < t.Length; i += 3)
        {
            int a = t[i] + 1, b = t[i + 1] + 1, c = t[i + 2] + 1;
            if (coUv && coN) sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", a, b, c));
            else if (coN) sb.AppendLine(string.Format("f {0}//{0} {1}//{1} {2}//{2}", a, b, c));
            else sb.AppendLine(string.Format("f {0} {1} {2}", a, b, c));
        }
        File.WriteAllText(duong, sb.ToString());
        var bd = m.bounds;
        // ve ngang theo do cao (truc z cuc bo): tim cho canh tay ngang
        var hang = new float[10]; var hangMin = new float[10]; for (int k = 0; k < 10; k++) { hang[k] = -1e9f; hangMin[k] = 1e9f; }
        foreach (var p in v) { int k = Mathf.Clamp((int)((p.z - bd.min.z) / bd.size.z * 10), 0, 9); hang[k] = Mathf.Max(hang[k], p.x); hangMin[k] = Mathf.Min(hangMin[k], p.x); }
        string ngang = ""; for (int k = 0; k < 10; k++) ngang += (hang[k] - hangMin[k]).ToString("0.0000", ci) + " ";
        return string.Format(ci, "{0}: {1} dinh, {2} tam giac, uv {3}, khung {4} -> {5}; ngang x theo 10 lat cao: {6}", m.name, v.Length, t.Length / 3, coUv, bd.min.ToString("F4"), bd.max.ToString("F4"), ngang);
    }
}
