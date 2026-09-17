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

    /// <summary>Dem cho phep thu: so lan cham dat / so lan co cum gai bang.</summary>
    public static int SoLanCham, SoLanCoGai;

    static readonly Collider[] boDem = new Collider[32];

    /// <summary>
    /// Trong ban kinh sat thuong (impactRadius, 1,7 m) co KE DICH con song (Damageable tren damageMask - quai vat, va nguoi
    /// choi khac khi choi mang - tru nguoi tung) khong. Nguoi dung chon: "trung" = nam trong vung sat thuong.
    /// 17/09/2026 nguoi dung: "chi tao bang tren mat dat khi danh trung nguoi choi khac hoac cac quai vat" - bo han
    /// truong hop do vat (va cham lop Default: bia, cay, nha mo...) truoc day cung moc gai.
    /// </summary>
    bool TrungKeDich()
    {
        int n = Physics.OverlapSphereNonAlloc(target, impactRadius, boDem, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = boDem[i].GetComponentInParent<Damageable>();
            if (d != null && !d.IsDead && d != boQua) return true;
        }
        return false;
    }

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
            // CUM GAI BANG CHI MOC KHI TRUNG KE DICH - quai vat / nguoi choi khac (nguoi dung 17/09/2026; 16/09 la ca do vat):
            // trung mat dat hay do vat thi no binh thuong, khong co gai. Hoi TRUOC khi gay sat thuong - con nao chet vi cu nay van tinh.
            bool coGai = TrungKeDich();
            // HINH cum bang to bang cua Qua cau bang (2,55 m, prefab nuong o 1,7 -> x1,5). Nguoi dung
            // 16/09/2026 xin. CHI hinh to ra: vung sat thuong / dong bang ben duoi van la impactRadius.
            VfxFactory.IceImpact(target, Mathf.Max(impactRadius, QuaCauBang.BanKinhHinhBang), coGai);
            SoLanCham++;
            if (coGai) SoLanCoGai++;
            // Qua cau bang roi (Mua bang): tha luong khi lanh + vet bang ra tan dan nhu qua cau cua ky nang
            VfxFactory.ThaDuoiQuaCauBang(transform);
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
