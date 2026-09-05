// Hat FLIPBOOK cong sang (lua, tia, no).
//
// FLIPBOOK la gi: thay vi mot anh dung yen, texture la mot LUOI o (vi du 8x8 =
// 64 khung hinh) chup mot dam khoi/lua dang cuon. He hat chay lan luot qua cac
// o do theo doi song cua hat, nen BEN TRONG tung hat cung co chuyen dong. Day
// la thu tao ra khac biet lon nhat giua hieu ung nghiep du va chuyen nghiep:
// khong co no thi du to mau dep den may, dam khoi van chi la mot nhum cham mo
// troi ngang.
//
// VI SAO CAN SHADER RIENG: he hat cua Unity tu cat o giup, nhung khi nhay tu
// khung nay sang khung ke tiep thi anh DOI DOT NGOT - khoi bi giat. Muon muot
// thi phai lay mau CA HAI khung roi tron chung theo mot he so.
//
// Ba thu do (uv khung hien tai, uv khung ke tiep, he so tron) khong tu nhien
// ma co: phai bat Custom Vertex Streams cua he hat theo dung THU TU
//     Position, Color, UV, UV2, AnimBlend
// thi Unity moi nhoi UV2 vao TEXCOORD0.zw va he so tron vao TEXCOORD1.x.
// Xem VfxFactory.BatFlipbook - ham do dat san dung thu tu nay.
Shader "Diablo25D/ParticleFlipbookAdd"
{
    Properties
    {
        _MainTex ("Luoi khung hinh", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
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
                float4 uv     : TEXCOORD0;   // xy = khung hien tai, zw = khung ke tiep
                float  blend  : TEXCOORD1;   // 0 = khung hien tai, 1 = khung ke tiep
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float4 uv    : TEXCOORD0;
                float  blend : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _TintColor;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.blend = v.blend;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lay mau HAI khung roi tron - day la ca ly do shader nay ton tai
                fixed4 a = tex2D(_MainTex, i.uv.xy);
                fixed4 b = tex2D(_MainTex, i.uv.zw);
                fixed4 c = lerp(a, b, i.blend) * i.color * _TintColor;
                c.rgb *= _Intensity;
                return c;
            }
            ENDCG
        }
    }
    FallBack "Diablo25D/ParticleAdditive"
}
