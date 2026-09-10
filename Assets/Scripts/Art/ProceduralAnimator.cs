using UnityEngine;

/// <summary>
/// HOAT HINH BANG CONG THUC (khong dung file animation).
///
/// Moi tu the (dung yen, buoc di, niem chu, chem, trung don, guc nga) deu
/// duoc tinh bang ham sin/cos roi ap vao goc xoay cua tung khop xuong.
/// Nho vay nhan vat cu dong muot ma khong can bat ky file .anim nao.
/// </summary>
public class ProceduralAnimator : MonoBehaviour
{
    public CharacterRig rig;

    [Header("Dang di")]
    public float strideScale = 1f;      // sai chan dai hay ngan
    public float bobScale = 1f;         // nhun nhay khi buoc

    [Tooltip("Bat cho phu thuy (mot tay cam gay), tat cho quai.")]
    public bool casterPose = false;

    // Trang thai
    float moveSpeed01;                  // 0 = dung yen, 1 = chay het toc
    float phase;                        // pha buoc chan
    float castTimer, castDuration;
    float attackTimer, attackDuration;
    float hitTimer;
    float deathTimer = -1f;
    bool dead;

    // Tu the goc (luc vua dung xong hinh)
    Quaternion bHips, bSpine, bChest, bNeck, bHead;
    Quaternion bArmL, bArmR, bForeL, bForeR, bHandL, bHandR;
    Quaternion bLegL, bLegR, bShinL, bShinR, bFootL, bFootR;
    Vector3 hipsBasePos;

    public bool IsCasting { get { return castTimer > 0f; } }
    public bool IsAttacking { get { return attackTimer > 0f; } }
    public bool IsDead { get { return dead; } }

    void Start()
    {
        if (rig == null) rig = GetComponentInChildren<CharacterRig>();
        if (rig == null) { enabled = false; return; }

        ApplyRestPose();
        CacheBindPose();
    }

    /// <summary>Tu the nghi: khuyu tay hoi co, tay hoi banh ra.</summary>
    void ApplyRestPose()
    {
        if (casterPose)
        {
            // Tay trai giu cay gay truoc nguoi, tay phai tha long
            Set(rig.armL, new Vector3(-22f, 0f, -12f));
            Set(rig.foreArmL, new Vector3(-52f, 0f, 0f));
            Set(rig.armR, new Vector3(6f, 0f, 9f));
            Set(rig.foreArmR, new Vector3(-16f, 0f, 0f));
        }
        else
        {
            Set(rig.armL, new Vector3(4f, 0f, -14f));
            Set(rig.armR, new Vector3(4f, 0f, 14f));
            Set(rig.foreArmL, new Vector3(-26f, 0f, 0f));
            Set(rig.foreArmR, new Vector3(-26f, 0f, 0f));
        }
    }

    static void Set(Transform t, Vector3 euler)
    {
        if (t != null) t.localRotation = Quaternion.Euler(euler);
    }

    void CacheBindPose()
    {
        bHips = Q(rig.hips); bSpine = Q(rig.spine); bChest = Q(rig.chest);
        bNeck = Q(rig.neck); bHead = Q(rig.head);
        bArmL = Q(rig.armL); bArmR = Q(rig.armR);
        bForeL = Q(rig.foreArmL); bForeR = Q(rig.foreArmR);
        bHandL = Q(rig.handL); bHandR = Q(rig.handR);
        bLegL = Q(rig.legL); bLegR = Q(rig.legR);
        bShinL = Q(rig.shinL); bShinR = Q(rig.shinR);
        bFootL = Q(rig.footL); bFootR = Q(rig.footR);
        if (rig.hips != null) hipsBasePos = rig.hips.localPosition;
    }

    static Quaternion Q(Transform t) { return t != null ? t.localRotation : Quaternion.identity; }

    // ================================================================
    //  LENH TU BEN NGOAI
    // ================================================================

    public void SetMoveSpeed(float normalized)
    {
        moveSpeed01 = Mathf.Clamp01(normalized);
    }

    /// <summary>Dang di nhanh bao nhieu, 0..1. Mo ra cho kich ban chay thu doc.</summary>
    public float TocDoDi01 { get { return moveSpeed01; } }

    public void PlayCast(float duration)
    {
        castDuration = Mathf.Max(0.15f, duration);
        castTimer = castDuration;
    }

    public void PlayAttack(float duration)
    {
        attackDuration = Mathf.Max(0.2f, duration);
        attackTimer = attackDuration;
    }

    public void PlayHit()
    {
        if (!dead) hitTimer = 0.22f;
    }

    public void PlayDeath()
    {
        dead = true;
        deathTimer = 0f;
    }

    // ================================================================
    //  CAP NHAT TUNG KHUNG HINH
    // ================================================================

    void LateUpdate()
    {
        if (rig == null) return;
        float dt = Time.deltaTime;

        if (dead) { UpdateDeath(dt); return; }

        if (castTimer > 0f) castTimer -= dt;
        if (attackTimer > 0f) attackTimer -= dt;
        if (hitTimer > 0f) hitTimer -= dt;

        phase += dt * Mathf.Lerp(2.2f, 9.5f, moveSpeed01) * (0.4f + moveSpeed01 * 0.9f);

        float sw = Mathf.Sin(phase);
        float sw2 = Mathf.Sin(phase * 2f);
        float amp = moveSpeed01 * 42f * strideScale;

        // ---- Chan: sai buoc ----
        Rot(rig.legL, bLegL, new Vector3(sw * amp, 0f, 0f));
        Rot(rig.legR, bLegR, new Vector3(-sw * amp, 0f, 0f));
        // Dau goi chi gap ve mot phia (khong be nguoc)
        Rot(rig.shinL, bShinL, new Vector3(-Mathf.Max(0f, -sw) * amp * 1.15f - moveSpeed01 * 6f, 0f, 0f));
        Rot(rig.shinR, bShinR, new Vector3(-Mathf.Max(0f, sw) * amp * 1.15f - moveSpeed01 * 6f, 0f, 0f));
        Rot(rig.footL, bFootL, new Vector3(Mathf.Max(0f, -sw) * amp * 0.5f, 0f, 0f));
        Rot(rig.footR, bFootR, new Vector3(Mathf.Max(0f, sw) * amp * 0.5f, 0f, 0f));

        // ---- Than: nhun theo buoc + tho khi dung yen ----
        float breathe = Mathf.Sin(Time.time * 1.6f) * 1.6f * (1f - moveSpeed01);
        float bob = -Mathf.Abs(sw2) * 0.035f * moveSpeed01 * bobScale;
        if (rig.hips != null) rig.hips.localPosition = hipsBasePos + new Vector3(0f, bob, 0f);
        Rot(rig.hips, bHips, new Vector3(0f, sw * 5f * moveSpeed01, 0f));
        Rot(rig.spine, bSpine, new Vector3(moveSpeed01 * 7f + breathe * 0.4f, -sw * 4f * moveSpeed01, 0f));

        // ---- Tay ----
        float castT = castTimer > 0f ? 1f - (castTimer / castDuration) : -1f;
        float atkT = attackTimer > 0f ? 1f - (attackTimer / attackDuration) : -1f;

        Vector3 armLOff, armROff, foreLOff, foreROff, chestOff, headOff;
        armLOff = armROff = foreLOff = foreROff = chestOff = headOff = Vector3.zero;

        // Tay vung khi di
        float swingArm = sw * amp * 0.62f;
        armLOff.x -= swingArm * (casterPose ? 0.25f : 1f);
        armROff.x += swingArm;
        chestOff.y = sw * -3f * moveSpeed01;
        chestOff.x = breathe * 0.5f + moveSpeed01 * 4f;

        // Niem chu: gio tay ve phia truoc, nguoi ngua ra roi don ve
        if (castT >= 0f)
        {
            float raise = Mathf.Sin(Mathf.Clamp01(castT) * Mathf.PI);           // len roi xuong
            float punch = Mathf.Pow(Mathf.Clamp01((castT - 0.45f) / 0.55f), 2f); // don tay ve truoc

            armROff.x -= 95f * raise + 25f * punch;
            armROff.z += 18f * raise;
            foreROff.x -= 55f * raise - 30f * punch;

            armLOff.x -= 35f * raise;      // tay cam gay nang len theo
            chestOff.x -= 10f * raise - 14f * punch;
            headOff.x -= 6f * raise;
        }

        // Chem / dam (quai vat)
        if (atkT >= 0f)
        {
            float wind = Mathf.Clamp01(atkT / 0.45f);
            float strike = Mathf.Clamp01((atkT - 0.45f) / 0.55f);
            float s = Mathf.Sin(strike * Mathf.PI * 0.5f);

            armROff.x -= 120f * wind - 175f * s;
            foreROff.x -= 70f * wind - 60f * s;
            chestOff.y += 22f * wind - 34f * s;
            chestOff.x += -8f * wind + 16f * s;
        }

        // Trung don: giat nguoi ve sau
        if (hitTimer > 0f)
        {
            float h = hitTimer / 0.22f;
            chestOff.x -= 22f * h;
            headOff.x -= 16f * h;
            armLOff.x += 18f * h;
            armROff.x += 18f * h;
        }

        Rot(rig.chest, bChest, chestOff);
        Rot(rig.neck, bNeck, headOff);
        Rot(rig.head, bHead, new Vector3(0f, Mathf.Sin(Time.time * 0.7f) * 2.5f, 0f));

        Rot(rig.armL, bArmL, armLOff);
        Rot(rig.armR, bArmR, armROff);
        Rot(rig.foreArmL, bForeL, foreLOff);
        Rot(rig.foreArmR, bForeR, foreROff);
    }

    /// <summary>Guc nga: khuyu goi, do nguoi ve truoc roi lun dan xuong dat.</summary>
    void UpdateDeath(float dt)
    {
        deathTimer += dt;
        float t = Mathf.Clamp01(deathTimer / 1.1f);
        float e = 1f - Mathf.Pow(1f - t, 3f);

        Rot(rig.legL, bLegL, new Vector3(-70f * e, 0f, 0f));
        Rot(rig.legR, bLegR, new Vector3(-62f * e, 0f, 0f));
        Rot(rig.shinL, bShinL, new Vector3(110f * e, 0f, 0f));
        Rot(rig.shinR, bShinR, new Vector3(105f * e, 0f, 0f));
        Rot(rig.spine, bSpine, new Vector3(45f * e, 0f, 12f * e));
        Rot(rig.chest, bChest, new Vector3(30f * e, 0f, -8f * e));
        Rot(rig.neck, bNeck, new Vector3(28f * e, 0f, 0f));
        Rot(rig.armL, bArmL, new Vector3(35f * e, 0f, -40f * e));
        Rot(rig.armR, bArmR, new Vector3(35f * e, 0f, 40f * e));

        if (rig.root != null)
        {
            rig.root.localRotation = Quaternion.Euler(20f * e, 0f, 8f * e);
            rig.root.localPosition = new Vector3(0f, -0.35f * e, 0.12f * e);
        }
    }

    static void Rot(Transform t, Quaternion bind, Vector3 offsetEuler)
    {
        if (t == null) return;
        t.localRotation = bind * Quaternion.Euler(offsetEuler);
    }
}
