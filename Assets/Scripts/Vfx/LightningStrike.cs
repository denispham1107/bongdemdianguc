using UnityEngine;

/// <summary>
/// MOT CU SET DANH XUONG mot diem tren mat dat.
///
/// Truoc khi danh co mot khoanh sang nho hien ra duoi dat bao truoc (rat ngan),
/// sau do tia set tu tren troi giang xuong, no bung anh sang trang va gay
/// sat thuong cho moi thu quanh diem cham dat.
/// </summary>
public class LightningStrike : MonoBehaviour
{
    [Header("Diem danh")]
    public Vector3 target;
    public float height = 20f;
    public float delay = 0.07f;

    [Header("Sat thuong")]
    public float damage = 30f;
    public float impactRadius = 2.1f;
    public LayerMask damageMask;

    [Header("Gay choang")]
    [Range(0f, 1f)] public float stunChance = 0.4f;
    public float stunSeconds = 2.2f;

    bool fired;
    float t;

    /// <summary>Goi mot cu set danh xuong diem chi dinh.</summary>
    public static LightningStrike Spawn(Vector3 target, float height, float delay)
    {
        var go = new GameObject("LightningStrike");
        go.transform.position = target;

        var s = go.AddComponent<LightningStrike>();
        s.target = target;
        s.height = height;
        s.delay = delay;
        return s;
    }

    void Start()
    {
        // Vet sang bao truoc noi set sap danh
        if (delay > 0.01f) VfxFactory.LightningWarning(target, impactRadius, delay);
    }

    void Update()
    {
        t += Time.deltaTime;

        if (!fired && t >= delay)
        {
            fired = true;
            Strike();
        }

        if (t >= delay + 1.5f) Destroy(gameObject);
    }

    void Strike()
    {
        Vector3 from = target + Vector3.up * height;

        // Tia set chinh: lech nhe theo phuong ngang cho ra dang tu may giang xuong
        Vector2 drift = Random.insideUnitCircle * height * 0.09f;
        from += new Vector3(drift.x, 0f, drift.y);

        var arc = LightningArc.Create(from, target, 1f, 0.30f);
        arc.segments = 20;
        arc.branches = Random.Range(2, 4);

        VfxFactory.LightningImpact(target, impactRadius);

        // CHO NAO SET DANH XUONG THI CHO DO CHAY, khong can co quai hay khong.
        //
        // Luong khoi chi 0,45: Sam set tha 22 tia trong 2,4 giay, moi tia mot
        // dam khoi day nhu Giut set thi ca vung chim trong khoi va khong con
        // nhin ra san choi.
        VfxFactory.SetChayDen(target, impactRadius * 0.72f, 0.45f);

        // Chuong ngai vat quanh cho set danh bi NAM DEN va boc khoi tren be mat
        VfxFactory.NamChuongNgai(target, impactRadius, VfxFactory.LopChuongNgai);

        if (damage > 0f)
            CombatUtil.AreaShock(target, impactRadius, damage, damageMask, stunChance, stunSeconds);

        CameraShake.Shake(0.18f, 0.075f);
    }
}
