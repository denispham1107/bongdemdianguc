// Shader LUA THAT: nhieu (noise) chay cuon tren be mat, loi trang nong,
// vien chay ra thanh luoi lua, tu an mon (erosion) tao cam giac lua song.
// Dung cho: loi qua cau lua, vong lua no, ngon lua tren duoc.
Shader "Diablo25D/Fire"
{
    Properties
    {
        _CoreColor  ("Mau loi (nong nhat)", Color) = (1, 0.95, 0.75, 1)
        _MidColor   ("Mau giua",            Color) = (1, 0.55, 0.12, 1)
        _EdgeColor  ("Mau vien",            Color) = (0.75, 0.10, 0.02, 1)
        _Speed      ("Toc do cuon",         Float) = 1.6
        _Scale      ("Do min cua noise",    Float) = 3.0
        _Erosion    ("Do an mon vien",      Range(0,1)) = 0.35
        _Intensity  ("Do sang",             Float) = 2.2
        _RimPower   ("Do day cua vien",     Float) = 1.6
        _Alpha      ("Do dam chung",        Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Lighting Off

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
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float3 opos   : TEXCOORD1;   // toa do trong vat the -> lam noise 3D
                float3 nrm    : TEXCOORD2;
                float3 vdir   : TEXCOORD3;
            };

            fixed4 _CoreColor, _MidColor, _EdgeColor;
            float _Speed, _Scale, _Erosion, _Intensity, _RimPower, _Alpha;

            // --- Noise 3D don gian, khong can texture ---
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
                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);
                return lerp(nxy0, nxy1, f.z);
            }

            float fbm(float3 p)
            {
                float s = 0.0;
                float a = 0.5;
                for (int k = 0; k < 4; k++)
                {
                    s += a * vnoise(p);
                    p *= 2.03;
                    a *= 0.5;
                }
                return s;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos  = UnityObjectToClipPos(v.vertex);
                o.uv   = v.uv;
                o.opos = v.vertex.xyz;
                o.nrm  = UnityObjectToWorldNormal(v.normal);
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vdir = normalize(UnityWorldSpaceViewDir(wp));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y * _Speed;

                // Hai lop noise chay nguoc chieu -> lua cuon
                float3 p = i.opos * _Scale;
                float n1 = fbm(p + float3(0, -t * 1.3, t * 0.35));
                float n2 = fbm(p * 1.9 + float3(t * 0.4, -t * 2.1, 0));
                float n  = saturate(n1 * 0.65 + n2 * 0.45);

                // Vien nhin nghieng (fresnel): giua sang, ria toi dan
                float rim = 1.0 - saturate(dot(normalize(i.nrm), normalize(i.vdir)));
                float core = saturate(pow(1.0 - rim, _RimPower));

                // Nhiet do: loi nong + noise
                float heat = saturate(core * 1.25 + n * 0.75 - 0.25);

                fixed3 col = lerp(_EdgeColor.rgb, _MidColor.rgb, saturate(heat * 1.6));
                col = lerp(col, _CoreColor.rgb, saturate((heat - 0.55) * 2.4));

                // An mon: cho vien chay lung linh nhu luoi lua that
                float a = saturate(heat + 0.25 - _Erosion * (1.0 - n));
                a = saturate(a * a * _Alpha);

                return fixed4(col * _Intensity, a);
            }
            ENDCG
        }
    }
    FallBack Off
}
