using UnityEngine;

public enum DamageType { Physical, Fire, Ice, Lightning }

/// <summary>
/// MAU + NHAN SAT THUONG. Gan cho ca phu thuy lan quai vat.
/// Lo luon phan nhap nhay khi trung don, hien so sat thuong va xu ly luc chet.
/// </summary>
public class Damageable : MonoBehaviour
{
    [Header("Mau")]
    public float maxHealth = 100f;
    public float health = 100f;

    /// <summary>
    /// MAU CUA VAT NAY DO MAY KHAC QUYET - may nay chi ve lai.
    ///
    /// Bat cho ban sao cua quai ben may khach va ban sao cua nguoi choi khac.
    /// Khi bat, don danh tren may nay van hien day du (nhap nhay, so sat
    /// thuong, tia hat) nhung KHONG tru mau va KHONG giet duoc no. Mau that
    /// den tu goi tin, cai chet that den tu mot goi tin noi "da chet".
    ///
    /// Loi da vap khi chua co co nay: phep cua nguoi khach van tru mau ban sao
    /// quai ngay tren may khach. Neu mau ve 0 truoc goi tin ke tiep thi con
    /// quai CHET o ben khach - va khong bao gio song lai, trong khi o ben chu
    /// phong no van song va van danh chu phong. Do duoc: IsDead = True ben
    /// khach, trong khi chu phong van bao "song, 100% mau". Ban sao nguoi choi
    /// dinh dung loi y het.
    /// </summary>
    public bool mauDoMayKhacQuyet;

    [Header("Khang")]
    [Range(-1f, 0.9f)] public float fireResist = 0f;
    [Range(-1f, 0.9f)] public float iceResist = 0f;
    [Range(-1f, 0.9f)] public float lightningResist = 0f;

    [Header("Khac")]
    public bool isPlayer = false;
    public float corpseSeconds = 6f;

    public CharacterRig rig;
    public ProceduralAnimator anim;

    /// <summary>Co khieng thi moi don deu tru vao khieng truoc. Xem <see cref="Khieng"/>.</summary>
    public Khieng khieng;

    public bool IsDead { get; private set; }
    public float Health01 { get { return maxHealth <= 0f ? 0f : Mathf.Clamp01(health / maxHealth); } }

    public System.Action<Damageable> onDeath;
    public System.Action<Damageable, float> onDamaged;

    float flashTimer;
    Color flashColor = Color.white;

    void Awake()
    {
        if (rig == null) rig = GetComponentInChildren<CharacterRig>();
        if (anim == null) anim = GetComponentInChildren<ProceduralAnimator>();
        health = Mathf.Min(health <= 0f ? maxHealth : health, maxHealth);
    }

    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (rig != null)
            {
                float k = Mathf.Clamp01(flashTimer / 0.18f);
                rig.SetFlash(flashColor, k * 1.4f);
            }
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        health = Mathf.Min(maxHealth, health + amount);
    }

    public void TakeDamage(float amount, DamageType type, Vector3 hitPoint)
    {
        if (IsDead || amount <= 0f) return;

        if (type == DamageType.Fire) amount *= (1f - fireResist);
        else if (type == DamageType.Ice) amount *= (1f - iceResist);
        else if (type == DamageType.Lightning) amount *= (1f - lightningResist);
        if (amount <= 0f) return;

        // KHIENG AN DON TRUOC. Dat SAU phan tinh khang (khang van co tac dung
        // len don danh vao khieng) nhung TRUOC khi tru mau that.
        //
        // Khieng con bat thi HapThu luon tra ve 0: don pha vo khieng khong tran
        // sang nguoi choi, dung nhu "khi mau khieng mat het se no tung, va luc
        // do nhan vat moi chiu sat thuong".
        if (khieng != null && khieng.DangBat)
        {
            amount = khieng.HapThu(amount);
            if (amount <= 0f) return;
        }

        // Ban sao: chi hien don danh, khong dung vao mau - xem mauDoMayKhacQuyet
        if (!mauDoMayKhacQuyet) health -= amount;

        // Nhap nhay theo loai sat thuong
        flashColor = type == DamageType.Fire ? new Color(1f, 0.45f, 0.1f)
                   : type == DamageType.Ice ? new Color(0.45f, 0.8f, 1f)
                   : type == DamageType.Lightning ? new Color(0.85f, 0.92f, 1f)
                   : new Color(1f, 0.25f, 0.2f);
        flashTimer = 0.18f;

        if (anim != null) anim.PlayHit();

        Vector3 popupAt = rig != null && rig.hitPoint != null ? rig.hitPoint.position : transform.position + Vector3.up * 1.2f;
        DamagePopup.Spawn(popupAt, Mathf.RoundToInt(amount), type, isPlayer);
        VfxFactory.HitBurst(hitPoint == Vector3.zero ? popupAt : hitPoint,
                            type == DamageType.Fire ? new Color(1f, 0.5f, 0.15f)
                          : type == DamageType.Ice ? new Color(0.6f, 0.85f, 1f)
                          : type == DamageType.Lightning ? new Color(0.85f, 0.93f, 1f)
                          : new Color(0.6f, 0.05f, 0.05f));

        if (onDamaged != null) onDamaged(this, amount);

        if (!mauDoMayKhacQuyet && health <= 0f) Die();
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        health = 0f;

        // Neu dang bi dong bang thi vo tan thay vi guc nga
        var frozen = GetComponent<FrozenEffect>();
        if (frozen != null && frozen.IsFullyFrozen)
        {
            VfxFactory.FrozenShatter(transform.position + Vector3.up * (rig != null ? rig.bodyHeight * 0.5f : 0.9f),
                                     rig != null ? rig.bodyHeight / 1.8f : 1f);
            if (onDeath != null) onDeath(this);
            Destroy(gameObject);
            return;
        }

        if (anim != null) anim.PlayDeath();

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        if (onDeath != null) onDeath(this);

        if (!isPlayer) Destroy(gameObject, corpseSeconds);
    }
}

/// <summary>Cac ham dung chung khi tinh sat thuong theo vung.</summary>
public static class CombatUtil
{
    static readonly Collider[] buffer = new Collider[64];

    /// <summary>
    /// Gay sat thuong cho moi muc tieu trong ban kinh.
    ///
    /// <paramref name="boQua"/> la nguoi TUNG phep - bo qua de khong tu thieu
    /// minh. Khi choi mot minh thi mask chi co lop Enemy nen khong can, nhung
    /// choi doi khang thi lop Player nam trong mask, va qua cau lua no ngay
    /// duoi chan se giet chinh nguoi vua bam phim.
    /// </summary>
    public static int AreaDamage(Vector3 center, float radius, float damage, LayerMask mask,
                                 DamageType type, float statusSeconds, Damageable boQua = null)
    {
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, mask, QueryTriggerInteraction.Collide);
        int hits = 0;

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            // Sat thuong giam dan tu tam ra ria
            float dist = Vector3.Distance(center, d.transform.position);
            float falloff = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(dist / Mathf.Max(0.01f, radius)));

            // Hoi TRUOC khi danh: don nay co the chinh la don lam vo khieng
            bool khiengDo = d.khieng != null && d.khieng.DangBat;

            d.TakeDamage(damage * falloff, type, d.transform.position + Vector3.up * 1f);
            hits++;

            if (d.IsDead) continue;

            // KHIENG DO TRON DON THI DO LUON HIEU UNG (chay, dong bang). Qua cau
            // no tren mat vom ma nguoi ben trong van boc chay thi nhin y nhu lua
            // da lot qua khieng.
            if (khiengDo) continue;

            if (type == DamageType.Fire && statusSeconds > 0f)
                BurningEffect.Apply(d, damage * 0.18f, statusSeconds);
            else if (type == DamageType.Ice && statusSeconds > 0f)
                FrozenEffect.Apply(d, statusSeconds);
        }

        return hits;
    }

    /// <summary>
    /// Sat thuong BANG theo vung, kem MOT XAC SUAT dong bang.
    ///
    /// Moi muc tieu duoc gieo RIENG mot lan, nen trong cung mot cu no co con bi
    /// dong cung co con chi an sat thuong - giong AreaShock cua set. Dung
    /// AreaDamage thi statusSeconds ap cho TAT CA, tuc cu trung la dong bang
    /// het, khong con la "xac suat" nua.
    /// </summary>
    /// <returns>So muc tieu trung don. <paramref name="soDongBang"/> tra ve so con bi dong.</returns>
    public static int AreaFreeze(Vector3 center, float radius, float damage, LayerMask mask,
                                 float freezeChance, float freezeSeconds, Damageable boQua,
                                 out int soDongBang)
    {
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, mask, QueryTriggerInteraction.Collide);
        int hits = 0;
        soDongBang = 0;

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            float dist = Vector3.Distance(center, d.transform.position);
            float falloff = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(dist / Mathf.Max(0.01f, radius)));

            d.TakeDamage(damage * falloff, DamageType.Ice, d.transform.position + Vector3.up * 1f);
            hits++;

            if (d.IsDead) continue;

            if (freezeSeconds > 0f && Random.value < freezeChance)
            {
                FrozenEffect.Apply(d, freezeSeconds);
                soDongBang++;
            }
        }
        return hits;
    }

    /// <summary>
    /// Sat thuong SET theo vung: moi muc tieu duoc gieo rieng mot lan xem co bi choang khong,
    /// nen trong cung mot cu set co con dung im co con van xong toi.
    /// </summary>
    public static int AreaShock(Vector3 center, float radius, float damage, LayerMask mask,
                                float stunChance, float stunSeconds, Damageable boQua = null)
    {
        int n = Physics.OverlapSphereNonAlloc(center, radius, buffer, mask, QueryTriggerInteraction.Collide);
        int hits = 0;

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            float dist = Vector3.Distance(center, d.transform.position);
            float falloff = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(dist / Mathf.Max(0.01f, radius)));

            d.TakeDamage(damage * falloff, DamageType.Lightning, d.transform.position + Vector3.up * 1f);
            hits++;

            if (d.IsDead) continue;

            if (Random.value < stunChance)
                StunnedEffect.Apply(d, stunSeconds);
        }

        return hits;
    }
}
