using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CHUA HAI LOI CUA CAY TRONG BAN DO BLENDER.
///
/// LOI 1 - LA LO LUNG KHONG CO CANH DE BAM.
/// Do duoc: 47% so dinh la nam cach mieng vo cay gan nhat hon 0.8 m, xa nhat
/// toi 2.60 m. Cay trong Blender chi co nhung canh TO, con nhanh con - thu that
/// su giu chum la - thi khong duoc sinh ra. Nhin vao thay tung mang la treo
/// giua khong khi.
/// Cach chua: GOM la thanh tung chum, roi MOC mot nhanh con thon tu cho vo cay
/// gan nhat ra toi chum la do. Nhanh con duoc noi thang vao mieng vo cay nen an
/// theo dung vat lieu vo cay.
///
/// LOI 2 - THAN CAY TRON LU MOT MAU.
/// Ca chin luoi cay deu KHONG CO toa do anh (UV), nen moi anh vo cay dap vao
/// deu chi lay dung mot diem mau. Cach chua nam o shader
/// "Diablo25D/BarkTriplanar": no khong hoi luoi ma tu tinh cho lay anh theo vi
/// tri trong khong gian, nen chay duoc tren luoi khong co UV.
/// </summary>
public static class Act2Cay
{
    const string ThuMuc = "Assets/BlenderMaps/GraveyardAct2/CayDaChua";

    /// <summary>Chum la gan hon khoang nay thi coi nhu da bam vao canh roi.</summary>
    ///
    /// Do duoc luc dau: 47% so dinh la nam xa mieng vo cay hon 0,8 m, xa nhat
    /// 2,60 m - do la nhung chum la lo lung that su. De nguong 0,35 m la moc
    /// nhanh cho ca nhung chum VON DA bam vao canh roi, thua ra hang nghin cai.
    const float DaBamRoi = 0.55f;

    /// <summary>
    /// Canh nao mot chum la, tinh theo met.
    ///
    /// Lan dau toi de 0,7 m: mot cay ra toi 1000-2000 nhanh con, nhin ra con
    /// nhim chu khong ra cay. O to gap doi thi so nhanh giam khoang tam lan.
    /// </summary>
    const float CoChum = 1.5f;

    // ================================================================
    //  VAT LIEU VO CAY
    // ================================================================

    static Material mVoCay;

    /// <summary>
    /// Vat lieu vo cay: den ngA xanh reu, nut ne, co it nhua ri ra.
    /// </summary>
    public static Material VoCay()
    {
        if (mVoCay != null) return mVoCay;

        var sh = Shader.Find("Diablo25D/BarkTriplanar");
        if (sh == null)
        {
            Debug.LogError("[Act2Cay] Khong tim thay shader Diablo25D/BarkTriplanar");
            return null;
        }

        if (!AssetDatabase.IsValidFolder(ThuMuc)) TaoThuMuc();

        var anh = GhiAnh(VeAnhVoCay(), "VoCay_Mau", false);
        var gan = GhiAnh(VeAnhGanVoCay(), "VoCay_Gan", true);

        var m = new Material(sh);
        m.name = "Act2_VoCay_SanSui";
        m.SetColor("_Color", new Color(0.115f, 0.135f, 0.100f));      // den nga xanh
        m.SetTexture("_MainTex", anh);
        m.SetTexture("_BumpMap", gan);
        m.SetColor("_NhuaColor", new Color(0.42f, 0.27f, 0.07f));     // nhua nau ho phach
        m.SetFloat("_NhuaMuc", 0.55f);
        m.SetFloat("_TiLe", 1.15f);
        m.SetFloat("_KeoDoc", 2.1f);
        m.SetFloat("_DoSanSui", 1.9f);
        m.SetFloat("_Glossiness", 0.05f);
        m.SetFloat("_XanhReu", 0.45f);

        string d = ThuMuc + "/Act2_VoCay_SanSui.mat";
        AssetDatabase.DeleteAsset(d);
        AssetDatabase.CreateAsset(m, d);
        mVoCay = AssetDatabase.LoadAssetAtPath<Material>(d);
        return mVoCay;
    }

    static void TaoThuMuc()
    {
        if (!AssetDatabase.IsValidFolder("Assets/BlenderMaps"))
            AssetDatabase.CreateFolder("Assets", "BlenderMaps");
        if (!AssetDatabase.IsValidFolder("Assets/BlenderMaps/GraveyardAct2"))
            AssetDatabase.CreateFolder("Assets/BlenderMaps", "GraveyardAct2");
        if (!AssetDatabase.IsValidFolder(ThuMuc))
            AssetDatabase.CreateFolder("Assets/BlenderMaps/GraveyardAct2", "CayDaChua");
    }

    /// <summary>
    /// Ve anh vo cay: van chay doc, nut ngang cat qua, dom reu.
    /// KENH ALPHA danh dau cho co nhua cay - shader doc kenh do de biet cho nao
    /// phai boi mau nhua va lam bong len.
    /// </summary>
    static Texture2D VeAnhVoCay()
    {
        const int S = 512;
        var t = new Texture2D(S, S, TextureFormat.RGBA32, true);
        var px = new Color[S * S];

        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float u = (float)x / S, v = (float)y / S;

                // Van doc: nhieu keo dai theo chieu doc
                float doc = Mathf.PerlinNoise(u * 26f, v * 3.2f);
                float doc2 = Mathf.PerlinNoise(u * 61f + 9f, v * 7f + 3f);
                float than = doc * 0.62f + doc2 * 0.38f;

                // Ranh nut sau: cho nao van doc xuong thap nhat thi khoet thanh ranh
                float ranh = Mathf.Clamp01((0.40f - than) * 4.2f);

                // Nut NGANG cat qua, thua hon
                float ngang = Mathf.PerlinNoise(u * 4.5f + 31f, v * 38f);
                float nutNgang = Mathf.Clamp01((0.30f - ngang) * 3.4f) * 0.55f;

                float nut = Mathf.Max(ranh, nutNgang);

                // Do sang nen
                float sang = Mathf.Lerp(0.72f, 1.0f, than);
                sang *= 1f - nut * 0.72f;

                // Dom reu bam thanh mang
                float reu = Mathf.PerlinNoise(u * 8.5f + 55f, v * 8.5f + 21f);
                reu = Mathf.Clamp01((reu - 0.58f) * 3.2f);

                float r = sang * (1f - reu * 0.30f);
                float g = sang * (1f - reu * 0.05f);
                float b = sang * (1f - reu * 0.42f);

                // Nhua cay: vai vet chay doc, hiem.
                //
                // BA CAI MOC DUOI DAY DA DUOC DO, KHONG PHAI DOAN. Nhieu Perlin cua
                // Unity trong thuc te chi chay quanh 0,25-0,75 chu khong trai deu 0-1.
                // Ban dau toi de moc 0,70 va 0,42 roi nhan them cho vet nut - ba cai
                // cong nhan vao nhau lam ca tam anh 512x512 co alpha cao nhat 0,055,
                // tuc la KHONG CO GIOT NHUA NAO. Bo moc xuong 0,48 va 0,34 thi do phu
                // len 1,7% dam va 5,8% nhat, dung nghia "co it nhua".
                float nhua = Mathf.PerlinNoise(u * 3.1f + 77f, v * 1.4f + 12f);
                float nhuaDoc = Mathf.PerlinNoise(u * 15f + 101f, v * 2.2f + 40f);
                float a = Mathf.Clamp01((nhua - 0.48f) * 6f) * Mathf.Clamp01((nhuaDoc - 0.34f) * 4f);

                // Nhua chi ri o trong ranh nut, khong bao gio bam tren cho loi
                a *= Mathf.Clamp01(nut * 2.6f);

                px[y * S + x] = new Color(r, g, b, a);
            }

        t.SetPixels(px);
        t.Apply(true);
        return t;
    }

    /// <summary>Anh gan lay tu do sang cua anh vo cay, de cac ranh nut noi khoi.</summary>
    static Texture2D VeAnhGanVoCay()
    {
        var mau = VeAnhVoCay();
        int S = mau.width;
        var cao = new float[S, S];
        var p = mau.GetPixels();
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
                cao[y, x] = p[y * S + x].r;

        var t = new Texture2D(S, S, TextureFormat.RGBA32, true);
        var kq = new Color[S * S];
        const float manh = 4.2f;

        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                int xt = (x - 1 + S) % S, xp = (x + 1) % S;
                int yt = (y - 1 + S) % S, yp = (y + 1) % S;

                float dx = (cao[y, xp] - cao[y, xt]) * manh;
                float dy = (cao[yp, x] - cao[yt, x]) * manh;

                Vector3 n = new Vector3(-dx, -dy, 1f).normalized;
                kq[y * S + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
            }

        t.SetPixels(kq);
        t.Apply(true);
        Object.DestroyImmediate(mau);
        return t;
    }

    static Texture2D GhiAnh(Texture2D src, string ten, bool laAnhGan)
    {
        string d = ThuMuc + "/" + ten + ".png";
        File.WriteAllBytes(d, src.EncodeToPNG());
        Object.DestroyImmediate(src);

        AssetDatabase.ImportAsset(d, ImportAssetOptions.ForceSynchronousImport);
        var imp = AssetImporter.GetAtPath(d) as TextureImporter;
        if (imp != null)
        {
            // Anh gan PHAI danh dau la NormalMap, khong thi Unity doc no nhu anh
            // mau thuong va cho san sui se ra sai huong hoan toan
            imp.textureType = laAnhGan ? TextureImporterType.NormalMap
                                       : TextureImporterType.Default;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.anisoLevel = 8;
            imp.mipmapEnabled = true;
            imp.alphaIsTransparency = false;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(d);
    }

    // ================================================================
    //  MOC CANH CON CHO LA CO CHO BAM
    // ================================================================

    /// <summary>
    /// Moc nhanh con noi tu vo cay ra toi tung chum la.
    ///
    /// Tra ve luoi MOI (khong dung vao luoi goc trong file FBX). Neu luoi khong
    /// co mieng la rieng thi tra ve chinh no, khong lam gi.
    /// </summary>
    public static Mesh MocNhanh(Mesh goc, float tiLeBu, string tenLuu, out int soNhanh)
    {
        soNhanh = 0;
        if (goc == null || goc.subMeshCount < 2) return goc;

        var dinhGoc = goc.vertices;
        var phapGoc = goc.normals;
        var tiepGoc = goc.tangents;
        var uvGoc = goc.uv;
        var voTam = goc.GetTriangles(0);
        var laTam = goc.GetTriangles(1);

        // ---- Diem vo cay, doi ra met that de nguong tinh cho dung ----
        var diemVo = new List<Vector3>();
        var daVo = new HashSet<int>();
        foreach (var i in voTam) if (daVo.Add(i)) diemVo.Add(dinhGoc[i] * tiLeBu);

        var oVo = new Dictionary<long, List<int>>();
        const float O = 0.5f;
        for (int i = 0; i < diemVo.Count; i++)
        {
            long ma = Ma(diemVo[i], O);
            List<int> ds;
            if (!oVo.TryGetValue(ma, out ds)) { ds = new List<int>(); oVo[ma] = ds; }
            ds.Add(i);
        }

        // ---- Gom la thanh tung chum theo o luoi ----
        var chum = new Dictionary<long, List<Vector3>>();
        var daLa = new HashSet<int>();
        foreach (var i in laTam)
        {
            if (!daLa.Add(i)) continue;
            Vector3 p = dinhGoc[i] * tiLeBu;
            long ma = Ma(p, CoChum);
            List<Vector3> ds;
            if (!chum.TryGetValue(ma, out ds)) { ds = new List<Vector3>(); chum[ma] = ds; }
            ds.Add(p);
        }

        // ---- Moc nhanh theo kieu lan dan, khong phai nan hoa ----
        var themDinh = new List<Vector3>();
        var themPhap = new List<Vector3>();
        var themTam = new List<int>();
        int nen = dinhGoc.Length;

        var tamChum = new List<Vector3>();
        foreach (var kv in chum)
        {
            Vector3 t = Vector3.zero;
            foreach (var p in kv.Value) t += p;
            tamChum.Add(t / kv.Value.Count);
        }

        // Lan dau toi noi THANG tung chum la ve mieng vo cay gan nhat. Nhin anh
        // chup ra ngay la sai: chum la o ria tan cach vo cay toi 5-6 m, nen moi
        // chum thanh mot cai que dai dam thang tu than ra - ca cay hoa con nhim.
        //
        // Cay that khong moc kieu do. Nhanh to de nhanh nho, nhanh nho de nhanh
        // nho hon. Nen doi sang cay khung nho nhat: moi vong chon chum NAO GAN
        // CHO DA CO NHANH NHAT roi noi vao dung cho do - co the la vo cay, cung
        // co the la mot chum da moc truoc do. Ket qua la nhung doan nhanh ngan
        // noi duoi nhau, toa dan ra nhu nhanh that.
        var noiVao = new Vector3[tamChum.Count];   // moc vao diem nao
        var gan = new float[tamChum.Count];        // cach diem do bao xa
        var xong = new bool[tamChum.Count];
        var xaVo = new float[tamChum.Count];       // di duong nhanh tu vo toi day

        for (int i = 0; i < tamChum.Count; i++)
        {
            float d;
            noiVao[i] = TimVoGanNhat(oVo, diemVo, tamChum[i], O, out d);
            gan[i] = d;
            xaVo[i] = d;
        }

        for (int lan = 0; lan < tamChum.Count; lan++)
        {
            int chon = -1;
            for (int i = 0; i < tamChum.Count; i++)
                if (!xong[i] && (chon < 0 || gan[i] < gan[chon])) chon = i;
            if (chon < 0) break;

            xong[chon] = true;

            if (gan[chon] > DaBamRoi && gan[chon] <= 6f)
            {
                // Cang xa vo cay thi nhanh cang manh, dung nhu cay that
                // Nhanh CON thi phai manh. De 5,5 cm nhu luc dau la to bang
                // canh that, moc ra tua cong ong hut cam quanh than cay.
                float day = Mathf.Lerp(0.026f, 0.007f, Mathf.Clamp01(xaVo[chon] / 4.5f));
                DungNhanh(themDinh, themPhap, themTam, nen + themDinh.Count,
                          noiVao[chon], tamChum[chon], day, tiLeBu);
                soNhanh++;
            }

            // Chum vua moc xong thanh cho bam moi cho nhung chum con lai
            for (int i = 0; i < tamChum.Count; i++)
            {
                if (xong[i]) continue;
                float k = Vector3.Distance(tamChum[i], tamChum[chon]);
                if (k < gan[i])
                {
                    gan[i] = k;
                    noiVao[i] = tamChum[chon];
                    xaVo[i] = xaVo[chon] + k;
                }
            }
        }

        if (soNhanh == 0) return goc;

        // ---- Ghep vao luoi moi ----
        var dinhMoi = new List<Vector3>(dinhGoc);
        var phapMoi = new List<Vector3>(phapGoc != null && phapGoc.Length == dinhGoc.Length
                                        ? phapGoc : new Vector3[dinhGoc.Length]);
        foreach (var p in themDinh) dinhMoi.Add(p / tiLeBu);   // tra lai ti le cua file
        foreach (var n in themPhap) phapMoi.Add(n);

        // PHAI CHEP LAI TIEP TUYEN. Luoi cay khong he co toa do anh (UV), nen
        // khong the goi RecalculateTangents de tinh lai - ma neu de trong thi
        // khong gian tiep tuyen cua la thanh vo dinh, la bi to den thui va nhin
        // nhu ca cay rung trui. Toi da dung phai loi nay: chup ra tuong luoi hong.
        var tiepMoi = new List<Vector4>();
        if (tiepGoc != null && tiepGoc.Length == dinhGoc.Length)
        {
            tiepMoi.AddRange(tiepGoc);
            for (int i = 0; i < themDinh.Count; i++)
            {
                Vector3 t = Vector3.Cross(themPhap[i], Vector3.up);
                if (t.sqrMagnitude < 1e-6f) t = Vector3.right;
                t.Normalize();
                tiepMoi.Add(new Vector4(t.x, t.y, t.z, 1f));
            }
        }

        var uvMoi = new List<Vector2>();
        if (uvGoc != null && uvGoc.Length == dinhGoc.Length)
        {
            uvMoi.AddRange(uvGoc);
            for (int i = 0; i < themDinh.Count; i++) uvMoi.Add(Vector2.zero);
        }

        var voMoi = new List<int>(voTam);
        voMoi.AddRange(themTam);

        if (!AssetDatabase.IsValidFolder(ThuMuc)) TaoThuMuc();
        string duong = ThuMuc + "/" + tenLuu + ".asset";

        // GHI THANG VAO CHINH DOI TUONG LUOI CU. Hai cach lam sai toi da vap:
        //
        //  1. Xoa file roi tao lai: Unity cap ma GUID moi, ma tung goc cay ngoai
        //     canh chi nho luoi theo GUID - xoa mot cai la 58 goc cay mat luoi.
        //
        //  2. Tao luoi moi roi EditorUtility.CopySerialized de len file cu: doc
        //     lai bang code thi so lieu dung y het (dinh, tam giac, mieng, hop
        //     bao deu khop tuyet doi), NHUNG ban nam tren card do hoa khong duoc
        //     dung lai, nen ve ra mat trang mieng la - ca cay trui lui. Do mai
        //     khong ra vi moi phep do deu bao "giong het".
        //
        // Lay dung doi tuong cu ra, Clear() roi do du lieu moi vao la vua giu
        // duoc GUID vua bat Unity dung lai ban tren card do hoa.
        bool laLuoiMoi = false;
        var m = AssetDatabase.LoadAssetAtPath<Mesh>(duong);
        if (m == null) { m = new Mesh(); laLuoiMoi = true; }

        m.Clear();
        m.name = tenLuu;
        m.indexFormat = dinhMoi.Count > 65000
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        m.SetVertices(dinhMoi);
        m.SetNormals(phapMoi);
        if (tiepMoi.Count == dinhMoi.Count) m.SetTangents(tiepMoi);
        if (uvMoi.Count == dinhMoi.Count) m.SetUVs(0, uvMoi);
        m.subMeshCount = goc.subMeshCount;
        m.SetTriangles(voMoi, 0);
        for (int s = 1; s < goc.subMeshCount; s++)
            m.SetTriangles(goc.GetTriangles(s), s);
        m.RecalculateBounds();
        m.UploadMeshData(false);

        if (laLuoiMoi) AssetDatabase.CreateAsset(m, duong);
        else EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        return m;
    }

    /// <summary>Mot nhanh con thon, cong nhe xuong duoi.</summary>
    static void DungNhanh(List<Vector3> dinh, List<Vector3> phap, List<int> tam, int nen,
                          Vector3 chan, Vector3 ngon, float day, float tiLeBu)
    {
        const int Canh = 4;      // bon canh la du: nhanh con rat manh, khong ai dem canh
        const int Dot = 4;

        Vector3 doc = (ngon - chan);
        float dai = doc.magnitude;
        if (dai < 0.01f) return;
        doc /= dai;

        Vector3 ngang = Vector3.Cross(doc, Vector3.up);
        if (ngang.sqrMagnitude < 1e-4f) ngang = Vector3.Cross(doc, Vector3.forward);
        ngang.Normalize();
        Vector3 ngang2 = Vector3.Cross(doc, ngang);

        for (int d = 0; d <= Dot; d++)
        {
            float t = (float)d / Dot;

            // Vong cung nhe: giua nhanh vong xuong mot chut
            Vector3 giua = Vector3.Lerp(chan, ngon, t)
                         + Vector3.down * Mathf.Sin(t * Mathf.PI) * dai * 0.09f;

            float r = Mathf.Lerp(day, day * 0.35f, t);

            for (int c = 0; c < Canh; c++)
            {
                float a = c / (float)Canh * Mathf.PI * 2f;
                Vector3 huong = ngang * Mathf.Cos(a) + ngang2 * Mathf.Sin(a);
                dinh.Add(giua + huong * r);
                phap.Add(huong);
            }
        }

        for (int d = 0; d < Dot; d++)
            for (int c = 0; c < Canh; c++)
            {
                int a0 = nen + d * Canh + c;
                int a1 = nen + d * Canh + (c + 1) % Canh;
                int b0 = a0 + Canh;
                int b1 = a1 + Canh;

                tam.Add(a0); tam.Add(b0); tam.Add(b1);
                tam.Add(a0); tam.Add(b1); tam.Add(a1);
            }
    }

    static Vector3 TimVoGanNhat(Dictionary<long, List<int>> oVo, List<Vector3> diemVo,
                                Vector3 p, float o, out float d)
    {
        Vector3 tot = p; d = float.MaxValue;
        int cx = Mathf.FloorToInt(p.x / o), cy = Mathf.FloorToInt(p.y / o), cz = Mathf.FloorToInt(p.z / o);

        for (int r = 0; r <= 14; r++)
        {
            for (int dz = -r; dz <= r; dz++)
                for (int dy = -r; dy <= r; dy++)
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (r > 0 && Mathf.Abs(dx) != r && Mathf.Abs(dy) != r && Mathf.Abs(dz) != r) continue;

                        long ma = (((long)(cx + dx) + 4096) << 26)
                                | (((long)(cy + dy) + 4096) << 13)
                                | ((long)(cz + dz) + 4096);

                        List<int> ds;
                        if (!oVo.TryGetValue(ma, out ds)) continue;
                        foreach (var i in ds)
                        {
                            float k = (diemVo[i] - p).sqrMagnitude;
                            if (k < d) { d = k; tot = diemVo[i]; }
                        }
                    }

            // Tim thay roi thi quet them mot vong nua cho chac, roi dung
            if (d < float.MaxValue && r >= 1) break;
        }

        d = d < float.MaxValue ? Mathf.Sqrt(d) : 999f;
        return tot;
    }

    static long Ma(Vector3 p, float o)
    {
        long x = Mathf.FloorToInt(p.x / o) + 4096;
        long y = Mathf.FloorToInt(p.y / o) + 4096;
        long z = Mathf.FloorToInt(p.z / o) + 4096;
        return (x << 26) | (y << 13) | z;
    }
}
