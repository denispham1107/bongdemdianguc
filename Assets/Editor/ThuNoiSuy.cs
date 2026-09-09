using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// CHAY THU BUOC 4: THAY NHAU DI CHUYEN MUOT.
///
/// Nam phep do:
///
///  1. GOI TIN gon va doc lai dung - do bang byte that, so voi JSON.
///  2. MANG LY TUONG: goi den deu dan -> nhan vat bam sat duong di that.
///  3. MANG CHAP CHON: bom dung so do that o buoc 0 (jitter 53,7 ms, p95
///     72,6 ms) -> van muot. Day la phep do co suc nang nhat: dem co dinh se
///     truot o day.
///  4. MAT GOI: bo bot goi -> khong duoc nhay coc.
///  5. DEM CO GIAN: mang xau thi dem phai DAY LEN, mang tot thi MONG LAI.
///
/// Do "muot" bang cach nao: dem so lan nhan vat NHAY - mot khung hinh ma dich
/// chuyen xa hon nhieu lan so voi buoc di binh thuong. Mat nguoi thay giat
/// chinh la thay nhung cu nhay do.
///
/// Ket qua ghi ra <c>PlayTestShots/noi_suy.txt</c>.
/// </summary>
public static class ThuNoiSuy
{
    static readonly StringBuilder bao = new StringBuilder();
    static int loi;
    static bool daBatDau;
    static bool truocBatPlayMode;
    static EnterPlayModeOptions truocPlayMode;

    [MenuItem("Diablo 2.5D/33. Chay thu NOI SUY (buoc 4)", false, 120)]
    public static void Chay()
    {
        Directory.CreateDirectory("PlayTestShots");

        truocBatPlayMode = EditorSettings.enterPlayModeOptionsEnabled;
        truocPlayMode = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        if (EditorSceneManager.GetActiveScene().name != "MainMenu")
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        bao.Length = 0; loi = 0; daBatDau = false;
        EditorApplication.update += Nhip;
        EditorApplication.EnterPlaymode();
    }

    static void Nhip()
    {
        if (!EditorApplication.isPlaying) return;
        if (daBatDau) return;
        daBatDau = true;
        var go = new GameObject("TAM_NoiSuy");
        go.AddComponent<ChayThuMang>().batDau = ChayKichBan();
    }

    static void Ghi(string s)
    {
        bao.AppendLine(s);
        Debug.Log("[NoiSuy] " + s);
    }

    /// <summary>Duong di that cua nguoi kia: chay vong tron deu.</summary>
    static Vector3 DuongDiThat(float giay)
    {
        float toc = 5.2f, banKinh = 6f;
        float goc = giay * toc / banKinh;
        return new Vector3(Mathf.Cos(goc) * banKinh, 0f, Mathf.Sin(goc) * banKinh);
    }

    /// <summary>
    /// Chay mot lan mo phong: bom moc theo nhip, do do muot va do bam sat.
    ///
    /// <paramref name="jitterMs"/> lam gio DEN cua goi xe dich - dung nhu mang
    /// that. <paramref name="tiLeMat"/> vut bot goi.
    /// </summary>
    static IEnumerator MoPhong(string ten, float jitterMs, float tiLeMat,
                               System.Action<int, float, float, float> xong,
                               float epDemGiay = 0f)
    {
        var ns = new NoiSuy();
        ns.demToiDaEp = epDemGiay;
        var ngau = new System.Random(777);

        const float GIAY_CHAY = 6f;
        int NHIP_GUI = DongBoTran.NhipGui;

        float batDau = Time.unscaledTime;
        float guiLanSau = 0f;
        int mocKe = 0;

        Vector3 truocDo = Vector3.zero;
        bool coTruoc = false;
        int soNhay = 0;
        float tongLech = 0f;
        int soMauLech = 0;
        float demCuoi = 0f;

        while (Time.unscaledTime - batDau < GIAY_CHAY)
        {
            float troi = Time.unscaledTime - batDau;

            // ---- Gui moc theo nhip, kem jitter va mat goi ----
            if (troi >= guiLanSau)
            {
                guiLanSau += 1f / NHIP_GUI;
                mocKe++;

                bool mat = tiLeMat > 0f && ngau.NextDouble() < tiLeMat;
                if (!mat)
                {
                    // Goi chup luc "troi", nhung DEN muon mot chut - do chinh
                    // la jitter that ngoai doi
                    float muon = jitterMs <= 0f ? 0f
                               : (float)(ngau.NextDouble() * jitterMs / 1000f);

                    int mocMs = Mathf.RoundToInt(troi * 1000f);
                    var viTri = DuongDiThat(troi);

                    if (muon > 0f)
                    {
                        // Cho den luc no "toi noi" moi nap vao
                        float toiLuc = Time.unscaledTime + muon;
                        while (Time.unscaledTime < toiLuc) yield return null;
                    }
                    ns.Nhan(mocMs, viTri, 0f, true);
                }
            }

            ns.CapNhat(Time.unscaledDeltaTime);
            demCuoi = ns.DemGiay;

            if (ns.SanSang)
            {
                Vector3 gio = ns.ViTriHienThi;

                if (coTruoc)
                {
                    // Buoc di binh thuong trong mot khung hinh, o toc do 5,2 m/s
                    float buocThuong = 5.2f * Time.unscaledDeltaTime;
                    float buocThat = Vector3.Distance(truocDo, gio);

                    // Nhay = dich xa hon ba lan buoc thuong trong mot khung
                    if (buocThat > buocThuong * 3f + 0.02f) soNhay++;
                }
                truocDo = gio; coTruoc = true;

                // Bam sat duong di that den dau - so voi vi tri that o thoi
                // diem da tru dem (do la thoi diem dang duoc hien)
                Vector3 that = DuongDiThat(troi - ns.DemGiay);
                tongLech += Vector3.Distance(gio, that);
                soMauLech++;
            }

            yield return null;
        }

        float lechTB = soMauLech > 0 ? tongLech / soMauLech : -1f;
        Ghi(string.Format("{0}: nhay {1} lan | lech trung binh {2:F3} m | "
                          + "dem {3:F0} ms | jitter uoc {4:F0} ms",
                          ten, soNhay, lechTB, demCuoi * 1000f, ns.JitterGiay * 1000f));

        xong(soNhay, lechTB, demCuoi, ns.JitterGiay);
    }

    static IEnumerator ChayKichBan()
    {
        Ghi("[ban 3] buoc 4 - do ca cai gia phai tra khi dem mong");

        // ---- 1. GOI TIN ----
        var ds = new GoiTin.MotNguoi[4];
        for (int i = 0; i < 4; i++)
            ds[i] = new GoiTin.MotNguoi
            {
                chiSo = (byte)i,
                viTri = new Vector3(i * 3.21f, 1.23f, -i * 2.5f),
                gocY = i * 90f + 12.5f,
                mau01 = 1f - i * 0.2f,
                dangChay = i % 2 == 0
            };

        byte[] goi = GoiTin.VietTrangThai(123456, ds, 4);
        var doc = new GoiTin.MotNguoi[8];
        int moc;
        int soNguoi = GoiTin.DocTrangThai(goi, doc, out moc);

        float lechViTri = 0f, lechGoc = 0f;
        for (int i = 0; i < 4 && i < soNguoi; i++)
        {
            lechViTri = Mathf.Max(lechViTri, Vector3.Distance(ds[i].viTri, doc[i].viTri));
            lechGoc = Mathf.Max(lechGoc, Mathf.Abs(Mathf.DeltaAngle(ds[i].gocY, doc[i].gocY)));
        }

        // JSON tuong duong de so sanh - viet tay dung nhung truong ay
        string json = "{\"t\":123456,\"n\":[";
        for (int i = 0; i < 4; i++)
            json += "{\"i\":" + i + ",\"x\":" + ds[i].viTri.x.ToString("F2")
                  + ",\"y\":" + ds[i].viTri.y.ToString("F2")
                  + ",\"z\":" + ds[i].viTri.z.ToString("F2")
                  + ",\"g\":" + ds[i].gocY.ToString("F1")
                  + ",\"m\":" + ds[i].mau01.ToString("F2")
                  + ",\"c\":" + (ds[i].dangChay ? 1 : 0) + "}" + (i < 3 ? "," : "");
        json += "]}";

        Ghi(string.Format("1. goi tin 4 nguoi: nhi phan {0} byte, JSON {1} byte "
                          + "(nho hon {2:F1} lan)", goi.Length, json.Length,
                          (float)json.Length / goi.Length));
        Ghi(string.Format("   doc lai: {0} nguoi, moc {1}, lech vi tri {2:F4} m, "
                          + "lech goc {3:F3} do", soNguoi, moc, lechViTri, lechGoc));

        if (soNguoi != 4) { Ghi("[LOI] doc lai khong ra du 4 nguoi"); loi++; }
        if (moc != 123456) { Ghi("[LOI] moc thoi gian sai"); loi++; }
        if (lechViTri > 0.02f) { Ghi("[LOI] nen vi tri sai qua 2 cm"); loi++; }
        if (lechGoc > 0.05f) { Ghi("[LOI] nen goc sai qua 0,05 do"); loi++; }
        if (goi.Length > 100) { Ghi("[LOI] goi qua to, khong con la nhi phan gon"); loi++; }

        // Goi hong phai bi tu choi chu khong duoc nem loi
        var hong = new byte[] { 1, 2, 3 };
        if (GoiTin.DocTrangThai(hong, doc, out moc) != -1)
        { Ghi("[LOI] goi cut ma van doc bua - se nem loi giua tran"); loi++; }
        else Ghi("   goi cut bi tu choi dung cach");

        // ---- 2. MANG LY TUONG ----
        int nhayDep = 0; float lechDep = 0f, demDep = 0f;
        yield return MoPhong("2. mang ly tuong (khong jitter, khong mat goi)",
                             0f, 0f, (n, l, d, j) => { nhayDep = n; lechDep = l; demDep = d; });

        if (nhayDep > 2) { Ghi("[LOI] mang ly tuong ma van nhay " + nhayDep + " lan"); loi++; }
        if (lechDep > 0.5f) { Ghi("[LOI] bam duong di that qua te: " + lechDep + " m"); loi++; }

        // ---- 3. MANG CHAP CHON (so do that o buoc 0) ----
        int nhayXau = 0; float lechXau = 0f, demXau = 0f, jitterXau = 0f;
        yield return MoPhong("3. mang 4G that (jitter 54 ms)",
                             54f, 0f, (n, l, d, j) => { nhayXau = n; lechXau = l; demXau = d; jitterXau = j; });

        if (nhayXau > 5)
        { Ghi("[LOI] mang chap chon lam nhan vat nhay " + nhayXau + " lan - dem chua du day"); loi++; }

        // ---- 4. MAT GOI ----
        int nhayMat = 0;
        yield return MoPhong("4. mat 10% goi", 30f, 0.10f, (n, l, d, j) => { nhayMat = n; });

        if (nhayMat > 8)
        { Ghi("[LOI] mat goi lam nhan vat nhay " + nhayMat + " lan"); loi++; }

        // ---- 5. DEM CO GIAN ----
        Ghi(string.Format("5. dem co gian: mang tot {0:F0} ms -> mang xau {1:F0} ms",
                          demDep * 1000f, demXau * 1000f));

        if (demXau <= demDep + 0.005f)
        { Ghi("[LOI] mang xau ma dem khong day len - dem khong co gian"); loi++; }

        // ---- 6. CAI GIA CUA DEM MONG ----
        // Dem day thi muot nhung tre. Dem mong thi nhanh nhung giat. Do ca
        // hai roi hay chon, dung doan.
        Ghi("");
        Ghi("--- Cai gia phai tra khi ep dem mong (van tren mang 4G that) ---");

        int nhay60 = 0; float lech60 = 0f, dem60 = 0f;
        yield return MoPhong("   ep dem 60 ms ", 54f, 0f,
                             (n, l, d, j) => { nhay60 = n; lech60 = l; dem60 = d; }, 0.06f);

        int nhay90 = 0; float dem90 = 0f;
        yield return MoPhong("   ep dem 90 ms ", 54f, 0f,
                             (n, l, d, j) => { nhay90 = n; dem90 = d; }, 0.09f);

        Ghi("");
        Ghi("So sanh tren mang 4G that (jitter 54 ms):");
        Ghi(string.Format("   dem tu co gian {0:F0} ms -> nhay {1} lan", demXau * 1000f, nhayXau));
        Ghi(string.Format("   ep 90 ms              -> nhay {0} lan", nhay90));
        Ghi(string.Format("   ep 60 ms              -> nhay {0} lan", nhay60));

        Ghi("so loi ghi nhan = " + loi);
        Ket();
    }

    static void Ket()
    {
        File.WriteAllText("PlayTestShots/noi_suy.txt", bao.ToString());

        var rac = GameObject.Find("TAM_NoiSuy");
        if (rac != null) Object.DestroyImmediate(rac);

        EditorApplication.update -= Nhip;
        EditorSettings.enterPlayModeOptionsEnabled = truocBatPlayMode;
        EditorSettings.enterPlayModeOptions = truocPlayMode;
        EditorApplication.isPlaying = false;
    }
}
