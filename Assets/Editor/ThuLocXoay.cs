using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU HOI SINH SAU LOC XOAY: canh vat bi con loc cuon mat phai moc lai
/// sau <see cref="VatTheBiCuon.GiayHoiSinh"/> giay, khong som hon va khong muon hon.
///
/// Dong ho la <c>Time.time</c> chu khong phai thoi gian that - luc vua vao Play
/// khung hinh nang toi muc <c>Time.deltaTime</c> ket o tran 0,333 s, dong ho
/// that chay nhanh gap may lan dong ho trong game.
/// </summary>
public static class ThuLocXoay
{
    static float t0 = -1f, tTanBien = -1f;
    static int buoc, loi;
    static string thuMuc;
    static readonly StringBuilder bao = new StringBuilder();
    static readonly List<GameObject> vat = new List<GameObject>();

    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;
    static Tornado loc;

    [MenuItem("Diablo 2.5D/19. Chay thu HOI SINH sau Loc xoay - Act2", false, 103)]
    public static void Chay()
    {
        thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "PlayTestShots");
        Directory.CreateDirectory(thuMuc);

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        bao.Length = 0;
        vat.Clear();
        loi = 0; buoc = 0; t0 = -1f; tTanBien = -1f;
        loc = null;

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[ThuLocXoay] " + s);
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (t0 < 0f) t0 = Time.time;
        float t = Time.time - t0;

        switch (buoc)
        {
            // ---- Tha con loc vao giua khu bia mo ----
            case 0:
                if (t > 1.0f)
                {
                    var nguoi = Object.FindAnyObjectByType<PlayerController>();
                    Vector3 cho = nguoi != null ? nguoi.transform.position : Vector3.zero;
                    loc = Tornado.Spawn(cho + new Vector3(4f, 0f, 4f), Vector3.forward,
                                        LayerMask.GetMask("Enemy"));
                    Ghi(string.Format("t={0:F2}s  tha loc tai {1}, song {2:F1}s",
                                      t, loc.transform.position.ToString("F1"), loc.duration));
                    buoc++;
                }
                break;

            // ---- Cho loc gom du vat ----
            case 1:
                if (t > 4.0f)
                {
                    foreach (var v in Object.FindObjectsByType<VatTheBiCuon>(FindObjectsInactive.Include,
                                                                            FindObjectsSortMode.None))
                        if (!vat.Contains(v.gameObject)) vat.Add(v.gameObject);

                    Ghi(string.Format("t={0:F2}s  loc dang cuon {1} vat the", t, vat.Count));
                    if (vat.Count == 0) { Ghi("[LOI] loc khong cuon duoc vat nao"); loi++; buoc = 5; }
                    else buoc++;
                }
                break;

            // ---- Cho toi luc chung tan bien (loc tan) ----
            case 2:
                if (DemHien() == 0 && vat.Count > 0)
                {
                    tTanBien = Time.time;
                    Ghi(string.Format("t={0:F2}s  ca {1} vat da TAN BIEN cung con loc", t, vat.Count));
                    buoc++;
                }
                else if (t > 20f) { Ghi("[LOI] qua 20s ma vat the chua tan bien"); loi++; buoc = 5; }
                break;

            // ---- Nua chang duong: chua duoc moc lai ----
            case 3:
                if (Time.time - tTanBien > VatTheBiCuon.GiayHoiSinh * 0.5f)
                {
                    int hien = DemHien();
                    Ghi(string.Format("sau {0:F1}s: dang hien = {1}/{2} (phai la 0)",
                                      Time.time - tTanBien, hien, vat.Count));
                    if (hien != 0) { Ghi("[LOI] moc lai som hon han"); loi++; }
                    buoc++;
                }
                break;

            // ---- Het 30 giay: phai moc lai het ----
            case 4:
                if (Time.time - tTanBien > VatTheBiCuon.GiayHoiSinh + 1.5f)
                {
                    int hien = DemHien(), conCom = 0;
                    for (int i = 0; i < vat.Count; i++)
                        if (vat[i] != null && vat[i].GetComponent<VatTheBiCuon>() != null) conCom++;

                    Ghi(string.Format("sau {0:F1}s (nguong {1}s): dang hien = {2}/{3}, con component = {4}",
                                      Time.time - tTanBien, VatTheBiCuon.GiayHoiSinh,
                                      hien, vat.Count, conCom));
                    if (hien != vat.Count) { Ghi("[LOI] khong phai vat nao cung moc lai"); loi++; }
                    if (conCom != 0) { Ghi("[LOI] component VatTheBiCuon khong tu go ra"); loi++; }
                    buoc++;
                }
                break;

            case 5:
                Ket();
                break;
        }
    }

    /// <summary>Bao nhieu vat trong danh sach dang HIEN HINH.</summary>
    static int DemHien()
    {
        int n = 0;
        for (int i = 0; i < vat.Count; i++)
        {
            if (vat[i] == null) continue;
            var r = vat[i].GetComponentInChildren<Renderer>(true);
            if (r != null && r.enabled) n++;
        }
        return n;
    }

    static void Ket()
    {
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText(Path.Combine(thuMuc, "loc_xoay.txt"), bao.ToString());

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }
}
