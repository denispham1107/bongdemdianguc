using UnityEngine;

/// <summary>
/// CAP DO, KINH NGHIEM VA DIEM KY NANG CUA NHAN VAT.
///
/// Nguoi dung chot (13/09/2026): vao tran ai cung CAP 1, giet quai va giet
/// nguoi choi khac thi duoc kinh nghiem, du thi len cap, toi da CAP 20
/// (13/09/2026 nguoi dung nang tu cap 10 len cap 20).
/// Moi cap: mau +15%, nang luong +10%, toc do +3,5%.
/// Bay ky nang lucdau deu KHOA; cap 1 co san MOT diem ky nang de mo mot cai,
/// va moi lan len cap duoc them mot diem - dung de mo them hoac nang cap.
/// Ky nang toi da CAP 5: moi cap +20% sat thuong, +10% nang luong tieu ton,
/// hieu ung keo dai them 0,15 giay; rieng Khien la +15% mau khien.
///
/// TINH THEO TRAN, KHONG CAT LAI. Moi tran la mot van dau rieng (nguoi song
/// sot cuoi cung thang) - giu cap giua cac tran thi nguoi choi lau nam vao
/// tran voi cap 20 con nguoi moi cap 1, khong con la mot van dau nua. Vao tran
/// la <see cref="BatDauTranMoi"/> xoa sach.
///
/// TAT CA DEU LA CUA NHAN VAT TREN MAY NAY. Cap ky nang cua NGUOI KHAC di kem
/// tung goi tung phep (xem GoiTin.MotPhep.capKyNang) - khong giu o day, vi may
/// nay khong theo doi tien do cua ho.
/// </summary>
public static class CapDo
{
    public const int CapToiDa = 20;
    public const int CapKyNangToiDa = 5;

    /// <summary>So hieu hai ky nang DUNG BINH (them 13/09/2026). Tu 25/09/2026 (nguoi dung): CO SAN CAP 1 luc
    /// vao tran, khong can mo khoa; nang cap duoc toi <see cref="CapBinhToiDa"/> - moi cap binh mau hoi them
    /// 75 mau, binh mana them 45 nang luong (PlayerController.MauBinhTheoCap / ManaBinhTheoCap).</summary>
    public const int KyBinhMau = 7;
    public const int KyBinhMana = 8;

    /// <summary>Qua cau bang (them 16/09/2026). So 9 chu KHONG chen vao sau Giut set: so hieu
    /// ky nang di qua mang va nam trong thu tu o da luu cua nguoi choi - doi so cu la ho
    /// bam nut binh ra phep khac.</summary>
    public const int KyQuaCauBang = 9;

    /// <summary>Gio loc (them 16/09/2026) - so 10, cung luat THEM O CUOI nhu Qua cau bang.</summary>
    public const int KyGioLoc = 10;

    /// <summary>Lua dia nguc (them 17/09/2026) - so 11, THEM O CUOI.</summary>
    public const int KyLuaDiaNguc = 11;

    /// <summary>Tang hinh (them 18/09/2026) - so 12, THEM O CUOI.</summary>
    public const int KyTangHinh = 12;

    /// <summary>Qua cau dien (them 18/09/2026) - so 13, THEM O CUOI.</summary>
    public const int KyCauDien = 13;

    /// <summary>Hoa loc xoay (them 18/09/2026) - so 14, THEM O CUOI.</summary>
    public const int KyHoaLocXoay = 14;

    /// <summary>Toc bien (them 18/09/2026) - so 15, THEM O CUOI.</summary>
    public const int KyTocBien = 15;

    // ================= NHOM BI DONG (them 19/09/2026, nguoi dung xin) =================
    // "Trong Sach phep them 1 nhom skill ten la bi dong, cac skill nam trong nhom nay moi khi mo khoa
    // hoac nang cap se tang vinh vien thuoc tinh cua nhan vat."
    // Bon ky nang khang: mo khoa giam 25% sat thuong cua he do TU NGUOI CHOI KHAC, moi cap them 5%
    // (cap 5 = 45%). Khong bam duoc, khong keo vao o ky nang (nguoi dung chot) - xem KhangHe.cs.

    /// <summary>Khang Lua (them 19/09/2026) - so 16, THEM O CUOI.</summary>
    public const int KyKhangLua = 16;
    /// <summary>Khang Bang - so 17.</summary>
    public const int KyKhangBang = 17;
    /// <summary>Khang Set - so 18.</summary>
    public const int KyKhangSet = 18;
    /// <summary>Khang Phong - so 19.</summary>
    public const int KyKhangPhong = 19;

    /// <summary>
    /// Toc do di chuyen (bi dong, them 25/09/2026) - so 20, THEM O CUOI. Mo khoa cong 10% TOC DO GOC cua
    /// nhan vat (toc do luc vao tran, chua tinh phan tang theo cap nhan vat), moi cap sau them 2,5% (cap 5 = 20%).
    /// </summary>
    public const int KyTocDo = 20;

    /// <summary>Ky nang 21 - MAY GIONG (nhom PHONG, 25/09/2026): vung may giong 6 m, 20 tia set trong 5 giay.
    /// Nguoi dung chon KHONG CAN dieu kien mo khoa.</summary>
    public const int KyMayGiong = 21;

    /// <summary>Phan toc do goc duoc cong them o cap <paramref name="capKy"/> cua ky nang Toc do (0 = chua mo).</summary>
    public static float TocThemTheoCap(int capKy)
    {
        return capKy <= 0 ? 0f : 0.10f + 0.025f * (Mathf.Min(capKy, CapKyNangToiDa) - 1);
    }

    /// <summary>Phan toc do goc cong them cua nhan vat tren may nay (doc cap ky nang Toc do).</summary>
    public static float TocThemBiDong { get { return TocThemTheoCap(CapCuaKyNang(KyTocDo)); } }

    /// <summary>Cap toi da cua binh mau / binh mana - nguoi dung chon 25/09/2026.</summary>
    public const int CapBinhToiDa = 3;

    /// <summary>Cap toi da cua TUNG ky nang: binh mau / binh mana la 3, con lai 5.</summary>
    public static int CapToiDaCua(int ky) { return LaKyBinh(ky) ? CapBinhToiDa : CapKyNangToiDa; }

    public static bool LaKyBinh(int ky) { return ky == KyBinhMau || ky == KyBinhMana; }

    /// <summary>
    /// Ky nang BI DONG: khong tung duoc, khong nam trong o ky nang - chi mo khoa / nang cap trong
    /// Sach phep roi no tu cong vinh vien vao thuoc tinh nhan vat (nguoi dung 19/09/2026).
    /// </summary>
    public static bool LaKyBiDong(int ky)
    {
        return ky == KyKhangLua || ky == KyKhangBang || ky == KyKhangSet || ky == KyKhangPhong || ky == KyTocDo;
    }

    /// <summary>
    /// Toc do chi tang toi cap nay, sau do dung (nguoi dung chot 13/09/2026). Tang
    /// deu toi cap 20 thi toc do x1,92 (~10 m/giay) - chay vuot moi loai quai (nhanh
    /// nhat 5,98 m/giay). Dung o cap 10: x1,363 (~7,1 m/giay).
    /// </summary>
    public const int CapTangTocToiDa = 10;
    public const int SoKyNang = 22;      // 7 phep + binh mau + binh mana + qua cau bang + gio loc + lua dia nguc + tang hinh + qua cau dien + hoa loc xoay + toc bien + 4 khang bi dong + toc do + may giong

    /// <summary>Giet mot nguoi choi khac duoc bao nhieu kinh nghiem.</summary>
    public const int KnGietNguoi = 250;

    // ================================================================
    //  TRANG THAI
    // ================================================================

    public static int Cap { get; private set; }
    public static int KinhNghiem { get; private set; }        // tich luy trong CAP hien tai
    public static int DiemKyNang { get; private set; }

    /// <summary>So binh mau / binh mana dang mang - nhat duoc khi giet quai, tinh theo tran.</summary>
    public static int SoBinhMau { get; private set; }
    public static int SoBinhMana { get; private set; }

    public static int SoBinh(int ky) { return ky == KyBinhMau ? SoBinhMau : ky == KyBinhMana ? SoBinhMana : 0; }

    /// <summary>Cong mot binh vua nhat duoc.</summary>
    public static void ThemBinh(int ky)
    {
        BaoDamCoSan();
        if (ky == KyBinhMau) SoBinhMau++;
        else if (ky == KyBinhMana) SoBinhMana++;
        else return;
        if (KhiDoi != null) KhiDoi();
    }

    /// <summary>Bot mot binh. Tra false neu het binh.</summary>
    public static bool BotBinh(int ky)
    {
        BaoDamCoSan();
        if (ky == KyBinhMau && SoBinhMau > 0) SoBinhMau--;
        else if (ky == KyBinhMana && SoBinhMana > 0) SoBinhMana--;
        else return false;
        if (KhiDoi != null) KhiDoi();
        return true;
    }

    /// <summary>Cap cua tung ky nang, 0 = con khoa.</summary>
    static readonly int[] capKyNang = new int[SoKyNang];

    /// <summary>Bao ra moi khi cap / kinh nghiem / diem ky nang doi - de HUD ve lai.</summary>
    public static event System.Action KhiDoi;

    /// <summary>Bao ra khi vua len cap (tham so la cap moi) - de nhan vat cong mau.</summary>
    public static event System.Action<int> KhiLenCap;

    static bool daDung;

    static void BaoDamCoSan() { if (!daDung) BatDauTranMoi(); }

    /// <summary>Xoa sach, bat dau mot van moi: cap 1, khong kinh nghiem, mot diem ky nang.</summary>
    public static void BatDauTranMoi()
    {
        daDung = true;
        Cap = 1;
        KinhNghiem = 0;
        DiemKyNang = 1;
        SoBinhMau = 0;
        SoBinhMana = 0;
        for (int i = 0; i < SoKyNang; i++) capKyNang[i] = 0;
        // Binh mau / binh mana CO SAN cap 1 (nguoi dung 25/09/2026) - khong ton diem ky nang
        capKyNang[KyBinhMau] = 1;
        capKyNang[KyBinhMana] = 1;
        if (KhiDoi != null) KhiDoi();
    }

    // ================================================================
    //  BANG KINH NGHIEM
    // ================================================================
    //
    // Cang len cao cang lau. Cap 1-10: moi bac nhan 1,35 lan bac truoc. Cap
    // 10-20 (them 13/09/2026): moi bac chi nhan 1,15 lan - giu 1,35 thi bac
    // 19->20 can 16 000 va tong len cap 20 la 62 000 kinh nghiem, choi mot minh
    // phai qua ~22 dot, mot tran khong ai cham noi. Lam tron cho de nhin. Con so
    // cu the va ly do nam trong kinhnghiem.md.

    static readonly int[] canDeLen =
    {
        //  1->2  2->3  3->4  4->5  5->6  6->7  7->8  8->9  9->10
            100,  135,  180,  245,  330,  445,  600,  810,  1090,
        // 10->11 11->12 12->13 13->14 14->15 15->16 16->17 17->18 18->19 19->20
            1250,  1440,  1660,  1910,  2200,  2530,  2910,  3350,  3850,  4430
    };

    /// <summary>Kinh nghiem can de tu <paramref name="cap"/> len cap ke tiep. 0 neu da toi da.</summary>
    public static int CanDeLenCap(int cap)
    {
        if (cap < 1 || cap >= CapToiDa) return 0;
        return canDeLen[cap - 1];
    }

    /// <summary>Phan tram da di duoc trong cap hien tai, 0..1.</summary>
    public static float TienDo01
    {
        get
        {
            BaoDamCoSan();
            int can = CanDeLenCap(Cap);
            return can <= 0 ? 1f : Mathf.Clamp01((float)KinhNghiem / can);
        }
    }

    /// <summary>Kinh nghiem giet QUAI nhan them - giet nguoi choi khong doi. Nguoi dung 26/09/2026: "tang them 35%", roi cung ngay
    /// "tang them 20%" TREN MUC HIEN TAI (chon) -> 1,35 x 1,20 = 1,62 so voi goc.</summary>
    public const float HeSoKnQuai = 1.35f * 1.20f;

    /// <summary>
    /// Kinh nghiem moi con quai = muc goc x HeSoKnQuai, lam tron nua len (Mathf.RoundToInt lam tron ve so CHAN: 40,5 -> 40).
    /// Con nao khoe hoac phien hon thi dang gia hon.
    /// </summary>
    public static int KnCuaQuai(MonsterType loai)
    {
        return Mathf.FloorToInt(KnGocCuaQuai(loai) * HeSoKnQuai + 0.5f);
    }

    /// <summary>Muc kinh nghiem GOC (truoc 26/09/2026) - chi de nhan he so.</summary>
    public static int KnGocCuaQuai(MonsterType loai)
    {
        switch (loai)
        {
            case MonsterType.Skeleton: return 18;
            case MonsterType.Ghoul: return 20;
            case MonsterType.Fallen: return 22;
            case MonsterType.QuyCay: return 30;      // chay rat nhanh, ban tia set
            case MonsterType.Witch: return 32;       // danh tu xa
            case MonsterType.QuyDu: return 40;       // goi thien thach
            case MonsterType.Brute: return 70;       // to va khoe nhat
            default: return 20;
        }
    }

    /// <summary>
    /// Cong kinh nghiem. Len duoc bao nhieu cap thi len bay nhieu (giet mot
    /// con to luc sap len cap co the nhay hai bac).
    /// </summary>
    public static void Them(int kn)
    {
        BaoDamCoSan();
        if (kn <= 0 || Cap >= CapToiDa) return;

        KinhNghiem += kn;
        while (Cap < CapToiDa)
        {
            int can = CanDeLenCap(Cap);
            if (can <= 0 || KinhNghiem < can) break;
            KinhNghiem -= can;
            Cap++;
            DiemKyNang++;
            if (KhiLenCap != null) KhiLenCap(Cap);
        }

        // Da toi da thi khong giu kinh nghiem thua lam gi
        if (Cap >= CapToiDa) KinhNghiem = 0;
        if (KhiDoi != null) KhiDoi();
    }

    // ================================================================
    //  CHI SO NHAN VAT THEO CAP
    // ================================================================
    //
    // NHAN DON chu khong cong don: "moi cap tang them 15%" nghia la 15% cua
    // cai dang co. Cong tuyen tinh thi cap 10 chi hon cap 1 co 2,35 lan trong
    // khi nhan don ra 3,52 lan (cap 20: mau x14,2, nang luong x6,1, toc do x1,92) - va cong tuyen tinh lam nhung cap cuoi gan nhu
    // khong thay khac gi.

    public static float HeSoMauTheoCap(int cap) { return Mathf.Pow(1.15f, Mathf.Max(0, cap - 1)); }
    public static float HeSoManaTheoCap(int cap) { return Mathf.Pow(1.10f, Mathf.Max(0, cap - 1)); }
    public static float HeSoTocTheoCap(int cap) { return Mathf.Pow(1.035f, Mathf.Clamp(cap - 1, 0, CapTangTocToiDa - 1)); }

    public static float HeSoMau { get { BaoDamCoSan(); return HeSoMauTheoCap(Cap); } }
    public static float HeSoMana { get { BaoDamCoSan(); return HeSoManaTheoCap(Cap); } }
    public static float HeSoToc { get { BaoDamCoSan(); return HeSoTocTheoCap(Cap); } }

    // ================================================================
    //  KY NANG: KHOA - MO - NANG CAP
    // ================================================================

    /// <summary>Cap cua mot ky nang, 0 = con khoa.</summary>
    public static int CapCuaKyNang(int ky)
    {
        BaoDamCoSan();
        return (ky >= 0 && ky < SoKyNang) ? capKyNang[ky] : 0;
    }

    public static bool DaMo(int ky) { return CapCuaKyNang(ky) > 0; }

    // ================================================================
    //  DIEU KIEN MO KHOA THEO BAC (nguoi dung xin 19/09/2026)
    // ================================================================
    //
    // Moi nhom he co mot duong len: ky nang re nhat mo truoc, len du cap moi mo duoc cai sau.
    //   LUA : Qua cau lua cap 2 -> Thien thach;  Thien thach cap 5 -> Lua dia nguc
    //   BANG: Qua cau bang cap 2 -> Mua bang;    Mua bang cap 5   -> Tang hinh
    //   SET : Giut set cap 2   -> Sam set;       Sam set cap 5    -> Qua cau dien
    //   PHONG: Gio loc cap 2   -> Loc xoay;      Loc xoay cap 5   -> Hoa loc xoay
    //
    // CHI CHAN LUC MO KHOA. Mo roi thi nang cap tu do - nguoi dung viet "moi duoc mo khoa".
    // Nhom HO TRO va BI DONG khong co dieu kien nao.

    /// <summary>{ky nang, ky nang phai co truoc, cap toi thieu cua no}.</summary>
    static readonly int[][] dieuKienMo = {
        new[] { 4, 0, 2 },                                  // Thien thach  <- Qua cau lua cap 2
        new[] { KyLuaDiaNguc, 4, 5 },                       // Lua dia nguc <- Thien thach cap 5
        new[] { 1, KyQuaCauBang, 2 },                       // Mua bang     <- Qua cau bang cap 2
        new[] { KyTangHinh, 1, 5 },                         // Tang hinh    <- Mua bang cap 5
        new[] { 2, 6, 2 },                                  // Sam set      <- Giut set cap 2
        new[] { KyCauDien, 2, 5 },                          // Qua cau dien <- Sam set cap 5
        new[] { 3, KyGioLoc, 2 },                           // Loc xoay     <- Gio loc cap 2
        new[] { KyHoaLocXoay, 3, 5 },                       // Hoa loc xoay <- Loc xoay cap 5
    };

    /// <summary>Ky nang phai co truoc moi mo duoc <paramref name="ky"/>; -1 neu khong doi gi.</summary>
    public static int KyCanTruoc(int ky)
    {
        for (int i = 0; i < dieuKienMo.Length; i++)
            if (dieuKienMo[i][0] == ky) return dieuKienMo[i][1];
        return -1;
    }

    /// <summary>Cap toi thieu cua ky nang can truoc; 0 neu khong doi gi.</summary>
    public static int CapCanTruoc(int ky)
    {
        for (int i = 0; i < dieuKienMo.Length; i++)
            if (dieuKienMo[i][0] == ky) return dieuKienMo[i][2];
        return 0;
    }

    /// <summary>Da du dieu kien BAC de mo ky nang nay chua (khong xet diem ky nang).</summary>
    public static bool DuBacDeMo(int ky)
    {
        int can = KyCanTruoc(ky);
        return can < 0 || CapCuaKyNang(can) >= CapCanTruoc(ky);
    }

    public static bool MoKhoaDuoc(int ky)
    {
        return ky >= 0 && ky < SoKyNang && !DaMo(ky) && DiemKyNang > 0 && DuBacDeMo(ky);
    }

    public static bool NangCapDuoc(int ky)
    {
        return ky >= 0 && ky < SoKyNang && DaMo(ky)
               && capKyNang[ky] < CapToiDaCua(ky) && DiemKyNang > 0;
    }

    /// <summary>Mo khoa mot ky nang (thanh cap 1). Tra ve false neu khong du dieu kien.</summary>
    public static bool MoKhoa(int ky)
    {
        if (!MoKhoaDuoc(ky)) return false;
        capKyNang[ky] = 1;
        DiemKyNang--;
        if (KhiDoi != null) KhiDoi();
        return true;
    }

    /// <summary>
    /// Mo khoa ky nang nay CUNG CA DUONG dan toi no: nang cac ky nang phai co truoc len du cap,
    /// tu them diem ky nang neu thieu.
    ///
    /// ⚠️ CHI DANH CHO PHEP THU. Trong tran nguoi choi phai tu di duong ay - dieu kien bac
    /// (nguoi dung xin 19/09/2026) chinh la thu bat ho di. Cac kich ban chay thu thi chi muon
    /// "cho toi dung thu ky nang X" nen goi ham nay thay cho MoKhoa.
    /// </summary>
    public static void MoCaDuongChoPhepThu(int ky)
    {
        int can = KyCanTruoc(ky);
        if (can >= 0)
        {
            MoCaDuongChoPhepThu(can);
            int capCan = CapCanTruoc(ky);
            for (int i = 0; i < 40 && CapCuaKyNang(can) < capCan; i++)
            {
                if (DiemKyNang <= 0) ThemDiemChoPhepThu(1);
                if (!DaMo(can)) { if (!MoKhoa(can)) break; }
                else if (!NangCap(can)) break;
            }
        }
        if (DaMo(ky)) return;
        if (DiemKyNang <= 0) ThemDiemChoPhepThu(1);
        MoKhoa(ky);
    }

    /// <summary>
    /// Cho them diem ky nang MA KHONG cho kinh nghiem.
    ///
    /// ⚠️ CHI DANH CHO PHEP THU. Ban dau MoCaDuongChoPhepThu di duong vong "them kinh nghiem de
    /// len cap lay diem", nhung mo ca duong ton toi 3 diem moi ky nang nen nhan vat bi day thang
    /// len CAP 20 - va o cap toi da thi giet quai khong con duoc kinh nghiem nua, menu 61 do ra
    /// "+0 kinh nghiem" o moi ky nang (19/09/2026).
    /// </summary>
    public static void ThemDiemChoPhepThu(int n)
    {
        BaoDamCoSan();
        if (n <= 0) return;
        DiemKyNang += n;
        if (KhiDoi != null) KhiDoi();
    }

    /// <summary>Nang mot ky nang da mo len mot cap.</summary>
    public static bool NangCap(int ky)
    {
        if (!NangCapDuoc(ky)) return false;
        capKyNang[ky]++;
        DiemKyNang--;
        if (KhiDoi != null) KhiDoi();
        return true;
    }

    // ================================================================
    //  SUC MANH KY NANG THEO CAP
    // ================================================================
    //
    // Nhan THEO CAP TRUYEN VAO chu khong doc thang capKyNang: phep cua nguoi
    // khac duoc phat lai tren may nay phai manh dung bang cap CUA HO, ma cap ay
    // di kem goi tin. Doc thang o day thi don cua nguoi cap 5 vao may minh lai
    // yeu di theo cap minh - hai may thay hai con so sat thuong khac nhau.

    public static float SatThuongTheoCap(int capKy) { return Mathf.Pow(1.20f, Mathf.Max(0, capKy - 1)); }
    public static float ManaTheoCap(int capKy) { return Mathf.Pow(1.10f, Mathf.Max(0, capKy - 1)); }
    public static float MauKhiengTheoCap(int capKy) { return Mathf.Pow(1.15f, Mathf.Max(0, capKy - 1)); }

    /// <summary>Hieu ung (dong bang, choang, chay...) keo dai them bao nhieu giay.</summary>
    public static float ThemGiayHieuUngTheoCap(int capKy) { return 0.15f * Mathf.Max(0, capKy - 1); }
}
