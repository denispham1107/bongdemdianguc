using System.Collections.Generic;
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

    /// <summary>Cap ky nang cua NGUOI TUNG (di kem goi tin) - cap 5 thi tang bang no khi het gio.</summary>
    public int capKyNang = 1;

    /// <summary>Dem cho phep thu: so lo lua Mua bang da dap tat.</summary>
    public static int SoLoDapTat;

    /// <summary>Dem cho phep thu: so lan cham dat / so tang bang da moc.</summary>
    public static int SoLanCham, SoLanCoGai;

    /// <summary>So ke bi dong bang o cu roi gan nhat - phep thu (menu 68) doc.</summary>
    public static int SoDongBangLanCuoi;

    static readonly List<Damageable> dsDongBang = new List<Damageable>();

    TiengViTri.Giong giongRoi;

    /// <summary>Dem cho phep thu (menu 136): so vien vo tren mat vom khien; diem vo gan nhat.</summary>
    public static int SoVoTrenVom;
    public static Vector3 DiemVoVomCuoi;

    // Khien vien bang vua cham (hoi MOI KHUNG tren doan vua roi - nguoi co khien di chuyen trong luc vien dang roi)
    Khieng khiengChan;
    Vector3 diemVom;

    void Start()
    {
        start = transform.position;
        if (travelTime <= 0.01f) travelTime = 0.4f;
        // Tieng qua bang ROI (16/10/2026): bam theo qua, to nhat luc cham dat (clip 0,96 s = IceStorm.fallTime)
        giongRoi = TiengViTri.MuaBangRoi(transform);
    }

    void OnDestroy() { TiengViTri.Dung(giongRoi, transform); }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / travelTime);
        // Roi nhanh dan (gia toc)
        float e = k * k;
        Vector3 moi = Vector3.Lerp(start, target, e);

        // KHIEN (16/10/2026, nguoi dung: bang chi cham mat khien, khong xuyen vao trong): doan vua roi cham mat vom cua ai thi vo
        // tung o do, khien nuot don
        float sVom;
        khiengChan = Khieng.DoanChamVom(transform.position, moi, damageMask, boQua, out diemVom, out sVom);
        if (khiengChan == null) khiengChan = Khieng.TrongVom(moi, damageMask, boQua, out diemVom);   // vom chay toi trum len vien bang
        if (khiengChan != null) { VoTrenVom(); return; }

        transform.position = moi;

        if (k >= 1f)
        {
            SoLanCham++;
            TiengViTri.Dung(giongRoi, transform);
            TiengViTri.MuaBangCham(target);

            // ⚠️ GAY SAT THUONG TRUOC, DUNG HINH SAU (doi thu tu tu 19/09/2026).
            // Nguoi dung: "chi khi nao lam Dong Bang doi thu thanh cong thi moi cho xuat hien tang bang
            // o duoi dat (moi doi thu bi dong bang thanh cong se cho xuat hien 1 tang bang)". Muon biet
            // ai bi dong bang thi phai gieo xac suat xong da - nen khong the ve hinh truoc nhu ban cu.
            //
            // Truoc day luat la "trong 1,7 m co ke dich nao con song thi moc gai" (TrungKeDich) - tuc
            // trung la co tang bang, ke ca khi khong dong bang duoc ai. Da bo han.
            dsDongBang.Clear();
            int soDong = 0;
            if (damage > 0f)
            {
                // AreaFreeze chu khong AreaDamage: AreaDamage ap statusSeconds cho
                // TAT CA muc tieu trung don, tuc cu cham la dong bang het - khong
                // con la mot xac suat nua.
                CombatUtil.AreaFreeze(target, impactRadius, damage, damageMask,
                                      chamTiLe, chamGiay,
                                      freezeChance, freezeSeconds, boQua, out soDong, dsDongBang);
            }
            SoDongBangLanCuoi = soDong;

            // Trung LO LUA thi dap tat lua nhu Gio loc (nguoi dung 25/09/2026) - lo nam trong vung sat thuong
            if (damage > 0f) SoLoDapTat += LoLuaDa.DapTatTrongVung(target, impactRadius, GioLoc.GiayLoChayLai);

            // HINH cum bang to bang cua Qua cau bang (2,55 m, prefab nuong o 1,7 -> x1,5). Nguoi dung
            // 16/09/2026 xin. CHI hinh to ra: vung sat thuong / dong bang ben duoi van la impactRadius.
            float coHinh = Mathf.Max(impactRadius, QuaCauBang.BanKinhHinhBang);

            if (dsDongBang.Count > 0)
            {
                // MOI KE BI DONG BANG MOT TANG - moc ngay duoi chan ke ay, khong phai mot cum o tam
                for (int i = 0; i < dsDongBang.Count; i++)
                {
                    var d = dsDongBang[i];
                    if (d == null) continue;
                    Vector3 cho = d.transform.position;
                    cho.y = VfxFactory.GroundY(cho);
                    var tang = VfxFactory.IceImpact(cho, coHinh, true);
                    VfxFactory.PhongManhVo(tang, VfxFactory.HeSoManhVoMuaBang);
                    SoLanCoGai++;
                    // Tang bang het gio thi no tung, them 100 sat thuong quanh do (tu cap TangBangNo.CapNo = 1, truoc 5)
                    if (capKyNang >= TangBangNo.CapNo) TangBangNo.Gan(tang, damageMask, boQua);
                }
                // O cho roi van co chop sang / vong lanh / suong, chi khong co tang bang
                if (dsDongBang.Count > 1 || Vector3.Distance(dsDongBang[0].transform.position, target) > 0.6f)
                    VfxFactory.PhongManhVo(VfxFactory.IceImpact(target, coHinh, false), VfxFactory.HeSoManhVoMuaBang);
            }
            else
            {
                // Khong dong bang duoc ai (ke ca khi co trung va gay sat thuong): no binh thuong, KHONG tang bang
                VfxFactory.PhongManhVo(VfxFactory.IceImpact(target, coHinh, false), VfxFactory.HeSoManhVoMuaBang);
            }

            // Qua cau bang roi (Mua bang): tha luong khi lanh + vet bang ra tan dan nhu qua cau cua ky nang
            VfxFactory.ThaDuoiQuaCauBang(transform);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Vien bang cham mat vom: vo tung TAI MAT VOM (manh + suong toe ra ngoai), khien nuot don (TrungMatVom - khong lam cham / dong
    /// bang chu khien). Ke dung NGOAI sat vom trong impactRadius quanh cho vo van an nhu thuong (tru chu khien). Khong cum bang,
    /// khong vet suong gia, khong dap lo duoi dat - vien bang khong toi duoc dat.
    /// </summary>
    void VoTrenVom()
    {
        SoVoTrenVom++;
        DiemVoVomCuoi = diemVom;
        transform.position = diemVom;
        TiengViTri.Dung(giongRoi, transform);
        TiengViTri.MuaBangCham(diemVom);
        VfxFactory.BangVoTrenVom(khiengChan, diemVom, VfxFactory.HeSoManhVoMuaBang);
        if (damage > 0f)
        {
            khiengChan.TrungMatVom(damage, DamageType.Ice, diemVom, boQua);
            int soDong;
            CombatUtil.AreaFreeze(diemVom, impactRadius, damage, damageMask, chamTiLe, chamGiay,
                                  freezeChance, freezeSeconds, boQua, out soDong, null, khiengChan.ChuNhan);
        }
        VfxFactory.ThaDuoiQuaCauBang(transform);
        Destroy(gameObject);
    }
}
