// LUA CHAY LAN KHAP NGUOI - lop vat lieu PHU THEM len moi SkinnedMeshRenderer / MeshRenderer cua ke dang chay (LuaToanThan).
//
// 28/09/2026, lan 3 (nguoi dung ve duong do quanh nhan vat): "cho ngon lua dot lan khap nguoi nhan vat, khong phai de nguyen
// 1 cuc roi de nhan vat ben trong - lua chay va nam ben trong duong ke do". Lan 2 la mot khoi lua billboard dung truoc than.
// Nay lua ve THANG TREN LUOI NHAN VAT nen om dung dang nguoi (tay, chan, dau), cu dong theo hoat hinh, khong tran ra ngoai.
//
// _MainTex: anh Blender MCP (CongCu/Blender/lua_chay_nguoi.blend, canh LuaPhuThan) - lap lien mach ca hai chieu:
//   R soc lua doc, G dam lua lon, B vet nut than hong.
// Toa do anh: ngang = chieu len truc PHAI cua may quay (tru goc nhan vat _Goc), doc = do cao tren chan / chieu cao than
// -> soc lua luon dung thang tren man hinh va TROI LEN theo thoi gian; di theo nhan vat (tru _Goc) nen chay khong truot.
// Luot 1: lua tren mat da (cong sang - than van hien ben duoi). Luot 2: VO PHONG ra theo phap tuyen + nhoc len tren, chi
//   hien o VIEN (fresnel) va o soc lua -> mep lua liem ra ngoai than mot chut, nhu net do nguoi dung ve.
Shader "Diablo25D/LuaPhuThan"
{
    Properties
    {
        _MainTex ("Nhieu lua (R soc, G dam, B nut)", 2D) = "gray" {}
        _Do ("Do manh", Range(0,1)) = 1
        _SangLua ("Do sang", Float) = 1.6
        _Goc ("Goc nhan vat (the gioi)", Vector) = (0,0,0,0)
        _CaoThan ("Chieu cao than (m)", Float) = 1.7
        _DoPhong ("Vo lua phong ra (m)", Float) = 0.07
        _TocDo ("Toc do lua boc len", Float) = 0.9
        _Xoan ("Do uon luon cua luoi lua", Float) = 0
        _NhapNhay ("Do nhap nhay sang toi", Float) = 0
        _ThoiGian ("Dong ho lua (LuaToanThan dat = Time.time moi khung)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float _Do, _SangLua, _CaoThan, _DoPhong, _TocDo, _Xoan, _NhapNhay;
        // Dong ho RIENG thay _Time.y: phep thu (menu 90) ve hai thoi diem chinh xac trong CUNG mot khung
        float _ThoiGian;
        float4 _Goc;

        struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 tq : TEXCOORD0;     // toa do lua: ngang theo may quay, doc theo chieu cao than
            float3 nrm : TEXCOORD1;
            float3 wpos : TEXCOORD2;
            float v01 : TEXCOORD3;     // 0 o chan, 1 o dinh dau
        };

        float2 ToaDoLua(float3 w, out float v01)
        {
            float3 phai = UNITY_MATRIX_V[0].xyz;
            float3 d = w - _Goc.xyz;
            v01 = d.y / max(_CaoThan, 0.3);
            return float2(dot(d, phai) / max(_CaoThan, 0.3), v01);
        }

        // Mau lua theo nhiet: do sam -> cam -> vang -> trang vang
        fixed3 MauLua(float h)
        {
            fixed3 c = lerp(fixed3(0.25, 0.02, 0.0), fixed3(0.95, 0.28, 0.03), saturate(h * 2.2));
            c = lerp(c, fixed3(1.0, 0.62, 0.14), saturate(h * 2.2 - 1.0));
            c = lerp(c, fixed3(1.0, 0.92, 0.62), saturate(h * 2.5 - 2.0));
            return c;
        }

        float Nhiet(float2 q, float t)
        {
            // Uon luon: lech ngang theo dam G troi nhanh -> luoi lua lac lu, liem len (lan 4, nguoi dung: "cho thay ro lua
            // dang boc chay" - ban lan 3 soc lua troi deu nhu mot tam anh truot)
            float lech = (tex2D(_MainTex, float2(q.x * 0.7 + 0.11, q.y * 0.6 - t * _TocDo * 0.8)).g - 0.5) * _Xoan;
            q.x += lech * (0.4 + saturate(q.y));
            float s1 = tex2D(_MainTex, float2(q.x * 1.6, q.y * 0.55 - t * _TocDo * 0.55)).r;
            float s2 = tex2D(_MainTex, float2(q.x * 0.9 + 0.37, q.y * 0.40 - t * _TocDo * 0.33)).g;
            float s3 = tex2D(_MainTex, float2(-q.x * 2.3 + 0.61, q.y * 0.9 - t * _TocDo * 0.9)).r;
            float h = s1 * 0.55 + s2 * 0.45 + s3 * 0.30 - 0.55;
            // Nhap nhay: tung mang than bung sang / lu xuong theo nhip nhanh (dam G, toa do thap, troi nhanh)
            float nhip = tex2D(_MainTex, float2(q.x * 0.35 + 0.53, q.y * 0.25 - t * _TocDo * 1.4)).g;
            h *= 1.0 + (nhip - 0.5) * 2.0 * _NhapNhay;
            return h;
        }
        ENDCG

        // ---- Luot 1: lua tren mat than ----
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert (appdata v)
            {
                v2f o;
                float3 w = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(w);
                o.nrm = UnityObjectToWorldNormal(v.normal);
                o.wpos = w;
                o.tq = ToaDoLua(w, o.v01);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _ThoiGian;
                float3 n = normalize(i.nrm);
                float3 vd = normalize(_WorldSpaceCameraPos - i.wpos);
                float rim = 1.0 - saturate(dot(n, vd));
                float h = Nhiet(i.tq, t);
                h += (1.0 - saturate(i.v01)) * 0.22;          // chan lua nong hon
                h += rim * 0.30;                              // mep than lua day hon (lua om quanh than)
                h = saturate(h * 1.6);
                // Vet nut than hong ben duoi lop lua
                float nut = tex2D(_MainTex, i.tq * float2(2.2, 2.2) + float2(0.13, -t * 0.08)).b;
                fixed3 c = MauLua(h) * h + fixed3(1.0, 0.30, 0.04) * nut * 0.35;
                return fixed4(c * _SangLua * _Do, 1);
            }
            ENDCG
        }

        // ---- Luot 2: vo lua phong ra, liem len tren - chi hien o vien than ----
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            v2f vert (appdata v)
            {
                v2f o;
                float3 w = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 n = UnityObjectToWorldNormal(v.normal);
                float v01;
                float2 q = ToaDoLua(w, v01);
                float t = _ThoiGian;
                float soc = tex2Dlod(_MainTex, float4(q.x * 1.6, q.y * 0.55 - t * _TocDo * 0.55, 0, 0)).r;
                // phong theo phap tuyen (dao dong theo soc lua) + nhoc len tren (lua boc len)
                w += n * _DoPhong * (0.45 + soc * 0.9);
                w.y += _DoPhong * 1.6 * soc * saturate(n.y * 0.5 + 0.6);
                o.pos = UnityWorldToClipPos(w);
                o.nrm = n;
                o.wpos = w;
                o.tq = q;
                o.v01 = v01;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _ThoiGian;
                float3 n = normalize(i.nrm);
                float3 vd = normalize(_WorldSpaceCameraPos - i.wpos);
                float rim = pow(1.0 - saturate(dot(n, vd)), 1.6);
                float h = Nhiet(i.tq + float2(0.21, 0.0), t) + 0.18;
                float m = saturate(h * 2.2) * rim;
                m *= 1.0 - smoothstep(0.85, 1.25, i.v01) * 0.6;
                fixed3 c = MauLua(saturate(m * 1.3)) * m;
                return fixed4(c * _SangLua * 0.9 * _Do, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}
