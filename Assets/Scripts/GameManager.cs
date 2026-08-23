using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private CannonCreator spawnCannon;
    private CannonManagement newCannon;
    public List<CannonManagement> cannons;
    public List<PirateManager> pirates;
    public AudioManager AudioManager;
    public Transform centerCannon;
    public PirateSpawnManager spawnPirate;
    public Matriz matriz;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private WinUI winUI;
    private Level _level;
    private bool _gameEnded = false;
    public bool GameEnded => _gameEnded;
    public int round = 0;
    public bool canDrag = false;
    private ReceiptCannon[] slots;
    void Start()
    {
        _level = LevelManager.Instance != null
            ? LevelManager.Instance.CurrentLevel
            : null;

        matriz = GetComponent<Matriz>();
        matriz.SetDimensions(4, 5);
        spawnCannon = GetComponent<CannonCreator>();
        spawnPirate = GetComponent<PirateSpawnManager>();
        spawnPirate.SetMatriz(matriz);
        slots = FindObjectsByType<ReceiptCannon>(FindObjectsSortMode.None);
        nextRound();
        GameFlow1();
        if (_level != null && _level.isHard)
            AudioManager.PlayHardLvl();
        else
            AudioManager.PlayNormalLvl();
    }

    public void nextRound()
    {
        if (_gameEnded) return;
        PirateAdvance();
        if (_level != null && _level.filas.Count > round)
            SpawnPiratesForRound(_level.filas[round]);
        round++;

        // musica final: solo la primera vez que se agotan las oleadas
        if (_level != null && round == _level.filas.Count)
            AudioManager.PlayFinalLvl(_level.isHard);

        // win: todas las oleadas terminaron y no quedan piratas vivos
        // cubre el caso donde el jugador mato todo antes de que round llegara a filas.Count
        if (_level != null && round >= _level.filas.Count && pirates.Count == 0)
            Win();
    }

    public void SpawnPiratesForRound(Level.Fila fila)
    {
        foreach (Level.Cuadro cuadro in fila.cuadros)
        {
            if (cuadro.tipo >= 1)
                NextPirate(cuadro);
        }
    }

    public void NextPirate(Level.Cuadro cuadro)
    {
        PirateManager newPirate = spawnPirate.SpawnPirate(cuadro);
        newPirate.SetManager(this);
        pirates.Add(newPirate);
    }
    public void RemovePirate(PirateManager pirate)
    {
        pirates.Remove(pirate);
        if (pirates.Count == 0 && _level != null && round >= _level.filas.Count)
            Win();
    }

    public void Win()
    {
        if (_gameEnded) return;
        _gameEnded = true;
        canDrag = false;
        LevelManager.Instance?.LevelCompleted();
        AudioManager.PlayWinLvl();
        Invoke(nameof(ShowWinDelayed), 1.5f);
    }

    void ShowWinDelayed() => winUI?.Show();

    public void GameOver()
    {
        if (_gameEnded) return;
        _gameEnded = true;
        canDrag = false;
        AudioManager.PlayLoseLvl();
        Invoke(nameof(ShowGameOverDelayed), 1.5f);
    }

    void ShowGameOverDelayed() => gameOverUI?.Show();
    public void PirateAdvance()
    {
        foreach (PirateManager pirate in pirates)
        {
            pirate.Advance();
        }
    }
    public CannonCreator SpawnCannon() => spawnCannon;

    // busca el slot cuya area se superpone con el area del cañon soltado, o null si no hay ninguno
    public ReceiptCannon FindSlotAt(Bounds cannonBounds)
    {
        foreach (ReceiptCannon slot in slots)
            if (slot.GetBounds().Intersects(cannonBounds))
                return slot;
        return null;
    }

    public void GameFlow2()
    {
        canDrag = false; // bloquea drag al entrar en fase de acción
        foreach (CannonManagement cannon in cannons)
        {
            cannon.Shoot();
        }
        Invoke(nameof(GameFlow1), 5f);
        Invoke(nameof(nextRound), 5f);
    }

    void GameFlow1()
    {
        if (_gameEnded) return;
        canDrag = true; // habilita drag en fase de agarre
        if (newCannon == null || cannons.Contains(newCannon))
        {
            newCannon = spawnCannon.SpawnCannon();
            newCannon.SetManager(this);
            newCannon.SetCenter(centerCannon);
            newCannon.CenterCannon();
            newCannon.ShowIcon();
        }
    }

    public void AddCannonInTable(CannonManagement cannon)
    {
        if (!cannons.Contains(cannon))
            cannons.Add(cannon);
    }

    public void RemoveCannonInTable(CannonManagement cannon)
    {
        cannons.Remove(cannon);
    }
}
