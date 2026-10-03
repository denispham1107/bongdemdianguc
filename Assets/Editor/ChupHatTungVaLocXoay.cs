using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 99b - CHUP ANH DONG: 3 quai that bi Gio loc hat tung (cao 3 m, NGA NGUA) + Loc xoay tung that hien ngay tai quai (04/10/2026). Khung cua menu 97b. (Mo ta goc 97b:) Gio loc cuon len o 3 MUC TOC DO (nguoi dung 04/10/2026 chon "chup 3 muc de toi chon"). Vao Play Act2, ban
/// NGAY (de thay ro chuyen dong), tat GameDirector; moi muc (VfxFactory.MucCuonGioLoc = 1 / 1,5 / 2) tung MOT Gio loc that bay ngang
/// truoc mat nhan vat, chup lien tiep 2 goc: may quay GAME (dung nhu khi choi) va may quay CAN di theo loc (cung huong nhin, gan hon).
/// Khung PNG o PlayTestShots/gioloc_cuon/m{muc}_{game|can}_{i}.png; ghep GIF bang Python (CongCu khong can).
/// </summary>
public static class ChupHatTungVaLocXoay
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/hattung_locxoay/";
    const int Rong = 480, Cao = 360, SoKhung = 30;
    const float CachKhung = 0.06f;

    [MenuItem("Diablo 2.5D/99b Chup anh dong: Gio loc hat tung nga ngua + Loc xoay hien tai doi thu", false, 165)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
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
        var go = new GameObject("TAM_ChupHatTung");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(Camera c, string ten)
    {
        var rt = RenderTexture.GetTemporary(Rong, Cao, 24);
        var cu = c.targetTexture; c.targetTexture = rt; c.Render(); c.targetTexture = cu;
        var ra = RenderTexture.active; RenderTexture.active = rt;
        var tx = new Texture2D(Rong, Cao, TextureFormat.RGB24, false); tx.ReadPixels(new Rect(0, 0, Rong, Cao), 0, 0); tx.Apply();
        RenderTexture.active = ra; RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(Thu + ten + ".png", tx.EncodeToPNG());
        Object.Destroy(tx);
    }

    static IEnumerator KichBan()
    {
        float mucCu = VfxFactory.MucCuonGioLoc;
        int loi = 0;
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        var ngay = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (ngay != null) { ngay.enabled = false; ngay.ApGiay(ChuyenChieuSangDem.GiayGiuaNgay); }
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var cam = Camera.main;
        var rig = cam.GetComponent<CameraRig>(); if (rig != null) rig.enabled = false;   // dung yen may quay game giua cac khung
        Vector3 phai = cam.transform.right; phai.y = 0f; phai.Normalize();
        Vector3 xa = cam.transform.forward; xa.y = 0f; xa.Normalize();
        var goCan = new GameObject("TAM_CamCan");
        var can = goCan.AddComponent<Camera>(); can.CopyFrom(cam); can.enabled = false;
        int lopDich = LayerMask.NameToLayer("Enemy");
        CapDo.MoCaDuongChoPhepThu(3);
        // 3 quai that dung thanh hang ngang truoc mat, cach 9 m
        Vector3 tamQ = toi.transform.position + xa * 9f;
        var quai = new System.Collections.Generic.List<GameObject>();
        var loai = new[] { MonsterType.Witch, MonsterType.Skeleton, MonsterType.Witch };
        for (int i = 0; i < 3; i++)
        {
            Vector3 p = tamQ + phai * (i - 1) * 2.2f; p.y = VfxFactory.GroundY(p);
            var q = EnemyFactory.Spawn(loai[i], p, null, toi.transform);
            q.name = "TAM_Quai" + i;
            var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
            var d = q.GetComponent<Damageable>(); d.maxHealth = 100000f; d.health = 100000f; d.tiLeDoDon = 0f;
            q.transform.rotation = Quaternion.LookRotation(-xa);
            quai.Add(q);
        }
        yield return new WaitForSeconds(0.5f);
        goCan.transform.SetPositionAndRotation(tamQ + Vector3.up * 2.6f - cam.transform.forward * 9f, cam.transform.rotation);
        // 1) Gio loc bay ngang qua hang quai (2 lan)
        for (int lan = 0; lan < 2; lan++)
        {
            var gl = GioLoc.Spawn(tamQ - phai * 9f, phai, 1 << lopDich);
            for (int i = 0; i < SoKhung; i++)
            {
                Ghi(can, "gioloc" + lan + "_can_" + i.ToString("00"));
                yield return new WaitForSeconds(CachKhung);
            }
            if (gl != null) Object.Destroy(gl.gameObject);
            yield return new WaitForSeconds(1.5f);
        }
        // 2) Loc xoay: nguoi choi tung THAT, ngam lech 1,5 m canh quai giua -> loc hien ngay tai quai
        goCan.transform.SetPositionAndRotation(tamQ + Vector3.up * 7f - cam.transform.forward * 22f, cam.transform.rotation);
        toi.mana = toi.maxMana;
        toi.CastAt(3, quai[1].transform.position + phai * 1.5f);
        for (int i = 0; i < SoKhung; i++)
        {
            Ghi(can, "locxoay_can_" + i.ToString("00"));
            Ghi(cam, "locxoay_game_" + i.ToString("00"));
            yield return new WaitForSeconds(CachKhung);
        }
        foreach (var t in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) Object.Destroy(t.gameObject);
        yield return new WaitForSeconds(0.5f);
        foreach (var q in quai) if (q != null) Object.Destroy(q);
        VfxFactory.MucCuonGioLoc = mucCu;
        File.WriteAllText(Thu + "xong.txt", "loi " + loi);
        Object.Destroy(goCan);
        var rac = GameObject.Find("TAM_ChupHatTung");
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
        Debug.Log("[ChupHatTungVaLocXoay] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
