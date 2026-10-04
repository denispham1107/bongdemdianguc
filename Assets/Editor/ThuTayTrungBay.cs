using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MENU 101 - DO LONG BAN TAY phu thuy (cho tu the trung bay o man chinh, nguoi dung 04/10/2026: "2 ban tay ngua len troi").
/// Khung xuong Meshy KHONG co xuong ngon tay, luoi Read/Write TAT -> khong doc duoc trong so xuong. Cach do: dung nhan vat trong
/// PreviewScene, BakeMesh, xoay xuong Hand 20 do roi BakeMesh lan nua - dinh nao dich gan dung bang "xoay cung theo Hand" la dinh
/// cua ban tay. PCA cac dinh ay trong he toa do Hand: truc dai nhat = huong ngon tay, truc mong nhat = phap tuyen long ban tay;
/// dau cua phap tuyen: ngon tay co cong ve phia LONG ban tay -> 30% dinh xa nhat theo huong ngon tay lech ve phia long.
/// Ket qua: PlayTestShots/tay_trungbay.txt (hang so ghi vao TuTheTrungBay).
/// </summary>
public static class ThuTayTrungBay
{
    [MenuItem("Diablo 2.5D/101 Do long ban tay phu thuy (tu the man chinh)", false, 167)]
    public static void DoLongBanTay()
    {
        var sb = new StringBuilder();
        var ps = EditorSceneManager.NewPreviewScene();
        try
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Sorceress.prefab");
            var nv = (GameObject)PrefabUtility.InstantiatePrefab(pf, ps);
            var smr = nv.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (var ten in new[] { "LeftHand", "RightHand" })
            {
                Transform tay = null;
                foreach (var t in nv.GetComponentsInChildren<Transform>(true)) if (t.name == ten) tay = t;
                if (tay == null || smr == null) { sb.AppendLine(ten + ": THIEU"); continue; }
                var A = new UnityEngine.Mesh(); smr.BakeMesh(A, true);
                var va = A.vertices;
                Quaternion q0 = tay.localRotation;
                Vector3 truc = tay.TransformDirection(new Vector3(0.3f, 0.7f, 0.65f).normalized);
                tay.rotation = Quaternion.AngleAxis(20f, truc) * tay.rotation;
                var B = new UnityEngine.Mesh(); smr.BakeMesh(B, true);
                var vb = B.vertices;
                tay.localRotation = q0;
                                var r = smr.transform; var qr = Quaternion.AngleAxis(20f, truc);
                var diem = new List<Vector3>();
                for (int i = 0; i < va.Length; i++)
                {
                    // BakeMesh(useScale: true): dinh da nhan ti le cua renderer, chi con xoay + dich -> the gioi = vi tri + xoay * dinh
                    Vector3 pa = r.position + r.rotation * va[i], pb = r.position + r.rotation * vb[i];
                    Vector3 cung = tay.position + qr * (pa - tay.position);
                    float mong = (cung - pa).magnitude;
                    if (mong < 0.004f) continue;
                    if ((pb - cung).magnitude < 0.12f * mong) diem.Add(tay.InverseTransformPoint(pa));
                }
                Object.DestroyImmediate(A); Object.DestroyImmediate(B);
                if (diem.Count < 30) { sb.AppendLine(ten + ": chi " + diem.Count + " dinh di theo ban tay"); continue; }
                Vector3 tb = Vector3.zero; foreach (var p in diem) tb += p; tb /= diem.Count;
                var C = new float[3, 3];
                foreach (var p in diem) { var d = p - tb; for (int a = 0; a < 3; a++) for (int b = 0; b < 3; b++) C[a, b] += d[a] * d[b]; }
                Vector3 dai = TrucRieng(C, true), mong2 = TrucRieng(C, false);
                // huong ngon tay: tu goc Hand ra phia trong tam cac dinh
                if (Vector3.Dot(dai, tb) < 0f) dai = -dai;
                // dau phap tuyen: 30% dinh xa nhat theo huong ngon lech ve phia long ban tay
                var ds = new List<Vector3>(diem); ds.Sort((x, y) => Vector3.Dot(y, dai).CompareTo(Vector3.Dot(x, dai)));
                Vector3 dau = Vector3.zero; int nd = Mathf.Max(1, ds.Count * 3 / 10); for (int i = 0; i < nd; i++) dau += ds[i]; dau /= nd;
                float lechDau = Vector3.Dot(dau - tb, mong2);
                if (lechDau < 0f) mong2 = -mong2;
                // be day / be ngang / be dai ban tay (do lech chuan theo ba truc)
                Vector3 ngang = Vector3.Cross(mong2, dai).normalized;
                sb.AppendLine(string.Format("{0}: {1} dinh ban tay; tam {2}; huong ngon (cuc bo Hand) {3}; PHAP TUYEN LONG (cuc bo Hand) {4}; dau ngon lech ve phia long {5:F4} m; do day theo ba truc dai/ngang/mong {6:F3}/{7:F3}/{8:F3} m",
                    ten, diem.Count, V(tb), V(dai), V(mong2), Mathf.Abs(lechDau), DoLech(diem, tb, dai), DoLech(diem, tb, ngang), DoLech(diem, tb, mong2)));
            }
        }
        finally { EditorSceneManager.ClosePreviewScene(ps); }
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText("PlayTestShots/tay_trungbay.txt", sb.ToString());
        Debug.Log("[ThuTayTrungBay]\n" + sb);
    }

    /// <summary>Chup can tung ban tay tu HAI phia cua truc phap tuyen (+n / -n) de nhin xem phia nao la long ban tay.</summary>
    [MenuItem("Diablo 2.5D/101b Chup can hai mat ban tay", false, 168)]
    public static void ChupHaiMat()
    {
        var ps = EditorSceneManager.NewPreviewScene();
        try
        {
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Sorceress.prefab");
            var nv = (GameObject)PrefabUtility.InstantiatePrefab(pf, ps);
            var goDen = new GameObject("Den"); SceneManager.MoveGameObjectToScene(goDen, ps);
            var den = goDen.AddComponent<Light>(); den.type = LightType.Directional; den.intensity = 1.2f; goDen.transform.rotation = Quaternion.Euler(40f, 30f, 0f);
            var goCam = new GameObject("Cam"); SceneManager.MoveGameObjectToScene(goCam, ps);
            var cam = goCam.AddComponent<Camera>(); cam.scene = ps; cam.enabled = false; cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.25f, 0.25f, 0.28f); cam.fieldOfView = 30f; cam.nearClipPlane = 0.01f;
            var rt = new RenderTexture(256, 256, 24);
            var tong = new Texture2D(512, 512, TextureFormat.RGB24, false);
            int o = 0;
            foreach (var ten in new[] { "LeftHand", "RightHand" })
            {
                Transform tay = null; foreach (var t in nv.GetComponentsInChildren<Transform>(true)) if (t.name == ten) tay = t;
                Vector3 nL = ten == "LeftHand" ? new Vector3(-0.720f, -0.144f, -0.679f) : new Vector3(-0.754f, 0.204f, 0.624f);
                Vector3 tam = tay.TransformPoint(new Vector3(0f, 0.128f, -0.007f));
                foreach (float dau in new[] { 1f, -1f })
                {
                    Vector3 n = tay.TransformDirection(nL) * dau;
                    goCam.transform.position = tam + n * 0.55f; goCam.transform.LookAt(tam, tay.TransformDirection(Vector3.up));
                    goDen.transform.rotation = Quaternion.LookRotation(-n + Vector3.down * 0.3f);
                    cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
                    var cu = RenderTexture.active; RenderTexture.active = rt;
                    tong.ReadPixels(new Rect(0, 0, 256, 256), (o % 2) * 256, (1 - o / 2) * 256); RenderTexture.active = cu; o++;
                }
            }
            tong.Apply();
            File.WriteAllBytes("PlayTestShots/tay_hai_mat.png", tong.EncodeToPNG());
            Object.DestroyImmediate(tong); Object.DestroyImmediate(rt);
        }
        finally { EditorSceneManager.ClosePreviewScene(ps); }
    }

    // ================= MENU 101c: DO TU THE THAT TRONG PLAY (scene MainMenu) =================
    static bool daBatDau101c;
    const string Ra101c = "PlayTestShots/tay_trungbay_play.txt";

    [MenuItem("Diablo 2.5D/101c Chay thu tu the tay + hai qua cau o man chinh", false, 169)]
    public static void ChayTuThe()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity") EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        if (File.Exists(Ra101c)) File.Delete(Ra101c);
        daBatDau101c = false;
        EditorApplication.update -= Nhip101c;
        EditorApplication.update += Nhip101c;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip101c()
    {
        if (!EditorApplication.isPlaying || daBatDau101c) return;
        daBatDau101c = true;
        var go = new GameObject("TAM_ThuTayTrungBay");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan101c();
    }

    static System.Collections.IEnumerator KichBan101c()
    {
        var sb = new StringBuilder(); int loi = 0;
        System.Action<bool, string> kiem = (d, l) => { if (!d) { loi++; sb.AppendLine("  LOI: " + l); } };
        yield return new WaitForSeconds(0.5f);
        var menu = Object.FindAnyObjectByType<MainMenuUI>();
        var nv = menu != null ? menu.showcase : null;
        var tt = nv != null ? nv.GetComponent<TuTheTrungBay>() : null;
        sb.AppendLine("TuTheTrungBay gan tu MainMenuUI: " + (tt != null));
        kiem(tt != null, "MainMenuUI khong gan TuTheTrungBay");
        if (tt != null)
        {
            // Dung xoay, quay mat ve may quay nhu luc dung scene (menu 51) de do ben man hinh
            float quay = menu.spinSpeed; menu.spinSpeed = 0f;
            var cam = Camera.main;
            Vector3 veCam = cam.transform.position - nv.position; veCam.y = 0f;
            nv.rotation = Quaternion.LookRotation(veCam.normalized);
            yield return new WaitForSeconds(1.5f);
            var hh = nv.GetComponentInChildren<NguoiChoiHoatHinh>();
            Transform hong = hh.hips, nguc = hh.spine;
            foreach (var tay in new[] { tt.BanTayLua, tt.BanTayBang })
            {
                Vector3 longCB = tay.name.StartsWith("Left") ? TuTheTrungBay.LongTayTrai : TuTheTrungBay.LongTayPhai;
                float ngua = Vector3.Dot(tay.TransformDirection(longCB).normalized, Vector3.up);
                float caoTay = tay.position.y - nv.position.y, caoHong = hong.position.y - nv.position.y, caoNguc = nguc.position.y - nv.position.y;
                float ngang = Mathf.Abs(Vector3.Dot(tay.position - nv.position, nv.right));
                float truoc = Vector3.Dot(tay.position - nv.position, nv.forward);
                sb.AppendLine(string.Format("{0} ({1}): long tay . len = {2:F3}; cao {3:F2} m (hong {4:F2}, xuong spine {5:F2}); ra ngoai {6:F2} m, ra truoc {7:F2} m; man hinh x {8:F2}",
                    tay.name, tay == tt.BanTayLua ? "LUA" : "BANG", ngua, caoTay, caoHong, caoNguc, ngang, truoc, cam.WorldToViewportPoint(tay.position).x));
                kiem(ngua > 0.95f, tay.name + ": long ban tay khong ngua len troi");
                kiem(caoTay > caoHong - 0.05f && caoTay < caoNguc + 0.25f, tay.name + ": ban tay khong ngang bung");
                kiem(truoc > 0.15f, tay.name + ": ban tay khong dua ra truoc");
            }
            var lua = GameObject.Find("CauLuaTrenTay"); var bang = GameObject.Find("CauBangTrenTay");
            kiem(lua != null && bang != null, "thieu qua cau tren tay");
            if (lua != null && bang != null)
            {
                float xLua = cam.WorldToViewportPoint(lua.transform.position).x, xBang = cam.WorldToViewportPoint(bang.transform.position).x;
                float caoLua = lua.transform.position.y - tt.BanTayLua.TransformPoint(TuTheTrungBay.TamLongTay).y;
                float caoBang = bang.transform.position.y - tt.BanTayBang.TransformPoint(TuTheTrungBay.TamLongTay).y;
                int denLua = lua.GetComponentsInChildren<Light>().Length, denBang = bang.GetComponentsInChildren<Light>().Length;
                float tamMax = 0f, sangMax = 0f;
                foreach (var lt in lua.GetComponentsInChildren<Light>()) { tamMax = Mathf.Max(tamMax, lt.range); sangMax = Mathf.Max(sangMax, lt.intensity); }
                foreach (var lt in bang.GetComponentsInChildren<Light>()) { tamMax = Mathf.Max(tamMax, lt.range); sangMax = Mathf.Max(sangMax, lt.intensity); }
                sb.AppendLine(string.Format("Den cua hai qua cau: tam lon nhat {0:F2} m, do sang lon nhat {1:F2} (mong <= {2} m / ~{3})", tamMax, sangMax, TuTheTrungBay.TamDenCau, TuTheTrungBay.DoSangDenCau));
                kiem(tamMax <= TuTheTrungBay.TamDenCau * 1.15f + 0.01f, "den qua cau tren tay qua xa (nhuom ca man chinh)");
                int hatLua = 0, hatBang = 0;
                foreach (var p in lua.GetComponentsInChildren<ParticleSystem>()) hatLua += p.particleCount;
                foreach (var p in bang.GetComponentsInChildren<ParticleSystem>()) hatBang += p.particleCount;
                sb.AppendLine(string.Format("Qua cau LUA: man hinh x {0:F2}, cao tren long tay {1:F2} m, {2} den, {3} hat | Qua cau BANG: man hinh x {4:F2}, cao tren long tay {5:F2} m, {6} den, {7} hat",
                    xLua, caoLua, denLua, hatLua, xBang, caoBang, denBang, hatBang));
                kiem(xLua > xBang, "lua khong o ben PHAI man hinh");
                kiem(caoLua > 0.1f && caoLua < 0.35f && caoBang > 0.1f && caoBang < 0.35f, "qua cau khong lo lung ngay tren long tay");
                kiem(hatLua > 0 && hatBang > 0, "qua cau khong co hat (khong sang)");
                // 04/10/2026: tan lua + lua loi cua qua cau tren tay = NGON LUA THAT (khong con vet keo dai / anh tam giac Tex_flame)
                int ngonThat = 0; string moTa = "";
                foreach (var ten in new[] { "Sparks", "Flames" })
                {
                    var t = lua.transform.Find(ten); var rr = t != null ? t.GetComponent<ParticleSystemRenderer>() : null;
                    var tx = rr != null && rr.sharedMaterial != null ? rr.sharedMaterial.mainTexture : null;
                    moTa += ten + ": " + (rr != null ? rr.renderMode.ToString() : "-") + " anh " + (tx != null ? tx.name : "-") + "; ";
                    if (rr != null && rr.renderMode == ParticleSystemRenderMode.Billboard && tx != null && tx.name == "NgonLuaThat") ngonThat++;
                }
                sb.AppendLine("Lua tren tay: " + moTa);
                kiem(ngonThat == 2, "tan lua / lua loi tren tay chua phai ngon lua that");
            }
            string anh = "PlayTestShots/tay_trungbay_man_chinh.png";
            if (File.Exists(anh)) File.Delete(anh);
            ScreenCapture.CaptureScreenshot(anh);
            for (int i = 0; i < 90 && !File.Exists(anh); i++) yield return new WaitForEndOfFrame();
            // anh CAN (may quay tam, khong co lop giao dien OnGUI): truoc mat nhan vat 3 m, ngang nguc
            {
                var goC = new GameObject("TAM_CamCanTay"); var cc = goC.AddComponent<Camera>(); cc.CopyFrom(cam); cc.enabled = false;
                Vector3 nhin = nv.position + Vector3.up * 1.1f;
                goC.transform.position = nhin + (cam.transform.position - nhin).normalized * 3f;
                goC.transform.LookAt(nhin);
                var rt = new RenderTexture(900, 600, 24); cc.targetTexture = rt; cc.Render(); cc.targetTexture = null;
                var cu = RenderTexture.active; RenderTexture.active = rt;
                var tx = new Texture2D(900, 600, TextureFormat.RGB24, false); tx.ReadPixels(new Rect(0, 0, 900, 600), 0, 0); tx.Apply();
                RenderTexture.active = cu;
                File.WriteAllBytes("PlayTestShots/tay_trungbay_can.png", tx.EncodeToPNG());
                Object.Destroy(tx); Object.Destroy(rt); Object.Destroy(goC);
            }
            menu.spinSpeed = quay;
        }
        sb.AppendLine("so loi ghi nhan = " + loi);
        File.WriteAllText(Ra101c, sb.ToString());
        var rac = GameObject.Find("TAM_ThuTayTrungBay"); if (rac != null) Object.Destroy(rac);
        EditorApplication.update -= Nhip101c;
        EditorApplication.isPlaying = false;
    }

    static string V(Vector3 v) { return string.Format("({0:F3}, {1:F3}, {2:F3})", v.x, v.y, v.z); }

    static float DoLech(List<Vector3> ds, Vector3 tb, Vector3 truc)
    {
        double s = 0; foreach (var p in ds) { float d = Vector3.Dot(p - tb, truc); s += d * d; }
        return Mathf.Sqrt((float)(s / ds.Count));
    }

    /// <summary>Vec-to rieng lon nhat (lon = true) hoac nho nhat cua ma tran hiep phuong sai 3x3 (lap luy thua).</summary>
    static Vector3 TrucRieng(float[,] C, bool lon)
    {
        float tr = C[0, 0] + C[1, 1] + C[2, 2];
        var M = new float[3, 3];
        for (int a = 0; a < 3; a++) for (int b = 0; b < 3; b++) M[a, b] = lon ? C[a, b] : ((a == b ? tr : 0f) - C[a, b]);
        Vector3 v = new Vector3(0.577f, 0.577f, 0.577f);
        for (int k = 0; k < 200; k++)
        {
            var w = new Vector3(M[0, 0] * v.x + M[0, 1] * v.y + M[0, 2] * v.z, M[1, 0] * v.x + M[1, 1] * v.y + M[1, 2] * v.z, M[2, 0] * v.x + M[2, 1] * v.y + M[2, 2] * v.z);
            if (w.sqrMagnitude < 1e-20f) break;
            v = w.normalized;
        }
        return v;
    }
}
