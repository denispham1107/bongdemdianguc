using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: DON CUA QUAI PHAI DI QUA DUOC MANG.
///
/// Sau khi cho chu phong lam trong tai cua quai, mot lo hong moi mo ra ma
/// khong ai bao gi ca:
///
///   - Tren may CHU PHONG, con quai danh vao ban sao cua nguoi khach. Ban sao
///     do co mat mau, nhung mau cua no bi goi tin tu may khach de len 60 lan
///     moi giay - nen sat thuong bien mat khong dau vet.
///   - Tren may KHACH, quai la ban sao va AI da tat, nen no khong danh ai.
///
/// Cong lai: NGUOI KHACH BAT TU TRUOC QUAI.
///
/// Phep thu nay do chinh dieu do (chieu 1), roi do rang sau khi sua thi don
/// cua quai di qua duoc mang va nguoi khach lai mat mau binh thuong.
///
/// Ket qua ghi ra <c>PlayTestShots/donquai.txt</c>.
/// </summary>
public static class ThuDonQuai
{
    const string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/39. Chay thu DON CUA QUAI qua mang", false, 126)]
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
        var go = new GameObject("TAM_DonQuai");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[DonQuai] " + s); }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 1] don cua quai phai di qua duoc mang");

        PlayerController toi = null;
        float hetHan = Time.time + 25f;
        while (toi == null && Time.time < hetHan)
        {
            toi = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }

        TranHienTai.DangChoiMang = true;
        TranHienTai.LaHost = true;

        // Don quai that di cho khoi lam nhieu so do
        var dir = GameDirector.Instance;
        if (dir != null) dir.enabled = false;
        int donQuai = 0;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include,
                                                           FindObjectsSortMode.None))
        { Object.DestroyImmediate(q.gameObject); donQuai++; }
        Ghi("da don " + donQuai + " con quai co san de mau chi doi vi phep thu");

        yield return new WaitForSeconds(1.5f);

        var mauToi = toi.GetComponent<Damageable>();

        // ---- 1. LO HONG: mau ban sao bi de len, sat thuong bien mat ----
        //
        // Dung lai dung tinh huong tren may chu phong: con quai danh vao ban
        // sao cua nguoi khach, roi mot goi tin tu may khach den va ghi de mau.
        Vector3 choKia = toi.transform.position + new Vector3(5f, 0f, 0f);
        var banSao = NguoiChoiKhac.Sinh("uid-kia", "Nguoi kia", choKia);
        if (banSao == null) { Ghi("[LOI] khong sinh duoc ban sao"); loi++; Ket(); yield break; }

        var mauBanSao = banSao.GetComponent<Damageable>();

        // Dung lai DUNG tinh huong cu, luc ban sao con tru mau cuc bo. Gio ban
        // sao mac dinh khong tru nua (mauDoMayKhacQuyet), nhung chieu nay ghi
        // lai VI SAO can goi don quai - nen phai cho no chay nhu hoi do.
        mauBanSao.mauDoMayKhacQuyet = false;
        float truocDanh = mauBanSao.health;
        mauBanSao.TakeDamage(40f, DamageType.Physical, banSao.transform.position);
        float sauDanh = mauBanSao.health;

        // Goi tin tu may khach den: "toi con day nhieu mau"
        var goDb = new GameObject("TAM_DongBoDonQuai");
        var db = goDb.AddComponent<DongBoTran>();
        db.toi = toi; db.chiSoCuaToi = 0; db.GanTaiNghe(); db.ThemNguoi(1, banSao);

        var ds = new GoiTin.MotNguoi[1];
        ds[0] = new GoiTin.MotNguoi
        {
            chiSo = 1, viTri = banSao.transform.position, gocY = 0f,
            mau01 = truocDanh / mauBanSao.maxHealth, dangChay = false, daChet = false
        };
        KenhTrucTiep.GiaLapNhan(GoiTin.SangChuoi(GoiTin.VietTrangThai(db.GioTran(), ds, 1)));
        yield return null; yield return null;

        float sauGoi = mauBanSao.health;
        Ghi("1. quai danh ban sao: " + truocDanh.ToString("F0") + " -> "
            + sauDanh.ToString("F0") + " mau, roi mot goi tin tu may kia den -> "
            + sauGoi.ToString("F0") + " mau");
        Ghi("   => sat thuong bi xoa sach: " + (Mathf.Abs(sauGoi - truocDanh) < 0.5f)
            + " (dung nhu vay thi nguoi khach bat tu truoc quai)");

        // ---- 2. Goi don quai: viet ra roi doc lai phai khop ----
        var donGoc = new GoiTin.MotDonQuai
        {
            idQuai = 777, kieuDon = 2, chiSoNanNhan = 1,
            diemNgam = new Vector3(-4.56f, 0.75f, 9.87f),
            satThuong = 35.05f
        };
        GoiTin.MotDonQuai donDoc;
        byte[] bd = GoiTin.VietDonQuai(donGoc);
        bool docDuoc = GoiTin.DocDonQuai(bd, out donDoc);
        float lech = docDuoc ? Vector3.Distance(donGoc.diemNgam, donDoc.diemNgam) : 999f;

        Ghi("2. goi don quai " + bd.Length + " byte, doc lai: idQuai=" + donDoc.idQuai
            + " kieuDon=" + donDoc.kieuDon + " nanNhan=" + donDoc.chiSoNanNhan
            + " lech diem ngam=" + lech.ToString("F4") + " m");
        if (!docDuoc || donDoc.idQuai != donGoc.idQuai || donDoc.kieuDon != donGoc.kieuDon
            || donDoc.chiSoNanNhan != donGoc.chiSoNanNhan || lech > 0.02f)
        { Ghi("[LOI] goi don quai doc ra khong khop luc viet"); loi++; }
        // 2b. Sat thuong that di kem (28/09/2026) + goi CU 15 byte van doc duoc (sat thuong 0)
        {
            var cu = new byte[15]; System.Array.Copy(bd, cu, 15);
            GoiTin.MotDonQuai donCu;
            bool docCu = GoiTin.DocDonQuai(cu, out donCu);
            Ghi(string.Format("2b. sat thuong kem goi: viet {0:F2} -> doc {1:F2}; goi cu 15 byte: doc duoc {2}, sat thuong {3:F2}",
                donGoc.satThuong, donDoc.satThuong, docCu, donCu.satThuong));
            if (Mathf.Abs(donDoc.satThuong - donGoc.satThuong) > 0.03f || !docCu || donCu.satThuong != 0f)
            { Ghi("[LOI] sat thuong trong goi don quai sai / goi cu khong doc duoc"); loi++; }
        }

        // ---- 3. Chu phong ra don thi CO goi di ra ----
        KenhTrucTiep.Tao();
        KenhTrucTiep.NhanTraLoi("{}");
        var daGui = new System.Collections.Generic.List<string>();
        KenhTrucTiep.guiSangBenKia = t => daGui.Add(t);

        var goQuai = new GameObject("TAM_BoQuai");
        var boQuai = goQuai.AddComponent<DongBoQuai>();
        boQuai.dongBo = db;
        db.quai = boQuai;
        yield return null;

        // Dung mot con quai that, cho no danh minh
        Vector3 choQuai = toi.transform.position + new Vector3(1.2f, 0f, 0f);
        choQuai.y = VfxFactory.GroundY(choQuai) + 0.15f;
        var conQuai = EnemyFactory.Spawn(MonsterType.Skeleton, choQuai, null, toi.transform);
        if (conQuai == null) { Ghi("[LOI] khong dung duoc con quai"); loi++; Ket(); yield break; }
        // DanhSo la duong that moi con quai deu di qua - no cap so hieu VA ghi
        // ten nghe. Goi dung ham ay chu khong tu tay lam hai viec do o day:
        // lam tay thi phep thu se khong bat duoc loi "quen ghi ten nghe".
        if (dir != null) dir.DanhSo(conQuai, MonsterType.Skeleton);
        var aiQuai = conQuai.GetComponent<EnemyAI>();
        var soQuai = conQuai.GetComponent<NhanDangQuai>();

        daGui.Clear();
        // Chu phong: GameDirector.LamManhTheoDot nhan sat thuong theo dot (dot 1 x 0,65). Lam dung phep nhan ay tren con nay
        // de biet goi gui di mang so DA NHAN, khong phai so goc.
        float satGoc = aiQuai.attackDamage;
        aiQuai.attackDamage = satGoc * GameDirector.HeSoSatThuongDotDau;
        aiQuai.RaDonNgay();                 // ep no ra don, khong doi hoi chieu
        yield return new WaitForSeconds(1.2f);

        int soGoiDon = 0;
        foreach (var t in daGui)
            if (GoiTin.LoaiCuaGoi(GoiTin.TuChuoi(t)) == GoiTin.LoaiDonQuai) soGoiDon++;

        Ghi("3. chu phong: quai ra don -> so goi don quai gui di: " + soGoiDon
            + " (phai it nhat 1)");
        if (soGoiDon < 1)
        { Ghi("[LOI] quai ra don ma khong bao gi sang may kia"); loi++; }
        float satTrongGoi = -1f;
        foreach (var t in daGui)
        {
            var bb = GoiTin.TuChuoi(t);
            GoiTin.MotDonQuai dd;
            if (GoiTin.LoaiCuaGoi(bb) == GoiTin.LoaiDonQuai && GoiTin.DocDonQuai(bb, out dd)) satTrongGoi = dd.satThuong;
        }
        Ghi(string.Format("3b. goi don quai mang sat thuong {0:F2} (con quai tren chu phong da nhan he so dot: {1:F2}; goc {2:F2})",
            satTrongGoi, aiQuai.attackDamage, satGoc));
        if (Mathf.Abs(satTrongGoi - aiQuai.attackDamage) > 0.03f)
        { Ghi("[LOI] goi don quai khong mang sat thuong DA NHAN he so dot"); loi++; }
        float satDaNhan = aiQuai.attackDamage;
        aiQuai.attackDamage = satGoc;       // ban sao ben khach: sinh bang EnemyFactory, KHONG qua he so dot

        // ---- 4. PHEP DO CHINH: may khach nghe goi thi MAT MAU ----
        //
        // Doi vai: bay gio may nay la KHACH. Con quai o day chi la ban sao,
        // AI da tat. No phai dien lai don theo loi ke - va don ay phai lam
        // dau that, khong thi nguoi khach van bat tu.
        TranHienTai.LaHost = false;
        aiQuai.enabled = false;             // dung nhu ban sao ben khach
        boQuai.GhiDeQuaiChoPhepThu(soQuai.id, conQuai);

        // Dat quai sat canh minh de don cham toi
        if (mauToi.IsDead) { Ghi("[LOI] nhan vat da chet truoc khi do"); loi++; Ket(); yield break; }

        float truoc4 = mauToi.health;
        byte[] goiDon = GoiTin.VietDonQuai(new GoiTin.MotDonQuai
        {
            idQuai = soQuai.id, kieuDon = 0, chiSoNanNhan = 0,
            diemNgam = toi.transform.position
        });
        boQuai.NhanDonQuai(goiDon, 0);
        yield return new WaitForSeconds(1.2f);

        float mat4 = truoc4 - mauToi.health;
        Ghi("4. may khach nghe 'quai ra don' -> minh mat " + mat4.ToString("F0")
            + " mau (phai lon hon 0)");
        if (mat4 <= 0f)
        { Ghi("[LOI] nguoi khach van bat tu truoc quai"); loi++; }

        // ---- 5. Goi lap lai khong duoc danh hai lan ----
        yield return new WaitForSeconds(0.5f);
        float truoc5 = mauToi.health;
        boQuai.NhanDonQuai(goiDon, 0);
        boQuai.NhanDonQuai(goiDon, 0);
        yield return new WaitForSeconds(1.2f);

        float mat5 = truoc5 - mauToi.health;
        Ghi("5. cung goi ay den them hai lan -> mat them " + mat5.ToString("F0")
            + " mau (phai la 0)");
        if (mat5 > 0f) { Ghi("[LOI] mot don cua quai an mau nhieu lan"); loi++; }

        // ---- 6. Don nham nguoi KHAC thi minh khong mat mau ----
        yield return new WaitForSeconds(0.5f);
        float truoc6 = mauToi.health;
        boQuai.NhanDonQuai(GoiTin.VietDonQuai(new GoiTin.MotDonQuai
        {
            idQuai = soQuai.id, kieuDon = 0, chiSoNanNhan = 1,   // nham nguoi kia
            diemNgam = banSao.transform.position
        }), 0);
        yield return new WaitForSeconds(1.2f);

        float mat6 = truoc6 - mauToi.health;
        Ghi("6. don nham NGUOI KIA -> minh mat " + mat6.ToString("F0") + " mau (phai la 0)");
        if (mat6 > 0f) { Ghi("[LOI] an don thay nguoi khac"); loi++; }

        // ---- 7. Khach an DUNG sat thuong cua chu phong (da nhan he so dot), khong phai so goc cua ban sao ----
        yield return new WaitForSeconds(0.5f);
        aiQuai.attackDamage = satGoc;
        float truoc7 = mauToi.health;
        boQuai.NhanDonQuai(GoiTin.VietDonQuai(new GoiTin.MotDonQuai
        {
            idQuai = soQuai.id, kieuDon = 0, chiSoNanNhan = 0, soThuTu = 1000,
            diemNgam = toi.transform.position, satThuong = satDaNhan
        }), 0);
        yield return new WaitForSeconds(1.2f);
        float mat7 = truoc7 - mauToi.health;
        // DOI CHUNG: goi khong kem sat thuong (nhu ban cu) -> khach an so GOC cua ban sao
        aiQuai.attackDamage = satGoc;
        float truoc7b = mauToi.health;
        boQuai.NhanDonQuai(GoiTin.VietDonQuai(new GoiTin.MotDonQuai
        {
            idQuai = soQuai.id, kieuDon = 0, chiSoNanNhan = 0, soThuTu = 1001,
            diemNgam = toi.transform.position
        }), 0);
        yield return new WaitForSeconds(1.2f);
        float mat7b = truoc7b - mauToi.health;
        Ghi(string.Format("7. khach an don co kem sat thuong: mat {0:F2} (chu phong {1:F2}) | DOI CHUNG goi cu khong kem: mat {2:F2} (so goc ban sao {3:F2})",
            mat7, satDaNhan, mat7b, satGoc));
        if (Mathf.Abs(mat7 - satDaNhan) > 0.05f) { Ghi("[LOI] khach khong an dung sat thuong cua chu phong"); loi++; }
        if (Mathf.Abs(mat7b - satGoc) > 0.05f) { Ghi("[LOI] doi chung goi cu khong ra so goc - phep do khong phan biet duoc"); loi++; }

        // ---- Don ----
        KenhTrucTiep.guiSangBenKia = null;
        KenhTrucTiep.Dong();
        if (conQuai != null) Object.DestroyImmediate(conQuai);
        Object.DestroyImmediate(goQuai);
        Object.DestroyImmediate(goDb);
        NguoiChoiKhac.Bo(banSao);
        TranHienTai.Xoa();

        Ghi("so loi ghi nhan = " + loi);
        Ket();
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
        File.WriteAllText("PlayTestShots/donquai.txt", bao.ToString());

        foreach (var ten in new[] { "TAM_DonQuai", "TAM_BoQuai", "TAM_DongBoDonQuai" })
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
