using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Levels/Level")]
public class Level : ScriptableObject
{
    public int levelNumber;
    public string password;
    public bool isHard;
    public Material skybox;
    public List<Fila> filas;

    [Serializable]
    public class Fila
    {
        public List<Cuadro> cuadros;
    }

    [Serializable]
    public class Cuadro
    {
        [Tooltip("Columna (0=derecha, 4=izquierda)")]
        [Range(0, 4)]
        public int index;

        // 0=ninguno 1-3=pirata normal 4=ultimo 5=reservado (futuro)
        [Range(0, 5)]
        public int tipo;

        [Range(1, 10)]
        public int hp;
    }
}
