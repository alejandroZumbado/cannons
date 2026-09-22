using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Revisa que el lanzamiento (LevelDatabase) y los niveles en reserva estén
// sanos antes de publicar: sin referencias rotas, sin IDs ni passwords
// repetidos (un password repetido haría que TryPassword abra el nivel
// equivocado), passwords con formato válido, y datos de cada nivel dentro de
// los rangos que el juego soporta.
//
// Errores = el juego se rompe o se comporta mal. Advertencias = raro pero
// jugable. Headless: Unity -batchmode -quit -executeMethod ReleaseValidator.RunHeadless
// (sale con código 1 si hay errores).
public static class ReleaseValidator
{
    private const string DatabasePath = "Assets/Levels/LevelDatabase.asset";
    private const string LevelsFolder = "Assets/Levels";
    private static readonly Regex PasswordFormat = new Regex(@"^[A-Z][0-9]{4}$");

    [MenuItem("Levels/Validate Release")]
    static void RunFromMenu()
    {
        bool ok = Validate(out int errors, out int warnings);
        EditorUtility.DisplayDialog("Validate Release",
            ok ? $"OK — 0 errores, {warnings} advertencia(s). Detalle en la Console."
               : $"{errors} error(es), {warnings} advertencia(s). Detalle en la Console.",
            "OK");
    }

    // para CI / línea de comandos: código de salida 1 si hay errores
    public static void RunHeadless()
    {
        bool ok = Validate(out _, out _);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    // true si no hay errores (las advertencias no bloquean)
    public static bool Validate(out int errorCount, out int warningCount)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        if (db == null || db.levels == null || db.levels.Length == 0)
        {
            errors.Add($"{DatabasePath} no existe o está vacío.");
        }
        else
        {
            CheckRelease(db, errors, warnings);
        }

        CheckAllAssets(errors, warnings);

        foreach (string w in warnings) Debug.LogWarning("[ReleaseValidator] " + w);
        foreach (string e in errors) Debug.LogError("[ReleaseValidator] " + e);
        Debug.Log($"[ReleaseValidator] Lanzamiento: {db?.Count ?? 0} niveles — " +
                  $"{errors.Count} error(es), {warnings.Count} advertencia(s).");

        errorCount = errors.Count;
        warningCount = warnings.Count;
        return errors.Count == 0;
    }

    // chequeos del lanzamiento en sí (lo que el jugador puede jugar)
    static void CheckRelease(LevelDatabase db, List<string> errors, List<string> warnings)
    {
        var seenIds = new Dictionary<int, int>();
        for (int i = 0; i < db.levels.Length; i++)
        {
            Level level = db.levels[i];
            string where = $"Posición {i + 1}";
            if (level == null)
            {
                errors.Add($"{where}: referencia rota (asset borrado o GUID cambiado).");
                continue;
            }
            if (seenIds.TryGetValue(level.levelNumber, out int firstPos))
                errors.Add($"{where}: levelNumber {level.levelNumber} repetido (ya está en la posición {firstPos}).");
            else
                seenIds[level.levelNumber] = i + 1;

            CheckLevelData(level, $"{where} (levelNumber {level.levelNumber})", errors, warnings);
        }
    }

    // chequeos sobre TODOS los assets (lanzamiento + reserva): IDs y passwords
    // deben ser únicos globalmente para que un nivel de reserva pueda entrar
    // al lanzamiento más adelante sin chocar con otro
    static void CheckAllAssets(List<string> errors, List<string> warnings)
    {
        var levels = AssetDatabase.FindAssets("t:Level", new[] { LevelsFolder })
            .Select(guid => AssetDatabase.LoadAssetAtPath<Level>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(l => l != null)
            .ToList();

        foreach (var group in levels.GroupBy(l => l.levelNumber).Where(g => g.Count() > 1))
            errors.Add($"levelNumber {group.Key} usado por varios assets: " +
                       string.Join(", ", group.Select(AssetDatabase.GetAssetPath)));

        foreach (var group in levels.Where(l => !string.IsNullOrEmpty(l.password))
                                    .GroupBy(l => l.password.Trim().ToUpperInvariant())
                                    .Where(g => g.Count() > 1))
            errors.Add($"Password {group.Key} repetido en niveles " +
                       string.Join(", ", group.Select(l => l.levelNumber)));
    }

    // rangos que el juego soporta (ver CLAUDE.md "Level data model")
    static void CheckLevelData(Level level, string where, List<string> errors, List<string> warnings)
    {
        if (string.IsNullOrEmpty(level.password))
            errors.Add($"{where}: sin password.");
        else if (!PasswordFormat.IsMatch(level.password.Trim().ToUpperInvariant()))
            errors.Add($"{where}: password '{level.password}' no cumple el formato letra + 4 dígitos.");

        if (level.filas == null || level.filas.Count == 0)
        {
            errors.Add($"{where}: no tiene rondas.");
            return;
        }

        int pirates = 0;
        for (int f = 0; f < level.filas.Count; f++)
        {
            var cuadros = level.filas[f]?.cuadros;
            if (cuadros == null || cuadros.Count(c => c.tipo >= 1) == 0)
            {
                warnings.Add($"{where}: ronda {f + 1} no tiene piratas.");
                continue;
            }
            var usedColumns = new HashSet<int>();
            foreach (var c in cuadros)
            {
                if (c.tipo < 1) continue;
                pirates++;
                if (c.index < 0 || c.index > 4)
                    errors.Add($"{where}: ronda {f + 1} tiene columna {c.index} (válido 0-4).");
                if (c.hp < 1 || c.hp > 10)
                    errors.Add($"{where}: ronda {f + 1} tiene hp {c.hp} (válido 1-10).");
                if (c.tipo > 5)
                    errors.Add($"{where}: ronda {f + 1} tiene tipo {c.tipo} (válido 0-5).");
                if (!usedColumns.Add(c.index))
                    errors.Add($"{where}: ronda {f + 1} tiene dos piratas en la columna {c.index}.");
            }
        }
        if (pirates == 0)
            errors.Add($"{where}: no tiene ningún pirata (se ganaría sin jugar).");
    }
}
