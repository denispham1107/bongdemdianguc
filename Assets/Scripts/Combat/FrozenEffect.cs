using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TRANG THAI DONG BANG: quai bi boc trong lop vo bang, cham hoac dung im hoan toan.
/// Neu chet trong luc dang dong bang thi vo tan thanh manh bang.
///
/// HAI LOP CHONG NHAU, dem GIO RIENG (nguoi dung chot 12/09/2026):
///   - LOP CHAM  : <see cref="slow"/> keo dai het <see cref="remaining"/>.
///   - LOP DONG CUNG: <see cref="dongCungConLai"/> - khong nhuc nhich, khong
///     tung duoc phep.
/// Mua bang ap ca hai cung luc: trung la cham 50% trong 2 giay, va 35% so lan
/// thi 1,5 giay dau bi dong cung han.
///
/// Truoc day chi co MOT cap (slow + remaining) va "dong cung" duoc suy ra tu
/// <c>slow &gt; 0,85</c>. Kieu ay khong the dien ta "dong cung 1,5 giay ROI cham
/// 50% them nua giay": ha slow xuong 0,5 la mat luon dau vet dong cung, con giu
/// slow = 1 la dung im ca 2 giay.
/// </summary>
public class FrozenEffect : MonoBehaviour
{
    [Tooltip("Con bao lau nua thi tan lop vo bang (cung la thoi han cua lop cham).")]
    public float remaining = 3f;

    [Tooltip("Ti le GIAM toc do cua lop cham: 0 = khong cham, 0.5 = cham mot nua.")]
    public float slow;

    [Tooltip("Con bao lau nua thi het DONG CUNG HOAN TOAN (khong di, khong danh, khong tung phep).")]
    public float dongCungConLai;

    public bool IsFullyFrozen { get { return dongCungConLai > 0f; } }

    /// <summary>He so toc do con lai, 0 = dung im. Dung cho ca quai lan nguoi choi.</summary>
    public float HeSoToc
    {
        get { return IsFullyFrozen ? 0f : Mathf.Clamp01(1f - slow); }
    }

    /// <summary>
    /// LAM CHAM (khong dong cung). Cong don thi lay cai NANG hon va lau hon.
    /// </summary>
    public static void ApCham(Damageable d, float tiLeCham, float giay)
    {
        if (d == null || d.IsDead || giay <= 0f) return;
        // Ban sao khong tu gieo hieu ung cho minh - xem ghi chu o Apply.
        if (d.mauDoMayKhacQuyet) return;

        var f = d.GetComponent<FrozenEffect>();
        if (f == null)
        {
            f = d.gameObject.AddComponent<FrozenEffect>();
            f.remaining = giay;
        }
        f.slow = Mathf.Max(f.slow, Mathf.Clamp01(tiLeCham));
        f.remaining = Mathf.Max(f.remaining, giay);
    }

    readonly List<Renderer> touched = new List<Renderer>();
    readonly List<Material[]> originals = new List<Material[]>();
    Material shellMat;
    GameObject breathVfx;
    float fadeIn;

    /// <summary>Mau chu "ĐÓNG BĂNG!" bay len - xanh bang, khac mau xanh trang cua "CHOÁNG!".</summary>
    public static readonly Color MauChuDongBang = new Color(0.55f, 0.88f, 1f);

    /// <summary>
    /// Chu "ĐÓNG BĂNG!" bay len dau - LUC BAT DAU dong cung (khong lap lai khi dang dong cung).
    ///
    /// Nguoi dung 14/09/2026: "khong thay xuat hien chu Dong bang giong nhu Choang, Nga".
    /// Truoc day khong co dong code nao sinh chu cho dong bang - ca quai lan nguoi choi.
    /// Goi o HAI noi, dung nhu "CHOÁNG!": may tu tinh (Apply) va may nhan tu goi tin
    /// (HieuUngQuaMang.ApCo) - ban sao khong di qua Apply nen khong bao gio in hai lan.
    /// </summary>
    public static void BaoChuDongBang(Transform t)
    {
        if (t == null) return;
        DamagePopup.SpawnText(t.position + Vector3.up * 2.1f, "ĐÓNG BĂNG!", MauChuDongBang);
    }

    public static void Apply(Damageable d, float seconds)
    {
        if (d == null || d.IsDead) return;
        // Ban sao (mau do may khac quyet) KHONG tu gieo hieu ung cho minh: ben
        // kia tung Random.value rieng, gieo them lan nua o day la hai may thay
        // hai ket qua khac nhau. Hieu ung cua ban sao den tu goi tin - xem
        // HieuUngQuaMang.ApCo.
        if (d.mauDoMayKhacQuyet) return;
        if (seconds <= 0f) return;

        var f = d.GetComponent<FrozenEffect>();
        if (f == null)
        {
            f = d.gameObject.AddComponent<FrozenEffect>();
            f.remaining = seconds;
        }
        if (!f.IsFullyFrozen) BaoChuDongBang(d.transform);
        f.dongCungConLai = Mathf.Max(f.dongCungConLai, seconds);
        f.remaining = Mathf.Max(f.remaining, seconds);
    }

    void Start()
    {
        shellMat = Mats.FrozenShell();
        shellMat.SetFloat("_Amount", 0f);

        // Phu them mot lop vat lieu "vo bang" len tren moi mieng hinh.
        //
        // CA SkinnedMeshRenderer (14/09/2026). Truoc day chi lay MeshRenderer - hop voi quai
        // dung bang code thoi dau, nhung nhan vat nguoi choi va quai hien nay deu la model co
        // xuong (SkinnedMeshRenderer): menu 67 do ra vo bang phu len 0 renderer, nguoi bi dong
        // cung trong y het binh thuong, chi dung im.
        // Bo qua renderer TRONG SUOT (hang doi >= 3000): vom khieng, hieu ung hat, tia set con
        // gan vao nhan vat - boc bang len chung chi ra mot dam mo.
        var rends = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            if (r == null || !r.enabled) continue;
            if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer)) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;
            bool trongSuot = false;
            for (int k = 0; k < mats.Length; k++)
                if (mats[k] == null || mats[k].renderQueue >= 3000 || mats[k].shader == Mats.FrozenShader) trongSuot = true;
            if (trongSuot) continue;

            touched.Add(r);
            originals.Add(mats);

            var extended = new Material[mats.Length + 1];
            for (int k = 0; k < mats.Length; k++) extended[k] = mats[k];
            extended[mats.Length] = shellMat;
            r.materials = extended;
        }

        // Hoi lanh boc quanh
        breathVfx = new GameObject("FrostAura");
        breathVfx.transform.SetParent(transform, false);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        remaining -= dt;
        if (dongCungConLai > 0f) dongCungConLai -= dt;

        // Dong bang cung dan cho muot mat. DAY DAC khi dong cung, MONG (0,5) khi chi bi cham -
        // nhin vao la phan biet duoc "dung im" voi "di cham".
        // Nguong 0,2 giay (truoc 0,4): ban sao qua mang chi duoc giu song 0,35 giay moi goi
        // (HieuUngQuaMang.GiuSongGiay) - nguong 0,4 thi vo bang tren ban sao KHONG BAO GIO hien.
        float dich = remaining > 0.2f ? (IsFullyFrozen ? 1f : 0.5f) : 0f;
        fadeIn = Mathf.MoveTowards(fadeIn, dich, Time.deltaTime * 5f);
        if (shellMat != null) shellMat.SetFloat("_Amount", fadeIn);

        if (remaining <= 0f) Thaw();
    }

    /// <summary>Tan bang: tra lai vat lieu goc.</summary>
    public void Thaw()
    {
        Restore();
        Destroy(this);
    }

    void Restore()
    {
        for (int i = 0; i < touched.Count; i++)
        {
            if (touched[i] == null) continue;
            touched[i].materials = originals[i];
        }
        touched.Clear();
        originals.Clear();
        if (breathVfx != null) Destroy(breathVfx);
    }

    void OnDestroy()
    {
        Restore();
    }
}
