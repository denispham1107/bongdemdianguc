// DAU VET TREN MAT DAT - NHAN MAU (lam dat TOI DI), dung cho vet cay dat + vet chay xem cua Loc xoay / Gio loc (29/09/2026).
//
// Tron alpha thuong (ParticleAlpha) KHONG dung duoc cho vet dat: shader khong nhan sang nen giua dem vet dat nau SANG HON mat dat
// xung quanh (nhu tu phat sang) - loi cu cua vung mau (HeSoSangMau 0,6). Nhan mau thi ngay hay dem vet chi lam dat toi di dung ti le.
// mau ra = dat x lerp(1, anh.rgb x _NhanMau, anh.a x mau dinh.a x _Duc). Suong mu: xa thi he so ve 1 (khong con toi) -
// khong thi vet thanh mang den giua suong.
Shader "Diablo25D/VetDatNhan"
{
    Properties
    {
        _MainTex ("Anh vet (RGB toi, A do phu)", 2D) = "white" {}
        _NhanMau ("Nhan mau anh (anh rat toi)", Float) = 4
        _Duc ("Do dam", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent-20" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend DstColor Zero
        ZWrite Off
        Cull Off
        Lighting Off
        Offset -1, -1

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; UNITY_FOG_COORDS(1) };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _NhanMau, _Duc;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 t = tex2D(_MainTex, i.uv);
                float a = saturate(t.a * i.color.a * _Duc);
                fixed4 c = fixed4(lerp(fixed3(1, 1, 1), saturate(t.rgb * _NhanMau), a), 1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(1, 1, 1, 1));
                return c;
            }
            ENDCG
        }
    }
}
