using UnityEngine;

/// <summary>
/// KY NANG 15 - "TOC BIEN" (them 18/09/2026, nhom HO TRO).
///
/// Nguoi dung: bam la nhan vat DICH CHUYEN TUC THOI toi cho khac trong pham vi 15 m - bien mat o cho
/// dang dung va hien ra o cho moi (moi cho mot hieu ung). Hoi chieu 5 giay, MOI CAP GIAM 0,25 giay
/// (cap 5 con 4 giay).
///
/// Nhung cho nguoi dung chon khi toi hoi lai (18/09/2026):
///   - 40 nang luong.
///   - Ngam vao cho khong dung duoc (trong tuong, duoi nuoc, sau bia mo) thi VAN NHAY, nhung lui ve
///     diem trong gan cho ngam nhat tren duong thang - tuc la DI XUYEN duoc tuong / bia mo, mien la
///     diem den dung duoc.
///   - KHONG NIEM CHU: bam la di lien (niem 0,01 giay - vua du mot khung hinh de goi tin "toi vua tung
///     phep" bay sang may khac, khong thi nguoi ta chi thay nhan vat truot 15 m).
/// </summary>
public static class TocBien
{
    public const float Tam = 15f;
    public const float NangLuong = 40f;
    public const float HoiChieuGoc = 5f;
    /// <summary>Moi cap ky nang giam bang nay giay hoi chieu (nguoi dung chot).</summary>
    public const float GiamMoiCap = 0.25f;

    /// <summary>Cao hon mat dat bao nhieu thi coi la "dung duoc" - do dung nhan vat.</summary>
    public const float CaoNguoi = 1.8f;
    public const float BanKinhNguoi = 0.35f;

    /// <summary>Lui dan ve phia nguoi choi bang nay met moi buoc khi cho ngam khong dung duoc.</summary>
    public const float BuocLui = 0.5f;

    static int lopMatDat = -1;

    /// <summary>
    /// Do cao MAT DAT THAT tai cho do - CHI lop Ground.
    ///
    /// ⚠️ Khong duoc dung VfxFactory.GroundY: ham do tinh ca lop Default, nen ngam vao giua mot khoi da
    /// hay mai nha mo thi no tra ve NOC cua vat ay - cho do "dung duoc", va nguoi choi nhay tot len mai
    /// (menu 76 do duoc 18/09/2026). Cung cai bay Gio loc da vap khi loc treo len mai nha (GioLoc.MatDatY).
    /// </summary>
    public static float MatDatY(Vector3 p, float neuKhongCo)
    {
        if (lopMatDat < 0) lopMatDat = LayerMask.GetMask("Ground");
        RaycastHit hit;
        if (Physics.Raycast(p + Vector3.up * 30f, Vector3.down, out hit, 80f, lopMatDat, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return neuKhongCo;
    }

    /// <summary>Dem cho phep thu (menu 76).</summary>
    public static int SoLanNhay;
    public static float QuangDuongCuoi;

    /// <summary>Hoi chieu THAT SU o cap <paramref name="capKy"/>: 5 giay, moi cap giam 0,25.</summary>
    public static float HoiChieuTheoCap(int capKy)
    {
        return Mathf.Max(0.5f, HoiChieuGoc - GiamMoiCap * Mathf.Max(0, capKy - 1));
    }

    /// <summary>
    /// Cho do co dung duoc khong: khong dam vao vat can nao va co mat dat ben duoi.
    /// </summary>
    public static bool DungDuoc(Vector3 cho, int matNaVatCan)
    {
        cho.y = MatDatY(cho, cho.y);
        Vector3 duoi = cho + Vector3.up * (BanKinhNguoi + 0.05f);
        Vector3 tren = cho + Vector3.up * (CaoNguoi - BanKinhNguoi);
        return !Physics.CheckCapsule(duoi, tren, BanKinhNguoi, matNaVatCan, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Tim cho den: bat dau tu <paramref name="diemNgam"/> (da kep trong 15 m) roi LUI DAN ve phia
    /// nguoi choi cho toi khi gap cho dung duoc. Tra ve false neu ca duong deu khong co cho nao.
    /// </summary>
    public static bool TimChoDen(Vector3 tu, Vector3 diemNgam, int matNaVatCan, out Vector3 choDen)
    {
        Vector3 a = new Vector3(tu.x, 0f, tu.z);
        Vector3 b = new Vector3(diemNgam.x, 0f, diemNgam.z);
        Vector3 v = b - a;
        float xa = v.magnitude;
        choDen = tu;
        if (xa < 0.01f) return false;

        Vector3 huong = v / xa;
        xa = Mathf.Min(xa, Tam);

        for (float d = xa; d >= 1f; d -= BuocLui)
        {
            Vector3 thu = a + huong * d;
            thu.y = MatDatY(thu, tu.y);
            if (DungDuoc(thu, matNaVatCan)) { choDen = thu; return true; }
        }
        return false;
    }

    /// <summary>
    /// Nhay nguoi choi toi cho ngam. Tra ve false neu khong tim duoc cho nao dung duoc
    /// (luc do PlayerController da tru mana roi - nen goi <see cref="TimChoDen"/> truoc de kiem).
    /// </summary>
    public static bool Nhay(PlayerController pc, Vector3 choDen)
    {
        if (pc == null) return false;

        Vector3 choCu = pc.transform.position;
        VfxFactory.TocBienBienMat(choCu);

        // CharacterController giu vi tri rieng: doi transform ma khong tat no thi khung sau
        // no keo nguoi ve cho cu (cai bay quen thuoc cua Unity).
        var cc = pc.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        pc.transform.position = choDen;
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();

        VfxFactory.TocBienHienRa(choDen);

        SoLanNhay++;
        QuangDuongCuoi = Vector3.Distance(new Vector3(choCu.x, 0f, choCu.z), new Vector3(choDen.x, 0f, choDen.z));
        CameraShake.Shake(0.10f, 0.03f);
        return true;
    }
}
