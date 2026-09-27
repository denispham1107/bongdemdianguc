using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// MO MAN CHOI (menu 2).
///
/// Truoc 27/09/2026 file nay con menu 5 "Dung Scene Trong" - dung lai scene Act1 chi co vat the GAME de
/// GameBootstrap tu dung dau truong bang code. Act1 da xoa han (nguoi dung), chi con man Act2.
/// </summary>
public static class WorldBuilder
{
    const string ScenePath = "Assets/Scenes/Act2.unity";

    [MenuItem("Diablo 2.5D/2. Mo Man Choi (Open Scene)", false, 1)]
    public static void OpenScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
    }
}
