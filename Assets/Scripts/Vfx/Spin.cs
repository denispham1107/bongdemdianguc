using UnityEngine;

/// <summary>Xoay deu (dung cho vong phep, manh bang bay).</summary>
public class Spin : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float degreesPerSecond = 90f;

    void Update()
    {
        transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
