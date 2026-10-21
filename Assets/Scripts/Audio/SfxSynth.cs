using UnityEngine;

// Efectos de sonido sintetizados por código (2026-10-20). Son PLACEHOLDERS: el
// proyecto no trae SFX y así el juego no queda mudo mientras se consiguen
// sonidos reales. Un archivo en Assets/Resources/Sfx/<nombre> los reemplaza
// (ver Sfx.LoadClip), sin tocar código.
// Todo es mono, 22 kHz y dura < 0.6 s: pesa casi nada en memoria.
public static class SfxSynth
{
    const int SampleRate = 22050;

    public static AudioClip Create(SfxId id)
    {
        switch (id)
        {
            case SfxId.Shoot:  return Build("sfx_shoot", 0.45f, Shoot);
            case SfxId.Hit:    return Build("sfx_hit", 0.15f, Hit);
            case SfxId.Death:  return Build("sfx_death", 0.55f, Death);
            case SfxId.Place:  return Build("sfx_place", 0.12f, Place);
            case SfxId.Merge:  return Build("sfx_merge", 0.45f, Merge);
            case SfxId.Click:  return Build("sfx_click", 0.05f, Click);
            default:
                Debug.LogWarning($"[SfxSynth] Sin sonido para {id}.");
                return null;
        }
    }

    // genera `seconds` de audio llamando `sample(t)` por cada muestra (t en segundos)
    static AudioClip Build(string name, float seconds, System.Func<float, float> sample)
    {
        int count = Mathf.CeilToInt(seconds * SampleRate);
        var data = new float[count];
        for (int i = 0; i < count; i++)
            data[i] = Mathf.Clamp(sample(i / (float)SampleRate), -1f, 1f);

        var clip = AudioClip.Create(name, count, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ruido blanco determinista (mismo sonido en cada partida y plataforma)
    static float Noise(float t) => Mathf.Repeat(Mathf.Sin(t * 12345.678f) * 43758.547f, 2f) - 1f;

    static float Sine(float freq, float t) => Mathf.Sin(2f * Mathf.PI * freq * t);

    // envolvente: decae exponencialmente; `speed` alto = sonido más seco
    static float Decay(float t, float speed) => Mathf.Exp(-t * speed);

    // cañonazo: golpe grave que baja de tono + soplo de ruido
    static float Shoot(float t)
    {
        float boom = Sine(Mathf.Lerp(110f, 45f, t / 0.45f), t) * Decay(t, 7f);
        float blast = Noise(t) * Decay(t, 18f) * 0.5f;
        return (boom + blast) * 0.8f;
    }

    // impacto en el pirata: golpe corto y seco
    static float Hit(float t)
    {
        float thud = Sine(Mathf.Lerp(220f, 90f, t / 0.15f), t) * Decay(t, 25f);
        return (thud + Noise(t) * Decay(t, 60f) * 0.4f) * 0.7f;
    }

    // pirata derrotado: tono que cae (tipo "uaaa")
    static float Death(float t)
    {
        float freq = Mathf.Lerp(420f, 110f, t / 0.55f);
        float tone = Sine(freq, t) + 0.3f * Sine(freq * 2f, t); // armónico: suena menos "puro"
        return tone * Decay(t, 5f) * 0.45f;
    }

    // cañón apoyado en la casilla: golpe de madera
    static float Place(float t) => (Sine(300f, t) * Decay(t, 40f) + Noise(t) * Decay(t, 120f) * 0.3f) * 0.7f;

    // fusión: arpegio ascendente do-mi-sol
    static float Merge(float t)
    {
        float freq = t < 0.12f ? 523f : t < 0.24f ? 659f : 784f;
        float local = t % 0.12f; // cada nota arranca con su propio ataque
        return Sine(freq, t) * Decay(local, 8f) * 0.45f;
    }

    // clic de botón de UI
    static float Click(float t) => Sine(900f, t) * Decay(t, 80f) * 0.5f;
}
