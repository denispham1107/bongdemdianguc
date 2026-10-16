using UnityEngine;

/// <summary>
/// MOT CU SET DANH XUONG mot diem tren mat dat.
///
/// Truoc khi danh co mot khoanh sang nho hien ra duoi dat bao truoc (rat ngan),
/// sau do tia set tu tren troi giang xuong, no bung anh sang trang va gay
/// sat thuong cho moi thu quanh diem cham dat.
/// </summary>
public class LightningStrike : MonoBehaviour
{
    [Header("Diem danh")]
    public Vector3 target;
    public float height = 20f;
    public float delay = 0.07f;

    [Header("Sat thuong")]
    public float damage = 30f;
    public float impactRadius = 2.1f;
    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    [Header("Gay choang")]
    [Range(0f, 1f)] public float stunChance = 0.4f;
    public float stunSeconds = 2.2f;

    bool fired;
    float t;

    /// <summary>Dem cho phep thu (menu 130): so tia da danh xuong.</summary>
    public static int SoLanDanh;

    /// <summary>Goi mot cu set danh xuong diem chi dinh.</summary>
    public static LightningStrike Spawn(Vector3 target, float height, float delay)
    {
        var go = new GameObject("LightningStrike");
        go.transform.position = target;

        var s = go.AddComponent<LightningStrike>();
        s.target = target;
        s.height = height;
        s.delay = delay;
        return s;
    }

    void Start()
    {
        // Vet sang bao truoc noi set sap danh
        if (delay > 0.01f) VfxFactory.LightningWarning(target, impactRadius, delay);
    }

    void Update()
    {
        t += Time.deltaTime;

        if (!fired && t >= delay)
        {
            fired = true;
            Strike();
        }

        if (t >= delay + 1.5f) Destroy(gameObject);
    }

    void Strike()
    {
        Vector3 from = target + Vector3.up * height;

        // Tia set chinh: lech nhe theo phuong ngang cho ra dang tu may giang xuong
        Vector2 drift = Random.insideUnitCircle * height * 0.09f;
        from += new Vector3(drift.x, 0f, drift.y);

        // KHIEN (16/10/2026, nguoi dung: set chi cham mat khien, khong xuyen vao trong): duong tia cham vom cua ai thi tia DUNG O MAT
        // VOM - dien bo tren vom, khien nuot don (TrungMatVom), nhung ke dung NGOAI sat vom van an tia lan quanh cho cham.
        Vector3 diemVom; float sVom;
        var kChan = Khieng.DoanChamVom(from, target, damageMask, boQua, out diemVom, out sVom);
        if (kChan != null) { DanhVaoVom(from, kChan, diemVom); return; }

        var arc = LightningArc.Create(from, target, 1f, 0.30f);
        arc.segments = 20;
        arc.branches = Random.Range(2, 4);

        VfxFactory.LightningImpact(target, impactRadius);
        // Tieng (16/10/2026): set xe xuong + no + ren ngan, va chay xem tai cho danh (mat dat hoac ke dich)
        SoLanDanh++;
        TiengViTri.SamSetDanh(target + Vector3.up);
        TiengViTri.SamSetChay(target + Vector3.up * 0.5f);

        // CHO NAO SET DANH XUONG THI CHO DO CHAY, khong can co quai hay khong.
        //
        // Luong khoi chi 0,45: Sam set tha 22 tia trong 2,4 giay, moi tia mot
        // dam khoi day nhu Giut set thi ca vung chim trong khoi va khong con
        // nhin ra san choi.
        VfxFactory.SetChayDen(target, impactRadius * 0.72f, 0.45f);

        // Chuong ngai vat quanh cho set danh bi NAM DEN va boc khoi tren be mat
        VfxFactory.NamChuongNgai(target, impactRadius, VfxFactory.LopChuongNgai);

        if (damage > 0f)
            CombatUtil.AreaShock(target, impactRadius, damage, damageMask,
                                 stunChance, stunSeconds, boQua);

        CameraShake.Shake(0.18f, 0.075f);
    }

    /// <summary>Dem cho phep thu (menu 136): so tia Sam set dung o mat vom khien; diem cham gan nhat.</summary>
    public static int SoChamVom;
    public static Vector3 DiemChamVomCuoi;

    /// <summary>Tia danh trung mat vom khien: tia tu troi toi mat vom, dien bo tren vom, khien nuot don; khong chay xem / vet nut /
    /// tia bo DAT (dat nam trong vom). Ke khac dung ngoai sat vom trong impactRadius quanh cho cham van an tia (khong tinh chu khien).</summary>
    void DanhVaoVom(Vector3 from, Khieng k, Vector3 diem)
    {
        SoChamVom++;
        DiemChamVomCuoi = diem;
        var arc = LightningArc.Create(from, diem, 1f, 0.30f);
        arc.segments = 20;
        arc.branches = Random.Range(2, 4);
        VfxFactory.DienBoTrenVom(k, diem, 1.25f);
        SoLanDanh++;
        TiengViTri.SamSetDanh(diem);
        TiengViTri.SamSetChay(diem);

        if (damage > 0f)
        {
            k.TrungMatVom(damage, DamageType.Lightning, diem, boQua);
            CombatUtil.AreaShock(diem, impactRadius, damage, damageMask, stunChance, stunSeconds, boQua, k.ChuNhan);
        }
        CameraShake.Shake(0.18f, 0.075f);
    }
}
