using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 106 - CHO SET CHAM DAT CUA SAM SET + MAY GIONG (nguoi dung 05/10/2026 chon mau A + vet nut cua mau B). Play Act2 ban dem, may
/// quay 2.5D, tung SAM SET roi MAY GIONG THAT (CastAt) vao cho trong 8 m truoc mat. Kiem:
///  A. moi cho cham (prefab Vfx_SetChamDat) KHONG con lop Sparks / Jet dang bat; DOI CHUNG: LoeSetChamDat goi thang (duong cua Loc xoay /
///     Gio loc) van con ca hai lop.
///  B. tia set con bo tren dat: so tia / cho cham = SoTiaDot1 + SoTiaDot2; hai dau moi tia o NangKhoiDat tren mat dat (+-0,03 m), dau xa
///     cach cho cham &lt;= 1,05 ban kinh + 1 m.
///  C. vet nut: <= ToiDaCungLuc vet; mau quang theo tuoi (trang -> cam -> do -> tat) do tren vet that; vet bien mat sau GiaySong.
/// Anh: PlayTestShots/setchamdat_samset_0.png (ngay luc cham), _1.png (~0,6 s, nut cam), maygiong.png.
/// </summary>
public static class ThuSetChamDat
{
    static bool daBatDau;
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    const string Ra = "PlayTestShots/setchamdat.txt";

    [MenuItem("Diablo 2.5D/106 Cho set cham dat (Sam set + May giong)", false, 175)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (File.Exists(Ra)) File.Delete(Ra);
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuSetChamDat");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool d, string l) { if (!d) { loi++; bao.AppendLine("  LOI: " + l); } }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/setchamdat_" + ten + ".png";
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

    static int DemVetGachBat(GameObject loe)
    {
        int n = 0;
        foreach (var ps in loe.GetComponentsInChildren<ParticleSystem>(false))
            if (System.Array.IndexOf(VfxFactory.LopVetGach, ps.name) >= 0 && ps.gameObject.activeInHierarchy) n++;
        return n;
    }

    /// <summary>Theo doi trong <paramref name="giay"/> giay: moi cho cham moi (prefab loe) + moi tia LightningArc moi.</summary>
    static IEnumerator TheoDoi(float giay, string ten, List<GameObject> loeMoi, List<LightningArc> tiaMoi, HashSet<Object> daThay, bool chup)
    {
        float t0 = Time.time; int chupSo = 0; float lucChamDau = -1f;
        while (Time.time - t0 < giay)
        {
            foreach (var tf in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (tf.parent != null) continue;
                if (!tf.name.StartsWith("Vfx_SetChamDat") && tf.name != "LightningImpact") continue;
                if (daThay.Add(tf.gameObject)) { loeMoi.Add(tf.gameObject); if (lucChamDau < 0f) lucChamDau = Time.time; }
            }
            foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                if (daThay.Add(a)) tiaMoi.Add(a);
            if (chup && lucChamDau > 0f)
            {
                if (chupSo == 0 && Time.time - lucChamDau > 0.05f) { chupSo++; yield return Chup(ten + "_0"); }
                else if (chupSo == 1 && Time.time - lucChamDau > 0.6f) { chupSo++; yield return Chup(ten + "_1"); }
            }
            yield return null;
        }
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
        CapDo.MoCaDuongChoPhepThu(2);
        CapDo.MoCaDuongChoPhepThu(21);
        rig.enabled = true;
        rig.SetView(1);
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        Vector3 cho = toi.transform.position + huong * 8f;
        yield return new WaitForSeconds(1f);

        var daThay = new HashSet<Object>();
        string[] tenKy = { "samset", "maygiong" };
        int[] soKy = { 2, 21 };
        for (int k = 0; k < 2; k++)
        {
            int tiaTruoc = TiaBoDat.SoTiaDaSinh;
            toi.mana = toi.maxMana;
            toi.transform.rotation = Quaternion.LookRotation(huong);
            toi.CastAt(soKy[k], cho);
            var loe = new List<GameObject>(); var tia = new List<LightningArc>();
            // Sam set ~2,4 s; May giong 5 s + bay 4 s
            float giay = k == 0 ? 3.2f : 7f;
            // Vet gach phai tat NGAY khung sinh: kiem trong luc theo doi
            int gachBat = 0;
            var tt = TheoDoi(giay, tenKy[k], loe, tia, daThay, true);
            while (tt.MoveNext())
            {
                foreach (var l in loe) if (l != null) gachBat += DemVetGachBat(l);
                Kiem(VetNutSet.SoDangCo <= VetNutSet.ToiDaCungLuc, "vuot tran vet nut");
                yield return tt.Current;
            }
            int soTiaCon = TiaBoDat.SoTiaDaSinh - tiaTruoc;
            bao.AppendLine(string.Format("{0}: {1} cho cham, {2} tia con (mong {3} = cho cham x {4}), luot vet gach dang bat {5}",
                tenKy[k], loe.Count, soTiaCon, loe.Count * (TiaBoDat.SoTiaDot1 + TiaBoDat.SoTiaDot2), TiaBoDat.SoTiaDot1 + TiaBoDat.SoTiaDot2, gachBat));
            Kiem(loe.Count >= 5, tenKy[k] + ": qua it cho cham");
            Kiem(gachBat == 0, tenKy[k] + ": van con vet gach Sparks / Jet");
            Kiem(soTiaCon == loe.Count * (TiaBoDat.SoTiaDot1 + TiaBoDat.SoTiaDot2), tenKy[k] + ": so tia con khong dung");
            yield return new WaitForSeconds(0.5f);
        }

        // B. Hai dau tia con tren dat: tung rieng mot cho cham, doc tia ngay khung sinh
        {
            var truoc = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            Vector3 p = cho + Vector3.right * 3f;
            VfxFactory.LightningImpact(p, 2.1f);
            float dat = TiaBoDat.DatY(p);
            int n = 0, lech = 0; float xa = 0f;
            foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
            {
                if (truoc.Contains(a)) continue;
                n++;
                if (Mathf.Abs(a.start.y - (TiaBoDat.DatY(a.start) + TiaBoDat.NangKhoiDat)) > 0.03f) lech++;
                if (Mathf.Abs(a.end.y - (TiaBoDat.DatY(a.end) + TiaBoDat.NangKhoiDat)) > 0.03f) lech++;
                Vector3 d = a.end - p; d.y = 0f; xa = Mathf.Max(xa, d.magnitude);
            }
            bao.AppendLine(string.Format("B. mot cho cham (ban kinh 2,1): {0} tia con dot 1, dau mut lech mat dat {1}, dau xa nhat cach {2:F2} m", n, lech, xa));
            Kiem(n == TiaBoDat.SoTiaDot1, "dot 1 khong du tia con");
            Kiem(lech == 0, "dau tia con khong bam mat dat");
            Kiem(xa <= 2.1f * 1.05f + 0.01f, "tia con dot 1 bo qua xa");

            // C. vet nut o cho nay: mau theo tuoi
            VetNutSet vn = null; float gan = 9f;
            foreach (var v in Object.FindObjectsByType<VetNutSet>(FindObjectsInactive.Exclude))
            {
                float d = Vector2.Distance(new Vector2(v.transform.position.x, v.transform.position.z), new Vector2(p.x, p.z));
                if (d < gan) { gan = d; vn = v; }
            }
            Kiem(vn != null && gan < 0.05f, "khong co vet nut o cho cham");
            if (vn != null)
            {
                var sb = new StringBuilder();
                float[] moc = { 0.05f, 0.4f, 1.0f, 1.8f, 2.5f, 5f };
                var mpb = new MaterialPropertyBlock();
                var rSang = vn.transform.Find("NutSang").GetComponent<Renderer>();
                var rDen = vn.transform.Find("NutDen").GetComponent<Renderer>();
                foreach (float m in moc)
                {
                    while (vn != null && vn.Tuoi < m) yield return null;
                    if (vn == null) break;
                    rSang.GetPropertyBlock(mpb); Color s = mpb.GetColor("_TintColor");
                    rDen.GetPropertyBlock(mpb); Color d = mpb.GetColor("_TintColor");
                    sb.AppendFormat("t {0:F2}: quang {1} ({2}) den a {3:F2} | ", vn.Tuoi, s.ToString("F2"), rSang.enabled ? "bat" : "tat", d.a);
                    if (Mathf.Abs(m - 0.05f) < 0.01f) Kiem(s.g > 0.8f && rSang.enabled, "luc dau quang khong trang nong");
                    if (Mathf.Abs(m - 0.4f) < 0.01f) Kiem(s.r > 0.9f && s.g < 0.7f && s.b < 0.4f, "0,4 s quang khong ngoai cam");
                    if (Mathf.Abs(m - 1.0f) < 0.01f) Kiem(s.g < 0.3f && s.r > 0.6f, "1 s quang khong do");
                    if (Mathf.Abs(m - 2.5f) < 0.01f) Kiem(!rSang.enabled && d.a > 0.5f, "2,5 s quang chua tat / vet den mat");
                }
                bao.AppendLine("C. vet nut theo tuoi: " + sb);
                float h2 = Time.time + VetNutSet.GiaySong;
                while (vn != null && Time.time < h2) yield return null;
                Kiem(vn == null, "vet nut khong bien mat");
            }
        }

        // A. DOI CHUNG: duong Loc xoay / Gio loc goi thang LoeSetChamDat - van con vet gach
        {
            var loe = VfxFactory.LoeSetChamDat(cho + Vector3.left * 3f, 2.1f);
            int n = DemVetGachBat(loe);
            bao.AppendLine("A. doi chung LoeSetChamDat (duong Loc xoay / Gio loc): lop vet gach dang bat " + n);
            Kiem(n == 2, "doi chung khong con hai lop vet gach - phep do khong phan biet duoc (hoac Loc xoay bi doi theo)");
        }
        yield return new WaitForSeconds(0.5f);
        Ket();
    }

    static void Ket()
    {
        bao.AppendLine("so loi ghi nhan = " + loi);
        File.WriteAllText(Ra, bao.ToString());
        var rac = GameObject.Find("TAM_ThuSetChamDat"); if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
    }
}
