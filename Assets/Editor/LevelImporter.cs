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
public static class LevelImporter
{
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
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(level, assetPath);
            AddToDatabase(level);

            string dest = Path.Combine(processedPath, Path.GetFileName(file));
            File.Delete(dest); // overwrite if a same-named file was processed before
            File.Move(file, dest);

            imported++;
            Debug.Log($"Importado: {assetPath} (nivel {json.levelNumber}, password {json.password})");
        }

        if (imported > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        Debug.Log($"Importación completa: {imported} nivel(es) agregado(s) a LevelDatabase.");
    }

    static void AddToDatabase(Level level)
    {
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LevelDatabase>();
            db.levels = new Level[] { level };
            AssetDatabase.CreateAsset(db, DatabasePath);
        }
        else
        {
            var list = new List<Level>(db.levels ?? new Level[0]);
            int existing = list.FindIndex(l => l != null && l.levelNumber == level.levelNumber);
            if (existing >= 0)
                list[existing] = level;
            else
                list.Add(level);

            list.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
            db.levels = list.ToArray();
            EditorUtility.SetDirty(db);
        }
    }

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder))
            AssetDatabase.CreateFolder("Assets", "Levels");
    }
}
