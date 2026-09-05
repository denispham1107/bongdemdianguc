using UnityEngine;

/// <summary>
/// LAM CHO CAI XAC TREO CU DONG.
///
/// Gan vao bo xuong treo co tren canh cay hoac bi giao dam ghim vao than cay.
/// Ba lop chuyen dong chong len nhau:
///
///   1. Than du dua qua lai nhu qua lac, dong thoi xoay cham quanh truc dung
///   2. Dau gat gu, tay chan dong dua le pha voi than
///   3. Thinh thoang HA HAM RA LA HET, dau ngua len, ca than giat manh mot cai
///
/// MOI CAI XAC MOT NHIP RIENG. Neu de chung mot nhip thi ca rung cay se lac
/// dong loat nhu dang mua tap the - lo ngay la do may lam chu khong phai gio.
/// Nhip rieng lay tu chinh toa do cua no, nen moi lan chay game van y nhu cu.
/// </summary>
public class XacLacLu : MonoBehaviour
{
    [Header("Cac khop can lay")]
    public Transform than;          // khoi than du dua
    public Transform dau;           // dau
    public Transform ham;           // ban le ham duoi
    public Transform[] tayChan;     // tay va chan

    [Header("Du dua")]
    [Tooltip("Bien do lac qua lai, tinh bang do")]
    public float bienDoLac = 5f;
    public float tocDoLac = 0.55f;
    [Tooltip("Bien do xoay quanh truc dung - xac treo co thi xoay nhieu hon")]
    public float bienDoXoay = 9f;
    public float tocDoXoay = 0.21f;

    [Header("Dau va tay chan")]
    public float bienDoDau = 6f;
    public float bienDoTayChan = 7f;

    [Header("La het")]
    [Tooltip("Cach nhau bao lau thi la het mot lan, giay")]
    public float nghiMin = 7f;
    public float nghiMax = 17f;
    public float daiTiengHet = 1.4f;
    [Tooltip("Ham ha xuong toi da bao nhieu do")]
    public float hamMoToiDa = 27f;

    const float TAU = Mathf.PI * 2f;

    float pha;                      // nhip rieng cua cai xac nay
    float henHet;                   // den luc nao thi la het
    float dangHet = -1f;            // -1 = khong het; >= 0 = da het duoc bao lau

    Quaternion thanGoc, dauGoc, hamGoc;
    Quaternion[] tayChanGoc;
    float[] phaTayChan;

    void Awake()
    {
        // Nhip rieng lay tu toa do: moi cai mot kieu, ma chay lai van y nhu cu
        Vector3 p = transform.position;
        pha = Mathf.Repeat(Mathf.Abs(p.x * 12.9898f + p.z * 78.233f + p.y * 37.719f), TAU);

        if (than != null) thanGoc = than.localRotation;
        if (dau != null) dauGoc = dau.localRotation;
        if (ham != null) hamGoc = ham.localRotation;

        if (tayChan != null)
        {
            tayChanGoc = new Quaternion[tayChan.Length];
            phaTayChan = new float[tayChan.Length];
            for (int i = 0; i < tayChan.Length; i++)
            {
                if (tayChan[i] != null) tayChanGoc[i] = tayChan[i].localRotation;
                phaTayChan[i] = pha + i * 1.37f;      // moi chi mot nhip lech
            }
        }

        henHet = Time.time + Random.Range(nghiMin * 0.3f, nghiMax);
    }

    void Update()
    {
        float t = Time.time;

        // ---- La het ----
        float manhHet = 0f;                 // 0 = im, 1 = dang het to nhat
        if (dangHet < 0f)
        {
            if (t >= henHet) dangHet = 0f;
        }
        else
        {
            dangHet += Time.deltaTime;
            float k = dangHet / Mathf.Max(0.05f, daiTiengHet);
            if (k >= 1f)
            {
                dangHet = -1f;
                henHet = t + Random.Range(nghiMin, nghiMax);
            }
            else
            {
                // Ha ham RAT NHANH roi ngam lai tu tu - giat minh thi moi so
                manhHet = k < 0.16f ? k / 0.16f : 1f - (k - 0.16f) / 0.84f;
                manhHet = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(manhHet));
            }
        }

        // ---- Than du dua ----
        if (than != null)
        {
            float lac = Mathf.Sin((t + pha) * tocDoLac * TAU);
            float lacPhu = Mathf.Sin((t + pha * 1.7f) * tocDoLac * TAU * 0.63f);
            float xoay = Mathf.Sin((t + pha * 2.3f) * tocDoXoay * TAU);

            // Luc la het thi ca than giat len mot cai
            float giat = manhHet * 3.5f;

            than.localRotation = thanGoc * Quaternion.Euler(
                lacPhu * bienDoLac * 0.55f - giat,
                xoay * bienDoXoay,
                lac * bienDoLac);
        }

        // ---- Dau gat gu, khi het thi ngua han len ----
        if (dau != null)
        {
            float gat = Mathf.Sin((t + pha * 1.4f) * tocDoLac * TAU * 0.8f);
            dau.localRotation = dauGoc * Quaternion.Euler(
                gat * bienDoDau * 0.6f - manhHet * 15f,
                Mathf.Sin((t + pha * 0.9f) * tocDoLac * TAU * 0.5f) * bienDoDau,
                gat * bienDoDau * 0.4f);
        }

        // ---- Ham ha ra khi la het ----
        if (ham != null)
            ham.localRotation = hamGoc * Quaternion.Euler(manhHet * hamMoToiDa, 0f, 0f);

        // ---- Tay chan dong dua ----
        if (tayChan != null)
            for (int i = 0; i < tayChan.Length; i++)
            {
                if (tayChan[i] == null) continue;
                float a = Mathf.Sin((t + phaTayChan[i]) * tocDoLac * TAU * 0.9f);
                float b = Mathf.Sin((t + phaTayChan[i] * 1.3f) * tocDoLac * TAU * 0.55f);
                tayChan[i].localRotation = tayChanGoc[i] * Quaternion.Euler(
                    a * bienDoTayChan * 0.7f,
                    0f,
                    b * bienDoTayChan);
            }
    }
}
