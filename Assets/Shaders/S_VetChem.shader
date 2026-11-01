// VET CHEM KIEM cua quai Bo xuong (01/11/2026, nguoi dung: "dung lai toan hieu ung chem... cho thay ro hieu ung chem").
// Luoi la mat quet cua LUOI KIEM THAT (VetChemKiem dung tu tu the vung tay): u = doc cung chem (0 dau cu, 1 cuoi cu),
// v = tu chuoi kiem (0) toi mui kiem (1). Anh VetChem.png (Blender MCP, CongCu/Blender/chem_bo_xuong.py): R than vet, G mep nong.
// Hien dan theo cu chem: _Dau chay 0 -> 1 (luoi kiem quet toi dau thi vet toi do), _Duoi duoi theo sau, _Mo lam mo ca vet.
// Tron "nhan truoc alpha" (One OneMinusSrcAlpha): than vet DO MAU SAM che bot nen (doc duoc ca ban ngay), mep cong sang do cam.
Shader "Diablo25D/VetChem"
{
    Properties
    {
        _MainTex ("Vet chem (R than, G mep)", 2D) = "black" {}
        _MauToi ("Mau than vet", Color) = (0.30, 0.012, 0.01, 1)
        _MauSang ("Mau mep nong", Color) = (1, 0.26, 0.10, 1)
        _DoDuc ("Do che nen cua than vet", Range(0, 1)) = 0.85
        _CuongDoSang ("Cuong do mep nong", Float) = 1.8
        _Dau ("Dau vet (u)", Float) = 1
        _Duoi ("Duoi vet (u)", Float) = 0
        _Mo ("Do hien", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            fixed4 _MauToi, _MauSang;
            float _DoDuc, _CuongDoSang, _Dau, _Duoi, _Mo;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float u = i.uv.x;
                fixed2 t = tex2D(_MainTex, i.uv).rg;

                // Cua so hien: tu duoi (mo dan 0,35) toi dau (mep dau sac 0,03); cuoi cung chem mo nhe cho khoi cat ngang
                float hien = smoothstep(_Duoi, _Duoi + 0.35, u) * (1 - smoothstep(_Dau - 0.03, _Dau + 0.005, u));
                hien *= smoothstep(1.0, 0.92, u) * smoothstep(0.0, 0.05, u) * _Mo;
                // Cang gan dau vet (cho luoi kiem vua qua) cang nong
                float nong = saturate((u - _Duoi) / max(_Dau - _Duoi, 0.05));
                nong = nong * nong;

                float a = t.r * hien * _DoDuc;
                float sang = (t.g * (0.45 + 0.55 * nong) + t.r * nong * 0.30) * hien * _CuongDoSang;
                float3 col = _MauToi.rgb * a + _MauSang.rgb * sang;
                return fixed4(col, a);
            }
            ENDCG
        }
    }
}
