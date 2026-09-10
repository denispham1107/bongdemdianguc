using UnityEngine;

/// <summary>
/// SO HIEU CUA MOT CON QUAI, DE HAI MAY GOI CUNG MOT TEN.
///
/// Khong co no thi khong cach nao noi "con quai o goc kia vua mat 40 mau" -
/// hai may co hai danh sach quai rieng, va thu tu trong danh sach thi doi
/// moi khi co con chet.
///
/// Chu phong cap so, tang dan, khong bao gio dung lai. Dung <c>ushort</c> vi
/// mot tran khong bao gio den 65 000 con quai, va moi byte deu di qua mang 10
/// lan moi giay.
///
/// <see cref="loai"/> phai gui kem: may khach chua co con quai ay bao gio,
/// no can biet dung mau nao de sinh ra - bo xuong hay phu thuy hay quy du.
/// </summary>
public class NhanDangQuai : MonoBehaviour
{
    public ushort id;
    public MonsterType loai;
}
