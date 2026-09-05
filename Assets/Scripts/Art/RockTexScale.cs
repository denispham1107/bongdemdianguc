using UnityEngine;

/// <summary>
/// BU DO LAP HOA TIET THEO CO KHOI DA.
///
/// Anh mat da duoc trai theo toa do CUA CHINH KHOI DA. Nghia la khi dat mot khoi
/// da vao canh roi phong to no len 5 lan, hoa tiet cung bi keo gian ra 5 lan:
/// mot vien soi be bang nam tay bong hoa ra to bang cai ban. Do la ly do nhung
/// vach nui bao quanh ban do (phong to toi 6,8 lan) nhin lang bong nhu be tong,
/// trong khi hon da nho ngay canh nhan vat lai san sui binh thuong.
///
/// Script nay doc ti le phong to that cua khoi da roi NHAN NGUOC do lap len bay
/// nhieu lan, nen vien soi tren vach nui to bang dung vien soi tren hon da nho.
///
/// Dung MaterialPropertyBlock chu khong tao vat lieu rieng, nen khong sinh them
/// vat lieu trong bo nho va cac khoi da van dung chung mot vat lieu.
/// </summary>
[RequireComponent(typeof(Renderer))]
[ExecuteAlways]
public class RockTexScale : MonoBehaviour
{
    /// <summary>Duoi muc nay coi nhu khong phong to, khoi tinh cho nhanh.</summary>
    const float NguongBoQua = 0.02f;

    void OnEnable() { ApDung(); }

    void Start() { ApDung(); }

#if UNITY_EDITOR
    void OnValidate() { ApDung(); }
#endif

    void ApDung()
    {
        var r = GetComponent<Renderer>();
        if (r == null || r.sharedMaterial == null) return;

        // Lay co that sau khi da nhan het moi cap cha con
        Vector3 co = transform.lossyScale;
        float k = (Mathf.Abs(co.x) + Mathf.Abs(co.y) + Mathf.Abs(co.z)) / 3f;
        if (k < 0.0001f) return;
        if (Mathf.Abs(k - 1f) < NguongBoQua) return;

        var mat = r.sharedMaterial;
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);

        Vector2 nen = mat.mainTextureScale;
        block.SetVector("_MainTex_ST", new Vector4(nen.x * k, nen.y * k, 0f, 0f));

        if (mat.HasProperty("_DetailAlbedoMap_ST"))
        {
            Vector2 ct = mat.GetTextureScale("_DetailAlbedoMap");
            block.SetVector("_DetailAlbedoMap_ST", new Vector4(ct.x * k, ct.y * k, 0f, 0f));
        }

        r.SetPropertyBlock(block);
    }
}
