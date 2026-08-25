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
    public static void Build()
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
            return;
        }

        Debug.Log($"[Build WebGL] OK — {report.summary.totalSize / (1024 * 1024)} MB in {report.summary.totalTime}. Output: {OutputPath}");
    }
}
