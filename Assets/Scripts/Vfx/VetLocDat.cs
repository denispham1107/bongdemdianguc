using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DAU VET LOC TREN MAT DAT (nguoi dung 29/09/2026: "moi khi gio loc cua Loc xoay va Gio loc di qua deu de lai dau vet tren mat dat";
/// chon "vet chay xem + cay dat", song 5 giay, anh dung bang Blender MCP - CongCu/Blender/vet_loc_dat.blend).
///
/// Hai kieu, cung mot component:
///   - DAI CAY DAT: con loc di duoc mot buoc (1/5 be rong) thi them mot LAT CAT NGANG 5 diem; moi diem do tia xuong CHI lop Ground
///     (+4 cm) nen dai bam dung dia hinh go ghe (tam phang thi dat go ghe che mat nua dai - bai hoc vong phep Tang hinh). Anh
///     KyNang/VetLoc/VetCayDat lap lien mach theo chieu doc (Blender: nhieu tren hinh tru), v = quang duong / (2 x be rong).
///   - VET CHAY XEM: moi nhip set trong long loc -> mot luoi 5x5 bam dat, 1 trong 4 o anh KyNang/VetLoc/ChayXem (2x2), xoay ngau nhien.
/// Vat lieu Diablo25D/VetDatNhan (NHAN MAU - dat toi di, dem hay ngay deu dung). Moi lat / moi vet song SongGiay: hien 0,2 s, giu,
/// mo dan GiayMo cuoi. Loc tan thi dai van mo not roi tu xoa. Chi hinh, khong qua goi tin (loc phat lai tren moi may).
/// </summary>
public class VetLocDat : MonoBehaviour
{
    public const float SongGiay = 5f, GiayMo = 1.5f, GiayHien = 0.2f;
    /// <summary>Be rong dai cay (m): Gio loc ~ chan vo ngoai, Loc xoay ~ chan vo Vo3 (2,94 m) x 2 (29/09/2026).</summary>
    public const float RongVetGioLoc = 2.4f, RongVetLocXoay = 5.6f;
    /// <summary>Tran so vet chay cung luc (ca ban do) - 5 Gio loc x 11 vet / 5 s.</summary>
    public const int ToiDaVetChay = 70;
    const int SoNgang = 5;
    const float NangKhoiDat = 0.04f;

    public static readonly List<VetLocDat> DangSong = new List<VetLocDat>();
    static int soVetChay;
    static int matDat = -1;
    static Material mVet, mChay;

    struct Lat { public Vector3[] p; public float v; public float luc; }
    readonly List<Lat> lat = new List<Lat>();

    Transform chu;
    float rongGoc, buoc, quangDuong;
    System.Func<float> heSoRong;
    Vector3 viTriCuoi;
    bool coCuoi, laChay;
    Mesh mesh;
    Color[] mauCache;

    // ---- doc cho phep thu ----
    public bool LaChay { get { return laChay; } }
    public Transform Chu { get { return chu; } }
    public int SoLat { get { return lat.Count; } }
    public float QuangDuong { get { return quangDuong; } }
    public float RongHienTai { get { return rongGoc * (heSoRong != null ? heSoRong() : 1f); } }
    public float TuoiLonNhat { get { return lat.Count > 0 ? Time.time - lat[0].luc : 0f; } }
    /// <summary>Cac diem dinh (the gioi) cua moi lat - phep thu do bam dat / be rong.</summary>
    public Vector3[] DiemLat(int i) { return lat[i].p; }

    static int MatDat { get { if (matDat < 0) matDat = LayerMask.GetMask("Ground"); return matDat; } }

    // Kiem bang null cua Unity (vat lieu nap lai sau khi thoat Play)
    static Material VatLieu(ref Material m, string ten)
    {
        if (m == null) m = Resources.Load<Material>("KyNang/VetLoc/" + ten);
        return m;
    }

    static Vector3 BamDat(Vector3 p, float yDuPhong)
    {
        RaycastHit h;
        if (Physics.Raycast(p + Vector3.up * 6f, Vector3.down, out h, 20f, MatDat, QueryTriggerInteraction.Ignore))
            return new Vector3(p.x, h.point.y + NangKhoiDat, p.z);
        return new Vector3(p.x, yDuPhong + NangKhoiDat, p.z);
    }

    VetLocDat TaoMesh(string ten, Material mat)
    {
        mesh = new Mesh { name = ten };
        mesh.MarkDynamic();
        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        return this;
    }

    /// <summary>Gan dai cay dat theo con loc <paramref name="chuLoc"/> (goc loc o mat dat). heSoRong: vd Hoa loc xoay phinh hinh.</summary>
    public static VetLocDat Gan(Transform chuLoc, float rong, System.Func<float> heSoRong = null)
    {
        var mat = VatLieu(ref mVet, "VetCayDat");
        if (chuLoc == null || mat == null) return null;
        var go = new GameObject("VetLoc");
        var v = go.AddComponent<VetLocDat>();
        v.chu = chuLoc; v.rongGoc = rong; v.heSoRong = heSoRong; v.buoc = Mathf.Max(0.3f, rong * 0.2f);
        return v.TaoMesh("VetLoc", mat);
    }

    /// <summary>Mot vet chay xem ngau nhien trong ban kinh quanh chan loc (moi nhip set trong long loc).</summary>
    public static void ChayXem(Vector3 chanLoc, float banKinh, float coMin, float coMax)
    {
        var mat = VatLieu(ref mChay, "ChayXem");
        if (mat == null || soVetChay >= ToiDaVetChay) return;
        var go = new GameObject("ChayXem");
        var v = go.AddComponent<VetLocDat>();
        v.laChay = true;
        v.TaoMesh("ChayXem", mat);
        Vector2 r = Random.insideUnitCircle * banKinh;
        Vector3 c = chanLoc + new Vector3(r.x, 0f, r.y);
        float co = Random.Range(coMin, coMax), goc = Random.Range(0f, 360f);
        int o = Random.Range(0, 4);
        v.DungLuoiChay(c, co, goc, (o % 2) * 0.5f, (o / 2) * 0.5f);
        soVetChay++;
    }

    void DungLuoiChay(Vector3 c, float co, float goc, float u0, float v0)
    {
        const int n = 5;
        var q = Quaternion.Euler(0f, goc, 0f);
        var p = new Vector3[n * n];
        var uv = new Vector2[n * n];
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                float a = i / (n - 1f), b = j / (n - 1f);
                p[j * n + i] = BamDat(c + q * new Vector3((a - 0.5f) * co, 0f, (b - 0.5f) * co), c.y);
                uv[j * n + i] = new Vector2(u0 + a * 0.5f, v0 + b * 0.5f);
            }
        var tri = new List<int>();
        for (int j = 0; j < n - 1; j++)
            for (int i = 0; i < n - 1; i++)
            {
                int k = j * n + i;
                tri.Add(k); tri.Add(k + n); tri.Add(k + 1);
                tri.Add(k + 1); tri.Add(k + n); tri.Add(k + n + 1);
            }
        mesh.vertices = p; mesh.uv = uv; mesh.SetTriangles(tri, 0);
        mauCache = new Color[p.Length];
        lat.Add(new Lat { p = p, v = 0f, luc = Time.time });
        mesh.RecalculateBounds();
        CapNhatMau();
    }

    void OnEnable() { DangSong.Add(this); }
    void OnDisable() { DangSong.Remove(this); }
    void OnDestroy() { if (laChay) soVetChay = Mathf.Max(0, soVetChay - 1); if (mesh != null) Destroy(mesh); }

    static float Alpha(float tuoi)
    {
        return Mathf.Clamp01(tuoi / GiayHien) * Mathf.Clamp01((SongGiay - tuoi) / GiayMo);
    }

    void LateUpdate()
    {
        if (laChay)
        {
            if (Time.time - lat[0].luc >= SongGiay) { Destroy(gameObject); return; }
            CapNhatMau();
            return;
        }

        bool doi = false;
        if (chu != null)
        {
            Vector3 p = chu.position;
            if (!coCuoi) { viTriCuoi = p; coCuoi = true; }
            Vector3 d = p - viTriCuoi; d.y = 0f;
            float dd = d.magnitude;
            if (dd > 8f) { viTriCuoi = p; }                     // nhay cho (luc sinh / dich chuyen) - khong keo dai gia
            else if (dd >= buoc)
            {
                Vector3 huong = d / dd;
                if (lat.Count == 0) ThemLat(viTriCuoi, huong);
                quangDuong += dd;
                ThemLat(p, huong);
                viTriCuoi = p;
                doi = true;
            }
        }
        while (lat.Count > 0 && Time.time - lat[0].luc >= SongGiay) { lat.RemoveAt(0); doi = true; }
        if (lat.Count == 0)
        {
            if (chu == null) Destroy(gameObject);
            else if (mesh.vertexCount > 0) mesh.Clear();
            return;
        }
        if (doi) DungLaiLuoi();
        CapNhatMau();
    }

    void ThemLat(Vector3 tam, Vector3 huong)
    {
        Vector3 phai = Vector3.Cross(Vector3.up, huong).normalized;
        float w = RongHienTai;
        var p = new Vector3[SoNgang];
        for (int k = 0; k < SoNgang; k++)
            p[k] = BamDat(tam + phai * ((k / (SoNgang - 1f) - 0.5f) * w), tam.y);
        lat.Add(new Lat { p = p, v = quangDuong / (2f * rongGoc), luc = Time.time });
    }

    void DungLaiLuoi()
    {
        int n = lat.Count;
        mesh.Clear();
        if (n < 2) return;
        var v = new Vector3[n * SoNgang];
        var uv = new Vector2[n * SoNgang];
        for (int i = 0; i < n; i++)
            for (int k = 0; k < SoNgang; k++)
            {
                v[i * SoNgang + k] = lat[i].p[k];
                uv[i * SoNgang + k] = new Vector2(k / (SoNgang - 1f), lat[i].v);
            }
        var tri = new int[(n - 1) * (SoNgang - 1) * 6];
        int t = 0;
        for (int i = 0; i < n - 1; i++)
            for (int k = 0; k < SoNgang - 1; k++)
            {
                int a = i * SoNgang + k;
                tri[t++] = a; tri[t++] = a + SoNgang; tri[t++] = a + 1;
                tri[t++] = a + 1; tri[t++] = a + SoNgang; tri[t++] = a + SoNgang + 1;
            }
        mesh.vertices = v; mesh.uv = uv; mesh.triangles = tri;
        mesh.RecalculateBounds();
        mauCache = new Color[v.Length];
    }

    void CapNhatMau()
    {
        if (mesh == null || mauCache == null || mauCache.Length != mesh.vertexCount || mesh.vertexCount == 0) return;
        float now = Time.time;
        if (laChay)
        {
            var c = new Color(1f, 1f, 1f, Alpha(now - lat[0].luc));
            for (int i = 0; i < mauCache.Length; i++) mauCache[i] = c;
        }
        else
        {
            for (int i = 0; i < lat.Count; i++)
            {
                var c = new Color(1f, 1f, 1f, Alpha(now - lat[i].luc));
                for (int k = 0; k < SoNgang; k++) mauCache[i * SoNgang + k] = c;
            }
        }
        mesh.colors = mauCache;
    }
}
