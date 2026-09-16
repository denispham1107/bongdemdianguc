using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHUP ANH (menu 71b): Gio loc bay ngang man hinh, nhieu khung lien tiep + Loc xoay lon de so mau / tia set.
/// Chi chup, khong kiem. Anh: PlayTestShots/gioloc_can_*.png, gioloc_locxoay_*.png.
/// </summary>
public static class ChupGioLoc
{
    static bool daBatDau;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/71b. Chup anh GIO LOC (so voi Loc xoay)", false, 160)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
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
        var go = new GameObject("TAM_ChupGioLoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var cam = Camera.main;
        // Huong ngang man hinh: loc bay tu trai sang phai truoc mat nhan vat, cach 7 m ve phia xa camera
        Vector3 phai = cam.transform.right; phai.y = 0f; phai.Normalize();
        Vector3 xa = cam.transform.forward; xa.y = 0f; xa.Normalize();
        Vector3 goc = toi.transform.position + xa * 9f - phai * 14f;

        GioLoc.Spawn(goc, phai, 0);
        GioLoc.Spawn(goc + xa * 3f - phai * 1f, phai, 0);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.17f);
            yield return Chup("gioloc_can_" + i);
        }
        yield return new WaitForSeconds(4f);

        Tornado.Spawn(toi.transform.position + xa * 12f, phai, 0);
        yield return new WaitForSeconds(1.2f);
        yield return Chup("gioloc_locxoay_0");
        yield return new WaitForSeconds(0.3f);
        yield return Chup("gioloc_locxoay_1");

        var rac = GameObject.Find("TAM_ChupGioLoc");
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
        Debug.Log("[ChupGioLoc] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
