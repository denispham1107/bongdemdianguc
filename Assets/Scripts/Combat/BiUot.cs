using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BI UOT (nguoi dung 25/09/2026, khuya): mua cua May giong lam MOI doi thu dung trong vung 6 m bi uot - 100%, khong sat
/// thuong. Con dung trong mua la con lam moi; ra khoi vung (hoac may tan) thi con uot them <see cref="GiayMacDinh"/> giay.
/// Ke dang uot chiu them 50% sat thuong (<see cref="HeSoSetDien"/>) tu cac ky nang nhom SET (Giut set, Sam set, Qua cau dien)
/// VA tu 20 tia cua chinh May giong (nguoi dung chon "nhom SET + tia cua May giong"). Tia set cua QUAI khong duoc cong.
///
/// Hinh (nguoi dung chon "nho nuoc + bong uot + chu"): giot nuoc nho tu nguoi xuong (anh Blender NhoNuoc), phu them mot lop
/// UOT BONG (anh Blender UotBong: nen toi xanh + vet nuoc chay sang) len MOI MeshRenderer + SkinnedMeshRenderer - go ra chi bo
/// DUNG lop cua minh nhu ChayDenToanThan, nen chong voi vo bang / chay den khong mat lop nao; chu "BỊ ƯỚT" bay len khi BAT DAU uot.
///
/// Qua mang: KHONG can bit goi tin. Mua la mot phan cua dam may, ma dam may duoc phat lai tren moi may (TungPhepTheoMang), nen
/// moi may tu thay ai dang dung trong mua. Sat thuong len nguoi choi do may NAN NHAN tinh (mauDoMayKhacQuyet), len quai do may
/// chu phong - ca hai deu doc trang thai uot cua chinh no. Tang hinh chan (TangHinh.ChanHieuUng) nhu moi hieu ung khac.
/// </summary>
public class BiUot : MonoBehaviour
{
    public const float GiayMacDinh = 5f;
    public const float HeSoSetDien = 1.5f;
    public static readonly Color MauChuUot = new Color(0.45f, 0.72f, 1f, 1f);
    const float GiayHien = 0.2f, GiayPhai = 0.6f;
    const string AnhUot = "KyNang/MayGiong/UotBong";

    public float conLai;
    float tuoi;
    Material lop;
    readonly List<Renderer> daPhu = new List<Renderer>();
    GameObject nho;

    static Texture2D anhUot;
    public static int SoLanApDung, SoLanBaoChu;

    /// <summary>Do dam lop bong uot hien tai (0..1) - phep thu doc.</summary>
    public float DoDam { get; private set; }
    public int SoRendererDaPhu { get { return daPhu.Count; } }
    public bool CoNhoNuoc { get { return nho != null; } }

    public static BiUot Apply(Damageable d, float giay)
    {
        if (d == null || d.IsDead) return null;
        if (TangHinh.ChanHieuUng(d)) return null;
        var u = d.GetComponent<BiUot>();
        if (u == null)
        {
            u = d.gameObject.AddComponent<BiUot>();
            // Dat ngay - AddComponent mang gia tri mac dinh (memory addcomponent-mang-gia-tri-mac-dinh)
            u.conLai = giay;
            SoLanBaoChu++;
            DamagePopup.SpawnText(d.transform.position + Vector3.up * 2.1f, "BỊ ƯỚT", MauChuUot);
        }
        else if (giay > u.conLai) u.conLai = giay;
        SoLanApDung++;
        return u;
    }

    public static bool DangUot(Damageable d)
    {
        if (d == null) return false;
        var u = d.GetComponent<BiUot>();
        return u != null && u.conLai > 0f;
    }

    /// <summary>He so nhan sat thuong cho don nhom SET / tia May giong: 1,5 neu dang uot, khong thi 1.</summary>
    public static float HeSo(Damageable d)
    {
        return DangUot(d) ? HeSoSetDien : 1f;
    }

    void Start()
    {
        if (anhUot == null) anhUot = Resources.Load<Texture2D>(AnhUot);
        lop = Mats.Alpha("P_BiUot", anhUot, new Color(1f, 1f, 1f, 0f));
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r == null || !r.enabled) continue;
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;
            // Bo renderer trong suot (vom khien, hat, tia set gan vao nguoi) - nhu ChayDenToanThan
            bool trongSuot = false;
            // Chi xet vat lieu GOC (o dau): cac lop PHU THEM (bong uot, chay den, vo bang) deu trong suot - xet ca mang thi
            // ke dang uot khong bao gio chay den (menu 83 bat duoc 25/09/2026: mua lam uot TRUOC tia dau)
            if (mats[0] == null || mats[0].renderQueue >= 3000) trongSuot = true;
            if (trongSuot) continue;
            var moi = new Material[mats.Length + 1];
            for (int k = 0; k < mats.Length; k++) moi[k] = mats[k];
            moi[mats.Length] = lop;
            r.sharedMaterials = moi;
            daPhu.Add(r);
        }
        nho = VfxFactory.NhoNuocTuNguoi(transform);
    }

    void Update()
    {
        conLai -= Time.deltaTime;
        tuoi += Time.deltaTime;
        // Hien theo TUOI (khong theo conLai): mua lam moi conLai moi 0,2 s, tinh theo conLai la lop bong nhay ve 0 moi lan
        float k = Mathf.Clamp01(tuoi / GiayHien) * Mathf.Clamp01(conLai / GiayPhai);
        DoDam = k;
        if (lop != null) lop.SetColor("_TintColor", new Color(1f, 1f, 1f, k));
        if (nho != null)
        {
            var ps = nho.GetComponent<ParticleSystem>();
            if (ps != null) { var em = ps.emission; em.enabled = conLai > 0.4f; }
        }
        if (conLai <= 0f) Destroy(this);
    }

    void OnDestroy()
    {
        for (int i = 0; i < daPhu.Count; i++)
        {
            var r = daPhu[i];
            if (r == null) continue;
            var mats = r.sharedMaterials;
            var giu = new List<Material>(mats.Length);
            for (int k = 0; k < mats.Length; k++) if (mats[k] != lop) giu.Add(mats[k]);
            r.sharedMaterials = giu.ToArray();
        }
        daPhu.Clear();
        if (nho != null)
        {
            foreach (var ps in nho.GetComponentsInChildren<ParticleSystem>()) { var em = ps.emission; em.enabled = false; }
            Destroy(nho, 1.2f);
        }
        if (lop != null) Destroy(lop);
    }
}
