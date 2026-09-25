using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TRANG THAI "TANG HINH" (ky nang 12, them 18/09/2026).
///
/// Nguoi dung: than nguoi trong suot nhu Dark Templar (anh gui) - QUAI KHONG THAY va khong danh; nguoi choi khac chi thay
/// DUONG NET khi minh DI CHUYEN, minh dung yen thi ho khong thay gi; mien MOI hieu ung; di nhanh hon 20%; keo 20 giay;
/// don danh dau tien bang mot ky nang gay sat thuong an GAP DOI sat thuong va lam tan tang hinh; hoi chieu 30 giay.
///
/// 26/09/2026 (nguoi dung, kem anh vong phep xanh): keo 90 GIAY; KET THUC (het gio HOAC tan do don dau - chon "ca hai") thi
/// NO VONG PHEP duoi chan (<see cref="VfxFactory.VongPhepTangHinh"/>, anh Blender) va gay sat thuong MOT LAN cho moi doi thu
/// trong 5 m: 100 + 10% MAU TOI DA (chon "mau toi da"), moi cap them 4% (cap 5: 26%). Hoi chieu: dang tang hinh khong bam lai
/// duoc, HIEN HINH XONG moi dem 10 giay (nguoi dung doi tu 30). He BANG (nhom cua Tang hinh): Khang Bang giam; loai sat thuong
/// Physical de khang bang +-25% cua quai khong lam lech con so "100 + 10%".
/// Qua mang: phep Tang hinh duoc phat lai tren may kia kem CAP nguoi tung (capKyNang) -> ban sao cung het gio / tan do don dau
/// -> vong no tren moi may, moi may tinh sat thuong theo luat trong tai nhu cac phep khac.
///
/// Hinh: thay TOAN BO vat lieu cua model bang <see cref="Mats.TangHinhShader"/> (giu ban goc de tra lai) - khac
/// <see cref="FrozenEffect"/> (chi PHU THEM mot lop vo bang). Phu them thi than nguoi van hien nguyen, khong the "trong suot".
/// </summary>
[DisallowMultipleComponent]
public class TangHinh : MonoBehaviour
{
    public const float ThoiGian = 90f;
    /// <summary>Hoi chieu - dem tu luc HIEN HINH (PlayerController giu day suot luc tang hinh).</summary>
    public const float HoiChieu = 10f;
    /// <summary>Vong phep luc het tang hinh: ban kinh, sat thuong goc, % mau toi da o cap 1, % them moi cap.</summary>
    public const float BanKinhVong = 5f, SatThuongVongGoc = 100f, PhanTramMauCap1 = 0.10f, PhanTramMoiCap = 0.04f;

    /// <summary>Sat thuong vong phep len mot doi thu co <paramref name="mauToiDa"/> mau, cap ky nang <paramref name="cap"/>.</summary>
    public static float SatThuongVong(float mauToiDa, int cap)
    {
        cap = Mathf.Clamp(cap, 1, CapDo.CapKyNangToiDa);
        return SatThuongVongGoc + mauToiDa * (PhanTramMauCap1 + PhanTramMoiCap * (cap - 1));
    }

    /// <summary>Cap Tang hinh cua NGUOI TUNG (ban sao lay tu goi phep) - quyet dinh % mau cua vong phep.</summary>
    public int capKyNang = 1;
    bool daNoVong;
    public static int SoLanNoVong, SoKeTrungVong;
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
        float lucNo;
        if (lucNoVong.TryGetValue(d, out lucNo) && Time.time - lucNo < GiayBoQuaBitCu) return;   // bit cu tre sau khi vong da no
        var t = d.GetComponent<TangHinh>();
        if (t == null)
        {
            t = d.gameObject.AddComponent<TangHinh>();
            t.conDonDau = false;
            // Component MOI mang conLai MAC DINH = ThoiGian (90 s): lay Max voi giuSong la ban sao tang hinh them 90 giay sau khi
            // nguoi ay da hien hinh (truoc 26/09/2026 la 20 s) - menu 73 muc W bat duoc. Dat DUNG giuSong.
            t.conLai = giuSong;
        }
        t.tuMang = true;
        t.conLai = Mathf.Max(t.conLai, giuSong);
    }

    /// <summary>Tan tang hinh ngay (danh don dau, hoac het gio) - no vong phep.</summary>
    public void Tat()
    {
        conLai = 0f;
        TraVatLieu();
        NoVong();
        Destroy(this);
    }

    /// <summary>
    /// VONG PHEP luc het tang hinh: hinh + sat thuong MOT LAN cho moi doi thu (tru chinh minh) trong BanKinhVong (tinh mat ngang).
    /// Chi goi tu Tat() (het gio / don dau) - chet hay roi tran (OnDestroy) thi khong no.
    /// </summary>
    void NoVong()
    {
        if (daNoVong) return;
        daNoVong = true;
        var toi = GetComponent<Damageable>();
        if (toi == null || toi.IsDead) return;
        var pc = GetComponent<PlayerController>();
        LayerMask mask = pc != null ? pc.MatNaKeThu : LayerMask.GetMask("Enemy");
        Vector3 tam = transform.position;
        SoLanNoVong++;
        lucNoVong[toi] = Time.time;
        VfxFactory.VongPhepTangHinh(tam, BanKinhVong);
        CameraShake.Shake(0.18f, 0.06f);

        int n = Physics.OverlapSphereNonAlloc(tam + Vector3.up, BanKinhVong + 2f, boVong, mask, QueryTriggerInteraction.Collide);
        dsVong.Clear();
        for (int i = 0; i < n; i++)
        {
            var d = boVong[i].GetComponentInParent<Damageable>();
            if (d == null || d == toi || d.IsDead || dsVong.Contains(d)) continue;
            Vector3 v = d.transform.position - tam; v.y = 0f;
            if (v.magnitude > BanKinhVong) continue;
            dsVong.Add(d);
            d.GhiKeDanh(toi, HeSat.Bang);          // TRUOC TakeDamage (kinh nghiem, bang diem); he BANG: Khang Bang giam
            d.TakeDamage(SatThuongVong(d.maxHealth, capKyNang), DamageType.Physical, d.transform.position + Vector3.up);
            SoKeTrungVong++;
        }
    }
    /// <summary>Luc vong phep vua no cua tung nguoi - ban sao bo qua bit "dang tang hinh" con tre trong goi tin ngay sau do,
    /// khong thi no tao lai tang hinh roi het gio them lan nua: VONG PHEP THU HAI tren may kia.</summary>
    static readonly Dictionary<Damageable, float> lucNoVong = new Dictionary<Damageable, float>();
    public const float GiayBoQuaBitCu = 1.5f;
    static readonly Collider[] boVong = new Collider[64];
    static readonly List<Damageable> dsVong = new List<Damageable>();

    public static void XoaHieuUngDangDinh(GameObject go)
    {
        if (go == null) return;
        var f = go.GetComponent<FrozenEffect>(); if (f != null) f.Thaw();
        var st = go.GetComponent<StunnedEffect>(); if (st != null) Destroy(st);
        var ng = go.GetComponent<BiDanhNga>(); if (ng != null) Destroy(ng);
        var ht = go.GetComponent<BiHatTung>(); if (ht != null) Destroy(ht);
        var ch = go.GetComponent<BurningEffect>(); if (ch != null) Destroy(ch);
        var uot = go.GetComponent<BiUot>(); if (uot != null) Destroy(uot);
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
