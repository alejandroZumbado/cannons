using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Dev → Build WebGL — builds straight into the cannons-build deploy repo's working
// copy (E:\Users\Alejandro\Opal\Builds\Cannons). That folder IS a git clone of
// alejandroZumbado/cannons-build (GitHub Pages served from its main branch root) —
// after building, cd there and `git add -A && git commit && git push` to publish.
// Same pattern as Tinted Showdown's WebGLBuildScript.cs / tinted-showdown-build.
public static class WebGLBuildScript
{
    private const string OutputPath = @"E:\Users\Alejandro\Opal\Builds\Cannons";

    [MenuItem("Dev/Build WebGL")]
    public static void Build() => BuildAndReport();

    // true si el build salió bien (usado por PlatformBuilds.BuildWebGL para
    // devolver código de salida en modo headless)
    public static bool BuildAndReport()
    {
        // GitHub Pages never sends a Content-Encoding: gzip/br header, so a
        // compressed WebGL build silently fails to load once hosted there —
        // it only works locally because dev servers happen to set that header.
        // Decompression Fallback packs a JS-side decompressor into the loader
        // so the build works regardless of what headers the host sends.
        if (!PlayerSettings.WebGL.decompressionFallback)
        {
            PlayerSettings.WebGL.decompressionFallback = true;
            Debug.Log("[Build WebGL] Decompression Fallback: OFF -> ON (required to host on GitHub Pages)");
        }

        // Archivos con hash en el nombre (2026-10-01): con nombres fijos, un
        // navegador que ya tenía el build anterior en caché (Pages: 10 min)
        // mezclaba wasm viejo + data nuevo y crasheaba al arrancar
        // ("memory access out of bounds"). Con hash cada build pide archivos nuevos.
        if (!PlayerSettings.WebGL.nameFilesAsHashes)
        {
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            Debug.Log("[Build WebGL] Name Files As Hashes: OFF -> ON (no cache mixing between deploys)");
        }
        ClearOldBuildFiles();

        var scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            scenes[i] = EditorBuildSettings.scenes[i].path;

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[Build WebGL] {report.summary.result} — {report.summary.totalErrors} error(s). See full log above.");
            return false;
        }

        Debug.Log($"[Build WebGL] OK — {report.summary.totalSize / (1024 * 1024)} MB in {report.summary.totalTime}. Output: {OutputPath}");
        return true;
    }

    // con nombres hash Unity no pisa los archivos viejos: se acumularían en el
    // repo de deploy. Solo se borra Build/ (index.html y TemplateData se regeneran);
    // es un clon git, así que cualquier borrado se recupera con git checkout.
    static void ClearOldBuildFiles()
    {
        string buildDir = Path.Combine(OutputPath, "Build");
        if (Directory.Exists(buildDir))
            Directory.Delete(buildDir, true);
    }
}
