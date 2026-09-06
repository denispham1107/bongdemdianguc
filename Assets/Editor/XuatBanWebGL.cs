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
/// NEN: Gzip + header Content-Encoding tren Firebase Hosting. Bat kem
/// decompressionFallback lam bao hiem: neu header hosting sai thi Unity tu giai
/// nen bang JavaScript, cham hon nhung van chay - hon la trang trang khong mot
/// loi nao.
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

        // Gzip + fallback. Xem ghi chu dau file ve ly do bat fallback.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

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
        bao.AppendLine("[ban 1] xuat ban WebGL");
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

        File.WriteAllText("PlayTestShots/build_webgl.txt", bao.ToString());
        Debug.Log("[BuildWebGL]\n" + bao);
    }
}
