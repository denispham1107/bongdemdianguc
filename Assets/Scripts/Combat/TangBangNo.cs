using UnityEngine;

/// <summary>
/// TANG BANG NO KHI HET GIO (nguoi dung xin 19/09/2026, cho CA Mua bang va Qua cau bang o CAP 5).
///
/// "Khi skill dat cap 5, tang bang den thoi gian bien mat thay vi bien mat nhu binh thuong, thi se
/// no tung gay them 100 sat thuong cho doi thu va cac doi thu o gan do."
///
/// Hoi lai, nguoi dung chot: ban kinh no <see cref="BanKinh"/> = 3,4 m (bang dung vung no cua Qua cau
/// bang), va sat thuong DUNG 100 co dinh - khong nhan theo cap, khong nhan theo Tang hinh.
/// Giam dan tu tam ra ria nhu moi vu no khac (CombatUtil.AreaDamage).
///
/// Gan vao goc cua mot tang bang (vat "IceImpact" do VfxFactory.IceImpact sinh ra).
///
/// ⚠️ NO DUNG LUC CUM GAI BIEN MAT (sua 26/09/2026 - nguoi dung: "khoi bang bien mat mot luc roi moi no"):
/// ban cu hen gio theo AutoDestroy cua CA vat (4 giay) - nhung moi cum gai co ExpandFade RIENG 1,6-2,6 s,
/// mo dan roi tu xoa, nen gai mat truoc 1,3-2,3 s roi mat dat moi no. Nay luc gan: moi cum gai (va quang
/// chan) keo toi CUNG MOT MOC = cum song lau nhat, TAT mo dan (gai dung nguyen, dac), va vu no hen som hon
/// moc ay <see cref="NoSomHon"/> - gai con nguyen luc no, roi tan cung vu no (No() xoa ca tang).
/// </summary>
public class TangBangNo : MonoBehaviour
{
    /// <summary>Sat thuong vu no - nguoi dung chot 100 CO DINH (19/09/2026).</summary>
    public const float SatThuong = 100f;

    /// <summary>Ban kinh vu no - nguoi dung chon 3,4 m = QuaCauBang.BanKinhNo.</summary>
    public const float BanKinh = 3.4f;

    /// <summary>Cap ky nang tro len thi tang bang moi no.</summary>
    public const int CapNo = 5;

    /// <summary>No som hon luc bi xoa bang nay de chac chan Update kip chay.</summary>
    public const float NoSomHon = 0.12f;

    public LayerMask damageMask;
    public Damageable boQua;

    /// <summary>Dem cho phep thu (menu 68).</summary>
    public static int SoLanNo, SoLanTrung;

    float conLai = 4f;
    /// <summary>Moc (giay tu luc sinh) cum gai tan - phep thu doc.</summary>
    public float MocGaiTan { get; private set; }

    /// <summary>
    /// Keo moi cum gai + quang chan cua tang toi CUNG MOT MOC (cum song lau nhat) va tat mo dan cua cum gai.
    /// Tra ve moc ay (giay), 0 neu tang khong co cum gai nao.
    /// </summary>
    static float GiuGaiDenMoc(GameObject tang)
    {
        float moc = 0f;
        var ds = tang.GetComponentsInChildren<ExpandFade>(true);
        foreach (var ef in ds)
            if (ef.name.StartsWith("CumGai") || ef.name.StartsWith("Spike")) moc = Mathf.Max(moc, ef.duration);
        if (moc <= 0f) return 0f;
        foreach (var ef in ds)
        {
            bool gai = ef.name.StartsWith("CumGai") || ef.name.StartsWith("Spike");
            if (!gai && !ef.name.StartsWith("HaoQuang")) continue;
            // Giu TOC DO MOC cu: duong cong tinh theo ti le thoi luong - keo dai ma khong co lai thi gai troi len cham di
            if (gai && ef.duration > 0.01f && ef.duration < moc && ef.ease != null && ef.ease.length > 0)
            {
                float k = ef.duration / moc;
                var cu = ef.ease;
                var keys = new Keyframe[cu.length + 1];
                for (int i = 0; i < cu.length; i++)
                {
                    var kf = cu[i];
                    keys[i] = new Keyframe(kf.time * k, kf.value, kf.inTangent / k, kf.outTangent / k);
                }
                keys[cu.length] = new Keyframe(1f, cu.Evaluate(1f), 0f, 0f);
                ef.ease = new AnimationCurve(keys);
            }
            ef.duration = moc;
            if (gai) ef.fadeColorAlpha = false;      // gai dung dac toi luc no, khong mo dan
        }
        return moc;
    }
    bool daNo;

    static readonly Collider[] boDem = new Collider[64];

    /// <summary>
    /// Gan luat no vao mot tang bang. Goi sau khi <see cref="VfxFactory.IceImpact"/> dung xong -
    /// tra ve component de phep thu doc, null neu tang bang khong co (trung dat, khong co gai).
    /// </summary>
    public static TangBangNo Gan(GameObject tangBang, LayerMask damageMask, Damageable boQua)
    {
        if (tangBang == null) return null;
        var t = tangBang.AddComponent<TangBangNo>();
        t.damageMask = damageMask;
        t.boQua = boQua;
        var ad = tangBang.GetComponent<AutoDestroy>();
        float moc = GiuGaiDenMoc(tangBang);
        if (moc <= 0f) moc = ad != null ? ad.lifetime : 4f;          // khong co cum gai (ban dung bang code cu)
        t.conLai = Mathf.Max(0.2f, moc - NoSomHon);
        t.MocGaiTan = moc;
        return t;
    }

    void Update()
    {
        if (daNo) return;
        conLai -= Time.deltaTime;
        if (conLai > 0f) return;
        No();
    }

    void No()
    {
        daNo = true;
        Vector3 cho = transform.position;
        VfxFactory.NoTangBang(cho, BanKinh);
        CameraShake.Shake(0.14f, 0.05f);
        SoLanNo++;

        int n = Physics.OverlapSphereNonAlloc(cho, BanKinh, boDem, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = boDem[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            float xa = Vector3.Distance(cho, d.transform.position);
            float giam = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(xa / BanKinh));

            // GHI KE DANH TRUOC KHI DANH - khong thi giet bang vu no nay la ke giet vo danh:
            // khong ai duoc kinh nghiem, bang diem de trong (luat chung cua du an, menu 61 kiem).
            d.GhiKeDanh(boQua);
            d.TakeDamage(SatThuong * giam, DamageType.Ice, d.transform.position + Vector3.up * 1f);
            SoLanTrung++;
        }

        // Tang bang tan ngay sau vu no - khong de cai cum gai dung tro lai them mot khoanh khac
        Destroy(gameObject);
    }
}
