// Shader MANH BANG VO (hat dang LUOI) - 28/09/2026, nguoi dung: manh vo cua Mua bang / Qua cau bang "chi la hinh tam giac,
// so sai" -> manh bang pha le trong suot tong xanh lam lanh. Luoi va anh chi tiet: Blender MCP
// (CongCu/Blender/manh_bang_pha_le.blend): 8 manh - 4 phien vien lom chom, 2 cuc, 2 kim.
//   _MainTex (tuyen tinh): R vet nut, G bot khi, B suong gia.
// Giong Diablo25D/CauBangPhaLe (mat cat phang loe sang, vien fresnel, nut trang duc) nhung:
//   - MOT luot, khong ghi do sau (hang tram manh nho bay chong nhau - khong can dung thu tu);
//   - NHAN MAU HAT (vertex color): colorOverLifetime cua he hat lam mo / nhuom duoc.
// Hat dang luoi cua Unity dua dinh vao KHONG GIAN THE GIOI, nen nhieu lap lanh lay theo toa do the gioi.
Shader "Diablo25D/ManhBangPhaLe"
{
    Properties
    {
        _MainTex    ("Chi tiet (R nut, G bot, B suong)", 2D) = "black" {}
        _Color      ("Mau bang",          Color) = (0.52, 0.80, 1.0, 0.70)
        _DeepColor  ("Mau long sau",      Color) = (0.05, 0.20, 0.55, 1)
        _RimColor   ("Mau vien sang",     Color) = (0.80, 0.94, 1.0, 1)
        _FrostColor ("Mau suong gia",     Color) = (0.88, 0.95, 1.0, 1)
        _RimPower   ("Do manh vien",      Float) = 2.2
        _Glow       ("Do phat sang vien", Float) = 0.8
        _Nut        ("Do dam vet nut",    Range(0,2)) = 0.7
        _Suong      ("Do dam suong gia",  Range(0,2)) = 0.8
        _Sparkle    ("Lap lanh",          Range(0,2)) = 1.0
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

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f
            {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
                float3 nrm  : TEXCOORD1;
                float3 wpos : TEXCOORD2;
                fixed4 col  : COLOR;
            };

            sampler2D _MainTex; float4 _MainTex_ST;
            fixed4 _Color, _DeepColor, _RimColor, _FrostColor;
            float _RimPower, _Glow, _Nut, _Suong, _Sparkle;

            float hash13(float3 p)
            {
                p = frac(p * 0.3183099 + float3(0.71, 0.113, 0.419));
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos  = UnityObjectToClipPos(v.vertex);
                o.uv   = TRANSFORM_TEX(v.uv, _MainTex);
                o.nrm  = UnityObjectToWorldNormal(v.normal);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.col  = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.nrm);
                float3 v = normalize(_WorldSpaceCameraPos - i.wpos);
                float rim = pow(1.0 - saturate(dot(n, v)), _RimPower);
                fixed4 ct = tex2D(_MainTex, i.uv);

                fixed3 col = lerp(_DeepColor.rgb, _Color.rgb, saturate(rim * 1.3 + 0.3));
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                col *= 0.50 + 0.70 * saturate(dot(n, l));                       // mat cat phang, moi mat mot sac
                col = lerp(col, _FrostColor.rgb, saturate(ct.b * _Suong * 0.8));
                col = lerp(col, _FrostColor.rgb * 1.05, saturate(ct.r * _Nut * 0.7));
                col += _RimColor.rgb * rim * _Glow;

                // Loe sang khi mat cat quay dung goc anh trang - manh bay lon nhao nen chop tat lien tuc
                float spec = pow(saturate(dot(reflect(-l, n), v)), 30.0);
                col += spec * 1.3 * _Sparkle;
                col += ct.g * 0.7 * _RimColor.rgb;

                col *= i.col.rgb;
                float a = saturate(_Color.a + rim * 0.4 + ct.r * 0.25 * _Nut + ct.b * 0.3 * _Suong + spec) * i.col.a;
                return fixed4(col, a);
            }
            ENDCG
        }
    }
    FallBack Off
}
