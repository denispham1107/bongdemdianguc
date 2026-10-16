using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 135b - NUONG KICH THUOC GOC VAT cho dau vet Loc xoay (16/10/2026, nguoi dung: "dau vet de lai chua dung ti le that - goc cay to gap doi cay,
/// bia mo cung vay"). Nguyen nhan do duoc: lay kich thuoc tu VA CHAM - capsule cay co cai rong 3,94 m trong khi than that ~1 m (TREE_oakC_727); luoi bia
/// FBX xoay san nen truc Z cuc bo cua BoxCollider la chieu DUNG (lay "be day" ra 1,7 m); tam nen nha mo lech pivot 0,8-1,3 m; va cham da to hon hinh x1,4.
/// Luc choi KHONG doc duoc luoi (Read/Write TAT) -> do SAN o Editor tu CHINH LUOI cua tung vat trong Act2, ghi Resources/KyNang/LocXoay/KichThuocGoc.txt:
///   khoa (ten@x,z lam tron 0,1) | kieu | tam x | tam z | huong (do, truc dai) | rong (truc dai) | day | cao | ban kinh than | day y
///   - CHAN: dinh trong 0..min(0,3 m, 30% cao) tren day (da: ca vat); HINH CHU NHAT BAO NHO NHAT tren mat ngang -> tam, huong, rong (canh dai) / day.
///   - THAN CAY: dinh o 0,3..1,0 m tren day: tam = trung binh, ban kinh = phan vi 85 khoang cach ngang.
/// Chay lai khi dung lai / doi canh vat Act2.
/// </summary>
public static class NuongKichThuocGoc
{
    public const string DuongRa = "Assets/Resources/KyNang/LocXoay/KichThuocGoc.txt";

    [MenuItem("Diablo 2.5D/135b. Nuong kich thuoc goc vat (dau vet Loc xoay)", false, 225)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[NuongGoc] dang Play"); return; }
        var canh = EditorSceneManager.GetActiveScene();
        if (canh.path != "Assets/Scenes/Act2.unity") { Debug.LogWarning("[NuongGoc] mo Act2 truoc"); return; }
        var sb = new StringBuilder();
        sb.AppendLine("# khoa|kieu|tamX|tamZ|huong|rong|day|cao|banKinhThan|dayY  (NuongKichThuocGoc, menu 135b)");
        int dem = 0; var trung = new HashSet<string>(); int soTrung = 0;
        var nhom = new[] { new[] { "World/Cay", "Cay" }, new[] { "World/BiaMo", "Bia" }, new[] { "World/Da", "Da" }, new[] { "World/NhaMo", "NhaMo" } };
        var vat = new List<KeyValuePair<GameObject, string>>();
        foreach (var n in nhom)
        {
            var cha = GameObject.Find(n[0]);
            if (cha == null) { Debug.LogWarning("[NuongGoc] khong co " + n[0]); continue; }
            foreach (Transform c in cha.transform) vat.Add(new KeyValuePair<GameObject, string>(c.gameObject, n[1]));
        }
        foreach (var lo in Object.FindObjectsByType<LoLuaDa>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (lo.gameObject.scene == canh) vat.Add(new KeyValuePair<GameObject, string>(lo.gameObject, "Lo"));

        foreach (var kv in vat)
        {
            string dong = DoMot(kv.Key, kv.Value);
            if (dong == null) continue;
            string khoa = dong.Substring(0, dong.IndexOf('|'));
            if (!trung.Add(khoa)) soTrung++;
            sb.AppendLine(dong); dem++;
        }
        File.WriteAllText(DuongRa, sb.ToString());
        AssetDatabase.ImportAsset(DuongRa);
        Debug.Log(string.Format("[NuongGoc] da nuong {0} vat -> {1} (khoa trung {2})", dem, DuongRa, soTrung));
    }

    public static string Khoa(GameObject g, Vector3 p) { return g.name + "@" + Mathf.RoundToInt(p.x * 10f) + "," + Mathf.RoundToInt(p.z * 10f); }

    /// <summary>Do mot vat tu LUOI (moi MeshFilter con, khong ke he hat); null neu khong doc duoc.</summary>
    public static string DoMot(GameObject g, string kieu)
    {
        var ws = new List<Vector3>();
        foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            Vector3[] v;
            try { v = mf.sharedMesh.vertices; } catch { continue; }
            var m = mf.transform.localToWorldMatrix;
            foreach (var p in v) ws.Add(m.MultiplyPoint3x4(p));
        }
        if (ws.Count < 4) return null;
        float minY = 1e9f, maxY = -1e9f;
        foreach (var w in ws) { minY = Mathf.Min(minY, w.y); maxY = Mathf.Max(maxY, w.y); }
        float cao = maxY - minY;
        float dai = kieu == "Da" ? cao + 1f : Mathf.Min(0.3f, 0.3f * cao);
        var chan = new List<Vector2>();
        foreach (var w in ws) if (w.y <= minY + dai) chan.Add(new Vector2(w.x, w.z));
        if (chan.Count < 3) foreach (var w in ws) chan.Add(new Vector2(w.x, w.z));
        // HINH CHU NHAT BAO NHO NHAT (quay 0-90 do moi 1 do) - ban dau dung PCA: hop co dinh don o 4 goc thi PCA chon truc CHEO 45 do -> rong / day ra
        // (w + d) / can2 moi canh, dau vet xoay lech 45 do va to hon (menu 135 F lan 3)
        float tot = 1e9f, gocTot = 0f, ra0 = 0, ra1 = 0, rb0 = 0, rb1 = 0;
        for (int gd = 0; gd < 90; gd++)
        {
            float r = gd * Mathf.Deg2Rad, c = Mathf.Cos(r), sn = Mathf.Sin(r);
            float a0 = 1e9f, a1 = -1e9f, b0 = 1e9f, b1 = -1e9f;
            foreach (var p in chan) { float u = p.x * c + p.y * sn, v = -p.x * sn + p.y * c; a0 = Mathf.Min(a0, u); a1 = Mathf.Max(a1, u); b0 = Mathf.Min(b0, v); b1 = Mathf.Max(b1, v); }
            float dt = (a1 - a0) * (b1 - b0);
            if (dt < tot) { tot = dt; gocTot = r; ra0 = a0; ra1 = a1; rb0 = b0; rb1 = b1; }
        }
        Vector2 a = new Vector2(Mathf.Cos(gocTot), Mathf.Sin(gocTot)), b = new Vector2(-a.y, a.x);
        if (rb1 - rb0 > ra1 - ra0) { var t0 = a; a = b; b = -t0; float x0 = ra0, x1 = ra1; ra0 = rb0; ra1 = rb1; rb0 = -x1; rb1 = -x0; }   // truc a = canh DAI
        Vector2 tam = a * (ra0 + ra1) * 0.5f + b * (rb0 + rb1) * 0.5f;
        float rong = ra1 - ra0, day = rb1 - rb0;
        // huong Unity: Euler(0, h, 0) * right = (cos h, 0, -sin h) -> h = atan2(-az, ax)
        float huong = Mathf.Atan2(-a.y, a.x) * Mathf.Rad2Deg;
        float bk = 0f;
        if (kieu == "Cay")
        {
            Vector2 t2 = Vector2.zero; int n = 0;
            foreach (var w in ws) if (w.y > minY + 0.3f && w.y < minY + 1.0f) { t2 += new Vector2(w.x, w.z); n++; }
            if (n >= 6)
            {
                t2 /= n;
                var ds = new List<float>();
                foreach (var w in ws) if (w.y > minY + 0.3f && w.y < minY + 1.0f) ds.Add((new Vector2(w.x, w.z) - t2).magnitude);
                ds.Sort();
                bk = ds[Mathf.Min(ds.Count - 1, (int)(ds.Count * 0.85f))];
                tam = t2;
            }
            else bk = Mathf.Min(rong, day) * 0.5f;
        }
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        return string.Format(ci, "{0}|{1}|{2:F3}|{3:F3}|{4:F1}|{5:F3}|{6:F3}|{7:F3}|{8:F3}|{9:F3}", Khoa(g, g.transform.position), kieu, tam.x, tam.y, huong, rong, day, cao, bk, minY);
    }
}
