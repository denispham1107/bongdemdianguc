// Vat lieu vai MONG nhin duoc CA HAI MAT (ao choang, mu trum, vay ao).
// Giong shader Standard nhung khong cat mat sau (Cull Off).
Shader "Diablo25D/ClothTwoSided"
{
    Properties
    {
        _Color ("Mau", Color) = (1,1,1,1)
        _MainTex ("Hoa tiet", 2D) = "white" {}
        _Glossiness ("Do bong", Range(0,1)) = 0.2
        _Metallic ("Do kim loai", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        struct Input { float2 uv_MainTex; float3 viewDir; INTERNAL_DATA };
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
