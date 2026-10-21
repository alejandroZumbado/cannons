using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Líneas de texto creadas por código en los paneles de fin de nivel (ganar /
// perder), 2026-10-20: antes solo tenían botones, sin decir qué pasó.
// Usan la misma fuente que los botones del panel, para que combinen.
public static class EndScreenText
{
    public static readonly Color TitleColor = new Color(1f, 0.85f, 0.2f); // dorado
    public static readonly Color DetailColor = Color.white;

    // texto anclado arriba al centro del panel; `top` = distancia desde el borde superior
    public static TMP_Text AddLine(Transform panel, string name, float top, float fontSize, Color color)
    {
        var label = UiFactory.CreateLabel((RectTransform)panel, name, fontSize, PanelFont(panel));
        UiFactory.Anchor(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -top),
                         new Vector2(1400f, fontSize * 1.6f));
        label.color = color;
        label.fontStyle = FontStyles.Bold;
        label.outlineWidth = 0.2f;          // borde: legible sobre cualquier fondo
        label.outlineColor = Color.black;
        label.enableAutoSizing = true;      // textos largos (español) se achican en vez de cortarse
        label.fontSizeMin = fontSize * 0.5f;
        label.fontSizeMax = fontSize;
        // el panel puede ordenar sus hijos con un layout: estas líneas no deben correr los botones
        label.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        return label;
    }

    // fuente del primer texto del panel (la de sus botones); null = la de TextMeshPro por defecto
    static TMP_FontAsset PanelFont(Transform panel)
    {
        var existing = panel.GetComponentInChildren<TMP_Text>(true);
        return existing != null ? existing.font : null;
    }
}
