using UnityEngine;

/// <summary>
/// VE CANH 3D O DO PHAN GIAI THAP - CHU, KHUNG, NUT VAN DU NET.
///
/// Truoc day muc do hoa thap ha devicePixelRatio cua CA khung game (trang web
/// dat truoc khi Unity khoi dong): moi thu - ke ca chu va nut OnGUI - ve o do
/// phan giai thap roi phong to, nguoi dung thay "qua mo va nhoe". Nguoi dung
/// muon: muc thap chi doi canh trong game va nen man menu, con giao dien giu
/// y het muc Cao.
///
/// Cach lam: khung game luon o do phan giai day du. Camera chinh ve vao mot
/// anh dem NHO (heSo x man hinh), bloom cung chay tren anh nho, roi phong len
/// man hinh o cuoi chuoi hau ky. OnGUI ve SAU moi camera, len thang man hinh
/// day du - nen chu va nut sac net nhu muc Cao.
///
/// CAI BAY PHAI TRANH: de camera LUON ve vao anh dem thi pixelWidth cua no la
/// kich thuoc anh dem - moi WorldToScreenPoint / ScreenPointToRay (ten tren
/// dau, so sat thuong, chuot nham) tinh theo man hinh nho va lech het. Nen
/// anh dem chi gan TRONG LUC camera ve (OnPreCull) va go ra ngay khi ve xong
/// (OnRenderImage) - ngoai luc ve, camera nhu chua tung bi dong vao.
///
/// Component nay phai dung SAU moi hieu ung OnRenderImage khac tren camera
/// (SimpleBloom): Unity goi OnRenderImage theo thu tu component, cai cuoi
/// nhan anh da qua bloom. CaiDatDoHoa gan no luc chay nen no luon dung cuoi.
/// </summary>
[RequireComponent(typeof(Camera))]
public class KetXuatThuNho : MonoBehaviour
{
    /// <summary>Ti le moi chieu cua anh dem so voi man hinh (1 = khong thu nho).</summary>
    public float heSo = 1f;

    Camera cam;
    RenderTexture dem;
    bool dangGan;

    // ---- Cho phep thu doc ----
    public int RongDem { get { return dem != null ? dem.width : 0; } }
    public int CaoDem { get { return dem != null ? dem.height : 0; } }
    /// <summary>So khung da phong anh dem len man hinh.</summary>
    public int SoKhungDaPhong { get; private set; }

    void OnEnable() { cam = GetComponent<Camera>(); }

    void OnDisable()
    {
        if (cam != null && dangGan) cam.targetTexture = null;
        dangGan = false;
        if (dem != null) { dem.Release(); Destroy(dem); dem = null; }
    }

    void OnPreCull()
    {
        if (heSo >= 0.999f || cam.targetTexture != null && !dangGan) return;

        int w = Mathf.Max(16, Mathf.RoundToInt(Screen.width * heSo));
        int h = Mathf.Max(16, Mathf.RoundToInt(Screen.height * heSo));
        int aa = Mathf.Max(1, QualitySettings.antiAliasing);
        if (dem == null || dem.width != w || dem.height != h || dem.antiAliasing != aa)
        {
            if (dem != null) { dem.Release(); Destroy(dem); }
            dem = new RenderTexture(w, h, 24, RenderTextureFormat.Default);
            dem.name = "KetXuatThuNho";
            dem.antiAliasing = aa;
            dem.filterMode = FilterMode.Bilinear;   // phong len muot, khong ra o vuong
            dem.Create();
        }
        cam.targetTexture = dem;
        dangGan = true;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!dangGan)
        {
            Graphics.Blit(src, dst);
            return;
        }
        // Go anh dem TRUOC khi phong: tu day camera lai la camera cua man hinh
        cam.targetTexture = null;
        dangGan = false;
        Graphics.Blit(src, (RenderTexture)null);
        SoKhungDaPhong++;
    }
}
