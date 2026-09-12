using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MOT CAI CAY BI THIEN THACH DOT CHAY.
///
/// Thien thach roi trung thi cay bat lua o goc, roi lua LAN DAN ra khap cay -
/// than, canh lon, canh nho, chum la - cho toi khi khong con bo phan nao chua
/// chay. Khoi den boc cuon, tung mang cay chay rot xuong dam lua duoi goc, than
/// cay den dan lai. Lan het roi thi ngon lua moi tat dan, cay bien mat, va
/// <see cref="GiayHoiSinh"/> giay sau no moc lai o dung cho cu.
///
/// ===================================================================
///  LUA BAM VAO BE MAT THAT, KHONG PHAI MOT KHOI HINH HOC
/// ===================================================================
///
/// Ban dau lua la mot cai hop quanh than cong mot qua cau o tan, ban kinh bi
/// chan o 5,5 m. Ma tan cay Act2 toe rong toi 23,6 m - nen phan lon canh nam
/// NGOAI lua, va nhin ra "co dam lua o cho cai cay" chu khong ra "cai cay dang
/// chay".
///
/// Nay moi cai cay co mot bo <b>diem moi lua</b> rai deu tren chinh be mat luoi
/// cua no. Lua an theo khoang cach: nhung diem cach goc duoi <c>nguongLan</c>
/// met la dang chay, va <c>nguongLan</c> lon dan theo thoi gian. Toi khi no
/// vuot qua diem xa nhat thi ca cai cay dang chay - luc do moi bat dau dem
/// <see cref="GiuSauKhiLanHet"/> giay roi tat dan.
///
/// ===================================================================
///  VI SAO KHONG XOA CAY ROI TAO LAI
/// ===================================================================
///
/// Cung mot ly do voi <see cref="VatTheBiCuon"/>: cay Act2 nam san trong canh,
/// khong co prefab de tao lai; ma cai dong ho 30 giay thi phai chay o dau do,
/// va <c>SetActive(false)</c> thi <c>Update</c> cua chinh no cung tat theo.
///
/// Nen "bien mat" o day la TAT Renderer va Collider - GameObject van song va
/// van dem gio. Het gio thi bat lai.
///
/// ===================================================================
///  CHAY DEN BANG MATERIALPROPERTYBLOCK, KHONG DOI VAT LIEU
/// ===================================================================
///
/// Ca 58 cay Act2 dung CHUNG mot vat lieu <c>Act2_VoCay_SanSui</c>. To den vat
/// lieu do la ca rung den theo, khong phai mot cai cay. Con tao ban sao vat
/// lieu cho tung cay thi moi lan chay lai sinh them mot dong vat lieu rac.
///
/// MaterialPropertyBlock doi mau cho RIENG mot renderer ma khong dung toi vat
/// lieu goc, va go ra chi bang <c>SetPropertyBlock(null)</c> - dung cai can khi
/// cay moc lai.
/// </summary>
public class CayChay : MonoBehaviour
{
    /// <summary>Lua an lan ra bao nhieu met moi giay.</summary>
    public const float TocDoLan = 2.6f;

    /// <summary>Lan het roi thi con chay bung them bao lau truoc khi tat dan.</summary>
    public const float GiuSauKhiLanHet = 2.2f;

    /// <summary>Chan tren va chan duoi cho thoi gian lan, giay.</summary>
    public const float LanNhanhNhat = 2.5f;
    public const float LanChamNhat = 9f;

    /// <summary>Bao lau sau khi chay rui thi cay moc lai o cho cu.</summary>
    public const float GiayHoiSinh = 30f;

    /// <summary>
    /// Nhieu nhat bao nhieu cay chay CUNG LUC.
    ///
    /// Mot loat thien thach la ba qua no gan nhau; roi vao mot vat cay ram thi
    /// so cay dinh lua co the len ca chuc. Ma mot cai cay chay het co la hon mot
    /// nghin hat moi giay cong mot ngon den diem.
    ///
    /// Do trong Editor (menu 18c): sau cai cung luc lam khung hinh tut khoang
    /// mot nua. Ha xuong BON - trong luc choi that, ba qua da roi trong ban kinh
    /// 2,8 m thuong chi bat lua mot den ba cai cay, nen it khi cham toi tran.
    /// </summary>
    public const int ToiDaCungLuc = 4;

    /// <summary>Moi diem moi lua nha bao nhieu hat lua mot giay.</summary>
    public const float HatMoiDiem = 3.4f;

    /// <summary>
    /// NGAN SACH HAT CHUNG cho ca man, tinh theo "may cai cay dang chay het co".
    ///
    /// Mot cai cay chay het co la hon mot nghin hat moi giay. Sau cai cung luc
    /// thi thanh bay nghin hat trong canh va khung hinh tut hon mot nua (do:
    /// 14,9 -> 6,5 fps). Nen luong hat cua MOI cay bi chia lai khi co nhieu cay
    /// cung chay: mot cay thi day du, sau cay thi moi cai con hon bon phan muoi.
    ///
    /// Tong luong hat vi the gan nhu khong doi du chay mot hay sau cai cay -
    /// va do la thu quyet dinh may co keo noi hay khong.
    /// </summary>
    public const float NganSachCay = 2.5f;

    /// <summary>Moi diem moi lua nha bao nhieu hat khoi mot giay.</summary>
    public const float KhoiMoiDiem = 0.22f;

    /// <summary>Bao nhieu diem moi lua cho mot cai cay (Act1 - luoi doc thang duoc).</summary>
    public const int SoDiemAct1 = 340;

    static int dangChay;

    /// <summary>
    /// Bao nhieu ngon den cay chay dang bat. Xem ghi chu trong VfxFactory:
    /// den diem thoi gian thuc dat hon han so hat.
    /// </summary>
    static int denDangBat;
    public const int DenToiDa = 2;

    bool coDen;

    [Header("Sat thuong cua lua tren cay")]
    public float satThuongMoiGiay = 22f;
    public float nhipDot = 0.3f;
    public LayerMask damageMask;

    /// <summary>Nguoi da dot cai cay nay - quai chet vi cay chay thi tinh cong cho ho.</summary>
    public Damageable keDot;

    /// <summary>Ban kinh dam lua duoi goc - dung cho CA hinh ve lan sat thuong.</summary>
    public float banKinhLua = 2.4f;

    /// <summary>Ban kinh tan cay, chi dung cho anh sang va tan lua.</summary>
    public float banKinhCanh = 3.6f;

    /// <summary>Lan het ca cay mat bao lau - tinh tu cho xa nhat tren cay.</summary>
    public float thoiGianLan { get; private set; }

    /// <summary>Ca doan chay: lan het + giu them. Cay bien mat sau chung nay giay.</summary>
    public float thoiGianChay { get { return thoiGianLan + GiuSauKhiLanHet; } }

    /// <summary>Lua da lan toi dau, 0..1. Bang 1 la khong con bo phan nao chua chay.</summary>
    public float TienDoLan { get { return xaNhat <= 0f ? 1f : Mathf.Clamp01(nguongLan / xaNhat); } }

    /// <summary>So diem moi lua dang chay tren tong so.</summary>
    public int SoDiemDangChay { get { return soChay; } }
    public int SoDiem { get { return diem != null ? diem.Length : 0; } }

    float cao;
    Vector3 chanCay;

    float age, dotTimer, manhTimer, denTimer, dongHo = -1f;
    float nguongLan, xaNhat;
    int soChay;
    bool daRui;

    VfxFactory.BoLuaCay bo;

    /// <summary>Diem moi lua trong khong gian THE GIOI, sap theo khoang cach tu goc.</summary>
    Vector3[] diem;
    float[] xa;

    /// <summary>Phan hat le chua du mot hat - don lai cho khung sau.</summary>
    float duLua, duKhoi;

    readonly List<Renderer> hinh = new List<Renderer>();
    readonly List<Color> mauGoc = new List<Color>();
    readonly List<float> xaCuaHinh = new List<float>();
    readonly List<Collider> vaCham = new List<Collider>();
    static MaterialPropertyBlock mpb;

    /// <summary>Bo diem da lay mau, nho lai theo luoi - nhieu cay dung chung mot luoi.</summary>
    static readonly Dictionary<Mesh, Vector3[]> khoDiem = new Dictionary<Mesh, Vector3[]>();

    // ================================================================
    //  NHAN RA MOT CAI CAY
    // ================================================================

    /// <summary>
    /// Goc cua vat the nay co phai mot cai cay khong.
    ///
    /// BON kieu ten, va deu phai nhan ra duoc:
    ///
    /// <code>
    ///   Act2   TREE_oakA_bare_701, TREE_dead_733, TREE_thin_742   (dua tu Blender)
    ///   Act1   CayXanh_19, CayChet_11                             (ten trong CANH da bake)
    ///   Act1   Tree7, DeadTree12                                  (ten khi dung bang CODE)
    /// </code>
    ///
    /// Hai dong Act1 cuoi la CUNG mot thu o hai thoi diem khac nhau:
    /// <c>WorldFactory.BuildLeafyTree</c> dat ten "Tree7", con AssetBaker khi
    /// nuong canh vao Act1.unity thi doi thanh "CayXanh_19". Chi nho moi ten
    /// trong code thi Act1 khong co cai cay nao chay - da vap dung cai do: kich
    /// ban thu bao "khong tim thay cay nao hop le" trong khi canh co 22 cai.
    ///
    /// Phan duoi ten phai la SO, khong thi mot vat ten "Treasure" hay "TreeStump"
    /// cung lot vao.
    /// </summary>
    public static bool LaCay(GameObject go)
    {
        if (go == null) return false;
        string n = go.name;

        if (n.StartsWith("TREE_")) return true;                       // Act2

        if (n.StartsWith("CayXanh_")) return LaSo(n, 8);              // Act1 (canh da bake)
        if (n.StartsWith("CayChet_")) return LaSo(n, 8);              // Act1 (canh da bake)

        if (n.StartsWith("DeadTree")) return LaSo(n, 8);              // Act1 (dung bang code)
        if (n.StartsWith("Tree")) return LaSo(n, 4);                  // Act1 (dung bang code)
        return false;
    }

    static bool LaSo(string s, int tu)
    {
        if (s.Length <= tu) return false;
        for (int i = tu; i < s.Length; i++)
            if (s[i] < '0' || s[i] > '9') return false;
        return true;
    }

    /// <summary>
    /// Tu mot bo phan bat ky tim len GOC cua cai cay.
    ///
    /// Act1 dung cay thanh nhieu manh con ("Trunk", "Leaves", "Branch3"), va
    /// tia quet co the trung bat cu manh nao. Dot chay mot manh la chi chay cai
    /// canh do, con ca cai cay van dung nguyen - nen phai leo len goc.
    /// </summary>
    public static Transform GocCay(Transform t)
    {
        while (t != null)
        {
            if (LaCay(t.gameObject)) return t;
            t = t.parent;
        }
        return null;
    }

    // ================================================================
    //  DOT
    // ================================================================

    /// <summary>
    /// Dot moi cai cay co than nam trong ban kinh.
    ///
    /// Quet o lop Default - toan bo canh vat cua man nam o do. Quet bang
    /// collider chu khong duyet ca canh: collider cua cay la mot bao nang OM LAY
    /// THAN (ban kinh 0,33 .. 0,74 m), nen "trung cay" o day dung nghia la thien
    /// thach no gan CAI THAN, khong phai cham vao mep tan la ngoai 11 m.
    /// </summary>
    /// <returns>So cay vua bat lua.</returns>
    public static int DotCayQuanh(Vector3 tam, float banKinh, LayerMask damageMask,
                                  float satThuongMoiGiay, Damageable keDot = null)
    {
        var col = Physics.OverlapSphere(tam, banKinh, 1 << 0, QueryTriggerInteraction.Ignore);
        int n = 0;

        for (int i = 0; i < col.Length; i++)
        {
            var goc = GocCay(col[i].transform);
            if (goc == null) continue;
            if (Dot(goc.gameObject, damageMask, satThuongMoiGiay, keDot) != null) n++;
        }
        return n;
    }

    /// <summary>Cham lua vao mot cai cay. Tra ve null neu no khong chay duoc.</summary>
    public static CayChay Dot(GameObject cay, LayerMask damageMask, float satThuongMoiGiay,
                              Damageable keDot = null)
    {
        if (cay == null || !LaCay(cay)) return null;
        if (cay.GetComponent<CayChay>() != null) return null;          // dang chay roi
        if (cay.GetComponent<VatTheBiCuon>() != null) return null;     // dang bi loc cuon
        if (dangChay >= ToiDaCungLuc) return null;

        var c = cay.AddComponent<CayChay>();
        c.damageMask = damageMask;
        c.satThuongMoiGiay = satThuongMoiGiay;
        c.keDot = keDot;

        // Dem NGAY o day chu khong doi toi Start.
        //
        // Start chi chay o khung hinh SAU. Ma mot qua thien thach dot ca chum
        // cay trong CUNG mot khung hinh: neu doi Start moi dem thi luc kiem tra
        // tran, dangChay van bang 0 cho ca chum. Do that: cham lua vao 12 cay
        // thi ca 12 cai deu bat lua du tran la 6, va khung hinh tut tu 13,6
        // xuong 7,3.
        dangChay++;
        return c;
    }

    // ================================================================
    //  DIEM MOI LUA
    // ================================================================

    /// <summary>
    /// Gom diem moi lua cua ca cai cay, trong khong gian THE GIOI.
    ///
    /// Ba duong lay, theo thu tu:
    ///
    /// <code>
    ///   1. asset nuong san   Act2 - 46/58 cay nam trong map_luoi.fbx voi
    ///                        Read/Write TAT, luc chay khong doc luoi duoc.
    ///                        Xem Act2DiemLua (menu 20).
    ///   2. doc thang luoi    Act1 - luoi dung bang code luc chay nen doc duoc.
    ///   3. rai trong khoi bao   duong lui cuoi cung, khi khong co ca hai.
    /// </code>
    ///
    /// Duong 3 khong bao gio dep, nhung no bao dam mot cai cay la khong bao gio
    /// chay ma khong co ngon lua nao.
    /// </summary>
    void LayDiemMoi()
    {
        var ds = new List<Vector3>(SoDiemAct1 + 32);

        var mfs = GetComponentsInChildren<MeshFilter>(true);

        // Chia ngan sach diem theo SO DINH cua tung manh, khong chia deu.
        //
        // Chia deu thi cay Act1 hong: no co 88 khuc canh nho va DUY NHAT MOT
        // manh "Leaves" chua ca tan la. Moi manh 3 diem nhu nhau thi ca tan la
        // chi duoc 3 cho bat lua, con may chuc khuc canh con thi thua diem -
        // nhin ra cai cay chay o than ma tan la gan nhu khong bat lua.
        int tongDinh = 0;
        for (int i = 0; i < mfs.Length; i++)
            if (mfs[i].sharedMesh != null) tongDinh += mfs[i].sharedMesh.vertexCount;
        if (tongDinh <= 0) tongDinh = 1;

        for (int i = 0; i < mfs.Length; i++)
        {
            var m = mfs[i].sharedMesh;
            if (m == null) continue;
            var tr = mfs[i].transform;
            int nganSach = Mathf.Max(1, Mathf.RoundToInt(SoDiemAct1 * (float)m.vertexCount / tongDinh));

            // 1) Asset nuong san (tra theo TEN luoi)
            var bo = DiemLuaCay.Nap(m.name);
            if (bo != null && bo.diem != null && bo.diem.Length > 0)
            {
                for (int k = 0; k < bo.diem.Length; k++)
                    ds.Add(tr.TransformPoint(bo.diem[k]));
                continue;
            }

            // 2) Doc thang luoi (chi khi doc duoc)
            if (m.isReadable)
            {
                Vector3[] mau;
                if (!khoDiem.TryGetValue(m, out mau))
                {
                    mau = LayMauLuoi(m, nganSach);
                    khoDiem[m] = mau;
                }
                for (int k = 0; k < mau.Length; k++)
                    ds.Add(tr.TransformPoint(mau[k]));
            }
        }

        // 3) Duong lui: rai trong khoi bao
        if (ds.Count < 12)
        {
            var bao = KhoiBao();
            for (int i = 0; i < 120; i++)
                ds.Add(new Vector3(
                    Random.Range(bao.min.x, bao.max.x),
                    Random.Range(bao.min.y, bao.max.y),
                    Random.Range(bao.min.z, bao.max.z)));
        }

        // Sap theo khoang cach tu goc: lua an tu goc lan dan ra dau canh.
        //
        // Khoang cach THANG chu khong phai doc theo canh - do doc canh thi phai
        // dung mot do thi lien thong cua ca cai luoi 100 nghin dinh. Voi mot cai
        // cay thi hai cach gan nhu trung nhau: canh nao cung toe ra xa dan goc.
        diem = ds.ToArray();
        xa = new float[diem.Length];
        for (int i = 0; i < diem.Length; i++) xa[i] = Vector3.Distance(diem[i], chanCay);
        System.Array.Sort(xa, diem);

        xaNhat = xa.Length > 0 ? xa[xa.Length - 1] : 1f;
    }

    /// <summary>Lay mau deu tren be mat mot luoi doc duoc (Act1).</summary>
    static Vector3[] LayMauLuoi(Mesh m, int soDiem)
    {
        var dinh = m.vertices;
        if (dinh.Length == 0) return new Vector3[0];

        soDiem = Mathf.Clamp(soDiem, 1, dinh.Length);
        var ra = new Vector3[soDiem];
        // Buoc nhay theo ti le vang cho diem khong don ve mot goc luoi
        int buoc = Mathf.Max(1, dinh.Length / soDiem);
        for (int i = 0; i < soDiem; i++) ra[i] = dinh[(i * buoc) % dinh.Length];
        return ra;
    }

    Bounds KhoiBao()
    {
        var bao = new Bounds(transform.position, Vector3.one * 0.5f);
        bool co = false;
        for (int i = 0; i < hinh.Count; i++)
        {
            if (hinh[i] == null || hinh[i] is ParticleSystemRenderer) continue;
            if (!co) { bao = hinh[i].bounds; co = true; }
            else bao.Encapsulate(hinh[i].bounds);
        }
        return bao;
    }

    // ================================================================

    void Start()
    {
        GetComponentsInChildren(true, hinh);
        GetComponentsInChildren(true, vaCham);

        var bao = KhoiBao();
        cao = Mathf.Clamp(bao.size.y, 2.5f, 20f);
        chanCay = new Vector3(bao.center.x, bao.min.y, bao.center.z);

        // Hai ban kinh khac nhau, va phai khac nhau:
        //
        //   banKinhLua   dam lua duoi GOC - cung la vung gay sat thuong. Nho,
        //                vi ke dich phai co cho ne; va vi cai gi gay sat thuong
        //                thi nguoi choi phai nhin ra duoc ranh gioi cua no.
        //   banKinhCanh  be rong cua tan, chi dung cho anh sang va tan lua bay.
        float banKinhTan = Mathf.Max(bao.size.x, bao.size.z) * 0.5f;
        banKinhLua = Mathf.Clamp(banKinhTan * 0.45f, 1.6f, 3.6f);
        banKinhCanh = Mathf.Clamp(banKinhTan * 0.72f, 2.2f, 5.5f);

        LayDiemMoi();

        // Thoi gian lan tinh tu CHO XA NHAT tren chinh cai cay nay: cay cang to
        // thi lua cang lau moi an het. Co chan tren chan duoi de mot cai cay con
        // khong tat phut mot cai, va mot cay khong lo khong chay ca phut.
        thoiGianLan = Mathf.Clamp(xaNhat / TocDoLan, LanNhanhNhat, LanChamNhat);

        // Mau goc va do xa cua tung bo phan, de con den dan theo dung buoc lua lan
        mauGoc.Clear();
        xaCuaHinh.Clear();
        for (int i = 0; i < hinh.Count; i++)
        {
            var r = hinh[i];
            var m = r != null ? r.sharedMaterial : null;
            mauGoc.Add(m != null && m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white);
            xaCuaHinh.Add(r != null ? Vector3.Distance(r.bounds.center, chanCay) : 0f);
        }

        coDen = denDangBat < DenToiDa;
        if (coDen) denDangBat++;

        bo = VfxFactory.BuildCayChay(chanCay, cao, banKinhLua, banKinhCanh, thoiGianChay, coDen);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // ---- Da chay rui: chi con dem gio cho toi luc moc lai ----
        if (daRui)
        {
            dongHo -= dt;
            if (dongHo <= 0f) MocLai();
            return;
        }

        age += dt;

        // ---- Lua an lan ra ----
        nguongLan = xaNhat * Mathf.Clamp01(age / Mathf.Max(0.01f, thoiGianLan));
        soChay = DemDangChay();

        BanHatLua(dt);

        // ---- Sat thuong cho ke dich dung trong dam lua duoi goc ----
        //
        // KHONG lien quan gi toi vung lua cua thien thach: hai nguon dot rieng,
        // nen ke dich dung duoi goc cay chay an ca hai. Dung y do.
        dotTimer -= dt;
        if (dotTimer <= 0f)
        {
            dotTimer = nhipDot;

            // Hai giay cuoi lua yeu dan, giong vung lua - khong tat phut mot cai
            float con = thoiGianChay - age;
            float manh = con < 2f ? Mathf.Clamp01(con / 2f) : 1f;
            float moiNhip = satThuongMoiGiay * nhipDot * manh;
            if (moiNhip > 0.01f)
                CombatUtil.AreaDamage(chanCay + Vector3.up * 0.8f, banKinhLua, moiNhip,
                                      damageMask, DamageType.Fire, 1.6f, keDot);
        }

        // ---- Than cay den dan, theo dung buoc lua lan ----
        //
        // Doi mau theo NHIP chu khong moi khung hinh: mot cai cay Act1 co 88
        // renderer, sau cai chay cung luc la 528 lan SetPropertyBlock moi khung.
        // Ma than cay den di trong may giay - mat nguoi khong phan biet noi buoc
        // nhay 0,12 giay voi doi mau lien tuc.
        denTimer -= dt;
        if (denTimer <= 0f)
        {
            denTimer = 0.12f;
            ToDen();
        }

        // ---- Tung mang cay chay rot xuong ----
        manhTimer -= dt;
        if (manhTimer <= 0f)
        {
            manhTimer = Random.Range(0.42f, 0.78f);
            RotMotManh();
        }

        if (age >= thoiGianChay) ChayRui();
    }

    /// <summary>
    /// Bao nhieu diem dang chay: nhung diem cach goc khong qua <see cref="nguongLan"/>.
    ///
    /// Mang <see cref="xa"/> da sap tang dan nen chi can tim CUNG TRAI - khong
    /// phai duyet ca ba tram diem moi khung hinh.
    /// </summary>
    int DemDangChay()
    {
        if (xa == null || xa.Length == 0) return 0;
        int lo = 0, hi = xa.Length;
        while (lo < hi)
        {
            int giua = (lo + hi) / 2;
            if (xa[giua] <= nguongLan) lo = giua + 1; else hi = giua;
        }
        return lo;
    }

    /// <summary>
    /// Ban hat lua va hat khoi vao dung nhung cho dang chay tren cai cay.
    ///
    /// Mot phan ba so hat danh cho RIA LUA - vanh ngoai cua vung da chay. Do la
    /// cho lua dang an toi, phai sang hon va dong hon phan phia trong, khong thi
    /// khong nhin ra lua dang lan ma chi thay no dot nhien to ra.
    /// </summary>
    void BanHatLua(float dt)
    {
        if (soChay <= 0 || diem == null) return;
        if (age >= thoiGianChay) return;          // het gio thi ngung ban

        var ep = new ParticleSystem.EmitParams();
        ep.applyShapeToPosition = false;

        // Nhieu cay cung chay thi moi cai bot hat di - xem NganSachCay
        float chiaSe = Mathf.Clamp01(NganSachCay / Mathf.Max(1, dangChay));

        if (bo.luaCanh != null)
        {
            duLua += soChay * HatMoiDiem * chiaSe * dt;
            int n = Mathf.FloorToInt(duLua);
            duLua -= n;
            n = Mathf.Min(n, 150);                // tran cho mot khung hinh

            int riaTu = Mathf.Max(0, soChay - Mathf.Max(8, soChay / 4));
            for (int i = 0; i < n; i++)
            {
                int k = (i % 3 == 0) ? Random.Range(riaTu, soChay) : Random.Range(0, soChay);
                ep.position = diem[k];
                bo.luaCanh.Emit(ep, 1);
            }
        }

        if (bo.khoiCanh != null)
        {
            duKhoi += soChay * KhoiMoiDiem * chiaSe * dt;
            int n = Mathf.FloorToInt(duKhoi);
            duKhoi -= n;
            n = Mathf.Min(n, 16);

            for (int i = 0; i < n; i++)
            {
                ep.position = diem[Random.Range(0, soChay)];
                bo.khoiCanh.Emit(ep, 1);
            }
        }
    }

    /// <summary>
    /// To den tung bo phan theo dung buoc lua lan toi no.
    ///
    /// Bo phan nao lua chua toi thi con nguyen mau; toi roi thi den dan trong
    /// khoang mot met ke tu luc bat lua. Cay Act1 co 88 bo phan nen nhin ro
    /// tung canh den lai lan luot; cay Act2 chi co mot bo phan nen ca cay den
    /// theo tien do chung.
    /// </summary>
    void ToDen()
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();

        for (int i = 0; i < hinh.Count; i++)
        {
            var r = hinh[i];
            if (r == null || r is ParticleSystemRenderer) continue;
            if (r.sharedMaterial == null || !r.sharedMaterial.HasProperty("_Color")) continue;

            float k;
            if (hinh.Count > 1)
            {
                // Den theo cho lua da lan toi - "1,2 m ke tu luc bat lua"
                float sauKhiChay = nguongLan - xaCuaHinh[i];
                k = Mathf.Lerp(1f, 0.16f, Mathf.Clamp01(sauKhiChay / 1.2f));
            }
            else
            {
                // Chi mot bo phan: den theo tien do chung cua ca cai cay
                k = Mathf.Lerp(1f, 0.16f, Mathf.Clamp01(age / thoiGianChay * 1.25f));
            }

            Color c = mauGoc[i];
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", new Color(c.r * k, c.g * k, c.b * k, c.a));
            r.SetPropertyBlock(mpb);
        }
    }

    /// <summary>
    /// Mot mang cay chay bat ra va rot xuong dam lua.
    ///
    /// Bat ra tu mot cho DANG CHAY that, khong phai mot diem ngau nhien trong
    /// khoi bao: luc lua moi bat o goc thi manh rot xuong tu goc, lan len ngon
    /// roi mang moi rot tu tren ngon xuong.
    /// </summary>
    void RotMotManh()
    {
        Vector3 tu;
        if (diem != null && soChay > 0)
        {
            // Uu tien nua ngoai cua vung dang chay - do la cho lua manh nhat
            int tu0 = soChay / 2;
            tu = diem[Random.Range(tu0, soChay)];
        }
        else
        {
            float goc = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(0.2f, 1f) * banKinhCanh * 0.75f;
            tu = chanCay + new Vector3(Mathf.Cos(goc) * r,
                                       Random.Range(0.35f, 0.95f) * cao,
                                       Mathf.Sin(goc) * r);
        }

        VfxFactory.ManhCayRoi(tu, Random.Range(0.85f, 1.9f));
    }

    /// <summary>Lua tat, cay bien mat, bat dau dem gio moc lai.</summary>
    void ChayRui()
    {
        if (daRui) return;
        daRui = true;
        dongHo = GiayHoiSinh;
        dangChay = Mathf.Max(0, dangChay - 1);
        if (coDen) { coDen = false; denDangBat = Mathf.Max(0, denDangBat - 1); }

        // Mot dot tan lua va tro bay len dung cho cai cay vua do xuong
        VfxFactory.HitBurst(chanCay + Vector3.up * cao * 0.35f, new Color(0.35f, 0.22f, 0.14f));
        VfxFactory.HitBurst(chanCay + Vector3.up * 0.5f, new Color(0.55f, 0.30f, 0.12f));

        // Vai manh cuoi cung rot xuong theo - cai cay do sap chu khong bay hoi
        for (int i = 0; i < 3; i++) RotMotManh();

        for (int i = 0; i < hinh.Count; i++)
            if (hinh[i] != null && !(hinh[i] is ParticleSystemRenderer)) hinh[i].enabled = false;
        for (int i = 0; i < vaCham.Count; i++)
            if (vaCham[i] != null) vaCham[i].enabled = false;

        // Lua thi de no tu tan het theo TatDanVungLua, khong xoa ngay: dam lua
        // con lai chinh la cai cay vua chay rui con am duoi dat.
    }

    void MocLai()
    {
        for (int i = 0; i < hinh.Count; i++)
        {
            var r = hinh[i];
            if (r == null || r is ParticleSystemRenderer) continue;
            r.SetPropertyBlock(null);      // tra lai mau vo cay nguyen ban
            r.enabled = true;
        }
        for (int i = 0; i < vaCham.Count; i++)
            if (vaCham[i] != null) vaCham[i].enabled = true;

        VfxFactory.HitBurst(chanCay + Vector3.up * 0.6f, new Color(0.45f, 0.60f, 0.35f));
        Destroy(this);
    }

    void OnDestroy()
    {
        // Phong khi bi xoa giua chung (doi man, choi lai): tra lai hinh, va cham
        // va mau goc - khong de mot cai cay vo hinh dung chan duong mai mai.
        if (!daRui) dangChay = Mathf.Max(0, dangChay - 1);
        if (coDen) { coDen = false; denDangBat = Mathf.Max(0, denDangBat - 1); }

        for (int i = 0; i < hinh.Count; i++)
        {
            var r = hinh[i];
            if (r == null || r is ParticleSystemRenderer) continue;
            r.SetPropertyBlock(null);
            r.enabled = true;
        }
        for (int i = 0; i < vaCham.Count; i++)
            if (vaCham[i] != null) vaCham[i].enabled = true;

        if (bo.goc != null) Destroy(bo.goc);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.45f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(chanCay + Vector3.up * 0.8f, banKinhLua);
        Gizmos.color = new Color(1f, 0.75f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(chanCay, nguongLan);
    }
}
