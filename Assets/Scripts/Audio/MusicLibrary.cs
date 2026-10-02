using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Música cargada bajo demanda desde StreamingAssets/Music (2026-10-01).
// Por qué: la música era ~47% de la descarga inicial de la web (CrazyGames
// pide <20 MB para su home móvil). Fuera del build, cada pista se baja recién
// cuando suena (y el navegador la cachea).
// - Pistas: mp3 mono 64 kbps (~1 MB c/u), mismo formato en todas las plataformas.
// - Caché estática: sobrevive a las recargas de escena (AudioManager vive en Game).
// - Tope de pistas en memoria: en web cada pista se decodifica a PCM (~35 MB),
//   así que se descarta la menos usada al pasar el tope.
// - Si una pista falla, se avisa por consola y el juego sigue sin esa música.
public static class MusicLibrary
{
    const string Folder = "Music";
    const int MaxCached = 4; // pistas de un nivel: base, final, victoria, derrota

    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    static readonly List<string> recentUse = new List<string>(); // al final = más reciente
    static readonly Dictionary<string, Action<AudioClip>> pending = new Dictionary<string, Action<AudioClip>>();

    // Entrega la pista (al instante si ya está cargada). done recibe null si falló.
    public static void Load(string track, Action<AudioClip> done)
    {
        if (cache.TryGetValue(track, out AudioClip cached) && cached != null)
        {
            Touch(track);
            done?.Invoke(cached);
            return;
        }
        // ya se está bajando: se suma a la espera en vez de bajarla dos veces
        if (pending.ContainsKey(track)) { pending[track] += done; return; }

        pending[track] = done;
        var request = UnityWebRequestMultimedia.GetAudioClip(UrlFor(track), AudioType.MPEG);
        // en nativo mantiene el mp3 comprimido en memoria (~1 MB en vez de ~18 MB de PCM); en web no aplica
        ((DownloadHandlerAudioClip)request.downloadHandler).compressed = true;
        request.SendWebRequest().completed += _ => OnDownloaded(track, request);
    }

    // Baja pistas de antemano (sin reproducir) para que el cambio de música no tenga demora.
    public static void Preload(params string[] tracks)
    {
        foreach (string track in tracks) Load(track, null);
    }

    static void OnDownloaded(string track, UnityWebRequest request)
    {
        AudioClip clip = null;
        if (request.result == UnityWebRequest.Result.Success)
        {
            clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = track;
            Store(track, clip);
        }
        else
        {
            Debug.LogWarning($"[MusicLibrary] No se pudo cargar '{track}' ({request.url}): {request.error}");
        }
        request.Dispose();

        Action<AudioClip> callbacks = pending[track];
        pending.Remove(track);
        callbacks?.Invoke(clip);
    }

    static void Store(string track, AudioClip clip)
    {
        cache[track] = clip;
        Touch(track);
        // descarta la pista usada hace más tiempo (nunca la recién cargada)
        while (recentUse.Count > MaxCached)
        {
            string oldest = recentUse[0];
            recentUse.RemoveAt(0);
            if (cache.TryGetValue(oldest, out AudioClip old) && old != null) UnityEngine.Object.Destroy(old);
            cache.Remove(oldest);
        }
    }

    static void Touch(string track)
    {
        recentUse.Remove(track);
        recentUse.Add(track);
    }

    // Web y Android ya traen esquema (http://, jar:file://); Windows/Editor dan una ruta de disco.
    static string UrlFor(string track)
    {
        string path = $"{Application.streamingAssetsPath}/{Folder}/{track}.mp3";
        return path.Contains("://") ? path : "file://" + path;
    }
}
