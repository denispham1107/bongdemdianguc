using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TAM NHIN 25 m + SUONG CHIEN TRANH (nguoi dung 10/10/2026: "keo camera ra xa nhin tu dau map toi cuoi map - chinh tam nhin chi 25 m,
/// vuot qua co suong mu bao phu nhu StarCraft 2"; chon kieu SC2 = canh vat ngoai tam van thay nhung TOI + mo, KHONG thay quai / nguoi /
/// BOT / hieu ung; che do Doi CHIA SE tam nhin dong doi; chet roi giu 25 m quanh xac; may BOT cung chi thay trong 25 m).
///
/// HINH (chi may nay, khong goi tin nao):
///   - Nguon nhin = nhan vat cua may nay (song hay da chet - quanh xac) + dong doi con song (Doi). Toi da 4 nguon -> bien toan cuc
///     _TN_Tam[] cho SuongChienTranh.cginc. Moi shader canh vat (DaMo, vo cay, nuoc, vuc dia nguc, ChuanSuong thay Standard) tu toi /
///     nhat mau / phu may ngay tren diem anh - khong them luot ve (khong co anh do sau - them vao la ve lai ca canh, nang cho dien thoai).
///   - Dia hinh (shader Unity, khong sua): mot TAM LUOI BAM DAT (S_SuongDat) ve de len.
///   - AN moi vat DONG ngoai tam: renderer sinh ra sau khi vao tran (ky nang, hieu ung, binh roi...) + moi renderer thuoc Damageable
///     (quai, nguoi choi khac, BOT, xac) tru cua minh / dong doi -> Renderer.forceRenderingOff (co RIENG, khong dung toi .enabled ma
///     code hieu ung tu bat tat). Den dong ngoai tam (tinh ca tam chieu) -> cullingMask = 0. Ten tren dau, so sat thuong, dau hieu nga
///     hoi <see cref="ThayDuoc"/> truoc khi ve.
///   - Dang chay phep thu (co ChayThuMang) thi TAT phan hinh (anh chup cua cac menu cu khong doi) - tru khi <see cref="BatTrongPhepThu"/>.
/// LOGIC (moi luc): <see cref="NhinThay"/> - may BOT chi chon muc tieu trong tam nhin cua no / dong doi (BotDieuKhien).
/// </summary>
[DefaultExecutionOrder(900)]
public class TamNhin : MonoBehaviour
{
    public const float BanKinh = 25f;
    public const float Mem = 3f;
    /// <summary>Phan sang con lai trong suong, muc mat mau, mau may troi (a = do dam).</summary>
    public const float DoToi = 0.32f, NhatMau = 0.75f;
    public static readonly Color MauMay = new Color(0.035f, 0.045f, 0.065f, 0.55f);
    const float NhipQuet = 0.2f;

    public static TamNhin Hien { get; private set; }
    /// <summary>Phan hinh dang bat (suong + an vat).</summary>
    public static bool Bat { get; private set; }
    /// <summary>Phep thu (menu 117) bat de do phan hinh khi co ChayThuMang.</summary>
    public static bool BatTrongPhepThu;

    static readonly Vector4[] tam = new Vector4[4];
    static int soTam;

    // So dem cho phep thu
    public static int SoRendererDangAn, SoDenDangTat, SoVatLieuDaDoi;
    public static Renderer TamSuongDat { get; private set; }

    readonly HashSet<Renderer> canhVat = new HashSet<Renderer>();
    /// <summary>Canh vat TRONG SUOT / hat (lua lo, tan lua, khoi vuc...): khong phu suong trong shader duoc (cong sang) -> AN ngoai tam.
    /// Mat nuoc (NuocDam tu phu suong) va tam suong dat khong tinh.</summary>
    readonly List<Renderer> canhTrong = new List<Renderer>();
    readonly List<Light> denCanhDiem = new List<Light>();
    readonly HashSet<Light> denCanh = new HashSet<Light>();
    readonly HashSet<Renderer> dangAn = new HashSet<Renderer>();
    readonly Dictionary<Light, int> matNaGoc = new Dictionary<Light, int>();
    float lucQuet, lucKiemPhepThu;
    bool coPhepThu;

    public static TamNhin Gan(GameObject go)
    {
        var t = go.GetComponent<TamNhin>();
        if (t == null) t = go.AddComponent<TamNhin>();
        return t;
    }

    void Awake() { Hien = this; }

    void Start()
    {
        // Canh vat co san luc vao tran: KHONG bao gio an (tru renderer cua Damageable) - chung tu phu suong trong shader
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include))
            if (r.GetComponentInParent<Damageable>() == null) canhVat.Add(r);
        foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Include))
            if (l.GetComponentInParent<Damageable>() == null) denCanh.Add(l);
        DoiVatLieuStandard();
        DungTamSuongDat();
        foreach (var r in canhVat)
            if (r != null && r != TamSuongDat && LaTrongSuot(r)) canhTrong.Add(r);
        foreach (var l in denCanh)
            if (l != null && l.type != LightType.Directional) denCanhDiem.Add(l);
        Shader.SetGlobalFloat("_TN_BanKinh", BanKinh);
        Shader.SetGlobalFloat("_TN_Mem", Mem);
        Shader.SetGlobalFloat("_TN_DoToi", DoToi);
        Shader.SetGlobalFloat("_TN_NhatMau", NhatMau);
        Shader.SetGlobalVector("_TN_MauMay", MauMay);
        KiemPhepThu();
    }

    void OnDestroy()
    {
        Shader.SetGlobalFloat("_TN_Bat", 0f);
        Bat = false; soTam = 0;
        if (Hien == this) Hien = null;
    }

    void KiemPhepThu()
    {
        coPhepThu = FindAnyObjectByType<ChayThuMang>() != null;
        lucKiemPhepThu = Time.time + 1f;
    }

    // ================================================================
    //  NGUON NHIN
    // ================================================================

    /// <summary>Nhan vat cua may nay (GameDirector.player).</summary>
    static Damageable CuaToi()
    {
        var dir = GameDirector.Instance;
        return dir != null && dir.player != null ? dir.player.GetComponent<Damageable>() : null;
    }

    static void TinhNguon()
    {
        soTam = 0;
        var toi = CuaToi();
        if (toi == null) return;
        Them(toi.transform.position);                    // chet roi van giu quanh xac (nguoi dung chon)
        var dir = GameDirector.Instance;
        if (dir == null) return;
        foreach (var t in dir.moiNguoi)
        {
            if (soTam >= tam.Length) break;
            if (t == null || t == toi.transform) continue;
            var d = t.GetComponent<Damageable>();
            if (d != null && !d.IsDead && CheDoTran.LaDongDoi(toi, d)) Them(t.position);
        }
    }

    static void Them(Vector3 p) { tam[soTam++] = new Vector4(p.x, p.z, 0f, 1f); }

    /// <summary>Diem nay may nay co thay khong. Phan hinh tat (man chinh, phep thu) thi luon thay.</summary>
    public static bool ThayDuoc(Vector3 p)
    {
        if (!Bat) return true;
        float r2 = BanKinh * BanKinh;
        for (int i = 0; i < soTam; i++)
        {
            float dx = p.x - tam[i].x, dz = p.z - tam[i].y;
            if (dx * dx + dz * dz <= r2) return true;
        }
        return false;
    }

    /// <summary>Hop bao co phan nao trong tam nhin (vat to nhu Loc xoay: thay mot phan la hien).</summary>
    public static bool ThayDuoc(Bounds b)
    {
        if (!Bat) return true;
        float r2 = BanKinh * BanKinh;
        for (int i = 0; i < soTam; i++)
        {
            float x = Mathf.Clamp(tam[i].x, b.min.x, b.max.x), z = Mathf.Clamp(tam[i].y, b.min.z, b.max.z);
            float dx = x - tam[i].x, dz = z - tam[i].y;
            if (dx * dx + dz * dz <= r2) return true;
        }
        return false;
    }

    /// <summary>
    /// LOGIC (khong phu thuoc phan hinh): <paramref name="nguoiNhin"/> (nguoi choi / BOT) co thay diem <paramref name="p"/> khong -
    /// trong 25 m quanh chinh no hoac quanh mot dong doi con song (che do Doi chia se tam nhin).
    /// </summary>
    public static bool NhinThay(Damageable nguoiNhin, Vector3 p)
    {
        if (nguoiNhin == null) return true;
        float r2 = BanKinh * BanKinh;
        if (KhoangNgang2(nguoiNhin.transform.position, p) <= r2) return true;
        var dir = GameDirector.Instance;
        if (dir == null || nguoiNhin.doi < 0) return false;
        foreach (var t in dir.moiNguoi)
        {
            if (t == null || t == nguoiNhin.transform) continue;
            var d = t.GetComponent<Damageable>();
            if (d != null && !d.IsDead && CheDoTran.LaDongDoi(nguoiNhin, d) && KhoangNgang2(t.position, p) <= r2) return true;
        }
        return false;
    }

    static float KhoangNgang2(Vector3 a, Vector3 b) { float dx = a.x - b.x, dz = a.z - b.z; return dx * dx + dz * dz; }

    // ================================================================
    //  MOI KHUNG
    // ================================================================

    void LateUpdate()
    {
        if (Time.time >= lucKiemPhepThu) KiemPhepThu();
        bool bat = !coPhepThu || BatTrongPhepThu;
        if (bat != Bat)
        {
            Bat = bat;
            if (!bat) HienHet();
            if (TamSuongDat != null) TamSuongDat.enabled = bat;
        }
        Shader.SetGlobalFloat("_TN_Bat", Bat ? 1f : 0f);
        if (!Bat) return;
        TinhNguon();
        for (int i = soTam; i < tam.Length; i++) tam[i] = Vector4.zero;
        Shader.SetGlobalVectorArray("_TN_Tam", tam);
        Shader.SetGlobalFloat("_TN_ThoiGian", Time.timeSinceLevelLoad);
        if (Time.time >= lucQuet) { lucQuet = Time.time + NhipQuet; QuetVatDong(); }
    }

    /// <summary>Phep thu goi de cap nhat ngay (khong cho nhip quet).</summary>
    public void QuetNgay() { if (Bat) { TinhNguon(); QuetVatDong(); } }

    void QuetVatDong()
    {
        var toi = CuaToi();
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
        {
            if (canhVat.Contains(r) || r == TamSuongDat) continue;
            var b = r.bounds;
            // he hat chua phat hat nao: khung bao rong nam o goc toa do -> dung vi tri vat
            if (b.extents.sqrMagnitude < 1e-6f) b = new Bounds(r.transform.position, Vector3.zero);
            AnHien(r, !ThayDuoc(b) && !CuaPheMinh(r.transform, toi));
        }
        foreach (var r in canhTrong)
            if (r != null) AnHien(r, !ThayDuoc(r.bounds));
        dangAn.RemoveWhere(x => x == null);
        SoRendererDangAn = dangAn.Count;
        int tat = 0;
        foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Exclude))
        {
            if (denCanh.Contains(l) || l.type == LightType.Directional) continue;
            float tamChieu = l.range;
            var b = new Bounds(l.transform.position, new Vector3(tamChieu * 2f, tamChieu * 2f, tamChieu * 2f));
            bool an = !ThayDuoc(b) && !CuaPheMinh(l.transform, toi);
            TatBatDen(l, an);
            if (an) tat++;
        }
        foreach (var l in denCanhDiem)
        {
            if (l == null) continue;
            bool an = !ThayDuoc(new Bounds(l.transform.position, Vector3.one * (l.range * 2f)));
            TatBatDen(l, an);
            if (an) tat++;
        }
        SoDenDangTat = tat;
    }

    static bool LaTrongSuot(Renderer r)
    {
        if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) return true;
        var m = r.sharedMaterial;
        return m != null && m.renderQueue >= 2500 && (m.shader == null || m.shader.name != "Diablo25D/NuocDam");
    }

    void AnHien(Renderer r, bool an)
    {
        if (an == r.forceRenderingOff) return;
        r.forceRenderingOff = an;
        if (an) dangAn.Add(r); else dangAn.Remove(r);
    }

    void TatBatDen(Light l, bool an)
    {
        int goc;
        bool dangTat = matNaGoc.TryGetValue(l, out goc);
        if (an && !dangTat) { matNaGoc[l] = l.cullingMask; l.cullingMask = 0; }
        else if (!an && dangTat) { l.cullingMask = goc; matNaGoc.Remove(l); }
    }

    /// <summary>Vat cua minh / dong doi (gan duoi nhan vat) thi khong an - van luon trong tam nhin cua chinh no.</summary>
    static bool CuaPheMinh(Transform t, Damageable toi)
    {
        var d = t.GetComponentInParent<Damageable>();
        if (d == null || toi == null) return false;
        return d == toi || CheDoTran.LaDongDoi(toi, d);
    }

    void HienHet()
    {
        foreach (var r in dangAn) if (r != null) r.forceRenderingOff = false;
        dangAn.Clear();
        foreach (var kv in matNaGoc) if (kv.Key != null) kv.Key.cullingMask = kv.Value;
        matNaGoc.Clear();
        SoRendererDangAn = 0; SoDenDangTat = 0;
    }

    // ================================================================
    //  DUNG LUC VAO TRAN
    // ================================================================

    /// <summary>Canh vat dung Standard (co rai, la, lo lua, sat) -> ban sao vat lieu voi shader ChuanSuong (cung ten thuoc tinh).</summary>
    void DoiVatLieuStandard()
    {
        var mau = Resources.Load<Material>("SuongChienTranh/ChuanSuong");
        if (mau == null) { Debug.LogWarning("[TamNhin] thieu Resources/SuongChienTranh/ChuanSuong.mat"); return; }
        var banSao = new Dictionary<Material, Material>();
        foreach (var r in canhVat)
        {
            if (r == null || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            var ms = r.sharedMaterials; bool doi = false;
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m == null || m.shader == null || m.shader.name != "Standard" || m.renderQueue > 2450) continue;
                Material moi;
                if (!banSao.TryGetValue(m, out moi))
                {
                    moi = new Material(m) { name = m.name + "_Suong" };
                    moi.shader = mau.shader;
                    moi.SetFloat("_CoAnhKim", m.IsKeywordEnabled("_METALLICGLOSSMAP") ? 1f : 0f);
                    moi.SetFloat("_CoPhat", m.IsKeywordEnabled("_EMISSION") ? 1f : 0f);
                    banSao[m] = moi;
                }
                ms[i] = moi; doi = true;
            }
            if (doi) r.sharedMaterials = ms;
        }
        SoVatLieuDaDoi = banSao.Count;
    }

    /// <summary>Tam luoi bam dia hinh (o 1 m, nhac 0,12 m) ve suong len dat.</summary>
    void DungTamSuongDat()
    {
        var ter = Terrain.activeTerrain;
        var vl = Resources.Load<Material>("SuongChienTranh/SuongDat");
        if (ter == null || vl == null) { Debug.LogWarning("[TamNhin] thieu dia hinh / vat lieu SuongDat"); return; }
        var td = ter.terrainData;
        Vector3 goc = ter.transform.position, kich = td.size;
        int n = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(kich.x, kich.z) / 1.0f), 16, 250);
        var vs = new Vector3[(n + 1) * (n + 1)];
        for (int z = 0; z <= n; z++)
            for (int x = 0; x <= n; x++)
            {
                float u = x / (float)n, v = z / (float)n;
                float h = td.GetInterpolatedHeight(u, v);
                vs[z * (n + 1) + x] = new Vector3(u * kich.x, h + 0.12f, v * kich.z);
            }
        var tris = new int[n * n * 6]; int k = 0;
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
            {
                int a = z * (n + 1) + x, b = a + 1, c = a + n + 1, d = c + 1;
                tris[k++] = a; tris[k++] = c; tris[k++] = b; tris[k++] = b; tris[k++] = c; tris[k++] = d;
            }
        var me = new Mesh { name = "TamSuongDat", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        me.vertices = vs; me.triangles = tris; me.RecalculateBounds();
        var go = new GameObject("TamSuongDat");
        go.layer = ter.gameObject.layer;          // cung lop Ground voi dia hinh (may quay chi ve Ground van thay suong)
        go.transform.position = goc;
        go.AddComponent<MeshFilter>().sharedMesh = me;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = vl;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
        TamSuongDat = mr;
        canhVat.Add(mr);
    }
}
