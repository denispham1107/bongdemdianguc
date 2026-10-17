using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 142 - KIEM MAT DAT SAN SUI (menu 141). Doi chung = MAT DAT CU dung lai tu Assets/Terrain/{ten}_DoCaoTruocSanSui.bytes (TerrainData tam).
/// Ngoai Play (Act2 + Man chinh):
///   A. terrain 1025, 4 lop co normal map, DatVunVe co du manh.
///   B. Z-FIGHTING DO VAT: tung renderer (bo cay, nhan vat, dat vun), luoi 9x9 tren khung bao, khe |day/noc - dat| < 7 mm o >= 3 diem
///      (cung tieu chi menu 14) - dem voi dat CU va dat MOI; vat MOI bi (dat moi ma dat cu khong) phai = 0. Do cao dat duoi chan vat doi bao nhieu.
///   C. MAT NUOC: diem trong long vung (hinh tron noi tiep) dat cach mat nuoc < 2 cm hoac nho len: cu / moi.
///   D. DAT VUN: tam giac nao (> 0,004 m2) lech < 12 do so voi mat dat ma cach dat < 2,5 cm.
///   E. TAM SUONG (TamNhin, o 1 m, nhac 0,12 m): diem luoi do cao nao cao hon tam suong (dam xuyen) - cach dung MOI (cao nhat trong o) va
///      doi chung CACH CU (do cao giua o).
///   F. DIA BAM DAT (GroundRing.BuildDisc - vung mau, vet nut set, vong ky nang): 60 cho ngau nhien, ban kinh 1,5 m, nhac 0,05: dat dam xuyen dia /
///      dia lo lung > 10 cm tren dat - luoi moi va doi chung luoi thua cu (20 nan x 2 vong).
/// Trong Play (Act2):
///   G. di that 6 s qua cho co o / nut: cham dat, toc do.
///   H. so manh dat vun ve / lan goi moi khung; anh chup may quay game DEM + NGAY (PlayTestShots/matdat_play_*.png).
/// Ket qua: PlayTestShots/thumatdatsansui.txt.
/// </summary>
public static class ThuMatDatSanSui
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_ThuMatDat";

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[ThuMatDat] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    [MenuItem("Diablo 2.5D/142. Chay thu MAT DAT san sui (z-fighting · nuoc · suong · dia bam dat · di lai · anh)", false, 233)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[ThuMatDat] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuMatDat] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        canhCu = EditorSceneManager.GetActiveScene().path;
        foreach (var kv in new[] { new[] { "Assets/Scenes/MainMenu.unity", "ManChinh" }, new[] { "Assets/Scenes/Act2.unity", "Act2" } })
        {
            EditorSceneManager.OpenScene(kv[0], OpenSceneMode.Single);
            NgoaiPlay(kv[1]);
        }
        // Act2 dang mo -> vao Play
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    // ================================================================
    //  NGOAI PLAY
    // ================================================================
    static TerrainData DatCu(string ten, Terrain ter, out Vector3 gocCu)
    {
        gocCu = ter.transform.position;
        string p = "Assets/Terrain/" + ten + "_DoCaoTruocSanSui.bytes";
        if (!File.Exists(p)) return null;
        using (var rd = new BinaryReader(File.OpenRead(p)))
        {
            int r = rd.ReadInt32(); float y0 = rd.ReadSingle(), sy = rd.ReadSingle();
            var h = new float[r, r];
            for (int z = 0; z < r; z++) for (int x = 0; x < r; x++) h[z, x] = rd.ReadSingle();
            var td = new TerrainData { heightmapResolution = r };
            var k = ter.terrainData.size; td.size = new Vector3(k.x, sy, k.z);
            td.SetHeights(0, 0, h);
            gocCu.y = y0;
            return td;
        }
    }

    static float Cao(TerrainData td, Vector3 goc, float x, float z)
    {
        var k = td.size;
        return td.GetInterpolatedHeight((x - goc.x) / k.x, (z - goc.z) / k.z) + goc.y;
    }

    static bool BoQua(Renderer r)
    {
        if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) return true;
        var g = r.gameObject;
        if (g.name.StartsWith("TREE_")) return true;
        if (g.GetComponent<Terrain>() != null) return true;
        if (g.GetComponentInParent<Damageable>() != null) return true;
        if (r is SkinnedMeshRenderer) return true;
        if (r.bounds.size.y < 0.02f) return true;               // mat nuoc / tam phang: muc C
        if (r.bounds.size.x * r.bounds.size.z > 400f) return true;
        return false;
    }

    static int DiemDongPhang(Bounds b, TerrainData td, Vector3 goc, float khe)
    {
        int n = 0;
        for (int ix = 0; ix <= 8; ix++)
            for (int iz = 0; iz <= 8; iz++)
            {
                float x = Mathf.Lerp(b.min.x, b.max.x, ix / 8f), z = Mathf.Lerp(b.min.z, b.max.z, iz / 8f);
                float d = Cao(td, goc, x, z);
                if (Mathf.Abs(b.min.y - d) < khe || Mathf.Abs(b.max.y - d) < khe) n++;
            }
        return n;
    }

    static void NgoaiPlay(string ten)
    {
        Ghi(""); Ghi("===== " + ten + " =====");
        var ter = Object.FindFirstObjectByType<Terrain>();
        var td = ter.terrainData; Vector3 goc = ter.transform.position;
        // A
        int soPhap = 0; foreach (var l in td.terrainLayers) if (l != null && l.normalMapTexture != null) soPhap++;
        var ve = Object.FindFirstObjectByType<DatVunVe>();
        Ghi(string.Format("A. terrain {0}x{0}, {1}/{2} lop co normal map; DatVunVe {3} manh, {4} luoi mau, vat lieu {5}", td.heightmapResolution, soPhap, td.terrainLayers.Length,
            ve != null ? ve.viTri.Length : 0, ve != null ? ve.luoi.Length : 0, ve != null && ve.vatLieu.Length > 0 ? ve.vatLieu[0].shader.name : "-"));
        Kiem(td.heightmapResolution == 1025 && soPhap == 4 && ve != null && ve.viTri.Length > 2000, "terrain / lop / dat vun chua dung");
        Vector3 gocCu; var cu = DatCu(ten, ter, out gocCu);
        if (cu == null) { Ghi("[LOI] thieu do cao goc"); loi++; return; }

        // B
        int datCu = 0, datMoi = 0, moiBi = 0; float lechChanMax = 0f, lechChanTong = 0f; int soVat = 0; var dsMoi = new List<string>();
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (BoQua(r)) continue;
            var b = r.bounds;
            float datTam = Cao(td, goc, b.center.x, b.center.z);
            if (b.min.y > datTam + 0.6f) continue;
            soVat++;
            bool bc = DiemDongPhang(b, cu, gocCu, 0.007f) >= 3, bm = DiemDongPhang(b, td, goc, 0.007f) >= 3;
            if (bc) datCu++; if (bm) datMoi++;
            if (bm && !bc) { moiBi++; if (dsMoi.Count < 8) dsMoi.Add(r.name + " @" + b.center.ToString("F1")); }
            // do cao dat doi TRONG CHAN VAT: lay mau tren khung bao CUC BO cua luoi (xoay theo vat) - khung thang truc cua the gioi
            // co goc nam ngoai than vat xoay (noi dat duoc phep doi)
            var mf = r.GetComponent<MeshFilter>(); var lb = mf != null && mf.sharedMesh != null ? mf.sharedMesh.bounds : new Bounds(r.transform.InverseTransformPoint(b.center), Vector3.zero);
            var M = r.transform.localToWorldMatrix;
            for (int ix = 0; ix <= 4; ix++)
                for (int iz = 0; iz <= 4; iz++)
                {
                    var w = M.MultiplyPoint3x4(new Vector3(Mathf.Lerp(lb.min.x, lb.max.x, ix / 4f), lb.min.y, Mathf.Lerp(lb.min.z, lb.max.z, iz / 4f)));
                    float d = Mathf.Abs(Cao(td, goc, w.x, w.z) - Cao(cu, gocCu, w.x, w.z));
                    lechChanMax = Mathf.Max(lechChanMax, d); lechChanTong += d;
                }
        }
        Ghi(string.Format("B. {0} vat cham dat: mang dong phang (< 7 mm, >= 3 diem) dat CU {1} -> dat MOI {2}; vat MOI bi {3}{4}; do cao dat trong chan vat (khung luoi xoay theo vat) doi TB {5:F4} m, lon nhat {6:F4} m",
            soVat, datCu, datMoi, moiBi, dsMoi.Count > 0 ? " (" + string.Join("; ", dsMoi) + ")" : "", lechChanTong / Mathf.Max(1, soVat * 25), lechChanMax));
        Kiem(moiBi == 0, "co vat MOI bi z-fighting voi mat dat moi");
        Kiem(lechChanMax < 0.01f, "dat duoi chan vat doi qua 1 cm");

        // C (tung dinh + trong tam tung tam giac cua LUOI NUOC - lan 1 lay hinh tron noi tiep khung bao: vung nuoc khong tron, dem ca dat ngoai vung)
        int nuocCu = 0, nuocMoi = 0, soNuoc = 0, soDiem = 0, khacNhau = 0; float kheMinMoi = 9f, kheMinCu = 9f;
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var t = r.transform; if (!(t.parent != null && t.parent.name == "MatNuoc")) continue;
            var mf = r.GetComponent<MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue;
            soNuoc++;
            var me = mf.sharedMesh; var v = me.vertices; var tri = me.triangles; var M = t.localToWorldMatrix;
            var diem = new List<Vector3>();
            foreach (var p in v) diem.Add(M.MultiplyPoint3x4(p));
            for (int i = 0; i < tri.Length; i += 3) diem.Add(M.MultiplyPoint3x4((v[tri[i]] + v[tri[i + 1]] + v[tri[i + 2]]) / 3f));
            foreach (var p in diem)
            {
                // kc / km = MAT NUOC - DAT (duong: nuoc lo ra tren dat; am: luoi nuoc chim duoi dat)
                float kc = p.y - Cao(cu, gocCu, p.x, p.z), km = p.y - Cao(td, goc, p.x, p.z);
                if (Mathf.Abs(kc) < 0.007f) nuocCu++; if (Mathf.Abs(km) < 0.007f) nuocMoi++;
                if (kc > -0.10f && Mathf.Abs(kc - km) > 0.001f) khacNhau++;          // nuoc lo / sat mep: dat phai giu nguyen
                if (kc <= -0.05f && km > -0.0495f) khacNhau++;                       // nuoc chim: dat moi khong xuong duoi mat nuoc + 5 cm
                kheMinMoi = Mathf.Min(kheMinMoi, km); kheMinCu = Mathf.Min(kheMinCu, kc); soDiem++;
            }
        }
        Ghi(string.Format("C. {0} mat nuoc, {1} diem (dinh + tam tam giac luoi nuoc): dat sat song song mat nuoc (|khe| < 7 mm) dat CU {2} -> MOI {3}; khe nho nhat cu {4:F3} / moi {5:F3} m; diem SAI (nuoc lo ma dat doi > 1 mm, hoac nuoc chim ma dat xuong < 5 cm tren nuoc) {6}",
            soNuoc, soDiem, nuocCu, nuocMoi, kheMinCu, kheMinMoi, khacNhau));
        // dem "sat song song" dao dong +-1-2 diem vi lam tron < 1 mm quanh nguong 7 mm (khacNhau = 0 da chung minh dat khong doi)
        Kiem(nuocMoi <= nuocCu + Mathf.Max(2, nuocCu / 200) && khacNhau == 0, "dat quanh mat nuoc bi doi / sat mat nuoc hon"); 

        // D
        if (ve != null)
        {
            int bi = 0;
            for (int k = 0; k < ve.viTri.Length; k++)
                if (MatDatSanSui.DongPhang(ve.luoi[ve.loai[k]], Matrix4x4.TRS(ve.viTri[k], ve.xoay[k], ve.tiLe[k]), td, goc)) bi++;
            Ghi(string.Format("D. dat vun {0} manh: manh co mat song song sat dat (< 12 do, < 2,5 cm) {1}", ve.viTri.Length, bi));
            Kiem(bi == 0, "dat vun co manh nam song song sat mat dat");
        }

        // E (dung tam suong y TamNhin, ca cach cu)
        int damMoi, damCu; float vuotMax;
        DoTamSuong(td, out damMoi, out damCu, out vuotMax);
        Ghi(string.Format("E. tam suong o 1 m nhac {0}: diem dat cao hon tam (dam xuyen) cach MOI {1} (vuot nhieu nhat {2:F3} m) · DOI CHUNG cach CU (do cao giua o) {3}", TamNhin.NhacSuongDat, damMoi, vuotMax, damCu));
        Kiem(damMoi == 0, "dat dam xuyen tam suong chien tranh");

        // F
        var rnd = new System.Random(4242);
        int damDia = 0, loLungDia = 0, damDiaCu = 0, loLungDiaCu = 0, soDia = 0;
        for (int i = 0; i < 60; i++)
        {
            var c = new Vector3(goc.x + 20 + (float)rnd.NextDouble() * (td.size.x - 40), 0, goc.z + 20 + (float)rnd.NextDouble() * (td.size.z - 40));
            c.y = Cao(td, goc, c.x, c.z);
            int d1, l1, d2, l2;
            DoDia(td, goc, c, 1.5f, 32, 2, 0.05f, true, out d1, out l1);
            DoDia(td, goc, c, 1.5f, 20, 2, 0.05f, false, out d2, out l2);
            damDia += d1; loLungDia += l1; damDiaCu += d2; loLungDiaCu += l2; soDia++;
        }
        Ghi(string.Format("F. {0} dia r 1,5 m nhac 0,05: diem dat dam xuyen dia MOI {1} / lo lung > 10 cm {2} · DOI CHUNG luoi thua cu {3} / {4}", soDia, damDia, loLungDia, damDiaCu, loLungDiaCu));
        Kiem(damDia * 4 <= damDiaCu + 4 && loLungDia * 2 <= loLungDiaCu + 4, "luoi dia moi khong do hon luoi cu");
        Object.DestroyImmediate(cu);
    }

    static void DoTamSuong(TerrainData td, out int damMoi, out int damCu, out float vuotMax)
    {
        Vector3 kich = td.size; int R = td.heightmapResolution; var hm = td.GetHeights(0, 0, R, R);
        int n = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(kich.x, kich.z) / 1.0f), 16, 250);
        float oX = kich.x / n; int bk = Mathf.CeilToInt(oX * 0.6f / (kich.x / (R - 1)));
        var moi = new float[n + 1, n + 1]; var cuo = new float[n + 1, n + 1];
        for (int z = 0; z <= n; z++)
            for (int x = 0; x <= n; x++)
            {
                float u = x / (float)n, v = z / (float)n;
                int cx = Mathf.RoundToInt(u * (R - 1)), cz = Mathf.RoundToInt(v * (R - 1)); float cao = 0f;
                for (int dz = -bk; dz <= bk; dz++) for (int dx = -bk; dx <= bk; dx++)
                    { int ix = cx + dx, iz = cz + dz; if (ix < 0 || iz < 0 || ix >= R || iz >= R) continue; cao = Mathf.Max(cao, hm[iz, ix]); }
                moi[z, x] = cao * kich.y + TamNhin.NhacSuongDat;
                cuo[z, x] = td.GetInterpolatedHeight(u, v) + TamNhin.NhacSuongDat;
            }
        damMoi = 0; damCu = 0; vuotMax = -9f;
        for (int iz = 0; iz < R; iz++)
            for (int ix = 0; ix < R; ix++)
            {
                float fx = ix / (float)(R - 1) * n, fz = iz / (float)(R - 1) * n;
                int gx = Mathf.Min((int)fx, n - 1), gz = Mathf.Min((int)fz, n - 1); float tx = fx - gx, tz = fz - gz;
                // tam giac nhu luoi TamNhin: (a, c, b) va (b, c, d) - duong cheo b-c
                float yM = TamGiac(moi, gx, gz, tx, tz), yC = TamGiac(cuo, gx, gz, tx, tz);
                float h = hm[iz, ix] * kich.y;
                if (h > yM) damMoi++; if (h > yC) damCu++;
                vuotMax = Mathf.Max(vuotMax, h - yM);
            }
    }

    static float TamGiac(float[,] g, int x, int z, float tx, float tz)
    {
        float a = g[z, x], b = g[z, x + 1], c = g[z + 1, x], d = g[z + 1, x + 1];
        if (tx + tz <= 1f) return a + (b - a) * tx + (c - a) * tz;
        return d + (c - d) * (1f - tx) + (b - d) * (1f - tz);
    }

    /// <summary>Dia polar nhu GroundRing.BuildDisc (moi = mat do moi tu tinh, cu = so nan / vong truyen vao) - dem diem luoi do cao trong dia
    /// cao hon mat dia (dam xuyen) hoac thap hon mat dia > 10 cm (lo lung).</summary>
    static void DoDia(TerrainData td, Vector3 goc, Vector3 tam, float r, int seg, int rings, float nhac, bool moi, out int dam, out int loLung)
    {
        if (moi)
        {
            rings = Mathf.Clamp(Mathf.Max(rings, Mathf.CeilToInt(r / GroundRing.KhoangDinhDia)), 1, 24);
            seg = Mathf.Clamp(Mathf.Max(seg, Mathf.CeilToInt(r * Mathf.PI * 2f / 0.3f)), 8, 72);
        }
        var y = new float[rings + 1, seg + 1];
        for (int i = 0; i <= rings; i++)
            for (int s = 0; s <= seg; s++)
            {
                float a = s / (float)seg * Mathf.PI * 2f, rr = r * i / rings;
                y[i, s] = Cao(td, goc, tam.x + Mathf.Cos(a) * rr, tam.z + Mathf.Sin(a) * rr) + nhac;
            }
        dam = 0; loLung = 0;
        float buoc = td.size.x / (td.heightmapResolution - 1);
        for (float dz = -r; dz <= r; dz += buoc)
            for (float dx = -r; dx <= r; dx += buoc)
            {
                float rr = Mathf.Sqrt(dx * dx + dz * dz); if (rr >= r * 0.98f) continue;
                float a = Mathf.Atan2(dz, dx); if (a < 0) a += Mathf.PI * 2f;
                float fi = rr / r * rings, fs = a / (Mathf.PI * 2f) * seg;
                int i0 = Mathf.Min((int)fi, rings - 1), s0 = Mathf.Min((int)fs, seg - 1); float ti = fi - i0, ts = fs - s0;
                float yd = Mathf.Lerp(Mathf.Lerp(y[i0, s0], y[i0, s0 + 1], ts), Mathf.Lerp(y[i0 + 1, s0], y[i0 + 1, s0 + 1], ts), ti);
                float h = Cao(td, goc, tam.x + dx, tam.z + dz);
                if (h > yd) dam++;
                else if (yd - h > 0.10f) loLung++;
            }
    }

    // ================================================================
    //  TRONG PLAY
    // ================================================================
    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find(Ten) != null) return;
        daBatDau = true;
        var go = new GameObject(Ten);
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; Ket(); yield break; }
        dir.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(e.gameObject);
        var pc = dir.player.GetComponent<PlayerController>();
        var cc = pc.GetComponent<CharacterController>();
        pc.GetComponent<Damageable>().maxHealth = pc.GetComponent<Damageable>().health = 1e7f;
        TamNhin.BatTrongPhepThu = true;
        yield return new WaitForSeconds(1f);
        Ghi(""); Ghi("===== Act2 trong Play =====");

        // G. di that qua cho co o ga / nut (tim huong co nhieu diem lom trong 10 m)
        var ter = Terrain.activeTerrain; var td = ter.terrainData; Vector3 g = ter.transform.position;
        Vector3 xp = pc.transform.position, huong = Vector3.forward; float tot = -1;
        for (int k = 0; k < 16; k++)
        {
            var h = Quaternion.Euler(0, k * 22.5f, 0) * Vector3.forward;
            if (Physics.SphereCast(xp + Vector3.up, 0.6f, h, out _, 10f, 1 << 0, QueryTriggerInteraction.Ignore)) continue;
            float diem = 0; for (float d = 0; d < 10f; d += 0.13f) { var p = xp + h * d; float u = (p.x - g.x) / td.size.x, v = (p.z - g.z) / td.size.z; diem += td.GetSteepness(u, v); }
            if (diem > tot) { tot = diem; huong = h; }
        }
        bool tuDoc = pc.tuDocInput; pc.tuDocInput = false;
        var goi = GoiInput.Rong(Time.deltaTime); goi.huongDi = huong;
        int khung = 0, chamDat = 0; float maxHo = 0; Vector3 p0 = pc.transform.position; float t0 = Time.time;
        while (Time.time - t0 < 5f)
        {
            goi.dt = Time.deltaTime; pc.input = goi;
            yield return null; khung++;
            if (cc.isGrounded) chamDat++;
            float dat = ter.SampleHeight(pc.transform.position) + g.y;
            maxHo = Mathf.Max(maxHo, Mathf.Abs(pc.transform.position.y - dat));
        }
        pc.input = GoiInput.Rong(Time.deltaTime); pc.tuDocInput = tuDoc;
        Vector3 di = pc.transform.position - p0; di.y = 0;
        Ghi(string.Format("G. di that 5 s huong co do doc tong {0:F0}: {1:F1} m ({2:F2} m/s), cham dat {3}/{4} khung, goc cach dat lon nhat {5:F3} m", tot, di.magnitude, di.magnitude / 5f, chamDat, khung, maxHo));
        // (goc cach dat tai TAM khong phai do lo lung: than nhan vat r 0,55 bac ngang mieng o voi - ban chan do menu 109)
        Kiem(di.magnitude > 15f && chamDat >= khung * 0.9f, "di lai tren dat moi bi ket / mat cham dat");

        // H. dat vun ve + anh
        yield return null;
        Ghi(string.Format("H. dat vun moi khung: {0} manh trong khung may quay, {1} lan goi ve (tren {2} manh)", DatVunVe.SoManhDaVe, DatVunVe.SoLanGoi,
            Object.FindFirstObjectByType<DatVunVe>().viTri.Length));
        Kiem(DatVunVe.SoManhDaVe > 30 && DatVunVe.SoLanGoi <= 30, "dat vun khong ve / qua nhieu lan goi");
        var ccsd = Object.FindFirstObjectByType<ChuyenChieuSangDem>();
        // 4 cho co dinh + 2 cho canh O VOI sau nhat (tim tren luoi do cao: so voi trung binh 3 m quanh do)
        var dsCho = new List<Vector3> { new Vector3(0, 0, -10.5f), new Vector3(25, 0, 20), new Vector3(-30, 0, -25), new Vector3(40, 0, -40) };
        {
            Vector3 gocCu; var cu = DatCu("Act2", ter, out gocCu);
            var tot2 = new List<Vector3>();
            for (int k = 0; k < 2 && cu != null; k++)
            {
                float sauNhat = 0f; Vector3 chon = Vector3.zero;
                for (float z = g.z + 8; z < g.z + td.size.z - 8; z += 0.4f)
                    for (float x = g.x + 8; x < g.x + td.size.x - 8; x += 0.4f)
                    {
                        var w = new Vector3(x, 0, z);
                        bool gan = false; foreach (var q in tot2) if ((q - w).sqrMagnitude < 400f) gan = true; if (gan) continue;
                        float sau = Cao(cu, gocCu, x, z) - Cao(td, g, x, z);       // dat moi thap hon dat cu bao nhieu
                        if (sau > sauNhat) { sauNhat = sau; chon = w; }
                    }
                tot2.Add(chon);
                dsCho.Add(chon + new Vector3(0, 0, -3.5f));
                Ghi(string.Format("H. o voi khoet sau thu {0}: ({1:F1}, {2:F1}) sau {3:F2} m duoi dat cu", k + 1, chon.x, chon.z, sauNhat));
            }
            if (cu != null) Object.Destroy(cu);
        }
        var cho = dsCho.ToArray();
        for (int buoi = 0; buoi < 2; buoi++)
        {
            if (ccsd != null) { ccsd.enabled = false; ccsd.ApGiay(buoi == 0 ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); }
            for (int i = 0; i < cho.Length; i++)
            {
                var p = cho[i]; p.y = ter.SampleHeight(p) + g.y + 0.3f;
                cc.enabled = false; pc.transform.position = p; cc.enabled = true;
                yield return new WaitForSeconds(0.8f);
                yield return new WaitForEndOfFrame();
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(string.Format("PlayTestShots/matdat_play_{0}_{1}.png", buoi == 0 ? "dem" : "ngay", i), tex.EncodeToPNG());
                Object.Destroy(tex);
            }
        }
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/thumatdatsansui.txt", bao.ToString());
        TamNhin.BatTrongPhepThu = false;
        TranHienTai.Xoa();
        var rac = GameObject.Find(Ten);
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        Debug.Log("[ThuMatDat] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
