using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 4 - LOC XOAY.
///
/// Nem ra mot con loc khong lo cuon bui mu mit, tren than co tia set lach tach.
/// Con loc TU DI theo huong nem, luon lon mot chut cho tu nhien, va keo dai vai giay.
///
/// Quai nao lot vao vung hut se BI NHAC BONG LEN: bay vong quanh truc loc theo
/// dung chieu xoay, bi keo dan len cao va di theo con loc toi bat cu dau.
/// Luc loc tan thi ca dam roi tu tren cao xuong dat.
/// </summary>
public class Tornado : MonoBehaviour
{
    [Header("Hinh dang")]
    public float scale = 1f;

    [Header("Thoi gian va di chuyen")]
    public float duration = 6f;
    public float moveSpeed = 3.4f;
    public float wanderAmount = 0.55f;

    [Header("Suc hut")]
    // 3,6 x 1,20 = 4,32 - no ngang cung mot he so voi than loc, xem
    // VfxFactory.LocNoNgang. De nguyen 3,6 thi vung hut hep hon cai vo nhin
    // thay: quai dung ngay trong than loc ma khong bi cuon.
    public float catchRadius = 4.32f;
    public float liftHeight = 7f;
    public float spinDegreesPerSecond = 230f;

    [Header("Khoi bui phia sau")]
    [Tooltip("Luong khoi luc loc vua hien ra, hat moi giay")]
    public float khoiBuiDau = 30f;

    [Tooltip("Luong khoi luc loc sap tan, hat moi giay")]
    public float khoiBuiCuoi = 210f;

    [Header("Sat thuong")]
    public float damagePerSecond = 20f;
    public float boltDamage = 14f;
    public float boltInterval = 0.45f;

    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    /// <summary>Huong con loc dang truot toi (chi tinh theo phuong ngang).</summary>
    public Vector3 travelDir = Vector3.forward;

    float age, catchTimer, boltTimer, wanderSeed, soilTimer;
    ParticleSystem psKhoiBui;
    GameObject visual;
    readonly List<WhirledEffect> caught = new List<WhirledEffect>();
    readonly List<VatTheBiCuon> vatCuon = new List<VatTheBiCuon>();
    readonly List<ParticleSystem> soilParticles = new List<ParticleSystem>();

    /// <summary>
    /// Cuon nhieu nhat bao nhieu VAT THE cung luc.
    ///
    /// Co tran vi con loc di qua mot bui cay ram co the om mot luc ba bon chuc
    /// vat - moi cai mot Update rieng, va ca dam bay quanh mot truc thi khung
    /// hinh tut thay ro tren dien thoai.
    /// </summary>
    const int VatToiDa = 14;

    static readonly Collider[] buffer = new Collider[64];

    public static Tornado Spawn(Vector3 pos, Vector3 dir, LayerMask damageMask)
    {
        GameObject go;
        Tornado t;

        var pf = GameAssets.I != null ? GameAssets.I.tornadoPrefab : null;
        if (pf != null)
        {
            go = GameAssets.Make(pf, pos);
            t = go.GetComponent<Tornado>();
            if (t == null) t = go.AddComponent<Tornado>();
        }
        else
        {
            go = new GameObject("Tornado");
            go.transform.position = pos;
            t = go.AddComponent<Tornado>();
        }

        dir.y = 0f;
        t.travelDir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.forward;
        t.damageMask = damageMask;
        return t;
    }

    void Start()
    {
        wanderSeed = Random.Range(0f, 100f);

        // Neu chua co san hinh (vd dung tu prefab tron) thi dung bang code
        if (transform.childCount == 0)
        {
            visual = VfxFactory.BuildTornado(scale);
            visual.transform.SetParent(transform, false);
        }

        CacheSoilParticles();
        SnapToGround();
        UpdateSoilColor();
        CameraShake.Shake(0.5f, 0.06f);
    }

    // ================================================================
    //  MAU DAT CUON THEO
    // ================================================================

    /// <summary>Tim cac he hat mang "dat" de con doi mau chung theo cho loc di qua.</summary>
    void CacheSoilParticles()
    {
        soilParticles.Clear();
        var all = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < all.Length; i++)
        {
            string n = all[i].name;

            // CHI hat cat moi doi mau theo dat.
            //   - "Dust" (bui o chan) thuoc ve THAN loc, phai giu mau xam trang
            //   - "KhoiBui" la KHOI, phai den - nhuom theo dat thi no thanh mau
            //     nau nhat, mat han cai ve khoi bui boc len
            if (n == "Grit")
                soilParticles.Add(all[i]);
            else if (n == "KhoiBui")
                psKhoiBui = all[i];
        }
    }

    /// <summary>
    /// Nhuom manh vun theo dung mau dat ngay duoi chan loc, nen di qua vung bun
    /// thi cuon len dat cat nau, di qua bai co thi cuon len tang co xanh.
    /// </summary>
    void UpdateSoilColor()
    {
        if (soilParticles.Count == 0) return;

        Color soil = WorldFactory.SoilColorAt(transform.position.x, transform.position.z);

        for (int i = 0; i < soilParticles.Count; i++)
        {
            var ps = soilParticles[i];
            if (ps == null) continue;

            // Moi manh mot do dam khac nhau cho dam dat trong tu nhien
            var m = ps.main;
            m.startColor = new ParticleSystem.MinMaxGradient(
                new Color(soil.r * 0.72f, soil.g * 0.72f, soil.b * 0.72f, 1f),
                new Color(Mathf.Min(1f, soil.r * 1.35f),
                          Mathf.Min(1f, soil.g * 1.35f),
                          Mathf.Min(1f, soil.b * 1.35f), 1f));
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;

        Move(dt);

        // Loc truot sang cho dat khac thi mau manh vun doi theo
        soilTimer -= dt;
        if (soilTimer <= 0f)
        {
            soilTimer = 0.15f;
            UpdateSoilColor();
        }

        catchTimer -= dt;
        if (catchTimer <= 0f && age < duration)
        {
            catchTimer = 0.2f;
            CatchNearby();
            CuonVatThe();
        }

        boltTimer -= dt;
        if (boltTimer <= 0f)
        {
            boltTimer = boltInterval;
            Zap();
        }

        CangDiCangNhieuKhoi();

        if (age >= duration) Dissipate();
    }

    /// <summary>
    /// Loc cang di lau, khoi bui phia sau cang day.
    ///
    /// Luc moi hien ra thi chi lam tam bui, di mot lat la ca mot dai khoi den un
    /// un keo dai phia sau - de nguoi choi thay ro con loc da cay nat den dau.
    /// </summary>
    void CangDiCangNhieuKhoi()
    {
        if (psKhoiBui == null) return;

        var em = psKhoiBui.emission;

        // Da het gio thi NGUNG PHUN, khong tat ngay. Dam khoi da phun ra van con
        // do tu tan, nen vet khoi mo dan chu khong bien mat mot cai.
        if (age >= duration) { em.rateOverTime = 0f; return; }

        float k = Mathf.Clamp01(age / Mathf.Max(0.01f, duration));
        em.rateOverTime = Mathf.Lerp(khoiBuiDau, khoiBuiCuoi, k * k);
    }

    // ================================================================
    //  DI CHUYEN
    // ================================================================

    void Move(float dt)
    {
        // Luon lon nhe sang hai ben cho duong di khong thang bang
        float w = (Mathf.PerlinNoise(wanderSeed, Time.time * 0.35f) - 0.5f) * 2f;
        Vector3 side = new Vector3(-travelDir.z, 0f, travelDir.x);
        Vector3 dir = (travelDir + side * w * wanderAmount).normalized;

        Vector3 next = transform.position + dir * moveSpeed * dt;

        // Cham vach da bao quanh thi doi huong quay vao trong
        var dir2 = GameDirector.Instance;
        float arena = dir2 != null ? dir2.arenaRadius : 34f;
        Vector2 flat = new Vector2(next.x, next.z);
        if (flat.magnitude > arena * 0.93f)
        {
            travelDir = (-new Vector3(flat.x, 0f, flat.y)).normalized;
            next = transform.position + travelDir * moveSpeed * dt;
        }

        transform.position = next;
        SnapToGround();
    }

    void SnapToGround()
    {
        var p = transform.position;
        p.y = VfxFactory.GroundY(p);
        transform.position = p;
    }

    // ================================================================
    //  HUT QUAI VAO LOC
    // ================================================================

    void CatchNearby()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 1.2f,
                                              catchRadius, buffer, damageMask,
                                              QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            var w = WhirledEffect.Catch(d, this);
            if (w != null) caught.Add(w);
        }
    }

    /// <summary>
    /// CUON LUON CANH VAT: cay, da, bia mo tren duong loc di qua.
    ///
    /// Quet o lop Default - toan bo canh vat cua man nam o do (dia hinh o lop
    /// Ground rieng, nen khong bao gio bi hut theo). Danh sach loai tru nam
    /// trong <see cref="VatTheBiCuon.CuonDuoc"/>.
    /// </summary>
    void CuonVatThe()
    {
        DonVatCuon();
        if (vatCuon.Count >= VatToiDa) return;

        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 1.2f,
                                              catchRadius, buffer, 1 << 0,
                                              QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            if (vatCuon.Count >= VatToiDa) break;
            if (buffer[i] == null) continue;

            var v = VatTheBiCuon.Cuon(buffer[i].gameObject, this);
            if (v != null) vatCuon.Add(v);
        }
    }

    void DonVatCuon()
    {
        for (int i = vatCuon.Count - 1; i >= 0; i--)
            if (vatCuon[i] == null) vatCuon.RemoveAt(i);
    }

    /// <summary>
    /// Ban kinh than loc o mot do cao - de quai bay dung sat vo loc.
    ///
    /// Phai no cung he so voi hinh ve (<see cref="VfxFactory.LocNoNgang"/>),
    /// khong thi quai bay theo mot duong nam LOT hon trong vo, nhin nhu no
    /// xuyen qua than loc.
    /// </summary>
    public float FunnelRadiusAt(float h)
    {
        float k = Mathf.Clamp01(h / Mathf.Max(0.01f, 12.6f * scale));
        return Mathf.Lerp(0.9f, 4.4f, k * k) * scale * VfxFactory.LocNoNgang;
    }

    // ================================================================
    //  TIA SET
    // ================================================================

    void Zap()
    {
        VfxFactory.TornadoBolt(transform.position, scale);

        // Thinh thoang giat thang vao mot ke dang bi cuon
        CleanCaught();
        if (caught.Count == 0 || Random.value > 0.55f) return;

        var victim = caught[Random.Range(0, caught.Count)];
        if (victim == null) return;

        var d = victim.GetComponent<Damageable>();
        if (d == null || d.IsDead) return;

        var top = transform.position + Vector3.up * 12.6f * scale;
        var arc = LightningArc.Create(top, d.transform.position + Vector3.up * 0.9f, 0.9f, 0.22f);
        arc.segments = 14;
        arc.branches = 1;

        d.TakeDamage(boltDamage, DamageType.Lightning, d.transform.position + Vector3.up * 1f);
    }

    // ================================================================
    //  TAN LOC
    // ================================================================

    void Dissipate()
    {
        CleanCaught();
        for (int i = 0; i < caught.Count; i++)
            if (caught[i] != null) caught[i].Release();
        caught.Clear();

        // Canh vat thi TAN BIEN cung con loc chu khong roi xuong. No se moc
        // lai o cho cu sau 30 giay - xem VatTheBiCuon.GiayHoiSinh.
        DonVatCuon();
        for (int i = 0; i < vatCuon.Count; i++)
            if (vatCuon[i] != null) vatCuon[i].TanBien();
        vatCuon.Clear();

        // De hat bui bay not roi moi xoa han
        if (visual != null)
        {
            var ps = visual.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < ps.Length; i++)
                ps[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        var mr = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < mr.Length; i++) mr[i].enabled = false;

        Destroy(gameObject, 2.5f);
        enabled = false;
    }

    void CleanCaught()
    {
        for (int i = caught.Count - 1; i >= 0; i--)
            if (caught[i] == null) caught.RemoveAt(i);
    }

    void OnDestroy()
    {
        // Bao dam khong con ai bi treo lo lung neu con loc bi xoa dot ngot
        for (int i = 0; i < caught.Count; i++)
            if (caught[i] != null) caught[i].Release();

        for (int i = 0; i < vatCuon.Count; i++)
            if (vatCuon[i] != null) vatCuon[i].TanBien();
    }
}
