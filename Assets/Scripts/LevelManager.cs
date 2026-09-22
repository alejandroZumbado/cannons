using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelDatabase database;

    public Level CurrentLevel { get; private set; }

    private int _currentIndex;

    // Guardado del progreso. Se guardan DOS cosas del nivel máximo desbloqueado:
    // - MaxLevel: su índice (posición) en el LevelDatabase.
    // - MaxLevelId: su levelNumber (ID estable del asset).
    // Al leer se prioriza el ID: si un día el orden del lanzamiento cambia, el
    // jugador sigue en el MISMO nivel aunque haya cambiado de posición. Si el
    // ID ya no está en el lanzamiento, se usa el índice (acotado al rango).
    // Índice == Count significa "campaña completa": cuando se agregan niveles
    // nuevos al final, ese mismo índice pasa a ser el primer nivel nuevo.
    private const string MaxLevelKey = "MaxLevel";
    private const string MaxLevelIdKey = "MaxLevelId";
    private const int NoLevelId = -1;

    void Awake()
    {
        // singleton: si ya existe uno, destruye este duplicado
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // sin database no hay juego; se avisa fuerte en vez de fallar callado
        if (database == null || database.Count == 0)
            Debug.LogError("[LevelManager] LevelDatabase no asignado o vacío — no se puede jugar ningún nivel.");
    }

    public int DatabaseCount => database != null ? database.Count : 0;

    // número que ve el jugador: posición en la campaña (1..N), no levelNumber.
    // levelNumber es solo el ID interno del asset.
    public int CurrentLevelPosition => _currentIndex + 1;

    // hay un nivel después del que se está jugando
    public bool HasNextLevel => _currentIndex + 1 < DatabaseCount;

    // el jugador ya ganó el último nivel del lanzamiento actual
    public bool IsCampaignComplete => DatabaseCount > 0 && GetMaxLevelIndex() >= DatabaseCount;

    // índice del nivel más alto desbloqueado, en [0, Count] (Count = completa)
    public int GetMaxLevelIndex()
    {
        // 1) por ID estable, si sigue existiendo en el lanzamiento
        int savedId = PlayerPrefs.GetInt(MaxLevelIdKey, NoLevelId);
        if (savedId != NoLevelId)
        {
            int byId = FindIndexByLevelNumber(savedId);
            if (byId >= 0) return byId;
        }

        // 2) por índice (partidas viejas sin ID, campaña completa, o nivel
        // sacado del lanzamiento); se acota para no apuntar fuera del array
        return Mathf.Clamp(PlayerPrefs.GetInt(MaxLevelKey, 0), 0, DatabaseCount);
    }

    // índice jugable para "Continuar": si la campaña está completa, el último
    int PlayableMaxIndex() => Mathf.Clamp(GetMaxLevelIndex(), 0, Mathf.Max(DatabaseCount - 1, 0));

    // carga el nivel donde el jugador quedó
    public void PlayCurrent() => LoadLevel(PlayableMaxIndex());

    // carga el nivel siguiente al que se acaba de jugar (no el máximo: si el
    // jugador rejugó un nivel viejo, "Siguiente" debe llevarlo al que sigue)
    public void PlayNext()
    {
        if (HasNextLevel) LoadLevel(_currentIndex + 1);
        else LoadMenu();
    }

    // valida password y salta al nivel correspondiente
    // retorna true si el password era válido
    public bool TryPassword(string input)
    {
        if (database == null || string.IsNullOrWhiteSpace(input)) return false;
        string normalized = input.Trim().ToUpperInvariant();

        for (int i = 0; i < database.Count; i++)
        {
            Level level = database.Get(i);
            // referencias rotas se saltan en vez de romper toda la búsqueda
            if (level == null || string.IsNullOrEmpty(level.password)) continue;

            if (level.password.Trim().ToUpperInvariant() == normalized)
            {
                // si el nivel del password supera el máximo, lo desbloquea
                if (i > GetMaxLevelIndex())
                    SaveMaxLevel(i);
                LoadLevel(i);
                return true;
            }
        }
        return false;
    }

    // llamado por GameManager al ganar un nivel
    public void LevelCompleted()
    {
        int next = _currentIndex + 1;
        // solo avanza el máximo si el nivel ganado era el máximo actual.
        // next puede ser == Count: eso guarda "campaña completa".
        if (next > GetMaxLevelIndex())
            SaveMaxLevel(next);
    }

    public void LoadLevel(int index)
    {
        Level level = database != null ? database.Get(index) : null;
        if (level == null)
        {
            Debug.LogError($"[LevelManager] No existe el nivel en la posición {index + 1} " +
                           $"(el lanzamiento tiene {DatabaseCount}).");
            return;
        }
        _currentIndex = index;
        CurrentLevel = level;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("Game");
        else
            SceneManager.LoadScene("Game");
    }

    public void ReloadCurrentLevel() => LoadLevel(_currentIndex);

    public void LoadMenu()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("Menu");
        else
            SceneManager.LoadScene("Menu");
    }

    // texto para el botón continuar: "Continuar - Nivel 5"
    public string ContinueButtonText()
    {
        if (DatabaseCount == 0) return "Jugar";
        if (IsCampaignComplete) return $"Completado - Nivel {DatabaseCount}";
        int max = GetMaxLevelIndex();
        return max == 0 ? "Jugar" : $"Continuar - Nivel {max + 1}";
    }

    // guarda índice + ID del nivel máximo y escribe a disco de inmediato
    // (en WebGL/Android el proceso puede morir sin OnApplicationQuit)
    void SaveMaxLevel(int index)
    {
        Level level = database != null ? database.Get(index) : null;
        PlayerPrefs.SetInt(MaxLevelKey, index);
        PlayerPrefs.SetInt(MaxLevelIdKey, level != null ? level.levelNumber : NoLevelId);
        PlayerPrefs.Save();
    }

    int FindIndexByLevelNumber(int levelNumber)
    {
        for (int i = 0; i < DatabaseCount; i++)
        {
            Level level = database.Get(i);
            if (level != null && level.levelNumber == levelNumber) return i;
        }
        return -1;
    }
}
