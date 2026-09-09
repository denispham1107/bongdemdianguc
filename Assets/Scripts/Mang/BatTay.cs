using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HAI MAY TIM THAY NHAU QUA FIREBASE, ROI NOI THANG VOI NHAU.
///
/// WebRTC khong tu tim duoc may kia: no can mot kenh co san de hai ben trao
/// doi "toi o dia chi nay, toi noi duoc kieu nay". Realtime Database dang mo
/// san cho sanh phong nen dung luon - khong phai dung them gi.
///
/// Toan bo viec o day chi dien ra MOT LAN, luc vao tran, mat chung 1-2 giay.
/// Xong roi Firebase dung sang mot ben: trong suot tran dau khong con goi tin
/// nao di qua Google nua, va neu Firebase sap giua tran thi tran van chay.
///
/// Ai moi ai: CHU PHONG moi, nguoi khac vao. Khong phai thoa thuan gi cho phuc
/// tap - phong von da co chu roi.
///
/// Duong di cua mot lan bat tay:
///   chu phong                        nguoi vao
///   -----------------------------    -----------------------------
///   tao ket noi, sinh "loi moi"
///   ghi loi moi len tran/{ma}/       doc loi moi
///                                    tao ket noi, sinh "tra loi"
///   doc tra loi                      ghi tra loi len tran/{ma}/
///   ..... hai ben lien tuc day ung vien ICE cua minh len va doc cua ben kia
///   ..... den khi kenh mo
/// </summary>
public static class BatTay
{
    /// <summary>Bao lau thi bo cuoc. Bat tay binh thuong mat 1-2 giay; qua
    /// nguong nay gan nhu chac chan la NAT hai ben chan nhau.</summary>
    public const float GiayHetHan = 20f;

    /// <summary>Nhip doc cho hen gap. Khong can nhanh: chi vai lan doc la
    /// xong, va doc day thi ton tien Firebase chu khong nhanh hon.</summary>
    const float NhipDoc = 0.4f;

    public static string LoiCuoi { get; private set; }

    /// <summary>Duong tren Realtime Database cho mot lan bat tay.</summary>
    static string Duong(string maPhong, string duoi)
    {
        return "tran/" + maPhong + "/battay/" + duoi;
    }

    /// <summary>
    /// Chu phong: moi nguoi kia noi vao.
    /// <paramref name="uidKia"/> la nguoi duoc moi - moi cap mot cho rieng,
    /// khong thi bon nguoi trong phong dam vao nhau.
    /// </summary>
    public static IEnumerator ChuPhongMoi(string maPhong, string uidKia,
                                          System.Action<bool, string> xong)
    {
        LoiCuoi = null;
        string cho = uidKia + "_" + FirebaseMang.Uid;

        KenhTrucTiep.Tao();
        if (!KenhTrucTiep.DaTao) { Ket(xong, false, "khong tao duoc ket noi"); yield break; }

        // Don cho hen gap cua lan truoc. Khong don thi lan vao lai vap phai
        // loi moi cu va bat tay voi mot phien da chet.
        yield return FirebaseMang.Xoa(Duong(maPhong, cho), (o, e) => { });

        KenhTrucTiep.TaoLoiMoi();

        // Doi trinh duyet sinh xong loi moi
        string moTa = "";
        float hanMoTa = Time.realtimeSinceStartup + 5f;
        while (moTa.Length == 0 && Time.realtimeSinceStartup < hanMoTa)
        {
            moTa = KenhTrucTiep.LayMoTa();
            if (moTa.Length == 0) yield return null;
        }
        if (moTa.Length == 0) { Ket(xong, false, "khong sinh duoc loi moi"); yield break; }

        yield return FirebaseMang.Ghi(Duong(maPhong, cho + "/moi"), moTa, (o, e) => { });

        // Vua day ung vien cua minh len, vua ngong cau tra loi
        bool daNhanTraLoi = false;
        yield return VongBatTay(maPhong, cho, "uv1", "uv2", () =>
        {
            if (daNhanTraLoi) return null;
            return Duong(maPhong, cho + "/tra");
        }, (json) => { KenhTrucTiep.NhanTraLoi(json); daNhanTraLoi = true; }, xong);
    }

    /// <summary>Nguoi vao: nhan loi moi cua chu phong roi tra loi.</summary>
    public static IEnumerator NguoiVaoNhan(string maPhong, string uidChuPhong,
                                           System.Action<bool, string> xong)
    {
        LoiCuoi = null;
        string cho = FirebaseMang.Uid + "_" + uidChuPhong;

        KenhTrucTiep.Tao();
        if (!KenhTrucTiep.DaTao) { Ket(xong, false, "khong tao duoc ket noi"); yield break; }

        // Cho chu phong dat loi moi len
        string moi = "";
        float han = Time.realtimeSinceStartup + GiayHetHan;
        while (moi.Length == 0 && Time.realtimeSinceStartup < han)
        {
            yield return FirebaseMang.Doc(Duong(maPhong, cho + "/moi"), s => moi = Sach(s));
            if (moi.Length == 0) yield return new WaitForSecondsRealtime(NhipDoc);
        }
        if (moi.Length == 0) { Ket(xong, false, "cho mai khong thay loi moi"); yield break; }

        KenhTrucTiep.TraLoi(moi);

        string moTa = "";
        float hanMoTa = Time.realtimeSinceStartup + 5f;
        while (moTa.Length == 0 && Time.realtimeSinceStartup < hanMoTa)
        {
            moTa = KenhTrucTiep.LayMoTa();
            if (moTa.Length == 0) yield return null;
        }
        if (moTa.Length == 0) { Ket(xong, false, "khong sinh duoc cau tra loi"); yield break; }

        yield return FirebaseMang.Ghi(Duong(maPhong, cho + "/tra"), moTa, (o, e) => { });

        yield return VongBatTay(maPhong, cho, "uv2", "uv1",
                                () => null, null, xong);
    }

    /// <summary>
    /// Vong chung cua hai ben: day ung vien cua minh len, doc ung vien ben kia,
    /// va doi kenh mo.
    ///
    /// <paramref name="duongCho"/> tra ve duong cua thu con phai doi (cau tra
    /// loi), hoac null neu khong doi gi nua.
    /// </summary>
    static IEnumerator VongBatTay(string maPhong, string cho,
                                  string nhanhToi, string nhanhKia,
                                  System.Func<string> duongCho,
                                  System.Action<string> khiCo,
                                  System.Action<bool, string> xong)
    {
        var daThemUngVien = new HashSet<string>();
        float han = Time.realtimeSinceStartup + GiayHetHan;

        while (Time.realtimeSinceStartup < han)
        {
            if (KenhTrucTiep.DaMo) { Ket(xong, true, null); yield break; }

            string loi = KenhTrucTiep.LayLoi();
            if (!string.IsNullOrEmpty(loi)) { Ket(xong, false, loi); yield break; }

            // Day ung vien cua minh len - moi lan mot cai, het thi thoi
            string uv;
            while ((uv = KenhTrucTiep.LayUngVien()).Length > 0)
                yield return FirebaseMang.Them(Duong(maPhong, cho + "/" + nhanhToi), uv,
                                               (k, e) => { });

            // Doc ung vien cua ben kia
            string json = null;
            yield return FirebaseMang.Doc(Duong(maPhong, cho + "/" + nhanhKia), s => json = s);
            foreach (var cap in TachUngVien(json))
            {
                if (daThemUngVien.Contains(cap.Key)) continue;
                daThemUngVien.Add(cap.Key);
                KenhTrucTiep.ThemUngVien(cap.Value);
            }

            // Con doi cau tra loi thi doc luon
            string d = duongCho != null ? duongCho() : null;
            if (d != null)
            {
                string tra = null;
                yield return FirebaseMang.Doc(d, s => tra = Sach(s));
                if (!string.IsNullOrEmpty(tra) && khiCo != null) khiCo(tra);
            }

            yield return new WaitForSecondsRealtime(NhipDoc);
        }

        Ket(xong, false, "het " + GiayHetHan + " giay ma khong noi duoc");
    }

    /// <summary>
    /// Tach mot khoi JSON gom nhieu ung vien do Firebase tu dat khoa.
    ///
    /// Firebase sinh khoa kieu "-P0abc..." nen khong khai truoc thanh truong
    /// duoc, JsonUtility chiu - phai tu tach nhu ben PhongMang.
    /// </summary>
    static IEnumerable<KeyValuePair<string, string>> TachUngVien(string json)
    {
        var ra = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrEmpty(json) || json == "null") return ra;

        int i = 0;
        while (i < json.Length)
        {
            int mo = json.IndexOf('"', i);
            if (mo < 0) break;
            int dong = json.IndexOf('"', mo + 1);
            if (dong < 0) break;
            string khoa = json.Substring(mo + 1, dong - mo - 1);

            int haiCham = json.IndexOf(':', dong);
            if (haiCham < 0) break;

            int batDau = json.IndexOf('{', haiCham);
            if (batDau < 0) break;

            int sau = 0, j = batDau;
            for (; j < json.Length; j++)
            {
                if (json[j] == '{') sau++;
                else if (json[j] == '}') { sau--; if (sau == 0) { j++; break; } }
            }

            ra.Add(new KeyValuePair<string, string>(khoa, json.Substring(batDau, j - batDau)));
            i = j;
        }
        return ra;
    }

    static string Sach(string s)
    {
        if (string.IsNullOrEmpty(s) || s == "null") return "";

        // Firebase tra chuoi ve trong dau nhay kep va co escape - go ra de
        // duoc dung JSON ma trinh duyet doi.
        s = s.Trim();
        if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
            s = s.Substring(1, s.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
        return s;
    }

    static void Ket(System.Action<bool, string> xong, bool ok, string loi)
    {
        LoiCuoi = loi;
        if (!ok) Debug.LogWarning("[BatTay] " + loi);
        if (xong != null) xong(ok, loi);
    }
}
