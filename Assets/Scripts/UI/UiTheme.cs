using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Estilo pirata de la UI creada por código (2026-10-21): la misma madera
// (BGBtn) y fuente (Pirates Pro) que los botones de las
// escenas. Antes los botones nuevos eran rectángulos negros con la fuente
// por defecto y no combinaban con el resto del juego.
// Vive en Resources/UiTheme.asset porque esta UI no tiene objetos en escena
// a los que asignarle referencias.
[CreateAssetMenu(menuName = "Cannons/UI Theme", fileName = "UiTheme")]
public class UiTheme : ScriptableObject
{
    const string ResourcePath = "UiTheme";

    public Sprite plank;     // tabla de madera enmarcada (9-slice: bordes en su .meta)
    public TMP_FontAsset font;

    static UiTheme current;
    static bool missingLogged;

    // tema cargado una vez; null si falta el asset (la UI cae a rectángulos oscuros, ver ApplyPlank)
    public static UiTheme Current
    {
        get
        {
            if (current != null || missingLogged) return current;
            current = Resources.Load<UiTheme>(ResourcePath);
            if (current == null)
            {
                missingLogged = true; // avisa una sola vez, no en cada botón
                Debug.LogWarning("[UiTheme] Falta Resources/UiTheme.asset: la UI por código queda sin estilo pirata.");
            }
            return current;
        }
    }

    public static TMP_FontAsset Font => Current != null ? Current.font : null;

    // pone la tabla de madera en `image`, teñida con `tint` (blanco = madera natural).
    // Sliced: los bordes se escalan para que el marco no se coma botones chicos.
    public static void ApplyPlank(Image image, Color tint)
    {
        Sprite sprite = Current != null ? Current.plank : null;
        if (sprite == null)
        {
            image.color = new Color(0f, 0f, 0f, 0.6f); // respaldo: el estilo anterior
            return;
        }
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = tint;
        image.pixelsPerUnitMultiplier = BorderScale(sprite, image.rectTransform.sizeDelta);
    }

    // multiplicador que mantiene la proporción del marco según el alto del botón,
    // sin que los bordes izquierdo + derecho pasen de la mitad del ancho
    static float BorderScale(Sprite sprite, Vector2 size)
    {
        if (size.x <= 0f || size.y <= 0f) return 1f; // sin tamaño todavía: escala original
        Vector4 border = sprite.border;              // x izq, y abajo, z der, w arriba (px)
        float byHeight = sprite.rect.height / size.y;
        float byWidth = (border.x + border.z) / (size.x * 0.5f);
        return Mathf.Max(byHeight, byWidth, 0.01f);
    }
}
