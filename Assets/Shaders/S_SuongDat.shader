// TAM SUONG PHU DIA HINH (10/10/2026): luoi bam dat (TamNhin dung tu Terrain, nhac 0,12 m) ve SAU dia hinh + canh vat dac. Khong sua
// shader dia hinh cua Unity (Nature/Terrain/Standard) - giu nguyen mau dat nguoi dung da duyet. Pha mau: ket qua = man * a + mau_may
// (Blend One SrcAlpha): trong tam nhin a = 1, mau may = 0 -> khong doi gi; ngoai tam toi theo _TN_DoToi + may troi.
// Bia / cay / nha dung TRUOC tam nay (ZTest) nen khong bi to lan - chung tu phu suong trong shader rieng (SuongChienTranh.cginc).
Shader "Diablo25D/SuongDat"
{
    SubShader
    {
        Tags { "Queue" = "Geometry+450" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Blend One SrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "SuongChienTranh.cginc"
            struct v2f { float4 pos : SV_POSITION; float3 w : TEXCOORD0; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.w = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(o.w);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float f = TN_HeSo(i.w);
                if (f <= 0.0) return fixed4(0, 0, 0, 1);
                float2 q = i.w.xz * 0.09 + float2(_TN_ThoiGian * 0.035, _TN_ThoiGian * 0.02);
                float may = TN_Nhieu(q) * 0.65 + TN_Nhieu(q * 2.3 + 7.1) * 0.35;
                float a = lerp(1.0, _TN_DoToi * (1.0 - _TN_NhatMau * 0.35), f);
                float3 m = _TN_MauMay.rgb * smoothstep(0.35, 0.85, may) * _TN_MauMay.a * f;
                return fixed4(m, a);
            }
            ENDCG
        }
    }
}
