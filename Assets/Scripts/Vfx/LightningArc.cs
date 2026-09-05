using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MOT TIA SET.
///
/// Duong di gap khuc duoc tinh bang code roi bien thanh hinh khoi (mesh):
/// MOT dai vai luon XOAY MAT VE MAY QUAY (billboard), nen nhin tu goc nao cung
/// chi thay be ngang cua tia.
///
/// Truoc day dung HAI dai cat cheo nhau hinh chu X. Nghe thi hop ly - "nhin goc
/// nao cung day dan" - nhung thuc te bao gio cung co MOT dai gan nhu vuong goc
/// voi huong nhin, va no hien ra nguyen mot MANG CHU NHAT MO. Nguoi choi mo ta
/// dung: "mot lop hinh mong nhu dang ghep tia set vao". Dai cang rong thi mang
/// do cang to.
///
/// Cu vai phan tram giay lai giat lai duong di mot lan -> tia set "nhay" nhu that,
/// roi mo dan trong khoang 0.25 giay va tu xoa.
/// </summary>
public class LightningArc : MonoBehaviour
{
    [Header("Duong di (toa do the gioi)")]
    public Vector3 start;
    public Vector3 end;
    public int segments = 16;
    public float jitter = 1f;

    [Header("Do day")]
    // GIU NGUYEN so cu. Bo dai chu X khong lam tia hep di: dai thu hai chi
    // chong them mot lop sang o giua chu khong noi rong ra hai ben. Sam set,
    // Loc va tia choang van nhu truoc.
    //
    // Rieng Giut set phong TU TAY nguoi choi nen phai hep hon nhieu - xem
    // GiatSet.VeTia.
    public float coreWidth = 0.22f;
    public float glowWidth = 1.05f;

    [Header("Nhanh re ra hai ben")]
    public int branches = 2;
    public float branchLength = 0.34f;

    [Header("Mau")]
    public Color coreColor = new Color(1f, 1f, 1f, 1f);
    public Color glowColor = new Color(0.45f, 0.70f, 1f, 1f);

    [Header("Thoi gian")]
    public float lifetime = 0.26f;
    public float flickerInterval = 0.045f;

    Mesh coreMesh, glowMesh;
    Material coreMat, glowMat;
    float age, flickerTimer;

    // Bo nho dung chung khi dung hinh (khoi phai cap phat lai moi khung hinh)
    static readonly List<Vector3> verts = new List<Vector3>();
    static readonly List<Vector2> uvs = new List<Vector2>();
    static readonly List<Color> cols = new List<Color>();
    static readonly List<int> tris = new List<int>();

    /// <summary>Tao mot tia set noi hai diem.</summary>
    public static LightningArc Create(Vector3 start, Vector3 end, float widthScale, float life)
    {
        var go = new GameObject("LightningArc");
        go.transform.position = start;

        var arc = go.AddComponent<LightningArc>();
        arc.start = start;
        arc.end = end;
        arc.coreWidth *= widthScale;
        arc.glowWidth *= widthScale;
        arc.lifetime = life;
        return arc;
    }

    void Start()
    {
        coreMesh = NewMesh("BoltCore");
        glowMesh = NewMesh("BoltGlow");

        coreMat = new Material(VfxFactory.BoltCoreMat);
        glowMat = new Material(VfxFactory.BoltGlowMat);

        // Lop sang mo o ngoai ve TRUOC, loi trang ve SAU -> loi noi len tren
        MakeRenderer("Glow", glowMesh, glowMat);
        MakeRenderer("Core", coreMesh, coreMat);

        // PHAI to mau NGAY O DAY, khong duoc doi Update.
        //
        // Hai vat lieu goc BoltCoreMat/BoltGlowMat mang mau XANH DUONG cua tia
        // set nguoi choi. Update moi la cho goi SetFade, ma Update chay SAU
        // Start mot khung hinh - va giua hai cai do co MOT LAN VE. Nen tia cua
        // Quy cay loe len xanh duong dung mot khung roi moi doi sang xanh la.
        //
        // Mot khung tren 18 khung (0,3 giay) chi la 5%, nhung no roi dung vao
        // luc tia SANG NHAT nen nhin ra rat ro.
        SetFade(coreMat, coreColor, 1f);
        SetFade(glowMat, glowColor, 0.9f);

        Rebuild();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;
        flickerTimer -= dt;

        if (flickerTimer <= 0f)
        {
            flickerTimer = flickerInterval;
            Rebuild();
        }

        // Sang nhat luc vua danh xuong roi tat dan
        float k = Mathf.Clamp01(age / Mathf.Max(0.01f, lifetime));
        float fade = 1f - k * k;

        SetFade(coreMat, coreColor, fade);
        SetFade(glowMat, glowColor, fade * 0.9f);

        if (age >= lifetime) Destroy(gameObject);
    }

    static void SetFade(Material m, Color c, float fade)
    {
        if (m == null) return;
        c.a = fade;
        m.SetColor("_TintColor", c);
    }

    static Mesh NewMesh(string name)
    {
        var m = new Mesh();
        m.name = name;
        m.MarkDynamic();
        return m;
    }

    void MakeRenderer(string name, Mesh mesh, Material mat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }

    // ================================================================
    //  DUNG HINH TIA SET
    // ================================================================

    void Rebuild()
    {
        var path = MakePath(start, end, segments, jitter);
        BuildInto(coreMesh, path, coreWidth);
        BuildInto(glowMesh, path, glowWidth);
    }

    void BuildInto(Mesh mesh, Vector3[] path, float width)
    {
        if (mesh == null) return;

        verts.Clear(); uvs.Clear(); cols.Clear(); tris.Clear();

        AppendRibbon(path, width);

        // Vai nhanh nho re ra tu than chinh
        for (int i = 0; i < branches; i++)
        {
            var b = MakeBranch(path);
            if (b == null) continue;
            AppendRibbon(b, width * 0.55f);
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
    }

    /// <summary>Duong gap khuc noi hai diem: hai dau ghim chat, doan giua lech nhieu nhat.</summary>
    static Vector3[] MakePath(Vector3 a, Vector3 b, int seg, float jitter)
    {
        if (seg < 2) seg = 2;
        var pts = new Vector3[seg + 1];

        Vector3 axis = b - a;
        float len = axis.magnitude;
        Vector3 dir = len > 0.001f ? axis / len : Vector3.down;

        Vector3 side, up;
        Frame(dir, out side, out up);

        for (int i = 0; i <= seg; i++)
        {
            float k = i / (float)seg;
            float amp = jitter * len * 0.055f * Mathf.Sin(k * Mathf.PI);
            pts[i] = a + axis * k
                   + side * Random.Range(-amp, amp)
                   + up * Random.Range(-amp, amp);
        }

        pts[0] = a;
        pts[seg] = b;
        return pts;
    }

    /// <summary>Mot nhanh nho tach ra tu giua than tia set.</summary>
    Vector3[] MakeBranch(Vector3[] path)
    {
        if (path.Length < 6) return null;

        int i = Random.Range(path.Length / 5, path.Length - 3);
        Vector3 from = path[i];

        Vector3 dir = end - start;
        if (dir.sqrMagnitude < 1e-6f) return null;
        dir.Normalize();

        Vector3 side, up;
        Frame(dir, out side, out up);

        float ang = Random.Range(0f, Mathf.PI * 2f);
        Vector3 outward = (side * Mathf.Cos(ang) + up * Mathf.Sin(ang)).normalized;

        float rest = Vector3.Distance(from, end);
        Vector3 to = from + (outward * 0.8f + dir * 0.6f).normalized * rest * branchLength;

        return MakePath(from, to, 5, jitter * 1.3f);
    }

    /// <summary>
    /// Cho dung cua may quay. Cache lai: <c>Camera.main</c> quet ca canh tim
    /// theo tag, ma ham nay bi goi cho TUNG DIEM tren TUNG tia.
    /// </summary>
    static Camera mayQuay;
    static Vector3 ChoMayQuay()
    {
        if (mayQuay == null) mayQuay = Camera.main;
        if (mayQuay == null) return Vector3.up * 1000f;
        return mayQuay.transform.position;
    }

    /// <summary>Hai truc vuong goc voi huong di - de trai mieng vai hai ben.</summary>
    static void Frame(Vector3 dir, out Vector3 side, out Vector3 up)
    {
        Vector3 helper = Mathf.Abs(dir.y) > 0.9f ? Vector3.forward : Vector3.up;
        side = Vector3.Cross(dir, helper).normalized;
        up = Vector3.Cross(dir, side).normalized;
    }

    /// <summary>
    /// Trai MOT dai vai doc theo duong di, mat dai luon quay ve may quay.
    ///
    /// Be ngang VUOT NHON o ca hai dau. Truoc day dai cat phang o dau: chinh cai
    /// canh cat rong bang ca be ngang tia, dat ngay tren tay nhan vat, la thu
    /// nhin ra "mieng vai dan vao tay". Vuot ve 0 thi tia MOC RA tu tay.
    /// </summary>
    void AppendRibbon(Vector3[] pts, float width)
    {
        int n = pts.Length;
        if (n < 2) return;

        int baseIndex = verts.Count;
        Transform space = transform;
        Vector3 mat = ChoMayQuay();

        for (int i = 0; i < n; i++)
        {
            Vector3 t = i == 0 ? pts[1] - pts[0]
                      : i == n - 1 ? pts[n - 1] - pts[n - 2]
                      : pts[i + 1] - pts[i - 1];
            if (t.sqrMagnitude < 1e-6f) t = Vector3.down;
            t.Normalize();

            // Be ngang cua dai vuong goc VOI CA huong di lan huong nhin -> mat
            // dai luon huong thang ve may quay.
            Vector3 s = Vector3.Cross(t, mat - pts[i]);
            if (s.sqrMagnitude < 1e-8f)
            {
                Vector3 tam1, tam2;
                Frame(t, out tam1, out tam2);
                s = tam1;
            }
            s.Normalize();

            float k = i / (float)(n - 1);

            // Vuot nhon 14% o moi dau
            const float Mep = 0.14f;
            float vuot = Mathf.SmoothStep(0f, 1f,
                            Mathf.Clamp01(Mathf.Min(k, 1f - k) / Mep));
            float w = width * Mathf.Lerp(1f, 0.62f, k) * vuot;

            Vector3 p = space.InverseTransformPoint(pts[i]);
            Vector3 off = space.InverseTransformDirection(s) * w;

            verts.Add(p - off); verts.Add(p + off);
            uvs.Add(new Vector2(0f, k)); uvs.Add(new Vector2(1f, k));
            cols.Add(Color.white); cols.Add(Color.white);
        }

        for (int i = 0; i < n - 1; i++)
        {
            int a = baseIndex + i * 2;
            tris.Add(a); tris.Add(a + 2); tris.Add(a + 1);
            tris.Add(a + 1); tris.Add(a + 2); tris.Add(a + 3);
        }
    }

    void OnDestroy()
    {
        if (coreMesh != null) Destroy(coreMesh);
        if (glowMesh != null) Destroy(glowMesh);
        if (coreMat != null) Destroy(coreMat);
        if (glowMat != null) Destroy(glowMat);
    }
}
