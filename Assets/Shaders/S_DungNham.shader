// BIEN DUNG NHAM DUOI VUC (05/10/2026, xem S_VachDiaNguc). Anh dung nham Blender MCP (vo nguoi den do chia mang, khe nut + vung
// long cam vang - lap lien mach) to theo toa do the gioi XZ, HAI LOP troi cham khac huong + nhip sang toi; khong nhan sang (tu phat).
// Suong mu: chi ap MOT PHAN (_SuongMu) - suong dem phu kin thi dung nham o xa chim vao mau xanh dem, mat cam giac dia nguc.
//
// DUNG NHAM CHAY THEO CAC DUONG GAN (05/10/2026, nguoi dung: "cho thay ro cac dong dung nham dang chuyen dong va chay"; chon "chay
// theo cac duong gan trong hinh"). Anh _HuongGan (Blender MCP, numpy tren DungNham.png, cung o lap): RG = HUONG TIEP TUYEN cua gan
// dang GOC KEP (cos2t, sin2t) x do ket hop (tensor cau truc) - goc kep nen loc song tuyen / mip trung binh dung; B = nhieu dai tan lap
// lien mach (~2,4 m / chu ky). Moi diem: huong gan (nua goc) lay dau theo dong CHAY VONG quanh ban do (D, lech nhe theo nhieu lon) -
// gan nam ngang dong chay thi chay cham; vung dung nham dac (do ket hop thap) theo D. Cac DOT SANG = nhieu B dich theo huong ay
// (flow map hai pha lech nua chu ky, tron tam giac, chuan hoa phuong sai de khong nhap nhay do tuong phan), chi hien tren gan / vung
// sang (mat na do sang anh). _DoChay 0 = hinh cu (doi chung). Dong ho: bien toan cuc _DN_ThoiGian (DiaNguc dat moi khung = Time.time;
// phep thu dat tay) - khong dung _Time de do duoc tung khung.
Shader "Diablo25D/DungNham"
{
    Properties
    {
        _MainTex ("Dung nham (lap lien mach)", 2D) = "black" {}
        _HuongGan ("Huong gan (RG goc kep, B nhieu)", 2D) = "gray" {}
        _TiLe ("So lan lap moi met", Float) = 0.022
        _Sang ("Do sang", Float) = 0.85
        _TroiA ("Troi lop 1 (xz, m/s)", Vector) = (0.12, 0.05, 0, 0)
        _TroiB ("Troi lop 2 (xz, m/s)", Vector) = (-0.07, 0.10, 0, 0)
        _SuongMu ("Muc suong mu ap len (0..1)", Range(0,1)) = 0.45
        _TocChay ("Toc do chay theo gan (m/s) - nguoi dung chon 0,8 qua anh dong menu 111b", Float) = 0.8
        _QuangChuKy ("Quang troi moi chu ky flow map (m)", Float) = 2.0
        _DoChay ("Do ro cua dot sang chay (0 = hinh cu)", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            sampler2D _MainTex, _HuongGan;
            float _TiLe, _Sang, _SuongMu, _TocChay, _QuangChuKy, _DoChay;
            float4 _TroiA, _TroiB;
            float _DN_ThoiGian;
            struct v2f { float4 pos : SV_POSITION; float3 w : TEXCOORD0; UNITY_FOG_COORDS(1) };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.w = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(o.w);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            // Mot lop dung nham: anh + dot sang chay doc gan. uv = toa do o anh, uvMoiMet = so o anh moi met cua lop nay.
            fixed3 LopChay(float2 uv, float uvMoiMet, float2 D, float t)
            {
                fixed3 c = tex2D(_MainTex, uv).rgb;
                float3 h = tex2D(_HuongGan, uv).rgb;
                float2 v2 = h.rg * 2.0 - 1.0;
                float ketHop = saturate(length(v2));
                float2 g = v2 / max(ketHop, 1e-4);
                // nua goc: (1+cos2t, sin2t) va (sin2t, 1-cos2t) deu song song (cos t, sin t); chon cai khong suy bien
                float2 tg = g.x >= 0.0 ? float2(1.0 + g.x, g.y) : float2(g.y, 1.0 - g.x);
                tg = normalize(tg);
                float dd = dot(tg, D);
                float2 huong = tg * (dd >= 0.0 ? 1.0 : -1.0);
                huong = normalize(lerp(D, huong, ketHop) + 1e-5);
                float toc = _TocChay * lerp(1.0, 0.4 + 0.6 * abs(dd), ketHop);

                // flow map hai pha. Chu ky = quang troi co dinh / toc chay: chu ky co dinh 1,6 s thi o 2,5 m/s mau troi 4 m moi chu ky
                // (> buoc song nhieu 2,4 m) ma huong gan doi trong ~1 m -> mau bi keo meo, menu 111 do toc chi ~1,6 m/s
                float chuKy = _QuangChuKy / max(_TocChay, 0.1);
                float p0 = frac(t / chuKy);
                float p1 = frac(t / chuKy + 0.5);
                float2 dichMotChuKy = huong * toc * chuKy * uvMoiMet;
                float n0 = tex2D(_HuongGan, uv - dichMotChuKy * p0).b - 0.5;
                float n1 = tex2D(_HuongGan, uv - dichMotChuKy * p1 + float2(0.37, 0.61)).b - 0.5;
                float w0 = 1.0 - abs(2.0 * p0 - 1.0);
                float w1 = 1.0 - w0;
                float n = (w0 * n0 + w1 * n1) * rsqrt(w0 * w0 + w1 * w1) + 0.5;

                float matNa = smoothstep(0.30, 0.75, dot(c, float3(0.3, 0.5, 0.2))) * _DoChay;
                float dot_ = smoothstep(0.38, 0.82, n);
                c *= lerp(1.0, 0.30 + 1.55 * dot_, matNa);
                c += matNa * dot_ * dot_ * fixed3(0.60, 0.42, 0.18);
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _DN_ThoiGian;
                float2 q = i.w.xz;
                // dong chay vong quanh ban do (nguoc chieu kim dong ho nhin tu tren), lech nhe theo nhieu co lon (~18 m)
                float2 D = normalize(float2(-q.y, q.x) + 1e-3);
                float lech = (tex2D(_HuongGan, q * _TiLe * 0.13).b - 0.5) * 2.2;
                float cs = cos(lech), sn = sin(lech);
                D = float2(D.x * cs - D.y * sn, D.x * sn + D.y * cs);

                fixed3 a = LopChay((q + _TroiA.xy * t) * _TiLe, _TiLe, D, t);
                fixed3 b = LopChay((q * 0.73 + float2(17.3, 5.1) + _TroiB.xy * t) * _TiLe, _TiLe * 0.73, D, t + 0.8);
                // ket hop: lay noi sang hon (khe nut cua hai lop dan cheo) + mot phan trung binh
                fixed3 c = max(a, b) * 0.75 + (a + b) * 0.125;
                // LOANG CO LON (o ~110 m): pha the lap cua anh - lan anh dau mat dung nham deu nhu tam tham
                fixed3 lon = tex2D(_MainTex, q * _TiLe * 0.2 + float2(0.37, 0.71)).rgb;
                float loang = smoothstep(0.04, 0.35, dot(lon, float3(0.3, 0.5, 0.2)));
                c *= lerp(0.3, 1.25, loang);
                // nhip sang toi cham theo vung
                float nhip = 0.85 + 0.15 * sin(t * 0.8 + dot(q, float2(0.05, 0.037)) * 6.2831);
                c *= _Sang * nhip;
                fixed4 col = fixed4(c, 1);
                fixed4 goc = col;
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.rgb = lerp(goc.rgb, col.rgb, _SuongMu);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
