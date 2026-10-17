using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// DO VO CAY SAN SUI (17/10/2026, nguoi dung: "vo cay con tron bong gia tao, cho san sui, mot vai cho nut troc").
///
/// Chup cac cay CO DINH (moi kieu luoi mot cay gan tam ban do, chi cay KHONG co la de bong than khong lan la) o hai cu ly:
///   can  - 1,9 m (soi mep mang troc); gan  - may quay 4 m, cao 1,6 m, dung phia anh trang chieu toi (mat dang chup duoc roi sang);
///   game - goc nhin cua may quay tran (nghieng 40 do, 14 m).
/// So do doc tu ANH (khong doc thong so vat lieu - phep kiem doc lap):
///   san  = do lech chuan cua (sang - sang lam mo 4 diem anh) trong BONG THAN (anh trang dac ve rieng cay, lop 31) -> chi tiet nho;
///   lech = do lech chuan do sang trong bong than; sang = do sang trung binh.
/// Goi <c>ThuVoCay.Chup("truoc")</c> truoc khi doi vat lieu, <c>Chup("sau")</c> sau -> PlayTestShots/vocay_{nhan}_*.png + vocay_{nhan}.txt.
/// trongPlay: phep thu ban ngay (nguoi goi tat TamNhin + dat ChuyenChieuSangDem.ApGiay o lenh TRUOC). Ngoai Play, khong tao vat the trong scene (may quay DontSave, xoa ngay; lop cay tra lai).
/// </summary>
public static class ThuVoCay
{
    const string Mat = "Assets/BlenderMaps/GraveyardAct2/CayDaChua/Act2_VoCay_SanSui.mat";
    const int R = 900, C = 700;

    [MenuItem("Diablo 2.5D/143 Vo cay - chup do (truoc)")]
    static void MenuTruoc() { Chup("truoc"); }

    [MenuItem("Diablo 2.5D/143b Vo cay - chup do (sau)")]
    static void MenuSau() { Chup("sau"); }

    public static string Chup(string nhan, bool trongPlay = false)
    {
        if (EditorApplication.isPlaying && !trongPlay) { Debug.LogError("[ThuVoCay] dang Play - thoat Play roi chay lai"); return "dang Play"; }
        var mat = AssetDatabase.LoadAssetAtPath<Material>(Mat);
        Shader.SetGlobalFloat("_TN_Bat", 0f);
        // moi kieu luoi: cay gan tam nhat, chi mot vat lieu (khong la)
        var chon = new Dictionary<string, MeshRenderer>();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.InstanceID))
        {
            if (r.sharedMaterials.Length != 1 || r.sharedMaterial != mat || !r.enabled) continue;
            var mf = r.GetComponent<MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue;
            string k = mf.sharedMesh.name;
            Vector3 c = r.bounds.center; c.y = 0;
            if (!chon.ContainsKey(k)) { chon[k] = r; continue; }
            Vector3 c0 = chon[k].bounds.center; c0.y = 0;
            if (c.magnitude < c0.magnitude) chon[k] = r;
        }
        var ds = new List<string>(chon.Keys); ds.Sort();
        Light mt = RenderSettings.sun;
        if (mt == null) foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional && l.enabled) { mt = l; break; }
        Vector3 huongDen = mt != null ? -mt.transform.forward : Vector3.up;
        Vector3 phiaSang = new Vector3(huongDen.x, 0, huongDen.z); if (phiaSang.sqrMagnitude < 1e-4f) phiaSang = Vector3.back; phiaSang.Normalize();

        var trang = Shader.Find("Hidden/Diablo25D/TrangDacPhepThu");
        var go = new GameObject("TAM_MayQuayVoCay"); go.hideFlags = HideFlags.HideAndDontSave;
        var cam = go.AddComponent<Camera>();
        cam.enabled = false; cam.fieldOfView = 40; cam.nearClipPlane = 0.3f; cam.farClipPlane = 300;
        var rt = new RenderTexture(R, C, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(R, C, TextureFormat.RGBA32, false);
        var bao = new StringBuilder();
        bao.AppendLine("Vo cay - " + nhan + " (" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "), den: " + (mt != null ? mt.name + " " + mt.intensity.ToString("0.00") : "?"));
        Directory.CreateDirectory("PlayTestShots");
        double tongSan = 0, tongLech = 0; int dem = 0;
        int lopCu;
        for (int i = 0; i < ds.Count; i++)
        {
            var r = chon[ds[i]];
            Vector3 goc = r.bounds.center; goc.y = r.bounds.min.y;
            string[] cuLy = { "can", "gan", "game" };
            foreach (var cl in cuLy)
            {
                Vector3 nhin = goc + Vector3.up * (cl != "game" ? 1.5f : 2.5f);
                Vector3 vt = cl != "game" ? goc + phiaSang * (cl == "can" ? 1.9f : 4f) + Vector3.up * 1.6f
                                         : nhin + (phiaSang * Mathf.Cos(40 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(40 * Mathf.Deg2Rad)) * 14f;
                go.transform.position = vt; go.transform.LookAt(nhin);
                cam.targetTexture = rt;
                // anh that
                cam.cullingMask = ~0; cam.clearFlags = CameraClearFlags.Skybox;
                cam.Render();
                RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, R, C), 0, 0); tex.Apply();
                Color32[] anh = tex.GetPixels32();
                File.WriteAllBytes("PlayTestShots/vocay_" + nhan + "_" + i + "_" + cl + ".png", tex.EncodeToPNG());
                // bong than: chi cay nay, trang dac
                lopCu = r.gameObject.layer; r.gameObject.layer = 31;
                cam.cullingMask = 1 << 31; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
                cam.RenderWithShader(trang, "");
                r.gameObject.layer = lopCu;
                RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, R, C), 0, 0); tex.Apply();
                Color32[] bong = tex.GetPixels32();
                Do(anh, bong, out double san, out double lech, out double sang, out int soDiem);
                bao.AppendLine(string.Format("  {0} {1,-24} {2,-4}: diem than {3,6}  san {4:0.0000}  lech {5:0.0000}  sang {6:0.000}", i, ds[i], cl, soDiem, san, lech, sang));
                if (soDiem > 500) { tongSan += san; tongLech += lech; dem++; }
            }
        }
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(go); Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
        bao.AppendLine(string.Format("TRUNG BINH ({0} anh): san {1:0.0000}  lech {2:0.0000}", dem, tongSan / Mathf.Max(1, dem), tongLech / Mathf.Max(1, dem)));
        File.WriteAllText("PlayTestShots/vocay_" + nhan + ".txt", bao.ToString());
        Debug.Log("[ThuVoCay]\n" + bao);
        return bao.ToString();
    }

    // Do trong bong than da co lai 3 diem anh (bo vien than lan nen)
    static void Do(Color32[] anh, Color32[] bong, out double san, out double lech, out double sang, out int soDiem)
    {
        float[] L = new float[R * C]; bool[] m = new bool[R * C];
        for (int k = 0; k < L.Length; k++) { var c = anh[k]; L[k] = (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f; m[k] = bong[k].r > 128; }
        bool[] m2 = new bool[L.Length];
        for (int y = 3; y < C - 3; y++) for (int x = 3; x < R - 3; x++)
        {
            bool ok = true;
            for (int dy = -3; dy <= 3 && ok; dy += 3) for (int dx = -3; dx <= 3 && ok; dx += 3) if (!m[(y + dy) * R + x + dx]) ok = false;
            m2[y * R + x] = ok;
        }
        // mo 4 diem anh (hop 9x9)
        float[] tam = new float[L.Length], mo = new float[L.Length];
        for (int y = 0; y < C; y++) { float s = 0; for (int x = -4; x < R + 4; x++) { if (x + 4 < R) s += L[y * R + x + 4]; if (x - 5 >= 0) s -= L[y * R + x - 5]; if (x >= 0 && x < R) tam[y * R + x] = s / 9f; } }
        for (int x = 0; x < R; x++) { float s = 0; for (int y = -4; y < C + 4; y++) { if (y + 4 < C) s += tam[(y + 4) * R + x]; if (y - 5 >= 0) s -= tam[(y - 5) * R + x]; if (y >= 0 && y < C) mo[y * R + x] = s / 9f; } }
        double s1 = 0, s2 = 0, h2 = 0; int n = 0;
        for (int k = 0; k < L.Length; k++) if (m2[k]) { s1 += L[k]; s2 += L[k] * L[k]; double d = L[k] - mo[k]; h2 += d * d; n++; }
        soDiem = n;
        if (n == 0) { san = lech = sang = 0; return; }
        sang = s1 / n; lech = System.Math.Sqrt(System.Math.Max(0, s2 / n - sang * sang)); san = System.Math.Sqrt(h2 / n);
    }
}
