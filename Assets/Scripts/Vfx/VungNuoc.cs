using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MOT VUNG NUOC (ho hoac bai dam lay).
///
/// Gan len chinh vat the mat nuoc luc dung ban do. No khong ve gi ca - viec do
/// la cua shader - ma chi TRA LOI mot cau hoi: dung o day thi ngap sau bao nhieu.
///
/// VI SAO KHONG DUNG COLLIDER: mat nuoc phai cho nguoi choi va quai di xuyen qua,
/// nen khong the dat collider dac. Dung trigger thi lai phai bat OnTriggerStay
/// tren tung con quai, ma con can biet ngap SAU BAO NHIEU chu khong chi biet
/// co cham hay khong. Giu san bo va muc nuoc o day roi hoi thang la vua re vua
/// tra ve dung con so can dung.
///
/// Bo nuoc KHONG tron: no meo theo dung long chao. Nen ban kinh duoc luu theo
/// tung huong, y het mang ma <c>Act2DamLay</c> da do luc dao.
/// </summary>
public class VungNuoc : MonoBehaviour
{
    [Tooltip("Do cao mat nuoc trong the gioi, met")]
    public float mucNuoc;

    [Tooltip("Ban kinh bo theo tung huong, chia deu quanh vong tron")]
    public float[] banKinhBo;

    [Tooltip("Ho nuoc thi sau, bai dam lay thi nong")]
    public bool laHo;

    static readonly List<VungNuoc> tatCa = new List<VungNuoc>();

    /// <summary>Ban kinh lon nhat, de loai nhanh nhung diem o xa.</summary>
    float banKinhToiDa;

    void OnEnable()
    {
        banKinhToiDa = 0f;
        if (banKinhBo != null)
            foreach (float r in banKinhBo)
                if (r > banKinhToiDa) banKinhToiDa = r;

        tatCa.Add(this);
    }

    void OnDisable()
    {
        tatCa.Remove(this);
    }

    /// <summary>Ban kinh bo theo huong cua diem <paramref name="p"/>.</summary>
    float BoTaiDiem(Vector3 p)
    {
        if (banKinhBo == null || banKinhBo.Length == 0) return 0f;

        Vector3 tam = transform.position;
        float goc = Mathf.Atan2(p.z - tam.z, p.x - tam.x);
        if (goc < 0f) goc += Mathf.PI * 2f;

        float k = goc / (Mathf.PI * 2f) * banKinhBo.Length;
        int i = Mathf.FloorToInt(k) % banKinhBo.Length;
        int j = (i + 1) % banKinhBo.Length;

        // Noi giua hai huong ke nhau, khong thi bo nuoc ra thanh hinh da giac
        return Mathf.Lerp(banKinhBo[i], banKinhBo[j], k - Mathf.Floor(k));
    }

    /// <summary>
    /// RIENG VUNG NAY: dung o <paramref name="chan"/> thi ngap sau bao nhieu met.
    ///
    /// Tach rieng khoi ban static de con THU DUOC ngoai Play mode. Ban static
    /// duyet danh sach <c>tatCa</c>, ma danh sach do chi duoc dien trong
    /// <c>OnEnable</c> - thu ngoai Play mode thi no rong tuech va ham tra ve 0 o
    /// moi diem, nhin y nhu code hong.
    /// </summary>
    public float NgapTai(Vector3 chan)
    {
        float ngap = mucNuoc - chan.y;
        if (ngap <= 0f) return 0f;                  // dung cao hon mat nuoc

        // banKinhToiDa duoc tinh trong OnEnable; ngoai Play mode thi no bang 0
        // nen phai tu tinh lai o day.
        float bkMax = banKinhToiDa;
        if (bkMax <= 0f && banKinhBo != null)
            foreach (float r in banKinhBo)
                if (r > bkMax) bkMax = r;

        Vector3 tam = transform.position;
        float dx = chan.x - tam.x, dz = chan.z - tam.z;
        float kc2 = dx * dx + dz * dz;
        if (kc2 > bkMax * bkMax) return 0f;         // loai nhanh

        if (Mathf.Sqrt(kc2) > BoTaiDiem(chan)) return 0f;   // ngoai bo

        return ngap;
    }

    /// <summary>
    /// Dung o <paramref name="chan"/> thi ngap sau bao nhieu met. 0 = kho chan.
    ///
    /// <paramref name="chan"/> la diem DUOI CUNG cua nhan vat, khong phai tam
    /// nguoi - lay tam nguoi thi ai cung ngap toi co.
    /// </summary>
    public static float DoNgap(Vector3 chan, out VungNuoc vung)
    {
        vung = null;
        float sauNhat = 0f;

        for (int i = 0; i < tatCa.Count; i++)
        {
            var v = tatCa[i];
            if (v == null) continue;

            float ngap = v.NgapTai(chan);
            if (ngap > sauNhat) { sauNhat = ngap; vung = v; }
        }

        return sauNhat;
    }

    /// <summary>Ban goi tat khi khong can biet la vung nao.</summary>
    public static float DoNgap(Vector3 chan)
    {
        VungNuoc bo;
        return DoNgap(chan, out bo);
    }
}
