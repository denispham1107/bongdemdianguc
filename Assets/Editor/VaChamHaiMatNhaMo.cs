using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// VA CHAM HAI MAT CHO NHA MO ACT2 - menu 88.
///
/// Nguoi dung (27/09/2026): "nguoi choi da vao trong nha roi thi khi di ra bi ket khong ra duoc".
/// Nguyen nhan (menu 87 do): luoi nha mo nhap tu Blender co nhieu mat bi LAT PHAP TUYEN vao trong (ban tia tu ngoai vao:
/// MAUS_A 15%, MAUS_B 32% lan cham dau la mat lung; hang rao 0%). CharacterController chi va cham mat THUAN, nen di tu
/// ngoai vao XUYEN qua tuong duoc, con tu trong di ra thi dung mat thuan - ket. 7/7 nha, 18 lan ket / 22 lan vao.
///
/// Sua: luoi VA CHAM rieng = cung dinh, moi tam giac them mot ban dao nguoc -> chan ca hai phia. Chi gan vao MeshCollider;
/// MeshFilter (hinh, diem moi lua theo ten luoi hinh) giu nguyen; hop bao khong doi nen luat "vat nho" cua qua cau van nhu cu.
/// Luoi luu o Assets/BlenderMaps/GraveyardAct2/VaCham/. Chay lai bao nhieu lan cung duoc (ghi de asset).
/// </summary>
public static class VaChamHaiMatNhaMo
{
    public const string ThuMuc = "Assets/BlenderMaps/GraveyardAct2/VaCham";
    public const string DuoiTen = "_VaChamHaiMat";

    [MenuItem("Diablo 2.5D/88. Va cham hai mat cho nha mo (Act2)", false, 177)]
    public static void Chay()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Act2") { Debug.LogError("[VaCham] Mo canh Act2 truoc da."); return; }
        var bao = new StringBuilder();
        if (!AssetDatabase.IsValidFolder(ThuMuc)) AssetDatabase.CreateFolder("Assets/BlenderMaps/GraveyardAct2", "VaCham");
        var daLam = new Dictionary<Mesh, Mesh>();
        var nhom = GameObject.Find("World").transform.Find("NhaMo");
        int soGan = 0;
        foreach (var mc in nhom.GetComponentsInChildren<MeshCollider>(true))
        {
            var goc = mc.sharedMesh;
            if (goc == null) continue;
            if (goc.name.EndsWith(DuoiTen))
            {
                // da gan tu lan truoc - lay lai luoi goc tu MeshFilter de lam lai cho moi
                var mf = mc.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                goc = mf.sharedMesh;
            }
            Mesh hai;
            if (!daLam.TryGetValue(goc, out hai))
            {
                hai = LamHaiMat(goc);
                string duong = ThuMuc + "/" + goc.name + DuoiTen + ".asset";
                var cu = AssetDatabase.LoadAssetAtPath<Mesh>(duong);
                if (cu != null) { EditorUtility.CopySerialized(hai, cu); Object.DestroyImmediate(hai); hai = cu; EditorUtility.SetDirty(cu); }
                else AssetDatabase.CreateAsset(hai, duong);
                daLam[goc] = hai;
                bao.AppendLine(string.Format("{0}: {1} dinh, {2} -> {3} tam giac -> {4}", goc.name, goc.vertexCount,
                    goc.triangles.Length / 3, hai.triangles.Length / 3, duong));
            }
            Undo.RecordObject(mc, "Va cham hai mat");
            mc.sharedMesh = hai;
            soGan++;
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        bao.AppendLine("gan cho " + soGan + " nha mo, luu Act2");
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText("PlayTestShots/va_cham_hai_mat_nha_mo.txt", bao.ToString());
        Debug.Log("[VaCham]\n" + bao);
    }

    /// <summary>Cung dinh, tam giac goc + ban dao chieu (gop moi submesh thanh mot - va cham khong can vat lieu).</summary>
    static Mesh LamHaiMat(Mesh goc)
    {
        var dinh = goc.vertices;
        var tg = new List<int>();
        for (int s = 0; s < goc.subMeshCount; s++) tg.AddRange(goc.GetTriangles(s));
        int n = tg.Count;
        for (int i = 0; i < n; i += 3) { tg.Add(tg[i]); tg.Add(tg[i + 2]); tg.Add(tg[i + 1]); }
        var m = new Mesh();
        m.name = goc.name + DuoiTen;
        m.indexFormat = dinh.Length > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        m.vertices = dinh;
        m.SetTriangles(tg, 0);
        m.RecalculateBounds();
        return m;
    }
}
