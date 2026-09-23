using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds de Windows y Android, desde el menú o sin abrir Unity:
//   Unity.exe -batchmode -quit -projectPath <Cannons> -buildTarget Win64   -executeMethod PlatformBuilds.BuildWindows
//   Unity.exe -batchmode -quit -projectPath <Cannons> -buildTarget Android -executeMethod PlatformBuilds.BuildAndroid
//   Unity.exe -batchmode -quit -projectPath <Cannons> -buildTarget WebGL   -executeMethod PlatformBuilds.BuildWebGL
// Cada build corre antes ReleaseValidator: si el lanzamiento de niveles tiene
// errores (password repetido, referencia rota...) NO se compila nada.
// En batchmode sale con código 1 si algo falla, para que un script lo detecte.
// Salida en E:\Users\Alejandro\Opal\Builds\ (misma convención que los otros juegos).
public static class PlatformBuilds
{
    private const string BuildsRoot = @"E:\Users\Alejandro\Opal\Builds";
    private static string WindowsExe => Path.Combine(BuildsRoot, "Cannons Windows", "Cannons.exe");
    private static string AndroidApk => Path.Combine(BuildsRoot, "Cannons Android", "Cannons.apk");

    [MenuItem("Dev/Build Windows")]
    public static void BuildWindows() => Finish(Build(BuildTarget.StandaloneWindows64, WindowsExe));

    // APK firmado con la llave debug de Unity: sirve para instalar y probar,
    // NO para Play Store (eso requiere .aab + keystore propio).
    [MenuItem("Dev/Build Android APK")]
    public static void BuildAndroid()
    {
        EditorUserBuildSettings.buildAppBundle = false; // .apk, no .aab
        Finish(Build(BuildTarget.Android, AndroidApk));
    }

    // mismo build que Dev > Build WebGL, pero con validación previa y código de salida
    public static void BuildWebGL() => Finish(ValidateFirst() && WebGLBuildScript.BuildAndReport());

    static bool Build(BuildTarget target, string outputPath)
    {
        if (!ValidateFirst()) return false;

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = target,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[Build {target}] {report.summary.result} — {report.summary.totalErrors} error(es). Ver log arriba.");
            return false;
        }
        Debug.Log($"[Build {target}] OK — {report.summary.totalSize / (1024 * 1024)} MB en {report.summary.totalTime}. Salida: {outputPath}");
        return true;
    }

    // no se compila un juego con niveles rotos
    static bool ValidateFirst()
    {
        if (ReleaseValidator.Validate(out int errors, out _)) return true;
        Debug.LogError($"[Build] Cancelado: ReleaseValidator encontró {errors} error(es) en los niveles.");
        return false;
    }

    // en batchmode el resultado va al código de salida; en el Editor solo al log
    static void Finish(bool ok)
    {
        if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
    }
}
