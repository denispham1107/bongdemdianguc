using UnityEngine;

/// <summary>Vet in tren mat dat (vet chay den, vet bang) mo dan roi bien mat.</summary>
public class GroundDecal : MonoBehaviour
{
    public float lifetime = 6f;
    public float fadeStart = 3f;

    Material mat;
    Color baseColor;
    float t;

    public static GroundDecal Spawn(Vector3 pos, float radius, Material sharedMat, float life, float fadeAt)
    {
        var go = new GameObject("Decal");
        go.transform.position = pos;

        // Vet in phai OM SAT mat dat: dat go ghe ma dung tam phang thi vet bi chon mat
        var mesh = new Mesh();
        mesh.name = "decal";
        GroundRing.BuildDisc(mesh, pos, radius, 20, 2, 0.06f);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = sharedMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        var d = go.AddComponent<GroundDecal>();
        d.lifetime = life;
        d.fadeStart = fadeAt;
        return d;
    }

    static Mesh quad;
    public static Mesh QuadMesh()
    {
        if (quad != null) return quad;
        quad = new Mesh();
        quad.name = "quad";
        quad.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),   new Vector3(-0.5f, 0.5f, 0f)
        };
        quad.uv = new Vector2[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        quad.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        quad.normals = new Vector3[] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
        quad.RecalculateBounds();
        return quad;
    }

    void Start()
    {
        var r = GetComponent<Renderer>();
        if (r != null)
        {
            mat = r.material;
            baseColor = mat.HasProperty("_TintColor") ? mat.GetColor("_TintColor")
                      : mat.HasProperty("_Color") ? mat.color : Color.white;
        }
    }

    void Update()
    {
        t += Time.deltaTime;
        if (mat != null && t > fadeStart)
        {
            float k = 1f - Mathf.Clamp01((t - fadeStart) / Mathf.Max(0.01f, lifetime - fadeStart));
            var c = baseColor; c.a = baseColor.a * k;
            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", c);
            else if (mat.HasProperty("_Color")) mat.color = c;
        }
        if (t >= lifetime) Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Moi vet in co hinh khoi rieng (dung theo dia hinh cho no) nen phai don tay
        var mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null) Destroy(mf.sharedMesh);
        if (mat != null) Destroy(mat);
    }
}
