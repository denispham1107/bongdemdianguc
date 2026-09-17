using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// KY NANG "LUA DIA NGUC" (so hieu 11, them 17/09/2026).
///
/// Nguoi dung: phong ra 4 qua cau lua giong Quả cầu lửa nhung TU DI dung vao muc tieu (nguoi choi / quai o gan), van thieu
/// dot nhu Qua cau lua; sat thuong ban dau bang Qua cau lua CAP 5; hoi chieu 0,5 giay. Nguoi dung chon them:
///   - cap 1 = sat thuong Qua cau lua cap 5 (85 x 1,2^4 ~ 176), van +20% moi cap nhu moi ky nang;
///   - chia 4 qua cho toi da 4 ke dich GAN NGUOI TUNG nhat trong 20 m, it hon 4 thi qua du nham ke gan nhat; muc tieu chet
///     giua duong thi doi sang ke con song gan qua nhat;
///   - bam chac (queo ~360 do/giay, gan muc tieu queo gap doi), dam vat can van no nhu Qua cau lua;
///   - 25 nang luong, niem 0,38 s; khong co ai thi 4 qua toa quat bay thang;
///   - hinh LUA DO SAM dia nguc (nhuom lai qua cau lua + vu no), toa quat roi uon cong; icon dung bang Blender MCP.
///
/// Duong bay / va cham / no / thieu dot la cua <see cref="Fireball"/> - chi them phan tu di (Fireball.tocQueo).
/// </summary>
public static class LuaDiaNguc
{
    public const int SoQua = 4;
    public const float TamTim = 20f;
    public const float TocQueo = 360f;
    public const float GocToe = 18f;
    /// <summary>Bay thang toa quat bao lau roi moi bat dau di (de thay hinh quat truoc khi uon cong).</summary>
    public const float GiayToaTruocKhiDi = 0.1f;

    /// <summary>Sat thuong CAP 1 = sat thuong mot qua Qua cau lua CAP 5. Doc tu PREFAB qua cau lua (prefab de len code).</summary>
    public static float SatThuongGoc
    {
        get
        {
            float goc = 85f;
            var pf = GameAssets.I != null ? GameAssets.I.fireballPrefab : null;
            var fb = pf != null ? pf.GetComponent<Fireball>() : null;
            if (fb != null) goc = fb.impactDamage;
            return goc * CapDo.SatThuongTheoCap(CapDo.CapKyNangToiDa);
        }
    }

    static readonly Collider[] boDem = new Collider[128];

    /// <summary>Toi da <paramref name="toiDa"/> ke dich con song (Damageable tren mask, tru boQua) gan <paramref name="tam"/> nhat.</summary>
    public static List<Damageable> TimGanNhat(Vector3 tam, LayerMask mask, Damageable boQua, float banKinh, int toiDa)
    {
        var ds = new List<Damageable>();
        int n = Physics.OverlapSphereNonAlloc(tam, banKinh, boDem, mask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < n; i++)
        {
            var d = boDem[i].GetComponentInParent<Damageable>();
            if (d == null || d == boQua || d.IsDead || ds.Contains(d)) continue;
            ds.Add(d);
        }
        ds.Sort((a, b) => (a.transform.position - tam).sqrMagnitude.CompareTo((b.transform.position - tam).sqrMagnitude));
        if (ds.Count > toiDa) ds.RemoveRange(toiDa, ds.Count - toiDa);
        return ds;
    }

    /// <summary>
    /// Phong 4 qua: toa quat quanh truc DUNG nhu <see cref="Fireball.SpawnChum"/>, moi qua gan mot muc tieu.
    /// </summary>
    public static void SpawnChum(Vector3 pos, Vector3 huong, Vector3 viTriNguoiTung, LayerMask hitMask, LayerMask damageMask,
                                 Damageable boQua, float heSoSatThuong = 1f, float themGiayChay = 0f)
    {
        huong.Normalize();
        var mucTieu = TimGanNhat(viTriNguoiTung, damageMask, boQua, TamTim, SoQua);
        float giua = (SoQua - 1) * 0.5f;
        float satThuong = SatThuongGoc;
        for (int i = 0; i < SoQua; i++)
        {
            Vector3 h = Quaternion.AngleAxis((i - giua) * GocToe, Vector3.up) * huong;
            var qua = Fireball.Spawn(pos, h, hitMask, damageMask);
            if (qua == null) continue;
            qua.name = "LuaDiaNguc";
            qua.boQua = boQua;
            qua.tuaTruoc = BuTre.TuaTruocGiay;
            qua.impactDamage = satThuong * heSoSatThuong;
            qua.burnSeconds += themGiayChay;
            qua.tocQueo = TocQueo;
            qua.tamTim = TamTim;
            qua.giayBatDauDi = GiayToaTruocKhiDi;
            qua.diaNguc = true;
            if (mucTieu.Count > 0) qua.mucTieu = i < mucTieu.Count ? mucTieu[i] : mucTieu[0];
            VfxFactory.NhuomLuaDiaNguc(qua.gameObject);
        }
        CameraShake.Shake(0.14f, 0.06f);
    }
}
