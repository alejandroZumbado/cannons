using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject nextLevelButton;
    // se muestra solo al ganar el último nivel del lanzamiento actual
    [SerializeField] private TMP_Text comingSoonText;
    [SerializeField] private GameObject btnPausar;

    // copia de comingSoonText fuera del botón (ver CampaignDoneLabel); se crea una sola vez
    private TMP_Text campaignDoneLabel;

    // placa con la contraseña del siguiente nivel, creada por código la primera vez (ver EndScreenText)
    private GameObject passwordPlate;
    private TMP_Text passwordCaption;
    private TMP_Text passwordValue;

    void Awake() => gameObject.SetActive(false);

    public void Show()
    {
        gameObject.SetActive(true);
        if (btnPausar != null) btnPausar.SetActive(false);

        // "siguiente" depende del nivel recién jugado, no del máximo guardado
        bool hasNext = LevelManager.Instance != null && LevelManager.Instance.HasNextLevel;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(hasNext);

        ShowTexts();

        if (comingSoonText == null) return;
        if (TextIsButtonLabel())
        {
            // en la escena, comingSoonText es la etiqueta del propio botón "Siguiente"
            // (encontrado 2026-10-20: antes se apagaba y el botón quedaba sin texto).
            // La etiqueta se deja como está (la traduce SceneTextLocalizer) y el
            // mensaje final va en una copia fuera del botón, que sí queda visible.
            if (!hasNext) ShowMessage(CampaignDoneLabel());
        }
        else
        {
            comingSoonText.gameObject.SetActive(!hasNext);
            if (!hasNext) ShowMessage(comingSoonText);
        }
    }

    // contraseña del siguiente nivel (para retomar en otro dispositivo); oculta si no hay siguiente
    void ShowTexts()
    {
        if (passwordPlate == null)
            EndScreenText.AddPlate(transform, "NextPassword", EndScreenText.LowerArea,
                                   out passwordPlate, out passwordCaption, out passwordValue);

        LevelManager levels = LevelManager.Instance;
        string password = levels != null ? levels.NextLevelPassword : null;
        bool hasPassword = !string.IsNullOrEmpty(password);
        passwordPlate.SetActive(hasPassword);
        if (!hasPassword) return;

        // número que ve el jugador = posición del nivel recién ganado + 1
        passwordCaption.text = Loc.Format("game.next_password", levels.CurrentLevelPosition + 1);
        passwordValue.text = password;
    }

    bool TextIsButtonLabel() =>
        nextLevelButton != null && comingSoonText.transform.IsChildOf(nextLevelButton.transform);

    static void ShowMessage(TMP_Text label)
    {
        label.text = Loc.T("game.campaign_done");
        label.gameObject.SetActive(true);
    }

    // copia la etiqueta (misma fuente y estilo) como hijo del panel, en el lugar del botón oculto
    TMP_Text CampaignDoneLabel()
    {
        if (campaignDoneLabel != null) return campaignDoneLabel;

        campaignDoneLabel = Instantiate(comingSoonText, transform);
        campaignDoneLabel.name = "CampaignDoneText";
        // la copia trae el LocalizedText del botón: lo quitaría y pondría "Siguiente nivel"
        var localized = campaignDoneLabel.GetComponent<LocalizedText>();
        if (localized != null) DestroyImmediate(localized);
        // el panel puede ordenar sus hijos con un layout: el mensaje no debe correr los botones
        campaignDoneLabel.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

        RectTransform rect = campaignDoneLabel.rectTransform;
        // la etiqueta venía estirada al tamaño del botón; con anclas al centro el tamaño es fijo
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.position = nextLevelButton.transform.position;
        rect.sizeDelta = new Vector2(900, 160); // dos líneas a lo ancho del panel
        campaignDoneLabel.alignment = TextAlignmentOptions.Center;
        campaignDoneLabel.enableAutoSizing = true;
        return campaignDoneLabel;
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
