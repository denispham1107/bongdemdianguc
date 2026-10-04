using UnityEngine;

/// <summary>
/// TU THE PHU THUY TRUNG BAY O MAN CHINH (nguoi dung 04/10/2026: "2 ban tay cua phu thuy ngua len troi va hoi co cao len 1 chut; 1 ban
/// tay phat sang qua cau bang, 1 ban tay phat sang qua cau lua"; chon: lua ben PHAI man hinh / bang ben TRAI (luc nhan vat quay mat ve
/// may quay), QUA CAU Y NHU TRONG TRAN thu nho, ban tay ngang BUNG chia sang hai ben).
///
/// Chay SAU NguoiChoiHoatHinh (no dat tu the dung + tho moi khung): ngam huong canh tay tren (buong xuong, hoi ra ngoai, hoi ra truoc)
/// va cang tay (ra truoc + ra ngoai, hoi len) bang NguoiChoiHoatHinh.NgamHuongKhop - cung cach TuTheGiatSet, khong doan goc Euler theo
/// truc khop Meshy. Roi XOAN cang tay quanh truc cua no + be co tay cho LONG BAN TAY NGUA LEN.
/// Khung xuong Meshy khong co xuong ngon tay: phap tuyen long ban tay (toa do cuc bo xuong Hand) DO bang menu 101 (xoay xuong Hand,
/// BakeMesh hai lan, PCA cac dinh di theo) va chon dau bang anh chup can menu 101b (phia co nhan o mu ngon la MU tay).
/// Hai qua cau: VfxFactory.BuildFireballVisual / BuildQuaCauBangVisual (dung hinh cua ky nang that), ban kinh BanKinhCau, lo lung tren
/// long ban tay, nhap nho nhe. Gan luc chay tu MainMenuUI.Start - KHONG sua scene MainMenu (dung bang menu 51).
/// </summary>
[DefaultExecutionOrder(10020)]
public class TuTheTrungBay : MonoBehaviour
{
    /// <summary>Phap tuyen LONG ban tay trong toa do cuc bo xuong LeftHand / RightHand (menu 101 + 101b).</summary>
    public static readonly Vector3 LongTayTrai = new Vector3(0.720f, 0.144f, 0.679f), LongTayPhai = new Vector3(-0.754f, 0.204f, 0.624f);

    /// <summary>Tam long ban tay trong toa do cuc bo xuong Hand (tam cac dinh ban tay, menu 101).</summary>
    public static readonly Vector3 TamLongTay = new Vector3(0f, 0.11f, -0.007f);

    /// <summary>Ban kinh qua cau tren tay (m) va qua cau lo lung cach long tay bao nhieu.</summary>
    public const float BanKinhCau = 0.13f, CachLongTay = 0.07f;

    /// <summary>Den cua qua cau tren tay: tam (m) va do sang toi da. Den cua hinh ky nang dat cho qua cau BAY trong tran (lua: 6 / 12 m)
    /// - de nguyen thi nhuom cam ca nen dat man chinh (anh menu 101c lan dau); o day chi can hat sang len tay, ao va mat dat quanh chan.</summary>
    public const float TamDenCau = 2.6f, DoSangDenCau = 2.2f;

    /// <summary>Nhan vat vao tu the trong bao lau (giay).</summary>
    public const float GiayVaoTuThe = 0.8f;

    NguoiChoiHoatHinh hh;
    Transform tayTraiTren, tayTraiDuoi, banTayTrai, tayPhaiTren, tayPhaiDuoi, banTayPhai;
    Transform cauLua, cauBang;
    float batDau;

    /// <summary>Phep thu (menu 101c) doc: tay nao cam lua.</summary>
    public Transform BanTayLua { get; private set; }
    public Transform BanTayBang { get; private set; }

    void Start()
    {
        hh = GetComponentInChildren<NguoiChoiHoatHinh>();
        if (hh == null) { enabled = false; return; }
        tayTraiTren = hh.tayTraiTren; tayTraiDuoi = hh.tayTraiDuoi; tayPhaiTren = hh.tayPhaiTren; tayPhaiDuoi = hh.tayPhaiDuoi;
        banTayTrai = hh.banTayTrai != null ? hh.banTayTrai : TimBanTay(tayTraiDuoi);
        banTayPhai = hh.banTayPhai != null ? hh.banTayPhai : TimBanTay(tayPhaiDuoi);
        if (tayTraiTren == null || tayTraiDuoi == null || banTayTrai == null || tayPhaiTren == null || tayPhaiDuoi == null || banTayPhai == null)
        { enabled = false; return; }
        batDau = Time.time;

        // Lua ben PHAI man hinh khi nhan vat quay mat ve may quay: nhan vat nhin vao may quay thi tay TRAI cua no nam ben phai man hinh
        // (menu 51 dat nhan vat quay mat ve may quay; MainMenuUI cho nhan vat xoay cham 18 do/giay nen luc quay lung thi doi ben)
        BanTayLua = banTayTrai; BanTayBang = banTayPhai;

        var goLua = new GameObject("CauLuaTrenTay");
        VfxFactory.BuildFireballVisual(goLua.transform, BanKinhCau);
        cauLua = goLua.transform;
        var goBang = new GameObject("CauBangTrenTay");
        VfxFactory.BuildQuaCauBangVisual(goBang.transform, BanKinhCau);
        cauBang = goBang.transform;
        // Tia lua Sparks cua qua cau BAY co trong luc 0,35 (rac lua xuong duong bay) - dung yen tren tay thi thanh vet chay XUONG dat
        // (anh can menu 101c lan hai): cho boc LEN nhu tan lua
        foreach (var ps in goLua.GetComponentsInChildren<ParticleSystem>(true))
            if (ps.name == "Sparks") { var m = ps.main; m.gravityModifier = -0.25f; }
        foreach (var goCau in new[] { goLua, goBang })
            foreach (var lt in goCau.GetComponentsInChildren<Light>(true))
            {
                var fl = lt.GetComponent<LightFlicker>();
                if (fl != null) { fl.baseIntensity = Mathf.Min(fl.baseIntensity, DoSangDenCau); fl.DatTamGoc(Mathf.Min(lt.range, TamDenCau)); }
                else { lt.intensity = Mathf.Min(lt.intensity, DoSangDenCau); lt.range = Mathf.Min(lt.range, TamDenCau); }
            }
        CapNhatCau();
    }

    static Transform TimBanTay(Transform cangTay)
    {
        if (cangTay == null) return null;
        for (int i = 0; i < cangTay.childCount; i++) if (cangTay.GetChild(i).name.Contains("Hand")) return cangTay.GetChild(i);
        return null;
    }

    void LateUpdate()
    {
        float w = Mathf.SmoothStep(0f, 1f, (Time.time - batDau) / GiayVaoTuThe);
        Transform goc = hh.transform;
        Vector3 f = goc.forward, r = goc.right, u = Vector3.up;
        DatTay(tayTraiTren, tayTraiDuoi, banTayTrai, LongTayTrai, f, r, u, w, 0f);
        DatTay(tayPhaiTren, tayPhaiDuoi, banTayPhai, LongTayPhai, f, r, u, w, 1.7f);
        CapNhatCau();
    }

    void DatTay(Transform tren, Transform duoi, Transform tay, Vector3 longCucBo, Vector3 f, Vector3 r, Vector3 u, float w, float pha)
    {
        // ben ngoai: phia cua ban tay so voi truc than
        Vector3 ngoai = Vector3.Dot(tay.position - hh.transform.position, r) >= 0f ? r : -r;
        float tho = Mathf.Sin(Time.time * 1.3f + pha) * 0.03f;
        // canh tay tren buong xuong, hoi ra ngoai + ra truoc; cang tay ra truoc + ra hai ben, hoi len -> ban tay ngang bung
        Vector3 huongTren = (-u * 0.85f + ngoai * 0.40f + f * 0.22f).normalized;
        Vector3 huongDuoi = (f * 0.72f + ngoai * 0.55f + u * (0.14f + tho)).normalized;
        NguoiChoiHoatHinh.NgamHuongKhop(tren, duoi, huongTren, w);
        NguoiChoiHoatHinh.NgamHuongKhop(duoi, tay, huongDuoi, w);

        // XOAN cang tay quanh truc cua no cho phap tuyen long ban tay quay len troi (khong doi vi tri ban tay)
        Vector3 truc = (tay.position - duoi.position).normalized;
        Vector3 longTG = tay.TransformDirection(longCucBo);
        Vector3 a = Vector3.ProjectOnPlane(longTG, truc), b = Vector3.ProjectOnPlane(u, truc);
        if (a.sqrMagnitude > 1e-6f && b.sqrMagnitude > 1e-6f)
            duoi.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(a, b, truc) * w, truc) * duoi.rotation;
        // be co tay phan con lai (cang tay hoi nghieng len nen long tay con lech vai do)
        longTG = tay.TransformDirection(longCucBo);
        tay.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(longTG, u), w) * tay.rotation;
    }

    void CapNhatCau()
    {
        DatCau(cauLua, BanTayLua, BanTayLua == banTayTrai ? LongTayTrai : LongTayPhai, 0f);
        DatCau(cauBang, BanTayBang, BanTayBang == banTayTrai ? LongTayTrai : LongTayPhai, 2.1f);
    }

    void DatCau(Transform cau, Transform tay, Vector3 longCucBo, float pha)
    {
        if (cau == null || tay == null) return;
        Vector3 tam = tay.TransformPoint(TamLongTay);
        Vector3 len = tay.TransformDirection(longCucBo).normalized;
        float nhun = Mathf.Sin(Time.time * 2.1f + pha) * 0.015f;
        cau.position = tam + len * (BanKinhCau + CachLongTay + nhun);
    }

    void OnDestroy()
    {
        if (cauLua != null) Destroy(cauLua.gameObject);
        if (cauBang != null) Destroy(cauBang.gameObject);
    }
}
