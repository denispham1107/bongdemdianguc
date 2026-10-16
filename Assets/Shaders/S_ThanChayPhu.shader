// LOP PHU THAN CHAY cho dau vet Thien thach dot rui cay / bia mo / nha mo (16/10/2026, nguoi dung: "Thien thach dot chay do vat, cay coi xong cung de
// lai dau vet chay den khi xuat hien lai"; chon tan tich chay den + tro, than hong am i ~10 giay roi nguoi). Ve CHONG len manh goc cay / chan bia /
// nen nha cua dau vet (vat lieu thu hai tren cung renderer, nhu Diablo25D/VetMauPhu). Anh ChayDen.png (Blender MCP, lop chay den cua May giong: than
// den + vet nut hong lua) chieu BA PHIA theo toa do THE GIOI (khong can UV). Vet nut (do > luc) phat sang theo _AmI (DauVetLocCuon dat qua
// MaterialPropertyBlock, 1 -> 0 trong 10 giay) va nhap nhay nhe; nguoi roi thi thanh khe den sau.
Shader "Diablo25D/ThanChayPhu"
{
    Properties
    {
        _MainTex ("Than (RGB; vet nut hong = do > luc)", 2D) = "white" {}
        _Co ("Co mot o anh (m)", Float) = 1.1
        _DoDam ("Do phu", Range(0, 1)) = 0.93
        _AmI ("Than hong am i (0 = nguoi)", Range(0, 1)) = 0
        _MauLua ("Mau than hong", Color) = (1, 0.32, 0.06, 1)
        _SangLua ("Do sang than hong", Float) = 3
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+5" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        ZWrite Off
        Offset -1, -1
        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0
        sampler2D _MainTex;
        float _Co, _DoDam, _AmI, _SangLua;
        fixed4 _MauLua;
        struct Input { float3 worldPos; float3 worldNormal; };
        void surf (Input IN, inout SurfaceOutput o)
        {
            float3 w = abs(IN.worldNormal); w = pow(w, 4); w /= (w.x + w.y + w.z + 1e-4);
            float3 p = IN.worldPos / _Co;
            fixed4 c = tex2D(_MainTex, p.zy) * w.x + tex2D(_MainTex, p.xz + 0.37) * w.y + tex2D(_MainTex, p.xy + 0.71) * w.z;
            // vet nut hong trong anh: kenh do vuot han kenh luc
            float nong = saturate((c.r - c.g * 1.25 - 0.04) * 6.0);
            float3 than = c.rgb * 0.55;
            o.Albedo = lerp(than, float3(0.05, 0.045, 0.04), nong);
            float nhay = 0.75 + 0.25 * sin(_Time.y * 3.1 + IN.worldPos.x * 2.3 + IN.worldPos.z * 1.7);
            o.Emission = _MauLua.rgb * _SangLua * nong * _AmI * nhay;
            o.Alpha = _DoDam;
        }
        ENDCG
    }
    FallBack Off
}
