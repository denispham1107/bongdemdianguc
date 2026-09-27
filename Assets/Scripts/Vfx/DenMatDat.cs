using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DEN SINH DOI CHO MAT DAT - chua "o vuong, o chu nhat chap va" duoi dat moi khi ky nang phat sang.
///
/// Nguoi dung (27/09/2026, 6 anh): quai hay nguoi choi danh ky nang phat sang thi mat dat khap ban do hien ro cac o
/// vuong, o chu nhat sang deu. Nguyen nhan (menu 89 do): Unity ve Terrain thanh NHIEU MANH va chia den cho TUNG MANH.
/// Moi vat chi co <c>QualitySettings.pixelLightCount</c> suat den diem anh (Cao 2, Trung binh 1, Yeu / Rat yeu 0 - muc
/// mac dinh la Yeu); den het suat bi ha xuong den dinh / SH tinh MOT LAN cho CA VAT. Voi bia, da, cay thi khong ai thay,
/// nhung mot manh dat rong ca chuc met sang deu mot mau thi lo ngay thanh o.
///
/// Nang suat den cho moi vat (thu 8) thi het o vuong nhung dat: lo lua, den quanh nhan vat... thanh den diem anh tren
/// MOI bia, cay, da trong tam - muc Yeu 455 -> 922 SetPass khi danh 6 den. Nen chi mat dat duoc tinh diem anh:
/// moi den diem / den chieu (tru den huong) co mot DEN SINH DOI la con cua no, <c>ForcePixel</c> (luon diem anh, khong
/// tinh vao suat), chi chieu lop Ground (chi co Terrain); den goc thoi chieu Ground. Vat nho van nhan den goc re nhu cu.
///
/// Bat den moi ngay trong <see cref="Camera.onPreCull"/> - truoc khi Unity chia den cua CHINH khung hinh ay - nen den
/// sinh ra giua khung (vu no) khong lo o vuong mot khung nao. Den sinh doi la con nen di theo, tat theo, bi xoa theo den
/// goc; mau / do sang / tam / goc chieu chep lai moi khung (den nhap nhay, den loe).
/// </summary>
public static class DenMatDat
{
    /// <summary>Tat de do doi chung (menu 89): tra lai mat na cho moi den goc va xoa het den sinh doi.</summary>
    public static bool Bat = true;

    public const string TenDenDoi = "DenMatDat";

    static readonly Dictionary<Light, Light> doi = new Dictionary<Light, Light>();
    static readonly HashSet<Light> laDoi = new HashSet<Light>();
    static readonly List<Light> tam = new List<Light>();
    static int khungDaQuet = -1;
    static int lopDat = -1;

    /// <summary>So den dang co den sinh doi (phep thu doc).</summary>
    public static int SoDenDangTach { get { return doi.Count; } }

    // SubsystemRegistration: du an TAT Domain Reload - bien tinh con giu tu lan Play truoc.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void KhoiDong()
    {
        doi.Clear(); laDoi.Clear(); khungDaQuet = -1; lopDat = -1; Bat = true;
        Camera.onPreCull -= TruocKhiVe;
        Camera.onPreCull += TruocKhiVe;
    }

    static void TruocKhiVe(Camera cam)
    {
        // Thoat Play thi uy thac van con (khong nap lai domain) - dung de camera Scene view dung den den trong canh.
        if (!Application.isPlaying) return;
        if (khungDaQuet == Time.frameCount) return;     // nhieu camera: mot lan moi khung la du
        khungDaQuet = Time.frameCount;

        if (lopDat < 0) lopDat = LayerMask.NameToLayer("Ground");
        if (lopDat < 0) return;
        int matDat = 1 << lopDat;

        DonCapDaMat(matDat);
        if (!Bat) { TraLaiHet(matDat); return; }

        var ds = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < ds.Length; i++)
        {
            var l = ds[i];
            if (l == null || laDoi.Contains(l) || doi.ContainsKey(l)) continue;
            if (l.type != LightType.Point && l.type != LightType.Spot) continue;
            if (l.renderMode == LightRenderMode.ForcePixel) continue;   // da la den diem anh o moi vat
            if ((l.cullingMask & matDat) == 0) continue;
            TaoDoi(l, matDat);
        }

        foreach (var kv in doi) DongBo(kv.Key, kv.Value);
    }

    static void TaoDoi(Light goc, int matDat)
    {
        var go = new GameObject(TenDenDoi);
        go.layer = goc.gameObject.layer;
        go.transform.SetParent(goc.transform, false);
        var d = go.AddComponent<Light>();
        d.type = goc.type;
        d.renderMode = LightRenderMode.ForcePixel;
        d.cullingMask = goc.cullingMask & matDat;
        d.shadows = goc.shadows;
        goc.cullingMask &= ~matDat;
        doi[goc] = d;
        laDoi.Add(d);
        DongBo(goc, d);
    }

    static void DongBo(Light goc, Light d)
    {
        if (goc == null || d == null) return;
        d.enabled = goc.enabled;
        d.color = goc.color;
        d.intensity = goc.intensity;
        d.range = goc.range;
        if (goc.type == LightType.Spot) { d.spotAngle = goc.spotAngle; d.innerSpotAngle = goc.innerSpotAngle; }
    }

    /// <summary>
    /// Bo cap da hong: den goc bi xoa (ca vat the thi den doi - con cua no - di theo; chi xoa rieng component Light thi
    /// den doi con lai, phai xoa tay), hoac den doi bi ai xoa (tra mat Ground cho den goc de nhip sau tao lai).
    /// </summary>
    static void DonCapDaMat(int matDat)
    {
        tam.Clear();
        foreach (var kv in doi) if (kv.Key == null || kv.Value == null) tam.Add(kv.Key);
        for (int i = 0; i < tam.Count; i++)
        {
            var goc = tam[i];
            Light d = doi[goc];
            if (goc == null) { if (d != null) Object.Destroy(d.gameObject); }
            else goc.cullingMask |= matDat;
            doi.Remove(goc);
        }
        laDoi.RemoveWhere(x => x == null);
    }

    static void TraLaiHet(int matDat)
    {
        foreach (var kv in doi)
        {
            if (kv.Key != null) kv.Key.cullingMask |= matDat;
            if (kv.Value != null) Object.Destroy(kv.Value.gameObject);
        }
        doi.Clear(); laDoi.Clear();
    }
}
