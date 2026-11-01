using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HINH CU LUOT BO XUONG (nguoi dung 01/11/2026, chon "bong mo + vet hon xanh"): trong luc luot de lai cac BONG MO (chup luoi BakeMesh dung tu the, ve bang shader vien
/// fresnel cua Tang hinh mau xanh lanh, mo dan), VET HON xanh sau lung (TrailRenderer, anh Blender MCP KyNang/BoXuong/VetHon) va bui dat o chan.
///
/// Gan vao mot vat rieng bam theo con quai (khong lam con quai: nhat chem tiep theo khong can biet gi toi no). Toi da <see cref="TranCoBong"/> cu luot cung luc co bong mo
/// (ca bay Bo xuong cung lao vao thi BakeMesh moi con moi 0,04 s la phi tren dien thoai); cu luot khac van co vet + bui.
/// Luoi bong mo giu lai dung lai giua cac lan (khong tao Mesh moi moi nhip).
/// </summary>
public class HinhLuotBoXuong : MonoBehaviour
{
    public const int TranCoBong = 4;
    const float NhipBong = 0.04f, GiayBongMo = 0.24f, GiayTuTat = EnemyAI.GiayLuotToiDa + 0.1f;
    static readonly Color MauVien = new Color(0.35f, 0.72f, 1f, 1f), MauSau = new Color(0.04f, 0.18f, 0.40f, 1f);

    /// <summary>Dem cho phep thu (menu 158).</summary>
    public static int SoBongDaSinh, SoCuLuotDangCoBong;

    class Bong { public GameObject go; public Mesh mesh; public Material vl; public float tuoi; }

    static readonly Stack<Mesh> khoLuoi = new Stack<Mesh>();
    readonly List<Bong> bong = new List<Bong>();
    SkinnedMeshRenderer[] da;
    Transform quai;
    TrailRenderer vet;
    ParticleSystem bui;
    float henBong, tuoi;
    bool dangChay = true, duocBong;

    public static HinhLuotBoXuong Tao(Transform quai, Material vatLieuVet, Gradient mauVet, ParticleSystem bui)
    {
        var go = new GameObject("LuotBoXuong");
        go.transform.position = quai.position + Vector3.up * 1f;
        var h = go.AddComponent<HinhLuotBoXuong>();
        h.quai = quai;
        h.da = quai.GetComponentsInChildren<SkinnedMeshRenderer>(false);
        h.duocBong = SoCuLuotDangCoBong < TranCoBong;
        if (h.duocBong) SoCuLuotDangCoBong++;

        var vet = go.AddComponent<TrailRenderer>();
        vet.sharedMaterial = vatLieuVet;
        vet.time = 0.42f;      // cu luot chi ~0,15 s: vet phai o lai sau khi toi noi moi kip thay
        vet.minVertexDistance = 0.15f;
        vet.widthCurve = new AnimationCurve(new Keyframe(0f, 1.7f), new Keyframe(1f, 0.25f));
        vet.colorGradient = mauVet;
        vet.alignment = LineAlignment.View;
        vet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        vet.receiveShadows = false;
        h.vet = vet;

        if (bui != null) { bui.transform.SetParent(go.transform, false); bui.transform.localPosition = Vector3.down * 0.9f; }
        h.bui = bui;
        return h;
    }

    /// <summary>Het cu luot: ngung sinh, bong + vet mo not roi tu xoa.</summary>
    public void Dung()
    {
        if (!dangChay) return;
        dangChay = false;
        if (duocBong) { duocBong = false; SoCuLuotDangCoBong = Mathf.Max(0, SoCuLuotDangCoBong - 1); }
        if (bui != null) bui.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void SinhBong()
    {
        foreach (var smr in da)
        {
            if (smr == null) continue;
            var m = khoLuoi.Count > 0 ? khoLuoi.Pop() : new Mesh { name = "BongLuot" };
            smr.BakeMesh(m, true);
            var go = new GameObject("BongLuot");
            go.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
            go.AddComponent<MeshFilter>().sharedMesh = m;
            var r = go.AddComponent<MeshRenderer>();
            var vl = new Material(Mats.TangHinhShader);
            vl.SetColor("_Color", MauVien);
            vl.SetColor("_MauSau", MauSau);
            vl.SetFloat("_RimPower", 0.9f);      // thap: ca than sang mem - 1,8 chi con vien, luoi Meshy gan guoc thanh lam tam
            vl.SetFloat("_VetDam", 0f);
            r.sharedMaterial = vl;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            bong.Add(new Bong { go = go, mesh = m, vl = vl });
            SoBongDaSinh++;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        tuoi += dt;
        if (dangChay && (quai == null || tuoi >= GiayTuTat)) Dung();

        if (dangChay)
        {
            transform.position = quai.position + Vector3.up * 1f;
            henBong -= dt;
            if (duocBong && henBong <= 0f) { henBong = NhipBong; SinhBong(); }
        }

        for (int i = bong.Count - 1; i >= 0; i--)
        {
            var b = bong[i];
            b.tuoi += dt;
            float t = b.tuoi / GiayBongMo;
            if (t >= 1f)
            {
                Destroy(b.go); Destroy(b.vl);
                if (b.mesh != null) khoLuoi.Push(b.mesh);
                bong.RemoveAt(i);
                continue;
            }
            b.vl.SetFloat("_Amount", 1f - t);
        }

        if (!dangChay && bong.Count == 0 && tuoi >= GiayTuTat + 0.6f) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (duocBong) SoCuLuotDangCoBong = Mathf.Max(0, SoCuLuotDangCoBong - 1);
        foreach (var b in bong)
        {
            if (b.go != null) Destroy(b.go);
            if (b.vl != null) Destroy(b.vl);
            if (b.mesh != null) khoLuoi.Push(b.mesh);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void DatLai() { SoCuLuotDangCoBong = 0; khoLuoi.Clear(); }
}
