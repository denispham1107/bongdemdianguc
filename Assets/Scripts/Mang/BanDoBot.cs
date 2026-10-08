using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BAN DO DI LAI CUA MAY BOT (buoc 3, 08/10/2026): LUOI O DI DUOC + TIM DUONG A*.
///
/// Du an KHONG co NavMesh; quai chi lai thang roi vong khi ket (EnemyAI). Nguoi dung xin BOT "biet ne di vong cac chuong
/// ngai vat de khong bi mac ket, biet dung truoc vuc doi duong khac de khong bi rot xuong dia nguc" - nen luc vao tran,
/// MAY CHU PHONG (noi BOT chay) dung MOT LUOI cho ca ban do, dung chung cho moi BOT:
///   - moi o <see cref="O"/> m: co dat (tia xuong lop Ground), dat KHONG thap hon <see cref="DatVucDuoi"/> (day vuc dia nguc
///     cung la lop Ground, o -26,5), doc khong qua <see cref="DocToiDa"/> do, va con nhong nhan vat dat o day KHONG cham vat
///     can lop Default (bia, cay, nha mo, rao, lo lua) tu cao 0,6 m tro len (thap hon thi CharacterController buoc qua duoc);
///   - o sat vuc (trong <see cref="LeVuc"/> m) cung cam - dung cach mep, khong dung sat mep.
/// Dung rai qua nhieu khung (<see cref="OMoiKhung"/> o moi khung) de khong khung nao khung lai tren dien thoai.
///
/// Canh vat DOI giua tran (bia chay rui roi moc lai, Loc xoay cuon do vat sang cho khac) - luoi khong theo kip. Bu lai:
/// BOT tu danh dau o "cam tam" khi ket / thay vuc phia truoc (<see cref="CamTam"/>), o tu mo lai sau vai giay.
/// </summary>
public static class BanDoBot
{
    public const float O = 0.8f;
    public const float DatVucDuoi = DiaNguc.NguongRoi + 2f;     // -4 m: thap hon la vuc
    public const float DocToiDa = 50f;
    public const float BanKinhNguoi = 0.42f;                     // CharacterController 0,32 + chua le
    public const float CaoBuocQua = 0.6f;                        // stepOffset 0,55
    public const float CaoNguoi = 1.95f;
    public const float LeVuc = 1.6f;
    public const int OMoiKhung = 2500;

    public static bool SanSang { get; private set; }
    public static bool DangDung { get; private set; }
    public static int SoCanh { get; private set; }
    public static int SoODiDuoc { get; private set; }
    public static int SoOVuc { get; private set; }
    public static int SoOVatCan { get; private set; }
    public static float GiayDung { get; private set; }

    static int n;
    static Vector3 goc;
    static byte[] loai;            // 0 di duoc, 1 vat can, 2 vuc / khong co dat, 3 sat vuc
    static float[] camDen;         // o bi cam tam toi luc nay (Time.time)
    static UnityEngine.SceneManagement.Scene canhDungCho, canhDangDung;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void DatLaiKhiVaoPlay() { SanSang = false; DangDung = false; canhDungCho = default(UnityEngine.SceneManagement.Scene); canhDangDung = canhDungCho; }

    const byte DiDuocO = 0, VatCan = 1, Vuc = 2, SatVuc = 3;

    public static int MatNaDat { get { return LayerMask.GetMask("Ground"); } }
    public static int MatNaVatCan { get { return LayerMask.GetMask("Default"); } }

    /// <summary>Luoi da dung cho CANH NAY chua (moi lan nap Act2 la canh moi - dung lai).</summary>
    public static bool HopLeChoCanh { get { return SanSang && canhDungCho == UnityEngine.SceneManagement.SceneManager.GetActiveScene(); } }

    /// <summary>Dung luoi quanh <paramref name="tam"/>, canh nua <paramref name="nuaCanh"/> m - chay nhu coroutine.</summary>
    public static IEnumerator Dung(Vector3 tam, float nuaCanh)
    {
        // Dang dung DO cho chinh canh nay thi thoi; con lan dung do dang tu canh cu (coroutine chet theo canh, du an tat
        // Domain Reload nen co tinh con giu) thi dung lai tu dau
        var canh = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (DangDung && canhDangDung == canh) yield break;
        canhDangDung = canh;
        DangDung = true; SanSang = false;
        float t0 = Time.realtimeSinceStartup;
        n = Mathf.CeilToInt(nuaCanh * 2f / O);
        goc = new Vector3(tam.x - nuaCanh, 0f, tam.z - nuaCanh);
        loai = new byte[n * n];
        camDen = new float[n * n];
        int matDat = MatNaDat, matVat = MatNaVatCan;
        float cosDoc = Mathf.Cos(DocToiDa * Mathf.Deg2Rad);

        int dem = 0;
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
            {
                Vector3 p = TamO(x, z);
                RaycastHit hit;
                byte l;
                if (!Physics.Raycast(new Vector3(p.x, 80f, p.z), Vector3.down, out hit, 160f, matDat, QueryTriggerInteraction.Ignore)
                    || hit.point.y < DatVucDuoi)
                    l = Vuc;
                else if (hit.normal.y < cosDoc)
                    l = VatCan;
                else
                {
                    float y = hit.point.y;
                    bool vuong = Physics.CheckCapsule(new Vector3(p.x, y + CaoBuocQua + BanKinhNguoi, p.z),
                                                      new Vector3(p.x, y + CaoNguoi - BanKinhNguoi, p.z),
                                                      BanKinhNguoi, matVat, QueryTriggerInteraction.Ignore);
                    l = vuong ? VatCan : DiDuocO;
                }
                loai[z * n + x] = l;
                if (++dem % OMoiKhung == 0) yield return null;
            }

        // O sat vuc: cam them mot vanh LeVuc m quanh moi o vuc
        int r = Mathf.CeilToInt(LeVuc / O);
        var satVuc = new List<int>();
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
            {
                if (loai[z * n + x] != DiDuocO) continue;
                bool gan = false;
                for (int dz = -r; dz <= r && !gan; dz++)
                    for (int dx = -r; dx <= r && !gan; dx++)
                    {
                        int xx = x + dx, zz = z + dz;
                        if (xx < 0 || zz < 0 || xx >= n || zz >= n) { gan = true; break; }
                        if (loai[zz * n + xx] == Vuc && dx * dx + dz * dz <= r * r) gan = true;
                    }
                if (gan) satVuc.Add(z * n + x);
            }
        foreach (int i in satVuc) loai[i] = SatVuc;

        int di = 0, vuc = 0, vat = 0;
        for (int i = 0; i < loai.Length; i++) { if (loai[i] == DiDuocO) di++; else if (loai[i] == VatCan) vat++; else vuc++; }
        SoCanh = n; SoODiDuoc = di; SoOVuc = vuc; SoOVatCan = vat;
        GiayDung = Time.realtimeSinceStartup - t0;
        canhDungCho = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        SanSang = true; DangDung = false;
        Debug.Log("[BanDoBot] luoi " + n + "x" + n + " o " + O + " m: di duoc " + di + ", vat can " + vat + ", vuc/sat vuc " + vuc
                  + " - dung " + GiayDung.ToString("F2") + " s");
    }

    // ================================================================
    //  TRA CUU
    // ================================================================

    public static Vector3 TamO(int x, int z) { return new Vector3(goc.x + (x + 0.5f) * O, 0f, goc.z + (z + 0.5f) * O); }

    static bool OCua(Vector3 p, out int x, out int z)
    {
        x = Mathf.FloorToInt((p.x - goc.x) / O);
        z = Mathf.FloorToInt((p.z - goc.z) / O);
        return x >= 0 && z >= 0 && x < n && z < n;
    }

    static bool DiDuoc(int x, int z)
    {
        if (x < 0 || z < 0 || x >= n || z >= n) return false;
        int i = z * n + x;
        return loai[i] == DiDuocO && camDen[i] <= Time.time;
    }

    /// <summary>Diem nay di duoc theo luoi (khong vat can, khong vuc, khong bi cam tam).</summary>
    public static bool DiDuoc(Vector3 p)
    {
        int x, z;
        return SanSang && OCua(p, out x, out z) && DiDuoc(x, z);
    }

    /// <summary>Diem nay la vuc / sat vuc theo luoi (ngoai luoi cung tinh la vuc).</summary>
    public static bool LaVuc(Vector3 p)
    {
        int x, z;
        if (!SanSang || !OCua(p, out x, out z)) return true;
        byte l = loai[z * n + x];
        return l == Vuc || l == SatVuc;
    }

    /// <summary>Diem nay la VUC THAT theo luoi (khong tinh vanh sat vuc) - dung cho phep do phia truoc cua BOT.</summary>
    public static bool LaVucThat(Vector3 p)
    {
        int x, z;
        if (!SanSang || !OCua(p, out x, out z)) return true;
        return loai[z * n + x] == Vuc;
    }

    /// <summary>Cam tam cac o trong ban kinh <paramref name="banKinh"/> quanh p trong <paramref name="giay"/> giay.</summary>
    public static void CamTam(Vector3 p, float banKinh, float giay)
    {
        if (!SanSang) return;
        int r = Mathf.CeilToInt(banKinh / O), cx, cz;
        OCua(p, out cx, out cz);
        for (int dz = -r; dz <= r; dz++)
            for (int dx = -r; dx <= r; dx++)
            {
                int x = cx + dx, z = cz + dz;
                if (x < 0 || z < 0 || x >= n || z >= n) continue;
                if ((dx * dx + dz * dz) * O * O > banKinh * banKinh + O * O) continue;
                camDen[z * n + x] = Mathf.Max(camDen[z * n + x], Time.time + giay);
            }
    }

    /// <summary>O di duoc gan p nhat (tim xoan ra toi <paramref name="banKinh"/> m). False neu khong co.</summary>
    public static bool ODiDuocGanNhat(Vector3 p, float banKinh, out Vector3 ra)
    {
        ra = p;
        int cx, cz;
        OCua(p, out cx, out cz);
        int rMax = Mathf.CeilToInt(banKinh / O);
        for (int r = 0; r <= rMax; r++)
        {
            float tot = float.MaxValue; int bx = -1, bz = -1;
            for (int dz = -r; dz <= r; dz++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dz) != r) continue;   // chi vanh ngoai
                    int x = cx + dx, z = cz + dz;
                    if (!DiDuoc(x, z)) continue;
                    float d = dx * dx + dz * dz;
                    if (d < tot) { tot = d; bx = x; bz = z; }
                }
            if (bx >= 0) { ra = TamO(bx, bz); ra.y = p.y; return true; }
        }
        return false;
    }

    /// <summary>Doan thang a -> b di duoc het (lay mau moi nua o) - dung lam thang duong.</summary>
    public static bool ThayThang(Vector3 a, Vector3 b)
    {
        Vector3 d = b - a; d.y = 0f;
        float dai = d.magnitude;
        int buoc = Mathf.Max(1, Mathf.CeilToInt(dai / (O * 0.5f)));
        for (int i = 0; i <= buoc; i++)
        {
            Vector3 p = a + d * (i / (float)buoc);
            int x, z;
            if (!OCua(p, out x, out z) || !DiDuoc(x, z)) return false;
        }
        return true;
    }

    /// <summary>Diem ngau nhien di duoc trong ban kinh r quanh tam (de di tuan khi khong co ai).</summary>
    public static bool DiemNgauNhien(Vector3 tam, float r, out Vector3 ra)
    {
        ra = tam;
        for (int lan = 0; lan < 40; lan++)
        {
            Vector2 v = Random.insideUnitCircle * r;
            var p = tam + new Vector3(v.x, 0f, v.y);
            if (DiDuoc(p)) { ra = p; return true; }
        }
        return false;
    }

    // ================================================================
    //  A*
    // ================================================================

    static int[] gScore;      // nhan 10 (thang) / 14 (cheo)
    static int[] tu;
    static int[] the;         // the he: o da mo trong luot tim nay
    static int[] dong;
    static int luot;
    static readonly List<int> heap = new List<int>();
    static int[] fScore;

    public static int SoLanTim { get; private set; }

    /// <summary>
    /// Tim duong tu <paramref name="tu0"/> toi <paramref name="den0"/>. Diem dich khong di duoc (quai dung sat bia, doi
    /// thu dung mep vuc) thi lay o di duoc gan nhat trong 6 m. Tra ve cac diem moc DA LAM THANG (bo moc giua hai moc
    /// thay thang nhau). False neu khong co duong.
    /// </summary>
    public static bool TimDuong(Vector3 tu0, Vector3 den0, List<Vector3> ra, int toiDaO = 60000)
    {
        ra.Clear();
        if (!SanSang) return false;
        SoLanTim++;
        Vector3 batDau = tu0, dich = den0;
        if (!DiDuoc(batDau) && !ODiDuocGanNhat(tu0, 3f, out batDau)) return false;
        if (!DiDuoc(dich) && !ODiDuocGanNhat(den0, 6f, out dich)) return false;

        int sx, sz, ex, ez;
        OCua(batDau, out sx, out sz);
        OCua(dich, out ex, out ez);
        int s = sz * n + sx, e = ez * n + ex;

        int tong = n * n;
        if (gScore == null || gScore.Length != tong)
        {
            gScore = new int[tong]; tu = new int[tong]; the = new int[tong]; dong = new int[tong]; fScore = new int[tong];
            luot = 0;
        }
        luot++;
        heap.Clear();
        MoO(s, -1, 0, ex, ez);

        int moRa = 0, timThay = -1;
        while (heap.Count > 0 && moRa < toiDaO)
        {
            int c = LayNho();
            if (dong[c] == luot) continue;
            dong[c] = luot;
            moRa++;
            if (c == e) { timThay = c; break; }
            int cx = c % n, cz = c / n;
            for (int dz = -1; dz <= 1; dz++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0) continue;
                    int x = cx + dx, z = cz + dz;
                    if (!DiDuoc(x, z)) continue;
                    // Di cheo chi khi hai o canh cung di duoc - khong cat goc vat can
                    if (dx != 0 && dz != 0 && (!DiDuoc(cx + dx, cz) || !DiDuoc(cx, cz + dz))) continue;
                    int i = z * n + x;
                    if (dong[i] == luot) continue;
                    int g = gScore[c] + (dx != 0 && dz != 0 ? 14 : 10);
                    if (the[i] == luot && g >= gScore[i]) continue;
                    MoO(i, c, g, ex, ez);
                }
        }
        if (timThay < 0) return false;

        // Lan nguoc ra danh sach o
        var oDuong = new List<Vector3>();
        for (int c = timThay; c >= 0; c = tu[c]) { var p = TamO(c % n, c / n); oDuong.Add(p); if (c == s) break; }
        oDuong.Reverse();
        if (oDuong.Count > 0) oDuong[oDuong.Count - 1] = new Vector3(dich.x, 0f, dich.z);

        // Lam thang: tu moc hien tai nhay toi moc XA NHAT con thay thang
        Vector3 hienTai = new Vector3(tu0.x, 0f, tu0.z);
        int k = 0;
        while (k < oDuong.Count)
        {
            int xa = k;
            for (int j = oDuong.Count - 1; j > k; j--)
                if (ThayThang(hienTai, oDuong[j])) { xa = j; break; }
            ra.Add(oDuong[xa]);
            hienTai = oDuong[xa];
            k = xa + 1;
        }
        return true;
    }

    static void MoO(int i, int tuO, int g, int ex, int ez)
    {
        the[i] = luot;
        gScore[i] = g;
        tu[i] = tuO;
        int x = i % n, z = i / n;
        int dx = Mathf.Abs(x - ex), dz = Mathf.Abs(z - ez);
        fScore[i] = g + 10 * Mathf.Max(dx, dz) + 4 * Mathf.Min(dx, dz);
        // Heap nho theo fScore
        heap.Add(i);
        int c = heap.Count - 1;
        while (c > 0)
        {
            int p = (c - 1) / 2;
            if (fScore[heap[p]] <= fScore[heap[c]]) break;
            int t = heap[p]; heap[p] = heap[c]; heap[c] = t; c = p;
        }
    }

    static int LayNho()
    {
        int top = heap[0];
        int cuoi = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        if (heap.Count > 0)
        {
            heap[0] = cuoi;
            int c = 0;
            while (true)
            {
                int l = c * 2 + 1, r = l + 1, m = c;
                if (l < heap.Count && fScore[heap[l]] < fScore[heap[m]]) m = l;
                if (r < heap.Count && fScore[heap[r]] < fScore[heap[m]]) m = r;
                if (m == c) break;
                int t = heap[m]; heap[m] = heap[c]; heap[c] = t; c = m;
            }
        }
        return top;
    }
}
