using UnityEngine;

/// <summary>
/// KY NANG 1 - BAN QUA CAU LUA.
/// Qua cau bay thang, keo theo duoi lua va khoi, cham vao dau la no ra
/// mot vung lua lam bong chay ke dich xung quanh.
/// </summary>
public class Fireball : MonoBehaviour
{
    [Header("Bay")]
    public float speed = 17f;
    public float lifetime = 4f;
    public float bodyRadius = 0.30f;

    [Header("Sat thuong")]
    public float impactDamage = 55f;
    public float blastRadius = 3.4f;
    public float burnSeconds = 3.5f;
    public LayerMask hitMask;
    public LayerMask damageMask;

    Vector3 dir;
    float age;
    bool exploded;

    public static Fireball Spawn(Vector3 pos, Vector3 direction, LayerMask hitMask, LayerMask damageMask)
    {
        GameObject go;
        Fireball fb;

        var pf = GameAssets.I != null ? GameAssets.I.fireballPrefab : null;
        if (pf != null)
        {
            // Dung PREFAB co san trong Assets/Prefabs
            go = GameAssets.Make(pf, pos, Quaternion.LookRotation(direction), null);
            fb = go.GetComponent<Fireball>();
            if (fb == null) fb = go.AddComponent<Fireball>();
        }
        else
        {
            // Khong co prefab thi dung bang code nhu cu
            go = new GameObject("Fireball");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(direction);
            fb = go.AddComponent<Fireball>();
            VfxFactory.BuildFireballVisual(go.transform, fb.bodyRadius);
        }

        fb.dir = direction.normalized;
        fb.hitMask = hitMask;
        fb.damageMask = damageMask;
        return fb;
    }

    /// <summary>
    /// Ban MOT CHUM ba qua cau lua toe hinh quat ve phia truoc.
    ///
    /// Ba qua bay cung luc, lech nhau <paramref name="gocToe"/> do quanh truc
    /// DUNG - toe ngang chu khong toe len xuong, vi tran nha va mat dat khong
    /// lien quan gi den chuyen nham ngang cua nguoi choi.
    ///
    /// Quay quanh truc dung (Vector3.up) chu khong quanh truc ngang cua nguoi
    /// ban: neu nguoi choi nham chech len hay chech xuong, quay quanh truc cua
    /// ho se lam ba qua toe theo mot mat phang NGHIENG, hai qua bien mot cai
    /// chui xuong dat mot cai bay len troi.
    /// </summary>
    public static void SpawnChum(Vector3 pos, Vector3 direction, LayerMask hitMask,
                                 LayerMask damageMask, int soQua = 3, float gocToe = 11f)
    {
        Vector3 huong = direction.normalized;

        // Xep deu hai ben huong ngam: 3 qua -> -1, 0, +1 lan goc toe
        float giua = (soQua - 1) * 0.5f;
        for (int i = 0; i < soQua; i++)
        {
            float goc = (i - giua) * gocToe;
            Vector3 h = Quaternion.AngleAxis(goc, Vector3.up) * huong;
            Spawn(pos, h, hitMask, damageMask);
        }
    }

    void Update()
    {
        if (exploded) return;

        float dt = Time.deltaTime;
        age += dt;

        float step = speed * dt;
        Vector3 from = transform.position;

        RaycastHit hit;
        if (Physics.SphereCast(from, bodyRadius, dir, out hit, step + 0.05f, hitMask,
                               QueryTriggerInteraction.Collide))
        {
            transform.position = hit.point - dir * bodyRadius * 0.5f;
            Explode();
            return;
        }

        transform.position = from + dir * step;

        if (age >= lifetime) Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        VfxFactory.FireExplosion(transform.position, blastRadius);
        CombatUtil.AreaDamage(transform.position, blastRadius, impactDamage, damageMask,
                              DamageType.Fire, burnSeconds);

        // Don no NGAY TREN MAT KHIENG thi tru mau khieng. AreaDamage o tren
        // khong lo duoc viec nay: no chi tim Damageable trong ban kinh, ma chu
        // khieng dung o TAM vom - xa hon ban kinh no. Xem Khieng.NoTrungKhieng.
        Khieng.NoTrungKhieng(transform.position, blastRadius, impactDamage, damageMask);

        CameraShake.Shake(0.25f, 0.16f);
        Destroy(gameObject);
    }
}
