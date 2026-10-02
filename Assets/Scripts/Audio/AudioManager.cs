using UnityEngine;

// Música del juego. Las pistas no están en el build: se cargan bajo demanda
// desde StreamingAssets/Music (ver MusicLibrary). La API pública no cambió.
public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    // nombres de archivo en StreamingAssets/Music (sin .mp3)
    const string MenuTrack = "menu";
    const string NormalTrack = "normal";
    const string NormalFinalTrack = "normal_final";
    const string HardTrack = "hard";
    const string HardFinalTrack = "hard_final";
    const string WinTrack = "win";
    const string LoseTrack = "lose";

    // última pista pedida: si una descarga vieja termina tarde, no pisa a la nueva
    string wantedTrack;

    public void PlayMenu()           => PlayTrack(MenuTrack);
    public void PlayNormalLvl()      => PlayTrack(NormalTrack);
    public void PlayNormalFinalLvl() => PlayTrack(NormalFinalTrack);
    public void PlayHardLvl()        => PlayTrack(HardTrack);
    public void PlayHardFinalLvl()   => PlayTrack(HardFinalTrack);
    public void PlayWinLvl()         => PlayTrack(WinTrack);
    public void PlayLoseLvl()        => PlayTrack(LoseTrack);

    // cambia a la musica "final" segun si el nivel es hard o no
    public void PlayFinalLvl(bool isHard)
    {
        if (isHard) PlayHardFinalLvl();
        else        PlayNormalFinalLvl();
    }

    // Llamado por GameManager al empezar el nivel: deja listas las pistas que
    // sonarán después (final, victoria, derrota) para que el cambio sea inmediato.
    public void PreloadLevelTracks(bool isHard)
    {
        MusicLibrary.Preload(isHard ? HardFinalTrack : NormalFinalTrack, WinTrack, LoseTrack);
    }

    void PlayTrack(string track)
    {
        wantedTrack = track;
        MusicLibrary.Load(track, clip => OnTrackReady(track, clip));
    }

    void OnTrackReady(string track, AudioClip clip)
    {
        // this == null: la escena se descargó mientras bajaba; clip null: falló (ya avisado)
        if (this == null || clip == null || track != wantedTrack) return;
        audioSource.Pause();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
