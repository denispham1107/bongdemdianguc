using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PHONG CHO: tao, vao, ra, san sang, bat dau.
///
/// Du lieu nam tren Realtime Database (khong phai Firestore) vi hai le:
/// no re hon nhieu cho kieu ghi nho lien tuc, va no co onDisconnect - thu ma
/// ban REST nay chua dung toi nhung ban dong bo tran o giai doan sau se can.
///
/// DOC BANG CACH HOI LAI THEO NHIP: REST khong co luong du lieu day ve. Sanh
/// cho hoi moi <see cref="NhipHoiSanh"/> giay, trong phong thi hoi day hon.
/// Voi mot cai sanh thi the la du - nguoi choi khong nhan ra mot giay tre.
/// </summary>
public static class PhongMang
{
    public const int SoNguoiToiDa = 4;
    public const int GiayDemNguoc = 10;

    /// <summary>Bao lau hoi lai danh sach phong mot lan, tinh bang giay.</summary>
    public const float NhipHoiSanh = 2.0f;

    /// <summary>Trong phong thi hoi day hon - nguoi choi cho doi nen sot ruot.</summary>
    public const float NhipHoiTrongPhong = 1.0f;

    // ================================================================
    //  KIEU DU LIEU
    // ================================================================

    public class NguoiTrongPhong
    {
        public string uid, ten;
        public bool sanSang;
        public int cho;
    }

    public class Phong
    {
        public string ma, ten, hostUid, hostTen, manChoi, trangThai;
        public int soNguoi, toiDa;
        public double batDauLuc;
        public double taoLuc, capNhatLuc;
        public readonly List<NguoiTrongPhong> nguoiChoi = new List<NguoiTrongPhong>();

        public bool DangCho { get { return trangThai == "cho"; } }
        public bool ConCho  { get { return soNguoi < (toiDa <= 0 ? SoNguoiToiDa : toiDa); } }
    }

    /// <summary>Phong dang o trong, null neu dang o ngoai sanh.</summary>
    public static Phong PhongHienTai;

    public static bool LaHost
    {
        get { return PhongHienTai != null && PhongHienTai.hostUid == FirebaseMang.Uid; }
    }

    // ================================================================
    //  DONG HO MAY CHU
    // ================================================================
    //
    // Dem nguoc phai khop tren moi may. Neu may nao cung dem bang dong ho cua
    // no thi may lech gio se vao tran som hoac muon vai giay.
    //
    // Cach lam: host ghi mot MOC thoi gian may chu, moi may do do lech giua
    // dong ho cua minh voi dong ho Firebase roi tu tinh con bao nhieu giay.

    static double doLechDongHo;

    public static IEnumerator DoDongHoMayChu()
    {
        string ketQua = null;
        yield return FirebaseMang.Doc(".info/serverTimeOffset", s => ketQua = s);

        // Nhanh .info khong doc duoc bang REST, nen do bang cach khac: ghi mot
        // moc thoi gian may chu roi doc lai xem no bang bao nhieu.
        if (string.IsNullOrEmpty(ketQua) || ketQua == "null")
        {
            double truoc = GioMay();
            string ma = null;
            yield return FirebaseMang.Them("dongho",
                "{\"t\":{\".sv\":\"timestamp\"}}", (k, loi) => ma = k);

            if (!string.IsNullOrEmpty(ma))
            {
                string doc = null;
                yield return FirebaseMang.Doc("dongho/" + ma + "/t", s => doc = s);

                double mayChu;
                if (double.TryParse(doc, out mayChu))
                {
                    double sau = GioMay();
                    // Tru di nua vong di-ve cho bot lech
                    doLechDongHo = mayChu - (truoc + (sau - truoc) * 0.5);
                }
                yield return FirebaseMang.Xoa("dongho/" + ma, (ok, loi) => { });
            }
        }
    }

    static double GioMay()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
               .TotalMilliseconds;
    }

    public static double GioMayChu() { return GioMay() + doLechDongHo; }

    /// <summary>Con bao nhieu giay nua thi vao tran. Am nghia la den gio roi.</summary>
    public static double ConLaiGiay()
    {
        if (PhongHienTai == null || PhongHienTai.batDauLuc <= 0) return 999;
        return (PhongHienTai.batDauLuc - GioMayChu()) / 1000.0;
    }

    /// <summary>
    /// DA DEN LUC NHAY VAO MAN CHOI CHUA.
    ///
    /// Phai xet CA HAI dieu kien, va day la cho tung hong that:
    ///
    /// Chi xet "dem nguoc va het gio" thoi thi khach bi ket lai o sanh. Vi khi
    /// host vao tran, no ghi trangThai = "dangChoi". May khach hoi lai phong
    /// moi giay mot lan, nen ngay sau do lan hoi ke tiep de len trangThai
    /// "dangChoi" - the la dieu kien "== demNguoc" sai VINH VIEN va khach dung
    /// mai o MainMenu. Voi do tre ghi ~300 ms, cua so de khach kip chi vai tram
    /// mili giay: hong gan nhu moi lan.
    ///
    /// Nen "dangChoi" cung phai la mot ly do de vao: no co nghia la tran da bat
    /// dau roi, ai con trong phong thi vao ngay.
    ///
    /// Ham thuan, khong dung bien toan cuc - de con kiem duoc bang so.
    /// </summary>
    public static bool DenGioVaoTran(Phong p, double conLaiGiay)
    {
        if (p == null) return false;
        if (p.trangThai == "dangChoi") return true;
        return p.trangThai == "demNguoc" && conLaiGiay <= 0;
    }

    // ================================================================
    //  NHIP SONG CUA PHONG
    // ================================================================

    /// <summary>Khong dap nhip qua ngan nay giay thi coi nhu phong da chet.</summary>
    public const double GiayCoiLaChet = 60.0;

    /// <summary>
    /// CHU PHONG DAP NHIP DE BAO "PHONG NAY CON SONG".
    ///
    /// Tang mang di qua REST, ma REST KHONG co onDisconnect() - thu duy nhat
    /// tu don phong khi nguoi ta dong tab. Khong co gi thay the thi phong nam
    /// lai vinh vien: sanh day nhung phong ma, nguoi choi bam vao roi ngoi doi
    /// mot chu phong khong bao gio quay lai.
    ///
    /// Nen chu phong ghi mot moc thoi gian moi nhip. Phong nao qua
    /// <see cref="GiayCoiLaChet"/> giay khong nhuc nhich thi bi loc khoi sanh.
    ///
    /// Chi CHU PHONG dap nhip, khong phai moi nguoi: phong la cua chu phong,
    /// chu phong bo di thi phong khong con y nghia.
    /// </summary>
    public static IEnumerator DapNhip()
    {
        if (PhongHienTai == null || !LaHost) yield break;
        yield return FirebaseMang.Ghi("phong/" + PhongHienTai.ma + "/capNhatLuc",
                                      ((long)GioMayChu()).ToString(), (ok, e) => { });
    }

    /// <summary>
    /// Phong con song khong. Ham thuan de con kiem duoc bang so.
    ///
    /// Phong vua tao chua kip dap nhip lan nao thi lay <c>taoLuc</c> thay the -
    /// khong thi phong nao cung chet ngay giay dau tien.
    /// </summary>
    public static bool PhongConSong(Phong p, double gioBayGio)
    {
        if (p == null) return false;
        double moc = p.capNhatLuc > 0 ? p.capNhatLuc : p.taoLuc;
        if (moc <= 0) return false;      // khong biet gi ve no thi dung tin
        return (gioBayGio - moc) / 1000.0 <= GiayCoiLaChet;
    }

    // ================================================================
    //  TAO / VAO / RA
    // ================================================================

    public static IEnumerator TaoPhong(string tenPhong, string manChoi,
                                       Action<bool, string> xong)
    {
        string ten = string.IsNullOrEmpty(tenPhong)
            ? "Phong cua " + FirebaseMang.TenHienThi : tenPhong;
        if (ten.Length > 24) ten = ten.Substring(0, 24);

        string tenToi = FirebaseMang.TenHienThi ?? "NguoiChoi";
        string than =
            "{\"ten\":\"" + FirebaseMang.Thoat(ten) + "\","
            + "\"hostUid\":\"" + FirebaseMang.Uid + "\","
            + "\"hostTen\":\"" + FirebaseMang.Thoat(tenToi) + "\","
            + "\"manChoi\":\"" + manChoi + "\","
            + "\"trangThai\":\"cho\","
            + "\"toiDa\":" + SoNguoiToiDa + ","
            + "\"soNguoi\":1,"
            + "\"taoLuc\":{\".sv\":\"timestamp\"},"
            + "\"nguoiChoi\":{\"" + FirebaseMang.Uid + "\":{"
                + "\"ten\":\"" + FirebaseMang.Thoat(tenToi) + "\","
                + "\"sanSang\":true,\"cho\":0,\"vaoLuc\":" + (long)GioMay() + "}}}";

        string ma = null; string loi = null;
        yield return FirebaseMang.Them("phong", than, (k, e) => { ma = k; loi = e; });

        if (string.IsNullOrEmpty(ma)) { xong(false, loi ?? "Khong tao duoc phong."); yield break; }

        yield return TaiLaiPhong(ma, (ok, e) => xong(ok, e));
    }

    public static IEnumerator VaoPhong(string maPhong, Action<bool, string> xong)
    {
        string json = null;
        yield return FirebaseMang.Doc("phong/" + maPhong, s => json = s);
        if (string.IsNullOrEmpty(json) || json == "null")
        { xong(false, "Phong nay khong con nua."); yield break; }

        var p = DocPhong(maPhong, json);
        if (!p.DangCho) { xong(false, "Phong nay da bat dau choi roi."); yield break; }

        bool daTrongPhong = p.nguoiChoi.Exists(n => n.uid == FirebaseMang.Uid);
        if (!daTrongPhong)
        {
            if (!p.ConCho) { xong(false, "Phong da du " + SoNguoiToiDa + " nguoi."); yield break; }

            // Chon cho ngoi con trong, de bon nhan vat khong de chong len nhau
            // khi vao man choi.
            var daDung = new HashSet<int>();
            foreach (var n in p.nguoiChoi) daDung.Add(n.cho);
            int cho = 0;
            while (daDung.Contains(cho) && cho < SoNguoiToiDa - 1) cho++;

            string tenToi = FirebaseMang.TenHienThi ?? "NguoiChoi";
            string than = "{\"ten\":\"" + FirebaseMang.Thoat(tenToi) + "\","
                        + "\"sanSang\":false,\"cho\":" + cho + ","
                        + "\"vaoLuc\":" + (long)GioMay() + "}";

            bool ok = false; string loi = null;
            yield return FirebaseMang.Ghi("phong/" + maPhong + "/nguoiChoi/" + FirebaseMang.Uid,
                                          than, (o, e) => { ok = o; loi = e; });
            if (!ok) { xong(false, loi ?? "Khong vao duoc phong."); yield break; }

            // Cap nhat so nguoi. Luat cho phep nhich mot don vi mot lan.
            yield return FirebaseMang.Ghi("phong/" + maPhong + "/soNguoi",
                                          (p.soNguoi + 1).ToString(), (o, e) => { });
        }

        yield return TaiLaiPhong(maPhong, (ok, e) => xong(ok, e));
    }

    /// <summary>Man mac dinh khi khong ai chon - xem <see cref="VaoPhongNhanh"/>.</summary>
    public const string ManMacDinh = "Act2";

    /// <summary>
    /// AI GIU PHONG, AI NHUONG, khi hai nguoi lo tao phong cung mot luc.
    ///
    /// Tra ve ma cua phong duoc giu lai. Khoa cua Realtime Database (push key)
    /// tang dan theo thoi gian tao, nen so chuoi la du de biet phong nao co
    /// truoc - va quan trong hon: HAI MAY DEU RA CUNG MOT DAP AN ma khong phai
    /// hoi nhau cau nao. Neu moi may tu chon theo y minh thi ca hai cung nhuong
    /// (khong ai o dau ca) hoac ca hai cung giu (van hai phong).
    ///
    /// Ham thuan de con kiem duoc bang so.
    /// </summary>
    public static string PhongDuocGiu(string maA, string maB)
    {
        if (string.IsNullOrEmpty(maA)) return maB;
        if (string.IsNullOrEmpty(maB)) return maA;
        return string.CompareOrdinal(maA, maB) <= 0 ? maA : maB;
    }

    /// <summary>
    /// VAO PHONG NHANH: chon phong dang cho con cho, dong nguoi nhat truoc
    /// (de tran bat dau som). Khong co phong nao thi tu tao mot phong moi.
    ///
    /// CUOC DUA PHAI XU LY, VI NO XAY RA GAN NHU MOI LAN. Hai nguoi ban nut
    /// nay cach nhau vai tram mili giay thi CA HAI cung doc duoc mot danh sach
    /// rong - phong cua nguoi kia chua kip len - roi ca hai cung tao phong.
    /// Ket qua: hai phong mot nguoi, va vi man truoc day boc ngau nhien nen
    /// hai nguoi con vao HAI MAN KHAC NHAU. Do 3 lan qua REST that: 3/3 lan
    /// khac phong, 2/3 lan khac man.
    ///
    /// Nen tao xong phai NGO LAI mot lan: neu co phong khac cung dang cho,
    /// hai ben cung tinh <see cref="PhongDuocGiu"/> - ai co ma lon hon thi bo
    /// phong minh vua tao va sang phong kia.
    ///
    /// Va man mac dinh phai CO DINH, khong boc ngau nhien: mot cai nut ma moi
    /// lan bam ra mot man khac thi nguoi choi khong hieu chuyen gi dang xay ra.
    /// </summary>
    public static IEnumerator VaoPhongNhanh(Action<bool, string> xong)
    {
        List<Phong> ds = null;
        yield return LayDanhSach(k => ds = k);

        if (ds != null)
        {
            ds.Sort((a, b) => b.soNguoi.CompareTo(a.soNguoi));
            foreach (var p in ds)
            {
                if (!p.DangCho || !p.ConCho) continue;

                bool ok = false; string loi = null;
                yield return VaoPhong(p.ma, (o, e) => { ok = o; loi = e; });
                if (ok) { xong(true, null); yield break; }
                // Phong vua day trong luc minh doc - thu phong ke tiep
            }
        }

        bool taoDuoc = false;
        yield return TaoPhong(null, ManMacDinh, (o, e) => { taoDuoc = o; });
        if (!taoDuoc || PhongHienTai == null)
        { xong(false, "Khong tao duoc phong."); yield break; }

        yield return NhuongNeuCoPhongCoHon(xong);
    }

    /// <summary>
    /// Vua tao phong xong thi ngo lai: co ai cung vua tao phong khong.
    ///
    /// Doi mot nhip truoc khi ngo - phong cua nguoi kia mat vai tram mili giay
    /// moi hien ra trong danh sach. Ngo ngay lap tuc thi lai khong thay gi,
    /// dung nhu luc nay.
    /// </summary>
    static IEnumerator NhuongNeuCoPhongCoHon(Action<bool, string> xong)
    {
        string maToi = PhongHienTai.ma;

        yield return new WaitForSecondsRealtime(1.2f);

        List<Phong> ds2 = null;
        yield return LayDanhSach(k => ds2 = k);
        if (ds2 == null) { xong(true, null); yield break; }

        // Trong so cac phong dang cho con cho, phong nao "thang" minh
        string maGiu = maToi;
        foreach (var p in ds2)
        {
            if (p.ma == maToi || !p.DangCho || !p.ConCho) continue;
            if (p.soNguoi > 1) continue;         // phong dong nguoi thi khong phai cuoc dua
            maGiu = PhongDuocGiu(maGiu, p.ma);
        }

        if (maGiu == maToi) { xong(true, null); yield break; }

        // Minh nhuong: bo phong cua minh roi sang phong kia. Bo TRUOC khi vao,
        // khong thi de lai mot phong ma khong ai cam.
        yield return RoiPhong(null);

        bool ok = false; string loi = null;
        yield return VaoPhong(maGiu, (o, e) => { ok = o; loi = e; });
        if (ok) { xong(true, null); yield break; }

        // Phong kia vua bien mat - tao lai phong cua minh chu khong bo nguoi
        // choi dung khong o dau ca.
        yield return TaoPhong(null, ManMacDinh, xong);
    }

    public static IEnumerator RoiPhong(Action xong)
    {
        if (PhongHienTai == null) { if (xong != null) xong(); yield break; }
        string ma = PhongHienTai.ma;
        bool laHost = LaHost;
        int soNguoi = PhongHienTai.soNguoi;
        PhongHienTai = null;

        if (laHost)
        {
            // Host di thi phong tan - khong de lai mot cai phong khong ai cam
            yield return FirebaseMang.Xoa("phong/" + ma, (ok, e) => { });
            yield return FirebaseMang.Xoa("tran/" + ma, (ok, e) => { });
        }
        else
        {
            yield return FirebaseMang.Xoa("phong/" + ma + "/nguoiChoi/" + FirebaseMang.Uid,
                                          (ok, e) => { });
            yield return FirebaseMang.Ghi("phong/" + ma + "/soNguoi",
                                          Mathf.Max(0, soNguoi - 1).ToString(), (ok, e) => { });
        }
        if (xong != null) xong();
    }

    public static IEnumerator DatSanSang(bool sanSang, Action xong)
    {
        if (PhongHienTai == null) { if (xong != null) xong(); yield break; }
        yield return FirebaseMang.Ghi(
            "phong/" + PhongHienTai.ma + "/nguoiChoi/" + FirebaseMang.Uid + "/sanSang",
            sanSang ? "true" : "false", (ok, e) => { });
        if (xong != null) xong();
    }

    /// <summary>
    /// Chu phong doi man. CHI DOI DUOC KHI PHONG CON DANG CHO.
    ///
    /// Da bam bat dau roi ma con doi duoc thi may khach co the da doc man cu
    /// va nhay vao do - hai nguoi hai man. Cua so ay chi vai tram mili giay,
    /// nhung no thuc su mo.
    /// </summary>
    public static IEnumerator DoiManChoi(string manChoi, Action xong)
    {
        if (PhongHienTai == null || !LaHost || !PhongHienTai.DangCho)
        { if (xong != null) xong(); yield break; }

        yield return FirebaseMang.Ghi("phong/" + PhongHienTai.ma + "/manChoi",
                                      "\"" + manChoi + "\"", (ok, e) => { });

        // Sua luon ban sao cuc bo: nut phai doi chu ngay chu khong doi lan hoi
        // ke tiep - khong thi nguoi ta bam hai lan vi tuong nut hong.
        PhongHienTai.manChoi = manChoi;
        if (xong != null) xong();
    }

    /// <summary>
    /// Host bam bat dau. Duoc phep bat dau KE CA khi chi co mot minh.
    ///
    /// Ghi mot moc thoi gian tuong lai roi de moi may tu dem - khong ai phai
    /// gui tin tung giay, va cac may dem khop nhau du dong ho lech.
    /// </summary>
    public static IEnumerator BatDauDemNguoc(Action xong)
    {
        if (PhongHienTai == null || !LaHost) { if (xong != null) xong(); yield break; }

        double moc = GioMayChu() + GiayDemNguoc * 1000.0;
        string than = "{\"trangThai\":\"demNguoc\",\"batDauLuc\":" + (long)moc + "}";
        yield return FirebaseMang.VaGhi("phong/" + PhongHienTai.ma, than, (ok, e) => { });
        if (xong != null) xong();
    }

    public static IEnumerator DanhDauDangChoi(Action xong)
    {
        if (PhongHienTai == null || !LaHost) { if (xong != null) xong(); yield break; }
        yield return FirebaseMang.Ghi("phong/" + PhongHienTai.ma + "/trangThai",
                                      "\"dangChoi\"", (ok, e) => { });
        if (xong != null) xong();
    }

    public static IEnumerator DuoiNguoi(string uid, Action xong)
    {
        if (PhongHienTai == null || !LaHost) { if (xong != null) xong(); yield break; }
        yield return FirebaseMang.Xoa("phong/" + PhongHienTai.ma + "/nguoiChoi/" + uid,
                                      (ok, e) => { });
        yield return FirebaseMang.Ghi("phong/" + PhongHienTai.ma + "/soNguoi",
                                      Mathf.Max(0, PhongHienTai.soNguoi - 1).ToString(),
                                      (ok, e) => { });
        if (xong != null) xong();
    }

    // ================================================================
    //  DOC
    // ================================================================

    public static IEnumerator LayDanhSach(Action<List<Phong>> xong)
    {
        string json = null;
        yield return FirebaseMang.Doc("phong", s => json = s);

        var ra = new List<Phong>();
        var chet = new List<string>();

        if (!string.IsNullOrEmpty(json) && json != "null")
        {
            double bayGio = GioMayChu();
            foreach (var cap in TachCapCapCao(json))
            {
                var p = DocPhong(cap.Key, cap.Value);
                if (PhongConSong(p, bayGio)) ra.Add(p);
                else chet.Add(p.ma);
            }
        }

        // Tra danh sach cho nguoi goi TRUOC, roi moi don dep. Sanh hien ra ngay,
        // khong phai doi vai lenh xoa chay xong.
        xong(ra);

        // Phong ma: chu phong dong tab thi phong nam lai vinh vien - REST khong
        // co onDisconnect. Ai nhin thay xac thi don ho: luat cho phep XOA (chu
        // khong cho sua) mot phong da qua nguong im lang. Khong don thi co so
        // du lieu cu day len mai, va trang quan tri day nhung phong ma.
        foreach (var ma in chet)
        {
            yield return FirebaseMang.Xoa("phong/" + ma, (ok, e) => { });
            yield return FirebaseMang.Xoa("tran/" + ma, (ok, e) => { });
        }
    }

    public static IEnumerator TaiLaiPhong(string maPhong, Action<bool, string> xong)
    {
        string json = null;
        yield return FirebaseMang.Doc("phong/" + maPhong, s => json = s);

        if (string.IsNullOrEmpty(json) || json == "null")
        {
            PhongHienTai = null;
            xong(false, "Phong da dong.");
            yield break;
        }

        PhongHienTai = DocPhong(maPhong, json);
        xong(true, null);
    }

    // ================================================================
    //  BOC JSON BANG TAY
    // ================================================================
    //
    // JsonUtility khong doc noi tu dien khoa-tu-sinh (moi phong mot khoa la
    // "-P0pDN5Bvye..."), ma keo ca mot thu vien JSON vao chi de doc vai truong
    // thi qua nang cho ban WebGL. Nen boc tay - cau truc o day nong va biet
    // truoc, khong can bo phan tich day du.

    static Phong DocPhong(string ma, string json)
    {
        var p = new Phong { ma = ma };
        p.ten       = LayChuoi(json, "ten");
        p.hostUid   = LayChuoi(json, "hostUid");
        p.hostTen   = LayChuoi(json, "hostTen");
        p.manChoi   = LayChuoi(json, "manChoi");
        p.trangThai = LayChuoi(json, "trangThai");
        p.soNguoi   = LaySo(json, "soNguoi");
        p.toiDa     = LaySo(json, "toiDa");
        p.batDauLuc = LaySoThuc(json, "batDauLuc");
        p.taoLuc     = LaySoThuc(json, "taoLuc");
        p.capNhatLuc = LaySoThuc(json, "capNhatLuc");

        string khoiNguoi = LayKhoi(json, "nguoiChoi");
        if (khoiNguoi != null)
        {
            foreach (var cap in TachCapCapCao(khoiNguoi))
            {
                p.nguoiChoi.Add(new NguoiTrongPhong
                {
                    uid     = cap.Key,
                    ten     = LayChuoi(cap.Value, "ten"),
                    sanSang = LayChuoi(cap.Value, "sanSang") == "true"
                              || LayThoTho(cap.Value, "sanSang") == "true",
                    cho     = LaySo(cap.Value, "cho")
                });
            }
        }
        return p;
    }

    /// <summary>Tach mot doi tuong JSON thanh cac cap khoa - than con.</summary>
    static List<KeyValuePair<string, string>> TachCapCapCao(string json)
    {
        var ra = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrEmpty(json)) return ra;

        int i = json.IndexOf('{');
        if (i < 0) return ra;
        i++;

        while (i < json.Length)
        {
            while (i < json.Length && json[i] != '"' && json[i] != '}') i++;
            if (i >= json.Length || json[i] == '}') break;

            int dauKhoa = ++i;
            while (i < json.Length && json[i] != '"') i++;
            if (i >= json.Length) break;
            string khoa = json.Substring(dauKhoa, i - dauKhoa);
            i++;

            while (i < json.Length && json[i] != ':') i++;
            i++;
            while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
            if (i >= json.Length) break;

            int dauThan = i;
            if (json[i] == '{')
            {
                int sau = 0;
                do
                {
                    if (json[i] == '{') sau++;
                    else if (json[i] == '}') sau--;
                    else if (json[i] == '"') { i++; while (i < json.Length && json[i] != '"') { if (json[i] == '\\') i++; i++; } }
                    i++;
                } while (i < json.Length && sau > 0);
                ra.Add(new KeyValuePair<string, string>(khoa, json.Substring(dauThan, i - dauThan)));
            }
            else
            {
                while (i < json.Length && json[i] != ',' && json[i] != '}') i++;
                ra.Add(new KeyValuePair<string, string>(khoa, json.Substring(dauThan, i - dauThan)));
            }

            while (i < json.Length && (json[i] == ',' || char.IsWhiteSpace(json[i]))) i++;
        }
        return ra;
    }

    /// <summary>Lay than cua mot doi tuong con theo ten khoa.</summary>
    static string LayKhoi(string json, string khoa)
    {
        foreach (var cap in TachCapCapCao(json))
            if (cap.Key == khoa) return cap.Value;
        return null;
    }

    static string LayThoTho(string json, string khoa)
    {
        foreach (var cap in TachCapCapCao(json))
            if (cap.Key == khoa) return cap.Value.Trim();
        return null;
    }

    static string LayChuoi(string json, string khoa)
    {
        string v = LayThoTho(json, khoa);
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Length >= 2 && v[0] == '"' && v[v.Length - 1] == '"')
            return v.Substring(1, v.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
        return v;
    }

    static int LaySo(string json, string khoa)
    {
        int n;
        return int.TryParse(LayThoTho(json, khoa), out n) ? n : 0;
    }

    static double LaySoThuc(string json, string khoa)
    {
        double n;
        return double.TryParse(LayThoTho(json, khoa),
                               System.Globalization.NumberStyles.Any,
                               System.Globalization.CultureInfo.InvariantCulture, out n) ? n : 0;
    }
}
