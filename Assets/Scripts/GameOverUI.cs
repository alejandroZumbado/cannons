using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    void Awake() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);

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
