using TMPro;
using UnityEngine;

// Texto fijo traducido: pone Loc.T(key) en el TMP_Text del mismo objeto y se
// re-pinta cuando el jugador cambia de idioma. Se agrega solo (SceneTextLocalizer)
// o a mano en el Inspector.
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    TMP_Text label;

    public void SetKey(string newKey)
    {
        key = newKey;
        Apply();
    }

    void Awake() => label = GetComponent<TMP_Text>();

    void OnEnable()
    {
        Loc.Changed += Apply;
        Apply(); // pudo cambiar el idioma mientras estaba oculto
    }

    void OnDisable() => Loc.Changed -= Apply;

    void Apply()
    {
        if (label != null && !string.IsNullOrEmpty(key))
            label.text = Loc.T(key);
    }
}
