// Hieu ung anh sang loe (bloom) tu viet - khong can cai package Post Processing.
// Pass 0: loc vung sang. Pass 1: lam nhoe. Pass 2: ghep lai + toi goc man hinh.
Shader "Diablo25D/Bloom"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;
    sampler2D _BloomTex;
    float _Threshold;
    float _SoftKnee;
    float _Intensity;
    float2 _BlurDir;
    float _Vignette;

    struct v2f_b { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

    v2f_b vertB (appdata_img v)
    {
        v2f_b o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    // ---- Pass 0: chi giu lai phan sang hon nguong ----
    fixed4 fragPrefilter (v2f_b i) : SV_Target
    {
        fixed4 c = tex2D(_MainTex, i.uv);
        float br = max(c.r, max(c.g, c.b));
        float knee = _Threshold * _SoftKnee + 1e-5;
        float soft = clamp(br - _Threshold + knee, 0, 2 * knee);
        soft = soft * soft / (4 * knee);
        float w = max(soft, br - _Threshold) / max(br, 1e-5);
        return fixed4(c.rgb * w, 1);
    }

    // ---- Pass 1: lam nhoe theo mot chieu (goi 2 lan: ngang + doc) ----
    fixed4 fragBlur (v2f_b i) : SV_Target
    {
        float2 d = _BlurDir * _MainTex_TexelSize.xy;
        fixed4 s = tex2D(_MainTex, i.uv) * 0.2270270;
        s += tex2D(_MainTex, i.uv + d * 1.3846154) * 0.3162162;
        s += tex2D(_MainTex, i.uv - d * 1.3846154) * 0.3162162;
        s += tex2D(_MainTex, i.uv + d * 3.2307692) * 0.0702702;
        s += tex2D(_MainTex, i.uv - d * 3.2307692) * 0.0702702;
        return s;
    }

    // ---- Pass 2: ghep anh goc + vung sang da nhoe ----
    fixed4 fragCombine (v2f_b i) : SV_Target
    {
        fixed4 base = tex2D(_MainTex, i.uv);
        fixed4 bloom = tex2D(_BloomTex, i.uv);
        fixed3 col = base.rgb + bloom.rgb * _Intensity;

        float2 q = i.uv - 0.5;
        float vig = 1.0 - saturate(dot(q, q) * _Vignette);
        col *= lerp(1.0, vig, 0.9);

        return fixed4(col, base.a);
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Pass 0
        Pass
        {
            CGPROGRAM
            #pragma vertex vertB
            #pragma fragment fragPrefilter
            ENDCG
        }

        // Pass 1
        Pass
        {
            CGPROGRAM
            #pragma vertex vertB
            #pragma fragment fragBlur
            ENDCG
        }

        // Pass 2
        Pass
        {
            CGPROGRAM
            #pragma vertex vertB
            #pragma fragment fragCombine
            ENDCG
        }
    }
    FallBack Off
}
