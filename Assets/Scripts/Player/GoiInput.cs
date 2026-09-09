using UnityEngine;

/// <summary>
/// MOT KHUNG HINH Y MUON CUA NGUOI CHOI, GOI LAI THANH MOT GOI.
///
/// Truoc day <see cref="PlayerController"/> vua doc phim vua thi hanh trong
/// cung mot ham. Choi mot minh thi khong sao, nhung choi mang thi cung mot
/// doan logic phai chay duoc o HAI NOI:
///   - may cua nguoi choi, chay ngay khi bam (du doan, khong doi mang),
///   - may trong tai, chay lai chinh chuoi input do de phan xu.
/// Ban phim chi co o noi thu nhat, nen phai tach y muon ra khoi viec thi hanh.
///
/// VI SAO CAC TRUONG DEU LA TOA DO THE GIOI, khong phai toa do man hinh:
/// moi nguoi xoay camera mot kieu, va may trong tai thi khong co camera cua ai
/// ca. Gui "nghieng can sang trai" thi ben kia khong dich duoc ra huong nao;
/// gui thang "di ve huong (0.7, 0, 0.7)" thi ai cung hieu giong nhau.
///
/// Struct chu khong phai class: mot goi moi khung hinh, 60 goi moi giay, va
/// sau nay con phai giu lai vai chuc goi chua duoc xac nhan de chay lai khi
/// hieu chinh - class thi moi goi la mot lan cap phat, rac don khong xue.
/// </summary>
[System.Serializable]
public struct GoiInput
{
    /// <summary>So thu tu tang dan. Trong tai bao "da xu ly toi so N", may
    /// nguoi choi chay lai tu N+1 tro di.</summary>
    public int soThuTu;

    /// <summary>Khung hinh nay dai bao nhieu giay. Phai gui kem, khong thi
    /// chay lai tren may khac ra quang duong khac.</summary>
    public float dt;

    /// <summary>Huong muon di, TRONG THE GIOI, do dai 0..1. Do dai chinh la
    /// muc day can: day nua chung thi di cham.</summary>
    public Vector3 huongDi;

    /// <summary>Huong dang day can, dung de tu ngam tren may cam ung. Zero
    /// nghia la khong day.</summary>
    public Vector3 huongCan;

    /// <summary>Ky nang vua bam: 0..6, khong bam thi -1.</summary>
    public int kyNang;

    /// <summary>Co dang doi di toi <see cref="diemToi"/> khong (chuot trai).</summary>
    public bool muonDiToi;
    public Vector3 diemToi;

    /// <summary>Diem ngam de tung ky nang, va co ngam duoc khong. Ban tia tu
    /// camera la viec cua may nguoi choi - trong tai chi nhan ket qua roi kep
    /// lai trong tam cho phep.</summary>
    public bool coNgam;
    public Vector3 diemNgam;

    /// <summary>May cam ung thi khong co kieu bam-de-di.</summary>
    public bool laCamUng;

    /// <summary>Co bam phim nao do khong - chi de chan doan, xem
    /// <c>PlayerController.SoLanBamPhim</c>.</summary>
    public bool coBamPhim;

    /// <summary>Goi rong: dung yen, khong bam gi.</summary>
    public static GoiInput Rong(float dt)
    {
        GoiInput g = new GoiInput();
        g.dt = dt;
        g.kyNang = -1;
        return g;
    }
}
