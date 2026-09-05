using UnityEngine;

/// <summary>Bat he hat chay o khung hinh dau tien (sau khi da chinh xong thong so).</summary>
public class PlayOnStart : MonoBehaviour
{
    void Start()
    {
        var ps = GetComponent<ParticleSystem>();
        if (ps != null) ps.Play(true);
    }
}
