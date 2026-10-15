using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// TIENG THEO VI TRI TRONG CANH (15/10/2026, nguoi dung - dau tien la Qua cau lua): tieng phat TAI mot cho trong the gioi (hoac BAM
/// theo mot vat dang bay). Nguoi dung chon:
///   - XA NHO, LECH TRAI / PHAI: AudioSource 3D (spatialBlend 1) - lech trai / phai theo cho tren man hinh (AudioListener o camera).
///     Spread <see cref="DoRong"/> giu do rong STEREO (quy tac nguoi dung: moi am thanh stereo); tren WebGL Unity dung PannerNode
///     "equalpower" - nhan vao stereo thi giu hai kenh, chi doi can bang. Do nho theo khoang cach KHONG de Unity tinh (camera cach
///     nhan vat ~15-20 m): duong cong rolloff phang, am luong tu tinh theo khoang cach NGANG toi nhan vat cua may nay
///     (<see cref="HeSoKhoangCach"/>: to nhat trong <see cref="GanDayDu"/> m, con <see cref="MucMepTamNhin"/> o mep tam nhin 25 m).
///   - SUONG MU: nhan them <see cref="TamNhin.MucThay"/> theo VI TRI tieng, MOI KHUNG ("theo vi tri qua"): qua cua nguoi khac bay tu
///     suong vao tam thi bat dau nghe, qua cua minh bay ra ngoai 25 m thi tat dan, no trong suong thi im. Che do Doi chia se tam nhin
///     dong doi -> nghe ca quanh dong doi (nho theo khoang cach toi minh).
/// Am luong cuoi = muc x thanh "Hiệu ứng" (<see cref="CaiDatAmThanh.HieuUngDangNghe"/>). Dang chay phep thu (co ChayThuMang) thi IM
/// tru <see cref="NhacNen.BatTrongPhepThu"/> - van dem <see cref="SoLan"/>.
///
/// Mot vat the DontDestroyOnLoad, <see cref="SoGiongToiDa"/> giong (moi giong mot vat con co AudioSource). Het giong thi lay giong
/// NHO NHAT; tieng bay toi da <see cref="SoTiengBayToiDa"/> (dot quai dong). Doi man thi dung het.
/// Clip WebGL giai ma bat dong bo: nap san luc scene nap (<see cref="NapTruoc"/>, TiengDong goi), clip chua san thi giong cho.
/// </summary>
public class TiengViTri : MonoBehaviour
{
    public enum Loai { QuaCauLuaBan = 0, QuaCauLuaBay = 1, QuaCauLuaNo = 2 }

    // Mau "C - Lua that" (nguoi dung chon trong 3 mau) - CongCu/AmThanh/tieng_qua_cau_lua.py, chep nguyen byte tu
    // CongCu/AmThanh/QuaCauLuaThu/C_lua_that_{ban,bay,no}.wav. Tieng bay la VONG LAP 2 s lien mach.
    static readonly string[] duong = { "AmThanh/KyNang/QuaCauLuaBan", "AmThanh/KyNang/QuaCauLuaBay", "AmThanh/KyNang/QuaCauLuaNo" };
    static readonly bool[] laVong = { false, true, false };
    static readonly AudioClip[] clip = new AudioClip[3];

    public const int SoGiongToiDa = 16;
    public const int SoTiengBayToiDa = 6;
    public const float GanDayDu = 4f;
    public const float MucMepTamNhin = 0.25f;
    /// <summary>Ngoai 25 m (chi nghe duoc nho tam nhin dong doi): tiep tuc nho dan ve 0 trong chung nay met.</summary>
    public const float XaThem = 20f;
    public const float DoRong = 90f;
    /// <summary>Thoi gian muot am luong (vao / ra tam nhin, dung tieng bay) - tranh tieng "tach".</summary>
    public const float GiayMuot = 0.1f;
    /// <summary>Tieng bay moi qua nho hon (3-5 qua bay cung luc, lech pha + cao do nen cong lai ~x1,7).</summary>
    public const float HeSoBay = 0.45f;
    /// <summary>Qua thu hai tro di no cung cho cung luc (ca chum trung mot ke) nho lai - khong thi ba tieng no chong nhau qua to.</summary>
    public const float HeSoNoTrung = 0.45f;

    /// <summary>Dem tieng da yeu cau theo loai (ke ca luc im trong phep thu / bi suong che) - phep thu doc / dat lai.</summary>
    public static readonly int[] SoLan = new int[3];
    /// <summary>Tieng mot lan bi BO vi luc phat nam trong suong (MucThay = 0).</summary>
    public static int SoBiSuongChe;

    public class Giong
    {
        public AudioSource nguon;
        public Loai loai;
        public Transform theo;
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
    readonly List<Vector4> noGanDay = new List<Vector4>();   // xyz cho no, w luc no
    static AnimationCurve duongPhang;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void KhiKhoiDong() { for (int i = 0; i < SoLan.Length; i++) SoLan[i] = 0; SoBiSuongChe = 0; }

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
        {
            if (clip[i] == null) clip[i] = Resources.Load<AudioClip>(duong[i]);
            if (clip[i] != null && clip[i].loadState == AudioDataLoadState.Unloaded) clip[i].LoadAudioData();
        }
    }

    static AudioClip LayClip(Loai l)
    {
        int i = (int)l;
        // Clip tinh: thoat Play trong Editor la clip bi huy - kiem bang null cua Unity
        if (clip[i] == null) clip[i] = Resources.Load<AudioClip>(duong[i]);
        if (clip[i] == null) Debug.LogWarning("[TiengViTri] thieu " + duong[i]);
        return clip[i];
    }

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
        b.Phat(Loai.QuaCauLuaBan, cho, null, 1f, Random.Range(0.97f, 1.03f));
    }

    /// <summary>Tieng BAY (vong lap) bam theo qua cau - goi <see cref="Dung"/> khi qua no / bi xoa.</summary>
    public static Giong QuaCauLuaBay(Transform qua)
    {
        SoLan[(int)Loai.QuaCauLuaBay]++;
        if (qua == null) return null;
        return DamBao().Phat(Loai.QuaCauLuaBay, qua.position, qua, HeSoBay, Random.Range(0.94f, 1.06f));
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
        b.Phat(Loai.QuaCauLuaNo, cho, null, heSo, heSo < 1f ? Random.Range(0.9f, 1.06f) : Random.Range(0.97f, 1.03f));
    }

    /// <summary>Dung tieng bay cua <paramref name="qua"/> (qua no / bi xoa): tat dan <see cref="GiayMuot"/> roi tra giong. Giong da bi
    /// lay cho tieng khac (het giong) thi khong con bam qua nay - bo qua, khong dung nham tieng khac.</summary>
    public static void Dung(Giong g, Transform qua)
    {
        if (g == null || qua == null || g.theo != qua) return;
        g.dung = true;
        g.theo = null;
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

    Giong Phat(Loai loai, Vector3 cho, Transform theo, float heSo, float caoDo)
    {
        if (Time.unscaledTime >= lucKiemPhepThu) KiemPhepThu();
        if (Im) return null;
        bool vong = laVong[(int)loai];
        var c = LayClip(loai);
        if (c == null) return null;

        var moi = new Giong { loai = loai, viTri = cho, theo = theo, heSo = heSo };
        float dich = MucDich(moi);
        // Tieng mot lan phat ra trong suong: bo han (khong ai nghe - khoi chiem giong)
        if (!vong && dich <= 0.001f) { SoBiSuongChe++; return null; }

        if (vong)
        {
            int soBay = 0; Giong nhoNhat = null;
            foreach (var g in giong)
                if (!g.Ranh && g.loai == Loai.QuaCauLuaBay && !g.dung)
                {
                    soBay++;
                    if (nhoNhat == null || g.muc < nhoNhat.muc) nhoNhat = g;
                }
            if (soBay >= SoTiengBayToiDa)
            {
                if (nhoNhat.muc >= dich) return null;
                TraVe(nhoNhat);
            }
        }
        var gg = LayGiongRanh(dich);
        if (gg == null) return null;

        gg.loai = loai; gg.viTri = cho; gg.theo = theo; gg.heSo = heSo; gg.dung = false;
        var a = gg.nguon;
        a.transform.position = cho;
        a.clip = c;
        a.loop = vong;
        a.pitch = caoDo;
        // Tieng mot lan: muc dung ngay (phan len cua tieng nam trong clip); tieng bay: to dan tu 0 (khong "tach")
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
        // Tieng bay: bat dau o cho ngau nhien trong vong lap - 3-5 qua cung luc khong trung pha (cong lai thanh mot tieng to, phang)
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
        g.dangPhat = false; g.dangCho = false; g.dung = false; g.theo = null; g.muc = 0f;
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
            // Vat dang bam bi xoa (qua cau bi Destroy khong qua Explode, doi man...) -> tat dan
            if (g.nguon.loop && g.theo == null) g.dung = true;
            if (g.theo != null) g.viTri = g.theo.position;
            g.nguon.transform.position = g.viTri;
            g.muc = Mathf.MoveTowards(g.muc, MucDich(g), buoc);
            g.nguon.volume = g.muc * hieuUng;
            if ((g.dung && g.muc <= 0f) || (!g.nguon.loop && !g.nguon.isPlaying)) TraVe(g);
        }
    }
}
