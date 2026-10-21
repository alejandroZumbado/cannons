using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Clic sonoro en TODOS los botones (2026-10-20), sin tocar escenas ni prefabs:
// al cargar cada escena se engancha a cada Button (incluidos los inactivos).
// Botones creados por código después pueden llamar Hook(button).
public class UiClickSounds : MonoBehaviour
{
    // botones ya enganchados (por ID de instancia): evita doble sonido en los
    // que sobreviven entre escenas (DontDestroyOnLoad). Los IDs no se reutilizan.
    static readonly HashSet<int> Hooked = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded += (scene, _) =>
            new GameObject("UiClickSounds").AddComponent<UiClickSounds>(); // vive en la escena: muere con ella
    }

    // espera un frame: así también entran los botones que otros scripts crean en su Start
    IEnumerator Start()
    {
        yield return null;
        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Hook(button);
    }

    public static void Hook(Button button)
    {
        if (button == null || !Hooked.Add(button.GetInstanceID())) return;
        button.onClick.AddListener(() => Sfx.Play(SfxId.Click));
    }
}
