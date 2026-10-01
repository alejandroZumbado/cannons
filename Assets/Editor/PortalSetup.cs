using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Dev → Setup CrazyGames SDK — deja el SDK de CrazyGames listo (2026-09-30).
// Requisito: el paquete ya importado (https://sdk.crazygames.com/UnityCrazySDK.unitypackage;
// headless: Unity -batchmode -quit -importPackage <ruta>).
// Qué hace (idempotente):
//   1. Borra Assets/CrazySDK/Demo (escenas/scripts de ejemplo, no son del juego).
//   2. Agrega nuestro GitHub Pages a whitelistedDomains de CrazyGamesSettings:
//      el SiteLock del SDK congela el juego en cualquier dominio que no sea de
//      CrazyGames, y así el mismo build WebGL sirve para probar en Pages.
//   3. Define CRAZYGAMES_SDK solo para WebGL (PortalSdk/GameStorage usan el SDK;
//      PC y Android no lo compilan: su Init tira excepción fuera de WebGL).
// Headless: -executeMethod PortalSetup.RunHeadless
public static class PortalSetup
{
    const string SdkFolder = "Assets/CrazySDK";
    const string DemoFolder = "Assets/CrazySDK/Demo";
    const string SettingsPath = "Assets/CrazySDK/Resources/CrazyGamesSettings.asset";
    const string TestDomain = "alejandrozumbado.github.io";
    const string Define = "CRAZYGAMES_SDK";

    [MenuItem("Dev/Setup CrazyGames SDK")]
    static void Run()
    {
        if (!AssetDatabase.IsValidFolder(SdkFolder))
            throw new System.InvalidOperationException(
                $"No está {SdkFolder}: importar primero UnityCrazySDK.unitypackage.");

        if (AssetDatabase.IsValidFolder(DemoFolder))
            AssetDatabase.DeleteAsset(DemoFolder);

        WhitelistTestDomain();
        AddWebGLDefine();
        AssetDatabase.SaveAssets();
        Debug.Log($"[PortalSetup] CrazyGames listo: {TestDomain} permitido, {Define} definido para WebGL.");
    }

    public static void RunHeadless()
    {
        try { Run(); EditorApplication.Exit(0); }
        catch (System.Exception e) { Debug.LogError($"[PortalSetup] Falló: {e}"); EditorApplication.Exit(1); }
    }

    // por SerializedObject: así este script compila aunque el SDK no esté importado
    static void WhitelistTestDomain()
    {
        var settings = AssetDatabase.LoadMainAssetAtPath(SettingsPath);
        if (settings == null)
            throw new System.InvalidOperationException($"No se encontró {SettingsPath}.");

        var so = new SerializedObject(settings);
        var list = so.FindProperty("whitelistedDomains");
        if (list == null || !list.isArray)
            throw new System.InvalidOperationException("CrazyGamesSettings no tiene whitelistedDomains (¿cambió el SDK?).");

        for (int i = 0; i < list.arraySize; i++)
            if (list.GetArrayElementAtIndex(i).stringValue == TestDomain) return; // ya estaba

        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = TestDomain;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
    }

    static void AddWebGLDefine()
    {
        var target = NamedBuildTarget.WebGL;
        var defines = PlayerSettings.GetScriptingDefineSymbols(target)
            .Split(';').Where(d => d.Length > 0).ToList();
        if (defines.Contains(Define)) return;
        defines.Add(Define);
        PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", defines));
    }
}
