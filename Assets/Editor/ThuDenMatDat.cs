using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: O VUONG / O CHU NHAT SANG TREN MAT DAT KHI KY NANG PHAT SANG (menu 89).
///
/// Nguoi dung (27/09/2026, 6 anh): quai hoac nguoi choi danh ky nang phat sang thi duoi dat hien ro cac o vuong,
/// o chu nhat chap va, khap ban do. Unity ve Terrain thanh NHIEU MANH, va chia den cho TUNG MANH: manh nao duoc
/// den "diem anh" (pixel light) thi sang muot, manh nao het suat thi den bi ha xuong den dinh / SH - ca manh
/// sang DEU mot mau, lo ra thanh o.
///
/// Do bang SO, khong doan: camera game chi ve lop cua Terrain, chup hai anh cung goc - khong den them (A) va co den
/// them (B) - roi xet D = B - A (phan anh sang cua rieng den ky nang). Anh sang dung thi D giam MUOT tu tam ra
/// ngoai: hai diem anh canh nhau chi chenh vai phan tram dinh. Canh o vuong thi D nhay vot mot buoc.
///   J = buoc nhay lon nhat giua hai diem anh canh nhau / dinh D
///   E = ti le cap diem anh canh nhau chenh qua 5% dinh
/// Do o CA BON muc do hoa (Cao / Trung binh / Yeu / Rat yeu), hai canh: 1 den (mot vu no) va 6 den (danh dong
/// ky nang). Den quanh nhan vat (HeroLight) de nguyen nhu luc choi that - no cung chiem suat den.
///
/// Ket qua: <c>PlayTestShots/den_mat_dat.txt</c>, anh <c>den_matdat_*.png</c>.
/// </summary>
public static class ThuDenMatDat
{
    /// <summary>
    /// "Duong noi" toi da cho phep, do tren Q = D / A (khu van dat): mot o 4x4 lech khoi do doc cua hai o hai ben.
    /// Do 28/09/2026: ban sua 0,043-0,069 (con lai la go dat that); doi chung muc Yeu 0,194-0,836. Nguong giua hai ben.
    /// </summary>
    const float NguongN = 0.12f;

    /// <summary>Ba diem do CO DINH (x, z) - cho xuat phat nhan vat ngau nhien, moi vung dat chia manh mot kieu.</summary>
    static readonly Vector2[] DiemDo = { new Vector2(0f, -10.5f), new Vector2(5.4f, -7.6f), new Vector2(-25f, 20f) };

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/89. Chay thu O VUONG SANG tren mat dat (den ky nang)", false, 178)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_DenMatDat") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_DenMatDat");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DenMatDat] " + s); }
    static void Kiem(bool dung, string loiNeuSai) { if (!dung) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator KichBan()
    {
        PlayerController pc = null;
        float han = Time.time + 25f;
        while (pc == null && Time.time < han) { pc = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        var terrain = Terrain.activeTerrain;
        var cam = Camera.main;
        if (pc == null || terrain == null || cam == null)
        { Ghi("[LOI] thieu nhan vat / terrain / camera"); loi++; Ket(); yield break; }

        // Tat nhung thu lam doi canh giua hai anh: sinh quai, may quay tu bam theo
        var dir = Object.FindAnyObjectByType<GameDirector>();
        if (dir != null) dir.enabled = false;
        var rig = cam.GetComponent<CameraRig>();
        if (rig != null) rig.enabled = false;
        yield return new WaitForSeconds(1f);

        // Den lo lua nhap nhay (LightFlicker) doi do sang giua hai lan chup A / B -> nhieu trong D. Dong bang lai.
        int soNhapNhay = 0;
        foreach (var f in Object.FindObjectsByType<LightFlicker>(FindObjectsSortMode.None)) { f.enabled = false; soNhapNhay++; }
        Ghi("lop terrain = " + LayerMask.LayerToName(terrain.gameObject.layer) + ", dong bang " + soNhapNhay + " den nhap nhay");

        Vector3 p = pc.transform.position;
        for (int iDiem = 0; iDiem < DiemDo.Length; iDiem++)
        {
        p = new Vector3(DiemDo[iDiem].x, 0f, DiemDo[iDiem].y);
        p.y = terrain.SampleHeight(p) + terrain.transform.position.y;
        // Nhan vat (va den quanh nhan vat - HeroLight) dung dung cho do, nhu luc choi that
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = p + Vector3.up * 0.05f;
        if (cc != null) cc.enabled = true;
        cam.transform.position = p + new Vector3(0f, 13f, -11.5f);
        cam.transform.LookAt(p);
        yield return null; yield return null;

        Ghi("");
        Ghi("===== DIEM DO " + (iDiem + 1) + ": " + p.ToString("F1") + " =====");

        for (int m = 0; m < CaiDatDoHoa.SoMuc; m++)
        {
            CaiDatDoHoa.ApDung((MucDoHoa)m);
            yield return null; yield return null;
            Ghi("");
            Ghi(string.Format("--- muc {0} (Unity {1}, pixelLightCount {2}) ---", CaiDatDoHoa.Ten[m],
                QualitySettings.names[QualitySettings.GetQualityLevel()], QualitySettings.pixelLightCount));

            for (int bien = 0; bien < 2; bien++)
            {
                // bien 0 = DOI CHUNG (tat den sinh doi - dung nhu truoc khi sua), bien 1 = ban da sua
                bool sua = bien == 1;
                DenMatDat.Bat = sua;
                yield return null; yield return null;
                Ghi(sua ? " [ban sua: den sinh doi cho mat dat]" : " [doi chung: tat den sinh doi]");

                for (int canh = 0; canh < 2; canh++)
                {
                    int soDen = canh == 0 ? 1 : 6;
                    float[] a = null, b = null; bool[] matDat = null;
                    a = Chup(cam, terrain, out matDat);
                    var den = DatDen(p, soDen, terrain);
                    yield return null; yield return null;
                    b = Chup(cam, terrain, out matDat);
                    int soTach = DenMatDat.SoDenDangTach;

                    // So lan ve thuc cua ca canh (moi lop) luc co den - gia phai tra cho hien man hinh
                    cam.cullingMask = ~0;
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    int setPass = UnityStats.setPassCalls, loat = UnityStats.drawCalls;

                    foreach (var d in den) Object.Destroy(d);
                    yield return null;

                    float dinh, J, E, N, N2;
                    DoBuocNhay(a, b, matDat, out dinh, out J, out E, out N, out N2);
                    Ghi(string.Format("  {0} den: dinh D {1:F3}, DUONG NOI N2 (khu van dat) = {8:F3}, N = {2:F3}, J = {3:F3}, E = {4:P1}; den sinh doi {5}; ca canh: {6} SetPass, {7} draw call",
                        soDen, dinh, N, J, E, soTach, setPass, loat, N2));
                    Kiem(dinh > 0.02f, "den khong chieu toi mat dat - phep do vo nghia (" + CaiDatDoHoa.Ten[m] + ")");
                    if (sua)
                        Kiem(N2 < NguongN, string.Format("muc {0}, {1} den: mat dat van lo O VUONG (N2 = {2:F3} >= {3})",
                            CaiDatDoHoa.Ten[m], soDen, N2, NguongN));
                    else if (m == (int)CaiDatDoHoa.MacDinh && soDen == 6)
                        Kiem(N2 >= NguongN, "doi chung o muc mac dinh KHONG thay o vuong - phep do khong bat duoc loi");
                    if (m == (int)CaiDatDoHoa.MacDinh)   // chi luu anh o muc mac dinh (Yeu) - du de mat nguoi so
                        LuuAnh(a, b, matDat, dinh, "PlayTestShots/den_matdat_d" + (iDiem + 1) + "_" + soDen + "den" + (sua ? "_sua" : "_cu") + ".png");
                }
            }
            DenMatDat.Bat = true;
        }
        }

        // ================= ANH THAT: vu no that cua ky nang, camera game, ca canh, muc mac dinh =================
        CaiDatDoHoa.ApDung(CaiDatDoHoa.MacDinh);
        cam.cullingMask = ~0;
        for (int bien = 0; bien < 2; bien++)
        {
            DenMatDat.Bat = bien == 1;
            yield return new WaitForSeconds(0.3f);
            var q1 = p + new Vector3(-3f, 0f, 1f); var q2 = p + new Vector3(3.5f, 0f, 2.5f); var q3 = p + new Vector3(0.5f, 0f, -3f);
            foreach (var q in new[] { q1, q2, q3 })
            {
                var qq = q; qq.y = terrain.SampleHeight(qq) + terrain.transform.position.y;
                VfxFactory.FireExplosion(qq, 3.4f);
            }
            var qs = p + new Vector3(-5f, 0f, -4f); qs.y = terrain.SampleHeight(qs) + terrain.transform.position.y;
            VfxFactory.LightningImpact(qs, 2.1f);
            float t0 = Time.time;
            while (Time.time - t0 < 0.18f) yield return null;
            ChupCaCanh(cam, "PlayTestShots/den_matdat_that_" + (bien == 1 ? "sua" : "cu") + ".png");
            Ghi("anh that (" + CaiDatDoHoa.Ten[(int)CaiDatDoHoa.MacDinh] + ", " + (bien == 1 ? "ban sua" : "doi chung") + "): den sinh doi "
                + DenMatDat.SoDenDangTach + " -> den_matdat_that_" + (bien == 1 ? "sua" : "cu") + ".png");
            float t1 = Time.time;
            while (Time.time - t1 < 3f) yield return null;   // cho vu no tan het truoc lan sau
        }
        DenMatDat.Bat = true;

        CaiDatDoHoa.ApDung();   // tra lai muc nguoi choi dang chon
        Ghi("");
        Ghi("tra lai muc dang chon: " + CaiDatDoHoa.Ten[(int)CaiDatDoHoa.Muc] + ", pixelLightCount " + QualitySettings.pixelLightCount);
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    const int Rong = 640, Cao = 360;
    static readonly Color MauNen = new Color(1f, 0f, 1f, 1f);

    /// <summary>Chup chi lop Terrain; tra ve do sang tung diem anh (HDR, khong cat) va mat na "la mat dat".</summary>
    static float[] Chup(Camera cam, Terrain terrain, out bool[] matDat)
    {
        var rt = RenderTexture.GetTemporary(Rong, Cao, 24, RenderTextureFormat.ARGBHalf);
        var cuMask = cam.cullingMask; var cuClear = cam.clearFlags; var cuNen = cam.backgroundColor; var cuRt = cam.targetTexture;
        cam.cullingMask = 1 << terrain.gameObject.layer;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = MauNen;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = cuRt; cam.clearFlags = cuClear; cam.backgroundColor = cuNen; cam.cullingMask = cuMask;

        var tx = new Texture2D(Rong, Cao, TextureFormat.RGBAFloat, false);
        var cu = RenderTexture.active;
        RenderTexture.active = rt;
        tx.ReadPixels(new Rect(0, 0, Rong, Cao), 0, 0);
        tx.Apply();
        RenderTexture.active = cu;
        RenderTexture.ReleaseTemporary(rt);

        var px = tx.GetPixels();
        Object.DestroyImmediate(tx);
        var sang = new float[px.Length];
        matDat = new bool[px.Length];
        for (int i = 0; i < px.Length; i++)
        {
            var c = px[i];
            bool nen = Mathf.Abs(c.r - 1f) < 0.02f && c.g < 0.02f && Mathf.Abs(c.b - 1f) < 0.02f;
            matDat[i] = !nen;
            sang[i] = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
        }
        return sang;
    }

    /// <summary>Den giong den vu no Qua cau lua (VfxFactory.FireExplosion: cam, 6, tam 12 m), treo 1,5 m tren dat.</summary>
    static List<GameObject> DatDen(Vector3 p, int soDen, Terrain terrain)
    {
        var ds = new List<GameObject>();
        for (int i = 0; i < soDen; i++)
        {
            Vector3 q = p;
            if (i > 0)
            {
                float g = (i - 1) * Mathf.PI * 2f / (soDen - 1);
                q += new Vector3(Mathf.Cos(g), 0f, Mathf.Sin(g)) * 6f;
            }
            q.y = terrain.SampleHeight(q) + terrain.transform.position.y + 1.5f;
            var go = new GameObject("TAM_DenKyNang_" + i);
            go.transform.position = q;
            var lt = go.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.55f, 0.20f);
            lt.intensity = 6f;
            lt.range = 12f;
            lt.shadows = LightShadows.None;
            ds.Add(go);
        }
        return ds;
    }

    const int O = 4;   // gop 4x4 diem anh: xoa hoa van dat (albedo) ma canh o vuong (dai ca chuc met) van nguyen

    /// <summary>
    /// D = B - A (phan sang cua rieng den them), gop thanh o 4x4 roi do buoc nhay giua hai o canh nhau.
    /// Chi xet vung D &gt; 5% dinh: ngoai do anh sang da tat, buoc nhay nao cung nho.
    /// </summary>
    static void DoBuocNhay(float[] a, float[] b, bool[] matDat, out float dinh, out float J, out float E, out float N, out float N2)
    {
        int nx = Rong / O, ny = Cao / O;
        var g = new float[nx * ny];
        var gA = new float[nx * ny];
        var co = new bool[nx * ny];
        dinh = 0f;
        for (int by = 0; by < ny; by++)
            for (int bx = 0; bx < nx; bx++)
            {
                float tong = 0f, tongA = 0f; int dem = 0; bool du = true;
                for (int y = 0; y < O; y++)
                    for (int x = 0; x < O; x++)
                    {
                        int i = (by * O + y) * Rong + bx * O + x;
                        if (!matDat[i]) { du = false; continue; }
                        tong += Mathf.Max(0f, b[i] - a[i]); tongA += a[i]; dem++;
                    }
                int k = by * nx + bx;
                co[k] = du && dem > 0;
                g[k] = dem > 0 ? tong / dem : 0f;
                gA[k] = dem > 0 ? tongA / dem : 0f;
                if (co[k] && g[k] > dinh) dinh = g[k];
            }
        J = 0f; E = 0f; N = 0f; N2 = 0f;
        if (dinh <= 1e-5f) return;
        // N2: nhu N nhung tren Q = D / A (chia cho anh KHONG den them) - khu mau / van dat, chi con hinh dang anh sang.
        var q = new float[nx * ny];
        float qMax = 0f;
        for (int k = 0; k < q.Length; k++)
        {
            q[k] = co[k] && gA[k] > 1e-4f ? g[k] / gA[k] : 0f;
            if (co[k] && g[k] >= 0.05f * dinh && q[k] > qMax) qMax = q[k];
        }
        // DUONG NOI: buoc giua o k va k+1 so voi trung binh hai buoc hai ben (k-1..k, k+1..k+2). Doc deu -> 0;
        // canh o vuong (nhay mot buoc giua duong doc) -> lon.
        for (int by = 0; by < ny; by++)
            for (int bx = 0; bx < nx; bx++)
                for (int h = 0; h < 2; h++)
                {
                    int dx = h == 0 ? 1 : 0, dy = h == 0 ? 0 : 1;
                    int x0 = bx - dx, y0 = by - dy, x3 = bx + 2 * dx, y3 = by + 2 * dy;
                    if (x0 < 0 || y0 < 0 || x3 >= nx || y3 >= ny) continue;
                    int k0 = y0 * nx + x0, k1 = by * nx + bx, k2 = (by + dy) * nx + bx + dx, k3 = y3 * nx + x3;
                    if (!co[k0] || !co[k1] || !co[k2] || !co[k3]) continue;
                    if (Mathf.Max(g[k1], g[k2]) < 0.05f * dinh) continue;
                    float giua = g[k2] - g[k1], trai = g[k1] - g[k0], phai = g[k3] - g[k2];
                    float lech = Mathf.Abs(giua - 0.5f * (trai + phai)) / dinh;
                    if (lech > N) N = lech;
                    if (qMax > 1e-4f)
                    {
                        float lq = Mathf.Abs((q[k2] - q[k1]) - 0.5f * ((q[k1] - q[k0]) + (q[k3] - q[k2]))) / qMax;
                        if (lq > N2) N2 = lq;
                    }
                }
        int soCap = 0, qua = 0;
        for (int by = 0; by < ny; by++)
            for (int bx = 0; bx < nx; bx++)
            {
                int k = by * nx + bx;
                if (!co[k]) continue;
                for (int h = 0; h < 2; h++)
                {
                    int k2 = h == 0 ? (bx + 1 < nx ? k + 1 : -1) : (by + 1 < ny ? k + nx : -1);
                    if (k2 < 0 || !co[k2]) continue;
                    if (Mathf.Max(g[k], g[k2]) < 0.05f * dinh) continue;
                    float buoc = Mathf.Abs(g[k2] - g[k]) / dinh;
                    soCap++;
                    if (buoc > J) J = buoc;
                    if (buoc > 0.05f) qua++;
                }
            }
        E = soCap > 0 ? (float)qua / soCap : 0f;
    }

    /// <summary>Luu anh D (phan sang cua rieng den ky nang) de mat nguoi cung thay o vuong.</summary>
    static void LuuAnh(float[] a, float[] b, bool[] matDat, float dinh, string duong)
    {
        var tx = new Texture2D(Rong, Cao, TextureFormat.RGB24, false);
        var px = new Color[b.Length];
        for (int i = 0; i < b.Length; i++)
        {
            float v = matDat[i] ? Mathf.Clamp01(Mathf.Max(0f, b[i] - a[i]) / Mathf.Max(0.02f, dinh)) : 0f;
            px[i] = new Color(v, v * 0.8f, v * 0.55f);
        }
        tx.SetPixels(px); tx.Apply();
        File.WriteAllBytes(duong, tx.EncodeToPNG());
        Object.DestroyImmediate(tx);
    }

    static void ChupCaCanh(Camera cam, string duong)
    {
        var rt = RenderTexture.GetTemporary(960, 540, 24, RenderTextureFormat.ARGB32);
        var cuRt = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = cuRt;
        var cu = RenderTexture.active;
        RenderTexture.active = rt;
        var tx = new Texture2D(960, 540, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
        tx.Apply();
        RenderTexture.active = cu;
        RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(duong, tx.EncodeToPNG());
        Object.DestroyImmediate(tx);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/den_mat_dat.txt", bao.ToString());
        var rac = GameObject.Find("TAM_DenMatDat");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
        if (!string.IsNullOrEmpty(canhCu)) EditorApplication.update += TraLaiCanh;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
