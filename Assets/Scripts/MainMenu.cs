using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text continueButtonText;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text passwordFeedback;

    void Start()
    {
        passwordFeedback.text = "";

        if (LevelManager.Instance != null)
            continueButtonText.text = LevelManager.Instance.ContinueButtonText();
    }

    public void OnContinuePressed()
    {
        // sin LevelManager no hay nivel que cargar; se avisa en vez de tirar NullReference
        if (LevelManager.Instance == null)
        {
            Debug.LogError("[MainMenu] No hay LevelManager en la escena.");
            return;
        }
        LevelManager.Instance.PlayCurrent();
    }

    public void OnPasswordSubmit()
    {
        // LevelManager normaliza (trim + mayúsculas); acá solo se evita el vacío
        string input = passwordInput.text;
        if (string.IsNullOrWhiteSpace(input)) return;

        // sin LevelManager (escena abierta suelta en el Editor) no hay progresión
        if (LevelManager.Instance == null)
        {
            passwordFeedback.text = "Error: abre el juego desde la escena Menu";
            return;
        }

        bool valid = LevelManager.Instance.TryPassword(input);
        passwordFeedback.text = valid ? "" : "Password incorrecto";
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
