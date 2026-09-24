using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

/// <summary>
/// CHAY THU (menu 66): NUT KY NANG O SANH (SACH PHEP XEM TRUOC) + CAT THU TU O + CON MAT QUY.
///
/// Nguoi dung 14/09/2026:
///   - nut CAI DAT len dau trang, ben trai DANG XUAT (vi tri menu 48 do);
///   - cho cu thanh nut KY NANG: mo NGUYEN cua so Sach phep, hien du moi ky nang KHONG O
///     KHOA, keo tha san vao o; tat game mo lai thu tu o van con;
///   - con mat khoa goc nhin ve lai kieu kinh di, het "be hinh".
///
/// Do nhung gi:
///   A. Ngoai Play
///      A1. chu co dau trong ManSanh / CuaSoSachPhep / SachPhep deu co trong cmap Inter.
///      A2. cau noi localStorage (CauNoiCaiDat.jslib) CHAY THAT bang node voi localStorage gia:
///          ghi roi doc lai, khoa chua co, trinh duyet cam luu, chu co dau.
///      A3. SachPhep.cs goi dung hai ham ay tren WebGL.
///   B. Trong Play (MainMenu, dang nhap that)
///      B1. mo Sach phep xem truoc: HienDaMo ca 9 ky nang; DOI CHUNG cung cua so o che do
///          trong tran voi moi ky nang con khoa -> 0/9. Do sang hinh o cot trai va o o: xem
///          truoc phai sang han ban khoa (hinh ban khoa bi nhan 0,38).
///      B1b. MOI ky nang deu co bieu tuong o sanh - ke ca nhom cuoi danh sach, phai CUON xuong moi thay.
///      B2. nhan vat mau cua man chinh co that, thong so trung prefab (doc thang prefab).
///      B3. doi o (ca bo may tinh lan bo cam ung) -> chuoi trong KHO LUU dung nhu tinh tay.
///      B4. dang mo sach ma vao tran (nap Act2): sach phai dong; thanh ky nang trong tran
///          theo dung thu tu da xep o sanh.
///      B5. con mat quy o Act2 (ban cam ung): hai trang thai khac nhau, mo = trong cam ruc,
///          khoa = vet chem do cheo; DOI CHUNG mau sac ngoai nut.
///   Sau khi thoat Play: NapLai tu kho luu (nhu mo game lan sau) -> van dung thu tu.
///
/// Ket qua: PlayTestShots/sachphep_sanh.txt, anh PlayTestShots/sachphep_sanh_*.png.
/// </summary>
public static class ThuSachPhepSanh
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    const string KhoaPhien = "diablo25d_refresh";
    const string KhoaTron = "diablo25d.sachphep.tron";
    const string KhoaVuong = "diablo25d.sachphep.vuong";
    static bool coPhienGoc, coTronGoc, coVuongGoc;
    static string phienGoc, tronGoc, vuongGoc;

    // Thu tu mong doi - TINH TAY tu cac thao tac o B3, khong hoi SachPhep
    const string MongVuong = "8,7,3,2,4,5,6";
    const string MongTron = "0,1,2,3,4,5,8";

    [MenuItem("Diablo 2.5D/66. Chay thu NUT KY NANG o sanh (sach phep xem truoc) + con mat", false, 153)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");
        if (!ThongTinChayThu.DocHoacBao()) return;
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[menu 66] nut KY NANG o sanh, cat thu tu o, con mat quy");

        KiemChu();
        KiemCauNoi();

        canhCu = EditorSceneManager.GetActiveScene().path;
        coPhienGoc = PlayerPrefs.HasKey(KhoaPhien); phienGoc = PlayerPrefs.GetString(KhoaPhien, "");
        coTronGoc = PlayerPrefs.HasKey(KhoaTron); tronGoc = PlayerPrefs.GetString(KhoaTron, "");
        coVuongGoc = PlayerPrefs.HasKey(KhoaVuong); vuongGoc = PlayerPrefs.GetString(KhoaVuong, "");
        PlayerPrefs.DeleteKey(KhoaPhien);
        PlayerPrefs.Save();

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/MainMenu.unity")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_ThuSachPhepSanh");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[SachPhepSanh] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    // =============================================================
    // A1. CHU CO DAU
    // =============================================================

    static void KiemChu()
    {
        var cmapDam = BangKyTuFont.Doc("Assets/Resources/Fonts/Inter-SemiBold.ttf");
        var cmapThuong = BangKyTuFont.Doc("Assets/Resources/Fonts/Inter-Regular.ttf");
        if (cmapDam == null || cmapThuong == null) { Ghi("[LOI] khong doc duoc cmap Inter"); loi++; return; }
        string[] tep = { "Assets/Scripts/UI/ManSanh.cs", "Assets/Scripts/UI/CuaSoSachPhep.cs", "Assets/Scripts/UI/SachPhep.cs" };
        var thieu = new StringBuilder();
        int soChuoi = 0;
        bool coKyNang = false;
        foreach (var t in tep)
        {
            string nguon = File.ReadAllText(t);
            foreach (Match m in Regex.Matches(nguon, "\"([^\"\\\\]|\\\\.)*\""))
            {
                int dauDong = nguon.LastIndexOf('\n', m.Index) + 1;
                string truoc = nguon.Substring(dauDong, m.Index - dauDong);
                if (truoc.Contains("//") || truoc.Contains("Debug.Log")) continue;
                soChuoi++;
                if (m.Value == "\"KỸ NĂNG\"") coKyNang = true;
                foreach (char c in m.Value)
                    if (c >= 128 && (!cmapDam.Contains(c) || !cmapThuong.Contains(c)) && thieu.ToString().IndexOf(c) < 0) thieu.Append(c);
            }
        }
        Ghi(string.Format("A1. {0} chuoi trong 3 file; ky tu Inter thieu: \"{1}\"; co chu \"KỸ NĂNG\" (co dau): {2}",
            soChuoi, thieu, coKyNang ? "co" : "KHONG"));
        Kiem(thieu.Length == 0, "font Inter thieu: " + thieu);
        Kiem(coKyNang, "nut KY NANG khong co chu co dau");
    }

    // =============================================================
    // A2-A3. CAU NOI localStorage
    // =============================================================

    static void KiemCauNoi()
    {
        const string DuongJs = "Assets/Plugins/WebGL/CauNoiCaiDat.jslib";
        string jslib = File.ReadAllText(DuongJs);

        // Moi truong gia cua Emscripten: bo nho la mot mang chuoi, "con tro" la chi so
        string js = @"
var nho = [null]; function _malloc(n){ nho.push(''); return nho.length-1; }
function lengthBytesUTF8(s){ return Buffer.byteLength(s,'utf8'); }
function stringToUTF8(s,p,n){ nho[p]=s; }
function UTF8ToString(p){ return typeof p==='number' ? nho[p] : p; }
var kho = {}; var camLuu = false;
var window = { localStorage: {
  getItem: function(k){ if(camLuu) throw new Error('cam'); return Object.prototype.hasOwnProperty.call(kho,k)?kho[k]:null; },
  setItem: function(k,v){ if(camLuu) throw new Error('cam'); kho[k]=String(v); } } };
var LibraryManager = { library: {} };
function mergeInto(a,b){ for (var k in b) a[k]=b[k]; }
" + jslib + @"
var L = LibraryManager.library;
var out = [];
out.push('rong=[' + UTF8ToString(L.CD_DocChuoi('diablo25d.sachphep.vuong')) + ']');
out.push('ghi=' + L.CD_GhiChuoi('diablo25d.sachphep.vuong', '8,7,3,2,4,5,6'));
out.push('doc=[' + UTF8ToString(L.CD_DocChuoi('diablo25d.sachphep.vuong')) + ']');
out.push('kho=[' + kho['diablo25d.sachphep.vuong'] + ']');
L.CD_GhiChuoi('thu.dau', 'Kỹ năng'); out.push('dau=[' + UTF8ToString(L.CD_DocChuoi('thu.dau')) + ']');
camLuu = true;
out.push('camdoc=[' + UTF8ToString(L.CD_DocChuoi('diablo25d.sachphep.vuong')) + ']');
out.push('camghi=' + L.CD_GhiChuoi('x','y'));
out.push('so=' + L.CD_DocSo('x', 7));
console.log(out.join('|'));
";
        string tam = Path.Combine(Path.GetTempPath(), "thu_caunoi_sachphep.js");
        File.WriteAllText(tam, js, new UTF8Encoding(false));
        string ra = "", loiRa = "";
        try
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo("node", "\"" + tam + "\"")
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            p.Start();
            ra = p.StandardOutput.ReadToEnd().Trim();
            loiRa = p.StandardError.ReadToEnd().Trim();
            p.WaitForExit(20000);
        }
        catch (System.Exception e) { loiRa = e.Message; }
        File.Delete(tam);

        Ghi("A2. cau noi chay bang node: " + ra + (loiRa.Length > 0 ? "  LOI: " + loiRa : ""));
        string mong = "rong=[]|ghi=1|doc=[8,7,3,2,4,5,6]|kho=[8,7,3,2,4,5,6]|dau=[Kỹ năng]|camdoc=[]|camghi=0|so=7";
        Kiem(ra == mong, "cau noi localStorage sai. Mong: " + mong);

        string sp = File.ReadAllText("Assets/Scripts/UI/SachPhep.cs");
        bool goiDoc = Regex.IsMatch(sp, @"extern\s+string\s+CD_DocChuoi") && sp.Contains("CD_DocChuoi(khoa)");
        bool goiGhi = Regex.IsMatch(sp, @"extern\s+int\s+CD_GhiChuoi") && sp.Contains("CD_GhiChuoi(khoa, giaTri)");
        bool trongWebGL = Regex.IsMatch(sp, @"#if UNITY_WEBGL && !UNITY_EDITOR\s*\[DllImport\(""__Internal""\)\] static extern string CD_DocChuoi");
        Ghi("A3. SachPhep.cs: khai bao + goi CD_DocChuoi " + (goiDoc ? "co" : "KHONG") + ", CD_GhiChuoi " + (goiGhi ? "co" : "KHONG")
            + ", chi tren WebGL " + (trongWebGL ? "co" : "KHONG"));
        Kiem(goiDoc && goiGhi && trongWebGL, "SachPhep khong luu bang localStorage tren WebGL");
    }

    // =============================================================
    // B. TRONG PLAY
    // =============================================================

    static IEnumerator Chup(string ten)
    {
        string d = "PlayTestShots/" + ten + ".png";
        if (File.Exists(d)) File.Delete(d);
        ScreenCapture.CaptureScreenshot(d);
        for (int i = 0; i < 60 && !File.Exists(d); i++) yield return new WaitForEndOfFrame();
    }

    static IEnumerator ChupTex(System.Action<Texture2D> nhan)
    {
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        nhan(ScreenCapture.CaptureScreenshotAsTexture());
    }

    /// <summary>Do sang trung binh mot khung (toa do GUI, y tu tren xuong).</summary>
    static float DoSang(Texture2D tex, Rect r)
    {
        int x0 = Mathf.Max(0, (int)r.xMin), x1 = Mathf.Min(tex.width, (int)r.xMax);
        int y0 = Mathf.Max(0, (int)r.yMin), y1 = Mathf.Min(tex.height, (int)r.yMax);
        double tong = 0; int dem = 0;
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
            {
                var c = tex.GetPixel(x, tex.height - 1 - y);
                tong += 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b; dem++;
            }
        return dem > 0 ? (float)(tong / dem) : 0f;
    }

    /// <summary>
    /// Do sang VANH hinh (tu 62% ra 92% kich thuoc) - NE o khoa nam giua hinh. Lan do dau
    /// do phan giua 60%: o khoa sang cua ban doi chung lam hinh toi (Sam set, Binh mau) ra
    /// x1,04 du anh chup cho thay ro ban xem truoc du mau, ban doi chung xam co khoa.
    /// </summary>
    static float DoSangVanhHinh(Texture2D tex, Rect r)
    {
        var ngoai = Giua(r, 0.92f); var trong = Giua(r, 0.62f);
        double tong = 0; int dem = 0;
        for (int y = (int)ngoai.yMin; y < (int)ngoai.yMax; y++)
            for (int x = (int)ngoai.xMin; x < (int)ngoai.xMax; x++)
            {
                if (trong.Contains(new Vector2(x + 0.5f, y + 0.5f))) continue;
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                var c = tex.GetPixel(x, tex.height - 1 - y);
                tong += 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b; dem++;
            }
        return dem > 0 ? (float)(tong / dem) : 0f;
    }

    static Rect Giua(Rect r, float k)
    {
        return new Rect(r.center.x - r.width * k * 0.5f, r.center.y - r.height * k * 0.5f, r.width * k, r.height * k);
    }

    /// <summary>O hinh ben trai mot HANG da biet (dung cho hang lay tu CuaSoSachPhep.VungHangKyNang - da tinh cuon va nhom).</summary>
    static Rect OHinhCuaHang(Rect hang, float s)
    {
        float kt = hang.height - 10f * s;
        return new Rect(hang.x + 10f * s, hang.y + 5f * s, kt, kt);
    }

    static IEnumerator VaoSanh()
    {
        var dn = Object.FindAnyObjectByType<ManDangNhap>();
        if (dn != null && dn.daVao != null) dn.daVao();
        yield return new WaitForSecondsRealtime(1.0f);
    }

    static IEnumerator KichBan()
    {
        yield return new WaitForSecondsRealtime(0.7f);
        bool ok = false; string e = null;
        yield return FirebaseMang.DangNhap(ThongTinChayThu.EmailB, ThongTinChayThu.MatKhau, (o, err) => { ok = o; e = err; });
        if (ok) yield return HoSoMang.TaiHoacTao(null, (o, err) => { ok = o; e = err; });
        if (!ok) { Ghi("[LOI] dang nhap: " + e); loi++; Ket(); yield break; }
        yield return VaoSanh();
        yield return new WaitForSecondsRealtime(1.2f);

        var sanh = Object.FindAnyObjectByType<ManSanh>();
        if (sanh == null) { Ghi("[LOI] khong co sanh"); loi++; Ket(); yield break; }
        float s = Screen.height / 1080f;
        Ghi("");
        Ghi("B. man Game " + Screen.width + "x" + Screen.height + ", ti le sach " + s.ToString("F3")
            + "; nut CAI DAT " + ManSanh.ViTriNutCaiDat() + ", KY NANG " + ManSanh.ViTriNutKyNang());
        yield return Chup("sachphep_sanh_1_sanh");

        // Tran truoc de lai cap do: xoa sach -> moi ky nang con khoa (dieu kien cho doi chung)
        CapDo.BatDauTranMoi();
        SachPhep.DatLai();
        var icon = new Texture2D[] { IconKyNang.Lua(), IconKyNang.Bang(), IconKyNang.Set(), IconKyNang.Loc(),
            IconKyNang.ThienThach(), IconKyNang.Khieng(), IconKyNang.GiatSet(), IconKyNang.BinhMau(), IconKyNang.BinhMana() };
        var tam = GameObject.Find("TAM_ThuSachPhepSanh").GetComponent<ChayThuMang>();
        var b = CuaSoSachPhep.TinhBoCuc(Screen.width, Screen.height, s);

        // ---- B1. xem truoc vs doi chung ----
        sanh.MoSachPhep();
        CuaSoSachPhep.ChonKyNang(4);          // hang 0..3 va 5..8 khong bi to sang "dang chon"
        yield return new WaitForSecondsRealtime(0.4f);
        int soMoXemTruoc = 0;
        for (int i = 0; i < SachPhep.SoKyNang; i++) if (CuaSoSachPhep.HienDaMo(i)) soMoXemTruoc++;
        bool xemTruoc = CuaSoSachPhep.XemTruoc;
        Texture2D texXem = null;
        yield return ChupTex(t => texXem = t);
        yield return Chup("sachphep_sanh_2_xemtruoc");

        // Doi chung: CUNG cua so, che do trong tran, ve bang phep thu (man chinh khong co GameHUD)
        CuaSoSachPhep.Mo();
        CuaSoSachPhep.ChonKyNang(4);
        tam.veGUI = () => CuaSoSachPhep.Ve(s, null, icon);
        yield return new WaitForSecondsRealtime(0.4f);
        int soMoDoiChung = 0;
        for (int i = 0; i < SachPhep.SoKyNang; i++) if (CuaSoSachPhep.HienDaMo(i)) soMoDoiChung++;
        Texture2D texKhoa = null;
        yield return ChupTex(t => texKhoa = t);
        yield return Chup("sachphep_sanh_2b_doichung_khoa");
        tam.veGUI = null;
        CuaSoSachPhep.Dong();

        var sb = new StringBuilder();
        float tiLeMin = 99f;
        int soHangThay = 0;
        for (int i = 0; i < SachPhep.SoKyNang; i++)
        {
            if (i == 4) continue;
            // ⚠️ PHAI hoi CuaSoSachPhep vi tri hang that: cot xep theo NHOM HE nen moi nhom co mot
            // dong tieu de, "chi so x chieu cao hang" tro sang cho khac (CLAUDE.md da ghi cai bay nay).
            // Ban cu do trung vao khoang trong giua cac nhom va cho ra ti le lung tung (19/09/2026).
            var hang = CuaSoSachPhep.VungHangKyNang(b, i, s);
            if (hang.height <= 1f) continue;
            var r = OHinhCuaHang(hang, s);
            if (r.yMax > b.kho.yMax || r.yMin < b.kho.yMin) continue;   // hang bi khuat (phai cuon)
            float a = DoSangVanhHinh(texXem, r), k = DoSangVanhHinh(texKhoa, r);
            float tl = a / Mathf.Max(0.001f, k);
            tiLeMin = Mathf.Min(tiLeMin, tl); soHangThay++;
            sb.AppendFormat("{0}:{1:F3}/{2:F3}(x{3:F2}) ", i, a, k, tl);
        }
        float oMin = 99f;
        var sbO = new StringBuilder();
        for (int o = 0; o < SachPhep.SoOVuong; o++)
        {
            if (SachPhep.OVuong[o] == 4 || SachPhep.OVuong[o] == SachPhep.Trong) continue;
            var r = CuaSoSachPhep.OTaiVung(b.vungO, o, s);
            float a = DoSangVanhHinh(texXem, r), k = DoSangVanhHinh(texKhoa, r);
            oMin = Mathf.Min(oMin, a / Mathf.Max(0.001f, k));
            sbO.AppendFormat("o{0}:x{1:F2} ", o + 1, a / Mathf.Max(0.001f, k));
        }
        Object.Destroy(texXem); Object.Destroy(texKhoa);
        Ghi("B1. xem truoc: XemTruoc = " + xemTruoc + ", ky nang hien da mo / keo duoc " + soMoXemTruoc + "/" + SachPhep.SoKyNang + "; doi chung trong tran (con khoa) "
            + soMoDoiChung + "/" + SachPhep.SoKyNang);
        Ghi("    do sang VANH hinh cot trai (xem truoc / doi chung khoa; ne o khoa giua hinh): " + sb.ToString().Trim() + " -> thap nhat x" + tiLeMin.ToString("F2")
            + " tren " + soHangThay + " hang");
        Ghi("    do sang hinh trong o: " + sbO.ToString().Trim() + " -> thap nhat x" + oMin.ToString("F2"));
        Kiem(xemTruoc, "nut KY NANG khong mo che do xem truoc");
        Kiem(soMoXemTruoc == SachPhep.SoKyNang, "xem truoc van con ky nang bi khoa");
        // Dau tran chi hai ky nang BINH co san (nguoi dung 25/09/2026) - moi thu khac con khoa
        int soBinh = 0; for (int i = 0; i < SachPhep.SoKyNang; i++) if (CapDo.LaKyBinh(i)) soBinh++;
        Kiem(soMoDoiChung == soBinh, "doi chung hong: trong tran luc moi vao so ky nang da mo khac so ky nang binh (" + soBinh + ")");
        Kiem(soHangThay >= 6 && tiLeMin >= 1.5f, "hinh ky nang o sanh chua sang du mau (van xam / co o khoa?)");
        Kiem(oMin >= 1.5f, "hinh trong o o sanh chua sang du mau (van xam / co o khoa?)");

        // ---- B1b. MOI KY NANG DEU CO BIEU TUONG (ke ca nhom cuoi danh sach) ----
        //
        // ⚠️ Muc B1 o tren bo qua moi hang bi khuat duoi day cot ("phai cuon"), ma bon ky nang BI DONG
        // nam CUOI danh sach - nen 19/09/2026 chung hien ra bon o TRONG TRON o sanh (khong co ca cai
        // dia nut) ma phep thu van bao xanh; nguoi dung phai tu nhin thay. Nguyen nhan: ManSanh giu
        // mot BAN RIENG cua bo bieu tuong, chi dai 16, khong ai sua khi them ky nang thu 17..20.
        //
        // Do hai lop: bang bieu tuong (re, chac), va anh THAT tren man hinh sau khi cuon het cot.
        var boIcon = IconKyNang.BoDayDu();
        int soTrong = 0;
        var thieu = new StringBuilder();
        for (int i = 0; i < boIcon.Length; i++)
            if (boIcon[i] == null) { soTrong++; thieu.Append(i).Append(' '); }
        Ghi("B1b. bang bieu tuong dung chung: " + boIcon.Length + " o (so ky nang " + CapDo.SoKyNang + "), o rong: "
            + (soTrong == 0 ? "khong co" : thieu.ToString().Trim()));
        Kiem(boIcon.Length == CapDo.SoKyNang, "bang bieu tuong khong du so ky nang");
        Kiem(soTrong == 0, "co ky nang khong co bieu tuong trong bang");

        // Cuon HET cot trai roi do anh that cua bon hang cuoi
        sanh.MoSachPhep();
        CuaSoSachPhep.ChonKyNang(5);
        CuaSoSachPhep.CuonKho = 99999f;             // bi kep lai o lan ve sau -> xuong day cot
        yield return new WaitForSecondsRealtime(0.4f);
        Texture2D texCuoi = null;
        yield return ChupTex(t => texCuoi = t);
        yield return Chup("sachphep_sanh_2c_nhom_bi_dong");

        var bCuoi = CuaSoSachPhep.TinhBoCuc(Screen.width, Screen.height, s);
        // Moc so sanh: cac ky nang KHAC dang nhin thay o cung khung hinh nay
        float sangMoc = 0f; int demMoc = 0;
        for (int ky = 0; ky < CapDo.SoKyNang; ky++)
        {
            if (CapDo.LaKyBiDong(ky)) continue;
            var rr = CuaSoSachPhep.VungHangKyNang(bCuoi, ky, s);
            if (rr.height <= 1f || rr.yMin < bCuoi.kho.yMin || rr.yMax > bCuoi.kho.yMax) continue;
            sangMoc += DoSangVanhHinh(texCuoi, OHinhCuaHang(rr, s)); demMoc++;
        }
        sangMoc = demMoc > 0 ? sangMoc / demMoc : 0f;

        var sbBd = new StringBuilder();
        float thapNhat = 99f; int soDo = 0;
        for (int ky = 0; ky < CapDo.SoKyNang; ky++)
        {
            if (!CapDo.LaKyBiDong(ky)) continue;
            var rr = CuaSoSachPhep.VungHangKyNang(bCuoi, ky, s);
            if (rr.height <= 1f || rr.yMin < bCuoi.kho.yMin || rr.yMax > bCuoi.kho.yMax)
            { sbBd.Append(ky).Append(":KHUAT "); continue; }
            float sang = DoSangVanhHinh(texCuoi, OHinhCuaHang(rr, s));
            sbBd.AppendFormat("{0}:{1:F3} ", ky, sang);
            thapNhat = Mathf.Min(thapNhat, sang); soDo++;
        }
        Object.Destroy(texCuoi);
        CuaSoSachPhep.Dong();
        Ghi("B1b2. sau khi cuon het cot, do sang VANH hinh nhom BI DONG: " + sbBd.ToString().Trim()
            + " -> thap nhat " + thapNhat.ToString("F3") + " tren " + soDo + " hang; moc cac ky nang khac cung khung hinh "
            + sangMoc.ToString("F3") + " (" + demMoc + " hang)");
        int soBiDong = SachPhep.KyNangTheoNhom[SachPhep.KyNangTheoNhom.Length - 1].Length;
        Kiem(soDo == soBiDong, "khong do duoc het " + soBiDong + " ky nang bi dong (cuon chua toi day cot?)");
        // ⚠️ NGUONG do bang DOI CHUNG that (19/09/2026): co bieu tuong 0,288-0,352 (x1,27-1,55 moc);
        // thao bieu tuong ra 0,115-0,127 (x0,51-0,56 moc). Nguong dau tien toi dat la x0,5 - va ban
        // THIEU BIEU TUONG van lot qua voi 0,115 > 0,1135. Nay lay x0,85, nam giua hai cum.
        Kiem(demMoc >= 2 && thapNhat > sangMoc * 0.85f,
             "bieu tuong ky nang bi dong qua toi so voi cac ky nang khac - o trong?");

        // ---- B2. nhan vat mau ----
        var pcMau = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player_Sorceress.prefab");
        var pcPrefab = prefab != null ? prefab.GetComponent<PlayerController>() : null;
        Ghi(string.Format("B2. nhan vat mau o man chinh: {0}; cau lua nang luong {1} / hoi {2} (prefab {3} / {4}); thien thach nang luong {5} (prefab {6})",
            pcMau != null ? pcMau.name : "KHONG CO",
            pcMau != null ? pcMau.fireballCost : -1, pcMau != null ? pcMau.fireballCooldown : -1,
            pcPrefab != null ? pcPrefab.fireballCost : -1, pcPrefab != null ? pcPrefab.fireballCooldown : -1,
            pcMau != null ? pcMau.meteorCost : -1, pcPrefab != null ? pcPrefab.meteorCost : -1));
        Kiem(pcMau != null && pcPrefab != null && pcMau.fireballCost > 0f
             && Mathf.Approximately(pcMau.fireballCost, pcPrefab.fireballCost)
             && Mathf.Approximately(pcMau.meteorCost, pcPrefab.meteorCost),
             "o sanh khong doc duoc thong so ky nang that");

        // ---- B3. sap o o sanh, doc kho luu ----
        sanh.MoSachPhep();
        yield return null;
        SachPhep.DatVaoO(0, 8);
        SachPhep.DatVaoO(1, 7);
        SachPhep.DoiHaiO(2, 3);
        string khoVuong = PlayerPrefs.GetString(KhoaVuong, "");
        // bo cam ung: doi qua CamUng.EpBat (man chinh khong co GameHUD ghi de moi khung)
        CamUng.EpBat = true;
        yield return null;
        bool camUngBat = CamUng.DangDung;
        SachPhep.DatVaoO(6, 8);
        string khoTron = PlayerPrefs.GetString(KhoaTron, "");
        CamUng.EpBat = false;
        yield return null;
        Ghi("B3. kho luu sau khi sap o sanh: vuong [" + khoVuong + "] (mong [" + MongVuong + "]), tron [" + khoTron
            + "] (mong [" + MongTron + "], cam ung bat " + camUngBat + ")");
        Kiem(khoVuong == MongVuong, "bo o may tinh khong luu dung");
        Kiem(camUngBat && khoTron == MongTron, "bo o cam ung khong luu dung");
        yield return Chup("sachphep_sanh_3_da_sap");

        // ---- B4. dang mo sach ma vao tran ----
        bool moTruocKhiVao = CuaSoSachPhep.DangMo;
        SceneManager.LoadScene("Act2");
        float han = Time.realtimeSinceStartup + 30f;
        while (SceneManager.GetActiveScene().name != "Act2" && Time.realtimeSinceStartup < han) yield return null;
        yield return new WaitForSeconds(2.0f);
        var hud = GameHUD.Ban;
        var sbTran = new StringBuilder();
        for (int o = 0; o < SachPhep.SoOVuong; o++) sbTran.Append(SachPhep.KyNangTaiO(o)).Append(o < 6 ? "," : "");
        Ghi("B4. vao Act2 luc sach dang mo (" + moTruocKhiVao + "): sach con mo = " + CuaSoSachPhep.DangMo
            + ", thanh ky nang trong tran [" + sbTran + "]");
        Kiem(moTruocKhiVao && !CuaSoSachPhep.DangMo, "vao tran ma Sach phep xem truoc van phu man hinh");
        Kiem(sbTran.ToString() == MongVuong, "thanh ky nang trong tran khong theo thu tu da xep o sanh");
        yield return Chup("sachphep_sanh_4_act2");

        // ---- B5. con mat quy ----
        if (hud == null) { Ghi("[LOI] khong co GameHUD o Act2"); loi++; }
        else
        {
            hud.epCamUng = true;
            CamUng.KhoaCam = false;
            yield return new WaitForSecondsRealtime(0.6f);
            float sh = Screen.height / 1080f;
            float r = 40f * sh;
            var tamNut = new Vector2(Screen.width - 62f * sh, 62f * sh);     // toa do GUI (y tu tren)
            Texture2D texMo = null, texDong = null;
            yield return ChupTex(t => texMo = t);
            CamUng.KhoaCam = true;
            yield return new WaitForSecondsRealtime(0.3f);
            yield return ChupTex(t => texDong = t);

            int cam = 0, camNgoai = 0, cheoMo = 0, cheoKhoa = 0, soCheo = 0, trong = 0;
            double khac = 0; int soKhac = 0;
            for (int y = (int)(tamNut.y - r); y < (int)(tamNut.y + r); y++)
                for (int x = (int)(tamNut.x - r); x < (int)(tamNut.x + r); x++)
                {
                    if (x < 0 || y < 0 || x >= texMo.width || y >= texMo.height) continue;
                    var a = texMo.GetPixel(x, texMo.height - 1 - y);
                    var k = texDong.GetPixel(x, texDong.height - 1 - y);
                    float dx = x - tamNut.x, dy = y - tamNut.y;
                    khac += Mathf.Abs(a.r - k.r) + Mathf.Abs(a.g - k.g) + Mathf.Abs(a.b - k.b); soKhac++;
                    // Trong mat (ban kinh 0,3r): cam ruc
                    if (dx * dx + dy * dy < (0.3f * r) * (0.3f * r))
                    {
                        trong++;
                        if (a.r > 0.55f && a.g > 0.18f && a.g < 0.85f && a.b < 0.40f && a.r > a.g * 1.3f) cam++;
                    }
                    // Duong cheo tren-trai -> duoi-phai (|dx - dy| nho), bo phan giua trong mat
                    if (Mathf.Abs(dx - dy) < 0.08f * r && Mathf.Abs(dx) > 0.30f * r && Mathf.Abs(dx) < 0.60f * r)
                    {
                        soCheo++;
                        if (a.r > 0.55f && a.r > a.g * 2.2f && a.r > a.b * 2.2f) cheoMo++;
                        if (k.r > 0.55f && k.r > k.g * 2.2f && k.r > k.b * 2.2f) cheoKhoa++;
                    }
                }
            // Doi chung: cung khung vuong nhung dich sang trai 3r (ngoai nut, canh tran dau) - khong duoc co mau cam
            int trongNgoai = 0;
            for (int y = (int)(tamNut.y - 0.3f * r); y < (int)(tamNut.y + 0.3f * r); y++)
                for (int x = (int)(tamNut.x - 3.3f * r); x < (int)(tamNut.x - 2.7f * r); x++)
                {
                    var a = texMo.GetPixel(x, texMo.height - 1 - y); trongNgoai++;
                    if (a.r > 0.55f && a.g > 0.18f && a.g < 0.85f && a.b < 0.40f && a.r > a.g * 1.3f) camNgoai++;
                }
            float tlCam = cam / (float)Mathf.Max(1, trong), tlCamNgoai = camNgoai / (float)Mathf.Max(1, trongNgoai);
            float tlCheoMo = cheoMo / (float)Mathf.Max(1, soCheo), tlCheoKhoa = cheoKhoa / (float)Mathf.Max(1, soCheo);
            float khacTB = (float)(khac / Mathf.Max(1, soKhac));
            Ghi(string.Format("B5. con mat (ban kinh nut {0:F1} diem): trong mat cam ruc {1:P0} (doi chung ngoai nut {2:P0}); duong cheo do: mo {3:P0} -> khoa {4:P0} ({5} diem); hai trang thai khac nhau trung binh {6:F3}/kenh",
                r, tlCam, tlCamNgoai, tlCheoMo, tlCheoKhoa, soCheo, khacTB / 3f));
            Kiem(tlCam >= 0.35f && tlCamNgoai < 0.10f, "con mat mo khong co trong mat cam");
            Kiem(tlCheoKhoa >= tlCheoMo + 0.35f, "dang khoa khong thay vet chem do cheo");
            Kiem(khacTB / 3f > 0.03f, "hai trang thai con mat trong nhu nhau");

            // Anh phong to vung nut de xem bang mat
            foreach (var cap in new[] { new { t = texMo, ten = "sachphep_sanh_5_mat_mo" }, new { t = texDong, ten = "sachphep_sanh_6_mat_khoa" } })
            {
                int co = Mathf.CeilToInt(r * 2.4f);
                int x0 = Mathf.Clamp((int)(tamNut.x - co * 0.5f), 0, cap.t.width - co);
                int y0 = Mathf.Clamp(cap.t.height - (int)(tamNut.y + co * 0.5f), 0, cap.t.height - co);
                var cat = new Texture2D(co * 4, co * 4, TextureFormat.RGB24, false);
                for (int y = 0; y < co * 4; y++)
                    for (int x = 0; x < co * 4; x++)
                        cat.SetPixel(x, y, cap.t.GetPixel(x0 + x / 4, y0 + y / 4));
                File.WriteAllBytes("PlayTestShots/" + cap.ten + ".png", cat.EncodeToPNG());
                Object.Destroy(cat);
            }
            Object.Destroy(texMo); Object.Destroy(texDong);
            CamUng.KhoaCam = false;
            hud.epCamUng = false;
        }

        Ket();
    }

    static void Ket()
    {
        CuaSoSachPhep.Dong();
        FirebaseMang.Quen();
        var rac = GameObject.Find("TAM_ThuSachPhepSanh");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLai;
    }

    static void TraLai()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLai;

        // ---- Nhu mo game lan sau: quen bo nho, doc lai tu kho luu ----
        SachPhep.NapLai();
        string vuong = string.Join(",", System.Array.ConvertAll(SachPhep.OVuong, x => x.ToString()));
        string tron = string.Join(",", System.Array.ConvertAll(SachPhep.OTron, x => x.ToString()));
        Ghi("C. sau khi thoat Play, nap lai tu kho luu: vuong [" + vuong + "], tron [" + tron + "]");
        Kiem(vuong == MongVuong && tron == MongTron, "thoat game mo lai thi mat thu tu o");

        if (coPhienGoc) PlayerPrefs.SetString(KhoaPhien, phienGoc); else PlayerPrefs.DeleteKey(KhoaPhien);
        if (coTronGoc) PlayerPrefs.SetString(KhoaTron, tronGoc); else PlayerPrefs.DeleteKey(KhoaTron);
        if (coVuongGoc) PlayerPrefs.SetString(KhoaVuong, vuongGoc); else PlayerPrefs.DeleteKey(KhoaVuong);
        PlayerPrefs.Save();
        SachPhep.NapLai();

        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        if (!string.IsNullOrEmpty(canhCu) && EditorSceneManager.GetActiveScene().path != canhCu)
            EditorSceneManager.OpenScene(canhCu);

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        File.WriteAllText("PlayTestShots/sachphep_sanh.txt", bao.ToString());
        Debug.Log("[SachPhepSanh] da tra lai: phien, thu tu o; scene = " + EditorSceneManager.GetActiveScene().path
                  + ", isDirty = " + EditorSceneManager.GetActiveScene().isDirty
                  + ", TAM_ con lai = " + (GameObject.Find("TAM_ThuSachPhepSanh") != null));
    }
}
