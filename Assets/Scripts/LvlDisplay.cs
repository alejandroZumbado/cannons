using TMPro;
using UnityEngine;

// muestra el número de nivel actual en el texto
public class LvlDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text lvlText;

    void Start()
    {
        Level current = LevelManager.Instance?.CurrentLevel;
        if (lvlText != null && current != null)
            lvlText.text = current.levelNumber.ToString();
    }
}
