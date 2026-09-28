// Chi dung cho phep thu (menu 90): ve vat thanh mau TRANG DAC de lay bong than chinh xac (khong dinh bong do tren dat,
// khong phu thuoc mau ao toi hay sang). Dung qua Camera.RenderWithShader.
Shader "Hidden/Diablo25D/TrangDacPhepThu"
{
    SubShader
    {
        Pass
        {
            ZWrite On
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 vert (float4 v : POSITION) : SV_POSITION { return UnityObjectToClipPos(v); }
            fixed4 frag () : SV_Target { return fixed4(1, 1, 1, 1); }
            ENDCG
        }
    }
}
