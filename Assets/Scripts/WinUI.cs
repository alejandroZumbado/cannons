using UnityEngine;
using TMPro;

public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject nextLevelButton; // el boton "Siguiente Nivel" completo
    [SerializeField] private TMP_Text comingSoonText;   // texto "proximamente mas niveles"

    void Awake() => gameObject.SetActive(false);

    public void Show()
    {
        gameObject.SetActive(true);

        bool hasNext = LevelManager.Instance != null &&
                       LevelManager.Instance.GetMaxLevelIndex() + 1 < LevelManager.Instance.DatabaseCount;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(hasNext);

        if (comingSoonText != null)
            comingSoonText.gameObject.SetActive(!hasNext);
    }

    public void OnNextLevelPressed()
    {
        LevelManager.Instance?.PlayCurrent();
    }

    public void OnMenuPressed()
    {
        LevelManager.Instance?.LoadMenu();
    }
}
