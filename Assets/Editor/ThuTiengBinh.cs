using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 137 - TIENG BINH MAU / BINH MANA (16/10/2026, nguoi dung: tieng UONG nhu nuoc chay nguoi choi dang uong; tieng binh BI HUT ve phia nguoi
/// choi va tieng LAY DUOC binh; nguoi choi khac trong suong thi khong nghe). Mau A "Chan that" (CongCu/AmThanh/tieng_binh.py). Nguoi dung chon:
/// tieng uong CHI may nguoi uong (khong qua mang). Play Act2 that, tat GameDirector + xoa quai, BAT tam nhin + tieng.
///   A. (ngoai Play) 12 file trung tung byte ban goc, STEREO, khong ForceToMono.
///   B. UONG THAT (PlayerController.UongBinh): mau -> 1 tieng uong MAU, mana -> 1 tieng uong MANA, bam theo nguoi, can bang (2D); loa co tieng,
///      khong mono; mau / mana day -> khong uong, khong keu.
///   C. HUT + NHAT: binh that (BinhRoi.Tao) cach 3 m bay vao nhan vat: tieng hut bam theo BINH luc bay; toi noi -> tieng nhat bam theo nguoi,
///      duoi tieng hut chuyen sang bam nguoi (khong tat); loa co tieng.
///   D. SUONG MU: binh bay vao "nguoi khac" o tam nhin + 10 m: hut + nhat muc 0, loa im; doi chung tat suong -> co muc.
///   E. BIEN THE: 24 lan moi loai -> du 3 bien the, cao do +-6%.
/// Ket qua: PlayTestShots/tiengbinh.txt.
/// </summary>
public static class ThuTiengBinh
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat, tatTiengGoc;
    static EnterPlayModeOptions truocOpt;
    const string Ten = "TAM_TiengBinh";

    [MenuItem("Diablo 2.5D/137. Chay thu TIENG BINH MAU · MANA (uong · hut · nhat, suong mu)", false, 227)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[TiengBinh] dang Play"); return; }
        if (EditorSceneManager.GetActiveScene().isDirty) { Debug.LogWarning("[TiengBinh] scene dang mo co thay doi chua luu"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng binh mau A Chan that, 3 bien the moi tieng");
        int dung = 0, tong = 0;
        var ds = new[] {
            new[] { "BinhUongMau", "CongCu/AmThanh/BinhThu/A_that_uong_mau" }, new[] { "BinhUongMana", "CongCu/AmThanh/BinhThu/A_that_uong_mana" },
            new[] { "BinhHut", "CongCu/AmThanh/BinhThu/A_that_hut" }, new[] { "BinhNhat", "CongCu/AmThanh/BinhThu/A_that_nhat" } };
        foreach (var kv in ds)
            for (int i = 1; i <= 3; i++)
            {
                tong++;
                string game = "Assets/Resources/AmThanh/KyNang/" + kv[0] + i + ".wav";
                string g = kv[1] + i + ".wav";
                var imp = AssetImporter.GetAtPath(game) as AudioImporter;
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game);
                bool trung = c != null && File.Exists(g) && Md5(g) == Md5(game);
                bool ok = trung && c.channels == 2 && imp != null && !imp.forceToMono;
                if (ok) dung++;
                else Ghi(string.Format("A. {0}{1}: trung ban goc {2}; {3} kenh; ForceToMono {4}", kv[0], i, trung, c != null ? c.channels : 0, imp != null && imp.forceToMono));
            }
        Ghi(string.Format("A. {0}/{1} file trung ban goc, 2 kenh, khong ForceToMono", dung, tong));
        Kiem(dung == tong, "co file khong dung ban goc / khong STEREO");
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        tatTiengGoc = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = false;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static string Md5(string d) { using (var m = MD5.Create()) using (var f = File.OpenRead(d)) return System.BitConverter.ToString(m.ComputeHash(f)); }
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TiengBinh] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find(Ten) != null) return;
        daBatDau = true;
        var go = new GameObject(Ten);
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static int SL(TiengViTri.Loai l) { return TiengViTri.SoLan[(int)l]; }

    static List<TiengViTri.Giong> Giong(TiengViTri.Loai l)
    {
        var ds = new List<TiengViTri.Giong>();
        foreach (var g in TiengViTri.CacGiong) if (!g.Ranh && g.loai == l && !g.dung) ds.Add(g);
        return ds;
    }

    static IEnumerator ChoIm(float toiDa)
    {
        float han = Time.realtimeSinceStartup + toiDa;
        while (Time.realtimeSinceStartup < han)
        {
            bool ranh = true;
            foreach (var g in TiengViTri.CacGiong) if (!g.Ranh) ranh = false;
            if (ranh) break;
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.15f);
    }

    /// <summary>Nghe loa: kq = {RMS TB, nang luong trai, nang luong phai, tuong quan, RMS dinh}.</summary>
    static IEnumerator Nghe(float giay, float[] kq)
    {
        var bl = new float[1024]; var br = new float[1024];
        double sl = 0, sr = 0, slr = 0, tong = 0; int so = 0; float dinh = 0;
        float han = Time.realtimeSinceStartup + giay;
        while (Time.realtimeSinceStartup < han)
        {
            yield return null;
            AudioListener.GetOutputData(bl, 0);
            AudioListener.GetOutputData(br, 1);
            double a = 0, b = 0, ab = 0;
            for (int i = 0; i < bl.Length; i++) { a += bl[i] * bl[i]; b += br[i] * br[i]; ab += bl[i] * br[i]; }
            sl += a; sr += b; slr += ab;
            float rms = (float)System.Math.Sqrt((a + b) / (2.0 * bl.Length));
            tong += rms; so++; dinh = Mathf.Max(dinh, rms);
        }
        kq[0] = so > 0 ? (float)(tong / so) : 0f;
        kq[1] = (float)sl; kq[2] = (float)sr;
        kq[3] = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        kq[4] = dinh;
    }

    static Vector3 Dat(Vector3 p) { p.y = GioLoc.MatDatY(p, p.y + 5f); return p; }

    static Damageable TaoNguoi(string ten, Vector3 p, bool banSao)
    {
        var go = new GameObject(ten);
        go.transform.position = p;
        var d = go.AddComponent<Damageable>();
        d.maxHealth = d.health = 1e7f; d.tiLeDoDon = 0f;
        d.mauDoMayKhacQuyet = banSao;
        return d;
    }

    static TiengViTri.Giong GiongMoi(int truocPhat, TiengViTri.Loai l)
    {
        var gm = TiengViTri.GiongMoiNhat;
        return TiengViTri.SoLanDuocGiong > truocPhat && gm != null && gm.loai == l ? gm : null;
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
        NhacNen.BatTrongPhepThu = true;
        TamNhin.BatTrongPhepThu = true;
        var nn = NhacNen.Ban;
        if (nn != null) { nn.sanh.nguon.mute = true; nn.tran.nguon.mute = true; }
        yield return new WaitForSeconds(1.0f);
        var rig = Object.FindAnyObjectByType<CameraRig>();
        if (rig != null) rig.enabled = false;
        var cam = Camera.main.transform;
        Vector3 truoc = cam.forward; truoc.y = 0; truoc.Normalize();
        Vector3 phai = Vector3.Cross(Vector3.up, truoc);
        float[] kq = new float[5];
        CapDo.BatDauTranMoi();
        Vector3 goc = pc.transform.position;
        yield return ChoIm(3f);

        // ===== B. uong that =====
        Ghi("");
        foreach (bool laMau in new[] { true, false })
        {
            int ky = laMau ? CapDo.KyBinhMau : CapDo.KyBinhMana;
            var loai = laMau ? TiengViTri.Loai.BinhUongMau : TiengViTri.Loai.BinhUongMana;
            var loaiKia = laMau ? TiengViTri.Loai.BinhUongMana : TiengViTri.Loai.BinhUongMau;
            for (int i = 0; i < 3; i++) CapDo.ThemBinh(ky);
            if (laMau) toi.health = toi.maxHealth * 0.4f; else pc.mana = 0f;
            int u0 = SL(loai), k0 = SL(loaiKia);
            float hoi = pc.UongBinh(ky);
            yield return null;
            var gU = Giong(loai);
            bool theo = gU.Count == 1 && gU[0].theo == pc.transform && gU[0].bamTheo && gU[0].nguon.spatialBlend == 0f;
            yield return Nghe(1.2f, kq);
            Ghi(string.Format("B. uong binh {0}: hoi {1:F0}; tieng uong +{2} (loai kia +{3}), giong {4} (bam theo nguoi + can bang 2D {5}); loa RMS {6:F4} (dinh {7:F4}), tuong quan {8:F3}",
                laMau ? "MAU" : "MANA", hoi, SL(loai) - u0, SL(loaiKia) - k0, gU.Count, theo, kq[0], kq[4], kq[3]));
            Kiem(hoi > 0f && SL(loai) - u0 == 1 && SL(loaiKia) - k0 == 0 && theo, "uong binh khong ra dung 1 tieng uong bam theo nguoi");
            Kiem(kq[0] > 1e-3f && kq[3] < 0.9f, "loa khong co tieng uong / gan mono");
            yield return ChoIm(4f);
            // day -> khong uong, khong keu
            if (laMau) toi.health = toi.maxHealth; else pc.mana = pc.maxMana;
            yield return new WaitForSeconds(0.6f);
            int u1 = SL(loai);
            float hoi2 = pc.UongBinh(ky);
            Ghi(string.Format("B2. {0} DAY: hoi {1:F0}, tieng uong +{2}", laMau ? "mau" : "mana", hoi2, SL(loai) - u1));
            Kiem(hoi2 == 0f && SL(loai) - u1 == 0, "day ma van uong / van keu");
        }
        yield return ChoIm(3f);

        // ===== C. hut + nhat =====
        Ghi("");
        {
            int h0 = SL(TiengViTri.Loai.BinhHut), n0 = SL(TiengViTri.Loai.BinhNhat);
            int soMau0 = CapDo.SoBinh(CapDo.KyBinhMau);
            // binh that cach 3 m (trong vung hut 3,5 m): GAME TU HUT (BinhRoi.KiemNhanVatToiGan -> QuanLyBinhRoi.XinNhat -> BayVao) nhu luc choi
            var b = BinhRoi.Tao(990001, CapDo.KyBinhMau, Dat(goc + phai * 3f));
            float hanHut = Time.time + 2f;
            while (b != null && !b.DaCoChu && Time.time < hanHut) yield return null;
            yield return null;
            var gH = Giong(TiengViTri.Loai.BinhHut);
            bool hutTheoBinh = gH.Count == 1 && gH[0].theo == b.transform && gH[0].bamTheo;
            var gHut = gH.Count == 1 ? gH[0] : null;
            float rmsBay = 0f;
            float t0 = Time.time;
            while (b != null && Time.time - t0 < 3f) { yield return Nghe(0.05f, kq); rmsBay = Mathf.Max(rmsBay, kq[0]); }
            float giayBay = Time.time - t0;
            yield return null;
            var gN = Giong(TiengViTri.Loai.BinhNhat);
            bool nhatTheo = gN.Count == 1 && gN[0].theo == pc.transform && gN[0].bamTheo;
            bool hutConKeu = gHut != null && !gHut.Ranh && !gHut.dung && gHut.theo == pc.transform;
            yield return Nghe(0.6f, kq);
            Ghi(string.Format("C. binh mau cach 3 m bay vao: tieng hut +{0} (bam theo BINH {1}), bay {2:F2} s, loa RMS lon nhat luc bay {3:F4}; toi noi: tieng nhat +{4} (bam theo nguoi {5}), duoi tieng hut chuyen sang bam nguoi {6}; so binh mau +{7}; loa sau khi nhat RMS {8:F4} tuong quan {9:F3}",
                SL(TiengViTri.Loai.BinhHut) - h0, hutTheoBinh, giayBay, rmsBay, SL(TiengViTri.Loai.BinhNhat) - n0, nhatTheo, hutConKeu,
                CapDo.SoBinh(CapDo.KyBinhMau) - soMau0, kq[0], kq[3]));
            Kiem(SL(TiengViTri.Loai.BinhHut) - h0 == 1 && hutTheoBinh, "binh bay vao khong ra dung 1 tieng hut bam theo binh");
            Kiem(SL(TiengViTri.Loai.BinhNhat) - n0 == 1 && nhatTheo && hutConKeu, "toi noi khong ra tieng nhat bam theo nguoi / tieng hut bi cat");
            Kiem(rmsBay > 1e-3f && kq[0] > 1e-3f && kq[3] < 0.9f, "loa khong co tieng hut / nhat / gan mono");
            Kiem(CapDo.SoBinh(CapDo.KyBinhMau) - soMau0 == 1, "nhat binh khong cong so binh");
            yield return ChoIm(4f);
        }

        // ===== D. suong mu =====
        Ghi("");
        {
            Vector3 xa = Dat(goc + truoc * (TamNhin.BanKinh + 10f));
            var nguoiXa = new GameObject("TAM_NguoiXa").transform;
            nguoiXa.position = xa;
            var b = BinhRoi.Tao(990002, CapDo.KyBinhMana, Dat(xa + phai * 3f));
            yield return null;
            Transform tBinh = b.transform;
            b.BayVao(nguoiXa, 1, false);
            float mucMax = 0f; int giongCo = 0;
            float t0 = Time.time;
            while (Time.time - t0 < 1.0f)
            {
                yield return null;
                foreach (var l in new[] { TiengViTri.Loai.BinhHut, TiengViTri.Loai.BinhNhat })
                    foreach (var g in Giong(l)) { giongCo++; mucMax = Mathf.Max(mucMax, g.MucDangDat); }
            }
            yield return Nghe(0.3f, kq);
            float rmsXa = kq[0];
            Ghi(string.Format("D. binh bay vao nguoi khac o {0:F0} m (MucThay {1:F2}): giong hut / nhat thay {2} lan, muc lon nhat {3:F3}; loa RMS {4:F5}",
                TamNhin.BanKinh + 10f, TamNhin.MucThay(xa), giongCo, mucMax, rmsXa));
            Kiem(giongCo > 0 && mucMax == 0f && rmsXa < 1e-4f, "binh trong suong van nghe thay");
            yield return ChoIm(3f);
            // doi chung tat suong
            TamNhin.BatTrongPhepThu = false;
            yield return null; yield return null;
            var b2 = BinhRoi.Tao(990003, CapDo.KyBinhMana, Dat(xa + phai * 3f));
            yield return null;
            b2.BayVao(nguoiXa, 1, false);
            float mucDc = 0f; float t1 = Time.time;
            while (Time.time - t1 < 1.0f)
            {
                yield return null;
                foreach (var l in new[] { TiengViTri.Loai.BinhHut, TiengViTri.Loai.BinhNhat })
                    foreach (var g in Giong(l)) mucDc = Mathf.Max(mucDc, g.MucDangDat);
            }
            Ghi(string.Format("D2. DOI CHUNG tat suong: muc lon nhat {0:F3}", mucDc));
            Kiem(mucDc > 0.05f, "doi chung tat suong ma van im");
            TamNhin.BatTrongPhepThu = true;
            Object.Destroy(nguoiXa.gameObject);
            yield return ChoIm(4f);
        }

        // ===== E. bien the + cao do =====
        Ghi("");
        var loaiE = new[] { TiengViTri.Loai.BinhUongMau, TiengViTri.Loai.BinhUongMana, TiengViTri.Loai.BinhHut, TiengViTri.Loai.BinhNhat };
        var dongG = new List<string>(); bool du = true;
        float caoMin = 9f, caoMax = 0f;
        var vat = new GameObject("TAM_VatGia").transform;
        foreach (var l in loaiE)
        {
            var clipL = new HashSet<string>(); int soDuoc = 0;
            for (int i = 0; i < 24; i++)
            {
                vat.position = Dat(goc + truoc * (3f + (i % 4) * 3f) + phai * ((i % 5) - 2f) * 2f);
                int tp = TiengViTri.SoLanDuocGiong;
                if (l == TiengViTri.Loai.BinhUongMau) TiengViTri.BinhUong(vat, true);
                else if (l == TiengViTri.Loai.BinhUongMana) TiengViTri.BinhUong(vat, false);
                else if (l == TiengViTri.Loai.BinhHut) TiengViTri.BinhHut(vat);
                else TiengViTri.BinhNhat(vat);
                var gm = GiongMoi(tp, l);
                if (gm != null && gm.nguon != null && gm.nguon.clip != null)
                {
                    soDuoc++; clipL.Add(gm.nguon.clip.name);
                    caoMin = Mathf.Min(caoMin, gm.nguon.pitch); caoMax = Mathf.Max(caoMax, gm.nguon.pitch);
                }
                yield return new WaitForSeconds(0.25f);
            }
            dongG.Add(l + " " + clipL.Count + " (" + soDuoc + " lan duoc giong)");
            if (clipL.Count != 3) du = false;
            yield return ChoIm(6f);
        }
        Object.Destroy(vat.gameObject);
        Ghi(string.Format("E. 24 lan moi loai: so bien the {0}; cao do {1:F3} - {2:F3}", string.Join(", ", dongG), caoMin, caoMax));
        Kiem(du, "khong du 3 bien the moi loai");
        Kiem(caoMin >= 1f - TiengViTri.LechCaoDo - 1e-4f && caoMax <= 1f + TiengViTri.LechCaoDo + 1e-4f && caoMax - caoMin > 0.06f, "cao do khong lech ngau nhien trong +-6%");
        yield return ChoIm(4f);
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengbinh.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        TamNhin.BatTrongPhepThu = false;
        TranHienTai.Xoa();
        var rac = GameObject.Find(Ten);
        if (rac != null) Object.DestroyImmediate(rac);
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
        EditorUtility.audioMasterMute = tatTiengGoc;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        Debug.Log("[TiengBinh] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
