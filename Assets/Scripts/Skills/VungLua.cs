using UnityEngine;

/// <summary>
/// VUNG DAT BOC CHAY, de lai sau khi thien thach roi trung.
///
/// Song <see cref="duration"/> giay. Trong suot thoi gian do, ke dich nao dung
/// trong ban kinh deu an sat thuong lien tuc theo nhip <see cref="nhipDot"/>.
///
/// SAT THUONG TINH THEO GIAY chu khong theo lan danh: <see cref="damagePerSecond"/>
/// duoc chia deu ra tung nhip, nen doi nhip nhanh cham bao nhieu cung khong lam
/// vung lua manh len hay yeu di - chi doi cam giac muot hay giat.
///
/// Lua LUI DAN vao cuoi doi: ban kinh gay sat thuong co dinh nhung phan nhin
/// thay thi tat dan, va hai giay cuoi sat thuong cung nhe di. Tat phut mot cai
/// thi nhin nhu bi loi.
/// </summary>
public class VungLua : MonoBehaviour
{
    [Header("Pham vi va thoi gian")]
    public float radius = 4.5f;
    public float duration = 5f;

    [Header("Sat thuong")]
    public float damagePerSecond = 26f;

    [Tooltip("Bao lau moi dot mot lan, giay")]
    public float nhipDot = 0.25f;

    [Tooltip("Chay them may giay len nguoi ke dich dinh lua")]
    public float thoiGianChay = 1.6f;

    public LayerMask damageMask;

    float age, dotTimer;
    GameObject visual;

    public static VungLua Spawn(Vector3 tam, float banKinh, float thoiGian,
                                float satThuongMoiGiay, LayerMask damageMask)
    {
        var go = new GameObject("VungLua");
        go.transform.position = tam;

        var v = go.AddComponent<VungLua>();
        v.radius = banKinh;
        v.duration = thoiGian;
        v.damagePerSecond = satThuongMoiGiay;
        v.damageMask = damageMask;

        v.visual = VfxFactory.BuildVungLua(go.transform, banKinh, thoiGian);
        return v;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        age += dt;

        if (age < duration)
        {
            dotTimer -= dt;
            if (dotTimer <= 0f)
            {
                dotTimer = nhipDot;
                Dot();
            }
        }

        // Song them mot chut sau khi tat de dam lua con lai kip tan
        if (age >= duration + 3f) Destroy(gameObject);   // cho VfxFactory tat dan xong han
    }

    void Dot()
    {
        // Hai giay cuoi lua yeu dan di, khong tat phut mot cai
        float con = duration - age;
        float manh = con < 2f ? Mathf.Clamp01(con / 2f) : 1f;

        // Chia deu sat thuong moi giay ra tung nhip - doi nhipDot khong lam
        // vung lua manh len hay yeu di
        float moiNhip = damagePerSecond * nhipDot * manh;
        if (moiNhip <= 0.01f) return;

        CombatUtil.AreaDamage(transform.position, radius, moiNhip,
                              damageMask, DamageType.Fire, thoiGianChay);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.45f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
