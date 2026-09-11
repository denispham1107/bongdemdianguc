using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU TEN TREN DAU NHAN VAT (<see cref="BangTen"/>).
///
/// Vao Play o Act2, gan ten cho nhan vat cua minh, sinh ba ban sao nguoi khac
/// (ten co dau tieng Viet) dung quanh minh, roi do tung bang ten:
///
///   1. CO VE that trong khung hinh vua roi, va nam trong man hinh.
///   2. NAM NGAY TREN DAU: day bang ten o tren chop mu that (dinh luoi BAKE o
///      tu the dang dien - doc lap voi xuong head_end ma BangTen dung) va cach
///      no khong qua 0,30 m.
///   3. DUNG MAU: cua minh vang, nguoi khac trang nga.
///   4. DUNG FONT Inter - font mac dinh thieu chu co dau.
///   5. BON BANG TEN KHONG DE LEN NHAU khi dung cach nhau ~2 m.
///   6. Nguoi da guc thi ten MO DI.
///
/// Anh <c>PlayTestShots/bangten_1.png</c> (du ten), <c>bangten_2_guc.png</c>;
/// so do <c>PlayTestShots/bangten.txt</c>.
/// </summary>
public static class ThuBangTen
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat; static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/52. Chay thu TEN TREN DAU nhan vat", false, 141)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (canhCu != Canh) EditorSceneManager.OpenScene(Canh);
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_BangTen");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[BangTen] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// CHOP MU THAT tren man hinh (toa do OnGUI, y tu tren xuong) va so diem anh
    /// cua 0,30 m tai cho ay. Dinh that = dinh cao nhat cua luoi da BAKE o tu the
    /// dang dien - doc lap voi cach BangTen tim dinh dau (xuong head_end).
    /// Khong dung con nhong va cham hay khung bao luoi: do duoc ca hai deu cao
    /// hon chop mu (2,06 m va 1,88 m so voi 1,61 m).
    /// </summary>
    static void ChopMu(Camera cam, GameObject nv, out float yChop, out float px30cm, out float caoChop)
    {
        float cao = 0f;
        Vector3 dinh = nv.transform.position;
        foreach (var smr in nv.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!smr.enabled) continue;
            var m = new UnityEngine.Mesh();
            smr.BakeMesh(m, true);
            foreach (var v in m.vertices)
            {
                var w = smr.transform.TransformPoint(v);
                if (w.y - nv.transform.position.y > cao) { cao = w.y - nv.transform.position.y; dinh = w; }
            }
            Object.DestroyImmediate(m);
        }
        caoChop = cao;
        float y0 = cam.WorldToScreenPoint(dinh).y;
        float y1 = cam.WorldToScreenPoint(dinh + Vector3.up * 0.30f).y;
        yChop = Screen.height - y0;
        px30cm = Mathf.Abs(y1 - y0);
    }

    static IEnumerator KichBan()
    {
        Ghi("[ban 1] ten tren dau nhan vat");

        PlayerController toi = null;
        float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { Loi("khong tim thay nhan vat cua minh"); Ket(); yield break; }
        yield return new WaitForSeconds(2f);

        var cam = Camera.main;
        if (cam == null) { Loi("khong co Camera.main"); Ket(); yield break; }

        // Dat ba ban sao quanh minh, trong khung hinh camera
        Vector3 phai = cam.transform.right; phai.y = 0f; phai.Normalize();
        Vector3 truoc = cam.transform.forward; truoc.y = 0f; truoc.Normalize();
        Vector3 P = toi.transform.position;

        var cacTen = new[] { "Ác Quỷ Bóng Đêm", "Kẻ Săn Hồn", "Người chơi 4" };
        var cho = new[] { P - phai * 2.4f, P + phai * 2.4f, P + truoc * 2.6f };
        var banSao = new List<PlayerController>();
        for (int i = 0; i < 3; i++)
        {
            Vector3 c = cho[i]; c.y = VfxFactory.GroundY(c) + 0.1f;
            var bs = NguoiChoiKhac.Sinh("uid-bangten-" + i, cacTen[i], c);
            if (bs == null) { Loi("khong sinh duoc ban sao " + i); continue; }
            banSao.Add(bs);
        }
        BangTen.Gan(toi.gameObject, "Phù Thuỷ Đỏ", true);

        yield return new WaitForSeconds(1.2f);
        yield return new WaitForEndOfFrame();

        float s = Screen.height / 1080f;
        Ghi(string.Format("man hinh {0}x{1}, ti le {2:F2}, co chu {3}", Screen.width, Screen.height, s,
                          Mathf.Max(12, Mathf.RoundToInt(BangTen.CoChu * s))));

        // ---- 4. Font ----
        var f = BangTen.FontDangDung;
        Ghi("4. font bang ten: " + (f != null ? f.name : "(chua ve lan nao)"));
        if (f == null || !f.name.Contains("Inter")) Loi("bang ten khong dung font Inter");

        // ---- 1-3, 5: tung bang ten ----
        var tatCa = Object.FindObjectsByType<BangTen>(FindObjectsSortMode.None);
        Ghi("so bang ten trong canh: " + tatCa.Length + " (mong 4)");
        if (tatCa.Length != 4) Loi("so bang ten khac 4");

        var oDaVe = new List<KeyValuePair<string, Rect>>();
        foreach (var b in tatCa)
        {
            bool vuaVe = b.khungVeCuoi >= Time.frameCount - 3;
            float yChop, px30, caoChop;
            ChopMu(cam, b.gameObject, out yChop, out px30, out caoChop);
            var o = b.oCuoi;
            float khe = yChop - o.yMax;          // >0: day chu nam tren chop mu
            bool trongManHinh = o.xMin >= 0 && o.xMax <= Screen.width && o.yMin >= 0 && o.yMax <= Screen.height;
            float lechNgang = Mathf.Abs(o.center.x - cam.WorldToScreenPoint(b.transform.position).x);

            Ghi(string.Format("   \"{0}\" ({1}): ve {2}, o chu x {3:F0}..{4:F0} y {5:F0}..{6:F0}; chop mu cao {7:F2} m, y {8:F0}; khe tu day chu den chop mu {9:F1} px (0,30 m = {10:F1} px); lech ngang so voi chan {11:F1} px; mau {12}",
                b.ten, b.laToi ? "cua minh" : "nguoi khac", vuaVe, o.xMin, o.xMax, o.yMin, o.yMax,
                caoChop, yChop, khe, px30, lechNgang, b.mauCuoi));

            if (!vuaVe) Loi(b.ten + ": khong duoc ve trong khung hinh vua roi");
            if (!trongManHinh) Loi(b.ten + ": bang ten ra ngoai man hinh");
            if (khe < -2f) Loi(b.ten + ": bang ten de len chop mu");
            if (khe > px30) Loi(b.ten + ": bang ten lo lung cach chop mu hon 0,30 m (" + khe.ToString("F0") + " px)");

            Color mong = b.laToi ? new Color(1.00f, 0.84f, 0.42f) : new Color(0.95f, 0.92f, 0.86f);
            if (Mathf.Abs(b.mauCuoi.r - mong.r) > 0.02f || Mathf.Abs(b.mauCuoi.b - mong.b) > 0.02f)
                Loi(b.ten + ": sai mau");

            oDaVe.Add(new KeyValuePair<string, Rect>(b.ten, o));
        }

        int chong = 0;
        for (int i = 0; i < oDaVe.Count; i++)
            for (int j = i + 1; j < oDaVe.Count; j++)
                if (oDaVe[i].Value.Overlaps(oDaVe[j].Value))
                { chong++; Ghi("   chong nhau: " + oDaVe[i].Key + " / " + oDaVe[j].Key); }
        Ghi("5. so cap bang ten de len nhau: " + chong);
        if (chong > 0) Loi("bang ten de len nhau");

        yield return Chup("bangten_1");

        // ---- 6. Nguoi da guc ----
        if (banSao.Count > 0)
        {
            var mau = banSao[0].GetComponent<Damageable>();
            if (mau != null) mau.Die();
            yield return new WaitForSeconds(0.6f);
            yield return new WaitForEndOfFrame();
            var b0 = banSao[0].GetComponent<BangTen>();
            float a = b0 != null ? b0.mauCuoi.a : -1f;
            Ghi(string.Format("6. \"{0}\" da guc: do duc ten {1:F2} (mong < 0,6)", b0 != null ? b0.ten : "?", a));
            if (a < 0f || a >= 0.6f) Loi("nguoi da guc ma ten khong mo di");
            yield return Chup("bangten_2_guc");
        }

        foreach (var bs in banSao) NguoiChoiKhac.Bo(bs);
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/bangten.txt", bao.ToString());
        var rac = GameObject.Find("TAM_BangTen");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh) EditorSceneManager.OpenScene(canhCu);
    }
}
