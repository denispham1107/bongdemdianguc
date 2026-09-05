using UnityEngine;

/// <summary>
/// KHO VAT LIEU (mau sac + do bong + hoa tiet) cua ca game.
/// Tat ca deu tao bang code khi game khoi dong, khong can file .mat co san.
/// </summary>
public static class Mats
{
    static Shader shStandard, shFire, shIce, shFrozen, shAdd, shAlpha, shSky, shTerrain, shFoliage;

    public static Shader Standard { get { if (shStandard == null) shStandard = Shader.Find("Standard"); return shStandard; } }
    public static Shader FireShader { get { if (shFire == null) shFire = Shader.Find("Diablo25D/Fire"); return shFire; } }
    public static Shader IceShader { get { if (shIce == null) shIce = Shader.Find("Diablo25D/Ice"); return shIce; } }
    public static Shader FrozenShader { get { if (shFrozen == null) shFrozen = Shader.Find("Diablo25D/FrozenShell"); return shFrozen; } }
    public static Shader AddShader { get { if (shAdd == null) shAdd = Shader.Find("Diablo25D/ParticleAdditive"); return shAdd; } }
    public static Shader AlphaShader { get { if (shAlpha == null) shAlpha = Shader.Find("Diablo25D/ParticleAlpha"); return shAlpha; } }
    public static Shader SkyShader { get { if (shSky == null) shSky = Shader.Find("Diablo25D/SkyGradient"); return shSky; } }
    public static Shader TerrainShader { get { if (shTerrain == null) shTerrain = Shader.Find("Diablo25D/Terrain"); return shTerrain; } }
    public static Shader FoliageShader { get { if (shFoliage == null) shFoliage = Shader.Find("Diablo25D/Foliage"); return shFoliage; } }

    // ================================================================
    //  HAM TAO NHANH
    // ================================================================

    /// <summary>Vat lieu thuong (co anh sang, do bong, kim loai).</summary>
    public static Material Solid(string name, Color color, float smoothness, float metallic,
                                 Texture2D tex, float tiling)
    {
        var m = new Material(Standard);
        m.name = name;
        m.color = color;
        m.SetFloat("_Glossiness", smoothness);
        m.SetFloat("_Metallic", metallic);
        if (tex != null)
        {
            m.mainTexture = tex;
            m.mainTextureScale = new Vector2(tiling, tiling);
        }
        return m;
    }

    public static Material Solid(string name, Color color, float smoothness, float metallic)
    {
        return Solid(name, color, smoothness, metallic, null, 1f);
    }

    /// <summary>Vat lieu TU PHAT SANG (mat quai, ngoc phep, than lua).</summary>
    public static Material Glow(string name, Color color, Color emission, float power)
    {
        var m = Solid(name, color, 0.7f, 0f);
        m.name = name;
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        m.SetColor("_EmissionColor", emission * power);
        return m;
    }

    /// <summary>Vat lieu hat cong sang (lua, tia sang).</summary>
    public static Material Additive(string name, Texture2D tex, Color tint, float intensity)
    {
        var m = new Material(AddShader);
        m.name = name;
        m.mainTexture = tex;
        m.SetColor("_TintColor", tint);
        m.SetFloat("_Intensity", intensity);
        return m;
    }

    /// <summary>
    /// Vat lieu hat FLIPBOOK trong suot: texture la mot LUOI khung hinh, tung
    /// hat chay lan luot qua cac o do trong doi song cua no.
    ///
    /// Dung shader rieng vi phai lay mau HAI khung roi tron - xem ghi chu day
    /// du o dau S_ParticleFlipbookAdd.shader.
    /// </summary>
    public static Material FlipbookAlpha(string name, Texture2D tex, Color tint)
    {
        var m = new Material(Shader.Find("Diablo25D/ParticleFlipbookAlpha"));
        m.name = name;
        m.mainTexture = tex;
        m.SetColor("_TintColor", tint);
        return m;
    }

    /// <summary>Vat lieu hat FLIPBOOK cong sang (lua, no).</summary>
    public static Material FlipbookAdd(string name, Texture2D tex, Color tint, float intensity)
    {
        var m = new Material(Shader.Find("Diablo25D/ParticleFlipbookAdd"));
        m.name = name;
        m.mainTexture = tex;
        m.SetColor("_TintColor", tint);
        m.SetFloat("_Intensity", intensity);
        return m;
    }

    /// <summary>Vat lieu hat trong suot (khoi, suong, vet tren dat).</summary>
    public static Material Alpha(string name, Texture2D tex, Color tint)
    {
        var m = new Material(AlphaShader);
        m.name = name;
        m.mainTexture = tex;
        m.SetColor("_TintColor", tint);
        return m;
    }

    /// <summary>
    /// Vat lieu CAY CO / LA: mieng vai hai mat, khoet trong theo anh, du dua theo gio.
    /// </summary>
    public static Material Foliage(string name, Texture2D tex, Color tint,
                                   float windBend, float cutoff, float ambientBoost)
    {
        var m = new Material(FoliageShader);
        m.name = name;
        m.mainTexture = tex;
        m.SetColor("_Color", tint);
        m.SetFloat("_WindBend", windBend);
        m.SetFloat("_Cutoff", cutoff);
        m.SetFloat("_AmbientBoost", ambientBoost);
        return m;
    }

    // ================================================================
    //  VAT LIEU DUNG SAN
    // ================================================================

    static Material mRobe, mRobeDark, mTrim, mSkin, mLeather, mSteel, mBone, mWood,
                    mGround, mRock, mDemon, mDemonHorn, mZombie, mRag, mGold, mGrassTuft, mLeaf, mBark,
                    mRobeLining, mPaleSkin, mWhiteHair, mGemPurple,
                    mEyeFire, mEyeIce, mEyeRed, mOrb, mFire, mFireSoft, mIce, mFrozen, mBlood;

    // ---- Ao choang phu thuy: den tham anh tim, lot trong mau tim ----
    public static Material Robe        { get { if (mRobe == null) mRobe = Solid("Robe", new Color(0.13f, 0.115f, 0.22f), 0.20f, 0f, TextureFactory.Cloth(new Color(0.07f, 0.06f, 0.13f), new Color(0.22f, 0.19f, 0.36f), 0.75f), 3f); return mRobe; } }
    public static Material RobeDark    { get { if (mRobeDark == null) mRobeDark = Solid("RobeDark", new Color(0.055f, 0.05f, 0.09f), 0.16f, 0f, TextureFactory.Cloth(new Color(0.03f, 0.03f, 0.06f), new Color(0.12f, 0.10f, 0.20f), 0.8f), 3f); return mRobeDark; } }

    /// <summary>Lop lot trong ao choang - mau tim tham.</summary>
    public static Material RobeLining  { get { if (mRobeLining == null) mRobeLining = Solid("RobeLining", new Color(0.30f, 0.16f, 0.44f), 0.26f, 0f, TextureFactory.Cloth(new Color(0.20f, 0.10f, 0.32f), new Color(0.44f, 0.26f, 0.62f), 0.7f), 3f); return mRobeLining; } }

    /// <summary>Da nguoi gia xanh xao, gan guoc.</summary>
    public static Material PaleSkin    { get { if (mPaleSkin == null) mPaleSkin = Solid("PaleSkin", new Color(0.575f, 0.565f, 0.525f), 0.14f, 0f, TextureFactory.Skin(new Color(0.63f, 0.63f, 0.59f), 0.18f, 3), 2f); return mPaleSkin; } }

    /// <summary>Toc va rau bac trang.</summary>
    public static Material WhiteHair   { get { if (mWhiteHair == null) mWhiteHair = Solid("WhiteHair", new Color(0.96f, 0.95f, 0.92f), 0.34f, 0f); return mWhiteHair; } }

    /// <summary>Ngoc tim phat sang (vien ngoc tren gay, da quy tren nguc).</summary>
    public static Material GemPurple   { get { if (mGemPurple == null) mGemPurple = Glow("GemPurple", new Color(0.42f, 0.16f, 0.62f), new Color(0.66f, 0.28f, 1f), 2.4f); return mGemPurple; } }
    public static Material Trim        { get { if (mTrim == null) mTrim = Solid("Trim", new Color(0.78f, 0.62f, 0.22f), 0.72f, 0.85f, TextureFactory.Metal(new Color(0.85f, 0.70f, 0.30f), 0.25f), 2f); return mTrim; } }
    public static Material Gold        { get { if (mGold == null) mGold = Solid("Gold", new Color(0.95f, 0.78f, 0.32f), 0.85f, 1f, TextureFactory.Metal(new Color(0.95f, 0.80f, 0.35f), 0.1f), 2f); return mGold; } }
    public static Material Skin        { get { if (mSkin == null) mSkin = Solid("Skin", new Color(0.86f, 0.70f, 0.60f), 0.28f, 0f, TextureFactory.Skin(new Color(0.80f, 0.64f, 0.55f), 0.30f, 1), 2f); return mSkin; } }
    public static Material Leather     { get { if (mLeather == null) mLeather = Solid("Leather", new Color(0.28f, 0.18f, 0.11f), 0.35f, 0f, TextureFactory.Skin(new Color(0.30f, 0.19f, 0.12f), 0.45f, 5), 3f); return mLeather; } }
    public static Material Steel       { get { if (mSteel == null) mSteel = Solid("Steel", new Color(0.62f, 0.64f, 0.68f), 0.68f, 0.95f, TextureFactory.Metal(new Color(0.60f, 0.63f, 0.68f), 0.5f), 2f); return mSteel; } }
    public static Material BoneMat     { get { if (mBone == null) mBone = Solid("Bone", new Color(0.86f, 0.83f, 0.70f), 0.30f, 0f, TextureFactory.Bone(), 2f); return mBone; } }
    public static Material Wood        { get { if (mWood == null) mWood = Solid("Wood", new Color(0.55f, 0.40f, 0.26f), 0.25f, 0f, TextureFactory.Wood(), 2f); return mWood; } }
    /// <summary>
    /// DA: sa thach nau do, mat da RAM SAN SUI.
    ///
    /// Truoc day da nhin nhu khoi be tong son mau vi hai le:
    ///   1. Anh trai o do lap 0.42 - tuc mot lan lap keo dai 2,4m, ma khoi da con bi
    ///      phong to 2-5 lan nua, thanh ra mot mat da chi thay dung mot mang mau nhat.
    ///   2. Khong co ban do phap tuyen, nen moi khe nut van da deu nam bet, khong bat sang.
    /// Gio siet do lap lai va them ban do phap tuyen + lop chi tiet.
    /// </summary>
    public static Material Rock
    {
        get
        {
            if (mRock == null)
            {
                mRock = Solid("Rock", new Color(0.47f, 0.36f, 0.28f), 0.14f, 0f,
                              TextureFactory.CliffRock(), 1.15f);

                // Ban do phap tuyen: thu lam da het tron
                mRock.EnableKeyword("_NORMALMAP");
                mRock.SetTexture("_BumpMap", TextureFactory.CliffRockNormal());
                mRock.SetFloat("_BumpScale", 1.5f);

                // Lop chi tiet lap DAY hon chong len tren: khoi da co bi phong to
                // may lan thi van con hat da li ti de nhin, khong bi mo thanh mang.
                mRock.EnableKeyword("_DETAIL_MULX2");
                mRock.SetTexture("_DetailAlbedoMap", TextureFactory.CliffRockDetail());
                mRock.SetTexture("_DetailNormalMap", TextureFactory.CliffRockNormal());
                mRock.SetFloat("_DetailNormalMapScale", 1.1f);
                mRock.SetTextureScale("_DetailAlbedoMap", new Vector2(6.5f, 6.5f));
            }
            return mRock;
        }
    }
    /// <summary>
    /// VO CAY: den dam, co song nau va mang reu xanh nhat.
    /// Hoa tiet lap DAY hon theo chieu doc than de van vo hien ro chu khong bi keo nhoe.
    /// </summary>
    public static Material Bark
    {
        get
        {
            if (mBark == null)
            {
                mBark = Solid("Bark", new Color(1.05f, 0.96f, 0.85f), 0.05f, 0f, TextureFactory.Bark(), 1f);
                mBark.mainTextureScale = new Vector2(1f, 3.2f);
            }
            return mBark;
        }
    }

    /// <summary>
    /// MAT DAT NAU DEN: mot mieng dat duy nhat tron hai lop - lop dat thit am sam mau
    /// (cho co moc) va lop dat cat kho nhat hon (nhung loi mon), theo mau dinh cua luoi.
    /// Xem shader Diablo25D/Terrain.
    /// </summary>
    public static Material Ground
    {
        get
        {
            if (mGround == null)
            {
                mGround = new Material(TerrainShader);
                mGround.name = "Ground";
                mGround.SetTexture("_GrassTex", TextureFactory.DarkSoil());
                mGround.SetTexture("_DirtTex", TextureFactory.MudGround());
                // Da nhan them 0.6 -> ca mat dat nga den them 40%
                mGround.SetColor("_GrassColor", new Color(0.69f, 0.60f, 0.516f));
                mGround.SetColor("_DirtColor", new Color(0.444f, 0.36f, 0.264f));
                mGround.SetFloat("_Tiling", 15f);
                mGround.SetFloat("_DetailTiling", 47f);
            }
            return mGround;
        }
    }

    /// <summary>Bui co moc tren mat dat.</summary>
    public static Material GrassTuft
    {
        get
        {
            if (mGrassTuft == null)
                mGrassTuft = Foliage("GrassTuft", TextureFactory.GrassTuft(),
                                     new Color(1f, 1f, 1f, 1f), 0.16f, 0.40f, 0.06f);
            return mGrassTuft;
        }
    }

    /// <summary>La cua bui ram va tan cay.</summary>
    public static Material Leaf
    {
        get
        {
            if (mLeaf == null)
                mLeaf = Foliage("Leaf", TextureFactory.LeafClump(),
                                new Color(1f, 1f, 1f, 1f), 0.09f, 0.45f, 0.08f);
            return mLeaf;
        }
    }
    public static Material DemonSkin   { get { if (mDemon == null) mDemon = Solid("DemonSkin", new Color(0.52f, 0.16f, 0.12f), 0.35f, 0f, TextureFactory.Skin(new Color(0.48f, 0.15f, 0.11f), 0.55f, 3), 2.5f); return mDemon; } }
    public static Material DemonHorn   { get { if (mDemonHorn == null) mDemonHorn = Solid("Horn", new Color(0.20f, 0.17f, 0.15f), 0.45f, 0.1f, TextureFactory.Bone(), 2f); return mDemonHorn; } }
    public static Material ZombieFlesh { get { if (mZombie == null) mZombie = Solid("Zombie", new Color(0.42f, 0.48f, 0.32f), 0.40f, 0f, TextureFactory.Skin(new Color(0.38f, 0.45f, 0.30f), 0.60f, 7), 2.5f); return mZombie; } }
    public static Material Rag         { get { if (mRag == null) mRag = Solid("Rag", new Color(0.30f, 0.26f, 0.20f), 0.15f, 0f, TextureFactory.Cloth(new Color(0.18f, 0.15f, 0.11f), new Color(0.40f, 0.35f, 0.26f), 0.9f), 3f); return mRag; } }
    public static Material Blood       { get { if (mBlood == null) mBlood = Solid("Blood", new Color(0.35f, 0.03f, 0.02f), 0.55f, 0f); return mBlood; } }

    public static Material EyeFire     { get { if (mEyeFire == null) mEyeFire = Glow("EyeFire", new Color(1f, 0.55f, 0.15f), new Color(1f, 0.45f, 0.10f), 4f); return mEyeFire; } }
    public static Material EyeIce      { get { if (mEyeIce == null) mEyeIce = Glow("EyeIce", new Color(0.30f, 0.48f, 0.78f), new Color(0.26f, 0.46f, 0.92f), 0.9f); return mEyeIce; } }
    public static Material EyeRed      { get { if (mEyeRed == null) mEyeRed = Glow("EyeRed", new Color(1f, 0.20f, 0.10f), new Color(1f, 0.12f, 0.05f), 5f); return mEyeRed; } }

    /// <summary>Ngoc phep tren dau cay gay (bang trong suot phat sang).</summary>
    public static Material Orb
    {
        get
        {
            if (mOrb == null)
            {
                mOrb = new Material(IceShader);
                mOrb.name = "Orb";
                mOrb.SetColor("_Color", new Color(0.45f, 0.80f, 1f, 0.75f));
                mOrb.SetColor("_DeepColor", new Color(0.05f, 0.30f, 0.85f, 1f));
                mOrb.SetColor("_RimColor", new Color(0.75f, 0.95f, 1f, 1f));
                mOrb.SetFloat("_Glow", 2.0f);
                mOrb.SetFloat("_Sparkle", 1.4f);
            }
            return mOrb;
        }
    }

    /// <summary>Than lua (dung cho loi qua cau lua, vong lua no).</summary>
    public static Material Fire
    {
        get
        {
            if (mFire == null)
            {
                mFire = new Material(FireShader);
                mFire.name = "Fire";
                mFire.SetColor("_CoreColor", new Color(1f, 0.96f, 0.80f));
                mFire.SetColor("_MidColor", new Color(1f, 0.52f, 0.10f));
                mFire.SetColor("_EdgeColor", new Color(0.70f, 0.09f, 0.02f));
                mFire.SetFloat("_Speed", 1.9f);
                mFire.SetFloat("_Scale", 4.0f);
                mFire.SetFloat("_Intensity", 2.4f);
            }
            return mFire;
        }
    }

    /// <summary>Lua mem hon, dung cho vien lua toa ra ngoai.</summary>
    public static Material FireSoft
    {
        get
        {
            if (mFireSoft == null)
            {
                mFireSoft = new Material(FireShader);
                mFireSoft.name = "FireSoft";
                mFireSoft.SetColor("_CoreColor", new Color(1f, 0.72f, 0.30f));
                mFireSoft.SetColor("_MidColor", new Color(1f, 0.32f, 0.06f));
                mFireSoft.SetColor("_EdgeColor", new Color(0.35f, 0.04f, 0.01f));
                mFireSoft.SetFloat("_Speed", 1.3f);
                mFireSoft.SetFloat("_Scale", 2.4f);
                mFireSoft.SetFloat("_Erosion", 0.55f);
                mFireSoft.SetFloat("_Intensity", 1.5f);
                mFireSoft.SetFloat("_Alpha", 0.75f);
            }
            return mFireSoft;
        }
    }

    /// <summary>Bang trong (manh bang, gai bang).</summary>
    public static Material Ice
    {
        get
        {
            if (mIce == null)
            {
                mIce = new Material(IceShader);
                mIce.name = "Ice";
                mIce.SetColor("_Color", new Color(0.60f, 0.84f, 1f, 0.55f));
                mIce.SetColor("_DeepColor", new Color(0.10f, 0.38f, 0.78f, 1f));
                mIce.SetColor("_RimColor", new Color(0.85f, 0.97f, 1f, 1f));
                mIce.SetFloat("_Glow", 1.35f);
            }
            return mIce;
        }
    }

    /// <summary>Lop vo bang boc quanh quai bi dong bang.</summary>
    public static Material FrozenShell()
    {
        var m = new Material(FrozenShader);
        m.name = "FrozenShell";
        return m;
    }

    /// <summary>Vat lieu rieng cho tung quai de nhap nhay khi trung don.</summary>
    public static Material Clone(Material src)
    {
        return new Material(src);
    }

    static Shader shCloth;
    static readonly System.Collections.Generic.Dictionary<Material, Material> twoSidedCache
        = new System.Collections.Generic.Dictionary<Material, Material>();

    /// <summary>Doi mot vat lieu thanh ban "nhin duoc ca hai mat" (ao choang, mu trum).</summary>
    public static Material TwoSided(Material src)
    {
        if (src == null) return null;
        Material found;
        if (twoSidedCache.TryGetValue(src, out found) && found != null) return found;

        if (shCloth == null) shCloth = Shader.Find("Diablo25D/ClothTwoSided");
        if (shCloth == null) return src;

        var m = new Material(shCloth);
        m.name = src.name + "_2Side";
        m.color = src.color;
        if (src.mainTexture != null)
        {
            m.mainTexture = src.mainTexture;
            m.mainTextureScale = src.mainTextureScale;
        }
        if (src.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", src.GetFloat("_Glossiness"));
        if (src.HasProperty("_Metallic")) m.SetFloat("_Metallic", src.GetFloat("_Metallic"));

        twoSidedCache[src] = m;
        return m;
    }
}
