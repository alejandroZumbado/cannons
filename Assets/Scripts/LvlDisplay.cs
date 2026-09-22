using TMPro;
using UnityEngine;

// muestra el número de nivel actual en el texto
public class LvlDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text lvlText;

    void Start()
    {
        // muestra la posicion en la campaña, no el levelNumber interno
        LevelManager manager = LevelManager.Instance;
        if (lvlText != null && manager != null && manager.CurrentLevel != null)
            lvlText.text = manager.CurrentLevelPosition.ToString();
    }
}
