using UnityEngine;

/// <summary>Anh sang loe roi tat (vu no lua, no bang).</summary>
public class LightBurst : MonoBehaviour
{
    public float peak = 12f;
    public float duration = 0.5f;
    public float range = 10f;
    public AnimationCurve shape = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.08f, 1f), new Keyframe(1f, 0f));

    Light lt;
    float t;

    void Start()
    {
        lt = GetComponent<Light>();
        if (lt == null) lt = gameObject.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.range = range;
        lt.shadows = LightShadows.None;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);
        if (lt != null) lt.intensity = shape.Evaluate(k) * peak;
        if (k >= 1f) Destroy(gameObject);
    }
}
