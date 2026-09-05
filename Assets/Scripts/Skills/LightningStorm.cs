using UnityEngine;

/// <summary>
/// KY NANG 3 - SAM SET.
///
/// Goi mot con giong xuong khu vuc chi dinh: may den keo den, khong khi tich dien,
/// roi HANG LOAT TIA SET tu tren cao lien tiep giang xuong. Moi tia no ra mot vung
/// sang trang tren mat dat, gay sat thuong va co 40% kha nang lam ke dich BI CHOANG
/// (dung im, khong danh duoc) trong vai giay.
///
/// Set uu tien nham vao quai dang dung trong vung, con lai thi danh ngau nhien
/// cho phu kin ca khu vuc.
/// </summary>
public class LightningStorm : MonoBehaviour
{
    [Header("Pham vi")]
    public float radius = 6f;
    public float duration = 2.4f;

    [Header("Tia set")]
    public float strikeInterval = 0.11f;
    public float strikeDamage = 26f;
    public float strikeRadius = 2.1f;
    public float boltHeight = 20f;
    public float aimAtEnemyChance = 0.65f;

    [Header("Gay choang")]
    [Range(0f, 1f)] public float stunChance = 0.4f;
    public float stunSeconds = 2.2f;

    public LayerMask damageMask;

    float timer, strikeTimer;

    static readonly Collider[] buffer = new Collider[64];

    public static LightningStorm Spawn(Vector3 center, LayerMask damageMask)
    {
        GameObject go;
        LightningStorm st;

        var pf = GameAssets.I != null ? GameAssets.I.lightningStormPrefab : null;
        if (pf != null)
        {
            go = GameAssets.Make(pf, center);
            st = go.GetComponent<LightningStorm>();
            if (st == null) st = go.AddComponent<LightningStorm>();
        }
        else
        {
            go = new GameObject("LightningStorm");
            go.transform.position = center;
            st = go.AddComponent<LightningStorm>();
        }

        st.damageMask = damageMask;
        return st;
    }

    void Start()
    {
        VfxFactory.LightningStormField(transform.position, radius, duration);
        CameraShake.Shake(0.35f, 0.05f);

        // Cu set dau tien danh ngay giua tam cho da mat
        Strike(transform.position, 0f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;
        strikeTimer -= dt;

        if (timer < duration && strikeTimer <= 0f)
        {
            strikeTimer = strikeInterval;
            Strike(PickTarget(), Random.Range(0.04f, 0.10f));
        }

        if (timer >= duration + 1.6f) Destroy(gameObject);
    }

    /// <summary>Chon cho danh: uu tien quai con song trong vung, khong co thi danh bua.</summary>
    Vector3 PickTarget()
    {
        if (Random.value < aimAtEnemyChance)
        {
            int n = Physics.OverlapSphereNonAlloc(transform.position, radius, buffer, damageMask,
                                                  QueryTriggerInteraction.Collide);
            int alive = 0;
            Damageable pick = null;

            // Chon ngau nhien mot muc tieu con song (khong can gom thanh danh sach)
            for (int i = 0; i < n; i++)
            {
                var d = buffer[i].GetComponentInParent<Damageable>();
                if (d == null || d.IsDead) continue;

                alive++;
                if (Random.Range(0, alive) == 0) pick = d;
            }

            if (pick != null)
            {
                Vector3 p = pick.transform.position;
                Vector2 off = Random.insideUnitCircle * 0.4f;
                p += new Vector3(off.x, 0f, off.y);
                p.y = VfxFactory.GroundY(p);
                return p;
            }
        }

        Vector2 rnd = Random.insideUnitCircle * radius * 0.95f;
        Vector3 spot = transform.position + new Vector3(rnd.x, 0f, rnd.y);
        spot.y = VfxFactory.GroundY(spot);
        return spot;
    }

    void Strike(Vector3 spot, float delay)
    {
        spot.y = VfxFactory.GroundY(spot);

        var s = LightningStrike.Spawn(spot, boltHeight, delay);
        s.damage = strikeDamage;
        s.impactRadius = strikeRadius;
        s.damageMask = damageMask;
        s.stunChance = stunChance;
        s.stunSeconds = stunSeconds;
    }
}
