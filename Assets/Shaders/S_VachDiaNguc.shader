// VACH VUC DIA NGUC (05/10/2026, nguoi dung: "ben duoi ngoai vung ban do la dia nguc" - chon vuc tham + dung nham).
// Luoi Blender MCP (CongCu/Blender/dia_nguc.blend -> Resources/DiaNguc/DiaNguc.fbx: VachVuc, VachNgoai, CotDa).
// Da: anh da bazan phan tang (Blender MCP, lap lien mach) to TRIPLANAR theo toa do the gioi (luoi khong co UV).
// Mau dinh (nuong trong Blender): R = anh dung nham hat len (manh o chan vach), G = vet nut dung nham phat sang, B = toi o hoc / ke.
Shader "Diablo25D/VachDiaNguc"
{
    Properties
    {
        _MainTex ("Da bazan (lap lien mach)", 2D) = "gray" {}
        _TiLe ("So lan lap moi met", Float) = 0.11
        _MauDa ("Mau da", Color) = (0.42, 0.36, 0.34, 1)
        _MauAnhDo ("Anh dung nham hat len", Color) = (1.0, 0.22, 0.03, 1)
        _AnhDo ("Do manh anh hat len", Float) = 1.6
        _MauNut ("Vet nut dung nham", Color) = (1.0, 0.45, 0.06, 1)
        _SangNut ("Do sang vet nut", Float) = 2.6
        _NhipNut ("Nhip tho cua vet nut", Float) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert finalcolor:tnFinal
        #pragma target 3.0
        #pragma multi_compile_fog       // finalcolor tat code suong tu sinh -> TN_SuongUnity can keyword FOG_*
        #include "SuongChienTranh.cginc"
        sampler2D _MainTex;
        float _TiLe, _AnhDo, _SangNut, _NhipNut;
        fixed4 _MauDa, _MauAnhDo, _MauNut;
        struct Input { float3 worldPos; float3 worldNormal; float4 color : COLOR; };
        void vert(inout appdata_full v) { }
        fixed3 Triplanar(float3 p, float3 n)
        {
            float3 w = pow(abs(n), 4.0); w /= (w.x + w.y + w.z + 1e-4);
            fixed3 a = tex2D(_MainTex, p.zy * _TiLe).rgb;
            fixed3 b = tex2D(_MainTex, p.xz * _TiLe).rgb;
            fixed3 c = tex2D(_MainTex, p.xy * _TiLe).rgb;
            return a * w.x + b * w.y + c * w.z;
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed3 da = Triplanar(IN.worldPos, normalize(IN.worldNormal));
            float ao = IN.color.b;
            o.Albedo = da * _MauDa.rgb * ao;
            // vet nut tho nhe: moi vung mot pha (theo vi tri) - khong nhap nhay dong loat
            float pha = dot(IN.worldPos, float3(0.071, 0.13, 0.057));
            float tho = 0.7 + 0.3 * sin(_Time.y * _NhipNut + pha * 6.2831);
            // vet nut sang hon o cho da sang (mat da), toi o khe - de nut nam tren da chu khong phu deu
            float nut = IN.color.g * tho * (0.6 + 0.8 * dot(da, float3(0.33, 0.33, 0.33)));
            o.Emission = _MauAnhDo.rgb * IN.color.r * _AnhDo + _MauNut.rgb * nut * _SangNut;
        }
        // Suong chien tranh (TamNhin.cs, 10/10/2026)
        void tnFinal(Input IN, SurfaceOutput o, inout fixed4 color)
        {
            #ifdef UNITY_PASS_FORWARDADD
                TN_SuongUnity(color.rgb, IN.worldPos, true);
                TN_ApCong(color.rgb, IN.worldPos);
            #else
                TN_SuongUnity(color.rgb, IN.worldPos, false);
                TN_Ap(color.rgb, IN.worldPos);
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}
