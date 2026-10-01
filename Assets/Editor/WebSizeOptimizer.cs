using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Dev → Optimize Web Size — baja el peso de descarga del build (2026-09-30).
// Por qué: el WebGL pesaba ~53 MB comprimido; CrazyGames pide <= 50 MB
// (<= 20 MB para su home móvil) y Poki apunta a ~8 MB. Cada MB extra pierde
// jugadores antes de que carguen.
// Qué hace (idempotente, se puede correr varias veces):
//   1. Texturas que usan las escenas del build → crunch (WebGL + Android),
//      tope 2048. Crunch baja mucho la descarga con poca pérdida visual.
//   2. Mallas del build → compresión Medium (Pirate.FBX sola pesaba 11 MB).
//   3. Splash "Made with Unity" apagado (Unity 6 Personal lo permite).
// Headless (sin build): -executeMethod WebSizeOptimizer.RunHeadless
public static class WebSizeOptimizer
{
    const int MaxTextureSize = 2048;
    const int CrunchQuality = 50; // 0-100; 50 = default de Unity, buen equilibrio peso/calidad
    static readonly string[] Platforms = { "WebGL", "Android" };

    [MenuItem("Dev/Optimize Web Size")]
    static void Run()
    {
        var deps = BuildDependencies();
        int textures = OptimizeTextures(deps);
        int meshes = OptimizeModels(deps);
        DisableSplash();
        AssetDatabase.SaveAssets();
        Debug.Log($"[WebSizeOptimizer] {textures} textura(s) y {meshes} modelo(s) cambiados; splash apagado. " +
                  "Revisar en Play Mode que nada se vea mal (fondos, piratas).");
    }

    // entrada para -batchmode; sale con 1 si algo tira excepción (no queda a medias en silencio)
    public static void RunHeadless()
    {
        try { Run(); EditorApplication.Exit(0); }
        catch (System.Exception e) { Debug.LogError($"[WebSizeOptimizer] Falló: {e}"); EditorApplication.Exit(1); }
    }

    // todos los assets que terminan en el build: dependencias recursivas de las escenas habilitadas
    static string[] BuildDependencies()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0)
            throw new System.InvalidOperationException("No hay escenas habilitadas en Build Settings.");
        return AssetDatabase.GetDependencies(scenes, true);
    }

    // crunch en WebGL/Android para cada textura usada; devuelve cuántas cambiaron
    static int OptimizeTextures(IEnumerable<string> deps)
    {
        int changed = 0;
        foreach (var path in deps)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
            if (!path.StartsWith("Assets/")) continue; // texturas de paquetes son de solo lectura

            bool dirty = false;
            foreach (var platform in Platforms)
                dirty |= ApplyCrunch(importer, platform);

            if (dirty) { importer.SaveAndReimport(); changed++; }
        }
        return changed;
    }

    // devuelve true solo si hubo que cambiar algo (para no reimportar de más)
    static bool ApplyCrunch(TextureImporter importer, string platform)
    {
        var s = importer.GetPlatformTextureSettings(platform);
        int size = Mathf.Min(s.overridden ? s.maxTextureSize : importer.maxTextureSize, MaxTextureSize);
        bool already = s.overridden && s.crunchedCompression && s.compressionQuality == CrunchQuality
                       && s.maxTextureSize == size && s.format == TextureImporterFormat.Automatic;
        if (already) return false;

        s.overridden = true;
        s.maxTextureSize = size;
        s.format = TextureImporterFormat.Automatic; // Unity elige DXT/ETC según plataforma
        s.textureCompression = TextureImporterCompression.Compressed;
        s.crunchedCompression = true;
        s.compressionQuality = CrunchQuality;
        importer.SetPlatformTextureSettings(s);
        return true;
    }

    // compresión de malla Medium en modelos del build; devuelve cuántos cambiaron
    static int OptimizeModels(IEnumerable<string> deps)
    {
        int changed = 0;
        foreach (var path in deps)
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
            if (!path.StartsWith("Assets/")) continue;
            // no bajar si alguien ya puso High a mano. Read/Write NO se toca:
            // partículas con forma de malla (Hovl) lo necesitan.
            if (importer.meshCompression >= ModelImporterMeshCompression.Medium) continue;

            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.SaveAndReimport();
            changed++;
        }
        return changed;
    }

    static void DisableSplash()
    {
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }
}
