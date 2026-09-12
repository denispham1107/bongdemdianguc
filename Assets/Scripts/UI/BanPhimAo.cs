using UnityEngine;

/// <summary>
/// BAN PHIM AO CUA DIEN THOAI CHE MAT BAO NHIEU MAN HINH.
///
/// Tren dien thoai, cham vao o nhap thi ban phim truot len va CHE KIN o nhap -
/// nguoi choi go ma khong thay minh dang go gi (nguoi dung gap 12/09/2026 o man
/// dang nhap). Game phai day khung nhap len tren ban phim, ma muon day thi phai
/// biet ban phim cao bao nhieu.
///
/// UNITY KHONG BIET. Ban phim la cua he dieu hanh, no khong thay doi kich thuoc
/// khung game (canvas), nen <c>Screen.height</c> giu nguyen. Chi TRANG WEB biet:
/// trinh duyet co <c>window.visualViewport</c> - phan man hinh dang THUC SU nhin
/// thay. Ban phim len thi vung ay thap di.
///
/// Nen trang (index.html) do roi goi sang day bang
/// <c>SendMessage("BanPhimAo", "DatChe", "<ti le>")</c>. Ti le chu khong phai so
/// diem anh: hai ben do bang hai he don vi khac nhau (CSS pixel va pixel that
/// cua khung game), nhan voi <c>Screen.height</c> o ben nay moi dung.
///
/// Ngoai web (Editor, ban PC) thi luon bang 0 - khong co ban phim ao nao.
/// </summary>
public class BanPhimAo : MonoBehaviour
{
    /// <summary>Ban phim che bao nhieu phan man hinh (0 = khong che).</summary>
    public static float TiLeChe { get; private set; }

    /// <summary>Ban phim che bao nhieu DIEM man hinh cua game.</summary>
    public static float CaoChe { get { return Screen.height * Mathf.Clamp01(TiLeChe); } }

    /// <summary>Vung con nhin thay duoc (tu day tinh len), tinh bang diem.</summary>
    public static float CaoNhinThay { get { return Screen.height - CaoChe; } }

    /// <summary>Co dang hien ban phim khong - de giao dien thu gon lai.</summary>
    public static bool DangHien { get { return TiLeChe > 0.05f; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Dung()
    {
        // Vat the PHAI ten dung "BanPhimAo" va song qua moi lan nap canh:
        // SendMessage cua trang web tim theo TEN.
        var go = new GameObject("BanPhimAo");
        go.AddComponent<BanPhimAo>();
        DontDestroyOnLoad(go);
    }

    /// <summary>Trang web goi vao day moi khi vung nhin thay doi.</summary>
    public void DatChe(string tiLe)
    {
        float v;
        if (!float.TryParse(tiLe, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out v)) return;
        TiLeChe = Mathf.Clamp01(v);
    }
}
