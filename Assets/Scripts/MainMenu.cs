using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text continueButtonText;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text passwordFeedback;

    // clave de Loc del último aviso de contraseña (null = sin aviso): se guarda la
    // clave y no el texto para re-traducirlo si el jugador cambia de idioma
    private string feedbackKey;

    void Start()
    {
        passwordFeedback = ResolveFeedbackLabel();
        SetFeedback(null);
#if UNITY_WEBGL && !UNITY_EDITOR
        HideQuitButtons(); // en navegador Application.Quit no hace nada: el botón sería un botón muerto
#endif
        RefreshContinueText();
        Loc.Changed += OnLanguageChanged;
        // con portal, el progreso puede venir de la cuenta del jugador: se refresca al terminar el init
        PortalSdk.Init(OnPortalReady);
    }

    void OnDestroy() => Loc.Changed -= OnLanguageChanged;

    void OnLanguageChanged()
    {
        RefreshContinueText();
        SetFeedback(feedbackKey);
    }

    void SetFeedback(string key)
    {
        feedbackKey = key;
        passwordFeedback.text = key == null ? "" : Loc.T(key);
    }

    // en la escena, passwordFeedback apunta al texto interno del InputField
    // (encontrado 2026-10-20): el aviso se escribía dentro de la caja y el
    // InputField lo pisaba con lo tecleado. En ese caso se crea un texto propio
    // debajo de la caja, con la misma fuente.
    TMP_Text ResolveFeedbackLabel()
    {
        if (passwordInput == null || passwordFeedback != passwordInput.textComponent)
            return passwordFeedback;

        var go = new GameObject("PasswordFeedback", typeof(TextMeshProUGUI));
        var rect = go.GetComponent<RectTransform>();
        // hijo del InputField: no entra al layout del menú y se mueve con la caja
        rect.SetParent(passwordInput.transform, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -8f);
        rect.sizeDelta = new Vector2(((RectTransform)passwordInput.transform).rect.width, 50f);

        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = passwordInput.textComponent.font;
        label.fontSize = 34;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.35f, 0.3f); // rojo claro: legible sobre el fondo del menú
        label.raycastTarget = false;              // que no tape clics a la caja ni a botones
        return label;
    }

    void OnPortalReady()
    {
        Loc.Reload(); // con portal, el idioma elegido vive en la cuenta: recién acá se puede leer
        RefreshContinueText();
        AudioListener.volume = VolumeSettings.Master; // el volumen guardado en la cuenta también
    }

    void RefreshContinueText()
    {
        if (this != null && LevelManager.Instance != null) // this: el menú pudo destruirse antes del callback
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
        // sin esto se podría cargar el nivel del guardado local antes de leer el del portal
        if (!PortalSdk.Ready) return;
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
            SetFeedback("menu.no_manager");
            return;
        }

        bool valid = LevelManager.Instance.TryPassword(input);
        SetFeedback(valid ? null : "menu.password_wrong");
    }

    // botones del menú cuyo clic llama OnQuitPressed (así no hace falta otra referencia en la escena)
    void HideQuitButtons()
    {
        // toda la escena: el script no tiene por qué ser padre del botón
        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentMethodName(i) == nameof(OnQuitPressed))
                    button.gameObject.SetActive(false);
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
