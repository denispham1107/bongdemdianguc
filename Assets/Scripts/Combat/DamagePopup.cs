using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO SAT THUONG bay len roi mo dan - kieu Diablo.
///
/// VE TREN MAN HINH (OnGUI), KHONG PHAI CHU 3D TRONG CANH.
///
/// Truoc day moi con so la mot TextMesh nam trong canh. Nguoi choi bao: cau
/// lua trung nguoi "chi thay sat thuong chay, khong thay sat thuong cu no".
/// Do duoc: cu no CO tru mau (48 mau, dung khung hinh qua cau no), nhung chup
/// man hinh 0,15 va 0,4 giay sau thi khong thay con so dau ca - no nam NGAY
/// GIUA qua cau lua ban kinh 3,4 m, bi hat lua (ve sau no, cong sang) va hieu
/// ung loe (SimpleBloom, chay SAU khi ca canh da ve) phu trang. Con so 48 song
/// 0,9 giay, dung bang luc vu no con sang nhat - nen bien mat tron. Cac cu
/// chay 5 mau den sau, luc lua da tan, nen van nhin thay.
///
/// Doi hang ve (renderQueue) khong du: hieu ung loe cong anh sang len ca man
/// hinh sau cung. Chi co ve bang OnGUI - cung lop voi thanh mau, thanh ky
/// nang - moi chac chan nam tren moi thu.
///
/// Moi con so van la mot GameObject "Dmg" tu bay len, tu mo, tu huy nhu cu;
/// chi phan VE la do <see cref="VeSoSatThuong"/> lam, mot lan cho tat ca.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    public float lifetime = 0.9f;
    public float riseSpeed = 1.6f;
    public Vector3 drift;

    string chu;
    Color mau;
    float t;

    /// <summary>Chieu cao mot dong chu tinh bang met trong canh - giu dung co
    /// chu cua TextMesh cu (fontSize 64 x characterSize 0,055).</summary>
    const float CaoDongMet = 0.35f;

    static readonly List<DamagePopup> dangHien = new List<DamagePopup>();

    public static DamagePopup Spawn(Vector3 pos, int amount, DamageType type, bool onPlayer)
    {
        Color c = onPlayer ? new Color(1f, 0.25f, 0.2f)
                : type == DamageType.Fire ? new Color(1f, 0.62f, 0.15f)
                : type == DamageType.Ice ? new Color(0.6f, 0.88f, 1f)
                : type == DamageType.Lightning ? new Color(0.85f, 0.94f, 1f)
                : new Color(0.95f, 0.92f, 0.85f);

        return SpawnText(pos, amount.ToString(), c);
    }

    /// <summary>Mot dong chu bay len (vd "CHOANG!") - dung chung voi so sat thuong.</summary>
    public static DamagePopup SpawnText(Vector3 pos, string label, Color color)
    {
        var go = new GameObject("Dmg");
        go.transform.position = pos + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0f, 0.3f), Random.Range(-0.25f, 0.25f));

        var p = go.AddComponent<DamagePopup>();
        p.chu = label;
        p.mau = color;
        p.drift = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.4f, 0.4f));

        if (Application.isPlaying) VeSoSatThuong.DamBao();
        return p;
    }

    void OnEnable() { dangHien.Add(this); }
    void OnDisable() { dangHien.Remove(this); }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / lifetime);

        transform.position += (Vector3.up * riseSpeed * (1f - k * 0.6f) + drift) * Time.deltaTime;

        if (k >= 1f) Destroy(gameObject);
    }

    /// <summary>So con so dang hien - de phep thu dem.</summary>
    public static int SoDangHien { get { return dangHien.Count; } }

    /// <summary>
    /// Ve moi con so len man hinh. Chi goi tu <see cref="VeSoSatThuong"/>, trong
    /// luot Repaint.
    /// </summary>
    internal static void VeTatCa(Camera cam, GUIStyle kieu)
    {
        float cao = Screen.height;
        for (int i = 0; i < dangHien.Count; i++)
        {
            var p = dangHien[i];
            if (p == null || string.IsNullOrEmpty(p.chu)) continue;

            Vector3 w = p.transform.position;
            Vector3 man = cam.WorldToScreenPoint(w);
            if (man.z <= 0f) continue;                        // sau lung camera

            // Co chu = chieu cao CaoDongMet met o dung cho ay tren man hinh,
            // nhu chu 3D cu. LAM TRON thanh so chan: font dong ve lai bang
            // chu cho MOI co chu moi, de co chu troi lien tuc la bat no ve
            // lai lien tuc.
            float tren = cam.WorldToScreenPoint(w + Vector3.up * CaoDongMet).y;
            int co = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(tren - man.y) * 0.5f) * 2, 12, 96);
            kieu.fontSize = co;

            float k = Mathf.Clamp01(p.t / p.lifetime);
            float alpha = 1f - Mathf.Pow(k, 2.2f);
            float phong = 1f + Mathf.Sin(k * Mathf.PI) * 0.25f;

            Vector2 tam = new Vector2(man.x, cao - man.y);    // OnGUI dem y tu tren xuong
            var o = new Rect(tam.x - 200f, tam.y - co, 400f, co * 2f);

            // Phong to quanh tam bang ma tran, khong bang co chu - xem tren
            Matrix4x4 cu = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(phong, phong), tam);

            // Vien toi phia duoi-phai: chu do tren nen lua vang van doc ra
            float lech = Mathf.Max(1f, co * 0.06f);
            kieu.normal.textColor = new Color(0f, 0f, 0f, alpha * 0.85f);
            GUI.Label(new Rect(o.x + lech, o.y + lech, o.width, o.height), p.chu, kieu);

            var c = p.mau; c.a = alpha;
            kieu.normal.textColor = c;
            GUI.Label(o, p.chu, kieu);

            GUI.matrix = cu;
        }
    }
}
