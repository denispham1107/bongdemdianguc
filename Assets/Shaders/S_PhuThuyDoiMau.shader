// PHU THUY DOI MAU AO (09/10/2026, nguoi dung: "quan ao cac nguoi choi khac nhau co mau khac nhau; che do Doi cung doi cung mau").
//
// Thay cho Standard tren vat lieu Player_PhuThuy: cung anh mau / normal / metallic-smoothness nhu Standard (Meshy), them buoc
// DOI MAU AO: vung ao choang mau TIM trong anh (sac do ~277-324 do, do bao hoa >= 0,2 - do bang histogram anh goc: da hong,
// vien vang, do, toc trang deu nam ngoai) duoc thay bang _MauAo, GIU sang toi + van vai cua anh goc (nhan theo do sang
// tung diem so voi do sang TB vung tim 0,171; he so 0,72 - nguoi dung chon "sang hon ~20%" so voi 0,6). _MauAo.a = 0 -> giu nguyen anh goc (man chinh, choi mot minh).
// Mau dat tung nhan vat bang MaterialPropertyBlock (MauAoNhanVat.cs) - khong tao vat lieu moi.
Shader "Diablo25D/PhuThuyDoiMau"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
        _GlossMapScale ("Smoothness Scale", Range(0,1)) = 1.0
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)

        _MauAo ("Mau ao (a = muc doi)", Color) = (1,1,1,0)
        _HueTu ("Sac do bat dau (0..1)", Range(0,1)) = 0.77
        _HueDen ("Sac do ket thuc (0..1)", Range(0,1)) = 0.90
        _MemHue ("Do mem bien sac do", Range(0.001,0.2)) = 0.03
        _BaoHoaToiThieu ("Do bao hoa toi thieu", Range(0,1)) = 0.2
        _SangThamChieu ("Do sang TB vung tim", Float) = 0.171
        _DoSangAo ("He so sang ao moi", Range(0,2)) = 0.72
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #include "UnityStandardUtils.cginc"

        sampler2D _MainTex, _BumpMap, _MetallicGlossMap;
        fixed4 _Color;
        half _BumpScale, _GlossMapScale;
        half4 _EmissionColor;
        fixed4 _MauAo;
        half _HueTu, _HueDen, _MemHue, _BaoHoaToiThieu, _SangThamChieu, _DoSangAo;

        struct Input { float2 uv_MainTex; };

        // RGB -> (sac do 0..1, bao hoa, gia tri)
        float3 RgbSangHsv(float3 c)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
            float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
            float d = q.x - min(q.w, q.y);
            float e = 1.0e-10;
            return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
        }

        // Muc "la vai ao tim" 0..1 cua mot diem anh
        half MatNaAo(float3 c)
        {
            float3 hsv = RgbSangHsv(c);
            half trong = smoothstep(_HueTu - _MemHue, _HueTu + _MemHue, hsv.x)
                       * (1.0 - smoothstep(_HueDen - _MemHue, _HueDen + _MemHue, hsv.x));
            half bh = smoothstep(_BaoHoaToiThieu - 0.06, _BaoHoaToiThieu + 0.06, hsv.y);
            return trong * bh;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            if (_MauAo.a > 0.001)
            {
                half m = MatNaAo(c.rgb) * _MauAo.a;
                half sang = dot(c.rgb, half3(0.299, 0.587, 0.114)) / _SangThamChieu;
                half3 moi = _MauAo.rgb * sang * _DoSangAo;
                c.rgb = lerp(c.rgb, moi, m);
            }
            o.Albedo = c.rgb;
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
            fixed4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = mg.r;
            o.Smoothness = mg.a * _GlossMapScale;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Standard"
}
