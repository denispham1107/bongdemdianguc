using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TIA DIEN BO TREN MAT DAT o cho tia Sam set / May giong cham dat (nguoi dung 05/10/2026 chon mau A: "khi danh trung vo ra cac hat
/// hoac hinh giong dau gach sang rat khong tu nhien" - hai lop hat KEO DAI THEO HUONG BAY Sparks / Jet cua prefab Vfx_SetChamDat ve
/// thanh vet gach thang). Thay bang cac TIA SET CON cung kieu tia chinh (LightningArc mac dinh nhu LightningStrike.Strike) bo ngang
/// mat dat toa ra tu cho cham, chop tat theo nhip cua LightningArc roi tan:
///   dot 1 (luc cham)  : SoTiaDot1 tia, deu quanh vong + lech ngau nhien, dai 0,6-1,05 ban kinh;
///   dot 2 (sau 0,09 s): SoTiaDot2 tia moc tu giua cac tia dot 1 bo tiep ra ngoai 0,5-1 m (thay "lan" tren dat).
/// Hai dau moi tia bam MAT DAT (chi lop Ground) + NangKhoiDat. Chi hinh, khong sat thuong.
/// Loc xoay (05/10/2026, cung mau): <c>bamTheo</c> = con loc - moi tia LightningArc.BamTheo, dot 2 doi theo quang loc da di.
/// </summary>
public class TiaBoDat : MonoBehaviour
{
    public const int SoTiaDot1 = 6, SoTiaDot2 = 3;
    public const float GiayDot2 = 0.09f;
    /// <summary>Nang tia len khoi dat (m) - duong di gap khuc lech len/xuong ~0,1 m, sat qua thi chui xuong dat.</summary>
    public const float NangKhoiDat = 0.12f;
    /// <summary>Be ngang tia con so voi tia Sam set (LightningArc.Create widthScale).</summary>
    public const float NgangTiaCon = 0.32f;

    /// <summary>Phep thu (menu 106) doc: so tia con da sinh, so lan tao (moi cho cham mot lan), so lan bo dot 2 (bi xoa truoc 0,09 s).</summary>
    public static int SoTiaDaSinh, SoLanTao, SoLanMatDot2;

    Vector3 tam;
    float banKinh, t;
    Transform bamTheo;
    Vector3 gocBamTheo;
    bool daDot2;
    readonly List<Vector3> dauDot2 = new List<Vector3>();
    readonly List<Vector3> huongDot2 = new List<Vector3>();

    static LayerMask matDat;
    static bool coMat;

    public static TiaBoDat Tao(Vector3 cho, float banKinh, Transform bamTheo = null)
    {
        var go = new GameObject("TiaBoDat");
        var tb = go.AddComponent<TiaBoDat>();
        tb.bamTheo = bamTheo;
        if (bamTheo != null) tb.gocBamTheo = bamTheo.position;
        tb.tam = new Vector3(cho.x, DatY(cho) , cho.z);
        go.transform.position = tb.tam;
        tb.banKinh = banKinh;
        SoLanTao++;
        tb.Dot1();
        return tb;
    }

    /// <summary>Do cao mat dat CHI lop Ground (GroundY gom ca bia / da - tia se treo len noc bia).</summary>
    public static float DatY(Vector3 p)
    {
        if (!coMat) { matDat = LayerMask.GetMask("Ground"); coMat = true; }
        RaycastHit h;
        if (Physics.Raycast(new Vector3(p.x, p.y + 6f, p.z), Vector3.down, out h, 30f, matDat, QueryTriggerInteraction.Ignore))
            return h.point.y;
        return p.y;
    }

    static Vector3 TrenDat(Vector3 p)
    {
        p.y = DatY(p) + NangKhoiDat;
        return p;
    }

    void Dot1()
    {
        float goc0 = Random.Range(0f, 360f);
        for (int i = 0; i < SoTiaDot1; i++)
        {
            float g = (goc0 + i * 360f / SoTiaDot1 + Random.Range(-22f, 22f)) * Mathf.Deg2Rad;
            Vector3 h = new Vector3(Mathf.Cos(g), 0f, Mathf.Sin(g));
            float dai = banKinh * Random.Range(0.6f, 1.05f);
            Vector3 a = TrenDat(tam + h * 0.08f), b = TrenDat(tam + h * dai);
            VeTia(a, b, Random.Range(0.22f, 0.30f), bamTheo);
            dauDot2.Add(tam + h * dai * Random.Range(0.35f, 0.65f));
            huongDot2.Add(h);
        }
    }

    void Update()
    {
        t += Time.deltaTime;
        if (!daDot2 && t >= GiayDot2)
        {
            daDot2 = true;
            Vector3 diDuoc = Vector3.zero;
            if (bamTheo != null) { diDuoc = bamTheo.position - gocBamTheo; diDuoc.y = 0f; }
            for (int i = 0; i < SoTiaDot2 && dauDot2.Count > 0; i++)
            {
                int k = Random.Range(0, dauDot2.Count);
                Vector3 h = Quaternion.Euler(0f, Random.Range(-40f, 40f), 0f) * huongDot2[k];
                Vector3 a = TrenDat(dauDot2[k] + diDuoc), b = TrenDat(dauDot2[k] + diDuoc + h * Random.Range(0.5f, 1.0f));
                VeTia(a, b, Random.Range(0.16f, 0.22f), bamTheo);
                dauDot2.RemoveAt(k); huongDot2.RemoveAt(k);
            }
            Destroy(gameObject, 0.05f);
        }
    }

    public const string TenTia = "TiaBoDat";

    /// <summary>Tia con moi sinh gan nhat (phep thu doc).</summary>
    public static LightningArc TiaMoiNhat;

    void OnDestroy() { if (!daDot2) SoLanMatDot2++; }

    static void VeTia(Vector3 a, Vector3 b, float song, Transform bamTheo)
    {
        SoTiaDaSinh++;
        float dai = Vector3.Distance(a, b);
        var arc = LightningArc.Create(a, b, NgangTiaCon, song);
        arc.name = TenTia;   // phep thu tia chinh (menu 82 E1, 71) bo qua tia con theo ten
        arc.segments = Mathf.Clamp(Mathf.RoundToInt(dai * 6f), 5, 14);
        arc.jitter = 0.9f;
        arc.branches = Random.Range(1, 3);
        arc.branchLength = 0.45f;
        if (bamTheo != null) arc.BamTheo(bamTheo);
        TiaMoiNhat = arc;
    }
}
