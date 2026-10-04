using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MENU 103 - CHU "ĐƠN" BI CAT THANH "Đ…" O NUT CHE DO (nguoi dung 05/10/2026, anh sanh tren dien thoai). Nguyen nhan: GiaoDien.VuaO tru
/// phan dem cua nut HAI LAN (noi goi tru khoi cho trong, CalcSize lai cong vao). Do: mo mot cua so Editor tam o tung CO MAN HINH (Screen
/// trong OnGUI = co cua so), goi GiaoDien.ChuanBi (ti le giao dien theo co ay) roi ve DUNG hai nut ĐƠN / ĐÔI bang GiaoDien.Nut, khung =
/// ManSanh.NutCheDo, ca hai kieu (dang chon / khong chon). Dem GiaoDien.SoLanCat. DOI CHUNG: GiaoDien.DoiChungTruDemHaiLan (ban cu).
/// Ket qua: PlayTestShots/chu_vua_nut.txt.
/// </summary>
public class ThuChuVuaNut : EditorWindow
{
    static readonly Vector2[] CoManHinh = { new Vector2(1616, 587), new Vector2(1619, 588), new Vector2(844, 390), new Vector2(740, 360),
                                            new Vector2(1280, 720), new Vector2(1920, 1080), new Vector2(2400, 1080), new Vector2(2532, 1170) };
    int buoc = -1, khung;
    readonly StringBuilder bao = new StringBuilder();
    int loi;
    readonly int[] catMoi = new int[2];
    readonly string[] chuCat = new string[2];

    [MenuItem("Diablo 2.5D/103 Chu vua nut che do DON - DOI o moi co man hinh", false, 171)]
    public static void Chay()
    {
        var w = CreateInstance<ThuChuVuaNut>();
        w.titleContent = new GUIContent("TAM_ThuChuVuaNut");
        w.ShowUtility();
        w.buoc = 0; w.khung = 0;
        w.DatCo();
    }

    void DatCo()
    {
        float ppp = EditorGUIUtility.pixelsPerPoint;
        var c = CoManHinh[buoc];
        position = new Rect(40, 40, c.x / ppp, c.y / ppp);
        khung = 0;
    }

    void OnGUI()
    {
        if (buoc < 0 || buoc >= CoManHinh.Length) return;
        GiaoDien.ChuanBi();
        float s = GiaoDien.TiLe;
        var kt = new Rect(0f, 0f, 1000f * s, 200f * s);
        var nDon = ManSanh.NutCheDo(kt, s, 0);
        var nDoi = ManSanh.NutCheDo(kt, s, 1);
        for (int ban = 0; ban < 2; ban++)
        {
            GiaoDien.DoiChungTruDemHaiLan = ban == 1;
            int truoc = GiaoDien.SoLanCat;
            GiaoDien.ChuBiCatCuoi = "";
            // ca hai kieu nut: dang chon (do, chu to hon) va khong chon
            GiaoDien.Nut(nDon, "ĐƠN", GiaoDien.KieuNutMau);
            GiaoDien.Nut(nDoi, "ĐÔI", GiaoDien.KieuNutDa);
            GiaoDien.Nut(nDon, "ĐƠN", GiaoDien.KieuNutDa);
            GiaoDien.Nut(nDoi, "ĐÔI", GiaoDien.KieuNutMau);
            if (Event.current.type == EventType.Repaint) { catMoi[ban] = GiaoDien.SoLanCat - truoc; chuCat[ban] = GiaoDien.ChuBiCatCuoi; }
        }
        GiaoDien.DoiChungTruDemHaiLan = false;
        if (Event.current.type != EventType.Repaint) return;
        khung++;
        if (khung < 3) { Repaint(); return; }          // de cua so nhan co moi
        bao.AppendLine(string.Format("{0}x{1} (Screen {2}x{3}, ti le {4:F2}, nut {5:F1} px, chu mau {6} / da {7}): BAN SUA cat {8} lan{9} | DOI CHUNG ban cu cat {10} lan{11}",
            CoManHinh[buoc].x, CoManHinh[buoc].y, Screen.width, Screen.height, s, nDon.width, GiaoDien.KieuNutMau.fontSize, GiaoDien.KieuNutDa.fontSize,
            catMoi[0], catMoi[0] > 0 ? " (\"" + chuCat[0] + "\")" : "", catMoi[1], catMoi[1] > 0 ? " (\"" + chuCat[1] + "\")" : ""));
        if (catMoi[0] > 0) loi++;
        buoc++;
        if (buoc < CoManHinh.Length) { DatCo(); Repaint(); return; }
        bao.AppendLine("so loi ghi nhan = " + loi);
        Directory.CreateDirectory("PlayTestShots");
        File.WriteAllText("PlayTestShots/chu_vua_nut.txt", bao.ToString());
        buoc = -1;
        EditorApplication.delayCall += Close;
    }
}
