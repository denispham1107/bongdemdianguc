using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TRANG THAI DONG BANG: quai bi boc trong lop vo bang, cham hoac dung im hoan toan.
/// Neu chet trong luc dang dong bang thi vo tan thanh manh bang.
/// </summary>
public class FrozenEffect : MonoBehaviour
{
    [Tooltip("Con bao lau nua thi tan bang.")]
    public float remaining = 3f;

    [Tooltip("1 = dung im hoan toan, 0.5 = cham mot nua.")]
    public float slow = 1f;

    public bool IsFullyFrozen { get { return slow > 0.85f && remaining > 0f; } }

    readonly List<Renderer> touched = new List<Renderer>();
    readonly List<Material[]> originals = new List<Material[]>();
    Material shellMat;
    GameObject breathVfx;
    float fadeIn;

    public static void Apply(Damageable d, float seconds)
    {
        if (d == null || d.IsDead) return;
        // Ban sao (mau do may khac quyet) KHONG tu gieo hieu ung cho minh: ben
        // kia tung Random.value rieng, gieo them lan nua o day la hai may thay
        // hai ket qua khac nhau. Hieu ung cua ban sao den tu goi tin - xem
        // HieuUngQuaMang.ApCo.
        if (d.mauDoMayKhacQuyet) return;

        var f = d.GetComponent<FrozenEffect>();
        if (f == null) f = d.gameObject.AddComponent<FrozenEffect>();
        f.remaining = Mathf.Max(f.remaining, seconds);
    }

    void Start()
    {
        shellMat = Mats.FrozenShell();
        shellMat.SetFloat("_Amount", 0f);

        // Phu them mot lop vat lieu "vo bang" len tren moi mieng hinh
        var rends = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            if (r == null) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

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
        remaining -= Time.deltaTime;

        // Dong bang cung dan trong 0.25 giay dau cho muot mat
        fadeIn = Mathf.MoveTowards(fadeIn, remaining > 0.4f ? 1f : 0f, Time.deltaTime * 4f);
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
