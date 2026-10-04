using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VET NUT DAT PHAT SANG o cho tia Sam set / May giong cham dat (nguoi dung 05/10/2026 chon "vet nut cua mau B"): mat dat nut toa ra tu
/// cho set danh, ke nut sang TRANG NONG -> CAM -> DO SAM roi nguoi han, chi con vet nut den mo dan. Thay vet chay xem 15% cu.
/// Anh dung bang Blender MCP (CongCu/Blender/vet_nut_set.blend, scene VetNutSet: duong cong nut toa tu tam, re nhanh, manh dan, 4 bien
/// the) -> Resources/KyNang/SamSet/VetNutSet.png (trang, alpha = ke nut) + VetNutSetQuang.png (quang loang quanh ke nut). 2 x 2 o.
/// Dia BAM DAT (GroundRing.BuildDisc - chi lop Ground), chon o + xoay tren UV (xoay vat the thi do cao dat lech goc).
/// Hai renderer dung chung luoi: "NutDen" (alpha, mau than) ve truoc, "NutSang" (cong sang) ve sau; mau doi bang MaterialPropertyBlock
/// tren vat lieu dung chung. Tran ToiDaCungLuc vet (Sam set 22 tia / 2,4 s, May giong 36 tia): vet moi gan vet cu &lt; GanGop m thi lam moi
/// vet cu (khong chong them), qua tran thi xoa vet cu nhat.
/// </summary>
public class VetNutSet : MonoBehaviour
{
    public const string AnhNut = "KyNang/SamSet/VetNutSet";
    public const string AnhQuang = "KyNang/SamSet/VetNutSetQuang";
    /// <summary>Ban kinh vet nut so voi ban kinh tia (Sam set 2,1 -> 1,6 m).</summary>
    public const float HeSoBanKinh = 0.75f;
    public const int ToiDaCungLuc = 12;
    public const float GanGop = 0.8f;
    /// <summary>Moc thoi gian (giay): het trang nong, het cam, het do (quang tat), vet den bat dau mo, xoa.</summary>
    public const float GiayTrang = 0.12f, GiayCam = 0.5f, GiayDo = 1.4f, GiayHetQuang = 2.2f, GiayMoDen = 4f, GiaySong = 6f;
    public const float DoDucDen = 0.85f;

    static readonly List<VetNutSet> dangCo = new List<VetNutSet>();
    public static int SoDangCo { get { dangCo.RemoveAll(v => v == null); return dangCo.Count; } }

    static Material matDen, matSang;
    static readonly int idTint = Shader.PropertyToID("_TintColor");

    Mesh luoi;
    Renderer rDen, rSang;
    MaterialPropertyBlock mpb;
    float t;

    static bool NapVatLieu()
    {
        // Kiem bang null cua Unity: vat lieu tao luc Play bi xoa khi thoat Play (bien static van giu)
        if (matDen != null && matSang != null) return true;
        var nut = Resources.Load<Texture2D>(AnhNut);
        var quang = Resources.Load<Texture2D>(AnhQuang);
        if (nut == null || quang == null) { Debug.LogWarning("[VetNutSet] thieu anh " + AnhNut); return false; }
        matDen = Mats.Alpha("P_VetNutDen", nut, new Color(0.05f, 0.035f, 0.03f, DoDucDen));
        matDen.renderQueue = 2950;
        matSang = Mats.Additive("P_VetNutSang", quang, Color.white, 2.2f);
        matSang.renderQueue = 2960;
        return true;
    }

    public static VetNutSet Tao(Vector3 cho, float banKinhTia)
    {
        if (!NapVatLieu()) return null;
        dangCo.RemoveAll(v => v == null);
        // Gan vet cu: lam moi vet ay (set danh lien mot cho - quanh ke dich)
        foreach (var v in dangCo)
        {
            Vector3 d = v.transform.position - cho; d.y = 0f;
            if (d.magnitude < GanGop) { v.t = 0f; return v; }
        }
        if (dangCo.Count >= ToiDaCungLuc)
        {
            VetNutSet cuNhat = dangCo[0];
            foreach (var v in dangCo) if (v.t > cuNhat.t) cuNhat = v;
            dangCo.Remove(cuNhat);
            Destroy(cuNhat.gameObject);
        }

        Vector3 tam = new Vector3(cho.x, TiaBoDat.DatY(cho), cho.z);
        var go = new GameObject("VetNutSet");
        go.transform.position = tam;
        var vn = go.AddComponent<VetNutSet>();
        vn.Dung(tam, banKinhTia * HeSoBanKinh);
        dangCo.Add(vn);
        return vn;
    }

    void Dung(Vector3 tam, float r)
    {
        luoi = new Mesh { name = "VetNutSet" };
        GroundRing.BuildDisc(luoi, tam, r, 24, 3, 0.05f);
        // Chon o (2 x 2) + xoay tren UV
        int o = Random.Range(0, 4);
        Vector2 tamO = new Vector2((o % 2) * 0.5f + 0.25f, (o / 2) * 0.5f + 0.25f);
        float g = Random.Range(0f, Mathf.PI * 2f), c = Mathf.Cos(g), s = Mathf.Sin(g);
        var uv = new List<Vector2>();
        luoi.GetUVs(0, uv);
        for (int i = 0; i < uv.Count; i++)
        {
            Vector2 p = uv[i] - new Vector2(0.5f, 0.5f);
            p = new Vector2(p.x * c - p.y * s, p.x * s + p.y * c);
            uv[i] = tamO + p * 0.5f * 0.97f;
        }
        luoi.SetUVs(0, uv);
        luoi.RecalculateBounds();

        rDen = TaoRenderer("NutDen", matDen);
        rSang = TaoRenderer("NutSang", matSang);
        mpb = new MaterialPropertyBlock();
        ApMau();
    }

    Renderer TaoRenderer(string ten, Material m)
    {
        var go = new GameObject(ten);
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = luoi;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = m;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        return mr;
    }

    /// <summary>Mau quang theo thoi gian: trang nong -> cam -> do sam -> tat.</summary>
    public static Color MauQuang(float t)
    {
        Color trang = new Color(1f, 0.92f, 0.75f, 1f), cam = new Color(1f, 0.45f, 0.10f, 0.95f), doSam = new Color(0.65f, 0.08f, 0.02f, 0.7f);
        if (t < GiayTrang) return trang;
        if (t < GiayCam) return Color.Lerp(trang, cam, (t - GiayTrang) / (GiayCam - GiayTrang));
        if (t < GiayDo) return Color.Lerp(cam, doSam, (t - GiayCam) / (GiayDo - GiayCam));
        Color c = doSam; c.a *= 1f - Mathf.Clamp01((t - GiayDo) / (GiayHetQuang - GiayDo));
        return c;
    }

    /// <summary>Do duc vet nut den theo thoi gian: hien nhanh 0,1 s, giu, mo tu GiayMoDen toi GiaySong.</summary>
    public static float DucDen(float t)
    {
        return DoDucDen * Mathf.Clamp01(t / 0.1f) * (1f - Mathf.Clamp01((t - GiayMoDen) / (GiaySong - GiayMoDen)));
    }

    void ApMau()
    {
        mpb.SetColor(idTint, MauQuang(t));
        rSang.SetPropertyBlock(mpb);
        rSang.enabled = t < GiayHetQuang;
        var d = new Color(0.05f, 0.035f, 0.03f, DucDen(t));
        mpb.SetColor(idTint, d);
        rDen.SetPropertyBlock(mpb);
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= GiaySong) { Destroy(gameObject); return; }
        ApMau();
    }

    /// <summary>Phep thu doc tuoi vet.</summary>
    public float Tuoi { get { return t; } }

    void OnDestroy()
    {
        dangCo.Remove(this);
        if (luoi != null) Destroy(luoi);
    }
}
