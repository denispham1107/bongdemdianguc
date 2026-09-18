using UnityEngine;

/// <summary>
/// TIA DIEN TOE RA quanh Qua cau dien (18/09/2026, nguoi dung gui anh Diablo III lan hai).
///
/// Trong anh, qua cau khong chi dung im: no lien tuc phong nhung tia dien ngan ra chung quanh,
/// phan lon cham xuong dat. Day la hieu ung THUAN HINH - sat thuong van do <see cref="QuaCauDien"/>
/// quyet dinh, vi the tia o day khong cham vao ai ca.
/// </summary>
public class ToeTiaDien : MonoBehaviour
{
    public float banKinh = 0.85f;
    /// <summary>Moi nhip phong bao nhieu tia.</summary>
    public int soTiaMoiNhip = 2;
    public float nhipToe = 0.13f;
    /// <summary>Do dai tia, tinh theo ban kinh qua cau.</summary>
    public float daiNhat = 3.2f, daiNgan = 1.4f;

    float dem;

    void Update()
    {
        dem -= Time.deltaTime;
        if (dem > 0f) return;
        dem = nhipToe * Random.Range(0.7f, 1.3f);

        for (int i = 0; i < soTiaMoiNhip; i++)
        {
            // Huong ngau nhien nhung THIEN VE PHIA DUOI: trong anh phan lon tia cham xuong dat
            Vector3 huong = Random.onUnitSphere;
            huong.y = -Mathf.Abs(huong.y) * 0.9f - 0.15f;
            huong.Normalize();

            float dai = banKinh * Random.Range(daiNgan, daiNhat);
            Vector3 tu = transform.position + huong * banKinh * 0.9f;
            Vector3 den = transform.position + huong * dai;

            // Cham dat thi dung o mat dat - tia cam xuong nen nhu trong anh
            float datY = VfxFactory.GroundY(den);
            if (den.y < datY) den.y = datY + 0.05f;

            VfxFactory.TiaCauDienToe(tu, den);
        }
    }
}
