using TMPro;
using UnityEngine;

// Texto que sube y se desvanece sobre un punto del mundo (2026-10-20): números
// de daño ("-2") y aviso de fusión ("x3"). Va en un canvas overlay (no en 3D)
// para que el tamaño sea el mismo sin importar la escala de los modelos.
public class FloatingText : MonoBehaviour
{
    const float Duration = 0.9f;   // segundos visible
    const float Rise = 90f;        // píxeles (referencia 1080p) que sube
    const float FontSize = 64f;
    const int SortingOrder = 40;   // sobre el juego, debajo del tutorial (50)

    static RectTransform canvas;   // uno por escena; se recrea si la escena cambió

    Vector3 worldAnchor;
    TMP_Text label;
    Color baseColor;
    float age;

    public static void Spawn(Vector3 worldPosition, string text, Color color)
    {
        if (Camera.main == null) return; // sin cámara no hay a dónde proyectar
        if (canvas == null) canvas = UiFactory.CreateOverlayCanvas("FloatingTextCanvas", SortingOrder, clickable: false);

        TMP_Text label = UiFactory.CreateLabel(canvas, "FloatingText", FontSize);
        label.text = text;
        label.color = color;
        label.fontStyle = FontStyles.Bold;
        label.outlineWidth = 0.25f;        // borde negro: se lee sobre el agua y la arena
        label.outlineColor = Color.black;
        label.rectTransform.sizeDelta = new Vector2(300, 100);

        var floating = label.gameObject.AddComponent<FloatingText>();
        floating.worldAnchor = worldPosition;
        floating.label = label;
        floating.baseColor = color;
        floating.Place(0f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= Duration) { Destroy(gameObject); return; }
        Place(age / Duration);
    }

    // posición y transparencia según el avance de la animación (0..1)
    void Place(float progress)
    {
        Vector3 screen = Camera.main.WorldToScreenPoint(worldAnchor);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out Vector2 local);
        label.rectTransform.anchoredPosition = local + Vector2.up * (Rise * progress);

        Color c = baseColor;
        c.a = 1f - progress * progress; // se borra más rápido al final
        label.color = c;
    }
}
