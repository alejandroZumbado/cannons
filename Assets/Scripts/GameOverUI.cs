using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject btnPausar;

    // consejos al perder: uno al azar por derrota (claves de Loc)
    static readonly string[] TipKeys = { "game.tip.merge", "game.tip.move", "game.tip.block" };

    // consejo, creado por código la primera vez (ver EndScreenText); sin título: el panel ya trae su cartel
    private TMP_Text tip;

    void Awake() => gameObject.SetActive(false);

    public void Show()
    {
        gameObject.SetActive(true);
        if (btnPausar != null) btnPausar.SetActive(false);
        ShowTexts();
    }

    void ShowTexts()
    {
        if (tip == null)
            tip = EndScreenText.AddLine(transform, "LoseTip", EndScreenText.LowerArea, 48f, EndScreenText.DetailColor);
        tip.text = Loc.T(TipKeys[Random.Range(0, TipKeys.Length)]);
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
