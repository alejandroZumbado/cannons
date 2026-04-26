using UnityEngine;

public class LvlUpCannonSkin : MonoBehaviour
{
    [Header("Assign in Inspector (level 1..N)")]
    [SerializeField] private GameObject[] skins;

    // activa la skin correspondiente al damage del cañon, apaga las demas
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
