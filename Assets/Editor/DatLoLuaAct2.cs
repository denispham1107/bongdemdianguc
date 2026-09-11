using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// DAT 10 LO LUA DA (cai lo dung cho man hinh chinh) VAO ACT2.
///
/// Nguoi dung xin (12/09/2026): 10 lo, mot lo ngay chinh giua ban do; moi lo
/// KHONG duoc duoi nuoc, trong nha mo, tren bia mo - chi nam tren mat dat.
///
/// CHINH GIUA BAN DO (0, 0) LA HO NUOC TRUNG TAM. Nen "lo giua" la cho dat kho
/// GAN TAM NHAT ma van dung luat - tim bang cach do dan tu tam ra ngoai.
///
/// Mot cho la HOP LE khi:
///   - Mat dat: tia chieu tu tren xuong o tam va 8 diem quanh chan lo deu cham
///     DIA HINH truoc tien (khong phai mai nha, da, bia), chenh cao duoi chan lo
///     khong qua <see cref="DocToiDa"/>.
///   - Nuoc: khong diem nao trong vong <see cref="BanKinhLo"/> + <see cref="CachNuoc"/>
///     nam duoi mat nuoc (nam trong tam giac luoi nuoc VA dia hinh thap hon mat nuoc
///     - mep luoi nuoc chim vao dat nen chi xet luoi thoi thi bo ho rong ra).
///   - Nha mo: ngoai hop bao nha (tinh ca mai) noi rong <see cref="CachNha"/>.
///   - Bia, da, hang rao: khong va cham nao trong ong tru quanh lo.
///   - Cay: cach than cay <see cref="CachCay"/> (lua + khoi cao ~3 m).
///   - Bui co (khong co va cham): cach hop bao bui co.
///   - Trong dau truong, cach cho nguoi choi xuat phat.
///
/// Chin lo con lai rai DEU tren phan dat hop le: chia vung kieu k-means (Lloyd)
/// voi lo giua co dinh, roi dat moi lo vao diem hop le gan tam vung nhat.
/// (Chon "xa nhat" don gian thi 9 lo deu don ra sat hang rao.)
///
/// Chay lai bao nhieu lan cung ra y het (khong ngau nhien) va xoa lo cu truoc.
/// Lo nam trong nhom goc rieng <see cref="TenNhom"/>, KHONG trong "World" -
/// menu 51 chep World sang man chinh, khong duoc keo lo Act2 theo.
/// </summary>
public static class DatLoLuaAct2
{
    public const string CanhAct2 = "Assets/Scenes/Act2.unity";
    public const string TenNhom = "LoLua_Act2";
    public const int SoLo = 10;

    public const float BanKinhLo = 0.55f;   // lo rong 1,09 m
    public const float CachNuoc = 1.5f;     // mep lo cach bo nuoc
    public const float CachVat = 0.9f;      // mep lo cach bia, da
    public const float CachNha = 2.0f;
    public const float CachCay = 2.5f;
    public const float CachCo = 0.6f;
    // Bia CAO (thap, cot > 1,5 m): cach xa hon. Lan dau lo giua dung sau mot
    // cot thap 1,45 m - nhin tu camera phia nam, cot thap nam thang duoi mieng
    // lo, trong nhu lo DAT TREN bia (anh lolua_1).
    public const float CaoBiaCao = 1.5f;
    public const float CachBiaCao = 2.5f;
    public const float DocToiDa = 0.22f;    // chenh cao mat dat duoi chan lo (m)
    public const float BanKinhDauTruong = 46f;
    public const float CachXuatPhat = 5f;
    const float BuocLuoi = 1f;

    [MenuItem("Diablo 2.5D/54. Dat 10 lo lua vao Act2", false, 143)]
    public static void Chay()
    {
        var hienTai = EditorSceneManager.GetActiveScene();
        if (hienTai.isDirty)
        {
            EditorUtility.DisplayDialog("Dat lo lua", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        string canhCu = hienTai.path;
        var bao = new StringBuilder();
        try { Dat(bao); }
        catch (System.Exception e) { bao.AppendLine("[LOI] " + e); }
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText("PlayTestShots/lolua_act2_dat.txt", bao.ToString());
        Debug.Log("[DatLoLuaAct2]\n" + bao);
        if (!string.IsNullOrEmpty(canhCu) && canhCu != CanhAct2)
            EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Dat(StringBuilder bao)
    {
        var canh = EditorSceneManager.OpenScene(CanhAct2, OpenSceneMode.Single);
        foreach (var g in canh.GetRootGameObjects())
            if (g.name == TenNhom) Object.DestroyImmediate(g);
        Physics.SyncTransforms();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DungManChinh.ThuMucLo + "/LoLuaDa.prefab");
        if (prefab == null) throw new System.Exception("chua co prefab lo da - chay menu 51 truoc");

        var kt = new KiemTraCho();
        var ter = kt.ter;
        Vector3 tamMap = ter.transform.position + ter.terrainData.size * 0.5f;
        tamMap.y = 0f;
        bao.AppendLine("tam ban do (giua dia hinh): " + tamMap.ToString("F2"));
        bao.AppendLine("  o tam: " + (kt.LyDo(tamMap) ?? "hop le"));

        // ---- 1. Lo giua: diem hop le gan tam nhat (do min 0,25 m) ----
        Vector3 giua = Vector3.zero; float kcGiua = float.MaxValue;
        for (float x = -20f; x <= 20f; x += 0.25f)
            for (float z = -20f; z <= 20f; z += 0.25f)
            {
                float d = x * x + z * z;
                if (d >= kcGiua) continue;
                var p = new Vector3(tamMap.x + x, 0f, tamMap.z + z);
                if (kt.LyDo(p) != null) continue;
                giua = p; kcGiua = d;
            }
        if (kcGiua == float.MaxValue) throw new System.Exception("khong tim duoc cho hop le trong 20 m quanh tam");
        bao.AppendLine("lo giua: " + giua.ToString("F2") + ", cach tam " + Mathf.Sqrt(kcGiua).ToString("F2") + " m");

        // ---- 2. Luoi diem hop le ----
        var hopLe = new List<Vector3>();
        int tong = 0;
        var lyDoDem = new Dictionary<string, int>();
        for (float x = -BanKinhDauTruong; x <= BanKinhDauTruong; x += BuocLuoi)
            for (float z = -BanKinhDauTruong; z <= BanKinhDauTruong; z += BuocLuoi)
            {
                var p = new Vector3(tamMap.x + x, 0f, tamMap.z + z);
                if (x * x + z * z > BanKinhDauTruong * BanKinhDauTruong) continue;
                tong++;
                string ly = kt.LyDo(p);
                if (ly == null) { hopLe.Add(p); continue; }
                string loai = ly.Split(':')[0];
                lyDoDem[loai] = (lyDoDem.TryGetValue(loai, out int n) ? n : 0) + 1;
            }
        bao.AppendLine("luoi " + BuocLuoi + " m trong vong " + BanKinhDauTruong + " m: " + tong + " diem, hop le " + hopLe.Count);
        foreach (var kv in lyDoDem) bao.AppendLine("   loai vi " + kv.Key + ": " + kv.Value);

        // ---- 3. Chia vung (Lloyd), lo giua co dinh ----
        var tam = new List<Vector3> { giua };
        // Khoi dau: chon xa nhat - chi de co diem xuat phat on dinh
        while (tam.Count < SoLo)
        {
            Vector3 tot = hopLe[0]; float xaNhat = -1f;
            foreach (var p in hopLe)
            {
                float m = float.MaxValue;
                foreach (var t in tam) m = Mathf.Min(m, (p - t).sqrMagnitude);
                if (m > xaNhat) { xaNhat = m; tot = p; }
            }
            tam.Add(tot);
        }
        var cong = new Vector3[SoLo]; var dem = new int[SoLo];
        for (int lap = 0; lap < 60; lap++)
        {
            System.Array.Clear(cong, 0, SoLo); System.Array.Clear(dem, 0, SoLo);
            foreach (var p in hopLe)
            {
                int k = 0; float m = float.MaxValue;
                for (int i = 0; i < SoLo; i++) { float d = (p - tam[i]).sqrMagnitude; if (d < m) { m = d; k = i; } }
                cong[k] += p; dem[k]++;
            }
            for (int i = 1; i < SoLo; i++) if (dem[i] > 0) tam[i] = cong[i] / dem[i];
        }

        // Dat vao diem hop le gan tam vung nhat
        var cho = new List<Vector3> { giua };
        for (int i = 1; i < SoLo; i++)
        {
            Vector3 tot = tam[i]; float m = float.MaxValue;
            foreach (var p in hopLe)
            {
                float d = (p - tam[i]).sqrMagnitude;
                if (d >= m) continue;
                bool gan = false;
                foreach (var c in cho) if ((p - c).sqrMagnitude < 10f * 10f) gan = true;
                if (gan) continue;
                m = d; tot = p;
            }
            cho.Add(tot);
            bao.AppendLine("vung " + i + ": " + dem[i] + " diem, tam " + tam[i].ToString("F1") + " -> lo " + tot.ToString("F1")
                           + " (lech " + Mathf.Sqrt(m).ToString("F1") + " m)");
        }

        // ---- 4. Dung lo ----
        var nhom = new GameObject(TenNhom);
        float kcNhoNhat = float.MaxValue;
        for (int i = 0; i < cho.Count; i++)
        {
            var p = cho[i];
            p.y = kt.DoCaoChan(p);
            var lo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canh);
            lo.name = i == 0 ? "LoLua_GiuaMap" : "LoLua_" + i.ToString("00");
            lo.transform.SetParent(nhom.transform, false);
            // Do kich thuoc khi CHUA xoay - hop bao sau khi xoay rong them ~30%
            lo.transform.SetPositionAndRotation(p, Quaternion.identity);
            var b = new Bounds(p, Vector3.zero);
            foreach (var r in lo.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            lo.transform.rotation = Quaternion.Euler(0f, (i * 137.5f + 20f) % 360f, 0f);

            // Va cham: nguoi choi va quai khong di xuyen qua lo; GameDirector
            // cung khong tha quai vao cho nay (CheckSphere lop Default).
            // Dat o GOC lo (co LoLuaDa) - con loc thay he hat con cua no nen
            // khong cuon lo di (VatTheBiCuon.CuonDuoc).
            //
            // CON NHONG PHAI CAO HAN HAN 2 x BAN KINH. Lan dau dat cao = cao lo
            // (1,33 m), ban kinh 0,65 m -> con nhong thanh gan nhu QUA CAU, va
            // nhan vat (buoc duoc bac 0,55 m) truot len mat cau roi dung tren
            // dinh lo: 7/10 lo bi di xuyen (menu 54b). Gio la cot dung cao hon
            // lo 1 m (vung ngon lua) - thanh dung tu 0,5 m tro len, qua bac buoc.
            var cap = lo.AddComponent<CapsuleCollider>();
            cap.radius = Mathf.Max(b.extents.x, b.extents.z) * 0.92f;
            cap.height = b.size.y + 1f;
            cap.center = new Vector3(0f, cap.height * 0.5f, 0f);

            for (int j = 0; j < i; j++)
                kcNhoNhat = Mathf.Min(kcNhoNhat, Vector2.Distance(new Vector2(cho[i].x, cho[i].z), new Vector2(cho[j].x, cho[j].z)));
            bao.AppendLine(string.Format("{0}: ({1:F2}, {2:F2}, {3:F2}), cach tam {4:F1} m",
                lo.name, p.x, p.y, p.z, new Vector2(p.x - tamMap.x, p.z - tamMap.z).magnitude));
        }
        bao.AppendLine("khoang cach nho nhat giua hai lo: " + kcNhoNhat.ToString("F1") + " m");

        EditorSceneManager.MarkSceneDirty(canh);
        EditorSceneManager.SaveScene(canh);
        bao.AppendLine("da luu " + CanhAct2 + " - " + cho.Count + " lo trong nhom " + TenNhom);
        kt.Don();
    }

    /// <summary>Luat dat lo - dung chung cho viec dat. Phep thu (menu 54b) do bang cach khac.</summary>
    public class KiemTraCho
    {
        public readonly Terrain ter;
        readonly List<Bounds> nha = new List<Bounds>();
        readonly List<Bounds> co = new List<Bounds>();
        readonly List<Bounds> biaCao = new List<Bounds>();
        readonly List<Collider> cay = new List<Collider>();
        readonly List<(Vector3[] v, int[] t, Bounds b, float y)> nuoc = new List<(Vector3[], int[], Bounds, float)>();
        readonly Vector3 xuatPhat;
        readonly int lopDat;

        public KiemTraCho()
        {
            ter = Object.FindAnyObjectByType<Terrain>();
            lopDat = ter.gameObject.layer;
            var nhaMo = GameObject.Find("NhaMo");
            if (nhaMo != null)
                foreach (Transform m in nhaMo.transform)
                {
                    var b = new Bounds(m.position, Vector3.zero);
                    foreach (var r in m.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
                    b.Expand(new Vector3(CachNha * 2f, 0f, CachNha * 2f));
                    nha.Add(b);
                }
            var buiCo = GameObject.Find("BuiCoRai");
            if (buiCo != null)
                foreach (var r in buiCo.GetComponentsInChildren<Renderer>()) co.Add(r.bounds);
            foreach (var ten in new[] { "BiaMo", "Da" })
            {
                var nhomBia = GameObject.Find(ten);
                if (nhomBia == null) continue;
                foreach (Transform c in nhomBia.transform)
                {
                    var rs = c.GetComponentsInChildren<Renderer>();
                    if (rs.Length == 0) continue;
                    var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                    if (b.size.y > CaoBiaCao) biaCao.Add(b);
                }
            }
            var nhomCay = GameObject.Find("Cay");
            if (nhomCay != null) cay.AddRange(nhomCay.GetComponentsInChildren<Collider>());
            var matNuoc = GameObject.Find("MatNuoc");
            if (matNuoc != null)
                foreach (var mf in matNuoc.GetComponentsInChildren<MeshFilter>())
                {
                    var m = mf.sharedMesh;
                    var v = m.vertices;
                    for (int i = 0; i < v.Length; i++) v[i] = mf.transform.TransformPoint(v[i]);
                    var r = mf.GetComponent<Renderer>();
                    var b = r.bounds; b.Expand(new Vector3(2f, 0f, 2f));
                    nuoc.Add((v, m.triangles, b, r.bounds.max.y));
                }
            var nv = GameObject.Find("Player");
            xuatPhat = nv != null ? nv.transform.position : Vector3.zero;
        }

        public void Don() { }

        /// <summary>Do cao dat thap nhat duoi chan lo - chan lo khong lo lung.</summary>
        public float DoCaoChan(Vector3 p)
        {
            float m = float.MaxValue;
            foreach (var d in Vong(p, BanKinhLo)) m = Mathf.Min(m, DatTai(d));
            return m;
        }

        float DatTai(Vector3 p)
        {
            return ter.SampleHeight(new Vector3(p.x, 0f, p.z)) + ter.transform.position.y;
        }

        static IEnumerable<Vector3> Vong(Vector3 p, float r)
        {
            yield return p;
            for (int i = 0; i < 8; i++)
            {
                float g = i * Mathf.PI / 4f;
                yield return p + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
            }
        }

        /// <summary>null = hop le; khong thi "loai: chi tiet".</summary>
        public string LyDo(Vector3 p)
        {
            if (new Vector2(p.x - xuatPhat.x, p.z - xuatPhat.z).magnitude < CachXuatPhat) return "cho xuat phat";

            foreach (var b in nha)
                if (p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z) return "nha mo";

            // Nuoc: tam, vong chan lo va vong cach bo
            foreach (var w in nuoc)
            {
                if (p.x < w.b.min.x - CachNuoc - 1f || p.x > w.b.max.x + CachNuoc + 1f
                    || p.z < w.b.min.z - CachNuoc - 1f || p.z > w.b.max.z + CachNuoc + 1f) continue;
                foreach (float r in new[] { 0f, BanKinhLo, BanKinhLo + CachNuoc * 0.5f, BanKinhLo + CachNuoc })
                    foreach (var d in Vong(p, r))
                        if (DatTai(d) < w.y + 0.02f && TrongLuoi(w.v, w.t, d)) return "nuoc: " + d.ToString("F1");
            }

            // Mat dat: tia tu tren xuong phai cham dia hinh truoc
            float thap = float.MaxValue, cao = float.MinValue;
            foreach (var d in Vong(p, BanKinhLo))
            {
                if (!Physics.Raycast(d + Vector3.up * 60f, Vector3.down, out var hit, 120f, ~0, QueryTriggerInteraction.Ignore))
                    return "khong co dat";
                if (!(hit.collider is TerrainCollider)) return "khong phai mat dat: " + hit.collider.name;
                thap = Mathf.Min(thap, hit.point.y); cao = Mathf.Max(cao, hit.point.y);
            }
            if (cao - thap > DocToiDa) return "doc";

            float y = thap;
            foreach (var c in Physics.OverlapCapsule(new Vector3(p.x, y + 0.15f, p.z), new Vector3(p.x, y + 2.4f, p.z),
                                                     BanKinhLo + CachVat, ~(1 << lopDat), QueryTriggerInteraction.Ignore))
            {
                if (c is TerrainCollider) continue;
                if (c.GetComponentInParent<CharacterController>() != null) continue;   // nhan vat trong canh
                return "vat can: " + c.name;
            }
            foreach (var c in cay)
            {
                var gan = c.ClosestPointOnBounds(new Vector3(p.x, c.bounds.center.y, p.z));
                if (new Vector2(gan.x - p.x, gan.z - p.z).magnitude < BanKinhLo + CachCay) return "cay: " + c.name;
            }
            foreach (var b in biaCao)
            {
                float dx = Mathf.Max(0f, Mathf.Max(b.min.x - p.x, p.x - b.max.x));
                float dz = Mathf.Max(0f, Mathf.Max(b.min.z - p.z, p.z - b.max.z));
                if (dx * dx + dz * dz < (BanKinhLo + CachBiaCao) * (BanKinhLo + CachBiaCao)) return "bia cao";
            }
            foreach (var b in co)
            {
                float dx = Mathf.Max(0f, Mathf.Max(b.min.x - p.x, p.x - b.max.x));
                float dz = Mathf.Max(0f, Mathf.Max(b.min.z - p.z, p.z - b.max.z));
                if (dx * dx + dz * dz < (BanKinhLo + CachCo) * (BanKinhLo + CachCo)) return "bui co";
            }
            return null;
        }

        static bool TrongLuoi(Vector3[] v, int[] t, Vector3 p)
        {
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = v[t[i]], b = v[t[i + 1]], c = v[t[i + 2]];
                float d1 = (p.x - b.x) * (a.z - b.z) - (a.x - b.x) * (p.z - b.z);
                float d2 = (p.x - c.x) * (b.z - c.z) - (b.x - c.x) * (p.z - c.z);
                float d3 = (p.x - a.x) * (c.z - a.z) - (c.x - a.x) * (p.z - a.z);
                bool am = d1 < 0 || d2 < 0 || d3 < 0, duong = d1 > 0 || d2 > 0 || d3 > 0;
                if (!(am && duong)) return true;
            }
            return false;
        }
    }
}
