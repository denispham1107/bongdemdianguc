using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TRANG THAI BI DANH NGA (thien thach roi trung).
///
/// Nguoi dung xin (13/09/2026): qua thien thach roi xuong co 40% danh nga ke
/// dich 1,5 giay - bi HAT NHE LEN KHOI MAT DAT roi rot xuong NAM NGUA, khong di
/// duoc va khong dung duoc ky nang. Ap cho ca quai lan nguoi choi.
///
/// LAT HINH, KHONG LAT NHAN VAT. Moi nhan vat co CharacterController o goc va
/// model (co component Animation) o con dau tien. Xoay goc thi con nhong va
/// cham xoay theo va ma AI / dieu khien cung ghi de huong moi khung hinh; xoay
/// con model thi va cham van dung yen o dat, chi hinh la nam xuong. Hoat hinh
/// (ModelHoatHinh, NguoiChoiHoatHinh) chi ghi cac KHOP XUONG ben trong nen
/// khong dung toi transform nay.
///
/// Dong thoi gian trong <see cref="thoiGian"/> giay (tinh tu luc trung):
///   0 .. 0,38 s  : bi hat len (duong parabol cao 0,9 m) va lat ngua dan
///   .. -0,28 s   : nam ngua tren dat
///   0,28 s cuoi  : chong dung day
/// Khoa di chuyen va ky nang trong SUOT khoang ay.
///
/// Chay SAU moi script khac (DefaultExecutionOrder) de khong ai ghi de hinh.
///
/// DAU HIEU TREN DAU (13/09/2026): nguoi dung danh rat nhieu lan ma "khong thay
/// hieu ung danh nga". Do bang duong that thi 8/10 lan co nga - nhung chuoi anh
/// chup trong 1,5 giay nam cho thay ca LOAT BA QUA THIEN THACH no noi nhau phu
/// lua kin cho con quai suot thoi gian ay, con chu "NGA!" troi mat sau nua giay
/// giua ca chuc con so sat thuong. Hieu ung co that, nhung khong ai nhin thay.
/// Nen suot luc nga, tren dau ke bi nga co MOT VONG SAO XOAY va chu NGA vien
/// den - ve bang OnGUI (<see cref="VeTatCa"/>) nen lua 3D khong the che.
/// </summary>
[DefaultExecutionOrder(10000)]
public class BiDanhNga : MonoBehaviour
{
    /// <summary>Tong thoi gian bi khoa, tinh tu luc trung.</summary>
    public float thoiGian = 1.5f;

    /// <summary>Da troi bao lau tu luc trung.</summary>
    public float daTroi;

    public const float TgBay = 0.38f;
    public const float CaoBay = 0.9f;
    public const float TgDay = 0.28f;
    /// <summary>Nhac hinh len mot chut khi nam de lung khong chim xuong dat.</summary>
    const float NangKhiNam = 0.22f;

    public bool DangNga { get { return daTroi < thoiGian; } }

    static readonly List<BiDanhNga> dangNga = new List<BiDanhNga>();

    /// <summary>So dau hieu da ve o luot Repaint gan nhat - phep thu (menu 62) doc.</summary>
    public static int SoDauVeLanCuoi { get; private set; }

    Transform hinh;
    Vector3 posGoc;
    Quaternion rotGoc;
    Damageable target;

    /// <summary>
    /// Danh nga mot muc tieu. Dang nga roi thi keo dai them (khong bay lai lan
    /// nua - dang nam ma nay len giua khong trung thi nhin ra loi).
    /// </summary>
    public static void Apply(Damageable d, float giay)
    {
        if (d == null || d.IsDead || giay <= 0f) return;
        // Ban sao khong tu gieo hieu ung cho minh - trang thai that den tu goi
        // tin cua may chu so huu (HieuUngQuaMang.ApCo). Cung quy uoc voi choang.
        if (d.mauDoMayKhacQuyet) return;
        GanHoacKeoDai(d, giay);
    }

    /// <summary>Gan / keo dai KHONG qua cho gac ban sao - danh cho HieuUngQuaMang.</summary>
    public static BiDanhNga GanHoacKeoDai(Damageable d, float giay)
    {
        if (d == null || d.IsDead) return null;
        var n = d.GetComponent<BiDanhNga>();
        if (n == null)
        {
            n = d.gameObject.AddComponent<BiDanhNga>();
            n.thoiGian = giay;
            n.daTroi = 0f;
            // Khong tha chu "NGA!" troi len nua: no troi mat sau nua giay giua
            // dong so sat thuong. Dau hieu bam tren dau (VeTatCa) thay the.
            if (d.anim != null) d.anim.PlayHit();
        }
        else
        {
            n.thoiGian = Mathf.Max(n.thoiGian, n.daTroi + giay);
        }
        return n;
    }

    void OnEnable()
    {
        dangNga.Add(this);
        VeSoSatThuong.DamBao();         // lop OnGUI ve dau hieu
    }

    void OnDisable() { dangNga.Remove(this); }

    void Awake()
    {
        target = GetComponent<Damageable>();

        // Model = con dau tien co Animation; khong co thi con dau tien co hinh
        foreach (Transform c in transform)
            if (c.GetComponent<Animation>() != null) { hinh = c; break; }
        if (hinh == null)
            foreach (Transform c in transform)
                if (c.GetComponentInChildren<Renderer>() != null) { hinh = c; break; }

        if (hinh != null) { posGoc = hinh.localPosition; rotGoc = hinh.localRotation; }
    }

    void LateUpdate()
    {
        daTroi += Time.deltaTime;

        if (hinh != null)
        {
            float goc, cao;
            if (daTroi < TgBay)
            {
                float u = daTroi / TgBay;
                cao = CaoBay * 4f * u * (1f - u) + NangKhiNam * u;      // nay len roi roi xuong
                goc = -90f * Mathf.SmoothStep(0f, 1f, u);               // lat ngua dan
            }
            else if (daTroi < thoiGian - TgDay)
            {
                cao = NangKhiNam;
                goc = -90f;
            }
            else
            {
                float u = Mathf.Clamp01((daTroi - (thoiGian - TgDay)) / TgDay);
                cao = NangKhiNam * (1f - u);
                goc = -90f * (1f - Mathf.SmoothStep(0f, 1f, u));        // chong dung day
            }

            // -90 quanh truc X CUA GOC NHAN VAT (nhan ben TRAI rotGoc): dinh dau
            // nga ve SAU lung, mat ngua len troi. Nhan ben phai la xoay theo
            // truc rieng cua model - model Meshy nhap vao thuong da xoay san
            // 180 do, luc ay nhan vat lai nga UP mat xuong.
            hinh.localRotation = Quaternion.Euler(goc, 0f, 0f) * rotGoc;
            hinh.localPosition = posGoc + Vector3.up * cao;
        }

        if (!DangNga) Destroy(this);
    }

    // ================================================================
    //  DAU HIEU TREN DAU - ve bang OnGUI, lua 3D khong che duoc
    // ================================================================

    static Texture2D dom;

    static Texture2D Dom
    {
        get
        {
            if (dom != null) return dom;
            const int n = 32;
            dom = new Texture2D(n, n, TextureFormat.RGBA32, false);
            dom.wrapMode = TextureWrapMode.Clamp;
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n - 0.5f, dy = (y + 0.5f) / n - 0.5f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f;         // 0 o tam, 1 o mep
                    float a = Mathf.Clamp01(1f - r);
                    px[y * n + x] = new Color(1f, 1f, 1f, a * a * (3f - 2f * a));
                }
            dom.SetPixels(px);
            dom.Apply(false, false);
            return dom;
        }
    }

    /// <summary>
    /// Ve vong sao xoay + chu NGA tren dau MOI ke dang bi nga. Goi tu
    /// VeSoSatThuong.OnGUI, SAU con so sat thuong de dau hieu nam tren cung.
    /// </summary>
    internal static void VeTatCa(Camera cam, GUIStyle kieu)
    {
        int so = 0;
        float H = Screen.height;
        var mauCu = GUI.color;

        for (int i = 0; i < dangNga.Count; i++)
        {
            var n = dangNga[i];
            if (n == null || !n.DangNga || n.target == null || n.target.IsDead) continue;

            // 2,7 m: CAO HON cho so sat thuong hien ra (2,1 m) - dat thap hon thi
            // ca chuc con so bay len de kin chu NGA.
            Vector3 w = n.transform.position + Vector3.up * 2.7f;
            Vector3 man = cam.WorldToScreenPoint(w);
            if (man.z <= 0f) continue;

            // Ti le theo phoi canh: bao nhieu diem anh cho mot met o cho ay
            float pxMoiMet = Mathf.Abs(cam.WorldToScreenPoint(w + Vector3.up).y - man.y);
            float rx = Mathf.Clamp(0.80f * pxMoiMet, 22f, 110f);
            float ry = rx * 0.32f;
            var tam = new Vector2(man.x, H - man.y);

            // Hien ro trong nua giay dau, mo dan luc dung day
            float conLai = n.thoiGian - n.daTroi;
            float alpha = Mathf.Clamp01(conLai / BiDanhNga.TgDay) * Mathf.Clamp01(n.daTroi / 0.08f);

            // Bon dom sao xoay quanh dau - dom phia truoc (gan camera) to hon
            for (int k = 0; k < 4; k++)
            {
                float g = Time.time * 5.2f + k * Mathf.PI * 0.5f;
                float truoc = Mathf.Sin(g);                                  // -1 sau, +1 truoc
                float kt = rx * (0.36f + 0.14f * truoc);
                var p = new Vector2(tam.x + Mathf.Cos(g) * rx, tam.y + truoc * ry);

                GUI.color = new Color(0f, 0f, 0f, 0.55f * alpha);            // quang toi de noi tren lua
                GUI.DrawTexture(new Rect(p.x - kt * 0.9f, p.y - kt * 0.9f, kt * 1.8f, kt * 1.8f), Dom);
                GUI.color = new Color(1f, 0.90f, 0.35f, alpha);
                GUI.DrawTexture(new Rect(p.x - kt * 0.5f, p.y - kt * 0.5f, kt, kt), Dom);
                GUI.color = new Color(1f, 1f, 0.92f, alpha);
                GUI.DrawTexture(new Rect(p.x - kt * 0.22f, p.y - kt * 0.22f, kt * 0.44f, kt * 0.44f), Dom);
            }

            // Chu NGA ngay tren vong sao, vien den bon phia
            int co = Mathf.Clamp(Mathf.RoundToInt(rx * 1.0f / 2f) * 2, 18, 72);
            kieu.fontSize = co;
            var o = new Rect(tam.x - 120f, tam.y - ry - co * 1.9f, 240f, co * 1.6f);
            float vien = Mathf.Max(1.5f, co * 0.08f);
            kieu.normal.textColor = new Color(0f, 0f, 0f, 0.9f * alpha);
            GUI.Label(new Rect(o.x - vien, o.y, o.width, o.height), "NGÃ", kieu);
            GUI.Label(new Rect(o.x + vien, o.y, o.width, o.height), "NGÃ", kieu);
            GUI.Label(new Rect(o.x, o.y - vien, o.width, o.height), "NGÃ", kieu);
            GUI.Label(new Rect(o.x, o.y + vien, o.width, o.height), "NGÃ", kieu);
            kieu.normal.textColor = new Color(1f, 0.74f, 0.28f, alpha);
            GUI.Label(o, "NGÃ", kieu);

            so++;
        }

        GUI.color = mauCu;
        SoDauVeLanCuoi = so;
    }

    void OnDestroy()
    {
        // Chet trong luc dang nam thi GIU hinh nam - xac nga ngua la dung, bat
        // dung day roi moi nga kieu chet thi nhin ra loi.
        if (target != null && target.IsDead) return;
        if (hinh != null) { hinh.localPosition = posGoc; hinh.localRotation = rotGoc; }
    }
}
