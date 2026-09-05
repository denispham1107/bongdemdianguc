using UnityEngine;

/// <summary>
/// LOI NUOC: nhan vat va quai buoc xuong nuoc thi phai ra dang loi nuoc that.
///
/// Gan len nguoi choi va tung con quai. Moi khung hinh no hoi <see cref="VungNuoc"/>
/// xem chan dang ngap bao nhieu, roi lo bon viec:
///
///   1. DI CHAM LAI  - nuoc can chan, ngap cang sau di cang nang (<see cref="HeSoToc"/>)
///   2. TOE NUOC     - luc VUA BUOC XUONG thi bot bay tung toe mot cai
///   3. GON SONG     - dang loi ma con di thi chan de lai vong gon lan ra
///   4. BOT QUANH CHAN - bot trang li ti bam quanh cho chan khuay nuoc
///
/// Ba thu 2-4 deu chi sinh ra khi CO DI CHUYEN that su. Dung yen mot cho ma
/// van phun bot thi nhin nhu nhan vat dang soi nuoc.
///
/// Gon song duoc phun o KHONG GIAN THE GIOI (simulationSpace = World): phun
/// theo vat the thi vong gon se di theo chan nguoi choi, ma gon song that thi
/// phai dung yen tai cho roi loang ra.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class LoiNuoc : MonoBehaviour
{
    [Header("Di cham lai")]
    [Tooltip("He so toc do khi ngap sau nhat")]
    public float chamNhat = 0.55f;

    [Tooltip("Ngap qua muc nay (met) thi cham toi da")]
    public float ngapSauNhat = 0.75f;

    [Header("Nhip sinh hieu ung")]
    [Tooltip("Bao lau moi de lai mot vong gon, giay")]
    public float nhipGon = 0.28f;

    /// <summary>He so nhan vao toc do di chuyen. 1 = kho chan.</summary>
    public float HeSoToc { get; private set; }

    /// <summary>Dang ngap sau bao nhieu met. 0 = kho chan.</summary>
    public float DoNgap { get; private set; }

    CharacterController cc;
    ParticleSystem psGon, psBot, psToe;
    float dongHoGon;
    bool dangTrongNuoc;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        HeSoToc = 1f;
    }

    void Update()
    {
        // Diem DUOI CUNG cua nhan vat, khong phai tam nguoi
        Vector3 chan = transform.position + Vector3.up * (cc.center.y - cc.height * 0.5f);

        DoNgap = VungNuoc.DoNgap(chan);
        bool trongNuoc = DoNgap > 0.02f;

        // ---- 1. Di cham lai ----
        float sau = Mathf.Clamp01(DoNgap / Mathf.Max(0.01f, ngapSauNhat));
        HeSoToc = trongNuoc ? Mathf.Lerp(1f, chamNhat, sau) : 1f;

        // Toc do ngang that su, de biet co dang di hay dung yen
        Vector3 v = cc.velocity; v.y = 0f;
        float toc = v.magnitude;

        // ---- 2. Toe nuoc luc vua buoc xuong ----
        if (trongNuoc && !dangTrongNuoc)
        {
            TaoHatNeuChua();
            psToe.transform.position = new Vector3(chan.x, chan.y + DoNgap, chan.z);
            psToe.Emit(Mathf.RoundToInt(Mathf.Lerp(8f, 26f, sau)));
        }
        dangTrongNuoc = trongNuoc;

        if (!trongNuoc || toc < 0.35f) { dongHoGon = 0f; return; }

        TaoHatNeuChua();
        Vector3 mat = new Vector3(chan.x, chan.y + DoNgap, chan.z);

        // ---- 3. Gon song theo nhip buoc ----
        dongHoGon -= Time.deltaTime;
        if (dongHoGon <= 0f)
        {
            dongHoGon = nhipGon;
            psGon.transform.position = mat;
            psGon.Emit(1);
        }

        // ---- 4. Bot trang quanh chan ----
        psBot.transform.position = mat;
        var em = psBot.emission;
        em.rateOverTime = Mathf.Lerp(6f, 34f, sau) * Mathf.Clamp01(toc / 3f);
        if (!psBot.isPlaying) psBot.Play();
    }

    void OnDisable()
    {
        if (psBot != null && psBot.isPlaying) psBot.Stop();
        HeSoToc = 1f;
    }

    // ================================================================
    //  HE HAT - dung mot lan, dung den luc nao thi tao luc do
    // ================================================================

    void TaoHatNeuChua()
    {
        if (psGon != null) return;

        psGon = VfxFactory.GonNuoc(transform);
        psBot = VfxFactory.BotNuoc(transform);
        psToe = VfxFactory.ToeNuoc(transform);
    }
}
