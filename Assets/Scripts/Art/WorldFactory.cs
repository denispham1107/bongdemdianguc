using UnityEngine;

/// <summary>
/// DUNG CANH VAT: mot vung dong hoang duoi anh trang.
///
/// Mat dat la dat nau den go ghe, xen ke nhung mang dat cat kho nhat mau hon.
/// Tren do moc rai rac nhung khom co du dua theo gio, bui ram, cay coi,
/// nhung khoi da phien goc canh va vanh nui da bao quanh dau truong.
///
/// Toan bo deu sinh bang code khi bat dau van choi - khong dung file .fbx nao.
/// </summary>
public static class WorldFactory
{
    public static void Build(Transform parent, float radius, int seed)
    {
        Random.InitState(seed);

        BuildSkyAndFog();
        BuildTerrain(parent, radius);
        BuildGrassField(parent, radius);
        BuildBoundary(parent, radius);
        BuildProps(parent, radius);
    }

    // ================================================================
    //  BAU TROI, SUONG MU, ANH SANG MOI TRUONG
    // ================================================================

    /// <summary>
    /// Man dang mo co phai Act2 khong.
    ///
    /// Act2 dung mot bo mau RIENG - xanh lam lanh, co mat trang. Act1 giu
    /// nguyen tong cu. Neu khong tach ra thi doi mau cho Act2 se doi luon ca
    /// Act1, vi GameBootstrap goi BuildSkyAndFog o MOI man.
    /// </summary>
    static bool LaAct2()
    {
        var ten = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return !string.IsNullOrEmpty(ten) && ten.Contains("Act2");
    }

    public static void BuildSkyAndFog()
    {
        bool act2 = LaAct2();

        if (Mats.SkyShader != null)
        {
            var sky = new Material(Mats.SkyShader);
            sky.name = "Sky";

            if (act2)
            {
                // ACT2 - troi dem XANH LAM LANH, sang han len o chan troi.
                //
                // Ban truoc troi gan nhu den, ma anh sang moi truong duoi dat
                // lai nga NAU - ca man choi ra tong nau do am.
                sky.SetColor("_TopColor", new Color(0.055f, 0.085f, 0.110f));
                sky.SetColor("_HorizColor", new Color(0.150f, 0.205f, 0.235f));
                sky.SetColor("_BottomColor", new Color(0.040f, 0.055f, 0.065f));
                sky.SetColor("_CloudColor", new Color(0.180f, 0.235f, 0.265f));
                sky.SetFloat("_CloudAmount", 0.50f);

                // MAT TRANG. Huong phai NGUOC voi huong chieu cua Moonlight: den
                // chieu THEO forward cua no, con day la huong NHIN TOI mat trang.
                // Dat sai dau la trang moc sau lung nguoi choi trong khi bong do
                // lai nga ve phia truoc.
                sky.SetVector("_MoonDir", -MoonForward());
                sky.SetColor("_MoonColor", new Color(0.92f, 0.95f, 1.0f));
                sky.SetFloat("_MoonSize", 0.024f);
                sky.SetFloat("_MoonGlow", 0.30f);
                sky.SetFloat("_MoonStrength", 0.9f);
            }
            else
            {
                sky.SetColor("_TopColor", new Color(0.025f, 0.035f, 0.085f));
                sky.SetColor("_HorizColor", new Color(0.11f, 0.13f, 0.20f));
                sky.SetColor("_BottomColor", new Color(0.02f, 0.02f, 0.03f));
                sky.SetColor("_CloudColor", new Color(0.15f, 0.16f, 0.24f));
                sky.SetFloat("_CloudAmount", 0.55f);
                sky.SetFloat("_MoonStrength", 0f);   // Act1 khong ve mat trang
            }

            sky.SetFloat("_Exponent", 1.5f);
            sky.SetFloat("_StarAmount", 1f);        // ban dem thi bat sao
            RenderSettings.skybox = sky;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

        if (act2)
        {
            // Anh sang moi truong XANH LAM ca ba tang.
            //
            // Tang equator va ground truoc day nga nau - do la thu keo ca man
            // choi ve tong nau do, chu khong phai bau troi. Anh sang hat len tu
            // mat dat cham vao MOI vat nen no quyet dinh tong mau chung.
            RenderSettings.ambientSkyColor = new Color(0.120f, 0.205f, 0.265f);
            RenderSettings.ambientEquatorColor = new Color(0.085f, 0.155f, 0.190f);
            RenderSettings.ambientGroundColor = new Color(0.045f, 0.085f, 0.105f);

            // Suong xanh lam, sang hon va thua hon: van nhin ro hang cay o xa
            RenderSettings.fogColor = new Color(0.115f, 0.175f, 0.215f);
            RenderSettings.fogDensity = 0.0105f;
        }
        else
        {
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.17f, 0.23f);
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.12f, 0.11f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.06f, 0.04f);
            RenderSettings.fogColor = new Color(0.08f, 0.08f, 0.11f);
            RenderSettings.fogDensity = 0.014f;
        }
    }

    /// <summary>Goc chieu cua anh trang. De o MOT CHO de bau troi dat dia
    /// trang dung cho anh sang hat toi.</summary>
    public static readonly Vector3 MoonAngles = new Vector3(42f, 148f, 0f);

    /// <summary>Huong CHIEU cua anh trang (forward cua den).</summary>
    public static Vector4 MoonForward()
    {
        Vector3 f = Quaternion.Euler(MoonAngles) * Vector3.forward;
        return new Vector4(f.x, f.y, f.z, 0f);
    }

    /// <summary>Anh trang: nguon sang chinh cua man choi ban dem.</summary>
    public static void SetupMoonlight(Light moon)
    {
        if (moon == null) return;
        moon.type = LightType.Directional;
        moon.shadows = LightShadows.Soft;

        if (LaAct2())
        {
            // THANH PHAN DO phai xuong THAP.
            //
            // Mat dat cua Act2 von la dat nau (do cao trong albedo). Anh sang
            // chi NHAN vao albedo, nen chi cang lam do nen bao nhieu thi dat van
            // cu nau bay nhieu - muon ca man choi nga xanh thi phai HA do trong
            // nguon sang xuong, chu khong phai nang xanh len.
            moon.color = new Color(0.52f, 0.72f, 0.98f);
            moon.intensity = 0.88f;
            moon.shadowStrength = 0.62f;
            moon.transform.rotation = Quaternion.Euler(MoonAngles);
        }
        else
        {
            moon.color = new Color(0.72f, 0.74f, 0.88f);
            moon.intensity = 0.62f;
            moon.shadowStrength = 0.75f;
            moon.transform.rotation = Quaternion.Euler(48f, 145f, 0f);
        }
        RenderSettings.sun = moon;
    }

    // ================================================================
    //  MAT DAT
    // ================================================================

    /// <summary>
    /// Do cao mat dat tai mot diem - tinh bang cong thuc chu khong ban tia,
    /// nen dat duoc hang nghin bui co ma van nhanh.
    /// </summary>
    /// <summary>
    /// Do cao mat dat tai mot cho.
    ///
    /// Neu trong scene co Unity Terrain thi DOC THANG TU TERRAIN - nho vay moi net
    /// ban to tay len mat dat deu duoc co, da, cay va vong tron phep bam theo dung.
    /// Khong co terrain (vi du man hinh chinh) thi quay ve dung ham toan cu.
    /// </summary>
    public static float GroundHeight(float px, float pz, float radius)
    {
        var t = Terrain.activeTerrain;
        if (t != null)
            return t.SampleHeight(new Vector3(px, 0f, pz)) + t.transform.position.y;

        return AnalyticHeight(px, pz, radius);
    }

    /// <summary>
    /// Hinh dang mat dat tinh bang cong thuc: doi thoai thoai, go nho, gon lam tam,
    /// va vanh dat cao dan o ria de vay kin dau truong.
    ///
    /// Day la ban goc dung de DIEN SAN cho Unity Terrain lan dau
    /// (xem <see cref="TerrainFactory.Make"/>). Sau khi co terrain roi thi ham nay
    /// chi con dung lam duong lui.
    /// </summary>
    public static float AnalyticHeight(float px, float pz, float radius)
    {
        float d = Mathf.Sqrt(px * px + pz * pz) / Mathf.Max(0.01f, radius);

        float hills = (Mathf.PerlinNoise(px * 0.035f + 4f, pz * 0.035f + 9f) - 0.5f) * 2.6f;
        float bumps = (Mathf.PerlinNoise(px * 0.11f + 11f, pz * 0.11f + 7f) - 0.5f) * 0.75f;
        float fine = (Mathf.PerlinNoise(px * 0.38f, pz * 0.38f) - 0.5f) * 0.16f;
        float rise = Mathf.Clamp01(d - 0.75f) * 9f;          // vanh dat cao dan o ria

        return hills + bumps + fine + rise;
    }

    /// <summary>
    /// Do "mau mo" cua mot cho dat (1 = dat thit am, co moc duoc; 0 = dat cat kho tro).
    /// Dung chung cho ca mau mat dat lan cho moc khom co, nen co khong bao gio
    /// moc giua bai cat kho.
    /// </summary>
    public static float GrassAmount(float px, float pz)
    {
        float patch = Mathf.PerlinNoise(px * 0.045f + 31f, pz * 0.045f + 17f);
        float detail = Mathf.PerlinNoise(px * 0.16f + 5f, pz * 0.16f + 23f);

        float k = patch * 0.75f + detail * 0.25f;

        // Keo manh ve hai dau: phan lon la co, chi nhung cho trung nhat moi tro dat
        return Mathf.Clamp01((k - 0.26f) * 6.5f);
    }

    /// <summary>
    /// Mau cua dat bi boc len tai mot cho.
    ///
    /// Dung chung mot cach tinh voi mau mat dat (xem <see cref="GrassAmount"/>),
    /// nen manh vun bi loc xoay cuon len luon co mau khop voi cho no vua di qua:
    /// qua bai cat kho thi bay ra dat nhat mau, qua vung dat thit thi bay ra dat sam.
    /// </summary>
    public static Color SoilColorAt(float px, float pz)
    {
        float g = GrassAmount(px, pz);

        // Toi theo mat dat (da nhan 0.6) de manh vun khong sang hon cho no bi boc len
        var mud = new Color(0.240f, 0.198f, 0.150f);   // dat cat kho, nhat hon
        var sod = new Color(0.138f, 0.114f, 0.078f);   // dat thit am, sam mau

        var c = Color.Lerp(mud, sod, g);

        // Cho dam cho nhat mot chut cho khoi deu tam tap
        float n = Mathf.PerlinNoise(px * 0.32f + 12f, pz * 0.32f + 4f);
        float k = Mathf.Lerp(0.82f, 1.16f, n);
        return new Color(c.r * k, c.g * k, c.b * k, 1f);
    }

    /// <summary>
    /// Dung mat dat kieu Unity Terrain (duong dung luc chay game, khi chua co
    /// scene nuong san). Sinh TerrainData ngay trong bo nho roi dat vao scene.
    /// </summary>
    public static GameObject BuildTerrain(Transform parent, float radius)
    {
        var layers = TerrainFactory.MakeLayers();
        var data = TerrainFactory.Make(radius, layers);
        return TerrainFactory.Build(parent, radius, data);
    }

    /// <summary>
    /// Mat dat kieu cu: MOT hinh khoi luoi voi shader tron hai lop.
    /// Van giu lai cho man hinh chinh va lam duong lui.
    /// </summary>
    public static GameObject BuildGround(Transform parent, float radius)
    {
        int grid = 110;
        float size = radius * 2.6f;
        var b = new ProcMesh.Builder();

        for (int z = 0; z <= grid; z++)
            for (int x = 0; x <= grid; x++)
            {
                float u = (float)x / grid, v = (float)z / grid;
                float px = (u - 0.5f) * size;
                float pz = (v - 0.5f) * size;

                float y = AnalyticHeight(px, pz, radius);

                // Mau dinh mang thong tin "cho nay bao nhieu co" cho shader Terrain
                float g = GrassAmount(px, pz);
                var c = new Color(g, g, g, 1f);

                b.AddVertex(new Vector3(px, y, pz), Vector3.up, new Vector2(u, v), c);
            }

        int cols = grid + 1;
        for (int z = 0; z < grid; z++)
            for (int x = 0; x < grid; x++)
            {
                int i0 = z * cols + x;
                b.AddQuad(i0, i0 + cols, i0 + cols + 1, i0 + 1);
            }

        var mesh = b.Build("ground");
        mesh.RecalculateNormals();

        var go = ProcMesh.Part("Ground", parent, mesh, Mats.Ground, Vector3.zero,
                               Quaternion.identity, Vector3.one, false);
        go.layer = LayerMask.NameToLayer("Ground");
        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        return go;
    }

    // ================================================================
    //  THAM CO
    // ================================================================

    /// <summary>So bui co tren moi met vuong. Ha xuong neu may chay giat.</summary>
    public static float grassDensity = 0.36f;

    /// <summary>
    /// Rai bui co khap dong.
    ///
    /// Ca bai co duoc gom thanh vai chuc "mang" - moi mang la MOT hinh khoi duy nhat
    /// chua hang tram bui. Nho vay may chi phai ve vai chuc lan thay vi hang nghin,
    /// va camera co the bo qua nhung mang nam ngoai tam nhin.
    /// </summary>
    public static GameObject BuildGrassField(Transform parent, float radius)
    {
        var root = new GameObject("ThamCo");
        root.transform.SetParent(parent, false);

        const int cells = 7;                       // 7 x 7 mang co
        float span = radius * 2f;
        float cell = span / cells;

        // Mat do: dong cang rong thi cang nhieu bui
        int perCell = Mathf.RoundToInt(cell * cell * grassDensity);

        for (int cz = 0; cz < cells; cz++)
            for (int cx = 0; cx < cells; cx++)
            {
                float x0 = -radius + cx * cell;
                float z0 = -radius + cz * cell;

                // Bo qua nhung o nam hoan toan ngoai vong tron dau truong
                Vector2 centre = new Vector2(x0 + cell * 0.5f, z0 + cell * 0.5f);
                if (centre.magnitude > radius + cell) continue;

                var b = new ProcMesh.Builder();
                int placed = 0;

                for (int i = 0; i < perCell; i++)
                {
                    float px = x0 + Random.value * cell;
                    float pz = z0 + Random.value * cell;

                    if (new Vector2(px, pz).magnitude > radius * 0.99f) continue;

                    // Chi moc o cho con co - vung bun de tran
                    float g = GrassAmount(px, pz);
                    if (Random.value > g) continue;

                    // Khong moc co giua long duong mon
                    if (DuongFactory.TranhDuong(px, pz, 0.25f)) continue;

                    float y = GroundHeight(px, pz, radius);

                    // Bui co cao thap khac nhau, cho nao co ram thi cao hon
                    // Cao hon 20% so voi truoc, be ngang giu nguyen nen than co manh va vuon cao
                    float h = Random.Range(0.384f, 0.864f) * Mathf.Lerp(0.7f, 1.3f, g);
                    float w = h * Random.Range(1.42f, 2.25f);

                    // Moi bui mot sac xanh hoi khac: cho vang uom, cho xanh dam
                    float tone = Mathf.PerlinNoise(px * 0.09f + 55f, pz * 0.09f + 12f);
                    var tint = Color.Lerp(new Color(0.72f, 0.80f, 0.58f),
                                          new Color(1.0f, 0.98f, 0.70f), tone);
                    tint *= Random.Range(0.80f, 1.05f);
                    tint.a = 1f;

                    ProcMesh.AppendFoliage(b, new Vector3(px, y - 0.04f, pz), w, h,
                                           Random.value < 0.35f ? 3 : 2,
                                           Random.Range(0f, 180f), tint);
                    placed++;
                }

                // Vai khom co lau cao vuot len cho dong co co lop lang
                int reeds = Mathf.RoundToInt(perCell * 0.035f);
                for (int i = 0; i < reeds; i++)
                {
                    float px = x0 + Random.value * cell;
                    float pz = z0 + Random.value * cell;
                    if (new Vector2(px, pz).magnitude > radius * 0.96f) continue;
                    if (GrassAmount(px, pz) < 0.55f) continue;

                    float y = GroundHeight(px, pz, radius);
                    float h = Random.Range(1.08f, 1.8f);

                    var tint = new Color(0.72f, 0.80f, 0.52f) * Random.Range(0.85f, 1.1f);
                    tint.a = 1f;

                    ProcMesh.AppendFoliage(b, new Vector3(px, y - 0.05f, pz),
                                           h * 0.71f, h, 3, Random.Range(0f, 180f), tint);
                    placed++;
                }

                if (placed == 0) continue;

                var mesh = b.Build("grass_" + cx + "_" + cz);
                var go = ProcMesh.Part("MangCo_" + cx + "_" + cz, root.transform, mesh,
                                       Mats.GrassTuft, Vector3.zero, Quaternion.identity,
                                       Vector3.one, false);
                go.isStatic = false;      // co dong dua nen khong gop tinh duoc
            }

        return root;
    }

    // ================================================================
    //  KHOI DA GOC CANH
    // ================================================================

    public const int RockStyleSpire = 0;   // nhung ngon nhon dam thang len troi
    public const int RockStyleCliff = 1;   // khoi vach da to, mat phang, vuong vuc
    public const int RockStyleRubble = 2;  // dong da phien nam bet, thap

    /// <summary>
    /// MOT KHOI DA: gom nhieu phien da goc canh dam len tu mot be da thap,
    /// kem da vun rai quanh chan - dung kieu da nui that chu khong phai cuc da tron.
    ///
    /// Khoi duoc dung o co chuan: ban kinh khoang 1, cao khoang 1 den 2.
    /// Muon to nho the nao thi phong to vat the chua no.
    /// </summary>
    public static Mesh RockClusterMesh(int seed, int style)
    {
        var state = Random.state;
        Random.InitState(seed);

        var b = new ProcMesh.Builder();

        // ---- 1) THEM DA thap bao quanh chan ----
        // Mat gan nhu phang va chi cao 15-30cm, vua du de nhan vat BUOC LEN
        // roi tu do treo tiep len nhung phien cao hon.
        int steps = Random.Range(4, 8);
        for (int i = 0; i < steps; i++)
        {
            float a = (i / (float)steps) * 360f + Random.Range(-30f, 30f);
            float d = Random.Range(0.52f, 0.95f);
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, -0.10f, d);

            Quaternion rot = Quaternion.Euler(0f, a + Random.Range(-40f, 40f), 0f)
                           * Quaternion.Euler(Random.Range(8f, 24f), 0f, 0f);

            ProcMesh.AppendShard(b, Matrix4x4.TRS(pos, rot, Vector3.one),
                                 Random.Range(0.44f, 0.82f), Random.Range(0.14f, 0.30f),
                                 Random.Range(5, 8), 2, Random.Range(0.68f, 0.88f),
                                 0.22f, Color.white);
        }

        // ---- 2) Be da: phien nghieng vua phai, lam bac trung gian len phan tren ----
        int slabs = Random.Range(4, 8);
        for (int i = 0; i < slabs; i++)
        {
            float a = (i / (float)slabs) * 360f + Random.Range(-28f, 28f);
            float d = Random.Range(0.25f, 0.62f);
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, -0.12f, d);

            Quaternion rot = Quaternion.Euler(0f, a + Random.Range(-40f, 40f), 0f)
                           * Quaternion.Euler(Random.Range(24f, 50f), 0f, 0f);

            ProcMesh.AppendShard(b, Matrix4x4.TRS(pos, rot, Vector3.one),
                                 Random.Range(0.40f, 0.70f), Random.Range(0.42f, 0.78f),
                                 Random.Range(5, 7), 3, Random.Range(0.42f, 0.66f),
                                 0.26f, Color.white);
        }

        // ---- 3) Phan chinh: hinh dang tuy kieu da ----
        int spikes;
        float rMin, rMax, hMin, hMax, tipMin, tipMax, tiltMax, spread;

        if (style == RockStyleSpire)
        {
            spikes = Random.Range(6, 10);
            rMin = 0.17f; rMax = 0.34f;
            hMin = 0.85f; hMax = 1.60f;
            tipMin = 0.04f; tipMax = 0.14f;      // vut nhon nhu luoi dao
            tiltMax = 17f; spread = 0.50f;
        }
        else if (style == RockStyleCliff)
        {
            spikes = Random.Range(3, 6);
            rMin = 0.44f; rMax = 0.74f;
            hMin = 0.80f; hMax = 1.45f;
            tipMin = 0.34f; tipMax = 0.62f;      // dinh bang -> khoi vuong vuc
            tiltMax = 11f; spread = 0.42f;
        }
        else
        {
            spikes = Random.Range(4, 8);
            rMin = 0.50f; rMax = 0.88f;
            hMin = 0.22f; hMax = 0.46f;
            tipMin = 0.58f; tipMax = 0.86f;      // dinh gan phang -> dung len duoc
            tiltMax = 18f; spread = 0.55f;
        }

        for (int i = 0; i < spikes; i++)
        {
            float a = Random.Range(0f, 360f);
            float d = Random.Range(0f, spread);
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, -0.1f, d);

            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                           * Quaternion.Euler(Random.Range(-tiltMax, tiltMax), 0f,
                                              Random.Range(-tiltMax, tiltMax));

            // Cang ra ria cang thap, o giua cao nhat -> khoi da co dang thap dan
            float shrink = Mathf.Lerp(1f, 0.55f, d / Mathf.Max(0.01f, spread));

            ProcMesh.AppendShard(b, Matrix4x4.TRS(pos, rot, Vector3.one),
                                 Random.Range(rMin, rMax),
                                 Random.Range(hMin, hMax) * shrink,
                                 Random.Range(5, 8), 4,
                                 Random.Range(tipMin, tipMax),
                                 0.24f, Color.white);
        }

        // ---- 4) Da vun rai quanh chan ----
        int rubble = Random.Range(5, 11);
        for (int i = 0; i < rubble; i++)
        {
            float a = Random.Range(0f, 360f);
            float d = Random.Range(0.55f, 1.15f);
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, -0.06f, d);

            Quaternion rot = Quaternion.Euler(Random.Range(0f, 360f),
                                              Random.Range(0f, 360f),
                                              Random.Range(0f, 360f));

            ProcMesh.AppendShard(b, Matrix4x4.TRS(pos, rot, Vector3.one),
                                 Random.Range(0.09f, 0.22f), Random.Range(0.12f, 0.34f),
                                 Random.Range(4, 7), 2, Random.Range(0.25f, 0.65f),
                                 0.32f, Color.white);
        }

        Random.state = state;
        var mesh = b.Build("rock_" + style + "_" + seed);

        // Ban do phap tuyen chi chay dung khi luoi CO TIEP TUYEN. Thieu no thi
        // may khong biet huong "ngang" cua be mat, anh sang se hat sai lung tung
        // va cong sap lam mat da san sui coi nhu bo di.
        mesh.RecalculateTangents();
        return mesh;
    }

    /// <summary>
    /// Gan VA CHAM THEO DUNG HINH KHOI cho mot tang da.
    ///
    /// Truoc day da dung mot qua cau va cham ban kinh 0.8 dat o goc toa do, trong khi
    /// khoi da rong toi 1.15 va cao 1.6 - nen nhan vat di xuyen qua phan lon than da,
    /// con nhung tang nho thi khong co va cham gi ca.
    ///
    /// Dung dung mieng hinh cua tang da lam va cham thi vua chan duoc nguoi di xuyen,
    /// vua cho phep treo len nhung mat da nghieng thoai.
    /// </summary>
    public static void AddRockCollider(GameObject go, Mesh mesh)
    {
        if (go == null || mesh == null) return;

        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = false;      // da nam yen mot cho nen dung duoc hinh lom
    }

    // Vai kieu da dung san de dung chung cho ca man, khoi phai dung lai tung khoi
    static Mesh[] rockVariants;

    /// <summary>Lay mot kieu khoi da da dung san (dung chung cho ca man cho nhe may).</summary>
    public static Mesh RockVariant(int index)
    {
        if (rockVariants == null || rockVariants.Length == 0 || rockVariants[0] == null)
        {
            rockVariants = new Mesh[9];
            for (int i = 0; i < rockVariants.Length; i++)
                rockVariants[i] = RockClusterMesh(500 + i * 37, i % 3);
        }
        return rockVariants[((index % rockVariants.Length) + rockVariants.Length) % rockVariants.Length];
    }

    // ================================================================
    //  VACH DA BAO QUANH DAU TRUONG
    // ================================================================

    static void BuildBoundary(Transform parent, float radius)
    {
        var root = new GameObject("Cliffs");
        root.transform.SetParent(parent, false);

        int count = 76;
        for (int i = 0; i < count; i++)
        {
            float a = (float)i / count * 360f + Random.Range(-2.2f, 2.2f);
            float r = radius + Random.Range(-0.6f, 2.4f);
            Vector3 pos = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, r);
            pos.y = Random.Range(-1.2f, 0.4f);

            float scale = Random.Range(2.6f, 5.4f);
            var mesh = RockVariant(i);
            var go = ProcMesh.Part("Cliff" + i, root.transform, mesh, Mats.Rock, pos,
                                   Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f)),
                                   new Vector3(scale * Random.Range(0.85f, 1.30f), scale * Random.Range(1.0f, 1.45f), scale * Random.Range(0.85f, 1.30f)),
                                   true);
            AddRockCollider(go, mesh);
        }
    }

    // ================================================================
    //  VAT TRANG TRI
    // ================================================================

    static void BuildProps(Transform parent, float radius)
    {
        var root = new GameObject("Props");
        root.transform.SetParent(parent, false);

        // Da tang phu reu rai rac
        for (int i = 0; i < 42; i++)
        {
            Vector3 p = RandomPoint(radius * 0.92f, 6f);
            float sc = Random.Range(0.75f, 2.2f);
            var mesh = RockVariant(i + 4);
            var go = ProcMesh.Part("Rock" + i, root.transform, mesh, Mats.Rock,
                                   p + Vector3.down * sc * 0.35f,
                                   Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-20f, 20f)),
                                   Vector3.one * sc, true);
            AddRockCollider(go, mesh);
        }

        // Bui ram thap
        for (int i = 0; i < 40; i++)
        {
            Vector3 p = RandomPoint(radius * 0.93f, 5f);
            BuildBush(root.transform, p, i);
        }

        // Cay: phan lon con la, xen vai cay chet khang khiu
        for (int i = 0; i < 22; i++)
        {
            Vector3 p = RandomPoint(radius * 0.9f, 7f);
            if (Random.value < 0.62f) BuildLeafyTree(root.transform, p, i);
            else BuildDeadTree(root.transform, p, i);
        }

        // Cot da do nat kieu tan tich
        for (int i = 0; i < 11; i++)
        {
            Vector3 p = RandomPoint(radius * 0.85f, 8f);
            BuildPillar(root.transform, p, Random.Range(1.6f, 4.6f), i);
        }

        // Vai khuc xuong nam lan trong co
        for (int i = 0; i < 16; i++)
        {
            Vector3 p = RandomPoint(radius * 0.9f, 4f);
            var mesh = ProcMesh.Taper(0.045f, 0.038f, Random.Range(0.3f, 0.6f), 6, true, Color.white);
            ProcMesh.Part("Bone" + i, root.transform, mesh, Mats.BoneMat,
                          p + Vector3.up * 0.04f,
                          Quaternion.Euler(90f, Random.Range(0f, 360f), 0f), Vector3.one, false);
        }

        // Vai lo lua con am o khu trung tam
        for (int i = 0; i < 3; i++)
        {
            float a = i / 3f * 360f + 18f;
            Vector3 p = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, Random.Range(10f, 15f));
            p.y = VfxFactory.GroundY(p);
            BuildBrazier(root.transform, p);
        }
    }

    /// <summary>Mot bui ram thap: nhieu chum la chong len nhau thanh khoi tron.</summary>
    public static GameObject BuildBush(Transform parent, Vector3 pos, int seed)
    {
        var go = new GameObject("Bush" + seed);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        float size = Random.Range(0.75f, 1.7f);
        var b = new ProcMesh.Builder();

        // Mau nen cua ca bui: cho xanh non, cho xanh sam
        float tone = Random.value;
        var baseTint = Color.Lerp(new Color(0.62f, 0.78f, 0.48f),
                                  new Color(0.95f, 0.95f, 0.70f), tone);

        int clumps = Mathf.RoundToInt(Random.Range(9f, 16f) * size);
        for (int i = 0; i < clumps; i++)
        {
            // Rai cac chum la trong mot khoi hinh vom
            Vector2 flat = Random.insideUnitCircle * size * 0.55f;
            float h = Random.Range(0.05f, 0.75f) * size;

            var tint = baseTint * Random.Range(0.72f, 1.15f);
            tint.a = 1f;

            // Chum dung
            ProcMesh.AppendFoliage(b, new Vector3(flat.x, h * 0.15f, flat.y),
                                   size * Random.Range(0.55f, 0.95f),
                                   size * Random.Range(0.42f, 0.72f),
                                   2, Random.Range(0f, 180f), tint);

            // Vai chum nam ngang phu len dinh cho vom bui kin
            if (Random.value < 0.45f)
                ProcMesh.AppendLeafCard(b, new Vector3(flat.x, h + size * 0.18f, flat.y),
                                        Quaternion.Euler(Random.Range(-28f, 28f),
                                                         Random.Range(0f, 360f),
                                                         Random.Range(-28f, 28f)),
                                        size * 0.7f, size * 0.7f, tint);
        }

        var mesh = b.Build("bush");
        ProcMesh.Part("Leaves", go.transform, mesh, Mats.Leaf, Vector3.zero,
                      Quaternion.identity, Vector3.one, false);

        return go;
    }

    // ================================================================
    //  CAY COI
    // ================================================================

    /// <summary>
    /// THAN CAY SAN SUI.
    ///
    /// Dung tung tang mot chu khong phai mot hinh tru tron lu:
    /// - Moi CANH duoc gieo mot do day rieng va GIU NGUYEN suot chieu cao,
    ///   nen tao ra nhung song doc chay tu goc len ngon dung nhu song vo cay.
    /// - Ban kinh moi tang lai u nan mot chut -> than cay cho phinh cho thot.
    /// - Sat goc phinh ra thanh be re, len den ngon thi VUT NHON hoan toan.
    /// - Than nghieng va uon luon dan theo do cao.
    /// </summary>
    /// <summary>
    /// TAM CUA THAN CAY o mot do cao (t = 0 la goc, t = 1 la ngon).
    ///
    /// Than cay nghieng va uon luon, nen tam cua no KHONG nam tren truc thang dung.
    /// Moi thu gan vao than (canh, nhanh con, tan la) deu phai lay vi tri tu ham nay,
    /// neu khong se bi lo lung giua khong trung.
    /// </summary>
    public static Vector3 TrunkCentreAt(float t, float height, Vector2 lean, int seed)
    {
        float nz = seed * 0.317f;
        float k = Mathf.Pow(t, 1.45f);

        Vector3 c = new Vector3(lean.x * k, height * t, lean.y * k);

        // Tru di gia tri o t = 0 de GOC than luon nam dung tai goc toa do
        c.x += (Mathf.PerlinNoise(nz + 3f, t * 2.6f) - Mathf.PerlinNoise(nz + 3f, 0f)) * height * 0.075f;
        c.z += (Mathf.PerlinNoise(nz + 9f, t * 2.6f) - Mathf.PerlinNoise(nz + 9f, 0f)) * height * 0.075f;

        return c;
    }

    public static Mesh TrunkMesh(float baseRadius, float tipRadius, float height,
                                 int sides, int rings, Vector2 lean, int seed)
    {
        var state = Random.state;
        Random.InitState(seed);

        sides = Mathf.Max(5, sides);
        rings = Mathf.Max(3, rings);

        // Song doc: do day rieng cua tung canh, giu nguyen ca chieu cao
        var flute = new float[sides];
        for (int s = 0; s < sides; s++) flute[s] = 1f + Random.Range(-0.22f, 0.22f);

        float nz = seed * 0.317f;
        var b = new ProcMesh.Builder();
        int cols = sides + 1;

        for (int r = 0; r <= rings; r++)
        {
            float t = r / (float)rings;

            // Thot dan len ngon; tang cuoi cung thu ve mot diem -> ngon vut nhon
            float rad = r == rings ? 0f
                      : Mathf.Lerp(baseRadius, tipRadius, Mathf.Pow(t, 0.72f));

            // Be re phinh ra o sat goc
            rad *= 1f + Mathf.Pow(Mathf.Clamp01(1f - t * 5f), 2f) * 0.6f;

            // U nan doc than
            rad *= 1f + (Mathf.PerlinNoise(nz, t * 6f) - 0.5f) * 0.36f;

            // Than nghieng va uon luon dan len cao
            Vector3 centre = TrunkCentreAt(t, height, lean, seed);

            for (int s = 0; s <= sides; s++)
            {
                float a = (s % sides) / (float)sides * Mathf.PI * 2f;
                float rr = rad * flute[s % sides];
                Vector3 p = centre + new Vector3(Mathf.Cos(a) * rr, 0f, Mathf.Sin(a) * rr);
                b.AddVertex(p, new Vector3(Mathf.Cos(a), 0.12f, Mathf.Sin(a)).normalized,
                            new Vector2(s / (float)sides, t), Color.white);
            }
        }

        for (int r = 0; r < rings; r++)
            for (int s = 0; s < sides; s++)
            {
                int i0 = r * cols + s;
                b.AddTriangle(i0, i0 + cols, i0 + 1);
                b.AddTriangle(i0 + 1, i0 + cols, i0 + cols + 1);
            }

        Random.state = state;
        return b.Build("trunk", true);
    }

    /// <summary>
    /// Mot canh cay: cung dung nhu than nhung nho va cong hon.
    /// Do cong (<paramref name="lean"/>) do BEN NGOAI truyen vao, de cho goi con
    /// tinh duoc duong tam cua canh ma gan nhanh con va tan la cho dung cho.
    /// </summary>
    static GameObject AddBranch(Transform parent, Vector3 at, Quaternion rot,
                                float radius, float length, Vector2 lean, int seed)
    {
        var mesh = TrunkMesh(radius, radius * 0.18f, length, 6, 7, lean, seed);
        return ProcMesh.Part("Branch", parent, mesh, Mats.Bark, at, rot, Vector3.one, true);
    }

    /// <summary>Cay con song: than san sui, canh toe ra co nhanh con, tan la day.</summary>
    public static GameObject BuildLeafyTree(Transform parent, Vector3 pos, int seed)
    {
        var go = new GameObject("Tree" + seed);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        float h = Random.Range(4.2f, 7.4f);
        float baseR = Random.Range(0.26f, 0.40f);

        // Nho lai do cong va hat giong cua than de con gan canh vao dung cho
        var trunkLean = new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f));
        int trunkSeed = seed * 31 + 7;

        var trunk = TrunkMesh(baseR, 0.045f, h, 10, 14, trunkLean, trunkSeed);
        ProcMesh.Part("Trunk", go.transform, trunk, Mats.Bark, Vector3.zero);

        // Mau tan la cua ca cay
        var baseTint = Color.Lerp(new Color(0.52f, 0.68f, 0.36f),
                                  new Color(0.88f, 0.88f, 0.58f), Random.value);

        var leaves = new ProcMesh.Builder();
        int branches = Random.Range(5, 8);

        for (int i = 0; i < branches; i++)
        {
            float bh = Random.Range(h * 0.42f, h * 0.90f);
            float yaw = i / (float)branches * 360f + Random.Range(-32f, 32f);
            float pitch = Random.Range(34f, 66f);
            float len = Random.Range(1.1f, 2.3f) * Mathf.Lerp(1.15f, 0.7f, bh / h);

            Quaternion rot = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(pitch, 0f, 0f);

            // Goc canh phai nam DUNG TREN THAN, khong phai tren truc thang dung.
            // Lui them mot chut vao trong long than cho khoi ho khe.
            Vector3 at = TrunkCentreAt(bh / h, h, trunkLean, trunkSeed)
                       - rot * new Vector3(0f, baseR * 0.6f, 0f);

            float bRadius = Random.Range(0.07f, 0.115f);
            var bLean = new Vector2(Random.Range(-0.35f, 0.35f), Random.Range(-0.35f, 0.35f)) * len;
            int bSeed = seed * 91 + i;

            var brGo = AddBranch(go.transform, at, rot, bRadius, len, bLean, bSeed);
            brGo.name = "Branch" + i;

            // Nhanh con moc ra tu giua canh chinh - bam theo duong tam cua canh
            int twigs = Random.Range(1, 3);
            for (int k = 0; k < twigs; k++)
            {
                float f = Random.Range(0.45f, 0.8f);
                Vector3 tAt = at + rot * TrunkCentreAt(f, len, bLean, bSeed);
                Quaternion tRot = rot * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                                      * Quaternion.Euler(Random.Range(26f, 58f), 0f, 0f);
                AddBranch(go.transform, tAt, tRot, Random.Range(0.028f, 0.05f),
                          len * Random.Range(0.35f, 0.6f),
                          new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)) * len,
                          seed * 191 + i * 7 + k).name = "Twig" + i + "_" + k;
            }

            // Tan la rai DOC theo canh chu khong don cuc o dau canh
            int cards = Random.Range(11, 17);
            for (int c = 0; c < cards; c++)
            {
                float f = Random.Range(0.42f, 1.05f);
                Vector3 lAt = at + rot * TrunkCentreAt(f, len, bLean, bSeed)
                            + Random.insideUnitSphere * len * 0.18f;

                float size = Mathf.Lerp(0.95f, 0.5f, Mathf.Clamp01(f)) * Random.Range(0.8f, 1.25f);
                var tint = baseTint * Random.Range(0.62f, 1.18f);
                tint.a = 1f;

                ProcMesh.AppendLeafCard(leaves, lAt,
                                        Quaternion.Euler(Random.Range(-45f, 45f),
                                                         Random.Range(0f, 360f),
                                                         Random.Range(-45f, 45f)),
                                        size, size, tint);
            }
        }

        // Chum la o ngon cay
        for (int k = 0; k < 14; k++)
        {
            Vector3 off = Random.insideUnitSphere * 1.15f;
            off.y *= 0.7f;
            var tint = baseTint * Random.Range(0.62f, 1.18f);
            tint.a = 1f;
            ProcMesh.AppendLeafCard(leaves, TrunkCentreAt(0.96f, h, trunkLean, trunkSeed) + off,
                                    Quaternion.Euler(Random.Range(-45f, 45f),
                                                     Random.Range(0f, 360f),
                                                     Random.Range(-45f, 45f)),
                                    Random.Range(0.6f, 1.0f), Random.Range(0.6f, 1.0f), tint);
        }

        ProcMesh.Part("Leaves", go.transform, leaves.Build("treeleaves"), Mats.Leaf,
                      Vector3.zero, Quaternion.identity, Vector3.one, false);

        var col = go.AddComponent<CapsuleCollider>();
        col.height = h;
        col.radius = baseR * 0.95f;
        col.center = new Vector3(0f, h * 0.5f, 0f);
        return go;
    }

    public static GameObject BuildPillar(Transform parent, Vector3 pos, float height, int seed)
    {
        var go = new GameObject("Pillar" + seed);
        go.transform.SetParent(parent, false);
        go.transform.position = pos + Vector3.down * 0.3f;
        go.transform.rotation = Quaternion.Euler(Random.Range(-7f, 7f), Random.Range(0f, 360f), Random.Range(-7f, 7f));

        var baseMesh = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.62f, 0f),
            new Vector2(0.58f, 0.22f),
            new Vector2(0.44f, 0.30f),
        }, 12, true, true, Color.white, "pbase");
        baseMesh.RecalculateTangents();   // can cho ban do phap tuyen cua da
        ProcMesh.Part("Base", go.transform, baseMesh, Mats.Rock, Vector3.zero);

        // Than cot da vo dau
        var shaft = ProcMesh.Taper(0.40f, 0.34f, height, 12, false, Color.white);
        ProcMesh.Displace(shaft, 0.035f, 2.2f, seed);
        shaft.RecalculateTangents();      // can cho ban do phap tuyen cua da
        ProcMesh.Part("Shaft", go.transform, shaft, Mats.Rock, new Vector3(0f, 0.28f, 0f));

        var col = go.AddComponent<CapsuleCollider>();
        col.height = height + 0.6f;
        col.radius = 0.42f;
        col.center = new Vector3(0f, (height + 0.6f) * 0.5f, 0f);

        // Manh vo nam duoi chan cot
        for (int i = 0; i < 3; i++)
        {
            Vector3 p = new Vector3(Random.Range(-1.6f, 1.6f), 0.1f, Random.Range(-1.6f, 1.6f));
            ProcMesh.Part("Chunk" + i, go.transform, RockVariant(seed * 7 + i),
                          Mats.Rock, p, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                          Vector3.one * Random.Range(0.25f, 0.55f), true);
        }

        return go;
    }

    /// <summary>Cay chet: than san sui khang khiu, canh gay toe ra nhieu nhanh nho.</summary>
    public static GameObject BuildDeadTree(Transform parent, Vector3 pos, int seed)
    {
        var go = new GameObject("DeadTree" + seed);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        float h = Random.Range(3.4f, 6.6f);
        float baseR = Random.Range(0.20f, 0.32f);

        var trunkLean = new Vector2(Random.Range(-0.85f, 0.85f), Random.Range(-0.85f, 0.85f));
        int trunkSeed = seed * 53 + 11;

        var trunk = TrunkMesh(baseR, 0.03f, h, 9, 13, trunkLean, trunkSeed);
        ProcMesh.Part("Trunk", go.transform, trunk, Mats.Bark, Vector3.zero);

        int branches = Random.Range(4, 8);
        for (int i = 0; i < branches; i++)
        {
            float bh = Random.Range(h * 0.40f, h * 0.94f);
            float yaw = i / (float)branches * 360f + Random.Range(-38f, 38f);
            float pitch = Random.Range(42f, 80f);
            float len = Random.Range(0.8f, 2.0f) * Mathf.Lerp(1.15f, 0.65f, bh / h);

            Quaternion rot = Quaternion.Euler(0f, yaw, 0f) * Quaternion.Euler(pitch, 0f, 0f);

            // Goc canh nam dung tren than (than cay chet cong nhieu nen cang phai chinh)
            Vector3 at = TrunkCentreAt(bh / h, h, trunkLean, trunkSeed)
                       - rot * new Vector3(0f, baseR * 0.6f, 0f);

            var bLean = new Vector2(Random.Range(-0.35f, 0.35f), Random.Range(-0.35f, 0.35f)) * len;
            int bSeed = seed * 77 + i;

            AddBranch(go.transform, at, rot, Random.Range(0.055f, 0.09f), len,
                      bLean, bSeed).name = "Branch" + i;

            // Nhanh con gay khuc - cay chet thi canh chia nho nhieu hon
            int twigs = Random.Range(1, 4);
            for (int k = 0; k < twigs; k++)
            {
                float f = Random.Range(0.4f, 0.85f);
                Vector3 tAt = at + rot * TrunkCentreAt(f, len, bLean, bSeed);
                Quaternion tRot = rot * Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                                      * Quaternion.Euler(Random.Range(30f, 70f), 0f, 0f);
                AddBranch(go.transform, tAt, tRot, Random.Range(0.02f, 0.042f),
                          len * Random.Range(0.3f, 0.55f),
                          new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)) * len,
                          seed * 157 + i * 9 + k).name = "Twig" + i + "_" + k;
            }
        }

        var col = go.AddComponent<CapsuleCollider>();
        col.height = h;
        col.radius = baseR * 0.95f;
        col.center = new Vector3(0f, h * 0.5f, 0f);
        return go;
    }

    public static GameObject BuildBrazier(Transform parent, Vector3 pos)
    {
        var go = new GameObject("Brazier");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;

        // Ba chan sat
        for (int i = 0; i < 3; i++)
        {
            var leg = ProcMesh.Taper(0.05f, 0.03f, 0.75f, 6, false, Color.white);
            ProcMesh.Part("Leg" + i, go.transform, leg, Mats.Steel, Vector3.zero,
                          Quaternion.Euler(0f, i * 120f, 0f) * Quaternion.Euler(-14f, 0f, 0f),
                          Vector3.one, true);
        }

        // Chau lua
        var bowl = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.10f, 0.00f),
            new Vector2(0.34f, 0.14f),
            new Vector2(0.42f, 0.30f),
            new Vector2(0.40f, 0.34f),
            new Vector2(0.30f, 0.20f),
        }, 16, true, false, Color.white, "bowl");
        ProcMesh.Part("Bowl", go.transform, bowl, Mats.Steel, new Vector3(0f, 0.72f, 0f));

        // Than cui chay do
        ProcMesh.Part("Coals", go.transform, ProcMesh.Sphere(0.22f, 10, 6, 0.5f, Color.white),
                      Mats.Glow("Coals", new Color(0.35f, 0.10f, 0.03f), new Color(1f, 0.35f, 0.08f), 2.5f),
                      new Vector3(0f, 0.90f, 0f));

        VfxFactory.Brazier(pos + new Vector3(0f, 0.95f, 0f), go.transform, 1f);

        var col = go.AddComponent<CapsuleCollider>();
        col.height = 1.4f;
        col.radius = 0.35f;
        col.center = new Vector3(0f, 0.7f, 0f);
        return go;
    }

    /// <summary>Mot tang da rieng le (dung de nuong thanh prefab).</summary>
    public static GameObject BuildRockProp(Transform parent, int seed, float scale, bool collider)
    {
        // Moi prefab mot kieu rieng: ngon nhon / vach vuong / dong da phien nam bet
        var mesh = RockClusterMesh(1200 + seed * 61, seed % 3);
        var go = ProcMesh.Part("Rock", parent, mesh, Mats.Rock, Vector3.zero,
                               Quaternion.identity, Vector3.one * scale, true);
        if (collider) AddRockCollider(go, mesh);

        // Bu lai do lap hoa tiet theo co khoi da, khong thi khoi da to se
        // bi keo gian hoa tiet ra thanh mat lang bong
        go.AddComponent<RockTexScale>();
        return go;
    }

    /// <summary>Mot khuc xuong nam duoi dat.</summary>
    public static GameObject BuildBoneProp(Transform parent, int seed)
    {
        Random.InitState(seed);
        var mesh = ProcMesh.Taper(0.045f, 0.038f, Random.Range(0.3f, 0.6f), 6, true, Color.white);
        return ProcMesh.Part("BoneProp", parent, mesh, Mats.BoneMat, Vector3.zero,
                             Quaternion.Euler(90f, 0f, 0f), Vector3.one, false);
    }

    static Vector3 RandomPoint(float maxRadius, float minRadius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 c = Random.insideUnitCircle * maxRadius;
            if (c.magnitude < minRadius) continue;
            Vector3 p = new Vector3(c.x, 0f, c.y);
            p.y = VfxFactory.GroundY(p);
            return p;
        }
        return new Vector3(minRadius, 0f, 0f);
    }
}
