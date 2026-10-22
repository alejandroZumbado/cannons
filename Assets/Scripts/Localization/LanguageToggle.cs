using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Botón de idioma en la esquina del menú (2026-10-20). Se crea solo al cargar
// la escena Menu (mismo patrón que IntroVideo/TutorialHints): no hay objetos
// en la escena que mantener. Muestra el idioma al que cambia ("Español" /
// "English") y llama Loc.Toggle; los textos visibles se re-pintan solos.
public class LanguageToggle : MonoBehaviour
{
    const string MenuScene = "Menu";
    const int SortingOrder = 10;   // sobre el menú (0-1), debajo de la intro (32000)
    static readonly Vector2 ButtonSize = new Vector2(240, 80);

    TMP_Text label;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            // el objeto vive en la escena: se destruye solo al salir del menú
            if (scene.name == MenuScene)
                new GameObject("LanguageToggle").AddComponent<LanguageToggle>();
        };
    }

    void Start()
    {
        BuildUI();
        Loc.Changed += Refresh;
        Refresh();
    }

    void OnDestroy() => Loc.Changed -= Refresh;

    void Refresh() => label.text = Loc.T("menu.language");

    // ─── UI por código (UiFactory: mismo escalado y tabla de madera que el resto, desde 10-21) ───

    void BuildUI()
    {
        RectTransform canvas = UiFactory.CreateOverlayCanvas("LanguageCanvas", SortingOrder, clickable: true, transform);
        Button button = UiFactory.CreateButton(canvas, "LanguageButton", ButtonSize, Color.white, 40f, Loc.Toggle, out label);
        // esquina superior derecha
        UiFactory.Anchor((RectTransform)button.transform, new Vector2(1f, 1f), new Vector2(-30, -30), ButtonSize);
    }
}
