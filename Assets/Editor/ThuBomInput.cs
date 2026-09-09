using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU BUOC 1: DA TACH DUOC Y MUON RA KHOI VIEC THI HANH CHUA.
///
/// Buoc 1 tach <see cref="PlayerController"/> lam doi: <see cref="DocInput"/>
/// doc phim, PlayerController thi hanh. Phep thu nay do dung cai ranh gioi do.
///
/// DO CA HAI CHIEU, khong phai mot:
///   - bom input tu ngoai  -> nhan vat PHAI di theo (nang luc moi, la thu ma
///     may trong tai va viec chay lai de hieu chinh se dung),
///   - tra ve tu doc phim  -> nhan vat PHAI dung yen (khong bam gi).
/// Chi do chieu dau thi khong phan biet duoc "bom an" voi "nhan vat cu chay
/// mai khong dung"; chi do chieu sau thi khong biet bom co an hay khong.
///
/// Ban phim khong bam duoc bang code (xem memory unity-runcommand-sandbox),
/// nen chieu "tu doc phim" chi kiem duoc rang KHONG bam thi KHONG chay - the
/// la du de bat cai loi nguy hiem nhat: input cu bi ket lai.
///
/// Ket qua ghi ra <c>PlayTestShots/bom_input.txt</c>.
/// </summary>
public static class ThuBomInput
{
    static string Canh = "Assets/Scenes/Act2.unity";

    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/30. Chay thu BOM INPUT - Act2 (buoc 1)", false, 116)]
    public static void ChayAct2() { Canh = "Assets/Scenes/Act2.unity"; Chay(); }

    /// <summary>
    /// Act1 dung bang code luc chay (GameBootstrap), Act2 la scene da nuong
    /// san - hai duong khac han nhau. Mot man chay dung khong chung minh duoc
    /// man kia, nen phai do ca hai.
    /// </summary>
    [MenuItem("Diablo 2.5D/30b. Chay thu BOM INPUT - Act1 (buoc 1)", false, 117)]
    public static void ChayAct1() { Canh = "Assets/Scenes/Act1.unity"; Chay(); }

    static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        canhCu = EditorSceneManager.GetActiveScene().path;

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().path != Canh)
            EditorSceneManager.OpenScene(Canh);

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;

        var go = new GameObject("TAM_BomInput");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[BomInput] " + s);
    }

    /// <summary>
    /// Bom mot goi input lien tuc trong <paramref name="giay"/> giay roi tra ve
    /// quang duong nhan vat da di (chi tinh mat phang ngang - roi tu do khong
    /// phai la di chuyen).
    /// </summary>
    static IEnumerator BomVaDo(PlayerController pc, GoiInput goi, float giay,
                               System.Action<Vector3> xong)
    {
        Vector3 dau = pc.transform.position;
        float het = Time.time + giay;

        while (Time.time < het)
        {
            goi.dt = Time.deltaTime;
            pc.input = goi;
            yield return null;
        }

        Vector3 di = pc.transform.position - dau;
        di.y = 0f;
        xong(di);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 2] buoc 1 - tach y muon, man = " + Canh);

        // Doi nhan vat xuat hien: Act2 dung canh mat vai giay
        PlayerController pc = null;
        float hetHan = Time.time + 20f;
        while (pc == null && Time.time < hetHan)
        {
            pc = Object.FindAnyObjectByType<PlayerController>();
            yield return null;
        }

        if (pc == null)
        {
            Ghi("[LOI] khong tim thay PlayerController sau 20 giay");
            loi++; Ket(); yield break;
        }

        Ghi("tim thay nhan vat: " + pc.name + ", moveSpeed = " + pc.moveSpeed);

        // Doi them mot chut cho nhan vat cham dat, khong thi quang duong dau
        // lan lon ca phan roi tu do
        yield return new WaitForSeconds(1.5f);

        bool coDocInput = pc.GetComponent<DocInput>() != null;
        Ghi("co bo doc input gan kem: " + coDocInput + " (phai la True)");
        if (!coDocInput) { Ghi("[LOI] Awake khong tu gan DocInput"); loi++; }

        float toc = pc.moveSpeed;
        float giay = 1.2f;
        float duKienToiThieu = toc * giay * 0.5f;   // vuong vat can thi ngan hon

        // ---- CHIEU 1: BOM INPUT TU NGOAI ----
        pc.tuDocInput = false;

        var goiPhai = GoiInput.Rong(0f);
        goiPhai.kyNang = -1;
        goiPhai.huongDi = Vector3.right;

        Vector3 diX = Vector3.zero;
        yield return BomVaDo(pc, goiPhai, giay, v => diX = v);

        Ghi(string.Format("bom huong (1,0,0) trong {0:F1}s -> di duoc {1:F2} m, "
                          + "lech truc X {2:F2} m (toi thieu can {3:F2} m)",
                          giay, diX.magnitude, diX.x, duKienToiThieu));

        if (diX.magnitude < duKienToiThieu)
        { Ghi("[LOI] bom input ma nhan vat khong di"); loi++; }
        else if (Vector3.Dot(diX.normalized, Vector3.right) < 0.7f)
        { Ghi("[LOI] co di nhung khong dung huong da bom"); loi++; }

        // ---- CHIEU 1b: DOI HUONG, PHAI DI HUONG KHAC ----
        var goiTien = GoiInput.Rong(0f);
        goiTien.kyNang = -1;
        goiTien.huongDi = Vector3.forward;

        Vector3 diZ = Vector3.zero;
        yield return BomVaDo(pc, goiTien, giay, v => diZ = v);

        Ghi(string.Format("bom huong (0,0,1) trong {0:F1}s -> di duoc {1:F2} m, "
                          + "lech truc Z {2:F2} m", giay, diZ.magnitude, diZ.z));

        if (diZ.magnitude < duKienToiThieu)
        { Ghi("[LOI] doi huong xong nhan vat khong di"); loi++; }
        else if (Vector3.Dot(diZ.normalized, Vector3.forward) < 0.7f)
        { Ghi("[LOI] di khong dung huong thu hai"); loi++; }

        // ---- CHIEU 2: TRA VE TU DOC PHIM, KHONG BAM GI -> PHAI DUNG YEN ----
        // Day la chieu de bat loi nguy hiem nhat: goi input cu bi ket lai va
        // nhan vat cu chay mai. Neu chi do chieu 1 thi loi do khong lo ra.
        pc.tuDocInput = true;

        Vector3 truocKhiDung = pc.transform.position;
        yield return new WaitForSeconds(1.2f);
        Vector3 troi = pc.transform.position - truocKhiDung;
        troi.y = 0f;

        Ghi(string.Format("tra ve tu doc phim, khong bam gi -> troi {0:F3} m "
                          + "(phai gan 0)", troi.magnitude));

        if (troi.magnitude > 0.35f)
        { Ghi("[LOI] khong bam gi ma nhan vat van chay - input cu bi ket"); loi++; }

        // ---- CHIEU 3: BOM MOT KY NANG ----
        // Bam phim khong gia lap duoc, nhung bom goi thi duoc - va do la dung
        // duong ma may trong tai se di.
        int truocSoLan = PlayerController.SoLanBamKyNang;
        float manaTruoc = pc.mana;

        pc.tuDocInput = false;
        var goiPhep = GoiInput.Rong(Time.deltaTime);
        goiPhep.kyNang = 0;                       // Qua cau lua
        goiPhep.coNgam = true;
        goiPhep.diemNgam = pc.transform.position + pc.transform.forward * 8f;
        pc.input = goiPhep;
        yield return null;

        // Khung sau phai tra ky nang ve -1, khong thi no tung lien tuc
        var goiRong = GoiInput.Rong(Time.deltaTime);
        pc.input = goiRong;
        yield return new WaitForSeconds(0.6f);

        int themSoLan = PlayerController.SoLanBamKyNang - truocSoLan;
        Ghi(string.Format("bom ky nang 0 -> so lan bam ky nang tang {0} (phai la 1), "
                          + "nang luong {1:F0} -> {2:F0}",
                          themSoLan, manaTruoc, pc.mana));

        if (themSoLan != 1)
        { Ghi("[LOI] bom ky nang khong toi duoc cho thi hanh"); loi++; }
        else if (pc.mana >= manaTruoc)
        { Ghi("[LOI] co nhan ky nang nhung khong tru nang luong - chua thuc su tung"); loi++; }

        pc.tuDocInput = true;

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        string ten = Canh.Contains("Act1") ? "bom_input_act1.txt" : "bom_input_act2.txt";
        File.WriteAllText("PlayTestShots/" + ten, bao.ToString());

        var rac = GameObject.Find("TAM_BomInput");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;

        // Tra lai canh nguoi dung dang mo truoc do
        if (!string.IsNullOrEmpty(canhCu) && canhCu != Canh)
            EditorApplication.delayCall += () => EditorSceneManager.OpenScene(canhCu);
    }
}
