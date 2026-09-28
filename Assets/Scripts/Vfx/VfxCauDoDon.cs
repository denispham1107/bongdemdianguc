using UnityEngine;

/// <summary>
/// QUA CAU BAO VE khi Bo xuong DO DON (ChongDo, nguoi dung 28/09/2026: "xuat hien 1 cau bao ve va hien kem chu Block").
///
/// Dung lai VOM KHIENG cua ky nang Khieng (luoi LuoiVom + shader Diablo25D/Khieng - shader nay da nam trong ban build) nhung
/// mot ban vat lieu RIENG to mau BAC XANH, khac vang kim cua khieng nguoi choi: nhin la biet day la quai do, khong phai ai
/// vua bat khieng. Loe to tu 55% len du co trong 0,1 s (_Loe 1 -> 0), giu roi mo han (_TrongSuot -> 0) va tu xoa sau 0,55 s.
/// Gan vao con quai nen di theo no.
/// </summary>
public static partial class VfxFactory
{
    public const float GiayCauDoDon = 0.55f;

    public static GameObject CauDoDon(Transform quai, float caoThan)
    {
        if (quai == null) return null;
        float banKinh = Mathf.Max(0.6f, caoThan * 0.55f);
        var root = new GameObject("CauDoDon");
        root.transform.SetParent(quai, false);
        root.transform.localPosition = Vector3.zero;

        var mat = new Material(KhiengMat) { name = "M_CauDoDon" };
        mat.SetColor("_VienColor", new Color(0.78f, 0.90f, 1f, 1f));
        mat.SetColor("_VanColor", new Color(0.62f, 0.80f, 1f, 1f));
        mat.SetColor("_DomColor", new Color(0.92f, 0.97f, 1f, 1f));
        mat.SetColor("_LoeColor", new Color(0.90f, 0.96f, 1f, 1f));
        mat.SetFloat("_MatTruoc", 0.6f);

        var qua = ProcMesh.Part("QuaCau", root.transform, LuoiVom(banKinh), mat,
                                Vector3.zero, Quaternion.identity, Vector3.one, false);
        var mr = qua.GetComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        var h = root.AddComponent<CauDoDonHinh>();
        h.mat = mat;
        return root;
    }
}

/// <summary>Loe to roi mo tat cua qua cau do don.</summary>
public class CauDoDonHinh : MonoBehaviour
{
    public Material mat;
    float t;
    float trongSuotGoc = 0.85f;

    void Start()
    {
        if (mat != null && mat.HasProperty("_TrongSuot")) trongSuotGoc = mat.GetFloat("_TrongSuot");
        transform.localScale = Vector3.one * 0.55f;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / 0.10f);
        transform.localScale = Vector3.one * Mathf.Lerp(0.55f, 1f, 1f - (1f - k) * (1f - k));
        if (mat != null)
        {
            mat.SetFloat("_Loe", 1f - k);
            float mo = 1f - Mathf.Clamp01((t - 0.2f) / (VfxFactory.GiayCauDoDon - 0.2f));
            mat.SetFloat("_TrongSuot", trongSuotGoc * mo);
            mat.SetFloat("_Yeu", 1f);
        }
        if (t >= VfxFactory.GiayCauDoDon) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}
