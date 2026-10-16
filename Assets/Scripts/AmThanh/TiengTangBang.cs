using UnityEngine;

/// <summary>
/// TIENG KHOI BANG VO KHI HET GIO (16/10/2026, nguoi dung: "khi cac khoi bang tren mat dat vo, them am thanh bang vo"; chon: het gio MOI cap,
/// ca khoi bang cua Mua bang). Gan vao goc tang bang luc VfxFactory.IceImpact dung (chi khi co cum gai - coGai).
///
/// Hinh khoi bang: moi cum gai (CumGai*) co ExpandFade rieng 1,6-2,6 s, MO DAN deu tu luc moc toi luc mat. Tieng vo keu luc cum DAU TIEN
/// bien mat (<see cref="Moc"/>) - cac cum con lai tan dan theo duoi tieng vo (ken ket -> nut -> vun -> tang roi, ~1,2 s).
/// Khoi CAP 5 (co TangBangNo) KHONG keu o day - TangBangNo.No phat tieng no tung rieng (<see cref="TiengViTri.BangVoNo"/>).
/// </summary>
public class TiengTangBang : MonoBehaviour
{
    /// <summary>Giay tu luc moc toi luc phat tieng vo (phep thu doc).</summary>
    public float Moc { get; private set; } = -1f;
    float tuoi;

    /// <summary>So lan khoi bang da keu vo / bi bo qua vi cap 5 (phep thu doc).</summary>
    public static int SoLanVo, SoLanBoQuaCap5;

    public static void Gan(GameObject tang)
    {
        if (tang == null) return;
        tang.AddComponent<TiengTangBang>();
    }

    void Start()
    {
        // CAP 5: TangBangNo.Gan (goi ngay sau IceImpact, truoc Start nay) keo moi cum gai toi CUNG mot moc va xoa ca tang luc no -
        // tieng do TangBangNo.No phat; o day nhan ra ngay, khong cho (cho thi tang bi xoa truoc, khong dem duoc)
        if (GetComponent<TangBangNo>() != null) { SoLanBoQuaCap5++; Destroy(this); return; }
        float moc = float.MaxValue;
        foreach (var ef in GetComponentsInChildren<ExpandFade>(false))
            if (ef.name.StartsWith("CumGai") || ef.name.StartsWith("Spike")) moc = Mathf.Min(moc, ef.duration);
        if (moc == float.MaxValue) { Destroy(this); return; }   // khong co cum gai dang hien -> khong co khoi bang de vo
        Moc = moc;
    }

    void Update()
    {
        if (Moc < 0f) return;
        tuoi += Time.deltaTime;
        if (tuoi < Moc) return;
        TiengViTri.BangVo(transform.position);
        SoLanVo++;
        Destroy(this);
    }
}
