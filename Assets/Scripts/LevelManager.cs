using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelDatabase database;

    public Level CurrentLevel { get; private set; }

    private int _currentIndex;
    private const string MaxLevelKey = "MaxLevel";

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
    }

    // indice del nivel mas alto desbloqueado
    public int GetMaxLevelIndex() => PlayerPrefs.GetInt(MaxLevelKey, 0);

    // carga el nivel donde el jugador quedo
    public void PlayCurrent() => LoadLevel(GetMaxLevelIndex());

    // valida password y salta al nivel correspondiente
    // retorna true si el password era valido
    public bool TryPassword(string input)
    {
        for (int i = 0; i < database.Count; i++)
        {
            if (database.Get(i).password == input)
            {
                // si el nivel del password supera el maximo, avanza el maximo
                if (i > GetMaxLevelIndex())
                    PlayerPrefs.SetInt(MaxLevelKey, i);
                PlayerPrefs.Save();
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
        // solo avanza el maximo si el nivel completado es el maximo actual
        if (next > GetMaxLevelIndex() && next < database.Count)
            PlayerPrefs.SetInt(MaxLevelKey, next);
        PlayerPrefs.Save();
    }

    public void LoadLevel(int index)
    {
        Level level = database.Get(index);
        if (level == null) return;
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

    public int DatabaseCount => database != null ? database.Count : 0;

    // texto para el boton continuar: "Continuar - Nivel 5"
    public string ContinueButtonText()
    {
        Level l = database.Get(GetMaxLevelIndex());
        if (l == null) return "Jugar";
        return GetMaxLevelIndex() == 0 ? "Jugar" : $"Continuar - Nivel {l.levelNumber}";
    }
}
