using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Giu cac vat lieu BAN SAO tao ra luc chay (Renderer.materials) va xoa chung khi vat bi huy - khong thi moi lan nhuom
/// (vd. Lua dia nguc: 4 qua + 4 vu no moi lan tung) de lai vat lieu mo coi trong bo nho, tren WebGL la ro ri dan.
/// </summary>
public class XoaVatLieuRieng : MonoBehaviour
{
    public readonly List<Material> ds = new List<Material>();

    void OnDestroy()
    {
        foreach (var m in ds) if (m != null) Destroy(m);
    }
}
