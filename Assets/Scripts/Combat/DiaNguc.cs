using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DIA NGUC NGOAI BAN DO (nguoi dung 05/10/2026: "ve thiet ke them cho ben duoi ngoai vung ban do la dia nguc, khi nguoi choi bi te nga xuong
/// la bi mat mau cho den chet"; chon: VUC THAM + DUNG NHAM, mo vai doan rao SAP, 20% mau toi da / giay, quai cung vay).
///
/// Phan LUAT CHOI (file nay): ai roi xuong duoi <see cref="NguongRoi"/> (dat thap nhat trong Act2 la -2,9 m) la da xuong vuc - cu
/// <see cref="Nhip"/> giay mat <see cref="TiLeMatMauMoiGiay"/> x Nhip mau TOI DA cho den chet, ca nguoi choi lan quai.
///   - Sat thuong VAT LY (khong khang nao giam), danh dau sat thuong RI (Bo xuong khong do don duoc), khong phun tia trung don.
///   - Chi may QUYET MAU tru mau (nguoi choi: may cua chinh ho; quai: chu phong) - ban sao (mauDoMayKhacQuyet) bo qua.
///   - Ai danh nan nhan GAN NHAT truoc luc roi (keDanhCuoi chup lai luc vua roi qua nguong) duoc tinh la ke ha - day nguoi / quai xuong
///     vuc van duoc kinh nghiem, bang diem.
///   - Day vuc: mot tam va cham lop Ground o <see cref="MucDungNham"/> (nguoi / quai roi cham day, xac nam tren do, vung mau co cho bam).
/// Phan HINH (vach vuc, bien dung nham, rao sap) dung bang Blender MCP - xem HUONG-DAN.
/// </summary>
public class DiaNguc : MonoBehaviour
{
    /// <summary>Mat dung nham (m, the gioi) - mat dat Act2 quanh 0 (-2,9 .. 2,9).</summary>
    public const float MucDungNham = -26f;
    /// <summary>Roi xuong duoi do cao nay (m) = da xuong vuc.</summary>
    public const float NguongRoi = -6f;
    /// <summary>Mat bao nhieu phan mau toi da moi giay (nguoi dung chon 20% -> ~5 giay).</summary>
    public const float TiLeMatMauMoiGiay = 0.20f;
    /// <summary>Nhip gay sat thuong (giay).</summary>
    public const float Nhip = 0.25f;

    static DiaNguc ins;
    readonly Dictionary<Damageable, Damageable> keDay = new Dictionary<Damageable, Damageable>();
    readonly List<Damageable> boKhoi = new List<Damageable>();
    float hen;

    /// <summary>Phep thu doc: so ke da roi xuong vuc (tinh tu luc vao tran).</summary>
    public static int SoLanRoi { get; private set; }

    /// <summary>Dung dia nguc cho tran nay (GameBootstrap.Awake). Goi lai thi bo qua.</summary>
    public static DiaNguc Dung()
    {
        if (ins != null) return ins;
        SoLanRoi = 0;
        var go = new GameObject("DiaNguc");
        ins = go.AddComponent<DiaNguc>();
        // Day vuc: tam va cham rong (ban do 135 m, vuc toa ra ngoai), mat tren thap hon mat dung nham 0,5 m (chim nua bap chan)
        var day = new GameObject("DayDungNham");
        day.transform.SetParent(go.transform, false);
        int lop = LayerMask.NameToLayer("Ground");
        if (lop >= 0) day.layer = lop;
        var bc = day.AddComponent<BoxCollider>();
        bc.size = new Vector3(600f, 2f, 600f);
        bc.center = new Vector3(0f, MucDungNham - 0.5f - 1f, 0f);
        DungHinh(go.transform);
        return ins;
    }

    /// <summary>Be rong tam dung nham (m) - phu het tam nhin xuong vuc (ban do 135 m, vach ngoai ~112 m).</summary>
    public const float CoDungNham = 480f;

    /// <summary>
    /// HINH dia nguc (Blender MCP, CongCu/Blender/dia_nguc.blend -> Resources/DiaNguc): vach vuc quanh mep dat (mep tren khop do cao dat Act2),
    /// vach ngoai quay vao trong cach mep ~45 m, rao sap (lan can do + da vun) o 4 khe rao; bien dung nham + tan lua,
    /// khoi do sam boc len doc 4 canh. Nap luc chay - KHONG sua scene. Luoi FBX xuat cung cach voi hang_rao_rong (goc xoay 270 / ti le 100),
    /// nen dat o goc toa do la khop dung cho.
    /// </summary>
    static void DungHinh(Transform cha)
    {
        var pf = Resources.Load<GameObject>("DiaNguc/DiaNguc");
        var matVach = Resources.Load<Material>("DiaNguc/VachDiaNguc");
        if (pf != null)
        {
            var hinh = Instantiate(pf, cha);
            hinh.name = "HinhDiaNguc";
            hinh.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            // Rao sap dung DUNG vat lieu cua hang rao trong canh (da triplanar + sat)
            Material daRao = null, satRao = null;
            var rao = GameObject.Find("PERIMETER_FENCE_681");
            var rr = rao != null ? rao.GetComponent<Renderer>() : null;
            if (rr != null)
                foreach (var m in rr.sharedMaterials)
                {
                    if (m == null) continue;
                    if (m.name.Contains("DaBia")) daRao = m; else if (m.name.Contains("Sat")) satRao = m;
                }
            foreach (var r in hinh.GetComponentsInChildren<MeshRenderer>())
            {
                if (r.name == "RaoSap")
                {
                    var ms = r.sharedMaterials;
                    for (int i = 0; i < ms.Length; i++)
                    {
                        if (ms[i] != null && ms[i].name.Contains("stone") && daRao != null) ms[i] = daRao;
                        else if (ms[i] != null && ms[i].name.Contains("iron") && satRao != null) ms[i] = satRao;
                    }
                    r.sharedMaterials = ms;
                }
                else
                {
                    if (matVach != null) r.sharedMaterial = matVach;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = false;
                }
            }
        }

        // Bien dung nham
        var matDN = Resources.Load<Material>("DiaNguc/DungNham");
        var dn = GameObject.CreatePrimitive(PrimitiveType.Quad);
        dn.name = "BienDungNham";
        Destroy(dn.GetComponent<Collider>());
        dn.transform.SetParent(cha, false);
        dn.transform.position = new Vector3(0f, MucDungNham, 0f);
        dn.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        dn.transform.localScale = new Vector3(CoDungNham, CoDungNham, 1f);
        var dr = dn.GetComponent<MeshRenderer>();
        if (matDN != null) dr.sharedMaterial = matDN;
        dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dr.receiveShadows = false;

        // Hai lop SUONG KHOI do sam lo lung giua vuc: dung nham nhin qua khoi thanh xa / sau (anh lan dau: sang deu sat chan rao nhu tam tham)
        var matSuong = Resources.Load<Material>("DiaNguc/SuongVuc");
        if (matSuong != null)
            foreach (var lop in new[] { new Vector2(-12f, 0.55f), new Vector2(-19f, 0.4f) })
            {
                var s = GameObject.CreatePrimitive(PrimitiveType.Quad);
                s.name = "SuongVuc";
                Destroy(s.GetComponent<Collider>());
                s.transform.SetParent(cha, false);
                s.transform.position = new Vector3(0f, lop.x, 0f);
                s.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                s.transform.localScale = new Vector3(CoDungNham, CoDungNham, 1f);
                var sr = s.GetComponent<MeshRenderer>();
                var m = new Material(matSuong); m.SetFloat("_Do", lop.y);
                if (lop.x < -15f) m.SetVector("_TroiA", new Vector4(-0.25f, 0.18f, 0f, 0f));
                sr.sharedMaterial = m;
                sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sr.receiveShadows = false;
            }

        // Tan lua + khoi do sam boc len doc 4 canh, trong long vuc
        for (int k = 0; k < 4; k++)
        {
            float goc = k * 90f;
            Vector3 huong = Quaternion.Euler(0f, goc, 0f) * Vector3.forward;   // huong ra ngoai
            Vector3 tam = huong * (MepDat + 14f) + Vector3.up * (MucDungNham + 0.5f);
            TaoTanLua(cha, tam, goc);
            TaoKhoi(cha, tam, goc);
        }
    }

    /// <summary>Nua canh dat Act2 (m) - vach vuc bat dau tu day.</summary>
    public const float MepDat = 67.31f;

    /// <summary>
    /// Tan lua / khoi boc len tu dung nham MOI CANH (nguoi dung 05/10/2026: "cho dung nham va khoi hieu ung bay len nhieu 1 chut" - x1,5 so
    /// voi ban dau 32 / 4,5, bay nhanh hon chut de len gan mieng vuc). Toc len (m/s) lay ngau nhien trong khoang.
    /// </summary>
    public const float TanLuaMoiGiay = 48f, KhoiMoiGiay = 7f;
    public static readonly Vector2 TocLenTanLua = new Vector2(2.0f, 4.5f), TocLenKhoi = new Vector2(1.2f, 2.4f);

    static void TaoTanLua(Transform cha, Vector3 tam, float goc)
    {
        var go = new GameObject("TanLuaVuc");
        go.transform.SetParent(cha, false);
        go.transform.SetPositionAndRotation(tam, Quaternion.Euler(0f, goc, 0f));
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.loop = true; m.prewarm = true; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.12f, 1f), new Color(1f, 0.85f, 0.4f, 1f));
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 420;
        var e = ps.emission; e.rateOverTime = TanLuaMoiGiay;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(150f, 1f, 22f);
        var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
        v.x = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f); v.y = new ParticleSystem.MinMaxCurve(TocLenTanLua.x, TocLenTanLua.y); v.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
        var nz = ps.noise; nz.enabled = true; nz.strength = 0.8f; nz.frequency = 0.35f;
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.3f, 0.05f), 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = VfxFactory.EmberMat;
        r.renderMode = ParticleSystemRenderMode.Billboard;
        ps.Play();
    }

    static void TaoKhoi(Transform cha, Vector3 tam, float goc)
    {
        var mat = VfxFactory.KhoiCuonMat;
        if (mat == null) return;
        var go = new GameObject("KhoiVuc");
        go.transform.SetParent(cha, false);
        go.transform.SetPositionAndRotation(tam, Quaternion.Euler(0f, goc, 0f));
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.loop = true; m.prewarm = true; m.playOnAwake = true;
        m.startLifetime = new ParticleSystem.MinMaxCurve(8f, 12f);
        m.startSpeed = 0f;
        m.startSize = new ParticleSystem.MinMaxCurve(5f, 9f);
        m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        // khoi xam den duoc dung nham ben duoi hat do len
        m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.30f, 0.07f, 0.03f, 0.30f), new Color(0.16f, 0.05f, 0.04f, 0.42f));
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 100;
        var e = ps.emission; e.rateOverTime = KhoiMoiGiay;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(150f, 1f, 22f);
        var v = ps.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
        v.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f); v.y = new ParticleSystem.MinMaxCurve(TocLenKhoi.x, TocLenKhoi.y); v.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        var sol = ps.sizeOverLifetime; sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.8f)));
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0.7f, 0.65f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var rot = ps.rotationOverLifetime; rot.enabled = true; rot.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.maxParticleSize = 10f;
        VfxFactory.BatFlipbook(ps, 6, 6, 1);
        ps.Play();
    }

    /// <summary>Damageable nay da roi xuong vuc chua.</summary>
    public static bool DaRoi(Damageable d) { return d != null && d.transform.position.y < NguongRoi; }

    void Update()
    {
        hen -= Time.deltaTime;
        if (hen > 0f) return;
        hen += Nhip;
        if (hen < 0f) hen = Nhip;

        // Ai da ra khoi vuc (Toc bien nguoc len, bi xoa) thi quen "ke day xuong" - lan roi sau chup lai tu dau
        boKhoi.Clear();
        foreach (var kv in keDay) if (kv.Key == null || !DaRoi(kv.Key)) boKhoi.Add(kv.Key);
        foreach (var k in boKhoi) keDay.Remove(k);

        foreach (var d in FindObjectsByType<Damageable>(FindObjectsSortMode.None))
        {
            if (d == null || d.IsDead || !DaRoi(d)) continue;
            Damageable ke;
            if (!keDay.TryGetValue(d, out ke))
            {
                // Vua roi qua nguong: chup lai ai danh no gan nhat (nguoi day xuong)
                ke = d.keDanhCuoi;
                keDay[d] = ke;
                SoLanRoi++;
            }
            if (d.mauDoMayKhacQuyet) continue;   // ban sao: may quyet mau tu tru roi gui sang

            if (ke != null && ke != d) d.GhiKeDanh(ke);
            Damageable.LaSatThuongRi = true;
            Damageable.BoQuaTiaTrungDon = true;
            try { d.TakeDamage(d.maxHealth * TiLeMatMauMoiGiay * Nhip, DamageType.Physical, d.transform.position); }
            finally { Damageable.LaSatThuongRi = false; Damageable.BoQuaTiaTrungDon = false; }
        }
    }

    void OnDestroy()
    {
        if (ins == this) ins = null;
    }
}
