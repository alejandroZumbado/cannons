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

    // ─── UI por código (overlay propio, mismo escalado que el resto: 1920x1080, match 1) ───

    void BuildUI()
    {
        var canvasGo = new GameObject("LanguageCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        var buttonGo = new GameObject("LanguageButton", typeof(Image), typeof(Button));
        var rt = buttonGo.GetComponent<RectTransform>();
        rt.SetParent(canvasGo.transform, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f); // esquina superior derecha
        rt.anchoredPosition = new Vector2(-30, -30);
        rt.sizeDelta = new Vector2(240, 80);
        buttonGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        buttonGo.GetComponent<Button>().onClick.AddListener(Loc.Toggle);

        var textGo = new GameObject("Text", typeof(TextMeshProUGUI));
        var trt = textGo.GetComponent<RectTransform>();
        trt.SetParent(rt, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        label = textGo.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 40;
        label.color = Color.white;
        label.raycastTarget = false; // el clic lo recibe la Image del botón
    }
}
