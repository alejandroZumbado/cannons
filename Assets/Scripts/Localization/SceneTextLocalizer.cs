using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Traduce los textos fijos de escenas y prefabs sin editarlos (2026-10-20):
// al cargar cada escena busca todo TMP_Text (también inactivos y placeholders
// de InputField) cuyo contenido coincida con una entrada de Loc, en cualquier
// idioma, y le agrega un LocalizedText con esa clave. Un texto nuevo en una
// escena solo necesita su entrada en la tabla de Loc.
// Los textos que pone el código (botón Continuar, avisos) no se tocan acá:
// esos usan Loc.T directamente.
public static class SceneTextLocalizer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        // BeforeSceneLoad: se registra antes de que cargue la primera escena,
        // así la primera también pasa por Localize
        SceneManager.sceneLoaded += (scene, _) => Localize(scene);
    }

    static void Localize(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
                Bind(label);
    }

    static void Bind(TMP_Text label)
    {
        if (label.GetComponent<LocalizedText>() != null) return; // ya enlazado (a mano o antes)
        if (string.IsNullOrWhiteSpace(label.text)) return;
        if (Loc.TryFindKey(label.text, out string key))
            label.gameObject.AddComponent<LocalizedText>().SetKey(key);
    }
}
