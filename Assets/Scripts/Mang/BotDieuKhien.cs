using UnityEngine;

/// <summary>
/// BO NAO CUA MOT MAY BOT - gan vao nhan vat BOT tren MAY CHU PHONG (QuanLy trong KhoiDongTranMang.SinhBot).
///
/// BOT la PlayerController that (cung prefab nguoi choi), tuDocInput tat, mau do may nay quyet (khong phai ban sao). Moi
/// khung bo nao nay dat <see cref="PlayerController.input"/> TRUOC khi PlayerController.Update thi hanh no - cung duong
/// ma ban phim di (GoiInput), nen BOT di, tung phep, uong binh y het nguoi choi.
///
/// BUOC 2 (08/10/2026): BOT CO MAT trong tran - dung yen (goi rong). Buoc 3: di lai (tim quai / doi thu, vong vat can,
/// tranh vuc dia nguc). Buoc 4: ky nang theo he chinh, chieu lien hoan, binh mau / mana, cong diem ky nang khi len cap.
/// </summary>
[DefaultExecutionOrder(-40)]
public class BotDieuKhien : MonoBehaviour
{
    /// <summary>Ghe cua BOT trong tran.</summary>
    public byte ghe;
    /// <summary>MayBot.De / Thuong / Kho - doc tu uid.</summary>
    public int doKho = MayBot.Thuong;
    /// <summary>He chinh (MayBot.HeLua / HeBang / HeSet / HePhong) - boc ngau nhien luc sinh, chi may chu phong biet.</summary>
    public int he;
    public string uid;

    PlayerController pc;

    void Awake() { pc = GetComponent<PlayerController>(); }

    void Update()
    {
        if (pc == null) return;
        pc.input = GoiInput.Rong(Time.deltaTime);
    }
}
