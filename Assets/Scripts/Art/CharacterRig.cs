using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BO XUONG cua mot nhan vat (phu thuy hoac quai).
///
/// Moi khop (hong, nguc, dau, vai, khuyu, dau goi...) la mot GameObject rong.
/// Hinh khoi (thit, ao, giap) duoc gan lam con cua khop, nen khi xoay khop
/// thi ca cum hinh xoay theo - giong nhu xuong keo thit vay.
/// </summary>
public class CharacterRig : MonoBehaviour
{
    [Header("Khop chinh")]
    public Transform root;          // toan than (de lac / nga khi chet)
    public Transform hips;
    public Transform spine;
    public Transform chest;
    public Transform neck;
    public Transform head;

    [Header("Tay")]
    public Transform armL, armR;        // vai
    public Transform foreArmL, foreArmR; // khuyu
    public Transform handL, handR;

    [Header("Chan")]
    public Transform legL, legR;        // hong -> dui
    public Transform shinL, shinR;      // dau goi
    public Transform footL, footR;

    [Header("Diem gan")]
    public Transform weaponAnchor;      // tay phai cam vu khi
    public Transform offHandAnchor;     // tay trai
    public Transform castPoint;         // dau gay / mieng - noi phep bay ra
    public Transform hitPoint;          // giua nguoi - noi hien so sat thuong

    [Header("Kich thuoc")]
    public float bodyHeight = 1.8f;
    public float bodyRadius = 0.35f;

    /// <summary>Cac mieng hinh co the doi mau khi trung don.</summary>
    public readonly List<Renderer> skinRenderers = new List<Renderer>();

    /// <summary>Cac mieng mat phat sang.</summary>
    public readonly List<Renderer> eyeRenderers = new List<Renderer>();

    // ================================================================
    //  NHAT LAI LOP DA SAU KHI NAP TU PREFAB
    // ================================================================

    /// <summary>
    /// Nhat lai danh sach lop da neu no rong.
    ///
    /// VI SAO CAN: <see cref="skinRenderers"/> khai bao la <c>readonly</c>, ma
    /// truong readonly thi Unity KHONG luu vao prefab. Nhan vat dung bang code
    /// luc chay thi danh sach day du, nhung tu ngay chuyen sang nuong san thanh
    /// prefab thi moi prefab nap len deu co danh sach RONG - va SetFlash duyet
    /// mot danh sach rong tuc la KHONG LAM GI. Don trung khong con loe sang nua
    /// ma khong ai hay, vi no hong lang le chu khong bao loi.
    ///
    /// Bo qua may mieng mat va hoc mat: chung von tu phat sang san, cho nhap
    /// nhay theo don danh thi mat het ve trung trung.
    /// </summary>
    void Awake()
    {
        if (skinRenderers.Count > 0) return;

        var tatCa = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < tatCa.Length; i++)
        {
            var r = tatCa[i];
            if (r == null) continue;

            string ten = r.name;
            if (ten.StartsWith("Eye") || ten.StartsWith("HocMat")) continue;

            skinRenderers.Add(r);
        }
    }

    // ================================================================
    //  HAM DUNG XUONG
    // ================================================================

    public Transform Bone(string name, Transform parent, Vector3 localPos)
    {
        var t = ProcMesh.Joint(name, parent == null ? transform : parent, localPos);
        return t;
    }

    /// <summary>Gan mot mieng hinh vao khop.</summary>
    public GameObject Attach(string name, Transform bone, Mesh mesh, Material mat,
                             Vector3 pos, Quaternion rot, Vector3 scale, bool isSkin)
    {
        var go = ProcMesh.Part(name, bone, mesh, mat, pos, rot, scale, true);
        if (isSkin)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) skinRenderers.Add(r);
        }
        return go;
    }

    public GameObject Attach(string name, Transform bone, Mesh mesh, Material mat, Vector3 pos)
    {
        return Attach(name, bone, mesh, mat, pos, Quaternion.identity, Vector3.one, true);
    }

    public GameObject AttachEye(string name, Transform bone, Mesh mesh, Material mat, Vector3 pos)
    {
        var go = ProcMesh.Part(name, bone, mesh, mat, pos, Quaternion.identity, Vector3.one, false);
        var r = go.GetComponent<Renderer>();
        if (r != null) eyeRenderers.Add(r);
        return go;
    }

    /// <summary>Doi mau nhap nhay khi an don (goi tu Damageable).</summary>
    public void SetFlash(Color c, float amount)
    {
        for (int i = 0; i < skinRenderers.Count; i++)
        {
            var r = skinRenderers[i];
            if (r == null) continue;
            var m = r.material;              // ban sao rieng cua tung quai
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", c * amount);
            }
        }
    }
}
