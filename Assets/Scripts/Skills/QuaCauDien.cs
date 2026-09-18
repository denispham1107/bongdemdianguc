using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 13 - "QUA CAU DIEN" (them 18/09/2026, nguoi dung gui anh Diablo III).
///
/// Mot QUA CAU DIEN hien ra LO LUNG NGAY CANH KE DICH gan cho ngam nhat (khong co ai thi dung
/// dung cho ngam), roi cu 0,4 giay BAN MOT LUOT TIA DIEN ra chung quanh - toi da 10 luot.
/// Moi luot ban toi da 5 tia, MOI KE DICH MOT TIA (nguoi dung chon 18/09/2026: thua thi bo),
/// chon 5 ke gan qua cau nhat trong 9 m - ca quai LAN nguoi choi khac.
/// Moi tia: sat thuong = Giut set o CAP 5 (75 x 1,2^4 = 155,5) va 30% lam ke trung don CHOANG 1,5 giay.
/// Hoi chieu 5 giay, 55 nang luong, tam ngam 18 m (bang Thien thach - nguoi dung chon).
///
/// Hinh dung bang Blender MCP - xem <see cref="VfxFactory.QuaCauDienHinh"/>.
/// </summary>
public class QuaCauDien : MonoBehaviour
{
    // ---- Nguoi dung chot 18/09/2026 ----
    public const float HoiChieu = 5f;
    public const float NangLuong = 55f;
    /// <summary>Tam ngam - BANG Thien thach (PlayerController.TamNgam tra 18 m cho Thien thach).</summary>
    public const float Tam = 18f;
    public const int SoLuot = 10;
    public const float NhipLuot = 0.4f;
    public const int SoTiaMoiLuot = 5;
    public const float BanKinhBan = 9f;
    public const float XacSuatChoang = 0.3f;
    public const float GiayChoang = 1.5f;

    /// <summary>Qua cau lo lung cao bang nay so voi mat dat.</summary>
    public const float CaoLoLung = 1.7f;
    public const float BanKinhCau = 0.85f;
    /// <summary>Tim ke dich de dat qua cau "ngay gan doi thu" trong ban kinh nay quanh cho ngam.</summary>
    public const float TimKeGanCho = 7f;

    /// <summary>Khong co ai trong tam thi qua cau van DUNG NGUYEN CHO doi, nhung khong qua ngan nay giay
    /// (nguoi dung chot 18/09/2026 - de khong co qua cau nam lai mai tren ban do).</summary>
    public const float GiayChoToiDa = 20f;

    /// <summary>
    /// Sat thuong moi tia = sat thuong ky nang GIUT SET o CAP 5 (nguoi dung chot 18/09/2026).
    /// Doc thang tu hang cua Giut set chu khong chep tay con so: sua Giut set thi cai nay tu theo.
    /// </summary>
    public static float SatThuongTia
    {
        get { return GiatSet.SatThuongNguoiChoi * CapDo.SatThuongTheoCap(5); }
    }

    public float satThuongTia = 0f;       // 0 = lay SatThuongTia luc Start
    public float giayChoang = GiayChoang;
    public float banKinhBan = BanKinhBan;
    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - khong tu ban minh khi choi doi khang (lop Player nam trong damageMask).</summary>
    public Damageable boQua;

    /// <summary>Dem cho phep thu (menu 74).</summary>
    public static int SoLuotDaBan, SoTiaDaBan, SoLanTrung;

    int daBan;          // so luot DA BAN RA THAT (co it nhat mot tia)
    float nhip;
    float dongHoSong;   // tong thoi gian qua cau da ton tai
    GameObject hinh;

    static readonly Collider[] bo = new Collider[64];
    readonly List<Damageable> mucTieu = new List<Damageable>();

    /// <summary>
    /// Goi qua cau ra. <paramref name="choNgam"/> la diem nguoi choi ngam; qua cau tu don ve
    /// canh ke dich gan diem ay nhat.
    /// </summary>
    public static QuaCauDien Spawn(Vector3 choNgam, LayerMask damageMask, Damageable boQua,
                                   float heSoSatThuong = 1f, float themGiayChoang = 0f)
    {
        Vector3 cho = ChoDat(choNgam, damageMask, boQua);

        var go = new GameObject("QuaCauDien");
        go.transform.position = cho;

        var qc = go.AddComponent<QuaCauDien>();
        qc.damageMask = damageMask;
        qc.boQua = boQua;
        qc.satThuongTia = SatThuongTia * heSoSatThuong;
        qc.giayChoang = GiayChoang + themGiayChoang;
        return qc;
    }

    /// <summary>Cho dat qua cau: canh KE DICH gan cho ngam nhat, khong co ai thi dung cho ngam.</summary>
    static Vector3 ChoDat(Vector3 choNgam, LayerMask damageMask, Damageable boQua)
    {
        Damageable gan = null;
        float xaNhat = float.MaxValue;
        int n = Physics.OverlapSphereNonAlloc(choNgam, TimKeGanCho, bo, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = bo[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;
            float kc = Vector3.Distance(d.transform.position, choNgam);
            if (kc < xaNhat) { xaNhat = kc; gan = d; }
        }

        Vector3 cho = gan != null ? gan.transform.position : choNgam;
        if (gan != null)
        {
            // Lech sang mot ben cho khoi nam trong nguoi ke dich
            Vector2 lech = Random.insideUnitCircle.normalized * 1.4f;
            cho += new Vector3(lech.x, 0f, lech.y);
        }
        cho.y = VfxFactory.GroundY(cho) + CaoLoLung;
        return cho;
    }

    void Start()
    {
        if (satThuongTia <= 0f) satThuongTia = SatThuongTia;
        // Hinh phai song bang DOI CUA QUA CAU: cau co the phai doi ke dich toi gan nen khong biet truoc
        // no song bao lau - giao han toi da, va OnDestroy tat hinh cung luc voi qua cau.
        hinh = VfxFactory.QuaCauDienHinh(transform.position, BanKinhCau, GiayChoToiDa + 1f);
        CameraShake.Shake(0.18f, 0.05f);
        nhip = 0.12f;      // luot dau ban gan nhu ngay khi cau hien ra
    }

    void Update()
    {
        // PHAI BAN DU 10 LUOT roi moi tan. Truoc 18/09/2026 dong ho chay deu 0,4 giay mot luot bat ke co ban
        // duoc hay khong, nen khong co ke dich nao quanh day thi qua cau bien mat sau 4 giay MA CHUA DANH lan
        // nao - nguoi dung khong muon vay: chua du 10 lan thi cu dung nguyen cho doi.
        if (daBan >= SoLuot) { Destroy(gameObject); return; }

        dongHoSong += Time.deltaTime;
        if (dongHoSong >= GiayChoToiDa) { Destroy(gameObject); return; }

        nhip -= Time.deltaTime;
        if (nhip > 0f) return;

        int soTiaVuaBan = BanMotLuot();
        if (soTiaVuaBan > 0)
        {
            nhip = NhipLuot;      // 0,4 giay la khoang cach GIUA HAI LUOT BAN THAT
            daBan++;
            SoLuotDaBan++;
        }
        else
        {
            nhip = 0.15f;         // quanh day khong co ai: ngo lai som, khong tinh la mot luot
        }
    }

    /// <summary>Mot luot: toi da 5 tia, MOI KE DICH MOT TIA, chon 5 ke gan qua cau nhat trong 9 m.
    /// Tra ve SO TIA da ban ra - 0 nghia la quanh day khong co ai, luot nay khong tinh.</summary>
    int BanMotLuot()
    {
        mucTieu.Clear();
        int n = Physics.OverlapSphereNonAlloc(transform.position, banKinhBan, bo, damageMask,
                                              QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = bo[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead) continue;
            if (boQua != null && d == boQua) continue;
            if (!mucTieu.Contains(d)) mucTieu.Add(d);
        }

        // Gan truoc, xa sau - it hon 5 ke thi ban it tia hon (nguoi dung chon "thua thi bo")
        mucTieu.Sort(SoSanhTheoKhoangCach);

        int soTia = Mathf.Min(SoTiaMoiLuot, mucTieu.Count);
        int daBanRa = 0;
        for (int i = 0; i < soTia; i++)
        {
            var d = mucTieu[i];
            if (d == null || d.IsDead) continue;

            Vector3 den = d.transform.position + Vector3.up * 1.0f;
            VfxFactory.TiaCauDien(transform.position, den);
            VfxFactory.ChamTiaDien(den);

            // GhiKeDanh TRUOC TakeDamage - khong thi ke giet vo danh, mat kinh nghiem (menu 61)
            d.GhiKeDanh(boQua);
            d.TakeDamage(satThuongTia, DamageType.Lightning, den);
            SoTiaDaBan++;
            SoLanTrung++;
            daBanRa++;

            if (!d.IsDead && Random.value < XacSuatChoang)
                StunnedEffect.Apply(d, giayChoang);
        }
        return daBanRa;
    }

    int SoSanhTheoKhoangCach(Damageable a, Damageable b)
    {
        if (a == null) return 1;
        if (b == null) return -1;
        float da = (a.transform.position - transform.position).sqrMagnitude;
        float db = (b.transform.position - transform.position).sqrMagnitude;
        return da.CompareTo(db);
    }

    void OnDestroy()
    {
        if (hinh != null) Destroy(hinh);
    }
}
