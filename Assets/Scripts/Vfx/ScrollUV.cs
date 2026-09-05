using UnityEngine;

/// <summary>
/// Keo hoa tiet truot lien tuc tren be mat - dung cho than loc xoay cuon tron.
/// </summary>
public class ScrollUV : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.6f, -0.25f);

    Material mat;
    Vector2 offset;

    void Start()
    {
        var r = GetComponent<Renderer>();
        if (r != null) mat = r.material;      // ban sao rieng, khong dung chung
    }

    void Update()
    {
        if (mat == null) return;
        offset += speed * Time.deltaTime;
        offset.x -= Mathf.Floor(offset.x);
        offset.y -= Mathf.Floor(offset.y);
        mat.mainTextureOffset = offset;
    }

    void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}
