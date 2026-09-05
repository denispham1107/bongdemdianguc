using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MANG LUOI DUONG MON TREN BAN DO.
///
/// Duong khong phai la khoi hinh dat de len mat dat, ma la LOP VAT LIEU SOI DA
/// duoc to thang vao terrain. Lam vay thi duong luon om sat mat dat du dia hinh
/// go ghe the nao, va khong ton them mot mieng luoi nao.
///
/// Hinh dang mang luoi:
///   - Mot nga tu o giua san
///   - Nam sau nhanh chinh toa tu nga tu ra cac diem vanh ngoai
///   - Vai doan vong noi cac diem vanh ngoai voi nhau, tao thanh vong khep kin
///   - Vai nhanh cut re ngang roi tit dan
///
/// CAY, DA, MO deu duoc dat SAU khi co mang duong, va deu phai tranh duong ra.
/// Xem <see cref="TranhDuong"/>.
/// </summary>
public static class DuongFactory
{
    /// <summary>Be rong long duong chinh, met.</summary>
    public const float BeRongChinh = 2.7f;

    /// <summary>Be rong duong nhanh, met.</summary>
    public const float BeRongNhanh = 1.8f;

    /// <summary>Vien mo hai ben mep duong, met.</summary>
    public const float VienMo = 0.9f;

    /// <summary>Mot doan duong: duong gay khuc noi nhieu diem.</summary>
    public class Doan
    {
        public Vector3[] diem;
        public float beRong;
    }

    /// <summary>
    /// Mang duong dang dung. Dat mot lan luc dung scene, sau do cho co, cay, da,
    /// mo deu doc chung cai nay de tranh ra.
    /// </summary>
    public static List<Doan> MangHienTai;

    // ================================================================
    //  SINH MANG LUOI
    // ================================================================

    /// <summary>
    /// Sinh mang luoi duong. Cung mot <paramref name="seed"/> thi luon ra cung
    /// mot mang - quan trong, vi cho to mat dat va cho dat cay da phai thay
    /// DUNG MOT mang duong thi cay moi khong moc giua duong.
    /// </summary>
    public static List<Doan> Dung(float banKinh, int seed)
    {
        var cu = Random.state;              // muon tam bo sinh so, tra lai sau
        Random.InitState(seed * 7919 + 101);

        var mang = new List<Doan>();

        // ---- Nga tu giua san, lech di mot chut cho khoi qua ngay ngan ----
        Vector3 tam = new Vector3(Random.Range(-3.5f, 3.5f), 0f, Random.Range(-3.5f, 3.5f));

        // ---- Cac diem o vanh ngoai ----
        int soNga = Random.Range(5, 7);
        var nga = new List<Vector3>();
        float gocDau = Random.Range(0f, 360f);
        for (int i = 0; i < soNga; i++)
        {
            float a = gocDau + i * (360f / soNga) + Random.Range(-15f, 15f);
            float r = banKinh * Random.Range(0.56f, 0.78f);
            nga.Add(Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, r));
        }

        // ---- Nhanh chinh: tu nga tu toa ra tung diem vanh ngoai ----
        for (int i = 0; i < nga.Count; i++)
            mang.Add(new Doan
            {
                diem = DuongCong(tam, nga[i], 6, banKinh * 0.11f),
                beRong = BeRongChinh
            });

        // ---- Doan vong noi cac diem vanh ngoai: tao vong khep kin ----
        for (int i = 0; i < nga.Count; i++)
            if (Random.value < 0.62f)
                mang.Add(new Doan
                {
                    diem = DuongCong(nga[i], nga[(i + 1) % nga.Count], 5, banKinh * 0.15f),
                    beRong = BeRongNhanh
                });

        // ---- Nhanh cut: re ngang tu mot doan co san roi tit dan ----
        int soNhanh = Random.Range(4, 7);
        int coSan = mang.Count;
        for (int i = 0; i < soNhanh; i++)
        {
            var goc = mang[Random.Range(0, coSan)];
            int k = Random.Range(1, goc.diem.Length - 1);
            Vector3 batDau = goc.diem[k];

            // Re vuong goc voi huong duong me, roi di mot doan ngan
            Vector3 huong = (goc.diem[k + 1] - goc.diem[k - 1]).normalized;
            Vector3 ngang = new Vector3(-huong.z, 0f, huong.x);
            if (Random.value < 0.5f) ngang = -ngang;

            float dai = banKinh * Random.Range(0.13f, 0.26f);
            Vector3 ketThuc = batDau + ngang * dai
                            + huong * Random.Range(-dai * 0.4f, dai * 0.4f);

            // Khong cho nhanh choc ra ngoai vanh da bao quanh
            if (new Vector2(ketThuc.x, ketThuc.z).magnitude > banKinh * 0.88f) continue;

            mang.Add(new Doan
            {
                diem = DuongCong(batDau, ketThuc, 4, banKinh * 0.06f),
                beRong = BeRongNhanh
            });
        }

        Random.state = cu;
        MangHienTai = mang;
        return mang;
    }

    /// <summary>Duong noi hai diem, uon cong nhe cho khoi thang bang nhu ke.</summary>
    static Vector3[] DuongCong(Vector3 a, Vector3 b, int soDiem, float lech)
    {
        var kq = new Vector3[soDiem];

        Vector3 huong = b - a; huong.y = 0f;
        Vector3 ngang = new Vector3(-huong.z, 0f, huong.x).normalized;

        float congChinh = Random.Range(-lech, lech);
        for (int i = 0; i < soDiem; i++)
        {
            float t = i / (float)(soDiem - 1);
            // Cong nhat o giua, hai dau van dinh dung cho
            float cong = Mathf.Sin(t * Mathf.PI) * congChinh
                       + Mathf.Sin(t * Mathf.PI * 2.3f) * lech * 0.30f;
            kq[i] = Vector3.Lerp(a, b, t) + ngang * cong;
            kq[i].y = 0f;
        }
        return kq;
    }

    // ================================================================
    //  HOI: CHO NAY CACH DUONG BAO XA
    // ================================================================

    /// <summary>
    /// Khoang cach tu mot cho toi tim duong gan nhat.
    /// <paramref name="beRong"/> tra ve be rong cua chinh doan duong gan nhat do.
    /// </summary>
    public static float KhoangCach(List<Doan> mang, float x, float z, out float beRong)
    {
        beRong = BeRongChinh;
        if (mang == null || mang.Count == 0) return float.MaxValue;

        float gan = float.MaxValue;
        var p = new Vector2(x, z);

        for (int d = 0; d < mang.Count; d++)
        {
            var doan = mang[d];
            for (int i = 0; i < doan.diem.Length - 1; i++)
            {
                float k = CachDoanThang(p,
                    new Vector2(doan.diem[i].x, doan.diem[i].z),
                    new Vector2(doan.diem[i + 1].x, doan.diem[i + 1].z));
                if (k < gan) { gan = k; beRong = doan.beRong; }
            }
        }
        return gan;
    }

    static float CachDoanThang(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float dai2 = ab.sqrMagnitude;
        if (dai2 < 1e-6f) return Vector2.Distance(p, a);

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / dai2);
        return Vector2.Distance(p, a + ab * t);
    }

    /// <summary>
    /// Cho nay co nam tren duong khong (tinh ca <paramref name="bienLe"/> chua ra
    /// hai ben)? Dung de khong dat cay, da, mo len duong.
    /// </summary>
    public static bool TranhDuong(float x, float z, float bienLe)
    {
        if (MangHienTai == null) return false;

        float beRong;
        float d = KhoangCach(MangHienTai, x, z, out beRong);
        return d < beRong * 0.5f + bienLe;
    }
}
