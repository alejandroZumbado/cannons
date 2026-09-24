using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Arma el panel "Opciones" de la pausa (escena Game): overlay a pantalla completa
// con un slider de volumen general (VolumeSlider) y un botón "Cerrar".
// Existe porque PauseUI.optionsPanel estaba sin asignar y el botón Opciones no hacía nada.
// Idempotente: si el panel ya está asignado, no toca nada.
// Headless: Unity -batchmode -quit -executeMethod PauseOptionsSetup.RunHeadless (exit 1 si falla).
public static class PauseOptionsSetup
{
    private const string GameScenePath = "Assets/Scenes/Game.unity";
    private const string SliderPrefabPath = "Assets/Unity UI Samples/Prefabs/SF Slider.prefab";
    private const string PanelName = "OptionsPanel";

    [MenuItem("Dev/Setup Pause Volume Panel")]
    static void RunFromMenu()
    {
        // abrir Game descarta cambios sin guardar de la escena actual: preguntar antes
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string result = Setup(out _);
        EditorUtility.DisplayDialog("Pause Volume Panel", result, "OK");
    }

    public static void RunHeadless()
    {
        string result = Setup(out bool ok);
        Debug.Log("[PauseOptionsSetup] " + result);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    // devuelve un mensaje legible; ok=false solo si algo falló de verdad
    static string Setup(out bool ok)
    {
        ok = false;
        var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        // PauseUI se desactiva en Awake, pero en el editor el objeto puede estar inactivo: incluir inactivos
        var pauseUI = Object.FindFirstObjectByType<PauseUI>(FindObjectsInactive.Include);
        if (pauseUI == null) return $"No se encontró PauseUI en {GameScenePath}.";

        var so = new SerializedObject(pauseUI);
        var panelProp = so.FindProperty("optionsPanel");
        if (panelProp.objectReferenceValue != null)
        {
            ok = true;
            return "optionsPanel ya estaba asignado — no se cambió nada.";
        }

        var sliderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SliderPrefabPath);
        if (sliderPrefab == null) return $"No se encontró el prefab {SliderPrefabPath}.";

        GameObject panel = CreateOverlay(pauseUI.transform);
        RectTransform box = CreateBox(panel.transform);
        CreateLabel(box, "Volumen");
        CreateSlider(box, sliderPrefab);
        CreateCloseButton(box, pauseUI);

        panel.SetActive(false); // PauseUI.OnOptionsPressed lo alterna
        panelProp.objectReferenceValue = panel;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) return "No se pudo guardar la escena Game.";

        ok = true;
        return "Panel de opciones creado y asignado. Revisalo visualmente en la escena Game.";
    }

    // fondo oscuro a pantalla completa; último hijo = se dibuja encima de los botones de pausa
    static GameObject CreateOverlay(Transform parent)
    {
        var go = new GameObject(PanelName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.transform.SetAsLastSibling();
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f); // bloquea clics a la pausa de atrás
        return go;
    }

    // caja central que agrupa título, slider y botón (layout vertical automático)
    static RectTransform CreateBox(Transform parent)
    {
        var go = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700f, 360f);
        go.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.08f, 0.95f);

        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 40, 40);
        layout.spacing = 30f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return rt;
    }

    static void CreateLabel(Transform parent, string text)
    {
        var go = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = new Vector2(0f, 70f);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 48f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    // usa el slider estilizado del pack UI Samples (mismo que el menú) + VolumeSlider
    static void CreateSlider(Transform parent, GameObject prefab)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = "Master Volume Slider";
        ((RectTransform)go.transform).sizeDelta = new Vector2(0f, 40f);
        go.AddComponent<VolumeSlider>();
    }

    // "Cerrar" llama a PauseUI.OnOptionsPressed (persistente: queda guardado en la escena)
    static void CreateCloseButton(Transform parent, PauseUI pauseUI)
    {
        var go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        go.name = "Close Button";
        go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = new Vector2(0f, 80f);

        var label = go.GetComponentInChildren<TextMeshProUGUI>();
        label.text = "Cerrar";
        label.fontSize = 40f;

        var button = go.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(pauseUI.OnOptionsPressed));
    }
}
