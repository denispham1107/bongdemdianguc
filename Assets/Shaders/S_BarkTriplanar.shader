// VO CAY CHIEU BA TRUC (triplanar).
//
// VI SAO CAN: luoi cay dua tu Blender sang KHONG CO toa do anh (UV) - kiem tra
// ca chin luoi cay deu vay. Khong co UV thi moi shader thuong deu lay dung mot
// diem anh cho ca cay, ket qua la than cay tron lu mot mau.
//
// Cach chua: khong hoi luoi xem lay anh o dau, ma TU TINH lay theo vi tri trong
// khong gian. Chieu anh tu ba huong X, Y, Z roi tron lai theo huong mat phang.
// Cach nay chay duoc tren MOI luoi, ke ca luoi khong he co UV.
//
// 17/10/2026 (nguoi dung: "vo cay con tron bong gia tao, cho san sui, mot vai cho nut troc"): bo anh nhieu cu (nhin nhu vai ran ri)
// thay bang ANH CHUP THAT Poly Haven CC0 xu ly trong Blender MCP (CongCu/Blender/anh_vo_cay_moi.py):
//   _MainTex  vo chinh nut sau (bark_willow_02): RGB mau x AO, A = do bong
//   _BumpMap  phap tuyen vo chinh
//   _TrongTex lop vo trong lo ra o cho troc (bark_platanus, nau vang nhat)
//   _TrocTex  mat na troc lap o nhip RIENG (_TiLeTroc, mang to vai chuc cm): A = cho troc / vet nut dai, B = do toi (bong duoi mep vo
//             con dinh + long vet nut), RG = phap tuyen bac vo (vo day -> long troc thap hon, mep vo quan len) -> mep troc co bac that.
// Tron TUNG HUONG CHIEU truoc (vo / trong / mat na cung mot huong) roi moi tron ba huong - tron mat na sau thi mep troc nhoe doi.
// Bo nhua cay + sac reu cua ban cu (kenh alpha nay la do bong).
Shader "Diablo25D/BarkTriplanar"
{
    Properties
    {
        _Color      ("Mau nhan (cay chay to den qua day)", Color) = (1, 1, 1, 1)
        _MainTex    ("Anh vo cay (A = do bong)", 2D) = "white" {}
        _BumpMap    ("Anh gan vo", 2D) = "bump" {}
        _TrongTex   ("Anh lop trong lo ra", 2D) = "white" {}
        _TrocTex    ("Mat na troc (RG phap tuyen, B toi, A troc)", 2D) = "black" {}
        _TiLe       ("Nhip lap vo (lan / m)", Float) = 1.0
        _KeoDoc     ("Keo van theo chieu doc", Range(0.2, 3)) = 1.0
        _DoSanSui   ("Do san sui", Range(0, 6)) = 1.8
        _TiLeTroc   ("Nhip lap mat na troc (lan / m)", Float) = 0.33
        _MucTroc    ("Muc troc", Range(0, 1)) = 1
        _DoSauTroc  ("Do sau bac troc", Range(0, 3)) = 1.2
        _ToiTroc    ("Bong mep troc + vet nut", Range(0, 1)) = 0.8
        _Glossiness ("He so do bong", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 250

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows finalcolor:tnFinal
        #pragma target 3.0
        #pragma multi_compile_fog       // finalcolor tat code suong tu sinh -> TN_SuongUnity can keyword FOG_*
        #include "SuongChienTranh.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _TrongTex;
        sampler2D _TrocTex;
        fixed4 _Color;
        half _TiLe;
        half _KeoDoc;
        half _DoSanSui;
        half _TiLeTroc;
        half _MucTroc;
        half _DoSauTroc;
        half _ToiTroc;
        half _Glossiness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        // Trong so tron ba huong. Mat huong len thi lay anh chieu tu tren xuong,
        // mat dung thi lay anh chieu ngang.
        float3 TrongSo(float3 n)
        {
            float3 w = pow(abs(n), 4);
            return w / max(w.x + w.y + w.z, 1e-5);
        }

        // Mot huong chieu: tra mau (rgb) + do bong (a), va phap tuyen khong gian chieu (xy nghieng, z > 0)
        void MotHuong(float2 uv, float2 uvTroc, out fixed4 mau, out float3 gan)
        {
            fixed4 v = tex2D(_MainTex, uv);
            fixed3 t = tex2D(_TrongTex, uv).rgb;
            fixed4 m = tex2D(_TrocTex, uvTroc);
            float3 g = UnpackNormal(tex2D(_BumpMap, uv));

            half a = saturate(m.a * _MucTroc);
            fixed3 c = lerp(v.rgb, t, a);
            c *= 1 - m.b * _ToiTroc * _MucTroc;          // bong duoi mep vo + long vet nut
            mau = fixed4(c, lerp(v.a, 0.10, a) * _Glossiness);

            // Long troc la lop trong min hon: giam san vo; cong bac vo o mep.
            // _DoSanSui nhan vao phan NGHIENG, khong lerp voi (0,0,1): lerp voi he so > 1 la ngoai suy, phap tuyen dai ra roi chuan hoa
            // lai nen gan nhu khong doi gi.
            g.xy *= _DoSanSui * (1 - a * 0.7);
            g.xy += (m.rg * 2 - 1) * _DoSauTroc * _MucTroc;
            gan = g;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 n = normalize(WorldNormalVector(IN, float3(0, 0, 1)));
            float3 w = TrongSo(n);

            float3 p = IN.worldPos * _TiLe;
            float3 q = IN.worldPos * _TiLeTroc + float3(0.37, 0.11, 0.73);   // lech de mat na khong trung nhip anh vo

            // Keo dan theo chieu doc: van vo cay chay doc than
            float2 uvX = float2(p.z, p.y / _KeoDoc);
            float2 uvY = float2(p.x, p.z);
            float2 uvZ = float2(p.x, p.y / _KeoDoc);

            fixed4 cX, cY, cZ; float3 gX, gY, gZ;
            MotHuong(uvX, float2(q.z, q.y), cX, gX);
            MotHuong(uvY, float2(q.x, q.z), cY, gY);
            MotHuong(uvZ, float2(q.x, q.y), cZ, gZ);
            fixed4 c = cX * w.x + cY * w.y + cZ * w.z;

            o.Albedo = c.rgb * _Color.rgb;
            o.Smoothness = c.a;
            o.Metallic = 0;

            // ============================================================
            //  ANH GAN PHAI TRON TRONG KHONG GIAN THE GIOI
            // ============================================================
            //
            // Luoi cay KHONG CO UV (uv.Length = 0) -> tangent Unity sinh ra la mot huong TUY TIEN, khong dinh gi den huong chieu anh.
            // Tron ba anh gan trong KHONG GIAN THE GIOI bang phep "whiteout" roi moi doi nguoc ve khong gian tangent cho o.Normal.
            float3 tX = float3(gX.xy + n.zy, abs(gX.z) * n.x);
            float3 tY = float3(gY.xy + n.xz, abs(gY.z) * n.y);
            float3 tZ = float3(gZ.xy + n.xy, abs(gZ.z) * n.z);

            float3 gThegioi = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);

            // Doi nguoc ve khong gian tangent. WorldNormalVector doi tangent -> the gioi, nen ba tich vo huong duoi day chinh la phep doi nguoc.
            float3 truc0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 truc1 = WorldNormalVector(IN, float3(0, 1, 0));
            o.Normal = normalize(float3(dot(gThegioi, truc0),
                                        dot(gThegioi, truc1),
                                        dot(gThegioi, n)));
        }
        // Suong chien tranh (TamNhin.cs, 10/10/2026)
        void tnFinal(Input IN, SurfaceOutputStandard o, inout fixed4 color)
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
