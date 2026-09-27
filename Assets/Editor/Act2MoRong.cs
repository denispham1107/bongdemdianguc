using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MO RONG BAN DO ACT2 THEM ~50% DIEN TICH (nguoi dung 27/09/2026, menu 86).
///
/// Nguoi dung: "cho dien tich trong ban do rong them 50%, phan dien tich moi phai nam trong vung hang rao; them cay coi,
/// bia mo va 6 vung nuoc". Chon: mo DEU 4 phia; cay / bia THUA hon mot nua vung cu; hang rao DUNG LAI bang Blender MCP
/// (giu nhip cot); them da, lo lua, nha mo.
///
/// HANG RAO (Blender MCP, <c>CongCu/Blender/hang_rao_rong.blend</c> -> <c>BlenderMaps/GraveyardAct2/hang_rao_rong.fbx</c>):
/// rao cu la mot luoi liet vuong 107,4 m, cot da cach deu <see cref="NhipCot"/> 5,086 m (21 khoang moi canh), chan rao chay
/// len xuong theo mat dat, giua moi canh co cong. Cat rao cu thanh 4 phan tu o giua hai cot (x = y = 10,17 theo Blender - tranh
/// cong), day moi phan ra ngoai <see cref="D"/>, lap moi khe giua canh bang <see cref="SoMoDun"/> mo-dun cot cat tu chinh canh
/// ay (nan nhe theo chieu cao 1-7 cm cho khop hai dau), han moi cat (0 canh ho). 5 mo-dun -> D = 12,71 m, rao 132,8 m, dien
/// tich trong rao +53% (gan 50% nhat khi giu dung nhip cot).
///
/// MAT DAT: nen phang goc (<c>Act2_DoCaoPhang.bytes</c>) la LONG CHAO - doc len dan tu ~40 m ra mep rao (cao them ~2 m), chan
/// rao cu nam tren mep chao. Nen moi: vung cu toi s = 51 m (s = max(|x|,|z|)) GIU Y TUNG DIEM (ca dam lay DamLay_10 sat
/// mep); tu 51 ra 65,8 m keo dan bang DUNG phep dich cua hang rao (q = p - W(s) * clamp(p - cat, +-D), W tang tuyen tinh
/// 0 -> 1 - tuyen tinh de phep keo khong gap nep: dao ham nho nhat 1 - D/14,8 = 0,14) -> duoi chan rao moi dung nen duoi chan
/// rao cu. Vanh them go ghe Perlin cua <see cref="Act2GoGhe"/> (mat na vanh 0,6 lan, san phang duoi nha mo moi).
/// ⚠️ VUNG CU LAY THANG DO CAO VA NET TO THAT, khong tinh lai bang cong thuc: lan chay dau tinh lai ca ban do thi 4,9% diem
/// lech hon 5 cm (toi 0,96 m) quanh dam lay / ho - long chao dao SAU lop go ghe. Noi vung cu voi vanh trong dai 51 -> 54,5 m.
/// Net to vanh: nhieu co chet / dat troc cua <c>Act2Terrain.ToMau</c> + soi tren doc + doi cho theo go - trung cua
/// <c>Act2GoGhe.ToLaiTheoDiaHinh</c>; dat troc quanh moi bia moi. File nen phang / net to goc duoc ghi lai cho co moi.
///
/// DO DAC MOI (nhan ban do dac co san - dung luoi, collider, vat lieu, diem moi lua): bia / cay / da THUA mot nua mat do
/// vung cu, 2 nha mo (quay truc -Y ra ngoai nhu 4/5 nha mo cu), 4 lo lua (prefab LoLuaDa), 6 dam lay dao bang
/// <see cref="Act2DamLay.DaoThem"/> SAU khi mat dat xong (bo nuoc do tren dat da dao nen khop tu nhien).
/// Moi vat (cu va moi) duoc dat lai theo phan dat duoi chan no da dich - do sau chon giu nguyen.
///
/// CHAY MOT LAN: terrain da rong hon 120 m thi bo qua (chay lai la dich hai lan).
/// </summary>
public static class Act2MoRong
{
    public const float NhipCot = 106.8f / 21f;
    public const int SoMoDun = 5;
    public static float D { get { return SoMoDun * NhipCot / 2f; } }
    /// <summary>Cat rao theo Blender x = y = 10,17 -> Unity x = -10,17, z = -10,17. ⚠ Luoi rao nhap vao Unity XOAY 180 do:
    /// Unity x = -Blender x, Unity z = -Blender y (do bang chan 84 cot rao cu tren nen cu: lech chuan 0,256 m, con
    /// "x = +Blender x" cho 0,496 m; cong mo duy nhat Blender x -12,7 hien ra o Unity x +12,7). Lan dau toi lay x = +Blender x
    /// nen dat vanh bi keo NGUOC chieu voi rao doc canh bac/nam - vet duong cu dan toi tuong kin cach cong that 25 m.</summary>
    public const float CatX = -10.17f, CatZ = -10.17f;
    public const float NuaCu = 54.6f;
    public static float NuaMoi { get { return NuaCu + D; } }
    public static float NuaRaoMoi { get { return 53.7f + D; } }
    /// <summary>Tu day tro ra nen bi keo theo phep dich cua rao; toi WHet thi keo du D.</summary>
    const float WBatDau = 51f, WHet = 65.8f;

    const string DuongNen = "Assets/Terrain/Act2_DoCaoPhang.bytes";
    const string DuongTo = "Assets/Terrain/Act2_ToMauGoc.bytes";
    const string DuongRao = "Assets/BlenderMaps/GraveyardAct2/hang_rao_rong.fbx";
    const int Hat = 20260927;

    // So do dac moi - THUA mot nua vung cu (vung cu: 452 bia, 58 cay, 229 da tren ~11 300 m2; vanh moi ~5 000 m2)
    public const int SoBiaMoi = 100, SoCayMoi = 13, SoDaMoi = 51, SoNhaMoMoi = 2, SoLoMoi = 4, SoVungNuocMoi = 6;
    const float BanKinhVungNuoc = 4.0f;

    /// <summary>Ket qua lan chay gan nhat - phep thu doc.</summary>
    public static string BaoCao = "";

    [MenuItem("Diablo 2.5D/86. Mo rong Act2 them 50% (rao moi + vanh dat + do dac)", false, 175)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[MoRong] Thoat Play truoc da."); return; }
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Act2") { Debug.LogError("[MoRong] Mo canh Act2 truoc da."); return; }
        var terr = Object.FindFirstObjectByType<Terrain>();
        var world = GameObject.Find("World");
        if (terr == null || world == null) { Debug.LogError("[MoRong] Thieu Terrain / World."); return; }
        var td = terr.terrainData;
        if (td.size.x > 120f) { Debug.LogWarning("[MoRong] Terrain da rong " + td.size.x + " m - da mo rong roi, bo qua."); return; }

        var bc = new System.Text.StringBuilder();
        int N = td.heightmapResolution, M = td.alphamapResolution, L = td.alphamapLayers;
        float buocCu = td.size.x / (N - 1);
        Vector3 gocCu = terr.transform.position;

        // ---- 0. Du lieu cu ----
        // VUNG CU LAY THANG DO CAO + NET TO THAT, KHONG TINH LAI tu "nen phang + go ghe": lan chay dau tinh lai ca ban do,
        // 12 895 diem (4,9%) lech hon 5 cm - lon nhat 0,96 m - dung quanh cac dam lay / ho nuoc (long chao dao SAU lop go ghe).
        float[,] nen = DocFloat(DuongNen, N);
        var toGoc = DocByte(DuongTo, M, L);
        if (nen == null || toGoc == null) { Debug.LogError("[MoRong] Thieu Act2_DoCaoPhang / Act2_ToMauGoc (chay menu 10 mot lan truoc)."); return; }
        var h01Cu = td.GetHeights(0, 0, N, N);
        var caoCu = new float[N, N];
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++) caoCu[z, x] = gocCu.y + h01Cu[z, x] * td.size.y;
        var toCu = td.GetAlphamaps(0, 0, M, M);

        // ---- 1. Dat duoi chan moi do dac CU ----
        var truocDich = new Dictionary<Transform, float>();
        foreach (var tr in GomDoDac(world)) truocDich[tr] = DatTai(terr, tr.position);
        var nguoiChoi = GameObject.Find("Player");
        if (nguoiChoi != null) truocDich[nguoiChoi.transform] = DatTai(terr, nguoiChoi.transform.position);
        var mauCu = new List<Vector3>();
        var rMau = new System.Random(7);
        for (int i = 0; i < 3000; i++)
        {
            float x = (float)(rMau.NextDouble() * 2 - 1) * 50f, z = (float)(rMau.NextDouble() * 2 - 1) * 50f;
            mauCu.Add(new Vector3(x, DatTai(terr, new Vector3(x, 0, z)), z));
        }

        // ---- 2. Chon cho cho do dac moi (chi toa do x/z) ----
        var rnd = new System.Random(Hat);
        var chiem = new List<Vector3>();       // x, ban kinh, z
        foreach (var tr in truocDich.Keys) chiem.Add(new Vector3(tr.position.x, 1.0f, tr.position.z));
        // Cho cong giua canh cu (x = 0 / z = 0) sau khi dich: ca hai deu nam phia "+" cua duong cat nen dich +D
        var congRao = new[] { new Vector2(D, NuaRaoMoi), new Vector2(D, -NuaRaoMoi), new Vector2(NuaRaoMoi, D), new Vector2(-NuaRaoMoi, D) };
        var choNhaMo = new List<Vector2> { new Vector2(60f, 42f), new Vector2(-60f, -42f) };
        foreach (var p in choNhaMo) chiem.Add(new Vector3(p.x, 5.5f, p.y));
        var choLo = new List<Vector2> { new Vector2(20f, 61f), new Vector2(-22f, -61f), new Vector2(61f, -18f), new Vector2(-61f, 22f) };
        foreach (var p in choLo) chiem.Add(new Vector3(p.x, 2.5f, p.y));
        // Vung nuoc, cay, bia, da chon SAU buoc 3 - vung nuoc can biet do cao de tim cho phang
        // ---- 3. Do cao moi: vung cu giu that, vanh = nen keo + go ghe vanh, noi 51 -> 54,5 ----
        float nuaMoi = NuaMoi, canhMoi = 2f * nuaMoi, buocMoi = canhMoi / (N - 1);
        var nenMoi = new float[N, N];
        var caoMoi = new float[N, N];
        float thap = 9e9f, cao = -9e9f;
        for (int z = 0; z < N; z++)
            for (int x = 0; x < N; x++)
            {
                float px = -nuaMoi + x * buocMoi, pz = -nuaMoi + z * buocMoi;
                float s = Mathf.Max(Mathf.Abs(px), Mathf.Abs(pz));
                Vector2 q = KeoVe(px, pz);
                float fn = LayMau(nen, N, gocCu.x, gocCu.z, buocCu, q.x, q.y);
                nenMoi[z, x] = fn;
                float them = Act2GoGhe.BaTang(px, pz) * Act2GoGhe.MatNaRia(px, pz);
                // San phang duoi nha mo moi (keo go ghe ve gia tri tai tam nhu Act2GoGhe.KeoVeTam)
                foreach (var nm in choNhaMo)
                {
                    float d = Vector2.Distance(nm, new Vector2(px, pz)) / 4.5f;
                    if (d >= 8.5f / 4.5f) continue;
                    float tam = Act2GoGhe.BaTang(nm.x, nm.y) * Act2GoGhe.MatNaRia(nm.x, nm.y);
                    them = d <= 1f ? tam : Mathf.Lerp(tam, them, Mathf.SmoothStep(0f, 1f, (d - 1f) / (8.5f / 4.5f - 1f)));
                }
                float vanh = fn + them;
                float t = Mathf.SmoothStep(0f, 1f, (s - 51f) / 3.5f);
                float y = t >= 1f ? vanh : Mathf.Lerp(LayMau(caoCu, N, gocCu.x, gocCu.z, buocCu, px, pz), vanh, t);
                caoMoi[z, x] = y;
                if (y < thap) thap = y;
                if (y > cao) cao = y;
            }

        // ---- 3b. Vung nuoc o cho PHANG nhat vanh (lan chay truoc dat co dinh: 2/6 dam nam tren doc, bo lech mat nuoc 0,43 m) ----
        var vungNuoc = ChonVungNuocPhang(caoMoi, N, -nuaMoi, buocMoi, SoVungNuocMoi, chiem, congRao, bc);
        foreach (var p in vungNuoc) chiem.Add(new Vector3(p.x, BanKinhVungNuoc * 1.25f + 0.8f, p.y));
        var choCay = Rai(rnd, SoCayMoi, 55.5f, 62.2f, 3.2f, 7.0f, chiem, congRao);
        var choBia = Rai(rnd, SoBiaMoi, 55.0f, 64.2f, 0.85f, 1.7f, chiem, congRao);
        var choDa = Rai(rnd, SoDaMoi, 54.8f, 64.6f, 0.6f, 1.2f, chiem, congRao);

        // ---- 4. Doi terrain sang co moi ----
        Undo.RegisterCompleteObjectUndo(td, "Mo rong Act2");
        float khoang = Mathf.Max(1f, cao - thap);
        var h01 = new float[N, N];
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++) h01[z, x] = (caoMoi[z, x] - thap) / khoang;
        td.size = new Vector3(canhMoi, khoang, canhMoi);
        td.SetHeights(0, 0, h01);
        terr.transform.position = new Vector3(-nuaMoi, thap, -nuaMoi);
        GhiFloat(DuongNen, nenMoi, N);
        Physics.SyncTransforms();

        // ---- 5. Net to: vung cu giu that, vanh to theo quy tac cua Act2Terrain + Act2GoGhe ----
        float buocToCu = 109.2f / (M - 1), buocToMoi = canhMoi / (M - 1);
        var toNenMoi = new float[M, M, L];                 // net to GOC (truoc doc/chenh) - ghi ra Act2_ToMauGoc
        for (int z = 0; z < M; z++)
            for (int x = 0; x < M; x++)
            {
                float px = -nuaMoi + x * buocToMoi, pz = -nuaMoi + z * buocToMoi;
                float s = Mathf.Max(Mathf.Abs(px), Mathf.Abs(pz));
                float g = NhieuCo(px, pz);
                float t = Mathf.Clamp01((s - 51f) / (NuaCu - 51f));
                for (int l = 0; l < L; l++)
                {
                    float moiL = l == Act2Terrain.LopCoChet ? g : l == Act2Terrain.LopDatTroc ? 1f - g : 0f;
                    float cuL = s <= NuaCu ? LayMauTo(toGoc, M, L, gocCu.x, gocCu.z, buocToCu, px, pz, l) : moiL;
                    toNenMoi[z, x, l] = Mathf.Lerp(cuL, moiL, t);
                }
            }
        foreach (var p in choBia) DamNatQuanh(toNenMoi, M, L, -nuaMoi, buocToMoi, p);
        ChuanHoa(toNenMoi, M, L);
        GhiByte(DuongTo, toNenMoi, M, L);
        // Net to vanh = net goc + soi tren doc + doi cho co / dat theo go - trung (dung quy tac Act2GoGhe.ToLaiTheoDiaHinh)
        var chenh = Act2GoGhe.ChenhSoVoiXungQuanh(td, 7f);
        var toMoi = new float[M, M, L];
        var a = new float[L];
        for (int z = 0; z < M; z++)
        {
            float v = (float)z / (M - 1);
            for (int x = 0; x < M; x++)
            {
                float u = (float)x / (M - 1);
                float px = -nuaMoi + x * buocToMoi, pz = -nuaMoi + z * buocToMoi;
                float s = Mathf.Max(Mathf.Abs(px), Mathf.Abs(pz));
                float t = Mathf.SmoothStep(0f, 1f, (s - 51f) / 3.5f);
                for (int l = 0; l < L; l++) a[l] = toNenMoi[z, x, l];
                if (t > 0f)
                {
                    float soi = Mathf.Clamp01((td.GetSteepness(u, v) / 90f - 0.056f) * 5.5f);
                    float con = 0f;
                    for (int l = 0; l < L; l++) if (l != Act2Terrain.LopSoiDa) con += a[l];
                    if (con > 1e-4f) { float ti = (1f - soi) / con; for (int l = 0; l < L; l++) if (l != Act2Terrain.LopSoiDa) a[l] *= ti; }
                    else a[Act2Terrain.LopDatTroc] = 1f - soi;
                    a[Act2Terrain.LopSoiDa] = soi;
                    int hx = Mathf.Clamp(Mathf.RoundToInt(u * (N - 1)), 0, N - 1), hz = Mathf.Clamp(Mathf.RoundToInt(v * (N - 1)), 0, N - 1);
                    float k = Mathf.Clamp(chenh[hz, hx] / 0.40f, -1f, 1f);
                    float tong = a[Act2Terrain.LopCoChet] + a[Act2Terrain.LopDatTroc];
                    if (tong > 1e-4f)
                    {
                        float pCo = Mathf.Clamp01(a[Act2Terrain.LopCoChet] / tong + k * 0.34f);
                        a[Act2Terrain.LopCoChet] = tong * pCo;
                        a[Act2Terrain.LopDatTroc] = tong * (1f - pCo);
                    }
                }
                for (int l = 0; l < L; l++)
                {
                    float cuL = s <= NuaCu ? LayMauTo(toCu, M, L, gocCu.x, gocCu.z, buocToCu, px, pz, l) : a[l];
                    toMoi[z, x, l] = Mathf.Lerp(cuL, a[l], t);
                }
            }
        }
        ChuanHoa(toMoi, M, L);
        td.SetAlphamaps(0, 0, toMoi);

        // ---- 6. Dat lai do dac CU theo phan dat da dich ----
        float dichMax = 0f; int soDich = 0;
        foreach (var kv in truocDich)
        {
            if (kv.Key == null) continue;
            float dich = DatTai(terr, kv.Key.position) - kv.Value;
            if (Mathf.Abs(dich) < 0.0005f) continue;
            kv.Key.position += Vector3.up * dich;
            var vn = kv.Key.GetComponent<VungNuoc>();
            if (vn != null) vn.mucNuoc += dich;       // muc nuoc di cung mat nuoc, khong thi cho loi lech cho thay nuoc
            soDich++;
            if (Mathf.Abs(dich) > Mathf.Abs(dichMax)) dichMax = dich;
        }

        // ---- 7. Dat do dac moi len mat dat da xong ----
        var nhomBia = world.transform.Find("BiaMo"); var nhomCay = world.transform.Find("Cay");
        var nhomDa = world.transform.Find("Da"); var nhomNhaMo = world.transform.Find("NhaMo");
        var truocGoc = new Dictionary<Transform, float>(truocDich);     // dat CU duoi vat goc - de lay do sau chon
        int dem = 0;
        foreach (var p in choBia) NhanBan(nhomBia, p, rnd, truocGoc, terr, true, "_V" + (++dem).ToString("000"));
        dem = 0; foreach (var p in choCay) NhanBan(nhomCay, p, rnd, truocGoc, terr, true, "_V" + (++dem).ToString("000"));
        dem = 0; foreach (var p in choDa) NhanBan(nhomDa, p, rnd, truocGoc, terr, true, "_V" + (++dem).ToString("000"));
        for (int i = 0; i < choNhaMo.Count; i++)
        {
            var o = NhanBan(nhomNhaMo, choNhaMo[i], rnd, truocGoc, terr, false, "_V" + (i + 1).ToString("000"));
            // truc -Y cua luoi chi RA NGOAI ban do nhu 4/5 nha mo cu
            Vector3 f = o.TransformDirection(new Vector3(0f, -1f, 0f)); f.y = 0f;
            Vector3 ra = new Vector3(Mathf.Sign(choNhaMo[i].x), 0f, 0f);
            o.rotation = Quaternion.FromToRotation(f.normalized, ra) * o.rotation;
        }
        var nhomLo = GameObject.Find("LoLua_Act2");
        var loMau = nhomLo != null && nhomLo.transform.childCount > 1 ? nhomLo.transform.GetChild(1) : null;
        var prefabLo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/LoLuaDa/LoLuaDa.prefab");
        int soLo = 0;
        if (nhomLo != null && prefabLo != null && loMau != null)
        {
            float nhung = loMau.position.y - DatTai(terr, loMau.position);     // lo mau da dat lai o buoc 6
            for (int i = 0; i < choLo.Count; i++)
            {
                var lo = (GameObject)PrefabUtility.InstantiatePrefab(prefabLo, nhomLo.transform);
                lo.name = "LoLua_" + (nhomLo.transform.childCount - 1).ToString("00");
                foreach (var c in loMau.GetComponents<Component>())
                {
                    if (c is Transform) continue;
                    // CapsuleCollider cua lo gan TRONG CANH (menu 54), prefab khong co - thieu thi them, khong thi lo moi di xuyen
                    var dich = lo.GetComponent(c.GetType());
                    if (dich == null) dich = lo.AddComponent(c.GetType());
                    if (dich != null) EditorUtility.CopySerialized(c, dich);
                }
                Vector3 p = new Vector3(choLo[i].x, 0f, choLo[i].y);
                lo.transform.position = new Vector3(p.x, DatTai(terr, p) + nhung, p.z);
                lo.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                Undo.RegisterCreatedObjectUndo(lo, "Mo rong Act2");
                soLo++;
            }
        }

        // ---- 9. Dao 6 dam lay ----
        int soNuocCu = world.transform.Find("MatNuoc").childCount;
        var bk = new List<float>(); foreach (var _ in vungNuoc) bk.Add(BanKinhVungNuoc);
        int soNuoc = Act2DamLay.DaoThem(terr, world.transform, vungNuoc, bk, soNuocCu, Hat + 1);

        // ---- 10. Hang rao moi ----
        var rao = world.transform.Find("HangRao").GetChild(0);
        Mesh luoiRao = null;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(DuongRao)) if (o is Mesh) luoiRao = (Mesh)o;
        if (luoiRao != null)
        {
            rao.GetComponent<MeshFilter>().sharedMesh = luoiRao;
            var mc = rao.GetComponent<MeshCollider>(); if (mc != null) mc.sharedMesh = luoiRao;
        }

        // ---- 11. Ban kinh cho xuat phat (ti le nhu cu: 50 / 53,7) ----
        float banKinhSan = Mathf.Round(50f * NuaRaoMoi / 53.7f);
        foreach (var gd in Object.FindObjectsByType<GameDirector>(FindObjectsInactive.Include)) { Undo.RecordObject(gd, "Mo rong Act2"); gd.arenaRadius = banKinhSan; }
        foreach (var gb in Object.FindObjectsByType<GameBootstrap>(FindObjectsInactive.Include)) { Undo.RecordObject(gb, "Mo rong Act2"); gb.arenaRadius = banKinhSan; }

        // ---- 12. Doi chieu vung cu ----
        float lechMax = 0f, lechTong = 0f;
        foreach (var m in mauCu)
        {
            float d = Mathf.Abs(DatTai(terr, m) - m.y);
            lechTong += d; if (d > lechMax) lechMax = d;
        }

        EditorUtility.SetDirty(td);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        bc.AppendFormat("terrain {0:F2} m (cu 109,20), rao moi nua canh {1:F2} m (cu 53,70), D {2:F3} m, dien tich trong rao x{3:F3}\n",
            canhMoi, NuaRaoMoi, D, (NuaRaoMoi * NuaRaoMoi) / (53.7f * 53.7f));
        bc.AppendFormat("do dac moi: bia {0}/{1}, cay {2}/{3}, da {4}/{5}, nha mo {6}, lo lua {7}, dam lay {8}\n",
            choBia.Count, SoBiaMoi, choCay.Count, SoCayMoi, choDa.Count, SoDaMoi, choNhaMo.Count, soLo, soNuoc);
        bc.AppendFormat("dat lai {0} vat, dich nhieu nhat {1:F2} m; vung cu (3000 diem |x|,|z| <= 50): lech TB {2:F4} m, lon nhat {3:F4} m\n",
            soDich, dichMax, lechTong / mauCu.Count, lechMax);
        bc.AppendFormat("ban kinh cho xuat phat {0} m (cu 50); hang rao {1}\n", banKinhSan, luoiRao != null ? luoiRao.name : "KHONG NAP DUOC");
        BaoCao = bc.ToString();
        Debug.Log("[MoRong] " + BaoCao);
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText("PlayTestShots/mo_rong_act2.txt", BaoCao);
    }

    // ================================================================

    /// <summary>Diem tren nen CU tuong ung voi diem p cua nen moi (phep dich cua hang rao, tang dan theo s).</summary>
    public static Vector2 KeoVe(float px, float pz)
    {
        float s = Mathf.Max(Mathf.Abs(px), Mathf.Abs(pz));
        float w = Mathf.Clamp01((s - WBatDau) / (WHet - WBatDau));
        float d = D;
        return new Vector2(px - w * Mathf.Clamp(px - CatX, -d, d), pz - w * Mathf.Clamp(pz - CatZ, -d, d));
    }

    static float DatTai(Terrain t, Vector3 p) { return t.SampleHeight(new Vector3(p.x, 0f, p.z)) + t.transform.position.y; }
    static float DatTaiTruoc(Dictionary<Transform, float> d, Transform t) { float v; return d.TryGetValue(t, out v) ? v : t.position.y; }

    static List<Transform> GomDoDac(GameObject world)
    {
        var ds = new List<Transform>();
        foreach (var ten in new[] { "BiaMo", "Da", "Cay", "NhaMo", "MatNuoc" })
        {
            var g = world.transform.Find(ten);
            if (g != null) foreach (Transform c in g) ds.Add(c);
        }
        foreach (var ten in new[] { "BuiCoRai", "LoLua_Act2" })
        {
            var g = GameObject.Find(ten);
            if (g != null) foreach (Transform c in g.transform) ds.Add(c);
        }
        return ds;
    }

    /// <summary>Chon n tam vung nuoc o cho do cao chenh it nhat trong ban kinh bo (vanh s 57..61), cach cong rao / vat da chiem,
    /// cach nhau >= 24 m va moi canh toi da 2 - de rai deu bon phia chu khong don vao mot canh phang.</summary>
    static List<Vector2> ChonVungNuocPhang(float[,] cao, int N, float goc, float buoc, int n, List<Vector3> chiem, Vector2[] cong,
                                           System.Text.StringBuilder bc)
    {
        float r = BanKinhVungNuoc * 1.25f;
        var ungVien = new List<KeyValuePair<float, Vector2>>();
        for (float x = -61f; x <= 61f; x += 1f)
            for (float z = -61f; z <= 61f; z += 1f)
            {
                float s = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
                if (s < 57f || s > 61f) continue;
                var p = new Vector2(x, z);
                bool vuong = false;
                foreach (var c in cong) if (Vector2.Distance(c, p) < 6f + r) { vuong = true; break; }
                if (vuong) continue;
                foreach (var c in chiem) if (Vector2.Distance(new Vector2(c.x, c.z), p) < c.y + r + 0.8f) { vuong = true; break; }
                if (vuong) continue;
                // CHAU PHAI KHEP: gờ tran (huong thap nhat cua vanh ngoai vung dao 1,3r..1,45r) cao hon muc nuoc du kien,
                // khong thi phia dat doc xuong mat nuoc khong gap bo, dia nuoc lo lung voi vien tron cung (DamLay_15 lan 2: 0,118 m)
                float hTam = LayMau(cao, N, goc, goc, buoc, x, z), tran = 9e9f;
                for (int k = 0; k < 32; k++)
                {
                    float g = k * Mathf.PI / 16f, vanh = -9e9f;
                    for (float bk = BanKinhVungNuoc * 1.3f; bk <= BanKinhVungNuoc * 1.45f + 1e-3f; bk += 0.1f)
                        vanh = Mathf.Max(vanh, LayMau(cao, N, goc, goc, buoc, x + Mathf.Cos(g) * bk, z + Mathf.Sin(g) * bk));
                    tran = Mathf.Min(tran, vanh);
                }
                float day = hTam - 0.35f;                                          // do sau dao trung binh (0,26..0,44)
                if (Mathf.Min(day + 0.20f, tran - 0.15f) - day < 0.12f) continue;   // nuoc con < 12 cm thi bo
                float thap = 9e9f, caoNhat = -9e9f;
                for (int vong = 0; vong <= 3; vong++)
                    for (int k = 0; k < (vong == 0 ? 1 : 16); k++)
                    {
                        float bk = r * vong / 3f, g = k * Mathf.PI / 8f;
                        float y = LayMau(cao, N, goc, goc, buoc, x + Mathf.Cos(g) * bk, z + Mathf.Sin(g) * bk);
                        if (y < thap) thap = y;
                        if (y > caoNhat) caoNhat = y;
                    }
                ungVien.Add(new KeyValuePair<float, Vector2>(caoNhat - thap, p));
            }
        ungVien.Sort((a, b) => a.Key.CompareTo(b.Key));
        var ds = new List<Vector2>();
        var soTheoCanh = new int[4];
        foreach (var uv in ungVien)
        {
            if (ds.Count >= n) break;
            var p = uv.Value;
            int canh = Mathf.Abs(p.x) >= Mathf.Abs(p.y) ? (p.x > 0 ? 0 : 1) : (p.y > 0 ? 2 : 3);
            if (soTheoCanh[canh] >= 2) continue;
            bool gan = false;
            foreach (var q in ds) if (Vector2.Distance(q, p) < 24f) { gan = true; break; }
            if (gan) continue;
            ds.Add(p); soTheoCanh[canh]++;
            bc.AppendLine("Vung nuoc moi (" + p.x + ", " + p.y + "): chenh cao trong " + r.ToString("F1") + " m = " + uv.Key.ToString("F3") + " m");
        }
        return ds;
    }

    /// <summary>Rai n diem trong vanh s in [sMin, sMax], cach vat da chiem (x, ban kinh, z) va cach nhau khoangCach.</summary>
    static List<Vector2> Rai(System.Random rnd, int n, float sMin, float sMax, float banKinhMinh, float khoangCach,
                             List<Vector3> chiem, Vector2[] cong)
    {
        var ds = new List<Vector2>();
        for (int lan = 0; lan < n * 400 && ds.Count < n; lan++)
        {
            float x = (float)(rnd.NextDouble() * 2 - 1) * sMax, z = (float)(rnd.NextDouble() * 2 - 1) * sMax;
            float s = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
            if (s < sMin || s > sMax) continue;
            var p = new Vector2(x, z);
            bool vuong = false;
            foreach (var c in cong) if (Vector2.Distance(c, p) < 6f) { vuong = true; break; }
            if (vuong) continue;
            foreach (var c in chiem)
                if (Vector2.Distance(new Vector2(c.x, c.z), p) < c.y + banKinhMinh) { vuong = true; break; }
            if (vuong) continue;
            foreach (var q in ds) if (Vector2.Distance(q, p) < khoangCach) { vuong = true; break; }
            if (vuong) continue;
            ds.Add(p);
            chiem.Add(new Vector3(x, banKinhMinh, z));
        }
        return ds;
    }

    /// <summary>Nhan ban mot vat ngau nhien cua nhom, dat tai p voi do sau chon nhu vat goc, xoay them quanh truc dung.</summary>
    static Transform NhanBan(Transform nhom, Vector2 p, System.Random rnd, Dictionary<Transform, float> truocDich,
                             Terrain terr, bool xoayNgauNhien, string duoi)
    {
        int soGoc = nhom.childCount;
        Transform goc = nhom.GetChild(rnd.Next(soGoc));
        // chi chon trong vat CU (khong nhan ban cai vua nhan ban)
        for (int i = 0; i < 20 && goc.name.Contains("_V"); i++) goc = nhom.GetChild(rnd.Next(soGoc));
        float nhung = goc.position.y - DatTai(terr, goc.position);      // vat goc da dat lai theo dat moi o buoc 6
        var o = Object.Instantiate(goc.gameObject, nhom).transform;
        string ten = goc.name;
        int gach = ten.LastIndexOf('_');
        if (gach > 0 && int.TryParse(ten.Substring(gach + 1), out _)) ten = ten.Substring(0, gach);
        o.name = ten + duoi;
        float dat = DatTai(terr, new Vector3(p.x, 0f, p.y));
        o.position = new Vector3(p.x, dat + nhung, p.y);
        if (xoayNgauNhien) o.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f) * goc.rotation;
        else o.rotation = goc.rotation;
        o.gameObject.isStatic = goc.gameObject.isStatic;
        Undo.RegisterCreatedObjectUndo(o.gameObject, "Mo rong Act2");
        truocDich[o] = dat;
        return o;
    }

    /// <summary>Cung cong thuc nhieu co chet / dat troc cua Act2Terrain.ToMau (vanh moi lien voi vung cu).</summary>
    static float NhieuCo(float px, float pz)
    {
        float nhieu = Mathf.PerlinNoise(px * 0.055f + 11.3f, pz * 0.055f + 4.7f);
        float nhieuNho = Mathf.PerlinNoise(px * 0.31f, pz * 0.31f);
        return Mathf.Clamp01((nhieu - 0.42f) * 3.4f + (nhieuNho - 0.5f) * 0.5f);
    }

    static void DamNatQuanh(float[,,] a, int M, int L, float goc, float buoc, Vector2 p)
    {
        const float BanKinh = 1.35f;
        int r = Mathf.CeilToInt(BanKinh / buoc);
        int cx = Mathf.RoundToInt((p.x - goc) / buoc), cz = Mathf.RoundToInt((p.y - goc) / buoc);
        for (int dz = -r; dz <= r; dz++)
            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx, z = cz + dz;
                if (x < 0 || z < 0 || x >= M || z >= M) continue;
                float d = Mathf.Sqrt(dx * dx + dz * dz) / r;
                if (d > 1f) continue;
                float k = Mathf.SmoothStep(1f, 0f, d) * Mathf.Lerp(0.7f, 1.1f, Mathf.PerlinNoise(x * 0.4f, z * 0.4f));
                k = Mathf.Clamp01(k) * 0.8f;
                for (int l = 0; l < L; l++) a[z, x, l] *= (1f - k);
                a[z, x, Act2Terrain.LopDatTroc] += k;
            }
    }

    static void ChuanHoa(float[,,] a, int M, int L)
    {
        for (int z = 0; z < M; z++)
            for (int x = 0; x < M; x++)
            {
                float t = 0f; for (int l = 0; l < L; l++) t += a[z, x, l];
                if (t < 1e-4f) { for (int l = 0; l < L; l++) a[z, x, l] = 0f; a[z, x, Act2Terrain.LopDatTroc] = 1f; continue; }
                for (int l = 0; l < L; l++) a[z, x, l] /= t;
            }
    }

    static float LayMau(float[,] h, int N, float gocX, float gocZ, float buoc, float x, float z)
    {
        float fx = Mathf.Clamp((x - gocX) / buoc, 0f, N - 1.001f), fz = Mathf.Clamp((z - gocZ) / buoc, 0f, N - 1.001f);
        int ix = (int)fx, iz = (int)fz; float tx = fx - ix, tz = fz - iz;
        float a = Mathf.Lerp(h[iz, ix], h[iz, ix + 1], tx), b = Mathf.Lerp(h[iz + 1, ix], h[iz + 1, ix + 1], tx);
        return Mathf.Lerp(a, b, tz);
    }

    static float LayMauTo(float[,,] a, int M, int L, float gocX, float gocZ, float buoc, float x, float z, int l)
    {
        float fx = Mathf.Clamp((x - gocX) / buoc, 0f, M - 1.001f), fz = Mathf.Clamp((z - gocZ) / buoc, 0f, M - 1.001f);
        int ix = (int)fx, iz = (int)fz; float tx = fx - ix, tz = fz - iz;
        float u = Mathf.Lerp(a[iz, ix, l], a[iz, ix + 1, l], tx), v = Mathf.Lerp(a[iz + 1, ix, l], a[iz + 1, ix + 1, l], tx);
        return Mathf.Lerp(u, v, tz);
    }

    static float[,] DocFloat(string duong, int N)
    {
        string that = Path.Combine(Directory.GetCurrentDirectory(), duong);
        if (!File.Exists(that)) return null;
        var b = File.ReadAllBytes(that);
        if (b.Length != N * N * 4) return null;
        var kq = new float[N, N]; int i = 0;
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++, i += 4) kq[z, x] = System.BitConverter.ToSingle(b, i);
        return kq;
    }

    static float[,,] DocByte(string duong, int M, int L)
    {
        string that = Path.Combine(Directory.GetCurrentDirectory(), duong);
        if (!File.Exists(that)) return null;
        var b = File.ReadAllBytes(that);
        if (b.Length != M * M * L) return null;
        var kq = new float[M, M, L]; int i = 0;
        for (int z = 0; z < M; z++) for (int x = 0; x < M; x++) for (int l = 0; l < L; l++, i++) kq[z, x, l] = b[i] / 255f;
        return kq;
    }

    static void GhiFloat(string duong, float[,] h, int N)
    {
        var b = new byte[N * N * 4]; int i = 0;
        for (int z = 0; z < N; z++) for (int x = 0; x < N; x++, i += 4) System.BitConverter.GetBytes(h[z, x]).CopyTo(b, i);
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), duong), b);
        AssetDatabase.ImportAsset(duong, ImportAssetOptions.ForceSynchronousImport);
    }

    static void GhiByte(string duong, float[,,] a, int M, int L)
    {
        var b = new byte[M * M * L]; int i = 0;
        for (int z = 0; z < M; z++) for (int x = 0; x < M; x++) for (int l = 0; l < L; l++, i++)
                    b[i] = (byte)Mathf.RoundToInt(Mathf.Clamp01(a[z, x, l]) * 255f);
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), duong), b);
        AssetDatabase.ImportAsset(duong, ImportAssetOptions.ForceSynchronousImport);
    }
}
