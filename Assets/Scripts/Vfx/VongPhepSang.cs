using UnityEngine;

/// <summary>
/// Hoat anh vong phep: hien + phong to, xoay cham, giu, tat dan - lam tren TOA DO ANH (doi uv moi khung) de luoi van bam dat
/// (xoay / co gian ca vat the thi luoi roi khoi mat dat go ghe). Tu xoa vat lieu + luoi rieng.
/// </summary>
public class VongPhepSang : MonoBehaviour
{
    public const float GiayHien = 0.18f, GiayGiu = 0.9f, TongGiay = 1.9f;
    public Material mat;
    public Mesh luoi;
    float tuoi;
    Vector2[] uvGoc, uvMoi;

    /// <summary>He so phong to hien tai (1 = du co) - phep thu doc.</summary>
    public float HeSoTo { get; private set; }

    /// <summary>Do sang hien tai 0..1 - phep thu doc.</summary>
    public float DoSang { get; private set; }

    void Update()
    {
        tuoi += Time.deltaTime;
        float hien = Mathf.Clamp01(tuoi / GiayHien);
        float tat = tuoi <= GiayGiu ? 1f : Mathf.Clamp01(1f - (tuoi - GiayGiu) / (TongGiay - GiayGiu));
        DoSang = hien * tat;
        float to = Mathf.Lerp(0.6f, 1f, 1f - (1f - hien) * (1f - hien));
        HeSoTo = to;
        if (luoi != null)
        {
            if (uvGoc == null) { uvGoc = luoi.uv; uvMoi = new Vector2[uvGoc.Length]; }
            // uv ra ngoai [0,1] thi anh Clamp ra mau vien - vien anh la DEN nen cong sang khong ve gi
            float goc = -tuoi * 25f * Mathf.Deg2Rad, cs = Mathf.Cos(goc), sn = Mathf.Sin(goc), k = 1f / Mathf.Max(0.05f, to);
            for (int i = 0; i < uvGoc.Length; i++)
            {
                float x = (uvGoc[i].x - 0.5f) * k, y = (uvGoc[i].y - 0.5f) * k;
                uvMoi[i] = new Vector2(x * cs - y * sn + 0.5f, x * sn + y * cs + 0.5f);
            }
            luoi.uv = uvMoi;
        }
        // Cong sang: nhan CA mau (khong chi alpha) de tat that
        if (mat != null) mat.SetColor("_TintColor", new Color(0.55f * DoSang, 0.85f * DoSang, DoSang, DoSang));
    }

    void OnDestroy() { if (mat != null) Destroy(mat); if (luoi != null) Destroy(luoi); }
}
