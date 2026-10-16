using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DAU VET VAT BI LOC XOAY CUON (16/10/2026, nguoi dung: "khi Loc xoay cuon bay len het moi do vat nhu bia mo, da, nha, cay coi thi de lai DAU VET
/// cua cac do vat da cuon bay len chu khong de mat dat trong tron"). Nguoi dung chon: RIENG TUNG LOAI, ton tai DEN KHI VAT MOC LAI (30 s).
///
/// Hinh dung bang Blender MCP (CongCu/Blender/dau_vet_loc_cuon.py -> Resources/KyNang/LocXoay/DauVetLoc.fbx, luoi CHUAN HOA):
///   CAY    - goc cay GAY lom chom + dam go + re troi (GocCay0-2, scale = ban kinh than) tren ho dat xoi rong.
///   BIA MO - chan bia GAY (ChanBia0-2, scale theo be ngang / day bia) + da vun (VunDa0-2) + ho dat xoi.
///   DA     - ho dat lom + da vun nho.
///   NHA MO - nen do nat: chan tuong gay quanh mep + nen lat nut + dong gach (NenNha0-1, scale theo nen nha) + dat xoi.
///   LO LUA - vong da vo (NenLo0-1) tren nen TRO DEN.
/// Goc gay / chan bia / nen nha dung CHINH vat lieu cua vat (vo cay, da bia triplanar - khop mau tuyet doi). Ho dat: anh dat dia hinh Act2
/// (Anh_Act2_DatTroc, lap 3,7 m nhu dia hinh) toi hon (dat vua xoi am) - vat lieu Resources/KyNang/LocXoay/DatXoi.mat.
/// HO DAT BAM DIA HINH: moi dinh ban tia xuong lop Ground (dat goc trong tam - dat doc thi vanh khong lo lung / chim).
/// Vong doi: hien ra luc vat BAT KHOI DAT (het pha rung nut 0,3 s - VatTheBiCuon), vat moc lai thi LUN xuong 0,5 s roi xoa.
/// Chi hinh (khong va cham) - nguoi / quai di qua duoc. Moi may tu dung (Loc xoay phat lai tren moi may), 0 byte mang.
/// </summary>
public class DauVetLocCuon : MonoBehaviour
{
    public enum Kieu { Cay, Bia, Da, NhaMo, Lo }

    public const string DuongFbx = "KyNang/LocXoay/DauVetLoc";
    public const string DuongDat = "KyNang/LocXoay/DatXoi";
    /// <summary>Lun xuong bao lau khi vat moc lai.</summary>
    public const float GiayLun = 0.5f;
    /// <summary>Mau nhan cho ho dat: dat vua xoi (am, toi) / tro den (lo lua).</summary>
    public static readonly Color MauDatXoi = new Color(0.62f, 0.55f, 0.5f), MauTro = new Color(0.16f, 0.145f, 0.135f);
    /// <summary>Ti le ho dat so voi kich thuoc vat (ban kinh ho / ban kinh than cay, / nua be ngang bia...).</summary>
    public const float HoCay = 3.0f, HoBia = 0.85f, HoDa = 0.8f, HoNha = 0.62f, HoLo = 0.75f;

    // ---- thong ke cho phep thu ----
    public static int SoLanTao, SoLanXoa;
    public static readonly List<DauVetLocCuon> DangCo = new List<DauVetLocCuon>();

    public Kieu kieu;
    /// <summary>Kich thuoc do duoc (m): Cay = ban kinh than; con lai = be ngang (x) / day (z) nen vat.</summary>
    public Vector2 kichThuoc;
    public Transform goc, ho;

    float lun = -1f;
    Vector3 viTriGoc;

    static readonly Dictionary<string, Mesh> luoi = new Dictionary<string, Mesh>();
    static Material vlDat;
    static int lopDat = -1;
    static MaterialPropertyBlock mpb;

    static Mesh Luoi(string ten)
    {
        if (luoi.Count == 0 || (luoi.TryGetValue(ten, out var m0) && m0 == null))
        {
            luoi.Clear();
            foreach (var m in Resources.LoadAll<Mesh>(DuongFbx)) luoi[m.name] = m;
        }
        luoi.TryGetValue(ten, out var m1);
        return m1;
    }

    static Material VatLieuDat()
    {
        // vat lieu tao luc chay bi xoa khi thoat Play - kiem bang null cua Unity (memory vat-lieu-static-bi-xoa-khi-thoat-play)
        if (vlDat == null) vlDat = Resources.Load<Material>(DuongDat);
        return vlDat;
    }

    /// <summary>Phan loai vat bi cuon: theo NHOM CHA trong canh (Cay / BiaMo / Da / NhaMo) va LoLuaDa; vat khac theo kich thuoc.</summary>
    public static Kieu PhanLoai(GameObject vat, Bounds bao)
    {
        if (vat.GetComponent<LoLuaDa>() != null) return Kieu.Lo;
        var cha = vat.transform.parent;
        string ten = cha != null ? cha.name : "";
        if (ten == "Cay") return Kieu.Cay;
        if (ten == "BiaMo") return Kieu.Bia;
        if (ten == "Da") return Kieu.Da;
        if (ten == "NhaMo") return Kieu.NhaMo;
        if (bao.size.y >= VatTheBiCuon.CaoLaCay) return Kieu.Cay;
        if (Mathf.Max(bao.size.x, bao.size.z) < 0.9f && bao.size.y < 1f) return Kieu.Da;
        return Kieu.Bia;
    }

    /// <summary>Kich thuoc nen (m) trong khung CUC BO cua vat (khong dung AABB the gioi - bia xoay 45 do se ra rong gap 1,4).</summary>
    static Vector2 DoNen(GameObject vat, Kieu k, Bounds baoTheGioi)
    {
        Vector3 ls = vat.transform.lossyScale;
        if (k == Kieu.Cay)
        {
            var cap = vat.GetComponent<CapsuleCollider>();
            if (cap != null) { float r = cap.radius * Mathf.Max(Mathf.Abs(ls.x), Mathf.Abs(ls.z)); return new Vector2(r, r); }
            float r2 = Mathf.Min(baoTheGioi.size.x, baoTheGioi.size.z) * 0.5f; return new Vector2(r2, r2);
        }
        var box = vat.GetComponent<BoxCollider>();
        if (box != null) return new Vector2(Mathf.Abs(box.size.x * ls.x), Mathf.Abs(box.size.z * ls.z));
        var mf = vat.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Vector3 s = mf.sharedMesh.bounds.size; Vector3 l2 = mf.transform.lossyScale;
            return new Vector2(Mathf.Abs(s.x * l2.x), Mathf.Abs(s.z * l2.z));
        }
        return new Vector2(baoTheGioi.size.x, baoTheGioi.size.z);
    }

    public static float MatDatY(Vector3 p, float neuKhong)
    {
        if (lopDat < 0) lopDat = LayerMask.GetMask("Ground");
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 30f, Vector3.down, out hit, 80f, lopDat, QueryTriggerInteraction.Ignore)) return hit.point.y;
        return neuKhong;
    }

    /// <summary>Dung dau vet tai cho cu cua vat (goi luc vat bat khoi dat).</summary>
    public static DauVetLocCuon Tao(GameObject vat, Vector3 choCu, Quaternion huongCu, Bounds bao, Material vlVat)
    {
        if (vat == null || Luoi("HoDat0") == null) return null;
        var k = PhanLoai(vat, bao);
        Vector2 kt = DoNen(vat, k, bao);
        var rnd = new System.Random(Mathf.RoundToInt(choCu.x * 131f) ^ Mathf.RoundToInt(choCu.z * 977f));

        var go = new GameObject("DauVetLocCuon_" + k);
        float y = MatDatY(choCu, choCu.y);
        go.transform.position = new Vector3(choCu.x, y, choCu.z);
        float yaw = huongCu.eulerAngles.y;
        var dv = go.AddComponent<DauVetLocCuon>();
        dv.kieu = k; dv.kichThuoc = kt; dv.viTriGoc = go.transform.position;

        // ---- ho dat (moi loai) ----
        float banKinhHo;
        Color mauHo = MauDatXoi;
        switch (k)
        {
            case Kieu.Cay: banKinhHo = kt.x * HoCay; break;
            case Kieu.Bia: banKinhHo = Mathf.Max(kt.x, kt.y) * HoBia; break;
            case Kieu.Da: banKinhHo = Mathf.Max(kt.x, kt.y) * HoDa; break;
            case Kieu.NhaMo: banKinhHo = Mathf.Max(kt.x, kt.y) * HoNha; break;
            default: banKinhHo = Mathf.Max(kt.x, kt.y) * HoLo; mauHo = MauTro; break;
        }
        banKinhHo = Mathf.Max(0.35f, banKinhHo);
        dv.ho = TaoHoDat(go.transform, "HoDat" + rnd.Next(3), banKinhHo, rnd.Next(360), mauHo);

        // ---- phan rieng tung loai (vat lieu cua chinh vat) ----
        Transform tren = null;
        switch (k)
        {
            case Kieu.Cay:
                tren = TaoManh(go.transform, "GocCay" + rnd.Next(3), vlVat, new Vector3(kt.x, kt.x, kt.x), rnd.Next(360), -0.04f);
                break;
            case Kieu.Bia:
                tren = TaoManh(go.transform, "ChanBia" + rnd.Next(3), vlVat, new Vector3(kt.x, kt.x * 0.8f, Mathf.Max(0.12f, kt.y)), yaw, 0f);
                TaoManh(go.transform, "VunDa" + rnd.Next(3), vlVat, Vector3.one * Mathf.Max(kt.x, kt.y) * 0.9f, rnd.Next(360), 0f);
                break;
            case Kieu.Da:
                tren = TaoManh(go.transform, "VunDa" + rnd.Next(3), vlVat, Vector3.one * Mathf.Max(0.35f, Mathf.Max(kt.x, kt.y) * 0.75f), rnd.Next(360), 0f);
                break;
            case Kieu.NhaMo:
                tren = TaoManh(go.transform, "NenNha" + rnd.Next(2), vlVat, new Vector3(kt.x, 1.2f, kt.y), yaw, 0f);
                break;
            case Kieu.Lo:
                tren = TaoManh(go.transform, "NenLo" + rnd.Next(2), vlVat, Vector3.one * Mathf.Max(kt.x, kt.y), rnd.Next(360), 0f);
                break;
        }
        dv.goc = tren;
        SoLanTao++;
        DangCo.Add(dv);
        return dv;
    }

    static Transform TaoManh(Transform cha, string ten, Material vl, Vector3 scale, float yaw, float hz)
    {
        var m = Luoi(ten);
        if (m == null) return null;
        var go = new GameObject(ten);
        go.transform.SetParent(cha, false);
        go.transform.localPosition = new Vector3(0f, hz, 0f);
        go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        go.transform.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = m;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = vl;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        return go.transform;
    }

    /// <summary>Ho dat BAM DIA HINH: chep luoi chuan, moi dinh nang / ha theo mat dat Ground ngay duoi (do cao rieng cua ho van giu).</summary>
    static Transform TaoHoDat(Transform cha, string ten, float banKinh, float yaw, Color mau)
    {
        var mg = Luoi(ten);
        if (mg == null) return null;
        var go = new GameObject(ten);
        go.transform.SetParent(cha, false);
        go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        float s = banKinh;
        go.transform.localScale = new Vector3(s, Mathf.Min(s, 1.6f), s);
        Mesh m = mg;
        if (mg.isReadable)
        {
            m = Object.Instantiate(mg);
            m.name = ten + "_BamDat";
            var v = m.vertices;
            float y0 = cha.position.y, sy = go.transform.localScale.y;
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 w = go.transform.TransformPoint(new Vector3(v[i].x, 0f, v[i].z));
                float yd = MatDatY(w, y0);
                v[i].y += (yd - y0) / sy;
            }
            m.vertices = v;
            m.RecalculateNormals();
            m.RecalculateBounds();
        }
        go.AddComponent<MeshFilter>().sharedMesh = m;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = VatLieuDat();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // mau (dat xoi / tro) + lap anh 3,7 m nhu dia hinh: UV luoi = toa do (ban kinh 1) -> nhan ban kinh / 3,7
        if (mpb == null) mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", mau);
        float lap = banKinh / 3.7f;
        mpb.SetVector("_MainTex_ST", new Vector4(lap, lap, Random.value, Random.value));
        mr.SetPropertyBlock(mpb);
        return go.transform;
    }

    /// <summary>Vat moc lai: lun xuong roi xoa.</summary>
    public void Xoa()
    {
        if (lun >= 0f) return;
        lun = 0f;
        SoLanXoa++;
    }

    void Update()
    {
        if (lun < 0f) return;
        lun += Time.deltaTime;
        float k = Mathf.Clamp01(lun / GiayLun);
        float sau = 0.35f + 0.25f * Mathf.Max(kichThuoc.x, kichThuoc.y);
        transform.position = viTriGoc - Vector3.up * sau * k * k;
        if (lun >= GiayLun) Destroy(gameObject);
    }

    void OnDestroy()
    {
        DangCo.Remove(this);
        if (ho != null)
        {
            var mf = ho.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.name.EndsWith("_BamDat")) Destroy(mf.sharedMesh);
        }
    }
}
