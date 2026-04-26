using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = System.Random;

public static class LevelGeneratorMass
{
    const string LevelsFolder = "Assets/Levels";
    const string DatabasePath  = "Assets/Levels/LevelDatabase.asset";
    const int    TotalLevels   = 500;
    const int    Seed          = 1337; // cambiar para otra distribucion aleatoria

    // ─── Config por tier ──────────────────────────────────────────────────

    struct Config
    {
        public int   minWaves, maxWaves;
        public int   minCols,  maxCols;
        public int   minHP,    maxHP;
        public int   maxPerWave;
        public float emptyChance;
        public bool  isHard;
        // maxHP >= 4 habilita piratas que requieren merge (cannonDamage 2)
    }

    // tier 0=Bajo, 1=Manejable, 2=Medio, 3=Alto, 4=Extremo
    static readonly Config[] Tiers =
    {
        new Config { minWaves=4, maxWaves=6,  minCols=1, maxCols=2, minHP=1, maxHP=1, maxPerWave=2, emptyChance=0.35f, isHard=false },
        new Config { minWaves=5, maxWaves=8,  minCols=2, maxCols=3, minHP=1, maxHP=2, maxPerWave=3, emptyChance=0.20f, isHard=false },
        new Config { minWaves=6, maxWaves=9,  minCols=3, maxCols=5, minHP=1, maxHP=3, maxPerWave=4, emptyChance=0.15f, isHard=false },
        new Config { minWaves=8, maxWaves=10, minCols=4, maxCols=5, minHP=1, maxHP=4, maxPerWave=5, emptyChance=0.10f, isHard=true  },
        new Config { minWaves=8, maxWaves=12, minCols=5, maxCols=5, minHP=2, maxHP=4, maxPerWave=5, emptyChance=0.05f, isHard=true  },
    };

    // ─── Curva de dificultad no lineal ────────────────────────────────────

    // rampa suave + ruido aleatorio + breaks periodicos de Bajo
    static int PickTier(int idx, Random rng)
    {
        if (idx > 0 && idx % 14 == 0) return 0; // break Bajo cada 14 niveles

        float t     = (float)idx / TotalLevels;
        float score = Mathf.Pow(t, 0.55f) * 4f;                    // 0..4 con curva raiz
        score      += (float)(rng.NextDouble() - 0.5) * 1.2f;      // ruido ±0.6 tiers
        return Mathf.Clamp(Mathf.RoundToInt(score), 0, 4);
    }

    // ─── Passwords ────────────────────────────────────────────────────────

    static string MakePassword(HashSet<string> used, Random rng)
    {
        string pw;
        do { pw = $"{(char)('A' + rng.Next(26))}{rng.Next(1000, 10000)}"; }
        while (used.Contains(pw));
        used.Add(pw);
        return pw;
    }

    // ─── Construccion de nivel ────────────────────────────────────────────

    static Level BuildLevel(int levelNumber, string password, Config cfg, Random rng)
    {
        int numWaves = rng.Next(cfg.minWaves, cfg.maxWaves + 1);
        int numCols  = rng.Next(cfg.minCols,  cfg.maxCols  + 1);

        // columnas activas: subconjunto aleatorio de 0-4
        var pool = new List<int> { 0, 1, 2, 3, 4 };
        Shuffle(pool, rng);
        var activeCols = pool.GetRange(0, numCols);

        // columnas que requieren merge (cannonDamage=2) — solo en tiers 3 y 4
        var mergedCols = new HashSet<int>();
        if (cfg.maxHP >= 4)
        {
            int maxMerges = Mathf.Max(0, numWaves - numCols); // merges que caben en las rondas disponibles
            int merges    = rng.Next(0, Mathf.Min(maxMerges, numCols) + 1);
            var shuffled  = new List<int>(activeCols);
            Shuffle(shuffled, rng);
            for (int m = 0; m < merges; m++) mergedCols.Add(shuffled[m]);
        }

        // nextFreeShot[col] = indice de disparo en que esa columna queda libre
        // garantiza que el HP generado sea siempre beatable por construccion
        int[] nextFreeShot = new int[5];

        var filas = new List<Level.Fila>();

        for (int w = 0; w < numWaves; w++)
        {
            bool isLast   = w == numWaves - 1;
            bool skipWave = !isLast && rng.NextDouble() < cfg.emptyChance;

            var cuadros = new List<Level.Cuadro>();

            if (!skipWave)
            {
                var waveCols = new List<int>(activeCols);
                Shuffle(waveCols, rng);
                int count = rng.Next(1, Mathf.Min(cfg.maxPerWave, waveCols.Count) + 1);

                for (int k = 0; k < count; k++)
                {
                    int col = waveCols[k];
                    int dmg = mergedCols.Contains(col) ? 2 : 1;

                    // ventana de disparos disponibles para este pirata
                    int firstShot  = Mathf.Max(w + 1, nextFreeShot[col]);
                    int lastShot   = w + 3; // 3 avances antes de game over
                    int maxShots   = lastShot - firstShot + 1;
                    if (maxShots <= 0) continue; // columna bloqueada, no cabe pirata aqui

                    int maxHP = Mathf.Min(cfg.maxHP, maxShots * dmg);
                    int minHP = Mathf.Min(cfg.minHP, maxHP);
                    if (maxHP < 1) continue;

                    int hp = rng.Next(minHP, maxHP + 1);

                    // sesgo hacia HP alto en tiers dificiles (re-roll hacia arriba 40% del tiempo)
                    if (cfg.maxHP >= 3 && hp < maxHP && rng.NextDouble() < 0.4f)
                        hp = rng.Next(hp, maxHP + 1);

                    cuadros.Add(new Level.Cuadro
                    {
                        index = col,
                        tipo  = rng.Next(1, 4), // 1, 2 o 3 — tipo 4 se asigna al final
                        hp    = hp,
                    });

                    // actualiza cuando queda libre esta columna
                    nextFreeShot[col] = firstShot + Mathf.CeilToInt((float)hp / dmg);
                }
            }

            filas.Add(new Level.Fila { cuadros = cuadros });
        }

        // fallback: si no se genero ningun pirata, agrega uno minimo
        if (!HasPirates(filas))
            filas[0].cuadros.Add(new Level.Cuadro { index = activeCols[0], tipo = 1, hp = 1 });

        MarkLastPirate(filas); // el ultimo pirata usa tipo 4 (skin especial)

        var level = ScriptableObject.CreateInstance<Level>();
        level.levelNumber = levelNumber;
        level.password    = password;
        level.isHard      = cfg.isHard;
        level.filas       = filas;
        return level;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    static bool HasPirates(List<Level.Fila> filas)
    {
        foreach (var f in filas)
            if (f.cuadros != null && f.cuadros.Count > 0) return true;
        return false;
    }

    // recorre de atras para adelante y pone tipo=4 al ultimo pirata generado
    static void MarkLastPirate(List<Level.Fila> filas)
    {
        for (int w = filas.Count - 1; w >= 0; w--)
        {
            var c = filas[w].cuadros;
            if (c != null && c.Count > 0) { c[c.Count - 1].tipo = 4; return; }
        }
    }

    static void Shuffle<T>(List<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ─── Entry point ──────────────────────────────────────────────────────

    [MenuItem("Levels/Generate All Levels")]
    static void GenerateAllLevels()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder))
            AssetDatabase.CreateFolder("Assets", "Levels");

        // limpia archivo del generador anterior si existe
        AssetDatabase.DeleteAsset($"{LevelsFolder}/Level_01.asset");

        var rng       = new Random(Seed);
        var passwords = new HashSet<string>();
        var levels    = new Level[TotalLevels];

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < TotalLevels; i++)
            {
                EditorUtility.DisplayProgressBar(
                    "Generando niveles",
                    $"Nivel {i + 1} / {TotalLevels}",
                    (float)i / TotalLevels);

                int    tier  = PickTier(i, rng);
                string pw    = MakePassword(passwords, rng);
                Level  level = BuildLevel(i + 1, pw, Tiers[tier], rng);

                string path = $"{LevelsFolder}/Level_{i + 1:D3}.asset";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(level, path);
                levels[i] = level;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
        }

        // actualiza la base de datos
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(db, DatabasePath);
        }
        db.levels = levels;
        EditorUtility.SetDirty(db);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[LevelGeneratorMass] {TotalLevels} niveles generados.");
    }
}
