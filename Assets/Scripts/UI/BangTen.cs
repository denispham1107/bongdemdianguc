using UnityEngine;

/// <summary>
/// TEN NGUOI CHOI TREN DAU NHAN VAT - trong tran mang.
///
/// Bon nguoi cung mot prefab phu thuy, cung mot bo quan ao: vao tran roi thi
/// khong ai biet con nao la ai. Moi nhan vat trong tran mang (ca cua minh lan
/// ban sao cua nguoi khac) duoc gan mot bang ten ve ngay tren dinh dau.
///
/// VE BANG OnGUI, khong phai chu 3D trong canh - cung ly do voi so sat thuong
/// (<see cref="DamagePopup"/>): chu trong canh bi hat lua cong sang va bloom
/// phu trang, con OnGUI luon nam tren. Font Inter (<see cref="GiaoDien.ChuDam"/>)
/// vi font mac dinh cua Unity thieu chu co dau tieng Viet - len web la mat chu.
///
/// Ten cua minh mau vang, ten nguoi khac mau trang nga; nguoi da guc thi ten
/// mo di. NEN TRONG SUOT (ban dau co nen den mo - nguoi dung khong muon), chi
/// con vien toi mong bon phia de doc duoc tren nen lua sang. Ten di qua
/// <see cref="GhepDauTiengViet"/>: dau roi do bo go de lai duoc ghep thanh chu
/// dung san truoc khi ve.
///
/// DINH DAU = XUONG "head_end" cua model, doc MOI KHUNG - ten di theo dau khi
/// chay, khi nga xuong. Lan dau lam theo khung bao luoi thi ten lo lung cach
/// chop mu 0,4 m: do bang BakeMesh, chop mu that o 1,61 m (head_end 1,58 m)
/// nhung khung bao SkinnedMesh len toi 1,88 m (khung "rong rai" de cat hinh,
/// khong phai dinh dau), con con nhong va cham cao 2,06 m. Chi nhan vat dung
/// bang code (khong co xuong) moi con dung khung bao.
///
/// KIEU "LUA DIA NGUC" (09/10/2026, nguoi dung: "ten con qua don dieu va tho, thiet ke lai cho hop phong cach kinh di";
/// chon kieu 3 trong anh xem truoc PlayTestShots/bangten_mau.png): font Playfair Display SC Black (GiaoDien.ChuTen - cung
/// ho chu voi ten game, du 134 chu co dau), QUANG LUA mem phia sau chu (anh Blender MCP Resources/GiaoDien/QuangTen.png,
/// nhuom cam + mau doi), vien toi tam huong, chu sang mau doi, GACH THAN HONG duoi chu (GachLua.png, vuot nhon hai dau).
/// Anh dung bang Blender MCP CongCu/Blender/bang_ten.blend (scene BangTenAnh), nhap voi alpha tu do xam.
/// </summary>
public class BangTen : MonoBehaviour
{
    public string ten = "";
    public bool laToi;

    /// <summary>
    /// Tran DOI (28/09/2026, nguoi dung chon "ten tren dau theo mau doi"): 0 = Doi A (xanh duong), 1 = Doi B (do), -1 = tran
    /// Don. Co doi thi ten ve bang MAU DOI (ca ten cua minh) va mot dong nho "ĐỘI A/B" ngay tren ten.
    /// </summary>
    public sbyte doi = -1;
    /// <summary>Co chu dong "ĐỘI A/B" so voi ten.</summary>
    public const float HeSoChuDoi = 0.72f;
    /// <summary>O chu "ĐỘI A/B" vua ve (phep thu doc lai).</summary>
    public Rect oDoiCuoi;

    /// <summary>Co chu o man hinh cao 1080 - cung thuoc do voi GameHUD (Playfair SC nho mat hon Inter: 22 ~ Inter 19).</summary>
    public const float CoChu = 22f;
    /// <summary>Chu sang len bao nhieu phan tram ve phia trang so voi mau doi (de doc tren quang lua).</summary>
    public const float ChuSangHon = 0.4f;
    /// <summary>Khoang ho giua dinh dau va day bang ten (m).</summary>
    public const float KheTrenDau = 0.12f;

    static readonly Color MauToi = new Color(1.00f, 0.84f, 0.42f);
    static readonly Color MauNguoiKhac = new Color(0.95f, 0.92f, 0.86f);

    static readonly Color MauQuangLua = new Color(1.00f, 0.42f, 0.08f);
    static readonly Color MauVien = new Color(0.12f, 0.03f, 0.00f);

    static GUIStyle kieu;
    static Texture2D anhQuang, anhGach;
    Damageable mau;
    float caoDinh = -1f;

    // Xuong dau va doan bu tu xuong len chop mu (m): head_end sat chop mu
    // (1,58 / 1,61 m), xuong Head thi thap hon 0,25 m
    Transform xuongDau;
    float buXuong;
    bool daTimXuong;

    // ---- Cho phep thu doc lai (menu 52) ----
    /// <summary>O chu vua ve (toa do OnGUI, y tinh tu tren xuong).</summary>
    public Rect oCuoi;
    public int khungVeCuoi = -1;
    public Color mauCuoi;
    public static Font FontDangDung { get { return kieu != null ? kieu.font : null; } }

    /// <summary>Gan (hoac doi ten) bang ten cho mot nhan vat.</summary>
    public static BangTen Gan(GameObject nv, string ten, bool laToi)
    {
        if (nv == null) return null;
        var b = nv.GetComponent<BangTen>();
        if (b == null) b = nv.AddComponent<BangTen>();
        // Ten do nguoi choi tu go - co the mang dau roi (U+0300...) ma Unity
        // khong dat len dung chu cai duoc
        b.ten = GhepDauTiengViet.Ghep(ten ?? "");
        b.laToi = laToi;
        return b;
    }

    void Awake()
    {
        mau = GetComponent<Damageable>();
    }

    void TimXuongDau()
    {
        daTimXuong = true;
        Transform head = null;
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "head_end") { xuongDau = t; buXuong = 0.03f; return; }
            if (head == null && (t.name == "Head" || t.name == "head")) head = t;
        }
        if (head != null) { xuongDau = head; buXuong = 0.25f; }
    }

    /// <summary>Diem ngay tren chop mu (the gioi) - noi day bang ten dat vao.</summary>
    public Vector3 DiemTrenDau()
    {
        if (!daTimXuong) TimXuongDau();
        if (xuongDau != null)
            return xuongDau.position + Vector3.up * (buXuong + KheTrenDau);
        if (caoDinh < 0f) caoDinh = DoCaoDinhDau();
        return transform.position + Vector3.up * (caoDinh + KheTrenDau);
    }

    float DoCaoDinhDau()
    {
        float goc = transform.position.y, cao = 0f;
        bool co = false;
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            // Chi tinh luoi cua nhan vat - hat va vet sang cua ky nang thi khong
            if (!r.enabled || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                continue;
            cao = Mathf.Max(cao, r.bounds.max.y - goc);
            co = true;
        }
        if (!co || cao < 0.8f)
        {
            var cc = GetComponent<CharacterController>();
            cao = cc != null ? cc.center.y + cc.height * 0.5f : 1.8f;
        }
        return Mathf.Clamp(cao, 1.0f, 3.2f);
    }

    void OnGUI()
    {
        // OnGUI chay nhieu luot moi khung - chi luot Repaint moi ve that
        if (Event.current.type != EventType.Repaint) return;
        if (string.IsNullOrEmpty(ten)) return;

        var cam = Camera.main;
        if (cam == null) return;

        if (!TamNhin.ThayDuoc(transform.position)) return;     // ngoai tam nhin 25 m (suong chien tranh)
        Vector3 man = cam.WorldToScreenPoint(DiemTrenDau());
        if (man.z <= 0f) return;                              // sau lung camera

        float s = Screen.height / 1080f;
        if (kieu == null)
        {
            kieu = new GUIStyle(GUI.skin.label);
            kieu.alignment = TextAnchor.MiddleCenter;
            kieu.wordWrap = false;
            kieu.clipping = TextClipping.Overflow;
            kieu.padding = new RectOffset(0, 0, 0, 0);
        }
        // Gan lai moi lan: font nap tu Resources, vao lai Play thi doi tuong cu co the da bi huy
        kieu.font = GiaoDien.ChuTen;
        if (anhQuang == null) anhQuang = Resources.Load<Texture2D>("GiaoDien/QuangTen");
        if (anhGach == null) anhGach = Resources.Load<Texture2D>("GiaoDien/GachLua");
        kieu.fontSize = Mathf.Max(13, Mathf.RoundToInt(CoChu * s));

        var nd = new GUIContent(ten);
        Vector2 kt = kieu.CalcSize(nd);

        // Day bang ten (gach than hong) nam ngay tren dinh dau; OnGUI dem y tu tren xuong.
        // O chu nam tren gach; day o chu co khoang chan chu (descent) nen gach de len phan duoi o mot chut.
        float yDay = Screen.height - man.y;
        float caoGach = Mathf.Max(4f, 9f * s);
        var o = new Rect(man.x - kt.x * 0.5f, yDay - caoGach * 0.5f - kt.y * 0.88f, kt.x, kt.y);

        bool daGuc = mau != null && mau.IsDead;
        Color c = doi >= 0 ? CheDoTran.MauDoi(doi) : laToi ? MauToi : MauNguoiKhac;
        float doDuc = daGuc ? 0.45f : 1f;
        c.a = doDuc;
        // Chu tran Doi sang len ve phia trang (mau doi dam qua thi chim vao quang lua)
        Color mauChu = doi >= 0 ? Color.Lerp(c, Color.white, ChuSangHon) : c;
        mauChu.a = doDuc;

        // Nam duoi HUD (do sau 0), cung lop voi so sat thuong
        GUI.depth = 10;
        Color mauCu = GUI.color;

        // 1. Quang lua mem phia sau (cam), long trong nhuom mau doi / mau ten
        if (anhQuang != null)
        {
            var cq = o.center;
            float rq = kt.x * 1.45f + 24f * s, hq = kt.y * 1.7f;
            GUI.color = new Color(MauQuangLua.r, MauQuangLua.g, MauQuangLua.b, 0.8f * doDuc);
            GUI.DrawTexture(new Rect(cq.x - rq * 0.5f, cq.y - hq * 0.5f, rq, hq), anhQuang);
            float rq2 = kt.x * 1.12f, hq2 = kt.y * 1.15f;
            GUI.color = new Color(c.r, c.g, c.b, 0.4f * doDuc);
            GUI.DrawTexture(new Rect(cq.x - rq2 * 0.5f, cq.y - hq2 * 0.5f, rq2, hq2), anhQuang);
        }

        // 2. Vien toi tam huong + chu
        float v = Mathf.Max(1f, 1.6f * s);
        VeVien(o, nd, v, doDuc);
        GUI.color = Color.white;
        kieu.normal.textColor = mauChu;
        GUI.Label(o, nd, kieu);

        // 3. Gach than hong duoi chu
        if (anhGach != null)
        {
            float rg = kt.x * 1.04f;
            GUI.color = new Color(1f, 1f, 1f, doDuc);
            GUI.DrawTexture(new Rect(man.x - rg * 0.5f, yDay - caoGach, rg, caoGach), anhGach);
        }
        GUI.color = mauCu;

        // Tran Doi: dong nho "ĐỘI A/B" ngay tren ten, mau doi
        if (doi >= 0)
        {
            int coTen = kieu.fontSize;
            kieu.fontSize = Mathf.Max(10, Mathf.RoundToInt(coTen * HeSoChuDoi));
            var ndDoi = new GUIContent(CheDoTran.TenDoi(doi));
            Vector2 ktDoi = kieu.CalcSize(ndDoi);
            var oDoi = new Rect(man.x - ktDoi.x * 0.5f, o.y - ktDoi.y + 4f * s, ktDoi.x, ktDoi.y);
            VeVien(oDoi, ndDoi, v, doDuc);
            kieu.normal.textColor = c;
            GUI.Label(oDoi, ndDoi, kieu);
            kieu.fontSize = coTen;
            oDoiCuoi = oDoi;
        }

        oCuoi = o; khungVeCuoi = Time.frameCount; mauCuoi = c;
    }

    /// <summary>Vien toi TAM huong quanh chu (bon huong thi net cheo cua chu khac Playfair ho vien).</summary>
    static void VeVien(Rect o, GUIContent nd, float v, float doDuc)
    {
        kieu.normal.textColor = new Color(MauVien.r, MauVien.g, MauVien.b, 0.9f * doDuc);
        float d = v * 0.7071f;
        GUI.Label(new Rect(o.x - v, o.y, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x + v, o.y, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x, o.y - v, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x, o.y + v, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x - d, o.y - d, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x + d, o.y - d, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x - d, o.y + d, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x + d, o.y + d, o.width, o.height), nd, kieu);
    }
}
