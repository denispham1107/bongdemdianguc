using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LEU THAY MO XUONG.
///
/// Bay cay giao chum lai thanh hinh chop, dau nhon choc vuot len troi, tren mot
/// so dau giao cam mot cai so nguoi. Nua duoi phu gie rach tuop, nua tren de tro
/// khung xuong ra. Trong leu co mot dom lua ma xanh let hat sang qua vai.
///
/// DUNG LAI GAN NHU MOI THU DA CO:
///   - <see cref="GraveFactory.So"/> cho cai so, no san co ban le "HamPivot"
///     nen ha mieng la het duoc
///   - <see cref="GraveFactory.DoanXich"/> cho xich treo
///   - <see cref="GraveFactory.DomMa"/> cho dom lua ma
///   - <see cref="XacLacLu"/> cho do treo lac lu
///
/// Rieng tam vai leu phai tu dung: no la mat trong suot hai chieu (nguoi choi
/// nhin tu ngoai vao va tu trong ra deu phai thay), ma khoi Taper san co chi co
/// mot mat.
/// </summary>
public static class LeuFactory
{
    // Kich thuoc mot can leu, met. Nhan vat cao khoang 1.8 m nen leu 2.4 m la
    // vua du chui vao - nho hon thi thanh cai leu do choi.
    const float BanKinhChan = 1.52f;   // ban kinh vong chan giao
    const float CaoChum     = 2.42f;   // do cao cho cac cay giao cham nhau
    const float VoiLen      = 0.86f;   // doan giao choc vuot qua cho chum
    const int   SoCoc       = 7;

    static Material mVaiLeu, mSat, mXuongBan;

    /// <summary>
    /// Xuong am muc lau ngay ngoai troi: ngan, o vang, dinh dat.
    ///
    /// KHONG dung <c>Mats.BoneMat</c> cho khung leu: no la xuong sach trang
    /// hop, bay cay dung canh nhau se sang loa nhu que nhua, keo het mat nguoi
    /// choi ve phia no thay vi ve may cai so.
    /// </summary>
    public static Material XuongBan
    {
        get
        {
            if (mXuongBan == null)
                mXuongBan = Mats.Solid("XuongBan", new Color(0.58f, 0.53f, 0.44f), 0.05f, 0f,
                                       TextureFactory.Bone(), 1.3f);
            return mXuongBan;
        }
    }

    /// <summary>
    /// Vai leu. KHONG dung thang <see cref="GraveFactory.GieRach"/> vi mau no
    /// (0.34, 0.31, 0.27) qua toi: dem chi co anh trang xanh chieu vao, tam vai
    /// se den kit thanh mot mang, nhin ra khoi hinh chop chu khong ra tam vai.
    /// Sang hon mot bac va co van vai thi moi doc duoc.
    /// </summary>
    public static Material VaiLeu
    {
        get
        {
            if (mVaiLeu == null)
                mVaiLeu = Mats.Solid("VaiLeu", new Color(0.50f, 0.44f, 0.36f), 0.03f, 0f,
                                     TextureFactory.Cloth(new Color(0.82f, 0.76f, 0.66f),
                                                          new Color(0.55f, 0.49f, 0.41f), 0.55f), 2.2f);
            return mVaiLeu;
        }
    }

    /// <summary>Sat ri set cho luoi giao va xich.</summary>
    public static Material Sat
    {
        get
        {
            if (mSat == null)
                mSat = Mats.Solid("SatRi", new Color(0.52f, 0.46f, 0.40f), 0.22f, 0.35f,
                                  TextureFactory.Metal(new Color(0.60f, 0.56f, 0.52f), 0.72f), 1.4f);
            return mSat;
        }
    }

    // ================================================================
    //  DUNG MOT CAN LEU
    // ================================================================

    /// <summary>
    /// Dung mot can leu thay mo. Cung <paramref name="seed"/> thi luon ra cung
    /// mot can, nen bake lai bao nhieu lan ban do van y het.
    /// </summary>
    public static GameObject Build(Transform parent, int seed)
    {
        var cu = Random.state;
        Random.InitState(seed * 6151 + 17);

        var leu = new GameObject("Leu_ThayMo");
        if (parent != null) leu.transform.SetParent(parent, false);

        // Cua leu quay ve mot huong bat ky. Cai khe ho o vong vai chinh la cua.
        int cocCua = Random.Range(0, SoCoc);

        var chanCoc = new Vector3[SoCoc];
        var ngonCoc = new Vector3[SoCoc];
        TinhKhungCoc(chanCoc, ngonCoc);

        DungKhungCoc(leu.transform, chanCoc, ngonCoc, cocCua);
        DungVachVai(leu.transform, chanCoc, ngonCoc, cocCua);
        QuanDayNep(leu.transform, chanCoc, ngonCoc);
        TreoDoLungLang(leu.transform, chanCoc, ngonCoc, cocCua);

        // Dom lua ma trong leu, dat lech ve phia sau cho anh sang hat ra cua
        GraveFactory.DomMa(leu.transform, new Vector3(0f, 0.52f, -0.26f), 1.25f);

        // Da vun va dat bi giam nat quanh chan leu
        GraveFactory.DaVun(leu.transform, Random.Range(6, 10), BanKinhChan * 1.35f, seed + 5);

        Random.state = cu;
        return leu;
    }

    // ----------------------------------------------------------------
    //  KHUNG COC
    // ----------------------------------------------------------------

    /// <summary>
    /// Tinh chan va ngon cua tung cay giao.
    ///
    /// Chan toa deu tren mot vong tron nhung moi cay xe dich mot chut cho khoi
    /// deu tap tap nhu hang rao. Ngon KHONG chum dung mot diem: neu chum dung
    /// diem thi bay cay giao dam vao nhau thanh mot cuc, nhin rat gia. Cho moi
    /// ngon lech ra khoi tam mot ti, thanh ra chung bat cheo nhau - dung kieu
    /// nguoi ta dung leu that.
    /// </summary>
    static void TinhKhungCoc(Vector3[] chan, Vector3[] ngon)
    {
        float gocDau = Random.Range(0f, 360f);

        for (int i = 0; i < SoCoc; i++)
        {
            float goc = gocDau + i * (360f / SoCoc) + Random.Range(-7f, 7f);
            float r = BanKinhChan * Random.Range(0.92f, 1.08f);
            Vector3 huong = Quaternion.Euler(0f, goc, 0f) * Vector3.forward;

            chan[i] = huong * r;
            chan[i].y = 0f;

            // Cho chum: lech khoi truc mot doan nho, va cao thap khac nhau
            // Xoe rong ra: cac ngon chum tit vao nhau thi may cai so cam tren
            // dau giao se de len nhau thanh mot cuc trang, khong doc ra la so.
            Vector2 lech = Random.insideUnitCircle * 0.30f;
            // Moi coc tua vao bo o mot DO CAO KHAC NHAU. Tat ca cung chum vao
            // dung mot diem thi bay cai ngon deu choi ra tu cung mot cho, may
            // cai so cam tren do se de chong len nhau thanh mot cuc trang.
            float cao = CaoChum * Random.Range(0.78f, 1.10f);
            Vector3 choChum = new Vector3(lech.x, cao, lech.y);

            // Ngon giao choc vuot qua cho chum, tiep tuc theo dung huong cu
            Vector3 doc = (choChum - chan[i]).normalized;
            ngon[i] = choChum + doc * VoiLen * Random.Range(0.55f, 1.60f);
        }
    }

    /// <summary>Dung bay cay giao, mot so dau giao cam so nguoi.</summary>
    static void DungKhungCoc(Transform cha, Vector3[] chan, Vector3[] ngon, int cocCua)
    {
        // Chon truoc nhung cay nao duoc cam so. Bay cai so tren bay cay thi thanh
        // ra deu dan nhu hang rao trang tri; bon cai la du day ma van con nhip.
        // Chi cam so len nhung ngon CACH NHAU DU XA. Cam bua thi hai cai so o
        // hai ngon ke nhau se long vao nhau, nhin ra mot khoi trang meo mo chu
        // khong ra hai cai dau lau.
        const float CachSo = 0.52f;

        var coSo = new bool[SoCoc];
        var tamSo = new List<Vector3>();
        int muon = Random.Range(4, 6);

        for (int d = 0; d < 60 && muon > 0; d++)
        {
            int k = Random.Range(0, SoCoc);
            if (coSo[k]) continue;

            bool dungCanh = false;
            for (int q = 0; q < tamSo.Count; q++)
                if (Vector3.Distance(ngon[k], tamSo[q]) < CachSo) { dungCanh = true; break; }
            if (dungCanh) continue;

            coSo[k] = true;
            tamSo.Add(ngon[k]);
            muon--;
        }

        for (int i = 0; i < SoCoc; i++)
        {
            Vector3 doc = ngon[i] - chan[i];
            float dai = doc.magnitude;
            Quaternion xoay = Quaternion.FromToRotation(Vector3.up, doc.normalized);

            // Cay chan de xuong nguoi (xuong dui), cay le lam can giao go
            bool laXuong = i % 2 == 0;

            if (laXuong)
            {
                // Xuong dui: chi phinh o CHAN, con NGON phai thon nhon de con
                // cam cai so vao. Dung GraveFactory.Xuong thi no dap mot cuc u
                // tron o ca hai dau, ngon giao hoa ra cai keo mut.
                var than = ProcMesh.Taper(0.056f, 0.032f, dai, 7, false, Color.white);
                ProcMesh.Part("CocXuong" + i, cha, than, XuongBan,
                              chan[i], xoay, Vector3.one, true);

                var cucChan = ProcMesh.Sphere(0.075f, 8, 6, 0.82f, Color.white);
                ProcMesh.Part("CocXuong" + i + "_goc", cha, cucChan, XuongBan,
                              chan[i], xoay, Vector3.one, false);
            }
            else
            {
                var can = ProcMesh.Taper(0.052f, 0.030f, dai, 6, false, Color.white);
                ProcMesh.Part("CocGiao" + i, cha, can, GraveFactory.GoMuc,
                              chan[i], xoay, Vector3.one, true);

                // Luoi giao ngay duoi cho cam so
                var luoi = ProcMesh.Crystal(0.062f, 0.30f, 4, 0.16f, Color.white);
                ProcMesh.Part("LuoiGiao" + i, cha, luoi, Sat,
                              chan[i] + doc.normalized * (dai - 0.30f), xoay, Vector3.one, true);
            }

            if (coSo[i])
            {
                // So cam XIEN qua dau coc: hoi nga ve truoc va nghieng di, chu
                // dat thang thom lom thi ra cai so dat len chu khong ra cai so
                // bi dam xuyen.
                // Quay mat so ra ngoai theo huong cua CHINH CAI NGON, khong phai
                // huong cua goc coc. Cac cay giao bat cheo nhau nen ngon nam o
                // phia DOI DIEN voi goc: lay huong goc thi cai so quay mat vao
                // trong leu, nguoi choi dung ngoai chi thay cai gao dua trang.
                Vector3 raNgoai = new Vector3(ngon[i].x, 0f, ngon[i].z);
                if (raNgoai.sqrMagnitude < 0.0004f) raNgoai = Vector3.forward;

                Quaternion huongSo = Quaternion.LookRotation(raNgoai.normalized, Vector3.up)
                    * Quaternion.Euler(Random.Range(14f, 34f), Random.Range(-22f, 22f), Random.Range(-16f, 16f));

                // So TO han len. Truoc de 0.135 m: tu goc camera Diablo cach
                // 12 m thi cai so chi con vai diem anh, nhin ra cai nut chu
                // khong ra cai dau lau.
                var so = GraveFactory.So(cha, "SoCoc" + i,
                                         ngon[i] - doc.normalized * 0.16f, huongSo, 0.195f);

                // Cai so tren dau giao cung phai song: lac nhe va thinh thoang
                // ha ham la het, giong het may bo xuong treo tren cay.
                GraveFactory.GanCuDong(so, so.transform, 2.6f, 4.5f, 0.24f,
                                       new Transform[0], so.transform);
            }
        }
    }

    // ----------------------------------------------------------------
    //  VACH VAI
    // ----------------------------------------------------------------

    /// <summary>
    /// Cang gie rach quanh nua duoi khung, chua lai mot khe lam cua.
    ///
    /// Moi tam vai la mot dai noi hai cay giao ke nhau, chia doc thanh nhieu doi
    /// de mep tren rach nham nhu bi xe. Vai phai co CA HAI MAT: nguoi choi di
    /// vong ra sau leu, hoac ghe mat vao cua, deu phai thay tam vai chu khong
    /// phai nhin xuyen qua.
    /// </summary>
    static void DungVachVai(Transform cha, Vector3[] chan, Vector3[] ngon, int cocCua)
    {
        var b = new ProcMesh.Builder();

        for (int i = 0; i < SoCoc; i++)
        {
            if (i == cocCua) continue;                 // chua khe nay lam cua

            int j = (i + 1) % SoCoc;
            CangMotTam(b, chan[i], ngon[i], chan[j], ngon[j]);
        }

        var m = b.Build("VachVaiLeu", false);
        m.RecalculateBounds();
        ProcMesh.Part("VachVai", cha, m, VaiLeu, Vector3.zero,
                      Quaternion.identity, Vector3.one, true);
    }

    /// <summary>Mot tam vai cang giua hai cay giao ke nhau.</summary>
    static void CangMotTam(ProcMesh.Builder b, Vector3 chanA, Vector3 ngonA,
                           Vector3 chanB, Vector3 ngonB)
    {
        const int SoDoi = 7;                           // so doi chia doc tam vai

        // Ti le do cao tren than coc ma mep vai leo toi. Vai chi phu nua duoi.
        float treGoc = Random.Range(0.42f, 0.56f);

        var duoi = new Vector3[SoDoi + 1];
        var tren = new Vector3[SoDoi + 1];

        for (int k = 0; k <= SoDoi; k++)
        {
            float t = k / (float)SoDoi;
            Vector3 c = Vector3.Lerp(chanA, chanB, t);
            Vector3 n = Vector3.Lerp(ngonA, ngonB, t);

            // Vai khong cang cung: hop giua hai coc no vong vao trong mot chut
            // Them mot nhip song nho doc theo tam, cho cho phong cho hop nhu vai
            // that bi gio thoi lau ngay - khong thi tam vai phang li nhu tam nhua.
            float vong = Mathf.Sin(t * Mathf.PI) * 0.085f
                       + Mathf.Sin(t * Mathf.PI * 3.4f + treGoc * 9f) * 0.032f;
            Vector3 vaoTrong = -new Vector3(c.x, 0f, c.z).normalized * vong;

            // Mep DUOI: phan lon cham dat, thinh thoang bi toc len ho ra mot khe
            // Nang it thoi: nang nhieu thi mep duoi thanh mot khia vuong ro net,
            // nhin ra tam bia cat bang keo chu khong ra vat vai muc.
            float dayLen = Random.value < 0.30f ? Random.Range(0.05f, 0.13f) : 0f;

            // Mep TREN: rach nham, cho cao cho thap
            float tre = Mathf.Clamp01(treGoc + Random.Range(-0.22f, 0.22f));

            duoi[k] = Vector3.Lerp(c, n, dayLen / Mathf.Max(0.01f, (n - c).magnitude)) + vaoTrong;
            duoi[k].y = Mathf.Max(duoi[k].y, dayLen);
            tren[k] = Vector3.Lerp(c, n, tre) + vaoTrong;
        }

        // Phap tuyen huong ra ngoai leu
        for (int k = 0; k < SoDoi; k++)
        {
            Vector3 canh1 = duoi[k + 1] - duoi[k];
            Vector3 canh2 = tren[k] - duoi[k];
            Vector3 phap = Vector3.Cross(canh2, canh1).normalized;
            if (Vector3.Dot(phap, new Vector3(duoi[k].x, 0f, duoi[k].z)) < 0f) phap = -phap;

            // Vai muc nat thung mot vai cho - nhin thau qua thay khung xuong
            // ben trong. Khong bao gio thung o doi dau va doi cuoi, vi hai doi
            // do ap vao coc, thung o do se ho ra mot khe doc suot rat lo.
            if (k > 0 && k < SoDoi - 1 && Random.value < 0.14f) continue;

            float u0 = k / (float)SoDoi, u1 = (k + 1) / (float)SoDoi;

            int a = b.AddVertex(duoi[k],     phap, new Vector2(u0, 0f), Color.white);
            int c2 = b.AddVertex(duoi[k + 1], phap, new Vector2(u1, 0f), Color.white);
            int d = b.AddVertex(tren[k + 1], phap, new Vector2(u1, 1f), Color.white);
            int e = b.AddVertex(tren[k],     phap, new Vector2(u0, 1f), Color.white);
            b.AddQuad(a, c2, d, e);

            // MAT TRONG: cung bon dinh do nhung lat nguoc lai, de nhin tu trong
            // leu ra van thay vai. Thieu mat nay thi dung o cua nhin vao se thay
            // xuyen thau ra ngoai dong co.
            int a2 = b.AddVertex(duoi[k],     -phap, new Vector2(u0, 0f), Color.white);
            int b2 = b.AddVertex(tren[k],     -phap, new Vector2(u0, 1f), Color.white);
            int c3 = b.AddVertex(tren[k + 1], -phap, new Vector2(u1, 1f), Color.white);
            int d2 = b.AddVertex(duoi[k + 1], -phap, new Vector2(u1, 0f), Color.white);
            b.AddQuad(a2, b2, c3, d2);
        }
    }

    // ----------------------------------------------------------------
    //  DAY NEP
    // ----------------------------------------------------------------

    /// <summary>Day thung quan quanh khung, mot vong o cho chum va mot vong thap.</summary>
    static void QuanDayNep(Transform cha, Vector3[] chan, Vector3[] ngon)
    {
        VeVongDay(cha, "DayNepTren", chan, ngon, 0.88f, 0.030f);
        VeVongDay(cha, "DayNepDuoi", chan, ngon, 0.58f, 0.024f);
    }

    /// <summary>Mot vong day noi qua tat ca cac coc o do cao <paramref name="tre"/>.</summary>
    static void VeVongDay(Transform cha, string ten, Vector3[] chan, Vector3[] ngon,
                          float tre, float day)
    {
        var diem = new Vector3[SoCoc];
        for (int i = 0; i < SoCoc; i++)
            diem[i] = Vector3.Lerp(chan[i], ngon[i], tre);

        var b = new ProcMesh.Builder();
        for (int i = 0; i < SoCoc; i++)
        {
            Vector3 a = diem[i], c = diem[(i + 1) % SoCoc];
            Vector3 doc = c - a;
            var khuc = ProcMesh.Taper(day, day, doc.magnitude, 5, false, Color.white);
            var trs = Matrix4x4.TRS(a, Quaternion.FromToRotation(Vector3.up, doc.normalized),
                                    Vector3.one);
            b.Append(khuc, trs, Color.white);
        }

        var m = b.Build(ten, false);
        m.RecalculateBounds();
        ProcMesh.Part(ten, cha, m, GraveFactory.GieRach, Vector3.zero,
                      Quaternion.identity, Vector3.one, false);
    }

    // ----------------------------------------------------------------
    //  DO TREO LUNG LANG
    // ----------------------------------------------------------------

    /// <summary>
    /// Treo bua chu quanh leu: xich, xuong vun, tam go muc, va mot cai so.
    ///
    /// Moi chum treo tren mot cai TRUC RIENG dat dung cho no mac vao coc, roi
    /// <see cref="XacLacLu"/> xoay cai truc do. Treo thang vao coc thi luc lac
    /// se keo ca cay giao lac theo, ca can leu rung rinh nhu bang giay.
    /// </summary>
    static void TreoDoLungLang(Transform cha, Vector3[] chan, Vector3[] ngon, int cocCua)
    {
        int soChum = Random.Range(3, 5);
        var daTreo = new List<int>();

        for (int n = 0; n < soChum; n++)
        {
            int i = -1;
            for (int d = 0; d < 30; d++)
            {
                int k = Random.Range(0, SoCoc);
                if (k == cocCua || daTreo.Contains(k)) continue;
                i = k; break;
            }
            if (i < 0) continue;
            daTreo.Add(i);

            // Moc treo nam o khuc tren cay giao, cho da tro khung khong con vai
            // Moc o khuc tren cay giao, va day han RA NGOAI mot chut: treo sat
            // coc thi chum bua bi chinh cai coc va tam vai che khuat, coi nhu
            // khong co.
            float tre = Random.Range(0.60f, 0.76f);
            Vector3 moc = Vector3.Lerp(chan[i], ngon[i], tre);
            moc += new Vector3(chan[i].x, 0f, chan[i].z).normalized * 0.11f;

            var truc = new GameObject("MocTreo" + i);
            truc.transform.SetParent(cha, false);
            truc.transform.localPosition = moc;

            var chum = new GameObject("Chum");
            chum.transform.SetParent(truc.transform, false);

            float daiXich = Random.Range(0.16f, 0.30f);
            GraveFactory.DoanXich(chum.transform, "Xich", Sat,
                                  new Vector3(0f, -daiXich * 0.5f, 0f),
                                  new Vector3(0f, daiXich, 0.030f), 3);

            float duoiXich = -daiXich;
            int kieu = Random.Range(0, 3);

            if (kieu == 0)
            {
                // Mot cai so nho lung lang - cai nay ha ham la het duoc
                var so = GraveFactory.So(chum.transform, "So",
                                         new Vector3(0f, duoiXich - 0.11f, 0f),
                                         Quaternion.Euler(Random.Range(6f, 20f),
                                                          Random.Range(0f, 360f), 0f), 0.098f);
                GraveFactory.GanCuDong(truc, chum.transform, 7.5f, 13f, 0.30f,
                                       new Transform[0], so.transform);
                continue;
            }

            if (kieu == 1)
            {
                // Chum xuong vun go vao nhau
                int soXuong = Random.Range(2, 4);
                for (int x = 0; x < soXuong; x++)
                    GraveFactory.Xuong(chum.transform, "XuongVun" + x,
                        new Vector3(Random.Range(-0.05f, 0.05f), duoiXich - 0.16f,
                                    Random.Range(-0.05f, 0.05f)),
                        Quaternion.Euler(Random.Range(-24f, 24f), Random.Range(0f, 360f),
                                         Random.Range(-24f, 24f)),
                        Random.Range(0.14f, 0.21f), 0.019f);
            }
            else
            {
                // Tam bua go muc buoc gie rach
                var tam = ProcMesh.Box(new Vector3(0.13f, 0.19f, 0.022f), Color.white);
                ProcMesh.Part("BuaGo", chum.transform, tam, GraveFactory.GoMuc,
                              new Vector3(0f, duoiXich - 0.11f, 0f),
                              Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-9f, 9f)),
                              Vector3.one, false);

                var gie = ProcMesh.Box(new Vector3(0.10f, 0.13f, 0.010f), Color.white);
                ProcMesh.Part("GieBuoc", chum.transform, gie, GraveFactory.GieRach,
                              new Vector3(Random.Range(-0.03f, 0.03f), duoiXich - 0.24f, 0.014f),
                              Quaternion.Euler(0f, 0f, Random.Range(-26f, 26f)),
                              Vector3.one, false);
            }

            GraveFactory.GanCuDong(truc, chum.transform, 8.5f, 15f, 0.33f,
                                   new Transform[0], null);
        }
    }
}
