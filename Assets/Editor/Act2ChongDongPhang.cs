using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// TACH MOI BE MAT NAM NGANG RA KHOI MAT DAT, de het "o vuong den" tren ban do.
///
/// Trieu chung nguoi choi thay: nhung hinh vuong toi, hoi trong suot, nam la
/// liet duoi mat dat - "giong nhu dang ghep tung manh dat lai". Tren WebGL dien
/// thoai thi nhieu, tren may ban thi khong thay.
///
/// <b>Nguyen nhan: z-fighting.</b> Rat nhieu canh vat cua Act2 co mot mat NAM
/// NGANG gan nhu trung khit voi mat dat - de bia mo, phien da nam, tang da lun.
///
/// ===================================================================
///  NGUONG PHAI TINH RA, KHONG DUOC DOAN
/// ===================================================================
///
/// Bo dem chieu sau phoi canh co do chinh xac giam theo BINH PHUONG khoang cach:
///
///     khe toi thieu ~ z^2 / (near * 2^bit)
///
/// Voi <c>near = 0,5</c> va vat o cach may quay 15 m:
///
///     16 bit (WebGL dien thoai) -> 6,87 mm
///     24 bit (may ban)          -> 0,027 mm
///
/// Chenh nhau 256 lan. Do dung la ly do "dien thoai bi ma may ban khong" - va
/// cung la ly do vi sao dung <see cref="Camera.nearClipPlane"/> 1,5 thay vi 0,5
/// (xem <c>GameBootstrap</c>) lai an nua so cho hong ma khong dong den mo hinh
/// nao ca.
///
/// ===================================================================
///  MOT DIEM CHAM KHONG PHAI LA MOT MANG
/// ===================================================================
///
/// Ban dau toi dem "vat co diem nao do gan dat duoi 2 cm" va ra 247 vat. Con so
/// do vo nghia: mot phien da nam VAT QUA go dat thi no CAT mat dat theo mot
/// duong, va cho cat luon co diem khoang cach bang 0. Giao cat khong gay
/// z-fighting - hai mat cat nhau mot goc ro rang thi bo dem chieu sau phan biet
/// duoc thoai mai.
///
/// z-fighting chi xay ra khi hai mat GAN SONG SONG va gan nhau tren mot MANG
/// rong. Nen phep dem dung la: quet luoi 9x9, va chi tinh khi co <b>tu 3 diem
/// tro len</b> cung nam trong khe nguy hiem.
///
/// Doi tieu chi thi con so doi han:
/// <code>
///   khe &lt; 7 mm tren >=  3 diem :  66 vat   &lt;- can chua
///   khe &lt; 7 mm tren >=  8 diem :   7 vat
///   khe &lt; 7 mm tren >= 20 diem :   0 vat
/// </code>
///
/// ===================================================================
///  DUNG BAO GIO NANG DIA HINH LEN OM LAY VAT
/// ===================================================================
///
/// Ban truoc cua file nay co them mot buoc "nang mat dat om lay chan vat" cho
/// nhung cai dich the nao cung con dinh. Da bo. No <b>khong hoi tu</b> (chi
/// nang, khong bao gio ha, nen chay bao nhieu vong dat cung leo len dan) va cai
/// gia phai tra la <b>31 vat bi chon HAN</b> duoi dat, 70 cai chon qua 70%. Doi
/// mot loi nhap nhay lay mot loi mat han do dac thi khong phai la chua.
///
/// Dia hinh da duoc dung lai tu <c>Act2GoGhe.VeLai(false)</c> + <c>Act2LongVungNuoc.Khoet()</c>.
/// </summary>
public static class Act2ChongDongPhang
{
    /// <summary>
    /// Khe muc tieu sau khi dich.
    ///
    /// 25 mm chu khong phai 7 mm: 7 mm chi du cho vat o cach may quay 15 m. O
    /// 30 m - van con trong tam nhin truoc khi suong mu che - can 9,2 mm, o
    /// 40 m can 16,3 mm. Lay 25 mm thi phu het vung nhin thay, ma dich 2,5 cm
    /// thi mat khong nhan ra.
    /// </summary>
    public const float KheMucTieu = 0.025f;

    /// <summary>Khe coi la nguy hiem khi DEM. Dung 7 mm - nguong 16 bit o 15 m.</summary>
    public const float KheNguyHiem = 0.007f;

    /// <summary>Phai co tu chung nay diem tro len moi goi la MANG dong phang.</summary>
    public const int DiemToiThieu = 3;

    /// <summary>Luoi quet (N+1)x(N+1) diem tren hop bao.</summary>
    const int N = 8;

    /// <summary>Khong nang vat len qua chung nay - qua thi ho chan, thay ngay.</summary>
    const float NangToiDa = 0.15f;

    /// <summary>Khong dim vat qua ti le nay cua chieu cao - qua thi mat vat.</summary>
    const float DimToiDa = 0.45f;

    [MenuItem("Diablo 2.5D/14. Chong o vuong den (tach mat khoi dat) - Act2", false, 95)]
    public static void ChayChoAct2()
    {
        var sc = EditorSceneManager.GetActiveScene();
        if (sc.name != "Act2")
            sc = EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", OpenSceneMode.Single);

        int traLai = TraLaiCumBiTach();
        if (traLai > 0)
            Debug.Log(string.Format("[ChongDongPhang] da tra {0} bo phan ve dung cho trong cum", traLai));

        int truoc = Dem();

        // Vai vong: dich mot vat co the lam vat ben canh doi trang thai (chung
        // dung chung mot cho tren dia hinh), nen quet lai cho den khi yen.
        int daDich = 0;
        for (int i = 0; i < 6; i++)
        {
            int n = Sua();
            daDich += n;
            if (n == 0) break;
        }
        int sau = Dem();

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        Debug.Log(string.Format(
            "[ChongDongPhang] da dich {0} vat the. Vat co MANG dong phang: {1} -> {2}",
            daDich, truoc, sau));
    }

    /// <summary>
    /// Dem so vat the co mot MANG nam ngang trung khit voi mat dat.
    /// </summary>
    public static int Dem()
    {
        var terr = Object.FindFirstObjectByType<Terrain>();
        if (terr == null) return -1;
        float goc = terr.transform.position.y;

        int n = 0;
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (BoQua(r)) continue;
            if (DiemDongPhang(r, terr, goc, KheNguyHiem) >= DiemToiThieu) n++;
        }
        return n;
    }

    /// <summary>
    /// Dich moi vat the co mang dong phang ra khoi mat dat. Tra ve so vat da dich.
    /// </summary>
    public static int Sua()
    {
        var terr = Object.FindFirstObjectByType<Terrain>();
        if (terr == null) { Debug.LogError("[ChongDongPhang] khong tim thay Terrain"); return 0; }
        float goc = terr.transform.position.y;

        // Gom truoc roi moi dich: dich trong khi dang duyet thi bounds doi giua
        // chung va vong lap doc phai so cu, so moi lan lon.
        //
        // Gom theo GOC CUM chu khong theo tung renderer. Mot bui co la hai
        // renderer roi - "MatCo" nam bet duoi dat va "MatHoa" nho len tren - va
        // chi co MatCo bi bat la dong phang. Dich rieng no thi mat co truot ra
        // khoi bui co cua no. Cung mot cai bay cho cay o Act1, noi than va tan
        // la hai vat the ten "Trunk" va "Leaves".
        var dyTheoGoc = new Dictionary<Transform, float>();
        var diemTheoGoc = new Dictionary<Transform, int>();

        foreach (var r in Object.FindObjectsByType<MeshRenderer>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (BoQua(r)) continue;
            int diem = DiemDongPhang(r, terr, goc, KheNguyHiem);
            if (diem < DiemToiThieu) continue;

            var gocCum = GocCum(r.transform);

            // Hai bo phan cung mot cum doi hai huong khac nhau thi nghe cai co
            // MANG dong phang rong hon - no la cai nguoi choi nhin thay.
            int cu2;
            if (diemTheoGoc.TryGetValue(gocCum, out cu2) && cu2 >= diem) continue;

            diemTheoGoc[gocCum] = diem;
            dyTheoGoc[gocCum] = TinhDich(r, terr, goc);
        }

        int daDich = 0;
        foreach (var kv in dyTheoGoc)
        {
            if (Mathf.Abs(kv.Value) < 0.0005f) continue;

            Undo.RecordObject(kv.Key, "tach mat khoi dat");
            kv.Key.position += new Vector3(0f, kv.Value, 0f);
            EditorUtility.SetDirty(kv.Key);
            daDich++;
        }
        return daDich;
    }

    /// <summary>
    /// Dich bao nhieu theo truc Y de moi mat nam ngang cua vat cach mat dat it
    /// nhat <see cref="KheMucTieu"/>.
    ///
    /// Hai duong ra, va chon duong NGAN hon:
    ///
    ///   NANG  - day het ca vat len tren dat. Phai lay do cao dat CAO NHAT
    ///           trong hop bao, khong phai do cao o giua: vat nam vat qua mot
    ///           go thi cho go moi la cho dinh.
    ///   DIM   - an het ca vat xuong duoi dat. Chi hop voi vat mong (phien da
    ///           nam, de bia) - dim mot cai bia cao 1,4 m thi mat cai bia.
    ///
    /// Cai nao vuot tran cua no thi loai; ca hai deu vuot thi lay cai it te hon,
    /// van con hon la de nguyen cho no nhap nhay.
    /// </summary>
    static float TinhDich(MeshRenderer r, Terrain terr, float goc)
    {
        var b = r.bounds;

        float datThap = 9e9f, datCao = -9e9f;
        for (int ix = 0; ix <= N; ix++)
            for (int iz = 0; iz <= N; iz++)
            {
                float x = Mathf.Lerp(b.min.x, b.max.x, ix / (float)N);
                float z = Mathf.Lerp(b.min.z, b.max.z, iz / (float)N);
                float d = terr.SampleHeight(new Vector3(x, 0f, z)) + goc;
                if (d < datThap) datThap = d;
                if (d > datCao) datCao = d;
            }

        // Nang: day cho DAY vat len tren cho dat cao nhat
        float dyNang = (datCao + KheMucTieu) - b.min.y;
        if (dyNang < 0f) dyNang = 0f;

        // Dim: keo cho NOC vat xuong duoi cho dat thap nhat
        float dyDim = (datThap - KheMucTieu) - b.max.y;
        if (dyDim > 0f) dyDim = 0f;

        bool nangDuoc = dyNang <= NangToiDa;
        bool dimDuoc = -dyDim <= b.size.y * DimToiDa;

        if (nangDuoc && dimDuoc) return Mathf.Abs(dyNang) <= Mathf.Abs(dyDim) ? dyNang : dyDim;
        if (nangDuoc) return dyNang;
        if (dimDuoc) return dyDim;
        return Mathf.Abs(dyNang) <= Mathf.Abs(dyDim) ? dyNang : dyDim;
    }

    /// <summary>
    /// Leo len den GOC CUM: vat the nho nhat con dang la MOT DO VAT tron ven.
    ///
    /// Phan biet CUM voi THUNG:
    ///
    ///   thung  - "BiaMo" o Act2 co 451 con, moi con la mot ngoi mo rieng. Dich
    ///            ca cai thung do la nang toan bo nghia dia len troi.
    ///   cum    - "BuiCo_34" co 2 con: mat co va bong hoa cua chinh no. Dich
    ///            rieng mot con la xe doi bui co.
    ///
    /// Moc phan biet: <b>tu 5 con tro len thi la thung</b>. Chua gap do vat nao
    /// o day duoc ghep tu qua bon manh, con thung thi it nhat cung vai chuc.
    /// </summary>
    static Transform GocCum(Transform t)
    {
        var hienTai = t;
        while (hienTai.parent != null)
        {
            int soCon = 0;
            foreach (Transform c in hienTai.parent)
                if (c.GetComponentInChildren<MeshRenderer>(true) != null) soCon++;

            if (soCon > 4) break;                       // cha la thung -> dung
            if (hienTai.parent.GetComponent<Damageable>() != null) break;
            hienTai = hienTai.parent;
        }
        return hienTai;
    }

    /// <summary>
    /// Tra nhung bo phan da bi dich RIENG ve dung cho cua no trong cum.
    ///
    /// Ban dau cua cong cu nay dich thang <c>r.transform</c>, nen 45 cai
    /// "MatCo" da truot ra khoi bui co cua chung, lech toi 13 cm. Nhan ra duoc
    /// vi <c>MatHoa</c> - bo phan kia cua CHINH nhung bui co do - van con
    /// nguyen <c>localPosition.y = 0</c> het.
    ///
    /// Quy tac tra lai khong doan mo hinh nao het, ma doc tu chinh canh: mot bo
    /// phan bi coi la da lech khi <b>co it nhat mot cai CUNG TEN trong cum nho
    /// dang de localY = 0</b>. Ten trung nhau ma cai co cai khong thi cai "co"
    /// la cai bi dich.
    /// </summary>
    public static int TraLaiCumBiTach()
    {
        // Ten nao co it nhat mot cai dang de 0
        var coCaiBang0 = new HashSet<string>();
        var trongCum = new List<Transform>();

        foreach (var r in Object.FindObjectsByType<MeshRenderer>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var t = r.transform;
            if (t.parent == null) continue;
            if (GocCum(t) == t) continue;               // chinh no la goc cum, khong phai bo phan

            trongCum.Add(t);
            if (Mathf.Abs(t.localPosition.y) <= 0.0005f) coCaiBang0.Add(t.name);
        }

        int n = 0;
        foreach (var t in trongCum)
        {
            if (Mathf.Abs(t.localPosition.y) <= 0.0005f) continue;
            if (!coCaiBang0.Contains(t.name)) continue;

            Undo.RecordObject(t, "tra bo phan ve cum");
            var v = t.localPosition;
            v.y = 0f;
            t.localPosition = v;
            EditorUtility.SetDirty(t);
            n++;
        }
        return n;
    }

    /// <summary>
    /// Vat the nao khong xet toi.
    ///
    /// CAY: than dung thang, khong co mat nam ngang nao ap vao dat - va dich cay
    /// len xuong thi goc cay ho ra khoi dat, thanh mot loi khac.
    /// DIA HINH: chinh no la mat dat.
    /// QUAI VA NGUOI CHOI: chung tu dat minh moi khung hinh.
    /// </summary>
    static bool BoQua(MeshRenderer r)
    {
        var g = r.gameObject;
        if (g.name.StartsWith("TREE_")) return true;
        if (g.GetComponent<Terrain>() != null) return true;
        if (g.GetComponentInParent<Damageable>() != null) return true;
        if (LaMatPhang(r)) return true;
        return false;
    }

    /// <summary>
    /// Mat NUOC (va moi tam phang tuyet doi khac): khong duoc dich.
    ///
    /// Chung day 0 m, nen <see cref="TinhDich"/> khong con duong "dim xuong"
    /// (dim toi da la 45% cua 0), chi con duong NANG - va nang thi lay theo cho
    /// dat CAO NHAT trong hop bao, tuc la len tren ca mep bo. Ket qua dung nhu
    /// nguoi choi bao: <i>"vung nuoc dang bi lo lung o tren khong trung"</i>.
    /// Do duoc 258 dinh mep lo ra khoi mat dat sau mot lan chay.
    ///
    /// Mat nuoc co co che rieng va dung han: <see cref="Act2LongVungNuoc"/> ha
    /// mat nuoc xuong duoi duong bo roi khoet long ben trong. Chay bo dich len
    /// chung la pha dung cai vua lam.
    /// </summary>
    static bool LaMatPhang(MeshRenderer r)
    {
        return r.bounds.size.y < 0.02f;
    }

    /// <summary>
    /// Dem xem co bao nhieu diem tren mat DAY hoac mat NOC cua vat nam trong khe
    /// nguy hiem so voi mat dat.
    /// </summary>
    static int DiemDongPhang(MeshRenderer r, Terrain terr, float goc, float khe)
    {
        var b = r.bounds;
        int n = 0;

        for (int ix = 0; ix <= N; ix++)
            for (int iz = 0; iz <= N; iz++)
            {
                float x = Mathf.Lerp(b.min.x, b.max.x, ix / (float)N);
                float z = Mathf.Lerp(b.min.z, b.max.z, iz / (float)N);
                float dat = terr.SampleHeight(new Vector3(x, 0f, z)) + goc;

                if (Mathf.Abs(b.min.y - dat) < khe || Mathf.Abs(b.max.y - dat) < khe) n++;
            }
        return n;
    }
}
