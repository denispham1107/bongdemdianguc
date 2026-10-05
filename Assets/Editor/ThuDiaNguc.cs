using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// MENU 110 - DIA NGUC NGOAI BAN DO (nguoi dung 05/10/2026: roi xuong vuc thi mat 20% mau toi da / giay cho den chet, quai cung vay).
/// Vao Play THAT o Act2 (qua MainMenu nhu menu 40), tat GameDirector, do bang dong ho game:
///   B (doi chung) nhan vat dung TRONG ban do 3 s: khong mat mau.
///   C Bo xuong (giu nguyen 45% do don) duoc nguoi choi "danh" (GhiKeDanh) roi dat ra ngoai mep: roi xuong day, chet; ke ha = nguoi choi.
///   D ban sao quai (mauDoMayKhacQuyet) roi xuong: KHONG bi tru mau tren may nay.
///   A nhan vat dat ra ngoai mep ban do (sau cung - chet la het tran): roi, cham day dung nham, toc mat mau, chet sau bao lau.
/// Ket qua: PlayTestShots/dia_nguc.txt.
/// </summary>
public static class ThuDiaNguc
{
    const string Ra = "PlayTestShots/dia_nguc.txt";
    static bool daBatDau; static string canhCu; static bool truocBat; static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/110 Dia nguc ngoai ban do (roi xuong mat mau)", false, 178)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) return;
        Directory.CreateDirectory("PlayTestShots");
        if (File.Exists(Ra)) File.Delete(Ra);
        canhCu = EditorSceneManager.GetActiveScene().path;
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        daBatDau = false;
        EditorApplication.update -= Nhip; EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_DiaNguc");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void DatCho(Component c, Vector3 p)
    {
        var cc = c.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        c.transform.position = p;
        if (cc != null) cc.enabled = true;
    }

    static float DatY(Vector3 p) { var t = Terrain.activeTerrain; return t.SampleHeight(p) + t.transform.position.y; }

    static IEnumerator KichBan()
    {
        var sb = new StringBuilder(); int loi = 0;
        System.Action<bool, string> kiem = (d, l) => { if (!d) { loi++; sb.AppendLine("  LOI: " + l); } };
        UnityEngine.SceneManagement.SceneManager.LoadScene("Act2");
        float han = Time.time + 25f;
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Act2" && Time.time < han) yield return null;
        PlayerController toi = null;
        han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        if (toi == null) { sb.AppendLine("LOI: khong thay nhan vat"); loi++; }
        else
        {
            var gd = Object.FindAnyObjectByType<GameDirector>(); if (gd != null) gd.enabled = false;
            foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(q.gameObject);
            var mau = toi.GetComponent<Damageable>();
            yield return new WaitForSeconds(1f);
            sb.AppendLine("DiaNguc trong canh: " + (Object.FindAnyObjectByType<DiaNguc>() != null) + string.Format("; muc dung nham {0} m, nguong roi {1} m, {2:P0} mau toi da / giay", DiaNguc.MucDungNham, DiaNguc.NguongRoi, DiaNguc.TiLeMatMauMoiGiay));
            kiem(Object.FindAnyObjectByType<DiaNguc>() != null, "khong co DiaNguc trong tran");

            // ---- B. doi chung: dung trong ban do ----
            Vector3 trong = new Vector3(60f, 0f, 0f); trong.y = DatY(trong) + 0.2f;
            DatCho(toi, trong);
            yield return new WaitForSeconds(0.5f);
            float m0 = mau.health;
            yield return new WaitForSeconds(3f);
            sb.AppendLine(string.Format("B. doi chung dung TRONG ban do (x 60) 3 s: mau {0:F0} -> {1:F0}, y {2:F2}", m0, mau.health, toi.transform.position.y));
            kiem(Mathf.Abs(mau.health - m0) < 0.5f, "dung trong ban do ma mat mau");

            // ---- C. Bo xuong do nguoi choi danh roi day xuong vuc ----
            {
                Vector3 p = new Vector3(0f, 0f, 69.5f); p.y = DatY(new Vector3(0f, 0f, 66f)) + 0.3f;
                var g = EnemyFactory.Spawn(MonsterType.Skeleton, p, null, toi.transform);
                g.name = "TAM_BoXuongRoi";
                var ai = g.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
                var d = g.GetComponent<Damageable>();
                float doDon = d.tiLeDoDon;
                d.GhiKeDanh(mau);
                Damageable keLucChet = null; float lucChet = -1f;
                d.onDeath += x => { keLucChet = x.keDanhCuoi; lucChet = Time.time; };
                // quai khong co trong luc khi AI tat? tu roi bang CharacterController
                var cc = g.GetComponent<CharacterController>();
                float t0 = Time.time, lucQuaNguong = -1f, yThap = 99f;
                while (Time.time - t0 < 12f && !d.IsDead)
                {
                    if (cc != null && cc.enabled) cc.Move(Vector3.down * 22f * Time.deltaTime * Mathf.Min(1f, Time.time - t0));
                    if (lucQuaNguong < 0f && d.transform.position.y < DiaNguc.NguongRoi) lucQuaNguong = Time.time;
                    yThap = Mathf.Min(yThap, d.transform.position.y);
                    yield return null;
                }
                float song = lucQuaNguong > 0f && lucChet > 0f ? lucChet - lucQuaNguong : -1f;
                sb.AppendLine(string.Format("C. Bo xuong (do don {0:P0}) dat ngoai mep z 69,5: qua nguong sau {1:F2} s, thap nhat y {2:F2}, chet {3}, song {4:F2} s sau khi qua nguong, ke ha {5}",
                    doDon, lucQuaNguong > 0 ? lucQuaNguong - t0 : -1f, yThap, d.IsDead, song, keLucChet == mau ? "NGUOI CHOI" : (keLucChet != null ? keLucChet.name : "null")));
                kiem(d.IsDead, "Bo xuong roi xuong vuc ma khong chet");
                kiem(song > 4.4f && song < 5.6f, "Bo xuong khong chet sau ~5 s (20%/giay)");
                kiem(yThap > DiaNguc.MucDungNham - 2f, "Bo xuong roi xuyen day dung nham");
                kiem(keLucChet == mau, "chet o vuc ma khong tinh cho nguoi danh no truoc do");
            }

            // ---- D. ban sao quai (may khac quyet mau) ----
            {
                Vector3 p = new Vector3(0f, -12f, 70f);
                var g = EnemyFactory.Spawn(MonsterType.Skeleton, p, null, toi.transform);
                g.name = "TAM_BanSaoRoi";
                var ai = g.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
                var d = g.GetComponent<Damageable>();
                d.mauDoMayKhacQuyet = true;
                float h0 = d.health;
                yield return new WaitForSeconds(1.5f);
                sb.AppendLine(string.Format("D. ban sao quai o y {0:F1}: mau {1:F0} -> {2:F0}", d.transform.position.y, h0, d.health));
                kiem(Mathf.Abs(d.health - h0) < 0.5f, "ban sao (may khac quyet mau) bi tru mau tren may nay");
                Object.Destroy(g);
            }

            // ---- A. nhan vat ra ngoai mep ban do ----
            {
                Vector3 p = new Vector3(68.6f, 0f, 0f); p.y = DatY(new Vector3(66f, 0f, 0f)) + 0.3f;
                DatCho(toi, p);
                float t0 = Time.time, lucQuaNguong = -1f, lucCham = -1f, yThap = 99f;
                float maxMau = mau.maxHealth;
                var mauTheoGiay = new List<string>();
                float mocGhi = 0f;
                while (Time.time - t0 < 12f && !mau.IsDead)
                {
                    float y = toi.transform.position.y;
                    if (lucQuaNguong < 0f && y < DiaNguc.NguongRoi) { lucQuaNguong = Time.time; mocGhi = Time.time; }
                    if (lucCham < 0f && y < DiaNguc.MucDungNham + 0.1f) lucCham = Time.time;
                    yThap = Mathf.Min(yThap, y);
                    if (lucQuaNguong > 0f && Time.time >= mocGhi) { mauTheoGiay.Add(string.Format("{0:F1}s:{1:F0}", Time.time - lucQuaNguong, mau.health)); mocGhi += 1f; }
                    yield return null;
                }
                float song = lucQuaNguong > 0f ? Time.time - lucQuaNguong : -1f;
                sb.AppendLine(string.Format("A. nhan vat (mau {0:F0}) dat ngoai mep x 68,6: qua nguong sau {1:F2} s, cham day dung nham sau {2:F2} s, thap nhat y {3:F2}; chet {4} sau {5:F2} s tu luc qua nguong; mau: {6}",
                    maxMau, lucQuaNguong > 0 ? lucQuaNguong - t0 : -1f, lucCham > 0 ? lucCham - t0 : -1f, yThap, mau.IsDead, song, string.Join(" ", mauTheoGiay.ToArray())));
                kiem(lucQuaNguong > 0f && lucQuaNguong - t0 < 2f, "nhan vat ra ngoai mep ma khong roi xuong");
                kiem(mau.IsDead && song > 4.4f && song < 5.6f, "nhan vat khong chet sau ~5 s (20%/giay)");
                kiem(yThap > DiaNguc.MucDungNham - 2f, "nhan vat roi xuyen day dung nham");
                sb.AppendLine("So lan roi DiaNguc dem: " + DiaNguc.SoLanRoi);
            }
        }
        sb.AppendLine("so loi ghi nhan = " + loi);
        File.WriteAllText(Ra, sb.ToString());
        foreach (var go in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (go != null && go.parent == null && go.name.StartsWith("TAM_")) Object.Destroy(go.gameObject);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }
}
