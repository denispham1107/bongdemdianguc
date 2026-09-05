using UnityEngine;

/// <summary>Luon quay mat ve phia camera (dung cho so sat thuong).</summary>
public class Billboard : MonoBehaviour
{
    public bool flipForward = true;

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 dir = transform.position - cam.transform.position;
        if (flipForward) transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        else transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
    }
}
