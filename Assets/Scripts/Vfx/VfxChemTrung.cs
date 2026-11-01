using UnityEngine;

/// <summary>
/// DON CHEM CUA BO XUONG TRUNG DOI THU (01/11/2026, nguoi dung khoanh anh: bo cac VET DO keo dai bay ra khi Bo xuong chem trung
/// - do la "tia trung don" <see cref="HitBurst"/> mau do sam cua sat thuong Vat ly - va "moi lan chem vao doi thu co hieu ung danh
/// trung va doi thu bi chay mau"). Nguoi dung chon: chay mau CHI HINH ANH (khong tru them mau), co vet mau nho tren dat tan sau vai
/// giay, chem trung KHIEN thi toe tia lua kim loai va khong co mau.
///
///   - Trung nguoi: <see cref="VetCatLoe"/> (nhat cat loe do trang ngang than, xien theo cu chem) + <see cref="MauPhun"/> (giot mau
///     tron meo bay theo huong chem, roi theo trong luc - KHONG keo dai, chinh kieu vet do nguoi dung che) + suong mau + vet mau
///     ban xuong dat + <see cref="ChayMau"/> (nho giot tu nguoi 2 giay, de vet nho duoi chan).
///   - Trung Khien: <see cref="TiaLuaKhien"/> tai cho luoi kiem cham mat vom.
///   - Dang Tang hinh: khong mau (mau bay ra la lo cho dung cua nguoi vo hinh).
///
/// Anh dung bang Blender MCP (CongCu/Blender/chem_bo_xuong.py) -> Resources/KyNang/BoXuong: GiotMau (2x2), BanMau (2x2), VetCat.
/// </summary>
public static partial class VfxFactory
{
    /// <summary>Dem cho phep thu (menu 152).</summary>
    public static int SoLanChemTrung, SoLanChemTrungKhien, SoLanChemTrungAn;

    static Material mGiotMau, mSuongMau, mVetCat, mTiaLuaKhien;

    static Texture2D AnhBoXuong(string ten)
    {
        var t = Resources.Load<Texture2D>("KyNang/BoXuong/" + ten);
        return t != null ? t : TextureFactory.SoftDot(1.5f);
    }

    public static Material GiotMauMat
    {
        get
        {
            // Anh da la do sam (0,3-0,5) - khong nhan toi them: o goc may quay game ban dem 0,75 thi giot chim vao dat (menu 152)
            if (mGiotMau == null) mGiotMau = Mats.Alpha("P_GiotMau", AnhBoXuong("GiotMau"), Color.white);
            return mGiotMau;
        }
    }

    static Material SuongMauMat
    {
        get
        {
            if (mSuongMau == null) mSuongMau = Mats.Alpha("P_SuongMau", TextureFactory.Smoke(), new Color(0.30f, 0.02f, 0.015f, 0.55f));
            return mSuongMau;
        }
    }

    static Material VetCatMat
    {
        get
        {
            if (mVetCat == null) mVetCat = Mats.Additive("P_VetCat", AnhBoXuong("VetCat"), Color.white, 1.6f);
            return mVetCat;
        }
    }

    static Material TiaLuaKhienMat
    {
        get
        {
            if (mTiaLuaKhien == null) mTiaLuaKhien = Mats.Additive("P_TiaLuaKhien", TextureFactory.SoftDot(2.4f), new Color(1f, 0.82f, 0.45f, 1f), 2.6f);
            return mTiaLuaKhien;
        }
    }

    /// <summary>
    /// Don chem can chien trung <paramref name="nan"/>. <paramref name="tuCho"/> = vi tri ke chem (goc). <paramref name="tre"/> > 0:
    /// hen sau chung ay giay (may khach: hoat hinh ra don bat dau luc nhan goi tin, luoi kiem toi noi sau thoi gian go suc).
    /// </summary>
    public static void ChemTrung(Damageable nan, Vector3 tuCho, float tre = 0f)
    {
        if (nan == null) return;
        if (tre > 0.001f)
        {
            var hen = new GameObject("HenChemTrung").AddComponent<HenChemTrung>();
            hen.nan = nan; hen.tuCho = tuCho; hen.conLai = tre;
            return;
        }
        Vector3 diem = nan.rig != null && nan.rig.hitPoint != null ? nan.rig.hitPoint.position
                     : nan.transform.position + Vector3.up * 1.1f;
        Vector3 huong = nan.transform.position - tuCho; huong.y = 0f;
        if (huong.sqrMagnitude < 1e-4f) huong = nan.transform.forward;
        huong.Normalize();
        SoLanChemTrung++;

        if (nan.khieng != null && nan.khieng.DangBat)
        {
            Vector3 cham;
            float s;
            Vector3 tu = tuCho + Vector3.up * 1.1f;
            if (!nan.khieng.CatVom(tu, diem, out cham, out s))
                cham = nan.khieng.transform.position + Vector3.up * 1.1f - huong * nan.khieng.banKinh;
            TiaLuaKhien(cham, -huong);
            SoLanChemTrungKhien++;
            return;
        }
        if (TangHinh.Dang(nan)) { SoLanChemTrungAn++; return; }

        VetCatLoe(diem - huong * 0.15f, huong);
        MauPhun(diem, huong);
        ChayMau.Bat(nan, huong);
    }

    /// <summary>Nhat cat loe: luoi liem mong do trang ngang than, xien nhu cu bo kiem cheo xuong, 0,16 giay.</summary>
    public static GameObject VetCatLoe(Vector3 cho, Vector3 huong)
    {
        var go = new GameObject("VetCatBoXuong");
        go.transform.position = cho;
        AutoDestroy.Add(go, 0.5f);
        var ps = NewPS("VetCat", go.transform, Vector3.zero, VetCatMat, ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.duration = 0.1f; m.loop = false;
        m.startLifetime = 0.16f;
        m.startSpeed = 0f;
        m.startSize3D = true;
        m.startSizeX = new ParticleSystem.MinMaxCurve(1.05f, 1.35f);
        m.startSizeY = new ParticleSystem.MinMaxCurve(0.42f, 0.55f);
        m.startSizeZ = 1f;
        // Xien cheo xuong (cu chem tu tren vai xuong), ngau nhien ±12 do
        m.startRotation = new ParticleSystem.MinMaxCurve(-38f * Mathf.Deg2Rad, -14f * Mathf.Deg2Rad);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission; em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var sh = ps.shape; sh.enabled = false;
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.45f, 0.35f), 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.35f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.55f), new Keyframe(0.25f, 1f), new Keyframe(1f, 1.08f)));
        return go;
    }

    /// <summary>
    /// Mau phun theo huong chem: 17 giot (anh GiotMau 2x2, quay ngau nhien, KHONG keo dai), roi theo trong luc, cham dat thi mat;
    /// 3 dam suong mau mong tan nhanh; 1-2 vet mau ban xuong dat phia sau nguoi bi chem.
    /// </summary>
    public static GameObject MauPhun(Vector3 cho, Vector3 huong)
    {
        var go = new GameObject("MauPhunBoXuong");
        go.transform.position = cho;
        go.transform.rotation = Quaternion.LookRotation(Vector3.Slerp(huong, Vector3.up, 0.30f));
        AutoDestroy.Add(go, 1.4f);

        var ps = NewPS("GiotMau", go.transform, Vector3.zero, GiotMauMat, ParticleSystemRenderMode.Billboard);
        var m = ps.main;
        m.duration = 0.08f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 4.2f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.19f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m.gravityModifier = 1.6f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 24;
        var em = ps.emission; em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 12), new ParticleSystem.Burst(0.04f, 5) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 32f; sh.radius = 0.12f;
        var tsa = ps.textureSheetAnimation; tsa.enabled = true;
        tsa.numTilesX = 2; tsa.numTilesY = 2;
        tsa.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 3.99f);
        var sz = ps.sizeOverLifetime; sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.65f)));
        // Cham dat (lop Ground) thi mat - khong chui xuong dat
        var col = ps.collision; col.enabled = true;
        col.type = ParticleSystemCollisionType.World;
        col.collidesWith = LayerMask.GetMask("Ground");
        col.lifetimeLoss = 1f; col.bounce = 0f;
        col.quality = ParticleSystemCollisionQuality.Low;

        var ps2 = NewPS("SuongMau", go.transform, Vector3.zero, SuongMauMat, ParticleSystemRenderMode.Billboard);
        var m2 = ps2.main;
        m2.duration = 0.05f; m2.loop = false;
        m2.startLifetime = new ParticleSystem.MinMaxCurve(0.30f, 0.45f);
        m2.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        m2.startSize = new ParticleSystem.MinMaxCurve(0.38f, 0.58f);
        m2.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        m2.simulationSpace = ParticleSystemSimulationSpace.World;
        var em2 = ps2.emission; em2.rateOverTime = 0f;
        em2.SetBursts(new[] { new ParticleSystem.Burst(0f, 3) });
        var sh2 = ps2.shape; sh2.shapeType = ParticleSystemShapeType.Cone; sh2.angle = 25f; sh2.radius = 0.08f;
        var sz2 = ps2.sizeOverLifetime; sz2.enabled = true;
        sz2.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(1f, 1.6f)));
        var c2 = ps2.colorOverLifetime; c2.enabled = true;
        var g2 = new Gradient();
        g2.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                   new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        c2.color = g2;

        // Vet ban xuong dat: phia sau nguoi bi chem (mau bay theo huong chem)
        Vector3 ngang = new Vector3(-huong.z, 0f, huong.x);
        int so = Random.value < 0.5f ? 1 : 2;
        for (int i = 0; i < so; i++)
        {
            Vector3 p = cho + huong * Random.Range(0.35f, 1.0f) + ngang * Random.Range(-0.35f, 0.35f);
            VetMauDat.Tao(p, Random.Range(0.40f, 0.65f));
        }
        return go;
    }

    /// <summary>Kiem cham mat vom Khien: tia lua kim loai vang trang toe nguoc lai + chop nho. Khong co mau.</summary>
    public static GameObject TiaLuaKhien(Vector3 cho, Vector3 huongRa)
    {
        var go = new GameObject("TiaLuaKhienBoXuong");
        go.transform.position = cho;
        go.transform.rotation = Quaternion.LookRotation(Vector3.Slerp(huongRa, Vector3.up, 0.25f));
        AutoDestroy.Add(go, 0.8f);

        var ps = NewPS("TiaLua", go.transform, Vector3.zero, TiaLuaKhienMat, ParticleSystemRenderMode.Stretch);
        var r = ps.GetComponent<ParticleSystemRenderer>(); r.velocityScale = 0.05f; r.lengthScale = 1.6f;
        var m = ps.main;
        m.duration = 0.05f; m.loop = false;
        m.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        m.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6.5f);
        m.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.05f);
        m.gravityModifier = 1.3f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission; em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 55f; sh.radius = 0.05f;
        var c = ps.colorOverLifetime; c.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.55f, 0.2f), 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        c.color = g;

        var ps2 = NewPS("ChopKhien", go.transform, Vector3.zero, TiaLuaKhienMat, ParticleSystemRenderMode.Billboard);
        var m2 = ps2.main;
        m2.duration = 0.05f; m2.loop = false;
        m2.startLifetime = 0.09f; m2.startSpeed = 0f; m2.startSize = 0.55f;
        m2.simulationSpace = ParticleSystemSimulationSpace.World;
        var em2 = ps2.emission; em2.rateOverTime = 0f;
        em2.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var sh2 = ps2.shape; sh2.enabled = false;
        var c2 = ps2.colorOverLifetime; c2.enabled = true;
        var g2 = new Gradient();
        g2.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                   new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        c2.color = g2;
        return go;
    }
}

/// <summary>Hen hien don chem trung sau mot luc (may khach - xem <see cref="VfxFactory.ChemTrung"/>).</summary>
public class HenChemTrung : MonoBehaviour
{
    public Damageable nan;
    public Vector3 tuCho;
    public float conLai;

    void Update()
    {
        conLai -= Time.deltaTime;
        if (conLai > 0f) return;
        if (nan != null) VfxFactory.ChemTrung(nan, tuCho);
        Destroy(gameObject);
    }
}
