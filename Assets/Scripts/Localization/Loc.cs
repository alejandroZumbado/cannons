using System;
using System.Collections.Generic;
using UnityEngine;

// Traducción inglés/español (2026-10-20). CrazyGames exige inglés; el juego
// nació en español, así que se ofrecen los dos.
// - Idioma inicial: el del sistema/navegador (español -> ES, cualquier otro -> EN).
// - El jugador lo cambia con el botón del menú (LanguageToggle) y queda guardado
//   en GameStorage (cuenta del portal o PlayerPrefs).
// - Textos por código: Loc.T(clave) / Loc.Format(clave, args).
// - Textos fijos de escena/prefab: SceneTextLocalizer los reconoce por su
//   contenido y les agrega un LocalizedText, sin tocar las escenas.
public static class Loc
{
    public enum Language { English, Spanish }

    // valor guardado: 0 = nunca eligió (usar el del sistema), 1 = EN, 2 = ES
    const int AutoChoice = 0;

    // clave -> (inglés, español). Una clave que falta se muestra tal cual y avisa en consola.
    static readonly Dictionary<string, (string en, string es)> Table = new Dictionary<string, (string, string)>
    {
        // menú
        ["menu.play"]             = ("Play", "Jugar"),
        ["menu.continue"]         = ("Continue - Level {0}", "Continuar - Nivel {0}"),
        ["menu.completed"]        = ("Completed - Level {0}", "Completado - Nivel {0}"),
        ["menu.password_hint"]    = ("Enter password...", "Escribe la contraseña..."),
        ["menu.password_wrong"]   = ("Wrong password", "Contraseña incorrecta"),
        ["menu.no_manager"]       = ("Error: start the game from the Menu scene", "Error: abre el juego desde la escena Menu"),
        // el botón muestra el idioma AL QUE cambia, escrito en ese idioma (lo entiende quien lo busca)
        ["menu.language"]         = ("Español", "English"),
        // partida
        ["game.menu"]             = ("Menu", "Menú"),
        ["game.next"]             = ("Next level", "Siguiente nivel"),
        ["game.options"]          = ("Options", "Opciones"),
        ["game.resume"]           = ("Resume", "Continuar"),
        ["game.retry"]            = ("Retry", "Reintentar"),
        ["game.volume"]           = ("Volume", "Volumen"),
        ["game.close"]            = ("Close", "Cerrar"),
        ["game.campaign_done"]    = ("You beat every level!\nMore coming soon.", "¡Completaste todos los niveles!\nPronto habrá más."),
        ["game.next_password"]    = ("Password for level {0}", "Contraseña del nivel {0}"),
        // consejos al perder (se elige uno al azar)
        ["game.tip.merge"]        = ("Tip: drop a cannon on another to merge them and add up their damage",
                                     "Consejo: suelta un cañón sobre otro para fusionarlos y sumar su daño"),
        ["game.tip.move"]         = ("Tip: placed cannons can be dragged to another column",
                                     "Consejo: los cañones colocados se pueden arrastrar a otra columna"),
        ["game.tip.block"]        = ("Tip: a cannon only hits the closest pirate in its column",
                                     "Consejo: un cañón solo le pega al pirata más cercano de su columna"),
        // selector de niveles
        ["levels.button"]         = ("Levels", "Niveles"),
        ["levels.title"]          = ("Choose a level", "Elige un nivel"),
        // tutorial
        ["tutorial.drag"]         = ("Drag the cannon from the barrel to a front slot",
                                     "Arrastra el cañón del barril a una casilla del frente"),
        ["tutorial.merge"]        = ("Strong pirate! Drop a cannon on another to merge them: their damage adds up",
                                     "¡Pirata fuerte! Suelta un cañón sobre otro para fusionarlos: su daño se suma"),
        ["tutorial.move"]         = ("Danger! Placed cannons can be dragged to another column",
                                     "¡Peligro! Los cañones colocados se pueden arrastrar a otra columna"),
    };

    // textos de escena que no son exactamente una entrada de la tabla -> su clave
    static readonly Dictionary<string, string> SceneAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Siguiente lvl"] = "game.next",
    };

    static Language? current;  // null = todavía no se leyó el guardado

    // avisa a los textos visibles para que se re-pinten al cambiar de idioma
    public static event Action Changed;

    public static Language Current
    {
        get
        {
            if (current == null) current = LoadLanguage();
            return current.Value;
        }
    }

    public static void SetLanguage(Language language)
    {
        current = language;
        GameStorage.SetInt(GameStorage.LanguageKey, language == Language.English ? 1 : 2);
        GameStorage.Save();
        Changed?.Invoke();
    }

    public static void Toggle() =>
        SetLanguage(Current == Language.English ? Language.Spanish : Language.English);

    // vuelve a leer el guardado: con portal, la elección puede llegar recién tras su init
    public static void Reload()
    {
        current = LoadLanguage();
        Changed?.Invoke();
    }

    public static bool HasKey(string key) => Table.ContainsKey(key);

    public static string T(string key)
    {
        if (!Table.TryGetValue(key, out var entry))
        {
            Debug.LogWarning($"[Loc] Falta la clave '{key}'.");
            return key;
        }
        return Current == Language.English ? entry.en : entry.es;
    }

    public static string Format(string key, params object[] args) => string.Format(T(key), args);

    // clave cuyo texto (en cualquiera de los dos idiomas) es exactamente `text`;
    // la usa SceneTextLocalizer para reconocer textos fijos de las escenas
    public static bool TryFindKey(string text, out string key)
    {
        string wanted = text.Trim();
        if (SceneAliases.TryGetValue(wanted, out key)) return true;
        foreach (var pair in Table)
        {
            // solo textos sin parámetros: "{0}" nunca aparece literal en una escena
            if (pair.Value.en.Contains("{")) continue;
            if (string.Equals(pair.Value.en, wanted, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pair.Value.es, wanted, StringComparison.OrdinalIgnoreCase))
            {
                key = pair.Key;
                return true;
            }
        }
        key = null;
        return false;
    }

    static Language LoadLanguage()
    {
        int saved = GameStorage.GetInt(GameStorage.LanguageKey, AutoChoice);
        if (saved == 1) return Language.English;
        if (saved == 2) return Language.Spanish;
        // en WebGL, systemLanguage sale del idioma del navegador
        return Application.systemLanguage == SystemLanguage.Spanish ? Language.Spanish : Language.English;
    }
}
