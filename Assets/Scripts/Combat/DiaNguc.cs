using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DIA NGUC NGOAI BAN DO (nguoi dung 05/10/2026: "ve thiet ke them cho ben duoi ngoai vung ban do la dia nguc, khi nguoi choi bi te nga xuong
/// la bi mat mau cho den chet"; chon: VUC THAM + DUNG NHAM, mo vai doan rao SAP, 20% mau toi da / giay, quai cung vay).
///
/// Phan LUAT CHOI (file nay): ai roi xuong duoi <see cref="NguongRoi"/> (dat thap nhat trong Act2 la -2,9 m) la da xuong vuc - cu
/// <see cref="Nhip"/> giay mat <see cref="TiLeMatMauMoiGiay"/> x Nhip mau TOI DA cho den chet, ca nguoi choi lan quai.
///   - Sat thuong VAT LY (khong khang nao giam), danh dau sat thuong RI (Bo xuong khong do don duoc), khong phun tia trung don.
///   - Chi may QUYET MAU tru mau (nguoi choi: may cua chinh ho; quai: chu phong) - ban sao (mauDoMayKhacQuyet) bo qua.
///   - Ai danh nan nhan GAN NHAT truoc luc roi (keDanhCuoi chup lai luc vua roi qua nguong) duoc tinh la ke ha - day nguoi / quai xuong
///     vuc van duoc kinh nghiem, bang diem.
///   - Day vuc: mot tam va cham lop Ground o <see cref="MucDungNham"/> (nguoi / quai roi cham day, xac nam tren do, vung mau co cho bam).
/// Phan HINH (vach vuc, bien dung nham, rao sap) dung bang Blender MCP - xem HUONG-DAN.
/// </summary>
public class DiaNguc : MonoBehaviour
{
    /// <summary>Mat dung nham (m, the gioi) - mat dat Act2 quanh 0 (-2,9 .. 2,9).</summary>
    public const float MucDungNham = -26f;
    /// <summary>Roi xuong duoi do cao nay (m) = da xuong vuc.</summary>
    public const float NguongRoi = -6f;
    /// <summary>Mat bao nhieu phan mau toi da moi giay (nguoi dung chon 20% -> ~5 giay).</summary>
    public const float TiLeMatMauMoiGiay = 0.20f;
    /// <summary>Nhip gay sat thuong (giay).</summary>
    public const float Nhip = 0.25f;

    static DiaNguc ins;
    readonly Dictionary<Damageable, Damageable> keDay = new Dictionary<Damageable, Damageable>();
    float hen;

    /// <summary>Phep thu doc: so ke da roi xuong vuc (tinh tu luc vao tran).</summary>
    public static int SoLanRoi { get; private set; }

    /// <summary>Dung dia nguc cho tran nay (GameBootstrap.Awake). Goi lai thi bo qua.</summary>
    public static DiaNguc Dung()
    {
        if (ins != null) return ins;
        SoLanRoi = 0;
        var go = new GameObject("DiaNguc");
        ins = go.AddComponent<DiaNguc>();
        // Day vuc: tam va cham rong (ban do 135 m, vuc toa ra ngoai), mat tren thap hon mat dung nham 0,5 m (chim nua bap chan)
        var day = new GameObject("DayDungNham");
        day.transform.SetParent(go.transform, false);
        int lop = LayerMask.NameToLayer("Ground");
        if (lop >= 0) day.layer = lop;
        var bc = day.AddComponent<BoxCollider>();
        bc.size = new Vector3(600f, 2f, 600f);
        bc.center = new Vector3(0f, MucDungNham - 0.5f - 1f, 0f);
        return ins;
    }

    /// <summary>Damageable nay da roi xuong vuc chua.</summary>
    public static bool DaRoi(Damageable d) { return d != null && d.transform.position.y < NguongRoi; }

    void Update()
    {
        hen -= Time.deltaTime;
        if (hen > 0f) return;
        hen += Nhip;
        if (hen < 0f) hen = Nhip;

        foreach (var d in FindObjectsByType<Damageable>(FindObjectsSortMode.None))
        {
            if (d == null || d.IsDead || !DaRoi(d)) continue;
            Damageable ke;
            if (!keDay.TryGetValue(d, out ke))
            {
                // Vua roi qua nguong: chup lai ai danh no gan nhat (nguoi day xuong)
                ke = d.keDanhCuoi;
                keDay[d] = ke;
                SoLanRoi++;
            }
            if (d.mauDoMayKhacQuyet) continue;   // ban sao: may quyet mau tu tru roi gui sang

            if (ke != null && ke != d) d.GhiKeDanh(ke);
            Damageable.LaSatThuongRi = true;
            Damageable.BoQuaTiaTrungDon = true;
            try { d.TakeDamage(d.maxHealth * TiLeMatMauMoiGiay * Nhip, DamageType.Physical, d.transform.position); }
            finally { Damageable.LaSatThuongRi = false; Damageable.BoQuaTiaTrungDon = false; }
        }
    }

    void OnDestroy()
    {
        if (ins == this) ins = null;
    }
}
