using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Menu Unity de bat/tat giao dien CAM UNG cho canh dang mo.
///
/// Khac phim F9 o cho nao: F9 chi doi trong PHIEN CHOI dang chay, thoat Play la
/// tro lai nhu cu - dung de xem nhanh. Menu nay GHI VAO CANH, nen giu nguyen
/// qua cac lan mo Unity va di theo ban build.
///
/// Ca hai chi de THU tren PC. May cam ung that tu nhan ra minh qua
/// <see cref="CamUng.DangDung"/>, khong can ai bat gi ca.
/// </summary>
public static class MenuCamUng
{
    const string DuongBat = "Diablo25D/Giao dien cam ung/Bat cho canh nay";
    const string DuongTat = "Diablo25D/Giao dien cam ung/Tat (tro ve PC)";
    const string DuongXem = "Diablo25D/Giao dien cam ung/Dang o che do nao?";

    [MenuItem(DuongBat)]
    static void Bat() { Dat(true); }

    [MenuItem(DuongTat)]
    static void Tat() { Dat(false); }

    [MenuItem(DuongXem)]
    static void Xem()
    {
        var hud = Object.FindFirstObjectByType<GameHUD>();
        if (hud == null) { Debug.LogWarning("[CamUng] Canh dang mo khong co GameHUD"); return; }

        Debug.Log("[CamUng] Canh '" + EditorSceneManager.GetActiveScene().name
                + "' dang o che do: " + (hud.epCamUng ? "CAM UNG" : "PC"));
    }

    static void Dat(bool bat)
    {
        var hud = Object.FindFirstObjectByType<GameHUD>();
        if (hud == null) { Debug.LogWarning("[CamUng] Canh dang mo khong co GameHUD"); return; }

        Undo.RecordObject(hud, "Doi giao dien cam ung");
        hud.epCamUng = bat;
        EditorUtility.SetDirty(hud);

        var canh = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(canh);
        EditorSceneManager.SaveScene(canh);

        Debug.Log("[CamUng] Canh '" + canh.name + "' -> "
                + (bat ? "CAM UNG (joystick + nut tron)" : "PC (thanh ky nang vuong)"));
    }
}
