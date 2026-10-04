using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 105 - "KHOI DEN" CHE HAI DAU TIA GIUT SET (nguoi dung 05/10/2026, anh khoanh hai dau tia: tia bi che boi mot khoi toi hinh
/// tam giac o canh tay va o canh cho trung). Nguyen nhan (do bang ban dau cua menu nay - tat tung lop): tia 10 m chi ~11 khuc
/// gap, ca ba lop vuot nhon ve 0 trong khuc dau / cuoi (~0,9 m) -> tam giac canh thang, loi trang bi ep mat.
///
/// Play Act2 ban dem, may quay 2.5D (goc nguoi dung chup), bia 9 m truoc mat; tung Giut set THAT, moi kieu (MOI / DOI CHUNG cach vuot
/// cu - LightningArc.DoiChungDauCu) 4 lan. Moi lan: dua rieng cac renderer cua tia sang lop 31, ve bang may quay phu CUNG VI TRI may
/// quay game len nen DEN (khong lan dat, khong bloom), roi cat ngang tia tai cac khoang cach tu dau tay (0,30-0,90 m) va tu cho trung
/// (0,65-0,95 m - cum bung che phan gan hon): lay diem anh SANG NHAT moi lat cat. Tia bi che / mat loi -> lat cat toi.
/// Kiem: kieu moi sang nhat moi lat cat >= 0,5 (thang 0..1) va dau tay sang hon doi chung. Anh toan canh hai kieu:
/// PlayTestShots/giatset_khoiden_moi.png / _cu.png; anh nen den: _moi_rieng.png / _cu_rieng.png.
/// </summary>
public static class ChupKhoiDenGiatSet
{
    static bool daBatDau;
    static int cheDo;   // 0 = menu 105 (khoi den), 1 = menu 105b (chup cac muc nhanh nho)
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    const string Ra = "PlayTestShots/giatset_khoiden.txt";
    const int W = 960, H = 540;

    [MenuItem("Diablo 2.5D/105 Khoi den o hai dau tia Giut set", false, 173)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (File.Exists(Ra)) File.Delete(Ra);
        bao.Length = 0; loi = 0; daBatDau = false; cheDo = 0;
        GiatSet.SoNhanhNhoMoiMet = GiatSet.SoNhanhNhoMoiMetChon;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_KhoiDenGiatSet");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = cheDo == 1 ? KichBanNhanh() : KichBan();
    }

    /// <summary>MENU 105b - chup tia Giut set o cac muc nhanh nho phu (GiatSet.SoNhanhNhoMoiMet 0 / 0,5 / 1 / 1,5) de nguoi dung chon:
    /// anh toan canh may quay 2.5D + anh can canh (may quay phu nhin ngang than tia). Dem so nhanh nho that tren moi tia.</summary>
    [MenuItem("Diablo 2.5D/105b Chup cac muc nhanh nho Giut set", false, 174)]
    public static void ChayNhanh()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (File.Exists(RaNhanh)) File.Delete(RaNhanh);
        bao.Length = 0; loi = 0; daBatDau = false; cheDo = 1;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }
    const string RaNhanh = "PlayTestShots/giatset_nhanhnho.txt";
    static readonly float[] MucNhanh = { 0f, 0.5f, 1f, 1.5f };

    static IEnumerator KichBanNhanh()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;
        if (toi == null || rig == null) { bao.AppendLine("THIEU nhan vat / may quay"); loi++; KetNhanh(); yield break; }
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (chuyen != null) { chuyen.enabled = false; chuyen.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); }
        var mau = toi.GetComponent<Damageable>(); mau.maxHealth = 1e7f; mau.health = 1e7f;
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(6);
        rig.enabled = true;
        rig.SetView(1);
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        var bia = new GameObject("TAM_BiaKhoiDen");
        bia.transform.position = toi.transform.position + huong * 9f;
        bia.layer = LayerMask.NameToLayer("Enemy");
        var cap = bia.AddComponent<CapsuleCollider>(); cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var dm = bia.AddComponent<Damageable>(); dm.maxHealth = 1e7f; dm.health = 1e7f;
        yield return new WaitForSeconds(1.2f);
        for (int m = 0; m < MucNhanh.Length; m++)
        {
            GiatSet.SoNhanhNhoMoiMet = MucNhanh[m];
            yield return new WaitForSeconds(0.9f);
            toi.transform.rotation = Quaternion.LookRotation(huong);
            toi.mana = toi.maxMana;
            toi.CastAt(6, bia.transform.position);
            float h = Time.time + 1.5f;
            while (TiaGiatSet() == null && Time.time < h) yield return null;
            yield return new WaitForSeconds(0.15f);
            var tia = TiaGiatSet();
            bao.AppendLine(string.Format("muc {0} nhanh nho / met: tia dai {1:F1} m, nhanhNho = {2}", MucNhanh[m],
                tia != null ? Vector3.Distance(tia.start, tia.end) : -1f, tia != null ? tia.nhanhNho : -1));
            if (tia == null) { loi++; continue; }
            yield return Chup("nhanh_" + MucNhanh[m].ToString("F1").Replace(",", "_").Replace(".", "_"));
        }
        GiatSet.SoNhanhNhoMoiMet = GiatSet.SoNhanhNhoMoiMetChon;
        KetNhanh();
    }

    static void KetNhanh()
    {
        bao.AppendLine("so loi ghi nhan = " + loi);
        File.WriteAllText(RaNhanh, bao.ToString());
        foreach (var n in new[] { "TAM_KhoiDenGiatSet", "TAM_BiaKhoiDen" }) { var g = GameObject.Find(n); if (g != null) Object.Destroy(g); }
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
    }

    static void Kiem(bool d, string l) { if (!d) { loi++; bao.AppendLine("  LOI: " + l); } }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/giatset_khoiden_" + ten + ".png";
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

    static LightningArc TiaGiatSet()
    {
        foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
            if (a.transform.Find("Aura") != null) return a;
        return null;
    }

    /// <summary>Ve rieng tia len nen den bang may quay phu dat dung cho may quay game, tra anh.</summary>
    static Texture2D VeRieng(Camera game, LightningArc tia, Camera phu, RenderTexture rt)
    {
        var rs = tia.GetComponentsInChildren<Renderer>();
        var lopCu = new int[rs.Length];
        for (int i = 0; i < rs.Length; i++) { lopCu[i] = rs[i].gameObject.layer; rs[i].gameObject.layer = 31; }
        phu.transform.SetPositionAndRotation(game.transform.position, game.transform.rotation);
        phu.fieldOfView = game.fieldOfView; phu.nearClipPlane = game.nearClipPlane; phu.farClipPlane = game.farClipPlane;
        phu.aspect = W / (float)H;
        phu.targetTexture = rt;
        phu.Render();
        for (int i = 0; i < rs.Length; i++) rs[i].gameObject.layer = lopCu[i];
        var cu = RenderTexture.active; RenderTexture.active = rt;
        var tx = new Texture2D(W, H, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, W, H), 0, 0); tx.Apply();
        RenderTexture.active = cu;
        return tx;
    }

    /// <summary>Lat cat vuong goc voi tia tai p, rong +-ban diem anh: tra do TRANG nhat (max cua min(r,g,b) - loi trang; quang xanh
    /// bao hoa kenh lam nen max kenh luc nao cung 1) va <paramref name="rong"/> = so diem anh co quang (kenh lam > 0,25).</summary>
    static float SangNhatLatCat(Texture2D tx, Camera phu, Vector3 p, Vector3 huong, int ban, out int rong)
    {
        rong = 0;
        Vector3 a = phu.WorldToScreenPoint(p), b = phu.WorldToScreenPoint(p + huong * 0.3f);
        Vector2 d = new Vector2(b.x - a.x, b.y - a.y);
        if (d.sqrMagnitude < 1e-4f || a.z <= 0f) return -1f;
        d.Normalize();
        Vector2 n = new Vector2(-d.y, d.x);
        float best = 0f;
        for (int k = -ban; k <= ban; k++)
        {
            int x = Mathf.RoundToInt(a.x + n.x * k), y = Mathf.RoundToInt(a.y + n.y * k);
            if (x < 0 || y < 0 || x >= W || y >= H) continue;
            Color c = tx.GetPixel(x, y);
            best = Mathf.Max(best, Mathf.Min(c.r, Mathf.Min(c.g, c.b)));
            if (c.b > 0.25f) rong++;
        }
        return best;
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;
        if (toi == null || rig == null) { bao.AppendLine("THIEU nhan vat / may quay"); loi++; Ket(); yield break; }
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (chuyen != null) { chuyen.enabled = false; chuyen.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); }
        var mau = toi.GetComponent<Damageable>(); mau.maxHealth = 1e7f; mau.health = 1e7f;
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(6);
        rig.enabled = true;
        rig.SetView(1);
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        var bia = new GameObject("TAM_BiaKhoiDen");
        bia.transform.position = toi.transform.position + huong * 9f;
        bia.layer = LayerMask.NameToLayer("Enemy");
        var cap = bia.AddComponent<CapsuleCollider>(); cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var dm = bia.AddComponent<Damageable>(); dm.maxHealth = 1e7f; dm.health = 1e7f;

        var phuGo = new GameObject("TAM_MayQuayPhu");
        var phu = phuGo.AddComponent<Camera>();
        phu.enabled = false; phu.clearFlags = CameraClearFlags.SolidColor; phu.backgroundColor = Color.black;
        phu.cullingMask = 1 << 31;
        var rt = new RenderTexture(W, H, 24);
        bao.AppendLine(string.Format("may quay '{0}', muc do hoa {1}, bia cach 9 m", rig.CurrentName, CaiDatDoHoa.Muc));
        yield return new WaitForSeconds(1.2f);

        float[] dDau = { 0.35f, 0.50f, 0.65f, 0.80f, 0.95f };
        float[] dCuoi = { 0.65f, 0.80f, 0.95f };
        float minDauMoi = 9f, minDauCu = 9f, minCuoiMoi = 9f, minCuoiCu = 9f, tbDauMoi = 0f, tbDauCu = 0f;
        float rongDauMoi = 9f, rongDauCu = 9f;   // be ngang quang o 0,35 m / be ngang o giua tia (thap nhat qua cac lan)
        const int SoLan = 4;
        for (int kieu = 0; kieu < 2; kieu++)
        {
            bool cu = kieu == 1;
            LightningArc.DoiChungDauCu = cu;
            var sbDau = new StringBuilder(); var sbCuoi = new StringBuilder();
            for (int lan = 0; lan < SoLan; lan++)
            {
                yield return new WaitForSeconds(0.9f);
                toi.transform.rotation = Quaternion.LookRotation(huong);
                toi.mana = toi.maxMana;
                toi.CastAt(6, bia.transform.position);
                float h = Time.time + 1.5f;
                while (TiaGiatSet() == null && Time.time < h) yield return null;
                yield return new WaitForSeconds(0.15f);
                var tia = TiaGiatSet();
                if (tia == null) { Kiem(false, "khong thay tia Giut set"); continue; }
                // Nhanh nho phu (nguoi dung chon 1 / met): so dat dung theo do dai
                int monDoi = Mathf.RoundToInt(GiatSet.SoNhanhNhoMoiMetChon * Vector3.Distance(tia.start, tia.end));
                if (lan == 0) bao.AppendLine(string.Format("{0}: tia dai {1:F1} m, nhanhNho {2} (mong doi {3})", cu ? "DOI CHUNG cu" : "MOI",
                    Vector3.Distance(tia.start, tia.end), tia.nhanhNho, monDoi));
                Kiem(tia.nhanhNho == monDoi && tia.nhanhNho >= 6, "so nhanh nho khong dung 1 / met");
                var tx = VeRieng(cam, tia, phu, rt);
                Vector3 s = tia.start, e = tia.end, dir = (e - s).normalized;
                int rongGiua, rongDau;
                SangNhatLatCat(tx, phu, Vector3.Lerp(s, e, 0.35f), dir, 60, out rongGiua);
                SangNhatLatCat(tx, phu, s + dir * 0.35f, dir, 60, out rongDau);
                float tiLe = rongDau / (float)Mathf.Max(1, rongGiua);
                sbDau.Append("[rong ").Append(tiLe.ToString("F2")).Append("] ");
                if (cu) rongDauCu = Mathf.Min(rongDauCu, tiLe); else rongDauMoi = Mathf.Min(rongDauMoi, tiLe);
                int bo;
                foreach (float d in dDau)
                {
                    float v = SangNhatLatCat(tx, phu, s + dir * d, dir, 30, out bo);
                    sbDau.Append(v.ToString("F2")).Append(' ');
                    if (cu) { minDauCu = Mathf.Min(minDauCu, v); tbDauCu += v / (dDau.Length * SoLan); }
                    else { minDauMoi = Mathf.Min(minDauMoi, v); tbDauMoi += v / (dDau.Length * SoLan); }
                }
                foreach (float d in dCuoi)
                {
                    float v = SangNhatLatCat(tx, phu, e - dir * d, dir, 30, out bo);
                    sbCuoi.Append(v.ToString("F2")).Append(' ');
                    if (cu) minCuoiCu = Mathf.Min(minCuoiCu, v); else minCuoiMoi = Mathf.Min(minCuoiMoi, v);
                }
                sbDau.Append("| "); sbCuoi.Append("| ");
                if (lan == 0)
                {
                    File.WriteAllBytes("PlayTestShots/giatset_khoiden_" + (cu ? "cu" : "moi") + "_rieng.png", tx.EncodeToPNG());
                    yield return Chup(cu ? "cu" : "moi");
                }
                Object.Destroy(tx);
            }
            bao.AppendLine(string.Format("{0}: lat cat dau tay (0,35-0,95 m, do trang loi) {1}", cu ? "DOI CHUNG cu" : "MOI", sbDau));
            bao.AppendLine(string.Format("{0}: lat cat cho trung (0,65-0,95 m) {1}", cu ? "DOI CHUNG cu" : "MOI", sbCuoi));
        }
        LightningArc.DoiChungDauCu = false;
        bao.AppendLine(string.Format("dau tay: MOI thap nhat {0:F2} trung binh {1:F2} | CU thap nhat {2:F2} trung binh {3:F2}", minDauMoi, tbDauMoi, minDauCu, tbDauCu));
        bao.AppendLine(string.Format("cho trung: MOI thap nhat {0:F2} | CU thap nhat {1:F2}", minCuoiMoi, minCuoiCu));
        bao.AppendLine(string.Format("be ngang quang o 0,35 m tu tay / o giua tia (thap nhat): MOI {0:F2} | CU {1:F2}", rongDauMoi, rongDauCu));
        Kiem(minDauMoi >= 0.5f, "dau tay van co lat cat mat loi trang (tia bi che)");
        Kiem(minCuoiMoi >= 0.5f, "cho trung van co lat cat mat loi trang");
        Kiem(rongDauMoi >= 0.35f, "quang o dau tay van thu nhon (tam giac)");
        Kiem(minDauCu < minDauMoi - 0.1f || rongDauCu < rongDauMoi - 0.1f, "doi chung cach vuot cu khong khac - phep do khong phan biet duoc");
        Object.Destroy(phuGo); rt.Release(); Object.Destroy(rt);
        Ket();
    }

    static void Ket()
    {
        LightningArc.DoiChungDauCu = false;
        bao.AppendLine("so loi ghi nhan = " + loi);
        File.WriteAllText(Ra, bao.ToString());
        foreach (var n in new[] { "TAM_KhoiDenGiatSet", "TAM_BiaKhoiDen", "TAM_MayQuayPhu" }) { var g = GameObject.Find(n); if (g != null) Object.Destroy(g); }
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
    }
}
