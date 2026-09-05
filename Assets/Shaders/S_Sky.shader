// Bau troi chuyen mau tu chan troi len dinh troi, them may cuon cham.
// _StarAmount = 0 cho canh ban ngay, = 1 cho canh ban dem co sao.
Shader "Diablo25D/SkyGradient"
{
    Properties
    {
        _TopColor    ("Mau dinh troi",   Color) = (0.02, 0.02, 0.05, 1)
        _HorizColor  ("Mau chan troi",   Color) = (0.35, 0.13, 0.06, 1)
        _BottomColor ("Mau duoi dat",    Color) = (0.02, 0.02, 0.02, 1)
        _CloudColor  ("Mau may",         Color) = (0.30, 0.16, 0.12, 1)
        _CloudAmount ("Do dam cua may",  Range(0,1)) = 0.55
        _Exponent    ("Do gat cua vien", Float) = 1.4
        _StarAmount  ("Do sang cua sao", Range(0,1)) = 1

        // ---- MAT TRANG ----
        // Huong TU tam nhin DEN mat trang. Dat khop voi huong den Moonlight thi
        // cai dia trang moi nam dung cho anh sang hat tu do toi.
        _MoonDir     ("Huong mat trang", Vector) = (0.45, 0.55, -0.70, 0)
        _MoonColor   ("Mau mat trang",   Color) = (0.92, 0.95, 1.0, 1)
        _MoonSize    ("Do to cua dia trang", Range(0.001, 0.08)) = 0.022
        _MoonGlow    ("Do rong cua quang trang", Range(0.02, 1.0)) = 0.34
        _MoonStrength("Do sang cua quang", Range(0, 3)) = 0.85
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            fixed4 _TopColor, _HorizColor, _BottomColor, _CloudColor;
            float _CloudAmount, _Exponent, _StarAmount;
            fixed4 _MoonColor;
            float4 _MoonDir;
            float _MoonSize, _MoonGlow, _MoonStrength;

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
            float fbm(float3 p)
            {
                float s = 0.0, a = 0.5;
                for (int k = 0; k < 4; k++) { s += a * vnoise(p); p *= 2.1; a *= 0.5; }
                return s;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float h = d.y;

                fixed3 col;
                if (h >= 0)
                {
                    float t = pow(saturate(h), 1.0 / max(_Exponent, 0.01));
                    col = lerp(_HorizColor.rgb, _TopColor.rgb, t);

                    // May cuon cham gan chan troi
                    float3 p = d * (2.5 / max(h + 0.12, 0.12));
                    float c = fbm(p * 0.9 + float3(_Time.y * 0.012, 0, _Time.y * 0.008));
                    c = saturate((c - 0.42) * 2.2) * _CloudAmount * saturate(1.0 - h * 1.4);
                    col = lerp(col, _CloudColor.rgb, c);

                    // Sao mo
                    float st = pow(saturate(hash13(floor(d * 260.0)) - 0.9985) * 700.0, 1.2);
                    col += st * saturate(h * 2.0) * 0.6 * _StarAmount;

                    // ---- MAT TRANG ----
                    //
                    // Hai lop: mot QUANG rong toa ra xa, va mot DIA nho ranh net
                    // o giua. Chi ve dia thi mat trang trong nhu mot cham son
                    // dan len troi; chi ve quang thi thanh mot vet mo khong ra
                    // hinh gi.
                    float3 md = normalize(_MoonDir.xyz);
                    float cosGoc = dot(d, md);

                    // Quang: tat dan theo luy thua, dung smoothstep vi quang
                    // that khong co bien
                    float quang = pow(saturate(cosGoc), max(1.0, 2.0 / max(_MoonGlow, 0.02)));
                    col += _MoonColor.rgb * quang * _MoonStrength * 0.5;

                    // Dia trang: mep mem chi mot chut cho khoi rang cua
                    float goc = acos(saturate(cosGoc));
                    float dia = 1.0 - smoothstep(_MoonSize * 0.82, _MoonSize, goc);
                    col = lerp(col, _MoonColor.rgb * 1.35, dia);
                }
                else
                {
                    col = lerp(_HorizColor.rgb, _BottomColor.rgb, saturate(-h * 3.0));
                }

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}
