using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU 10 LO LUA TRONG ACT2 (menu 54 dat).
///
/// Kiem bang cach KHAC voi luc dat - dung lai ham luat cua menu 54 thi no se
/// luon bao "dung":
///   - Nuoc: gan va cham TAM vao luoi nuoc, chieu tia tu tren xuong quanh chan
///     lo - tia cham mat nuoc truoc dia hinh la lo dung duoi nuoc. Do ca khoang
///     cach toi mep nuoc gan nhat.
///   - Nha mo: tia tu mieng lo chieu len - cham nha la lo nam duoi mai; va chan
///     lo khong nam trong hop bao cua nha.
///   - Bia, da: hop bao HINH cua lo khong cat hop bao hinh bia/da nao.
///   - Mat dat: chan lo so voi do cao dia hinh o 16 diem quanh chan - khong lo
///     lung, khong chon qua sau.
/// Trong Play: lua + den bat that o du 10 lo, nhan vat di thang vao lo thi bi
/// chan lai, chup anh bang camera game va mot ban do nhin tu tren xuong.
///
/// Bao cao <c>PlayTestShots/lolua_act2.txt</c>, anh <c>PlayTestShots/lolua_*.png</c>.
/// </summary>
public static class ThuLoLuaAct2
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/54b. Chay thu lo lua Act2", false, 144)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu lo lua", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 3] 10 lo lua trong Act2");

        canhCu = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene(DatLoLuaAct2.CanhAct2, OpenSceneMode.Single);
        try { KiemTinh(); }
        catch (System.Exception e) { Loi("kiem tinh nem loi: " + e); }

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_LoLuaAct2");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[LoLuaAct2] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }

    static List<Transform> TimLo()
    {
        var ds = new List<Transform>();
        var nhom = GameObject.Find(DatLoLuaAct2.TenNhom);
        if (nhom != null) foreach (Transform t in nhom.transform) ds.Add(t);
        return ds;
    }

    // =============================================================
    // A. NGOAI PLAY: VI TRI
    // =============================================================

    static void KiemTinh()
    {
        Ghi("");
        Ghi("A. vi tri (ngoai Play)");
        var lo = TimLo();
        Ghi("so lo trong nhom " + DatLoLuaAct2.TenNhom + ": " + lo.Count);
        if (lo.Count != DatLoLuaAct2.SoLo) Loi("can " + DatLoLuaAct2.SoLo + " lo");

        var ter = Object.FindAnyObjectByType<Terrain>();
        Vector3 tamMap = ter.transform.position + ter.terrainData.size * 0.5f;

        // Va cham tam cho luoi nuoc - an, khong luu
        var tam = new List<GameObject>();
        var nuocCol = new HashSet<Collider>();
        var matNuoc = GameObject.Find("MatNuoc");
        foreach (var mf in matNuoc.GetComponentsInChildren<MeshFilter>())
        {
            var g = new GameObject("TAM_VaChamNuoc");
            g.hideFlags = HideFlags.HideAndDontSave;
            g.transform.SetPositionAndRotation(mf.transform.position, mf.transform.rotation);
            g.transform.localScale = mf.transform.lossyScale;
            var mc = g.AddComponent<MeshCollider>(); mc.sharedMesh = mf.sharedMesh;
            nuocCol.Add(mc); tam.Add(g);
        }
        Physics.SyncTransforms();

        var nhaCol = new List<Collider>(GameObject.Find("NhaMo").GetComponentsInChildren<Collider>());
        var nhaHop = new List<Bounds>();
        foreach (Transform m in GameObject.Find("NhaMo").transform)
        {
            var b = new Bounds(m.position, Vector3.zero);
            foreach (var r in m.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            nhaHop.Add(b);
        }
        var biaHop = new List<(string, Bounds)>();
        foreach (var ten in new[] { "BiaMo", "Da" })
            foreach (Transform c in GameObject.Find(ten).transform)
            {
                var rs = c.GetComponentsInChildren<Renderer>();
                if (rs.Length == 0) continue;
                var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                biaHop.Add((ten + "/" + c.name, b));
            }

        bool DuoiNuoc(Vector3 d)
        {
            var hits = Physics.RaycastAll(new Vector3(d.x, 60f, d.z), Vector3.down, 120f, ~0, QueryTriggerInteraction.Collide);
            float gan = float.MaxValue; Collider c = null;
            foreach (var h in hits)
            {
                if (h.collider.GetComponentInParent<LoLuaDa>() != null) continue;   // chinh cai lo
                if (h.distance < gan) { gan = h.distance; c = h.collider; }
            }
            return c != null && nuocCol.Contains(c);
        }

        float kcNho = float.MaxValue;
        for (int i = 0; i < lo.Count; i++)
        {
            var t = lo[i];
            var p = t.position;
            var sb = new StringBuilder(t.name + " " + p.ToString("F1") + ":");

            // 1. Nuoc + khoang cach toi mep nuoc
            int uot = 0;
            for (int k = 0; k < 16; k++)
            {
                float g = k * Mathf.PI / 8f;
                if (DuoiNuoc(p + new Vector3(Mathf.Cos(g), 0f, Mathf.Sin(g)) * 0.55f)) uot++;
            }
            if (DuoiNuoc(p)) uot++;
            float mep = 99f;
            for (float r = 0.25f; r <= 12f && mep > 50f; r += 0.25f)
                for (int k = 0; k < 32; k++)
                {
                    float g = k * Mathf.PI / 16f;
                    if (DuoiNuoc(p + new Vector3(Mathf.Cos(g), 0f, Mathf.Sin(g)) * r)) { mep = r; break; }
                }
            sb.Append(" mep nuoc " + (mep > 50f ? "> 12" : mep.ToString("F2")) + " m;");
            if (uot > 0) Loi(t.name + ": " + uot + "/17 diem quanh chan lo nam duoi mat nuoc");

            // 2. Nha mo
            var b = new Bounds(p, Vector3.zero);
            foreach (var r in t.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            foreach (var h in Physics.RaycastAll(p + Vector3.up * 1.4f, Vector3.up, 40f, ~0, QueryTriggerInteraction.Ignore))
                if (nhaCol.Contains(h.collider)) Loi(t.name + ": nam duoi mai nha " + h.collider.name);
            float nhaGan = float.MaxValue;
            foreach (var nb in nhaHop)
            {
                if (nb.Intersects(b)) Loi(t.name + ": cham hop bao nha mo");
                float dx = Mathf.Max(0f, Mathf.Max(nb.min.x - p.x, p.x - nb.max.x));
                float dz = Mathf.Max(0f, Mathf.Max(nb.min.z - p.z, p.z - nb.max.z));
                nhaGan = Mathf.Min(nhaGan, Mathf.Sqrt(dx * dx + dz * dz));
            }
            sb.Append(" nha mo " + nhaGan.ToString("F1") + " m;");

            // 3. Bia, da
            float biaGan = float.MaxValue, caoGan = float.MaxValue; string tenBia = "";
            foreach (var (ten, bb) in biaHop)
            {
                if (bb.Intersects(b)) Loi(t.name + ": de len " + ten);
                float dx = Mathf.Max(0f, Mathf.Max(bb.min.x - b.max.x, b.min.x - bb.max.x));
                float dz = Mathf.Max(0f, Mathf.Max(bb.min.z - b.max.z, b.min.z - bb.max.z));
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d < biaGan) { biaGan = d; tenBia = ten; }
                // Bia cao dung sat lo: nhin tu camera trong nhu lo dat tren bia
                if (bb.size.y > 1.5f && d < caoGan) { caoGan = d; }
                if (bb.size.y > 1.5f && d < 2.0f) Loi(t.name + ": bia cao " + ten + " (" + bb.size.y.ToString("F1") + " m) chi cach " + d.ToString("F2") + " m");
            }
            sb.Append(" bia/da gan nhat " + biaGan.ToString("F2") + " m (" + tenBia + "), bia cao gan nhat " + caoGan.ToString("F1") + " m;");

            // 4. Mat dat
            float loLung = 0f, chon = 0f;
            for (int k = 0; k < 16; k++)
            {
                float g = k * Mathf.PI / 8f;
                var d = p + new Vector3(Mathf.Cos(g), 0f, Mathf.Sin(g)) * 0.5f;
                float h = ter.SampleHeight(d) + ter.transform.position.y;
                loLung = Mathf.Max(loLung, p.y - h);
                chon = Mathf.Max(chon, h - p.y);
            }
            sb.Append(" chan lo: ho " + loLung.ToString("F2") + " m, chon " + chon.ToString("F2") + " m;");
            if (loLung > 0.05f) Loi(t.name + ": chan lo lo lung " + loLung.ToString("F2") + " m");
            if (chon > 0.30f) Loi(t.name + ": chan lo chon sau " + chon.ToString("F2") + " m");

            // 5. Dau truong, va cham
            float tuTam = new Vector2(p.x - tamMap.x, p.z - tamMap.z).magnitude;
            sb.Append(" cach tam " + tuTam.ToString("F1") + " m");
            if (tuTam > 50f) Loi(t.name + ": ngoai dau truong");
            if (t.GetComponent<CapsuleCollider>() == null) Loi(t.name + ": thieu va cham");
            if (t.GetComponent<LoLuaDa>() == null) Loi(t.name + ": thieu LoLuaDa (khong co lua)");
            for (int j = 0; j < i; j++)
                kcNho = Mathf.Min(kcNho, Vector2.Distance(new Vector2(p.x, p.z), new Vector2(lo[j].position.x, lo[j].position.z)));
            Ghi(sb.ToString());
        }
        Ghi("hai lo gan nhau nhat: " + kcNho.ToString("F1") + " m");
        if (lo.Count > 0 && lo[0].name != "LoLua_GiuaMap") Loi("lo dau tien khong phai lo giua");

        foreach (var g in tam) Object.DestroyImmediate(g);
        Physics.SyncTransforms();
    }

    // =============================================================
    // B. TRONG PLAY: LUA, DEN, VA CHAM, ANH
    // =============================================================

    static IEnumerator KichBan()
    {
        Ghi("");
        Ghi("B. trong Play");
        float han = Time.time + 25f;
        PlayerController toi = null;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        yield return new WaitForSeconds(2f);
        if (!EditorApplication.isPlaying) { Ghi("[bo] Editor da thoat Play"); Ket(); yield break; }

        var lo = TimLo();
        int coLua = 0, coDen = 0;
        foreach (var t in lo)
        {
            var lua = t.Find("LuaLoDa");
            if (lua == null) { Loi(t.name + ": khong thay lua (LuaLoDa)"); continue; }
            int dangChay = 0, hat = 0;
            foreach (var ps in lua.GetComponentsInChildren<ParticleSystem>()) { if (ps.isPlaying) dangChay++; hat += ps.particleCount; }
            var den = lua.GetComponentInChildren<Light>();
            if (dangChay >= 4 && hat >= 3) coLua++; else Loi(t.name + ": he hat lua " + dangChay + " dang chay, " + hat + " hat");
            if (den != null && den.enabled && den.intensity > 0f) coDen++; else Loi(t.name + ": den lua tat");
        }
        Ghi("lo co lua chay: " + coLua + "/" + lo.Count + ", co den: " + coDen + "/" + lo.Count);

        // ---- Di thang vao lo ----
        if (toi != null && lo.Count > 0)
        {
            var cc = toi.GetComponent<CharacterController>();
            var pc = toi.GetComponent<PlayerController>();
            Vector3 cu = toi.transform.position;
            pc.enabled = false;
            int thu = 0, chan = 0;
            foreach (var t in lo)
            {
                var cap = t.GetComponent<CapsuleCollider>();
                // Chon huong trong: diem xuat phat cach lo 3 m khong vuong gi
                Vector3 bd = Vector3.zero; bool co = false;
                for (int h = 0; h < 8 && !co; h++)
                {
                    var huong = Quaternion.Euler(0f, h * 45f, 0f) * Vector3.forward;
                    var q = t.position + huong * 3f;
                    q.y = VfxFactory.GroundY(q) + 0.1f;
                    if (Physics.CheckCapsule(q + Vector3.up * (cc.radius + 0.1f), q + Vector3.up * (cc.height - cc.radius),
                                             cc.radius, 1 << 0, QueryTriggerInteraction.Ignore)) continue;
                    bd = q; co = true;
                }
                if (!co) { Ghi("   " + t.name + ": khong co huong trong de thu - bo qua"); continue; }
                cc.enabled = false; toi.transform.position = bd; cc.enabled = true;
                yield return null;
                for (int k = 0; k < 60; k++)
                {
                    var d = t.position - toi.transform.position; d.y = 0f;
                    cc.Move(d.normalized * 0.1f + Vector3.down * 0.05f);
                }
                yield return null;
                var con = toi.transform.position - t.position; con.y = 0f;
                float kc = con.magnitude, toiThieu = cap.radius + cc.radius - 0.1f;
                // Dung lai XA lo la vuong vat khac tren duong - khong chung minh gi
                if (kc > cap.radius + cc.radius + 0.3f)
                { Ghi("   " + t.name + ": dung lai cach tam " + kc.ToString("F2") + " m - vuong vat khac, khong tinh"); continue; }
                thu++;
                if (kc >= toiThieu) chan++;
                else Loi(t.name + ": nhan vat di xuyen vao lo (con cach tam " + kc.ToString("F2") + " m, can >= " + toiThieu.ToString("F2") + ")");
            }
            Ghi("nhan vat di thang vao lo: bi chan " + chan + "/" + thu + " lan");
            if (thu < lo.Count - 2) Loi("chi thu duoc " + thu + "/" + lo.Count + " lo - phep thu khong du");
            cc.enabled = false; toi.transform.position = cu; cc.enabled = true;
            pc.enabled = true;
        }

        // ---- Anh ----
        var cam = Camera.main;
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        if (cam != null && lo.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(3, lo.Count); i++)
            {
                var t = lo[i == 0 ? 0 : (i == 1 ? lo.Count / 3 : 2 * lo.Count / 3)];
                cam.transform.position = t.position + new Vector3(0f, 6.5f, -7.5f);
                cam.transform.LookAt(t.position + Vector3.up * 1.2f);
                yield return null;
                Chup(cam, "PlayTestShots/lolua_" + (i + 1) + "_" + t.name + ".png", 960, 540);
            }
            ChupBanDo(cam, lo, "PlayTestShots/lolua_bando.png");
        }

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Chup(Camera cam, string duong, int w, int h)
    {
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        var cuRt = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = cuRt;
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(duong, tex.EncodeToPNG());
        Object.DestroyImmediate(tex); rt.Release(); Object.DestroyImmediate(rt);
        Ghi("anh: " + duong);
    }

    /// <summary>Nhin thang tu tren xuong ca ban do, khoanh vong do quanh tung lo.</summary>
    static void ChupBanDo(Camera cam, List<Transform> lo, string duong)
    {
        const int N = 1024;
        var go = new GameObject("TAM_CamBanDo");
        var c = go.AddComponent<Camera>();
        c.CopyFrom(cam);
        c.orthographic = true; c.orthographicSize = 55f;
        c.nearClipPlane = 1f; c.farClipPlane = 200f;
        go.transform.SetPositionAndRotation(new Vector3(0f, 80f, 0f), Quaternion.Euler(90f, 0f, 0f));
        var rt = new RenderTexture(N, N, 24, RenderTextureFormat.ARGB32);
        c.targetTexture = rt;
        bool suong = RenderSettings.fog; RenderSettings.fog = false;
        c.Render();
        RenderSettings.fog = suong;
        RenderTexture.active = rt;
        var tex = new Texture2D(N, N, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, N, N), 0, 0);
        RenderTexture.active = null;
        foreach (var t in lo)
        {
            var s = c.WorldToScreenPoint(t.position);
            bool giua = t.name == "LoLua_GiuaMap";
            for (int a = 0; a < 360; a++)
                for (float r = 11f; r <= 14f; r += 0.5f)
                {
                    int x = Mathf.RoundToInt(s.x + Mathf.Cos(a * Mathf.Deg2Rad) * r);
                    int y = Mathf.RoundToInt(s.y + Mathf.Sin(a * Mathf.Deg2Rad) * r);
                    if (x >= 0 && y >= 0 && x < N && y < N) tex.SetPixel(x, y, giua ? Color.yellow : Color.red);
                }
        }
        tex.Apply();
        File.WriteAllBytes(duong, tex.EncodeToPNG());
        c.targetTexture = null;
        Object.DestroyImmediate(tex); rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(go);
        Ghi("anh: " + duong + " (vong vang = lo giua, vong do = 9 lo con lai)");
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[LoLuaAct2] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/lolua_act2.txt", bao.ToString());
        var rac = GameObject.Find("TAM_LoLuaAct2");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
