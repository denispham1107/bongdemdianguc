using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 69): GIUT SET MOI (16/09/2026).
///
/// Nguoi dung: tam 20 m; sat thuong ban dau 75; phong CUNG LUC 4 tia neu co 4 ke dich phia truoc
/// trong tam (moi tia van lan nhu cu); moi cu trung (ca tia lan) 15% choang.
///
/// Do nhung gi (Play, Act2, bia dung tren khong trung cho khoi vuong canh vat):
///   A. Thong so: vat Giut set nguoi choi (tam, sat thuong, so tia, choang), vach ngam TamNgam(6);
///      Giut set cua QUAI khong choang.
///   B. Tung THAT CastAt(6) vao 5 bia xep vong cung (cach nhau > ban kinh lan 5,5 m): dung 4 bia mat
///      dung 75 CUNG MOT KHUNG HINH, bia thu 5 khong mat gi.
///   C. Tam: bia duy nhat, mat bia (tam - 0,4 m) cach 19,9 m -> trung; doi chung 20,4 m -> khong trung.
///      Tam cu 19,5 se KHONG trung ca bia thu nhat - phep do phan biet duoc.
///   D. Lan: bia A trong tam, bia B NGOAI tam 20 m nhung cach A 4 m -> B trung qua tia lan, mat 75 x 0,85.
///   E. Choang: 160 lan phong vao 4 bia dau + 4 bia lan (ngoai tam) -> ti le choang tia dau va tia lan
///      rieng, dem bang StunnedEffect co tren bia (doc lap voi bo dem trong GiatSet).
///   F. Cap ky nang 3: choang keo dai 1,5 + 0,30 giay.
///
/// Ket qua: PlayTestShots/giatset.txt, anh giatset_*.png.
/// </summary>
public static class ThuGiatSet
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/69. Chay thu GIUT SET (20 m, 75, 4 tia, 15% choang)", false, 157)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu giut set", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Giut set: tam 20, sat thuong 75, 4 tia, 15% choang");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
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
        if (GameObject.Find("TAM_GiatSet") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_GiatSet");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GiatSet] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static readonly List<GameObject> rac = new List<GameObject>();

    static Damageable TaoBia(string ten, Vector3 chan)
    {
        var go = new GameObject(ten);
        go.transform.position = chan;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 10000000f; d.health = 10000000f;
        rac.Add(go);
        return d;
    }

    static void DonBia()
    {
        foreach (var g in rac) if (g != null) Object.DestroyImmediate(g);
        rac.Clear();
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

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>Diem tren vong cung ban kinh r, goc a (do) quanh huong h, tinh tu goc g (mat phang ngang).</summary>
    static Vector3 Vong(Vector3 g, Vector3 h, float r, float a)
    {
        return g + Quaternion.AngleAxis(a, Vector3.up) * h * r;
    }

    static IEnumerator KichBan()
    {
        float han = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han) yield return null;
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);
        int maskEnemy = LayerMask.GetMask("Enemy");
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();

        // ================= A. THONG SO =================
        var mau = GiatSet.Phong(Vector3.up * 500f, huong, 0);
        var quai = GiatSet.PhongCuaQuai(Vector3.up * 520f, huong, 0, 20f, 14f, Color.white, Color.white);
        Ghi(string.Format("A. Giut set nguoi choi: tam {0}, sat thuong {1}, so tia {2}, choang {3:P0} trong {4} giay; vach ngam TamNgam(6) = {5}; Giut set cua quai: choang {6:P0}",
            mau.range, mau.damage, mau.soTiaDau, mau.xacSuatChoang, mau.giayChoang, toi.TamNgam(6), quai.xacSuatChoang));
        Kiem(Mathf.Approximately(mau.range, 20f) && Mathf.Approximately(toi.TamNgam(6), 20f), "tam khong phai 20 m");
        Kiem(Mathf.Approximately(mau.damage, 75f), "sat thuong ban dau khong phai 75");
        Kiem(mau.soTiaDau == 4, "khong phong 4 tia");
        Kiem(Mathf.Approximately(mau.xacSuatChoang, 0.15f), "xac suat choang khong phai 15%");
        Kiem(quai.xacSuatChoang == 0f, "Giut set cua quai cung choang");
        Object.Destroy(mau.gameObject); Object.Destroy(quai.gameObject);
        yield return null;

        // ================= B. TUNG THAT VAO 5 BIA =================
        Ghi("");
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(6);
        toi.mana = toi.maxMana;
        Vector3 chanToi = toi.transform.position;
        // 5 bia tren vong cung 15 m, cach nhau 25 do (day cung 6,5 m > ban kinh lan 5,5)
        var bia = new List<Damageable>();
        float[] goc = { -50f, -25f, 0f, 25f, 50f };
        foreach (var a in goc) bia.Add(TaoBia("TAM_Bia" + a, Vong(chanToi, huong, 15f, a)));
        yield return new WaitForFixedUpdate();
        var truoc = new float[5];
        for (int i = 0; i < 5; i++) truoc[i] = bia[i].health;
        toi.CastAt(6, bia[2].transform.position);
        // doi den khung dau tien co bia mat mau, roi dem trong CHINH khung ay
        int khungDau = -1; int soTrungKhungDau = 0;
        float hanB = Time.time + 2f;
        while (Time.time < hanB)
        {
            int dem = 0;
            for (int i = 0; i < 5; i++) if (bia[i].health < truoc[i] - 0.01f) dem++;
            if (dem > 0 && khungDau < 0) { khungDau = Time.frameCount; soTrungKhungDau = dem; yield return Chup("giatset_1_bon_tia"); }
            yield return null;
        }
        var mat = new float[5]; int soMat75 = 0, soKhong = 0;
        var sb = new StringBuilder();
        for (int i = 0; i < 5; i++)
        {
            mat[i] = truoc[i] - bia[i].health;
            if (Mathf.Abs(mat[i] - 75f) < 0.01f) soMat75++;
            if (mat[i] < 0.01f) soKhong++;
            sb.AppendFormat("{0}do:{1:F2} ", goc[i], mat[i]);
        }
        Ghi("B. tung that vao 5 bia (vong cung 15 m, cach nhau 6,5 m): mat mau " + sb.ToString().Trim()
            + " -> mat dung 75: " + soMat75 + ", khong mat: " + soKhong + "; so bia trung ngay khung dau tien: " + soTrungKhungDau);
        Kiem(soMat75 == 4 && soKhong == 1, "khong phong dung 4 tia, moi tia 75 sat thuong");
        Kiem(soTrungKhungDau == 4, "4 tia khong trung cung mot luc");
        DonBia();
        yield return new WaitForSeconds(0.5f);

        // ================= C. TAM 20 M =================
        Vector3 g = chanToi + Vector3.up * 60f + huong * 3f;     // tren khong trung
        Vector3 goc0 = g + Vector3.up;                            // cung do cao voi tam bia
        float[] tam = { 20.3f, 20.8f };
        var trungTam = new bool[2];
        for (int k = 0; k < 2; k++)
        {
            var b = TaoBia("TAM_BiaTam", g + huong * tam[k]);
            yield return new WaitForFixedUpdate();
            float m0 = b.health;
            var gs = GiatSet.Phong(goc0, huong, maskEnemy);
            gs.xacSuatChoang = 0f;
            yield return new WaitForSeconds(0.25f);
            trungTam[k] = b.health < m0 - 0.01f;
            DonBia();
            yield return null;
        }
        Ghi(string.Format("C. tam: bia tam 20,3 m (mat bia 19,9 m) trung {0}; bia tam 20,8 m (mat bia 20,4 m) trung {1} (tam cu 19,5 thi ca hai deu khong trung)",
            trungTam[0], trungTam[1]));
        Kiem(trungTam[0] && !trungTam[1], "tam danh khong phai 20 m");

        // ================= D. TIA LAN =================
        {
            var A = TaoBia("TAM_BiaA", g + huong * 18f);
            var B = TaoBia("TAM_BiaB", g + huong * 22f);
            yield return new WaitForFixedUpdate();
            float mA = A.health, mB = B.health;
            var gs = GiatSet.Phong(goc0, huong, maskEnemy);
            gs.xacSuatChoang = 0f;
            yield return new WaitForSeconds(0.4f);
            Ghi(string.Format("D. lan: bia A (18 m) mat {0:F2}, bia B (22 m, ngoai tam, cach A 4 m) mat {1:F2} (mong 75 va 63,75)", mA - A.health, mB - B.health));
            // TakeDamage lam tron sat thuong ve so nguyen (63,75 -> 64) - so voi so da lam tron
            Kiem(Mathf.Abs(mA - A.health - 75f) < 0.01f && Mathf.Abs(mB - B.health - Mathf.Round(63.75f)) < 0.01f, "tia lan khong con nhu cu");
            DonBia();
        }

        // ================= E. CHOANG 15% =================
        {
            var dau = new List<Damageable>(); var lan = new List<Damageable>();
            foreach (var a in new[] { -30f, -10f, 10f, 30f })
            {
                dau.Add(TaoBia("TAM_Dau" + a, Vong(g, huong, 17f, a)));
                lan.Add(TaoBia("TAM_Lan" + a, Vong(g, huong, 21.5f, a)));
            }
            yield return new WaitForFixedUpdate();
            int trungDau = 0, choangDau = 0, trungLan = 0, choangLan = 0;
            int d0 = GiatSet.SoTrungDau, cd0 = GiatSet.SoChoangDau, l0 = GiatSet.SoTrungLan, cl0 = GiatSet.SoChoangLan;
            const int N = 160;
            for (int lanPhong = 0; lanPhong < N; lanPhong++)
            {
                var hp = new Dictionary<Damageable, float>();
                foreach (var x in dau) { x.health = x.maxHealth; hp[x] = x.health; }
                foreach (var x in lan) { x.health = x.maxHealth; hp[x] = x.health; }
                GiatSet.Phong(goc0, huong, maskEnemy);
                yield return new WaitForSeconds(0.16f);
                foreach (var x in dau)
                {
                    if (x.health < hp[x] - 0.01f) trungDau++;
                    var st = x.GetComponent<StunnedEffect>();
                    if (st != null) { choangDau++; Object.DestroyImmediate(st); }
                }
                foreach (var x in lan)
                {
                    if (x.health < hp[x] - 0.01f) trungLan++;
                    var st = x.GetComponent<StunnedEffect>();
                    if (st != null) { choangLan++; Object.DestroyImmediate(st); }
                }
                if (lanPhong == 3) yield return Chup("giatset_2_tia_lan_choang");
            }
            float tlDau = 100f * choangDau / Mathf.Max(1, trungDau), tlLan = 100f * choangLan / Mathf.Max(1, trungLan);
            Ghi(string.Format("E. {0} lan phong: tia dau trung {1}, choang {2} = {3:F1}%; tia lan trung {4}, choang {5} = {6:F1}% (mong 15%); bo dem trong GiatSet: dau {7}/{8}, lan {9}/{10}",
                N, trungDau, choangDau, tlDau, trungLan, choangLan, tlLan,
                GiatSet.SoChoangDau - cd0, GiatSet.SoTrungDau - d0, GiatSet.SoChoangLan - cl0, GiatSet.SoTrungLan - l0));
            Kiem(trungDau == 4 * N && trungLan == 4 * N, "cach xep bia khong cho 4 tia dau + 4 tia lan moi lan - phep do hong");
            Kiem(Mathf.Abs(tlDau - 15f) <= 4f, "ti le choang tia dau lech qua xa 15%");
            Kiem(Mathf.Abs(tlLan - 15f) <= 4f, "ti le choang tia lan lech qua xa 15%");
            DonBia();
        }

        // ================= F. CAP KY NANG =================
        {
            CapDo.BatDauTranMoi();
            CapDo.Them(100 + 135);
            CapDo.MoCaDuongChoPhepThu(6); CapDo.NangCap(6); CapDo.NangCap(6);
            yield return new WaitForSeconds(0.7f);
            toi.mana = toi.maxMana;
            toi.CastAt(6, chanToi + huong * 10f);
            GiatSet vua = null;
            float hanF = Time.time + 1f;
            // Loc DUNG vat cua nguoi choi (boQua = minh): lan chay dau bat nham Giut set cua con phu thuy
            // trong dot quai vua ra (sat thuong 14).
            while (vua == null && Time.time < hanF)
            {
                foreach (var x in Object.FindObjectsByType<GiatSet>(FindObjectsInactive.Exclude)) if (x.boQua == mauToi) vua = x;
                yield return null;
            }
            Ghi(string.Format("F. cap ky nang {0}: choang {1:F2} giay (mong 1,80), sat thuong {2:F1} (mong 75 x 1,44 = 108)",
                CapDo.CapCuaKyNang(6), vua != null ? vua.giayChoang : -1f, vua != null ? vua.damage : -1f));
            Kiem(vua != null && Mathf.Abs(vua.giayChoang - 1.8f) < 0.01f, "cap ky nang khong keo dai choang");
            Kiem(vua != null && Mathf.Abs(vua.damage - 108f) < 0.01f, "sat thuong theo cap sai");
        }

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[GiatSet] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        DonBia();
        File.WriteAllText("PlayTestShots/giatset.txt", bao.ToString());
        var r = GameObject.Find("TAM_GiatSet");
        if (r != null) Object.DestroyImmediate(r);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
