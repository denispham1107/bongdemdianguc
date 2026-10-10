using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 119 - NHAO LON (ky nang 22, nguoi dung 10/10/2026). Play Act2 that, tung bang PlayerController.CastAt.
///   A. CO SAN cap 1, toi da 1 cap, khong mo / nang duoc, nam trong nhom HO TRO, icon nap duoc (Read/Write).
///   B. QUANG DUONG (huong trong, khong vat can - tu do bang SphereCast): ngam Tam / 2 / Tam+4 / 0,3 m -> lan Tam / 2 / Tam / 0,8 m (do vi tri goc that
///      truoc / sau), thoi gian = NhaoLon.ThoiGianLan, khong tru mana, hoi chieu 4 giay (bam lai bi tu choi, 4,1 s sau duoc).
///   C. HINH (tung khung): goc lat tang deu 0 -> 360 (do tu XUONG DAU so voi hong, khong doc bien cua NhaoLon), buoc goc lon nhat moi khung
///      (mem = khong nhay), giua vong dau thap hon hong; CHAM DAT do bang BakeMesh: dinh thap nhat cach dat trong [-0,06; 0,22] m khi dang
///      cuon; het lan model ve dung tu the goc. Anh tung khung: PlayTestShots/nhaolon_*.png.
///   D. VAT CAN: hop 1 m o 2,5 m phia truoc -> dung truoc hop (khong xuyen nhu Toc bien).
///   E. CHAN: dang lan bam Qua cau lua -> tu choi; bi choang -> khong lan; bi danh nga GIUA vong lan -> nga xong model ve tu the dung goc.
///   F. BAN SAO MANG: TungPhepTheoMang tren ban sao (mau do may khac quyet) -> co hinh lan nhung KHONG tu di (vi tri tu goi tin).
/// </summary>
public static class ThuNhaoLon
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/119. Chay thu NHAO LON (ky nang 22)", false, 212)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[NhaoLon] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] nhao lon");
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
        if (GameObject.Find("TAM_NhaoLon") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_NhaoLon");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[NhaoLon] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }
    static float Ngang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    static void DatCho(Transform t, Vector3 p)
    {
        var cc = t.GetComponent<CharacterController>();
        bool bat = cc != null && cc.enabled;
        if (bat) cc.enabled = false;
        p.y = GioLoc.MatDatY(p, p.y) + 0.1f;
        t.position = p;
        if (bat) cc.enabled = true;
        Physics.SyncTransforms();
    }

    /// <summary>Tim cho + huong trong: 7 m phia truoc khong vat can (SphereCast 0,45 m o do cao 0,6 / 1,2), dat phang (chenh &lt; 0,6 m).</summary>
    static bool TimDuongTrong(Vector3 tam, out Vector3 cho, out Vector3 huong)
    {
        int mask = ~LayerMask.GetMask("Player", "Enemy", "Ignore Raycast", "Ground");
        for (int vong = 0; vong < 6; vong++)
            for (int g = 0; g < 360; g += 20)
            {
                float a = g * Mathf.Deg2Rad;
                var h = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var c = tam + new Vector3(Mathf.Cos(vong * 1.3f), 0f, Mathf.Sin(vong * 1.3f)) * (vong * 6f);
                c.y = GioLoc.MatDatY(c, c.y);
                bool trong = true;
                foreach (float cao in new[] { 0.6f, 1.2f })
                    if (Physics.SphereCast(c + Vector3.up * cao - h * 0.6f, 0.45f, h, out _, NhaoLon.Tam + 3f, mask, QueryTriggerInteraction.Ignore)) { trong = false; break; }
                if (!trong) continue;
                float d0 = c.y;
                bool phang = true;
                for (float s = 1f; s <= NhaoLon.Tam + 2.5f; s += 1f)
                {
                    var q = c + h * s;
                    if (Mathf.Abs(GioLoc.MatDatY(q, q.y) - d0) > 0.6f) { phang = false; break; }
                }
                if (!phang) continue;
                cho = c; huong = h; return true;
            }
        cho = tam; huong = Vector3.forward; return false;
    }

    static float DinhThapNhat(GameObject g, Mesh m)
    {
        float thap = float.MaxValue;
        foreach (var smr in g.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.BakeMesh(m);
            var tf = smr.transform;
            foreach (var v in m.vertices) thap = Mathf.Min(thap, tf.TransformPoint(v).y);
        }
        return thap;
    }

    static Transform TimXuong(Transform goc, string ten)
    {
        foreach (var x in goc.GetComponentsInChildren<Transform>()) if (x.name == ten) return x;
        return null;
    }

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 90 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    /// <summary>Tung that va doi lan xong; tra ve quang da di (ngang) va thoi gian.</summary>
    static IEnumerator LanMotLan(PlayerController pc, Vector3 ngam, float[] kq)
    {
        Vector3 truoc = pc.transform.position;
        float t0 = Time.time;
        pc.CastAt(CapDo.KyNhaoLon, ngam);
        float han = Time.time + 3f;
        // doi bat dau
        while (pc.GetComponent<NhaoLon>() == null && Time.time < han) yield return null;
        float tBat = Time.time;
        while (NhaoLon.Dang(pc.gameObject) && Time.time < han) yield return null;
        kq[0] = Ngang(truoc, pc.transform.position);
        kq[1] = Time.time - tBat;
        yield return new WaitForSeconds(0.1f);
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        yield return new WaitForSeconds(1.5f);
        if (dir == null) { Ghi("[LOI] khong co GameDirector"); loi++; Ket(); yield break; }
        dir.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(e.gameObject);
        var pc = dir.player.GetComponent<PlayerController>();
        var toi = dir.player.GetComponent<Damageable>();
        toi.maxHealth = toi.health = 1e7f;
        var rig = Object.FindAnyObjectByType<CameraRig>();

        // ===== A =====
        var cap = pc.Cap;
        bool trongNhom = false;
        foreach (var nhom in SachPhep.KyNangTheoNhom) if (System.Array.IndexOf(nhom, CapDo.KyNhaoLon) >= 0 && System.Array.IndexOf(nhom, CapDo.KyTocBien) >= 0) trongNhom = true;
        var icon = IconKyNang.BoDayDu()[CapDo.KyNhaoLon];
        Color giua = icon != null ? icon.GetPixel(150, 120) : Color.black;
        Ghi(string.Format("A. so ky nang {0}; cap Nhao lon dau tran {1}/{2}; mo khoa duoc {3}, nang cap duoc {4}; nhom HO TRO {5}; ten \"{6}\"; icon {7} (diem giua {8})",
            CapDo.SoKyNang, cap.CapCuaKyNang(CapDo.KyNhaoLon), CapDo.CapToiDaCua(CapDo.KyNhaoLon), cap.MoKhoaDuoc(CapDo.KyNhaoLon),
            cap.NangCapDuoc(CapDo.KyNhaoLon), trongNhom, SachPhep.Ten(CapDo.KyNhaoLon), icon != null ? icon.width + "x" + icon.height : "null", giua));
        Kiem(CapDo.SoKyNang == 23 && cap.CapCuaKyNang(CapDo.KyNhaoLon) == 1 && CapDo.CapToiDaCua(CapDo.KyNhaoLon) == 1, "Nhao lon khong co san cap 1 / toi da 1");
        Kiem(!cap.MoKhoaDuoc(CapDo.KyNhaoLon) && !cap.NangCapDuoc(CapDo.KyNhaoLon) && trongNhom && SachPhep.Ten(CapDo.KyNhaoLon) == "NHÀO LỘN", "Nhao lon sai nhom / mo khoa / ten");

        // ===== B: quang duong =====
        Vector3 cho, huong;
        if (!TimDuongTrong(dir.player.position, out cho, out huong)) { Ghi("[LOI] khong tim duoc duong trong"); loi++; Ket(); yield break; }
        float[] kq = new float[2];
        float[] ngams = { NhaoLon.Tam, 2f, NhaoLon.Tam + 4f, 0.3f }, mong = { NhaoLon.Tam, 2f, NhaoLon.Tam, NhaoLon.QuangToiThieu };
        for (int i = 0; i < ngams.Length; i++)
        {
            DatCho(pc.transform, cho);
            yield return new WaitForSeconds(0.25f);
            pc.mana = 100f;
            float mana0 = pc.mana;
            // het hoi chieu cua lan truoc
            while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
            Vector3 ngam = cho + huong * ngams[i];
            ngam.y = GioLoc.MatDatY(ngam, ngam.y);
            yield return LanMotLan(pc, ngam, kq);
            float tgMong = NhaoLon.ThoiGianLan(mong[i]);
            Ghi(string.Format("B{0}. ngam {1} m -> lan {2:F2} m (mong {3}), {4:F2} s (mong {5:F2}); mana {6:F1} -> {7:F1}; hoi chieu con {8:F2} s",
                i + 1, ngams[i], kq[0], mong[i], kq[1], tgMong, mana0, pc.mana, pc.HoiChieuGiay(CapDo.KyNhaoLon)));
            Kiem(Mathf.Abs(kq[0] - mong[i]) < 0.2f, "quang duong lan sai (ngam " + ngams[i] + " m)");
            Kiem(Mathf.Abs(kq[1] - tgMong) < 0.1f, "thoi gian lan sai");
            Kiem(pc.mana >= mana0 - 0.01f, "nhao lon tru nang luong");
            if (i == 0)
            {
                // bam lai ngay -> hoi chieu tu choi; doi het 4 s -> duoc
                Vector3 p0 = pc.transform.position;
                pc.CastAt(CapDo.KyNhaoLon, p0 + huong * 3f);
                yield return new WaitForSeconds(0.3f);
                bool tuChoi = NhaoLon.Dang(pc.gameObject) == false && Ngang(p0, pc.transform.position) < 0.05f;
                Ghi("    bam lai ngay: tu choi = " + tuChoi + " (\"" + pc.LastMessage + "\"), hoi chieu con " + pc.HoiChieuGiay(CapDo.KyNhaoLon).ToString("F2") + " s");
                Kiem(tuChoi && pc.LastMessage == "NHÀO LỘN đang hồi chiêu", "hoi chieu 4 giay khong chan");
            }
        }

        // ===== C: hinh tung khung =====
        DatCho(pc.transform, cho);
        if (rig != null) rig.enabled = false;
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 benPhai = Vector3.Cross(Vector3.up, huong).normalized;
            Vector3 giuaDuong = cho + huong * (NhaoLon.Tam * 0.5f) + Vector3.up * 0.9f;
            cam.transform.position = giuaDuong - benPhai * 9.5f + Vector3.up * 1.4f;
            cam.transform.LookAt(giuaDuong);
        }
        while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
        yield return new WaitForSeconds(0.3f);
        var hh = pc.GetComponent<NguoiChoiHoatHinh>();
        Transform hinh = null;
        foreach (Transform c in pc.transform) if (c.GetComponent<Animation>() != null) { hinh = c; break; }
        Vector3 hinhPos0 = hinh.localPosition; Quaternion hinhRot0 = hinh.localRotation;
        var mBake = new Mesh();
        float datGoc = GioLoc.MatDatY(cho, cho.y);
        Ghi(string.Format("C0. dung yen: dinh thap nhat cach dat {0:F3} m", DinhThapNhat(pc.gameObject, mBake) - datGoc));

        // Dong ho game CO DINH 60 khung / giay: Editor cham (~15 khung/giay) thi ca vong lan chi lay duoc 9 mau, khong do duoc do muot
        Time.captureDeltaTime = 1f / 60f;
        pc.CastAt(CapDo.KyNhaoLon, cho + huong * NhaoLon.Tam);
        var goc = new List<float>(); var khe = new List<float>(); int dauDuoiHong = 0, soKhung = 0, soAnh = 0;
        float gocTruoc = 0f, gocCong = 0f, buocLonNhat = 0f;
        Vector3 phai = Vector3.Cross(Vector3.up, huong).normalized;
        han = Time.time + 3f;
        while (pc.GetComponent<NhaoLon>() == null && Time.time < han) yield return null;
        var nl = pc.GetComponent<NhaoLon>();
        while (nl != null && nl.DangLan && Time.time < han)
        {
            yield return new WaitForEndOfFrame();
            if (nl == null || !nl.DangLan) break;
            soKhung++;
            // goc lat do tu xuong: vector hong -> dau chieu len mat phang (huong, len)
            Vector3 v = hh.head.position - hh.hips.position;
            float a = Mathf.Atan2(Vector3.Dot(v, huong), Vector3.Dot(v, Vector3.up)) * Mathf.Rad2Deg;   // 0 = dau tren, 90 = dau truoc
            if (soKhung > 1) { float d = Mathf.DeltaAngle(gocTruoc, a); gocCong += d; buocLonNhat = Mathf.Max(buocLonNhat, Mathf.Abs(d)); }
            gocTruoc = a;
            if (nl.CuonHienTai > 0.95f) goc.Add(gocCong);      // chi luc cuon het: luc duoi nguoi dau ngang len lam goc do lui lai
            if (hh.head.position.y < hh.hips.position.y) dauDuoiHong++;
            float dat = GioLoc.MatDatY(pc.transform.position, pc.transform.position.y);
            if (nl.CuonHienTai > 0.6f) khe.Add(DinhThapNhat(pc.gameObject, mBake) - dat);
        }
        yield return null; yield return null;
        // Luot rieng de CHUP ANH (chup chen giua luot do lam lech dong ho)
        while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
        DatCho(pc.transform, cho);
        yield return new WaitForSeconds(0.3f);
        pc.CastAt(CapDo.KyNhaoLon, cho + huong * NhaoLon.Tam);
        float hanAnh = Time.time + 3f;
        while (pc.GetComponent<NhaoLon>() == null && Time.time < hanAnh) yield return null;
        var nlAnh = pc.GetComponent<NhaoLon>();
        int khungAnh = 0;
        while (nlAnh != null && nlAnh.DangLan && Time.time < hanAnh && soAnh < 10)
        {
            yield return new WaitForEndOfFrame();
            if (khungAnh++ % 4 == 0) { yield return Chup("nhaolon_" + soAnh); soAnh++; }
        }
        Time.captureDeltaTime = 0f;
        yield return new WaitForSeconds(0.2f);
        float kheMin = 1e9f, kheMax = -1e9f; foreach (var k in khe) { kheMin = Mathf.Min(kheMin, k); kheMax = Mathf.Max(kheMax, k); }
        bool donDieu = true; for (int i = 1; i < goc.Count; i++) if (goc[i] < goc[i - 1] - 3f) donDieu = false;
        float tbBuoc = soKhung > 1 ? gocCong / (soKhung - 1) : 0f;
        bool traVe = Vector3.Distance(hinh.localPosition, hinhPos0) < 0.01f && Quaternion.Angle(hinh.localRotation, hinhRot0) < 0.5f;
        Ghi(string.Format("C. {0} khung: goc lat (xuong hong->dau) cong {1:F0} do, don dieu {2}, buoc lon nhat {3:F1} do/khung (TB {4:F1}); dau duoi hong {5} khung",
            soKhung, gocCong, donDieu, buocLonNhat, tbBuoc, dauDuoiHong));
        Ghi(string.Format("    cham dat khi cuon (BakeMesh, {0} khung): dinh thap nhat cach dat {1:F3} .. {2:F3} m; het lan model ve goc: {3}", khe.Count, kheMin, kheMax, traVe));
        Kiem(gocCong > 300f && gocCong < 400f && donDieu, "khong lat du mot vong 360 do / lat nguoc chieu");
        Kiem(soKhung >= 25 && buocLonNhat < Mathf.Max(2.5f * tbBuoc, 25f), "goc lat nhay giat (khong mem)");
        Kiem(dauDuoiHong >= 3, "giua vong lan dau khong xuong duoi hong");
        Kiem(khe.Count >= 3 && kheMin > -0.05f && kheMax < 0.15f, "qua bong lo lung / chui xuong dat khi cuon");
        Kiem(traVe, "het lan model khong ve tu the dung goc");
        if (rig != null) rig.enabled = true;

        // ===== C9: DO BANG KHE (khe xuong thap nhat -> dinh luoi thap nhat theo goc lat, cuon het) - in ra de ghi cung vao NhaoLon.BangKhe =====
        {
            while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
            DatCho(pc.transform, cho);
            yield return new WaitForSeconds(0.3f);
            var tong = new float[25]; var dem = new int[25];
            Time.captureDeltaTime = 1f / 400f;            // lay mau day: ~240 khung trong mot vong lan
            pc.CastAt(CapDo.KyNhaoLon, cho + huong * NhaoLon.Tam);
            float hanK = Time.time + 3f;
            while (pc.GetComponent<NhaoLon>() == null && Time.time < hanK) yield return null;
            var nk = pc.GetComponent<NhaoLon>();
            while (nk != null && nk.DangLan && Time.time < hanK)
            {
                yield return new WaitForEndOfFrame();
                if (nk == null || !nk.DangLan) break;
                if (nk.CuonHienTai < 0.98f) continue;
                float kheDo = nk.XuongThapNhatY - DinhThapNhat(pc.gameObject, mBake);
                int b = Mathf.Clamp(Mathf.RoundToInt(Mathf.Repeat(nk.GocHienTai, 360f) / 15f), 0, 24);
                tong[b] += kheDo; dem[b]++;
            }
            Time.captureDeltaTime = 0f;
            var sb = new StringBuilder("C9. bang khe (cuon het, moi 15 do): { ");
            for (int i = 0; i < 25; i++) sb.Append(dem[i] > 0 ? (tong[i] / dem[i]).ToString("F3") + "f" : "-").Append(i < 24 ? ", " : " }");
            Ghi(sb.ToString());
            var sbn = new StringBuilder("    so mau moi o: ");
            for (int i = 0; i < 25; i++) sbn.Append(dem[i]).Append(' ');
            Ghi(sbn.ToString());
        }

        // ===== D: vat can =====
        while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
        DatCho(pc.transform, cho);
        yield return new WaitForSeconds(0.2f);
        var hop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hop.name = "TAM_HopChan"; hop.transform.localScale = new Vector3(3f, 2.5f, 0.6f);
        hop.transform.position = cho + huong * 2.5f + Vector3.up * 1.0f;
        hop.transform.rotation = Quaternion.LookRotation(huong);
        Physics.SyncTransforms();
        yield return LanMotLan(pc, cho + huong * NhaoLon.Tam, kq);
        float denMat = 2.5f - 0.3f;
        Ghi(string.Format("D. hop chan o {0:F1} m (mat truoc {1:F1} m): lan duoc {2:F2} m", 2.5f, denMat, kq[0]));
        Kiem(kq[0] < denMat + 0.05f && kq[0] > 1.0f, "nhao lon xuyen qua vat can / khong lan");
        Object.Destroy(hop);
        yield return null;

        // ===== E: chan =====
        while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
        DatCho(pc.transform, cho);
        CapDo.MoCaDuongChoPhepThu(0);
        yield return new WaitForSeconds(0.2f);
        pc.CastAt(CapDo.KyNhaoLon, cho + huong * NhaoLon.Tam);
        han = Time.time + 1f;
        while (!NhaoLon.Dang(pc.gameObject) && Time.time < han) yield return null;
        pc.mana = 100f;
        int cauTruoc = Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None).Length;
        pc.CastAt(0, pc.transform.position + huong * 8f);
        string nhacLan = pc.LastMessage;
        // bi danh nga giua vong lan
        yield return new WaitForSeconds(0.2f);
        bool dangLanLucNga = NhaoLon.Dang(pc.gameObject);
        BiDanhNga.Apply(toi, 1.0f);
        yield return new WaitForSeconds(0.1f);
        bool hetLan = !NhaoLon.Dang(pc.gameObject);
        yield return new WaitForSeconds(1.3f);
        bool hinhGoc = Vector3.Distance(hinh.localPosition, hinhPos0) < 0.01f && Quaternion.Angle(hinh.localRotation, hinhRot0) < 0.5f;
        Ghi(string.Format("E1. dang lan bam Qua cau lua -> \"{0}\"; bi danh nga giua vong (dang lan {1}) -> dung lan {2}; nga xong model ve dung goc {3}",
            nhacLan, dangLanLucNga, hetLan, hinhGoc));
        Kiem(nhacLan == "Đang nhào lộn!", "dang lan van tung phep khac");
        Kiem(dangLanLucNga && hetLan && hinhGoc, "bi nga giua vong lan: khong dung lan / model ket tu the lat");
        // bi choang -> khong lan
        while (pc.HoiChieuGiay(CapDo.KyNhaoLon) > 0f) yield return null;
        StunnedEffect.Apply(toi, 1.0f);
        yield return null;
        Vector3 pChoang = pc.transform.position;
        pc.CastAt(CapDo.KyNhaoLon, pChoang + huong * NhaoLon.Tam);
        yield return new WaitForSeconds(0.5f);
        Ghi(string.Format("E2. bi choang: \"{0}\", di {1:F2} m", pc.LastMessage, Ngang(pChoang, pc.transform.position)));
        Kiem(pc.LastMessage == "BẠN ĐANG BỊ CHOÁNG!" && Ngang(pChoang, pc.transform.position) < 0.05f, "bi choang van nhao lon");
        yield return new WaitForSeconds(0.8f);

        // ===== F: ban sao mang =====
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Sorceress.prefab");
        var saoGo = Object.Instantiate(pf, cho + phai * 3f, Quaternion.LookRotation(huong));
        saoGo.name = "TAM_BanSao";
        var sao = saoGo.GetComponent<PlayerController>();
        sao.tuDocInput = false;
        var saoMau = saoGo.GetComponent<Damageable>(); saoMau.mauDoMayKhacQuyet = true;
        yield return new WaitForSeconds(0.3f);
        Vector3 sao0 = saoGo.transform.position;
        sao.TungPhepTheoMang(CapDo.KyNhaoLon, sao0 + huong * 5f, 0f, 1);
        float gocSaoMax = 0f; bool coLan = false;
        han = Time.time + 1.5f;
        var hhSao = saoGo.GetComponent<NguoiChoiHoatHinh>();
        while (Time.time < han)
        {
            yield return new WaitForEndOfFrame();
            var n2 = saoGo.GetComponent<NhaoLon>();
            if (n2 != null) { coLan = true; gocSaoMax = Mathf.Max(gocSaoMax, n2.GocHienTai); }
        }
        Ghi(string.Format("F. ban sao: co hinh lan {0} (goc lon nhat {1:F0}), tu di {2:F2} m (phai 0 - vi tri tu goi tin)", coLan, gocSaoMax, Ngang(sao0, saoGo.transform.position)));
        Kiem(coLan && gocSaoMax > 300f && Ngang(sao0, saoGo.transform.position) < 0.05f, "ban sao mang khong lan hinh / tu di chuyen");
        Object.Destroy(saoGo);

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        Time.captureDeltaTime = 0f;
        File.WriteAllText("PlayTestShots/nhaolon.txt", bao.ToString());
        var rac = GameObject.Find("TAM_NhaoLon");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
