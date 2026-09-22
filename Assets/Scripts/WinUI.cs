using UnityEngine;
using TMPro;

public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject nextLevelButton;
    // se muestra solo al ganar el último nivel del lanzamiento actual
    [SerializeField] private TMP_Text comingSoonText;
    [SerializeField] private GameObject btnPausar;

    private const string CampaignCompleteMessage = "¡Completaste todos los niveles!\nPronto habrá más.";

    void Awake() => gameObject.SetActive(false);

    public void Show()
    {
        gameObject.SetActive(true);
        if (btnPausar != null) btnPausar.SetActive(false);

        // "siguiente" depende del nivel recién jugado, no del máximo guardado
        bool hasNext = LevelManager.Instance != null && LevelManager.Instance.HasNextLevel;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(hasNext);

        if (comingSoonText != null)
        {
            comingSoonText.text = CampaignCompleteMessage;
            comingSoonText.gameObject.SetActive(!hasNext);

            // si el texto vive dentro del botón, al ocultar el botón también se
            // oculta el texto; se avisa para moverlo en la escena
            if (!hasNext && nextLevelButton != null &&
                comingSoonText.transform.IsChildOf(nextLevelButton.transform))
                Debug.LogWarning("[WinUI] comingSoonText es hijo de nextLevelButton y queda oculto — moverlo fuera del botón.");
        }
    }

    public void OnNextLevelPressed()
    {
        LevelManager.Instance?.PlayNext();
    }

    public void OnMenuPressed()
    {
        LevelManager.Instance?.LoadMenu();
    }
}
