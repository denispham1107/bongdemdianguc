using UnityEngine;

/// <summary>
/// CAP DO, KINH NGHIEM VA DIEM KY NANG CUA NHAN VAT.
///
/// Nguoi dung chot (13/09/2026): vao tran ai cung CAP 1, giet quai va giet
/// nguoi choi khac thi duoc kinh nghiem, du thi len cap, toi da CAP 10.
/// Moi cap: mau +15%, nang luong +10%, toc do +3,5%.
/// Bay ky nang lucdau deu KHOA; cap 1 co san MOT diem ky nang de mo mot cai,
/// va moi lan len cap duoc them mot diem - dung de mo them hoac nang cap.
/// Ky nang toi da CAP 5: moi cap +20% sat thuong, +10% nang luong tieu ton,
/// hieu ung keo dai them 0,15 giay; rieng Khien la +15% mau khien.
///
/// TINH THEO TRAN, KHONG CAT LAI. Moi tran la mot van dau rieng (nguoi song
/// sot cuoi cung thang) - giu cap giua cac tran thi nguoi choi lau nam vao
/// tran voi cap 10 con nguoi moi cap 1, khong con la mot van dau nua. Vao tran
/// la <see cref="BatDauTranMoi"/> xoa sach.
///
/// TAT CA DEU LA CUA NHAN VAT TREN MAY NAY. Cap ky nang cua NGUOI KHAC di kem
/// tung goi tung phep (xem GoiTin.MotPhep.capKyNang) - khong giu o day, vi may
/// nay khong theo doi tien do cua ho.
/// </summary>
public static class CapDo
{
    public const int CapToiDa = 10;
    public const int CapKyNangToiDa = 5;
    public const int SoKyNang = 7;

    /// <summary>Giet mot nguoi choi khac duoc bao nhieu kinh nghiem.</summary>
    public const int KnGietNguoi = 250;

    // ================================================================
    //  TRANG THAI
    // ================================================================

    public static int Cap { get; private set; }
    public static int KinhNghiem { get; private set; }        // tich luy trong CAP hien tai
    public static int DiemKyNang { get; private set; }

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
        for (int i = 0; i < SoKyNang; i++) capKyNang[i] = 0;
        if (KhiDoi != null) KhiDoi();
    }

    // ================================================================
    //  BANG KINH NGHIEM
    // ================================================================
    //
    // Cang len cao cang lau: moi bac nhan 1,35 lan bac truoc, lam tron cho de
    // nhin. Con so cu the va ly do nam trong kinhnghiem.md.

    static readonly int[] canDeLen =
    {
        //  1->2  2->3  3->4  4->5  5->6  6->7  7->8  8->9  9->10
            100,  135,  180,  245,  330,  445,  600,  810,  1090
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

    /// <summary>
    /// Kinh nghiem moi con quai. Con nao khoe hoac phien hon thi dang gia hon.
    /// </summary>
    public static int KnCuaQuai(MonsterType loai)
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
    // khi nhan don ra 3,52 lan - va cong tuyen tinh lam nhung cap cuoi gan nhu
    // khong thay khac gi.

    public static float HeSoMauTheoCap(int cap) { return Mathf.Pow(1.15f, Mathf.Max(0, cap - 1)); }
    public static float HeSoManaTheoCap(int cap) { return Mathf.Pow(1.10f, Mathf.Max(0, cap - 1)); }
    public static float HeSoTocTheoCap(int cap) { return Mathf.Pow(1.035f, Mathf.Max(0, cap - 1)); }

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

    public static bool MoKhoaDuoc(int ky)
    {
        return ky >= 0 && ky < SoKyNang && !DaMo(ky) && DiemKyNang > 0;
    }

    public static bool NangCapDuoc(int ky)
    {
        return ky >= 0 && ky < SoKyNang && DaMo(ky)
               && capKyNang[ky] < CapKyNangToiDa && DiemKyNang > 0;
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
