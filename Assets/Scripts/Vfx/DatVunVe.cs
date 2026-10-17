using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// VE DAT VUN NGON NGANG BANG GPU INSTANCING (17/10/2026, menu 141 MatDatSanSui rai: cuc dat · da vun · mang dat nut vo).
///
/// Vi sao khong gop thanh luoi: lan dau gop theo o 16 m ra 572 nghin DINH (manh mat cat phang -> moi tam giac dinh rieng) = hon 20 MB bo nho
/// tren dien thoai. Nay chi giu 13 luoi mau (Blender MCP CongCu/Blender/dat_vun.py) + moi manh 40 byte (vi tri, xoay, ti le); moi khung chi ve
/// cac O nam trong khung may quay (Camera.main), gom toi 1023 manh mot lan goi.
///
/// Vat lieu dung shader <c>Diablo25D/ChuanSuong</c> (Standard + suong chien tranh): Act2 ngoai tam nhin toi mo nhu canh vat khac; man chinh
/// _TN_Bat = 0 -> nhu Standard. Khong do bong, nhan bong; khong va cham (cuc dat 0,1-0,35 m - di qua duoc).
/// </summary>
[ExecuteAlways]
public class DatVunVe : MonoBehaviour
{
    public Mesh[] luoi;
    public Material[] vatLieu;           // theo tung luoi
    // moi manh
    public int[] loai;
    public Vector3[] viTri;
    public Quaternion[] xoay;
    public Vector3[] tiLe;
    // o (manh da sap theo o): manh [batDau[o], batDau[o+1])
    public int[] batDau;
    public Bounds[] khung;

    Matrix4x4[][] dem;
    int[] soDem;
    Plane[] mp = new Plane[6];

    /// <summary>Phep thu: so manh / so lan goi ve cua khung truoc.</summary>
    public static int SoManhDaVe, SoLanGoi;

    void OnEnable()
    {
        if (luoi == null) return;
        dem = new Matrix4x4[luoi.Length][];
        soDem = new int[luoi.Length];
        for (int i = 0; i < luoi.Length; i++) dem[i] = new Matrix4x4[1023];
    }

    void Update()
    {
        if (luoi == null || viTri == null || batDau == null || dem == null) return;
        var cam = Camera.main;
        bool loc = Application.isPlaying && cam != null;
        if (loc) GeometryUtility.CalculateFrustumPlanes(cam, mp);
        int daVe = 0, goi = 0;
        for (int i = 0; i < soDem.Length; i++) soDem[i] = 0;
        for (int o = 0; o < khung.Length; o++)
        {
            if (loc && !GeometryUtility.TestPlanesAABB(mp, khung[o])) continue;
            for (int k = batDau[o]; k < batDau[o + 1]; k++)
            {
                int l = loai[k];
                dem[l][soDem[l]++] = Matrix4x4.TRS(viTri[k], xoay[k], tiLe[k]);
                daVe++;
                if (soDem[l] == 1023) { Ve(l); goi++; }
            }
        }
        for (int l = 0; l < soDem.Length; l++) if (soDem[l] > 0) { Ve(l); goi++; }
        SoManhDaVe = daVe; SoLanGoi = goi;
    }

    void Ve(int l)
    {
        if (luoi[l] != null && vatLieu[l] != null)
            Graphics.DrawMeshInstanced(luoi[l], 0, vatLieu[l], dem[l], soDem[l], null, ShadowCastingMode.Off, true, gameObject.layer, null, LightProbeUsage.Off);
        soDem[l] = 0;
    }
}
