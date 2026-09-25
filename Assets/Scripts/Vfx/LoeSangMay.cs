using UnityEngine;

/// <summary>
/// SET ROI SANG MAY GIONG (nguoi dung 25/09/2026: "cho toan may giong mau toi den hon, de giong may giong den that"; chon
/// "xam den, set roi sang"). May, cot khoi, quang may sat dat nay toi ~25% do sang cu; moi lan tia set
/// giang thi ca dam may LOE NHE len roi tat nhanh trong <see cref="GiayTat"/> giay; phan sang chinh la MANG SANG cho tia phat
/// ra (VfxFactory.MangSangTrongMay) - "set roi sang tung mang". Loe ca dam nhe thoi, khong thi 4 tia/giay giu may sang gan het
/// thoi gian va may het toi.
///
/// Doi mau bang MaterialPropertyBlock tren tung renderer (khong tao vat lieu moi moi lan tung, khong dung vat lieu dung chung:
/// hai dam may cung luc khong chop theo nhau). Shader Diablo25D/ParticleAlpha: mau = anh x mau hat x _TintColor.
/// </summary>
public class LoeSangMay : MonoBehaviour
{
    /// <summary>Luc chop manh nhat, mau may nhan len bao nhieu lan (0,26 x 1,8 = ~47% do sang cu).</summary>
    public const float HeSoSangNhat = 1.8f;
    /// <summary>Chop tat dan trong bao lau.</summary>
    public const float GiayTat = 0.15f;

    Renderer[] rs;
    Color[] goc;
    MaterialPropertyBlock mpb;
    float sang;
    bool canTraLai;

    /// <summary>He so mau dang ap (1 = mau toi goc) - phep thu doc.</summary>
    public float HeSoHienTai { get; private set; } = 1f;
    public int SoRenderer { get { return rs != null ? rs.Length : 0; } }

    public void Gan(Renderer[] cacRenderer)
    {
        rs = cacRenderer;
        goc = new Color[rs.Length];
        for (int i = 0; i < rs.Length; i++)
            goc[i] = rs[i] != null && rs[i].sharedMaterial != null ? rs[i].sharedMaterial.GetColor("_TintColor") : Color.white;
        mpb = new MaterialPropertyBlock();
    }

    /// <summary>Chop sang voi do manh 0..1 (chop manh hon chop dang tat thi thay).</summary>
    public void Chop(float manh)
    {
        sang = Mathf.Max(sang, Mathf.Clamp01(manh));
    }

    void LateUpdate()
    {
        if (rs == null) return;
        if (sang <= 0f && !canTraLai) return;
        float k = 1f + (HeSoSangNhat - 1f) * sang;
        HeSoHienTai = k;
        for (int i = 0; i < rs.Length; i++)
        {
            if (rs[i] == null) continue;
            Color c = goc[i];
            c.r = Mathf.Min(1f, c.r * k); c.g = Mathf.Min(1f, c.g * k); c.b = Mathf.Min(1f, c.b * k);
            rs[i].GetPropertyBlock(mpb);
            mpb.SetColor("_TintColor", c);
            rs[i].SetPropertyBlock(mpb);
        }
        canTraLai = sang > 0f;
        sang = Mathf.Max(0f, sang - Time.deltaTime / GiayTat);
    }
}
