using UnityEngine;

/// <summary>
/// CHOP TAT CAC DOAN TIA DIEN quanh Qua cau dien (nguoi dung chon 18/09/2026).
///
/// Yeu cau: "thay cac duong van trang thanh cac tia dien mong bao boc xung quanh qua cau ...
/// (khong ve lien mach nhe)" - va khi hoi lai thi chon CHOP TAT LIEN TUC chu khong dung yen.
///
/// Cach lam: Blender xuat BON KHUNG (VoTia0..3 trong VoTiaDien.fbx), moi khung la cung nam duong
/// vong ay nhung cac doan tia nam o cho khac. Component nay chi DOI sharedMesh cua MeshFilter -
/// mot renderer duy nhat, khong sinh rac, khong bat/tat GameObject: re nhat cho dien thoai.
///
/// Khong bao gio doi sang DUNG khung dang hien (bat mot khung hai lan lien nhau nhin ra "dung hinh"),
/// va nhip doi ngau nhien trong khoang <see cref="NhipMin"/>..<see cref="NhipMax"/> - nhip deu tam tap
/// nhin ra may moc chu khong ra dien.
/// </summary>
public class ChopTiaDien : MonoBehaviour
{
    public const float NhipMin = 0.045f;
    public const float NhipMax = 0.105f;

    public Mesh[] khung;

    /// <summary>So lan doi khung - phep thu (menu 74) doc.</summary>
    public int soLanDoi;

    MeshFilter mf;
    float henDoi;
    int dangO;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        henDoi = Time.time + Random.Range(NhipMin, NhipMax);
    }

    void Update()
    {
        if (mf == null || khung == null || khung.Length < 2) return;
        if (Time.time < henDoi) return;

        int moi = Random.Range(0, khung.Length - 1);
        if (moi >= dangO) moi++;            // khong lay lai chinh khung dang hien
        dangO = moi;
        mf.sharedMesh = khung[moi];
        soLanDoi++;
        henDoi = Time.time + Random.Range(NhipMin, NhipMax);
    }
}
