using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VONG SANG / VUNG SANG BAM THEO MAT DAT.
///
/// Truoc day cac vong phep duoi chan chi la MOT TAM PHANG dat cao hon tam
/// dung 8cm. Tu khi mat dat co doi nhap nho, chenh lech do cao tren mot vong
/// ban kinh 6m len toi hon 1m - nen quá nua vong bi chon duoi dat va bien mat.
///
/// Vat the nay dung vong tron thanh mot cai dia nhieu manh, moi dinh duoc
/// BAN TIA XUONG DAT de lay dung do cao cho no, roi nang len mot chut.
/// Nho vay vong phep om sat mat dat du dat go ghe the nao.
///
/// Neu dat duration > 0 thi vong con phong to dan (dung cho song xung kich),
/// va moi khung hinh se do lai do cao cho khop.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class GroundRing : MonoBehaviour
{
    [Header("Kich thuoc (ban kinh ngoai cung)")]
    public float startRadius = 3f;
    public float endRadius = 3f;

    [Header("Thoi gian")]
    [Tooltip("0 = dung yen mot cho, khong phong to.")]
    public float duration = 0f;
    [Tooltip("Giu sang bao lau roi moi mo di. Chi dung khi duration = 0.")]
    public float hold = 0f;
    public float fadeTime = 0.4f;

    [Header("Do min")]
    public float lift = 0.14f;          // nang len khoi mat dat cho khoi lem nhem
    public int segments = 32;           // so nan quanh vong
    public int rings = 4;               // so vong dong tam

    public AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Mesh mesh;
    Material mat;
    Color baseColor;
    string colorProp;
    float age;
    float builtRadius = -1f;

    /// <summary>
    /// Bat len thi vong ve DE LEN vat can thay vi bi chung che.
    ///
    /// Chi dung cho vung phep cua Mua bang / Sam set: nguoi choi can nhin thay
    /// TRON VEN cai vong de biet pham vi phep den dau. Cac vong khac (song xung
    /// kich, vet chay...) van de tat - chung la vat trong canh, bi bia mo che la
    /// dung.
    /// </summary>
    public bool xuyenVatCan = false;

    static LayerMask groundMask;
    static bool maskReady;

    // Bo nho dung chung khi dung hinh
    static readonly List<Vector3> verts = new List<Vector3>();
    static readonly List<Vector2> uvs = new List<Vector2>();
    static readonly List<Color> cols = new List<Color>();
    static readonly List<int> tris = new List<int>();

    /// <summary>Tao mot vong sang bam dat tai mot diem.</summary>
    public static GroundRing Create(string name, Transform parent, Vector3 worldPos,
                                    Material material, float radius)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = worldPos;

        go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

        var r = go.AddComponent<GroundRing>();
        r.startRadius = radius;
        r.endRadius = radius;
        return r;
    }

    void Start()
    {
        if (!maskReady)
        {
            // CHI lop "Ground" - khong lay "Default".
            //
            // Lop Default la noi cua 458 bia mo, 229 tang da va 58 goc cay. Bat
            // tia xuong ma trung nhung thu do thi tung dinh cua vong bi keo LEN
            // NOC chung, va ca cai vong gay khuc thanh nhung hinh thu ki quac
            // quanh moi tam bia. Vong phep phai bam theo DIA HINH, khong bam
            // theo do dac dung tren dia hinh.
            groundMask = LayerMask.GetMask("Ground");
            maskReady = true;
        }

        mesh = new Mesh();
        mesh.name = "GroundRing";
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mat = mr.material;                       // ban sao rieng, khong dung chung
            colorProp = mat.HasProperty("_TintColor") ? "_TintColor"
                      : mat.HasProperty("_Color") ? "_Color" : null;
            if (colorProp != null) baseColor = mat.GetColor(colorProp);

            if (xuyenVatCan && mat.HasProperty("_ZTest"))
            {
                mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                // Day ra sau moi thu trong hang Transparent, khong thi vong ve
                // truoc roi khoi lua / hat tuyet lai de len chinh no
                mat.renderQueue = 3100;
            }
        }

        Rebuild(startRadius);
    }

    void Update()
    {
        age += Time.deltaTime;

        float alpha = 1f;

        if (duration > 0.001f)
        {
            // Song xung kich: vua lan rong ra vua nhat dan
            float k = Mathf.Clamp01(age / duration);
            Rebuild(Mathf.LerpUnclamped(startRadius, endRadius, ease.Evaluate(k)));
            alpha = 1f - k;

            if (k >= 1f) { Destroy(gameObject); return; }
        }
        else
        {
            // Dung yen: sang deu roi mo dan o cuoi doi
            float left = hold - age;
            if (left < fadeTime) alpha = Mathf.Clamp01(left / Mathf.Max(0.01f, fadeTime));
            if (hold > 0f && age >= hold) { Destroy(gameObject); return; }
        }

        if (mat != null && colorProp != null)
        {
            var c = baseColor;
            c.a = baseColor.a * alpha;
            mat.SetColor(colorProp, c);
        }
    }

    // ================================================================
    //  DUNG HINH DIA BAM DAT
    // ================================================================

    void Rebuild(float radius)
    {
        if (mesh == null) return;
        if (Mathf.Abs(radius - builtRadius) < 0.01f) return;   // chua doi gi thi thoi
        builtRadius = radius;

        BuildDisc(mesh, transform.position, radius, segments, rings, lift);
    }

    /// <summary>
    /// Dung mot cai dia OM SAT MAT DAT quanh mot diem, tra ve trong he toa do
    /// cuc bo cua diem do. Dung chung cho vong phep va cho ca vet in tren dat.
    /// </summary>
    public static void BuildDisc(Mesh mesh, Vector3 worldCenter, float radius,
                                 int segments, int rings, float lift)
    {
        if (mesh == null) return;

        if (!maskReady)
        {
            // CHI lop "Ground" - khong lay "Default".
            //
            // Lop Default la noi cua 458 bia mo, 229 tang da va 58 goc cay. Bat
            // tia xuong ma trung nhung thu do thi tung dinh cua vong bi keo LEN
            // NOC chung, va ca cai vong gay khuc thanh nhung hinh thu ki quac
            // quanh moi tam bia. Vong phep phai bam theo DIA HINH, khong bam
            // theo do dac dung tren dia hinh.
            groundMask = LayerMask.GetMask("Ground");
            maskReady = true;
        }

        if (segments < 8) segments = 8;
        if (rings < 1) rings = 1;

        verts.Clear(); uvs.Clear(); cols.Clear(); tris.Clear();

        for (int r = 0; r <= rings; r++)
        {
            float rad = radius * r / rings;

            for (int s = 0; s <= segments; s++)
            {
                float a = s / (float)segments * Mathf.PI * 2f;
                float dx = Mathf.Cos(a) * rad;
                float dz = Mathf.Sin(a) * rad;

                float y = SampleGround(worldCenter.x + dx, worldCenter.z + dz, worldCenter.y)
                        + lift - worldCenter.y;

                verts.Add(new Vector3(dx, y, dz));
                cols.Add(Color.white);

                // Trai anh dung nhu tam vuong cu, de dung lai duoc moi hoa tiet san co
                float u = 0.5f + 0.5f * dx / Mathf.Max(0.001f, radius);
                float v = 0.5f + 0.5f * dz / Mathf.Max(0.001f, radius);
                uvs.Add(new Vector2(u, v));
            }
        }

        int cols1 = segments + 1;
        for (int r = 0; r < rings; r++)
            for (int s = 0; s < segments; s++)
            {
                int i0 = r * cols1 + s;
                int i1 = i0 + 1;
                int i2 = i0 + cols1;
                int i3 = i2 + 1;

                tris.Add(i0); tris.Add(i2); tris.Add(i1);
                tris.Add(i1); tris.Add(i2); tris.Add(i3);
            }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    /// <summary>Do cao mat dat ngay duoi mot diem (khong trung gi thi giu nguyen do cao tam).</summary>
    static float SampleGround(float x, float z, float fallback)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, fallback + 12f, z), Vector3.down, out hit, 40f,
                            groundMask, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return fallback;
    }

    void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
        if (mat != null) Destroy(mat);
    }
}
