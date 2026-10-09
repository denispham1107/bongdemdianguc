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
///   ⚠️ 01/10/2026 GIO LOC = LOC XOAY THU NHO x0,318 (nguoi dung) - muc C viet lai, cac dong C phia tren la LICH SU:
///   C1 tia set Loc xoay THAT (dung yen) do lam chuan; C2 tung thanh phan (luoi, mau, quay, truot anh, hat, den) khop Loc xoay that,
///      cao 5 m, moi he hat cuc bo; C3 phan bo bui (khu ti le) khop tung lop, tong hat x1 (doi chung Gio loc cu ~x0,5); C4 anh
///      chup theo chieu cao moi con tren troi: IoU mat na + do sang (doi chung Gio loc cu); C5 Gio loc bay: quay / truot / bui bay
///      len cuon cung chieu, vong phun bui = Loc xoay that, tia set moi nhip 2 tia, hinh hoc trong khoang tia Loc xoay that, be day
///      x ti le, bam theo loc (doi chung), den tat khi tan. D toc do 9,5 / song 4,5.
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
        DoDaiXoanNgoaiPlay();
        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");
        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    // ---- 03/10/2026: THAN DAI GIO XOAN (Blender MCP GioXoan.fbx) - do NGOAI Play (luoi FBX tat Read/Write: trong Play vertices rong) ----
    static readonly StringBuilder baoXoan = new StringBuilder();
    static int soDaiXoan, soDaiKinVong, soDaiXoanDung, soDaiVLen, soDaiMoHaiDau;
    static float phuVongDaiMax, phuVongVoCu = -1f, caoXoanMax;

    /// <summary>Phu vong lon nhat: trong moi lat cao <paramref name="buoc"/> m, ti le 36 o goc 10 do co dinh -&gt; lay lat nhieu nhat.
    /// Vo pheu kin = 100% (nhin ro hinh tron); dai xoan ho chi chiem mot cung.</summary>
    static float PhuVongToiDa(Vector3[] v, List<int> ds, float buoc)
    {
        float yMin = 99f, yMax = -99f;
        foreach (var i in ds) { yMin = Mathf.Min(yMin, v[i].y); yMax = Mathf.Max(yMax, v[i].y); }
        float tot = 0f;
        for (float y = yMin; y < yMax; y += buoc)
        {
            var o = new bool[36]; int n = 0;
            foreach (var i in ds)
                if (v[i].y >= y && v[i].y < y + buoc)
                {
                    int b = Mathf.Clamp((int)(((Mathf.Atan2(v[i].z, v[i].x) * Mathf.Rad2Deg) + 360f) % 360f / 10f), 0, 35);
                    if (!o[b]) { o[b] = true; n++; }
                }
            tot = Mathf.Max(tot, n / 36f);
        }
        return tot;
    }

    static void DoDaiXoanNgoaiPlay()
    {
        baoXoan.Length = 0; soDaiXoan = 0; soDaiKinVong = 0; soDaiXoanDung = 0; soDaiVLen = 0; soDaiMoHaiDau = 0;
        phuVongDaiMax = 0f; phuVongVoCu = -1f; caoXoanMax = 0f;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/KyNang/GioLoc/GioXoan.fbx"))
        {
            var m = o as UnityEngine.Mesh;
            if (m == null) continue;
            var v = m.vertices; var uv = m.uv; var c = m.colors;
            caoXoanMax = Mathf.Max(caoXoanMax, m.bounds.max.y);
            // Tach tung DAI = thanh phan lien thong theo tam giac
            var cha = new int[v.Length];
            for (int i = 0; i < v.Length; i++) cha[i] = i;
            System.Func<int, int> tim = null;
            tim = x => { while (cha[x] != x) { cha[x] = cha[cha[x]]; x = cha[x]; } return x; };
            var t = m.triangles;
            for (int i = 0; i < t.Length; i += 3) { int a = tim(t[i]); cha[tim(t[i + 1])] = a; cha[tim(t[i + 2])] = a; }
            // FBX xuat do bong PHANG -> Unity tach dinh theo tung mat (967 manh): gop dinh TRUNG VI TRI (lam tron 1 mm) truoc khi tach dai
            var theoViTri = new Dictionary<Vector3Int, int>();
            for (int i = 0; i < v.Length; i++)
            {
                var kh = new Vector3Int(Mathf.RoundToInt(v[i].x * 1000f), Mathf.RoundToInt(v[i].y * 1000f), Mathf.RoundToInt(v[i].z * 1000f));
                int j; if (theoViTri.TryGetValue(kh, out j)) cha[tim(i)] = tim(j); else theoViTri[kh] = i;
            }
            var nhom = new Dictionary<int, List<int>>();
            for (int i = 0; i < v.Length; i++) { int r = tim(i); List<int> ds; if (!nhom.TryGetValue(r, out ds)) nhom[r] = ds = new List<int>(); ds.Add(i); }
            foreach (var ds in nhom.Values)
            {
                soDaiXoan++;
                // Doc xoan: goc (mo vong, theo thu tu v doc dai) hoi quy theo do cao; v theo do cao
                ds.Sort((x, y) => uv[x].y.CompareTo(uv[y].y));
                float cong = 0f, truoc = Mathf.Atan2(v[ds[0]].z, v[ds[0]].x);
                var g = new float[ds.Count];
                double zTb = 0, gTb = 0, vTb = 0;
                for (int k = 0; k < ds.Count; k++)
                {
                    float a = Mathf.Atan2(v[ds[k]].z, v[ds[k]].x);
                    cong += Mathf.DeltaAngle(truoc * Mathf.Rad2Deg, a * Mathf.Rad2Deg) * Mathf.Deg2Rad; truoc = a; g[k] = cong;
                    zTb += v[ds[k]].y; gTb += cong; vTb += uv[ds[k]].y;
                }
                zTb /= ds.Count; gTb /= ds.Count; vTb /= ds.Count;
                double sxy = 0, sxx = 0, svz = 0;
                for (int k = 0; k < ds.Count; k++) { double dz = v[ds[k]].y - zTb; sxy += dz * (g[k] - gTb); sxx += dz * dz; svz += dz * (uv[ds[k]].y - vTb); }
                float xoan = (float)(sxy / sxx);
                float phu = PhuVongToiDa(v, ds, 0.25f);
                phuVongDaiMax = Mathf.Max(phuVongDaiMax, phu);
                if (phu >= 0.95f) soDaiKinVong++;
                // 04/10/2026: dai xoan NGUOC chieu quay (goc GIAM theo do cao; ChieuQuayGioLoc lam goc atan2 TANG) -> quay la vet dai LEO
                // LEN (menu 97: ban cung chieu troi XUONG -0,04 than/giay). Truoc: xoan > 0,3 (cung chieu).
                if (xoan < -0.3f) soDaiXoanDung++;
                if (svz > 0) soDaiVLen++;
                // Mo hai dau: dinh co v nho nhat va lon nhat (dau / cuoi dai) phai gan trong suot
                float aDau = c.Length > 0 ? c[ds[0]].a : 1f, aCuoi = c.Length > 0 ? c[ds[ds.Count - 1]].a : 1f;
                if (aDau < 0.05f && aCuoi < 0.05f) soDaiMoHaiDau++;
                baoXoan.AppendFormat(" {0}:{1:F2}rad/m,phu{2:P0}", m.name, xoan, phu);
            }
        }
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/KyNang/LocXoay/LocXoay.fbx"))
        {
            var m = o as UnityEngine.Mesh;
            if (m == null || m.name != "Vo1") continue;
            var v = m.vertices; var ds = new List<int>();
            for (int i = 0; i < v.Length; i++) ds.Add(i);
            phuVongVoCu = PhuVongToiDa(v, ds, 0.25f * 15.72f / 5f);   // lat cung ti le
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

    /// <summary>Mot tia set do luc vua thay: dau / duoi (do cao va xa truc) CHIA chieu cao than, be day loi, so nhanh.</summary>
    struct SoTia { public float hDau, rDau, hDuoi, rDuoi, day; public int nhanh; }

    static SoTia DoTia(LightningArc a, Vector3 chan, float cao)
    {
        var t = new SoTia();
        t.hDau = (a.start.y - chan.y) / cao; t.hDuoi = (a.end.y - chan.y) / cao;
        t.rDau = new Vector2(a.start.x - chan.x, a.start.z - chan.z).magnitude / cao;
        t.rDuoi = new Vector2(a.end.x - chan.x, a.end.z - chan.z).magnitude / cao;
        t.day = a.coreWidth; t.nhanh = a.branches;
        return t;
    }

    /// <summary>So den cua vat - KHONG tinh den sinh doi chieu mat dat (DenMatDat tu them luc ve) va den loe set cham dat.</summary>
    static int DemDen(GameObject go)
    {
        int n = 0;
        // khong tinh den sinh doi mat dat, va den cua LOE SET cham dat (BoltLight - 01/10/2026 dinh vao loc, song 3,5 s)
        foreach (var l in go.GetComponentsInChildren<Light>(true)) if (l.name != DenMatDat.TenDenDoi && l.name != "BoltLight") n++;
        return n;
    }

    /// <summary>Loe cham dat (DiTheo con loc) moi: ghi luc + CHO thay; o tuoi 0,2 s do trung vi khoang cach NGANG tu cac hat tia lua toi
    /// tam loe - hat THE GIOI tinh tu cho luc sinh (tam loe chay theo Gio loc 9,5 m/s, hat the gioi thi khong), hat CUC BO tu tam hien tai.</summary>
    static void TheoDoiLoe(Transform loc, Dictionary<DiTheo, Vector4> theoDoi, List<float> ketQua)
    {
        foreach (var d in Object.FindObjectsByType<DiTheo>(FindObjectsInactive.Exclude))
            if (d.theo == loc && !theoDoi.ContainsKey(d)) { var p = d.transform.position; theoDoi[d] = new Vector4(p.x, p.y, p.z, Time.time); }
        var xong = new List<DiTheo>();
        foreach (var kv in theoDoi)
        {
            if (kv.Value.w < 0f || Time.time - kv.Value.w < 0.2f) continue;
            xong.Add(kv.Key);
            if (kv.Key == null) continue;
            var ds = new List<float>();
            foreach (var ps in kv.Key.GetComponentsInChildren<ParticleSystem>())
            {
                if (ps.name != "Sparks") continue;
                var hat = new ParticleSystem.Particle[ps.particleCount];
                int n = ps.GetParticles(hat);
                bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
                Vector3 tam = cucBo ? kv.Key.transform.position : (Vector3)kv.Value;
                for (int i = 0; i < n; i++)
                {
                    Vector3 w = cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
                    ds.Add(new Vector2(w.x - tam.x, w.z - tam.z).magnitude);
                }
            }
            // 05/10/2026: loe cua Loc xoay da TAT lop Sparks (mau A + vet nut) - van dem loe, rong = -1 khi khong con hat tia lua
            ketQua.Add(ds.Count >= 5 ? TrungVi(ds) : -1f);
        }
        foreach (var d in xong) theoDoi[d] = new Vector4(0f, 0f, 0f, -1f);
    }

    /// <summary>Chup vi tri tung hat (theo randomSeed) cua he ten trong toa do he hinh.</summary>
    static void ChupHat(Transform hinh, string ten, Dictionary<uint, Vector3> ra)
    {
        ra.Clear();
        if (hinh == null) return;
        foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name != ten) continue;
            var hat = new ParticleSystem.Particle[ps.particleCount];
            int n = ps.GetParticles(hat);
            bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
            for (int i = 0; i < n; i++)
                ra[hat[i].randomSeed] = hinh.InverseTransformPoint(cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position);
        }
    }

    /// <summary>Toc goc trung binh (rad/s) quanh truc Y cua cac hat con song tu lan chup truoc.</summary>
    static float TocGocTB(Transform hinh, string ten, Dictionary<uint, Vector3> truoc, float dt)
    {
        var bay = new Dictionary<uint, Vector3>();
        ChupHat(hinh, ten, bay);
        double tong = 0; int n = 0;
        foreach (var kv in bay)
        {
            Vector3 p0;
            if (!truoc.TryGetValue(kv.Key, out p0)) continue;
            float d = Mathf.DeltaAngle(Mathf.Atan2(p0.z, p0.x) * Mathf.Rad2Deg, Mathf.Atan2(kv.Value.z, kv.Value.x) * Mathf.Rad2Deg);
            tong += d * Mathf.Deg2Rad; n++;
        }
        return n > 0 && dt > 0f ? (float)(tong / n / dt) : 0f;
    }

    static float TrungVi(List<float> ds)
    {
        if (ds.Count == 0) return 0f;
        var c = new List<float>(ds); c.Sort();
        return c[c.Count / 2];
    }

    static SoTia TrungBinh(List<SoTia> ds)
    {
        var t = new SoTia();
        if (ds.Count == 0) return t;
        foreach (var x in ds) { t.hDau += x.hDau; t.rDau += x.rDau; t.hDuoi += x.hDuoi; t.rDuoi += x.rDuoi; t.day += x.day; }
        float n = ds.Count;
        t.hDau /= n; t.rDau /= n; t.hDuoi /= n; t.rDuoi /= n; t.day /= n;
        return t;
    }

    static bool GanBang(Color a, Color b) { return Mathf.Abs(a.r - b.r) < 0.005f && Mathf.Abs(a.g - b.g) < 0.005f && Mathf.Abs(a.b - b.b) < 0.005f && Mathf.Abs(a.a - b.a) < 0.005f; }

    /// <summary>Hat cua cac he ten <paramref name="ten"/> trong he toa do <paramref name="hinh"/> (LocXoayHinh - da khu ti le, don vi
    /// Loc xoay goc): so hat, do cao 50% / 90%, ban kinh trung vi. Hat Local doi qua transform he hat, hat World lay thang.</summary>
    static void PhanBo(Transform hinh, string ten, out int n, out float h50, out float h90, out float r50)
    {
        n = 0; h50 = h90 = r50 = 0f;
        if (hinh == null) return;
        var cao = new List<float>(); var rs = new List<float>();
        foreach (var ps in hinh.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name != ten) continue;
            var hat = new ParticleSystem.Particle[ps.particleCount];
            int m = ps.GetParticles(hat);
            bool cucBo = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
            for (int i = 0; i < m; i++)
            {
                Vector3 w = cucBo ? ps.transform.TransformPoint(hat[i].position) : hat[i].position;
                Vector3 l = hinh.InverseTransformPoint(w);
                cao.Add(l.y); rs.Add(new Vector2(l.x, l.z).magnitude);
            }
        }
        n = cao.Count;
        if (n == 0) return;
        cao.Sort(); rs.Sort();
        h50 = cao[n / 2]; h90 = cao[Mathf.Min(n - 1, (int)(n * 0.9f))]; r50 = rs[n / 2];
    }

    static Color[] RenderRT(Camera cam, int W, int H)
    {
        var rt = new RenderTexture(W, H, 24);
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        var px = t.GetPixels();
        Object.Destroy(t); Object.Destroy(rt);
        return px;
    }

    /// <summary>Chup mot con loc tu phia sau, may quay cach 2,4 x chieu cao, nhin giua than (khung TI LE THEO CHIEU CAO - loc thu deu
    /// thi trung khung); render hai lan cung khung (bat / tat renderer cua no) -> mat na diem anh doi do sang &gt; 0,03, va do sang trung
    /// binh anh co loc trong mat na. Luu PNG neu co ten.</summary>
    static void ChupMatNa(Camera cam, GameObject go, Vector3 q, Vector3 huong, float cao, int W, int H, out bool[] matNa, out float sang, string luu)
    {
        Vector3 viTriCu = cam.transform.position; Quaternion quayCu = cam.transform.rotation;
        cam.transform.position = q - huong * (2.4f * cao) + Vector3.up * (0.5f * cao);
        cam.transform.LookAt(q + Vector3.up * (0.5f * cao));
        var rs = go.GetComponentsInChildren<Renderer>();
        var a = RenderRT(cam, W, H);
        foreach (var r in rs) r.enabled = false;
        var b = RenderRT(cam, W, H);
        foreach (var r in rs) r.enabled = true;
        cam.transform.position = viTriCu; cam.transform.rotation = quayCu;
        matNa = new bool[a.Length];
        double tong = 0; int n = 0;
        for (int i = 0; i < a.Length; i++)
        {
            float la = 0.299f * a[i].r + 0.587f * a[i].g + 0.114f * a[i].b, lb = 0.299f * b[i].r + 0.587f * b[i].g + 0.114f * b[i].b;
            if (Mathf.Abs(la - lb) > 0.03f) { matNa[i] = true; tong += la; n++; }
        }
        sang = n > 0 ? (float)(tong / n) : 0f;
        if (luu != null)
        {
            var t = new Texture2D(W, H, TextureFormat.RGB24, false);
            t.SetPixels(a); t.Apply();
            File.WriteAllBytes("PlayTestShots/" + luu + ".png", t.EncodeToPNG());
            Object.Destroy(t);
        }
    }

    static float IoU(bool[] x, bool[] y)
    {
        int giao = 0, hop = 0;
        for (int i = 0; i < x.Length; i++) { if (x[i] && y[i]) giao++; if (x[i] || y[i]) hop++; }
        return hop > 0 ? (float)giao / hop : 0f;
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
        // 09/10/2026 nguoi dung: 40 nang luong, hoi chieu 0,9 giay (truoc 20 / 0,4)
        Kiem(Mathf.Approximately(toi.gioLocCost, 40f) && Mathf.Approximately(nl, 40f), "nang luong khong phai 40");
        Kiem(Mathf.Approximately(toi.gioLocCooldown, 0.9f) && Mathf.Approximately(hc, 0.9f), "hoi chieu khong phai 0,9");
        Kiem(Mathf.Approximately(toi.gioLocCastTime, 0.38f), "niem khong phai 0,38");
        // 26/09/2026: hinh quat - cap 1-4 ba loc, cap 5 nam loc; 29/09/2026 goc 15 -> 20 do giua hai loc (nguoi dung chon)
        // 04/10/2026 nguoi dung: cap 1-4 HAI loc, cap 5 BA loc, lech 30 do (truoc 3 / 5 loc, 20 do)
        Kiem(GioLoc.SoLocTheoCap(1) == 2 && GioLoc.SoLocTheoCap(4) == 2 && GioLoc.SoLocTheoCap(5) == 3 && Mathf.Approximately(GioLoc.GocQuat, 30f)
             && Mathf.Approximately(GioLoc.ThoiGianSong, 4.5f), "khong phai 2 loc (cap 5: 3) / 30 do / 4,5 giay");
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
        CapDo.MoCaDuongChoPhepThu(K);
        // Cap 1 chi co 1 diem: len mot cap de co them diem mo Qua cau lua (muc H can mot phep dang niem)
        CapDo.Them(CapDo.CanDeLenCap(1));
        CapDo.MoCaDuongChoPhepThu(0);
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
        // Hoi chieu 0,9 giay (09/10/2026 nguoi dung; truoc 0,4) - so viet tay
        bool tuChoiHoiChieu = lucNhan >= 0.9f - 0.001f;
        bool tungLai = lucNhan > 0f && lucNhan < 1.0f;
        yield return new WaitForSeconds(0.6f);
        Ghi(string.Format("B. khoa -> tu choi {0}; mo khoa -> tung {1}, {2} loc bay, ton {3} nang luong; hoi chieu ngay sau khi tung {4:F2} s; bam lai moi khung -> duoc nhan sau {5:F3} s (thay nhac hoi chieu: {6}); bia truoc mat mat {7:F0} mau, ke danh cuoi la nguoi tung {8}",
            tuChoiKhoa, daTung, soLocBay, manaTon, hoiNgaySau, lucNhan, thayNhacHoi, mauB - biaB.health, biaB.keDanhCuoi == mauToi));
        Kiem(tuChoiKhoa, "ky nang khoa ma van tung duoc");
        Kiem(daTung && soLocBay == 2, "tung Gio loc cap 1 khong ra dung 2 loc");
        Kiem(Mathf.Abs(manaTon - 40f) < 0.01f, "khong ton dung 40 nang luong");
        Kiem(tuChoiHoiChieu && tungLai, "hoi chieu 0,9 giay khong dung");
        Kiem(biaB.keDanhCuoi == mauToi, "trung bia ma khong ghi ke danh (mat kinh nghiem)");
        Object.Destroy(biaB.gameObject);
        XoaLoc();
        yield return new WaitForSeconds(0.5f);

        // ================= C + D. HINH = LOC XOAY THU NHO, TOC DO, THOI GIAN SONG =================
        // 01/10/2026 (nguoi dung: "Gio loc co hieu ung giong hoan toan Loc xoay, chi co kich thuoc bang Gio loc hien gio"; chon thu
        // deu x0,318, bo mau + may giong, giu 240 hat/giay). MOI so sanh la voi mot LOC XOAY THAT (prefab qua Tornado.Spawn +
        // Tornado.Start, dung yen) - khong voi hang so trong code. DOI CHUNG: Gio loc moi NAM NGANG (05/10/2026 hinh Gio loc cu
        // da xoa theo nguoi dung - cung lop, cung mau nhung nam ngang thi bong phai lech han cot Loc xoay that, chung to phep so anh
        // phan biet duoc hinh dang; ban LAT NGUOC thu truoc ra IoU 0,70 ~ 0,73 vi bong la dai gio + bui, it phu thuoc chieu pheu).
        Ghi("");
        {
            float k = VfxFactory.HeSoHinhGioLoc;
            Vector3 phai = Vector3.Cross(Vector3.up, huong);
            var locXoay = Tornado.Spawn(goc + huong * 40f, huong, 0);
            locXoay.moveSpeed = 0f; locXoay.wanderAmount = 0f; locXoay.duration = 120f; locXoay.catchRadius = 0f;
            yield return null; yield return null;
            float caoLon = CaoHinh(locXoay.gameObject);
            var hinhLx = locXoay.transform.Find("LocXoayHinh");
            float rVongLx = -1f;
            foreach (var ps in locXoay.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "BuiChan") rVongLx = ps.shape.radius;

            // ---- C1. Tia set THAT cua Loc xoay trong 4,5 s (~20 tia): do luc vua thay, chia cho chieu cao than ----
            var arcDaThay = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            var tiaLon = new List<SoTia>();
            float hanLon = Time.time + 4.5f;
            yield return new WaitForEndOfFrame();
            var loeLon = new Dictionary<DiTheo, Vector4>(); var rongLoeLon = new List<float>();
            while (Time.time < hanLon)
            {
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                    if (arcDaThay.Add(a) && a.name != TiaBoDat.TenTia) tiaLon.Add(DoTia(a, locXoay.transform.position, caoLon));   // bo tia con bo tren dat (05/10/2026)
                TheoDoiLoe(locXoay.transform, loeLon, rongLoeLon);
                yield return new WaitForEndOfFrame();
            }

            // ---- C2. Cau truc: Gio loc dung yen (BuildGioLoc) so TUNG thanh phan voi Loc xoay that ----
            Vector3 pGl = goc + huong * 40f + phai * 14f;
            pGl.y = GioLoc.MatDatY(pGl, pGl.y);
            var glDo = VfxFactory.BuildGioLoc();
            glDo.name = "TAM_GioLocDo";
            glDo.transform.position = pGl;
            Vector3 pCu = goc + huong * 40f - phai * 14f;
            pCu.y = GioLoc.MatDatY(pCu, pCu.y);
            var glCu = VfxFactory.BuildGioLoc();
            glCu.name = "TAM_GioLocNamNgang";
            glCu.transform.rotation = Quaternion.AngleAxis(90f, huong);
            glCu.transform.position = pCu;
            yield return null; yield return null;
            var hinhGl = glDo.transform.Find("LocXoayHinh");
            float caoNhoDung = CaoHinh(glDo);
            // 01/10/2026 lan ba (nguoi dung): Gio loc KHAC Loc xoay that o dung cac cho nay - so DUNG HE SO NGUOI DUNG CHON (viet tay,
            // khong doc hang trong code): BO HaoQuang + StormLight; mau vo / bui x0,7 (toi 30%); BuiCuonLen so hat x2, quay x2. Moi thu khac y het.
            const float mauMong = 0.7f, buiLenMong = 2f, quayMong = 2f;
            // 03/10/2026: bo them 2 lop cuon len than (nguoi dung: "bo tat ca bui khoi cuon tu chan len tan dinh") - chi giu BuiChan
            // 03/10/2026 (lan hai): bo ca 4 vo pheu + vanh - than la cac dai gio xoan GioXoan (kiem rieng ben duoi)
            var phaiBo = new HashSet<string> { "HaoQuang", "StormLight", "BuiThanDuoi", "BuiCuonLen", "Vo0", "Vo1", "Vo2", "Vo3", "Vanh" };
            int soThanhPhan = 0, soKhop = 0, daBo = 0; string thieu = "", lech = "", thua = "";
            var mpbDo = new MaterialPropertyBlock();
            System.Func<Color, Color, bool> toiDung = (g, l) => Mathf.Abs(g.r - l.r * mauMong) < 0.005f && Mathf.Abs(g.g - l.g * mauMong) < 0.005f
                                                                && Mathf.Abs(g.b - l.b * mauMong) < 0.005f && Mathf.Abs(g.a - l.a) < 0.005f;
            if (hinhLx != null && hinhGl != null)
            {
                foreach (Transform cL in hinhLx)
                {
                    soThanhPhan++;
                    var cG = hinhGl.Find(cL.name);
                    if (phaiBo.Contains(cL.name)) { if (cG == null) daBo++; else lech += cL.name + "(con) "; continue; }
                    if (cG == null) { thieu += cL.name + " "; continue; }
                    bool ok = (cG.localPosition - cL.localPosition).magnitude < 0.01f;
                    var mfL = cL.GetComponent<MeshFilter>(); var mfG = cG.GetComponent<MeshFilter>();
                    if (mfL != null && (mfG == null || mfG.sharedMesh != mfL.sharedMesh)) ok = false;
                    var mrL = cL.GetComponent<MeshRenderer>(); var mrG = cG.GetComponent<MeshRenderer>();
                    if (mrL != null)
                    {
                        // Mau THAT Gio loc ve = khoi thuoc tinh (MaterialPropertyBlock); vat lieu dung chung phai GIU nguyen mau Loc xoay
                        Color mauVe = Color.clear;
                        if (mrG != null) { mrG.GetPropertyBlock(mpbDo); mauVe = mpbDo.GetColor("_TintColor"); }
                        if (mrG == null || mrG.sharedMaterial.mainTexture != mrL.sharedMaterial.mainTexture
                            || !toiDung(mauVe, mrL.sharedMaterial.GetColor("_TintColor"))
                            || !GanBang(mrG.sharedMaterial.GetColor("_TintColor"), mrL.sharedMaterial.GetColor("_TintColor"))) ok = false;
                    }
                    var spL = cL.GetComponent<Spin>(); var spG = cG.GetComponent<Spin>();
                    if (spL != null && (spG == null || Mathf.Abs(spG.degreesPerSecond - spL.degreesPerSecond) > 0.01f)) ok = false;
                    var suL = cL.GetComponent<ScrollUV>(); var suG = cG.GetComponent<ScrollUV>();
                    if (suL != null && (suG == null || (suG.speed - suL.speed).magnitude > 1e-4f)) ok = false;
                    var psL = cL.GetComponent<ParticleSystem>(); var psG = cG.GetComponent<ParticleSystem>();
                    if (psL != null)
                    {
                        float hsHat = cL.name == "BuiCuonLen" ? buiLenMong : 1f, hsQuay = cL.name == "BuiCuonLen" ? quayMong : 1f;
                        if (psG == null || Mathf.Abs(psG.emission.rateOverTime.constant - psL.emission.rateOverTime.constant * hsHat) > 0.01f
                            || Mathf.Abs(psG.main.maxParticles - psL.main.maxParticles * hsHat) > 1.01f
                            || !toiDung(psG.main.startColor.colorMin, psL.main.startColor.colorMin) || !toiDung(psG.main.startColor.colorMax, psL.main.startColor.colorMax)
                            || psG.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture != psL.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture
                            || Mathf.Abs(psG.shape.radius - psL.shape.radius) > 0.01f
                            || Mathf.Abs(psG.velocityOverLifetime.orbitalYMultiplier - psL.velocityOverLifetime.orbitalYMultiplier * hsQuay) > 1e-3f) ok = false;
                    }
                    if (cL.GetComponent<Light>() != null) ok = false;     // den chi o StormLight (da bo)
                    if (ok) soKhop++; else lech += cL.name + " ";
                }
                foreach (Transform cG in hinhGl) if (hinhLx.Find(cG.name) == null) thua += cG.name + " ";
            }
            int soCucBo = 0, soHe = 0;
            // Chi he hat TRONG THAN (LocXoayHinh) - hai vet sau lung (03/10/2026) co y o khong gian THE GIOI, la con cua goc hinh
            foreach (var ps in hinhGl != null ? hinhGl.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0])
            {
                soHe++;
                if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local && ps.main.scalingMode == ParticleSystemScalingMode.Hierarchy) soCucBo++;
            }
            int soDenGl = DemDen(glDo), soDenLx = DemDen(locXoay.gameObject);
            Ghi(string.Format("C2. cau truc: Loc xoay that {0} thanh phan, Gio loc da bo {1}/9 (sang, 2 lop cuon len, 4 vo + vanh), khop (mau x0,7) {2} (thieu: {3}| lech: {4}| thua: {5}); ti le hinh {6:F3} (mong {7:F3}); cao Gio loc {8:F2} m / Loc xoay {9:F2} m = x{10:F3}; he hat cuc bo + Hierarchy {11}/{12}; den {13} (Loc xoay that {14})",
                soThanhPhan, daBo, soKhop, thieu, lech, thua, hinhGl != null ? hinhGl.localScale.y : -1f, k, caoNhoDung, caoLon, caoNhoDung / caoLon, soCucBo, soHe, soDenGl, soDenLx));
            Kiem(hinhLx != null && hinhGl != null && soThanhPhan >= 8 && daBo == 9 && soKhop == soThanhPhan - 9 && thua == "", "Gio loc khong dung Loc xoay that (bo sang + 2 lop cuon len + 4 vo + vanh, toi 30%)");
            // THAN DAI GIO XOAN: 3 nhom, anh GioXoan, mau = mau vo Loc xoay Vo0/1/2 (viet tay) x0,8 x0,7, quay CHAM cung chieu vo Loc xoay that,
            // anh truot v AM (chay len), met that (khong an 0,318)
            var thanXoan = glDo.transform.Find("GioXoan");
            Color[] mauVoGoc = { new Color(0.86f, 0.88f, 0.92f), new Color(0.92f, 0.94f, 0.97f), new Color(0.97f, 0.98f, 1.00f) };
            string[] tenDai = { "DaiTrong", "DaiGiua", "DaiNgoai" };
            var spLx = hinhLx != null && hinhLx.Find("Vo0") != null ? hinhLx.Find("Vo0").GetComponent<Spin>() : null;
            int daiDung = 0; string moTaDai = "";
            for (int i = 0; i < 3; i++)
            {
                var d = thanXoan != null ? thanXoan.Find(tenDai[i]) : null;
                if (d == null) { moTaDai += tenDai[i] + ":THIEU "; continue; }
                var mr = d.GetComponent<MeshRenderer>(); var sp = d.GetComponent<Spin>(); var su = d.GetComponent<ScrollUV>();
                var tint = mr != null ? mr.sharedMaterial.GetColor("_TintColor") : Color.clear;
                var mong = mauVoGoc[i] * 0.56f;
                bool ok = mr != null && mr.sharedMaterial.mainTexture != null && mr.sharedMaterial.mainTexture.name == "GioXoan"
                          && Mathf.Abs(tint.r - mong.r) < 0.005f && Mathf.Abs(tint.g - mong.g) < 0.005f && Mathf.Abs(tint.b - mong.b) < 0.005f
                          && sp != null && spLx != null && Mathf.Sign(sp.degreesPerSecond) == Mathf.Sign(spLx.degreesPerSecond) && Mathf.Abs(sp.degreesPerSecond) > 90f
                          && su != null && su.speed.y < 0f && Mathf.Abs(d.lossyScale.y - 1f) < 0.001f;
                if (ok) daiDung++;
                moTaDai += string.Format("{0}: mau ({1:F2} {2:F2} {3:F2}) quay {4:F0} do/s truot {5:F2} ", tenDai[i], tint.r, tint.g, tint.b, sp != null ? sp.degreesPerSecond : 0f, su != null ? su.speed.y : 0f);
            }
            Ghi("C2. than dai gio xoan: " + moTaDai + "(vo Loc xoay that quay " + (spLx != null ? spLx.degreesPerSecond.ToString("F0") : "?") + " do/s)");
            Ghi(string.Format("C2. luoi dai xoan (do ngoai Play): {0} dai, cao toi {1:F2} m; kin vong (phu >= 95% mot lat 0,25 m) {2}, phu vong lon nhat {3:P0} - DOI CHUNG vo pheu Loc xoay Vo1 {4:P0}; xoan NGUOC chieu quay (leo len khi quay) {5}/{0}, v tang theo cao {6}/{0}, mo hai dau {7}/{0};{8}",
                soDaiXoan, caoXoanMax, soDaiKinVong, phuVongDaiMax, phuVongVoCu, soDaiXoanDung, soDaiVLen, soDaiMoHaiDau, baoXoan));
            Kiem(thanXoan != null && thanXoan.parent == glDo.transform && daiDung == 3, "than Gio loc khong phai 3 nhom dai gio xoan dung (anh, mau toi 30%, quay cham cung chieu, truot len)");
            Kiem(phuVongVoCu >= 0.95f, "doi chung: vo pheu Loc xoay khong kin vong - phep do phu vong vo nghia");
            // 03/10/2026 lan ba: 12 -> 18 dai rong hon (nguoi dung khoanh khoang trong) - moi dai van HO
            Kiem(soDaiXoan == 18 && soDaiKinVong == 0 && phuVongDaiMax < 0.6f, "con dai gio kin vong tron (nhin ro hinh tron) / khong du 18 dai");
            Kiem(soDaiXoanDung == soDaiXoan && soDaiVLen == soDaiXoan && soDaiMoHaiDau == soDaiXoan, "dai xoan CUNG chieu quay (quay la troi xuong) / anh truot sai chieu / dau dai khong mo");
            // VET GIO XOAN LEN (04/10/2026): con cua goc hinh, cuc bo, chi ve duoi (anh VetGio), lap + prewarm
            var vetGio = glDo.transform.Find("VetGioXoan");
            var psVg = vetGio != null ? vetGio.GetComponent<ParticleSystem>() : null;
            var rVg = psVg != null ? psVg.GetComponent<ParticleSystemRenderer>() : null;
            bool vgDung = psVg != null && psVg.main.simulationSpace == ParticleSystemSimulationSpace.Local && psVg.trails.enabled && !psVg.trails.worldSpace
                          && rVg.renderMode == ParticleSystemRenderMode.None && rVg.trailMaterial != null && rVg.trailMaterial.mainTexture != null
                          && rVg.trailMaterial.mainTexture.name == "VetGio" && psVg.main.loop && psVg.main.prewarm;
            Ghi("C2. vet gio xoan len: " + (psVg == null ? "THIEU" : string.Format("cuc bo {0}, duoi {1} ({2}), che do ve {3}, lap+prewarm {4}",
                psVg.main.simulationSpace == ParticleSystemSimulationSpace.Local, psVg.trails.enabled, rVg.trailMaterial != null ? rVg.trailMaterial.mainTexture.name : "-", rVg.renderMode, psVg.main.loop && psVg.main.prewarm)));
            Kiem(vgDung, "thieu / sai lop vet gio xoan len (cuc bo, chi ve duoi anh VetGio, lap + prewarm)");
            Kiem(Mathf.Abs(caoXoanMax - 5f) < 0.2f, "than dai xoan khong cao ~5 m");
            // Bui chan prewarm (co bui ngay luc tung) - 02/10/2026 nguoi dung chon "khoi co san"; nay chi con mot lop bui
            int soBuiGl = 0, soPrewarmGl = 0;
            foreach (var ps in glDo.GetComponentsInChildren<ParticleSystem>(true))
                if (ps.name.StartsWith("Bui")) { soBuiGl++; if (ps.main.prewarm && ps.main.loop) soPrewarmGl++; }
            // VET SAU LUNG (03/10/2026): hai he THE GIOI sinh theo QUANG DUONG, con cua goc hinh; khoi den xam TOI hon bui chan ro
            ParticleSystem vetBui = null, vetKhoi = null, buiChanGl = null;
            foreach (var ps in glDo.GetComponentsInChildren<ParticleSystem>(true))
            { if (ps.name == "VetBuiXam") vetBui = ps; if (ps.name == "VetKhoiDen") vetKhoi = ps; if (ps.name == "BuiChan") buiChanGl = ps; }
            System.Func<Color, float> sangMau = c => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            bool vetDung = vetBui != null && vetKhoi != null && buiChanGl != null
                           && vetBui.transform.parent == glDo.transform && vetKhoi.transform.parent == glDo.transform
                           && vetBui.main.simulationSpace == ParticleSystemSimulationSpace.World && vetKhoi.main.simulationSpace == ParticleSystemSimulationSpace.World
                           && vetBui.emission.rateOverTime.constant == 0f && vetKhoi.emission.rateOverTime.constant == 0f
                           && vetBui.emission.rateOverDistance.constant > 0f && vetKhoi.emission.rateOverDistance.constant > 0f
                           && vetBui.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture == buiChanGl.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture
                           && GanBang(vetBui.main.startColor.colorMin, buiChanGl.main.startColor.colorMin)
                           && sangMau(vetKhoi.main.startColor.colorMax) < 0.7f * sangMau(buiChanGl.main.startColor.colorMin);
            Ghi(string.Format("C2. lop bui: {0} (prewarm {1}); vet sau lung: bui xam {2} hat/m, khoi den xam {3} hat/m (khong gian the gioi, theo quang duong); do sang mau khoi {4:F2} / bui chan {5:F2}",
                soBuiGl, soPrewarmGl, vetBui != null ? vetBui.emission.rateOverDistance.constant : -1f, vetKhoi != null ? vetKhoi.emission.rateOverDistance.constant : -1f,
                vetKhoi != null ? sangMau(vetKhoi.main.startColor.colorMax) : -1f, buiChanGl != null ? sangMau(buiChanGl.main.startColor.colorMin) : -1f));
            Kiem(soBuiGl == 1 && soPrewarmGl == 1, "Gio loc con lop bui cuon len than / bui chan chua prewarm");
            Kiem(vetDung, "vet sau lung khong dung (2 he the gioi theo quang duong, bui = bui chan, khoi den xam toi hon)");
            Kiem(Mathf.Abs(caoNhoDung - 5f) < 0.2f && Mathf.Abs(caoNhoDung / caoLon - k) < 0.02f, "Gio loc khong phai Loc xoay thu deu cao 5 m");
            Kiem(soCucBo == soHe && soHe > 0, "con he hat khong mo phong cuc bo (bui se roi lai sau lung loc bay 9,5 m/s / khong thu ti le)");
            Kiem(soDenGl == 0 && soDenLx >= 1, "Gio loc con den chop (doi chung: Loc xoay that co den)");

            // ---- C3. Phan bo hat bui (dung yen 4 s): toa do trong he LocXoayHinh = don vi Loc xoay goc (da khu ti le) ----
            foreach (var go in new[] { locXoay.gameObject, glDo, glCu })
                foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true)) { var m = ps.main; m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate; }
            yield return new WaitForSeconds(4f);
            int tongL = 0, tongG = 0, tongCu = 0, lopDat = 0;
            {
                int nL, nG; float h50L, h90L, r50L, h50G, h90G, r50G;
                PhanBo(hinhLx, "BuiChan", out nL, out h50L, out h90L, out r50L);
                PhanBo(hinhGl, "BuiChan", out nG, out h50G, out h90G, out r50G);
                tongL += nL; tongG += nG;
                bool dat = nL > 20 && Mathf.Abs((float)nG / nL - 1f) < 0.25f && Mathf.Abs(h90G / h90L - 1f) < 0.15f
                           && Mathf.Abs(h50G / Mathf.Max(0.01f, h50L) - 1f) < 0.2f && Mathf.Abs(r50G / r50L - 1f) < 0.2f;
                if (dat) lopDat++;
                Ghi(string.Format("C3. BuiChan: Loc xoay that {0} hat, cao 50% {1:F2} / 90% {2:F2} m, ban kinh trung vi {3:F2} m | Gio loc (don vi Loc xoay) {4} hat, {5:F2} / {6:F2} m, {7:F2} m -> {8}",
                    nL, h50L, h90L, r50L, nG, h50G, h90G, r50G, dat ? "khop" : "LECH"));
            }
            // BO cuon len than: dem hat bui o tren 40% than (6 m don vi Loc xoay) - Gio loc ~0; DOI CHUNG Loc xoay that (con 3 lop) phai nhieu
            int trenL = 0, trenG = 0;
            foreach (var ten in new[] { "BuiChan", "BuiCuonLen", "BuiThanDuoi", "BuiThanTren" })
            {
                var dsL = new Dictionary<uint, Vector3>(); var dsG = new Dictionary<uint, Vector3>();
                ChupHat(hinhLx, ten, dsL); ChupHat(hinhGl, ten, dsG);
                foreach (var v in dsL.Values) if (v.y > 6f) trenL++;
                foreach (var v in dsG.Values) if (v.y > 6f) trenG++;
            }
            // Loc DUNG YEN khong de vet (vet sinh theo quang duong)
            int vetDungYen = 0;
            foreach (var ps in glDo.GetComponentsInChildren<ParticleSystem>(true)) if (ps.name == "VetBuiXam" || ps.name == "VetKhoiDen") vetDungYen += ps.particleCount;
            Ghi(string.Format("C3. hat bui tren 40% than (6 m don vi Loc xoay): Gio loc {0}, DOI CHUNG Loc xoay that {1}; hat vet sau lung khi Gio loc DUNG YEN 4 s: {2}",
                trenG, trenL, vetDungYen));
            Kiem(lopDat == 1, "bui chan Gio loc (da khu ti le) khong khop bui chan Loc xoay that");
            Kiem(trenL > 100 && trenG < 0.03f * trenL, "Gio loc van con bui cuon len than (doi chung Loc xoay that phai nhieu)");
            Kiem(vetDungYen == 0, "loc dung yen ma van de vet sau lung");

            // ---- C4. Anh: dua ca ba len troi (nen dong deu), chup cung khung theo chieu cao moi con; bat / tat renderer cung khung -> mat na ----
            bool suongCu = RenderSettings.fog;
            locXoay.enabled = false;              // dung Update (bam dat / di / phong set) - hinh van chay
            Vector3 bauTroi = goc + Vector3.up * 250f;
            Vector3 qL = bauTroi + phai * 70f, qG = bauTroi, qC = bauTroi - phai * 70f;
            float caoCu = caoNhoDung;   // doi chung nam ngang: cung co voi Gio loc moi; dat giua than vao TAM khung chup (q + 0,5 cao)
            Vector3 trucNam = glCu.transform.rotation * Vector3.up;
            locXoay.transform.position = qL; glDo.transform.position = qG;
            glCu.transform.position = qC + Vector3.up * (0.5f * caoCu) - trucNam * (0.5f * caoCu);
            yield return new WaitForSeconds(3.2f);   // bui chan the gioi cua Loc xoay sinh lai quanh cho moi
            RenderSettings.fog = false;
            var cam = Camera.main;
            float iouG = 0f, iouC = 0f, sangG = 0f, sangC = 0f, phuL = 0f;
            const int W = 360, H = 480, SoLan = 4;
            for (int lan = 0; lan < SoLan; lan++)
            {
                yield return new WaitForEndOfFrame();
                bool luu = lan == SoLan - 1;
                float sL, sG, sC; bool[] mL, mG, mC;
                ChupMatNa(cam, locXoay.gameObject, qL, huong, caoLon, W, H, out mL, out sL, luu ? "gioloc_6a_locxoay_that" : null);
                ChupMatNa(cam, glDo, qG, huong, caoNhoDung, W, H, out mG, out sG, luu ? "gioloc_6b_gioloc_moi" : null);
                ChupMatNa(cam, glCu, qC, huong, caoCu, W, H, out mC, out sC, luu ? "gioloc_6c_gioloc_nam_ngang" : null);
                iouG += IoU(mL, mG); iouC += IoU(mL, mC);
                sangG += sG / Mathf.Max(1e-4f, sL); sangC += sC / Mathf.Max(1e-4f, sL);
                int phu = 0; foreach (var b in mL) if (b) phu++; phuL += (float)phu / mL.Length;
                yield return new WaitForSeconds(0.3f);
            }
            RenderSettings.fog = suongCu;
            iouG /= SoLan; iouC /= SoLan; sangG /= SoLan; sangC /= SoLan; phuL /= SoLan;
            Ghi(string.Format("C4. anh (chup theo chieu cao moi con, {0} lan): Loc xoay that phu {1:P0} khung; Gio loc moi trung hinh IoU {2:F2}, do sang trong hinh x{3:F2}; DOI CHUNG Gio loc nam ngang IoU {4:F2}, x{5:F2}",
                SoLan, phuL, iouG, sangG, iouC, sangC));
            Kiem(phuL > 0.05f, "doi chung: Loc xoay that khong hien trong anh - phep chup vo nghia");
            // 01/10/2026 lan ba: Gio loc toi 30% + bo quang sang mieng -> do sang trong hinh phai THAP hon ro (hinh van la Loc xoay thu nho)
            // 03/10/2026 than la dai xoan ho (khong con vo kin) -> chi doi hinh van chiem dang loc (IoU > 0,5) va toi hon
            Kiem(iouG > 0.5f && sangG < 0.85f && sangG > 0.3f, "anh Gio loc khong con dang loc / khong toi hon Loc xoay");
            Kiem(iouC < iouG - 0.1f, "DOI CHUNG: Gio loc nam ngang cung 'giong' Loc xoay - phep so anh khong phan biet duoc hinh dang");
            // ---- C6 (03/10/2026, nguoi dung khoanh KHOANG TRONG o than / chan / tren): DO PHU cua than dai gio - chi ve 3 nhom dai (lop 31)
            // tren nen DEN, may quay TRUC GIAO nhin ngang (1 px = 1 cm); trong vien than |x| <= 0,95 x vo chinh, chia chan / giua / tren:
            // "phu" = ti le diem anh sang > 0,05. DOI CHUNG: chi bat NHOM NGOAI -> phai thua ro hon (phep do phan biet duoc).
            {
                var thanX = glDo.transform.Find("GioXoan");
                var rsDai = thanX != null ? thanX.GetComponentsInChildren<MeshRenderer>() : new MeshRenderer[0];
                var lopCu = new Dictionary<MeshRenderer, int>();
                foreach (var r in rsDai) { lopCu[r] = r.gameObject.layer; r.gameObject.layer = 31; }
                var goCam = new GameObject("TAM_CamPhu");
                var cp = goCam.AddComponent<Camera>();
                cp.enabled = false; cp.orthographic = true; cp.orthographicSize = 3f; cp.cullingMask = 1 << 31;
                cp.clearFlags = CameraClearFlags.SolidColor; cp.backgroundColor = Color.black; cp.nearClipPlane = 0.3f; cp.farClipPlane = 60f;
                cp.transform.position = qG + Vector3.up * 2.5f - huong * 20f; cp.transform.rotation = Quaternion.LookRotation(huong);
                bool suong2 = RenderSettings.fog; RenderSettings.fog = false;
                System.Func<float[]> doPhu = () =>
                {
                    const int N = 600;
                    var rt = new RenderTexture(N, N, 24); cp.targetTexture = rt; cp.Render(); cp.targetTexture = null;
                    var tr = RenderTexture.active; RenderTexture.active = rt;
                    var tx = new Texture2D(N, N, TextureFormat.RGB24, false); tx.ReadPixels(new Rect(0, 0, N, N), 0, 0); tx.Apply();
                    RenderTexture.active = tr;
                    var px = tx.GetPixels(); Object.Destroy(tx); Object.Destroy(rt);
                    var kq = new float[3]; var n = new int[3];
                    for (int y = 0; y < N; y++)
                    {
                        float z = 2.5f + (y - N / 2 + 0.5f) * 0.01f;
                        int ph = z < 0.05f ? -1 : z < 1.65f ? 0 : z < 3.25f ? 1 : z < 4.85f ? 2 : -1;
                        if (ph < 0) continue;
                        float rr = 0.95f * VfxFactory.BanKinhLocXoay(z, k);
                        for (int x = 0; x < N; x++)
                        {
                            float X = (x - N / 2 + 0.5f) * 0.01f;
                            if (Mathf.Abs(X) > rr) continue;
                            var c = px[y * N + x];
                            n[ph]++; if (0.299f * c.r + 0.587f * c.g + 0.114f * c.b > 0.05f) kq[ph] += 1f;
                        }
                    }
                    for (int i = 0; i < 3; i++) kq[i] /= Mathf.Max(1, n[i]);
                    return kq;
                };
                var phuDu = doPhu();
                foreach (var r in rsDai) if (r.name != "DaiNgoai") r.enabled = false;
                var phuNgoai = doPhu();
                foreach (var r in rsDai) r.enabled = true;
                foreach (var kv in lopCu) kv.Key.gameObject.layer = kv.Value;
                RenderSettings.fog = suong2;
                Object.Destroy(goCam);
                Ghi(string.Format("C6. phu than dai gio (truc giao, trong vien than): chan {0:P0}, giua {1:P0}, tren {2:P0} | DOI CHUNG chi nhom ngoai: {3:P0} / {4:P0} / {5:P0} (Blender cung cach do: ban 12 dai 61-63% / 85-94% / 77%)",
                    phuDu[0], phuDu[1], phuDu[2], phuNgoai[0], phuNgoai[1], phuNgoai[2]));
                Kiem(rsDai.Length == 3 && phuDu[0] > 0.8f && phuDu[1] > 0.8f && phuDu[2] > 0.8f, "than dai gio con nhieu khoang trong (chan / giua / tren < 80%)");
                Kiem(phuNgoai[0] < phuDu[0] - 0.1f || phuNgoai[1] < phuDu[1] - 0.1f || phuNgoai[2] < phuDu[2] - 0.1f, "doi chung: chi nhom ngoai ma phu nhu du - phep do khong phan biet duoc");
            }
            Object.Destroy(locXoay.gameObject); Object.Destroy(glDo); Object.Destroy(glCu);
            yield return new WaitForSeconds(0.3f);

            // ---- C5 + D. Gio loc THAT dang bay ----
            var loc = GioLoc.Spawn(goc + huong * 1.2f, huong, maskEnemy);
            float lucSinh = Time.time;
            yield return null; yield return null;
            float caoNho = CaoHinh(loc.gameObject);
            var hinhBay = loc.transform.Find("GioLocHinh/LocXoayHinh");
            int soDen = DemDen(loc.gameObject);
            Kiem(hinhBay != null && Mathf.Abs(hinhBay.lossyScale.y - k) < 0.005f, "Gio loc tung that khong dung hinh Loc xoay thu nho");

            // CHIEU QUAY THAT cua tung lop + CHIEU TRUOT anh
            var gocTruoc = new Dictionary<Transform, float>();
            var sps = loc.GetComponentsInChildren<Spin>();
            // Bui CHAN (03/10/2026 lop duy nhat con lai): theo doi tung hat (randomSeed) - bay LEN va quay CUNG chieu than
            ParticleSystem psChan = null;
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>()) { if (ps.name == "BuiChan") psChan = ps; }
            ParticleSystem psLen = psChan;
            yield return new WaitForSeconds(0.5f);
            var hat0 = new ParticleSystem.Particle[psLen != null ? psLen.main.maxParticles : 1];
            int n0 = psLen != null ? psLen.GetParticles(hat0) : 0;
            var cuHat = new Dictionary<uint, Vector3>();
            for (int i = 0; i < n0; i++) cuHat[hat0[i].randomSeed] = hat0[i].position;
            // VONG PHUN bui chan: nhan ban, tat van toc, phun 200 hat - lech doc va ban kinh luc sinh (don vi he hat, chua nhan ti le)
            float yMaxLucSinh = 0f, rMaxLucSinh = 0f; int soSinhMoi = 0;
            if (psChan != null)
            {
                var ban = Object.Instantiate(psChan.gameObject, psChan.transform.parent);
                var psBan = ban.GetComponent<ParticleSystem>();
                psBan.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var vBan = psBan.velocityOverLifetime; vBan.enabled = false;
                var mBan = psBan.main; mBan.startSpeed = 0f; mBan.maxParticles = 300;
                var eBan = psBan.emission; eBan.enabled = false;
                psBan.Emit(200);
                var hBan = new ParticleSystem.Particle[300];
                soSinhMoi = psBan.GetParticles(hBan);
                for (int i = 0; i < soSinhMoi; i++)
                {
                    yMaxLucSinh = Mathf.Max(yMaxLucSinh, Mathf.Abs(hBan[i].position.y));
                    rMaxLucSinh = Mathf.Max(rMaxLucSinh, new Vector2(hBan[i].position.x, hBan[i].position.z).magnitude);
                }
                Object.Destroy(ban);
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
            foreach (var su in loc.GetComponentsInChildren<ScrollUV>()) { if (su.speed.y < 0f) truotLen++; else truotSai++; }
            int hatLen = 0, hatXuong = 0, hatCungChieu = 0, hatNguoc = 0;
            if (psLen != null)
            {
                var hat1 = new ParticleSystem.Particle[psLen.main.maxParticles];
                int n1 = psLen.GetParticles(hat1);
                for (int i = 0; i < n1; i++)
                {
                    Vector3 p0;
                    if (!cuHat.TryGetValue(hat1[i].randomSeed, out p0)) continue;
                    Vector3 p1 = hat1[i].position;
                    if (p1.y > p0.y) hatLen++; else hatXuong++;
                    float dg = Mathf.DeltaAngle(Mathf.Atan2(p0.z, p0.x) * Mathf.Rad2Deg, Mathf.Atan2(p1.z, p1.x) * Mathf.Rad2Deg);
                    if (dg > 0f) hatCungChieu++; else hatNguoc++;
                }
            }
            int tongHat = 0;
            foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>()) tongHat += ps.particleCount;

            yield return Chup("gioloc_1_bay");

            Vector3 p0v = loc.transform.position; float t0 = Time.time;
            yield return new WaitForSeconds(1f);
            Vector3 p1v = loc.transform.position; float t1 = Time.time;
            float toc = new Vector2(p1v.x - p0v.x, p1v.z - p0v.z).magnitude / (t1 - t0);

            // TIA SET khi bay: moi nhip 2 tia ten "SetTrongGioLoc"; hinh hoc (chia chieu cao) so voi tia Loc xoay that o C1; bam theo loc
            Vector3 cu = loc.transform.position; float lucDiCuoi = Time.time;
            var tiaNho = new List<SoTia>();
            int soArc = 0, soKhungCoTia = 0, soKhungDung2 = 0;
            var arcTruoc = new HashSet<LightningArc>(Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude));
            int nhip0 = VfxFactory.SoNhipSetTrongGioLoc;
            // DOI CHUNG bo lai phia sau: tia khong bam theo, song 0,28 s
            var tiaDoiChung = LightningArc.Create(loc.transform.position + Vector3.up * 4.5f, loc.transform.position + Vector3.up * 1f, 0.28f, 0.28f);
            tiaDoiChung.name = "TAM_TiaDoiChung";
            arcTruoc.Add(tiaDoiChung);
            float lechDoiChungMax = 0f; int soKhungDoLech = 0;
            var lechBanDau = new Dictionary<LightningArc, Vector3>();
            float troiMax = 0f, tuoiTroiMax = 0f; var lucThay = new Dictionary<LightningArc, float>();
            // Do o CUOI khung (sau LateUpdate - tia da bam theo loc buoc moi), xem ghi chu 26/09/2026
            yield return new WaitForEndOfFrame();
            float hanS = Time.time + 3.6f;
            var loeNho = new Dictionary<DiTheo, Vector4>(); var rongLoeNho = new List<float>();
            // VET SAU LUNG: chup mot lan o giay 2,6 sau khi tung (truoc khi tan o 4,5): vi tri THE GIOI tung hat so voi loc - sau lung bao xa,
            // lech ngang, cao, tuoi lon nhat
            bool daDoVet = false; int soVetBui = 0, soVetKhoi = 0; float sauMax = 0f, sauMin = 99f, ngangMax = 0f, caoVetMax = 0f, tuoiMax = 0f;
            var dsSau = new List<float>();
            while (Time.time < hanS && loc != null)
            {
                if (!daDoVet && Time.time - lucSinh > 2.6f)
                {
                    daDoVet = true;
                    Vector3 hv = loc.dir; hv.y = 0f; hv.Normalize();
                    Vector3 tam = loc.transform.position;
                    foreach (var ps in loc.GetComponentsInChildren<ParticleSystem>())
                    {
                        if (ps.name != "VetBuiXam" && ps.name != "VetKhoiDen") continue;
                        var hat = new ParticleSystem.Particle[ps.particleCount];
                        int n = ps.GetParticles(hat);
                        if (ps.name == "VetBuiXam") soVetBui += n; else soVetKhoi += n;
                        for (int i = 0; i < n; i++)
                        {
                            Vector3 d = hat[i].position - tam;
                            float doc = Vector3.Dot(new Vector3(d.x, 0f, d.z), hv);
                            float sau = -doc;
                            float ngang = (new Vector3(d.x, 0f, d.z) - hv * doc).magnitude;
                            dsSau.Add(sau); sauMax = Mathf.Max(sauMax, sau); sauMin = Mathf.Min(sauMin, sau);
                            ngangMax = Mathf.Max(ngangMax, ngang); caoVetMax = Mathf.Max(caoVetMax, d.y);
                            tuoiMax = Mathf.Max(tuoiMax, hat[i].startLifetime - hat[i].remainingLifetime);
                        }
                    }
                }
                TheoDoiLoe(loc.transform, loeNho, rongLoeNho);
                Vector3 tl = loc.transform.position;
                int trongKhung = 0;
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (a.name != "SetTrongGioLoc") continue;
                    if (arcTruoc.Add(a)) { soArc++; trongKhung++; tiaNho.Add(DoTia(a, tl, caoNho)); }
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
                if (trongKhung > 0) { soKhungCoTia++; if (trongKhung == 2) soKhungDung2++; }
                if (tiaDoiChung != null)
                {
                    var rc = tiaDoiChung.transform.Find("Core");
                    if (rc != null) { var bc = rc.GetComponent<MeshRenderer>().bounds.center; lechDoiChungMax = Mathf.Max(lechDoiChungMax, new Vector2(bc.x - tl.x, bc.z - tl.z).magnitude); }
                }
                if ((loc.transform.position - cu).sqrMagnitude > 1e-6f) { lucDiCuoi = Time.time; cu = loc.transform.position; }
                yield return new WaitForEndOfFrame();
            }
            float song = lucDiCuoi - lucSinh;
            int soNhip = VfxFactory.SoNhipSetTrongGioLoc - nhip0;
            if (tiaDoiChung != null) Object.Destroy(tiaDoiChung.gameObject);

            // So hinh hoc tia bang TRUNG BINH (chia chieu cao than): ban dau so tung tia voi khoang min-max cua 10 tia Loc xoay -> 7/10
            // (10 mau ngau nhien khong phu het khoang that). Dung sai ~2,5-3 sigma sai so trung binh hai mau ~10-20 tia.
            SoTia tbL = TrungBinh(tiaLon), tbG = TrungBinh(tiaNho);
            int nhanhMin = 99, nhanhMax = 0;
            foreach (var t in tiaLon) { nhanhMin = Mathf.Min(nhanhMin, t.nhanh); nhanhMax = Mathf.Max(nhanhMax, t.nhanh); }
            int nhanhDung = 0;
            foreach (var t in tiaNho) if (t.nhanh >= nhanhMin && t.nhanh <= nhanhMax) nhanhDung++;
            bool hinhTiaDung = Mathf.Abs(tbG.hDau - tbL.hDau) < 0.05f && Mathf.Abs(tbG.hDuoi - tbL.hDuoi) < 0.1f
                               && Mathf.Abs(tbG.rDau - tbL.rDau) < 0.06f && Mathf.Abs(tbG.rDuoi - tbL.rDuoi) < 0.06f && nhanhDung == tiaNho.Count;
            float dayL = tbL.day, dayG = tbG.day;
            Ghi(string.Format("C5. Gio loc bay: cao hinh {0:F2} m, den {1}, tong hat {2}; quay that sau 0,1 s: goc tang {3} lop / giam {4}; truot anh len {5} / sai {6}",
                caoNho, soDen, tongHat, quayTang, quayGiam, truotLen, truotSai));
            Ghi(string.Format("C5. bui chan: {0} hat so sanh duoc - bay len {1} / xuong {2}, quay cung chieu {3} / nguoc {4}; vong phun bui chan ({5} hat thu) lech doc {6:F3}, ban kinh toi da {7:F2} (don vi he hat; Loc xoay that {8:F2})",
                hatLen + hatXuong, hatLen, hatXuong, hatCungChieu, hatNguoc, soSinhMoi, yMaxLucSinh, rMaxLucSinh, rVongLx));
            Ghi(string.Format("C5. tia set (trung binh, chia chieu cao than): Loc xoay that {0} tia - dau cao {1:F3} xa truc {2:F3}, duoi cao {3:F3} xa truc {4:F3}, nhanh {5}-{6}, day {7:F3} | Gio loc {8} tia trong {9} khung ({10} khung dung 2), nhip +{11} - dau {12:F3} / {13:F3}, duoi {14:F3} / {15:F3}, nhanh trong khoang {16}/{8}, day {17:F3} = x{18:F3} (ti le cao x{19:F3})",
                tiaLon.Count, tbL.hDau, tbL.rDau, tbL.hDuoi, tbL.rDuoi, nhanhMin, nhanhMax, dayL,
                soArc, soKhungCoTia, soKhungDung2, soNhip, tbG.hDau, tbG.rDau, tbG.hDuoi, tbG.rDuoi, nhanhDung, dayG, dayL > 0 ? dayG / dayL : 0f, caoNho / caoLon));
            Ghi(string.Format("C5. tia bam theo loc ({0} lan do): troi lon nhat {1:F3} m (o tuoi {2:F3} s, {3} tia); DOI CHUNG tia khong bam theo bi bo lai {4:F2} m",
                soKhungDoLech, troiMax, tuoiTroiMax, lechBanDau.Count, lechDoiChungMax));
            Ghi(string.Format("C5. loe cham dat: Loc xoay that {0} loe do duoc trong 4,5 s (doi chung), Gio loc bay {1} loe (mong 0 - nguoi dung bo hieu ung sang)",
                rongLoeLon.Count, loeNho.Count));
            Kiem(rongLoeLon.Count >= 5, "doi chung: khong bat duoc loe cham dat cua Loc xoay that");
            Kiem(loeNho.Count == 0, "Gio loc van loe sang cho tia set cham dat");
            Ghi(string.Format("C5. vet sau lung (giay 2,6 sau khi tung): bui xam {0} hat, khoi den xam {1} hat; sau lung loc tu {2:F2} den {3:F2} m (trung vi {4:F2}), lech ngang toi da {5:F2} m, cao toi da {6:F2} m, tuoi lon nhat {7:F2} s",
                soVetBui, soVetKhoi, sauMin, sauMax, TrungVi(dsSau), ngangMax, caoVetMax, tuoiMax));
            // ~2 s o 9,5 m/s -> vet dai ~15-21 m; khong hat nao o PHIA TRUOC loc (qua 1,5 m - vong sinh 1,27 m)
            Kiem(soVetBui > 50 && soVetKhoi > 30, "khong co vet bui xam / khoi den xam sau lung loc dang bay");
            Kiem(sauMin > -1.5f && TrungVi(dsSau) > 4f && sauMax > 12f && sauMax < 23f, "vet khong nam SAU LUNG loc / khong dai ~2 giay");
            Kiem(ngangMax < 3.5f && caoVetMax < 3.5f && tuoiMax < 2.35f, "vet loe rong / bay cao / song lau qua");
            Ghi(string.Format("D. toc do do {0:F2} m/s; ngung di sau {1:F2} s", toc, song));
            Kiem(soDen == 0, "Gio loc bay con den chop");
            Kiem(quayTang == sps.Length && quayGiam == 0 && sps.Length == 3, "3 nhom dai gio khong xoay cung mot chieu");
            Kiem(truotSai == 0 && truotLen == 3, "anh gio truot khong cung chieu di len (3 nhom dai)");
            Kiem(hatLen + hatXuong > 10 && hatXuong == 0, "bui khong bay len");
            Kiem(hatCungChieu > (hatCungChieu + hatNguoc) * 0.9f, "bui khong cuon cung chieu than loc");
            Kiem(soSinhMoi >= 150 && yMaxLucSinh < 0.02f && Mathf.Abs(rMaxLucSinh - rVongLx) < 0.1f, "vong phun bui chan khong nam ngang / khong bang vong Loc xoay that");
            Kiem(tiaLon.Count >= 16, "doi chung: do duoc qua it tia Loc xoay that");
            Kiem(soNhip >= 5 && soArc == soNhip * 2 && soKhungDung2 == soKhungCoTia, "tia set khong phai moi nhip dung 2 tia");
            Kiem(soArc > 0 && hinhTiaDung, "hinh tia set (dau, duoi, xa truc, so nhanh - chia chieu cao) khong nhu tia Loc xoay that");
            Kiem(dayL > 0f && Mathf.Abs((dayG / dayL) / (caoNho / caoLon) - 1f) < 0.15f, "be day tia set khong thu nho theo co loc (so voi tia that cua Loc xoay)");
            Kiem(lechDoiChungMax > 1f, "doi chung: tia khong bam theo ma khong bi bo lai - phep do vo nghia");
            Kiem(soKhungDoLech > 20 && troiMax < 0.05f, "tia set bi bo lai phia sau con loc");
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
            int soArc = 0, soTiaKhac = 0, tiaSangBia = 0, soChaySem = 0, soChop = 0;
            float hanL = Time.time + 3.0f;
            bool daChupL = false;
            while (Time.time < hanL && locL != null)
            {
                foreach (var a in Object.FindObjectsByType<LightningArc>(FindObjectsInactive.Exclude))
                {
                    if (!arcCu.Add(a)) continue;
                    soArc++;
                    // 01/10/2026: tia THAN loc (Loc xoay thu nho) giang doc MAT NGOAI than, xa truc toi 1,12 x vo - co lan duoi tia tinh co
                    // nam canh bia (do ra "1 tia sang bia"). Tia than loc mang ten SetTrongGioLoc -> chi xet tia KHAC ten
                    if (a.name == "SetTrongGioLoc") continue;
                    soTiaKhac++;
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
                    // 01/10/2026: loe cham dat cua tia THAN loc (kieu Sam set) la chu dich - no DiTheo con loc; chi dem loe khac
                    if ((g.name.StartsWith("Vfx_SetChamDat") || g.name.StartsWith("LightningImpact")) && g.GetComponent<DiTheo>() == null) soChop++;
                }
                if (!daChupL && soArc >= 2 && Time.time > hanL - 2.2f) { daChupL = true; yield return Chup("gioloc_3_set_trong_loc"); }
                yield return null;
            }
            int matDung75 = 0;
            for (int i = 0; i < 5; i++) if (Mathf.Abs((mauTruocL[i] - cac[i].health) - 75f) < 0.5f) matDung75++;
            Ghi(string.Format("L. 5 bia tren duong: tia set moi {0}, khong phai tia than loc {5}; tia tu than loc sang bia {1}; cho chay sem moi {2}; chop sang Sam set moi {3}; bia mat dung 75 {4}/5",
                soArc, tiaSangBia, soChaySem, soChop, matDung75, soTiaKhac));
            Kiem(soTiaKhac == 0 && tiaSangBia == 0 && soChop == 0 && soChaySem == 0, "van con hieu ung set / chop / chay sem khi loc trung doi thu");
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
            // 04/10/2026 (cao 3 m, 0,7 s, NGA NGUA tren khong): do cao = do cao DIEM HONG cua hinh (diem xoay, 0,9 m tren chan) so voi
            // luc dung - doc tu tu the that cua hinh (TransformPoint), khong doc bien CaoHienTai; nga = goc giua truc len cua hinh va
            // cua goc, "ngua" = dau nga ve SAU (truc len cua hinh nghieng nguoc huong mat goc); roi xuong phai dung thang lai
            float ngaMax = 0f; int mauNga = 0, mauNguaSau = 0, soDungLai = 0;
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
                        if (hinh != null)
                        {
                            float hong = hinh.TransformPoint(new Vector3(0f, BiHatTung.TamXoayNgua / 2f, 0f)).y - (d.transform.position.y + 1f + BiHatTung.TamXoayNgua);
                            caoMax = Mathf.Max(caoMax, hong);
                            float nga = Vector3.Angle(hinh.up, d.transform.up);
                            ngaMax = Mathf.Max(ngaMax, nga);
                            if (nga > 30f) { mauNga++; if (Vector3.Dot(hinh.up, d.transform.forward) < 0f) mauNguaSau++; }
                        }
                        if (h != null && !dangCo.Contains(d))
                        {
                            dangCo.Add(d); batDau[d] = Time.time; soHat++;
                            if (!daChupHat && d == hang[1]) { daChupHat = true; }
                        }
                        if (h == null && batDau.ContainsKey(d))
                        {
                            tongGiay += Time.time - batDau[d]; soDoGiay++;
                            if (hinh != null && Vector3.Angle(hinh.up, d.transform.up) < 1f && Mathf.Abs(hinh.localPosition.y - 1f) < 0.01f) soDungLai++;
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
            Kiem(Mathf.Abs(tile - GioLoc.XacSuatHatTung) < 0.08f, "ti le hat tung khong quanh GioLoc.XacSuatHatTung");
            Kiem(soHat == GioLoc.SoLanHat - hat0, "dem doc lap khac bo dem trong code");
            Ghi(string.Format("G. tu the tren khong: nga lon nhat {0:F0} do; {1} mau nga > 30 do, trong do dau nga ve SAU {2}; roi xuong dung thang lai {3}/{4}",
                ngaMax, mauNga, mauNguaSau, soDungLai, soDoGiay));
            Kiem(Mathf.Abs(caoMax - 3f) < 0.15f, "do cao hat tung khong phai 3 m (nguoi dung 04/10/2026)");
            // 09/10/2026 nguoi dung: hat tung 20% (190 lan trung -> ~38 lan bay; truoc 80% ~150), bay 0,8 giay (truoc 0,7)
            Kiem(soDoGiay > 20 && Mathf.Abs(tongGiay / soDoGiay - 0.8f) < 0.06f, "thoi gian bay khong phai 0,8 giay");
            Kiem(ngaMax > 65f && mauNga > 40 && mauNguaSau == mauNga, "bi hat tung khong nga NGUA ra sau tren khong");
            Kiem(soDungLai == soDoGiay, "roi xuong khong dung thang lai");
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
            string demDoiChung = "";
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
                // >= 3 chu khong == 3 (29/09/2026): tu 26/09 qua trung ke dich sinh them QUA NAY -> co luc dem cung luc ra 4
                if (maxMoi >= 3) nDoiChung++;
                demDoiChung += maxMoi + " ";
                yield return new WaitForSeconds(0.4f);
            }
            Ghi(string.Format("H1. dang niem Qua cau lua bi hat tung: {0}/3 lan khong qua nao bay ra (bo dem ngat {1}); doi chung khong hat: {2}/3 lan ra du 3 qua (so qua cung luc: {3})",
                nNgat, PlayerController.SoLanNgatChieu - soNgatCode0, nDoiChung, demDoiChung));
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
                mauQuai.tiLeDoDon = 0f;     // Bo xuong do don 25% (28/09/2026) - muc nay do NGAT DON, khong do do don (menu 91)
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
                    // Chi dem trong DUNG 0,5 s bi hat (ca luot doi chung): Bo xuong nay danh 0,3 s mot don (28/09/2026) nen cua so
                    // 0,9 s cu dem ca cu MOI sau khi roi xuong -> "5/5 don van trung" gia du bo dem ngat bao du 5.
                    yield return new WaitForSeconds(0.5f);
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
            Ghi(string.Format("I1. goi ky nang so 10 tu nguoi kia -> may minh phat lai {0} loc (mong 2); minh tung -> goi mang kyNang = 10: {1}", maxLoc, goi10));
            Kiem(maxLoc == 2, "may minh khong phat lai Gio loc cua nguoi kia (2 loc)");
            Kiem(goi10, "goi ky nang khong mang so 10");

            // I1b. nguoi kia tung Gio loc CAP 5 (cap di kem goi) -> may minh ra 5 loc hinh quat
            XoaLoc();
            yield return new WaitForSeconds(0.5f);
            int truocMang5 = DemLoc(), maxLoc5 = 0;
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietKyNang(new GoiTin.MotPhep
            { chiSo = 1, kyNang = (byte)K, capKyNang = 5, soThuTu = 950, diemNgam = kia.transform.position + vuongI * 10f })));
            float hanI5 = Time.time + 1f;
            while (Time.time < hanI5) { maxLoc5 = Mathf.Max(maxLoc5, DemLoc() - truocMang5); nhetCo(0); yield return null; }
            Ghi(string.Format("I1b. goi ky nang cap 5 tu nguoi kia -> may minh phat lai {0} loc (mong 3; goi cap 1 o tren ra 2)", maxLoc5));
            Kiem(maxLoc5 == 3, "may minh khong phat lai 3 loc cua nguoi kia cap 5");
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

        // ================= M. HINH QUAT: CAP 4 BA LOC, CAP 5 NAM LOC =================
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
            // cap 4: doi chung 3 loc, nang luong thuong
            toi.mana = toi.maxMana; float mn4 = toi.mana;
            var truoc4 = new HashSet<GioLoc>(Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude));
            toi.CastAt(K, gM + hM * 10f);
            float ton4 = mn4 - toi.mana;
            var bon = new List<GioLoc>();
            float han4 = Time.time + 1.2f;
            while (Time.time < han4 && bon.Count < 2)
            {
                foreach (var l in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!truoc4.Contains(l) && !bon.Contains(l)) bon.Add(l);
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            foreach (var l in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!truoc4.Contains(l) && !bon.Contains(l)) bon.Add(l);
            string goc4 = GocCacLoc(bon, hM);
            XoaLoc();
            // Cho het hoi chieu cua lan tung cap 4 (0,9 s tu 09/10/2026 - truoc 0,4 nen 0,3 s la du)
            float hanHc = Time.time + 2f;
            while (toi.HoiChieuGiay(K) > 0f && Time.time < hanHc) yield return null;
            yield return new WaitForSeconds(0.1f);
            if (CapDo.DiemKyNang <= 0) CapDo.Them(CapDo.CanDeLenCap(CapDo.Cap));
            CapDo.NangCap(K);
            int cap5 = CapDo.CapCuaKyNang(K);
            // bia dung ngay tren duong bay cua loc GIUA, 7 m truoc mat: loc giua trung chac chan; hai loc +-30 do (04/10/2026) di qua
            // cach tam bia 7 x sin30 = 3,5 m > vung trung 2,42 + be ngang bia 0,4 - truot -> dung MOT cu
            var biaM = TaoBia("TAM_BiaGiuaQuat", gM + hM * 7f);
            yield return new WaitForFixedUpdate();
            float mauM = biaM.health;
            toi.mana = toi.maxMana; float mn5 = toi.mana;
            var truoc5 = new HashSet<GioLoc>(Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude));
            toi.CastAt(K, gM + hM * 10f);
            float ton5 = mn5 - toi.mana;
            var nam = new List<GioLoc>();
            float hanM = Time.time + 1.2f;
            while (Time.time < hanM && nam.Count < 3)
            {
                foreach (var l in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!truoc5.Contains(l) && !nam.Contains(l)) nam.Add(l);
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            foreach (var l in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude)) if (!truoc5.Contains(l) && !nam.Contains(l)) nam.Add(l);
            string goc5 = GocCacLoc(nam, hM);
            // Doi chung doc lap: goc tinh tu QUANG DUONG loc that su di trong 0,35 s (khong doc dir) - loc phai that su toe ra
            var viTriDau = new Dictionary<GioLoc, Vector3>();
            foreach (var l in nam) if (l != null) viTriDau[l] = l.transform.position;
            yield return new WaitForSeconds(0.35f);
            string gocViTri = "";
            int dungGocViTri = 0;
            // goc mong doi VIET TAY (30 do tu 04/10/2026), khong doc GioLoc.GocQuat - phep kiem doc lap voi hang dang sua
            var gocMong5 = new List<float> { -30f, 0f, 30f };
            var gocDo = new List<float>();
            foreach (var l in nam)
            {
                if (l == null) continue;
                if (!viTriDau.ContainsKey(l)) continue;
                Vector3 d = l.transform.position - viTriDau[l]; d.y = 0f;
                gocDo.Add(Vector3.SignedAngle(hM, d, Vector3.up));
            }
            gocDo.Sort();
            for (int i = 0; i < gocDo.Count; i++)
            {
                gocViTri += gocDo[i].ToString("F1") + " ";
                if (i < gocMong5.Count && Mathf.Abs(gocDo[i] - gocMong5[i]) < 1.5f) dungGocViTri++;
            }
            yield return Chup("gioloc_4_cap5_nam_loc_quat");
            yield return new WaitForSeconds(1.2f);
            float matM = mauM - biaM.health;
            float moiCu = 75f * Mathf.Pow(1.2f, 4);            // 155,52 moi lan trung o cap 5
            int soCu = Mathf.RoundToInt(matM / moiCu);
            // 09/10/2026 nguoi dung bo ngoai le "cap 5 chi 25" - cap 5 tinh nhu ky nang khac: 40 x 1,1^4 (viet tay, khong doc ham)
            float mongTon4 = 40f * Mathf.Pow(1.1f, 3), mongTon5 = 40f * Mathf.Pow(1.1f, 4);
            Ghi(string.Format("M. cap 4 (doi chung): {0} loc, huong bay {1}(mong -15 15), ton {2:F2} nang luong (mong {3:F2} - nang luong KHONG nhan theo so loc)",
                bon.Count, goc4, ton4, mongTon4));
            Ghi(string.Format("M. cap {0}: {1} loc, huong bay {2}(mong -30 0 30), goc tinh tu quang duong da bay 0,35 s: {3}(khop {4}/3); ton {5:F2} (mong {6:F2} - 40 x 1,1^4)",
                cap5, nam.Count, goc5, gocViTri, dungGocViTri, ton5, mongTon5));
            Ghi(string.Format("M. bia tren duong loc giua mat {0:F1} = {1} cu x {2:F2} (moi loc trung mot lan; mong 1 cu: chi loc giua, hai loc 30 do cach 3,5 m)",
                matM, soCu, moiCu));
            Kiem(bon.Count == 2 && goc4 == "-15 15 " && Mathf.Abs(ton4 - mongTon4) < 0.05f, "cap 4 khong phai 2 loc lech 30 do / nang luong thuong");
            Kiem(cap5 == 5 && nam.Count == 3 && goc5 == "-30 0 30 ", "cap 5 khong ra dung 3 loc hinh quat 30 do");
            Kiem(dungGocViTri == 3, "vi tri loc sau khi bay khong toe dung hinh quat");
            Kiem(Mathf.Abs(ton5 - mongTon5) < 0.05f, "cap 5 khong ton dung 40 x 1,1^4 nang luong");
            // bia co 10 000 000 mau: float o do lon nay chi chinh xac toi 1 don vi, moi cu 155,52 thanh 156
            Kiem(soCu == 1 && Mathf.Abs(matM - soCu * moiCu) < 0.6f * soCu, "bia giua khong bi dung so loc di qua trung (moi loc mot lan)");
            Object.Destroy(biaM.gameObject);
            XoaLoc();
        }

        toi.DaTungPhep -= dem;
        if (dir != null) dir.enabled = true;
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>Goc (do, lam tron) giua huong bay cua tung loc va huong ngam, xep tang dan, cach nhau dau cach.</summary>
    static string GocCacLoc(List<GioLoc> ds, Vector3 huong)
    {
        var g = new List<int>();
        foreach (var l in ds) if (l != null) g.Add(Mathf.RoundToInt(Vector3.SignedAngle(huong, l.dir, Vector3.up)));
        g.Sort();
        string kq = "";
        foreach (int x in g) kq += x + " ";
        return kq;
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
