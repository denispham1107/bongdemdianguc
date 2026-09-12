using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CAY, DA, BIA MO BI LOC XOAY CUON DI.
///
/// Con loc di qua thi boc luon canh vat len: vat the bay vong quanh than loc
/// giong het ke dich bi cuon (<see cref="WhirledEffect"/>), roi TAN BIEN cung
/// voi con loc. Sau <see cref="GiayHoiSinh"/> giay no moc lai o dung cho cu.
///
/// Vi sao khong xoa han vat the roi tao lai:
///
///   - Vat the trang tri duoc dung khi vao man, khong co prefab de tao lai.
///     Xoa la mat vinh vien.
///   - Va cai dong ho 30 giay phai chay o dau do. Neu SetActive(false) thi
///     Update cua chinh no khong chay nua va khong bao gio co ai danh thuc no.
///
/// Nen "bien mat" o day la TAT Renderer va Collider, GameObject van song va van
/// dem gio. Het gio thi bat lai va tra ve toa do cu.
/// </summary>
public class VatTheBiCuon : MonoBehaviour
{
    /// <summary>Bao lau sau khi bi cuon mat thi vat the moc lai o cho cu.</summary>
    public const float GiayHoiSinh = 30f;

    /// <summary>Vat rong ngang hon chung nay met thi khong cuon - do la vach da, nen nha.</summary>
    public const float RongToiDa = 6f;

    /// <summary>Lo lua vua bi dap tat thi doi chung nay giay moi cuon len.</summary>
    public const float ChoSauKhiTat = 0.35f;

    public Tornado tornado;

    [Header("Quy dao")]
    public float angle;
    public float orbitRadius = 2f;
    public float targetHeight = 5f;
    public float height;
    public float riseSpeed = 4.5f;
    public float suckSpeed = 7f;

    Vector3 choCu;
    Quaternion huongCu;
    Vector3 truc;
    float tocDoLon;

    float dongHo = -1f;              // < 0 = chua bien mat
    bool daAn;

    readonly List<Renderer> hinh = new List<Renderer>();
    readonly List<Collider> vaCham = new List<Collider>();

    // ================================================================
    //  CHON VAT DE CUON
    // ================================================================

    /// <summary>
    /// Vat the nay co cuon duoc khong.
    ///
    /// Danh sach loai tru dai, nhung moi dong deu co ly do: cuon nham mot trong
    /// nhung thu duoi day thi hong han mot thu khac trong game.
    /// </summary>
    public static bool CuonDuoc(GameObject go, Tornado loc)
    {
        if (go == null || loc == null) return false;
        if (go.GetComponent<VatTheBiCuon>() != null) return false;   // dang bi cuon roi

        // Quai va nguoi choi da co WhirledEffect lo - cuon o day nua thi hai
        // co che cung dat vi tri cho no, va no giat lien hoi giua hai quy dao.
        if (go.GetComponentInParent<Damageable>() != null) return false;

        // Dia hinh: cuon ca qua doi len troi.
        if (go.GetComponent<Terrain>() != null) return false;

        // Chinh con loc, hoac mot bo phan cua no.
        if (go.transform.IsChildOf(loc.transform)) return false;

        // Cay dang boc chay: hai co che cung muon an vat the di roi tra lai sau
        // mot khoang thoi gian, ma moi cai nho mot cho cu va mot cai dong ho
        // rieng. De ca hai cung chay tren mot cai cay thi cai nao het gio sau
        // se bat lai mot cai cay ma cai kia da tat, hoac nguoc lai.
        if (go.GetComponentInParent<CayChay>() != null) return false;

        // LO LUA: cuon duoc, nhung phai DAP TAT LUA TRUOC (Tornado.CuonVatThe
        // lo viec ay khi con loc cham toi lo). Lua con chay thi chua cuon; lua
        // vua tat cung doi <see cref="ChoSauKhiTat"/> giay de nguoi choi kip
        // thay no tat truoc khi ca cai lo bay len troi.
        var lo = go.GetComponent<LoLuaDa>();
        if (lo != null)
        {
            if (lo.DangChay) return false;
            if (lo.GiayTuLucTat < ChoSauKhiTat) return false;
        }
        // Hieu ung dang chay (lua, khoi, vong phep) - cuon len thi vua vo ly
        // vua lam hong vong doi cua chung.
        else if (go.GetComponentInChildren<ParticleSystem>(true) != null) return false;
        if (go.GetComponent<GroundRing>() != null) return false;

        // Khong co gi de nhin thay thi cuon cung bang khong.
        var r = go.GetComponentInChildren<Renderer>();
        if (r == null) return false;

        // Qua to: vach da bao quanh dau truong, nen nha. Do theo be NGANG chu
        // khong theo chieu cao - cay cao 15 m van cuon duoc, ma cuon cay len moi
        // dung la con loc.
        //
        // Do theo VA CHAM, khong theo hinh ve. Bounds cua hinh om ca TAN LA:
        // mot cay soi Act2 rong 22 m theo hinh nhung than chi 3 m. Lay bounds
        // hinh thi moi cai cay trong game deu bi loai vi "qua to", va con loc
        // di qua ca khu rung ma khong boc noi mot cai gi.
        var col = go.GetComponent<Collider>();
        Bounds bao;
        if (col != null) bao = col.bounds;
        else
        {
            bao = r.bounds;
            foreach (var r2 in go.GetComponentsInChildren<Renderer>()) bao.Encapsulate(r2.bounds);
        }
        if (Mathf.Max(bao.size.x, bao.size.z) > RongToiDa) return false;

        return true;
    }

    /// <summary>Hut mot vat the vao con loc.</summary>
    public static VatTheBiCuon Cuon(GameObject go, Tornado loc)
    {
        if (!CuonDuoc(go, loc)) return null;

        var v = go.AddComponent<VatTheBiCuon>();
        v.tornado = loc;

        Vector3 off = go.transform.position - loc.transform.position;
        v.angle = Mathf.Atan2(off.z, off.x) * Mathf.Rad2Deg;
        v.orbitRadius = Mathf.Max(0.8f, new Vector2(off.x, off.z).magnitude);
        v.height = Mathf.Max(0f, go.transform.position.y - loc.transform.position.y);
        v.targetHeight = Random.Range(loc.liftHeight * 0.35f, loc.liftHeight * 1.15f);
        return v;
    }

    void Start()
    {
        choCu = transform.position;
        huongCu = transform.rotation;

        truc = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f),
                           Random.Range(-1f, 1f)).normalized;
        tocDoLon = Random.Range(90f, 260f);

        // Tat va cham NGAY: mot goc cay dang bay ngang tam nguoi ma van chan
        // duong thi nguoi choi dam vao khong khi.
        GetComponentsInChildren(true, vaCham);
        for (int i = 0; i < vaCham.Count; i++)
            if (vaCham[i] != null) vaCham[i].enabled = false;

        GetComponentsInChildren(true, hinh);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // ---- Da bien mat: chi con dem gio cho toi luc moc lai ----
        if (daAn)
        {
            dongHo -= dt;
            if (dongHo <= 0f) MocLai();
            return;
        }

        // Loc tan (bi xoa) ma chua kip goi TanBien
        if (tornado == null) { TanBien(); return; }

        angle += tornado.spinDegreesPerSecond * dt;
        height = Mathf.MoveTowards(height, targetHeight, riseSpeed * dt);

        float wanted = tornado.FunnelRadiusAt(height);
        orbitRadius = Mathf.MoveTowards(orbitRadius, wanted, suckSpeed * dt);

        float rad = angle * Mathf.Deg2Rad;
        Vector3 want = tornado.transform.position
                     + new Vector3(Mathf.Cos(rad) * orbitRadius, height, Mathf.Sin(rad) * orbitRadius);

        transform.position = Vector3.Lerp(transform.position, want, Mathf.Clamp01(suckSpeed * dt));
        transform.Rotate(truc, tocDoLon * dt, Space.World);
    }

    /// <summary>
    /// Tan bien cung con loc: an vat the di va bat dau dem 30 giay.
    /// </summary>
    public void TanBien()
    {
        if (daAn) return;
        daAn = true;
        dongHo = GiayHoiSinh;

        // Mot nhum bui bay ra dung cho no vua bien mat, cho khoi "tat den" dot ngot
        VfxFactory.HitBurst(transform.position, new Color(0.72f, 0.66f, 0.52f));

        for (int i = 0; i < hinh.Count; i++)
            if (hinh[i] != null) hinh[i].enabled = false;

        // Dua ve cho cu NGAY tu bay gio, luc dang khong ai nhin thay.
        //
        // Doi den luc moc lai moi dat vi tri thi trong suot 30 giay ay vat the
        // van "nam" o giua troi: moi truy van vat ly, moi phep tinh khoang cach
        // deu lay toa do tren khong do.
        transform.position = choCu;
        transform.rotation = huongCu;
    }

    void MocLai()
    {
        transform.position = choCu;
        transform.rotation = huongCu;

        for (int i = 0; i < hinh.Count; i++)
            if (hinh[i] != null) hinh[i].enabled = true;
        for (int i = 0; i < vaCham.Count; i++)
            if (vaCham[i] != null) vaCham[i].enabled = true;

        VfxFactory.HitBurst(transform.position + Vector3.up * 0.4f,
                            new Color(0.72f, 0.66f, 0.52f));

        // Lo lua moc lai thi chay tiep - loc xoay da dap tat no truoc khi cuon
        var lo = GetComponent<LoLuaDa>();
        if (lo != null) lo.Chay();

        Destroy(this);
    }

    void OnDestroy()
    {
        // Phong khi bi xoa giua chung (doi man chang han): tra lai hinh va va
        // cham, khong de mot goc cay vo hinh dung chan duong mai mai.
        for (int i = 0; i < hinh.Count; i++)
            if (hinh[i] != null) hinh[i].enabled = true;
        for (int i = 0; i < vaCham.Count; i++)
            if (vaCham[i] != null) vaCham[i].enabled = true;
    }
}
