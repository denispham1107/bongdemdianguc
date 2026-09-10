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

    class QuaiBenNay
    {
        public GameObject vat;
        public Damageable mau;
        public NoiSuy noiSuy = new NoiSuy();
        public float ngheLanCuoi;
    }

    readonly Dictionary<ushort, QuaiBenNay> theoId = new Dictionary<ushort, QuaiBenNay>();
    readonly GoiTin.MotQuai[] demGui = new GoiTin.MotQuai[256];
    readonly GoiTin.MotQuai[] demNhan = new GoiTin.MotQuai[GoiTin.SoQuaiMoiGoi];
    readonly List<ushort> canXoa = new List<ushort>();

    Transform gocQuai;
    float guiLanSau;

    // ---- So dem de chan doan ----
    public int SoQuaiDangGiu { get { return theoId.Count; } }
    public int SoGoiQuaiDaGui { get; private set; }
    public int SoGoiQuaiDaNhan { get; private set; }
    public int SoQuaiDaSinh { get; private set; }

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
            return;
        }

        VeDanQuai(dt);
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

    // ================================================================
    //  MAY KHACH: NGHE VA VE LAI
    // ================================================================

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

            c.vat.transform.position = c.noiSuy.ViTriHienThi;
            c.vat.transform.rotation = Quaternion.Euler(0f, c.noiSuy.GocHienThi, 0f);
        }
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
