using UnityEditor;
using UnityEngine;

public static class DevTools
{
    // mismas claves que LevelManager (ver "Save & passwords" en CLAUDE.md)
    private const string MaxLevelKey = "MaxLevel";
    private const string MaxLevelIdKey = "MaxLevelId";
    private const string DatabasePath = "Assets/Levels/LevelDatabase.asset";

    [MenuItem("Dev/Clear PlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DevTools] PlayerPrefs cleared.");
    }

    // simula haber ganado el último nivel: sirve para probar el mensaje de
    // "campaña completa" y que, al agregar una tanda, se continúe en la nueva
    [MenuItem("Dev/Progress: Complete Campaign")]
    static void CompleteCampaign()
    {
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        if (db == null)
        {
            Debug.LogError($"[DevTools] No se encontró {DatabasePath}.");
            return;
        }
        PlayerPrefs.SetInt(MaxLevelKey, db.Count);
        PlayerPrefs.SetInt(MaxLevelIdKey, -1); // sin ID: índice == Count significa completa
        PlayerPrefs.Save();
        Debug.Log($"[DevTools] Progreso = campaña completa ({db.Count} niveles).");
    }

    // muestra qué nivel cree el juego que es el máximo desbloqueado
    [MenuItem("Dev/Progress: Show Saved")]
    static void ShowSaved()
    {
        Debug.Log($"[DevTools] MaxLevel (índice) = {PlayerPrefs.GetInt(MaxLevelKey, 0)}, " +
                  $"MaxLevelId (levelNumber) = {PlayerPrefs.GetInt(MaxLevelIdKey, -1)}");
    }
}
