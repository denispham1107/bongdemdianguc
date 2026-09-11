using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 6 - KHIENG BAO VE.
///
/// Mot qua cau nang luong bao quanh nhan vat. Khieng lam BA viec, va ca ba deu
/// can thiet - thieu mot cai la no khong con la cai khien nua:
///
///   1. HAP THU SAT THUONG. Moi don danh trung nguoi choi deu tru vao mau khieng
///      truoc. Xem cho goi trong <see cref="Damageable.TakeDamage"/>.
///
///   2. CHAN QUAI LAI GAN. Qua cau co collider dac o lop rieng "Khieng"; quai
///      dung phai thi khong vao duoc ben trong, chi dam vao mat cau.
///
///   3. CHAN DAN TU XA. Cau lua cua mu phu thuy no ngay tren mat cau chu khong
///      bay xuyen qua - <see cref="EnemyAI"/> them lop Khieng vao mask cua dan.
///
/// VI SAO PHAI DUNG MOT LOP RIENG: neu de khieng o lop Default thi dan cua CHINH
/// NGUOI CHOI cung no vao no. Dan nguoi choi sinh ra o tay, tuc BEN TRONG qua
/// cau, nen no se dam vao mat trong va no ngay lap tuc - bat khieng len la khong
/// ban ra duoc phat nao. Lop rieng cho phep noi "dan dich thi chan, dan minh thi
/// cho qua".
///
/// DON PHA VO KHIENG KHONG TRAN SANG NGUOI CHOI. Khieng con 5 mau ma an mot don
/// 200 thi nguoi choi van khong mat mau nao - dung nhu yeu cau "khi mau khieng
/// mat het se no tung, va luc do nhan vat moi chiu sat thuong".
/// </summary>
public class Khieng : MonoBehaviour
{
    [Header("Mau khieng")]
    public float mauToiDa = 150f;

    /// <summary>
    /// Mau hien tai. MAC DINH BANG 0 - tuc "chua bat".
    ///
    /// Khong duoc de mac dinh 150. Component nay co the nam san tren nhan vat
    /// (bi them nham vao canh, hoac sot lai tu mot lan chay thu trong Editor),
    /// va neu mac dinh la 150 thi DangBat tra ve true ngay tu khi vao game -
    /// khieng tu bat len du nguoi choi khong bam nut nao. Da vap dung loi do.
    /// Chi <see cref="Bat"/> moi duoc nap mau vao.
    /// </summary>
    public float mau = 0f;

    [Header("Hinh dang")]
    public float banKinh = 2.2f;

    /// <summary>Con mau thi con chan duoc.</summary>
    public bool DangBat { get { return mau > 0f; } }

    public float Mau01 { get { return mauToiDa <= 0f ? 0f : Mathf.Clamp01(mau / mauToiDa); } }

    GameObject hinh;
    Material vatLieu;
    SphereCollider vachChan;
    float loe;                       // do loe len sau khi vua chan mot don

    static readonly int idLoe = Shader.PropertyToID("_Loe");
    static readonly int idYeu = Shader.PropertyToID("_Yeu");

    /// <summary>Lop rieng cua khieng - xem ghi chu tren dau lop.</summary>
    public const string TenLop = "Khieng";

    /// <summary>
    /// Moi cai khieng DANG BAT trong man. Giu san mot danh sach thay vi quet ca
    /// canh moi lan co vu no - mot tran co the co hang chuc vu no mot luc.
    /// </summary>
    static readonly List<Khieng> dangBat = new List<Khieng>();

    /// <summary>
    /// Mot vu no gan khieng thi TRU MAU KHIENG, du chu khieng dung ngoai tam no.
    ///
    /// VI SAO CAN HAM NAY: dan cua mu phu thuy no ngay TREN MAT VOM, ma ban
    /// kinh no (2,6 m) lai NHO HON ban kinh khieng (3,04 m) - nen nguoi choi
    /// dung giua khieng khong bao gio nam trong tam sat thuong. Ket qua la
    /// khieng chan duoc het nhung KHONG BAO GIO MAT MAU: bat tu.
    ///
    /// Tra ve so khieng bi tru mau.
    /// </summary>
    // boQua la nguoi tung phep. Khieng CUA HO khong bi tru: truoc day thieu cho
    // nay, cau lua cua minh no trong vong 6,4 m quanh minh la khieng cua minh
    // mat 55 mau - do duoc o menu 49.
    public static int NoTrungKhieng(Vector3 tamNo, float banKinhNo, float satThuong,
                                    LayerMask mask, Damageable boQua = null)
    {
        int trung = 0;
        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!k.DangBat) continue;

            // Chi tinh khieng cua nhung ai nam trong mask sat thuong cua don do
            if ((mask.value & (1 << k.gameObject.layer)) == 0) continue;

            // Khieng cua chinh nguoi tung phep
            if (boQua != null && k.gameObject == boQua.gameObject) continue;

            float d = Vector3.Distance(tamNo, k.transform.position);

            // Chu khieng DA nam trong tam no roi thi don da di qua duong
            // TakeDamage -> HapThu. Tinh them lan nua la tru mau hai lan.
            if (d <= banKinhNo) continue;

            // Vu no co cham toi mat vom khong
            if (d > banKinhNo + k.banKinh) continue;

            k.HapThu(satThuong);
            trung++;
        }
        return trung;
    }

    /// <summary>
    /// Mot vien dan bay tu <paramref name="tu"/> theo <paramref name="huong"/>
    /// (da chuan hoa) them <paramref name="quangDuong"/> met: co cham mat vom
    /// cua ai khong. Tra ve khoang cach toi cho cham GAN NHAT.
    ///
    /// VI SAO KHONG DUNG VA CHAM VAT LY (lop "Khieng" trong mask) nhu dan cua
    /// quai: cau lua cua NGUOI CHOI cung phai bi khieng cua nguoi choi KHAC
    /// chan - truoc day no bay xuyen thang vao trong vom va no cach tam 1,0 m
    /// (menu 49). Nhung ba luat sau phai giu, va hoi thang o day thi ro rang
    /// hon trong cay vao cach Unity xu ly vien cau xuat phat ben trong mot
    /// collider:
    ///   - Khieng CUA NGUOI TUNG thi bo qua (<paramref name="boQua"/>).
    ///   - Chi khieng cua nhung ai nam trong mask sat thuong cua vien dan.
    ///   - Vien dan xuat phat BEN TRONG vom thi vom khong chan no: dung trong
    ///     khieng cua nguoi khac ma ban ra thi van ra duoc.
    /// </summary>
    public static bool DanChamVom(Vector3 tu, Vector3 huong, float quangDuong, float banKinhDan,
                                  LayerMask mask, Damageable boQua, out float khoangCach)
    {
        khoangCach = float.MaxValue;
        bool cham = false;

        for (int i = dangBat.Count - 1; i >= 0; i--)
        {
            var k = dangBat[i];
            if (k == null) { dangBat.RemoveAt(i); continue; }
            if (!k.DangBat) continue;
            if ((mask.value & (1 << k.gameObject.layer)) == 0) continue;
            if (boQua != null && k.gameObject == boQua.gameObject) continue;

            // Tam vom o chan nhan vat (xem Dung). Cong ban kinh vien dan: no no
            // khi MAT NGOAI cua no cham vom, khong phai khi tam no cham.
            Vector3 c = k.transform.position;
            float R = k.banKinh + banKinhDan;

            Vector3 oc = tu - c;
            float c2 = oc.sqrMagnitude - R * R;
            if (c2 <= 0f) continue;                  // xuat phat trong vom

            float b = Vector3.Dot(oc, huong);
            if (b >= 0f) continue;                   // dang bay ra xa vom

            float delta = b * b - c2;
            if (delta < 0f) continue;                // bay truot ben canh

            float t = -b - Mathf.Sqrt(delta);
            if (t < 0f || t > quangDuong) continue;

            if (t < khoangCach) { khoangCach = t; cham = true; }
        }
        return cham;
    }

    // ================================================================

    /// <summary>
    /// Bat khieng len cho <paramref name="nguoi"/>. Dang co khieng thi NAP LAI
    /// day mau chu khong chong them mot cai nua.
    /// </summary>
    public static Khieng Bat(GameObject nguoi, float mauKhieng, float banKinh)
    {
        var k = nguoi.GetComponent<Khieng>();
        if (k == null) k = nguoi.AddComponent<Khieng>();

        k.mauToiDa = mauKhieng;
        k.mau = mauKhieng;
        k.banKinh = banKinh;
        k.enabled = true;
        k.Dung();
        return k;
    }

    void Dung()
    {
        DonBangVaCham();

        // Don sach VOM CU truoc khi dung vom moi.
        //
        // 'hinh' la bien private khong luu vao canh, nen sau khi vao Play mode
        // no luon bang null - trong khi GameObject con "Khieng" tu lan truoc
        // VAN CON do. Khong don thi moi lan bat lai la chong them mot vom nua,
        // va vom cu khong con ai tat duoc: nguoi choi thay khieng vo roi lai
        // hien ra nhu chua he vo.
        if (hinh == null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var con = transform.GetChild(i);
                if (con.name != "Khieng" && con.name != "VachChanKhieng") continue;
                if (Application.isPlaying) Destroy(con.gameObject);
                else DestroyImmediate(con.gameObject);
            }
            vachChan = null;
        }

        if (hinh == null)
        {
            hinh = VfxFactory.BuildKhieng(transform, banKinh);
            // Ban RIENG cho tung cai khieng: moi cai co muc mau va do loe khac
            // nhau, dung chung mot vat lieu thi chung dam len nhau.
            //
            // Ngoai Play mode thi KHONG duoc goi .material - no nhan ban vat lieu
            // va bo lai trong canh, Unity se keu "leak materials into the scene".
            var mr = hinh.GetComponentInChildren<MeshRenderer>();
            if (mr != null)
                vatLieu = Application.isPlaying ? mr.material : mr.sharedMaterial;
        }
        hinh.SetActive(true);

        if (!dangBat.Contains(this)) dangBat.Add(this);

        if (vachChan == null)
        {
            var go = new GameObject("VachChanKhieng");
            go.transform.SetParent(transform, false);
            go.layer = LayerMask.NameToLayer(TenLop);
            vachChan = go.AddComponent<SphereCollider>();
        }

        // Tam vach chan phai TRUNG voi tam qua cau nhin thay duoc, khong thi
        // quai bi chan o mot vong khong khop voi cho nguoi choi thay.
        //
        // Van dung hinh CAU du qua cau ve ra hoi det: SphereCollider khong bop
        // det duoc, ma lay ban kinh ngang thi vung chan hoi cao hon vom mot chut
        // - thua con hon thieu, quai khong loi vao duoc qua mep tren.
        // Tam o CHAN nhan vat, dung bang chan vom. Nua duoi hinh cau chim
        // xuong duoi dat nen khong anh huong gi - quai van bi chan dung o mep
        // vom theo phuong ngang.
        vachChan.center = Vector3.zero;
        vachChan.radius = banKinh;
        vachChan.enabled = true;

        CapNhatVatLieu();
    }

    /// <summary>
    /// Dat bang va cham cho lop Khieng. Phai lam bang code luc chay: bang trong
    /// Project Settings khong tu co dong nao cho mot lop vua duoc them vao.
    /// </summary>
    static bool daDonBang;
    static void DonBangVaCham()
    {
        if (daDonBang) return;
        daDonBang = true;

        int kh = LayerMask.NameToLayer(TenLop);
        if (kh < 0) { Debug.LogError("[Khieng] Chua co lop '" + TenLop + "'"); return; }

        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Enemy"), false);  // CHAN quai
        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Player"), true);
        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Ground"), true);
        Physics.IgnoreLayerCollision(kh, LayerMask.NameToLayer("Spell"), true);
        Physics.IgnoreLayerCollision(kh, 0, true);
        Physics.IgnoreLayerCollision(kh, kh, true);
    }

    // ================================================================

    /// <summary>
    /// Khieng nuot bot sat thuong. Tra ve phan CON LAI danh vao nguoi choi.
    ///
    /// Luon tra ve 0 khi khieng con bat: don pha vo khieng khong tran sang
    /// nguoi choi.
    /// </summary>
    public float HapThu(float amount)
    {
        if (!DangBat || amount <= 0f) return amount;

        // Khieng cua BAN SAO van chan don (don khong di xuyen qua vom), nhung
        // KHONG tu tru mau: mau khieng that nam o may cua chu nhan, den day qua
        // goi tin. Tru o day nua thi khieng vo tren may nay trong khi ben kia
        // con nua mau - dung loi nguoi choi thay.
        if (chuNhan == null) chuNhan = GetComponent<Damageable>();
        if (chuNhan != null && chuNhan.mauDoMayKhacQuyet) { loe = 1f; return 0f; }

        mau -= amount;
        loe = 1f;

        if (mau <= 0f)
        {
            mau = 0f;
            Vo();
        }
        else CapNhatVatLieu();

        return 0f;
    }

    void Update()
    {
        if (loe > 0f)
        {
            loe = Mathf.MoveTowards(loe, 0f, Time.deltaTime * 3.2f);
            if (vatLieu != null) vatLieu.SetFloat(idLoe, loe);
        }

        // Khieng tho nhe cho no khong dung im nhu mot khoi thuy tinh
        if (hinh != null && DangBat)
        {
            float n = 1f + Mathf.Sin(Time.time * 1.7f) * 0.018f;
            hinh.transform.localScale = Vector3.one * n;
        }
    }

    void CapNhatVatLieu()
    {
        if (vatLieu == null) return;
        vatLieu.SetFloat(idYeu, Mau01);
        vatLieu.SetFloat(idLoe, loe);
    }

    /// <summary>Het mau: no tung roi tat han.</summary>
    void Vo()
    {
        Vector3 tam = transform.position + Vector3.up * banKinh * 0.55f;
        VfxFactory.KhiengVo(tam, banKinh);
        CameraShake.Shake(0.22f, 0.10f);

        Tat();
    }

    Damageable chuNhan;

    /// <summary>Dat mau khieng theo loi ke cua may chu nhan. Chi ban sao dung.</summary>
    public void DatMauTuMang(float mau01)
    {
        if (!DangBat) return;
        float moi = Mathf.Clamp01(mau01) * mauToiDa;
        if (moi < mau - 0.5f) loe = 1f;        // vua an don ben kia - loe len cho thay
        mau = Mathf.Max(0.01f, moi);
        CapNhatVatLieu();
    }

    /// <summary>May chu nhan bao khieng da vo - vo theo, co hieu ung, chu khong
    /// tat im lang: nguoi xem phai thay no vo dung luc no vo that.</summary>
    public void VoTuMang()
    {
        if (!DangBat) return;
        mau = 0f;
        Vo();
    }

    public void Tat()
    {
        dangBat.Remove(this);
        mau = 0f;
        if (hinh != null) hinh.SetActive(false);
        if (vachChan != null) vachChan.enabled = false;
    }

    void OnDisable()
    {
        dangBat.Remove(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, banKinh);
    }
}
