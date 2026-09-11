using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: KY NANG PHAI DI QUA DUOC MANG.
///
/// Hien tuong nguoi choi bao: hai nguoi thay nhau chay, nhung mot ben tung
/// phep thi ben kia khong thay gi, khong mat mau, khong dinh hieu ung nao -
/// ke ca cai khieng bat len cung khong ai thay.
///
/// Nguyen nhan: goi tin chi cho VI TRI, GOC va MAU. Khong co lay mot bit nao
/// noi rang "toi vua tung phep". Va mau tuy co gui nhung ben nhan chua bao
/// gio doc den.
///
/// Phep thu di dung duong that: nhet goi vao hang nhan cua kenh (GiaLapNhan)
/// roi de chinh DongBoTran doc ra - khong goi tat vao ham ben trong, vi goi
/// tat thi khong kiem duoc phan phan loai goi, cho de hong nhat.
///
/// Ket qua ghi ra <c>PlayTestShots/kynang_mang.txt</c>.
/// </summary>
public static class ThuKyNangQuaMang
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/37. Chay thu KY NANG qua mang", false, 124)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != Canh)
            EditorSceneManager.OpenScene(Canh);

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_KyNangMang");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[KyNangMang] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] ky nang phai di qua duoc mang");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        TranHienTai.DangChoiMang = true;

        // DON SACH QUAI TRUOC KHI DO.
        //
        // Lan chay dau bao hai loi, va ca hai deu la loi gia: Act2 co 33 con
        // quai, chung van danh nguoi choi trong luc phep thu dang cho. Mau tut
        // vi bi quai can thi khong noi len duoc dieu gi ve mang ca. Phai tat
        // han nguon nhieu ay roi moi do.
        // PHAI DOI GameDirector RAI XONG DA. Don ngay khi vua tim thay nhan
        // vat thi con hai dong quai rieng chua kip sinh - chung ra doi sau do
        // va dung chan giua duong. Do duoc: qua cau lua dung lai o 4,33 m, tuc
        // chi bay duoc 0,7 m khoi tay nguoi tung, va phep thu bao "phep khong
        // gay sat thuong" trong khi no chi no vao mot con quai.
        yield return new WaitForSeconds(2f);

        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;

        int soQuaiDon = DonSachQuai();
        Ghi("da don " + soQuaiDon + " con quai de mau chi doi vi ky nang");

        yield return new WaitForSeconds(1f);
        int sotLai = DonSachQuai();
        if (sotLai > 0) Ghi("don them " + sotLai + " con sinh sau");

        // ---- 1. Nhan vat phai nam o lop Player ----
        // Khong nam dung lop thi phep cua nguoi kia bay xuyen qua nguoi minh
        // ma khong ai bao gi ca. Menu 34 truoc day phai dat lop bang tay cho
        // con bia - nen phai hoi thang xem nhan vat that thi sao.
        int lopPlayer = LayerMask.NameToLayer("Player");
        Ghi("1. lop cua nhan vat minh: " + LayerMask.LayerToName(toi.gameObject.layer)
            + " (can la Player, so hieu " + lopPlayer + ")");
        if (toi.gameObject.layer != lopPlayer)
        {
            Ghi("[LOI] nhan vat khong nam o lop Player - phep cua nguoi kia se khong trung");
            loi++;
        }

        // ---- 2. Goi ky nang: viet ra roi doc lai phai khop ----
        var goc = new GoiTin.MotPhep
        {
            chiSo = 1, kyNang = 4, soThuTu = 77,
            diemNgam = new Vector3(12.34f, 1.5f, -8.76f)
        };
        GoiTin.MotPhep doc;
        byte[] byteGoi = GoiTin.VietKyNang(goc);
        bool docDuoc = GoiTin.DocKyNang(byteGoi, out doc);
        float lech = docDuoc ? Vector3.Distance(goc.diemNgam, doc.diemNgam) : 999f;

        Ghi("2. goi ky nang " + byteGoi.Length + " byte, doc lai: kyNang=" + doc.kyNang
            + " chiSo=" + doc.chiSo + " soThuTu=" + doc.soThuTu
            + " lech diem ngam=" + lech.ToString("F4") + " m (duoi 0,02 m)");
        if (!docDuoc || doc.kyNang != goc.kyNang || doc.chiSo != goc.chiSo
            || doc.soThuTu != goc.soThuTu || lech > 0.02f)
        { Ghi("[LOI] goi ky nang doc ra khong khop luc viet"); loi++; }

        // ---- Dung mot nguoi choi thu hai lam nguoi tung phep ----
        Vector3 choKia = toi.transform.position + new Vector3(5f, 0f, 0f);
        var kia = NguoiChoiKhac.Sinh("uid-kia", "Nguoi kia", choKia);
        if (kia == null) { Ghi("[LOI] khong sinh duoc nguoi choi kia"); loi++; Ket(); yield break; }

        var mauToi = toi.GetComponent<Damageable>();
        var mauKia = kia.GetComponent<Damageable>();

        // Chieu 4 hoi "nguoi tung co tu thieu minh khong". De nguyen co ban sao
        // thi ho KHONG THE mat mau cuc bo, chieu 4 luon dat du boQua co hong -
        // mot phep do khong the sai thi khong do gi ca. Bat lai viec tru mau.
        if (mauKia != null) mauKia.mauDoMayKhacQuyet = false;
        if (mauToi == null || mauKia == null)
        { Ghi("[LOI] thieu Damageable"); loi++; Ket(); yield break; }

        // Bo dong bo that len, dung nhu trong tran
        var goDb = new GameObject("TAM_DongBo");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi;
        db.chiSoCuaToi = 0;
        db.GanTaiNghe();
        db.ThemNguoi(1, kia);

        // NHIP GIU SONG, giong mot nguoi that o dau ben kia.
        //
        // Bo dong bo gio coi 10 giay im lang la "nguoi kia da roi tran" va thoi
        // doc kenh ay. Trong tran that khong bao gio im lang the (60 goi moi
        // giay), nhung phep thu nay co nhung quang cho dai - doi mau ngung tut,
        // doi thien thach roi - ma khong nhet goi nao. Khong giu song thi tu
        // chieu 5 tro di moi goi nhet vao deu roi vao mot kenh khong ai nghe.
        var chay = Object.FindAnyObjectByType<ChayThuMang>();
        if (chay != null) chay.StartCoroutine(GiuSong());

        yield return new WaitForSeconds(0.5f);
        DonSachQuai();      // lan cuoi, ngay truoc khi do

        // ---- 3. Nguoi kia tung phep vao cho minh -> MINH MAT MAU ----
        float toiTruoc = mauToi.health;
        float kiaTruoc = mauKia.health;

        NhetGoiPhep(1, 0, 1, toi.transform.position);   // 0 = qua cau lua

        // Bam theo tung buoc mot, de neu hong thi biet hong o dau chu khong
        // chi biet "mau khong tut". Ba cau hoi: co nhan duoc goi khong, co vao
        // trang thai niem chu khong, va co qua cau nao thuc su bay ra khong.
        int nhanTruoc = db.SoPhepDaNhan;
        bool daNiem = false;
        int quaCauToiDa = 0;

        float ganNhat = 999f;
        Vector3 choGanNhat = Vector3.zero;

        float doiDen = Time.time + 6f;
        while (Time.time < doiDen && mauToi.health >= toiTruoc)
        {
            if (kia.DangNiemChu) daNiem = true;
            var ds = Object.FindObjectsByType<Fireball>(FindObjectsSortMode.None);
            if (ds.Length > quaCauToiDa) quaCauToiDa = ds.Length;

            // Qua cau bay gan minh nhat den muc nao - neu no di sat nguoi ma
            // van khong no thi loi nam o mat na va cham, con neu no bay xa tit
            // thi loi nam o huong ngam.
            foreach (var q in ds)
            {
                float d = Vector3.Distance(q.transform.position,
                                           toi.transform.position + Vector3.up * 0.9f);
                if (d < ganNhat) { ganNhat = d; choGanNhat = q.transform.position; }
            }
            yield return null;
        }

        Ghi("   qua cau bay gan minh nhat: " + ganNhat.ToString("F2") + " m (tai "
            + choGanNhat.ToString("F1") + "), minh o " + toi.transform.position.ToString("F1"));
        Ghi("   nguoi tung o " + kia.transform.position.ToString("F1")
            + ", qua cau dung cach ho "
            + Vector3.Distance(choGanNhat, kia.transform.position).ToString("F2") + " m");

        // Con gi khac dang dung giua duong khong - mot Damageable la qua cau no
        int soChan = 0;
        foreach (var d in Object.FindObjectsByType<Damageable>(FindObjectsSortMode.None))
        {
            if (d == null || d == mauToi || d == kia.GetComponent<Damageable>()) continue;
            Vector3 giua = (toi.transform.position + kia.transform.position) * 0.5f;
            if (Vector3.Distance(d.transform.position, giua) < 4f) soChan++;
        }
        Ghi("   so Damageable la dung gan duong bay: " + soChan);
        Ghi("   mat na vat can cua ban sao co lop Player = "
            + ((kia.MatNaVatCan & (1 << lopPlayer)) != 0)
            + " | collider tren nhan vat minh: "
            + toi.GetComponents<Collider>().Length + " roi + CharacterController="
            + (toi.GetComponent<CharacterController>() != null));

        Ghi("   chan doan: goi phep nhan duoc = " + (db.SoPhepDaNhan - nhanTruoc)
            + " | ban sao co vao the niem chu = " + daNiem
            + " | so qua cau lua thay tren canh = " + quaCauToiDa
            + " | mask cua ban sao co lop Player = "
            + ((kia.MatNaKeThu & (1 << lopPlayer)) != 0));

        float toiMat = toiTruoc - mauToi.health;
        Ghi("3. nguoi kia tung cau lua vao cho minh -> minh mat " + toiMat.ToString("F0")
            + " mau (" + toiTruoc.ToString("F0") + " -> " + mauToi.health.ToString("F0")
            + "), phai lon hon 0");
        if (toiMat <= 0f)
        { Ghi("[LOI] phep cua nguoi kia khong gay sat thuong - dung hien tuong nguoi choi bao"); loi++; }

        // ---- 3b. CHIA DOI BAI TOAN ----
        //
        // Qua cau lua co bay ra (dem duoc 3 qua) ma minh khong mat mau. Hai
        // kha nang: sat thuong khong an vao nguoi choi, hay qua cau bay truot.
        // Hoi tung cai mot.
        float truoc3b = mauToi.health;
        CombatUtil.AreaDamage(toi.transform.position + Vector3.up * 0.5f, 3.5f, 40f,
                              kia.MatNaKeThu, DamageType.Fire, 0f,
                              kia.GetComponent<Damageable>());
        yield return new WaitForSeconds(0.3f);
        float mat3b = truoc3b - mauToi.health;
        Ghi("3b. no thang mot cu ngay tren dau minh voi mask cua nguoi kia -> minh mat "
            + mat3b.ToString("F0") + " mau (neu > 0 thi sat thuong an duoc, "
            + "loi nam o duong bay cua qua cau)");

        // ---- 3c. Phep ROI DUNG CHO thay vi phep bay ----
        // Thien thach roi thang xuong diem ngam, khong phai bay ngang - neu
        // cai nay trung ma qua cau lua thi khong, van de la duong bay.
        float truoc3c = mauToi.health;
        kia.TungPhepTheoMang(4, toi.transform.position);
        float doi3c = Time.time + 8f;
        while (Time.time < doi3c && mauToi.health >= truoc3c) yield return null;
        float mat3c = truoc3c - mauToi.health;
        Ghi("3c. nguoi kia tung THIEN THACH vao cho minh -> minh mat "
            + mat3c.ToString("F0") + " mau");

        // ---- 4. Nguoi tung khong tu thieu ----
        float kiaMat = kiaTruoc - mauKia.health;
        Ghi("4. nguoi tung phep mat " + kiaMat.ToString("F0") + " mau (phai la 0)");
        if (kiaMat > 0f) { Ghi("[LOI] nguoi tung tu thieu chinh minh"); loi++; }

        // ---- 5. Goi gui lai ba lan chi duoc no MOT lan ----
        //
        // CHO DEN KHI MAU NGUNG TUT, khong cho mot so giay tu doan.
        //
        // Truoc do toi viet "cho 6 giay cho DOT tan" - va no van sai: vung lua
        // cua thien thach o phep 3c chay them mot lat nua, the la phep thu bao
        // "mot cu bam ra nhieu lan sat thuong" trong khi goi lap da bi bo dung.
        // Doi khi mau dung yen mot giay tron thi moi chac khong con gi dang chay.
        float mauCu = -1f;
        float yenTu = Time.time;
        float hanCho = Time.time + 20f;
        while (Time.time < hanCho)
        {
            if (Mathf.Abs(mauToi.health - mauCu) > 0.01f)
            {
                mauCu = mauToi.health;
                yenTu = Time.time;
            }
            else if (Time.time - yenTu >= 1.0f) break;
            yield return null;
        }

        float truocLap = mauToi.health;
        int boTruoc = db.SoPhepBoVITrung;

        NhetGoiPhep(1, 0, 1, toi.transform.position);   // DUNG so thu tu cu
        NhetGoiPhep(1, 0, 1, toi.transform.position);
        yield return new WaitForSeconds(3f);

        float matThem = truocLap - mauToi.health;
        Ghi("5. gui lai dung goi ay hai lan nua -> mat them " + matThem.ToString("F0")
            + " mau (phai la 0), so goi bo vi trung tang "
            + (db.SoPhepBoVITrung - boTruoc) + " (phai la 2)");
        if (matThem > 0f) { Ghi("[LOI] mot cu bam ra nhieu lan sat thuong"); loi++; }
        if (db.SoPhepBoVITrung - boTruoc != 2)
        { Ghi("[LOI] khong bo dung so ban sao"); loi++; }

        // ---- 6. Mau cua nguoi kia lay tu goi tin, khong tu tinh ----
        NhetGoiTrangThai(1, 0.5f, false, kia.transform.position, db);
        yield return null;      // dung mot khung hinh de DongBoTran doc goi
        yield return null;

        float ti = mauKia.maxHealth > 0f ? mauKia.health / mauKia.maxHealth : -1f;
        Ghi("6. may kia bao con 50% mau -> ban sao ben nay: "
            + (ti * 100f).ToString("F0") + "% (phai khoang 50%)");
        if (Mathf.Abs(ti - 0.5f) > 0.02f)
        { Ghi("[LOI] mau nhan tu mang khong duoc ap len nhan vat nguoi kia"); loi++; }

        // ---- 7. Chieu nguoc: minh tung phep thi CO goi di ra ----
        // Thieu chieu nay thi chi chung minh duoc mot nua: nhan duoc ma khong
        // gui duoc thi nguoi kia van khong thay gi.
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");            // mo kenh gia lap
        var daGui = new List<string>();
        KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);

        int phepGuiTruoc = db.SoPhepDaGui;
        toi.CastAt(2, toi.transform.position + new Vector3(0f, 0f, 6f));   // sam set
        yield return new WaitForSeconds(0.5f);

        int soGoiPhep = 0;
        foreach (var t in daGui)
        {
            byte[] b = GoiTin.TuChuoi(t);
            if (GoiTin.LoaiCuaGoi(b) == GoiTin.LoaiKyNang) soGoiPhep++;
        }

        Ghi("7. minh tung sam set -> so goi ky nang gui di: " + soGoiPhep
            + " (phai it nhat 1), SoPhepDaGui tang " + (db.SoPhepDaGui - phepGuiTruoc));
        if (soGoiPhep < 1 || db.SoPhepDaGui <= phepGuiTruoc)
        { Ghi("[LOI] tung phep ma khong co goi nao di ra - nguoi kia se khong thay gi"); loi++; }

        KenhTrucTiep.guiSangBenKia = null;
        // KHONG dong kenh o day: bo dong bo gio coi mot kenh bi dong la "nguoi
        // o dau ben kia da roi tran" va thoi doc no - chieu 8 ben duoi se nhet
        // goi vao mot kenh khong ai nghe. Dong o phan don cuoi.

        // ---- 8. KHIENG cua nguoi kia phai hien ra ben nay ----
        // Nguoi choi bao rieng cai nay: mot ben bat khieng, ben kia khong thay
        // gi. Khieng khong gay sat thuong nen khong the do bang mau - phai dem
        // xem cai vom co that su moc ra tren canh khong.
        // Tra co ban sao ve DUNG nhu trong tran that: khieng cua ban sao gio chi
        // den tu goi trang thai (HieuUngQuaMang.ApKhieng), phat lai phep Khieng
        // chi con dong tac. Nen phai gui CA goi phep lan goi trang thai co
        // khieng - dung nhu may cua nguoi kia gui.
        mauKia.mauDoMayKhacQuyet = true;
        int khiengTruoc = Object.FindObjectsByType<Khieng>(FindObjectsSortMode.None).Length;
        NhetGoiPhep(1, 5, 9, kia.transform.position);   // 5 = khieng
        {
            var dsK = new GoiTin.MotNguoi[1];
            dsK[0] = new GoiTin.MotNguoi
            {
                chiSo = 1, viTri = kia.transform.position, gocY = 0f,
                mau01 = 0.5f, khieng01 = 1f
            };
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), dsK, 1)));
        }

        int khiengToiDa = khiengTruoc;
        float doiKhieng = Time.time + 6f;
        while (Time.time < doiKhieng)
        {
            int n = Object.FindObjectsByType<Khieng>(FindObjectsSortMode.None).Length;
            if (n > khiengToiDa) khiengToiDa = n;
            if (khiengToiDa > khiengTruoc) break;
            yield return null;
        }

        Ghi("8. nguoi kia bat khieng -> so khieng tren canh: " + khiengTruoc
            + " -> " + khiengToiDa + " (phai tang)");
        if (khiengToiDa <= khiengTruoc)
        { Ghi("[LOI] khieng cua nguoi kia khong hien ra ben nay"); loi++; }

        // ---- Don ----
        KenhTrucTiep.Dong();
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(kia);
        TranHienTai.DangChoiMang = false;

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    /// <summary>Xoa moi con quai trong canh. Tra ve so con da xoa.</summary>
    static int DonSachQuai()
    {
        int n = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include,
                                                           FindObjectsSortMode.None))
        { Object.DestroyImmediate(q.gameObject); n++; }
        return n;
    }

    /// <summary>Nhet mot goi ky nang vao hang nhan, y nhu vua den tu may kia.</summary>
    static IEnumerator GiuSong()
    {
        while (EditorApplication.isPlaying && GameObject.Find("TAM_DongBo") != null)
        {
            KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietNhip(true, 1)));
            yield return new WaitForSeconds(0.5f);
        }
    }

    static void NhetGoiPhep(byte chiSo, byte kyNang, int soThuTu, Vector3 ngam)
    {
        byte[] b = GoiTin.VietKyNang(new GoiTin.MotPhep
        {
            chiSo = chiSo, kyNang = kyNang, soThuTu = soThuTu, diemNgam = ngam
        });
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(b));
    }

    static void NhetGoiTrangThai(byte chiSo, float mau01, bool daChet,
                                 Vector3 viTri, DongBoTran db)
    {
        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = chiSo, viTri = viTri, gocY = 0f,
            mau01 = mau01, dangChay = false, daChet = daChet
        };
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        KenhTrucTiep.guiSangBenKia = null;
        File.WriteAllText("PlayTestShots/kynang_mang.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_KyNangMang", "TAM_DongBo" })
        {
            var rac = GameObject.Find(ten);
            if (rac != null) Object.DestroyImmediate(rac);
        }

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.update += TraLaiCanh;
    }
}
