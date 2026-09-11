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
    public float minSpawnDistance = 14f;
    public float maxSpawnDistance = 26f;

    [Header("Nhip do")]
    public int startingCount = 4;
    public float waveDelay = 4f;

    [Header("Quai rai san khap ban do luc vao man")]
    // De MAC DINH BANG 0 trong code, roi bat rieng o canh nao can. GameDirector
    // nay dung cho CA Act1 lan Act2 - de mac dinh khac 0 la ca hai man deu co
    // them quai.
    [Tooltip("So mu phu thuy rai san khap ban do ngay khi vao man")]
    public int soPhuThuyRaiSan = 0;

    [Tooltip("So bo xuong rai san khap ban do ngay khi vao man")]
    public int soBoXuongRaiSan = 0;

    [Tooltip("Quai rai san khong duoc sinh gan nguoi choi hon khoang nay")]
    public float raiSanCachXaNhat = 9f;

    [Header("Quy du - dong quai rieng, khong theo nhip dot")]
    [Tooltip("So Quy du rai khap ban do ngay khi vao man")]
    public int soQuyDuBanDau = 7;

    [Tooltip("Cu bao nhieu giay thi them mot dot Quy du")]
    public float chuKyThemQuyDu = 120f;

    [Tooltip("Moi dot them bao nhieu con")]
    public int soQuyDuMoiDot = 5;

    [Tooltip("Toi da bao nhieu Quy du song cung luc. 0 = khong gioi han (coi chung tut khung hinh)")]
    public int quyDuToiDa = 40;

    [Header("Quy cay - dong quai rieng thu hai")]
    [Tooltip("So Quy cay rai khap ban do ngay khi vao man")]
    public int soQuyCayBanDau = 4;

    [Tooltip("Cu bao nhieu giay thi them mot dot Quy cay")]
    public float chuKyThemQuyCay = 240f;

    [Tooltip("Moi dot them bao nhieu con")]
    public int soQuyCayMoiDot = 10;

    [Tooltip("Toi da bao nhieu Quy cay song cung luc. 0 = khong gioi han")]
    public int quyCayToiDa = 40;

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
            return alive.Count + quyDu.Count + quyCay.Count;
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

    /// <summary>Rieng so Quy du con song.</summary>
    public int QuyDuSong { get { return quyDu.Count; } }

    /// <summary>Rieng so Quy cay con song.</summary>
    public int QuyCaySong { get { return quyCay.Count; } }

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

    // Quy du dem RIENG, khong nam trong 'alive'.
    //
    // Chung tu sinh them mai mai, ma nhip dot lai dua vao "alive.Count == 0"
    // de biet khi nao tha dot moi. Cho chung vao chung mot danh sach thi so do
    // khong bao gio ve 0 nua va CAC DOT QUAI THUONG DUNG HAN.
    readonly List<Damageable> quyDu = new List<Damageable>();
    float quyDuTimer;

    readonly List<Damageable> quyCay = new List<Damageable>();
    float quyCayTimer;

    Transform enemyRoot;
    Damageable playerHealth;
    float waveTimer;
    bool waiting;

    void Awake()
    {
        Instance = this;
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

        // May khach khong rai con nao ca - ca dan den tu chu phong.
        if (LaTrongTaiCuaQuai)
        {
            // Rai quai khap ban do TRUOC khi dot dau bat dau. Bon nay dung san o
            // cho cua chung, khong lien quan gi den nhip dot.
            RaiQuaiKhapBanDo();
            RaiDongRieng(MonsterType.QuyDu, soQuyDuBanDau, quyDu, 0.5f, "Quy du");
            RaiDongRieng(MonsterType.QuyCay, soQuyCayBanDau, quyCay, 0.25f, "Quy cay");
        }

        waveTimer = 2.5f;
        waiting = true;
        quyDuTimer = chuKyThemQuyDu;
        quyCayTimer = chuKyThemQuyCay;
    }

    /// <summary>
    /// RAI QUAI KHAP BAN DO ngay khi vao man.
    ///
    /// Khac han <see cref="SpawnOne"/>: ham kia tha quai quanh NGUOI CHOI trong
    /// mot vanh dai 14-26 met, dung cho tung dot. Ham nay rai deu khap ca dau
    /// truong de nguoi choi di toi dau cung gap.
    ///
    /// Dung XOAN OC VANG chu khong rai ngau nhien: rai ngau nhien thi cho tum
    /// ba bon con vao mot goc, cho khac trong hoac ca mot vung rong. Con chia
    /// deu theo goc thi ra mot vong tron dieu dan, lo ngay ra la dat tay.
    /// Xoan oc vang cho cac diem trai deu ma van khong theo quy luat nhin thay
    /// duoc.
    /// </summary>
    void RaiQuaiKhapBanDo()
    {
        int tong = soPhuThuyRaiSan + soBoXuongRaiSan;
        if (tong <= 0) return;

        const float GocVang = 137.50776f;
        Vector3 choNguoiChoi = player != null ? player.position : arenaCenter;

        int daPhuThuy = 0, daBoXuong = 0;
        int daRai = 0;

        for (int i = 0; i < tong; i++)
        {
            // Xen ke hai loai. Xep lien khoi thi phu thuy don het vao giua con
            // bo xuong dat het ra ria, vi ban kinh tang dan theo so thu tu.
            MonsterType loai;
            if (daPhuThuy < soPhuThuyRaiSan && (daBoXuong >= soBoXuongRaiSan || i % 2 == 0))
            { loai = MonsterType.Witch; daPhuThuy++; }
            else
            { loai = MonsterType.Skeleton; daBoXuong++; }

            // CAN BAC HAI de mat do deu tren DIEN TICH. Khong lay can thi quai
            // don ca vao giua, vi vong ngoai co dien tich lon hon nhieu.
            float t = (i + 0.5f) / tong;
            float ban = Mathf.Lerp(raiSanCachXaNhat, arenaRadius * 0.92f, Mathf.Sqrt(t));
            float goc = i * GocVang;

            Vector3 huong = Quaternion.Euler(0f, goc, 0f) * Vector3.forward;
            Vector3 diem = arenaCenter + huong * ban;

            if (DatQuaiTaiDo(loai, diem, choNguoiChoi)) daRai++;
        }

        Debug.Log("[GameDirector] Da rai " + daRai + "/" + tong + " quai khap ban do ("
                + soPhuThuyRaiSan + " phu thuy, " + soBoXuongRaiSan + " bo xuong)");
    }

    /// <summary>
    /// RAI MOT DONG QUAI RIENG KHAP BAN DO luc vao man.
    ///
    /// Rai khap chu khong tha quanh nguoi choi: bon nay ban tu xa nen tha sat
    /// nguoi choi la vua vao game da an ngay mot loat.
    ///
    /// Dung XOAN OC VANG nhu <see cref="RaiQuaiKhapBanDo"/>. <paramref name="lechGoc"/>
    /// day ca xoan oc di mot phan goc vang, de moi dong quai chiem mot bo cho
    /// khac nhau chu khong don ca vao cung nhung diem do.
    /// </summary>
    void RaiDongRieng(MonsterType loai, int soCon, List<Damageable> dich,
                      float lechGoc, string ten)
    {
        if (soCon <= 0) return;

        const float GocVang = 137.50776f;
        Vector3 choNguoiChoi = player != null ? player.position : arenaCenter;
        int daRai = 0;

        for (int i = 0; i < soCon; i++)
        {
            float t = (i + 0.5f) / soCon;
            float ban = Mathf.Lerp(raiSanCachXaNhat, arenaRadius * 0.92f, Mathf.Sqrt(t));
            float goc = i * GocVang + GocVang * lechGoc;

            Vector3 huong = Quaternion.Euler(0f, goc, 0f) * Vector3.forward;
            Vector3 diem = arenaCenter + huong * ban;

            if (DatQuaiTaiDo(loai, diem, choNguoiChoi, dich)) daRai++;
        }

        Debug.Log("[GameDirector] Da rai " + daRai + "/" + soCon + " " + ten + " khap ban do.");
    }

    /// <summary>
    /// Dem gio cho mot dong quai rieng; den han thi tha mot dot tang vien quanh
    /// nguoi choi.
    ///
    /// Co TRAN so con song cung luc. Khong co tran thi sau nua tieng la ca tram
    /// con - may khong keo noi ma nguoi choi cung khong con cho ma dung.
    /// </summary>
    void DemGioDongRieng(ref float dongHo, float chuKy, int soMoiDot, int toiDa,
                         MonsterType loai, List<Damageable> dich, string ten)
    {
        if (soMoiDot <= 0 || chuKy <= 0f) return;

        dongHo -= Time.deltaTime;
        if (dongHo > 0f) return;
        dongHo = chuKy;

        int them = soMoiDot;
        if (toiDa > 0)
        {
            them = Mathf.Min(them, toiDa - dich.Count);
            if (them <= 0)
            {
                Debug.Log("[GameDirector] Da du " + dich.Count + " " + ten
                        + " (tran " + toiDa + ") - bo qua dot nay.");
                return;
            }
        }

        for (int i = 0; i < them; i++) SpawnOne(loai, dich);
        Debug.Log("[GameDirector] Them " + them + " " + ten + ", dang co " + dich.Count + " con.");
    }

    /// <summary>
    /// Dat mot con quai tai diem cho truoc. Vuong vat can thi tim quanh do.
    /// Tra ve true neu dat duoc.
    /// </summary>
    bool DatQuaiTaiDo(MonsterType loai, Vector3 diem, Vector3 choNguoiChoi)
    {
        return DatQuaiTaiDo(loai, diem, choNguoiChoi, alive);
    }

    bool DatQuaiTaiDo(MonsterType loai, Vector3 diem, Vector3 choNguoiChoi,
                      List<Damageable> dich)
    {
        for (int lan = 0; lan < 10; lan++)
        {
            Vector3 pos = diem;

            // Lan dau thu dung diem tinh duoc; cac lan sau ne sang ben mot chut
            if (lan > 0)
            {
                float g = Random.Range(0f, 360f);
                pos += Quaternion.Euler(0f, g, 0f) * Vector3.forward * (1.2f * lan);
            }

            // Giu trong long dau truong
            Vector3 tuTam = pos - arenaCenter;
            if (tuTam.magnitude > arenaRadius)
                pos = arenaCenter + tuTam.normalized * (arenaRadius * 0.92f);

            // Khong tha ngay canh nguoi choi
            if (Vector3.Distance(pos, choNguoiChoi) < raiSanCachXaNhat) continue;

            pos.y = VfxFactory.GroundY(pos) + 0.15f;

            if (Physics.CheckSphere(pos + Vector3.up * 1f, 0.6f,
                                    LayerMask.GetMask("Default", "Enemy"),
                                    QueryTriggerInteraction.Ignore))
                continue;

            var go = EnemyFactory.Spawn(loai, pos, enemyRoot, player);
            DanhSo(go, loai);
            var d = go.GetComponent<Damageable>();
            if (d != null)
            {
                d.onDeath += OnEnemyDeath;
                dich.Add(d);
            }
            return true;
        }
        return false;
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
        quyDu.RemoveAll(d => d == null || d.IsDead);
        quyCay.RemoveAll(d => d == null || d.IsDead);

        // Nhip sinh quai chi chay o may lam trong tai. May khach ma cung dem
        // gio thi no se tu de ra mot dot quai rieng khong ai khac nhin thay.
        if (LaTrongTaiCuaQuai)
        {
            // Hai dong quai rieng tu sinh them theo DONG HO RIENG cua chung,
            // khong doi dot cu chet het
            DemGioDongRieng(ref quyDuTimer, chuKyThemQuyDu, soQuyDuMoiDot, quyDuToiDa,
                            MonsterType.QuyDu, quyDu, "Quy du");
            DemGioDongRieng(ref quyCayTimer, chuKyThemQuyCay, soQuyCayMoiDot, quyCayToiDa,
                            MonsterType.QuyCay, quyCay, "Quy cay");

            if (waiting)
            {
                waveTimer -= Time.deltaTime;
                if (waveTimer <= 0f)
                {
                    waiting = false;
                    SpawnWave();
                }
            }
            else if (alive.Count == 0)
            {
                waiting = true;
                waveTimer = waveDelay;
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

    void SpawnWave()
    {
        Wave++;
        int count = startingCount + Mathf.RoundToInt((Wave - 1) * 1.8f);
        count = Mathf.Min(count, 22);

        for (int i = 0; i < count; i++)
            SpawnOne(PickType(i));

        // DOT DAU: them mot mu phu thuy CHAC CHAN co, ngoai so boc ngau nhien
        // o tren. Chi boc ngau nhien thi voi 5 quai va ti le 16%, co toi 42% so
        // lan choi khong gap mu nao ca - nguoi choi vao tran ba lan van chua
        // biet trong game co loai quai danh tu xa.
        if (Wave == 1) SpawnOne(MonsterType.Witch);
    }

    /// <summary>
    /// Loai quai cho mot khe trong dot.
    ///
    /// CHI CON HAI LOAI. Quy lun, Xac song va Quy khong lo da bi bo khoi game -
    /// khong con duong nao sinh ra chung nua. Hinh dang va prefab cua chung van
    /// con trong du an, chi la khong ai goi toi.
    ///
    /// ⚠️ Bo ba loai do lam MAT PHAN LEO THANG cua nhip dot: truoc day Xac song
    /// ra tu dot 2 va Quy khong lo tu dot 3, nen dot cang ve sau quai cang manh.
    /// Gio moi dot deu la mot ro phu thuy va bo xuong nhu nhau. Muon kho dan
    /// tro lai thi nang ti le phu thuy theo <see cref="Wave"/> o day.
    /// </summary>
    MonsterType PickType(int index)
    {
        float r = Random.value;

        // Phu thuy dung tu xa nem lua nen rat kho chiu neu ra qua dong: chi mot
        // it thoi. Ra ngay tu DOT DAU - dot dau co 5 quai nen trung binh 0.8 mu,
        // tuc khoang mot nua so lan choi la gap ngay tu dau.
        if (r < 0.16f) return MonsterType.Witch;

        return MonsterType.Skeleton;
    }

    void SpawnOne(MonsterType type)
    {
        SpawnOne(type, alive);
    }

    void SpawnOne(MonsterType type, List<Damageable> dich)
    {
        Vector3 center = player != null ? player.position : arenaCenter;

        for (int attempt = 0; attempt < 12; attempt++)
        {
            float ang = Random.Range(0f, 360f);
            float dist = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 pos = center + Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0f, dist);

            // Giu quai trong long dau truong
            Vector3 fromCenter = pos - arenaCenter;
            if (fromCenter.magnitude > arenaRadius)
                pos = arenaCenter + fromCenter.normalized * (arenaRadius * 0.92f);

            pos.y = VfxFactory.GroundY(pos) + 0.15f;

            // Khong tha quai chong len vat can
            if (Physics.CheckSphere(pos + Vector3.up * 1f, 0.6f,
                                    LayerMask.GetMask("Default", "Enemy"), QueryTriggerInteraction.Ignore))
                continue;

            var go = EnemyFactory.Spawn(type, pos, enemyRoot, player);
            DanhSo(go, type);
            var d = go.GetComponent<Damageable>();
            if (d != null)
            {
                d.onDeath += OnEnemyDeath;
                dich.Add(d);
            }
            return;
        }
    }

    /// <summary>
    /// Gan so hieu cho mot con quai vua sinh, de may kia goi dung ten no.
    ///
    /// Goi o MOI cho sinh quai, ke ca RaiQuaiKhapBanDo - bo sot mot cho thi
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
        alive.Remove(d);
        quyDu.Remove(d);
        quyCay.Remove(d);
    }

    void OnPlayerDeath(Damageable d)
    {
        PlayerDead = true;
    }
}
