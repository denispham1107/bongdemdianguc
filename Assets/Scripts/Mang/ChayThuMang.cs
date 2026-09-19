using System.Collections;
using UnityEngine;

/// <summary>
/// VAT THE TAM chi de chay mot coroutine trong Play mode.
///
/// PHAI NAM NGOAI THU MUC Editor. Dat trong Assets/Editor thi Unity tu choi
/// gan no vao GameObject: "Can't add script behaviour because it is an editor
/// script" - va cai bao do chi la mot dong Log mau trang, con thu nem ra la
/// NullReferenceException o cho khac han, rat de tim nham.
///
/// File rieng, ten trung ten class - xem "Phan 6" trong HUONG-DAN.md.
/// </summary>
public class ChayThuMang : MonoBehaviour
{
    void Awake()
    {
        // Co mat vat the nay = dang chay mot kich ban chay thu. Act2 gio vao tran luc XE CHIEU roi toi
        // dan (ChuyenChieuSangDem); de nguyen thi moi phep thu chup anh trong Act2 se thay mot bau troi
        // khac han cai ma anh doi chung cua no da chup. Nhay thang toi dem cho anh sang dung yen.
        // Menu 79 tu tat co nay vi no do CHINH cai chuyen anh sang.
        ChuyenChieuSangDem.ToiDemNgay();
    }

    public IEnumerator batDau;

    /// <summary>Phep thu can ve them len man hinh (vd mau doi chung) thi gan vao day.</summary>
    public System.Action veGUI;

    void Start()
    {
        if (batDau != null) StartCoroutine(batDau);
    }

    void OnGUI()
    {
        if (veGUI == null) return;
        GUI.depth = -1000;
        veGUI();
    }
}
