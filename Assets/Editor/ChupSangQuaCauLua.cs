using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 108 - CHUP CAC MUC DIU SANG QUA CAU LUA + VU NO (nguoi dung 05/10/2026: "anh sang qua cau lua va khi no qua sang, choi mat"; chon chup
/// 3 muc de chon). Play Act2 ban dem, may quay game (goc mac dinh "3D tu do" va "2.5D"), tung Quả cầu lửa THAT (CastAt 0) vao bia 10 m truoc mat
/// voi VfxFactory.HeSoSangQuaCauLua = HeSoSangNoLua = 1 / 0,7 / 0,55 / 0,4; chup luc DANG BAY (0,25 s sau khi qua sinh) va luc NO (0,06 s sau vu no).
/// Ban 2 (ban 1 tung that thi moi vu no mot cho mot luc - khong so duoc): qua cau DUNG YEN + vu no o CUNG mot cho, may quay dung yen, do MOI KHUNG
/// (do sang trung binh tru nen, % diem anh chui) - khi bay muc Muc, khi no muc MucNo; cuoi cung KIEM qua tung that voi muc da chon (den 6 x 0,55,
/// den no 22 x 0,25). Anh: PlayTestShots/sang_caulua_bay_&lt;muc&gt;.png, sang_caulua_no_&lt;muc&gt;.png (moc 0,3 s / 0,1 s).
/// </summary>
public static class ChupSangQuaCauLua
{
    static bool daBatDau;
    static readonly StringBuilder bao = new StringBuilder();
    const string Ra = "PlayTestShots/sang_caulua.txt";
    public static readonly float[] Muc = { 1f, 0.7f, 0.55f, 0.4f };
    /// <summary>Muc VU NO rieng: 0,4 van trang chui (vo lua + hat bung cong sang chong nhau bao hoa) - chup them muc manh tay hon.</summary>
    public static readonly float[] MucNo = { 1f, 0.4f, 0.25f, 0.15f };

    [MenuItem("Diablo 2.5D/108 Chup cac muc diu sang Qua cau lua", false, 177)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (File.Exists(Ra)) File.Delete(Ra);
        bao.Length = 0; daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_SangCauLua");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/sang_caulua_" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

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

    const int W = 640, H = 360;

    /// <summary>Ve may quay game vao anh (gom ca loc bloom cua may quay) - KHONG lop OnGUI.</summary>
    static Texture2D VeKhung(Camera cam, RenderTexture rt)
    {
        var cu = cam.targetTexture;
        cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var a = RenderTexture.active; RenderTexture.active = rt;
        var tx = new Texture2D(W, H, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, W, H), 0, 0); tx.Apply();
        RenderTexture.active = a;
        return tx;
    }

    /// <summary>Do sang trung binh (0..1) va % diem anh CHOI (ca ba kenh > 0,94) cua ca khung.</summary>
    static void DoKhung(Texture2D tx, out float sang, out float choi)
    {
        var px = tx.GetPixels32();
        double tong = 0; int c = 0;
        for (int i = 0; i < px.Length; i += 2)
        {
            var p = px[i];
            tong += (0.2126 * p.r + 0.7152 * p.g + 0.0722 * p.b) / 255.0;
            if (p.r > 240 && p.g > 240 && p.b > 240) c++;
        }
        int n = (px.Length + 1) / 2;
        sang = (float)(tong / n); choi = 100f * c / n;
    }

    /// <summary>Trung binh moi khung trong <paramref name="giay"/> s; luu khung o moc <paramref name="mocLuu"/> s.</summary>
    static IEnumerator DoTrongKhoang(Camera cam, RenderTexture rt, float giay, float mocLuu, string tenLuu, float[] ra)
    {
        float t0 = Time.time; double tongSang = 0, tongChoi = 0; int n = 0; bool daLuu = false;
        while (Time.time - t0 < giay)
        {
            yield return new WaitForEndOfFrame();
            var tx = VeKhung(cam, rt);
            float s, c; DoKhung(tx, out s, out c);
            tongSang += s; tongChoi += c; n++;
            if (!daLuu && Time.time - t0 >= mocLuu) { daLuu = true; File.WriteAllBytes("PlayTestShots/sang_caulua_" + tenLuu + ".png", tx.EncodeToPNG()); }
            Object.Destroy(tx);
        }
        ra[0] = (float)(tongSang / Mathf.Max(1, n)); ra[1] = (float)(tongChoi / Mathf.Max(1, n)); ra[2] = n;
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;
        if (toi == null || rig == null) { bao.AppendLine("THIEU nhan vat / may quay"); Ket(); yield break; }
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (chuyen != null) { chuyen.enabled = false; chuyen.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); }
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        rig.enabled = true;
        rig.SetView(0);
        yield return new WaitForSeconds(1.5f);
        rig.enabled = false;   // may quay dung yen: moi muc cung mot khung canh
        var rt = new RenderTexture(W, H, 24);
        Vector3 p = toi.transform.position + huong * 7f;
        p.y = VfxFactory.GroundY(p) + 1.0f;
        var nen = new float[3];
        yield return DoTrongKhoang(cam, rt, 0.5f, 99f, "nen", nen);
        bao.AppendLine(string.Format("nen (khong hieu ung): sang {0:F4}, choi {1:F3}%", nen[0], nen[1]));
        var noGoc = new float[2]; var bayGoc = new float[2];
        for (int m = 0; m < Muc.Length; m++)
        {
            VfxFactory.HeSoSangQuaCauLua = Muc[m];
            VfxFactory.HeSoSangNoLua = MucNo[m];
            string ten = Mathf.RoundToInt(Muc[m] * 100f).ToString();
            // BAY: qua cau dung yen (toc 0) giua khung - 0,6 s
            var f = Fireball.Spawn(p, Vector3.Cross(Vector3.up, huong), 0, 0);
            f.speed = 0f; f.lifetime = 30f;
            yield return new WaitForSeconds(0.3f);
            var bay = new float[3];
            yield return DoTrongKhoang(cam, rt, 0.6f, 0.3f, "bay_" + ten, bay);
            Object.Destroy(f.gameObject);
            yield return new WaitForSeconds(0.8f);
            // NO: vu no tai cung cho - 0,7 s
            VfxFactory.FireExplosion(p, 3.4f);
            var no = new float[3];
            yield return DoTrongKhoang(cam, rt, 0.7f, 0.1f, "no_" + Mathf.RoundToInt(MucNo[m] * 100f), no);
            float themBay = bay[0] - nen[0], themNo = no[0] - nen[0];
            if (m == 0) { bayGoc[0] = themBay; noGoc[0] = themNo; bayGoc[1] = bay[1]; noGoc[1] = no[1]; }
            bao.AppendLine(string.Format("BAY muc {0}% / NO muc {9}%: BAY sang them {1:F4} (x{2:F2} so voi hien tai), choi {3:F2}% | NO sang them {4:F4} (x{5:F2}), choi {6:F2}% ({7} + {8} khung)",
                ten, themBay, themBay / Mathf.Max(1e-5f, bayGoc[0]), bay[1], themNo, themNo / Mathf.Max(1e-5f, noGoc[0]), no[1], bay[2], no[2], Mathf.RoundToInt(MucNo[m] * 100f)));
            yield return new WaitForSeconds(2.5f);
        }
        rt.Release(); Object.Destroy(rt);

        // KIEM tren qua TUNG THAT voi muc da chon (mac dinh): den qua cau khi bay va den vu no
        VfxFactory.HeSoSangQuaCauLua = VfxFactory.HeSoSangQuaCauLuaChon; VfxFactory.HeSoSangNoLua = VfxFactory.HeSoSangNoLuaChon;
        rig.enabled = true;
        var mau = toi.GetComponent<Damageable>(); mau.maxHealth = 1e7f; mau.health = 1e7f;
        CapDo.BatDauTranMoi(); CapDo.MoCaDuongChoPhepThu(0);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        toi.mana = toi.maxMana;
        var truoc = new HashSet<Fireball>(Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude));
        var denNoTruoc = new HashSet<LightBurst>(Object.FindObjectsByType<LightBurst>(FindObjectsInactive.Exclude));
        toi.CastAt(0, p);
        float denBay = -1f, denNo = -1f; float hh = Time.time + 3f;
        while ((denBay < 0f || denNo < 0f) && Time.time < hh)
        {
            foreach (var q in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude))
                if (!truoc.Contains(q) && q.boQua == mau && denBay < 0f) { var lf = q.GetComponentInChildren<LightFlicker>(); if (lf != null) denBay = lf.baseIntensity; }
            foreach (var lb in Object.FindObjectsByType<LightBurst>(FindObjectsInactive.Exclude))
                if (!denNoTruoc.Contains(lb) && denNo < 0f) denNo = lb.peak;
            yield return null;
        }
        bao.AppendLine(string.Format("KIEM tung that (muc chon): den qua cau khi bay {0:F2} (prefab 6 x {1} = {2:F2}), den vu no dinh {3:F2} (prefab 22 x {4} = {5:F2})",
            denBay, VfxFactory.HeSoSangQuaCauLuaChon, 6f * VfxFactory.HeSoSangQuaCauLuaChon, denNo, VfxFactory.HeSoSangNoLuaChon, 22f * VfxFactory.HeSoSangNoLuaChon));
        int loiKiem = 0;
        if (Mathf.Abs(denBay - 6f * VfxFactory.HeSoSangQuaCauLuaChon) > 0.01f) loiKiem++;
        if (Mathf.Abs(denNo - 22f * VfxFactory.HeSoSangNoLuaChon) > 0.01f) loiKiem++;
        bao.AppendLine("so loi ghi nhan = " + loiKiem);
        Ket();
    }

    static void Ket()
    {
        VfxFactory.HeSoSangQuaCauLua = VfxFactory.HeSoSangQuaCauLuaChon; VfxFactory.HeSoSangNoLua = VfxFactory.HeSoSangNoLuaChon;
        File.WriteAllText(Ra, bao.ToString());
        foreach (var n in new[] { "TAM_SangCauLua", "TAM_BiaSang" }) { var g = GameObject.Find(n); if (g != null) Object.Destroy(g); }
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
    }
}
