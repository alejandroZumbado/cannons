using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Textos creados por código en los paneles de fin de nivel (ganar / perder),
// 2026-10-20. Desde 10-21 sin título: el panel ya trae su propio cartel y el
// número de nivel, y el título se encimaba con ambos. Todo va en la zona vacía
// bajo los botones. Usan la misma fuente que los botones del panel.
public static class EndScreenText
{
    public static readonly Color ValueColor = new Color(1f, 0.85f, 0.2f); // dorado
    public static readonly Color DetailColor = Color.white;
    static readonly Color PlateTint = new Color(0.75f, 0.75f, 0.75f);     // tabla algo más oscura que el panel: se despega del fondo

    // altura (0 = borde de abajo del panel, 1 = arriba) de la zona libre bajo los botones
    public const float LowerArea = 0.3f;

    static readonly Vector2 PlateSize = new Vector2(700f, 210f);

    // texto suelto (consejo al perder), centrado a la altura relativa `heightRatio` del panel
    public static TMP_Text AddLine(Transform panel, string name, float heightRatio, float fontSize, Color color)
    {
        var label = CreateText((RectTransform)panel, name, fontSize, color, PanelFont(panel));
        UiFactory.Anchor(label.rectTransform, new Vector2(0.5f, heightRatio), Vector2.zero,
                         new Vector2(1150f, fontSize * 3f)); // hasta 2 líneas, dentro del marco
        IgnoreLayout(label.gameObject);
        return label;
    }

    // tabla de madera con rótulo chico arriba y valor grande abajo (contraseña del siguiente nivel)
    public static void AddPlate(Transform panel, string name, float heightRatio,
                                out GameObject plate, out TMP_Text caption, out TMP_Text value)
    {
        TMP_FontAsset font = PanelFont(panel); // antes de crear hijos: si no, devolvería la fuente por defecto
        Image background = UiFactory.CreatePanel((RectTransform)panel, name, PlateTint, blocksClicks: false);
        UiFactory.Anchor(background.rectTransform, new Vector2(0.5f, heightRatio), Vector2.zero, PlateSize);
        UiTheme.ApplyPlank(background, PlateTint);
        IgnoreLayout(background.gameObject);
        plate = background.gameObject;

        // ancho sin los costados de la tabla (cortinas y marco)
        float inner = PlateSize.x - 160f;
        caption = CreateText(background.rectTransform, "Caption", 36f, DetailColor, font);
        UiFactory.Anchor(caption.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -28f),
                         new Vector2(inner, 46f));

        value = CreateText(background.rectTransform, "Value", 90f, ValueColor, font);
        UiFactory.Anchor(value.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 24f),
                         new Vector2(inner, 105f));
        value.characterSpacing = 15f; // letras separadas: el código se lee (y se copia a mano) sin errores
    }

    // texto en negrita con borde negro; los largos (español) se achican en vez de cortarse
    static TMP_Text CreateText(RectTransform parent, string name, float fontSize, Color color, TMP_FontAsset font)
    {
        var label = UiFactory.CreateLabel(parent, name, fontSize, font);
        label.color = color;
        label.fontStyle = FontStyles.Bold;
        UiFactory.Outline(label);           // legible sobre cualquier fondo
        label.enableAutoSizing = true;
        label.fontSizeMin = fontSize * 0.5f;
        label.fontSizeMax = fontSize;
        return label;
    }

    // el panel puede ordenar sus hijos con un layout: estos textos no deben correr los botones
    static void IgnoreLayout(GameObject go) => go.AddComponent<LayoutElement>().ignoreLayout = true;

    // fuente del primer texto del panel (la de sus botones); null = la de TextMeshPro por defecto
    static TMP_FontAsset PanelFont(Transform panel)
    {
        var existing = panel.GetComponentInChildren<TMP_Text>(true);
        return existing != null ? existing.font : null;
    }
}
