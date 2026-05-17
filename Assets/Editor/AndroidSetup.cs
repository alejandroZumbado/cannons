using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

// Dev → Setup Android   — aplica toda la configuracion
// Dev → Verify Android  — solo lee y reporta, no cambia nada
public static class AndroidSetup
{
    const string PackageName = "com.mandrix.cannons";
    const string SceneGame   = "Game";
    const string SceneMenu   = "Menu";

    // ─── VERIFY ───────────────────────────────────────────────────────────

    [MenuItem("Dev/Verify Android")]
    static void Verify()
    {
        int ok = 0, fail = 0;

        Check(ref ok, ref fail, "Input Handling = Both",       GetActiveInputHandler() == 2);
        Check(ref ok, ref fail, "Orientacion = LandscapeLeft", PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft);
        Check(ref ok, ref fail, "Portrait desactivado",        !PlayerSettings.allowedAutorotateToPortrait && !PlayerSettings.allowedAutorotateToPortraitUpsideDown);
        Check(ref ok, ref fail, $"Package = {PackageName}",   PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == PackageName);
        Check(ref ok, ref fail, "Backend = IL2CPP",            PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP);
        Check(ref ok, ref fail, "Arquitectura = ARM64",        PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64);
        Check(ref ok, ref fail, "Min API = 23",                (int)PlayerSettings.Android.minSdkVersion    == 23);
        Check(ref ok, ref fail, "Target API = 34",             (int)PlayerSettings.Android.targetSdkVersion == 34);

        CheckCanvasScalersInScene(SceneGame, ref ok, ref fail);
        CheckCanvasScalersInScene(SceneMenu, ref ok, ref fail);

        Debug.Log($"[Verify Android] {ok} OK  |  {fail} FAIL");
    }

    static int GetActiveInputHandler()
    {
        var so   = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var prop = so.FindProperty("activeInputHandler");
        return prop != null ? prop.intValue : -1;
    }

    static void Check(ref int ok, ref int fail, string label, bool condition)
    {
        if (condition) { Debug.Log($"[Verify] ✓  {label}");      ok++;   }
        else           { Debug.LogError($"[Verify] ✗  {label}"); fail++; }
    }

    static void CheckCanvasScalersInScene(string sceneName, ref int ok, ref int fail)
    {
        string path = FindScenePathExact(sceneName);
        if (path == null)
        {
            Debug.LogError($"[Verify] ✗  Escena '{sceneName}' no encontrada");
            fail++;
            return;
        }

        // abre aditivo para no tocar la escena activa
        var scene   = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        var scalers = GetScalersInScene(scene);

        if (scalers.Length == 0)
        {
            Debug.LogError($"[Verify] ✗  Sin CanvasScaler en '{sceneName}'");
            fail++;
        }
        else
        {
            foreach (var cs in scalers)
            {
                bool correct =
                    cs.uiScaleMode        == CanvasScaler.ScaleMode.ScaleWithScreenSize       &&
                    cs.referenceResolution == new Vector2(1920, 1080)                          &&
                    cs.screenMatchMode    == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight   &&
                    Mathf.Approximately(cs.matchWidthOrHeight, 1f);

                Check(ref ok, ref fail, $"CanvasScaler '{sceneName}/{cs.gameObject.name}'", correct);
            }
        }

        EditorSceneManager.CloseScene(scene, true);
    }

    // ─── SETUP ────────────────────────────────────────────────────────────

    [MenuItem("Dev/Setup Android")]
    static void Run()
    {
        if (!EditorUtility.DisplayDialog("Setup Android",
            $"Se configurara:\n\n"              +
            $"- Orientacion: Landscape Left\n"  +
            $"- Package: {PackageName}\n"        +
            $"- Backend: IL2CPP\n"               +
            $"- Arquitectura: ARM64\n"           +
            $"- API Min: 23 / Target: 34\n"      +
            $"- Input Handling: Both\n"          +
            $"- Canvas Scaler en Game y Menu\n\n"+
            $"Al terminar se abre la escena Game.\nContinuar?",
            "Si", "Cancelar"))
            return;

        ApplyPlayerSettings();
        ApplyCanvasScalers();

        Debug.Log("[AndroidSetup] Listo. Corre Dev → Verify Android para confirmar.");
    }

    static void ApplyPlayerSettings()
    {
        PlayerSettings.defaultInterfaceOrientation           = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait           = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft      = true;
        PlayerSettings.allowedAutorotateToLandscapeRight     = true;

        var so   = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        var prop = so.FindProperty("activeInputHandler");
        if (prop != null) { prop.intValue = 2; so.ApplyModifiedProperties(); }

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion       = (AndroidSdkVersions)23;
        PlayerSettings.Android.targetSdkVersion    = (AndroidSdkVersions)34;

        AssetDatabase.SaveAssets();
        Debug.Log("[AndroidSetup] Player Settings aplicados.");
    }

    static void ApplyCanvasScalers()
    {
        // guarda cambios pendientes de la escena activa antes de cambiar
        var active = EditorSceneManager.GetActiveScene();
        if (active.isDirty)
            EditorSceneManager.SaveScene(active);

        SetCanvasScalersInScene(SceneGame);
        SetCanvasScalersInScene(SceneMenu);

        // al terminar deja abierta la escena Game (no la que estaba antes, que podria ser Demo)
        string gamePath = FindScenePathExact(SceneGame);
        if (gamePath != null)
            EditorSceneManager.OpenScene(gamePath, OpenSceneMode.Single);
    }

    static void SetCanvasScalersInScene(string sceneName)
    {
        string path = FindScenePathExact(sceneName);
        if (path == null)
        {
            Debug.LogWarning($"[AndroidSetup] Escena '{sceneName}' no encontrada.");
            return;
        }

        var scene   = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var scalers = GetScalersInScene(scene);

        if (scalers.Length == 0)
        {
            Debug.LogWarning($"[AndroidSetup] Sin CanvasScaler en '{sceneName}'.");
            return;
        }

        foreach (var cs in scalers)
        {
            cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight  = 1f;
            EditorUtility.SetDirty(cs);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[AndroidSetup] CanvasScaler configurado en '{sceneName}' ({scalers.Length} canvas).");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    // busca la escena por nombre EXACTO del archivo (sin extension)
    static string FindScenePathExact(string sceneName)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
                return path;
        }
        return null;
    }

    // devuelve solo los CanvasScaler que pertenecen a la escena indicada
    static CanvasScaler[] GetScalersInScene(UnityEngine.SceneManagement.Scene scene)
    {
        var result = new System.Collections.Generic.List<CanvasScaler>();
        foreach (var root in scene.GetRootGameObjects())
            result.AddRange(root.GetComponentsInChildren<CanvasScaler>(true));
        return result.ToArray();
    }
}
