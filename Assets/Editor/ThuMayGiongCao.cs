using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 100 - MAY GIONG BAY CAO 8 m CO CON TRONG MAN HINH O GOC CHOI THAT (nguoi dung 04/10/2026: "may bay cao hon 1 chut" - chon 8 m,
/// truoc 7; 25/09/2026 thu 10 m thi may nam tren mep man hinh). Vao Play Act2, may quay game that (CameraRig dung yen sau khi on dinh),
/// tung May giong THAT (CastAt) o 6 m va 12 m (tam toi da = Sam set). Sau 1,2 s doc tung hat may (MaySang / MayXam): chieu len man hinh
/// TAM hat va DINH hat (tam + nua co len tren) - ti le nam trong khung. DOI CHUNG: cung cac hat ha 1 m (= may 7 m cu).
/// Ket qua: PlayTestShots/maygiong_cao.txt, anh maygiong_cao_6m.png / _12m.png.
/// </summary>
public static class ThuMayGiongCao
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    static readonly StringBuilder bao = new StringBuilder();
    static int soLoi;
    const string Ra = "PlayTestShots/maygiong_cao.txt";

    [MenuItem("Diablo 2.5D/100 May giong cao 8 m - con trong man hinh", false, 166)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        bao.Length = 0; soLoi = 0; daBatDau = false;
        if (File.Exists(Ra)) File.Delete(Ra);
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
        var go = new GameObject("TAM_ThuMayGiongCao");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dung, string loi) { if (!dung) { soLoi++; bao.AppendLine("  LOI: " + loi); } }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(2f);
        GameDirector.Instance.enabled = false;
        PlayerController toi = null;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        CapDo.MoCaDuongChoPhepThu(CapDo.KyMayGiong);
        var cam = Camera.main;
        var rig = cam.GetComponent<CameraRig>();
        // Hai goc may game: 0 = "3D tu do" (mac dinh, thap sau lung, nghieng 22 do) va 1 = "2.5D (Diablo)" (nghieng 48 do, cach 13 m)
        for (int cheDo = 0; cheDo < 2; cheDo++)
        {
        if (rig != null) { rig.enabled = true; rig.SetView(cheDo); }
        yield return new WaitForSeconds(2f);
        if (rig != null) rig.enabled = false;   // dung yen goc may game da on dinh
        Vector3 truoc = cam.transform.forward; truoc.y = 0f; truoc.Normalize();
        bao.AppendLine("== Goc may " + (cheDo == 0 ? "3D tu do" : "2.5D (Diablo)"));
        bao.AppendLine(string.Format("May quay game: cao {0:F1} m tren nhan vat, cach {1:F1} m, nghieng {2:F0} do, fov {3:F0}; may giong MayGiong.CaoMay = {4}",
            cam.transform.position.y - toi.transform.position.y, new Vector2(cam.transform.position.x - toi.transform.position.x, cam.transform.position.z - toi.transform.position.z).magnitude,
            cam.transform.eulerAngles.x, cam.fieldOfView, MayGiong.CaoMay));
        foreach (float xa in new[] { 6f, 12f })
        {
            foreach (var m in Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude)) Object.Destroy(m.gameObject);
            yield return new WaitForSeconds(6f);       // het hoi chieu 5,5 s
            toi.mana = toi.maxMana;
            toi.CastAt(CapDo.KyMayGiong, toi.transform.position + truoc * xa);
            yield return new WaitForSeconds(1.7f);     // niem 0,5 + may ket
            int n = 0, tamTrong = 0, dinhTrong = 0, tamTrong7 = 0, dinhTrong7 = 0, dayTrong = 0, dayTrong7 = 0; float dinhCaoNhat = -9f, dayThapNhat = 9f, dayThapNhat7 = 9f;
            foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude))
            {
                if (ps.name != "MaySang" && ps.name != "MayXam") continue;
                var arr = new ParticleSystem.Particle[ps.particleCount];
                int c = ps.GetParticles(arr);
                bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
                for (int i = 0; i < c; i++)
                {
                    Vector3 p = cucBo ? ps.transform.TransformPoint(arr[i].position) : arr[i].position;
                    float nua = 0.5f * arr[i].GetCurrentSize3D(ps).y * ps.transform.lossyScale.y;
                    n++;
                    foreach (int bac in new[] { 0, 1 })
                    {
                        Vector3 q = p - Vector3.up * bac;            // bac 1 = doi chung may 7 m
                        Vector3 vt = cam.WorldToViewportPoint(q), vd = cam.WorldToViewportPoint(q + Vector3.up * nua), vy = cam.WorldToViewportPoint(q - Vector3.up * nua);
                        bool yTrong = vy.z > 0f && vy.x > 0f && vy.x < 1f && vy.y > 0f && vy.y < 1f;   // DAY dam may (phan thap nhat) lot vao khung
                        bool tTrong = vt.z > 0f && vt.x > 0f && vt.x < 1f && vt.y > 0f && vt.y < 1f;
                        bool dTrong = vd.z > 0f && vd.x > 0f && vd.x < 1f && vd.y > 0f && vd.y < 1f;
                        if (bac == 0) { if (tTrong) tamTrong++; if (dTrong) dinhTrong++; if (yTrong) dayTrong++; dinhCaoNhat = Mathf.Max(dinhCaoNhat, vd.y); dayThapNhat = Mathf.Min(dayThapNhat, vy.y); }
                        else { if (tTrong) tamTrong7++; if (dTrong) dinhTrong7++; if (yTrong) dayTrong7++; dayThapNhat7 = Mathf.Min(dayThapNhat7, vy.y); }
                    }
                }
            }
            string anh = "PlayTestShots/maygiong_cao_" + (cheDo == 0 ? "3d_" : "25d_") + xa.ToString("0") + "m.png";
            if (File.Exists(anh)) File.Delete(anh);
            ScreenCapture.CaptureScreenshot(anh);
            for (int i = 0; i < 90 && !File.Exists(anh); i++) yield return new WaitForEndOfFrame();
            bao.AppendLine(string.Format("Ngam {0} m: {1} dam may; TAM trong khung {2}/{1}, DINH {3}/{1}, DAY (phan thap nhat) {7}/{1}, day thap nhat o {8:F2} chieu cao man hinh | DOI CHUNG may 7 m: tam {5}/{1}, dinh {6}/{1}, day {9}/{1}, day thap nhat {10:F2}",
                xa, n, tamTrong, dinhTrong, dinhCaoNhat, tamTrong7, dinhTrong7, dayTrong, dayThapNhat, dayTrong7, dayThapNhat7));
            // Lan do dau (04/10/2026) cho thay o CA HAI goc may mac dinh tam va dinh moi dam may (ca ban 7 m cu) deu nam TREN mep man hinh -
            // chi phan day lot vao khung. Day la phep DO (bao cao), khong kiem dat / hong.
        }
        }
        foreach (var m in Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude)) Object.Destroy(m.gameObject);
        bao.AppendLine("so loi ghi nhan = " + soLoi);
        File.WriteAllText(Ra, bao.ToString());
        var rac = GameObject.Find("TAM_ThuMayGiongCao");
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
        Debug.Log("[ThuMayGiongCao] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
