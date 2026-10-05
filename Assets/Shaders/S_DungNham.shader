// BIEN DUNG NHAM DUOI VUC (05/10/2026, xem S_VachDiaNguc). Anh dung nham Blender MCP (vo nguoi den do chia mang, khe nut + vung
// long cam vang - lap lien mach) to theo toa do the gioi XZ, HAI LOP troi cham khac huong + nhip sang toi; khong nhan sang (tu phat).
// Suong mu: chi ap MOT PHAN (_SuongMu) - suong dem phu kin thi dung nham o xa chim vao mau xanh dem, mat cam giac dia nguc.
Shader "Diablo25D/DungNham"
{
    Properties
    {
        _MainTex ("Dung nham (lap lien mach)", 2D) = "black" {}
        _TiLe ("So lan lap moi met", Float) = 0.022
        _Sang ("Do sang", Float) = 0.85
        _TroiA ("Troi lop 1 (xz, m/s)", Vector) = (0.12, 0.05, 0, 0)
        _TroiB ("Troi lop 2 (xz, m/s)", Vector) = (-0.07, 0.10, 0, 0)
        _SuongMu ("Muc suong mu ap len (0..1)", Range(0,1)) = 0.45
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float _TiLe, _Sang, _SuongMu;
            float4 _TroiA, _TroiB;
            struct v2f { float4 pos : SV_POSITION; float3 w : TEXCOORD0; UNITY_FOG_COORDS(1) };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.w = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(o.w);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y;
                float2 q = i.w.xz;
                fixed3 a = tex2D(_MainTex, (q + _TroiA.xy * t) * _TiLe).rgb;
                fixed3 b = tex2D(_MainTex, (q * 0.73 + float2(17.3, 5.1) + _TroiB.xy * t) * _TiLe).rgb;
                // ket hop: lay noi sang hon (khe nut cua hai lop dan cheo) + mot phan trung binh
                fixed3 c = max(a, b) * 0.75 + (a + b) * 0.125;
                // LOANG CO LON (o ~110 m): pha the lap cua anh - lan anh dau mat dung nham deu nhu tam tham
                fixed3 lon = tex2D(_MainTex, q * _TiLe * 0.2 + float2(0.37, 0.71)).rgb;
                float loang = smoothstep(0.04, 0.35, dot(lon, float3(0.3, 0.5, 0.2)));
                c *= lerp(0.3, 1.25, loang);
                // nhip sang toi cham theo vung
                float nhip = 0.85 + 0.15 * sin(t * 0.8 + dot(q, float2(0.05, 0.037)) * 6.2831);
                c *= _Sang * nhip;
                fixed4 col = fixed4(c, 1);
                fixed4 goc = col;
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.rgb = lerp(goc.rgb, col.rgb, _SuongMu);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
