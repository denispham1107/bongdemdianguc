// Cay co / la bui: mieng vai hai mat, khoet trong theo do trong suot cua anh,
// va DU DUA THEO GIO. Phan ngon (uv.y cao) lac manh, phan goc dung yen.
// Mau dinh (vertex color) dung de moi bui co mot sac xanh hoi khac nhau.
Shader "Diablo25D/Foliage"
{
    Properties
    {
        _MainTex   ("Anh la / co",     2D) = "white" {}
        _Color     ("Mau nhuom",    Color) = (1,1,1,1)
        _Cutoff    ("Nguong khoet", Range(0,1)) = 0.42
        _WindSpeed ("Toc do gio",   Float) = 1.5
        _WindScale ("Do lon con gio", Float) = 0.28
        _WindBend  ("Do lac",       Float) = 0.16
        _AmbientBoost ("Sang them o goc khuat", Range(0,1)) = 0.10
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert alphatest:_Cutoff addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        float _WindSpeed, _WindScale, _WindBend, _AmbientBoost;

        struct Input
        {
            float2 uv_MainTex;
            float4 vcolor;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vcolor = v.color;

            // Goc bam dat, ngon lac nhieu -> uv.y quyet dinh do lac
            float sway = v.texcoord.y * v.texcoord.y;

            float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
            float t = _Time.y * _WindSpeed;

            // Hai con song lech pha nhau cho gio khong deu tam tap
            float w = sin(t + wp.x * _WindScale + wp.z * _WindScale * 0.7)
                    + 0.45 * sin(t * 1.7 + wp.x * _WindScale * 2.3 - wp.z * _WindScale * 1.1);

            v.vertex.x += w * sway * _WindBend;
            v.vertex.z += w * sway * _WindBend * 0.55;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.vcolor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            // La mong nen luon co chut anh sang xuyen qua, khong den kit o mat khuat
            o.Emission = c.rgb * _AmbientBoost;
        }
        ENDCG
    }

    FallBack "Transparent/Cutout/Diffuse"
}
