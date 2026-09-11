using UnityEngine;

/// <summary>
/// Gan len lo da: dung lua luc chay. Lam luc chay chu khong luu vao prefab vi
/// vat lieu hat tao bang code (Mats.FlipbookAdd) khong luu thanh asset duoc.
/// </summary>
public class LoLuaDa : MonoBehaviour
{
    [Tooltip("Mieng chau cao bao nhieu so voi goc lo (m)")]
    public float doCaoMieng = 1.20f;
    [Tooltip("Ban kinh long chau (m)")]
    public float banKinhChau = 0.42f;

    void Start()
    {
        VfxFactory.LuaLoDa(transform, new Vector3(0f, doCaoMieng, 0f), banKinhChau);
    }
}
