using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Visual grid editor for reviewing/adjusting Level assets directly in the
// Unity Editor. Built 2026-09-15/16 right after applying 337 levels' worth
// of automated edits (repair, rebalance, fill-empty-rounds — see CLAUDE.md's
// "AI-generated levels pipeline" section and CannonsLevelGen) so the current
// state of any level (post-edit) can be reviewed and hand-tuned without
// leaving Unity or hand-editing YAML.
//
// Reads/writes the SAME Level assets LevelDatabase already references —
// this is a browser+editor for what's already there, not a generator. Any
// edit here is applied to the loaded Level asset immediately (so the
// in-memory state always reflects what you see), but only written to disk
// when you press "Guardar cambios" (EditorUtility.SetDirty +
// AssetDatabase.SaveAssets) — closing the window or selecting a different
// level without saving discards unsaved edits on the next Editor reload,
// same as any other unsaved asset change in Unity.
public class LevelGridEditor : EditorWindow
{
    private const string DatabasePath = "Assets/Levels/LevelDatabase.asset";
    private const int NumColumns = 5;

    private LevelDatabase _database;
    private Vector2 _listScroll;
    private Vector2 _detailScroll;
    private string _search = "";
    private Level _selected;
    private bool _dirty;

    [MenuItem("Levels/Level Grid Editor")]
    public static void Open()
    {
        var window = GetWindow<LevelGridEditor>("Level Grid Editor");
        window.minSize = new Vector2(720, 420);
        window.LoadDatabase();
    }

    private void OnEnable()
    {
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        _database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
    }

    private void OnGUI()
    {
        if (_database == null)
        {
            EditorGUILayout.HelpBox($"No se encontró LevelDatabase en {DatabasePath}.", MessageType.Warning);
            if (GUILayout.Button("Reintentar")) LoadDatabase();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        DrawLevelList();
        DrawSeparator();
        DrawDetail();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSeparator()
    {
        GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));
    }

    private void DrawLevelList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(230));

        EditorGUILayout.LabelField($"Niveles ({_database.Count})", EditorStyles.boldLabel);
        _search = EditorGUILayout.TextField("Buscar", _search);

        _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

        var levels = (_database.levels ?? new Level[0])
            .Where(l => l != null)
            .OrderBy(l => l.levelNumber);

        foreach (var level in levels)
        {
            if (!string.IsNullOrEmpty(_search))
            {
                bool matchesNumber = level.levelNumber.ToString().Contains(_search);
                bool matchesPassword = !string.IsNullOrEmpty(level.password) &&
                                        level.password.ToUpper().Contains(_search.ToUpper());
                if (!matchesNumber && !matchesPassword) continue;
            }

            bool isSelected = _selected == level;
            GUI.backgroundColor = isSelected ? new Color(0.55f, 0.75f, 1f) : Color.white;
            string label = $"#{level.levelNumber}  {level.password}" + (level.isHard ? "  [HARD]" : "");
            if (GUILayout.Button(label, EditorStyles.miniButton))
            {
                if (_selected != level)
                {
                    _selected = level;
                    _dirty = false;
                }
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDetail()
    {
        EditorGUILayout.BeginVertical();

        if (_selected == null)
        {
            EditorGUILayout.HelpBox("Elegí un nivel de la lista a la izquierda.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);

        DrawHeader();
        EditorGUILayout.Space(8);
        DrawStats();
        EditorGUILayout.Space(8);
        DrawGrid();
        EditorGUILayout.Space(8);

        if (GUILayout.Button("+ Agregar ronda (fila)"))
        {
            _selected.filas.Add(new Level.Fila { cuadros = new List<Level.Cuadro>() });
            _dirty = true;
        }

        EditorGUILayout.Space(12);
        DrawSaveBar();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField($"Nivel #{_selected.levelNumber}", EditorStyles.largeLabel);

        EditorGUI.BeginChangeCheck();
        string newPassword = EditorGUILayout.TextField("Password", _selected.password);
        bool newIsHard = EditorGUILayout.Toggle("isHard", _selected.isHard);
        if (EditorGUI.EndChangeCheck())
        {
            _selected.password = newPassword;
            _selected.isHard = newIsHard;
            _dirty = true;
        }
    }

    private void DrawStats()
    {
        int totalPirates = 0;
        int maxHp = 0;
        var activeColumns = new HashSet<int>();
        int emptyFilas = 0;

        foreach (var fila in _selected.filas)
        {
            bool hasAny = false;
            foreach (var cuadro in fila.cuadros)
            {
                if (cuadro.tipo < 1) continue;
                hasAny = true;
                totalPirates++;
                if (cuadro.hp > maxHp) maxHp = cuadro.hp;
                activeColumns.Add(cuadro.index);
            }
            if (!hasAny) emptyFilas++;
        }

        EditorGUILayout.LabelField(
            $"{_selected.filas.Count} rondas · {totalPirates} piratas · HP máx {maxHp} · columnas activas {activeColumns.Count}",
            EditorStyles.miniLabel);

        if (emptyFilas > 0)
        {
            EditorGUILayout.HelpBox(
                $"{emptyFilas} ronda(s) sin ningún pirata — ronda muerta, ver CLAUDE.md ('Level design constraints'). " +
                "Agregale al menos 1 pirata con la celda '+' de abajo.",
                MessageType.Warning);
        }
    }

    private void DrawGrid()
    {
        // Column header: 0 = derecha ... 4 = izquierda, igual que el juego real.
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(50));
        for (int col = 0; col < NumColumns; col++)
            GUILayout.Label(col.ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(90));
        EditorGUILayout.EndHorizontal();

        for (int fi = 0; fi < _selected.filas.Count; fi++)
        {
            DrawFilaRow(fi);
        }
    }

    private void DrawFilaRow(int filaIndex)
    {
        var fila = _selected.filas[filaIndex];

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(50));
        GUILayout.Label($"F{filaIndex}", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("eliminar", EditorStyles.miniButton))
        {
            _selected.filas.RemoveAt(filaIndex);
            _dirty = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            return; // list changed under us — bail out of this row for this repaint
        }
        EditorGUILayout.EndVertical();

        for (int col = 0; col < NumColumns; col++)
        {
            DrawCell(fila, col);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCell(Level.Fila fila, int col)
    {
        Level.Cuadro cuadro = fila.cuadros.FirstOrDefault(c => c.index == col);

        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(90));

        if (cuadro == null)
        {
            GUILayout.Label("—", EditorStyles.centeredGreyMiniLabel);
            if (GUILayout.Button("+", GUILayout.Height(20)))
            {
                fila.cuadros.Add(new Level.Cuadro { index = col, tipo = 1, hp = 1 });
                _dirty = true;
            }
        }
        else
        {
            GUILayout.Label("HP", EditorStyles.centeredGreyMiniLabel);
            EditorGUI.BeginChangeCheck();
            int newHp = EditorGUILayout.IntField(cuadro.hp);
            GUILayout.Label("tipo", EditorStyles.centeredGreyMiniLabel);
            int newTipo = EditorGUILayout.IntField(cuadro.tipo);
            if (EditorGUI.EndChangeCheck())
            {
                cuadro.hp = Mathf.Clamp(newHp, 1, 10);
                cuadro.tipo = Mathf.Clamp(newTipo, 0, 5);
                _dirty = true;
            }

            if (GUILayout.Button("× quitar", GUILayout.Height(16)))
            {
                fila.cuadros.Remove(cuadro);
                _dirty = true;
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawSaveBar()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = _dirty;
        if (GUILayout.Button("Guardar cambios", GUILayout.Height(28)))
        {
            EditorUtility.SetDirty(_selected);
            AssetDatabase.SaveAssets();
            _dirty = false;
            Debug.Log($"Nivel #{_selected.levelNumber} guardado.");
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        if (_dirty)
        {
            EditorGUILayout.HelpBox(
                "Cambios sin guardar (solo en memoria) — el archivo .asset en disco todavía " +
                "no cambió. No hay botón de 'descartar'; si ya guardaste y te arrepentís, " +
                "usá git para revertir ese archivo.",
                MessageType.None);
        }
    }
}
