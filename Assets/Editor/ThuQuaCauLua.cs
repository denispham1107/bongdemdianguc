using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 70): QUA CAU LUA - SAT THUONG 85 VA VET LUA MOI (16/09/2026).
///
/// Nguoi dung: sat thuong ban dau 85; ve lai vet lua phia sau qua cau bang Blender MCP - vet cu "co dang
/// hinh tam giac khong that lam". Nguyen nhan do duoc: he hat Flames cua prefab Skill_QuaCauLua dung anh
/// Tex_flame.png - mot HINH TAM GIAC mo; moi hat la mot tam giac dung, noi nhau thanh vet.
///
///   A. Sat thuong: gia tri trong PREFAB (prefab de len code) va tren qua cau THAT sinh ra khi tung.
///   B. Trung that: qua cau bay ngang vao bia dat tam o do cao qua cau -> bia mat trong khung no
///      = 85 x he so giam theo khoang cach (tinh lai tu cho no that).
///   C. Vet lua: he hat Flames KHONG con dung Tex_flame (tam giac) ma dung flipbook Blender; co vet lua dai
///      (TrailRenderer) dung anh vet lua Blender; no xong vet lua duoc tha ra tan dan. Chup can canh.
///
/// Ket qua: PlayTestShots/quacaulua.txt, anh quacaulua_*.png.
/// </summary>
public static class ThuQuaCauLua
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/70. Chay thu QUA CAU LUA (85 sat thuong, vet lua moi)", false, 158)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu qua cau lua", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Qua cau lua: sat thuong 85, vet lua moi");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_QuaCauLua") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_QuaCauLua");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[QuaCauLua] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            if (!pc.tuDocInput) continue;
            return pc;
        }
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>Chup can canh quanh diem w (phong 3 lan).</summary>
    static IEnumerator ChupCan(Vector3 w, string ten)
    {
        yield return new WaitForEndOfFrame();
        var cam = Camera.main;
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        var sp = cam.WorldToScreenPoint(w);
        int co = Mathf.RoundToInt(Screen.height * 0.30f);
        int x0 = Mathf.Clamp((int)sp.x - co / 2, 0, tex.width - co), y0 = Mathf.Clamp((int)sp.y - co / 2, 0, tex.height - co);
        var cat = new Texture2D(co * 3, co * 3, TextureFormat.RGB24, false);
        for (int y = 0; y < co * 3; y++)
            for (int x = 0; x < co * 3; x++)
                cat.SetPixel(x, y, tex.GetPixelBilinear((x0 + x / 3f) / tex.width, (y0 + y / 3f) / tex.height));
        File.WriteAllBytes("PlayTestShots/" + ten + ".png", cat.EncodeToPNG());
        Object.Destroy(tex); Object.Destroy(cat);
    }

    static Vector3 HuongTrong(PlayerController pc)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 0.6f, h, out RaycastHit _, 14f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);
        int maskEnemy = LayerMask.GetMask("Enemy");

        // ================= A. SAT THUONG =================
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Skill_QuaCauLua.prefab");
        var fbPf = pf != null ? pf.GetComponent<Fireball>() : null;
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;
        CapDo.BatDauTranMoi(); CapDo.MoKhoa(0);
        toi.mana = toi.maxMana;
        toi.CastAt(0, goc + huong * 10f);
        Fireball that = null;
        float hanA = Time.time + 1.5f;
        while (that == null && Time.time < hanA) { that = Object.FindAnyObjectByType<Fireball>(); yield return null; }
        Ghi(string.Format("A. sat thuong trong prefab {0}; qua cau that khi tung (cap 1) {1}; so qua {2}",
            fbPf != null ? fbPf.impactDamage : -1f, that != null ? that.impactDamage : -1f,
            Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude).Length));
        Kiem(fbPf != null && Mathf.Approximately(fbPf.impactDamage, 85f), "sat thuong trong prefab khong phai 85");
        Kiem(that != null && Mathf.Approximately(that.impactDamage, 85f), "qua cau that khong mang sat thuong 85");

        // ================= C. VET LUA (dang bay) =================
        Ghi("");
        if (that != null)
        {
            var flames = that.transform.Find("Flames");
            var r = flames != null ? flames.GetComponent<ParticleSystemRenderer>() : null;
            var tex = r != null && r.sharedMaterial != null ? r.sharedMaterial.mainTexture : null;
            var tsa = flames != null ? flames.GetComponent<ParticleSystem>().textureSheetAnimation : default(ParticleSystem.TextureSheetAnimationModule);
            var tr = that.GetComponentInChildren<TrailRenderer>();
            var trTex = tr != null && tr.sharedMaterial != null ? tr.sharedMaterial.mainTexture : null;
            Ghi(string.Format("C. he hat Flames: anh \"{0}\" {1}, flipbook {2} ({3}x{4}); vet lua dai TrailRenderer: {5}, anh \"{6}\"",
                tex != null ? tex.name : "KHONG", tex != null ? tex.width + "x" + tex.height : "",
                flames != null && tsa.enabled, flames != null ? tsa.numTilesX : 0, flames != null ? tsa.numTilesY : 0,
                tr != null, trTex != null ? trTex.name : "KHONG"));
            Kiem(tex != null && tex.name != "Tex_flame", "vet lua van dung anh tam giac Tex_flame");
            Kiem(flames != null && tsa.enabled && tsa.numTilesX * tsa.numTilesY >= 16, "vet lua khong phai flipbook");
            Kiem(tr != null && trTex != null, "khong co vet lua dai");
            // Doi qua cau bay xa ~5 m moi chup: lan chay doi chung chup ngay luc sinh chi ra mot dom sang trum nguoi
            float hanBay = Time.time + 1f;
            while (that != null && Vector3.Distance(that.transform.position, goc) < 5f && Time.time < hanBay) yield return null;
            if (that == null) { Ghi("[LOI] qua cau no truoc khi chup"); loi++; }
            else
            {
                Vector3 w = that.transform.position - that.transform.forward * 1.2f;
                yield return ChupCan(w, "quacaulua_1_bay_can");
                yield return Chup("quacaulua_1_bay");
            }
        }
        // Doi ca chum no xong roi dem NGAY - vet tha ra chi song them ~1,3 giay
        float hanNo = Time.time + 4f;
        while (Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude).Length > 0 && Time.time < hanNo) yield return null;
        yield return null;
        int vetConLai = 0;
        foreach (var t in Object.FindObjectsByType<TrailRenderer>(FindObjectsInactive.Exclude))
            if (t.transform.parent == null && t.name == "VetLua") vetConLai++;

        // ================= B. TRUNG THAT =================
        foreach (var f in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude)) Object.Destroy(f.gameObject);
        yield return new WaitForSeconds(0.5f);
        Vector3 cao = goc + Vector3.up * 60f;
        var bia = new GameObject("TAM_BiaLua");
        bia.layer = LayerMask.NameToLayer("Enemy");
        bia.transform.position = cao + huong * 8f;
        var cap = bia.AddComponent<CapsuleCollider>(); cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.zero;
        var d = bia.AddComponent<Damageable>(); d.maxHealth = 1e7f; d.health = 1e7f;
        yield return new WaitForFixedUpdate();
        float m0 = d.health;
        var qua = Fireball.Spawn(cao, huong, 0, maskEnemy);
        qua.burnSeconds = 0f;                                  // chi do don no, khong tinh chay
        Vector3 viTriTruoc = qua.transform.position;
        float mat = 0f; Vector3 choNo = Vector3.zero;
        float hanB = Time.time + 2f;
        while (Time.time < hanB)
        {
            if (qua != null) viTriTruoc = qua.transform.position;
            if (d.health < m0 - 0.01f) { mat = m0 - d.health; choNo = viTriTruoc; break; }
            yield return null;
        }
        float dist = Vector3.Distance(choNo, bia.transform.position);
        float mong = Mathf.Round(85f * Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(dist / 3.4f)));
        Ghi(string.Format("B. qua cau (sinh tu prefab) bay vao bia 8 m: bia mat {0:F2}; no cach tam bia ~{1:F2} m -> mong 85 x giam theo khoang cach ~ {2} (sai so vi tri no 1 khung)",
            mat, dist, mong));
        Kiem(mat > 85f * 0.80f && mat <= 85.01f, "don no qua cau lua khong con cua sat thuong 85");
        Object.Destroy(bia);

        Ghi(string.Format("C2. no xong: {0} vet lua duoc tha ra tan dan tai cho", vetConLai));
        Kiem(vetConLai >= 1, "no xong vet lua bien mat cung qua cau");

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[QuaCauLua] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/quacaulua.txt", bao.ToString());
        var r = GameObject.Find("TAM_QuaCauLua");
        if (r != null) Object.DestroyImmediate(r);
        var b = GameObject.Find("TAM_BiaLua");
        if (b != null) Object.DestroyImmediate(b);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
