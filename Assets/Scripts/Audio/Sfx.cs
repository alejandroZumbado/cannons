using System.Collections.Generic;
using UnityEngine;

// efectos de sonido del juego (la música va aparte, en AudioManager)
public enum SfxId { Shoot, Hit, Death, Place, Merge, Click }

// Punto único para tocar efectos (2026-10-20): Sfx.Play(SfxId.Shoot).
// - Clip: Assets/Resources/Sfx/<id en minúsculas> (ej. "shoot.wav") si existe;
//   si no, el placeholder sintetizado de SfxSynth.
// - Respeta el volumen general (AudioListener.volume, ver VolumeSettings).
// - Se crea solo la primera vez que se usa y sobrevive a los cambios de escena.
public static class Sfx
{
    const int Voices = 8;               // sonidos simultáneos máximos
    const float MinRepeatGap = 0.05f;   // 5 cañones disparan en el mismo frame: suena un solo cañonazo, no 5 apilados
    const float PitchJitter = 0.06f;    // variación de tono para que repetir no suene robótico
    const string ResourcesFolder = "Sfx/";

    static readonly Dictionary<SfxId, AudioClip> Clips = new Dictionary<SfxId, AudioClip>();
    static readonly Dictionary<SfxId, float> LastPlayed = new Dictionary<SfxId, float>();
    static AudioSource[] sources;
    static int nextSource;

    public static void Play(SfxId id, float volume = 1f)
    {
        float now = Time.unscaledTime;
        if (LastPlayed.TryGetValue(id, out float last) && now - last < MinRepeatGap) return;
        LastPlayed[id] = now;

        AudioClip clip = GetClip(id);
        if (clip == null) return; // ya avisado al cargar

        AudioSource source = NextSource();
        source.pitch = 1f + Random.Range(-PitchJitter, PitchJitter);
        source.PlayOneShot(clip, volume);
    }

    static AudioClip GetClip(SfxId id)
    {
        if (!Clips.TryGetValue(id, out AudioClip clip))
        {
            clip = LoadClip(id);
            Clips[id] = clip; // se guarda aunque sea null: no se reintenta cada vez
        }
        return clip;
    }

    // archivo real si alguien lo agregó; si no, el sintetizado
    static AudioClip LoadClip(SfxId id)
    {
        var real = Resources.Load<AudioClip>(ResourcesFolder + id.ToString().ToLowerInvariant());
        return real != null ? real : SfxSynth.Create(id);
    }

    // reparte los sonidos entre varias fuentes (cada una con su pitch)
    static AudioSource NextSource()
    {
        if (sources == null || sources[0] == null) CreateSources();
        AudioSource source = sources[nextSource];
        nextSource = (nextSource + 1) % Voices;
        return source;
    }

    static void CreateSources()
    {
        var host = new GameObject("Sfx");
        Object.DontDestroyOnLoad(host);
        sources = new AudioSource[Voices];
        for (int i = 0; i < Voices; i++)
        {
            sources[i] = host.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = 0f; // 2D: mismo volumen en toda la pantalla
        }
    }
}
