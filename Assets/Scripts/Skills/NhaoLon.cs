using UnityEngine;

/// <summary>
/// KY NANG 22 - NHAO LON (nguoi dung 10/10/2026, nhom HO TRO): nhan vat nhao lon tien ve phia ngam, toi da 8 m (nguoi dung
/// 10/10/2026 lan hai, truoc 5 m - van MOT vong lon, chon 0,76 giay), hoi chieu 4 giay.
/// Nguoi dung chon: CO SAN tu dau tran nhu binh mau / binh mana, CHI MOT CAP; khong ton nang luong; lan TOI CHO NGAM (gan hon 8 m thi
/// lan ngan lai, bam nhanh tren cam ung = du 8 m ve huong dang nhin / day can); KHONG mien sat thuong (ne duoc la nho ra khoi vung
/// don); vat can (bia, tuong, cay) thi DUNG LAI - khac Toc bien di xuyen; BOT dung de lan tranh khi bi ap sat.
///
/// DI CHUYEN (chi may dieu khien nhan vat - PlayerController.HandleMovement goi <see cref="BuocDi"/>): CharacterController.Move theo
/// mot duong cong quang duong lao nhanh luc dau, cham dan luc dung day (<see cref="TiLeQuang"/>), trong luc van keo - lan qua mep vuc
/// thi roi xuong. Thoi gian lan theo quang (<see cref="ThoiGianLan"/>: 5 m = 0,6 giay, 8 m = 0,76 giay). Ban sao nguoi choi khac: vi tri den tu goi tin,
/// may nay chi chay HINH (cung goi phep -> cung huong, cung thoi gian).
///
/// HINH - nhao lon MEM (dung tai cho, model Meshy chi co clip di bo): moi khung SAU NguoiChoiHoatHinh (thu tu 9000, truoc BiDanhNga
/// 10000):
///   1. CUON TRON THAN (muc <see cref="MucCuon"/> tang trong 20% dau, tha ra trong 28% cuoi): cot song cong ve truoc, dau cui, dui keo
///      sat nguc, cang chan gap ve sau, hai tay om goi - ngam huong tung doan xuong bang NguoiChoiHoatHinh.NgamHuongKhop (huong trong
///      khung GOC nhan vat, nhu tu the Giut set) nen khong phu thuoc truc xuong Meshy.
///   2. LAT CA MODEL CON 360 do quanh truc phai cua GOC (duong = dau di toi truoc), quanh TAM QUA BONG (trung binh hong / dau / hai
///      goi), goc theo smootherstep - van toc goc lien tuc, khong giat dau / cuoi.
///   3. CHAM DAT: xuong thap nhat (dau, hong, goi, ban chan, ban tay, cot song) cach mat dat <see cref="KheXuongDat"/> - qua bong lan
///      tren dat, khong lo lung, khong chui xuong.
/// Bi danh nga / hat tung / lốc xoay cuon / dong bang / choang / chet giua chung: dung lan ngay, KHONG tra hinh (hieu ung kia lam chu
/// model) - va hieu ung kia lay tu the dung GOC qua <see cref="LayGoc"/>, khong chup nham tu the dang lon nguoc.
/// </summary>
[DefaultExecutionOrder(9000)]
public class NhaoLon : MonoBehaviour
{
    public const float Tam = 8f;
    public const float HoiChieu = 4f;
    public const float NangLuong = 0f;
    /// <summary>Khong niem chu - 0,02 giay vua du mot khung de goi phep bay sang may khac (nhu Toc bien).</summary>
    public const float GiayNiem = 0.02f;
    /// <summary>Lan ngan hon 0,8 m thi thoi - ngam ngay duoi chan van lan mot doan ngan ve huong ay.</summary>
    public const float QuangToiThieu = 0.8f;
    const float GiayGoc = 0.34f, GiayMoiMet = 0.052f;
    /// <summary>Khe giua xuong thap nhat va mat dat khi dang cuon (do day than + ao).</summary>
    /// <remarks>0,10 lun 12 cm (ao choang thua duoi xuong thap nhat toi 0,22 m o vai goc - menu 119 do BakeMesh) -> 0,20.</remarks>
    public const float KheXuongDat = 0.20f;

    /// <summary>
    /// Khe xuong thap nhat -> dinh LUOI thap nhat theo GOC LAT khi cuon het (moi 15 do, 0..360): ao choang thua duoi xuong tu 7 toi 29 cm
    /// tuy goc - khe co dinh 0,20 thi lun 9 cm o goc nay, ho 14 cm o goc kia (menu 119). Tu the o mot goc luon y het nhau nen DO MOT LAN
    /// bang BakeMesh trong Editor (menu 119 muc C9 in ra bang) roi ghi cung - luc choi khong BakeMesh. null = dung KheXuongDat.
    /// </summary>
    // Menu 119 muc C9 (10/10/2026, 147 mau): cong 0,01 cho de giay / vat ao khong sat dat; o 0 va 360 do lay theo o ke ben (luc ay chua cuon het)
    public static readonly float[] BangKhe = { 0.080f, 0.080f, 0.159f, 0.282f, 0.294f, 0.273f, 0.235f, 0.188f, 0.157f, 0.195f, 0.183f, 0.190f, 0.222f, 0.234f, 0.230f, 0.203f, 0.192f, 0.171f, 0.154f, 0.156f, 0.137f, 0.127f, 0.115f, 0.105f, 0.095f };

    /// <summary>Khe can o goc <paramref name="goc"/> (do), noi suy bang <see cref="BangKhe"/>.</summary>
    public static float KheTheoGoc(float goc)
    {
        if (BangKhe == null || BangKhe.Length < 2) return KheXuongDat;
        float g = Mathf.Repeat(goc, 360f) / 15f;
        int i = Mathf.Min((int)g, BangKhe.Length - 2);
        return Mathf.Lerp(BangKhe[i], BangKhe[i + 1], g - i);
    }

    /// <summary>Thoi gian lan het quang <paramref name="quang"/> m: 8 m = 0,76 giay, 5 m = 0,60 giay, 2 m = 0,44 giay.</summary>
    public static float ThoiGianLan(float quang) { return GiayGoc + GiayMoiMet * quang; }

    /// <summary>Ti le quang duong da di o ti le thoi gian u (0..1): lao nhanh luc dau (van toc 1,6 x trung binh), cham dan luc dung day (0,4 x).</summary>
    public static float TiLeQuang(float u)
    {
        u = Mathf.Clamp01(u);
        float r = 1f - u;
        return 0.6f * (1f - r * r) + 0.4f * u;
    }

    /// <summary>Muc cuon tron than (0 dung - 1 cuon het) o ti le thoi gian u.</summary>
    public static float MucCuon(float u)
    {
        return Mathf.SmoothStep(0f, 1f, u / 0.20f) * (1f - Mathf.SmoothStep(0f, 1f, (u - 0.72f) / 0.28f));
    }

    /// <summary>Goc lat (do) o ti le thoi gian u: 0 -> 360, smootherstep tu 6% toi 90%.</summary>
    public static float GocLat(float u)
    {
        float t = Mathf.Clamp01((u - 0.06f) / 0.84f);
        return 360f * t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    // ---------------- trang thai ----------------
    Vector3 huong;          // huong ngang don vi
    float quang, tong, daTroi;
    bool laChu;             // may nay dieu khien nhan vat (di chuyen that)
    float quangDaDi;

    Transform hinh; Vector3 posGoc; Quaternion rotGoc; bool coGoc;
    NguoiChoiHoatHinh hh;
    Transform xHong, xDau, xDauCuoi, xCo, xSong02, xSong01, xSong;
    readonly Transform[] dui = new Transform[2], cang = new Transform[2], banChan = new Transform[2], muiChan = new Transform[2];
    Transform[] xThap;      // cac xuong xet cham dat
    bool daHuy;

    /// <summary>Dang nhao lon (con trong thoi gian lan, chua bi huy).</summary>
    public bool DangLan { get { return !daHuy && daTroi < tong; } }
    public float TiLeThoiGian { get { return tong > 0f ? Mathf.Clamp01(daTroi / tong) : 1f; } }
    public float GocHienTai { get; private set; }
    public float CuonHienTai { get; private set; }
    public Vector3 Huong { get { return huong; } }
    public float Quang { get { return quang; } }
    public float ThoiGian { get { return tong; } }
    /// <summary>Do cao xuong thap nhat tren mat dat o khung vua roi (m) - phep thu doc.</summary>
    public float KheDatCuoi { get; private set; }
    /// <summary>Do cao (the gioi) xuong thap nhat sau khi dat cham dat o khung vua roi - phep thu do bang khe.</summary>
    public float XuongThapNhatY { get; private set; }

    /// <summary>So lan nhao lon da bat (moi may) - phep thu doc.</summary>
    public static int SoLanBat;

    public static bool Dang(GameObject g)
    {
        if (g == null) return false;
        var n = g.GetComponent<NhaoLon>();
        return n != null && n.DangLan;
    }

    /// <summary>
    /// Bat dau nhao lon tu cho dang dung toi <paramref name="diemNgam"/> (kep Tam = 8 m, toi thieu QuangToiThieu). <paramref name="laChu"/> = may
    /// nay dieu khien nhan vat (di chuyen that); false = ban sao mang, chi hinh.
    /// </summary>
    public static NhaoLon Bat(PlayerController pc, Vector3 diemNgam, bool laChu)
    {
        if (pc == null) return null;
        Vector3 p = pc.transform.position;
        Vector3 v = diemNgam - p; v.y = 0f;
        float d = v.magnitude;
        Vector3 h;
        if (d < 0.05f) { h = pc.transform.forward; h.y = 0f; d = Tam; }
        else h = v / d;
        if (h.sqrMagnitude < 1e-4f) h = Vector3.forward;
        h.Normalize();
        d = Mathf.Clamp(d, QuangToiThieu, Tam);

        var cu = pc.GetComponent<NhaoLon>();
        if (cu != null) { cu.TraHinh(); DestroyImmediate(cu); }
        var n = pc.gameObject.AddComponent<NhaoLon>();
        n.huong = h; n.quang = d; n.tong = ThoiGianLan(d); n.daTroi = 0f; n.laChu = laChu;
        if (laChu) pc.transform.rotation = Quaternion.LookRotation(h);
        SoLanBat++;
        return n;
    }

    /// <summary>Tu the dung GOC cua model khi dang nhao lon - BiDanhNga / BiHatTung / XacNam goi luc gan vao (khong chup tu the dang lat).</summary>
    public static bool LayGoc(Component c, ref Vector3 pos, ref Quaternion rot)
    {
        if (c == null) return false;
        var n = c.GetComponent<NhaoLon>();
        if (n == null || !n.coGoc) return false;
        pos = n.posGoc; rot = n.rotGoc;
        return true;
    }

    void Awake()
    {
        foreach (Transform c in transform)
            if (c.GetComponent<Animation>() != null) { hinh = c; break; }
        if (hinh == null)
            foreach (Transform c in transform)
                if (c.GetComponentInChildren<Renderer>() != null) { hinh = c; break; }
        if (hinh != null) { posGoc = hinh.localPosition; rotGoc = hinh.localRotation; coGoc = true; }

        hh = GetComponent<NguoiChoiHoatHinh>();
        if (hh != null) { xHong = hh.hips; xDau = hh.head; }
        var goc = xHong != null ? xHong : (hinh != null ? hinh : transform);
        string[] ben = { "Left", "Right" };
        foreach (var t in goc.GetComponentsInChildren<Transform>(true))
        {
            switch (t.name)
            {
                case "Spine02": xSong02 = t; break;
                case "Spine01": xSong01 = t; break;
                case "Spine": xSong = t; break;
                case "neck": xCo = t; break;
                case "head_end": xDauCuoi = t; break;
                case "Head": if (xDau == null) xDau = t; break;
                case "Hips": if (xHong == null) xHong = t; break;
            }
            for (int i = 0; i < 2; i++)
            {
                if (t.name == ben[i] + "UpLeg") dui[i] = t;
                else if (t.name == ben[i] + "Leg") cang[i] = t;
                else if (t.name == ben[i] + "Foot") banChan[i] = t;
                else if (t.name == ben[i] + "ToeBase") muiChan[i] = t;
            }
        }
        var ds = new System.Collections.Generic.List<Transform>();
        foreach (var t in new[] { xHong, xDau, xDauCuoi, xSong, xSong01, cang[0], cang[1], banChan[0], banChan[1], muiChan[0], muiChan[1] })
            if (t != null) ds.Add(t);
        if (hh != null) { if (hh.banTayTrai != null) ds.Add(hh.banTayTrai); if (hh.banTayPhai != null) ds.Add(hh.banTayPhai); }
        xThap = ds.ToArray();
    }

    /// <summary>
    /// MAY CHU: quang duong phai di trong khung nay (ngang, the gioi). PlayerController.HandleMovement cong trong luc roi cc.Move.
    /// </summary>
    public Vector3 BuocDi(float dt)
    {
        if (!DangLan) return Vector3.zero;
        float u0 = TiLeThoiGian;
        daTroi += dt;
        float u1 = TiLeThoiGian;
        float ds = (TiLeQuang(u1) - TiLeQuang(u0)) * quang;
        quangDaDi += ds;
        return huong * ds;
    }

    /// <summary>Huy lan giua chung. <paramref name="traHinh"/> = false khi mot hieu ung khac (nga, hat tung, chet) lam chu model.</summary>
    public void Huy(bool traHinh)
    {
        if (daHuy) return;
        daHuy = true;
        if (traHinh) TraHinh();
        Destroy(this);
    }

    void TraHinh()
    {
        if (hinh != null && coGoc) { hinh.localPosition = posGoc; hinh.localRotation = rotGoc; }
    }

    bool HieuUngKhacLamChu()
    {
        var nga = GetComponent<BiDanhNga>(); if (nga != null && nga.DangNga) return true;
        var hat = GetComponent<BiHatTung>(); if (hat != null && hat.DangBay) return true;
        if (GetComponent<XacNam>() != null) return true;
        return false;
    }

    void LateUpdate()
    {
        if (daHuy) return;
        // Ban sao (khong di chuyen that) tu dem dong ho; may chu dem trong BuocDi
        if (!laChu) daTroi += Time.deltaTime;

        if (HieuUngKhacLamChu()) { Huy(false); return; }
        var dm = GetComponent<Damageable>();
        if (dm != null && dm.IsDead) { Huy(false); return; }
        var pc = GetComponent<PlayerController>();
        if (laChu && pc != null && (pc.DangBiKhoaCung || GetComponent<WhirledEffect>() != null)) { Huy(true); return; }
        if (daTroi >= tong) { Huy(true); return; }

        VeTuThe(TiLeThoiGian);
    }

    void VeTuThe(float u)
    {
        if (hinh == null || !coGoc) return;
        float k = MucCuon(u);
        float goc = GocLat(u);
        CuonHienTai = k; GocHienTai = goc;

        // ---- 1. Model ve tu the dung goc, roi cuon than (huong tinh trong khung GOC nhan vat) ----
        hinh.localPosition = posGoc; hinh.localRotation = rotGoc;
        Vector3 f = transform.forward, up = transform.up, r = transform.right;
        if (k > 0.001f)
        {
            Ngam(xSong02, xSong01, (up * 0.80f + f * 0.60f), k);
            Ngam(xSong01, xSong, (up * 0.55f + f * 0.85f), k);
            Ngam(xSong, xCo, (up * 0.25f + f * 0.97f), k);
            Ngam(xCo, xDau, (f * 0.85f - up * 0.35f), k);
            Ngam(xDau, xDauCuoi, (f * 0.45f - up * 0.90f), k);
            for (int i = 0; i < 2; i++)
            {
                float s = i == 0 ? -1f : 1f;          // 0 = trai (ve phia -r), 1 = phai
                Ngam(dui[i], cang[i], (f * 0.85f + up * 0.42f + r * 0.14f * s), k);
                Ngam(cang[i], banChan[i], (-up * 0.88f - f * 0.40f + r * 0.05f * s), k);
                Ngam(banChan[i], muiChan[i], (-up * 0.60f - f * 0.80f), k);
            }
            if (hh != null)
            {
                Ngam(hh.tayTraiTren, hh.tayTraiDuoi, (f * 0.62f - up * 0.55f - r * 0.38f), k);
                Ngam(hh.tayTraiDuoi, hh.banTayTrai, (f * 0.75f - up * 0.20f + r * 0.62f), k);
                Ngam(hh.tayPhaiTren, hh.tayPhaiDuoi, (f * 0.62f - up * 0.55f + r * 0.38f), k);
                Ngam(hh.tayPhaiDuoi, hh.banTayPhai, (f * 0.75f - up * 0.20f - r * 0.62f), k);
            }
        }

        // ---- 2. Lat ca model quanh tam qua bong, truc phai cua goc ----
        Vector3 tam = TamQuaBong();
        var q = Quaternion.AngleAxis(goc, r);
        hinh.position = tam + q * (hinh.position - tam);
        hinh.rotation = q * hinh.rotation;

        // ---- 3. Cham dat: xuong thap nhat cach dat KheXuongDat (tron theo muc cuon - dau / cuoi dung thang tren chan) ----
        float dat = GioLoc.MatDatY(transform.position, transform.position.y);
        float thap = float.MaxValue;
        foreach (var x in xThap) if (x != null) thap = Mathf.Min(thap, x.position.y);
        if (thap < float.MaxValue)
        {
            float can = (dat + KheTheoGoc(goc)) - thap;       // duong = phai nang len, am = ha xuong
            float w = Mathf.SmoothStep(0f, 1f, k * 1.4f);
            hinh.position += Vector3.up * (can * w);
            KheDatCuoi = thap + can * w - dat;
            XuongThapNhatY = thap + can * w;
        }
    }

    Vector3 TamQuaBong()
    {
        Vector3 s = Vector3.zero; int n = 0;
        foreach (var x in new[] { xHong, xDau, cang[0], cang[1] })
            if (x != null) { s += x.position; n++; }
        return n > 0 ? s / n : transform.position + Vector3.up * 0.8f;
    }

    static void Ngam(Transform khop, Transform con, Vector3 muon, float w)
    {
        if (khop == null || con == null || muon.sqrMagnitude < 1e-6f) return;
        NguoiChoiHoatHinh.NgamHuongKhop(khop, con, muon.normalized, w);
    }

    void OnDestroy()
    {
        // Bi xoa giua chung (khong qua Huy) ma khong co hieu ung nao lam chu model: tra hinh ve dung
        if (!daHuy && !HieuUngKhacLamChu()) TraHinh();
    }
}
