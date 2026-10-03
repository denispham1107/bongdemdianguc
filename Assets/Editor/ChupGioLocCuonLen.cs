using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 97b - CHUP ANH DONG Gio loc cuon len o 3 MUC TOC DO (nguoi dung 04/10/2026 chon "chup 3 muc de toi chon"). Vao Play Act2, ban
/// NGAY (de thay ro chuyen dong), tat GameDirector; moi muc (VfxFactory.MucCuonGioLoc = 1 / 1,5 / 2) tung MOT Gio loc that bay ngang
/// truoc mat nhan vat, chup lien tiep 2 goc: may quay GAME (dung nhu khi choi) va may quay CAN di theo loc (cung huong nhin, gan hon).
/// Khung PNG o PlayTestShots/gioloc_cuon/m{muc}_{game|can}_{i}.png; ghep GIF bang Python (CongCu khong can).
/// </summary>
public static class ChupGioLocCuonLen
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/gioloc_cuon/";
    const int Rong = 480, Cao = 360, SoKhung = 30;
    const float CachKhung = 0.06f;

    [MenuItem("Diablo 2.5D/97b Gio loc - chup anh dong cuon len 3 muc", false, 161)]
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
        var go = new GameObject("TAM_ChupGioLocCuon");
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
        foreach (var muc in new[] { 1f, 1.5f, 2f })
        {
            VfxFactory.MucCuonGioLoc = muc;
            Vector3 goc = toi.transform.position + xa * 6f - phai * 8f;
            var gl = GioLoc.Spawn(goc, phai, 0);
            if (gl == null) { loi++; continue; }
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < SoKhung && gl != null; i++)
            {
                goCan.transform.SetPositionAndRotation(gl.transform.position + Vector3.up * 2.4f - cam.transform.forward * 7.5f, cam.transform.rotation);   // nhin vao giua than, cach 7,5 m
                string m = muc.ToString("0.0").Replace(',', '.');
                Ghi(cam, "m" + m + "_game_" + i.ToString("00"));
                Ghi(can, "m" + m + "_can_" + i.ToString("00"));
                yield return new WaitForSeconds(CachKhung);
            }
            if (gl != null) Object.Destroy(gl.gameObject);
            yield return new WaitForSeconds(2.5f);
        }
        VfxFactory.MucCuonGioLoc = mucCu;
        File.WriteAllText(Thu + "xong.txt", "loi " + loi);
        Object.Destroy(goCan);
        var rac = GameObject.Find("TAM_ChupGioLocCuon");
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
        Debug.Log("[ChupGioLocCuonLen] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
