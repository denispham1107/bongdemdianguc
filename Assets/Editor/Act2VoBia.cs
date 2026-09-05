using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// SUT ME THAT MOT PHAN BIA MO ACT2 - o muc HINH HOC, khong phai ve len anh.
///
/// Vet vo tren anh chi lam be mat lom xuong; nhin nghieng thi duong vien cua
/// tam bia van thang bang tap. Muon "bi vo mot vai cho" that thi phai cat vao
/// chinh cai luoi.
///
/// ===================================================================
///  CAT BANG MAT PHANG, KHONG PHAI DAY TUNG DINH
/// ===================================================================
///
/// Luoi bia rat thua - <c>TS_gothic</c> chi 350 dinh, tuc khoang cach hai dinh
/// ke nhau da vai centimet. Day tung dinh vao trong theo mot qua cau thi vet
/// lom ra lom nham nhu bi bop, khong ra vet me.
///
/// Cat bang MAT PHANG thi khac: moi dinh nam ngoai mat phang duoc chieu vuong
/// goc len chinh no, nen cho cat thanh mot mat VAT phang - dung nhu da vo theo
/// thoi. Cong them mot chut nhieu theo phap tuyen cho mep khoi thang bang thuoc.
///
/// ===================================================================
///  BIEN THE DUNG CHUNG, KHONG PHAI MOI BIA MOT LUOI
/// ===================================================================
///
/// 458 tam bia ma moi tam mot luoi rieng thi mat sach kha nang gop lenh ve.
/// Nen chi dung <see cref="SoBienThe"/> ban vo cho MOI LOAI luoi, roi chia nhau
/// dung: cac bia cung mot bien the van gop duoc voi nhau.
///
/// Chon bia nao bi vo la TAT DINH - bam tu ten vat the - nen chay lai bao nhieu
/// lan cung ra dung mot nghia dia.
/// </summary>
public static class Act2VoBia
{
    /// <summary>Bao nhieu phan tram bia bi sut me.</summary>
    public const float TiLeVo = 0.28f;

    /// <summary>Moi loai luoi dung bao nhieu ban vo khac nhau.</summary>
    public const int SoBienThe = 3;

    const string ThuMuc = "Assets/Meshes/Act2Vo";

    /// <summary>Chi cat bia mo va cot da; tang da von da lom nhom san.</summary>
    static readonly string[] DauTen = { "TS_" };

    // ================================================================

    static float Bam01(int hat)
    {
        unchecked
        {
            int n = hat * 1664525 + 1013904223;
            n = (n ^ (n >> 13)) * 1274126177;
            n = n ^ (n >> 16);
            return (n & 0x7fffffff) / 2147483647f;
        }
    }

    static int BamTen(string s)
    {
        unchecked
        {
            int h = 17;
            for (int i = 0; i < s.Length; i++) h = h * 31 + s[i];
            return h;
        }
    }

    [MenuItem("Diablo 2.5D/17. Sut me mot phan bia mo - Act2", false, 98)]
    public static void Chay()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Act2")
            sc = EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", OpenSceneMode.Single);

        if (!AssetDatabase.IsValidFolder("Assets/Meshes")) AssetDatabase.CreateFolder("Assets", "Meshes");
        if (!AssetDatabase.IsValidFolder(ThuMuc)) AssetDatabase.CreateFolder("Assets/Meshes", "Act2Vo");

        // Ban vo da dung, theo khoa "tenLuoi#bienThe"
        var kho = new Dictionary<string, Mesh>();
        int daVo = 0, daXet = 0;

        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var g = mf.gameObject;
            if (!HopLe(g.name)) continue;
            var goc = mf.sharedMesh;
            if (goc == null) continue;
            // Da la ban vo roi thi thoi - chay lai lan hai khong cat chong len
            if (goc.name.EndsWith("_vo0") || goc.name.EndsWith("_vo1") || goc.name.EndsWith("_vo2")) { daVo++; continue; }

            daXet++;
            int hat = BamTen(g.name);
            if (Bam01(hat) > TiLeVo) continue;

            int bienThe = Mathf.Clamp(Mathf.FloorToInt(Bam01(hat + 7717) * SoBienThe), 0, SoBienThe - 1);
            string khoa = goc.name + "#" + bienThe;

            Mesh vo;
            if (!kho.TryGetValue(khoa, out vo))
            {
                vo = DungBanVo(goc, bienThe);
                string duong = string.Format("{0}/{1}_vo{2}.asset", ThuMuc, goc.name, bienThe);
                AssetDatabase.CreateAsset(vo, duong);
                kho[khoa] = vo;
            }

            Undo.RecordObject(mf, "sut me bia mo");
            mf.sharedMesh = vo;
            EditorUtility.SetDirty(mf);
            daVo++;
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        Debug.Log(string.Format("[Act2VoBia] xet {0} bia, sut me {1} cai bang {2} ban vo",
                  daXet, daVo, kho.Count));
    }

    static bool HopLe(string ten)
    {
        for (int i = 0; i < DauTen.Length; i++)
            if (ten.StartsWith(DauTen[i])) return true;
        return false;
    }

    /// <summary>
    /// Cat mot goc cua luoi bang mot mat phang nghieng.
    ///
    /// Mat phang di qua mot diem nam o <b>phan tren</b> cua khoi bao (60..90%
    /// chieu cao) - da mo me o dinh va o goc tren, hiem khi me o chan. Huong
    /// phap tuyen boc ngang va cheo len, nen cho cat luon la mot goc chu khong
    /// phai cat ngang doi ca tam bia.
    ///
    /// Cat CAN THAN mot chuyen: neu mat phang an qua sau, no se an het ca dinh
    /// va tam bia bien thanh mot cai nem. Nen luong dinh bi cat duoc dem lai, va
    /// neu qua <see cref="TranCat"/> phan tram thi keo mat phang lui ra.
    /// </summary>
    const float TranCat = 0.22f;

    static Mesh DungBanVo(Mesh goc, int bienThe)
    {
        var vo = Object.Instantiate(goc);
        vo.name = goc.name + "_vo" + bienThe;

        var dinh = vo.vertices;
        var bb = goc.bounds;

        int hat = BamTen(goc.name) + bienThe * 9187;

        // Huong cat: quanh truc dung, cheo len 20..55 do
        float yaw = Bam01(hat) * Mathf.PI * 2f;
        float pitch = Mathf.Lerp(20f, 55f, Bam01(hat + 131)) * Mathf.Deg2Rad;
        var phap = new Vector3(Mathf.Cos(yaw) * Mathf.Cos(pitch),
                               Mathf.Sin(pitch),
                               Mathf.Sin(yaw) * Mathf.Cos(pitch)).normalized;

        // Diem tren mat phang: mot goc o phan tren khoi bao
        float caoT = Mathf.Lerp(0.60f, 0.90f, Bam01(hat + 271));
        var diem = new Vector3(
            Mathf.Lerp(bb.min.x, bb.max.x, Bam01(hat + 379)),
            Mathf.Lerp(bb.min.y, bb.max.y, caoT),
            Mathf.Lerp(bb.min.z, bb.max.z, Bam01(hat + 487)));

        // Keo lui cho den khi khong cat qua nhieu
        float d0 = Vector3.Dot(diem, phap);
        for (int lan = 0; lan < 12; lan++)
        {
            int ngoai = 0;
            for (int i = 0; i < dinh.Length; i++)
                if (Vector3.Dot(dinh[i], phap) > d0) ngoai++;
            if (ngoai <= dinh.Length * TranCat) break;
            d0 += bb.size.magnitude * 0.03f;
        }

        // Chieu moi dinh nam ngoai len mat phang, cong nhieu cho mep khoi thang
        float nham = bb.size.magnitude * 0.012f;
        int daCat = 0;
        for (int i = 0; i < dinh.Length; i++)
        {
            float d = Vector3.Dot(dinh[i], phap) - d0;
            if (d <= 0f) continue;
            float lech = (Bam01(hat + i * 37) - 0.5f) * 2f * nham;
            dinh[i] -= phap * (d - lech);
            daCat++;
        }

        vo.vertices = dinh;
        vo.RecalculateNormals();
        vo.RecalculateBounds();
        vo.RecalculateTangents();
        return vo;
    }

    /// <summary>
    /// Tra moi bia ve luoi goc - go tat ca cac ban vo ra.
    ///
    /// Duong lui, phong khi ti le sut me nhin ra qua tay: chay lai menu 17 sau
    /// khi da doi <see cref="TiLeVo"/> thi nhung bia DA vo van giu nguyen (ham
    /// Chay bo qua luoi ten "_voN"), nen phai tra ve truoc roi moi cat lai.
    /// </summary>
    [MenuItem("Diablo 2.5D/17b. Tra bia mo ve nguyen ven - Act2", false, 99)]
    public static void TraVe()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Act2")
            sc = EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", OpenSceneMode.Single);

        // Gom san moi luoi goc trong project theo TEN, bo qua thu muc ban vo
        var theoTen = new Dictionary<string, Mesh>();
        var guids = AssetDatabase.FindAssets("t:Mesh");
        for (int i = 0; i < guids.Length; i++)
        {
            string d = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (d.StartsWith(ThuMuc)) continue;
            var tat = AssetDatabase.LoadAllAssetsAtPath(d);
            for (int k = 0; k < tat.Length; k++)
            {
                var lm = tat[k] as Mesh;
                if (lm != null && !theoTen.ContainsKey(lm.name)) theoTen[lm.name] = lm;
            }
        }

        int n = 0, khongThay = 0;
        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var m = mf.sharedMesh;
            if (m == null) continue;
            int cho = m.name.LastIndexOf("_vo");
            if (cho <= 0) continue;

            string tenGoc = m.name.Substring(0, cho);
            Mesh luoiGoc;
            if (!theoTen.TryGetValue(tenGoc, out luoiGoc)) { khongThay++; continue; }

            Undo.RecordObject(mf, "tra bia ve nguyen ven");
            mf.sharedMesh = luoiGoc;
            EditorUtility.SetDirty(mf);
            n++;
        }

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);
        Debug.Log(string.Format("[Act2VoBia] da tra {0} bia ve luoi goc ({1} cai khong tim thay luoi goc)",
                  n, khongThay));
    }
}
