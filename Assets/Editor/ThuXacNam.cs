using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU (menu 85): XAC NAM TREN VUNG MAU + DAU "+" NANG CAP TREN O KY NANG (nguoi dung 26/09/2026).
///
/// Nguoi dung: "quai vat va nguoi choi khac khi bi giet chet thi thay xac nam duoi mat dat kem vung mau nho, xac nam de len
/// vung mau" (chon: xac quai 20 giay roi chim dan; xac nguoi choi nam toi het tran); "o ky nang vuong / tron: co diem ky nang
/// moi thi hien dau + nho phia tren, bam vao de nang cap, khong de tran de che o khac" (chon: ca mo khoa lan nang cap; nut tron
/// thi dau + o dinh nut lan nua vao vien).
///
///   A. Bon loai quai DANG DUNG (bo xuong, phu thuy, quy du, quy cay - sinh THAT tu kho quai): giet, roi do HINH THAT bang
///      BakeMesh (khong doc bien cua XacNam): dinh hinh tren dat luc dung / luc nam (nam phai thap han); vung mau co, loang tu
///      nho ra du; bao nhieu phan tram dinh cua xac nam TRONG vung mau (xac de len mau); bao nhieu dinh chui duoi dat; mau ve
///      SAU xac (hang doi). Moc thoi gian: 15 giay VAN CON (ban cu mat sau 5-6 giay - doi chung), 21 giay dang chim, 23 giay
///      da xoa CA vung mau.
///   B. Chet luc dang bi danh nga (qua moc chong day 1,5 s van nam) va luc dang bi hat tung (mau chi loang sau khi roi xuong).
///   D. Dau cong: co / khong theo diem, khoa, bac, cap toi da, binh; bam -> mo khoa / nang cap, tru diem; hinh hoc - huy hieu
///      va vung bam KHONG cham nut / o nao khac, cham giua nut van la tung ky nang; chup ban may tinh + cam ung.
///   C. Nguoi choi chet: xac nam, co vung mau, 23 giay sau VAN nam (khong chim).
///
///   T. (26/09/2026, nguoi dung xin tu the NGAU NHIEN: nam ngua / sap / nghieng) - ep lan luot SAP, NGHIENG TRAI, NGHIENG PHAI
///      cho ca bon loai quai: HUONG MAT do bang xuong that Head -> headfront (ngua: mat len troi, sap: mat xuong dat, nghieng:
///      mat nam ngang), dau ha sat dat, than trong vung mau, it dinh chui dat; muc A chay voi NGUA. Ti le chon tu the tren
///      6000 hat giong (~1/3 moi kieu); cung so hieu mang -> cung tu the (hai may thay giong nhau).
///
/// Ket qua: PlayTestShots/xacnam.txt, anh xacnam_*.png.
/// </summary>
public static class ThuXacNam
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static string canhCu;
    static bool truocBat;
    static EnterPlayModeOptions truocOpt;

    [MenuItem("Diablo 2.5D/85. Chay thu XAC NAM + DAU CONG NANG CAP", false, 174)]
    public static void Chay()
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorUtility.DisplayDialog("Chay thu", "Scene dang mo co thay doi chua luu - luu hoac bo truoc da.", "OK");
            return;
        }
        Directory.CreateDirectory("PlayTestShots");
        bao.Length = 0; loi = 0; daBatDau = false;
        Ghi("[ban 1] xac nam tren vung mau + dau cong nang cap");
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
        if (GameObject.Find("TAM_XacNam") != null) return;
        daBatDau = true;
        var go = new GameObject("TAM_XacNam");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ChayThuMang>().batDau = KichBan();
    }

    static void Ghi(string s) { bao.AppendLine(s); Debug.Log("[XacNam] " + s); }
    static void Kiem(bool dat, string loiNeuSai) { if (!dat) { Ghi("[LOI] " + loiNeuSai); loi++; } }

    static IEnumerator Chup(string ten)
    {
        string duong = "PlayTestShots/" + ten + ".png";
        if (File.Exists(duong)) File.Delete(duong);
        ScreenCapture.CaptureScreenshot(duong);
        for (int i = 0; i < 90 && !File.Exists(duong); i++) yield return new WaitForEndOfFrame();
    }

    static PlayerController TimToi()
    {
        foreach (var pc in Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude))
        {
            var d = pc.GetComponent<Damageable>();
            if (d != null && d.mauDoMayKhacQuyet) continue;
            if (!pc.tuDocInput) continue;
            return pc;
        }
        return null;
    }

    static float DatY(Vector3 p)
    {
        RaycastHit h;
        if (Physics.Raycast(p + Vector3.up * 3f, Vector3.down, out h, 12f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
            return h.point.y;
        return p.y;
    }

    /// <summary>Dinh THAT cua hinh (BakeMesh cho luoi co xuong; luoi thuong doc duoc thi doc, khong thi 8 goc khung bao).</summary>
    static List<Vector3> DinhHinh(GameObject go)
    {
        var ds = new List<Vector3>();
        if (go == null) return ds;
        var tam = new Mesh();
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
            var smr = r as SkinnedMeshRenderer;
            if (smr != null)
            {
                smr.BakeMesh(tam, true);
                var m = Matrix4x4.TRS(smr.transform.position, smr.transform.rotation, Vector3.one);
                var v = tam.vertices;
                for (int i = 0; i < v.Length; i += 3) ds.Add(m.MultiplyPoint3x4(v[i]));
                continue;
            }
            var mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable)
            {
                var v = mf.sharedMesh.vertices;
                for (int i = 0; i < v.Length; i += 3) ds.Add(r.transform.TransformPoint(v[i]));
            }
            else
            {
                var b = r.bounds;
                for (int k = 0; k < 8; k++)
                    ds.Add(new Vector3((k & 1) == 0 ? b.min.x : b.max.x, (k & 2) == 0 ? b.min.y : b.max.y, (k & 4) == 0 ? b.min.z : b.max.z));
            }
        }
        Object.Destroy(tam);
        return ds;
    }

    /// <summary>Do cao tren dat cua xuong co ten chua chuoi nay (Head / Hips) - xuong THAT cua model, khong phai bien cua XacNam.</summary>
    static float CaoXuong(GameObject go, string ten, float datY)
    {
        if (go == null) return -1f;
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (var b in smr.bones)
                if (b != null && b.name == ten) return b.position.y - datY;
        return -1f;
    }

    static Transform TimXuong(GameObject go, string ten)
    {
        if (go == null) return null;
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (var b in smr.bones)
                if (b != null && b.name == ten) return b;
        return null;
    }

    /// <summary>Thanh phan THANG DUNG cua huong mat (Head -> headfront): +1 mat len troi, -1 up xuong dat, 0 nhin ngang.</summary>
    static float HuongMatY(GameObject go)
    {
        var dau = TimXuong(go, "Head"); var mat = TimXuong(go, "headfront");
        if (dau == null || mat == null) return 9f;
        Vector3 v = mat.position - dau.position;
        return v.sqrMagnitude < 1e-8f ? 9f : v.normalized.y;
    }

    /// <summary>Xuong THAN thap nhat (Hips / Spine / neck / Head) so voi dat - than lun thi am.</summary>
    static float ThanThapNhat(GameObject go, float datY)
    {
        float m = 99f;
        if (go == null) return m;
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (var b in smr.bones)
                if (b != null && (b.name == "Hips" || b.name == "Head" || b.name.Contains("Spine") || b.name.Contains("neck")))
                    m = Mathf.Min(m, b.position.y - datY);
        return m;
    }

    static float DinhTren(GameObject go, float datY)
    {
        float m = -99f;
        foreach (var p in DinhHinh(go)) m = Mathf.Max(m, p.y - datY);
        return m;
    }

    class MotXac
    {
        public string ten; public Damageable d; public GameObject go; public float datY, caoDung, caoNam = -1f, daiNam, dayNam;
        public float dauDung = -1f, dauNam = -1f, hongNam = -1f;
        public float lucChet, lucCoMau = -1f, loangSom = -1f, loangMuon = -1f, trongMau, duoiDat, kcTam; public int queueXac;
        public bool con15, chim21, xoa23, mauXoa23;
    }

    static IEnumerator KichBan()
    {
        float han0 = Time.time + 30f;
        while (GameDirector.Instance == null && Time.time < han0) yield return null;
        yield return new WaitForSeconds(1.5f);
        if (GameDirector.Instance != null) GameDirector.Instance.enabled = false;
        foreach (var q in Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include)) Object.Destroy(q.gameObject);

        var toi = TimToi();
        if (toi == null) { Ghi("[LOI] khong tim thay nhan vat"); loi++; Ket(); yield break; }
        var mauToi = toi.GetComponent<Damageable>();
        mauToi.maxHealth = 1e6f; mauToi.health = 1e6f;
        var hud = Object.FindAnyObjectByType<GameHUD>();
        var rig = Object.FindAnyObjectByType<CameraRig>();
        CapDo.BatDauTranMoi();
        Vector3 goc = toi.transform.position;
        Vector3 huong = toi.transform.forward; huong.y = 0f; huong.Normalize();
        Vector3 ngang = Vector3.Cross(Vector3.up, huong).normalized;

        var loai = new[] { MonsterType.Skeleton, MonsterType.Witch, MonsterType.QuyDu, MonsterType.QuyCay };

        // ================= T. TU THE NGAU NHIEN: SAP, NGHIENG TRAI, NGHIENG PHAI =================
        Ghi("");
        {
            // Ti le va tinh xac dinh cua cach chon (khong can Play)
            int[] dem = new int[4];
            for (uint h = 0; h < 6000; h++) dem[(int)XacNam.ChonTuThe(h)]++;
            bool giongNhau = true;
            for (uint h = 0; h < 200; h++) if (XacNam.ChonTuThe(h * 37u) != XacNam.ChonTuThe(h * 37u)) giongNhau = false;
            int doiLienTiep = 0;
            for (uint h = 1; h < 300; h++) if (XacNam.ChonTuThe(h) != XacNam.ChonTuThe(h - 1)) doiLienTiep++;
            Ghi(string.Format("T. 6000 hat giong: ngua {0}, sap {1}, nghieng trai {2}, nghieng phai {3} (mong ~2000 / 2000 / 1000 / 1000); cung hat -> cung tu the {4}; so hieu lien tiep doi tu the {5}/299 lan",
                dem[0], dem[1], dem[2], dem[3], giongNhau, doiLienTiep));
            Kiem(Mathf.Abs(dem[0] - 2000) < 150 && Mathf.Abs(dem[1] - 2000) < 150 && Mathf.Abs(dem[2] - 1000) < 120 && Mathf.Abs(dem[3] - 1000) < 120,
                 "ti le tu the khong deu 1/3 ngua - 1/3 sap - 1/3 nghieng");
            Kiem(giongNhau && doiLienTiep > 150, "chon tu the khong xac dinh / so hieu lien tiep ra cung tu the");

            // Hai con quai cung SO HIEU MANG (nhu tren may chu phong va may khach) -> cung tu the, khong ep
            int giongMang = 0;
            for (ushort id = 11; id < 17; id++)
            {
                var kieu = new List<XacNam.KieuNam>();
                for (int k = 0; k < 2; k++)
                {
                    Vector3 p = goc - huong * 6f + ngang * (k * 3f - 1.5f); p.y = DatY(p);
                    var go = EnemyFactory.Spawn(MonsterType.Skeleton, p, null, toi.transform);
                    go.GetComponent<EnemyAI>().enabled = false;
                    var nd = go.GetComponent<NhanDangQuai>(); if (nd == null) nd = go.AddComponent<NhanDangQuai>();
                    nd.id = id;
                    go.GetComponent<Damageable>().Die();
                    kieu.Add(go.GetComponent<XacNam>().TuThe);
                    Object.Destroy(go);
                }
                if (kieu[0] == kieu[1]) giongMang++;
            }
            Ghi(string.Format("T. cung so hieu mang (6 so, moi so hai con) -> cung tu the {0}/6", giongMang));
            Kiem(giongMang == 6, "cung mot con quai ma hai may ra hai tu the khac nhau");
            yield return null;

            var cacKieu = new[] { XacNam.KieuNam.Sap, XacNam.KieuNam.NghiengTrai, XacNam.KieuNam.NghiengPhai };
            foreach (var kieu in cacKieu)
            {
                XacNam.EpTuThe = kieu;
                var nhom = new List<MotXac>();
                for (int i = 0; i < loai.Length; i++)
                {
                    Vector3 p = goc + huong * 9f + ngang * ((i - 1.5f) * 4.5f);
                    p.y = DatY(p);
                    var go = EnemyFactory.Spawn(loai[i], p, null, toi.transform);
                    if (go == null) continue;
                    go.GetComponent<EnemyAI>().enabled = false;
                    nhom.Add(new MotXac { ten = loai[i].ToString(), d = go.GetComponent<Damageable>(), go = go, datY = p.y });
                }
                yield return new WaitForSeconds(0.8f);
                foreach (var x in nhom) { x.dauDung = CaoXuong(x.go, "Head", x.datY); x.d.GhiKeDanh(mauToi); x.d.Die(); }
                XacNam.EpTuThe = null;
                yield return new WaitForSeconds(3.2f);
                if (rig != null) rig.enabled = false;
                var camT = Camera.main;
                Vector3 giuaT = goc + huong * 9f;
                camT.transform.position = giuaT - huong * 6.5f + Vector3.up * 6f;
                camT.transform.rotation = Quaternion.LookRotation(giuaT + Vector3.up * 0.3f - camT.transform.position);
                yield return null;
                yield return Chup("xacnam_0_" + kieu);
                foreach (var x in nhom)
                {
                    if (x.go == null || (x.ten != "Witch" && x.ten != "QuyDu")) continue;
                    Vector3 t = x.go.transform.position;
                    camT.transform.position = t - huong * 2.4f + ngang * 1.6f + Vector3.up * 2.4f;
                    camT.transform.rotation = Quaternion.LookRotation(t + Vector3.up * 0.2f - camT.transform.position);
                    yield return null;
                    yield return Chup("xacnam_0_" + kieu + "_" + x.ten);
                }
                foreach (var x in nhom)
                {
                    var xn = x.go.GetComponent<XacNam>();
                    float matY = HuongMatY(x.go);
                    x.dauNam = CaoXuong(x.go, "Head", x.datY); x.hongNam = CaoXuong(x.go, "Hips", x.datY);
                    var dinh = DinhHinh(x.go);
                    int trong = 0, duoi = 0;
                    foreach (var v in dinh)
                    {
                        if (xn != null && xn.vungMau != null && new Vector2(v.x - xn.TamVungMau.x, v.z - xn.TamVungMau.z).magnitude <= xn.BanKinhVungMau) trong++;
                        if (v.y < x.datY + 0.01f) duoi++;
                    }
                    float tiTrong = dinh.Count > 0 ? trong / (float)dinh.Count : 0f, tiDuoi = dinh.Count > 0 ? duoi / (float)dinh.Count : 1f;
                    bool matDung = kieu == XacNam.KieuNam.Sap ? matY < -0.6f : Mathf.Abs(matY) < 0.5f;
                    Ghi(string.Format("T. {0} {1}: tu the that {2}; huong mat y {3:F2} ({4}); xuong Head {5:F2} -> {6:F2} m (Hips {7:F2}); nang chong lun {8:F2} (xuong {11}, them theo luoi {13:F2}; xuong than thap nhat {12:F2} m tren dat); {9:P0} dinh trong vung mau; {10:P1} dinh duoi dat",
                        kieu, x.ten, xn != null ? xn.TuThe.ToString() : "?", matY, kieu == XacNam.KieuNam.Sap ? "mong < -0,6: up xuong" : "mong |y| < 0,5: nhin ngang",
                        x.dauDung, x.dauNam, x.hongNam, xn != null ? xn.NangChongLun : -1f, tiTrong, tiDuoi, xn != null ? xn.XuongChong : "?", ThanThapNhat(x.go, x.datY), xn != null ? xn.NangTheoLuoi : -1f));
                    Kiem(xn != null && xn.TuThe == kieu, kieu + " " + x.ten + ": khong nam dung tu the ep");
                    Kiem(matDung, kieu + " " + x.ten + ": huong mat sai tu the (xuong Head -> headfront)");
                    // Nam nghieng: dau cao bang NUA BE VAI (quy cay vai rong: 0,60 m) - nguong theo ti le luc dung, khong co dinh
                    Kiem(x.dauNam >= 0f && x.dauNam < x.dauDung * 0.5f && Mathf.Abs(x.dauNam - x.hongNam) < 0.4f, kieu + " " + x.ten + ": xac khong nam han");
                    Kiem(xn != null && xn.vungMau != null && tiTrong >= 0.45f, kieu + " " + x.ten + ": xac khong nam de len vung mau");
                    // THAN khong duoc lun (xuong hong / song lung / co / dau tren mat dat); tay chan lun chut thi chap nhan - trong
                    // nhu co duoi nguoi. So dinh duoi dat chi chan truong hop lun ca xac.
                    Kiem(ThanThapNhat(x.go, x.datY) > -0.03f && tiDuoi < 0.2f, kieu + " " + x.ten + ": xac chui xuong dat");
                }
                foreach (var x in nhom) if (x.go != null) Object.Destroy(x.go);
                if (rig != null) rig.enabled = true;
                yield return new WaitForSeconds(0.3f);
            }
        }

        // ================= A. BON LOAI QUAI (NAM NGUA) =================
        Ghi("");
        XacNam.EpTuThe = XacNam.KieuNam.Ngua;
        var xac = new List<MotXac>();
        for (int i = 0; i < loai.Length; i++)
        {
            Vector3 p = goc + huong * 9f + ngang * ((i - 1.5f) * 4.5f);
            p.y = DatY(p);
            var go = EnemyFactory.Spawn(loai[i], p, null, toi.transform);
            if (go == null) { Ghi("[LOI] khong sinh duoc " + loai[i]); loi++; continue; }
            var ai = go.GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;     // dung yen, khong danh
            xac.Add(new MotXac { ten = loai[i].ToString(), d = go.GetComponent<Damageable>(), go = go, datY = p.y });
        }
        yield return new WaitForSeconds(0.8f);
        foreach (var x in xac) { x.caoDung = DinhTren(x.go, x.datY); x.dauDung = CaoXuong(x.go, "Head", x.datY); }
        foreach (var x in xac)
        {
            x.d.GhiKeDanh(mauToi);
            x.d.TakeDamage(x.d.health + 1000f, DamageType.Physical, x.go.transform.position);
            if (!x.d.IsDead) x.d.Die();
            x.lucChet = Time.time;
        }
        XacNam.EpTuThe = null;
        // Theo doi moi khung toi 3 giay: luc co mau, do loang som / muon
        // Cua so do: toi khi MOI con da co mau du 2,6 s (Editor giat khung thi mau co cham - lan chay 0,84 s thay vi 0,56)
        for (float h = Time.time + 6f; Time.time < h; )
        {
            foreach (var x in xac)
            {
                var xn = x.go != null ? x.go.GetComponent<XacNam>() : null;
                if (xn == null || xn.vungMau == null) continue;
                if (x.lucCoMau < 0f) x.lucCoMau = Time.time;
                float t = Time.time - x.lucCoMau;
                if (t >= 0.25f && x.loangSom < 0f) x.loangSom = xn.Loang01;
                if (t >= XacNam.GiayLoangMau + 0.1f && x.loangMuon < 0f) x.loangMuon = xn.Loang01;
            }
            bool du = true; foreach (var x in xac) if (x.loangMuon < 0f) du = false;
            if (du) break;
            yield return null;
        }
        foreach (var x in xac)
        {
            var xn = x.go.GetComponent<XacNam>();
            x.caoNam = DinhTren(x.go, x.datY);
            x.dauNam = CaoXuong(x.go, "Head", x.datY); x.hongNam = CaoXuong(x.go, "Hips", x.datY);
            var dinh = DinhHinh(x.go);
            // Dang nam: be DAI theo truc than (huong truoc cua goc - dau nga ve phia sau) lon hon han be CAO
            Vector3 f = x.go.transform.forward; f.y = 0f; f.Normalize();
            float fMin = 1e9f, fMax = -1e9f, yMin = 1e9f, yMax = -1e9f;
            foreach (var v in dinh) { float t = Vector3.Dot(v, f); fMin = Mathf.Min(fMin, t); fMax = Mathf.Max(fMax, t); yMin = Mathf.Min(yMin, v.y); yMax = Mathf.Max(yMax, v.y); }
            x.daiNam = fMax - fMin; x.dayNam = yMax - yMin;
            if (xn == null || xn.vungMau == null) continue;
            int trong = 0, duoi = 0; Vector3 tb = Vector3.zero;
            float poolY = xn.TamVungMau.y;
            foreach (var v in dinh)
            {
                Vector2 d = new Vector2(v.x - xn.TamVungMau.x, v.z - xn.TamVungMau.z);
                if (d.magnitude <= xn.BanKinhVungMau) trong++;
                if (v.y < poolY - 0.02f) duoi++;
                tb += v;
            }
            if (dinh.Count > 0) tb /= dinh.Count;
            x.trongMau = dinh.Count > 0 ? trong / (float)dinh.Count : 0f;
            x.duoiDat = dinh.Count > 0 ? duoi / (float)dinh.Count : 1f;
            x.kcTam = new Vector2(tb.x - xn.TamVungMau.x, tb.z - xn.TamVungMau.z).magnitude;
            int q = 0;
            foreach (var r in x.go.GetComponentsInChildren<Renderer>())
                if (!(r is ParticleSystemRenderer) && r.sharedMaterial != null) q = Mathf.Max(q, r.sharedMaterial.renderQueue);
            x.queueXac = q;
        }
        // Anh: dat may quay nhin xuong cum xac
        if (rig != null) rig.enabled = false;
        var cam = Camera.main;
        Vector3 camCu = cam.transform.position; Quaternion camRotCu = cam.transform.rotation;
        Vector3 giua = goc + huong * 9f;
        cam.transform.position = giua - huong * 7f + Vector3.up * 7.5f;
        cam.transform.rotation = Quaternion.LookRotation(giua + Vector3.up * 0.3f - cam.transform.position);
        yield return null;
        yield return Chup("xacnam_1_bon_loai_quai");
        cam.transform.position = giua + ngang * 2.3f - huong * 3.2f + Vector3.up * 2.6f;
        cam.transform.rotation = Quaternion.LookRotation(giua + ngang * 2.3f + huong * 0.3f - cam.transform.position);
        yield return null;
        yield return Chup("xacnam_2_can");
        foreach (var x in xac)
        {
            if (x.go == null) continue;
            Vector3 t = x.go.transform.position;
            cam.transform.position = t - huong * 2.6f + ngang * 1.4f + Vector3.up * 2.2f;
            cam.transform.rotation = Quaternion.LookRotation(t + Vector3.up * 0.2f - cam.transform.position);
            yield return null;
            yield return Chup("xacnam_2_can_" + x.ten);
        }

        foreach (var x in xac)
        {
            var xn = x.go.GetComponent<XacNam>();
            int qMau = xn != null && xn.vungMau != null ? xn.vungMau.GetComponent<Renderer>().sharedMaterial.renderQueue : -1;
            Ghi(string.Format("A. {0}: nga {19}, xuong Head cao {20:F2} m luc dung -> {21:F2} m luc nam (Hips {22:F2} m), nam dai {15:F2} m x day {16:F2} m, nang chong lun {17:F2} m (xuong {23}), cao than do tu xuong {18:F2} m; dinh hinh tren dat luc dung {1:F2} m -> luc nam {2:F2} m (x{3:F2}); vung mau {4} (ban kinh {5:F2} m, co sau {6:F2} s tu luc chet), loang 0,25 s {7:F2} -> {8:F2} s {9:F2}; {10:P0} dinh xac nam TRONG vung mau, tam xac cach tam mau {11:F2} m; {12:P1} dinh chui duoi mat mau; hang doi mau {13} > xac {14}",
                x.ten, x.caoDung, x.caoNam, x.caoNam / Mathf.Max(0.01f, x.caoDung), xn != null && xn.vungMau != null,
                xn != null ? xn.BanKinhVungMau : -1f, x.lucCoMau >= 0f ? x.lucCoMau - x.lucChet : -1f, x.loangSom,
                XacNam.GiayLoangMau + 0.1f, x.loangMuon, x.trongMau, x.kcTam, x.duoiDat, qMau, x.queueXac,
                x.daiNam, x.dayNam, xn != null ? xn.NangChongLun : -1f, xn != null ? xn.caoThan : -1f,
                "NGUA", x.dauDung, x.dauNam, x.hongNam, xn != null ? xn.XuongChong : "?"));
            Kiem(xn != null, x.ten + ": chet ma khong co XacNam");
            float matNgua = HuongMatY(x.go);
            Ghi(string.Format("A. {0}: huong mat y {1:F2} (NGUA - mong > 0,6: mat ngua len troi)", x.ten, matNgua));
            Kiem(matNgua > 0.6f, x.ten + ": nam NGUA ma mat khong huong len troi");
            // NAM = xuong dau ha xuong sat dat va ngang hong (than nam ngang). Khong do bang be cao ca hinh: Quy cay co
            // cap sung lon + tay gio len khi nam ngua nen hinh van cao 1,31 m du than da nam han (anh xacnam_1).
            Kiem(x.dauDung > 1.0f && x.dauNam >= 0f && x.dauNam < x.dauDung * 0.5f && Mathf.Abs(x.dauNam - x.hongNam) < 0.4f && x.caoNam < x.caoDung * 0.8f,
                 x.ten + ": xac khong nam han xuong dat");
            Kiem(xn != null && xn.vungMau != null && x.lucCoMau - x.lucChet < 1.5f, x.ten + ": khong co vung mau / co qua cham");
            Kiem(x.loangSom > 0f && x.loangSom < 0.8f && x.loangMuon > 0.99f, x.ten + ": vung mau khong loang tu nho ra du");
            Kiem(x.trongMau >= 0.5f && x.kcTam < (xn != null ? xn.BanKinhVungMau : 0f), x.ten + ": xac khong nam de len vung mau");
            Kiem(ThanThapNhat(x.go, x.datY) > -0.03f && x.duoiDat < 0.2f, x.ten + ": xac chui xuong duoi mat dat");
            Ghi(string.Format("A. {0}: xuong than thap nhat {1:F2} m tren dat", x.ten, ThanThapNhat(x.go, x.datY)));
            Kiem(qMau >= 3000 && x.queueXac < qMau, x.ten + ": vung mau khong ve sau xac (xac khong de len mau)");
        }

        // ---- B. chet luc dang bi danh nga / dang bi hat tung ----
        Ghi("");
        {
            Vector3 p1 = goc - huong * 8f + ngang * 3f; p1.y = DatY(p1);
            Vector3 p2 = goc - huong * 8f - ngang * 3f; p2.y = DatY(p2);
            var g1 = EnemyFactory.Spawn(MonsterType.Skeleton, p1, null, toi.transform);
            var g2 = EnemyFactory.Spawn(MonsterType.Skeleton, p2, null, toi.transform);
            g1.GetComponent<EnemyAI>().enabled = false; g2.GetComponent<EnemyAI>().enabled = false;
            var d1 = g1.GetComponent<Damageable>(); var d2 = g2.GetComponent<Damageable>();
            yield return new WaitForSeconds(0.5f);
            BiDanhNga.Apply(d1, 1.5f);
            BiHatTung.Apply(d2, 0.5f);
            yield return new WaitForSeconds(0.15f);
            float lucHat = Time.time - 0.15f;
            d1.Die(); d2.Die();
            float lucMau2 = -1f;
            for (float h = Time.time + 2.2f; Time.time < h; )
            {
                var x2 = g2.GetComponent<XacNam>();
                if (lucMau2 < 0f && x2 != null && x2.vungMau != null) lucMau2 = Time.time;
                yield return null;
            }
            float cao1 = DinhTren(g1, p1.y), cao2 = DinhTren(g2, p2.y);
            float dau1 = CaoXuong(g1, "Head", p1.y), dau2 = CaoXuong(g2, "Head", p2.y), hong2 = CaoXuong(g2, "Hips", p2.y);
            bool conNga = g1.GetComponent<BiDanhNga>() != null;
            var xn2 = g2.GetComponent<XacNam>();
            Ghi(string.Format("B. chet luc dang NGA: 1,85 s sau khi bi nga (qua moc chong day 1,5 s) dinh hinh {0:F2} m, con BiDanhNga {1}, co mau {2}", cao1, conNga, g1.GetComponent<XacNam>() != null && g1.GetComponent<XacNam>().vungMau != null));
            Ghi(string.Format("B. chet luc dang BAY (hat tung 0,5 s): mau loang sau khi bi hat {0:F2} s (phai >= 0,5 - cho roi xuong), dinh hinh cuoi {1:F2} m, tu the {2}, xuong Head {3:F2} m (Hips {4:F2}); con bi nga: tu the {5}, Head {6:F2} m",
                lucMau2 >= 0f ? lucMau2 - lucHat : -1f, cao2, xn2 != null ? xn2.TuThe.ToString() : "?", dau2, hong2,
                g1.GetComponent<XacNam>() != null ? g1.GetComponent<XacNam>().TuThe.ToString() : "?", dau1));
            Kiem(!conNga && cao1 < 1.0f && g1.GetComponent<XacNam>().TuThe == XacNam.KieuNam.Ngua, "chet luc dang nga: xac chong day / con BiDanhNga / khong giu nam ngua");
            Kiem(lucMau2 - lucHat >= 0.5f && dau2 >= 0f && dau2 < 0.7f && Mathf.Abs(dau2 - hong2) < 0.4f && xn2 != null && xn2.vungMau != null, "chet luc dang bay: mau loang giua khong trung / xac khong nam");
        }

        // ---- D. DAU CONG (lam trong luc cho moc 15 / 21 / 23 giay cua xac) ----
        Ghi("");
        {
            var oVCu = (int[])SachPhep.OVuong.Clone(); var oTCu = (int[])SachPhep.OTron.Clone();
            int[] boThu = { 0, CapDo.KyQuaCauBang, CapDo.KyBinhMana, CapDo.KyBinhMau, CapDo.KyGioLoc, 5, 6 };
            for (int o = 0; o < 7; o++) { SachPhep.OVuong[o] = boThu[o]; SachPhep.OTron[o] = boThu[o]; }
            CapDo.BatDauTranMoi();     // cap 1, 1 diem, binh co san cap 1
            bool c0 = GameHUD.CoDauCong(0), cTT = GameHUD.CoDauCong(4), cBinh = GameHUD.CoDauCong(CapDo.KyBinhMau), cTrong = GameHUD.CoDauCong(SachPhep.Trong);
            Ghi(string.Format("D. dau tran ({0} diem): Cau lua (khoa, khong can bac) {1}; Thien thach (can Cau lua cap 2) {2}; Binh mau (cap 1/3) {3}; o trong {4}",
                CapDo.DiemKyNang, c0, cTT, cBinh, cTrong));
            Kiem(c0 && !cTT && cBinh && !cTrong, "dau cong hien sai theo khoa / bac / binh");
            bool bam = GameHUD.BamDauCong(0);
            bool het = !GameHUD.CoDauCong(0) && !GameHUD.CoDauCong(CapDo.KyBinhMau) && !GameHUD.CoDauCong(CapDo.KyQuaCauBang);
            Ghi(string.Format("D. bam + Cau lua -> {0}, da mo {1}, cap {2}, con {3} diem; het diem -> khong o nao con dau cong {4}", bam, CapDo.DaMo(0), CapDo.CapCuaKyNang(0), CapDo.DiemKyNang, het));
            Kiem(bam && CapDo.CapCuaKyNang(0) == 1 && CapDo.DiemKyNang == 0 && het, "bam dau cong khong mo khoa / het diem van hien");
            CapDo.ThemDiemChoPhepThu(20);
            GameHUD.BamDauCong(0);
            bool ttSauBac = GameHUD.CoDauCong(4);
            for (int i = 0; i < 6; i++) GameHUD.BamDauCong(0);
            for (int i = 0; i < 4; i++) GameHUD.BamDauCong(CapDo.KyBinhMau);
            Ghi(string.Format("D. Cau lua cap 2 -> Thien thach hien dau cong {0}; bam tiep -> Cau lua cap {1} (toi da 5) dau cong {2}; Binh mau cap {3} (toi da 3) dau cong {4}",
                ttSauBac, CapDo.CapCuaKyNang(0), GameHUD.CoDauCong(0), CapDo.CapCuaKyNang(CapDo.KyBinhMau), GameHUD.CoDauCong(CapDo.KyBinhMau)));
            Kiem(ttSauBac && CapDo.CapCuaKyNang(0) == 5 && !GameHUD.CoDauCong(0) && CapDo.CapCuaKyNang(CapDo.KyBinhMau) == 3 && !GameHUD.CoDauCong(CapDo.KyBinhMau),
                 "dau cong khong theo bac / van hien khi da toi da");

            // Hinh hoc: dat lai de moi o deu co dau cong
            CapDo.BatDauTranMoi(); CapDo.ThemDiemChoPhepThu(30);
            int soCo = 0; for (int o = 0; o < 7; o++) if (GameHUD.CoDauCong(boThu[o])) soCo++;
            float s = Screen.height / 1080f;
            float r = hud.BanKinhNut(s), rb = r * GameHUD.DauCongTronBanKinh;
            float heHinh = 999f, heBam = 999f, traiMan = 999f; int dungO = 0, giuaNutLaTung = 0;
            for (int o = 0; o < 7; o++)
            {
                Vector2 tc = hud.TamDauCongTron(o, s);
                for (int j = 0; j < 7; j++)
                {
                    if (j == o) continue;
                    float kc = Vector2.Distance(tc, hud.TamNut(j, s));
                    heHinh = Mathf.Min(heHinh, (kc - r - rb) / s);
                    heBam = Mathf.Min(heBam, (kc - r - rb * 1.2f) / s);
                }
                traiMan = Mathf.Min(traiMan, Mathf.Min(Screen.height - (tc.y + rb), Screen.width - (tc.x + rb)) / s);
                if (hud.DauCongTronTaiDiem(tc, s) == o) dungO++;
                if (hud.DauCongTronTaiDiem(hud.TamNut(o, s), s) == -1) giuaNutLaTung++;
            }
            // O vuong: huy hieu khong cat o khac, nam gon trong be ngang o minh
            float heVuong = 999f; int trongO = 0, chuotDung = 0;
            for (int o = 0; o < 7; o++)
            {
                Rect b = GameHUD.VungDauCongVuong(o, s), me = GameHUD.RectOVuong(o, s);
                for (int j = 0; j < 7; j++)
                {
                    if (j == o) continue;
                    Rect k = GameHUD.RectOVuong(j, s);
                    float he = Mathf.Max(k.xMin - b.xMax, b.xMin - k.xMax);
                    heVuong = Mathf.Min(heVuong, he / s);
                }
                if (b.xMin >= me.xMin && b.xMax <= me.xMax) trongO++;
                Vector2 c = new Vector2(b.center.x, Screen.height - b.center.y);
                Vector2 giuaO = new Vector2(me.center.x, Screen.height - me.center.y);
                if (GameHUD.ConTroTrenDauCongVuong(c) && !GameHUD.ConTroTrenDauCongVuong(giuaO)) chuotDung++;
            }
            Ghi(string.Format("D. nut TRON ({0}/7 o co dau cong): khe hep nhat tu huy hieu toi nut KHAC {1:F1} (vung bam x1,2: {2:F1}) don vi 1080; cach mep man {3:F1}; cham giua huy hieu -> dung o {4}/7; cham giua nut -> van la tung ky nang {5}/7",
                soCo, heHinh, heBam, traiMan, dungO, giuaNutLaTung));
            Ghi(string.Format("D. o VUONG: huy hieu {0:F1} x {0:F1}, khe toi o khac {1:F1}; nam trong be ngang o minh {2}/7; chuot tren huy hieu duoc chan (khong keo nhan vat) va giua o thi khong {3}/7",
                GameHUD.RectOVuong(0, s).width * GameHUD.DauCongVuongCo / s, heVuong, trongO, chuotDung));
            Kiem(soCo == 7, "doi chung hong: khong phai o nao cung co dau cong");
            Kiem(heHinh > 0f && heBam > 0f && traiMan > 0f, "dau cong nut tron cham / de len nut khac hoac tran man");
            Kiem(dungO == 7 && giuaNutLaTung == 7, "vung bam dau cong nut tron sai");
            Kiem(heVuong > 0f && trongO == 7 && chuotDung == 7, "dau cong o vuong de len o khac / khong chan chuot");

            // Anh: ban may tinh roi ban cam ung
            if (rig != null) rig.enabled = true;
            cam.transform.position = camCu; cam.transform.rotation = camRotCu;
            hud.epCamUng = false;
            yield return new WaitForSeconds(0.3f);
            yield return Chup("xacnam_3_dau_cong_may_tinh");
            hud.epCamUng = true;
            yield return new WaitForSeconds(0.3f);
            yield return Chup("xacnam_4_dau_cong_cam_ung");
            hud.epCamUng = false;
            for (int o = 0; o < 7; o++) { SachPhep.OVuong[o] = oVCu[o]; SachPhep.OTron[o] = oTCu[o]; }
        }

        // ---- A (tiep). moc thoi gian cua xac quai ----
        float lucChet0 = xac.Count > 0 ? xac[0].lucChet : Time.time;
        while (Time.time < lucChet0 + 15f) yield return null;
        foreach (var x in xac) x.con15 = x.go != null;
        while (Time.time < lucChet0 + 21f) yield return null;
        var mauTruoc = new List<GameObject>();
        foreach (var x in xac)
        {
            var xn = x.go != null ? x.go.GetComponent<XacNam>() : null;
            x.chim21 = xn != null && xn.DangChim && x.go.transform.position.y < x.datY - 0.1f;
            mauTruoc.Add(xn != null ? xn.vungMau : null);
        }
        while (Time.time < lucChet0 + 23f) yield return null;
        for (int i = 0; i < xac.Count; i++) { xac[i].xoa23 = xac[i].go == null; xac[i].mauXoa23 = mauTruoc[i] == null; }
        var sb = new StringBuilder();
        foreach (var x in xac) sb.Append(string.Format("{0}: 15 s con {1}, 21 s dang chim {2}, 23 s xoa {3} (mau xoa {4}); ", x.ten, x.con15, x.chim21, x.xoa23, x.mauXoa23));
        Ghi("A. moc thoi gian (ban cu xac mat sau 5-6 giay): " + sb);
        foreach (var x in xac) Kiem(x.con15 && x.chim21 && x.xoa23 && x.mauXoa23, x.ten + ": xac khong nam 20 giay / khong chim / khong xoa ca vung mau");

        // ================= C. NGUOI CHOI CHET =================
        Ghi("");
        {
            float datToi = DatY(toi.transform.position);
            float caoDung = DinhTren(toi.gameObject, datToi);
            float dauDungToi = CaoXuong(toi.gameObject, "Head", datToi);
            mauToi.Die();
            yield return new WaitForSeconds(3f);
            var xn = toi.GetComponent<XacNam>();
            float caoNam = DinhTren(toi.gameObject, datToi);
            float dauNamToi = CaoXuong(toi.gameObject, "Head", datToi);
            bool coMau = xn != null && xn.vungMau != null;
            if (rig != null) rig.enabled = false;
            cam.transform.position = toi.transform.position - huong * 4f + ngang * 1.5f + Vector3.up * 3.2f;
            cam.transform.rotation = Quaternion.LookRotation(toi.transform.position - huong * 0.7f - cam.transform.position);
            yield return null;
            yield return Chup("xacnam_5_nguoi_choi");
            float y0 = toi.transform.position.y;
            yield return new WaitForSeconds(21f);
            bool van = toi != null && toi.GetComponent<XacNam>() != null && !toi.GetComponent<XacNam>().DangChim
                       && Mathf.Abs(toi.transform.position.y - y0) < 0.05f && xn.vungMau != null;
            Ghi(string.Format("C. nguoi choi chet: la xac nguoi choi {0}; dinh hinh {1:F2} m -> {2:F2} m; vung mau {3}; 24 giay sau van nam nguyen cho, khong chim {4}",
                xn != null && xn.laNguoiChoi, caoDung, caoNam, coMau, van));
            Ghi(string.Format("C. tu the (ngau nhien) {0}; xuong Head {1:F2} -> {2:F2} m", xn != null ? xn.TuThe.ToString() : "?", dauDungToi, dauNamToi));
            Kiem(xn != null && xn.laNguoiChoi && dauDungToi > 1f && dauNamToi < dauDungToi * 0.5f && coMau, "nguoi choi chet khong nam xuong vung mau");
            Kiem(van, "xac nguoi choi chim / mat (phai nam toi het tran)");
        }

        foreach (var d0 in Object.FindObjectsByType<Damageable>(FindObjectsInactive.Exclude)) if (d0.name.StartsWith("TAM_")) Object.Destroy(d0.gameObject);
        Ghi("");
        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void TraLaiCanh()
    {
        if (EditorApplication.isPlaying) return;
        EditorApplication.update -= TraLaiCanh;
        if (!string.IsNullOrEmpty(canhCu)) EditorSceneManager.OpenScene(canhCu, OpenSceneMode.Single);
    }

    static void Ket()
    {
        XacNam.EpTuThe = null;
        File.WriteAllText("PlayTestShots/xacnam.txt", bao.ToString());
        var rac = GameObject.Find("TAM_XacNam");
        if (rac != null) Object.DestroyImmediate(rac);
        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBat;
        EditorSettings.enterPlayModeOptions = truocOpt;
        EditorApplication.isPlaying = false;
        EditorApplication.update += TraLaiCanh;
    }
}
