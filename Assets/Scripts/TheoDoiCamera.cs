using UnityEngine;

/// <summary>
/// GAN <see cref="KetXuatThuNho"/> VAO CAMERA CHINH cua moi man - song suot
/// game. Kiem moi khung chu khong chi luc nap scene: Act1 dung camera bang code
/// SAU khi scene nap xong, va CameraRig co the thay camera.
/// </summary>
public class TheoDoiCamera : MonoBehaviour
{
    static TheoDoiCamera ban;
    Camera daGan;

    public static void DamBao()
    {
        if (ban != null) return;
        var go = new GameObject("TheoDoiCamera");
        go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
        DontDestroyOnLoad(go);
        ban = go.AddComponent<TheoDoiCamera>();
    }

    KetXuatThuNho kx;

    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        if (cam != daGan) { daGan = cam; kx = cam.GetComponent<KetXuatThuNho>(); }

        // Doc lai moi khung: camera co the song qua lan nap lai (doi muc) ma
        // he so van phai theo muc moi
        float heSo = CaiDatDoHoa.HeSoPhanGiai;
        if (heSo >= 0.999f)
        {
            // Muc Cao: khong ve qua anh dem - OnRenderImage thua van ton mot lan chep anh
            if (kx != null && kx.enabled) kx.enabled = false;
            return;
        }
        // Gan SAU SimpleBloom (AddComponent luc chay luon dung cuoi) - bloom
        // chay tren anh nho, KetXuatThuNho phong anh da qua bloom len man hinh
        if (kx == null) kx = cam.gameObject.AddComponent<KetXuatThuNho>();
        kx.heSo = heSo;
        if (!kx.enabled) kx.enabled = true;
    }
}
