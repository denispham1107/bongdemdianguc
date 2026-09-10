using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NHAN VAT NGUOI KHAC PHAI DI MUOT, KHONG NHAY COC.
///
/// Vi tri cua nguoi khac den 20 lan moi giay, con man hinh ve 60 lan. Dat
/// thang vi tri vua nhan duoc thi nhan vat nhay tung nac 50 ms mot - nhin nhu
/// tua hinh. Phai VE O GIUA hai moc da nhan.
///
/// MA MUON VE O GIUA THI PHAI CHAP NHAN TUT LAI PHIA SAU MOT CHUT: luon hien
/// nhan vat o thoi diem "bay gio tru di do day dem". Do day dem chinh la cai
/// gia phai tra de doi lay su muot ma.
///
/// DEM PHAI CO GIAN, KHONG DUOC CO DINH.
/// So do that o buoc 0 (mot may Wi-Fi, mot may 4G):
///   trung vi 28 ms, nhung p95 = 72,6 ms va dao dong 53,7 ms.
/// Dem co dinh mot nhip (17 ms) thi cu 20 goi lai co mot goi den muon hon
/// dem - luc do khong con gi de noi suy va nhan vat khung lai. Nen dem phai
/// tu day len khi duong truyen chap chon, va mong lai khi no on dinh.
/// </summary>
public class NoiSuy
{
    struct Moc
    {
        public int mocMayChu;      // gio may chu luc chup, mili giay
        public float nhanLuc;      // gio may nay luc nhan, giay
        public Vector3 viTri;
        public float gocY;
        public bool dangChay;
    }

    /// <summary>Giu lai bao nhieu moc. 20 moc o 20 goi/giay la mot giay -
    /// thua de noi suy, va du de con biet duong truyen dang the nao.</summary>
    const int SoMocGiuLai = 24;

    /// <summary>Dem mong nhat va day nhat, tinh bang giay.</summary>
    public const float DemMongNhat = 0.02f;
    public const float DemDayNhat = 0.30f;

    /// <summary>
    /// EP TRAN DO DAY DEM. Bang 0 la khong ep, de no tu co gian.
    ///
    /// De do cai gia phai tra khi chon dem mong: dem mong thi do tre thap
    /// nhung goi den muon se khong con gi de noi suy va nhan vat khung lai.
    /// Do bang so roi hay chon, dung doan.
    /// </summary>
    public float demToiDaEp;

    /// <summary>Dem doi cham the nao. Day len phai NHANH (mang vua xau di thi
    /// giat ngay), mong lai phai CHAM (mong voi qua roi lai giat).</summary>
    const float TocDayLen = 8f;
    const float TocMongLai = 0.4f;

    readonly List<Moc> mocs = new List<Moc>(SoMocGiuLai + 4);

    /// <summary>Lech giua gio may chu va gio may nay - do mot lan roi bam
    /// theo. Khong can chinh xac tuyet doi, chi can ON DINH.</summary>
    float lechDongHo;
    bool daCoLech;

    float demGiay = 0.05f;
    float jitterUocLuong;
    float khoangDenTruoc = -1f;

    /// <summary>
    /// KHOANG CACH THAT giua hai goi den, do lay chu khong tin hang so.
    ///
    /// Vi sao khong dung 1/DongBoTran.NhipGui: con so ay chi dung cho nguoi
    /// choi. Dan quai gui 10 lan moi giay - moc cach nhau 100 ms - ma dem lai
    /// duoc tinh theo 17 ms, tuc MONG HON KHOANG CACH HAI MOC sau lan. Khi ay
    /// noi suy luon het moc de ve o giua: nhan vat dung im mot lat roi NHAY
    /// mot cai toi moc moi, va mat nguoi ta doc cu nhay ay thanh "no di nhanh
    /// hon that".
    ///
    /// Nhip gui thuc te cung khong bang nhip khai bao: goi duoc gui trong
    /// Update, nen may chay 30 khung/giay thi chi gui duoc 30 goi/giay du co
    /// khai 60. Do lay la dung cho ca hai truong hop.
    /// </summary>
    float mocCachNhauDo = -1f;

    /// <summary>Khoang cach hai moc dang do duoc, giay. Chi de chan doan.</summary>
    public float MocCachNhauGiay
    {
        get { return mocCachNhauDo > 0f ? mocCachNhauDo : 1f / DongBoTran.NhipGui; }
    }

    // ---- Doc de chan doan ----
    public float DemGiay { get { return demGiay; } }
    public float JitterGiay { get { return jitterUocLuong; } }
    public int SoMoc { get { return mocs.Count; } }
    public Vector3 ViTriHienThi { get; private set; }
    public float GocHienThi { get; private set; }
    public bool DangChay { get; private set; }

    /// <summary>Da co du hai moc de ve o giua chua.</summary>
    public bool SanSang { get { return mocs.Count >= 2; } }

    /// <summary>
    /// Nhan mot moc moi tu may kia.
    /// <paramref name="mocMayChu"/> la gio LUC CHUP, tinh bang mili giay.
    /// </summary>
    public void Nhan(int mocMayChu, Vector3 viTri, float gocY, bool dangChay)
    {
        float bayGio = Time.unscaledTime;

        // Lan dau: ghi lay do lech giua hai dong ho
        if (!daCoLech)
        {
            lechDongHo = bayGio - mocMayChu / 1000f;
            daCoLech = true;
        }

        // Do jitter: khoang cach giua hai goi den lech bao nhieu so voi lan
        // truoc. Day moi la thu quyet dinh dem phai day bao nhieu - do tre
        // trung binh khong lam nhan vat giat, chi co dao dong moi lam.
        if (khoangDenTruoc >= 0f)
        {
            float khoang = bayGio - khoangDenTruoc;

            // Trung binh truot cua khoang cach that. Bo qua nhung khoang qua
            // dai (goi den cum sau mot lan nghen): chung lam con so nay phinh
            // ra va keo dem day len mai khong rut lai duoc.
            if (khoang < 0.5f)
            {
                mocCachNhauDo = mocCachNhauDo < 0f
                    ? khoang
                    : Mathf.Lerp(mocCachNhauDo, khoang, 0.05f);
            }

            // Jitter do so voi khoang cach THAT, khong so voi hang so: neu so
            // voi 17 ms trong khi goi that ve 100 ms mot lan thi moi goi deu
            // bi tinh la "lech 83 ms", va dem phinh len vi mot dao dong khong
            // he ton tai.
            float lech = Mathf.Abs(khoang - MocCachNhauGiay);
            jitterUocLuong = Mathf.Lerp(jitterUocLuong, lech, 0.1f);
        }
        khoangDenTruoc = bayGio;

        var m = new Moc
        {
            mocMayChu = mocMayChu,
            nhanLuc = bayGio,
            viTri = viTri,
            gocY = gocY,
            dangChay = dangChay
        };

        // Goi den KHONG DUNG THU TU la chuyen binh thuong tren kenh khong tin
        // cay - chen vao dung cho chu khong vut di.
        int i = mocs.Count;
        while (i > 0 && mocs[i - 1].mocMayChu > mocMayChu) i--;
        mocs.Insert(i, m);

        if (mocs.Count > SoMocGiuLai) mocs.RemoveAt(0);
    }

    /// <summary>Goi moi khung hinh. Tinh ra vi tri nen hien luc nay.</summary>
    public void CapNhat(float dt)
    {
        DieuChinhDem(dt);

        if (mocs.Count == 0) return;

        if (mocs.Count == 1)
        {
            ViTriHienThi = mocs[0].viTri;
            GocHienThi = mocs[0].gocY;
            DangChay = mocs[0].dangChay;
            return;
        }

        // Thoi diem muon hien: bay gio, quy ve gio may chu, tru di do day dem
        float gioMayChu = Time.unscaledTime - lechDongHo;
        float muonHien = gioMayChu - demGiay;
        int muonHienMs = Mathf.RoundToInt(muonHien * 1000f);

        // Tim hai moc kep lay thoi diem do
        int sau = -1;
        for (int i = 0; i < mocs.Count; i++)
            if (mocs[i].mocMayChu >= muonHienMs) { sau = i; break; }

        if (sau <= 0)
        {
            // Muon hien mot thoi diem cu hon moi thu dang giu, hoac moi hon -
            // bam lay moc gan nhat con hon la nhay lung tung.
            var m = sau == 0 ? mocs[0] : mocs[mocs.Count - 1];
            ViTriHienThi = m.viTri;
            GocHienThi = m.gocY;
            DangChay = m.dangChay;
            return;
        }

        var a = mocs[sau - 1];
        var b = mocs[sau];

        float khoangMs = b.mocMayChu - a.mocMayChu;
        float t = khoangMs <= 0f ? 0f : Mathf.Clamp01((muonHienMs - a.mocMayChu) / khoangMs);

        ViTriHienThi = Vector3.Lerp(a.viTri, b.viTri, t);
        GocHienThi = Mathf.LerpAngle(a.gocY, b.gocY, t);
        DangChay = t < 0.5f ? a.dangChay : b.dangChay;
    }

    /// <summary>
    /// Dem day len nhanh, mong lai cham.
    ///
    /// Day len cham thi mang vua xau di la nhan vat giat ngay - nguoi choi
    /// thay lien. Mong lai nhanh thi vua het mot con chap chon da voi rut dem,
    /// gap con sau lai giat tiep. Nen hai chieu co hai toc do khac han nhau.
    /// </summary>
    void DieuChinhDem(float dt)
    {
        // Hai lan dao dong cho du an toan, cong mot khoang giua hai moc.
        //
        // Khoang giua hai moc la san khong the pha: dem mong hon no thi khong
        // con moc sau de ve o giua, va nhan vat se dung im roi nhay tung cai -
        // trong nhu di nhanh hon that.
        //
        // DO LAY chu khong tin hang so: nguoi choi gui 60 lan/giay con dan quai
        // gui 10, ma ca hai dung chung lop nay. Va nhip gui thuc te con tut
        // theo khung hinh cua may gui nua.
        float mocCachNhau = MocCachNhauGiay;
        float tran = demToiDaEp > 0f ? demToiDaEp : DemDayNhat;
        float demCan = Mathf.Clamp(jitterUocLuong * 2f + mocCachNhau,
                                   DemMongNhat, tran);
        float toc = demCan > demGiay ? TocDayLen : TocMongLai;
        demGiay = Mathf.MoveTowards(demGiay, demCan, toc * dt);
    }

    public void Xoa()
    {
        mocs.Clear();
        daCoLech = false;
        khoangDenTruoc = -1f;
        mocCachNhauDo = -1f;
        jitterUocLuong = 0f;
        demGiay = 0.05f;
    }
}
