// Hat TRONG SUOT (nhu Diablo25D/ParticleAlpha) nhung MO DAN KHI XUONG SAT MAT DAT.
//
// 28/09/2026: vong suong bang thay qua cau chop sang cua vu no bang - chup ra nhung DUONG THANG SAC cat ngang mat dat: tam suong
// billboard (quay mat ve camera) cam xuyen xuong dat, cho giao nhau la mot duong thang (tung bi qua cau chop sang che mat).
// "Hat mem" chuan can anh do sau cua ca canh - them mot luot ve ca canh, nang cho dien thoai. O day re hon: moi he hat cho biet
// do cao mat dat noi no sinh ra (_MatDatY, qua MaterialPropertyBlock - khong tao vat lieu moi), diem anh cang gan mat dat cang
// mo, tat han o mat dat -> khong con duong cat. Vu no bang nho (~3,4 m) nen coi dat quanh do la phang.
Shader "Diablo25D/ParticleAlphaSatDat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1,1,1,1)
        _MatDatY ("Do cao mat dat (the gioi)", Float) = -1000
        _DoMemDat ("Mo dan trong (m) tren mat dat", Float) = 0.6
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float wy : TEXCOORD1; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TintColor;
            float _MatDatY, _DoMemDat;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.wy = mul(unity_ObjectToWorld, v.vertex).y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color * _TintColor;
                float k = saturate((i.wy - _MatDatY) / max(_DoMemDat, 0.01));
                c.a *= k * k * (3.0 - 2.0 * k);
                return c;
            }
            ENDCG
        }
    }
}
