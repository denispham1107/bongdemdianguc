using UnityEngine;

/// <summary>
/// VET CHEM KIEM CUA QUAI BO XUONG (01/11/2026, nguoi dung: "hieu ung chem hien thoi qua so sai, cho thay ro hieu ung chem").
///
/// Vet la MAT QUET CUA LUOI KIEM THAT trong cu bo kiem xuong: <see cref="ModelHoatHinh"/> tinh mot lan duong di cua chuoi
/// kiem + mui kiem (dat tu the vung tay o nhieu moc thoi gian, doc xuong ban tay phai) trong khung GOC cua con quai, roi moi
/// lan chem goi <see cref="Tao"/>. Luoi dung chung cho moi con (cung model), moi vet chi doi 3 so qua MaterialPropertyBlock.
///
/// Vi sao khong TrailRenderer theo mui kiem: cu bo kiem chi dai ~0,04 giay (Bo xuong 0,3 s mot don) - tren dien thoai 30
/// khung/giay chi lay duoc 1-2 diem, vet thanh mot doan thang. Duong di tinh san thi vet luon cong dung theo cu chem.
///
/// Vet la CON cua con quai (di theo khi no buoc), song <see cref="GiaySong"/> roi tu xoa.
/// </summary>
public class VetChemKiem : MonoBehaviour
{
    /// <summary>Luoi kiem hien toi het cung chem trong it nhat chung nay giay (cu chem that nhanh hon - 30 khung/giay van thay vet chay).</summary>
    public const float GiayQuetToiThieu = 0.06f;
    /// <summary>Sau khi quet xong duoi vet duoi theo trong chung nay giay.</summary>
    public const float GiayDuoi = 0.20f;
    /// <summary>Doi tong cong cua mot vet.</summary>
    public const float GiaySong = 0.30f;

    /// <summary>Dem cho phep thu (menu 152).</summary>
    public static int SoVetDaTao;

    static Mesh luoiChung;
    static Vector3[] duongCanDaDung, duongMuiDaDung;
    static Material vatLieu;
    static readonly int idDau = Shader.PropertyToID("_Dau"), idDuoi = Shader.PropertyToID("_Duoi"), idMo = Shader.PropertyToID("_Mo");

    MeshRenderer mr;
    MaterialPropertyBlock mpb;
    float tuoi, giayQuet;

    /// <summary>Luoi cua vet dang dung chung - phep thu doc de do hinh.</summary>
    public static Mesh LuoiChung { get { return luoiChung; } }

    /// <summary>
    /// Sinh mot vet chem. <paramref name="goc"/> la goc con quai (khung cua hai duong di);
    /// <paramref name="giayQuet"/> la thoi gian that luoi kiem di het cung chem.
    /// </summary>
    public static VetChemKiem Tao(Transform goc, Vector3[] duongCan, Vector3[] duongMui, float giayQuet)
    {
        if (goc == null || duongCan == null || duongMui == null || duongCan.Length < 2) return null;
        var luoi = LayLuoi(duongCan, duongMui);
        var mat = LayVatLieu();
        if (luoi == null || mat == null) return null;

        var go = new GameObject("VetChemBoXuong");
        go.transform.SetParent(goc, false);
        go.layer = goc.gameObject.layer;
        go.AddComponent<MeshFilter>().sharedMesh = luoi;
        var v = go.AddComponent<VetChemKiem>();
        v.mr = go.AddComponent<MeshRenderer>();
        v.mr.sharedMaterial = mat;
        v.mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        v.mr.receiveShadows = false;
        v.mpb = new MaterialPropertyBlock();
        v.giayQuet = Mathf.Max(GiayQuetToiThieu, giayQuet);
        v.DatSo(0f);
        SoVetDaTao++;
        return v;
    }

    void Update()
    {
        tuoi += Time.deltaTime;
        if (tuoi >= GiaySong) { Destroy(gameObject); return; }
        DatSo(tuoi);
    }

    void DatSo(float t)
    {
        float dau = Mathf.Clamp01(t / giayQuet);
        // Duoi bat dau duoi khi dau da di nua cung, toi het cung sau GiayDuoi
        float duoi = Mathf.Clamp01((t - giayQuet * 0.5f) / (giayQuet * 0.5f + GiayDuoi)) * 1.2f;
        float mo = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - giayQuet - 0.06f) / (GiaySong - giayQuet - 0.06f)));
        mr.GetPropertyBlock(mpb);
        mpb.SetFloat(idDau, dau);
        mpb.SetFloat(idDuoi, duoi);
        mpb.SetFloat(idMo, mo);
        mr.SetPropertyBlock(mpb);
    }

    /// <summary>
    /// Dung luoi mat quet: moi moc mot cot 3 dinh (chuoi kiem, giua, mui keo dai 8% cho quang mep). u = moc / (so moc - 1), v = 0 chuoi -> 1 mui.
    /// Duong di la cua cung model nen dung mot luoi cho moi con; doi duong (phep thu) thi dung lai.
    /// </summary>
    static Mesh LayLuoi(Vector3[] can, Vector3[] mui)
    {
        if (luoiChung != null && duongCanDaDung == can && duongMuiDaDung == mui) return luoiChung;
        int n = can.Length;
        var dinh = new Vector3[n * 3];
        var uv = new Vector2[n * 3];
        var tg = new int[(n - 1) * 12];
        for (int i = 0; i < n; i++)
        {
            Vector3 c = can[i], m = mui[i];
            Vector3 xa = m + (m - c) * 0.08f;
            dinh[i * 3] = c; dinh[i * 3 + 1] = Vector3.Lerp(c, xa, 0.5f); dinh[i * 3 + 2] = xa;
            float u = i / (float)(n - 1);
            uv[i * 3] = new Vector2(u, 0f); uv[i * 3 + 1] = new Vector2(u, 0.5f); uv[i * 3 + 2] = new Vector2(u, 1f);
        }
        int k = 0;
        for (int i = 0; i < n - 1; i++)
            for (int r = 0; r < 2; r++)
            {
                int a = i * 3 + r, b = a + 1, c = a + 3, d = c + 1;
                tg[k++] = a; tg[k++] = c; tg[k++] = b;
                tg[k++] = b; tg[k++] = c; tg[k++] = d;
            }
        if (luoiChung == null) luoiChung = new Mesh { name = "VetChemBoXuong" };
        luoiChung.Clear();
        luoiChung.vertices = dinh;
        luoiChung.uv = uv;
        luoiChung.triangles = tg;
        luoiChung.RecalculateBounds();
        duongCanDaDung = can; duongMuiDaDung = mui;
        return luoiChung;
    }

    /// <summary>Vat lieu nap tu Resources (giu shader trong ban build). Kiem null cua Unity: vat lieu nap luc Play bi go khi thoat Play.</summary>
    static Material LayVatLieu()
    {
        if (vatLieu == null) vatLieu = Resources.Load<Material>("KyNang/BoXuong/VetChem");
        return vatLieu;
    }
}
