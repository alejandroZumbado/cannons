using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneMigrator
{
    [MenuItem("Dev/Migrate All Scenes to Unity 6")]
    static void MigrateAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        int total = guids.Length;
        int done  = 0;

        // guarda la escena activa antes de empezar
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                EditorUtility.DisplayProgressBar(
                    "Migrating scenes",
                    path,
                    (float)done / total);

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                EditorSceneManager.SaveScene(scene);
                done++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog(
            "Migrate All Scenes",
            $"{done} escenas migradas a Unity 6.4.",
            "OK");
    }
}
