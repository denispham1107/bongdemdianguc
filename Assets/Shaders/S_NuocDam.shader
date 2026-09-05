// MAT NUOC DAM LAY.
//
// Nuoc nghia dia: duc, toi, tu dong nhung khong lang. Khong lam nuoc trong veo
// kieu ho boi - o day nuoc phai ra ve dong lau ngay, day bun.
//
// Nam thu tao ra cai ve do:
//
//   1. SONG THAT, dich han dinh luoi len xuong (xem ham Song). Chi doi anh gon
//      thi nhin ky van thay mat nuoc phang li; phai cho no phong len that thi
//      moi ra nuoc dang dong day.
//
//   2. HAI LOP GON troi nguoc chieu nhau, khac nhip - mot lop troi thi ra vet
//      keo thang, hai lop nguoc chieu moi ra kieu mat nuoc lan tan.
//
//   3. ONG ANH: dom sang li ti nhay theo song. Lay nhieu tan so cao nang len
//      luy thua lon nen chi con lai vai dom, roi cho no truot theo mat song -
//      dung kieu anh trang vo vun tren mat nuoc gon.
//
//   4. BO NUOC DONG: mep nuoc KHONG dung yen. Song dang len thi nuoc tran ra
//      xa hon, song rut thi nuoc lui vao - dung nhu nuoc vo bo. Bot bam theo
//      duong bo dang dao dong do nen bot cung tran ra rut vao theo.
//
//      Lam duoc la nho luoi nuoc THO RA NGOAI bo tinh, va do sau luu trong mau
//      dinh CO CA GIA TRI AM (cho dat cao hon mat nuoc). Cong song vao do sau
//      am do, luc nao tong con duong thi cho do co nuoc. Truoc day toi cat luoi
//      dung o bo tinh va lay do sau co dinh, nen quanh vung nuoc luon co mot
//      vong vien sang dung im nhu net ke bang but.
//
//   5. TRONG SUOT THEO DO SAU: cho nong thi thay day bun, cho sau thi duc han.
//
// DO SAU DUOC NUONG SAN VAO MAU DINH (kenh do) luc dung luoi mat nuoc - luc do
// da co terrain nen do duoc chinh xac tung diem. Ban dau toi doc
// _CameraDepthTexture cho tien, nhung trong Built-in RP camera khong tu sinh ra
// bo dem do sau; thieu no thi do duc ra 0 va ca mat nuoc trong suot nhu khong
// co gi. Nuong san vao luoi thi chay o dau cung dung.
//
// RIA NGOAI CUNG CUA LUOI phai nam o cho dat CAO HON dinh song. Luoi duoc cat
// tai cho dat = mucNuoc + TranToiDa (xem Act2DamLay), ma TranToiDa lon hon bien
// do song, nen du song dang o dinh thi ria luoi van nam duoi mat dat va bi dat
// che di - khong bao gio thay canh luoi.
//
// Luoi mat nuoc khong co UV, nen toa do anh duoc tinh thang tu vi tri trong
// khong gian - xem uv o ham vert.
Shader "Diablo25D/NuocDam"
{
    Properties
    {
        _Color      ("Mau nuoc nong", Color) = (0.16, 0.20, 0.17, 1)
        _ColorSau   ("Mau nuoc sau", Color) = (0.05, 0.08, 0.08, 1)
        _MepColor   ("Mau vien mep", Color) = (0.42, 0.50, 0.46, 1)
        _DoSau      ("Bao sau thi duc han", Range(0.05, 4)) = 0.85

        _TiLeGon    ("Nhip gon song", Float) = 0.75
        _TocDoGon   ("Toc do troi", Float) = 0.035
        _DoGon      ("Do manh cua gon", Range(0, 1)) = 0.55

        _CaoSong    ("Do cao song (met)", Range(0, 0.25)) = 0.055
        _NhipSong   ("Nhip song", Float) = 1.15
        _TocSong    ("Toc do song", Float) = 1.30

        _BotColor   ("Mau bot nuoc", Color) = (0.80, 0.85, 0.82, 1)
        _BotDay     ("Bot lan vao sau bao nhieu (met)", Range(0.01, 1)) = 0.30
        _BotManh    ("Do dac cua bot", Range(0, 1)) = 0.75

        // Hai so nay PHAI khop voi Act2DamLay.LechAm / ThangSau, khong thi
        // duong bo lech han di mot doan
        _LechAm     ("Do sau am luu duoc toi (met)", Float) = 0.25
        _ThangSau   ("Thang do sau trong mau dinh (met)", Float) = 1.5
        _MemBo      ("Do mem cua mep nuoc (met)", Range(0.005, 0.2)) = 0.045

        _OngAnh     ("Do ong anh", Range(0, 3)) = 1.15
        _OngAnhNhip ("Nhip dom ong anh", Float) = 5.5

        _DoBong     ("Do bong", Range(0, 1)) = 0.82
        _VienManh   ("Do manh cua vien mep", Range(0.5, 8)) = 3.2
    }

    SubShader
    {
        // Trong suot: phai ve SAU moi thu khac, va khong ghi vao bo dem do sau
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade vertex:vert
        #pragma target 3.0

        fixed4 _Color;
        fixed4 _ColorSau;
        fixed4 _MepColor;
        fixed4 _BotColor;
        half _DoSau;
        half _TiLeGon;
        half _TocDoGon;
        half _DoGon;
        half _CaoSong;
        half _NhipSong;
        half _TocSong;
        half _BotDay;
        half _BotManh;
        half _LechAm;
        half _ThangSau;
        half _MemBo;
        half _OngAnh;
        half _OngAnhNhip;
        half _DoBong;
        half _VienManh;

        struct Input
        {
            float2 uvNuoc;
            float3 worldPos;
            float3 viewDir;
            float doSau;        // nuong san trong mau dinh, tinh bang met
            float2 docSong;     // do doc cua mat song, de xoay phap tuyen
        };

        // ---- Song: ba nhip chong len nhau cho khoi ra song hinh sin deu tam ----
        float Song(float2 xz, float t)
        {
            return sin(xz.x * _NhipSong * 1.00 + t * _TocSong * 1.00) * 0.45
                 + sin(xz.y * _NhipSong * 1.63 - t * _TocSong * 0.82) * 0.33
                 + sin((xz.x + xz.y) * _NhipSong * 2.45 + t * _TocSong * 1.51) * 0.22;
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float3 tg = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.uvNuoc = tg.xz * _TiLeGon;

            // Doi tu 0..1 trong mau dinh ve met that. Co the RA SO AM - do la
            // cho dat cao hon mat nuoc, tuc ngoai bo tinh.
            o.doSau = v.color.r * _ThangSau - _LechAm;

            float t = _Time.y;

            // Ca mat nuoc len xuong theo song, KE CA O MEP. Truoc day toi tat
            // song o mep cho khoi tran ra ngoai chao - nhung tat di thi bo nuoc
            // dung im, thanh ra cai vong vien co dinh. Gio de no len xuong that,
            // va luoi da tho san ra ngoai bo nen co cho ma tran.
            v.vertex.y += Song(tg.xz, t) * _CaoSong;

            // Do doc cua mat song, tinh bang sai phan - de ben surf xoay phap
            // tuyen theo. Thieu buoc nay thi hinh phong len ma anh sang khong
            // doi, nhin nhu tam vai chu khong ra nuoc.
            const float e = 0.35;
            o.docSong = float2(
                (Song(tg.xz + float2(e, 0), t) - Song(tg.xz - float2(e, 0), t)) * _CaoSong / (2 * e),
                (Song(tg.xz + float2(0, e), t) - Song(tg.xz - float2(0, e), t)) * _CaoSong / (2 * e));
        }

        // Nhieu tron, du dung cho gon nuoc ma khong can anh ngoai
        float Nhieu(float2 p)
        {
            float2 i = floor(p), f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = frac(sin(dot(i + float2(0, 0), float2(127.1, 311.7))) * 43758.5453);
            float b = frac(sin(dot(i + float2(1, 0), float2(127.1, 311.7))) * 43758.5453);
            float c = frac(sin(dot(i + float2(0, 1), float2(127.1, 311.7))) * 43758.5453);
            float d = frac(sin(dot(i + float2(1, 1), float2(127.1, 311.7))) * 43758.5453);
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float t = _Time.y;

            // Hai lop gon TROI NGUOC CHIEU nhau - mot lop thi chi ra vet keo thang
            float2 troi1 = IN.uvNuoc + float2(t * _TocDoGon, t * _TocDoGon * 0.6);
            float2 troi2 = IN.uvNuoc * 1.7 - float2(t * _TocDoGon * 0.8, t * _TocDoGon * 1.3);
            float g = Nhieu(troi1) * 0.6 + Nhieu(troi2) * 0.4;

            // Phap tuyen: gon nho cong voi do doc cua song lon
            float e = 0.06;
            float gx = Nhieu(troi1 + float2(e, 0)) - Nhieu(troi1 - float2(e, 0));
            float gz = Nhieu(troi1 + float2(0, e)) - Nhieu(troi1 - float2(0, e));
            float3 phap = normalize(float3(
                -gx * _DoGon * 4 - IN.docSong.x * 3.0,
                -gz * _DoGon * 4 - IN.docSong.y * 3.0,
                1));
            o.Normal = phap;

            // ---- DO SAU THUC: dao dong theo song ----
            // Song len thi cho dat hoi cao hon mat nuoc cung thanh co nuoc; song
            // rut thi nuoc lui lai. Day chinh la cho mep nuoc tran qua rut lai.
            float songTaiDay = Song(IN.worldPos.xz, t);
            float sauThuc = IN.doSau + songTaiDay * _CaoSong;

            // Ngoai duong bo dang dao dong thi khong co nuoc. Cat MEM chu khong
            // cat thang, khong thi lai ra dung cai vien sac canh nhu cu.
            float coNuoc = smoothstep(0, _MemBo, sauThuc);
            if (coNuoc <= 0.001) discard;

            float sau = saturate(sauThuc / max(_DoSau, 0.001));

            fixed3 mau = lerp(_Color.rgb, _ColorSau.rgb, sau);
            mau += (g - 0.5) * 0.06;                 // van gon lam mau khong phang li

            // ---- Vien mep sang khi nhin cheo ----
            half cheo = 1 - saturate(dot(normalize(IN.viewDir), float3(0, 0, 1)));
            half vien = pow(cheo, _VienManh);
            mau = lerp(mau, _MepColor.rgb, vien * 0.22);

            // ---- BOT MEP BO ----
            // Bam theo do sau THUC, nen bo tran ra thi bot tran theo.
            float bot = saturate(1 - sauThuc / max(_BotDay, 0.001));
            bot *= bot;
            float ranBot = Nhieu(IN.uvNuoc * 3.4 + float2(t * 0.06, -t * 0.04));
            bot = saturate(bot * (0.55 + ranBot * 0.9)) * _BotManh;

            mau = lerp(mau, _BotColor.rgb, bot);

            // ---- ONG ANH ----
            // Nhieu tan so cao, nang luy thua lon nen chi con vai dom sang, roi
            // cho truot theo mat song. Chi loe o cho nghieng - dung nhu anh sang
            // vo vun tren song.
            float2 diem = IN.uvNuoc * _OngAnhNhip + float2(t * 0.20, -t * 0.14)
                        + IN.docSong * 2.2;
            // Luy thua PHAI LON. De 14 thi vung quanh dinh nhieu con rong, cac
            // dom dinh lai thanh mang trang loa chu khong ra dom li ti tren song.
            float dom = Nhieu(diem);
            dom = pow(saturate(dom), 24.0);
            float nghieng = saturate(length(IN.docSong) * 1.6);
            half3 loe = _OngAnh * dom * (0.25 + nghieng * 0.75) * (1 - bot)
                      * fixed3(0.85, 0.95, 1.0);

            o.Albedo = mau;
            o.Emission = loe;

            // Bot thi nham, mat nuoc thi bong
            o.Smoothness = lerp(_DoBong, 0.18, bot);
            o.Metallic = 0;

            // Cho nong thi lo day bun ra, cho sau thi duc han.
            // Bot va vien mep deu dac hon cho ra khoi nuoc.
            o.Alpha = saturate(lerp(0.30, 0.94, sau) + vien * 0.25 + bot * 0.55) * coNuoc;
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
