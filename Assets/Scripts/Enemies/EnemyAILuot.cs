using UnityEngine;

/// <summary>
/// CU LUOT CUA BO XUONG (nguoi dung 01/11/2026, tham khao cu lao toi cua Zealot SC2: "trong pham vi 5 m luot rat nhanh, delay 1,5 giay, 55% xuyen thung pha be
/// khien"; chon sat thuong = 1 nhat chem, xuyen = vo khien + danh vao nguoi).
///
/// Muc tieu (nguoi hoac BOT - GameDirector.GanNhat) o phia truoc mat trong 5 m, duong thang khong vuong -> luot bam theo muc tieu 18 m/s (5 m ~ 0,28 s), toi noi gay
/// 1 nhat chem roi chem tiep binh thuong. Da o sat (trong tam chem) thi KHONG luot, tru khi muc tieu bat khien: con quai dung o mep vom cach tam > tam chem, va cu luot
/// chinh la thu pha vom. Cham mat vom gieo MOT lan: 55% khien vo ngay + luot tiep vao nguoi; 45% dung o mat vom, khien an don nhu don thuong.
/// Bi choang / dong bang / nga / hat tung / loc cuon thi dung luot (khong cong hoi chieu lai).
///
/// Chi may TRONG TAI quai chay doan nay. May khach: cac kieu don 4-7 trong DienLaiDon (nguoi bi nham moi that su mat mau / vo khien that).
/// </summary>
public partial class EnemyAI
{
    public const float TamLuot = 5f;
    public const float TocLuot = 18f;
    public const float GiayHoiLuot = 1.5f;
    public const float XacSuatXuyenKhien = 0.55f;
    /// <summary>Luot lau hon nua la bo (muc tieu chay tron / vuong).</summary>
    public const float GiayLuotToiDa = 0.5f;

    /// <summary>Kieu don qua mang (GoiTin.MotDonQuai): bat dau luot (chi hinh), trung nguoi, trung khien khong xuyen, xuyen vo khien.</summary>
    public const int KieuLuotBatDau = 4, KieuLuotTrung = 5, KieuLuotTrungKhien = 6, KieuLuotXuyenKhien = 7;

    /// <summary>Dat trong EnemyFactory.ApDacTinh (Bo xuong). NonSerialized: prefab khong de len.</summary>
    [System.NonSerialized] public bool coLuot;

    /// <summary>Dem cho phep thu (menu 158).</summary>
    public static int SoLanLuot, SoLanLuotTrung, SoLanLuotXuyen, SoLanLuotKhongXuyen, SoLanLuotHut;

    /// <summary>Phep thu dat 0 / 1 de co dinh ket qua gieo xuyen khien; am = dung that (55%).</summary>
    public static float EpXuyenChoPhepThu = -1f;

    float hoiLuot, luotConLai;
    bool daGieoKhien;
    int soKhungKet;
    HinhLuotBoXuong hinhLuot;

    public bool DangLuot { get { return luotConLai > 0f; } }
    public float HoiLuotConLai { get { return hoiLuot; } }

    bool CoTheLuot(float dist, Transform t)
    {
        if (!coLuot || hoiLuot > 0f || attackTimer >= 0f || t == null || dist > TamLuot) return false;

        var k = t.GetComponent<Khieng>();
        bool coKhien = k != null && k.DangBat;
        if (dist <= attackRange && !coKhien) return false;

        Vector3 toi = FlatPos(t.position) - FlatPos(transform.position);
        if (toi.sqrMagnitude > 0.01f && Vector3.Dot(transform.forward, toi.normalized) < 0.5f) return false;   // chua quay mat ve muc tieu
        if (Mathf.Abs(t.position.y - transform.position.y) > 1.5f) return false;

        // Vom khien khong tinh la vuong (lop Khieng khong co trong mat na); bia, cay, nha, doc dat thi co
        int matNa = LayerMask.GetMask("Default", "Ground");
        return !Physics.Linecast(transform.position + Vector3.up, t.position + Vector3.up, matNa, QueryTriggerInteraction.Ignore);
    }

    void BatDauLuot()
    {
        SoLanLuot++;
        hoiLuot = GiayHoiLuot;
        luotConLai = GiayLuotToiDa;
        daGieoKhien = false;
        soKhungKet = 0;
        attackTimer = -1f;
        hitApplied = false;
        hinhLuot = VfxFactory.LuotBoXuong(transform);
        if (DaRaDon != null) DaRaDon(this, KieuLuotBatDau, target, target.position);
    }

    public void NgatLuot()
    {
        if (luotConLai <= 0f) return;
        luotConLai = 0f;
        velocity.x = 0f; velocity.z = 0f;
        if (hinhLuot != null) hinhLuot.Dung();
        hinhLuot = null;
    }

    /// <summary>Moi khung trong luc luot. Toi noi / cham vom / het gio deu ket thuc o day.</summary>
    void BuocLuot(float dt, float heSoToc)
    {
        var d = target != null ? target.GetComponent<Damageable>() : null;
        if (d == null || d.IsDead) { SoLanLuotHut++; NgatLuot(); return; }

        Vector3 toi = FlatPos(target.position) - FlatPos(transform.position);
        float dist = toi.magnitude;
        Vector3 huong = dist > 0.001f ? toi / dist : transform.forward;
        if (toi.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(huong);

        // Cham MAT VOM: gieo mot lan
        var k = target.GetComponent<Khieng>();
        if (!daGieoKhien && k != null && k.DangBat && dist <= k.banKinh + (cc != null ? cc.radius : 0.4f) + 0.3f)
        {
            daGieoKhien = true;
            float xs = EpXuyenChoPhepThu >= 0f ? EpXuyenChoPhepThu : XacSuatXuyenKhien;
            if (Random.value < xs)
            {
                SoLanLuotXuyen++;
                k.PhaVo();           // tat ca vach chan: luot tiep vao nguoi. Ban sao: chu khien nhan KieuLuotXuyenKhien roi vo that
                if (DaRaDon != null) DaRaDon(this, KieuLuotXuyenKhien, target, target.position);
            }
            else
            {
                SoLanLuotKhongXuyen++;
                d.GhiKeDanh(health);
                ChemTrung(d);        // TakeDamage -> Khieng.HapThu; chu khien tru mau qua KieuLuotTrungKhien
                if (DaRaDon != null) DaRaDon(this, KieuLuotTrungKhien, target, target.position);
                RaDonSauLuot();
                NgatLuot();
                return;
            }
        }

        // Toi noi: 1 nhat chem
        if (dist <= attackRange * 0.85f)
        {
            SoLanLuotTrung++;
            d.GhiKeDanh(health);
            ChemTrung(d);
            if (DaRaDon != null) DaRaDon(this, KieuLuotTrung, target, target.position);
            RaDonSauLuot();
            NgatLuot();
            return;
        }

        luotConLai -= dt;
        if (luotConLai <= 0f) { SoLanLuotHut++; NgatLuot(); return; }

        // Di: khong vuot qua cho dung; vuong vat 3 khung lien thi bo
        float buoc = Mathf.Min(TocLuot * heSoToc * dt, Mathf.Max(0f, dist - attackRange * 0.8f));
        Vector3 truoc = transform.position;
        velocity.x = 0f; velocity.z = 0f;
        if (cc != null && cc.enabled)
        {
            if (cc.isGrounded && velocity.y < 0f) velocity.y = -2f;
            velocity.y += -22f * dt;
            cc.Move(huong * buoc + Vector3.up * velocity.y * dt);
        }
        float daDi = Vector3.Distance(FlatPos(truoc), FlatPos(transform.position));
        if (buoc > 0.02f && daDi < buoc * 0.25f) soKhungKet++; else soKhungKet = 0;
        if (soKhungKet >= 3) { SoLanLuotHut++; NgatLuot(); return; }

        if (anim != null) anim.SetMoveSpeed(1f);
    }

    /// <summary>Toi noi: vung kiem cho khop nhat chem, roi chem tiep theo nhip thuong.</summary>
    void RaDonSauLuot()
    {
        TiengViTri.BoXuongToi(transform.position);
        cooldown = attackCooldown;
        if (anim != null) anim.PlayAttack(attackAnimTime);
        if (hoatHinhRieng != null) hoatHinhRieng.RaDon(attackAnimTime);
    }

    /// <summary>May khach dien lai 4 kieu don cua cu luot. Tra ve false neu khong phai kieu luot.</summary>
    bool DienLaiLuot(int kieu, Vector3 diemNgam, Damageable nanNhan)
    {
        if (kieu < KieuLuotBatDau || kieu > KieuLuotXuyenKhien) return false;

        if (kieu == KieuLuotBatDau)
        {
            if (hinhLuot != null) hinhLuot.Dung();
            hinhLuot = VfxFactory.LuotBoXuong(transform);
            return true;
        }

        if (kieu == KieuLuotXuyenKhien)
        {
            // Nguoi bi nham: vo that (roi goi trang thai bao moi may); nguoi khac: vo hinh tren ban sao cho kip luc
            var ai = nanNhan != null ? nanNhan : NguoiChoiGan(diemNgam, 1.5f);
            if (ai != null && ai.khieng != null) ai.khieng.PhaVo();
            return true;
        }

        // Trung nguoi hoac trung khien: nguoi bi nham moi that su mat mau / mat mau khien
        if (hinhLuot != null) { hinhLuot.Dung(); hinhLuot = null; }
        TiengViTri.BoXuongToi(transform.position);
        if (anim != null) anim.PlayAttack(attackAnimTime);
        if (hoatHinhRieng != null) hoatHinhRieng.RaDon(attackAnimTime);
        if (nanNhan != null && !nanNhan.IsDead)
        {
            nanNhan.GhiKeDanh(health);
            ChemTrung(nanNhan);
        }
        else
        {
            var ai = NguoiChoiGan(diemNgam, 1.5f);
            if (ai != null) VfxFactory.ChemTrung(ai, transform.position, 0f);
        }
        return true;
    }

    /// <summary>Ep luot ngay (bo qua hoi chieu va dieu kien) - chi phep thu.</summary>
    public void LuotNgayChoPhepThu()
    {
        hoiLuot = 0f;
        if (target != null) BatDauLuot();
    }
}
