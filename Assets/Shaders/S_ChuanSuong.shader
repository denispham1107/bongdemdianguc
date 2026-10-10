// STANDARD + SUONG CHIEN TRANH (10/10/2026). TamNhin.cs thay shader Standard cua canh vat Act2 (co rai, la, lo lua, sat cong rao) bang shader nay
// LUC CHAY (ban sao vat lieu - khong sua asset): cung ten thuoc tinh voi Standard nen chep vat lieu la giu nguyen gia tri. Khong dung keyword
// (vat lieu tao luc chay - bien the shader_feature bi bo khi build); co / khong anh kim loai + phat sang do TamNhin dat qua _CoAnhKim, _CoPhat.
Shader "Diablo25D/ChuanSuong"
{
    Properties
    {
        _Color ("Mau", Color) = (1,1,1,1)
        _MainTex ("Anh mau", 2D) = "white" {}
        _BumpMap ("Anh phap tuyen", 2D) = "bump" {}
        _BumpScale ("Do noi", Float) = 1
        _MetallicGlossMap ("Anh kim loai", 2D) = "white" {}
        _Metallic ("Kim loai", Range(0,1)) = 0
        _Glossiness ("Do bong", Range(0,1)) = 0.5
        _GlossMapScale ("He so bong anh", Range(0,1)) = 1
        _OcclusionMap ("Anh che sang", 2D) = "white" {}
        _OcclusionStrength ("Muc che sang", Range(0,1)) = 1
        _EmissionMap ("Anh phat sang", 2D) = "white" {}
        _EmissionColor ("Mau phat sang", Color) = (0,0,0,1)
        _CoAnhKim ("Co anh kim loai", Float) = 0
        _CoPhat ("Co phat sang", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows finalcolor:tnFinal
        #pragma target 3.0
        #pragma multi_compile_fog       // finalcolor tat code suong tu sinh -> TN_SuongUnity can keyword FOG_*
        #include "SuongChienTranh.cginc"
        sampler2D _MainTex, _BumpMap, _MetallicGlossMap, _OcclusionMap, _EmissionMap;
        fixed4 _Color, _EmissionColor;
        half _BumpScale, _Metallic, _Glossiness, _GlossMapScale, _OcclusionStrength, _CoAnhKim, _CoPhat;
        struct Input { float2 uv_MainTex; float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            fixed4 c = tex2D(_MainTex, uv) * _Color;
            o.Albedo = c.rgb;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, uv), _BumpScale);
            fixed4 mg = tex2D(_MetallicGlossMap, uv);
            o.Metallic = lerp(_Metallic, mg.r, _CoAnhKim);
            o.Smoothness = lerp(_Glossiness, mg.a * _GlossMapScale, _CoAnhKim);
            o.Occlusion = lerp(1, tex2D(_OcclusionMap, uv).g, _OcclusionStrength);
            o.Emission = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _CoPhat;
            o.Alpha = 1;
        }
        void tnFinal(Input IN, SurfaceOutputStandard o, inout fixed4 color)
        {
            #ifdef UNITY_PASS_FORWARDADD
                TN_SuongUnity(color.rgb, IN.worldPos, true);
                TN_ApCong(color.rgb, IN.worldPos);
            #else
                TN_SuongUnity(color.rgb, IN.worldPos, false);
                TN_Ap(color.rgb, IN.worldPos);
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}
