using UnityEngine;

/// <summary>
/// MOT BINH MAU / BINH MANA NAM TREN DAT - hinh ve va luc bay vao nguoi.
///
/// Luat chia binh (ai duoc) nam o <see cref="QuanLyBinhRoi"/>; lop nay chi:
///   - ve binh (tam hinh ve san quay mat ve camera, lo lung nhap nho, quang sang),
///   - thay nhan vat CUA MAY NAY vao pham vi hut thi xin nhat,
///   - co chu roi thi bay vao nguoi chu, toi noi thi bao lai.
///
/// Hinh la mot TAM PHANG (anh Resources/VatPham/*.png ve bang CongCu/Icon/sinh_binh.py)
/// chu khong phai luoi 3D: camera nhin chech tu tren xuong, mot cai binh nho hon
/// mot met nhin bang tam phang quay theo camera la ro nhat va nhe nhat cho dien thoai.
/// </summary>
public class BinhRoi : MonoBehaviour
{
    public int SoHieu { get; private set; }
    public int Ky { get; private set; }

    /// <summary>Da co nguoi duoc giao binh nay (dang bay vao nguoi ho).</summary>
    public bool DaCoChu { get; private set; }
    /// <summary>Nguoi duoc giao la nhan vat cua may nay - toi noi thi cong so binh.</summary>
    public bool LaCuaToi { get; private set; }
    public byte GheChu { get; private set; }

    /// <summary>May khach da gui loi xin, dang cho chu phong tra loi.</summary>
    public bool DaXin { get; private set; }

    float lucXin;
    Transform dich;
    float giayBay;
    Transform hinh, quang;
    float phaNhap;

    const float CaoLoLung = 0.85f;
    const float CoHinh = 0.95f;
    /// <summary>Xin roi ma chua thay tra loi sau bay nhieu giay thi xin lai (goi co the mat).</summary>
    const float XinLaiSau = 1.5f;

    static Material vlMau, vlMana, vlQuangMau, vlQuangMana;
    static Texture2D anhQuang;

    public static BinhRoi Tao(int soHieu, int ky, Vector3 viTri)
    {
        var go = new GameObject("BinhRoi_" + (ky == CapDo.KyBinhMau ? "Mau_" : "Mana_") + soHieu);
        go.transform.position = viTri;
        var b = go.AddComponent<BinhRoi>();
        b.SoHieu = soHieu;
        b.Ky = ky;
        b.phaNhap = Random.value * 6.28f;
        b.DungHinh();
        return b;
    }

    void DungHinh()
    {
        bool mau = Ky == CapDo.KyBinhMau;
        if (vlMau == null)
        {
            vlMau = Mats.Alpha("BinhMau", Resources.Load<Texture2D>("VatPham/BinhMau"), Color.white);
            vlMana = Mats.Alpha("BinhMana", Resources.Load<Texture2D>("VatPham/BinhMana"), Color.white);
            vlQuangMau = Mats.Additive("QuangBinhMau", AnhQuang, new Color(1f, 0.20f, 0.10f), 1.1f);
            vlQuangMana = Mats.Additive("QuangBinhMana", AnhQuang, new Color(0.25f, 0.55f, 1f), 1.1f);
        }

        quang = TamPhang("Quang", mau ? vlQuangMau : vlQuangMana, CoHinh * 1.9f);
        hinh = TamPhang("Hinh", mau ? vlMau : vlMana, CoHinh);
    }

    Transform TamPhang(string ten, Material vl, float co)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = ten;
        var va = go.GetComponent<Collider>();
        if (va != null) Destroy(va);                  // khong chan duong, khong an tia ngam
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = vl;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        go.transform.SetParent(transform, false);
        go.transform.localScale = new Vector3(co, co, 1f);
        return go.transform;
    }

    /// <summary>Anh vong sang mo dan (tu sinh, 64x64).</summary>
    static Texture2D AnhQuang
    {
        get
        {
            if (anhQuang != null) return anhQuang;
            const int n = 64;
            anhQuang = new Texture2D(n, n, TextureFormat.RGBA32, false);
            anhQuang.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n - 0.5f, dy = (y + 0.5f) / n - 0.5f;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 2f);
                    float a = (1f - d) * (1f - d);
                    px[y * n + x] = new Color(a, a, a, a);
                }
            anhQuang.SetPixels(px);
            anhQuang.Apply(false, false);
            return anhQuang;
        }
    }

    public void DanhDauDaXin() { DaXin = true; lucXin = Time.time; }

    /// <summary>Giao binh cho nhan vat <paramref name="nguoi"/>: bat dau bay vao.</summary>
    public void BayVao(Transform nguoi, byte ghe, bool laCuaToi)
    {
        DaCoChu = true;
        dich = nguoi;
        GheChu = ghe;
        LaCuaToi = laCuaToi;
        giayBay = 0f;
    }

    void Update()
    {
        float t = Time.time;
        if (!DaCoChu)
        {
            // Nhap nho tren mat dat
            float cao = CaoLoLung + Mathf.Sin(t * 2.4f + phaNhap) * 0.12f;
            hinh.localPosition = new Vector3(0f, cao, 0f);
            quang.localPosition = new Vector3(0f, cao, 0.02f);
            float dap = 1f + Mathf.Sin(t * 3.1f + phaNhap) * 0.10f;
            quang.localScale = new Vector3(CoHinh * 1.9f * dap, CoHinh * 1.9f * dap, 1f);

            if (DaXin && t - lucXin > XinLaiSau) DaXin = false;
            if (!DaXin) KiemNhanVatToiGan();
            return;
        }

        // ---- Bay vao nguoi chu ----
        if (dich == null) { Xong(); return; }
        giayBay += Time.deltaTime;
        Vector3 den = dich.position + Vector3.up * 1.1f;
        Vector3 hien = hinh.position;
        float toc = 4f + 22f * giayBay;                       // nhanh dan len
        Vector3 moi = Vector3.MoveTowards(hien, den, toc * Time.deltaTime);
        // Di chuyen GOC chu khong di tam hinh: hinh va quang cung theo
        transform.position += moi - hien;
        float con = Vector3.Distance(moi, den);
        float co = Mathf.Lerp(0.35f, 1f, Mathf.Clamp01(con / 2.5f));
        hinh.localScale = new Vector3(CoHinh * co, CoHinh * co, 1f);
        quang.localScale = new Vector3(CoHinh * 1.9f * co, CoHinh * 1.9f * co, 1f);
        if (con < 0.3f || giayBay > 3f) Xong();
    }

    void KiemNhanVatToiGan()
    {
        var dir = GameDirector.Instance;
        var nv = dir != null ? dir.player : null;
        if (nv == null) return;
        var mau = nv.GetComponent<Damageable>();
        if (mau != null && mau.IsDead) return;
        Vector3 d = nv.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude <= QuanLyBinhRoi.BanKinhHut * QuanLyBinhRoi.BanKinhHut)
            QuanLyBinhRoi.XinNhat(this);
    }

    void LateUpdate()
    {
        // Quay mat ve camera
        var cam = Camera.main;
        if (cam == null) return;
        Quaternion q = cam.transform.rotation;
        hinh.rotation = q;
        quang.rotation = q;
    }

    bool daXong;

    void Xong()
    {
        if (daXong) return;
        daXong = true;
        QuanLyBinhRoi.DaToiNoi(this);
        Destroy(gameObject);
    }
}
