using UnityEngine;

/// <summary>Tu xoa minh sau mot khoang thoi gian.</summary>
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 2f;

    void Start() { Destroy(gameObject, lifetime); }

    public static AutoDestroy Add(GameObject go, float life)
    {
        var a = go.AddComponent<AutoDestroy>();
        a.lifetime = life;
        return a;
    }
}
