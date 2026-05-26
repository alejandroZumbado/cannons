using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject btnPausar;

    void Awake() => gameObject.SetActive(false);

    public void Show()
    {
        gameObject.SetActive(true);
        if (btnPausar != null) btnPausar.SetActive(false);
    }

    public void OnRestartPressed()
    {
        gameObject.SetActive(false);
        LevelManager.Instance?.ReloadCurrentLevel();
    }

    public void OnMenuPressed()
    {
        LevelManager.Instance?.LoadMenu();
    }
}
