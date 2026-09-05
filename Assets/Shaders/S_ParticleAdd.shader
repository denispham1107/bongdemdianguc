// Shader hat lua / tia sang: cong them anh sang (additive) nen luon phat sang.
Shader "Diablo25D/ParticleAdditive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1

        // 4 = LEqual (binh thuong, bi vat can che),  8 = Always (ve de len tren).
        //
        // De o day duoi dang thuoc tinh chu khong viet cung "ZTest Always": ca
        // tram hieu ung dung chung shader nay, va gan het trong so do PHAI bi
        // vat can che moi dung. Chi vai cai - vong phep duoi chan cua vung
        // Mua bang / Sam set - moi can ve xuyen qua.
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha One
        ZWrite Off
        ZTest [_ZTest]
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
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TintColor;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color * _TintColor;
                c.rgb *= _Intensity;
                return c;
            }
            ENDCG
        }
    }
}
