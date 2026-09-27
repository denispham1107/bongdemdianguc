// Shader KHOI BANG PHA LE cua ky nang Qua cau bang (28/09/2026, nguoi dung: "qua cau bang that, chi tiet").
// Luoi va anh chi tiet dung bang Blender MCP (CongCu/Blender/cau_bang_pha_le.blend):
//   _MainTex (tuyen tinh, KHONG sRGB): R = vet nut (to + mang nut manh), G = bot khi, B = mang suong gia.
// Nhin nhu mot khoi bang trong:
//   - mat cat PHANG (phap tuyen tung mat) -> moi mat mot sac do, loe sang rieng khi quay (mat trang / den);
//   - vien fresnel xanh trang, trong long sau xanh dam;
//   - lop nut "CHIM SAU" ben trong: nhieu 3D lay mau o vi tri lui vao theo huong nhin -> nut troi lech khi khoi quay;
//   - suong gia trang duc, nut sang lanh, bot khi lap lanh.
// Hai luot: luot 1 chi ghi do sau (chi mat GAN NHAT duoc ve - khoi bang loi lom co tinh the khong bi ve chong
// nhieu lop sai thu tu), luot 2 tron trong suot. Loi sang ben trong ve TRUOC (hang doi 2999) nen van hien qua lop bang.
Shader "Diablo25D/CauBangPhaLe"
{
    Properties
    {
        _MainTex    ("Chi tiet (R nut, G bot, B suong)", 2D) = "black" {}
        _Color      ("Mau bang",          Color) = (0.50, 0.78, 1.0, 0.62)
        _DeepColor  ("Mau long sau",      Color) = (0.03, 0.15, 0.46, 1)
        _RimColor   ("Mau vien sang",     Color) = (0.78, 0.94, 1.0, 1)
        _FrostColor ("Mau suong gia",     Color) = (0.86, 0.94, 1.0, 1)
        _RimPower   ("Do manh vien",      Float) = 2.4
        _Glow       ("Do phat sang vien", Float) = 0.7
        _Nut        ("Do dam vet nut",    Range(0,2)) = 0.8
        _NutSau     ("Nut chim sau",      Range(0,2)) = 0.5
        _DoSau      ("Do sau lop nut (m)",Float) = 0.14
        _Suong      ("Do dam suong gia",  Range(0,2)) = 1.2
        _Sparkle    ("Lap lanh",          Range(0,2)) = 0.9
        _AlphaGoc   ("Alpha goc cua _Color (mo dan = _Color.a / so nay)", Float) = 0.62
    }

    SubShader
    {
        Tags { "Queue"="Transparent+1" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Pass
        {
            ZWrite On
            ColorMask 0
            Cull Back
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
                float3 opos : TEXCOORD1;
                float3 nrm  : TEXCOORD2;
                float3 wpos : TEXCOORD3;
                float3 ovd  : TEXCOORD4;   // huong nhin trong khong gian vat (de lui lop nut vao trong)
            };

            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _Color, _DeepColor, _RimColor, _FrostColor;
            float _RimPower, _Glow, _Nut, _NutSau, _DoSau, _Suong, _Sparkle, _AlphaGoc;

            float hash13(float3 p)
            {
                p = frac(p * 0.3183099 + float3(0.71, 0.113, 0.419));
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            float vnoise(float3 x)
            {
                float3 i = floor(x); float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(hash13(i), hash13(i + float3(1,0,0)), f.x),
                                 lerp(hash13(i + float3(0,1,0)), hash13(i + float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash13(i + float3(0,0,1)), hash13(i + float3(1,0,1)), f.x),
                                 lerp(hash13(i + float3(0,1,1)), hash13(i + float3(1,1,1)), f.x), f.y), f.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos  = UnityObjectToClipPos(v.vertex);
                o.uv   = TRANSFORM_TEX(v.uv, _MainTex);
                o.opos = v.vertex.xyz;
                o.nrm  = UnityObjectToWorldNormal(v.normal);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 camO = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                o.ovd = v.vertex.xyz - camO;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.nrm);
                float3 v = normalize(_WorldSpaceCameraPos - i.wpos);
                float ndv = saturate(dot(n, v));
                float rim = pow(1.0 - ndv, _RimPower);

                fixed4 ct = tex2D(_MainTex, i.uv);     // R nut, G bot, B suong

                // Lop nut CHIM SAU: lui diem lay mau vao trong theo huong nhin
                float3 p2 = i.opos + normalize(i.ovd) * _DoSau;
                float nz = vnoise(p2 * 7.0);
                float nutSau = pow(1.0 - saturate(abs(nz - 0.5) * 7.0), 2.0) * _NutSau;

                // Than bang: long sau xanh dam -> ria trong xanh nhat
                fixed3 col = lerp(_DeepColor.rgb, _Color.rgb, saturate(rim * 1.3 + 0.28));

                // Mat cat phang: moi mat nhan anh sang huong mot kieu (quay la doi sac). Tuong phan manh de doc ra
                // "khoi deo nhieu mat" o co 30-40 diem anh cua goc choi.
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float ndl = saturate(dot(n, l));
                col *= 0.50 + 0.70 * ndl;

                // Vet nut la mat vo TRANG DUC trong bang (tan xa anh sang), KHONG phat sang: cong sang thi ra luoi dien
                // nhu Qua cau dien (lan dau 28/09/2026 da bi the). Tron ve trang chu khong cong.
                col = lerp(col, _FrostColor.rgb * 0.92, saturate(nutSau * 0.45));
                col = lerp(col, _FrostColor.rgb, saturate(ct.b * _Suong * 0.85));   // suong gia trang duc
                col = lerp(col, _FrostColor.rgb * 1.05, saturate(ct.r * _Nut * 0.75)); // nut trang duc
                col += _RimColor.rgb * rim * _Glow;

                // Loe sang tren mat cat + bot khi lap lanh theo goc nhin
                float spec = pow(saturate(dot(reflect(-l, n), v)), 40.0);
                col += spec * 1.1;
                float sp = vnoise(i.opos * 24.0 + float3(0, _Time.y * 0.7, 0));
                col += (pow(saturate(sp - 0.70) * 3.3, 3.0) * _Sparkle + ct.g * 0.8) * _RimColor.rgb;

                // MO DAN: ExpandFade (tang bang tren dat) ha _Color.a -> ca khoi mo theo ti le _Color.a / _AlphaGoc.
                // Cap 5 TangBangNo tat mo dan (fadeColorAlpha = false) thi _Color.a giu nguyen, ti le = 1.
                float hien = saturate(_Color.a / max(_AlphaGoc, 0.001));
                float a = saturate(_AlphaGoc + rim * 0.45 + ct.r * 0.30 * _Nut + ct.b * 0.35 * _Suong + nutSau * 0.25 + spec) * hien;
                return fixed4(col, a);
            }
            ENDCG
        }
    }
    FallBack Off
}
