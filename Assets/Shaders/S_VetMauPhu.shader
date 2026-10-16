// LOP PHU VET MAU + BUI BAN cho dau vet Loc xoay (16/10/2026, nguoi dung: "dau vet phai san sui, nham, co vet mau chu khong tron bong").
// Ve CHONG len manh goc cay / chan bia / nen nha (vat lieu thu hai tren cung renderer - Unity ve lai luoi). Anh VetMau.png (Blender MCP,
// CongCu/Blender/dau_vet_loc_anh.py: mau loang + giot ban + vet chay + bui nau den, alpha = do phu) chieu BA PHIA theo toa do THE GIOI (moi cho mot
// hoa van, khong can UV, khong lap y het giua cac dau vet). Nhan sang (Lambert), khong bong (nham).
Shader "Diablo25D/VetMauPhu"
{
    Properties
    {
        _MainTex ("Vet mau (RGB mau, A do phu)", 2D) = "white" {}
        _Co ("Co mot o anh (m)", Float) = 0.9
        _DoDam ("Do dam", Range(0, 1.5)) = 1
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
        float _Co, _DoDam;
        struct Input { float3 worldPos; float3 worldNormal; };
        void surf (Input IN, inout SurfaceOutput o)
        {
            float3 w = abs(IN.worldNormal); w = pow(w, 4); w /= (w.x + w.y + w.z + 1e-4);
            float3 p = IN.worldPos / _Co;
            fixed4 cx = tex2D(_MainTex, p.zy);
            fixed4 cy = tex2D(_MainTex, p.xz + 0.37);
            fixed4 cz = tex2D(_MainTex, p.xy + 0.71);
            fixed4 c = cx * w.x + cy * w.y + cz * w.z;
            o.Albedo = c.rgb;
            o.Alpha = saturate(c.a * _DoDam);
        }
        ENDCG
    }
    FallBack Off
}
