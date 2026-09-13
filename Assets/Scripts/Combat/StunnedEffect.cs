using UnityEngine;

/// <summary>
/// TRANG THAI CHOANG (bi set danh trung dau).
///
/// Quai dung ngay tai cho, khong di duoc va khong danh duoc trong vai giay.
/// Tren nguoi no tia dien van con lach tach nhay, phia tren dau co mot vong
/// dom sang xoay tron cho de nhan ra.
/// </summary>
public class StunnedEffect : MonoBehaviour
{
    [Tooltip("Con bao lau nua thi tinh lai.")]
    public float remaining = 2f;

    public bool IsStunned { get { return remaining > 0f; } }

    Damageable target;
    GameObject vfx;
    float crackleTimer;
    float bodyHeight = 1.8f, bodyRadius = 0.4f;

    /// <summary>Lam mot muc tieu bi choang (cong don thi lay thoi gian dai hon).</summary>
    public static void Apply(Damageable d, float seconds)
    {
        if (d == null || d.IsDead || seconds <= 0f) return;
        // Ban sao (mau do may khac quyet) KHONG tu gieo hieu ung cho minh: ben
        // kia tung Random.value rieng, gieo them lan nua o day la hai may thay
        // hai ket qua khac nhau. Hieu ung cua ban sao den tu goi tin - xem
        // HieuUngQuaMang.ApCo.
        if (d.mauDoMayKhacQuyet) return;

        var s = d.GetComponent<StunnedEffect>();
        bool isNew = s == null;
        if (isNew) s = d.gameObject.AddComponent<StunnedEffect>();

        s.remaining = Mathf.Max(s.remaining, seconds);

        // Chi bao "CHOÁNG!" o lan dau bi dinh, khoi roi mat
        if (isNew)
            DamagePopup.SpawnText(d.transform.position + Vector3.up * 2.1f, "CHOANG!",
                                  new Color(0.75f, 0.90f, 1f));
    }

    void Start()
    {
        target = GetComponent<Damageable>();
        if (target != null && target.rig != null)
        {
            bodyHeight = target.rig.bodyHeight;
            bodyRadius = target.rig.bodyRadius;
        }

        vfx = VfxFactory.AttachStunned(transform, bodyHeight, bodyRadius);

        // Giat nay minh mot cai cho ro la vua an don
        if (target != null && target.anim != null) target.anim.PlayHit();
    }

    void Update()
    {
        if (target == null || target.IsDead) { Destroy(this); return; }

        float dt = Time.deltaTime;
        remaining -= dt;
        crackleTimer -= dt;

        // Thinh thoang lai co mot tia dien nho chay doc than nguoi
        if (crackleTimer <= 0f && remaining > 0.15f)
        {
            crackleTimer = Random.Range(0.10f, 0.24f);

            Vector3 a = transform.position + RandomOnBody();
            Vector3 b = transform.position + RandomOnBody();
            var arc = LightningArc.Create(a, b, 0.35f, Random.Range(0.10f, 0.18f));
            arc.transform.SetParent(transform, true);
            arc.segments = 7;
            arc.jitter = 2.2f;
            arc.branches = 0;
        }

        if (remaining <= 0f) Destroy(this);
    }

    Vector3 RandomOnBody()
    {
        Vector2 r = Random.insideUnitCircle.normalized * bodyRadius * 1.1f;
        return new Vector3(r.x, Random.Range(bodyHeight * 0.25f, bodyHeight * 1.02f), r.y);
    }

    void OnDestroy()
    {
        if (vfx != null) Destroy(vfx);
    }
}
