using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateSpawnManager : MonoBehaviour
{
    public GameObject pirate;
    public Matriz matriz;
    private void Start()
    {
        matriz = GetComponent<Matriz>();
    }
    public PirateManager SpawnPirate(Level.Cuadro cuadro)
    {
        GameObject barrel = matriz.GetFirstElementOfColumn(cuadro.index);
        Transform newTransform = barrel.transform.GetChild(0).transform;
        PirateManager newPirate = Instantiate(pirate).GetComponent<PirateManager>();
        newPirate.SetHp(cuadro.hp);
        newPirate.SetBarrel(barrel);
        newPirate.SetPositionPirate(newTransform);
        return newPirate;
    }
    public void SetMatriz(Matriz newmatriz) => matriz = newmatriz;
}
