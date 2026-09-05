using UnityEngine;

/// <summary>
/// SO SAT THUONG bay len roi mo dan - kieu Diablo.
/// Dung TextMesh (chu 3D) nen khong can package UI nao.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    public float lifetime = 0.9f;
    public float riseSpeed = 1.6f;
    public Vector3 drift;

    TextMesh text;
    float t;

    static Font font;
    static Material fontMat;

    static void EnsureFont()
    {
        if (font != null) return;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Segoe UI", 32);
        if (font != null) fontMat = font.material;
    }

    public static DamagePopup Spawn(Vector3 pos, int amount, DamageType type, bool onPlayer)
    {
        Color c = onPlayer ? new Color(1f, 0.25f, 0.2f)
                : type == DamageType.Fire ? new Color(1f, 0.62f, 0.15f)
                : type == DamageType.Ice ? new Color(0.6f, 0.88f, 1f)
                : type == DamageType.Lightning ? new Color(0.85f, 0.94f, 1f)
                : new Color(0.95f, 0.92f, 0.85f);

        return SpawnText(pos, amount.ToString(), c);
    }

    /// <summary>Mot dong chu bay len (vd "CHOANG!") - dung chung voi so sat thuong.</summary>
    public static DamagePopup SpawnText(Vector3 pos, string label, Color color)
    {
        EnsureFont();
        if (font == null) return null;

        var go = new GameObject("Dmg");
        go.transform.position = pos + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0f, 0.3f), Random.Range(-0.25f, 0.25f));

        var tm = go.AddComponent<TextMesh>();
        tm.text = label;
        tm.font = font;
        tm.fontSize = 64;
        tm.characterSize = 0.055f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = color;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null && fontMat != null)
        {
            mr.sharedMaterial = fontMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        go.AddComponent<Billboard>();
        var p = go.AddComponent<DamagePopup>();
        p.text = tm;
        p.drift = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));
        return p;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / lifetime);

        transform.position += (Vector3.up * riseSpeed * (1f - k * 0.6f) + drift) * Time.deltaTime;
        transform.localScale = Vector3.one * (1f + Mathf.Sin(k * Mathf.PI) * 0.25f);

        if (text != null)
        {
            var c = text.color;
            c.a = 1f - Mathf.Pow(k, 2.2f);
            text.color = c;
        }

        if (k >= 1f) Destroy(gameObject);
    }
}
