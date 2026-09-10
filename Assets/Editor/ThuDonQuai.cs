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
            diemNgam = new Vector3(-4.56f, 0.75f, 9.87f)
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
        aiQuai.RaDonNgay();                 // ep no ra don, khong doi hoi chieu
        yield return new WaitForSeconds(1.2f);

        int soGoiDon = 0;
        foreach (var t in daGui)
            if (GoiTin.LoaiCuaGoi(GoiTin.TuChuoi(t)) == GoiTin.LoaiDonQuai) soGoiDon++;

        Ghi("3. chu phong: quai ra don -> so goi don quai gui di: " + soGoiDon
            + " (phai it nhat 1)");
        if (soGoiDon < 1)
        { Ghi("[LOI] quai ra don ma khong bao gi sang may kia"); loi++; }

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
