using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matriz : MonoBehaviour
{
    public GameObject[,] enemysMatriz;
    public int rows;
    public int columns;
    public Vector2 ultimaModificacion;
    public GameObject matriz;


    public void SetDimensions(int x, int y)
    {
        rows = x;
        columns = y;
        enemysMatriz = new GameObject[rows, columns];
        FillMatriz();
        //PrintElements();

    }

    public void FillMatriz()
    {
        for (int i = 0; i < matriz.GetComponent<Transform>().childCount; i++)
        {
            GameObject row = matriz.GetComponent<Transform>().GetChild(i).gameObject;
            for (int y = 0; y < row.GetComponent<Transform>().childCount; y++)
            {
                enemysMatriz[i, y] = row.GetComponent<Transform>().GetChild(y).gameObject;
            }
        }
   
    }

    public void PrintElements()
    {
        int rows = enemysMatriz.GetLength(0); // Obtener el número de filas de la matriz
        int cols = enemysMatriz.GetLength(1); // Obtener el número de columnas de la matriz

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                GameObject elemento = enemysMatriz[i, j];
                if (elemento != null)
                {
                    Debug.Log("Elemento en posición [" + i + ", " + j + "]: " + elemento.name);
                }
                else
                {
                    Debug.Log("Elemento en posición [" + i + ", " + j + "]: Nulo");
                }
            }
        }
    }

    public GameObject GetObjectInCoordinate(int x, int y)
    {
        if (x >= 0 && x < rows && y >= 0 && y < columns)
        {
            ultimaModificacion = new Vector2(x, y);
            return enemysMatriz[x, y];
        }
        else
        {
            Debug.LogWarning("Coordenadas fuera de rango.");
            return null;
        }
    }

    public GameObject GetObjectInFrontOf(int x, int y)
    {
        if (x >= 0 && x < rows - 1)
        {
            ultimaModificacion = new Vector2(x, y);
            return enemysMatriz[x + 1 , y];
        }
        else
        {
            return null;
        }
    }

    public GameObject GetObjectInFrontOf(GameObject objeto)
    {
        TryGetCoordinates(objeto, out int x, out int y); // si no esta, x = -1 y devuelve null
        return GetObjectInFrontOf(x, y);
    }

    // fila (0 = donde aparecen los piratas) y columna de un barril; false (y -1, -1) si no esta en la matriz
    public bool TryGetCoordinates(GameObject objeto, out int row, out int column)
    {
        for (int i = 0; i < enemysMatriz.GetLength(0); i++)
        {
            for (int j = 0; j < enemysMatriz.GetLength(1); j++)
            {
                if (enemysMatriz[i, j] == objeto)
                {
                    row = i;
                    column = j;
                    return true;
                }
            }
        }
        row = column = -1;
        return false;
    }

    public GameObject GetFirstElementOfColumn(int column)
    {  
        if (column >= 0 && column < columns)
        {
            ultimaModificacion = new Vector2(0, column);
            return enemysMatriz[0, column];
        }
        else
        {
            Debug.LogWarning("Column index out of range.");
            return null;
        }
    }

    public GameObject GetLastElementOfColumn(int column)
    {
        if (column >= 0 && column < columns)
        {
            ultimaModificacion = new Vector2(rows - 1, column);
            return enemysMatriz[rows - 1, column];
        }
        else
        {
            Debug.LogWarning("Column index out of range.");
            return null;
        }
    }

}
