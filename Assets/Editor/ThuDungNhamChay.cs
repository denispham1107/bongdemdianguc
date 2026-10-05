using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MENU 111 / 111b - DUNG NHAM CHAY THEO CAC DUONG GAN (05/10/2026, nguoi dung: "cho thay ro hieu ung cac dong dung nham dang chuyen
/// dong va chay"; chon "chay theo cac duong gan trong hinh", toc do chon qua anh dong 3 muc).
///
/// 111 (NGOAI Play, PreviewScene): mot tam dung nham THAT (vat lieu DungNham.mat) + may quay TRUC GIAO nhin thang xuong vung 12 x 12 m
/// trong vuc; dat dong ho shader `_DN_ThoiGian` bang tay, chup 12 khung cach 0,15 s / toc (hinh cu 0,1 s). Do DOC LAP voi anh huong gan: lay trung binh theo
/// thoi gian (phan dung yen, de tim gan); moi o 24 px nam tren GAN (sang) tim dich chuyen (dx, dy) khop nhat giua hai khung
/// lien tiep (HIEU hai khung lien tiep - phan dung yen tu triet tieu; tuong quan cheo chuan hoa >= 0,3 giua hai hieu lien tiep, cong qua 10 cap, noi suy duoi diem anh) va huong gan TINH TU ANH CHUP (tensor cau truc cua anh trung binh). Kiem: chuyen dong >= 5x hinh cu, dot sang
/// xuoi dong chay vong quanh ban do (>= 60%, hon doi chung 20 diem), toc do ti le voi _TocChay, doc gan (|cos| > 0,8) >= 60%; doi chung `_DoChay` = 0 (hinh cu).
///
/// 111b (Play Act2): chup khung PNG o 4 muc (cu / 0,8 / 1,5 / 2,5 m/s) tu hai goc - MEP vuc (nhu anh nguoi dung: tren mep da nhin xuoi xuong) va
/// GAN tren vuc - dong ho shader dat tay, tron so nguyen chu ky (lap lien mach). Ghep GIF bang Python.
/// </summary>
public static class ThuDungNhamChay
{
    const string Ra = "PlayTestShots/dungnham_chay.txt";
    const int N = 384;               // diem anh moi canh
    const float CoVung = 12f;         // met moi canh (32 diem anh / m)
    const int SoKhung = 12;
    const float Dt = 0.1f;
    const int O = 24, Buoc = 16, TamDich = 10;

    [MenuItem("Diablo 2.5D/111 Dung nham chay theo gan (do dich chuyen)", false, 179)]
    public static void Do()
    {
        if (EditorApplication.isPlaying) return;
        Directory.CreateDirectory("PlayTestShots");
        var sb = new StringBuilder(); int loi = 0;
        System.Action<bool, string> kiem = (d, l) => { if (!d) { loi++; sb.AppendLine("  LOI: " + l); } };
        var matGoc = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DiaNguc/DungNham.mat");
        sb.AppendLine("Vat lieu: " + matGoc.shader.name + ", huong gan " + (matGoc.GetTexture("_HuongGan") != null) + ", toc " + matGoc.GetFloat("_TocChay") + " m/s, quang troi moi chu ky " + matGoc.GetFloat("_QuangChuKy") + " m");
        kiem(matGoc.GetTexture("_HuongGan") != null, "vat lieu thieu anh huong gan");

        var vung = new[] { new Vector2(0f, -90f), new Vector2(90f, 0f) };
        foreach (var tam in vung)
        {
            var D0 = new Vector2(-tam.y, tam.x).normalized;
            sb.AppendLine(string.Format("--- Vung tam ({0}; {1}), dong chay vong quanh ({2:F2}; {3:F2}) ---", tam.x, tam.y, D0.x, D0.y));
            var cu = DoMotMuc(matGoc, tam, 1.5f, 0f);
            var m08 = DoMotMuc(matGoc, tam, 0.8f, 1f);
            var m15 = DoMotMuc(matGoc, tam, 1.5f, 1f);
            var m25 = DoMotMuc(matGoc, tam, 2.5f, 1f);
            foreach (var kq in new[] { cu, m08, m15, m25 }) sb.AppendLine("  " + kq.MoTa(D0));
            kiem(m15.Chuyen >= cu.Chuyen * 5f, "dung nham moi khong chuyen dong ro hon hinh cu (phan du / trung binh)");
            // CHU Y: "doc gan" bi HIEU UNG KHAU DO: doc mot gan thang anh gan nhu khong doi nen dinh tuong quan nghieng ve huong gan ca
            // khi khong co gi chay (doi chung 61-80%, ngau nhien 41%) -> chi la kiem tinh hop ly, khong phai phep phan biet.
            // Phan biet that: nang luong chuyen dong (tren), XUOI DONG (doi chung 26-43%) va toc do tang theo _TocChay.
            float xuoiCu = cu.TiLeXuoi(D0);
            foreach (var kq in new[] { m08, m15, m25 })
            {
                kiem(kq.TiLeCoDich >= 0.5f, "muc " + kq.toc + ": duoi mot nua o tren gan co dich chuyen");
                kiem(kq.TiLeDocGan >= 0.6f, "muc " + kq.toc + ": dot sang khong chay DOC THEO gan");
                kiem(kq.TiLeXuoi(D0) >= 0.6f && kq.TiLeXuoi(D0) >= xuoiCu + 0.2f, "muc " + kq.toc + ": khong xuoi dong chay vong quanh");
                float tv = kq.TocTrungVi;
                kiem(tv >= 0.35f * kq.toc && tv <= 1.25f * kq.toc, "muc " + kq.toc + ": toc do do duoc " + tv.ToString("F2") + " lech xa _TocChay");
            }
            kiem(m25.TocTrungVi >= 2f * m08.TocTrungVi, "toc 2,5 khong nhanh hon ro so voi 0,8");
        }
        sb.AppendLine(loi == 0 ? "KET QUA: 0 loi" : "KET QUA: " + loi + " loi");
        File.WriteAllText(Ra, sb.ToString());
        Debug.Log(sb.ToString());
    }

    class KetQua
    {
        public float toc, doChay, Chuyen;
        public int soO;
        public List<Vector2> dich = new List<Vector2>();   // m/s
        public List<float> cosGan = new List<float>();
        public float TiLeCoDich { get { return soO > 0 ? dich.Count / (float)soO : 0f; } }
        public float TiLeDocGan { get { int n = 0; foreach (var c in cosGan) if (c > 0.8f) n++; return cosGan.Count > 0 ? n / (float)cosGan.Count : 0f; } }
        public float TiLeXuoi(Vector2 D) { int n = 0; foreach (var v in dich) if (Vector2.Dot(v, D) > 0f) n++; return dich.Count > 0 ? n / (float)dich.Count : 0f; }
        public float TocTrungVi { get { if (dich.Count == 0) return 0f; var l = new List<float>(); foreach (var v in dich) l.Add(v.magnitude); l.Sort(); return l[l.Count / 2]; } }
        public string MoTa(Vector2 D)
        {
            return string.Format("{0} toc {1}: chuyen dong (RMS phan du / TB) {2:F3}; o tren gan {3}, co dich {4:P0}, doc gan {5:P0}, xuoi dong {6:P0}, toc trung vi {7:F2} m/s",
                doChay > 0f ? "MOI" : "CU (doi chung)", toc, Chuyen, soO, TiLeCoDich, TiLeDocGan, TiLeXuoi(D), TocTrungVi);
        }
    }

    static KetQua DoMotMuc(Material matGoc, Vector2 tam, float toc, float doChay)
    {
        var kq = new KetQua { toc = toc, doChay = doChay };
        // khoang cach hai khung theo toc: moi muc dot sang dich ~4,8 diem anh / khung (muc cham 0,8 m/s o 0,1 s chi ~2 diem anh - nhieu keo so do len)
        float dt = doChay > 0f ? 0.15f / toc : Dt;
        var ps = EditorSceneManager.NewPreviewScene();
        var mat = new Material(matGoc);
        mat.SetFloat("_TocChay", toc); mat.SetFloat("_DoChay", doChay); mat.SetFloat("_SuongMu", 0f);
        var rt = new RenderTexture(N, N, 24, RenderTextureFormat.ARGB32);
        var tx = new Texture2D(N, N, TextureFormat.RGB24, false);
        var khung = new float[SoKhung][];
        try
        {
            var go = new GameObject("TAM_DungNham");
            SceneManager.MoveGameObjectToScene(go, ps);
            var me = new UnityEngine.Mesh();
            float h = CoVung * 0.6f, y = DiaNguc.MucDungNham;
            me.vertices = new[] { new Vector3(tam.x - h, y, tam.y - h), new Vector3(tam.x - h, y, tam.y + h), new Vector3(tam.x + h, y, tam.y + h), new Vector3(tam.x + h, y, tam.y - h) };
            me.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            me.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = me;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            go.layer = 31;
            var goCam = new GameObject("TAM_CamDungNham");
            SceneManager.MoveGameObjectToScene(goCam, ps);
            var cam = goCam.AddComponent<Camera>();
            cam.enabled = false; cam.scene = ps; cam.orthographic = true; cam.orthographicSize = CoVung * 0.5f;
            cam.cullingMask = 1 << 31; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.transform.position = new Vector3(tam.x, y + 20f, tam.y); cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.nearClipPlane = 1f; cam.farClipPlane = 40f; cam.allowHDR = false; cam.allowMSAA = false;
            cam.targetTexture = rt;
            for (int k = 0; k < SoKhung; k++)
            {
                Shader.SetGlobalFloat("_DN_ThoiGian", 50f + k * dt);
                cam.Render();
                RenderTexture.active = rt; tx.ReadPixels(new Rect(0, 0, N, N), 0, 0); tx.Apply(false); RenderTexture.active = null;
                var px = tx.GetPixels32(); var l = new float[N * N];
                for (int i = 0; i < l.Length; i++) l[i] = (px[i].r * 0.3f + px[i].g * 0.5f + px[i].b * 0.2f) / 255f;
                khung[k] = l;
            }
            cam.targetTexture = null;
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(ps);
            Object.DestroyImmediate(mat); Object.DestroyImmediate(rt); Object.DestroyImmediate(tx);
            Shader.SetGlobalFloat("_DN_ThoiGian", 0f);
        }

        // trung binh theo thoi gian (phan dung yen: anh vo + gan) va HIEU HAI KHUNG LIEN TIEP: phan dung yen tu triet tieu, con
        // lai dau vet cua thu dang chuyen dong; hieu khung k va k+1 la cung mot mau dich di v*dt -> tuong quan cheo tim v.
        // (Ban dau tru xu huong tuyen tinh: dot sang troi chua het mot buoc song trong 1 s nen gan nhu tuyen tinh -> phep tru xoa
        // mat chinh chuyen dong can do, toc do ra lung tung.)
        var tb = new float[N * N];
        for (int k = 0; k < SoKhung; k++) for (int i = 0; i < tb.Length; i++) tb[i] += khung[k][i] / SoKhung;
        int soHieu = SoKhung - 1;
        var du = new float[soHieu][];
        for (int k = 0; k < soHieu; k++) { du[k] = new float[N * N]; for (int i = 0; i < tb.Length; i++) du[k][i] = khung[k + 1][i] - khung[k][i]; }
        // nguong "gan": 25% diem sang nhat cua anh trung binh
        var sx = (float[])tb.Clone(); System.Array.Sort(sx); float nguong = sx[(int)(sx.Length * 0.75f)];
        double tongDu = 0, tongTb = 0; int nGan = 0;
        for (int i = 0; i < tb.Length; i++) if (tb[i] >= nguong) { nGan++; tongTb += tb[i]; for (int k = 0; k < soHieu; k++) tongDu += du[k][i] * du[k][i]; }
        // do chuyen dong = RMS hieu hai khung (moi dt) / do sang TB, tren gan
        kq.Chuyen = nGan > 0 ? (float)(System.Math.Sqrt(tongDu / (nGan * soHieu)) / (tongTb / nGan)) : 0f;

        float pxMoiMet = N / CoVung;
        int ban = O / 2, W = 2 * TamDich + 1;
        var diem = new double[W * W];
        for (int cy = TamDich + ban + 1; cy < N - TamDich - ban - 1; cy += Buoc)
            for (int cx = TamDich + ban + 1; cx < N - TamDich - ban - 1; cx += Buoc)
            {
                if (tb[cy * N + cx] < nguong) continue;
                // huong gan tu anh trung binh (tensor cau truc trong o)
                double jxx = 0, jyy = 0, jxy = 0;
                for (int y = cy - ban; y < cy + ban; y++)
                    for (int x = cx - ban; x < cx + ban; x++)
                    {
                        float gx = (tb[y * N + x + 1] - tb[y * N + x - 1]) * 0.5f, gy = (tb[(y + 1) * N + x] - tb[(y - 1) * N + x]) * 0.5f;
                        jxx += gx * gx; jyy += gy * gy; jxy += gx * gy;
                    }
                double dd = jxx - jyy, r = System.Math.Sqrt(dd * dd + 4 * jxy * jxy);
                double ketHop = r / (jxx + jyy + 1e-12);
                if (ketHop < 0.4) continue;   // giao diem / vung dac: khong co huong gan ro
                kq.soO++;
                double gocGrad = 0.5 * System.Math.Atan2(2 * jxy, dd);
                var tiep = new Vector2(-(float)System.Math.Sin(gocGrad), (float)System.Math.Cos(gocGrad));
                // tuong quan cheo hieu khung, cong qua cac cap hieu lien tiep
                double tot = double.MinValue; int bx = 0, by = 0;
                for (int sy = -TamDich; sy <= TamDich; sy++)
                    for (int sxx = -TamDich; sxx <= TamDich; sxx++)
                    {
                        double s = 0;
                        for (int k = 0; k + 1 < soHieu; k++)
                        {
                            var a = du[k]; var b = du[k + 1];
                            for (int y = cy - ban; y < cy + ban; y++)
                            {
                                int ha = y * N, hb = (y + sy) * N + sxx;
                                for (int x = cx - ban; x < cx + ban; x++) s += a[ha + x] * b[hb + x];
                            }
                        }
                        diem[(sy + TamDich) * W + sxx + TamDich] = s;
                        if (s > tot) { tot = s; bx = sxx; by = sy; }
                    }
                // tuong quan CHUAN HOA cua dinh
                double ea = 0, eb = 0;
                for (int k = 0; k + 1 < soHieu; k++)
                    for (int y = cy - ban; y < cy + ban; y++)
                        for (int x = cx - ban; x < cx + ban; x++)
                        {
                            float va = du[k][y * N + x], vb = du[k + 1][(y + by) * N + x + bx];
                            ea += va * va; eb += vb * vb;
                        }
                double chuan = tot / System.Math.Sqrt(ea * eb + 1e-20);
                if (chuan < 0.3) continue;
                // noi suy duoi diem anh (parabol theo tung truc)
                float fx = bx, fy = by;
                if (bx > -TamDich && bx < TamDich)
                {
                    double l = diem[(by + TamDich) * W + bx + TamDich - 1], c0 = tot, rr = diem[(by + TamDich) * W + bx + TamDich + 1];
                    double mau = l - 2 * c0 + rr; if (mau < 0) fx += (float)(0.5 * (l - rr) / mau);
                }
                if (by > -TamDich && by < TamDich)
                {
                    double l = diem[(by + TamDich - 1) * W + bx + TamDich], c0 = tot, rr = diem[(by + TamDich + 1) * W + bx + TamDich];
                    double mau = l - 2 * c0 + rr; if (mau < 0) fy += (float)(0.5 * (l - rr) / mau);
                }
                var v = new Vector2(fx, fy);
                if (v.magnitude < 0.5f) continue;   // dung yen
                // anh: cot = +x the gioi, hang = +z the gioi (may quay nhin xuong, len = +z)
                kq.dich.Add(v / pxMoiMet / dt);
                kq.cosGan.Add(Mathf.Abs(Vector2.Dot(v.normalized, tiep)));
            }
        return kq;
    }

    // ================= 111b: anh dong =================
    static bool daBatDau; static bool truocBat; static EnterPlayModeOptions truocOpt;
    const string Thu = "PlayTestShots/dungnham_chay/";
    const int Rong = 480, Cao = 360, SoKhungGif = 30;
    const float CachKhung = 0.08f;
    public static readonly float[] MucToc = { 0f, 0.8f, 1.5f, 2.5f };   // 0 = hinh cu (doi chung)

    [MenuItem("Diablo 2.5D/111b Dung nham chay - chup anh dong 4 muc", false, 180)]
    public static void ChupGif()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        if (Directory.Exists(Thu)) Directory.Delete(Thu, true);
        Directory.CreateDirectory(Thu);
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        daBatDau = false;
        EditorApplication.update -= Nhip; EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_DungNhamGif");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBanGif();
    }

    static float DatY(Vector3 p) { var t = Terrain.activeTerrain; return t.SampleHeight(p) + t.transform.position.y; }

    static IEnumerator KichBanGif()
    {
        var sb = new StringBuilder();
        PlayerController toi = null; float han = Time.time + 25f;
        while (toi == null && Time.time < han) { toi = Object.FindAnyObjectByType<PlayerController>(); yield return null; }
        GameObject bien = null; han = Time.time + 10f;
        while (bien == null && Time.time < han) { bien = GameObject.Find("BienDungNham"); yield return null; }
        if (toi != null && bien != null)
        {
            var gd = Object.FindAnyObjectByType<GameDirector>(); if (gd != null) gd.enabled = false;
            foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)) Object.Destroy(q.gameObject);
            var cc = toi.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
            toi.transform.position = new Vector3(22.885f, DatY(new Vector3(22.885f, 0, -61f)) + 0.1f, -61f);
            toi.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            if (cc != null) cc.enabled = true;
            var cam = Camera.main;
            var rig = cam.GetComponent<CameraRig>(); if (rig != null) rig.enabled = false;
            var mat = bien.GetComponent<MeshRenderer>().material;   // ban sao trong Play, khong sua .mat
            yield return new WaitForSeconds(1f);
            Vector3 p = toi.transform.position;
            var goc = new[] {
                new { ten = "mep", pos = new Vector3(12f, 6f, -68f), nhin = new Vector3(22f, -26f, -76f) },
                new { ten = "gan", pos = new Vector3(22.885f, -8f, -66f), nhin = new Vector3(22.885f, -26f, -80f) },
            };
            var rt = RenderTexture.GetTemporary(Rong, Cao, 24, RenderTextureFormat.ARGB32);
            var tx = new Texture2D(Rong, Cao, TextureFormat.RGB24, false);
            DiaNguc.GiuDongHoDungNham = true;
            foreach (float toc in MucToc)
            {
                mat.SetFloat("_DoChay", toc > 0f ? 1f : 0f);
                mat.SetFloat("_TocChay", toc > 0f ? toc : 1.5f);
                // lop chay lap lai sau moi chu ky (= quang troi / toc) -> quay tron so nguyen chu ky ~2,5 s: anh dong lap LIEN MACH
                float buoc = CachKhung;
                if (toc > 0f)
                {
                    float chuKy = mat.GetFloat("_QuangChuKy") / toc;
                    buoc = Mathf.Max(1, Mathf.RoundToInt(2.5f / chuKy)) * chuKy / SoKhungGif;
                }
                sb.AppendLine("buoc " + toc.ToString("F1") + " " + buoc.ToString("F4"));
                for (int i = 0; i < SoKhungGif; i++)
                {
                    float t = 200f + i * buoc;
                    // hinh cu (_DoChay 0) cung doc dong ho nay: lop vo van troi cham nhu truoc
                    Shader.SetGlobalFloat("_DN_ThoiGian", t);
                    yield return new WaitForEndOfFrame();
                    foreach (var g in goc)
                    {
                        cam.transform.position = g.pos; cam.transform.LookAt(g.nhin);
                        cam.targetTexture = rt; cam.Render(); cam.targetTexture = null;
                        RenderTexture.active = rt; tx.ReadPixels(new Rect(0, 0, Rong, Cao), 0, 0); tx.Apply(false); RenderTexture.active = null;
                        File.WriteAllBytes(Thu + "m" + toc.ToString("F1") + "_" + g.ten + "_" + i.ToString("D2") + ".png", tx.EncodeToPNG());
                    }
                }
                sb.AppendLine("Muc " + (toc > 0f ? toc + " m/s" : "cu") + ": " + SoKhungGif + " khung x 2 goc");
            }
            DiaNguc.GiuDongHoDungNham = false;
            RenderTexture.ReleaseTemporary(rt); Object.Destroy(tx);
            if (rig != null) rig.enabled = true;
        }
        else sb.AppendLine("LOI: khong thay nhan vat / BienDungNham");
        File.WriteAllText(Thu + "xong.txt", sb.ToString());
        EditorApplication.update -= Nhip;
        EditorApplication.ExitPlaymode();
        EditorApplication.delayCall += () =>
        {
            EditorSettings.enterPlayModeOptionsEnabled = truocBat; EditorSettings.enterPlayModeOptions = truocOpt;
        };
    }
}
