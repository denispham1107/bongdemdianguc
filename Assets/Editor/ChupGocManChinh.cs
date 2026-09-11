using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHUP THU CAC GOC NHIN TRONG ACT2 de chon phong nen cho man hinh chinh.
///
/// Dat nhan vat truoc tung nha mo theo bon huong, bo nhung cho vuong bia/da
/// (o cho nhan vat va hai lo lua) hoac dung giua vung nuoc, roi chup bang DUNG
/// khung hinh camera cua man chinh (cao 1,5 m, lui 4,2 m, cui 6 do, goc 42).
///
/// Anh ra <c>PlayTestShots/goc/goc_*.png</c>. Mo Act2 de chup roi tra lai
/// scene cu - khong luu gi.
/// </summary>
public static class ChupGocManChinh
{
    public struct Goc
    {
        public string ten;
        public Vector3 cho;     // cho nhan vat dung (da tinh do cao mat dat)
        public float huongY;    // camera nhin theo huong nay
        public int vuong;       // so vat can quanh cho dung
    }

    [MenuItem("Diablo 2.5D/51b. Chup thu goc nhin man chinh (Act2)", false, 139)]
    public static void Chay()
    {
        string canhCu = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", OpenSceneMode.Single);
        Directory.CreateDirectory("PlayTestShots/goc");

        var ds = TimGoc();
        var bao = new StringBuilder();
        foreach (var g in ds)
        {
            Chup(g, "PlayTestShots/goc/" + g.ten + ".png", 800, 450);
            bao.AppendLine(string.Format("{0}: cho {1}, huong {2:F0}, vat can {3}", g.ten, g.cho.ToString("F1"), g.huongY, g.vuong));
        }
        File.WriteAllText("PlayTestShots/goc/goc.txt", bao.ToString());
        Debug.Log("[GocManChinh]\n" + bao);

        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    /// <summary>Do cao mat dat (dia hinh) tai x,z.</summary>
    public static float MatDat(float x, float z)
    {
        var ter = Terrain.activeTerrain;
        if (ter == null) ter = Object.FindAnyObjectByType<Terrain>();
        if (ter != null) return ter.SampleHeight(new Vector3(x, 0, z)) + ter.transform.position.y;
        return 0f;
    }

    /// <summary>So vat can (khong tinh dia hinh) trong vung dat nhan vat va hai lo lua.</summary>
    public static int DemVatCan(Vector3 cho, float huongY)
    {
        var q = Quaternion.Euler(0, huongY, 0);
        Vector3 phai = q * Vector3.right, truoc = q * Vector3.forward;
        int dem = 0;
        foreach (var p in new[] { cho, cho + phai * 2.2f + truoc * 1.4f, cho - phai * 2.2f + truoc * 1.4f,
                                   cho - truoc * 2.0f })
        {
            foreach (var c in Physics.OverlapCapsule(p + Vector3.up * 0.3f, p + Vector3.up * 1.8f, 0.9f))
                if (!(c is TerrainCollider)) dem++;
        }
        return dem;
    }

    public static bool TrongNuoc(Vector3 p)
    {
        var nuoc = GameObject.Find("MatNuoc");
        if (nuoc == null) return false;
        foreach (var r in nuoc.GetComponentsInChildren<Renderer>())
        {
            var b = r.bounds; b.Expand(new Vector3(3f, 10f, 3f));
            if (b.Contains(p)) return true;
        }
        return false;
    }

    static List<Goc> TimGoc()
    {
        var ds = new List<Goc>();
        var nhaMo = GameObject.Find("NhaMo");
        int stt = 0;
        foreach (Transform m in nhaMo.transform)
        {
            for (int h = 0; h < 4; h++)
            {
                float goc = h * 90f + m.eulerAngles.y;
                Vector3 raNgoai = Quaternion.Euler(0, goc, 0) * Vector3.forward;
                foreach (float xa in new[] { 9f, 12f })
                {
                    Vector3 cho = m.position + raNgoai * xa;
                    cho.y = MatDat(cho.x, cho.z);
                    float huong = Quaternion.LookRotation(-raNgoai).eulerAngles.y;
                    if (TrongNuoc(cho) || Mathf.Abs(cho.x) > 48f || Mathf.Abs(cho.z) > 48f) continue;
                    int vc = DemVatCan(cho, huong);
                    if (vc > 0) continue;
                    ds.Add(new Goc { ten = "goc_" + (stt++).ToString("00") + "_" + m.name.Substring(0, 6) + "_h" + h + "_" + xa, cho = cho, huongY = huong, vuong = vc });
                    break;
                }
            }
        }
        return ds;
    }

    /// <summary>Chup tu camera dat nhu camera man chinh.</summary>
    public static void Chup(Goc g, string duong, int w, int h)
    {
        var go = new GameObject("TAM_CamGoc");
        go.hideFlags = HideFlags.HideAndDontSave;
        var cam = go.AddComponent<Camera>();
        cam.fieldOfView = 42f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 300f;
        cam.allowHDR = true; cam.clearFlags = CameraClearFlags.Skybox;
        var q = Quaternion.Euler(0, g.huongY, 0);
        go.transform.position = g.cho - (q * Vector3.forward) * 4.2f + Vector3.up * 1.5f;
        go.transform.rotation = Quaternion.Euler(6f, g.huongY, 0f);
        go.AddComponent<SimpleBloom>();

        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(duong, tex.EncodeToPNG());
        cam.targetTexture = null;
        Object.DestroyImmediate(tex); rt.Release(); Object.DestroyImmediate(rt);
        Object.DestroyImmediate(go);
    }
}
