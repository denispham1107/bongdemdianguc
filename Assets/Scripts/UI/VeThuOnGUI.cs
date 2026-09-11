using UnityEngine;

/// <summary>
/// CHO PHEP THU VE BANG OnGUI - dung chung voi <see cref="ChayThuMang"/>.
///
/// Hai viec:
///   - Goi <see cref="GiaoDien.ChuanBi"/> moi luot OnGUI nhu man dang nhap /
///     sanh van lam. Phep thu vao THANG man choi thi bo qua hai man ay, va moi
///     thu cua GiaoDien chua tao se lang le khong ve gi: GiaoDien.To ve bang
///     texture "trang" chi co sau ChuanBi. Bang ten tung co nen den ma phep thu
///     vao thang Act2 khong he thay, trong khi nguoi choi (di qua sanh) thay ro.
///   - Ve them thu gi phep thu can (vd mot o toi mau doi chung).
/// </summary>
public class VeThuOnGUI : MonoBehaviour
{
    public System.Action ve;

    void OnGUI()
    {
        GiaoDien.ChuanBi();
        if (ve != null) ve();
    }
}
