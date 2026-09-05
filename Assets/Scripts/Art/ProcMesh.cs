using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// XUONG DUC HINH 3D BANG CODE.
///
/// Toan bo nhan vat, quai vat, canh vat trong game deu duoc "tien" ra tu day:
/// ta ve mot duong vien (profile) roi QUAY quanh truc dung -> ra khoi tron
/// (than nguoi, tay chan, mu trum, cay gay...). Nho vay ban KHONG can tai
/// bat ky file .fbx / .obj nao ve may.
/// </summary>
public static class ProcMesh
{
    // ================================================================
    //  BUILDER - gom nhieu manh nho thanh mot mesh
    // ================================================================
    public class Builder
    {
        public readonly List<Vector3> verts = new List<Vector3>();
        public readonly List<Vector3> norms = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<Color> colors = new List<Color>();
        public readonly List<int> tris = new List<int>();

        public int VertexCount { get { return verts.Count; } }

        public int AddVertex(Vector3 p, Vector3 n, Vector2 uv, Color c)
        {
            verts.Add(p); norms.Add(n); uvs.Add(uv); colors.Add(c);
            return verts.Count - 1;
        }

        public void AddTriangle(int a, int b, int c)
        {
            tris.Add(a); tris.Add(b); tris.Add(c);
        }

        public void AddQuad(int a, int b, int c, int d)
        {
            AddTriangle(a, b, c);
            AddTriangle(a, c, d);
        }

        /// <summary>Dan mot mesh co san vao (co the doi vi tri / xoay / phong to).</summary>
        public void Append(Mesh m, Matrix4x4 trs, Color tint)
        {
            if (m == null) return;
            int off = verts.Count;
            var mv = m.vertices;
            var mn = m.normals;
            var mu = m.uv;
            Matrix4x4 nrm = trs.inverse.transpose;

            for (int i = 0; i < mv.Length; i++)
            {
                verts.Add(trs.MultiplyPoint3x4(mv[i]));
                norms.Add((mn != null && mn.Length == mv.Length)
                    ? nrm.MultiplyVector(mn[i]).normalized
                    : Vector3.up);
                uvs.Add((mu != null && mu.Length == mv.Length) ? mu[i] : Vector2.zero);
                colors.Add(tint);
            }

            var mt = m.triangles;
            for (int i = 0; i < mt.Length; i++) tris.Add(mt[i] + off);
        }

        public Mesh Build(string name, bool recalcNormals = false)
        {
            var m = new Mesh();
            m.name = name;
            if (verts.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(verts);
            m.SetNormals(norms);
            m.SetUVs(0, uvs);
            m.SetColors(colors);
            m.SetTriangles(tris, 0);
            if (recalcNormals) m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    // ================================================================
    //  QUAY TRON MOT DUONG VIEN (cach dung hinh chinh)
    // ================================================================

    /// <summary>
    /// Quay duong vien quanh truc Y.
    /// profile[i].x = ban kinh, profile[i].y = do cao.
    /// </summary>
    public static Mesh Revolve(Vector2[] profile, int segments, bool capBottom,
                               bool capTop, Color color, string name)
    {
        var b = new Builder();
        AppendRevolve(b, profile, segments, capBottom, capTop, Matrix4x4.identity, color);
        return b.Build(name);
    }

    public static Mesh Revolve(Vector2[] profile, int segments, Color color)
    {
        return Revolve(profile, segments, true, true, color, "revolve");
    }

    /// <summary>
    /// Quay duong vien nhung CHI QUAY MOT CUNG (vd 250 do) - phan con lai de ho.
    /// Dung de lam mu trum dau ho mat, mang giap ho nach...
    /// </summary>
    public static Mesh RevolveArc(Vector2[] profile, int segments, float startDegrees,
                                  float sweepDegrees, Color color, string name)
    {
        var b = new Builder();
        AppendRevolve(b, profile, segments, false, false, Matrix4x4.identity, color,
                      startDegrees, sweepDegrees);
        var m = b.Build(name);
        m.RecalculateNormals();
        return m;
    }

    /// <summary>Quay duong vien va dan thang vao mot Builder co san.</summary>
    public static void AppendRevolve(Builder b, Vector2[] profile, int segments,
                                     bool capBottom, bool capTop, Matrix4x4 trs, Color color)
    {
        AppendRevolve(b, profile, segments, capBottom, capTop, trs, color, 0f, 360f);
    }

    public static void AppendRevolve(Builder b, Vector2[] profile, int segments,
                                     bool capBottom, bool capTop, Matrix4x4 trs, Color color,
                                     float startDegrees, float sweepDegrees)
    {
        if (profile == null || profile.Length < 2) return;
        segments = Mathf.Max(3, segments);
        int rings = profile.Length;
        int cols = segments + 1;                 // +1 de khep duong noi (seam) cho dep
        int start = b.VertexCount;
        Matrix4x4 nrmM = trs.inverse.transpose;

        for (int r = 0; r < rings; r++)
        {
            float rad = Mathf.Max(0f, profile[r].x);
            float y = profile[r].y;

            // Phap tuyen tinh tu do doc cua duong vien
            Vector2 prev = profile[Mathf.Max(0, r - 1)];
            Vector2 next = profile[Mathf.Min(rings - 1, r + 1)];
            Vector2 tangent = next - prev;
            Vector2 n2 = new Vector2(tangent.y, -tangent.x).normalized;
            if (n2.sqrMagnitude < 0.0001f) n2 = Vector2.right;

            for (int c = 0; c < cols; c++)
            {
                float t = (float)c / segments;
                float ang = (startDegrees + t * sweepDegrees) * Mathf.Deg2Rad;
                float sx = Mathf.Cos(ang), sz = Mathf.Sin(ang);

                Vector3 p = new Vector3(sx * rad, y, sz * rad);
                Vector3 n = new Vector3(sx * n2.x, n2.y, sz * n2.x).normalized;

                b.AddVertex(trs.MultiplyPoint3x4(p),
                            nrmM.MultiplyVector(n).normalized,
                            new Vector2(t, (float)r / (rings - 1)),
                            color);
            }
        }

        for (int r = 0; r < rings - 1; r++)
        {
            for (int c = 0; c < segments; c++)
            {
                int i0 = start + r * cols + c;
                int i1 = i0 + 1;
                int i2 = i0 + cols;
                int i3 = i2 + 1;
                b.AddTriangle(i0, i2, i1);
                b.AddTriangle(i1, i2, i3);
            }
        }

        // Dap nap tren / duoi neu ban kinh con lon hon 0
        if (capBottom && profile[0].x > 0.001f)
            AppendCap(b, profile[0], segments, trs, color, false);
        if (capTop && profile[rings - 1].x > 0.001f)
            AppendCap(b, profile[rings - 1], segments, trs, color, true);
    }

    static void AppendCap(Builder b, Vector2 ring, int segments, Matrix4x4 trs, Color color, bool up)
    {
        Matrix4x4 nrmM = trs.inverse.transpose;
        Vector3 n = nrmM.MultiplyVector(up ? Vector3.up : Vector3.down).normalized;
        int center = b.AddVertex(trs.MultiplyPoint3x4(new Vector3(0, ring.y, 0)), n,
                                 new Vector2(0.5f, 0.5f), color);

        int first = b.VertexCount;
        for (int c = 0; c <= segments; c++)
        {
            float ang = (float)c / segments * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(ang) * ring.x, ring.y, Mathf.Sin(ang) * ring.x);
            b.AddVertex(trs.MultiplyPoint3x4(p), n,
                        new Vector2(Mathf.Cos(ang) * 0.5f + 0.5f, Mathf.Sin(ang) * 0.5f + 0.5f), color);
        }

        for (int c = 0; c < segments; c++)
        {
            if (up) b.AddTriangle(center, first + c, first + c + 1);
            else b.AddTriangle(center, first + c + 1, first + c);
        }
    }

    // ================================================================
    //  CAC KHOI CO BAN
    // ================================================================

    /// <summary>Qua cau (hoi bep neu squashY khac 1).</summary>
    public static Mesh Sphere(float radius, int segments, int stacks, float squashY, Color color)
    {
        var prof = new Vector2[stacks + 1];
        for (int i = 0; i <= stacks; i++)
        {
            float t = (float)i / stacks;
            float ang = t * Mathf.PI;              // 0 = day, PI = dinh
            prof[i] = new Vector2(Mathf.Sin(ang) * radius, -Mathf.Cos(ang) * radius * squashY);
        }
        return Revolve(prof, segments, false, false, color, "sphere");
    }

    public static Mesh Sphere(float radius, Color color)
    {
        return Sphere(radius, 20, 12, 1f, color);
    }

    /// <summary>Ong thuon (dung cho tay, chan, than cay, cay gay).</summary>
    public static Mesh Taper(float rBottom, float rTop, float height, int segments,
                             bool round, Color color)
    {
        var pts = new List<Vector2>();
        if (round) pts.Add(new Vector2(rBottom * 0.55f, -rBottom * 0.45f));
        pts.Add(new Vector2(rBottom, 0f));
        pts.Add(new Vector2(Mathf.Lerp(rBottom, rTop, 0.5f), height * 0.5f));
        pts.Add(new Vector2(rTop, height));
        if (round) pts.Add(new Vector2(rTop * 0.55f, height + rTop * 0.45f));
        return Revolve(pts.ToArray(), segments, !round, !round, color, "taper");
    }

    /// <summary>Khoi hop.</summary>
    public static Mesh Box(Vector3 size, Color color)
    {
        var b = new Builder();
        AppendBox(b, Matrix4x4.identity, size, color);
        return b.Build("box");
    }

    public static void AppendBox(Builder b, Matrix4x4 trs, Vector3 size, Color color)
    {
        Vector3 h = size * 0.5f;
        Vector3[] dirs = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        Matrix4x4 nrmM = trs.inverse.transpose;

        for (int k = 0; k < dirs.Length; k++)
        {
            Vector3 d = dirs[k];
            Vector3 u = (Mathf.Abs(d.y) > 0.5f) ? Vector3.right : Vector3.up;
            Vector3 v = Vector3.Cross(d, u);
            Vector3 c = Vector3.Scale(d, h);
            Vector3 su = Vector3.Scale(u, h);
            Vector3 sv = Vector3.Scale(v, h);
            Vector3 n = nrmM.MultiplyVector(d).normalized;

            int i0 = b.AddVertex(trs.MultiplyPoint3x4(c - su - sv), n, new Vector2(0, 0), color);
            int i1 = b.AddVertex(trs.MultiplyPoint3x4(c + su - sv), n, new Vector2(1, 0), color);
            int i2 = b.AddVertex(trs.MultiplyPoint3x4(c + su + sv), n, new Vector2(1, 1), color);
            int i3 = b.AddVertex(trs.MultiplyPoint3x4(c - su + sv), n, new Vector2(0, 1), color);
            b.AddQuad(i0, i1, i2, i3);
        }
    }

    /// <summary>Tinh the / manh bang: hai chop nhon up vao nhau, mat cat co canh sac.</summary>
    public static Mesh Crystal(float radius, float height, int sides, float tipRatio, Color color)
    {
        var prof = new Vector2[]
        {
            new Vector2(0.001f, -height * tipRatio),
            new Vector2(radius * 0.75f, -height * tipRatio * 0.35f),
            new Vector2(radius, 0f),
            new Vector2(radius * 0.85f, height * 0.45f),
            new Vector2(0.001f, height),
        };
        var m = Revolve(prof, sides, false, false, color, "crystal");
        m.RecalculateNormals();      // de mat cat co canh sac nhu tinh the that
        return m;
    }

    /// <summary>Khoi da xu xi (dung cho da tang, tuong do nat).</summary>
    public static Mesh Rock(float radius, int seed, float roughness, int segments, Color color)
    {
        var m = Sphere(radius, segments, Mathf.Max(4, segments / 2), Random.Range(0.7f, 1.0f), color);
        Displace(m, radius * roughness, 1.6f / Mathf.Max(0.01f, radius), seed);
        m.RecalculateNormals();
        return m;
    }

    // ================================================================
    //  BIEN DANG
    // ================================================================

    /// <summary>Lam bien dang mesh bang nhieu (noise) -> be mat go ghe, tu nhien hon.</summary>
    public static Mesh Displace(Mesh m, float amount, float frequency, int seed)
    {
        var v = m.vertices;
        var n = m.normals;
        Vector3 o = new Vector3(seed * 13.31f, seed * 7.77f, seed * 3.19f);

        for (int i = 0; i < v.Length; i++)
        {
            Vector3 p = v[i] * frequency + o;
            float d = Noise3(p) - 0.5f;
            Vector3 dir = (n != null && n.Length == v.Length) ? n[i] : v[i].normalized;
            v[i] += dir * d * 2f * amount;
        }

        m.vertices = v;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    /// <summary>Uon cong mesh theo do cao (dung cho sung, mong vuot, lung go).</summary>
    public static Mesh Bend(Mesh m, float degreesPerUnit, Vector3 axis)
    {
        var v = m.vertices;
        for (int i = 0; i < v.Length; i++)
        {
            float ang = v[i].y * degreesPerUnit;
            v[i] = Quaternion.AngleAxis(ang, axis) * v[i];
        }
        m.vertices = v;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    public static Mesh ScaleMesh(Mesh m, Vector3 s)
    {
        var v = m.vertices;
        for (int i = 0; i < v.Length; i++) v[i] = Vector3.Scale(v[i], s);
        m.vertices = v;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    // Nhieu 3D don gian, dung chung cho ca game
    public static float Noise3(Vector3 p)
    {
        float ab = Mathf.PerlinNoise(p.x, p.y);
        float bc = Mathf.PerlinNoise(p.y, p.z);
        float ca = Mathf.PerlinNoise(p.z, p.x);
        return (ab + bc + ca) / 3f;
    }

    // ================================================================
    //  DA GOC CANH  (phien da, ngon da nhon)
    // ================================================================

    /// <summary>
    /// Dan mot PHIEN DA GOC CANH vao builder.
    ///
    /// Khac voi <see cref="Rock"/> (qua cau bi bop meo -> tron lu), phien da o day
    /// la mot khoi lang tru thit dan len ngon: moi mat ben la MOT MAT PHANG rieng,
    /// co phap tuyen rieng, nen ria da sac lem nhu da phien that.
    ///
    /// Moi canh duoc gieo mot do lech GIU NGUYEN suot chieu cao, nho vay cac ria
    /// chay thang tu chan len ngon chu khong lom nhom lung tung.
    /// </summary>
    public static void AppendShard(Builder b, Matrix4x4 trs, float baseRadius, float height,
                                   int sides, int levels, float tipScale, float jitter,
                                   Color color)
    {
        sides = Mathf.Max(3, sides);
        levels = Mathf.Max(1, levels);

        // Do lech rieng cua tung canh - giu nguyen tu duoi len tren
        var sideBias = new float[sides];
        for (int s = 0; s < sides; s++) sideBias[s] = 1f + Random.Range(-jitter, jitter);

        var rings = new Vector3[levels + 1][];
        var centres = new Vector3[levels + 1];

        for (int L = 0; L <= levels; L++)
        {
            float t = L / (float)levels;
            float r = baseRadius * Mathf.Lerp(1f, tipScale, Mathf.Pow(t, 0.8f));
            float twist = t * Random.Range(-18f, 18f) * Mathf.Deg2Rad;

            Vector3 c = new Vector3(0f, height * t, 0f);
            centres[L] = trs.MultiplyPoint3x4(c);

            rings[L] = new Vector3[sides];
            for (int s = 0; s < sides; s++)
            {
                float a = s / (float)sides * Mathf.PI * 2f + twist;
                float rr = r * sideBias[s] * Random.Range(0.94f, 1.06f);
                rings[L][s] = trs.MultiplyPoint3x4(
                    c + new Vector3(Mathf.Cos(a) * rr, 0f, Mathf.Sin(a) * rr));
            }
        }

        var quad = new Vector3[4];

        // Cac mat ben
        for (int L = 0; L < levels; L++)
            for (int s = 0; s < sides; s++)
            {
                int s2 = (s + 1) % sides;
                quad[0] = rings[L][s];
                quad[1] = rings[L][s2];
                quad[2] = rings[L + 1][s2];
                quad[3] = rings[L + 1][s];

                Vector3 mid = (centres[L] + centres[L + 1]) * 0.5f;
                AddFlatFace(b, quad, 4, mid, color);
            }

        // Nap tren (long khoi nam ben duoi) va nap duoi (long khoi nam ben tren)
        AddFlatFace(b, rings[levels], sides, centres[levels - 1], color);
        AddFlatFace(b, rings[0], sides, centres[1], color);
    }

    /// <summary>
    /// Dan mot mat phang kin vao builder, moi dinh mot ban sao rieng nen mat nay
    /// co phap tuyen doc lap -> ria sac.
    /// <paramref name="inside"/> la mot diem nam PHIA TRONG khoi, dung de biet
    /// mat dang quay ra ngoai hay vao trong ma lat lai cho dung.
    /// </summary>
    static void AddFlatFace(Builder b, Vector3[] pts, int count, Vector3 inside, Color color)
    {
        if (count < 3) return;

        Vector3 n = Vector3.Cross(pts[1] - pts[0], pts[2] - pts[0]);
        if (n.sqrMagnitude < 1e-10f) return;
        n.Normalize();

        // Tam cua mat, de biet mat dang huong ra ngoai hay vao trong
        Vector3 centre = Vector3.zero;
        for (int i = 0; i < count; i++) centre += pts[i];
        centre /= count;

        bool flip = Vector3.Dot(n, centre - inside) < 0f;
        if (flip) n = -n;

        // Trai anh theo mat phang vuong goc voi truc lon nhat cua phap tuyen,
        // nhu vay hoa tiet khong bi keo nhoe tren bat ky mat nao
        float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);

        int first = b.VertexCount;
        for (int i = 0; i < count; i++)
        {
            Vector3 p = pts[i];
            Vector2 uv = (ay >= ax && ay >= az) ? new Vector2(p.x, p.z)
                       : (ax >= az) ? new Vector2(p.z, p.y)
                                    : new Vector2(p.x, p.y);
            b.AddVertex(p, n, uv, color);
        }

        for (int i = 1; i < count - 1; i++)
        {
            if (flip) b.AddTriangle(first, first + i + 1, first + i);
            else b.AddTriangle(first, first + i, first + i + 1);
        }
    }

    // ================================================================
    //  CAY CO / LA  (mieng vai cat cheo nhau)
    // ================================================================

    /// <summary>
    /// Dan mot BUI CO / CHUM LA vao builder: gom vai mieng vai phang dung thang,
    /// xoay lech nhau quanh truc dung nen nhin tu huong nao cung thay day.
    ///
    /// uv.y = 0 o goc, 1 o ngon -> shader Foliage dua vao do de lac ngon theo gio.
    /// Gom nhieu bui vao CHUNG mot builder giup ca bai co chi ton mot lenh ve.
    /// </summary>
    public static void AppendFoliage(Builder b, Vector3 pos, float width, float height,
                                     int planes, float yawOffset, Color tint)
    {
        if (planes < 1) planes = 1;

        for (int p = 0; p < planes; p++)
        {
            float yaw = yawOffset + p * (180f / planes);
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);

            Vector3 right = rot * Vector3.right * (width * 0.5f);
            Vector3 up = Vector3.up * height;
            Vector3 n = rot * Vector3.forward;

            int i0 = b.AddVertex(pos - right, n, new Vector2(0f, 0f), tint);
            int i1 = b.AddVertex(pos + right, n, new Vector2(1f, 0f), tint);
            int i2 = b.AddVertex(pos + right + up, n, new Vector2(1f, 1f), tint);
            int i3 = b.AddVertex(pos - right + up, n, new Vector2(0f, 1f), tint);

            b.AddQuad(i0, i1, i2, i3);
        }
    }

    /// <summary>Mot chum la nam NGANG (dung lam tan cay nhin tu tren xuong).</summary>
    public static void AppendLeafCard(Builder b, Vector3 pos, Quaternion rot,
                                      float width, float length, Color tint)
    {
        Vector3 right = rot * Vector3.right * (width * 0.5f);
        Vector3 fwd = rot * Vector3.forward * (length * 0.5f);
        Vector3 n = rot * Vector3.up;

        int i0 = b.AddVertex(pos - right - fwd, n, new Vector2(0f, 0f), tint);
        int i1 = b.AddVertex(pos + right - fwd, n, new Vector2(1f, 0f), tint);
        int i2 = b.AddVertex(pos + right + fwd, n, new Vector2(1f, 1f), tint);
        int i3 = b.AddVertex(pos - right + fwd, n, new Vector2(0f, 1f), tint);

        b.AddQuad(i0, i1, i2, i3);
    }

    // ================================================================
    //  TAO GAMEOBJECT NHANH
    // ================================================================

    public static GameObject Part(string name, Transform parent, Mesh mesh, Material mat,
                                  Vector3 localPos, Quaternion localRot, Vector3 localScale,
                                  bool castShadow)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale = localScale;

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = castShadow
            ? UnityEngine.Rendering.ShadowCastingMode.On
            : UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = true;
        return go;
    }

    public static GameObject Part(string name, Transform parent, Mesh mesh, Material mat,
                                  Vector3 localPos, Quaternion localRot, Vector3 localScale)
    {
        return Part(name, parent, mesh, mat, localPos, localRot, localScale, true);
    }

    public static GameObject Part(string name, Transform parent, Mesh mesh, Material mat, Vector3 localPos)
    {
        return Part(name, parent, mesh, mat, localPos, Quaternion.identity, Vector3.one, true);
    }

    /// <summary>Tao mot khop xuong rong (chi de xoay, khong co hinh).</summary>
    public static Transform Joint(string name, Transform parent, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }
}
