using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 153 - VU NO LUA KHONG CON VET HAT (01/11/2026, nguoi dung khoanh cac vet / hat cam mong ban ra sau khi Qua cau lua, Thien thach,
/// Lua dia nguc va cham va no: "xoa di". Chon: ap ca qua cau lua cua Phu thuy; xoa luon "tia trung don" mau lua tren ke trung).
///
/// Vao Play Act2, tat GameDirector, Bo xuong THAT lam bia (AI tat, mau 100 000, khong do don), tung ky nang THAT (CastAt):
///   A/B/C. Qua cau lua (0) / Thien thach (4) / Lua dia nguc (11): bia mat mau; tia trung don (HitBurst) TANG 0 suot luc no + vung lua
///      + cay chay; moi vu no ("Vfx_NoLua" prefab hoac "FireExplosion" dung bang code) KHONG con con "Sparks", va KHONG he hat Stretch nao
///      dang co hat duoi goc vu no.
///   D. Qua cau lua cua quai Phu thuy (Fireball.Spawn nhu EnemyAI.NemPhep): vu no cung khong Sparks.
///      (Lan 1: luot Lua dia nguc ra +8 - cay / nha mo Thien thach luot truoc dot CHAY RUI dung luc ay, moi vat 2 chum HitBurst tan lua -> da bo.)
///   E. DOI CHUNG: prefab Vfx_NoLua dung thang (khong qua FireExplosion) van co "Sparks" phun hat Stretch; TakeDamage Lua thuong +1 tia.
/// PlayTestShots/no_lua_khong_vet/nhatky.txt.
/// </summary>
public static class ThuNoLuaKhongVet
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/no_lua_khong_vet/";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/153 Vu no lua khong con vet hat (Qua cau lua, Thien thach, Lua dia nguc)", false, 227)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuNoLuaKhongVet] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        nk.Length = 0; loi = 0;
        daBatDau = false;
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
        daBatDau = true;
        var go = new GameObject("TAM_ThuNoLuaKhongVet");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static PlayerController toi;
    static readonly List<Damageable> bia = new List<Damageable>();

    static void SinhBia(Vector3 tam, Vector3 F, Vector3 R)
    {
        foreach (var d in bia) if (d != null) Object.Destroy(d.gameObject);
        bia.Clear();
        Vector3[] lech = { Vector3.zero, R * 1.4f, -R * 1.4f, F * 1.3f };
        foreach (var l in lech)
        {
            Vector3 p = tam + l;
            p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p));
            var q = EnemyFactory.Spawn(MonsterType.Skeleton, p, null, toi.transform);
            q.name = "TAM_Bia";
            var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
            var d = q.GetComponent<Damageable>(); d.maxHealth = 100000f; d.health = 100000f; d.tiLeDoDon = 0f;
            bia.Add(d);
        }
    }

    static float TongMat()
    {
        float t = 0f;
        foreach (var d in bia) if (d != null) t += d.maxHealth - d.health;
        return t;
    }

    static bool LaGocNo(Transform t) { return t.name == "Vfx_NoLua" || t.name == "FireExplosion"; }

    /// <summary>Quet moi vu no lua dang co: dem goc, so con "Sparks", so hat dang song o he hat Stretch duoi goc.</summary>
    static void QuetNo(HashSet<GameObject> daThay, ref int sparks, ref int hatKeoDai)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (!LaGocNo(t)) continue;
            daThay.Add(t.gameObject);
            if (t.Find("Sparks") != null && t.Find("Sparks").gameObject.activeInHierarchy) sparks++;
            foreach (var ps in t.GetComponentsInChildren<ParticleSystem>())
                if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Stretch) hatKeoDai += ps.particleCount;
        }
    }

    static IEnumerator ThuKyNang(string ten, int ky, Vector3 tam, Vector3 F, Vector3 R, float giayTheoDoi)
    {
        nk.AppendLine("== " + ten);
        SinhBia(tam, F, R);
        yield return new WaitForSeconds(0.4f);
        int h0 = VfxFactory.SoLanTiaTrungDon, r0 = CayChay.SoLanChayRui; float m0 = TongMat();
        var thay = new HashSet<GameObject>(); int sp = 0, kd = 0;
        toi.mana = toi.maxMana;
        toi.CastAt(ky, tam);
        for (float t = 0f; t < giayTheoDoi; t += Time.deltaTime)
        {
            yield return null;
            QuetNo(thay, ref sp, ref kd);
        }
        int hit = VfxFactory.SoLanTiaTrungDon - h0; float mat = TongMat() - m0;
        Kiem(mat > 0f, ten + ": bia mat " + mat.ToString("F0") + " mau");
        Kiem(thay.Count > 0, ten + ": " + thay.Count + " vu no lua");
        Kiem(sp == 0 && kd == 0, ten + ": con 'Sparks' trong vu no " + sp + " lan, hat keo dai " + kd + " (phai 0 / 0)");
        Kiem(hit == 0, ten + ": tia trung don phun them " + hit + " lan trong " + giayTheoDoi + " s (phai 0; co " + (CayChay.SoLanChayRui - r0) + " vat chay rui trong luot)");
        foreach (var d in bia) if (d != null) Object.Destroy(d.gameObject);
        bia.Clear();
        yield return new WaitForSeconds(1.2f);
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        TamNhin.BatTrongPhepThu = true;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var rig = Camera.main.GetComponent<CameraRig>();
        if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
        var hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.epCamUng = true;
        foreach (int ky in new[] { 0, 4, CapDo.KyLuaDiaNguc }) CapDo.MoCaDuongChoPhepThu(ky);
        yield return new WaitForSeconds(1.0f);
        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        Vector3 P = toi.transform.position;
        toi.health.maxHealth = 100000f; toi.health.health = 100000f;
        Vector3 tam = P + F * 7f;

        yield return ThuKyNang("A Qua cau lua", 0, tam, F, R, 2.0f);
        // Thien thach: theo doi ca vung lua chay (con nhip ri dot bia) - 4 giay
        yield return ThuKyNang("B Thien thach", 4, tam, F, R, 4.0f);
        yield return ThuKyNang("C Lua dia nguc", CapDo.KyLuaDiaNguc, tam, F, R, 3.0f);

        // ---- D. Qua cau lua cua Phu thuy ----
        nk.AppendLine("== D Qua cau lua cua quai Phu thuy");
        SinhBia(P + F * 5f, F, R);
        yield return new WaitForSeconds(0.3f);
        {
            Vector3 tu = P + F * 1.5f + Vector3.up * 1.4f;
            Vector3 den = bia[0].transform.position + Vector3.up;
            var f = Fireball.Spawn(tu, (den - tu).normalized, ~0, LayerMask.GetMask("Enemy"));
            f.impactDamage = 16f; f.blastRadius = 2.6f;
            var thay = new HashSet<GameObject>(); int sp = 0, kd = 0;
            int h0 = VfxFactory.SoLanTiaTrungDon;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime) { yield return null; QuetNo(thay, ref sp, ref kd); }
            Kiem(thay.Count > 0 && sp == 0 && kd == 0, "D Phu thuy: " + thay.Count + " vu no, Sparks " + sp + ", hat keo dai " + kd);
            Kiem(VfxFactory.SoLanTiaTrungDon - h0 == 0, "D Phu thuy: tia trung don +" + (VfxFactory.SoLanTiaTrungDon - h0));
        }
        foreach (var d in bia) if (d != null) Object.Destroy(d.gameObject);
        bia.Clear();

        // ---- E. Doi chung ----
        nk.AppendLine("== E Doi chung");
        var pf = GameAssets.I != null ? GameAssets.I.fireExplosionPrefab : null;
        if (pf != null)
        {
            var cu = GameAssets.Make(pf, P + F * 6f);
            int maxHat = 0;
            for (int i = 0; i < 20; i++)
            {
                yield return null;
                var s = cu != null ? cu.transform.Find("Sparks") : null;
                if (s != null) maxHat = Mathf.Max(maxHat, s.GetComponent<ParticleSystem>().particleCount);
            }
            Kiem(maxHat > 20, "E prefab Vfx_NoLua dung thang van phun " + maxHat + " hat Sparks keo dai (bo dem song)");
            if (cu != null) Object.Destroy(cu);
        }
        else Kiem(false, "E khong co prefab vu no lua");
        int hc = VfxFactory.SoLanTiaTrungDon;
        toi.health.TakeDamage(5f, DamageType.Fire, toi.transform.position + Vector3.up);
        Kiem(VfxFactory.SoLanTiaTrungDon - hc == 1, "E TakeDamage Lua thuong van phun tia trung don (+" + (VfxFactory.SoLanTiaTrungDon - hc) + ")");
        Kiem(!Damageable.BoQuaTiaTrungDon, "E co BoQuaTiaTrungDon tra ve false");

        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 153 - Vu no lua khong con vet hat: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuNoLuaKhongVet] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuNoLuaKhongVet");
        if (rac != null) Object.Destroy(rac);
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
        Debug.Log("[ThuNoLuaKhongVet] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
