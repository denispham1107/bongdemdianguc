using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TRANG THAI "TANG HINH" (ky nang 12, them 18/09/2026).
///
/// Nguoi dung: than nguoi trong suot nhu Dark Templar (anh gui) - QUAI KHONG THAY va khong danh; nguoi choi khac chi thay
/// DUONG NET khi minh DI CHUYEN, minh dung yen thi ho khong thay gi; mien MOI hieu ung; di nhanh hon 20%; keo 20 giay;
/// don danh dau tien bang mot ky nang gay sat thuong an GAP DOI sat thuong va lam tan tang hinh; hoi chieu 30 giay.
///
/// Hinh: thay TOAN BO vat lieu cua model bang <see cref="Mats.TangHinhShader"/> (giu ban goc de tra lai) - khac
/// <see cref="FrozenEffect"/> (chi PHU THEM mot lop vo bang). Phu them thi than nguoi van hien nguyen, khong the "trong suot".
/// </summary>
[DisallowMultipleComponent]
public class TangHinh : MonoBehaviour
{
    public const float ThoiGian = 20f;
    public const float HoiChieu = 30f;
    public const float HeSoToc = 1.2f;
    /// <summary>Don dau tien nhan doi sat thuong (nguoi dung chon "gap 2 lan").</summary>
    public const float NhanDonDau = 2f;
    /// <summary>Dung yen bao lau thi mo han tren may NGUOI KHAC (nguoi dung chon 0,4 giay).</summary>
    public const float GiayMoHan = 0.4f;
    /// <summary>Do hien duong net khi dang di chuyen.</summary>
    public const float MucKhiDi = 1f;
    /// <summary>Cham hon nguong nay coi nhu dang dung yen (m/s).</summary>
    public const float NguongDung = 0.15f;
    /// <summary>Con bao nhieu giay thi bat dau NHAP NHAY bao sap hien hinh (nguoi dung xin 18/09/2026).</summary>
    public const float GiayNhapNhay = 2f;
    /// <summary>Mot nhip nhap nhay: nua dau mo han, nua sau hien lai.</summary>
    public const float NhipNhapNhay = 0.22f;
    /// <summary>Do hien o nua "toi" cua nhip nhap nhay.</summary>
    public const float MucLucNhay = 0.12f;

    public float conLai = ThoiGian;

    /// <summary>Con giu cu nhan doi sat thuong khong (chua danh don nao).</summary>
    public bool conDonDau = true;

    /// <summary>Ban sao cua nguoi choi khac: chi song theo goi tin (khong tu dem gio).</summary>
    public bool tuMang;

    /// <summary>Dem cho phep thu (menu 73).</summary>
    public static int SoLanBat, SoLanDonDau, SoLanChanHieuUng;

    /// <summary>Do hien dang dat cho shader (_Amount) - phep thu menu 73 doc truong nay.</summary>
    public float mucHien = 1f;
    /// <summary>Dang trong 2 giay nhap nhay bao sap hien hinh khong (chi dung tren may CUA MINH).</summary>
    public bool dangNhapNhay;

    readonly List<Renderer> daDoi = new List<Renderer>();
    readonly List<Material[]> vatLieuGoc = new List<Material[]>();
    Material matTang;
    float muc = 1f;
    Vector3 choTruoc;
    bool laCuaMinh;

    /// <summary>Vat nay co dang tang hinh khong.</summary>
    public static bool Dang(Component c)
    {
        if (c == null) return false;
        var t = c.GetComponent<TangHinh>();
        return t != null && t.conLai > 0f;
    }

    /// <summary>Hieu ung (dong bang, choang, nga, hat tung, chay...) co bi chan khong. Dem lai cho phep thu.</summary>
    public static bool ChanHieuUng(Component c)
    {
        if (!Dang(c)) return false;
        SoLanChanHieuUng++;
        return true;
    }

    /// <summary>Bat tang hinh: xoa sach hieu ung dang dinh roi dem gio.</summary>
    public static TangHinh Bat(Damageable d, float giay)
    {
        if (d == null || d.IsDead) return null;
        var t = d.GetComponent<TangHinh>();
        // Component vua goi Tat() con song toi cuoi khung hinh: bat lai ngay thi GetComponent tra ve cai SAP BI HUY,
        // dat conLai cho no cung vo ich (menu 73 do 18/09/2026: dat 1,2 giay ma tan sau 0,02 giay).
        if (t != null && t.conLai <= 0f) { Object.DestroyImmediate(t); t = null; }
        // Component MOI: dat DUNG so giay (truong conLai khai bao san 20 s, lay Max thi dat 1,2 giay khong an gi - menu 73 18/09/2026)
        bool moi = t == null;
        if (moi) t = d.gameObject.AddComponent<TangHinh>();
        t.conLai = moi ? giay : Mathf.Max(t.conLai, giay);
        t.conDonDau = true;
        SoLanBat++;
        XoaHieuUngDangDinh(d.gameObject);
        return t;
    }

    /// <summary>Ban sao qua mang: giu song them mot khoang moi goi tin (nhu cac hieu ung khac).</summary>
    public static void ApTuMang(Damageable d, float giuSong)
    {
        if (d == null || d.IsDead) return;
        var t = d.GetComponent<TangHinh>();
        if (t == null)
        {
            t = d.gameObject.AddComponent<TangHinh>();
            t.conDonDau = false;
        }
        t.tuMang = true;
        t.conLai = Mathf.Max(t.conLai, giuSong);
    }

    /// <summary>Tan tang hinh ngay (danh don dau, hoac het gio).</summary>
    public void Tat()
    {
        conLai = 0f;
        TraVatLieu();
        Destroy(this);
    }

    public static void XoaHieuUngDangDinh(GameObject go)
    {
        if (go == null) return;
        var f = go.GetComponent<FrozenEffect>(); if (f != null) f.Thaw();
        var st = go.GetComponent<StunnedEffect>(); if (st != null) Destroy(st);
        var ng = go.GetComponent<BiDanhNga>(); if (ng != null) Destroy(ng);
        var ht = go.GetComponent<BiHatTung>(); if (ht != null) Destroy(ht);
        var ch = go.GetComponent<BurningEffect>(); if (ch != null) Destroy(ch);
    }

    void Start()
    {
        var pc = GetComponent<PlayerController>();
        laCuaMinh = pc != null && pc.tuDocInput;
        choTruoc = transform.position;

        matTang = new Material(Mats.TangHinhShader);
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r == null || !r.enabled) continue;
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;
            // Bo qua thu da trong suot (hieu ung, vom khieng) - thay vat lieu cua chung chi lam hinh la
            bool trongSuot = false;
            for (int k = 0; k < mats.Length; k++) if (mats[k] == null || mats[k].renderQueue >= 3000) trongSuot = true;
            if (trongSuot) continue;

            daDoi.Add(r);
            vatLieuGoc.Add(mats);
            var thay = new Material[mats.Length];
            for (int k = 0; k < mats.Length; k++) thay[k] = matTang;
            r.materials = thay;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (!tuMang) conLai -= dt;
        else conLai -= dt;      // ban sao cung dem lui; moi goi tin lai day len (ApTuMang)

        float toc = dt > 0f ? (transform.position - choTruoc).magnitude / dt : 0f;
        choTruoc = transform.position;

        // MINH luon thay than minh mo mo (con dieu khien duoc); NGUOI KHAC chi thay khi minh di chuyen,
        // dung yen thi mo dan trong GiayMoHan roi bien mat han (nguoi dung chon 18/09/2026).
        float dich = laCuaMinh || toc > NguongDung ? MucKhiDi : 0f;
        muc = Mathf.MoveTowards(muc, dich, dt / Mathf.Max(0.01f, GiayMoHan));

        // HAI GIAY CUOI: than minh NHAP NHAY de biet sap hien hinh (nguoi dung xin 18/09/2026).
        // CHI tren may cua MINH: ban sao cua nguoi khac (tuMang) khong dem duoc gio that - moi goi tin lai day
        // conLai len, nen no se nhap nhay suot; va nguoi dung muon nguoi choi khac KHONG thay dau hieu nay.
        dangNhapNhay = laCuaMinh && !tuMang && conLai > 0f && conLai <= GiayNhapNhay;
        float hien = muc;
        if (dangNhapNhay && Mathf.Repeat(conLai, NhipNhapNhay) < NhipNhapNhay * 0.5f) hien = muc * MucLucNhay;
        mucHien = hien;

        if (matTang != null) matTang.SetFloat("_Amount", hien);
        // Bat/tat renderer van theo "muc" chu khong theo "hien": nhap nhay bang do sang, khong tat han hinh.
        for (int i = 0; i < daDoi.Count; i++)
            if (daDoi[i] != null) daDoi[i].enabled = muc > 0.01f;

        if (conLai <= 0f) Tat();
    }

    void OnDestroy() { TraVatLieu(); }

    void TraVatLieu()
    {
        for (int i = 0; i < daDoi.Count; i++)
        {
            if (daDoi[i] == null) continue;
            daDoi[i].materials = vatLieuGoc[i];
            daDoi[i].enabled = true;
            daDoi[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        daDoi.Clear();
        vatLieuGoc.Clear();
        if (matTang != null) { Destroy(matTang); matTang = null; }
    }
}
