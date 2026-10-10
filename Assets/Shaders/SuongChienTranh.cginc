// SUONG CHIEN TRANH (tam nhin 25 m, nguoi dung 10/10/2026 - kieu StarCraft 2): ngoai tam nhin cua nhan vat (va dong doi che do Doi)
// canh vat van THAY nhung TOI, nhat mau, phu may xam xanh troi cham; quai / nguoi / hieu ung ngoai tam bi an hoan toan (TamNhin.cs).
// Moi shader canh vat goi TN_Ap(mau, viTriTheGioi) o cuoi - tinh ngay tren diem anh, KHONG them luot ve nao (dien thoai).
// Gia tri do TamNhin.cs dat bang Shader.SetGlobal*; _TN_Bat = 0 (man chinh, Editor) thi khong lam gi.
#ifndef SUONG_CHIEN_TRANH_INCLUDED
#define SUONG_CHIEN_TRANH_INCLUDED

float4 _TN_Tam[4];          // xy = toa do (x, z) the gioi cua nguon nhin, w = 1 neu dang dung
float _TN_BanKinh;          // 25 m
float _TN_Mem;              // be rong mep mo dan (m)
float _TN_Bat;              // 0 = tat
float _TN_DoToi;            // phan sang con lai trong suong (0.3 = toi con 30%)
float _TN_NhatMau;          // 0..1 muc mat mau
float4 _TN_MauMay;          // mau may troi (a = do dam)
float _TN_ThoiGian;         // dong ho rieng (TamNhin dat) - may troi

float TN_Bam(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
float TN_Nhieu(float2 p)
{
    float2 i = floor(p), f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(TN_Bam(i), TN_Bam(i + float2(1, 0)), f.x), lerp(TN_Bam(i + float2(0, 1)), TN_Bam(i + float2(1, 1)), f.x), f.y);
}

// 0 = trong tam nhin, 1 = trong suong
float TN_HeSo(float3 wp)
{
    if (_TN_Bat < 0.5) return 0.0;
    float d = 1e6;
    for (int i = 0; i < 4; i++)
    {
        if (_TN_Tam[i].w > 0.5) d = min(d, distance(wp.xz, _TN_Tam[i].xy));
    }
    return smoothstep(_TN_BanKinh - _TN_Mem, _TN_BanKinh, d);
}

// Mau sau khi phu suong (mau = mau da chieu sang)
float3 TN_MauSuong(float3 mau, float3 wp)
{
    float2 q = wp.xz * 0.09 + float2(_TN_ThoiGian * 0.035, _TN_ThoiGian * 0.02);
    float may = TN_Nhieu(q) * 0.65 + TN_Nhieu(q * 2.3 + 7.1) * 0.35;
    float sang = dot(mau, float3(0.299, 0.587, 0.114));
    float3 m = lerp(mau, sang.xxx, _TN_NhatMau) * _TN_DoToi;
    return lerp(m, _TN_MauMay.rgb, smoothstep(0.35, 0.85, may) * _TN_MauMay.a);
}

void TN_Ap(inout float3 mau, float3 wp)
{
    float f = TN_HeSo(wp);
    if (f > 0.0) mau = lerp(mau, TN_MauSuong(mau, wp), f);
}

void TN_ApCong(inout float3 mau, float3 wp)
{
    // luot cong den (ForwardAdd): chi lam toi theo suong, khong cong mau may (khong thi moi den cong them mot lop may)
    float f = TN_HeSo(wp);
    mau *= lerp(1.0, _TN_DoToi, f);
}

// Surface shader co finalcolor thi Unity KHONG tu sinh code suong khoang cach (RenderSettings.fog) nua - ap lai o day,
// khoang cach = do dai tu may quay (xap xi do sau). Goi TRUOC TN_Ap.
void TN_SuongUnity(inout float3 mau, float3 wp, bool luotCong)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
        float z = length(_WorldSpaceCameraPos.xyz - wp);
        UNITY_CALC_FOG_FACTOR_RAW(z);
        float k = saturate(unityFogFactor);
        mau = luotCong ? mau * k : lerp(unity_FogColor.rgb, mau, k);
    #endif
}

#endif
