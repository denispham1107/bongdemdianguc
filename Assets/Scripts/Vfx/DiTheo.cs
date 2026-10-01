using UnityEngine;

/// <summary>
/// Cho mot vat DI THEO vi tri vat khac (giu khoang lech luc gan), KHONG lam con cua no (01/10/2026).
///
/// Loe set cham dat cua Loc xoay / Gio loc phai chay theo con loc (Gio loc bay 9,5 m/s - loe dung yen bi bo lai sau lung), nhung
/// lam con thi: Hoa loc xoay / Gio loc thu nho phong luon ca loe, va loc bi xoa la loe mat giua chung. Vat theo bi xoa thi dung lai.
/// </summary>
public class DiTheo : MonoBehaviour
{
    public Transform theo;
    Vector3 lech;

    public static DiTheo Gan(GameObject go, Transform theo)
    {
        var d = go.AddComponent<DiTheo>();
        d.theo = theo;
        d.lech = go.transform.position - theo.position;
        return d;
    }

    void LateUpdate()
    {
        if (theo != null) transform.position = theo.position + lech;
    }
}
