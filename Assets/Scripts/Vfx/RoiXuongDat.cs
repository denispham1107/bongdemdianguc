using UnityEngine;

/// <summary>
/// CHO MOT CAI XAC DANG LO LUNG ROI XUONG DAT.
///
/// Quai bi loc cuon len cao roi chet giua chung: <see cref="Damageable.Die"/> da
/// tat CharacterController va Collider cua no, nen khong con gi keo no xuong.
/// No treo nguyen tren khong cho toi luc bi xoa - nhin thay ngay la sai.
///
/// Vat the nay tu roi lay: khong dung vat ly (Rigidbody) vi cai xac khong con
/// collider nao de va cham, va them Rigidbody vao mot vat vua chet thi no lai
/// lot qua dia hinh. Chi la roi tu do rat don gian, dung lai khi cham cao do
/// mat dat.
///
/// Tu HUY chinh no khi da nam xuong, nen khong ton gi sau do.
/// </summary>
public class RoiXuongDat : MonoBehaviour
{
    [Tooltip("Toc do roi ban dau, m/s. Duong = dang bay len.")]
    public float vanToc = 0f;

    [Tooltip("Gia toc trong truong, m/s^2")]
    public float trongTruong = 22f;

    [Tooltip("Roi nhanh nhat bao nhieu - de xac khong cam xuyen qua dat")]
    public float tocDoToiDa = 26f;

    [Tooltip("Nga sap xuong trong luc roi")]
    public bool nganNguoi = true;

    float caoDat;
    bool daDo;

    /// <summary>Gan vao mot vat the va cho no roi.</summary>
    public static RoiXuongDat Gan(GameObject go, float vanTocDau)
    {
        if (go == null) return null;

        var r = go.GetComponent<RoiXuongDat>();
        if (r == null) r = go.AddComponent<RoiXuongDat>();
        r.vanToc = vanTocDau;
        r.daDo = false;
        return r;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Do cao mat dat do MOT LAN roi nho lai.
        //
        // Ban tia moi khung thi vua ton vua co the trung ngay cai xac dang roi
        // ben canh; ma cai xac thi roi thang dung, dat duoi chan no khong doi.
        if (!daDo)
        {
            caoDat = VfxFactory.GroundY(transform.position);
            daDo = true;
        }

        vanToc -= trongTruong * dt;
        if (vanToc < -tocDoToiDa) vanToc = -tocDoToiDa;

        Vector3 p = transform.position;
        p.y += vanToc * dt;

        if (p.y <= caoDat)
        {
            p.y = caoDat;
            transform.position = p;

            // Nam sap xuong dat cho ra dang mot cai xac, khong dung thang nhu
            // vua ha canh.
            if (nganNguoi)
            {
                var e = transform.eulerAngles;
                transform.rotation = Quaternion.Euler(78f, e.y, 0f);
            }

            Destroy(this);
            return;
        }

        transform.position = p;

        // Vua roi vua lon nguoi mot chut
        if (nganNguoi)
            transform.Rotate(Vector3.right, 150f * dt, Space.Self);
    }
}
