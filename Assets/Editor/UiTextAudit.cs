using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Lista todos los textos de UI (TMP y Text legacy, también los inactivos) y
// los botones de las escenas del build, con su ruta en la jerarquía. Sirve
// para revisar la traducción (Loc): todo texto visible tiene que estar en la
// tabla o ponerse por código. Salida: Logs/ui_texts.txt (Logs/ no va a git).
// Headless: Unity -batchmode -quit -executeMethod UiTextAudit.RunHeadless
public static class UiTextAudit
{
    const string OutputPath = "Logs/ui_texts.txt";

    [MenuItem("Dev/Audit UI Texts")]
    static void RunFromMenu()
    {
        // abrir otras escenas descarta cambios sin guardar: se pregunta antes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Run();
        EditorUtility.RevealInFinder(OutputPath);
    }

    public static void RunHeadless()
    {
        Run();
        EditorApplication.Exit(0);
    }

    static void Run()
    {
        var report = new StringBuilder();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            report.AppendLine($"== {scene.path}");
            AppendScene(report);
        }
        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
        File.WriteAllText(OutputPath, report.ToString(), Encoding.UTF8);
        Debug.Log($"[UiTextAudit] escrito {OutputPath}");
    }

    static void AppendScene(StringBuilder report)
    {
        // true = incluye objetos inactivos (paneles de victoria/pausa empiezan ocultos)
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
                report.AppendLine(Line("TMP ", tmp.gameObject, tmp.text));
            foreach (var legacy in root.GetComponentsInChildren<Text>(true))
                report.AppendLine(Line("TEXT", legacy.gameObject, legacy.text));
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                // un botón sin texto propio puede llevar la palabra dibujada en el sprite
                var image = button.GetComponent<Image>();
                string sprite = image != null && image.sprite != null ? image.sprite.name : "-";
                report.AppendLine(Line("BTN ", button.gameObject, $"sprite={sprite}"));
            }
            // a qué objetos apuntan los campos de los scripts de UI (cablear mal un
            // texto es invisible en la escena; acá se ve la ruta real)
            foreach (var ui in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (ui is WinUI || ui is MainMenu || ui is LvlDisplay)
                    AppendReferences(report, ui);
        }
    }

    static void AppendReferences(StringBuilder report, MonoBehaviour ui)
    {
        var property = new SerializedObject(ui).GetIterator();
        // NextVisible(true) entra al primer nivel; después se recorre sin entrar a hijos
        for (bool more = property.NextVisible(true); more; more = property.NextVisible(false))
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script")
                continue;
            var target = property.objectReferenceValue;
            string where = target is Component c ? PathOf(c.transform) + $" ({c.GetType().Name})"
                         : target is GameObject g ? PathOf(g.transform) : "null";
            report.AppendLine($"REF  {ui.GetType().Name}.{property.name} -> {where}");
        }
    }

    static string Line(string kind, GameObject go, string content)
    {
        string state = go.activeInHierarchy ? "on " : "off";
        string flat = (content ?? "").Replace("\n", "\\n");
        return $"{kind} {state} {PathOf(go.transform)} | {flat}";
    }

    static string PathOf(Transform t)
    {
        var parts = new List<string>();
        for (; t != null; t = t.parent) parts.Insert(0, t.name);
        return string.Join("/", parts);
    }
}
