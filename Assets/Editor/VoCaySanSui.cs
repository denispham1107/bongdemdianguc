using UnityEditor;
using UnityEngine;

/// <summary>
/// VO CAY SAN SUI - ANH CHUP THAT + MANG TROC (17/10/2026, nguoi dung: "vo cay san sui nhieu hon nua, than + canh con tron bong gia tao,
/// mot vai cho vo than cay bi nut troc ra cho giong that"; chon anh chup that, chi be mat, tai 3 bo Poly Haven).
///
/// Anh sinh trong Blender MCP bang <c>CongCu/Blender/anh_vo_cay_moi.py</c> (nguon CongCu/AnhVoCay/Goc/, khong vao git):
///   VoCay3_Mau (vo chinh bark_willow_02, A = do bong) · VoCay3_Phap (normal GL) · VoCay3_Trong (lop trong lo ra, bark_platanus) ·
///   VoCay3_Troc (mat na: RG phap tuyen bac vo, B do toi, A cho troc - TUYEN TINH, tat sRGB).
/// Menu nay dat kieu nhap + gan vao vat lieu chung <c>Act2_VoCay_SanSui</c> (moi cay Act2 + Man chinh dung chung).
/// Thay menu 15 cu (Act2VoCay.Dung - anh nhieu tu sinh, nhin nhu vai ran ri; ham van con trong code).
/// </summary>
public static class VoCaySanSui
{
    const string ThuMuc = "Assets/BlenderMaps/GraveyardAct2/CayDaChua/";
    const string DuongMat = ThuMuc + "Act2_VoCay_SanSui.mat";

    // Thong so da chon bang menu 143 (ThuVoCay) - doi o day roi chay lai menu 15
    public const float TiLe = 0.55f;       // vo lap moi 1,8 m ~ co that (anh 2,15 m); lap 1 m thi o cu ly game ranh nut nho qua, mip lam mo phang
    public const float KeoDoc = 1.0f;      // anh chup da co ranh doc, keo them thi meo
    public const float DoSanSui = 2.4f;
    public const float TiLeTroc = 0.45f;   // mat na lap moi 2,2 m -> mang troc 10-40 cm (3 m: moi than chi 1 mang)
    public const float DoSauTroc = 1.6f;
    public const float ToiTroc = 0.8f;

    [MenuItem("Diablo 2.5D/15. Vo cay san sui (anh chup that + mang troc)", false, 96)]
    public static void Dung()
    {
        Nhap("VoCay3_Mau", TextureImporterType.Default, true);
        Nhap("VoCay3_Phap", TextureImporterType.NormalMap, false);
        Nhap("VoCay3_Trong", TextureImporterType.Default, true);
        Nhap("VoCay3_Troc", TextureImporterType.Default, false);

        var mat = AssetDatabase.LoadAssetAtPath<Material>(DuongMat);
        if (mat == null) { Debug.LogError("[VoCaySanSui] khong thay " + DuongMat); return; }
        mat.shader = Shader.Find("Diablo25D/BarkTriplanar");
        mat.SetTexture("_MainTex", Anh("VoCay3_Mau"));
        mat.SetTexture("_BumpMap", Anh("VoCay3_Phap"));
        mat.SetTexture("_TrongTex", Anh("VoCay3_Trong"));
        mat.SetTexture("_TrocTex", Anh("VoCay3_Troc"));
        // Mau nhan TRANG: anh da mang du mau (CayChay to den qua _Color bang MaterialPropertyBlock, doc mau goc tu day)
        mat.SetColor("_Color", Color.white);
        mat.SetFloat("_TiLe", TiLe);
        mat.SetFloat("_KeoDoc", KeoDoc);
        mat.SetFloat("_DoSanSui", DoSanSui);
        mat.SetFloat("_TiLeTroc", TiLeTroc);
        mat.SetFloat("_MucTroc", 1f);
        mat.SetFloat("_DoSauTroc", DoSauTroc);
        mat.SetFloat("_ToiTroc", ToiTroc);
        mat.SetFloat("_Glossiness", 1f);
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        Debug.Log("[VoCaySanSui] da gan bo anh VoCay3 vao " + DuongMat);
    }

    static Texture2D Anh(string ten) { return AssetDatabase.LoadAssetAtPath<Texture2D>(ThuMuc + ten + ".png"); }

    static void Nhap(string ten, TextureImporterType kieu, bool srgb)
    {
        var ti = AssetImporter.GetAtPath(ThuMuc + ten + ".png") as TextureImporter;
        if (ti == null) { Debug.LogError("[VoCaySanSui] thieu anh " + ten); return; }
        ti.textureType = kieu;
        if (kieu == TextureImporterType.Default)
        {
            ti.sRGBTexture = srgb;
            ti.alphaSource = TextureImporterAlphaSource.FromInput;
            ti.alphaIsTransparency = false;
        }
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.mipmapEnabled = true;
        ti.anisoLevel = 2;
        ti.maxTextureSize = 1024;
        ti.textureCompression = TextureImporterCompression.Compressed;
        ti.SaveAndReimport();
    }
}
