using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TRANG THAI BONG CHAY: quai bi lua bam vao nguoi, mat mau dan theo thoi gian.
/// </summary>
public class BurningEffect : MonoBehaviour
{
    public float damagePerSecond = 6f;
    public float remaining = 4f;

    /// <summary>
    /// Ai chay lua vao nguoi nay. Moi nhip chay deu ghi lai lam "ke danh cuoi"
    /// - khong thi con nao chet vi CHAY (sau khi qua cau da no xong) thanh vo
    /// danh neu co ai khac cham vao no truoc do, hoac chua ai ghi gi.
    /// </summary>
    public Damageable keGayChay;

    Damageable target;
    GameObject vfx;
    float tick;

    public static void Apply(Damageable d, float dps, float seconds, Damageable keGay = null)
    {
        if (d == null || d.IsDead) return;

        var b = d.GetComponent<BurningEffect>();
        if (b == null) b = d.gameObject.AddComponent<BurningEffect>();
        if (keGay != null) b.keGayChay = keGay;       // lua moi nhat la cua nguoi vua gay

        b.damagePerSecond = Mathf.Max(b.damagePerSecond, dps);
        b.remaining = Mathf.Max(b.remaining, seconds);
    }

    void Start()
    {
        target = GetComponent<Damageable>();
        float h = target != null && target.rig != null ? target.rig.bodyHeight : 1.8f;
        float r = target != null && target.rig != null ? target.rig.bodyRadius : 0.35f;
        vfx = VfxFactory.AttachBurning(transform, h, r * 1.15f);

        // Lua thieu lam tan bang
        var frozen = GetComponent<FrozenEffect>();
        if (frozen != null) frozen.Thaw();
    }

    void Update()
    {
        if (target == null || target.IsDead) { Destroy(this); return; }

        remaining -= Time.deltaTime;
        tick += Time.deltaTime;

        if (tick >= 0.5f)
        {
            target.GhiKeDanh(keGayChay);
            target.TakeDamage(damagePerSecond * tick, DamageType.Fire, transform.position + Vector3.up * 1f);
            tick = 0f;
        }

        if (remaining <= 0f) Destroy(this);
    }

    void OnDestroy()
    {
        if (vfx != null) Destroy(vfx);
    }
}
