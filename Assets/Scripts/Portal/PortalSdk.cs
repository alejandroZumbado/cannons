using System;
using UnityEngine;
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
using CrazyGames;
#endif

// Puente con el portal web donde corre el juego (2026-09-30: CrazyGames).
// Por qué un puente: el SDK de CrazyGames trae SiteLock, que congela el juego
// en cualquier dominio que no sea suyo (p. ej. nuestro GitHub Pages), y su
// Init tira excepción fuera de WebGL/Editor. Así que el SDK solo entra en el
// build de CrazyGames: ahí se importa el paquete y se define CRAZYGAMES_SDK
// (Player Settings > Scripting Define Symbols). Sin ese define, todo esto es
// un no-op y el juego se comporta como siempre (PC, Android, GitHub Pages).
public static class PortalSdk
{
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
    // en el portal (o Editor/localhost, donde el SDK simula el portal)
    static bool Active => CrazySDK.IsAvailable;
#endif

    // true cuando el SDK terminó de inicializar (o cuando no hay portal: nada que esperar)
    public static bool Ready
    {
        get
        {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
            return !Active || CrazySDK.IsInitialized;
#else
            return true;
#endif
        }
    }

    // inicializa el SDK y llama onReady al terminar; sin portal llama onReady al instante.
    // Se puede llamar varias veces (el SDK lo permite).
    public static void Init(Action onReady)
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (Active)
        {
            CrazySDK.Init(() =>
            {
                GameStorage.MigrateLocalProgress(); // jugadores que ya tenían progreso local no lo pierden
                onReady?.Invoke();
            });
            return;
        }
#endif
        onReady?.Invoke();
    }

    // el jugador empieza/retoma a jugar (inicio de nivel, reanudar pausa).
    // CrazyGames mide la carga hasta el primer GameplayStart.
    public static void GameplayStart()
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (Active && CrazySDK.IsInitialized) CrazySDK.Game.GameplayStart();
#endif
    }

    // pausa del juego: menú, fin de nivel, pausa
    public static void GameplayStop()
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (Active && CrazySDK.IsInitialized) CrazySDK.Game.GameplayStop();
#endif
    }

    // celebración del portal: usar poco (CrazyGames pide momentos especiales, no cada nivel)
    public static void HappyTime()
    {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
        if (Active && CrazySDK.IsInitialized) CrazySDK.Game.HappyTime();
#endif
    }

    // true si el guardado debe ir al módulo Data del portal en vez de PlayerPrefs
    public static bool UsesPortalStorage
    {
        get
        {
#if CRAZYGAMES_SDK && (UNITY_WEBGL || UNITY_EDITOR)
            return Active && CrazySDK.IsInitialized;
#else
            return false;
#endif
        }
    }
}
