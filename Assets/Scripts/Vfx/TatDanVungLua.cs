using UnityEngine;

/// <summary>
/// Tat dan vung dat dang chay.
///
/// NGUNG PHUN TRUOC, roi de dam lua con lai tu chay het doi cua no. Goi
/// Destroy ngay khi het gio thi ca vung lua bien mat mot cai, nhin nhu bi loi
/// chu khong ra lua tan.
///
/// Ngon den cung phai lu dan theo, khong thi dat da toi thui ma van con mot
/// quang sang cam do lo lung giua khong khi.
/// </summary>
public class TatDanVungLua : MonoBehaviour
{
    [Tooltip("Phun lua trong bao lau, giay")]
    public float thoiGian = 4f;

    [Tooltip("Sau khi ngung phun thi cho them bao lau cho lua tan het")]
    public float thoiGianTan = 2.6f;

    public Light den;
    public float denSangGoc = 2.8f;

    float age;
    bool daNgungPhun;

    void Update()
    {
        age += Time.deltaTime;

        if (!daNgungPhun && age >= thoiGian)
        {
            daNgungPhun = true;
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
            {
                var em = ps.emission;
                em.rateOverTime = 0f;
            }
        }

        // Den lu dan trong ca doan chay, tat han luc lua tan het
        if (den != null)
        {
            float k = 1f - Mathf.Clamp01((age - thoiGian * 0.55f)
                                       / Mathf.Max(0.01f, thoiGian * 0.45f + thoiGianTan));
            den.intensity = denSangGoc * k * k;
        }

        if (age >= thoiGian + thoiGianTan) Destroy(gameObject);
    }
}
