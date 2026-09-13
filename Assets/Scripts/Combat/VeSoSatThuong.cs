using UnityEngine;

/// <summary>
/// VE MOI CON SO SAT THUONG LEN MAN HINH - mot lan cho tat ca.
///
/// Mot doi tuong an duy nhat, song qua moi lan doi man. Tu sinh ra lan dau co
/// con so nao can ve (<see cref="DamagePopup.SpawnText"/>).
///
/// Vi sao ve bang OnGUI chu khong de chu 3D trong canh: xem ghi chu dau
/// <see cref="DamagePopup"/> - con so cua cu no bi chinh vu no che mat.
/// </summary>
public class VeSoSatThuong : MonoBehaviour
{
    static VeSoSatThuong ban;
    GUIStyle kieu;

    public static void DamBao()
    {
        if (ban != null) return;
        var go = new GameObject("VeSoSatThuong");
        go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
        DontDestroyOnLoad(go);
        ban = go.AddComponent<VeSoSatThuong>();
    }

    void OnGUI()
    {
        // OnGUI chay nhieu luot moi khung (Layout, cac su kien chuot...) - chi
        // luot Repaint moi ve that
        if (Event.current.type != EventType.Repaint) return;

        // Dang mo Sach phep thi tran dau dung im sau mot lop man toi - so sat
        // thuong bay len de len ca bang la nhin nhu bang bi loi.
        if (CuaSoSachPhep.DangMo) return;

        var cam = Camera.main;
        if (cam == null) return;

        if (kieu == null)
        {
            kieu = new GUIStyle(GUI.skin.label);
            // FONT INTER, khong de font mac dinh: chu noi co dau (CHOÁNG!, NGÃ!,
            // BỊ CUỐN!) - font mac dinh thieu chu Viet, Editor van hien dung vi
            // Windows ve bu nhung len web thi mat dau.
            kieu.font = GiaoDien.ChuDam;
            kieu.fontStyle = FontStyle.Normal;
            kieu.alignment = TextAnchor.MiddleCenter;
            kieu.wordWrap = false;
            kieu.clipping = TextClipping.Overflow;
        }

        // Nam DUOI thanh mau va thanh ky nang: so thap thi ve sau, tuc nam tren.
        // HUD dung do sau 0.
        GUI.depth = 10;
        DamagePopup.VeTatCa(cam, kieu);
    }
}
