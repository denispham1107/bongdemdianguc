using UnityEngine;

/// <summary>
/// Gan len lo da: dung lua luc chay. Lam luc chay chu khong luu vao prefab vi
/// vat lieu hat tao bang code (Mats.FlipbookAdd) khong luu thanh asset duoc.
///
/// LOC XOAY di qua thi <see cref="DapTat"/> - lua tat, chi con mot cuon khoi
/// bay len - roi con loc moi boc CAI LO len (xem <see cref="VatTheBiCuon"/>).
/// Lo moc lai sau 30 giay thi <see cref="Chay"/> nhom lua lai.
/// </summary>
public class LoLuaDa : MonoBehaviour
{
    [Tooltip("Mieng chau cao bao nhieu so voi goc lo (m)")]
    public float doCaoMieng = 1.20f;
    [Tooltip("Ban kinh long chau (m)")]
    public float banKinhChau = 0.42f;

    /// <summary>Cum hieu ung lua dang chay - null la lua da tat.</summary>
    GameObject lua;
    float tatLuc = -999f;

    // Dong than trong chau: vat lieu M_Coals TU PHAT SANG (emission 2,5). Tat
    // lua ma khong dung toi no thi cai lo bay theo con loc voi mot dong than
    // do ruc trong long - nhin nhu lua chua he tat.
    Renderer[] than;
    MaterialPropertyBlock khoiChu;

    public bool DangChay { get { return lua != null; } }

    /// <summary>Bao nhieu giay tinh tu luc lua tat.</summary>
    public float GiayTuLucTat { get { return Time.time - tatLuc; } }

    void Start()
    {
        var ds = new System.Collections.Generic.List<Renderer>();
        foreach (var r in GetComponentsInChildren<Renderer>())
            if (r.name.Contains("Than")) ds.Add(r);
        than = ds.ToArray();
        Chay();
    }

    /// <summary>Nhom lua trong chau. Goi lai khi da chay roi thi khong lam gi.</summary>
    public void Chay()
    {
        if (lua != null) return;
        lua = VfxFactory.LuaLoDa(transform, new Vector3(0f, doCaoMieng, 0f), banKinhChau);
        DatMauThan(false);
    }

    /// <summary>
    /// Dap tat lua: xoa han cum hieu ung (lua, khoi, tan lua, den), de lai mot
    /// cuon khoi xam bay len o cho mieng chau.
    ///
    /// Cuon khoi KHONG lam con cua lo: lat nua con loc boc lo bay di, ma khoi
    /// thi phai o lai tren mat dat. No cung la ly do lo sau khi tat khong con
    /// he hat con nao - dieu kien de <see cref="VatTheBiCuon"/> chiu cuon no.
    /// </summary>
    public void DapTat()
    {
        if (lua == null) return;
        Destroy(lua);
        lua = null;
        tatLuc = Time.time;
        DatMauThan(true);
        VfxFactory.KhoiTatLua(transform.position + Vector3.up * doCaoMieng, banKinhChau);
    }

    /// <summary>Than nguoi (xam den, khong phat sang) hay than do dang chay.</summary>
    void DatMauThan(bool nguoi)
    {
        if (than == null) return;
        if (khoiChu == null) khoiChu = new MaterialPropertyBlock();
        khoiChu.Clear();
        if (nguoi)
        {
            khoiChu.SetColor("_Color", new Color(0.10f, 0.09f, 0.085f, 1f));
            khoiChu.SetColor("_EmissionColor", new Color(0.02f, 0.015f, 0.012f, 1f));
        }
        for (int i = 0; i < than.Length; i++)
            if (than[i] != null) than[i].SetPropertyBlock(khoiChu);
    }
}
