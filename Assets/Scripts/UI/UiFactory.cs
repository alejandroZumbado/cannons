using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Piezas de UI creadas por código (2026-10-20), con el mismo escalado que las
// escenas (1920x1080, match 1). Las usan las pantallas que no viven en las
// escenas (selector de niveles, números de daño, títulos de fin de nivel).
public static class UiFactory
{
    static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);

    // canvas overlay propio; `clickable` agrega el raycaster (sin él, la UI no recibe clics)
    public static RectTransform CreateOverlayCanvas(string name, int sortingOrder, bool clickable, Transform parent = null)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler));
        if (parent != null) go.transform.SetParent(parent, false);
        if (clickable) go.AddComponent<GraphicRaycaster>();

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 1f;
        return go.GetComponent<RectTransform>();
    }

    // rectángulo de color; blocksClicks = tapa lo de atrás (fondos de paneles modales)
    public static Image CreatePanel(RectTransform parent, string name, Color color, bool blocksClicks)
    {
        var go = new GameObject(name, typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = blocksClicks;
        return image;
    }

    // texto blanco centrado; font null = la fuente por defecto de TextMeshPro
    public static TMP_Text CreateLabel(RectTransform parent, string name, float fontSize, TMP_FontAsset font = null)
    {
        var go = new GameObject(name, typeof(TextMeshProUGUI));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false; // los textos nunca roban clics
        return label;
    }

    // botón con fondo y texto que lo llena; ya trae el sonido de clic
    public static Button CreateButton(RectTransform parent, string name, Vector2 size, Color color,
                                      float fontSize, UnityAction onClick, out TMP_Text label)
    {
        Image background = CreatePanel(parent, name, color, blocksClicks: true);
        background.rectTransform.sizeDelta = size;
        var button = background.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        UiClickSounds.Hook(button);

        label = CreateLabel(background.rectTransform, "Text", fontSize);
        Stretch(label.rectTransform);
        return button;
    }

    // ocupa todo el padre
    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ancla y pivote en el mismo punto relativo del padre (0,0 = abajo izq, 1,1 = arriba der)
    public static void Anchor(RectTransform rt, Vector2 point, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = point;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }
}
