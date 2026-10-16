using UnityEngine;

/// <summary>
/// TIENG MOI TRUONG TRONG TRAN ACT2 (17/10/2026, nguoi dung): "cu moi 1 phut them am thanh stereo tieng QUA KEU THAM THIET, sau khi qua keu 30 giay them
/// tieng BAY DOI DAP CANH BAY". Nguoi dung chon: CHI trong tran Act2 (sanh / dang nhap khong co); qua keu tu HUONG NGAU NHIEN xa xa; doi bay VUT NGANG tu
/// ben nay sang ben kia qua dau; am luong theo thanh HIEU UNG.
///
/// Tieng (CongCu/AmThanh/tieng_qua_doi_3.py, ban 3 - ban 1 che "mong, don dieu, mono"; ban 2 doi "nhu sung lien thanh"): qua = mau A "Qua den tham
/// thiet" (QuaDoiThu3/A_qua_den_tham_thiet_qua1-3), doi = mau C (C_bay_qua_hoang_loan_doi1-3). HUONG NUONG SAN trong clip (WebGL khong co panStereo):
/// QuaKeu1-3 con chinh ben PHAI, 4-6 ban LAT (ben trai); DoiBay1-3 bay trai -> phai, 4-6 phai -> trai. Phat 2D giua (giu nguyen stereo cua clip), chon
/// ngau nhien (khong trung lan truoc), cao do +-3%.
///
/// Lich: dong ho TRAN (Time.deltaTime tu luc vao Act2): qua o 60, 120, 180... giay; doi o qua + 30 giay. Dang chay phep thu (co ChayThuMang) thi IM -
/// tru <see cref="BatTrongPhepThu"/> (menu 139).
/// </summary>
public class TiengMoiTruong : MonoBehaviour
{
    public const float ChuKy = 60f, TreDoi = 30f;
    public const float HeSoQua = 0.8f, HeSoDoi = 0.85f;
    public const int SoBienThe = 6;
    const string ThuMuc = "AmThanh/MoiTruong/";

    /// <summary>Phep thu (menu 139) bat de nghe / do khi co ChayThuMang.</summary>
    public static bool BatTrongPhepThu;
    /// <summary>Dem cho phep thu: so lan qua / doi da phat, ten clip va dong ho tran luc phat gan nhat.</summary>
    public static int SoLanQua, SoLanDoi;
    public static string ClipQuaCuoi, ClipDoiCuoi;
    public static float LucQuaCuoi = -1f, LucDoiCuoi = -1f;
    public static TiengMoiTruong Ban { get; private set; }

    /// <summary>Dong ho tran (giay) - phep thu tua toi bang <see cref="DatDongHo"/>.</summary>
    public float DongHo { get; private set; }
    float lanQuaTiep = ChuKy, lanDoiTiep = ChuKy + TreDoi;
    AudioSource nguon;
    float heSoDangPhat;
    int quaTruoc = -1, doiTruoc = -1;
    bool coPhepThu;
    float lucKiemPhepThu;

    static AudioClip[] qua, doi;

    public static TiengMoiTruong Gan(GameObject cha)
    {
        var go = new GameObject("TiengMoiTruong");
        go.transform.SetParent(cha.transform, false);
        return go.AddComponent<TiengMoiTruong>();
    }

    void Awake()
    {
        Ban = this;
        nguon = gameObject.AddComponent<AudioSource>();
        nguon.playOnAwake = false;
        nguon.loop = false;
        nguon.spatialBlend = 0f;          // 2D: giu nguyen stereo nuong san trong clip
        nguon.panStereo = 0f;
        nguon.priority = 64;
        NapClip();
        coPhepThu = FindAnyObjectByType<ChayThuMang>() != null;
    }

    static void NapClip()
    {
        if (qua == null || qua.Length != SoBienThe || qua[0] == null)
        {
            qua = new AudioClip[SoBienThe]; doi = new AudioClip[SoBienThe];
            for (int i = 0; i < SoBienThe; i++)
            {
                qua[i] = Resources.Load<AudioClip>(ThuMuc + "QuaKeu" + (i + 1));
                doi[i] = Resources.Load<AudioClip>(ThuMuc + "DoiBay" + (i + 1));
            }
        }
        // WebGL giai ma bat dong bo: nap san de luc den gio phat la co tieng
        foreach (var c in qua) if (c != null && c.loadState == AudioDataLoadState.Unloaded) c.LoadAudioData();
        foreach (var c in doi) if (c != null && c.loadState == AudioDataLoadState.Unloaded) c.LoadAudioData();
    }

    /// <summary>Phep thu: dat dong ho tran (cac moc chua toi sau gio nay van phat dung lich).</summary>
    public void DatDongHo(float giay)
    {
        DongHo = giay;
        lanQuaTiep = Mathf.Max(ChuKy, Mathf.Ceil((giay + 1e-3f) / ChuKy) * ChuKy);
        lanDoiTiep = lanQuaTiep - ChuKy + TreDoi;
        if (lanDoiTiep <= giay) lanDoiTiep += ChuKy;
    }

    void Update()
    {
        if (Time.unscaledTime >= lucKiemPhepThu)
        {
            lucKiemPhepThu = Time.unscaledTime + 0.5f;
            coPhepThu = FindAnyObjectByType<ChayThuMang>() != null;
        }
        DongHo += Time.deltaTime;
        if (DongHo >= lanQuaTiep) { lanQuaTiep += ChuKy; Phat(true); }
        if (DongHo >= lanDoiTiep) { lanDoiTiep += ChuKy; Phat(false); }
        bool im = coPhepThu && !BatTrongPhepThu;
        nguon.volume = im ? 0f : heSoDangPhat * CaiDatAmThanh.HieuUngDangNghe;
    }

    void Phat(bool laQua)
    {
        NapClip();
        var ds = laQua ? qua : doi;
        int truoc = laQua ? quaTruoc : doiTruoc;
        int k = Random.Range(0, SoBienThe);
        if (k == truoc) k = (k + 1 + Random.Range(0, SoBienThe - 1)) % SoBienThe;
        var c = ds[k];
        if (laQua) { SoLanQua++; quaTruoc = k; LucQuaCuoi = DongHo; ClipQuaCuoi = c != null ? c.name : null; }
        else { SoLanDoi++; doiTruoc = k; LucDoiCuoi = DongHo; ClipDoiCuoi = c != null ? c.name : null; }
        if (c == null) return;
        heSoDangPhat = laQua ? HeSoQua : HeSoDoi;
        // mot nguon: qua (giay 60) va doi (giay 90) cach nhau 30 s - clip dai nhat 12 s, khong chong
        nguon.Stop();
        nguon.clip = c;
        nguon.pitch = Random.Range(0.97f, 1.03f);
        nguon.volume = (coPhepThu && !BatTrongPhepThu) ? 0f : heSoDangPhat * CaiDatAmThanh.HieuUngDangNghe;
        nguon.Play();
    }

    void OnDestroy() { if (Ban == this) Ban = null; }
}
