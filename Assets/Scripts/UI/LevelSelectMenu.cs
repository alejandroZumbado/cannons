using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Selector de niveles del menú (2026-10-20). Antes la única forma de rejugar
// un nivel viejo era saber su contraseña. Botón "Niveles" arriba a la
// izquierda; abre una grilla de 50 niveles por página: los desbloqueados
// (hasta el máximo guardado) se pueden jugar, el resto aparece con candado.
// Se crea solo al cargar Menu (mismo patrón que LanguageToggle): no hay
// objetos en la escena que mantener. Esc / atrás de Android lo cierra.
public class LevelSelectMenu : MonoBehaviour
{
    const string MenuScene = "Menu";
    const int SortingOrder = 11;            // sobre el menú y el botón de idioma (10), debajo de la intro
    const int Columns = 10;
    const int Rows = 5;
    const int PerPage = Columns * Rows;
    static readonly Vector2 CellSize = new Vector2(120, 100);
    const float CellGap = 18f;

    // tintes sobre la madera del tema (blanco = madera natural), desde 10-21
    static readonly Color UnlockedColor = Color.white;
    static readonly Color CurrentColor = new Color(1f, 0.8f, 0.3f);           // el nivel del "Continuar": madera dorada
    static readonly Color LockedColor = new Color(0.35f, 0.35f, 0.35f, 0.9f); // madera apagada
    static readonly Color ButtonColor = Color.white;
    static readonly Vector2 TitleSize = new Vector2(900, 130);

    RectTransform canvas;
    TMP_Text openLabel;
    GameObject panel;
    TMP_Text titleLabel;
    TMP_Text pageLabel;
    Button prevButton;
    Button nextButton;
    readonly List<Button> cells = new List<Button>();
    readonly List<TMP_Text> cellLabels = new List<TMP_Text>();
    int page;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == MenuScene)
                new GameObject("LevelSelectMenu").AddComponent<LevelSelectMenu>();
        };
    }

    void Start()
    {
        canvas = UiFactory.CreateOverlayCanvas("LevelSelectCanvas", SortingOrder, clickable: true, transform);
        BuildOpenButton();
        BuildPanel();
        panel.SetActive(false);
        Loc.Changed += RefreshTexts;
        RefreshTexts();
    }

    void OnDestroy() => Loc.Changed -= RefreshTexts;

    void Update()
    {
        // Esc / atrás de Android: cierra el selector si está abierto
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void RefreshTexts()
    {
        openLabel.text = Loc.T("levels.button");
        titleLabel.text = Loc.T("levels.title");
    }

    // ─── abrir / cerrar / páginas ───

    void Open()
    {
        LevelManager levels = LevelManager.Instance;
        if (levels == null || levels.DatabaseCount == 0)
        {
            Debug.LogError("[LevelSelectMenu] No hay LevelManager o el LevelDatabase está vacío.");
            return;
        }
        // con portal, el progreso puede venir de la cuenta: hasta que cargue, el máximo local podría estar viejo
        if (!PortalSdk.Ready) return;

        page = levels.PlayableMaxIndex() / PerPage; // abre en la página del nivel actual
        panel.SetActive(true);
        ShowPage();
    }

    void Close() => panel.SetActive(false);

    void ChangePage(int delta)
    {
        page = Mathf.Clamp(page + delta, 0, PageCount() - 1);
        ShowPage();
    }

    int PageCount() => Mathf.Max(1, Mathf.CeilToInt(LevelManager.Instance.DatabaseCount / (float)PerPage));

    void ShowPage()
    {
        LevelManager levels = LevelManager.Instance;
        int maxIndex = levels.PlayableMaxIndex();

        for (int i = 0; i < PerPage; i++)
        {
            int index = page * PerPage + i;
            bool exists = index < levels.DatabaseCount;
            cells[i].gameObject.SetActive(exists);
            if (!exists) continue;

            bool unlocked = index <= maxIndex;
            cells[i].interactable = unlocked;
            cells[i].image.color = index == maxIndex ? CurrentColor : unlocked ? UnlockedColor : LockedColor;
            cellLabels[i].text = (index + 1).ToString(); // el jugador ve la posición, no el levelNumber
            cellLabels[i].alpha = unlocked ? 1f : 0.4f;
        }

        pageLabel.text = $"{page + 1} / {PageCount()}";
        prevButton.interactable = page > 0;
        nextButton.interactable = page < PageCount() - 1;
    }

    void PlayCell(int cell)
    {
        int index = page * PerPage + cell;
        // doble chequeo: el botón ya está desactivado si el nivel no está desbloqueado
        if (index > LevelManager.Instance.PlayableMaxIndex()) return;
        LevelManager.Instance.LoadLevel(index);
    }

    // ─── UI por código ───

    void BuildOpenButton()
    {
        Button open = UiFactory.CreateButton(canvas, "LevelsButton", new Vector2(240, 80), ButtonColor, 40f, Open, out openLabel);
        UiFactory.Anchor((RectTransform)open.transform, new Vector2(0f, 1f), new Vector2(30, -30), new Vector2(240, 80));
    }

    void BuildPanel()
    {
        // fondo oscuro a pantalla completa: tapa (y bloquea) el menú de atrás
        Image dim = UiFactory.CreatePanel(canvas, "LevelSelectPanel", new Color(0f, 0f, 0f, 0.75f), blocksClicks: true);
        UiFactory.Stretch(dim.rectTransform);
        panel = dim.gameObject;
        RectTransform root = dim.rectTransform;

        // título sobre una tabla de madera, como cartel
        Image banner = UiFactory.CreatePanel(root, "TitleBanner", Color.white, blocksClicks: false);
        UiFactory.Anchor(banner.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -40), TitleSize);
        UiTheme.ApplyPlank(banner, Color.white);
        titleLabel = UiFactory.CreateLabel(banner.rectTransform, "Title", 64f);
        UiFactory.Stretch(titleLabel.rectTransform);
        UiFactory.Outline(titleLabel);

        BuildGrid(root);
        BuildFooter(root);
    }

    void BuildGrid(RectTransform root)
    {
        var grid = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup)).GetComponent<RectTransform>();
        grid.SetParent(root, false);
        float width = Columns * CellSize.x + (Columns - 1) * CellGap;
        float height = Rows * CellSize.y + (Rows - 1) * CellGap;
        UiFactory.Anchor(grid, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(width, height));

        var layout = grid.GetComponent<GridLayoutGroup>();
        layout.cellSize = CellSize;
        layout.spacing = new Vector2(CellGap, CellGap);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Columns;

        for (int i = 0; i < PerPage; i++)
        {
            int cell = i; // copia para el closure del clic
            Button button = UiFactory.CreateButton(grid, "Level" + i, CellSize, UnlockedColor, 44f, () => PlayCell(cell), out TMP_Text label);
            cells.Add(button);
            cellLabels.Add(label);
        }
    }

    // flechas de página + número de página + cerrar
    void BuildFooter(RectTransform root)
    {
        Vector2 bottom = new Vector2(0.5f, 0f);
        prevButton = UiFactory.CreateButton(root, "PrevPage", new Vector2(120, 90), ButtonColor, 60f, () => ChangePage(-1), out TMP_Text prev);
        prev.text = "<";
        UiFactory.Anchor((RectTransform)prevButton.transform, bottom, new Vector2(-200, 60), new Vector2(120, 90));

        pageLabel = UiFactory.CreateLabel(root, "Page", 48f);
        UiFactory.Anchor(pageLabel.rectTransform, bottom, new Vector2(0, 60), new Vector2(240, 90));

        nextButton = UiFactory.CreateButton(root, "NextPage", new Vector2(120, 90), ButtonColor, 60f, () => ChangePage(1), out TMP_Text next);
        next.text = ">";
        UiFactory.Anchor((RectTransform)nextButton.transform, bottom, new Vector2(200, 60), new Vector2(120, 90));

        Button close = UiFactory.CreateButton(root, "Close", new Vector2(120, 90), ButtonColor, 60f, Close, out TMP_Text x);
        x.text = "X";
        UiFactory.Anchor((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(-40, -40), new Vector2(120, 90));
    }
}
