using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 106 - CHO SET CHAM DAT CUA SAM SET + MAY GIONG + LOC XOAY (nguoi dung 05/10/2026 chon mau A + vet nut cua mau B; Loc xoay them
/// cung ngay). Play Act2 ban dem, may quay 2.5D, tung SAM SET, MAY GIONG, LOC XOAY THAT (CastAt) vao cho trong 8 m truoc mat. Kiem:
///  A. moi cho cham (prefab Vfx_SetChamDat) KHONG con lop Sparks / Jet dang bat; DOI CHUNG: LoeSetChamDat goi thang (prefab tho)
///     van con ca hai lop; Gio loc (khong loe) khong sinh tia con nao.
///  L. Loc xoay: tia con BAM THEO loc - lech tu loc giu nguyen trong luc loc da di (do 0,15 s sau khi sinh).
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

    [MenuItem("Diablo 2.5D/106 Cho set cham dat (Sam set + May giong + Loc xoay)", false, 175)]
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

    static void Quet(List<GameObject> loeMoi, HashSet<Object> daThay)
    {
        foreach (var tf in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
        {
            if (tf.parent != null) continue;
            if (!tf.name.StartsWith("Vfx_SetChamDat") && tf.name != "LightningImpact") continue;
            if (daThay.Add(tf.gameObject)) loeMoi.Add(tf.gameObject);
        }
    }

    /// <summary>Tat moi nguon set dang chay (Sam set, May giong, Loc xoay) - cua so do sau khong lan cua so truoc.</summary>
    static void TatNguonSet()
    {
        foreach (var x in Object.FindObjectsByType<LightningStorm>(FindObjectsInactive.Exclude)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<MayGiong>(FindObjectsInactive.Exclude)) Object.Destroy(x.gameObject);
        foreach (var x in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) Object.Destroy(x.gameObject);
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
        CapDo.MoCaDuongChoPhepThu(3);
        CapDo.MoCaDuongChoPhepThu(10);
        rig.enabled = true;
        rig.SetView(1);
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        Vector3 cho = toi.transform.position + huong * 8f;
        yield return new WaitForSeconds(1f);

        var daThay = new HashSet<Object>();
        string[] tenKy = { "samset", "maygiong", "locxoay" };
        int[] soKy = { 2, 21, 3 };
        for (int k = 0; k < 3; k++)
        {
            int tiaTruoc = TiaBoDat.SoTiaDaSinh, taoTruoc = TiaBoDat.SoLanTao, matTruoc = TiaBoDat.SoLanMatDot2;
            toi.mana = toi.maxMana;
            toi.transform.rotation = Quaternion.LookRotation(huong);
            toi.CastAt(soKy[k], cho);
            var loe = new List<GameObject>(); var tia = new List<LightningArc>();
            // Sam set ~2,4 s; May giong 5 s + bay 4 s
            float giay = k == 0 ? 3.2f : k == 1 ? 9.5f : 3.5f;
            // Vet gach phai tat NGAY khung sinh: kiem trong luc theo doi
            int gachBat = 0;
            var tt = TheoDoi(giay, tenKy[k], loe, tia, daThay, true);
            while (tt.MoveNext())
            {
                foreach (var l in loe) if (l != null) gachBat += DemVetGachBat(l);
                Kiem(VetNutSet.SoDangCo <= VetNutSet.ToiDaCungLuc, "vuot tran vet nut");
                yield return tt.Current;
            }
            // Dong cua so: tat nguon set (lan Zap / tia cuoi khong lot sang cua so sau), quet lan cuoi, cho dot 2
            TatNguonSet();
            Quet(loe, daThay);
            int soTiaCon = TiaBoDat.SoTiaDaSinh - tiaTruoc;
            yield return new WaitForSeconds(0.2f);   // dot 2 cua cho cham cuoi
            soTiaCon = TiaBoDat.SoTiaDaSinh - tiaTruoc;
            int taoMoi = TiaBoDat.SoLanTao - taoTruoc;
            Kiem(taoMoi == loe.Count, tenKy[k] + ": so lan tao tia bo dat khac so cho cham");
            bao.AppendLine(string.Format("{0}: {1} cho cham, {2} tia con (mong {3} = cho cham x {4}), luot vet gach dang bat {5}; TiaBoDat tao {6}, mat dot 2 {7}",
                tenKy[k], loe.Count, soTiaCon, loe.Count * (TiaBoDat.SoTiaDot1 + TiaBoDat.SoTiaDot2), TiaBoDat.SoTiaDot1 + TiaBoDat.SoTiaDot2, gachBat,
                TiaBoDat.SoLanTao - taoTruoc, TiaBoDat.SoLanMatDot2 - matTruoc));
            Kiem(loe.Count >= 5, tenKy[k] + ": qua it cho cham");
            Kiem(gachBat == 0, tenKy[k] + ": van con vet gach Sparks / Jet");
            Kiem(soTiaCon == loe.Count * (TiaBoDat.SoTiaDot1 + TiaBoDat.SoTiaDot2), tenKy[k] + ": so tia con khong dung");
            yield return new WaitForSeconds(1.5f);
        }

        // L. Loc xoay: tia con bam theo loc dang di
        {
            foreach (var x in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) Object.Destroy(x.gameObject);
            yield return null;
            toi.mana = toi.maxMana;
            toi.CastAt(3, cho + Vector3.forward * 2f);
            Tornado loc = null; float h = Time.time + 2f;
            while (loc == null && Time.time < h) { loc = Object.FindAnyObjectByType<Tornado>(); yield return null; }
            int lan = 0, lech = 0; float diMax = 0f, lechMax = 0f;
            float h2 = Time.time + 3f;
            LightningArc daDo = null;
            while (loc != null && Time.time < h2 && lan < 6)
            {
                var a = TiaBoDat.TiaMoiNhat;
                if (a != null && a != daDo)
                {
                    daDo = a;
                    // Doc o CUOI khung: LightningArc.BamTheo cap nhat trong LateUpdate (coroutine yield null doc truoc do -> lech mot khung)
                    yield return new WaitForEndOfFrame();
                    if (a == null || loc == null) continue;
                    Vector3 lech0 = a.start - loc.transform.position, p0 = loc.transform.position;
                    yield return new WaitForSeconds(0.15f);
                    yield return new WaitForEndOfFrame();
                    if (a == null || loc == null) continue;
                    Vector3 lech1 = a.start - loc.transform.position;
                    float di = Vector3.Distance(loc.transform.position, p0);
                    lan++; diMax = Mathf.Max(diMax, di); lechMax = Mathf.Max(lechMax, (lech1 - lech0).magnitude);
                    if ((lech1 - lech0).magnitude > 0.05f) lech++;
                }
                yield return null;
            }
            bao.AppendLine(string.Format("L. Loc xoay: {0} lan do tia con sau 0,15 s - loc da di toi {1:F2} m, lech tia so voi loc doi toi {2:F3} m", lan, diMax, lechMax));
            Kiem(lan >= 3 && diMax > 0.3f, "L: khong do duoc tia con khi loc dang di");
            Kiem(lech == 0, "L: tia con khong bam theo loc");
            foreach (var x in Object.FindObjectsByType<Tornado>(FindObjectsInactive.Exclude)) Object.Destroy(x.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        // DOI CHUNG Gio loc: khong loe cham dat -> khong tia con
        {
            int truoc = TiaBoDat.SoTiaDaSinh;
            toi.mana = toi.maxMana;
            toi.transform.rotation = Quaternion.LookRotation(huong);
            toi.CastAt(10, cho);
            yield return new WaitForSeconds(3f);
            bao.AppendLine("A. doi chung Gio loc (khong loe): tia con sinh them " + (TiaBoDat.SoTiaDaSinh - truoc));
            Kiem(TiaBoDat.SoTiaDaSinh == truoc, "Gio loc lai sinh tia con bo dat");
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
            bao.AppendLine("A. doi chung LoeSetChamDat goi thang (prefab tho): lop vet gach dang bat " + n);
            Kiem(n == 2, "doi chung khong con hai lop vet gach - phep do khong phan biet duoc");
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
