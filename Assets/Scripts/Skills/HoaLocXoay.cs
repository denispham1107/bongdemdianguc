using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 14 - "HOA LOC XOAY" (them 18/09/2026).
///
/// Nguoi dung: bam ky nang nay thi con GIO LOC dang bay cua LAN TUNG GAN NHAT hoa thanh LOC XOAY -
/// gio loc TO DAN len thanh loc xoay; giu nguyen TOC DO BAY cua Gio loc (9,5 m/s) va giu tat ca hieu
/// ung cuon bay len cua Loc xoay; sat thuong tong = sat thuong cap hien tai cua Gio loc + cua Loc xoay;
/// ton 45 nang luong, hoi chieu 0,5 giay.
///
/// Nhung cho nguoi dung chon khi toi hoi lai (18/09/2026):
///   - GIU CA HAI KIEU DON: ke dich cham vao an ngay 75 (don mot lan cua Gio loc, nhan theo cap Gio loc),
///     roi bi cuon len va tiep tuc an 20/giay + tia set 14 cua Loc xoay (nhan theo cap Loc xoay).
///   - Con loc sau khi hoa song DU 6 GIAY cua Loc xoay (dong ho tinh lai tu dau).
///   - Bam ma khong co con Gio loc nao dang bay: TU CHOI, khong ton mana, khong vao hoi chieu.
///   - Gio loc CAP 5 ra hai con song song: hoa CA HAI trong mot lan bam.
/// </summary>
public static class HoaLocXoay
{
    public const float NangLuong = 45f;
    public const float HoiChieu = 0.5f;

    /// <summary>Gio loc phinh to thanh Loc xoay trong bao lau.</summary>
    public const float GiayPhongTo = 0.55f;

    /// <summary>Dem cho phep thu (menu 75).</summary>
    public static int SoLanHoa, SoLocDaHoa;

    /// <summary>
    /// Cac con Gio loc con song thuoc LAN TUNG GAN NHAT cua <paramref name="nguoiTung"/>.
    /// Danh sach rong = nguoi ay khong co con loc nao dang bay.
    /// </summary>
    public static List<GioLoc> LanTungGanNhat(Damageable nguoiTung)
    {
        var ra = new List<GioLoc>();
        float moiNhat = float.MinValue;

        foreach (var loc in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude))
        {
            if (loc == null || loc.boQua != nguoiTung) continue;
            if (loc.lucTung > moiNhat) moiNhat = loc.lucTung;
        }
        if (moiNhat == float.MinValue) return ra;

        foreach (var loc in Object.FindObjectsByType<GioLoc>(FindObjectsInactive.Exclude))
        {
            // Cung mot LAN BAM thi cung moc thoi gian (cap 5 ra hai con cung luc)
            if (loc == null || loc.boQua != nguoiTung) continue;
            if (Mathf.Abs(loc.lucTung - moiNhat) < 0.05f) ra.Add(loc);
        }
        return ra;
    }

    /// <summary>Con Gio loc nao cua nguoi nay dang bay khong - PlayerController hoi truoc khi tru mana.</summary>
    public static bool CoLocDeHoa(Damageable nguoiTung)
    {
        return LanTungGanNhat(nguoiTung).Count > 0;
    }

    /// <summary>
    /// Hoa cac con Gio loc cua lan tung gan nhat thanh Loc xoay. Tra ve so con da hoa.
    /// </summary>
    /// <param name="heSoGioLoc">He so sat thuong theo cap HIEN TAI cua Gio loc (don cham mot lan).</param>
    /// <param name="heSoLocXoay">He so sat thuong theo cap HIEN TAI cua Loc xoay (moi giay + tia set).</param>
    public static int Hoa(Damageable nguoiTung, LayerMask damageMask, float heSoGioLoc, float heSoLocXoay)
    {
        var ds = LanTungGanNhat(nguoiTung);
        if (ds.Count == 0) return 0;

        SoLanHoa++;
        foreach (var loc in ds)
        {
            if (loc == null) continue;

            Vector3 cho = loc.transform.position;
            Vector3 huong = loc.dir;
            float donCham = loc.damage;        // 75 x cap Gio loc (heSoSatThuong da nhan luc tung)

            var xoay = Tornado.Spawn(cho, huong, damageMask);
            xoay.boQua = nguoiTung;

            // GIU TOC DO BAY CUA GIO LOC (nguoi dung chot): Loc xoay von chi 3,4 m/s
            xoay.moveSpeed = GioLoc.TocDo;

            // Sat thuong: don cham mot lan cua Gio loc + sat thuong moi giay / tia set cua Loc xoay
            xoay.donChamMotLan = donCham > 0f ? donCham : GioLoc.SatThuongGoc * heSoGioLoc;
            xoay.damagePerSecond *= heSoLocXoay;
            xoay.boltDamage *= heSoLocXoay;

            // TO DAN: bat dau bang co con gio loc roi phinh len co loc xoay
            var to = xoay.gameObject.AddComponent<PhinhToThanhLoc>();
            to.giay = GiayPhongTo;
            to.coDau = GioLoc.ChieuCao / 12f;      // than gio loc cao 5 m, loc xoay ~12 m

            // Con gio loc cu bien mat ngay - hinh cua no do PhinhToThanhLoc thay the
            Object.Destroy(loc.gameObject);
            SoLocDaHoa++;
        }
        return ds.Count;
    }
}

/// <summary>
/// Cho con loc phinh to dan trong <see cref="giay"/> giay dau doi (18/09/2026).
///
/// Nguoi dung xin "hieu ung Gio loc hoa to dan thanh Loc xoay": con loc moi hien ra nho bang than
/// gio loc roi lon len het co Loc xoay. Chi doi TI LE HINH, khong dong den suc hut hay sat thuong -
/// de nguoc lai thi ke dich dung sat con loc dang phinh se khong bi cuon, nhin nhu loc bi hong.
/// </summary>
public class PhinhToThanhLoc : MonoBehaviour
{
    public float giay = 0.55f;
    public float coDau = 0.42f;

    float t;
    Transform hinh;

    void Start()
    {
        // Hinh cua Loc xoay la vat con dau tien (Tornado.Start dung no)
        if (transform.childCount > 0) hinh = transform.GetChild(0);
        if (hinh != null) hinh.localScale = Vector3.one * coDau;
    }

    void Update()
    {
        if (hinh == null) { Destroy(this); return; }

        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / Mathf.Max(0.01f, giay));
        // Phinh nhanh luc dau roi cham lai - nhu con loc dang hut lay khong khi
        float co = Mathf.Lerp(coDau, 1f, 1f - (1f - k) * (1f - k));
        hinh.localScale = Vector3.one * co;

        if (k >= 1f) { hinh.localScale = Vector3.one; Destroy(this); }
    }
}
