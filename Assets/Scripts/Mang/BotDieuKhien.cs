using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BO NAO CUA MOT MAY BOT - gan vao nhan vat BOT tren MAY CHU PHONG (KhoiDongTranMang.SinhBot).
///
/// BOT la PlayerController that (cung prefab nguoi choi), tuDocInput tat, mau do may nay quyet (khong phai ban sao). Moi
/// khung bo nao nay dat <see cref="PlayerController.input"/> TRUOC khi PlayerController.Update thi hanh no - cung duong
/// ma ban phim di (GoiInput), nen BOT di, tung phep, uong binh y het nguoi choi.
///
/// BUOC 3 (08/10/2026) - DI LAI. Nguoi dung: "biet tu kiem quai vat, doi thu nguoi choi khac, biet ne di vong cac chuong
/// ngai vat de khong bi mac ket, biet dung truoc vuc doi duong khac de khong bi rot xuong dia nguc".
///   - MUC TIEU: quai / doi thu gan nhat trong <see cref="TamPhatHien"/> m (bo dong doi, nguoi dang Tang hinh, ke da chet,
///     ke duoi vuc); khong ai trong tam thi SAN doi thu gan nhat ca ban do, roi toi quai, khong con gi thi DI TUAN. Chon lai
///     theo nhip phan xa cua do kho (<see cref="NhipPhanXa"/>).
///   - DUONG DI: A* tren luoi BanDoBot (dung mot lan luc vao tran), lam thang, tinh lai moi 1,2 s / khi dich doi cho. Toi gan
///     muc tieu <see cref="TamGiu"/> m thi dung (buoc 4: tung phep tu day).
///   - CHONG VUC: moi khung do dat PHIA TRUOC 0,9 / 1,7 m (tia xuong lop Ground + luoi). Hut chan -> DUNG, cam tam cho do,
///     tim duong khac.
///   - CHONG KET: cu 0,8 s ma muon di nhung dich chua toi 0,35 m -> cam tam cho chan truoc mat, tim lai; ket 3 lan lien -> di
///     vong ra mot diem ngau nhien gan do.
/// BUOC 4: ky nang theo he chinh, chieu lien hoan, binh mau / mana, cong diem ky nang khi len cap.
/// </summary>
[DefaultExecutionOrder(-40)]
public class BotDieuKhien : MonoBehaviour
{
    /// <summary>Ghe cua BOT trong tran.</summary>
    public byte ghe;
    /// <summary>MayBot.De / Thuong / Kho - doc tu uid.</summary>
    public int doKho = MayBot.Thuong;
    /// <summary>He chinh (MayBot.HeLua / HeBang / HeSet / HePhong) - boc ngau nhien luc sinh, chi may chu phong biet.</summary>
    public int he;
    public string uid;

    // ---- Hang so di lai ----
    /// <summary>Thay quai / doi thu trong ban kinh nay thi nham toi.</summary>
    public const float TamPhatHien = 30f;
    /// <summary>Toi gan muc tieu chung nay thi dung (tam phep tam trung binh 12-18 m - buoc 4 tung phep tu day).</summary>
    public const float TamGiu = 9f;
    const float NhipTimLai = 1.2f;
    const float NhipKiemKet = 0.8f;
    const float NguongKet = 0.35f;

    /// <summary>Bao lau chon lai muc tieu mot lan (giay) - phan xa theo do kho.</summary>
    public static float NhipPhanXa(int doKho) { return doKho == MayBot.De ? 1.2f : doKho == MayBot.Kho ? 0.35f : 0.7f; }

    // ---- Cho phep thu (menu 114) ----
    /// <summary>Tat = lai THANG toi dich (doi chung: khong tim duong).</summary>
    [System.NonSerialized] public bool dungTimDuong = true;
    /// <summary>Tat = khong do vuc phia truoc (doi chung).</summary>
    [System.NonSerialized] public bool dungChongVuc = true;
    /// <summary>Ep di toi mot diem (bo qua muc tieu) - phep thu.</summary>
    [System.NonSerialized] public bool coDiemEp;
    [System.NonSerialized] public Vector3 diemEp;
    /// <summary>Dung hoan toan (phep thu buoc 2 / ket tran).</summary>
    [System.NonSerialized] public bool dungYen;

    // ---- So dem chan doan ----
    [System.NonSerialized] public int soLanKet, soLanDungVuc, soLanVong;
    [System.NonSerialized] public float quangDuong;
    public Damageable MucTieu { get; private set; }
    public IReadOnlyList<Vector3> Duong { get { return duong; } }

    PlayerController pc;
    Damageable mau;
    float lucChonLai, lucTimLai, lucKiemKet = -1f, lucDoiTuan;
    readonly List<Vector3> duong = new List<Vector3>();
    int chiSoMoc;
    Vector3 dichCu, mocKiemKet, viTriTruoc, diemTuan;
    bool coDiemTuan, dangGiu;
    int ketLienTiep;
    float vongDen; Vector3 diemVong;
    float thoiGianMuonDi;

    void Awake()
    {
        pc = GetComponent<PlayerController>();
        mau = GetComponent<Damageable>();
        viTriTruoc = transform.position;
        // Moi BOT chon lai lech pha nhau - khong ca dan cung nghi mot khung
        lucChonLai = Time.time + Random.Range(0f, NhipPhanXa(doKho));
    }

    void Update()
    {
        if (pc == null) return;
        float dt = Time.deltaTime;
        var g = GoiInput.Rong(dt);

        Vector3 p = transform.position;
        quangDuong += Vector2.Distance(new Vector2(p.x, p.z), new Vector2(viTriTruoc.x, viTriTruoc.z));
        viTriTruoc = p;

        if (dungYen || KetTran.DaXong || (mau != null && mau.IsDead) || !pc.enabled || !BanDoBot.HopLeChoCanh)
        { pc.input = g; return; }

        if (Time.time >= lucChonLai)
        {
            MucTieu = ChonMucTieu();
            lucChonLai = Time.time + NhipPhanXa(doKho);
        }

        Vector3 huong = Vector3.zero;
        Vector3 dich;
        float dungKhi;
        if (TinhDich(out dich, out dungKhi)) huong = LaiToi(dich, dungKhi);

        if (dungChongVuc && huong.sqrMagnitude > 0.01f && PhiaTruocLaVuc(huong))
        {
            soLanDungVuc++;
            BanDoBot.CamTam(p + huong * 1.7f, 1.2f, 20f);
            lucTimLai = 0f;                       // tim duong khac ngay khung sau
            huong = Vector3.zero;
        }

        KiemKet(huong, dt);

        g.huongDi = huong;
        pc.input = g;
    }

    // ================================================================
    //  MUC TIEU
    // ================================================================

    Damageable ChonMucTieu()
    {
        var dir = GameDirector.Instance;
        if (dir == null) return null;
        Vector3 p = transform.position;

        Damageable gan = null; float dGan = TamPhatHien * TamPhatHien;
        Damageable nguoiXa = null; float dNguoiXa = float.MaxValue;
        Damageable quaiXa = null; float dQuaiXa = float.MaxValue;

        var quai = dir.QuaiConSong;
        for (int i = 0; i < quai.Count; i++)
        {
            var q = quai[i];
            if (!HopLe(q)) continue;
            float d = (q.transform.position - p).sqrMagnitude;
            if (d < dGan) { dGan = d; gan = q; }
            if (d < dQuaiXa) { dQuaiXa = d; quaiXa = q; }
        }
        foreach (var t in dir.moiNguoi)
        {
            if (t == null || t == transform) continue;
            var d = t.GetComponent<Damageable>();
            if (!HopLe(d) || CheDoTran.LaDongDoi(mau, d) || TangHinh.Dang(t)) continue;
            float kc = (t.position - p).sqrMagnitude;
            if (kc < dGan) { dGan = kc; gan = d; }
            if (kc < dNguoiXa) { dNguoiXa = kc; nguoiXa = d; }
        }
        if (gan != null) return gan;
        return nguoiXa != null ? nguoiXa : quaiXa;
    }

    static bool HopLe(Damageable d)
    {
        return d != null && !d.IsDead && d.transform.position.y > BanDoBot.DatVucDuoi;
    }

    /// <summary>Diem can toi va khoang cach dung lai. False = dung yen.</summary>
    bool TinhDich(out Vector3 dich, out float dungKhi)
    {
        dich = transform.position; dungKhi = 0.7f;
        if (coDiemEp) { dich = diemEp; return true; }
        if (Time.time < vongDen) { dich = diemVong; return true; }

        if (MucTieu != null && !MucTieu.IsDead)
        {
            coDiemTuan = false;
            float kc = KhoangNgang(transform.position, MucTieu.transform.position);
            // Tre: vao gan TamGiu x 0,8 moi dung, ra xa qua TamGiu moi di lai - khong giat cuc buoc mot
            if (dangGiu && kc <= TamGiu) return false;
            dangGiu = kc <= TamGiu * 0.8f;
            if (dangGiu) return false;
            dich = MucTieu.transform.position; dungKhi = TamGiu * 0.8f;
            return true;
        }

        // Khong co ai: di tuan
        dangGiu = false;
        if (!coDiemTuan || Time.time >= lucDoiTuan || KhoangNgang(transform.position, diemTuan) < 1.5f)
        {
            coDiemTuan = BanDoBot.DiemNgauNhien(transform.position, 20f, out diemTuan);
            lucDoiTuan = Time.time + 8f;
        }
        if (!coDiemTuan) return false;
        dich = diemTuan;
        return true;
    }

    // ================================================================
    //  LAI THEO DUONG
    // ================================================================

    Vector3 LaiToi(Vector3 dich, float dungKhi)
    {
        Vector3 p = transform.position;
        if (KhoangNgang(p, dich) <= dungKhi) { duong.Clear(); return Vector3.zero; }

        if (!dungTimDuong) return HuongNgang(p, dich);

        if (duong.Count == 0 || Time.time >= lucTimLai || KhoangNgang(dich, dichCu) > 2.5f)
        {
            if (!BanDoBot.TimDuong(p, dich, duong)) duong.Clear();
            chiSoMoc = 0;
            dichCu = dich;
            lucTimLai = Time.time + NhipTimLai;
        }
        if (duong.Count == 0) return Vector3.zero;     // khong co duong: dung, khong lao bua (co the la vuc)

        while (chiSoMoc < duong.Count - 1 && KhoangNgang(p, duong[chiSoMoc]) < 0.7f) chiSoMoc++;
        return HuongNgang(p, duong[chiSoMoc]);
    }

    bool PhiaTruocLaVuc(Vector3 huong)
    {
        Vector3 p = transform.position;
        int matDat = BanDoBot.MatNaDat;
        for (int i = 0; i < 2; i++)
        {
            Vector3 q = p + huong.normalized * (i == 0 ? 0.9f : 1.7f);
            if (BanDoBot.LaVucThat(q)) return true;
            RaycastHit hit;
            if (!Physics.Raycast(new Vector3(q.x, p.y + 2.5f, q.z), Vector3.down, out hit, 8f, matDat, QueryTriggerInteraction.Ignore))
                return true;
            if (hit.point.y < p.y - 2.2f) return true;
        }
        return false;
    }

    void KiemKet(Vector3 huong, float dt)
    {
        bool muonDi = huong.sqrMagnitude > 0.01f;
        if (muonDi) thoiGianMuonDi += dt;
        if (lucKiemKet < 0f) { lucKiemKet = Time.time + NhipKiemKet; mocKiemKet = transform.position; thoiGianMuonDi = 0f; return; }
        if (Time.time < lucKiemKet) return;

        float daDi = KhoangNgang(transform.position, mocKiemKet);
        bool dangNiem = pc.DangNiem >= 0;
        if (thoiGianMuonDi > NhipKiemKet * 0.7f && daDi < NguongKet && !dangNiem && !pc.DangBiKhoaCung)
        {
            soLanKet++;
            ketLienTiep++;
            BanDoBot.CamTam(transform.position + huong.normalized * 0.9f, 0.5f, 6f);
            lucTimLai = 0f;
            if (ketLienTiep >= 3 && BanDoBot.DiemNgauNhien(transform.position, 6f, out diemVong))
            {
                soLanVong++;
                vongDen = Time.time + 1.5f;
                ketLienTiep = 0;
            }
        }
        else if (daDi >= NguongKet) ketLienTiep = 0;

        lucKiemKet = Time.time + NhipKiemKet;
        mocKiemKet = transform.position;
        thoiGianMuonDi = 0f;
    }

    static float KhoangNgang(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    static Vector3 HuongNgang(Vector3 tu, Vector3 den)
    {
        Vector3 d = den - tu; d.y = 0f;
        return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.zero;
    }
}
