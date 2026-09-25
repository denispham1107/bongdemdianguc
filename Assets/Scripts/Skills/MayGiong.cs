using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG 21 - MAY GIONG (nhom PHONG, nguoi dung 25/09/2026, kem anh mau: dam may trang sang tu ben trong, tia set trang
/// xanh re nhanh chang chit giang xuong).
///
/// Goi mot vung MAY GIONG xuong cho nguoi choi chon (tam = tam Sam set, 12 m). Trong 5 GIAY giang du 20 TIA SET (0,25 s mot
/// tia), ban kinh vung 6 m, tia UU TIEN ke dich dung trong vung (65%, nhu Sam set) con lai danh ngau nhien phu kin vung.
/// Moi tia: 125 sat thuong (cap 1) trong vung nho 2 m quanh cho danh, 45% HAT NGA 0,85 giay (BiDanhNga, khien chan),
/// ke bi trung CHAY DEN TOAN THAN 3 giay (chi hinh - ChayDenToanThan). 50 nang luong, niem 0,5 giay, hoi chieu 7 giay.
/// Nguoi dung chon: khong can dieu kien mo khoa; chay den chi la hinh, 3 giay.
///
/// Tia set thuoc he PHONG (GhiKeDanh(boQua, HeSat.Phong)) - Khang Phong chan, nhu tia set trong Loc xoay (nguoi dung chot
/// 19/09/2026 "ky nang nhom Phong thi Khang Phong"). Cap ky nang di kem goi tung phep: satThuong, giayNga cua NGUOI TUNG.
/// Hinh (Blender MCP, CongCu/Blender/may_giong.blend -> Resources/KyNang/MayGiong): xem VfxMayGiong.cs.
/// </summary>
public class MayGiong : MonoBehaviour
{
    public const float NangLuong = 50f;
    public const float HoiChieu = 7f;
    public const float NiemChu = 0.5f;
    public const float BanKinh = 6f;
    public const float ThoiGian = 5f;
    public const int SoTia = 20;
    public const float SatThuongGoc = 125f;
    public const float BanKinhTia = 2f;
    public const float XacSuatNga = 0.45f;
    public const float GiayNga = 0.85f;
    public const float GiayChayDen = 3f;
    public const float NhamKeDich = 0.65f;
    /// <summary>
    /// Do cao day may (chan tia set) tren mat dat. 7 m chu khong 10: may quay "3D tu do" (sau lung 7,5 m, cui 22 do, goc nhin
    /// 55 do) o cho cach nhan vat 10 - 12 m chi thay toi ~5,5 m - may o 10 m nam han tren mep man hinh (anh ban ngay dau tien).
    /// </summary>
    public const float CaoMay = 7f;
    /// <summary>May ket lai bao lau truoc tia dau.</summary>
    public const float GiayTuMay = 0.35f;
    /// <summary>Khoang cach giua hai tia: 5 giay / 20 tia.</summary>
    public const float NhipTia = ThoiGian / SoTia;

    public LayerMask damageMask;
    public Damageable boQua;
    public float satThuong = SatThuongGoc;
    public float giayNga = GiayNga;

    int daDanh;
    float dongHo, nhipNgang;
    GameObject hinh;

    static readonly Collider[] bo = new Collider[64];
    readonly List<Damageable> ds = new List<Damageable>();

    // Dem cho phep thu (menu 83)
    public static int SoTiaDaDanh, SoLanTrung, SoLanNga, SoTiaNhamKeDich;

    public static MayGiong Spawn(Vector3 tam, LayerMask damageMask, Damageable boQua, float heSoSatThuong = 1f, float themGiay = 0f)
    {
        tam.y = VfxFactory.GroundY(tam);
        var go = new GameObject("MayGiong");
        go.transform.position = tam;
        var mg = go.AddComponent<MayGiong>();
        mg.damageMask = damageMask;
        mg.boQua = boQua;
        mg.satThuong = SatThuongGoc * heSoSatThuong;
        mg.giayNga = GiayNga + themGiay;
        return mg;
    }

    void Start()
    {
        hinh = VfxFactory.MayGiongHinh(transform.position, BanKinh, CaoMay, GiayTuMay + ThoiGian + 0.3f);
        CameraShake.Shake(0.2f, 0.05f);
        nhipNgang = 0.2f;
    }

    void Update()
    {
        dongHo += Time.deltaTime;

        // Tia thu i giang o moc GiayTuMay + i x 0,25 s - dem theo DONG HO, khong theo khung: khung giat van du 20 tia dung gio
        while (daDanh < SoTia && dongHo >= GiayTuMay + daDanh * NhipTia)
        {
            DanhMotTia();
            daDanh++;
        }

        // Tia ngang lach tach TRONG may (chi hinh, nhu anh mau)
        nhipNgang -= Time.deltaTime;
        if (nhipNgang <= 0f && dongHo < GiayTuMay + ThoiGian)
        {
            nhipNgang = Random.Range(0.18f, 0.34f);
            VfxFactory.TiaNgangTrongMay(transform.position + Vector3.up * CaoMay, BanKinh);
        }

        if (daDanh >= SoTia && dongHo >= GiayTuMay + ThoiGian) Destroy(gameObject);
    }

    /// <summary>Mot tia: chon cho (ke dich hoac ngau nhien), ve tia tu day may, sat thuong + hat nga + chay den trong 2 m.</summary>
    void DanhMotTia()
    {
        Damageable muc = Random.value < NhamKeDich ? ChonKeDich() : null;
        Vector3 cho;
        if (muc != null) { cho = muc.transform.position; SoTiaNhamKeDich++; }
        else
        {
            Vector2 r = Random.insideUnitCircle * BanKinh;
            cho = transform.position + new Vector3(r.x, 0f, r.y);
        }
        cho.y = VfxFactory.GroundY(cho);

        Vector2 lech = Random.insideUnitCircle * (BanKinh * 0.45f);
        Vector3 tu = transform.position + new Vector3(lech.x, CaoMay, lech.y);
        VfxFactory.TiaMayGiong(tu, cho, muc != null ? muc.transform : null);
        SoTiaDaDanh++;

        ds.Clear();
        int n = Physics.OverlapSphereNonAlloc(cho + Vector3.up, BanKinhTia, bo, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = bo[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || ds.Contains(d)) continue;
            if (boQua != null && d == boQua) continue;
            ds.Add(d);
        }
        for (int i = 0; i < ds.Count; i++)
        {
            var d = ds[i];
            // GhiKeDanh TRUOC TakeDamage (kinh nghiem, bang diem - menu 61); he PHONG: Khang Phong chan
            d.GhiKeDanh(boQua, HeSat.Phong);
            d.TakeDamage(satThuong, DamageType.Lightning, d.transform.position + Vector3.up);
            SoLanTrung++;
            ChayDenToanThan.Apply(d, GiayChayDen);
        }
        // 45% hat nga - moi ke gieo rieng, bo qua nguoi tung / ke da chet / ke co khien (ThienThach.GieoDanhNga)
        SoLanNga += ThienThach.GieoDanhNga(cho + Vector3.up, BanKinhTia, damageMask, boQua, XacSuatNga, giayNga);
    }

    /// <summary>Mot ke dich con song ngau nhien trong vung may (tru nguoi tung).</summary>
    Damageable ChonKeDich()
    {
        ds.Clear();
        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up, BanKinh, bo, damageMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = bo[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || ds.Contains(d)) continue;
            if (boQua != null && d == boQua) continue;
            Vector3 v = d.transform.position - transform.position; v.y = 0f;
            if (v.magnitude > BanKinh) continue;
            ds.Add(d);
        }
        return ds.Count > 0 ? ds[Random.Range(0, ds.Count)] : null;
    }
}
