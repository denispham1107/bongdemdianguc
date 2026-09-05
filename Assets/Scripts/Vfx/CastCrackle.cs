using UnityEngine;

/// <summary>
/// Vai tia dien nho lach tach quanh dau gay trong luc phu thuy dang niem chu Sam set.
/// </summary>
public class CastCrackle : MonoBehaviour
{
    public float radius = 0.55f;
    public float minGap = 0.06f;
    public float maxGap = 0.16f;

    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = Random.Range(minGap, maxGap);

        Vector3 a = transform.position + Random.onUnitSphere * radius;
        Vector3 b = transform.position + Random.onUnitSphere * radius;

        var arc = LightningArc.Create(a, b, 0.3f, Random.Range(0.06f, 0.13f));
        arc.transform.SetParent(transform, true);
        arc.segments = 6;
        arc.jitter = 2.4f;
        arc.branches = 0;
    }
}
