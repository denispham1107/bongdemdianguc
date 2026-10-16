using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DAU VET VAT BI LOC XOAY CUON (16/10/2026, nguoi dung: "khi Loc xoay cuon bay len het moi do vat nhu bia mo, da, nha, cay coi thi de lai DAU VET
/// cua cac do vat da cuon bay len chu khong de mat dat trong tron"). Nguoi dung chon: RIENG TUNG LOAI, ton tai DEN KHI VAT MOC LAI (30 s).
///
/// Hinh dung bang Blender MCP (CongCu/Blender/dau_vet_loc_cuon.py -> Resources/KyNang/LocXoay/DauVetLoc.fbx, luoi CHUAN HOA, LAM SAN SUI):
///   CAY    - goc cay GAY lom chom + dam go + re ngan (GocCay0-2, ban kinh 1 = ban kinh than) tren ho dat xoi.
///   BIA MO - chan bia GAY (ChanBia0-2, 1 x 1 = chan bia) + da vun (VunDa0-2) + ho dat xoi.
///   DA     - ho dat lom + da vun nho.
///   NHA MO - nen do nat: be nen nut + chan tuong gay + dong gach (NenNha0-1, 1 x 1 = nen nha) + dat xoi.
///   LO LUA - vong da vo (NenLo0-1) tren nen TRO DEN.
///
/// ⚠️ TI LE THAT (16/10/2026 lan 2, nguoi dung: "goc cay to gap doi cay luc binh thuong, bia mo cung vay"): ban dau lay kich thuoc tu VA CHAM - capsule
/// cay co cai rong 3,94 m trong khi than ~1 m; BoxCollider bia: luoi FBX xoay san nen truc Z cuc bo la chieu DUNG (be day ra 1,7 m); tam nen nha lech
/// pivot 0,8-1,3 m; va cham da to x1,4; goc cay Blender loe chan x1,35 + re dai 2,7 ban kinh. Nay doc SO DO NUONG SAN tu CHINH LUOI cua tung vat
/// (menu 135b NuongKichThuocGoc -> KichThuocGoc.txt: tam + huong (PCA) + rong / day CHAN (0-0,3 m) + cao + ban kinh THAN o 0,3-1 m) - luc choi luoi
/// Read/Write tat nen khong do duoc. Vat khong co trong bang (khong thuoc Act2) thi uoc theo khung hinh.
///
/// ⚠️ SAN SUI + VET MAU (nguoi dung lan 2: "dau vet phai san sui, nham, co vet mau chu khong tron bong"): luoi lam san (chia canh + doi dinh), moi manh
/// ve THEM lop phu Diablo25D/VetMauPhu (vet mau + bui ban, chieu ba phia theo toa do the gioi) tren vat lieu cua chinh vat; ho dat dung anh rieng
/// DatXoi_Mau (dat xoi von cuc, ke nut, mau kho loang) + normal map DatXoi_Phap, do bong 0.
/// HO DAT BAM DIA HINH: moi dinh ban tia xuong lop Ground. Vong doi: hien luc vat BAT KHOI DAT (het pha rung nut 0,3 s - VatTheBiCuon), vat moc lai thi
/// LUN xuong 0,5 s roi xoa. Moi may tu dung, 0 byte mang; ngoai tam nhin TamNhin an.
/// ⚠️ VA CHAM (16/10/2026, nguoi dung: "nhan vat va quai khong the di xuyen qua dau vet; neu thap thi leo len roi leo xuong"): moi manh noi (goc cay, chan
/// bia, da vun, nen nha, vong lo) co MeshCollider dung hinh, lop Default nhu canh vat -> CharacterController (nguoi choi stepOffset 0,55 m, quai 0,4 m) bi
/// CHAN boi manh cao, BUOC LEN roi xuong manh thap. Ho dat (noi 5-7 cm, sat mat dat) KHONG va cham - di tren dia hinh nhu cu. Loc xoay KHONG cuon dau
/// vet (VatTheBiCuon.CuonDuoc). Vat moc lai: tat va cham NGAY khi bat dau lun.
/// </summary>
public class DauVetLocCuon : MonoBehaviour
{
    public enum Kieu { Cay, Bia, Da, NhaMo, Lo }

    public const string DuongFbx = "KyNang/LocXoay/DauVetLoc";
    public const string DuongDat = "KyNang/LocXoay/DatXoi";
    public const string DuongVetMau = "KyNang/LocXoay/VetMauPhu";
    public const string DuongBang = "KyNang/LocXoay/KichThuocGoc";
    /// <summary>Lun xuong bao lau khi vat moc lai.</summary>
    public const float GiayLun = 0.5f;
    /// <summary>Mau nhan cho ho dat: dat vua xoi / tro den (lo lua).</summary>
    public static readonly Color MauDatXoi = Color.white, MauTro = new Color(0.22f, 0.2f, 0.19f);
    /// <summary>Ban kinh ho dat theo kich thuoc vat: cay = x ban kinh than; con lai = x canh dai cua chan + them.</summary>
    public const float HoCay = 2.4f, HoBia = 0.55f, HoDa = 0.6f, HoNha = 0.55f, HoLo = 0.75f;
    public const float TiLeNenLo = 0.78f;

    // ---- thong ke cho phep thu ----
    public static int SoLanTao, SoLanXoa, SoLanCoSoDo;
    public static readonly List<DauVetLocCuon> DangCo = new List<DauVetLocCuon>();

    public Kieu kieu;
    /// <summary>Kich thuoc dung (m): Cay = (ban kinh than, ban kinh than); con lai = (rong, day) CHAN vat theo truc dai / ngan.</summary>
    public Vector2 kichThuoc;
    public bool coSoDo;
    public Transform goc, ho;

    float lun = -1f;
    Vector3 viTriGoc;

    /// <summary>So do nuong san cua mot vat (menu 135b).</summary>
    public struct SoDo { public Kieu kieu; public Vector2 tam; public float huong, rong, day, cao, banKinhThan, dayY; }

    static readonly Dictionary<string, Mesh> luoi = new Dictionary<string, Mesh>();
    static Dictionary<string, SoDo> bang;
    static Material vlDat, vlVetMau;
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

    // vat lieu tao luc chay / nap tu Resources bi xoa khi thoat Play - kiem bang null cua Unity (memory vat-lieu-static-bi-xoa-khi-thoat-play)
    static Material VatLieuDat() { if (vlDat == null) vlDat = Resources.Load<Material>(DuongDat); return vlDat; }
    static Material VatLieuVetMau() { if (vlVetMau == null) vlVetMau = Resources.Load<Material>(DuongVetMau); return vlVetMau; }

    public static string Khoa(string ten, Vector3 p) { return ten + "@" + Mathf.RoundToInt(p.x * 10f) + "," + Mathf.RoundToInt(p.z * 10f); }

    /// <summary>Bang so do nuong san (KichThuocGoc.txt): khoa = ten@x,z (lam tron 0,1 m).</summary>
    public static Dictionary<string, SoDo> Bang()
    {
        if (bang != null && bang.Count > 0) return bang;     // (rong thi nap lai - vua nuong menu 135b khi Play khong nap lai domain)
        bang = new Dictionary<string, SoDo>();
        var ta = Resources.Load<TextAsset>(DuongBang);
        if (ta == null) return bang;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var dong in ta.text.Split('\n'))
        {
            if (dong.Length < 5 || dong[0] == '#') continue;
            var c = dong.Trim().Split('|');
            if (c.Length < 10) continue;
            Kieu k;
            switch (c[1]) { case "Cay": k = Kieu.Cay; break; case "Bia": k = Kieu.Bia; break; case "Da": k = Kieu.Da; break; case "NhaMo": k = Kieu.NhaMo; break; default: k = Kieu.Lo; break; }
            bang[c[0]] = new SoDo
            {
                kieu = k, tam = new Vector2(float.Parse(c[2], ci), float.Parse(c[3], ci)), huong = float.Parse(c[4], ci),
                rong = float.Parse(c[5], ci), day = float.Parse(c[6], ci), cao = float.Parse(c[7], ci), banKinhThan = float.Parse(c[8], ci), dayY = float.Parse(c[9], ci)
            };
        }
        return bang;
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

    /// <summary>So do cua vat: tu bang nuong san; khong co thi UOC theo khung hinh (AABB the gioi - chi la du phong).</summary>
    public static SoDo LaySoDo(GameObject vat, Vector3 choCu, Bounds baoHinh, out bool coBang)
    {
        coBang = Bang().TryGetValue(Khoa(vat.name, choCu), out var sd);
        if (coBang) return sd;
        var k = PhanLoai(vat, baoHinh);
        sd = new SoDo { kieu = k, tam = new Vector2(choCu.x, choCu.z), huong = vat.transform.eulerAngles.y, rong = baoHinh.size.x, day = baoHinh.size.z, cao = baoHinh.size.y, dayY = baoHinh.min.y };
        sd.banKinhThan = Mathf.Min(baoHinh.size.x, baoHinh.size.z) * (k == Kieu.Cay ? 0.12f : 0.5f);
        return sd;
    }

    public static float MatDatY(Vector3 p, float neuKhong)
    {
        if (lopDat < 0) lopDat = LayerMask.GetMask("Ground");
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 30f, Vector3.down, out hit, 80f, lopDat, QueryTriggerInteraction.Ignore)) return hit.point.y;
        return neuKhong;
    }

    /// <summary>Dung dau vet tai cho cu cua vat (goi luc vat bat khoi dat).</summary>
    public static DauVetLocCuon Tao(GameObject vat, Vector3 choCu, Quaternion huongCu, Bounds baoHinh, Material vlVat)
    {
        if (vat == null || Luoi("HoDat0") == null) return null;
        var sd = LaySoDo(vat, choCu, baoHinh, out bool coBang);
        var k = sd.kieu;
        var rnd = new System.Random(Mathf.RoundToInt(choCu.x * 131f) ^ Mathf.RoundToInt(choCu.z * 977f));

        var go = new GameObject("DauVetLocCuon_" + k);
        Vector3 tam = new Vector3(sd.tam.x, choCu.y, sd.tam.y);
        tam.y = MatDatY(tam, sd.dayY);
        go.transform.position = tam;
        var dv = go.AddComponent<DauVetLocCuon>();
        dv.kieu = k; dv.viTriGoc = tam; dv.coSoDo = coBang;
        if (coBang) SoLanCoSoDo++;
        float dai = Mathf.Max(sd.rong, sd.day);
        dv.kichThuoc = k == Kieu.Cay ? new Vector2(sd.banKinhThan, sd.banKinhThan) : new Vector2(sd.rong, sd.day);

        // ---- ho dat ----
        float banKinhHo;
        Color mauHo = MauDatXoi;
        switch (k)
        {
            case Kieu.Cay: banKinhHo = sd.banKinhThan * HoCay; break;
            case Kieu.Bia: banKinhHo = dai * HoBia + 0.12f; break;
            case Kieu.Da: banKinhHo = dai * HoDa + 0.08f; break;
            case Kieu.NhaMo: banKinhHo = dai * HoNha; break;
            default: banKinhHo = dai * HoLo + 0.1f; mauHo = MauTro; break;
        }
        dv.ho = TaoHoDat(go.transform, "HoDat" + rnd.Next(3), Mathf.Max(0.25f, banKinhHo), rnd.Next(360), mauHo);

        // ---- phan rieng tung loai: vat lieu CUA VAT + lop phu vet mau ----
        Transform tren = null;
        switch (k)
        {
            case Kieu.Cay:
                tren = TaoManh(go.transform, "GocCay" + rnd.Next(3), vlVat, Vector3.one * sd.banKinhThan, rnd.Next(360), -0.03f);
                break;
            case Kieu.Bia:
                tren = TaoManh(go.transform, "ChanBia" + rnd.Next(3), vlVat, new Vector3(sd.rong, Mathf.Clamp(sd.cao * 0.55f, 0.12f, 1.2f), sd.day), sd.huong, 0f);
                TaoManh(go.transform, "VunDa" + rnd.Next(3), vlVat, Vector3.one * dai * 0.55f, rnd.Next(360), 0f);
                break;
            case Kieu.Da:
                tren = TaoManh(go.transform, "VunDa" + rnd.Next(3), vlVat, Vector3.one * Mathf.Max(0.08f, dai * 0.5f), rnd.Next(360), 0f);   // (lan 2: kep 0,18 -> da nho x1,5)
                break;
            case Kieu.NhaMo:
                tren = TaoManh(go.transform, "NenNha" + rnd.Next(2), vlVat, new Vector3(sd.rong, 1.2f, sd.day), sd.huong, 0f);
                break;
            case Kieu.Lo:
                // vong da NenLo ban kinh ngoai ~0,64 (luoi) -> x0,78 cho vong = chan lo (lan 2: x1 -> vong to x1,62 chan lo, menu 135 F)
                tren = TaoManh(go.transform, "NenLo" + rnd.Next(2), vlVat, Vector3.one * dai * TiLeNenLo, rnd.Next(360), 0f);
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
        // va cham DUNG HINH (luoi FBX doc duoc): manh cao chan, manh thap buoc len (stepOffset) - nguoi dung 16/10/2026
        if (m.isReadable) go.AddComponent<MeshCollider>().sharedMesh = m;
        var mr = go.AddComponent<MeshRenderer>();
        var vm = VatLieuVetMau();
        mr.sharedMaterials = vm != null ? new[] { vl, vm } : new[] { vl };      // vat lieu thu hai ve CHONG len cung luoi
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
            m.RecalculateTangents();
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
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;     // vat moc lai dung dung cho nay - khong chong va cham
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
