using UnityEngine;

// Esc (PC/WebGL) y el botón "atrás" de Android (Unity lo entrega como Escape)
// abren/cierran la pausa (2026-10-20). Antes "atrás" en Android no hacía nada.
// Lo agrega GameManager.Start: no requiere objetos en la escena.
public class PauseHotkey : MonoBehaviour
{
    GameManager manager;
    PauseUI pause;

    public void Init(GameManager gameManager)
    {
        manager = gameManager;
        // el panel de pausa arranca inactivo: hay que buscarlo incluyendo inactivos
        pause = FindFirstObjectByType<PauseUI>(FindObjectsInactive.Include);
        if (pause == null)
        {
            Debug.LogWarning("[PauseHotkey] No hay PauseUI en la escena: Esc/atrás no hará nada.");
            enabled = false;
        }
    }

    void Update()
    {
        // con el nivel terminado ya hay un panel de fin (ganar/perder) con sus botones
        if (manager.GameEnded) return;
        if (Input.GetKeyDown(KeyCode.Escape))
            pause.HandleBack();
    }
}
