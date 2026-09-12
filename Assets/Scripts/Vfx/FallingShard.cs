using UnityEngine;

/// <summary>Manh bang roi: bay xuong muc tieu roi no.</summary>
public class FallingShard : MonoBehaviour
{
    public Vector3 target;
    public float travelTime = 0.5f;
    public float impactRadius = 1.6f;
    public float damage = 0f;
    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    [Tooltip("Dong cung hoan toan bao lau khi an du xac suat")]
    public float freezeSeconds = 1.5f;

    [Tooltip("Xac suat lam ke dich DONG CUNG khi trung, 0..1")]
    public float freezeChance = 0.35f;

    [Tooltip("Giam bao nhieu phan toc do - ap CHAC CHAN cho moi muc tieu trung don")]
    public float chamTiLe = 0.5f;

    [Tooltip("Lam cham keo dai bao lau")]
    public float chamGiay = 2f;

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
                                      chamTiLe, chamGiay,
                                      freezeChance, freezeSeconds, boQua, out soDong);
            }
            Destroy(gameObject);
        }
    }
}
