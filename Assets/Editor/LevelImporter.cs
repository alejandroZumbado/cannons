using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Imports Level JSON files dropped by CannonsLevelGen into Assets/Levels/,
// following the exact same AssetDatabase pattern as LevelGenerator.cs.
// Source folder lives OUTSIDE Assets/ on purpose (GeneratedLevels/incoming at
// the repo root, tracked in git so each drop is reviewable as a commit diff)
// so raw AI output never lands in the asset database until this explicit
// step runs.
//
// [InitializeOnLoad] added 2026-09-05: importing was easy to forget entirely
// (Level_501 sat unimported for 2 weeks) since nothing surfaced that new
// levels had arrived short of manually checking the incoming/ folder. This
// just logs a reminder on every Editor load/recompile — it does not import
// automatically, since a human should still eyeball each AI-generated level
// before it becomes part of the real game.
[InitializeOnLoad]
public static class LevelImporter
{
    static LevelImporter()
    {
        EditorApplication.delayCall += WarnIfLevelsPending;
    }

    static void WarnIfLevelsPending()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string incomingPath = Path.Combine(projectRoot, IncomingFolder);
        if (!Directory.Exists(incomingPath)) return;

        string[] files = Directory.GetFiles(incomingPath, "*.json");
        if (files.Length == 0) return;

        Debug.LogWarning($"CannonsLevelGen: {files.Length} nivel(es) generado(s) esperando revisión en " +
                          $"{IncomingFolder}/ — Levels > Import Generated Levels (JSON) para importarlos " +
                          "(revisa cada uno antes, esto no importa nada automáticamente).");
    }

    private const string LevelsFolder = "Assets/Levels";
    private const string DatabasePath = "Assets/Levels/LevelDatabase.asset";
    private const string IncomingFolder = "GeneratedLevels/incoming";
    private const string ProcessedFolder = "GeneratedLevels/processed";

    [Serializable]
    private class CuadroJson { public int index; public int tipo; public int hp; }
    [Serializable]
    private class FilaJson { public List<CuadroJson> cuadros; }
    [Serializable]
    private class LevelJson
    {
        public int levelNumber;
        public string password;
        public bool isHard;
        public List<FilaJson> filas;
    }

    [MenuItem("Levels/Import Generated Levels (JSON)")]
    static void ImportGeneratedLevels()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string incomingPath = Path.Combine(projectRoot, IncomingFolder);
        string processedPath = Path.Combine(projectRoot, ProcessedFolder);

        if (!Directory.Exists(incomingPath))
        {
            Debug.Log($"No hay carpeta {IncomingFolder} — nada que importar.");
            return;
        }
        Directory.CreateDirectory(processedPath);

        string[] files = Directory.GetFiles(incomingPath, "*.json");
        if (files.Length == 0)
        {
            Debug.Log("No hay niveles nuevos en GeneratedLevels/incoming/.");
            return;
        }

        EnsureFolder();
        int imported = 0;

        foreach (string file in files)
        {
            LevelJson json;
            try
            {
                json = JsonUtility.FromJson<LevelJson>(File.ReadAllText(file));
            }
            catch (Exception e)
            {
                Debug.LogError($"No se pudo parsear {Path.GetFileName(file)}: {e.Message}");
                continue;
            }

            Level level = ScriptableObject.CreateInstance<Level>();
            level.levelNumber = json.levelNumber;
            level.password = json.password;
            level.isHard = json.isHard;
            level.filas = json.filas.Select(f => new Level.Fila
            {
                cuadros = f.cuadros.Select(c => new Level.Cuadro { index = c.index, tipo = c.tipo, hp = c.hp }).ToList()
            }).ToList();

            string assetPath = $"{LevelsFolder}/Level_{json.levelNumber:D3}_generated.asset";
            // posición en el lanzamiento ANTES de borrar: después del DeleteAsset
            // la referencia vieja ya es null y no se podría encontrar
            int releaseIndex = FindInDatabase(json.levelNumber);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(level, assetPath);
            bool inRelease = releaseIndex >= 0;
            if (inRelease) ReplaceInDatabase(releaseIndex, level);

            string dest = Path.Combine(processedPath, Path.GetFileName(file));
            File.Delete(dest); // overwrite if a same-named file was processed before
            File.Move(file, dest);

            imported++;
            Debug.Log(inRelease
                ? $"Importado: {assetPath} (nivel {json.levelNumber}, password {json.password}) — reemplazado en su posición del LevelDatabase."
                : $"Importado: {assetPath} (nivel {json.levelNumber}, password {json.password}) — queda en RESERVA (no está en LevelDatabase).");
        }

        if (imported > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        Debug.Log($"Importación completa: {imported} nivel(es) importado(s). Los nuevos quedan en reserva " +
                  "hasta que la curación del lanzamiento (CannonsLevelGen verification/extend_release.py) les asigne posición.");

        // valida todo tras importar: un password repetido o un nivel mal
        // formado se detecta acá y no cuando un jugador lo encuentra
        if (imported > 0)
            ReleaseValidator.Validate(out _, out _);
    }

    // Desde el lanzamiento curado (2026-09-22) LevelDatabase = SOLO los niveles
    // del lanzamiento, en su orden de dificultad (no por levelNumber). Por eso
    // un nivel importado que ya está en el lanzamiento se reemplaza en la misma
    // posición (DeleteAsset+CreateAsset le da un GUID nuevo), y uno nuevo NO se
    // agrega: queda como asset suelto en reserva. Agregarlo y reordenar por
    // levelNumber (lo que hacía antes) destruiría el orden curado.

    // índice del nivel en LevelDatabase, o -1 si no está (o no hay database)
    static int FindInDatabase(int levelNumber)
    {
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        if (db == null || db.levels == null)
        {
            Debug.LogError($"No se encontró {DatabasePath} — el nivel {levelNumber} queda solo como asset.");
            return -1;
        }
        return Array.FindIndex(db.levels, l => l != null && l.levelNumber == levelNumber);
    }

    // apunta la posición `index` del lanzamiento al asset recién creado
    static void ReplaceInDatabase(int index, Level level)
    {
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        db.levels[index] = level;
        EditorUtility.SetDirty(db);
    }

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder))
            AssetDatabase.CreateFolder("Assets", "Levels");
    }
}
