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

    [Header("Anh ve tu Blender (Giut set)")]
    [Tooltip("Dai tia dan ANH (loi trang manh + quang xanh + soi re nhanh) thay cho vet mo ve bang code")]
    public bool anhBlender;
    [Tooltip("Be ngang ca dai anh, met - quang thay duoc chi chiem khoang 40% o giua")]
    public float beNgang = 1.1f;
    [Tooltip("Co cum dien bung o dau tia (tay) va cuoi tia (cho trung), met. 0 = khong ve")]
    public float coBungDau = 0.85f;
    public float coBungCuoi = 1.7f;
    [Tooltip("Hao quang xanh MEM, RONG nam duoi dai anh, tinh theo be ngang. 0 = khong ve")]
    public float heSoHaoQuang = 0.605f;
    /// <summary>Do dam lop hao quang (0,85 thi o goc choi that loa trang ca vung quanh tay).</summary>
    public const float HaoQuangDuc = 0.66f;

    /// <summary>
    /// Luoi VIEN XANH (anh GiatSetQuang) rong hon luoi loi trang bao nhieu lan. Nguoi dung 25/09/2026: "tia set trang da
    /// on, cho vien xanh sang va day hon 10%" - truoc day hai lop dung chung mot luoi nen noi vien la noi ca loi.
    /// </summary>
    public const float HeSoVienXanh = 1.10f;
    /// <summary>He so vien xanh CUA TIA NAY (mac dinh HeSoVienXanh). May giong dat rieng x1,2 (nguoi dung 25/09/2026:
    /// "tia set mau xanh duong dam hon") - Giut set / Qua cau dien giu nguyen.</summary>
    public float heSoVien = HeSoVienXanh;

    Mesh coreMesh, glowMesh, auraMesh;
    Material coreMat, glowMat, auraMat;
    float age, flickerTimer;

    // Bo nho dung chung khi dung hinh (khoi phai cap phat lai moi khung hinh)
    static readonly List<Vector3> verts = new List<Vector3>();
    static readonly List<Vector2> uvs = new List<Vector2>();
    static readonly List<Color> cols = new List<Color>();
    static readonly List<int> tris = new List<int>();

    // Bam theo mot vat dang di chuyen (tia set trong long Gio loc). Null = dung yen tai cho nhu moi tia khac.
    Transform bamTheo;
    Vector3 lechDau, lechCuoi;

    // Bam RIENG tung dau (Giut set: dau o giua hai tay, cuoi o than ke dich) - tia hien 0,6 s, dung yen
    // thi nguoi choi buoc di hay quai chay la tia treo lo lung giua khong trung.
    Transform dauT, cuoiT;
    bool coDau, coCuoi;

    /// <summary>
    /// Cho DAU tia di theo <paramref name="dau"/> va CUOI tia di theo <paramref name="cuoi"/> (null = dung yen).
    /// Giu nguyen khoang lech luc goi. Vat bi xoa (quai chet) thi dau ay dung lai o cho cuoi cung.
    /// </summary>
    public void BamHaiDau(Transform dau, Transform cuoi)
    {
        dauT = dau; cuoiT = cuoi;
        coDau = dau != null; coCuoi = cuoi != null;
        if (coDau) lechDau = start - dau.position;
        if (coCuoi) lechCuoi = end - cuoi.position;
    }

    /// <summary>
    /// Cho tia DI THEO vat <paramref name="t"/> (chi vi tri): hai dau giu nguyen khoang lech so voi vat luc goi.
    /// Nguoi dung 17/09/2026: tia trong Gio loc "luon bi bo lai phia sau" - loc bay 9,5 m/s, tia song 0,13-0,28 s dung yen
    /// o toa do the gioi nen loc di mat 1,2-2,7 m truoc khi tia tat.
    /// </summary>
    public void BamTheo(Transform t)
    {
        bamTheo = t;
        if (t == null) return;
        lechDau = start - t.position;
        lechCuoi = end - t.position;
    }

    void LateUpdate()
    {
        if (coDau || coCuoi)
        {
            if (coDau && dauT != null) start = dauT.position + lechDau;
            if (coCuoi && cuoiT != null) end = cuoiT.position + lechCuoi;
            transform.position = start;
            return;
        }
        if (bamTheo == null) return;
        // Luoi dung trong khong gian cuc bo cua transform nay (goc = start) nen doi transform la ca tia doi theo;
        // start/end doi cung luc de lan giat hinh sau (Rebuild) van dung cho.
        start = bamTheo.position + lechDau;
        end = bamTheo.position + lechCuoi;
        transform.position = start;
    }

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
        if (anhBlender && MatAnhLoi == null) anhBlender = false;   // thieu anh thi quay ve tia cu, dung de trong

        coreMesh = NewMesh("BoltCore");
        // Che do anh: loi va quang nam cung cho tren hai anh nhung luoi QUANG RONG HON (HeSoVienXanh) - hai luoi
        // rieng, cung duong di / dai anh / nhanh (xem BuildAnh)
        glowMesh = NewMesh("BoltGlow");

        coreMat = new Material(anhBlender ? MatAnhLoi : VfxFactory.BoltCoreMat);
        glowMat = new Material(anhBlender ? MatAnhQuang : VfxFactory.BoltGlowMat);

        // HAO QUANG (chi che do anh): anh mau co mot vung xanh dam bao quanh ca than tia, rong hon han quang
        // trong anh (lan chup dau chi thay loi trang). Dung lai vet mo cua tia cu, khong nhanh, ve DUOI CUNG.
        if (anhBlender && heSoHaoQuang > 0f)
        {
            auraMesh = NewMesh("BoltAura");
            auraMat = new Material(VfxFactory.BoltGlowMat);
            MakeRenderer("Aura", auraMesh, auraMat);
            SetFade(auraMat, glowColor, HaoQuangDuc);
        }

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
        // Tia anh la tia DUY TRI (nhu anh mau): giu sang nguyen 65% doi roi moi tat
        if (anhBlender) fade = k < 0.65f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (k - 0.65f) / 0.35f);

        SetFade(coreMat, coreColor, fade);
        SetFade(glowMat, glowColor, fade * 0.9f);
        if (auraMat != null) SetFade(auraMat, glowColor, fade * HaoQuangDuc);

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
        if (anhBlender) { BuildAnh(coreMesh, path); return; }
        BuildInto(coreMesh, path, coreWidth);
        BuildInto(glowMesh, path, glowWidth);
    }

    // ================================================================
    //  CHE DO ANH (Giut set, nguoi dung 24/09/2026)
    // ================================================================

    /// <summary>
    /// Hai anh dung bang Blender MCP (<c>CongCu/Blender/giat_set.blend</c>), cung bo cuc:
    /// nua DUOI (v 0..0,5) la 4 dai than tia xep chong, moi dai dai 4 lan be ngang va LAP LIEN MACH theo u
    /// (than chinh vao/ra dung giua mep) - tia dai bao nhieu cung chi can lap lai, khong keo gian;
    /// nua TREN (v 0,5..1) la 4 cum dien bung 2 x 2 cho hai dau tia.
    /// GiatSetLoi = loi trang manh + soi re nhanh; GiatSetQuang = cung hinh ay nhung day hon va lam mo.
    /// Anh xam nen to mau luc chay duoc (quai Giut set mau xanh la).
    /// </summary>
    public const string AnhLoi = "KyNang/GiatSet/GiatSetLoi";
    public const string AnhQuang = "KyNang/GiatSet/GiatSetQuang";
    static Material matAnhLoi, matAnhQuang;
    static bool thieuAnh;

    static void NapAnh()
    {
        // Kiem bang null CUA UNITY chu khong bang co "da nap": vat lieu tao luc Play bi xoa khi thoat Play,
        // ma bien static van giu (phep thu tat nap lai domain) - co "da nap" thi lan Play sau tia am tham
        // quay ve kieu cu. Cung cach VfxFactory.BoltCoreMat.
        if (thieuAnh || (matAnhLoi != null && matAnhQuang != null)) return;
        var loi = Resources.Load<Texture2D>(AnhLoi);
        var quang = Resources.Load<Texture2D>(AnhQuang);
        if (loi == null || quang == null) { thieuAnh = true; Debug.LogWarning("[LightningArc] thieu anh Giut set: " + AnhLoi); return; }
        // Nguoi dung 25/09/2026: "qua sang, qua day" khi tu choi - o goc choi that (may quay sau lung, 3D tu do)
        // ba bon tia moc tu tay chong len nhau thanh mot mang trang chui (16% vung tia). Ha 1,8/2,8 -> 1,2/1,5.
        // Lan hai cung ngay: "tia trang manh thanh hon 50%, tang anh sang xanh boc ngoai" - anh ve lai trong Blender
        // (loi ong nho mot nua, quang bot dai chay sang o tam, loang rong hon), quang day len 2,2 cho bu.
        matAnhLoi = Mats.Additive("P_GiatSetLoi", loi, Color.white, 1.0f);
        matAnhQuang = Mats.Additive("P_GiatSetQuang", quang, Color.white, 2.42f);   // vien xanh sang hon 10% (2,2)
    }
    public static Material MatAnhLoi { get { NapAnh(); return matAnhLoi; } }
    public static Material MatAnhQuang { get { NapAnh(); return matAnhQuang; } }

    /// <summary>Dem cho phep thu: so lan dung luoi che do anh.</summary>
    public static int SoLanDungAnh;

    void BuildAnh(Mesh mesh, Vector3[] path)
    {
        if (mesh == null) return;
        SoLanDungAnh++;

        if (auraMesh != null)
        {
            verts.Clear(); uvs.Clear(); cols.Clear(); tris.Clear();
            AppendRibbon(path, beNgang * heSoHaoQuang);
            auraMesh.Clear();
            auraMesh.SetVertices(verts);
            auraMesh.SetUVs(0, uvs);
            auraMesh.SetColors(cols);
            auraMesh.SetTriangles(tris, 0);
            auraMesh.RecalculateBounds();
        }

        // Moi lan giat hinh: doi dai khac, truot u ngau nhien, lat doc ngau nhien -> 4 dai ra hang chuc hinh.
        // Gieo MOT LAN roi dung cho CA HAI luoi (loi trang, vien xanh): vien rong hon nhung phai trung tung khuc.
        daiChinh = Random.Range(0, 4); uChinh = Random.value; latChinh = Random.value < 0.5f;
        nhanhAnh.Clear();
        for (int i = 0; i < branches; i++)
        {
            var b = MakeBranch(path);
            if (b == null) continue;
            nhanhAnh.Add(new NhanhAnh { pts = b, dai = Random.Range(0, 4), u = Random.value, lat = Random.value < 0.5f });
        }
        gocBungDau = Random.Range(0f, Mathf.PI * 2f); oBungDau = Random.Range(0, 4);
        gocBungCuoi = Random.Range(0f, Mathf.PI * 2f); oBungCuoi = Random.Range(0, 4);

        DungLuoiAnh(mesh, path, 1f);
        if (glowMesh != null && glowMesh != mesh) DungLuoiAnh(glowMesh, path, heSoVien);
    }

    struct NhanhAnh { public Vector3[] pts; public int dai; public float u; public bool lat; }
    readonly List<NhanhAnh> nhanhAnh = new List<NhanhAnh>();
    int daiChinh, oBungDau, oBungCuoi;
    float uChinh, gocBungDau, gocBungCuoi;
    bool latChinh;

    /// <summary>Dung mot luoi anh voi be ngang x <paramref name="noi"/> (1 = loi trang, HeSoVienXanh = vien xanh).
    /// Chu ky u tinh theo be ngang GOC nen anh cua hai luoi trung nhau theo chieu doc tia, chi vien phinh ra hai ben.</summary>
    void DungLuoiAnh(Mesh mesh, Vector3[] path, float noi)
    {
        verts.Clear(); uvs.Clear(); cols.Clear(); tris.Clear();

        // Dau tia vuot DAI hon (12%): moi tia deu moc tu cung mot diem giua hai tay, vuot ngan thi ba bon dai
        // anh chong kin nhau ngay truoc mat may quay.
        AppendRibbonAnh(path, beNgang * noi, beNgang, daiChinh, uChinh, latChinh, 0.12f, 0.07f);
        foreach (var nh in nhanhAnh)
            AppendRibbonAnh(nh.pts, beNgang * 0.6f * noi, beNgang * 0.6f, nh.dai, nh.u, nh.lat, 0.12f, 0.35f);

        if (coBungDau > 0f) AppendBung(path[0], coBungDau * noi, gocBungDau, oBungDau);
        if (coBungCuoi > 0f) AppendBung(path[path.Length - 1], coBungCuoi * noi, gocBungCuoi, oBungCuoi);

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
    }

    /// <summary>
    /// Dai anh doc theo duong di, mat quay ve may quay nhu AppendRibbon. u = met da di / (4 x be ngang)
    /// nen anh giu dung ti le, lap lai tren tia dai; v nam trong mot dai (1/8 anh).
    /// </summary>
    void AppendRibbonAnh(Vector3[] pts, float width, float beNgangChuKy, int dai, float lechU, bool latDoc, float mepDau, float mepCuoi)
    {
        int n = pts.Length;
        if (n < 2) return;

        int baseIndex = verts.Count;
        Transform space = transform;
        Vector3 mat = ChoMayQuay();
        float v0 = dai * 0.125f + 0.004f, v1 = (dai + 1) * 0.125f - 0.004f;
        if (latDoc) { float t = v0; v0 = v1; v1 = t; }

        float tong = 0f;
        for (int i = 1; i < n; i++) tong += Vector3.Distance(pts[i], pts[i - 1]);
        tong = Mathf.Max(0.001f, tong);
        float daDi = 0f;
        float chuKy = 4f * Mathf.Max(0.05f, beNgangChuKy);

        for (int i = 0; i < n; i++)
        {
            if (i > 0) daDi += Vector3.Distance(pts[i], pts[i - 1]);
            Vector3 t = i == 0 ? pts[1] - pts[0]
                      : i == n - 1 ? pts[n - 1] - pts[n - 2]
                      : pts[i + 1] - pts[i - 1];
            if (t.sqrMagnitude < 1e-6f) t = Vector3.down;
            t.Normalize();

            Vector3 s = Vector3.Cross(t, mat - pts[i]);
            if (s.sqrMagnitude < 1e-8f) { Vector3 a1, a2; Frame(t, out a1, out a2); s = a1; }
            s.Normalize();

            float k = daDi / tong;
            // Vuot NGAN o hai dau: cum dien bung da che cho noi, tia phai day dan ngay tu tay
            float vuot = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(k / Mathf.Max(0.001f, mepDau)))
                       * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - k) / Mathf.Max(0.001f, mepCuoi)));
            float w = width * 0.5f * Mathf.Lerp(1f, 0.85f, k) * vuot;

            Vector3 p = space.InverseTransformPoint(pts[i]);
            Vector3 off = space.InverseTransformDirection(s) * w;
            float u = daDi / chuKy + lechU;

            verts.Add(p - off); verts.Add(p + off);
            uvs.Add(new Vector2(u, v0)); uvs.Add(new Vector2(u, v1));
            cols.Add(Color.white); cols.Add(Color.white);
        }

        for (int i = 0; i < n - 1; i++)
        {
            int a = baseIndex + i * 2;
            tris.Add(a); tris.Add(a + 2); tris.Add(a + 1);
            tris.Add(a + 1); tris.Add(a + 2); tris.Add(a + 3);
        }
    }

    /// <summary>Mot cum dien bung (o vuong quay mat ve may quay, xoay ngau nhien) - o tay va o cho trung.</summary>
    void AppendBung(Vector3 tam, float co, float g, int o)
    {
        Vector3 mat = ChoMayQuay();
        Vector3 nhin = tam - mat;
        if (nhin.sqrMagnitude < 1e-6f) return;
        nhin.Normalize();
        Vector3 phai, len;
        Frame(nhin, out phai, out len);
        Vector3 a = (phai * Mathf.Cos(g) + len * Mathf.Sin(g)) * (co * 0.5f);
        Vector3 b = (-phai * Mathf.Sin(g) + len * Mathf.Cos(g)) * (co * 0.5f);

        float u0 = (o % 2) * 0.5f + 0.004f, u1 = u0 + 0.5f - 0.008f;
        float v0 = 0.5f + (o / 2) * 0.25f + 0.004f, v1 = v0 + 0.25f - 0.008f;

        Transform space = transform;
        int baseIndex = verts.Count;
        verts.Add(space.InverseTransformPoint(tam - a - b)); uvs.Add(new Vector2(u0, v0));
        verts.Add(space.InverseTransformPoint(tam + a - b)); uvs.Add(new Vector2(u1, v0));
        verts.Add(space.InverseTransformPoint(tam - a + b)); uvs.Add(new Vector2(u0, v1));
        verts.Add(space.InverseTransformPoint(tam + a + b)); uvs.Add(new Vector2(u1, v1));
        for (int i = 0; i < 4; i++) cols.Add(Color.white);
        tris.Add(baseIndex); tris.Add(baseIndex + 2); tris.Add(baseIndex + 1);
        tris.Add(baseIndex + 1); tris.Add(baseIndex + 2); tris.Add(baseIndex + 3);
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
        if (glowMesh != null && glowMesh != coreMesh) Destroy(glowMesh);
        if (auraMesh != null) Destroy(auraMesh);
        if (auraMat != null) Destroy(auraMat);
        if (coreMat != null) Destroy(coreMat);
        if (glowMat != null) Destroy(glowMat);
    }
}
