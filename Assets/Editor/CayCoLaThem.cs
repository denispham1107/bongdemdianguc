using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// THEM 10 CAY CO LA THEO CHO NGUOI DUNG DANH DAU (17/10/2026, nguoi dung gui anh Scene view co 10 cham do: "khong can chinh xac, gan dung
/// la duoc"). Toa do cham do doi ra the gioi bang CHINH may quay Scene view luc ay (4 goc dat chieu len khop anh +-2 diem anh) -> bang Dich.
///
/// Moi cho: tim diem trong GAN NHAT (xoan oc 0-9 m; cham 2 roi giua dam lay DamLay_11 6 x 6 m -> dat o bo) thoa: trong rao (|x|,|z| &lt;= 61), cach bia / da / nha mo / lo lua >= 2,2 m (khung bao
/// that), cach bui co >= 1 m, cach mat nuoc >= 2,5 m, cach than cay khac >= 5 m, dat duoi than phang (chenh <= 0,25 m trong 1 m). Cay = ban sao
/// mot cay CO LA san cua giong (giu vat lieu vo + la, CapsuleCollider, lop, co tinh), goc chan = dat THAP NHAT trong 0,8 m - 5 cm, xoay ngau
/// nhien, ti le trong khoang ti le cac cay co la cung giong. Giong xen ke: soi C · soi A · soi B · cay manh.
/// Sau do PHAI chay lai menu 141 (dat duoi chan cay moi giu nguyen, dat vun tranh than) roi 142, 135b. Cay nao da co (TREE_*_Tnn) thi bo qua.
/// </summary>
public static class CayCoLaThem
{
    public static readonly Vector2[] Dich = {
        new Vector2(60.3f, 13.3f), new Vector2(59.2f, -8.8f), new Vector2(13.6f, 61.1f), new Vector2(19.3f, -55.0f), new Vector2(-6.7f, 59.5f),
        new Vector2(-9.6f, -55.4f), new Vector2(-33.6f, 59.4f), new Vector2(-32.7f, -51.9f), new Vector2(-59.7f, 18.1f), new Vector2(-59.8f, -9.8f) };
    static readonly string[] GiongLan = { "oakC", "oakA", "oakB", "thin", "oakC", "oakA", "oakC", "oakB", "oakA", "thin" };

    [MenuItem("Diablo 2.5D/145 Them 10 cay co la theo cho danh dau (Act2)")]
    static void Menu() { Debug.Log(Them()); }

    public static string Them()
    {
        if (EditorApplication.isPlaying) return "dang Play";
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.path.EndsWith("Act2.unity")) return "phai mo Act2";
        var ter = Terrain.activeTerrain; float ty = ter.transform.position.y;
        var cay = GameObject.Find("World/Cay").transform;
        // vat can + cay san
        var vatCan = new List<Bounds>(); var bui = new List<Bounds>(); var nuoc = new List<Bounds>(); var than = new List<Vector2>();
        var mau = new Dictionary<string, GameObject>(); var tiLe = new Dictionary<string, Vector2>();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
        {
            var p = r.transform.parent; string cha = p != null ? p.name : "";
            if (cha == "BiaMo" || cha == "Da" || cha == "NhaMo" || r.GetComponentInParent<LoLuaDa>() != null) vatCan.Add(r.bounds);
            else if (cha == "MatNuoc") nuoc.Add(r.bounds);
            else if (r.name.StartsWith("BuiCo") || (p != null && p.name.StartsWith("BuiCo"))) bui.Add(r.bounds);
            else if (cha == "Cay")
            {
                than.Add(new Vector2(r.transform.position.x, r.transform.position.z));
                if (r.sharedMaterials.Length >= 2)
                {
                    string g = null; foreach (var s in new[] { "oakA", "oakB", "oakC", "thin" }) if (r.name.Contains("_" + s + "_")) g = s;
                    if (g == null) continue;
                    if (!mau.ContainsKey(g)) mau[g] = r.gameObject;
                    float k = r.transform.localScale.x;
                    tiLe[g] = tiLe.ContainsKey(g) ? new Vector2(Mathf.Min(tiLe[g].x, k), Mathf.Max(tiLe[g].y, k)) : new Vector2(k, k);
                }
            }
        }
        var bao = new StringBuilder();
        bao.AppendLine(string.Format("vat can {0}, bui co {1}, mat nuoc {2}, cay san {3}", vatCan.Count, bui.Count, nuoc.Count, than.Count));
        var rng = new System.Random(1017);
        for (int i = 0; i < Dich.Length; i++)
        {
            string g = GiongLan[i];
            if (GameObject.Find(string.Format("TREE_{0}_T{1:00}", g, i + 1)) != null) continue;   // da dat o lan truoc
            Vector2 tot = Vector2.zero; bool co = false; float dLech = 0;
            for (float rr = 0; rr <= 9.01f && !co; rr += 0.5f)
            {
                int soGoc = rr == 0 ? 1 : 16;
                for (int a = 0; a < soGoc && !co; a++)
                {
                    float goc = a * Mathf.PI * 2 / soGoc;
                    var c = Dich[i] + new Vector2(Mathf.Cos(goc), Mathf.Sin(goc)) * rr;
                    if (Hop(c, vatCan, bui, nuoc, than, ter, ty)) { tot = c; co = true; dLech = rr; }
                }
            }
            if (!co) { bao.AppendLine("[LOI] khong tim duoc cho cho dau " + i); continue; }
            // goc chan: dat thap nhat trong 0,8 m - 5 cm
            float yMin = 1e9f;
            for (int a = 0; a < 12; a++) for (float rr = 0; rr <= 0.81f; rr += 0.4f)
            {
                var q = tot + new Vector2(Mathf.Cos(a * 0.5236f), Mathf.Sin(a * 0.5236f)) * rr;
                yMin = Mathf.Min(yMin, ter.SampleHeight(new Vector3(q.x, 0, q.y)) + ty);
            }
            var moi = (GameObject)Object.Instantiate(mau[g], cay);
            Undo.RegisterCreatedObjectUndo(moi, "them cay co la");
            moi.name = string.Format("TREE_{0}_T{1:00}", g, i + 1);
            moi.transform.position = new Vector3(tot.x, yMin - 0.05f, tot.y);
            var e = mau[g].transform.eulerAngles; e.y = (float)rng.NextDouble() * 360f;
            moi.transform.eulerAngles = e;
            float k = Mathf.Lerp(tiLe[g].x, tiLe[g].y, (float)rng.NextDouble());
            moi.transform.localScale = Vector3.one * k;
            than.Add(tot);
            bao.AppendLine(string.Format("  dau {0} ({1:0.0}, {2:0.0}) -> {3} ({4:0.0}, {5:0.0}) lech {6:0.0} m, cao {7:0.0} m, ti le {8:0.0}",
                i, Dich[i].x, Dich[i].y, moi.name, tot.x, tot.y, dLech, moi.GetComponent<Renderer>().bounds.size.y, k));
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        File.WriteAllText("PlayTestShots/caycola_them.txt", bao.ToString());
        return bao.ToString();
    }

    static float KcHop(Vector2 c, Bounds b)
    {
        float dx = Mathf.Max(0, Mathf.Max(b.min.x - c.x, c.x - b.max.x)), dz = Mathf.Max(0, Mathf.Max(b.min.z - c.y, c.y - b.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static bool Hop(Vector2 c, List<Bounds> vatCan, List<Bounds> bui, List<Bounds> nuoc, List<Vector2> than, Terrain ter, float ty)
    {
        if (Mathf.Abs(c.x) > 61f || Mathf.Abs(c.y) > 61f) return false;
        foreach (var b in vatCan) if (KcHop(c, b) < 2.2f) return false;
        foreach (var b in bui) if (KcHop(c, b) < 1.0f) return false;
        foreach (var b in nuoc) if (KcHop(c, b) < 2.5f) return false;
        foreach (var t in than) if (Vector2.Distance(c, t) < 5f) return false;
        float lo = 1e9f, hi = -1e9f;
        for (int a = 0; a < 8; a++)
        {
            var q = c + new Vector2(Mathf.Cos(a * 0.785f), Mathf.Sin(a * 0.785f));
            float h = ter.SampleHeight(new Vector3(q.x, 0, q.y)); lo = Mathf.Min(lo, h); hi = Mathf.Max(hi, h);
        }
        return hi - lo <= 0.25f;
    }
}
