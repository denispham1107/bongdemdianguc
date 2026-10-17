using UnityEngine;

/// <summary>
/// MUC DO HOA "RAT YEU": CHI NHAN VAT CUA NGUOI DANG CHOI CO BONG (17/10/2026, nguoi dung: "Rat yeu: tat bong van giu nguyen, chi cho hien thi
/// bong nhan vat chinh cua nguoi dang choi"; chon BONG CHIEU PHANG).
///
/// Bong cua Unity VAN TAT o muc nay. Ban dau thu bat bong + den chinh chi do bong mot lop rieng cho nhan vat (Light.layerShadowCullDistances):
/// do o Act2 +350 luot ve (+53%), tam giac x2 (den chinh can ve them mot luot do sau ca canh) va van lot 5 lan ve bong cua vat khac -> bo.
/// Nay: them MOT o vat lieu <c>Diablo25D/BongPhang</c> (Resources/DoHoa/BongPhang.mat) vao renderer HINH cua nhan vat - Unity ve lai o vat lieu
/// thua tren luoi con cuoi (cung skinning) -> shader ep det xuong mat phang dat duoi chan theo huong den chinh: bong dung dang, ~1 luot ve.
/// Tang hinh thay CA mang vat lieu -> bong tu mat khi tang hinh (dung y). Lop phu chay / bang / uot them / go DUNG vat lieu cua no -> khong dung.
/// Moi khung (LateUpdate, sau hoat hinh + chan cham dat): mat phang = dat duoi chan (tia lop Ground, du phong Terrain) + 3 cm theo phap tuyen
/// dat; huong tia = den Directional sang nhat dang bat (ngay / chieu / dem cua ChuyenChieuSangDem); do dam theo cuong do x shadowStrength.
/// Goi tu GameBootstrap (Act2, nhan vat minh) va MainMenuUI (phu thuy trung bay). Muc khac: khong lam gi.
/// </summary>
public class BongChiNhanVat : MonoBehaviour
{
    public const string DuongVatLieu = "DoHoa/BongPhang";

    public static bool Bat { get { return CaiDatDoHoa.Muc == MucDoHoa.RatYeu; } }

    static readonly int idDat = Shader.PropertyToID("_BP_Dat"), idPhap = Shader.PropertyToID("_BP_PhapTuyen"),
                        idHuong = Shader.PropertyToID("_BP_HuongDen"), idDam = Shader.PropertyToID("_BP_DoDam");

    /// <summary>Phep thu: so renderer da gan bong, mat phang / huong den khung truoc.</summary>
    public int soRenderer;
    public Vector3 datDaDat, phapDaDat, huongDaDat;

    int matNaDat;
    Light den;
    float henTimDen;

    public static BongChiNhanVat Ap(Transform nhanVat)
    {
        if (!Bat || nhanVat == null) return null;
        var vl = Resources.Load<Material>(DuongVatLieu);
        if (vl == null) { Debug.LogWarning("[BongChiNhanVat] thieu " + DuongVatLieu); return null; }
        var b = nhanVat.GetComponent<BongChiNhanVat>();
        if (b == null) b = nhanVat.gameObject.AddComponent<BongChiNhanVat>();
        b.soRenderer = 0;
        foreach (var r in nhanVat.GetComponentsInChildren<Renderer>(true))
        {
            if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) continue;
            if (r.GetComponent<Collider>() != null) continue;
            var ds = r.sharedMaterials;
            bool co = false; foreach (var m in ds) if (m == vl) co = true;
            if (co) { b.soRenderer++; continue; }
            var moi = new Material[ds.Length + 1];
            ds.CopyTo(moi, 0); moi[ds.Length] = vl;
            r.sharedMaterials = moi;
            b.soRenderer++;
        }
        Debug.Log("[BongChiNhanVat] bong chieu phang tren " + b.soRenderer + " renderer cua " + nhanVat.name);
        return b;
    }

    void Awake() { matNaDat = LayerMask.GetMask("Ground"); }

    Light DenChinh()
    {
        if (den != null && den.isActiveAndEnabled && Time.unscaledTime < henTimDen) return den;
        henTimDen = Time.unscaledTime + 2f;
        den = null; float sang = -1f;
        foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            if (l.type == LightType.Directional && l.enabled && l.intensity > sang) { sang = l.intensity; den = l; }
        return den;
    }

    void LateUpdate()
    {
        Vector3 goc = transform.position, dat = goc, phap = Vector3.up;
        RaycastHit h;
        if (Physics.Raycast(goc + Vector3.up * 1.5f, Vector3.down, out h, 6f, matNaDat, QueryTriggerInteraction.Ignore)) { dat = h.point; phap = h.normal; }
        else if (Terrain.activeTerrain != null) dat.y = Terrain.activeTerrain.SampleHeight(goc) + Terrain.activeTerrain.transform.position.y;
        dat += phap * 0.03f;
        var l = DenChinh();
        Vector3 huong = l != null ? l.transform.forward : new Vector3(0.3f, -1f, 0.2f).normalized;
        float dam = l != null ? Mathf.Clamp01(l.intensity) * (l.shadows != LightShadows.None ? l.shadowStrength : 0.8f) : 0.6f;
        Shader.SetGlobalVector(idDat, dat);
        Shader.SetGlobalVector(idPhap, phap);
        Shader.SetGlobalVector(idHuong, huong);
        Shader.SetGlobalFloat(idDam, dam);
        datDaDat = dat; phapDaDat = phap; huongDaDat = huong;
    }
}
