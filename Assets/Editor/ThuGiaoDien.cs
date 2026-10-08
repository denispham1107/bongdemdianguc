using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: GIAO DIEN DANG NHAP - SANH CHO - TRONG PHONG - DEM NGUOC - CAI DAT.
///
/// Nguoi dung bao: vao game van hien menu choi don cu; giao dien so sai, nhieu
/// cho chu tran bi che; bang Cai dat mat dau tieng Viet ("CAI D T").
///
/// Phep thu nay di het cac man, o moi man:
///   - dem so lan chu KHONG VUA O (GiaoDien.SoLanCat: phai cat bot va them
///     "…") va so lan phai thu nho co chu - dem trong chinh ham ve, nen moi
///     nhan, moi nut deu duoc tinh, khong sot cai nao;
///   - hoi kieu chu dang dung co that la font Inter khong (font mac dinh thieu
///     chu co dau);
///   - chup anh de nhin.
/// Va kiem: da dang nhap san ma quay ve MainMenu thi vao THANG sanh, khong
/// hien menu choi don cu.
///
/// Anh ra <c>PlayTestShots/gd_*.png</c>, so do ra <c>PlayTestShots/giaodien.txt</c>.
/// Buoc cuoi xoa phong da tao, dang xuat, tra lai phien dang nhap cu.
/// </summary>
public static class ThuGiaoDien
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;
    static bool coPhienGoc; static string phienGoc;
    const string KhoaPhien = "diablo25d_refresh";

    [MenuItem("Diablo 2.5D/50. Chay thu GIAO DIEN dang nhap - sanh - phong", false, 137)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (!ThongTinChayThu.DocHoacBao()) return;

        canhCu = EditorSceneManager.GetActiveScene().path;
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien);
        phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        PlayerPrefs.DeleteKey(KhoaPhien);           // de man dang nhap hien ra - xem ChupManMang
        PlayerPrefs.Save();
        // Bo ca phien CON TRONG BO NHO: tat domain reload thi bien tinh cua
        // FirebaseMang song qua cac lan Play - xoa khoa thoi van vao thang sanh.
        FirebaseMang.Quen();

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] giao dien dang nhap - sanh - phong");
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuGiaoDien");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[GiaoDien] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 60 && !File.Exists(duong); i++)
            yield return new WaitForEndOfFrame();
    }

    /// <summary>Dem chu tran trong vai khung hinh roi chup.</summary>
    static IEnumerator DoMan(string ten, string anh)
    {
        GiaoDien.DatLaiDem();
        for (int i = 0; i < 4; i++) yield return null;
        int cat = GiaoDien.SoLanCat, nho = GiaoDien.SoLanThuNho, ve = GiaoDien.SoLuotVe;
        string chuCat = GiaoDien.ChuBiCatCuoi;
        yield return Chup(anh);
        Ghi(string.Format("{0}: {5} luot ve, chu bi cat {1} lan{2}, chu phai thu nho {3} lan - anh {4}.png",
                          ten, cat, cat > 0 ? " (vd \"" + chuCat + "\")" : "", nho, anh, ve));
        // Khong ve gi thi hai so tren bang 0 ma chang chung minh duoc gi -
        // lan chay dau cua phep thu nay da bao "0 lan cat" cho mot sanh trong tron.
        if (GiaoDien.ChuBiThuNho.Count > 0)
            Ghi("      chu phai thu nho: " + string.Join(" | ", GiaoDien.ChuBiThuNho));
        Kiem(ve > 0, ten + ": KHONG CO GI DUOC VE");
        Kiem(cat == 0, ten + ": co chu khong vua o, phai cat bot");
    }

    /// <summary>
    /// Hoan nhip hoi lai cua sanh 30 giay - de du lieu gia dat vao khong bi ban
    /// tai tu may chu ghi de truoc luc chup. Lan chay dau, anh "phong du bon
    /// nguoi" van hien 1/4 vi mot nhip hoi lai den giua chung.
    /// </summary>
    static void HoanHoiLai(ManSanh sanh)
    {
        var f = typeof(ManSanh).GetField("hoiLanSau", BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null) f.SetValue(sanh, Time.unscaledTime + 30f);
    }

    /// <summary>
    /// Nguoi dung 08/10/2026: khung dang nhap / sanh / phong DUC BOT 40% de thay phu thuy phia sau. DO TREN ANH,
    /// khong doc hang so: chup man CO giao dien va KHONG giao dien (tat component ve), trong <paramref name="vung"/>
    /// lay trung vi ti so do sang (co / khong) - phan canh lot qua long khung. Ban cu (duc 0,55) ~0,45 + mau long
    /// khung; ban moi phai ~0,67. Neu <paramref name="tieuDe"/> thi do luon cho ANH TEN GAME: cac diem doi mau
    /// phia TREN khung la anh ten game, lay tam cua chung theo chieu cao man hinh.
    /// </summary>
    static IEnumerator DoXuyenThau(string ten, MonoBehaviour ui, Rect vung, float duKien, bool tieuDe)
    {
        yield return new WaitForEndOfFrame();
        var co = ScreenCapture.CaptureScreenshotAsTexture();
        ui.enabled = false;
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        var khong = ScreenCapture.CaptureScreenshotAsTexture();
        ui.enabled = true;
        int H = co.height;
        var tiSo = new System.Collections.Generic.List<float>();
        int x0 = Mathf.Max(0, Mathf.RoundToInt(vung.x)), x1 = Mathf.Min(co.width, Mathf.RoundToInt(vung.xMax));
        int y0 = Mathf.Max(0, Mathf.RoundToInt(vung.y)), y1 = Mathf.Min(H, Mathf.RoundToInt(vung.yMax));
        for (int y = y0; y < y1; y += 2)
            for (int x = x0; x < x1; x += 2)
            {
                float a = co.GetPixel(x, H - 1 - y).grayscale, b = khong.GetPixel(x, H - 1 - y).grayscale;
                if (b > 0.05f) tiSo.Add(a / b);
            }
        tiSo.Sort();
        float trungVi = tiSo.Count > 0 ? tiSo[tiSo.Count / 2] : -1f;
        Ghi(ten + ": canh lot qua long khung (trung vi sang co/khong giao dien, " + tiSo.Count + " diem) = " + trungVi.ToString("F2")
            + "; du kien ~" + duKien.ToString("F2") + " (ban cu duc 0,55 -> ~0,45)");
        Kiem(trungVi > 0.58f && trungVi < 0.85f, ten + ": long khung khong trong hon 40% nhu yeu cau");

        if (tieuDe)
        {
            int dinh = -1, day = -1;
            int ySat = Mathf.RoundToInt(vung.y - 6f * GiaoDien.TiLe);
            // Chi trong be ngang anh ten game, va hang phai doi >= 10% so diem: qua cau lua tren tay phu thuy
            // (lap loe giua hai lan chup) lot qua duoi chan anh tung lam tam lech xuong 0,21.
            float rongTd = Mathf.Min(co.width - 40f * GiaoDien.TiLe, 760f * GiaoDien.TiLe);
            int xa = Mathf.Max(0, Mathf.RoundToInt((co.width - rongTd) * 0.5f)), xb = Mathf.Min(co.width, Mathf.RoundToInt((co.width + rongTd) * 0.5f));
            int nguong = Mathf.Max(6, (xb - xa) / 2 / 10);
            for (int y = 0; y < ySat; y++)
            {
                int dem = 0;
                for (int x = xa; x < xb; x += 2)
                {
                    var c1 = co.GetPixel(x, H - 1 - y); var c2 = khong.GetPixel(x, H - 1 - y);
                    if (Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b) > 0.25f) dem++;
                }
                if (dem >= nguong) { if (dinh < 0) dinh = y; day = y; }
            }
            float tam = dinh >= 0 ? (dinh + day) * 0.5f / H : -1f;
            float tamCu = (vung.y - 22f * GiaoDien.TiLe - 0.5f * Mathf.Min(Screen.width - 40f * GiaoDien.TiLe, 760f * GiaoDien.TiLe) / GiaoDien.TiLeTieuDe) / H;
            Ghi("   anh ten game tren anh: y " + dinh + "-" + day + " / " + H + ", tam " + tam.ToString("F3") + " chieu cao (TamTieuDe " + ManDangNhap.TamTieuDe.ToString("F3") + " + ~0,011;"
                + " cho cu sat tren khung: tam ~" + tamCu.ToString("F3") + ")");
            Kiem(dinh >= 0, "khong thay anh ten game phia tren khung dang nhap");
            // Tam chu tren anh lech ~+0,011 so voi TamTieuDe (vien quang trong suot phia tren anh)
            Kiem(Mathf.Abs(tam - (ManDangNhap.TamTieuDe + 0.011f)) < 0.02f, "anh ten game khong o dung cho da chon");
        }
        Object.Destroy(co); Object.Destroy(khong);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("man hinh Game: " + Screen.width + "x" + Screen.height + ", ti le giao dien " + GiaoDien.TiLe.ToString("F2"));
        yield return new WaitForSecondsRealtime(0.8f);

        // ---- 1. Dang nhap ----
        Kiem(!FirebaseMang.DaDangNhap, "chua xoa duoc phien cu - khong co man dang nhap de do");
        yield return DoMan("1. man dang nhap", "gd_1_dangnhap");

        string fontChu = GiaoDien.KieuChu != null && GiaoDien.KieuChu.font != null ? GiaoDien.KieuChu.font.name : "(khong co)";
        string fontDam = GiaoDien.KieuNutMau != null && GiaoDien.KieuNutMau.font != null ? GiaoDien.KieuNutMau.font.name : "(khong co)";
        Ghi("   font dang dung: chu thuong = " + fontChu + ", nut = " + fontDam);
        Kiem(fontChu == "Inter-Regular" && fontDam == "Inter-SemiBold", "giao dien khong dung font Inter");

        var dn = Object.FindAnyObjectByType<ManDangNhap>();
        if (dn != null)
        {
            float sDn = GiaoDien.TiLe;
            var bcDn = ManDangNhap.TinhBoCuc(Screen.width, Screen.height, sDn, false, 0f, GiaoDien.TiLeTieuDe);
            yield return DoXuyenThau("1b. khung dang nhap", dn, bcDn.khung, 1f - GiaoDien.DoDucKhungSanh, true);
        }
        var fTrang = typeof(ManDangNhap).GetField("trang", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dn != null && fTrang != null)
        {
            fTrang.SetValue(dn, ManDangNhap.Trang.DangKy);
            yield return DoMan("2. tab tao tai khoan", "gd_2_dangky");
            fTrang.SetValue(dn, ManDangNhap.Trang.DangNhap);
        }

        // ---- 2. Sanh ----
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] dang nhap: " + e); loi++; Ket(); yield break; }
        if (dn != null && dn.daVao != null) dn.daVao();
        yield return new WaitForSecondsRealtime(2.5f);
        {
            var dsSanh = Object.FindObjectsByType<ManSanh>(FindObjectsInactive.Include);
            var dsMenu = Object.FindObjectsByType<MainMenuUI>(FindObjectsInactive.Include);
            var dsDn = Object.FindObjectsByType<ManDangNhap>(FindObjectsInactive.Include);
            string bat = "";
            foreach (var ms in dsSanh) bat += (ms.enabled ? "bat" : "tat") + (ms.isActiveAndEnabled ? "/chay " : "/khong ");
            Ghi("   chan doan: MainMenuUI " + dsMenu.Length + ", ManDangNhap " + dsDn.Length + ", ManSanh " + dsSanh.Length
                + " [" + bat.Trim() + "], da dang nhap = " + FirebaseMang.DaDangNhap
                + ", daVao cua dn = " + (dn != null && dn.daVao != null));
        }
        yield return DoMan("3. sanh cho", "gd_3_sanh");

        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh == null) { Ghi("[LOI] khong co sanh"); loi++; Ket(); yield break; }
        {
            var bcS = ManSanh.TinhBoCucSanh(Screen.width, Screen.height, GiaoDien.TiLe);
            yield return DoXuyenThau("3c. khung danh sach phong", sanh, bcS.khungDanhSach, 1f - GiaoDien.DoDucKhungSanh, false);
        }

        // ---- 3b. Danh sach co phong - du lieu gia CHI trong may, sanh hoi lai
        // moi 2 giay se ghi de, nen chup ngay. Mot phong ten dai het co, mot
        // phong da day - hai truong hop de tran chu nhat.
        {
            var fDs = typeof(ManSanh).GetField("danhSach", BindingFlags.NonPublic | BindingFlags.Instance);
            var ds = new System.Collections.Generic.List<PhongMang.Phong>();
            var p1 = new PhongMang.Phong { ma = "gia1", ten = "Nghĩa địa không lối thoát", hostUid = "x",
                                           hostTen = "KẻĐiSănĐêmKhuya99", manChoi = "Act2", trangThai = "cho", soNguoi = 2, toiDa = 4 };
            var p2 = new PhongMang.Phong { ma = "gia2", ten = "Hầm mộ máu", hostUid = "y",
                                           hostTen = "Bóng Ma", manChoi = "Act2", trangThai = "cho", soNguoi = 4, toiDa = 4 };
            ds.Add(p1); ds.Add(p2);
            HoanHoiLai(sanh);
            yield return new WaitForSecondsRealtime(2.5f);     // cho luot hoi dang bay (neu co) ve het
            fDs.SetValue(sanh, ds);
            yield return DoMan("3b. sanh co hai phong (du lieu gia)", "gd_3b_sanh_coPhong");
        }

        sanh.MoCaiDat();
        yield return DoMan("4. bang cai dat", "gd_4_caidat");
        sanh.BamOKCaiDat();          // khong doi gi -> chi dong

        // ---- 3. Quay ve MainMenu khi DA dang nhap -> vao thang sanh ----
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        yield return new WaitForSecondsRealtime(1.5f);
        var sanhMoi = Object.FindAnyObjectByType<ManSanh>();
        bool vaoThang = sanhMoi != null && sanhMoi.enabled;
        Ghi("5. nap lai MainMenu khi dang dang nhap -> sanh hien ngay: " + (vaoThang ? "co" : "KHONG (hien menu choi don cu)"));
        Kiem(vaoThang, "quay ve MainMenu ma khong vao thang sanh");
        yield return DoMan("   sanh sau khi nap lai", "gd_5_vethang_sanh");
        sanh = sanhMoi;

        // ---- 4. Trong phong ----
        yield return PhongMang.TaoPhong("", (o, err) => { ok = o; e = err; });
        Ghi("   tao phong: " + (ok ? "OK - " + PhongMang.PhongHienTai.ma + " \"" + PhongMang.PhongHienTai.ten + "\"" : "LOI - " + e));
        if (ok && sanh != null)
        {
            var f = typeof(ManSanh).GetField("dangO", BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(sanh, System.Enum.Parse(f.FieldType, "TrongPhong"));
            yield return new WaitForSecondsRealtime(2.0f);
            yield return DoMan("6. trong phong (chu phong)", "gd_6_trongphong");

            // 6b. Phong du bon nguoi - them ba nguoi GIA vao ban sao trong may
            {
                HoanHoiLai(sanh);
                yield return new WaitForSecondsRealtime(1.5f);
                var pp = PhongMang.PhongHienTai;
                pp.nguoiChoi.Add(new PhongMang.NguoiTrongPhong { uid = "g1", ten = "Nguyễn Thị Hằng Nga", sanSang = true, cho = 1 });
                pp.nguoiChoi.Add(new PhongMang.NguoiTrongPhong { uid = "g2", ten = "ĐồTểLàngMa", sanSang = false, cho = 2 });
                pp.nguoiChoi.Add(new PhongMang.NguoiTrongPhong { uid = "g3", ten = "Bóng Ma", sanSang = true, cho = 3 });
                yield return DoMan("6b. phong du bon nguoi (du lieu gia)", "gd_6b_phong_4nguoi");
            }

            // ---- 5. Dem nguoc - chi dat o ban sao trong may, KHONG ghi len
            // Firebase: dem nguoc that thi PhongMang.GiayDemNguoc giay sau nap Act2 va danh dau
            // phong "dang choi".
            var p = PhongMang.PhongHienTai;
            string cu = p.trangThai; double cuLuc = p.batDauLuc;
            p.trangThai = "demNguoc";
            p.batDauLuc = PhongMang.GioMayChu() + 7400.0;
            yield return DoMan("7. dem nguoc", "gd_7_demnguoc");

            // 7b. Dong chu dem nguoc + gach do KHONG de len nhan vat (nguoi dung 13/09/2026: "dang bi
            // lan xuong che mat nhan vat"). Dinh nhan vat do bang KHUNG BAO cac SkinnedMeshRenderer
            // dang hien trong canh chieu len man hinh - khong dua vao con so bo cuc cua ManSanh.
            {
                float sGd = GiaoDien.TiLe;
                var cam = Camera.main;
                float dinhNv = float.MaxValue;
                int soNv = 0;
                if (cam != null)
                    foreach (var sk in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None))
                    {
                        if (!sk.enabled || !sk.gameObject.activeInHierarchy) continue;
                        var bb = sk.bounds;
                        var tamMh = cam.WorldToScreenPoint(bb.center);
                        if (tamMh.z <= 0f || tamMh.x < 0f || tamMh.x > Screen.width) continue;
                        soNv++;
                        for (int gc = 0; gc < 8; gc++)
                        {
                            var g = bb.center + Vector3.Scale(bb.extents, new Vector3((gc & 1) == 0 ? -1 : 1, (gc & 2) == 0 ? -1 : 1, (gc & 4) == 0 ? -1 : 1));
                            var mh = cam.WorldToScreenPoint(g);
                            dinhNv = Mathf.Min(dinhNv, Screen.height - mh.y);
                        }
                    }
                var tieuDe = ManSanh.KhungTieuDeDemNguoc(sGd);
                var gach = ManSanh.KhungGachDemNguoc(sGd);
                Ghi("7b. dem nguoc: dong chu y " + tieuDe.yMin.ToString("F0") + "-" + tieuDe.yMax.ToString("F0") + ", gach do y "
                    + gach.yMax.ToString("F0") + "; dinh khung bao nhan vat y " + (soNv > 0 ? dinhNv.ToString("F0") : "?")
                    + " (" + soNv + " mesh, man " + Screen.height + " cao)");
                Kiem(soNv > 0, "khong tim thay nhan vat trong canh de do");
                Kiem(gach.yMax < dinhNv, "dong chu / gach do dem nguoc van de len nhan vat");
            }

            // 7c. Con so + vong phu chu: 13/09/2026 nho bot 15%, 08/10/2026 thu tiep x0,6 va dua len tren dau
            // nhan vat. Do be ngang VUNG DO RUC cua vong tren ANH CHUP (vong dap nhip to them toi da 7%) quanh
            // DUONG NGANG QUA TAM VONG MOI: phai khop CoVongDemNguoc.
            {
                float sGd = GiaoDien.TiLe;
                yield return new WaitForEndOfFrame();
                var tex = ScreenCapture.CaptureScreenshotAsTexture();
                int trai = int.MaxValue, phai = -1;
                var tamDn = ManSanh.TamDemNguoc(sGd);
                int cx = tex.width / 2, nua = Mathf.RoundToInt(ManSanh.CoVongDemNguoc * 0.75f * sGd);
                int cy = tex.height - Mathf.RoundToInt(tamDn.y);   // anh chup goc duoi-trai
                // Quet 5 hang quanh DUONG NGANG QUA TAM (vong tron doi xung, xoay khong doi be ngang):
                // diem "do troi" = do hon ca xanh la lan xanh lam 0,12. Lan dau dung nguong "do ruc"
                // (r > 0,55) thi vong bi ve mo theo nhip khong qua duoc - chi do trung con so (72 diem).
                for (int y = cy - 2; y <= cy + 2; y++)
                    for (int x = Mathf.Max(0, cx - nua); x < Mathf.Min(tex.width, cx + nua); x++)
                    {
                        var c = tex.GetPixel(x, y);
                        if (c.r - Mathf.Max(c.g, c.b) > 0.12f) { if (x < trai) trai = x; if (x > phai) phai = x; }
                    }
                Object.Destroy(tex);
                float rongVong = phai >= 0 ? phai - trai + 1 : 0f;
                float moi = ManSanh.CoVongDemNguoc * sGd, cuVong = 400f * sGd;
                Ghi("7c. vong phu chu tren anh (hang y " + tamDn.y.ToString("F0") + "): rong " + rongVong.ToString("F0") + " diem; co moi "
                    + moi.ToString("F0") + "-" + (moi * 1.07f).ToString("F0") + " (dap nhip), co cu 400s = " + cuVong.ToString("F0")
                    + "; chu so cao " + (ManSanh.CaoSoDemNguoc * sGd).ToString("F0") + " (cu " + (306f * sGd).ToString("F0") + ")");
                // Phan co hinh cua anh vong chiem 94% canh anh, dap nhip to them toi da 7% -> 0,94-1,0 lan
                // co moi; co cu (400s, tam giua man) thi hang quet cat vong o day cung ngan hon / khong trung vong
                Kiem(rongVong >= moi * 0.85f && rongVong <= moi * 1.04f, "vong phu chu khong o dung co / dung cho moi");

                // 7d. Ca vong lan con so (luc DAP TO NHAT) phai nam TREN dinh mu phu thuy (xuong head_end - khung bao
                // SkinnedMesh cao hon mu that). Doi chung: vong + so co cu o giua man hinh thi de len.
                float dinhMu = float.MaxValue, tran = float.MaxValue; string tenXuong = "?";
                var camD = Camera.main;
                if (camD != null)
                    foreach (var tf in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                    {
                        string tn = tf.name.ToLowerInvariant();
                        if (!tn.EndsWith("head_end") && tn != "head") continue;
                        if (tf.GetComponentInParent<PlayerController>(true) == null) continue;   // phu thuy trung bay
                        var mh = camD.WorldToScreenPoint(tf.position);
                        if (mh.z <= 0f) continue;
                        // Cung cach BangTen: head_end + 0,03 m la chop mu; chi co Head thi + 0,25 m. Co head_end thi uu tien.
                        bool laEnd = tn.EndsWith("head_end");
                        if (!laEnd && tenXuong.ToLowerInvariant().EndsWith("head_end")) continue;
                        float yMh = Screen.height - camD.WorldToScreenPoint(tf.position + Vector3.up * (laEnd ? 0.03f : 0.25f)).y;
                        if (laEnd && !tenXuong.ToLowerInvariant().EndsWith("head_end")) dinhMu = float.MaxValue;
                        if (yMh < dinhMu)
                        {
                            dinhMu = yMh; tenXuong = tf.name;
                            // Tran ~0,15 m duoi dinh dau (head_end) / ~0,10 m tren xuong Head: day mat bat dau tu day
                            tran = Screen.height - camD.WorldToScreenPoint(tf.position + Vector3.up * (laEnd ? -0.15f : 0.10f)).y;
                        }
                    }
                float nuaCaoDap = Mathf.Max(ManSanh.CoVongDemNguoc * ManSanh.DapVongToiDa, ManSanh.CaoSoDemNguoc * ManSanh.DapSoToiDa) * sGd * 0.5f;
                float dayDn = tamDn.y + nuaCaoDap;
                float dayYen = tamDn.y + Mathf.Max(ManSanh.CoVongDemNguoc, ManSanh.CaoSoDemNguoc) * sGd * 0.5f;
                float dayCu = Screen.height * 0.5f + 400f * 1.07f * sGd * 0.5f;
                Ghi("7d. day vong/so dem nguoc: dung yen y " + dayYen.ToString("F0") + ", dap to nhat y " + dayDn.ToString("F0")
                    + "; chop mu phu thuy (" + tenXuong + ") y " + (dinhMu < float.MaxValue ? dinhMu.ToString("F0") : "?")
                    + ", tran y " + (tran < float.MaxValue ? tran.ToString("F0") : "?") + "; doi chung ban cu giua man: tam y "
                    + (Screen.height * 0.5f).ToString("F0") + " day " + dayCu.ToString("F0"));
                Kiem(dinhMu < float.MaxValue, "khong tim thay xuong dau phu thuy de do");
                // 08/10/2026 nguoi dung xin to them 10% (x0,572): luc dap nhip to nhat (thoang qua moi giay) mep vong cham chop mu
                // 2 diem - chap nhan; luc DUNG YEN phai tren chop mu, luc dap to nhat phai tren TRAN (khong che mat).
                Kiem(dayYen < dinhMu, "vong / con so dem nguoc (dung yen) de len chop mu phu thuy");
                Kiem(dayDn < tran, "vong / con so dem nguoc (dap to nhat) che mat phu thuy");
                Kiem(dayCu > dinhMu, "doi chung hong: ban cu giua man cung khong de len dau - phep do khong phan biet duoc");
                Kiem(tamDn.y - nuaCaoDap > ManSanh.KhungGachDemNguoc(sGd).yMax, "vong dem nguoc de len gach do / dong chu");
            }
            p.trangThai = cu; p.batDauLuc = cuLuc;

            string ma = p.ma;
            yield return FirebaseMang.Xoa("phong/" + ma, (o2, e2) => { });
            yield return FirebaseMang.Xoa("tran/" + ma, (o2, e2) => { });
            PhongMang.PhongHienTai = null;
            Ghi("   don phong thu: xong");
        }

        Ket();
    }

    static void Ket()
    {
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/giaodien.txt", bao.ToString());

        FirebaseMang.Quen();
        var rac = GameObject.Find("TAM_ThuGiaoDien");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;

        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc);
        else PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);
        Debug.Log("[GiaoDien] da tra lai phien dang nhap va scene " + EditorSceneManager.GetActiveScene().path
                  + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty);
    }
}
