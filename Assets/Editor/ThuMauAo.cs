using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU MAU AO NGUOI CHOI (<see cref="MauAoNhanVat"/>, shader Diablo25D/PhuThuyDoiMau) - 09/10/2026.
///
/// Vao Play o Act2, sinh 6 ban sao phu thuy dung hang ngang truoc may quay, gan mau qua DUNG duong cua tran mang
/// (<see cref="KhoiDongTranMang.GanDoi"/> 4 tham so):
///   A. Che do DON: ghe 0..5 -> 6 mau khac nhau, dung bang MauDon.
///   B. Che do DOI: ghe 0-2 doi A, 3-5 doi B -> cung doi cung mau, hai doi khac mau.
///   C. DO TREN ANH (doc lap voi code dat mau): chup RIENG tung nhan vat (lop 31, nen den, ban ngay), so diem anh:
///      - vung "tim ao choang" (sac do 277-324, bao hoa > 0,25) phai gan het (doi chung a = 0 cung cho: con tim);
///      - diem anh mang sac do mau dich (+-25 do) phai nhieu len ro so voi doi chung;
///      - do sang (V) va bao hoa (S) trung vi cua phan ao doi mau: khong qua toi, khong qua nhat.
///   D. Vat lieu goc: _MauAo.a = 0 (man chinh, choi mot minh giu ao tim nhu cu).
/// Anh: PlayTestShots/mauao_don_dem.png, mauao_don_ngay.png, mauao_doi_ngay.png; so do mauao.txt.
/// </summary>
public static class ThuMauAo
{
    const string Canh = "Assets/Scenes/Act2.unity";
    const int LopChup = 31;

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/116. Chay thu MAU AO nguoi choi (Don - Doi)", false, 209)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (canhCu != Canh) EditorSceneManager.OpenScene(Canh);
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_MauAo");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[MauAo] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    struct SoDo { public float tiTim, tiDich, vTrungVi, sTrungVi; public int soDiem; public Color[] px; }

    /// <summary>Chup rieng nhan vat (lop 31, nen den) tu phia truoc, dem diem anh theo sac do.</summary>
    static SoDo DoNhanVat(GameObject nv, float hueDich)
    {
        var lopCu = new Dictionary<Transform, int>();
        foreach (var t in nv.GetComponentsInChildren<Transform>(true)) { lopCu[t] = t.gameObject.layer; t.gameObject.layer = LopChup; }

        var camGo = new GameObject("TAM_MayChupAo");
        var cam = camGo.AddComponent<Camera>();
        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
        cam.cullingMask = 1 << LopChup;
        cam.fieldOfView = 30f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 20f;
        Vector3 truoc = nv.transform.forward; truoc.y = 0f; truoc.Normalize();
        camGo.transform.position = nv.transform.position + truoc * 4.2f + Vector3.up * 1.0f;
        camGo.transform.LookAt(nv.transform.position + Vector3.up * 0.85f);
        var rt = new RenderTexture(240, 360, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var anh = new Texture2D(240, 360, TextureFormat.RGB24, false);
        anh.ReadPixels(new Rect(0, 0, 240, 360), 0, 0); anh.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt); Object.DestroyImmediate(camGo);
        foreach (var kv in lopCu) if (kv.Key != null) kv.Key.gameObject.layer = kv.Value;

        var px = anh.GetPixels();
        Object.DestroyImmediate(anh);
        int tong = 0, tim = 0, dich = 0;
        var vs = new List<float>(); var ss = new List<float>();
        foreach (var c in px)
        {
            float h, s, v; Color.RGBToHSV(c, out h, out s, out v);
            if (v < 0.03f) continue;                         // nen den
            tong++;
            if (s < 0.25f) continue;
            float hd = h * 360f;
            if (hd >= 277f && hd <= 324f) tim++;
            float lech = Mathf.Abs(Mathf.DeltaAngle(hd, hueDich));
            if (lech <= 25f) { dich++; vs.Add(v); ss.Add(s); }
        }
        vs.Sort(); ss.Sort();
        return new SoDo
        {
            soDiem = tong,
            px = px,
            tiTim = tong > 0 ? (float)tim / tong : 0f,
            tiDich = tong > 0 ? (float)dich / tong : 0f,
            vTrungVi = vs.Count > 0 ? vs[vs.Count / 2] : 0f,
            sTrungVi = ss.Count > 0 ? ss[ss.Count / 2] : 0f,
        };
    }

    static float HueCua(Color c) { float h, s, v; Color.RGBToHSV(c, out h, out s, out v); return h * 360f; }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] mau ao nguoi choi (Don / Doi)");

        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { Loi("khong tim thay nhan vat cua minh"); Ket(); yield break; }
        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        yield return new WaitForSeconds(1.5f);
        var cam = Camera.main;
        if (cam == null) { Loi("khong co Camera.main"); Ket(); yield break; }

        // ---- D. Vat lieu goc ----
        var vlGoc = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Player_PhuThuy.mat");
        bool dungShader = vlGoc != null && vlGoc.shader != null && vlGoc.shader.name == "Diablo25D/PhuThuyDoiMau";
        float aGoc = vlGoc != null && vlGoc.HasProperty("_MauAo") ? vlGoc.GetColor("_MauAo").a : -1f;
        Ghi("D. Player_PhuThuy.mat: shader " + (vlGoc != null ? vlGoc.shader.name : "?") + ", _MauAo.a = " + aGoc.ToString("F2") + " (mong 0 - giu ao tim)");
        if (!dungShader) Loi("vat lieu phu thuy khong dung shader doi mau");
        if (aGoc != 0f) Loi("vat lieu goc dang doi mau san");
        Ghi("   mau ao nhan vat cua minh (chua vao tran mang): a = " + MauAoNhanVat.DangDat(toi.gameObject).a.ToString("F2"));

        // Hang 6 ban sao truoc may quay game, quay mat ve may quay
        Vector3 phai = cam.transform.right; phai.y = 0f; phai.Normalize();
        Vector3 truoc = cam.transform.forward; truoc.y = 0f; truoc.Normalize();
        Vector3 goc = toi.transform.position + truoc * 3.5f;
        var bs = new List<PlayerController>();
        for (int i = 0; i < 6; i++)
        {
            Vector3 c = goc + phai * ((i - 2.5f) * 1.6f); c.y = VfxFactory.GroundY(c) + 0.05f;
            var nv = NguoiChoiKhac.Sinh("uid-mauao-" + i, "Ghế " + (i + 1), c);
            if (nv == null) { Loi("khong sinh duoc ban sao " + i); continue; }
            nv.transform.rotation = Quaternion.LookRotation(-truoc);
            bs.Add(nv);
        }
        if (bs.Count != 6) { Ket(); yield break; }
        yield return new WaitForSeconds(0.8f);

        var ngayDem = Object.FindAnyObjectByType<ChuyenChieuSangDem>();

        // ---- A. Che do DON ----
        var mauDon = new Color[6];
        for (int i = 0; i < 6; i++)
        {
            KhoiDongTranMang.GanDoi(bs[i], bs[i].GetComponent<BangTen>(), -1, (byte)i);
            mauDon[i] = MauAoNhanVat.DangDat(bs[i].gameObject);
        }
        int khacNhau = 0, dungBang = 0;
        float hueGanNhat = 999f;
        for (int i = 0; i < 6; i++)
        {
            Color m = mauDon[i]; m.a = 1f;
            if (m == MauAoNhanVat.MauDon[i] && mauDon[i].a > 0.99f) dungBang++;
            for (int j = i + 1; j < 6; j++)
            {
                float lech = Mathf.Abs(Mathf.DeltaAngle(HueCua(mauDon[i]), HueCua(mauDon[j])));
                hueGanNhat = Mathf.Min(hueGanNhat, lech);
                if (lech >= 25f) khacNhau++;
            }
        }
        Ghi("A. Don: " + dungBang + "/6 ghe dung mau bang; " + khacNhau + "/15 cap khac sac do >= 25 do (gan nhat " + hueGanNhat.ToString("F0") + " do)");
        if (dungBang != 6) Loi("Don: mau ao khong dung theo ghe");
        if (khacNhau != 15) Loi("Don: co hai ghe mau gan nhau");

        // Anh dem (vao tran la dem) va ngay
        yield return new WaitForEndOfFrame();
        yield return Chup("mauao_don_dem");
        // Anh can canh ban DEM (vao tran la dem): 6 mau
        {
            var dem = new Texture2D(240 * 6, 360, TextureFormat.RGB24, false);
            for (int i = 0; i < 6; i++) dem.SetPixels(240 * i, 0, 240, 360, DoNhanVat(bs[i].gameObject, HueCua(MauAoNhanVat.MauDon[i])).px);
            dem.Apply();
            File.WriteAllBytes("PlayTestShots/mauao_can_canh_dem.png", dem.EncodeToPNG());
            Object.DestroyImmediate(dem);
        }
        if (ngayDem != null) { ngayDem.enabled = false; ngayDem.ApGiay(ChuyenChieuSangDem.GiayGiuaNgay); }
        yield return new WaitForSeconds(0.3f);
        yield return new WaitForEndOfFrame();
        yield return Chup("mauao_don_ngay");

        // ---- C. Do tren anh (ban ngay), doi chung a = 0 ----
        var sb = new StringBuilder();
        var anhCan = new List<Color[]>();
        Color[] anhGoc = null;
        for (int i = 0; i < 6; i++)
        {
            float hd = HueCua(MauAoNhanVat.MauDon[i]);
            var sau = DoNhanVat(bs[i].gameObject, hd);
            var dc = new MaterialPropertyBlock();
            foreach (var r in bs[i].GetComponentsInChildren<Renderer>(true))
            { r.GetPropertyBlock(dc); dc.SetColor("_MauAo", new Color(1, 1, 1, 0)); r.SetPropertyBlock(dc); }
            var truocDoi = DoNhanVat(bs[i].gameObject, hd);
            KhoiDongTranMang.GanDoi(bs[i], bs[i].GetComponent<BangTen>(), -1, (byte)i);

            // Ti le diem anh cua nhan vat DOI MAU giua anh goc (a = 0) va anh doi mau - cung tu the, cung khung. Dem "tim" tren
            // anh khong dung duoc: duoi anh sang game ao goc gan nhu khong roi vao dai sac do tim (do 09/10/2026: 1%)
            int doi = 0, tongNv = 0;
            for (int k = 0; k < sau.px.Length; k++)
            {
                Color x = sau.px[k], y = truocDoi.px[k];
                if (Mathf.Max(x.r, x.g, x.b) < 0.03f && Mathf.Max(y.r, y.g, y.b) < 0.03f) continue;
                tongNv++;
                if (Mathf.Abs(x.r - y.r) + Mathf.Abs(x.g - y.g) + Mathf.Abs(x.b - y.b) > 0.12f) doi++;
            }
            float tiDoi = tongNv > 0 ? (float)doi / tongNv : 0f;
            anhCan.Add(sau.px); if (i == 0) anhGoc = truocDoi.px;
            sb.AppendFormat("   ghe {0} (sac do {1:F0}): diem anh doi mau {2:P0}; diem mau dich {3:P0} -> {4:P0}; ao moi V {5:F2} S {6:F2} ({7} diem)\n",
                i, hd, tiDoi, truocDoi.tiDich, sau.tiDich, sau.vTrungVi, sau.sTrungVi, sau.soDiem);
            if (tiDoi < 0.12f) Loi("ghe " + i + ": qua it diem anh doi mau (" + tiDoi.ToString("P0") + ")");
            if (tiDoi > 0.60f) Loi("ghe " + i + ": doi mau tran ra ngoai ao (" + tiDoi.ToString("P0") + ")");
            bool laTim = hd >= 260f && hd <= 330f;       // ghe mau tim: ao goc da tim san, khong so "tang"
            if (!laTim && sau.tiDich < truocDoi.tiDich + 0.10f) Loi("ghe " + i + ": phan ao mang mau moi khong tang ro");
            if (sau.vTrungVi < 0.22f) Loi("ghe " + i + ": ao qua toi (V " + sau.vTrungVi.ToString("F2") + ")");
            if (sau.vTrungVi > 0.85f) Loi("ghe " + i + ": ao qua sang (V " + sau.vTrungVi.ToString("F2") + ")");
            if (sau.sTrungVi < 0.40f) Loi("ghe " + i + ": ao qua nhat (S " + sau.sTrungVi.ToString("F2") + ")");
        }
        Ghi("C. do tren anh chup rieng tung nhan vat, ban ngay (doi chung a = 0 -> doi mau):\n" + sb.ToString().TrimEnd());
        // Anh can canh: ao goc + 6 mau (240 x 360 moi o)
        if (anhGoc != null && anhCan.Count == 6)
        {
            var dai = new Texture2D(240 * 7, 360, TextureFormat.RGB24, false);
            dai.SetPixels(0, 0, 240, 360, anhGoc);
            for (int i = 0; i < 6; i++) dai.SetPixels(240 * (i + 1), 0, 240, 360, anhCan[i]);
            dai.Apply();
            File.WriteAllBytes("PlayTestShots/mauao_can_canh.png", dai.EncodeToPNG());
            Object.DestroyImmediate(dai);
        }

        // ---- B. Che do DOI ----
        var mauDoi = new Color[6];
        for (int i = 0; i < 6; i++)
        {
            sbyte d = i < 3 ? CheDoTran.DoiA : CheDoTran.DoiB;
            KhoiDongTranMang.GanDoi(bs[i], bs[i].GetComponent<BangTen>(), d, (byte)i);
            mauDoi[i] = MauAoNhanVat.DangDat(bs[i].gameObject);
        }
        bool cungA = mauDoi[0] == mauDoi[1] && mauDoi[1] == mauDoi[2];
        bool cungB = mauDoi[3] == mauDoi[4] && mauDoi[4] == mauDoi[5];
        float lechDoi = Mathf.Abs(Mathf.DeltaAngle(HueCua(mauDoi[0]), HueCua(mauDoi[3])));
        Ghi("B. Doi: doi A cung mau " + cungA + ", doi B cung mau " + cungB + ", hai doi lech sac do " + lechDoi.ToString("F0") + " do");
        if (!cungA || !cungB) Loi("Doi: cung doi ma khac mau");
        if (lechDoi < 60f) Loi("Doi: hai doi mau gan nhau");
        yield return new WaitForEndOfFrame();
        yield return Chup("mauao_doi_ngay");
        var sbDoi = new StringBuilder();
        for (int i = 0; i < 6; i += 3)
        {
            var sd = DoNhanVat(bs[i].gameObject, HueCua(mauDoi[i]));
            sbDoi.AppendFormat(" doi {0}: diem mau doi {1:P0}, V {2:F2} S {3:F2};", i < 3 ? "A" : "B", sd.tiDich, sd.vTrungVi, sd.sTrungVi);
        }
        Ghi("   do tren anh:" + sbDoi);

        if (ngayDem != null) { ngayDem.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); }
        foreach (var x in bs) NguoiChoiKhac.Bo(x);
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/mauao.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_MauAo", "TAM_MayChupAo" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh) EditorSceneManager.OpenScene(canhCu);
    }
}
