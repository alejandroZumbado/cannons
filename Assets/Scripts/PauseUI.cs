using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject optionsPanel;

    // estado de drag al pausar: se restaura tal cual al reanudar. Si se forzara
    // true, pausar durante la fase de disparo (canDrag=false 5s) y reanudar
    // dejaría arrastrar en pleno disparo y el turno se ejecutaría dos veces.
    private bool _dragBeforePause;

    void Awake() => gameObject.SetActive(false);

    public void Toggle()
    {
        if (gameObject.activeSelf) Hide();
        else Show();
    }

    void Show()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        if (gameManager != null)
        {
            _dragBeforePause = gameManager.canDrag;
            gameManager.canDrag = false;
        }
    }

    void Hide()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        if (gameManager != null && !gameManager.GameEnded)
            gameManager.canDrag = _dragBeforePause;
    }

    public void OnResumePressed() => Hide();

    public void OnRetryPressed()
    {
        Hide();
        LevelManager.Instance?.ReloadCurrentLevel();
    }

    public void OnMenuPressed()
    {
        Time.timeScale = 1f;
        LevelManager.Instance?.LoadMenu();
    }

    public void OnOptionsPressed()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }
}
