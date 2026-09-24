using UnityEngine;

// Volumen general del juego, guardado en PlayerPrefs.
// Usa AudioListener.volume (global): afecta música y efectos a la vez, sin
// tener que conocer cada AudioSource de la escena.
public static class VolumeSettings
{
    const string MasterVolumeKey = "MasterVolume";
    const float DefaultVolume = 1f;

    // Volumen guardado (0-1). Si nunca se guardó, volumen completo.
    public static float Master => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume));

    // Aplica y guarda al instante: en Android/WebGL el proceso puede morir sin evento de salida.
    public static void SetMaster(float value)
    {
        float clamped = Mathf.Clamp01(value);
        AudioListener.volume = clamped;
        PlayerPrefs.SetFloat(MasterVolumeKey, clamped);
        PlayerPrefs.Save();
    }

    // Se ejecuta solo al arrancar el juego (antes de cargar la primera escena),
    // así el volumen guardado se respeta aunque ninguna escena tenga un slider.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedVolume() => AudioListener.volume = Master;
}
