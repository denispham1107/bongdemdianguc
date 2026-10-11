using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MENU 124 - O KY NANG LUU THEO TAI KHOAN + MAC DINH CAM UNG MOI (11/10/2026, nguoi dung).
///
/// Chay NGOAI Play (SachPhep doc / ghi kho luu cua may - PlayerPrefs trong Editor), uid GIA qua FirebaseMang.DatUidChoPhepThu, ho so
/// gia qua HoSoMang.CuaToi / UidDaTai. Cat moi khoa dung toi roi tra lai.
///   1. Mac dinh cam ung moi = [Cau lua, Mua bang, Sam set, Binh mana, Binh mau, Toc bien, Giat set, Nhao lon]; may tinh giu cu.
///   2. Tai khoan MOI (0 tran) tren may DA CO cach xep cu -> bo mac dinh moi, ghi ngay vao khoa rieng.
///   3. Tai khoan CU (co tran) chua co khoa rieng -> dung cach xep cu cua may.
///   4. Ho so chua tai -> tam mac dinh, KHONG ghi; tai xong -> quyet dinh dung.
///   5. Doi qua lai hai tai khoan: moi tai khoan giu cach xep rieng; sua o tai khoan nay khong dong toi tai khoan kia.
///   6. Tai khoan moi choi xong tran dau (soTranChoi 0 -> 1) van giu bo cua minh (khong bi "chuyen" sang cach xep cua may).
///   7. Chua dang nhap -> khoa chung cua may (nhu cu).
/// Ket qua: PlayTestShots/okynangtaikhoan.txt.
/// </summary>
public static class ThuOKyNangTaiKhoan
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[OTaiKhoan] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }
    static string Ke(int[] a) { return string.Join(",", a); }

    [MenuItem("Diablo 2.5D/124. Chay thu O KY NANG theo tai khoan (mac dinh cam ung moi)", false, 213)]
    public static void Chay()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[OTaiKhoan] thoat Play roi chay"); return; }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0;
        Ghi("[ban 1] o ky nang theo tai khoan");

        string kT = SachPhep.KhoaTronGoc, kV = SachPhep.KhoaVuongGoc;
        string[] uids = { "thu_cu_124", "thu_moi_124", "thu_moi2_124" };
        var khoa = new List<string> { kT, kV };
        foreach (var u in uids) { khoa.Add(SachPhep.KhoaCua(kT, u)); khoa.Add(SachPhep.KhoaCua(kV, u)); }
        var cat = new Dictionary<string, string>();
        foreach (var k in khoa) if (PlayerPrefs.HasKey(k)) cat[k] = PlayerPrefs.GetString(k);
        string uidCu = FirebaseMang.Uid, uidHoSoCu = HoSoMang.UidDaTai;
        int tranCu = HoSoMang.CuaToi.soTranChoi;
        bool epCu = CamUng.EpBat;
        try
        {
            foreach (var k in khoa) PlayerPrefs.DeleteKey(k);
            string mdTron = "0,1,2," + CapDo.KyBinhMana + "," + CapDo.KyBinhMau + "," + CapDo.KyTocBien + ",6," + CapDo.KyNhaoLon;
            string mdVuong = "0,1,2,3,4,5,6," + CapDo.KyNhaoLon;

            // 7. chua dang nhap
            FirebaseMang.DatUidChoPhepThu(null); SachPhep.NapLai();
            Ghi("1/7. chua dang nhap, may chua tung xep: tron [" + Ke(SachPhep.OTron) + "] (mong " + mdTron + "), vuong [" + Ke(SachPhep.OVuong) + "] (mong " + mdVuong + ")");
            Kiem(Ke(SachPhep.OTron) == mdTron && Ke(SachPhep.OVuong) == mdVuong, "mac dinh sai");

            // May da co cach xep cu (khoa chung, nhu nguoi choi truoc 11/10/2026)
            const string cuTron = "6,5,4,3,2,1,0,22", cuVuong = "6,5,4,3,2,1,0,22";
            PlayerPrefs.SetString(kT, cuTron); PlayerPrefs.SetString(kV, cuVuong);
            SachPhep.NapLai();
            Ghi("7. chua dang nhap, may co cach xep cu: tron [" + Ke(SachPhep.OTron) + "]");
            Kiem(Ke(SachPhep.OTron) == cuTron, "chua dang nhap ma khong dung khoa chung cua may");

            // 4. ho so chua tai
            FirebaseMang.DatUidChoPhepThu(uids[1]); HoSoMang.UidDaTai = null; HoSoMang.CuaToi.soTranChoi = 0;
            SachPhep.NapLai();
            bool ghiSom = PlayerPrefs.HasKey(SachPhep.KhoaCua(kT, uids[1]));
            Ghi("4. tai khoan moi, ho so CHUA tai: tron [" + Ke(SachPhep.OTron) + "], da ghi khoa rieng " + ghiSom);
            Kiem(Ke(SachPhep.OTron) == mdTron && !ghiSom, "ho so chua tai ma da quyet dinh / ghi");

            // 2. tai khoan moi (0 tran) - ho so tai xong (Nap tu doc lai, khong goi NapLai)
            HoSoMang.UidDaTai = uids[1];
            string t2 = Ke(SachPhep.OTron), v2 = Ke(SachPhep.OVuong);
            string ghi2 = PlayerPrefs.GetString(SachPhep.KhoaCua(kT, uids[1]), "");
            Ghi("2. tai khoan MOI (0 tran), may da co cach xep cu: tron [" + t2 + "], vuong [" + v2 + "], khoa rieng [" + ghi2 + "]");
            Kiem(t2 == mdTron && v2 == mdVuong && ghi2 == mdTron, "tai khoan moi khong bat dau bang bo mac dinh moi");

            // 3. tai khoan cu (co tran) chua co khoa rieng -> cach xep cu cua may
            FirebaseMang.DatUidChoPhepThu(uids[0]); HoSoMang.UidDaTai = uids[0]; HoSoMang.CuaToi.soTranChoi = 64;
            string t3 = Ke(SachPhep.OTron), v3 = Ke(SachPhep.OVuong);
            Ghi("3. tai khoan CU (64 tran): tron [" + t3 + "], vuong [" + v3 + "] (mong cach xep cu cua may)");
            Kiem(t3 == cuTron && v3 == cuVuong, "tai khoan cu mat cach xep dang dung");

            // 5. sua o tai khoan cu, doi sang tai khoan moi: khong bi dong
            CamUng.EpBat = true;
            SachPhep.DatVaoO(0, CapDo.KyTocBien);
            string t5cu = Ke(SachPhep.OTron);
            FirebaseMang.DatUidChoPhepThu(uids[1]); HoSoMang.UidDaTai = uids[1]; HoSoMang.CuaToi.soTranChoi = 0;
            string t5moi = Ke(SachPhep.OTron);
            FirebaseMang.DatUidChoPhepThu(uids[0]); HoSoMang.UidDaTai = uids[0]; HoSoMang.CuaToi.soTranChoi = 64;
            string t5cuLai = Ke(SachPhep.OTron);
            Ghi("5. sua tai khoan cu -> [" + t5cu + "]; doi sang tai khoan moi -> [" + t5moi + "]; quay lai tai khoan cu -> [" + t5cuLai + "]");
            Kiem(t5moi == mdTron && t5cuLai == t5cu && t5cu != cuTron, "cach xep cua hai tai khoan dong vao nhau");

            // 6. tai khoan moi choi xong tran dau
            FirebaseMang.DatUidChoPhepThu(uids[1]); HoSoMang.UidDaTai = uids[1]; HoSoMang.CuaToi.soTranChoi = 1;
            SachPhep.NapLai();
            Ghi("6. tai khoan moi sau tran dau (1 tran): tron [" + Ke(SachPhep.OTron) + "]");
            Kiem(Ke(SachPhep.OTron) == mdTron, "tai khoan moi choi xong tran dau bi chuyen sang cach xep cua may");

            // 2b. tai khoan moi thu hai tren cung may
            FirebaseMang.DatUidChoPhepThu(uids[2]); HoSoMang.UidDaTai = uids[2]; HoSoMang.CuaToi.soTranChoi = 0;
            Ghi("2b. tai khoan moi thu hai: tron [" + Ke(SachPhep.OTron) + "]");
            Kiem(Ke(SachPhep.OTron) == mdTron, "tai khoan moi thu hai khong nhan bo mac dinh moi");
        }
        finally
        {
            foreach (var k in khoa) PlayerPrefs.DeleteKey(k);
            foreach (var kv in cat) PlayerPrefs.SetString(kv.Key, kv.Value);
            PlayerPrefs.Save();
            FirebaseMang.DatUidChoPhepThu(uidCu);
            HoSoMang.UidDaTai = uidHoSoCu;
            HoSoMang.CuaToi.soTranChoi = tranCu;
            CamUng.EpBat = epCu;
            SachPhep.NapLai();
        }
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/okynangtaikhoan.txt", bao.ToString());
    }
}
