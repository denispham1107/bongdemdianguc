using UnityEngine;

/// <summary>He phep cua mot don danh - de biet Khang nao chan no.</summary>
public enum HeSat { Khac, Lua, Bang, Set, Phong }

/// <summary>
/// BON KY NANG BI DONG "KHANG ..." (them 19/09/2026, nguoi dung xin).
///
/// "Khang Lua: nguoi choi khi nang diem hoac mo khoa skill nay se nhan sat thuong tu cac skill thuoc
/// nhom Lua TU NGUOI CHOI KHAC giam 25% cho cap dau tien, va moi lan nang cap se giam them 5%."
/// Y het voi Khang Bang / Khang Set / Khang Phong.
///
/// Nen: cap 1 = 25%, cap 2 = 30%, ... cap 5 = 45%. Chua mo khoa = 0%.
///
/// ⚠️ CHI CHAN DON CUA NGUOI CHOI KHAC, khong chan don cua quai - dung nhu nguoi dung viet. Quai van
/// danh dau nhu cu. (Damageable van co san fireResist/iceResist/lightningResist rieng, ap cho MOI
/// nguon - hai thu do khong dinh gi toi nhau.)
///
/// HE cua mot don lay theo NHOM KY NANG trong Sach phep, khong theo DamageType:
///   LUA (Cau lua, Thien thach, Lua dia nguc) - BANG (Cau bang, Mua bang, Tang hinh)
///   SET (Giut set, Sam set, Cau dien)        - PHONG (Gio loc, Loc xoay, Hoa loc xoay)
/// Gan het khop voi DamageType (Fire/Ice/Lightning/Physical) nen suy thang tu do. NGOAI LE DUY NHAT:
/// tia set danh trong long Loc xoay / Gio loc mang DamageType.Lightning nhung thuoc ky nang nhom PHONG
/// - nguoi dung chot 19/09/2026 la tinh PHONG. Cho ay goi Damageable.GhiKeDanh(ai, HeSat.Phong) de
/// noi ro he, con moi cho khac cu de ham nay suy tu DamageType.
/// </summary>
public static class KhangHe
{
    /// <summary>Mo khoa (cap 1) giam bao nhieu.</summary>
    public const float GiamCapDau = 0.25f;

    /// <summary>Moi cap sau cap 1 giam them bao nhieu.</summary>
    public const float GiamMoiCap = 0.05f;

    /// <summary>Dem cho phep thu (menu 77): so don da bi khang chan.</summary>
    public static int SoDonBiChan;
    public static float TongDaChan;

    /// <summary>So hieu ky nang khang cua he do (-1 neu he khong co khang).</summary>
    public static int KyCua(HeSat he)
    {
        switch (he)
        {
            case HeSat.Lua: return CapDo.KyKhangLua;
            case HeSat.Bang: return CapDo.KyKhangBang;
            case HeSat.Set: return CapDo.KyKhangSet;
            case HeSat.Phong: return CapDo.KyKhangPhong;
        }
        return -1;
    }

    /// <summary>He suy tu loai sat thuong (dung khi noi gay don khong noi ro he).</summary>
    public static HeSat HeCua(DamageType loai)
    {
        switch (loai)
        {
            case DamageType.Fire: return HeSat.Lua;
            case DamageType.Ice: return HeSat.Bang;
            case DamageType.Lightning: return HeSat.Set;
            case DamageType.Physical: return HeSat.Phong;
        }
        return HeSat.Khac;
    }

    /// <summary>Ti le giam o cap <paramref name="cap"/>: 0 khi chua mo, 0,25 o cap 1, +0,05 moi cap.</summary>
    public static float GiamOCap(int cap)
    {
        if (cap <= 0) return 0f;
        return GiamCapDau + GiamMoiCap * (cap - 1);
    }

    /// <summary>Ti le giam HIEN TAI cua nhan vat may nay voi he do.</summary>
    public static float TiLeGiam(HeSat he)
    {
        int ky = KyCua(he);
        if (ky < 0) return 0f;
        return GiamOCap(CapDo.CapCuaKyNang(ky));
    }

    /// <summary>Mo ta ngan cho Sach phep: "giam 25%" ...</summary>
    public static string MoTaCap(int cap)
    {
        return Mathf.RoundToInt(GiamOCap(cap) * 100f) + "%";
    }
}
