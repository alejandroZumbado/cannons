using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds de Windows y Android, desde el menú o sin abrir Unity:
//   Unity.exe -batchmode -quit -projectPath <Cannons> -buildTarget Win64   -executeMethod PlatformBuilds.BuildWindows
//   Unity.exe -batchmode -quit -projectPath <Cannons> -buildTarget Android -executeMethod PlatformBuilds.BuildAndroid
//   Unity.exe -batchmode -quit -projectPath <Cannons> -buildTarget Android -executeMethod PlatformBuilds.BuildAndroidAab
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
    private static string AndroidAab => Path.Combine(BuildsRoot, "Cannons Android", "Cannons.aab");

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

    // .aab firmado con la llave propia, para subir a Play Store.
    // La llave NUNCA va al repo ni a ProjectSettings: se lee de variables de entorno
    //   CANNONS_KEYSTORE_PATH  (ruta al .keystore, fuera del repo)
    //   CANNONS_KEYSTORE_PASS  CANNONS_KEY_ALIAS  CANNONS_KEY_PASS
    // Si falta alguna, no se compila (falla explícito, no cae a la llave debug).
    // Recordatorio: Play Store exige subir bundleVersionCode en cada subida.
    [MenuItem("Dev/Build Android AAB (Play Store)")]
    public static void BuildAndroidAab() => Finish(BuildSignedAab());

    static bool BuildSignedAab()
    {
        if (!TryReadSigningEnv(out string keystorePath, out string storePass, out string alias, out string keyPass))
            return false;

        // se guardan los valores previos para restaurarlos: así ProjectSettings
        // no queda apuntando a la llave ni con el modo .aab activado
        bool prevCustom = PlayerSettings.Android.useCustomKeystore;
        string prevKeystore = PlayerSettings.Android.keystoreName;
        string prevAlias = PlayerSettings.Android.keyaliasName;
        bool prevBundle = EditorUserBuildSettings.buildAppBundle;
        try
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = storePass;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = keyPass;
            EditorUserBuildSettings.buildAppBundle = true;

            Debug.Log($"[Build AAB] versión {PlayerSettings.bundleVersion}, bundleVersionCode {PlayerSettings.Android.bundleVersionCode}.");
            return Build(BuildTarget.Android, AndroidAab);
        }
        finally
        {
            PlayerSettings.Android.useCustomKeystore = prevCustom;
            PlayerSettings.Android.keystoreName = prevKeystore;
            PlayerSettings.Android.keyaliasName = prevAlias;
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasPass = "";
            EditorUserBuildSettings.buildAppBundle = prevBundle;
        }
    }

    // lee la configuración de firma; nombra lo que falta pero nunca imprime contraseñas
    static bool TryReadSigningEnv(out string keystorePath, out string storePass, out string alias, out string keyPass)
    {
        keystorePath = System.Environment.GetEnvironmentVariable("CANNONS_KEYSTORE_PATH");
        storePass = System.Environment.GetEnvironmentVariable("CANNONS_KEYSTORE_PASS");
        alias = System.Environment.GetEnvironmentVariable("CANNONS_KEY_ALIAS");
        keyPass = System.Environment.GetEnvironmentVariable("CANNONS_KEY_PASS");

        var missing = new System.Collections.Generic.List<string>();
        if (string.IsNullOrEmpty(keystorePath)) missing.Add("CANNONS_KEYSTORE_PATH");
        if (string.IsNullOrEmpty(storePass)) missing.Add("CANNONS_KEYSTORE_PASS");
        if (string.IsNullOrEmpty(alias)) missing.Add("CANNONS_KEY_ALIAS");
        if (string.IsNullOrEmpty(keyPass)) missing.Add("CANNONS_KEY_PASS");
        if (missing.Count > 0)
        {
            Debug.LogError($"[Build AAB] Cancelado: faltan variables de entorno {string.Join(", ", missing)}.");
            return false;
        }
        if (!File.Exists(keystorePath))
        {
            Debug.LogError($"[Build AAB] Cancelado: no existe el keystore en {keystorePath}.");
            return false;
        }
        return true;
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
