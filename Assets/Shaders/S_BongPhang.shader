// BONG CHIEU PHANG CHO NHAN VAT (muc do hoa "Rat yeu", 17/10/2026 - nguoi dung: "tat bong van giu nguyen, chi cho hien thi bong nhan vat chinh
// cua nguoi dang choi"; chon bong chieu phang).
//
// Vi sao khong dung bong that cua Unity: bat bong de chi MOT nhan vat do bong van ton +350 luot ve (+53%) va gap doi tam giac o Act2 - den chinh
// can ve them mot luot do sau ca canh (bong khong gian man hinh). Shader nay ve lai CHINH luoi nhan vat (dang cu dong, cung luot skinning - them
// mot o vat lieu tren renderer) va EP DET moi dinh xuong mat phang dat duoi chan theo huong den chinh -> bong dung dang nguoi, ~1 luot ve.
// Mat phang: diem _BP_Dat (dat duoi chan + 3 cm), phap tuyen _BP_PhapTuyen (phap tuyen dat), huong tia den _BP_HuongDen (chi XUONG) -
// BongChiNhanVat dat moi khung. Stencil: moi diem anh chi to MOT lan (tay chan chong nhau khong toi gap doi).
Shader "Diablo25D/BongPhang"
{
    Properties
    {
        _DoDam ("Do dam bong", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-100" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Offset -1, -1
            Stencil { Ref 7 Comp NotEqual Pass Replace }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _BP_Dat, _BP_PhapTuyen, _BP_HuongDen;
            float _BP_DoDam;
            half _DoDam;

            struct v2f { float4 pos : SV_POSITION; float mo : TEXCOORD0; };

            v2f vert (float4 v : POSITION)
            {
                v2f o;
                float3 p = mul(unity_ObjectToWorld, v).xyz;
                float3 n = normalize(_BP_PhapTuyen.xyz);
                float3 L = normalize(_BP_HuongDen.xyz);
                float d = dot(L, n);
                d = min(d, -0.25);                              // den sat chan troi: bong dai vo han -> kep
                float t = dot(_BP_Dat.xyz - p, n) / d;
                float3 q = p + L * max(t, 0);                   // dinh nam duoi mat phang: giu nguyen (khong chieu nguoc len)
                o.pos = UnityWorldToClipPos(float4(q, 1));
                // mo dan theo do cao dinh so voi dat (phan tren cao toa ra xa thi nhat hon mot chut)
                o.mo = saturate(1 - dot(p - _BP_Dat.xyz, n) * 0.12);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(0, 0, 0, _DoDam * _BP_DoDam * (0.75 + 0.25 * i.mo));
            }
            ENDCG
        }
    }
}
