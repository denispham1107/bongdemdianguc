using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// XAC NAM TREN VUNG MAU (nguoi dung 26/09/2026): quai va nguoi choi bi giet thi thay XAC NAM HAN xuong dat,
/// duoi xac loang ra mot VUNG MAU NHO, xac nam de len vung mau.
///
/// Nguoi dung chon: xac QUAI nam 20 giay roi CHIM DAN xuong dat trong 2 giay (ca vung mau mo theo) - de lau
/// hon thi dien thoai phai ganh nhieu khung xuong; xac NGUOI CHOI nam toi het tran (toi da 3 xac, khong nang).
///
/// Truoc day: quai guc (cui nguoi, lun hong) roi bien mat sau 5-6 giay (<c>Damageable.corpseSeconds</c>, nay
/// khong dung nua); nguoi choi guc ngoi mai. Khong co mau.
///
/// NGA: LAT MODEL CON, khong lat goc - y cach <see cref="BiDanhNga"/> (xoay -90 do quanh truc X CUA GOC, nhan
/// ben trai rotGoc: dau nga ve SAU lung, nam ngua; nhan ben phai thi model Meshy xoay san 180 do se nam up).
/// Dau ve phia -forward cua goc, nen tam vung mau lui ve phia do <see cref="TamMauTheoCao"/> x chieu cao than.
/// Cac bo hoat hinh (ModelHoatHinh, NguoiChoiHoatHinh, ProceduralAnimator) thay co XacNam thi bo tu the guc
/// (cui nguoi 58 do + lun hong) - tu the ay nam ngua ra thanh ngoi day giua dat.
///
/// Dang bi danh nga / hat tung luc chet: lay VI TRI GOC ma hai hieu ung ay da cat, xoa BiDanhNga, nga tiep tu
/// goc dang co; con dang bay (BiHatTung) thi cong do cao cua no len va CHO roi het moi loang mau. Xac chet giua
/// long loc (RoiXuongDat) cung cho cham dat.
///
/// VUNG MAU: anh ve bang Blender MCP (<c>CongCu/Blender/vung_mau.blend</c> -> <c>Resources/KyNang/Mau/VungMau0-3</c>,
/// nen trong suot, mau chiem ban kinh <see cref="TiLeMauTrongAnh"/> cua nua anh, mep anh alpha 0 de phong to
/// UV khong keo vet). Dia ve bang <c>GroundRing.BuildDisc</c> - bam CHI lop Ground, khong treo len bia mo.
/// Loang ra trong <see cref="GiayLoangMau"/> giay bang cach thu UV (Mat _MainTex_ST), khong dung lai luoi.
/// Trong suot (hang doi 3000) ve SAU xac (2000) va nam thap hon xac -> xac tu nhien de len vung mau.
///
/// Chay sau BiDanhNga (10000) va BiHatTung (10001): hai cai ay con ghi hinh trong khung chet.
/// </summary>
[DefaultExecutionOrder(10002)]
public class XacNam : MonoBehaviour
{
    /// <summary>Thoi gian nga tu tu the dang co xuong nam han.</summary>
    public const float GiayNga = 0.55f;
    /// <summary>Xac QUAI nam bao lau truoc khi chim (nguoi dung chon 20 giay).</summary>
    public const float GiayNamQuai = 20f;
    /// <summary>Chim xuong dat trong bao lau, roi xoa han.</summary>
    public const float GiayChim = 2f;
    public const float DoSauChim = 1.3f;
    /// <summary>Mau loang tu nho den du trong bao lau (tinh tu luc xac cham dat).</summary>
    public const float GiayLoangMau = 2.4f;
    /// <summary>Nhac hinh len khi nam de lung khong chim vao dat (nhu BiDanhNga).</summary>
    public const float NangKhiNam = 0.16f;
    /// <summary>Ban kinh PHAN CO MAU = he so x chieu cao than (1,8 m -> 0,65 m: "vung mau nho").</summary>
    public const float BanKinhMauTheoCao = 0.36f;
    /// <summary>Tam vung mau lui ve phia dau = he so x chieu cao than (duoi lung / nguc).</summary>
    public const float TamMauTheoCao = 0.40f;
    /// <summary>Trong anh Blender, mau chiem ban kinh nay cua nua anh (0,60 + meo nhieu).</summary>
    public const float TiLeMauTrongAnh = 0.66f;
    /// <summary>Bat dau loang khi nho hon ti le nay (khong bat dau tu 0 - trong nhu mot cham).</summary>
    const float LoangTu = 0.22f;
    /// <summary>Tu xuong dau toi dinh chop dau.</summary>
    const float ChopDau = 0.10f;
    /// <summary>Xuong thap nhat cua xac nam phai cao hon mat dat chung nay (lung / vai day hon xuong).</summary>
    public const float DoDayLung = 0.10f;

    static readonly List<XacNam> ds = new List<XacNam>();
    public static int SoXac { get { return ds.Count; } }
    public static IList<XacNam> TatCa { get { return ds; } }

    public bool laNguoiChoi;
    public float caoThan = 1.8f;
    public float daTroi;
    /// <summary>Vung mau - phep thu doc. Null khi chua cham dat.</summary>
    public GameObject vungMau;
    public Vector3 TamVungMau { get; private set; }
    /// <summary>Ban kinh PHAN CO MAU (khong phai ca dia).</summary>
    public float BanKinhVungMau { get; private set; }
    /// <summary>Ti le loang hien tai 0..1 (1 = du co).</summary>
    public float Loang01 { get; private set; }
    public bool DangChim { get { return lucChim >= 0f; } }
    public Transform Hinh { get { return hinh; } }

    Transform hinh;
    Quaternion rotGoc, rotDau;
    Vector3 posGoc, posDau;
    Material matMau;
    float lucCoMau = -1f;
    float lucChim = -1f;
    Vector3 viTriTruocChim;
    int lopDat;
    /// <summary>
    /// CHONG LUN: xoay quanh goc o chan chi dung khi goc hinh nam o chan. Quy cay (model Meshy, goc lech) nam xuong thi
    /// 57,6% dinh chui duoi mat dat (menu 85). Moi khung trong luc nga do XUONG thap nhat so voi dat roi nhac len; nga xong
    /// thi chot mot con so (ca nang lan ha - xac lo lung cung sai).
    /// </summary>
    readonly List<Transform> xuong = new List<Transform>();
    /// <summary>-90 = NGA NGUA (dau ve -forward cua goc) - moi loai deu nga ngua nhu BiDanhNga.</summary>
    public const float GocNga = -90f;
    float nangChot = float.NaN;
    public float NangChongLun { get; private set; }

    static Texture2D[] anhMau;

    /// <summary>Gan luc chet (Damageable.Die). Da co thi tra ve cai cu.</summary>
    public static XacNam Gan(Damageable d)
    {
        if (d == null) return null;
        var x = d.GetComponent<XacNam>();
        if (x != null) return x;
        x = d.gameObject.AddComponent<XacNam>();
        x.laNguoiChoi = d.isPlayer;
        // Chieu cao than do tu XUONG THAT (dinh xuong cao nhat + chop dau): rig.bodyHeight lech xa hinh that - bo xuong
        // 2,36 / quy cay 2,89 m trong khi hinh cao 1,67 / 1,69 m (menu 85 do bang BakeMesh) -> vung mau to gap ruoi.
        float cao = x.xuong.Count > 0 ? x.XuongCaoNhat() - d.transform.position.y + ChopDau : 0f;
        if (cao <= 0.2f && d.rig != null) cao = d.rig.bodyHeight;
        if (cao <= 0.2f)
        {
            var cc = d.GetComponent<CharacterController>();
            if (cc != null) cao = cc.height;
        }
        x.caoThan = cao > 0.2f ? Mathf.Clamp(cao, 0.9f, 3f) : 1.8f;
        return x;
    }

    void Awake()
    {
        lopDat = LayerMask.GetMask("Ground");
        // Model = con dau tien co Animation; khong co thi con dau tien co hinh (nhu BiDanhNga)
        foreach (Transform c in transform)
            if (c.GetComponent<Animation>() != null) { hinh = c; break; }
        if (hinh == null)
            foreach (Transform c in transform)
                if (c.GetComponentInChildren<Renderer>() != null) { hinh = c; break; }
        if (hinh == null) return;
        var daCo = new HashSet<Transform>();
        foreach (var smr in hinh.GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (var b in smr.bones)
                if (b != null && daCo.Add(b)) xuong.Add(b);

        posDau = hinh.localPosition; rotDau = hinh.localRotation;
        posGoc = posDau; rotGoc = rotDau;
        // Dang nga / dang bay: tu the DUNG goc nam trong hai hieu ung ay, khong phai tu the luc nay
        var hat = GetComponent<BiHatTung>();
        if (hat != null && hat.CoPosGoc) posGoc = hat.PosGoc;
        var nga = GetComponent<BiDanhNga>();
        if (nga != null && nga.CoGoc)
        {
            posGoc = nga.PosGoc; rotGoc = nga.RotGoc;
            Destroy(nga);          // xac nga tiep tu goc dang co, khong chong day
        }
    }

    void OnEnable() { ds.Add(this); }
    void OnDisable() { ds.Remove(this); }

    void OnDestroy()
    {
        if (vungMau != null) Destroy(vungMau);
        if (matMau != null) Destroy(matMau);
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        daTroi += dt;

        var hat = GetComponent<BiHatTung>();
        if (hinh != null)
        {
            // Nga nhanh dan (nhu roi tu do), khong em dan ve cuoi
            float u = Mathf.Clamp01(daTroi / GiayNga);
            u = u * u;
            Quaternion rotNam = Quaternion.Euler(GocNga, 0f, 0f) * rotGoc;
            hinh.localRotation = Quaternion.Slerp(rotDau, rotNam, u);
            Vector3 p = Vector3.Lerp(posDau, posGoc + Vector3.up * NangKhiNam, u);
            if (hat != null) p += Vector3.up * hat.CaoHienTai;     // chet giua luc bi hat: van roi het duong
            hinh.localPosition = p;

            // Chong lun: dang nga thi chi nhac (khong de chui dat), nga xong chot mot lan (nhac hoac ha)
            if (float.IsNaN(nangChot))
            {
                float datY, can = 0f;
                if (xuong.Count > 0 && MatDat(transform.position, out datY))
                    can = datY + DoDayLung - XuongThapNhat();
                if (daTroi >= GiayNga && hat == null && GetComponent<RoiXuongDat>() == null)
                    nangChot = Mathf.Clamp(can, -0.3f, 1.2f);
                NangChongLun = float.IsNaN(nangChot) ? Mathf.Max(0f, can) : nangChot;
            }
            hinh.localPosition = p + Vector3.up * NangChongLun;
        }

        // Cham dat moi loang mau: xac chet trong loc (RoiXuongDat) hay dang bi hat (BiHatTung) thi cho
        if (vungMau == null && lucChim < 0f && daTroi >= GiayNga && hat == null
            && GetComponent<RoiXuongDat>() == null)
        {
            float datY;
            if (MatDat(transform.position, out datY) && transform.position.y - datY < 0.45f)
                TaoVungMau(datY);
        }

        if (matMau != null)
        {
            float k = Mathf.Clamp01((Time.time - lucCoMau) / GiayLoangMau);
            k = 1f - (1f - k) * (1f - k) * (1f - k);          // loang nhanh luc dau, cham dan
            Loang01 = Mathf.Lerp(LoangTu, 1f, k);
            float s = 1f / Loang01;
            matMau.mainTextureScale = new Vector2(s, s);
            matMau.mainTextureOffset = new Vector2(0.5f - 0.5f * s, 0.5f - 0.5f * s);
        }

        // Xac quai: nam du 20 giay thi chim dan xuong dat cung vung mau roi xoa
        if (!laNguoiChoi)
        {
            if (lucChim < 0f && daTroi >= GiayNamQuai)
            {
                lucChim = Time.time;
                viTriTruocChim = transform.position;
            }
            if (lucChim >= 0f)
            {
                float c = Mathf.Clamp01((Time.time - lucChim) / GiayChim);
                transform.position = viTriTruocChim + Vector3.down * (DoSauChim * c * c);
                if (matMau != null)
                {
                    var mau = matMau.GetColor("_TintColor");
                    mau.a = MauVung.a * (1f - c);
                    matMau.SetColor("_TintColor", mau);
                }
                if (c >= 1f) Destroy(gameObject);
            }
        }
    }

    /// <summary>Mau nhan len anh (anh da sam san, chi giam alpha mot chut cho thay dat duoi).</summary>
    public static readonly Color MauVung = new Color(HeSoSangMau, HeSoSangMau, HeSoSangMau, 0.94f);
    /// <summary>Do sang nhan len anh mau (anh Blender da sam; 1,0 thi giua dem do choi nhu phat sang).</summary>
    public const float HeSoSangMau = 0.6f;

    float XuongThapNhat()
    {
        float m = float.MaxValue;
        for (int i = 0; i < xuong.Count; i++) if (xuong[i] != null) m = Mathf.Min(m, xuong[i].position.y);
        return m;
    }

    float XuongCaoNhat()
    {
        float m = float.MinValue;
        for (int i = 0; i < xuong.Count; i++) if (xuong[i] != null) m = Mathf.Max(m, xuong[i].position.y);
        return m;
    }

    bool MatDat(Vector3 p, out float y)
    {
        RaycastHit h;
        if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out h, 12f, lopDat, QueryTriggerInteraction.Ignore))
        { y = h.point.y; return true; }
        y = p.y;
        return false;
    }

    /// <summary>Tam vung mau: lui ve phia DAU - nga ngua dau ve -forward cua goc, nga sap ve +forward.</summary>
    public static Vector3 TamMauCho(Transform goc, float caoThan, float gocNga)
    {
        Vector3 f = goc.forward; f.y = 0f;
        if (f.sqrMagnitude < 1e-4f) f = Vector3.forward;
        f.Normalize();
        return goc.position + f * (Mathf.Sign(gocNga) * TamMauTheoCao * caoThan);
    }

    void TaoVungMau(float datY)
    {
        Vector3 tam = TamMauCho(transform, caoThan, GocNga);
        tam.y = datY;
        TamVungMau = tam;
        BanKinhVungMau = BanKinhMauTheoCao * caoThan;
        float banKinhDia = BanKinhVungMau / TiLeMauTrongAnh;

        var go = new GameObject("VungMau");
        go.transform.position = tam;
        var mesh = new Mesh { name = "VungMau" };
        GroundRing.BuildDisc(mesh, tam, banKinhDia, 24, 3, 0.03f);
        // Xoay anh mot goc ngau nhien quanh tam (UV quanh 0,5) - bon anh x moi goc thi hai xac canh nhau khong giong nhau
        var uv = mesh.uv;
        float a = Random.Range(0f, Mathf.PI * 2f), ca = Mathf.Cos(a), sa = Mathf.Sin(a);
        for (int i = 0; i < uv.Length; i++)
        {
            Vector2 d = uv[i] - new Vector2(0.5f, 0.5f);
            uv[i] = new Vector2(0.5f + d.x * ca - d.y * sa, 0.5f + d.x * sa + d.y * ca);
        }
        mesh.uv = uv;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        matMau = TaoVatLieuMau(AnhMau(Random.Range(0, 4)));
        mr.sharedMaterial = matMau;
        vungMau = go;
        lucCoMau = Time.time;
        Loang01 = LoangTu;
    }

    /// <summary>
    /// Vat lieu vung mau: shader trong suot KHONG anh sang (nhu vet chay), NHAN TOI <see cref="HeSoSangMau"/>.
    /// Da thu hai dau: nhan 1,0 thi giua dem vung mau do tuoi nhu phat sang; dung shader NHAN anh sang (Legacy
    /// Transparent/Specular) thi anh trang xanh cua dem lam mau den kit, khong con thay vung mau (menu 85, 26/09/2026).
    /// </summary>
    static Material TaoVatLieuMau(Texture2D anh)
    {
        return Mats.Alpha("M_VungMau", anh, MauVung);
    }

    static Texture2D AnhMau(int i)
    {
        if (anhMau == null) anhMau = new Texture2D[4];
        // Kiem bang null cua Unity: anh nap luc Play bi go khi thoat Play (xem vat-lieu-static-bi-xoa-khi-thoat-play)
        if (anhMau[i] == null)
        {
            anhMau[i] = Resources.Load<Texture2D>("KyNang/Mau/VungMau" + i);
            if (anhMau[i] == null) anhMau[i] = TextureFactory.ScorchMark();
            anhMau[i].wrapMode = TextureWrapMode.Clamp;
        }
        return anhMau[i];
    }

    /// <summary>Doi Damageable nay nam la xac chua - bo hoat hinh goi de bo tu the guc.</summary>
    public static bool LaXac(Component c) { return c != null && c.GetComponent<XacNam>() != null; }
}
