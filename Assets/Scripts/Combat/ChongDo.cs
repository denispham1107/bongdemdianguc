using UnityEngine;

/// <summary>
/// CHONG DO (DO DON) - Bo xuong co 25% chan dung moi don ky nang cua nguoi choi (nguoi dung 28/09/2026).
///
/// Nguoi dung chon:
///   - do duoc thi KHONG mat mau VA KHONG dinh hieu ung nao cua don ay (chay, dong bang, choang, nga, hat tung, cuon, chay den);
///   - MOI DON TRUNG gieo rieng (moi qua cau, moi hat bang, moi tia set), TRU sat thuong RI (nhip chay, nhip loc cuon, vung
///     lua Thien thach, cay chay - <see cref="Damageable.LaSatThuongRi"/>): gieo o do thi chu do don nhay lien tuc;
///   - chu hien len: "ĐỠ ĐÒN!" (co dau), kem mot QUA CAU BAO VE loe len quanh than.
///
/// MOT LAN GIEO MOI KHUNG HINH MOI CON (<see cref="Gieo"/> nho ket qua theo Time.frameCount): don trung va cac hieu ung cua CHINH
/// don ay chay trong cung mot khung (CombatUtil.AreaDamage: TakeDamage roi moi BurningEffect.Apply...), nen dung chung mot ket
/// qua - do sat thuong thi do luon hieu ung. Hieu ung gan ma khong co don nao (vd bi Loc xoay cuon) thi tu gieo.
///
/// QUA MANG: chu phong la trong tai cua quai (DongBoQuai) - chi may chu gieo. Ban sao tren may khach (mauDoMayKhacQuyet) khong
/// gieo, khong chan; hinh "do don" den qua bit 7 cua byte co trong goi quai (<see cref="GoiTin.MotQuai.doDon"/>).
/// </summary>
public static class ChongDo
{
    /// <summary>Ti le Bo xuong do don ky nang cua nguoi choi.</summary>
    public const float TiLeBoXuong = 0.25f;
    /// <summary>Bit "vua do don" trong goi quai giu bat bao lau sau moi lan do.</summary>
    public const float GiayBaoQuaMang = 0.35f;
    /// <summary>Hai lan hien hinh do don cach nhau it nhat (Mua bang 36 hat trung cung mot con - khong chong 36 chu).</summary>
    const float GiayGiuaHaiLanHien = 0.25f;

    public const string ChuDoDon = "ĐỠ ĐÒN!";
    public static readonly Color MauChu = new Color(0.80f, 0.92f, 1f);

    /// <summary>Dem cho phep thu (menu 91).</summary>
    public static int SoLanGieo, SoLanDo, SoLanHien;

    /// <summary>
    /// Gieo (hoac tra lai ket qua da gieo trong khung nay) xem con nay co do duoc don khong. Chi con co ti le do don, chi tren
    /// may co quyen tinh mau cua no.
    /// </summary>
    public static bool Gieo(Damageable d)
    {
        if (d == null || d.tiLeDoDon <= 0f || d.IsDead || d.mauDoMayKhacQuyet) return false;
        if (d.khungGieoDoDon == Time.frameCount) return d.ketQuaDoDon;
        d.khungGieoDoDon = Time.frameCount;
        SoLanGieo++;
        d.ketQuaDoDon = Random.value < d.tiLeDoDon;
        if (d.ketQuaDoDon)
        {
            SoLanDo++;
            d.lucDoDonCuoi = Time.time;
            HienDoDon(d);
        }
        return d.ketQuaDoDon;
    }

    /// <summary>
    /// Goi o DAU moi Apply hieu ung (chay, bang, choang, nga, hat tung, cuon, chay den): true = don nay da bi do, bo hieu ung.
    /// Sat thuong ri khong bao gio bi do (nguoi dung chon).
    /// </summary>
    public static bool ChanHieuUng(Damageable d)
    {
        if (d == null || d.tiLeDoDon <= 0f || Damageable.LaSatThuongRi) return false;
        return Gieo(d);
    }

    /// <summary>Vua do don trong <see cref="GiayBaoQuaMang"/> giay qua - DongBoQuai doc de bat bit 7.</summary>
    public static bool VuaDoDon(Damageable d)
    {
        return d != null && Time.time - d.lucDoDonCuoi < GiayBaoQuaMang;
    }

    /// <summary>Hinh do don: qua cau bao ve loe len quanh than + chu "ĐỠ ĐÒN!". May khach goi khi bit 7 vua bat.</summary>
    public static void HienDoDon(Damageable d)
    {
        if (d == null) return;
        if (Time.time - d.lucHienDoDon < GiayGiuaHaiLanHien) return;
        d.lucHienDoDon = Time.time;
        SoLanHien++;
        float cao = 1.7f;
        var cc = d.GetComponent<CharacterController>();
        if (cc != null) cao = Mathf.Clamp(cc.height, 1.2f, 2.6f);
        VfxFactory.CauDoDon(d.transform, cao);
        DamagePopup.SpawnText(d.transform.position + Vector3.up * (cao + 0.35f), ChuDoDon, MauChu);
    }
}
