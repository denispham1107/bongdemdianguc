using UnityEngine;

/// <summary>
/// HIEU UNG LOE SANG (Bloom) tu viet.
///
/// Lay nhung diem sang nhat tren man hinh, lam nhoe chung ra roi cong tro lai.
/// Nho vay lua va bang trong nhu dang PHAT SANG that su, khong phai mau phang.
/// Khong can cai package Post Processing nao.
/// </summary>
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SimpleBloom : MonoBehaviour
{
    [Header("Loc vung sang")]
    [Tooltip("Sang hon muc nay moi bi loe.")]
    public float threshold = 1.15f;
    [Range(0f, 1f)] public float softKnee = 0.55f;

    [Header("Do manh")]
    public float intensity = 0.9f;
    [Range(1, 6)] public int iterations = 4;

    [Header("Toi 4 goc man hinh")]
    public float vignette = 0.85f;

    Material mat;
    Shader shader;

    void OnEnable()
    {
        var cam = GetComponent<Camera>();
        if (cam != null) cam.allowHDR = true;
    }

    void OnDisable()
    {
        if (mat != null) DestroyImmediate(mat);
        mat = null;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (shader == null) shader = Shader.Find("Diablo25D/Bloom");
        if (shader == null || !shader.isSupported) { Graphics.Blit(src, dst); return; }
        if (mat == null) mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

        int w = Mathf.Max(2, src.width / 2);
        int h = Mathf.Max(2, src.height / 2);
        var fmt = src.format;

        var a = RenderTexture.GetTemporary(w, h, 0, fmt);
        var b = RenderTexture.GetTemporary(w, h, 0, fmt);
        a.filterMode = FilterMode.Bilinear;
        b.filterMode = FilterMode.Bilinear;

        // 1) Loc lay phan sang
        mat.SetFloat("_Threshold", threshold);
        mat.SetFloat("_SoftKnee", softKnee);
        Graphics.Blit(src, a, mat, 0);

        // 2) Lam nhoe nhieu vong, moi vong nhoe rong hon
        for (int i = 0; i < iterations; i++)
        {
            float spread = 1f + i * 1.35f;
            mat.SetVector("_BlurDir", new Vector4(spread, 0f, 0f, 0f));
            Graphics.Blit(a, b, mat, 1);
            mat.SetVector("_BlurDir", new Vector4(0f, spread, 0f, 0f));
            Graphics.Blit(b, a, mat, 1);
        }

        // 3) Cong tro lai anh goc
        mat.SetTexture("_BloomTex", a);
        mat.SetFloat("_Intensity", intensity);
        mat.SetFloat("_Vignette", vignette);
        Graphics.Blit(src, dst, mat, 2);

        RenderTexture.ReleaseTemporary(a);
        RenderTexture.ReleaseTemporary(b);
    }
}
