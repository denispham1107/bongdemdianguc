using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CHAY DEN TOAN THAN khi bi tia set May giong danh trung (nguoi dung 25/09/2026: "ke dich khi bi tia set danh trung se co
/// hieu ung bi chay den toan than"; chon "chi hinh, 3 giay" - khong them sat thuong).
///
/// Phu THEM mot lop vat lieu than den (anh Blender Resources/KyNang/MayGiong/ChayDen.png: than den + vet nut hong lua) len
/// moi MeshRenderer VA SkinnedMeshRenderer (bai hoc vo bang 14/09/2026: model co xuong la SkinnedMesh), kem khoi den boc len.
/// Hien nhanh 0,12 s, giu nguyen, phai dan trong 1 giay cuoi; bi danh tiep thi tinh lai tu dau.
/// Go ra chi bo DUNG lop cua minh (loc theo tham chieu vat lieu) - khong tra ca mang goc nhu FrozenEffect, de khong de mat
/// vo bang neu hai hieu ung chong nhau. Tang hinh chan (TangHinh.ChanHieuUng) nhu moi hieu ung khac.
/// </summary>
public class ChayDenToanThan : MonoBehaviour
{
    public const float GiayMacDinh = 3f;
    const float GiayHien = 0.12f, GiayPhai = 1f;
    const string AnhChayDen = "KyNang/MayGiong/ChayDen";

    float conLai, tong;
    Material lop;
    readonly List<Renderer> daPhu = new List<Renderer>();
    GameObject khoi;

    static Texture2D anhChayDen;
    public static int SoLanApDung;

    /// <summary>Do dam lop than den hien tai (0..1) - phep thu doc.</summary>
    public float DoDam { get; private set; }
    public int SoRendererDaPhu { get { return daPhu.Count; } }

    public static ChayDenToanThan Apply(Damageable d, float giay)
    {
        if (d == null) return null;
        if (TangHinh.ChanHieuUng(d)) return null;
        var c = d.GetComponent<ChayDenToanThan>();
        if (c == null) c = d.gameObject.AddComponent<ChayDenToanThan>();
        c.conLai = giay; c.tong = giay;
        SoLanApDung++;
        return c;
    }

    void Start()
    {
        if (anhChayDen == null) anhChayDen = Resources.Load<Texture2D>(AnhChayDen);
        lop = Mats.Alpha("P_ChayDen", anhChayDen, new Color(1f, 1f, 1f, 0f));

        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r == null || !r.enabled) continue;
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;
            // Bo renderer TRONG SUOT (vom khien, hat, tia set con gan vao nguoi) - boc than len chung chi ra mot dam mo
            bool trongSuot = false;
            for (int k = 0; k < mats.Length; k++) if (mats[k] == null || mats[k].renderQueue >= 3000) trongSuot = true;
            if (trongSuot) continue;
            var moi = new Material[mats.Length + 1];
            for (int k = 0; k < mats.Length; k++) moi[k] = mats[k];
            moi[mats.Length] = lop;
            r.sharedMaterials = moi;
            daPhu.Add(r);
        }

        khoi = VfxFactory.KhoiChayDen(transform);
    }

    void Update()
    {
        conLai -= Time.deltaTime;
        float daQua = tong - conLai;
        float k = Mathf.Clamp01(daQua / GiayHien) * Mathf.Clamp01(conLai / GiayPhai);
        DoDam = k;
        if (lop != null) lop.SetColor("_TintColor", new Color(1f, 1f, 1f, k));
        if (conLai <= 0f) Destroy(this);
    }

    void OnDestroy()
    {
        for (int i = 0; i < daPhu.Count; i++)
        {
            var r = daPhu[i];
            if (r == null) continue;
            var mats = r.sharedMaterials;
            var giu = new List<Material>(mats.Length);
            for (int k = 0; k < mats.Length; k++) if (mats[k] != lop) giu.Add(mats[k]);
            r.sharedMaterials = giu.ToArray();
        }
        daPhu.Clear();
        if (khoi != null)
        {
            foreach (var ps in khoi.GetComponentsInChildren<ParticleSystem>()) { var em = ps.emission; em.enabled = false; }
            Destroy(khoi, 2f);
        }
        if (lop != null) Destroy(lop);
    }
}
