// Shader TANG HINH: than nguoi gan nhu trong suot, chi con DUONG NET o ria (fresnel) va vai vet sang troi doc than -
// nhu Dark Templar trong StarCraft 2 / anh nguoi dung gui 18/09/2026.
//
// Khong ZWrite (trong suot), khong nhan bong. _Amount 0 = bien mat han, 1 = hien duong net day nhat.
Shader "Diablo25D/TangHinh"
{
    Properties
    {
        _Color     ("Mau vien",      Color) = (0.55, 0.95, 1.0, 1)
        _MauSau    ("Mau than mo",   Color) = (0.10, 0.35, 0.45, 1)
        _RimPower  ("Do manh cua vien", Float) = 2.6
        _Amount    ("Do hien 0..1",  Range(0,1)) = 1
        _VetToc    ("Toc do vet sang", Float) = 0.8
        _VetDam    ("Do dam vet sang", Range(0,2)) = 0.7
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One          // cong sang: than mo di, chi duong net noi len
        ZWrite Off
        Cull Back
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 pos  : SV_POSITION;
                float3 nrm  : TEXCOORD0;
                float3 vdir : TEXCOORD1;
                float3 opos : TEXCOORD2;
            };

            fixed4 _Color, _MauSau;
            float _RimPower, _Amount, _VetToc, _VetDam;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos  = UnityObjectToClipPos(v.vertex);
                o.nrm  = UnityObjectToWorldNormal(v.normal);
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vdir = normalize(UnityWorldSpaceViewDir(wpos));
                o.opos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.nrm);
                float3 v = normalize(i.vdir);
                // VIEN: cang nghieng so voi mat nhin cang sang - ra duong net quanh than
                float rim = pow(saturate(1.0 - dot(n, v)), _RimPower);

                // VAI VET SANG troi doc than (nhu lop nguy trang dang chay)
                float vet = sin((i.opos.y * 14.0) - _Time.y * _VetToc * 6.2831) * 0.5 + 0.5;
                vet = pow(vet, 6.0) * _VetDam;

                fixed3 col = _Color.rgb * (rim * 1.6 + vet) + _MauSau.rgb * 0.25;
                float a = saturate(rim * 0.95 + vet * 0.5 + 0.06) * _Amount;
                return fixed4(col * _Amount, a);
            }
            ENDCG
        }
    }
    FallBack Off
}
