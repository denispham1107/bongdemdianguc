using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 7 - GIUT SET.
///
/// Mot tia set phong tu tay phu thuy sang ke dich gan nhat, roi LAN TIEP tu
/// con do sang con khac o quanh no: moi nhip nhay them mot tia moi. Khac han
/// Sam set - Sam set giang tia tu tren troi xuong nhung diem roi rac, con Giut
/// set la MOT mach dien chay xuyen qua ca dam quai.
///
/// Ba dieu lam no doc ra la "lan" chu khong phai "danh nhieu con cung luc":
///
///   1. MOI NHIP CACH NHAU 0,07 giay. Ve het mot luot trong cung mot khung
///      hinh thi nguoi choi thay mot mang set hien ra dong loat, khong doc duoc
///      thu tu lan.
///   2. Tia moi ve tu con VUA TRUNG chu khong tu tay phu thuy - do la thu noi
///      len mach dien di chuyen.
///   3. SAT THUONG GIAM DAN theo tung nhip, nen con cuoi mach chiu it hon con
///      dau. Khong giam thi ky nang nay manh hon moi ky nang khac trong game.
/// </summary>
public class GiatSet : MonoBehaviour
{
    [Header("Tam va muc tieu")]
    [Tooltip("Tam ban cua tia dau tien, met")]
    // 19,5 = 13 x 1,5. Tang 50% theo yeu cau.
    public float range = 19.5f;

    [Tooltip("Ban kinh tim con tiep theo quanh con vua trung, met")]
    public float chainRadius = 5.5f;

    [Tooltip("So lan LAN THEM sau muc tieu dau tien")]
    public int maxChains = 5;

    [Tooltip("So MACH phong thang tu tay, moi mach di mot muc tieu rieng")]
    public int soTiaDau = 3;

    [Header("Sat thuong")]
    public float damage = 30f;

    [Tooltip("Moi nhip lan, sat thuong con lai bao nhieu phan")]
    [Range(0.3f, 1f)]
    public float damageFalloff = 0.85f;

    [Header("Nhip")]
    [Tooltip("Cach nhau bao lau giua hai nhip lan, giay")]
    public float hopDelay = 0.07f;

    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong an don cua chinh minh. Chi co nghia
    /// khi choi doi khang, luc do lop Player nam trong damageMask.</summary>
    public Damageable boQua;

    [Header("Mau tia")]
    [Tooltip("Loi tia - sang gan nhu trang")]
    public Color mauLoi = Color.white;
    [Tooltip("Quang quanh tia - day moi la mau nguoi choi NHIN THAY")]
    public Color mauQuang = new Color(0.45f, 0.70f, 1f, 1f);

    Vector3 origin, huong;

    /// <summary>So mach chua chay xong - het thi moi xoa vat the.</summary>
    int machConChay;

    static readonly Collider[] buffer = new Collider[64];

    /// <summary>
    /// Phong mot mach Giut set. <paramref name="origin"/> la diem xuat phat
    /// (tay phu thuy), <paramref name="huong"/> la huong nham.
    /// </summary>
    public static GiatSet Phong(Vector3 origin, Vector3 huong, LayerMask damageMask)
    {
        var go = new GameObject("GiatSet");
        go.transform.position = origin;

        var gs = go.AddComponent<GiatSet>();
        gs.origin = origin;
        gs.huong = huong.sqrMagnitude > 0.0001f ? huong.normalized : Vector3.forward;
        gs.damageMask = damageMask;
        return gs;
    }

    /// <summary>
    /// Ban cho QUAI dung: doi mau tia, sat thuong va tam ban.
    ///
    /// Mau nam o QUANG chu khong o loi. Loi tia luon gan nhu trang - do la cach
    /// tia set that trong: cai nguoi choi doc ra mau la vung sang toa quanh no.
    /// Nhuom ca loi thanh xanh la thi tia mat het ve chay bong, nhin ra mot
    /// coong nhua phat sang.
    /// </summary>
    public static GiatSet PhongCuaQuai(Vector3 origin, Vector3 huong, LayerMask damageMask,
                                       float satThuong, float tam, Color loi, Color quang)
    {
        var gs = Phong(origin, huong, damageMask);
        gs.damage = satThuong;
        gs.range = tam;
        gs.mauLoi = loi;
        gs.mauQuang = quang;

        // Quai chi danh MOT nguoi choi, khong co gi de lan sang
        gs.maxChains = 0;
        gs.soTiaDau = 1;
        return gs;
    }

    void Start()
    {
        StartCoroutine(ChayTatCa());
    }

    /// <summary>
    /// Phong <see cref="soTiaDau"/> mach cung mot luc, moi mach di mot muc tieu
    /// rieng, roi tu do lan tiep nhu cu.
    ///
    /// Ba mach DUNG CHUNG mot danh sach "da trung": mot con quai chi an mot lan
    /// du no nam trong tam cua ca ba mach. Cho moi mach mot danh sach rieng thi
    /// dam quai dung sat nhau an ba lan sat thuong, va mach nay lan sang chinh
    /// con ma mach kia vua danh - nhin ra mot mo tia chong len nhau.
    /// </summary>
    IEnumerator ChayTatCa()
    {
        var daTrung = new HashSet<Damageable>();
        var dauTien = TimMayMucTieuDau(Mathf.Max(1, soTiaDau));

        if (dauTien.Count == 0)
        {
            BanVaoKhongKhi();
            Destroy(gameObject, 0.6f);
            yield break;
        }

        // Danh dau truoc TAT CA muc tieu dau: neu khong, mach thu nhat lan sang
        // dung con ma mach thu hai sap di toi, va mach hai thanh ra khong con
        // gi de danh.
        foreach (var d in dauTien) daTrung.Add(d);

        CameraShake.Shake(0.14f, 0.05f);

        machConChay = dauTien.Count;
        for (int i = 0; i < dauTien.Count; i++)
            StartCoroutine(MotMach(dauTien[i], daTrung));

        while (machConChay > 0) yield return null;
        Destroy(gameObject, 0.6f);
    }

    /// <summary>Mot mach: danh muc tieu dau roi lan tiep sang con quanh no.</summary>
    IEnumerator MotMach(Damageable muc, HashSet<Damageable> daTrung)
    {
        Vector3 tu = origin;
        float sat = damage;

        for (int nhip = 0; nhip <= maxChains && muc != null; nhip++)
        {
            Vector3 den = DiemTrung(muc);
            VeTia(tu, den, nhip == 0 ? 1.15f : 0.9f);
            VfxFactory.SetChayDen(den, 1.25f);
            VfxFactory.NamChuongNgai(den, 2.2f, VfxFactory.LopChuongNgai);

            muc.GhiKeDanh(boQua);
            muc.TakeDamage(sat, DamageType.Lightning, den);
            daTrung.Add(muc);

            tu = den;
            sat *= damageFalloff;
            muc = TimConTiepTheo(den, daTrung);

            // Danh dau NGAY khi chon: hai mach chay song song, khong danh dau
            // som thi ca hai cung nham vao mot con o nhip sau.
            if (muc != null)
            {
                daTrung.Add(muc);
                yield return new WaitForSeconds(hopDelay);
            }
        }

        machConChay--;
    }

    /// <summary>Khong co ai trong tam: van phong mot tia ra phia truoc roi tat.</summary>
    void BanVaoKhongKhi()
    {
        // Im lang khong lam gi thi nguoi choi tuong nut hong.
        //
        // Tia CHAM VAO DAU thi chay cho do: ban thang vao mot goc tuong hay mot
        // than cay ma khong de lai dau vet gi thi nhin ra tia set di xuyen qua
        // vat the.
        Vector3 den = origin + huong * range * 0.62f;

        RaycastHit hit;
        if (Physics.Raycast(origin, huong, out hit, range * 0.62f,
                            VfxFactory.LopChuongNgai, QueryTriggerInteraction.Ignore))
        {
            den = hit.point;
            VeTia(origin, den, 1f);
            VfxFactory.NamDen(hit.point, hit.normal, Random.Range(0.40f, 0.70f));
            VfxFactory.SetChayDen(hit.point + hit.normal * 0.1f, 1.0f, 0.7f);
        }
        else VeTia(origin, den, 1f);
    }

    /// <summary>Diem ngam tren than ke dich, khong phai duoi chan no.</summary>
    static Vector3 DiemTrung(Damageable d)
    {
        var col = d.GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.center;
        return d.transform.position + Vector3.up * 0.9f;
    }

    void VeTia(Vector3 tu, Vector3 den, float day)
    {
        var arc = LightningArc.Create(tu, den, day, 0.30f);
        arc.coreColor = mauLoi;
        arc.glowColor = mauQuang;

        // HEP HON HAN cac tia set khac. Giut set phong RA TU TAY nguoi choi:
        // voi be ngang mac dinh (quang 1,05 -> dai rong 2,1 m) doan gan goc
        // trum kin ca dau lan mat nhan vat, con moi khuc gap thi quet ngang qua
        // than. Sam set giang tu troi xuong cach xa nguoi choi nen khong dinh.
        arc.coreWidth = 0.16f * day;
        arc.glowWidth = 0.42f * day;
        // Doan cang dai thi cang nhieu khuc gap, khong thi tia dai thanh mot
        // duong gan nhu thang con tia ngan thi gap khuc lung tung.
        float dai = Vector3.Distance(tu, den);
        arc.segments = Mathf.Clamp(Mathf.RoundToInt(dai * 2.2f), 8, 26);
        arc.jitter = Mathf.Clamp(dai * 0.10f, 0.22f, 0.75f);
        arc.branches = 3;
    }

    /// <summary>
    /// Lay toi da <paramref name="soLuong"/> muc tieu trong tam, xep theo diem.
    ///
    /// Cham diem theo CA hai thu - lech huong bao nhieu va o xa bao nhieu. Chi
    /// lay con gan nhat thi nham ve phia truoc van ban trung con dang dung sau
    /// lung.
    /// </summary>
    List<Damageable> TimMayMucTieuDau(int soLuong)
    {
        int n = Physics.OverlapSphereNonAlloc(origin, range, buffer, damageMask,
                                              QueryTriggerInteraction.Collide);
        ungVien.Clear();

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;

            // OverlapSphere tra ve TUNG COLLIDER: mot con quai co may collider
            // la no vao danh sach may lan, va ba tia se doi het vao mot con.
            bool trung = false;
            for (int j = 0; j < ungVien.Count; j++)
                if (ungVien[j].ai == d) { trung = true; break; }
            if (trung) continue;

            Vector3 toi = DiemTrung(d) - origin;
            toi.y = 0f;
            float xa = toi.magnitude;
            if (xa < 0.01f) continue;

            float hop = Vector3.Dot(toi / xa, huong);      // -1..1
            if (hop < -0.2f) continue;                     // sau lung han thi bo

            ungVien.Add(new Diem { ai = d, so = hop * 2.4f - xa / Mathf.Max(1f, range) });
        }

        ungVien.Sort(delegate (Diem a, Diem b) { return b.so.CompareTo(a.so); });

        var ra = new List<Damageable>();
        for (int i = 0; i < ungVien.Count && ra.Count < soLuong; i++)
            ra.Add(ungVien[i].ai);
        return ra;
    }

    struct Diem { public Damageable ai; public float so; }
    static readonly List<Diem> ungVien = new List<Diem>();

    /// <summary>Con gan nhat quanh <paramref name="tu"/> ma chua bi trung.</summary>
    Damageable TimConTiepTheo(Vector3 tu, HashSet<Damageable> daTrung)
    {
        int n = Physics.OverlapSphereNonAlloc(tu, chainRadius, buffer, damageMask,
                                              QueryTriggerInteraction.Collide);
        Damageable gan = null;
        float ganNhat = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            var d = buffer[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || daTrung.Contains(d)) continue;
            if (boQua != null && d == boQua) continue;

            float xa = Vector3.Distance(DiemTrung(d), tu);
            if (xa < ganNhat) { ganNhat = xa; gan = d; }
        }
        return gan;
    }
}
