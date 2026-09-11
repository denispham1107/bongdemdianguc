using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CA HAI NGUOI DANH CHUNG MOT DAN QUAI.
///
/// Truoc day moi may tu rai quai cua minh, nen hai nguoi dung canh nhau ma
/// danh hai dan hoan toan khac nhau - ca hai man hinh cung ghi "quai con lai
/// 33" mot cach doc lap, va giet mot con thi chi minh minh thay no nga.
///
/// QUY UOC: CHU PHONG LA TRONG TAI CUA QUAI.
///   - Chu phong: rai quai, chay AI, tinh mau - y het choi mot minh. Moi
///     <see cref="NhipGuiQuai"/> lan mot giay no ke lai ca dan cho may khach.
///   - May khach: KHONG rai, KHONG chay AI. No sinh quai theo nhung gi nghe
///     duoc, dat vi tri theo do, va bo qua moi con so mau tu tinh duoc.
///
/// Vi sao chu phong chu khong phai "moi may tu tinh": AI quai chon muc tieu,
/// doi huong, ra don theo dong ho rieng cua tung may - chi lech vai mili giay
/// la hai ben re hai huong khac nhau, va cang chay cang xa nhau.
///
/// SAT THUONG CUA MAY KHACH VAN VAO DUOC QUAI, va no di duong vong: goi ky
/// nang sang chu phong, ban sao cua nguoi khach ben do tung dung phep ay,
/// phep trung quai THAT tren may chu phong. Nen o day khong can gui rieng
/// mot goi "toi vua danh con quai so 7".
/// </summary>
public class DongBoQuai : MonoBehaviour
{
    /// <summary>
    /// Bao nhieu lan ke lai dan quai moi giay.
    ///
    /// Thua hon nguoi choi (60) rat nhieu, va co ly do: quai di cham, khong ai
    /// ngam ban chinh xac vao chan chung, va mot dan 33 con la 431 byte moi
    /// lan. Sau muoi lan mot giay la 4,3 KB/giay - con nguoi choi thi chi 3,2.
    /// Giua hai moc thi noi suy cho muot.
    /// </summary>
    public const int NhipGuiQuai = 10;

    /// <summary>Khong nghe thay mot con trong bao lau thi coi nhu no da bien.</summary>
    public const float GiayCoiLaBienMat = 5f;

    public DongBoTran dongBo;

    /// <summary>
    /// Bo dong bo quai dang chay, de <see cref="GameDirector.DanhSo"/> ghi ten
    /// nghe cho con quai NGAY LUC no sinh ra.
    ///
    /// Vi sao khong chi dua vao nhip quet moi nua giay: con quai sinh giua tran
    /// se im lang trong khoang do - no vung kiem ma may kia khong nghe thay gi.
    /// Do duoc: phep thu sinh mot con roi ep no ra don ngay, so goi gui di = 0.
    /// Nhip quet van giu lai lam luoi an toan cho nhung con sinh o duong khac.
    /// </summary>
    public static DongBoQuai Hien { get; private set; }

    void OnEnable()  { Hien = this; }
    void OnDisable() { if (Hien == this) Hien = null; }

    /// <summary>Ghi ten nghe mot con quai vua sinh. Goi nhieu lan cung khong sao.</summary>
    public void NgheConNay(GameObject conQuai)
    {
        if (conQuai == null) return;
        var ai = conQuai.GetComponent<EnemyAI>();
        if (ai == null || daNgheDon.Contains(ai)) return;

        daNgheDon.Add(ai);
        ai.DaRaDon += KhiQuaiRaDon;
    }

    /// <summary>Goi don quai duoc gui lai may lan - xem <see cref="DongBoTran.SoLanGuiLaiPhep"/>.</summary>
    public const int SoLanGuiLaiDon = 3;
    public const float CachNhauGuiLaiDon = 0.05f;

    class QuaiBenNay
    {
        public GameObject vat;
        public Damageable mau;
        public NoiSuy noiSuy = new NoiSuy();
        public float ngheLanCuoi;

        /// <summary>Cho ve o khung truoc - de tinh ra no dang di nhanh bao nhieu.</summary>
        public Vector3 choKhungTruoc;
        public bool daCoChoTruoc;

        /// <summary>So thu tu don gan nhat da dien - de bo ban sao lap lai.</summary>
        public int donDaDien = -1;
    }

    struct DonChoGui
    {
        public byte[] goi;
        public int conLai;
        public float guiLanSau;
    }

    readonly Dictionary<ushort, QuaiBenNay> theoId = new Dictionary<ushort, QuaiBenNay>();
    readonly GoiTin.MotQuai[] demGui = new GoiTin.MotQuai[256];
    readonly GoiTin.MotQuai[] demNhan = new GoiTin.MotQuai[GoiTin.SoQuaiMoiGoi];
    readonly List<ushort> canXoa = new List<ushort>();

    readonly List<DonChoGui> donChoGui = new List<DonChoGui>();
    readonly HashSet<EnemyAI> daNgheDon = new HashSet<EnemyAI>();

    Transform gocQuai;
    float guiLanSau;
    float ngheDonLanSau;
    int soDonDaKe;

    // ---- So dem de chan doan ----
    public int SoQuaiDangGiu { get { return theoId.Count; } }

    /// <summary>So ban sao con song - duong du phong cho HUD may khach khi chua
    /// nghe bang so nao tu chu phong.</summary>
    public int SoQuaiConSong
    {
        get
        {
            int n = 0;
            foreach (var cap in theoId)
                if (cap.Value.vat != null && (cap.Value.mau == null || !cap.Value.mau.IsDead)) n++;
            return n;
        }
    }
    public int SoGoiQuaiDaGui { get; private set; }
    public int SoGoiQuaiDaNhan { get; private set; }
    public int SoQuaiDaSinh { get; private set; }
    public int SoDonDaKe { get; private set; }
    public int SoDonDaDien { get; private set; }
    public int SoDonBoViTrung { get; private set; }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (GameDirector.LaTrongTaiCuaQuai)
        {
            if (Time.unscaledTime >= guiLanSau)
            {
                guiLanSau = Time.unscaledTime + 1f / NhipGuiQuai;
                KeLaiCaDan();
            }
            NgheQuaiRaDon();
            GuiLaiDonDangCho();
            GuiBangSo();
            return;
        }

        VeDanQuai(dt);

        // Chi go nhung con "lau khong duoc nhac den" khi DUONG TRUYEN CON TOT.
        //
        // Neu ca kenh im lang (chu phong chuyen tab, mat song, hay da roi tran)
        // thi MOI con deu "lau khong duoc nhac den" cung luc - va truoc day ca
        // dan quai bien mat sau 5 giay, khong mot loi giai thich. Im lang chung
        // khong noi len rang con quai nao da chet ca.
        if (dongBo == null || dongBo.TinhTrang == DongBoTran.TinhTrangKetNoi.Tot)
            DonNhungConDaBien();
    }

    // ================================================================
    //  CHU PHONG: KE LAI CA DAN
    // ================================================================

    void KeLaiCaDan()
    {
        if (dongBo == null || !KenhTrucTiep.DaMo) return;

        var ds = Object.FindObjectsByType<NhanDangQuai>(FindObjectsSortMode.None);
        int soCon = 0;

        for (int i = 0; i < ds.Length && soCon < demGui.Length; i++)
        {
            var n = ds[i];
            if (n == null) continue;

            var d = n.GetComponent<Damageable>();
            demGui[soCon++] = new GoiTin.MotQuai
            {
                id = n.id,
                loai = (byte)n.loai,
                viTri = n.transform.position,
                gocY = n.transform.eulerAngles.y,
                mau01 = d != null && d.maxHealth > 0f ? d.health / d.maxHealth : 1f,
                daChet = d != null && d.IsDead
            };
        }

        // Chia thanh nhieu goi nho. Mot goi to di qua duong truyen khong tin
        // cay thi mat ca goi, ma mat ca goi la ca dan quai dung hinh mot nhip.
        int moc = dongBo.GioTran();
        for (int tu = 0; tu < soCon; tu += GoiTin.SoQuaiMoiGoi)
        {
            int lay = Mathf.Min(GoiTin.SoQuaiMoiGoi, soCon - tu);
            byte[] b = GoiTin.VietQuai(moc, demGui, tu, lay);
            if (KenhTrucTiep.Gui(GoiTin.SangChuoi(b))) SoGoiQuaiDaGui++;
        }
    }

    /// <summary>
    /// Ghi ten nghe moi con quai moi sinh ra.
    ///
    /// Phai lam theo NHIP chu khong mot lan luc bat dau: quai sinh them suot
    /// tran (hai dong quai rieng co dong ho rieng), va con nao khong duoc ghi
    /// ten nghe thi don cua no khong bao gio sang toi may kia - nguoi khach
    /// dung do nhin mot con quai vung kiem trong im lang.
    /// </summary>
    void NgheQuaiRaDon()
    {
        if (Time.unscaledTime < ngheDonLanSau) return;
        ngheDonLanSau = Time.unscaledTime + 0.5f;

        daNgheDon.RemoveWhere(a => a == null);

        foreach (var ai in Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            if (ai == null || daNgheDon.Contains(ai)) continue;
            daNgheDon.Add(ai);
            ai.DaRaDon += KhiQuaiRaDon;
        }
    }

    void KhiQuaiRaDon(EnemyAI conQuai, int kieu, Transform mucTieu, Vector3 diemNgam)
    {
        if (dongBo == null || !KenhTrucTiep.DaMo || conQuai == null) return;

        // Khong co so hieu thi may kia khong biet cho don ay vao mieng ai
        var soHieu = conQuai.GetComponent<NhanDangQuai>();
        if (soHieu == null) return;

        var goi = GoiTin.VietDonQuai(new GoiTin.MotDonQuai
        {
            idQuai = soHieu.id,
            kieuDon = (byte)kieu,
            chiSoNanNhan = dongBo.ChiSoCua(mucTieu),
            soThuTu = ++soDonDaKe,
            diemNgam = diemNgam
        });

        SoDonDaKe++;
        if (KenhTrucTiep.Gui(GoiTin.SangChuoi(goi))) SoGoiQuaiDaGui++;
        donChoGui.Add(new DonChoGui
        {
            goi = goi, conLai = SoLanGuiLaiDon - 1,
            guiLanSau = Time.unscaledTime + CachNhauGuiLaiDon
        });
    }

    float guiBangSoLanSau;

    /// <summary>Bao nhieu lan gui bang so moi giay.</summary>
    public const float NhipGuiBangSo = 2f;

    /// <summary>
    /// Ke cho may khach nghe bang so cua tran. Hai lan moi giay la du: day la
    /// con so nguoi ta liec qua, khong phai thu nguoi ta ngam ban vao.
    /// </summary>
    void GuiBangSo()
    {
        if (dongBo == null || !KenhTrucTiep.DaMo) return;
        if (Time.unscaledTime < guiBangSoLanSau) return;
        guiBangSoLanSau = Time.unscaledTime + 1f / NhipGuiBangSo;

        var dir = GameDirector.Instance;
        if (dir == null) return;

        byte[] b = GoiTin.VietBangSo(dir.Wave, dir.Kills, dir.Alive, dir.NextWaveIn);
        if (KenhTrucTiep.Gui(GoiTin.SangChuoi(b))) SoGoiQuaiDaGui++;
    }

    void GuiLaiDonDangCho()
    {
        for (int i = donChoGui.Count - 1; i >= 0; i--)
        {
            var d = donChoGui[i];
            if (Time.unscaledTime < d.guiLanSau) continue;

            if (KenhTrucTiep.Gui(GoiTin.SangChuoi(d.goi))) SoGoiQuaiDaGui++;
            d.conLai--;
            if (d.conLai <= 0) { donChoGui.RemoveAt(i); continue; }

            d.guiLanSau = Time.unscaledTime + CachNhauGuiLaiDon;
            donChoGui[i] = d;
        }
    }

    // ================================================================
    //  MAY KHACH: NGHE VA VE LAI
    // ================================================================

    /// <summary>
    /// Mot goi "quai ra don" vua den. <paramref name="chiSoCuaToi"/> de biet
    /// don nay co nham vao nhan vat cua may nay khong.
    /// </summary>
    public void NhanDonQuai(byte[] b, byte chiSoCuaToi)
    {
        GoiTin.MotDonQuai d;
        if (!GoiTin.DocDonQuai(b, out d)) return;

        QuaiBenNay c;
        if (!theoId.TryGetValue(d.idQuai, out c) || c.vat == null) return;

        // Goi duoc gui ba lan cho chac - hai lan sau la ban sao. Khong bo thi
        // mot cu vung kiem an mau ba lan.
        if (d.soThuTu <= c.donDaDien) { SoDonBoViTrung++; return; }
        c.donDaDien = d.soThuTu;

        var ai = c.vat.GetComponent<EnemyAI>();
        if (ai == null) return;

        // Chi nhan vat CUA MAY NAY moi that su mat mau: mau cua nguoi kia do
        // may cua ho quyet dinh, ghi vao day cung bi goi tin cua ho de len.
        Damageable nanNhan = null;
        if (d.chiSoNanNhan == chiSoCuaToi && dongBo != null && dongBo.toi != null)
            nanNhan = dongBo.toi.GetComponent<Damageable>();

        SoDonDaDien++;
        ai.DienLaiDon(d.kieuDon, d.diemNgam, nanNhan);
    }

    /// <summary>Chi kich ban chay thu dung: nhet san mot con quai vao so tay.</summary>
    public void GhiDeQuaiChoPhepThu(ushort id, GameObject vat)
    {
        theoId[id] = new QuaiBenNay
        {
            vat = vat,
            mau = vat != null ? vat.GetComponent<Damageable>() : null,
            ngheLanCuoi = Time.unscaledTime
        };
    }

    /// <summary>Mot goi quai vua den. <see cref="DongBoTran"/> chuyen sang day.</summary>
    public void NhanGoiQuai(byte[] b)
    {
        int moc;
        int soCon = GoiTin.DocQuai(b, demNhan, out moc);
        if (soCon < 0) return;

        SoGoiQuaiDaNhan++;

        for (int i = 0; i < soCon; i++)
        {
            var q = demNhan[i];

            QuaiBenNay c;
            if (!theoId.TryGetValue(q.id, out c))
            {
                // Chu phong van ke ten con quai trong luc no nam guc cho bien
                // mat. Ben nay chua thay no bao gio (vua vao, hoac da go di) thi
                // dung dung lai mot cai xac chi de no chet them lan nua.
                if (q.daChet) continue;

                c = SinhTheoLoi(q);
                if (c == null) continue;
                theoId[q.id] = c;
            }

            c.ngheLanCuoi = Time.unscaledTime;
            c.noiSuy.Nhan(moc, q.viTri, q.gocY, true);

            if (c.mau != null && c.mau.maxHealth > 0f)
                c.mau.health = Mathf.Clamp(q.mau01 * c.mau.maxHealth, 0f, c.mau.maxHealth);

            if (q.daChet && c.mau != null && !c.mau.IsDead) c.mau.Die();
        }
    }

    /// <summary>
    /// Sinh mot con quai theo loi ke, roi TAT HET phan tu quyet cua no.
    ///
    /// Khong tat AI thi con quai ben nay se tu chon muc tieu, tu di, tu ra don
    /// - va no se troi khoi vi tri that ngay trong vai giay. Cai ta thay se la
    /// mot con quai khac han con ma nguoi kia dang danh.
    /// </summary>
    QuaiBenNay SinhTheoLoi(GoiTin.MotQuai q)
    {
        if (gocQuai == null)
        {
            var cu = GameObject.Find("Enemies");
            gocQuai = cu != null ? cu.transform : new GameObject("Enemies").transform;
        }

        var go = EnemyFactory.Spawn((MonsterType)q.loai, q.viTri, gocQuai, null);
        if (go == null) return null;

        var n = go.GetComponent<NhanDangQuai>();
        if (n == null) n = go.AddComponent<NhanDangQuai>();
        n.id = q.id;
        n.loai = (MonsterType)q.loai;

        foreach (var ai in go.GetComponentsInChildren<EnemyAI>(true)) ai.enabled = false;

        // Mau cua con nay do CHU PHONG quyet. Phep cua nguoi khach van trung no
        // va van hien don danh, nhung khong duoc giet no ngay tren may khach -
        // sat thuong that da di duong vong sang chu phong roi.
        var mauQuai = go.GetComponent<Damageable>();
        if (mauQuai != null) mauQuai.mauDoMayKhacQuyet = true;

        // Tat luon phan va cham voi dia hinh: vi tri den tu may kia, ben do da
        // tinh roi. De nguyen thi con quai bi day ra khoi cho hai lan.
        var cc = go.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        SoQuaiDaSinh++;
        return new QuaiBenNay
        {
            vat = go,
            mau = go.GetComponent<Damageable>(),
            ngheLanCuoi = Time.unscaledTime
        };
    }

    void VeDanQuai(float dt)
    {
        foreach (var cap in theoId)
        {
            var c = cap.Value;
            if (c.vat == null) continue;

            c.noiSuy.CapNhat(dt);
            if (!c.noiSuy.SanSang) continue;

            Vector3 choMoi = c.noiSuy.ViTriHienThi;

            // Y HET ban sao nguoi choi: con quai duoc dat thang vi tri nen
            // CharacterController cua no khong bao gio chuyen dong, ma
            // ModelHoatHinh lai doc toc do tu cc.velocity. Khong tinh lay thi
            // ca dan quai TRUOT tren mat dat nhu dang bay - dung nhu nguoi
            // dung mo ta khi choi may khach.
            float tocDo = 0f;
            if (c.daCoChoTruoc && dt > 0.0001f)
            {
                Vector3 di = choMoi - c.choKhungTruoc;
                di.y = 0f;
                tocDo = di.magnitude / dt;
            }
            c.choKhungTruoc = choMoi;
            c.daCoChoTruoc = true;

            c.vat.transform.position = choMoi;
            c.vat.transform.rotation = Quaternion.Euler(0f, c.noiSuy.GocHienThi, 0f);

            EpNhipBuoc(c.vat, tocDo);
        }
    }

    /// <summary>
    /// Bao cho bo hoat hinh biet con quai nay dang di nhanh bao nhieu.
    ///
    /// Hai kieu quai hai kieu hoat hinh: bon dung bang code chay
    /// <see cref="ProceduralAnimator"/>, con bon dung model san (phu thuy, quy
    /// du) chay <see cref="ModelHoatHinh"/>. Bo sot mot kieu thi mot nua dan
    /// quai van bay.
    /// </summary>
    static void EpNhipBuoc(GameObject vat, float tocDo)
    {
        var ai = vat.GetComponent<EnemyAI>();
        float toiDa = ai != null ? ai.moveSpeed : 2.6f;
        float tocDo01 = Mathf.Clamp01(tocDo / Mathf.Max(0.1f, toiDa));

        var anim = vat.GetComponentInChildren<ProceduralAnimator>();
        if (anim != null) anim.SetMoveSpeed(tocDo01);

        var model = vat.GetComponentInChildren<ModelHoatHinh>();
        if (model != null) model.tocDoEp = tocDo01;
    }

    /// <summary>
    /// Con nao lau khong duoc nhac den thi go di.
    ///
    /// Chu phong khong gui rieng mot loi "con so 7 da bien mat": no chi thoi
    /// nhac den con ay. Im lang la cach re nhat de noi mot con quai khong con
    /// nua, va no chiu duoc ca truong hop mat goi.
    /// </summary>
    void DonNhungConDaBien()
    {
        canXoa.Clear();
        float hetHan = Time.unscaledTime - GiayCoiLaBienMat;

        foreach (var cap in theoId)
            if (cap.Value.ngheLanCuoi < hetHan || cap.Value.vat == null)
                canXoa.Add(cap.Key);

        for (int i = 0; i < canXoa.Count; i++)
        {
            var c = theoId[canXoa[i]];
            if (c.vat != null) Destroy(c.vat);
            theoId.Remove(canXoa[i]);
        }
    }
}
