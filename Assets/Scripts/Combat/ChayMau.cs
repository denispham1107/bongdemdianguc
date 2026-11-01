using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CHAY MAU sau don chem cua Bo xuong (01/11/2026, nguoi dung: "doi thu bi chay mau"; chon CHI HINH ANH - khong tru them mau).
///
/// Moi lan trung (<see cref="Bat"/>) dat lai dong ho <see cref="GiayChayMau"/>: trong luc ay mau NHO GIOT tu than (he hat bam xuong
/// trung don, mo phong THE GIOI de giot roi thang xuong khi nguoi chay), cu <see cref="NhipVetDat"/> giay de mot vet mau nho duoi chan.
/// Het gio thi ngung phun, doi giot cuoi roi xong roi tu go. Chet / Tang hinh thi ngung ngay.
/// </summary>
public class ChayMau : MonoBehaviour
{
    public const float GiayChayMau = 2f;
    public const float GiotMoiGiay = 9f;
    public const float NhipVetDat = 0.55f;

    /// <summary>Dem cho phep thu (menu 152).</summary>
    public static int SoLanBat;

    Damageable d;
    ParticleSystem giot;
    float conLai, demVet;

    public float ConLai { get { return conLai; } }

    public static ChayMau Bat(Damageable nan, Vector3 huong)
    {
        if (nan == null) return null;
        var c = nan.GetComponent<ChayMau>();
        if (c == null) { c = nan.gameObject.AddComponent<ChayMau>(); c.d = nan; c.TaoGiot(); }
        c.conLai = GiayChayMau;
        if (c.giot != null && !c.giot.isEmitting) c.giot.Play(true);
        SoLanBat++;
        return c;
    }

    void TaoGiot()
    {
        Transform neo = d.rig != null && d.rig.hitPoint != null ? d.rig.hitPoint : transform;
        var go = new GameObject("MauNhoGiot");
        go.transform.SetParent(neo, false);
        go.transform.localPosition = neo == transform ? Vector3.up * 1.1f : Vector3.zero;
        go.layer = gameObject.layer;
        giot = go.AddComponent<ParticleSystem>();
        giot.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material = VfxFactory.GiotMauMat;
        r.sortingFudge = -5f;
        var m = giot.main;
        m.loop = true; m.duration = 1f;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.9f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.6f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.075f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.gravityModifier = 1.1f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 16;
        var em = giot.emission; em.rateOverTime = GiotMoiGiay;
        var sh = giot.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.16f;
        var tsa = giot.textureSheetAnimation; tsa.enabled = true;
        tsa.numTilesX = 2; tsa.numTilesY = 2;
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
        var col = giot.collision; col.enabled = true;
        col.type = ParticleSystemCollisionType.World;
        col.collidesWith = LayerMask.GetMask("Ground");
        col.lifetimeLoss = 1f; col.bounce = 0f;
        col.quality = ParticleSystemCollisionQuality.Low;
        giot.Play(true);
    }

    void Update()
    {
        if (d == null) { Destroy(this); return; }
        bool dung = d.IsDead || TangHinh.Dang(d);
        if (dung) conLai = 0f;

        if (conLai > 0f)
        {
            conLai -= Time.deltaTime;
            demVet -= Time.deltaTime;
            if (demVet <= 0f)
            {
                demVet = NhipVetDat * Random.Range(0.8f, 1.25f);
                Vector2 lech = Random.insideUnitCircle * 0.35f;
                VetMauDat.Tao(transform.position + new Vector3(lech.x, 0f, lech.y), Random.Range(0.16f, 0.28f));
            }
            return;
        }

        // Het gio: ngung phun, doi giot cuoi roi het roi go (Tang hinh: xoa ngay - giot lo cho dung)
        if (giot != null)
        {
            if (giot.isEmitting) giot.Stop(false, dung ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
            if (giot.particleCount > 0) return;
            Destroy(giot.gameObject);
        }
        Destroy(this);
    }

    void OnDestroy()
    {
        if (giot != null) Destroy(giot.gameObject);
    }
}

/// <summary>
/// VET MAU BAN tren mat dat (anh BanMau 2x2, Blender MCP): dia bam CHI lop Ground (<see cref="GroundRing.BuildDisc"/>), xoay ngau
/// nhien, giu <see cref="GiayGiu"/> giay roi mo trong <see cref="GiayMo"/> giay. Toi da <see cref="ToiDa"/> vet ca tran (dien thoai)
/// - vuot thi xoa vet cu nhat.
/// </summary>
public class VetMauDat : MonoBehaviour
{
    public const float GiayGiu = 4f, GiayMo = 2f;
    public const int ToiDa = 24;

    /// <summary>Dem cho phep thu (menu 152).</summary>
    public static int SoVetDaTao;
    static readonly List<VetMauDat> dangCo = new List<VetMauDat>();
    static Material vatLieu;
    static Texture2D anh;
    static readonly int idTint = Shader.PropertyToID("_TintColor");
    /// <summary>Nhan toi nhu vung mau XacNam (sang hon thi giua dem do tuoi nhu phat sang).</summary>
    static readonly Color MauVet = new Color(0.6f, 0.6f, 0.6f, 0.92f);

    MeshRenderer mr;
    MaterialPropertyBlock mpb;
    Mesh luoi;
    float tuoi;

    public static int SoDangCo { get { dangCo.RemoveAll(v => v == null); return dangCo.Count; } }

    public static VetMauDat Tao(Vector3 cho, float banKinh)
    {
        dangCo.RemoveAll(v => v == null);
        while (dangCo.Count >= ToiDa) { var cu = dangCo[0]; dangCo.RemoveAt(0); if (cu != null) Destroy(cu.gameObject); }

        float datY = VfxFactory.GroundY(cho);
        Vector3 tam = new Vector3(cho.x, datY, cho.z);
        var go = new GameObject("VetMauDat");
        go.transform.position = tam;
        var v = go.AddComponent<VetMauDat>();
        v.luoi = new Mesh { name = "VetMauDat" };
        GroundRing.BuildDisc(v.luoi, tam, banKinh, 12, 2, 0.025f);
        // Chon mot trong 4 o anh + xoay ngau nhien quanh tam o (dia ban kinh 0,5 trong UV nen xoay khong lan sang o ben)
        int o = Random.Range(0, 4);
        Vector2 goc = new Vector2((o % 2) * 0.5f, (o / 2) * 0.5f);
        float a = Random.Range(0f, Mathf.PI * 2f), ca = Mathf.Cos(a), sa = Mathf.Sin(a);
        var uv = v.luoi.uv;
        for (int i = 0; i < uv.Length; i++)
        {
            Vector2 p = uv[i] - new Vector2(0.5f, 0.5f);
            p = new Vector2(p.x * ca - p.y * sa, p.x * sa + p.y * ca);
            uv[i] = goc + (p + new Vector2(0.5f, 0.5f)) * 0.5f;
        }
        v.luoi.uv = uv;
        go.AddComponent<MeshFilter>().sharedMesh = v.luoi;
        v.mr = go.AddComponent<MeshRenderer>();
        v.mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        v.mr.receiveShadows = false;
        v.mr.sharedMaterial = LayVatLieu();
        v.mpb = new MaterialPropertyBlock();
        v.DatMo(1f);
        dangCo.Add(v);
        SoVetDaTao++;
        return v;
    }

    static Material LayVatLieu()
    {
        if (vatLieu == null)
        {
            if (anh == null)
            {
                anh = Resources.Load<Texture2D>("KyNang/BoXuong/BanMau");
                if (anh == null) anh = TextureFactory.SoftDot(1.5f);
            }
            vatLieu = Mats.Alpha("M_VetMauDat", anh, MauVet);
        }
        return vatLieu;
    }

    void DatMo(float k)
    {
        mpb.SetColor(idTint, new Color(MauVet.r, MauVet.g, MauVet.b, MauVet.a * k));
        mr.SetPropertyBlock(mpb);
    }

    void Update()
    {
        tuoi += Time.deltaTime;
        if (tuoi >= GiayGiu + GiayMo) { Destroy(gameObject); return; }
        if (tuoi > GiayGiu) DatMo(1f - (tuoi - GiayGiu) / GiayMo);
    }

    void OnDestroy()
    {
        dangCo.Remove(this);
        if (luoi != null) Destroy(luoi);
    }
}
