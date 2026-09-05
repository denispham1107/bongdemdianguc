using System.Collections;
using UnityEngine;

/// <summary>
/// Cho vai giay trong PLAY MODE, chup mot anh man hinh roi TU THOAT Play mode.
///
/// De lam gi: nhung thu chi song trong Play mode - OnGUI, LightningArc, he hat
/// dang chay that - khong the chup bang cach dung Camera.Render ngoai Editor.
/// Cach duy nhat de NHIN thay chung la vao Play mode, ma khi Unity dang Play thi
/// khong chay duoc lenh nao nua. Nen phai dat lich truoc: bat Play, doi cho no
/// tu chup, tu thoat, roi moi doc anh.
///
/// Xem <c>ChupTrongPlayMode</c> ben Editor - cho do dung ra doi tuong nay ngay
/// khi Play mode bat dau.
/// </summary>
public class ChupRoiThoat : MonoBehaviour
{
    public string duong;
    public float choGiay = 2f;

    IEnumerator Start()
    {
        // Cho HUD, he hat va anh sang on dinh. Chup ngay khung dau thi con dang
        // do dang: hat chua sinh, thanh mau chua kip cap nhat.
        yield return new WaitForSeconds(choGiay);

        ScreenCapture.CaptureScreenshot(duong);
        Debug.Log("[ChupRoiThoat] da yeu cau chup: " + duong);

        // CaptureScreenshot ghi file KHONG dong bo - thoat Play ngay thi file
        // co the con dang do dang hoac chua co gi.
        yield return new WaitForSeconds(1.5f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
