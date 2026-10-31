using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 151 - GIUT SET TRUNG CHI CON KHOI + CHU TRANG THAI KIEU KHAC LUA (01/11/2026, nguoi dung khoanh cac vet sang trang khi Giut set
/// trung: "bo di, chi cho doi thu bi boc khoi" - chon giu khoi + tan lua + vet chay, ap ca Quy cay + Qua cau dien; va "chu hieu ung
/// CHOÁNG, BỊ CUỐN... qua don gian, size to qua" - chon kieu khac lua nhu ten tren dau, co ~55%).
///
/// Vao Play Act2, tat GameDirector, quai that (AI tat, mau 100 000, khong do don):
///   A. Giut set THAT (CastAt 6) vao 4 Bo xuong: so lan phun tia trung don (VfxFactory.SoLanTiaTrungDon) TANG 0, quai mat mau, co dam khoi
///      "SetChayDen"; DOI CHUNG: TakeDamage set thuong cung con -> tia trung don tang 1 (bo dem con song).
///   B. Qua cau dien THAT (CastAt 13): tia trung don tang 0, moi "ChamTiaDien" khong con lop "Toe" nhung van co chop "Loe".
///   C. Tia set cua Quy cay (GiatSet.PhongCuaQuai) vao nguoi choi: tia trung don tang 0, nguoi choi mat mau.
///   D. Font Playfair (doc thang cmap file .ttf) du moi ky tu cua 6 chu trang thai; ve bang GiaoDien.ChuTen.
///   E. Anh chup man Game: cung chu "ĐÓNG BĂNG!" / "CHOÁNG!" ve kieu cu (SpawnText) va kieu moi (SpawnTrangThai) cung cho, cung tuoi
///      0,35 s - do khung bao cac diem anh SANG LEN so voi anh nen (khong popup): cao moi / cao cu trong 0,40-0,75.
/// PlayTestShots/chu_trang_thai/*.png + nhatky.txt.
/// </summary>
public static class ThuChuTrangThai
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/chu_trang_thai/";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;
    static readonly string[] ChuTrangThai = { "CHOÁNG!", "BỊ CUỐN!", "ĐÓNG BĂNG!", "HẤT TUNG!", "BỊ ƯỚT", "ĐỠ ĐÒN!" };

    [MenuItem("Diablo 2.5D/151 Giut set chi con khoi + chu trang thai kieu khac lua", false, 225)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuChuTrangThai] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        nk.Length = 0; loi = 0;
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
        var go = new GameObject("TAM_ThuChuTrangThai");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static PlayerController toi;
    static readonly List<Damageable> quai = new List<Damageable>();

    static Damageable SinhQuai(Vector3 p, Vector3 nhinVe)
    {
        p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p));
        var q = EnemyFactory.Spawn(MonsterType.Skeleton, p, null, toi.transform);
        q.name = "TAM_Quai" + quai.Count;
        var ai = q.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
        var d = q.GetComponent<Damageable>(); d.maxHealth = 100000f; d.health = 100000f; d.tiLeDoDon = 0f;
        Vector3 h = nhinVe - p; h.y = 0; if (h.sqrMagnitude > 0.01f) q.transform.rotation = Quaternion.LookRotation(h);
        quai.Add(d);
        return d;
    }

    static float TongMat()
    {
        float t = 0f;
        foreach (var d in quai) if (d != null) t += d.maxHealth - d.health;
        return t;
    }

    static void XoaPopup()
    {
        foreach (var p in Object.FindObjectsByType<DamagePopup>(FindObjectsInactive.Exclude)) Object.Destroy(p.gameObject);
    }

    static Texture2D anhNen;

    static IEnumerator ChupMan(System.Action<Texture2D> nhan)
    {
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    /// <summary>
    /// Khung bao cac diem anh SANG LEN > 0,18 so voi anh nen VA gan MAU CHU (khoang cach RGB < 0,35), trong cua so quanh diem man hinh
    /// (x, y tu duoi). Loc mau vi lua lo chap chon cung sang len (lan 2 do "chu cu" ra 254 x 90 px).
    /// </summary>
    static Vector2 KhungSang(Texture2D tx, Vector2 tam, int nua, Color mauChu)
    {
        int x0 = int.MaxValue, x1 = -1, y0 = int.MaxValue, y1 = -1;
        for (int y = Mathf.Max(0, (int)tam.y - nua); y < Mathf.Min(tx.height, (int)tam.y + nua); y++)
            for (int x = Mathf.Max(0, (int)tam.x - nua * 2); x < Mathf.Min(tx.width, (int)tam.x + nua * 2); x++)
            {
                Color a = tx.GetPixel(x, y), b = anhNen.GetPixel(x, y);
                float la = 0.299f * a.r + 0.587f * a.g + 0.114f * a.b, lb = 0.299f * b.r + 0.587f * b.g + 0.114f * b.b;
                if (la - lb < 0.18f) continue;
                if (Mathf.Abs(a.r - mauChu.r) + Mathf.Abs(a.g - mauChu.g) + Mathf.Abs(a.b - mauChu.b) > 0.35f * 3f) continue;
                if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y;
            }
        return x1 < 0 ? Vector2.zero : new Vector2(x1 - x0 + 1, y1 - y0 + 1);
    }

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        TamNhin.BatTrongPhepThu = true;
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var rig = Camera.main.GetComponent<CameraRig>();
        if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
        var hud = Object.FindAnyObjectByType<GameHUD>();
        if (hud != null) hud.epCamUng = true;
        foreach (int ky in new[] { 6, CapDo.KyCauDien }) CapDo.MoCaDuongChoPhepThu(ky);
        yield return new WaitForSeconds(1.2f);
        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        Vector3 P = toi.transform.position;
        toi.health.maxHealth = 100000f; toi.health.health = 100000f;

        // ---- A. Giut set that ----
        nk.AppendLine("== A. Giut set");
        foreach (var g in new[] { 0f, 18f, -18f, 0f })
        {
            var h = Quaternion.AngleAxis(g, Vector3.up) * F;
            SinhQuai(P + h * (quai.Count == 3 ? 9f : 6f), P);
        }
        yield return new WaitForSeconds(0.5f);
        int truoc = VfxFactory.SoLanTiaTrungDon; float matTruoc = TongMat();
        int khoi = 0; var daThay = new HashSet<GameObject>();
        toi.mana = toi.maxMana; toi.CastAt(6, P + F * 6f);
        for (int i = 0; i < 30; i++)
        {
            yield return new WaitForSeconds(0.05f);
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                if (t.name == "SetChayDen" && daThay.Add(t.gameObject)) khoi++;
        }
        int tiaGS = VfxFactory.SoLanTiaTrungDon - truoc; float matGS = TongMat() - matTruoc;
        Kiem(matGS > 0f, "A Giut set trung: quai mat " + matGS.ToString("F0") + " mau");
        Kiem(tiaGS == 0, "A Giut set: tia trung don phun them " + tiaGS + " lan (phai 0)");
        Kiem(khoi > 0, "A Giut set: van boc khoi - " + khoi + " dam SetChayDen (khoi + tan lua + vet chay)");
        truoc = VfxFactory.SoLanTiaTrungDon;
        quai[0].TakeDamage(10f, DamageType.Lightning, quai[0].transform.position + Vector3.up);
        Kiem(VfxFactory.SoLanTiaTrungDon - truoc == 1, "A doi chung: sat thuong set thuong van phun tia trung don (+" + (VfxFactory.SoLanTiaTrungDon - truoc) + ")");
        Kiem(!Damageable.BoQuaTiaTrungDon, "A co BoQuaTiaTrungDon tra ve false sau don");
        yield return new WaitForSeconds(1f);

        // ---- B. Qua cau dien that ----
        nk.AppendLine("== B. Qua cau dien");
        truoc = VfxFactory.SoLanTiaTrungDon; matTruoc = TongMat();
        int coToe = 0, coLoe = 0; var cham = new HashSet<GameObject>();
        toi.mana = toi.maxMana; toi.CastAt(CapDo.KyCauDien, quai[0].transform.position);
        for (int i = 0; i < 60; i++)
        {
            yield return new WaitForSeconds(0.1f);
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                if (t.name == "ChamTiaDien" && cham.Add(t.gameObject))
                {
                    if (t.Find("Toe") != null) coToe++;
                    if (t.Find("Loe") != null) coLoe++;
                }
        }
        int tiaCD = VfxFactory.SoLanTiaTrungDon - truoc; float matCD = TongMat() - matTruoc;
        nk.AppendLine("   so tia da ban " + QuaCauDien.SoTiaDaBan + ", cho cham " + cham.Count);
        Kiem(matCD > 0f && cham.Count > 0, "B Qua cau dien trung: quai mat " + matCD.ToString("F0") + " mau, " + cham.Count + " cho cham");
        Kiem(tiaCD == 0, "B Qua cau dien: tia trung don phun them " + tiaCD + " lan (phai 0)");
        Kiem(coToe == 0 && coLoe == cham.Count, "B cho cham: " + coToe + " co lop Toe (phai 0), " + coLoe + "/" + cham.Count + " co chop Loe");
        foreach (var qc in Object.FindObjectsByType<QuaCauDien>(FindObjectsInactive.Exclude)) Object.Destroy(qc.gameObject);
        yield return new WaitForSeconds(0.5f);

        // ---- C. Tia set Quy cay vao nguoi choi ----
        nk.AppendLine("== C. Quy cay");
        truoc = VfxFactory.SoLanTiaTrungDon;
        float mauToi = toi.health.health;
        Vector3 goc = quai[1].transform.position + Vector3.up * 1.4f;
        Vector3 dich = toi.transform.position + Vector3.up * 1.0f;
        GiatSet.PhongCuaQuai(goc, (dich - goc).normalized, LayerMask.GetMask("Player"), 30f, 14f, Color.white, new Color(0.3f, 1f, 0.3f));
        yield return new WaitForSeconds(1.2f);
        int tiaQC = VfxFactory.SoLanTiaTrungDon - truoc;
        Kiem(toi.health.health < mauToi, "C tia Quy cay trung nguoi choi: mat " + (mauToi - toi.health.health).ToString("F0") + " mau");
        Kiem(tiaQC == 0, "C Quy cay: tia trung don phun them " + tiaQC + " lan (phai 0)");
        foreach (var d in quai) if (d != null) Object.Destroy(d.gameObject);
        quai.Clear();
        yield return new WaitForSeconds(3.5f);   // cho khoi tan het

        // ---- D. Font ----
        nk.AppendLine("== D. Font");
        var cmap = BangKyTuFont.Doc("Assets/Resources/Fonts/PlayfairDisplaySC-Black.ttf");
        Kiem(cmap != null, "D doc duoc cmap PlayfairDisplaySC-Black.ttf");
        if (cmap != null)
        {
            var thieu = new StringBuilder();
            foreach (var s in ChuTrangThai) foreach (char ch in s) if (ch != ' ' && !cmap.Contains(ch)) thieu.Append(ch);
            Kiem(thieu.Length == 0, "D du ky tu cho 6 chu trang thai" + (thieu.Length > 0 ? " - thieu: " + thieu : ""));
        }

        // ---- E. Anh chup: cu vs moi ----
        nk.AppendLine("== E. Co chu tren anh");
        XoaPopup();
        var cam = Camera.main;
        Vector3 W = toi.transform.position + F * 3f + Vector3.up * 1.6f;
        yield return new WaitForSeconds(0.3f);
        Vector3 s3 = cam.WorldToScreenPoint(W);
        Vector2 tam = new Vector2(s3.x, s3.y);
        int nua = Mathf.RoundToInt(Screen.height * 0.18f);
        var mauChu = new[] { FrozenEffect.MauChuDongBang, StunnedEffect.MauChuChoang };
        string[] chu = { "ĐÓNG BĂNG!", "CHOÁNG!" };
        for (int i = 0; i < chu.Length; i++)
        {
            Vector2 kCu = Vector2.zero, kMoi = Vector2.zero;
            for (int kieu = 0; kieu < 2; kieu++)
            {
                XoaPopup();
                yield return new WaitForSeconds(0.1f);
                if (anhNen != null) Object.Destroy(anhNen);
                yield return ChupMan(t => anhNen = t);       // nen chup NGAY TRUOC moi lan (lua lo chap chon)
                var p = kieu == 0 ? DamagePopup.SpawnText(W, chu[i], mauChu[i]) : DamagePopup.SpawnTrangThai(W, chu[i], mauChu[i]);
                p.transform.position = W; p.drift = Vector3.zero;
                yield return new WaitForSeconds(0.35f);
                Texture2D tx = null;
                yield return ChupMan(t => tx = t);
                File.WriteAllBytes(Thu + "chu_" + i + (kieu == 0 ? "_cu" : "_moi") + ".png", tx.EncodeToPNG());
                // mau chu tren man: kieu cu = mau goc, kieu moi = sang 25% ve trang
                var k = KhungSang(tx, tam, nua, kieu == 0 ? mauChu[i] : Color.Lerp(mauChu[i], Color.white, 0.25f));
                if (kieu == 0) kCu = k; else kMoi = k;
                Object.Destroy(tx);
                if (kieu == 1) Kiem(DamagePopup.FontTrangThaiCuoi == GiaoDien.ChuTen, "E \"" + chu[i] + "\" ve bang GiaoDien.ChuTen (" + (DamagePopup.FontTrangThaiCuoi != null ? DamagePopup.FontTrangThaiCuoi.name : "null") + ")");
            }
            float tl = kCu.y > 0 ? kMoi.y / kCu.y : 0f;
            nk.AppendLine("   \"" + chu[i] + "\": cu " + kCu.x + "x" + kCu.y + " px, moi " + kMoi.x + "x" + kMoi.y + " px");
            Kiem(tl > 0.40f && tl < 0.75f, "E \"" + chu[i] + "\" cao moi / cu = " + tl.ToString("F2") + " (0,40-0,75; mong ~0,55 + gach chan)");
        }
        XoaPopup();
        // Anh trinh bay: 3 Bo xuong hang ngang 2,5 m truoc mat, moi con 2 chu (dau 2,1 m + 2,7 m) - kieu cu roi kieu moi, cung bo cuc
        Vector3 goc0 = toi.transform.position + F * 2.5f;
        for (int i = 0; i < 3; i++) SinhQuai(goc0 + R * (i - 1) * 2.4f, toi.transform.position);
        yield return new WaitForSeconds(0.3f);
        var mauTT = new[] { StunnedEffect.MauChuChoang, new Color(0.85f, 0.82f, 0.70f), FrozenEffect.MauChuDongBang,
                            new Color(0.86f, 0.66f, 0.40f), BiUot.MauChuUot, ChongDo.MauChu };
        for (int kieu = 0; kieu < 2; kieu++)
        {
            XoaPopup();
            yield return null;
            for (int i = 0; i < ChuTrangThai.Length; i++)
            {
                Vector3 w = quai[i % 3].transform.position + Vector3.up * (2.1f + (i / 3) * 0.6f);
                var p = kieu == 0 ? DamagePopup.SpawnText(w, ChuTrangThai[i], mauTT[i]) : DamagePopup.SpawnTrangThai(w, ChuTrangThai[i], mauTT[i]);
                p.transform.position = w; p.drift = Vector3.zero;
            }
            yield return new WaitForSeconds(0.35f);
            string ten = kieu == 0 ? "trinhbay_cu" : "trinhbay_moi";
            yield return ChupMan(t => { File.WriteAllBytes(Thu + ten + ".png", t.EncodeToPNG()); Object.Destroy(t); });
        }
        Object.Destroy(anhNen);
        foreach (var d in quai) if (d != null) Object.Destroy(d.gameObject);
        quai.Clear();
        XoaPopup();

        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        nk.Insert(0, "MENU 151 - Giut set chi con khoi + chu trang thai: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuChuTrangThai] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuChuTrangThai");
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
        Debug.Log("[ThuChuTrangThai] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
