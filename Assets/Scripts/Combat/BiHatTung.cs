using UnityEngine;

/// <summary>
/// TRANG THAI BI HAT TUNG (ky nang Gio loc).
///
/// Nguoi dung xin (16/09/2026): loc nho trung thi 55% HAT TUNG doi thu len khoi mat dat trong 0,5 giay -
/// KHONG cuon len troi nhu Loc xoay. Nguoi choi / quai dang tung chieu ma bi hat tung thi BI NGAT CHIEU
/// NGAY (nguoi dung chon: ca nguoi choi lan quai). Nguoi dung chon do cao 1,5 m.
///
/// Cung cach voi <see cref="BiDanhNga"/>: nhac MODEL CON (con dau tien co Animation / co hinh) theo mot
/// duong parabol, va cham o goc van dung yen tren dat. Khoa di chuyen va ky nang suot khoang ay
/// (PlayerController.DangBiKhoaCung, EnemyAI).
///
/// Dang bay ma trung them mot lan hat nua (loc thu hai) thi TINH LAI TU DAU 0,5 giay (da noi voi nguoi
/// dung khi hoi), bay tiep tu DO CAO HIEN TAI chu khong giat ve dat.
///
/// Chay SAU BiDanhNga (10000) - neu vua nga vua bi hat thi cong do cao len tren hinh nga.
///
/// 04/10/2026 nguoi dung: "bi hat tung cao hon nua" - chon 3 m, bay 0,7 giay (truoc 1,5 m / 0,5 s), "dong thoi doi thu bi hat tung
/// se co tu the bi NGA NGUA ra sau tren khong trung thay vi tu the dung": hinh lat ngua toi GocNgua 75 do (cung chieu lat cua
/// BiDanhNga: quanh truc X cua GOC nhan vat - dinh dau ve sau lung) quanh diem HONG (TamXoayNgua 0,9 m tren chan) trong 30% dau
/// duong bay, giu ngua, 28% cuoi dung lai de cham dat bang chan. Dang bi danh nga thi BiDanhNga giu goc xoay, cai nay chi cong do cao.
/// </summary>
[DefaultExecutionOrder(10001)]
public class BiHatTung : MonoBehaviour
{
    public const float GiayMacDinh = 0.7f;
    public const float CaoBay = 3f;

    /// <summary>Goc nga ngua toi da tren khong (do) va do cao diem xoay (hong) tren chan model (m).</summary>
    public const float GocNgua = 75f, TamXoayNgua = 0.9f;

    /// <summary>Tong thoi gian bay (tinh tu lan hat gan nhat).</summary>
    public float thoiGian = GiayMacDinh;

    /// <summary>Da troi bao lau tu lan hat gan nhat.</summary>
    public float daTroi;

    /// <summary>Do cao luc bat dau lan hat nay (khac 0 khi bi hat tiep giua khong trung).</summary>
    float caoLucDau;

    /// <summary>Muc nga ngua (0-1) luc bat dau lan hat nay - hat tiep giua khong trung thi khong dung bat day roi nga lai.</summary>
    float nguaLucDau;

    /// <summary>Muc nga ngua (0-1) o khung vua ve.</summary>
    public float NguaHienTai { get; private set; }

    /// <summary>Do cao hinh dang o khung hinh vua ve.</summary>
    public float CaoHienTai { get; private set; }

    /// <summary>Do cao lon nhat da dat - phep thu (menu 71) doc.</summary>
    public float CaoLonNhat { get; private set; }

    public bool DangBay { get { return daTroi < thoiGian; } }

    /// <summary>Dem cho phep thu: so lan hat tung that (tren may chu so huu).</summary>
    public static int SoLanHat;

    /// <summary>Luc ket thuc cu hat gan nhat cua tung muc tieu - chong ban sao hat lai lan hai vi goi "dang bay"
    /// cuoi cung den tre sau khi hinh da roi xuong.</summary>
    static readonly System.Collections.Generic.Dictionary<Damageable, float> ketThucLuc =
        new System.Collections.Generic.Dictionary<Damageable, float>();

    Transform hinh;
    Vector3 posGoc;
    Quaternion rotGoc;
    bool coPosGoc;
    /// <summary>Vi tri / goc xoay dung goc cua model - XacNam doc khi chet giua luc bay.</summary>
    public Vector3 PosGoc { get { return posGoc; } }
    public Quaternion RotGoc { get { return rotGoc; } }
    public bool CoPosGoc { get { return coPosGoc; } }

    /// <summary>
    /// Hat tung mot muc tieu. Ban sao mang tu bo qua - may chu so huu gieo va bao qua bit CoHatTung
    /// (cung quy uoc voi choang / nga).
    /// </summary>
    public static BiHatTung Apply(Damageable d, float giay)
    {
        if (TangHinh.ChanHieuUng(d)) return null;   // tang hinh mien moi hieu ung (18/09/2026)
        if (d == null || d.IsDead || giay <= 0f) return null;
        if (d.mauDoMayKhacQuyet) return null;
        if (ChongDo.ChanHieuUng(d)) return null;  // Bo xuong do duoc don nay (28/09/2026)
        // Dang bi Loc xoay cuon tren troi: WhirledEffect da dat vi tri, hat them la hai co che giat nhau
        if (d.GetComponent<WhirledEffect>() != null) return null;
        SoLanHat++;
        return HatLai(d, giay);
    }

    /// <summary>Gan / hat lai KHONG qua cho gac ban sao.</summary>
    public static BiHatTung HatLai(Damageable d, float giay)
    {
        if (d == null || d.IsDead) return null;
        var h = d.GetComponent<BiHatTung>();
        if (h == null)
        {
            h = d.gameObject.AddComponent<BiHatTung>();
            h.caoLucDau = 0f;
            DamagePopup.SpawnText(d.transform.position + Vector3.up * 2.1f, "HẤT TUNG!",
                                  new Color(0.86f, 0.66f, 0.40f));
            if (d.anim != null) d.anim.PlayHit();
        }
        else
        {
            h.caoLucDau = h.CaoHienTai;
            h.nguaLucDau = h.NguaHienTai;
        }
        h.thoiGian = giay;
        h.daTroi = 0f;
        NgatChieu(d);
        return h;
    }

    /// <summary>
    /// Ban sao mang: goi tin cua may chu so huu bao "dang bi hat tung". Chua co thi bat dau mot cu hat
    /// tron 0,5 giay (hinh bay len roi xuong); dang co thi chi keo dai rat it cho khop luc goi tin ngung.
    /// Van NGAT CHIEU ban sao dang niem - may chu da ngat, ban sao phai ngat theo khong thi van phong phep ra.
    /// </summary>
    public static void ApTuMang(Damageable d)
    {
        if (d == null || d.IsDead) return;
        var h = d.GetComponent<BiHatTung>();
        float kt;
        if (h == null && ketThucLuc.TryGetValue(d, out kt) && Time.time - kt < HieuUngQuaMang.GiuSongGiay) return;
        if (h == null) { HatLai(d, GiayMacDinh); return; }
        h.thoiGian = Mathf.Max(h.thoiGian, h.daTroi + 0.05f);
        NgatChieu(d);
    }

    /// <summary>Ngat chieu dang niem (nguoi choi) / don dang ra (quai).</summary>
    static void NgatChieu(Damageable d)
    {
        var pc = d.GetComponent<PlayerController>();
        if (pc != null) pc.NgatChieu();
        var ai = d.GetComponent<EnemyAI>();
        if (ai != null) ai.NgatDon();
    }

    void Awake()
    {
        foreach (Transform c in transform)
            if (c.GetComponent<Animation>() != null) { hinh = c; break; }
        if (hinh == null)
            foreach (Transform c in transform)
                if (c.GetComponentInChildren<Renderer>() != null) { hinh = c; break; }
        if (hinh != null) { posGoc = hinh.localPosition; rotGoc = hinh.localRotation; coPosGoc = true; }
    }

    void LateUpdate()
    {
        daTroi += Time.deltaTime;
        float u = Mathf.Clamp01(daTroi / Mathf.Max(0.01f, thoiGian));
        // Parabol len CaoBay roi ve dat; bat dau tu do cao dang co (bi hat tiep giua khong trung)
        CaoHienTai = Mathf.Lerp(caoLucDau, 0f, u) + CaoBay * 4f * u * (1f - u);
        if (CaoHienTai > CaoLonNhat) CaoLonNhat = CaoHienTai;
        // Nga ngua: nhanh trong 30% dau, giu, dung lai trong 28% cuoi (cham dat bang chan)
        float vao = Mathf.Max(nguaLucDau, Mathf.SmoothStep(0f, 1f, u / 0.3f));
        NguaHienTai = vao * (1f - Mathf.SmoothStep(0f, 1f, (u - 0.72f) / 0.28f));

        if (hinh != null && coPosGoc)
        {
            // Dang bi danh nga: BiDanhNga (chay truoc) da dat vi tri tuyet doi cua hinh nam - cong them len
            var nga = GetComponent<BiDanhNga>();
            if (nga != null && nga.DangNga) hinh.localPosition += Vector3.up * CaoHienTai;
            else
            {
                // -GocNgua quanh truc X cua GOC (nhan ben trai rotGoc - xem BiDanhNga: model Meshy xoay san 180 do), quanh diem hong
                var xoay = Quaternion.Euler(-GocNgua * NguaHienTai, 0f, 0f);
                Vector3 tam = posGoc + Vector3.up * TamXoayNgua;
                hinh.localRotation = xoay * rotGoc;
                hinh.localPosition = tam + xoay * (posGoc - tam) + Vector3.up * CaoHienTai;
            }
        }

        if (!DangBay) Destroy(this);
    }

    void OnDestroy()
    {
        var dm = GetComponent<Damageable>();
        if (dm != null) ketThucLuc[dm] = Time.time;
        var nga = GetComponent<BiDanhNga>();
        if (nga != null && nga.DangNga) return;
        if (hinh != null && coPosGoc) { hinh.localPosition = posGoc; hinh.localRotation = rotGoc; }
    }
}
