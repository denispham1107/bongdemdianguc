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
///   G. VE LAI (nguoi dung 24/09/2026, theo anh mau): tung THAT CastAt(6) vao 3 bia truoc mat -
///      G1 tia dung anh Blender (GiatSetLoi/GiatSetQuang), song 0,6 giay, van sang nguyen o giay 0,40
///         (doi chung: tia LightningArc mac dinh khong dung anh, song 0,26);
///      G2 tu the DAY HAI TAY: ca hai canh tay chi ra truoc (tich vo huong voi huong nhin > 0,85), hai ban
///         tay o truoc nguc (doi chung: luc dung yen truoc khi tung);
///      G3 dau tia nam GIUA HAI TAY (doi chung: diem phong phep cu o dau gay tay phai);
///      G4 bia chay 1,5 m thi cuoi tia chay theo (doi chung: tia khong bam thi dung yen);
///      G5 dang giu tay ma nhan vat buoc di thi bo tay xuong; G6 Giut set cua QUAI cung dung anh, giu mau xanh la.
///   H. GOC CHOI THAT (nguoi dung 25/09/2026: "khi tu choi trong Unity tia qua sang, qua day"): may quay game o
///      goc mac dinh (3D tu do, 7,5 m, fov 55) - ca BAN NGAY (nhu luc choi, phep thu thuong nhay thang toi dem)
///      lan ban dem; chup roi DEM diem anh chui trang (ca ba kenh > 0,94) trong vung giua nguoi choi va bia.
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

        yield return MucG(toi, chanToi, huong);
        yield return MucH(toi, chanToi, huong);

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static Damageable TaoBiaThay(string ten, Vector3 chan)
    {
        var d = TaoBia(ten, chan);
        // Than nhin thay duoc cho anh chup (bia goc chi co va cham)
        var hinh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Object.DestroyImmediate(hinh.GetComponent<Collider>());
        hinh.transform.SetParent(d.transform, false);
        hinh.transform.localPosition = Vector3.up;
        hinh.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        // Than TOI: bia trang duoi nang ban ngay lot vao phep dem diem anh trang (cat ngang tia)
        var mr = hinh.GetComponent<MeshRenderer>();
        if (mr != null) mr.material.color = new Color(0.12f, 0.10f, 0.10f);
        return d;
    }

    static List<LightningArc> TiaCua(Transform goc, float xa)
    {
        var ds = new List<LightningArc>();
        foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
            if (Vector3.Distance(a.start, goc.position) < xa || Vector3.Distance(a.end, goc.position) < xa) ds.Add(a);
        return ds;
    }

    static float DoAlpha(LightningArc a, string ten)
    {
        var t = a.transform.Find(ten);
        if (t == null) return -1f;
        var mr = t.GetComponent<MeshRenderer>();
        return mr != null && mr.sharedMaterial != null ? mr.sharedMaterial.GetColor("_TintColor").a : -1f;
    }

    static string TenAnh(LightningArc a, string ten)
    {
        var t = a.transform.Find(ten);
        if (t == null) return "(khong co)";
        var mr = t.GetComponent<MeshRenderer>();
        return mr != null && mr.sharedMaterial != null && mr.sharedMaterial.mainTexture != null ? mr.sharedMaterial.mainTexture.name : "(khong anh)";
    }

    /// <summary>
    /// Dem diem anh CHOI TRANG (ca ba kenh > 0,94) va diem anh XANH SANG (b > 0,55, b > r + 0,12) trong khung chu
    /// nhat bao quanh doan tia tren man hinh (tu diem giua hai tay toi bia, noi rong 12% man hinh).
    /// </summary>
    static void DemDiemAnh(string duong, Camera cam, Vector3 a, Vector3 b, out int choi, out int xanh, out int tong)
    {
        choi = xanh = tong = 0;
        if (!File.Exists(duong) || cam == null) return;
        var t = new Texture2D(2, 2);
        t.LoadImage(File.ReadAllBytes(duong));
        Vector3 sa = cam.WorldToViewportPoint(a), sb = cam.WorldToViewportPoint(b);
        float le = 0.12f;
        int x0 = Mathf.Clamp((int)((Mathf.Min(sa.x, sb.x) - le) * t.width), 0, t.width - 1);
        int x1 = Mathf.Clamp((int)((Mathf.Max(sa.x, sb.x) + le) * t.width), 0, t.width - 1);
        int y0 = Mathf.Clamp((int)((Mathf.Min(sa.y, sb.y) - le) * t.height), 0, t.height - 1);
        int y1 = Mathf.Clamp((int)((Mathf.Max(sa.y, sb.y) + le) * t.height), 0, t.height - 1);
        var px = t.GetPixels();
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                var c = px[y * t.width + x];
                tong++;
                if (c.r > 0.94f && c.g > 0.94f && c.b > 0.94f) choi++;
                else if (c.b > 0.55f && c.b > c.r + 0.12f) xanh++;
            }
        Object.DestroyImmediate(t);
    }

    /// <summary>
    /// CAT NGANG tia tren anh chup: voi moi bia va moi t (0,40..0,80 doan tu tay toi bia), quet mot doan vuong goc
    /// voi tia tren man hinh (+-70 diem anh) va dem so diem TRANG (ca ba kenh > 0,85) va XANH (b > 0,45, b > r + 0,15,
    /// khong trang). Tra ve trung binh moi lat cat = be rong phan trang / phan xanh tinh bang diem anh.
    /// </summary>
    static void CatNgang(string duong, Camera cam, Vector3 a, List<Damageable> ds, out float trang, out float xanh)
    {
        float sangXanh;
        CatNgang(duong, cam, a, ds, out trang, out xanh, out sangXanh);
    }

    /// <summary>Nhu tren, them <paramref name="sangXanh"/> = TONG kenh xanh lam (b) cua cac diem anh xanh tren moi lat cat
    /// - vien xanh day hon hay sang hon deu lam so nay tang.</summary>
    static void CatNgang(string duong, Camera cam, Vector3 a, List<Damageable> ds, out float trang, out float xanh, out float sangXanh)
    {
        trang = xanh = sangXanh = 0f;
        if (!File.Exists(duong) || cam == null) return;
        var tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(duong));
        var px = tex.GetPixels();
        int w = tex.width, h = tex.height, soLat = 0;
        foreach (var d in ds)
        {
            if (d == null) continue;
            Vector3 b = d.transform.position + Vector3.up;
            Vector3 sa = cam.WorldToViewportPoint(a), sb = cam.WorldToViewportPoint(b);
            Vector2 A = new Vector2(sa.x * w, sa.y * h), B = new Vector2(sb.x * w, sb.y * h);
            Vector2 dir = (B - A).normalized, n = new Vector2(-dir.y, dir.x);
            for (float t = 0.40f; t <= 0.801f; t += 0.1f)
            {
                Vector2 P = Vector2.Lerp(A, B, t);
                int tr = 0, xa = 0; float sb2 = 0f;
                for (int k = -70; k <= 70; k++)
                {
                    Vector2 Q = P + n * k;
                    int x = Mathf.RoundToInt(Q.x), y = Mathf.RoundToInt(Q.y);
                    if (x < 0 || y < 0 || x >= w || y >= h) continue;
                    var c = px[y * w + x];
                    if (c.r > 0.85f && c.g > 0.85f && c.b > 0.85f) tr++;
                    else if (c.b > 0.45f && c.b > c.r + 0.15f) { xa++; sb2 += c.b; }
                }
                trang += tr; xanh += xa; sangXanh += sb2; soLat++;
            }
        }
        if (soLat > 0) { trang /= soLat; xanh /= soLat; sangXanh /= soLat; }
        Object.DestroyImmediate(tex);
    }

    static IEnumerator MucH(PlayerController toi, Vector3 chanToi, Vector3 huong)
    {
        Ghi("");
        var hh = toi.hoatHinhRieng;
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;
        var chuyen = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        if (hh == null || cam == null || rig == null) { Ghi("[LOI] H. thieu nhan vat / may quay"); loi++; yield break; }
        rig.enabled = true;
        rig.SetView(0);
        var ds = new List<Damageable>();
        // MOT bia: lat cat ngang chi gap dung mot tia (ba tia thi lat cat cua tia nay quet trung tia ben canh)
        ds.Add(TaoBiaThay("TAM_H", Vong(chanToi, huong, 9f, 0f)));
        string[] ten = { "ngay", "dem" };
        for (int lan = 0; lan < 2; lan++)
        {
            if (chuyen != null) { chuyen.enabled = false; chuyen.ApGiay(lan == 0 ? ChuyenChieuSangDem.GiayGiuaNgay : ChuyenChieuSangDem.GiayGiuaDem); }
            toi.transform.rotation = Quaternion.LookRotation(huong);
            yield return new WaitForSeconds(1.2f);                // may quay bam kip, tay buong xuong
            // NAM LAN TUNG roi lay trung binh: tia doi hinh ngau nhien moi 0,045 s, mot anh chup lech toi vai chuc
            // phan tram - khong du de thay mot thay doi 10% (nguoi dung 25/09/2026: "vien xanh sang va day hon 10%")
            const int SoLanTung = 5;
            float tbTrang = 0f, tbXanh = 0f, tbSang = 0f;
            string duong = "PlayTestShots/giatset_6_goc_choi_" + ten[lan] + ".png";
            Vector3 a0 = hh.DiemGiatSet.position, b0 = ds[0].transform.position + Vector3.up;
            int choi = 0, xanh = 0, tong = 0;
            for (int k = 0; k < SoLanTung; k++)
            {
                if (k > 0) yield return new WaitForSeconds(0.9f);   // tia truoc tat han, het hoi chieu
                toi.transform.rotation = Quaternion.LookRotation(huong);
                toi.mana = toi.maxMana;
                toi.CastAt(6, ds[0].transform.position);
                float h = Time.time + 1.5f;
                while (hh.MucDayTay < 0.99f && Time.time < h) yield return null;
                yield return new WaitForSeconds(0.08f);
                a0 = hh.DiemGiatSet.position;
                yield return Chup("giatset_6_goc_choi_" + ten[lan]);
                if (k == 0) DemDiemAnh(duong, cam, a0, b0, out choi, out xanh, out tong);
                float catTrang, catXanh, catSang;
                CatNgang(duong, cam, a0, ds, out catTrang, out catXanh, out catSang);
                tbTrang += catTrang / SoLanTung; tbXanh += catXanh / SoLanTung; tbSang += catSang / SoLanTung;
            }
            Ghi(string.Format("H. cat ngang tia ({0}, trung binh {1} lan tung x 5 lat cat): phan TRANG rong {2:F1} diem anh, phan XANH rong {3:F1} diem anh, tong do sang xanh {4:F1}",
                ten[lan], SoLanTung, tbTrang, tbXanh, tbSang));
            Ghi(string.Format("H. goc choi that ({0}, may quay '{1}', cach {2:F1} m, fov {3:F0}): vung tia {4} diem anh - chui trang {5} ({6:F2}%), xanh sang {7} ({8:F2}%)",
                ten[lan], rig.CurrentName, Vector3.Distance(cam.transform.position, (a0 + b0) * 0.5f), cam.fieldOfView,
                tong, choi, 100f * choi / Mathf.Max(1, tong), xanh, 100f * xanh / Mathf.Max(1, tong)));
            yield return new WaitForSeconds(0.8f);
        }
        DonBia();
    }

    static IEnumerator MucG(PlayerController toi, Vector3 chanToi, Vector3 huong)
    {
        Ghi("");
        var hh = toi.hoatHinhRieng;
        if (hh == null || hh.banTayTrai == null || hh.banTayPhai == null || hh.DiemGiatSet == null)
        {
            Ghi("[LOI] G. nhan vat thieu NguoiChoiHoatHinh / ban tay / DiemGiatSet");
            loi++;
            yield break;
        }
        // Tat dot quai + don quai va dan dang bay: anh chup khong lan lon, va phep thu dai khong de
        // GameDirector sinh quai (tung lam GPU qua tai, Unity tat han - menu 74)
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);
        foreach (var q in Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude)) Object.Destroy(q.gameObject);
        CapDo.BatDauTranMoi();
        CapDo.MoCaDuongChoPhepThu(6);
        yield return new WaitForSeconds(0.8f);

        // Quay mat ve phia bia, dung yen cho tay buong xuong
        toi.transform.rotation = Quaternion.LookRotation(huong);
        yield return new WaitForSeconds(0.6f);
        Vector3 f = toi.transform.forward;

        // --- Doi chung G2: tay luc dung yen ---
        float chiTruocPhai0 = Vector3.Dot((hh.banTayPhai.position - hh.tayPhaiTren.position).normalized, f);
        float chiTruocTrai0 = Vector3.Dot((hh.banTayTrai.position - hh.tayTraiTren.position).normalized, f);

        var ds = new List<Damageable>();
        foreach (var a in new[] { -14f, 0f, 14f }) ds.Add(TaoBiaThay("TAM_G" + a, Vong(chanToi, huong, 9f, a)));
        yield return new WaitForFixedUpdate();

        toi.mana = toi.maxMana;
        int ve0 = GiatSet.SoTiaDaVe;
        toi.CastAt(6, ds[1].transform.position);

        // Doi tia dau tien hien ra
        float hanG = Time.time + 1.5f;
        LightningArc tia = null;
        while (tia == null && Time.time < hanG)
        {
            yield return null;
            // Dau o giua hai tay VA cuoi o mot bia - tia chop nho quanh tay luc niem (CastCrackle) cung la
            // LightningArc, lan chay dau bat nham no.
            foreach (var a in TiaCua(hh.DiemGiatSet, 0.6f))
            {
                if (Vector3.Distance(a.start, hh.DiemGiatSet.position) > 0.6f) continue;
                bool denBia = false;
                foreach (var b in ds) if (Vector3.Distance(a.end, b.transform.position + Vector3.up) < 1.2f) denBia = true;
                if (denBia) { tia = a; break; }
            }
        }
        if (tia == null) { Ghi("[LOI] G. khong thay tia nao moc tu tay"); loi++; DonBia(); yield break; }
        float lucHien = Time.time;
        // Doc SAU khi tia da dung hinh: luc vua tao, Start chua chay nen chua co luoi / vat lieu con
        yield return new WaitForSeconds(0.08f);
        Vector3 castPointCu = toi.rig != null && toi.rig.castPoint != null ? toi.rig.castPoint.position : toi.transform.position;

        // --- G1 ---
        string anhLoi = tia != null ? TenAnh(tia, "Core") : "(tia da mat)", anhQuang = tia != null ? TenAnh(tia, "Glow") : "(tia da mat)";
        bool laAnh = tia != null && tia.anhBlender;
        float song = tia != null ? tia.lifetime : -1f;
        var doiChung = LightningArc.Create(Vector3.up * 700f, Vector3.up * 700f + Vector3.right * 5f, 1f, 0.26f);
        yield return null;
        string anhDoiChung = TenAnh(doiChung, "Core");
        bool dcAnh = doiChung.anhBlender; float dcSong = doiChung.lifetime;

        // Doi toi 0,22 s sau khi hien (dang giu tay) roi do tay + dau tia
        while (Time.time < lucHien + 0.22f) yield return null;
        float day = hh.MucDayTay;
        float chiTruocPhai = Vector3.Dot((hh.banTayPhai.position - hh.tayPhaiTren.position).normalized, f);
        float chiTruocTrai = Vector3.Dot((hh.banTayTrai.position - hh.tayTraiTren.position).normalized, f);
        Vector3 giua = (hh.banTayTrai.position + hh.banTayPhai.position) * 0.5f;
        float tayTruocNguc = Vector3.Dot(giua - toi.transform.position, f);
        float haiTayCach = Vector3.Distance(hh.banTayTrai.position, hh.banTayPhai.position);
        float dauLechGiua = tia != null ? Vector3.Distance(tia.start, hh.DiemGiatSet.position) : 99f;
        float cuLechGiua = Vector3.Distance(castPointCu, hh.DiemGiatSet.position);

        // Do sang o giay 0,30 - TRUOC khi chup anh (moi lan chup mat vai khung, lan truoc do tre toi giay 0,54)
        while (Time.time < lucHien + 0.30f) yield return null;
        float tuoiDo = Time.time - lucHien;
        float alpha030 = tia != null ? DoAlpha(tia, "Core") : -2f;     // -2 = tia da bi xoa
        yield return Chup("giatset_3_hai_tay");

        // Anh can canh: dua may quay game lai gan (tat CameraRig), roi tra lai
        var cam = Camera.main;
        var rig = cam != null ? cam.GetComponentInParent<CameraRig>() : null;
        Vector3 camP = cam != null ? cam.transform.position : Vector3.zero;
        Quaternion camQ = cam != null ? cam.transform.rotation : Quaternion.identity;
        if (rig != null) rig.enabled = false;
        if (cam != null)
        {
            // Nhin cheo tu tren xuong nhu anh mau; chon ben nao khong co vat can giua may quay va tia
            Vector3 r = toi.transform.right;
            Vector3 tam = chanToi + huong * 4.5f + Vector3.up * 1.0f;
            Vector3 cho = tam - huong * 1.0f + r * 6.5f + Vector3.up * 6.5f;
            if (Physics.Linecast(cho, tam, VfxFactory.LopChuongNgai, QueryTriggerInteraction.Ignore))
                cho = tam - huong * 1.0f - r * 6.5f + Vector3.up * 6.5f;
            cam.transform.position = cho;
            cam.transform.LookAt(tam);
        }
        yield return null;
        Vector3 aCan = hh.DiemGiatSet.position;
        yield return Chup("giatset_4_can_canh");
        {
            float catTrang, catXanh;
            CatNgang("PlayTestShots/giatset_4_can_canh.png", cam, aCan, ds, out catTrang, out catXanh);
            Ghi(string.Format("G0. cat ngang tia o anh can canh: phan TRANG rong {0:F1} diem anh, phan XANH rong {1:F1} diem anh (trung binh 15 lat cat)",
                catTrang, catXanh));
        }

        Ghi(string.Format("G1. tia tu tay: anh loi '{0}', anh quang '{1}', anhBlender {2}, song {3:F2} giay, do sang o giay {4:F2} = {5:F2} (tia duy tri: con nguyen toi 65% doi = 0,39 s); doi chung (tia mac dinh): anhBlender {6}, anh '{7}', song {8:F2}",
            anhLoi, anhQuang, laAnh, song, tuoiDo, alpha030, dcAnh, anhDoiChung, dcSong));
        Kiem(anhLoi == "GiatSetLoi" && anhQuang == "GiatSetQuang", "tia Giut set khong dung anh ve tu Blender");
        Kiem(Mathf.Abs(song - GiatSet.GiayTiaHien) < 0.001f && Mathf.Abs(GiatSet.GiayTiaHien - 0.6f) < 0.001f, "tia khong song 0,6 giay");
        Kiem(tuoiDo < 0.38f && alpha030 > 0.98f, "o giay 0,30 tia da mo - khong phai tia duy tri");
        Kiem(!dcAnh && anhDoiChung != "GiatSetLoi", "doi chung: tia mac dinh cung dung anh - phep do khong phan biet duoc");
        if (doiChung != null) Object.Destroy(doiChung.gameObject);

        Ghi(string.Format("G2. tay: day {0:F2}; canh tay chi ra truoc (tich vo huong) phai {1:F2}, trai {2:F2} (mong > 0,85); giua hai tay truoc than {3:F2} m, hai tay cach nhau {4:F2} m; doi chung luc dung yen: phai {5:F2}, trai {6:F2}",
            day, chiTruocPhai, chiTruocTrai, tayTruocNguc, haiTayCach, chiTruocPhai0, chiTruocTrai0));
        Kiem(day > 0.99f, "chua o the day tay luc tia dang hien");
        Kiem(chiTruocPhai > 0.85f && chiTruocTrai > 0.85f, "CA HAI tay khong chi thang ra truoc");
        Kiem(tayTruocNguc > 0.45f, "hai ban tay khong o truoc than");
        Kiem(haiTayCach < 0.45f, "hai ban tay khong chum vao nhau");
        Kiem(chiTruocPhai0 < 0.6f && chiTruocTrai0 < 0.6f, "doi chung: luc dung yen tay da chi ra truoc - phep do khong phan biet duoc");

        Ghi(string.Format("G3. dau tia cach diem giua hai tay {0:F3} m (mong < 0,05); doi chung: diem phong phep cu (dau gay) cach {1:F2} m",
            dauLechGiua, cuLechGiua));
        Kiem(dauLechGiua < 0.05f, "tia khong moc tu giua hai tay");
        Kiem(cuLechGiua > 0.15f, "doi chung: diem phong phep cu trung diem giua hai tay - phep do khong phan biet duoc");

        // --- G4: bia chay, cuoi tia chay theo ---
        yield return new WaitForSeconds(0.5f);
        if (cam != null) { cam.transform.position = camP; cam.transform.rotation = camQ; }
        if (rig != null) rig.enabled = true;
        {
            var b = TaoBiaThay("TAM_G4", chanToi + huong * 30f + Vector3.up * 40f);
            yield return new WaitForFixedUpdate();
            Vector3 goc = chanToi + huong * 22f + Vector3.up * 41f;
            var gs = GiatSet.Phong(goc, huong, LayerMask.GetMask("Enemy"));
            gs.xacSuatChoang = 0f;
            LightningArc t4 = null;
            float h4 = Time.time + 1f;
            while (t4 == null && Time.time < h4)
            {
                yield return null;
                foreach (var a in TiaCua(b.transform, 1.5f)) { t4 = a; break; }
            }
            var dc4 = LightningArc.Create(goc, b.transform.position + Vector3.up, 1f, 0.6f);
            yield return null;
            Vector3 cuoi0 = t4 != null ? t4.end : Vector3.zero, dc0 = dc4.end;
            b.transform.position += Vector3.right * 1.5f;
            yield return null; yield return null;
            float diTheo = t4 != null ? Vector3.Distance(t4.end, cuoi0) : -1f;
            float dcDi = Vector3.Distance(dc4.end, dc0);
            Ghi(string.Format("G4. bia chay 1,5 m: cuoi tia chay theo {0:F2} m (mong 1,50); doi chung tia khong bam: {1:F2} m", diTheo, dcDi));
            Kiem(Mathf.Abs(diTheo - 1.5f) < 0.02f, "cuoi tia khong bam theo ke dich");
            Kiem(dcDi < 0.01f, "doi chung: tia khong bam cung di chuyen - phep do khong phan biet duoc");
            if (dc4 != null) Object.Destroy(dc4.gameObject);
        }

        // --- G5: dang giu tay ma buoc di thi bo tay ---
        {
            yield return new WaitForSeconds(0.5f);
            toi.mana = toi.maxMana;
            toi.transform.rotation = Quaternion.LookRotation(huong);
            // Anh can tu the hai tay, chup LUC DANG GIU TAY (lan truoc chup sau khi tia tat, tay da ha)
            if (rig != null) rig.enabled = false;
            if (cam != null)
            {
                Vector3 r = toi.transform.right;
                Vector3 nguc = toi.transform.position + Vector3.up * 1.25f;
                // Nhin NGANG tu mot ben, hoi lui sau: tia bay ra xa may quay, khong che hai tay
                cam.transform.position = nguc - huong * 0.9f + r * 3.4f + Vector3.up * 0.9f;
                cam.transform.LookAt(nguc + huong * 0.9f);
            }
            toi.CastAt(6, ds[1].transform.position);
            float h5 = Time.time + 1f;
            while (hh.MucDayTay < 0.99f && Time.time < h5) yield return null;
            yield return new WaitForSeconds(0.08f);
            yield return Chup("giatset_5_tu_the");
            if (cam != null) { cam.transform.position = camP; cam.transform.rotation = camQ; }
            if (rig != null) rig.enabled = true;
            yield return new WaitForSeconds(0.02f);
            float dayTruoc = hh.MucDayTay;
            float ep = hh.tocDoEp;
            hh.tocDoEp = 1f;                    // gia nhu dang di (cung duong cua ban sao mang)
            yield return null; yield return null;
            float daySau = hh.MucDayTay;
            hh.tocDoEp = ep;
            Ghi(string.Format("G5. dang giu tay (day {0:F2}) thi buoc di: day con {1:F2} (mong 0 - bo tay xuong, khong truot nhu tuong)", dayTruoc, daySau));
            Kiem(dayTruoc > 0.99f && daySau < 0.01f, "buoc di ma tay van giu tu the Giut set");
        }

        // --- G6: Giut set cua quai ---
        {
            yield return new WaitForSeconds(0.7f);
            var b = TaoBiaThay("TAM_G6", chanToi + huong * 30f + Vector3.up * 40f);
            yield return new WaitForFixedUpdate();
            var xanhLa = new Color(0.4f, 1f, 0.35f, 1f);
            GiatSet.PhongCuaQuai(chanToi + huong * 22f + Vector3.up * 41f, huong, LayerMask.GetMask("Enemy"), 14f, 14f, Color.white, xanhLa);
            LightningArc t6 = null;
            float h6 = Time.time + 1f;
            while (t6 == null && Time.time < h6)
            {
                yield return null;
                foreach (var a in TiaCua(b.transform, 1.5f)) { t6 = a; break; }
            }
            Ghi(string.Format("G6. Giut set cua quai: anh '{0}', mau quang ({1})", t6 != null ? TenAnh(t6, "Glow") : "(khong co tia)", t6 != null ? t6.glowColor.ToString("F2") : "-"));
            Kiem(t6 != null && TenAnh(t6, "Glow") == "GiatSetQuang" && t6.glowColor == xanhLa, "Giut set cua quai khong dung anh moi / mat mau xanh la");
        }

        Ghi(string.Format("   so tia da ve trong muc G: {0}", GiatSet.SoTiaDaVe - ve0));
        DonBia();
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
