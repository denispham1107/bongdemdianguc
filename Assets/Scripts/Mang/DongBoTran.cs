using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GUI TRANG THAI CUA MINH, NHAN VA VE NHAN VAT CUA NGUOI KHAC.
///
/// Day la cho bon manh ghep cua giai doan 2 noi lai voi nhau:
///   - <see cref="KenhTrucTiep"/> (buoc 0) cho duong truyen,
///   - <see cref="GoiInput"/> (buoc 1) cho y muon,
///   - <see cref="DuDoan"/> (buoc 2) cho nhan vat cua minh,
///   - <see cref="NguoiChoiKhac"/> (buoc 3) cho nhan vat cua nguoi khac,
///   - <see cref="NoiSuy"/> (buoc nay) cho no di muot.
///
/// NHIP GUI 60 LAN MOI GIAY - va day la mot bai hoc phai tra gia.
///
/// Ban dau toi chon 20 lan/giay cho tiet kiem bang thong. Do ra: dem noi suy
/// phinh len 132 ms, vi dem KHONG THE mong hon khoang cach giua hai moc. Gui
/// 20 lan/giay nghia la hai moc cach nhau 50 ms, va dem phai chua duoc it nhat
/// tung ay moi luon co moc sau de ve o giua. Cong ca duong truyen thi do tre
/// nhin thay nhau len ~154 ms - gap doi muc tieu 50-80 ms.
///
/// Gui 60 lan/giay thi hai moc cach nhau 17 ms, dem mong han. Cai gia phai
/// tra la bang thong: 54 byte x 60 = 3,2 KB/giay moi nguoi. Tren 4G thi do la
/// con so khong dang ke - re hon nhieu so voi 80 ms do tre.
///
/// Bai hoc: tiet kiem bang thong o day KHONG doi lay duoc gi, ma con lam hong
/// dung cai dang co gang dat duoc.
/// </summary>
public class DongBoTran : MonoBehaviour
{
    /// <summary>Bao nhieu lan gui trang thai moi giay.</summary>
    public const int NhipGui = 60;

    /// <summary>Nhan vat cua may nay.</summary>
    public PlayerController toi;

    /// <summary>Chi so cua minh trong phong (0..3).</summary>
    public byte chiSoCuaToi;

    class MotNguoiKhac
    {
        public PlayerController nhanVat;
        public NoiSuy noiSuy = new NoiSuy();
    }

    readonly Dictionary<byte, MotNguoiKhac> nguoiKhac = new Dictionary<byte, MotNguoiKhac>();
    readonly GoiTin.MotNguoi[] demGui = new GoiTin.MotNguoi[1];
    readonly GoiTin.MotNguoi[] demNhan = new GoiTin.MotNguoi[8];

    float guiLanSau;
    float batDauLuc;

    // ---- So dem de chan doan ----
    public int SoGoiDaGui { get; private set; }
    public int SoGoiDaNhan { get; private set; }
    public int SoGoiHong { get; private set; }
    public int SoByteDaGui { get; private set; }

    void Awake()
    {
        batDauLuc = Time.unscaledTime;
    }

    /// <summary>Gio tran dau, mili giay ke tu luc vao. Dung lam moc thoi gian
    /// trong goi tin - khong gui ca dau thoi gian Unix cho ton byte.</summary>
    public int GioTran()
    {
        return Mathf.RoundToInt((Time.unscaledTime - batDauLuc) * 1000f);
    }

    /// <summary>Them mot nguoi choi khac vao tran.</summary>
    public void ThemNguoi(byte chiSo, PlayerController nv)
    {
        if (nv == null) return;
        nguoiKhac[chiSo] = new MotNguoiKhac { nhanVat = nv };
    }

    public void BoNguoi(byte chiSo)
    {
        MotNguoiKhac n;
        if (!nguoiKhac.TryGetValue(chiSo, out n)) return;
        NguoiChoiKhac.Bo(n.nhanVat);
        nguoiKhac.Remove(chiSo);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        NhanHet();
        VeNguoiKhac(dt);

        if (Time.unscaledTime >= guiLanSau)
        {
            guiLanSau = Time.unscaledTime + 1f / NhipGui;
            GuiTrangThaiCuaToi();
        }
    }

    void GuiTrangThaiCuaToi()
    {
        if (toi == null || !KenhTrucTiep.DaMo) return;

        var mau = toi.GetComponent<Damageable>();

        demGui[0] = new GoiTin.MotNguoi
        {
            chiSo = chiSoCuaToi,
            viTri = toi.transform.position,
            gocY = toi.transform.eulerAngles.y,
            mau01 = mau != null && mau.maxHealth > 0f ? mau.health / mau.maxHealth : 1f,
            dangChay = toi.input.huongDi.sqrMagnitude > 0.01f || toi.DangCoDiemDen,
            daChet = mau != null && mau.IsDead
        };

        byte[] b = GoiTin.VietTrangThai(GioTran(), demGui, 1);
        if (KenhTrucTiep.Gui(GoiTin.SangChuoi(b)))
        {
            SoGoiDaGui++;
            SoByteDaGui += b.Length;
        }
    }

    /// <summary>Vet sach hang cho - mot khung hinh co the co vai goi den cung luc.</summary>
    void NhanHet()
    {
        string s;
        while ((s = KenhTrucTiep.Nhan()).Length > 0)
        {
            byte[] b = GoiTin.TuChuoi(s);
            if (b == null) { SoGoiHong++; continue; }

            int moc;
            int soNguoi = GoiTin.DocTrangThai(b, demNhan, out moc);
            if (soNguoi < 0) { SoGoiHong++; continue; }

            SoGoiDaNhan++;

            for (int i = 0; i < soNguoi; i++)
            {
                var p = demNhan[i];
                if (p.chiSo == chiSoCuaToi) continue;   // trang thai cua chinh minh

                MotNguoiKhac n;
                if (!nguoiKhac.TryGetValue(p.chiSo, out n)) continue;

                n.noiSuy.Nhan(moc, p.viTri, p.gocY, p.dangChay);
            }
        }
    }

    void VeNguoiKhac(float dt)
    {
        foreach (var cap in nguoiKhac)
        {
            var n = cap.Value;
            if (n.nhanVat == null) continue;

            n.noiSuy.CapNhat(dt);
            if (!n.noiSuy.SanSang) continue;

            // Dat thang, KHONG qua CharacterController: vi tri nay den tu may
            // kia, no da tinh va cham o ben do roi. Cho CharacterController
            // xen vao lan nua thi nhan vat bi ket vao tuong hai lan.
            var t = n.nhanVat.transform;
            var cc = n.nhanVat.GetComponent<CharacterController>();
            bool batLai = cc != null && cc.enabled;
            if (batLai) cc.enabled = false;

            t.position = n.noiSuy.ViTriHienThi;
            t.rotation = Quaternion.Euler(0f, n.noiSuy.GocHienThi, 0f);

            if (batLai) cc.enabled = true;

            // Cho chan buoc khop voi viec dang di hay dung
            var anim = n.nhanVat.GetComponentInChildren<ProceduralAnimator>();
            if (anim != null) anim.SetMoveSpeed(n.noiSuy.DangChay ? 1f : 0f);
        }
    }

    /// <summary>Do day dem cua mot nguoi - de chan doan.</summary>
    public float DemCuaNguoi(byte chiSo)
    {
        MotNguoiKhac n;
        return nguoiKhac.TryGetValue(chiSo, out n) ? n.noiSuy.DemGiay : 0f;
    }

    public NoiSuy NoiSuyCuaNguoi(byte chiSo)
    {
        MotNguoiKhac n;
        return nguoiKhac.TryGetValue(chiSo, out n) ? n.noiSuy : null;
    }
}
