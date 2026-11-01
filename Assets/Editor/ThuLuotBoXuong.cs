using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 158 - CU LUOT BO XUONG (nguoi dung 01/11/2026: luot rat nhanh nhu Zealot, hoi 1,5 s, 1 nhat chem, 55% xuyen vo khien + danh vao nguoi; 02/11/2026 tam 8 m, 15% nga 1 s).
/// Quai THAT (EnemyFactory, AI bat), do vi tri tung khung - khong doc bien cua cu luot:
///   A. 4,5 m khong khien: toc do lon nhat >= 15 m/s (di bo 4,92), toi noi, mat dung attackDamage luc trung, hoi chieu 1,5 s do giua hai lan luot.
///   B. Tu 11 m di bo vao: chi luot khi da trong tam 8 m; luot het 8 m van toi noi.
///   J. 15% danh nga 1 s: ep nga -> nguoi / khien vua xuyen deu nga; khien con nguyen mien dich; 60 lan gieo that ~15%; may khach kieu 5.
///   C. DOI CHUNG: dung sat (trong tam chem) khong khien -> 0 luot; hop chan giua duong -> 0 luot.
///   D. Khien: ep xuyen -> vo ngay, mat attackDamage vao NGUOI, toi sat; ep khong xuyen -> khien mat dung attackDamage, nguoi 0, dung ngoai vom; 40 lan gieo that ~55%.
///   E. Muc tieu BOT (nhan vat khac trong GameDirector) cung bi luot.
///   F. Dinh choang giua cu luot: dung ngay, khong trung.
///   G. May khach (DienLaiDon 4-7) + goi tin giu kieu.
///   H. Hinh: bong mo sinh ra + tan het, tran 4 cu luot co bong, anh may quay game (PlayTestShots/luot_bo_xuong/).
/// </summary>
public static class ThuLuotBoXuong
{
    const string Thu = "PlayTestShots/luot_bo_xuong/";
    static readonly StringBuilder nk = new StringBuilder();
    static int loi;
    static bool daBatDau, truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/158 Cu luot Bo xuong (8 m, hoi 1,5 s, 55% xuyen khien, 15% nga)", false, 232)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[ThuLuotBoXuong] dang Play hoac canh chua luu"); return; }
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        Directory.CreateDirectory(Thu);
        nk.Length = 0; loi = 0; daBatDau = false;
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
        var go = new GameObject("TAM_ThuLuotBoXuong");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Kiem(bool dat, string mo)
    {
        if (!dat) loi++;
        nk.AppendLine((dat ? "  OK   " : "  LOI  ") + mo);
    }

    static float Ngang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    static Vector3 TrenDat(Vector3 p) { p.y = VfxFactory.GroundY(p); return p; }

    static void DatCho(Component c, Vector3 p)
    {
        var cc = c.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        c.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    /// <summary>Tim cho thoang: 12 m ngang tu goc khong vuong gi, duoi chan khong nuoc.</summary>
    static Vector3 HuongTrong(Vector3 goc)
    {
        int mat = LayerMask.GetMask("Default", "Ground");
        for (int g = 0; g < 24; g++)
        {
            Vector3 h = Quaternion.Euler(0f, g * 15f, 0f) * Vector3.forward;
            bool sach = !Physics.SphereCast(goc + Vector3.up * 1.2f, 0.6f, h, out _, 12f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore)
                        && !Physics.Linecast(goc + Vector3.up, goc + h * 12f + Vector3.up, mat, QueryTriggerInteraction.Ignore);
            if (sach && Mathf.Abs(VfxFactory.GroundY(goc + h * 10f) - goc.y) < 1f) return h;
        }
        return Vector3.forward;
    }

    static EnemyAI Sinh(Vector3 p, Transform muc, Vector3 nhinVe)
    {
        var go = EnemyFactory.Spawn(MonsterType.Skeleton, TrenDat(p), null, muc);
        go.name = "TAM_BoXuong";
        var ai = go.GetComponent<EnemyAI>();
        ai.target = muc;
        var d = go.GetComponent<Damageable>();
        d.maxHealth = 100000f; d.health = 100000f;
        go.transform.rotation = Quaternion.LookRotation(nhinVe);
        return ai;
    }

    static PlayerController toi;

    static IEnumerator KichBan()
    {
        while (GameDirector.Instance == null) yield return null;
        yield return new WaitForSeconds(1.5f);
        GameDirector.Instance.enabled = false;
        foreach (var e in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude)) Object.Destroy(e.gameObject);
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude)) if (pc.tuDocInput) toi = pc;
        var mau = toi.health;
        mau.maxHealth = 1000000f; mau.health = 1000000f;
        Vector3 goc = toi.transform.position;
        Vector3 h = HuongTrong(goc);
        yield return null;

        // ================= A =================
        nk.AppendLine("== A 4,5 m, khong khien");
        {
            var ai = Sinh(goc + h * 4.5f, toi.transform, -h);
            float dmg = ai.attackDamage, mau0 = mau.health, tocMax = 0f, kcTruoc = Ngang(ai.transform.position, goc), luc0 = -1f, lucTrung = -1f, matLucTrung = 0f;
            int l0 = EnemyAI.SoLanLuot, t0 = EnemyAI.SoLanLuotTrung;
            Vector3 viTruoc = ai.transform.position;
            for (float han = Time.time + 1.2f; Time.time < han && lucTrung < 0f; )
            {
                float truocFrame = mau.health;
                yield return null;
                if (Time.deltaTime > 0f) tocMax = Mathf.Max(tocMax, Ngang(viTruoc, ai.transform.position) / Time.deltaTime);
                viTruoc = ai.transform.position;
                if (luc0 < 0f && EnemyAI.SoLanLuot > l0) luc0 = Time.time;
                if (EnemyAI.SoLanLuotTrung > t0) { lucTrung = Time.time; matLucTrung = mau0 - mau.health; }
            }
            Kiem(EnemyAI.SoLanLuot == l0 + 1 && lucTrung > 0f, "luot " + (EnemyAI.SoLanLuot - l0) + " lan, trung " + (EnemyAI.SoLanLuotTrung - t0));
            Kiem(tocMax >= 15f, "toc do lon nhat do tu vi tri " + tocMax.ToString("F1") + " m/s (di bo " + ai.moveSpeed.ToString("F2") + ")");
            Kiem(lucTrung - luc0 < 0.4f, "4,5 m mat " + (lucTrung - luc0).ToString("F3") + " s");
            Kiem(Mathf.Abs(matLucTrung - dmg) < 0.5f, "mat luc trung " + matLucTrung.ToString("F2") + " = 1 nhat chem " + dmg.ToString("F2"));
            float kc = Ngang(ai.transform.position, goc);
            Kiem(kc <= ai.attackRange + 0.2f && kc > 0.5f, "dung cach " + kc.ToString("F2") + " m");

            // Hoi chieu: keo ra lai 4,5 m ngay sau khi trung, do moc luot ke tiep
            yield return new WaitForSeconds(0.2f);
            DatCho(ai, TrenDat(goc + h * 4.5f));
            ai.transform.rotation = Quaternion.LookRotation(-h);
            float cuoi = -1f; int l1 = EnemyAI.SoLanLuot;
            for (float han = Time.time + 3f; Time.time < han && cuoi < 0f; )
            {
                // giu con quai o 4,5 m (khong cho no di bo toi noi) cho toi khi hoi chieu xong
                if (!ai.DangLuot && ai.HoiLuotConLai > 0.02f) { DatCho(ai, TrenDat(goc + h * 4.5f)); ai.transform.rotation = Quaternion.LookRotation(-h); }
                yield return null;
                if (EnemyAI.SoLanLuot > l1) cuoi = Time.time;
            }
            Kiem(cuoi > 0f && Mathf.Abs(cuoi - luc0 - EnemyAI.GiayHoiLuot) < 0.1f, "giua hai lan luot " + (cuoi - luc0).ToString("F3") + " s (phai ~1,5)");
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= B =================
        nk.AppendLine("== B Tu 11 m di bo vao");
        {
            var ai = Sinh(goc + h * 11f, toi.transform, -h);
            int l0 = EnemyAI.SoLanLuot, tb0 = EnemyAI.SoLanLuotTrung; float kcLuc = -1f, kcTruoc = 11f;
            for (float han = Time.time + 3f; Time.time < han && kcLuc < 0f; )
            {
                kcTruoc = Ngang(ai.transform.position, goc);
                yield return null;
                if (EnemyAI.SoLanLuot > l0) kcLuc = kcTruoc;
            }
            Kiem(kcLuc > EnemyAI.TamLuot - 0.7f && kcLuc <= EnemyAI.TamLuot + 0.05f, "bat dau luot khi cach " + kcLuc.ToString("F2") + " m (tam " + EnemyAI.TamLuot + ")");
            for (float han = Time.time + 0.8f; Time.time < han && EnemyAI.SoLanLuotTrung == tb0; ) yield return null;
            Kiem(EnemyAI.SoLanLuotTrung == tb0 + 1, "luot het tam van toi noi");
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= C =================
        nk.AppendLine("== C Doi chung: sat nguoi / co vat chan");
        {
            var ai = Sinh(goc + h * 1.4f, toi.transform, -h);
            int l0 = EnemyAI.SoLanLuot;
            yield return new WaitForSeconds(2f);
            Kiem(EnemyAI.SoLanLuot == l0, "dung sat 1,4 m khong khien: " + (EnemyAI.SoLanLuot - l0) + " luot");
            Object.Destroy(ai.gameObject);

            var hop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hop.name = "TAM_HopChan";
            hop.transform.position = TrenDat(goc + h * 2.6f) + Vector3.up;
            hop.transform.localScale = new Vector3(5f, 3f, 0.6f);
            hop.transform.rotation = Quaternion.LookRotation(h);
            Physics.SyncTransforms();
            ai = Sinh(goc + h * 4.5f, toi.transform, -h);
            l0 = EnemyAI.SoLanLuot;
            yield return new WaitForSeconds(1.2f);
            Kiem(EnemyAI.SoLanLuot == l0, "hop chan giua duong: " + (EnemyAI.SoLanLuot - l0) + " luot");
            Object.Destroy(ai.gameObject);
            Object.Destroy(hop);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= D =================
        nk.AppendLine("== D Khien");
        {
            var k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); mau.khieng = k;
            EnemyAI.EpXuyenChoPhepThu = 1f;
            var ai = Sinh(goc + h * 4.8f, toi.transform, -h);
            ai.attackCooldown = 99f;        // tat nhat chem thuong: chi do cu luot
            float dmg = ai.attackDamage, mauN = mau.health;
            int x0 = EnemyAI.SoLanLuotXuyen, t0 = EnemyAI.SoLanLuotTrung;
            for (float han = Time.time + 1.2f; Time.time < han && EnemyAI.SoLanLuotTrung == t0; ) yield return null;
            Kiem(EnemyAI.SoLanLuotXuyen == x0 + 1 && !k.DangBat, "xuyen: khien vo " + !k.DangBat + " (con " + k.mau + ")");
            Kiem(Mathf.Abs(mauN - mau.health - dmg) < 0.5f, "xuyen: nguoi mat " + (mauN - mau.health).ToString("F2") + " = " + dmg.ToString("F2"));
            float kcX = Ngang(ai.transform.position, goc);
            Kiem(kcX < toi.khiengBanKinh - 0.5f, "xuyen: luot qua vom, cach tam " + kcX.ToString("F2") + " m (vom " + toi.khiengBanKinh + ")");
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(0.3f);

            k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); mau.khieng = k;
            EnemyAI.EpXuyenChoPhepThu = 0f;
            ai = Sinh(goc + h * 4.8f, toi.transform, -h);
            ai.attackCooldown = 99f;
            dmg = ai.attackDamage; mauN = mau.health;
            int kx0 = EnemyAI.SoLanLuotKhongXuyen;
            for (float han = Time.time + 1.2f; Time.time < han && EnemyAI.SoLanLuotKhongXuyen == kx0; ) yield return null;
            yield return null;
            Kiem(k.DangBat && Mathf.Abs(225f - k.mau - dmg) < 0.5f, "khong xuyen: khien mat " + (225f - k.mau).ToString("F2") + " = " + dmg.ToString("F2"));
            Kiem(Mathf.Abs(mauN - mau.health) < 0.01f, "khong xuyen: nguoi mat " + (mauN - mau.health).ToString("F2"));
            float kcK = Ngang(ai.transform.position, goc);
            Kiem(kcK >= toi.khiengBanKinh, "khong xuyen: dung ngoai vom, cach tam " + kcK.ToString("F2") + " m");
            Object.Destroy(ai.gameObject);
            k.Tat();
            yield return new WaitForSeconds(0.2f);

            // 40 lan gieo that
            EnemyAI.EpXuyenChoPhepThu = -1f;
            int x1 = EnemyAI.SoLanLuotXuyen, n1 = EnemyAI.SoLanLuotKhongXuyen;
            ai = Sinh(goc + h * 4.8f, toi.transform, -h);
            ai.attackCooldown = 99f;
            yield return null;
            for (int lan = 0; lan < 40; lan++)
            {
                k = Khieng.Bat(toi.gameObject, 100000f, toi.khiengBanKinh); mau.khieng = k;
                DatCho(ai, TrenDat(goc + h * 4.2f));
                ai.transform.rotation = Quaternion.LookRotation(-h);
                ai.target = toi.transform;
                int tong = EnemyAI.SoLanLuotXuyen + EnemyAI.SoLanLuotKhongXuyen;
                ai.LuotNgayChoPhepThu();
                for (float han = Time.time + 0.6f; Time.time < han && EnemyAI.SoLanLuotXuyen + EnemyAI.SoLanLuotKhongXuyen == tong; ) yield return null;
                ai.NgatLuot();
                k.Tat();
                yield return null;
            }
            int soX = EnemyAI.SoLanLuotXuyen - x1, soK = EnemyAI.SoLanLuotKhongXuyen - n1;
            float tiLe = soX / (float)Mathf.Max(1, soX + soK);
            Kiem(soX + soK == 40 && tiLe >= 0.3f && tiLe <= 0.8f, "40 lan gieo that: xuyen " + soX + ", khong " + soK + " (" + (tiLe * 100f).ToString("F0") + "%, mong 55%)");
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= E =================
        nk.AppendLine("== E Muc tieu la BOT (nhan vat khac trong danh sach GameDirector)");
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Sorceress.prefab");
            var bot = Object.Instantiate(pf, TrenDat(goc + h * 10f), Quaternion.identity);
            bot.name = "TAM_Bot";
            var pcBot = bot.GetComponent<PlayerController>();
            pcBot.tuDocInput = false;
            var mauBot = bot.GetComponent<Damageable>();
            mauBot.maxHealth = 100000f; mauBot.health = 100000f;
            GameDirector.Instance.ThemNguoiChoi(bot.transform);
            yield return null;
            var ai = Sinh(goc + h * 14.5f, bot.transform, -h);
            yield return null;
            ai.target = bot.transform;
            float mauB0 = mauBot.health, dmg = ai.attackDamage;
            int t0 = EnemyAI.SoLanLuotTrung;
            for (float han = Time.time + 1.5f; Time.time < han && EnemyAI.SoLanLuotTrung == t0; ) yield return null;
            Kiem(EnemyAI.SoLanLuotTrung == t0 + 1 && mauB0 - mauBot.health >= dmg - 0.5f, "BOT trung luot, mat " + (mauB0 - mauBot.health).ToString("F2") + " (nhat chem " + dmg.ToString("F2") + ")");
            Object.Destroy(ai.gameObject);
            Object.Destroy(bot);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= F =================
        nk.AppendLine("== F Dinh choang giua cu luot");
        {
            var ai = Sinh(goc + h * 4.8f, toi.transform, -h);
            ai.GetComponent<Damageable>().tiLeDoDon = 0f;      // Bo xuong 45% do don chan ca choang (ChongDo) -> lan chay thu hai bao loi gia
            int l0 = EnemyAI.SoLanLuot, t0 = EnemyAI.SoLanLuotTrung;
            for (float han = Time.time + 1f; Time.time < han && EnemyAI.SoLanLuot == l0; ) yield return null;
            StunnedEffect.Apply(ai.GetComponent<Damageable>(), 1f);
            yield return null; yield return null;
            Kiem(!ai.DangLuot, "choang: dung luot ngay");
            Kiem(EnemyAI.SoLanLuotTrung == t0, "choang: khong trung");
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(0.3f);
        }

        // ================= G =================
        nk.AppendLine("== G May khach + goi tin");
        {
            var go = GoiTin.VietDonQuai(new GoiTin.MotDonQuai { idQuai = 7, kieuDon = (byte)EnemyAI.KieuLuotXuyenKhien, chiSoNanNhan = 1, soThuTu = 3, diemNgam = goc, satThuong = 35f });
            GoiTin.MotDonQuai doc;
            Kiem(GoiTin.DocDonQuai(go, out doc) && doc.kieuDon == EnemyAI.KieuLuotXuyenKhien && Mathf.Abs(doc.satThuong - 35f) < 0.1f, "goi tin giu kieu " + doc.kieuDon + ", sat thuong " + doc.satThuong);

            var ai = Sinh(goc + h * 3f, toi.transform, -h);
            ai.enabled = false;
            yield return null;
            ai.DienLaiDon(EnemyAI.KieuLuotBatDau, goc, null);
            yield return null;
            Kiem(GameObject.Find("LuotBoXuong") != null, "kieu 4: co hinh luot");
            float mauN = mau.health, dmg = ai.attackDamage;
            ai.DienLaiDon(EnemyAI.KieuLuotTrung, goc, null);
            Kiem(Mathf.Abs(mauN - mau.health) < 0.01f, "kieu 5 nham nguoi khac: 0 mau");
            ai.DienLaiDon(EnemyAI.KieuLuotTrung, goc, mau);
            Kiem(Mathf.Abs(mauN - mau.health - dmg) < 0.5f, "kieu 5 nham minh: mat " + (mauN - mau.health).ToString("F2"));
            var k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); mau.khieng = k;
            mauN = mau.health;
            ai.DienLaiDon(EnemyAI.KieuLuotTrungKhien, goc, mau);
            Kiem(k.DangBat && Mathf.Abs(225f - k.mau - dmg) < 0.5f && Mathf.Abs(mauN - mau.health) < 0.01f, "kieu 6: khien mat " + (225f - k.mau).ToString("F2") + ", nguoi 0");
            ai.DienLaiDon(EnemyAI.KieuLuotXuyenKhien, goc, mau);
            Kiem(!k.DangBat, "kieu 7: khien vo");
            HieuUngQuaMang.ApKhieng(toi, 0.5f);
            Kiem(!k.DangBat, "goi cu 'con khien' den ngay sau khi vo: khong dung lai");
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(1f);
        }

        // ================= H =================
        nk.AppendLine("== H Hinh");
        {
            var cam = Camera.main;
            var rig = cam.GetComponent<CameraRig>();
            if (rig != null) rig.SetView((int)ViewMode.Isometric25D);
            TamNhin.BatTrongPhepThu = true;
            yield return new WaitForSeconds(0.8f);
            if (rig != null) rig.enabled = false;
            var ai = Sinh(goc + h * 4.8f, toi.transform, -h);
            ai.attackCooldown = 99f;
            int b0 = HinhLuotBoXuong.SoBongDaSinh, l0 = EnemyAI.SoLanLuot;
            for (float han = Time.time + 1f; Time.time < han && EnemyAI.SoLanLuot == l0; ) yield return null;
            Time.captureDeltaTime = 1f / 60f;
            for (int k = 1; k <= 22; k++)
            {
                yield return new WaitForEndOfFrame();
                if (k == 6 || k == 10 || k == 14 || k == 22)
                {
                    var hinh = GameObject.Find("LuotBoXuong");
                    var vet = hinh != null ? hinh.GetComponent<TrailRenderer>() : null;
                    if (vet != null)
                    {
                        nk.AppendLine("  khung " + k + ": vet " + vet.positionCount + " diem, an " + vet.forceRenderingOff + ", bat " + vet.enabled
                            + ", mau " + vet.bounds.size.ToString("F2") + ", anh " + (vet.sharedMaterial.mainTexture != null ? vet.sharedMaterial.mainTexture.name : "null"));
                        if (k == 6) Kiem(vet.positionCount >= 2 && vet.sharedMaterial.mainTexture != null && vet.sharedMaterial.mainTexture.name == "VetHon", "vet hon dung anh VetHon, co diem");
                    }
                    else nk.AppendLine("  khung " + k + ": da het vat LuotBoXuong");
                    var rt = RenderTexture.GetTemporary(960, 540, 24);
                    cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
                    RenderTexture.active = rt;
                    var tx = new Texture2D(960, 540, TextureFormat.RGB24, false);
                    tx.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); tx.Apply();
                    RenderTexture.active = null; RenderTexture.ReleaseTemporary(rt);
                    File.WriteAllBytes(Thu + "luot_" + k + ".png", tx.EncodeToPNG());
                    Object.Destroy(tx);
                }
            }
            Time.captureDeltaTime = 0f;
            Kiem(HinhLuotBoXuong.SoBongDaSinh > b0, "bong mo sinh " + (HinhLuotBoXuong.SoBongDaSinh - b0));
            Object.Destroy(ai.gameObject);
            yield return new WaitForSeconds(1.5f);
            int con = 0;
            foreach (var g in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
                if (g.name == "BongLuot" || g.name == "LuotBoXuong") con++;
            Kiem(con == 0, "sau 1,5 s con " + con + " vat hinh luot");
            Kiem(HinhLuotBoXuong.SoCuLuotDangCoBong == 0, "dem cu luot co bong ve " + HinhLuotBoXuong.SoCuLuotDangCoBong);

            // Tran: 6 cu luot cung luc
            for (int i = 0; i < 6; i++) VfxFactory.LuotBoXuong(toi.transform);
            Kiem(HinhLuotBoXuong.SoCuLuotDangCoBong == HinhLuotBoXuong.TranCoBong, "6 cu luot cung luc: " + HinhLuotBoXuong.SoCuLuotDangCoBong + " co bong (tran " + HinhLuotBoXuong.TranCoBong + ")");
            yield return new WaitForSeconds(1.5f);
            if (rig != null) rig.enabled = true;
            TamNhin.BatTrongPhepThu = false;
        }

        // ================= J =================
        nk.AppendLine("== J 15% danh nga 1 s");
        {
            var ai = Sinh(goc + h * 6f, toi.transform, -h);
            ai.attackCooldown = 99f;
            yield return null;
            EnemyAI.EpNgaChoPhepThu = 1f;
            int t0 = EnemyAI.SoLanLuotTrung;
            for (float han = Time.time + 1f; Time.time < han && EnemyAI.SoLanLuotTrung == t0; ) yield return null;
            var ng = toi.GetComponent<BiDanhNga>();
            Kiem(ng != null && ng.DangNga, "ep nga: nguoi bi luot trung nga");
            float lucNga = Time.time;
            while (toi.GetComponent<BiDanhNga>() != null && toi.GetComponent<BiDanhNga>().DangNga && Time.time < lucNga + 3f) yield return null;
            float giayNga = Time.time - lucNga;
            Kiem(Mathf.Abs(giayNga - EnemyAI.GiayNgaLuot) < 0.15f, "nga " + giayNga.ToString("F2") + " s");
            yield return new WaitForSeconds(0.3f);

            var k = Khieng.Bat(toi.gameObject, 100000f, toi.khiengBanKinh); mau.khieng = k;
            EnemyAI.EpXuyenChoPhepThu = 0f;
            DatCho(ai, TrenDat(goc + h * 5f)); ai.transform.rotation = Quaternion.LookRotation(-h); ai.target = toi.transform;
            int kx0 = EnemyAI.SoLanLuotKhongXuyen;
            ai.LuotNgayChoPhepThu();
            for (float han = Time.time + 1f; Time.time < han && EnemyAI.SoLanLuotKhongXuyen == kx0; ) yield return null;
            ng = toi.GetComponent<BiDanhNga>();
            Kiem(EnemyAI.SoLanLuotKhongXuyen == kx0 + 1 && (ng == null || !ng.DangNga), "khien con nguyen: khong nga");
            yield return null;

            EnemyAI.EpXuyenChoPhepThu = 1f;
            k = Khieng.Bat(toi.gameObject, 225f, toi.khiengBanKinh); mau.khieng = k;
            DatCho(ai, TrenDat(goc + h * 5f)); ai.transform.rotation = Quaternion.LookRotation(-h); ai.target = toi.transform;
            t0 = EnemyAI.SoLanLuotTrung;
            ai.LuotNgayChoPhepThu();
            for (float han = Time.time + 1f; Time.time < han && EnemyAI.SoLanLuotTrung == t0; ) yield return null;
            ng = toi.GetComponent<BiDanhNga>();
            Kiem(!k.DangBat && ng != null && ng.DangNga, "khien vua bi xuyen: nga");
            EnemyAI.EpXuyenChoPhepThu = -1f;
            yield return new WaitForSeconds(1.3f);

            // Gieo that
            EnemyAI.EpNgaChoPhepThu = -1f;
            int soNga = 0, soLan = 0;
            for (int lan = 0; lan < 60; lan++)
            {
                foreach (var c in toi.GetComponents<BiDanhNga>()) Object.DestroyImmediate(c);
                DatCho(ai, TrenDat(goc + h * 4f)); ai.transform.rotation = Quaternion.LookRotation(-h); ai.target = toi.transform;
                t0 = EnemyAI.SoLanLuotTrung;
                ai.LuotNgayChoPhepThu();
                for (float han = Time.time + 0.5f; Time.time < han && EnemyAI.SoLanLuotTrung == t0; ) yield return null;
                ai.NgatLuot();
                if (EnemyAI.SoLanLuotTrung > t0) soLan++;
                if (toi.GetComponent<BiDanhNga>() != null) soNga++;
                yield return null;
            }
            foreach (var c in toi.GetComponents<BiDanhNga>()) Object.DestroyImmediate(c);
            float tl = soNga / (float)Mathf.Max(1, soLan);
            Kiem(soLan == 60 && tl > 0.03f && tl < 0.32f, "60 lan gieo that: nga " + soNga + "/" + soLan + " (" + (tl * 100f).ToString("F0") + "%, mong 15%)");

            // May khach
            ai.enabled = false;
            EnemyAI.EpNgaChoPhepThu = 1f;
            ai.DienLaiDon(EnemyAI.KieuLuotTrung, goc, mau);
            ng = toi.GetComponent<BiDanhNga>();
            Kiem(ng != null && ng.DangNga, "may khach kieu 5 nham minh: gieo nga");
            foreach (var c in toi.GetComponents<BiDanhNga>()) Object.DestroyImmediate(c);
            ai.DienLaiDon(EnemyAI.KieuLuotTrung, goc, null);
            Kiem(toi.GetComponent<BiDanhNga>() == null, "may khach kieu 5 nham nguoi khac: khong nga");
            EnemyAI.EpNgaChoPhepThu = -1f;
            Object.Destroy(ai.gameObject);
            var kk = toi.GetComponent<Khieng>(); if (kk != null) kk.Tat();
            yield return new WaitForSeconds(0.3f);
        }

        // ================= I =================
        nk.AppendLine("== I Tieng");
        {
            NhacNen.BatTrongPhepThu = true; TamNhin.BatTrongPhepThu = true;     // khong co thi co ChayThuMang la tieng IM, loa do ra 0
            bool tatGoc = EditorUtility.audioMasterMute;
            EditorUtility.audioMasterMute = false;      // Editor tat tieng tong thi loa do ra 0
            var nn = NhacNen.Ban;
            if (nn != null) { nn.sanh.nguon.mute = true; nn.tran.nguon.mute = true; }    // chi nghe tieng cu luot, khong lan nhac
            yield return new WaitForSeconds(0.2f);
            for (int i = 1; i <= 3; i++)
                Kiem(Resources.Load<AudioClip>("AmThanh/KyNang/BoXuongLao" + i) != null && Resources.Load<AudioClip>("AmThanh/KyNang/BoXuongToi" + i) != null, "clip bien the " + i);
            var cl = Resources.Load<AudioClip>("AmThanh/KyNang/BoXuongLao1");
            Kiem(cl != null && cl.channels == 2, "clip stereo: " + (cl != null ? cl.channels : 0) + " kenh");
            int sl0 = TiengViTri.SoLan[(int)TiengViTri.Loai.BoXuongLao], st0 = TiengViTri.SoLan[(int)TiengViTri.Loai.BoXuongToi];
            var ai = Sinh(goc + h * 4.8f, toi.transform, -h);
            ai.attackCooldown = 99f;
            float rms = 0f, tuong = 0f; int dem = 0;
            var tr = new float[2048]; var ph = new float[2048];
            int l0 = EnemyAI.SoLanLuot;
            for (float han = Time.time + 1f; Time.time < han && EnemyAI.SoLanLuot == l0; ) yield return null;
            for (float han = Time.time + 0.5f; Time.time < han; )
            {
                yield return null;
                AudioListener.GetOutputData(tr, 0); AudioListener.GetOutputData(ph, 1);
                float ll = 0f, rr = 0f, lr = 0f;
                for (int i = 0; i < tr.Length; i++) { ll += tr[i] * tr[i]; rr += ph[i] * ph[i]; lr += tr[i] * ph[i]; }
                if (ll + rr > 1e-5f) { rms += Mathf.Sqrt((ll + rr) / (2f * tr.Length)); tuong += lr / Mathf.Sqrt(ll * rr + 1e-12f); dem++; }
            }
            Kiem(TiengViTri.SoLan[(int)TiengViTri.Loai.BoXuongLao] == sl0 + 1 && TiengViTri.SoLan[(int)TiengViTri.Loai.BoXuongToi] == st0 + 1,
                 "mot cu luot: tieng lao +" + (TiengViTri.SoLan[(int)TiengViTri.Loai.BoXuongLao] - sl0) + ", tieng toi +" + (TiengViTri.SoLan[(int)TiengViTri.Loai.BoXuongToi] - st0));
            Kiem(dem > 0 && rms / Mathf.Max(1, dem) > 1e-3f && tuong / Mathf.Max(1, dem) < 0.95f,
                 "loa: RMS TB " + (rms / Mathf.Max(1, dem)).ToString("F4") + ", tuong quan hai kenh " + (tuong / Mathf.Max(1, dem)).ToString("F2"));
            Object.Destroy(ai.gameObject);
            NhacNen.BatTrongPhepThu = false; TamNhin.BatTrongPhepThu = false;
            EditorUtility.audioMasterMute = tatGoc;
            yield return new WaitForSeconds(0.5f);
        }

        EnemyAI.EpXuyenChoPhepThu = -1f;
        EnemyAI.EpNgaChoPhepThu = -1f;
        nk.Insert(0, "MENU 158 - Cu luot Bo xuong: " + loi + " loi\n");
        File.WriteAllText(Thu + "nhatky.txt", nk.ToString());
        Debug.Log("[ThuLuotBoXuong] " + loi + " loi\n" + nk);
        var rac = GameObject.Find("TAM_ThuLuotBoXuong");
        if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
    }
}
