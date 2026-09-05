// Shader cho DUONG NGAM ky nang tren may cam ung.
//
// Khac moi shader hat khac cua du an o DUNG MOT dong: ZTest Always.
//
// Duong ngam la mot thu cua GIAO DIEN, khong phai vat the trong the gioi. No
// phai doc duoc trong MOI hoan canh - ke ca khi nguoi choi dang dung sau mot
// tang da, hay khi duong ngam chay xuyen qua mot goc cay. De ZTest mac dinh
// (LEqual) thi cay va da AN MAT tung khuc duong, va nguoi choi thay mot vach
// dut quang khong hieu no chi toi dau.
Shader "Diablo25D/ChiBaoNgam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1
    }
    SubShader
    {
        // Queue cao hon Transparent thuong (3000) de duong ngam ve SAU cac hieu
        // ung trong suot khac - lua, khoi, suong - chu khong bi chung phu len.
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent"
               "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha One
        ZWrite Off
        ZTest Always
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
