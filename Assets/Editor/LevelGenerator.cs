using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class LevelGenerator
{
    private const string LevelsFolder = "Assets/Levels";
    private const string DatabasePath = "Assets/Levels/LevelDatabase.asset";

    [MenuItem("Levels/Generate Intro Level")]
    static void GenerateIntroLevel()
    {
        EnsureFolder();

        Level level = ScriptableObject.CreateInstance<Level>();
        level.levelNumber = 1;
        level.password = "B2341";
        level.filas = new List<Level.Fila>
        {
            // Fila 1: 1 pirata centro
            MakeFila((2, 1, 1)),

            // Fila 2: 2 piratas internos
            MakeFila((3, 2, 1), (1, 3, 1)),

            // Fila 3: vacia — respiro
            MakeFila(),

            // Fila 4: HP2 centro, vuelve la presion
            MakeFila((2, 1, 2)),

            // Fila 5: flancos suaves
            MakeFila((4, 2, 1), (0, 3, 1)),

            // Fila 6: cierre — tipo4 en el ultimo pirata
            MakeFila((3, 1, 1), (1, 4, 1)),
        };

        string path = $"{LevelsFolder}/Level_01.asset";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(level, path);

        AddToDatabase(level);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level_01 generado en Assets/Levels/");
    }

    // construye una Fila con los cuadros indicados como (index, tipo, hp)
    static Level.Fila MakeFila(params (int index, int tipo, int hp)[] entries)
    {
        var fila = new Level.Fila { cuadros = new List<Level.Cuadro>() };
        foreach (var e in entries)
            fila.cuadros.Add(new Level.Cuadro { index = e.index, tipo = e.tipo, hp = e.hp });
        return fila;
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
            // reemplaza o agrega segun levelNumber
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
