// DA MO CHIEU BA TRUC: muc nat, sut me, reu bam, moc va vet mau.
//
// VI SAO LAI LA TRIPLANAR: luoi bia mo dua tu Blender KHONG CO toa do anh -
// kiem ca 36 luoi deu uv.Length = 0. Khong co UV thi moi shader thuong deu lay
// dung mot diem anh cho ca vien da, ket qua la bia mo tron lu mot mau. Chieu
// anh tu ba huong roi tron lai thi chay duoc tren moi luoi.
//
// VA VI THE ANH GAN PHAI TRON TRONG KHONG GIAN THE GIOI. Khong co UV thi tangent
// Unity sinh ra la mot huong tuy tien, khong dinh gi den huong chieu; cong thang
// ba anh gan roi gan vao o.Normal - von la khong gian tangent - thi cho sut me
// bi xoay lung tung theo tung tam giac va bia mo lai nhin nhu moi duc. Xem
// S_BarkTriplanar, cung mot cai bay.
//
// ===================================================================
//  REU MOC O DAU LA CO LY DO
// ===================================================================
//
// Reu khong rai deu. No can am, va cho am la cho NUOC DONG LAI: mat tren nam
// ngang, va chan bia cho giap dat. Nen luong reu = mat na reu (tu anh) NHAN voi
// hai he so hinh hoc:
//
//   huong len troi   saturate(phap tuyen the gioi . truc Y)
//   thap gan dat     cang xuong thap cang nhieu
//
// Thieu hai he so do thi reu phu deu ca mat duoi va mat dung, nhin ra mot lop
// son xanh chu khong ra reu.
//
// ===================================================================
//  MAU THI NGUOC LAI: CHAY XUONG
// ===================================================================
//
// Vet mau khong phu theo huong len, ma CHAY doc xuong duoi. Hinh dang do da
// nam san trong anh mat na (keo dan theo chieu doc); shader chi lam dam them o
// cho lom - mau dong lai trong ranh.
Shader "Diablo25D/DaMoTriplanar"
{
    Properties
    {
        _Color      ("Mau da", Color) = (0.62, 0.63, 0.60, 1)
        _MainTex    ("Anh da (A = vet vo)", 2D) = "white" {}
        _BumpMap    ("Anh gan", 2D) = "bump" {}
        _MatNa      ("Mat na (R reu, G moc, B mau)", 2D) = "black" {}

        _ReuColor   ("Mau reu", Color) = (0.16, 0.26, 0.11, 1)
        _ReuMuc     ("Muc reu", Range(0,1)) = 0.85
        _ReuHuongLen("Reu doi hoi huong len troi", Range(0,1)) = 0.75

        _MocColor   ("Mau moc", Color) = (0.60, 0.62, 0.55, 1)
        _MocMuc     ("Muc moc", Range(0,1)) = 0.55

        _MauColor   ("Mau vet mau", Color) = (0.28, 0.030, 0.022, 1)
        _MauMuc     ("Muc vet mau", Range(0,1)) = 0.80

        _VoMuc      ("Muc vet vo", Range(0,2)) = 1.0
        _TiLe       ("Nhip lap - da", Float) = 1.15
        _TiLeMatNa  ("Nhip lap - reu/moc/mau", Float) = 0.42
        _DoSanSui   ("Do san sui", Range(0, 6)) = 2.4
        _Glossiness ("Do bong", Range(0,1)) = 0.10
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
        sampler2D _MatNa;

        fixed4 _Color;
        fixed4 _ReuColor;
        fixed4 _MocColor;
        fixed4 _MauColor;
        half _ReuMuc, _ReuHuongLen;
        half _MocMuc, _MauMuc, _VoMuc;
        half _TiLe, _TiLeMatNa, _DoSanSui, _Glossiness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

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
            float2 uvX = float2(p.z, p.y);
            float2 uvY = float2(p.x, p.z);
            float2 uvZ = float2(p.x, p.y);

            // ---------------- da ----------------
            fixed4 dX = tex2D(_MainTex, uvX);
            fixed4 dY = tex2D(_MainTex, uvY);
            fixed4 dZ = tex2D(_MainTex, uvZ);
            fixed4 da = dX * w.x + dY * w.y + dZ * w.z;

            fixed3 c = da.rgb * _Color.rgb;

            // Vet vo: da bi me thi long vet TOI hon han - be mat moi chua kip
            // bac mau, va nam thut vao trong nen it anh sang loi toi
            half vo = saturate(da.a * _VoMuc);
            c *= 1 - vo * 0.55;

            // ---------------- ba mat na ----------------
            //
            // Mat na lay TI LE RIENG, to hon han anh da.
            //
            // Dung chung mot ti le thi hat da va mang reu buoc phai cung co: de
            // hat da vua mat thi mang reu chi con hai chuc phan tram met, va ca
            // vien da nhin ra mot tam vai hoa lam tam chu khong ra reu bam. Tach
            // ra thi da giu duoc chi tiet nho ma reu, moc, mau van la nhung mang
            // LON troi han len.
            // Moi vien da lay mot CHO KHAC NHAU tren mat na.
            //
            // Lay thang theo toa do the gioi thi hai tam bia dung canh nhau -
            // cach nhau mot met, ma mat na lap moi hai met ruoi - se roi vao gan
            // dung mot cho tren mang: ca hang bia cung co vet mau o cung mot cho,
            // nhin ra mot duong vien do ke san chu khong ra mau vay.
            //
            // Goc vat the doi thanh mot do lech gia ngau nhien nhung TAT DINH:
            // cung mot tam bia thi lan chay nao cung ra dung do lech do.
            float3 gocVat = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            float3 lech = frac(sin(gocVat * 12.9898 + gocVat.yzx * 78.233) * 43758.5453) * 41.0;
            float3 q = (IN.worldPos + lech) * _TiLeMatNa;
            fixed3 mX = tex2D(_MatNa, float2(q.z, q.y)).rgb;
            fixed3 mY = tex2D(_MatNa, float2(q.x, q.z)).rgb;
            fixed3 mZ = tex2D(_MatNa, float2(q.x, q.y)).rgb;
            fixed3 mn = mX * w.x + mY * w.y + mZ * w.z;

            // REU: doi hoi mat huong LEN TROI. Mat duoi bia khong bao gio co reu.
            half len = saturate(n.y);
            half heSoLen = lerp(1, len * len, _ReuHuongLen);
            half reu = saturate(mn.r * _ReuMuc * heSoLen);
            // reu bam day hon o cho lom - nuoc dong lai trong ranh
            reu *= 0.55 + 0.45 * (1 - da.r);

            // MOC: rai deu hon reu, khong phu thuoc huong
            half moc = saturate(mn.g * _MocMuc) * (1 - reu);

            // MAU: khong phu thuoc huong, va dam them o cho lom
            half mau = saturate(mn.b * _MauMuc);
            mau *= 0.60 + 0.40 * (1 - da.r);

            c = lerp(c, _MocColor.rgb, moc * 0.7);
            c = lerp(c, _ReuColor.rgb, reu);
            // Mau de sau cung: mau do chay DE len ca reu lan moc
            c = lerp(c, _MauColor.rgb, mau);

            o.Albedo = c;

            // ---------------- anh gan trong khong gian the gioi ----------------
            float3 gX = UnpackNormal(tex2D(_BumpMap, uvX));
            float3 gY = UnpackNormal(tex2D(_BumpMap, uvY));
            float3 gZ = UnpackNormal(tex2D(_BumpMap, uvZ));

            gX.xy *= _DoSanSui;
            gY.xy *= _DoSanSui;
            gZ.xy *= _DoSanSui;

            float3 tX = float3(gX.xy + n.zy, abs(gX.z) * n.x);
            float3 tY = float3(gY.xy + n.xz, abs(gY.z) * n.y);
            float3 tZ = float3(gZ.xy + n.xy, abs(gZ.z) * n.z);
            float3 gThegioi = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);

            float3 truc0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 truc1 = WorldNormalVector(IN, float3(0, 1, 0));
            o.Normal = normalize(float3(dot(gThegioi, truc0),
                                        dot(gThegioi, truc1),
                                        dot(gThegioi, n)));

            // Reu hut sang, mau kho thi hoi bong hon da
            o.Smoothness = _Glossiness * (1 - reu * 0.8) + mau * 0.10;
            o.Metallic = 0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
