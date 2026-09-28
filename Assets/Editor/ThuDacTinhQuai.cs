using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: DAC TINH MOI CUA BA LOAI QUAI (menu 91, nguoi dung 28/09/2026).
///   A. BO XUONG: toc do x1,35 va sat thuong x1,30 so voi PREFAB goc (doc thang prefab, khong doc hang so); do THAT: chay
///      duoi nguoi choi bao nhieu m/s, danh trung mat bao nhieu mau. Doi chung: Phu thuy (khong doi).
///   B. QUY CAY: 15% choang 1 giay - phong 200 tia THAT vao nguoi choi, dem choang, doc so giay. Doi chung: tat ti le -> 0.
///   C. QUY DU: 15% danh nga 1 giay - doc thien thach THAT no sinh ra (ngaXacSuat / ngaGiay) + goi 40 qua that, dem nga.
///   D. BO XUONG DO DON 25%: 400 don ky nang (AreaDamage lua co chay) moi don mot khung: dem don khong mat mau; don do thi
///      KHONG dinh chay, don khong do thi co chay; hinh qua cau + chu. Doi chung: sat thuong RI (0 do), don cua QUAI (0 do),
///      Phu thuy (0 do); hieu ung khong kem don (hat tung) cung bi do ~25%.
///   E. GOI TIN: bit "vua do don" qua VietQuai / DocQuai giu nguyen, khong de len 6 bit hieu ung.
/// Ket qua: PlayTestShots/dac_tinh_quai.txt, anh dac_tinh_quai_do_don.png.
/// </summary>
public static class ThuDacTinhQuai
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/91. Chay thu DAC TINH QUAI (Quy cay choang, Quy du nga, Bo xuong do don)", false, 180)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_DacTinhQuai");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DacTinhQuai] " + s); }
    static void Kiem(bool dung, string loiNeuSai) { if (!dung) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static float DatY(Vector3 p)
    {
        var t = Terrain.activeTerrain;
        return t != null ? t.SampleHeight(p) + t.transform.position.y : 0f;
    }

    static void DatCho(Component c, Vector3 p)
    {
        var cc = c.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        c.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    static Vector3 TrenDat(Vector3 p) { p.y = DatY(p); return p; }

    static EnemyAI Sinh(MonsterType loai, Vector3 p, Transform muc, string ten)
    {
        var go = EnemyFactory.Spawn(loai, TrenDat(p), null, muc);
        go.name = ten;
        var ai = go.GetComponent<EnemyAI>();
        ai.enabled = false;
        ai.target = muc;
        var d = go.GetComponent<Damageable>();
        d.maxHealth = 100000f; d.health = 100000f;
        return ai;
    }

    static void DonDepHieuUng(GameObject go)
    {
        foreach (var c in go.GetComponents<StunnedEffect>()) Object.DestroyImmediate(c);
        foreach (var c in go.GetComponents<BiDanhNga>()) Object.DestroyImmediate(c);
        foreach (var c in go.GetComponents<BurningEffect>()) Object.DestroyImmediate(c);
        foreach (var c in go.GetComponents<BiHatTung>()) Object.DestroyImmediate(c);
    }

    static IEnumerator KichBan()
    {
        PlayerController pc = null;
        float han = Time.time + 25f;
        while (pc == null && Time.time < han) { pc = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        var cam = Camera.main;
        if (pc == null || cam == null || GameAssets.I == null) { Ghi("[LOI] thieu nhan vat / camera / GameAssets"); loi++; Ket(); yield break; }
        // Phep thu dai: tat GameDirector (dot quai 24 con giua chung tung lam GPU treo - xem bo nho)
        var dir = Object.FindAnyObjectByType<GameDirector>();
        if (dir != null) dir.enabled = false;
        var rig = cam.GetComponent<CameraRig>();
        if (rig != null) rig.enabled = false;
        var mauPc = pc.GetComponent<Damageable>();
        pc.enabled = false;

        Vector3 p = TrenDat(new Vector3(0f, 0f, -10.5f));
        DatCho(pc, p + Vector3.up * 0.05f);
        pc.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        yield return new WaitForSeconds(0.5f);

        // ================= A. BO XUONG: TOC DO + SAT THUONG =================
        {
            var pfXuong = GameAssets.EnemyPrefab(MonsterType.Skeleton).GetComponent<EnemyAI>();
            var pfPhu = GameAssets.EnemyPrefab(MonsterType.Witch).GetComponent<EnemyAI>();
            var bx = Sinh(MonsterType.Skeleton, p + new Vector3(12f, 0f, 0f), pc.transform, "TAM_BoXuong");
            var ph = Sinh(MonsterType.Witch, p + new Vector3(-12f, 0f, 6f), pc.transform, "TAM_PhuThuy");
            float rT = bx.moveSpeed / pfXuong.moveSpeed, rS = bx.attackDamage / pfXuong.attackDamage;
            float rT2 = ph.moveSpeed / pfPhu.moveSpeed, rS2 = ph.attackDamage / pfPhu.attackDamage;
            Ghi(string.Format("A1. Bo xuong: toc {0:F3} / prefab {1:F3} = x{2:F3}; sat thuong {3:F2} / prefab {4:F2} = x{5:F3}; do don {6:P0} | DOI CHUNG Phu thuy x{7:F3} / x{8:F3}, do don {9:P0}",
                bx.moveSpeed, pfXuong.moveSpeed, rT, bx.attackDamage, pfXuong.attackDamage, rS, bx.health.tiLeDoDon, rT2, rS2, ph.health.tiLeDoDon));
            Kiem(Mathf.Abs(rT - 1.35f) < 0.001f, "Bo xuong toc do khong phai x1,35");
            Kiem(Mathf.Abs(rS - 1.30f) < 0.001f, "Bo xuong sat thuong khong phai x1,30");
            Kiem(Mathf.Abs(bx.health.tiLeDoDon - 0.25f) < 1e-4f, "Bo xuong khong co 25% do don");
            Kiem(Mathf.Abs(rT2 - 1f) < 1e-4f && Mathf.Abs(rS2 - 1f) < 1e-4f && ph.health.tiLeDoDon == 0f, "Phu thuy bi doi theo (doi chung)");
            Object.Destroy(ph.gameObject);

            // A2. Chay THAT: bat AI, duoi nguoi choi tu 12 m
            bx.enabled = true;
            yield return new WaitForSeconds(0.4f);
            Vector3 a = bx.transform.position; float ta = Time.time;
            while (Time.time - ta < 1.2f) yield return null;
            Vector3 b = bx.transform.position;
            float v = Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z)) / (Time.time - ta);
            bx.enabled = false;
            Ghi(string.Format("A2. Bo xuong chay THAT {0:F2} m/s (moveSpeed {1:F3}; prefab goc {2:F2})", v, bx.moveSpeed, pfXuong.moveSpeed));
            Kiem(v > pfXuong.moveSpeed * 1.2f && v < bx.moveSpeed * 1.05f, "toc do chay that khong khop x1,35");

            // A3. Danh THAT: dung sat nguoi choi, ra don
            DatCho(bx, TrenDat(p + new Vector3(1.4f, 0f, 0f)));
            yield return null;
            float m0 = mauPc.health;
            bx.RaDonNgay();
            yield return null;
            float mat = m0 - mauPc.health;
            Ghi(string.Format("A3. Bo xuong danh trung nguoi choi: mat {0:F2} mau (mong {1:F2} = 12 x 1,30)", mat, pfXuong.attackDamage * 1.3f));
            Kiem(Mathf.Abs(mat - pfXuong.attackDamage * 1.3f) < 0.05f, "Bo xuong danh nguoi choi khong ra x1,30");
            Object.Destroy(bx.gameObject);
            mauPc.health = mauPc.maxHealth;
        }
        yield return null;

        // ================= B. QUY CAY: 15% CHOANG 1 GIAY =================
        {
            var qc = Sinh(MonsterType.QuyCay, p + new Vector3(0f, 0f, 6f), pc.transform, "TAM_QuyCay");
            qc.transform.rotation = Quaternion.LookRotation(pc.transform.position - qc.transform.position);
            Ghi(string.Format("B0. Quy cay: xacSuatChoangTia {0:F2}, giayChoangTia {1:F2}", qc.xacSuatChoangTia, qc.giayChoangTia));
            int soBan = 200, soTrung = 0, soChoang = 0; float giayMin = -1f, giayMax = -1f;
            for (int i = 0; i < soBan; i++)
            {
                DonDepHieuUng(pc.gameObject);
                float m0 = mauPc.health;
                float tBan = Time.time;
                qc.RaDonNgay();
                yield return null; yield return null; yield return null;
                if (mauPc.health < m0) soTrung++;
                var st = pc.GetComponent<StunnedEffect>();
                if (st != null && st.IsStunned)
                {
                    soChoang++;
                    if (giayMin < 0f)
                    {
                        // Lan choang dau: cho toi luc het. Choang bat dau giua tBan va luc thay -> thoi gian nam trong [het - thay; het - tBan]
                        float tThay = Time.time;
                        float tc2 = Time.time;
                        while (st != null && st.IsStunned && Time.time - tc2 < 3f) yield return null;
                        giayMin = Time.time - tThay; giayMax = Time.time - tBan;
                    }
                }
                mauPc.health = mauPc.maxHealth;
            }
            float tl = soTrung > 0 ? (float)soChoang / soTrung : 0f;
            Ghi(string.Format("B1. {0} tia THAT, trung {1}, choang {2} = {3:P1} (mong 15%, sai so ngau nhien ~2,5 diem); lan choang dau keo dai trong [{4:F2}; {5:F2}] s",
                soBan, soTrung, soChoang, tl, giayMin, giayMax));
            Kiem(soTrung >= soBan * 0.9f, "tia Quy cay khong trung nguoi choi - phep do vo nghia");
            Kiem(tl > 0.08f && tl < 0.23f, "ti le choang cua Quy cay khong phai ~15%");
            Kiem(giayMin <= 1.02f && giayMax >= 0.98f && giayMax < 1.4f, "choang cua Quy cay khong phai 1 giay");
            // Doi chung: tat ti le
            qc.xacSuatChoangTia = 0f;
            int choang0 = 0;
            for (int i = 0; i < 60; i++)
            {
                DonDepHieuUng(pc.gameObject);
                qc.RaDonNgay();
                yield return null; yield return null; yield return null;
                var st = pc.GetComponent<StunnedEffect>();
                if (st != null && st.IsStunned) choang0++;
                mauPc.health = mauPc.maxHealth;
            }
            Ghi("B2. DOI CHUNG ti le 0: 60 tia -> choang " + choang0);
            Kiem(choang0 == 0, "doi chung tat ti le ma van choang");
            DonDepHieuUng(pc.gameObject);
            Object.Destroy(qc.gameObject);
        }
        yield return null;

        // ================= C. QUY DU: 15% DANH NGA 1 GIAY =================
        {
            var qd = Sinh(MonsterType.QuyDu, p + new Vector3(0f, 0f, 8f), pc.transform, "TAM_QuyDu");
            // C1. Doc thien thach THAT no goi ra
            var truoc = new HashSet<ThienThach>(Object.FindObjectsByType<ThienThach>());
            qd.RaDonNgay();
            yield return null;
            ThienThach tt = null;
            foreach (var x in Object.FindObjectsByType<ThienThach>()) if (!truoc.Contains(x)) tt = x;
            Ghi(string.Format("C1. thien thach THAT cua Quy du: ngaXacSuat {0:F2}, ngaGiay {1:F2}", tt != null ? tt.ngaXacSuat : -1f, tt != null ? tt.ngaGiay : -1f));
            Kiem(tt != null && Mathf.Abs(tt.ngaXacSuat - 0.15f) < 1e-4f && Mathf.Abs(tt.ngaGiay - 1f) < 1e-4f, "thien thach Quy du khong mang 15% nga / 1 giay");
            float tc = Time.time; while (Time.time - tc < 1.6f) yield return null;
            // C2. 40 qua that roi trung nguoi choi, dem nga
            int soQua = 40, soNga = 0, soTrung = 0; float giayNga = -1f;
            for (int i = 0; i < soQua; i++)
            {
                DonDepHieuUng(pc.gameObject);
                mauPc.health = mauPc.maxHealth;
                float m0 = mauPc.health;
                qd.RaDonNgay();
                bool thayNga = false;
                float t0 = Time.time;
                while (Time.time - t0 < 1.5f)
                {
                    var ng = pc.GetComponent<BiDanhNga>();
                    if (ng != null && ng.DangNga && !thayNga) { thayNga = true; if (giayNga < 0f) giayNga = ng.thoiGian; }
                    yield return null;
                }
                if (mauPc.health < m0) soTrung++;
                if (thayNga) soNga++;
            }
            Ghi(string.Format("C2. {0} thien thach THAT, trung {1}, danh nga {2} ({3:P0}); thoi gian nga {4:F2} s", soQua, soTrung, soNga, soTrung > 0 ? (float)soNga / soTrung : 0f, giayNga));
            Kiem(soTrung >= soQua * 0.8f, "thien thach Quy du khong trung nguoi choi - phep do vo nghia");
            Kiem(soNga >= 1 && soNga <= 14, "so lan nga ngoai khoang ngau nhien cua 15%");
            Kiem(soNga == 0 || Mathf.Abs(giayNga - 1f) < 0.01f, "nga cua Quy du khong phai 1 giay");
            DonDepHieuUng(pc.gameObject);
            Object.Destroy(qd.gameObject);
            mauPc.health = mauPc.maxHealth;
        }
        yield return null;

        // ================= D. BO XUONG DO DON =================
        {
            DatCho(pc, p + Vector3.up * 0.05f);
            var bx = Sinh(MonsterType.Skeleton, p + new Vector3(0f, 0f, 5f), pc.transform, "TAM_BoXuongDo");
            var d = bx.health;
            var mask = LayerMask.GetMask("Enemy");
            Vector3 tam = bx.transform.position + Vector3.up * 1f;
            int soDon = 400, soDo = 0, doMaChay = 0, khongDoMaKhongChay = 0;
            int hien0 = ChongDo.SoLanHien;
            bool daChup = false;
            for (int i = 0; i < soDon; i++)
            {
                DonDepHieuUng(bx.gameObject);
                d.health = d.maxHealth;
                CombatUtil.AreaDamage(tam, 1.2f, 20f, mask, DamageType.Fire, 3f, mauPc);
                bool biDo = d.health >= d.maxHealth - 1e-3f;
                bool coChay = bx.GetComponent<BurningEffect>() != null;
                if (biDo) { soDo++; if (coChay) doMaChay++; }
                else if (!coChay) khongDoMaKhongChay++;
                if (biDo && !daChup)
                {
                    daChup = true;
                    cam.transform.position = bx.transform.position + new Vector3(0f, 2.4f, -3.6f);
                    cam.transform.LookAt(bx.transform.position + Vector3.up * 1.0f);
                    float tw = Time.time; while (Time.time - tw < 0.08f) yield return null;
                    yield return new WaitForEndOfFrame();
                    Chup(cam, "PlayTestShots/dac_tinh_quai_do_don.png");
                }
                yield return null;
            }
            int hien = ChongDo.SoLanHien - hien0;
            Ghi(string.Format("D1. {0} don ky nang (moi don mot khung): do {1} = {2:P1} (mong 25%, sai so ~2,2 diem); do ma van chay {3}; khong do ma khong chay {4}; hinh do don hien {5} lan",
                soDon, soDo, (float)soDo / soDon, doMaChay, khongDoMaKhongChay, hien));
            Kiem((float)soDo / soDon > 0.18f && (float)soDo / soDon < 0.32f, "ti le do don khong phai ~25%");
            Kiem(doMaChay == 0, "do duoc don ma van dinh chay");
            Kiem(khongDoMaKhongChay == 0, "khong do ma khong dinh chay - phep do hieu ung vo nghia");
            Kiem(hien >= 1 && hien <= soDo, "hinh do don khong hien / hien nhieu hon so lan do");
            DonDepHieuUng(bx.gameObject);

            // D2. Sat thuong RI (nhip chay, loc cuon...): khong bao gio do
            int doRi = 0;
            for (int i = 0; i < 80; i++)
            {
                d.health = d.maxHealth;
                d.GhiKeDanh(mauPc);
                Damageable.LaSatThuongRi = true;
                try { d.TakeDamage(10f, DamageType.Fire, tam); } finally { Damageable.LaSatThuongRi = false; }
                if (d.health >= d.maxHealth - 1e-3f) doRi++;
                yield return null;
            }
            // D3. Don cua QUAI (ke gay khong phai nguoi choi): khong do
            var quaiKhac = Sinh(MonsterType.Witch, p + new Vector3(4f, 0f, 5f), pc.transform, "TAM_PhuThuyD").health;
            int doQuai = 0;
            for (int i = 0; i < 80; i++)
            {
                d.health = d.maxHealth;
                d.GhiKeDanh(quaiKhac);
                d.TakeDamage(10f, DamageType.Physical, tam);
                if (d.health >= d.maxHealth - 1e-3f) doQuai++;
                yield return null;
            }
            // D4. Phu thuy (khong co do don) an don ky nang
            int doPhu = 0;
            for (int i = 0; i < 80; i++)
            {
                quaiKhac.health = quaiKhac.maxHealth;
                quaiKhac.GhiKeDanh(mauPc);
                quaiKhac.TakeDamage(10f, DamageType.Fire, quaiKhac.transform.position + Vector3.up);
                if (quaiKhac.health >= quaiKhac.maxHealth - 1e-3f) doPhu++;
                yield return null;
            }
            // D5. Hieu ung KHONG kem don (hat tung): van bi do ~25%
            int hatDuoc = 0;
            for (int i = 0; i < 200; i++)
            {
                DonDepHieuUng(bx.gameObject);
                if (BiHatTung.Apply(d, 0.5f) != null) hatDuoc++;
                yield return null;
            }
            DonDepHieuUng(bx.gameObject);
            Ghi(string.Format("D2. DOI CHUNG sat thuong ri: 80 nhip -> do {0} | D3. don cua QUAI: 80 -> do {1} | D4. Phu thuy an don ky nang: 80 -> do {2} | D5. hat tung khong kem don: 200 lan -> hat duoc {3} ({4:P0}, mong ~75%)",
                doRi, doQuai, doPhu, hatDuoc, hatDuoc / 200f));
            Kiem(doRi == 0, "sat thuong ri bi do");
            Kiem(doQuai == 0, "don cua quai bi Bo xuong do");
            Kiem(doPhu == 0, "Phu thuy lai do don");
            Kiem(hatDuoc > 120 && hatDuoc < 180, "hieu ung hat tung khong bi do ~25%");
            Object.Destroy(quaiKhac.gameObject);
            Object.Destroy(bx.gameObject);
        }

        // ================= E. GOI TIN: BIT "VUA DO DON" =================
        {
            var ds = new GoiTin.MotQuai[3];
            ds[0] = new GoiTin.MotQuai { id = 7, loai = (byte)MonsterType.Skeleton, viTri = p, mau01 = 0.5f, coHieuUng = 0x3F, doDon = true };
            ds[1] = new GoiTin.MotQuai { id = 8, loai = (byte)MonsterType.Skeleton, viTri = p, mau01 = 1f, coHieuUng = 0x3F, doDon = false };
            ds[2] = new GoiTin.MotQuai { id = 9, loai = (byte)MonsterType.Skeleton, viTri = p, mau01 = 1f, coHieuUng = 0, doDon = true, daChet = true };
            var b = GoiTin.VietQuai(123, ds, 0, 3);
            var ra = new GoiTin.MotQuai[16];
            int moc;
            int n = GoiTin.DocQuai(b, ra, out moc);
            bool dung = n == 3 && ra[0].doDon && ra[0].coHieuUng == 0x3F && !ra[1].doDon && ra[1].coHieuUng == 0x3F
                        && ra[2].doDon && ra[2].daChet && ra[2].coHieuUng == 0;
            Ghi(string.Format("E. goi quai qua VietQuai/DocQuai: {0} con; doDon {1}/{2}/{3}, co hieu ung 0x{4:X2}/0x{5:X2}/0x{6:X2}, chet {7}",
                n, ra[0].doDon, ra[1].doDon, ra[2].doDon, ra[0].coHieuUng, ra[1].coHieuUng, ra[2].coHieuUng, ra[2].daChet));
            Kiem(dung, "bit vua do don mat / de len hieu ung qua goi tin");
        }

        float tk = Time.time; while (Time.time - tk < 0.5f) yield return null;
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Chup(Camera cam, string duong)
    {
        var rt = RenderTexture.GetTemporary(960, 540, 24, RenderTextureFormat.ARGB32);
        var cuRt = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = cuRt;
        var cu = RenderTexture.active;
        RenderTexture.active = rt;
        var tx = new Texture2D(960, 540, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
        tx.Apply();
        RenderTexture.active = cu;
        RenderTexture.ReleaseTemporary(rt);
        File.WriteAllBytes(duong, tx.EncodeToPNG());
        Object.DestroyImmediate(tx);
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/dac_tinh_quai.txt", bao.ToString());
        foreach (var go in Object.FindObjectsByType<Transform>())
            if (go != null && go.parent == null && go.name.StartsWith("TAM_")) Object.Destroy(go.gameObject);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
        if (!string.IsNullOrEmpty(canhCu)) EditorApplication.update += TraLaiCanh;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }
}
