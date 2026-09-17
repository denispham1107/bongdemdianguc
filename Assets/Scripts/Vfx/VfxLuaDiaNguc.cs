using UnityEngine;

/// <summary>
/// HINH "LUA DIA NGUC" (17/09/2026): nguoi dung chon lua DO SAM dia nguc de phan biet voi Qua cau lua. Khong dung hinh moi -
/// nhuom lai CHINH qua cau lua (prefab + vet lua Blender) va vu no cua no: moi mau hat, vat lieu, vet, den nhan voi
/// <see cref="MauDiaNguc"/> (cam 1,00/0,55/0,18 -> do sam ~1,00/0,15/0,03).
/// </summary>
public static partial class VfxFactory
{
    /// <summary>He so nhan mau: giu kenh do, dim xanh la / xanh lam.</summary>
    public static readonly Color MauDiaNguc = new Color(1f, 0.28f, 0.16f, 1f);

    /// <summary>So vat da nhuom (phep thu menu 72 doc).</summary>
    public static int SoLanNhuomDiaNguc;

    static Color Nhan(Color c) { return new Color(c.r * MauDiaNguc.r, c.g * MauDiaNguc.g, c.b * MauDiaNguc.b, c.a); }

    static Gradient Nhan(Gradient g)
    {
        if (g == null) return null;
        var ck = g.colorKeys;
        for (int i = 0; i < ck.Length; i++) ck[i].color = Nhan(ck[i].color);
        var ng = new Gradient();
        ng.SetKeys(ck, g.alphaKeys);
        ng.mode = g.mode;
        return ng;
    }

    static ParticleSystem.MinMaxGradient Nhan(ParticleSystem.MinMaxGradient m)
    {
        switch (m.mode)
        {
            case ParticleSystemGradientMode.Color: return new ParticleSystem.MinMaxGradient(Nhan(m.color));
            case ParticleSystemGradientMode.TwoColors: return new ParticleSystem.MinMaxGradient(Nhan(m.colorMin), Nhan(m.colorMax));
            case ParticleSystemGradientMode.Gradient: return new ParticleSystem.MinMaxGradient(Nhan(m.gradient));
            case ParticleSystemGradientMode.TwoGradients: return new ParticleSystem.MinMaxGradient(Nhan(m.gradientMin), Nhan(m.gradientMax));
            default:
                var r = new ParticleSystem.MinMaxGradient(Nhan(m.gradient));
                r.mode = m.mode;
                return r;
        }
    }

    /// <summary>Nhuom do sam moi thu nhin thay duoc duoi <paramref name="go"/> (hat, vat lieu, vet, den).</summary>
    public static void NhuomLuaDiaNguc(GameObject go)
    {
        if (go == null) return;
        SoLanNhuomDiaNguc++;
        foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            m.startColor = Nhan(m.startColor);
            var col = ps.colorOverLifetime;
            if (col.enabled) col.color = Nhan(col.color);
        }
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            // .materials: ban sao rieng cua qua nay - khong nhuom vat lieu dung chung cua Qua cau lua. Xoa ban sao khi
            // renderer bi huy (vet lua duoc tha ra song lau hon qua cau nen gan vao chinh renderer).
            var mats = r.materials;
            var xoa = r.gameObject.GetComponent<XoaVatLieuRieng>();
            if (xoa == null) xoa = r.gameObject.AddComponent<XoaVatLieuRieng>();
            foreach (var mt in mats)
            {
                if (mt == null) continue;
                xoa.ds.Add(mt);
                if (mt.HasProperty("_TintColor")) mt.SetColor("_TintColor", Nhan(mt.GetColor("_TintColor")));
                if (mt.HasProperty("_Color")) mt.SetColor("_Color", Nhan(mt.GetColor("_Color")));
                if (mt.HasProperty("_EmissionColor")) mt.SetColor("_EmissionColor", Nhan(mt.GetColor("_EmissionColor")));
            }
            var tr = r as TrailRenderer;
            if (tr != null) tr.colorGradient = Nhan(tr.colorGradient);
        }
        foreach (var l in go.GetComponentsInChildren<Light>(true)) l.color = Nhan(l.color);
    }
}
