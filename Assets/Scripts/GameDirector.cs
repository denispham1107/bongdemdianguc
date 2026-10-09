using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// NGUOI DIEU KHIEN VAN CHOI: tha quai theo tung dot, dem so quai da diet,
/// bao thang / thua va cho choi lai bang phim R.
/// </summary>
public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance;

    [Header("Tha quai")]

    /// <summary>
    /// NGUOI CHOI CUA MAY NAY - camera bam theo, HUD hien mau cua nguoi nay.
    ///
    /// Van la mot nguoi duy nhat, va dung nhu vay: moi may chi co mot chu.
    /// Nhung tu gio no khong con la NGUOI CHOI DUY NHAT trong canh nua - xem
    /// <see cref="moiNguoi"/>.
    /// </summary>
    public Transform player;

    /// <summary>
    /// MOI NGUOI CHOI TRONG CANH, ke ca minh.
    ///
    /// Truoc day quai chi biet mot nguoi va nham thang vao <c>player</c>. Choi
    /// nhieu nguoi ma van the thi ba nguoi kia dung giua bay quai ma khong con
    /// nao them, con nguoi thu nhat thi bi ca ban do duoi danh.
    ///
    /// Danh sach chu khong phai mang co dinh: nguoi vao giua chung, nguoi guc
    /// nga, nguoi mat ket noi - so nguoi doi lien tuc trong mot van.
    /// </summary>
    public readonly List<Transform> moiNguoi = new List<Transform>();
    public Vector3 arenaCenter = Vector3.zero;
    public float arenaRadius = 34f;

    public int Wave { get; private set; }
    public int Kills { get; private set; }
    /// <summary>So quai con song, TINH CA cac dong quai rieng - con so nguoi choi thay.</summary>
    public int Alive
    {
        get
        {
            // May khach khong rai quai nen ba danh sach duoi day RONG - dem tu
            // do thi HUD ben khach mai mai ghi "Quai con lai: 0" du dang dung
            // giua ca dan. Phai lay con so cua chu phong.
            if (!LaTrongTaiCuaQuai)
            {
                if (daNgheChuPhong) return aliveTuChuPhong;

                // Chua nghe bang so nao (nua giay dau tran, hoac mat goi): dem
                // chinh dan ban sao dang giu con hon ghi 0 giua mot bay quai.
                if (DongBoQuai.Hien != null) return DongBoQuai.Hien.SoQuaiConSong;
            }
            return alive.Count;
        }
    }

    // ---- Con so cua chu phong, cho may khach hien len HUD ----
    bool daNgheChuPhong;
    int aliveTuChuPhong;
    float dotMoiTuChuPhong;

    /// <summary>
    /// May khach nhan bang so cua tran tu chu phong: dot may, da diet bao
    /// nhieu, con bao nhieu, bao lau nua den dot moi.
    ///
    /// Chi chu phong dem duoc ba con so nay, vi chi no rai quai va chi no thay
    /// con quai nao that su chet. May khach tu dem thi lech ngay: no chi thay
    /// ban sao, va ban sao chet muon hon con that mot nhip goi tin.
    /// </summary>
    public void NhanBangSoTuChuPhong(int wave, int kills, int con, float dotMoiSau)
    {
        if (LaTrongTaiCuaQuai) return;
        Wave = wave;
        Kills = kills;
        aliveTuChuPhong = con;
        dotMoiTuChuPhong = dotMoiSau;
        daNgheChuPhong = true;
    }

    public float NextWaveIn
    {
        get
        {
            // Dong ho dot chi chay o chu phong - ben khach no dung yen mai.
            if (!LaTrongTaiCuaQuai && daNgheChuPhong) return Mathf.Max(0f, dotMoiTuChuPhong);
            return Mathf.Max(0f, waveTimer);
        }
    }
    public bool PlayerDead { get; private set; }

    readonly List<Damageable> alive = new List<Damageable>();

    /// <summary>Quai dang song (chi may trong tai quai co day du) - may BOT doc de chon muc tieu.</summary>
    public IReadOnlyList<Damageable> QuaiConSong { get { return alive; } }

    Transform enemyRoot;
    Damageable playerHealth;
    float waveTimer;
    bool waiting;

    void Awake()
    {
        Instance = this;

        // MOI TRAN LA MOT VAN RIENG: cap 1, chua co kinh nghiem, mot diem ky
        // nang, moi ky nang deu khoa. Dat o day chu khong o PlayerController:
        // ban sao cua nguoi choi khac cung la PlayerController, ma chung duoc
        // dung len giua tran - xoa sach o do la moi lan co nguoi vao la ca
        // phong tut ve cap 1.
        CapDo.BatDauTranMoi();
    }

    // ================================================================
    //  DANH SACH NGUOI CHOI
    // ================================================================

    public void ThemNguoiChoi(Transform t)
    {
        if (t == null || moiNguoi.Contains(t)) return;
        moiNguoi.Add(t);
    }

    public void BoNguoiChoi(Transform t)
    {
        moiNguoi.Remove(t);
    }

    /// <summary>
    /// Nguoi choi CON SONG gan <paramref name="tu"/> nhat. Khong con ai song
    /// thi tra ve null.
    ///
    /// Quai hoi lai ham nay thay vi om cung mot muc tieu: nguoi choi chay tan
    /// ra, guc nga, hoac vao giua chung - muc tieu dung phai doi theo.
    ///
    /// Bo qua nguoi da chet: quai dung dam vao mot cai xac thi vua vo ly vua
    /// khien nhung nguoi con song di lai thoai mai.
    /// </summary>
    public Transform GanNhat(Vector3 tu)
    {
        Transform gan = null;
        float gonNhat = float.MaxValue;

        for (int i = 0; i < moiNguoi.Count; i++)
        {
            var t = moiNguoi[i];
            if (t == null) continue;

            var mau = t.GetComponent<Damageable>();
            if (mau != null && mau.IsDead) continue;
            // TANG HINH: quai khong thay (18/09/2026) - bo qua han khoi danh sach chon muc tieu
            if (TangHinh.Dang(t)) continue;

            float d = (t.position - tu).sqrMagnitude;
            if (d < gonNhat) { gonNhat = d; gan = t; }
        }
        return gan;
    }

    /// <summary>Con bao nhieu nguoi con song. Bang 1 la van dau sap xong.</summary>
    public int SoNguoiConSong()
    {
        int n = 0;
        for (int i = 0; i < moiNguoi.Count; i++)
        {
            var t = moiNguoi[i];
            if (t == null) continue;
            var mau = t.GetComponent<Damageable>();
            if (mau == null || !mau.IsDead) n++;
        }
        return n;
    }

    /// <summary>
    /// MAY NAY CO DUOC TU RAI QUAI KHONG.
    ///
    /// Choi mot minh: co. Choi mang: CHI CHU PHONG. May khach khong rai, khong
    /// chay AI, chi ve lai dan quai nghe duoc tu chu phong - xem
    /// <see cref="DongBoQuai"/>.
    ///
    /// De moi may tu rai thi hai nguoi danh hai dan quai khac han nhau ma van
    /// tuong dang choi chung: ca hai man hinh cung ghi "quai con lai 33" mot
    /// cach doc lap.
    /// </summary>
    public static bool LaTrongTaiCuaQuai
    {
        get { return !TranHienTai.DangChoiMang || TranHienTai.LaHost; }
    }

    // ================================================================
    //  DOT QUAI QUANH TUNG NGUOI CHOI
    // ================================================================

    // LUAT DOT QUAI (12/09/2026, nguoi dung chot) - luat DUY NHAT tu khi Act1 bi xoa (27/09/2026):
    //   - Dot 1: quanh MOI nguoi choi hien ra bon con, moi loai mot con:
    //     bo xuong, mu phu thuy, quy cay, quy du. Hai nguoi la tam con.
    //   - Giet het -> doi 30 giay -> dot sau GIONG HET dot truoc, CONG THEM
    //     mot so quai bat ki: dot 2 them 1, dot 3 them 2 (thanh 3), dot 4 them
    //     3 (thanh 6)... cong don.
    //   - Moi dot, quai manh hon dot truoc 5% mau va 5% sat thuong.

    public const float GiayChoDotQuanhNguoi = 30f;

    /// <summary>
    /// Vao tran thi doi DUNG 30 giay moi ra dot quai dau (nguoi dung chot
    /// 13/09/2026). Truoc do chi cho 1,5 giay (choi mot minh) / 6 giay (choi
    /// mang, doi ban sao nguoi khac hien ra).
    /// </summary>
    public const float GiayChoDotDau = 30f;

    /// <summary>
    /// Moi dot con tha them CO DINH 20 con quai loai ngau nhien o vong ngoai:
    /// cach nguoi choi GAN NHAT tu 20 den 25 m (nguoi dung doi 13/09/2026 - truoc
    /// do 10 con o 55-65 m). Khong cong don, nhung van manh
    /// len 5% moi dot nhu moi con khac, va van tinh vao "giet het moi sang dot".
    /// </summary>
    public const int SoQuaiXaMoiDot = 20;
    /// <summary>
    /// Moi NGUOI CHOI THEM trong phong thi vong ngoai them 10 con (nguoi dung 26/09/2026): 1 nguoi 20, 2 nguoi 30, 3 nguoi 40,
    /// 4 nguoi 50. Dem nguoi CON SONG luc ra dot - nhu 4 con quanh moi nguoi (nguoi da chet khong can them quai).
    /// </summary>
    public const int SoQuaiXaThemMoiNguoi = 10;
    public static int SoQuaiXaCho(int soNguoi) { return SoQuaiXaMoiDot + SoQuaiXaThemMoiNguoi * Mathf.Max(0, soNguoi - 1); }

    /// <summary>
    /// BA DOT DAU CHI MOT NUA SO QUAI (nguoi dung 09/10/2026: "giam 50% so luong quai o dot 1, dot 2, dot 3 - giam quanh nguoi va
    /// ca vong ngoai"; chon chi dot 1-3, tu dot 4 nhu cu; quanh moi nguoi 2 LOAI NGAU NHIEN khac nhau thay vi du 4 loai).
    /// Cung ngay nguoi dung: "giam 50% o dot 4 luon" -> dot 1-4; tu dot 5 nhu cu.
    /// Quai cong don va vong ngoai chia doi, lam tron LEN.
    /// </summary>
    public const int SoDotGiamQuai = 4;
    public static bool LaDotGiam(int dot) { return dot >= 1 && dot <= SoDotGiamQuai; }
    /// <summary>So con quanh MOI nguoi trong dot nay (4, ba dot dau 2).</summary>
    public static int SoQuanhMoiNguoi(int dot) { return LaDotGiam(dot) ? 2 : 4; }
    /// <summary>So con vong ngoai cua dot (ba dot dau chia doi, lam tron len).</summary>
    public static int SoQuaiXaChoDot(int soNguoi, int dot)
    {
        int n = SoQuaiXaCho(soNguoi);
        return LaDotGiam(dot) ? (n + 1) / 2 : n;
    }
    /// <summary>So con cong don cua dot (1, 3, 6... - ba dot dau chia doi, lam tron len).</summary>
    public static int SoCongDonChoDot(int dot)
    {
        int n = dot >= 2 ? dot * (dot - 1) / 2 : 0;       // 0, 1, 3, 6, 10...
        return LaDotGiam(dot) ? (n + 1) / 2 : n;
    }
    /// <summary>TONG so quai cua dot voi <paramref name="soNguoi"/> nguoi con song.</summary>
    public static int TongQuaiDot(int soNguoi, int dot)
    {
        return soNguoi * SoQuanhMoiNguoi(dot) + SoCongDonChoDot(dot) + SoQuaiXaChoDot(soNguoi, dot);
    }
    public const float QuaiXaGanNhat = 20f;
    public const float QuaiXaXaNhat = 25f;

    /// <summary>Nhung con quai xa cua dot gan nhat - phep thu (menu 56) doc.</summary>
    public readonly List<Damageable> QuaiXaDotNay = new List<Damageable>();

    /// <summary>Trong dot gan nhat, bao nhieu con quai xa dat DUNG khoang QuaiXaGanNhat-QuaiXaXaNhat.</summary>
    public int SoQuaiXaDungKhoang { get; private set; }

    /// <summary>Moi dot quai manh hon dot truoc bao nhieu (0,05 = 5%).</summary>
    public const float ManhThemMoiDot = 0.05f;

    /// <summary>Bon loai quai co mat quanh MOI nguoi choi o moi dot.</summary>
    static readonly MonsterType[] BonLoaiMoiNguoi =
    {
        MonsterType.Skeleton, MonsterType.Witch, MonsterType.QuyCay, MonsterType.QuyDu
    };

    /// <summary>Tong so quai "bat ki" cong them - cong don qua tung dot.</summary>
    int soThemCongDon;

    /// <summary>Quai dot nay manh gap may lan dot dau.</summary>
    public float HeSoManhDot { get { return Mathf.Pow(1f + ManhThemMoiDot, Mathf.Max(0, Wave - 1)); } }

    /// <summary>
    /// SAT THUONG quai dot dau = 65% sat thuong goc (nguoi dung 27/09/2026: giam 35%). Cac dot sau van +5% moi dot
    /// nhung tinh tu muc 65% nay: dot n = 0,65 x 1,05^(n-1). MAU quai khong doi (van 1,05^(n-1)).
    /// </summary>
    public const float HeSoSatThuongDotDau = 0.65f;

    /// <summary>Sat thuong quai dot nay gap may lan sat thuong GOC cua loai quai (menu 56 doc).</summary>
    public float HeSoSatThuongDot { get { return HeSoSatThuongDotDau * HeSoManhDot; } }

    /// <summary>So hieu cap cho con quai ke tiep. Chi chu phong dung den.</summary>
    ushort soHieuKeTiep = 1;

    /// <summary>Goc cua moi con quai trong canh - de bo dong bo di qua.</summary>
    public Transform GocQuai { get { return enemyRoot; } }

    /// <summary>Dat goc quai tu ben ngoai (may khach: bo dong bo tu dung).</summary>
    public void DatGocQuai(Transform t) { if (enemyRoot == null) enemyRoot = t; }

    void Start()
    {
        enemyRoot = new GameObject("Enemies").transform;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player != null)
        {
            playerHealth = player.GetComponent<Damageable>();
            if (playerHealth != null)
                playerHealth.onDeath += OnPlayerDeath;

            // Choi mot minh thi danh sach chi co mot nguoi - moi thu chay y
            // nhu cu. Choi mang thi nhung nguoi kia duoc them vao sau.
            ThemNguoiChoi(player);
        }

        // Quai chi hien ra quanh nguoi choi theo tung dot. Cho mot nhip truoc dot dau: choi mang thi
        // ban sao cua nhung nguoi kia chi hien ra sau khi bat tay xong (1-2 giay). 30 giay chu khong
        // 1,5 / 6 giay nhu truoc: nguoi dung muon nguoi choi co thoi gian lam quen, mo ky nang dau tien
        // trong Sach phep va chay ra cho minh muon truoc khi quai toi.
        waveTimer = GiayChoDotDau;
        waiting = true;
    }

    /// <summary>
    /// MOT DOT: bon con quanh MOI nguoi choi, cong so quai "bat ki".
    ///
    /// Quanh TUNG nguoi chu khong phai quanh mot nguoi: bon nguoi dung bon goc
    /// ban do ma chi mot nguoi bi quai vay thi ba nguoi kia dung khong.
    /// </summary>
    public void SinhDotQuanhNguoi()
    {
        Wave++;
        if (Wave >= 2) soThemCongDon += Wave - 1;    // cong don: 1, rồi 3, rồi 6...
        int soCongDon = SoCongDonChoDot(Wave);        // ba dot dau chia doi
        int soQuanh = SoQuanhMoiNguoi(Wave);

        float heSo = HeSoManhDot;
        int soNguoi = 0;

        for (int i = 0; i < moiNguoi.Count; i++)
        {
            var t = moiNguoi[i];
            if (t == null) continue;
            soNguoi++;
            if (soQuanh >= BonLoaiMoiNguoi.Length)
            {
                for (int k = 0; k < BonLoaiMoiNguoi.Length; k++)
                    SinhQuanhNguoi(BonLoaiMoiNguoi[k], t, heSo);
            }
            else
            {
                // Ba dot dau: soQuanh LOAI NGAU NHIEN KHAC NHAU (xao tron bon loai, lay dau danh sach)
                var loai = (MonsterType[])BonLoaiMoiNguoi.Clone();
                for (int k = loai.Length - 1; k > 0; k--)
                {
                    int j = Random.Range(0, k + 1);
                    var tg = loai[k]; loai[k] = loai[j]; loai[j] = tg;
                }
                for (int k = 0; k < soQuanh; k++) SinhQuanhNguoi(loai[k], t, heSo);
            }
        }

        // Quai cong them: loai bat ki, quanh mot nguoi bat ki
        for (int i = 0; i < soCongDon; i++)
        {
            var t = NguoiBatKy();
            if (t == null) break;
            SinhQuanhNguoi(BonLoaiMoiNguoi[Random.Range(0, BonLoaiMoiNguoi.Length)], t, heSo);
        }

        // SoQuaiXaCho(so nguoi con song) con o vong ngoai, loai ngau nhien
        SinhQuaiXa(heSo);

        Debug.Log("[GameDirector] Dot " + Wave + ": " + soNguoi + " nguoi x " + soQuanh + " con + "
                  + soCongDon + " con bat ki + " + QuaiXaDotNay.Count + " con xa ("
                  + SoQuaiXaDungKhoang + " dung " + QuaiXaGanNhat + "-" + QuaiXaXaNhat + " m), manh x" + heSo.ToString("F2"));
    }

    static Bounds banDo;
    static bool daDoBanDo;
    static string canhDaDo;

    /// <summary>Hop bao cua MAT DAT (Terrain / lop Ground) - noi duoc phep tha quai.</summary>
    static Bounds BanDo()
    {
        string canh = SceneManager.GetActiveScene().name;
        if (daDoBanDo && canhDaDo == canh) return banDo;
        daDoBanDo = true; canhDaDo = canh;
        int lopDat = LayerMask.NameToLayer("Ground");
        bool co = false;
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (!(c is TerrainCollider) && c.gameObject.layer != lopDat) continue;
            if (!co) { banDo = c.bounds; co = true; } else banDo.Encapsulate(c.bounds);
        }
        if (!co) banDo = new Bounds(Vector3.zero, new Vector3(100f, 10f, 100f));
        return banDo;
    }

    /// <summary>
    /// Tha SoQuaiXaMoiDot con quai o cach nguoi choi GAN NHAT tu QuaiXaGanNhat den
    /// QuaiXaXaNhat met (hien 20 con, 20-25 m).
    ///
    /// Tinh theo nguoi GAN NHAT chu khong theo mot nguoi bat ki: tha cach nguoi A
    /// 22 m ma lai ngay canh nguoi B thi voi B do la quai "sat ben", khong con la
    /// quai vong ngoai.
    ///
    /// Ban do Act2 chi 109 x 109 m. Vanh 20-25 m quanh mot nguoi thuong con rong
    /// cho, nhung nguoi dung sat bo ban do hay bon nguoi dung gan nhau thi co khi
    /// khong du. Luc ay van tha du so con, o cho HOP LE (tren dat, ngoai nuoc,
    /// khong vuong vat can) GAN KHOANG NHAT co the - va dem so con dat dung khoang
    /// vao SoQuaiXaDungKhoang cho phep thu doc, khong im lang gia vo la dat.
    /// </summary>
    void SinhQuaiXa(float heSo)
    {
        QuaiXaDotNay.Clear();
        SoQuaiXaDungKhoang = 0;

        var nguoi = new List<Transform>();
        foreach (var t in moiNguoi)
        {
            if (t == null) continue;
            var m = t.GetComponent<Damageable>();
            if (m != null && m.IsDead) continue;
            nguoi.Add(t);
        }
        if (nguoi.Count == 0) return;

        var ban = BanDo();
        int lopCan = LayerMask.GetMask("Default", "Enemy");

        int soQuaiXa = SoQuaiXaChoDot(nguoi.Count, Wave);
        for (int k = 0; k < soQuaiXa; k++)
        {
            Vector3 chon = Vector3.zero;
            float lechTot = float.MaxValue;
            bool co = false, dung = false;

            for (int lan = 0; lan < 160 && !dung; lan++)
            {
                Vector3 p;
                if (lan % 2 == 0)
                {
                    // Nua so lan: nhieu ngay vanh QuaiXaGanNhat-QuaiXaXaNhat quanh mot nguoi bat ky
                    var t = nguoi[Random.Range(0, nguoi.Count)];
                    float g = Random.Range(0f, Mathf.PI * 2f);
                    float r = Random.Range(QuaiXaGanNhat, QuaiXaXaNhat);
                    p = t.position + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
                }
                else
                {
                    // Nua con lai: rai deu ca ban do - bat duoc cac goc ma vanh tron bo lo
                    p = new Vector3(Random.Range(ban.min.x + 2f, ban.max.x - 2f), 0f,
                                    Random.Range(ban.min.z + 2f, ban.max.z - 2f));
                }
                if (p.x < ban.min.x + 1f || p.x > ban.max.x - 1f || p.z < ban.min.z + 1f || p.z > ban.max.z - 1f)
                    continue;

                // Do khoang cach TRUOC (re) - khong hon cho tot nhat thi khoi ban tia
                float gan = float.MaxValue;
                foreach (var t in nguoi)
                {
                    var d2 = new Vector2(p.x - t.position.x, p.z - t.position.z);
                    gan = Mathf.Min(gan, d2.magnitude);
                }
                float lech = gan < QuaiXaGanNhat ? QuaiXaGanNhat - gan
                           : gan > QuaiXaXaNhat ? gan - QuaiXaXaNhat : 0f;
                if (lech > 0f && lech >= lechTot) continue;

                float y;
                if (!ChoXuatPhat.DungTrenDat(p, out y)) continue;
                p.y = y;
                if (ChoXuatPhat.DuoiNuoc(p) || ChoXuatPhat.VuongVatCan(p)) continue;
                if (Physics.CheckSphere(p + Vector3.up * 1f, 0.6f, lopCan, QueryTriggerInteraction.Ignore)) continue;

                chon = p; lechTot = lech; co = true;
                if (lech <= 0f) dung = true;
            }

            if (!co) continue;

            // Muc tieu = nguoi gan nhat: no se tu chay toi, khong dung im o goc ban do
            Transform ganNhat = nguoi[0];
            float dMin = float.MaxValue;
            foreach (var t in nguoi)
            {
                float dd = Vector3.Distance(chon, t.position);
                if (dd < dMin) { dMin = dd; ganNhat = t; }
            }

            var loai = BonLoaiMoiNguoi[Random.Range(0, BonLoaiMoiNguoi.Length)];
            var go = EnemyFactory.Spawn(loai, chon + Vector3.up * 0.15f, enemyRoot, ganNhat);
            if (go == null) continue;
            DanhSo(go, loai);
            LamManhTheoDot(go, heSo);

            // 60 giay chua ai tim thay thi no tu di tim nguoi gan nhat (EnemyAI.GiayTruyLung)
            var ai = go.GetComponent<EnemyAI>();
            if (ai != null) ai.HenTruyLung(EnemyAI.GiayTruyLung);

            var d = go.GetComponent<Damageable>();
            if (d != null) { d.onDeath += OnEnemyDeath; alive.Add(d); QuaiXaDotNay.Add(d); }
            if (dung) SoQuaiXaDungKhoang++;

            // Cho con sau khong chon trung cho con nay (CheckSphere doc vat ly)
            Physics.SyncTransforms();
        }
    }

    Transform NguoiBatKy()
    {
        int n = 0;
        for (int i = 0; i < moiNguoi.Count; i++) if (moiNguoi[i] != null) n++;
        if (n == 0) return null;
        int chon = Random.Range(0, n);
        for (int i = 0; i < moiNguoi.Count; i++)
        {
            if (moiNguoi[i] == null) continue;
            if (chon-- == 0) return moiNguoi[i];
        }
        return null;
    }

    /// <summary>Khoang cach tu nguoi choi toi con quai vua hien ra.</summary>
    public const float GanNhatQuanhNguoi = 7f;
    public const float XaNhatQuanhNguoi = 13f;

    /// <summary>
    /// Tha mot con quai quanh mot nguoi choi, roi lam no manh len theo dot.
    ///
    /// Manh len bang cach nhan THANG vao mau va sat thuong cua con vua sinh -
    /// khong sua prefab, khong sua gia tri goc, nen dot sau khong bi cong don
    /// nham len dot truoc.
    /// </summary>
    void SinhQuanhNguoi(MonsterType loai, Transform nguoi, float heSo)
    {
        for (int lan = 0; lan < 14; lan++)
        {
            float goc = Random.Range(0f, 360f);
            float xa = Random.Range(GanNhatQuanhNguoi, XaNhatQuanhNguoi);
            Vector3 pos = nguoi.position + Quaternion.Euler(0f, goc, 0f) * new Vector3(0f, 0f, xa);

            Vector3 tuTam = pos - arenaCenter;
            if (tuTam.magnitude > arenaRadius) pos = arenaCenter + tuTam.normalized * (arenaRadius * 0.92f);

            pos.y = VfxFactory.GroundY(pos) + 0.15f;

            if (Physics.CheckSphere(pos + Vector3.up * 1f, 0.6f,
                                    LayerMask.GetMask("Default", "Enemy"), QueryTriggerInteraction.Ignore))
                continue;

            var go = EnemyFactory.Spawn(loai, pos, enemyRoot, nguoi);
            if (go == null) return;
            DanhSo(go, loai);
            LamManhTheoDot(go, heSo);

            var d = go.GetComponent<Damageable>();
            if (d != null) { d.onDeath += OnEnemyDeath; alive.Add(d); }
            return;
        }
    }

    /// <summary>
    /// Nhan mau theo he so cua dot, sat thuong theo he so dot x <see cref="HeSoSatThuongDotDau"/>. Chi hai duong sinh quai
    /// cua luat dot goi ham nay (quanh nguoi choi + vong ngoai).
    /// </summary>
    void LamManhTheoDot(GameObject go, float heSo)
    {
        if (go == null) return;

        var d = go.GetComponent<Damageable>();
        if (d != null && heSo > 1.0001f)
        {
            d.maxHealth *= heSo;
            d.health = d.maxHealth;
        }

        // KHONG bo qua o dot dau nhu truoc (heSo = 1): dot dau van phai giam con 65%
        var ai = go.GetComponent<EnemyAI>();
        if (ai != null)
        {
            float heSoSat = heSo * HeSoSatThuongDotDau;
            ai.attackDamage *= heSoSat;
            ai.satThuongCau *= heSoSat;  // don danh xa: cau lua (ca chay - tinh theo sat thuong no), thien thach, tia set
        }
    }

    void Update()
    {
        if (PlayerDead)
        {
            if (DuocChoiLai && Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            if (Input.GetKeyDown(KeyCode.Escape)) BackToMenu();
            return;
        }

        alive.RemoveAll(d => d == null || d.IsDead);

        // Nhip sinh quai chi chay o may lam trong tai. May khach ma cung dem
        // gio thi no se tu de ra mot dot quai rieng khong ai khac nhin thay.
        if (LaTrongTaiCuaQuai)
        {
            // Giet het -> doi 30 giay -> dot sau, dong hinh quanh tung nguoi
            if (waiting)
            {
                waveTimer -= Time.deltaTime;
                if (waveTimer <= 0f) { waiting = false; SinhDotQuanhNguoi(); }
            }
            else if (alive.Count == 0)
            {
                waiting = true;
                waveTimer = GiayChoDotQuanhNguoi;
            }
        }

        if (DuocChoiLai && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.Escape)) BackToMenu();
    }

    /// <summary>
    /// Phim R (nap lai man) co duoc dung khong. Choi mot minh: co. Choi mang:
    /// KHONG, ke ca khi da chet.
    ///
    /// Nap lai man giua tran mang la pha tran ma khong bao gi ca: chu phong
    /// bam R thi ca dan quai bi rai lai tu dau, hai may lech nhau hoan toan;
    /// nguoi khach bam R thi bat tay lai tu dau trong khi chu phong van dang
    /// cho ban sao cu cua ho dung do. Va R lai nam ngay canh WASD - bam nham
    /// la chuyen se xay ra.
    /// </summary>
    public static bool DuocChoiLai { get { return !TranHienTai.DangChoiMang; } }

    /// <summary>Ve man hinh chinh (neu scene MainMenu co trong danh sach build).</summary>
    public static void BackToMenu()
    {
        // Roi tran mang thi dong kenh TRUOC: may kia nhan ra ngay la minh da
        // di, thay vi phai doi het thoi gian cho moi biet - xem
        // DongBoTran.GiayMatKetNoi.
        if (TranHienTai.DangChoiMang) KenhTrucTiep.Dong();

        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Gan so hieu cho mot con quai vua sinh, de may kia goi dung ten no.
    ///
    /// Goi o MOI cho sinh quai - bo sot mot cho thi
    /// nhung con sinh ra o do se khong bao gio hien len may khach, va khong ai
    /// bao gi ca.
    /// </summary>
    public void DanhSo(GameObject go, MonsterType loai)
    {
        if (go == null) return;
        var n = go.GetComponent<NhanDangQuai>();
        if (n == null) n = go.AddComponent<NhanDangQuai>();
        n.id = soHieuKeTiep++;
        n.loai = loai;

        // Ghi ten nghe NGAY, khong doi nhip quet: con quai sinh giua tran ma
        // ra don trong nua giay dau thi don ay khong sang duoc may kia.
        if (DongBoQuai.Hien != null) DongBoQuai.Hien.NgheConNay(go);
    }

    void OnEnemyDeath(Damageable d)
    {
        Kills++;

        // Bang diem cuoi tran: con nay la cong cua AI. Chi may lam trong tai
        // cua quai moi ghi - may khach chi ve lai dan quai nghe duoc, dem o do
        // la dem ca nhung con minh khong he giet.
        if (LaTrongTaiCuaQuai && KetTran.Hien != null && d != null)
            KetTran.Hien.GhiQuaiChet(d.keDanhCuoi);

        // KINH NGHIEM cho nguoi ha no - cung chi may trong tai cua quai moi
        // chia, vi chi no biet con nay chet vi tay ai.
        if (LaTrongTaiCuaQuai && d != null) ChiaKinhNghiem(d);

        // BINH MAU / BINH MANA (25% / 15%) - cung chi may trong tai quai gieo,
        // roi bao ca phong (QuanLyBinhRoi)
        if (LaTrongTaiCuaQuai && d != null) QuanLyBinhRoi.GieoKhiQuaiChet(d.transform.position);

        alive.Remove(d);
    }

    /// <summary>
    /// Chia kinh nghiem cua mot con quai vua chet cho nguoi ha no.
    ///
    /// Ke ha la nhan vat CUA MAY NAY thi cong thang; la nguoi choi khac thi
    /// GUI mot goi kinh nghiem sang may ho - ho khong chay AI quai nen khong
    /// the tu biet con nay chet vi tay minh.
    /// </summary>
    void ChiaKinhNghiem(Damageable quai)
    {
        var keDanh = quai.keDanhCuoi;
        if (keDanh == null || keDanh.IsDead) return;

        var nhanDang = quai.GetComponent<NhanDangQuai>();
        int kn = CapDo.KnCuaQuai(nhanDang != null ? nhanDang.loai : MonsterType.Skeleton);
        if (kn <= 0) return;

        // Nhan vat cua may nay (hoac may BOT chu phong dieu khien): mau KHONG do may khac quyet - cong vao bang cap
        // CUA CHINH NO (BOT co bang rieng, 08/10/2026)
        if (!keDanh.mauDoMayKhacQuyet)
        {
            var pcKe = keDanh.GetComponent<PlayerController>();
            (pcKe != null ? pcKe.Cap : CapDo.CuaMay).Them(kn);
            return;
        }

        // Nguoi choi khac: gui sang may ho
        if (KetTran.Hien == null) return;
        byte ghe = KetTran.Hien.GheCuaDamageable(keDanh);
        if (ghe == 255) return;

        var db = Object.FindAnyObjectByType<DongBoTran>();
        if (db != null) db.GuiGoi(GoiTin.VietKinhNghiem(ghe, kn));
    }

    void OnPlayerDeath(Damageable d)
    {
        PlayerDead = true;
    }
}
