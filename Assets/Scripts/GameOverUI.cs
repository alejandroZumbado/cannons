using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject btnPausar;

    // consejos al perder: uno al azar por derrota (claves de Loc)
    static readonly string[] TipKeys = { "game.tip.merge", "game.tip.move", "game.tip.block" };

    // título y consejo, creados por código la primera vez (ver EndScreenText)
    private TMP_Text title;
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
        if (title == null)
        {
            title = EndScreenText.AddLine(transform, "LoseTitle", 80f, 90f, EndScreenText.TitleColor);
            tip = EndScreenText.AddLine(transform, "LoseTip", 200f, 44f, EndScreenText.DetailColor);
        }
        title.text = Loc.T("game.lose_title");
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
