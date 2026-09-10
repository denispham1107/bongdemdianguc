using UnityEngine;

/// <summary>
/// NHO XEM MOT GIAY VUA ROI MINH DA DUNG O NHUNG DAU.
///
/// Can cho viec bu tre: khi phep cua nguoi kia bay sang, ta phai tinh trung
/// theo vi tri cua minh LUC HO BAM, khong phai luc goi tin den noi. Muon lui
/// ve luc do thi phai co nguoi nho ho.
///
/// Vong dem co dinh, ghi de vong quanh - khong cap phat gi trong luc choi.
/// Mot giay la du: do tre nhin thay nhau do duoc 82-131 ms, va bu qua 300 ms
/// thi sinh ra chuyen vo ly kieu "da nap sau goc tuong roi van an don".
/// </summary>
public class LichSuViTri : MonoBehaviour
{
    /// <summary>Nho lui bao nhieu giay.</summary>
    public const float GiayNhoLai = 1.0f;

    /// <summary>Ghi lai bao nhieu lan moi giay. 60 la moi khung hinh.</summary>
    public const int NhipGhi = 60;

    struct MotMoc
    {
        public float luc;        // Time.unscaledTime
        public Vector3 viTri;
    }

    static readonly int SoMoc = Mathf.CeilToInt(GiayNhoLai * NhipGhi) + 2;

    MotMoc[] vong;
    int dau;          // cho ghi tiep theo
    int soDaGhi;
    float ghiLanSau;

    /// <summary>Vi tri that luc dang bi lui - de tra lai cho dung.</summary>
    Vector3 choThat;
    bool dangLui;

    public bool DangLui { get { return dangLui; } }

    void Awake()
    {
        vong = new MotMoc[SoMoc];
    }

    void LateUpdate()
    {
        // Ghi o LateUpdate: luc nay nhan vat da di xong trong khung nay.
        // Ghi o Update thi moc luon cham mot khung so voi cai mat nhin thay.
        if (dangLui) return;
        if (Time.unscaledTime < ghiLanSau) return;

        ghiLanSau = Time.unscaledTime + 1f / NhipGhi;

        vong[dau] = new MotMoc { luc = Time.unscaledTime, viTri = transform.position };
        dau = (dau + 1) % SoMoc;
        if (soDaGhi < SoMoc) soDaGhi++;
    }

    /// <summary>
    /// Minh dang o dau vao <paramref name="doTreGiay"/> giay truoc.
    ///
    /// Chen giua hai moc gan nhat chu khong lay moc gan dung: hai moc cach
    /// nhau 17 ms, ma nhan vat chay 6 m/giay thi 17 ms la 10 cm - du de mot
    /// don sat sat thanh trung hay truot.
    /// </summary>
    public Vector3 ViTriLuc(float doTreGiay)
    {
        if (soDaGhi == 0) return transform.position;

        float can = Time.unscaledTime - Mathf.Max(0f, doTreGiay);

        MotMoc? truoc = null, sau = null;
        for (int k = 1; k <= soDaGhi; k++)
        {
            var m = vong[(dau - k + SoMoc * 2) % SoMoc];
            if (m.luc <= can) { truoc = m; break; }
            sau = m;
        }

        if (truoc == null) return sau.HasValue ? sau.Value.viTri : transform.position;
        if (sau == null) return truoc.Value.viTri;

        float khoang = sau.Value.luc - truoc.Value.luc;
        if (khoang <= 0.0001f) return truoc.Value.viTri;

        float t = Mathf.Clamp01((can - truoc.Value.luc) / khoang);
        return Vector3.Lerp(truoc.Value.viTri, sau.Value.viTri, t);
    }

    /// <summary>Lui ve qua khu. Nho goi <see cref="Tra"/> ngay trong cung khung hinh.</summary>
    public void Lui(float doTreGiay)
    {
        if (dangLui) return;
        choThat = transform.position;
        Vector3 cu = ViTriLuc(doTreGiay);
        dangLui = true;

        var cc = GetComponent<CharacterController>();
        bool batLai = cc != null && cc.enabled;
        if (batLai) cc.enabled = false;
        transform.position = cu;
        if (batLai) cc.enabled = true;
    }

    public void Tra()
    {
        if (!dangLui) return;

        var cc = GetComponent<CharacterController>();
        bool batLai = cc != null && cc.enabled;
        if (batLai) cc.enabled = false;
        transform.position = choThat;
        if (batLai) cc.enabled = true;

        dangLui = false;
    }
}
