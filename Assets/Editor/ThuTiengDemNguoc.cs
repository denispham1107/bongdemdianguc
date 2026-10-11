using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 123 - TIENG DEM NGUOC (11/10/2026, nguoi dung chon mau "Nhịp tim").
///
/// A. NGOAI PLAY: DemNguoc.wav / DemNguocCuoi.wav trung tung byte ban goc CongCu/AmThanh/DemNguocThu/B_tim_dap_nhip*.wav, STEREO.
/// B. ManSanh.XetTiengDemNguoc voi chuoi thoi gian GIA (buoc khung ngau nhien 8-50 ms + mot khung giat 0,4 s): dung 5 tieng 5-4-3-2-1,
///    so 1 la nhip cuoi; vao giua chung (con 3,4 s) -> 4-3-2-1; het dem nguoc / dem lai -> bat dau lai.
/// C. TIENG THAT o loa theo dong ho that 5 giay (nhac nen tat tieng tam): 5 diem bat dau tieng cach nhau ~1 s, nhip cuoi to / dai
///    hon, hai kenh khac nhau (stereo).
/// Ket qua: PlayTestShots/tiengdemnguoc.txt.
/// </summary>
public static class ThuTiengDemNguoc
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool tatTiengGoc;

    [MenuItem("Diablo 2.5D/123. Chay thu TIENG DEM NGUOC (nhip tim moi giay)", false, 212)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (EditorApplication.isPlaying) { Debug.LogWarning("[DemNguoc] dang Play"); return; }
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] tieng dem nguoc");
        string[] game = { "Assets/Resources/" + TiengDong.DuongDemNguoc + ".wav", "Assets/Resources/" + TiengDong.DuongDemNguocCuoi + ".wav" };
        string[] goc = { "CongCu/AmThanh/DemNguocThu/B_tim_dap_nhip.wav", "CongCu/AmThanh/DemNguocThu/B_tim_dap_nhip_cuoi.wav" };
        for (int i = 0; i < 2; i++)
        {
            var imp = AssetImporter.GetAtPath(game[i]) as AudioImporter;
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(game[i]);
            bool trung = c != null && File.Exists(goc[i]) && Md5(goc[i]) == Md5(game[i]);
            Ghi(string.Format("A. {0}: trung ban goc {1}; {2} kenh {3:F2} s; ForceToMono {4}", Path.GetFileName(game[i]), trung,
                c != null ? c.channels : 0, c != null ? c.length : 0, imp != null && imp.forceToMono));
            Kiem(trung && c.channels == 2 && !imp.forceToMono, "tieng dem nguoc khong dung ban goc / khong STEREO");
        }
        canhCu = EditorSceneManager.GetActiveScene().path;
        tatTiengGoc = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = false;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static string Md5(string d) { using (var m = MD5.Create()) using (var f = File.OpenRead(d)) return System.BitConverter.ToString(m.ComputeHash(f)); }
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DemNguoc] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        NhacNen.BatTrongPhepThu = true;
        var go = new GameObject("TAM_ThuTiengDemNguoc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    /// <summary>Chay chuoi thoi gian gia tu batDau xuong -0,3 s; tra ve cac so da keu va loai tieng.</summary>
    static string ChuoiGia(double batDau, System.Random r, bool coGiat, out List<int> so)
    {
        so = new List<int>();
        var loai = new List<string>();
        int daKeu = -1;
        int c0 = TiengDong.SoLan[2], c1 = TiengDong.SoLan[3];
        double t = batDau; bool daGiat = false;
        while (t > -0.3)
        {
            int k = ManSanh.XetTiengDemNguoc(true, t, ref daKeu);
            if (k > 0) so.Add(k);
            double buoc = 0.008 + r.NextDouble() * 0.042;
            if (coGiat && !daGiat && t < 2.6) { buoc = 0.4; daGiat = true; }
            t -= buoc;
        }
        ManSanh.XetTiengDemNguoc(false, 0, ref daKeu);
        return string.Join("-", so) + " (nhip " + (TiengDong.SoLan[2] - c0) + ", cuoi " + (TiengDong.SoLan[3] - c1) + ")";
    }

    static IEnumerator ChayKichBan()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        var nn = NhacNen.Ban;
        if (nn == null) { Ghi("[LOI] khong co NhacNen"); loi++; Ket(); yield break; }
        nn.sanh.nguon.mute = true;
        Ghi("");

        // ---- B. chuoi thoi gian gia ---- (cac chuoi chay trong MOT khung -> hang chuc tieng cung luc: tam tat tieng hieu ung)
        CaiDatAmThanh.XemTruocHieuUng(0);
        var r = new System.Random(123);
        List<int> so;
        string kq1 = ChuoiGia(5.0, r, false, out so);
        bool dung1 = string.Join("-", so) == "5-4-3-2-1";
        string kq2 = ChuoiGia(5.0, r, true, out so);
        bool dung2 = string.Join("-", so) == "5-4-3-2-1";
        string kq3 = ChuoiGia(3.4, r, false, out so);
        bool dung3 = string.Join("-", so) == "4-3-2-1";
        string kq4 = ChuoiGia(5.0, r, false, out so);              // dem lai lan nua (vd. chu phong huy roi bat dau lai)
        bool dung4 = string.Join("-", so) == "5-4-3-2-1";
        CaiDatAmThanh.BoXemTruoc();
        Ghi("B. chuoi gia: binh thuong " + kq1 + "; co khung giat 0,4 s " + kq2 + "; vao giua chung (3,4 s) " + kq3 + "; dem lai " + kq4);
        Kiem(dung1 && dung2 && dung3 && dung4 && kq1.EndsWith("(nhip 4, cuoi 1)") && kq3.EndsWith("(nhip 3, cuoi 1)"),
             "tieng dem nguoc khong dung moi giay mot tieng / nhip cuoi o so 1");
        yield return new WaitForSecondsRealtime(2.5f);

        // ---- C. tieng that theo dong ho that ----
        var bufL = new float[512]; var bufR = new float[512];
        var tg = new List<float>(); var rms = new List<float>();
        double sl = 0, sr = 0, slr = 0;
        float t0 = Time.realtimeSinceStartup;
        int daKeu = -1;
        var lucKeu = new List<float>();
        while (Time.realtimeSinceStartup - t0 < 7.5f)
        {
            float t = Time.realtimeSinceStartup - t0;
            int k = ManSanh.XetTiengDemNguoc(t < 5.3f, 5.0 - t, ref daKeu);
            if (k > 0) lucKeu.Add(t);
            yield return null;
            AudioListener.GetOutputData(bufL, 0);
            AudioListener.GetOutputData(bufR, 1);
            double a = 0, b = 0, ab = 0;
            for (int i = 0; i < bufL.Length; i++) { a += bufL[i] * bufL[i]; b += bufR[i] * bufR[i]; ab += bufL[i] * bufR[i]; }
            sl += a; sr += b; slr += ab;
            tg.Add(Time.realtimeSinceStartup - t0);
            rms.Add((float)System.Math.Sqrt((a + b) / (2.0 * bufL.Length)));
        }
        // Diem bat dau tieng = RMS vuot nguong sau khi da duoi nguong it nhat 0,25 s
        float dinh = 0; foreach (var v in rms) dinh = Mathf.Max(dinh, v);
        float nguong = dinh * 0.25f;
        // co tre: phai xuong duoi 40% nguong (im) roi moi tinh tieng moi - duoi tieng gam cua nhip cuoi khong bi dem thanh tieng thu sau
        var batDau = new List<float>(); var dinhMoi = new List<float>();
        bool im = true;
        for (int i = 0; i < rms.Count; i++)
        {
            if (im && rms[i] >= nguong) { batDau.Add(tg[i]); dinhMoi.Add(rms[i]); im = false; }
            else if (!im && rms[i] < nguong * 0.4f && tg[i] - batDau[batDau.Count - 1] > 0.3f) im = true;
            if (batDau.Count > 0 && rms[i] > dinhMoi[dinhMoi.Count - 1] && tg[i] - batDau[batDau.Count - 1] < 0.4f) dinhMoi[dinhMoi.Count - 1] = rms[i];
        }
        // do dai tieng cuoi: thoi gian tren 10% dinh sau diem bat dau cuoi
        float daiCuoi = 0;
        if (batDau.Count > 0)
            for (int i = 0; i < rms.Count; i++) if (tg[i] >= batDau[batDau.Count - 1] && rms[i] >= dinh * 0.1f) daiCuoi = tg[i] - batDau[batDau.Count - 1];
        float tq = (sl > 0 && sr > 0) ? (float)(slr / System.Math.Sqrt(sl * sr)) : 1f;
        var kc = new List<string>();
        for (int i = 1; i < batDau.Count; i++) kc.Add((batDau[i] - batDau[i - 1]).ToString("F2"));
        Ghi("C. tieng that: phat luc " + string.Join(", ", lucKeu.ConvertAll(x => x.ToString("F2"))) + " s; nghe thay " + batDau.Count
            + " tieng, cach nhau " + string.Join(" / ", kc) + " s; dinh tung tieng " + string.Join(" / ", dinhMoi.ConvertAll(x => x.ToString("F3")))
            + "; tieng cuoi keo dai " + daiCuoi.ToString("F2") + " s; tuong quan hai kenh " + tq.ToString("F3"));
        bool deu = batDau.Count == 5;
        for (int i = 1; i < batDau.Count; i++) deu &= Mathf.Abs(batDau[i] - batDau[i - 1] - 1f) < 0.08f;
        Kiem(deu, "khong nghe thay dung 5 tieng cach nhau 1 giay");
        Kiem(dinhMoi.Count == 5 && dinhMoi[4] > dinhMoi[0] * 1.05f && daiCuoi > 0.6f, "nhip cuoi khong manh / dai hon nhip thuong");
        Kiem(tq < 0.99f, "tieng dem nguoc khong stereo");
        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/tiengdemnguoc.txt", bao.ToString());
        NhacNen.BatTrongPhepThu = false;
        var rac = GameObject.Find("TAM_ThuTiengDemNguoc");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;
        EditorUtility.audioMasterMute = tatTiengGoc;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);
        Debug.Log("[DemNguoc] da tra lai: scene = " + EditorSceneManager.GetActiveScene().path + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
