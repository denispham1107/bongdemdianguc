using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// NUONG BO DIEM MOI LUA cho moi loai luoi cay trong Act2.
///
/// Quet ca hai man, gom cac <c>sharedMesh</c> cua cay lai (chin loai o Act2),
/// rai diem deu tren be mat tung cai roi luu thanh
/// <c>Assets/Resources/DiemLua/&lt;ten luoi&gt;.asset</c>.
///
/// ===================================================================
///  RAI THEO DIEN TICH TAM GIAC, KHONG PHAI LAY DINH CACH QUANG
/// ===================================================================
///
/// Lay dinh cach quang thi diem don ve cho luoi CHIA DAY - tuc nhung cho nhieu
/// chi tiet, thuong la cho noi canh va cac muc u nan - con mot khuc canh thang
/// dai hai met chi co vai chuc dinh thi gan nhu khong duoc diem nao. Lua se ro
/// tung cum o cho noi canh va bo trong ca doan giua.
///
/// Chon tam giac voi xac suat ti le DIEN TICH roi lay mot diem ngau nhien trong
/// tam giac do thi diem rai deu tren be mat that: khuc canh dai duoc nhieu diem
/// hon dung theo be mat cua no.
///
/// Trong Editor, <c>mesh.vertices</c> doc duoc ke ca khi Read/Write TAT - do
/// chinh la ly do buoc nuong nay lam duoc viec ma luc chay khong lam duoc.
/// </summary>
public static class Act2DiemLua
{
    /// <summary>Moi luoi rai bao nhieu diem moi lua.</summary>
    public const int SoDiem = 340;

    const string ThuMuc = "Assets/Resources/" + DiemLuaCay.ThuMuc;

    [MenuItem("Diablo 2.5D/20. Nuong diem moi lua cho cay", false, 99)]
    public static void Chay()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(ThuMuc))
            AssetDatabase.CreateFolder("Assets/Resources", DiemLuaCay.ThuMuc);

        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Act2")
            sc = EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", OpenSceneMode.Single);

        var luoi = new Dictionary<string, Mesh>();
        foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,
                                                               FindObjectsSortMode.None))
        {
            if (!CayChay.LaCay(mf.gameObject) && CayChay.GocCay(mf.transform) == null) continue;
            var m = mf.sharedMesh;
            if (m == null || string.IsNullOrEmpty(m.name)) continue;
            if (!luoi.ContainsKey(m.name)) luoi[m.name] = m;
        }

        int daLam = 0;
        var bao = new System.Text.StringBuilder();
        foreach (var kv in luoi)
        {
            var diem = Rai(kv.Value, SoDiem);
            if (diem == null || diem.Length == 0)
            {
                bao.AppendLine(string.Format("  {0}: KHONG rai duoc diem nao", kv.Key));
                continue;
            }

            string duong = string.Format("{0}/{1}.asset", ThuMuc, kv.Key);
            var cu = AssetDatabase.LoadAssetAtPath<DiemLuaCay>(duong);
            if (cu == null)
            {
                cu = ScriptableObject.CreateInstance<DiemLuaCay>();
                AssetDatabase.CreateAsset(cu, duong);
            }
            cu.diem = diem;
            cu.tenLuoi = kv.Key;
            cu.soDinhGoc = kv.Value.vertexCount;
            EditorUtility.SetDirty(cu);
            daLam++;

            // Do be cao cua bo diem de doi chieu voi be cao cua luoi: lech nhieu
            // tuc la rai truot mat mot phan cay.
            float yMin = float.MaxValue, yMax = float.MinValue;
            for (int i = 0; i < diem.Length; i++)
            {
                yMin = Mathf.Min(yMin, diem[i].y);
                yMax = Mathf.Max(yMax, diem[i].y);
            }
            bao.AppendLine(string.Format("  {0}: {1} diem tu {2} dinh, cao diem {3:F2} / cao luoi {4:F2}",
                           kv.Key, diem.Length, kv.Value.vertexCount,
                           yMax - yMin, kv.Value.bounds.size.y));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(string.Format("[Act2DiemLua] da nuong {0} bo diem moi lua:\n{1}", daLam, bao));
    }

    /// <summary>
    /// Rai <paramref name="soDiem"/> diem deu tren be mat luoi.
    ///
    /// Tat dinh: dung <see cref="System.Random"/> voi hat lay tu ten luoi, nen
    /// nuong lai bao nhieu lan cung ra dung bo diem cu - khong tao ra mot thay
    /// doi gia trong git moi lan chay menu.
    /// </summary>
    public static Vector3[] Rai(Mesh m, int soDiem)
    {
        var dinh = m.vertices;
        var tam = m.triangles;
        if (dinh.Length == 0 || tam.Length < 3) return null;

        int soTam = tam.Length / 3;

        // Bang cong don dien tich: chon tam giac voi xac suat ti le dien tich
        var congDon = new float[soTam];
        float tong = 0f;
        for (int i = 0; i < soTam; i++)
        {
            var a = dinh[tam[i * 3]];
            var b = dinh[tam[i * 3 + 1]];
            var c = dinh[tam[i * 3 + 2]];
            tong += Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            congDon[i] = tong;
        }
        if (tong <= 0f) return null;

        var rnd = new System.Random(Bam(m.name));
        var ra = new Vector3[soDiem];
        for (int i = 0; i < soDiem; i++)
        {
            float moc = (float)rnd.NextDouble() * tong;
            int t = TimCungTrai(congDon, moc);

            var a = dinh[tam[t * 3]];
            var b = dinh[tam[t * 3 + 1]];
            var c = dinh[tam[t * 3 + 2]];

            // Toa do trong tam ngau nhien deu tren tam giac
            float u = (float)rnd.NextDouble(), v = (float)rnd.NextDouble();
            if (u + v > 1f) { u = 1f - u; v = 1f - v; }
            ra[i] = a + (b - a) * u + (c - a) * v;
        }
        return ra;
    }

    static int TimCungTrai(float[] congDon, float moc)
    {
        int lo = 0, hi = congDon.Length - 1;
        while (lo < hi)
        {
            int giua = (lo + hi) / 2;
            if (congDon[giua] < moc) lo = giua + 1; else hi = giua;
        }
        return lo;
    }

    static int Bam(string s)
    {
        unchecked
        {
            int h = 17;
            for (int i = 0; i < s.Length; i++) h = h * 31 + s[i];
            return h;
        }
    }
}
