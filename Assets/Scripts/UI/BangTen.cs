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
/// mo di. Co vien toi va nen mo de doc duoc ca tren nen lua sang.
///
/// DINH DAU = XUONG "head_end" cua model, doc MOI KHUNG - ten di theo dau khi
/// chay, khi nga xuong. Lan dau lam theo khung bao luoi thi ten lo lung cach
/// chop mu 0,4 m: do bang BakeMesh, chop mu that o 1,61 m (head_end 1,58 m)
/// nhung khung bao SkinnedMesh len toi 1,88 m (khung "rong rai" de cat hinh,
/// khong phai dinh dau), con con nhong va cham cao 2,06 m. Chi nhan vat dung
/// bang code (khong co xuong) moi con dung khung bao.
/// </summary>
public class BangTen : MonoBehaviour
{
    public string ten = "";
    public bool laToi;

    /// <summary>Co chu o man hinh cao 1080 - cung thuoc do voi GameHUD.</summary>
    public const float CoChu = 19f;
    /// <summary>Khoang ho giua dinh dau va day bang ten (m).</summary>
    public const float KheTrenDau = 0.12f;

    static readonly Color MauToi = new Color(1.00f, 0.84f, 0.42f);
    static readonly Color MauNguoiKhac = new Color(0.95f, 0.92f, 0.86f);

    static GUIStyle kieu;
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
        b.ten = ten ?? "";
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

        Vector3 man = cam.WorldToScreenPoint(DiemTrenDau());
        if (man.z <= 0f) return;                              // sau lung camera

        float s = Screen.height / 1080f;
        if (kieu == null)
        {
            kieu = new GUIStyle(GUI.skin.label);
            kieu.font = GiaoDien.ChuDam;
            kieu.alignment = TextAnchor.MiddleCenter;
            kieu.wordWrap = false;
            kieu.clipping = TextClipping.Overflow;
            kieu.padding = new RectOffset(0, 0, 0, 0);
        }
        kieu.fontSize = Mathf.Max(12, Mathf.RoundToInt(CoChu * s));

        var nd = new GUIContent(ten);
        Vector2 kt = kieu.CalcSize(nd);

        // Day bang ten nam ngay tren dinh dau; OnGUI dem y tu tren xuong
        float yDay = Screen.height - man.y;
        var o = new Rect(man.x - kt.x * 0.5f, yDay - kt.y, kt.x, kt.y);

        bool daGuc = mau != null && mau.IsDead;
        Color c = laToi ? MauToi : MauNguoiKhac;
        float doDuc = daGuc ? 0.45f : 1f;
        c.a = doDuc;

        // Nam duoi HUD (do sau 0), cung lop voi so sat thuong
        GUI.depth = 10;

        // Nen mo phia sau - doc duoc ca tren nen lua sang
        float le = Mathf.Max(3f, 6f * s);
        var nen = new Rect(o.x - le, o.y + kt.y * 0.08f, o.width + le * 2f, kt.y * 0.86f);
        GiaoDien.To(nen, new Color(0f, 0f, 0f, 0.38f * doDuc));

        // Vien toi bon phia
        float v = Mathf.Max(1f, 1.5f * s);
        kieu.normal.textColor = new Color(0f, 0f, 0f, 0.85f * doDuc);
        GUI.Label(new Rect(o.x - v, o.y, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x + v, o.y, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x, o.y - v, o.width, o.height), nd, kieu);
        GUI.Label(new Rect(o.x, o.y + v, o.width, o.height), nd, kieu);

        kieu.normal.textColor = c;
        GUI.Label(o, nd, kieu);

        oCuoi = o; khungVeCuoi = Time.frameCount; mauCuoi = c;
    }
}
