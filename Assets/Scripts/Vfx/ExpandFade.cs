using UnityEngine;

/// <summary>Phong to dan roi mo di - dung cho song xung kich, qua cau no.</summary>
public class ExpandFade : MonoBehaviour
{
    public float duration = 0.6f;
    public Vector3 startScale = Vector3.one * 0.2f;
    public Vector3 endScale = Vector3.one * 3f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public string alphaProperty = "";     // vd "_Alpha" cho shader lua
    public bool fadeColorAlpha = true;

    float t;
    Material mat;
    Color startColor;

    void Start()
    {
        var r = GetComponent<Renderer>();
        if (r != null)
        {
            mat = r.material;
            if (mat.HasProperty("_TintColor")) startColor = mat.GetColor("_TintColor");
            else if (mat.HasProperty("_Color")) startColor = mat.color;
            else fadeColorAlpha = false;
        }
        transform.localScale = startScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);
        float e = ease.Evaluate(k);

        transform.localScale = Vector3.LerpUnclamped(startScale, endScale, e);

        float fade = 1f - k;
        if (mat != null)
        {
            if (!string.IsNullOrEmpty(alphaProperty) && mat.HasProperty(alphaProperty))
                mat.SetFloat(alphaProperty, fade);
            else if (fadeColorAlpha)
            {
                var c = startColor;
                c.a = startColor.a * fade;
                if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", c);
                else if (mat.HasProperty("_Color")) mat.color = c;
            }
        }

        if (k >= 1f) Destroy(gameObject);
    }
}
