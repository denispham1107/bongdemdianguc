// Shader BANG: trong mo, vien sang lanh (fresnel), co van nut ben trong
// va diem lap lanh. Dung cho: manh bang roi, gai bang, qua cau bang.
Shader "Diablo25D/Ice"
{
    Properties
    {
        _Color      ("Mau bang",        Color) = (0.55, 0.80, 1.0, 0.55)
        _DeepColor  ("Mau loi sau",     Color) = (0.10, 0.35, 0.75, 1)
        _RimColor   ("Mau vien sang",   Color) = (0.80, 0.95, 1.0, 1)
        _RimPower   ("Do manh cua vien",Float) = 2.2
        _Glow       ("Do phat sang",    Float) = 1.3
        _CrackScale ("Do min van nut",  Float) = 6.0
        _CrackAmount("Do dam van nut",  Range(0,1)) = 0.45
        _Sparkle    ("Lap lanh",        Range(0,2)) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos  : SV_POSITION;
                float3 opos : TEXCOORD0;
                float3 nrm  : TEXCOORD1;
                float3 vdir : TEXCOORD2;
                float3 wpos : TEXCOORD3;
            };

            fixed4 _Color, _DeepColor, _RimColor;
            float _RimPower, _Glow, _CrackScale, _CrackAmount, _Sparkle;

            float hash13(float3 p)
            {
                p = frac(p * 0.3183099 + float3(0.71, 0.113, 0.419));
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float vnoise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = hash13(i + float3(0,0,0));
                float n100 = hash13(i + float3(1,0,0));
                float n010 = hash13(i + float3(0,1,0));
                float n110 = hash13(i + float3(1,1,0));
                float n001 = hash13(i + float3(0,0,1));
                float n101 = hash13(i + float3(1,0,1));
                float n011 = hash13(i + float3(0,1,1));
                float n111 = hash13(i + float3(1,1,1));
                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);
                return lerp(lerp(nx00, nx10, f.y), lerp(nx01, nx11, f.y), f.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos  = UnityObjectToClipPos(v.vertex);
                o.opos = v.vertex.xyz;
                o.nrm  = UnityObjectToWorldNormal(v.normal);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vdir = normalize(UnityWorldSpaceViewDir(o.wpos));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.nrm);
                float3 v = normalize(i.vdir);
                float rim = pow(1.0 - saturate(dot(n, v)), _RimPower);

                // Van nut ben trong: noise dang soi -> gan 0 tao duong nut
                float nz = vnoise(i.opos * _CrackScale);
                float crack = 1.0 - saturate(abs(nz - 0.5) * 6.0);
                crack = pow(crack, 2.0) * _CrackAmount;

                // Diem lap lanh nho chay theo goc nhin
                float sp = vnoise(i.opos * 22.0 + float3(0, _Time.y * 0.6, 0));
                float sparkle = pow(saturate(sp - 0.72) * 3.6, 3.0) * _Sparkle;

                fixed3 col = lerp(_DeepColor.rgb, _Color.rgb, saturate(rim * 1.2 + 0.35));
                col += _RimColor.rgb * rim * _Glow;
                col += crack * _RimColor.rgb * 0.8;
                col += sparkle;

                // Anh sang mat troi hat len mat bang
                float3 ldir = normalize(_WorldSpaceLightPos0.xyz);
                float spec = pow(saturate(dot(reflect(-ldir, n), v)), 48.0);
                col += spec * 0.9;

                float a = saturate(_Color.a + rim * 0.55 + crack * 0.3 + sparkle);
                return fixed4(col, a);
            }
            ENDCG
        }
    }
    FallBack Off
}
