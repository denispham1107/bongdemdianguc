using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU: QUAI VONG NGOAI TU TRUY LUNG SAU 60 GIAY.
///
/// Nguoi dung xin (13/09/2026): 20 con quai vong ngoai cua moi dot, neu sau 60
/// giay van chua tim thay nguoi choi, thi tu biet nguoi choi gan nhat o dau va
/// chay toi tan cong - tranh canh con vai con tren ban do ma tim mai khong thay.
///
/// Dung DOT THAT cua director (doi no tu tha dot dau o giay 30), khong goi tay.
/// Giet nhom quanh nguoi ngay khi dot ra de phep do chi con quai vong ngoai.
///
/// Do bang SO, khong doan:
///   A. Hen gio: moi con vong ngoai hen dung 60 giay tu luc sinh.
///   B. Truoc 60 giay: nhung con chua tu gap nguoi choi (chua vao 14 m) TRUNG
///      BINH khong duoc tien lai gan (so khoang cach giay 0 va giay 57).
///   C. Sau 60 giay: trong 30 giay, dem con vao duoc tam phat hien (14 m) va con
///      thuc su RA DON (su kien EnemyAI.DaRaDon - doc lap voi bien truy lung).
///      Con khong toi duoc thi ghi vi tri + quang di 5 giay cuoi (ket vat can?).
///   D. Doi chung: hai con quai thuong (khong hen truy lung) dat 38-45 m - suot
///      phep thu phai KHONG ra don va khong ap sat. Khong co doi chung thi "C dat"
///      co the chi vi quai nao cung tu chay toi.
///   E. Ca ket dung dat: bo xuong truy lung dung sau vat can lon (tia thang bi
///      chan) - phai toi sat nguoi choi trong 30 giay nho vong vat can / doi cho.
///
/// Ket qua ghi ra <c>PlayTestShots/quai_truylung.txt</c>.
/// </summary>
public static class ThuQuaiTruyLung
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/64. Chay thu QUAI VONG NGOAI TRUY LUNG (60 giay)", false, 155)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu quai truy lung",
                "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] quai vong ngoai tu truy lung sau " + EnemyAI.GiayTruyLung + " giay");

        canhCu = EditorSceneManager.GetActiveScene().path;
        if (canhCu != "Assets/Scenes/Act2.unity") EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity");

        truocBat = EditorSettings.enterPlayModeOptionsEnabled; truocOpt = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= Nhip;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying || daBatDau) return;
        if (GameObject.Find("TAM_TruyLung") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_TruyLung");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[TruyLung] " + s); }
    static void Loi(string s) { Ghi("[LOI] " + s); loi++; }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) Loi(loiNeuSai); }

    static float KhoangPhang(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    class TheoDoi
    {
        public EnemyAI ai;
        public Damageable mau;
        public string ten;
        public float kc0, kc57 = -1f;
        public bool gapTruoc60;          // da tu vao 14 m truoc giay 60
        public float toiLuc = -1f;       // giay (tinh tu luc sinh) lan dau vao 14 m SAU giay 60
        public bool raDon;
        public float kcMin = float.MaxValue;
        public readonly List<Vector3> vet = new List<Vector3>();   // vi tri moi 0,5 giay
    }

    static IEnumerator KichBan()
    {
        var dir = GameDirector.Instance;
        float han = Time.time + 30f;
        while (dir == null && Time.time < han) { dir = GameDirector.Instance; yield return null; }
        if (dir == null) { Loi("khong co GameDirector"); Ket(); yield break; }

        var pc = Object.FindAnyObjectByType<PlayerController>();
        if (pc == null) { Loi("khong tim thay nhan vat"); Ket(); yield break; }
        var toi = pc.GetComponent<Damageable>();
        toi.maxHealth = 10000000f; toi.health = 10000000f;
        Ghi("Act2, che do dot quanh nguoi = " + dir.CheDoDotQuanhNguoi + ", nhan vat o " + pc.transform.position.ToString("F1"));
        Kiem(dir.CheDoDotQuanhNguoi, "Act2 khong bat luat dot quanh nguoi");

        // ---- D. dat hai con doi chung (quai thuong, khong hen truy lung) ----
        var doiChung = new List<TheoDoi>();
        for (int lan = 0; lan < 400 && doiChung.Count < 2; lan++)
        {
            float g = Random.Range(0f, Mathf.PI * 2f), r = Random.Range(38f, 45f);
            Vector3 p = pc.transform.position + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
            float y;
            if (!ChoXuatPhat.DungTrenDat(p, out y)) continue;
            p.y = y;
            if (ChoXuatPhat.DuoiNuoc(p) || ChoXuatPhat.VuongVatCan(p)) continue;
            var go = EnemyFactory.Spawn(MonsterType.Skeleton, p + Vector3.up * 0.15f, null, pc.transform);
            if (go == null) continue;
            go.name = "TAM_DoiChung_" + doiChung.Count;
            var td = new TheoDoi { ai = go.GetComponent<EnemyAI>(), mau = go.GetComponent<Damageable>(), ten = go.name };
            td.kc0 = KhoangPhang(p, pc.transform.position);
            doiChung.Add(td);
        }
        Ghi("D0. doi chung: " + doiChung.Count + " con bo xuong khong hen truy lung, cach "
            + (doiChung.Count > 0 ? doiChung[0].kc0.ToString("F1") : "?") + " / "
            + (doiChung.Count > 1 ? doiChung[1].kc0.ToString("F1") : "?") + " m");
        Kiem(doiChung.Count == 2, "khong dat duoc hai con doi chung");
        foreach (var td in doiChung)
            Kiem(td.ai != null && td.ai.truyLungTuLuc < 0f, "quai thuong lai co hen truy lung");

        // ---- Doi director TU tha dot dau ----
        han = Time.time + 45f;
        while (dir.Wave < 1 && Time.time < han) yield return null;
        if (dir.Wave < 1) { Loi("director khong tha dot dau trong 45 giay"); Ket(); yield break; }
        float t0 = Time.time;
        var vongNgoai = new List<TheoDoi>();
        foreach (var d in dir.QuaiXaDotNay)
        {
            if (d == null) continue;
            vongNgoai.Add(new TheoDoi { ai = d.GetComponent<EnemyAI>(), mau = d, ten = d.name });
        }
        // Giet nhom quanh nguoi NGAY cung khung hinh - con quai vong ngoai trong alive nen director khong ra dot moi
        var tapNgoai = new HashSet<Damageable>(dir.QuaiXaDotNay);
        int daGiet = 0;
        foreach (var d in Object.FindObjectsByType<Damageable>(FindObjectsSortMode.None))
        {
            if (d == null || d.IsDead || tapNgoai.Contains(d) || d == toi) continue;
            if (d.GetComponent<EnemyAI>() == null || d.name.StartsWith("TAM_")) continue;
            d.TakeDamage(d.maxHealth * 10f, DamageType.Fire, d.transform.position);
            daGiet++;
        }
        Ghi("dot " + dir.Wave + " ra o giay " + Time.timeSinceLevelLoad.ToString("F1") + ": " + vongNgoai.Count
            + " con vong ngoai, giet " + daGiet + " con quanh nguoi de do rieng");
        Kiem(vongNgoai.Count == GameDirector.SoQuaiXaMoiDot, "dot dau khong du " + GameDirector.SoQuaiXaMoiDot + " con vong ngoai");

        // ---- A. hen gio ----
        int henDung = 0; float lechMax = 0f;
        foreach (var td in vongNgoai)
        {
            if (td.ai == null) continue;
            float lech = Mathf.Abs(td.ai.truyLungTuLuc - (t0 + EnemyAI.GiayTruyLung));
            lechMax = Mathf.Max(lechMax, lech);
            if (lech < 0.05f) henDung++;
            td.kc0 = KhoangPhang(td.ai.transform.position, pc.transform.position);
        }
        Ghi("A1. hen truy lung dung " + EnemyAI.GiayTruyLung + " giay tu luc sinh: " + henDung + "/" + vongNgoai.Count
            + " (lech lon nhat " + lechMax.ToString("F3") + " giay)");
        Kiem(henDung == vongNgoai.Count, "co con vong ngoai khong hen dung 60 giay");

        // Dem ra don bang su kien that cua EnemyAI
        var daRaDon = new HashSet<EnemyAI>();
        System.Action<EnemyAI, int, Transform, Vector3> nghe = (q, kieu, mt, diem) => daRaDon.Add(q);
        foreach (var td in vongNgoai) if (td.ai != null) td.ai.DaRaDon += nghe;
        foreach (var td in doiChung) if (td.ai != null) td.ai.DaRaDon += nghe;

        // ---- Lay mau moi 0,5 giay den giay 90 ----
        const float HetGio = 90f;
        float nhipMau = 0f;
        while (Time.time - t0 < HetGio)
        {
            float t = Time.time - t0;
            toi.health = toi.maxHealth;
            if (Time.time >= nhipMau)
            {
                nhipMau = Time.time + 0.5f;
                foreach (var td in vongNgoai)
                {
                    if (td.ai == null || td.mau == null || td.mau.IsDead) continue;
                    float kc = KhoangPhang(td.ai.transform.position, pc.transform.position);
                    td.vet.Add(td.ai.transform.position);
                    if (t < EnemyAI.GiayTruyLung)
                    {
                        if (kc <= td.ai.aggroRange) td.gapTruoc60 = true;
                        if (t >= 57f && td.kc57 < 0f) td.kc57 = kc;
                        if (t >= 57f) Kiem(!td.ai.DangTruyLung, td.ten + " da truy lung truoc 60 giay");
                    }
                    else
                    {
                        td.kcMin = Mathf.Min(td.kcMin, kc);
                        if (td.toiLuc < 0f && kc <= td.ai.aggroRange) td.toiLuc = t;
                    }
                }
                foreach (var td in doiChung)
                    if (td.ai != null) td.kcMin = Mathf.Min(td.kcMin, KhoangPhang(td.ai.transform.position, pc.transform.position));
            }
            yield return null;
        }
        foreach (var td in vongNgoai) td.raDon = td.ai != null && daRaDon.Contains(td.ai);

        // ---- B. truoc 60 giay ----
        Ghi("");
        Ghi("B. truoc " + EnemyAI.GiayTruyLung + " giay");
        int chuaGap = 0, tuGap = 0;
        float tongTien = 0f, tienMax = float.MinValue;
        var sbB = new StringBuilder();
        foreach (var td in vongNgoai)
        {
            if (td.gapTruoc60) { tuGap++; continue; }
            if (td.kc57 < 0f) continue;
            chuaGap++;
            float tien = td.kc0 - td.kc57;
            sbB.Append(td.kc0.ToString("F0")).Append("->").Append(td.kc57.ToString("F0")).Append(" ");
            tongTien += tien;
            tienMax = Mathf.Max(tienMax, tien);
        }
        Ghi("B1. " + tuGap + " con tu gap nguoi choi (lang thang vao 14 m); " + chuaGap
            + " con chua gap - khoang cach giay 0 -> giay 57 (m): " + sbB.ToString().Trim());
        // Lang thang la NGAU NHIEN: tung con co the troi lai gan 8 m hay ra xa 16 m
        // (lan chay thu nam: 25 -> 17 m; lan khac 22 -> 38 m) nen khong xet tung con.
        // Con truy lung thi vao 14 m chi trong vai giay; con chua gap ma TRUNG BINH
        // khong tien lai la chung khong truy lung.
        float tbTien = chuaGap > 0 ? tongTien / chuaGap : 0f;
        Ghi("B2. trung binh tien lai " + tbTien.ToString("F1") + " m (tien nhieu nhat " + (chuaGap > 0 ? tienMax.ToString("F1") : "-") + " m)");
        Kiem(tbTien <= 3f, "quai chua gap trung binh da tien lai " + tbTien.ToString("F1") + " m truoc gio truy lung");

        // ---- C. sau 60 giay ----
        Ghi("");
        Ghi("C. sau " + EnemyAI.GiayTruyLung + " giay (theo doi den giay " + HetGio + ")");
        int conSong = 0, toiNoi = 0, raDon = 0;
        var thoiGian = new List<float>();
        foreach (var td in vongNgoai)
        {
            if (td.mau == null || td.mau.IsDead) continue;
            conSong++;
            if (td.kcMin <= td.ai.aggroRange) toiNoi++;
            // thoi gian chi tinh cho con CHUA tu gap truoc do - con da o gan thi vao 14 m ngay giay 60
            if (!td.gapTruoc60 && td.toiLuc >= 0f) thoiGian.Add(td.toiLuc - EnemyAI.GiayTruyLung);
            if (td.raDon) raDon++;
        }
        thoiGian.Sort();
        int tongVong = 0, tongDoiCho = 0, conDoiCho = 0;
        foreach (var td in vongNgoai)
            if (td.ai != null) { tongVong += td.ai.soLanVong; tongDoiCho += td.ai.soLanDoiChoViKet; if (td.ai.soLanDoiChoViKet > 0) conDoiCho++; }
        Ghi("C0. bi ket phai vong vat can " + tongVong + " lan; phai doi cho gan nguoi choi " + tongDoiCho
            + " lan (" + conDoiCho + " con)");
        Ghi("C1. " + toiNoi + "/" + conSong + " con vao duoc tam phat hien 14 m; " + raDon + "/" + conSong
            + " con thuc su ra don vao nguoi choi");
        if (thoiGian.Count > 0)
            Ghi("C2. mat bao lau sau giay 60 de vao 14 m: nhanh nhat " + thoiGian[0].ToString("F1") + " giay, cham nhat "
                + thoiGian[thoiGian.Count - 1].ToString("F1") + " giay, trung vi " + thoiGian[thoiGian.Count / 2].ToString("F1") + " giay");
        foreach (var td in vongNgoai)
        {
            if (td.mau == null || td.mau.IsDead) continue;
            if (td.kcMin <= td.ai.aggroRange) continue;
            float di5 = 0f;
            for (int i = Mathf.Max(1, td.vet.Count - 10); i < td.vet.Count; i++) di5 += KhoangPhang(td.vet[i], td.vet[i - 1]);
            Ghi("    KHONG TOI: " + td.ten + " o " + td.ai.transform.position.ToString("F1") + ", gan nhat "
                + td.kcMin.ToString("F1") + " m, di duoc " + di5.ToString("F1") + " m trong 5 giay cuoi");
        }
        // Con vao duoc 14 m ma KHONG ra don: ghi du de phan biet "bi quai khac chen
        // khong lot vao tam tay" voi "dung lai / khong danh". Don can chien chi bao
        // DaRaDon khi CHAM nguoi (TryHit) - dung sau lung dam dong thi khong bao.
        int biChen = 0;
        foreach (var td in vongNgoai)
        {
            if (td.mau == null || td.mau.IsDead || td.raDon || td.kcMin > td.ai.aggroRange) continue;
            float kcCuoi = KhoangPhang(td.ai.transform.position, pc.transform.position);
            int sathon = 0;
            foreach (var kh in vongNgoai)
                if (kh != td && kh.ai != null && kh.mau != null && !kh.mau.IsDead
                    && KhoangPhang(kh.ai.transform.position, pc.transform.position) < kcCuoi) sathon++;
            float di5 = 0f;
            for (int i = Mathf.Max(1, td.vet.Count - 10); i < td.vet.Count; i++) di5 += KhoangPhang(td.vet[i], td.vet[i - 1]);
            var nd = td.ai.GetComponent<NhanDangQuai>();
            bool chen = !td.ai.danhTuXa && kcCuoi <= td.ai.attackRange * 1.35f + 2.5f && sathon >= 6;
            if (chen) biChen++;
            Ghi("    KHONG RA DON: " + td.ten + " (" + (nd != null ? nd.loai.ToString() : "?") + (td.ai.danhTuXa ? ", danh xa" : ", can chien")
                + ") cach nguoi choi " + kcCuoi.ToString("F1") + " m, tam tay " + (td.ai.attackRange * 1.35f).ToString("F1")
                + " m, " + sathon + " con dung sat hon, di " + di5.ToString("F1") + " m trong 5 giay cuoi"
                + (chen ? " -> BI CHEN ngoai dam dong" : ""));
        }
        Kiem(conSong > 0, "khong con con vong ngoai nao de do");
        Kiem(toiNoi == conSong, (conSong - toiNoi) + " con vong ngoai khong toi duoc nguoi choi sau khi truy lung");
        Ghi("C3. ra don " + raDon + " + bi chen sat vong vay " + biChen + " = " + (raDon + biChen) + "/" + conSong);
        Kiem(raDon + biChen == conSong, (conSong - raDon - biChen) + " con vong ngoai toi noi ma khong danh, cung khong phai bi chen");

        // ---- D. doi chung ----
        Ghi("");
        foreach (var td in doiChung)
        {
            bool don = td.ai != null && daRaDon.Contains(td.ai);
            Ghi("D1. " + td.ten + ": cach " + td.kc0.ToString("F1") + " m, gan nhat trong " + HetGio + " giay "
                + td.kcMin.ToString("F1") + " m, ra don " + don);
            Kiem(!don && td.kcMin > 20f, td.ten + " (quai thuong) cung tu chay toi - phep do C khong phan biet duoc");
        }

        // ---- E. ca ket DUNG DAT: quai truy lung dung SAU vat can lon ----
        // Vi tri tha la ngau nhien nen lan chay nay co the khong ket con nao. Tu dung
        // ca ket: tim vat can lop Default rong 3-14 m (nha mo, bia lon, lo lua) cach
        // nguoi choi 8-30 m, dat bo xuong o PHIA BEN KIA vat can; KIEM bang tia thang
        // la vat can ay dang chan giua quai va nguoi choi (khong chan thi khong phai ca ket).
        Ghi("");
        // Don sach quai vong ngoai con song: dam dong vay nguoi choi se chan bo xuong
        // cua phan E ngoai vong vay. Giet het thi director doi 30 giay moi ra dot
        // moi - phan E xong trong 25 giay nen khong bi chen.
        foreach (var td in vongNgoai)
            if (td.mau != null && !td.mau.IsDead) td.mau.TakeDamage(td.mau.maxHealth * 10f, DamageType.Fire, td.mau.transform.position);
        yield return null;
        var caKet = new List<TheoDoi>();
        var vatDaDung = new List<Collider>();
        Vector3 goc = pc.transform.position;
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (caKet.Count >= 3) break;
            if (c.isTrigger || c.gameObject.layer != 0 || c is TerrainCollider) continue;
            var b = c.bounds;
            float rong = Mathf.Max(b.size.x, b.size.z);
            if (rong < 3f || rong > 14f || b.size.y < 1.5f) continue;
            float kc = KhoangPhang(b.center, goc);
            if (kc < 8f || kc > 30f) continue;
            bool gan = false;
            foreach (var v in vatDaDung) if (KhoangPhang(v.bounds.center, b.center) < 10f) gan = true;
            if (gan) continue;

            Vector3 huong = b.center - goc; huong.y = 0f; huong.Normalize();
            Vector3 p = b.center + huong * (rong * 0.5f + 1.6f);
            float y;
            if (!ChoXuatPhat.DungTrenDat(p, out y)) continue;
            p.y = y;
            if (ChoXuatPhat.DuoiNuoc(p) || ChoXuatPhat.VuongVatCan(p)) continue;
            RaycastHit hit;
            bool chan = Physics.Linecast(p + Vector3.up, goc + Vector3.up, out hit, 1 << 0, QueryTriggerInteraction.Ignore)
                        && hit.collider == c;
            if (!chan) continue;

            var go = EnemyFactory.Spawn(MonsterType.Skeleton, p + Vector3.up * 0.15f, null, pc.transform);
            if (go == null) continue;
            go.name = "TAM_CaKet_" + caKet.Count;
            var ai = go.GetComponent<EnemyAI>();
            ai.HenTruyLung(0f);
            var td = new TheoDoi { ai = ai, mau = go.GetComponent<Damageable>(), ten = go.name + " sau " + c.name };
            td.kc0 = KhoangPhang(p, goc);
            caKet.Add(td);
            vatDaDung.Add(c);
        }
        Ghi("E0. dung duoc " + caKet.Count + " ca quai bi vat can chan thang (tia thang tu quai toi nguoi choi trung vat can)");
        Kiem(caKet.Count >= 2, "khong dung duoc du ca vat can chan - phan E khong do gi");

        // Ca NHOT: bon buc tuong tam bao kin mot con - khong huong vong nao thong,
        // chi con cach DOI CHO. Khong co ca nay thi tang cuu thu hai khong bao gio
        // duoc do (lan chay truoc: doi cho 0 lan).
        var tuong = new List<GameObject>();
        TheoDoi nhot = null;
        for (int lan = 0; lan < 200 && nhot == null; lan++)
        {
            float g = Random.Range(0f, Mathf.PI * 2f), r = Random.Range(17f, 22f);
            Vector3 p = goc + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
            float y;
            if (!ChoXuatPhat.DungTrenDat(p, out y)) continue;
            p.y = y;
            if (ChoXuatPhat.DuoiNuoc(p) || ChoXuatPhat.VuongVatCan(p)) continue;
            if (Physics.CheckBox(p + Vector3.up * 1.3f, new Vector3(2f, 1.2f, 2f), Quaternion.identity, 1 << 0, QueryTriggerInteraction.Ignore)) continue;
            foreach (var t in new[] { new Vector3(1.6f, 0f, 0f), new Vector3(-1.6f, 0f, 0f), new Vector3(0f, 0f, 1.6f), new Vector3(0f, 0f, -1.6f) })
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.name = "TAM_Tuong";
                w.layer = 0;
                w.transform.position = p + t + Vector3.up * 1.4f;
                w.transform.localScale = Mathf.Abs(t.x) > 0f ? new Vector3(0.4f, 3.2f, 3.6f) : new Vector3(3.6f, 3.2f, 0.4f);
                tuong.Add(w);
            }
            Physics.SyncTransforms();
            var go = EnemyFactory.Spawn(MonsterType.Skeleton, p + Vector3.up * 0.15f, null, pc.transform);
            if (go == null) continue;
            go.name = "TAM_CaNhot";
            var ai = go.GetComponent<EnemyAI>();
            ai.HenTruyLung(0f);
            nhot = new TheoDoi { ai = ai, mau = go.GetComponent<Damageable>(), ten = go.name + " (bon buc tuong)" };
            nhot.kc0 = KhoangPhang(p, goc);
            caKet.Add(nhot);
        }
        Kiem(nhot != null, "khong dung duoc ca nhot trong tuong");
        float tE = Time.time;
        while (Time.time - tE < 25f)
        {
            toi.health = toi.maxHealth;
            foreach (var td in caKet)
                if (td.ai != null) td.kcMin = Mathf.Min(td.kcMin, KhoangPhang(td.ai.transform.position, pc.transform.position));
            yield return null;
        }
        foreach (var td in caKet)
        {
            Ghi("E1. " + td.ten + ": cach " + td.kc0.ToString("F1") + " m -> gan nhat trong 25 giay "
                + td.kcMin.ToString("F1") + " m; vong vat can " + td.ai.soLanVong + " lan, doi cho " + td.ai.soLanDoiChoViKet + " lan");
            Kiem(td.kcMin <= 4f, td.ten + " bi vat can chan van khong toi sat nguoi choi");
        }
        if (nhot != null) Kiem(nhot.ai.soLanDoiChoViKet >= 1, "ca nhot khong dung toi cach doi cho");
        foreach (var w in tuong) Object.Destroy(w);
        foreach (var td in caKet) if (td.ai != null) Object.Destroy(td.ai.gameObject);

        foreach (var td in vongNgoai) if (td.ai != null) td.ai.DaRaDon -= nghe;
        foreach (var td in doiChung) if (td.ai != null) { td.ai.DaRaDon -= nghe; Object.Destroy(td.ai.gameObject); }

        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
        var sc = EditorSceneManager.GetActiveScene();
        Debug.Log("[TruyLung] tra lai canh " + sc.path + ", isDirty = " + sc.isDirty);
    }

    static void Ket()
    {
        TranHienTai.Xoa();
        File.WriteAllText("PlayTestShots/quai_truylung.txt", bao.ToString());
        var rac = GameObject.Find("TAM_TruyLung");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
