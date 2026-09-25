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
    /// <summary>Hoi chieu (nguoi dung 26/09/2026: 7 -> 5,5 giay).</summary>
    public const float HoiChieu = 5.5f;
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
    /// <summary>Mua lam uot: bao lau quet vung mot lan (con dung trong mua la con lam moi BiUot.GiayMacDinh).</summary>
    public const float NhipLamUot = 0.2f;
    /// <summary>
    /// Do cao day may (chan tia set) tren mat dat. 7 m chu khong 10: may quay "3D tu do" (sau lung 7,5 m, cui 22 do, goc nhin
    /// 55 do) o cho cach nhan vat 10 - 12 m chi thay toi ~5,5 m - may o 10 m nam han tren mep man hinh (anh ban ngay dau tien).
    /// </summary>
    public const float CaoMay = 7f;
    /// <summary>May ket lai bao lau truoc tia dau.</summary>
    public const float GiayTuMay = 0.35f;
    /// <summary>Khoang cach giua hai tia: 5 giay / 20 tia.</summary>
    public const float NhipTia = ThoiGian / SoTia;
    /// <summary>
    /// MAY TU BAY (nguoi dung 26/09/2026): het 5 giay dung yen, may KHONG tan ma bay theo huong ngau nhien them 4 giay, vua bay
    /// vua mua (lam uot, dap lo) vua giat set - chon: 1,5 m/s (~6 m), set nhu cu 4 tia/giay (them 16 tia, tong 36).
    /// </summary>
    public const float ThoiGianBay = 4f, TocDoBay = 1.5f;
    public const int SoTiaKhiBay = 16;
    public const int SoTiaTong = SoTia + SoTiaKhiBay;
    /// <summary>Het giay nay (tinh tu luc tung) thi may tan.</summary>
    public const float GiayHetMay = GiayTuMay + ThoiGian + ThoiGianBay;

    public LayerMask damageMask;
    public Damageable boQua;
    public float satThuong = SatThuongGoc;
    public float giayNga = GiayNga;

    int daDanh;
    float dongHo, nhipNgang;
    GameObject hinh;
    LoeSangMay roiSang;

    static readonly Collider[] bo = new Collider[64];
    readonly List<Damageable> ds = new List<Damageable>();

    // Dem cho phep thu (menu 83)
    public static int SoTiaDaDanh, SoLanTrung, SoLanNga, SoTiaNhamKeDich, SoLanTrungUot, SoLoDapTat;
    /// <summary>Cho tia cuoi cung vua giang (phep thu doc: tia khi bay phai roi trong vung quanh cho MAY DANG O).</summary>
    public static Vector3 DiemTiaCuoi;
    float nhipUot;
    Vector3 huongBay;
    /// <summary>Dong ho cua may (giay tu luc sinh) - phep thu doc.</summary>
    public float DongHo { get { return dongHo; } }

    /// <summary>
    /// Huong bay "ngau nhien" nhung GIONG NHAU tren moi may: bam tu toa do ngam DA NEN nhu goi tin (GoiTin.NenToaDo) - may tung
    /// dung diem that, may nhan dung diem da nen, hai ben nen ra cung mot so nguyen. Random.value moi may mot khac thi moi nguoi
    /// thay may bay mot nga, set danh mot noi.
    /// </summary>
    public static Vector3 HuongBay(Vector3 diemNgam)
    {
        unchecked
        {
            uint h = (uint)(GoiTin.NenToaDo(diemNgam.x) * 73856093) ^ (uint)(GoiTin.NenToaDo(diemNgam.z) * 19349663);
            h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
            float goc = (h % 3600u) / 3600f * Mathf.PI * 2f;
            return new Vector3(Mathf.Cos(goc), 0f, Mathf.Sin(goc));
        }
    }

    public static MayGiong Spawn(Vector3 tam, LayerMask damageMask, Damageable boQua, float heSoSatThuong = 1f, float themGiay = 0f)
    {
        Vector3 huong = HuongBay(tam);          // tinh TRUOC khi doi y (chi dung x, z)
        tam.y = VfxFactory.GroundY(tam);
        var go = new GameObject("MayGiong");
        go.transform.position = tam;
        var mg = go.AddComponent<MayGiong>();
        mg.huongBay = huong;
        mg.damageMask = damageMask;
        mg.boQua = boQua;
        mg.satThuong = SatThuongGoc * heSoSatThuong;
        mg.giayNga = GiayNga + themGiay;
        return mg;
    }

    void Start()
    {
        hinh = VfxFactory.MayGiongHinh(transform.position, BanKinh, CaoMay, GiayHetMay + 0.3f);
        roiSang = hinh != null ? hinh.GetComponent<LoeSangMay>() : null;
        CameraShake.Shake(0.2f, 0.05f);
        nhipNgang = 0.2f;
    }

    void Update()
    {
        dongHo += Time.deltaTime;

        // MAY TU BAY 4 giay sau pha dung yen - bam mat dat CHI lop Ground (GroundY gom ca mai nha: may treo len mai nha mo)
        if (dongHo > GiayTuMay + ThoiGian && dongHo <= GiayHetMay)
        {
            Vector3 p = transform.position + huongBay * (TocDoBay * Time.deltaTime);
            p.y = GioLoc.MatDatY(p, p.y);
            transform.position = p;
            if (hinh != null) hinh.transform.position = p;
        }

        // MUA lam UOT (nguoi dung 25/09/2026 khuya): suot luc mua (GiayTuMay .. het may) moi 0,2 s quet vung 6 m.
        // Lam TRUOC khi danh tia trong cung khung: tia dau roi dung luc mua bat dau, ke dung trong vung phai kip uot.
        if (dongHo >= GiayTuMay && dongHo <= GiayHetMay)
        {
            nhipUot -= Time.deltaTime;
            if (nhipUot <= 0f) { nhipUot = NhipLamUot; LamUotTrongVung(); }
        }

        // Tia thu i giang o moc GiayTuMay + i x 0,25 s - dem theo DONG HO, khong theo khung: khung giat van du 20 tia dung gio
        while (daDanh < SoTiaTong && dongHo >= GiayTuMay + daDanh * NhipTia)
        {
            DanhMotTia();
            daDanh++;
        }

        // Tia ngang lach tach TRONG may (chi hinh, nhu anh mau)
        nhipNgang -= Time.deltaTime;
        if (nhipNgang <= 0f && dongHo < GiayHetMay)
        {
            nhipNgang = Random.Range(0.18f, 0.34f);
            VfxFactory.TiaNgangTrongMay(transform.position + Vector3.up * CaoMay, BanKinh);
        }

        if (daDanh >= SoTiaTong && dongHo >= GiayHetMay) Destroy(gameObject);
    }

    /// <summary>Bi xoa GIUA CHUNG (khong phai het gio tu nhien) thi xoa luon phan hinh - hinh la vat rieng, khong thi may + mua
    /// con roi tiep toi het gio o cho cu (phep thu menu 83 muc K xoa may som, mua roi quanh nhan vat trong anh sau).</summary>
    void OnDestroy()
    {
        if (hinh != null && dongHo < GiayHetMay - 0.05f) Destroy(hinh);
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
        DiemTiaCuoi = cho;

        Vector2 lech = Random.insideUnitCircle * (BanKinh * 0.45f);
        Vector3 tu = transform.position + new Vector3(lech.x, CaoMay, lech.y);
        VfxFactory.TiaMayGiong(tu, cho, muc != null ? muc.transform : null);
        // Cho set cham dat CHAY XEM + boc khoi, NAM DEN vat quanh do - dung y nhu tia Sam set (LightningStrike.Strike,
        // nguoi dung 25/09/2026 khuya "giong nhu cac tia set trong skill Sam set")
        VfxFactory.SetChayDen(cho, BanKinhTia * 0.72f, 0.45f);
        VfxFactory.NamChuongNgai(cho, BanKinhTia, VfxFactory.LopChuongNgai);
        // May giong DEN (nguoi dung 25/09/2026 toi): set roi sang MANG may cho tia phat ra + ca dam loe nhe
        VfxFactory.MangSangTrongMay(tu + Vector3.up * 0.6f);
        if (roiSang != null) roiSang.Chop(Random.Range(0.75f, 1f));
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
            // Ke dang UOT an them 50% (BiUot - nguoi dung chon ca tia May giong)
            float heSoUot = BiUot.HeSo(d);
            if (heSoUot > 1f) SoLanTrungUot++;
            d.TakeDamage(satThuong * heSoUot, DamageType.Lightning, d.transform.position + Vector3.up);
            SoLanTrung++;
            ChayDenToanThan.Apply(d, GiayChayDen);
        }
        // 45% hat nga - moi ke gieo rieng, bo qua nguoi tung / ke da chet / ke co khien (ThienThach.GieoDanhNga)
        SoLanNga += ThienThach.GieoDanhNga(cho + Vector3.up, BanKinhTia, damageMask, boQua, XacSuatNga, giayNga);
    }

    /// <summary>Moi doi thu (tru nguoi tung) dung trong vung 6 m (tinh theo mat ngang) bi UOT them BiUot.GiayMacDinh giay.</summary>
    void LamUotTrongVung()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up, BanKinh + 2f, bo, damageMask, QueryTriggerInteraction.Collide);
        ds.Clear();
        for (int i = 0; i < n; i++)
        {
            var d = bo[i].GetComponentInParent<Damageable>();
            if (d == null || d.IsDead || ds.Contains(d)) continue;
            if (boQua != null && d == boQua) continue;
            Vector3 v = d.transform.position - transform.position; v.y = 0f;
            if (v.magnitude > BanKinh) continue;
            ds.Add(d);
            BiUot.Apply(d, BiUot.GiayMacDinh);
        }
        // Mua roi trung LO LUA thi dap tat, 30 giay sau chay lai - nhu Gio loc / Qua cau bang (nguoi dung 26/09/2026)
        SoLoDapTat += LoLuaDa.DapTatTrongVung(transform.position + Vector3.up, BanKinh, GioLoc.GiayLoChayLai);
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
