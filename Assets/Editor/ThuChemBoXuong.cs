using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 152 - DON CHEM CUA BO XUONG (01/11/2026, nguoi dung khoanh cac VET DO keo dai khi Bo xuong chem trung: "xoa hieu ung nay", roi
/// "dung lai toan hieu ung chem... cho thay ro hieu ung chem, moi lan chem vao doi thu co hieu ung danh trung va doi thu bi chay mau").
/// Nguoi dung chon: vet chem do mau sam, chay mau chi hinh anh, vet mau nho tren dat tan sau vai giay, trung Khien -> tia lua kim loai.
///
/// Vao Play Act2, tat GameDirector, Bo xuong THAT (EnemyFactory.Spawn), nguoi choi mau 100 000:
///   A. AI BAT, Bo xuong tu chem nguoi choi 3 giay: dem don trung (su kien DaRaDon) - tia trung don (HitBurst, chinh vet do) TANG 0,
///      moi don mot vet chem, mot hinh trung; nguoi choi mat dung so mau; co ChayMau, co vet mau tren dat (<= tran); giot mau KHONG keo
///      dai (Billboard). DOI CHUNG: TakeDamage Vat ly thuong -> tia trung don +1.
///   B. Vet chem bam LUOI KIEM THAT: buoc khung 1/400 giay, doc mui kiem that (xuong RightHand + MuiKiemTrongTay) moi khung trong cu chem,
///      khoang cach toi duong mui cua luoi vet <= 0,12 m; DOI CHUNG duong vet lat guong (x -> -x) >= 0,30 m; duong mui dai >= 1 m.
///   C. Khien bat: don chem -> tia lua khien, KHONG mau (ChayMau.SoLanBat khong tang), khien mat mau.
///   D. Duong mang (may khach): DienLaiDon(0) nham nguoi choi -> mat mau + hinh trung hen toi luc luoi kiem bo xuong; nham nguoi khac
///      (nanNhan null) -> chi hinh, khong mat mau. Tia trung don van 0.
///   E. Anh: may quay game 2.5D (dem + ngay) va may quay can, cung cu chem kieu CU (HitBurst do, khong vet) va MOI, 4 moc thoi gian;
///      dem diem anh "do mau" quanh hai nhan vat: moi / cu.
/// PlayTestShots/chem_bo_xuong/*.png + nhatky.txt.
/// </summary>
public static class ThuChemBoXuong
{
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/chem_bo_xuong/";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;

    [MenuItem("Diablo 2.5D/152 Don chem Bo xuong (vet chem, mau, chay mau, khien)", false, 226)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuChemBoXuong] dang Play hoac canh chua luu"); return; }
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
        var go = new GameObject("TAM_ThuChemBoXuong");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static PlayerController toi;

    static EnemyAI SinhBoXuong(Vector3 p, Vector3 nhinVe, bool batAI)
    {
        p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p));
        var q = EnemyFactory.Spawn(MonsterType.Skeleton, p, null, toi.transform);
        q.name = "TAM_BoXuong";
        var ai = q.GetComponent<EnemyAI>();
        ai.enabled = batAI;
        var d = q.GetComponent<Damageable>(); d.maxHealth = 100000f; d.health = 100000f; d.tiLeDoDon = 0f;
        Vector3 h = nhinVe - p; h.y = 0; if (h.sqrMagnitude > 0.01f) q.transform.rotation = Quaternion.LookRotation(h);
        return ai;
    }

    static void DonHinh()
    {
        foreach (var n in new[] { "VetChemBoXuong", "MauPhunBoXuong", "VetCatBoXuong", "TiaLuaKhienBoXuong", "VetMauDat", "HenChemTrung", "Hit" })
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
                if (t != null && t.name == n) Object.Destroy(t.gameObject);
        foreach (var c in Object.FindObjectsByType<ChayMau>(FindObjectsInactive.Exclude)) Object.Destroy(c);
        foreach (var p in Object.FindObjectsByType<DamagePopup>(FindObjectsInactive.Exclude)) Object.Destroy(p.gameObject);
    }

    static int DemTen(string ten)
    {
        int n = 0;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude)) if (t.name == ten) n++;
        return n;
    }

    static Transform TimXuong(Transform goc, string ten)
    {
        foreach (var t in goc.GetComponentsInChildren<Transform>(true)) if (t.name == ten) return t;
        return null;
    }

    static float KcToiDuong(Vector3 p, List<Vector3> duong)
    {
        float best = float.MaxValue;
        for (int i = 0; i + 1 < duong.Count; i++)
        {
            Vector3 a = duong[i], b = duong[i + 1], ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
            best = Mathf.Min(best, Vector3.Distance(p, a + ab * t));
        }
        return best;
    }

    static IEnumerator ChupMan(System.Action<Texture2D> nhan)
    {
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    static Texture2D ChupCan(Camera cc)
    {
        var rt = RenderTexture.GetTemporary(640, 480, 24);
        cc.targetTexture = rt; cc.Render(); cc.targetTexture = null;
        RenderTexture.active = rt;
        var tx = new Texture2D(640, 480, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 640, 480), 0, 0); tx.Apply();
        RenderTexture.active = null; RenderTexture.ReleaseTemporary(rt);
        return tx;
    }

    /// <summary>So diem anh "do mau" (do troi, sang vua) trong khung quanh tam (toa do man hinh, y tu duoi).</summary>
    static int DemDo(Texture2D tx, Vector2 tam, int nua)
    {
        int n = 0;
        for (int y = Mathf.Max(0, (int)tam.y - nua); y < Mathf.Min(tx.height, (int)tam.y + nua); y++)
            for (int x = Mathf.Max(0, (int)tam.x - nua); x < Mathf.Min(tx.width, (int)tam.x + nua); x++)
            {
                Color c = tx.GetPixel(x, y);
                if (c.r > 0.16f && c.r > c.g * 2.2f && c.r > c.b * 2.2f) n++;
            }
        return n;
    }

    static Texture2D Ghep(List<Texture2D> anh, int cot)
    {
        int w = anh[0].width, h = anh[0].height, hang = (anh.Count + cot - 1) / cot;
        var o = new Texture2D(w * cot, h * hang, TextureFormat.RGB24, false);
        for (int i = 0; i < anh.Count; i++)
        {
            int cx = i % cot, cy = hang - 1 - i / cot;
            o.SetPixels(cx * w, cy * h, w, h, anh[i].GetPixels());
        }
        o.Apply();
        return o;
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
        yield return new WaitForSeconds(1.0f);
        Vector3 F = rig != null ? rig.FlatForward : Vector3.forward;
        Vector3 R = Vector3.Cross(Vector3.up, F).normalized;
        Vector3 P = toi.transform.position;
        toi.health.maxHealth = 100000f; toi.health.health = 100000f;
        toi.enabled = false;   // dung yen, khong tung phep

        // ---- A. AI that chem nguoi choi ----
        nk.AppendLine("== A. Bo xuong that chem 3 giay");
        var ai = SinhBoXuong(P + F * 1.8f, P, true);
        int don = 0;
        ai.DaRaDon += (a, k, t, p) => { if (k == 0) don++; };
        yield return new WaitForSeconds(0.2f);
        don = 0;
        int hit0 = VfxFactory.SoLanTiaTrungDon, vet0 = VetChemKiem.SoVetDaTao, trung0 = VfxFactory.SoLanChemTrung, bat0 = ChayMau.SoLanBat;
        float mau0 = toi.health.health;
        int maxVetDat = 0, maxGiot = 0, keoDai = 0; bool coChayMau = false;
        for (float t = 0f; t < 3f; t += Time.deltaTime)
        {
            yield return null;
            maxVetDat = Mathf.Max(maxVetDat, VetMauDat.SoDangCo);
            if (toi.GetComponent<ChayMau>() != null) coChayMau = true;
            int g = 0;
            foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude))
                if (ps.name == "GiotMau" || ps.name == "MauNhoGiot")
                {
                    g += ps.particleCount;
                    if (ps.GetComponent<ParticleSystemRenderer>().renderMode == ParticleSystemRenderMode.Stretch) keoDai++;
                }
            maxGiot = Mathf.Max(maxGiot, g);
        }
        ai.enabled = false;
        int hitA = VfxFactory.SoLanTiaTrungDon - hit0, vetA = VetChemKiem.SoVetDaTao - vet0, trungA = VfxFactory.SoLanChemTrung - trung0;
        float matA = mau0 - toi.health.health;
        nk.AppendLine("   don trung " + don + ", vet chem " + vetA + ", hinh trung " + trungA + ", mat " + matA.ToString("F1") + " mau (moi don " + ai.attackDamage.ToString("F2") + ")");
        Kiem(don >= 6, "A Bo xuong chem trung " + don + " don trong 3 giay (nhip 0,3 s)");
        Kiem(hitA == 0, "A tia trung don (vet do keo dai) phun them " + hitA + " lan (phai 0)");
        Kiem(vetA >= don && vetA <= don + 2, "A moi cu chem mot vet: " + vetA + " vet / " + don + " don");
        Kiem(trungA == don, "A moi don trung mot hinh trung: " + trungA + " / " + don);
        Kiem(Mathf.Abs(matA - don * ai.attackDamage) < 0.5f, "A sat thuong giu nguyen: mat " + matA.ToString("F1") + " = " + don + " x " + ai.attackDamage.ToString("F2"));
        Kiem(ChayMau.SoLanBat - bat0 == don && coChayMau, "A chay mau bat " + (ChayMau.SoLanBat - bat0) + " lan, co component ChayMau tren nguoi choi");
        Kiem(maxVetDat > 0 && maxVetDat <= VetMauDat.ToiDa, "A vet mau tren dat: nhieu nhat " + maxVetDat + " (tran " + VetMauDat.ToiDa + ")");
        Kiem(maxGiot > 0 && keoDai == 0, "A giot mau bay: nhieu nhat " + maxGiot + " hat, he hat keo dai " + keoDai + " (phai 0)");
        int hc = VfxFactory.SoLanTiaTrungDon;
        toi.health.TakeDamage(5f, DamageType.Physical, toi.transform.position + Vector3.up);
        Kiem(VfxFactory.SoLanTiaTrungDon - hc == 1, "A doi chung: sat thuong Vat ly thuong van phun tia trung don (+" + (VfxFactory.SoLanTiaTrungDon - hc) + ")");
        Kiem(!Damageable.BoQuaTiaTrungDon, "A co BoQuaTiaTrungDon tra ve false");
        // het chay mau: 2 giay sau don cuoi component tu go
        yield return new WaitForSeconds(ChayMau.GiayChayMau + 1.2f);
        Kiem(toi.GetComponent<ChayMau>() == null, "A het " + ChayMau.GiayChayMau + " giay chay mau thi tu go");
        yield return new WaitForSeconds(VetMauDat.GiayGiu + VetMauDat.GiayMo);
        Kiem(VetMauDat.SoDangCo == 0, "A vet mau tren dat tan het sau " + (VetMauDat.GiayGiu + VetMauDat.GiayMo) + " giay (con " + VetMauDat.SoDangCo + ")");

        // ---- B. Vet bam luoi kiem that ----
        nk.AppendLine("== B. Vet chem bam luoi kiem that");
        DonHinh();
        var hh = ai.GetComponent<ModelHoatHinh>();
        var tay = TimXuong(ai.transform, "RightHand");
        yield return new WaitForSeconds(0.5f);
        Time.captureDeltaTime = 1f / 400f;
        hh.RaDon(ai.attackAnimTime);
        VetChemKiem vet = null;
        var muiThat = new List<Vector3>();
        float giayQuet = (1f - hh.mocAn) * ModelHoatHinh.CuoiCungChem * ai.attackAnimTime;
        float batDau = -1f;
        for (int i = 0; i < 400; i++)
        {
            yield return new WaitForEndOfFrame();
            if (vet == null) vet = Object.FindAnyObjectByType<VetChemKiem>();
            if (vet == null) continue;
            if (batDau < 0f) batDau = Time.time;
            muiThat.Add(tay.TransformPoint(ModelHoatHinh.MuiKiemTrongTay));
            if (Time.time - batDau > giayQuet) break;
        }
        Time.captureDeltaTime = 0f;
        var duongVet = new List<Vector3>(); var duongGuong = new List<Vector3>();
        float daiVet = 0f;
        if (vet != null)
        {
            var mf = vet.GetComponent<MeshFilter>().sharedMesh.vertices;
            for (int i = 0; i < mf.Length / 3; i++)
            {
                Vector3 can = mf[i * 3], xa = mf[i * 3 + 2];
                Vector3 mui = can + (xa - can) / 1.08f;
                duongVet.Add(vet.transform.TransformPoint(mui));
                duongGuong.Add(vet.transform.TransformPoint(new Vector3(-mui.x, mui.y, mui.z)));
                if (i > 0) daiVet += Vector3.Distance(duongVet[i], duongVet[i - 1]);
            }
        }
        float lechMax = 0f, lechGuong = 0f;
        foreach (var p in muiThat) { lechMax = Mathf.Max(lechMax, KcToiDuong(p, duongVet)); lechGuong = Mathf.Max(lechGuong, KcToiDuong(p, duongGuong)); }
        nk.AppendLine("   mui kiem that: " + muiThat.Count + " mau trong " + giayQuet.ToString("F3") + " s; duong mui vet dai " + daiVet.ToString("F2") + " m");
        Kiem(vet != null && muiThat.Count >= 10, "B co vet chem + " + muiThat.Count + " mau mui kiem that");
        Kiem(lechMax <= 0.12f, "B mui kiem that cach duong mui cua vet <= " + lechMax.ToString("F3") + " m (phai <= 0,12)");
        Kiem(lechGuong >= 0.30f, "B doi chung vet lat guong: lech " + lechGuong.ToString("F2") + " m (phai >= 0,30)");
        Kiem(daiVet >= 1.0f && daiVet <= 5f, "B duong mui vet dai " + daiVet.ToString("F2") + " m (1-5; ban cu quay gan tron vong 9,2 m)");
        // Nhat bo kiem DUNG TRUOC MAT: mui cuoi o phia truoc, khong mau nao ra sau lung qua 0,5 m (ban cu vong ra sau lung -1,36 m)
        if (duongVet.Count > 0)
        {
            float zCuoi = ai.transform.InverseTransformPoint(duongVet[duongVet.Count - 1]).z, zMin = float.MaxValue;
            foreach (var p in duongVet) zMin = Mathf.Min(zMin, ai.transform.InverseTransformPoint(p).z);
            Kiem(zCuoi > 0.3f && zMin > -0.5f, "B nhat chem dung truoc mat: mui cuoi z " + zCuoi.ToString("F2") + " m (> 0,3), sau lung nhat z " + zMin.ToString("F2") + " m (> -0,5)");
        }
        yield return new WaitForSeconds(0.5f);

        // ---- C. Khien ----
        nk.AppendLine("== C. Chem trung Khien");
        DonHinh();
        CapDo.MoCaDuongChoPhepThu(5);
        toi.enabled = true;
        toi.mana = toi.maxMana; toi.CastAt(5, toi.transform.position + F);
        yield return new WaitForSeconds(0.8f);
        toi.enabled = false;
        var kh = toi.health.khieng;
        Kiem(kh != null && kh.DangBat, "C khien da bat");
        // dat Bo xuong ra ngoai vom roi cho no tu chem
        if (kh != null) { var p = P + F * (kh.banKinh + 1.0f); p.y = GioLoc.MatDatY(p, VfxFactory.GroundY(p)); ai.transform.position = p; ai.transform.rotation = Quaternion.LookRotation(-F); }
        int kh0 = VfxFactory.SoLanChemTrungKhien, b0 = ChayMau.SoLanBat, h0 = VfxFactory.SoLanTiaTrungDon, tl = 0; don = 0;
        float mauKh0 = kh != null ? kh.mau : 0f, mauNg0 = toi.health.health;
        var thay = new HashSet<GameObject>();
        ai.enabled = true;
        for (float t = 0f; t < 1.5f; t += Time.deltaTime)
        {
            yield return null;
            foreach (var tr in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude)) if (tr.name == "TiaLuaKhienBoXuong" && thay.Add(tr.gameObject)) tl++;
            if (kh == null || !kh.DangBat) break;
        }
        ai.enabled = false;
        nk.AppendLine("   don " + don + ", tia lua " + tl + ", khien " + mauKh0.ToString("F0") + " -> " + (kh != null ? kh.mau.ToString("F0") : "-"));
        Kiem(don > 0 && VfxFactory.SoLanChemTrungKhien - kh0 == don && tl == don, "C moi don trung khien mot chum tia lua: " + tl + " / " + don);
        Kiem(ChayMau.SoLanBat - b0 == 0, "C trung khien KHONG chay mau (bat " + (ChayMau.SoLanBat - b0) + ")");
        Kiem(VfxFactory.SoLanTiaTrungDon - h0 == 0, "C tia trung don +" + (VfxFactory.SoLanTiaTrungDon - h0) + " (phai 0)");
        Kiem(kh != null && kh.mau < mauKh0 && toi.health.health == mauNg0, "C khien an don, nguoi choi khong mat mau");
        if (kh != null) kh.mau = 0f;
        yield return new WaitForSeconds(0.6f);

        // ---- D. Duong mang ----
        nk.AppendLine("== D. Duong mang (DienLaiDon)");
        DonHinh();
        if (toi.health.khieng != null) toi.health.khieng.mau = 0f;
        ai.transform.position = P + F * 1.8f; ai.transform.rotation = Quaternion.LookRotation(-F);
        yield return new WaitForSeconds(0.3f);
        int t0 = VfxFactory.SoLanChemTrung; h0 = VfxFactory.SoLanTiaTrungDon;
        float m0 = toi.health.health;
        ai.DienLaiDon(0, toi.transform.position, toi.health);
        Kiem(toi.health.health < m0 && VfxFactory.SoLanChemTrung == t0, "D nham nguoi choi: mat mau ngay, hinh trung CHUA hien (hen)");
        float tre = ai.attackAnimTime * ai.attackHitAt;
        yield return new WaitForSeconds(tre + 0.08f);
        Kiem(VfxFactory.SoLanChemTrung - t0 == 1, "D hinh trung hien sau " + tre.ToString("F3") + " s (luoi kiem bo xuong)");
        yield return new WaitForSeconds(0.3f);
        t0 = VfxFactory.SoLanChemTrung; m0 = toi.health.health;
        ai.DienLaiDon(0, toi.transform.position, null);
        yield return new WaitForSeconds(tre + 0.08f);
        Kiem(VfxFactory.SoLanChemTrung - t0 == 1 && toi.health.health == m0, "D nham nguoi khac (nanNhan null): chi hinh, khong mat mau");
        Kiem(VfxFactory.SoLanTiaTrungDon - h0 == 0, "D tia trung don +" + (VfxFactory.SoLanTiaTrungDon - h0) + " (phai 0)");
        yield return new WaitForSeconds(2.5f);

        // ---- E. Anh ----
        nk.AppendLine("== E. Anh cu / moi");
        DonHinh();
        foreach (var v in Object.FindObjectsByType<VetMauDat>(FindObjectsInactive.Exclude)) Object.Destroy(v.gameObject);
        var camCan = new GameObject("TAM_CamCan").AddComponent<Camera>();
        camCan.enabled = false; camCan.fieldOfView = 40f; camCan.nearClipPlane = 0.1f;
        Vector3 giua = (P + ai.transform.position) * 0.5f + Vector3.up * 1.1f;
        camCan.transform.position = giua + R * 3.2f + F * 0.6f + Vector3.up * 0.6f;
        camCan.transform.LookAt(giua);
        var sang = Object.FindAnyObjectByType<ChuyenChieuSangDem>();
        float[] moc = { 0.02f, 0.06f, 0.12f, 0.22f };
        var cam = Camera.main;
        string[] buoi = { "dem", "ngay" };
        for (int bu = 0; bu < 2; bu++)
        {
            if (sang != null) { sang.enabled = false; sang.ApGiay(bu == 0 ? ChuyenChieuSangDem.GiayGiuaDem : ChuyenChieuSangDem.GiayGiuaNgay); }
            yield return new WaitForSeconds(0.3f);
            Vector3 s3 = cam.WorldToScreenPoint(giua);
            Vector2 tam = new Vector2(s3.x, s3.y);
            int nua = Mathf.RoundToInt(Screen.height * 0.16f);
            // Vung dem: quanh Bo xuong (cung kiem quet), KHONG quanh nguoi choi - than nguoi choi nhay do khi trung (co tu truoc, ca hai kieu)
            Vector3 sb = cam.WorldToScreenPoint(ai.transform.position + Vector3.up * 1.5f);
            Vector2 tamBX = new Vector2(sb.x, sb.y);
            int nuaBX = Mathf.RoundToInt(Screen.height * 0.07f);
            var anhGame = new List<Texture2D>(); var anhCan = new List<Texture2D>();
            int[,] dem = new int[2, moc.Length];
            for (int kieu = 0; kieu < 2; kieu++)
            {
                for (int m = 0; m < moc.Length; m++)
                {
                    DonHinh();
                    yield return new WaitForSeconds(0.25f);
                    ModelHoatHinh.TatVetChem = kieu == 0;
                    hh.RaDon(ai.attackAnimTime);
                    float tAn = ai.attackAnimTime * ai.attackHitAt;
                    yield return new WaitForSeconds(tAn);
                    if (kieu == 0) toi.health.TakeDamage(ai.attackDamage, DamageType.Physical, toi.transform.position + Vector3.up * 1.1f);  // duong CU
                    else
                    {
                        VfxFactory.ChemTrung(toi.health, ai.transform.position);
                        Damageable.BoQuaTiaTrungDon = true;
                        toi.health.TakeDamage(ai.attackDamage, DamageType.Physical, toi.transform.position + Vector3.up * 1.1f);
                        Damageable.BoQuaTiaTrungDon = false;
                    }
                    yield return new WaitForSeconds(moc[m]);
                    foreach (var p in Object.FindObjectsByType<DamagePopup>(FindObjectsInactive.Exclude)) Object.Destroy(p.gameObject);
                    Texture2D tx = null;
                    yield return ChupMan(t => tx = t);
                    dem[kieu, m] = DemDo(tx, tamBX, nuaBX);
                    // cat vung quanh hai nhan vat cho anh ghep
                    int cw = nua * 2, ch = nua * 2;
                    var cat = new Texture2D(cw, ch, TextureFormat.RGB24, false);
                    int x0 = Mathf.Clamp((int)tam.x - nua, 0, tx.width - cw), y0 = Mathf.Clamp((int)tam.y - nua, 0, tx.height - ch);
                    cat.SetPixels(tx.GetPixels(x0, y0, cw, ch)); cat.Apply();
                    anhGame.Add(cat);
                    Object.Destroy(tx);
                    anhCan.Add(ChupCan(camCan));
                    ModelHoatHinh.TatVetChem = false;
                }
            }
            var g1 = Ghep(anhGame, moc.Length); File.WriteAllBytes(Thu + "game_" + buoi[bu] + ".png", g1.EncodeToPNG()); Object.Destroy(g1);
            var g2 = Ghep(anhCan, moc.Length); File.WriteAllBytes(Thu + "can_" + buoi[bu] + ".png", g2.EncodeToPNG()); Object.Destroy(g2);
            foreach (var t in anhGame) Object.Destroy(t);
            foreach (var t in anhCan) Object.Destroy(t);
            string dong = "   " + buoi[bu] + " diem anh do quanh Bo xuong (cu / moi) tai";
            for (int m = 0; m < moc.Length; m++) dong += " +" + moc[m].ToString("F2") + "s: " + dem[0, m] + " / " + dem[1, m] + ";";
            nk.AppendLine(dong);
            int tongCu = 0, tongMoi = 0;
            for (int m = 0; m < 2; m++) { tongCu += dem[0, m]; tongMoi += dem[1, m]; }
            Kiem(tongMoi > tongCu * 3 + 100, "E " + buoi[bu] + ": 2 moc dau diem anh do quanh Bo xuong moi " + tongMoi + " > 3 x cu " + tongCu + " (thay ro vet chem)");
            Kiem(dem[1, 3] < Mathf.Max(60, dem[1, 0] / 4), "E " + buoi[bu] + ": vet tan het truoc don sau (+0,22 s: " + dem[1, 3] + ")");
        }
        Object.Destroy(camCan.gameObject);
        if (sang != null) { sang.ApGiay(ChuyenChieuSangDem.GiayGiuaDem); }

        DonHinh();
        if (ai != null) Object.Destroy(ai.gameObject);
        if (hud != null) hud.epCamUng = false;
        TamNhin.BatTrongPhepThu = false;
        ModelHoatHinh.TatVetChem = false;
        nk.Insert(0, "MENU 152 - Don chem Bo xuong: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuChemBoXuong] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuChemBoXuong");
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
        Debug.Log("[ThuChemBoXuong] xong, isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
