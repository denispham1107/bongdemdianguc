using UnityEngine;

/// <summary>Manh bang roi: bay xuong muc tieu roi no.</summary>
public class FallingShard : MonoBehaviour
{
    public Vector3 target;
    public float travelTime = 0.5f;
    public float impactRadius = 1.6f;
    public float damage = 0f;
    public LayerMask damageMask;
    public float freezeSeconds = 2.5f;

    [Tooltip("Xac suat lam ke dich DONG BANG khi trung, 0..1")]
    public float freezeChance = 0.30f;

    Vector3 start;
    float t;

    void Start()
    {
        start = transform.position;
        if (travelTime <= 0.01f) travelTime = 0.4f;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / travelTime);
        // Roi nhanh dan (gia toc)
        float e = k * k;
        transform.position = Vector3.Lerp(start, target, e);

        if (k >= 1f)
        {
            VfxFactory.IceImpact(target, impactRadius);
            if (damage > 0f)
            {
                // AreaFreeze chu khong AreaDamage: AreaDamage ap statusSeconds cho
                // TAT CA muc tieu trung don, tuc cu cham la dong bang het - khong
                // con la mot xac suat nua.
                int soDong;
                CombatUtil.AreaFreeze(target, impactRadius, damage, damageMask,
                                      freezeChance, freezeSeconds, out soDong);
            }
            Destroy(gameObject);
        }
    }
}
