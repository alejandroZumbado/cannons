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
        LevelManager.Instance.PlayCurrent();
    }

    public void OnPasswordSubmit()
    {
        string input = passwordInput.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(input)) return;

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
