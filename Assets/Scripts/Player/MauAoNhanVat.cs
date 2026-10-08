using UnityEngine;

/// <summary>
/// MAU AO CUA TUNG NGUOI CHOI TRONG TRAN (09/10/2026, nguoi dung: "quan ao cac nguoi choi khac nhau co mau khac nhau (khong
/// qua toi qua nhat) trong che do Don; che do Doi cung doi cung mau, doi khac mau khac").
///
/// Ca phong dung chung mot prefab phu thuy, mot anh mau (ao choang TIM). Vat lieu Player_PhuThuy dung shader
/// Diablo25D/PhuThuyDoiMau: vung tim cua anh duoc thay bang _MauAo (giu sang toi, van vai). Mau dat qua MaterialPropertyBlock
/// tren tung renderer - khong tao vat lieu moi, khong dung den vat lieu dung chung.
///
/// Mau chon THEO GHE (Don) / THEO DOI (Doi) - may nao cung tinh ra cung mot mau, khong ton goi tin nao. Ca nhan vat cua
/// minh, ban sao nguoi khac, may BOT deu qua <see cref="KhoiDongTranMang.GanDoi"/>.
/// </summary>
public static class MauAoNhanVat
{
    /// <summary>Bang mau che do DON - moi ghe mot mau (6 ghe), do sang vua: khong qua toi, khong qua nhat.</summary>
    public static readonly Color[] MauDon =
    {
        new Color(0.80f, 0.12f, 0.12f),   // do tham
        new Color(0.10f, 0.62f, 0.28f),   // luc bao
        new Color(0.16f, 0.40f, 0.92f),   // lam
        new Color(0.95f, 0.50f, 0.08f),   // cam ho phach
        new Color(0.62f, 0.22f, 0.88f),   // tim
        new Color(0.06f, 0.66f, 0.68f),   // lam ngoc
    };

    /// <summary>Mau ao hai doi (che do DOI) - cung tong voi mau ten doi (CheDoTran.MauDoiA/B).</summary>
    public static readonly Color MauDoiA = new Color(0.16f, 0.40f, 0.92f);
    public static readonly Color MauDoiB = new Color(0.82f, 0.12f, 0.10f);

    static readonly int IdMauAo = Shader.PropertyToID("_MauAo");
    static MaterialPropertyBlock mpb;

    /// <summary>Mau ao cua ghe / doi: doi &gt;= 0 thi theo doi, khong thi theo ghe.</summary>
    public static Color MauCua(byte ghe, sbyte doi)
    {
        if (doi == CheDoTran.DoiA) return MauDoiA;
        if (doi == CheDoTran.DoiB) return MauDoiB;
        return MauDon[ghe % MauDon.Length];
    }

    /// <summary>Dat mau ao cho moi renderer cua nhan vat dang dung shader doi mau.</summary>
    public static void Dat(GameObject nv, Color mau)
    {
        if (nv == null) return;
        if (mpb == null) mpb = new MaterialPropertyBlock();
        mau.a = 1f;
        foreach (var r in nv.GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            if (!CoShaderDoiMau(r)) continue;
            r.GetPropertyBlock(mpb);          // giu thuoc tinh nguoi khac da dat (neu co)
            mpb.SetColor(IdMauAo, mau);
            r.SetPropertyBlock(mpb);
        }
    }

    /// <summary>Mau ao dang dat tren nhan vat (a = 0: chua doi mau) - phep thu doc lai.</summary>
    public static Color DangDat(GameObject nv)
    {
        if (nv == null) return Color.clear;
        if (mpb == null) mpb = new MaterialPropertyBlock();
        foreach (var r in nv.GetComponentsInChildren<Renderer>(true))
        {
            if (!CoShaderDoiMau(r)) continue;
            r.GetPropertyBlock(mpb);
            return mpb.GetColor(IdMauAo);
        }
        return Color.clear;
    }

    static bool CoShaderDoiMau(Renderer r)
    {
        foreach (var m in r.sharedMaterials)
            if (m != null && m.HasProperty(IdMauAo)) return true;
        return false;
    }
}
