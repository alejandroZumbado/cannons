using UnityEditor;
using UnityEngine;

public static class DevTools
{
    [MenuItem("Dev/Clear PlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DevTools] PlayerPrefs cleared.");
    }
}
