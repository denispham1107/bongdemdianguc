using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 102 - TAN LUA THANH NGON LUA THAT: QUA CAU LUA TRONG TRAN CO CHOI HON KHONG (04/10/2026). Anh chup mot khung sau khi doi tan
/// lua (menu 70) co x2,9 diem chay trang so voi anh cu - mot khung dao dong manh, nen do lai: vao Play Act2, tat GameDirector, ban
/// CUNG LUC ba qua cau lua that (Fireball.Spawn) bay song song tren cao (khong cham gi): A = ban moi (tan lua ngon lua that), B = tan
/// lua CU (lop Sparks dung lai bang VfxFactory.BuildFireballVisual - vet keo dai), C = KHONG tan lua. Moi qua mot lop (29 / 30 / 31) va
/// mot may quay di theo (nen den, cung khoang cach). Cong do sang + dem diem chay (> 0,96) qua 20 khung. Ket qua: PlayTestShots/ngonluathat.txt.
/// </summary>
public static class ThuNgonLuaThat
{
    static bool daBatDau;
    const string Ra = "PlayTestShots/ngonluathat.txt";

    [MenuItem("Diablo 2.5D/102 Ngon lua that tren qua cau lua - do do choi trong tran", false, 170)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (File.Exists(Ra)) File.Delete(Ra);
        daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuNgonLua");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void DatLop(GameObject g, int lop) { foreach (var t in g.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = lop; }

    static IEnumerator KichBan()
    {
        var sb = new StringBuilder();
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        var toi = Object.FindAnyObjectByType<PlayerController>();
        Vector3 goc = toi.transform.position + Vector3.up * 12f;
        string[] ten = { "A moi (ngon lua that)", "B tan lua CU (vet keo dai)", "C khong tan lua" };
        var tong = new double[3]; var chay = new long[3];
        for (int lan = 0; lan < 3; lan++)
        {
            var qs = new Fireball[3];
            for (int i = 0; i < 3; i++)
            {
                qs[i] = Fireball.Spawn(goc + Vector3.right * (i * 6f) + Vector3.forward * lan * 0.01f, Vector3.forward, 0, 0);
                qs[i].lifetime = 2.5f;
            }
            // B: thay Sparks bang ban CU dung tu code; C: bo Sparks
            var cu = new GameObject("TAM_SparksCu"); VfxFactory.BuildFireballVisual(cu.transform, qs[1].bodyRadius);
            var spCu = cu.transform.Find("Sparks");
            var spB = qs[1].transform.Find("Sparks"); if (spB != null) Object.Destroy(spB.gameObject);
            spCu.SetParent(qs[1].transform, false); Object.Destroy(cu);
            var spC = qs[2].transform.Find("Sparks"); if (spC != null) Object.Destroy(spC.gameObject);
            yield return null;
            for (int i = 0; i < 3; i++) DatLop(qs[i].gameObject, 29 + i);
            var cams = new Camera[3];
            for (int i = 0; i < 3; i++)
            {
                var gc = new GameObject("TAM_CamLua" + i); var c = gc.AddComponent<Camera>(); c.enabled = false;
                c.cullingMask = 1 << (29 + i); c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = Color.black;
                c.fieldOfView = 40f; c.nearClipPlane = 0.1f; c.farClipPlane = 50f; c.allowHDR = false;
                cams[i] = c;
            }
            bool suong = RenderSettings.fog; RenderSettings.fog = false;
            var rt = new RenderTexture(256, 256, 24); var tx = new Texture2D(256, 256, TextureFormat.RGB24, false);
            yield return new WaitForSeconds(0.25f);
            for (int k = 0; k < 20; k++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (qs[i] == null) continue;
                    // may quay ngang canh qua cau, cach 6 m, nhin vuong goc huong bay (thay ca duoi lua)
                    cams[i].transform.position = qs[i].transform.position + Vector3.left * 6f + Vector3.back * 1.5f;
                    cams[i].transform.LookAt(qs[i].transform.position + Vector3.back * 1.5f);
                    cams[i].targetTexture = rt; cams[i].Render(); cams[i].targetTexture = null;
                    var tr = RenderTexture.active; RenderTexture.active = rt; tx.ReadPixels(new Rect(0, 0, 256, 256), 0, 0); RenderTexture.active = tr;
                    foreach (var p in tx.GetPixels()) { float l = 0.299f * p.r + 0.587f * p.g + 0.114f * p.b; tong[i] += l; if (l > 0.96f) chay[i]++; }
                }
                yield return new WaitForSeconds(0.05f);
            }
            RenderSettings.fog = suong;
            Object.Destroy(rt); Object.Destroy(tx);
            foreach (var c in cams) Object.Destroy(c.gameObject);
            foreach (var q in qs) if (q != null) Object.Destroy(q.gameObject);
            yield return new WaitForSeconds(0.5f);
        }
        for (int i = 0; i < 3; i++)
            sb.AppendLine(string.Format("{0}: tong do sang {1:F0} (x{2:F2} so voi khong tan lua), diem chay trang {3} (x{4:F2})",
                ten[i], tong[i], tong[i] / tong[2], chay[i], chay[2] > 0 ? (double)chay[i] / chay[2] : -1));
        File.WriteAllText(Ra, sb.ToString());
        var rac = GameObject.Find("TAM_ThuNgonLua"); if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
    }
}
