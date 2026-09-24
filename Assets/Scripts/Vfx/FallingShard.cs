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

    void Start()
    {
        start = transform.position;
        if (travelTime <= 0.01f) travelTime = 0.4f;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / travelTime);
        // Roi nhanh dan (gia toc)
        float e = k * k;
        transform.position = Vector3.Lerp(start, target, e);

        if (k >= 1f)
        {
            SoLanCham++;

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
                    SoLanCoGai++;
                    // CAP 5: tang bang het gio thi no tung, them 100 sat thuong quanh do
                    if (capKyNang >= TangBangNo.CapNo) TangBangNo.Gan(tang, damageMask, boQua);
                }
                // O cho roi van co chop sang / vong lanh / suong, chi khong co tang bang
                if (dsDongBang.Count > 1 || Vector3.Distance(dsDongBang[0].transform.position, target) > 0.6f)
                    VfxFactory.IceImpact(target, coHinh, false);
            }
            else
            {
                // Khong dong bang duoc ai (ke ca khi co trung va gay sat thuong): no binh thuong, KHONG tang bang
                VfxFactory.IceImpact(target, coHinh, false);
            }

            // Qua cau bang roi (Mua bang): tha luong khi lanh + vet bang ra tan dan nhu qua cau cua ky nang
            VfxFactory.ThaDuoiQuaCauBang(transform);
            Destroy(gameObject);
        }
    }
}
