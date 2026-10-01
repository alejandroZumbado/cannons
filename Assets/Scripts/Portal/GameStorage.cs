using UnityEngine;
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
using CrazyGames;
#endif

// Guardado del juego en un solo lugar (2026-09-30). Hoy = PlayerPrefs.
// En CrazyGames (ver PortalSdk) usa su módulo Data: guarda en la cuenta del
// jugador y sincroniza entre dispositivos (invitados: localStorage). Misma
// API que PlayerPrefs, así que el resto del juego no se entera del cambio.
public static class GameStorage
{
    // todas las claves que el juego guarda; MigrateLocalProgress copia estas
    public const string MaxLevelKey = "MaxLevel";
    public const string MaxLevelIdKey = "MaxLevelId";
    public const string MasterVolumeKey = "MasterVolume";
    public const string TutorialMergeSeenKey = "TutMergeSeen";
    static readonly string[] IntKeys = { MaxLevelKey, MaxLevelIdKey, TutorialMergeSeenKey };
    static readonly string[] FloatKeys = { MasterVolumeKey };

    public static int GetInt(string key, int defaultValue)
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (PortalSdk.UsesPortalStorage) return CrazySDK.Data.GetInt(key, defaultValue);
#endif
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (PortalSdk.UsesPortalStorage) { CrazySDK.Data.SetInt(key, value); return; }
#endif
        PlayerPrefs.SetInt(key, value);
    }

    public static float GetFloat(string key, float defaultValue)
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (PortalSdk.UsesPortalStorage) return CrazySDK.Data.GetFloat(key, defaultValue);
#endif
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SetFloat(string key, float value)
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (PortalSdk.UsesPortalStorage) { CrazySDK.Data.SetFloat(key, value); return; }
#endif
        PlayerPrefs.SetFloat(key, value);
    }

    // escribe a disco ya: en WebGL/Android el proceso puede morir sin aviso.
    // El módulo Data del portal guarda solo, no necesita esto.
    public static void Save()
    {
        if (!PortalSdk.UsesPortalStorage) PlayerPrefs.Save();
    }

    // una sola vez por clave: si el portal no la tiene pero PlayerPrefs sí
    // (jugador que ya jugaba antes del SDK), la copia. Pedido explícito de la
    // doc de CrazyGames para no perder progreso.
    public static void MigrateLocalProgress()
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (!PortalSdk.UsesPortalStorage) return;
        foreach (var key in IntKeys)
            if (!CrazySDK.Data.HasKey(key) && PlayerPrefs.HasKey(key))
                CrazySDK.Data.SetInt(key, PlayerPrefs.GetInt(key));
        foreach (var key in FloatKeys)
            if (!CrazySDK.Data.HasKey(key) && PlayerPrefs.HasKey(key))
                CrazySDK.Data.SetFloat(key, PlayerPrefs.GetFloat(key));
#endif
    }
}
