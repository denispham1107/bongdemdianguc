using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VE DUONG NGAM cua ky nang tren MAT DAT, trong luc nguoi choi giu ngon tay
/// tren mot nut ky nang va keo ra.
///
/// Ve trong THE GIOI chu khong phai bang OnGUI: mot hinh quat 14 m vach tren
/// mat dat go ghe thi phai bam theo dat moi doc ra duoc no toi dau. Ve len lop
/// giao dien phang thi no chi la mot hinh tam giac lo lung, khong noi duoc gi
/// ve tam xa that.
///
/// Cach dung: goi <see cref="Hien"/> moi khung hinh trong luc dang ngam, va
/// <see cref="An"/> khi tha tay. Khong co ai goi thi vat the nay khong ve gi
/// va gan nhu khong ton gi.
///
/// MOI DINH DEU BAN TIA XUONG DAT de lay do cao, giong <see cref="GroundRing"/>.
/// Do la ly do so nan duoc giu vua phai: 40 nan cho mot vanh la 80 tia moi
/// khung, con chap nhan duoc vi chi ban trong may giay dang ngam.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChiBaoNgam : MonoBehaviour
{
    /// <summary>Nang len khoi mat dat cho khoi lem vao dat.</summary>
    public float lift = 0.09f;

    Mesh mesh;
    MeshRenderer mr;

    static LayerMask groundMask;
    static bool maskSan;

    readonly List<Vector3> verts = new List<Vector3>();
    readonly List<Color> cols = new List<Color>();
    readonly List<Vector2> uvs = new List<Vector2>();
    readonly List<int> tris = new List<int>();

    /// <summary>Tao vat the chi bao, gan san vat lieu va luoi rong.</summary>
    public static ChiBaoNgam Tao(Transform parent)
    {
        var go = new GameObject("ChiBaoNgam");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        var mf = go.AddComponent<MeshFilter>();
        var r = go.AddComponent<MeshRenderer>();

        // CONG SANG, va quan trong hon la XUYEN QUA VAT CAN.
        //
        // Duong ngam la mot thu cua giao dien chu khong phai vat the trong the
        // gioi: no phai doc duoc ke ca khi chay xuyen goc cay hay sau tang da.
        // Shader rieng nay chi khac ParticleAdditive o dong ZTest Always - xem
        // ghi chu day du trong S_ChiBaoNgam.shader.
        //
        // ⚠️ Shader nap luc CHAY nen phai co trong Always Included Shaders cua
        // Graphics Settings, khong thi Editor van dep ma ban build ra mau hong.
        var sh = Shader.Find("Diablo25D/ChiBaoNgam");
        if (sh == null)
        {
            // Bao THAT TO. Duong lui van ve ra hinh dung, chi mat ZTest Always -
            // tuc duong ngam bi cay va da an mat tung khuc. Do la kieu hong RAT
            // de tuong la "hieu ung co y do", nen phai co mot dong do trong
            // Console de con biet duong ma di tim.
            Debug.LogError("[ChiBaoNgam] Khong tim thay shader 'Diablo25D/ChiBaoNgam'. "
                         + "Kiem tra Project Settings > Graphics > Always Included Shaders. "
                         + "Duong ngam se bi vat can che khuat.");
        }
        var mat = new Material(sh != null ? sh : Mats.AddShader);
        mat.name = "P_ChiBaoNgam";
        mat.mainTexture = Texture2D.whiteTexture;
        mat.SetColor("_TintColor", Color.white);
        mat.SetFloat("_Intensity", 1.25f);
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        r.enabled = false;

        return go.AddComponent<ChiBaoNgam>();
    }

    void Awake()
    {
        // CHI lop "Ground" - KHONG lay "Default".
        //
        // Ca 154 goc cay, tang da, bia mo trong man deu nam o lop Default.
        // Cho chung vao mat na thi tia do cao ban trung NOC hon da chu khong
        // phai mat dat, dinh luoi nhay vot len, va duong ngam gay vun ra -
        // leo len than cay roi xoe ngang. Dia hinh that chi co MOT collider,
        // va no o lop Ground.
        if (!maskSan) { groundMask = LayerMask.GetMask("Ground"); maskSan = true; }

        mesh = new Mesh();
        mesh.name = "ChiBaoNgam";
        mesh.MarkDynamic();

        // sharedMesh chu KHONG PHAI mesh.
        //
        // Getter cua .mesh NHAN BAN luoi ra mot ban rieng roi gan ban do vao
        // MeshFilter. Chi can mot ai do doc mf.mesh mot lan - mot doan do, mot
        // dong go loi - la tu do MeshFilter ve ban sao, con o day van sua tiep
        // luoi goc: hinh chi bao DONG BANG o khung dau tien va khong bao gio
        // doi nua. Da vap dung cai nay: bay ky nang deu ve ra cung mot hinh.
        GetComponent<MeshFilter>().sharedMesh = mesh;
        mr = GetComponent<MeshRenderer>();
        mr.enabled = false;
    }

    public void An()
    {
        if (mr != null) mr.enabled = false;
    }

    /// <summary>
    /// Dung lai hinh chi bao cho mot ky nang.
    ///
    /// <paramref name="tu"/> la chan nguoi choi, <paramref name="huong"/> la
    /// huong ngam da chuan hoa, <paramref name="diemNgam"/> la cho phep se roi
    /// xuong (voi don danh theo diem) hoac cuoi duong bay (voi don thang).
    /// </summary>
    public void Hien(int skill, Vector3 tu, Vector3 huong, Vector3 diemNgam,
                     float tamToiDa, float banKinhSatThuong, bool donThang)
    {
        if (mesh == null) return;

        verts.Clear(); cols.Clear(); uvs.Clear(); tris.Clear();

        Color mau = MauKyNang(skill);
        Color mauMo = new Color(mau.r, mau.g, mau.b, 0.16f);
        Color mauDam = new Color(mau.r, mau.g, mau.b, 0.85f);
        Color mauVua = new Color(mau.r, mau.g, mau.b, 0.45f);

        if (skill == 5)
        {
            // KHIENG: khong co huong, chi co mot vom quanh chinh minh.
            ThemDia(tu, banKinhSatThuong, mauMo, 40);
            ThemVanh(tu, banKinhSatThuong, 0.16f, mauDam, 40);
        }
        else if (donThang)
        {
            float dai = Vector3.Distance(new Vector3(tu.x, 0f, tu.z),
                                         new Vector3(diemNgam.x, 0f, diemNgam.z));

            // Mot LAN DUONG rong bang duong kinh vung sat thuong, cho nguoi choi
            // uoc duoc don nay quet ngang bao nhieu - chu khong phai mot soi chi.
            float rong = Mathf.Max(0.45f, banKinhSatThuong * 0.9f);
            ThemDai(tu, huong, 0.9f, dai, rong, mauMo);

            // Truc giua + ba muc ten chong nhau, dung nhu duong ngam trong game
            // MOBA: mot vach thang thi khong doc ra duoc dau la dau mui ten.
            ThemDai(tu, huong, 0.9f, dai, 0.22f, mauDam);
            for (int i = 0; i < 3; i++)
            {
                float t = dai * (0.62f + i * 0.14f);
                ThemChevron(tu + huong * t, huong, rong * 0.95f, 0.75f, 0.20f, mauDam);
            }

            // Rieng QUA CAU LUA ban BA qua toe 11 do - phai ve ca hai qua ben
            // canh, khong thi nguoi choi tuong no chi ban mot duong thang.
            if (skill == 0)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    Vector3 h = Quaternion.AngleAxis(11f * k, Vector3.up) * huong;
                    ThemDai(tu, h, 0.9f, dai, 0.14f, mauVua);
                    ThemVanh(tu + h * dai, banKinhSatThuong, 0.12f, mauVua, 28);
                }
            }

            if (banKinhSatThuong > 0.05f)
            {
                ThemDia(tu + huong * dai, banKinhSatThuong, mauMo, 32);
                ThemVanh(tu + huong * dai, banKinhSatThuong, 0.14f, mauDam, 32);
            }
        }
        else
        {
            // DON THEO DIEM: vanh TAM quanh nguoi choi cho biet keo duoc toi dau,
            // vanh SAT THUONG o cho phep roi.
            ThemVanh(tu, tamToiDa, 0.13f, mauVua, 48);
            ThemDai(tu, huong, 0.9f,
                    Vector3.Distance(new Vector3(tu.x, 0f, tu.z),
                                     new Vector3(diemNgam.x, 0f, diemNgam.z)),
                    0.16f, mauVua);

            ThemDia(diemNgam, banKinhSatThuong, mauMo, 40);
            ThemVanh(diemNgam, banKinhSatThuong, 0.16f, mauDam, 40);

            // Dau cong nho ngay giua diem roi: mot cai vanh khong thi kho nhin
            // ra tam cua no o dau tren nen dat lo cho.
            ThemDai(diemNgam - huong * 0.55f, huong, 0f, 1.1f, 0.11f, mauDam);
            Vector3 ngang = new Vector3(huong.z, 0f, -huong.x);
            ThemDai(diemNgam - ngang * 0.55f, ngang, 0f, 1.1f, 0.11f, mauDam);
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetColors(cols);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        mr.enabled = true;
    }

    /// <summary>
    /// Mau theo NGUYEN TO cua ky nang.
    ///
    /// Khong dung mot mau xanh chung cho ca bay: nguoi choi cam dien thoai
    /// khong nhin nut ma nhin san choi, mau chinh la cach doc nhanh nhat xem
    /// dang cam ky nang nao.
    /// </summary>
    static Color MauKyNang(int skill)
    {
        switch (skill)
        {
            case 0: return new Color(1.00f, 0.55f, 0.18f);   // qua cau lua
            case 1: return new Color(0.45f, 0.85f, 1.00f);   // mua bang
            case 2: return new Color(0.70f, 0.80f, 1.00f);   // sam set
            case 3: return new Color(0.75f, 0.95f, 0.80f);   // loc xoay
            case 4: return new Color(1.00f, 0.42f, 0.12f);   // thien thach
            case 5: return new Color(0.85f, 0.62f, 1.00f);   // khieng
            case 6: return new Color(0.55f, 1.00f, 0.60f);   // giut set
            default: return new Color(0.40f, 0.95f, 1.00f);
        }
    }

    // ================================================================
    //  DUNG HINH
    // ================================================================

    float Cao(float x, float z)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 40f, z), Vector3.down, out hit, 90f,
                            groundMask, QueryTriggerInteraction.Ignore))
            return hit.point.y + lift;
        return lift;
    }

    void ThemDinh(float x, float z, Color c)
    {
        verts.Add(new Vector3(x, Cao(x, z), z));
        cols.Add(c);
        uvs.Add(new Vector2(0.5f, 0.5f));
    }

    void ThemQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color mau)
    {
        int i0 = verts.Count;
        ThemDinh(a.x, a.z, mau);
        ThemDinh(b.x, b.z, mau);
        ThemDinh(c.x, c.z, mau);
        ThemDinh(d.x, d.z, mau);
        tris.Add(i0); tris.Add(i0 + 1); tris.Add(i0 + 2);
        tris.Add(i0); tris.Add(i0 + 2); tris.Add(i0 + 3);
    }

    /// <summary>
    /// Mot DAI phang chay tu <paramref name="tu"/> theo huong, bat dau o
    /// <paramref name="batDau"/> met va dai <paramref name="dai"/> met.
    ///
    /// Chia thanh nhieu doan chu khong mot mieng: mot mieng dai 14 m chi co bon
    /// goc thi tren dia hinh go ghe no cam thang qua doi, nua duong nam duoi
    /// dat con nua kia lo lung tren khong.
    /// </summary>
    void ThemDai(Vector3 tu, Vector3 huong, float batDau, float dai, float rong, Color mau)
    {
        if (dai <= 0.01f) return;

        Vector3 ngang = new Vector3(huong.z, 0f, -huong.x) * (rong * 0.5f);
        int doan = Mathf.Clamp(Mathf.RoundToInt(dai / 1.2f), 1, 24);

        // Dung LIEN MACH: hai doan ke nhau DUNG CHUNG hai dinh giua.
        //
        // Ve tung mieng roi thi moi moc bi ban tia xuong dat HAI lan - mot cho
        // doan truoc, mot cho doan sau. Duong ngam cua Qua cau lua co ba lan
        // chay song song, tuc lang phi hang tram tia moi khung hinh.
        int goc = verts.Count;
        for (int i = 0; i <= doan; i++)
        {
            Vector3 pi = tu + huong * (batDau + dai * i / doan);
            Vector3 tr = pi - ngang, ph = pi + ngang;
            ThemDinh(tr.x, tr.z, mau);
            ThemDinh(ph.x, ph.z, mau);
        }
        for (int i = 0; i < doan; i++)
        {
            int a = goc + i * 2;
            tris.Add(a); tris.Add(a + 1); tris.Add(a + 3);
            tris.Add(a); tris.Add(a + 3); tris.Add(a + 2);
        }
    }

    /// <summary>Mot dau mui ten ">" quay theo huong.</summary>
    void ThemChevron(Vector3 giua, Vector3 huong, float rong, float sau, float day, Color mau)
    {
        Vector3 ngang = new Vector3(huong.z, 0f, -huong.x);

        for (int k = -1; k <= 1; k += 2)
        {
            Vector3 dau = giua + huong * (sau * 0.5f);
            Vector3 duoi = giua - huong * (sau * 0.5f) + ngang * (rong * 0.5f * k);

            Vector3 v = dau - duoi;
            float d = v.magnitude;
            if (d < 0.01f) continue;
            v /= d;
            Vector3 n = new Vector3(v.z, 0f, -v.x) * (day * 0.5f);
            ThemQuad(duoi - n, duoi + n, dau + n, dau - n, mau);
        }
    }

    /// <summary>
    /// Mot VANH tron. Dung lien mach quanh vong, va CHI BAN MOT TIA cho moi nan.
    ///
    /// Cach ngay tho - moi nan mot mieng tu bon dinh rieng - ton gap BON lan so
    /// tia: hai lan vi hai nan ke nhau khong dung chung dinh, hai lan nua vi
    /// dinh trong va dinh ngoai cua cung mot nan cung ban rieng. Ma hai dinh ay
    /// chi cach nhau bang DO DAY vanh (0,12 - 0,16 m), do cao mat dat giua
    /// chung chenh nhau khong den mot centimet.
    /// </summary>
    void ThemVanh(Vector3 tam, float banKinh, float day, Color mau, int nan)
    {
        if (banKinh <= 0.05f) return;

        float r0 = Mathf.Max(0.01f, banKinh - day * 0.5f);
        float r1 = banKinh + day * 0.5f;

        int goc = verts.Count;
        for (int i = 0; i <= nan; i++)
        {
            float a = i * Mathf.PI * 2f / nan;
            Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));

            Vector3 pTrong = tam + d * r0;
            Vector3 pNgoai = tam + d * r1;
            float y = Cao(tam.x + d.x * banKinh, tam.z + d.z * banKinh);

            verts.Add(new Vector3(pTrong.x, y, pTrong.z));
            cols.Add(mau); uvs.Add(new Vector2(0.5f, 0.5f));
            verts.Add(new Vector3(pNgoai.x, y, pNgoai.z));
            cols.Add(mau); uvs.Add(new Vector2(0.5f, 0.5f));
        }
        for (int i = 0; i < nan; i++)
        {
            int a = goc + i * 2;
            tris.Add(a); tris.Add(a + 1); tris.Add(a + 3);
            tris.Add(a); tris.Add(a + 3); tris.Add(a + 2);
        }
    }

    /// <summary>Dia mo danh dau VUNG GAY SAT THUONG.</summary>
    void ThemDia(Vector3 tam, float banKinh, Color mau, int nan)
    {
        if (banKinh <= 0.05f) return;

        int giua = verts.Count;
        ThemDinh(tam.x, tam.z, mau);

        for (int i = 0; i <= nan; i++)
        {
            float a = i * Mathf.PI * 2f / nan;
            ThemDinh(tam.x + Mathf.Cos(a) * banKinh, tam.z + Mathf.Sin(a) * banKinh, mau);
        }
        for (int i = 0; i < nan; i++)
        {
            tris.Add(giua);
            tris.Add(giua + 1 + i);
            tris.Add(giua + 2 + i);
        }
    }
}
