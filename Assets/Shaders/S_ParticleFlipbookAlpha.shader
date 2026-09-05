// Hat FLIPBOOK trong suot (khoi, bui, hoi nuoc).
//
// Giong S_ParticleFlipbookAdd nhung tron thuong thay vi cong sang: khoi thi
// CHE mat canh phia sau chu khong lam sang no len. Xem ghi chu day du ve
// flipbook va ve Custom Vertex Streams o dau file shader kia.
Shader "Diablo25D/ParticleFlipbookAlpha"
{
    Properties
    {
        _MainTex ("Luoi khung hinh", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1,1,1,1)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv     : TEXCOORD0;   // xy = khung hien tai, zw = khung ke tiep
                float  blend  : TEXCOORD1;
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
                fixed4 a = tex2D(_MainTex, i.uv.xy);
                fixed4 b = tex2D(_MainTex, i.uv.zw);
                return lerp(a, b, i.blend) * i.color * _TintColor;
            }
            ENDCG
        }
    }
    FallBack "Diablo25D/ParticleAlpha"
}
