using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 71): KY NANG "GIO LOC" (so hieu 10, 16/09/2026).
///
/// Nguoi dung: 3 loc nho (hinh Loc xoay, cao duoi 1/2, mau nau, co tia set), bay bang toc do Qua cau bang,
/// tan sau 3,5 giay; sat thuong 75, hoi chieu 0,4; 55% hat tung 0,5 giay (khong cuon), dang tung chieu thi
/// bi ngat; xuyen moi vat can va nguoi choi. Nguoi dung chon them: 75 mot lan moi muc tieu moi loc, tia set 15,
/// 55% moi loc, 20 nang luong, toa quat 11 do, ngat ca quai, niem 0,38, vung trung 2,2 m, cao 1,5 m, dap tat lo lua
/// (chay lai sau 30 giay), icon Loc xoay nhuom nau.
/// 17/09/2026 nguoi dung doi: hinh dung lai bang Blender (xam trang nhu Loc xoay, cao ~5 m, xoay MOT CHIEU tu duoi len,
/// khoi bui den cuon len + vet phia sau), BO tia set (chi con 75), toc do giam 25% (12,75 m/s).
///
///   A. Thong so that: hang so, nhan vat (nang luong / hoi chieu / niem), Sach phep, HUD 11 icon, icon nap duoc,
///      toc do = truong speed cua mot Qua cau bang THAT.
///   B. Tung that CastAt(10): khoa thi tu choi; mo khoa -> dung 3 loc, tru 20 nang luong, bam lai bi hoi chieu,
///      doi 0,45 s thi tung duoc. Trung bia -> keDanhCuoi = nguoi tung.
///   C. Hinh: luoi Blender (Vo0-2, DaiGio); cao ~5 m (doi chung Loc xoay that); xam trang; khong den; KHONG tia set;
///      XOAY MOT CHIEU TU DUOI LEN: doc xoan cua dai gio doc tu luoi + chieu quay do bang goc that cua vo theo thoi gian
///      + chieu truot anh, moi lop deu cung chieu; khoi bui den cuon quanh than: hat bay LEN va quay CUNG chieu (do tung
///      hat theo randomSeed), vet bui con lai phia sau (hat khong gian the gioi); chup anh.
///   D. Toc do do bang vi tri (m/s) va thoi gian song (luc ngung di).
///   E. Xuyen vat can: bay thang qua mot bia mo; xuyen nguoi choi / ke dich: di tiep sau khi trung.
///   F. Sat thuong: mot loc qua bia dung yen = 75 dung mot lan; ba loc cung trung = 225;
///      vung trung: bia lech 2,5 m trung, 2,7 m truot (tinh toi mat than bia ban kinh 0,4).
///   G. Hat tung: 19 bia x 10 loc -> ti le (dem doc lap bang component xuat hien), do cao hinh lon nhat,
///      thoi gian bay; bia co khieng -> 0 lan.
///   H. Ngat chieu: nguoi choi dang niem Qua cau lua bi hat -> 0 qua bay ra (doi chung khong hat -> 3);
///      loc that trung nguoi dang niem; bi hat thi CastAt bi tu choi. Quai dang ra don bi hat -> khong trung
///      (doi chung -> trung).
///   I. Qua mang: goi ky nang so 10; goi trang thai minh mang bit hat tung; ban sao nhan bit -> bay len;
///      ban sao dang niem nhan bit -> khong phong phep (doi chung phong 3 qua); goi tre khong hat lai lan hai;
///      mat na 5 bit ca goi nguoi choi va goi quai.
///   J. Lo lua: loc luot qua -> tat, lo doi chung van chay; 25 giay van tat, 31 giay chay lai.
///   K. (17/09/2026, nguoi dung: "loc xoay danh trung 1 can nha nho, loc xoay tu treo len mai nha") loc bay XUYEN mot nha mo
///      that: moi khung do do cao loc tru mat dat (tia chi lop Ground, tu viet o day) - phai ~0; DOI CHUNG: tia cu (Ground +
///      Default) tren cung duong phai cham mai (> 1 m) de chac duong thu co mai nha. Do rieng Loc xoay lon (chi bao, khong kiem).
///   Cung ngay: MOT loc moi lan tung (khong con 3), tan sau 4,5 giay, chan to 40% so ban goc (noi toi 2,3 m), roi them 20%
///   tren ban ay (x1,68 so goc).
///   L. (17/09/2026) Trung doi thu -> tia set CHI HIEU UNG tu than loc sang tung doi thu + chop sang + chay sem boc khoi nhu Sam
///      set: 5 bia tren duong -> dung 5 tia (dau tia o than loc, cuoi tia o bia), 5 cho chay sem, moi bia mat DUNG 75 (tia khong
///      sat thuong); loc khong trung ai thi 0 tia (muc C).
///   Sua 17/09/2026 (nguoi dung: toan bo ban kinh to them 10% - chon ca hinh lan vung trung; BO hieu ung set khi trung doi thu;
///   toc do 9,5; LUON co 2 tia set trong loc danh tu dinh xuong nhu Loc xoay, co tia thu nho theo loc; chon: nhip 0,45 s nhu
///   Loc xoay, chi hinh):
///   A/C. hinh: be ngang THAT cua vo Vo1 (renderer) / be ngang luoi = 1,1, chieu cao / luoi = 1,0; vong phun bui 0,495 m;
///      tia set trong loc: moi nhip dung 2 tia, dau tia o gan dinh (>= 4,2 m), duoi tia thap hon >= 2 m, ca hai gan truc loc;
///      be day tia so voi tia THAT cua Loc xoay (do cung luc, DOI CHUNG doc lap voi hang so) ~ ti le chieu cao do duoc.
///   D. toc do 9,5. F. vung trung 2,42 m: bia lech 2,72 m trung, 2,92 m truot (ban kinh cu 2,2 thi 2,72 truot).
///   L. 5 bia tren duong: 0 tia tu than loc sang bia, 0 chop, 0 chay sem; moi bia van mat dung 75.
///   Sua tiep (nguoi dung: "2 tia set luon bi bo lai phia sau con loc"): MOI KHUNG, moi tia trong loc con song: khoang cach
///      ngang tu tam HINH VE THAT (bounds renderer Core) va tu hai dau tia toi truc loc - phai ~ trong long loc suot doi tia;
///      DOI CHUNG: mot tia cung kieu KHONG bam theo, sinh cung luc - phai bi bo lai > 1 m (phep do bat duoc loi cu).
///   Sua tiep (nguoi dung: hai tia "gan sat nhau qua", cach xa ~1,2 m nhung van trong loc va bam loc; cap 5 ra 2 loc song song
///      cach 4 m, trung ca hai, ton gap doi):
///   C. moi nhip: khoang cach ngang giua DINH hai tia (~1,2 m) va giua DUOI hai tia; moi dau tia phai nam TRONG vo trong cung Vo0
///      o dung do cao - ban kinh vo DOC THANG tu luoi FBX ngoai Play (khong dung bang so trong code).
///   M. that bang CastAt: ky nang cap 4 (DOI CHUNG) -> 1 loc, ton 20 x 1,1^3; cap 5 -> dung 2 loc, cung huong, tam cach 4,00 m
///      theo phuong VUONG GOC huong bay, sau 1 s van cach 4 m (song song), ton 20 x 1,1^4 x 2; bia dung giua hai duong bay mat
///      75 x 1,2^4 x 2 (trung ca hai). Goi mang: nguoi kia tung cap 5 -> may minh ra 2 loc.
///
/// Ket qua: PlayTestShots/gioloc.txt, anh gioloc_*.png.
/// </summary>
public static class ThuGioLoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;
    const int K = CapDo.KyGioLoc;

    [MenuItem("Diablo 2.5D/71. Chay thu GIO LOC (ky nang moi)", false, 159)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu gio loc", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] Gio loc");
        DoDocXoanNgoaiPlay();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static int docTangEditor, docGiamEditor;
    static float rChanVo1, rGiuaVo1;
    // Ban kinh lon nhat vo trong cung Vo0 moi 0,25 m do cao (0..5 m) - doc tu luoi FBX ngoai Play
    static readonly float[] rVo0 = new float[21];

    static float BanKinhVo0That(float y)
    {
        float f = Mathf.Clamp(y / 0.25f, 0f, rVo0.Length - 1.001f);
        int i = Mathf.FloorToInt(f);
        // lay NHO hon hai moc ke nhau (than loc loe dan len) - kiem "nam trong" cho chat
        return Mathf.Min(rVo0[i], rVo0[i + 1]) * VfxFactory.HeSoBanKinhGioLoc;
    }

    /// <summary>
    /// DOC XOAN cua dai gio doc thang tu luoi FBX - LAM NGOAI PLAY: trong Play luoi khong bat Read/Write tra ve mang rong.
    /// Dinh ke tiep doc dai (uv.x lon hon mot buoc) len cao thi goc atan2(z,x) tang hay giam.
    /// </summary>
    static void DoDocXoanNgoaiPlay()
    {
        docTangEditor = 0; docGiamEditor = 0; rChanVo1 = 0f; rGiuaVo1 = 0f;
        for (int i = 0; i < rVo0.Length; i++) rVo0[i] = 0f;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/KyNang/GioLoc/LocNho.fbx"))
        {
            var m = o as Mesh;
            // Ban kinh vo Vo1 o chan va o 2,5 m (tren doan noi): nguoi dung xin chan to them 20%, phan tren giu nguyen
            if (m != null && m.name == "Vo1")
                foreach (var p in m.vertices)
                {
                    float r = new Vector2(p.x, p.z).magnitude;
                    if (p.y < 0.05f) rChanVo1 = Mathf.Max(rChanVo1, r);
                    if (Mathf.Abs(p.y - 2.5f) < 0.1f) rGiuaVo1 = Mathf.Max(rGiuaVo1, r);
                }
            if (m != null && m.name == "Vo0")
                foreach (var p in m.vertices)
                {
                    int i = Mathf.RoundToInt(p.y / 0.25f);
                    if (i >= 0 && i < rVo0.Length && Mathf.Abs(p.y - i * 0.25f) < 0.13f) rVo0[i] = Mathf.Max(rVo0[i], new Vector2(p.x, p.z).magnitude);
                }
            if (m == null || m.name != "DaiGio") continue;
            var v = m.vertices; var uv = m.uv;
            for (int i = 0; i < v.Length; i += 5)
            {
                // Bo chan loc (< 1,5 m): ban kinh 0,3 m, nam dai cach nhau ~0,37 m - tim dinh ke tiep de bat nham sang dai ben
                if (v[i].y < 1.5f) continue;
                // Dinh ke tiep = dinh GAN NHAT thoa uv (lay ung vien dau tien thi co luc bat nham dai ben canh)
                int tot = -1; float ganNhat = 0.6f;
                for (int j = 0; j < v.Length; j++)
                {
                    float du = uv[j].x - uv[i].x;
                    if (du < 0.02f || du > 0.04f || Mathf.Abs(uv[j].y - uv[i].y) > 0.01f || v[j].y <= v[i].y) continue;
                    float kc = (v[j] - v[i]).magnitude;
                    if (kc < ganNhat) { ganNhat = kc; tot = j; }
                }
                if (tot < 0) continue;
                float dg = Mathf.DeltaAngle(Mathf.Atan2(v[i].z, v[i].x) * Mathf.Rad2Deg, Mathf.Atan2(v[tot].z, v[tot].x) * Mathf.Rad2Deg);
                if (dg > 0.01f) docTangEditor++; else if (dg < -0.01f) docGiamEditor++;
            }
        }
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_GioLoc") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_GioLoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GioLoc] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    /// <summary>Bia do don co mot khoi hinh con ("Hinh") de BiHatTung co cai nhac len.</summary>
    static Damageable TaoBia(string ten, Vector3 p)
    {
        p.y = VfxFactory.GroundY(p);
        var go = new GameObject(ten);
        go.transform.position = p;
        go.layer = LayerMask.NameToLayer("Enemy");
        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.4f; cap.center = Vector3.up;
        var hinh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(hinh.GetComponent<Collider>());
        hinh.name = "Hinh";
        hinh.transform.SetParent(go.transform, false);
        hinh.transform.localPosition = Vector3.up;
        hinh.transform.localScale = new Vector3(0.7f, 2f, 0.7f);
        var d = go.AddComponent<Damageable>();
        d.maxHealth = 10000000f; d.health = 10000000f;
        return d;
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

    static IEnumerator GiuSong()
    {
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBoGioLoc") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static Vector3 HuongTrong(PlayerController pc)
    {
        Vector3 goc = pc.transform.position + Vector3.up * 1.2f;
        for (int i = 0; i < 24; i++)
        {
            Vector3 h = Quaternion.AngleAxis(i * 15f, Vector3.up) * Vector3.forward;
            if (!Physics.SphereCast(goc, 0.6f, h, out RaycastHit _, 12f, pc.MatNaVatCan, QueryTriggerInteraction.Ignore))
                return h;
        }
        return pc.transform.forward;
    }

    static int DemLoc() { return Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude).Length; }
    static int DemCauLua() { return Object.FindObjectsByType<Fireball>(FindObjectsInactive.Exclude).Length; }

    static void XoaLoc()
    {
        foreach (var g in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) Object.Destroy(g.gameObject);
    }

    /// <summary>Do cao tu goc toi dinh hop bao cac MeshRenderer (vo, dai xoan) cua mot vat.</summary>
    static float CaoHinh(GameObject go)
    {
        float dinh = float.MinValue;
        foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
            dinh = Mathf.Max(dinh, r.bounds.max.y);
        return dinh - go.transform.position.y;
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        // Phep thu dai hon 30 giay: tat GameDirector de dot quai dau khong chen vao
        if (dir != null) dir.enabled = false;
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e7f; mauToi.health = 1e7f;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.DestroyImmediate(q.gameObject);
        int maskEnemy = LayerMask.GetMask("Enemy");

        // ================= A. THONG SO =================
        Ghi("");
        float nl, hc, nc;
        SachPhep.ThongSo(toi, K, out nl, out hc, out nc);
        var quaThat = QuaCauBang.Spawn(toi.transform.position + Vector3.up * 30f, Vector3.up, 0, 0);
        float tocQuaCau = quaThat.speed;
        Object.DestroyImmediate(quaThat.gameObject);
        var hud = GameHUD.Ban;
        var bo = hud != null ? hud.BoIcon() : null;
        var tIcon = Resources.Load<Texture2D>("Icons/GioLoc");
        Ghi(string.Format("A. nhan vat: nang luong {0}, hoi chieu {1} s, niem {2} s; Sach phep doc {3}/{4}/{5}; sat thuong {6}, hat tung {7:P0} x {8} s cao {9} m, vung trung {10} m, song {11} s, cao hinh {12} m",
            toi.gioLocCost, toi.gioLocCooldown, toi.gioLocCastTime, nl, hc, nc, GioLoc.SatThuongGoc,
            GioLoc.XacSuatHatTung, BiHatTung.GiayMacDinh, BiHatTung.CaoBay, GioLoc.BanKinhTrung, GioLoc.ThoiGianSong, GioLoc.ChieuCao));
        Ghi(string.Format("A. toc do loc {0} m/s, toc do mot Qua cau bang that {1} m/s; HUD {2} icon, icon so 10 {3}; icon file {4}; ten \"{5}\", tom tat \"{6}\", mo ta {7} ky tu",
            GioLoc.TocDo, tocQuaCau, bo != null ? bo.Length : -1, bo != null && bo.Length > K && bo[K] != null ? "co" : "KHONG",
            tIcon != null ? tIcon.width + "x" + tIcon.height : "KHONG", SachPhep.Ten(K), SachPhep.TomTat(K), SachPhep.MoTa(K).Length));
        Kiem(Mathf.Approximately(toi.gioLocCost, 20f) && Mathf.Approximately(nl, 20f), "nang luong khong phai 20");
        Kiem(Mathf.Approximately(toi.gioLocCooldown, 0.4f) && Mathf.Approximately(hc, 0.4f), "hoi chieu khong phai 0,4");
        Kiem(Mathf.Approximately(toi.gioLocCastTime, 0.38f), "niem khong phai 0,38");
        // Doi chung doc lap voi code: ban kinh goc = cong thuc Blender cu R(t) = 0,30 + 1,95 t^1,9 (Vo1 he so 1)
        float rChanGoc = 0.30f, rGiuaGoc = 0.30f + 1.95f * Mathf.Pow(0.5f, 1.9f);
        Ghi(string.Format("A. luoi Vo1: ban kinh chan {0:F3} m (goc {1:F3}, x{2:F3}); o 2,5 m {3:F3} m (goc {4:F3}, x{5:F3})",
            rChanVo1, rChanGoc, rChanVo1 / rChanGoc, rGiuaVo1, rGiuaGoc, rGiuaVo1 / rGiuaGoc));
        Kiem(Mathf.Abs(rChanVo1 / rChanGoc - 1.68f) < 0.01f, "chan loc khong to x1,68 so ban goc (40% roi them 20%)");
        Kiem(GioLoc.SoLocMoiLan == 1 && Mathf.Approximately(GioLoc.ThoiGianSong, 4.5f), "khong phai 1 loc / 4,5 giay");
        Kiem(Mathf.Abs(rGiuaVo1 / rGiuaGoc - 1f) < 0.01f, "phan than tren bi doi kich thuoc");
        Kiem(Mathf.Abs(GioLoc.TocDo - 9.5f) < 0.001f && Mathf.Abs(GioLoc.BanKinhTrung - 2.42f) < 0.001f, "toc do loc khong phai 9,5 m/s / vung trung khong phai 2,42 m");
        // 17/09/2026: tia set quay lai nhung CHI HIEU UNG - mo ta phai noi ro, va khong con con so 15 cu
        Kiem(SachPhep.MoTa(K).Contains("tia sét") && SachPhep.MoTa(K).Contains("không gây thêm sát thương") && !SachPhep.MoTa(K).Contains("15") && !SachPhep.MoTa(K).Contains("cháy sém") && SachPhep.MoTa(K).Contains("hai tia sét"), "mo ta Sach phep khong noi dung ve 2 tia set trong loc / con noi chay sem");
        Kiem(bo != null && bo.Length == CapDo.SoKyNang && bo[K] != null && tIcon != null, "thieu icon Gio loc");
        Kiem(SachPhep.Ten(K) == "GIÓ LỐC" && SachPhep.MoTa(K).Length > 100, "Sach phep thieu chu Gio loc");

        // ================= B. TUNG THAT =================
        Ghi("");
        Vector3 huong = HuongTrong(toi);
        toi.transform.rotation = Quaternion.LookRotation(huong);
        Vector3 goc = toi.transform.position;
        int soPhep = 0, kyVua = -1;
        System.Action<int, Vector3, bool> dem = (s, a, d) => { soPhep++; kyVua = s; };
        toi.DaTungPhep += dem;
        CapDo.BatDauTranMoi();
        int s0 = soPhep;
        toi.CastAt(K, goc + huong * 8f);
        bool tuChoiKhoa = soPhep == s0;
        CapDo.MoKhoa(K);
        // Cap 1 chi co 1 diem: len mot cap de co them diem mo Qua cau lua (muc H can mot phep dang niem)
        CapDo.Them(CapDo.CanDeLenCap(1));
        CapDo.MoKhoa(0);
        Kiem(CapDo.DaMo(K) && CapDo.DaMo(0), "khong mo khoa duoc Gio loc va Qua cau lua cho phep thu");
        var biaB = TaoBia("TAM_BiaB", goc + huong * 8f);
        yield return new WaitForSeconds(0.3f);
        toi.mana = toi.maxMana;
        float manaTruoc = toi.mana, mauB = biaB.health;
        toi.CastAt(K, biaB.transform.position);
        bool daTung = soPhep == s0 + 1 && kyVua == K;
        float manaTon = manaTruoc - toi.mana;
        int sBam = soPhep;
        float lucTung = Time.time;
        float hoiNgaySau = toi.HoiChieuGiay(K);
        // Bam lai MOI KHUNG cho toi khi duoc nhan: do bang dong ho game, khong phu thuoc khung hinh Editor giat
        bool thayNhacHoi = false; float lucNhan = -1f; int soLocBay = 0;
        float hanB = Time.time + 1.5f;
        while (Time.time < hanB)
        {
            soLocBay = Mathf.Max(soLocBay, DemLoc());
            toi.CastAt(K, biaB.transform.position);
            if (soPhep > sBam) { lucNhan = Time.time - lucTung; break; }
            if (toi.LastMessage == "GIÓ LỐC đang hồi chiêu") thayNhacHoi = true;
            yield return null;
        }
        bool tuChoiHoiChieu = lucNhan >= 0.4f - 0.001f;
        bool tungLai = lucNhan > 0f && lucNhan < 0.5f;
        yield return new WaitForSeconds(0.6f);
        Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> tung {1}, {2} loc bay, ton {3} nang luong; hoi chieu ngay sau khi tung {4:F2} s; bam lai moi khung -> duoc nhan sau {5:F3} s (thay nhac hoi chieu: {6}); bia truoc mat mat {7:F0} mau, ke danh cuoi la nguoi tung {8}",
            tuChoiKhoa, daTung, soLocBay, manaTon, hoiNgaySau, lucNhan, thayNhacHoi, mauB - biaB.health, biaB.keDanhCuoi == mauToi));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(daTung && soLocBay == 1, "tung Gio loc khong ra dung 1 loc");
        Kiem(Mathf.Abs(manaTon - 20f) < 0.01f, "khong ton dung 20 nang luong");
        Kiem(tuChoiHoiChieu && tungLai, "hoi chieu 0,4 giay khong dung");
        Kiem(biaB.keDanhCuoi == mauToi, "trung bia ma khong ghi ke danh (mat kinh nghiem)");
        Object.Destroy(biaB.gameObject);
        XoaLoc();
        yield return new WaitForSeconds(0.5f);

        // ================= C + D. HINH, TOC DO, THOI GIAN SONG =================
        Ghi("");
        {
            var locXoay = Tornado.Spawn(goc + huong * 40f, huong, 0);
            yield return null; yield return null;
            float caoLon = CaoHinh(locXoay.gameObject);
            // DOI CHUNG be day tia: tia set THAT cua Loc xoay trong 1,2 s (chi co loc nay phong tia luc do)
            var arcLonCu = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            float dayLonMax = 0f; int soTiaLon = 0;
            float hanLon = Time.time + 1.2f;
            while (Time.time < hanLon)
            {
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                    if (arcLonCu.Add(a)) { soTiaLon++; dayLonMax = Mathf.Max(dayLonMax, a.coreWidth); }
                yield return null;
            }
            Object.Destroy(locXoay.gameObject);
            yield return new WaitForSeconds(0.4f);

            var arcTruoc = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            var loc = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
            int nhip0 = VfxFactory.SoNhipSetTrongGioLoc;
            float lucSinh = Time.time;
            yield return null; yield return null;
            float caoNho = CaoHinh(loc.gameObject);
            int soDen = loc.GetComponentsInChildren<Light>(true).Length;

            // Luoi Blender + mau vo
            var tenLuoi = new List<string>();
            Color tb = Color.black; int soVo = 0;
            Transform vo0 = null, dai = null;
            foreach (var mf in loc.GetComponentsInChildren<MeshFilter>())
            {
                tenLuoi.Add(mf.name + "(" + (mf.sharedMesh != null ? mf.sharedMesh.name : "null") + ")");
                if (mf.name == "Vo0") vo0 = mf.transform;
                if (mf.name == "DaiGio") dai = mf.transform;
                var mat = mf.GetComponent<MeshRenderer>().sharedMaterial;
                if (mf.name.StartsWith("Vo") && mat.HasProperty("_TintColor")) { tb += mat.GetColor("_TintColor"); soVo++; }
            }
            if (soVo > 0) tb /= soVo;
            float tiNgang = 0f, tiDoc = 0f;
            foreach (var mf in loc.GetComponentsInChildren<MeshFilter>())
                if (mf.name == "Vo1")
                {
                    // renderer.bounds (the gioi) / sharedMesh.bounds (cuc bo, doc duoc ca khi luoi khong Read/Write). Do luc vo o GOC 0:
                    // renderer.bounds la hop truc the gioi bao hop cuc bo DA XOAY -> vo dang quay cho ti le gia toi x1,32 (lan chay dau).
                    var quayCu = mf.transform.localRotation;
                    mf.transform.localRotation = Quaternion.identity;
                    var bw = mf.GetComponent<MeshRenderer>().bounds.size; var bm = mf.sharedMesh.bounds.size;
                    mf.transform.localRotation = quayCu;
                    tiNgang = Mathf.Max(bw.x / bm.x, bw.z / bm.z); tiDoc = bw.y / bm.y;
                }

            // DOC XOAN cua dai gio: do ngoai Play (DoDocXoanNgoaiPlay)
            int docTang = docTangEditor, docGiam = docGiamEditor;

            // CHIEU QUAY THAT cua tung lop: goc cua truc x cuc bo sau 0,1 s; CHIEU TRUOT anh
            var gocTruoc = new Dictionary<Transform, float>();
            var sps = loc.GetComponentsInChildren<Spin>();
            foreach (var sp in sps) { var x = sp.transform.right; gocTruoc[sp.transform] = Mathf.Atan2(x.z, x.x) * Mathf.Rad2Deg; }

            // Hat khoi bui cuon: chup vi tri cuc bo theo randomSeed
            var psBui = loc.transform.Find("GioLocHinh/BuiCuon").GetComponent<ParticleSystem>();
            yield return new WaitForSeconds(0.5f);
            var hat0 = new ParticleSystem.Particle[psBui.main.maxParticles];
            int n0 = psBui.GetParticles(hat0);
            var cuHat = new Dictionary<uint, Vector3>();
            // VONG PHUN: nhan ban he hat, tat moi van toc, phun 200 hat mot luc roi doc ngay - lech doc va ban kinh luc sinh
            var ban = Object.Instantiate(psBui.gameObject, psBui.transform.parent);
            var psBan = ban.GetComponent<ParticleSystem>();
            psBan.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var vBan = psBan.velocityOverLifetime; vBan.enabled = false;
            var mBan = psBan.main; mBan.startSpeed = 0f; mBan.maxParticles = 300;
            var eBan = psBan.emission; eBan.enabled = false;
            psBan.Emit(200);
            var hBan = new ParticleSystem.Particle[300];
            int nBan = psBan.GetParticles(hBan);
            float yMaxLucSinh = 0f, rMaxLucSinh = 0f; int soSinhMoi = nBan;
            for (int i = 0; i < nBan; i++)
            {
                yMaxLucSinh = Mathf.Max(yMaxLucSinh, Mathf.Abs(hBan[i].position.y));
                rMaxLucSinh = Mathf.Max(rMaxLucSinh, new Vector2(hBan[i].position.x, hBan[i].position.z).magnitude);
            }
            Object.Destroy(ban);
            for (int i = 0; i < n0; i++)
            {
                cuHat[hat0[i].randomSeed] = hat0[i].position;
            }
            foreach (var sp in sps) { var x = sp.transform.right; gocTruoc[sp.transform] = Mathf.Atan2(x.z, x.x) * Mathf.Rad2Deg; }
            yield return new WaitForSeconds(0.1f);
            int quayTang = 0, quayGiam = 0;
            foreach (var sp in sps)
            {
                var x = sp.transform.right;
                float dg = Mathf.DeltaAngle(gocTruoc[sp.transform], Mathf.Atan2(x.z, x.x) * Mathf.Rad2Deg);
                if (dg > 0f) quayTang++; else quayGiam++;
            }
            int truotLen = 0, truotSai = 0;
            foreach (var su in loc.GetComponentsInChildren<ScrollUV>())
            {
                bool dung = su.name == "DaiGio" ? su.speed.x < 0f : su.speed.y < 0f;
                if (dung) truotLen++; else truotSai++;
            }
            var hat1 = new ParticleSystem.Particle[psBui.main.maxParticles];
            int n1 = psBui.GetParticles(hat1);
            int hatLen = 0, hatXuong = 0, hatCungChieu = 0, hatNguoc = 0;
            for (int i = 0; i < n1; i++)
            {
                Vector3 p0;
                if (!cuHat.TryGetValue(hat1[i].randomSeed, out p0)) continue;
                Vector3 p1 = hat1[i].position;
                if (p1.y > p0.y) hatLen++; else hatXuong++;
                float dg = Mathf.DeltaAngle(Mathf.Atan2(p0.z, p0.x) * Mathf.Rad2Deg, Mathf.Atan2(p1.z, p1.x) * Mathf.Rad2Deg);
                if (dg > 0f) hatCungChieu++; else hatNguoc++;
            }
            // Vet bui phia sau: he hat khong gian the gioi
            var psVet = loc.transform.Find("GioLocHinh/KhoiBui").GetComponent<ParticleSystem>();
            int tongHat = 0;
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>()) tongHat += ps.particleCount;

            yield return Chup("gioloc_1_bay");

            Vector3 p0v = loc.transform.position; float t0 = Time.time;
            yield return new WaitForSeconds(1f);
            Vector3 p1v = loc.transform.position; float t1 = Time.time;
            float toc = new Vector2(p1v.x - p0v.x, p1v.z - p0v.z).magnitude / (t1 - t0);

            float batDau = lucSinh;
            Vector3 cu = loc.transform.position; float lucDiCuoi = Time.time;
            int soArc = 0, soKhungCoTia = 0, soKhungDung2 = 0, dauCao = 0, duoiThap = 0, ganTruc = 0;
            float cachDinhMin = 99f, cachDinhTong = 0f, cachDuoiMin = 99f, cachDuoiTong = 0f; int soCap = 0;
            float traiVoMax = -9f;
            float dayNhoMax = 0f, dauThapNhat = 99f, lechTrucMax = 0f;
            System.Action demTia = () =>
            {
                int trongKhung = 0;
                var moi = new List<LightningArc>();
                foreach (var aNew in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (!arcTruoc.Add(aNew)) continue;
                    soArc++; trongKhung++; moi.Add(aNew);
                    if (loc == null) continue;
                    float datY = loc.transform.position.y;
                    float hDau = aNew.start.y - datY;
                    dauThapNhat = Mathf.Min(dauThapNhat, hDau);
                    if (hDau >= 4.2f) dauCao++;
                    if (aNew.start.y - aNew.end.y >= 2f) duoiThap++;
                    // lech khoi truc: tru quang loc da di trong mot khung (9,5 m/s)
                    Vector3 t = loc.transform.position;
                    float lech = Mathf.Max(new Vector2(aNew.start.x - t.x, aNew.start.z - t.z).magnitude, new Vector2(aNew.end.x - t.x, aNew.end.z - t.z).magnitude);
                    lechTrucMax = Mathf.Max(lechTrucMax, lech);
                    // Nam TRONG vo trong cung Vo0 (ban kinh doc tu luoi) o dung do cao cua moi dau tia
                    float traiDau = new Vector2(aNew.start.x - t.x, aNew.start.z - t.z).magnitude - BanKinhVo0That(aNew.start.y - datY);
                    float traiDuoi = new Vector2(aNew.end.x - t.x, aNew.end.z - t.z).magnitude - BanKinhVo0That(aNew.end.y - datY);
                    traiVoMax = Mathf.Max(traiVoMax, Mathf.Max(traiDau, traiDuoi));
                    if (traiDau < 0f && traiDuoi < 0f) ganTruc++;
                    dayNhoMax = Mathf.Max(dayNhoMax, aNew.coreWidth);
                }
                if (trongKhung > 0) { soKhungCoTia++; if (trongKhung == 2) soKhungDung2++; }
                if (moi.Count == 2)
                {
                    float cd = new Vector2(moi[0].start.x - moi[1].start.x, moi[0].start.z - moi[1].start.z).magnitude;
                    float cu2 = new Vector2(moi[0].end.x - moi[1].end.x, moi[0].end.z - moi[1].end.z).magnitude;
                    cachDinhMin = Mathf.Min(cachDinhMin, cd); cachDinhTong += cd;
                    cachDuoiMin = Mathf.Min(cachDuoiMin, cu2); cachDuoiTong += cu2; soCap++;
                }
            };
            // Chup lai moc dem NGAY TRUOC vong: tia sinh trong luc cho o tren da tat (song 0,13-0,28 s), dem chung la lech nhip
            arcTruoc = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            nhip0 = VfxFactory.SoNhipSetTrongGioLoc;
            // DOI CHUNG bo lai phia sau: tia khong bam theo, song 0,28 s
            var tiaDoiChung = LightningArc.Create(loc.transform.position + Vector3.up * 4.5f, loc.transform.position + Vector3.up * 1f, 0.28f, 0.28f);
            tiaDoiChung.name = "TAM_TiaDoiChung";
            arcTruoc.Add(tiaDoiChung);
            float lechDoiChungMax = 0f, lechHinhMax = 0f, lechDauMax = 0f; int soKhungDoLech = 0;
            // DO TROI: khoang lech (dau tia - tam loc) luc thay tia lan dau; moi khung sau so voi no -> bam sat thi ~0
            var lechBanDau = new Dictionary<LightningArc, Vector3>();
            float troiMax = 0f, tuoiTroiMax = 0f; var lucThay = new Dictionary<LightningArc, float>();
            System.Func<Transform, float> ngang = tr => loc == null ? 0f : new Vector2(tr.position.x - loc.transform.position.x, tr.position.z - loc.transform.position.z).magnitude;
            float hanS = Time.time + 3.6f;
            while (Time.time < hanS && loc != null)
            {
                demTia();
                Vector3 tl = loc.transform.position;
                if (tiaDoiChung != null)
                {
                    var rc = tiaDoiChung.transform.Find("Core");
                    if (rc != null) { var bc = rc.GetComponent<MeshRenderer>().bounds.center; lechDoiChungMax = Mathf.Max(lechDoiChungMax, new Vector2(bc.x - tl.x, bc.z - tl.z).magnitude); }
                }
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (a.name != "SetTrongGioLoc") continue;
                    var rc = a.transform.Find("Core");
                    if (rc == null) continue;
                    var bc = rc.GetComponent<MeshRenderer>().bounds.center;
                    lechHinhMax = Mathf.Max(lechHinhMax, new Vector2(bc.x - tl.x, bc.z - tl.z).magnitude);
                    lechDauMax = Mathf.Max(lechDauMax, Mathf.Max(new Vector2(a.start.x - tl.x, a.start.z - tl.z).magnitude, new Vector2(a.end.x - tl.x, a.end.z - tl.z).magnitude));
                    Vector3 lechNay = a.start - tl; lechNay.y = 0f;
                    Vector3 lech0;
                    if (!lechBanDau.TryGetValue(a, out lech0)) { lechBanDau[a] = lechNay; lucThay[a] = Time.time; }
                    else
                    {
                        float troi = (lechNay - lech0).magnitude;
                        if (troi > troiMax) { troiMax = troi; tuoiTroiMax = Time.time - lucThay[a]; }
                    }
                    soKhungDoLech++;
                }
                if ((loc.transform.position - cu).sqrMagnitude > 1e-6f) { lucDiCuoi = Time.time; cu = loc.transform.position; }
                // Cuoi khung (sau LateUpdate, dung luc da ve): coroutine 'yield null' chay GIUA Update va LateUpdate - luc do loc da
                // di buoc moi con tia chua bam theo, do ra lech gia 9,5 m/s x dt (~0,35-0,5 m, lan chay dau 0,68 m)
                yield return new WaitForEndOfFrame();
            }
            float song = lucDiCuoi - batDau;
            int soNhip = VfxFactory.SoNhipSetTrongGioLoc - nhip0;
            if (tiaDoiChung != null) Object.Destroy(tiaDoiChung.gameObject);
            Ghi(string.Format("C. luoi: {0}; cao hinh loc nho {1:F2} m, Loc xoay that {2:F2} m, ti le {3:F2}; mau vo trung binh ({4:F2}, {5:F2}, {6:F2}); den {7}; tia set {8}",
                string.Join(", ", tenLuoi.ToArray()), caoNho, caoLon, caoNho / caoLon, tb.r, tb.g, tb.b, soDen, soArc));
            Ghi(string.Format("C. xoan: dai gio len cao goc tang {0} / giam {1}; quay that sau 0,1 s: goc tang {2} lop / giam {3} lop; truot anh len {4} lop / sai {5}",
                docTang, docGiam, quayTang, quayGiam, truotLen, truotSai));
            Ghi(string.Format("C. khoi bui cuon: {0} hat so sanh duoc - bay len {1} / xuong {2}, quay goc tang {3} / giam {4}; vong phun ({6} hat phun thu, tat van toc) lech doc toi da {5:F3} m, ban kinh toi da {9:F2} m; vet phia sau khong gian {7}; tong hat mot loc {8}",
                hatLen + hatXuong, hatLen, hatXuong, hatCungChieu, hatNguoc, yMaxLucSinh, soSinhMoi, psVet.main.simulationSpace, tongHat, rMaxLucSinh));
            Ghi(string.Format("D. toc do do {0:F2} m/s; ngung di sau {1:F2} s", toc, song));
            Ghi(string.Format("C. ban kinh: be ngang that vo Vo1 / luoi x{0:F3}, chieu cao x{1:F3}; vong phun bui ban kinh toi da {2:F3} m (goc 0,45)",
                tiNgang, tiDoc, rMaxLucSinh));
            Ghi(string.Format("C. tia set trong loc: {0} tia trong {1} khung co tia ({2} khung dung 2 tia), bo dem nhip +{3}; dau tia cao >= 4,2 m {4} (thap nhat {5:F2} m), duoi thap hon dau >= 2 m {6}, gan truc {7} (lech lon nhat {8:F2} m)",
                soArc, soKhungCoTia, soKhungDung2, soNhip, dauCao, dauThapNhat, duoiThap, ganTruc, lechTrucMax));
            Ghi(string.Format("C. be day loi tia lon nhat: Gio loc {0:F4} / Loc xoay THAT {1:F4} ({2} tia do) = x{3:F3}; ti le chieu cao do duoc x{4:F3}",
                dayNhoMax, dayLonMax, soTiaLon, dayLonMax > 0 ? dayNhoMax / dayLonMax : 0f, caoNho / caoLon));
            Kiem(tenLuoi.Count == 4 && vo0 != null && dai != null, "khong dung luoi Blender (Vo0-2, DaiGio)");
            Kiem(Mathf.Abs(caoNho - 5f) < 0.35f && caoNho / caoLon < 0.5f, "loc nho khong cao ~5 m");
            Kiem(Mathf.Abs(tb.r - tb.b) < 0.12f && tb.r > 0.75f, "vo loc khong xam trang nhu Loc xoay");
            Kiem(soDen == 0, "loc nho con den diem");
            Kiem(soNhip >= 5 && soArc == soNhip * 2 && soKhungDung2 == soKhungCoTia, "tia set trong loc khong phai moi nhip dung 2 tia");
            Ghi(string.Format("C. khoang cach hai tia ({0} nhip): dinh - nho nhat {1:F2} m, trung binh {2:F2} m; duoi - nho nhat {3:F2} m, trung binh {4:F2} m; dau tia nam trong vo trong cung {5}/{6} (sat vo nhat {7:F2} m, am = ben trong)",
                soCap, cachDinhMin, soCap > 0 ? cachDinhTong / soCap : 0f, cachDuoiMin, soCap > 0 ? cachDuoiTong / soCap : 0f, ganTruc, soArc, traiVoMax));
            Kiem(dauCao == soArc && duoiThap == soArc && ganTruc == soArc, "tia set khong danh tu dinh loc xuong trong long loc (co dau tia lo ra ngoai vo trong cung)");
            Kiem(soCap >= 5 && cachDinhMin > 1.1f && cachDinhTong / soCap < 1.3f, "dinh hai tia khong cach nhau ~1,2 m");
            Ghi(string.Format("C. tia bam theo loc (moi khung, {0} lan do): tam hinh ve that cach truc loc lon nhat {1:F2} m, hai dau tia cach truc lon nhat {2:F2} m; tia TROI khoi cho ban dau so voi loc lon nhat {4:F3} m (o tuoi {5:F3} s, {6} tia theo doi); DOI CHUNG tia khong bam theo: bi bo lai {3:F2} m",
                soKhungDoLech, lechHinhMax, lechDauMax, lechDoiChungMax, troiMax, tuoiTroiMax, lechBanDau.Count));
            Kiem(lechDoiChungMax > 1f, "doi chung: tia khong bam theo ma khong bi bo lai - phep do vo nghia");
            Kiem(soKhungDoLech > 20 && troiMax < 0.05f, "tia set trong loc bi bo lai phia sau con loc");
            Kiem(soTiaLon > 0 && dayLonMax > 0f && Mathf.Abs((dayNhoMax / dayLonMax) / (caoNho / caoLon) - 1f) < 0.15f, "be day tia set khong thu nho theo co loc (so voi tia that cua Loc xoay)");
            Kiem(Mathf.Abs(tiNgang - 1.1f) < 0.02f && Mathf.Abs(tiDoc - 1f) < 0.02f, "hinh loc khong rong them 10% (hoac bi doi chieu cao)");
            Kiem(Mathf.Abs(rMaxLucSinh - 0.495f) < 0.03f, "vong bui chan loc khong rong them 10%");
            // Vet gio "goc giam khi len cao" + quay lam goc TANG = vet chay LEN
            bool xoanGiam = docGiam > 50 && docTang == 0;
            Kiem(xoanGiam && quayTang == sps.Length && quayGiam == 0, "cac lop khong xoay cung mot chieu di len");
            Kiem(truotSai == 0 && truotLen == sps.Length, "anh gio truot khong cung chieu di len");
            Kiem(hatLen + hatXuong > 10 && hatXuong == 0, "khoi bui khong bay len");
            Kiem(hatCungChieu > (hatCungChieu + hatNguoc) * 0.9f, "khoi bui khong cuon cung chieu than loc");
            Kiem(soSinhMoi >= 150 && yMaxLucSinh < 0.02f, "vong phun khoi bui khong nam ngang tren dat");
            Kiem(psVet.main.simulationSpace == ParticleSystemSimulationSpace.World, "khong co vet bui o lai phia sau");
            Kiem(Mathf.Abs(toc - 9.5f) < 0.35f, "toc do loc khong phai 9,5 m/s");
            Kiem(Mathf.Abs(song - 4.5f) < 0.12f, "loc khong tan sau 4,5 giay");
            float hanX = Time.time + 7f;
            while (loc != null && Time.time < hanX) yield return null;
            Kiem(loc == null, "loc tan roi ma vat the khong bi xoa");
        }

        // ================= E. XUYEN VAT CAN =================
        Ghi("");
        {
            BoxCollider bia = null; Vector3 tu = Vector3.zero, huongE = Vector3.zero;
            foreach (var c in Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude))
            {
                if (c.gameObject.layer != 0 || !c.name.StartsWith("TS_")) continue;
                if (Vector3.Distance(c.transform.position, goc) > 40f) continue;
                Vector3 h = (c.bounds.center - goc); h.y = 0f; h.Normalize();
                bia = c; huongE = h; tu = c.bounds.center - h * 6f; break;
            }
            if (bia == null) { Ghi("[LOI] khong tim thay bia mo de thu xuyen"); loi++; }
            else
            {
                var loc = GioLoc.Spawn(tu, huongE, maskEnemy);
                float xaNhat = 0f;
                float hanE = Time.time + 1.9f;
                while (Time.time < hanE && loc != null)
                {
                    Vector3 v = loc.transform.position - tu; v.y = 0f;
                    xaNhat = Mathf.Max(xaNhat, Vector3.Dot(v, huongE));
                    yield return null;
                }
                // Doi chung: tia thang doc duong ay co cham bia that khong
                bool coCham = Physics.Raycast(tu, huongE, 12f, 1 << 0, QueryTriggerInteraction.Ignore);
                Ghi(string.Format("E. bia {0} nam giua duong (tia doi chung cham vat can: {1}); loc di duoc {2:F1} m doc huong trong 1,9 s (bia o 6 m)", bia.name, coCham, xaNhat));
                Kiem(coCham, "doi chung: duong thu khong co vat can - phep do vo nghia");
                Kiem(xaNhat > 12f, "loc bi vat can chan lai");
                if (loc != null) Object.Destroy(loc.gameObject);
            }
        }

        // ================= K. KHONG TREO LEN MAI NHA =================
        Ghi("");
        {
            int lopDat = LayerMask.GetMask("Ground"), lopCu = LayerMask.GetMask("Ground", "Default");
            System.Func<Vector3, int, float> tia = (q, lop) =>
            {
                RaycastHit h;
                return Physics.Raycast(q + Vector3.up * 30f, Vector3.down, out h, 80f, lop, QueryTriggerInteraction.Ignore) ? h.point.y : float.NaN;
            };
            var nha = GameObject.Find("MAUS_A_001_682");
            var cNha = nha != null ? nha.GetComponent<Collider>() : null;
            if (cNha == null) { Ghi("[LOI] khong tim thay nha mo MAUS_A_001_682"); loi++; }
            else
            {
                Vector3 tam = cNha.bounds.center; tam.y = 0f;
                Vector3 hK = Vector3.right;
                Vector3 tuK = tam - hK * 9f;
                // Doi chung: tia cu tren duong di cham mai cao bao nhieu so voi dat
                float maiCaoNhat = 0f;
                for (float d = 0f; d <= 18f; d += 0.25f)
                {
                    Vector3 q = tuK + hK * d;
                    float yd = tia(q, lopDat), yc = tia(q, lopCu);
                    if (!float.IsNaN(yd) && !float.IsNaN(yc)) maiCaoNhat = Mathf.Max(maiCaoNhat, yc - yd);
                }
                var locK = GioLoc.Spawn(tuK, hK, maskEnemy);
                float lechMax = 0f; int soKhung = 0, khungTrongNha = 0;
                float hanK = Time.time + 2.4f;
                while (Time.time < hanK && locK != null)
                {
                    Vector3 q = locK.transform.position;
                    float yd = tia(q, lopDat);
                    if (!float.IsNaN(yd)) { lechMax = Mathf.Max(lechMax, Mathf.Abs(q.y - yd)); soKhung++; }
                    Vector3 phang = new Vector3(q.x, cNha.bounds.center.y, q.z);
                    if (cNha.bounds.Contains(phang)) khungTrongNha++;
                    yield return null;
                }
                if (locK != null) Object.Destroy(locK.gameObject);

                // Loc xoay lon: cung duong, chi bao
                var locLon = Tornado.Spawn(tuK + hK * 5f, hK, 0);
                float lechLon = 0f;
                float hanL = Time.time + 2.5f;
                while (Time.time < hanL && locLon != null)
                {
                    Vector3 q = locLon.transform.position;
                    float yd = tia(q, lopDat);
                    if (!float.IsNaN(yd)) lechLon = Mathf.Max(lechLon, q.y - yd);
                    yield return null;
                }
                if (locLon != null) Object.Destroy(locLon.gameObject);

                Ghi(string.Format("K. nha {0}: doi chung - tia cu cham mai cao nhat {1:F2} m tren dat; Gio loc di {2} khung ({3} khung trong nha): lech khoi mat dat toi da {4:F3} m",
                    nha.name, maiCaoNhat, soKhung, khungTrongNha, lechMax));
                Ghi(string.Format("K. (chi bao) Loc xoay lon cung duong trong 2,5 s: cao hon mat dat toi da {0:F2} m", lechLon));
                Kiem(maiCaoNhat > 1f, "doi chung: duong thu khong co mai nha - phep do vo nghia");
                Kiem(khungTrongNha > 5, "loc khong di xuyen qua nha");
                Kiem(lechMax < 0.05f, "Gio loc van treo len mai nha / vat");
            }
            yield return new WaitForSeconds(0.5f);
        }

        // ================= L. KHONG CON HIEU UNG SET KHI TRUNG =================
        Ghi("");
        {
            var vuongL = Vector3.Cross(Vector3.up, huong).normalized;
            var cac = new List<Damageable>();
            for (int i = 0; i < 5; i++)
                cac.Add(TaoBia("TAM_L" + i, goc + huong * (5f + i * 3f) + vuongL * ((i % 2 == 0 ? 1f : -1f) * 1.0f)));
            yield return new WaitForFixedUpdate();
            var mauTruocL = new float[5];
            for (int i = 0; i < 5; i++) mauTruocL[i] = cac[i].health;
            var arcCu = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            var gocCu = new HashSet<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
            var locL = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
            locL.xacSuatHatTung = 0f;
            int soArc = 0, tiaSangBia = 0, soChaySem = 0, soChop = 0;
            float hanL = Time.time + 3.0f;
            bool daChupL = false;
            while (Time.time < hanL && locL != null)
            {
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (!arcCu.Add(a)) continue;
                    soArc++;
                    // tia "sang doi thu": cuoi tia nam o mot bia (ngang < 0,5 m) va cach truc loc > 0,9 m
                    Vector3 t = locL.transform.position;
                    bool xaTruc = new Vector2(a.end.x - t.x, a.end.z - t.z).magnitude > 0.9f;
                    foreach (var b in cac)
                        if (xaTruc && new Vector2(a.end.x - b.transform.position.x, a.end.z - b.transform.position.z).magnitude < 0.5f) { tiaSangBia++; break; }
                }
                foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (!gocCu.Add(g)) continue;
                    if (g.name == "SetChayDen") soChaySem++;
                    if (g.name.StartsWith("Vfx_SetChamDat") || g.name.StartsWith("LightningImpact")) soChop++;
                }
                if (!daChupL && soArc >= 2 && Time.time > hanL - 2.2f) { daChupL = true; yield return Chup("gioloc_3_set_trong_loc"); }
                yield return null;
            }
            int matDung75 = 0;
            for (int i = 0; i < 5; i++) if (Mathf.Abs((mauTruocL[i] - cac[i].health) - 75f) < 0.5f) matDung75++;
            Ghi(string.Format("L. 5 bia tren duong: tia set moi {0} (deu la tia trong long loc), tia tu than loc sang bia {1}; cho chay sem moi {2}; chop sang Sam set moi {3}; bia mat dung 75 {4}/5",
                soArc, tiaSangBia, soChaySem, soChop, matDung75));
            Kiem(tiaSangBia == 0 && soChop == 0 && soChaySem == 0, "van con hieu ung set / chop / chay sem khi loc trung doi thu");
            Kiem(soArc > 0, "doi chung: khong thay tia nao trong 3 giay - phep dem tia vo nghia");
            Kiem(matDung75 == 5, "bia khong mat dung 75");
            foreach (var b in cac) Object.Destroy(b.gameObject);
            if (locL != null) Object.Destroy(locL.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        // ================= F. SAT THUONG + VUNG TRUNG =================
        Ghi("");
        {
            Vector3 tu = goc + huong * 1.2f;
            var vuong = Vector3.Cross(Vector3.up, huong).normalized;
            var bGiua = TaoBia("TAM_FGiua", tu + huong * 10f);
            // Mep trung = 2,42 + ban kinh than bia 0,4 = 2,82 m: 2,72 trung, 2,92 truot (voi 2,2 cu thi 2,72 da truot)
            var b25 = TaoBia("TAM_F25", tu + huong * 14f + vuong * 2.72f);
            var b27 = TaoBia("TAM_F27", tu + huong * 18f - vuong * 2.92f);
            yield return new WaitForFixedUpdate();
            float m1 = bGiua.health, m25 = b25.health, m27 = b27.health;
            var loc = GioLoc.Spawn(tu, huong, maskEnemy);
            float xaSauTrung = 0f;
            float hanF = Time.time + 3.8f;
            while (Time.time < hanF && loc != null)
            {
                Vector3 v = loc.transform.position - tu; v.y = 0f;
                xaSauTrung = Mathf.Max(xaSauTrung, Vector3.Dot(v, huong));
                yield return null;
            }
            // Ghi so mat mau TRUOC khi ba loc sau bay qua (cung duong, se trung them cac bia tren)
            float matGiua = m1 - bGiua.health, mat25 = m25 - b25.health, mat27 = m27 - b27.health;
            // Bia truoc mat nhan vat 4 m: ca ba loc cung trung
            var bBa = TaoBia("TAM_FBa", goc + huong * 4f);
            yield return new WaitForFixedUpdate();
            float mBa = bBa.health;
            GioLoc.SpawnChum(goc + huong * 1.2f, huong, maskEnemy, mauToi);
            yield return new WaitForSeconds(1f);
            Ghi(string.Format("F. mot loc qua bia dung yen mat {0:F0}; bia lech 2,72 m mat {1:F0}; bia lech 2,92 m mat {2:F0}; loc di tiep sau khi trung toi {3:F1} m; ba loc cung qua mot bia mat {4:F0}",
                matGiua, mat25, mat27, xaSauTrung, mBa - bBa.health));
            Kiem(Mathf.Abs(matGiua - 75f) < 0.5f, "mot loc khong gay dung 75 mot lan");
            Kiem(Mathf.Abs(mat25 - 75f) < 0.5f && mat27 < 0.5f, "vung trung 2,42 m khong dung");
            Kiem(xaSauTrung > 25f, "loc dung lai sau khi trung ke dich");
            Kiem(Mathf.Abs((mBa - bBa.health) - 225f) < 0.5f, "ba loc cung trung khong ra 225");
            foreach (var b in new[] { bGiua, b25, b27, bBa }) Object.Destroy(b.gameObject);
            XoaLoc();
            yield return new WaitForSeconds(0.3f);
        }

        // ================= G. HAT TUNG =================
        Ghi("");
        {
            var hang = new List<Damageable>();
            var bay = new Dictionary<Damageable, float>();
            var tu = goc + huong * 1.2f;
            // 8 m/s x 3,5 s = 28 m: 19 bia cach 1,3 m tu 4 m toi 27,4 m
            for (int i = 0; i < 19; i++) hang.Add(TaoBia("TAM_G" + i, tu + huong * (4f + i * 1.3f)));
            yield return new WaitForFixedUpdate();
            int soTrung = 0, soHat = 0; float caoMax = 0f; float tongGiay = 0f; int soDoGiay = 0;
            int hat0 = GioLoc.SoLanHat, trung0 = GioLoc.SoLanTrung;
            bool daChupHat = false;
            for (int lan = 0; lan < 10; lan++)
            {
                var loc = GioLoc.Spawn(tu, huong, maskEnemy);
                var dangCo = new HashSet<Damageable>();
                var batDau = new Dictionary<Damageable, float>();
                float hanG = Time.time + 4.2f;
                while (Time.time < hanG)
                {
                    foreach (var d in hang)
                    {
                        var h = d.GetComponent<BiHatTung>();
                        var hinh = d.transform.Find("Hinh");
                        if (hinh != null) caoMax = Mathf.Max(caoMax, hinh.localPosition.y - 1f);
                        if (h != null && !dangCo.Contains(d))
                        {
                            dangCo.Add(d); batDau[d] = Time.time; soHat++;
                            if (!daChupHat && d == hang[1]) { daChupHat = true; }
                        }
                        if (h == null && batDau.ContainsKey(d))
                        {
                            tongGiay += Time.time - batDau[d]; soDoGiay++;
                            batDau.Remove(d);
                        }
                    }
                    yield return null;
                }
                if (loc != null) Object.Destroy(loc.gameObject);
            }
            soTrung = GioLoc.SoLanTrung - trung0;
            float tile = soTrung > 0 ? soHat / (float)soTrung : 0f;
            Ghi(string.Format("G. {0} lan loc trung bia; dem doc lap {1} lan bia co BiHatTung ({2:P1}), bo dem trong code {3}; do cao hinh lon nhat {4:F2} m; bay trung binh {5:F2} s ({6} lan do)",
                soTrung, soHat, tile, GioLoc.SoLanHat - hat0, caoMax, soDoGiay > 0 ? tongGiay / soDoGiay : 0f, soDoGiay));
            Kiem(soTrung == 190, "khong du 190 lan trung (19 bia x 10 loc)");
            Kiem(tile > 0.47f && tile < 0.63f, "ti le hat tung khong quanh 55%");
            Kiem(soHat == GioLoc.SoLanHat - hat0, "dem doc lap khac bo dem trong code");
            Kiem(Mathf.Abs(caoMax - 1.5f) < 0.08f, "do cao hat tung khong phai 1,5 m");
            Kiem(soDoGiay > 50 && Mathf.Abs(tongGiay / soDoGiay - 0.5f) < 0.06f, "thoi gian bay khong phai 0,5 giay");
            foreach (var d in hang) Object.Destroy(d.gameObject);
            yield return new WaitForSeconds(0.3f);

            // Anh: mot bia dang bay len
            var biaAnh = TaoBia("TAM_GAnh", goc + huong * 7f);
            yield return new WaitForFixedUpdate();
            var locAnh = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
            locAnh.xacSuatHatTung = 1f;
            float hanA = Time.time + 1f;
            while (Time.time < hanA)
            {
                var h = biaAnh.GetComponent<BiHatTung>();
                if (h != null && h.daTroi > 0.2f) { yield return Chup("gioloc_2_hat_tung"); break; }
                yield return null;
            }
            Object.Destroy(biaAnh.gameObject);
            XoaLoc();

            // Khieng
            var biaK = TaoBia("TAM_GKhieng", goc + huong * 6f);
            biaK.khieng = Khieng.Bat(biaK.gameObject, 1e6f, 1.2f);
            yield return new WaitForFixedUpdate();
            float mK = biaK.health; int hatK = 0;
            for (int lan = 0; lan < 20; lan++)
            {
                var l = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
                l.xacSuatHatTung = 1f;
                float hanK = Time.time + 0.6f;
                bool coLan = false;
                while (Time.time < hanK) { if (biaK.GetComponent<BiHatTung>() != null) coLan = true; yield return null; }
                if (coLan) hatK++;
                Object.Destroy(l.gameObject);
            }
            Ghi(string.Format("G. bia co khieng, 20 loc (ti le hat 100%): hat tung {0} lan, mau mat {1:F0}", hatK, mK - biaK.health));
            Kiem(hatK == 0, "khieng khong chan hat tung");
            Object.Destroy(biaK.gameObject);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= H. NGAT CHIEU =================
        Ghi("");
        {
            // H1: nguoi choi dang niem Qua cau lua, bi hat (Apply that) -> khong qua nao
            int nNgat = 0, nDoiChung = 0, soNgatCode0 = PlayerController.SoLanNgatChieu;
            for (int lan = 0; lan < 3; lan++)
            {
                toi.mana = toi.maxMana;
                int truoc = DemCauLua(), maxMoi = 0;
                toi.CastAt(0, goc + huong * 10f);
                yield return null;
                BiHatTung.Apply(mauToi, 0.5f);
                bool khoa = toi.DangBiKhoaCung;
                float hanH = Time.time + 1f;
                while (Time.time < hanH) { maxMoi = Mathf.Max(maxMoi, DemCauLua() - truoc); yield return null; }
                if (maxMoi == 0 && khoa) nNgat++;
                yield return new WaitForSeconds(0.4f);

                toi.mana = toi.maxMana;
                truoc = DemCauLua(); maxMoi = 0;
                toi.CastAt(0, goc + huong * 10f);
                hanH = Time.time + 1f;
                while (Time.time < hanH) { maxMoi = Mathf.Max(maxMoi, DemCauLua() - truoc); yield return null; }
                if (maxMoi == 3) nDoiChung++;
                yield return new WaitForSeconds(0.4f);
            }
            Ghi(string.Format("H1. dang niem Qua cau lua bi hat tung: {0}/3 lan khong qua nao bay ra (bo dem ngat {1}); doi chung khong hat: {2}/3 lan ra du 3 qua",
                nNgat, PlayerController.SoLanNgatChieu - soNgatCode0, nDoiChung));
            Kiem(nNgat == 3 && nDoiChung == 3, "bi hat tung ma chieu dang niem khong bi ngat");

            // H2: loc THAT trung nguoi choi dang niem; bi hat thi CastAt bi tu choi
            toi.mana = toi.maxMana;
            int truocH2 = DemCauLua(), maxH2 = 0;
            toi.CastAt(0, goc + huong * 10f);
            var locH2 = GioLoc.Spawn(goc - huong * 3f, huong, LayerMask.GetMask("Player"));
            locH2.xacSuatHatTung = 1f;
            bool biHat = false; string nhac = null;
            float hanH2 = Time.time + 1f;
            while (Time.time < hanH2)
            {
                maxH2 = Mathf.Max(maxH2, DemCauLua() - truocH2);
                if (!biHat && toi.GetComponent<BiHatTung>() != null)
                {
                    biHat = true;
                    toi.mana = toi.maxMana;
                    yield return new WaitForSeconds(0.45f);   // het hoi chieu cau lua, van dang bay
                    if (toi.GetComponent<BiHatTung>() != null) { toi.CastAt(0, goc + huong * 10f); nhac = toi.LastMessage; }
                }
                yield return null;
            }
            Ghi(string.Format("H2. loc that trung nguoi dang niem: bi hat {0}, qua cau lua bay ra {1}; bam ky nang luc dang bay -> \"{2}\"", biHat, maxH2, nhac));
            Kiem(biHat && maxH2 == 0, "loc that trung nguoi dang niem ma chieu van ra");
            XoaLoc();
            yield return new WaitForSeconds(0.8f);

            // H3: QUAI dang ra don bi hat -> khong trung; doi chung -> trung
            var goQuai = EnemyFactory.Spawn(MonsterType.Skeleton, goc + huong * 1.6f, null, toi.transform);
            var ai = goQuai != null ? goQuai.GetComponent<EnemyAI>() : null;
            if (ai == null) { Ghi("[LOI] khong sinh duoc bo xuong"); loi++; }
            else
            {
                var mauQuai = goQuai.GetComponent<Damageable>();
                mauQuai.maxHealth = 1e7f; mauQuai.health = 1e7f;
                int donNgat = 0, donDoiChung = 0, lanNgat = 0, lanDoi = 0;
                int dongNgat0 = EnemyAI.SoLanNgatDon;
                for (int lan = 0; lan < 10; lan++)
                {
                    bool hat = lan % 2 == 0;
                    float hanQ = Time.time + 6f;
                    while (!ai.DangRaDonChuaTrung && Time.time < hanQ) yield return null;
                    if (!ai.DangRaDonChuaTrung) continue;
                    int soDon = 0;
                    System.Action<EnemyAI, int, Transform, Vector3> nghe = (e, k, t, p) => soDon++;
                    ai.DaRaDon += nghe;
                    float mTruoc = mauToi.health;
                    if (hat) BiHatTung.Apply(mauQuai, 0.5f);
                    yield return new WaitForSeconds(0.9f);
                    ai.DaRaDon -= nghe;
                    bool trung = soDon > 0 || mauToi.health < mTruoc - 0.1f;
                    if (hat) { lanNgat++; if (trung) donNgat++; } else { lanDoi++; if (trung) donDoiChung++; }
                }
                Ghi(string.Format("H3. bo xuong dang vung tay bi hat tung: {0}/{1} don van trung (bo dem ngat {2}); doi chung khong hat: {3}/{4} don trung",
                    donNgat, lanNgat, EnemyAI.SoLanNgatDon - dongNgat0, donDoiChung, lanDoi));
                Kiem(lanNgat >= 4 && donNgat == 0, "quai bi hat tung ma don van trung");
                Kiem(lanDoi >= 4 && donDoiChung == lanDoi, "doi chung: quai ra don ma khong trung - phep do vo nghia");
                Object.Destroy(goQuai);
            }
            mauToi.health = mauToi.maxHealth;
            yield return new WaitForSeconds(0.5f);
        }

        // ================= J. LO LUA (bat dau - kiem tra lan cuoi o cuoi phep thu) =================
        Ghi("");
        LoLuaDa loThu = null, loDoiChung = null;
        float lucDapTat = -1f;
        {
            var cacLo = Object.FindObjectsByType<LoLuaDa>(FindObjectsInactive.Exclude);
            float ganNhat = 1e9f;
            foreach (var lo in cacLo)
            {
                float kc = Vector3.Distance(lo.transform.position, goc);
                if (lo.DangChay && kc < ganNhat) { ganNhat = kc; loThu = lo; }
            }
            if (loThu != null)
                foreach (var lo in cacLo)
                    if (lo != loThu && lo.DangChay && Vector3.Distance(lo.transform.position, loThu.transform.position) > 15f) { loDoiChung = lo; break; }
            if (loThu == null || loDoiChung == null) { Ghi("[LOI] Act2 khong co du lo lua dang chay"); loi++; }
            else
            {
                var h = Vector3.forward;
                var loc = GioLoc.Spawn(loThu.transform.position - h * 8f, h, maskEnemy);
                float hanJ = Time.time + 1.2f;
                while (Time.time < hanJ && loThu.DangChay) yield return null;
                lucDapTat = loThu.DangChay ? -1f : Time.time;
                Ghi(string.Format("J. loc luot qua lo {0}: lua tat {1}; lo doi chung {2} van chay {3}", loThu.name, !loThu.DangChay, loDoiChung.name, loDoiChung.DangChay));
                Kiem(!loThu.DangChay, "loc luot qua ma lo lua khong tat");
                Kiem(loDoiChung.DangChay, "lo doi chung o xa cung bi tat");
                yield return new WaitForSeconds(1.2f);
                if (loc != null) Object.Destroy(loc.gameObject);
            }
        }

        // ================= I. QUA MANG =================
        Ghi("");
        TranHienTai.DangChoiMang = true;
        var vuongI = Vector3.Cross(Vector3.up, huong).normalized;
        var kia = NguoiChoiKhac.Sinh("uid-gioloc", "Người bị hất", goc + vuongI * 6f);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi kia"); loi++; }
        else
        {
            var mauKia = kia.GetComponent<Damageable>();
            var goDb = new GameObject("TAM_DongBoGioLoc");
            var db = goDb.AddComponent<DongBoTran>();
            db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, kia);
            KenhTrucTiep.Tao(); KenhTrucTiep.NhanTraLoi("{}");
            var chay = Object.FindAnyObjectByType<ChayThuMang>();
            if (chay != null) chay.StartCoroutine(GiuSong());
            var daGui = new List<string>();
            KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);
            yield return new WaitForSeconds(0.6f);
            System.Action<byte> nhetCo = co =>
            {
                var ds = new GoiTin.MotNguoi[1];
                ds[0] = new GoiTin.MotNguoi { chiSo = 1, viTri = kia.transform.position, gocY = kia.transform.eulerAngles.y, mau01 = 1f, coHieuUng = co };
                KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
            };

            // I1. goi ky nang 10
            int truocMang = DemLoc(), maxLoc = 0;
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            { chiSo = 1, kyNang = (byte)K, capKyNang = 1, soThuTu = 900, diemNgam = kia.transform.position + vuongI * 10f })));
            float hanI = Time.time + 1f;
            while (Time.time < hanI) { maxLoc = Mathf.Max(maxLoc, DemLoc() - truocMang); nhetCo(0); yield return null; }
            yield return new WaitForSeconds(0.5f);
            daGui.Clear();
            toi.mana = toi.maxMana;
            toi.CastAt(K, goc + huong * 8f);
            yield return null; yield return null;
            bool goi10 = false;
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s); GoiTin.MotPhep p;
                if (b != null && GoiTin.LoaiCuaGoi(b) == GoiTin.LoaiKyNang && GoiTin.DocKyNang(b, out p) && p.kyNang == K) goi10 = true;
            }
            Ghi(string.Format("I1. goi ky nang so 10 tu nguoi kia -> may minh phat lai {0} loc (mong 1); minh tung -> goi mang kyNang = 10: {1}", maxLoc, goi10));
            Kiem(maxLoc == 1, "may minh khong phat lai Gio loc cua nguoi kia (1 loc)");
            Kiem(goi10, "goi ky nang khong mang so 10");

            // I1b. nguoi kia tung Gio loc CAP 5 (cap di kem goi) -> may minh ra 2 loc song song
            XoaLoc();
            yield return new WaitForSeconds(0.5f);
            int truocMang5 = DemLoc(), maxLoc5 = 0;
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            { chiSo = 1, kyNang = (byte)K, capKyNang = 5, soThuTu = 950, diemNgam = kia.transform.position + vuongI * 10f })));
            float hanI5 = Time.time + 1f;
            while (Time.time < hanI5) { maxLoc5 = Mathf.Max(maxLoc5, DemLoc() - truocMang5); nhetCo(0); yield return null; }
            Ghi(string.Format("I1b. goi ky nang cap 5 tu nguoi kia -> may minh phat lai {0} loc (mong 2; goi cap 1 o tren ra 1)", maxLoc5));
            Kiem(maxLoc5 == 2, "may minh khong phat lai 2 loc cua nguoi kia cap 5");
            XoaLoc();
            yield return new WaitForSeconds(0.3f);
            XoaLoc();
            yield return new WaitForSeconds(0.5f);

            // I2. minh bi hat -> goi trang thai minh gui di co bit hat tung
            daGui.Clear();
            BiHatTung.Apply(mauToi, 0.5f);
            float hanI2 = Time.time + 0.3f;
            while (Time.time < hanI2) { nhetCo(0); yield return null; }
            bool goiCoBit = false;
            var ra = new GoiTin.MotNguoi[4];
            foreach (var s in daGui)
            {
                var b = GoiTin.TuChuoi(s);
                if (b == null || GoiTin.LoaiCuaGoi(b) != GoiTin.LoaiTrangThai) continue;
                int moc; int n = GoiTin.DocTrangThai(b, ra, out moc);
                for (int i = 0; i < n; i++) if (ra[i].chiSo == 0 && (ra[i].coHieuUng & HieuUngQuaMang.CoHatTung) != 0) goiCoBit = true;
            }
            // Mat na ca hai goi: viet 0x1F doc ra 0x1F
            var ds1 = new GoiTin.MotNguoi[] { new GoiTin.MotNguoi { chiSo = 2, coHieuUng = 0x1F, mau01 = 1f } };
            var ra1 = new GoiTin.MotNguoi[1]; int mc;
            GoiTin.DocTrangThai(GoiTin.VietTrangThai(1, ds1, 1), ra1, out mc);
            var dq = new GoiTin.MotQuai[] { new GoiTin.MotQuai { id = 5, coHieuUng = 0x1F, mau01 = 1f } };
            var rq = new GoiTin.MotQuai[1];
            GoiTin.DocQuai(GoiTin.VietQuai(1, dq, 0, 1), rq, out mc);
            Ghi(string.Format("I2. minh bi hat -> goi trang thai gui di co bit hat tung: {0}; mat na: goi nguoi choi 0x1F -> 0x{1:X2}, goi quai 0x1F -> 0x{2:X2} (co chet quai {3})",
                goiCoBit, ra1[0].coHieuUng, rq[0].coHieuUng, rq[0].daChet));
            Kiem(goiCoBit, "bi hat tung ma goi trang thai khong bao cho may kia");
            Kiem(ra1[0].coHieuUng == 0x1F && rq[0].coHieuUng == 0x1F && !rq[0].daChet, "mat na goi tin cat mat bit hat tung");
            yield return new WaitForSeconds(0.6f);

            // I3. ban sao nhan bit -> bay len; goi tre sau khi roi xuong khong hat lai
            nhetCo(HieuUngQuaMang.CoHatTung);
            bool coHtKia = false;
            float caoKia = 0f;
            float hanI3 = Time.time + 1.2f;
            float lucHet = -1f;
            while (Time.time < hanI3)
            {
                var h = kia.GetComponent<BiHatTung>();
                if (h != null) { coHtKia = true; caoKia = Mathf.Max(caoKia, h.CaoHienTai); }
                if (h == null && coHtKia && lucHet < 0f) { lucHet = Time.time; nhetCo(HieuUngQuaMang.CoHatTung); }   // goi tre
                yield return null;
                nhetCo(0);
            }
            bool hatLaiLanHai = kia.GetComponent<BiHatTung>() != null;
            Ghi(string.Format("I3. ban sao nhan bit hat tung: bay len {0}, cao nhat {1:F2} m; goi 'dang bay' den tre ngay sau khi roi xuong -> hat lai lan hai: {2}",
                coHtKia, caoKia, hatLaiLanHai));
            Kiem(coHtKia && caoKia > 1.3f, "ban sao khong bay len khi nhan bit hat tung");
            Kiem(!hatLaiLanHai, "goi tre lam ban sao bi hat lai lan hai");
            yield return new WaitForSeconds(0.5f);

            // I4. ban sao dang niem Qua cau lua nhan bit -> khong phong; doi chung -> 3 qua
            int ngatBanSao = 0, raDoiChung = 0;
            for (int lan = 0; lan < 3; lan++)
            {
                for (int ca = 0; ca < 2; ca++)
                {
                    bool coBit = ca == 0;
                    int truoc = DemCauLua(), maxMoi = 0;
                    KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
                    { chiSo = 1, kyNang = 0, capKyNang = 1, soThuTu = 950 + lan * 2 + ca, diemNgam = kia.transform.position + vuongI * 12f })));
                    yield return null;
                    if (coBit) nhetCo(HieuUngQuaMang.CoHatTung);
                    float hanI4 = Time.time + 1f;
                    while (Time.time < hanI4) { maxMoi = Mathf.Max(maxMoi, DemCauLua() - truoc); yield return null; nhetCo(0); }
                    if (coBit && maxMoi == 0) ngatBanSao++;
                    if (!coBit && maxMoi == 3) raDoiChung++;
                    yield return new WaitForSeconds(0.6f);
                }
            }
            Ghi(string.Format("I4. ban sao dang niem Qua cau lua nhan bit hat tung: {0}/3 lan khong qua nao; doi chung khong bit: {1}/3 lan ra du 3 qua", ngatBanSao, raDoiChung));
            Kiem(ngatBanSao == 3 && raDoiChung == 3, "ban sao bi hat tung ma van phong phep");

            KenhTrucTiep.guiSangBenKia = null;
            KenhTrucTiep.Dong();
            Object.DestroyImmediate(goDb);
            NguoiChoiKhac.Bo(kia);
        }
        TranHienTai.DangChoiMang = false;

        // ================= J. LO LUA CHAY LAI =================
        if (loThu != null && lucDapTat > 0f)
        {
            while (Time.time < lucDapTat + 25f) yield return null;
            bool tat25 = !loThu.DangChay;
            while (Time.time < lucDapTat + 31f) yield return null;
            bool chay31 = loThu.DangChay;
            Ghi(string.Format("J. lo {0}: 25 giay sau van tat {1}; 31 giay sau chay lai {2}", loThu.name, tat25, chay31));
            Kiem(tat25 && chay31, "lo lua khong chay lai dung sau 30 giay");
        }

        // ================= M. CAP 5: HAI LOC SONG SONG =================
        Ghi("");
        {
            XoaLoc();
            yield return new WaitForSeconds(0.5f);
            // Len cap nhan vat de co diem, nang Gio loc toi cap 4 (doi chung) roi 5
            int solan = 0;
            while (CapDo.CapCuaKyNang(K) < 4 && solan++ < 20)
            {
                if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
                CapDo.NangCap(K);
            }
            Vector3 hM = HuongTrong(toi);
            toi.transform.rotation = Quaternion.LookRotation(hM);
            Vector3 gM = toi.transform.position;
            // cap 4: doi chung 1 loc
            toi.mana = toi.maxMana; float mn4 = toi.mana;
            var truoc4 = new HashSet<GioLoc>(Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude));
            toi.CastAt(K, gM + hM * 10f);
            float ton4 = mn4 - toi.mana;
            yield return new WaitForSeconds(0.6f);
            int soLoc4 = 0;
            foreach (var l in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!truoc4.Contains(l)) soLoc4++;
            XoaLoc();
            yield return new WaitForSeconds(0.3f);
            if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            CapDo.NangCap(K);
            int cap5 = CapDo.CapCuaKyNang(K);
            // bia dung giua hai duong bay (lech 0 m), 6 m truoc mat
            var biaM = TaoBia("TAM_BiaGiuaHaiLoc", gM + hM * 7f);
            yield return new WaitForFixedUpdate();
            float mauM = biaM.health;
            toi.mana = toi.maxMana; float mn5 = toi.mana;
            var truoc5 = new HashSet<GioLoc>(Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude));
            toi.CastAt(K, gM + hM * 10f);
            float ton5 = mn5 - toi.mana;
            var hai = new List<GioLoc>();
            float hanM = Time.time + 1.2f;
            while (Time.time < hanM && hai.Count < 2)
            {
                foreach (var l in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!truoc5.Contains(l) && !hai.Contains(l)) hai.Add(l);
                yield return null;
            }
            yield return null;
            float cach0 = -1f, cach1 = -1f, dotHuong = 0f, lechDoc = 99f;
            if (hai.Count == 2 && hai[0] != null && hai[1] != null)
            {
                Vector3 d = hai[1].transform.position - hai[0].transform.position; d.y = 0f;
                cach0 = d.magnitude;
                lechDoc = Mathf.Abs(Vector3.Dot(d, hM));             // theo huong bay ~0 = xep ngang
                dotHuong = Vector3.Dot(hai[0].dir.normalized, hai[1].dir.normalized);
                yield return new WaitForSeconds(0.35f);
                yield return Chup("gioloc_4_cap5_hai_loc");
                yield return new WaitForSeconds(0.65f);
                if (hai[0] != null && hai[1] != null)
                {
                    Vector3 d1 = hai[1].transform.position - hai[0].transform.position; d1.y = 0f;
                    cach1 = d1.magnitude;
                }
            }
            yield return new WaitForSeconds(1.2f);
            float matM = mauM - biaM.health;
            float mongTon4 = 20f * Mathf.Pow(1.1f, 3), mongTon5 = 20f * Mathf.Pow(1.1f, 4) * 2f, mongMat = 75f * Mathf.Pow(1.2f, 4) * 2f;
            Ghi(string.Format("M. cap 4 (doi chung): {0} loc, ton {1:F2} nang luong (mong {2:F2}); cap {3}: {4} loc, ton {5:F2} (mong {6:F2})",
                soLoc4, ton4, mongTon4, cap5, hai.Count, ton5, mongTon5));
            Ghi(string.Format("M. hai loc: tam cach nhau luc sinh {0:F2} m, sau 1 s {1:F2} m; lech theo huong bay {2:F3} m (0 = xep ngang); cos hai huong bay {3:F4}; bia giua hai duong bay mat {4:F1} (mong {5:F1} = trung ca hai)",
                cach0, cach1, lechDoc, dotHuong, matM, mongMat));
            Kiem(soLoc4 == 1 && Mathf.Abs(ton4 - mongTon4) < 0.05f, "doi chung cap 4 khong phai 1 loc / nang luong thuong");
            Kiem(cap5 == 5 && hai.Count == 2, "cap 5 khong ra dung 2 loc");
            Kiem(Mathf.Abs(ton5 - mongTon5) < 0.05f, "cap 5 khong ton gap doi nang luong");
            Kiem(Mathf.Abs(cach0 - 4f) < 0.05f && Mathf.Abs(cach1 - 4f) < 0.05f && lechDoc < 0.05f && dotHuong > 0.9999f, "hai loc khong song song cach nhau 4 m");
            // bia co 10 000 000 mau: so thuc float o do lon nay chi chinh xac toi 1 don vi, moi cu tru 155,52 thanh 156 -> hai loc 312
            Kiem(Mathf.Abs(matM - mongMat) < 1.1f, "bia giua khong bi ca hai loc trung");
            Object.Destroy(biaM.gameObject);
            XoaLoc();

        }

        toi.DaTungPhep -= dem;
        if (dir != null) dir.enabled = true;
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
        Debug.Log("[GioLoc] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/gioloc.txt", bao.ToString());
        foreach (var ten in new[] { "TAM_GioLoc", "TAM_DongBoGioLoc" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
