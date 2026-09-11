using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// XUAT BAN BAN WEBGL DE CHOI TREN TRINH DUYET.
///
/// Ban WebGL la thu duy nhat trinh duyet chay duoc: ma nguon C# duoc dich sang
/// WebAssembly. Day GitHub chi la luu ma nguon - khong ai "vao link GitHub"
/// ma choi duoc.
///
/// NEN: KHONG nen san - de Firebase Hosting tu nen luc gui (gzip/brotli, tuy
/// trinh duyet), trinh duyet giai nen bang ma may.
///
/// Truoc 11/09/2026 build nen Gzip san + decompressionFallback, voi y dinh dat
/// header Content-Encoding tren Firebase. Nhung Firebase GAT BO header
/// Content-Encoding tu dat (da thu voi .unityweb: quy tac co trong cau hinh ma
/// phan hoi khong co), nen Unity giai nen 166 MB bang JavaScript MOI LAN vao
/// game. Do tren trang that, tai am (khong di mang): nen san 14,3 s, khong nen
/// san 10,6 s. Tai lan dau chi nang hon 0,6 % (Firebase nen wasm bang brotli,
/// con .data thi hau nhu khong nen duoc vi texture da nen san).
/// firebase.json dat Content-Type cho *.data - khong thi Firebase goi no la
/// text/html.
///
/// Doi sang may chu khac thi xem lai: may chu khong tu nen se bat nguoi choi
/// tai 198 MB thay vi 172 MB.
///
/// Ket qua ra <c>Build/WebGL</c>, bao ghi ra <c>PlayTestShots/build_webgl.txt</c>.
/// Buoc copy sang <c>web/</c> lam o ngoai, khong lam o day: build va trien khai
/// la hai viec, tron vao nhau thi build hong cung xoa mat ban dang chay.
/// </summary>
public static class XuatBanWebGL
{
    const string ThuMucRa = "Build/WebGL";

    [MenuItem("Diablo 2.5D/29. Xuat ban WEBGL", false, 115)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        var canh = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Act1.unity",
            "Assets/Scenes/Act2.unity",
        };

        foreach (var c in canh)
            if (!File.Exists(c))
            {
                Debug.LogError("[BuildWebGL] thieu scene " + c);
                return;
            }

        // Khong nen san, Firebase tu nen. Xem ghi chu dau file.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;

        // TEN FILE = MA BAM NOI DUNG. Ban khong nen chi dua .data qua Cache
        // Storage cua Unity; framework (the <script>) va wasm (framework tu tai)
        // di qua bo dem HTTP cua trinh duyet. De bo dem do giu duoc thi
        // Build/** phai "immutable" - chi an toan khi ten file doi theo noi
        // dung. Ten co dinh + immutable chinh la loi sap luc tai truoc day.
        PlayerSettings.WebGL.nameFilesAsHashes = true;

        // Xoa file cua lan build truoc: doi kieu nen thi doi ca ten file
        // (.unityweb -> khong duoi), file cu nam lai se bi chep nham len web.
        string thuMucBuildCu = Path.Combine(ThuMucRa, "Build");
        if (Directory.Exists(thuMucBuildCu)) Directory.Delete(thuMucBuildCu, true);

        // Cat bot ma engine khong dung - ban WebGL cang nho cang de tai tren
        // dien thoai, ma dien thoai moi la nen tang chinh cua game nay.
        PlayerSettings.stripEngineCode = true;

        // Khong mo trinh duyet ngay: may nay dang chay qua cong cu, mo cua so
        // len chi lam vuong.
        var tuyChon = new BuildPlayerOptions
        {
            scenes = canh,
            locationPathName = ThuMucRa,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        Debug.Log("[BuildWebGL] bat dau build - viec nay lau, dung dong Unity");
        var t0 = DateTime.Now;

        BuildReport bc = BuildPipeline.BuildPlayer(tuyChon);

        double phut = (DateTime.Now - t0).TotalMinutes;
        var tt = bc.summary;

        var bao = new System.Text.StringBuilder();
        bao.AppendLine("[ban 4] xuat ban WebGL (khong nen san, ten file = ma bam)");
        bao.AppendLine("ket qua: " + tt.result);
        bao.AppendLine(string.Format("thoi gian: {0:F1} phut", phut));
        bao.AppendLine(string.Format("tong dung luong: {0:F1} MB", tt.totalSize / 1048576.0));
        bao.AppendLine("so loi: " + tt.totalErrors + " | so canh bao: " + tt.totalWarnings);
        bao.AppendLine("thu muc ra: " + Path.GetFullPath(ThuMucRa));

        if (tt.result == BuildResult.Succeeded && Directory.Exists(ThuMucRa))
        {
            bao.AppendLine("--- file trong Build/ ---");
            string thuMucBuild = Path.Combine(ThuMucRa, "Build");
            if (Directory.Exists(thuMucBuild))
                foreach (var f in Directory.GetFiles(thuMucBuild))
                    bao.AppendLine(string.Format("  {0}  {1:F1} MB",
                        Path.GetFileName(f), new FileInfo(f).Length / 1048576.0));
        }

        if (tt.result == BuildResult.Succeeded)
            bao.AppendLine("gan phien ban vao index.html: " + GanPhienBanChoTrang(ThuMucRa));

        File.WriteAllText("PlayTestShots/build_webgl.txt", bao.ToString());
        Debug.Log("[BuildWebGL]\n" + bao);
    }

    /// <summary>
    /// GAN MA PHIEN BAN VAO DUONG DAN CAC FILE MA GAME trong index.html.
    ///
    /// Ten file khong doi giua cac ban (WebGL.wasm.unityweb...), ma truoc day
    /// firebase.json cho Build/** header "max-age=31536000, immutable": trinh
    /// duyet da vao trang giu ban wasm CU mot nam va khong hoi lai. Ban moi
    /// len thi du lieu (.data) moi ghep voi ma (.wasm) cu - va game sap ngay
    /// luc tai ("memory access out of bounds", "Maximum call stack size
    /// exceeded"). Do duoc 11/09/2026: trinh duyet dung wasm 5 472 605 byte
    /// (transferSize = 0, lay tu cache) trong khi may chu co 5 482 538.
    ///
    /// Them "?v=ma" (lay tu noi dung file) thi moi ban mot duong dan moi -
    /// cache cu khong con dung duoc. index.html thi luon duoc hoi lai
    /// (no-cache).
    ///
    /// File .data CUNG gan (tu 11/09/2026). Truoc do de tran vi tuong "Unity tu
    /// hoi lai, may chu tra 304" - sai: Firebase khong bao gio tra 304 cho file
    /// no-cache, nen moi lan vao trang tai lai 166 MB. Gio index.html bao Unity
    /// coi file co ?v= la "immutable" (lay thang tu cache, khong hoi mang), va
    /// productVersion mang ma cua .data de Unity tu xoa ban cu luc khoi dong -
    /// khong don 166 MB moi ban trong may nguoi choi. Xem cacheControl trong
    /// Assets/WebGLTemplates/Diablo25D/index.html.
    /// </summary>
    public static string GanPhienBanChoTrang(string thuMuc)
    {
        string trang = Path.Combine(thuMuc, "index.html");
        string build = Path.Combine(thuMuc, "Build");
        if (!File.Exists(trang)) return "khong co index.html";
        if (!Directory.Exists(build)) return "khong co thu muc Build";

        // Lay ten file tu chinh thu muc Build: ten doi theo kieu nen
        // (WebGL.data / WebGL.data.unityweb / WebGL.data.gz...).
        string html = File.ReadAllText(trang);
        var ra = new System.Text.StringBuilder();
        string maDuLieu = null;
        int soFileGan = 0;
        foreach (var duong in Directory.GetFiles(build))
        {
            string t = Path.GetFileName(duong);
            // Chi gan cho file trang that su tro toi
            if (!html.Contains("/" + t + "\"") && !html.Contains("/" + t + "?v="))
            {
                ra.Append(t + " (trang khong dung); ");
                continue;
            }
            string ma;
            // Doc theo luong: .data 166 MB, khong nap ca file vao RAM
            using (var md5 = System.Security.Cryptography.MD5.Create())
            using (var luong = File.OpenRead(duong))
                ma = System.BitConverter.ToString(md5.ComputeHash(luong))
                         .Replace("-", "").Substring(0, 10).ToLowerInvariant();
            // Ten co the la "WebGL.data" hoac "<ma bam>.data"
            if (t.EndsWith(".data") || t.Contains(".data.")) maDuLieu = ma;

            // Bo ma cu (neu chay lai lan nua) roi gan ma moi
            html = System.Text.RegularExpressions.Regex.Replace(html,
                "/" + System.Text.RegularExpressions.Regex.Escape(t) + "(\\?v=[0-9a-f]+)?\"",
                "/" + t + "?v=" + ma + "\"");
            ra.Append(t + "?v=" + ma + "; ");
            soFileGan++;
        }

        // loader + framework + wasm + data. Thieu la co file di tran - chinh
        // loi ghep ma cu voi du lieu moi.
        if (soFileGan != 4) ra.Append("CANH BAO: chi gan duoc " + soFileGan + "/4 file; ");

        // productVersion = "<bundleVersion>+<ma .data>": UnityCache xoa moi muc
        // co productVersion khac ngay luc khoi dong, truoc khi tai ban moi.
        if (maDuLieu != null)
        {
            html = System.Text.RegularExpressions.Regex.Replace(html,
                "(productVersion:\\s*\")([^\"+]*)(\\+[0-9a-f]+)?\"",
                "${1}${2}+" + maDuLieu + "\"");
            ra.Append("productVersion +" + maDuLieu + "; ");
        }

        File.WriteAllText(trang, html);
        return ra.ToString();
    }
}
