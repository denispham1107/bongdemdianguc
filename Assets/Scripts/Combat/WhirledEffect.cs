using UnityEngine;

/// <summary>
/// TRANG THAI BI LOC CUON.
///
/// Quai bi hut vao than loc roi NHAC BONG KHOI MAT DAT: no vua bay vong quanh
/// truc loc theo dung chieu xoay, vua bi keo dan len cao, vua quay lon nguoi.
/// Con loc di toi dau thi no bi loi theo toi do.
///
/// Trong luc bi cuon, quai khong dieu khien duoc than minh (tri tue va va cham
/// deu bi tat) va lien tuc mat mau vi bui da quat vao nguoi.
/// Khi loc tan hoac quai thoat ra, no roi tu do xuong dat va an them mot don
/// tuong ung voi do cao dang o.
/// </summary>
public class WhirledEffect : MonoBehaviour
{
    [Header("Quy dao")]
    public float angle;                 // goc hien tai quanh truc loc (do)
    public float orbitRadius = 2f;      // ban kinh quy dao
    public float targetHeight = 5f;     // do cao muon dat toi
    public float height;                // do cao hien tai

    [Header("Toc do")]
    public float riseSpeed = 5.5f;
    public float suckSpeed = 9f;

    public Tornado tornado;

    /// <summary>
    /// Nguoi tung con loc. Giu RIENG mot ban luc bi hut vao: con loc co the tan
    /// (bi Destroy) truoc khi nguoi nay rot xuong dat, ma don rot cung phai
    /// tinh cong cho nguoi tung.
    /// </summary>
    public Damageable keCuon;

    Damageable KeCuonHienTai { get { return tornado != null && tornado.boQua != null ? tornado.boQua : keCuon; } }

    Damageable target;
    EnemyAI ai;
    CharacterController cc;

    bool aiWasEnabled, ccWasEnabled;
    float tick;
    Vector3 tumble;

    /// <summary>Hut mot muc tieu vao con loc (bo qua neu no dang bi cuon roi).</summary>
    public static WhirledEffect Catch(Damageable d, Tornado tornado)
    {
        if (d == null || d.IsDead || tornado == null) return null;
        if (d.GetComponent<WhirledEffect>() != null) return null;

        var w = d.gameObject.AddComponent<WhirledEffect>();
        w.tornado = tornado;
        w.keCuon = tornado.boQua;

        Vector3 off = d.transform.position - tornado.transform.position;
        w.angle = Mathf.Atan2(off.z, off.x) * Mathf.Rad2Deg;
        w.orbitRadius = Mathf.Max(0.8f, new Vector2(off.x, off.z).magnitude);
        w.height = Mathf.Max(0f, d.transform.position.y - tornado.transform.position.y);
        w.targetHeight = Random.Range(tornado.liftHeight * 0.45f, tornado.liftHeight);

        DamagePopup.SpawnText(d.transform.position + Vector3.up * 2.1f, "BỊ CUỐN!",
                              new Color(0.85f, 0.82f, 0.70f));
        return w;
    }

    void Start()
    {
        target = GetComponent<Damageable>();

        // Tat tri tue va va cham de tu tay dieu khien vi tri trong luc bay
        ai = GetComponent<EnemyAI>();
        if (ai != null) { aiWasEnabled = ai.enabled; ai.enabled = false; }

        cc = GetComponent<CharacterController>();
        if (cc != null) { ccWasEnabled = cc.enabled; cc.enabled = false; }

        tumble = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void Update()
    {
        if (target == null || target.IsDead) { Detach(false); return; }
        if (tornado == null) { Detach(true); return; }

        float dt = Time.deltaTime;

        // 1) Bay vong quanh truc theo dung chieu loc dang xoay
        angle += tornado.spinDegreesPerSecond * dt;

        // 2) Bi keo dan len cao
        height = Mathf.MoveTowards(height, targetHeight, riseSpeed * dt);

        // 3) Cang len cao than loc cang phinh -> quy dao rong dan ra
        float wanted = tornado.FunnelRadiusAt(height);
        orbitRadius = Mathf.MoveTowards(orbitRadius, wanted, suckSpeed * dt);

        // 4) Vi tri muc tieu tinh theo TAM LOC -> loc di dau thi bi loi theo do
        float rad = angle * Mathf.Deg2Rad;
        Vector3 want = tornado.transform.position
                     + new Vector3(Mathf.Cos(rad) * orbitRadius, height, Mathf.Sin(rad) * orbitRadius);

        transform.position = Vector3.Lerp(transform.position, want, Mathf.Clamp01(suckSpeed * dt));

        // 5) Quay lon nguoi cho ra dang bi quang quat
        transform.Rotate(tumble, 200f * dt, Space.World);

        // 6) Bui da quat lien tuc vao nguoi
        tick += dt;
        if (tick >= 0.4f)
        {
            // Ghi ke danh truoc: con nao chet vi bi cuon ma khong ai ghi thi
            // thanh vo danh (13/09/2026 - menu 61 bat duoc)
            target.GhiKeDanh(KeCuonHienTai);
            target.TakeDamage(tornado.damagePerSecond * tick, DamageType.Physical,
                              transform.position);
            tick = 0f;
        }
    }

    /// <summary>Tha ra: tra lai quyen dieu khien roi de no roi tu do xuong dat.</summary>
    public void Release()
    {
        Detach(true);
    }

    void Detach(bool applyFall)
    {
        if (target != null && !target.IsDead)
        {
            if (cc != null) cc.enabled = ccWasEnabled;
            if (ai != null) ai.enabled = aiWasEnabled;

            // Nga tu tren cao xuong thi dau don hon
            if (applyFall && height > 1.5f)
            {
                target.GhiKeDanh(KeCuonHienTai);
                target.TakeDamage(height * 4f, DamageType.Physical, transform.position);
            }

            // Dung nguoi lai cho khoi nam nghieng giua khong trung
            var e = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, e.y, 0f);
        }
        else
        {
            if (cc != null) cc.enabled = ccWasEnabled;

            // CHET GIUA KHONG TRUNG thi cai xac phai ROI XUONG.
            //
            // Damageable.Die da tat CharacterController va Collider cua no, nen
            // khong con gi keo no xuong nua: no treo nguyen tren cao cho toi
            // luc bi xoa. Nguoi choi nhin thay mot cai xac lo lung giua troi.
            if (target != null
                && transform.position.y - VfxFactory.GroundY(transform.position) > 0.6f)
                RoiXuongDat.Gan(gameObject, 0f);
        }

        Destroy(this);
    }

    void OnDestroy()
    {
        // Phong khi bi xoa thang (vd doi scene) thi van tra lai va cham
        if (cc != null && !cc.enabled && target != null && !target.IsDead)
            cc.enabled = ccWasEnabled;
    }
}
