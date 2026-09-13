using UnityEngine;

/// <summary>
/// TRANG THAI BI DANH NGA (thien thach roi trung).
///
/// Nguoi dung xin (13/09/2026): qua thien thach roi xuong co 40% danh nga ke
/// dich 1,5 giay - bi HAT NHE LEN KHOI MAT DAT roi rot xuong NAM NGUA, khong di
/// duoc va khong dung duoc ky nang. Ap cho ca quai lan nguoi choi.
///
/// LAT HINH, KHONG LAT NHAN VAT. Moi nhan vat co CharacterController o goc va
/// model (co component Animation) o con dau tien. Xoay goc thi con nhong va
/// cham xoay theo va ma AI / dieu khien cung ghi de huong moi khung hinh; xoay
/// con model thi va cham van dung yen o dat, chi hinh la nam xuong. Hoat hinh
/// (ModelHoatHinh, NguoiChoiHoatHinh) chi ghi cac KHOP XUONG ben trong nen
/// khong dung toi transform nay.
///
/// Dong thoi gian trong <see cref="thoiGian"/> giay (tinh tu luc trung):
///   0 .. 0,38 s  : bi hat len (duong parabol cao 0,9 m) va lat ngua dan
///   .. -0,28 s   : nam ngua tren dat
///   0,28 s cuoi  : chong dung day
/// Khoa di chuyen va ky nang trong SUOT khoang ay.
///
/// Chay SAU moi script khac (DefaultExecutionOrder) de khong ai ghi de hinh.
/// </summary>
[DefaultExecutionOrder(10000)]
public class BiDanhNga : MonoBehaviour
{
    /// <summary>Tong thoi gian bi khoa, tinh tu luc trung.</summary>
    public float thoiGian = 1.5f;

    /// <summary>Da troi bao lau tu luc trung.</summary>
    public float daTroi;

    public const float TgBay = 0.38f;
    public const float CaoBay = 0.9f;
    public const float TgDay = 0.28f;
    /// <summary>Nhac hinh len mot chut khi nam de lung khong chim xuong dat.</summary>
    const float NangKhiNam = 0.22f;

    public bool DangNga { get { return daTroi < thoiGian; } }

    Transform hinh;
    Vector3 posGoc;
    Quaternion rotGoc;
    Damageable target;

    /// <summary>
    /// Danh nga mot muc tieu. Dang nga roi thi keo dai them (khong bay lai lan
    /// nua - dang nam ma nay len giua khong trung thi nhin ra loi).
    /// </summary>
    public static void Apply(Damageable d, float giay)
    {
        if (d == null || d.IsDead || giay <= 0f) return;
        // Ban sao khong tu gieo hieu ung cho minh - trang thai that den tu goi
        // tin cua may chu so huu (HieuUngQuaMang.ApCo). Cung quy uoc voi choang.
        if (d.mauDoMayKhacQuyet) return;
        GanHoacKeoDai(d, giay);
    }

    /// <summary>Gan / keo dai KHONG qua cho gac ban sao - danh cho HieuUngQuaMang.</summary>
    public static BiDanhNga GanHoacKeoDai(Damageable d, float giay)
    {
        if (d == null || d.IsDead) return null;
        var n = d.GetComponent<BiDanhNga>();
        if (n == null)
        {
            n = d.gameObject.AddComponent<BiDanhNga>();
            n.thoiGian = giay;
            n.daTroi = 0f;
            DamagePopup.SpawnText(d.transform.position + Vector3.up * 2.1f, "NGÃ!",
                                  new Color(1f, 0.72f, 0.40f));
            if (d.anim != null) d.anim.PlayHit();
        }
        else
        {
            n.thoiGian = Mathf.Max(n.thoiGian, n.daTroi + giay);
        }
        return n;
    }

    void Awake()
    {
        target = GetComponent<Damageable>();

        // Model = con dau tien co Animation; khong co thi con dau tien co hinh
        foreach (Transform c in transform)
            if (c.GetComponent<Animation>() != null) { hinh = c; break; }
        if (hinh == null)
            foreach (Transform c in transform)
                if (c.GetComponentInChildren<Renderer>() != null) { hinh = c; break; }

        if (hinh != null) { posGoc = hinh.localPosition; rotGoc = hinh.localRotation; }
    }

    void LateUpdate()
    {
        daTroi += Time.deltaTime;

        if (hinh != null)
        {
            float goc, cao;
            if (daTroi < TgBay)
            {
                float u = daTroi / TgBay;
                cao = CaoBay * 4f * u * (1f - u) + NangKhiNam * u;      // nay len roi roi xuong
                goc = -90f * Mathf.SmoothStep(0f, 1f, u);               // lat ngua dan
            }
            else if (daTroi < thoiGian - TgDay)
            {
                cao = NangKhiNam;
                goc = -90f;
            }
            else
            {
                float u = Mathf.Clamp01((daTroi - (thoiGian - TgDay)) / TgDay);
                cao = NangKhiNam * (1f - u);
                goc = -90f * (1f - Mathf.SmoothStep(0f, 1f, u));        // chong dung day
            }

            // -90 quanh truc X CUA GOC NHAN VAT (nhan ben TRAI rotGoc): dinh dau
            // nga ve SAU lung, mat ngua len troi. Nhan ben phai la xoay theo
            // truc rieng cua model - model Meshy nhap vao thuong da xoay san
            // 180 do, luc ay nhan vat lai nga UP mat xuong.
            hinh.localRotation = Quaternion.Euler(goc, 0f, 0f) * rotGoc;
            hinh.localPosition = posGoc + Vector3.up * cao;
        }

        if (!DangNga) Destroy(this);
    }

    void OnDestroy()
    {
        // Chet trong luc dang nam thi GIU hinh nam - xac nga ngua la dung, bat
        // dung day roi moi nga kieu chet thi nhin ra loi.
        if (target != null && target.IsDead) return;
        if (hinh != null) { hinh.localPosition = posGoc; hinh.localRotation = rotGoc; }
    }
}
