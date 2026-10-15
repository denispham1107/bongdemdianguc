using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TIENG THEO VI TRI TRONG CANH (15/10/2026, nguoi dung - Qua cau lua, roi Thien thach): tieng phat TAI mot cho trong the gioi (hoac BAM
/// theo mot vat dang bay). Nguoi dung chon:
///   - XA NHO, LECH TRAI / PHAI: AudioSource 3D (spatialBlend 1) - lech trai / phai theo cho tren man hinh (AudioListener o camera).
///     Spread <see cref="DoRong"/> giu do rong STEREO (quy tac nguoi dung: moi am thanh stereo; 90 cu bop hep - do o loa tuong quan
///     0,85 du clip 0,25-0,62 - nguoi dung: "van con qua mono"); tren WebGL Unity dung PannerNode
///     "equalpower" - nhan vao stereo thi giu hai kenh, chi doi can bang (WebGL KHONG co panStereo). Do nho theo khoang cach KHONG de
///     Unity tinh (camera cach nhan vat ~15-20 m): duong cong rolloff phang, am luong tu tinh theo khoang cach NGANG toi nhan vat cua
///     may nay (<see cref="HeSoKhoangCach"/>: to nhat trong <see cref="GanDayDu"/> m, con <see cref="MucMepTamNhin"/> o mep tam nhin 25 m).
///   - SUONG MU: nhan them <see cref="TamNhin.MucThay"/> theo VI TRI tieng, MOI KHUNG ("theo vi tri qua"): qua cua nguoi khac bay tu
///     suong vao tam thi bat dau nghe, qua cua minh bay ra ngoai 25 m thi tat dan, no trong suong thi im. Che do Doi chia se tam nhin
///     dong doi -> nghe ca quanh dong doi (nho theo khoang cach toi minh).
/// Am luong cuoi = muc x thanh "Hiệu ứng" (<see cref="CaiDatAmThanh.HieuUngDangNghe"/>). Dang chay phep thu (co ChayThuMang) thi IM
/// tru <see cref="NhacNen.BatTrongPhepThu"/> - van dem <see cref="SoLan"/>.
///
/// Moi giong co mot CHU (Transform - qua cau, thien thach, vung lua, cai cay): chu bi xoa thi giong tat dan; <see cref="Dung"/> /
/// <see cref="DatHeSo"/> chi tac dung khi giong con thuoc chu ay (giong da bi lay cho tieng khac thi bo qua). Giong BAM THEO chu (qua
/// dang bay) hoac DUNG YEN tai mot cho (lua chay).
///
/// Mot vat the DontDestroyOnLoad, <see cref="SoGiongToiDa"/> giong (moi giong mot vat con co AudioSource). Het giong thi lay giong
/// NHO NHAT; vong lap moi loai co tran rieng (<see cref="TranVong"/>). Doi man thi dung het.
/// Clip WebGL giai ma bat dong bo: nap san luc scene nap (<see cref="NapTruoc"/>, TiengDong goi), clip chua san thi giong cho.
/// </summary>
public class TiengViTri : MonoBehaviour
{
    public enum Loai { QuaCauLuaBan = 0, QuaCauLuaBay = 1, QuaCauLuaNo = 2, ThienThachRoi = 3, ThienThachNo = 4, LuaChay = 5 }

    // Qua cau lua: ban 3 "B - Khoi lua lon" (15/10/2026, CongCu/AmThanh/tieng_qua_cau_lua_3.py, QuaCauLuaThu3/B_khoi_lua_lon_*.wav -
    // mo hinh lua that: liem lua / xi hoi / lach tach ngau nhien khong deu). BA BIEN THE tieng ban + BA bien the tieng no, moi lan chon
    // ngau nhien + lech cao do +-6% (nguoi dung: ban truoc "don dieu"). Tieng bay la VONG LAP 3 s.
    // Thien thach: ban 3 "A - Dien anh" (16/10/2026, CongCu/AmThanh/tieng_thien_thach_3.py, ThienThachThu3/A_dien_anh_*.wav - nguoi dung:
    // ban truoc "con qua mono", "chua giong thien thach roi va vo tung"): gam lao toi sang dan + to theo 1 / khoang cach, ram nhu sam, rit
    // roi kieu bom; no = nut + "bum" + da nghien + da vo nay + dat da do. BA bien the roi + BA bien the no (chon ngau nhien + cao do +-6%).
    // Lua chay = lua that (mo hinh Qua cau lua ban 3) VONG LAP 3 s, dung cho vung lua sau vu no VA cay / bia / nha mo bi dot (CayChay).
    // Moi file chep nguyen byte.
    static readonly string[][] duong = {
        new[] { "AmThanh/KyNang/QuaCauLuaBan1", "AmThanh/KyNang/QuaCauLuaBan2", "AmThanh/KyNang/QuaCauLuaBan3" },
        new[] { "AmThanh/KyNang/QuaCauLuaBay" },
        new[] { "AmThanh/KyNang/QuaCauLuaNo1", "AmThanh/KyNang/QuaCauLuaNo2", "AmThanh/KyNang/QuaCauLuaNo3" },
        new[] { "AmThanh/KyNang/ThienThachRoi1", "AmThanh/KyNang/ThienThachRoi2", "AmThanh/KyNang/ThienThachRoi3" },
        new[] { "AmThanh/KyNang/ThienThachNo1", "AmThanh/KyNang/ThienThachNo2", "AmThanh/KyNang/ThienThachNo3" },
        new[] { "AmThanh/KyNang/LuaChay" } };
    static readonly bool[] laVong = { false, true, false, false, false, true };
    static readonly AudioClip[][] clip = { new AudioClip[3], new AudioClip[1], new AudioClip[3], new AudioClip[3], new AudioClip[3], new AudioClip[1] };
    /// <summary>Lech cao do toi da cua tieng ban / no Qua cau lua va roi / no Thien thach (+-6%, nguoi dung chon).</summary>
    public const float LechCaoDo = 0.06f;

    public const int SoGiongToiDa = 24;
    public const int SoTiengBayToiDa = 6;
    public const int SoTiengLuaToiDa = 4;
    public static int TranVong(Loai l) { return l == Loai.LuaChay ? SoTiengLuaToiDa : SoTiengBayToiDa; }
    public const float GanDayDu = 4f;
    public const float MucMepTamNhin = 0.25f;
    /// <summary>Ngoai 25 m (chi nghe duoc nho tam nhin dong doi): tiep tuc nho dan ve 0 trong chung nay met.</summary>
    public const float XaThem = 20f;
    /// <summary>Spread cua moi giong (chi Editor / ban may - WebGL dung PannerNode, khong doc spread). Bien tinh de phep thu menu 125 do
    /// cac muc; muc chon ghi o <see cref="DoRongMacDinh"/>.</summary>
    public static float DoRong = DoRongMacDinh;
    /// <summary>120 (menu 125 C2 do, 15/10/2026, no 'Khoi lua lon' o 6 m): tuong quan hai kenh o loa 90 -> 0,63 · 120 -> 0,42 · 150 -> 0,25 ·
    /// 180 -> 0,21; con nghe ro ben (phai / trai khi no ben phai) 2,91 · 2,03 · 1,49 · 1,04 - 150 tro len mat ben.</summary>
    public const float DoRongMacDinh = 120f;
    /// <summary>Thoi gian muot am luong (vao / ra tam nhin, dung tieng bay) - tranh tieng "tach".</summary>
    public const float GiayMuot = 0.1f;
    /// <summary>Tieng bay moi qua nho hon (3-5 qua bay cung luc, lech pha + cao do nen cong lai ~x1,7).</summary>
    public const float HeSoBay = 0.45f;
    /// <summary>Qua thu hai tro di no cung cho cung luc (ca chum trung mot ke) nho lai - khong thi ba tieng no chong nhau qua to.</summary>
    public const float HeSoNoTrung = 0.45f;
    /// <summary>Tieng thien thach roi (3-5 qua noi nhau 0,35 s).</summary>
    public const float HeSoRoi = 0.8f;
    /// <summary>Tieng lua chay: vung lua sau vu no (3-5 vung chong nhau) / cay - bia - nha mo dang chay (lua to hon).</summary>
    public const float HeSoLuaVung = 0.6f, HeSoLuaCay = 0.9f;

    /// <summary>Dem tieng da yeu cau theo loai (ke ca luc im trong phep thu / bi suong che) - phep thu doc / dat lai.</summary>
    public static readonly int[] SoLan = new int[6];
    /// <summary>Tieng mot lan bi BO vi luc phat nam trong suong (MucThay = 0).</summary>
    public static int SoBiSuongChe;
    /// <summary>Giong vua phat gan nhat (phep thu doc clip / cao do bien the).</summary>
    public static Giong GiongMoiNhat;

    public class Giong
    {
        public AudioSource nguon;
        public Loai loai;
        /// <summary>Chu cua giong (vat phat ra tieng). null = tieng tu do (vu no).</summary>
        public Transform theo;
        /// <summary>true = vi tri bam theo chu moi khung (qua dang bay); false = dung yen o viTri (lua chay).</summary>
        public bool bamTheo, coChu;
        public Vector3 viTri;
        public float heSo, muc;
        public bool dung, dangCho, dangPhat;
        public float hanCho;
        public bool Ranh { get { return !dangPhat && !dangCho; } }
        /// <summary>Muc chua nhan thanh Hiệu ứng: heSo x khoang cach x tam nhin (phep thu doc).</summary>
        public float MucDangDat { get { return muc; } }
    }

    public static TiengViTri Ban { get; private set; }
    readonly List<Giong> giong = new List<Giong>();
    public static IReadOnlyList<Giong> CacGiong { get { return Ban != null ? Ban.giong : (IReadOnlyList<Giong>)new Giong[0]; } }

    bool coPhepThu;
    float lucKiemPhepThu;
    float lucBanCuoi = -9f; Vector3 choBanCuoi;
    readonly List<Vector4> noGanDay = new List<Vector4>();   // xyz cho no, w luc no (Qua cau lua)
    static AnimationCurve duongPhang;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong() { for (int i = 0; i < SoLan.Length; i++) SoLan[i] = 0; SoBiSuongChe = 0; DoRong = DoRongMacDinh; }

    static TiengViTri DamBao()
    {
        if (Ban != null) return Ban;
        var go = new GameObject("TiengViTri");
        DontDestroyOnLoad(go);
        Ban = go.AddComponent<TiengViTri>();
        return Ban;
    }

    void Awake()
    {
        if (Ban != null && Ban != this) { Destroy(gameObject); return; }
        Ban = this;
        SceneManager.sceneLoaded += KhiNapCanh;
        if (duongPhang == null) duongPhang = AnimationCurve.Constant(0f, 1f, 1f);
        for (int i = 0; i < SoGiongToiDa; i++)
        {
            var con = new GameObject("Giong" + i);
            con.transform.SetParent(transform, false);
            var a = con.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = 1f;
            a.rolloffMode = AudioRolloffMode.Custom;
            a.SetCustomCurve(AudioSourceCurveType.CustomRolloff, duongPhang);
            a.minDistance = 1f;
            a.maxDistance = 1000f;
            a.spread = DoRong;
            a.dopplerLevel = 0f;
            a.priority = 64;
            a.bypassReverbZones = true;
            a.volume = 0f;
            giong.Add(new Giong { nguon = a });
        }
        KiemPhepThu();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= KhiNapCanh;
        if (Ban == this) Ban = null;
    }

    void KhiNapCanh(Scene c, LoadSceneMode kieu)
    {
        if (kieu != LoadSceneMode.Single) return;
        foreach (var g in giong) TraVe(g);
        noGanDay.Clear();
        KiemPhepThu();
    }

    void KiemPhepThu()
    {
        coPhepThu = FindAnyObjectByType<ChayThuMang>() != null;
        lucKiemPhepThu = Time.unscaledTime + 1f;
    }

    bool Im { get { return coPhepThu && !NhacNen.BatTrongPhepThu; } }

    /// <summary>Nap san moi clip (TiengDong.NapTruoc goi luc scene nap) - WebGL giai ma bat dong bo.</summary>
    public static void NapTruoc()
    {
        for (int i = 0; i < clip.Length; i++)
            for (int j = 0; j < clip[i].Length; j++)
            {
                if (clip[i][j] == null) clip[i][j] = Resources.Load<AudioClip>(duong[i][j]);
                if (clip[i][j] != null && clip[i][j].loadState == AudioDataLoadState.Unloaded) clip[i][j].LoadAudioData();
            }
    }

    /// <summary>Clip cua loai - nhieu bien the thi chon NGAU NHIEN mot.</summary>
    static AudioClip LayClip(Loai l)
    {
        int i = (int)l;
        int j = clip[i].Length > 1 ? Random.Range(0, clip[i].Length) : 0;
        // Clip tinh: thoat Play trong Editor la clip bi huy - kiem bang null cua Unity
        if (clip[i][j] == null) clip[i][j] = Resources.Load<AudioClip>(duong[i][j]);
        if (clip[i][j] == null) Debug.LogWarning("[TiengViTri] thieu " + duong[i][j]);
        return clip[i][j];
    }

    static float CaoDoNgauNhien() { return Random.Range(1f - LechCaoDo, 1f + LechCaoDo); }

    // ================================================================
    //  QUA CAU LUA (Fireball goi: qua cua nguoi choi, ban sao mang, BOT, quai Phu thuy, Lua dia nguc, qua nay)
    // ================================================================

    /// <summary>Tieng BAN: mot tieng cho ca chum (cac qua sinh cung luc, cach nhau &lt; 2,5 m chi keu mot lan).</summary>
    public static void QuaCauLuaBan(Vector3 cho)
    {
        SoLan[(int)Loai.QuaCauLuaBan]++;
        var b = DamBao();
        if (Time.time - b.lucBanCuoi < 0.06f && (cho - b.choBanCuoi).sqrMagnitude < 2.5f * 2.5f) return;
        b.lucBanCuoi = Time.time; b.choBanCuoi = cho;
        b.Phat(Loai.QuaCauLuaBan, cho, null, false, 1f, CaoDoNgauNhien());
    }

    /// <summary>Tieng BAY (vong lap) bam theo qua cau - goi <see cref="Dung"/> khi qua no / bi xoa.</summary>
    public static Giong QuaCauLuaBay(Transform qua)
    {
        SoLan[(int)Loai.QuaCauLuaBay]++;
        if (qua == null) return null;
        return DamBao().Phat(Loai.QuaCauLuaBay, qua.position, qua, true, HeSoBay, Random.Range(0.94f, 1.06f));
    }

    /// <summary>Tieng VA CHAM + NO. Cac vu no gan nhau cung luc (ca chum trung mot cho) - qua thu hai tro di nho lai.</summary>
    public static void QuaCauLuaNo(Vector3 cho)
    {
        SoLan[(int)Loai.QuaCauLuaNo]++;
        var b = DamBao();
        float heSo = 1f;
        for (int i = b.noGanDay.Count - 1; i >= 0; i--)
        {
            var v = b.noGanDay[i];
            if (Time.time - v.w > 0.15f) { b.noGanDay.RemoveAt(i); continue; }
            if (((Vector3)v - cho).sqrMagnitude < 4f * 4f) heSo = HeSoNoTrung;
        }
        b.noGanDay.Add(new Vector4(cho.x, cho.y, cho.z, Time.time));
        b.Phat(Loai.QuaCauLuaNo, cho, null, false, heSo, CaoDoNgauNhien());
    }

    // ================================================================
    //  THIEN THACH (ThienThach goi - cua nguoi choi, ban sao mang, BOT, Quy du) + LUA CHAY (VungLua, CayChay)
    // ================================================================

    /// <summary>Tieng ROI: bat dau luc thien thach hien hinh (bat dau roi), bam theo no, to dan toi luc cham dat (~0,8 s trong clip).
    /// Thien thach no thi goi <see cref="Dung"/> - tieng roi tat ngay, tieng no thay vao.</summary>
    public static Giong ThienThachRoi(Transform tt)
    {
        SoLan[(int)Loai.ThienThachRoi]++;
        if (tt == null) return null;
        return DamBao().Phat(Loai.ThienThachRoi, tt.position, tt, true, HeSoRoi, CaoDoNgauNhien());
    }

    /// <summary>Tieng VA CHAM + NO cua thien thach.</summary>
    public static void ThienThachNo(Vector3 cho)
    {
        SoLan[(int)Loai.ThienThachNo]++;
        DamBao().Phat(Loai.ThienThachNo, cho, null, false, 1f, CaoDoNgauNhien());
    }

    /// <summary>Tieng LUA CHAY (vong lap) DUNG YEN tai <paramref name="cho"/>, thuoc <paramref name="chu"/> (vung lua / cai cay): chu doi
    /// do to bang <see cref="DatHeSo"/> (lua yeu dan) va goi <see cref="Dung"/> khi tat.</summary>
    public static Giong LuaChay(Vector3 cho, Transform chu, float heSo, float caoDo)
    {
        SoLan[(int)Loai.LuaChay]++;
        if (chu == null) return null;
        return DamBao().Phat(Loai.LuaChay, cho, chu, false, heSo, caoDo);
    }

    /// <summary>Doi he so cua giong (lua yeu dan / to len) - chi khi giong con thuoc <paramref name="chu"/>.</summary>
    public static void DatHeSo(Giong g, Transform chu, float heSo)
    {
        if (g == null || chu == null || g.theo != chu || g.Ranh) return;
        g.heSo = heSo;
    }

    /// <summary>Dung tieng cua <paramref name="chu"/> (qua no / lua tat / bi xoa): tat dan <see cref="GiayMuot"/> roi tra giong. Giong da
    /// bi lay cho tieng khac (het giong) thi khong con thuoc chu nay - bo qua, khong dung nham tieng khac.</summary>
    public static void Dung(Giong g, Transform chu)
    {
        if (g == null || chu == null || g.theo != chu) return;
        g.dung = true;
        g.theo = null;
        g.coChu = false;
    }

    // ================================================================
    //  AM LUONG
    // ================================================================

    /// <summary>Nho theo khoang cach NGANG toi nhan vat cua may nay (chet roi: toi xac - tam nhin cung giu quanh xac).</summary>
    public static float HeSoKhoangCach(Vector3 p)
    {
        Vector3 tam;
        var dir = GameDirector.Instance;
        if (dir != null && dir.player != null) tam = dir.player.position;
        else if (Camera.main != null) tam = Camera.main.transform.position;
        else return 1f;
        float dx = p.x - tam.x, dz = p.z - tam.z;
        float d = Mathf.Sqrt(dx * dx + dz * dz);
        if (d <= GanDayDu) return 1f;
        if (d <= TamNhin.BanKinh) return Mathf.Lerp(1f, MucMepTamNhin, (d - GanDayDu) / (TamNhin.BanKinh - GanDayDu));
        return Mathf.Lerp(MucMepTamNhin, 0f, (d - TamNhin.BanKinh) / XaThem);
    }

    static float MucDich(Giong g) { return g.dung ? 0f : g.heSo * HeSoKhoangCach(g.viTri) * TamNhin.MucThay(g.viTri); }

    Giong Phat(Loai loai, Vector3 cho, Transform chu, bool bamTheo, float heSo, float caoDo)
    {
        if (Time.unscaledTime >= lucKiemPhepThu) KiemPhepThu();
        if (Im) return null;
        bool vong = laVong[(int)loai];
        var c = LayClip(loai);
        if (c == null) return null;

        var moi = new Giong { loai = loai, viTri = cho, heSo = heSo };
        float dich = MucDich(moi);
        // Tieng mot lan TU DO (vu no) phat ra trong suong: bo han (khong ai nghe - khoi chiem giong). Tieng co chu (bay, roi, lua)
        // van giu giong de vat di vao tam nhin thi nghe.
        if (chu == null && dich <= 0.001f) { SoBiSuongChe++; return null; }

        if (vong)
        {
            int soVong = 0; Giong nhoNhat = null;
            foreach (var g in giong)
                if (!g.Ranh && g.loai == loai && !g.dung)
                {
                    soVong++;
                    if (nhoNhat == null || g.muc < nhoNhat.muc) nhoNhat = g;
                }
            if (soVong >= TranVong(loai))
            {
                if (nhoNhat.muc >= dich) return null;
                TraVe(nhoNhat);
            }
        }
        var gg = LayGiongRanh(dich);
        if (gg == null) return null;

        gg.loai = loai; gg.viTri = cho; gg.theo = chu; gg.coChu = chu != null; gg.bamTheo = bamTheo && chu != null;
        gg.heSo = heSo; gg.dung = false;
        var a = gg.nguon;
        a.transform.position = cho;
        a.clip = c;
        a.loop = vong;
        a.pitch = caoDo;
        a.spread = DoRong;
        GiongMoiNhat = gg;
        // Tieng mot lan: muc dung ngay (phan len cua tieng nam trong clip); vong lap: to dan tu 0 (khong "tach")
        gg.muc = vong ? 0f : dich;
        a.volume = gg.muc * CaiDatAmThanh.HieuUngDangNghe;
        if (c.loadState != AudioDataLoadState.Loaded)
        {
            if (c.loadState == AudioDataLoadState.Unloaded) c.LoadAudioData();
            gg.dangCho = true;
            gg.hanCho = Time.unscaledTime + (vong ? 2f : 0.3f);
        }
        else BatDau(gg);
        return gg;
    }

    static void BatDau(Giong g)
    {
        var a = g.nguon;
        g.dangCho = false;
        g.dangPhat = true;
        // Vong lap: bat dau o cho ngau nhien - nhieu nguon cung luc khong trung pha (cong lai thanh mot tieng to, phang)
        if (a.loop && a.clip.samples > 0) a.timeSamples = Random.Range(0, a.clip.samples);
        a.Play();
    }

    Giong LayGiongRanh(float dich)
    {
        Giong nhoNhat = null;
        foreach (var g in giong)
        {
            if (g.Ranh) return g;
            if (nhoNhat == null || g.muc < nhoNhat.muc) nhoNhat = g;
        }
        if (nhoNhat == null || nhoNhat.muc >= dich) return null;
        TraVe(nhoNhat);
        return nhoNhat;
    }

    static void TraVe(Giong g)
    {
        if (g.nguon != null) { g.nguon.Stop(); g.nguon.clip = null; g.nguon.volume = 0f; }
        g.dangPhat = false; g.dangCho = false; g.dung = false; g.theo = null; g.coChu = false; g.bamTheo = false; g.muc = 0f;
    }

    void Update()
    {
        if (Time.unscaledTime >= lucKiemPhepThu) KiemPhepThu();
        float buoc = Time.unscaledDeltaTime / GiayMuot;
        float hieuUng = CaiDatAmThanh.HieuUngDangNghe;
        foreach (var g in giong)
        {
            if (g.Ranh) continue;
            if (Im) { TraVe(g); continue; }
            if (g.dangCho)
            {
                var c = g.nguon.clip;
                if (c == null || c.loadState == AudioDataLoadState.Failed || Time.unscaledTime > g.hanCho) { TraVe(g); continue; }
                if (c.loadState == AudioDataLoadState.Loaded) BatDau(g);
                else continue;
            }
            // Chu bi xoa (qua cau bi Destroy khong qua Explode, vung lua het, doi man...) -> tat dan
            if (g.coChu && g.theo == null) { g.dung = true; g.coChu = false; }
            if (g.bamTheo && g.theo != null) g.viTri = g.theo.position;
            g.nguon.transform.position = g.viTri;
            g.muc = Mathf.MoveTowards(g.muc, MucDich(g), buoc);
            g.nguon.volume = g.muc * hieuUng;
            if ((g.dung && g.muc <= 0f) || (!g.nguon.loop && !g.nguon.isPlaying)) TraVe(g);
        }
    }
}
