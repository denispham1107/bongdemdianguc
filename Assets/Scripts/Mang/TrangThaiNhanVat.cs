using UnityEngine;

/// <summary>
/// ANH CHUP TRANG THAI NHAN VAT TAI MOT THOI DIEM.
///
/// Dung cho hai viec, va ca hai deu la xuong song cua choi mang:
///   - may nguoi choi luu lai de con QUAY VE mot moc cu khi hieu chinh,
///   - may trong tai gui di de noi "den luc nay, su that la day".
///
/// CHUA GI VA KHONG CHUA GI:
/// Chi nhung thu ma thi hanh mot khung hinh co the doc hoac sua. Vi tri va
/// van toc thi ro rang. Nhung ho chieu cung phai co - thieu no thi luc chay
/// lai, mot ky nang dang doi hoi se duoc coi la san sang va ban ra lan nua,
/// tu nhien nhan vat tung hai qua cau lua tu mot lan bam.
///
/// Struct chu khong phai class: moi khung hinh mot ban, giu lai vai chuc ban
/// de chay lai - class thi moi ban la mot lan cap phat.
/// </summary>
[System.Serializable]
public struct TrangThaiNhanVat
{
    /// <summary>Goi input cuoi cung da duoc thi hanh de ra trang thai nay.</summary>
    public int soThuTu;

    public Vector3 viTri;
    public Vector3 vanToc;
    public Quaternion huongMat;

    public float mana;

    // Ho chieu bay ky nang. Thieu thi chay lai se tung phep hai lan.
    public float hoiCauLua, hoiBang, hoiSet, hoiLoc, hoiThienThach, hoiKhieng, hoiGiatSet;

    /// <summary>Dang niem chu do dang - con bao nhieu giay nua thi xong.</summary>
    public float dangNiem;

    /// <summary>Co diem den dang chay toi khong, va o dau.</summary>
    public bool coDiemDen;
    public Vector3 diemDen;

    /// <summary>Lech bao nhieu met so voi mot trang thai khac. Dung de quyet
    /// dinh co can keo nhan vat ve hay khong.</summary>
    public float LechVoi(TrangThaiNhanVat kia)
    {
        return Vector3.Distance(viTri, kia.viTri);
    }
}
