using UnityEngine;

/// <summary>
/// HINH ANH KY NANG "GIO LOC" (them 16/09/2026).
///
/// Nguoi dung: loc nho "co hieu ung skill Loc xoay nhung chieu cao thap hon 1/2", MAU NAU, van co tia set
/// ben trong. Nguoi dung chon (khi toi hoi): dung chinh hinh Loc xoay thu nho va nhuom nau, khong dung Blender.
///
/// Dung lai <see cref="BuildTornado"/> (prefab Skill_LocXoay co dung cung bo con: 3 Shell, 3 DaiXoan, Dust,
/// Vut, Grit, KhoiBui, Cloud, StormLight) o ti le <see cref="GioLoc.TiLe"/>, roi:
///   - NHUOM NAU: nhan mau vo loc / dai xoan (vat lieu rieng tung cai) va startColor cua tung he hat
///     (vat lieu hat DUNG CHUNG voi Loc xoay - khong duoc sua vat lieu, chi sua mau hat);
///   - BAN NHE: bo den StormLight (ba loc cung luc = ba den diem, qua nang cho dien thoai) va giam nua
///     luong hat - loc nho hon nhieu nen dam hat cung khong can day nhu con loc lon.
/// Tia set: <see cref="TornadoBolt"/> goi voi cung ti le tu GioLoc.Update.
/// </summary>
public static partial class VfxFactory
{
    /// <summary>Mau nau nhan vao hinh xam trang cua Loc xoay.</summary>
    public static readonly Color NauGioLoc = new Color(0.80f, 0.56f, 0.32f, 1f);

    public static GameObject BuildGioLoc(float scale)
    {
        var root = BuildTornado(scale);
        root.name = "GioLocHinh";

        // Den diem: bo han (DestroyImmediate - vat dung luc chay, chua ai tham chieu)
        var den = root.transform.Find("StormLight");
        if (den != null) Object.DestroyImmediate(den.gameObject);

        foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            var m = r.sharedMaterial;
            if (m == null || !m.HasProperty("_TintColor")) continue;
            Color c = m.GetColor("_TintColor");
            m.SetColor("_TintColor", new Color(c.r * NauGioLoc.r, c.g * NauGioLoc.g, c.b * NauGioLoc.b, c.a));
        }

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            var sc = main.startColor;
            if (ps.name == "KhoiBui" || ps.name == "Cloud")
            {
                // Khoi / may den nga nau dat toi
                main.startColor = new ParticleSystem.MinMaxGradient(
                    new Color(0.16f, 0.10f, 0.06f), new Color(0.30f, 0.20f, 0.12f));
            }
            else
            {
                if (sc.mode == ParticleSystemGradientMode.TwoColors)
                    main.startColor = new ParticleSystem.MinMaxGradient(Nau(sc.colorMin), Nau(sc.colorMax));
                else if (sc.mode == ParticleSystemGradientMode.Color)
                    main.startColor = new ParticleSystem.MinMaxGradient(Nau(sc.color));
            }

            main.maxParticles = Mathf.Max(20, main.maxParticles / 2);
            var em = ps.emission;
            em.rateOverTime = new ParticleSystem.MinMaxCurve(em.rateOverTime.constant * 0.5f);
        }
        return root;
    }

    static Color Nau(Color c)
    {
        return new Color(c.r * NauGioLoc.r, c.g * NauGioLoc.g, c.b * NauGioLoc.b, c.a);
    }
}
