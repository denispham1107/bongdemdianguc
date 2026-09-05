using UnityEngine;

/// <summary>
/// DUNG MAT DAT BANG UNITY TERRAIN.
///
/// Truoc day mat dat la MOT hinh khoi khong lo (110 x 110 o luoi) voi shader rieng,
/// tron hai lop anh theo mau dinh. Sua tay khong duoc - muon doi gi phai sua code
/// roi nuong lai.
///
/// Gio mat dat la mot <b>Unity Terrain</b> that:
///   - Hinh dang do cao nam trong file <c>TerrainData</c> (Assets/Terrain)
///   - Co BON lop vat lieu, ban to bang co ngay trong cua so Scene
///   - Unity tu lo phan chia o, cat bot phan ngoai tam nhin, do bong
///
/// Lan nuong dau tien, do cao va cho co / cho dat duoc dien san bang dung
/// nhung ham toan cu (<see cref="WorldFactory.AnalyticHeight"/> va
/// <see cref="WorldFactory.GrassAmount"/>) - nen canh vat trong nhu truoc.
/// Tu do tro di ban to len tren bang cong cu Terrain, khong dung toi code nua.
///
/// NET TO TAY CUA BAN DUOC GIU: nut "1. Nuong Asset" chi tao file TerrainData
/// khi no CHUA CO. Da co roi thi dung lai file cu, nen to bao nhieu cung khong mat.
/// Muon xoa het lam lai tu dau thi bam "8. Dung lai dia hinh".
/// </summary>
public static class TerrainFactory
{
    /// <summary>Do phan giai luoi do cao. Bat buoc la 2^n + 1.</summary>
    public const int HeightRes = 257;

    /// <summary>Do phan giai ban do "cho nao lop vat lieu nao".</summary>
    public const int AlphaRes = 512;

    // Thu tu bon lop vat lieu
    public const int LayerCo = 0;      // dat co co moc
    public const int LayerDatKho = 1;  // dat cat kho tro
    public const int LayerSoiDa = 2;   // soi da vun  (de trong, danh cho ban to tay)
    public const int LayerBun = 3;     // bun uot sam (de trong, danh cho ban to tay)

    /// <summary>Be ngang mot canh cua tam terrain. Giu dung bang tam luoi cu.</summary>
    public static float WorldSize(float radius)
    {
        return radius * 2.6f;
    }

    // ================================================================
    //  BON LOP VAT LIEU
    // ================================================================

    /// <summary>
    /// Tao bon lop vat lieu. Hai lop dau nhuom dung mau cua shader cu
    /// (M_Ground: _GrassColor va _DirtColor) de canh vat khong bi doi mau.
    /// </summary>
    public static TerrainLayer[] MakeLayers()
    {
        var co = new TerrainLayer();
        co.name = "Lop_Co";
        co.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.DarkSoil(), new Color(0.69f, 0.60f, 0.516f), "DatCo");
        co.tileSize = new Vector2(2.95f, 2.95f);
        co.specular = Color.black;
        co.smoothness = 0.04f;

        var kho = new TerrainLayer();
        kho.name = "Lop_DatKho";
        kho.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.MudGround(), new Color(0.444f, 0.36f, 0.264f), "DatKho");
        // Lech nhip voi lop tren mot chut cho hai lop khong lap trung o
        kho.tileSize = new Vector2(3.65f, 3.65f);
        kho.specular = Color.black;
        kho.smoothness = 0.03f;

        // LOP NAY LA LONG DUONG MON. Phai SANG HAN dat xung quanh thi loi di moi
        // hien ra; mau cu (0.40, 0.33, 0.28) cho ra do sang 0.170/0.069/0.033,
        // gan y het lop co ben canh (0.154/0.076/0.032) nen duong nhin ra vet
        // bong chu khong ra loi mon.
        var soi = new TerrainLayer();
        soi.name = "Lop_SoiDa";
        soi.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.CliffRock(), new Color(0.98f, 0.90f, 0.80f), "SoiDa");
        soi.tileSize = new Vector2(2.05f, 2.05f);
        soi.specular = Color.black;
        soi.smoothness = 0.08f;

        var bun = new TerrainLayer();
        bun.name = "Lop_Bun";
        bun.diffuseTexture = TextureFactory.Tinted(
            TextureFactory.MudGround(), new Color(0.20f, 0.165f, 0.125f), "Bun");
        bun.tileSize = new Vector2(4.85f, 4.85f);
        bun.specular = Color.black;
        bun.smoothness = 0.18f;

        return new[] { co, kho, soi, bun };
    }

    // ================================================================
    //  DUNG TERRAINDATA
    // ================================================================

    /// <summary>
    /// Khoang do cao ma ham toan sinh ra: <paramref name="baseY"/> la cho thap nhat,
    /// <paramref name="span"/> la chenh lech giua cho cao nhat va thap nhat.
    ///
    /// Unity luu do cao terrain la so 0..1 nen tam terrain luon nam TREN goc cua no.
    /// Phai keo goc xuong dung bang cho thap nhat thi mat dat moi trung voi luoi cu.
    /// Tinh rieng ra day de lan nuong sau con biet dat tam terrain o dau,
    /// ma khong phai sinh lai TerrainData (se xoa mat net to tay).
    /// </summary>
    public static void HeightRange(float radius, out float baseY, out float span)
    {
        float size = WorldSize(radius);
        float lo = float.MaxValue, hi = float.MinValue;

        for (int z = 0; z < HeightRes; z++)
            for (int x = 0; x < HeightRes; x++)
            {
                float px = ((float)x / (HeightRes - 1) - 0.5f) * size;
                float pz = ((float)z / (HeightRes - 1) - 0.5f) * size;

                float h = WorldFactory.AnalyticHeight(px, pz, radius);
                if (h < lo) lo = h;
                if (h > hi) hi = h;
            }

        baseY = lo;
        span = Mathf.Max(1f, hi - lo);
    }

    /// <summary>
    /// Dien san do cao va lop vat lieu cho mot tam terrain.
    /// </summary>
    public static TerrainData Make(float radius, TerrainLayer[] layers)
    {
        float size = WorldSize(radius);

        float lo, span;
        HeightRange(radius, out lo, out span);

        var heights = new float[HeightRes, HeightRes];
        for (int z = 0; z < HeightRes; z++)
            for (int x = 0; x < HeightRes; x++)
            {
                float px = ((float)x / (HeightRes - 1) - 0.5f) * size;
                float pz = ((float)z / (HeightRes - 1) - 0.5f) * size;
                heights[z, x] = (WorldFactory.AnalyticHeight(px, pz, radius) - lo) / span;
            }

        var data = new TerrainData();
        data.name = "DiaHinh";

        // Thu tu quan trong: dat do phan giai TRUOC, roi moi dat kich thuoc
        data.heightmapResolution = HeightRes;
        data.size = new Vector3(size, span, size);
        data.SetHeights(0, 0, heights);

        // ---- Lop vat lieu: cho nao co, cho nao dat ----
        data.terrainLayers = layers;
        data.alphamapResolution = AlphaRes;

        var alpha = new float[AlphaRes, AlphaRes, 4];
        for (int z = 0; z < AlphaRes; z++)
            for (int x = 0; x < AlphaRes; x++)
            {
                float px = ((float)x / (AlphaRes - 1) - 0.5f) * size;
                float pz = ((float)z / (AlphaRes - 1) - 0.5f) * size;

                float g = WorldFactory.GrassAmount(px, pz);
                alpha[z, x, LayerCo] = g;
                alpha[z, x, LayerDatKho] = 1f - g;
                alpha[z, x, LayerSoiDa] = 0f;   // de trong cho ban to
                alpha[z, x, LayerBun] = 0f;   // de trong cho ban to
            }
        data.SetAlphamaps(0, 0, alpha);

        return data;
    }

    // ================================================================
    //  DAT TAM TERRAIN VAO SCENE
    // ================================================================

    /// <summary>
    /// Dung doi tuong MatDat tu mot TerrainData co san va keo cho dung tam ban do.
    /// </summary>
    public static GameObject Build(Transform parent, float radius, TerrainData data)
    {
        float baseY, span;
        HeightRange(radius, out baseY, out span);

        var go = Terrain.CreateTerrainGameObject(data);
        go.name = "MatDat";
        if (parent != null) go.transform.SetParent(parent, false);

        // TerrainData tinh toa do tu GOC DUOI-TRAI, keo ve cho tam nam o (0, 0)
        float size = WorldSize(radius);
        go.transform.position = new Vector3(-size * 0.5f, baseY, -size * 0.5f);

        int ground = LayerMask.NameToLayer("Ground");
        if (ground >= 0) go.layer = ground;

        var t = go.GetComponent<Terrain>();
        if (t != null)
        {
            t.heightmapPixelError = 3f;      // cang nho cang net, cang ton may
            t.basemapDistance = 220f;        // xa hon nguong nay thi dung anh gop
            t.drawInstanced = true;
            t.allowAutoConnect = false;

            // Mat dat KHONG tu do bong len chinh no: tam terrain qua rong nen bong
            // tu do bi vo thanh nhung mang vuong (shadow acne). Cay, da, nhan vat
            // van do bong xuong dat binh thuong.
            t.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        return go;
    }
}
