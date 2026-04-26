using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Levels/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    // levels[0] = nivel 1, levels[1] = nivel 2, etc.
    public Level[] levels;

    public Level Get(int index)
    {
        if (levels == null || index < 0 || index >= levels.Length) return null;
        return levels[index];
    }

    public int Count => levels != null ? levels.Length : 0;
}
