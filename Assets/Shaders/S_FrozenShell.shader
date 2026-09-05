// Lop VO BANG boc quanh quai vat bi dong bang.
// Phong to mesh goc mot chut theo phap tuyen roi phu bang len tren.
Shader "Diablo25D/FrozenShell"
{
    Properties
    {
        _Color   ("Mau vo bang", Color) = (0.60, 0.85, 1.0, 0.60)
        _RimColor("Mau vien",    Color) = (0.85, 0.97, 1.0, 1)
        _Grow    ("Do phong to",  Float) = 0.035
        _RimPower("Do manh vien", Float) = 1.8
        _Frost   ("Do sut suong", Range(0,1)) = 0.6
        _Amount  ("Muc do dong bang", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f
            {
                float4 pos  : SV_POSITION;
                float3 opos : TEXCOORD0;
                float3 nrm  : TEXCOORD1;
                float3 vdir : TEXCOORD2;
            };

            fixed4 _Color, _RimColor;
            float _Grow, _RimPower, _Frost, _Amount;

            float hash13(float3 p)
            {
                p = frac(p * 0.3183099 + float3(0.71, 0.113, 0.419));
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            float vnoise(float3 x)
            {
                float3 i = floor(x); float3 f = frac(x); f = f * f * (3.0 - 2.0 * f);
                float n000=hash13(i), n100=hash13(i+float3(1,0,0)), n010=hash13(i+float3(0,1,0)), n110=hash13(i+float3(1,1,0));
                float n001=hash13(i+float3(0,0,1)), n101=hash13(i+float3(1,0,1)), n011=hash13(i+float3(0,1,1)), n111=hash13(i+float3(1,1,1));
                return lerp(lerp(lerp(n000,n100,f.x), lerp(n010,n110,f.x), f.y),
                            lerp(lerp(n001,n101,f.x), lerp(n011,n111,f.x), f.y), f.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float bump = (0.6 + 0.8 * vnoise(v.vertex.xyz * 9.0)) * _Grow * _Amount;
                float4 p = v.vertex;
                p.xyz += normalize(v.normal) * bump;
                o.pos  = UnityObjectToClipPos(p);
                o.opos = v.vertex.xyz;
                o.nrm  = UnityObjectToWorldNormal(v.normal);
                float3 wp = mul(unity_ObjectToWorld, p).xyz;
                o.vdir = normalize(UnityWorldSpaceViewDir(wp));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.nrm);
                float rim = pow(1.0 - saturate(dot(n, normalize(i.vdir))), _RimPower);
                float frost = vnoise(i.opos * 14.0);
                fixed3 col = _Color.rgb + _RimColor.rgb * rim * 1.1 + frost * _Frost * 0.35;
                float a = saturate((_Color.a + rim * 0.5 + frost * 0.15) * _Amount);
                return fixed4(col, a);
            }
            ENDCG
        }
    }
    FallBack Off
}
