using UnityEngine;
using TMPro;

public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private TMP_Text comingSoonText;
    [SerializeField] private GameObject btnPausar;

    void Awake() => gameObject.SetActive(false);

    public void Show()
    {
        gameObject.SetActive(true);
        if (btnPausar != null) btnPausar.SetActive(false);

        bool hasNext = LevelManager.Instance != null &&
                       LevelManager.Instance.GetMaxLevelIndex() + 1 < LevelManager.Instance.DatabaseCount;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(hasNext);
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
