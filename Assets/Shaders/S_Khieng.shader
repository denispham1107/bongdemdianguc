// KHIENG BAO VE - mot quy BONG BONG bao quanh nhan vat.
//
// MAU CHUAN: bong bong xa phong. Gan nhu VO HINH - nhin xuyen qua thay ro mat
// dat, thay ro ca quai vat dung ben trong. Cai lam nguoi ta "thay" duoc no chi
// gom bon thu, khong co thu thu nam:
//
//   1. VIEN mong o mep, cho nhin gan tiep tuyen. Day la thu nhin ra dau tien.
//   2. VAN XA CU loang nhe tren mat, day dan ve phia ria.
//   3. DOM LAP LANH rai rac, moi dom tu nhap nhay theo nhip rieng.
//   4. VANH SANG o CHAN vom, ngay tren mat dat.
//
// BAN TRUOC LA MOT THU KHAC HAN: vom vang duc voi nhung tia set to chay ngoan
// ngoeo khap mat cau. Nhin ra mot qua cau nang luong, khong ra bong bong - va
// tia set thi che mat nhan vat ben trong.
//
// BON CAI BAY DA VAP KHI VIET SHADER NAY, ca bon deu cho ra CUNG MOT trieu
// chung la qua cau trang duc kin mit:
//
//   - Dat "Blend SrcAlpha One" cung luc voi "alpha:fade": chi thi alpha:fade da
//     tu dat blend roi, dat them la hai cai da nhau.
//   - Tinh fresnel bang dot(viewDir, o.Normal): trong surface shader, viewDir
//     nam o KHONG GIAN TIEP TUYEN, ma trong khong gian do phap tuyen luon la
//     (0,0,1). o.Normal khong tu ghi thi bang 0, fresnel ra 1 o moi diem.
//   - Dung IN.viewDir tren mot LUOI KHONG CO TANGENT. ProcMesh khong sinh
//     tangent, nen ma tran TBN hong va fresnel chi con nhan HAI gia tri 0 hoac
//     1. Cach chua: tinh fresnel trong KHONG GIAN THE GIOI voi phap tuyen va vi
//     tri tu truyen tu vertex shader - khong dung den tangent mot chut nao.
//   - Quen "noforwardadd": ham chieu sang tu viet tra thang Albedo, ma Unity
//     chay them mot pass cho MOI ngon den, nen mau bi cong nhieu lan.
Shader "Diablo25D/Khieng"
{
    Properties
    {
        // VANG KIM. Ba mau, dung cho ba lop khac nhau.
        _VienColor  ("Mau VIEN mep vom", Color) = (1.0, 0.86, 0.46, 1)
        _VanColor   ("Mau VAN xa cu", Color) = (1.0, 0.80, 0.38, 1)
        _DomColor   ("Mau DOM lap lanh", Color) = (1.0, 0.97, 0.82, 1)

        // ---- Vien ----
        _VienManh   ("Do MANH cua vien (cang lon cang mong)", Range(1, 10)) = 6.0
        _VienDam    ("Do dam cua vien", Range(0, 2)) = 0.90
        _VienChoi   ("Do choi cua vien", Range(1, 8)) = 3.4

        // ---- Van xa cu ----
        _VanDam     ("Do dam cua van", Range(0, 1)) = 0.17
        _VanCo      ("Co cua van", Range(1, 12)) = 4.2
        _VanTroi    ("Toc do van troi", Range(0, 1)) = 0.10

        // ---- Dom lap lanh ----
        _DomCo      ("Mat do dom (cang lon cang nhieu)", Range(6, 40)) = 26.0
        _DomThua    ("Do thua cua dom (cang lon cang it)", Range(0.85, 0.995)) = 0.940
        _DomTo      ("Do to cua moi dom", Range(0.05, 0.5)) = 0.26
        _DomChoi    ("Do choi cua dom", Range(0, 8)) = 4.0

        // ---- Vanh chan vom ----
        _ChanDay    ("Be day vanh chan", Range(0.02, 0.4)) = 0.13
        _ChanChoi   ("Do choi vanh chan", Range(0, 6)) = 2.2

        // ---- Chung ----
        _MangNen    ("Do duc cua MANG (de rat nho)", Range(0, 0.2)) = 0.012
        _MatTruoc   ("Nua vom truoc mat nhan vat mo di con", Range(0.05, 1)) = 0.35
        _TrongSuot  ("Do trong suot chung", Range(0.1, 2)) = 0.85
        _PhatSang   ("Do phat sang chung", Range(0.5, 4)) = 1.6

        // Hai cai nay do Khieng.cs dat moi khung - dung doi ten
        _Yeu        ("Mau khieng con lai 0..1", Range(0, 1)) = 1
        _Loe        ("Loe sang khi vua an don", Range(0, 1)) = 0
        _LoeColor   ("Mau loe khi an don", Color) = (1.0, 0.95, 0.80, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 200
        Cull Off
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Khong alpha:fade vertex:vert noforwardadd
        #pragma target 3.0

        fixed4 _VienColor;
        fixed4 _VanColor;
        fixed4 _DomColor;
        fixed4 _LoeColor;

        half _VienManh, _VienDam, _VienChoi;
        half _VanDam, _VanCo, _VanTroi;
        half _DomCo, _DomThua, _DomTo, _DomChoi;
        half _ChanDay, _ChanChoi;
        half _MangNen, _MatTruoc, _TrongSuot, _PhatSang;
        half _Yeu, _Loe;

        // CHI HAI bien truyen xuong. Them mot float3 nua la vuot han muc
        // interpolator cua pass ForwardBase (11 tren toi da 10) va shader khong
        // bien dich duoc. Van va dom lay luon phap tuyen THE GIOI - vom khong
        // nghieng bao gio nen n.y van dung la "cao thap tren vom".
        struct Input
        {
            float3 phapTG;      // phap tuyen the gioi - TU TRUYEN, khong nho tangent
            float3 viTriTG;     // vi tri the gioi
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.phapTG = UnityObjectToWorldNormal(v.normal);
            o.viTriTG = mul(unity_ObjectToWorld, v.vertex).xyz;
        }

        // Khong nhan anh sang tu den nao ca - khien tu phat sang.
        half4 LightingKhong(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }

        // ---- Nhieu ----
        float Bam(float3 p)
        {
            return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453);
        }

        float Nhieu(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = lerp(lerp(lerp(Bam(i + float3(0,0,0)), Bam(i + float3(1,0,0)), f.x),
                                lerp(Bam(i + float3(0,1,0)), Bam(i + float3(1,1,0)), f.x), f.y),
                           lerp(lerp(Bam(i + float3(0,0,1)), Bam(i + float3(1,0,1)), f.x),
                                lerp(Bam(i + float3(0,1,1)), Bam(i + float3(1,1,1)), f.x), f.y), f.z);
            return a;
        }

        /// Van xa cu: nhieu nhieu lop, troi cham theo thoi gian.
        float Van(float3 n, float t)
        {
            float3 p = n * _VanCo + float3(0, t * _VanTroi, 0);
            float v = Nhieu(p) * 0.60 + Nhieu(p * 2.1 + 11.3) * 0.28 + Nhieu(p * 4.3 + 5.7) * 0.12;
            // Chi giu nhung mang DAM nhat - de nguyen thi van phu deu ca qua cau
            // va bong bong lai duc len.
            return smoothstep(0.52, 0.78, v);
        }

        /// Dom lap lanh: chia mat cau thanh o, mot so it o co mot dom o giua.
        float LapLanh(float3 n, float t)
        {
            float3 p = n * _DomCo;
            float3 i = floor(p);
            float3 f = frac(p) - 0.5;

            float h = Bam(i);
            if (h < _DomThua) return 0.0;

            // Moi dom mot nhip rieng, khong thi ca dam nhap nhay dong loat nhu
            // mot cai den bao.
            float h2 = Bam(i + 3.17);
            float nhay = 0.35 + 0.65 * saturate(sin(t * (1.6 + h2 * 4.0) + h2 * 6.28318));

            float r = length(f);
            return smoothstep(_DomTo, 0.0, r) * nhay;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float t = _Time.y;

            // Fresnel trong KHONG GIAN THE GIOI - khong dung tangent.
            //
            // Lay abs() vi Cull Off ve CA HAI MAT: mat xa quay lung lai camera
            // nen dot ra am. Khong lay abs thi vom trong nhu bi cat doi.
            float3 phap = normalize(IN.phapTG);
            float3 n = phap;
            float3 huongNhin = normalize(_WorldSpaceCameraPos - IN.viTriTG);
            half doc = dot(huongNhin, phap);
            half cheo = 1 - saturate(abs(doc));

            // NUA VOM TRUOC MAT NHAN VAT mo di nhieu. Cull Off ve ca hai mat, ma
            // nhan vat dung GIUA hai lop - lop truoc dam bang lop sau thi nhan
            // vat bi phu mo. Dua vao DAU cua tich vo huong chu khong vao VFACE
            // hay culling: cach nay khong phu thuoc chieu cuon tam giac.
            half heSoMat = lerp(1.0, _MatTruoc, smoothstep(-0.35, 0.35, doc));

            // ---- 1. VIEN o mep ----
            half vien = pow(cheo, _VienManh) * _VienDam;

            // ---- 2. VAN xa cu, DAY DAN RA RIA ----
            // O giua vom gan nhu khong co gi. Phu deu ca qua cau thi bong bong
            // duc lai va che mat nhan vat ben trong.
            half raRia = pow(cheo, 0.75);
            half van = Van(n, t) * _VanDam * (0.10 + raRia * 1.5);

            // ---- 3. DOM LAP LANH ----
            half dom = LapLanh(n, t);

            // ---- 4. VANH SANG o CHAN vom ----
            // Chan vom la noi phap tuyen nam NGANG, tuc n.y quanh 0.
            half chan = 1.0 - smoothstep(0.0, _ChanDay, abs(n.y));

            // Khieng cang yeu cang nga do - liec mot cai la biet no sap vo.
            // Vom da vang kim thi do cam trong y het luc con nguyen, phai lech
            // han sang DO SAM.
            fixed3 mauVien = lerp(fixed3(0.92, 0.14, 0.05), _VienColor.rgb, saturate(_Yeu));
            fixed3 mauVan  = lerp(fixed3(0.80, 0.10, 0.04), _VanColor.rgb,  saturate(_Yeu));
            fixed3 mauDom  = lerp(fixed3(1.00, 0.55, 0.35), _DomColor.rgb,  saturate(_Yeu));

            // ---- GOP CAC LOP ----
            //
            // Mau cuoi cung la TRUNG BINH CO TRONG SO, tuc chia cho TONG do dac.
            // Phai chia: cong thang thi vung chi co mang mong ra mau toi nhung
            // van co do dac, lam canh phia sau XAM DI thay vi sang len - ca cai
            // vom trong nhu mot vet ban mo tren man hinh.
            half aMang = _MangNen;
            half aVien = vien;
            half aVan  = van;
            half aDom  = dom;
            half aChan = chan * 0.55;
            half aLoe  = _Loe * 0.30;
            half tong  = aMang + aVien + aVan + aDom + aChan + aLoe;

            fixed3 mau = (mauVien * aMang * 0.6
                        + mauVien * aVien * _VienChoi
                        + mauVan  * aVan  * 1.6
                        + mauDom  * aDom  * _DomChoi
                        + mauVien * aChan * _ChanChoi
                        + _LoeColor.rgb * aLoe * 2.0) / max(tong, 0.0001);

            // NHAN VAO MAU, khong dung den o.Alpha.
            //
            // Do TRONG SUOT do o.Alpha quyet dinh, do SANG do o.Albedo quyet
            // dinh - hai thu doc lap. Muon sang hon ma van trong nhu cu thi chi
            // duoc dong vao mau.
            o.Albedo = mau * _PhatSang;

            half dac = (aMang + aVien + aVan + aChan + aLoe) * _TrongSuot + aDom;
            o.Alpha = saturate(dac * heSoMat / (1.0 + _MatTruoc) * 1.6);
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
