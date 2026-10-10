// SUONG KHOI LO LUNG GIUA VUC DIA NGUC (05/10/2026): tam phang trong suot o luong chung vuc, hai lop anh khoi (Blender MCP, lap lien mach)
// troi cham khac huong. Lan anh dau (chua co lop nay) dung nham sang deu sat chan rao nhu mot tam tham, khong thay vuc sau 26 m -
// lop khoi do sam che mot phan dung nham, cho cam giac xa / sau.
Shader "Diablo25D/SuongVuc"
{
    Properties
    {
        _MainTex ("Khoi (xam = do duc, lap lien mach)", 2D) = "white" {}
        _Mau ("Mau khoi", Color) = (0.30, 0.06, 0.025, 1)
        _Do ("Do duc toi da", Range(0,1)) = 0.6
        _TiLe ("So lan lap moi met", Float) = 0.018
        _TroiA ("Troi lop 1 (xz, m/s)", Vector) = (0.35, 0.12, 0, 0)
        _TroiB ("Troi lop 2 (xz, m/s)", Vector) = (-0.2, 0.28, 0, 0)
        _SuongMu ("Muc suong mu ap len (0..1)", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent-5" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "SuongChienTranh.cginc"
            sampler2D _MainTex;
            fixed4 _Mau;
            float _Do, _TiLe, _SuongMu;
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
                float a = tex2D(_MainTex, (q + _TroiA.xy * t) * _TiLe).r;
                float b = tex2D(_MainTex, (q * 1.37 + float2(31.0, 9.0) + _TroiB.xy * t) * _TiLe).r;
                fixed4 col = fixed4(_Mau.rgb, saturate(a * b * 1.6) * _Do);
                fixed4 goc = col;
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.rgb = lerp(goc.rgb, col.rgb, _SuongMu);
                TN_Ap(col.rgb, i.w);       // suong chien tranh (TamNhin.cs)
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
