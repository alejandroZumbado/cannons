using UnityEngine;

public class LvlUpCannonSkin : MonoBehaviour
{
    [Header("Assign in Inspector (level 1..N)")]
    [SerializeField] private GameObject[] skins;

    /// <summary>
    /// Activa solo la skin del nivel indicado (1..N) y apaga las demás.
    /// </summary>
    public void SetLevel(int level)
    {
        if (skins == null || skins.Length == 0)
        {
            return;
        }

        int indexToEnable = Mathf.Clamp(level - 1, 0, skins.Length - 1);

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i] == null) continue;
            skins[i].SetActive(i == indexToEnable);
        }
    }
}
