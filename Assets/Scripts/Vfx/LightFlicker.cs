using UnityEngine;

/// <summary>Den nhap nhay nhu lua that (do sang len xuong ngau nhien).</summary>
public class LightFlicker : MonoBehaviour
{
    public float baseIntensity = 2f;
    public float amount = 0.4f;
    public float speed = 6f;
    public float rangeWobble = 0f;

    Light lt;
    float seed, baseRange;

    void Awake()
    {
        lt = GetComponent<Light>();
        seed = Random.value * 100f;
        if (lt != null) baseRange = lt.range;
    }

    void Update()
    {
        if (lt == null) return;
        float n = Mathf.PerlinNoise(seed, Time.time * speed) * 2f - 1f;
        lt.intensity = Mathf.Max(0f, baseIntensity * (1f + n * amount));
        if (rangeWobble > 0f) lt.range = baseRange * (1f + n * rangeWobble);
    }
}
