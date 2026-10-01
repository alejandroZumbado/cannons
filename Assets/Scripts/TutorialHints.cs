using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tutorial visual mínimo (2026-09-30). Los portales (Poki/CrazyGames) piden
// tutoriales visuales; antes el jugador tenía que adivinar el arrastre y la
// fusión (que recién hace falta en la posición ~5-11).
// - Arrastre: en la posición 1, durante la fase de agarre, mientras haya
//   menos de MinCannonsTaught cañones colocados.
// - Fusión: la primera vez que un nivel trae un pirata de HP >= MergeHp
//   (no se gana sin fusionar). Se marca vista (GameStorage) al fusionar.
// Se crea solo desde GameManager (AddComponent): no requiere objetos en la
// escena. Todo es UI overlay sin raycast, así que no interfiere con los
// OnMouse* del arrastre.
public class TutorialHints : MonoBehaviour
{
    const int MinCannonsTaught = 2;     // tras 2 colocaciones el arrastre ya se entendió
    const int MergeHp = 4;              // 3 disparos de daño 1 no alcanzan: hay que fusionar
    const string MergeSeenKey = GameStorage.TutorialMergeSeenKey;
    const float PointerCycle = 1.4f;    // segundos de un recorrido de la flecha

    const string DragText = "Arrastra el cañón del barril a una casilla del frente";
    const string MergeText = "¡Pirata fuerte! Suelta un cañón sobre otro para fusionarlos: su daño se suma";

    GameManager manager;
    bool teachDrag;
    bool teachMerge;

    RectTransform pointer;
    GameObject banner;
    TMP_Text bannerText;
    RectTransform canvasRect;

    // llamado por GameManager.Start; position = posición del nivel en la campaña (1 = primero)
    public void Init(GameManager gameManager, Level level, int position)
    {
        manager = gameManager;
        teachDrag = position == 1;
        teachMerge = GameStorage.GetInt(MergeSeenKey, 0) == 0 && HasStrongPirate(level);
        if (teachDrag || teachMerge)
            BuildUI();
        else
            enabled = false; // nada que enseñar en este nivel: no gasta Update
    }

    static bool HasStrongPirate(Level level)
    {
        if (level == null) return false;
        foreach (var fila in level.filas)
            foreach (var c in fila.cuadros)
                if (c.tipo >= 1 && c.hp >= MergeHp) return true;
        return false;
    }

    void Update()
    {
        if (manager.GameEnded || !manager.canDrag) { Hide(); return; }

        if (teachMerge && AnyMerged())
        {
            // ya fusionó: lección aprendida para siempre (guardado inmediato, WebGL puede cerrarse sin aviso)
            teachMerge = false;
            GameStorage.SetInt(MergeSeenKey, 1);
            GameStorage.Save();
        }

        if (teachMerge && manager.cannons.Count >= 1 && StrongPirateOnBoard())
            Show(MergeText, NearestSlot(occupied: true));
        else if (teachDrag && manager.cannons.Count < MinCannonsTaught)
            Show(DragText, NearestSlot(occupied: false));
        else
            Hide();
    }

    // pirata vivo que todavía necesita fusión (con 1 disparo ya recibido, HP 3 sigue pidiéndola)
    bool StrongPirateOnBoard()
    {
        foreach (var p in manager.pirates)
            if (p != null && p.Hp >= MergeHp - 1) return true;
        return false;
    }

    bool AnyMerged()
    {
        foreach (var c in manager.cannons)
            if (c != null && c.GetDamage() > 1) return true;
        return false;
    }

    // slot vacío u ocupado más cercano al barril (destino natural del cañón nuevo); null si no hay
    ReceiptCannon NearestSlot(bool occupied)
    {
        ReceiptCannon best = null;
        float bestDist = float.MaxValue;
        foreach (var slot in manager.Slots)
        {
            if ((slot.cannon != null) != occupied) continue;
            float d = Vector3.Distance(slot.parent.position, manager.centerCannon.position);
            if (d < bestDist) { bestDist = d; best = slot; }
        }
        return best;
    }

    void Show(string text, ReceiptCannon target)
    {
        if (target == null) { Hide(); return; }
        banner.SetActive(true);
        bannerText.text = text;
        pointer.gameObject.SetActive(true);

        // la flecha viaja del barril al slot destino, en bucle
        float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time / PointerCycle, 1f));
        Vector3 world = Vector3.Lerp(manager.centerCannon.position, target.parent.position, t);
        pointer.anchoredPosition = WorldToCanvas(world);
    }

    void Hide()
    {
        if (banner != null) banner.SetActive(false);
        if (pointer != null) pointer.gameObject.SetActive(false);
    }

    Vector2 WorldToCanvas(Vector3 world)
    {
        Vector3 screen = Camera.main.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
        return local;
    }

    // ─── UI construida por código (overlay propio, mismo escalado que el resto: 1920x1080, match 1) ───

    void BuildUI()
    {
        var canvasGo = new GameObject("TutorialCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // encima del HUD, debajo de nada crítico (paneles se muestran con el juego terminado)
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;
        canvasRect = canvasGo.GetComponent<RectTransform>();

        banner = CreateBanner(canvasRect);
        pointer = CreatePointer(canvasRect);
        Hide();
    }

    GameObject CreateBanner(RectTransform parent)
    {
        var go = new GameObject("TutorialBanner", typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -40);
        rt.sizeDelta = new Vector2(1300, 110);
        var bg = go.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);
        bg.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(TextMeshProUGUI));
        var trt = textGo.GetComponent<RectTransform>();
        trt.SetParent(rt, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(30, 10);
        trt.offsetMax = new Vector2(-30, -10);
        bannerText = textGo.GetComponent<TextMeshProUGUI>();
        bannerText.alignment = TextAlignmentOptions.Center;
        bannerText.fontSize = 44;
        bannerText.enableAutoSizing = true;
        bannerText.fontSizeMin = 28;
        bannerText.fontSizeMax = 44;
        bannerText.color = Color.white;
        bannerText.raycastTarget = false;
        return go;
    }

    RectTransform CreatePointer(RectTransform parent)
    {
        // rombo amarillo con borde: sin sprites extra para no sumar peso al build
        var go = new GameObject("TutorialPointer", typeof(Image), typeof(Outline));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(56, 56);
        rt.localRotation = Quaternion.Euler(0, 0, 45);
        var img = go.GetComponent<Image>();
        img.color = new Color(1f, 0.85f, 0.2f, 0.95f);
        img.raycastTarget = false;
        var outline = go.GetComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);
        return rt;
    }
}
