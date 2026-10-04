using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: LUA CHAY TOAN THAN (menu 90).
///
/// Lan 3 (28/09/2026, nguoi dung ve duong do om sat nhan vat): lua CHAY LAN KHAP NGUOI va NAM TRONG DUONG VIEN than, khong
/// phai mot cuc dung truoc nguoi. Do bang ANH THAT, doc lap voi cach ve lua - moi nhan vat chup 3 anh CUNG MOT KHUNG:
///   (1) co than, an lua  (2) rieng nhan vat ve trang dac tren nen den -> BONG THAN (noi rong 14 diem anh ~ net do nguoi dung ve)
///   (3) co than, co lua (den lua tat ca 3 anh) -> 3-1 la PHAN LUA THEM VAO
///   A. TRONG VIEN = phan anh sang lua nam trong bong than / tong; PHU THAN = ti le diem anh bong than co lua > 0,08.
///      DOI CHUNG: khoi lua billboard lan 2 (LuaToanThan.DoiChungKhoiLua) tren cung nhan vat.
///   B. DI CHUYEN 6 m/s: goc toa do lua (_Goc) cach Hips bao nhieu, va TRONG VIEN chup luc dang chay.
///      DOI CHUNG: khoi billboard + mo phong the gioi (ban lan 1-2).
///   C. TAT NGAY: het gio / bi go giua chung -> khung ke tiep khong con hinh lua, vat lieu phu da go khoi than.
///   E. Nhip chay khong phun tia trung don (doi chung 1 don lua thuong).
/// Anh: PlayTestShots/lua_chay_*.png. Ket qua: PlayTestShots/lua_chay.txt.
/// </summary>
public static class ThuLuaChay
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/90. Chay thu LUA CHAY TOAN THAN (nguoi choi + quai)", false, 179)]
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
        LuaToanThan.DoiChungTheGioi = false;
        LuaToanThan.DoiChungKhoiLua = false;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_LuaChay");
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[LuaChay] " + s); }
    static void Kiem(bool dung, string loiNeuSai) { if (!dung) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static float DatY(Vector3 p)
    {
        var t = Terrain.activeTerrain;
        return t != null ? t.SampleHeight(p) + t.transform.position.y : 0f;
    }

    static Transform Hong(Damageable d)
    {
        foreach (var smr in d.GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (var b in smr.bones) if (b != null && b.name == "Hips") return b;
        return d.transform;
    }

    static void DatCho(Component c, Vector3 p)
    {
        var cc = c.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        c.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    static IEnumerator KichBan()
    {
        PlayerController pc = null;
        float han = Time.time + 25f;
        while (pc == null && Time.time < han) { pc = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        var cam = Camera.main;
        if (pc == null || cam == null) { Ghi("[LOI] thieu nhan vat / camera"); loi++; Ket(); yield break; }
        var dir = Object.FindAnyObjectByType<GameDirector>();
        if (dir != null) dir.enabled = false;
        var rig = cam.GetComponent<CameraRig>();
        if (rig != null) rig.enabled = false;
        var mauPc = pc.GetComponent<Damageable>();

        Vector3 p = new Vector3(0f, 0f, -10.5f);
        p.y = DatY(p);
        DatCho(pc, p + Vector3.up * 0.05f);
        pc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        pc.enabled = false;

        var loai = new[] { MonsterType.Skeleton, MonsterType.QuyCay };
        var quai = new List<Damageable>();
        for (int i = 0; i < loai.Length; i++)
        {
            var q = p + new Vector3(-2.2f + i * 4.4f, 0f, -0.3f);
            q.y = DatY(q);
            var go = EnemyFactory.Spawn(loai[i], q, null, pc.transform);
            go.name = "TAM_Quai_" + loai[i];
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var ai = go.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
            var d = go.GetComponent<Damageable>();
            d.maxHealth = 100000f; d.health = 100000f;
            // Bo xuong 45% DO DON (28/09/2026) chan ca hieu ung chay -> lua luc co luc khong (A "khong co LuaToanThan", doi chung B -100%)
            d.tiLeDoDon = 0f;
            quai.Add(d);
        }
        yield return new WaitForSeconds(0.6f);

        // ================= A. LUA NAM TRONG VIEN THAN + PHU THAN =================
        var tatCa = new List<Damageable> { mauPc }; tatCa.AddRange(quai);
        foreach (var d in tatCa) BurningEffect.Apply(d, 0.01f, 3.5f, null);
        float giayChay = mauPc.GetComponent<BurningEffect>().remaining;   // AddComponent mang san 4 s (xem HUONG-DAN)
        float t0 = Time.time;
        while (Time.time - t0 < 1.2f) yield return null;
        cam.transform.position = p + new Vector3(0f, 4.2f, -6.2f);
        cam.transform.LookAt(p + new Vector3(0f, 1.0f, 0f));
        yield return new WaitForEndOfFrame();
        Chup(cam, "PlayTestShots/lua_chay_gan.png");
        cam.transform.position = p + new Vector3(0f, 13f, -11.5f);
        cam.transform.LookAt(p);
        yield return new WaitForEndOfFrame();
        Chup(cam, "PlayTestShots/lua_chay_goc_choi.png");
        foreach (var d in tatCa)
        {
            var l = d.GetComponentInChildren<LuaToanThan>();
            Kiem(l != null, d.name + ": khong co LuaToanThan");
            if (l == null) continue;
            var c = d.transform.position;
            cam.transform.position = c + new Vector3(0f, 2.6f, -3.6f);
            cam.transform.LookAt(c + Vector3.up * 0.9f);
            yield return new WaitForEndOfFrame();
            float trongVien, phu;
            DoAnh(cam, d, l, out trongVien, out phu, d == mauPc ? "PlayTestShots/lua_chay_vien.png" : null);
            float lo, hi; KhungDoc(d, out lo, out hi);
            Ghi(string.Format("A. {0}: phu lop lua len {1} renderer; than cao theo xuong {2:F2} m / hinh that {3:F2} m; vung co lua NAM TRONG VIEN than {4:P0} (theo nang luong {6:P0}); than duoc lua phu {5:P0}",
                d.name, l.SoRendererDaPhu, l.ChieuCaoThan, hi - lo, trongVien, phu, NangLuongTrong));
            Kiem(l.SoRendererDaPhu >= 1, d.name + ": khong phu duoc lop lua len renderer nao");
            Kiem(Mathf.Abs(l.ChieuCaoThan / (hi - lo) - 1f) < 0.15f, d.name + ": chieu cao theo xuong lech hinh that qua 15%");
            Kiem(trongVien >= 0.90f, d.name + ": lua tran ra ngoai vien than");
            Kiem(phu >= 0.80f, d.name + ": lua chua lan khap than");
        }

        // ================= F. BOC CHAY: LUOI LUA LIEM LEN + KHOI + LUA TREN THAN DONG =================
        {
            var l = mauPc.GetComponentInChildren<LuaToanThan>();
            if (l == null || l.LuoiLua == null || l.KhoiBoc == null || l.DinhDau == null) { Ghi("[LOI] F: thieu luoi lua / khoi / xuong dau"); loi++; }
            else
            {
                var hat = new ParticleSystem.Particle[l.LuoiLua.main.maxParticles];
                int k = 0;
                int trenDau = 0; float vuot = -9f;
                // 04/10/2026 (nguoi dung: lua phai BAO QUANH TOAN THAN, khong lo lung tren khong): goc moi luoi lua phai SAT XUONG (khoang
                // cach toi doan xuong gan nhat), dung yen, va co luoi lua ca o THAN DUOI (duoi xuong Hips) lan THAN TREN
                var smrF = mauPc.GetComponentInChildren<SkinnedMeshRenderer>();
                var doanXuong = new List<KeyValuePair<Vector3, Vector3>>();
                Transform hipsF = null;
                if (smrF != null)
                {
                    var tapXuong = new HashSet<Transform>(smrF.bones);
                    foreach (var b in smrF.bones)
                    {
                        if (b == null) continue;
                        if (b.name == "Hips") hipsF = b;
                        if (b.parent != null && tapXuong.Contains(b.parent)) doanXuong.Add(new KeyValuePair<Vector3, Vector3>(b.parent.position, b.position));
                    }
                }
                float xaXuongMax = 0f, tocMax = 0f; int thanDuoi = 0, thanTren = 0;
                // lay mau 12 khung (0,6 s): mot khung chi co ~15 luoi lua, co luc khong luoi nao o dinh dau
                float yDau = 0f;
                for (int khung = 0; khung < 12; khung++)
                {
                if (khung > 0) yield return new WaitForSeconds(0.05f);
                yDau = l.DinhDau.position.y;
                k = l.LuoiLua.GetParticles(hat);
                if (doanXuong.Count > 0 && smrF != null)
                {
                    doanXuong.Clear();
                    var tapXuong2 = new HashSet<Transform>(smrF.bones);
                    foreach (var b in smrF.bones) if (b != null && b.parent != null && tapXuong2.Contains(b.parent)) doanXuong.Add(new KeyValuePair<Vector3, Vector3>(b.parent.position, b.position));
                }
                for (int i = 0; i < k; i++)
                {
                    Vector3 w = l.LuoiLua.transform.TransformPoint(hat[i].position);
                    float dinhLua = w.y + hat[i].GetCurrentSize(l.LuoiLua) * 0.45f;   // dinh ngon lua (pivot 0,3 + 0,15 nua tren)
                    if (dinhLua > yDau) trenDau++;
                    vuot = Mathf.Max(vuot, dinhLua - yDau);
                    float gan = 9f;
                    foreach (var dx in doanXuong)
                    {
                        Vector3 ab = dx.Value - dx.Key; float t = Mathf.Clamp01(Vector3.Dot(w - dx.Key, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                        gan = Mathf.Min(gan, Vector3.Distance(w, dx.Key + ab * t));
                    }
                    xaXuongMax = Mathf.Max(xaXuongMax, gan);
                    tocMax = Mathf.Max(tocMax, hat[i].totalVelocity.magnitude);
                    if (hipsF != null && w.y < hipsF.position.y) thanDuoi++; else thanTren++;
                }
                }
                Ghi(string.Format("F. bam than (12 khung): goc luoi lua xa doan xuong gan nhat toi da {0:F2} m, toc do lon nhat {1:F2} m/s; luot luoi lua than duoi (duoi hong) {2}, than tren {3}",
                    xaXuongMax, tocMax, thanDuoi, thanTren));
                var hk = new ParticleSystem.Particle[l.KhoiBoc.main.maxParticles];
                int nk = l.KhoiBoc.GetParticles(hk);
                float yKhoi = 0f;
                for (int i = 0; i < nk; i++) yKhoi += l.KhoiBoc.transform.TransformPoint(hk[i].position).y - yDau;
                yKhoi = nk > 0 ? yKhoi / nk : -9f;
                Ghi(string.Format("F. luoi lua dang song {0} (khung cuoi), {1} luot luoi liem cao qua dinh dau (cao nhat vuot {2:F2} m); khoi {3} lan, trung binh cao hon dinh dau {4:F2} m",
                    k, trenDau, vuot, nk, yKhoi));
                Kiem(k >= 8, "qua it luoi lua boc len");
                // 04/10/2026: luoi lua van liem len tren dau (ngon lua o dau) nhung KHONG vot cao lo lung (lan truoc 0,72 m)
                Kiem(trenDau >= 1 && vuot > 0.05f && vuot < 0.45f, "luoi lua tren dau khong liem len / vot cao lo lung tren khong");
                Kiem(xaXuongMax < 0.30f && tocMax < 0.1f, "luoi lua tach khoi than, bay lo lung");
                Kiem(thanDuoi >= 2 && thanTren >= 2, "luoi lua khong bao quanh toan than (thieu than duoi / than tren)");
                Kiem(nk >= 3 && yKhoi > 0f, "khong co khoi boc len tren dau");

                var c = mauPc.transform.position;
                cam.transform.position = c + new Vector3(0f, 2.6f, -3.6f);
                cam.transform.LookAt(c + Vector3.up * 0.9f);
                yield return new WaitForEndOfFrame();
                Chup(cam, "PlayTestShots/lua_chay_boc_gan.png");
                // Khoi THAY DUOC khong: bat / tat rieng lop khoi, dem diem anh doi > 0,02
                var rk = l.KhoiBoc.GetComponent<ParticleSystemRenderer>();
                var coKhoi = ChupMau(cam); rk.enabled = false; var khongKhoi = ChupMau(cam); rk.enabled = true;
                int doiKhoi = 0; float doiTB = 0f;
                for (int i = 0; i < coKhoi.Length; i++)
                {
                    float dk = Mathf.Abs(Sang(coKhoi[i]) - Sang(khongKhoi[i]));
                    if (dk > 0.02f) { doiKhoi++; doiTB += dk; }
                }
                Ghi(string.Format("F2. khoi lam doi {0} diem anh ({1:P2} anh {2}x{3}), moi diem doi trung binh {4:F3}",
                    doiKhoi, (float)doiKhoi / coKhoi.Length, RongAnh, CaoAnh, doiKhoi > 0 ? doiTB / doiKhoi : 0f));
                Kiem(doiKhoi > 400, "khoi co ma khong nhin thay");
                yield return DoChuyenDong(cam, mauPc, l);
                float dongMoi = ketQuaDong;
                // DOI CHUNG: thong so lop phu cua lan 3 tren CHINH vat lieu nay
                l.LopPhu.SetFloat("_TocDo", 0.9f); l.LopPhu.SetFloat("_Xoan", 0f); l.LopPhu.SetFloat("_NhapNhay", 0f);
                yield return DoChuyenDong(cam, mauPc, l);
                float dongCu = ketQuaDong;
                l.LopPhu.SetFloat("_TocDo", LuaToanThan.TocDoLua); l.LopPhu.SetFloat("_Xoan", LuaToanThan.XoanLua); l.LopPhu.SetFloat("_NhapNhay", LuaToanThan.NhapNhayLua);
                Ghi(string.Format("F3. lua tren than doi trong dung 0,05 s (TB 6 cap): {0:F4} (doi chung thong so lan 3: {1:F4}) -> x{2:F2}", dongMoi, dongCu, dongCu > 0f ? dongMoi / dongCu : 0f));
                Kiem(dongMoi > dongCu * 1.3f, "lua tren than khong dong hon lan 3");
            }
        }

        // ================= C1. HET GIO -> TAT NGAY =================
        bool daThayHet = false; int khungSauHet = -1; bool conHinh = false;
        while (Time.time - t0 < giayChay + 1f)
        {
            bool coChay = mauPc.GetComponent<BurningEffect>() != null;
            bool coHinh = mauPc.GetComponentInChildren<LuaToanThan>() != null;
            if (!coChay && !daThayHet) { daThayHet = true; khungSauHet = 0; conHinh = coHinh; }
            else if (daThayHet && khungSauHet == 0) { khungSauHet = 1; conHinh |= coHinh; }
            yield return null;
        }
        Ghi(string.Format("C1. het chay ({0:F2} s): khung dau khong con BurningEffect -> vat hinh lua con: {1}", giayChay, conHinh));
        Kiem(daThayHet && !conHinh, "het gio chay ma lua van con");
        int conVatLieu = 0;
        foreach (var r in mauPc.GetComponentsInChildren<Renderer>())
            foreach (var m in r.sharedMaterials) if (m != null && m.name.StartsWith("P_LuaPhuThan")) conVatLieu++;
        Ghi("C1. vat lieu lua con tren than sau khi het: " + conVatLieu);
        Kiem(conVatLieu == 0, "het chay ma lop lua van dinh tren than");

        // DOI CHUNG muc A: khoi lua billboard lan 2
        LuaToanThan.DoiChungKhoiLua = true;
        foreach (var d in tatCa) BurningEffect.Apply(d, 0.01f, 3.5f, null);
        float tdc = Time.time; while (Time.time - tdc < 1.2f) yield return null;
        float tongDc = 0f; int soDc = 0;
        foreach (var d in tatCa)
        {
            var l = d.GetComponentInChildren<LuaToanThan>();
            if (l == null) continue;
            var c = d.transform.position;
            cam.transform.position = c + new Vector3(0f, 2.6f, -3.6f);
            cam.transform.LookAt(c + Vector3.up * 0.9f);
            yield return new WaitForEndOfFrame();
            float trongVien, phu;
            DoAnh(cam, d, l, out trongVien, out phu, d == mauPc ? "PlayTestShots/lua_chay_vien_doichung.png" : null, false);
            Ghi(string.Format("A. DOI CHUNG khoi lua lan 2 {0}: vung co lua nam trong vien {1:P0} (theo nang luong {3:P0}); phu than {2:P0}", d.name, trongVien, phu, NangLuongTrong));
            tongDc += trongVien; soDc++;
        }
        // Trung binh ca ba (bo xuong cam khien + kiem nen vien than rong, rieng no doi chung ra 84%)
        Ghi(string.Format("A. DOI CHUNG trung binh: {0:P0}", soDc > 0 ? tongDc / soDc : -1f));
        Kiem(soDc > 0 && tongDc / soDc < 0.75f, "doi chung khoi billboard lai nam trong vien - phep do khong phan biet duoc");
        foreach (var d in tatCa) { var be = d.GetComponent<BurningEffect>(); if (be != null) Object.Destroy(be); }
        LuaToanThan.DoiChungKhoiLua = false;
        yield return null;

        // ================= B. DI CHUYEN: LUA BAM THEO NGUOI =================
        var dB = quai[0];
        var hongB = Hong(dB);
        var ketQua = new float[2];
        for (int bien = 0; bien < 2; bien++)
        {
            LuaToanThan.DoiChungKhoiLua = bien == 1;
            LuaToanThan.DoiChungTheGioi = bien == 1;
            var goc = p + new Vector3(-6f, 0f, 3f); goc.y = DatY(goc);
            DatCho(dB, goc);
            dB.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            BurningEffect.Apply(dB, 0.01f, 3f, null);
            float tb = Time.time; while (Time.time - tb < 0.8f) yield return null;   // dung yen cho du lua
            float tc = Time.time;
            float lechGoc = 0f;
            while (Time.time - tc < 1.0f)
            {
                var q = dB.transform.position + Vector3.right * 6f * Time.deltaTime;
                q.y = DatY(q);
                DatCho(dB, q);
                yield return null;
                var lb = dB.GetComponentInChildren<LuaToanThan>();
                if (lb != null && lb.LopPhu != null)
                {
                    Vector4 g = lb.LopPhu.GetVector("_Goc");
                    lechGoc = Mathf.Max(lechGoc, Vector2.Distance(new Vector2(g.x, g.z), new Vector2(hongB.position.x, hongB.position.z)));
                }
            }
            var c = dB.transform.position;
            cam.transform.position = c + new Vector3(0f, 2.6f, -3.6f);
            cam.transform.LookAt(c + Vector3.up * 0.9f);
            yield return new WaitForEndOfFrame();
            var l = dB.GetComponentInChildren<LuaToanThan>();
            float trongVien = -1f, phu = -1f;
            if (l != null) DoAnh(cam, dB, l, out trongVien, out phu, bien == 0 ? "PlayTestShots/lua_chay_dang_chay.png" : "PlayTestShots/lua_chay_dang_chay_doichung.png", bien == 0);
            ketQua[bien] = trongVien;
            Ghi(string.Format("B. {0}: chay 6 m/s trong 1 s -> lua nam trong vien than {1:P0}, phu than {2:P0}{3}",
                bien == 0 ? "BAN SUA (lop lua tren than)" : "DOI CHUNG khoi billboard + mo phong the gioi (lan 1-2)", trongVien, phu,
                bien == 0 ? string.Format("; goc toa do lua lech Hips toi da {0:F3} m", lechGoc) : ""));
            if (bien == 0) Kiem(lechGoc < 0.05f, "goc toa do lua khong di theo nguoi");
            Object.Destroy(dB.GetComponent<BurningEffect>());
            yield return null; yield return null;
        }
        LuaToanThan.DoiChungTheGioi = false;
        LuaToanThan.DoiChungKhoiLua = false;
        Kiem(ketQua[0] >= 0.90f, "dang chay ma lua tran ra ngoai / bi bo lai");
        Kiem(ketQua[1] >= 0f && ketQua[1] < 0.6f, "doi chung (lua bi bo lai) van nam trong vien - phep do khong bat duoc loi cu");

        // ================= C2. BI GO GIUA CHUNG -> TAT NGAY =================
        var dC = quai[1];
        BurningEffect.Apply(dC, 0.01f, 5f, null);
        float t2 = Time.time; while (Time.time - t2 < 1.0f) yield return null;
        bool coTruoc = dC.GetComponentInChildren<LuaToanThan>() != null;
        Object.Destroy(dC.GetComponent<BurningEffect>());    // nhu TocBien.GoSachTrangThai
        yield return null;
        bool conSau = dC.GetComponentInChildren<LuaToanThan>() != null;
        Ghi(string.Format("C2. go giua chung: truoc co lua {0} -> khung ke tiep con lua {1}", coTruoc, conSau));
        Kiem(coTruoc && !conSau, "bi go giua chung ma lua khong mat ngay");

        // ================= E. NHIP CHAY KHONG PHUN TIA TRUNG DON =================
        var dE = quai[0];
        var daCo = new HashSet<Transform>();
        foreach (var t in Object.FindObjectsByType<Transform>()) if (t.parent == null) daCo.Add(t);
        float mauTruoc = dE.health;
        BurningEffect.Apply(dE, 5f, 2.2f, null);
        int chumChay = 0;
        float te = Time.time;
        while (Time.time - te < 2.0f)
        {
            foreach (var t in Object.FindObjectsByType<Transform>())
                if (t.parent == null && (t.name.Contains("TrungDon") || t.name == "Hit") && daCo.Add(t)) chumChay++;
            yield return null;
        }
        float mauMat = mauTruoc - dE.health;
        dE.TakeDamage(10f, DamageType.Fire, dE.transform.position + Vector3.up);
        int chumDon = 0;
        yield return null;
        foreach (var t in Object.FindObjectsByType<Transform>())
            if (t.parent == null && (t.name.Contains("TrungDon") || t.name == "Hit") && daCo.Add(t)) chumDon++;
        Ghi(string.Format("E. 2 s chay: mat {0:F1} mau, {1} chum tia trung don | DOI CHUNG 1 don lua thuong: {2} chum", mauMat, chumChay, chumDon));
        Kiem(mauMat > 5f, "chay khong con tru mau");
        Kiem(chumChay == 0, "nhip chay van phun tia trung don");
        Kiem(chumDon >= 1, "doi chung: don thuong khong sinh chum tia - phep dem khong bat duoc gi");
        Object.Destroy(dE.GetComponent<BurningEffect>());

        float tk = Time.time; while (Time.time - tk < 0.5f) yield return null;
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void KhungDoc(Damageable d, out float lo, out float hi)
    {
        lo = 1e9f; hi = -1e9f;
        foreach (var smr in d.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var m = new Mesh();
            smr.BakeMesh(m, true);
            foreach (var v in m.vertices) { float y = smr.transform.TransformPoint(v).y; if (y < lo) lo = y; if (y > hi) hi = y; }
            Object.DestroyImmediate(m);
        }
    }

    const int RongAnh = 480, CaoAnh = 540;
    static float NangLuongTrong;
    const int NoiVien = 14;

    static Color[] ChupMau(Camera cam, Shader thay = null)
    {
        var rt = RenderTexture.GetTemporary(RongAnh, CaoAnh, 24, RenderTextureFormat.ARGB32);
        var cuRt = cam.targetTexture;
        cam.targetTexture = rt;
        if (thay != null) cam.RenderWithShader(thay, ""); else cam.Render();
        cam.targetTexture = cuRt;
        var cu = RenderTexture.active;
        RenderTexture.active = rt;
        var tx = new Texture2D(RongAnh, CaoAnh, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, RongAnh, CaoAnh), 0, 0);
        tx.Apply();
        RenderTexture.active = cu;
        RenderTexture.ReleaseTemporary(rt);
        var px = tx.GetPixels();
        Object.DestroyImmediate(tx);
        return px;
    }

    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static List<Renderer> RendererThan(Damageable d, LuaToanThan l)
    {
        var than = new List<Renderer>();
        foreach (var r in d.GetComponentsInChildren<Renderer>())
            if (r.enabled && (r is SkinnedMeshRenderer || r is MeshRenderer) && !r.transform.IsChildOf(l.transform)) than.Add(r);
        return than;
    }

    /// <summary>Bong than: rieng cac renderer nay ve TRANG DAC tren nen den (lop 31, shader thay the).</summary>
    static bool[] BongThan(Camera cam, List<Renderer> than)
    {
        var lopCu = new List<int>();
        foreach (var r in than) { lopCu.Add(r.gameObject.layer); r.gameObject.layer = 31; }
        var cf = cam.clearFlags; var nen = cam.backgroundColor; int mat = cam.cullingMask;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black; cam.cullingMask = 1 << 31;
        var a2 = ChupMau(cam, Shader.Find("Hidden/Diablo25D/TrangDacPhepThu"));
        cam.clearFlags = cf; cam.backgroundColor = nen; cam.cullingMask = mat;
        for (int k = 0; k < than.Count; k++) than[k].gameObject.layer = lopCu[k];
        var bong = new bool[a2.Length];
        for (int i = 0; i < a2.Length; i++) bong[i] = a2[i].r > 0.5f;
        return bong;
    }

    static float ketQuaDong;

    /// <summary>
    /// Lua tren than CHUYEN DONG bao nhieu: chup LOP PHU (an luoi lua, khoi, den, bloom) o hai thoi diem cach nhau DUNG 0,05 s
    /// - dat thang dong ho _ThoiGian cua vat lieu, ca hai trong CUNG mot khung (than dung yen tuyet doi). Trung binh |do sang
    /// khac nhau| tren bong than, lay trung binh 6 cap thoi diem. Lan dau cho troi theo khung Editor that (0,04-0,1 s, dai ngan
    /// khac nhau) -> so lieu dao ca chieu.
    /// </summary>
    static IEnumerator DoChuyenDong(Camera cam, Damageable d, LuaToanThan l)
    {
        yield return new WaitForEndOfFrame();
        var than = RendererThan(d, l);
        var bong = BongThan(cam, than);
        var bloom = cam.GetComponent<SimpleBloom>();
        bool bloomBat = bloom != null && bloom.enabled;
        if (bloom != null) bloom.enabled = false;
        bool denBat = l.Den != null && l.Den.enabled;
        if (l.Den != null) l.Den.enabled = false;
        l.AnHinh(false, false);
        float T0 = Time.time, tong = 0f; int dem = 0;
        for (int cap = 0; cap < 6; cap++)
        {
            l.LopPhu.SetFloat("_ThoiGian", T0 + cap * 0.37f);
            var a = ChupMau(cam);
            l.LopPhu.SetFloat("_ThoiGian", T0 + cap * 0.37f + 0.05f);
            var b = ChupMau(cam);
            for (int i = 0; i < a.Length; i++) if (bong[i]) { tong += Mathf.Abs(Sang(a[i]) - Sang(b[i])); dem++; }
        }
        l.AnHinh(false);
        if (l.Den != null) l.Den.enabled = denBat;
        if (bloom != null) bloom.enabled = bloomBat;
        ketQuaDong = dem > 0 ? tong / dem : 0f;
    }

    /// <summary>
    /// Ba anh CUNG MOT KHUNG (khong ai cap nhat giua ba lan Render): co than + an lua, co than + co lua, va BONG THAN = rieng
    /// nhan vat nay ve TRANG DAC tren nen den (lop 31, shader thay the). Lan dau lay bong bang "co than tru an than" thi dinh
    /// ca BONG DO tren dat (than tat thi bong do cung mat) va sot phan ao toi lan vao nen dem -> doi chung khoi billboard ra
    /// "93% trong vien". Den lua tat ca ba (chi do HINH lua, khong do anh sang no hat len dat).
    /// </summary>
    static void DoAnh(Camera cam, Damageable d, LuaToanThan l, out float trongVien, out float phu, string luuAnh, bool chiLopPhu = true)
    {
        var than = RendererThan(d, l);
        bool denBat = l.Den != null && l.Den.enabled;
        if (l.Den != null) l.Den.enabled = false;
        // Tat BLOOM khi do HINH lua: quang toa sang cua bloom quanh moi vat sang (lo lua cung co) ra mot vanh mo 20-30 diem
        // anh ngoai vien - lan do thu hai tuong la lop vo lua tran ra (76-87% "trong vien").
        var bloom = cam.GetComponent<SimpleBloom>();
        bool bloomBat = bloom != null && bloom.enabled;
        if (bloom != null) bloom.enabled = false;

        // Chi LOP PHU tren than (luoi lua + khoi boc len tren dau la co y - lan 4 - do rieng o muc F)
        l.AnHinh(true);
        var a1 = ChupMau(cam);
        l.AnHinh(false, !chiLopPhu);      // doi chung khoi billboard: hinh lua CHINH la hat -> phai ve ca hat
        var a3 = ChupMau(cam);
        l.AnHinh(false);
        if (l.Den != null) l.Den.enabled = denBat;
        if (bloom != null) bloom.enabled = bloomBat;

        int n = RongAnh * CaoAnh;
        var bong = BongThan(cam, than);
        // Noi vien bong than NoiVien diem anh (hai luot tach: ngang roi doc)
        var tam = new bool[n]; var vien = new bool[n];
        for (int y = 0; y < CaoAnh; y++)
        {
            int cuoi = -100000;
            for (int x = 0; x < RongAnh; x++) { if (bong[y * RongAnh + x]) cuoi = x; if (x - cuoi <= NoiVien) tam[y * RongAnh + x] = true; }
            cuoi = 100000;
            for (int x = RongAnh - 1; x >= 0; x--) { if (bong[y * RongAnh + x]) cuoi = x; if (cuoi - x <= NoiVien) tam[y * RongAnh + x] = true; }
        }
        for (int x = 0; x < RongAnh; x++)
        {
            int cuoi = -100000;
            for (int y = 0; y < CaoAnh; y++) { if (tam[y * RongAnh + x]) cuoi = y; if (y - cuoi <= NoiVien) vien[y * RongAnh + x] = true; }
            cuoi = 100000;
            for (int y = CaoAnh - 1; y >= 0; y--) { if (tam[y * RongAnh + x]) cuoi = y; if (cuoi - y <= NoiVien) vien[y * RongAnh + x] = true; }
        }
        float trong = 0f, tong = 0f; int soBong = 0, bongCoLua = 0, dtLua = 0, dtTrong = 0;
        var anh = luuAnh != null ? new Color[n] : null;
        for (int i = 0; i < n; i++)
        {
            float them = Mathf.Max(0f, Sang(a3[i]) - Sang(a1[i]));
            tong += them;
            if (vien[i]) trong += them;
            if (them > 0.05f) { dtLua++; if (vien[i]) dtTrong++; }
            if (bong[i]) { soBong++; if (them > 0.08f) bongCoLua++; }
            if (anh != null)
            {
                // Anh soi: anh co lua, vien noi rong ve do (nhu net nguoi dung ve), lua ngoai vien to xanh la
                var c = a3[i];
                bool bien = vien[i] && ((i % RongAnh > 0 && !vien[i - 1]) || (i % RongAnh < RongAnh - 1 && !vien[i + 1]) || (i >= RongAnh && !vien[i - RongAnh]) || (i < n - RongAnh && !vien[i + RongAnh]));
                if (bien) c = Color.red;
                else if (!vien[i] && them > 0.05f) c = Color.Lerp(c, Color.green, 0.7f);
                anh[i] = c;
            }
        }
        // Tinh theo DIEN TICH vung co lua (sang them > 0,05): tinh theo nang luong thi khoi billboard sang nhat o giua than
        // nen van ra 75-88% "trong vien" (doi chung lan dau).
        trongVien = dtLua > 0 ? (float)dtTrong / dtLua : 0f;
        NangLuongTrong = tong > 1e-3f ? trong / tong : 0f;
        phu = soBong > 0 ? (float)bongCoLua / soBong : 0f;
        if (anh != null)
        {
            var tx = new Texture2D(RongAnh, CaoAnh, TextureFormat.RGB24, false);
            tx.SetPixels(anh); tx.Apply();
            File.WriteAllBytes(luuAnh, tx.EncodeToPNG());
            Object.DestroyImmediate(tx);
        }
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
        LuaToanThan.DoiChungTheGioi = false;
        LuaToanThan.DoiChungKhoiLua = false;
        File.WriteAllText("PlayTestShots/lua_chay.txt", bao.ToString());
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
