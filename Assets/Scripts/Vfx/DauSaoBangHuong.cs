using UnityEngine;

/// <summary>
/// DAU VET SAO BANG cua Mua bang (17/09/2026). Nguoi dung: dau vet "hoi nhon nhon va co rang cua mot chut" cho giong sao
/// bang that, mui chi XUONG DAT (huong roi). Hat billboard xoay ngau nhien khong giu duoc huong mui, nen dau la mot tam
/// tu giac rieng: truc len (+Y cua tam, cung la mui trong anh DauSaoBang) luon trung huong roi, mat tam quay ve camera
/// quanh truc do (billboard theo truc).
///
/// Anh DauSaoBang.png (Blender MCP): dom sang o y = -0,30 (toa do anh -1..1), mui thuon nhon ve +y, rang cua o hai suon.
/// Tam dom phai trung dau vet (vi tri qua) -> tam tu giac lech ve phia mui 0,30 x nua chieu cao.
/// </summary>
public class DauSaoBangHuong : MonoBehaviour
{
    /// <summary>Vi tri dom sang trong anh theo truc doc (-1..1).</summary>
    public const float DomSangY = -0.30f;

    /// <summary>Huong roi (the gioi). Mua bang roi thang dung.</summary>
    public Vector3 huongRoi = Vector3.down;

    public float rong = 1f;

    static Mesh tuGiac;

    public static Mesh TuGiac
    {
        get
        {
            if (tuGiac == null)
            {
                tuGiac = new Mesh { name = "TuGiacDauSaoBang" };
                tuGiac.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) };
                tuGiac.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
                tuGiac.colors = new[] { Color.white, Color.white, Color.white, Color.white };
                tuGiac.triangles = new[] { 0, 2, 1, 2, 3, 1 };
                tuGiac.RecalculateBounds();
            }
            return tuGiac;
        }
    }

    void LateUpdate()
    {
        var cha = transform.parent;
        if (cha == null) return;
        Vector3 len = huongRoi.normalized;
        float cao = rong * 2f;
        transform.localScale = new Vector3(rong, cao, 1f);
        transform.position = cha.position + len * (-DomSangY * cao * 0.5f);

        var cam = Camera.main;
        Vector3 toiCam = cam != null ? cam.transform.position - transform.position : Vector3.back;
        toiCam -= len * Vector3.Dot(toiCam, len);
        if (toiCam.sqrMagnitude < 1e-6f) toiCam = Vector3.Cross(len, Vector3.right);
        transform.rotation = Quaternion.LookRotation(-toiCam.normalized, len);
    }
}
