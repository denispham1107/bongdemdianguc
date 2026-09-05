// Mat dat dong co: tron hai lop anh (co xanh va dat bun) theo MAU DINH cua luoi.
// Cho -> vertex color .r = 1, loi mon / vung bun -> .r = 0.
// Nho vay mot mieng dat duy nhat van co cho co ram, cho tro dat.
Shader "Diablo25D/Terrain"
{
    Properties
    {
        _GrassTex   ("Anh co",        2D) = "white" {}
        _DirtTex    ("Anh dat",       2D) = "white" {}
        _GrassColor ("Mau co",     Color) = (0.42, 0.52, 0.24, 1)
        _DirtColor  ("Mau dat",    Color) = (0.40, 0.33, 0.24, 1)
        _Tiling     ("So lan lap",  Float) = 15
        _DetailTiling ("Lap chi tiet", Float) = 47
        _Glossiness ("Do bong", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _GrassTex;
        sampler2D _DirtTex;
        fixed4 _GrassColor, _DirtColor;
        float _Tiling, _DetailTiling, _Glossiness;

        struct Input
        {
            float2 uv_GrassTex;
            float4 vcolor;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vcolor = v.color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_GrassTex * _Tiling;
            float2 uvd = IN.uv_GrassTex * _DetailTiling;

            fixed4 grass = tex2D(_GrassTex, uv) * _GrassColor;
            fixed4 dirt  = tex2D(_DirtTex,  uv) * _DirtColor;

            // Lop chi tiet lap nhanh hon de khong bi "mo nhoe" khi camera lai gan.
            // Xoay 90 do va lam that nhat de khong tao ra o ban co tren mat dat.
            fixed3 detail = tex2D(_GrassTex, uvd.yx * 0.73 + 0.37).rgb;

            float k = saturate(IN.vcolor.r);
            fixed3 c = lerp(dirt.rgb, grass.rgb, k);
            c *= lerp(0.94, 1.06, detail.g);

            // Loang mau tren dien rong de mat dat khong deu tam tap mot mau
            fixed3 broad = tex2D(_GrassTex, IN.uv_GrassTex * 2.3 + 0.11).rgb;
            c *= lerp(0.84, 1.10, broad.r);

            o.Albedo = c;
            o.Smoothness = _Glossiness * lerp(1.6, 0.6, k);   // dat bun uot hon co
            o.Metallic = 0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
