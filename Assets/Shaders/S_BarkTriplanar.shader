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
// Chieu doc than cay duoc keo dan ra (_KeoDoc) vi vo cay that co van chay doc,
// khong phai o vuong.
Shader "Diablo25D/BarkTriplanar"
{
    Properties
    {
        _Color      ("Mau nen", Color) = (0.09, 0.11, 0.08, 1)
        _MainTex    ("Anh vo cay", 2D) = "white" {}
        _BumpMap    ("Anh gan", 2D) = "bump" {}
        _NhuaColor  ("Mau nhua cay", Color) = (0.34, 0.22, 0.06, 1)
        _NhuaMuc    ("Muc nhua cay", Range(0,1)) = 0.35
        _TiLe       ("Nhip lap", Float) = 1.4
        _KeoDoc     ("Keo van theo chieu doc", Range(0.2, 3)) = 1.7
        _DoSanSui   ("Do san sui", Range(0, 6)) = 2.6
        _Glossiness ("Do bong", Range(0,1)) = 0.06
        _XanhReu    ("Sac xanh reu", Range(0,1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 250

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _Color;
        fixed4 _NhuaColor;
        half _NhuaMuc;
        half _TiLe;
        half _KeoDoc;
        half _DoSanSui;
        half _Glossiness;
        half _XanhReu;

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

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 n = normalize(WorldNormalVector(IN, float3(0, 0, 1)));
            float3 w = TrongSo(n);

            float3 p = IN.worldPos * _TiLe;

            // Keo dan theo chieu doc: van vo cay chay doc than, khong phai o vuong
            float2 uvX = float2(p.z, p.y / _KeoDoc);
            float2 uvY = float2(p.x, p.z);
            float2 uvZ = float2(p.x, p.y / _KeoDoc);

            fixed4 cX = tex2D(_MainTex, uvX);
            fixed4 cY = tex2D(_MainTex, uvY);
            fixed4 cZ = tex2D(_MainTex, uvZ);
            fixed4 c = cX * w.x + cY * w.y + cZ * w.z;

            // Kenh alpha cua anh danh dau cho co nhua cay ri ra
            half nhua = saturate(c.a * _NhuaMuc);

            fixed3 nen = c.rgb * _Color.rgb;

            // Nga xanh reu: keo kenh xanh len mot chut o cho toi
            half toi = 1 - saturate(c.r * 2);
            nen.g += toi * _XanhReu * 0.11;
            nen.b += toi * _XanhReu * 0.035;

            o.Albedo = lerp(nen, _NhuaColor.rgb, nhua);

            // ============================================================
            //  ANH GAN PHAI TRON TRONG KHONG GIAN THE GIOI
            // ============================================================
            //
            // Luoi cay KHONG CO UV (kiem ca chin luoi: uv.Length = 0). Khong co
            // UV thi tangent ma Unity sinh ra la mot huong TUY TIEN, khong dinh
            // gi den huong chieu anh cua triplanar. Cong thang ba anh gan roi
            // gan vao o.Normal - von la khong gian tangent - thi cho san sui bi
            // xoay lung tung theo tung tam giac, va ket qua nhin ra mot than cay
            // NHAN THIN, y het nhu chua he co anh gan.
            //
            // Cach dung: tron ba anh gan trong KHONG GIAN THE GIOI bang phep
            // "whiteout" - cong phan xy cua anh gan vao phap tuyen the gioi roi
            // hoan doi truc cho khop voi huong chieu - sau do moi doi nguoc ve
            // khong gian tangent de tra cho o.Normal.
            float3 gX = UnpackNormal(tex2D(_BumpMap, uvX));
            float3 gY = UnpackNormal(tex2D(_BumpMap, uvY));
            float3 gZ = UnpackNormal(tex2D(_BumpMap, uvZ));

            // _DoSanSui nhan vao phan NGHIENG, khong lerp voi (0,0,1): lerp voi
            // he so > 1 la ngoai suy, phap tuyen dai ra roi chuan hoa lai nen
            // gan nhu khong doi gi - de 2,6 hay 1,0 cung ra mot ket qua.
            gX.xy *= _DoSanSui;
            gY.xy *= _DoSanSui;
            gZ.xy *= _DoSanSui;

            float3 tX = float3(gX.xy + n.zy, abs(gX.z) * n.x);
            float3 tY = float3(gY.xy + n.xz, abs(gY.z) * n.y);
            float3 tZ = float3(gZ.xy + n.xy, abs(gZ.z) * n.z);

            float3 gThegioi = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);

            // Doi nguoc ve khong gian tangent. WorldNormalVector doi tangent ->
            // the gioi, nen ba tich vo huong duoi day chinh la phep doi nguoc.
            float3 truc0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 truc1 = WorldNormalVector(IN, float3(0, 1, 0));
            o.Normal = normalize(float3(dot(gThegioi, truc0),
                                        dot(gThegioi, truc1),
                                        dot(gThegioi, n)));

            // Cho co nhua thi bong hon han, do la dau hieu de nhan ra nhua
            o.Smoothness = lerp(_Glossiness, 0.55, nhua);
            o.Metallic = 0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
