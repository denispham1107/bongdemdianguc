using UnityEngine;

/// <summary>
/// KY NANG 5 - THIEN THACH.
///
/// Mot khoi da rực lua lao tu tren troi xuong diem nguoi choi ngam. Cham dat
/// hoac cham ke dich la NO TUNG: sat thuong mot vung rong, roi de lai mot
/// <see cref="VungLua"/> chay am i trong 4 giay dot moi ke dich dung trong do.
///
/// SINH RA TREN CAO CHU KHONG TU TAY PHU THUY. Phep nay khong phai nem di -
/// no la goi da tu troi xuong, nen diem xuat phat o thang tren dau muc tieu,
/// cach <see cref="doCaoRoi"/> met.
///
/// ROI HOI XIEN mot chut (<see cref="doNghieng"/>) cho co huong lao, khong thi
/// no rot thang dung nhu tha thang may.
///
/// VA CHAM: khong dung SphereCast lien tuc nhu qua cau lua, vi thien thach to
/// va roi rat nhanh - moi khung hinh di ca met. Dung SphereCast theo dung doan
/// vua di duoc trong khung hinh do, khong thi no xuyen qua mat dat.
/// </summary>
public class ThienThach : MonoBehaviour
{
    [Header("Duong roi")]
    [Tooltip("Sinh ra cao hon diem ngam bao nhieu met")]
    public float doCaoRoi = 26f;

    [Tooltip("Lech ngang bao nhieu met de duong roi hoi xien")]
    public float doNghieng = 7f;

    public float speed = 34f;

    [Header("Khoi da")]
    public float bodyRadius = 0.95f;

    [Header("Vu no")]
    public float impactDamage = 85f;
    public float blastRadius = 4.2f;

    [Header("Vung lua de lai")]
    public float chayBanKinh = 4.5f;      // rong hon 25% so voi 3,6 m
    public float chayThoiGian = 5f;
    public float chaySatThuongMoiGiay = 26f;

    [Header("Cay bat lua")]
    [Tooltip("Sat thuong moi giay cua ngon lua tren cai cay dang chay")]
    public float cayChaySatThuong = 22f;

    public LayerMask hitMask;
    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    /// <summary>
    /// Cho bao nhieu giay roi moi bat dau roi. Dung cho LOAT ba qua: qua dau
    /// roi ngay, hai qua sau xep hang phia sau.
    /// </summary>
    public float treTruocKhiRoi = 0f;

    Vector3 dir;
    Vector3 diemDich;

    /// <summary>Cho ma qua nay se dap xuong. Cong khai de con kiem tra duoc.</summary>
    public Vector3 DiemDich { get { return diemDich; } }
    float age;
    bool daNo;
    bool daHienHinh;

    /// <summary>Roi qua lau ma chua cham gi thi tu no, khoi bay mai xuong duoi.</summary>
    const float SongToiDa = 6f;

    public static ThienThach Spawn(Vector3 diemNgam, LayerMask hitMask, LayerMask damageMask)
    {
        var go = new GameObject("ThienThach");
        var tt = go.AddComponent<ThienThach>();

        tt.hitMask = hitMask;
        tt.damageMask = damageMask;

        // Diem dat that su ngay duoi cho ngam
        Vector3 dich = diemNgam;
        dich.y = VfxFactory.GroundY(diemNgam);
        tt.diemDich = dich;

        // Sinh ra tren cao, lech sang mot ben cho duong roi xien
        float goc = Random.Range(0f, Mathf.PI * 2f);
        Vector3 lech = new Vector3(Mathf.Cos(goc), 0f, Mathf.Sin(goc)) * tt.doNghieng;
        go.transform.position = dich + Vector3.up * tt.doCaoRoi + lech;

        tt.dir = (dich - go.transform.position).normalized;
        go.transform.rotation = Quaternion.LookRotation(tt.dir);

        // KHONG dung hinh o day. Qua nao dang cho luot thi phai VO HINH -
        // dung hinh ngay tu dau thi hai qua sau treo lo lung tren troi cho san,
        // nguoi choi ngua len la thay.
        return tt;
    }

    /// <summary>
    /// Goi ca MOT LOAT thien thach roi noi duoi nhau.
    ///
    /// Ba qua khong roi trung mot cho: qua dau dung diem ngam, hai qua sau lech
    /// ra chung quanh. Roi chong len nhau thi ba vu no gop thanh mot, nhin ra
    /// mot qua to chu khong ra mot loat.
    /// </summary>
    public static void SpawnLoat(Vector3 diemNgam, LayerMask hitMask, LayerMask damageMask,
                                 Damageable boQua = null,
                                 int soQua = 3, float cachNhau = 0.7f, float tanRong = 2.8f)
    {
        for (int i = 0; i < soQua; i++)
        {
            Vector3 diem = diemNgam;
            if (i > 0)
            {
                float goc = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(tanRong * 0.45f, tanRong);
                diem += new Vector3(Mathf.Cos(goc) * r, 0f, Mathf.Sin(goc) * r);
            }

            var tt = Spawn(diem, hitMask, damageMask);
            tt.treTruocKhiRoi = i * cachNhau;
            tt.boQua = boQua;
        }
    }

    void Update()
    {
        if (daNo) return;

        float dt = Time.deltaTime;

        // Chua toi luot thi dung im tren troi, VO HINH, va khong tinh tuoi.
        // Khong tru tuoi trong luc cho, khong thi qua cuoi cung cua loat co the
        // het "SongToiDa" ngay khi vua bat dau roi.
        if (treTruocKhiRoi > 0f)
        {
            treTruocKhiRoi -= dt;
            if (treTruocKhiRoi > 0f) return;
        }

        if (!daHienHinh)
        {
            daHienHinh = true;
            VfxFactory.BuildThienThachVisual(transform, bodyRadius);
        }

        age += dt;

        float buoc = speed * dt;
        Vector3 tu = transform.position;

        // Quet DUNG doan vua di duoc. Thien thach to va roi nhanh, moi khung
        // hinh di ca met - kiem tra tung diem thi no xuyen thang qua mat dat.
        RaycastHit hit;
        if (Physics.SphereCast(tu, bodyRadius, dir, out hit, buoc + 0.05f, hitMask,
                               QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point - dir * bodyRadius * 0.4f;
            No();
            return;
        }

        transform.position = tu + dir * buoc;

        // Luoi bao ve: xuong qua diem dich roi ma van chua cham gi (dat khong co
        // collider chang han) thi cu no ngay tai do.
        if (transform.position.y <= diemDich.y + 0.05f) { No(); return; }
        if (age >= SongToiDa) No();
    }

    void No()
    {
        if (daNo) return;
        daNo = true;

        Vector3 tam = transform.position;
        tam.y = VfxFactory.GroundY(tam) + 0.05f;

        // 1. Sat thuong tuc thi ca vung
        CombatUtil.AreaDamage(tam, blastRadius, impactDamage, damageMask,
                              DamageType.Fire, 2.2f, boQua);

        // 2. Vu no nhin thay duoc
        VfxFactory.BuildFireExplosion(tam, blastRadius * 0.95f);
        CameraShake.Shake(0.35f, 0.22f);

        // 3. Dat con chay am i - day moi la phan lam nen ky nang nay.
        //
        // TAT DUOC bang cach dat thoi gian hoac ban kinh ve 0. Thien thach cua
        // Quy du dung duong nay: muoi con cung nem, moi qua mot dam chay thi ca
        // san thanh bien lua va nguoi choi khong con cho dat chan.
        if (chayThoiGian > 0f && chayBanKinh > 0f)
        {
            VungLua.Spawn(tam, chayBanKinh, chayThoiGian, chaySatThuongMoiGiay, damageMask);

            // 4. Cay dung trong dam lua thi BAT LUA ca cai cay.
            //
            // Buoc theo dung dieu kien cua vung lua, khong tach ra thanh mot
            // cong tac rieng: thien thach cua Quy du da tat vung lua di (muoi
            // con cung nem thi ca san thanh bien lua), va neu no van dot duoc
            // cay thi ca khu rung chay het trong mot dot ban - dung cai loi da
            // tranh duoc o vung lua.
            CayChay.DotCayQuanh(tam, chayBanKinh, damageMask, cayChaySatThuong);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, blastRadius);
        Gizmos.color = new Color(1f, 0.25f, 0.05f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, chayBanKinh);
    }
}
